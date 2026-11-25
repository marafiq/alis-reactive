#!/usr/bin/env bash
set -uo pipefail
cd /Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/container-only
. scripts/toolchain-env.sh
out=.landing/ContainerOnly/scratch
caffeinate -i dotnet build tests/Alis.Reactive.Conformance.App.Net48 -c Release > "$out/final-net48-build.log" 2>&1 < /dev/null
echo "exit $?" >> "$out/final-net48-build.log"
caffeinate -i scripts/playwright.sh --all > "$out/final-pw-all.log" 2>&1 < /dev/null
echo "exit $?" >> "$out/final-pw-all.log"
caffeinate -i python3 spec/tools/check.py > "$out/final-spec-check.log" 2>&1 < /dev/null
echo "exit $?" >> "$out/final-spec-check.log"
echo done > "$out/final.done"
