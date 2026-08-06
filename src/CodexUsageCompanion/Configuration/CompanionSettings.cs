namespace CodexUsageCompanion.Configuration;

public sealed record CompanionSettings
{
    public bool ShowFiveHourLimit { get; init; }
    public bool EnableClaudeUsage { get; init; } = true;
    public bool EnableCodexUsage { get; init; } = true;
    public bool EnableSystemTray { get; init; }
    public string TrayIconStyle { get; init; } = TrayIconStyleOptions.Original;
    public bool ShowTaskbarIcon { get; init; } = true;
    public bool StartOnBoot { get; init; }
    public bool MinimizeOnStart { get; init; }
    public string Language { get; init; } =
        SystemLanguageOptions.ResolveDefault(
            System.Globalization.CultureInfo.CurrentUICulture);
    public string Theme { get; init; } = UiThemeOptions.System;
    public bool EnableLowUsageAlert { get; init; }
    public int LowUsageAlertThresholdPercent { get; init; } =
        UsageAlertOptions.DefaultThresholdPercent;
    public bool NotifyOnReset { get; init; }
    public bool EnableUsageLogging { get; init; }
    public string UsageLogFilePath { get; init; } = UsageLogOptions.DefaultFilePath();
    public string UsageLogFormat { get; init; } = UsageLogOptions.Csv;
    public string Position { get; init; } = WindowPosition.RightBottom;
    public double Opacity { get; init; } = 1d;
    public int Margin { get; init; } = 16;
    public bool AlwaysOnTop { get; init; } = true;
    public int RefreshIntervalSeconds { get; init; } = 60;
    public string ResetDateTimeFormat { get; init; } = DateTimeFormatOptions.MonthDay;
    public string LastUpdatedDateTimeFormat { get; init; } = DateTimeFormatOptions.HourMinute;
}
