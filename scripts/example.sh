#!/usr/bin/env bash
set -Eeuo pipefail
cd "$(dirname "$0")/.."

usage() {
  cat <<'USAGE'
Usage:
  scripts/example.sh [version]
  scripts/example.sh --smoke [version]

Builds examples/resident-intake the way an app outside this repository builds: against the packed
NuGet packages, not the projects.
  1. scripts/pack.sh <version>: the five packages into ./nupkgs, the local feed the example's
     nuget.config reads
  2. npm ci in examples/resident-intake: its own Tailwind CLI and @syncfusion/ej2
  3. dotnet build the example: restores the packages, the design system copies its theme into
     Styles/, the example's npm build compiles its stylesheet and copies its Syncfusion files
  4. runs it at http://localhost:5300 (Ctrl+C stops it)

With --smoke, step 4 starts the app, drives it with ci/smoke.mjs (Playwright for node) and stops it:
the proof the gate's `example` job runs.

The version defaults to 0.0.0-local.<timestamp>. NuGet never re-extracts a version its cache already
holds, so every pack gets a version of its own. A Syncfusion license comes from Syncfusion__LicenseKey
or the example's user secrets; without one EJ2 shows a trial dialog and the smoke fails.
USAGE
}

smoke=0
version=""
while [ "$#" -gt 0 ]; do
  case "$1" in
    --smoke)
      smoke=1
      shift
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    -*)
      echo "Unexpected argument '$1'." >&2
      usage >&2
      exit 2
      ;;
    *)
      if [ -n "$version" ]; then
        echo "Expected at most one version argument." >&2
        usage >&2
        exit 2
      fi
      version="$1"
      shift
      ;;
  esac
done
version="${version:-0.0.0-local.$(date +%Y%m%d%H%M%S)}"

example="examples/resident-intake"
url="http://localhost:5300"

echo "[example] packing the packages as $version"
scripts/pack.sh "$version"

echo "[example] installing the example's npm dependencies"
(cd "$example" && npm ci --no-audit --no-fund)

echo "[example] building the example against AlisReactive $version"
dotnet build "$example" --configuration Release -p:AlisReactiveVersion="$version"

# Development serves the packages' static web assets (~/_content/...) from the restored packages; the
# example runs from its folder so that is its content root. The shell becomes the app, so the app's
# pid is the one started (and the one stopped after the smoke).
start_app() {
  cd "$example"
  ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS="$url" exec dotnet bin/Release/net10.0/ResidentIntake.dll
}

if [ "$smoke" -eq 0 ]; then
  echo "[example] starting $url (Ctrl+C to stop)"
  start_app
fi

echo "[example] installing the smoke's Playwright and browser"
# The browser install leaves other versions in the cache alone (it prunes them by default).
(cd "$example/ci" && npm ci --no-audit --no-fund && PLAYWRIGHT_SKIP_BROWSER_GC=1 npx playwright install chromium)

log="$example/ci/app.log"
start_app > "$log" 2>&1 &
app_pid=$!
stop_app() {
  kill "$app_pid" 2>/dev/null || true
}
trap stop_app EXIT

for _ in $(seq 1 60); do
  if curl -fs -o /dev/null "$url/"; then
    break
  fi
  if ! kill -0 "$app_pid" 2>/dev/null; then
    echo "[example] the app exited before it answered; its output:" >&2
    cat "$log" >&2
    exit 1
  fi
  sleep 1
done
if ! curl -fs -o /dev/null "$url/"; then
  echo "[example] the app did not answer at $url within 60 s; its output:" >&2
  cat "$log" >&2
  exit 1
fi

echo "[example] driving the app at $url"
if ! SMOKE_URL="$url/" node "$example/ci/smoke.mjs"; then
  echo "[example] the smoke failed; the app's output:" >&2
  cat "$log" >&2
  exit 1
fi
echo "[example] smoke passed against AlisReactive $version"
