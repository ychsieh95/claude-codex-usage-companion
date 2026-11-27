# Claude Codex Usage Companion for Linux

[![CI](https://github.com/ychsieh95/claude-codex-usage-companion/actions/workflows/ci.yml/badge.svg)](https://github.com/ychsieh95/claude-codex-usage-companion/actions/workflows/ci.yml) [![Release](https://img.shields.io/github/v/release/ychsieh95/claude-codex-usage-companion)](https://github.com/ychsieh95/claude-codex-usage-companion/releases/latest) [![License: MIT](https://img.shields.io/github/license/ychsieh95/claude-codex-usage-companion)](LICENSE) ![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet) ![Platform: Linux](https://img.shields.io/badge/platform-Linux-FCC624?logo=linux&logoColor=black) [![Snap: classic](https://img.shields.io/badge/snap-classic-82BEA0?logo=snapcraft&logoColor=white)](#snap-package)

[繁體中文（zh-tw）](README.zh-TW.md) · [简体中文（zh-cn）](README.zh-CN.md)

## Contents

- [Introduction](#introduction)
- [Installation](#installation)
  - [Requirements](#requirements)
  - [Download and install](#download-and-install)
  - [Start the app](#start-the-app)
  - [Uninstall](#uninstall)
- [More information](#more-information)

## Introduction

Claude Codex Usage Companion shows your Claude Code and Codex usage limits on the Linux desktop. It is local and open source, and it has no telemetry.

This Linux version is based on [gkfriend/codex-usage-companion](https://github.com/gkfriend/codex-usage-companion), a Windows/WPF application.

| Latest (bar) | Latest (ring) | Legacy |
| --- | --- | --- |
| ![Latest design with bars](assets/screenshots/overlay-en.png) | ![Latest design with rings](assets/screenshots/overlay-rings-en.png) | ![Legacy design](assets/screenshots/overlay-previous-en.png) |

### Functions

| Function | What it does |
| --- | --- |
| [Desktop panel](docs/USER-GUIDE.md#desktop-panel) | Shows each usage limit with its remaining percentage, reset time, and pace. It stays above other windows. |
| [Usage providers](docs/USER-GUIDE.md#usage-providers) | Reads Claude Code and Codex usage. You can enable or disable each provider. |
| [System tray](docs/USER-GUIDE.md#system-tray) | Keeps the app running when the panel is closed. The tray icon can show a remaining percentage. |
| [Notifications](docs/USER-GUIDE.md#notifications) | Sends a desktop notification when usage is low or a limit resets. |
| [Usage logging](docs/USER-GUIDE.md#usage-logging) | Writes each usage update to a TXT, CSV, or JSONL file. |
| [Updates](docs/USER-GUIDE.md#updates) | Checks GitHub for new releases. It can download, verify, and install them. |
| [Plasma 6 widget](docs/USER-GUIDE.md#plasma-6-widget) | Shows the limits in a KDE Plasma panel or on the desktop. |
| [CLI](docs/USER-GUIDE.md#cli) | Prints usage once, as JSON, or continuously in a terminal. |
| [Codex hooks](docs/USER-GUIDE.md#codex-hooks) | Starts or refreshes the panel when a Codex session starts or stops. |
| [Settings](docs/USER-GUIDE.md#settings) | Sets language, theme, design, position, alerts, logging, startup, and more. |

The app has English, Traditional Chinese, and Simplified Chinese text.

## Installation

### Requirements

- Kubuntu 26.04 or another modern Linux distribution. Release packages are for x64. For ARM64, [build from source](docs/DEVELOPMENT.md#build).
- A Wayland desktop or an X11 session.
- For Codex usage: Codex CLI, signed in and available as `codex`.
- For Claude usage: Claude Code, signed in.
- These host libraries: `libx11-6`, `libice6`, `libsm6`, `libfontconfig1`, `libharfbuzz0b`, `fonts-noto-cjk`, and `libnotify-bin`.

The `.deb` installs the host libraries automatically. For the snap and the portable archive, run:

```bash
sudo apt install libx11-6 libice6 libsm6 libfontconfig1 libharfbuzz0b fonts-noto-cjk libnotify-bin
```

Release packages include the .NET runtime.

#### Set up Codex CLI

Install or update Codex CLI with the [official installer](https://developers.openai.com/codex/cli). Then open a new terminal and sign in:

```bash
curl -fsSL https://chatgpt.com/codex/install.sh | sh
codex login
```

Verify the installation:

```bash
command -v codex
codex --version
codex login status
codex app-server --help >/dev/null && echo "Codex app-server is available"
```

The last command must print `Codex app-server is available`.

The app also searches `~/.local/bin`, `~/.npm-global/bin`, and `~/.codex/bin`. If Codex is in another location, set `CODEX_CLI_PATH`.

#### Set up Claude Code (optional)

Install [Claude Code](https://code.claude.com) and sign in. Run `claude` once to create `~/.claude/.credentials.json`.

The app searches `~/.local/bin`, `~/.npm-global/bin`, and `~/.claude/local` for Claude CLI. If you do not use Claude Code, clear **Enable Claude usage** in Settings.

### Download and install

Download the files from the [latest release](https://github.com/ychsieh95/claude-codex-usage-companion/releases/latest). Choose one package for the app. The Plasma widget package and the Codex plugin are optional.

| Package | Root access | Menu entry | Plasma widget | Use it when |
| --- | --- | --- | --- | --- |
| [Debian package](#debian-package) (`.deb`) | Yes | Yes | Included | You use Debian, Ubuntu, or Kubuntu. |
| [Snap package](#snap-package) (`.snap`) | Yes | Yes | Separate | Your distribution has snapd. |
| [Portable archive](#portable-archive) (`.tar.gz`) | No | No | Separate | You do not want a system package. |
| [Plasma widget package](#plasma-widget-package) (`.plasmoid`) | No | — | Widget only | You use the snap or the portable archive. |
| [Codex plugin](#codex-plugin) (`.zip`) | No | No | No | You want Codex to start and refresh the app. |

To verify the downloads, put `SHA256SUMS` in the same directory. Then run this command. Each file must show `OK`.

```bash
sha256sum --check --ignore-missing SHA256SUMS
```

#### Debian package

```bash
sudo apt install ./claude-codex-usage-companion_0.2.3_amd64.deb
```

The package installs:

- The `claude-codex-usage-companion` command and the `claude-codex-usage` alias.
- An application menu entry and icon.
- The Plasma 6 widget.

#### Snap package

```bash
sudo snap install --classic --dangerous ./claude-codex-usage-companion_0.2.3_amd64.snap
```

The `--dangerous` flag is necessary because the file does not come from the Snap Store. The snap uses classic confinement. The app must read and renew Claude credentials, read `~/.codex`, start `codex`, write the autostart entry, and call `notify-send`.

To add the `claude-codex-usage` alias:

```bash
sudo snap alias claude-codex-usage-companion claude-codex-usage
```

To install the included Plasma widget:

```bash
kpackagetool6 --type Plasma/Applet --install /snap/claude-codex-usage-companion/current/usr/share/plasma/claude-codex-usage-companion-0.2.3.plasmoid
```

#### Portable archive

```bash
tar -xzf claude-codex-usage-companion-0.2.3-linux-x64.tar.gz
cd claude-codex-usage-companion-0.2.3-linux-x64
install -D -m 755 claude-codex-usage-companion ~/.local/bin/claude-codex-usage-companion
```

Make sure that `PATH` includes `~/.local/bin`. The archive also contains the Plasma widget package. It does not add a menu entry or the alias.

The autostart entry uses the path of the app file. If you move the file, disable and enable automatic startup again.

#### Plasma widget package

Use this package only with the snap or the portable archive. Install it without root access:

```bash
kpackagetool6 --type Plasma/Applet --install ./claude-codex-usage-companion-0.2.3.plasmoid
```

To update the widget, use `--upgrade` instead of `--install`.

#### Codex plugin

Extract `CodexUsageCompanionMarketplace-v0.2.3-linux-x64.zip`. Then add the marketplace:

```bash
codex plugin marketplace add /path/to/extracted-marketplace
```

Install and enable the plugin from the marketplace. When Codex asks, review and trust the `SessionStart` and `Stop` hooks. The plugin contains its own copy of the app. It does not add a menu entry, a command on `PATH`, or the Plasma widget.

### Start the app

Launch **Claude Codex Usage Companion** from the application menu, or run:

```bash
claude-codex-usage-companion gui
```

### Uninstall

Disable automatic startup in Settings first. Then run the command for your package:

```bash
sudo apt remove claude-codex-usage-companion      # Debian package
sudo snap remove claude-codex-usage-companion     # Snap package
rm ~/.local/bin/claude-codex-usage-companion      # Portable archive
kpackagetool6 --type Plasma/Applet --remove com.github.ychsieh95.claude-codex-usage-companion  # Plasma widget package
```

These commands keep your settings and logs. To remove them, delete `${XDG_CONFIG_HOME:-$HOME/.config}/claude-codex-usage-companion` and `${XDG_STATE_HOME:-$HOME/.local/state}/claude-codex-usage-companion`.

## More information

- [User guide](docs/USER-GUIDE.md)
- [Development](docs/DEVELOPMENT.md)
- [Changelog](CHANGELOG.md)
- [Privacy](docs/PRIVACY.md)
- [Security](docs/SECURITY.md)
- [Contributing](docs/CONTRIBUTING.md)
- [MIT License](LICENSE)
