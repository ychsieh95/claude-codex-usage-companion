using CodexUsageCompanion.Platform;
using Xunit;

namespace CodexUsageCompanion.Tests;

public sealed class LinuxTrayTooltipBridgeTests
{
    [Fact]
    public void SplitsTitleFromMultilineDescription()
    {
        var result = LinuxTrayTooltipBridge.SplitText(
            "Claude Codex Usage Companion\r\nWeekly: 58% remaining\nResets Aug 6");

        Assert.Equal("Claude Codex Usage Companion", result.Title);
        Assert.Equal("Weekly: 58% remaining\nResets Aug 6", result.Description);
    }

    [Fact]
    public void KeepsSingleLineTextAsTitle()
    {
        var result = LinuxTrayTooltipBridge.SplitText("Claude Codex Usage Companion");

        Assert.Equal("Claude Codex Usage Companion", result.Title);
        Assert.Equal(string.Empty, result.Description);
    }
}
