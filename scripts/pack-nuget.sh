#!/usr/bin/env bash
# Build fresh JS/CSS bundles and pack all Alis.Reactive NuGet packages.
#
# Usage:
#   ./scripts/pack-nuget.sh                  # version suffix defaults to "preview.local"
#   ./scripts/pack-nuget.sh preview.42       # explicit preview suffix (CI uses this form)
#   ./scripts/pack-nuget.sh ""               # stable release — no version suffix
#
# Outputs to ./nupkgs/.
#
# This script is the single entry point for producing the NuGet packages, used
# by both local development and the nuget-publish GitHub Actions workflow.

set -euo pipefail

cd "$(dirname "$0")/.."

# Unset → preview.local. Explicit empty string ("") → empty suffix (stable).
VERSION_SUFFIX="${1-preview.local}"
OUTPUT_DIR="./nupkgs"

echo "=== [1/4] Building JS + CSS bundles ==="
npm run build:all

echo "=== [2/4] Staging shipped assets into Alis.Reactive/assets/ ==="
mkdir -p Alis.Reactive/assets/js Alis.Reactive/assets/css
cp Alis.Reactive.SandboxApp/wwwroot/js/alis-reactive.js Alis.Reactive/assets/js/
cp Alis.Reactive.SandboxApp/wwwroot/css/design-system.css Alis.Reactive/assets/css/

echo "=== [3/4] Building all C# projects (Release) ==="
dotnet build Alis.Reactive.slnx --configuration Release

echo "=== [4/4] Packing NuGet packages ==="
if [ -n "$VERSION_SUFFIX" ]; then
    echo "    Version suffix: $VERSION_SUFFIX"
    PACK_ARGS=(--configuration Release --no-build --output "$OUTPUT_DIR" --version-suffix "$VERSION_SUFFIX")
else
    echo "    Stable release — no version suffix"
    PACK_ARGS=(--configuration Release --no-build --output "$OUTPUT_DIR")
fi

PROJECTS=(
    Alis.Reactive
    Alis.Reactive.Native
    Alis.Reactive.Fusion
    Alis.Reactive.FluentValidator
    Alis.Reactive.NativeTagHelpers
)

for proj in "${PROJECTS[@]}"; do
    dotnet pack "$proj/$proj.csproj" "${PACK_ARGS[@]}"
done

echo
echo "=== Packages in $OUTPUT_DIR ==="
ls -1 "$OUTPUT_DIR"
