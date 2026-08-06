using System.Globalization;
using CodexUsageCompanion.Configuration;
using CodexUsageCompanion.RateLimits;

namespace CodexUsageCompanion.Localization;

public sealed record UiText(
    UiLanguage Language,
    string FiveHourTitle,
    string WeeklyTitle,
    string WaitingForData,
    string LimitUnavailable,
    string ResetUnavailable,
    string MinimizeAction,
    string RefreshAction,
    string HideToTrayAction,
    string TrayShowAction,
    string TrayQuitAction,
    string CloseAction,
    string SettingsAction,
    string SettingsTitle,
    string SystemTrayOption,
    string StartOnBootOption,
    string LanguageOption,
    string ThemeOption,
    string DarkTheme,
    string LightTheme,
    string SystemTheme,
    string PositionOption,
    string UsageLoggingOption,
    string UsageLogFilePathOption,
    string UsageLogFormatOption,
    string UpdateIntervalOption,
    string ResetDateTimeFormatOption,
    string LastUpdatedDateTimeFormatOption,
    string FormatPreview,
    string InvalidDateTimeFormat,
    string AlwaysOnTopOption,
    string SaveAction,
    string CancelAction,
    string EnglishLanguage,
    string TraditionalChineseLanguage,
    string SimplifiedChineseLanguage,
    string AuthorInfo)
{
    public string ResetDateTimeFormat { get; init; } = DateTimeFormatOptions.MonthDay;
    public string LastUpdatedDateTimeFormat { get; init; } =
        DateTimeFormatOptions.HourMinute;

    public static UiText For(
        UiLanguage language,
        string? resetDateTimeFormat = null,
        string? lastUpdatedDateTimeFormat = null)
    {
        var text = language switch
        {
            UiLanguage.TraditionalChinese => new UiText(
                language,
                "5 小時使用量限制",
                "每週使用上限",
                "等待使用量資料",
                "目前方案未提供此額度",
                "重置時間未提供",
                "最小化",
                "重新整理使用量",
                "隱藏至系統匣",
                "顯示視窗",
                "結束",
                "關閉",
                "設定",
                "設定 - Claude Codex Usage Companion",
                "啟用系統匣",
                "開機時自動啟動",
                "語言",
                "主題",
                "深色",
                "淺色",
                "跟隨系統設定",
                "視窗位置",
                "啟用用量更新記錄",
                "記錄檔路徑",
                "記錄格式",
                "更新間隔",
                "重置日期時間格式",
                "最後更新日期時間格式",
                "預覽",
                "日期時間格式無效",
                "視窗永遠置頂",
                "儲存",
                "取消",
                "English (en-US)",
                "繁體中文 (zh-tw)",
                "简体中文 (zh-cn)",
                "作者：ychsieh95 • 原始專案：gkfriend/codex-usage-companion"),
            UiLanguage.SimplifiedChinese => new UiText(
                language,
                "5 小时使用量限制",
                "每周使用上限",
                "等待使用量数据",
                "当前方案未提供此额度",
                "未提供重置时间",
                "最小化",
                "刷新使用量",
                "隐藏到系统托盘",
                "显示窗口",
                "退出",
                "关闭",
                "设置",
                "设置 - Claude Codex Usage Companion",
                "启用系统托盘",
                "开机时自动启动",
                "语言",
                "主题",
                "深色",
                "浅色",
                "跟随系统设置",
                "窗口位置",
                "启用用量更新日志",
                "日志文件路径",
                "日志格式",
                "更新间隔",
                "重置日期时间格式",
                "最后更新日期时间格式",
                "预览",
                "日期时间格式无效",
                "窗口始终置顶",
                "保存",
                "取消",
                "English (en-US)",
                "繁體中文 (zh-tw)",
                "简体中文 (zh-cn)",
                "作者：ychsieh95 • 原始项目：gkfriend/codex-usage-companion"),
            _ => new UiText(
                language,
                "5-hour usage limit",
                "Weekly usage limit",
                "Waiting for usage data",
                "Not available on this plan",
                "Reset time unavailable",
                "Minimize",
                "Refresh usage",
                "Hide to system tray",
                "Show window",
                "Quit",
                "Close",
                "Settings",
                "Settings - Claude Codex Usage Companion",
                "Enable system tray",
                "Start automatically when I sign in",
                "Language",
                "Theme",
                "Dark",
                "Light",
                "Follow system settings",
                "Window position",
                "Enable usage update logging",
                "Log file path",
                "Log format",
                "Update interval",
                "Reset date/time format",
                "Last updated date/time format",
                "Preview",
                "Invalid date/time format",
                "Keep window always on top",
                "Save",
                "Cancel",
                "English (en-US)",
                "繁體中文 (zh-tw)",
                "简体中文 (zh-cn)",
                "Author: ychsieh95 • Original project: gkfriend/codex-usage-companion")
        };
        return text with
        {
            ResetDateTimeFormat = DateTimeFormatOptions.NormalizeReset(
                resetDateTimeFormat),
            LastUpdatedDateTimeFormat = DateTimeFormatOptions.NormalizeLastUpdated(
                lastUpdatedDateTimeFormat)
        };
    }

    public string FormatRemaining(int remainingPercent)
    {
        return Language == UiLanguage.English
            ? $"{remainingPercent}% remaining"
            : Language == UiLanguage.SimplifiedChinese
                ? $"剩余 {remainingPercent}%"
                : $"剩餘 {remainingPercent}%";
    }

    public string RemainingUnavailable => Language == UiLanguage.English
        ? "-- remaining"
        : Language == UiLanguage.SimplifiedChinese
            ? "剩余 --"
            : "剩餘 --";

    public string EnableCodexUsageOption => Language switch
    {
        UiLanguage.TraditionalChinese => "啟用 Codex 用量顯示",
        UiLanguage.SimplifiedChinese => "启用 Codex 用量显示",
        _ => "Enable Codex usage"
    };

    public string EnableClaudeUsageOption => Language switch
    {
        UiLanguage.TraditionalChinese => "啟用 Claude 用量顯示",
        UiLanguage.SimplifiedChinese => "启用 Claude 用量显示",
        _ => "Enable Claude usage"
    };

    public string MinimizeOnStartOption => Language switch
    {
        UiLanguage.TraditionalChinese => "最小化視窗",
        UiLanguage.SimplifiedChinese => "最小化窗口",
        _ => "Minimize the window"
    };

    public string ShowTaskbarIconOption => Language switch
    {
        UiLanguage.TraditionalChinese => "在工作列顯示圖示",
        UiLanguage.SimplifiedChinese => "在任务栏显示图标",
        _ => "Show icon in the taskbar"
    };

    public string TrayIconStyleOption => Language switch
    {
        UiLanguage.TraditionalChinese => "系統匣圖示樣式",
        UiLanguage.SimplifiedChinese => "系统托盘图标样式",
        _ => "System tray icon style"
    };

    public string OriginalTrayIconStyle => Language switch
    {
        UiLanguage.TraditionalChinese => "原始圖示",
        UiLanguage.SimplifiedChinese => "原始图标",
        _ => "Original icon"
    };

    public string ClaudeCurrentTrayIconStyle => Language switch
    {
        UiLanguage.TraditionalChinese => "Claude 目前工作階段剩餘用量",
        UiLanguage.SimplifiedChinese => "Claude 当前会话剩余用量",
        _ => "Claude current session remaining"
    };

    public string ClaudeWeeklyTrayIconStyle => Language switch
    {
        UiLanguage.TraditionalChinese => "Claude 每週工作階段剩餘用量",
        UiLanguage.SimplifiedChinese => "Claude 每周会话剩余用量",
        _ => "Claude weekly session remaining"
    };

    public string CodexSessionTrayIconStyle => Language switch
    {
        UiLanguage.TraditionalChinese => "Codex 工作階段剩餘用量",
        UiLanguage.SimplifiedChinese => "Codex 会话剩余用量",
        _ => "Codex session remaining"
    };

    public string OkAction => Language switch
    {
        UiLanguage.TraditionalChinese => "確定",
        UiLanguage.SimplifiedChinese => "确定",
        _ => "OK"
    };

    public string ApplyAction => Language switch
    {
        UiLanguage.TraditionalChinese => "應用",
        UiLanguage.SimplifiedChinese => "应用",
        _ => "Apply"
    };

    public string WindowSettingsGroup => Language switch
    {
        UiLanguage.TraditionalChinese => "視窗與啟動",
        UiLanguage.SimplifiedChinese => "窗口与启动",
        _ => "Window and startup"
    };

    public string UsageSettingsGroup => Language switch
    {
        UiLanguage.TraditionalChinese => "用量",
        UiLanguage.SimplifiedChinese => "用量",
        _ => "Usage"
    };

    public string AppearanceSettingsGroup => Language switch
    {
        UiLanguage.TraditionalChinese => "外觀",
        UiLanguage.SimplifiedChinese => "外观",
        _ => "Appearance"
    };

    public string NotificationSettingsGroup => Language switch
    {
        UiLanguage.TraditionalChinese => "通知",
        UiLanguage.SimplifiedChinese => "通知",
        _ => "Notifications"
    };

    public string DateTimeSettingsGroup => Language switch
    {
        UiLanguage.TraditionalChinese => "日期與時間",
        UiLanguage.SimplifiedChinese => "日期与时间",
        _ => "Date and time"
    };

    public string LoggingSettingsGroup => Language switch
    {
        UiLanguage.TraditionalChinese => "記錄",
        UiLanguage.SimplifiedChinese => "日志",
        _ => "Logging"
    };

    public string PinOnTopAction => Language switch
    {
        UiLanguage.TraditionalChinese => "將視窗釘選在最上層",
        UiLanguage.SimplifiedChinese => "将窗口固定在最上层",
        _ => "Pin window on top"
    };

    public string UnpinFromTopAction => Language switch
    {
        UiLanguage.TraditionalChinese => "取消視窗置頂",
        UiLanguage.SimplifiedChinese => "取消窗口置顶",
        _ => "Unpin window from top"
    };

    public string UnsavedChangesTitle => Language switch
    {
        UiLanguage.TraditionalChinese => "尚未儲存變更",
        UiLanguage.SimplifiedChinese => "尚未保存更改",
        _ => "Unsaved changes"
    };

    public string UnsavedChangesMessage => Language switch
    {
        UiLanguage.TraditionalChinese => "設定中有尚未儲存的變更。要捨棄這些變更嗎？",
        UiLanguage.SimplifiedChinese => "设置中有尚未保存的更改。要放弃这些更改吗？",
        _ => "Your settings contain unsaved changes. Discard them?"
    };

    public string DiscardChangesAction => Language switch
    {
        UiLanguage.TraditionalChinese => "捨棄變更",
        UiLanguage.SimplifiedChinese => "放弃更改",
        _ => "Discard changes"
    };

    public string KeepEditingAction => Language switch
    {
        UiLanguage.TraditionalChinese => "繼續編輯",
        UiLanguage.SimplifiedChinese => "继续编辑",
        _ => "Keep editing"
    };

    public string ClaudeFiveHourTitle => Language switch
    {
        UiLanguage.TraditionalChinese => "目前工作階段",
        UiLanguage.SimplifiedChinese => "当前会话",
        _ => "Current session"
    };

    public string ClaudeWeeklyTitle => Language switch
    {
        UiLanguage.TraditionalChinese => "本週用量（全部）",
        UiLanguage.SimplifiedChinese => "本周用量（全部）",
        _ => "Current week (All)"
    };

    public string CombinedUsageHeaderTitle => "USAGE";

    public string ClaudeLowUsageAlertTitle => Language switch
    {
        UiLanguage.TraditionalChinese => "Claude 剩餘用量偏低",
        UiLanguage.SimplifiedChinese => "Claude 剩余用量偏低",
        _ => "Claude usage is running low"
    };

    public string ClaudeUsageResetTitle => Language switch
    {
        UiLanguage.TraditionalChinese => "Claude 用量已重置",
        UiLanguage.SimplifiedChinese => "Claude 用量已重置",
        _ => "Claude usage has reset"
    };

    public string FormatClaudeCreditsDetails(RateLimitExtraUsageState? extraUsage)
    {
        if (extraUsage is null)
        {
            return Language switch
            {
                UiLanguage.TraditionalChinese => "使用點數：--, 自動加購：--",
                UiLanguage.SimplifiedChinese => "使用点数：--, 自动加购：--",
                _ => "Usage credits: --, Auto-reload: --"
            };
        }

        if (!extraUsage.Enabled)
        {
            return Language switch
            {
                UiLanguage.TraditionalChinese => "使用點數：--, 自動加購：已停用",
                UiLanguage.SimplifiedChinese => "使用点数：--, 自动加购：已禁用",
                _ => "Usage credits: --, Auto-reload: Disabled"
            };
        }

        var credits = FormatClaudeCreditsAmount(extraUsage);
        return Language switch
        {
            UiLanguage.TraditionalChinese => $"使用點數：{credits}, 自動加購：已啟用",
            UiLanguage.SimplifiedChinese => $"使用点数：{credits}, 自动加购：已启用",
            _ => $"Usage credits: {credits}, Auto-reload: Enabled"
        };
    }

    private static string FormatClaudeCreditsAmount(RateLimitExtraUsageState extraUsage)
    {
        if (extraUsage.UsedAmount is not decimal used || extraUsage.LimitAmount is not decimal limit)
        {
            return "--";
        }

        var usedText = used.ToString("0.00", CultureInfo.InvariantCulture);
        var limitText = limit.ToString("0.00", CultureInfo.InvariantCulture);
        return extraUsage.Currency switch
        {
            null => $"{usedText} / {limitText}",
            "USD" => $"${usedText} / ${limitText}",
            _ => $"{usedText} / {limitText} {extraUsage.Currency}"
        };
    }

    public string LowUsageAlertOption => Language switch
    {
        UiLanguage.TraditionalChinese => "剩餘用量低於門檻時發出通知",
        UiLanguage.SimplifiedChinese => "剩余用量低于阈值时发出通知",
        _ => "Alert when remaining usage is below the threshold"
    };

    public string LowUsageAlertThresholdOption => Language switch
    {
        UiLanguage.TraditionalChinese => "低用量通知門檻（%）",
        UiLanguage.SimplifiedChinese => "低用量通知阈值（%）",
        _ => "Low-usage alert threshold (%)"
    };

    public string NotifyOnResetOption => Language switch
    {
        UiLanguage.TraditionalChinese => "用量重置時發出通知",
        UiLanguage.SimplifiedChinese => "用量重置时发出通知",
        _ => "Notify when usage resets"
    };

    public string LowUsageAlertTitle => Language switch
    {
        UiLanguage.TraditionalChinese => "Codex 剩餘用量偏低",
        UiLanguage.SimplifiedChinese => "Codex 剩余用量偏低",
        _ => "Codex usage is running low"
    };

    public string UsageResetTitle => Language switch
    {
        UiLanguage.TraditionalChinese => "Codex 用量已重置",
        UiLanguage.SimplifiedChinese => "Codex 用量已重置",
        _ => "Codex usage has reset"
    };

    public string FormatCreditDetails(
        string? creditBalance,
        bool automaticReloadEnabled)
    {
        var balance = FormatCreditBalance(creditBalance);
        return Language switch
        {
            UiLanguage.TraditionalChinese =>
                $"點數：{balance}, 自動儲值：{(automaticReloadEnabled ? "已啟用" : "已停用")}",
            UiLanguage.SimplifiedChinese =>
                $"点数：{balance}, 自动充值：{(automaticReloadEnabled ? "已启用" : "已禁用")}",
            _ =>
                $"Credits: {balance}, Automatic reload: {(automaticReloadEnabled ? "Enabled" : "Disabled")}"
        };
    }

    private static string FormatCreditBalance(string? creditBalance)
    {
        if (string.IsNullOrWhiteSpace(creditBalance))
        {
            return "--";
        }

        return decimal.TryParse(
            creditBalance,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var numericBalance)
            ? numericBalance.ToString("0.00", CultureInfo.InvariantCulture)
            : "--";
    }

    public string FormatLowUsageAlert(bool weekly, int remainingPercent)
    {
        var limit = FormatLimitName(weekly);
        return Language switch
        {
            UiLanguage.TraditionalChinese => $"{limit}剩餘 {remainingPercent}%。",
            UiLanguage.SimplifiedChinese => $"{limit}剩余 {remainingPercent}%。",
            _ => $"{limit} has {remainingPercent}% remaining."
        };
    }

    public string FormatResetNotification(bool weekly, int remainingPercent)
    {
        var limit = FormatLimitName(weekly);
        return Language switch
        {
            UiLanguage.TraditionalChinese =>
                $"{limit}已重置，目前剩餘 {remainingPercent}%。",
            UiLanguage.SimplifiedChinese =>
                $"{limit}已重置，目前剩余 {remainingPercent}%。",
            _ => $"{limit} has reset and is now {remainingPercent}% remaining."
        };
    }

    public string FormatFiveHourReset(DateTimeOffset localReset)
    {
        return FormatResetDateTime(localReset);
    }

    public string FormatWeeklyReset(DateTimeOffset localReset)
    {
        return FormatResetDateTime(localReset);
    }

    public string FormatUpdatedTime(DateTimeOffset updatedAt)
    {
        var dateTime = FormatDateTime(updatedAt, LastUpdatedDateTimeFormat);
        return Language switch
        {
            UiLanguage.TraditionalChinese => $"最後更新於 {dateTime}",
            UiLanguage.SimplifiedChinese => $"最后更新于 {dateTime}",
            _ => $"Last updated at {dateTime}"
        };
    }

    public string FormatUpdateInterval(int seconds)
    {
        var normalizedSeconds = UpdateIntervalOptions.Normalize(seconds);
        var minutes = (normalizedSeconds / 60d).ToString(
            "0.##",
            CultureInfo.InvariantCulture);
        return Language switch
        {
            UiLanguage.TraditionalChinese => $"每 {minutes} 分鐘",
            UiLanguage.SimplifiedChinese => $"每 {minutes} 分钟",
            _ when normalizedSeconds == 60 => "Every 1 minute",
            _ => $"Every {minutes} minutes"
        };
    }

    public string FormatTrayTooltip(
        RateLimitState? codexState,
        DateTimeOffset? codexUpdatedAt,
        RateLimitState? claudeState,
        DateTimeOffset? claudeUpdatedAt)
    {
        var updatedAt = claudeUpdatedAt ?? codexUpdatedAt;
        return string.Join(
            Environment.NewLine,
            "Claude Codex Usage Companion",
            FormatTrayUsageLine("Claude", TrayCurrentLabel, claudeState?.FiveHour),
            FormatTrayUsageLine("Claude", TrayWeeklyLabel, claudeState?.Weekly),
            FormatTrayUsageLine("Codex", TrayWeeklyLabel, codexState?.Weekly),
            updatedAt is null ? WaitingForData : FormatUpdatedTime(updatedAt.Value));
    }

    private string FormatTrayUsageLine(string provider, string label, RateLimitWindowState? window)
    {
        var remaining = window is null ? RemainingUnavailable : FormatRemaining(window.RemainingPercent);
        var reset = window?.ResetsAt is long unixSeconds
            ? FormatWeeklyReset(DateTimeOffset.FromUnixTimeSeconds(unixSeconds).ToLocalTime())
            : ResetUnavailable;
        return $"[{provider}] {label}: {remaining} ({reset})";
    }

    private string TrayCurrentLabel => Language switch
    {
        UiLanguage.TraditionalChinese => "目前",
        UiLanguage.SimplifiedChinese => "当前",
        _ => "Current"
    };

    private string TrayWeeklyLabel => Language switch
    {
        UiLanguage.TraditionalChinese => "每週",
        UiLanguage.SimplifiedChinese => "每周",
        _ => "Weekly"
    };

    public bool TryFormatDateTime(
        DateTimeOffset value,
        string? format,
        out string formatted)
    {
        if (!DateTimeFormatOptions.IsValid(format))
        {
            formatted = string.Empty;
            return false;
        }

        try
        {
            formatted = FormatDateTime(value, format!);
            return true;
        }
        catch (FormatException)
        {
            formatted = string.Empty;
            return false;
        }
    }

    private string FormatLimitName(bool weekly)
    {
        if (weekly)
        {
            return WeeklyTitle;
        }

        return FiveHourTitle;
    }

    private string FormatResetDateTime(DateTimeOffset localReset)
    {
        var dateTime = FormatDateTime(localReset, ResetDateTimeFormat);
        return Language switch
        {
            UiLanguage.TraditionalChinese => $"於 {dateTime} 重置",
            UiLanguage.SimplifiedChinese => $"于 {dateTime} 重置",
            _ => $"Resets at {dateTime}"
        };
    }

    private string FormatDateTime(DateTimeOffset value, string format)
    {
        return format switch
        {
            DateTimeFormatOptions.MonthDay => FormatMonthDay(value),
            DateTimeFormatOptions.MonthDayTime =>
                $"{FormatMonthDay(value)} {value.ToString("HH:mm", CultureInfo.InvariantCulture)}",
            DateTimeFormatOptions.YearMonthDay =>
                value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            DateTimeFormatOptions.YearMonthDayTime =>
                value.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
            DateTimeFormatOptions.HourMinuteSecond =>
                value.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
            DateTimeFormatOptions.HourMinute =>
                value.ToString("HH:mm", CultureInfo.InvariantCulture),
            _ => value.ToString(
                DateTimeFormatOptions.ToDotNetFormat(format),
                LanguageCulture())
        };
    }

    private string FormatMonthDay(DateTimeOffset value)
    {
        return Language == UiLanguage.English
            ? value.ToString("MMM d", CultureInfo.GetCultureInfo("en-US"))
            : $"{value.Month}月{value.Day}日";
    }

    private CultureInfo LanguageCulture()
    {
        return Language switch
        {
            UiLanguage.TraditionalChinese => CultureInfo.GetCultureInfo("zh-TW"),
            UiLanguage.SimplifiedChinese => CultureInfo.GetCultureInfo("zh-CN"),
            _ => CultureInfo.GetCultureInfo("en-US")
        };
    }
}
