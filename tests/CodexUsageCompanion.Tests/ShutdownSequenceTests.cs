using CodexUsageCompanion.Lifecycle;
using Xunit;

namespace CodexUsageCompanion.Tests;

public sealed class ShutdownSequenceTests
{
    [Fact]
    public void DefaultBackgroundDrainTimeoutIsBounded()
    {
        Assert.InRange(
            ShutdownSequence.DefaultBackgroundDrainTimeout,
            TimeSpan.FromMilliseconds(1),
            TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task UiCleanupRunsBeforeWaitingForBackgroundWork()
    {
        var backgroundCompletion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var uiCleanupRan = false;

        var shutdown = ShutdownSequence.RunAsync(
            () => uiCleanupRan = true,
            () => backgroundCompletion.Task,
            () => ValueTask.CompletedTask);

        Assert.True(uiCleanupRan);
        Assert.False(shutdown.IsCompleted);

        backgroundCompletion.SetResult();
        await shutdown;
    }

    [Fact]
    public async Task FinalCleanupRunsAfterBackgroundWorkFails()
    {
        var finalCleanupRan = false;

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ShutdownSequence.RunAsync(
                () => { },
                () => Task.FromException(new InvalidOperationException("drain failed")),
                () =>
                {
                    finalCleanupRan = true;
                    return ValueTask.CompletedTask;
                }));

        Assert.Equal("drain failed", exception.Message);
        Assert.True(finalCleanupRan);
    }

    [Fact]
    public async Task BackgroundAndFinalCleanupStillRunAfterUiCleanupFails()
    {
        var backgroundRan = false;
        var finalCleanupRan = false;

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ShutdownSequence.RunAsync(
                () => throw new InvalidOperationException("UI cleanup failed"),
                () =>
                {
                    backgroundRan = true;
                    return Task.CompletedTask;
                },
                () =>
                {
                    finalCleanupRan = true;
                    return ValueTask.CompletedTask;
                }));

        Assert.True(backgroundRan);
        Assert.True(finalCleanupRan);
    }

    [Fact]
    public async Task FinalCleanupRunsWhenBackgroundDrainTimesOut()
    {
        var neverCompletes = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var finalCleanupRan = false;

        await Assert.ThrowsAsync<TimeoutException>(() =>
            ShutdownSequence.RunAsync(
                () => { },
                () => neverCompletes.Task,
                () =>
                {
                    finalCleanupRan = true;
                    return ValueTask.CompletedTask;
                },
                TimeSpan.FromMilliseconds(10)));

        Assert.True(finalCleanupRan);
    }
}
