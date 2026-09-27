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
shard=""
print_filter=0
list_only=0
retry_failed=0
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
      \( -path '*/bin' -o -path '*/bin/*' -o -path '*/obj' -o -path '*/obj/*' \) -prune -o \
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

# The CI shard partition (docs/CI.md, "Sharding"). VSTest filters have no wildcards, so Fusion
# component namespaces are split by their first letter: `~.Components.Fusion.A` is a contains
# match on the namespace segment, and every letter is listed so a new component always lands in
# exactly one shard. The last shard is the complement of the others, so a new top-level namespace
# can never fall outside every shard.
fusion_letters() {
  local expr="" letter
  for letter in "$@"; do
    expr="${expr:+$expr|}FullyQualifiedName~.Components.Fusion.$letter"
  done
  printf '%s' "$expr"
}

# The CI shards, in matrix order. .github/workflows/gate.yml lists the same names (scripts/test.sh
# fails when the two lists differ), and every name has an arm in shard_filter below.
shards="fusion-a-f-and-core fusion-g-l-and-http fusion-m-z components-and-validation conditions-patterns-and-rest"

shard_filter() {
  case "$1" in
    fusion-a-f-and-core)
      printf '(%s)|FullyQualifiedName~.CoreBehaviors.' "$(fusion_letters A B C D E F)"
      ;;
    fusion-g-l-and-http)
      printf '(%s)|FullyQualifiedName~.HttpPipeline.' "$(fusion_letters G H I J K L)"
      ;;
    fusion-m-z)
      printf '(%s)' "$(fusion_letters M N O P Q R S T U V W X Y Z)"
      ;;
    components-and-validation)
      printf '(FullyQualifiedName~.Components.&FullyQualifiedName!~.Components.Fusion.)|FullyQualifiedName~.Validation.'
      ;;
    conditions-patterns-and-rest)
      printf 'FullyQualifiedName!~.Components.&FullyQualifiedName!~.Validation.&FullyQualifiedName!~.CoreBehaviors.&FullyQualifiedName!~.HttpPipeline.'
      ;;
    *)
      echo "Unknown shard '$1'. Known shards: $shards." >&2
      exit 2
      ;;
  esac
}

# Builds the filter that re-runs exactly the tests listed (one FQN per line). A parametrised test
# carries its arguments after "(", which VSTest cannot match exactly, so it falls back to a
# contains match on the method — re-running every row of that method.
retry_filter_for() {
  local expr="" fqn
  while IFS= read -r fqn; do
    [ -z "$fqn" ] && continue
    case "$fqn" in
      *"("*) expr="${expr:+$expr|}FullyQualifiedName~${fqn%%(*}" ;;
      *)     expr="${expr:+$expr|}FullyQualifiedName=$fqn" ;;
    esac
  done <<< "$1"
  printf '%s' "$expr"
}

usage() {
  cat <<'USAGE'
Usage:
  scripts/playwright.sh
  scripts/playwright.sh --filter "FullyQualifiedName~Components.Fusion.Grid"
  scripts/playwright.sh --filter "Name=test_one|Name=test_two"
  scripts/playwright.sh Components.Fusion.Grid
  scripts/playwright.sh --shard fusion-m-z --no-build
  scripts/playwright.sh --shard fusion-m-z --print-filter
  scripts/playwright.sh --shard fusion-m-z --list --no-build

Options:
  --filter <expr>       VSTest filter expression to pass through unchanged.
  --shard <name>        Run one CI shard: a fixed, documented partition of the suite
                        (docs/CI.md, "Sharding"). Names: fusion-a-f-and-core,
                        fusion-g-l-and-http, fusion-m-z, components-and-validation,
                        conditions-patterns-and-rest. Cannot be combined with --filter.
  --list-shards         Print the CI shard names, one per line, and exit.
  --print-filter        Print the resolved VSTest filter and exit without running.
  --list                Print the fully qualified names the filter or shard selects, one
                        per line, without running them: dotnet test --list-tests for the
                        discovered set (it ignores --filter), then the same filter grammar
                        applied by scripts/vstest-filter.mjs. The five shards' lists
                        together must equal the full list, each test exactly once
                        (docs/CI.md, "Sharding").
  --retry-failed <n>    After a red run, re-run only the failed tests, up to n more
                        times (default 0). A test that then passes is reported as flaky:
                        [playwright:flaky] lines and TestResults/observable/flaky-<stamp>.txt.
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
    --shard)
      if [ "$#" -lt 2 ]; then
        echo "--shard requires a value." >&2
        exit 2
      fi
      shard="$2"
      shift 2
      ;;
    --shard=*)
      shard="${1#--shard=}"
      shift
      ;;
    --list-shards)
      printf '%s\n' $shards
      exit 0
      ;;
    --print-filter)
      print_filter=1
      shift
      ;;
    --list)
      list_only=1
      shift
      ;;
    --retry-failed)
      if [ "$#" -lt 2 ]; then
        echo "--retry-failed requires a value." >&2
        exit 2
      fi
      retry_failed="$2"
      shift 2
      ;;
    --retry-failed=*)
      retry_failed="${1#--retry-failed=}"
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

