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
  --shard <i>/<k> Take the i-th of a pool's k shards, its units placed longest first by the weights --measure wrote.
  --measure       Run the selection 20 times, each green, and write to the harness's measured.json the budget,
                  four times the slowest test, and the action wait and the navigation wait, each four times the
                  slowest step of its kind, and to shard-weights.json the seconds each shard unit took in a run.
                  A measured run sets no measured wait and records every step.
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
    [[ "$args" == *"$app.dll"* ]] || continue
    [[ "$args" =~ --test-host[[:space:]]+([0-9]+) ]] || continue
    test_process="${BASH_REMATCH[1]}"
    kill -0 "$test_process" 2>/dev/null && continue
    echo "[playwright:runner] $1: pid $pid, whose test process $test_process has ended" >&2
    kill -9 "$pid" 2>/dev/null || true
    stopped=$((stopped + 1))
  done < <(ps -eo pid=,args=)
  printf '%s' "$stopped"
}

run="$project/TestResults/$(date +%Y%m%d-%H%M%S)-$$"
mkdir -p "$run"

echo "[playwright:runner] building $project"
if ! dotnet build "$project" -c Release --nologo; then
  echo "[playwright:runner] the build failed; no test ran." >&2
  exit 4
fi

dotnet run scripts/list-tests.cs -p:RestorePackagesWithLockFile=false -- "$assembly" > "$run/tests.tsv"
python3 spec/tools/landings.py > "$run/landings.tsv"
filter="$(node scripts/playwright-select.mjs --spec "$run/landings.tsv" --assembly "$run/tests.tsv" --root "$name" --weights "$weights" --out "$run/selection.json" \
  "${selector[@]}" ${host[@]+"${host[@]}"} ${shard[@]+"${shard[@]}"})"
echo "[playwright:runner] filter: ${filter:-every test}"

run_the_tests() {
  local command=(dotnet test "$project" -c Release --no-build --nologo
    --logger "console;verbosity=normal" --logger "trx;LogFileName=$1.trx" --results-directory "$run")
  if [ -n "$filter" ]; then command+=(--filter "$filter"); fi
  command+=(-- "NUnit.NumberOfTestWorkers=$2")
  "${command[@]}" 2>&1 || true
}

run_measured() {
  export ALIS_MEASURED_STEPS="$PWD/$run/$1.steps.tsv"
  run_the_tests "$1" "$2" > "$run/$1.log"
  unset ALIS_MEASURED_STEPS
  local left
  left="$(stop_servers_whose_test_process_ended "a server left alive after the measured run $1")"
  [ "$left" -eq 0 ] || refuse "the measured run $1 left $left server alive; nothing is written."
}

at_once=""
if [ "$parallelism" -eq 0 ]; then
  at_once="$(node scripts/playwright-result.mjs --at-once --budget "$budget")" \
    || refuse "no count of tests at once is measured in $budget; scripts/playwright.sh --measure-parallelism writes it."
fi

stop_servers_whose_test_process_ended "stopped a server an earlier run left alive" > /dev/null

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
  for number in $(seq 1 "$measured_runs"); do
    run_measured "measure-$number" "$at_once"
    echo "[playwright:runner] measured run $number of $measured_runs"
  done
  node scripts/playwright-result.mjs --measure --selection "$run/selection.json" --budget "$budget" --weights "$weights" "$run"/measure-*.trx
else
  run_the_tests run "$at_once" | tee "$run/run.log"
  left="$(stop_servers_whose_test_process_ended "a server left alive after this run")"
  echo "[playwright:runner] log: $run/run.log"
  node scripts/playwright-result.mjs --selection "$run/selection.json" --trx "$run/run.trx" --budget "$budget" --servers-left "$left"
fi
