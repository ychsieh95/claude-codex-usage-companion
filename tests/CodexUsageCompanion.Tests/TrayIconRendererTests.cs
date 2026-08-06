using CodexUsageCompanion.Configuration;
using CodexUsageCompanion.RateLimits;
using CodexUsageCompanion.Ui;
using System.IO.Compression;
using Xunit;

namespace CodexUsageCompanion.Tests;

public sealed class TrayIconRendererTests
{
    [Fact]
    public void ResolvesEachConfiguredUsageWindow()
    {
        var claude = new RateLimitState(
            new RateLimitWindowState(72, 300, null),
            new RateLimitWindowState(61, 10080, null),
            null);
        var codex = new RateLimitState(
            new RateLimitWindowState(40, 300, null),
            new RateLimitWindowState(35, 10080, null),
            null);

        Assert.Equal(
            72,
            TrayIconRenderer.ResolveRemainingPercent(
                TrayIconStyleOptions.ClaudeCurrentSession,
                claude,
                codex));
        Assert.Equal(
            61,
            TrayIconRenderer.ResolveRemainingPercent(
                TrayIconStyleOptions.ClaudeWeeklySession,
                claude,
                codex));
        Assert.Equal(
            40,
            TrayIconRenderer.ResolveRemainingPercent(
                TrayIconStyleOptions.CodexSession,
                claude,
                codex));
        Assert.Null(
            TrayIconRenderer.ResolveRemainingPercent(
                TrayIconStyleOptions.Original,
                claude,
                codex));
    }

    [Fact]
    public void CodexSessionFallsBackToWeeklyUsage()
    {
        var codex = new RateLimitState(
            null,
            new RateLimitWindowState(38, 10080, null),
            null);

        Assert.Equal(
            38,
            TrayIconRenderer.ResolveRemainingPercent(
                TrayIconStyleOptions.CodexSession,
                null,
                codex));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(40)]
    [InlineData(100)]
    public void CreatesA64PixelPngWithDarkBackground(int? remainingPercent)
    {
        var png = TrayIconRenderer.CreateUsageIcon(remainingPercent);

        Assert.Equal(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, png[..8]);
        Assert.Equal(64, ReadBigEndianInt32(png, 16));
        Assert.Equal(64, ReadBigEndianInt32(png, 20));
        Assert.True(png.Length > 100);
        var rawPixels = DecompressImageData(png);
        Assert.Equal(TrayIconRenderer.IconSize * (TrayIconRenderer.IconSize * 4 + 1), rawPixels.Length);
        Assert.Equal(new byte[] { 0, 32, 34, 37, 255 }, rawPixels[..5]);
        Assert.True(rawPixels.AsSpan().IndexOf(new byte[] { 242, 243, 245, 255 }) >= 0);
    }

    [Theory]
    [InlineData(null, TrayIconStyleOptions.Original)]
    [InlineData("unknown", TrayIconStyleOptions.Original)]
    [InlineData("CODEX-SESSION", TrayIconStyleOptions.CodexSession)]
    [InlineData(TrayIconStyleOptions.CodexSession, TrayIconStyleOptions.CodexSession)]
    public void NormalizesTrayIconStyle(string? value, string expected)
    {
        Assert.Equal(expected, TrayIconStyleOptions.Normalize(value));
    }

    private static int ReadBigEndianInt32(byte[] bytes, int offset)
    {
        return System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(
            bytes.AsSpan(offset, sizeof(int)));
    }

    private static byte[] DecompressImageData(byte[] png)
    {
        var offset = 8;
        using var compressed = new MemoryStream();
        while (offset < png.Length)
        {
            var length = ReadBigEndianInt32(png, offset);
            var type = System.Text.Encoding.ASCII.GetString(png, offset + 4, 4);
            if (type == "IDAT")
            {
                compressed.Write(png, offset + 8, length);
            }

            offset += 12 + length;
            if (type == "IEND")
            {
                break;
            }
        }

        compressed.Position = 0;
        using var zlib = new ZLibStream(compressed, CompressionMode.Decompress);
        using var raw = new MemoryStream();
        zlib.CopyTo(raw);
        return raw.ToArray();
    }
}
