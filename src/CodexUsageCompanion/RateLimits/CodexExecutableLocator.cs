using System.IO;

namespace CodexUsageCompanion.RateLimits;

public static class CodexExecutableLocator
{
    public static string? Find()
    {
        return Find(
            Environment.GetEnvironmentVariable("CODEX_CLI_PATH"),
            Environment.GetEnvironmentVariable("PATH"),
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            File.Exists);
    }

    public static string? Find(
        string? explicitPath,
        string? pathVariable,
        string localApplicationData,
        string userProfile,
        Func<string, bool> fileExists)
    {
        var executableName = OperatingSystem.IsWindows() ? "codex.exe" : "codex";
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

        if (OperatingSystem.IsWindows() && !string.IsNullOrWhiteSpace(localApplicationData))
        {
            candidates.Add(Path.Combine(localApplicationData, "Programs", "OpenAI", "Codex", "bin", "codex.exe"));
            candidates.Add(Path.Combine(localApplicationData, "OpenAI", "Codex", "bin", "codex.exe"));
        }

        if (!string.IsNullOrWhiteSpace(userProfile))
        {
            candidates.Add(Path.Combine(userProfile, ".local", "bin", executableName));
            candidates.Add(Path.Combine(userProfile, ".npm-global", "bin", executableName));
            candidates.Add(Path.Combine(userProfile, ".codex", "bin", executableName));
        }

        return candidates
            .Distinct(StringComparer.Ordinal)
            .FirstOrDefault(fileExists);
    }
}
