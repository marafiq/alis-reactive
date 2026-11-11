#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "$0")/.."

configuration="${CONFIGURATION:-Debug}"
hang_timeout="${PLAYWRIGHT_HANG_TIMEOUT:-10m}"
filter=""
shard=""
selected=""
print_filter=0
list_only=0
retry_failed=0
no_build=0

descriptors() {
  local found=0 descriptor
  for descriptor in tests/*/playwright.project.sh; do
    [ -f "$descriptor" ] || continue
    found=1
    printf '%s\n' "$descriptor"
  done
  if [ "$found" -eq 0 ]; then
    echo "[playwright:runner] ERROR: no tests/*/playwright.project.sh found." >&2
    exit 2
  fi
}

load_project() {
  unset -f playwright_shard_filter playwright_check_assets playwright_build_assets playwright_sources
  playwright_project=""
  playwright_shards=""
  source "$1"
  project_dir="$(dirname "$1")"
  local name
  name="$(basename "$project_dir")"
  project="$project_dir/$name.csproj"
  assembly="$project_dir/bin/$configuration/net10.0/$name.dll"
  results_dir="$project_dir/TestResults/observable"
  if [ -z "$playwright_project" ]; then
    echo "[playwright:runner] ERROR: $1 declares no playwright_project." >&2
    exit 2
  fi
}

descriptor_named() {
  local descriptor
  while IFS= read -r descriptor; do
    load_project "$descriptor"
    if [ "$playwright_project" = "$1" ]; then printf '%s' "$descriptor"; return 0; fi
  done < <(descriptors)
  echo "Unknown project '$1'. Known projects: $(list_projects | tr '\n' ' ')" >&2
  exit 2
}

descriptor_of_shard() {
  local descriptor name
  while IFS= read -r descriptor; do
    load_project "$descriptor"
    for name in $playwright_shards; do
      if [ "$name" = "$1" ]; then printf '%s' "$descriptor"; return 0; fi
    done
  done < <(descriptors)
  echo "Unknown shard '$1'. Known shards: $(list_shards | tr '\n' ' ')" >&2
  exit 2
}

list_projects() {
  local descriptor
  while IFS= read -r descriptor; do
    load_project "$descriptor"
    printf '%s\n' "$playwright_project"
  done < <(descriptors)
}

list_shards() {
  local descriptor names=""
  while IFS= read -r descriptor; do
    load_project "$descriptor"
    names="$names $playwright_shards"
  done < <(descriptors)
  local duplicate
  duplicate="$(printf '%s\n' $names | sort | uniq -d | head -1)"
  if [ -n "$duplicate" ]; then
    echo "[playwright:runner] ERROR: shard '$duplicate' is declared by two projects." >&2
    exit 2
  fi
  printf '%s\n' $names
}

check_asset_output_is_fresh() {
  local label="$1"
  local output="$2"
  shift 2

  if [ ! -f "$output" ]; then
    echo "[playwright:runner] ERROR: $label output '$output' does not exist." >&2
    echo "[playwright:runner] Build the project's assets before Playwright (poc/build.sh)." >&2
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
    echo "[playwright:runner] Build the project's assets before Playwright (poc/build.sh)." >&2
    exit 3
  fi
}

check_no_build_is_fresh() {
  if [ ! -f "$assembly" ]; then
    echo "[playwright:runner] ERROR: --no-build requested, but '$assembly' does not exist." >&2
    echo "[playwright:runner] Run scripts/playwright.sh without --no-build, or run dotnet build first." >&2
    exit 3
  fi

  local sources=(
    tests/Alis.Reactive.Playwright.TestHost
    "$project_dir"
  )
  local source
  while IFS= read -r source; do
    [ -n "$source" ] && sources+=("$source")
  done < <(playwright_sources)

  local output_dir built changed_dotnet="" changed
  output_dir="$(dirname "$assembly")"
  for source in "${sources[@]}"; do
    built="$output_dir/$(basename "$source").dll"
    [ -f "$built" ] || built="$assembly"
    changed="$(
      find "$source" \
        \( -path '*/bin' -o -path '*/bin/*' -o -path '*/obj' -o -path '*/obj/*' \) -prune -o \
        -type f \
        \( -name '*.cs' -o -name '*.cshtml' -o -name '*.csproj' -o -name '*.props' -o -name '*.targets' \) \
        -newer "$built" \
        -print \
        | head -20
    )"
    [ -n "$changed" ] && changed_dotnet+="$changed"$'\n'
  done

  if [ -n "$changed_dotnet" ]; then
    echo "[playwright:runner] ERROR: --no-build would run stale Playwright binaries." >&2
    echo "[playwright:runner] Source files are newer than their built assemblies in '$output_dir':" >&2
    printf '%s' "$changed_dotnet" | head -20 >&2
    echo "[playwright:runner] Run scripts/playwright.sh without --no-build, or rebuild first." >&2
    exit 3
  fi
}

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
  cat <<USAGE
