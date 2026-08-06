using System.Runtime.ExceptionServices;
using CodexUsageCompanion.Platform;
using Xunit;

namespace CodexUsageCompanion.Tests;

public sealed class TrayIconExceptionPolicyTests
{
    [Fact]
    public void HandlesAvaloniaDbusWatcherTaskCancellation()
    {
        var exception = new TaskCanceledException("The tray watcher stopped.");
        ExceptionDispatchInfo.SetRemoteStackTrace(
            exception,
            """
               at Avalonia.FreeDesktop.DBusTrayIconImpl.WatchAsync()
               at Avalonia.Threading.DispatcherOperation.Execute()
            """);

        Assert.True(TrayIconExceptionPolicy.ShouldHandle(exception));
    }

    [Fact]
    public void DoesNotHandleOtherTaskCancellation()
    {
        var exception = new TaskCanceledException("An unrelated task stopped.");
        ExceptionDispatchInfo.SetRemoteStackTrace(
            exception,
            "   at CodexUsageCompanion.Lifecycle.CompanionRuntime.RunAsync()");

        Assert.False(TrayIconExceptionPolicy.ShouldHandle(exception));
    }

    [Fact]
    public void DoesNotHandleNonTaskCancellationFromTrayWatcher()
    {
        var exception = new InvalidOperationException("The tray watcher failed.");
        ExceptionDispatchInfo.SetRemoteStackTrace(
            exception,
            "   at Avalonia.FreeDesktop.DBusTrayIconImpl.WatchAsync()");

        Assert.False(TrayIconExceptionPolicy.ShouldHandle(exception));
    }
}
