namespace CodexUsageCompanion.Platform;

public static class LinuxPaths
{
    public static string ConfigDirectory
    {
        get
        {
            var xdg = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
            return string.IsNullOrWhiteSpace(xdg)
                ? Path.Combine(UserHome, ".config", "claude-codex-usage-companion")
                : Path.Combine(xdg, "claude-codex-usage-companion");
        }
    }

    // Config directories used before each rename, most recent first, kept for one-time settings migration.
    public static IReadOnlyList<string> LegacyConfigDirectories
    {
        get
        {
            var xdg = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
            var configHome = string.IsNullOrWhiteSpace(xdg) ? Path.Combine(UserHome, ".config") : xdg;
            return
            [
                Path.Combine(configHome, "codex-claude-usage-companion"),
                Path.Combine(configHome, "codex-usage-companion")
            ];
        }
    }

    public static string StateDirectory
    {
        get
        {
            var xdg = Environment.GetEnvironmentVariable("XDG_STATE_HOME");
            return string.IsNullOrWhiteSpace(xdg)
                ? Path.Combine(UserHome, ".local", "state", "claude-codex-usage-companion")
                : Path.Combine(xdg, "claude-codex-usage-companion");
        }
    }

    public static string AutostartDirectory
    {
        get
        {
            var xdg = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
            var configHome = string.IsNullOrWhiteSpace(xdg)
                ? Path.Combine(UserHome, ".config")
                : xdg;
            return Path.Combine(configHome, "autostart");
        }
    }

    public static string RuntimeDirectory
    {
        get
        {
            var xdg = Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR");
            if (!string.IsNullOrWhiteSpace(xdg) && Path.IsPathFullyQualified(xdg))
            {
                return xdg;
            }

            return Path.GetTempPath();
        }
    }

    public static string UserHome =>
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    public static string InstanceSocketPath
    {
        get
        {
            var suffix = string.IsNullOrWhiteSpace(Environment.UserName)
                ? Environment.ProcessId.ToString(System.Globalization.CultureInfo.InvariantCulture)
                : Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(Environment.UserName)))[..12].ToLowerInvariant();
            return Path.Combine(RuntimeDirectory, $"claude-codex-usage-companion-{suffix}.sock");
        }
    }
}
