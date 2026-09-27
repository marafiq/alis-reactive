#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."

usage() {
  cat <<'USAGE'
Usage:
  scripts/test.sh
  scripts/test.sh --parallel
  scripts/test.sh --no-e2e

Runs the ordered verification gate:
  1. npm run typecheck
  2. npm run build:all
  3. npm test
  4. gate-script self-tests (.claude/skills/onboard-fusion-component/scripts/*.selftest.mjs)
  5. dotnet build
  6. non-Playwright dotnet test projects, if any
  7. scripts/playwright.sh --no-build (or its five CI shards at once with --parallel)
  8. behavioral coverage gate (0b) over behaviorally-audited Fusion components

Options:
  --parallel  Run the Playwright leg as the five CI shards (scripts/playwright.sh
              --list-shards), side by side, each with its own sandbox; the 0b gate
              then reads every shard's TRX. About 20 minutes instead of about 60.
  --no-e2e    Skip the Playwright browser leg after typecheck, assets, vitest,
              self-tests, dotnet build, and non-Playwright dotnet tests have passed.
  -h, --help  Show this help.

Playwright starts its own sandbox on a random free port. Do not pre-start the
sandbox for this command.

Set CONFIGURATION=Release to run the .NET build/test legs in Release.
USAGE
}

configuration="${CONFIGURATION:-Debug}"
run_e2e=1
parallel=0
while [ "$#" -gt 0 ]; do
  case "$1" in
    --no-e2e)
      run_e2e=0
      shift
      ;;
    --parallel)
      parallel=1
      shift
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      echo "Unexpected argument '$1'." >&2
      usage >&2
      exit 2
      ;;
  esac
done

behavioral_gate="node .claude/skills/onboard-fusion-component/scripts/verify-behavioral-coverage.mjs --all"

run_dotnet_tests() {
  local projects=()
  local project

  while IFS= read -r project; do
    if [[ "$project" == *Playwright* ]]; then
      continue
    fi

    if grep -q 'Microsoft.NET.Test.Sdk' "$project"; then
      projects+=("$project")
    fi
  done < <(find tests -name '*.csproj' -print | sort)

  if [ "${#projects[@]}" -eq 0 ]; then
    echo "[test] no non-Playwright dotnet test projects found"
    return
  fi

  for project in "${projects[@]}"; do
    echo "[test] running dotnet tests: $project"
    dotnet test "$project" --configuration "$configuration" --no-build
  done
}

# The 0b gate and the parity tool only mean something if they bite: their self-tests prove each
# failure mode turns them red and a clean input keeps them green.
run_gate_selftests() {
  local selftest output
  for selftest in .claude/skills/onboard-fusion-component/scripts/*.selftest.mjs; do
    if output="$(node "$selftest" 2>&1)"; then
      echo "[test]   $(printf '%s\n' "$output" | tail -1)"
    else
      printf '%s\n' "$output"
      echo "[test] self-test failed: $selftest" >&2
      return 1
    fi
  done
}

# CI runs exactly the shards listed in gate.yml's matrix; scripts/playwright.sh defines them. A shard
# added to the script but not to the matrix would never run in CI and nothing would fail, so the two
# lists must match.
check_ci_shard_matrix() {
  local script_shards ci_shards
  script_shards="$(scripts/playwright.sh --list-shards | sort)"
  ci_shards="$(awk '/^[ \t]+shard:[ \t]*$/ { in_list = 1; next }
                   in_list && /^[ \t]+- / { sub(/^[ \t]+- /, ""); print; next }
                   in_list { exit }' .github/workflows/gate.yml | sort)"
  if [ "$script_shards" != "$ci_shards" ]; then
    echo "[test] shard lists differ: scripts/playwright.sh --list-shards vs .github/workflows/gate.yml matrix" >&2
    diff <(printf '%s\n' "$script_shards") <(printf '%s\n' "$ci_shards") >&2 || true
    return 1
  fi
  echo "[test]   CI shard matrix matches scripts/playwright.sh ($(printf '%s\n' "$script_shards" | wc -l | tr -d ' ') shards)"
}

# The five CI shards side by side. Each scripts/playwright.sh run starts its own sandbox on a free
# port and stamps its log/TRX to the second, so the starts are staggered. Every shard's console
# output lands in one directory; the 0b gate then reads all of their TRX files (and any retries).
run_parallel_playwright() {
  local run_dir shard status=0 code
  local shards=() pids=() trx_files=()
  run_dir="tests/Alis.Reactive.PlaywrightTests/TestResults/observable/parallel-$(date +%Y%m%d-%H%M%S)"
  mkdir -p "$run_dir"

  while IFS= read -r shard; do
    shards+=("$shard")
  done < <(scripts/playwright.sh --list-shards)

  for shard in "${shards[@]}"; do
    CONFIGURATION="$configuration" scripts/playwright.sh --no-build --shard "$shard" > "$run_dir/$shard.log" 2>&1 &
    pids+=("$!")
    echo "[test] shard $shard started: tail -f $run_dir/$shard.log"
    sleep 2
  done

  for i in "${!shards[@]}"; do
    shard="${shards[$i]}"
    if wait "${pids[$i]}"; then code=0; else code=$?; status=1; fi
    echo "[test] shard $shard exit=$code | $(grep -aE '^(Total tests|     Passed|     Failed):' "$run_dir/$shard.log" | tr -s ' ' | tr '\n' ' ')"
    grep -aE '^\s+Failed [A-Za-z_]' "$run_dir/$shard.log" | sed "s/^/[test]   $shard: /" || true
    while IFS= read -r trx; do
      trx_files+=("$trx")
    done < <(grep -aoE '\[playwright:runner\] trx=[^ ]+' "$run_dir/$shard.log" | sed 's/.*trx=//' | sort -u)
  done

  if [ "$status" -ne 0 ]; then
    echo "[test] Playwright shards failed; logs in $run_dir" >&2
    return 1
  fi
  if [ "${#trx_files[@]}" -eq 0 ]; then
    echo "[test] no shard reported a TRX file; logs in $run_dir" >&2
    return 1
  fi

  echo "[test] behavioral coverage gate (0b) over ${#trx_files[@]} shard TRX file(s)"
  $behavioral_gate --trx "$(IFS=,; printf '%s' "${trx_files[*]}")"
}

echo "[test] ensuring npm dependencies"
[ -d node_modules ] || npm ci

echo "[test] checking generated TS contract and TypeScript"
npm run typecheck

echo "[test] building browser assets"
npm run build:all

echo "[test] running vitest"
npm test

echo "[test] gate-script self-tests"
run_gate_selftests
check_ci_shard_matrix

echo "[test] compiling C# projects ($configuration)"
dotnet build --configuration "$configuration"

run_dotnet_tests

if [ "$run_e2e" -eq 0 ]; then
  echo "[test] skipping Playwright and behavioral coverage gate (--no-e2e)"
elif [ "$parallel" -eq 1 ]; then
  echo "[test] running observable Playwright as parallel shards"
  run_parallel_playwright
else
  echo "[test] running observable Playwright"
  CONFIGURATION="$configuration" scripts/playwright.sh --no-build
  # 0b behavioral coverage gate: the fresh TRX from the run above is the truth
  # source. For every component that claims behavioral coverage
  # (proof/behavioral-coverage.json), this confirms each mapped member's test
  # exists in that TRX and passed. Components without a map are below bar, not a
  # failure. Skipped under --no-e2e: no fresh TRX means no behavioral proof.
  echo "[test] behavioral coverage gate (0b)"
  $behavioral_gate
fi

echo "All gates green."
