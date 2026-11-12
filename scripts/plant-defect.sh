#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "$0")/.."

tests="tests/Alis.Reactive.Conformance.PlaywrightTests"

usage() {
  cat >&2 <<'USAGE'
usage: scripts/plant-defect.sh --test <constant> --in <file> --replace <text> --with <text>

Plants one defect in <file>, the text to replace found once, runs the one test through scripts/playwright.sh, says
whether the test failed, and restores <file> on every exit, an interrupt included.
  --test <constant>   the test's promise constant as the test writes it after Promises. (M3_F1.X8_27_2)

  [plant] caught: ...    the test failed with the defect in place; its failure follows
  [plant] ESCAPED: ...   the test passed with the defect in place
Anything else is no verdict: a defect the build refuses is held by the compiler and needs no test.
USAGE
  exit 2
}

refuse() {
  echo "[plant] ERROR: $1" >&2
  exit 2
}

constant=""
file=""
anchor=""
replacement=""
replacement_given=0
while [ "$#" -gt 0 ]; do
  [ "$#" -ge 2 ] || usage
  case "$1" in
    --test) constant="$2" ;;
    --in) file="$2" ;;
    --replace) anchor="$2" ;;
    --with) replacement="$2"; replacement_given=1 ;;
    *) usage ;;
  esac
  shift 2
done
if [ -z "$constant" ] || [ -z "$file" ] || [ -z "$anchor" ] || [ "$replacement_given" -eq 0 ]; then usage; fi
[[ "$constant" =~ ^[A-Za-z_][A-Za-z0-9_]*\.[A-Za-z_][A-Za-z0-9_]*$ ]] \
  || refuse "--test takes a promise constant as a test writes it after Promises., its class and its name; '$constant' is not one."

repo="$(pwd -P)"
case "$file" in "$repo"/*) file="${file#"$repo"/}" ;; esac
[ -f "$file" ] || refuse "no file '$file'."
case "$file" in "$tests"/*) refuse "'$file' is in the test project; a defect goes in the code the test proves." ;; esac

stop_tree() {
  local child_pid
  for child_pid in $(pgrep -P "$1" || true); do stop_tree "$child_pid"; done
  kill "$1" 2>/dev/null || true
}

backup="$(mktemp "${TMPDIR:-/tmp}/plant-defect.XXXXXX")"
log="$(mktemp "${TMPDIR:-/tmp}/plant-defect-run.XXXXXX")"
cp -p "$file" "$backup"
child=""

restore() {
  local status=$?
  trap - EXIT
  cp "$backup" "$file"
  touch "$file"
  if cmp -s "$backup" "$file"; then
    rm -f "$backup"
    echo "[plant] restored $file"
  else
    echo "[plant] RESTORE FAILED: $file differs from its text before the plant, kept in $backup" >&2
    status=3
  fi
  exit "$status"
}

on_signal() {
  if [ -n "$child" ]; then
    stop_tree "$child"
    wait "$child" 2>/dev/null || true
  fi
  echo "[plant] interrupted; no verdict." >&2
  exit 130
}

trap restore EXIT
trap on_signal INT TERM HUP

if ! python3 -I - "$file" "$anchor" "$replacement" <<'PY'
import pathlib
import sys

path, anchor, replacement = sys.argv[1:4]
source = pathlib.Path(path)
text = source.read_text(encoding="utf-8")
found = text.count(anchor)
if found != 1:
    sys.exit(f"[plant] ERROR: the text to replace is found {found} times in {path}; it must be found once.")
source.write_text(text.replace(anchor, replacement, 1), encoding="utf-8")
PY
then
  exit 2
fi

echo "[plant] planted in $file; running the test $constant"
scripts/playwright.sh --test "$constant" >"$log" 2>&1 &
child=$!
run_status=0
wait "$child" || run_status=$?
child=""

if [ "$run_status" -eq 4 ]; then
  refuse "the build refuses the defect, so the compiler holds it and no test is needed for it. The run's output is $log"
fi
result="$(grep -E '^\[playwright:result\] .*: [0-9]+ selected, [0-9]+ ran, [0-9]+ passed, [0-9]+ failed, ' "$log" | tail -1 || true)"
[[ "$result" =~ :\ ([0-9]+)\ selected,\ ([0-9]+)\ ran,\ ([0-9]+)\ passed,\ ([0-9]+)\ failed, ]] \
  || refuse "the run printed no result line (scripts/playwright.sh exit $run_status); its output is $log"
selected="${BASH_REMATCH[1]}"
ran="${BASH_REMATCH[2]}"
passed="${BASH_REMATCH[3]}"
failed="${BASH_REMATCH[4]}"
[ "$selected" -gt 0 ] && [ "$ran" -eq "$selected" ] || refuse "the run selected $selected and ran $ran; its output is $log"

echo "[plant] $result"
if [ "$passed" -gt 0 ]; then
  echo "[plant] ESCAPED: $constant passed with the defect in place ($passed of $ran)."
  exit 1
fi

echo "[plant] caught: $constant failed with the defect in place ($failed of $ran)."
awk '/^[[:space:]]*Error Message:/ { showing = 1; next } /^[[:space:]]*Stack Trace:/ { showing = 0 } showing && NF { print "[plant]   " $0 }' "$log"
rm -f "$log"
