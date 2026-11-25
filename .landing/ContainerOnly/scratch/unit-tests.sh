#!/usr/bin/env bash
set -uo pipefail
cd /Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/container-only
. scripts/toolchain-env.sh
out=.landing/ContainerOnly/scratch
caffeinate -i dotnet test tests/Alis.Reactive.ModuleTests > "$out/final-module.log" 2>&1 < /dev/null
echo "exit $?" >> "$out/final-module.log"
caffeinate -i dotnet test tests/Alis.Reactive.ArchitectureTests > "$out/final-arch.log" 2>&1 < /dev/null
echo "exit $?" >> "$out/final-arch.log"
caffeinate -i node node_modules/vitest/vitest.mjs run > "$out/final-vitest.log" 2>&1 < /dev/null
echo "exit $?" >> "$out/final-vitest.log"
