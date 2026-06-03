#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."

usage() {
  cat <<'USAGE'
Usage:
  scripts/test.sh
  scripts/test.sh --no-e2e

Runs the ordered verification gate:
  1. npm run typecheck
  2. npm run build:all
  3. npm test
  4. dotnet build
  5. scripts/playwright.sh --no-build

Options:
  --no-e2e    Skip the Playwright browser leg after typecheck, assets, vitest,
              and dotnet build have passed.
  -h, --help  Show this help.

Playwright starts its own sandbox on a random free port. Do not pre-start the
sandbox for this command.
USAGE
}

run_e2e=1
while [ "$#" -gt 0 ]; do
  case "$1" in
    --no-e2e)
      run_e2e=0
      shift
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
done

echo "[test] ensuring npm dependencies"
[ -d node_modules ] || npm ci

echo "[test] checking generated TS contract and TypeScript"
npm run typecheck

echo "[test] building browser assets"
npm run build:all

echo "[test] running vitest"
npm test

echo "[test] compiling C# projects"
dotnet build

if [ "$run_e2e" -eq 1 ]; then
  echo "[test] running observable Playwright"
  scripts/playwright.sh --no-build
else
  echo "[test] skipping Playwright (--no-e2e)"
fi

echo "All gates green."
