using CodexUsageCompanion.RateLimits;
using Xunit;

namespace CodexUsageCompanion.Tests;

public sealed class ClaudeUserAgentResolverTests
{
    [Fact]
    public void ResolveUsesInstalledVersionWhenAvailable()
    {
        var result = ClaudeUserAgentResolver.Resolve(
            () => "/usr/bin/claude",
            _ => "2.1.160");

        Assert.Equal("claude-cli/2.1.160", result);
    }

    [Fact]
    public void ResolveFallsBackWhenExecutableIsNotFound()
    {
        var result = ClaudeUserAgentResolver.Resolve(
            () => null,
            _ => "2.1.160");

        Assert.Equal("claude-cli/2.0.0", result);
    }

    [Fact]
    public void ResolveFallsBackWhenVersionCannotBeRead()
    {
        var result = ClaudeUserAgentResolver.Resolve(
            () => "/usr/bin/claude",
            _ => null);

        Assert.Equal("claude-cli/2.0.0", result);
    }
}
