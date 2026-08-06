using Avalonia.Threading;
using CodexUsageCompanion.Configuration;
using CodexUsageCompanion.Diagnostics;
using CodexUsageCompanion.RateLimits;
using CodexUsageCompanion.Ui;

namespace CodexUsageCompanion.Lifecycle;

public sealed class CompanionRuntime : IAsyncDisposable
{
    private readonly ResidentLease _lease;
    private readonly UsageOverlayWindow _window;
    private readonly CodexAppServerClient? _appServerClient;
    private readonly ClaudeUsageClient? _claudeClient;
    private readonly CancellationTokenSource _cancellation = new();
    private readonly SemaphoreSlim _codexRefreshGate = new(1, 1);
    private readonly SemaphoreSlim _claudeRefreshGate = new(1, 1);
    private TimeSpan _refreshInterval;
    private PeriodicTimer? _periodicTimer;
    private Task? _periodicTask;
    private bool _hasCodexUsageState;
    private bool _hasClaudeUsageState;
    private DateTimeOffset? _lastClaudeFetchAt;
    private int _activeRefreshCount;
    private int _stopping;
    private int _disposed;
    private bool _started;

    public CompanionRuntime(
        ResidentLease lease,
        UsageOverlayWindow window,
        CompanionSettings settings)
    {
        _lease = lease;
        _window = window;
        _refreshInterval = TimeSpan.FromSeconds(settings.RefreshIntervalSeconds);
        _appServerClient = settings.EnableCodexUsage ? new CodexAppServerClient() : null;
        _claudeClient = settings.EnableClaudeUsage ? new ClaudeUsageClient() : null;
    }

    public event Action<UsageProvider, RateLimitState, DateTimeOffset>? UsageUpdated;
    public event Action<UsageProvider, string, DateTimeOffset>? UsageUpdateFailed;

    public void Start()
    {
        if (_started || Volatile.Read(ref _disposed) != 0)
        {
            return;
        }

        _started = true;
        CompanionLog.Shared.Write("lifecycle", "resident-started platform=linux");
        _window.RefreshRequested += HandleRefreshRequested;
        if (_appServerClient is not null)
        {
            _appServerClient.RateLimitsChanged += HandleRateLimitsChanged;
        }

        _lease.MessageReceived += HandleInstanceMessage;
        _lease.Start();
        _periodicTimer = new PeriodicTimer(_refreshInterval);
        _periodicTask = RunPeriodicRefreshAsync(_periodicTimer, _cancellation.Token);
        RequestRefresh();
    }

    public void UpdateRefreshInterval(int seconds)
    {
        if (Volatile.Read(ref _stopping) != 0)
        {
            return;
        }

        _refreshInterval = TimeSpan.FromSeconds(UpdateIntervalOptions.Normalize(seconds));
        if (_periodicTimer is not null)
        {
            _periodicTimer.Period = _refreshInterval;
        }
    }

    public void RefreshUsage()
    {
        RequestRefresh();
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        Volatile.Write(ref _stopping, 1);
        _started = false;
        _window.RefreshRequested -= HandleRefreshRequested;
        if (_appServerClient is not null)
        {
            _appServerClient.RateLimitsChanged -= HandleRateLimitsChanged;
        }

        _lease.MessageReceived -= HandleInstanceMessage;
        _cancellation.Cancel();
        _periodicTimer?.Dispose();
        var failures = new List<Exception>();
        if (_periodicTask is not null)
        {
            try
            {
                await _periodicTask;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                failures.Add(exception);
            }
        }

        _periodicTimer = null;
        var codexGateHeld = false;
        var claudeGateHeld = false;
        try
        {
            await _codexRefreshGate.WaitAsync();
            codexGateHeld = true;
            await _claudeRefreshGate.WaitAsync();
            claudeGateHeld = true;
            if (_appServerClient is not null)
            {
                try
                {
                    await _appServerClient.DisposeAsync();
                }
                catch (Exception exception)
                {
                    failures.Add(exception);
                }
            }

            if (_claudeClient is not null)
            {
                try
                {
                    await _claudeClient.DisposeAsync();
                }
                catch (Exception exception)
                {
                    failures.Add(exception);
                }
            }
        }
        catch (Exception exception)
        {
            failures.Add(exception);
        }
        finally
        {
            if (claudeGateHeld)
            {
                _claudeRefreshGate.Release();
            }

            if (codexGateHeld)
            {
                _codexRefreshGate.Release();
            }

            _codexRefreshGate.Dispose();
            _claudeRefreshGate.Dispose();
            _cancellation.Dispose();
        }

        try
        {
            _lease.Dispose();
        }
        catch (Exception exception)
        {
            failures.Add(exception);
        }

        CompanionLog.Shared.Write("lifecycle", "resident-stopped");
        if (failures.Count == 1)
        {
            throw failures[0];
        }

        if (failures.Count > 1)
        {
            throw new AggregateException(failures);
        }
    }

