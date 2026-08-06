namespace CodexUsageCompanion.Platform;

public static class TrayIconExceptionPolicy
{
    private const string AvaloniaDbusWatcher =
        "Avalonia.FreeDesktop.DBusTrayIconImpl.WatchAsync";

    public static bool ShouldHandle(Exception exception)
    {
        return exception is TaskCanceledException &&
            exception.StackTrace?.Contains(
                AvaloniaDbusWatcher,
                StringComparison.Ordinal) == true;
    }
}
