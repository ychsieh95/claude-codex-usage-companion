using CodexUsageCompanion.Configuration;
using CodexUsageCompanion.RateLimits;

namespace CodexUsageCompanion.Lifecycle;

public enum UsageNotificationKind
{
    LowFiveHourUsage,
    LowWeeklyUsage,
    FiveHourUsageReset,
    WeeklyUsageReset
}

public sealed record UsageNotification(
    UsageNotificationKind Kind,
    int RemainingPercent);

public sealed class UsageNotificationTracker
{
    private RateLimitState? _previous;
    private bool _fiveHourLowAlerted;
    private bool _weeklyLowAlerted;

    public IReadOnlyList<UsageNotification> Evaluate(
        RateLimitState current,
        DateTimeOffset observedAt,
        CompanionSettings settings)
    {
        var notifications = new List<UsageNotification>();
        var fiveHourReset = IsReset(_previous?.FiveHour, current.FiveHour, observedAt);
        var weeklyReset = IsReset(_previous?.Weekly, current.Weekly, observedAt);

        if (fiveHourReset)
        {
            _fiveHourLowAlerted = false;
            if (settings.NotifyOnReset && current.FiveHour is not null)
            {
                notifications.Add(new UsageNotification(
                    UsageNotificationKind.FiveHourUsageReset,
                    current.FiveHour.RemainingPercent));
            }
        }

        if (weeklyReset)
        {
            _weeklyLowAlerted = false;
            if (settings.NotifyOnReset && current.Weekly is not null)
            {
                notifications.Add(new UsageNotification(
                    UsageNotificationKind.WeeklyUsageReset,
                    current.Weekly.RemainingPercent));
            }
        }

        var threshold = UsageAlertOptions.NormalizeThreshold(
            settings.LowUsageAlertThresholdPercent);
        EvaluateLowUsage(
            current.FiveHour,
            settings.EnableLowUsageAlert,
            threshold,
            ref _fiveHourLowAlerted,
            UsageNotificationKind.LowFiveHourUsage,
            notifications);
        EvaluateLowUsage(
            current.Weekly,
            settings.EnableLowUsageAlert,
            threshold,
            ref _weeklyLowAlerted,
            UsageNotificationKind.LowWeeklyUsage,
            notifications);

        _previous = current;
        return notifications;
    }

    private static void EvaluateLowUsage(
        RateLimitWindowState? window,
        bool enabled,
        int threshold,
        ref bool alerted,
        UsageNotificationKind kind,
        ICollection<UsageNotification> notifications)
    {
        if (!enabled)
        {
            alerted = false;
            return;
        }

        if (window is null || window.RemainingPercent >= threshold)
        {
            alerted = false;
            return;
        }

        if (alerted)
        {
            return;
        }

        alerted = true;
        notifications.Add(new UsageNotification(kind, window.RemainingPercent));
    }

    private static bool IsReset(
        RateLimitWindowState? previous,
        RateLimitWindowState? current,
        DateTimeOffset observedAt)
    {
        if (previous?.ResetsAt is not long previousReset ||
            current?.ResetsAt is not long currentReset ||
            currentReset <= previousReset)
        {
            return false;
        }

        return observedAt.ToUnixTimeSeconds() >= previousReset ||
               current.RemainingPercent > previous.RemainingPercent;
    }
}
