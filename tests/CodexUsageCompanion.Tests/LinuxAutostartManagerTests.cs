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
}