if [ -n "$shard" ]; then
  if [ -n "$filter" ]; then
    echo "--shard cannot be combined with --filter or a positional filter." >&2
    exit 2
  fi
  filter="$(shard_filter "$shard")"
fi

case "$retry_failed" in
  ''|*[!0-9]*)
    echo "--retry-failed expects a non-negative integer, got '$retry_failed'." >&2
    exit 2
    ;;
esac

if [ "$print_filter" -eq 1 ]; then
  printf '%s\n' "${filter:-<full suite>}"
  exit 0
fi

# Listing needs the test assembly but no browser assets and no sandbox, so it builds without the
# npm step and skips the asset freshness checks.
if [ "$list_only" -eq 1 ]; then
  if [ "$no_build" -eq 0 ]; then
    echo "[playwright:runner] building test project for listing (browser assets not needed)" >&2
    dotnet build "$project" -c "$configuration" -p:BuildReactiveBrowserAssets=false >&2
  else
    check_no_build_is_fresh
  fi
  # The NUnit adapter's default display name is the bare method name; FullName makes each line a
  # fully qualified, unique test name. dotnet test --list-tests lists every discovered test and
  # ignores --filter, so the shard or --filter expression is applied afterwards with the same
  # grammar (scripts/vstest-filter.mjs).
  list_cmd=(
    dotnet test "$project"
    -c "$configuration"
    -p:BuildReactiveBrowserAssets=false
    --no-build
    --nologo
    --list-tests
    -- NUnit.DisplayName=FullName
  )
  discovered="$("${list_cmd[@]}" | awk '/^The following Tests are available:/ { listing = 1; next } listing && /^    / { sub(/^    /, ""); print }')"
  if [ -n "$filter" ]; then
    printf '%s\n' "$discovered" | node scripts/vstest-filter.mjs "$filter" | sort
  else
    printf '%s\n' "$discovered" | sort
  fi
  exit 0
fi

mkdir -p "$results_dir"
# The stamp names this run's log, TRX and diagnostics. scripts/test.sh --parallel hands one stamp to
# all its shards (ALIS_PLAYWRIGHT_RUN_STAMP) so their files read as one run; each shard appends its
# name after a dot, so concurrent shards never share a file name.
stamp="${ALIS_PLAYWRIGHT_RUN_STAMP:-$(date +%Y%m%d-%H%M%S)}"
if [ -n "$shard" ]; then
  stamp="$stamp.$shard"
fi

echo "[playwright:runner] project=$project"
echo "[playwright:runner] configuration=$configuration"
echo "[playwright:runner] shard=${shard:-<none>}"
echo "[playwright:runner] filter=${filter:-<full suite>}"
echo "[playwright:runner] hang-timeout=$hang_timeout"
echo "[playwright:runner] retry-failed=$retry_failed"

check_browser_assets_are_fresh

