#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."

name="Alis.Reactive.Conformance.PlaywrightTests"
project="tests/$name"
app="Alis.Reactive.Conformance.App"
assembly="$project/bin/Release/net10.0/$name.dll"
budget="$project/Harness/measured.json"
weights="$project/shard-weights.json"
measured_runs=20
own_versions="0.0.0-playwright"

usage() {
  cat <<'USAGE'
Usage:
  scripts/playwright.sh <selector> [--host <name>]
  scripts/playwright.sh --pool <name> [--shard <i>/<k>]
  scripts/playwright.sh --measure <selector>
  scripts/playwright.sh --measure-parallelism <selector>

Builds the browser tests and the conformance app, runs the tests one selector names, and reads the run:
  [playwright:result] <selection> <i>/<k>: N selected, N ran, N passed, 0 failed, 0 over budget, slowest <s>s <test>
The run fails when it wrote no TRX, selected no test, ran other than it selected, failed a test, ran a test over
the budget, or left a server alive. It retries nothing.

Selectors, one a run:
  --all                 every test the project holds
  --unit U<n>           the tests the spec lands in that unit
  --through U<n>        the tests the spec lands in that unit and in every unit before it
  --feature M<n>.F<n>   the tests of that feature
  --screen <id>         the tests app.json places on that screen
  --test <constant>     one test, by its promise constant as a test writes it after Promises. (M3_F1.X8_27_2)
  --pool <name>         one pool of the tests the project holds: core, hosts or fusion
  <name>                a folder or a fixture of the test project, by its name (RequestsAndResponses)

Options:
  --host <name>   Take that host alone. Without it a run takes the hosts the conformance app serves on this machine.
                  The net48 and consumerNet48 hosts run under IIS Express on Windows. Under a fresh app's host
                  the run builds that app on the packs and starts its own
                  server: with no PACKAGE_VERSION set it packs at a version of its own first (scripts/pack.sh), so
                  what the shipped projects hold reaches the app's page; with it set it takes the packs in nupkgs/.
                  Before it builds a fresh app it removes every version restored in the app's obj/packages/ but
                  the one it builds on, and a run that packed its own version removes those packs and their
                  restored folders when it ends, whatever its exit code.
  --shard <i>/<k> Take the i-th of a pool's k shards, its units placed longest first by the weights --measure wrote.
  --measure       Run the selection 20 times, each green, and write to the harness's measured.json the budget,
                  four times the slowest test, and the action wait and the navigation wait, each four times the
                  slowest step of its kind, and to shard-weights.json the seconds each shard unit took in a run.
                  A measured run sets no measured wait and records every step. A selection of Fusion fixtures'
                  tests alone writes the same fields in measured.json's fusion block, which a Fusion fixture reads;
                  a selection that mixes them with other tests is refused before it runs.
  --measure-parallelism
                  Run the selection 20 times at each of 1, 2, 4 and 8 tests at once, each a measured run, and write
                  to measured.json the count a run takes: the quickest of 2, 4 and 8 whose green runs took at most
                  one part in 1.6 of the time at 1, else 1. A budget is measured again at a count that changed.
  -h, --help      Show this help.

A run's files are under tests/Alis.Reactive.Conformance.PlaywrightTests/TestResults/<its stamp>/.
USAGE
}

refuse() {
  echo "[playwright:runner] $1" >&2
  exit 2
}

selector=()
selectors=0
host=()
shard=()
measure=0
parallelism=0
while [ "$#" -gt 0 ]; do
  case "$1" in
    --all) selector=("$1"); selectors=$((selectors + 1)); shift ;;
    --unit|--through|--feature|--screen|--test|--pool)
      [ "$#" -ge 2 ] || refuse "$1 takes a value."
      selector=("$1" "$2"); selectors=$((selectors + 1)); shift 2 ;;
    --host)
      [ "$#" -ge 2 ] || refuse "--host takes a value."
      host=("$1" "$2"); shift 2 ;;
    --shard)
      [ "$#" -ge 2 ] || refuse "--shard takes a value."
      shard=("$1" "$2"); shift 2 ;;
    --measure) measure=1; shift ;;
    --measure-parallelism) parallelism=1; shift ;;
    -h|--help) usage; exit 0 ;;
    -*) refuse "'$1' is no option; --help lists them." ;;
    *) selector=("$1"); selectors=$((selectors + 1)); shift ;;
  esac
done
[ "$selectors" -eq 1 ] || refuse "a run takes one selector, and $selectors were given; --help lists them."
[ "$((measure + parallelism))" -le 1 ] || refuse "--measure and --measure-parallelism are two measurements; a run takes one."

