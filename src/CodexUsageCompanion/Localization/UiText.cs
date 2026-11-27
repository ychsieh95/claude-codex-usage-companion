using System.Globalization;
using CodexUsageCompanion.Configuration;
using CodexUsageCompanion.Lifecycle;
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
    string SimplifiedChineseLanguage)
{
    private string UpdateText(string english, string traditional, string simplified) => Language switch
    {
        UiLanguage.TraditionalChinese => traditional,
        UiLanguage.SimplifiedChinese => simplified,
        _ => english
    };
    public string UpdateSettingsGroup => UpdateText("Updates", "更新", "更新");
    public string CheckForUpdatesAction => UpdateText("Check for updates", "檢查更新", "检查更新");
    public string AutoUpdateCheckOption => UpdateText("Automatically check for updates (startup and daily)", "自動檢查更新（啟動時及每天）", "自动检查更新（启动时及每天）");
    public string CheckingForUpdates => UpdateText("Checking for updates…", "正在檢查更新…", "正在检查更新…");
    public string AppUpToDate => UpdateText("You’re up to date.", "目前已是最新版本。", "当前已是最新版本。");
    public string UpdateCheckFailed => UpdateText("Unable to check for updates. Please try again later.", "無法檢查更新，請稍後再試。", "无法检查更新，请稍后重试。");
    public string OpenReleaseAction => UpdateText("Open release page", "開啟版本頁面", "打开版本页面");
    public string FormatAppUpdate(string version) => UpdateText($"Version {version} is available.", $"有新版本 {version} 可用。", $"有新版本 {version} 可用。");
    public string AutoInstallUpdatesOption => UpdateText("Automatically download and install updates", "自動下載並安裝更新", "自动下载并安装更新");
    public string InstallUpdateAction => UpdateText("Install update", "安裝更新", "安装更新");
    public string InstallNowAction => UpdateText("Install now", "立即安裝", "立即安装");
    public string DownloadingUpdate => UpdateText("Downloading and verifying the update…", "正在下載並驗證更新…", "正在下载并验证更新…");
    public string InstallingUpdate => UpdateText("Installing the update…", "正在安裝更新…", "正在安装更新…");
    public string UpdateInstallFailed => UpdateText(
        "Unable to install the update. Open the release page to update manually.",
        "無法安裝更新。請開啟版本頁面手動更新。",
        "无法安装更新。请打开版本页面手动更新。");
    public string UpdateInstallCancelled => UpdateText(
        "The update was not installed because the administrator password prompt was closed.",
        "已關閉管理員密碼提示，因此未安裝更新。",
        "已关闭管理员密码提示，因此未安装更新。");
    public string FormatUpdateInstalled(string version) => UpdateText(
        $"Version {version} is installed. The app is restarting.",
        $"已安裝版本 {version}。應用程式正在重新啟動。",
        $"已安装版本 {version}。应用正在重新启动。");
    public string FormatUpdateReadyToInstall(string version) => UpdateText(
        $"Version {version} is downloaded and verified. Install it now?",
        $"版本 {version} 已下載並驗證。要立即安裝嗎？",
        $"版本 {version} 已下载并验证。要立即安装吗？");
    public string InstallKindHint(InstallKind kind) => kind switch
    {
        InstallKind.Portable => UpdateText(
            "Updates replace this app's file and restart the app.",
            "更新會取代此應用程式的檔案並重新啟動應用程式。",
            "更新会替换此应用的文件并重新启动应用。"),
        InstallKind.Debian => UpdateText(
            "Updates download automatically. Installing asks for your administrator password.",
            "更新會自動下載。安裝時會要求輸入管理員密碼。",
            "更新会自动下载。安装时会要求输入管理员密码。"),
        InstallKind.Snap => UpdateText(
            "Snap packages cannot update themselves. Open the release page to update.",
            "Snap 套件無法自行更新。請開啟版本頁面進行更新。",
            "Snap 软件包无法自行更新。请打开版本页面进行更新。"),
        InstallKind.CodexPlugin => UpdateText(
            "Codex manages plugin updates.",
            "外掛程式的更新由 Codex 管理。",
            "插件的更新由 Codex 管理。"),
        _ => UpdateText(
            "This copy cannot update itself. Open the release page to update.",
            "此副本無法自行更新。請開啟版本頁面進行更新。",
            "此副本无法自行更新。请打开版本页面进行更新。")
    };

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
                "5 小時用量限制",
                "每週用量限制",
                "等待用量資料",
                "目前方案未提供此額度",
                "無法取得重置時間",
                "最小化",
                "重新整理用量",
                "隱藏至系統匣",
                "顯示視窗",
                "結束",
                "關閉",
                "設定",
                "設定 - Claude Codex Usage Companion",
                "啟用系統匣",
                "登入時自動啟動",
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
                "简体中文 (zh-cn)"),
            UiLanguage.SimplifiedChinese => new UiText(
                language,
                "5 小时用量限制",
                "每周用量限制",
                "等待用量数据",
                "当前方案未提供此额度",
                "未提供重置时间",
                "最小化",
                "刷新用量",
                "隐藏到系统托盘",
                "显示窗口",
                "退出",
                "关闭",
                "设置",
                "设置 - Claude Codex Usage Companion",
                "启用系统托盘",
                "登录时自动启动",
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
                "简体中文 (zh-cn)"),
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
                "简体中文 (zh-cn)")
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

    public string FormatPaceMarkerTooltip(int elapsedPercent) => Language switch
    {
        UiLanguage.TraditionalChinese =>
            $"平均速度指標：此週期已過 {elapsedPercent}%。若進度條在指標左側結束，代表使用速度比平均快。",
        UiLanguage.SimplifiedChinese =>
            $"平均速度指针：此周期已过 {elapsedPercent}%。若进度条在指针左侧结束，表示使用速度比平均快。",
        _ => $"Even-pace pointer: {elapsedPercent}% of this window has passed. " +
             "A bar that ends before the pointer means you are using it faster than an even pace."
    };

    public string ShowPacePointerOption => Language switch
    {
        UiLanguage.TraditionalChinese => "在進度條上顯示平均速度指標",
        UiLanguage.SimplifiedChinese => "在进度条上显示平均速度指针",
        _ => "Show even-pace pointer on bars"
    };

    public string OverlayDesignOption => UpdateText("Design", "設計", "设计");
    public string LatestOverlayDesign => UpdateText("Latest", "最新", "最新");
    public string PreviousOverlayDesign => UpdateText("Previous (v0.1.5)", "舊版（v0.1.5）", "旧版（v0.1.5）");
    public string PreviousDesignOnlyNote => UpdateText(
        "The two gap settings apply only to the Previous design.",
        "這兩個間距設定僅適用於舊版設計。",
        "这两个间距设置仅适用于旧版设计。");
    public string ResetTimeOnNameRowOption => UpdateText(
        "Show each limit's name and reset time on one row",
        "將每項限制的名稱與重置時間顯示在同一列",
        "将每项限制的名称与重置时间显示在同一行");
    public string HeaderBadgeOption => UpdateText("Header badge", "標題徽章", "标题徽章");
    public string LowestBadgeChoice => UpdateText("Lowest remaining limit", "剩餘最少的限制", "剩余最少的限制");
    public string AverageBadgeChoice => UpdateText("Average of shown limits", "顯示中限制的平均值", "显示中限制的平均值");
    public string ClaudeBadgeChoice => UpdateText("Claude (lowest remaining)", "Claude（剩餘最少）", "Claude（剩余最少）");
    public string CodexBadgeChoice => UpdateText("Codex (lowest remaining)", "Codex（剩餘最少）", "Codex（剩余最少）");
    public string HiddenBadgeChoice => UpdateText("Hidden", "隱藏", "隐藏");
    public string FormatAverageRemaining(int count, int remainingPercent) => UpdateText(
        $"Average remaining of {count} limits: {remainingPercent}%",
        $"{count} 項限制的平均剩餘：{remainingPercent}%",
        $"{count} 项限制的平均剩余：{remainingPercent}%");
    public string LimitRowsLabel => UpdateText("Limit rows", "限制列", "限制行");
    public string ProvidersLabel => UpdateText("Providers", "供應商", "提供方");
    public string SpacingLabel => UpdateText("Spacing", "間距", "间距");
    public string SystemTrayLabel => UpdateText("System tray", "系統匣", "系统托盘");
    public string StartupLabel => UpdateText("Startup", "啟動", "启动");
    public string WindowLabel => UpdateText("Window", "視窗", "窗口");
    public string AlertsLabel => UpdateText("Alerts", "通知", "通知");
    public string ProjectLinkLabel => UpdateText("Project:", "專案：", "项目：");
    public string OriginalProjectLinkLabel => UpdateText("Original project:", "原始專案：", "原始项目：");
    public string AboutLabel => UpdateText("About", "關於", "关于");
    public string UsageOverviewTitle =>UpdateText("Usage overview", "用量概覽", "用量概览");
    public string RemainingCaption => UpdateText("REMAINING", "剩餘", "剩余");
    public string CodexFiveHourShortTitle => UpdateText("5-hour limit", "5 小時限制", "5 小时限制");
    public string CodexWeeklyShortTitle => UpdateText("Weekly limit", "每週限制", "每周限制");

    /// <summary>A reset countdown with the local reset time, such as "Resets in 2h 14m · 16:23".</summary>
    public string FormatResetCountdown(DateTimeOffset localReset, DateTimeOffset now)
    {
        var untilReset = localReset - now;
        var duration = FormatDuration(untilReset);
        // The countdown already gives the day, so a reset within a day needs only its time.
        var when = untilReset < TimeSpan.FromDays(1)
            ? localReset.ToString("HH:mm", CultureInfo.InvariantCulture)
            : FormatDateTime(localReset, ResetDateTimeFormat);
        return UpdateText($"Resets in {duration} · {when}", $"{duration}後重置 · {when}", $"{duration}后重置 · {when}");
    }

    public string FormatDuration(TimeSpan duration)
    {
        var minutes = Math.Max(0, (int)Math.Ceiling(duration.TotalMinutes));
        var days = minutes / 1440;
        var hours = minutes % 1440 / 60;
        var mins = minutes % 60;
        if (days > 0)
            return hours > 0
                ? UpdateText($"{days}d {hours}h", $"{days} 天 {hours} 小時", $"{days} 天 {hours} 小时")
                : UpdateText($"{days}d", $"{days} 天", $"{days} 天");
        if (hours > 0)
            return mins > 0
                ? UpdateText($"{hours}h {mins}m", $"{hours} 小時 {mins} 分", $"{hours} 小时 {mins} 分")
                : UpdateText($"{hours}h", $"{hours} 小時", $"{hours} 小时");
        return UpdateText(minutes == 1 ? "1 minute" : $"{minutes} minutes", $"{minutes} 分鐘", $"{minutes} 分钟");
    }

    /// <summary>The pace tooltip of the Latest design, comparing the remaining share with an even pace.</summary>
    public string FormatPaceTooltip(int expectedRemainingPercent, int remainingPercent)
    {
        var delta = remainingPercent - expectedRemainingPercent;
        var pace = Math.Abs(delta) < 3
            ? UpdateText("on pace", "符合平均速度", "符合平均速度")
            : delta > 0
                ? UpdateText("ahead of pace", "比平均速度慢", "比平均速度慢")
                : UpdateText("running faster than pace", "比平均速度快", "比平均速度快");
        return UpdateText(
            $"Even pace: about {expectedRemainingPercent}% should remain now. Your usage is {pace}.",
            $"平均速度：目前應剩餘約 {expectedRemainingPercent}%。你的使用速度{pace}。",
            $"平均速度：目前应剩余约 {expectedRemainingPercent}%。你的使用速度{pace}。");
    }

    public string FormatLowestRemaining(string provider, string limit, int remainingPercent) => UpdateText(
        $"Lowest remaining: {provider} {limit}, {remainingPercent}%",
        $"剩餘最少：{provider} {limit}，{remainingPercent}%",
        $"剩余最少：{provider} {limit}，{remainingPercent}%");

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

    public string DisplayedLimitsOption => Language switch
    {
        UiLanguage.TraditionalChinese => "顯示的用量限制",
        UiLanguage.SimplifiedChinese => "显示的用量限制",
        _ => "Displayed limits"
    };

    public string ShowClaudeSessionOption => Language switch
    {
        UiLanguage.TraditionalChinese => "Claude 目前工作階段",
        UiLanguage.SimplifiedChinese => "Claude 当前会话",
        _ => "Claude current session"
    };

    public string ShowClaudeWeeklyOption => Language switch
    {
        UiLanguage.TraditionalChinese => "Claude 本週用量（全部）",
        UiLanguage.SimplifiedChinese => "Claude 本周用量（全部）",
        _ => "Claude current week (All)"
    };

    public string ShowCodexFiveHourOption => Language switch
    {
        UiLanguage.TraditionalChinese => "Codex 5 小時用量限制",
        UiLanguage.SimplifiedChinese => "Codex 5 小时用量限制",
        _ => "Codex 5-hour limit"
    };

    public string ShowCodexWeeklyOption => Language switch
    {
        UiLanguage.TraditionalChinese => "Codex 每週用量限制",
        UiLanguage.SimplifiedChinese => "Codex 每周用量限制",
        _ => "Codex weekly limit"
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
        UiLanguage.TraditionalChinese => "套用",
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

    public string FontSizeOption => Language switch
    {
        UiLanguage.TraditionalChinese => "面板字體大小",
        UiLanguage.SimplifiedChinese => "面板字体大小",
        _ => "Panel font size"
    };

    public string FontSizePreview => Language switch
    {
        UiLanguage.TraditionalChinese => "預覽：目前工作階段 · 剩餘 78%",
        UiLanguage.SimplifiedChinese => "预览：当前会话 · 剩余 78%",
        _ => "Preview: Current session · 78% remaining"
    };

    public string FormatResizeCardHint(string title) => Language switch
    {
        UiLanguage.TraditionalChinese => $"調整 {title} 卡片高度（向上／向下鍵）",
        UiLanguage.SimplifiedChinese => $"调整{title}卡片高度（上／下方向键）",
        _ => $"Resize {title} card (Up/Down keys)"
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

    public string ShortcutsAction => Language switch
    {
        UiLanguage.TraditionalChinese => "鍵盤快速鍵",
        UiLanguage.SimplifiedChinese => "键盘快捷键",
        _ => "Keyboard shortcuts"
    };

    public string ShortcutsTitle => Language switch
    {
        UiLanguage.TraditionalChinese => "鍵盤快速鍵 - Claude Codex Usage Companion",
        UiLanguage.SimplifiedChinese => "键盘快捷键 - Claude Codex Usage Companion",
        _ => "Keyboard shortcuts - Claude Codex Usage Companion"
    };

    public string MainWindowShortcutsGroup => Language switch
    {
        UiLanguage.TraditionalChinese => "主視窗",
        UiLanguage.SimplifiedChinese => "主窗口",
        _ => "Main window"
    };

    public string SettingsWindowShortcutsGroup => Language switch
    {
        UiLanguage.TraditionalChinese => "設定視窗",
        UiLanguage.SimplifiedChinese => "设置窗口",
        _ => "Settings window"
    };

    public string ShowShortcutsShortcut => Language switch
    {
        UiLanguage.TraditionalChinese => "顯示鍵盤快速鍵",
        UiLanguage.SimplifiedChinese => "显示键盘快捷键",
        _ => "Show keyboard shortcuts"
    };

    public string OpenSettingsShortcut => Language switch
    {
        UiLanguage.TraditionalChinese => "開啟設定",
        UiLanguage.SimplifiedChinese => "打开设置",
        _ => "Open Settings"
    };

    public string CloseWindowShortcut => Language switch
    {
        UiLanguage.TraditionalChinese => "關閉視窗",
        UiLanguage.SimplifiedChinese => "关闭窗口",
        _ => "Close the window"
    };

    public string SaveSettingsShortcut => Language switch
    {
        UiLanguage.TraditionalChinese => "儲存設定",
        UiLanguage.SimplifiedChinese => "保存设置",
        _ => "Save settings"
    };

    public string CloseSettingsShortcut => Language switch
    {
        UiLanguage.TraditionalChinese => "關閉設定",
        UiLanguage.SimplifiedChinese => "关闭设置",
        _ => "Close Settings"
    };

    public IReadOnlyList<ShortcutGroup> ShortcutGroups => new[]
    {
        new ShortcutGroup(
            MainWindowShortcutsGroup,
            new[]
            {
                new ShortcutHint("F1", ShowShortcutsShortcut),
                new ShortcutHint("S", OpenSettingsShortcut),
                new ShortcutHint("Ctrl+R", RefreshAction),
                new ShortcutHint("Esc", CloseWindowShortcut)
            }),
        new ShortcutGroup(
            SettingsWindowShortcutsGroup,
            new[]
            {
                new ShortcutHint("Ctrl+S", SaveSettingsShortcut),
                new ShortcutHint("Esc", CloseSettingsShortcut)
            })
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

    public string ConfirmResetDefaultsTitle => Language switch
    {
        UiLanguage.TraditionalChinese => "還原預設值？",
        UiLanguage.SimplifiedChinese => "恢复默认值？",
        _ => "Reset to defaults?"
    };

    public string ConfirmResetDefaultsMessage => Language switch
    {
        UiLanguage.TraditionalChinese => "要將所有設定還原為預設值嗎？還原後請按確定或套用儲存。",
        UiLanguage.SimplifiedChinese => "要将所有设置恢复为默认值吗？恢复后请点击确定或应用保存。",
        _ => "Reset all settings to their default values? Choose OK or Apply afterward to save them."
    };

    public string YesAction => Language switch
    {
        UiLanguage.TraditionalChinese => "是",
        UiLanguage.SimplifiedChinese => "是",
        _ => "Yes"
    };

    public string NoAction => Language switch
    {
        UiLanguage.TraditionalChinese => "否",
        UiLanguage.SimplifiedChinese => "否",
        _ => "No"
    };

    public string ResetDefaultsAction => Language switch
    {
        UiLanguage.TraditionalChinese => "還原預設值",
        UiLanguage.SimplifiedChinese => "恢复默认值",
        _ => "Reset to defaults"
    };

    public string BackgroundColorOption => Language switch
    {
        UiLanguage.TraditionalChinese => "背景顏色",
        UiLanguage.SimplifiedChinese => "背景颜色",
        _ => "Background color"
    };

    public string BackgroundColorFormatHint => UpdateText(
        "Use #RRGGBB or #AARRGGBB.", "格式為 #RRGGBB 或 #AARRGGBB。", "格式为 #RRGGBB 或 #AARRGGBB。");

    public string ThemeDefaultColor => Language switch
    {
        UiLanguage.TraditionalChinese => "留白以使用主題顏色",
        UiLanguage.SimplifiedChinese => "留空以使用主题默认颜色",
        _ => "Leave blank to use the theme color"
    };

    public string InvalidBackgroundColor => Language switch
    {
        UiLanguage.TraditionalChinese => "請輸入 #RRGGBB 或 #AARRGGBB，或留白。",
        UiLanguage.SimplifiedChinese => "请输入 #RRGGBB 或 #AARRGGBB，或留空。",
        _ => "Enter #RRGGBB or #AARRGGBB, or leave blank."
    };

    public string LabelPercentageSpacingOption => Language switch
    {
        UiLanguage.TraditionalChinese => "5h／每週標籤與百分比的間距（像素）",
        UiLanguage.SimplifiedChinese => "5h／每周标签与百分比的间距（像素）",
        _ => "Gap between 5h/Weekly and percentage (pixels)"
    };

    public string PercentageTimeSpacingOption => Language switch
    {
        UiLanguage.TraditionalChinese => "百分比與重置時間的間距（像素）",
        UiLanguage.SimplifiedChinese => "百分比与重置时间的间距（像素）",
        _ => "Gap between percentage and reset time (pixels)"
    };

    public string ProgressFiveHourLabel => Language switch
    {
        UiLanguage.TraditionalChinese => "5h",
        UiLanguage.SimplifiedChinese => "5h",
        _ => "5h"
    };

    public string ProgressWeeklyLabel => Language switch
    {
        UiLanguage.TraditionalChinese => "週",
        UiLanguage.SimplifiedChinese => "周",
        _ => "Weekly"
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
                UiLanguage.TraditionalChinese => "使用點數：--，自動加購：--",
                UiLanguage.SimplifiedChinese => "使用点数：--，自动加购：--",
                _ => "Usage credits: --, Auto-reload: --"
            };
        }

        if (!extraUsage.Enabled)
        {
            return Language switch
            {
                UiLanguage.TraditionalChinese => "使用點數：--，自動加購：已停用",
                UiLanguage.SimplifiedChinese => "使用点数：--，自动加购：已禁用",
                _ => "Usage credits: --, Auto-reload: Disabled"
            };
        }

        var credits = FormatClaudeCreditsAmount(extraUsage);
        return Language switch
        {
            UiLanguage.TraditionalChinese => $"使用點數：{credits}，自動加購：已啟用",
            UiLanguage.SimplifiedChinese => $"使用点数：{credits}，自动加购：已启用",
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
                $"點數：{balance}，自動儲值：{(automaticReloadEnabled ? "已啟用" : "已停用")}",
            UiLanguage.SimplifiedChinese =>
                $"点数：{balance}，自动充值：{(automaticReloadEnabled ? "已启用" : "已禁用")}",
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

    public string FormatProgressReset(DateTimeOffset localReset)
    {
        var dateTime = ResetDateTimeFormat is DateTimeFormatOptions.MonthDay or DateTimeFormatOptions.MonthDayTime
            ? localReset.ToString("MM/dd HH:mm", CultureInfo.InvariantCulture)
            : FormatDateTime(localReset, ResetDateTimeFormat);
        var localLabel = Language switch
        {
            UiLanguage.TraditionalChinese => "本機",
            UiLanguage.SimplifiedChinese => "本机",
            _ => "Local"
        };
        return $"{dateTime} {localLabel}";
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
