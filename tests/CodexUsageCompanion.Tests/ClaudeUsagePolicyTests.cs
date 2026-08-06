using CodexUsageCompanion.Lifecycle;
using Xunit;

namespace CodexUsageCompanion.Tests;

public sealed class ClaudeUsagePolicyTests
{
    [Fact]
    public void ShouldFetchWhenNeverFetchedBefore()
    {
        Assert.True(ClaudeUsagePolicy.ShouldFetch(DateTimeOffset.UtcNow, null));
    }

    [Fact]
    public void ShouldNotFetchBeforeMinimumIntervalElapses()
    {
        var now = DateTimeOffset.UtcNow;
        var lastFetchAt = now - TimeSpan.FromSeconds(179);

        Assert.False(ClaudeUsagePolicy.ShouldFetch(now, lastFetchAt));
    }

    [Fact]
    public void ShouldFetchOnceMinimumIntervalElapses()
    {
        var now = DateTimeOffset.UtcNow;
        var lastFetchAt = now - TimeSpan.FromSeconds(180);

        Assert.True(ClaudeUsagePolicy.ShouldFetch(now, lastFetchAt));
    }
}
