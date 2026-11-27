using System.ComponentModel;
using System.Diagnostics;
using System.Formats.Tar;
using System.IO.Compression;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using CodexUsageCompanion.Platform;

namespace CodexUsageCompanion.Lifecycle;

public sealed record PreparedUpdate(string Version, InstallKind Kind, string PackagePath);

public sealed class UpdateInstallException(string message, bool cancelled = false, Exception? inner = null)
    : Exception(message, inner)
{
    /// <summary>The user dismissed the administrator password prompt.</summary>
    public bool Cancelled { get; } = cancelled;
}

/// <summary>
/// Downloads a release package, checks it against the signed SHA256SUMS, and installs it.
/// A package is never installed unless the SHA256SUMS signature matches the embedded public key
/// and the package hash matches its signed entry.
/// </summary>
public sealed class AppUpdateInstaller
{
    public const string ChecksumsName = "SHA256SUMS";
    public const string SignatureName = "SHA256SUMS.sig";
    private const long MaxPackageBytes = 512L * 1024 * 1024;
    private const long MaxMetadataBytes = 64 * 1024;

    private static readonly HttpClient DefaultClient = new() { Timeout = TimeSpan.FromMinutes(10) };
    private readonly HttpClient _client;
    private readonly string _cacheDirectory;
    private readonly string? _publicKeyPem;
    private readonly Architecture _architecture;
    private readonly Func<string, IReadOnlyList<string>, CancellationToken, Task<int>> _runElevated;

    public AppUpdateInstaller(
        InstallKind? kind = null,
        string? targetPath = null,
        HttpClient? client = null,
        string? cacheDirectory = null,
        string? publicKeyPem = null,
        Architecture? architecture = null,
        Func<string, IReadOnlyList<string>, CancellationToken, Task<int>>? runElevated = null)
    {
        Kind = kind ?? InstallKindDetector.Detect();
        TargetPath = targetPath ?? Environment.ProcessPath;
        _client = client ?? DefaultClient;
        _cacheDirectory = cacheDirectory ?? Path.Combine(LinuxPaths.CacheDirectory, "updates");
        _publicKeyPem = publicKeyPem;
        _architecture = architecture ?? RuntimeInformation.ProcessArchitecture;
        _runElevated = runElevated ?? RunProcessAsync;
    }

    public InstallKind Kind { get; }
    public string? TargetPath { get; }

    /// <summary>The release asset this installation needs, or null when it cannot update itself.</summary>
    public string? PackageName(string version)
    {
        var plain = version.Trim().TrimStart('v', 'V');
        var rid = InstallKindDetector.RuntimeId(_architecture);
        if (rid is null) return null;
        return Kind switch
        {
            InstallKind.Portable => $"claude-codex-usage-companion-{plain}-{rid}.tar.gz",
            InstallKind.Debian => $"claude-codex-usage-companion_{plain}_{(rid == "linux-x64" ? "amd64" : "arm64")}.deb",
            _ => null
        };
    }

    /// <summary>True when the release has a signed package that this installation can install.</summary>
    public bool CanInstall(AppUpdateResult result) =>
        result.UpdateAvailable &&
        InstallKindDetector.SupportsInstall(Kind) &&
        !string.IsNullOrWhiteSpace(TargetPath) &&
        PackageName(result.Version) is { } name &&
        result.AssetNames.Contains(name) &&
        result.AssetNames.Contains(ChecksumsName) &&
        result.AssetNames.Contains(SignatureName);

