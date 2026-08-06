using System.Text.Json;
using CodexUsageCompanion.Configuration;
using CodexUsageCompanion.Diagnostics;
using CodexUsageCompanion.Lifecycle;
using CodexUsageCompanion.RateLimits;
using Xunit;

namespace CodexUsageCompanion.Tests;

public sealed class UsageUpdateLogTests
{
    private static readonly DateTimeOffset UpdatedAt =
        new(2026, 7, 31, 2, 10, 0, TimeSpan.FromHours(8));

    private static readonly RateLimitState State = new(
        new RateLimitWindowState(71, 300, 1785435000),
        new RateLimitWindowState(58, 10080, 1786039800),
        2);

    [Fact]
    public void WritesTextSuccessAndFailureEntries()
    {
        WithTemporaryDirectory(directory =>
        {
            var path = Path.Combine(directory, "usage.txt");
            var log = new UsageUpdateLog();

            log.WriteSuccess(path, UsageLogOptions.Text, UsageProvider.Codex, State, UpdatedAt);
            log.WriteFailure(path, UsageLogOptions.Text, UsageProvider.Claude, "Claude unavailable", UpdatedAt);

            var lines = File.ReadAllLines(path);
            Assert.Equal(2, lines.Length);
            Assert.Contains("provider=codex", lines[0]);
            Assert.Contains("status=success", lines[0]);
            Assert.Contains("five_hour_remaining_percent=71", lines[0]);
            Assert.Contains("weekly_remaining_percent=58", lines[0]);
            Assert.Contains("available_reset_credits=2", lines[0]);
            Assert.Contains("provider=claude", lines[1]);
            Assert.Contains("status=error", lines[1]);
            Assert.Contains("error=Claude unavailable", lines[1]);
        });
    }

    [Fact]
    public void WritesOneCsvHeaderAndEscapesErrors()
    {
        WithTemporaryDirectory(directory =>
        {
            var path = Path.Combine(directory, "usage.csv");
            var log = new UsageUpdateLog();

            log.WriteSuccess(path, UsageLogOptions.Csv, UsageProvider.Codex, State, UpdatedAt);
            log.WriteFailure(path, UsageLogOptions.Csv, UsageProvider.Claude, "failed, retry", UpdatedAt);

            var lines = File.ReadAllLines(path);
            Assert.Equal(3, lines.Length);
            Assert.StartsWith("updated_at,provider,status,", lines[0]);
            Assert.Contains(",codex,success,71,", lines[1]);
            Assert.Contains(",claude,error,", lines[2]);
            Assert.EndsWith("\"failed, retry\"", lines[2]);
        });
    }

    [Fact]
    public void WritesJsonLinesWithStableFieldNames()
    {
        WithTemporaryDirectory(directory =>
        {
            var path = Path.Combine(directory, "usage.jsonl");
            var log = new UsageUpdateLog();

            log.WriteSuccess(path, UsageLogOptions.JsonLines, UsageProvider.Claude, State, UpdatedAt);

            using var document = JsonDocument.Parse(File.ReadAllText(path));
            var root = document.RootElement;
            Assert.Equal("claude", root.GetProperty("provider").GetString());
            Assert.Equal("success", root.GetProperty("status").GetString());
            Assert.Equal(71, root.GetProperty("fiveHourRemainingPercent").GetInt32());
            Assert.Equal(58, root.GetProperty("weeklyRemainingPercent").GetInt32());
            Assert.Equal(2, root.GetProperty("availableResetCredits").GetInt32());
        });
    }

    private static void WithTemporaryDirectory(Action<string> test)
    {
        var directory = Path.Combine(
            Path.GetTempPath(),
            $"CodexUsageCompanion.Tests.{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            test(directory);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
