#!/usr/bin/env bash
set -uo pipefail
cd /Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/container-only
. scripts/toolchain-env.sh
log="$1"
shift
caffeinate -i "$@" > "$log" 2>&1 < /dev/null
echo "exit $?" >> "$log"
