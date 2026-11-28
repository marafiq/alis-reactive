import os
import re
import subprocess
import sys
from pathlib import Path

root = Path(__file__).resolve().parent.parent
os.chdir(root)
if sys.argv[1:] not in ([], ["--print"]):
    raise SystemExit("usage: handoff.py [--print]")


def command(*args):
    return subprocess.run(args, stdin=subprocess.DEVNULL, capture_output=True, text=True, check=True).stdout


def git(*args):
    return command("git", "--no-optional-locks", *args).rstrip("\n")


handoff = root / "HANDOFF.md"
next_text = "\nThe lead writes the queue here in the plan's words.\n"
notes_text = "\nNone open.\n"
if handoff.exists():
    with handoff.open(newline="") as existing:
        previous = existing.read()
    starts = list(re.finditer(r"^<!-- next -->\r?$", previous, re.M))
    ends = list(re.finditer(r"^<!-- /next -->\r?$", previous, re.M))
    if len(starts) != 1 or len(ends) != 1 or starts[0].end() >= ends[0].start():
        raise SystemExit("HANDOFF.md must hold one ordered pair of next markers")
    next_text = previous[starts[0].end():ends[0].start()]
    held = re.search(r"^<!-- notes -->\r?$(.*?)^<!-- /notes -->\r?$", previous, re.M | re.S)
    notes_text = held.group(1) if held else notes_text

def condensed(machine):
    process = Path(".claude/rules/process.md").read_text()
    unit_states = sorted(set(re.findall(r"^\| (unit: [a-z ]+?) \|", process, re.M)), key=len, reverse=True)
    items = []
    for line in machine.splitlines():
        if line.startswith("  ") and items:
            items[-1][1].append(line)
        else:
            items.append((line, []))
    notes, units, kept = {}, {}, []
    for head, details in items:
        note = re.match(r"(note: \w+) ", head)
        unit = next((state for state in unit_states if head.startswith(state + " ")), None)
        if note:
            notes.setdefault(note.group(1), []).append(head)
        elif unit:
            units.setdefault(unit, []).append(head[len(unit) + 1:].split(" ")[1].rstrip(":"))
        else:
            kept.append("\n".join([head, *details]))
    lines = [f"{state}: {len(heads)}, each in the notes block below" if state == "note: open" else "\n".join(heads) for state, heads in notes.items()]
    lines += [f"{state} ({len(names)}): {', '.join(names)}" for state, names in units.items()]
    if units:
        lines.append("Each unit's worktree, reason and next command: `python3 scripts/state.py`.")
    return "\n".join(lines + kept)


def build():
    branch = git("branch", "--show-current") or "detached HEAD"
    head = git("rev-parse", "--short=9", "HEAD")
    subject = git("log", "-1", "--format=%s")
    date = command("date").rstrip("\n")
    logs = list(Path(".work/logs").glob("spec-check-*.log"))
    full_check = "No .work/logs/spec-check-*.log exists."
    if logs:
        newest = max(logs, key=lambda path: (path.stat().st_mtime_ns, path.name))
        lines = newest.read_text().splitlines()
        verdict = next((line for line in lines if line.startswith("spec check at ")), "(no result line)")
        full_check = f"{newest}: {verdict}"
    last_run = Path(".work/last-run.txt")
    browser_run = last_run.read_text().rstrip("\n") if last_run.exists() else "missing"
    state = git("log", "-1", "--format=%H", "--grep=^spec: STATE")
    landed_args = ["log", "--format=%h %s"]
    if state:
        parents = git("rev-list", "--parents", "-n", "1", state).split()
        if len(parents) > 1:
            landed_args.append(f"{parents[1]}..HEAD")
    landed = git(*landed_args)
    opened = (command(sys.executable, "scripts/open.py").splitlines() or ["open: nothing"])[0] + "\nEach, by file and line: `python3 scripts/open.py`."
    machine = condensed(command(sys.executable, "scripts/state.py").rstrip("\n"))
    decisions = []
    for number, line in enumerate(Path("spec/CLAUDE.md").read_text().splitlines(), 1):
        dated = re.match(r"^- The owner, (\d{4}-\d{2}-\d{2})", line)
        if dated:
            decisions.append((dated.group(1), number, line))
    latest = max((dated for dated, _, _ in decisions), default="")
    owners = [f"spec/CLAUDE.md:{number}: {line[:140]}" for dated, number, line in decisions if dated == latest]
    body = "\n".join([
        "# Handoff",
        "",
        f"Written at `date`: {date}",
        "",
        "## State, as `scripts/state.py` reads it from git",
        machine,
        "",
        "## Where it stands",
        f"- Branch: {branch}",
        f"- HEAD: {head} {subject}",
        f"- Full check: {full_check}",
        f"- .work/last-run.txt: {browser_run}",
        "",
        "## Landed since the commit before the last spec: STATE commit",
        landed,
        "",
        "## Open",
        opened,
        "",
        "## The owner's latest decisions",
        "\n".join(owners) if owners else "No dated owner decision is recorded.",
        "",
        "## The owner's notes that are open: date | kind | home | follow-up | the owner's words",
        "<!-- notes -->" + notes_text + "<!-- /notes -->",
        "",
        "## Next",
        "<!-- next -->" + next_text + "<!-- /next -->",
        "",
    ])
    return body
if sys.argv[1:] == ["--print"]:
    print(build(), end="")
else:
    handoff.write_bytes(build().encode())
    body = build()
    handoff.write_bytes(body.encode())
    print(f"HANDOFF.md: {len(body.splitlines())} lines written")
