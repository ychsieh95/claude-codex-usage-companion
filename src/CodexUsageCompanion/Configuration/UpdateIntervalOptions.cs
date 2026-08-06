namespace CodexUsageCompanion.Configuration;

public static class UpdateIntervalOptions
{
    public const int MinimumSeconds = 60;
    public const int MaximumSeconds = 3600;

    public static IReadOnlyList<int> CommonValues { get; } =
        [60, 300, 600, 900, 1800, 3600];

    public static int Normalize(int seconds)
    {
        return Math.Clamp(seconds, MinimumSeconds, MaximumSeconds);
    }
}
