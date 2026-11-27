# Development

[繁體中文（zh-tw）](DEVELOPMENT.zh-TW.md) · [简体中文（zh-cn）](DEVELOPMENT.zh-CN.md)

This page explains how to build [Claude Codex Usage Companion](../README.md) and how it behaves on Linux.

## Build

Install the .NET 10 SDK or newer. Then run:

```bash
bash scripts/build-linux.sh               # host architecture
bash scripts/build-linux.sh linux-arm64   # ARM64
```

The build runs the tests. It writes the `.deb`, the `.tar.gz`, the `.plasmoid`, the Codex marketplace `.zip`, and `SHA256SUMS` to `artifacts/`.

To build the `.snap`, install snapcraft with `sudo snap install snapcraft --classic`. Then run `bash scripts/build-snap.sh`.

- `SKIP_LINUX_BUILD=true` uses an existing build.
- `SNAPCRAFT_ARGS=--destructive-mode` builds on an Ubuntu 24.04 host without LXD.

To rebuild, reinstall the `.deb`, and restart the installed app, run `./scripts/rebuild-reinstall.sh` as your normal user. Only the package installation uses sudo. The script disables NuGet vulnerability auditing. To enable it, set `NUGET_AUDIT=true`.

Other scripts:

- `bash scripts/install-plasma-widget.sh`: install the widget from the source tree.
- `bash scripts/capture-screenshots.sh`: generate the screenshots in `assets/screenshots/`.

For a development run:

```bash
dotnet run --project src/CodexUsageCompanion -- status
dotnet run --project src/CodexUsageCompanion -- gui
```

## Linux behavior

- The GUI uses a Unix socket under `XDG_RUNTIME_DIR` that only the owner can access. The socket keeps one app instance and receives refresh signals.
- Settings follow the XDG config convention. App logs have a size limit and are in `${XDG_STATE_HOME:-$HOME/.local/state}/claude-codex-usage-companion`.
- On Wayland with XWayland, the app uses the X11 compatibility backend, so the configured position applies. With native Wayland only, the compositor controls placement.
- The Codex `account/rateLimits/read` API and the Claude usage endpoint can change in future releases. Such changes can require an app update.

## Documentation style

Use [ASD-STE100 Simplified Technical English](https://www.asd-ste100.org/) as the writing guide for the English documentation (`README.md`, `docs/USER-GUIDE.md`, and `docs/DEVELOPMENT.md`):

- Use short sentences and the active voice.
- Give one instruction in each sentence.
- Use a maximum of 20 words in procedural sentences and 25 words in descriptive sentences.
- Use the same term for the same item.
- Keep commands, paths, setting keys, and UI labels unchanged.

Use the English files as the source for the Traditional Chinese (`.zh-TW.md`) and Simplified Chinese (`.zh-CN.md`) translations. Keep their structure, instructions, examples, and technical details aligned with the English version.
