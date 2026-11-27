namespace CodexUsageCompanion.Lifecycle;

/// <summary>Shows or replaces a notification. Returns its id, or null when it could not be shown.</summary>
/// <param name="body">The notification text.</param>
/// <param name="percent">The download percentage for a progress hint, or null when there is none.</param>
/// <param name="replaceId">The notification to replace, or null to show a new one.</param>
/// <param name="final">True for the last notification, which reports the result.</param>
public delegate Task<uint?> ShowProgressNotification(string body, int? percent, uint? replaceId, bool final);

/// <summary>
/// Keeps one notification up to date while an update downloads and installs, then replaces it with the result.
/// Each update runs a separate notify-send, so reports are limited to one per interval, plus one whenever the
/// stage changes. Only the latest report is shown, and one notification is shown at a time, in order.
/// </summary>
public sealed class UpdateProgressNotification : IProgress<UpdateProgress>
{
    private readonly ShowProgressNotification _show;
    private readonly Func<UpdateProgress, string> _format;
    private readonly TimeSpan _interval;
    private readonly TimeProvider _time;
    private readonly object _gate = new();
    private UpdateProgress? _pending;
    private UpdateStage? _lastStage;
    private long _lastAcceptedAt;
    private Task _pump = Task.CompletedTask;
    private bool _pumping;
    private bool _finished;
    // Set when a notification could not be shown or gave no id. Without an id, every update would
    // show a new notification instead of replacing the last one, so progress stops.
    private bool _unavailable;
    private uint? _id;

    public UpdateProgressNotification(
        ShowProgressNotification show,
        Func<UpdateProgress, string> format,
        TimeSpan? interval = null,
        TimeProvider? time = null)
    {
        _show = show;
        _format = format;
        _interval = interval ?? TimeSpan.FromSeconds(1);
        _time = time ?? TimeProvider.System;
    }

    public void Report(UpdateProgress value)
    {
        lock (_gate)
        {
            if (_finished || _unavailable) return;
            var now = _time.GetTimestamp();
            if (value.Stage == _lastStage && _time.GetElapsedTime(_lastAcceptedAt, now) < _interval) return;
            _lastStage = value.Stage;
            _lastAcceptedAt = now;
            _pending = value;
            if (_pumping) return;
            _pumping = true;
            _pump = Task.Run(PumpAsync);
        }
    }

    /// <summary>Waits for the progress shown so far, then replaces it with the result.</summary>
    public async Task FinishAsync(string body)
    {
        Task pump;
        lock (_gate)
        {
            if (_finished) return;
            _finished = true;
            _pending = null;
            pump = _pump;
        }
        await pump;
        uint? id;
        lock (_gate) id = _id;
        await _show(body, null, id, final: true);
    }

    private async Task PumpAsync()
    {
        while (true)
        {
            UpdateProgress next;
            uint? id;
            lock (_gate)
            {
                if (_pending is null || _unavailable)
                {
                    _pumping = false;
                    return;
                }
                next = _pending;
                _pending = null;
                id = _id;
            }

            uint? shown;
            try
            {
                shown = await _show(_format(next), PercentOf(next), id, final: false);
            }
            catch (Exception)
            {
                shown = null;
            }

            lock (_gate)
            {
                if (shown is null) _unavailable = true;
                else _id = shown;
            }
        }
    }

    private static int? PercentOf(UpdateProgress progress) =>
        progress.Fraction is { } fraction ? (int)Math.Floor(fraction * 100) : null;
}
