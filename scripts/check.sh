#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."

usage() {
  cat <<'USAGE'
Usage:
  scripts/check.sh
  scripts/check.sh --no-spec

Runs every check that needs no browser, in order, and stops at the first that fails:
  1. the plan contract, generated from the grammar and compared with the tracked plan.ts and plan.schema.json
  2. npm ci when node_modules is missing, then npm audit
  3. the Syncfusion pin's agreement in NuGet, npm and the lock
  4. tsc -b over the runtime, eslint with --max-warnings 0, vitest over the runtime's pure tests, then the browser bundle
  5. dotnet restore in locked mode, then dotnet build -c Release
  6. dotnet format --verify-no-changes over the solution
  7. the architecture tests, the analyzer cases, the test rules' cases and the module tests
  8. python3 spec/tools/check.py --ci, which reads JAVA_HOME and TLA2TOOLS_JAR

Options:
  --no-spec   Leave step 8 out: CI runs it as a job of its own.
  -h, --help  Show this help.
USAGE
}

spec=1
case "${1:-}" in
  "") ;;
  --no-spec) spec=0 ;;
  -h|--help) usage; exit 0 ;;
  *) echo "Unexpected argument '$1'." >&2; usage >&2; exit 2 ;;
esac

step() {
  echo "[check] $1"
}

contract=Alis.Reactive.Assets/runtime/Schema
step "the plan contract, generated from the grammar"
dotnet run --project tools/PlanContractGenerator -c Release --property:RestoreLockedMode=true -- "$contract"
git ls-files --error-unmatch "$contract/plan.ts" "$contract/plan.schema.json" > /dev/null
if ! git diff --exit-code -- "$contract/plan.ts" "$contract/plan.schema.json"; then
  echo "[check] The grammar generates another contract than the tracked one: stage the plan.ts and plan.schema.json it wrote." >&2
  exit 1
fi

step "npm audit"
[ -d node_modules ] || npm ci
npm audit

step "the Syncfusion pin"
node scripts/pin-syncfusion.mjs --check

step "tsc -b over the runtime"
node node_modules/typescript/bin/tsc -b Alis.Reactive.Assets

step "eslint over the runtime"
node node_modules/eslint/bin/eslint.js Alis.Reactive.Assets --max-warnings 0

step "vitest over the runtime's pure tests"
node node_modules/vitest/vitest.mjs run

step "the browser bundle"
node build.mjs

step "dotnet restore, locked"
dotnet restore Alis.Reactive.slnx --locked-mode

step "dotnet build -c Release"
dotnet build Alis.Reactive.slnx -c Release --no-restore

step "dotnet format --verify-no-changes"
dotnet format Alis.Reactive.slnx --verify-no-changes --severity warn --no-restore

for project in tests/Alis.Reactive.ArchitectureTests tests/Alis.Reactive.AnalyzerTests tests/Alis.Reactive.TestRules.AnalyzerTests tests/Alis.Reactive.ModuleTests; do
  step "dotnet test $project"
  dotnet test "$project" -c Release --no-build
done

if [ "$spec" -eq 1 ]; then
  step "check.py --ci"
  python3 spec/tools/check.py --ci
fi

echo "[check] every check passed."
