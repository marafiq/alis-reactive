#!/usr/bin/env bash
cd /Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/container-only
( cd .landing/ContainerOnly/scratch/tlc-request-mutant-others && caffeinate -i /opt/homebrew/opt/openjdk/bin/java -cp "$HOME/.local/share/tlaplus/tla2tools.jar" tlc2.TLC -deadlock -workers 2 -metadir states -config Request.cfg Request.tla < /dev/null > tlc.log 2>&1 )
echo "exit $?"
