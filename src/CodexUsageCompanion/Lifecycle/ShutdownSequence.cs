namespace CodexUsageCompanion.Lifecycle;

public static class ShutdownSequence
{
    public static readonly TimeSpan DefaultBackgroundDrainTimeout =
        TimeSpan.FromSeconds(2);

    public static async Task RunAsync(
        Action uiCleanup,
        Func<Task> drainBackground,
        Func<ValueTask> asyncCleanup,
        TimeSpan? backgroundDrainTimeout = null)
    {
        var failures = new List<Exception>();
        try
        {
            uiCleanup();
        }
        catch (Exception exception)
        {
            failures.Add(exception);
        }

        try
        {
            var drainTimeout = backgroundDrainTimeout ?? DefaultBackgroundDrainTimeout;
            await drainBackground().WaitAsync(drainTimeout);
        }
        catch (Exception exception)
        {
            failures.Add(exception);
        }

        try
        {
            await asyncCleanup();
        }
        catch (Exception exception)
        {
            failures.Add(exception);
        }

        if (failures.Count == 1)
        {
            throw failures[0];
        }

        if (failures.Count > 1)
        {
            throw new AggregateException(failures);
        }
    }
}
