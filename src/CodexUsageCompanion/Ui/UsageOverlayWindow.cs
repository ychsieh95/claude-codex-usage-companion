using Avalonia;
using Avalonia.Controls;
using Avalonia.Automation;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CodexUsageCompanion.Configuration;
using CodexUsageCompanion.Lifecycle;
using CodexUsageCompanion.Localization;
using CodexUsageCompanion.RateLimits;

namespace CodexUsageCompanion.Ui;

public sealed class UsageOverlayWindow : Window
{
    private const double CardSpacing = 12;
    private const double IconSize = 20;
    private const double HeaderGroupSpacing = 14;
    private const string CodexAccentColor = "#8D9BEF";
    private const string ClaudeIconBackground = "#D77655";
    private const string ClaudeIconPath =
        "M142.27 316.619l73.655-41.326 1.238-3.589-1.238-1.996-3.589-.001-12.31-.759-42.084-1.138-36.498-1.516-35.361-1.896-8.897-1.895-8.34-10.995.859-5.484 7.482-5.03 10.717.935 23.683 1.617 35.537 2.452 25.782 1.517 38.193 3.968h6.064l.86-2.451-2.073-1.517-1.618-1.517-36.776-24.922-39.81-26.338-20.852-15.166-11.273-7.683-5.687-7.204-2.451-15.721 10.237-11.273 13.75.935 3.513.936 13.928 10.716 29.749 23.027 38.848 28.612 5.687 4.727 2.275-1.617.278-1.138-2.553-4.271-21.13-38.193-22.546-38.848-10.035-16.101-2.654-9.655c-.935-3.968-1.617-7.304-1.617-11.374l11.652-15.823 6.445-2.073 15.545 2.073 6.547 5.687 9.655 22.092 15.646 34.78 24.265 47.291 7.103 14.028 3.791 12.992 1.416 3.968 2.449-.001v-2.275l1.997-26.641 3.69-32.707 3.589-42.084 1.239-11.854 5.863-14.206 11.652-7.683 9.099 4.348 7.482 10.716-1.036 6.926-4.449 28.915-8.72 45.294-5.687 30.331h3.313l3.792-3.791 15.342-20.372 25.782-32.227 11.374-12.789 13.27-14.129 8.517-6.724 16.1-.001 11.854 17.617-5.307 18.199-16.581 21.029-13.75 17.819-19.716 26.54-12.309 21.231 1.138 1.694 2.932-.278 44.536-9.479 24.062-4.347 28.714-4.928 12.992 6.066 1.416 6.167-5.106 12.613-30.71 7.583-36.018 7.204-53.636 12.689-.657.48.758.935 24.164 2.275 10.337.556h25.301l47.114 3.514 12.309 8.139 7.381 9.959-1.238 7.583-18.957 9.655-25.579-6.066-59.702-14.205-20.474-5.106-2.83-.001v1.694l17.061 16.682 31.266 28.233 39.152 36.397 1.997 8.999-5.03 7.102-5.307-.758-34.401-25.883-13.27-11.651-30.053-25.302-1.996-.001v2.654l6.926 10.136 36.574 54.975 1.895 16.859-2.653 5.485-9.479 3.311-10.414-1.895-21.408-30.054-22.092-33.844-17.819-30.331-2.173 1.238-10.515 113.261-4.929 5.788-11.374 4.348-9.478-7.204-5.03-11.652 5.03-23.027 6.066-30.052 4.928-23.886 4.449-29.674 2.654-9.858-.177-.657-2.173.278-22.37 30.71-34.021 45.977-26.919 28.815-6.445 2.553-11.173-5.789 1.037-10.337 6.243-9.2 37.257-47.392 22.47-29.371 14.508-16.961-.101-2.451h-.859l-98.954 64.251-17.618 2.275-7.583-7.103.936-11.652 3.589-3.791 29.749-20.474-.101.102.024.101z";
    private const string CodexIconPath =
        "M9.205 8.658v-2.26c0-.19.072-.333.238-.428l4.543-2.616c.619-.357 1.356-.523 2.117-.523 2.854 0 4.662 2.212 4.662 4.566 0 .167 0 .357-.024.547l-4.71-2.759a.797.797 0 00-.856 0l-5.97 3.473zm10.609 8.8V12.06c0-.333-.143-.57-.429-.737l-5.97-3.473 1.95-1.118a.433.433 0 01.476 0l4.543 2.617c1.309.76 2.189 2.378 2.189 3.948 0 1.808-1.07 3.473-2.76 4.163zM7.802 12.703l-1.95-1.142c-.167-.095-.239-.238-.239-.428V5.899c0-2.545 1.95-4.472 4.591-4.472 1 0 1.927.333 2.712.928L8.23 5.067c-.285.166-.428.404-.428.737v6.898zM12 15.128l-2.795-1.57v-3.33L12 8.658l2.795 1.57v3.33L12 15.128zm1.796 7.23c-1 0-1.927-.332-2.712-.927l4.686-2.712c.285-.166.428-.404.428-.737v-6.898l1.974 1.142c.167.095.238.238.238.428v5.233c0 2.545-1.974 4.472-4.614 4.472zm-5.637-5.303l-4.544-2.617c-1.308-.761-2.188-2.378-2.188-3.948A4.482 4.482 0 014.21 6.327v5.423c0 .333.143.571.428.738l5.947 3.449-1.95 1.118a.432.432 0 01-.476 0zm-.262 3.9c-2.688 0-4.662-2.021-4.662-4.519 0-.19.024-.38.047-.57l4.686 2.71c.286.167.571.167.856 0l5.97-3.448v2.26c0 .19-.07.333-.237.428l-4.543 2.616c-.619.357-1.356.523-2.117.523zm5.899 2.83a5.947 5.947 0 005.827-4.756C22.287 18.339 24 15.84 24 13.296c0-1.665-.713-3.282-1.998-4.448.119-.5.19-.999.19-1.498 0-3.401-2.759-5.947-5.946-5.947-.642 0-1.26.095-1.88.31A5.962 5.962 0 0010.205 0a5.947 5.947 0 00-5.827 4.757C1.713 5.447 0 7.945 0 10.49c0 1.666.713 3.283 1.998 4.448-.119.5-.19 1-.19 1.499 0 3.401 2.759 5.946 5.946 5.946.642 0 1.26-.095 1.88-.309a5.96 5.96 0 004.162 1.713z";
    private readonly UsageCardControls _codexFiveHourCard;
    private readonly UsageCardControls _codexWeeklyCard;
    private readonly UsageCardControls _claudeFiveHourCard;
    private readonly UsageCardControls _claudeWeeklyCard;
    private UiText _text;
    private readonly Border _root;
    private readonly StackPanel _contentStack;
    private readonly Control _header;
    private Border _claudeGroup = null!;
    private Border _codexGroup = null!;
    private readonly List<TextBlock> _providerTitles = [];
    private TextBlock _headerTitle = null!;
    private readonly TextBlock _status;
    private Button _minimizeButton = null!;
    private ToggleButton _pinButton = null!;
    private Button _shortcutsButton = null!;
    private Button _settingsButton = null!;
    private Button _refreshButton = null!;
    private Button _closeButton = null!;
    private string _position;
    private PixelPoint? _customPosition;
    private PixelPoint? _manualDragOrigin;
    private PixelPoint? _pendingCustomPosition;
    private readonly DispatcherTimer _customPositionSaveTimer = new()
    {
        Interval = TimeSpan.FromMilliseconds(400)
    };
    private readonly DispatcherTimer _manualDragResetTimer = new()
    {
        Interval = TimeSpan.FromSeconds(1)
    };
    private readonly int _margin;
    private bool _trayEnabled;
    private bool _showTaskbarIcon;
    private bool _showClaudeSession;
    private bool _showClaudeWeekly;
    private bool _showCodexFiveHour;
    private bool _showCodexWeekly;
    private bool _codexEnabled;
    private bool _claudeEnabled;
    private double _fontScale;
    private double _claudeSessionCardHeight;
    private double _claudeWeeklyCardHeight;
    private double _codexFiveHourCardHeight;
    private double _codexWeeklyCardHeight;
    private RateLimitState? _lastCodexState;
    private RateLimitState? _lastClaudeState;
    private readonly UsageStatusState _usageStatus = new();
    private string _backgroundColor = string.Empty;
    private int _percentageTimeSpacing = OverlayAppearanceOptions.DefaultSpacing;
    private int _labelPercentageSpacing = OverlayAppearanceOptions.DefaultLabelPercentageSpacing;
    private bool _fitWidthRequested = true;
    private double? _lastFittedWidth;
    private PixelPoint? _positionBeforeTrayHide;
    private OverlayThemePalette _palette = OverlayThemePalette.Dark;

