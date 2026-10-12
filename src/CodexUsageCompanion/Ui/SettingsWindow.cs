using System.Reflection;
using CodexUsageCompanion.Lifecycle;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Automation;
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
    private readonly CheckBox _autoUpdateCheck;
    private readonly AppUpdateChecker _updateChecker = new();
    private readonly CancellationTokenSource _updateCancellation = new();
    private CompanionSettings _settings;
    private readonly UiText _text;
    private readonly CheckBox _systemTray;
    private readonly ComboBox _trayIconStyle;
    private readonly CheckBox _showTaskbarIcon;
    private readonly CheckBox _startOnBoot;
    private readonly CheckBox _minimizeOnStart;
    private readonly CheckBox _alwaysOnTop;
    private readonly CheckBox _enableClaudeUsage;
    private readonly CheckBox _enableCodexUsage;
    private readonly CheckBox _showClaudeSession;
    private readonly CheckBox _showClaudeWeekly;
    private readonly CheckBox _showCodexFiveHour;
    private readonly CheckBox _showCodexWeekly;
    private readonly CheckBox _lowUsageAlert;
    private readonly NumericUpDown _lowUsageAlertThreshold;
    private readonly CheckBox _notifyOnReset;
    private readonly CheckBox _usageLogging;
    private readonly ComboBox _language;
    private readonly ComboBox _theme;
    private readonly Slider _fontSizeSlider;
    private readonly TextBlock _fontSizeValue;
    private readonly TextBlock _fontSizePreview;
    private readonly TextBox _backgroundColor;
    private readonly TextBlock _backgroundColorStatus;
    private readonly NumericUpDown _percentageTimeSpacing;
    private readonly NumericUpDown _labelPercentageSpacing;
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
    private bool _resettingDefaults;
    private bool _resetPromptOpen;
    private readonly Dictionary<string, Expander> _sections = [];

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
        _showClaudeSession = new CheckBox
        {
            Content = text.ShowClaudeSessionOption,
            IsChecked = settings.ShowClaudeSession,
            Margin = new Thickness(16, 0, 0, 0)
        };
        _showClaudeWeekly = new CheckBox
        {
            Content = text.ShowClaudeWeeklyOption,
            IsChecked = settings.ShowClaudeWeekly,
            Margin = new Thickness(16, 0, 0, 0)
        };
        _showCodexFiveHour = new CheckBox
        {
            Content = text.ShowCodexFiveHourOption,
            IsChecked = settings.ShowCodexFiveHour,
            Margin = new Thickness(16, 0, 0, 0)
        };
        _showCodexWeekly = new CheckBox
        {
            Content = text.ShowCodexWeeklyOption,
            IsChecked = settings.ShowCodexWeekly,
            Margin = new Thickness(16, 0, 0, 0)
        };
        _enableClaudeUsage.IsCheckedChanged += (_, _) =>
            UpdateDisplayedLimitControls();
        _enableCodexUsage.IsCheckedChanged += (_, _) =>
            UpdateDisplayedLimitControls();
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
        _fontSizeSlider = new Slider
        {
            Minimum = OverlayLayoutOptions.MinimumFontScale * 100,
            Maximum = OverlayLayoutOptions.MaximumFontScale * 100,
            Value = OverlayLayoutOptions.NormalizeFontScale(settings.FontScale) * 100,
            TickFrequency = 5,
            IsSnapToTickEnabled = true,
            SmallChange = 5,
            LargeChange = 10,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        AutomationProperties.SetName(_fontSizeSlider, text.FontSizeOption);
        _fontSizeValue = new TextBlock
        {
            Width = 48,
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };
        _fontSizePreview = new TextBlock
        {
            Text = text.FontSizePreview,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(8)
        };
        _fontSizeSlider.ValueChanged += (_, _) => UpdateFontSizePreview();
        UpdateFontSizePreview();
        _backgroundColor = new TextBox
        {
            Text = settings.BackgroundColor,
            PlaceholderText = text.ThemeDefaultColor,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        _backgroundColorStatus = new TextBlock { FontSize = 12, TextWrapping = TextWrapping.Wrap };
        _percentageTimeSpacing = new NumericUpDown
        {
            Value = OverlayAppearanceOptions.NormalizeSpacing(settings.PercentageTimeSpacing),
            Minimum = 0,
            Maximum = OverlayAppearanceOptions.MaximumSpacing,
            Increment = 1,
            FormatString = "0",
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        _labelPercentageSpacing = new NumericUpDown
        {
            Value = OverlayAppearanceOptions.NormalizeLabelPercentageSpacing(settings.LabelPercentageSpacing),
            Minimum = 0,
            Maximum = 300,
            Increment = 1,
            FormatString = "0",
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
            if (!_resettingDefaults && _position.SelectedItem is string position)
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
        var fontSizeLabel = new TextBlock
        {
            Text = text.FontSizeOption,
            Margin = new Thickness(0, 8, 0, 4)
        };
        var fontSizeControls = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto")
        };
        Grid.SetColumn(_fontSizeValue, 1);
        fontSizeControls.Children.Add(_fontSizeSlider);
        fontSizeControls.Children.Add(_fontSizeValue);
        var fontSizePreviewBorder = new Border
        {
            BorderBrush = Brushes.Gray,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Child = _fontSizePreview
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
        var displayedLimitsLabel = new TextBlock
        {
            Text = text.DisplayedLimitsOption,
            Margin = new Thickness(0, 8, 0, 4),
            FontWeight = FontWeight.SemiBold
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
        _autoUpdateCheck = new CheckBox
        {
            Content = text.AutoUpdateCheckOption,
            IsChecked = settings.EnableAutoUpdateCheck
        };
        var checkUpdate = new Button { Content = text.CheckForUpdatesAction };
        var updateStatus = new TextBlock { TextWrapping = TextWrapping.Wrap };
        var openRelease = new Button { Content = text.OpenReleaseAction, IsVisible = false };
        Uri? releaseUrl = null;
        openRelease.Click += async (_, _) =>
        {
            if (releaseUrl is null) return;
            try
            {
                if (TopLevel.GetTopLevel(this) is { } topLevel)
                    await topLevel.Launcher.LaunchUriAsync(releaseUrl);
            }
            catch (Exception)
            {
                updateStatus.Text = text.UpdateCheckFailed;
            }
        };
        checkUpdate.Click += async (_, _) =>
        {
            checkUpdate.IsEnabled = false;
            openRelease.IsVisible = false;
            updateStatus.Text = text.CheckingForUpdates;
            try
            {
                var result = await _updateChecker.CheckAsync(_updateCancellation.Token);
                updateStatus.Text = result.UpdateAvailable ? text.FormatAppUpdate(result.Version) : text.AppUpToDate;
                releaseUrl = result.ReleaseUrl;
                openRelease.IsVisible = result.UpdateAvailable;
            }
            catch (OperationCanceledException) when (_updateCancellation.IsCancellationRequested) { }
            catch (Exception)
            {
                updateStatus.Text = text.UpdateCheckFailed;
            }
            finally { checkUpdate.IsEnabled = true; }
        };
        Closed += (_, _) => _updateCancellation.Cancel();
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
            Spacing = 8,
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
            Children =
            {
                CreateSection("usage", text.UsageSettingsGroup,
                _enableClaudeUsage,
                _enableCodexUsage,
                displayedLimitsLabel,
                _showClaudeSession,
                _showClaudeWeekly,
                _showCodexFiveHour,
                _showCodexWeekly,
                updateIntervalLabel,
                _updateInterval
                ),
                CreateSection("appearance", text.AppearanceSettingsGroup,
                languageLabel,
                _language,
                themeLabel,
                _theme,
                fontSizeLabel, fontSizeControls, fontSizePreviewBorder,
                new TextBlock { Text = text.BackgroundColorOption, Margin = new Thickness(0, 8, 0, 4) },
                _backgroundColor,
                _backgroundColorStatus,
                new TextBlock { Text = text.LabelPercentageSpacingOption, Margin = new Thickness(0, 8, 0, 4) },
                _labelPercentageSpacing,
                new TextBlock { Text = text.PercentageTimeSpacingOption, Margin = new Thickness(0, 8, 0, 4) },
                _percentageTimeSpacing,
                positionLabel,
                _position
                ),
                CreateSection("window", text.WindowSettingsGroup,
                _systemTray,
                trayIconStyleLabel,
                _trayIconStyle,
                _showTaskbarIcon,
                _startOnBoot,
                _minimizeOnStart,
                _alwaysOnTop
                ),
                CreateSection("notifications", text.NotificationSettingsGroup,
                _lowUsageAlert,
                lowUsageAlertThresholdLabel,
                _lowUsageAlertThreshold,
                _notifyOnReset
                ),
                CreateSection("dateTime", text.DateTimeSettingsGroup,
                resetDateTimeFormatLabel,
                _resetDateTimeFormat,
                _resetDateTimeFormatStatus,
                lastUpdatedDateTimeFormatLabel,
                _lastUpdatedDateTimeFormat,
                _lastUpdatedDateTimeFormatStatus
                ),
                CreateSection("logging", text.LoggingSettingsGroup,
                _usageLogging,
                usageLogFilePathLabel,
                _usageLogFilePath,
                usageLogFormatLabel,
                _usageLogFormat),
                CreateSection("updates", text.UpdateSettingsGroup,
                _autoUpdateCheck,
                checkUpdate,
                updateStatus,
                openRelease
                )
            }
        };
        var expandAll = CreateSectionActionButton(text.ExpandAllAction,
            "M3,2 L8,2 Q9,2 9,3 Q9,4 8,4 L5.4,4 L10.7,9.3 Q11.4,10 10.7,10.7 Q10,11.4 9.3,10.7 L4,5.4 L4,8 Q4,9 3,9 Q2,9 2,8 L2,3 Q2,2 3,2 Z M22,3 L22,8 Q22,9 21,9 Q20,9 20,8 L20,5.4 L14.7,10.7 Q14,11.4 13.3,10.7 Q12.6,10 13.3,9.3 L18.6,4 L16,4 Q15,4 15,3 Q15,2 16,2 L21,2 Q22,2 22,3 Z M21,22 L16,22 Q15,22 15,21 Q15,20 16,20 L18.6,20 L13.3,14.7 Q12.6,14 13.3,13.3 Q14,12.6 14.7,13.3 L20,18.6 L20,16 Q20,15 21,15 Q22,15 22,16 L22,21 Q22,22 21,22 Z M2,21 L2,16 Q2,15 3,15 Q4,15 4,16 L4,18.6 L9.3,13.3 Q10,12.6 10.7,13.3 Q11.4,14 10.7,14.7 L5.4,20 L8,20 Q9,20 9,21 Q9,22 8,22 L3,22 Q2,22 2,21 Z");
        var collapseAll = CreateSectionActionButton(text.CollapseAllAction,
            "M9,10 L4,10 Q3,10 3,9 Q3,8 4,8 L6.6,8 L1.3,2.7 Q0.6,2 1.3,1.3 Q2,0.6 2.7,1.3 L8,6.6 L8,4 Q8,3 9,3 Q10,3 10,4 L10,9 Q10,10 9,10 Z M14,9 L14,4 Q14,3 15,3 Q16,3 16,4 L16,6.6 L21.3,1.3 Q22,0.6 22.7,1.3 Q23.4,2 22.7,2.7 L17.4,8 L20,8 Q21,8 21,9 Q21,10 20,10 L15,10 Q14,10 14,9 Z M15,14 L20,14 Q21,14 21,15 Q21,16 20,16 L17.4,16 L22.7,21.3 Q23.4,22 22.7,22.7 Q22,23.4 21.3,22.7 L16,17.4 L16,20 Q16,21 15,21 Q14,21 14,20 L14,15 Q14,14 15,14 Z M10,15 L10,20 Q10,21 9,21 Q8,21 8,20 L8,17.4 L2.7,22.7 Q2,23.4 1.3,22.7 Q0.6,22 1.3,21.3 L6.6,16 L4,16 Q3,16 3,15 Q3,14 4,14 L9,14 Q10,14 10,15 Z");
        expandAll.Click += (_, _) => SetAllSectionsExpanded(true);
        collapseAll.Click += (_, _) => SetAllSectionsExpanded(false);
        var resetDefaults = new Button { Content = text.ResetDefaultsAction };
        resetDefaults.Click += async (_, _) => await ConfirmResetDefaultsAsync();
        var bulkActions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8,
            Children = { expandAll, collapseAll }
        };
        var sectionActions = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            Margin = new Thickness(0, 0, 0, 12),
            Children = { resetDefaults, bulkActions }
        };
        resetDefaults.HorizontalAlignment = HorizontalAlignment.Left;
        Grid.SetColumn(bulkActions, 1);
        var optionsScroller = new ScrollViewer
        {
            Content = options,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };
        var layout = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*,Auto,Auto"),
            Margin = new Thickness(24)
        };
        Grid.SetRow(optionsScroller, 1);
        Grid.SetRow(attributionInfo, 2);
        Grid.SetRow(actions, 3);
        layout.Children.Add(sectionActions);
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
        UpdateDisplayedLimitControls();
        UpdateDateTimeFormatValidation();
        _initialSettings = CaptureSettings();
        _backgroundColor.TextChanged += (_, _) => PreviewAppearance();
        _percentageTimeSpacing.ValueChanged += (_, _) => PreviewAppearance();
        _labelPercentageSpacing.ValueChanged += (_, _) => PreviewAppearance();
        AddHandler(KeyDownEvent, HandleKeyDown, RoutingStrategies.Tunnel);
        Closing += HandleClosing;
    }

    public SettingsSectionState SectionState => new()
    {
        Appearance = _sections["appearance"].IsExpanded,
        DateTime = _sections["dateTime"].IsExpanded,
        Logging = _sections["logging"].IsExpanded,
        Notifications = _sections["notifications"].IsExpanded,
        Usage = _sections["usage"].IsExpanded,
        Window = _sections["window"].IsExpanded,
        Updates = _sections["updates"].IsExpanded
    };

    private void SetAllSectionsExpanded(bool expanded)
    {
        foreach (var section in _sections.Values)
            section.IsExpanded = expanded;
    }

    public event Action<string>? PositionPreviewRequested;
    public event Action<string, int, int>? AppearancePreviewRequested;

    public event Func<CompanionSettings, bool>? ApplyRequested;

    private async Task ConfirmResetDefaultsAsync()
    {
        if (_resetPromptOpen)
            return;
        _resetPromptOpen = true;
        try
        {
            var dialog = new Window
            {
                Title = _text.ConfirmResetDefaultsTitle,
                Width = 420,
                Height = 190,
                CanResize = false,
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            var yes = new Button
            {
                Content = _text.YesAction,
                MinWidth = 88,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            };
            var no = new Button
            {
                Content = _text.NoAction,
                MinWidth = 88,
                IsDefault = true,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            };
            yes.Click += (_, _) => dialog.Close(true);
            no.Click += (_, _) => dialog.Close(false);
            var actions = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 8,
                Children = { yes, no }
            };
            var layout = new Grid { RowDefinitions = new RowDefinitions("*,Auto"), Margin = new Thickness(24) };
            Grid.SetRow(actions, 1);
            layout.Children.Add(new TextBlock { Text = _text.ConfirmResetDefaultsMessage, TextWrapping = TextWrapping.Wrap });
            layout.Children.Add(actions);
            dialog.Content = layout;
            dialog.Opened += (_, _) => no.Focus();
            dialog.KeyDown += (_, args) =>
            {
                if (args.Key == Key.Escape)
                {
                    args.Handled = true;
                    dialog.Close(false);
                }
            };
            if (await dialog.ShowDialog<bool>(this))
                ResetToDefaults();
        }
        finally
        {
            _resetPromptOpen = false;
        }
    }

    private void ResetToDefaults()
    {
        var defaults = new CompanionSettings { SettingsSections = SectionState };
        _resettingDefaults = true;
        try
        {
            // Keep reset edits in the form until OK/Apply; Cancel still discards them.
            _settings = defaults;
            _systemTray.IsChecked = defaults.EnableSystemTray;
            _trayIconStyle.SelectedItem = _trayIconStyle.Items.OfType<TrayIconStyleChoice>().First(choice => choice.Value == defaults.TrayIconStyle);
            _showTaskbarIcon.IsChecked = defaults.ShowTaskbarIcon;
            _autoUpdateCheck.IsChecked = defaults.EnableAutoUpdateCheck;
            _startOnBoot.IsChecked = defaults.StartOnBoot;
            _minimizeOnStart.IsChecked = defaults.MinimizeOnStart;
            _alwaysOnTop.IsChecked = defaults.AlwaysOnTop;
            _enableClaudeUsage.IsChecked = defaults.EnableClaudeUsage;
            _enableCodexUsage.IsChecked = defaults.EnableCodexUsage;
            _showClaudeSession.IsChecked = defaults.ShowClaudeSession;
            _showClaudeWeekly.IsChecked = defaults.ShowClaudeWeekly;
            _showCodexFiveHour.IsChecked = defaults.ShowCodexFiveHour;
            _showCodexWeekly.IsChecked = defaults.ShowCodexWeekly;
            _language.SelectedItem = _language.Items.OfType<LanguageChoice>().First(choice => choice.Value == defaults.Language);
            _theme.SelectedItem = _theme.Items.OfType<ThemeChoice>().First(choice => choice.Value == defaults.Theme);
            _fontSizeSlider.Value = defaults.FontScale * 100;
            _backgroundColor.Text = defaults.BackgroundColor;
            _labelPercentageSpacing.Value = defaults.LabelPercentageSpacing;
            _percentageTimeSpacing.Value = defaults.PercentageTimeSpacing;
            _position.SelectedItem = defaults.Position;
            _updateInterval.SelectedItem = _updateInterval.Items.OfType<UpdateIntervalChoice>().First(choice => choice.Seconds == defaults.RefreshIntervalSeconds);
            _lowUsageAlert.IsChecked = defaults.EnableLowUsageAlert;
            _lowUsageAlertThreshold.Value = defaults.LowUsageAlertThresholdPercent;
            _notifyOnReset.IsChecked = defaults.NotifyOnReset;
            _resetDateTimeFormat.SelectedItem = defaults.ResetDateTimeFormat;
            _resetDateTimeFormat.Text = defaults.ResetDateTimeFormat;
            _lastUpdatedDateTimeFormat.SelectedItem = defaults.LastUpdatedDateTimeFormat;
            _lastUpdatedDateTimeFormat.Text = defaults.LastUpdatedDateTimeFormat;
            _usageLogging.IsChecked = defaults.EnableUsageLogging;
            _usageLogFormat.SelectedItem = defaults.UsageLogFormat;
            _usageLogFilePath.Text = defaults.UsageLogFilePath;
        }
        finally
        {
            _resettingDefaults = false;
        }
        UpdateMinimizeOnStartControls();
        UpdateDisplayedLimitControls();
        UpdateLowUsageAlertControls();
        UpdateUsageLoggingControls();
        UpdateFontSizePreview();
        PreviewAppearance();
        PositionPreviewRequested?.Invoke(defaults.Position);
    }

    private void UpdateFontSizePreview()
    {
        var scale = OverlayLayoutOptions.NormalizeFontScale(_fontSizeSlider.Value / 100);
        _fontSizeValue.Text = $"{scale * 100:0}%";
        _fontSizePreview.FontSize = 14 * scale;
    }

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
            EnableAutoUpdateCheck = _autoUpdateCheck.IsChecked == true,
            StartOnBoot = _startOnBoot.IsChecked == true,
            MinimizeOnStart = _minimizeOnStart.IsChecked == true,
            AlwaysOnTop = _alwaysOnTop.IsChecked == true,
            EnableClaudeUsage = _enableClaudeUsage.IsChecked == true,
            EnableCodexUsage = _enableCodexUsage.IsChecked == true,
            ShowClaudeSession = _showClaudeSession.IsChecked == true,
            ShowClaudeWeekly = _showClaudeWeekly.IsChecked == true,
            ShowCodexFiveHour = _showCodexFiveHour.IsChecked == true,
            ShowCodexWeekly = _showCodexWeekly.IsChecked == true,
            Language = (_language.SelectedItem as LanguageChoice)?.Value ?? "en-US",
            Theme = (_theme.SelectedItem as ThemeChoice)?.Value ?? UiThemeOptions.System,
            FontScale = OverlayLayoutOptions.NormalizeFontScale(_fontSizeSlider.Value / 100),
            BackgroundColor = OverlayAppearanceOptions.NormalizeBackgroundColor(_backgroundColor.Text),
            PercentageTimeSpacing = (int)(_percentageTimeSpacing.Value ?? OverlayAppearanceOptions.DefaultSpacing),
            LabelPercentageSpacing = (int)(_labelPercentageSpacing.Value ?? OverlayAppearanceOptions.DefaultLabelPercentageSpacing),
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

    private void UpdateDisplayedLimitControls()
    {
        var claudeEnabled = _enableClaudeUsage.IsChecked == true;
        _showClaudeSession.IsEnabled = claudeEnabled;
        _showClaudeWeekly.IsEnabled = claudeEnabled;

        var codexEnabled = _enableCodexUsage.IsChecked == true;
        _showCodexFiveHour.IsEnabled = codexEnabled;
        _showCodexWeekly.IsEnabled = codexEnabled;
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

    private void PreviewAppearance()
    {
        if (_resettingDefaults)
            return;
        UpdateDateTimeFormatValidation();
        if (OverlayAppearanceOptions.IsValidBackgroundColor(_backgroundColor.Text))
            AppearancePreviewRequested?.Invoke(_backgroundColor.Text ?? string.Empty,
                (int)(_percentageTimeSpacing.Value ?? OverlayAppearanceOptions.DefaultSpacing),
                (int)(_labelPercentageSpacing.Value ?? OverlayAppearanceOptions.DefaultLabelPercentageSpacing));
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
        var colorValid = OverlayAppearanceOptions.IsValidBackgroundColor(_backgroundColor.Text);
        _backgroundColorStatus.Text = colorValid ? string.Empty : _text.InvalidBackgroundColor;
        _backgroundColorStatus.Foreground = Brushes.IndianRed;
        _save.IsEnabled = resetValid && lastUpdatedValid && colorValid;
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

    private static Button CreateSectionActionButton(string label, string geometry)
    {
        var button = new Button
        {
            Content = new PathIcon { Width = 18, Height = 18, Data = Geometry.Parse(geometry) },
            Width = 32,
            Height = 32,
            Padding = new Thickness(0),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center
        };
        ToolTip.SetTip(button, label);
        AutomationProperties.SetName(button, label);
        return button;
    }

    private Expander CreateSection(string key, string title, params Control[] controls)
    {
        var content = new StackPanel
        {
            Spacing = 12,
            Margin = new Thickness(0, 8, 0, 8)
        };
        foreach (var control in controls)
            content.Children.Add(control);

        var section = new Expander
        {
            Header = new TextBlock
            {
                Text = title,
                FontWeight = FontWeight.Bold,
                FontSize = 15
            },
            IsExpanded = _settings.SettingsSections.IsExpanded(key),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Content = content
        };
        _sections.Add(key, section);
        return section;
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
