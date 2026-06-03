#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."

usage() {
  cat <<'USAGE'
Usage:
  scripts/doctor.sh

Read-only CLI preflight for framework developers. Checks required tools,
repository command wrappers, dependency restore state, and common stale-output
hints before build/test/package work.

It does not build, restore, test, pack, or modify files.
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

failures=0

ok() { echo "[doctor:ok] $*"; }
warn() { echo "[doctor:warn] $*"; }
fail() {
  echo "[doctor:fail] $*" >&2
  failures=$((failures + 1))
}

require_command() {
  local name="$1"
  if command -v "$name" >/dev/null 2>&1; then
    ok "$name: $(command -v "$name")"
  else
    fail "$name is missing from PATH"
  fi
}

check_executable() {
  local path="$1"
  if [ -x "$path" ]; then
    ok "$path is executable"
  else
    fail "$path is missing or not executable"
  fi
}

check_output() {
  local label="$1"
  local path="$2"
  local fix="$3"

  if [ -e "$path" ]; then
    ok "$label exists: $path"
  else
    warn "$label is missing: $path"
    warn "  run: $fix"
  fi
}

echo "[doctor] required tools"
require_command dotnet
require_command node
require_command npm

if command -v dotnet >/dev/null 2>&1; then
  expected_dotnet="$(sed -n 's/.*"version": *"\([^"]*\)".*/\1/p' global.json | head -1)"
  actual_dotnet="$(dotnet --version)"
  if [ "$actual_dotnet" = "$expected_dotnet" ]; then
    ok "dotnet SDK matches global.json: $actual_dotnet"
  else
    warn "dotnet SDK is $actual_dotnet; global.json asks for $expected_dotnet with latestFeature roll-forward"
  fi
fi

if command -v node >/dev/null 2>&1; then
  node_major="$(node -p 'Number(process.versions.node.split(".")[0])')"
  if [ "$node_major" -ge 22 ]; then
    ok "Node.js is >= 22: $(node --version)"
  else
    fail "Node.js must be 22 or newer; found $(node --version)"
  fi
fi

if command -v pwsh >/dev/null 2>&1; then
  ok "pwsh is available for first-time Playwright browser install"
else
  warn "pwsh is missing; only needed for first-time Playwright browser install"
fi

echo "[doctor] command wrappers"
check_executable scripts/build.sh
check_executable scripts/run.sh
check_executable scripts/test.sh
check_executable scripts/playwright.sh
check_executable scripts/pack.sh
check_executable scripts/doctor.sh

echo "[doctor] restore and build-output hints"
if [ -d node_modules ]; then
  ok "node_modules exists"
else
  warn "node_modules is missing"
  warn "  run: npm ci"
fi

check_output "runtime bundle" "Alis.Reactive.Assets/dist/scripts/alis-reactive.dev.js" "scripts/build.sh"
check_output "design-system CSS" "Alis.Reactive.Assets/dist/css/design-system.dev.css" "scripts/build.sh"
check_output "Syncfusion CSS" "Alis.Reactive.Assets/dist/css/syncfusion.dev.css" "scripts/build.sh"
check_output "sandbox plugin bundle" "Alis.Reactive.SandboxApp/wwwroot/js/sandbox-plugins.js" "scripts/build.sh"
check_output "sandbox CSS" "Alis.Reactive.SandboxApp/wwwroot/css/sandbox.css" "scripts/build.sh"
check_output "Playwright test DLL" "tests/Alis.Reactive.PlaywrightTests/bin/Debug/net10.0/Alis.Reactive.PlaywrightTests.dll" "dotnet build"

echo "[doctor] git status snapshot"
status="$(git status --short)"
if [ -n "$status" ]; then
  printf '%s\n' "$status"
  warn "Review dirty files before committing. Build outputs should stay gitignored."
else
  ok "working tree is clean"
fi

if [ "$failures" -gt 0 ]; then
  echo "[doctor] failed with $failures required problem(s)." >&2
  exit 1
fi

echo "[doctor] preflight passed."
