using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using CodexUsageCompanion.Configuration;
using CodexUsageCompanion.Localization;

namespace CodexUsageCompanion.Ui;

public sealed class SettingsWindow : Window
{
    private readonly CompanionSettings _settings;
    private readonly UiText _text;
    private readonly CheckBox _systemTray;
    private readonly ComboBox _trayIconStyle;
    private readonly CheckBox _showTaskbarIcon;
    private readonly CheckBox _startOnBoot;
    private readonly CheckBox _minimizeOnStart;
    private readonly CheckBox _alwaysOnTop;
    private readonly CheckBox _enableClaudeUsage;
    private readonly CheckBox _enableCodexUsage;
    private readonly CheckBox _lowUsageAlert;
    private readonly NumericUpDown _lowUsageAlertThreshold;
    private readonly CheckBox _notifyOnReset;
    private readonly CheckBox _usageLogging;
    private readonly ComboBox _language;
    private readonly ComboBox _theme;
    private readonly ComboBox _position;
    private readonly ComboBox _updateInterval;
    private readonly ComboBox _resetDateTimeFormat;
    private readonly ComboBox _lastUpdatedDateTimeFormat;
    private readonly ComboBox _usageLogFormat;
    private readonly TextBox _usageLogFilePath;
    private readonly TextBlock _resetDateTimeFormatStatus;
    private readonly TextBlock _lastUpdatedDateTimeFormatStatus;
    private readonly Button _save;
    private readonly Button _apply;
    private CompanionSettings _initialSettings = null!;
    private bool _allowClose;
    private bool _discardPromptOpen;

