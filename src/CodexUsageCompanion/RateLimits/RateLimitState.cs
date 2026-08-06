namespace CodexUsageCompanion.RateLimits;

public sealed record RateLimitWindowState(
    int RemainingPercent,
    int? WindowDurationMins,
    long? ResetsAt);

public sealed record RateLimitExtraUsageState(
    bool Enabled,
    decimal? UsedAmount,
    decimal? LimitAmount,
    string? Currency);

public sealed record RateLimitState(
    RateLimitWindowState? FiveHour,
    RateLimitWindowState? Weekly,
    int? AvailableResetCredits)
{
    public string? CreditBalance { get; init; }
    public bool AutomaticReloadEnabled { get; init; }
    public RateLimitExtraUsageState? ExtraUsage { get; init; }
}
