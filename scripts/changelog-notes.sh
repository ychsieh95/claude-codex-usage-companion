#!/usr/bin/env bash
set -euo pipefail

# Print the CHANGELOG.md section for a given version, e.g.:
#   scripts/changelog-notes.sh v0.1.0
#   scripts/changelog-notes.sh 0.1.0
#
# Extracts everything between the matching "## [<version>]" heading and the
# next "## [" heading (or end of file), trimmed of leading/trailing blank
# lines.

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CHANGELOG="$ROOT/CHANGELOG.md"

if [[ $# -ne 1 ]]; then
    echo "Usage: $(basename "$0") <version>" >&2
    exit 1
fi

version="${1#v}"

notes="$(awk -v ver="$version" '
    $0 ~ "^## \\[" ver "\\]" { found=1; next }
    found && /^## \[/ { exit }
    found { print }
' "$CHANGELOG" | sed -e '/./,$!d')"

if [[ -z "$notes" ]]; then
    echo "No CHANGELOG.md section found for version $version" >&2
    exit 1
fi

printf '%s\n' "$notes"
