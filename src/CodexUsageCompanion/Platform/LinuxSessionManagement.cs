using System.Diagnostics;

namespace CodexUsageCompanion.Platform;

public static class LinuxSessionManagement
{
    public const string AvaloniaSessionManagementVariable =
        "AVALONIA_X11_USE_SESSION_MANAGEMENT";
    public const string SessionManagerVariable = "SESSION_MANAGER";
    public const string RelaunchedVariable =
        "CLAUDE_CODEX_USAGE_COMPANION_XSMP_DISABLED";

    public static bool RequiresSanitizedRelaunch()
    {
        return OperatingSystem.IsLinux() &&
            !string.IsNullOrWhiteSpace(
                Environment.GetEnvironmentVariable(SessionManagerVariable)) &&
            !string.Equals(
                Environment.GetEnvironmentVariable(RelaunchedVariable),
                "1",
                StringComparison.Ordinal);
    }

    public static bool RelaunchCurrentProcess(IReadOnlyList<string> args)
    {
        ArgumentNullException.ThrowIfNull(args);

        var executablePath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(executablePath))
        {
            return false;
        }

        try
        {
            var startInfo = CreateSanitizedStartInfo(
                executablePath,
                Environment.GetCommandLineArgs().FirstOrDefault(),
                args);
            using var process = Process.Start(startInfo);
            return process is not null;
        }
        catch (Exception exception) when (
            exception is InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            return false;
        }
    }

    internal static ProcessStartInfo CreateSanitizedStartInfo(
        string executablePath,
        string? commandLineEntryPoint,
        IReadOnlyList<string> args)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(executablePath);
        ArgumentNullException.ThrowIfNull(args);

        var startInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            UseShellExecute = false,
            WorkingDirectory = Environment.CurrentDirectory
        };

        // Framework-dependent launches use the dotnet host and need the managed
        // entry point repeated before the original application arguments.
        if (!string.IsNullOrWhiteSpace(commandLineEntryPoint) &&
            string.Equals(
                Path.GetExtension(commandLineEntryPoint),
                ".dll",
                StringComparison.OrdinalIgnoreCase))
        {
            startInfo.ArgumentList.Add(commandLineEntryPoint);
        }

        foreach (var argument in args)
        {
            startInfo.ArgumentList.Add(argument);
        }

        DisableForChildProcess(startInfo);
        startInfo.Environment[RelaunchedVariable] = "1";
        return startInfo;
    }

    public static void DisableForCurrentProcess()
    {
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        // This utility has no documents or session state that a desktop session
        // manager needs to restore. Avalonia 12.1 still opens an XSMP connection
        // when only EnableSessionManagement is false, so SESSION_MANAGER must be
        // removed before the X11/XWayland backend is initialized.
        Environment.SetEnvironmentVariable(SessionManagerVariable, null);
        Environment.SetEnvironmentVariable(AvaloniaSessionManagementVariable, "0");
        Environment.SetEnvironmentVariable(RelaunchedVariable, "1");
    }

    public static void DisableForChildProcess(ProcessStartInfo startInfo)
    {
        ArgumentNullException.ThrowIfNull(startInfo);
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        startInfo.Environment.Remove(SessionManagerVariable);
        startInfo.Environment[AvaloniaSessionManagementVariable] = "0";
        startInfo.Environment[RelaunchedVariable] = "1";
    }
}
