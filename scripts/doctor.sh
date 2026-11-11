#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."

usage() {
  cat <<'USAGE'
Usage:
  scripts/doctor.sh
  scripts/doctor.sh --latest

Compares every pin this machine runs with what the machine runs:
  .NET SDK   global.json                 dotnet --version
  Node       .node-version               node --version
  Python     .python-version             python3 --version
  Java       .java-version               java -version
  TLC        spec/tools/tla2tools.lock   the SHA-256 of the jar at TLA2TOOLS_JAR
  npm        package-lock.json           node_modules

--latest also compares Node, Python, Java and TLC with their registries, which
Dependabot does not read. It needs the network.

It reads and reports; it installs and changes nothing. Exit 1 on any difference.
USAGE
}

latest=0
case "${1:-}" in
  "") ;;
  --latest) latest=1 ;;
  -h|--help) usage; exit 0 ;;
  *) echo "Unexpected argument '$1'." >&2; usage >&2; exit 2 ;;
esac

failures=0
ok() { echo "[doctor:ok] $*"; }
fail() {
  echo "[doctor:fail] $*" >&2
  failures=$((failures + 1))
}

compare() {
  local tool="$1" pinned="$2" found="$3" source="$4"
  if [ "$found" = "$pinned" ]; then
    ok "$tool $found ($source)"
  else
    fail "$tool is ${found:-missing}; $source pins $pinned"
  fi
}

found_or_empty() {
  local found
  if found="$("$@" 2>&1)"; then printf '%s\n' "$found"; fi
}

sdk_pin="$(sed -n 's/.*"version": *"\([^"]*\)".*/\1/p' global.json | head -1)"
compare ".NET SDK" "$sdk_pin" "$(found_or_empty dotnet --version)" "global.json"
compare "Node" "$(cat .node-version)" "$(found_or_empty node --version | sed 's/^v//')" ".node-version"
compare "Python" "$(cat .python-version)" "$(found_or_empty python3 --version | sed 's/^Python //')" ".python-version"

java_bin="${JAVA_HOME:+$JAVA_HOME/bin/}java"
java_found="$(found_or_empty "$java_bin" -version | sed -n 's/.*version "\([0-9]*\.[0-9]*\.[0-9]*\).*/\1/p' | head -1)"
compare "Java" "$(cat .java-version)" "$java_found" ".java-version"

jar="${TLA2TOOLS_JAR:-$HOME/.local/share/tlaplus/tla2tools.jar}"
jar_pin="$(sed -n 's/.*"sha256": *"\([^"]*\)".*/\1/p' spec/tools/tla2tools.lock)"
if [ -f "$jar" ]; then
  compare "TLC jar" "$jar_pin" "$(shasum -a 256 "$jar" | cut -d' ' -f1)" "spec/tools/tla2tools.lock"
else
  fail "TLC jar is missing at $jar; spec/tools/tla2tools.lock pins $jar_pin"
fi

if [ -d node_modules ] && npm ls --all >/dev/null 2>&1; then
  ok "node_modules holds what package-lock.json locks"
else
  fail "node_modules differs from package-lock.json; run: npm ci"
fi

if [ "$latest" -eq 1 ]; then
  if ! node --input-type=module - "$(cat .node-version)" "$(cat .python-version)" "$(cat .java-version)" "$(sed -n 's/.*"release": *"\([^"]*\)".*/\1/p' spec/tools/tla2tools.lock)" <<'LATEST'
const [nodePin, pythonPin, javaPin, tlcPin] = process.argv.slice(2);
const read = async (url) => {
  const response = await fetch(url, { headers: { "User-Agent": "doctor" } });
  if (!response.ok) throw new Error(`${url} answered ${response.status}`);
  return response.json();
};
const newest = (versions) => versions
  .map((version) => version.split(".").map(Number))
  .sort((a, b) => b[0] - a[0] || b[1] - a[1] || b[2] - a[2])[0].join(".");
const final = /^\d+\.\d+\.\d+$/;
const node = newest((await read("https://nodejs.org/dist/index.json")).map((release) => release.version.slice(1)).filter((version) => final.test(version)));
const python = newest((await read("https://www.python.org/api/v2/downloads/release/?is_published=true&pre_release=false"))
  .map((release) => release.name.replace(/^Python /, "")).filter((version) => final.test(version)));
const feature = (await read("https://api.adoptium.net/v3/info/available_releases")).most_recent_feature_release;
const java = newest((await read(`https://api.adoptium.net/v3/info/release_versions?release_type=ga&version=%5B${feature}%2C${feature + 1})&page_size=50`)).versions
  .map((version) => `${version.major}.${version.minor}.${version.security}`));
const tlc = (await read("https://api.github.com/repos/tlaplus/tlaplus/releases/latest")).tag_name;
let behind = 0;
for (const [tool, pin, found] of [["Node", nodePin, node], ["Python", pythonPin, python], ["Java", javaPin, java], ["TLC", tlcPin, tlc]]) {
  if (pin === found) {
    console.log(`[doctor:ok] ${tool} ${pin} is its registry's latest stable`);
  } else {
    console.error(`[doctor:fail] ${tool} is pinned at ${pin}; its registry's latest stable is ${found}`);
    behind += 1;
  }
}
process.exit(behind === 0 ? 0 : 1);
LATEST
  then
    failures=$((failures + 1))
  fi
fi

if [ "$failures" -gt 0 ]; then
  echo "[doctor] $failures difference(s)." >&2
  exit 1
fi

echo "[doctor] every pin matches."
