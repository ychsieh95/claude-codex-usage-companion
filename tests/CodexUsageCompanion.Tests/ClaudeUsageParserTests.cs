using System.Text.Json;
using CodexUsageCompanion.RateLimits;
using Xunit;

namespace CodexUsageCompanion.Tests;

public sealed class ClaudeUsageParserTests
{
    [Fact]
    public void ParseResponseReadsConfirmedLiveResponseShape()
    {
        const string json = """
        {
          "five_hour": {"utilization": 12.0, "resets_at": "2026-07-31T17:50:00.441718+00:00"},
          "seven_day": {"utilization": 2.0, "resets_at": "2026-08-02T07:00:00.441741+00:00"},
          "seven_day_opus": null,
          "seven_day_sonnet": null,
          "extra_usage": {
            "is_enabled": true,
            "monthly_limit": 100,
            "used_credits": 6.32,
            "currency": "USD"
          }
        }
        """;

        var state = ClaudeUsageParser.ParseResponse(json);

        Assert.Equal(88, state.FiveHour?.RemainingPercent);
        Assert.Equal(300, state.FiveHour?.WindowDurationMins);
        Assert.Equal(98, state.Weekly?.RemainingPercent);
        Assert.Equal(10080, state.Weekly?.WindowDurationMins);
        Assert.Equal(
            new DateTimeOffset(2026, 7, 31, 17, 50, 0, TimeSpan.Zero).ToUnixTimeSeconds(),
            state.FiveHour?.ResetsAt);
        Assert.NotNull(state.ExtraUsage);
        Assert.True(state.ExtraUsage!.Enabled);
        Assert.Equal(6.32m, state.ExtraUsage.UsedAmount);
        Assert.Equal(100m, state.ExtraUsage.LimitAmount);
        Assert.Equal("USD", state.ExtraUsage.Currency);
        Assert.Null(state.CreditBalance);
        Assert.False(state.AutomaticReloadEnabled);
        Assert.Null(state.AvailableResetCredits);
    }

    [Theory]
    [InlineData(12.0, 88)]
    [InlineData(12.4, 88)]
    [InlineData(12.5, 87)]
    [InlineData(0, 100)]
    [InlineData(100, 0)]
    [InlineData(-5, 100)]
    [InlineData(140, 0)]
    public void ParseResponseRoundsFloatUtilizationToRemainingPercent(double utilization, int expectedRemaining)
    {
        var json = $$"""{"five_hour": {"utilization": {{utilization}} } }""";

        var state = ClaudeUsageParser.ParseResponse(json);

        Assert.Equal(expectedRemaining, state.FiveHour?.RemainingPercent);
    }

    [Fact]
    public void ParseResponseHandlesDisabledExtraUsageWithNullAmounts()
    {
        const string json = """
        {
          "five_hour": {"utilization": 5.0},
          "seven_day": {"utilization": 1.0},
          "extra_usage": {
            "is_enabled": false,
            "monthly_limit": null,
            "used_credits": null,
            "currency": null
          }
        }
        """;

        var state = ClaudeUsageParser.ParseResponse(json);

        Assert.NotNull(state.ExtraUsage);
        Assert.False(state.ExtraUsage!.Enabled);
        Assert.Null(state.ExtraUsage.UsedAmount);
        Assert.Null(state.ExtraUsage.LimitAmount);
        Assert.Null(state.ExtraUsage.Currency);
    }

    [Fact]
    public void ParseResponseAllowsMissingWindowsAndExtraUsage()
    {
        const string json = "{}";

        var state = ClaudeUsageParser.ParseResponse(json);

        Assert.Null(state.FiveHour);
        Assert.Null(state.Weekly);
        Assert.Null(state.ExtraUsage);
    }

    [Fact]
    public void ParseResponseTreatsUnparsableResetsAtAsUnavailable()
    {
        const string json = """{"five_hour": {"utilization": 10.0, "resets_at": "not-a-date"}}""";

        var state = ClaudeUsageParser.ParseResponse(json);

        Assert.Equal(90, state.FiveHour?.RemainingPercent);
        Assert.Null(state.FiveHour?.ResetsAt);
    }

    [Fact]
    public void ParseResponseRejectsMalformedJson()
    {
        Assert.ThrowsAny<JsonException>(() => ClaudeUsageParser.ParseResponse("{"));
    }
}
