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

    /// <summary>
    /// Shows a notification with one action button and waits until it closes.
    /// Returns true when the user selects the action, false when the notification closes without it,
    /// and null when notify-send is missing or does not support actions (libnotify before 0.7.10).
    /// </summary>
    public async Task<bool?> ShowWithActionAsync(
        string title, string body, string actionLabel, CancellationToken cancellationToken)
    {
        if (!OperatingSystem.IsLinux())
        {
            return null;
        }

        const string actionKey = "accept";
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "notify-send",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            startInfo.ArgumentList.Add("--app-name=Claude Codex Usage Companion");
            startInfo.ArgumentList.Add("--icon=claude-codex-usage-companion");
            startInfo.ArgumentList.Add("--urgency=normal");
            startInfo.ArgumentList.Add($"--action={actionKey}={actionLabel}");
            startInfo.ArgumentList.Add("--wait");
            startInfo.ArgumentList.Add(title);
            startInfo.ArgumentList.Add(body);
            using var process = Process.Start(startInfo);
            if (process is null)
            {
                return null;
            }

            var output = process.StandardOutput.ReadToEndAsync(cancellationToken);
            _ = process.StandardError.ReadToEndAsync(cancellationToken);
            try
            {
                await process.WaitForExitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                try { process.Kill(); } catch (InvalidOperationException) { }
                throw;
            }

            if (process.ExitCode != 0)
            {
                return null;
            }

            return (await output).Split('\n').Any(line => line.Trim() == actionKey);
        }
        catch (Exception exception) when (
            exception is Win32Exception or
            InvalidOperationException or
            NotSupportedException)
        {
            CompanionLog.Shared.Write("notification", exception);
            return null;
        }
    }
}
