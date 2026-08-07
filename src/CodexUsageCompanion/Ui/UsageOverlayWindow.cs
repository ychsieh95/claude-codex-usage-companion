using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using CodexUsageCompanion.Configuration;
using CodexUsageCompanion.Lifecycle;
using CodexUsageCompanion.Localization;
using CodexUsageCompanion.RateLimits;

namespace CodexUsageCompanion.Ui;

public sealed class UsageOverlayWindow : Window
{
    private const double CellWidth = 47;
    private const double BaseHeight = 66;
    private const double CardSpacing = 9;
    private const double FiveHourCardHeight = 78;
    private const double WeeklyCardHeight = 96;
    private const double IconSize = 14;
    private const string CodexAccentColor = "#10A37F";
    private const string ClaudeIconBackground = "#D77655";
    private const string ClaudeIconForeground = "#FCF2EE";
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
    private TextBlock _headerTitle = null!;
    private readonly TextBlock _status;
    private Button _minimizeButton = null!;
    private ToggleButton _pinButton = null!;
    private Button _settingsButton = null!;
    private Button _refreshButton = null!;
    private Button _closeButton = null!;
    private string _position;
    private readonly int _margin;
    private bool _trayEnabled;
    private bool _showTaskbarIcon;
    private bool _showFiveHourLimit;
    private bool _codexEnabled;
    private bool _claudeEnabled;
    private RateLimitState? _lastCodexState;
    private RateLimitState? _lastClaudeState;
    private DateTimeOffset? _lastUpdatedAt;
    private string? _lastError;
    private OverlayThemePalette _palette = OverlayThemePalette.Dark;

    public UsageOverlayWindow(CompanionSettings? settings = null, UiText? text = null)
    {
        settings ??= new CompanionSettings();
        _text = text ?? UiText.For(UiLanguage.English);
        _position = WindowPosition.Normalize(settings.Position);
        _margin = settings.Margin;
        _trayEnabled = settings.EnableSystemTray;
        _showTaskbarIcon = settings.ShowTaskbarIcon;
        _showFiveHourLimit = settings.ShowFiveHourLimit;
        _codexEnabled = settings.EnableCodexUsage;
        _claudeEnabled = settings.EnableClaudeUsage;

        Title = "Claude Codex Usage Companion";
        Width = 344;
        Height = ComputeHeight(_codexEnabled, _showFiveHourLimit, _claudeEnabled);
        MinWidth = Width;
        MaxWidth = Width;
        MinHeight = Height;
        MaxHeight = Height;
        CanResize = false;
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
        var stack = new StackPanel { Spacing = CardSpacing };
        var header = CreateHeader();
        _codexFiveHourCard = CreateCard(_text.FiveHourTitle, CreateCodexIcon, showDetails: false);
        _codexWeeklyCard = CreateCard(_text.WeeklyTitle, CreateCodexIcon, showDetails: true);
        _claudeFiveHourCard = CreateCard(_text.ClaudeFiveHourTitle, CreateClaudeIcon, showDetails: false);
        _claudeWeeklyCard = CreateCard(_text.ClaudeWeeklyTitle, CreateClaudeIcon, showDetails: true);
        ApplyCardVisibility();
        _status = new TextBlock
        {
            Text = _text.WaitingForData,
            Foreground = Brush("#8E938E"),
            FontSize = 11,
            TextTrimming = TextTrimming.CharacterEllipsis
        };

        stack.Children.Add(header);
        stack.Children.Add(_claudeFiveHourCard.Container);
        stack.Children.Add(_claudeWeeklyCard.Container);
        stack.Children.Add(_codexFiveHourCard.Container);
        stack.Children.Add(_codexWeeklyCard.Container);
        stack.Children.Add(_status);
        _root.Child = stack;
        Content = _root;
        ActualThemeVariantChanged += (_, _) => ApplyThemePalette();

        Opened += (_, _) =>
        {
            PositionOnPrimaryScreen();
            ApplyThemePalette();
        };
        AddHandler(KeyDownEvent, HandleKeyDown, RoutingStrategies.Tunnel);
    }

    public event EventHandler? RefreshRequested;
    public event EventHandler? SettingsRequested;
    public event Action<bool>? AlwaysOnTopRequested;

