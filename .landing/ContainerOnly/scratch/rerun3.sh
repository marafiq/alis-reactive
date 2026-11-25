#!/usr/bin/env bash
set -uo pipefail
cd /Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/container-only
. scripts/toolchain-env.sh
out=.landing/ContainerOnly/scratch
for t in M9_F2.M9_X74_0 M9_F2.M9_X75_0 M9_F2.M9_X247_0 M9_F2.M9_X254_0; do
  caffeinate -i scripts/playwright.sh --test "$t" > "$out/rerun-$t.log" 2>&1 < /dev/null
  echo "exit $?" >> "$out/rerun-$t.log"
done
echo done > "$out/rerun.done"
