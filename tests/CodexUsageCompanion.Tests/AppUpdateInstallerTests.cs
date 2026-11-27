using System.Formats.Tar;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CodexUsageCompanion.Lifecycle;
using Xunit;

namespace CodexUsageCompanion.Tests;

public sealed class AppUpdateInstallerTests : IDisposable
{
    private const string Version = "v9.1.0";
    private const string TarName = "claude-codex-usage-companion-9.1.0-linux-x64.tar.gz";
    private const string DebName = "claude-codex-usage-companion_9.1.0_amd64.deb";
    private readonly string _root = Path.Combine(Path.GetTempPath(), "update-installer-" + Guid.NewGuid().ToString("N"));
    private readonly ECDsa _key = ECDsa.Create(ECCurve.NamedCurves.nistP256);

    public AppUpdateInstallerTests() => Directory.CreateDirectory(_root);

    public void Dispose()
    {
        _key.Dispose();
        Directory.Delete(_root, recursive: true);
    }

    private string PublicKeyPem => _key.ExportSubjectPublicKeyInfoPem();

    private static AppUpdateResult Release(params string[] assets) =>
        new(Version, true, new Uri(AppUpdateChecker.RepositoryUrl + "/releases/tag/" + Version))
        {
            AssetNames = assets.ToHashSet()
        };

    private static AppUpdateResult FullRelease(string package) =>
        Release(package, AppUpdateInstaller.ChecksumsName, AppUpdateInstaller.SignatureName);

    private byte[] Sign(byte[] data) =>
        _key.SignData(data, HashAlgorithmName.SHA256, DSASignatureFormat.Rfc3279DerSequence);

    private static byte[] PortableArchive(byte[] binary)
    {
        using var buffer = new MemoryStream();
        using (var gzip = new GZipStream(buffer, CompressionLevel.Fastest, leaveOpen: true))
        using (var tar = new TarWriter(gzip))
        {
            tar.WriteEntry(new PaxTarEntry(TarEntryType.RegularFile, "claude-codex-usage-companion-9.1.0-linux-x64/README.md")
            {
                DataStream = new MemoryStream("readme"u8.ToArray())
            });
            tar.WriteEntry(new PaxTarEntry(TarEntryType.RegularFile,
                "claude-codex-usage-companion-9.1.0-linux-x64/claude-codex-usage-companion")
            {
                DataStream = new MemoryStream(binary)
            });
        }
        return buffer.ToArray();
    }

    private Dictionary<string, byte[]> SignedFiles(string package, byte[] packageBytes, byte[]? listedBytes = null)
    {
        var hash = Convert.ToHexString(SHA256.HashData(listedBytes ?? packageBytes)).ToLowerInvariant();
        var sums = Encoding.UTF8.GetBytes($"{new string('0', 64)}  other.zip\n{hash}  {package}\n");
        return new()
        {
            [package] = packageBytes,
            [AppUpdateInstaller.ChecksumsName] = sums,
            [AppUpdateInstaller.SignatureName] = Sign(sums)
        };
    }

    private AppUpdateInstaller Installer(
        InstallKind kind,
        Dictionary<string, byte[]> files,
        string? target = null,
        Func<string, IReadOnlyList<string>, CancellationToken, Task<int>>? runElevated = null) =>
        new(kind, target ?? Path.Combine(_root, "app", "claude-codex-usage-companion"),
            new HttpClient(new FileHandler(files)), Path.Combine(_root, "cache"), PublicKeyPem,
            Architecture.X64, runElevated);

    [Fact]
    public async Task PortableUpdateReplacesTheBinary()
    {
        var target = Path.Combine(_root, "app", "claude-codex-usage-companion");
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        await File.WriteAllTextAsync(target, "old");
        var installer = Installer(InstallKind.Portable, SignedFiles(TarName, PortableArchive("new"u8.ToArray())), target);

        Assert.True(installer.CanInstall(FullRelease(TarName)));
        var prepared = await installer.DownloadAsync(FullRelease(TarName));
        await installer.InstallAsync(prepared);

        Assert.Equal("new", await File.ReadAllTextAsync(target));
        if (!OperatingSystem.IsWindows())
            Assert.True(File.GetUnixFileMode(target).HasFlag(UnixFileMode.UserExecute));
        Assert.Single(Directory.GetFiles(Path.GetDirectoryName(target)!));
    }

