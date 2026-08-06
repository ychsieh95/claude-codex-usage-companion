using CodexUsageCompanion.Configuration;
using CodexUsageCompanion.Lifecycle;
using CodexUsageCompanion.RateLimits;
using Xunit;

namespace CodexUsageCompanion.Tests;

public sealed class UsageNotificationTrackerTests
{
    private static readonly DateTimeOffset Now =
        DateTimeOffset.FromUnixTimeSeconds(2_000);

    [Fact]
    public void LowUsageAlertFiresOnceUntilUsageRecovers()
    {
        var tracker = new UsageNotificationTracker();
        var settings = new CompanionSettings
        {
            EnableLowUsageAlert = true,
            LowUsageAlertThresholdPercent = 20
        };

        var first = tracker.Evaluate(State(fiveHour: 19), Now, settings);
        var repeated = tracker.Evaluate(State(fiveHour: 10), Now, settings);
        tracker.Evaluate(State(fiveHour: 20), Now, settings);
        var rearmed = tracker.Evaluate(State(fiveHour: 5), Now, settings);

        Assert.Collection(
            first,
            item => Assert.Equal(
                UsageNotificationKind.LowFiveHourUsage,
                item.Kind));
        Assert.Empty(repeated);
        Assert.Single(rearmed);
    }

    [Fact]
    public void ThresholdComparisonIsStrictlyLowerThanConfiguredValue()
    {
        var tracker = new UsageNotificationTracker();
        var settings = new CompanionSettings
        {
            EnableLowUsageAlert = true,
            LowUsageAlertThresholdPercent = 20
        };

        var notifications = tracker.Evaluate(
            State(fiveHour: 20, weekly: 19),
            Now,
            settings);

        var notification = Assert.Single(notifications);
        Assert.Equal(UsageNotificationKind.LowWeeklyUsage, notification.Kind);
        Assert.Equal(19, notification.RemainingPercent);
    }

    [Fact]
    public void DisabledAlertDoesNotLatchLowState()
    {
        var tracker = new UsageNotificationTracker();
        tracker.Evaluate(
            State(weekly: 10),
            Now,
            new CompanionSettings());

        var notifications = tracker.Evaluate(
            State(weekly: 10),
            Now,
            new CompanionSettings
            {
                EnableLowUsageAlert = true,
                LowUsageAlertThresholdPercent = 20
            });

        Assert.Single(notifications);
    }

    [Fact]
    public void ResetNotificationRequiresAConfirmedNewWindow()
    {
        var tracker = new UsageNotificationTracker();
        var settings = new CompanionSettings { NotifyOnReset = true };
        tracker.Evaluate(
            State(weekly: 5, weeklyReset: 1_900),
            DateTimeOffset.FromUnixTimeSeconds(1_800),
            settings);

        var notifications = tracker.Evaluate(
            State(weekly: 100, weeklyReset: 8_000),
            Now,
            settings);

        var notification = Assert.Single(notifications);
        Assert.Equal(UsageNotificationKind.WeeklyUsageReset, notification.Kind);
        Assert.Equal(100, notification.RemainingPercent);
    }

    [Fact]
    public void ChangedFutureResetTimeWithoutRecoveryDoesNotNotify()
    {
        var tracker = new UsageNotificationTracker();
        var settings = new CompanionSettings { NotifyOnReset = true };
        tracker.Evaluate(
            State(weekly: 50, weeklyReset: 5_000),
            Now,
            settings);

        var notifications = tracker.Evaluate(
            State(weekly: 40, weeklyReset: 6_000),
            Now,
            settings);

        Assert.Empty(notifications);
    }

    private static RateLimitState State(
        int? fiveHour = null,
        int? weekly = null,
        long fiveHourReset = 5_000,
        long weeklyReset = 9_000)
    {
        return new RateLimitState(
            fiveHour is int five
                ? new RateLimitWindowState(five, 300, fiveHourReset)
                : null,
            weekly is int week
                ? new RateLimitWindowState(week, 10080, weeklyReset)
                : null,
            null);
    }
}
