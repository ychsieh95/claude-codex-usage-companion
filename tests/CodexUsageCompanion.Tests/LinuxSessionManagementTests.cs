using System.Diagnostics;
using CodexUsageCompanion.Platform;
using Xunit;

namespace CodexUsageCompanion.Tests;

[Collection("Process environment")]
public sealed class LinuxSessionManagementTests
{
    [Fact]
    public void DisableForCurrentProcessRemovesXsmpEnvironment()
    {
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        var originalSessionManager = Environment.GetEnvironmentVariable(
            LinuxSessionManagement.SessionManagerVariable);
        var originalAvaloniaSetting = Environment.GetEnvironmentVariable(
            LinuxSessionManagement.AvaloniaSessionManagementVariable);
        var originalRelaunchedSetting = Environment.GetEnvironmentVariable(
            LinuxSessionManagement.RelaunchedVariable);
        try
        {
            Environment.SetEnvironmentVariable(
                LinuxSessionManagement.SessionManagerVariable,
                "local/test-session");
            Environment.SetEnvironmentVariable(
                LinuxSessionManagement.AvaloniaSessionManagementVariable,
                "1");

            LinuxSessionManagement.DisableForCurrentProcess();

            Assert.Null(Environment.GetEnvironmentVariable(
                LinuxSessionManagement.SessionManagerVariable));
            Assert.Equal(
                "0",
                Environment.GetEnvironmentVariable(
                    LinuxSessionManagement.AvaloniaSessionManagementVariable));
            Assert.Equal(
                "1",
                Environment.GetEnvironmentVariable(
                    LinuxSessionManagement.RelaunchedVariable));
        }
        finally
        {
            Environment.SetEnvironmentVariable(
                LinuxSessionManagement.SessionManagerVariable,
                originalSessionManager);
            Environment.SetEnvironmentVariable(
                LinuxSessionManagement.AvaloniaSessionManagementVariable,
                originalAvaloniaSetting);
            Environment.SetEnvironmentVariable(
                LinuxSessionManagement.RelaunchedVariable,
                originalRelaunchedSetting);
        }
    }

    [Fact]
    public void DisableForChildProcessRemovesInheritedXsmpEnvironment()
    {
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        var startInfo = new ProcessStartInfo();
        startInfo.Environment[LinuxSessionManagement.SessionManagerVariable] =
            "local/test-session";
        startInfo.Environment[LinuxSessionManagement.AvaloniaSessionManagementVariable] = "1";

        LinuxSessionManagement.DisableForChildProcess(startInfo);

        Assert.False(startInfo.Environment.ContainsKey(
            LinuxSessionManagement.SessionManagerVariable));
        Assert.Equal(
            "0",
            startInfo.Environment[LinuxSessionManagement.AvaloniaSessionManagementVariable]);
        Assert.Equal(
            "1",
            startInfo.Environment[LinuxSessionManagement.RelaunchedVariable]);
    }

    [Fact]
    public void SanitizedRelaunchPreservesEntryPointAndArguments()
    {
        var startInfo = LinuxSessionManagement.CreateSanitizedStartInfo(
            "/usr/bin/dotnet",
            "/opt/companion/CodexUsageCompanion.dll",
            ["gui", "--background"]);

        Assert.Equal("/usr/bin/dotnet", startInfo.FileName);
        Assert.Equal(
            ["/opt/companion/CodexUsageCompanion.dll", "gui", "--background"],
            startInfo.ArgumentList);
        Assert.False(startInfo.Environment.ContainsKey(
            LinuxSessionManagement.SessionManagerVariable));
        Assert.Equal(
            "0",
            startInfo.Environment[LinuxSessionManagement.AvaloniaSessionManagementVariable]);
        Assert.Equal(
            "1",
            startInfo.Environment[LinuxSessionManagement.RelaunchedVariable]);
    }
}

[CollectionDefinition("Process environment", DisableParallelization = true)]
public sealed class ProcessEnvironmentCollection;
