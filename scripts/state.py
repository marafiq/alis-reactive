import os
import re
import subprocess
import sys
from pathlib import Path

root = Path(__file__).resolve().parent.parent
os.chdir(root)
asked = sys.argv[1:]
if asked and not (len(asked) == 3 and asked[0] == "--unit"):
    raise SystemExit("usage: state.py [--unit <worktree> <landing folder's name>]")

PROCESS = Path(".claude/rules/process.md")
BRIEF_PARTS = ("Scope", "Rules", "Outcome", "Done", "Commands", "Reply")
GUARDS = {}


def guard(name):
    def keep(check):
        GUARDS[name] = check
        return check
    return keep


def git(*args, cwd=None):
    done = subprocess.run(["git", "--no-optional-locks", *args], stdin=subprocess.DEVNULL, capture_output=True, text=True, cwd=cwd)
    return done.stdout.rstrip("\n")


def blob(path):
    done = subprocess.run(["git", "--no-optional-locks", "show", f"HEAD:{path}"], stdin=subprocess.DEVNULL, capture_output=True)
    return done.stdout if done.returncode == 0 else None


def running(script):
    return subprocess.run(["pgrep", "-f", f"scripts/{script}"], stdin=subprocess.DEVNULL, capture_output=True).returncode == 0


def table(heading):
    section = PROCESS.read_text().split(f"\n## {heading}\n", 1)[1].split("\n## ", 1)[0]
    rows = [[cell.strip() for cell in line.strip().strip("|").split("|")] for line in section.splitlines() if line.startswith("|")]
    return rows[2:]


def part(brief, heading):
    return brief.split(f"\n## {heading}\n", 1)[1].split("\n## ", 1)[0] if f"\n## {heading}\n" in brief else ""


@guard("always")
def always(item):
    return True, ""


@guard("note-lacks-a-part")
def note_lacks_a_part(note):
    parts = note["parts"]
    lacks = len(parts) < 5 or any(not text or text == "?" for text in parts[1:4])
    return lacks, "it names no kind, home or follow-up" if lacks else ""


@guard("note-follow-up-landed")
def note_follow_up_landed(note):
    return note["parts"][3] in note["landed"], f"its follow-up {note['parts'][3]} landed"


@guard("paths-equal-head")
def paths_equal_head(unit):
    return unit["equal"], ""


@guard("brief-six-parts")
def brief_six_parts(unit):
    lacks = [name for name in BRIEF_PARTS if not re.search(rf"^## {name}$", unit["brief"], re.MULTILINE)]
    return not lacks, f"brief.md lacks {', '.join(lacks)}" if lacks else ""


@guard("rules-name-files")
def rules_name_files(unit):
    named = re.findall(r"[\w./-]+\.(?:md|tla|json|txt|cs|ts|py|sh|cshtml)\b", part(unit["brief"], "Rules"))
    absent = [path for path in named if not (unit["tree"] / path).is_file()]
    return bool(named) and not absent, "its Rules name no file" if not named else f"its Rules name {', '.join(absent[:4])}, which the worktree lacks"


@guard("base-line")
def base_line(unit):
    stands = re.search(r"^At the base:", part(unit["brief"], "Done"), re.MULTILINE) is not None
    return stands, "" if stands else "its Done holds no line \"At the base:\""


@guard("verdict-drop")
def verdict_drop(unit):
    return unit["verdict"].startswith("verdict: drop"), unit["verdict"]


@guard("verdict-back")
def verdict_back(unit):
    return unit["verdict"].startswith("verdict: back"), unit["verdict"]


@guard("reply-unread")
def reply_unread(unit):
    return unit["files"]["reply.md"] and unit["verdict"] != "verdict: land", "reply.md waits for read.md"


@guard("after-unlanded")
def after_unlanded(unit):
    waits = [name for name in re.findall(r"^After: (\w+)", unit["brief"], re.MULTILINE) if name in unit["unlanded"]]
    return bool(waits), f"for {', '.join(waits)}"


@guard("built")
def built(unit):
    return unit["files"]["paths.txt"] and unit["files"]["message.txt"], "it lacks paths.txt or message.txt"


@guard("tree-changed")
def tree_changed(unit):
    return bool(git("status", "--porcelain", cwd=unit["tree"])), ""


