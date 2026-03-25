#!/usr/bin/env bash
# rebuild-example-app.sh — Rebuilds example app DLLs, JS/CSS assets, and packages the zip.
# Run from repo root: ./scripts/rebuild-example-app.sh
#
# What it does:
#   1. Builds all 5 Alis.Reactive framework DLLs (Release)
#   2. Copies fresh DLLs into examples/resident-intake/lib/
#   3. Copies fresh JS + CSS bundles into examples/resident-intake/wwwroot/
#   4. Verifies the example app compiles against the new DLLs
#   5. Packages examples/resident-intake/ into docs-site/public/downloads/resident-intake.zip

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
EXAMPLE_DIR="$REPO_ROOT/examples/resident-intake"
DOWNLOADS_DIR="$REPO_ROOT/docs-site/public/downloads"
ZIP_FILE="$DOWNLOADS_DIR/resident-intake.zip"

echo "=== Step 1: Build framework DLLs (Release) ==="
dotnet build "$REPO_ROOT/Alis.Reactive/Alis.Reactive.csproj" -c Release --nologo -v q
dotnet build "$REPO_ROOT/Alis.Reactive.Native/Alis.Reactive.Native.csproj" -c Release --nologo -v q
dotnet build "$REPO_ROOT/Alis.Reactive.Fusion/Alis.Reactive.Fusion.csproj" -c Release --nologo -v q
dotnet build "$REPO_ROOT/Alis.Reactive.FluentValidator/Alis.Reactive.FluentValidator.csproj" -c Release --nologo -v q

echo "=== Step 2: Copy DLLs to example app lib/ ==="
mkdir -p "$EXAMPLE_DIR/lib"

# Find Release output directories
CORE_OUT="$REPO_ROOT/Alis.Reactive/bin/Release/net10.0"
NATIVE_OUT="$REPO_ROOT/Alis.Reactive.Native/bin/Release/net10.0"
FUSION_OUT="$REPO_ROOT/Alis.Reactive.Fusion/bin/Release/net10.0"
VALIDATOR_OUT="$REPO_ROOT/Alis.Reactive.FluentValidator/bin/Release/net10.0"

cp "$CORE_OUT/Alis.Reactive.dll" "$EXAMPLE_DIR/lib/"
cp "$NATIVE_OUT/Alis.Reactive.Native.dll" "$EXAMPLE_DIR/lib/"
cp "$FUSION_OUT/Alis.Reactive.Fusion.dll" "$EXAMPLE_DIR/lib/"
cp "$VALIDATOR_OUT/Alis.Reactive.FluentValidator.dll" "$EXAMPLE_DIR/lib/"

# NativeTagHelpers is part of Alis.Reactive.Native output
if [ -f "$NATIVE_OUT/Alis.Reactive.NativeTagHelpers.dll" ]; then
  cp "$NATIVE_OUT/Alis.Reactive.NativeTagHelpers.dll" "$EXAMPLE_DIR/lib/"
else
  # May be in a separate project — check common locations
  TAGHELPERSPROJECT="$REPO_ROOT/Alis.Reactive.NativeTagHelpers"
  if [ -d "$TAGHELPERSPROJECT" ]; then
    dotnet build "$TAGHELPERSPROJECT/Alis.Reactive.NativeTagHelpers.csproj" -c Release --nologo -v q
    cp "$TAGHELPERSPROJECT/bin/Release/net10.0/Alis.Reactive.NativeTagHelpers.dll" "$EXAMPLE_DIR/lib/"
  fi
fi

echo "=== Step 3: Copy JS + CSS bundles ==="
SANDBOX_WWWROOT="$REPO_ROOT/Alis.Reactive.SandboxApp/wwwroot"
mkdir -p "$EXAMPLE_DIR/wwwroot/js" "$EXAMPLE_DIR/wwwroot/css"
cp "$SANDBOX_WWWROOT/js/alis-reactive.js" "$EXAMPLE_DIR/wwwroot/js/"
cp "$SANDBOX_WWWROOT/css/design-system.css" "$EXAMPLE_DIR/wwwroot/css/"

echo "=== Step 4: Verify example app compiles ==="
dotnet build "$EXAMPLE_DIR/ResidentIntake.csproj" --nologo -v q
if [ $? -ne 0 ]; then
  echo "ERROR: Example app failed to compile with updated DLLs."
  exit 1
fi
echo "Example app compiles successfully."

echo "=== Step 5: Package zip ==="
mkdir -p "$DOWNLOADS_DIR"
# Remove old zip
rm -f "$ZIP_FILE"
# Create zip from example directory (excludes build artifacts)
cd "$EXAMPLE_DIR"
zip -r "$ZIP_FILE" . \
  -x "bin/*" "obj/*" ".DS_Store" "*.user" \
  > /dev/null
cd "$REPO_ROOT"

ZIP_SIZE=$(du -h "$ZIP_FILE" | cut -f1)
echo "=== Done ==="
echo "  DLLs:  $(ls -1 "$EXAMPLE_DIR/lib/"*.dll | wc -l | tr -d ' ') files updated"
echo "  Zip:   $ZIP_FILE ($ZIP_SIZE)"
echo "  Ready for commit."
