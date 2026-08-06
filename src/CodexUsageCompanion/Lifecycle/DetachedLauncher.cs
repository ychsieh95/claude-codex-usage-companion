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

    public static string? FindSetsid(Func<string, bool>? fileExists = null)
    {
        fileExists ??= File.Exists;
        return new[] { "/usr/bin/setsid", "/bin/setsid" }.FirstOrDefault(fileExists);
    }
}
