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

/// <summary>
/// A limit that the header badge can summarize. <paramref name="Shown"/> is false for a limit
/// whose row is hidden: only a badge mode that names that one limit uses it.
/// <paramref name="WindowMinutes"/> and <paramref name="MinutesUntilReset"/> are null when unknown.
/// </summary>
public sealed record BadgeEntry(string Provider, UsageCardKind Kind, string Limit, int RemainingPercent, bool Shown = true,
    double? WindowMinutes = null, double? MinutesUntilReset = null);

/// <summary>How the header badge value was picked.</summary>
public enum BadgeBasis
{
    /// <summary>The lowest remaining limit among the candidates.</summary>
    Lowest,
    /// <summary>The one limit the mode names.</summary>
    Limit,
    /// <summary>The average of several limits.</summary>
    Average
}

/// <summary>
/// The header badge value. <paramref name="Source"/> names the limit it comes from;
/// it is null for an average, which covers <paramref name="Count"/> limits of
/// <paramref name="Provider"/>, or of every provider when that is null.
/// <paramref name="Method"/> is the <see cref="Configuration.BadgeAverageOptions"/> value an
/// average used. With <see cref="Configuration.BadgeAverageOptions.Pace"/>,
/// <paramref name="RemainingPercent"/> is a signed gap in percentage points, not a percentage left.
/// </summary>
public sealed record BadgeValue(int RemainingPercent, BadgeEntry? Source, int Count,
    BadgeBasis Basis = BadgeBasis.Lowest, string? Provider = null,
    string Method = Configuration.BadgeAverageOptions.Plain)
{
    public bool IsPaceGap => Basis == BadgeBasis.Average && Method == Configuration.BadgeAverageOptions.Pace;
}

public static class UsagePresentation
{
    /// <summary>
    /// Picks the header badge value for a <see cref="Configuration.HeaderBadgeOptions"/> mode,
    /// or null when the badge is hidden or the limits it needs have no data. Average modes
    /// combine limits with <paramref name="averageMethod"/>.
    /// </summary>
    public static BadgeValue? SelectBadge(string mode, IReadOnlyList<BadgeEntry> entries,
        string averageMethod = Configuration.BadgeAverageOptions.Plain)
    {
        var normalized = Configuration.HeaderBadgeOptions.Normalize(mode);
        if (normalized == Configuration.HeaderBadgeOptions.Hidden)
            return null;

        UsageCardKind? kind = normalized switch
        {
            Configuration.HeaderBadgeOptions.ClaudeSession => UsageCardKind.ClaudeSession,
            Configuration.HeaderBadgeOptions.ClaudeWeekly => UsageCardKind.ClaudeWeekly,
            Configuration.HeaderBadgeOptions.CodexFiveHour => UsageCardKind.CodexFiveHour,
            Configuration.HeaderBadgeOptions.CodexWeekly => UsageCardKind.CodexWeekly,
            _ => null
        };
        if (kind is not null)
        {
            // A named limit shows even when its row is hidden.
            var named = entries.FirstOrDefault(entry => entry.Kind == kind);
            return named is null
                ? null
                : new BadgeValue(Math.Clamp(named.RemainingPercent, 0, 100), named, 1, BadgeBasis.Limit, named.Provider);
        }

        string? provider = normalized switch
        {
            Configuration.HeaderBadgeOptions.Claude or Configuration.HeaderBadgeOptions.ClaudeAverage => "Claude",
            Configuration.HeaderBadgeOptions.Codex or Configuration.HeaderBadgeOptions.CodexAverage => "Codex",
            _ => null
        };
        var candidates = entries
            .Where(entry => entry.Shown && (provider is null || entry.Provider == provider))
            .ToList();
        if (candidates.Count == 0)
            return null;

        var average = normalized is Configuration.HeaderBadgeOptions.Average
            or Configuration.HeaderBadgeOptions.ClaudeAverage
            or Configuration.HeaderBadgeOptions.CodexAverage;
        if (average)
        {
            var method = Configuration.BadgeAverageOptions.Normalize(averageMethod);
            if (method == Configuration.BadgeAverageOptions.Pace)
            {
                // A limit without a reset time has no pace, so it sits out; with none
                // left, fall back to the plain average.
                var timed = candidates.Where(entry => entry.MinutesUntilReset is not null).ToList();
                if (timed.Count > 0)
                    candidates = timed;
                else
                    method = Configuration.BadgeAverageOptions.Plain;
            }
            // A pace gap means something even for one limit; any other average of one
            // limit is that limit, so the badge can name it.
            if (method == Configuration.BadgeAverageOptions.Pace || candidates.Count > 1)
                return new BadgeValue(Combine(candidates, method), null, candidates.Count, BadgeBasis.Average, provider, method);

            var only = candidates[0];
            return new BadgeValue(Math.Clamp(only.RemainingPercent, 0, 100), only, 1, BadgeBasis.Limit, provider);
        }

        var lowest = candidates.MinBy(entry => entry.RemainingPercent)!;
        return new BadgeValue(Math.Clamp(lowest.RemainingPercent, 0, 100), lowest, 1, BadgeBasis.Lowest, provider);
    }

    /// <summary>The length a limit's window has when the usage response leaves it out.</summary>
    public static double DefaultWindowMinutes(UsageCardKind kind) =>
        kind is UsageCardKind.ClaudeWeekly or UsageCardKind.CodexWeekly ? 7 * 24 * 60 : 5 * 60;

    private static int Combine(IReadOnlyList<BadgeEntry> entries, string method)
    {
        static double Remaining(BadgeEntry entry) => Math.Clamp(entry.RemainingPercent, 0, 100);
        static double Window(BadgeEntry entry) =>
            entry.WindowMinutes is > 0 ? entry.WindowMinutes.Value : DefaultWindowMinutes(entry.Kind);
        // Without a reset time, treat the whole window as still to come.
        static double UntilReset(BadgeEntry entry) =>
            Math.Clamp(entry.MinutesUntilReset ?? Window(entry), 0, Window(entry));

        double value = method switch
        {
            Configuration.BadgeAverageOptions.TimeToReset => Weighted(entries, UntilReset),
            Configuration.BadgeAverageOptions.WindowLength => Weighted(entries, Window),
            Configuration.BadgeAverageOptions.Pace => entries.Average(entry =>
                Remaining(entry) - UntilReset(entry) / Window(entry) * 100),
            _ => entries.Average(Remaining)
        };
        return (int)Math.Round(value, MidpointRounding.AwayFromZero);

        static double Weighted(IReadOnlyList<BadgeEntry> entries, Func<BadgeEntry, double> weight)
        {
            var total = entries.Sum(weight);
            // Every limit resetting right now leaves no weights; count them the same.
            return total > 0
                ? entries.Sum(entry => Remaining(entry) * weight(entry)) / total
                : entries.Average(Remaining);
        }
    }

    /// <summary>
    /// The color for a pace gap: on or ahead of an even pace is green, and the
    /// further behind it, the warmer.
    /// </summary>
    public static UsageSignal GetPaceSignal(int gap) => gap switch
    {
        >= 0 => UsageSignal.Green,
        > -10 => UsageSignal.Yellow,
        > -25 => UsageSignal.Orange,
        _ => UsageSignal.Red
    };

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
