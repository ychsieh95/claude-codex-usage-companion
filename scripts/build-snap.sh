#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
source "$ROOT/scripts/lib/log-block.sh"

ARTIFACTS="$ROOT/artifacts"
RID="${1:-linux-x64}"
SNAP_ROOT="$ARTIFACTS/snap-root"

case "$RID" in
  linux-x64) SNAP_ARCH=amd64 ;;
  linux-arm64) SNAP_ARCH=arm64 ;;
  *)
    fail "Unsupported RID: $RID (use linux-x64 or linux-arm64)."
    exit 2
    ;;
esac

if ! command -v snapcraft >/dev/null; then
  fail "snapcraft is not installed. Run: sudo snap install snapcraft --classic"
  exit 1
fi

PUBLISH="$ARTIFACTS/publish/$RID/CodexUsageCompanion"
if [[ "${SKIP_LINUX_BUILD:-false}" != true || ! -x "$PUBLISH" ]]; then
  info "Running the Linux release build for $RID."
  bash "$ROOT/scripts/build-linux.sh" "$RID"
fi

VERSION="$(cat "$ARTIFACTS/VERSION")"
PLASMOID="$ARTIFACTS/claude-codex-usage-companion-${VERSION}.plasmoid"

info "Staging snap contents for $SNAP_ARCH."
rm -rf "$SNAP_ROOT"
mkdir -p \
  "$SNAP_ROOT/bin" \
  "$SNAP_ROOT/usr/share/applications" \
  "$SNAP_ROOT/usr/share/plasma"
install -m 755 "$PUBLISH" "$SNAP_ROOT/bin/CodexUsageCompanion"
install -m 644 "$ROOT/packaging/snap/claude-codex-usage-companion.desktop" \
  "$SNAP_ROOT/usr/share/applications/claude-codex-usage-companion.desktop"
install -m 644 "$PLASMOID" "$SNAP_ROOT/usr/share/plasma/$(basename "$PLASMOID")"
echo "$VERSION" > "$SNAP_ROOT/VERSION"
ok "Snap contents staged."

SNAP_FILE="$ARTIFACTS/claude-codex-usage-companion_${VERSION}_${SNAP_ARCH}.snap"
info "Packing the snap."
(
  cd "$ROOT"
  snapcraft pack --build-for="$SNAP_ARCH" --output "$SNAP_FILE" ${SNAPCRAFT_ARGS:-}
)
ok "Snap package created: $SNAP_FILE"

info "Adding the snap to the checksums."
(
  cd "$ARTIFACTS"
  sha256sum "$(basename "$SNAP_FILE")" >> SHA256SUMS
)
ok "Checksums updated."

echo "$SNAP_FILE"
