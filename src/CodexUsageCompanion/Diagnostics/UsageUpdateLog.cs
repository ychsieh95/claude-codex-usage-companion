using System.Globalization;
using System.Text;
using System.Text.Json;
using CodexUsageCompanion.Configuration;
using CodexUsageCompanion.Lifecycle;
using CodexUsageCompanion.RateLimits;

namespace CodexUsageCompanion.Diagnostics;

public sealed class UsageUpdateLog
{
    private const string CsvHeader =
        "updated_at,provider,status,five_hour_remaining_percent,five_hour_reset_at," +
        "weekly_remaining_percent,weekly_reset_at,available_reset_credits,error";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly object _sync = new();

    public void WriteSuccess(
        string filePath,
        string format,
        UsageProvider provider,
        RateLimitState state,
        DateTimeOffset updatedAt)
    {
        Write(filePath, format, CreateEntry(provider, "success", state, updatedAt, null));
    }

    public void WriteFailure(
        string filePath,
        string format,
        UsageProvider provider,
        string error,
        DateTimeOffset updatedAt)
    {
        Write(filePath, format, CreateEntry(provider, "error", null, updatedAt, error));
    }

    private void Write(string filePath, string format, UsageUpdateLogEntry entry)
    {
        var normalizedFormat = UsageLogOptions.NormalizeFormat(format);
        var normalizedPath = UsageLogOptions.NormalizeFilePath(filePath, normalizedFormat);

        lock (_sync)
        {
            var directory = Path.GetDirectoryName(normalizedPath)
                ?? throw new InvalidOperationException("The usage log directory is unavailable.");
            Directory.CreateDirectory(directory);

            var addCsvHeader =
                normalizedFormat == UsageLogOptions.Csv &&
                (!File.Exists(normalizedPath) || new FileInfo(normalizedPath).Length == 0);
            var content = normalizedFormat switch
            {
                UsageLogOptions.Csv => FormatCsv(entry),
                UsageLogOptions.JsonLines => JsonSerializer.Serialize(entry, JsonOptions),
                _ => FormatText(entry)
            };

            using var writer = new StreamWriter(
                normalizedPath,
                append: true,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            if (addCsvHeader)
            {
                writer.WriteLine(CsvHeader);
            }

            writer.WriteLine(content);
        }
    }

    private static UsageUpdateLogEntry CreateEntry(
        UsageProvider provider,
        string status,
        RateLimitState? state,
        DateTimeOffset updatedAt,
        string? error)
    {
        return new UsageUpdateLogEntry(
            updatedAt.ToString("O", CultureInfo.InvariantCulture),
            provider == UsageProvider.Claude ? "claude" : "codex",
            status,
            state?.FiveHour?.RemainingPercent,
            FormatReset(state?.FiveHour?.ResetsAt),
            state?.Weekly?.RemainingPercent,
            FormatReset(state?.Weekly?.ResetsAt),
            state?.AvailableResetCredits,
            error);
    }

    private static string? FormatReset(long? unixSeconds)
    {
        return unixSeconds is long value
            ? DateTimeOffset.FromUnixTimeSeconds(value)
                .ToString("O", CultureInfo.InvariantCulture)
            : null;
    }

    private static string FormatText(UsageUpdateLogEntry entry)
    {
        return string.Join(
            " | ",
            $"updated_at={Text(entry.UpdatedAt)}",
            $"provider={Text(entry.Provider)}",
            $"status={Text(entry.Status)}",
            $"five_hour_remaining_percent={Text(entry.FiveHourRemainingPercent)}",
            $"five_hour_reset_at={Text(entry.FiveHourResetAt)}",
            $"weekly_remaining_percent={Text(entry.WeeklyRemainingPercent)}",
            $"weekly_reset_at={Text(entry.WeeklyResetAt)}",
            $"available_reset_credits={Text(entry.AvailableResetCredits)}",
            $"error={Text(entry.Error)}");
    }

    private static string FormatCsv(UsageUpdateLogEntry entry)
    {
        return string.Join(
            ',',
            Csv(entry.UpdatedAt),
            Csv(entry.Provider),
            Csv(entry.Status),
            Csv(entry.FiveHourRemainingPercent),
            Csv(entry.FiveHourResetAt),
            Csv(entry.WeeklyRemainingPercent),
            Csv(entry.WeeklyResetAt),
            Csv(entry.AvailableResetCredits),
            Csv(entry.Error));
    }

    private static string Csv(object? value)
    {
        var text = Value(value);
        return text.IndexOfAny([',', '"', '\r', '\n']) >= 0
            ? $"\"{text.Replace("\"", "\"\"", StringComparison.Ordinal)}\""
            : text;
    }

    private static string Value(object? value) =>
        Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;

    private static string Text(object? value) =>
        Value(value)
            .Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal);

    private sealed record UsageUpdateLogEntry(
        string UpdatedAt,
        string Provider,
        string Status,
        int? FiveHourRemainingPercent,
        string? FiveHourResetAt,
        int? WeeklyRemainingPercent,
        string? WeeklyResetAt,
        int? AvailableResetCredits,
        string? Error);
}