    public SettingsWindow(CompanionSettings settings, UiText text)
    {
        _settings = settings;
        _text = text;
        Title = text.SettingsTitle;
        Width = 520;
        Height = 720;
        MinWidth = 420;
        MinHeight = 480;
        CanResize = true;
        ShowInTaskbar = settings.ShowTaskbarIcon;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        _systemTray = new CheckBox
        {
            Content = text.SystemTrayOption,
            IsChecked = settings.EnableSystemTray
        };
        var trayIconStyles = new[]
        {
            new TrayIconStyleChoice(
                TrayIconStyleOptions.Original,
                text.OriginalTrayIconStyle),
            new TrayIconStyleChoice(
                TrayIconStyleOptions.ClaudeCurrentSession,
                text.ClaudeCurrentTrayIconStyle),
            new TrayIconStyleChoice(
                TrayIconStyleOptions.ClaudeWeeklySession,
                text.ClaudeWeeklyTrayIconStyle),
            new TrayIconStyleChoice(
                TrayIconStyleOptions.CodexSession,
                text.CodexSessionTrayIconStyle)
        };
        var selectedTrayIconStyle = TrayIconStyleOptions.Normalize(
            settings.TrayIconStyle);
        _trayIconStyle = new ComboBox
        {
            ItemsSource = trayIconStyles,
            SelectedItem = trayIconStyles.First(choice =>
                choice.Value == selectedTrayIconStyle),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        _showTaskbarIcon = new CheckBox
        {
            Content = text.ShowTaskbarIconOption,
            IsChecked = settings.ShowTaskbarIcon
        };
        _startOnBoot = new CheckBox
        {
            Content = text.StartOnBootOption,
            IsChecked = settings.StartOnBoot
        };
        _minimizeOnStart = new CheckBox
        {
            Content = text.MinimizeOnStartOption,
            IsChecked = settings.MinimizeOnStart
        };
        _startOnBoot.IsCheckedChanged += (_, _) => UpdateMinimizeOnStartControls();
        _alwaysOnTop = new CheckBox
        {
            Content = text.AlwaysOnTopOption,
            IsChecked = settings.AlwaysOnTop
        };
        _enableClaudeUsage = new CheckBox
        {
            Content = text.EnableClaudeUsageOption,
            IsChecked = settings.EnableClaudeUsage
        };
        _enableCodexUsage = new CheckBox
        {
            Content = text.EnableCodexUsageOption,
            IsChecked = settings.EnableCodexUsage
        };
        _lowUsageAlert = new CheckBox
        {
            Content = text.LowUsageAlertOption,
            IsChecked = settings.EnableLowUsageAlert
        };
        _lowUsageAlertThreshold = new NumericUpDown
        {
            Value = UsageAlertOptions.NormalizeThreshold(
                settings.LowUsageAlertThresholdPercent),
            Minimum = UsageAlertOptions.MinimumThresholdPercent,
            Maximum = UsageAlertOptions.MaximumThresholdPercent,
            Increment = 1,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        _notifyOnReset = new CheckBox
        {
            Content = text.NotifyOnResetOption,
            IsChecked = settings.NotifyOnReset
        };
        _lowUsageAlert.IsCheckedChanged += (_, _) =>
            UpdateLowUsageAlertControls();

        var languages = new[]
        {
            new LanguageChoice("en-US", text.EnglishLanguage),
            new LanguageChoice("zh-tw", text.TraditionalChineseLanguage),
            new LanguageChoice("zh-cn", text.SimplifiedChineseLanguage)
        };
        var selectedLanguage = settings.Language?.ToLowerInvariant() switch
        {
            "zh-tw" => "zh-tw",
            "zh-cn" => "zh-cn",
            "auto" when text.Language == UiLanguage.TraditionalChinese => "zh-tw",
            "auto" when text.Language == UiLanguage.SimplifiedChinese => "zh-cn",
            _ => "en-US"
        };
        _language = new ComboBox
        {
            ItemsSource = languages,
            SelectedItem = languages.First(choice => choice.Value == selectedLanguage),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        var themes = new[]
        {
            new ThemeChoice(UiThemeOptions.Dark, text.DarkTheme),
            new ThemeChoice(UiThemeOptions.Light, text.LightTheme),
            new ThemeChoice(UiThemeOptions.System, text.SystemTheme)
        };
        var selectedTheme = UiThemeOptions.Normalize(settings.Theme);
        _theme = new ComboBox
        {
            ItemsSource = themes,
            SelectedItem = themes.First(choice => choice.Value == selectedTheme),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        _position = new ComboBox
        {
            ItemsSource = WindowPosition.Values,
            SelectedItem = WindowPosition.Normalize(settings.Position),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        _position.SelectionChanged += (_, _) =>
        {
            if (_position.SelectedItem is string position)
            {
                PositionPreviewRequested?.Invoke(position);
            }
        };
        var updateIntervals = UpdateIntervalOptions.CommonValues
            .Append(UpdateIntervalOptions.Normalize(settings.RefreshIntervalSeconds))
            .Distinct()
            .Order()
            .Select(seconds => new UpdateIntervalChoice(
                seconds,
                text.FormatUpdateInterval(seconds)))
            .ToArray();
        var selectedUpdateInterval = UpdateIntervalOptions.Normalize(
            settings.RefreshIntervalSeconds);
        _updateInterval = new ComboBox
        {
            ItemsSource = updateIntervals,
            SelectedItem = updateIntervals.First(choice =>
                choice.Seconds == selectedUpdateInterval),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        var resetDateTimeFormat = DateTimeFormatOptions.NormalizeReset(
            settings.ResetDateTimeFormat);
        var resetDateTimeFormats = DateTimeFormatOptions.ResetFormats
            .Append(resetDateTimeFormat)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        _resetDateTimeFormat = new ComboBox
        {
            ItemsSource = resetDateTimeFormats,
            SelectedItem = resetDateTimeFormat,
            Text = resetDateTimeFormat,
            IsEditable = true,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        var lastUpdatedDateTimeFormat = DateTimeFormatOptions.NormalizeLastUpdated(
            settings.LastUpdatedDateTimeFormat);
        var lastUpdatedDateTimeFormats = DateTimeFormatOptions.LastUpdatedFormats
            .Append(lastUpdatedDateTimeFormat)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        _lastUpdatedDateTimeFormat = new ComboBox
        {
            ItemsSource = lastUpdatedDateTimeFormats,
            SelectedItem = lastUpdatedDateTimeFormat,
            Text = lastUpdatedDateTimeFormat,
            IsEditable = true,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        _usageLogging = new CheckBox
        {
            Content = text.UsageLoggingOption,
            IsChecked = settings.EnableUsageLogging
        };
        _usageLogFilePath = new TextBox
        {
            Text = UsageLogOptions.NormalizeFilePath(
                settings.UsageLogFilePath,
                settings.UsageLogFormat),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        _usageLogFormat = new ComboBox
        {
            ItemsSource = UsageLogOptions.Formats,
            SelectedItem = UsageLogOptions.NormalizeFormat(settings.UsageLogFormat),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        _usageLogFormat.SelectionChanged += (_, _) =>
        {
            if (_usageLogFormat.SelectedItem is not string newFormat)
            {
                return;
            }

            _usageLogFilePath.Text = UsageLogOptions.ChangeFileExtension(
                _usageLogFilePath.Text,
                newFormat);
        };
        _usageLogging.IsCheckedChanged += (_, _) => UpdateUsageLoggingControls();

        var languageLabel = new TextBlock
        {
            Text = text.LanguageOption,
            Margin = new Thickness(0, 8, 0, 4)
        };
        var trayIconStyleLabel = new TextBlock
        {
            Text = text.TrayIconStyleOption,
            Margin = new Thickness(0, 8, 0, 4)
        };
        var positionLabel = new TextBlock
        {
            Text = text.PositionOption,
            Margin = new Thickness(0, 8, 0, 4)
        };
        var themeLabel = new TextBlock
        {
            Text = text.ThemeOption,
            Margin = new Thickness(0, 8, 0, 4)
        };
        var usageLogFilePathLabel = new TextBlock
        {
            Text = text.UsageLogFilePathOption,
            Margin = new Thickness(0, 8, 0, 4)
        };
        var usageLogFormatLabel = new TextBlock
        {
            Text = text.UsageLogFormatOption,
            Margin = new Thickness(0, 8, 0, 4)
        };
        var updateIntervalLabel = new TextBlock
        {
            Text = text.UpdateIntervalOption,
            Margin = new Thickness(0, 8, 0, 4)
        };
        var lowUsageAlertThresholdLabel = new TextBlock
        {
            Text = text.LowUsageAlertThresholdOption,
            Margin = new Thickness(0, 8, 0, 4)
        };
        var resetDateTimeFormatLabel = new TextBlock
        {
            Text = text.ResetDateTimeFormatOption,
            Margin = new Thickness(0, 8, 0, 4)
        };
        var lastUpdatedDateTimeFormatLabel = new TextBlock
        {
            Text = text.LastUpdatedDateTimeFormatOption,
            Margin = new Thickness(0, 8, 0, 4)
        };
        _resetDateTimeFormatStatus = new TextBlock
        {
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap
        };
        _lastUpdatedDateTimeFormatStatus = new TextBlock
        {
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap
        };
        var appVersion = typeof(SettingsWindow).Assembly.GetName().Version?.ToString(3) ?? "0.0.0";
        var appVersionInfo = new TextBlock
        {
            Text = $"Claude Codex Usage Companion v{appVersion}",
            Opacity = 0.7,
            TextWrapping = TextWrapping.Wrap
        };
        var authorInfo = new TextBlock
        {
            Text = text.AuthorInfo,
            Opacity = 0.7,
            TextWrapping = TextWrapping.Wrap
        };
        var attributionInfo = new StackPanel
        {
            Margin = new Thickness(0, 12),
            Children = { appVersionInfo, authorInfo }
        };
        _save = new Button
        {
            Content = text.OkAction,
            MinWidth = 88,
            HorizontalContentAlignment = HorizontalAlignment.Center
        };
        _save.Click += (_, _) => SaveAndClose();
        var cancel = new Button
        {
            Content = text.CancelAction,
            MinWidth = 88,
            HorizontalContentAlignment = HorizontalAlignment.Center
        };
        cancel.Click += (_, _) => Close(null);
        _apply = new Button
        {
            Content = text.ApplyAction,
            MinWidth = 88,
            HorizontalContentAlignment = HorizontalAlignment.Center
        };
        _apply.Click += (_, _) => ApplyWithoutClosing();

        var actions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8,
            Children = { _save, cancel, _apply }
        };
        var options = new StackPanel
        {
            Spacing = 12,
            Margin = new Thickness(0, 0, 18, 0),
            Children =
            {
                CreateSectionHeader(text.WindowSettingsGroup, first: true),
                _systemTray,
                trayIconStyleLabel,
                _trayIconStyle,
                _showTaskbarIcon,
                _startOnBoot,
                _minimizeOnStart,
                _alwaysOnTop,
                CreateSeparator(),
                CreateSectionHeader(text.UsageSettingsGroup),
                _enableClaudeUsage,
                _enableCodexUsage,
                updateIntervalLabel,
                _updateInterval,
                CreateSeparator(),
                CreateSectionHeader(text.AppearanceSettingsGroup),
                languageLabel,
                _language,
                themeLabel,
                _theme,
                positionLabel,
                _position,
                CreateSeparator(),
                CreateSectionHeader(text.NotificationSettingsGroup),
                _lowUsageAlert,
                lowUsageAlertThresholdLabel,
                _lowUsageAlertThreshold,
                _notifyOnReset,
                CreateSeparator(),
                CreateSectionHeader(text.DateTimeSettingsGroup),
                resetDateTimeFormatLabel,
                _resetDateTimeFormat,
                _resetDateTimeFormatStatus,
                lastUpdatedDateTimeFormatLabel,
                _lastUpdatedDateTimeFormat,
                _lastUpdatedDateTimeFormatStatus,
                CreateSeparator(),
                CreateSectionHeader(text.LoggingSettingsGroup),
                _usageLogging,
                usageLogFilePathLabel,
                _usageLogFilePath,
                usageLogFormatLabel,
                _usageLogFormat
            }
        };
        var optionsScroller = new ScrollViewer
        {
            Content = options,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };
        var layout = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto,Auto"),
            Margin = new Thickness(24)
        };
        Grid.SetRow(attributionInfo, 1);
        Grid.SetRow(actions, 2);
        layout.Children.Add(optionsScroller);
        layout.Children.Add(attributionInfo);
        layout.Children.Add(actions);
        Content = layout;
        _resetDateTimeFormat.PropertyChanged += HandleDateTimeFormatChanged;
        _lastUpdatedDateTimeFormat.PropertyChanged += HandleDateTimeFormatChanged;
        _resetDateTimeFormat.SelectionChanged += HandleDateTimeFormatSelectionChanged;
        _lastUpdatedDateTimeFormat.SelectionChanged += HandleDateTimeFormatSelectionChanged;
        UpdateUsageLoggingControls();
        UpdateLowUsageAlertControls();
        UpdateMinimizeOnStartControls();
        UpdateDateTimeFormatValidation();
        _initialSettings = CaptureSettings();
        AddHandler(KeyDownEvent, HandleKeyDown, RoutingStrategies.Tunnel);
        Closing += HandleClosing;
    }

    public event Action<string>? PositionPreviewRequested;

    public event Func<CompanionSettings, bool>? ApplyRequested;

    private void HandleKeyDown(object? sender, KeyEventArgs eventArgs)
    {
        if (eventArgs.Key == Key.Escape)
        {
            eventArgs.Handled = true;
            Close(null);
            return;
        }

        if (eventArgs.Key != Key.S ||
            (eventArgs.KeyModifiers & KeyModifiers.Control) == 0)
        {
            return;
        }

        eventArgs.Handled = true;
        SaveAndClose();
    }

    private void HandleClosing(object? sender, WindowClosingEventArgs eventArgs)
    {
        if (_allowClose ||
            eventArgs.CloseReason == WindowCloseReason.OSShutdown ||
            CaptureSettings() == _initialSettings)
        {
            return;
        }

        eventArgs.Cancel = true;
        if (!_discardPromptOpen)
        {
            _ = ConfirmDiscardChangesAsync();
        }
    }

    private async Task ConfirmDiscardChangesAsync()
    {
        _discardPromptOpen = true;
        try
        {
            var dialog = CreateUnsavedChangesDialog();
            if (!await dialog.ShowDialog<bool>(this))
            {
                return;
            }

            _allowClose = true;
            Close(null);
        }
        finally
        {
            _discardPromptOpen = false;
        }
    }

    private Window CreateUnsavedChangesDialog()
    {
        var dialog = new Window
        {
            Title = _text.UnsavedChangesTitle,
            Width = 420,
            Height = 180,
            CanResize = false,
            ShowInTaskbar = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        var message = new TextBlock
        {
            Text = _text.UnsavedChangesMessage,
            TextWrapping = TextWrapping.Wrap
        };
        var discard = new Button
        {
            Content = _text.DiscardChangesAction,
            MinWidth = 112,
            HorizontalContentAlignment = HorizontalAlignment.Center
        };
        discard.Click += (_, _) => dialog.Close(true);
        var keepEditing = new Button
        {
            Content = _text.KeepEditingAction,
            MinWidth = 112,
            HorizontalContentAlignment = HorizontalAlignment.Center
        };
        keepEditing.Click += (_, _) => dialog.Close(false);
        var actions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8,
            Children = { discard, keepEditing }
        };
        var layout = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto"),
            Margin = new Thickness(24)
        };
        Grid.SetRow(actions, 1);
        layout.Children.Add(message);
        layout.Children.Add(actions);
        dialog.Content = layout;
        dialog.KeyDown += (_, eventArgs) =>
        {
            if (eventArgs.Key != Key.Escape)
            {
                return;
            }

            eventArgs.Handled = true;
            dialog.Close(false);
        };
        return dialog;
    }

    private void SaveAndClose()
    {
        if (!UpdateDateTimeFormatValidation())
        {
            return;
        }

        _allowClose = true;
        Close(CaptureSettings());
    }

    private void ApplyWithoutClosing()
    {
        if (!UpdateDateTimeFormatValidation())
        {
            return;
        }

        var settings = CaptureSettings();
        if (ApplyRequested?.Invoke(settings) != true)
        {
            return;
        }

        _initialSettings = settings;
    }

    private CompanionSettings CaptureSettings()
    {
        return _settings with
        {
            EnableSystemTray = _systemTray.IsChecked == true,
            TrayIconStyle =
                (_trayIconStyle.SelectedItem as TrayIconStyleChoice)?.Value ??
                TrayIconStyleOptions.Original,
            ShowTaskbarIcon = _showTaskbarIcon.IsChecked == true,
            StartOnBoot = _startOnBoot.IsChecked == true,
            MinimizeOnStart = _minimizeOnStart.IsChecked == true,
            AlwaysOnTop = _alwaysOnTop.IsChecked == true,
            EnableClaudeUsage = _enableClaudeUsage.IsChecked == true,
            EnableCodexUsage = _enableCodexUsage.IsChecked == true,
            Language = (_language.SelectedItem as LanguageChoice)?.Value ?? "en-US",
            Theme = (_theme.SelectedItem as ThemeChoice)?.Value ?? UiThemeOptions.System,
            Position = _position.SelectedItem as string ?? WindowPosition.RightBottom,
            RefreshIntervalSeconds =
                (_updateInterval.SelectedItem as UpdateIntervalChoice)?.Seconds ??
                UpdateIntervalOptions.MinimumSeconds,
            EnableLowUsageAlert = _lowUsageAlert.IsChecked == true,
            LowUsageAlertThresholdPercent = (int)(
                _lowUsageAlertThreshold.Value ??
                UsageAlertOptions.DefaultThresholdPercent),
            NotifyOnReset = _notifyOnReset.IsChecked == true,
            ResetDateTimeFormat = CurrentFormat(_resetDateTimeFormat),
            LastUpdatedDateTimeFormat = CurrentFormat(_lastUpdatedDateTimeFormat),
            EnableUsageLogging = _usageLogging.IsChecked == true,
            UsageLogFilePath = _usageLogFilePath.Text ?? string.Empty,
            UsageLogFormat =
                _usageLogFormat.SelectedItem as string ?? UsageLogOptions.Csv
        };
    }

    private void UpdateUsageLoggingControls()
    {
        var enabled = _usageLogging.IsChecked == true;
        _usageLogFilePath.IsEnabled = enabled;
        _usageLogFormat.IsEnabled = enabled;
    }

    private void UpdateLowUsageAlertControls()
    {
        _lowUsageAlertThreshold.IsEnabled = _lowUsageAlert.IsChecked == true;
    }

    private void UpdateMinimizeOnStartControls()
    {
        _minimizeOnStart.IsVisible = _startOnBoot.IsChecked == true;
    }

    private void HandleDateTimeFormatChanged(
        object? sender,
        AvaloniaPropertyChangedEventArgs eventArgs)
    {
        if (eventArgs.Property == ComboBox.TextProperty)
        {
            UpdateDateTimeFormatValidation();
        }
    }

    private void HandleDateTimeFormatSelectionChanged(
        object? sender,
        SelectionChangedEventArgs eventArgs)
    {
        if (sender is ComboBox comboBox && comboBox.SelectedItem is string selected)
        {
            comboBox.Text = selected;
        }

        UpdateDateTimeFormatValidation();
    }

    private bool UpdateDateTimeFormatValidation()
    {
        var sample = new DateTimeOffset(
            2026,
            8,
            6,
            13,
            55,
            9,
            TimeSpan.Zero);
        var resetValid = UpdateDateTimeFormatStatus(
            CurrentFormat(_resetDateTimeFormat),
            _resetDateTimeFormatStatus,
            sample);
        var lastUpdatedValid = UpdateDateTimeFormatStatus(
            CurrentFormat(_lastUpdatedDateTimeFormat),
            _lastUpdatedDateTimeFormatStatus,
            sample);
        _save.IsEnabled = resetValid && lastUpdatedValid;
        _apply.IsEnabled = _save.IsEnabled;
        return _save.IsEnabled;
    }

    private bool UpdateDateTimeFormatStatus(
        string format,
        TextBlock status,
        DateTimeOffset sample)
    {
        if (_text.TryFormatDateTime(sample, format, out var preview))
        {
            status.Text = $"{_text.FormatPreview}: {preview}";
            status.Foreground = Brushes.Gray;
            return true;
        }

        status.Text = _text.InvalidDateTimeFormat;
        status.Foreground = Brushes.IndianRed;
        return false;
    }

    private static string CurrentFormat(ComboBox comboBox)
    {
        return (comboBox.Text ?? comboBox.SelectedItem as string ?? string.Empty).Trim();
    }

    private static TextBlock CreateSectionHeader(string text, bool first = false)
    {
        return new TextBlock
        {
            Text = text,
            FontWeight = FontWeight.Bold,
            FontSize = 15,
            Margin = new Thickness(0, first ? 0 : 4, 0, 0)
        };
    }

    private static Border CreateSeparator()
    {
        return new Border
        {
            Height = 1,
            Background = Brushes.Gray,
            Opacity = 0.35,
            Margin = new Thickness(0, 4)
        };
    }

    private sealed record LanguageChoice(string Value, string DisplayName)
    {
        public override string ToString() => DisplayName;
    }

    private sealed record ThemeChoice(string Value, string DisplayName)
    {
        public override string ToString() => DisplayName;
    }

    private sealed record TrayIconStyleChoice(string Value, string DisplayName)
    {
        public override string ToString() => DisplayName;
    }

    private sealed record UpdateIntervalChoice(int Seconds, string DisplayName)
    {
        public override string ToString() => DisplayName;
    }
}
