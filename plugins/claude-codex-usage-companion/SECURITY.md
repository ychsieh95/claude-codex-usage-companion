# Security Policy

## Supported version

Security fixes are provided for the latest published version.

## Reporting a vulnerability

Use the repository's private GitHub Security Advisory reporting flow. Do not include authentication data, private Codex conversations, or diagnostic logs in a public issue.

## Trust boundary

The plugin starts or refreshes a local executable through Codex `SessionStart` and `Stop` hooks. Codex asks users to review and trust bundled hooks before enabling them. On headless Linux sessions, automatic GUI startup is skipped.

The executable communicates with the local Codex app-server over redirected standard input/output and relies on Codex-managed authentication. It does not read or persist Codex authentication tokens.

When the `enableClaudeUsage` setting is on (the default), the executable makes one outbound HTTPS request per refresh to `https://api.anthropic.com`, authenticated with the OAuth access token Claude Code already stores at `~/.claude/.credentials.json`. This app never writes to that file and never implements its own token refresh; an expired token surfaces as an actionable error instead. This is the only outbound network call this app makes, and it stops entirely if `enableClaudeUsage` is turned off.

The resident GUI accepts only `activate` and `refresh` messages through an owner-only Unix socket under `XDG_RUNTIME_DIR`. The optional system-tray icon exposes only local show, settings, and quit actions; it does not add a network or IPC endpoint. The optional login autostart entry invokes only the same local executable with `--background`. The app writes local settings and bounded diagnostic logs to the locations documented in [PRIVACY.md](PRIVACY.md). Optional usage logging writes only rate-limit refresh results to the user-selected local file and never writes authentication tokens or Claude/Codex conversation content.
