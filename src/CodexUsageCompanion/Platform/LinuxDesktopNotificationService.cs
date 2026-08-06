using System.ComponentModel;
using System.Diagnostics;
using CodexUsageCompanion.Diagnostics;

namespace CodexUsageCompanion.Platform;

public sealed class LinuxDesktopNotificationService
{
    public void Show(string title, string body, bool critical)
    {
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "notify-send",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            startInfo.ArgumentList.Add("--app-name=Claude Codex Usage Companion");
            startInfo.ArgumentList.Add("--icon=claude-codex-usage-companion");
            startInfo.ArgumentList.Add(
                critical ? "--urgency=critical" : "--urgency=normal");
            startInfo.ArgumentList.Add(title);
            startInfo.ArgumentList.Add(body);
            using var process = Process.Start(startInfo);
        }
        catch (Exception exception) when (
            exception is Win32Exception or
            InvalidOperationException or
            NotSupportedException)
        {
            CompanionLog.Shared.Write("notification", exception);
        }
    }
}
