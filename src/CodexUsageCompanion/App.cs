using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using CodexUsageCompanion.Configuration;
using CodexUsageCompanion.Diagnostics;
using CodexUsageCompanion.Lifecycle;
using CodexUsageCompanion.Localization;
using CodexUsageCompanion.Platform;
using CodexUsageCompanion.RateLimits;
using CodexUsageCompanion.Ui;

namespace CodexUsageCompanion;

public sealed class App : Application
{
    private static readonly TimeSpan OsShutdownDrainTimeout = TimeSpan.FromSeconds(2);
    private IClassicDesktopStyleApplicationLifetime? _desktop;
    private CompanionRuntime? _runtime;
    private UsageOverlayWindow? _window;
    private SettingsWindow? _settingsWindow;
    private TrayIcon? _trayIcon;
    private DispatcherTimer? _trayTooltipBridgeTimer;
    private readonly LinuxTrayTooltipBridge _trayTooltipBridge = new();
    private readonly LinuxDesktopNotificationService _notificationService = new();
    private readonly UsageNotificationTracker _codexNotificationTracker = new();
    private readonly UsageNotificationTracker _claudeNotificationTracker = new();
    private readonly UsageUpdateLog _usageUpdateLog = new();
    private CompanionSettings _settings = new();
    private UiText _text = UiText.For(UiLanguage.English);
    private RateLimitState? _lastCodexUsage;
    private DateTimeOffset? _lastCodexUpdatedAt;
    private RateLimitState? _lastClaudeUsage;
    private DateTimeOffset? _lastClaudeUpdatedAt;
    private string? _renderedTrayIconStyle;
    private int? _renderedTrayRemainingPercent;
    private bool _shutdownRequested;
    private Task? _shutdownTask;

