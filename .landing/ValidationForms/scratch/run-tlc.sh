#!/bin/bash
# usage: run-tlc.sh <copy-dir> <spec-name> <cfg-file> <log-file>
copy="${1}"
spec="${2}"
cfg="${3}"
log="${4}"
( cd "${copy}" && /opt/homebrew/opt/openjdk/bin/java -cp "${HOME}/.local/share/tlaplus/tla2tools.jar" tlc2.TLC -deadlock -workers 2 -metadir "states-${log}" -config "${cfg}" "${spec}.tla" < /dev/null > "${log}" 2>&1 )
echo "exit $?"
grep -E "Model checking completed|is violated|states generated|Error:|error" "${copy}/${log}" | head -6
