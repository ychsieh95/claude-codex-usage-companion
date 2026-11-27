# 開發

[English](DEVELOPMENT.md) · [简体中文（zh-cn）](DEVELOPMENT.zh-CN.md)

本文件說明如何建置 [Claude Codex Usage Companion](../README.zh-TW.md)，以及程式在 Linux 上的行為。

## 建置

安裝 .NET 10 SDK 或更新版本，然後執行：

```bash
bash scripts/build-linux.sh               # host architecture
bash scripts/build-linux.sh linux-arm64   # ARM64
```

建置時會執行測試，並將 `.deb`、`.tar.gz`、`.plasmoid`、Codex 市集 `.zip` 與 `SHA256SUMS` 寫入 `artifacts/`。

若要建置 `.snap`，請先以 `sudo snap install snapcraft --classic` 安裝 snapcraft，再執行 `bash scripts/build-snap.sh`。

- `SKIP_LINUX_BUILD=true` 使用既有建置。
- `SNAPCRAFT_ARGS=--destructive-mode` 可在 Ubuntu 24.04 主機上建置，不需 LXD。

若要重新建置、重新安裝 `.deb` 並重新啟動已安裝的程式，請以一般使用者執行 `./scripts/rebuild-reinstall.sh`。只有套件安裝會使用 sudo。此指令碼會停用 NuGet 弱點稽核。若要啟用，請設定 `NUGET_AUDIT=true`。

其他指令碼：

- `bash scripts/install-plasma-widget.sh`：從原始碼目錄安裝小工具。
- `bash scripts/capture-screenshots.sh`：產生 `assets/screenshots/` 中的螢幕截圖。

開發時可執行：

```bash
dotnet run --project src/CodexUsageCompanion -- status
dotnet run --project src/CodexUsageCompanion -- gui
```

## Linux 行為

- GUI 在 `XDG_RUNTIME_DIR` 下使用僅擁有者可存取的 Unix socket，以維持單一程式執行個體並接收重新整理訊號。
- 設定遵循 XDG 組態慣例。程式記錄有大小上限，存放於 `${XDG_STATE_HOME:-$HOME/.local/state}/claude-codex-usage-companion`。
- 在有 XWayland 的 Wayland 環境中，程式使用 X11 相容後端，因此設定的位置會生效。僅有原生 Wayland 時，位置由合成器控制。
- Codex `account/rateLimits/read` API 與 Claude 用量端點可能在未來版本變更，因此可能需要更新程式。

## 文件風格

英文文件（`README.md`、`docs/USER-GUIDE.md` 與 `docs/DEVELOPMENT.md`）以 [ASD-STE100 簡化技術英文](https://www.asd-ste100.org/)為寫作指南：

- 使用短句與主動語態。
- 每句只提供一項指示。
- 操作句最多 20 個單字，描述句最多 25 個單字。
- 同一項目使用相同術語。
- 保持命令、路徑、設定鍵與 UI 標籤不變。

繁體中文（`.zh-TW.md`）與簡體中文（`.zh-CN.md`）翻譯皆以英文檔案為來源，結構、指示、範例與技術細節須與英文版一致。
