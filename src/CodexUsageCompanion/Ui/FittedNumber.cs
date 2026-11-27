using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;

namespace CodexUsageCompanion.Ui;

/// <summary>
/// Draws a short number whose digits fill <see cref="TargetHeight"/> exactly, from the
/// top of the digits to their baseline. A text block cannot do this, because its box also
/// holds the font's line spacing. The control reserves the width of <see cref="WidthSample"/>
/// so numbers of different lengths keep the same column width.
/// </summary>
internal sealed class FittedNumber : Control
{
    private const double ReferenceSize = 100;
    private string _text = string.Empty;
    private IBrush? _foreground;
    private double _targetHeight;

    public FittedNumber()
    {
        ClipToBounds = false;
    }

    public string Text
    {
        get => _text;
        set
        {
            if (_text == value)
                return;
            _text = value;
            InvalidateVisual();
        }
    }

    public IBrush? Foreground
    {
        get => _foreground;
        set
        {
            _foreground = value;
            InvalidateVisual();
        }
    }

    /// <summary>The height the digits fill, from their top to the baseline.</summary>
    public double TargetHeight
    {
        get => _targetHeight;
        set
        {
            if (Math.Abs(_targetHeight - value) < 0.1)
                return;
            _targetHeight = value;
            InvalidateMeasure();
            InvalidateVisual();
        }
    }

    public string WidthSample { get; init; } = "100";

    public FontWeight FontWeight { get; init; } = FontWeight.SemiBold;

    /// <summary>The digits' font; null uses the inherited font.</summary>
    public FontFamily? FontFamily { get; init; }

    protected override Size MeasureOverride(Size availableSize)
    {
        // The height stays out of the measure, so the number never makes its row taller.
        if (Ink(WidthSample) is not { } sample || Ink("0") is not { } digit || digit.Bounds.Height <= 0)
            return default;
        return new Size(Math.Ceiling(sample.Advance * Scale(digit.Bounds)), 0);
    }

    public override void Render(DrawingContext context)
    {
        if (_foreground is null || Ink(_text) is not { } ink || Ink("0") is not { } digit || digit.Bounds.Height <= 0)
            return;

        // Scale by the digit height, so a dash or other short glyph keeps the digits' size.
        var scale = Scale(digit.Bounds);
        // Right-align by advance width, not ink, so monospaced digits stay in their
        // character cells; put the digits' top at the control's top.
        var width = ink.Advance * scale;
        var matrix = Matrix.CreateTranslation(0, -digit.Bounds.Y) *
                     Matrix.CreateScale(scale, scale) *
                     Matrix.CreateTranslation(Bounds.Width - width, 0);
        using (context.PushTransform(matrix))
            context.DrawGeometry(_foreground, null, ink.Geometry);
    }

    private double Scale(Rect digitBounds) => _targetHeight / digitBounds.Height;

    private (Geometry Geometry, Rect Bounds, double Advance)? Ink(string text)
    {
        if (string.IsNullOrEmpty(text))
            return null;
        var typeface = new Typeface(FontFamily ?? GetValue(TextElement.FontFamilyProperty), FontStyle.Normal, FontWeight);
        var formatted = new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
            typeface, ReferenceSize, Brushes.Black);
        return formatted.BuildGeometry(default) is { } geometry
            ? (geometry, geometry.Bounds, formatted.WidthIncludingTrailingWhitespace)
            : null;
    }
}