    private void HandleKeyDown(object? sender, KeyEventArgs eventArgs)
    {
        if (eventArgs.Key == Key.Escape)
        {
            eventArgs.Handled = true;
            Close();
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
            return;
        }

        _lastClaudeState = state;
        UpdateCard(_claudeFiveHourCard, state?.FiveHour, weekly: false, dataAvailable: state is not null);
        UpdateCard(_claudeWeeklyCard, state?.Weekly, weekly: true, dataAvailable: state is not null);
        _claudeWeeklyCard.Details.Text = _text.FormatClaudeCreditsDetails(state?.ExtraUsage);
    }

    public void SetLoading(bool loading)
    {
        _refreshButton.IsEnabled = !loading;
        _refreshButton.Content = loading ? "…" : "↻";
    }

    public void SetStatus(DateTimeOffset? updatedAt, string? error)
    {
        _lastUpdatedAt = updatedAt;
        _lastError = error;
        if (!string.IsNullOrWhiteSpace(error))
        {
            _status.Text = error;
            _status.Foreground = Brush(_palette.ErrorText);
            return;
        }

        _status.Text = updatedAt is null
            ? _text.WaitingForData
            : _text.FormatUpdatedTime(updatedAt.Value);
        _status.Foreground = Brush(_palette.StatusText);
    }

    public void ApplySettings(CompanionSettings settings, UiText text)
    {
        _text = text;
        _trayEnabled = settings.EnableSystemTray;
        _showTaskbarIcon = settings.ShowTaskbarIcon;
        ShowInTaskbar = _showTaskbarIcon;
        ApplyAlwaysOnTop(settings.AlwaysOnTop);
        _showFiveHourLimit = settings.ShowFiveHourLimit;
        _codexEnabled = settings.EnableCodexUsage;
        _claudeEnabled = settings.EnableClaudeUsage;
        _codexFiveHourCard.Title.Text = text.FiveHourTitle;
        _codexWeeklyCard.Title.Text = text.WeeklyTitle;
        _claudeFiveHourCard.Title.Text = text.ClaudeFiveHourTitle;
        _claudeWeeklyCard.Title.Text = text.ClaudeWeeklyTitle;
        _headerTitle.Text = text.CombinedUsageHeaderTitle;
        ApplyCardVisibility();
        Height = ComputeHeight(_codexEnabled, _showFiveHourLimit, _claudeEnabled);
        MinHeight = Height;
        MaxHeight = Height;
        ApplyPosition(settings.Position);
        ToolTip.SetTip(_minimizeButton, text.MinimizeAction);
        UpdatePinButton();
        ToolTip.SetTip(_settingsButton, text.SettingsAction);
        ToolTip.SetTip(_refreshButton, text.RefreshAction);
        ToolTip.SetTip(_closeButton, CloseTooltip());
        ApplyThemePalette();
        UpdateUsage(UsageProvider.Codex, _lastCodexState);
        UpdateUsage(UsageProvider.Claude, _lastClaudeState);
        SetStatus(_lastUpdatedAt, _lastError);
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
        var wasHidden = !IsVisible;
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
        Activate();
    }

    public void ApplyPosition(string position)
    {
        _position = WindowPosition.Normalize(position);
        PositionOnPrimaryScreen();
    }

    private void ApplyCardVisibility()
    {
        _codexFiveHourCard.Container.IsVisible = _showFiveHourLimit && _codexEnabled;
        _codexWeeklyCard.Container.IsVisible = _codexEnabled;
        _claudeFiveHourCard.Container.IsVisible = _claudeEnabled;
        _claudeWeeklyCard.Container.IsVisible = _claudeEnabled;
    }

    private static double ComputeHeight(bool codexEnabled, bool showFiveHourLimit, bool claudeEnabled)
    {
        var cardHeights = new List<double>();
        if (codexEnabled)
        {
            cardHeights.Add(WeeklyCardHeight);
            if (showFiveHourLimit)
            {
                cardHeights.Add(FiveHourCardHeight);
            }
        }

        if (claudeEnabled)
        {
            cardHeights.Add(FiveHourCardHeight);
            cardHeights.Add(WeeklyCardHeight);
        }

        return BaseHeight + cardHeights.Sum() + (CardSpacing * (cardHeights.Count + 1));
    }

