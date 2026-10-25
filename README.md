# Claude Codex Usage Companion for Linux

[繁體中文（zh-tw）](README.zh-TW.md) · [简体中文（zh-cn）](README.zh-CN.md)

Claude Codex Usage Companion is a local, open-source usage monitor for Claude Code and Codex on Linux. It has these features:

- A small desktop panel that uses Avalonia and stays above other windows.
- A CLI for scripts, single results, JSON output, and continuous updates.
- Codex `SessionStart` and `Stop` hooks that start or refresh one resident GUI process.
- Claude Code and Codex usage in one panel. Both providers are enabled by default. Settings can disable each provider separately.
- Usage text in English, Traditional Chinese, and Simplified Chinese.
- Settings for language, theme, window position, alerts, notifications, logging, icons, automatic startup, and window behavior.
- Four tray icon styles: original, Claude session remaining, Claude weekly remaining, and Codex session remaining.
- No telemetry. The app renews existing Claude credentials in their original file. The app never writes tokens to logs.

This Linux version is based on [gkfriend/codex-usage-companion](https://github.com/gkfriend/codex-usage-companion). The original application uses Windows and WPF.

## v0.1.3

The panel uses provider headings and rounded bars based on [QuotaGem](https://github.com/gyozalab/QuotaGem). Each row shows the 5h/Weekly label, remaining percentage, and local reset time. The columns stay aligned. If you hide a provider, the panel fits its content and keeps its configured screen position. Move the pointer over a weekly bar to see credit details.

Settings sections use this order: **Usage → Appearance → Window and startup → Notifications → Date and time → Logging → Updates**. Select a section title to expand or collapse it. **Expand all** and **Collapse all** control every section.

The app saves section expansion states when Settings closes, including after **Cancel**. It restores these states when Settings opens again or the app restarts. **Cancel** discards form changes that you have not applied.

Under **Appearance**, set these values:

- Background color: `#RRGGBB` or `#AARRGGBB`. An empty field uses the theme color.
- Gap between the label and percentage: 0–300 pixels. The default is **128**.
- Gap between the percentage and reset time: 0–64 pixels. The default is **16**.

Appearance changes appear immediately. Select **OK** or **Apply** to save them. Select **Cancel** to restore the saved appearance. The app keeps existing saved spacing values.

Under **Settings → Updates**, select **Check for updates** to check for a newer stable GitHub release. If a newer version is available, select **Open release page** to open its download page.

**Automatically check for updates (startup and daily)** is enabled by default. The app checks at startup and every 24 hours while it runs. To disable automatic checks, clear the checkbox. Then select **OK** or **Apply**. Manual checks remain available.

Automatic checks show a desktop notification when a newer version is available. Download updates manually. Install the downloaded updates manually. Settings shows network errors from manual checks. The app writes network errors from automatic checks to its log.

If you hide a provider's bars, its usage checks continue. To stop its usage checks, clear **Enable Claude usage** or **Enable Codex usage**. This also clears that provider's errors.

Errors from enabled providers stay visible until the provider recovers. Hidden bars and successful updates from another provider do not clear these errors. See the [changelog](CHANGELOG.md) for all release notes.

Usage cards have 36-pixel progress bars. Weekly cards have extra space below the bars for credits and automatic reload information. Cards grow to fit larger fonts. Resize marks appear when the pointer is over them or they have keyboard focus.

The app replaces unchanged 78/96-pixel default card heights with smaller heights. It keeps heights that you changed manually.

The **Expand all** icon has four outward arrows. The **Collapse all** icon has four inward arrows. Move the pointer over an icon to see its tooltip.

The app applies saved gaps from the first display, including while usage loads. The percentage column fits its text and spacing. Changes to either gap adjust the panel width. If you widen the panel manually, extra space appears before the percentage column. The reset-time column stays near the right edge. Preset positions use the desktop working area to avoid reserved taskbars and panels.

Select **Reset to defaults** to open a **Yes/No** confirmation. Select **Yes** to restore all preferences in the settings form, including card heights and spacing. Select **No** to keep the form unchanged. Escape and closing the confirmation also keep the form unchanged.

Select **OK** or **Apply** to save the reset. Select **Cancel** to discard it. Section expansion states stay the same. Changes to card height also change the height of the progress track and fill.

Before it opens, the window fits all visible cards and the status line. The bottom has only the normal inset. After the first usage result, the panel adjusts the columns that contained the waiting text. It keeps the configured gaps, which default to 128 and 16 pixels.

## Screenshots

| English (`en-US`) | Traditional Chinese (`zh-tw`) | Simplified Chinese (`zh-cn`) |
| --- | --- | --- |
| ![English usage overlay](assets/screenshots/overlay-en.png) | ![Traditional Chinese usage overlay](assets/screenshots/overlay-zh-tw.png) | ![Simplified Chinese usage overlay](assets/screenshots/overlay-zh-cn.png) |

The panel images use sample usage data. Select a settings section title to expand it.

### Settings

| English (`en-US`) | Traditional Chinese (`zh-tw`) | Simplified Chinese (`zh-cn`) |
| --- | --- | --- |
| ![English settings](assets/screenshots/settings-en.png) | ![Traditional Chinese settings](assets/screenshots/settings-zh-tw.png) | ![Simplified Chinese settings](assets/screenshots/settings-zh-cn.png) |

### Reset confirmation

The **Yes/No** labels are centered horizontally and vertically in their buttons. Screenshot generation requires the .NET 10 SDK. To generate the panel, settings, and confirmation images, run `bash scripts/capture-screenshots.sh`.

| English (`en-US`) | Traditional Chinese (`zh-tw`) | Simplified Chinese (`zh-cn`) |
| --- | --- | --- |
| ![English reset confirmation](assets/screenshots/reset-confirmation-en.png) | ![Traditional Chinese reset confirmation](assets/screenshots/reset-confirmation-zh-tw.png) | ![Simplified Chinese reset confirmation](assets/screenshots/reset-confirmation-zh-cn.png) |

### Plasma 6 widget

| Compact circles | Expanded popup |
| --- | --- |
| ![Plasma 6 widget compact circles](assets/screenshots/plasma-widget-circles.png) | ![Plasma 6 widget expanded popup](assets/screenshots/plasma-widget-expanded.png) |

## Requirements

- Kubuntu 26.04 or another modern x64/ARM64 Linux distribution.
- A Wayland desktop or an X11 session.
- Codex CLI installed, authenticated, and available as `codex` on `PATH`.
- For the `.deb`: `libx11-6`, `libice6`, `libsm6`, `libfontconfig1`, `libharfbuzz0b`, `fonts-noto-cjk`, and `libnotify-bin`.

Release packages include the .NET runtime. You do not need a separate .NET runtime installation.

## Install and verify Codex CLI

The app requires the official Codex CLI and its `app-server` subcommand. To install or update Codex CLI on Linux, use the [official installer](https://developers.openai.com/codex/cli):

```bash
curl -fsSL https://chatgpt.com/codex/install.sh | sh
```

After installation, open a new terminal. Sign in with ChatGPT or another available authentication method:

```bash
codex login
```

Verify the executable path, version, authentication, and required `app-server` subcommand:

```bash
command -v codex
codex --version
codex login status
codex app-server --help >/dev/null && echo "Codex app-server is available"
```

The commands should give these results, in order:

1. The absolute path to `codex`.
2. The Codex version.
3. The active authentication method.
4. The message `Codex app-server is available`.

If `command -v codex` gives no output, open a new terminal. Make sure that `PATH` includes the installation directory shown by the installer.

The app also searches `~/.local/bin`, `~/.npm-global/bin`, and `~/.codex/bin`. If Codex is in another location, set `CODEX_CLI_PATH=/absolute/path/to/codex`.

## Enable Claude usage (optional)

The panel shows Claude Code and Codex usage by default. To use Claude usage, install [Claude Code](https://code.claude.com). Sign in to Claude Code. Run `claude` once to create `~/.claude/.credentials.json`.

If you do not use Claude Code, clear **Enable Claude usage** in Settings. You can also set `"enableClaudeUsage": false` in `settings.json`. The **Enable Codex usage** checkbox controls Codex in the same way.

Claude Code has no official usage API. The app reads the OAuth access token that Claude Code stores locally. It uses this token to call Anthropic's undocumented usage endpoint.

The access token lasts a few hours. Claude CLI renews it only while the CLI runs. After a restart of the computer, the stored token is usually expired.

The app renews the token with the OAuth refresh grant. It uses the refresh token in `~/.claude/.credentials.json`. It writes both renewed tokens to that file in one atomic operation. All other fields stay the same. The app uses the same lock file as Claude CLI to prevent simultaneous token changes. The app never writes tokens to logs.

If the refresh token has expired or been revoked, the app shows an error. Run `claude` to sign in again.

The undocumented endpoint can change without notice. The app checks Claude usage at most once every 180 seconds, regardless of the update interval.

The app searches `~/.local/bin`, `~/.npm-global/bin`, and `~/.claude/local` for Claude CLI. If your credential file or CLI is in another location, set the applicable variable:

- `CLAUDE_CREDENTIALS_PATH=/absolute/path/to/.credentials.json`
- `CLAUDE_CLI_PATH=/absolute/path/to/claude`

## Install the desktop and CLI

Build or download the `.deb` package. Install it with this command:

```bash
sudo apt install ./claude-codex-usage-companion_0.1.3_amd64.deb
```

Launch **Claude Codex Usage Companion** from KDE's application menu, or run:

```bash
claude-codex-usage-companion gui
```

The package also installs the shorter `claude-codex-usage` alias.

The `.deb` creates a KDE/GNOME application menu entry at `/usr/share/applications/claude-codex-usage-companion.desktop`. It installs the icon under `/usr/share/icons/hicolor/scalable/apps/`.

Automatic startup at login is disabled by default. If you enable it in Settings, the app creates `${XDG_CONFIG_HOME:-$HOME/.config}/autostart/claude-codex-usage-companion.desktop`.

Enable automatic startup at login to show the **Minimize the window** option. Otherwise, this option stays hidden. If both options are enabled, the app starts at login with its window hidden. Usage continues to update in the background.

To show the window, open the app from the application menu. You can also use the tray icon, if enabled, or run the app again.

Select **⚙ Settings** to configure the app. Settings includes taskbar and system-tray icons, tray icon style, and automatic startup at login. It also includes language, theme, position, low-usage alerts, reset notifications, logging, and always-on-top behavior.

- **OK** saves changes and closes Settings.
- **Cancel** discards changes that you have not applied.
- **Apply** saves changes and keeps Settings open.

Under **Appearance**, set a background color with `#RRGGBB` or `#AARRGGBB`. Leave the field empty to use the theme color. Set the gap between the percentage and reset time to 0–64 pixels. Set the gap between the 5h/Weekly label and percentage to 0–300 pixels. The default gaps are 16 and 128 pixels, respectively.

The panel width adjusts to fit. Appearance changes appear immediately. Select **Cancel** to restore the last saved appearance.

## Plasma 6 widget

The `.deb` also installs the **Claude Codex Usage** Plasma 6 widget. To add it:

1. Right-click the Plasma desktop or panel.
2. Select **Add Widgets**.
3. Search for `Claude Codex Usage`.
4. Drag the widget onto the panel or desktop.

The widget shows four limits: Claude's current session and week, and Codex's five-hour and weekly limits. All four limits are enabled by default. The configuration page controls circle or bar views, the update interval, and displayed limits. The selected limits appear in the panel and expanded popup.

A wide widget arranges the indicators horizontally. A tall widget arranges them vertically. Open the widget to see local reset times, credits, update status, and errors for each provider.

The widget calls the app's local `status --json` command. The app handles credentials and network access.

Release builds also include a standalone `.plasmoid` package. Install one without root access with:

```bash
kpackagetool6 --type Plasma/Applet --install ./claude-codex-usage-companion-0.1.3.plasmoid
```

For a development checkout, install or update the current source package with `bash scripts/install-plasma-widget.sh`. The companion CLI must remain installed and available on `PATH`.

## CLI

Print current limits:

```bash
claude-codex-usage status
```

Return machine-readable JSON:

```bash
claude-codex-usage status --json
```

Continuously refresh the terminal every 30 seconds:

```bash
claude-codex-usage watch --interval 30
```

Print the settings path and effective settings:

```bash
claude-codex-usage config
```

`status`, `watch`, and their `--json` output show Claude before Codex. This matches the panel's card order. JSON results use this structure: `{"claude": {"state": ..., "error": ...}, "codex": {"state": ..., "error": ...}}`. If a provider is disabled, its key is absent. This replaces the previous flat JSON structure. Scripts that read the previous structure need changes.

If Codex is not on `PATH`, set `CODEX_CLI_PATH=/absolute/path/to/codex`. To disable ANSI colors, set `NO_COLOR=1` or use `--no-color`.

## Install as a Codex plugin

The Linux release build creates `CodexUsageCompanionMarketplace-v0.1.3-linux-x64.zip`. Extract this file. Add the extracted marketplace with this command:

```bash
codex plugin marketplace add /path/to/extracted-marketplace
```

Install the plugin from the added marketplace. Enable the plugin. When Codex asks, review the included `SessionStart` and `Stop` hooks. Trust the hooks to enable them. When the plugin starts the app, the app uses `PLUGIN_DATA` for settings.

## Settings

The default settings file is:

```text
${XDG_CONFIG_HOME:-$HOME/.config}/claude-codex-usage-companion/settings.json
```

```json
{
  "showClaudeSession": true,
  "showClaudeWeekly": true,
  "showCodexFiveHour": false,
  "showCodexWeekly": true,
  "enableClaudeUsage": true,
  "enableCodexUsage": true,
  "enableSystemTray": false,
  "trayIconStyle": "original",
  "showTaskbarIcon": true,
  "enableAutoUpdateCheck": true,
  "startOnBoot": false,
  "minimizeOnStart": false,
  "language": "en-US",
  "theme": "system",
  "settingsSections": {
    "usage": true,
    "appearance": true,
    "window": true,
    "notifications": true,
    "dateTime": true,
    "logging": true,
    "updates": true
  },
  "backgroundColor": "",
  "percentageTimeSpacing": 16,
  "labelPercentageSpacing": 128,
  "enableLowUsageAlert": false,
  "lowUsageAlertThresholdPercent": 20,
  "notifyOnReset": false,
  "enableUsageLogging": false,
  "usageLogFilePath": "/home/you/.local/state/claude-codex-usage-companion/usage-history.csv",
  "usageLogFormat": "csv",
  "position": "right-bottom",
  "opacity": 1.0,
  "margin": 16,
  "alwaysOnTop": true,
  "refreshIntervalSeconds": 60,
  "resetDateTimeFormat": "MMM D",
  "lastUpdatedDateTimeFormat": "HH:mm"
}
```

Language values are `en-US`, `zh-tw`, and `zh-cn`. On the first run, the app selects a language from the system culture:

- `zh-TW`, `zh-HS`, `zh-Hant`, `zh-HK`, `zh-MO`, and other Traditional Chinese cultures use `zh-tw`.
- `zh-CN`, `zh-Hans`, `zh-SG`, and other Simplified Chinese cultures use `zh-cn`.
- All other cultures use `en-US`.

The app also accepts the legacy values `auto` and `en`. Letter case does not affect language matching.

Theme values: `dark`, `light`, and `system`. `system` is the default and follows the desktop's current light or dark preference.

Tray icon style values are `original`, `claude-current-session`, `claude-weekly-session`, and `codex-session`. Numeric styles show bold, anti-aliased seven-segment digits on a dark background. They do not show a percent sign. Codex session uses the five-hour limit when available. Otherwise, it uses weekly usage.

Position values: `left-top`, `middle-top`, `right-top`, `left-center`, `middle-center`, `right-center`, `left-bottom`, `middle-bottom`, `right-bottom`, `custom`.

Select a position to see it immediately. Select **OK** or **Apply** to keep it. Select **Cancel** to restore the last applied position. The app also accepts legacy corner values such as `top-left` and `bottom-right`.

Drag the header to move the window freely. The app changes the position to `custom` and saves the coordinates. Select `custom` in Settings to restore the saved location. If no location is saved, the window keeps its current location.

Use these shortcuts in the main window:

- `F1`: Show the shortcut list.
- `S`: Open Settings.
- `Ctrl+R`: Refresh usage.
- `Esc`: Close the window.

The header's **?** button and the tray menu's **Keyboard shortcuts** item open the same shortcut list. Press `Esc` or `F1` to close the list. You can also select **OK**.

In Settings, press `Ctrl+S` to save changes. Press `Esc` to close Settings. If you close Settings with unsaved changes, the app asks for confirmation before it discards them.

The **Update interval** setting has common values from 1 to 60 minutes. The default and minimum are 1 minute. Previously saved custom intervals in this range remain available.

Low-usage alerts are disabled by default. When enabled, the app checks the remaining five-hour and weekly usage against the configured threshold. The threshold range is `1`–`100` percent. The app sends one desktop notification when either limit falls below the threshold. That alert becomes available again after the remaining usage returns to the threshold or higher.

Reset notifications have a separate setting and are disabled by default. The app sends them when a usage update confirms a new reset window.

Reset date/time presets are `MMM D` (default), `MMM D HH:mm`, `yyyy-MM-dd`, and `yyyy-MM-dd HH:mm`. Last-updated presets are `HH:mm` (default), `HH:mm:ss`, `MMM D HH:mm`, and `yyyy-MM-dd HH:mm`.

Both fields accept custom .NET date/time format strings. Uppercase `D` is supported as the day token. A preview uses the selected language to check the format as you type. An invalid format shows an error and disables **OK** and **Apply**. Month names and day notation use the selected language.

These settings have the following ranges:

- Opacity: `0.5`–`1.0`.
- Margin: `0`–`64` pixels.
- Update interval: `60`–`3600` seconds.

`enableClaudeUsage` and `enableCodexUsage` control each provider separately. Both default to `true`. Disabling a provider hides its section and stops its usage checks. The panel height adjusts automatically.

Claude usage checks occur at most once every 180 seconds, regardless of the update interval. See [Enable Claude usage (optional)](#enable-claude-usage-optional).

The **Displayed limits** checkboxes control individual cards:

- `showClaudeSession`: Claude current session. Visible by default.
- `showClaudeWeekly`: Claude weekly usage. Visible by default.
- `showCodexFiveHour`: Codex five-hour usage. Hidden by default to keep the previous layout.
- `showCodexWeekly`: Codex weekly usage. Visible by default.

## Usage update logging

Usage logging is disabled by default. When enabled, the app adds each successful update and update error to the configured file. The default format is CSV. The default path is `${XDG_STATE_HOME:-$HOME/.local/state}/claude-codex-usage-companion/usage-history.csv`.

The app supports these log formats:

- `txt`: One key/value record per line.
- `csv`: A header followed by data rows.
- `jsonl`: One JSON object per line.

A format change automatically changes the configured filename extension to `.txt`, `.csv`, or `.jsonl`. Relative paths use the app's XDG state directory.

Each record includes these fields:

- Update timestamp.
- Provider: `claude` or `codex`.
- Success or error status.
- Five-hour and weekly remaining percentages and reset times.
- Available reset credits.
- Any update error.

The added `provider` field changes the log structure. Parsers for previous log files can need changes.

## Build

Install the .NET 10 SDK or newer. Run this command:

```bash
bash scripts/build-linux.sh
```

For ARM64:

```bash
bash scripts/build-linux.sh linux-arm64
```

To rebuild, reinstall, and restart the installed app:

```bash
./scripts/rebuild-reinstall.sh
```

The script stops your running app and builds for the host architecture. It reinstalls the generated `.deb` with `sudo apt reinstall -f`. Then it starts the installed app. The script stops if a step fails.

Run the script as your normal desktop user. Only package installation requires sudo.

The local rebuild script disables NuGet vulnerability auditing. This prevents `NU1900` warnings when the audit service is unavailable. Downloads still require NuGet access if dependencies are not cached.

To enable auditing, run `NUGET_AUDIT=true ./scripts/rebuild-reinstall.sh`. Direct `build-linux.sh` runs and CI keep auditing enabled by default.

The build runs the test suite. It writes these files under `artifacts/`:

- A `.deb` package.
- A portable `.tar.gz` archive.
- A Plasma `.plasmoid` package.
- A Codex marketplace `.zip` archive.
- Checksums.

For a development run:

```bash
dotnet run --project src/CodexUsageCompanion -- status
dotnet run --project src/CodexUsageCompanion -- gui
```

## Linux behavior

The GUI uses a Unix socket under `XDG_RUNTIME_DIR` that only the owner can access. The socket permits one app instance and accepts refresh signals. Settings follow the XDG config convention. The app stores logs with a size limit under `${XDG_STATE_HOME:-$HOME/.local/state}/claude-codex-usage-companion`.

On Wayland desktops with XWayland, the app uses Avalonia's X11 compatibility backend. The configured position controls the initial window location and later position changes. With native Wayland only, the compositor controls placement and can ignore this setting. X11 sessions use the X11 backend.

Drag the header to move the panel. Drag the lower-right corner to resize it. Cards and usage bars adjust to the width. Shorter windows permit scrolling.

The header controls appear in this order: keyboard shortcuts, pin, settings, refresh, minimize, and close. A wider gap separates the minimize and close controls. The pin button controls the always-on-top setting. The **Keep window always on top** checkboxes in Settings and the tray menu control the same setting.

Claude's **Current session** and **Current week (All)** cards appear before Codex. Provider headings show the Claude or OpenAI icon. Below weekly bars, the panel shows Codex credit balance and automatic reload information. For Claude, it shows extra-usage spending and the monthly limit. These details also appear in tooltips.

The system tray is disabled by default. If the tray is disabled, closing the panel stops the app. If the tray is enabled, closing the panel hides it and keeps the latest usage data. Scheduled updates and Codex rate-limit notifications continue while the panel is hidden.

To restore the panel, select the tray icon or **Show window**, or start the app again. The panel restores its saved usage data and last screen position. It does not request new usage data immediately.

The tray menu groups these actions with separators:

- **Show window**
- **Refresh usage**
- **Keep window always on top**
- **Start automatically when I sign in**
- **Settings**
- **Keyboard shortcuts**
- **Quit**

Select **Quit** to stop the app.

The tray tooltip starts with the app name. It then shows one line for each usage window, for example:

- `[Claude] Current: d% remaining (Resets at ...)`
- `[Claude] Weekly: d% remaining (Resets at ...)`
- `[Codex] Weekly: d% remaining (Resets at ...)`

The last line is `Last updated at HH:mm`. This line uses the selected last-updated date/time format.

Drag the small handle at the bottom of a usage card to change its height. The app saves the height automatically. When the handle has keyboard focus, use the Up/Down keys to adjust it.

Under **Settings → Appearance**, drag **Panel font size** (80–150%) to see the text size. Select **Apply** or **OK** to save it. The lower-right window resize mark appears when the pointer is over it.

The app reads `account/rateLimits/read` from a local `codex app-server` process. Changes to this API in future Codex releases can require app updates. Claude usage uses an undocumented Anthropic endpoint that can change without notice. See [Enable Claude usage (optional)](#enable-claude-usage-optional).

## Documentation style

Use [ASD-STE100 Simplified Technical English](https://www.asd-ste100.org/) as the writing guide for the English README:

- Use short sentences and the active voice.
- Give one instruction in each sentence.
- Use a maximum of 20 words in procedural sentences and 25 words in descriptive sentences.
- Use the same term for the same item.
- Keep commands, paths, setting keys, and UI labels unchanged.

Use `README.md` as the source for both Chinese translations. Keep their instructions, examples, and technical details aligned with the English version.

## Privacy and license

See [PRIVACY.md](PRIVACY.md), [SECURITY.md](SECURITY.md), and the [MIT License](LICENSE).
