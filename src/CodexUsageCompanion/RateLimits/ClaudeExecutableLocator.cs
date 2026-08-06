using System.IO;

namespace CodexUsageCompanion.RateLimits;

public static class ClaudeExecutableLocator
{
    public static string? Find()
    {
        return Find(
            Environment.GetEnvironmentVariable("CLAUDE_CLI_PATH"),
            Environment.GetEnvironmentVariable("PATH"),
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            File.Exists);
    }

    public static string? Find(
        string? explicitPath,
        string? pathVariable,
        string userProfile,
        Func<string, bool> fileExists)
    {
        var executableName = OperatingSystem.IsWindows() ? "claude.exe" : "claude";
        var candidates = new List<string>();
        if (!string.IsNullOrWhiteSpace(explicitPath))
        {
            candidates.Add(explicitPath.Trim().Trim('"'));
        }

        if (!string.IsNullOrWhiteSpace(pathVariable))
        {
            candidates.AddRange(pathVariable
                .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(directory => Path.Combine(directory.Trim('"'), executableName)));
        }

        if (!string.IsNullOrWhiteSpace(userProfile))
        {
            candidates.Add(Path.Combine(userProfile, ".local", "bin", executableName));
            candidates.Add(Path.Combine(userProfile, ".npm-global", "bin", executableName));
            candidates.Add(Path.Combine(userProfile, ".claude", "local", executableName));
        }

        return candidates
            .Distinct(StringComparer.Ordinal)
            .FirstOrDefault(fileExists);
    }
}
