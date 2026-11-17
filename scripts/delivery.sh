#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."

usage() {
  cat <<'USAGE'
Usage:
  scripts/delivery.sh

Proves what the packages deliver to an app, in order, and stops at the first that fails:
  1. scripts/pack.sh at a version of this run's own, 0.0.0-delivery.<UTC time>, into ./nupkgs
  2. WhenAConsumerAppIsBuilt, whose cases build small apps on those packs, restored
     from ./nupkgs and nuget.org into each app's own obj folder, and whose tests build
     tests/Alis.Reactive.Consumer.Net10 on them; its test of the fresh MVC 5 app,
     tests/Alis.Reactive.Consumer.Net48, builds it with Visual Studio's MSBuild, which
     only CI's Windows job holds, so it runs there
  3. the browser tests under consumerNet10, scripts/playwright.sh --all --host consumerNet10,
     on the fresh net10 app built on the same packs

Its last line is "[delivery] every delivery check passed."
USAGE
}

case "${1:-}" in
  "") ;;
  -h|--help) usage; exit 0 ;;
  *) echo "Unexpected argument '$1'." >&2; usage >&2; exit 2 ;;
esac

version="0.0.0-delivery.$(date -u +%Y%m%d%H%M%S)"

echo "[delivery] packing at $version"
scripts/pack.sh "$version"

echo "[delivery] WhenAConsumerAppIsBuilt"
dotnet test tests/Alis.Reactive.ArchitectureTests -c Release --no-build --filter "FullyQualifiedName~WhenAConsumerAppIsBuilt&FullyQualifiedName!~fresh_net48_app"

echo "[delivery] the browser tests under consumerNet10"
PACKAGE_VERSION="$version" scripts/playwright.sh --all --host consumerNet10

echo "[delivery] every delivery check passed."