    public override void Initialize()
    {
        Dispatcher.UIThread.UnhandledException += HandleDispatcherUnhandledException;
        RequestedThemeVariant = ThemeVariant.Default;
        Styles.Add(new FluentTheme());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _desktop = desktop;
            _settings = CompanionSettingsStore.Load();
            ApplyTheme(_settings.Theme);
            _text = ResolveText(_settings);
            _window = new UsageOverlayWindow(_settings, _text);
            var lease = GuiLaunchContext.ResidentLease
                ?? throw new InvalidOperationException("The GUI instance lease is unavailable.");
            _runtime = new CompanionRuntime(lease, _window, _settings);
            _runtime.UsageUpdated += HandleUsageUpdated;
            _runtime.UsageUpdateFailed += HandleUsageUpdateFailed;

            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var startHidden = _settings.StartOnBoot &&
                _settings.MinimizeOnStart &&
                GuiLaunchContext.LaunchedInBackground;
            if (startHidden)
            {
                _runtime.Start();
            }
            else
            {
                desktop.MainWindow = _window;
            }

            _window.Opened += (_, _) => _runtime.Start();
            _window.SettingsRequested += async (_, _) => await ShowSettingsAsync();
            _window.AlwaysOnTopRequested += HandleAlwaysOnTopRequested;
            _window.Closing += async (_, eventArgs) =>
            {
                if (_shutdownRequested)
                {
                    return;
                }

                if (eventArgs.CloseReason == WindowCloseReason.OSShutdown)
                {
                    await ShutdownAsync(OsShutdownDrainTimeout);
                    return;
                }

                eventArgs.Cancel = true;
                if (_settings.EnableSystemTray)
                {
                    _window.Hide();
                    return;
                }

                await ShutdownAsync();
            };

            if (_settings.StartOnBoot)
            {
                try
                {
                    SetAutostart(enabled: true);
                }
                catch (Exception exception) when (
                    exception is IOException or
                    UnauthorizedAccessException or
                    ArgumentException or
                    InvalidOperationException)
                {
                    CompanionLog.Shared.Write("autostart", exception);
                }
            }

            UpdateTrayIcon();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void UpdateTrayIcon()
    {
        if (!_settings.EnableSystemTray || _window is null)
        {
            DisposeTrayIcon();
            return;
        }

        var menu = CreateTrayMenu();
        var tooltipText = FormatTrayTooltip();
        _trayTooltipBridge.UpdateText(tooltipText);
        if (_trayIcon is not null)
        {
            _trayIcon.ToolTipText = tooltipText;
            _trayIcon.Menu = menu;
            UpdateTrayIconImage();
            _trayIcon.IsVisible = true;
            StartTrayTooltipBridge();
            _trayTooltipBridge.Invalidate();
            return;
        }

        _trayIcon = new TrayIcon
        {
            ToolTipText = tooltipText,
            Menu = menu,
            IsVisible = false
        };
        UpdateTrayIconImage();
        _trayIcon.IsVisible = true;
        _trayIcon.Clicked += (_, _) => ShowWindow();
        TrayIcon.SetIcons(this, new TrayIcons { _trayIcon });
        StartTrayTooltipBridge();
    }

    private NativeMenu CreateTrayMenu()
    {
        var show = new NativeMenuItem(_text.TrayShowAction);
        show.Click += (_, _) => ShowWindow();

        var refresh = new NativeMenuItem(_text.RefreshAction);
        refresh.Click += (_, _) => _runtime?.RefreshUsage();

        var settings = new NativeMenuItem(_text.SettingsAction);
        settings.Click += async (_, _) =>
        {
            ShowWindow();
            await ShowSettingsAsync();
        };

        var alwaysOnTop = new NativeMenuItem(_text.AlwaysOnTopOption)
        {
            ToggleType = MenuItemToggleType.CheckBox,
            IsChecked = _settings.AlwaysOnTop
        };
        alwaysOnTop.Click += (_, _) =>
        {
            var enabled = !_settings.AlwaysOnTop;
            try
            {
                _settings = CompanionSettingsStore.Save(
                    _settings with { AlwaysOnTop = enabled });
                _window!.ApplyAlwaysOnTop(_settings.AlwaysOnTop);
                alwaysOnTop.IsChecked = _settings.AlwaysOnTop;
            }
            catch (Exception exception) when (
                exception is IOException or
                UnauthorizedAccessException or
                ArgumentException or
                InvalidOperationException)
            {
                alwaysOnTop.IsChecked = _settings.AlwaysOnTop;
                CompanionLog.Shared.Write("tray-always-on-top", exception);
            }
        };

        var startOnBoot = new NativeMenuItem(_text.StartOnBootOption)
        {
            ToggleType = MenuItemToggleType.CheckBox,
            IsChecked = _settings.StartOnBoot
        };
        startOnBoot.Click += (_, _) =>
        {
            var enabled = !_settings.StartOnBoot;
            try
            {
                SetAutostart(enabled);
                _settings = CompanionSettingsStore.Save(
                    _settings with { StartOnBoot = enabled });
                startOnBoot.IsChecked = _settings.StartOnBoot;
            }
            catch (Exception exception) when (
                exception is IOException or
                UnauthorizedAccessException or
                ArgumentException or
                InvalidOperationException)
            {
                startOnBoot.IsChecked = _settings.StartOnBoot;
                CompanionLog.Shared.Write("tray-start-on-boot", exception);
            }
        };

        var quit = new NativeMenuItem(_text.TrayQuitAction);
        quit.Click += async (_, _) => await ShutdownAsync();

        var menu = new NativeMenu();
        menu.Items.Add(show);
        menu.Items.Add(refresh);
        menu.Items.Add(new NativeMenuItemSeparator());
        menu.Items.Add(alwaysOnTop);
        menu.Items.Add(startOnBoot);
        menu.Items.Add(new NativeMenuItemSeparator());
        menu.Items.Add(settings);
        menu.Items.Add(quit);
        return menu;
    }

    private WindowIcon CreateTrayWindowIcon()
    {
        if (_settings.TrayIconStyle == TrayIconStyleOptions.Original)
        {
            using var iconStream = AssetLoader.Open(
                new Uri("avares://CodexUsageCompanion/Assets/claude-codex-usage-companion.png"));
            return new WindowIcon(iconStream);
        }

        var remainingPercent = TrayIconRenderer.ResolveRemainingPercent(
            _settings.TrayIconStyle,
            _lastClaudeUsage,
            _lastCodexUsage);
        using var generatedIcon = new MemoryStream(
            TrayIconRenderer.CreateUsageIcon(remainingPercent),
            writable: false);
        return new WindowIcon(generatedIcon);
    }

    private void UpdateTrayIconImage()
    {
        if (_trayIcon is null)
        {
            return;
        }

        var style = TrayIconStyleOptions.Normalize(_settings.TrayIconStyle);
        var remainingPercent = TrayIconRenderer.ResolveRemainingPercent(
            style,
            _lastClaudeUsage,
            _lastCodexUsage);
        if (style == _renderedTrayIconStyle &&
            (style == TrayIconStyleOptions.Original ||
             remainingPercent == _renderedTrayRemainingPercent))
        {
            return;
        }

        _trayIcon.Icon = CreateTrayWindowIcon();
        _renderedTrayIconStyle = style;
        _renderedTrayRemainingPercent = remainingPercent;
    }

    private void DisposeTrayIcon()
    {
        _trayTooltipBridgeTimer?.Stop();
        _trayTooltipBridgeTimer = null;
        if (_trayIcon is null)
        {
            return;
        }

        _trayIcon.IsVisible = false;
        _trayIcon = null;
        _renderedTrayIconStyle = null;
        _renderedTrayRemainingPercent = null;
        TrayIcon.SetIcons(this, new TrayIcons());
    }

    private void StartTrayTooltipBridge()
    {
        if (!OperatingSystem.IsLinux() || _trayIcon is null)
        {
            return;
        }

        _trayTooltipBridge.TryInstall(_trayIcon);
        if (_trayTooltipBridgeTimer is not null)
        {
            _trayTooltipBridgeTimer.Interval = TimeSpan.FromSeconds(1);
            return;
        }

        _trayTooltipBridgeTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _trayTooltipBridgeTimer.Tick += (_, _) =>
        {
            if (_trayIcon is null)
            {
                _trayTooltipBridgeTimer?.Stop();
                return;
            }

            _trayTooltipBridgeTimer!.Interval =
                _trayTooltipBridge.TryInstall(_trayIcon)
                    ? TimeSpan.FromSeconds(30)
                    : TimeSpan.FromSeconds(1);
        };
        _trayTooltipBridgeTimer.Start();
    }

    private static void HandleDispatcherUnhandledException(
        object? sender,
        DispatcherUnhandledExceptionEventArgs eventArgs)
    {
        if (!TrayIconExceptionPolicy.ShouldHandle(eventArgs.Exception))
        {
            return;
        }

        eventArgs.Handled = true;
        CompanionLog.Shared.Write(
            "tray",
            "Ignored Avalonia D-Bus tray watcher cancellation during disposal.");
    }

    private void ShowWindow()
    {
        if (_window is null)
        {
            return;
        }

        _window.RestoreAndActivate();
    }

    private async Task ShowSettingsAsync()
    {
        if (_window is null)
        {
            return;
        }

        if (_settingsWindow is not null)
        {
            _settingsWindow.Activate();
            return;
        }

        var dialog = new SettingsWindow(_settings, _text);
        dialog.PositionPreviewRequested += _window.ApplyPosition;
        dialog.ApplyRequested += TryApplySettings;
        _settingsWindow = dialog;
        try
        {
            var updated = await dialog.ShowDialog<CompanionSettings?>(_window);
            if (updated is null)
            {
                _window.ApplyPosition(_settings.Position);
                return;
            }

            ApplySettings(updated);
        }
        catch (Exception exception) when (
            exception is IOException or
            UnauthorizedAccessException or
            ArgumentException or
            InvalidOperationException)
        {
            CompanionLog.Shared.Write("settings", exception);
            _window.ApplyPosition(_settings.Position);
            _window.SetStatus(null, $"Unable to save settings: {exception.Message}");
        }
        finally
        {
            dialog.PositionPreviewRequested -= _window.ApplyPosition;
            dialog.ApplyRequested -= TryApplySettings;
            _settingsWindow = null;
        }
    }

    private bool TryApplySettings(CompanionSettings settings)
    {
        if (_window is null)
        {
            return false;
        }

        try
        {
            ApplySettings(settings);
            return true;
        }
        catch (Exception exception) when (
            exception is IOException or
            UnauthorizedAccessException or
            ArgumentException or
            InvalidOperationException)
        {
            CompanionLog.Shared.Write("settings-apply", exception);
            _window.SetStatus(null, $"Unable to apply settings: {exception.Message}");
            return false;
        }
    }

    private void ApplySettings(CompanionSettings settings)
    {
        var normalized = CompanionSettingsStore.Save(settings);
        SetAutostart(normalized.StartOnBoot);
        _settings = normalized;
        ApplyTheme(_settings.Theme);
        _text = ResolveText(_settings);
        _window!.ApplySettings(_settings, _text);
        _runtime?.UpdateRefreshInterval(_settings.RefreshIntervalSeconds);
        UpdateTrayIcon();
    }

    private void HandleAlwaysOnTopRequested(bool enabled)
    {
        if (_window is null)
        {
            return;
        }

        try
        {
            _settings = CompanionSettingsStore.Save(
                _settings with { AlwaysOnTop = enabled });
            _window.ApplyAlwaysOnTop(_settings.AlwaysOnTop);
            UpdateTrayIcon();
        }
        catch (Exception exception) when (
            exception is IOException or
            UnauthorizedAccessException or
            ArgumentException or
            InvalidOperationException)
        {
            _window.ApplyAlwaysOnTop(_settings.AlwaysOnTop);
            CompanionLog.Shared.Write("window-always-on-top", exception);
            _window.SetStatus(
                null,
                $"Unable to save always-on-top setting: {exception.Message}");
        }
    }

    private static void SetAutostart(bool enabled)
    {
        var executablePath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(executablePath))
        {
            throw new InvalidOperationException("The application executable path is unavailable.");
        }

        LinuxAutostartManager.SetEnabled(enabled, executablePath);
    }

