using System.Globalization;
using CodexUsageCompanion.Localization;
using CodexUsageCompanion.RateLimits;
using Xunit;

namespace CodexUsageCompanion.Tests;

public sealed class UiTextTests
{
    [Theory]
    [InlineData("auto", "en-US", UiLanguage.English)]
    [InlineData("auto", "zh-TW", UiLanguage.TraditionalChinese)]
    [InlineData("auto", "zh-CN", UiLanguage.SimplifiedChinese)]
    [InlineData("auto", "zh-HK", UiLanguage.TraditionalChinese)]
    [InlineData("zh-tw", "en-US", UiLanguage.TraditionalChinese)]
    [InlineData("zh-cn", "en-US", UiLanguage.SimplifiedChinese)]
    [InlineData("en-US", "zh-TW", UiLanguage.English)]
    [InlineData("zh-TW", "en-US", UiLanguage.TraditionalChinese)]
    [InlineData("zh-CN", "en-US", UiLanguage.SimplifiedChinese)]
    public void ResolveUsesExplicitSettingOrSystemCulture(string setting, string culture, UiLanguage expected)
    {
        Assert.Equal(expected, UiLanguageResolver.Resolve(setting, CultureInfo.GetCultureInfo(culture)));
    }

    [Theory]
    [InlineData(UiLanguage.English, "58% remaining", "Reset time unavailable", "Last updated at 00:51")]
    [InlineData(UiLanguage.TraditionalChinese, "剩餘 58%", "重置時間未提供", "最後更新於 00:51")]
    [InlineData(UiLanguage.SimplifiedChinese, "剩余 58%", "未提供重置时间", "最后更新于 00:51")]
    public void TrayTooltipContainsUsageResetAndUpdatedTime(
        UiLanguage language,
        string remaining,
        string reset,
        string updated)
    {
        var text = UiText.For(language);
        var claudeState = new RateLimitState(
            null,
            new RateLimitWindowState(58, 10080, null),
            null);
        var updatedAt = new DateTimeOffset(
            2026,
            7,
            31,
            0,
            51,
            0,
            TimeSpan.FromHours(8));

        var tooltip = text.FormatTrayTooltip(null, null, claudeState, updatedAt);

        Assert.Contains("Claude Codex Usage Companion", tooltip);
        Assert.Contains(remaining, tooltip);
        Assert.Contains(reset, tooltip);
        Assert.Contains(updated, tooltip);
    }

    [Fact]
    public void TrayTooltipUsesWeeklyLimitWithoutFallingBackToFiveHour()
    {
        var text = UiText.For(UiLanguage.English);
        var codexState = new RateLimitState(
            new RateLimitWindowState(91, 300, 1785435000),
            null,
            null);

        var tooltip = text.FormatTrayTooltip(codexState, DateTimeOffset.Now, null, null);

        Assert.Contains("[Codex] Weekly: -- remaining", tooltip);
        Assert.DoesNotContain("91% remaining", tooltip);
    }

    [Fact]
    public void SimplifiedChineseContainsExpectedVisibleText()
    {
        var text = UiText.For(UiLanguage.SimplifiedChinese);

        Assert.Equal("5 小时使用量限制", text.FiveHourTitle);
        Assert.Equal("每周使用上限", text.WeeklyTitle);
        Assert.Equal("剩余 58%", text.FormatRemaining(58));
        Assert.Equal("当前方案未提供此额度", text.LimitUnavailable);
    }