    public UsageOverlayWindow(CompanionSettings? settings = null, UiText? text = null)
    {
        settings ??= new CompanionSettings();
        _text = text ?? UiText.For(UiLanguage.English);
        _backgroundColor = OverlayAppearanceOptions.NormalizeBackgroundColor(settings.BackgroundColor);
        _percentageTimeSpacing = OverlayAppearanceOptions.NormalizeSpacing(settings.PercentageTimeSpacing);
        _labelPercentageSpacing = OverlayAppearanceOptions.NormalizeLabelPercentageSpacing(settings.LabelPercentageSpacing);
        _position = WindowPosition.Normalize(settings.Position);
        _customPosition = GetCustomPosition(settings);
        _customPositionSaveTimer.Tick += (_, _) => FlushCustomPosition();
        _manualDragResetTimer.Tick += (_, _) =>
        {
            _manualDragResetTimer.Stop();
            _manualDragOrigin = null;
        };
        _margin = settings.Margin;
        _trayEnabled = settings.EnableSystemTray;
        _showTaskbarIcon = settings.ShowTaskbarIcon;
        _showClaudeSession = settings.ShowClaudeSession;
        _showClaudeWeekly = settings.ShowClaudeWeekly;
        _showCodexFiveHour = settings.ShowCodexFiveHour;
        _showCodexWeekly = settings.ShowCodexWeekly;
        _codexEnabled = settings.EnableCodexUsage;
        _claudeEnabled = settings.EnableClaudeUsage;
        _usageStatus.ApplyProviders(settings);
        _fontScale = OverlayLayoutOptions.NormalizeFontScale(settings.FontScale);
        _claudeSessionCardHeight = OverlayLayoutOptions.NormalizeCardHeight(settings.ClaudeSessionCardHeight, weekly: false);
        _claudeWeeklyCardHeight = OverlayLayoutOptions.NormalizeCardHeight(settings.ClaudeWeeklyCardHeight, weekly: true);
        _codexFiveHourCardHeight = OverlayLayoutOptions.NormalizeCardHeight(settings.CodexFiveHourCardHeight, weekly: false);
        _codexWeeklyCardHeight = OverlayLayoutOptions.NormalizeCardHeight(settings.CodexWeeklyCardHeight, weekly: true);

        Title = "Claude Codex Usage Companion";
        Width = 430;
        Height = ComputeHeight();
        MinWidth = 280;
        MinHeight = 120;
        CanResize = true;
        ExtendClientAreaToDecorationsHint = true;
        WindowDecorations = Avalonia.Controls.WindowDecorations.None;
        ShowInTaskbar = _showTaskbarIcon;
        Topmost = settings.AlwaysOnTop;
        Opacity = settings.Opacity;
        Background = Brushes.Transparent;
        WindowStartupLocation = WindowStartupLocation.Manual;

        _root = new Border
        {
            Background = Brush("#F2272927"),
            BorderBrush = Brush("#655B605B"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(16),
            Padding = new Thickness(12),
            BoxShadow = new BoxShadows(new BoxShadow
            {
                Blur = 22,
                OffsetY = 5,
                Color = Color.Parse("#70000000")
            })
        };
        var stack = _contentStack = new StackPanel { Spacing = CardSpacing };
        var header = _header = CreateHeader();
        _codexFiveHourCard = CreateCard(UsageCardKind.CodexFiveHour, _text.ProgressFiveHourLabel, showDetails: false);
        _codexWeeklyCard = CreateCard(UsageCardKind.CodexWeekly, _text.ProgressWeeklyLabel, showDetails: true);
        _claudeFiveHourCard = CreateCard(UsageCardKind.ClaudeSession, _text.ProgressFiveHourLabel, showDetails: false);
        _claudeWeeklyCard = CreateCard(UsageCardKind.ClaudeWeekly, _text.ProgressWeeklyLabel, showDetails: true);
        _claudeGroup = CreateProviderGroup("Claude", CreateClaudeIcon, _claudeFiveHourCard, _claudeWeeklyCard);
        _codexGroup = CreateProviderGroup("Codex", CreateCodexIcon, _codexFiveHourCard, _codexWeeklyCard);
        ApplyCardVisibility();
        _status = new TextBlock
        {
            Text = _text.WaitingForData,
            Foreground = Brush("#8E938E"),
            FontSize = 11,
            TextTrimming = TextTrimming.CharacterEllipsis
        };

        stack.Children.Add(_claudeGroup);
        stack.Children.Add(_codexGroup);
        stack.Children.Add(_status);
        var layout = new Grid { RowDefinitions = new RowDefinitions("Auto,*") };
        var scroll = new ScrollViewer
        {
            Content = stack,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        };
        Grid.SetRow(scroll, 1);
        layout.Children.Add(header);
        layout.Children.Add(scroll);
        var resizeGrip = new Border
        {
            Width = 24,
            Height = 24,
            Background = Brushes.Transparent,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, -9, -9),
            Child = new TextBlock
            {
                Text = "◢",
                FontSize = 13,
                Foreground = Brush("#8E938E"),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom
            }
        };
        resizeGrip.Child!.Opacity = 0;
        resizeGrip.PointerEntered += (_, _) => resizeGrip.Child!.Opacity = 0.8;
        resizeGrip.PointerExited += (_, _) => resizeGrip.Child!.Opacity = 0;
        resizeGrip.PointerPressed += HandleResizeGripPointerPressed;
        Grid.SetRow(resizeGrip, 1);
        layout.Children.Add(resizeGrip);
        _root.Child = layout;
        Content = _root;
        ApplyFontScale();
        ApplyCardHeights();
        // Initialize placeholder text, configured spacing, and fitted size
        // before Show() so the first frame already uses the saved appearance.
        ApplyThemePalette();
        ApplyComputedHeight();
        ActualThemeVariantChanged += (_, _) => ApplyThemePalette();
        SizeChanged += HandleWindowSizeChanged;
        PositionChanged += HandlePositionChanged;
        Closed += (_, _) => { FlushCustomPosition(); _manualDragResetTimer.Stop(); };

        Opened += (_, _) =>
        {
            if (GetTrayRestorePosition() is { } savedPosition) Position = savedPosition;
            else PositionOnPrimaryScreen();
        };
        AddHandler(KeyDownEvent, HandleKeyDown, RoutingStrategies.Tunnel);
        PrepareForFirstShow();
    }

