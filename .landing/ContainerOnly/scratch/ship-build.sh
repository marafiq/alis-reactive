#!/usr/bin/env bash
set -uo pipefail
cd /Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/container-only
. scripts/toolchain-env.sh
out=.landing/ContainerOnly/scratch
for p in Alis.Reactive Alis.Reactive.Native Alis.Reactive.Fusion; do
  caffeinate -i dotnet build "$p" -c Release > "$out/ship-build-$p.log" 2>&1 < /dev/null
  echo "exit $?" >> "$out/ship-build-$p.log"
done
