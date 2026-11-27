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
    /// <summary>The lowest remaining percentage among Codex's shown limits.</summary>
    public const string Codex = "codex";
    public const string Hidden = "hidden";

    public static IReadOnlyList<string> Values { get; } =
        Array.AsReadOnly([Lowest, Average, Claude, Codex, Hidden]);

    public static string Normalize(string? value) =>
        Values.FirstOrDefault(option => string.Equals(option, value, StringComparison.OrdinalIgnoreCase)) ?? Lowest;
}
