# Privacy

Claude Codex Usage Companion runs locally on Linux. It does not include telemetry, analytics, advertising, or its own external service.

The companion starts the locally installed `codex app-server` over standard input/output and reads only account rate-limit responses. Authentication remains managed by Codex; the companion does not read, copy, or store Codex authentication tokens.

Claude usage is controlled by the `enableClaudeUsage` setting, which defaults to `true` (a matching `enableCodexUsage` setting independently controls the Codex section, also defaulting to `true`). When Claude usage is enabled, the companion reads the OAuth access token that Claude Code itself already stores at `~/.claude/.credentials.json` (or the path set by `CLAUDE_CREDENTIALS_PATH`) — a file this app never writes to, refreshes, or otherwise modifies — and sends it, only to `https://api.anthropic.com`, to read the same account-level usage figures Claude Code itself displays. The token is held in memory for the duration of that request, never logged, never written to disk by this app, and never sent anywhere except that one Anthropic endpoint. If the token has expired, the companion shows an error asking the user to run `claude` again rather than attempting to refresh it itself. Users who do not use Claude Code can turn `enableClaudeUsage` off in Settings, which stops the companion from reading that file or contacting Anthropic entirely.

Local files:

- Settings are stored as `settings.json` in the Codex plugin data directory when available, otherwise under `${XDG_CONFIG_HOME:-$HOME/.config}/claude-codex-usage-companion`.
- If login autostart is enabled, the app creates `${XDG_CONFIG_HOME:-$HOME/.config}/autostart/claude-codex-usage-companion.desktop`; disabling the option removes that file.
- Bounded diagnostic logs are stored under `${XDG_STATE_HOME:-$HOME/.local/state}/claude-codex-usage-companion`.
- Optional usage update logging is disabled by default. When enabled, the selected TXT, CSV, or JSONL file stores refresh timestamps, which provider (Claude or Codex) the entry is for, success or error status, remaining percentages, reset times, available reset credits, and refresh errors. The user can choose its local path.
- Single-instance messages use an owner-only Unix socket under `XDG_RUNTIME_DIR`.
- No settings or logs are uploaded by the plugin.

The desktop menu entry, optional login autostart, taskbar and optional system-tray presence, and minimize or hide-to-tray behavior do not add background data collection. The source code and release workflows are public so these behaviors can be inspected.