    [Fact]
    public void EnglishContainsExpectedVisibleText()
    {
        var text = UiText.For(UiLanguage.English);

        Assert.Equal("5-hour usage limit", text.FiveHourTitle);
        Assert.Equal("Weekly usage limit", text.WeeklyTitle);
        Assert.Equal("58% remaining", text.FormatRemaining(58));
        Assert.Equal("Not available on this plan", text.LimitUnavailable);
        Assert.Equal("Hide to system tray", text.HideToTrayAction);
        Assert.Equal("Show window", text.TrayShowAction);
        Assert.Equal("Quit", text.TrayQuitAction);
        Assert.Equal("Settings - Claude Codex Usage Companion", text.SettingsTitle);
        Assert.Equal("Theme", text.ThemeOption);
        Assert.Equal("Dark", text.DarkTheme);
        Assert.Equal("Light", text.LightTheme);
        Assert.Equal("Follow system settings", text.SystemTheme);
        Assert.Equal(
            "Alert when remaining usage is below the threshold",
            text.LowUsageAlertOption);
        Assert.Equal("Low-usage alert threshold (%)", text.LowUsageAlertThresholdOption);
        Assert.Equal("Notify when usage resets", text.NotifyOnResetOption);
        Assert.Equal(
            "Credits: 715.00, Automatic reload: Disabled",
            text.FormatCreditDetails("715", automaticReloadEnabled: false));
        Assert.Equal(
            "Credits: 189.17, Automatic reload: Enabled",
            text.FormatCreditDetails(
                "189.1717000000",
                automaticReloadEnabled: true));
        Assert.Equal("Update interval", text.UpdateIntervalOption);
        Assert.Equal("Reset date/time format", text.ResetDateTimeFormatOption);
        Assert.Equal(
            "Last updated date/time format",
            text.LastUpdatedDateTimeFormatOption);
        Assert.Equal("Author: ychsieh95 • Original project: gkfriend/codex-usage-companion", text.AuthorInfo);
        Assert.Equal("Enable Codex usage", text.EnableCodexUsageOption);
        Assert.Equal("Enable Claude usage", text.EnableClaudeUsageOption);
        Assert.Equal("Show icon in the taskbar", text.ShowTaskbarIconOption);
        Assert.Equal("System tray icon style", text.TrayIconStyleOption);
        Assert.Equal("Original icon", text.OriginalTrayIconStyle);
        Assert.Equal("Claude current session remaining", text.ClaudeCurrentTrayIconStyle);
        Assert.Equal("Claude weekly session remaining", text.ClaudeWeeklyTrayIconStyle);
        Assert.Equal("Codex session remaining", text.CodexSessionTrayIconStyle);
        Assert.Equal("OK", text.OkAction);
        Assert.Equal("Apply", text.ApplyAction);
        Assert.Equal("Current session", text.ClaudeFiveHourTitle);
        Assert.Equal("Current week (All)", text.ClaudeWeeklyTitle);
        Assert.Equal("Pin window on top", text.PinOnTopAction);
        Assert.Equal("Unpin window from top", text.UnpinFromTopAction);
        Assert.Equal("Claude usage is running low", text.ClaudeLowUsageAlertTitle);
        Assert.Equal("Claude usage has reset", text.ClaudeUsageResetTitle);
    }

    [Theory]
    [InlineData(
        UiLanguage.English,
        "Enable Claude usage",
        "Current session",
        "Current week (All)")]
    [InlineData(
        UiLanguage.TraditionalChinese,
        "啟用 Claude 用量顯示",
        "目前工作階段",
        "本週用量（全部）")]
    [InlineData(
        UiLanguage.SimplifiedChinese,
        "启用 Claude 用量显示",
        "当前会话",
        "本周用量（全部）")]
    public void ClaudeOptionsAreLocalized(
        UiLanguage language,
        string enableOption,
        string fiveHourTitle,
        string weeklyTitle)
    {
        var text = UiText.For(language);

        Assert.Equal(enableOption, text.EnableClaudeUsageOption);
        Assert.Equal(fiveHourTitle, text.ClaudeFiveHourTitle);
        Assert.Equal(weeklyTitle, text.ClaudeWeeklyTitle);
    }

    [Theory]
    [InlineData(UiLanguage.English, "Enable Codex usage")]
    [InlineData(UiLanguage.TraditionalChinese, "啟用 Codex 用量顯示")]
    [InlineData(UiLanguage.SimplifiedChinese, "启用 Codex 用量显示")]
    public void EnableCodexUsageOptionIsLocalized(UiLanguage language, string expected)
    {
        var text = UiText.For(language);

        Assert.Equal(expected, text.EnableCodexUsageOption);
    }

    [Theory]
    [InlineData(UiLanguage.English, "Minimize the window")]
    [InlineData(UiLanguage.TraditionalChinese, "最小化視窗")]
    [InlineData(UiLanguage.SimplifiedChinese, "最小化窗口")]
    public void MinimizeOnStartOptionIsLocalized(UiLanguage language, string expected)
    {
        var text = UiText.For(language);

        Assert.Equal(expected, text.MinimizeOnStartOption);
    }

    [Theory]
    [InlineData(UiLanguage.English, "Show icon in the taskbar")]
    [InlineData(UiLanguage.TraditionalChinese, "在工作列顯示圖示")]
    [InlineData(UiLanguage.SimplifiedChinese, "在任务栏显示图标")]
    public void ShowTaskbarIconOptionIsLocalized(UiLanguage language, string expected)
    {
        var text = UiText.For(language);

        Assert.Equal(expected, text.ShowTaskbarIconOption);
    }

