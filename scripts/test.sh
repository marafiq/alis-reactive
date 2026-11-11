#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."

usage() {
  cat <<'USAGE'
Usage:
  scripts/test.sh

Runs both gates in order: scripts/check.sh, every check that needs no browser,
then scripts/playwright.sh, every browser test.
USAGE
}

case "${1:-}" in
  "") ;;
  -h|--help) usage; exit 0 ;;
  *) echo "Unexpected argument '$1'." >&2; usage >&2; exit 2 ;;
esac

scripts/check.sh
scripts/playwright.sh

echo "[test] every check and every browser test passed."
