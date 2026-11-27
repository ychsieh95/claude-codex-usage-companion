using System.Diagnostics;
using CodexUsageCompanion.Diagnostics;
using CodexUsageCompanion.Platform;

namespace CodexUsageCompanion.Lifecycle;

public static class DetachedLauncher
{
    public static bool Start()
    {
        if (OperatingSystem.IsLinux() &&
            string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DISPLAY")) &&
            string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WAYLAND_DISPLAY")))
        {
            CompanionLog.Shared.Write("launcher", "No graphical session is available; skipping GUI startup.");
            return true;
        }

        var executablePath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(executablePath))
        {
            return false;
        }

        try
        {
            var setsid = FindSetsid();
            var startInfo = new ProcessStartInfo
            {
                FileName = setsid ?? executablePath,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(executablePath) ?? Environment.CurrentDirectory
            };
            LinuxSessionManagement.DisableForChildProcess(startInfo);
            if (setsid is not null)
            {
                startInfo.ArgumentList.Add("-f");
                startInfo.ArgumentList.Add(executablePath);
            }

            startInfo.ArgumentList.Add("--background");
            using var process = Process.Start(startInfo);
            return process is not null;
        }
        catch (Exception exception) when (
            exception is InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            CompanionLog.Shared.Write("launcher", exception);
            return false;
        }
    }

    /// <summary>
    /// Starts the GUI again once this process exits, so an installed update can take over
    /// the single-instance socket. A detached shell waits for the current process ID.
    /// </summary>
    public static bool StartAfterExit(string executablePath)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = FindSetsid() ?? "/bin/sh",
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(executablePath) ?? Environment.CurrentDirectory
            };
            LinuxSessionManagement.DisableForChildProcess(startInfo);
            if (startInfo.FileName != "/bin/sh")
            {
                startInfo.ArgumentList.Add("-f");
                startInfo.ArgumentList.Add("/bin/sh");
            }
            startInfo.ArgumentList.Add("-c");
            startInfo.ArgumentList.Add(
                "i=0; while kill -0 \"$1\" 2>/dev/null && [ $i -lt 300 ]; do sleep 0.2; i=$((i+1)); done; exec \"$2\" gui");
            startInfo.ArgumentList.Add("sh");
            startInfo.ArgumentList.Add(Environment.ProcessId.ToString(System.Globalization.CultureInfo.InvariantCulture));
            startInfo.ArgumentList.Add(executablePath);
            using var process = Process.Start(startInfo);
            return process is not null;
        }
        catch (Exception exception) when (
            exception is InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            CompanionLog.Shared.Write("launcher", exception);
            return false;
        }
    }

    public static string? FindSetsid(Func<string, bool>? fileExists = null)
    {
        fileExists ??= File.Exists;
        return new[] { "/usr/bin/setsid", "/bin/setsid" }.FirstOrDefault(fileExists);
    }
}
