#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."

usage() {
  cat <<'USAGE'
Usage:
  scripts/pack.sh <version>

Example:
  scripts/pack.sh 1.0.0-rc.1

Builds delivery artifacts in order:
  1. install npm dependencies if node_modules is missing
  2. build all browser assets
  3. run dotnet build --configuration Release
  4. pack the six shipped NuGet packages into ./nupkgs

The script clears old .nupkg/.snupkg files from ./nupkgs first so the output
list only contains packages from this run. dotnet pack never runs npm.
USAGE
}

case "${1:-}" in
  "")
    echo "Missing package version." >&2
    usage >&2
    exit 2
    ;;
  -h|--help)
    usage
    exit 0
    ;;
esac

if [ "$#" -ne 1 ]; then
  echo "Expected exactly one version argument." >&2
  usage >&2
  exit 2
fi

VERSION="$1"
OUTPUT_DIR="nupkgs"

echo "[pack] ensuring npm dependencies"
[ -d node_modules ] || npm ci

echo "[pack] building browser assets"
npm run build:all

echo "[pack] compiling Release projects"
dotnet build --configuration Release

PACKAGES=(
  Alis.Reactive                 # core plan model, builders, serialization, runtime JS bundle
  Alis.Reactive.Native          # native HTML components
  Alis.Reactive.Fusion          # Syncfusion EJ2 integration + syncfusion CSS bundle (net10.0 only)
  Alis.Reactive.FluentValidator # FluentValidation client-metadata adapter
  Alis.Reactive.DesignSystem    # design-system tokens + layout helpers + CSS bundle
  Alis.Reactive.NativeTagHelpers # ASP.NET Core tag helpers (net10.0 only)
)

mkdir -p "$OUTPUT_DIR"
rm -f "$OUTPUT_DIR"/*.nupkg "$OUTPUT_DIR"/*.snupkg

for proj in "${PACKAGES[@]}"; do
  dotnet pack "$proj/$proj.csproj" --configuration Release --no-build --output "./$OUTPUT_DIR" -p:Version="$VERSION"
done

echo "Packed ${#PACKAGES[@]} packages to ./$OUTPUT_DIR (version $VERSION)."
ls -1 "$OUTPUT_DIR"/*.nupkg
