using CodexUsageCompanion.Lifecycle;
using Xunit;

namespace CodexUsageCompanion.Tests;

public sealed class CommandModeTests
{
    [Theory]
    [InlineData("gui", CommandMode.Gui)]
    [InlineData("status", CommandMode.Status)]
    [InlineData("watch", CommandMode.Watch)]
    [InlineData("config", CommandMode.Config)]
    [InlineData("--session-start", CommandMode.SessionStart)]
    [InlineData("--refresh", CommandMode.Refresh)]
    [InlineData("--background", CommandMode.Background)]
    [InlineData("--probe", CommandMode.Probe)]
    [InlineData("--help", CommandMode.Help)]
    public void ParseRecognizesSupportedMode(string argument, CommandMode expected)
    {
        Assert.Equal(expected, CommandModeParser.Parse([argument]));
    }

    [Fact]
    public void MissingCommandOpensGuiAndUnknownCommandIsRejected()
    {
        Assert.Equal(CommandMode.Gui, CommandModeParser.Parse([]));
        Assert.Equal(CommandMode.Unknown, CommandModeParser.Parse(["--other"]));
    }

    [Fact]
    public void WatchOptionsAreValidated()
    {
        var valid = CommandModeParser.ParseOptions(["watch", "--interval", "15", "--json"]);
        var invalid = CommandModeParser.ParseOptions(["watch", "--interval", "1"]);

        Assert.Equal(15, valid.IntervalSeconds);
        Assert.True(valid.Json);
        Assert.Null(valid.Error);
        Assert.NotNull(invalid.Error);
    }

    [Fact]
    public void DetachedLauncherFindsLinuxSetsid()
    {
        var result = DetachedLauncher.FindSetsid(path => path == "/usr/bin/setsid");

        Assert.Equal("/usr/bin/setsid", result);
    }
}
