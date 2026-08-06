namespace CodexUsageCompanion.Configuration;

public static class TrayIconStyleOptions
{
    public const string Original = "original";
    public const string ClaudeCurrentSession = "claude-current-session";
    public const string ClaudeWeeklySession = "claude-weekly-session";
    public const string CodexSession = "codex-session";

    public static readonly IReadOnlyList<string> Values =
    [
        Original,
        ClaudeCurrentSession,
        ClaudeWeeklySession,
        CodexSession
    ];

    public static string Normalize(string? value)
    {
        return Values.FirstOrDefault(option =>
            string.Equals(option, value, StringComparison.OrdinalIgnoreCase)) ??
            Original;
    }
}
