#!/usr/bin/env bash
# Observable Playwright/NUnit runner.
#
# Use this instead of raw `dotnet test` for browser tests. The wrapper makes both
# filtered and full runs observable by printing the active filter, teeing live
# output to a log, writing TRX/diagnostic artifacts, and enabling blame-hang.
set -euo pipefail

cd "$(dirname "$0")/.."

project="tests/Alis.Reactive.PlaywrightTests/Alis.Reactive.PlaywrightTests.csproj"
results_dir="tests/Alis.Reactive.PlaywrightTests/TestResults/observable"
configuration="${CONFIGURATION:-Debug}"
hang_timeout="${PLAYWRIGHT_HANG_TIMEOUT:-10m}"
filter=""
no_build=0

usage() {
  cat <<'USAGE'
Usage:
  scripts/playwright.sh
  scripts/playwright.sh --filter "FullyQualifiedName~Components.Fusion.Grid"
  scripts/playwright.sh --filter "Name=test_one|Name=test_two"
  scripts/playwright.sh Components.Fusion.Grid

Options:
  --filter <expr>       VSTest filter expression to pass through unchanged.
  --no-build            Skip the project build and run the existing binaries.
  --configuration <cfg> Build/test configuration. Defaults to Debug.
  --hang-timeout <dur>  Per-test blame-hang timeout. Defaults to 10m.
  -h, --help            Show this help.

During a run, look for:
  [playwright:start] ... Fully.Qualified.Test.Name
  [playwright:end]   ... Status ... Fully.Qualified.Test.Name

If a run appears stuck, the most recent [playwright:start] line is the active
test. Logs, TRX, and VSTest diagnostics are written under:
  tests/Alis.Reactive.PlaywrightTests/TestResults/observable/
USAGE
}

while [ "$#" -gt 0 ]; do
  case "$1" in
    --filter)
      if [ "$#" -lt 2 ]; then
        echo "--filter requires a value." >&2
        exit 2
      fi
      filter="$2"
      shift 2
      ;;
    --filter=*)
      filter="${1#--filter=}"
      shift
      ;;
    --no-build)
      no_build=1
      shift
      ;;
    --configuration)
      if [ "$#" -lt 2 ]; then
        echo "--configuration requires a value." >&2
        exit 2
      fi
      configuration="$2"
      shift 2
      ;;
    --configuration=*)
      configuration="${1#--configuration=}"
      shift
      ;;
    --hang-timeout)
      if [ "$#" -lt 2 ]; then
        echo "--hang-timeout requires a value." >&2
        exit 2
      fi
      hang_timeout="$2"
      shift 2
      ;;
    --hang-timeout=*)
      hang_timeout="${1#--hang-timeout=}"
      shift
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      if [ -n "$filter" ]; then
        echo "Unexpected argument '$1'. Use --filter for complex expressions." >&2
        exit 2
      fi
      filter="FullyQualifiedName~$1"
      shift
      ;;
  esac
done

mkdir -p "$results_dir"
stamp="$(date +%Y%m%d-%H%M%S)"
log_path="$results_dir/playwright-$stamp.log"
diag_path="$results_dir/vstest-$stamp.diag.log"
trx_name="playwright-$stamp.trx"

echo "[playwright:runner] project=$project"
echo "[playwright:runner] configuration=$configuration"
echo "[playwright:runner] filter=${filter:-<full suite>}"
echo "[playwright:runner] hang-timeout=$hang_timeout"
echo "[playwright:runner] log=$log_path"
echo "[playwright:runner] trx=$results_dir/$trx_name"
echo "[playwright:runner] diag=$diag_path"

if [ "$no_build" -eq 0 ]; then
  echo "[playwright:runner] building test project"
  dotnet build "$project" -c "$configuration"
else
  echo "[playwright:runner] skipping build (--no-build)"
fi

cmd=(
  dotnet test "$project"
  -c "$configuration"
  --no-build
  --nologo
  --logger "console;verbosity=detailed"
  --logger "trx;LogFileName=$trx_name"
  --results-directory "$results_dir"
  --diag "$diag_path"
  --blame-hang
  --blame-hang-dump-type none
  --blame-hang-timeout "$hang_timeout"
)

if [ -n "$filter" ]; then
  cmd+=(--filter "$filter")
fi

printf '[playwright:runner] command='
printf '%q ' "${cmd[@]}"
printf '\n'

set +e
"${cmd[@]}" 2>&1 | tee "$log_path"
status="${PIPESTATUS[0]}"
set -e

echo "[playwright:runner] exit-code=$status"
echo "[playwright:runner] log=$log_path"
echo "[playwright:runner] trx=$results_dir/$trx_name"
echo "[playwright:runner] diag=$diag_path"

exit "$status"
