namespace CodexUsageCompanion.Configuration;

public static class UsageAlertOptions
{
    public const int MinimumThresholdPercent = 1;
    public const int MaximumThresholdPercent = 100;
    public const int DefaultThresholdPercent = 20;

    public static int NormalizeThreshold(int thresholdPercent)
    {
        return Math.Clamp(
            thresholdPercent,
            MinimumThresholdPercent,
            MaximumThresholdPercent);
    }
}
