#!/usr/bin/env bash
# What the framework's own tests run of the framework. Runs the framework Playwright project with the
# runtime's block coverage recorded per page (ALIS_JS_COVERAGE_DIR, read by AppPageTest) and the core's
# assemblies statically instrumented by dotnet-coverage, then writes the report
# (scripts/framework-coverage.mjs). The test output's assemblies are restored afterwards.
# Needs dotnet-coverage: dotnet tool install -g dotnet-coverage.
#
# usage: scripts/framework-coverage.sh
set -euo pipefail
cd "$(dirname "$0")/.."

command -v dotnet-coverage > /dev/null || { echo "dotnet-coverage is not installed: dotnet tool install -g dotnet-coverage" >&2; exit 3; }

out="$PWD/tests/TestResults/framework-coverage/$(date +%Y%m%d-%H%M%S)"
bin="tests/Alis.Reactive.Framework.PlaywrightTests/bin/Debug/net10.0"
session="alis-framework-coverage-$$"
core=(Alis.Reactive Alis.Reactive.FluentValidator)
mkdir -p "$out/js" "$out/original"

dotnet build tests/Alis.Reactive.Framework.PlaywrightTests/Alis.Reactive.Framework.PlaywrightTests.csproj --nologo -v q

restore() {
  for name in "${core[@]}"; do
    cp "$out/original/$name.dll" "$bin/$name.dll"
    cp "$out/original/$name.pdb" "$bin/$name.pdb"
  done
}

for name in "${core[@]}"; do
  cp "$bin/$name.dll" "$bin/$name.pdb" "$out/original/"
  dotnet-coverage instrument --nologo --session-id "$session" --output "$bin/$name.dll" "$out/original/$name.dll" > "$out/instrument-$name.log"
done
trap restore EXIT

dotnet-coverage collect --nologo --server-mode --session-id "$session" --output "$out/core.cobertura.xml" --output-format cobertura > "$out/collect.log" 2>&1 &
collector=$!
sleep 2

ALIS_JS_COVERAGE_DIR="$out/js" scripts/playwright.sh --project framework --no-build \
  || echo "[coverage] the framework tests failed; the report covers the tests that ran" >&2

dotnet-coverage shutdown --nologo "$session" > "$out/shutdown.log" 2>&1 || true
wait "$collector" || true

node scripts/framework-coverage.mjs "$out/js" "$out/core.cobertura.xml" "$out/report.md"
