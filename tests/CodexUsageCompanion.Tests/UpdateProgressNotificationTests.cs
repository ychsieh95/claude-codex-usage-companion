using CodexUsageCompanion.Lifecycle;
using Xunit;

namespace CodexUsageCompanion.Tests;

public sealed class UpdateProgressNotificationTests
{
    private sealed class ManualTime : TimeProvider
    {
        private long _ticks;
        public override long TimestampFrequency => TimeSpan.TicksPerSecond;
        public override long GetTimestamp() => _ticks;
        public void Advance(TimeSpan by) => _ticks += by.Ticks;
    }

    private sealed record Shown(string Body, int? Percent, uint? ReplaceId, bool Final);

    private sealed class Recorder
    {
        private readonly object _gate = new();
        public List<Shown> Items { get; } = [];
        public uint? NextId { get; set; } = 7;

        public Task<uint?> Show(string body, int? percent, uint? replaceId, bool final)
        {
            lock (_gate) Items.Add(new(body, percent, replaceId, final));
            return Task.FromResult(NextId);
        }
    }

    private static string Format(UpdateProgress progress) => $"{progress.Stage}:{progress.BytesReceived}";

    // Waits for the background pump to show what has been reported so far.
    private static async Task Settle(Recorder recorder, int count)
    {
        for (var i = 0; i < 200 && recorder.Items.Count < count; i++) await Task.Delay(5);
    }

    [Fact]
    public async Task ReplacesOneNotificationAndEndsWithTheResult()
    {
        var recorder = new Recorder();
        var time = new ManualTime();
        var notification = new UpdateProgressNotification(recorder.Show, Format, TimeSpan.FromSeconds(1), time);

        notification.Report(new(UpdateStage.VerifyingSignature));
        await Settle(recorder, 1);
        notification.Report(new(UpdateStage.Downloading, 0, 100));
        await Settle(recorder, 2);
        // Within the interval and in the same stage, so it is skipped.
        notification.Report(new(UpdateStage.Downloading, 10, 100));
        time.Advance(TimeSpan.FromSeconds(1));
        notification.Report(new(UpdateStage.Downloading, 50, 100));
        await Settle(recorder, 3);
        await notification.FinishAsync("done");

        Assert.Equal(
        [
            new Shown("VerifyingSignature:0", null, null, false),
            new Shown("Downloading:0", 0, 7, false),
            new Shown("Downloading:50", 50, 7, false),
            new Shown("done", null, 7, true)
        ], recorder.Items);
    }

    [Fact]
    public async Task ShowsEveryStageChangeEvenWithinTheInterval()
    {
        var recorder = new Recorder();
        var notification = new UpdateProgressNotification(recorder.Show, Format, TimeSpan.FromHours(1), new ManualTime());

        foreach (var stage in new[] { UpdateStage.VerifyingSignature, UpdateStage.Downloading, UpdateStage.VerifyingPackage, UpdateStage.Installing })
        {
            notification.Report(new(stage));
            await Settle(recorder, recorder.Items.Count + 1);
        }
        await notification.FinishAsync("done");

        Assert.Equal(5, recorder.Items.Count);
        Assert.Equal("Installing:0", recorder.Items[3].Body);
    }

    [Fact]
    public async Task StopsProgressWhenNoIdComesBackButStillShowsTheResult()
    {
        var recorder = new Recorder { NextId = null };
        var time = new ManualTime();
        var notification = new UpdateProgressNotification(recorder.Show, Format, TimeSpan.Zero, time);

        notification.Report(new(UpdateStage.VerifyingSignature));
        await Settle(recorder, 1);
        notification.Report(new(UpdateStage.Downloading, 0, 100));
        notification.Report(new(UpdateStage.VerifyingPackage));
        await notification.FinishAsync("failed");

        Assert.Equal(
        [
            new Shown("VerifyingSignature:0", null, null, false),
            new Shown("failed", null, null, true)
        ], recorder.Items);
    }

    [Fact]
    public async Task IgnoresReportsAfterTheResult()
    {
        var recorder = new Recorder();
        var notification = new UpdateProgressNotification(recorder.Show, Format);

        await notification.FinishAsync("done");
        notification.Report(new(UpdateStage.Downloading));
        await notification.FinishAsync("again");
        await Task.Delay(20);

        Assert.Equal([new Shown("done", null, null, true)], recorder.Items);
    }
}
