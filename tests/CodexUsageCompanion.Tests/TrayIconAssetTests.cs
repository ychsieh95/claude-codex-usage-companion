using System.Text;
using Xunit;

namespace CodexUsageCompanion.Tests;

public sealed class TrayIconAssetTests
{
    [Fact]
    public void TrayIconIsEmbeddedAsAnAvaloniaResource()
    {
        var assembly = typeof(App).Assembly;
        const string resourceName = "!AvaloniaResources";
        Assert.Contains(resourceName, assembly.GetManifestResourceNames());

        using var stream = assembly.GetManifestResourceStream(resourceName);
        Assert.NotNull(stream);
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);

        var assetPath = Encoding.UTF8.GetBytes(
            "/Assets/claude-codex-usage-companion.png");
        Assert.True(buffer.ToArray().AsSpan().IndexOf(assetPath) >= 0);
    }
}
