namespace CodexUsageCompanion.Lifecycle;

public static class ClaudeUsagePolicy
{
    public static readonly TimeSpan MinimumPollInterval = TimeSpan.FromSeconds(180);

    public static bool ShouldFetch(DateTimeOffset now, DateTimeOffset? lastFetchAt) =>
        lastFetchAt is null || now - lastFetchAt.Value >= MinimumPollInterval;
}