    [Theory]
    [InlineData(UiLanguage.English, "OK", "Cancel", "Apply")]
    [InlineData(UiLanguage.TraditionalChinese, "確定", "取消", "應用")]
    [InlineData(UiLanguage.SimplifiedChinese, "确定", "取消", "应用")]
    public void SettingsActionsAreLocalized(
        UiLanguage language,
        string ok,
        string cancel,
        string apply)
    {
        var text = UiText.For(language);

        Assert.Equal(ok, text.OkAction);
        Assert.Equal(cancel, text.CancelAction);
        Assert.Equal(apply, text.ApplyAction);
    }

    [Theory]
    [InlineData(UiLanguage.English, "Pin window on top", "Unpin window from top")]
    [InlineData(UiLanguage.TraditionalChinese, "將視窗釘選在最上層", "取消視窗置頂")]
    [InlineData(UiLanguage.SimplifiedChinese, "将窗口固定在最上层", "取消窗口置顶")]
    public void PinOnTopActionsAreLocalized(
        UiLanguage language,
        string pin,
        string unpin)
    {
        var text = UiText.For(language);

        Assert.Equal(pin, text.PinOnTopAction);
        Assert.Equal(unpin, text.UnpinFromTopAction);
    }

    [Theory]
    [InlineData(
        UiLanguage.English,
        "Unsaved changes",
        "Discard changes",
        "Keep editing")]
    [InlineData(
        UiLanguage.TraditionalChinese,
        "尚未儲存變更",
        "捨棄變更",
        "繼續編輯")]
    [InlineData(
        UiLanguage.SimplifiedChinese,
        "尚未保存更改",
        "放弃更改",
        "继续编辑")]
    public void UnsavedChangesWarningIsLocalized(
        UiLanguage language,
        string title,
        string discard,
        string keepEditing)
    {
        var text = UiText.For(language);

        Assert.Equal(title, text.UnsavedChangesTitle);
        Assert.Equal(discard, text.DiscardChangesAction);
        Assert.Equal(keepEditing, text.KeepEditingAction);
    }

    [Theory]
    [InlineData(UiLanguage.English, "Usage credits: $6.32 / $100.00, Auto-reload: Enabled")]
    [InlineData(UiLanguage.TraditionalChinese, "使用點數：$6.32 / $100.00, 自動加購：已啟用")]
    [InlineData(UiLanguage.SimplifiedChinese, "使用点数：$6.32 / $100.00, 自动加购：已启用")]
    public void FormatClaudeCreditsDetailsShowsUsedAndLimitWhenEnabled(
        UiLanguage language,
        string expected)
    {
        var text = UiText.For(language);
        var extraUsage = new RateLimitExtraUsageState(true, 6.32m, 100m, "USD");

        Assert.Equal(expected, text.FormatClaudeCreditsDetails(extraUsage));
    }

    [Fact]
    public void FormatClaudeCreditsDetailsHandlesDisabledAndUnavailableStates()
    {
        var text = UiText.For(UiLanguage.English);

        Assert.Equal(
            "Usage credits: --, Auto-reload: Disabled",
            text.FormatClaudeCreditsDetails(new RateLimitExtraUsageState(false, null, null, null)));
        Assert.Equal(
            "Usage credits: --, Auto-reload: --",
            text.FormatClaudeCreditsDetails(null));
    }

    [Fact]
    public void FormatTrayTooltipIncludesBothProvidersWhenClaudeEnabled()
    {
        var text = UiText.For(UiLanguage.English);
        var codexState = new RateLimitState(null, new RateLimitWindowState(40, 10080, null), null);
        var claudeState = new RateLimitState(
            new RateLimitWindowState(75, 300, null),
            new RateLimitWindowState(91, 10080, null),
            null);

        var tooltip = text.FormatTrayTooltip(codexState, DateTimeOffset.Now, claudeState, null);

        Assert.Contains("[Claude] Current: 75% remaining", tooltip);
        Assert.Contains("[Claude] Weekly: 91% remaining", tooltip);
        Assert.Contains("[Codex] Weekly: 40% remaining", tooltip);
    }

