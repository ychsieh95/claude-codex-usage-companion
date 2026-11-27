using CodexUsageCompanion.Configuration;
using CodexUsageCompanion.RateLimits;
using CodexUsageCompanion.Ui;
using Xunit;

namespace CodexUsageCompanion.Tests;

public sealed class UsagePresentationTests
{
    [Theory]
    [InlineData(0, UsageSignal.Gray)]
    [InlineData(1, UsageSignal.Red)]
    [InlineData(39, UsageSignal.Red)]
    [InlineData(40, UsageSignal.Orange)]
    [InlineData(59, UsageSignal.Orange)]
    [InlineData(60, UsageSignal.Yellow)]
    [InlineData(79, UsageSignal.Yellow)]
    [InlineData(80, UsageSignal.Green)]
    [InlineData(100, UsageSignal.Green)]
    public void GetSignalUsesApprovedThresholds(int remainingPercent, UsageSignal expected)
    {
        Assert.Equal(expected, UsagePresentation.GetSignal(remainingPercent));
    }

    [Fact]
    public void GetPaceMarkerRatioPlacesPointerAtTimeStillToCome()
    {
        var now = DateTimeOffset.FromUnixTimeSeconds(1_000_000);
        // A 5h window that resets in 2h: 60% has passed, so the pointer sits at 40%.
        var state = new RateLimitWindowState(50, 300, now.ToUnixTimeSeconds() + 2 * 3600);

        Assert.Equal(0.6, UsagePresentation.GetElapsedRatio(state, now)!.Value, 6);
        Assert.Equal(0.4, UsagePresentation.GetPaceMarkerRatio(state, now)!.Value, 6);
    }

    [Fact]
    public void GetPaceMarkerRatioClampsResetFurtherThanOneWindow()
    {
        var now = DateTimeOffset.FromUnixTimeSeconds(1_000_000);
        var state = new RateLimitWindowState(100, 300, now.ToUnixTimeSeconds() + 6 * 3600);

        Assert.Equal(1d, UsagePresentation.GetPaceMarkerRatio(state, now));
    }

    [Theory]
    [InlineData(null, 3600L)]
    [InlineData(0, 3600L)]
    [InlineData(300, null)]
    [InlineData(300, 0L)]
    [InlineData(300, -60L)]
    public void GetPaceMarkerRatioIsNullWithoutUsableTiming(int? durationMins, long? resetOffsetSeconds)
    {
        var now = DateTimeOffset.FromUnixTimeSeconds(1_000_000);
        var state = new RateLimitWindowState(50, durationMins,
            resetOffsetSeconds is long offset ? now.ToUnixTimeSeconds() + offset : null);

        Assert.Null(UsagePresentation.GetPaceMarkerRatio(state, now));
        Assert.Null(UsagePresentation.GetPaceMarkerRatio(null, now));
    }

    [Fact]
    public void GetCellFillRatiosUsesFiveTwentyPercentCells()
    {
        var ratios = UsagePresentation.GetCellFillRatios(51);

        Assert.Equal(new[] { 1d, 1d, 0.55d, 0d, 0d }, ratios);
    }

    [Fact]
    public void FormatFiveHourResetUsesDefaultMonthDayFormat()
    {
        var reset = new DateTimeOffset(2026, 7, 10, 23, 33, 0, TimeSpan.FromHours(8));

        Assert.Equal("於 7月10日 重置", UsagePresentation.FormatFiveHourReset(reset));
    }

    [Fact]
    public void FormatWeeklyResetUsesDefaultMonthDayFormat()
    {
        var reset = new DateTimeOffset(2026, 7, 17, 9, 0, 0, TimeSpan.FromHours(8));

        Assert.Equal("於 7月17日 重置", UsagePresentation.FormatWeeklyReset(reset));
    }

    [Theory]
    [InlineData(HeaderBadgeOptions.Lowest, 23, "Codex")]
    [InlineData(HeaderBadgeOptions.Claude, 41, "Claude")]
    [InlineData(HeaderBadgeOptions.Codex, 23, "Codex")]
    [InlineData("unknown", 23, "Codex")]
    public void BadgeShowsTheLowestRemainingLimitOfTheSelectedScope(string mode, int expected, string provider)
    {
        var badge = UsagePresentation.SelectBadge(mode, Entries)!;
        Assert.Equal(expected, badge.RemainingPercent);
        Assert.Equal(provider, badge.Source!.Provider);
    }

    [Fact]
    public void BadgeAveragesShownLimitsAndNamesASingleLimit()
    {
        // (68 + 41 + 23 + 82) / 4 = 53.5, rounded away from zero.
        var average = UsagePresentation.SelectBadge(HeaderBadgeOptions.Average, Entries)!;
        Assert.Equal(54, average.RemainingPercent);
        Assert.Null(average.Source);
        Assert.Equal(4, average.Count);

        var single = UsagePresentation.SelectBadge(HeaderBadgeOptions.Average, [Entries[1]])!;
        Assert.Equal(41, single.RemainingPercent);
        Assert.Equal(Entries[1], single.Source);
    }

    [Fact]
    public void BadgeHidesWhenHiddenOrTheScopeHasNoData()
    {
        Assert.Null(UsagePresentation.SelectBadge(HeaderBadgeOptions.Hidden, Entries));
        Assert.Null(UsagePresentation.SelectBadge(HeaderBadgeOptions.Codex, [Entries[0], Entries[1]]));
        Assert.Null(UsagePresentation.SelectBadge(HeaderBadgeOptions.Lowest, []));
    }

    private static readonly BadgeEntry[] Entries =
    [
        new("Claude", "Current session", 68),
        new("Claude", "Current week (All)", 41),
        new("Codex", "5-hour limit", 23),
        new("Codex", "Weekly limit", 82)
    ];
}