@guard("land-running")
def land_running(lane):
    return running("land.sh"), ""


@guard("check-running")
def check_running(lane):
    return running("full-check.sh"), ""


@guard("push-running")
def push_running(lane):
    return running("push.sh"), ""


@guard("tracked-changed")
def tracked_changed(lane):
    changed = [line[3:] for line in git("status", "--porcelain", "--untracked-files=no", "--", ".", ":!spec/STATE.json", ":!HANDOFF.md").splitlines()]
    return bool(changed), f"changed: {', '.join(changed[:6])}"


@guard("head-is-remote")
def head_is_remote(lane):
    return lane["head"] == lane["remote"], "equal to the remote"


@guard("state-commit-unpushed")
def state_commit_unpushed(lane):
    return git("log", "-1", "--format=%s").startswith("spec: STATE and handoff at "), f"the remote is at {lane['remote']}"


@guard("check-log-absent")
def check_log_absent(lane):
    return not lane["log"].exists(), f"the remote is at {lane['remote']}"


@guard("check-passed")
def check_passed(lane):
    verdict = next((line for line in lane["log"].read_text().splitlines() if line.startswith(f"spec check at {lane['head']}: ")), "")
    passed = re.fullmatch(rf"spec check at {lane['head']}: (\d+) of (\d+) checks pass", verdict)
    return bool(passed) and passed.group(1) == passed.group(2), verdict or f"no result line in {lane['log'].as_posix()}"


@guard("ci-unread")
def ci_unread(ci):
    return not ci["lines"], ""


@guard("ci-failed")
def ci_failed(ci):
    failed = [line for line in ci["lines"][1:] if line.endswith(": failure")]
    return bool(failed), "; ".join([ci["lines"][0], *failed])


@guard("ci-running")
def ci_running(ci):
    return any(line.endswith((": running", ": queued")) for line in ci["lines"][1:]), ci["lines"][0]


@guard("file-over-its-limit")
def file_over_its_limit(file):
    return file["lines"] > file["limit"], f"holds {file['lines']} lines, its limit {file['limit']}"


@guard("ten-units-since-a-change")
def ten_units_since_a_change(machine):
    return machine["landed"] >= 10, f"{machine['landed']} units have landed since {PROCESS.as_posix()} last changed"


TRACKS = {name: table(f"Track: {heading}") for name, heading in (
    ("note", "a note of the owner's"), ("unit", "a unit"), ("lane", "the lane"), ("ci", "CI at the pushed commit"),
    ("file", "a file a reader is given"), ("machine", "the machine itself"))}
named = {term.removeprefix("not ") for rows in TRACKS.values() for row in rows for both in row[1].split(" or ") for term in both.split(" and ")}
stated = {row[0] for row in table("Guards")}
if not named == stated == set(GUARDS):
    raise SystemExit(f"{PROCESS.as_posix()} and scripts/state.py differ on the guards: {sorted((named | stated) ^ set(GUARDS)) + sorted(named ^ stated)}")


def holds(term, item):
    held, note = GUARDS[term.removeprefix("not ")](item)
    return (not held, note) if term.startswith("not ") else (held, note)


def state_of(track, item):
    for row in TRACKS[track]:
        for both in row[1].split(" or "):
            terms = [holds(term, item) for term in both.split(" and ")]
            if all(held for held, _ in terms):
                return row, "; ".join(note for _, note in terms if note)
    raise SystemExit(f"{PROCESS.as_posix()}: no row of the track {track} holds")


