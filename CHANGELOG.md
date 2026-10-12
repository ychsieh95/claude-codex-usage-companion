# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.3] - 2026-10-12

### Added

- A localized Reset to defaults button opens a Yes/No confirmation before restoring the settings form, including layout defaults; changes are saved only with OK/Apply.
- Window movement and resizing on Wayland desktops, per-card vertical resizing, and a font-size slider with preview.
- A `custom` window position that saves coordinates after dragging the app window and restores them at launch.
- Custom panel background colors with optional alpha transparency, live preview, validation, and a theme-color fallback.
- Configurable horizontal spacing between the 5h/Weekly labels and percentages (default 128 pixels), and between percentages and reset times (default 16 pixels).
- Collapsible settings sections ordered A–Z: Appearance, Date and time, Logging, Notifications, Usage, and Window and startup.
- Expand all and Collapse all settings icons with localized tooltips and accessible labels. Each section’s state is saved on closing (including Cancel) and restored across app restarts without saving unapplied form edits.
- `scripts/rebuild-reinstall.sh` stops the current user’s companion, runs the Linux build for the host architecture, reinstalls the generated `.deb` using `sudo apt reinstall -f`, and launches the installed app. A failed stop, build, or installation prevents relaunching.

### Changed

- Updated Avalonia dependencies to 12.1.3 and synchronized application, plugin, widget, and release artifact versions.
- Redesigned the overlay with QuotaGem-inspired provider headings and compact rounded progress bars containing the usage label, remaining percentage, reset icon, and local reset time.
- Aligned percentage and reset/status columns across providers. The panel fits its content and selected spacing; weekly credit and automatic reload details appear below the progress bars and remain available in tooltips.
- Updated dark and light colors and simplified the toolbar styling.
- The companion now writes to `~/.claude/.credentials.json` when it renews a token. The write is atomic, stays owner-readable only, preserves every field the app does not own, and takes the same lock file the Claude CLI uses so the two never rotate the refresh token at once. Tokens are still never logged.
- Percentage columns fit their text and configured spacing instead of using a fixed 50/100-pixel width. Changing a gap refits the panel in both directions; ordinary usage refreshes preserve manually resized windows.
- Default usage cards use 36-pixel bars with additional room for weekly credit details, scale with the font setting, and show resize marks only on hover or focus. Previous untouched 78/96-pixel defaults migrate to compact heights; custom resized layouts are preserved.
- Refreshed English, Traditional Chinese, and Simplified Chinese overlay, settings, and reset-confirmation screenshots and README instructions.

### Fixed

- Wait for the initial enabled-provider refresh (including errors) before displaying the overlay, so live-data column sizing is completed while hidden.
- Prepare theme templates, layout, card sizes, and placement before the overlay first appears, avoiding visible initialization changes.
- Refit the overlay after settings layout updates so enabling the Codex 5-hour limit keeps weekly credits and the status line visible without scrolling.
- Center Yes/No button labels horizontally and vertically in the Reset to defaults confirmation.
- Measure the header and scrolling content directly before the ScrollViewer template is ready, so the initial window fits all visible cards instead of opening at the minimum height. The first display includes every visible card and the status line, with only the normal bottom inset.
- Progress tracks and fills now resize with their cards for saved sizes, dragging, keyboard resizing, and font changes.
- Skip Avalonia XAML compilation for the C#-only UI, avoiding an intermittent build-time DLL lock.
- Restore reliable tray registration by waiting until Avalonia exports the StatusNotifierItem before installing the tooltip bridge.
- Apply the selected window position at startup and on settings changes through XWayland when available; use the primary monitor’s working area to avoid reserved desktop panels.
- Restore the last on-screen position after hiding the window to the tray.
- Restrict replacement Claude credentials files to owner access during token refresh.
- Panel positioning now follows its configured screen anchor after provider visibility and content-size changes, including delayed native resize events.
- Disabled providers stop polling and their pending results no longer display stale errors. Re-enabling a provider resumes refreshing without restarting the app.
- Provider errors are tracked independently: a successful Codex refresh no longer clears an active Claude error. Hiding Claude bars preserves its errors; disabling Claude usage clears them.
- Removed extra right-side spacing from the settings list.
- Claude usage no longer stalls at "Waiting for usage data" after a login autostart. The stored OAuth access token expires after a few hours and the Claude CLI renews it only while the CLI itself runs, so following a reboot the companion was reading an expired token and reporting "Claude session expired" until the user happened to open Claude Code. It now renews the token itself through the same OAuth refresh grant the CLI uses, and also retries once when the server rejects a token that has not reached its recorded expiry.
- Saved gap settings and placeholder text are applied before the first display. The panel refits when the first usage result replaces waiting text, so startup respects the 128/16-pixel defaults without reopening Settings; manually resized widths remain preserved.
- The reset-time column stays at the progress bar’s right edge with a ten-pixel inset when the window is widened, instead of leaving an unused tail.
- Preset overlay positions use the desktop working area so left-bottom and other anchors avoid reserved taskbars and panels.

## [0.1.2] - 2026-08-10

### Added

- A Plasma 6 widget with compact circle or bar views and an expanded popup showing reset times, credits, and provider-specific errors.
- Independent display toggles for Claude's current session and weekly limits and Codex's five-hour and weekly limits.
- A standalone `.plasmoid` release artifact and a source-checkout installation script for the Plasma widget.

### Changed

- The overlay now computes its height from the visible usage cards and remains anchored to the configured screen corner after resizing.
- Refreshed the README screenshots and removed the stale package-rename upgrade note.

