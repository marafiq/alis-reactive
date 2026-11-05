#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."

usage() {
  cat <<'USAGE'
Usage:
  scripts/build.sh

Builds the repository from the repo root:
  1. install npm dependencies if node_modules is missing
  2. build the packages' browser assets (runtime bundle, design-system CSS)
  3. run dotnet build for all target frameworks, which builds the apps' own assets

Use this after a fresh clone and before any --no-build test run.
USAGE
}

case "${1:-}" in
  "")
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

echo "[build] ensuring npm dependencies"
[ -d node_modules ] || npm ci

echo "[build] building the packages' browser assets (dotnet build builds the apps' own)"
npm run build:packages

echo "[build] compiling C# projects"
dotnet build

echo "Build complete. git status should stay clean (all bundle outputs are gitignored)."
