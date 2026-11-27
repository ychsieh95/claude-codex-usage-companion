using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using CodexUsageCompanion.Localization;

namespace CodexUsageCompanion.Ui;

/// <summary>A provider's current window outside its weekly window, with matching center values and reset labels.</summary>
internal sealed class RingsProviderCard
{
    private readonly TextBlock _name;
    private readonly TextBlock _caption = new();
    private readonly TextBlock _credits = new() { TextWrapping = TextWrapping.Wrap };
    private readonly Grid _dial;
    private readonly RingDrawing _drawing = new();
    private readonly Limit _current = new();
    private readonly Limit _weekly = new();

    public RingsProviderCard(string name, Color brand, Control icon)
    {
        _name = new TextBlock { Text = name, FontWeight = FontWeight.SemiBold };
        var iconBox = new Border
        {
            Width = 28, Height = 28, CornerRadius = new CornerRadius(6),
            Background = new SolidColorBrush(brand, 0.12), Child = icon
        };
        icon.HorizontalAlignment = HorizontalAlignment.Center;
        icon.VerticalAlignment = VerticalAlignment.Center;
        var heading = new StackPanel
        {
            Orientation = Orientation.Horizontal, Spacing = 8,
            Children = { iconBox, _name }
        };
        _name.VerticalAlignment = VerticalAlignment.Center;
        var values = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { _current.Percent, _weekly.Percent, _caption }
        };
        _dial = new Grid { Children = { _drawing, values } };
        var labels = new StackPanel
        {
            Spacing = 12, VerticalAlignment = VerticalAlignment.Center,
            Children = { _current.Labels, _weekly.Labels }
        };
        var body = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*"), Children = { _dial, labels } };
        Grid.SetColumn(labels, 1);
        labels.Margin = new Thickness(12, 0, 0, 0);
        Root = new Border
        {
            CornerRadius = new CornerRadius(8), BorderThickness = new Thickness(1),
            Padding = new Thickness(12),
            Child = new StackPanel { Spacing = 10, Children = { heading, body, _credits } }
        };
    }

    public Border Root { get; }

    public void ApplyVisibility(bool current, bool weekly)
    {
        _current.Visible = current;
        _weekly.Visible = weekly;
        _drawing.CurrentVisible = current;
        _drawing.WeeklyVisible = weekly;
        ApplyShades();
    }

    public void ApplyText(UiText text, string currentTitle, string weeklyTitle)
    {
        _caption.Text = text.RemainingCaption;
        _current.Title.Text = currentTitle;
        _weekly.Title.Text = weeklyTitle;
    }

    public void ApplyFontScale(double scale)
    {
        _dial.Width = _dial.Height = 140 * scale;
        _drawing.Scale = scale;
        _name.FontSize = 15 * scale;
        _caption.FontSize = 9 * scale;
        _credits.FontSize = 11 * scale;
        foreach (var limit in new[] { _current, _weekly })
        {
            limit.Percent.FontSize = 20 * scale;
            limit.Title.FontSize = 12 * scale;
            limit.Reset.FontSize = 11 * scale;
        }
    }

    public void ApplyTheme(OverlayThemePalette palette)
    {
        Root.Background = Brush(palette.LatestCardBackground);
        Root.BorderBrush = Brush(palette.LatestDivider);
        _name.Foreground = Brush(palette.CardTitle);
        _caption.Foreground = _credits.Foreground = Brush(palette.StatusText);
        _drawing.Track = Brush(palette.EmptyCell);
        _drawing.Marker = Brush(palette.CardTitle);
        foreach (var limit in new[] { _current, _weekly })
        {
            limit.Reset.Foreground = Brush(palette.StatusText);
        }
        _drawing.InvalidateVisual();
    }

    // Use the same formatted values and countdowns as Latest, including its unavailable states.
    public void SetLimit(bool weekly, int? remaining, string reset, IBrush signal, double? pace, string? paceTooltip)
    {
        var limit = weekly ? _weekly : _current;
        limit.Percent.Text = remaining is { } value ? $"{value}%" : "—";
        limit.Signal = signal;
        ApplyShades();
        limit.Reset.Text = reset;
        var tooltip = $"{limit.Title.Text}: {limit.Percent.Text}\n{reset}";
        if (paceTooltip is not null)
            tooltip += $"\n{paceTooltip}";
        ToolTip.SetTip(limit.Labels, tooltip);
        ToolTip.SetTip(limit.Percent, tooltip);
        if (weekly)
        {
            _drawing.WeeklyRatio = remaining.GetValueOrDefault() / 100d;
            _drawing.WeeklyPace = pace;
        }
        else
        {
            _drawing.CurrentRatio = remaining.GetValueOrDefault() / 100d;
            _drawing.CurrentPace = pace;
        }
        _drawing.InvalidateVisual();
    }

    /// <summary>
    /// With both rings shown, the inner weekly ring takes a lighter shade, and its
    /// center value, title and legend swatch match it, so each ring is identifiable.
    /// </summary>
    private void ApplyShades()
    {
        var both = _drawing.CurrentVisible && _drawing.WeeklyVisible;
        foreach (var limit in new[] { _current, _weekly })
        {
            var faded = both && limit == _weekly;
            var shade = faded ? Fade(limit.Signal, WeeklyShadeOpacity) : limit.Signal;
            // Text fades less than the ring so it stays readable on light backgrounds.
            limit.Percent.Foreground = limit.Title.Foreground =
                faded ? Fade(limit.Signal, WeeklyTextOpacity) : limit.Signal;
            limit.Swatch.Background = shade;
            if (limit == _weekly) _drawing.WeeklySignal = shade;
            else _drawing.CurrentSignal = shade;
        }
        _drawing.InvalidateVisual();
    }

    public void SetCredits(string? credits)
    {
        _credits.Text = credits;
        _credits.IsVisible = !string.IsNullOrEmpty(credits);
    }

    private static SolidColorBrush Brush(string color) => new(Color.Parse(color));

    internal const double WeeklyShadeOpacity = 0.55;

    internal const double WeeklyTextOpacity = 0.78;

    private static IBrush Fade(IBrush brush, double opacity) => brush is ISolidColorBrush solid
        ? new SolidColorBrush(solid.Color, solid.Opacity * opacity)
        : brush;

    private sealed class Limit
    {
        public TextBlock Percent { get; } = new() { HorizontalAlignment = HorizontalAlignment.Center, FontWeight = FontWeight.SemiBold };
        public TextBlock Title { get; } = new() { FontWeight = FontWeight.Medium, TextWrapping = TextWrapping.Wrap };
        public TextBlock Reset { get; } = new() { TextWrapping = TextWrapping.Wrap };
        public IBrush Signal { get; set; } = Brushes.Gray;
        public Border Swatch { get; } = new() { Width = 4, CornerRadius = new CornerRadius(2) };
        public Grid Labels { get; }

        public Limit()
        {
            var text = new StackPanel { Spacing = 3, Children = { Title, Reset } };
            Grid.SetColumn(text, 1);
            Swatch.Margin = new Thickness(0, 2, 8, 2);
            Labels = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*"), Children = { Swatch, text } };
        }

        public bool Visible
        {
            set { Percent.IsVisible = Labels.IsVisible = value; }
        }
    }

    private sealed class RingDrawing : Control
    {
        public bool CurrentVisible { get; set; }
        public bool WeeklyVisible { get; set; }
        public double CurrentRatio { get; set; }
        public double WeeklyRatio { get; set; }
        public double? CurrentPace { get; set; }
        public double? WeeklyPace { get; set; }
        public double Scale { get; set; } = 1;
        public IBrush Track { get; set; } = Brushes.Gray;
        public IBrush Marker { get; set; } = Brushes.White;
        public IBrush CurrentSignal { get; set; } = Brushes.Gray;
        public IBrush WeeklySignal { get; set; } = Brushes.Gray;

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            var center = new Point(Bounds.Width / 2, Bounds.Height / 2);
            var radius = Math.Max(0, Math.Min(Bounds.Width, Bounds.Height) / 2 - 6 * Scale);
            if (CurrentVisible)
                DrawRing(context, center, radius, CurrentRatio, CurrentSignal, CurrentPace);
            if (WeeklyVisible)
                DrawRing(context, center, CurrentVisible ? Math.Max(0, radius - 14 * Scale) : radius,
                    WeeklyRatio, WeeklySignal, WeeklyPace);
        }

        private void DrawRing(DrawingContext context, Point center, double radius, double ratio, IBrush signal, double? pace)
        {
            var pen = new Pen(signal, 8 * Scale, lineCap: PenLineCap.Round);
            context.DrawEllipse(null, new Pen(Track, 8 * Scale), center, radius, radius);
            ratio = Math.Clamp(ratio, 0, 1);
            if (ratio >= 1)
                context.DrawEllipse(null, pen, center, radius, radius);
            else if (ratio > 0)
            {
                var geometry = new StreamGeometry();
                using (var path = geometry.Open())
                {
                    path.BeginFigure(new Point(center.X, center.Y - radius), false);
                    path.ArcTo(PointAt(center, radius, ratio), new Size(radius, radius), 0,
                        ratio > 0.5, SweepDirection.Clockwise);
                    path.EndFigure(false);
                }
                context.DrawGeometry(null, pen, geometry);
            }
            if (pace is { } marker)
                context.DrawEllipse(Marker, new Pen(Track, Scale), PointAt(center, radius, marker), 3 * Scale, 3 * Scale);
        }

        private static Point PointAt(Point center, double radius, double ratio)
        {
            var angle = ratio * Math.PI * 2 - Math.PI / 2;
            return new Point(center.X + Math.Cos(angle) * radius, center.Y + Math.Sin(angle) * radius);
        }
    }
}
