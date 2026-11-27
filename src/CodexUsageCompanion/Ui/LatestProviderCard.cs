using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using CodexUsageCompanion.Localization;
using CodexUsageCompanion.RateLimits;

namespace CodexUsageCompanion.Ui;

/// <summary>
/// One provider in the Latest overlay design: a card with a brand accent strip,
/// a heading, a row per limit (title, reset countdown, percentage, and a thin bar
/// with the even-pace marker), and an optional credits line.
/// </summary>
internal sealed class LatestProviderCard
{
    private const double MarkerWidth = 14;
    private const double TrackHeight = 6;
    // Monospaced digits; fontconfig resolves the generic family when none of the named ones exist.
    private static readonly FontFamily PercentFont =
        new("Noto Sans Mono, DejaVu Sans Mono, Liberation Mono, Cascadia Mono, monospace");
    // Space between the bar and the bottom of its row; the percentage's baseline sits there.
    // It leaves room for the lower half of the pace dot, which is centered on the track.
    private const double TrackBottomInset = (MarkerWidth - TrackHeight) / 2;
    private readonly TextBlock _name;
    private readonly TextBlock _caption;
    private readonly Border _iconBox;
    private readonly Border _divider;
    private readonly TextBlock _credits;
    private readonly Border _creditsDivider;

