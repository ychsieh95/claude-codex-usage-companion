using System.Buffers.Binary;
using System.IO.Compression;
using CodexUsageCompanion.Configuration;
using CodexUsageCompanion.RateLimits;

namespace CodexUsageCompanion.Ui;

public static class TrayIconRenderer
{
    public const int IconSize = 64;
    private const int Scale = 4;
    private const int CanvasSize = IconSize * Scale;
    private static readonly Rgba Background = new(32, 34, 37, 255);
    private static readonly Rgba Foreground = new(242, 243, 245, 255);

    private static readonly bool[,] DigitSegments =
    {
        { true, true, true, false, true, true, true },
        { false, false, true, false, false, true, false },
        { true, false, true, true, true, false, true },
        { true, false, true, true, false, true, true },
        { false, true, true, true, false, true, false },
        { true, true, false, true, false, true, true },
        { true, true, false, true, true, true, true },
        { true, false, true, false, false, true, false },
        { true, true, true, true, true, true, true },
        { true, true, true, true, false, true, true }
    };

    public static int? ResolveRemainingPercent(
        string? style,
        RateLimitState? claudeState,
        RateLimitState? codexState)
    {
        return TrayIconStyleOptions.Normalize(style) switch
        {
            TrayIconStyleOptions.ClaudeCurrentSession =>
                claudeState?.FiveHour?.RemainingPercent,
            TrayIconStyleOptions.ClaudeWeeklySession =>
                claudeState?.Weekly?.RemainingPercent,
            TrayIconStyleOptions.CodexSession =>
                (codexState?.FiveHour ?? codexState?.Weekly)?.RemainingPercent,
            _ => null
        };
    }

    public static byte[] CreateUsageIcon(int? remainingPercent)
    {
        var pixels = new Rgba[CanvasSize * CanvasSize];
        Array.Fill(pixels, Background);

        var text = remainingPercent is int value
            ? Math.Clamp(value, 0, 100).ToString(System.Globalization.CultureInfo.InvariantCulture)
            : "--";
        DrawText(pixels, text);
        return EncodePng(Downsample(pixels));
    }

    private static void DrawText(Rgba[] pixels, string text)
    {
        var digitCount = text.Length;
        var digitWidth = digitCount switch
        {
            1 => 31,
            2 => 25,
            _ => 18
        };
        var digitHeight = digitCount == 3 ? 42 : 48;
        var spacing = digitCount == 3 ? 2 : 3;
        var totalWidth = digitCount * digitWidth + (digitCount - 1) * spacing;
        var startX = (IconSize - totalWidth) / 2d;
        var startY = (IconSize - digitHeight) / 2d;

        for (var index = 0; index < text.Length; index++)
        {
            var x = startX + index * (digitWidth + spacing);
            if (text[index] == '-')
            {
                DrawSegment(
                    pixels,
                    x + 2,
                    startY + digitHeight / 2d - 2.5,
                    digitWidth - 4,
                    5);
                continue;
            }

            DrawDigit(pixels, text[index] - '0', x, startY, digitWidth, digitHeight);
        }
    }

    private static void DrawDigit(
        Rgba[] pixels,
        int digit,
        double x,
        double y,
        double width,
        double height)
    {
        var thickness = width >= 24 ? 6d : 5d;
        var horizontalInset = thickness * 0.55;
        var verticalLength = (height - thickness * 1.8) / 2d;
        if (DigitSegments[digit, 0]) DrawSegment(pixels, x + horizontalInset, y, width - horizontalInset * 2, thickness);
        if (DigitSegments[digit, 1]) DrawSegment(pixels, x, y + thickness * 0.55, thickness, verticalLength);
        if (DigitSegments[digit, 2]) DrawSegment(pixels, x + width - thickness, y + thickness * 0.55, thickness, verticalLength);
        if (DigitSegments[digit, 3]) DrawSegment(pixels, x + horizontalInset, y + height / 2d - thickness / 2d, width - horizontalInset * 2, thickness);
        if (DigitSegments[digit, 4]) DrawSegment(pixels, x, y + height / 2d + thickness * 0.1, thickness, verticalLength);
        if (DigitSegments[digit, 5]) DrawSegment(pixels, x + width - thickness, y + height / 2d + thickness * 0.1, thickness, verticalLength);
        if (DigitSegments[digit, 6]) DrawSegment(pixels, x + horizontalInset, y + height - thickness, width - horizontalInset * 2, thickness);
    }

