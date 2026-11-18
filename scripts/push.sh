#!/usr/bin/env bash
cd "$(dirname "$0")/.." || exit 1
set -u
test "$#" -eq 0 || { echo "usage: push.sh"; exit 1; }
branch="$(git branch --show-current)"
test -n "$branch" || { echo "detached HEAD; no push"; exit 1; }
test "$branch" != main || { echo "main is refused; no push"; exit 1; }
test -f .work/attribution.txt || { echo "[push] missing .work/attribution.txt; write the two attribution lines there"; exit 1; }
python3 -I -B - <<'ATTRIBUTION' || exit 1
from pathlib import Path
lines = Path(".work/attribution.txt").read_text().splitlines()
assert len(lines) == 2 and all(lines), ".work/attribution.txt must hold two attribution lines"
ATTRIBUTION
W=.work/logs
mkdir -p "$W" || exit 1
checked="$(git rev-parse --short=9 HEAD)"
log="$W/spec-check-$checked.log"
git diff --quiet -- spec/STATE.json && { echo "spec/STATE.json holds no change: the full check at $checked did not write it, or it was restored; no push"; exit 1; }
test -z "$(git status --porcelain --untracked-files=no -- . ':!spec/STATE.json' ':!HANDOFF.md')" || { echo "tracked changes beside STATE and HANDOFF.md; no push"; exit 1; }
grep -q -x "spec check at $checked: 40 of 40 checks pass" "$log" && git push origin "HEAD:refs/heads/$branch" || { echo "content push refused"; exit 1; }
test "$(git ls-remote origin "refs/heads/$branch" | cut -c1-40)" = "$(git rev-parse HEAD)" || { echo "remote differs after the content push"; exit 1; }
python3 scripts/handoff.py || { echo "handoff refused; no STATE commit"; exit 1; }
git add spec/STATE.json HANDOFF.md
{
  printf 'spec: STATE and handoff at %s\n\n' "$checked"
  cat .work/attribution.txt
  printf '\n'
} > "$W/message-state-$checked.txt"
git commit -q -F "$W/message-state-$checked.txt" || { echo "STATE commit refused"; exit 1; }
grep -q -x "spec check at $checked: 40 of 40 checks pass" "$log" && git push origin "HEAD:refs/heads/$branch" || { echo "STATE push refused"; exit 1; }
test "$(git ls-remote origin "refs/heads/$branch" | cut -c1-40)" = "$(git rev-parse HEAD)" || { echo "remote differs after the STATE push"; exit 1; }
echo "pushed $checked and $(git rev-parse --short=9 HEAD); remote equals HEAD"
