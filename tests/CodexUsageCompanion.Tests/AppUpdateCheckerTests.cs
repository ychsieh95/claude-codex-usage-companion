using System.Text.Json;
using CodexUsageCompanion.Lifecycle;
using Xunit;

namespace CodexUsageCompanion.Tests;

public sealed class AppUpdateCheckerTests
{
    [Theory]
    [InlineData("v0.1.4", "0.1.3", true)]
    [InlineData("v0.1.3", "0.1.3.0", false)]
    [InlineData("0.1.2", "0.1.3", false)]
    [InlineData("v0.10.0", "0.9.9", true)]
    [InlineData("v1.0.0+build", "0.1.3", true)]
    public void ComparesNumericVersions(string tag, string current, bool available)
    {
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(new
        {
            tag_name = tag, draft = false, prerelease = false
        }));
        var result = AppUpdateChecker.ParseRelease(json.RootElement, current);
        Assert.Equal(available, result.UpdateAvailable);
        Assert.StartsWith(AppUpdateChecker.RepositoryUrl + "/releases/tag/", result.ReleaseUrl.AbsoluteUri);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void DoesNotOfferDraftsOrPrereleases(bool draft, bool prerelease)
    {
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(new { tag_name = "v9.0.0", draft, prerelease }));
        Assert.False(AppUpdateChecker.ParseRelease(json.RootElement, "0.1.3").UpdateAvailable);
    }

    [Fact]
    public void RejectsMalformedVersion()
    {
        using var json = JsonDocument.Parse("{\"tag_name\":\"unknown\",\"draft\":false,\"prerelease\":false}");
        Assert.Throws<FormatException>(() => AppUpdateChecker.ParseRelease(json.RootElement, "0.1.3"));
    }
}
