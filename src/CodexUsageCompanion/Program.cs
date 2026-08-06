using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Avalonia;
using CodexUsageCompanion.Configuration;
using CodexUsageCompanion.Lifecycle;
using CodexUsageCompanion.Localization;
using CodexUsageCompanion.Platform;
using CodexUsageCompanion.RateLimits;
using CodexUsageCompanion.Ui;

namespace CodexUsageCompanion;

public static class Program
{
    private const string ProductName = "Claude Codex Usage Companion";

    [STAThread]
    public static int Main(string[] args)
    {
        ConfigureLinuxHarfBuzz();

        var options = CommandModeParser.ParseOptions(args);
        if (options.Error is not null)
        {
            Console.Error.WriteLine(options.Error);
            Console.Error.WriteLine("Run 'claude-codex-usage-companion --help' for usage.");
            return 2;
        }

        return options.Mode switch
        {
            CommandMode.Gui or CommandMode.Background => RunGui(args),
            CommandMode.Status or CommandMode.Probe => RunStatusAsync(options).GetAwaiter().GetResult(),
            CommandMode.Watch => RunWatchAsync(options).GetAwaiter().GetResult(),
            CommandMode.Config => ShowConfig(),
            CommandMode.SessionStart => HandleHookAsync(waitForReady: false).GetAwaiter().GetResult(),
            CommandMode.Refresh => HandleHookAsync(waitForReady: true).GetAwaiter().GetResult(),
            CommandMode.Help => ShowHelp(),
            CommandMode.Version => ShowVersion(),
            _ => UnknownCommand(args)
        };
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new X11PlatformOptions
            {
                EnableSessionManagement = false
            })
            .LogToTrace();

    private static void ConfigureLinuxHarfBuzz()
    {
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        NativeLibrary.SetDllImportResolver(
            typeof(HarfBuzzSharp.Blob).Assembly,
            static (libraryName, _, _) =>
            {
                if (!string.Equals(libraryName, "libHarfBuzzSharp", StringComparison.Ordinal))
                {
                    return IntPtr.Zero;
                }

                return NativeLibrary.TryLoad("libharfbuzz.so.0", out var handle)
                    ? handle
                    : IntPtr.Zero;
            });
    }

    private static int RunGui(string[] args)
    {
        // Native X11 libraries can observe SESSION_MANAGER before managed
        // environment changes take effect. Re-enter once with a clean initial
        // environment before any Avalonia platform initialization occurs.
        if (LinuxSessionManagement.RequiresSanitizedRelaunch())
        {
            return LinuxSessionManagement.RelaunchCurrentProcess(args) ? 0 : 1;
        }

        LinuxSessionManagement.DisableForCurrentProcess();

        var coordinator = new InstanceCoordinator();
        if (coordinator.Signal("activate"))
        {
            return 0;
        }

        using var lease = coordinator.TryAcquireResident();
        if (lease is null)
        {
            return coordinator.Signal("activate") ? 0 : 1;
        }

        GuiLaunchContext.ResidentLease = lease;
        GuiLaunchContext.LaunchedInBackground = args.Contains("--background");
        try
        {
            var guiArgs = args.Where(argument => argument is not "gui" and not "--background").ToArray();
            return BuildAvaloniaApp().StartWithClassicDesktopLifetime(guiArgs);
        }
        finally
        {
            GuiLaunchContext.ResidentLease = null;
            GuiLaunchContext.LaunchedInBackground = false;
        }
    }

    private static async Task<int> RunStatusAsync(CommandOptions options)
    {
        var settings = CompanionSettingsStore.Load();
        var (codexState, codexError) = await ReadCodexUsageAsync(settings, CancellationToken.None);
        var (claudeState, claudeError) = await ReadClaudeUsageAsync(settings, CancellationToken.None);

        if (options.Json)
        {
            var payload = BuildStatusPayload(
                settings.EnableClaudeUsage, claudeState, claudeError,
                settings.EnableCodexUsage, codexState, codexError);
            Console.WriteLine(payload.ToJsonString(JsonDefaults.Output));
        }
        else
        {
            var text = ResolveUiText(settings);
            var wroteAny = false;
            if (settings.EnableClaudeUsage)
            {
                ConsoleUsageRenderer.WriteClaude(claudeState, claudeError, text, UseColor(options));
                wroteAny = true;
            }

            if (settings.EnableCodexUsage)
            {
                if (wroteAny)
                {
                    Console.WriteLine();
                }

                ConsoleUsageRenderer.WriteCodex(codexState, codexError, text, UseColor(options));
                wroteAny = true;
            }

            if (!wroteAny)
            {
                Console.WriteLine("No usage providers enabled. Enable Claude and/or Codex in Settings.");
            }
        }

        if (settings.EnableCodexUsage && codexError is not null)
        {
            Console.Error.WriteLine($"claude-codex-usage-companion: {codexError}");
        }

        var exitCode = (settings.EnableCodexUsage && codexError is not null) ||
                       (settings.EnableClaudeUsage && claudeError is not null)
            ? 1
            : 0;
        return exitCode;
    }

