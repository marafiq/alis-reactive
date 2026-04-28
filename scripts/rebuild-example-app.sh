#!/usr/bin/env bash
# rebuild-example-app.sh — Regenerates the downloadable resident-intake zip.
# Run from repo root: ./scripts/rebuild-example-app.sh
#
# What it does:
#   1. `dotnet build examples/resident-intake/ResidentIntake.csproj`
#      NuGet restore pulls the versions pinned in the csproj
#      (e.g. AlisReactive 1.0.0-preview.2). On Build, the restored
#      AlisReactive.targets copies the NuGet's own bundles into
#      examples/resident-intake/wwwroot/ — the same code path every
#      real consumer uses. This is the authoritative source for the
#      JS + CSS shipped inside the zip. See docs-site/public/downloads/
#      for the resulting file.
#   2. Package the example directory into resident-intake.zip, excluding
#      local build artefacts.
#
# Bumping the example to a new published preview:
#   - Update PackageReference versions in examples/resident-intake/ResidentIntake.csproj
#   - Update the ~/scripts/alis-reactive.<version>.js and
#     ~/css/design-system.<version>.css references in examples/resident-intake/Views/Shared/_Layout.cshtml
#   - Re-run this script — the zip will reflect the new pinned version
#     exactly as consumers of that NuGet would see it.

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
EXAMPLE_DIR="$REPO_ROOT/examples/resident-intake"
DOWNLOADS_DIR="$REPO_ROOT/docs-site/public/downloads"
ZIP_FILE="$DOWNLOADS_DIR/resident-intake.zip"

echo "=== Step 1: Restore + build example (NuGet drives the version) ==="
dotnet build "$EXAMPLE_DIR/ResidentIntake.csproj" --nologo -v q
if [ $? -ne 0 ]; then
    echo "ERROR: example failed to compile. Fix build errors before re-running." >&2
    exit 1
fi

# Verify AlisReactive.targets produced the versioned bundles in wwwroot.
# If missing, either the pinned NuGet version is broken or AlisReactive.targets
# itself has regressed — either way, do not ship a broken zip.
JS_BUNDLE=$(ls "$EXAMPLE_DIR/wwwroot/scripts/alis-reactive."*.js 2>/dev/null | head -1)
CSS_BUNDLE=$(ls "$EXAMPLE_DIR/wwwroot/css/design-system."*.css 2>/dev/null | head -1)
FUSION_CSS_BUNDLE=$(ls "$EXAMPLE_DIR/wwwroot/css/syncfusion."*.css 2>/dev/null | head -1)
if [ -z "$JS_BUNDLE" ] || [ -z "$CSS_BUNDLE" ] || [ -z "$FUSION_CSS_BUNDLE" ]; then
    echo "ERROR: AlisReactive targets did not populate wwwroot with all expected bundles." >&2
    echo "       Expected: wwwroot/scripts/alis-reactive.<version>.js" >&2
    echo "                 wwwroot/css/design-system.<version>.css" >&2
    echo "                 wwwroot/css/syncfusion.<version>.css" >&2
    exit 1
fi
echo "  bundled JS:         $(basename "$JS_BUNDLE")"
echo "  bundled CSS:        $(basename "$CSS_BUNDLE")"
echo "  bundled Fusion CSS: $(basename "$FUSION_CSS_BUNDLE")"

echo "=== Step 2: Package zip ==="
mkdir -p "$DOWNLOADS_DIR"
rm -f "$ZIP_FILE"
cd "$EXAMPLE_DIR"
zip -r "$ZIP_FILE" . \
    -x "bin/*" "obj/*" "lib/*" ".DS_Store" "*.user" \
    > /dev/null
cd "$REPO_ROOT"

ZIP_SIZE=$(du -h "$ZIP_FILE" | cut -f1)
echo "=== Done ==="
echo "  Zip:   $ZIP_FILE ($ZIP_SIZE)"
echo "  Contains: Model, Validator, Controller, Views, Razor _Layout, pinned PackageReferences,"
echo "            and NuGet-restored alis-reactive.<version>.js + design-system.<version>.css."
