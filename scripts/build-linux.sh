#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
source "$ROOT/scripts/lib/log-block.sh"

PROJECT="$ROOT/src/CodexUsageCompanion/CodexUsageCompanion.csproj"
SOLUTION="$ROOT/CodexUsageCompanion.slnx"
ARTIFACTS="$ROOT/artifacts"
RID="${1:-linux-x64}"

info "Preparing Linux release build for $RID."

case "$RID" in
  linux-x64)
    DEB_ARCH=amd64
    ;;
  linux-arm64)
    DEB_ARCH=arm64
    ;;
  *)
    fail "Unsupported RID: $RID (use linux-x64 or linux-arm64)."
    exit 2
    ;;
esac

VERSION="$(dotnet msbuild "$PROJECT" -getProperty:Version -nologo)"
PUBLISH="$ARTIFACTS/publish/$RID"
STAGING="$ARTIFACTS/deb-staging"
MARKETPLACE="$ARTIFACTS/marketplace"
PLUGIN="$MARKETPLACE/plugins/claude-codex-usage-companion"

if [[ "$ARTIFACTS" != "$ROOT/artifacts" ]]; then
  fail "Artifact directory escaped the repository."
  exit 1
fi

info "Cleaning previous artifacts."
rm -rf "$ARTIFACTS"
mkdir -p "$PUBLISH"
ok "Artifact directory prepared."

info "Restoring .NET dependencies."
dotnet restore "$SOLUTION" -m:1 --disable-parallel
dotnet restore "$PROJECT" --runtime "$RID" -m:1 --disable-parallel
ok "Dependencies restored."

info "Running the release test suite."
dotnet test "$SOLUTION" \
  --configuration Release \
  --no-restore \
  -p:TreatWarningsAsErrors=true \
  -p:UseSharedCompilation=false
ok "Release tests passed."

info "Publishing Claude Codex Usage Companion $VERSION for $RID."
dotnet publish "$PROJECT" \
  --configuration Release \
  --runtime "$RID" \
  --self-contained true \
  --no-restore \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  -p:EnableCompressionInSingleFile=true \
  -p:PublishTrimmed=false \
  -p:TreatWarningsAsErrors=true \
  -p:UseSharedCompilation=false \
  -p:DebugType=None \
  -p:DebugSymbols=false \
  --output "$PUBLISH"
ok "Self-contained application published."

info "Building the Debian package for $DEB_ARCH."
mkdir -p \
  "$STAGING/DEBIAN" \
  "$STAGING/usr/bin" \
  "$STAGING/usr/lib/claude-codex-usage-companion" \
  "$STAGING/usr/share/applications" \
  "$STAGING/usr/share/icons/hicolor/scalable/apps"

install -m 755 "$PUBLISH/CodexUsageCompanion" \
  "$STAGING/usr/lib/claude-codex-usage-companion/CodexUsageCompanion"
ln -s ../lib/claude-codex-usage-companion/CodexUsageCompanion \
  "$STAGING/usr/bin/claude-codex-usage-companion"
ln -s claude-codex-usage-companion "$STAGING/usr/bin/claude-codex-usage"
install -m 644 "$ROOT/packaging/linux/claude-codex-usage-companion.desktop" \
  "$STAGING/usr/share/applications/claude-codex-usage-companion.desktop"
install -m 644 "$ROOT/packaging/linux/claude-codex-usage-companion.svg" \
  "$STAGING/usr/share/icons/hicolor/scalable/apps/claude-codex-usage-companion.svg"
chmod -R go-w "$STAGING"

INSTALLED_SIZE="$(du -sk "$STAGING/usr" | cut -f1)"
{
  echo "Package: claude-codex-usage-companion"
  echo "Version: $VERSION"
  echo "Section: devel"
  echo "Priority: optional"
  echo "Architecture: $DEB_ARCH"
  echo "Installed-Size: $INSTALLED_SIZE"
  echo "Depends: libx11-6, libice6, libsm6, libfontconfig1, libharfbuzz0b, fonts-noto-cjk, libnotify-bin, libc6"
  echo "Maintainer: ychsieh95"
  echo "Homepage: https://github.com/ychsieh95/claude-codex-usage-companion"
  echo "Description: Claude and Codex rate-limit usage companion for Linux"
  echo " A compact Avalonia GUI and scriptable CLI for Claude and Codex account limits."
} > "$STAGING/DEBIAN/control"

