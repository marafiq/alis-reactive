#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."

usage() {
  cat <<'USAGE'
Usage:
  scripts/test.sh
  scripts/test.sh --through U<n>

Runs both gates in order: scripts/check.sh, every check that needs no browser,
then scripts/playwright.sh over every browser test, or with --through over the
tests the spec lands in that unit and in every unit before it.
USAGE
}

browser=(--all)
case "${1:-}" in
  "") ;;
  --through)
    [ "$#" -eq 2 ] || { echo "--through takes a unit." >&2; usage >&2; exit 2; }
    browser=(--through "$2") ;;
  -h|--help) usage; exit 0 ;;
  *) echo "Unexpected argument '$1'." >&2; usage >&2; exit 2 ;;
esac

scripts/check.sh
scripts/playwright.sh "${browser[@]}"

echo "[test] every check and every selected browser test passed."
