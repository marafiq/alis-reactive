#!/usr/bin/env bash
cd "$(dirname "$0")/.." || exit 1
set -u
test "$#" -ge 4 || { echo 'usage: land-unit.sh <worktree> <base sha> "<title>" <landing folder>...'; exit 1; }
P="$1"; BASE="$2"; title="$3"; shift 3
W=.work/logs
batch="unit-$(git rev-parse --short=9 HEAD)-$$"
test -f .work/attribution.txt || { echo "[land-unit] missing .work/attribution.txt; write the two attribution lines there"; exit 1; }
python3 -I -B - <<'ATTRIBUTION' || exit 1
from pathlib import Path
lines = Path(".work/attribution.txt").read_text().splitlines()
assert len(lines) == 2 and all(lines), ".work/attribution.txt must hold two attribution lines"
ATTRIBUTION
P="$(cd "$P" && pwd -P)" || exit 1
folders=()
for landing in "$@"; do
  case "$landing" in /*) d="$landing" ;; *) d="$P/$landing" ;; esac
  test -d "$d" || { echo "[land-unit] no landing folder: $d"; exit 1; }
  folders+=("$d")
done
mkdir -p "$W" || exit 1
[ -z "$(git diff --name-only -- . ':!HANDOFF.md')" ] || { echo "[$batch] unstaged changes before the batch"; exit 1; }
for d in "${folders[@]}"; do
  slice="$(basename "$d")"
  [ -f "$d/paths.txt" ] && [ -f "$d/message.txt" ] || { echo "[$batch] $slice: no paths.txt or message.txt"; exit 1; }
  [ "$d" = "$P/.landing/$slice" ] || { echo "[$batch] $slice: a landing folder lives under its worktree's .landing"; exit 1; }
  python3 -I -B scripts/state.py --unit "$P" "$slice" || { echo "[$batch] $slice: not ready, as scripts/state.py reads it"; exit 1; }
  while IFS= read -r path; do
    [ -n "$path" ] || continue
    if [ "$path" = "spec/MISTAKES.md" ]; then continue; fi
    case "$path" in tests/Alis.Reactive.Conformance.World/Promises.g.cs|tests/Alis.Reactive.Conformance.World/Gates.g.cs) continue;; esac
    if [ ! -e "$P/$path" ]; then git rm -q -- "$path" || exit 1; continue; fi
    atbase="$(git rev-parse -q --verify "$BASE:$path" 2>/dev/null)"; athead="$(git rev-parse -q --verify "HEAD:$path" 2>/dev/null)"
    mkdir -p "$(dirname "$path")" || exit 1
    if [ "$atbase" = "$athead" ]; then
      cp "$P/$path" "$path" || exit 1
    else
      basefile="$(mktemp "$W/base.XXXXXX")"; merged="$(mktemp "$W/merged.XXXXXX")"
      git show "$BASE:$path" > "$basefile" || exit 1
      git merge-file -p "$path" "$basefile" "$P/$path" > "$merged" || { echo "[$batch] $slice: $path conflicts with the head's change since $BASE"; exit 1; }
      cp "$merged" "$path" && rm -f "$basefile" "$merged"
      echo "[$batch] $slice: $path merged with the head's change since $BASE"
    fi
    git add -- "$path"
  done < "$d/paths.txt"
done
python3 -I -B - "$P" "$BASE" <<'PY' || exit 1
import re, sys, subprocess
from pathlib import Path
def counts(text):
    return {m.group(1).strip(): int(m.group(2)) for m in re.finditer(r"^\| ([^|]+?) \| (\d+) \|", text, re.M)}
base = counts(subprocess.run(["git", "show", sys.argv[2] + ":spec/MISTAKES.md"], capture_output=True, text=True, check=True).stdout)
built = counts(Path(sys.argv[1], "spec/MISTAKES.md").read_text())
p = Path("spec/MISTAKES.md"); t = p.read_text()
for kind in built:
    n = built[kind] - base.get(kind, built[kind])
    if n == 0: continue
    pattern = re.compile(r"^(\| " + re.escape(kind) + r" \| )(\d+)( \|)", re.M)
    assert len(pattern.findall(t)) == 1, kind
    t = pattern.sub(lambda m: f"{m.group(1)}{int(m.group(2)) + n}{m.group(3)}", t)
    print("counted", kind[:40], "+", n)
p.write_text(t)
PY
git add -- spec/MISTAKES.md
for d in "${folders[@]}"; do
  slice="$(basename "$d")"
  if [ -f "$d/spec.py" ]; then python3 -I -B "$d/spec.py" . > /dev/null || { echo "[$batch] $slice: spec.py refused"; exit 1; }; fi
done
git add -- spec
python3 -I -B - "$batch" HEAD --staged <<'HELD' || exit 1
import re
import subprocess
import sys
batch, before_ref, after_ref = sys.argv[1:4]
def text(ref):
    spec = ":spec/MISTAKES.md" if ref == "--staged" else f"{ref}:spec/MISTAKES.md"
    return subprocess.run(["git", "show", spec], capture_output=True, text=True, check=True).stdout
def rows(text):
    return {row.group(1).strip(): (int(row.group(2)), row.group(3)) for row in re.finditer(r"^\| ([^|]+?) \| (\d+) \|(.*)$", text, re.M)}
before, after = rows(text(before_ref)), rows(text(after_ref))
compared = ["--cached", before_ref] if after_ref == "--staged" else [before_ref, after_ref]
changed = set(subprocess.run(["git", "diff", "--name-only", *compared], capture_output=True, text=True, check=True).stdout.split())
stood = [kind for kind, (count, holder) in after.items() if kind in before and count > before[kind][0] and holder == before[kind][1] and not changed & set(re.findall(r"`([^`]+)`", holder))]
for kind in stood:
    print(f"[{batch}] the kind \"{kind[:50]}\" grew from {before[kind][0]} to {after[kind][0]} while its row and every file it names stood: move its holder up a rung")
sys.exit(1 if stood else 0)
HELD
python3 spec/tools/check.py --generate > /dev/null || { echo "[$batch] check.py --generate refused"; exit 1; }
git checkout -- spec/STATE.json 2>/dev/null
git add -- tests/Alis.Reactive.Conformance.World/Promises.g.cs tests/Alis.Reactive.Conformance.World/Gates.g.cs
[ -z "$(git diff --name-only -- . ':!HANDOFF.md')" ] || { echo "[$batch] unstaged changes after staging"; git diff --name-only -- . ':!HANDOFF.md'; exit 1; }
python3 -I -B - "$W/msg-$batch.txt" "$title" .work/attribution.txt "${folders[@]}" <<'PYMESSAGE' || exit 1
import sys
import re
from pathlib import Path
out, title, attribution, folders = sys.argv[1], sys.argv[2], Path(sys.argv[3]), sys.argv[4:]
parts = [title, "", "Each slice as its builder wrote it, landed as one commit by the owner's answer of 2026-11-17 (\"One commit per batch\"):"]
for folder in map(Path, folders):
    note = folder / "note.txt"
    if note.exists():
        parts += ["", note.read_text().rstrip("\n")]
    text = (folder / "message.txt").read_text().rstrip("\n")
    body = text.split("\n\nCo-Authored-By:")[0]
    body = re.sub(r"\n*^Here: .*?\{compared\}\"\.$", "", body, flags=re.S | re.M)
    parts += ["", body]
parts += ["",
          "Here: {here}",
          "",
          *attribution.read_text().splitlines()]
Path(out).write_text("\n".join(parts) + "\n")
PYMESSAGE
: > "$W/retired-$batch.txt" || exit 1
for d in "${folders[@]}"; do
  if [ -f "$d/retired.txt" ]; then cat "$d/retired.txt" >> "$W/retired-$batch.txt" || exit 1; fi
done
caffeinate -i bash scripts/land.sh "$batch" "$W/msg-$batch.txt" > "$W/land-$batch.log" 2>&1
if [ -z "$(git status --porcelain --untracked-files=no -- . ':!HANDOFF.md')" ] && [ "$(git log -1 --format=%s)" = "$title" ]; then
  echo "[$batch] landed $(git rev-parse --short=9 HEAD): $(grep -E 'selected, .* ran|^\[gate\] the Markdown path: every check passed\.$' "$W/land-$batch.log" | tail -1)"
else
  echo "[$batch] did not land; read $W/land-$batch.log"; grep -E "\[land\]|\[check\]|no test that passed|fails" "$W/land-$batch.log" | tail -4; exit 1
fi
