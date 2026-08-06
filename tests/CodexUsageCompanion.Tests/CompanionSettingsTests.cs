using CodexUsageCompanion.Configuration;
using Xunit;

namespace CodexUsageCompanion.Tests;

public sealed class CompanionSettingsTests
{
    [Fact]
    public void LoadCreatesDefaultsWhenSettingsFileIsMissing()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"CodexUsageCompanion.Tests.{Guid.NewGuid():N}");
        var path = Path.Combine(directory, "settings.json");
        try
        {
            var settings = CompanionSettingsStore.Load(path);

            Assert.False(settings.ShowFiveHourLimit);
            Assert.True(settings.EnableCodexUsage);
            Assert.True(settings.EnableClaudeUsage);
            Assert.False(settings.EnableSystemTray);
            Assert.Equal(TrayIconStyleOptions.Original, settings.TrayIconStyle);
            Assert.True(settings.ShowTaskbarIcon);
            Assert.False(settings.StartOnBoot);
            Assert.False(settings.MinimizeOnStart);
            Assert.Equal(
                SystemLanguageOptions.ResolveDefault(
                    System.Globalization.CultureInfo.CurrentUICulture),
                settings.Language);
            Assert.Equal(UiThemeOptions.System, settings.Theme);
            Assert.False(settings.EnableLowUsageAlert);
            Assert.Equal(
                UsageAlertOptions.DefaultThresholdPercent,
                settings.LowUsageAlertThresholdPercent);
            Assert.False(settings.NotifyOnReset);
            Assert.False(settings.EnableUsageLogging);
            Assert.Equal(
                UsageLogOptions.DefaultFilePath(UsageLogOptions.Csv),
                settings.UsageLogFilePath);
            Assert.Equal(UsageLogOptions.Csv, settings.UsageLogFormat);
            Assert.Equal(WindowPosition.RightBottom, settings.Position);
            Assert.Equal(1d, settings.Opacity);
            Assert.Equal(16, settings.Margin);
            Assert.True(settings.AlwaysOnTop);
            Assert.Equal(60, settings.RefreshIntervalSeconds);
            Assert.Equal(
                DateTimeFormatOptions.MonthDay,
                settings.ResetDateTimeFormat);
            Assert.Equal(
                DateTimeFormatOptions.HourMinute,
                settings.LastUpdatedDateTimeFormat);
            Assert.True(File.Exists(path));
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }
    }

    [Fact]
    public void LoadReadsAndNormalizesUserSettings()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"CodexUsageCompanion.Tests.{Guid.NewGuid():N}");
        var path = Path.Combine(directory, "settings.json");
        Directory.CreateDirectory(directory);
        File.WriteAllText(path, """
        {
          "showFiveHourLimit": true,
          "language": "zh-cn",
          "theme": "LIGHT",
          "position": "top-left",
          "opacity": 1.5,
          "margin": 200,
          "refreshIntervalSeconds": 2,
          "lowUsageAlertThresholdPercent": 500,
          "resetDateTimeFormat": "yyyy-MM-dd '",
          "lastUpdatedDateTimeFormat": "HH:mm '"
        }
        """);
        try
        {
            var settings = CompanionSettingsStore.Load(path);

            Assert.True(settings.ShowFiveHourLimit);
            Assert.False(settings.EnableSystemTray);
            Assert.Equal(TrayIconStyleOptions.Original, settings.TrayIconStyle);
            Assert.True(settings.ShowTaskbarIcon);
            Assert.False(settings.StartOnBoot);
            Assert.Equal("zh-cn", settings.Language);
            Assert.Equal(UiThemeOptions.Light, settings.Theme);
            Assert.Equal(WindowPosition.LeftTop, settings.Position);
            Assert.Equal(1d, settings.Opacity);
            Assert.Equal(64, settings.Margin);
            Assert.Equal(UpdateIntervalOptions.MinimumSeconds, settings.RefreshIntervalSeconds);
            Assert.Equal(
                UsageAlertOptions.MaximumThresholdPercent,
                settings.LowUsageAlertThresholdPercent);
            Assert.Equal(
                DateTimeFormatOptions.MonthDay,
                settings.ResetDateTimeFormat);
            Assert.Equal(
                DateTimeFormatOptions.HourMinute,
                settings.LastUpdatedDateTimeFormat);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void SavePersistsExplicitlyDisabledProviders()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"CodexUsageCompanion.Tests.{Guid.NewGuid():N}");
        var path = Path.Combine(directory, "settings.json");
        try
        {
            var saved = CompanionSettingsStore.Save(
                new CompanionSettings
                {
                    EnableCodexUsage = false,
                    EnableClaudeUsage = false
                },
                path);
            var loaded = CompanionSettingsStore.Load(path);

            Assert.False(saved.EnableCodexUsage);
            Assert.False(saved.EnableClaudeUsage);
            Assert.False(loaded.EnableCodexUsage);
            Assert.False(loaded.EnableClaudeUsage);
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }
    }

    [Fact]
    public void SavePersistsNormalizedInteractiveSettings()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"CodexUsageCompanion.Tests.{Guid.NewGuid():N}");
        var path = Path.Combine(directory, "settings.json");
        try
        {
            var saved = CompanionSettingsStore.Save(
                new CompanionSettings
                {
                    EnableSystemTray = true,
                    TrayIconStyle = TrayIconStyleOptions.CodexSession,
                    ShowTaskbarIcon = false,
                    StartOnBoot = true,
                    MinimizeOnStart = true,
                    EnableClaudeUsage = true,
                    Language = "zh-tw",
                    Theme = UiThemeOptions.System,
                    AlwaysOnTop = false,
                    EnableLowUsageAlert = true,
                    LowUsageAlertThresholdPercent = 15,
                    NotifyOnReset = true,
                    Position = WindowPosition.MiddleCenter,
                    RefreshIntervalSeconds = 900,
                    ResetDateTimeFormat = DateTimeFormatOptions.YearMonthDayTime,
                    LastUpdatedDateTimeFormat = DateTimeFormatOptions.HourMinuteSecond,
                    EnableUsageLogging = true,
                    UsageLogFilePath = Path.Combine(directory, "usage.csv"),
                    UsageLogFormat = "CSV"
                },
                path);
            var loaded = CompanionSettingsStore.Load(path);

            Assert.True(saved.EnableSystemTray);
            Assert.Equal(TrayIconStyleOptions.CodexSession, saved.TrayIconStyle);
            Assert.False(saved.ShowTaskbarIcon);
            Assert.True(saved.StartOnBoot);
            Assert.True(saved.MinimizeOnStart);
            Assert.True(saved.EnableClaudeUsage);
            Assert.Equal("zh-tw", saved.Language);
            Assert.Equal(UiThemeOptions.System, saved.Theme);
            Assert.False(saved.AlwaysOnTop);
            Assert.True(saved.EnableLowUsageAlert);
            Assert.Equal(15, saved.LowUsageAlertThresholdPercent);
            Assert.True(saved.NotifyOnReset);
            Assert.Equal(WindowPosition.MiddleCenter, saved.Position);
            Assert.Equal(900, saved.RefreshIntervalSeconds);
            Assert.Equal(
                DateTimeFormatOptions.YearMonthDayTime,
                saved.ResetDateTimeFormat);
            Assert.Equal(
                DateTimeFormatOptions.HourMinuteSecond,
                saved.LastUpdatedDateTimeFormat);
            Assert.True(saved.EnableUsageLogging);
            Assert.Equal(Path.Combine(directory, "usage.csv"), saved.UsageLogFilePath);
            Assert.Equal(UsageLogOptions.Csv, saved.UsageLogFormat);
            Assert.Equal(saved, loaded);
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }
    }

    [Fact]
    public void TryMigrateLegacySettingsCopiesOnceFromOldPath()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"CodexUsageCompanion.Tests.{Guid.NewGuid():N}");
        var legacySettingsPath = Path.Combine(directory, "legacy", "settings.json");
        var legacyAutostartPath = Path.Combine(directory, "legacy-autostart.desktop");
        var settingsPath = Path.Combine(directory, "new", "settings.json");
        Directory.CreateDirectory(Path.GetDirectoryName(legacySettingsPath)!);
        File.WriteAllText(legacySettingsPath, """{"language":"zh-tw","margin":32}""");
        File.WriteAllText(legacyAutostartPath, "[Desktop Entry]");
        try
        {
            var migrated = CompanionSettingsStore.TryMigrateLegacySettings(
                settingsPath,
                [legacySettingsPath],
                [legacyAutostartPath]);

            Assert.True(migrated);
            Assert.True(File.Exists(settingsPath));
            Assert.Contains("zh-tw", File.ReadAllText(settingsPath));
            Assert.False(File.Exists(legacyAutostartPath));

            File.WriteAllText(legacySettingsPath, """{"language":"en-US"}""");
            var migratedAgain = CompanionSettingsStore.TryMigrateLegacySettings(
                settingsPath,
                [legacySettingsPath],
                [legacyAutostartPath]);

            Assert.False(migratedAgain);
            Assert.Contains("zh-tw", File.ReadAllText(settingsPath));
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }
    }

    [Fact]
    public void TryMigrateLegacySettingsFallsBackToOlderLegacyPath()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"CodexUsageCompanion.Tests.{Guid.NewGuid():N}");
        var mostRecentLegacyPath = Path.Combine(directory, "codex-claude-usage-companion", "settings.json");
        var oldestLegacyPath = Path.Combine(directory, "codex-usage-companion", "settings.json");
        var settingsPath = Path.Combine(directory, "new", "settings.json");
        Directory.CreateDirectory(Path.GetDirectoryName(oldestLegacyPath)!);
        File.WriteAllText(oldestLegacyPath, """{"language":"zh-cn"}""");
        try
        {
            var migrated = CompanionSettingsStore.TryMigrateLegacySettings(
                settingsPath,
                [mostRecentLegacyPath, oldestLegacyPath],
                []);

            Assert.True(migrated);
            Assert.Contains("zh-cn", File.ReadAllText(settingsPath));
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }
    }

    [Fact]
    public void TryMigrateLegacySettingsDoesNothingWithoutALegacyFile()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"CodexUsageCompanion.Tests.{Guid.NewGuid():N}");
        var settingsPath = Path.Combine(directory, "settings.json");
        var legacySettingsPath = Path.Combine(directory, "legacy", "settings.json");
        try
        {
            var migrated = CompanionSettingsStore.TryMigrateLegacySettings(
                settingsPath,
                [legacySettingsPath],
                [Path.Combine(directory, "legacy-autostart.desktop")]);

            Assert.False(migrated);
            Assert.False(File.Exists(settingsPath));
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }
    }

    [Fact]
    public void LoadFallsBackSafelyForInvalidJson()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"CodexUsageCompanion.Tests.{Guid.NewGuid():N}");
        var path = Path.Combine(directory, "settings.json");
        Directory.CreateDirectory(directory);
        File.WriteAllText(path, "{");
        try
        {
            var settings = CompanionSettingsStore.Load(path);

            Assert.Equal(new CompanionSettings(), settings);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Theory]
    [InlineData("left-top", WindowPosition.LeftTop)]
    [InlineData("middle-top", WindowPosition.MiddleTop)]
    [InlineData("right-top", WindowPosition.RightTop)]
    [InlineData("left-center", WindowPosition.LeftCenter)]
    [InlineData("middle-center", WindowPosition.MiddleCenter)]
    [InlineData("right-center", WindowPosition.RightCenter)]
    [InlineData("left-bottom", WindowPosition.LeftBottom)]
    [InlineData("middle-bottom", WindowPosition.MiddleBottom)]
    [InlineData("right-bottom", WindowPosition.RightBottom)]
    [InlineData("top-left", WindowPosition.LeftTop)]
    [InlineData("bottom-right", WindowPosition.RightBottom)]
    [InlineData("invalid", WindowPosition.RightBottom)]
    public void WindowPositionNormalizesCurrentAndLegacyValues(string value, string expected)
    {
        Assert.Equal(expected, WindowPosition.Normalize(value));
    }

    [Theory]
    [InlineData("txt", UsageLogOptions.Text)]
    [InlineData("CSV", UsageLogOptions.Csv)]
    [InlineData("jsonl", UsageLogOptions.JsonLines)]
    [InlineData("invalid", UsageLogOptions.Csv)]
    public void UsageLogFormatNormalizesSupportedValues(string value, string expected)
    {
        Assert.Equal(expected, UsageLogOptions.NormalizeFormat(value));
    }

    [Theory]
    [InlineData("dark", UiThemeOptions.Dark)]
    [InlineData("LIGHT", UiThemeOptions.Light)]
    [InlineData("system", UiThemeOptions.System)]
    [InlineData("invalid", UiThemeOptions.System)]
    public void ThemeNormalizesSupportedValues(string value, string expected)
    {
        Assert.Equal(expected, UiThemeOptions.Normalize(value));
    }

    [Theory]
    [InlineData(-1, 1)]
    [InlineData(1, 1)]
    [InlineData(20, 20)]
    [InlineData(100, 100)]
    [InlineData(101, 100)]
    public void LowUsageAlertThresholdIsClamped(int value, int expected)
    {
        Assert.Equal(expected, UsageAlertOptions.NormalizeThreshold(value));
    }

    [Theory]
    [InlineData("en-US", "en-US")]
    [InlineData("fr-FR", "en-US")]
    [InlineData("zh-TW", "zh-tw")]
    [InlineData("zh-Hant", "zh-tw")]
    [InlineData("zh-HK", "zh-tw")]
    [InlineData("zh-CN", "zh-cn")]
    [InlineData("zh-Hans", "zh-cn")]
    [InlineData("zh-SG", "zh-cn")]
    public void SystemLanguageDefaultUsesSupportedIdentifiers(
        string culture,
        string expected)
    {
        Assert.Equal(
            expected,
            SystemLanguageOptions.ResolveDefault(
                System.Globalization.CultureInfo.GetCultureInfo(culture)));
    }

    [Theory]
    [InlineData(10, 60)]
    [InlineData(60, 60)]
    [InlineData(900, 900)]
    [InlineData(3600, 3600)]
    [InlineData(7200, 3600)]
    public void UpdateIntervalUsesCurrentDefaultAsMinimum(int value, int expected)
    {
        Assert.Equal(expected, UpdateIntervalOptions.Normalize(value));
    }

    [Theory]
    [InlineData("MMM D", "MMM D")]
    [InlineData("MMM d", "MMM D")]
    [InlineData("MMM D HH:mm", "MMM D HH:mm")]
    [InlineData("yyyy-MM-dd", "yyyy-MM-dd")]
    [InlineData("yyyy-MM-dd HH:mm", "yyyy-MM-dd HH:mm")]
    [InlineData("dddd, MMM D 'at' HH:mm", "dddd, MMM D 'at' HH:mm")]
    [InlineData("yyyy-MM-dd '", "MMM D")]
    public void ResetDateTimeFormatNormalizesSupportedValues(
        string value,
        string expected)
    {
        Assert.Equal(expected, DateTimeFormatOptions.NormalizeReset(value));
    }

    [Theory]
    [InlineData("HH:mm", "HH:mm")]
    [InlineData("HH:mm:ss", "HH:mm:ss")]
    [InlineData("MMM D HH:mm", "MMM D HH:mm")]
    [InlineData("yyyy-MM-dd HH:mm", "yyyy-MM-dd HH:mm")]
    [InlineData("HH:mm 'UTC'", "HH:mm 'UTC'")]
    [InlineData("HH:mm '", "HH:mm")]
    public void LastUpdatedDateTimeFormatNormalizesSupportedValues(
        string value,
        string expected)
    {
        Assert.Equal(expected, DateTimeFormatOptions.NormalizeLastUpdated(value));
    }

    [Theory]
    [InlineData("/tmp/usage-history.txt", "csv", "/tmp/usage-history.csv")]
    [InlineData("/tmp/usage-history.csv", "jsonl", "/tmp/usage-history.jsonl")]
    [InlineData("/tmp/usage-history", "txt", "/tmp/usage-history.txt")]
    public void UsageLogFormatChangesFileExtension(
        string path,
        string format,
        string expected)
    {
        Assert.Equal(expected, UsageLogOptions.ChangeFileExtension(path, format));
    }

    [Fact]
    public void SaveSynchronizesUsageLogExtensionWithFormat()
    {
        var directory = Path.Combine(
            Path.GetTempPath(),
            $"CodexUsageCompanion.Tests.{Guid.NewGuid():N}");
        var settingsPath = Path.Combine(directory, "settings.json");
        try
        {
            var saved = CompanionSettingsStore.Save(
                new CompanionSettings
                {
                    UsageLogFilePath = Path.Combine(directory, "custom-name.txt"),
                    UsageLogFormat = UsageLogOptions.Csv
                },
                settingsPath);

            Assert.Equal(
                Path.Combine(directory, "custom-name.csv"),
                saved.UsageLogFilePath);
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }
    }
}