    private static void DrawSegment(
        Rgba[] pixels,
        double x,
        double y,
        double width,
        double height)
    {
        var scaledX = x * Scale;
        var scaledY = y * Scale;
        var scaledWidth = width * Scale;
        var scaledHeight = height * Scale;
        var radius = Math.Min(scaledWidth, scaledHeight) * 0.34;
        var left = Math.Max(0, (int)Math.Floor(scaledX));
        var top = Math.Max(0, (int)Math.Floor(scaledY));
        var right = Math.Min(CanvasSize, (int)Math.Ceiling(scaledX + scaledWidth));
        var bottom = Math.Min(CanvasSize, (int)Math.Ceiling(scaledY + scaledHeight));

        for (var py = top; py < bottom; py++)
        {
            for (var px = left; px < right; px++)
            {
                var sampleX = px + 0.5;
                var sampleY = py + 0.5;
                if (InsideRoundedRectangle(
                        sampleX,
                        sampleY,
                        scaledX,
                        scaledY,
                        scaledWidth,
                        scaledHeight,
                        radius))
                {
                    pixels[py * CanvasSize + px] = Foreground;
                }
            }
        }
    }

    private static bool InsideRoundedRectangle(
        double px,
        double py,
        double x,
        double y,
        double width,
        double height,
        double radius)
    {
        var centerX = Math.Clamp(px, x + radius, x + width - radius);
        var centerY = Math.Clamp(py, y + radius, y + height - radius);
        var dx = px - centerX;
        var dy = py - centerY;
        return dx * dx + dy * dy <= radius * radius;
    }

    private static Rgba[] Downsample(Rgba[] source)
    {
        var target = new Rgba[IconSize * IconSize];
        for (var y = 0; y < IconSize; y++)
        {
            for (var x = 0; x < IconSize; x++)
            {
                var red = 0;
                var green = 0;
                var blue = 0;
                var alpha = 0;
                for (var sy = 0; sy < Scale; sy++)
                {
                    for (var sx = 0; sx < Scale; sx++)
                    {
                        var pixel = source[(y * Scale + sy) * CanvasSize + x * Scale + sx];
                        red += pixel.Red;
                        green += pixel.Green;
                        blue += pixel.Blue;
                        alpha += pixel.Alpha;
                    }
                }

                const int sampleCount = Scale * Scale;
                target[y * IconSize + x] = new Rgba(
                    (byte)(red / sampleCount),
                    (byte)(green / sampleCount),
                    (byte)(blue / sampleCount),
                    (byte)(alpha / sampleCount));
            }
        }

        return target;
    }

    private static byte[] EncodePng(Rgba[] pixels)
    {
        using var output = new MemoryStream();
        output.Write([137, 80, 78, 71, 13, 10, 26, 10]);

        Span<byte> header = stackalloc byte[13];
        BinaryPrimitives.WriteUInt32BigEndian(header, IconSize);
        BinaryPrimitives.WriteUInt32BigEndian(header[4..], IconSize);
        header[8] = 8;
        header[9] = 6;
        WriteChunk(output, "IHDR"u8, header);

        using var raw = new MemoryStream();
        for (var y = 0; y < IconSize; y++)
        {
            raw.WriteByte(0);
            for (var x = 0; x < IconSize; x++)
            {
                var pixel = pixels[y * IconSize + x];
                raw.WriteByte(pixel.Red);
                raw.WriteByte(pixel.Green);
                raw.WriteByte(pixel.Blue);
                raw.WriteByte(pixel.Alpha);
            }
        }

        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.SmallestSize, leaveOpen: true))
        {
            raw.Position = 0;
            raw.CopyTo(zlib);
        }

        WriteChunk(output, "IDAT"u8, compressed.ToArray());
        WriteChunk(output, "IEND"u8, []);
        return output.ToArray();
    }

    private static void WriteChunk(Stream output, ReadOnlySpan<byte> type, ReadOnlySpan<byte> data)
    {
        Span<byte> length = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(length, (uint)data.Length);
        output.Write(length);
        output.Write(type);
        output.Write(data);

        var crc = 0xffffffffu;
        foreach (var value in type)
        {
            crc = UpdateCrc(crc, value);
        }

        foreach (var value in data)
        {
            crc = UpdateCrc(crc, value);
        }

        Span<byte> checksum = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(checksum, ~crc);
        output.Write(checksum);
    }

    private static uint UpdateCrc(uint crc, byte value)
    {
        crc ^= value;
        for (var bit = 0; bit < 8; bit++)
        {
            crc = (crc & 1) != 0
                ? 0xedb88320u ^ (crc >> 1)
                : crc >> 1;
        }

        return crc;
    }

    private readonly record struct Rgba(byte Red, byte Green, byte Blue, byte Alpha);
}
