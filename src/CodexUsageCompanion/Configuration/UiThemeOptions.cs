namespace CodexUsageCompanion.Configuration;

public static class UiThemeOptions
{
    public const string Dark = "dark";
    public const string Light = "light";
    public const string System = "system";

    public static IReadOnlyList<string> Values { get; } = Array.AsReadOnly(
    [
        Dark,
        Light,
        System
    ]);

    public static string Normalize(string? theme)
    {
        return theme?.ToLowerInvariant() switch
        {
            Dark => Dark,
            Light => Light,
            System => System,
            _ => System
        };
    }
}