    private void HandleUsageUpdated(UsageProvider provider, RateLimitState state, DateTimeOffset updatedAt)
    {
        var tracker = provider == UsageProvider.Claude ? _claudeNotificationTracker : _codexNotificationTracker;
        var notifications = tracker.Evaluate(state, updatedAt, _settings);
        if (provider == UsageProvider.Claude)
        {
            _lastClaudeUsage = state;
            _lastClaudeUpdatedAt = updatedAt;
        }
        else
        {
            _lastCodexUsage = state;
            _lastCodexUpdatedAt = updatedAt;
        }

        if (_trayIcon is not null)
        {
            var tooltipText = FormatTrayTooltip();
            _trayTooltipBridge.UpdateText(tooltipText);
            _trayIcon.ToolTipText = tooltipText;
            UpdateTrayIconImage();
            _trayTooltipBridge.Invalidate();
        }

        if (_settings.EnableUsageLogging)
        {
            TryWriteUsageLog(() => _usageUpdateLog.WriteSuccess(
                _settings.UsageLogFilePath,
                _settings.UsageLogFormat,
                provider,
                state,
                updatedAt));
        }

        foreach (var notification in notifications)
        {
            ShowUsageNotification(provider, notification);
        }
    }

    private string FormatTrayTooltip() =>
        _text.FormatTrayTooltip(_lastCodexUsage, _lastCodexUpdatedAt, _lastClaudeUsage, _lastClaudeUpdatedAt);

