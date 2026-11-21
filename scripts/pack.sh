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
  1. pack AlisReactive.DesignSystem and AlisReactive.NativeTagHelpers into ./nupkgs from
     build/EmptyPackage.proj, each restored in locked mode from its own lock file,
     build/packages.<id>.lock.json
  2. the browser bundle, node build.mjs, which each package carries
  3. run dotnet build --configuration Release
  4. pack the four packages that ship code into ./nupkgs
  5. read each package's nuspec back: its id and version, its frameworks, and for
     AlisReactive.Native, AlisReactive.Fusion and AlisReactive.OpenTelemetry the
     dependency on AlisReactive at exactly this version, [<version>], excluding no
     asset; each line also names the browser files the package carries

The script clears old .nupkg/.snupkg files from ./nupkgs first so the output
list only contains packages from this run. dotnet pack never runs npm.
Exit 1 when a lock file disagrees with its project, or a package reads back other
than the six ids ship.
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

PACKAGES=(
  Alis.Reactive
  Alis.Reactive.Native
  Alis.Reactive.Fusion
  Alis.Reactive.OpenTelemetry
)

EMPTY_PACKAGES=(
  AlisReactive.DesignSystem
  AlisReactive.NativeTagHelpers
)

mkdir -p "$OUTPUT_DIR"
rm -f "$OUTPUT_DIR"/*.nupkg "$OUTPUT_DIR"/*.snupkg

for id in "${EMPTY_PACKAGES[@]}"; do
  echo "[pack] $id, restored from build/packages.$id.lock.json"
  dotnet restore build/EmptyPackage.proj --locked-mode -p:PackageId="$id"
  dotnet pack build/EmptyPackage.proj --configuration Release --no-restore --output "./$OUTPUT_DIR" -p:Version="$VERSION" -p:PackageId="$id"
done

echo "[pack] building the browser bundle"
[ -d node_modules ] || npm ci
node build.mjs

echo "[pack] compiling Release projects"
dotnet build --configuration Release

for proj in "${PACKAGES[@]}"; do
  dotnet pack "$proj/$proj.csproj" --configuration Release --no-build --output "./$OUTPUT_DIR" -p:Version="$VERSION"
done

echo "[pack] reading each package back"
python3 -I - "$OUTPUT_DIR" "$VERSION" <<'PY'
import pathlib
import sys
import xml.etree.ElementTree as ElementTree
import zipfile

folder, version = pathlib.Path(sys.argv[1]), sys.argv[2]
code = [".NETFramework4.8", "net10.0"]
exactly = f"[{version}]"
shipped = {
    "AlisReactive": (code, None),
    "AlisReactive.Native": (code, exactly),
    "AlisReactive.Fusion": (code, exactly),
    "AlisReactive.OpenTelemetry": (code, exactly),
    "AlisReactive.DesignSystem": (code, None),
    "AlisReactive.NativeTagHelpers": (["net10.0"], None),
}

problems = []
packed = sorted(path.name for path in folder.glob("*.nupkg"))
expected = sorted(f"{package}.{version}.nupkg" for package in shipped)
for name in sorted(set(packed) - set(expected)):
    problems.append(f"{folder}/{name} is packed and is none of the six ids at {version}")

for package, (frameworks, dependency) in shipped.items():
    path = folder / f"{package}.{version}.nupkg"
    if not path.exists():
        problems.append(f"{path} is not packed")
        continue
    with zipfile.ZipFile(path) as archive:
        nuspec = ElementTree.fromstring(archive.read(f"{package}.nuspec"))
        browser_files = sorted(name.removeprefix("staticwebassets/") for name in archive.namelist() if name.startswith("staticwebassets/"))
    namespace = {"nuspec": nuspec.tag.split("}")[0].strip("{")}
    metadata = nuspec.find("nuspec:metadata", namespace)
    read_id = metadata.findtext("nuspec:id", namespaces=namespace)
    read_version = metadata.findtext("nuspec:version", namespaces=namespace)
    groups = metadata.findall("nuspec:dependencies/nuspec:group", namespace)
    read_frameworks = sorted(group.get("targetFramework") for group in groups)
    on_the_core = sorted({
        item.get("version")
        for group in groups
        for item in group.findall("nuspec:dependency", namespace)
        if item.get("id") == "AlisReactive"
    })
    on_each_group = all(
        any(item.get("id") == "AlisReactive" for item in group.findall("nuspec:dependency", namespace))
        for group in groups
    )
    narrowed = sorted(
        f"{group.get('targetFramework')} " + " ".join(f'{name}="{item.get(name)}"' for name in ("include", "exclude") if item.get(name) is not None)
        for group in groups
        for item in group.findall("nuspec:dependency", namespace)
        if item.get("id") == "AlisReactive"
        and ((item.get("exclude") or "").strip() != "" or (item.get("include") or "All").strip().lower() != "all")
    )
    read = [f"{read_id} {read_version}: {', '.join(read_frameworks)}"]
    if on_the_core:
        read.append(f"AlisReactive {', '.join(on_the_core)}")
    if browser_files:
        read.append(f"browser files {', '.join(browser_files)}")
    print("[pack] " + "; ".join(read))
    if read_id != package or read_version != version:
        problems.append(f"{path} reads back as {read_id} {read_version}")
    if read_frameworks != sorted(frameworks):
        problems.append(f"{package} is packed for {', '.join(read_frameworks)} and ships for {', '.join(sorted(frameworks))}")
    if dependency is None and on_the_core:
        problems.append(f"{package} depends on AlisReactive {', '.join(on_the_core)} and ships on no package of the family")
    if dependency is not None and (on_the_core != [dependency] or not on_each_group):
        problems.append(f"{package} depends on AlisReactive at {', '.join(on_the_core) or 'no version'}; a consumer package depends on it at exactly {dependency} in each framework")
    if dependency is not None and narrowed:
        problems.append(f"{package} depends on AlisReactive with {'; '.join(narrowed)}; a consumer package passes every asset of AlisReactive to an app that names it alone: write PrivateAssets=\"none\" on its ProjectReference to Alis.Reactive.csproj")

sys.stdout.flush()
for problem in problems:
    print(f"[pack] ERROR: {problem}", file=sys.stderr)
sys.exit(1 if problems else 0)
PY

echo "Packed $((${#PACKAGES[@]} + ${#EMPTY_PACKAGES[@]})) packages to ./$OUTPUT_DIR (version $VERSION)."
ls -1 "$OUTPUT_DIR"/*.nupkg
