using System.Globalization;
using System.Text.Json;

namespace CodexUsageCompanion.RateLimits;

public static class ClaudeUsageParser
{
    private const int FiveHourDurationMins = 300;
    private const int WeeklyDurationMins = 10080;

    public static RateLimitState ParseResponse(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var fiveHour = ParseWindow(root, "five_hour", FiveHourDurationMins);
        var weekly = ParseWindow(root, "seven_day", WeeklyDurationMins);
        return new RateLimitState(fiveHour, weekly, null)
        {
            ExtraUsage = ParseExtraUsage(root)
        };
    }

    private static RateLimitWindowState? ParseWindow(JsonElement root, string propertyName, int durationMins)
    {
        if (!root.TryGetProperty(propertyName, out var window) ||
            window.ValueKind != JsonValueKind.Object ||
            !window.TryGetProperty("utilization", out var utilizationElement) ||
            utilizationElement.ValueKind != JsonValueKind.Number)
        {
            return null;
        }

        var utilization = utilizationElement.GetDouble();
        var remainingPercent = 100 - (int)Math.Round(
            Math.Clamp(utilization, 0, 100),
            MidpointRounding.AwayFromZero);
        var resetsAt = ParseResetsAt(window);
        return new RateLimitWindowState(remainingPercent, durationMins, resetsAt);
    }

    private static long? ParseResetsAt(JsonElement window)
    {
        if (!window.TryGetProperty("resets_at", out var resetsAtElement) ||
            resetsAtElement.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return DateTimeOffset.TryParse(
            resetsAtElement.GetString(),
            CultureInfo.InvariantCulture,
            DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
            out var parsed)
            ? parsed.ToUnixTimeSeconds()
            : null;
    }

    private static RateLimitExtraUsageState? ParseExtraUsage(JsonElement root)
    {
        if (!root.TryGetProperty("extra_usage", out var extraUsage) ||
            extraUsage.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var enabled = extraUsage.TryGetProperty("is_enabled", out var enabledElement) &&
                      enabledElement.ValueKind == JsonValueKind.True;
        var usedAmount = ParseDecimal(extraUsage, "used_credits");
        var limitAmount = ParseDecimal(extraUsage, "monthly_limit");
        var currency = extraUsage.TryGetProperty("currency", out var currencyElement) &&
                       currencyElement.ValueKind == JsonValueKind.String
            ? currencyElement.GetString()
            : null;

        return new RateLimitExtraUsageState(enabled, usedAmount, limitAmount, currency);
    }

    private static decimal? ParseDecimal(JsonElement parent, string propertyName)
    {
        return parent.TryGetProperty(propertyName, out var element) &&
               element.ValueKind == JsonValueKind.Number
            ? element.GetDecimal()
            : null;
    }
}