    public void PrepareForFirstShow()
    {
        if (IsVisible)
            return;

        // Materialize theme templates and measure their final content before
        // the native window is mapped, rather than configuring it in Opened.
        ApplyTemplates(this);
        ApplyThemePalette();
        ApplyCardHeights();
        ApplyComputedHeight();
        Measure(new Size(Width, Height));
        Arrange(new Rect(0, 0, Width, Height));
        UpdateLayout();
        PositionOnPrimaryScreen();
    }

    private static void ApplyTemplates(Visual visual)
    {
        if (visual is TemplatedControl control)
            control.ApplyTemplate();
        foreach (var child in visual.GetVisualChildren().ToArray())
            ApplyTemplates(child);
    }

    public event EventHandler? RefreshRequested;
    public event EventHandler? SettingsRequested;
    public event EventHandler? ShortcutsRequested;
    public event Action<UsageCardKind, double>? CardHeightChanged;
    public event Action<PixelPoint>? CustomPositionChanged;
    public event Action<bool>? AlwaysOnTopRequested;

    private void HandleKeyDown(object? sender, KeyEventArgs eventArgs)
    {
        if (eventArgs.Key == Key.Escape)
        {
            eventArgs.Handled = true;
            Close();
            return;
        }

        if (eventArgs.Key == Key.F1)
        {
            eventArgs.Handled = true;
            ShortcutsRequested?.Invoke(this, EventArgs.Empty);
            return;
        }

        if (eventArgs.Key == Key.S && eventArgs.KeyModifiers == KeyModifiers.None)
        {
            eventArgs.Handled = true;
            SettingsRequested?.Invoke(this, EventArgs.Empty);
            return;
        }

        if (eventArgs.Key != Key.R ||
            (eventArgs.KeyModifiers & KeyModifiers.Control) == 0)
        {
            return;
        }

        eventArgs.Handled = true;
        RefreshRequested?.Invoke(this, EventArgs.Empty);
    }

    public void UpdateUsage(UsageProvider provider, RateLimitState? state)
    {
        if (provider == UsageProvider.Codex)
        {
            _lastCodexState = state;
            UpdateCard(_codexFiveHourCard, state?.FiveHour, weekly: false, dataAvailable: state is not null);
            UpdateCard(_codexWeeklyCard, state?.Weekly, weekly: true, dataAvailable: state is not null);
            _codexWeeklyCard.Details.Text = _text.FormatCreditDetails(
                state?.CreditBalance,
                state?.AutomaticReloadEnabled == true);
            ToolTip.SetTip(_codexWeeklyCard.Container, _codexWeeklyCard.Details.Text);
            FitResetColumns();
            return;
        }

        _lastClaudeState = state;
        UpdateCard(_claudeFiveHourCard, state?.FiveHour, weekly: false, dataAvailable: state is not null);
        UpdateCard(_claudeWeeklyCard, state?.Weekly, weekly: true, dataAvailable: state is not null);
        _claudeWeeklyCard.Details.Text = _text.FormatClaudeCreditsDetails(state?.ExtraUsage);
        ToolTip.SetTip(_claudeWeeklyCard.Container, _claudeWeeklyCard.Details.Text);
        FitResetColumns();
    }

    public void SetLoading(bool loading)
    {
        _refreshButton.IsEnabled = !loading;
        _refreshButton.Content = loading ? "…" : "↻";
    }

    public void SetStatus(DateTimeOffset? updatedAt, string? error, UsageProvider? provider = null)
    {
        _usageStatus.Update(updatedAt, error, provider);
        RenderStatus();
    }

    private void RenderStatus()
    {
        if (!string.IsNullOrWhiteSpace(_usageStatus.Error))
        {
            _status.Text = _usageStatus.Error;
            _status.Foreground = Brush(_palette.ErrorText);
            return;
        }

        _status.Text = _usageStatus.UpdatedAt is { } updatedAt
            ? _text.FormatUpdatedTime(updatedAt)
            : _text.WaitingForData;
        _status.Foreground = Brush(_palette.StatusText);
    }

