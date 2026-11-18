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
if handoff.exists():
    with handoff.open(newline="") as existing:
        previous = existing.read()
    starts = list(re.finditer(r"^<!-- next -->\r?$", previous, re.M))
    ends = list(re.finditer(r"^<!-- /next -->\r?$", previous, re.M))
    if len(starts) != 1 or len(ends) != 1 or starts[0].end() >= ends[0].start():
        raise SystemExit("HANDOFF.md must hold one ordered pair of next markers")
    next_text = previous[starts[0].end():ends[0].start()]

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
opened = command(sys.executable, "scripts/open.py").rstrip("\n")
unlanded = []
for entry in git("worktree", "list", "--porcelain", "-z").split("\0\0"):
    fields = dict(field.split(" ", 1) for field in entry.split("\0") if " " in field)
    if "worktree" not in fields:
        continue
    worktree = Path(fields["worktree"])
    if worktree.resolve() == root:
        continue
    changed = git("-C", str(worktree), "status", "--porcelain").splitlines()
    if not changed:
        continue
    base = git("merge-base", "HEAD", fields["HEAD"])
    relative = os.path.relpath(worktree, root)
    unlanded.append(f"- {relative}: base {base}, {len(changed)} paths changed")
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
    "## Unlanded work",
    "\n".join(unlanded) if unlanded else "No other worktree has uncommitted paths.",
    "",
    "## The owner's latest decisions",
    "\n".join(owners) if owners else "No dated owner decision is recorded.",
    "",
    "## Next",
    "<!-- next -->" + next_text + "<!-- /next -->",
    "",
])
if sys.argv[1:] == ["--print"]:
    print(body, end="")
else:
    handoff.write_bytes(body.encode())
    print(f"HANDOFF.md: {len(body.splitlines())} lines written")
