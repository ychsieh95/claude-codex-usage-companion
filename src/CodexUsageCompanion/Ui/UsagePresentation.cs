using CodexUsageCompanion.Localization;
using CodexUsageCompanion.RateLimits;

namespace CodexUsageCompanion.Ui;

public enum UsageSignal
{
    Gray,
    Red,
    Orange,
    Yellow,
    Green
}

public static class UsagePresentation
{
    public static UsageSignal GetSignal(int remainingPercent)
    {
        var remaining = Math.Clamp(remainingPercent, 0, 100);
        return remaining switch
        {
            0 => UsageSignal.Gray,
            < 40 => UsageSignal.Red,
            < 60 => UsageSignal.Orange,
            < 80 => UsageSignal.Yellow,
            _ => UsageSignal.Green
        };
    }

    public static double[] GetCellFillRatios(int remainingPercent)
    {
        var remaining = Math.Clamp(remainingPercent, 0, 100);
        return Enumerable.Range(0, 5)
            .Select(index => Math.Clamp((remaining - index * 20) / 20d, 0d, 1d))
            .ToArray();
    }

    /// <summary>
    /// The share of the reset window that has already passed, from 0 to 1.
    /// Null when the window length or reset time is unknown, or the reset time has passed.
    /// </summary>
    public static double? GetElapsedRatio(RateLimitWindowState? state, DateTimeOffset now)
    {
        if (state?.WindowDurationMins is not > 0 || state.ResetsAt is not long resetsAt)
        {
            return null;
        }

        var windowSeconds = state.WindowDurationMins.Value * 60d;
        var untilReset = resetsAt - now.ToUnixTimeSeconds();
        if (untilReset <= 0)
        {
            return null;
        }

        return Math.Clamp(1 - untilReset / windowSeconds, 0d, 1d);
    }

    /// <summary>
    /// Where the even-pace pointer sits on a remaining-usage bar: the share of the window
    /// still to come. A bar that ends before the pointer means usage is faster than an even pace.
    /// </summary>
    public static double? GetPaceMarkerRatio(RateLimitWindowState? state, DateTimeOffset now) =>
        GetElapsedRatio(state, now) is double elapsed ? 1 - elapsed : null;

    public static string FormatFiveHourReset(DateTimeOffset localReset)
    {
        return UiText.For(UiLanguage.TraditionalChinese).FormatFiveHourReset(localReset);
    }

    public static string FormatWeeklyReset(DateTimeOffset localReset)
    {
        return UiText.For(UiLanguage.TraditionalChinese).FormatWeeklyReset(localReset);
    }
}
