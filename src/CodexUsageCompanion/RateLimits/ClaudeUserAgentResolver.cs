using System.ComponentModel;
using System.Diagnostics;

namespace CodexUsageCompanion.RateLimits;

public static class ClaudeUserAgentResolver
{
    private const string FallbackVersion = "2.0.0";
    private static readonly Lazy<string> Cached = new(() => Resolve(ClaudeExecutableLocator.Find, TryReadInstalledVersion));

    public static string Resolve() => Cached.Value;

    public static string Resolve(Func<string?> findExecutable, Func<string, string?> readVersion)
    {
        var executable = findExecutable();
        var version = string.IsNullOrWhiteSpace(executable) ? null : readVersion(executable);
        return $"claude-cli/{version ?? FallbackVersion}";
    }

    private static string? TryReadInstalledVersion(string executable)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = executable,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            startInfo.ArgumentList.Add("--version");
            using var process = Process.Start(startInfo);
            if (process is null)
            {
                return null;
            }

            var outputTask = process.StandardOutput.ReadToEndAsync();
            if (!process.WaitForExit(2000))
            {
                process.Kill(true);
                return null;
            }

            var output = outputTask.GetAwaiter().GetResult();
            var token = output.Split(
                (char[]?)null,
                StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            return string.IsNullOrWhiteSpace(token) ? null : token;
        }
        catch (Exception exception) when (
            exception is Win32Exception or InvalidOperationException or NotSupportedException)
        {
            return null;
        }
    }
}
