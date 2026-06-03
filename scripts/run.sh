#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."

usage() {
  cat <<'USAGE'
Usage:
  scripts/run.sh

Builds browser assets and starts the sandbox:
  http://localhost:5220

Use Ctrl+C in this terminal to stop the sandbox. Playwright starts its own
sandbox, so do not run this script before Playwright tests.
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

echo "[run] ensuring npm dependencies"
[ -d node_modules ] || npm ci

echo "[run] building browser assets"
npm run build:all

echo "Starting sandbox -> http://localhost:5220 (Ctrl+C to stop)"
dotnet run --project Alis.Reactive.SandboxApp
