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

    [Theory]
    [InlineData(HeaderBadgeOptions.ClaudeSession, 68, UsageCardKind.ClaudeSession)]
    [InlineData(HeaderBadgeOptions.ClaudeWeekly, 41, UsageCardKind.ClaudeWeekly)]
    [InlineData(HeaderBadgeOptions.CodexFiveHour, 23, UsageCardKind.CodexFiveHour)]
    [InlineData(HeaderBadgeOptions.CodexWeekly, 82, UsageCardKind.CodexWeekly)]
    public void BadgeShowsTheNamedLimit(string mode, int expected, UsageCardKind kind)
    {
        var badge = UsagePresentation.SelectBadge(mode, Entries)!;
        Assert.Equal(expected, badge.RemainingPercent);
        Assert.Equal(kind, badge.Source!.Kind);
        Assert.Equal(BadgeBasis.Limit, badge.Basis);
    }

    [Fact]
    public void BadgeShowsANamedLimitEvenWhenItsRowIsHidden()
    {
        BadgeEntry[] entries = [Entries[0], Entries[1] with { Shown = false }];

        Assert.Equal(41, UsagePresentation.SelectBadge(HeaderBadgeOptions.ClaudeWeekly, entries)!.RemainingPercent);
        Assert.Equal(68, UsagePresentation.SelectBadge(HeaderBadgeOptions.Claude, entries)!.RemainingPercent);
        Assert.Null(UsagePresentation.SelectBadge(HeaderBadgeOptions.CodexWeekly, entries));
    }

    [Theory]
    [InlineData(HeaderBadgeOptions.ClaudeAverage, "Claude", 55)]
    [InlineData(HeaderBadgeOptions.CodexAverage, "Codex", 53)]
    public void BadgeAveragesTheShownLimitsOfOneProvider(string mode, string provider, int expected)
    {
        var badge = UsagePresentation.SelectBadge(mode, Entries)!;
        Assert.Equal(expected, badge.RemainingPercent);
        Assert.Equal(BadgeBasis.Average, badge.Basis);
        Assert.Equal(provider, badge.Provider);
        Assert.Equal(2, badge.Count);
        Assert.Null(badge.Source);

        var single = UsagePresentation.SelectBadge(HeaderBadgeOptions.ClaudeAverage, [Entries[0], Entries[1] with { Shown = false }])!;
        Assert.Equal(68, single.RemainingPercent);
        Assert.Equal(BadgeBasis.Limit, single.Basis);
    }

    // Claude's session resets in 1h of 5h; its week resets in 3 days of 7.
    private static readonly BadgeEntry[] TimedEntries =
    [
        new("Claude", UsageCardKind.ClaudeSession, "Current session", 68, true, 300, 60),
        new("Claude", UsageCardKind.ClaudeWeekly, "Current week (All)", 41, true, 10080, 4320)
    ];

    [Theory]
    // (68 + 41) / 2 = 54.5
    [InlineData(BadgeAverageOptions.Plain, 55)]
    // (68 × 60 + 41 × 4320) / 4380 ≈ 41.37
    [InlineData(BadgeAverageOptions.TimeToReset, 41)]
    // (68 × 300 + 41 × 10080) / 10380 ≈ 41.78
    [InlineData(BadgeAverageOptions.WindowLength, 42)]
    // ((68 − 20) + (41 − 42.86)) / 2 ≈ 23.07
    [InlineData(BadgeAverageOptions.Pace, 23)]
    [InlineData("unknown", 55)]
    public void BadgeAverageUsesTheChosenMethod(string method, int expected)
    {
        foreach (var mode in new[] { HeaderBadgeOptions.Average, HeaderBadgeOptions.ClaudeAverage })
        {
            var badge = UsagePresentation.SelectBadge(mode, TimedEntries, method)!;
            Assert.Equal(expected, badge.RemainingPercent);
            Assert.Equal(BadgeBasis.Average, badge.Basis);
            Assert.Equal(BadgeAverageOptions.Normalize(method), badge.Method);
            Assert.Equal(method == BadgeAverageOptions.Pace, badge.IsPaceGap);
        }
    }

    [Fact]
    public void BadgeAverageMethodLeavesOtherModesAlone()
    {
        foreach (var method in BadgeAverageOptions.Values)
        {
            Assert.Equal(41, UsagePresentation.SelectBadge(HeaderBadgeOptions.Lowest, TimedEntries, method)!.RemainingPercent);
            Assert.Equal(68, UsagePresentation.SelectBadge(HeaderBadgeOptions.ClaudeSession, TimedEntries, method)!.RemainingPercent);
        }
    }

    [Fact]
    public void PaceGapCoversASingleLimitAndSkipsLimitsWithoutAResetTime()
    {
        var single = UsagePresentation.SelectBadge(HeaderBadgeOptions.Average, [TimedEntries[0]], BadgeAverageOptions.Pace)!;
        Assert.Equal(48, single.RemainingPercent);
        Assert.True(single.IsPaceGap);
        Assert.Equal(1, single.Count);

        BadgeEntry[] partly = [TimedEntries[0], TimedEntries[1] with { MinutesUntilReset = null }];
        Assert.Equal(48, UsagePresentation.SelectBadge(HeaderBadgeOptions.Average, partly, BadgeAverageOptions.Pace)!.RemainingPercent);

        // With no reset times at all, the pace gap falls back to the plain average.
        var untimed = UsagePresentation.SelectBadge(HeaderBadgeOptions.Average, Entries, BadgeAverageOptions.Pace)!;
        Assert.Equal(54, untimed.RemainingPercent);
        Assert.False(untimed.IsPaceGap);
    }

    [Fact]
    public void WeightedAveragesUseDefaultWindowsWhenTimingIsUnknown()
    {
        BadgeEntry[] untimed = [TimedEntries[0] with { WindowMinutes = null, MinutesUntilReset = null },
            TimedEntries[1] with { WindowMinutes = null, MinutesUntilReset = null }];
        // Unknown reset counts as the whole default window: 5h and 7 days.
        Assert.Equal(42, UsagePresentation.SelectBadge(HeaderBadgeOptions.Average, untimed, BadgeAverageOptions.TimeToReset)!.RemainingPercent);
        Assert.Equal(42, UsagePresentation.SelectBadge(HeaderBadgeOptions.Average, untimed, BadgeAverageOptions.WindowLength)!.RemainingPercent);

        // Both resetting right now leaves no weight, so they count the same.
        BadgeEntry[] resetting = [TimedEntries[0] with { MinutesUntilReset = 0 }, TimedEntries[1] with { MinutesUntilReset = 0 }];
        Assert.Equal(55, UsagePresentation.SelectBadge(HeaderBadgeOptions.Average, resetting, BadgeAverageOptions.TimeToReset)!.RemainingPercent);
    }

    [Theory]
    [InlineData(23, UsageSignal.Green)]
    [InlineData(0, UsageSignal.Green)]
    [InlineData(-9, UsageSignal.Yellow)]
    [InlineData(-10, UsageSignal.Orange)]
    [InlineData(-24, UsageSignal.Orange)]
    [InlineData(-25, UsageSignal.Red)]
    public void PaceSignalWarmsAsUsageFallsBehind(int gap, UsageSignal expected)
    {
        Assert.Equal(expected, UsagePresentation.GetPaceSignal(gap));
    }

    private static readonly BadgeEntry[] Entries =
    [
        new("Claude", UsageCardKind.ClaudeSession, "Current session", 68),
        new("Claude", UsageCardKind.ClaudeWeekly, "Current week (All)", 41),
        new("Codex", UsageCardKind.CodexFiveHour, "5-hour limit", 23),
        new("Codex", UsageCardKind.CodexWeekly, "Weekly limit", 82)
    ];
}
