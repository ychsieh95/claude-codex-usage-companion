#!/usr/bin/env bash
# Sign artifacts/SHA256SUMS so the app can verify downloaded updates.
#
# The private key comes from UPDATE_SIGNING_KEY (PEM text, used by the release
# workflow), from the file named by UPDATE_SIGNING_KEY_FILE, or from the local
# .secrets/update-signing-key.pem (gitignored). The script then
# checks the signature against packaging/update-signing-key.pub.pem, the public
# key built into the app, so a release cannot ship with a key the app rejects.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
source "$ROOT/scripts/lib/log-block.sh"

ARTIFACTS="$ROOT/artifacts"
SUMS="$ARTIFACTS/SHA256SUMS"
SIG="$ARTIFACTS/SHA256SUMS.sig"
PUBLIC_KEY="$ROOT/packaging/update-signing-key.pub.pem"

if [[ ! -f "$SUMS" ]]; then
  fail "Missing $SUMS. Build the release artifacts first."
  exit 1
fi

key_file="${UPDATE_SIGNING_KEY_FILE:-}"
LOCAL_KEY="$ROOT/.secrets/update-signing-key.pem"
if [[ -z "$key_file" && -z "${UPDATE_SIGNING_KEY:-}" && -f "$LOCAL_KEY" ]]; then
  key_file="$LOCAL_KEY"
fi
cleanup() { :; }
if [[ -z "$key_file" ]]; then
  if [[ -z "${UPDATE_SIGNING_KEY:-}" ]]; then
    fail "Set UPDATE_SIGNING_KEY or UPDATE_SIGNING_KEY_FILE, or add .secrets/update-signing-key.pem, to sign the release."
    exit 1
  fi
  key_file="$(mktemp)"
  cleanup() { rm -f "$key_file"; }
  chmod 600 "$key_file"
  printf '%s\n' "$UPDATE_SIGNING_KEY" > "$key_file"
fi
trap cleanup EXIT

info "Signing SHA256SUMS."
openssl dgst -sha256 -sign "$key_file" -out "$SIG" "$SUMS"
openssl dgst -sha256 -verify "$PUBLIC_KEY" -signature "$SIG" "$SUMS" >/dev/null
ok "Signature created and verified with the app's public key: $SIG"
