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
  2. the net48 time zone table, written by scripts/windows-zones.cs from CLDR's windowsZones.xml at its pin and
     compared with the tracked Alis.Reactive/PathsAndWire/WindowsZones.g.Net48.cs
  3. npm ci when node_modules is missing, then npm audit by scripts/npm-audit.mjs, past the advisories it excuses
  4. the Syncfusion pin's agreement in NuGet, npm and the lock
  5. tsc -b over the runtime, the OpenTelemetry adapter, the Native components, the Fusion components and templates and conformance app, eslint with --max-warnings 0,
     vitest over the runtime's pure tests, then the browser bundle
  6. dotnet restore in locked mode, then dotnet build -c Release, then the doors each browser test's pages call,
     written by scripts/page-doors.cs and compared with the tracked spec/Reference/page-doors.json
  7. dotnet format --verify-no-changes over the solution
  8. the architecture tests but WhenAConsumerAppIsBuilt, which scripts/delivery.sh runs on the packs,
     the analyzer cases, the test rules' cases and the module tests
  9. python3 spec/tools/check.py --ci, which reads JAVA_HOME and TLA2TOOLS_JAR

Options:
  --no-spec   Leave step 9 out: CI runs it as a job of its own.
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

zones=Alis.Reactive/PathsAndWire/WindowsZones.g.Net48.cs
step "the net48 time zone table, generated from CLDR's windowsZones.xml at its pin"
dotnet run scripts/windows-zones.cs -p:RestorePackagesWithLockFile=false -- spec/probes/cldr/windowsZones.xml.txt spec/probes/cldr/windowsZones.lock "$zones"
git ls-files --error-unmatch "$zones" > /dev/null
if ! git diff --exit-code -- "$zones"; then
  echo "[check] CLDR's windowsZones.xml at its pin generates another table than the tracked one: stage the WindowsZones.g.Net48.cs it wrote." >&2
  exit 1
fi

step "npm audit"
[ -d node_modules ] || npm ci
node scripts/npm-audit.mjs

step "the Syncfusion pin"
node scripts/pin-syncfusion.mjs --check

step "tsc -b over the runtime, the OpenTelemetry adapter, the Native components, the Fusion components and templates and conformance app"
node node_modules/typescript/bin/tsc -b Alis.Reactive.Assets Alis.Reactive.OpenTelemetry Alis.Reactive.Native Alis.Reactive.Fusion tests/Alis.Reactive.Conformance.App

step "eslint over the runtime, the OpenTelemetry adapter, the Native components, the Fusion components and templates and conformance app"
node node_modules/eslint/bin/eslint.js Alis.Reactive.Assets Alis.Reactive.OpenTelemetry Alis.Reactive.Native Alis.Reactive.Fusion tests/Alis.Reactive.Conformance.App --max-warnings 0

step "vitest over the runtime's and the adapter's pure tests"
node node_modules/vitest/vitest.mjs run

step "the browser bundle"
node build.mjs

step "dotnet restore, locked"
dotnet restore Alis.Reactive.slnx --locked-mode

step "dotnet build -c Release"
dotnet build Alis.Reactive.slnx -c Release --no-restore

doors=spec/Reference/page-doors.json
step "the doors each browser test's pages call"
dotnet run scripts/page-doors.cs -p:RestorePackagesWithLockFile=false -- \
  tests/Alis.Reactive.Conformance.App/bin/Release/net10.0/Alis.Reactive.Conformance.App.dll \
  tests/Alis.Reactive.Conformance.PlaywrightTests/bin/Release/net10.0/Alis.Reactive.Conformance.PlaywrightTests.dll "$doors"
git ls-files --error-unmatch "$doors" > /dev/null
if ! git diff --exit-code -- "$doors"; then
  echo "[check] The built pages call other doors than the tracked table lands: stage the page-doors.json it wrote." >&2
  exit 1
fi

step "dotnet format --verify-no-changes"
dotnet format Alis.Reactive.slnx --verify-no-changes --severity warn --no-restore

step "dotnet test tests/Alis.Reactive.ArchitectureTests, but WhenAConsumerAppIsBuilt"
dotnet test tests/Alis.Reactive.ArchitectureTests -c Release --no-build --filter "FullyQualifiedName!~WhenAConsumerAppIsBuilt"

for project in tests/Alis.Reactive.AnalyzerTests tests/Alis.Reactive.TestRules.AnalyzerTests tests/Alis.Reactive.ModuleTests; do
  step "dotnet test $project"
  dotnet test "$project" -c Release --no-build
done

if [ "$spec" -eq 1 ]; then
  step "check.py --ci"
  python3 spec/tools/check.py --ci
fi

echo "[check] every check passed."
