#!/usr/bin/env bash
# Full test gate (CLAUDE.md "Run the tests" + "Before every push"):
#   drift typecheck  ->  fresh browser assets  ->  vitest (jsdom)
#   ->  both-TFM C# build  ->  Playwright (browser).
# The Playwright fixture starts and stops its OWN sandbox on a random free port,
# so port 5220 does not need to be free. Pass --no-e2e to skip the browser leg.
set -euo pipefail
cd "$(dirname "$0")/.."

[ -d node_modules ] || npm ci
npm run typecheck                       # regenerate plan.ts + tsc both projects (contract drift gate)
npm run build:all                       # bundle generated plan types + browser assets
npm test                                # vitest across both npm workspaces
dotnet build                            # net48 + net10.0

if [ "${1:-}" != "--no-e2e" ]; then
  scripts/playwright.sh --no-build
fi

echo "All gates green."