    private void ShowUsageNotification(UsageProvider provider, UsageNotification notification)
    {
        var weekly = notification.Kind is
            UsageNotificationKind.LowWeeklyUsage or
            UsageNotificationKind.WeeklyUsageReset;
        var reset = notification.Kind is
            UsageNotificationKind.FiveHourUsageReset or
            UsageNotificationKind.WeeklyUsageReset;
        var claude = provider == UsageProvider.Claude;
        _notificationService.Show(
            reset
                ? (claude ? _text.ClaudeUsageResetTitle : _text.UsageResetTitle)
                : (claude ? _text.ClaudeLowUsageAlertTitle : _text.LowUsageAlertTitle),
            reset
                ? _text.FormatResetNotification(
                    weekly,
                    notification.RemainingPercent)
                : _text.FormatLowUsageAlert(
                    weekly,
                    notification.RemainingPercent),
            critical: !reset);
    }

    private void HandleUsageUpdateFailed(UsageProvider provider, string error, DateTimeOffset updatedAt)
    {
        if (_settings.EnableUsageLogging)
        {
            TryWriteUsageLog(() => _usageUpdateLog.WriteFailure(
                _settings.UsageLogFilePath,
                _settings.UsageLogFormat,
                provider,
                error,
                updatedAt));
        }
    }

