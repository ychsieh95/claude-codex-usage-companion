#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
source "$ROOT/scripts/lib/log-block.sh"

case "$(dpkg --print-architecture)" in
  amd64) RID=linux-x64; DEB_ARCH=amd64 ;;
  arm64) RID=linux-arm64; DEB_ARCH=arm64 ;;
  *) fail "Unsupported architecture; use amd64 or arm64."; exit 2 ;;
esac

# Match executable names, not shells or scripts containing the project path.
PROCESS_PATTERN='^([^[:space:]]*/)?(claude-codex-usage-companion|CodexUsageCompanion)([[:space:]]|$)'
COMPANION_UID="$(id -u)"

info "Stopping Claude Codex Usage Companion."
if pkill --signal TERM --uid "$COMPANION_UID" --full "$PROCESS_PATTERN"; then
  # Allow the resident process to release its instance socket before restarting.
  for ((attempt = 0; attempt < 50; attempt++)); do
    if ! pgrep --uid "$COMPANION_UID" --full "$PROCESS_PATTERN" >/dev/null; then
      break
    fi
    sleep 0.2
  done
  if pgrep --uid "$COMPANION_UID" --full "$PROCESS_PATTERN" >/dev/null; then
    warn "Companion did not stop within 10 seconds; forcing it to exit."
    pkill --signal KILL --uid "$COMPANION_UID" --full "$PROCESS_PATTERN"
  fi
else
  stop_status=$?
  if ((stop_status != 1)); then
    fail "Unable to stop the companion (exit $stop_status)."
    exit "$stop_status"
  fi
  note "Companion is not running."
fi

info "Building the Linux package."
# Local reinstalls can use cached packages without contacting the audit service.
NUGET_AUDIT="${NUGET_AUDIT:-false}" bash "$ROOT/scripts/build-linux.sh" "$RID"

shopt -s nullglob
packages=("$ROOT"/artifacts/claude-codex-usage-companion_*_"$DEB_ARCH".deb)
if ((${#packages[@]} != 1)); then
  fail "Expected exactly one generated $DEB_ARCH package; found ${#packages[@]}."
  exit 1
fi

info "Reinstalling ${packages[0]}."
sudo apt reinstall -f "${packages[0]}"

info "Starting Claude Codex Usage Companion."
exec claude-codex-usage-companion
