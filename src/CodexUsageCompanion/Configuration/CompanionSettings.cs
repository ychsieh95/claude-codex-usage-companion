using System.Text.Json.Serialization;

namespace CodexUsageCompanion.Configuration;

public sealed record CompanionSettings
{
    public bool ShowClaudeSession { get; init; } = true;
    public bool ShowClaudeWeekly { get; init; } = true;
    public bool ShowCodexFiveHour { get; init; }
    public bool ShowCodexWeekly { get; init; } = true;
    [JsonPropertyName("showFiveHourLimit")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? LegacyShowFiveHourLimit { get; init; }
    public bool EnableClaudeUsage { get; init; } = true;
    public bool EnableCodexUsage { get; init; } = true;
    public bool EnableSystemTray { get; init; }
    public string TrayIconStyle { get; init; } = TrayIconStyleOptions.Original;
    public bool ShowTaskbarIcon { get; init; } = true;
    public bool EnableAutoUpdateCheck { get; init; } = true;
    public bool StartOnBoot { get; init; }
    public bool MinimizeOnStart { get; init; }
    public string Language { get; init; } =
        SystemLanguageOptions.ResolveDefault(
            System.Globalization.CultureInfo.CurrentUICulture);
    public string Theme { get; init; } = UiThemeOptions.System;
    public SettingsSectionState SettingsSections { get; init; } = new();
    public double FontScale { get; init; } = OverlayLayoutOptions.DefaultFontScale;
    public double ClaudeSessionCardHeight { get; init; } = OverlayLayoutOptions.FiveHourCardHeight;
    public double ClaudeWeeklyCardHeight { get; init; } = OverlayLayoutOptions.WeeklyCardHeight;
    public double CodexFiveHourCardHeight { get; init; } = OverlayLayoutOptions.FiveHourCardHeight;
    public double CodexWeeklyCardHeight { get; init; } = OverlayLayoutOptions.WeeklyCardHeight;
    public string BackgroundColor { get; init; } = string.Empty;
    public int PercentageTimeSpacing { get; init; } = OverlayAppearanceOptions.DefaultSpacing;
    public int LabelPercentageSpacing { get; init; } = OverlayAppearanceOptions.DefaultLabelPercentageSpacing;
    public bool EnableLowUsageAlert { get; init; }
    public int LowUsageAlertThresholdPercent { get; init; } =
        UsageAlertOptions.DefaultThresholdPercent;
    public bool NotifyOnReset { get; init; }
    public bool EnableUsageLogging { get; init; }
    public string UsageLogFilePath { get; init; } = UsageLogOptions.DefaultFilePath();
    public string UsageLogFormat { get; init; } = UsageLogOptions.Csv;
    public string Position { get; init; } = WindowPosition.RightBottom;
    public int? CustomPositionX { get; init; }
    public int? CustomPositionY { get; init; }
    public double Opacity { get; init; } = 1d;
    public int Margin { get; init; } = 16;
    public bool AlwaysOnTop { get; init; } = true;
    public int RefreshIntervalSeconds { get; init; } = 60;
    public string ResetDateTimeFormat { get; init; } = DateTimeFormatOptions.MonthDay;
    public string LastUpdatedDateTimeFormat { get; init; } = DateTimeFormatOptions.HourMinute;
}
