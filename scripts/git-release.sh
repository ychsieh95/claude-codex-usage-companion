#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
source "$ROOT/scripts/lib/log-block.sh"
cd "$ROOT"

VERSION="v0.2.3"

info "Preparing Git release $VERSION."

NOTES="$("$ROOT/scripts/changelog-notes.sh" "$VERSION")"

git switch main
git pull --ff-only origin main

git status --short

# A retry may follow a successful tag creation but a failed push. Preserve the
# original release commit, even if main has advanced since the tag was created.
if git show-ref --verify --quiet "refs/tags/$VERSION"; then
    info "Tag $VERSION already exists at $(git rev-parse --short "$VERSION^{commit}"); reusing it."
    git push origin "refs/tags/$VERSION:refs/tags/$VERSION"
    ok "Git release $VERSION published (existing tag reused)."
    exit 0
fi

bash "$ROOT/scripts/build-linux.sh"
(cd "$ROOT/artifacts" && sha256sum -c SHA256SUMS)

git tag -a "$VERSION" -m "Claude Codex Usage Companion $VERSION" -m "$NOTES"
git push origin "refs/tags/$VERSION:refs/tags/$VERSION"

ok "Git release $VERSION published."
