#!/usr/bin/env bash
set -uo pipefail
cd /Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/container-only
. scripts/toolchain-env.sh
export GIT_INDEX_FILE=/Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/container-only/tests/Alis.Reactive.ArchitectureTests/obj/private-index
caffeinate -i dotnet test tests/Alis.Reactive.ArchitectureTests > /Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/container-only/.landing/ContainerOnly/scratch/arch-priv2.log 2>&1 < /dev/null
echo "exit $?" >> /Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/container-only/.landing/ContainerOnly/scratch/arch-priv2.log
