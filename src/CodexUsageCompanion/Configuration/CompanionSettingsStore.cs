using System.IO;
using System.Text.Json;
using CodexUsageCompanion.Platform;

namespace CodexUsageCompanion.Configuration;

public static class CompanionSettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    // Autostart file names used before each rename, most recent first.
    private static readonly string[] LegacyAutostartDesktopFileNames =
    [
        "codex-claude-usage-companion.desktop",
        "codex-usage-companion.desktop"
    ];

    public static CompanionSettings Load(string? path = null)
    {
        var settingsPath = path ?? GetDefaultPath();
        if (path is null &&
            string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("PLUGIN_DATA")))
        {
            TryMigrateLegacySettings(settingsPath);
        }

        try
        {
            if (!File.Exists(settingsPath))
            {
                var defaults = new CompanionSettings();
                var directory = Path.GetDirectoryName(settingsPath);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(settingsPath, JsonSerializer.Serialize(defaults, JsonOptions));
                return defaults;
            }

            var settings = JsonSerializer.Deserialize<CompanionSettings>(File.ReadAllText(settingsPath), JsonOptions);
            return Normalize(settings ?? new CompanionSettings());
        }
        catch (JsonException)
        {
            return new CompanionSettings();
        }
        catch (IOException)
        {
            return new CompanionSettings();
        }
        catch (UnauthorizedAccessException)
        {
            return new CompanionSettings();
        }
    }

    public static CompanionSettings Save(CompanionSettings settings, string? path = null)
    {
        var normalized = Normalize(settings);
        var settingsPath = path ?? GetDefaultPath();
        var directory = Path.GetDirectoryName(settingsPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(settingsPath, JsonSerializer.Serialize(normalized, JsonOptions));
        return normalized;
    }

    public static bool TryMigrateLegacySettings(
        string settingsPath,
        IReadOnlyList<string>? legacySettingsPaths = null,
        IReadOnlyList<string>? legacyAutostartPaths = null)
    {
        legacySettingsPaths ??= LinuxPaths.LegacyConfigDirectories
            .Select(directory => Path.Combine(directory, "settings.json"))
            .ToArray();
        legacyAutostartPaths ??= LegacyAutostartDesktopFileNames
            .Select(name => Path.Combine(LinuxPaths.AutostartDirectory, name))
            .ToArray();
        try
        {
            if (File.Exists(settingsPath))
            {
                return false;
            }

            var legacySettingsPath = legacySettingsPaths.FirstOrDefault(File.Exists);
            if (legacySettingsPath is null)
            {
                return false;
            }

            var directory = Path.GetDirectoryName(settingsPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.Copy(legacySettingsPath, settingsPath);
            foreach (var legacyAutostartPath in legacyAutostartPaths)
            {
                TryRemoveLegacyAutostartEntry(legacyAutostartPath);
            }

            return true;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    private static void TryRemoveLegacyAutostartEntry(string legacyAutostartPath)
    {
        try
        {
            if (File.Exists(legacyAutostartPath))
            {
                File.Delete(legacyAutostartPath);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    public static string GetDefaultPath()
    {
        var pluginData = Environment.GetEnvironmentVariable("PLUGIN_DATA");
        var directory = string.IsNullOrWhiteSpace(pluginData)
            ? LinuxPaths.ConfigDirectory
            : pluginData;
        return Path.Combine(directory, "settings.json");
    }

    internal static CompanionSettings Normalize(CompanionSettings settings)
    {
        // Migrate the previous untouched defaults to the compact bar layout.
        // Individually resized layouts remain unchanged.
        if (settings.ClaudeSessionCardHeight == 78 && settings.ClaudeWeeklyCardHeight == 96 &&
            settings.CodexFiveHourCardHeight == 78 && settings.CodexWeeklyCardHeight == 96)
        {
            settings = settings with
            {
                ClaudeSessionCardHeight = OverlayLayoutOptions.FiveHourCardHeight,
                ClaudeWeeklyCardHeight = OverlayLayoutOptions.WeeklyCardHeight,
                CodexFiveHourCardHeight = OverlayLayoutOptions.FiveHourCardHeight,
                CodexWeeklyCardHeight = OverlayLayoutOptions.WeeklyCardHeight
            };
        }
        var language = settings.Language?.ToLowerInvariant() switch
        {
            "en" or "en-us" => "en-US",
            "zh-tw" => "zh-tw",
            "zh-cn" => "zh-cn",
            "auto" => "auto",
            _ => "en-US"
        };
        var usageLogFormat = UsageLogOptions.NormalizeFormat(settings.UsageLogFormat);
        return settings with
        {
            ShowCodexFiveHour = settings.LegacyShowFiveHourLimit ?? settings.ShowCodexFiveHour,
            LegacyShowFiveHourLimit = null,
            Language = language,
            Theme = UiThemeOptions.Normalize(settings.Theme),
            OverlayDesign = OverlayDesignOptions.Normalize(settings.OverlayDesign),
            HeaderBadge = HeaderBadgeOptions.Normalize(settings.HeaderBadge),
            SettingsPage = SettingsPageOptions.Normalize(settings.SettingsPage),
            FontScale = OverlayLayoutOptions.NormalizeFontScale(settings.FontScale),
            ClaudeSessionCardHeight = OverlayLayoutOptions.NormalizeCardHeight(settings.ClaudeSessionCardHeight, weekly: false),
            ClaudeWeeklyCardHeight = OverlayLayoutOptions.NormalizeCardHeight(settings.ClaudeWeeklyCardHeight, weekly: true),
            CodexFiveHourCardHeight = OverlayLayoutOptions.NormalizeCardHeight(settings.CodexFiveHourCardHeight, weekly: false),
            CodexWeeklyCardHeight = OverlayLayoutOptions.NormalizeCardHeight(settings.CodexWeeklyCardHeight, weekly: true),
            BackgroundColor = OverlayAppearanceOptions.NormalizeBackgroundColor(settings.BackgroundColor),
            PercentageTimeSpacing = OverlayAppearanceOptions.NormalizeSpacing(settings.PercentageTimeSpacing),
            LabelPercentageSpacing = OverlayAppearanceOptions.NormalizeLabelPercentageSpacing(settings.LabelPercentageSpacing),
            TrayIconStyle = TrayIconStyleOptions.Normalize(settings.TrayIconStyle),
            LowUsageAlertThresholdPercent = UsageAlertOptions.NormalizeThreshold(
                settings.LowUsageAlertThresholdPercent),
            UsageLogFilePath = UsageLogOptions.NormalizeFilePath(
                settings.UsageLogFilePath,
                usageLogFormat),
            UsageLogFormat = usageLogFormat,
            Position = WindowPosition.Normalize(settings.Position),
            Opacity = Math.Clamp(settings.Opacity, 0.5d, 1d),
            Margin = Math.Clamp(settings.Margin, 0, 64),
            RefreshIntervalSeconds = UpdateIntervalOptions.Normalize(
                settings.RefreshIntervalSeconds),
            ResetDateTimeFormat = DateTimeFormatOptions.NormalizeReset(
                settings.ResetDateTimeFormat),
            LastUpdatedDateTimeFormat = DateTimeFormatOptions.NormalizeLastUpdated(
                settings.LastUpdatedDateTimeFormat)
        };
    }
}