    [Fact]
    public void FormatTrayTooltipMatchesRequestedLineFormat()
    {
        var text = UiText.For(UiLanguage.English);
        var codexState = new RateLimitState(null, new RateLimitWindowState(40, 10080, null), null);
        var claudeState = new RateLimitState(
            new RateLimitWindowState(75, 300, null),
            new RateLimitWindowState(91, 10080, null),
            null);
        var updatedAt = new DateTimeOffset(2026, 8, 1, 9, 30, 0, TimeSpan.Zero);

        var tooltip = text.FormatTrayTooltip(codexState, updatedAt, claudeState, updatedAt);

        Assert.Equal(
            string.Join(
                Environment.NewLine,
                "Claude Codex Usage Companion",
                "[Claude] Current: 75% remaining (Reset time unavailable)",
                "[Claude] Weekly: 91% remaining (Reset time unavailable)",
                "[Codex] Weekly: 40% remaining (Reset time unavailable)",
                "Last updated at 09:30"),
            tooltip);
    }

    [Theory]
    [InlineData(
        UiLanguage.TraditionalChinese,
        "點數：715.00, 自動儲值：已啟用",
        "每週使用上限剩餘 12%。")]
    [InlineData(
        UiLanguage.SimplifiedChinese,
        "点数：715.00, 自动充值：已启用",
        "每周使用上限剩余 12%。")]
    [InlineData(
        UiLanguage.English,
        "Credits: 715.00, Automatic reload: Enabled",
        "Weekly usage limit has 12% remaining.")]
    public void CreditAndAlertTextIsLocalized(
        UiLanguage language,
        string creditDetails,
        string lowUsage)
    {
        var text = UiText.For(language);

        Assert.Equal(
            creditDetails,
            text.FormatCreditDetails("715", automaticReloadEnabled: true));
        Assert.Equal(lowUsage, text.FormatLowUsageAlert(weekly: true, 12));
    }

    [Theory]
    [InlineData(UiLanguage.TraditionalChinese, "設定 - Claude Codex Usage Companion")]
    [InlineData(UiLanguage.SimplifiedChinese, "设置 - Claude Codex Usage Companion")]
    public void SettingsTitleIsLocalized(UiLanguage language, string expected)
    {
        var text = UiText.For(language);

        Assert.Equal(expected, text.SettingsTitle);
    }

    [Theory]
    [InlineData(
        UiLanguage.TraditionalChinese,
        "主題",
        "深色",
        "淺色",
        "跟隨系統設定")]
    [InlineData(
        UiLanguage.SimplifiedChinese,
        "主题",
        "深色",
        "浅色",
        "跟随系统设置")]
    public void ThemeOptionsAreLocalized(
        UiLanguage language,
        string option,
        string dark,
        string light,
        string system)
    {
        var text = UiText.For(language);

        Assert.Equal(option, text.ThemeOption);
        Assert.Equal(dark, text.DarkTheme);
        Assert.Equal(light, text.LightTheme);
        Assert.Equal(system, text.SystemTheme);
    }

    [Theory]
    [InlineData(UiLanguage.TraditionalChinese, "作者：ychsieh95 • 原始專案：gkfriend/codex-usage-companion")]
    [InlineData(UiLanguage.SimplifiedChinese, "作者：ychsieh95 • 原始项目：gkfriend/codex-usage-companion")]
    public void AuthorInfoIsLocalized(UiLanguage language, string expected)
    {
        var text = UiText.For(language);

        Assert.Equal(expected, text.AuthorInfo);
    }

    [Theory]
    [InlineData(
        UiLanguage.TraditionalChinese,
        "隱藏至系統匣",
        "顯示視窗",
        "結束")]
    [InlineData(
        UiLanguage.SimplifiedChinese,
        "隐藏到系统托盘",
        "显示窗口",
        "退出")]
    public void TrayActionsAreLocalized(
        UiLanguage language,
        string hide,
        string show,
        string quit)
    {
        var text = UiText.For(language);

        Assert.Equal(hide, text.HideToTrayAction);
        Assert.Equal(show, text.TrayShowAction);
        Assert.Equal(quit, text.TrayQuitAction);
    }