    public void ApplySettings(CompanionSettings settings, UiText text)
    {
        _customPosition = GetCustomPosition(settings);
        _text = text;
        _trayEnabled = settings.EnableSystemTray;
        _showTaskbarIcon = settings.ShowTaskbarIcon;
        ShowInTaskbar = _showTaskbarIcon;
        ApplyAlwaysOnTop(settings.AlwaysOnTop);
        _showClaudeSession = settings.ShowClaudeSession;
        _showClaudeWeekly = settings.ShowClaudeWeekly;
        _showCodexFiveHour = settings.ShowCodexFiveHour;
        _showCodexWeekly = settings.ShowCodexWeekly;
        _codexEnabled = settings.EnableCodexUsage;
        _claudeEnabled = settings.EnableClaudeUsage;
        _usageStatus.ApplyProviders(settings);
        _fontScale = OverlayLayoutOptions.NormalizeFontScale(settings.FontScale);
        _claudeSessionCardHeight = OverlayLayoutOptions.NormalizeCardHeight(settings.ClaudeSessionCardHeight, weekly: false);
        _claudeWeeklyCardHeight = OverlayLayoutOptions.NormalizeCardHeight(settings.ClaudeWeeklyCardHeight, weekly: true);
        _codexFiveHourCardHeight = OverlayLayoutOptions.NormalizeCardHeight(settings.CodexFiveHourCardHeight, weekly: false);
        _codexWeeklyCardHeight = OverlayLayoutOptions.NormalizeCardHeight(settings.CodexWeeklyCardHeight, weekly: true);
        _codexFiveHourCard.Title.Text = text.ProgressFiveHourLabel;
        _codexWeeklyCard.Title.Text = text.ProgressWeeklyLabel;
        _claudeFiveHourCard.Title.Text = text.ProgressFiveHourLabel;
        _claudeWeeklyCard.Title.Text = text.ProgressWeeklyLabel;
        _headerTitle.Text = text.CombinedUsageHeaderTitle;
        ApplyFontScale();
        ApplyCardHeights();
        foreach (var kind in Enum.GetValues<UsageCardKind>()) UpdateCardResizeHint(GetCard(kind));
        ApplyCardVisibility();
        ApplySizeAndPosition(settings.Position);
        ToolTip.SetTip(_minimizeButton, text.MinimizeAction);
        UpdatePinButton();
        ToolTip.SetTip(_shortcutsButton, text.ShortcutsAction);
        ToolTip.SetTip(_settingsButton, text.SettingsAction);
        ToolTip.SetTip(_refreshButton, text.RefreshAction);
        ToolTip.SetTip(_closeButton, CloseTooltip());
        ApplyAppearance(settings.BackgroundColor, settings.PercentageTimeSpacing, settings.LabelPercentageSpacing);
        UpdateUsage(UsageProvider.Codex, _lastCodexState);
        UpdateUsage(UsageProvider.Claude, _lastClaudeState);
        RenderStatus();
        // Visibility and localized text invalidate nested layouts asynchronously.
        // Fit again after those invalidations reach the content stack.
        Dispatcher.UIThread.Post(ApplyComputedHeight, DispatcherPriority.Loaded);
    }

    public void ApplyAppearance(string backgroundColor, int percentageTimeSpacing, int labelPercentageSpacing)
    {
        var timeSpacing = OverlayAppearanceOptions.NormalizeSpacing(percentageTimeSpacing);
        var labelSpacing = OverlayAppearanceOptions.NormalizeLabelPercentageSpacing(labelPercentageSpacing);
        _fitWidthRequested |= timeSpacing != _percentageTimeSpacing || labelSpacing != _labelPercentageSpacing;
        _backgroundColor = OverlayAppearanceOptions.NormalizeBackgroundColor(backgroundColor);
        _percentageTimeSpacing = timeSpacing;
        _labelPercentageSpacing = labelSpacing;
        ApplyThemePalette();
    }

    public void ApplyAlwaysOnTop(bool enabled)
    {
        Topmost = enabled;
        if (_pinButton is not null)
        {
            _pinButton.IsChecked = enabled;
            UpdatePinButton();
        }
    }

    public void RestoreAndActivate()
    {
        if (!IsVisible && _positionBeforeTrayHide is null)
            PrepareForFirstShow();
        var wasHidden = !IsVisible;
        var restorePosition = wasHidden ? GetTrayRestorePosition() : null;
        var restoreTopmost = wasHidden && Topmost;
        // Some X11 window managers discard _NET_WM_STATE_ABOVE and
        // _NET_WM_STATE_SKIP_TASKBAR when a window is unmapped. Change the
        // managed values before remapping so restoring them after Show()
        // sends both states to the native window again.
        if (restoreTopmost)
        {
            Topmost = false;
        }

        if (wasHidden && !_showTaskbarIcon)
        {
            ShowInTaskbar = true;
        }

        Show();
        ShowInTaskbar = _showTaskbarIcon;
        if (restoreTopmost)
        {
            Topmost = true;
        }

        WindowState = WindowState.Normal;
        if (restorePosition is { } savedPosition)
        {
            Position = savedPosition;
            Dispatcher.UIThread.Post(() =>
            {
                if (IsVisible)
                {
                    Position = savedPosition;
                }
            }, DispatcherPriority.Background);
        }
        Activate();
    }

    public void ApplyPosition(string position)
    {
        _manualDragOrigin = null;
        _manualDragResetTimer.Stop();
        _customPositionSaveTimer.Stop();
        _pendingCustomPosition = null;
        _position = WindowPosition.Normalize(position);
        if (_position == WindowPosition.Custom && _customPosition is null && IsVisible)
        {
            _customPosition = Position;
        }
        PositionOnPrimaryScreen();
    }

    public void HideToTray()
    {
        FlushCustomPosition();
        if (IsVisible)
        {
            _positionBeforeTrayHide = Position;
        }

        Hide();
    }

    private PixelPoint? GetTrayRestorePosition() =>
        _positionBeforeTrayHide is { } savedPosition &&
        Screens.All.Any(screen => screen.Bounds.Contains(savedPosition))
            ? savedPosition
            : null;

    private static PixelPoint? GetCustomPosition(CompanionSettings settings) =>
        settings.CustomPositionX is { } x && settings.CustomPositionY is { } y
            ? new PixelPoint(x, y)
            : null;

    private void HandlePositionChanged(object? sender, PixelPointEventArgs eventArgs)
    {
        if (_manualDragOrigin is { } origin && Position != origin)
        {
            _position = WindowPosition.Custom;
            _manualDragOrigin = null;
            _manualDragResetTimer.Stop();
        }

        if (_position != WindowPosition.Custom || !IsVisible)
        {
            return;
        }

        _customPosition = Position;
        _pendingCustomPosition = Position;
        _customPositionSaveTimer.Stop();
        _customPositionSaveTimer.Start();
    }

    private void FlushCustomPosition()
    {
        _customPositionSaveTimer.Stop();
        if (_pendingCustomPosition is not { } position)
        {
            return;
        }

        _pendingCustomPosition = null;
        CustomPositionChanged?.Invoke(position);
    }

