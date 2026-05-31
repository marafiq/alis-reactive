#!/usr/bin/env bash
# Delivery: build bundles + Release build, then pack the six library NuGets to ./nupkgs.
# `dotnet pack` never runs npm, so build:all must finish first (CLAUDE.md "Pack the NuGet").
#   usage: scripts/pack.sh <version>     e.g. scripts/pack.sh 1.0.0-rc.1
set -euo pipefail
cd "$(dirname "$0")/.."

VERSION="${1:?usage: scripts/pack.sh <version>   e.g. scripts/pack.sh 1.0.0-rc.1}"

[ -d node_modules ] || npm ci
npm run build:all
dotnet build --configuration Release

PACKAGES=(
  Alis.Reactive                 # core plan model, builders, serialization, runtime JS bundle
  Alis.Reactive.Native          # native HTML components
  Alis.Reactive.Fusion          # Syncfusion EJ2 integration + syncfusion CSS bundle (net10.0 only)
  Alis.Reactive.FluentValidator # FluentValidation client-metadata adapter
  Alis.Reactive.DesignSystem    # design-system tokens + layout helpers + CSS bundle
  Alis.Reactive.NativeTagHelpers # ASP.NET Core tag helpers (net10.0 only)
)

mkdir -p nupkgs
for proj in "${PACKAGES[@]}"; do
  dotnet pack "$proj/$proj.csproj" --configuration Release --no-build --output ./nupkgs -p:Version="$VERSION"
done

echo "Packed ${#PACKAGES[@]} packages to ./nupkgs (version $VERSION)."
ls -1 nupkgs/*.nupkg