    /// <summary>Downloads and verifies the package. Reuses an already verified download of the same version.</summary>
    public async Task<PreparedUpdate> DownloadAsync(AppUpdateResult result, CancellationToken cancellationToken = default)
    {
        if (!CanInstall(result))
            throw new UpdateInstallException("This installation cannot install the update automatically.");
        var name = PackageName(result.Version)!;

        var checksums = await DownloadBytesAsync(result.Version, ChecksumsName, MaxMetadataBytes, cancellationToken);
        var signature = await DownloadBytesAsync(result.Version, SignatureName, MaxMetadataBytes, cancellationToken);
        if (!UpdateSignature.Verify(checksums, signature, _publicKeyPem))
            throw new UpdateInstallException("The update signature is not valid.");
        var expectedHash = FindChecksum(Encoding.UTF8.GetString(checksums), name)
            ?? throw new UpdateInstallException("The signed checksums do not list the update package.");

        var versionDirectory = Path.Combine(_cacheDirectory, SafeSegment(result.Version));
        var packagePath = Path.Combine(versionDirectory, name);
        if (File.Exists(packagePath) && await HashFileAsync(packagePath, cancellationToken) == expectedHash)
            return new(result.Version, Kind, packagePath);

        // Keep only one downloaded version.
        if (Directory.Exists(_cacheDirectory)) Directory.Delete(_cacheDirectory, recursive: true);
        Directory.CreateDirectory(versionDirectory);
        var partialPath = packagePath + ".part";
        await using (var output = new FileStream(partialPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            using var response = await _client.GetAsync(DownloadUrl(result.Version, name),
                HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();
            if (response.Content.Headers.ContentLength > MaxPackageBytes)
                throw new UpdateInstallException("The update package is too large.");
            await using var input = await response.Content.ReadAsStreamAsync(cancellationToken);
            await CopyWithLimitAsync(input, output, MaxPackageBytes, cancellationToken);
        }

        if (await HashFileAsync(partialPath, cancellationToken) != expectedHash)
        {
            File.Delete(partialPath);
            throw new UpdateInstallException("The downloaded update does not match its signed checksum.");
        }
        File.Move(partialPath, packagePath, overwrite: true);
        return new(result.Version, Kind, packagePath);
    }

    /// <summary>
    /// Installs a verified package. A portable binary is replaced in place. A .deb is installed with
    /// <c>pkexec apt-get</c>, which shows the desktop's administrator password prompt.
    /// </summary>
    public async Task InstallAsync(PreparedUpdate update, CancellationToken cancellationToken = default)
    {
        switch (update.Kind)
        {
            case InstallKind.Portable:
                await ReplacePortableBinaryAsync(update.PackagePath, cancellationToken);
                break;
            case InstallKind.Debian:
                int exitCode;
                try
                {
                    exitCode = await _runElevated("pkexec",
                        ["/usr/bin/apt-get", "install", "-y", Path.GetFullPath(update.PackagePath)], cancellationToken);
                }
                catch (Win32Exception exception)
                {
                    throw new UpdateInstallException("pkexec is not available.", inner: exception);
                }
                // pkexec returns 126 when the user dismisses the prompt and 127 when authorization fails.
                if (exitCode is 126 or 127)
                    throw new UpdateInstallException("The administrator password prompt was dismissed.", cancelled: true);
                if (exitCode != 0)
                    throw new UpdateInstallException($"apt-get exited with code {exitCode}.");
                break;
            default:
                throw new UpdateInstallException("This installation cannot install updates.");
        }
    }

    internal static string? FindChecksum(string checksums, string fileName)
    {
        foreach (var rawLine in checksums.Split('\n'))
        {
            var line = rawLine.TrimEnd('\r');
            var separator = line.IndexOf(' ');
            if (separator != 64) continue;
            var name = line[(separator + 1)..].TrimStart(' ', '*');
            if (name == fileName) return line[..64].ToLowerInvariant();
        }
        return null;
    }

    private async Task ReplacePortableBinaryAsync(string archivePath, CancellationToken cancellationToken)
    {
        var target = TargetPath ?? throw new UpdateInstallException("The app location is unknown.");
        var directory = Path.GetDirectoryName(target)!;
        var staging = Path.Combine(directory, $".{Path.GetFileName(target)}.update-{Environment.ProcessId}");
        var found = false;
        try
        {
            await using (var file = File.OpenRead(archivePath))
            await using (var gzip = new GZipStream(file, CompressionMode.Decompress))
            await using (var tar = new TarReader(gzip))
            {
                while (await tar.GetNextEntryAsync(cancellationToken: cancellationToken) is { } entry)
                {
                    // The archive holds one top-level folder with the binary directly inside it.
                    var parts = entry.Name.TrimStart('.', '/').Split('/');
                    if (entry.EntryType is not (TarEntryType.RegularFile or TarEntryType.V7RegularFile) ||
                        parts.Length != 2 || parts[1] != "claude-codex-usage-companion" || entry.DataStream is null)
                        continue;
                    await using (var output = new FileStream(staging, FileMode.Create, FileAccess.Write, FileShare.None))
                        await CopyWithLimitAsync(entry.DataStream, output, MaxPackageBytes, cancellationToken);
                    found = true;
                    break;
                }
            }
            if (!found) throw new UpdateInstallException("The update archive does not contain the app.");

            if (!OperatingSystem.IsWindows()) File.SetUnixFileMode(staging,
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                UnixFileMode.GroupRead | UnixFileMode.GroupExecute |
                UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
            // rename(2) swaps the file in one step. The running process keeps the old file open.
            File.Move(staging, target, overwrite: true);
        }
        finally
        {
            if (File.Exists(staging)) File.Delete(staging);
        }
    }

    private async Task<byte[]> DownloadBytesAsync(string version, string name, long limit, CancellationToken cancellationToken)
    {
        using var response = await _client.GetAsync(DownloadUrl(version, name),
            HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var input = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var output = new MemoryStream();
        await CopyWithLimitAsync(input, output, limit, cancellationToken);
        return output.ToArray();
    }

    // Build the URL from the trusted repository, never from a server-provided destination.
    internal static Uri DownloadUrl(string version, string name) => new(
        AppUpdateChecker.RepositoryUrl + "/releases/download/" +
        Uri.EscapeDataString(version) + "/" + Uri.EscapeDataString(name));

    private static async Task CopyWithLimitAsync(Stream input, Stream output, long limit, CancellationToken cancellationToken)
    {
        var buffer = new byte[81920];
        long total = 0;
        int read;
        while ((read = await input.ReadAsync(buffer, cancellationToken)) > 0)
        {
            total += read;
            if (total > limit) throw new UpdateInstallException("The update download is too large.");
            await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
        }
    }

    private static async Task<string> HashFileAsync(string path, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        return Convert.ToHexString(await SHA256.HashDataAsync(stream, cancellationToken)).ToLowerInvariant();
    }

    private static string SafeSegment(string value) =>
        string.Concat(value.Where(c => char.IsAsciiLetterOrDigit(c) || c is '.' or '-' or '_'));

    private static async Task<int> RunProcessAsync(string fileName, IReadOnlyList<string> arguments, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo { FileName = fileName, UseShellExecute = false, CreateNoWindow = true };
        foreach (var argument in arguments) startInfo.ArgumentList.Add(argument);
        using var process = Process.Start(startInfo) ?? throw new UpdateInstallException($"{fileName} did not start.");
        await process.WaitForExitAsync(cancellationToken);
        return process.ExitCode;
    }
}
