using CodexUsageCompanion.RateLimits;
using Xunit;

namespace CodexUsageCompanion.Tests;

public sealed class ClaudeCredentialsLocatorTests
{
    [Fact]
    public void FindPrefersExplicitPathWhenItExists()
    {
        var result = ClaudeCredentialsLocator.Find(
            "/explicit/.credentials.json",
            "/home/user",
            path => path == "/explicit/.credentials.json");

        Assert.Equal("/explicit/.credentials.json", result);
    }

    [Fact]
    public void FindFallsBackToDefaultClaudeDirectory()
    {
        var expected = Path.Combine("/home/user", ".claude", ".credentials.json");

        var result = ClaudeCredentialsLocator.Find(
            explicitPath: null,
            "/home/user",
            path => path == expected);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void FindReturnsNullWhenNothingExists()
    {
        var result = ClaudeCredentialsLocator.Find(
            "/explicit/.credentials.json",
            "/home/user",
            _ => false);

        Assert.Null(result);
    }

    [Fact]
    public void FindTrimsQuotesFromExplicitPath()
    {
        var result = ClaudeCredentialsLocator.Find(
            "\"/explicit/.credentials.json\"",
            "/home/user",
            path => path == "/explicit/.credentials.json");

        Assert.Equal("/explicit/.credentials.json", result);
    }
}
