#!/usr/bin/env bash
# Full test gate (CLAUDE.md "Run the tests" + "Before every push"):
#   vitest (jsdom)  ->  both-TFM C# build  ->  drift typecheck  ->  Playwright (browser).
# The Playwright fixture starts and stops its OWN sandbox on a random free port,
# so port 5220 does not need to be free. Pass --no-e2e to skip the browser leg.
set -euo pipefail
cd "$(dirname "$0")/.."

[ -d node_modules ] || npm ci
npm run build:all                       # Playwright runs against freshly built runtime assets
npm test                                # vitest across both npm workspaces
dotnet build                            # net48 + net10.0
npm run typecheck                       # regenerate plan.ts + tsc both projects (contract drift gate)

if [ "${1:-}" != "--no-e2e" ]; then
  dotnet test tests/Alis.Reactive.PlaywrightTests --logger "console;verbosity=detailed"
fi

echo "All gates green."
