#!/usr/bin/env bash
# Build the framework bundles and start the sandbox at http://localhost:5220.
# The sandbox refuses to start without the bundles, so build:all runs first.
set -euo pipefail
cd "$(dirname "$0")/.."

[ -d node_modules ] || npm ci
npm run build:all
echo "Starting sandbox -> http://localhost:5220 (Ctrl+C to stop)"
dotnet run --project Alis.Reactive.SandboxApp
