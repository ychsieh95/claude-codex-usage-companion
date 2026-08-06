using CodexUsageCompanion.RateLimits;
using Xunit;

namespace CodexUsageCompanion.Tests;

public sealed class CodexExecutableLocatorTests
{
    [Fact]
    public void FindUsesExplicitOverrideFirst()
    {
        var existing = new HashSet<string>(StringComparer.Ordinal)
        {
            "/opt/codex/bin/codex",
            "/usr/local/bin/codex"
        };

        var result = CodexExecutableLocator.Find(
            "/opt/codex/bin/codex",
            "/usr/local/bin",
            "/unused",
            "/home/test",
            existing.Contains);

        Assert.Equal("/opt/codex/bin/codex", result);
    }

    [Fact]
    public void FindUsesPathBeforeKnownUserLocations()
    {
        var existing = new HashSet<string>(StringComparer.Ordinal)
        {
            "/usr/local/bin/codex",
            "/home/test/.local/bin/codex"
        };

        var result = CodexExecutableLocator.Find(
            null,
            "/missing:/usr/local/bin",
            "/unused",
            "/home/test",
            existing.Contains);

        Assert.Equal("/usr/local/bin/codex", result);
    }

    [Fact]
    public void FindUsesKnownUserInstallLocation()
    {
        const string expected = "/home/test/.local/bin/codex";

        var result = CodexExecutableLocator.Find(
            null,
            string.Empty,
            "/unused",
            "/home/test",
            path => path == expected);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void FindReturnsNullWhenNoCandidateExists()
    {
        var result = CodexExecutableLocator.Find(
            null,
            "/missing",
            "/unused",
            "/home/test",
            _ => false);

        Assert.Null(result);
    }
}
