#!/usr/bin/env bash
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
run="$here/RefusedRun"
trap 'rm -rf "$run"' EXIT
rm -rf "$run"
mkdir "$run"
cp "$here"/HttpSurface/*.cs "$here/HttpSurface/HttpSurface.csproj" "$run"/
built=$(dotnet build "$run" -nologo -v q 2>&1 | grep -cE "error CS" || true)
if [ "$built" != "0" ]; then echo "FAIL the surface does not build"; exit 1; fi
failed=0
total=0
for file in "$here"/HttpSurface/refused/*.cs.txt; do
  name=$(basename "$file" .cs.txt)
  expected=$(echo "$name" | tr '.' '\n' | grep -E '^CS[0-9]+$' | sort -u | tr '\n' ' ')
  cp "$file" "$run/Refused.cs"
  observed=$(dotnet build "$run" -nologo -v q -p:BuildProjectReferences=false 2>&1 | grep -oE "error CS[0-9]+" | sed 's/error //' | sort -u | tr '\n' ' ' || true)
  total=$((total + 1))
  if [ "$expected" = "$observed" ]; then echo "PASS $name"; else echo "FAIL $name expected [$expected] observed [$observed]"; failed=$((failed + 1)); fi
done
echo "refused: $((total - failed)) of $total fail as named"
[ "$failed" = "0" ]
