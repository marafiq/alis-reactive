#!/usr/bin/env bash
cd "$(dirname "$0")/.." || exit 1
set -u
test "$#" -eq 0 || { echo "usage: full-check.sh"; exit 1; }
W=.work/logs
mkdir -p "$W" || exit 1
test -n "$(git status --porcelain --untracked-files=no -- . ':!spec/STATE.json' ':!HANDOFF.md')" && { echo "tracked changes beside STATE and HANDOFF.md; no full check"; exit 1; }
sha="$(git rev-parse --short=9 HEAD)"
log="$W/spec-check-$sha.log"
test -f "$log" && mv "$log" "$W/spec-check-$sha.$(date +%H%M%S).log"
caffeinate -i python3 spec/tools/check.py > "$log" 2>&1
echo "log $log"
head -1 "$log"