Usage:
  scripts/playwright.sh
  scripts/playwright.sh --project poc --filter "FullyQualifiedName~Properties"
  scripts/playwright.sh --filter "Name=test_one|Name=test_two"
  scripts/playwright.sh Properties
  scripts/playwright.sh --shard poc --no-build
  scripts/playwright.sh --shard poc --print-filter
  scripts/playwright.sh --shard poc --list --no-build

Projects (tests/*/playwright.project.sh): $(list_projects | tr '\n' ' ')
Shards: $(list_shards | tr '\n' ' ')

Options:
  --project <name>      Run one project. Without --project or --shard, every project runs,
                        each against its own app, and a filter applies to each.
  --filter <expr>       VSTest filter expression to pass through unchanged.
  --shard <name>        Run one CI shard: a fixed partition of a project's tests, declared in its
                        playwright.project.sh. Selects that project. Cannot be
                        combined with --project or --filter.
  --list-shards         Print every project's CI shard names, one per line, and exit.
  --list-projects       Print the project names, one per line, and exit.
  --print-filter        Print the resolved VSTest filter and exit without running.
  --list                Print the fully qualified names the filter or shard selects, one
                        per line, without running them: dotnet test --list-tests for the
                        discovered set (it ignores --filter), then the same filter grammar
                        applied by scripts/vstest-filter.mjs. A project's shard lists together
                        must equal its full list, each test exactly once.
  --retry-failed <n>    After a red run, re-run only the failed tests, up to n more
                        times (default 0). A test that then passes is reported as flaky:
                        [playwright:flaky] lines and TestResults/observable/flaky-<stamp>.txt.
  --no-build            Skip the project build and run the existing binaries.
                        Fails if C# or Razor sources are newer than the test DLL.
  --configuration <cfg> Build/test configuration. Defaults to Debug.
  --hang-timeout <dur>  Per-test blame-hang timeout. Defaults to 10m.
  -h, --help            Show this help.

During a run, look for:
  [playwright:start] ... Fully.Qualified.Test.Name
  [playwright:end]   ... Status ... Fully.Qualified.Test.Name

If a run appears stuck, the most recent [playwright:start] line is the active
test. Logs, TRX, and VSTest diagnostics are written under each project's
  tests/<Project>/TestResults/observable/
USAGE
}

option_value() {
  if [ "$#" -lt 2 ]; then
    echo "$1 requires a value." >&2
    exit 2
  fi
  printf '%s' "$2"
}

while [ "$#" -gt 0 ]; do
  case "$1" in
    --project) selected="$(option_value "$@")"; shift 2 ;;
    --project=*) selected="${1#--project=}"; shift ;;
    --filter) filter="$(option_value "$@")"; shift 2 ;;
    --filter=*) filter="${1#--filter=}"; shift ;;
    --shard) shard="$(option_value "$@")"; shift 2 ;;
    --shard=*) shard="${1#--shard=}"; shift ;;
    --list-shards) list_shards; exit 0 ;;
    --list-projects) list_projects; exit 0 ;;
    --print-filter) print_filter=1; shift ;;
    --list) list_only=1; shift ;;
    --retry-failed) retry_failed="$(option_value "$@")"; shift 2 ;;
    --retry-failed=*) retry_failed="${1#--retry-failed=}"; shift ;;
    --no-build) no_build=1; shift ;;
    --configuration) configuration="$(option_value "$@")"; shift 2 ;;
    --configuration=*) configuration="${1#--configuration=}"; shift ;;
    --hang-timeout) hang_timeout="$(option_value "$@")"; shift 2 ;;
    --hang-timeout=*) hang_timeout="${1#--hang-timeout=}"; shift ;;
    -h|--help) usage; exit 0 ;;
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

case "$retry_failed" in
  ''|*[!0-9]*)
    echo "--retry-failed expects a non-negative integer, got '$retry_failed'." >&2
    exit 2
    ;;
esac

projects_to_run=()
if [ -n "$shard" ]; then
  if [ -n "$filter" ] || [ -n "$selected" ]; then
    echo "--shard cannot be combined with --project, --filter or a positional filter." >&2
    exit 2
  fi
  projects_to_run+=("$(descriptor_of_shard "$shard")")
elif [ -n "$selected" ]; then
  projects_to_run+=("$(descriptor_named "$selected")")
else
  while IFS= read -r descriptor; do
    projects_to_run+=("$descriptor")
  done < <(descriptors)
fi

project_filter() {
  if [ -n "$shard" ]; then
    playwright_shard_filter "$shard"
  else
    printf '%s' "$filter"
  fi
}

if [ "$print_filter" -eq 1 ]; then
  for descriptor in "${projects_to_run[@]}"; do
    load_project "$descriptor"
    resolved="$(project_filter)"
    printf '%s\n' "${resolved:-<full suite>}"
  done
  exit 0
fi

list_project() {
  local resolved discovered
  resolved="$(project_filter)"
  if [ "$no_build" -eq 0 ]; then
    echo "[playwright:runner] building $playwright_project for listing (browser assets not needed)" >&2
    dotnet build "$project" -c "$configuration" -p:BuildReactiveBrowserAssets=false >&2
  else
    check_no_build_is_fresh
  fi
  local list_cmd=(
    dotnet test "$project"
    -c "$configuration"
    -p:BuildReactiveBrowserAssets=false
    --no-build
    --nologo
    --list-tests
    -- NUnit.DisplayName=FullName
  )
  discovered="$("${list_cmd[@]}" | awk '/^The following Tests are available:/ { listing = 1; next } listing && /^    / { sub(/^    /, ""); print }')"
  if [ -n "$resolved" ]; then
    printf '%s\n' "$discovered" | node scripts/vstest-filter.mjs "$resolved" | sort
  else
    printf '%s\n' "$discovered" | sort
  fi
}

if [ "$list_only" -eq 1 ]; then
  for descriptor in "${projects_to_run[@]}"; do
    load_project "$descriptor"
    list_project
  done
  exit 0
fi

stop_apps_whose_test_run_ended() {
  local pid args url host
  while read -r pid args; do
    [[ "$args" =~ --urls[[:space:]]+(http://localhost:[0-9]+)[[:space:]]+--alis-test-host[[:space:]]+([0-9]+) ]] || continue
    url="${BASH_REMATCH[1]}"
    host="${BASH_REMATCH[2]}"
    kill -0 "$host" 2>/dev/null && continue
    echo "[playwright:runner] stopping an app its test run left behind: pid $pid at $url (test host $host has exited)"
    kill "$pid" 2>/dev/null || continue
    for _ in 1 2 3 4 5 6 7 8 9 10; do
      kill -0 "$pid" 2>/dev/null || break
      sleep 0.5
    done
    if kill -0 "$pid" 2>/dev/null; then kill -9 "$pid" 2>/dev/null || true; fi
  done < <(ps -eo pid=,args=)
}

stop_apps_whose_test_run_ended
trap stop_apps_whose_test_run_ended EXIT

attempt_trx=""

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

run_project() {
  local project_filter_expr status
  project_filter_expr="$(project_filter)"
  mkdir -p "$results_dir"
  stamp="${ALIS_PLAYWRIGHT_RUN_STAMP:-$(date +%Y%m%d-%H%M%S)}"
  if [ -n "$shard" ]; then
    stamp="$stamp.$shard"
  fi

  echo "[playwright:runner] project=$playwright_project ($project)"
  echo "[playwright:runner] configuration=$configuration"
  echo "[playwright:runner] shard=${shard:-<none>}"
  echo "[playwright:runner] filter=${project_filter_expr:-<full suite>}"
  echo "[playwright:runner] hang-timeout=$hang_timeout"
  echo "[playwright:runner] retry-failed=$retry_failed"

  playwright_check_assets

  if [ "$no_build" -eq 0 ]; then
    echo "[playwright:runner] building test project"
    if ! dotnet build "$project" -c "$configuration"; then
      echo "[playwright:runner] ERROR: the build of $project failed; no test ran." >&2
      return 4
    fi
  else
    echo "[playwright:runner] skipping build (--no-build)"
    check_no_build_is_fresh
  fi
  echo "[playwright:runner] disabling VSTest-time browser asset rebuild"

  if run_attempt "" "$project_filter_expr"; then status=0; else status=$?; fi

  if [ "${GITHUB_ACTIONS:-}" = "true" ]; then
    node scripts/trx-failed-tests.mjs "$attempt_trx" summary
  fi

  local retries_done=0 failed_trx failed_tests failed_count fqn flaky_path
  local flaky=()
  local flaky_annotations=""
  while [ "$status" -ne 0 ] && [ "$retries_done" -lt "$retry_failed" ]; do
    failed_trx="$attempt_trx"
    failed_tests="$(node scripts/trx-failed-tests.mjs "$failed_trx")"
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
    flaky_annotations+="$(node scripts/trx-failed-tests.mjs "$failed_trx" flaky "$attempt_trx")"$'\n'
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
        echo "Two red runs in a row is a regression or a quarantine candidate."
      } >> "$GITHUB_STEP_SUMMARY"
    fi
    if [ "${GITHUB_ACTIONS:-}" = "true" ]; then
      printf '%s' "$flaky_annotations" | sed '/^$/d'
    fi
  fi

  if [ "$status" -ne 0 ] && [ "${GITHUB_ACTIONS:-}" = "true" ]; then
    node scripts/trx-failed-tests.mjs "$attempt_trx" annotations
  fi

  return "$status"
}

overall=0
for descriptor in "${projects_to_run[@]}"; do
  load_project "$descriptor"
  if run_project; then code=0; else code=$?; fi
  if [ "$overall" -eq 0 ] && [ "$code" -ne 0 ]; then overall="$code"; fi
done
exit "$overall"
