# Claude Codex Usage Companion Linux 版

[![CI](https://github.com/ychsieh95/claude-codex-usage-companion/actions/workflows/ci.yml/badge.svg)](https://github.com/ychsieh95/claude-codex-usage-companion/actions/workflows/ci.yml) [![Release](https://img.shields.io/github/v/release/ychsieh95/claude-codex-usage-companion)](https://github.com/ychsieh95/claude-codex-usage-companion/releases/latest) [![License: MIT](https://img.shields.io/github/license/ychsieh95/claude-codex-usage-companion)](LICENSE) ![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet) ![Platform: Linux](https://img.shields.io/badge/platform-Linux-FCC624?logo=linux&logoColor=black) [![Snap: classic](https://img.shields.io/badge/snap-classic-82BEA0?logo=snapcraft&logoColor=white)](#snap-软件包)

[English](README.md) · [繁體中文（zh-tw）](README.zh-TW.md)

Claude Codex Usage Companion 是一款在 Linux 上运行的本地开源 Claude Code 与 Codex 用量监视器，包含：

- 使用 Avalonia 构建的小型桌面面板，可保持在其他窗口上方。
- 支持脚本、单次结果、JSON 输出和持续更新的 CLI。
- Codex `SessionStart` 和 `Stop` hooks，可启动或刷新单个常驻 GUI 进程。
- Claude Code 和 Codex 用量显示在同一面板。两者默认均启用，可在设置中分别禁用。
- 英文、繁体中文和简体中文用量文字。
- 语言、主题、窗口位置、警报、通知、日志、图标、自动启动和窗口行为设置。
- 四种系统托盘图标样式：原始图标、Claude 当前会话剩余用量、Claude 每周会话剩余用量和 Codex 会话剩余用量。
- 不含遥测。程序会在原始文件中更新现有 Claude 凭据，且不会将 token 写入日志。

此 Linux 移植版基于 [gkfriend/codex-usage-companion](https://github.com/gkfriend/codex-usage-companion)。原始实现以 Windows/WPF 为目标。

## 面板设计

在**设置 → 外观**中，选择面板的**设计**：

- **最新**（默认）与 Plasma 小部件的弹出窗口一致。每个提供方各有一张带彩色边缘的卡片。每项限制显示名称、重置倒计时与本机重置时间、以及细进度条。右侧的大号百分比使用等宽字体，与名称、重置时间及进度条合计等高。点数信息上方以分隔线与限制隔开。标题栏显示最后更新时间，以及显示剩余最少限制的徽章。将指针移到徽章上可查看是哪一项限制。
- **旧版**还原 v0.1.5 面板，使用带标签的 36 像素进度条。

选择**确定**或**应用**以更改设计。

在最新设计中，**限制行**可将每项限制的名称与重置时间显示在同一行，使面板更矮。默认会将重置时间显示在名称下方的第二行。

两种设计都显示平均速度指针。在最新设计中，指针是进度条上的圆点。将鼠标指针移到圆点上可查看使用速度是否低于平均速度。两个间距设置与卡片高度调整控点仅适用于旧版设计。

## v0.1.3

面板采用以 [QuotaGem](https://github.com/gyozalab/QuotaGem) 为参考的提供方标题和圆角进度条。每行显示 5h／每周标签、剩余百分比和本地重置时间，各列保持对齐。隐藏提供方时，面板会根据内容调整大小，并保留设置的屏幕位置。将鼠标指针移至每周进度条可查看点数信息。

每个进度条都有平均速度指针：上下边缘的小刻度标出平均使用整个重置周期时，进度条应该停在的位置。若进度条在指针左侧结束，表示该用量限制的使用速度比平均快。将鼠标指针移至指针可查看此周期已过的比例。面板与展开的 Plasma 小部件在最新设计中改以进度条上的圆点显示此指针。若要隐藏，请在设置的**用量**页面取消勾选**在进度条上显示平均速度指针**，或在小部件设置的 **Popup** 旁取消勾选 **Show even-pace marker**。

设置窗口左侧有侧边栏，包含以下页面：**用量 → 外观 → 窗口与启动 → 通知 → 日期与时间 → 日志 → 更新**。在侧边栏点击页面即可显示其设置。每个页面左侧为标签栏，右侧为控件。**窗口位置**位于**窗口与启动**页面。版本与项目链接位于**更新**页面。

关闭设置时，程序会保存当前选中的页面，点击**取消**也会保留。重新打开设置时（包括重启程序后），会回到此页面。**取消**会丢弃尚未应用的表单更改。

在**外观**中设置以下项目：

- 背景颜色：`#RRGGBB` 或 `#AARRGGBB`。留空使用主题颜色。
- 设计：**最新**或**旧版**，默认 **最新**。
- 限制行：将每项限制的名称与重置时间显示在同一行，默认关闭。仅适用于**最新**设计。
- 标题徽章：**最新**设计标题栏徽章显示的值。可选**剩余最少的限制**（默认）、**显示中限制的平均值**、**Claude（剩余最少）**、**Codex（剩余最少）**或**隐藏**。
- 标签与百分比间距：0–300 像素，默认 **128**。
- 百分比与重置时间间距：0–64 像素，默认 **16**。

外观更改会立即显示。点击**确定**或**应用**保存。点击**取消**恢复已保存的外观。现有间距设置会保留。

在**设置 → 更新**中，点击**检查更新**可检查是否有更新的 GitHub 稳定版本。如有新版本，点击**打开版本页面**可打开下载页面。

**自动检查更新（启动时及每天）**默认启用。程序会在启动时及运行期间每 24 小时检查一次。要禁用自动检查，请取消勾选，再点击**确定**或**应用**。仍可手动检查。

自动检查发现新版本时会显示桌面通知。手动检查的网络错误会显示在设置中。自动检查的网络错误会写入日志。

### 自动安装

检查发现新版本时，**检查更新**旁会出现**安装更新**。如果不想每次手动选择，请勾选**自动下载并安装更新**，再点击**确定**或**应用**。此选项默认关闭，并且需要启用**自动检查更新**。

实际行为取决于安装方式。设置会在选项下方显示当前安装方式的说明。

| 安装方式 | 行为 |
| --- | --- |
| 位于可写入文件夹（例如 `~/.local/bin`）的便携归档 | 程序下载更新、替换自身文件并重新启动，不会要求密码。 |
| Debian 软件包 | 程序在后台下载更新，再通过通知询问是否安装。点击**立即安装**并输入管理员密码后，程序使用 `pkexec apt-get install` 安装软件包，然后重新启动。 |
| Snap | 程序无法替换 snapd 管理的文件。请打开版本页面手动更新。 |
| Codex 插件 | 插件的更新由 Codex 管理。 |

更新包必须通过两项检查才会安装：

1. 版本中的 `SHA256SUMS` 文件带有项目发布密钥的有效签名。程序内置公钥，因此被修改的版本或其他密钥都无法通过检查。
2. 下载包的 SHA-256 哈希值与已签名 `SHA256SUMS` 中的条目一致。

v0.2.1 之前的版本没有签名，因此程序不会自动安装。下载文件存放在 `${XDG_CACHE_HOME:-$HOME/.cache}/claude-codex-usage-companion/updates`，并且只保留最新的下载。更新失败时，通知会附上版本页面链接，错误会写入日志。

要手动检查下载的版本，请先将[公钥](packaging/update-signing-key.pub.pem)保存到下载文件夹，再在该文件夹中运行：

```bash
openssl dgst -sha256 -verify update-signing-key.pub.pem -signature SHA256SUMS.sig SHA256SUMS
sha256sum --check --ignore-missing SHA256SUMS
```

隐藏提供方进度条后，用量查询仍会继续。取消勾选**启用 Claude 用量显示**或**启用 Codex 用量显示**，才会停止对应的用量查询。这也会清除该提供方的错误。

已启用提供方的错误会持续显示，直到该提供方恢复正常。隐藏进度条或其他提供方更新成功都不会清除这些错误。完整版本说明请参阅[变更日志](CHANGELOG.md)。

用量卡片使用 36 像素进度条。每周卡片在进度条下方保留额外空间，显示点数和自动充值信息。卡片会随较大字体增加高度。调整大小的标记会在鼠标悬停或获得键盘焦点时显示。

程序会将未更改的 78／96 像素默认卡片高度改为较小高度。手动调整过的高度会保留。

程序从首次显示起就会应用已保存的间距，包括加载用量时。百分比列会根据文字和间距调整宽度。更改任一间距都会调整面板宽度。手动加宽面板时，多余空间会放在百分比列前方。重置时间列仍靠近右侧。预设位置使用桌面工作区域，避免覆盖保留空间的任务栏和面板。

点击**恢复默认值**可打开**是／否**确认窗口。点击**是**会恢复设置表单中的所有偏好，包括卡片高度和间距。点击**否**会保留表单不变。按 Escape 或关闭确认窗口也会保留表单不变。

点击**确定**或**应用**可保存恢复结果。点击**取消**可丢弃恢复结果。当前选中的页面会保留。更改卡片高度时，进度条轨道和填充区域的高度也会改变。

窗口打开前会根据所有可见卡片和状态行调整大小。底部仅保留正常内边距。首次获取用量结果后，面板会调整原先显示等待文字的列宽。设置的间距会保留，默认为 128 和 16 像素。

## 屏幕截图

| 设计 | 英文（`en-US`） | 繁体中文（`zh-tw`） | 简体中文（`zh-cn`） |
| --- | --- | --- | --- |
| 最新（默认） | ![英文用量面板（最新设计）](assets/screenshots/overlay-en.png) | ![繁体中文用量面板（最新设计）](assets/screenshots/overlay-zh-tw.png) | ![简体中文用量面板（最新设计）](assets/screenshots/overlay-zh-cn.png) |
| 旧版（v0.1.5） | ![英文用量面板（旧版设计）](assets/screenshots/overlay-previous-en.png) | ![繁体中文用量面板（旧版设计）](assets/screenshots/overlay-previous-zh-tw.png) | ![简体中文用量面板（旧版设计）](assets/screenshots/overlay-previous-zh-cn.png) |

用量面板图片使用示例数据。可在**设置 → 外观 → 设计**切换设计。

### 设置

| 英文（`en-US`） | 繁体中文（`zh-tw`） | 简体中文（`zh-cn`） |
| --- | --- | --- |
| ![英文设置](assets/screenshots/settings-en.png) | ![繁体中文设置](assets/screenshots/settings-zh-tw.png) | ![简体中文设置](assets/screenshots/settings-zh-cn.png) |

### 恢复确认

**是／否**按钮文字在按钮内水平和垂直居中。运行 `bash scripts/capture-screenshots.sh` 可重新生成面板、设置和确认窗口截图（需要 .NET 10 SDK）。

| 英文（`en-US`） | 繁体中文（`zh-tw`） | 简体中文（`zh-cn`） |
| --- | --- | --- |
| ![英文恢复确认](assets/screenshots/reset-confirmation-en.png) | ![繁体中文恢复确认](assets/screenshots/reset-confirmation-zh-tw.png) | ![简体中文恢复确认](assets/screenshots/reset-confirmation-zh-cn.png) |

### Plasma 6 小组件

| 设计 | 面板 | 展开弹出窗口 |
| --- | --- | --- |
| 最新（默认） | ![Plasma 6 小组件面板圆环（最新设计）](assets/screenshots/plasma-widget-circles.png) | ![Plasma 6 小组件展开弹出窗口（最新设计）](assets/screenshots/plasma-widget-expanded.png) |
| 先前 | ![Plasma 6 小组件面板圆环（先前设计）](assets/screenshots/plasma-widget-previous-circles.png) | ![Plasma 6 小组件展开弹出窗口（先前设计）](assets/screenshots/plasma-widget-previous-expanded.png) |

![Plasma 6 小组件设置](assets/screenshots/plasma-widget-settings.png)

## 系统要求

- Kubuntu 26.04 或其他现代 Linux 发行版。Release 软件包适用于 x64。ARM64 请从源码构建，请参阅[构建](#构建)。
- Wayland 桌面环境或 X11 会话。
- 已安装并完成身份验证的 Codex CLI，且可通过 `PATH` 中的 `codex` 运行。
- 以下主机库：`libx11-6`、`libice6`、`libsm6`、`libfontconfig1`、`libharfbuzz0b`、`fonts-noto-cjk` 和 `libnotify-bin`。

`.deb` 会自动安装这些主机库。使用 snap 或便携式压缩包时，请使用以下命令安装：

```bash
sudo apt install libx11-6 libice6 libsm6 libfontconfig1 libharfbuzz0b fonts-noto-cjk libnotify-bin
```

发布的软件包已包含 .NET runtime，无需另行安装。

## 安装并验证 Codex CLI

Claude Codex Usage Companion 需要官方 Codex CLI 及其 `app-server` 子命令。请使用[官方安装程序](https://developers.openai.com/codex/cli)在 Linux 上安装或更新到最新版 Codex CLI：

```bash
curl -fsSL https://chatgpt.com/codex/install.sh | sh
```

安装完成后打开新的终端，然后使用 ChatGPT 或其他可用的身份验证方式登录：

```bash
codex login
```

验证可执行文件路径、版本、登录状态以及必需的 `app-server` 子命令：

```bash
command -v codex
codex --version
codex login status
codex app-server --help >/dev/null && echo "Codex app-server is available"
```

各命令应依次输出以下结果：

1. `codex` 的绝对路径。
2. Codex 版本。
3. 当前的身份验证方式。
4. 消息 `Codex app-server is available`。

如果 `command -v codex` 没有输出，请打开新的终端。确认 `PATH` 包含安装程序显示的安装目录。

此程序还会搜索 `~/.local/bin`、`~/.npm-global/bin` 和 `~/.codex/bin`。如果 Codex 安装在其他位置，请设置 `CODEX_CLI_PATH=/absolute/path/to/codex`。

## 启用 Claude 用量（可选）

面板默认显示 Claude Code 和 Codex 用量。要使用 Claude 用量，请安装 [Claude Code](https://code.claude.com)，并登录 Claude Code。运行一次 `claude` 以创建 `~/.claude/.credentials.json`。

如果不使用 Claude Code，请在设置中取消勾选**启用 Claude 用量显示**。也可在 `settings.json` 中设置 `"enableClaudeUsage": false`。**启用 Codex 用量显示**复选框以同样方式控制 Codex。

Claude Code 没有官方用量 API。程序会读取 Claude Code 保存在本地的 OAuth access token，并用它调用 Anthropic 未公开的用量端点。

Access token 的有效期只有数小时。Claude CLI 只会在运行时更新它。电脑重启后，保存的 token 通常已过期。

程序使用 OAuth refresh 授权更新 token，并使用 `~/.claude/.credentials.json` 中的 refresh token。更新后的两个 token 会以单次原子操作写回该文件，其余字段保持不变。程序使用与 Claude CLI 相同的锁文件，避免同时更改 token。程序不会将 token 写入日志。

如果 refresh token 已过期或被撤销，程序会显示错误。请运行 `claude` 重新登录。

未公开的端点可能随时更改。无论更新间隔设置为何，程序最多每 180 秒查询一次 Claude 用量。

程序会在 `~/.local/bin`、`~/.npm-global/bin` 和 `~/.claude/local` 搜索 Claude CLI。如果凭据文件或 CLI 位于其他位置，请设置对应变量：

- `CLAUDE_CREDENTIALS_PATH=/absolute/path/to/.credentials.json`
- `CLAUDE_CLI_PATH=/absolute/path/to/claude`

## 安装

从[最新版本](https://github.com/ychsieh95/claude-codex-usage-companion/releases/latest)下载文件。为程序选择一种软件包。Plasma 小组件包和 Codex plugin 为可选项。

| 软件包 | 需要 root | 菜单项 | Plasma 小组件 | 适用情况 |
| --- | --- | --- | --- | --- |
| [Debian 软件包](#debian-软件包)（`.deb`） | 是 | 是 | 已包含 | 使用 Debian、Ubuntu 或 Kubuntu。 |
| [Snap 软件包](#snap-软件包)（`.snap`） | 是 | 是 | 另行安装 | 发行版提供 snapd。 |
| [便携式压缩包](#便携式压缩包)（`.tar.gz`） | 否 | 否 | 另行安装 | 不想安装系统软件包。 |
| [Plasma 小组件包](#plasma-小组件包)（`.plasmoid`） | 否 | — | 仅小组件 | 使用 snap 或便携式压缩包。 |
| [Codex plugin](#codex-plugin)（`.zip`） | 否 | 否 | 否 | 希望由 Codex 启动并刷新程序。 |

若要验证下载的文件，请将 `SHA256SUMS` 下载到同一目录，然后运行：

```bash
sha256sum --check --ignore-missing SHA256SUMS
```

每个下载的文件都必须显示 `OK`。

### Debian 软件包

使用以下命令安装软件包：

```bash
sudo apt install ./claude-codex-usage-companion_0.2.0_amd64.deb
```

软件包会安装以下项目：

- `claude-codex-usage-companion` 命令和较短的 `claude-codex-usage` 别名。
- 位于 `/usr/share/applications/claude-codex-usage-companion.desktop` 的 KDE/GNOME 应用程序菜单项。
- 位于 `/usr/share/icons/hicolor/scalable/apps/` 的图标。
- Plasma 6 小组件。

### Snap 软件包

使用以下命令安装软件包：

```bash
sudo snap install --classic --dangerous ./claude-codex-usage-companion_0.2.0_amd64.snap
```

由于文件并非来自 Snap Store，因此需要 `--dangerous` 标志。

此 snap 使用 classic 隔离模式。程序必须读取并更新 `~/.claude` 中的 Claude 凭据，也必须读取 `~/.codex`、启动 `codex` CLI、写入自动启动项并调用 `notify-send`。strict 隔离模式会阻止这些操作。

此 snap 会在 `/snap/bin` 安装 `claude-codex-usage-companion` 命令，并添加应用程序菜单项。若要添加较短的 `claude-codex-usage` 别名，请运行：

```bash
sudo snap alias claude-codex-usage-companion claude-codex-usage
```

此 snap 包含 Plasma 小组件包，但不会自动安装。若要安装小组件，请运行：

```bash
kpackagetool6 --type Plasma/Applet --install /snap/claude-codex-usage-companion/current/usr/share/plasma/claude-codex-usage-companion-0.2.0.plasmoid
```

若启用登录时自动启动，自动启动项会使用 `/snap/bin/claude-codex-usage-companion`。此路径在 snap 更新后保持不变。

### 便携式压缩包

解压压缩包，并将程序复制到 `PATH` 中的目录：

```bash
tar -xzf claude-codex-usage-companion-0.2.0-linux-x64.tar.gz
cd claude-codex-usage-companion-0.2.0-linux-x64
install -D -m 755 claude-codex-usage-companion ~/.local/bin/claude-codex-usage-companion
```

请确认 `PATH` 包含 `~/.local/bin`。Plasma 小组件会从 `PATH` 调用程序。

压缩包还包含 Plasma 小组件包、README 和许可证。它不会添加菜单项或 `claude-codex-usage` 别名。

自动启动项会使用程序文件的路径。若移动文件，请禁用后再重新启用登录时自动启动。

### Plasma 小组件包

此软件包适用于 snap 或便携式压缩包。`.deb` 已包含小组件，请勿与 `.deb` 一起使用。可无需 root 权限安装：

```bash
kpackagetool6 --type Plasma/Applet --install ./claude-codex-usage-companion-0.2.0.plasmoid
```

若要更新已安装的小组件，请用 `--upgrade` 替换 `--install`。小组件需要可从 `PATH` 运行 `claude-codex-usage-companion` 命令。

### Codex plugin

Linux release 构建会生成 `CodexUsageCompanionMarketplace-v0.2.0-linux-x64.zip`。请解压此文件。使用以下命令添加解压后的 marketplace：

```bash
codex plugin marketplace add /path/to/extracted-marketplace
```

从已添加的 marketplace 安装并启用 plugin。Codex 询问时，请检查随附的 `SessionStart` 和 `Stop` hooks。信任 hooks 后才能启用。Plugin 启动程序时，程序会使用 `PLUGIN_DATA` 保存设置。

Plugin 包含自己的程序副本。它不会添加菜单项、`PATH` 中的命令或 Plasma 小组件。

### 卸载

请先在设置中禁用登录时自动启动，再按软件包运行对应命令：

```bash
sudo apt remove claude-codex-usage-companion      # Debian 软件包
sudo snap remove claude-codex-usage-companion     # Snap 软件包
rm ~/.local/bin/claude-codex-usage-companion      # 便携式压缩包
kpackagetool6 --type Plasma/Applet --remove com.github.ychsieh95.claude-codex-usage-companion  # Plasma 小组件包
```

这些命令会保留设置和日志。若要删除，请删除 `${XDG_CONFIG_HOME:-$HOME/.config}/claude-codex-usage-companion` 和 `${XDG_STATE_HOME:-$HOME/.local/state}/claude-codex-usage-companion`。

## 启动程序

从应用程序菜单启动 **Claude Codex Usage Companion**，或运行：

```bash
claude-codex-usage-companion gui
```

登录时自动启动默认禁用。在设置中启用后，程序会创建 `${XDG_CONFIG_HOME:-$HOME/.config}/autostart/claude-codex-usage-companion.desktop`。

启用登录时自动启动后，会显示**最小化窗口**选项。否则此选项会隐藏。两者均启用时，程序会在登录时启动并隐藏窗口。用量仍会在后台更新。

从应用程序菜单打开程序即可显示窗口。也可使用已启用的系统托盘图标，或再次运行程序。

点击 **⚙ 设置**可配置程序。设置项包括任务栏与系统托盘图标、托盘图标样式及登录时自动启动。也可设置语言、主题、位置、低用量通知、重置通知、日志和始终置顶行为。

- **确定**会保存更改并关闭设置。
- **取消**会丢弃尚未应用的更改。
- **应用**会保存更改并保持设置打开。

在**外观**中，使用 `#RRGGBB` 或 `#AARRGGBB` 设置背景颜色。留空使用主题颜色。百分比与重置时间间距可设为 0–64 像素。5h／每周标签与百分比间距可设为 0–300 像素。两者默认分别为 16 和 128 像素。这两个间距仅适用于**旧版**设计。

面板宽度会自动配合调整。外观更改会立即显示。点击**取消**可恢复上次保存的外观。

## Plasma 6 小组件

`.deb` 会安装 **Claude Codex Usage** Plasma 6 小组件。使用 snap 或便携式压缩包时，请安装 [Plasma 小组件包](#plasma-小组件包)。添加方式如下：

1. 右键点击 Plasma 桌面或面板。
2. 选择**添加小组件**。
3. 搜索 `Claude Codex Usage`。
4. 将小组件拖到面板或桌面。

小组件显示四种用量限制：Claude 当前会话和本周用量，以及 Codex 5 小时和每周用量。四者默认均启用。选中的限制会显示在面板和展开的弹出窗口中。

在水平面板中，指示器会并排显示；在垂直面板中则会上下堆叠。打开小组件可查看本地重置时间、点数、更新状态和各提供方的错误。

设置页面（英文界面）提供以下选项：

| 选项 | 值 | 效果 |
| --- | --- | --- |
| **Design** | Latest（默认）、Previous | Latest 为当前外观，Previous 恢复 v0.1.5 的外观。**Percentages**、**Panel**、**Warn below**、**Critical below**、**Popup badge** 和 **Show credits** 仅适用于最新设计。 |
| **Style** | Radial meters、Meter strips | 面板中显示圆环或条形。 |
| **Update every** | 1–60 分钟 | 小组件向程序查询用量的频率。 |
| **Percentages** | Remaining（默认）、Used | 显示各限制的剩余或已用比例。颜色始终按剩余量决定，因此红色表示该限制即将用完。 |
| **On click** | Open the popup（默认）、Open the companion app | 点击面板上的小组件时执行的操作。 |
| **Panel** | 两个复选框 | 只显示剩余最少（或已用最多）的限制而非所有选中的限制，以及显示或隐藏面板指示器下方的简短标签。 |
| **Warn below** 和 **Critical below** | 剩余 1–100%（默认 70% 和 40%） | 限制低于警告值时显示琥珀色，低于严重值时显示红色。 |
| **Reset time** | Countdown and time（默认）、Countdown、Time | 弹出窗口和提示中重置时间的显示方式。选择 **Time** 时，超过一天后才重置会加上星期。 |
| **Clock** | System default、12-hour、24-hour | 重置时间和最后更新时间的时钟格式。 |
| **Tooltip** | 三个复选框 | 在面板提示中显示剩余最少（或已用最多）的限制、重置时间和最后更新时间。 |
| **Limits** | 四个复选框 | 面板和弹出窗口中显示的限制。 |
| **Popup badge** | Most-used limit（默认）、Average of shown limits、Claude's most-used limit、Codex's most-used limit、Hidden | 弹出窗口标题徽章显示的值。用量最多的限制即剩余最少（或已用最多）的限制。平均值涵盖显示中的限制。将鼠标指针移到徽章上可查看显示内容。 |
| **Popup** | 两个复选框 | 显示各提供方下方的点数信息，以及进度条上的平均速度指针。 |

小组件会调用程序的本地 `status --json` 命令。凭据和网络访问由程序处理。

使用开发版源码时，可运行 `bash scripts/install-plasma-widget.sh` 安装或更新当前源码包。Companion CLI 必须保持安装并可从 `PATH` 运行。

## CLI

示例使用 `claude-codex-usage` 别名。若没有此别名，请改用 `claude-codex-usage-companion`。

输出当前限额：

```bash
claude-codex-usage status
```

返回机器可读的 JSON：

```bash
claude-codex-usage status --json
```

每 30 秒持续刷新终端：

```bash
claude-codex-usage watch --interval 30
```

输出配置文件路径和实际生效的设置：

```bash
claude-codex-usage config
```

`status`、`watch` 及其 `--json` 输出会先显示 Claude，再显示 Codex，与面板卡片顺序一致。JSON 结果使用此结构：`{"claude": {"state": ..., "error": ...}, "codex": {"state": ..., "error": ...}}`。禁用提供方时，对应键会省略。这会取代先前的扁平 JSON 结构。读取旧结构的脚本需要调整。

如果 Codex 不在 `PATH` 中，请设置 `CODEX_CLI_PATH=/absolute/path/to/codex`。如需禁用 ANSI 颜色，请设置 `NO_COLOR=1` 或传入 `--no-color`。

## 设置

默认配置文件位于：

```text
${XDG_CONFIG_HOME:-$HOME/.config}/claude-codex-usage-companion/settings.json
```

```json
{
  "showClaudeSession": true,
  "showClaudeWeekly": true,
  "showCodexFiveHour": false,
  "showCodexWeekly": true,
  "enableClaudeUsage": true,
  "enableCodexUsage": true,
  "enableSystemTray": false,
  "trayIconStyle": "original",
  "showTaskbarIcon": true,
  "enableAutoUpdateCheck": true,
  "startOnBoot": false,
  "minimizeOnStart": false,
  "language": "en-US",
  "theme": "system",
  "overlayDesign": "latest",
  "resetTimeOnNameRow": false,
  "headerBadge": "lowest",
  "settingsPage": "usage",
  "backgroundColor": "",
  "percentageTimeSpacing": 16,
  "labelPercentageSpacing": 128,
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

语言值为 `en-US`、`zh-tw` 和 `zh-cn`。首次运行时，程序根据系统语言选择：

- `zh-TW`、`zh-HS`、`zh-Hant`、`zh-HK`、`zh-MO` 和其他繁体中文区域设置使用 `zh-tw`。
- `zh-CN`、`zh-Hans`、`zh-SG` 和其他简体中文区域设置使用 `zh-cn`。
- 其他语言使用 `en-US`。

程序也接受旧版值 `auto` 和 `en`。语言值匹配不区分大小写。

主题值：`dark`、`light`、`system`。`system` 是默认值，会跟随桌面当前的浅色或深色偏好设置。

设计值：`latest` 和 `previous`。`latest` 是默认值。`resetTimeOnNameRow` 会在最新设计中将每项限制的名称与重置时间显示在同一行，默认为 `false`。

标题徽章值：`lowest`、`average`、`claude`、`codex`、`hidden`。`lowest` 是默认值。`claude` 和 `codex` 显示该提供方剩余最少的限制。`average` 显示显示中限制的平均值。

设置页面值：`usage`、`appearance`、`window`、`notifications`、`dateTime`、`logging`、`updates`。`settingsPage` 保存设置窗口打开时显示的页面。程序会忽略旧版的 `settingsSections` 值。

系统托盘图标样式值为 `original`、`claude-current-session`、`claude-weekly-session` 和 `codex-session`。数字样式会在深色背景上显示粗体、抗锯齿的七段显示器数字，并且不显示百分号。Codex 会话会优先使用 5 小时窗口，若未提供则改用每周用量。

位置值：`left-top`、`middle-top`、`right-top`、`left-center`、`middle-center`、`right-center`、`left-bottom`、`middle-bottom`、`right-bottom`、`custom`。

选择位置后会立即显示。点击**确定**或**应用**可保留新位置。点击**取消**可恢复到上次应用的位置。程序也接受 `top-left` 和 `bottom-right` 等旧版角落位置值。

拖动标题栏可自由移动窗口。程序会将位置改为 `custom` 并保存坐标。在设置中选择 `custom` 可恢复已保存的位置。如果尚未保存位置，窗口会保留当前位置。

主窗口提供以下快捷键：

- `F1`：显示快捷键列表。
- `S`：打开设置。
- `Ctrl+R`：刷新用量。
- `Esc`：关闭窗口。

标题栏的 **?** 按钮和系统托盘菜单的**键盘快捷键**会打开相同列表。按 `Esc` 或 `F1` 可关闭列表，也可点击**确定**。

在设置中，按 `Ctrl+S` 可保存更改。按 `Esc` 可关闭设置。如果仍有未保存的更改，程序会在丢弃更改前要求确认。

**更新间隔** 设置提供 1 至 60 分钟的常用值。当前的 1 分钟间隔是默认值和最小值。以前保存在此范围内的自定义间隔仍可选择。

低用量通知默认禁用。启用后，程序会比较 5 小时及每周剩余用量与设置的阈值。阈值范围为 `1`–`100`%。任一剩余用量低于阈值时，程序会发送一次桌面通知。剩余用量恢复到阈值或以上后，该通知可再次触发。

重置通知可独立设置，默认禁用。用量更新确认新的重置周期时，程序会发送通知。

重置日期时间预设格式为 `MMM D`（默认）、`MMM D HH:mm`、`yyyy-MM-dd` 和 `yyyy-MM-dd HH:mm`。最后更新预设格式为 `HH:mm`（默认）、`HH:mm:ss`、`MMM D HH:mm` 和 `yyyy-MM-dd HH:mm`。

两个字段均接受自定义 .NET 日期时间格式字符串，并支持用大写 `D` 表示日。输入时，预览会使用所选语言检查格式。无效格式会显示错误，并禁用**确定**和**应用**。月份名称和日期表示方式会使用所选语言。

以下设置有范围限制：

- 不透明度：`0.5`–`1.0`。
- 边距：`0`–`64` 像素。
- 更新间隔：`60`–`3600` 秒。

`enableClaudeUsage` 和 `enableCodexUsage` 分别控制各提供方，默认均为 `true`。禁用提供方会隐藏其区域，并停止用量查询。面板高度会自动调整。

无论更新间隔设置为何，Claude 用量最多每 180 秒查询一次。请参阅[启用 Claude 用量（可选）](#启用-claude-用量可选)。

**显示的用量限制**复选框可控制各张卡片：

- `showClaudeSession`：Claude 当前会话。默认显示。
- `showClaudeWeekly`：Claude 每周用量。默认显示。
- `showCodexFiveHour`：Codex 5 小时用量。默认隐藏，以保留原有布局。
- `showCodexWeekly`：Codex 每周用量。默认显示。

## 用量更新日志

用量日志默认禁用。启用后，每次成功刷新和刷新错误都会追加到指定文件。CSV 是默认格式，默认路径遵循 `${XDG_STATE_HOME:-$HOME/.local/state}/claude-codex-usage-companion/usage-history.csv`。

程序支持以下日志格式：

- `txt`：每行一条键值记录。
- `csv`：表头后跟数据行。
- `jsonl`：每行一个 JSON 对象。

更改格式时，设置的文件扩展名会自动改为 `.txt`、`.csv` 或 `.jsonl`。相对路径以程序的 XDG state 目录为基准。

每条记录包含以下字段：

- 更新时间。
- 提供方：`claude` 或 `codex`。
- 成功或错误状态。
- 5 小时及每周剩余百分比和重置时间。
- 可用重置点数。
- 任何更新错误。

新增的 `provider` 字段会改变日志结构。读取旧日志文件的解析程序可能需要调整。

## 构建

安装 .NET 10 SDK 或更高版本，然后运行：

```bash
bash scripts/build-linux.sh
```

ARM64：

```bash
bash scripts/build-linux.sh linux-arm64
```

重新构建、安装并启动已安装的程序：

```bash
./scripts/rebuild-reinstall.sh
```

脚本会停止当前运行的程序，并按主机架构构建。接着使用 `sudo apt reinstall -f` 重新安装生成的 `.deb`，再启动已安装的程序。任一步骤失败就会停止。

请以普通桌面用户运行脚本。只有安装软件包需要 sudo。

本地重新构建脚本会禁用 NuGet 漏洞审计，避免审计服务无法连接时出现 `NU1900` 警告。如果依赖包尚未缓存，下载时仍需要连接 NuGet。

要启用审计，请运行 `NUGET_AUDIT=true ./scripts/rebuild-reinstall.sh`。直接运行 `build-linux.sh` 和 CI 默认仍启用审计。

构建过程会运行测试套件，并在 `artifacts/` 下输出以下文件：

- `.deb` 软件包。
- 便携式 `.tar.gz` 压缩包。
- Plasma `.plasmoid` 软件包。
- Codex marketplace `.zip` 压缩包。
- 校验码。

若也要构建 `.snap` 软件包，请用 `sudo snap install snapcraft --classic` 安装 snapcraft，然后运行：

```bash
bash scripts/build-snap.sh
```

此脚本会先运行 `build-linux.sh`，再在 `artifacts/` 下写入 `claude-codex-usage-companion_<version>_<arch>.snap`，并将其加入 `SHA256SUMS`。设置 `SKIP_LINUX_BUILD=true` 可使用已有的构建。设置 `SNAPCRAFT_ARGS=--destructive-mode` 可在没有 LXD 的 Ubuntu 24.04 主机上构建。

开发环境运行方式：

```bash
dotnet run --project src/CodexUsageCompanion -- status
dotnet run --project src/CodexUsageCompanion -- gui
```

## Linux 行为

GUI 使用 `XDG_RUNTIME_DIR` 下仅限所有者访问的 Unix socket。此 socket 可确保只运行一个程序实例，并接收刷新信号。设置遵循 XDG config 约定。程序会将有大小限制的日志文件保存在 `${XDG_STATE_HOME:-$HOME/.local/state}/claude-codex-usage-companion`。

在提供 XWayland 的 Wayland 桌面中，程序使用 Avalonia 的 X11 兼容后端。设置的位置会控制初始窗口位置和后续位置更改。如果只有原生 Wayland，合成器会控制位置，并可能忽略此设置。X11 会话使用 X11 后端。

拖动标题栏可移动面板。拖动右下角可调整大小。卡片和进度条会随宽度调整。窗口较矮时可滚动。

标题栏控件依次为键盘快捷键、固定、设置、刷新、最小化和关闭。最小化和关闭控件之间有较宽的间距。固定按钮控制始终置顶设置。设置和系统托盘菜单中的**窗口始终置顶**复选框也控制同一设置。

Claude 的**当前会话**和**本周用量（全部）**卡片会显示在 Codex 之前。提供方标题显示 Claude 或 OpenAI 图标。每周进度条下方会显示 Codex 点数余额和自动充值信息。Claude 则显示额外用量支出和每月上限。提示中也会显示这些信息。

系统托盘默认禁用。禁用时，关闭面板会退出程序。启用时，关闭面板会隐藏窗口，并保留最新用量数据。隐藏期间仍会定时更新，并接收 Codex 用量限制通知。

点击系统托盘图标、选择**显示窗口**或再次启动程序，可恢复面板。面板会恢复已保留的用量数据和上次屏幕位置，不会立即重新请求用量数据。

系统托盘菜单包含以下操作，并以分隔线分组：

- **显示窗口**
- **刷新用量**
- **窗口始终置顶**
- **登录时自动启动**
- **设置**
- **键盘快捷键**
- **退出**

选择**退出**可停止程序。

系统托盘提示先显示程序名称，再按每个用量窗口一行的方式显示，例如：

- `[Claude] 当前: 剩余 d% (于 ... 重置)`
- `[Claude] 每周: 剩余 d% (于 ... 重置)`
- `[Codex] 每周: 剩余 d% (于 ... 重置)`

最后一行为 `最后更新于 HH:mm`。此行使用所选的最后更新日期时间格式。

拖动用量卡片底部的小把手可更改高度。程序会自动保存高度。把手获得键盘焦点后，可使用上／下方向键调整。

在**设置 → 外观**中，拖动**面板字体大小**（80–150%）可查看文字大小。点击**应用**或**确定**保存。鼠标指针移至右下角时，会显示窗口缩放标记。

程序从本地 `codex app-server` 进程读取 `account/rateLimits/read`。未来 Codex 版本如果更改此 API，可能需要更新程序。Claude 用量使用 Anthropic 未公开的端点，可能随时更改。请参阅[启用 Claude 用量（可选）](#启用-claude-用量可选)。

## 文档写作规范

英文 README 使用 [ASD-STE100 简化技术英语](https://www.asd-ste100.org/)作为写作指南：

- 使用短句和主动语态。
- 每句只给出一项指令。
- 操作步骤中的英文句子最多 20 个单词，说明性英文句子最多 25 个单词。
- 同一事物使用相同术语。
- 保留命令、路径、设置键和界面标签的原文。

两个中文版本均以 `README.md` 为翻译来源。操作说明、示例和技术细节应与英文版本保持一致。

## 隐私与许可

请参阅 [PRIVACY.md](PRIVACY.md)、[SECURITY.md](SECURITY.md) 和 [MIT License](LICENSE)。