    private static async Task<(RateLimitState? State, string? Error)> ReadCodexUsageAsync(
        CompanionSettings settings,
        CancellationToken cancellationToken)
    {
        if (!settings.EnableCodexUsage)
        {
            return (null, null);
        }

        try
        {
            await using var client = new CodexAppServerClient();
            return (await client.ReadRateLimitsAsync(cancellationToken), null);
        }
        catch (Exception exception)
        {
            return (null, FriendlyError(exception));
        }
    }

    private static async Task<(RateLimitState? State, string? Error)> ReadClaudeUsageAsync(
        CompanionSettings settings,
        CancellationToken cancellationToken)
    {
        if (!settings.EnableClaudeUsage)
        {
            return (null, null);
        }

        try
        {
            await using var client = new ClaudeUsageClient();
            return (await client.ReadUsageAsync(cancellationToken), null);
        }
        catch (Exception exception)
        {
            return (null, FriendlyError(exception));
        }
    }

    private static JsonObject BuildStatusPayload(
        bool claudeEnabled,
        RateLimitState? claudeState,
        string? claudeError,
        bool codexEnabled,
        RateLimitState? codexState,
        string? codexError)
    {
        var payload = new JsonObject();
        if (claudeEnabled)
        {
            payload["claude"] = SerializeProvider(claudeState, claudeError);
        }

        if (codexEnabled)
        {
            payload["codex"] = SerializeProvider(codexState, codexError);
        }

        return payload;
    }

    private static JsonObject SerializeProvider(RateLimitState? state, string? error)
    {
        return new JsonObject
        {
            ["state"] = state is null ? null : JsonSerializer.SerializeToNode(state, JsonDefaults.Output),
            ["error"] = error
        };
    }

    private static async Task<int> RunWatchAsync(CommandOptions options)
    {
        using var cancellation = new CancellationTokenSource();
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellation.Cancel();
        };

        var settings = CompanionSettingsStore.Load();
        if (!settings.EnableCodexUsage && !settings.EnableClaudeUsage)
        {
            Console.Error.WriteLine(
                "claude-codex-usage-companion: No usage providers enabled. Enable Claude and/or Codex in Settings.");
            return 1;
        }

        var text = ResolveUiText(settings);
        RateLimitState? codexState = null;
        string? codexError = null;
        RateLimitState? claudeState = null;
        string? claudeError = null;
        DateTimeOffset? lastClaudeFetchAt = null;

        try
        {
            await using var codexClient = settings.EnableCodexUsage ? new CodexAppServerClient() : null;
            await using var claudeClient = settings.EnableClaudeUsage ? new ClaudeUsageClient() : null;
            while (!cancellation.IsCancellationRequested)
            {
                if (codexClient is not null)
                {
                    try
                    {
                        codexState = await codexClient.ReadRateLimitsAsync(cancellation.Token);
                        codexError = null;
                    }
                    catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception exception)
                    {
                        codexError = FriendlyError(exception);
                    }
                }

                if (claudeClient is not null &&
                    ClaudeUsagePolicy.ShouldFetch(DateTimeOffset.UtcNow, lastClaudeFetchAt))
                {
                    lastClaudeFetchAt = DateTimeOffset.UtcNow;
                    try
                    {
                        claudeState = await claudeClient.ReadUsageAsync(cancellation.Token);
                        claudeError = null;
                    }
                    catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception exception)
                    {
                        claudeError = FriendlyError(exception);
                    }
                }

