namespace CodexUsageCompanion.Configuration;

/// <summary>How the header badge's average modes combine several limits.</summary>
public static class BadgeAverageOptions
{
    /// <summary>Every limit counts the same.</summary>
    public const string Plain = "plain";
    /// <summary>A limit counts by the time left until it resets, so one about to refill counts less.</summary>
    public const string TimeToReset = "time-to-reset";
    /// <summary>A limit counts by the length of its window, so weekly limits count more.</summary>
    public const string WindowLength = "window-length";
    /// <summary>
    /// The average gap from an even pace: remaining percent minus the share of the window
    /// still to come. Positive means usage is slower than an even pace.
    /// </summary>
    public const string Pace = "pace";

    public static IReadOnlyList<string> Values { get; } =
        Array.AsReadOnly([Plain, TimeToReset, WindowLength, Pace]);

    public static string Normalize(string? value) =>
        Values.FirstOrDefault(option => string.Equals(option, value, StringComparison.OrdinalIgnoreCase)) ?? Plain;
}
