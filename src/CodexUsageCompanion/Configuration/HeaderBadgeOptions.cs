namespace CodexUsageCompanion.Configuration;

/// <summary>What the header badge of the Latest panel design shows.</summary>
public static class HeaderBadgeOptions
{
    /// <summary>The lowest remaining percentage among the shown limits.</summary>
    public const string Lowest = "lowest";
    /// <summary>The average remaining percentage of the shown limits.</summary>
    public const string Average = "average";
    /// <summary>The lowest remaining percentage among Claude's shown limits.</summary>
    public const string Claude = "claude";
    /// <summary>The remaining percentage of Claude's current session limit.</summary>
    public const string ClaudeSession = "claude-session";
    /// <summary>The remaining percentage of Claude's weekly limit.</summary>
    public const string ClaudeWeekly = "claude-weekly";
    /// <summary>The average remaining percentage of Claude's shown limits.</summary>
    public const string ClaudeAverage = "claude-average";
    /// <summary>The lowest remaining percentage among Codex's shown limits.</summary>
    public const string Codex = "codex";
    /// <summary>The remaining percentage of Codex's 5-hour limit.</summary>
    public const string CodexFiveHour = "codex-five-hour";
    /// <summary>The remaining percentage of Codex's weekly limit.</summary>
    public const string CodexWeekly = "codex-weekly";
    /// <summary>The average remaining percentage of Codex's shown limits.</summary>
    public const string CodexAverage = "codex-average";
    public const string Hidden = "hidden";

    public static IReadOnlyList<string> Values { get; } =
        Array.AsReadOnly([
            Lowest, Average,
            Claude, ClaudeSession, ClaudeWeekly, ClaudeAverage,
            Codex, CodexFiveHour, CodexWeekly, CodexAverage,
            Hidden]);

    public static string Normalize(string? value) =>
        Values.FirstOrDefault(option => string.Equals(option, value, StringComparison.OrdinalIgnoreCase)) ?? Lowest;
}
