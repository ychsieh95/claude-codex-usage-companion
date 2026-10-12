# Privacy

Claude Codex Usage Companion runs locally on Linux. It does not include telemetry, analytics, advertising, or its own external service.

The companion starts the locally installed `codex app-server` over standard input/output and reads only account rate-limit responses. Authentication remains managed by Codex; the companion does not read, copy, or store Codex authentication tokens.

Claude usage is controlled by the `enableClaudeUsage` setting, which defaults to `true` (a matching `enableCodexUsage` setting independently controls the Codex section, also defaulting to `true`). When Claude usage is enabled, the companion reads the OAuth access token that Claude Code itself already stores at `~/.claude/.credentials.json` (or the path set by `CLAUDE_CREDENTIALS_PATH`) and sends it, only to `https://api.anthropic.com`, to read the same account-level usage figures Claude Code itself displays. When that access token has expired — the normal state after a reboot, since the Claude CLI renews it only while running — the companion renews it through the same OAuth refresh grant the CLI uses, sending the stored refresh token only to `https://platform.claude.com/v1/oauth/token`, and writes the renewed pair back into that same credentials file. That write is the only modification this app makes to the file: it is atomic, keeps the file owner-readable only, preserves every field the app does not own, and takes the same lock the CLI takes so the two never rotate the token at once. Tokens are held in memory only for the duration of those requests, are never logged, and are never sent anywhere except those two Anthropic endpoints. If the refresh token itself has expired or been revoked, the companion shows an error asking the user to run `claude` to sign in again. Users who do not use Claude Code can turn `enableClaudeUsage` off in Settings, which stops the companion from reading that file or contacting Anthropic entirely.

Local files:

- Settings are stored as `settings.json` in the Codex plugin data directory when available, otherwise under `${XDG_CONFIG_HOME:-$HOME/.config}/claude-codex-usage-companion`.
- If login autostart is enabled, the app creates `${XDG_CONFIG_HOME:-$HOME/.config}/autostart/claude-codex-usage-companion.desktop`; disabling the option removes that file.
- Bounded diagnostic logs are stored under `${XDG_STATE_HOME:-$HOME/.local/state}/claude-codex-usage-companion`.
- Optional usage update logging is disabled by default. When enabled, the selected TXT, CSV, or JSONL file stores refresh timestamps, which provider (Claude or Codex) the entry is for, success or error status, remaining percentages, reset times, available reset credits, and refresh errors. The user can choose its local path.
- Single-instance messages use an owner-only Unix socket under `XDG_RUNTIME_DIR`.
- No settings or logs are uploaded by the plugin.

The desktop menu entry, optional login autostart, taskbar and optional system-tray presence, and minimize or hide-to-tray behavior do not add background data collection. The source code and release workflows are public so these behaviors can be inspected.
