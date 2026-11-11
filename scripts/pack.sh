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
  1. run dotnet build --configuration Release
  2. pack the shipped NuGet packages into ./nupkgs

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

echo "[pack] compiling Release projects"
dotnet build --configuration Release

PACKAGES=(
  Alis.Reactive
  Alis.Reactive.Native
  Alis.Reactive.Fusion
)

mkdir -p "$OUTPUT_DIR"
rm -f "$OUTPUT_DIR"/*.nupkg "$OUTPUT_DIR"/*.snupkg

for proj in "${PACKAGES[@]}"; do
  dotnet pack "$proj/$proj.csproj" --configuration Release --no-build --output "./$OUTPUT_DIR" -p:Version="$VERSION"
done

echo "Packed ${#PACKAGES[@]} packages to ./$OUTPUT_DIR (version $VERSION)."
ls -1 "$OUTPUT_DIR"/*.nupkg
