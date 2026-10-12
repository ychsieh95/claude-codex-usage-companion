# Claude Codex Usage Companion for Linux

[繁體中文（zh-tw）](README.zh-TW.md) · [简体中文（zh-cn）](README.zh-CN.md)

Claude Codex Usage Companion is a local, open-source usage monitor for Claude Code and Codex on Linux. It includes:

- A compact always-on-top desktop companion built with Avalonia.
- A scriptable CLI with one-shot, JSON, and live-watch output.
- Codex `SessionStart` and `Stop` hooks that start or refresh one resident GUI process.
- Claude Code and Codex usage, stacked in one panel — both shown by default, and each can be turned off independently in Settings.
- English, Traditional Chinese, and Simplified Chinese usage text.
- A categorized settings window for language, theme, window position, usage alerts, reset notifications, usage logging, taskbar and system-tray icons, login autostart, and always-on-top behavior.
- Four system-tray icon styles: the original icon, Claude current-session remaining, Claude weekly remaining, or Codex session remaining.
- No telemetry; existing Claude credentials are renewed in place and tokens are never logged.

This Linux port is based on [gkfriend/codex-usage-companion](https://github.com/gkfriend/codex-usage-companion), whose original implementation targets Windows/WPF.

## v0.1.3

The panel now uses [QuotaGem](https://github.com/gyozalab/QuotaGem)-inspired provider headings and rounded bars, with the 5h/Weekly label, remaining percentage, and local reset time on one row. The columns stay aligned, and the panel fits its content and remains at the configured screen position when providers are hidden. Hover over a weekly bar to see credit details.

Settings sections are collapsible and ordered by common tasks: **Usage → Appearance → Window and startup → Notifications → Date and time → Logging → Updates**. **Expand all** and **Collapse all** control every section. Each section’s state is saved when Settings closes, even after **Cancel**, and restored on reopening or after an app restart; unapplied form edits are still discarded. Under **Appearance**, configure the background color (`#RRGGBB` or `#AARRGGBB`; blank follows the theme), then the label-to-percentage gap (0–300 pixels, default **128**) and percentage-to-reset-time gap (0–64 pixels, default **16**). Changes preview immediately; **OK/Apply** saves them and **Cancel** restores the saved appearance. Existing saved spacing values are preserved.

Under **Settings → Updates**, use **Check for updates** to compare the installed version with the latest stable GitHub release. If a newer version is available, **Open release page** opens its download page. **Automatically check for updates (startup and daily)** is enabled by default and checks at startup and every 24 hours while the app runs. Clear the checkbox and choose **OK/Apply** to disable automatic checks; manual checks remain available. Automatic checks show a desktop notification when a newer version is available. Downloads and installation are manual. Network errors appear in Settings for manual checks and are logged for automatic checks.

Hiding a provider's bars only changes its display. Turning off **Enable Claude usage** or **Enable Codex usage** stops that provider's polling and clears its errors. Errors from enabled providers remain visible until that provider recovers, even if its bars are hidden or another provider refreshes successfully. See the [changelog](CHANGELOG.md) for the full release notes.

Usage cards use 36-pixel progress bars, with extra space below weekly bars for credits and automatic reload information, and grow with larger fonts. Resize marks appear on hover or keyboard focus. Previous untouched 78/96-pixel defaults migrate to compact heights; custom resized layouts are preserved.

The **Expand all** icon has four outward arrows; **Collapse all** has four inward arrows. Hover for a tooltip. Saved gaps apply from the first display, including while usage is loading. The percentage column fits its text and spacing; changing either gap refits the panel width. When you manually widen the panel, extra space goes before the percentage column and the reset-time column remains near the right edge. Preset positions use the desktop working area to avoid reserved taskbars and panels.

**Reset to defaults** opens a **Yes/No** confirmation before restoring all preferences in the settings form, including card heights and spacing. **No**, Escape, or closing the confirmation leaves the form unchanged. Choose **OK/Apply** to save, or **Cancel** to discard the reset. Section expansion states are preserved. Resizing a card also changes its progress track and fill height.

The initial window fits all visible cards and the status line before opening, with only the normal bottom inset. After the first usage result arrives, the panel refits the waiting-text columns while retaining the configured 128/16-pixel default gaps.

## Screenshots

| English (`en-US`) | Traditional Chinese (`zh-tw`) | Simplified Chinese (`zh-cn`) |
| --- | --- | --- |
| ![English usage overlay](assets/screenshots/overlay-en.png) | ![Traditional Chinese usage overlay](assets/screenshots/overlay-zh-tw.png) | ![Simplified Chinese usage overlay](assets/screenshots/overlay-zh-cn.png) |

The overlay images use sample usage data. Settings sections can be expanded by clicking their titles.

### Settings

| English (`en-US`) | Traditional Chinese (`zh-tw`) | Simplified Chinese (`zh-cn`) |
| --- | --- | --- |
| ![English settings](assets/screenshots/settings-en.png) | ![Traditional Chinese settings](assets/screenshots/settings-zh-tw.png) | ![Simplified Chinese settings](assets/screenshots/settings-zh-cn.png) |

### Reset confirmation

The **Yes/No** labels are centered horizontally and vertically within their buttons. Regenerate the overlay, settings, and confirmation images with `bash scripts/capture-screenshots.sh` (.NET 10 SDK required).

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

Release packages are self-contained; a separate .NET runtime is not required.

## Install and verify Codex CLI

Claude Codex Usage Companion requires the official Codex CLI and its `app-server` subcommand. Install or update the latest Codex CLI on Linux with the [official installer](https://developers.openai.com/codex/cli):

```bash
curl -fsSL https://chatgpt.com/codex/install.sh | sh
```

Open a new terminal after installation, then sign in with ChatGPT or another available authentication method:

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

The first command should print an absolute path, the second should print a version, the third should confirm the active authentication method, and the final command should print the availability message. If `command -v codex` prints nothing, open a new terminal and make sure the install directory shown by the installer is on `PATH`.

The companion also searches `~/.local/bin`, `~/.npm-global/bin`, and `~/.codex/bin`. If Codex is installed elsewhere, set `CODEX_CLI_PATH=/absolute/path/to/codex`.

## Enable Claude usage (optional)

Claude Code usage is shown by default, alongside Codex. To use it, install and sign in to [Claude Code](https://code.claude.com), then run `claude` once so it writes `~/.claude/.credentials.json`. If you don't use Claude Code, turn off **Enable Claude usage** in Settings (or set `"enableClaudeUsage": false` in `settings.json`); an equivalent **Enable Codex usage** checkbox does the same for the Codex section.

Claude Code has no official API for this, unlike Codex. The companion reads the OAuth access token Claude Code already stores locally and calls Anthropic's own (undocumented) usage endpoint directly. That access token lasts only a few hours and the Claude CLI renews it only while the CLI itself runs, so after a reboot the stored token is normally stale. The companion therefore renews it the same way the CLI does — the OAuth refresh grant, using the refresh token already in `~/.claude/.credentials.json` — and writes the renewed pair back atomically, preserving every other field, holding the same lock file the CLI uses so the two never rotate the token at once. Tokens are never logged. If the refresh token itself has expired or been revoked, it shows an error asking you to run `claude` to sign in again. Because that endpoint is undocumented, it could change without notice; Claude usage is polled no more than once every 180 seconds regardless of the configured update interval. Set `CLAUDE_CREDENTIALS_PATH=/absolute/path/to/.credentials.json` or `CLAUDE_CLI_PATH=/absolute/path/to/claude` if either is installed somewhere the companion does not already search (`~/.local/bin`, `~/.npm-global/bin`, `~/.claude/local`).

## Install the desktop and CLI

Build or download the `.deb`, then:

```bash
sudo apt install ./claude-codex-usage-companion_0.1.3_amd64.deb
```

Launch **Claude Codex Usage Companion** from KDE's application menu, or run:

```bash
claude-codex-usage-companion gui
```

The package also installs the shorter `claude-codex-usage` alias.

The `.deb` creates the KDE/GNOME application-menu entry at `/usr/share/applications/claude-codex-usage-companion.desktop` and installs its icon under `/usr/share/icons/hicolor/scalable/apps/`. Login autostart is disabled by default. Enabling it in the settings window creates `${XDG_CONFIG_HOME:-$HOME/.config}/autostart/claude-codex-usage-companion.desktop`.

Enabling login autostart reveals a **Minimize the window** option (hidden otherwise). When both are enabled, the app launches at login without showing its window — usage still refreshes in the background — and reopening the app (from the application menu, the tray icon if enabled, or by running it again) brings the window back.

Use the **⚙ Settings** button to configure the taskbar icon, system tray, tray icon style, login autostart, language, theme, window position, low-usage alerts, reset notifications, usage logging, and always-on-top behavior. **OK** applies changes and closes the window, **Cancel** discards unapplied changes, and **Apply** saves changes without closing it. Under **Appearance**, set a background color using `#RRGGBB` or `#AARRGGBB` (blank uses the theme color), and adjust the gap between the percentage and reset time from 0 to 64 pixels. The horizontal gap between the 5h/Weekly label column and percentage column is adjustable from 0 to 300 pixels (default 128). The panel width adjusts to fit. Appearance changes preview immediately; **Cancel** restores the last saved appearance.

## Plasma 6 widget

The `.deb` also installs the **Claude Codex Usage** Plasma 6 widget. Right-click the Plasma desktop or panel, choose **Add Widgets**, search for `Claude Codex Usage`, and drag it onto the panel or desktop.

The panel indicator shows separate progress values for Claude's current session and week and Codex's five-hour and weekly limits. Its configuration page offers circle and bar views, controls the refresh interval, and selects which limits appear in both the panel and expanded popup; all four limits are enabled by default. The compact layout automatically flows horizontally in a wide widget and vertically in a tall widget. Open it to see local reset times, credits, refresh status, and provider-specific errors. It calls the companion's local `status --json` command, so credentials and network access remain in the companion rather than the widget.

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

`status`, `watch`, and their `--json` output show Claude before Codex, matching the overlay's card order. `--json` nests results as `{"claude": {"state": ..., "error": ...}, "codex": {"state": ..., "error": ...}}` (either key is omitted entirely when that provider is disabled) — this is a breaking change from the previous flat `--json` shape.

Set `CODEX_CLI_PATH=/absolute/path/to/codex` if Codex is not on `PATH`. Set `NO_COLOR=1` or pass `--no-color` to disable ANSI colors.

## Install as a Codex plugin

The Linux release build creates `CodexUsageCompanionMarketplace-v0.1.3-linux-x64.zip`. Extract it, add the extracted marketplace, then install and enable the plugin:

```bash
codex plugin marketplace add /path/to/extracted-marketplace
```

Review and trust the bundled `SessionStart` and `Stop` hooks when Codex asks. `PLUGIN_DATA` is used for settings when the app is launched by the plugin.

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

Language values: `en-US`, `zh-tw`, `zh-cn`. On first run, system `zh-TW`, `zh-HS`, `zh-Hant`, `zh-HK`, `zh-MO`, and other Traditional Chinese cultures default to `zh-tw`; system `zh-CN`, `zh-Hans`, `zh-SG`, and other Simplified Chinese cultures default to `zh-cn`; all other cultures default to `en-US`, as shown above. The legacy values `auto` and `en` remain accepted, and values are matched case-insensitively.

Theme values: `dark`, `light`, and `system`. `system` is the default and follows the desktop's current light or dark preference.

Tray icon style values are `original`, `claude-current-session`, `claude-weekly-session`, and `codex-session`. Numeric styles show bold, anti-aliased seven-segment digits without a percent sign on a dark background. Codex session uses the five-hour window when available and falls back to weekly usage otherwise.

Position values: `left-top`, `middle-top`, `right-top`, `left-center`, `middle-center`, `right-center`, `left-bottom`, `middle-bottom`, `right-bottom`, `custom`.

Selecting a position previews it immediately. **OK** or **Apply** keeps the new position; **Cancel** restores the most recently applied position. Legacy corner values such as `top-left` and `bottom-right` remain accepted.
Drag the header to move the window freely; the position switches to `custom` and the coordinates are saved. Selecting `custom` in Settings restores the saved location, or keeps the current location if none has been saved yet.

Keyboard shortcuts are available throughout the GUI. In the main window, press `F1` to list the shortcuts, `S` to open Settings, `Ctrl+R` to refresh usage, or `Esc` to close the window. The header's **?** button and the tray menu's **Keyboard shortcuts** item open the same list, which closes with `Esc`, `F1`, or **OK**. In Settings, press `Ctrl+S` to save or `Esc` to close. Closing Settings with unsaved changes asks for confirmation before discarding them.

The **Update interval** setting offers common values from 1 to 60 minutes. The current 1-minute interval is the default and minimum; a previously saved custom interval in that range remains selectable.

Low-usage alerts are disabled by default. When enabled, a desktop notification is sent once when either the five-hour or weekly remaining usage becomes strictly lower than the configured `1`–`100` percent threshold. The alert re-arms after that limit recovers to the threshold or higher. Reset notifications are independently disabled by default and are sent after a refreshed limit confirms a new reset window.

Reset date/time presets are `MMM D` (default), `MMM D HH:mm`, `yyyy-MM-dd`, and `yyyy-MM-dd HH:mm`. Last-updated presets are `HH:mm` (default), `HH:mm:ss`, `MMM D HH:mm`, and `yyyy-MM-dd HH:mm`. Both comboboxes also accept custom .NET date/time format strings, with uppercase `D` supported as the day token. A localized preview checks the format while typing; malformed input displays an error and disables **OK** and **Apply**. Month names and day notation are localized.

Opacity is limited to `0.5`–`1.0`, margin to `0`–`64` pixels, and update interval to `60`–`3600` seconds.

`enableClaudeUsage` and `enableCodexUsage` (both default `true`) independently show or hide each provider's section; the overlay adjusts its height automatically. Claude is polled no more than once every 180 seconds regardless of the configured update interval — see [Enable Claude usage (optional)](#enable-claude-usage-optional).

The **Displayed limits** checkboxes independently control Claude's current session and week and Codex's five-hour and weekly cards. The corresponding settings are `showClaudeSession`, `showClaudeWeekly`, `showCodexFiveHour`, and `showCodexWeekly`; the Codex five-hour card defaults to hidden to preserve the app's previous layout, while the other three default to visible.

## Usage update logging

Usage logging is disabled by default. When enabled, every successful refresh and refresh error is appended to the configured file. CSV is the default format, and the default path follows `${XDG_STATE_HOME:-$HOME/.local/state}/claude-codex-usage-companion/usage-history.csv`.

Supported formats are `txt` (one key/value record per line), `csv` (a header followed by data rows), and `jsonl` (one JSON object per line). Changing the format automatically changes the configured filename extension to `.txt`, `.csv`, or `.jsonl`. Records include the update timestamp, which provider (`claude` or `codex`) the entry is for, success or error status, five-hour and weekly remaining percentages and reset times, available reset credits, and any refresh error. Relative paths are resolved under the app's XDG state directory. The `provider` field is a breaking addition to the log schema for anyone parsing existing log files.

## Build

Install the .NET 10 SDK or newer, then run:

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

This stops your running companion, builds for the host architecture, runs `sudo apt reinstall -f` on the generated `.deb`, and launches the installed command. Run the script as your normal desktop user; sudo is requested only for package installation. The script stops on failure.

The build runs the test suite and writes a `.deb`, portable `.tar.gz`, Plasma `.plasmoid`, Codex marketplace `.zip`, and checksums under `artifacts/`.

For a development run:

```bash
dotnet run --project src/CodexUsageCompanion -- status
dotnet run --project src/CodexUsageCompanion -- gui
```

## Linux behavior

The GUI uses an owner-only Unix socket under `XDG_RUNTIME_DIR` to enforce one instance and accept refresh signals. Settings follow the XDG config convention; bounded logs are stored under `${XDG_STATE_HOME:-$HOME/.local/state}/claude-codex-usage-companion`.

On a Wayland desktop with XWayland available, the app uses Avalonia's X11 compatibility backend so the selected window position controls startup placement and later position changes. With native Wayland only, the compositor controls placement and may ignore this setting. X11 sessions use the X11 backend. Drag the header to move the panel, or drag its lower-right corner to resize it; cards and usage bars adapt to the width, and shorter windows scroll. The panel remains draggable and includes keyboard shortcuts, pin (always-on-top), settings, refresh, minimize, and close controls, in that order, with the two window controls set apart by a wider gap. The header's pin toggle, the Settings window's **Keep window always on top** checkbox, and the tray menu's **Keep window always on top** checkbox all reflect and update the same setting. The Claude **Current session**/**Current week (All)** section appears first, followed by the Codex section. Provider headings show the Claude or OpenAI icon. Below the weekly bars, the panel shows Codex credit balance and automatic reload, or Claude extra-usage spending and its monthly limit. These details also appear in tooltips. The system tray is disabled by default: closing the panel exits the resident process. When the tray is enabled, closing hides the panel while preserving its latest usage snapshot; background interval updates and Codex rate-limit notifications continue while hidden. Clicking the tray icon, choosing **Show window**, or launching the app again restores the cached state and its last on-screen position without an immediate refetch. The tray menu contains **Show window**, **Refresh usage**, **Keep window always on top**, **Start automatically when I sign in**, **Settings**, **Keyboard shortcuts**, and **Quit**, with separators between action groups. Choose **Quit** to stop the process. The tray tooltip shows the app name, then one line per usage window — `[Claude] Current: d% remaining (Resets at ...)`, `[Claude] Weekly: d% remaining (Resets at ...)`, and `[Codex] Weekly: d% remaining (Resets at ...)` — followed by `Last updated at HH:mm` using the selected last-updated date/time format.

Drag the small handle at the bottom of a usage card to change that card's height; the height is saved automatically. The handle also accepts Up/Down keys when focused. In **Settings → Appearance**, drag **Panel font size** (80–150%) to preview text, then choose **Apply** or **OK** to save it. The lower-right window resize mark appears on hover.

The app reads `account/rateLimits/read` from a local `codex app-server` process. This API may evolve, so future Codex releases can require compatibility changes. Claude usage relies on an undocumented Anthropic endpoint (see [Enable Claude usage (optional)](#enable-claude-usage-optional)) that could change without notice.

## Privacy and license

See [PRIVACY.md](PRIVACY.md), [SECURITY.md](SECURITY.md), and the [MIT License](LICENSE).
