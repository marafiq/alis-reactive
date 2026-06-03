#!/usr/bin/env bash
# Observable Playwright/NUnit runner.
#
# Use this instead of raw `dotnet test` for browser tests. The wrapper makes both
# filtered and full runs observable by printing the active filter, teeing live
# output to a log, writing TRX/diagnostic artifacts, and enabling blame-hang.
# Browser assets are built before dotnet test, never during the VSTest phase.
set -euo pipefail

cd "$(dirname "$0")/.."

project="tests/Alis.Reactive.PlaywrightTests/Alis.Reactive.PlaywrightTests.csproj"
results_dir="tests/Alis.Reactive.PlaywrightTests/TestResults/observable"
configuration="${CONFIGURATION:-Debug}"
hang_timeout="${PLAYWRIGHT_HANG_TIMEOUT:-10m}"
filter=""
no_build=0

assembly_path() {
  printf 'tests/Alis.Reactive.PlaywrightTests/bin/%s/net10.0/Alis.Reactive.PlaywrightTests.dll' "$configuration"
}

check_asset_output_is_fresh() {
  local label="$1"
  local output="$2"
  shift 2

  if [ ! -f "$output" ]; then
    echo "[playwright:runner] ERROR: $label output '$output' does not exist." >&2
    echo "[playwright:runner] Run npm run build:all before Playwright." >&2
    exit 3
  fi

  local changed
  changed="$(
    find "$@" \
      -type f \
      \( -name '*.ts' -o -name '*.tsx' -o -name '*.js' -o -name '*.mjs' -o -name '*.css' -o -name '*.json' \) \
      -newer "$output" \
      -print \
      | head -20
  )"

  if [ -n "$changed" ]; then
    echo "[playwright:runner] ERROR: $label output is stale." >&2
    echo "[playwright:runner] Sources newer than '$output':" >&2
    printf '%s\n' "$changed" >&2
    echo "[playwright:runner] Run npm run build:all before Playwright." >&2
    exit 3
  fi
}

check_browser_assets_are_fresh() {
  check_asset_output_is_fresh \
    "runtime bundle" \
    "Alis.Reactive.Assets/dist/scripts/alis-reactive.dev.js" \
    Alis.Reactive.Assets/runtime \
    Alis.Reactive.Assets/esbuild.config.mjs \
    Alis.Reactive.Assets/package.json \
    Alis.Reactive.Assets/tsconfig.json

  check_asset_output_is_fresh \
    "design-system CSS" \
    "Alis.Reactive.Assets/dist/css/design-system.dev.css" \
    Alis.Reactive.Assets/design-system \
    Alis.Reactive.Assets/vite.design-system.config.ts \
    Alis.Reactive.Assets/package.json

  check_asset_output_is_fresh \
    "Syncfusion CSS" \
    "Alis.Reactive.Assets/dist/css/syncfusion.dev.css" \
    Alis.Reactive.Assets/fusion \
    Alis.Reactive.Assets/vite.fusion.config.ts \
    Alis.Reactive.Assets/package.json

  check_asset_output_is_fresh \
    "sandbox plugin bundle" \
    "Alis.Reactive.SandboxApp/wwwroot/js/sandbox-plugins.js" \
    Alis.Reactive.SandboxApp/Scripts \
    Alis.Reactive.SandboxApp/esbuild.config.mjs \
    Alis.Reactive.SandboxApp/package.json

  check_asset_output_is_fresh \
    "sandbox CSS" \
    "Alis.Reactive.SandboxApp/wwwroot/css/sandbox.css" \
    Alis.Reactive.SandboxApp/Styles \
    Alis.Reactive.SandboxApp/build-css.mjs \
    Alis.Reactive.SandboxApp/vite.config.ts \
    Alis.Reactive.SandboxApp/package.json
}

check_no_build_is_fresh() {
  local assembly
  assembly="$(assembly_path)"

  if [ ! -f "$assembly" ]; then
    echo "[playwright:runner] ERROR: --no-build requested, but '$assembly' does not exist." >&2
    echo "[playwright:runner] Run scripts/playwright.sh without --no-build, or run dotnet build first." >&2
    exit 3
  fi

  local changed_dotnet
  changed_dotnet="$(
    find \
      Alis.Reactive \
      Alis.Reactive.Analyzers \
      Alis.Reactive.DesignSystem \
      Alis.Reactive.FluentValidator \
      Alis.Reactive.Fusion \
      Alis.Reactive.Native \
      Alis.Reactive.NativeTagHelpers \
      Alis.Reactive.SandboxApp \
      tests/Alis.Reactive.Playwright.Extensions \
      tests/Alis.Reactive.PlaywrightTests \
      -type f \
      \( -name '*.cs' -o -name '*.cshtml' -o -name '*.csproj' -o -name '*.props' -o -name '*.targets' \) \
      -newer "$assembly" \
      -print \
      | head -20
  )"

  if [ -n "$changed_dotnet" ]; then
    echo "[playwright:runner] ERROR: --no-build would run stale Playwright binaries." >&2
    echo "[playwright:runner] Source files are newer than '$assembly':" >&2
    printf '%s\n' "$changed_dotnet" >&2
    echo "[playwright:runner] Run scripts/playwright.sh without --no-build, or rebuild first." >&2
    exit 3
  fi
}

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
                        Use after npm run build:all + dotnet build.
                        Fails if C# or Razor sources are newer than the test DLL.
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

check_browser_assets_are_fresh

if [ "$no_build" -eq 0 ]; then
  echo "[playwright:runner] building test project"
  dotnet build "$project" -c "$configuration"
else
  echo "[playwright:runner] skipping build (--no-build)"
  check_no_build_is_fresh
fi
echo "[playwright:runner] disabling VSTest-time browser asset rebuild"

cmd=(
  dotnet test "$project"
  -c "$configuration"
  -p:BuildReactiveBrowserAssets=false
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