stop_servers_whose_test_process_ended() {
  local pid args test_process stopped=0
  while read -r pid args; do
    [[ "$args" == *"$1"* ]] || continue
    [[ "$args" =~ --test-host[[:space:]]+([0-9]+) ]] || continue
    test_process="${BASH_REMATCH[1]}"
    kill -0 "$test_process" 2>/dev/null && continue
    echo "[playwright:runner] $2: pid $pid, $1, whose test process $test_process has ended" >&2
    kill -9 "$pid" 2>/dev/null || true
    stopped=$((stopped + 1))
  done < <(ps -eo pid=,args=)
  printf '%s' "$stopped"
}

run="$project/TestResults/$(date +%Y%m%d-%H%M%S)-$$"
mkdir -p "$run"

echo "[playwright:runner] building the browser bundle, then $project"
if ! node build.mjs || ! dotnet build "$project" -c Release --nologo; then
  echo "[playwright:runner] the build failed; no test ran." >&2
  exit 4
fi

dotnet run scripts/list-tests.cs -p:RestorePackagesWithLockFile=false -- "$assembly" > "$run/tests.tsv"
python3 spec/tools/landings.py > "$run/landings.tsv"
filter="$(node scripts/playwright-select.mjs --spec "$run/landings.tsv" --assembly "$run/tests.tsv" --weights "$weights" --out "$run/selection.json" \
  "${selector[@]}" ${host[@]+"${host[@]}"} ${shard[@]+"${shard[@]}"})"
echo "[playwright:runner] filter: ${filter:-every test}"

fresh_apps=()
while IFS= read -r fresh_app; do fresh_apps+=("$fresh_app"); done < <(
  node -e 'for (const app of JSON.parse(require("node:fs").readFileSync(process.argv[1], "utf8")).freshApps) console.log(app);' "$run/selection.json")

ALIS_PREPARE_MOVE_IN="$(node -p 'JSON.parse(require("node:fs").readFileSync(process.argv[1], "utf8")).prepareMoveIn' "$run/selection.json")"
export ALIS_PREPARE_MOVE_IN
selected_host="${host[1]:-default}"
packed_apps=(${fresh_apps[@]+"${fresh_apps[@]}"})
case "$selected_host" in
  net48|consumerNet48)
    [ "${OS:-}" = Windows_NT ] || refuse "$selected_host needs IIS Express on Windows."
    if [ "$selected_host" = net48 ]; then packed_apps+=("tests/$app.Net48"); fi ;;
esac

remove_every_restored_version_but_the_runs() {
  local version
  for version in "$1"/obj/packages/alisreactive*/*; do
    if [ -d "$version" ] && [ "$(basename "$version")" != "$PACKAGE_VERSION" ]; then rm -rf "$version"; fi
  done
}

remove_the_packs_this_run_wrote() {
  local pack fresh_app version
  for pack in nupkgs/*."$PACKAGE_VERSION".nupkg nupkgs/*."$PACKAGE_VERSION".snupkg; do
    if [ -f "$pack" ]; then rm -f "$pack"; fi
  done
  for fresh_app in "${packed_apps[@]}"; do
    for version in "$fresh_app"/obj/packages/alisreactive*/"$PACKAGE_VERSION"; do
      if [ -d "$version" ]; then rm -rf "$version"; fi
    done
  done
}

build_failed() {
  echo "[playwright:runner] the build failed; no test ran." >&2
  exit 4
}

if [ "${#packed_apps[@]}" -gt 0 ]; then
  if [ -z "${PACKAGE_VERSION:-}" ]; then
    PACKAGE_VERSION="$own_versions.$(date -u +%Y%m%d%H%M%S)"
    export PACKAGE_VERSION
    trap remove_the_packs_this_run_wrote EXIT
    echo "[playwright:runner] packing at $PACKAGE_VERSION, which ${packed_apps[*]} builds on"
    scripts/pack.sh "$PACKAGE_VERSION" || build_failed
  else
    echo "[playwright:runner] ${packed_apps[*]} builds on the packs in nupkgs/ at $PACKAGE_VERSION"
  fi
  for packed_app in "${packed_apps[@]}"; do
    if [[ "$selected_host" = net48 || "$selected_host" = consumerNet48 ]] && [ "${ALIS_PREBUILT_MVC5:-}" = "$selected_host" ]; then
      echo "[playwright:runner] using the CI build of $packed_app at $PACKAGE_VERSION"
    else
      remove_every_restored_version_but_the_runs "$packed_app"
      if [ "$selected_host" = consumerNet48 ]; then
        pwsh -NoProfile -Command '
          $ErrorActionPreference = "Stop"
          $msbuild = $env:MSBUILD_EXE
          if (!$msbuild) { $msbuild = (Get-Command MSBuild.exe -ErrorAction Stop).Source }
          & $msbuild tests/Alis.Reactive.Consumer.Net48/Alis.Reactive.Consumer.Net48.csproj -restore -t:Rebuild -p:Configuration=Release -warnaserror
          exit $LASTEXITCODE' || build_failed
      else
        dotnet build "$packed_app" -c Release -warnaserror --no-incremental --nologo || build_failed
      fi
    fi
  done