    [Theory]
    [InlineData(UiLanguage.TraditionalChinese, "於 7月10日 重置", "於 7月17日 重置")]
    [InlineData(UiLanguage.SimplifiedChinese, "于 7月10日 重置", "于 7月17日 重置")]
    [InlineData(UiLanguage.English, "Resets at Jul 10", "Resets at Jul 17")]
    public void ResetFormattingIsLocalized(UiLanguage language, string fiveHour, string weekly)
    {
        var text = UiText.For(language);
        var time = new DateTimeOffset(2026, 7, 10, 23, 33, 0, TimeSpan.FromHours(8));
        var date = new DateTimeOffset(2026, 7, 17, 9, 0, 0, TimeSpan.FromHours(8));

        Assert.Equal(fiveHour, text.FormatFiveHourReset(time));
        Assert.Equal(weekly, text.FormatWeeklyReset(date));
    }

    [Theory]
    [InlineData(UiLanguage.TraditionalChinese, "最後更新於 23:07")]
    [InlineData(UiLanguage.SimplifiedChinese, "最后更新于 23:07")]
    [InlineData(UiLanguage.English, "Last updated at 23:07")]
    public void UpdatedTimeUsesTwentyFourHourClock(UiLanguage language, string expected)
    {
        var text = UiText.For(language);
        var updatedAt = new DateTimeOffset(2026, 7, 31, 23, 7, 0, TimeSpan.FromHours(8));

        Assert.Equal(expected, text.FormatUpdatedTime(updatedAt));
    }

    [Theory]
    [InlineData(
        UiLanguage.TraditionalChinese,
        "MMM D HH:mm",
        "於 7月10日 23:33 重置")]
    [InlineData(
        UiLanguage.SimplifiedChinese,
        "yyyy-MM-dd",
        "于 2026-07-10 重置")]
    [InlineData(
        UiLanguage.English,
        "yyyy-MM-dd HH:mm",
        "Resets at 2026-07-10 23:33")]
    [InlineData(
        UiLanguage.English,
        "dddd, MMM D 'at' HH:mm",
        "Resets at Friday, Jul 10 at 23:33")]
    public void ConfiguredResetDateTimeFormatIsApplied(
        UiLanguage language,
        string format,
        string expected)
    {
        var text = UiText.For(language, resetDateTimeFormat: format);
        var reset = new DateTimeOffset(2026, 7, 10, 23, 33, 0, TimeSpan.FromHours(8));

        Assert.Equal(expected, text.FormatFiveHourReset(reset));
    }

    [Theory]
    [InlineData("HH:mm:ss", "Last updated at 23:07:09")]
    [InlineData("MMM D HH:mm", "Last updated at Jul 31 23:07")]
    [InlineData("yyyy-MM-dd HH:mm", "Last updated at 2026-07-31 23:07")]
    public void ConfiguredLastUpdatedDateTimeFormatIsApplied(
        string format,
        string expected)
    {
        var text = UiText.For(
            UiLanguage.English,
            lastUpdatedDateTimeFormat: format);
        var updatedAt = new DateTimeOffset(
            2026,
            7,
            31,
            23,
            7,
            9,
            TimeSpan.FromHours(8));

        Assert.Equal(expected, text.FormatUpdatedTime(updatedAt));
    }

    [Theory]
    [InlineData("yyyy/MM/dd HH:mm", true, "2026/08/06 13:55")]
    [InlineData("MMM D 'at' h:mm tt", true, "Aug 6 at 1:55 PM")]
    [InlineData("yyyy-MM-dd '", false, "")]
    [InlineData("", false, "")]
    public void DateTimeFormatCheckerReturnsPreviewForValidInput(
        string format,
        bool expectedValid,
        string expectedPreview)
    {
        var text = UiText.For(UiLanguage.English);
        var sample = new DateTimeOffset(2026, 8, 6, 13, 55, 9, TimeSpan.Zero);

        var valid = text.TryFormatDateTime(sample, format, out var preview);

        Assert.Equal(expectedValid, valid);
        Assert.Equal(expectedPreview, preview);
    }

    [Theory]
    [InlineData(UiLanguage.TraditionalChinese, 60, "每 1 分鐘")]
    [InlineData(UiLanguage.SimplifiedChinese, 300, "每 5 分钟")]
    [InlineData(UiLanguage.English, 60, "Every 1 minute")]
    [InlineData(UiLanguage.English, 90, "Every 1.5 minutes")]
    [InlineData(UiLanguage.English, 1800, "Every 30 minutes")]
    public void UpdateIntervalFormattingIsLocalized(
        UiLanguage language,
        int seconds,
        string expected)
    {
        var text = UiText.For(language);

        Assert.Equal(expected, text.FormatUpdateInterval(seconds));
    }
}
