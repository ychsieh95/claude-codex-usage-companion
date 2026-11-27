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
    private readonly CheckBox _autoInstallUpdates;
    private readonly AppUpdateInstaller _updateInstaller = new();
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
    private readonly CheckBox _showPacePointer;
    private readonly CheckBox _lowUsageAlert;
    private readonly NumericUpDown _lowUsageAlertThreshold;
    private readonly CheckBox _notifyOnReset;
    private readonly CheckBox _usageLogging;
    private readonly ComboBox _language;
    private readonly ComboBox _theme;
    private readonly ComboBox _overlayDesign;
    private readonly TextBlock _previousDesignNote;
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
    private readonly CheckBox _resetTimeOnNameRow;
    private readonly ComboBox _headerBadge;
    private readonly ComboBox _badgeAverage;
    private readonly TextBlock _badgeAverageDescription;
    private readonly Dictionary<string, Control> _pages = [];
    private readonly ListBox _sidebar = new()
    {
        Background = Brushes.Transparent,
        SelectionMode = SelectionMode.Single | SelectionMode.AlwaysSelected
    };

    private const string OriginalProjectUrl = "https://github.com/gkfriend/codex-usage-companion";

    // Sidebar icons (24×24 outlines).
    private const string UsageIcon =
        "M12,16A3,3 0 0,1 9,13C9,11.88 9.61,10.9 10.5,10.39L20.21,4.77L14.68,14.35C14.18,15.33 13.17,16 12,16M12,3C13.81,3 15.5,3.5 16.97,4.32L14.87,5.53C14,5.19 13,5 12,5A8,8 0 0,0 4,13C4,15.21 4.89,17.21 6.34,18.65C6.73,19.04 6.73,19.67 6.34,20.06C5.95,20.45 5.32,20.45 4.93,20.07C3.12,18.26 2,15.76 2,13A10,10 0 0,1 12,3M22,13C22,15.76 20.88,18.26 19.07,20.07C18.68,20.45 18.05,20.45 17.66,20.06C17.27,19.67 17.27,19.04 17.66,18.65C19.11,17.2 20,15.21 20,13C20,12 19.81,11 19.46,10.1L20.67,8C21.5,9.5 22,11.18 22,13Z";
    private const string AppearanceIcon =
        "M17.5,12A1.5,1.5 0 0,1 16,10.5A1.5,1.5 0 0,1 17.5,9A1.5,1.5 0 0,1 19,10.5A1.5,1.5 0 0,1 17.5,12M14.5,8A1.5,1.5 0 0,1 13,6.5A1.5,1.5 0 0,1 14.5,5A1.5,1.5 0 0,1 16,6.5A1.5,1.5 0 0,1 14.5,8M9.5,8A1.5,1.5 0 0,1 8,6.5A1.5,1.5 0 0,1 9.5,5A1.5,1.5 0 0,1 11,6.5A1.5,1.5 0 0,1 9.5,8M6.5,12A1.5,1.5 0 0,1 5,10.5A1.5,1.5 0 0,1 6.5,9A1.5,1.5 0 0,1 8,10.5A1.5,1.5 0 0,1 6.5,12M12,3A9,9 0 0,0 3,12A9,9 0 0,0 12,21A1.5,1.5 0 0,0 13.5,19.5C13.5,19.11 13.35,18.76 13.11,18.5C12.88,18.23 12.73,17.88 12.73,17.5A1.5,1.5 0 0,1 14.23,16H16A5,5 0 0,0 21,11C21,6.58 16.97,3 12,3Z";
    private const string WindowIcon =
        "M21,2H3C1.9,2 1,2.9 1,4V20C1,21.1 1.9,22 3,22H21C22.1,22 23,21.1 23,20V4C23,2.9 22.1,2 21,2M21,20H3V6H21V20Z";
    private const string NotificationsIcon =
        "M21,19V20H3V19L5,17V11C5,7.9 7.03,5.17 10,4.29C10,4.19 10,4.1 10,4A2,2 0 0,1 12,2A2,2 0 0,1 14,4C14,4.1 14,4.19 14,4.29C16.97,5.17 19,7.9 19,11V17L21,19M14,21A2,2 0 0,1 12,23A2,2 0 0,1 10,21H14Z";
    private const string DateTimeIcon =
        "M12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22C6.47,22 2,17.5 2,12A10,10 0 0,1 12,2M12.5,7V12.25L17,14.92L16.25,16.15L11,13V7H12.5Z";
    private const string LoggingIcon =
        "M5,3C3.89,3 3,3.89 3,5V19A2,2 0 0,0 5,21H19A2,2 0 0,0 21,19V5C21,3.89 20.1,3 19,3H5M5,5H19V19H5V5M7,7V9H17V7H7M7,11V13H17V11H7M7,15V17H14V15H7Z";
    private const string UpdatesIcon =
        "M5,20H19V18H5M19,9H15V3H9V9H5L12,16L19,9Z";

    public SettingsWindow(CompanionSettings settings, UiText text)
    {
        _settings = settings;
        _text = text;
        Title = text.SettingsTitle;
        Width = 860;
        Height = 600;
        MinWidth = 700;
        MinHeight = 420;
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
            IsChecked = settings.ShowClaudeSession
        };
        _showClaudeWeekly = new CheckBox
        {
            Content = text.ShowClaudeWeeklyOption,
            IsChecked = settings.ShowClaudeWeekly
        };
        _showCodexFiveHour = new CheckBox
        {
            Content = text.ShowCodexFiveHourOption,
            IsChecked = settings.ShowCodexFiveHour
        };
        _showCodexWeekly = new CheckBox
        {
            Content = text.ShowCodexWeeklyOption,
            IsChecked = settings.ShowCodexWeekly
        };
        _showPacePointer = new CheckBox
        {
            Content = text.ShowPacePointerOption,
            IsChecked = settings.ShowPacePointer
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
        var designs = new[]
        {
            new OptionChoice(OverlayDesignOptions.Latest, text.LatestOverlayDesign),
            new OptionChoice(OverlayDesignOptions.Previous, text.PreviousOverlayDesign),
            new OptionChoice(OverlayDesignOptions.Rings, text.RingsOverlayDesign)
        };
        var selectedDesign = OverlayDesignOptions.Normalize(settings.OverlayDesign);
        _overlayDesign = new ComboBox
        {
            ItemsSource = designs,
            SelectedItem = designs.First(choice => choice.Value == selectedDesign),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        _previousDesignNote = new TextBlock
        {
            Text = text.PreviousDesignOnlyNote,
            FontSize = 12,
            Opacity = 0.75,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 8, 0, 0)
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
        var badges = text.HeaderBadgeChoices
            .Select(choice => new OptionChoice(choice.Value, choice.Label))
            .ToArray();
        var selectedBadge = HeaderBadgeOptions.Normalize(settings.HeaderBadge);
        _headerBadge = new ComboBox
        {
            ItemsSource = badges,
            SelectedItem = badges.First(choice => choice.Value == selectedBadge),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        var averages = text.BadgeAverageChoices
            .Select(choice => new OptionChoice(choice.Value, choice.Label))
            .ToArray();
        var averageDescriptions = text.BadgeAverageChoices
            .ToDictionary(choice => choice.Value, choice => choice.Description);
        var selectedAverage = BadgeAverageOptions.Normalize(settings.BadgeAverage);
        _badgeAverage = new ComboBox
        {
            ItemsSource = averages,
            SelectedItem = averages.First(choice => choice.Value == selectedAverage),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        // The description follows the selected method.
        _badgeAverageDescription = Hint(averageDescriptions[selectedAverage]);
        _badgeAverage.SelectionChanged += (_, _) =>
        {
            if (_badgeAverage.SelectedItem is OptionChoice choice)
                _badgeAverageDescription.Text = averageDescriptions[choice.Value];
        };
        _resetTimeOnNameRow = new CheckBox
        {
            Content = text.ResetTimeOnNameRowOption,
            IsChecked = settings.ResetTimeOnNameRow
        };
        _autoUpdateCheck = new CheckBox
        {
            Content = text.AutoUpdateCheckOption,
            IsChecked = settings.EnableAutoUpdateCheck
        };
        var canSelfUpdate = InstallKindDetector.SupportsInstall(_updateInstaller.Kind);
        _autoInstallUpdates = new CheckBox
        {
            Content = text.AutoInstallUpdatesOption,
            IsChecked = canSelfUpdate && settings.EnableAutoInstallUpdates,
            IsEnabled = canSelfUpdate && settings.EnableAutoUpdateCheck
        };
        _autoUpdateCheck.IsCheckedChanged += (_, _) =>
            _autoInstallUpdates.IsEnabled = canSelfUpdate && _autoUpdateCheck.IsChecked == true;
        var installKindHint = Hint(text.InstallKindHint(_updateInstaller.Kind));
        var checkUpdate = new Button { Content = text.CheckForUpdatesAction };
        var updateStatus = new TextBlock { TextWrapping = TextWrapping.Wrap };
        var updateProgress = new ProgressBar { Minimum = 0, Maximum = 1, IsVisible = false };
        var openRelease = new Button { Content = text.OpenReleaseAction, IsVisible = false };
        var installUpdate = new Button { Content = text.InstallUpdateAction, IsVisible = false };
        // Checks for the latest release and installs it, also when it is the installed version.
        var reinstall = new Button { Content = text.ReinstallLatestAction, IsVisible = canSelfUpdate };
        Uri? releaseUrl = null;
        AppUpdateResult? availableUpdate = null;
        // Installs the checked release: an update, or the installed version again.
        async Task InstallCheckedReleaseAsync()
        {
            if (availableUpdate is null || InstallUpdateRequested is not { } install) return;
            checkUpdate.IsEnabled = false;
            installUpdate.IsEnabled = false;
            reinstall.IsEnabled = false;
            // Progress<T> posts each report back to the UI thread.
            var progress = new Progress<UpdateProgress>(report =>
            {
                updateStatus.Text = text.FormatUpdateProgress(report);
                updateProgress.IsVisible = true;
                // Only the download has a known share; the other stages show an indeterminate bar.
                updateProgress.IsIndeterminate = report.Fraction is null;
                updateProgress.Value = report.Fraction ?? 0;
            });
            try
            {
                var error = await install(availableUpdate, progress);
                if (error is not null) updateStatus.Text = error;
            }
            finally
            {
                updateProgress.IsVisible = false;
                checkUpdate.IsEnabled = true;
                installUpdate.IsEnabled = true;
                reinstall.IsEnabled = true;
            }
        }
        installUpdate.Click += async (_, _) => await InstallCheckedReleaseAsync();
        reinstall.Click += async (_, _) =>
        {
            checkUpdate.IsEnabled = false;
            reinstall.IsEnabled = false;
            openRelease.IsVisible = false;
            installUpdate.IsVisible = false;
            updateStatus.Text = text.CheckingForUpdates;
            AppUpdateResult result;
            try
            {
                result = await _updateChecker.CheckAsync(_updateCancellation.Token);
            }
            catch (OperationCanceledException) when (_updateCancellation.IsCancellationRequested) { return; }
            catch (Exception)
            {
                updateStatus.Text = text.UpdateCheckFailed;
                checkUpdate.IsEnabled = true;
                reinstall.IsEnabled = true;
                return;
            }
            releaseUrl = result.ReleaseUrl;
            availableUpdate = result;
            // A newer release is the latest version, so installing it is the reinstall.
            if (_updateInstaller.CanInstall(result) || _updateInstaller.CanReinstall(result))
            {
                await InstallCheckedReleaseAsync();
                return;
            }
            updateStatus.Text = text.ReinstallUnavailable;
            openRelease.IsVisible = true;
            checkUpdate.IsEnabled = true;
            reinstall.IsEnabled = true;
        };
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
            reinstall.IsEnabled = false;
            openRelease.IsVisible = false;
            installUpdate.IsVisible = false;
            updateStatus.Text = text.CheckingForUpdates;
            try
            {
                var result = await _updateChecker.CheckAsync(_updateCancellation.Token);
                updateStatus.Text = result.UpdateAvailable ? text.FormatAppUpdate(result.Version) : text.AppUpToDate;
                releaseUrl = result.ReleaseUrl;
                openRelease.IsVisible = result.UpdateAvailable;
                availableUpdate = result;
                installUpdate.IsVisible = _updateInstaller.CanInstall(result);
            }
            catch (OperationCanceledException) when (_updateCancellation.IsCancellationRequested) { }
            catch (Exception)
            {
                updateStatus.Text = text.UpdateCheckFailed;
            }
            finally
            {
                checkUpdate.IsEnabled = true;
                reinstall.IsEnabled = true;
            }
        };
        Closed += (_, _) => _updateCancellation.Cancel();
        var fontSizeControls = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            Children = { _fontSizeSlider, _fontSizeValue }
        };
        Grid.SetColumn(_fontSizeValue, 1);
        var fontSizePreviewBorder = new Border
        {
            BorderBrush = Brushes.Gray,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Child = _fontSizePreview
        };
        var appVersion = typeof(SettingsWindow).Assembly.GetName().Version?.ToString(3) ?? "0.0.0";

        AddPage(SettingsPageOptions.Usage, text.UsageSettingsGroup, UsageIcon,
            Row(text.ProvidersLabel, _enableClaudeUsage, _enableCodexUsage),
            Row(text.DisplayedLimitsOption, _showClaudeSession, _showClaudeWeekly,
                _showCodexFiveHour, _showCodexWeekly, _showPacePointer),
            Row(text.UpdateIntervalOption, _updateInterval));
        AddPage(SettingsPageOptions.Appearance, text.AppearanceSettingsGroup, AppearanceIcon,
            Row(text.LanguageOption, _language),
            Row(text.ThemeOption, _theme),
            Row(text.OverlayDesignOption, _overlayDesign),
            Row(text.LimitRowsLabel, _resetTimeOnNameRow),
            Row(text.HeaderBadgeOption, _headerBadge),
            Row(text.BadgeAverageOption, _badgeAverage, Hint(text.BadgeAverageHint), _badgeAverageDescription),
            Row(text.FontSizeOption, fontSizeControls, fontSizePreviewBorder),
            Row(text.BackgroundColorOption, _backgroundColor, Hint(text.BackgroundColorFormatHint), _backgroundColorStatus),
            Row(text.SpacingLabel,
                Hint(text.LabelPercentageSpacingOption, secondary: false), _labelPercentageSpacing,
                Hint(text.PercentageTimeSpacingOption, secondary: false), _percentageTimeSpacing,
                _previousDesignNote));
        AddPage(SettingsPageOptions.Window, text.WindowSettingsGroup, WindowIcon,
            Row(text.PositionOption, _position),
            Row(text.WindowLabel, _alwaysOnTop, _showTaskbarIcon),
            Row(text.SystemTrayLabel, _systemTray, Hint(text.TrayIconStyleOption, secondary: false), _trayIconStyle),
            Row(text.StartupLabel, _startOnBoot, _minimizeOnStart));
        AddPage(SettingsPageOptions.Notifications, text.NotificationSettingsGroup, NotificationsIcon,
            Row(text.AlertsLabel, _lowUsageAlert, _notifyOnReset),
            Row(text.LowUsageAlertThresholdOption, _lowUsageAlertThreshold));
        AddPage(SettingsPageOptions.DateTime, text.DateTimeSettingsGroup, DateTimeIcon,
            Row(text.ResetDateTimeFormatOption, _resetDateTimeFormat, _resetDateTimeFormatStatus),
            Row(text.LastUpdatedDateTimeFormatOption, _lastUpdatedDateTimeFormat, _lastUpdatedDateTimeFormatStatus));
        AddPage(SettingsPageOptions.Logging, text.LoggingSettingsGroup, LoggingIcon,
            Row(null, _usageLogging),
            Row(text.UsageLogFilePathOption, _usageLogFilePath),
            Row(text.UsageLogFormatOption, _usageLogFormat));
        AddPage(SettingsPageOptions.Updates, text.UpdateSettingsGroup, UpdatesIcon,
            Row(null, _autoUpdateCheck, _autoInstallUpdates, installKindHint),
            Row(null, new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children = { checkUpdate, installUpdate, reinstall, openRelease }
            }, updateProgress, updateStatus),
            Row(text.AboutLabel,
                new TextBlock { Text = $"Claude Codex Usage Companion v{appVersion}", TextWrapping = TextWrapping.Wrap },
                LinkLine(text.ProjectLinkLabel, AppUpdateChecker.RepositoryUrl),
                LinkLine(text.OriginalProjectLinkLabel, OriginalProjectUrl)));

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
        var resetDefaults = new Button { Content = text.ResetDefaultsAction };
        resetDefaults.Click += async (_, _) => await ConfirmResetDefaultsAsync();
        var actions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children = { _save, cancel, _apply }
        };
        var footer = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            Margin = new Thickness(16, 12),
            Children = { resetDefaults, actions }
        };
        Grid.SetColumn(actions, 1);

        // Every page stays in the tree; the sidebar only changes which one is visible.
        var pages = new Grid { Margin = new Thickness(28, 20, 28, 20) };
        foreach (var page in _pages.Values)
            pages.Children.Add(page);
        var pageScroller = new ScrollViewer
        {
            Content = pages,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };
        _sidebar.SelectionChanged += (_, _) => ShowSelectedPage();
        var sidebarHost = new Border
        {
            // The sidebar fits its longest page title, which differs by language and font.
            MinWidth = 170,
            Padding = new Thickness(8),
            Background = new SolidColorBrush(Color.FromArgb(0x10, 0x80, 0x80, 0x80)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(0x30, 0x80, 0x80, 0x80)),
            BorderThickness = new Thickness(0, 0, 1, 0),
            Child = _sidebar
        };
        var body = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*"),
            Children = { sidebarHost, pageScroller }
        };
        Grid.SetColumn(pageScroller, 1);
        var footerHost = new Border
        {
            BorderBrush = new SolidColorBrush(Color.FromArgb(0x30, 0x80, 0x80, 0x80)),
            BorderThickness = new Thickness(0, 1, 0, 0),
            Child = footer
        };
        var layout = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto"),
            Children = { body, footerHost }
        };
        Grid.SetRow(footerHost, 1);
        Content = layout;
        SelectedPage = settings.SettingsPage;
        _resetDateTimeFormat.PropertyChanged += HandleDateTimeFormatChanged;
        _lastUpdatedDateTimeFormat.PropertyChanged += HandleDateTimeFormatChanged;
        _resetDateTimeFormat.SelectionChanged += HandleDateTimeFormatSelectionChanged;
        _lastUpdatedDateTimeFormat.SelectionChanged += HandleDateTimeFormatSelectionChanged;
        UpdateUsageLoggingControls();
        UpdateLowUsageAlertControls();
        UpdateMinimizeOnStartControls();
        UpdateDisplayedLimitControls();
        UpdateDesignControls();
        _overlayDesign.SelectionChanged += (_, _) => UpdateDesignControls();
        UpdateDateTimeFormatValidation();
        _initialSettings = CaptureSettings();
        _backgroundColor.TextChanged += (_, _) => PreviewAppearance();
        _percentageTimeSpacing.ValueChanged += (_, _) => PreviewAppearance();
        _labelPercentageSpacing.ValueChanged += (_, _) => PreviewAppearance();
        AddHandler(KeyDownEvent, HandleKeyDown, RoutingStrategies.Tunnel);
        Closing += HandleClosing;
    }

    /// <summary>The page shown in the sidebar, as a <see cref="SettingsPageOptions"/> value.</summary>
    public string SelectedPage
    {
        get => (_sidebar.SelectedItem as ListBoxItem)?.Tag as string ?? SettingsPageOptions.Usage;
        set
        {
            var page = SettingsPageOptions.Normalize(value);
            _sidebar.SelectedItem = _sidebar.Items.OfType<ListBoxItem>().First(item => (string?)item.Tag == page);
            ShowSelectedPage();
        }
    }

    private void ShowSelectedPage()
    {
        var selected = SelectedPage;
        foreach (var (key, page) in _pages)
            page.IsVisible = key == selected;
    }

    public event Action<string>? PositionPreviewRequested;
    public event Action<string, int, int>? AppearancePreviewRequested;

    public event Func<CompanionSettings, bool>? ApplyRequested;

    /// <summary>Downloads and installs an update. Returns an error message, or null when the app restarts.</summary>
    public event Func<AppUpdateResult, IProgress<UpdateProgress>, Task<string?>>? InstallUpdateRequested;

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
        var defaults = new CompanionSettings { SettingsPage = SelectedPage };
        _resettingDefaults = true;
        try
        {
            // Keep reset edits in the form until OK/Apply; Cancel still discards them.
            _settings = defaults;
            _systemTray.IsChecked = defaults.EnableSystemTray;
            _trayIconStyle.SelectedItem = _trayIconStyle.Items.OfType<TrayIconStyleChoice>().First(choice => choice.Value == defaults.TrayIconStyle);
            _showTaskbarIcon.IsChecked = defaults.ShowTaskbarIcon;
            _autoUpdateCheck.IsChecked = defaults.EnableAutoUpdateCheck;
            _autoInstallUpdates.IsChecked = defaults.EnableAutoInstallUpdates;
            _startOnBoot.IsChecked = defaults.StartOnBoot;
            _minimizeOnStart.IsChecked = defaults.MinimizeOnStart;
            _alwaysOnTop.IsChecked = defaults.AlwaysOnTop;
            _enableClaudeUsage.IsChecked = defaults.EnableClaudeUsage;
            _enableCodexUsage.IsChecked = defaults.EnableCodexUsage;
            _showClaudeSession.IsChecked = defaults.ShowClaudeSession;
            _showClaudeWeekly.IsChecked = defaults.ShowClaudeWeekly;
            _showCodexFiveHour.IsChecked = defaults.ShowCodexFiveHour;
            _showCodexWeekly.IsChecked = defaults.ShowCodexWeekly;
            _showPacePointer.IsChecked = defaults.ShowPacePointer;
            _language.SelectedItem = _language.Items.OfType<LanguageChoice>().First(choice => choice.Value == defaults.Language);
            _theme.SelectedItem = _theme.Items.OfType<ThemeChoice>().First(choice => choice.Value == defaults.Theme);
            _overlayDesign.SelectedItem = _overlayDesign.Items.OfType<OptionChoice>().First(choice => choice.Value == defaults.OverlayDesign);
            _resetTimeOnNameRow.IsChecked = defaults.ResetTimeOnNameRow;
            _headerBadge.SelectedItem = _headerBadge.Items.OfType<OptionChoice>().First(choice => choice.Value == defaults.HeaderBadge);
            _badgeAverage.SelectedItem = _badgeAverage.Items.OfType<OptionChoice>().First(choice => choice.Value == defaults.BadgeAverage);
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
        UpdateDesignControls();
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
            // Keep the saved choice on an installation that cannot update itself.
            EnableAutoInstallUpdates = InstallKindDetector.SupportsInstall(_updateInstaller.Kind)
                ? _autoInstallUpdates.IsChecked == true
                : _settings.EnableAutoInstallUpdates,
            StartOnBoot = _startOnBoot.IsChecked == true,
            MinimizeOnStart = _minimizeOnStart.IsChecked == true,
            AlwaysOnTop = _alwaysOnTop.IsChecked == true,
            EnableClaudeUsage = _enableClaudeUsage.IsChecked == true,
            EnableCodexUsage = _enableCodexUsage.IsChecked == true,
            ShowClaudeSession = _showClaudeSession.IsChecked == true,
            ShowClaudeWeekly = _showClaudeWeekly.IsChecked == true,
            ShowCodexFiveHour = _showCodexFiveHour.IsChecked == true,
            ShowCodexWeekly = _showCodexWeekly.IsChecked == true,
            ShowPacePointer = _showPacePointer.IsChecked == true,
            Language = (_language.SelectedItem as LanguageChoice)?.Value ?? "en-US",
            Theme = (_theme.SelectedItem as ThemeChoice)?.Value ?? UiThemeOptions.System,
            OverlayDesign = (_overlayDesign.SelectedItem as OptionChoice)?.Value ?? OverlayDesignOptions.Latest,
            ResetTimeOnNameRow = _resetTimeOnNameRow.IsChecked == true,
            HeaderBadge = (_headerBadge.SelectedItem as OptionChoice)?.Value ?? HeaderBadgeOptions.Lowest,
            BadgeAverage = (_badgeAverage.SelectedItem as OptionChoice)?.Value ?? BadgeAverageOptions.Plain,
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

    private void UpdateDesignControls()
    {
        // The label gaps belong to the Previous design's bar layout.
        var previous = (_overlayDesign.SelectedItem as OptionChoice)?.Value == OverlayDesignOptions.Previous;
        _labelPercentageSpacing.IsEnabled = previous;
        _percentageTimeSpacing.IsEnabled = previous;
        _previousDesignNote.IsVisible = !previous;
        // The one-row layout belongs to the Latest design's limit rows.
        _resetTimeOnNameRow.IsEnabled =
            (_overlayDesign.SelectedItem as OptionChoice)?.Value == OverlayDesignOptions.Latest;
        // Only the Latest design shows the header badge.
        _headerBadge.IsEnabled =
            (_overlayDesign.SelectedItem as OptionChoice)?.Value == OverlayDesignOptions.Latest;
        _badgeAverage.IsEnabled = _headerBadge.IsEnabled;
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

    /// <summary>Adds a sidebar entry and its page: a heading over a two-column form.</summary>
    private void AddPage(string key, string title, string icon, params FormRow[] rows)
    {
        // A fixed label column keeps the controls in place when the page changes.
        var form = new Grid { ColumnDefinitions = new ColumnDefinitions("170,*"), RowSpacing = 18, ColumnSpacing = 16 };
        foreach (var row in rows)
        {
            form.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            var index = form.RowDefinitions.Count - 1;
            var (label, content) = row;
            if (label is not null)
            {
                Grid.SetRow(label, index);
                form.Children.Add(label);
            }
            Grid.SetRow(content, index);
            Grid.SetColumn(content, 1);
            form.Children.Add(content);
        }

        _pages.Add(key, new StackPanel
        {
            Spacing = 20,
            IsVisible = false,
            Children =
            {
                new TextBlock { Text = title, FontSize = 20, FontWeight = FontWeight.SemiBold },
                form
            }
        });
        _sidebar.Items.Add(new ListBoxItem
        {
            Tag = key,
            Padding = new Thickness(10, 8),
            Content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10,
                Children =
                {
                    new PathIcon { Width = 18, Height = 18, Data = Geometry.Parse(icon) },
                    new TextBlock { Text = title, VerticalAlignment = VerticalAlignment.Center }
                }
            }
        });
    }

    /// <summary>One form row: a right-aligned label and its controls stacked on the right.</summary>
    private static FormRow Row(string? label, params Control[] controls)
    {
        var stack = new StackPanel
        {
            Spacing = 8,
            Width = 380,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        foreach (var control in controls)
            stack.Children.Add(control);
        // Align the label with the first control's text line.
        var first = controls[0];
        var labelText = label is null ? null : new TextBlock
        {
            Text = label,
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Right,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, first is CheckBox ? 6 : first is TextBlock ? 0 : 7, 0, 0)
        };
        return new FormRow(labelText, stack);
    }

    private sealed record FormRow(TextBlock? Label, Control Content);

    /// <summary>A label and a link that opens <paramref name="url"/> in the browser.</summary>
    private static Control LinkLine(string label, string url)
    {
        var link = new HyperlinkButton
        {
            Content = url,
            NavigateUri = new Uri(url),
            Padding = new Thickness(0),
            FontSize = 12,
            VerticalAlignment = VerticalAlignment.Center
        };
        ToolTip.SetTip(link, url);
        AutomationProperties.SetName(link, $"{label} {url}");
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            Children = { Hint(label), link }
        };
    }

    private static TextBlock Hint(string text, bool secondary = true) => new()
    {
        Text = text,
        FontSize = secondary ? 12 : 13,
        Opacity = secondary ? 0.7 : 0.9,
        TextWrapping = TextWrapping.Wrap
    };

    private sealed record LanguageChoice(string Value, string DisplayName)
    {
        public override string ToString() => DisplayName;
    }

    private sealed record ThemeChoice(string Value, string DisplayName)
    {
        public override string ToString() => DisplayName;
    }

    /// <summary>A combo box choice: a stored setting value and its display name.</summary>
    /// <summary>Shows a header badge mode chosen from the panel, so saving keeps it.</summary>
    public void SelectHeaderBadge(string value, bool confirmHideBadge)
    {
        _settings = _settings with { ConfirmHideBadge = confirmHideBadge };
        var normalized = HeaderBadgeOptions.Normalize(value);
        _headerBadge.SelectedItem = _headerBadge.Items.OfType<OptionChoice>()
            .FirstOrDefault(choice => choice.Value == normalized) ?? _headerBadge.SelectedItem;
    }

    /// <summary>Shows a badge average method chosen from the panel, so saving keeps it.</summary>
    public void SelectBadgeAverage(string value)
    {
        var normalized = BadgeAverageOptions.Normalize(value);
        _badgeAverage.SelectedItem = _badgeAverage.Items.OfType<OptionChoice>()
            .FirstOrDefault(choice => choice.Value == normalized) ?? _badgeAverage.SelectedItem;
    }

    private sealed record OptionChoice(string Value, string DisplayName)
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
