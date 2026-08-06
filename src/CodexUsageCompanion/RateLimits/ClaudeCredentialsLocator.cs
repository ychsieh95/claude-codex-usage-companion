using System.IO;

namespace CodexUsageCompanion.RateLimits;

public static class ClaudeCredentialsLocator
{
    public static string? Find()
    {
        return Find(
            Environment.GetEnvironmentVariable("CLAUDE_CREDENTIALS_PATH"),
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            File.Exists);
    }

    public static string? Find(
        string? explicitPath,
        string userProfile,
        Func<string, bool> fileExists)
    {
        var candidate = !string.IsNullOrWhiteSpace(explicitPath)
            ? explicitPath.Trim().Trim('"')
            : string.IsNullOrWhiteSpace(userProfile)
                ? null
                : Path.Combine(userProfile, ".claude", ".credentials.json");

        return candidate is not null && fileExists(candidate) ? candidate : null;
    }
}
