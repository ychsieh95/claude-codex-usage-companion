namespace CodexUsageCompanion.Configuration;

public sealed record SettingsSectionState
{
    public bool Appearance { get; init; } = true;
    public bool DateTime { get; init; } = true;
    public bool Logging { get; init; } = true;
    public bool Notifications { get; init; } = true;
    public bool Usage { get; init; } = true;
    public bool Window { get; init; } = true;
    public bool Updates { get; init; } = true;

    public bool IsExpanded(string key) => key switch
    {
        "appearance" => Appearance,
        "dateTime" => DateTime,
        "logging" => Logging,
        "notifications" => Notifications,
        "usage" => Usage,
        "window" => Window,
        "updates" => Updates,
        _ => true
    };
}
