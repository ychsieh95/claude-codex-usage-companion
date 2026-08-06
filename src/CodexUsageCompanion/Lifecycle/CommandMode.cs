namespace CodexUsageCompanion.Lifecycle;

public enum CommandMode
{
    Gui,
    Status,
    Watch,
    Config,
    SessionStart,
    Refresh,
    Background,
    Probe,
    Help,
    Version,
    Unknown
}

public sealed record CommandOptions(
    CommandMode Mode,
    bool Json = false,
    bool NoColor = false,
    int IntervalSeconds = 60,
    string? Error = null);

public static class CommandModeParser
{
    public static CommandMode Parse(IReadOnlyList<string> arguments) => ParseOptions(arguments).Mode;

    public static CommandOptions ParseOptions(IReadOnlyList<string> arguments)
    {
        if (arguments.Count == 0)
        {
            return new CommandOptions(CommandMode.Gui);
        }

        var mode = arguments[0] switch
        {
            "gui" => CommandMode.Gui,
            "status" => CommandMode.Status,
            "watch" => CommandMode.Watch,
            "config" => CommandMode.Config,
            "--session-start" => CommandMode.SessionStart,
            "--refresh" => CommandMode.Refresh,
            "--background" => CommandMode.Background,
            "--probe" => CommandMode.Probe,
            "--help" or "-h" or "help" => CommandMode.Help,
            "--version" or "-V" => CommandMode.Version,
            _ => CommandMode.Unknown
        };

        var json = mode == CommandMode.Probe;
        var noColor = false;
        var interval = 60;
        for (var index = 1; index < arguments.Count; index++)
        {
            switch (arguments[index])
            {
                case "--json":
                    json = true;
                    break;
                case "--no-color":
                    noColor = true;
                    break;
                case "--interval" when index + 1 < arguments.Count &&
                                       int.TryParse(arguments[index + 1], out var value) &&
                                       value is >= 2 and <= 3600:
                    interval = value;
                    index++;
                    break;
                case "--interval":
                    return new CommandOptions(mode, json, noColor, interval, "--interval must be between 2 and 3600 seconds.");
                default:
                    return new CommandOptions(mode, json, noColor, interval, $"Unknown option: {arguments[index]}");
            }
        }

        return new CommandOptions(mode, json, noColor, interval);
    }
}
