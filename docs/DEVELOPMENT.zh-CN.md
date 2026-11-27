# 开发

[English](DEVELOPMENT.md) · [繁體中文（zh-tw）](DEVELOPMENT.zh-TW.md)

本文档说明如何构建 [Claude Codex Usage Companion](../README.zh-CN.md)，以及程序在 Linux 上的行为。

## 构建

安装 .NET 10 SDK 或更新版本，然后运行：

```bash
bash scripts/build-linux.sh               # host architecture
bash scripts/build-linux.sh linux-arm64   # ARM64
```

构建时会运行测试，并将 `.deb`、`.tar.gz`、`.plasmoid`、Codex 市场 `.zip` 和 `SHA256SUMS` 写入 `artifacts/`。

要构建 `.snap`，请先通过 `sudo snap install snapcraft --classic` 安装 snapcraft，再运行 `bash scripts/build-snap.sh`。

- `SKIP_LINUX_BUILD=true` 使用现有构建。
- `SNAPCRAFT_ARGS=--destructive-mode` 可在 Ubuntu 24.04 主机上构建，无需 LXD。

要重新构建、重新安装 `.deb` 并重启已安装的程序，请以普通用户运行 `./scripts/rebuild-reinstall.sh`。只有软件包安装会使用 sudo。此脚本会禁用 NuGet 漏洞审核。要启用，请设置 `NUGET_AUDIT=true`。

其他脚本：

- `bash scripts/install-plasma-widget.sh`：从源码目录安装小部件。
- `bash scripts/capture-screenshots.sh`：生成 `assets/screenshots/` 中的屏幕截图。

开发时可运行：

```bash
dotnet run --project src/CodexUsageCompanion -- status
dotnet run --project src/CodexUsageCompanion -- gui
```

## Linux 行为

- GUI 在 `XDG_RUNTIME_DIR` 下使用仅所有者可访问的 Unix socket，以维持单个程序实例并接收刷新信号。
- 设置遵循 XDG 配置约定。程序日志有大小上限，保存在 `${XDG_STATE_HOME:-$HOME/.local/state}/claude-codex-usage-companion`。
- 在有 XWayland 的 Wayland 环境中，程序使用 X11 兼容后端，因此设置的位置会生效。仅有原生 Wayland 时，位置由合成器控制。
- Codex `account/rateLimits/read` API 和 Claude 用量端点可能在未来版本变更，因此可能需要更新程序。

## 文档风格

英文文档（`README.md`、`docs/USER-GUIDE.md` 和 `docs/DEVELOPMENT.md`）以 [ASD-STE100 简化技术英语](https://www.asd-ste100.org/)为写作指南：

- 使用短句和主动语态。
- 每句只提供一项指令。
- 操作句最多 20 个单词，描述句最多 25 个单词。
- 同一项内容使用相同术语。
- 保持命令、路径、设置键和 UI 标签不变。

繁体中文（`.zh-TW.md`）和简体中文（`.zh-CN.md`）翻译均以英文文件为来源，结构、指令、示例和技术细节须与英文版一致。
