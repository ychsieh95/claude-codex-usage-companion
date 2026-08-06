# Canonical color/log boilerplate shared by scripts across this repo.
#
# Scripts that run in place from this repo checkout (e.g. bottles-line/line-setup.sh)
# can source this file directly:
#   REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
#   source "$REPO_ROOT/lib/log-block.sh"
#
# Scripts that get copied out standalone (~/.local/bin for cron/systemd/
# desktop-entry use, decoupled from this checkout) can't rely on a runtime
# `source` — it would break once deployed. Those instead wrap their own
# copy of the block below with the markers, then run ./sync-log-block.sh
# to keep it in sync with this canonical copy.
#
# >>> shared-log-block (managed by sync-log-block.sh — do not edit by hand) >>>
# Define color codes (disabled per-stream when output is not a terminal,
# so redirected/journald-captured logs stay free of escape codes)
if [[ -t 1 ]]; then
    BLUE=$'\033[0;34m'
    GREEN=$'\033[0;32m'
    YELLOW=$'\033[1;33m'
    PURPLE=$'\033[0;35m'
    GRAY=$'\033[0;90m'   # timestamp (bright black)
    NC=$'\033[0m' # No Color (Reset)
else
    BLUE='' GREEN='' YELLOW='' PURPLE='' GRAY='' NC=''
fi

if [[ -t 2 ]]; then
    ORANGE=$'\033[38;5;208m'
    RED=$'\033[0;31m'
    GRAY_ERR=$'\033[0;90m'
    NC_ERR=$'\033[0m'
else
    ORANGE='' RED='' GRAY_ERR='' NC_ERR=''
fi

# Log function: log <time_color> <level_color> <level_text> <reset_code> <message...>
# Timestamps are on by default. Set LOG_NO_TIME=1 (before sourcing, or via
# systemd Environment=) to drop them — e.g. for short interactive scripts,
# or units where journald already stamps every line.
log() {
    local tcolor="$1" color="$2" level="$3" reset="$4"
    shift 4
    if [[ -n "${LOG_NO_TIME:-}" ]]; then
        printf '[%s%s%s] %s\n' "${color}" "${level}" "${reset}" "$*"
    else
        printf '[%s%s%s] [%s%s%s] %s\n' \
            "${tcolor}" "$(date '+%Y-%m-%d %H:%M:%S')" "${reset}" \
            "${color}" "${level}" "${reset}" "$*"
    fi
}
# Wrapper functions for cleaner syntax (warn/fail go to stderr)
info() { log "${GRAY}"     "${BLUE}"   " INFO " "${NC}"     "$@"; }
ok  () { log "${GRAY}"     "${GREEN}"  "  OK  " "${NC}"     "$@"; }
note() { log "${GRAY}"     "${YELLOW}" " NOTE " "${NC}"     "$@"; }
warn() { log "${GRAY_ERR}" "${ORANGE}" " WARN " "${NC_ERR}" "$@" >&2; }
fail() { log "${GRAY_ERR}" "${RED}"    " FAIL " "${NC_ERR}" "$@" >&2; }
# <<< shared-log-block <<<