    private double EffectiveCardHeight(double configuredHeight, bool weekly) =>
        Math.Max(configuredHeight, OverlayLayoutOptions.MinimumCardHeight(weekly, _fontScale));

    private double GetConfiguredCardHeight(UsageCardKind kind) => kind switch
    {
        UsageCardKind.ClaudeSession => _claudeSessionCardHeight,
        UsageCardKind.ClaudeWeekly => _claudeWeeklyCardHeight,
        UsageCardKind.CodexFiveHour => _codexFiveHourCardHeight,
        _ => _codexWeeklyCardHeight
    };

    private void SetCardHeight(UsageCardKind kind, double height)
    {
        var weekly = kind is UsageCardKind.ClaudeWeekly or UsageCardKind.CodexWeekly;
        var normalized = OverlayLayoutOptions.NormalizeCardHeight(height, weekly);
        switch (kind)
        {
            case UsageCardKind.ClaudeSession:
                _claudeSessionCardHeight = normalized;
                break;
            case UsageCardKind.ClaudeWeekly:
                _claudeWeeklyCardHeight = normalized;
                break;
            case UsageCardKind.CodexFiveHour:
                _codexFiveHourCardHeight = normalized;
                break;
            case UsageCardKind.CodexWeekly:
                _codexWeeklyCardHeight = normalized;
                break;
        }

        ApplyCardHeight(GetCard(kind), EffectiveCardHeight(normalized, weekly));
    }

    private UsageCardControls GetCard(UsageCardKind kind) => kind switch
    {
        UsageCardKind.ClaudeSession => _claudeFiveHourCard,
        UsageCardKind.ClaudeWeekly => _claudeWeeklyCard,
        UsageCardKind.CodexFiveHour => _codexFiveHourCard,
        _ => _codexWeeklyCard
    };

    private void ApplyCardHeights()
    {
        foreach (var kind in Enum.GetValues<UsageCardKind>())
        {
            var weekly = kind is UsageCardKind.ClaudeWeekly or UsageCardKind.CodexWeekly;
            ApplyCardHeight(GetCard(kind), EffectiveCardHeight(GetConfiguredCardHeight(kind), weekly));
        }
    }

    private static void ApplyCardHeight(UsageCardControls card, double height)
    {
        card.Container.Height = height;
        card.Details.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        var progressHeight = Math.Max(36, height -
            (card.Details.IsVisible ? Math.Max(20, card.Details.DesiredSize.Height) : 0));
        card.Progress.Height = progressHeight;
        foreach (var cell in card.Cells) cell.Height = progressHeight;
        foreach (var fill in card.Fills) fill.Height = progressHeight;
    }

    private void ApplyFontScale()
    {
        foreach (var title in _providerTitles) title.FontSize = 21 * _fontScale;
        _headerTitle.FontSize = 11 * _fontScale;
        _status.FontSize = 11 * _fontScale;
        foreach (var card in new[]
                 { _claudeFiveHourCard, _claudeWeeklyCard, _codexFiveHourCard, _codexWeeklyCard })
        {
            card.Title.FontSize = 14 * _fontScale;
            card.Remaining.FontSize = 13 * _fontScale;
            card.Reset.FontSize = 11.5 * _fontScale;
            card.Details.FontSize = 11 * _fontScale;
        }
    }

    private void UpdateCardResizeHint(UsageCardControls card)
    {
        var hint = _text.FormatResizeCardHint(card.Title.Text ?? string.Empty);
        ToolTip.SetTip(card.ResizeStrip, hint);
        AutomationProperties.SetName(card.ResizeStrip, hint);
    }

    private void HandleResizeGripPointerPressed(object? sender, PointerPressedEventArgs eventArgs)
    {
        if (eventArgs.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginResizeDrag(WindowEdge.SouthEast, eventArgs);
        }
    }

    private void ApplyCardVisibility()
    {
        _codexFiveHourCard.Container.IsVisible = _showCodexFiveHour && _codexEnabled;
        _codexWeeklyCard.Container.IsVisible = _showCodexWeekly && _codexEnabled;
        _claudeFiveHourCard.Container.IsVisible = _showClaudeSession && _claudeEnabled;
        _claudeWeeklyCard.Container.IsVisible = _showClaudeWeekly && _claudeEnabled;
        _claudeGroup.IsVisible = _claudeEnabled && (_showClaudeSession || _showClaudeWeekly);
        _codexGroup.IsVisible = _codexEnabled && (_showCodexFiveHour || _showCodexWeekly);
    }

    private double ComputeHeight()
    {
        // The first call precedes construction of the content.
        if (_root is null)
            return 70;

        // A ScrollViewer has no measured viewport before its template is ready.
        // Measure its content directly so startup includes every visible card.
        var horizontalInset = _root.Padding.Left + _root.Padding.Right +
            _root.BorderThickness.Left + _root.BorderThickness.Right;
        var available = new Size(Math.Max(0, Width - horizontalInset), double.PositiveInfinity);
        _header.Measure(available);
        _contentStack.Measure(available);
        var verticalInset = _root.Padding.Top + _root.Padding.Bottom +
            _root.BorderThickness.Top + _root.BorderThickness.Bottom;
        return Math.Max(MinHeight, Math.Ceiling(_header.DesiredSize.Height +
            _contentStack.DesiredSize.Height + verticalInset));
    }

    private void ApplyComputedHeight()
    {
        var targetHeight = ComputeHeight();
        if (Math.Abs(Height - targetHeight) < 0.5)
            return;

        Height = targetHeight;
        PositionOnPrimaryScreen();
        Dispatcher.UIThread.Post(CompletePendingReposition, DispatcherPriority.Loaded);
    }

    private void ApplySizeAndPosition(string position)
    {
        ApplyPosition(position);
        ApplyComputedHeight();
        PositionOnPrimaryScreen();
        Dispatcher.UIThread.Post(
            CompletePendingReposition,
            DispatcherPriority.Loaded);
    }

    private void HandleWindowSizeChanged(object? sender, SizeChangedEventArgs eventArgs)
    {
        // Native resize events can arrive after the settings/layout callback.
        // Keep the configured anchor whenever the content changes the size.
        PositionOnPrimaryScreen();
    }

    private void CompletePendingReposition()
    {
        // Native layout may complete after a requested content resize.
        PositionOnPrimaryScreen();
    }