    [Fact]
    public async Task RejectsAnInvalidSignature()
    {
        var files = SignedFiles(TarName, PortableArchive("new"u8.ToArray()));
        files[AppUpdateInstaller.SignatureName] = Sign("something else"u8.ToArray());
        var installer = Installer(InstallKind.Portable, files);

        var error = await Assert.ThrowsAsync<UpdateInstallException>(() => installer.DownloadAsync(FullRelease(TarName)));
        Assert.Contains("signature", error.Message);
    }

    [Fact]
    public async Task RejectsASignatureFromAnotherKey()
    {
        var files = SignedFiles(TarName, PortableArchive("new"u8.ToArray()));
        using var other = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        files[AppUpdateInstaller.SignatureName] = other.SignData(files[AppUpdateInstaller.ChecksumsName],
            HashAlgorithmName.SHA256, DSASignatureFormat.Rfc3279DerSequence);

        await Assert.ThrowsAsync<UpdateInstallException>(() =>
            Installer(InstallKind.Portable, files).DownloadAsync(FullRelease(TarName)));
    }

    [Fact]
    public async Task RejectsAPackageThatDoesNotMatchItsChecksum()
    {
        var files = SignedFiles(TarName, PortableArchive("tampered"u8.ToArray()), listedBytes: "original"u8.ToArray());

        var error = await Assert.ThrowsAsync<UpdateInstallException>(() =>
            Installer(InstallKind.Portable, files).DownloadAsync(FullRelease(TarName)));
        Assert.Contains("checksum", error.Message);
        Assert.Empty(Directory.GetFiles(Path.Combine(_root, "cache"), "*", SearchOption.AllDirectories));
    }

    [Fact]
    public async Task DebianUpdateRunsAptThroughPkexec()
    {
        string? program = null;
        IReadOnlyList<string>? arguments = null;
        var installer = Installer(InstallKind.Debian, SignedFiles(DebName, "deb"u8.ToArray()),
            runElevated: (file, args, _) =>
            {
                program = file;
                arguments = args;
                return Task.FromResult(0);
            });

        var prepared = await installer.DownloadAsync(FullRelease(DebName));
        await installer.InstallAsync(prepared);

        Assert.Equal("pkexec", program);
        Assert.Equal(["/usr/bin/apt-get", "install", "-y", prepared.PackagePath], arguments);
        Assert.EndsWith(DebName, prepared.PackagePath);
    }

    [Theory]
    [InlineData(126, true)]
    [InlineData(127, true)]
    [InlineData(100, false)]
    public async Task ReportsDebianInstallFailures(int exitCode, bool cancelled)
    {
        var installer = Installer(InstallKind.Debian, SignedFiles(DebName, "deb"u8.ToArray()),
            runElevated: (_, _, _) => Task.FromResult(exitCode));
        var prepared = await installer.DownloadAsync(FullRelease(DebName));

        var error = await Assert.ThrowsAsync<UpdateInstallException>(() => installer.InstallAsync(prepared));
        Assert.Equal(cancelled, error.Cancelled);
    }

    [Fact]
    public void RequiresThePackageAndSignedChecksums()
    {
        var installer = Installer(InstallKind.Portable, []);
        Assert.False(installer.CanInstall(Release(TarName, AppUpdateInstaller.ChecksumsName)));
        Assert.False(installer.CanInstall(Release(DebName, AppUpdateInstaller.ChecksumsName, AppUpdateInstaller.SignatureName)));
        Assert.False(installer.CanInstall(FullRelease(TarName) with { UpdateAvailable = false }));
        Assert.True(installer.CanInstall(FullRelease(TarName)));
    }

    [Theory]
    [InlineData(InstallKind.Snap)]
    [InlineData(InstallKind.CodexPlugin)]
    [InlineData(InstallKind.Unsupported)]
    public void OtherInstallationsCannotInstall(InstallKind kind)
    {
        var installer = Installer(kind, []);
        Assert.Null(installer.PackageName(Version));
        Assert.False(installer.CanInstall(FullRelease(TarName)));
    }

    [Fact]
    public void NamesPackagesForTheArchitecture()
    {
        var arm = new AppUpdateInstaller(InstallKind.Debian, "/x", architecture: Architecture.Arm64);
        Assert.Equal("claude-codex-usage-companion_9.1.0_arm64.deb", arm.PackageName(Version));
        var portable = new AppUpdateInstaller(InstallKind.Portable, "/x", architecture: Architecture.Arm64);
        Assert.Equal("claude-codex-usage-companion-9.1.0-linux-arm64.tar.gz", portable.PackageName(Version));
        Assert.Null(new AppUpdateInstaller(InstallKind.Portable, "/x", architecture: Architecture.X86).PackageName(Version));
    }

