using CodexUsageCompanion.Lifecycle;
using Xunit;

namespace CodexUsageCompanion.Tests;

public sealed class UsageRefreshPolicyTests
{
    [Fact]
    public void InitialFailureShowsUnavailableState()
    {
        Assert.True(UsageRefreshPolicy.ShouldShowUnavailable(false));
    }

    [Fact]
    public void LaterFailureKeepsLastKnownUsage()
    {
        Assert.False(UsageRefreshPolicy.ShouldShowUnavailable(true));
    }

    [Theory]
    [InlineData("activate", false)]
    [InlineData("refresh", true)]
    [InlineData("ping", false)]
    public void OnlyExplicitRefreshMessagesFetchUsage(
        string message,
        bool expected)
    {
        Assert.Equal(
            expected,
            UsageRefreshPolicy.ShouldRefreshForInstanceMessage(message));
    }
}
