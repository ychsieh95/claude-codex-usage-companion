# Claude Codex Usage Companion Linux 版

[English](README.md) · [繁體中文（zh-tw）](README.zh-TW.md)

Claude Codex Usage Companion 是一款在 Linux 上运行的本地开源 Claude Code 与 Codex 用量监视器，包含：

- 使用 Avalonia 构建、小巧且始终置顶的桌面辅助面板。
- 支持单次输出、JSON 和实时监看的可脚本化 CLI。
- Codex `SessionStart` 和 `Stop` hooks，用于启动或刷新单一常驻 GUI 进程。
- Claude Code 与 Codex 用量会显示在同一面板中——两者默认均为启用，并可在设置中单独关闭。
- 英文、繁体中文和简体中文用量文字。
- 分类显示的设置窗口，可设置语言、主题、窗口位置、低用量通知、重置通知、用量日志、任务栏与系统托盘图标、登录时自动启动和始终置顶行为。
- 四种系统托盘图标样式：原始图标、Claude 当前会话剩余用量、Claude 每周剩余用量或 Codex 会话剩余用量。
- 不包含遥测；已有 Claude 凭据会原地更新，token 不会写入日志。

此 Linux 移植版基于 [gkfriend/codex-usage-companion](https://github.com/gkfriend/codex-usage-companion)；原始实现以 Windows/WPF 为目标。

## v0.1.3

面板采用参考 [QuotaGem](https://github.com/gyozalab/QuotaGem) 的提供方标题与圆角进度条，将 5h／每周标签、剩余百分比与本地重置时间放在同一行。各列保持对齐，面板根据内容调整大小，隐藏提供方时仍保持设置的屏幕位置。将鼠标移至每周进度条可查看点数信息。

设置分组可点击标题展开或折叠，按常用操作排序：**用量 → 外观 → 窗口与启动 → 通知 → 日期与时间 → 日志 → 更新**。**全部展开**与**全部折叠**可一次控制所有分组。关闭设置时会保存各分组状态，即使点击**取消**也会保留；重新打开设置或重启程序后会恢复，未应用的表单更改仍会丢弃。**外观**设置提供背景颜色（`#RRGGBB` 或 `#AARRGGBB`，留空使用主题颜色），接着是标签与百分比间距（0–300 像素，默认 **128**）及百分比与重置时间间距（0–64 像素，默认 **16**）。更改会实时预览；**确定／应用**保存，**取消**恢复已保存的外观。已有间距设置会保留。

在**设置 → 更新**中，点击**检查更新**可比较当前版本与 GitHub 最新稳定版本。有新版本时，点击**打开版本页面**即可前往下载页面。**自动检查更新（启动时及每天）**默认启用，会在启动时及程序运行期间每 24 小时检查一次。取消勾选后点击**确定／应用**可禁用自动检查，仍可手动检查。自动检查发现新版本时会显示桌面通知；下载与安装需手动完成。手动检查的网络错误会显示在设置中，自动检查的错误则写入日志。

隐藏提供方进度条只影响显示；关闭**启用 Claude 用量显示**或**启用 Codex 用量显示**才会停止该提供方的查询并清除其错误。已启用提供方的错误会保留至该提供方恢复正常，即使进度条已隐藏或另一提供方刷新成功也不会消失。完整版本信息请见[更新日志](CHANGELOG.md)。

用量卡片使用 36 像素进度条，每周进度条下方另留空间显示点数与自动充值信息，并随较大字体增加高度。调整大小的标记仅在鼠标悬停或获得键盘焦点时显示。原有未调整的 78／96 像素默认高度会转换为紧凑高度，自定义调整的布局则保留。

**全部展开**图标是四个向外箭头，**全部折叠**是四个向内箭头；鼠标悬停时会显示提示。已保存的间距从首次显示就会应用，包括等待用量数据时。百分比列根据文字与间距调整宽度；更改任一间距会重新调整面板宽度。手动加宽面板时，多余空间放在百分比列前方，重置时间列仍靠近右侧。预设位置使用桌面工作区域，避免覆盖保留空间的任务栏或面板。

**恢复默认值**会先显示**是／否**确认对话框，再恢复设置表单中的所有偏好，包括卡片高度与间距。选择**否**、按 Escape 或关闭确认对话框不会更改表单。点击**确定／应用**保存，或点击**取消**放弃恢复操作。分组展开状态会保留。调整卡片高度时，进度条轨道与填充区域的高度也会同步调整。

窗口在首次打开前就会根据所有可见卡片与状态行调整大小，底部仅保留正常内边距。首次获取用量数据后，面板会重新调整等待文字所占的列宽，并保持设置的 128／16 像素默认间距。

## 屏幕截图

| 英文（`en-US`） | 繁体中文（`zh-tw`） | 简体中文（`zh-cn`） |
| --- | --- | --- |
| ![英文用量面板](assets/screenshots/overlay-en.png) | ![繁体中文用量面板](assets/screenshots/overlay-zh-tw.png) | ![简体中文用量面板](assets/screenshots/overlay-zh-cn.png) |

用量面板图片使用示例数据。点击设置分组的标题即可展开内容。

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

| 紧凑圆环 | 展开弹出窗口 |
| --- | --- |
| ![Plasma 6 小组件紧凑圆环](assets/screenshots/plasma-widget-circles.png) | ![Plasma 6 小组件展开弹出窗口](assets/screenshots/plasma-widget-expanded.png) |

## 系统要求

- Kubuntu 26.04 或其他现代 x64/ARM64 Linux 发行版。
- Wayland 桌面环境或 X11 会话。
- 已安装并完成身份验证的 Codex CLI，且可通过 `PATH` 中的 `codex` 运行。
- `.deb` 软件包需要：`libx11-6`、`libice6`、`libsm6`、`libfontconfig1`、`libharfbuzz0b`、`fonts-noto-cjk` 和 `libnotify-bin`。

Release 软件包为自包含软件包，无需另外安装 .NET runtime。

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

第一个命令应输出绝对路径，第二个命令应输出版本，第三个命令应确认当前的身份验证方式，最后一个命令应输出可用消息。如果 `command -v codex` 没有输出，请打开新的终端，并确认安装程序显示的安装目录已添加到 `PATH`。

此程序还会搜索 `~/.local/bin`、`~/.npm-global/bin` 和 `~/.codex/bin`。如果 Codex 安装在其他位置，请设置 `CODEX_CLI_PATH=/absolute/path/to/codex`。

## 启用 Claude 用量（可选）

Claude Code 用量默认会与 Codex 一起显示。如需使用，请先安装并登录 [Claude Code](https://code.claude.com)，运行一次 `claude` 让它写入 `~/.claude/.credentials.json`。如果您不使用 Claude Code，可在设置中关闭**启用 Claude 用量显示**（或在 `settings.json` 中设置 `"enableClaudeUsage": false`）；对应的**启用 Codex 用量显示**复选框可用同样方式关闭 Codex 区块。

Claude Code 没有官方 API 可供查询用量，这点与 Codex 不同。本程序会读取 Claude Code 本身已存储在本地的 OAuth access token，直接调用 Anthropic 自家（未公开文档记录）的用量接口。该 access token 仅数小时有效，且 Claude CLI 只在自身运行时更新它，因此重启后存储的 token 通常已过期。本程序因此会以与 CLI 相同的方式更新它——使用 `~/.claude/.credentials.json` 中已有的 refresh token 进行 OAuth refresh 授权——并以原子方式写回更新后的凭据，保留其余所有字段，同时获取 CLI 使用的同一个锁文件，确保两者不会同时轮换 token。程序不会记录任何 token。如果 refresh token 本身已过期或被吊销，会显示错误提示您运行 `claude` 重新登录。由于该接口未公开文档记录，未来可能在未通知的情况下变更；无论配置的更新间隔为何，Claude 用量最多每 180 秒查询一次。如果 Claude 或其凭据文件安装在程序默认搜索位置（`~/.local/bin`、`~/.npm-global/bin`、`~/.claude/local`）以外，请设置 `CLAUDE_CREDENTIALS_PATH=/absolute/path/to/.credentials.json` 或 `CLAUDE_CLI_PATH=/absolute/path/to/claude`。

## 安装桌面程序和 CLI

构建或下载 `.deb`，然后运行：

```bash
sudo apt install ./claude-codex-usage-companion_0.1.3_amd64.deb
```

从 KDE 应用程序菜单启动 **Claude Codex Usage Companion**，或运行：

```bash
claude-codex-usage-companion gui
```

软件包还会安装较短的 `claude-codex-usage` 别名。

`.deb` 会在 `/usr/share/applications/claude-codex-usage-companion.desktop` 创建 KDE/GNOME 应用程序菜单项，并将图标安装到 `/usr/share/icons/hicolor/scalable/apps/`。登录时自动启动默认禁用。在设置窗口中启用后，程序会创建 `${XDG_CONFIG_HOME:-$HOME/.config}/autostart/claude-codex-usage-companion.desktop`。

启用登录时自动启动后，会显示「**最小化窗口**」选项（未启用时则隐藏）。两者都启用时，程序会在登录时启动并且不显示窗口——用量仍会在后台持续更新——之后只要重新打开程序（通过应用程序菜单、系统托盘图标，或再次运行程序）即可让窗口重新显示。

使用 **⚙ 设置** 按钮即可配置任务栏图标、系统托盘、系统托盘图标样式、登录时自动启动、语言、主题、窗口位置、低用量通知、重置通知、用量日志和始终置顶行为。**确定**会应用更改并关闭窗口，**取消**会放弃尚未应用的更改，**应用**则会保存更改但保持窗口打开。

## Plasma 6 小组件

`.deb` 也会安装 **Claude Codex Usage** Plasma 6 小组件。在 Plasma 桌面或面板上单击右键，选择**添加小组件**，搜索 `Claude Codex Usage`，再将它拖到面板或桌面。

面板指示器会分别显示 Claude 当前会话和本周用量，以及 Codex 的 5 小时和每周限制。设置页面提供圆形和条形两种视图，可调整刷新间隔，并选择要在面板和展开弹窗中显示的限制；四个限制默认全部启用。紧凑布局在较宽的小组件中会水平排列，在较高的小组件中则会自动改为垂直排列。打开后可查看本地重置时间、点数、刷新状态及各自的错误。小组件会调用本地 companion 的 `status --json` 命令，因此凭据和网络访问仍由 companion 处理。

Release 构建也会包含独立的 `.plasmoid` 软件包，可无需 root 权限安装：

```bash
kpackagetool6 --type Plasma/Applet --install ./claude-codex-usage-companion-0.1.3.plasmoid
```

使用开发 checkout 时，可运行 `bash scripts/install-plasma-widget.sh` 安装或更新当前源码包。Companion CLI 必须保持安装并可从 `PATH` 运行。

## CLI

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

`status`、`watch` 及其 `--json` 输出会先显示 Claude 再显示 Codex，与面板的卡片顺序一致。`--json` 会嵌套输出为 `{"claude": {"state": ..., "error": ...}, "codex": {"state": ..., "error": ...}}`（任一提供方禁用时，对应字段会完全省略）——这是相对于之前扁平 `--json` 结构的重大变更。

如果 Codex 不在 `PATH` 中，请设置 `CODEX_CLI_PATH=/absolute/path/to/codex`。如需禁用 ANSI 颜色，请设置 `NO_COLOR=1` 或传入 `--no-color`。

## 安装为 Codex plugin

Linux release 构建会生成 `CodexUsageCompanionMarketplace-v0.1.3-linux-x64.zip`。解压后，添加解压出的 marketplace，然后安装并启用 plugin：

```bash
codex plugin marketplace add /path/to/extracted-marketplace
```

Codex 询问时，请检查并信任随附的 `SessionStart` 和 `Stop` hooks。通过 plugin 启动程序时，设置会使用 `PLUGIN_DATA`。

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
  "settingsSections": {
    "usage": true,
    "appearance": true,
    "window": true,
    "notifications": true,
    "dateTime": true,
    "logging": true,
    "updates": true
  },
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

语言值：`en-US`、`zh-tw`、`zh-cn`。首次运行时，系统语言为 `zh-TW`、`zh-HS`、`zh-Hant`、`zh-HK`、`zh-MO` 和其他繁体中文区域性时，默认使用 `zh-tw`；系统语言为 `zh-CN`、`zh-Hans`、`zh-SG` 和其他简体中文区域性时，默认使用 `zh-cn`；其他语言则默认使用上方所示的 `en-US`。仍接受旧版的 `auto` 和 `en` 值，且语言值匹配不区分大小写。

主题值：`dark`、`light`、`system`。`system` 是默认值，会跟随桌面当前的浅色或深色偏好设置。

系统托盘图标样式值为 `original`、`claude-current-session`、`claude-weekly-session` 和 `codex-session`。数字样式会在深色背景上显示粗体、抗锯齿的七段显示器数字，并且不显示百分号。Codex 会话会优先使用 5 小时窗口，若未提供则改用每周用量。

位置值：`left-top`、`middle-top`、`right-top`、`left-center`、`middle-center`、`right-center`、`left-bottom`、`middle-bottom`、`right-bottom`、`custom`。

选择位置后会立即预览。点击 **确定** 或 **应用** 会保留新位置；点击 **取消** 会恢复到最近一次应用的位置。程序仍接受 `top-left` 和 `bottom-right` 等旧版角落位置值。
拖动标题栏可自由移动窗口；位置会切换为 `custom` 并保存坐标。在设置中选择 `custom` 会恢复已保存的位置；如果尚未保存，则保留当前位置。

GUI 全局提供键盘快捷键。在主窗口中，按 `F1` 列出所有快捷键、按 `S` 打开设置、按 `Ctrl+R` 刷新使用量，或按 `Esc` 关闭窗口。标题栏的 **?** 按钮与系统托盘菜单的**键盘快捷键**会打开同一份清单，可按 `Esc`、`F1` 或 **确定** 关闭。在设置窗口中，按 `Ctrl+S` 保存，或按 `Esc` 关闭。若关闭设置时仍有未保存的更改，程序会先要求确认，避免直接放弃更改。

**更新间隔** 设置提供 1 至 60 分钟的常用值。当前的 1 分钟间隔是默认值和最小值；以前保存在此范围内的自定义间隔仍可选择。

低用量通知默认禁用。启用后，5 小时或每周剩余用量一旦严格低于配置的 `1`–`100` 百分比阈值，就会发送一次桌面通知；当该限额恢复到阈值或以上时会重新启用通知。重置通知可独立设置且默认禁用，刷新后确认新的重置周期时会发送通知。

重置日期时间预设格式包括 `MMM D`（默认）、`MMM D HH:mm`、`yyyy-MM-dd` 和 `yyyy-MM-dd HH:mm`。最后更新预设格式包括 `HH:mm`（默认）、`HH:mm:ss`、`MMM D HH:mm` 和 `yyyy-MM-dd HH:mm`。两个下拉框也接受自定义 .NET 日期时间格式字符串，并支持以大写 `D` 作为日期中的日。输入时会显示本地化预览以检查格式；格式错误时会显示错误并禁用 **确定** 和 **应用**。月份名称和日期表示方式会根据语言本地化。

不透明度限制为 `0.5`–`1.0`，边距限制为 `0`–`64` 像素，更新间隔限制为 `60`–`3600` 秒。

`enableClaudeUsage` 与 `enableCodexUsage`（默认均为 `true`）可分别独立显示或隐藏对应提供方的区块；面板高度会自动调整。无论配置的更新间隔为何，Claude 最多每 180 秒查询一次——详见[启用 Claude 用量（可选）](#启用-claude-用量可选)。

**显示的用量限制**复选框可分别控制 Claude 当前会话、Claude 本周用量、Codex 5 小时用量和 Codex 每周用量卡片。对应设置为 `showClaudeSession`、`showClaudeWeekly`、`showCodexFiveHour` 和 `showCodexWeekly`；为保留应用程序原有布局，Codex 5 小时卡片默认隐藏，其余三张卡片默认显示。

## 用量更新日志

用量日志默认禁用。启用后，每次成功刷新和刷新错误都会追加到指定文件。CSV 是默认格式，默认路径遵循 `${XDG_STATE_HOME:-$HOME/.local/state}/claude-codex-usage-companion/usage-history.csv`。

支持的格式为 `txt`（每行一条键值记录）、`csv`（表头后跟数据行）和 `jsonl`（每行一个 JSON 对象）。更改格式时，配置的文件扩展名会自动更改为 `.txt`、`.csv` 或 `.jsonl`。记录包含更新时间、该条记录属于哪个提供方（`claude` 或 `codex`）、成功或错误状态、5 小时和每周剩余百分比及重置时间、可用重置点数，以及任何刷新错误。相对路径会以应用程序的 XDG state 目录为基准解析。对于解析现有日志文件的程序而言，新增的 `provider` 字段属于重大变更。

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

此命令会停止当前用户的辅助程序，按主机架构构建，再以 `sudo apt reinstall -f` 重新安装生成的 `.deb`，最后启动已安装的命令。请以普通桌面用户运行，仅在安装软件包时请求 sudo；任何步骤失败都会停止。

构建过程会运行测试套件，并在 `artifacts/` 下输出 `.deb`、便携式 `.tar.gz`、Plasma `.plasmoid`、Codex marketplace `.zip` 和校验和。

开发环境运行方式：

```bash
dotnet run --project src/CodexUsageCompanion -- status
dotnet run --project src/CodexUsageCompanion -- gui
```

## Linux 行为

GUI 使用 `XDG_RUNTIME_DIR` 下仅限所有者访问的 Unix socket，以确保只运行一个实例并接收刷新信号。设置遵循 XDG config 约定；大小受限的日志存储在 `${XDG_STATE_HOME:-$HOME/.local/state}/claude-codex-usage-companion`。

在提供 XWayland 的 Wayland 桌面中，程序使用 Avalonia 的 X11 兼容后端，让所选窗口位置决定启动位置，并能在更改设置后移动窗口。如果只有原生 Wayland，合成器会控制窗口位置，也可能忽略此设置。X11 会话使用 X11 后端。拖动标题栏可移动面板，拖动右下角可调整大小；卡片和用量条会随宽度调整，窗口较矮时可以滚动。面板仍可拖动，并依次包含键盘快捷键、置顶固定、设置、刷新、最小化和关闭控件，其中最小化和关闭这两个窗口控件会以较宽的间距分隔。标题栏的固定按钮、设置窗口的「窗口始终置顶」复选框，以及系统托盘菜单的「窗口始终置顶」复选框都会反映并更新同一项设置。Claude 的**当前会话**/**本周用量（全部）**区块会显示在最前面，其后才是 Codex 区块。提供方标题显示 Claude 或 OpenAI 图标。每周进度条的提示会显示 Codex 点数余额与自动充值，或 Claude 加购用量支出与每月上限。系统托盘默认禁用：关闭面板会退出常驻进程。启用系统托盘后，关闭会隐藏面板并保留最新用量快照；隐藏期间仍会按照更新间隔在后台更新，并接收 Codex 用量限制通知。单击系统托盘图标、选择 **显示窗口**，或再次启动程序时，会恢复缓存状态而不立即重新获取。系统托盘菜单包含**显示窗口**、**刷新使用量**、**窗口始终置顶**、**登录时自动启动**、**设置**、**键盘快捷键**和**退出**，并以分隔线区分操作分组。选择 **退出** 才会停止进程。系统托盘提示会先显示应用程序名称，接着每个用量窗口各占一行，格式为 `[Claude] 当前: 剩余 d% (于 ... 重置)`、`[Claude] 每周: 剩余 d% (于 ... 重置)`、`[Codex] 每周: 剩余 d% (于 ... 重置)`，最后是使用所选最后更新日期时间格式的 `最后更新于 HH:mm`。

拖动用量卡片底部的小把手可调整该卡片高度，系统会自动保存；把手获得焦点后也可使用上／下方向键。在**设置 → 外观**中拖动**面板字体大小**（80–150%）可预览文字，再按**应用**或**确定**保存。右下角的窗口缩放标记仅在鼠标悬停时显示。

程序从本地 `codex app-server` 进程读取 `account/rateLimits/read`。此 API 可能会演进，因此未来的 Codex release 可能需要兼容性更新。Claude 用量依赖 Anthropic 未公开文档记录的接口（参见[启用 Claude 用量（可选）](#启用-claude-用量可选)），该接口可能在未通知的情况下变更。

## 隐私与许可

请参阅 [PRIVACY.md](PRIVACY.md)、[SECURITY.md](SECURITY.md) 和 [MIT License](LICENSE)。