if [ "$no_build" -eq 0 ]; then
  echo "[playwright:runner] building test project"
  dotnet build "$project" -c "$configuration"
else
  echo "[playwright:runner] skipping build (--no-build)"
  check_no_build_is_fresh
fi
echo "[playwright:runner] disabling VSTest-time browser asset rebuild"

attempt_trx=""

# One dotnet test invocation. $1 suffixes the artifact names ("" for the first attempt, then
# "-retry1", "-retry2", ...), $2 is the VSTest filter. Returns dotnet test's exit code.
run_attempt() {
  local suffix="$1"
  local attempt_filter="$2"
  local log_path="$results_dir/playwright-$stamp$suffix.log"
  local diag_path="$results_dir/vstest-$stamp$suffix.diag.log"
  local trx_name="playwright-$stamp$suffix.trx"
  attempt_trx="$results_dir/$trx_name"

  echo "[playwright:runner] log=$log_path"
  echo "[playwright:runner] trx=$attempt_trx"
  echo "[playwright:runner] diag=$diag_path"

  local cmd=(
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

  if [ -n "$attempt_filter" ]; then
    cmd+=(--filter "$attempt_filter")
  fi

  printf '[playwright:runner] command='
  printf '%q ' "${cmd[@]}"
  printf '\n'

  local status
  set +e
  "${cmd[@]}" 2>&1 | tee "$log_path"
  status="${PIPESTATUS[0]}"
  set -e

  echo "[playwright:runner] exit-code=$status"
  echo "[playwright:runner] log=$log_path"
  echo "[playwright:runner] trx=$attempt_trx"
  echo "[playwright:runner] diag=$diag_path"
  return "$status"
}

if run_attempt "" "$filter"; then status=0; else status=$?; fi

# Re-run only what failed, up to --retry-failed times. Every attempt keeps its own log, TRX and
# diagnostics; the final exit code is the last attempt's. Tests that fail and then pass are the
# flaky list — visible in the output, in flaky-<stamp>.txt, and in the GitHub job summary.
retries_done=0
flaky=()
while [ "$status" -ne 0 ] && [ "$retries_done" -lt "$retry_failed" ]; do
  failed_tests="$(node scripts/trx-failed-tests.mjs "$attempt_trx")"
  if [ -z "$failed_tests" ]; then
    echo "[playwright:retry] no failed test recorded in $attempt_trx (crash or hang?); not retrying"
    break
  fi
  retries_done=$((retries_done + 1))
  failed_count="$(printf '%s\n' "$failed_tests" | wc -l | tr -d ' ')"
  echo "[playwright:retry] attempt $((retries_done + 1)) of $((retry_failed + 1)): re-running $failed_count failed test(s)"
  printf '%s\n' "$failed_tests" | sed 's/^/[playwright:retry]   /'
  if run_attempt "-retry$retries_done" "$(retry_filter_for "$failed_tests")"; then status=0; else status=$?; fi
  while IFS= read -r fqn; do
    [ -n "$fqn" ] && flaky+=("$fqn")
  done < <(node scripts/trx-failed-tests.mjs "$attempt_trx" passed)
done

if [ "${#flaky[@]}" -gt 0 ]; then
  flaky_path="$results_dir/flaky-$stamp.txt"
  printf '%s\n' "${flaky[@]}" | sort -u > "$flaky_path"
  echo "[playwright:flaky] $(wc -l < "$flaky_path" | tr -d ' ') test(s) failed, then passed on re-run (list: $flaky_path):"
  sed 's/^/[playwright:flaky]   /' "$flaky_path"
  if [ -n "${GITHUB_STEP_SUMMARY:-}" ]; then
    {
      echo "### Flaky on this run: failed, then passed on re-run"
      echo
      sed 's/^/- `/; s/$/`/' "$flaky_path"
      echo
      echo "Two red runs in a row is a regression or a quarantine candidate: docs/CI.md, \"Flaky tests\"."
    } >> "$GITHUB_STEP_SUMMARY"
  fi
fi

exit "$status"
