#!/usr/bin/env bash
# Build everything: JS deps (if missing) -> framework bundles -> both-TFM C# build.
# Mirrors CLAUDE.md "First run" + "The bundles". Run from anywhere.
set -euo pipefail
cd "$(dirname "$0")/.."

[ -d node_modules ] || npm ci          # JS deps, from package-lock.json
npm run build:all                       # framework JS/CSS bundles into Alis.Reactive.Assets/dist/
dotnet build                            # compile all C# (net48 + net10.0); fails fast if a bundle is missing

echo "Build complete. git status should stay clean (all bundle outputs are gitignored)."
