using System.Runtime.InteropServices;

namespace CodexUsageCompanion.Lifecycle;

public enum InstallKind
{
    /// <summary>A portable binary in a folder the user can write to. Updates install silently.</summary>
    Portable,
    /// <summary>The .deb package. Updates install with an administrator password prompt.</summary>
    Debian,
    /// <summary>A snap. snapd owns the files, so the app cannot replace them.</summary>
    Snap,
    /// <summary>The Codex plugin. Codex manages the plugin files.</summary>
    CodexPlugin,
    /// <summary>Any other location, such as a read-only folder or a development build.</summary>
    Unsupported
}

public static class InstallKindDetector
{
    public const string DebianBinaryPath = "/usr/lib/claude-codex-usage-companion/CodexUsageCompanion";
    public const string DebianPackageList = "/var/lib/dpkg/info/claude-codex-usage-companion.list";

    public static InstallKind Detect() => Detect(
        Environment.ProcessPath,
        Environment.GetEnvironmentVariable("SNAP"),
        Environment.GetEnvironmentVariable("PLUGIN_ROOT"),
        Environment.GetEnvironmentVariable("PLUGIN_DATA"),
        File.Exists,
        IsDirectoryWritable);

    internal static InstallKind Detect(
        string? processPath,
        string? snapRoot,
        string? pluginRoot,
        string? pluginData,
        Func<string, bool> fileExists,
        Func<string, bool> directoryWritable)
    {
        if (string.IsNullOrWhiteSpace(processPath)) return InstallKind.Unsupported;
        if (!string.IsNullOrWhiteSpace(snapRoot) || processPath.StartsWith("/snap/", StringComparison.Ordinal))
            return InstallKind.Snap;
        if (!string.IsNullOrWhiteSpace(pluginRoot) || !string.IsNullOrWhiteSpace(pluginData) ||
            processPath.EndsWith("/bin/linux-x64/CodexUsageCompanion", StringComparison.Ordinal) ||
            processPath.EndsWith("/bin/linux-arm64/CodexUsageCompanion", StringComparison.Ordinal))
            return InstallKind.CodexPlugin;
        if (processPath == DebianBinaryPath)
            return fileExists(DebianPackageList) ? InstallKind.Debian : InstallKind.Unsupported;
        // A `dotnet run` build ships a managed DLL next to the apphost; only a single-file publish can be replaced.
        var directory = Path.GetDirectoryName(processPath);
        if (string.IsNullOrEmpty(directory) || fileExists(Path.Combine(directory, "CodexUsageCompanion.dll")))
            return InstallKind.Unsupported;
        return directoryWritable(directory) ? InstallKind.Portable : InstallKind.Unsupported;
    }

    public static bool SupportsInstall(InstallKind kind) => kind is InstallKind.Portable or InstallKind.Debian;

    /// <summary>The runtime identifier used in release asset names, or null for an architecture without releases.</summary>
    public static string? RuntimeId(Architecture architecture) => architecture switch
    {
        Architecture.X64 => "linux-x64",
        Architecture.Arm64 => "linux-arm64",
        _ => null
    };

    private static bool IsDirectoryWritable(string directory)
    {
        try
        {
            var probe = Path.Combine(directory, $".update-probe-{Environment.ProcessId}");
            using (File.Create(probe, 1, FileOptions.DeleteOnClose)) { }
            return true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }
}
