using CodexUsageCompanion.Platform;
using Xunit;

namespace CodexUsageCompanion.Tests;

public sealed class LinuxAutostartManagerTests
{
    [Fact]
    public void EnableWritesUserDesktopEntryAndDisableRemovesIt()
    {
        var directory = Path.Combine(
            Path.GetTempPath(),
            $"CodexUsageCompanion.Tests.{Guid.NewGuid():N}");
        var path = Path.Combine(directory, LinuxAutostartManager.DesktopFileName);
        try
        {
            LinuxAutostartManager.SetEnabled(
                enabled: true,
                "/opt/codex usage/claude-codex-usage-companion",
                path);

            var content = File.ReadAllText(path);
            Assert.Contains("[Desktop Entry]", content);
            Assert.Contains(
                "Exec=/usr/bin/env -u SESSION_MANAGER " +
                "AVALONIA_X11_USE_SESSION_MANAGEMENT=0 " +
                "\"/opt/codex usage/claude-codex-usage-companion\" --background",
                content);
            Assert.Contains("X-GNOME-Autostart-enabled=true", content);

            LinuxAutostartManager.SetEnabled(
                enabled: false,
                "/opt/codex usage/claude-codex-usage-companion",
                path);

            Assert.False(File.Exists(path));
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }
    }

    [Fact]
    public void EnableRejectsRelativeExecutablePath()
    {
        Assert.Throws<ArgumentException>(() =>
            LinuxAutostartManager.SetEnabled(
                enabled: true,
                "claude-codex-usage-companion",
                Path.Combine(Path.GetTempPath(), "unused.desktop")));
    }

    [Fact]
    public void ResolveLaunchPathUsesStableSnapLauncherInsideSnap()
    {
        Assert.Equal(
            "/snap/bin/claude-codex-usage-companion",
            LinuxAutostartManager.ResolveLaunchPath(
                "/snap/claude-codex-usage-companion/x3/bin/CodexUsageCompanion",
                "/snap/claude-codex-usage-companion/x3",
                "claude-codex-usage-companion"));
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "claude-codex-usage-companion")]
    [InlineData("/snap/other-app/12", "other-app")]
    public void ResolveLaunchPathKeepsPathOutsideSnap(string? snapDirectory, string? snapInstanceName)
    {
        const string executablePath = "/usr/lib/claude-codex-usage-companion/CodexUsageCompanion";
        Assert.Equal(
            executablePath,
            LinuxAutostartManager.ResolveLaunchPath(executablePath, snapDirectory, snapInstanceName));
    }
}
