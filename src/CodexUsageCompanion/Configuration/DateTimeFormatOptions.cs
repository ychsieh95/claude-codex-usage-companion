using System.Globalization;
using System.Text;

namespace CodexUsageCompanion.Configuration;

public static class DateTimeFormatOptions
{
    public const string MonthDay = "MMM D";
    public const string MonthDayTime = "MMM D HH:mm";
    public const string YearMonthDay = "yyyy-MM-dd";
    public const string YearMonthDayTime = "yyyy-MM-dd HH:mm";
    public const string HourMinute = "HH:mm";
    public const string HourMinuteSecond = "HH:mm:ss";

    public static IReadOnlyList<string> ResetFormats { get; } =
        [MonthDay, MonthDayTime, YearMonthDay, YearMonthDayTime];

    public static IReadOnlyList<string> LastUpdatedFormats { get; } =
        [HourMinute, HourMinuteSecond, MonthDayTime, YearMonthDayTime];

    public static string NormalizeReset(string? value)
    {
        var candidate = value?.Trim();
        if (string.Equals(candidate, "MMM d", StringComparison.Ordinal))
        {
            return MonthDay;
        }

        return IsValid(candidate) ? candidate! : MonthDay;
    }

    public static string NormalizeLastUpdated(string? value)
    {
        var candidate = value?.Trim();
        return IsValid(candidate) ? candidate! : HourMinute;
    }

    public static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 80)
        {
            return false;
        }

        try
        {
            _ = DateTimeOffset.UnixEpoch.ToString(
                ToDotNetFormat(value),
                CultureInfo.InvariantCulture);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public static string ToDotNetFormat(string value)
    {
        if (value == "D")
        {
            return "%d";
        }

        var result = new StringBuilder(value.Length);
        var inSingleQuote = false;
        var inDoubleQuote = false;
        var escaped = false;
        foreach (var character in value)
        {
            if (escaped)
            {
                result.Append(character);
                escaped = false;
                continue;
            }

            if (character == '\\')
            {
                result.Append(character);
                escaped = true;
                continue;
            }

            if (character == '\'' && !inDoubleQuote)
            {
                inSingleQuote = !inSingleQuote;
                result.Append(character);
                continue;
            }

            if (character == '"' && !inSingleQuote)
            {
                inDoubleQuote = !inDoubleQuote;
                result.Append(character);
                continue;
            }

            result.Append(character == 'D' && !inSingleQuote && !inDoubleQuote
                ? 'd'
                : character);
        }

        return result.ToString();
    }
}