    [Fact]
    public void FindsChecksumsInBothSha256sumFormats()
    {
        var hash = new string('a', 64);
        Assert.Equal(hash, AppUpdateInstaller.FindChecksum($"{hash}  a.deb\n", "a.deb"));
        Assert.Equal(hash, AppUpdateInstaller.FindChecksum($"{hash} *a.deb\r\n", "a.deb"));
        Assert.Null(AppUpdateInstaller.FindChecksum($"{hash}  a.deb.extra\n", "a.deb"));
        Assert.Null(AppUpdateInstaller.FindChecksum("short  a.deb\n", "a.deb"));
    }

    [Fact]
    public void BuildsDownloadUrlsFromTheRepository()
    {
        Assert.Equal(AppUpdateChecker.RepositoryUrl + "/releases/download/v1.0.0/SHA256SUMS",
            AppUpdateInstaller.DownloadUrl("v1.0.0", "SHA256SUMS").AbsoluteUri);
        Assert.StartsWith(AppUpdateChecker.RepositoryUrl + "/releases/download/",
            AppUpdateInstaller.DownloadUrl("../../evil", "x").AbsoluteUri);
    }

    [Fact]
    public void ParsesReleaseAssetNames()
    {
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(new
        {
            tag_name = Version, draft = false, prerelease = false,
            assets = new[] { new { name = TarName }, new { name = "SHA256SUMS" } }
        }));
        var result = AppUpdateChecker.ParseRelease(json.RootElement, "0.2.0");
        Assert.Contains(TarName, result.AssetNames);
        Assert.Contains("SHA256SUMS", result.AssetNames);
    }

    [Fact]
    public void EmbedsTheReleasePublicKey()
    {
        var pem = UpdateSignature.EmbeddedPublicKeyPem;
        Assert.Contains("BEGIN PUBLIC KEY", pem);
        using var key = ECDsa.Create();
        key.ImportFromPem(pem);
        Assert.Equal(256, key.KeySize);
        Assert.False(UpdateSignature.Verify("data"u8.ToArray(), new byte[70]));
    }

    private sealed class FileHandler(Dictionary<string, byte[]> files) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var prefix = AppUpdateChecker.RepositoryUrl + "/releases/download/" + Version + "/";
            var url = request.RequestUri!.AbsoluteUri;
            if (url.StartsWith(prefix, StringComparison.Ordinal) && files.TryGetValue(url[prefix.Length..], out var body))
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(body) });
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }
    }
}

public sealed class InstallKindDetectorTests
{
    private static InstallKind Detect(string? path, string? snap = null, string? pluginRoot = null,
        bool dpkgList = false, bool writable = true, bool hasDll = false) =>
        InstallKindDetector.Detect(path, snap, pluginRoot, null,
            file => file == InstallKindDetector.DebianPackageList ? dpkgList : hasDll && file.EndsWith(".dll"),
            _ => writable);

    [Fact]
    public void DetectsEachInstallation()
    {
        Assert.Equal(InstallKind.Debian, Detect(InstallKindDetector.DebianBinaryPath, dpkgList: true, writable: false));
        Assert.Equal(InstallKind.Unsupported, Detect(InstallKindDetector.DebianBinaryPath, dpkgList: false));
        Assert.Equal(InstallKind.Snap, Detect("/snap/claude-codex-usage-companion/12/bin/CodexUsageCompanion"));
        Assert.Equal(InstallKind.Snap, Detect("/somewhere/CodexUsageCompanion", snap: "/snap/x/1"));
        Assert.Equal(InstallKind.CodexPlugin, Detect("/home/u/.codex/p/bin/linux-x64/CodexUsageCompanion"));
        Assert.Equal(InstallKind.CodexPlugin, Detect("/home/u/bin/app", pluginRoot: "/home/u/.codex/p"));
        Assert.Equal(InstallKind.Portable, Detect("/home/u/.local/bin/claude-codex-usage-companion"));
        Assert.Equal(InstallKind.Unsupported, Detect("/opt/tools/claude-codex-usage-companion", writable: false));
        Assert.Equal(InstallKind.Unsupported, Detect("/home/u/src/bin/Debug/net10.0/CodexUsageCompanion", hasDll: true));
        Assert.Equal(InstallKind.Unsupported, Detect(null));
    }
}