    private Control CreateHeader()
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto,Auto,Auto,Auto"),
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

        _minimizeButton = HeaderButton("−", _text.MinimizeAction);
        _minimizeButton.Click += (_, _) => WindowState = WindowState.Minimized;
        Grid.SetColumn(_minimizeButton, 1);

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

        _closeButton = HeaderButton("×", CloseTooltip());
        _closeButton.Click += (_, _) => Close();
        Grid.SetColumn(_closeButton, 5);

        grid.Children.Add(_headerTitle);
        grid.Children.Add(_minimizeButton);
        grid.Children.Add(_pinButton);
        grid.Children.Add(_settingsButton);
        grid.Children.Add(_refreshButton);
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
            Data = Geometry.Parse(
                "M115.612 0h280.775C459.974 0 512 52.026 512 115.612v278.415c0 63.587-52.026 115.612-115.613 115.612H115.612C52.026 509.639 0 457.614 0 394.027V115.612C0 52.026 52.026 0 115.612 0z"),
            Fill = Brush(ClaudeIconBackground)
        });
        canvas.Children.Add(new Avalonia.Controls.Shapes.Path
        {
            Data = Geometry.Parse(ClaudeIconPath),
            Fill = Brush(ClaudeIconForeground)
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

    private static UsageCardControls CreateCard(string title, Func<Control> createIcon, bool showDetails)
    {
        var container = new Border
        {
            Height = showDetails ? WeeklyCardHeight : FiveHourCardHeight,
            Background = Brush("#FF353835"),
            BorderBrush = Brush("#FF4A4E4A"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(11),
            Padding = new Thickness(11, 8)
        };
        var grid = new Grid
        {
            RowDefinitions = showDetails
                ? new RowDefinitions("Auto,Auto,*,Auto")
                : new RowDefinitions("Auto,Auto,*"),
            ColumnDefinitions = new ColumnDefinitions("*,Auto")
        };
        var icon = createIcon();
        icon.VerticalAlignment = VerticalAlignment.Center;
        icon.Margin = new Thickness(0, 0, 6, 0);
        var titleText = new TextBlock
        {
            Text = title,
            Foreground = Brush("#F4F5F3"),
            FontSize = 14,
            FontWeight = FontWeight.SemiBold,
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        var titleRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*")
        };
        Grid.SetColumn(icon, 0);
        Grid.SetColumn(titleText, 1);
        titleRow.Children.Add(icon);
        titleRow.Children.Add(titleText);
        var remaining = new TextBlock
        {
            Foreground = Brush("#9CA09C"),
            FontSize = 13,
            FontWeight = FontWeight.Bold,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(remaining, 1);
        var reset = new TextBlock
        {
            Foreground = Brush("#B7BAB6"),
            FontSize = 11.5,
            Margin = new Thickness(0, 1, 0, 3)
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
        Grid.SetRow(details, 3);
        Grid.SetColumnSpan(details, 2);

        var bar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            VerticalAlignment = VerticalAlignment.Bottom
        };
        var fills = new Border[5];
        var cells = new Border[5];
        for (var index = 0; index < fills.Length; index++)
        {
            var fill = new Border
            {
                Width = 0,
                Height = 9,
                HorizontalAlignment = HorizontalAlignment.Left,
                CornerRadius = new CornerRadius(2.5)
            };
            var cell = new Border
            {
                Width = CellWidth,
                Height = 9,
                Background = Brush("#FF4A4D49"),
                CornerRadius = new CornerRadius(2.5),
                Child = fill,
                ClipToBounds = true
            };
            cells[index] = cell;
            fills[index] = fill;
            bar.Children.Add(cell);
        }

        Grid.SetRow(bar, 2);
        Grid.SetColumnSpan(bar, 2);
        grid.Children.Add(titleRow);
        grid.Children.Add(remaining);
        grid.Children.Add(reset);
        grid.Children.Add(details);
        grid.Children.Add(bar);
        container.Child = grid;
        return new UsageCardControls(
            container,
            titleText,
            remaining,
            reset,
            details,
            cells,
            fills);
    }

    private void UpdateCard(
        UsageCardControls card,
        RateLimitWindowState? state,
        bool weekly,
        bool dataAvailable)
    {
        if (state is null)
        {
            card.Remaining.Text = _text.RemainingUnavailable;
            card.Reset.Text = dataAvailable ? _text.LimitUnavailable : _text.WaitingForData;
            ApplyBar(card, 0, UsageSignal.Gray);
            return;
        }

        card.Remaining.Text = _text.FormatRemaining(state.RemainingPercent);
        card.Reset.Text = state.ResetsAt is long unixSeconds
            ? weekly
                ? _text.FormatWeeklyReset(DateTimeOffset.FromUnixTimeSeconds(unixSeconds).ToLocalTime())
                : _text.FormatFiveHourReset(DateTimeOffset.FromUnixTimeSeconds(unixSeconds).ToLocalTime())
            : _text.ResetUnavailable;
        ApplyBar(card, state.RemainingPercent, UsagePresentation.GetSignal(state.RemainingPercent));
    }

    private void ApplyBar(UsageCardControls card, int remainingPercent, UsageSignal signal)
    {
        var color = SignalBrush(signal);
        var ratios = UsagePresentation.GetCellFillRatios(remainingPercent);
        card.Remaining.Foreground = color;
        for (var index = 0; index < card.Fills.Length; index++)
        {
            card.Fills[index].Background = color;
            card.Fills[index].Width = CellWidth * ratios[index];
        }
    }

    private void HandleHeaderPointerPressed(object? sender, PointerPressedEventArgs eventArgs)
    {
        if (eventArgs.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(eventArgs);
        }
    }

    private void PositionOnPrimaryScreen()
    {
        var screen = Screens.Primary;
        if (screen is null)
        {
            return;
        }

        var area = screen.WorkingArea;
        var width = (int)Math.Ceiling(Width * screen.Scaling);
        var height = (int)Math.Ceiling(Height * screen.Scaling);
        var margin = (int)Math.Ceiling(_margin * screen.Scaling);
        var x = _position switch
        {
            WindowPosition.LeftTop or
            WindowPosition.LeftCenter or
            WindowPosition.LeftBottom => area.X + margin,
            WindowPosition.MiddleTop or
            WindowPosition.MiddleCenter or
            WindowPosition.MiddleBottom => area.X + ((area.Width - width) / 2),
            _ => area.Right - width - margin
        };
        var y = _position switch
        {
            WindowPosition.LeftTop or
            WindowPosition.MiddleTop or
            WindowPosition.RightTop => area.Y + margin,
            WindowPosition.LeftCenter or
            WindowPosition.MiddleCenter or
            WindowPosition.RightCenter => area.Y + ((area.Height - height) / 2),
            _ => area.Bottom - height - margin
        };
        Position = new PixelPoint(x, y);
    }

    private void ApplyThemePalette()
    {
        _palette = ActualThemeVariant == ThemeVariant.Light
            ? OverlayThemePalette.Light
            : OverlayThemePalette.Dark;
        _root.Background = Brush(_palette.RootBackground);
        _root.BorderBrush = Brush(_palette.RootBorder);
        _root.BoxShadow = new BoxShadows(new BoxShadow
        {
            Blur = 22,
            OffsetY = 5,
            Color = Color.Parse(_palette.Shadow)
        });
        _headerTitle.Foreground = Brush(_palette.HeaderForeground);
        ApplyCardTheme(_codexFiveHourCard);
        ApplyCardTheme(_codexWeeklyCard);
        ApplyCardTheme(_claudeFiveHourCard);
        ApplyCardTheme(_claudeWeeklyCard);
        SetStatus(_lastUpdatedAt, _lastError);
        UpdateUsage(UsageProvider.Codex, _lastCodexState);
        UpdateUsage(UsageProvider.Claude, _lastClaudeState);
    }

    private void ApplyCardTheme(UsageCardControls card)
    {
        card.Container.Background = Brush(_palette.CardBackground);
        card.Container.BorderBrush = Brush(_palette.CardBorder);
        card.Title.Foreground = Brush(_palette.CardTitle);
        card.Reset.Foreground = Brush(_palette.SecondaryText);
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
        TextBlock Details,
        Border[] Cells,
        Border[] Fills);
}