### Migration

- Existing `showFiveHourLimit` settings migrate to `showCodexFiveHour`; the legacy key is removed the next time settings are saved.

## [0.1.1] - 2026-08-09

### Added

- Keyboard shortcuts list: a **?** button in the panel header and a **Keyboard shortcuts** item in the system-tray menu open a localized list of every shortcut, grouped by window. `F1` opens the same list, and `Esc`, `F1`, or **OK** closes it.
- A shutdown watchdog that exits the process if the shutdown sequence has not finished within 10 seconds, so a stalled step can no longer leave a resident process holding the single-instance lock.

### Changed

- Reordered the panel header controls to keyboard shortcuts, pin (always-on-top), settings, refresh, minimize, and close, with wider spacing separating the two window controls from the panel actions.
- Refreshed the README screenshots for all three languages.

### Fixed

- **Quit** (and closing the panel with the system tray disabled) hid the window but left the process running. Disposing the single-instance lease blocked the dispatcher thread waiting for its own accept loop, whose continuations were queued back to that same thread; the wedged dispatcher then prevented the shutdown sequence from ever calling `Shutdown()`. The accept loop now runs off the dispatcher and both disposal paths are bounded.

## [0.1.0] - 2026-08-01

### Added

- Initial Linux port of Claude Codex Usage Companion, based on [gkfriend/codex-usage-companion](https://github.com/gkfriend/codex-usage-companion) (originally Windows/WPF).
- Compact always-on-top desktop companion built with Avalonia.
- Scriptable CLI with one-shot, JSON, and live-watch output.
- Codex `SessionStart` and `Stop` hooks that start or refresh one resident GUI process.
- English, Traditional Chinese, and Simplified Chinese usage text.
- Settings window for language, theme, window position, usage alerts, reset notifications, usage logging, system tray, login autostart, and always-on-top behavior.
- `.deb` package with KDE/GNOME application-menu entry and icon.
- Claude Code usage support: an `enableClaudeUsage` setting (default on) adds a Claude section (current session and current week, all models) stacked below the existing Codex section in the overlay, tray tooltip, usage log, and CLI `status`/`watch` commands, with a colored dot marking each card's provider and a `Usage credits: used / limit` line for Claude's pay-as-you-go extra usage. Reads the OAuth access token Claude Code itself already maintains in `~/.claude/.credentials.json` (never written or refreshed by this app) and calls Anthropic's usage endpoint directly, polled no more often than every 180 seconds regardless of the configured refresh interval. `CLAUDE_CREDENTIALS_PATH` and `CLAUDE_CLI_PATH` environment variables mirror the existing `CODEX_CLI_PATH` override pattern.
- A matching `enableCodexUsage` setting (default on) independently shows or hides the Codex section, with a Settings checkbox alongside Claude's; the overlay height, CLI output, and usage log all adapt to whichever providers are enabled, including both being off.
- Main-window keyboard shortcuts: `S` opens Settings, `Ctrl+R` refreshes usage, and `Esc` closes the window.
- Settings keyboard shortcuts: `Ctrl+S` saves, while `Esc` closes the active settings or warning window.
- A localized confirmation warning when closing Settings with unsaved changes, with options to discard the changes or continue editing.
- Four configurable system-tray icon styles: the original icon, Claude current-session remaining, Claude weekly remaining, and Codex session remaining (with weekly fallback when the five-hour window is unavailable).
- Bold, anti-aliased seven-segment remaining-usage digits on a dark tray-icon background, without a percent sign.
- **Refresh usage** and **Start automatically when I sign in** actions in the system-tray context menu.
- A categorized Settings layout with separators and an **Apply** button that saves without closing the window.

### Changed

- Renamed the package, CLI commands, XDG paths, desktop entry, and autostart file from `codex-usage-companion` to `codex-claude-usage-companion`, and then again to `claude-codex-usage-companion`. Existing `settings.json` migrates automatically on first run from whichever older package's config is found; old `.deb`s should be removed manually (`sudo apt remove codex-usage-companion` and/or `sudo apt remove codex-claude-usage-companion`) since each rename used a different package name.
- **Breaking:** `status --json`/`watch --json` now nest results under `claude`/`codex` keys (in that order) instead of a flat object (each key is present only when its provider is enabled).
- **Breaking:** the usage-update log gains a `provider` column/field (`codex` or `claude`) in all three formats (txt/csv/jsonl).
- The Claude section (current session/week) now appears before the Codex section in the overlay, tray tooltip, and CLI output.
- Card titles show each provider's own icon (the official Claude mark and the OpenAI mark) instead of a `[Codex]`/`[Claude]` text prefix.
- Settings actions now appear in the standard **OK**, **Cancel**, **Apply** order and are localized in all supported languages.
- Updated the application, tests, CI, and release builds to .NET 10 and C# 14.

### Notes

- No telemetry. This app never stores or writes authentication tokens of its own; when Claude usage is enabled, it reads the OAuth access token Claude Code already stores locally and sends it only to Anthropic's official API over HTTPS.

[0.1.3]: https://github.com/ychsieh95/claude-codex-usage-companion/releases/tag/v0.1.3
[0.1.2]: https://github.com/ychsieh95/claude-codex-usage-companion/releases/tag/v0.1.2
[0.1.1]: https://github.com/ychsieh95/claude-codex-usage-companion/releases/tag/v0.1.1
[0.1.0]: https://github.com/ychsieh95/claude-codex-usage-companion/releases/tag/v0.1.0
