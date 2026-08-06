# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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

[0.1.0]: https://github.com/ychsieh95/claude-codex-usage-companion/releases/tag/v0.1.0