    private static void TryWriteUsageLog(Action write)
    {
        try
        {
            write();
        }
        catch (Exception exception) when (
            exception is IOException or
            UnauthorizedAccessException or
            ArgumentException or
            InvalidOperationException or
            NotSupportedException)
        {
            CompanionLog.Shared.Write("usage-log", exception);
        }
    }

    private async Task ShutdownAsync(TimeSpan? backgroundDrainTimeout = null)
    {
        if (_shutdownTask is not null)
        {
            await _shutdownTask;
            return;
        }

        if (_desktop is null)
        {
            return;
        }

        _shutdownRequested = true;
        _shutdownTask = ShutdownCoreAsync(_desktop, backgroundDrainTimeout);
        await _shutdownTask;
    }

    private async Task ShutdownCoreAsync(
        IClassicDesktopStyleApplicationLifetime desktop,
        TimeSpan? backgroundDrainTimeout)
    {
        try
        {
            await ShutdownSequence.RunAsync(
                () =>
                {
                    DisposeTrayIcon();
                    _settingsWindow?.Hide();
                    _window?.Hide();
                },
                async () =>
                {
                    var runtime = _runtime;
                    _runtime = null;
                    if (runtime is null)
                    {
                        return;
                    }

                    runtime.UsageUpdated -= HandleUsageUpdated;
                    runtime.UsageUpdateFailed -= HandleUsageUpdateFailed;
                    await runtime.DisposeAsync();
                },
                () =>
                {
                    Dispatcher.UIThread.UnhandledException -=
                        HandleDispatcherUnhandledException;
                    desktop.Shutdown();
                    return ValueTask.CompletedTask;
                },
                backgroundDrainTimeout);
        }
        catch (Exception exception)
        {
            CompanionLog.Shared.Write("shutdown", exception);
        }
    }

    private static UiText ResolveText(CompanionSettings settings)
    {
        var language = UiLanguageResolver.Resolve(
            settings.Language,
            System.Globalization.CultureInfo.CurrentUICulture);
        return UiText.For(
            language,
            settings.ResetDateTimeFormat,
            settings.LastUpdatedDateTimeFormat);
    }

    private void ApplyTheme(string theme)
    {
        RequestedThemeVariant = UiThemeOptions.Normalize(theme) switch
        {
            UiThemeOptions.Light => ThemeVariant.Light,
            UiThemeOptions.System => ThemeVariant.Default,
            _ => ThemeVariant.Dark
        };
    }
}
