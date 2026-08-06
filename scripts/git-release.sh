#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
source "$ROOT/scripts/lib/log-block.sh"

VERSION="v0.1.0"

info "Preparing Git release $VERSION."

NOTES="$("$ROOT/scripts/changelog-notes.sh" "$VERSION")"

git switch main
git pull --ff-only origin main

git status --short

bash "$ROOT/scripts/build-linux.sh"
sha256sum -c artifacts/SHA256SUMS

git tag -a "$VERSION" -m "Claude Codex Usage Companion $VERSION" -m "$NOTES"
git push origin "$VERSION"

ok "Git release $VERSION published."