fi

stop_every_server_whose_test_process_ended() {
  local fresh_app
  left="$(stop_servers_whose_test_process_ended "$app.dll" "$1")"
  fresh_left=0
  for fresh_app in ${fresh_apps[@]+"${fresh_apps[@]}"}; do
    fresh_left=$((fresh_left + $(stop_servers_whose_test_process_ended "$(basename "$fresh_app").dll" "$1")))
  done
}

run_the_tests() {
  local command=(dotnet test "$project" -c Release --no-build --nologo
    --logger "console;verbosity=normal" --logger "trx;LogFileName=$1.trx" --results-directory "$run")
  if [ -n "$filter" ]; then command+=(--filter "$filter"); fi
  command+=(-- "NUnit.NumberOfTestWorkers=$2")
  if [ "${OS:-}" = Windows_NT ]; then
    pwsh -NoProfile -File scripts/playwright-windows.ps1 "$run/$1-hosts" "$selected_host" "${command[@]:1}" 2>&1
  else
    "${command[@]}" 2>&1
  fi
}

read_servers_left() {
  if [ "${OS:-}" = Windows_NT ]; then
    read -r left fresh_left < "$run/$1-hosts/servers-left" || refuse "the Windows runner did not report server cleanup for $1."
  else
    stop_every_server_whose_test_process_ended "$2"
  fi
}

run_measured() {
  export ALIS_MEASURED_STEPS="$PWD/$run/$1.steps.tsv"
  local test_exit=0
  run_the_tests "$1" "$2" > "$run/$1.log" || test_exit=$?
  unset ALIS_MEASURED_STEPS
  read_servers_left "$1" "a server left alive after the measured run $1"
  [ "$((left + fresh_left))" -eq 0 ] || refuse "the measured run $1 left $((left + fresh_left)) server alive; nothing is written."
  [ "$test_exit" -eq 0 ] || refuse "the measured run $1 exited $test_exit; read $run/$1.log."
}

at_once=""
if [ "$parallelism" -eq 0 ]; then
  at_once="$(node scripts/playwright-result.mjs --at-once --budget "$budget")" \
    || refuse "no count of tests at once is measured in $budget; scripts/playwright.sh --measure-parallelism writes it."
fi

if [ "${OS:-}" != Windows_NT ]; then
  stop_every_server_whose_test_process_ended "stopped a server an earlier run left alive"
fi

if [ "$parallelism" -eq 1 ]; then
  measured=()
  for count in 1 2 4 8; do
    measured+=(--at "$count")
    for number in $(seq 1 "$measured_runs"); do
      run_measured "parallel-$count-$number" "$count"
      measured+=("$run/parallel-$count-$number.trx")
    done
    echo "[playwright:runner] measured $measured_runs runs at $count at once"
  done
  node scripts/playwright-result.mjs --measure-parallelism --selection "$run/selection.json" --budget "$budget" "${measured[@]}"
elif [ "$measure" -eq 1 ]; then
  node scripts/playwright-result.mjs --measurable --selection "$run/selection.json" || exit 2
  for number in $(seq 1 "$measured_runs"); do
    run_measured "measure-$number" "$at_once"
    echo "[playwright:runner] measured run $number of $measured_runs"
  done
  node scripts/playwright-result.mjs --measure --selection "$run/selection.json" --budget "$budget" --weights "$weights" "$run"/measure-*.trx
else
  test_exit=0
  run_the_tests run "$at_once" | tee "$run/run.log" || test_exit=$?
  read_servers_left run "a server left alive after this run"
  echo "[playwright:runner] log: $run/run.log"
  node scripts/playwright-result.mjs --selection "$run/selection.json" --trx "$run/run.trx" --budget "$budget" --servers-left "$left" --fresh-servers-left "$fresh_left"
  exit "$test_exit"
fi
