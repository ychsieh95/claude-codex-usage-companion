namespace CodexUsageCompanion.Lifecycle;

public static class ShutdownSequence
{
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
            var backgroundDrain = drainBackground();
            if (backgroundDrainTimeout is null)
            {
                await backgroundDrain;
            }
            else
            {
                await backgroundDrain.WaitAsync(backgroundDrainTimeout.Value);
            }
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