    public LatestProviderCard(string name, Color brand, Control icon)
    {
        _name = new TextBlock
        {
            Text = name,
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        _caption = new TextBlock
        {
            LetterSpacing = 0.6,
            VerticalAlignment = VerticalAlignment.Center
        };
        _iconBox = new Border
        {
            Width = 28,
            Height = 28,
            CornerRadius = new CornerRadius(6),
            Background = new SolidColorBrush(Color.FromArgb(0x1F, brand.R, brand.G, brand.B)),
            Child = icon
        };
        icon.HorizontalAlignment = HorizontalAlignment.Center;
        icon.VerticalAlignment = VerticalAlignment.Center;
        var heading = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
            Children = { _iconBox, _name, _caption }
        };
        _name.Margin = new Thickness(8, 0, 8, 0);
        Grid.SetColumn(_name, 1);
        Grid.SetColumn(_caption, 2);

        FiveHour = new LatestLimitRow();
        Weekly = new LatestLimitRow();
        _divider = new Border { Height = 1 };
        _credits = new TextBlock { TextWrapping = TextWrapping.Wrap };
        _creditsDivider = new Border { Height = 1, IsVisible = false };

        var content = new StackPanel
        {
            Spacing = 10,
            Margin = new Thickness(12, 12, 12, 12),
            Children = { heading, FiveHour.Root, _divider, Weekly.Root, _creditsDivider, _credits }
        };
        var accent = new Border { Background = new SolidColorBrush(brand) };
        var layout = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("3,*"),
            Children = { accent, content }
        };
        Grid.SetColumn(content, 1);
        Root = new Border
        {
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(1),
            ClipToBounds = true,
            Child = layout
        };
    }

    public Border Root { get; }

    public LatestLimitRow FiveHour { get; }

    public LatestLimitRow Weekly { get; }

    public void ApplyVisibility(bool showFiveHour, bool showWeekly)
    {
        FiveHour.Root.IsVisible = showFiveHour;
        Weekly.Root.IsVisible = showWeekly;
        _divider.IsVisible = showFiveHour && showWeekly;
    }

    public void SetResetOnNameRow(bool oneRow)
    {
        FiveHour.SetResetOnNameRow(oneRow);
        Weekly.SetResetOnNameRow(oneRow);
    }

    public void ApplyText(UiText text, string fiveHourTitle, string weeklyTitle)
    {
        _caption.Text = text.RemainingCaption;
        FiveHour.Title.Text = fiveHourTitle;
        Weekly.Title.Text = weeklyTitle;
    }

    public void ApplyFontScale(double scale)
    {
        _name.FontSize = 15 * scale;
        _caption.FontSize = 10.5 * scale;
        _credits.FontSize = 11 * scale;
        FiveHour.ApplyFontScale(scale);
        Weekly.ApplyFontScale(scale);
    }

    public void ApplyTheme(OverlayThemePalette palette)
    {
        Root.Background = Brush(palette.LatestCardBackground);
        Root.BorderBrush = Brush(palette.LatestDivider);
        _name.Foreground = Brush(palette.CardTitle);
        _caption.Foreground = Brush(palette.StatusText);
        _credits.Foreground = Brush(palette.StatusText);
        _divider.Background = Brush(palette.LatestDivider);
        _creditsDivider.Background = Brush(palette.LatestDivider);
        FiveHour.ApplyTheme(palette);
        Weekly.ApplyTheme(palette);
    }

    /// <summary>Sets the credits line; an empty text hides it.</summary>
    public void SetCredits(string? credits)
    {
        _credits.Text = credits ?? string.Empty;
        _credits.IsVisible = !string.IsNullOrEmpty(credits);
        _creditsDivider.IsVisible = _credits.IsVisible;
    }

    private static SolidColorBrush Brush(string color) => new(Color.Parse(color));

    internal sealed class LatestLimitRow
    {
        private readonly TextBlock _percentSign;
        private readonly Grid _labels;
        private readonly StackPanel _percent;
        private double _leftHeight;
        private readonly Border _fill;
        private readonly Border _paceDot;
        private double _fillRatio;
        private double? _markerRatio;

        public LatestLimitRow()
        {
            Title = new TextBlock
            {
                FontWeight = FontWeight.Medium,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            Reset = new TextBlock { TextTrimming = TextTrimming.CharacterEllipsis };
            Percent = new FittedNumber { FontFamily = PercentFont };
            _percentSign = new TextBlock
            {
                Text = "%",
                VerticalAlignment = VerticalAlignment.Bottom
            };
            var labels = _labels = new Grid
            {
                VerticalAlignment = VerticalAlignment.Center,
                RowDefinitions = new RowDefinitions("Auto,Auto"),
                ColumnDefinitions = new ColumnDefinitions("Auto,*"),
                Children = { Title, Reset }
            };
            SetResetOnNameRow(false);

            _fill = new Border
            {
                Width = 0,
                CornerRadius = new CornerRadius(3),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            Track = new Border
            {
                Height = TrackHeight,
                CornerRadius = new CornerRadius(TrackHeight / 2),
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 0, TrackBottomInset),
                Child = _fill
            };
            // The even-pace marker is a dot centered on the track. A ring in the panel
            // color sets it apart from both the fill and the empty track.
            _paceDot = new Border
            {
                Width = MarkerWidth,
                Height = MarkerWidth,
                CornerRadius = new CornerRadius(MarkerWidth / 2),
                BorderThickness = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Center
            };
            PaceMarker = new Grid
            {
                Width = MarkerWidth,
                HorizontalAlignment = HorizontalAlignment.Left,
                Background = Brushes.Transparent,
                IsVisible = false,
                Children = { _paceDot }
            };
            // As tall as the pace dot, which overhangs the track equally on both sides.
            var bar = new Grid { Height = MarkerWidth, Children = { Track, PaceMarker } };
            Track.SizeChanged += (_, _) => UpdateGeometry();
            // The name, reset time, and bar sit on the left; the percentage on the
            // right spans their full height, from the name's top to the bar's bottom.
            var left = new StackPanel { Spacing = 4, Children = { labels, bar } };
            _percent = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(14, 0, 0, 0),
                Children = { Percent, _percentSign }
            };
            Root = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                Children = { left, _percent }
            };
            Grid.SetColumn(_percent, 1);
            left.SizeChanged += (_, args) => FitPercent(args.NewSize.Height);
        }

        public Grid Root { get; }

        public TextBlock Title { get; }

        public TextBlock Reset { get; }

        public FittedNumber Percent { get; }

        public Border Track { get; }

        public Grid PaceMarker { get; }

        /// <summary>Puts the reset time after the name on one line, or on its own line below it.</summary>
        public void SetResetOnNameRow(bool oneRow)
        {
            Grid.SetRow(Reset, oneRow ? 0 : 1);
            Grid.SetColumn(Reset, oneRow ? 1 : 0);
            Grid.SetColumnSpan(Reset, oneRow ? 1 : 2);
            Grid.SetColumnSpan(Title, oneRow ? 1 : 2);
            Reset.Margin = oneRow ? new Thickness(10, 0, 0, 0) : default;
            Reset.VerticalAlignment = oneRow ? VerticalAlignment.Center : VerticalAlignment.Top;
        }

        public void ApplyFontScale(double scale)
        {
            Title.FontSize = 13.5 * scale;
            Reset.FontSize = 11.5 * scale;
            _percentSign.FontSize = 11 * scale;
            FitPercent(_leftHeight);
        }

        /// <summary>
        /// Sizes the digits to the left column: their top level with the top of the
        /// name's capitals, their baseline level with the bottom of the bar.
        /// </summary>
        private void FitPercent(double leftHeight)
        {
            _leftHeight = leftHeight;
            // A text line has about a quarter of its font size above the capitals.
            var top = Math.Round(Title.FontSize * 0.24);
            _percent.Margin = new Thickness(14, top, 0, 0);
            Percent.TargetHeight = Math.Max(0, leftHeight - top - TrackBottomInset);
            // Sit the percent sign on the same baseline, below the font's descent.
            _percentSign.Margin = new Thickness(2, 0, 0, TrackBottomInset - Math.Round(_percentSign.FontSize * 0.24));
        }

        public void ApplyTheme(OverlayThemePalette palette)
        {
            Title.Foreground = Brush(palette.CardTitle);
            Reset.Foreground = Brush(palette.StatusText);
            Track.Background = Brush(palette.EmptyCell);
            _paceDot.Background = Brush(palette.CardTitle);
            _paceDot.BorderBrush = Brush(palette.RootBackground);
        }

        /// <summary>Shows a percentage, or a dash when <paramref name="remainingPercent"/> is null.</summary>
        public void SetValue(int? remainingPercent, IBrush signal, IBrush unavailable)
        {
            Percent.Text = remainingPercent is { } value ? value.ToString() : "—";
            Percent.Foreground = remainingPercent is null ? unavailable : signal;
            _percentSign.IsVisible = remainingPercent is not null;
            _percentSign.Foreground = signal;
            _fill.Background = signal;
            _fillRatio = remainingPercent is { } percent ? Math.Clamp(percent, 0, 100) / 100d : 0;
            UpdateGeometry();
        }

        /// <summary>Places the even-pace marker at a share of the bar, or hides it when null.</summary>
        public void SetPaceMarker(double? ratio, string? tooltip)
        {
            _markerRatio = ratio;
            PaceMarker.IsVisible = ratio is not null;
            ToolTip.SetTip(PaceMarker, tooltip);
            UpdateGeometry();
        }

        private void UpdateGeometry()
        {
            var width = Track.Bounds.Width;
            _fill.Width = width * _fillRatio;
            if (_markerRatio is { } ratio)
            {
                var left = Math.Clamp(width * ratio - MarkerWidth / 2, 0, Math.Max(0, width - MarkerWidth));
                PaceMarker.Margin = new Thickness(left, 0, 0, 0);
            }
        }
    }
}
