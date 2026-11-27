# Security Policy

## Supported version

Security fixes are provided for the latest published version.

## Reporting a vulnerability

Use the repository's private GitHub Security Advisory reporting flow. Do not include authentication data, private Codex conversations, or diagnostic logs in a public issue.

## Trust boundary

The plugin starts or refreshes a local executable through Codex `SessionStart` and `Stop` hooks. Codex asks users to review and trust bundled hooks before enabling them. On headless Linux sessions, automatic GUI startup is skipped.

The executable communicates with the local Codex app-server over redirected standard input/output and relies on Codex-managed authentication. It does not read or persist Codex authentication tokens.

When the `enableClaudeUsage` setting is on (the default), the executable makes one outbound HTTPS request per refresh to `https://api.anthropic.com`, authenticated with the OAuth access token Claude Code already stores at `~/.claude/.credentials.json`. Because that token expires after a few hours and the Claude CLI renews it only while running, an expired token is renewed here with the same OAuth refresh grant the CLI uses, against `https://platform.claude.com/v1/oauth/token`. The renewed pair is written back to the credentials file atomically, owner-readable only, preserving every other field, and under the same lock file the CLI uses; tokens are never logged. A refresh token that has itself expired or been revoked surfaces as an actionable error instead. These calls stop entirely if `enableClaudeUsage` is turned off.

## Updates

Update checks request `https://api.github.com/repos/ychsieh95/claude-codex-usage-companion/releases/latest`. When **Automatically download and install updates** is on, or the user selects **Install update**, the app downloads `SHA256SUMS`, `SHA256SUMS.sig`, and one package from that repository's release download URLs. The app builds these URLs from the repository name. It never follows a download location from the API response.

The app installs a package only when both checks pass:

1. `SHA256SUMS.sig` is a valid ECDSA P-256 / SHA-256 signature of `SHA256SUMS`, made with the release private key. The matching public key, [`packaging/update-signing-key.pub.pem`](../packaging/update-signing-key.pub.pem), is built into the app.
2. The package's SHA-256 hash matches its entry in the signed `SHA256SUMS`. The entry name includes the version, so an older signed release cannot replace a newer one.

The release workflow reads the release private key from the `UPDATE_SIGNING_KEY` GitHub Actions secret. The maintainer's local copy is `.secrets/update-signing-key.pem`, which is gitignored and never committed. `scripts/sign-release.sh` signs each release and fails if the signature does not match the built-in public key. A portable install replaces its own file with an atomic rename. A `.deb` install runs `pkexec /usr/bin/apt-get install -y <package>`, so the desktop's polkit agent asks for the administrator password every time. The app cannot update snap or Codex plugin installs.

The resident GUI accepts only `activate` and `refresh` messages through an owner-only Unix socket under `XDG_RUNTIME_DIR`. The optional system-tray icon exposes only local show, settings, and quit actions; it does not add a network or IPC endpoint. The optional login autostart entry invokes only the same local executable with `--background`. The app writes local settings and bounded diagnostic logs to the locations documented in [PRIVACY.md](PRIVACY.md). Optional usage logging writes only rate-limit refresh results to the user-selected local file and never writes authentication tokens or Claude/Codex conversation content.
