#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "$0")/.."

dotnet build poc/Alis.Reactive.Poc
dotnet run --project poc/tools/PlanContract -- poc/runtime/src/plan.ts
(cd poc/runtime && node "$(node -p "require.resolve('typescript/bin/tsc')")" -p tsconfig.json)
node poc/runtime/build.mjs
