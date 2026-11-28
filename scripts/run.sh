#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."

usage() {
  cat <<'USAGE'
Usage:
  scripts/run.sh

Builds the browser bundle and the conformance app, and starts the app in Development:
  http://localhost:5220

A browser that opens a page with no world gets a world of its own, so the app runs for a person as it does
for a test. Use Ctrl+C in this terminal to stop it. Playwright starts its own server, so a run of the tests
needs none of this.
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

node build.mjs
ASPNETCORE_ENVIRONMENT=Development exec dotnet run --project tests/Alis.Reactive.Conformance.App -- --urls http://localhost:5220 --host net10
