#!/usr/bin/env bash
cd "$(dirname "$0")/.." || exit 1
set -u
test "$#" -eq 2 || { echo "usage: gate.sh <name> <full or markdown>"; exit 1; }
n="$1"; path="$2"; W=.work/logs
test "$path" = full || test "$path" = markdown || { echo "usage: gate.sh <name> <full or markdown>"; exit 1; }
mkdir -p "$W" || exit 1
test "$(git branch --show-current)" != main || { echo "gate not run: main is refused"; exit 1; }
test -z "$(git diff --name-only -- . ':!HANDOFF.md')" || { echo "gate not run: unstaged changes"; git diff --name-only -- . ':!HANDOFF.md'; exit 1; }
log="$W/gate-$n.log"
: > "$log"
say() {
  printf '%s\n' "$1" | tee -a "$log"
}
if [ "$path" = markdown ]; then
  dotnet test tests/Alis.Reactive.ArchitectureTests --filter "FullyQualifiedName~WhenARuleNamesTheCode|FullyQualifiedName~WhenADocShowsAnExample" > "$W/markdown-tests-$n.log" 2>&1
  code=$?
  say "WhenARuleNamesTheCode and WhenADocShowsAnExample: $(grep -E '^(Passed|Failed)!' "$W/markdown-tests-$n.log" | tail -1)"
  test "$code" -eq 0 && grep -q -E '^Passed!' "$W/markdown-tests-$n.log" || { echo "gate not run: the tests that read Markdown did not pass"; grep -A4 'Error Message' "$W/markdown-tests-$n.log" | head -12; exit 1; }
elif git diff --cached --name-only | grep -q -E '(^|/)CLAUDE\.md$|^\.claude/(rules|agents|skills)/'; then
  dotnet test tests/Alis.Reactive.ArchitectureTests --filter "FullyQualifiedName~WhenARuleNamesTheCode" > "$W/rule-names-$n.log" 2>&1
  code=$?
  say "rule names: $(grep -E '^(Passed|Failed)!' "$W/rule-names-$n.log" | tail -1)"
  test "$code" -eq 0 || { echo "gate not run: the rule-name test failed"; grep -A4 'Error Message' "$W/rule-names-$n.log" | head -12; exit 1; }
fi
if git diff --cached --name-only | grep -q -E '^spec/.*\.(md|json)$'; then
  python3 spec/tools/check.py --check hedges > "$W/hedges-$n.log" 2>&1
  say "hedges: $(tail -1 "$W/hedges-$n.log")"
  grep -q '"pass": true' "$W/hedges-$n.log" || { echo "gate not run: the hedges check did not pass"; exit 1; }
fi
if git diff --cached --name-only | grep -q -E '^spec/.*\.json$|\.g\.cs$'; then
  for check in defects bindings; do
    python3 spec/tools/check.py --check "$check" > "$W/$check-$n.log" 2>&1
    git checkout -- spec/STATE.json 2>/dev/null
    say "$check: $(tail -1 "$W/$check-$n.log" | cut -c1-160)"
    grep -q '"pass": true' "$W/$check-$n.log" || { echo "gate not run: the $check check did not pass"; exit 1; }
  done
fi
if git diff --cached --name-status | grep -q -E '^A'; then
  python3 spec/tools/check.py --check paths > "$W/paths-$n.log" 2>&1
  git checkout -- spec/STATE.json 2>/dev/null
  say "paths: $(tail -1 "$W/paths-$n.log" | cut -c1-160)"
  grep -q '"pass": true' "$W/paths-$n.log" || { echo "gate not run: a file the commit adds has no verdict in spec/REWRITE.md"; exit 1; }
fi
if [ "$path" = markdown ]; then
  python3 scripts/state.py > "$W/state-$n.log" 2>&1 || { echo "gate not run: scripts/state.py did not read the process"; tail -5 "$W/state-$n.log"; exit 1; }
  say "python3 scripts/state.py: exit 0"
  say "[gate] the Markdown path: every check passed."
  exit 0
fi
caffeinate -i scripts/check.sh --no-spec >> "$log" 2>&1
tail -1 "$log"
