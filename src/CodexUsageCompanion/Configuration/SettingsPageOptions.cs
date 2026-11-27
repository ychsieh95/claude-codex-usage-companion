namespace CodexUsageCompanion.Configuration;

/// <summary>The Settings window pages, in sidebar order. The window reopens on the last page.</summary>
public static class SettingsPageOptions
{
    public const string Usage = "usage";
    public const string Appearance = "appearance";
    public const string Window = "window";
    public const string Notifications = "notifications";
    public const string DateTime = "dateTime";
    public const string Logging = "logging";
    public const string Updates = "updates";

    public static IReadOnlyList<string> Values { get; } =
        Array.AsReadOnly([Usage, Appearance, Window, Notifications, DateTime, Logging, Updates]);

    public static string Normalize(string? value) =>
        Values.FirstOrDefault(page => string.Equals(page, value, StringComparison.OrdinalIgnoreCase)) ?? Usage;
}