def units():
    found = []
    for entry in git("worktree", "list", "--porcelain", "-z").split("\0\0"):
        fields = dict(field.split(" ", 1) for field in entry.split("\0") if " " in field)
        tree = Path(fields.get("worktree", root))
        if tree.resolve() == root or not (tree / ".landing").is_dir():
            continue
        for folder in sorted(path for path in (tree / ".landing").iterdir() if path.is_dir()):
            files = {file: (folder / file).is_file() for file in ("brief.md", "paths.txt", "message.txt", "reply.md", "read.md")}
            paths = [line for line in (folder / "paths.txt").read_text().splitlines() if line] if files["paths.txt"] else []
            equal = bool(paths) and files["message.txt"] and all(((tree / path).read_bytes() if (tree / path).is_file() else None) == blob(path) for path in paths)
            verdict = ""
            if files["read.md"]:
                verdict = next((line for line in (folder / "read.md").read_text().splitlines() if line.startswith("verdict: ")), "")
            found.append({"tree": tree, "worktree": os.path.relpath(tree, root), "name": folder.name, "files": files, "equal": equal, "verdict": verdict,
                          "brief": (folder / "brief.md").read_text() if files["brief.md"] else "",
                          "base": git("rev-parse", "--short=9", "HEAD", cwd=tree),
                          "title": (folder / "message.txt").read_text().splitlines()[0] if files["message.txt"] else ""})
    unlanded = {unit["name"] for unit in found if not unit["equal"]}
    for unit in found:
        unit["unlanded"] = unlanded
    return found


def show(row, subject, note, item, seen):
    state, _, read, leaves, exit_line, goes, _ = row
    print(f"{state} {subject}{': ' + note if note else ''}")
    command = re.search(r"`([^`]*\{[^`]*\}[^`]*)`", leaves)
    if command:
        print(f"  next: {command.group(1).format(**{key: value for key, value in item.items() if isinstance(value, str)})}")
    if state not in seen:
        seen.add(state)
        print(f"  leaves by: {leaves}")
        if read != "nothing":
            print(f"  read first: {read}")
        if exit_line != "none":
            print(f"  exit line: {exit_line}")
        print(f"  goes to: {goes}")


built_units = units()
if asked:
    wanted = (os.path.relpath(Path(asked[1]).resolve(), root), asked[2])
    match = [unit for unit in built_units if (unit["worktree"], unit["name"]) == wanted]
    if not match:
        raise SystemExit(f"no landing folder {wanted[1]} in {wanted[0]}")
    row, note = state_of("unit", match[0])
    raise SystemExit(0 if row[0] == "unit: ready" else f"{row[0]} {wanted[0]} {wanted[1]}{': ' + note if note else ''}")

seen = set()
landed = {unit["name"] for unit in built_units if unit["equal"]}
handoff = Path("HANDOFF.md").read_text() if Path("HANDOFF.md").is_file() else ""
block = re.search(r"^<!-- notes -->$(.*?)^<!-- /notes -->$", handoff, re.MULTILINE | re.DOTALL)
for line in (block.group(1).splitlines() if block else []):
    if line.startswith("- "):
        note = {"parts": [text.strip() for text in line[2:].split(" | ")], "landed": landed}
        row, why = state_of("note", note)
        show(row, line[2:110], why, note, seen)
for unit in built_units:
    row, why = state_of("unit", unit)
    show(row, f"{unit['worktree']} {unit['name']}", why, unit, seen)
branch = git("branch", "--show-current") or "detached HEAD"
head = git("rev-parse", "--short=9", "HEAD")
lane = {"branch": branch, "head": head, "remote": git("rev-parse", "--short=9", "-q", "--verify", f"refs/remotes/origin/{branch}") or "no remote",
        "log": Path(".work/logs") / f"spec-check-{head}.log"}
row, why = state_of("lane", lane)
show(row, f"at {head} on {branch}", why, lane, seen)
reading = Path(".work") / f"ci-{lane['remote']}.txt"
ci = {"lines": reading.read_text().splitlines() if reading.is_file() else []}
row, why = state_of("ci", ci)
show(row, f"at {lane['remote']}", why, ci, seen)
for name, _, limit in table("The files a reader is given"):
    path = Path(name.strip("`"))
    if path.is_file():
        file = {"lines": len(path.read_text().splitlines()), "limit": int(limit)}
        if GUARDS["file-over-its-limit"](file)[0]:
            row, why = state_of("file", file)
            show(row, path.as_posix(), why, file, seen)
changed = git("log", "-1", "--format=%H", "--", PROCESS.as_posix())
machine = {"landed": int(git("rev-list", "--count", "--invert-grep", "--grep=^spec: STATE", f"{changed}..HEAD") or 0) if changed else 0}
if GUARDS["ten-units-since-a-change"](machine)[0]:
    row, why = state_of("machine", machine)
    show(row, "", why, machine, seen)
