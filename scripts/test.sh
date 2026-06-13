#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."

usage() {
  cat <<'USAGE'
Usage:
  scripts/test.sh
  scripts/test.sh --no-e2e

Runs the ordered verification gate:
  1. npm run typecheck
  2. npm run build:all
  3. npm test
  4. dotnet build
  5. non-Playwright dotnet test projects, if any
  6. scripts/playwright.sh --no-build
  7. behavioral coverage gate (0b) over behaviorally-audited Fusion components

Options:
  --no-e2e    Skip the Playwright browser leg after typecheck, assets, vitest,
              dotnet build, and non-Playwright dotnet tests have passed.
  -h, --help  Show this help.

Playwright starts its own sandbox on a random free port. Do not pre-start the
sandbox for this command.

Set CONFIGURATION=Release to run the .NET build/test legs in Release.
USAGE
}

configuration="${CONFIGURATION:-Debug}"
run_e2e=1
while [ "$#" -gt 0 ]; do
  case "$1" in
    --no-e2e)
      run_e2e=0
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

echo "[test] ensuring npm dependencies"
[ -d node_modules ] || npm ci

echo "[test] checking generated TS contract and TypeScript"
npm run typecheck

echo "[test] building browser assets"
npm run build:all

echo "[test] running vitest"
npm test

echo "[test] compiling C# projects ($configuration)"
dotnet build --configuration "$configuration"

run_dotnet_tests

if [ "$run_e2e" -eq 1 ]; then
  echo "[test] running observable Playwright"
  CONFIGURATION="$configuration" scripts/playwright.sh --no-build
  # 0b behavioral coverage gate: the fresh TRX from the run above is the truth
  # source. For every component that claims behavioral coverage
  # (proof/behavioral-coverage.json), this confirms each mapped member's test
  # exists in that TRX and passed. Components without a map are below bar, not a
  # failure. Skipped under --no-e2e: no fresh TRX means no behavioral proof.
  echo "[test] behavioral coverage gate (0b)"
  node .claude/skills/onboard-fusion-component/scripts/verify-behavioral-coverage.mjs --all
else
  echo "[test] skipping Playwright and behavioral coverage gate (--no-e2e)"
fi

echo "All gates green."