    private Control CreateHeader()
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto,Auto,Auto,Auto,Auto"),
            Height = 26
        };
        _headerTitle = new TextBlock
        {
            Text = _text.CombinedUsageHeaderTitle,
            Foreground = Brush("#CDD1CD"),
            FontSize = 11,
            FontWeight = FontWeight.SemiBold,
            LetterSpacing = 1.3,
            TextTrimming = TextTrimming.CharacterEllipsis,
            VerticalAlignment = VerticalAlignment.Center
        };
        _headerTitle.PointerPressed += HandleHeaderPointerPressed;

        _shortcutsButton = HeaderButton("?", _text.ShortcutsAction);
        _shortcutsButton.Click += (_, _) => ShortcutsRequested?.Invoke(this, EventArgs.Empty);
        Grid.SetColumn(_shortcutsButton, 1);

        _pinButton = HeaderToggleButton(CreatePinIcon(), string.Empty);
        _pinButton.IsChecked = Topmost;
        _pinButton.Click += (_, _) =>
            AlwaysOnTopRequested?.Invoke(_pinButton.IsChecked == true);
        UpdatePinButton();
        Grid.SetColumn(_pinButton, 2);

        _settingsButton = HeaderButton("⚙", _text.SettingsAction);
        _settingsButton.Click += (_, _) => SettingsRequested?.Invoke(this, EventArgs.Empty);
        Grid.SetColumn(_settingsButton, 3);

        _refreshButton = HeaderButton("↻", _text.RefreshAction);
        _refreshButton.Click += (_, _) => RefreshRequested?.Invoke(this, EventArgs.Empty);
        Grid.SetColumn(_refreshButton, 4);

        _minimizeButton = HeaderButton("−", _text.MinimizeAction);
        _minimizeButton.Click += (_, _) => WindowState = WindowState.Minimized;
        // Sets the window controls apart from the panel actions before them.
        _minimizeButton.Margin = new Thickness(HeaderGroupSpacing, 0, 0, 0);
        Grid.SetColumn(_minimizeButton, 5);

        _closeButton = HeaderButton("×", CloseTooltip());
        _closeButton.Click += (_, _) => Close();
        Grid.SetColumn(_closeButton, 6);

        grid.Children.Add(_headerTitle);
        grid.Children.Add(_shortcutsButton);
        grid.Children.Add(_pinButton);
        grid.Children.Add(_settingsButton);
        grid.Children.Add(_refreshButton);
        grid.Children.Add(_minimizeButton);
        grid.Children.Add(_closeButton);
        return grid;
    }

    private string CloseTooltip() =>
        _trayEnabled ? _text.HideToTrayAction : _text.CloseAction;

    private static Button HeaderButton(string content, string tooltip)
    {
        var button = new Button
        {
            Content = content,
            Width = 28,
            Height = 26,
            Padding = new Thickness(0),
            Margin = new Thickness(4, 0, 0, 0),
            FontSize = 16,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center
        };
        ToolTip.SetTip(button, tooltip);
        return button;
    }

    private static ToggleButton HeaderToggleButton(Control content, string tooltip)
    {
        var button = new ToggleButton
        {
            Content = content,
            Width = 28,
            Height = 26,
            Padding = new Thickness(0),
            Margin = new Thickness(4, 0, 0, 0),
            FontSize = 14,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center
        };
        ToolTip.SetTip(button, tooltip);
        return button;
    }

    private static Control CreatePinIcon() => new PathIcon
    {
        Width = 14,
        Height = 14,
        Data = Geometry.Parse(
            "M16 9V4L17 3V2H7V3L8 4V9C8 10.1 7.1 11 6 11V13H11V20L12 21L13 20V13H18V11C16.9 11 16 10.1 16 9Z")
    };

    private void UpdatePinButton()
    {
        var pinned = _pinButton.IsChecked == true;
        _pinButton.Opacity = pinned ? 1d : 0.55d;
        ToolTip.SetTip(
            _pinButton,
            pinned ? _text.UnpinFromTopAction : _text.PinOnTopAction);
    }

    private static Control CreateClaudeIcon()
    {
        var canvas = new Canvas { Width = 512, Height = 509.64 };
        canvas.Children.Add(new Avalonia.Controls.Shapes.Path
        {
            Data = Geometry.Parse(ClaudeIconPath),
            Fill = Brush(ClaudeIconBackground)
        });
        return new Viewbox { Width = IconSize, Height = IconSize, Child = canvas };
    }

    private static Control CreateCodexIcon()
    {
        var canvas = new Canvas { Width = 24, Height = 24 };
        canvas.Children.Add(new Avalonia.Controls.Shapes.Path
        {
            Data = Geometry.Parse(CodexIconPath),
            Fill = Brush(CodexAccentColor)
        });
        return new Viewbox { Width = IconSize, Height = IconSize, Child = canvas };
    }

    private Border CreateProviderGroup(string name, Func<Control> createIcon,
        UsageCardControls session, UsageCardControls weekly)
    {
        var title = new TextBlock
        {
            Text = name, FontSize = 21, FontWeight = FontWeight.SemiBold,
            FontFamily = new FontFamily("Georgia, Noto Serif, serif"),
            VerticalAlignment = VerticalAlignment.Center
        };
        _providerTitles.Add(title);
        var heading = new StackPanel
        {
            Orientation = Orientation.Horizontal, Spacing = 9,
            Margin = new Thickness(0, 0, 0, 10),
            Children = { createIcon(), title }
        };
        return new Border
        {
            BorderThickness = new Thickness(0),
            Child = new StackPanel { Children = { heading, session.Container, weekly.Container } }
        };
    }

    private UsageCardControls CreateCard(UsageCardKind kind, string title, bool showDetails)
    {
        var container = new Border
        {
            Height = EffectiveCardHeight(GetConfiguredCardHeight(kind), showDetails),
            Background = Brush("#FF353835"),
            BorderBrush = Brush("#FF4A4E4A"),
            BorderThickness = new Thickness(0),
            Margin = new Thickness(0, 0, 0, 6)
        };
        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,Auto,Auto"),
            ColumnDefinitions = new ColumnDefinitions("*,Auto")
        };

        var titleText = new TextBlock
        {
            Text = title,
            Foreground = Brush("#F4F5F3"),
            FontSize = 14,
            FontWeight = FontWeight.Medium,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 8, 0),
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        var remaining = new TextBlock
        {
            Foreground = Brush("#9CA09C"),
            FontSize = 14,
            FontWeight = FontWeight.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 8, 0),
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        var reset = new TextBlock
        {
            Foreground = Brush("#B7BAB6"),
            FontSize = 13,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0),
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        Grid.SetRow(reset, 1);
        Grid.SetColumnSpan(reset, 2);
        var details = new TextBlock
        {
            Foreground = Brush("#B7BAB6"),
            FontSize = 11,
            Margin = new Thickness(0, 4, 0, 0),
            IsVisible = showDetails
        };
        Grid.SetRow(details, 2);
        Grid.SetColumnSpan(details, 2);
        grid.Children.Add(details);

        var bar = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*,*,*"),
            VerticalAlignment = VerticalAlignment.Bottom
        };
        var fills = new Border[5];
        var cells = new Border[5];
        for (var index = 0; index < fills.Length; index++)
        {
            var fill = new Border
            {
                Width = 0,
                Opacity = 0.35,
                Height = 36,
                HorizontalAlignment = HorizontalAlignment.Left,
                CornerRadius = new CornerRadius(0)
            };
            var cell = new Border
            {
                Height = 36,
                Background = Brush("#FF4A4D49"),
                CornerRadius = new CornerRadius(0),
                Child = fill,
                ClipToBounds = true
            };
            cells[index] = cell;
            fills[index] = fill;
            Grid.SetColumn(cell, index);
            bar.Children.Add(cell);
        }

        var resetIcon = new TextBlock
        {
            Text = "↻",
            FontSize = 13,
            VerticalAlignment = VerticalAlignment.Center,
            Opacity = 0
        };
        var labels = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto,16,180"),
            Children = { titleText, remaining, resetIcon, reset }
        };
        Grid.SetColumn(remaining, 1);
        Grid.SetRow(reset, 0);
        Grid.SetColumn(resetIcon, 2);
        Grid.SetColumn(reset, 3);
        Grid.SetColumnSpan(reset, 1);
        var progress = new Border
        {
            Height = 36,
            CornerRadius = new CornerRadius(6),
            ClipToBounds = true,
            VerticalAlignment = VerticalAlignment.Bottom,
            Child = new Grid { Children = { bar, labels } }
        };
        Grid.SetRow(progress, 0);
        Grid.SetColumnSpan(progress, 2);
        grid.Children.Add(progress);
        var resizeMark = new Border
        {
            Width = 28,
            Height = 2,
            Background = Brush("#8E938E"),
            CornerRadius = new CornerRadius(1),
            Opacity = 0,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        var resizeStrip = new Border
        {
            Height = 12,
            Background = Brushes.Transparent,
            Child = resizeMark,
            VerticalAlignment = VerticalAlignment.Bottom,
            RenderTransform = new TranslateTransform(0, 8),
            Focusable = true
        };
        var cardLayout = new Grid();
        cardLayout.Children.Add(grid);
        cardLayout.Children.Add(resizeStrip);
        container.Child = cardLayout;
        resizeStrip.PointerEntered += (_, _) => resizeMark.Opacity = 0.9;
        resizeStrip.PointerExited += (_, _) => resizeMark.Opacity = 0;
        resizeStrip.GotFocus += (_, _) => resizeMark.Opacity = 0.9;
        resizeStrip.LostFocus += (_, _) => resizeMark.Opacity = 0;
        resizeStrip.KeyDown += (_, eventArgs) =>
        {
            if (eventArgs.Key is not (Key.Up or Key.Down))
            {
                return;
            }

            SetCardHeight(kind, GetConfiguredCardHeight(kind) + (eventArgs.Key == Key.Up ? -10 : 10));
            CardHeightChanged?.Invoke(kind, GetConfiguredCardHeight(kind));
            eventArgs.Handled = true;
        };
        var dragging = false;
        var startY = 0d;
        var startHeight = 0d;
        resizeStrip.PointerPressed += (_, eventArgs) =>
        {
            if (!eventArgs.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                return;
            }

            dragging = true;
            startY = eventArgs.GetPosition(this).Y;
            startHeight = container.Height;
            eventArgs.Pointer.Capture(resizeStrip);
            eventArgs.Handled = true;
        };
        resizeStrip.PointerMoved += (_, eventArgs) =>
        {
            if (!dragging)
            {
                return;
            }

            var target = startHeight + eventArgs.GetPosition(this).Y - startY;
            var minimum = OverlayLayoutOptions.MinimumCardHeight(showDetails, _fontScale);
            SetCardHeight(kind, Math.Clamp(target, minimum, OverlayLayoutOptions.MaximumCardHeight));
            eventArgs.Handled = true;
        };
        resizeStrip.PointerReleased += (_, eventArgs) =>
        {
            if (!dragging)
            {
                return;
            }

            dragging = false;
            eventArgs.Pointer.Capture(null);
            CardHeightChanged?.Invoke(kind, GetConfiguredCardHeight(kind));
            eventArgs.Handled = true;
        };
        resizeStrip.PointerCaptureLost += (_, _) => dragging = false;
        var card = new UsageCardControls(container, titleText, remaining, reset,
            resetIcon, details, labels, cells, fills, resizeStrip, progress);
        UpdateCardResizeHint(card);
        foreach (var cell in cells) cell.SizeChanged += (_, _) => UpdateBarFillWidths(card);
        return card;
    }

    private void FitResetColumns()
    {
        var cards = new[] { _claudeFiveHourCard, _claudeWeeklyCard,
            _codexFiveHourCard, _codexWeeklyCard };
        var resetWidth = 0d;
        var labelWidth = 0d;
        var percentageWidth = 0d;
        foreach (var card in cards)
        {
            var group = card == _claudeFiveHourCard || card == _claudeWeeklyCard
                ? _claudeGroup : _codexGroup;
            if (!group.IsVisible || !card.Container.IsVisible)
                continue;

            card.Reset.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            card.Title.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            card.Remaining.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            percentageWidth = Math.Max(percentageWidth, card.Remaining.DesiredSize.Width);
            resetWidth = Math.Max(resetWidth, card.Reset.DesiredSize.Width);
            labelWidth = Math.Max(labelWidth, card.Title.DesiredSize.Width);
        }

        var labelColumnWidth = Math.Ceiling(labelWidth);
        var percentageColumnWidth = Math.Ceiling(percentageWidth);
        var resetColumnWidth = Math.Min(Math.Ceiling(resetWidth), 228);
        var targetWidth = Math.Max(280, 26 + labelColumnWidth + percentageColumnWidth + 16 + resetColumnWidth);
        var stillAutoFitted = _lastFittedWidth is { } previousWidth && Math.Abs(Width - previousWidth) < 0.5;
        if (_fitWidthRequested || stillAutoFitted || Width < targetWidth)
        {
            if (Math.Abs(Width - targetWidth) >= 0.5)
            {
                Width = targetWidth;
                PositionOnPrimaryScreen();
            }
            _lastFittedWidth = targetWidth;
        }
        _fitWidthRequested = false;
        foreach (var card in cards)
        {
            // Let any extra window width sit before the percentage, rather
            // than leaving an empty tail after the reset-time column.
            card.Labels.ColumnDefinitions[0].MinWidth = labelColumnWidth;
            card.Labels.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
            card.Labels.ColumnDefinitions[1].Width = new GridLength(percentageColumnWidth);
            card.Labels.ColumnDefinitions[3].Width = new GridLength(resetColumnWidth);

        }
    }

    private void UpdateCard(
        UsageCardControls card,
        RateLimitWindowState? state,
        bool weekly,
        bool dataAvailable)
    {
        card.ResetIcon.Opacity = state?.ResetsAt is not null ? 1 : 0;
        if (state is null)
        {
            card.Remaining.Text = _text.RemainingUnavailable;
            card.Reset.Text = dataAvailable ? _text.LimitUnavailable : _text.WaitingForData;
            ApplyBar(card, 0, UsageSignal.Gray);
            return;
        }

        card.Remaining.Text = $"{Math.Clamp(state.RemainingPercent, 0, 100)}%";
        card.Reset.Text = state.ResetsAt is long unixSeconds
            ? _text.FormatProgressReset(DateTimeOffset.FromUnixTimeSeconds(unixSeconds).ToLocalTime())
            : _text.ResetUnavailable;
        ApplyBar(card, state.RemainingPercent, UsagePresentation.GetSignal(state.RemainingPercent));
    }

    private void ApplyBar(UsageCardControls card, int remainingPercent, UsageSignal signal)
    {
        var color = SignalBrush(signal);
        var ratios = UsagePresentation.GetCellFillRatios(remainingPercent);
        card.Remaining.Foreground = Brush(_palette.CardTitle);
        card.FillRatios = ratios;
        for (var index = 0; index < card.Fills.Length; index++)
        {
            card.Fills[index].Background = color;
        }
        UpdateBarFillWidths(card);
    }

    private static void UpdateBarFillWidths(UsageCardControls card)
    {
        for (var index = 0; index < card.Fills.Length; index++)
        {
            card.Fills[index].Width = card.Cells[index].Bounds.Width * card.FillRatios[index];
        }
    }

    private void HandleHeaderPointerPressed(object? sender, PointerPressedEventArgs eventArgs)
    {
        if (!eventArgs.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        _manualDragOrigin = Position;
        _manualDragResetTimer.Stop();
        _manualDragResetTimer.Interval = TimeSpan.FromSeconds(10);
        _manualDragResetTimer.Start();
        BeginMoveDrag(eventArgs);
    }

    private void PositionOnPrimaryScreen()
    {
        if (_position == WindowPosition.Custom && _customPosition is { } customPosition)
        {
            if (Screens.All.Any(screen => screen.Bounds.Contains(customPosition)))
            {
                Position = customPosition;
                return;
            }
        }

        var screen = Screens.Primary;
        if (screen is null)
        {
            return;
        }

        // The working area excludes panels/taskbars reserved by the desktop.
        var area = screen.WorkingArea;
        if (area.Width <= 0 || area.Height <= 0)
            area = screen.Bounds;
        var size = new PixelSize(
            (int)Math.Ceiling(Width * screen.Scaling),
            (int)Math.Ceiling(Height * screen.Scaling));
        var margin = (int)Math.Ceiling(_margin * screen.Scaling);
        Position = OverlayWindowPlacement.Calculate(area, size, margin, _position);
    }

    private void ApplyThemePalette()
    {
        _palette = ActualThemeVariant == ThemeVariant.Light
            ? OverlayThemePalette.Light
            : OverlayThemePalette.Dark;
        _root.Background = Brush(string.IsNullOrEmpty(_backgroundColor) ? _palette.RootBackground : _backgroundColor);
        _root.BorderBrush = Brush(_palette.RootBorder);
        _root.BoxShadow = new BoxShadows(new BoxShadow
        {
            Blur = 22,
            OffsetY = 5,
            Color = Color.Parse(_palette.Shadow)
        });
        _headerTitle.Foreground = Brush(_palette.HeaderForeground);
        foreach (var group in new[] { _claudeGroup, _codexGroup })
        {
            group.Background = Brushes.Transparent;
            group.BorderBrush = Brush(_palette.CardBorder);
        }
        foreach (var title in _providerTitles)
            title.Foreground = Brush(_palette.CardTitle);
        foreach (var button in new Button[] { _shortcutsButton, _pinButton, _settingsButton,
            _refreshButton, _minimizeButton, _closeButton })
        {
            button.Background = Brushes.Transparent;
            button.BorderThickness = new Thickness(0);
            button.Foreground = Brush(_palette.SecondaryText);
        }
        ApplyCardTheme(_codexFiveHourCard);
        ApplyCardTheme(_codexWeeklyCard);
        ApplyCardTheme(_claudeFiveHourCard);
        ApplyCardTheme(_claudeWeeklyCard);
        RenderStatus();
        UpdateUsage(UsageProvider.Codex, _lastCodexState);
        UpdateUsage(UsageProvider.Claude, _lastClaudeState);
    }

    private void ApplyCardTheme(UsageCardControls card)
    {
        card.Container.Margin = new Thickness(0, 0, 0, 6);
        card.Container.Background = Brushes.Transparent;
        card.Container.BorderBrush = Brush(_palette.CardBorder);
        card.Remaining.Margin = new Thickness(_labelPercentageSpacing, 0, _percentageTimeSpacing, 0);
        card.Title.Foreground = Brush(_palette.CardTitle);
        card.Reset.Foreground = Brush(_palette.SecondaryText);
        card.ResetIcon.Foreground = Brush(_palette.SecondaryText);
        card.Details.Foreground = Brush(_palette.SecondaryText);
        foreach (var cell in card.Cells)
        {
            cell.Background = Brush(_palette.EmptyCell);
        }
    }

    private SolidColorBrush SignalBrush(UsageSignal signal) => signal switch
    {
        UsageSignal.Green => Brush(_palette.Green),
        UsageSignal.Yellow => Brush(_palette.Yellow),
        UsageSignal.Orange => Brush(_palette.Orange),
        UsageSignal.Red => Brush(_palette.Red),
        _ => Brush(_palette.Gray)
    };

    private static SolidColorBrush Brush(string color) => new(Color.Parse(color));

    private sealed record UsageCardControls(
        Border Container,
        TextBlock Title,
        TextBlock Remaining,
        TextBlock Reset,
        TextBlock ResetIcon,
        TextBlock Details,
        Grid Labels,
        Border[] Cells,
        Border[] Fills,
        Border ResizeStrip,
        Border Progress)
    {
        public double[] FillRatios { get; set; } = new double[5];
    }
}
