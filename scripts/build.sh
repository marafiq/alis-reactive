#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."

usage() {
  cat <<'USAGE'
Usage:
  scripts/build.sh

Builds the repository from the repo root:
  1. install npm dependencies if node_modules is missing
  2. run dotnet build for all target frameworks

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

echo "[build] compiling C# projects"
dotnet build

echo "Build complete. git status should stay clean (build outputs are gitignored)."