    private async Task RunPeriodicRefreshAsync(
        PeriodicTimer timer,
        CancellationToken cancellationToken)
    {
        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            RequestRefresh();
        }
    }

    private void HandleRefreshRequested(object? sender, EventArgs eventArgs) => RequestRefresh();

    private void HandleRateLimitsChanged() => RequestRefresh();

    private void HandleInstanceMessage(string message)
    {
        if (Volatile.Read(ref _stopping) != 0)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (Volatile.Read(ref _stopping) != 0)
            {
                return;
            }

            if (message == "activate")
            {
                _window.RestoreAndActivate();
            }

            if (UsageRefreshPolicy.ShouldRefreshForInstanceMessage(message))
            {
                RequestRefresh();
            }
        });
    }

    private void RequestRefresh()
    {
        if (Volatile.Read(ref _stopping) != 0)
        {
            return;
        }

        _ = RefreshCodexAsync(_cancellation.Token);
        _ = RefreshClaudeAsync(_cancellation.Token);
    }

    private async Task RefreshCodexAsync(CancellationToken cancellationToken)
    {
        if (_appServerClient is null)
        {
            return;
        }

        if (!await _codexRefreshGate.WaitAsync(0, cancellationToken))
        {
            return;
        }

        await EnterLoadingAsync();
        try
        {
            var state = await _appServerClient.ReadRateLimitsAsync(cancellationToken);
            var updatedAt = DateTimeOffset.Now;
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _window.UpdateUsage(UsageProvider.Codex, state);
                _window.SetStatus(updatedAt, null);
                UsageUpdated?.Invoke(UsageProvider.Codex, state, updatedAt);
            });
            _hasCodexUsageState = true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            CompanionLog.Shared.Write("refresh", exception);
            var failedAt = DateTimeOffset.Now;
            var error = Program.FriendlyError(exception);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (UsageRefreshPolicy.ShouldShowUnavailable(_hasCodexUsageState))
                {
                    _window.UpdateUsage(UsageProvider.Codex, null);
                }

                _window.SetStatus(null, error);
                UsageUpdateFailed?.Invoke(UsageProvider.Codex, error, failedAt);
            });
        }
        finally
        {
            await ExitLoadingAsync();
            _codexRefreshGate.Release();
        }
    }

    private async Task RefreshClaudeAsync(CancellationToken cancellationToken)
    {
        if (_claudeClient is null)
        {
            return;
        }

        if (!await _claudeRefreshGate.WaitAsync(0, cancellationToken))
        {
            return;
        }

        if (!ClaudeUsagePolicy.ShouldFetch(DateTimeOffset.UtcNow, _lastClaudeFetchAt))
        {
            _claudeRefreshGate.Release();
            return;
        }

        await EnterLoadingAsync();
        try
        {
            _lastClaudeFetchAt = DateTimeOffset.UtcNow;
            var state = await _claudeClient.ReadUsageAsync(cancellationToken);
            var updatedAt = DateTimeOffset.Now;
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _window.UpdateUsage(UsageProvider.Claude, state);
                _window.SetStatus(updatedAt, null);
                UsageUpdated?.Invoke(UsageProvider.Claude, state, updatedAt);
            });
            _hasClaudeUsageState = true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            CompanionLog.Shared.Write("claude-refresh", exception);
            var failedAt = DateTimeOffset.Now;
            var error = Program.FriendlyError(exception);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (UsageRefreshPolicy.ShouldShowUnavailable(_hasClaudeUsageState))
                {
                    _window.UpdateUsage(UsageProvider.Claude, null);
                }

                UsageUpdateFailed?.Invoke(UsageProvider.Claude, error, failedAt);
            });
        }
        finally
        {
            await ExitLoadingAsync();
            _claudeRefreshGate.Release();
        }
    }

    private async Task EnterLoadingAsync()
    {
        if (Interlocked.Increment(ref _activeRefreshCount) == 1)
        {
            await Dispatcher.UIThread.InvokeAsync(() => _window.SetLoading(true));
        }
    }

    private async Task ExitLoadingAsync()
    {
        if (Interlocked.Decrement(ref _activeRefreshCount) == 0)
        {
            await Dispatcher.UIThread.InvokeAsync(() => _window.SetLoading(false));
        }
    }
}
