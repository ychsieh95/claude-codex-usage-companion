# Contributing

Issues and pull requests are welcome.

Requirements:

- Linux (Kubuntu 26.04 or a comparable distribution)
- .NET 10 SDK or newer
- Codex CLI for manual integration testing
- X11 or XWayland for GUI integration testing
- A desktop environment with a system tray/status notifier for tray integration testing

Before submitting a change:

```bash
dotnet test CodexUsageCompanion.slnx --configuration Debug -p:TreatWarningsAsErrors=true
dotnet test CodexUsageCompanion.slnx --configuration Release -p:TreatWarningsAsErrors=true
dotnet list src/CodexUsageCompanion/CodexUsageCompanion.csproj package --vulnerable --include-transitive
dotnet list tests/CodexUsageCompanion.Tests/CodexUsageCompanion.Tests.csproj package --vulnerable --include-transitive
desktop-file-validate packaging/linux/claude-codex-usage-companion.desktop
bash scripts/build-linux.sh
```

Keep changes focused, add regression tests for behavior changes, and do not commit generated output, logs, user settings, credentials, or local paths. For GUI lifecycle changes, manually verify minimize, settings save, tray enable/disable, hide-to-tray, tray restore, tray tooltip, tray quit, autostart enable/disable, language switching, nine-position preview/save/cancel behavior, usage logging enable/disable plus TXT/CSV/JSONL output and custom paths, and always-on-top behavior. Keep `README.md`, `README.zh-TW.md`, and `README.zh-CN.md` structurally equivalent whenever documentation changes.

## KDE shutdown regression check

On Plasma X11 or XWayland, launch the GUI with `SESSION_MANAGER` set and verify
that the resident process does not connect to KSMServer's ICE socket:

```bash
dotnet run --project src/CodexUsageCompanion -- gui
KSM_PID=$(pidof ksmserver)
ss -xapn | grep -E "ICE-unix/$KSM_PID|claude-codex"
```

The second command must not show a Usage Companion ICE/XSMP connection. Also
verify that opening and cancelling KDE's shutdown prompt returns
`org.kde.KSMServerInterface.isShuttingDown` to `false`, and that a real logout,
restart, or shutdown does not leave `plasma-shutdown` running.