                if (options.Json)
                {
                    var payload = BuildStatusPayload(
                        settings.EnableClaudeUsage, claudeState, claudeError,
                        settings.EnableCodexUsage, codexState, codexError);
                    payload["observedAt"] = JsonValue.Create(DateTimeOffset.Now);
                    Console.WriteLine(payload.ToJsonString(JsonDefaults.Output));
                }
                else
                {
                    if (!Console.IsOutputRedirected)
                    {
                        Console.Write("\u001b[2J\u001b[H");
                    }

                    var wroteAny = false;
                    if (settings.EnableClaudeUsage)
                    {
                        ConsoleUsageRenderer.WriteClaude(claudeState, claudeError, text, UseColor(options));
                        wroteAny = true;
                    }

                    if (settings.EnableCodexUsage)
                    {
                        if (wroteAny)
                        {
                            Console.WriteLine();
                        }

                        ConsoleUsageRenderer.WriteCodex(codexState, codexError, text, UseColor(options));
                    }

                    Console.WriteLine();
                    Console.WriteLine($"Updated {DateTimeOffset.Now:t} · Ctrl+C to stop");
                }

                await Task.Delay(TimeSpan.FromSeconds(options.IntervalSeconds), cancellation.Token);
            }
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"claude-codex-usage-companion: {FriendlyError(exception)}");
            return 1;
        }

        return 0;
    }

    private static int ShowConfig()
    {
        var path = CompanionSettingsStore.GetDefaultPath();
        var settings = CompanionSettingsStore.Load(path);
        Console.WriteLine(path);
        Console.WriteLine(JsonSerializer.Serialize(settings, JsonDefaults.Output));
        return 0;
    }

    private static async Task<int> HandleHookAsync(bool waitForReady)
    {
        var coordinator = new InstanceCoordinator();
        if (coordinator.Signal("refresh"))
        {
            return 0;
        }

        if (!DetachedLauncher.Start())
        {
            return 1;
        }

        if (!waitForReady)
        {
            return 0;
        }

        var timeoutAt = DateTimeOffset.UtcNow + ResidentStartupPolicy.SignalWaitTimeout;
        while (DateTimeOffset.UtcNow < timeoutAt)
        {
            await Task.Delay(100);
            if (coordinator.Signal("refresh"))
            {
                return 0;
            }
        }

        return 1;
    }

    private static int ShowHelp()
    {
        Console.WriteLine(
            """
            Claude Codex Usage Companion for Linux

            Usage:
              claude-codex-usage-companion [gui]
              claude-codex-usage-companion status [--json] [--no-color]
              claude-codex-usage-companion watch [--interval SECONDS] [--json] [--no-color]
              claude-codex-usage-companion config

            Commands:
              gui       Open the desktop companion (default).
              status    Print the current five-hour and weekly limits once.
              watch     Continuously refresh usage in the terminal.
              config    Print the settings file path and effective settings.

            Environment:
              CLAUDE_CREDENTIALS_PATH  Absolute path to Claude Code's credentials file.
              CLAUDE_CLI_PATH          Absolute path to the Claude CLI.
              CODEX_CLI_PATH           Absolute path to the Codex CLI.
              PLUGIN_DATA              Writable plugin data directory when launched by Codex.
              NO_COLOR                 Disable ANSI colors in terminal output.
            """);
        return 0;
    }

    private static int ShowVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.0.0";
        Console.WriteLine($"{ProductName} {version}");
        return 0;
    }

    private static int UnknownCommand(IReadOnlyList<string> args)
    {
        Console.Error.WriteLine($"Unknown command: {(args.Count == 0 ? string.Empty : args[0])}");
        Console.Error.WriteLine("Run 'claude-codex-usage-companion --help' for usage.");
        return 2;
    }

    private static UiText ResolveUiText(CompanionSettings settings)
    {
        return UiText.For(
            UiLanguageResolver.Resolve(settings.Language, CultureInfo.CurrentUICulture),
            settings.ResetDateTimeFormat,
            settings.LastUpdatedDateTimeFormat);
    }

    private static bool UseColor(CommandOptions options) =>
        !options.NoColor &&
        !Console.IsOutputRedirected &&
        string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NO_COLOR"));

    internal static string FriendlyError(Exception exception)
    {
        if (exception is FileNotFoundException)
        {
            return "Codex CLI was not found. Install Codex CLI or set CODEX_CLI_PATH.";
        }

        if (exception is System.Net.Http.HttpRequestException)
        {
            return "Claude usage is temporarily unavailable.";
        }

        return exception.Message;
    }
}

internal static class JsonDefaults
{
    internal static JsonSerializerOptions Output { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
}
