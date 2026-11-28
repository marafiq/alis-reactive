#!/usr/bin/env bash
set -u
case "${1:-}" in STOP|CONT) ;; *) echo "usage: tlc.sh STOP|CONT" >&2; exit 2 ;; esac
pids="$(pgrep -f 'tlc2.TL[C]' | tr '\n' ' ')"
if [ -n "$pids" ]; then kill "-$1" $pids || exit 1; fi
for pid in $pids; do printf '[tlc] %s %s: %s\n' "$1" "$pid" "$(ps -o stat= -p "$pid")"; done
[ -n "$pids" ] || echo "[tlc] $1: no TLC running"
