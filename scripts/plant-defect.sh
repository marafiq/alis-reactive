#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "$0")/.."

usage() {
  echo "usage: scripts/plant-defect.sh --project <name> --filter <expression> --test <method> --in <file> --replace <text> --with <text>" >&2
  echo "Plants one defect in <file>, runs the filtered Playwright tests, says whether <method> failed, and restores <file>." >&2
  exit 2
}

refuse() {
  echo "[plant] ERROR: $1" >&2
  exit 2
}

project=""
filter=""
test_name=""
file=""
anchor=""
replacement=""
replacement_given=0
while [ "$#" -gt 0 ]; do
  [ "$#" -ge 2 ] || usage
  case "$1" in
    --project) project="$2" ;;
    --filter) filter="$2" ;;
    --test) test_name="$2" ;;
    --in) file="$2" ;;
    --replace) anchor="$2" ;;
    --with) replacement="$2"; replacement_given=1 ;;
    *) usage ;;
  esac
  shift 2
done
if [ -z "$project" ] || [ -z "$filter" ] || [ -z "$file" ] || [ -z "$anchor" ] || [ "$replacement_given" -eq 0 ]; then usage; fi
[[ "$test_name" =~ ^[A-Za-z_][A-Za-z0-9_]*$ ]] || refuse "--test takes the test's method name, a C# identifier; '$test_name' is not one."

repo="$(pwd -P)"
case "$file" in "$repo"/*) file="${file#"$repo"/}" ;; esac
[ -f "$file" ] || refuse "no file '$file'."

descriptor=""
for candidate in tests/*/playwright.project.sh; do
  if (source "$candidate" && [ "${playwright_project:-}" = "$project" ]); then descriptor="$candidate"; break; fi
done
[ -n "$descriptor" ] || refuse "no tests/*/playwright.project.sh declares the project '$project'."
project_dir="$(dirname "$descriptor")"
results_dir="$project_dir/TestResults/observable"

case "$file" in "$project_dir"/*) refuse "'$file' is in the test project; a defect goes in the code the test proves." ;; esac
(source "$descriptor" && declare -F playwright_build_assets >/dev/null) \
  || refuse "$descriptor declares no playwright_build_assets, so a defect planted in a source cannot reach the page."

build_assets() {
  (source "$descriptor" && playwright_build_assets)
}

stop_tree() {
  local child_pid
  for child_pid in $(pgrep -P "$1" || true); do stop_tree "$child_pid"; done
  kill "$1" 2>/dev/null || true
}

backup="$(mktemp "${TMPDIR:-/tmp}/plant-defect.XXXXXX")"
cp -p "$file" "$backup"
child=""

restore() {
  local status=$?
  trap - EXIT
  cp "$backup" "$file"
  touch "$file"
  if ! build_assets >/dev/null 2>&1; then
    echo "[plant] ERROR: the assets did not build after the restore." >&2
    status=3
  fi
  if cmp -s "$backup" "$file"; then
    rm -f "$backup"
    echo "[plant] restored $file"
  else
    echo "[plant] RESTORE FAILED: $file differs from its text before the plant, kept in $backup" >&2
    status=3
  fi
  echo "[plant] dotnet listeners: $(lsof -nP -iTCP -sTCP:LISTEN 2>/dev/null | grep -ci dotnet || true)"
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
build_assets >/dev/null || refuse "the assets did not build with the defect planted; the build refuses it, so no test is needed for it."

stamp="plant-$(date +%Y%m%d-%H%M%S)-$$"
trx="$results_dir/playwright-$stamp.trx"
mkdir -p "$results_dir"
log="$results_dir/plant-$stamp.log"
echo "[plant] planted in $file; running project $project, filter $filter"
ALIS_PLAYWRIGHT_RUN_STAMP="$stamp" scripts/playwright.sh --project "$project" --filter "$filter" >"$log" 2>&1 &
child=$!
run_status=0
wait "$child" || run_status=$?
child=""

[ -f "$trx" ] || refuse "no test ran (scripts/playwright.sh exit $run_status); its output is $log"
grep -E '^Total tests:' "$log" | sed 's/^/[plant] /' || true

failed="$(node scripts/trx-failed-tests.mjs "$trx" failed)"
passed="$(node scripts/trx-failed-tests.mjs "$trx" passed)"
named_failed="$(printf '%s\n' "$failed" | grep -E "\.$test_name\$" || true)"
named_passed="$(printf '%s\n' "$passed" | grep -E "\.$test_name\$" || true)"
named_count="$(printf '%s\n%s\n' "$named_failed" "$named_passed" | grep -c . || true)"
[ "$named_count" -eq 1 ] || refuse "the run holds $named_count tests named $test_name; the filter must run it once. Its output is $log"

if [ -z "$named_failed" ]; then
  echo "[plant] ESCAPED: $named_passed passed with the defect in place."
  exit 1
fi

echo "[plant] caught: $named_failed failed."
grep -A3 -E "^  Failed $test_name " "$log" | sed '1d; s/^/[plant]   /' || true
printf '%s\n' "$failed" | grep -vE "\.$test_name\$" | grep . | sed 's/^/[plant] also failed: /' || true
echo "[plant] passed: $(printf '%s\n' "$passed" | grep -c . || true) other test(s)."
