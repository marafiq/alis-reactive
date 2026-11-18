#!/usr/bin/env bash
cd "$(dirname "$0")/.." || exit 1
set -u
test "$#" -eq 2 || { echo "usage: land.sh <name> <message file>"; exit 1; }
n="$1"; msg="$2"; W=.work/logs
T=scripts
mkdir -p "$W" || exit 1
R=tests/Alis.Reactive.Conformance.PlaywrightTests/TestResults
test "$(git branch --show-current)" != main || { echo "[land] main is refused"; exit 1; }
test -f "$msg" || { echo "[land] no message at $msg"; exit 1; }
test -n "$(git diff --cached --name-only)" || { echo "[land] nothing staged"; exit 1; }
test -f .work/last-run.txt || {
  echo "[land] .work/last-run.txt is missing. At the commit before, run scripts/playwright.sh --all once."
  echo "[land] Then write the line '<run folder> <sha>' to .work/last-run.txt; the run folder is its name under $R."
  exit 1
}
paused="$(pgrep -f 'tlc2.TL[C]' | tr '\n' ' ')"
resume() {
  test -n "$paused" && kill -CONT $paused 2>/dev/null
  echo "[quiet] resumed: ${paused:-none}"
}
trap resume EXIT
test -n "$paused" && kill -STOP $paused
echo "[quiet] paused TLC: ${paused:-none}"
bash "$T/gate.sh" "$n"
grep -q -x '\[check\] every check passed\.' "$W/gate-$n.log" || { echo "[land] the gate did not pass"; exit 1; }
test -z "$(git diff --name-only)" || { echo "[land] the gate left unstaged changes:"; git diff --name-only; exit 1; }
read -r last_run last_sha < .work/last-run.txt
caffeinate -i scripts/playwright.sh --all > "$W/pw-all-$n.log" 2>&1
grep -E '^\[playwright:result\] all ' "$W/pw-all-$n.log" | tail -1
run="$(command ls -1dt "$R"/[0-9]*/ | head -1)"
run="${run%/}"
test "$(basename "$run")" != "$last_run" || { echo "[land] the browser run wrote no new folder"; exit 1; }
grep -q -E '"label": "all"' "$run/selection.json" || { echo "[land] the newest run is not --all: $run"; exit 1; }
python3 -I "$T/regressions.py" "$R/$last_run" "$run" "$last_sha" > "$W/regress-$n.txt"
cat "$W/regress-$n.txt"
grep -q -x "no test that passed at $last_sha fails or is gone" "$W/regress-$n.txt" || { echo "[land] a test that passed before fails; not committed"; exit 1; }
test -z "$(git diff --name-only)" || { echo "[land] the browser run left unstaged changes:"; git diff --name-only; exit 1; }
python3 -I "$T/stamp.py" "$msg" "$W/pw-all-$n.log" "$W/regress-$n.txt" "$W/stamped-$n.txt" || { echo "[land] the message could not be stamped"; exit 1; }
ALIS_LANDING=1 git commit -q -F "$W/stamped-$n.txt" || { echo "[land] the commit failed"; exit 1; }
printf '%s %s\n' "$(basename "$run")" "$(git rev-parse --short=9 HEAD)" > .work/last-run.txt
git log --oneline -1
lsof -nP -iTCP -sTCP:LISTEN | grep -E 'Alis|dotnet' || true
