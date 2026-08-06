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
- No telemetry and no authentication tokens stored by this app.

This Linux port is based on [gkfriend/codex-usage-companion](https://github.com/gkfriend/codex-usage-companion), whose original implementation targets Windows/WPF.

## Screenshots

| English (`en-US`) | Traditional Chinese (`zh-tw`) | Simplified Chinese (`zh-cn`) |
| --- | --- | --- |
| ![English usage overlay](assets/screenshots/overlay-en.png) | ![Traditional Chinese usage overlay](assets/screenshots/overlay-zh-tw.png) | ![Simplified Chinese usage overlay](assets/screenshots/overlay-zh-cn.png) |

## Requirements

- Kubuntu 26.04 or another modern x64/ARM64 Linux distribution.
- X11 or an XWayland-capable Wayland desktop (the default on Kubuntu).
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

Claude Code has no official API for this, unlike Codex. The companion reads the OAuth access token Claude Code already stores locally and calls Anthropic's own (undocumented) usage endpoint directly — it never reads, writes, or refreshes that credentials file itself. If the token has expired, it shows an error asking you to run `claude` again rather than trying to refresh the session on its own. Because that endpoint is undocumented, it could change without notice; Claude usage is polled no more than once every 180 seconds regardless of the configured update interval. Set `CLAUDE_CREDENTIALS_PATH=/absolute/path/to/.credentials.json` or `CLAUDE_CLI_PATH=/absolute/path/to/claude` if either is installed somewhere the companion does not already search (`~/.local/bin`, `~/.npm-global/bin`, `~/.claude/local`).

## Install the desktop and CLI

Upgrading from an older package: this project has been renamed twice (`codex-usage-companion` → `codex-claude-usage-companion` → `claude-codex-usage-companion`). Run `sudo apt remove codex-usage-companion` and/or `sudo apt remove codex-claude-usage-companion`, whichever is currently installed, before installing the new `.deb`. Each rename used a different package name, so `apt` will not replace an old install automatically, and leaving an old one installed would autostart two resident tray processes. Existing settings migrate automatically on first run of the new package, from whichever older package's settings file is found.

Build or download the `.deb`, then:

```bash
sudo apt install ./claude-codex-usage-companion_0.1.0_amd64.deb
```

Launch **Claude Codex Usage Companion** from KDE's application menu, or run:

```bash
claude-codex-usage-companion gui
```

The package also installs the shorter `claude-codex-usage` alias.

The `.deb` creates the KDE/GNOME application-menu entry at `/usr/share/applications/claude-codex-usage-companion.desktop` and installs its icon under `/usr/share/icons/hicolor/scalable/apps/`. Login autostart is disabled by default. Enabling it in the settings window creates `${XDG_CONFIG_HOME:-$HOME/.config}/autostart/claude-codex-usage-companion.desktop`.

Enabling login autostart reveals a **Minimize the window** option (hidden otherwise). When both are enabled, the app launches at login without showing its window — usage still refreshes in the background — and reopening the app (from the application menu, the tray icon if enabled, or by running it again) brings the window back.

Use the **⚙ Settings** button to configure the taskbar icon, system tray, tray icon style, login autostart, language, theme, window position, low-usage alerts, reset notifications, usage logging, and always-on-top behavior. **OK** applies changes and closes the window, **Cancel** discards unapplied changes, and **Apply** saves changes without closing it.

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

The Linux release build creates `CodexUsageCompanionMarketplace-v0.1.0-linux-x64.zip`. Extract it, add the extracted marketplace, then install and enable the plugin:

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
  "showFiveHourLimit": false,
  "enableClaudeUsage": true,
  "enableCodexUsage": true,
  "enableSystemTray": false,
  "trayIconStyle": "original",
  "showTaskbarIcon": true,
  "startOnBoot": false,
  "minimizeOnStart": false,
  "language": "en-US",
  "theme": "system",
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

Position values: `left-top`, `middle-top`, `right-top`, `left-center`, `middle-center`, `right-center`, `left-bottom`, `middle-bottom`, `right-bottom`.

Selecting a position previews it immediately. **OK** or **Apply** keeps the new position; **Cancel** restores the most recently applied position. Legacy corner values such as `top-left` and `bottom-right` remain accepted.

Keyboard shortcuts are available throughout the GUI. In the main window, press `S` to open Settings, `Ctrl+R` to refresh usage, or `Esc` to close the window. In Settings, press `Ctrl+S` to save or `Esc` to close. Closing Settings with unsaved changes asks for confirmation before discarding them.

The **Update interval** setting offers common values from 1 to 60 minutes. The current 1-minute interval is the default and minimum; a previously saved custom interval in that range remains selectable.

Low-usage alerts are disabled by default. When enabled, a desktop notification is sent once when either the five-hour or weekly remaining usage becomes strictly lower than the configured `1`–`100` percent threshold. The alert re-arms after that limit recovers to the threshold or higher. Reset notifications are independently disabled by default and are sent after a refreshed limit confirms a new reset window.

Reset date/time presets are `MMM D` (default), `MMM D HH:mm`, `yyyy-MM-dd`, and `yyyy-MM-dd HH:mm`. Last-updated presets are `HH:mm` (default), `HH:mm:ss`, `MMM D HH:mm`, and `yyyy-MM-dd HH:mm`. Both comboboxes also accept custom .NET date/time format strings, with uppercase `D` supported as the day token. A localized preview checks the format while typing; malformed input displays an error and disables **OK** and **Apply**. Month names and day notation are localized.

Opacity is limited to `0.5`–`1.0`, margin to `0`–`64` pixels, and update interval to `60`–`3600` seconds.

`enableClaudeUsage` and `enableCodexUsage` (both default `true`) independently show or hide each provider's section; the overlay adjusts its height automatically. Claude is polled no more than once every 180 seconds regardless of the configured update interval — see [Enable Claude usage (optional)](#enable-claude-usage-optional).

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

The build runs the test suite and writes a `.deb`, portable `.tar.gz`, Codex marketplace `.zip`, and checksums under `artifacts/`.

For a development run:

```bash
dotnet run --project src/CodexUsageCompanion -- status
dotnet run --project src/CodexUsageCompanion -- gui
```

## Linux behavior

The GUI uses an owner-only Unix socket under `XDG_RUNTIME_DIR` to enforce one instance and accept refresh signals. Settings follow the XDG config convention; bounded logs are stored under `${XDG_STATE_HOME:-$HOME/.local/state}/claude-codex-usage-companion`.

Avalonia uses X11/XWayland by default on Linux. A Wayland compositor may apply its own window-positioning or always-on-top policy. The panel remains draggable and includes minimize, pin (always-on-top), settings, refresh, and close controls. The header's pin toggle, the Settings window's **Keep window always on top** checkbox, and the tray menu's **Keep window always on top** checkbox all reflect and update the same setting. The Claude **Current session**/**Current week (All)** section appears first, followed by the Codex section. Purchased **Credits** appear below the Codex weekly usage bar with exactly two decimal places as `Credits: 23.75, Automatic reload: Enabled/Disabled`; missing automatic-reload state is displayed as `Disabled`. Claude's weekly card shows `Usage credits: 6.32 / 100.00, Auto-reload: Enabled/Disabled` reflecting its optional pay-as-you-go extra usage credits (amount spent against the monthly cap, not a prepaid balance). Each card title shows its provider's own icon (the Claude mark or the OpenAI mark). The system tray is disabled by default: closing the panel exits the resident process. When the tray is enabled, closing hides the panel while preserving its latest usage snapshot; background interval updates and Codex rate-limit notifications continue while hidden. Clicking the tray icon, choosing **Show window**, or launching the app again restores the cached state without an immediate refetch. The tray menu contains **Show window**, **Refresh usage**, **Keep window always on top**, **Start automatically when I sign in**, **Settings**, and **Quit**, with separators between action groups. Choose **Quit** to stop the process. The tray tooltip shows the app name, then one line per usage window — `[Claude] Current: d% remaining (Resets at ...)`, `[Claude] Weekly: d% remaining (Resets at ...)`, and `[Codex] Weekly: d% remaining (Resets at ...)` — followed by `Last updated at HH:mm` using the selected last-updated date/time format.

The app reads `account/rateLimits/read` from a local `codex app-server` process. This API may evolve, so future Codex releases can require compatibility changes. Claude usage relies on an undocumented Anthropic endpoint (see [Enable Claude usage (optional)](#enable-claude-usage-optional)) that could change without notice.

## Privacy and license

See [PRIVACY.md](PRIVACY.md), [SECURITY.md](SECURITY.md), and the [MIT License](LICENSE).