DEB="$ARTIFACTS/claude-codex-usage-companion_${VERSION}_${DEB_ARCH}.deb"
dpkg-deb --root-owner-group --build "$STAGING" "$DEB"
ok "Debian package created: $DEB"

info "Building the portable archive."
PORTABLE_ROOT="$ARTIFACTS/portable/claude-codex-usage-companion-$VERSION-$RID"
mkdir -p "$PORTABLE_ROOT/assets/screenshots"
install -m 755 "$PUBLISH/CodexUsageCompanion" "$PORTABLE_ROOT/claude-codex-usage-companion"
install -m 644 "$ROOT/README.md" "$PORTABLE_ROOT/README.md"
install -m 644 "$ROOT/README.zh-TW.md" "$PORTABLE_ROOT/README.zh-TW.md"
install -m 644 "$ROOT/README.zh-CN.md" "$PORTABLE_ROOT/README.zh-CN.md"
install -m 644 "$ROOT/assets/screenshots/"*.png "$PORTABLE_ROOT/assets/screenshots/"
install -m 644 "$ROOT/docs/PRIVACY.md" "$PORTABLE_ROOT/PRIVACY.md"
install -m 644 "$ROOT/docs/SECURITY.md" "$PORTABLE_ROOT/SECURITY.md"
install -m 644 "$ROOT/LICENSE" "$PORTABLE_ROOT/LICENSE"
tar -C "$ARTIFACTS/portable" -czf \
  "$ARTIFACTS/claude-codex-usage-companion-$VERSION-$RID.tar.gz" \
  "$(basename "$PORTABLE_ROOT")"
ok "Portable archive created."

info "Building the Codex marketplace archive."
mkdir -p \
  "$MARKETPLACE/.agents/plugins" \
  "$PLUGIN/.codex-plugin" \
  "$PLUGIN/hooks" \
  "$PLUGIN/bin/$RID" \
  "$PLUGIN/assets/screenshots"
install -m 644 "$ROOT/packaging/marketplace.json" \
  "$MARKETPLACE/.agents/plugins/marketplace.json"
install -m 644 "$ROOT/.codex-plugin/plugin.json" "$PLUGIN/.codex-plugin/plugin.json"
install -m 644 "$ROOT/hooks/hooks.json" "$PLUGIN/hooks/hooks.json"
install -m 644 "$ROOT/README.md" "$PLUGIN/README.md"
install -m 644 "$ROOT/README.zh-TW.md" "$PLUGIN/README.zh-TW.md"
install -m 644 "$ROOT/README.zh-CN.md" "$PLUGIN/README.zh-CN.md"
install -m 644 "$ROOT/LICENSE" "$PLUGIN/LICENSE"
install -m 644 "$ROOT/docs/PRIVACY.md" "$PLUGIN/PRIVACY.md"
install -m 644 "$ROOT/docs/SECURITY.md" "$PLUGIN/SECURITY.md"
install -m 644 "$ROOT/packaging/linux/claude-codex-usage-companion.svg" \
  "$PLUGIN/assets/claude-codex-usage-companion.svg"
install -m 644 "$ROOT/assets/screenshots/"*.png "$PLUGIN/assets/screenshots/"
install -m 755 "$PUBLISH/CodexUsageCompanion" \
  "$PLUGIN/bin/$RID/CodexUsageCompanion"

PLUGIN_ZIP="$ARTIFACTS/CodexUsageCompanionMarketplace-v$VERSION-$RID.zip"
(
  cd "$MARKETPLACE"
  zip -q -r "$PLUGIN_ZIP" .
)
ok "Marketplace archive created: $PLUGIN_ZIP"

info "Generating SHA-256 checksums."
sha256sum "$DEB" \
  "$ARTIFACTS/claude-codex-usage-companion-$VERSION-$RID.tar.gz" \
  "$PLUGIN_ZIP" > "$ARTIFACTS/SHA256SUMS"
ok "Checksums generated."

echo "$DEB"
echo "$ARTIFACTS/claude-codex-usage-companion-$VERSION-$RID.tar.gz"
echo "$PLUGIN_ZIP"
echo "$ARTIFACTS/SHA256SUMS"
ok "Linux release build completed successfully."
