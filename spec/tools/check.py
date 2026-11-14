#!/usr/bin/env python3
import hashlib
import io
import fnmatch
import itertools
import json
import os
import platform
import tokenize
import re
import shutil
import subprocess
import sys
import tempfile
from collections import Counter
from concurrent.futures import ThreadPoolExecutor
from pathlib import Path, PurePosixPath

SPEC = Path(__file__).resolve().parent.parent
REPO = SPEC.parent
STATE = SPEC / "STATE.json"
MAIN_SHA = "623dd4a272f8134fb692985fd04ae7151954c9f0"
BASE_SHA = "65fdf118d1498a8285db84a43c4da81dfaa0ea3e"
MAIN_TIP = "7b9e987cc3b9991c1107aa782bc3f6e8486968f9"
SPEC_BRANCH = "feature/rewrite"
JAVA = str(Path(os.environ["JAVA_HOME"]) / "bin" / "java") if os.environ.get("JAVA_HOME") else "/opt/homebrew/opt/openjdk/bin/java"
TLA_JAR = Path(os.environ["TLA2TOOLS_JAR"]) if os.environ.get("TLA2TOOLS_JAR") else Path.home() / ".local/share/tlaplus/tla2tools.jar"
TLA_LOCK = SPEC / "tools" / "tla2tools.lock"
TLC_GREEN = "Model checking completed. No error has been found."
LOCAL_SETTINGS = Path("/Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.claude/settings.local.json")
HOOK_COMMAND = "cd /Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/rewrite; python3 spec/tools/check.py --orient"
CI_SKIPPED = ("survival", "probes")
CI_TOOLS = ("JAVA_HOME", "TLA2TOOLS_JAR")
CI_CACHE = "TLC_CACHE"
SKIPPED = ()
HANDOFF = Path.home() / ".claude/projects/-Users-muhammadadnanrafiq-Documents-Code-alis-reactive-framework-1-0-Alis-Reactive/memory/project-spec-first-branch.md"
OWNER_LINE = re.compile(r"^- The owner, (\d{4}-\d{2}-\d{2})")

MAIN_PACKAGES = {
    "Alis.Reactive": ["net48", "net10.0"],
    "Alis.Reactive.Fusion": ["net48", "net10.0"],
    "Alis.Reactive.Native": ["net48", "net10.0"],
    "Alis.Reactive.FluentValidator": ["net48", "net10.0"],
    "Alis.Reactive.NativeTagHelpers": ["net10.0"],
    "Alis.Reactive.DesignSystem": ["net48", "net10.0"],
}
INVENTORY_FILES = ["doors.tsv", "kinds.tsv", "cases.tsv", "analyzers.tsv", "tests.tsv", "plugins.tsv", "browser.tsv", "runtime.tsv"]
QUESTIONS = [f"Q{n}" for n in range(1, 22)]
DESIGN_PARTS = ["Decision", "Schema nodes", "Checker", "Doors", "Evidence", "Defects"]
TLC_SPECS = {
    "Absence": {"question": "Q4", "step": 4, "expect": "green"},
    "Request": {"question": "Q9 Q10", "step": 4, "expect": "green"},
    "Lanes": {"question": "Q11", "step": 4, "expect": "green"},
    "Run": {"question": "Q11", "step": 4, "expect": "green"},
    "Ownership": {"question": "Q12", "step": 4, "expect": "green"},
    "CallbackProperty": {"question": "Q17", "step": 4, "expect": "green"},
    "DateWire": {"question": "date wire form", "step": 4, "expect": "green"},
    "Schema": {"question": "Q19", "step": 5, "expect": "green"},
    "ValidationToday": {"question": "Q16 at bff9eb6c0", "step": 5, "expect": "counterexamples", "gaps": 4},
    "Validation": {"question": "Q16", "step": 5, "expect": "green"},
    "Tracing": {"question": "Q20", "step": 5, "expect": "green"},
    "Composition": {"question": "Q10 with Q11 and Q12", "step": 5, "expect": "green"},
    "Realtime": {"question": "Q21 with Q10 and Q12", "step": 5, "expect": "green"},
    "Partials": {"question": "Q14 with Q13 and Q16", "step": 5, "expect": "green"},
    "ConfirmAsks": {"question": "Q11 with Q17", "step": 5, "expect": "green"},
    "ContainerRead": {"question": "Q13 with Q12", "step": 5, "expect": "green"},
    "SlotHolds": {"question": "Q12 with Q14", "step": 5, "expect": "green"},
}
PLUGIN_DATA_DISPATCH = "pluginNamed(locator.name).contribute"
HEDGES = [
    "rare", "rarely", "usually", "may", "might", "maybe", "perhaps", "possibly", "probably", "likely",
    "typically", "generally", "often", "sometimes", "ideally", "should", "etc", "tbd", "todo",
    "when needed", "where possible", "if possible", "as needed", "as appropriate", "for now",
]
HEDGE = re.compile(r"\b(" + "|".join(re.escape(word).replace(r"\ ", r"\s+") for word in HEDGES) + r")\b", re.IGNORECASE)
OWNER_TEXTS = {"spec/BRIEF.md", "spec/NOTES.md"}
CODE_KEYS = {
    "door", "claims", "ts", "expect", "member", "type", "cite", "source", "csharp", "js", "id", "for", "probe", "file",
    "old", "new", "kind", "feature", "screen", "hosts", "parts", "harness", "line",
}
QUOTED_LINES = {"main", "redesign"}


def git(*args):
    return subprocess.run(["git", "-C", str(REPO), *args], capture_output=True, text=True).stdout.rstrip("\n")


def result(passed, detail, **counts):
    return {"pass": passed, "detail": detail, "counts": counts}


def not_started(*paths):
    missing = [str(path.relative_to(REPO)) for path in paths if not path.exists()]
    return result(False, "not started: " + ", ".join(missing) + " absent")


def not_built(what):
    return result(False, "input present; the check for " + what + " is not built")


def prose_files():
    listed = git("ls-files", "--cached", "--others", "--exclude-standard", "spec").splitlines()
    return sorted(REPO / name for name in listed if name.endswith(".md") and name not in OWNER_TEXTS)


def blanked(match):
    return re.sub(r"[^\n]", " ", match.group(0))


def without_code(text):
    text = re.sub(r"```.*?```", blanked, text, flags=re.DOTALL)
    return re.sub(r"`[^`\n]*`", blanked, text)


def json_prose(node, key="", holder=None):
    if isinstance(node, dict):
        for name, value in node.items():
            yield from json_prose(value, name, node)
    elif isinstance(node, list):
        for value in node:
            yield from json_prose(value, key, holder)
    elif isinstance(node, str) and key not in CODE_KEYS and not (key == "text" and holder.get("line") in QUOTED_LINES):
        yield node


def check_survival():
    needed = [SPEC / "BRIEF.md", SPEC / "NOTES.md", SPEC / "CLAUDE.md", SPEC / "tools" / "check.py"]
    if any(not path.exists() for path in needed):
        return not_started(*needed)
    problems = []
    try:
        hooks = json.loads(LOCAL_SETTINGS.read_text()).get("hooks", {}).get("SessionStart", [])
    except (OSError, json.JSONDecodeError) as error:
        hooks = []
        problems.append(f"{LOCAL_SETTINGS} unreadable: {error}")
    installed = [
        hook for entry in hooks if entry.get("matcher") == "compact|resume"
        for hook in entry.get("hooks", []) if hook.get("command") == HOOK_COMMAND
    ]
    if not installed:
        problems.append("the compact|resume SessionStart hook is not installed in " + str(LOCAL_SETTINGS))
    if not HANDOFF.is_file():
        problems.append(f"the handoff the compact hook prints, {HANDOFF}, is missing")
    if not git("rev-parse", "--verify", "-q", f"refs/remotes/origin/{SPEC_BRANCH}"):
        problems.append(f"origin/{SPEC_BRANCH} is not pushed")
    tracked = set(git("ls-files", "spec").splitlines())
    untracked = [str(path.relative_to(REPO)) for path in needed if str(path.relative_to(REPO)) not in tracked]
    if untracked:
        problems.append("untracked: " + ", ".join(untracked))
    return result(not problems, "; ".join(problems) or "brief, notes, map, check, hook and branch in place", files=len(needed))


def check_main_surface():
    folder = SPEC / "inventory" / "main"
    texts = {(package, tfm): folder / package / tfm / "PublicAPI.Unshipped.txt" for package, tfms in MAIN_PACKAGES.items() for tfm in tfms}
    if not folder.exists():
        return not_started(folder)
    counts = {}
    missing = []
    for (package, tfm), path in texts.items():
        if not path.exists():
            missing.append(f"{package}/{tfm}")
            continue
        counts[f"{package} {tfm}"] = sum(1 for line in path.read_text().splitlines() if line.strip() and line.strip() != "#nullable enable")
    empty = [name for name, count in counts.items() if count == 0]
    passed = not missing and not empty
    detail = "; ".join(filter(None, ["missing " + ", ".join(missing) if missing else "", "empty " + ", ".join(empty) if empty else ""]))
    return result(passed, detail or "main's surface written for every package and target framework", **counts)


REDESIGN_PACKAGES = {
    "Alis.Reactive": ["net48", "net10.0"],
    "Alis.Reactive.Fusion": ["net48", "net10.0"],
    "Alis.Reactive.Native": ["net48", "net10.0"],
    "Alis.Reactive.NativeTagHelpers": ["net10.0"],
}
TEST_ATTRIBUTE = re.compile(r"\[\s*(Test|TestCase|TestCaseSource)(?!\w)")


def table(name):
    lines = (SPEC / "inventory" / name).read_text().splitlines()
    header = lines[0].split("\t")
    return [dict(zip(header, line.split("\t"))) for line in lines[1:]]


def door_lines(text):
    return [line for line in text.splitlines() if line.strip() and line.strip() != "#nullable enable"]


LINE_SHAS = {"redesign": BASE_SHA, "main": MAIN_SHA}
MAIN_PLAN_TS = "Alis.Reactive.Assets/runtime/types/plan.ts"
KIND_READERS = {
    ("redesign", "grammar"): lambda: grammar_kinds(BASE_SHA),
    ("redesign", "plan.schema.json"): lambda: schema_json_kinds(BASE_SHA),
    ("main", "plan.ts"): lambda: set(re.findall(r'\bkind:\s*"(\w+)"', git("show", f"{MAIN_SHA}:{MAIN_PLAN_TS}"))),
}
ANALYZER_DECLARATION = re.compile(r'(\w+) = "(ALIS\d{3})"')
ANALYZER_STATUS = {(True, True): "both", (True, False): "redesign-only", (False, True): "main-only"}
CASE_LABEL = re.compile(r'case "([^"]+)"')


def inventory_door_problems(doors):
    sides = [
        ("redesign", REDESIGN_PACKAGES, "package", "redesign_line",
         lambda package, tfm: [line for name in ["PublicAPI.Unshipped.txt", "PublicAPI.Shipped.txt"]
                               for line in door_lines(git("show", f"{BASE_SHA}:{package}/PublicAPI/{tfm}/{name}"))]),
        ("main", MAIN_PACKAGES, "main_package", "main_line",
         lambda package, tfm: door_lines((SPEC / "inventory" / "main" / package / tfm / "PublicAPI.Unshipped.txt").read_text())),
    ]
    problems = []
    for side, packages, holder, column, source_lines in sides:
        for package, tfms in packages.items():
            for tfm in tfms:
                source = Counter(source_lines(package, tfm))
                rows = Counter(row[column] for row in doors if row[holder] == package and row["tfm"] == tfm and row[column])
                unheld, unsourced = source - rows, rows - source
                if unheld or unsourced:
                    first = next(iter(unheld or unsourced))
                    problems.append(f"{side} {package} {tfm}: {sum(unheld.values())} door lines no row holds and "
                                    f"{sum(unsourced.values())} rows no door line holds, first {first[:80]!r}")
    return problems


def inventory_kind_problems(kinds):
    held = {(row["line"], row["source"]) for row in kinds}
    problems = [f"kinds.tsv holds {line} {source} rows, which no reader reads" for line, source in sorted(held - set(KIND_READERS))]
    for (line, source), read in KIND_READERS.items():
        rows = {row["kind"] for row in kinds if row["line"] == line and row["source"] == source}
        found = read()
        if rows != found:
            problems.append(f"kinds.tsv's {line} {source} rows differ from the source: {sorted(rows ^ found)[:6]}")
    return problems


def inventory_analyzer_problems(analyzers):
    problems = []
    for side, sha in LINE_SHAS.items():
        declared = {}
        for hit in git("grep", "-n", "-E", r'[A-Za-z_]+ = "ALIS[0-9]{3}"', sha, "--", "Alis.Reactive.Analyzers").splitlines():
            _, path, _, text = hit.split(":", 3)
            declared.update({ident: f"{path}#{name}" for name, ident in ANALYZER_DECLARATION.findall(text)})
        rows = {row["id"]: row[side] for row in analyzers if row[side]}
        problems += [f"analyzers.tsv: the {side} line declares {ident} at {where}, and no row holds it" for ident, where in sorted(declared.items()) if ident not in rows]
        problems += [f"analyzers.tsv: {ident}'s {side} location {where} declares no {ident} at {sha[:9]}" for ident, where in sorted(rows.items()) if declared.get(ident) != where]
    problems += [f"analyzers.tsv: {row['id']} is marked {row['status']}" for row in analyzers if ANALYZER_STATUS.get((bool(row["redesign"]), bool(row["main"]))) != row["status"]]
    return problems


def inventory_case_problems(cases):
    problems, held, texts = [], {}, {}
    for row in cases:
        key = (row["line"], row["file"])
        if key not in texts:
            texts[key] = git("show", f"{LINE_SHAS[row['line']]}:{row['file']}").splitlines()
        held.setdefault(key, set()).add(row["label"])
        number = int(row["line_number"])
        if number > len(texts[key]) or f'case "{row["label"]}"' not in texts[key][number - 1]:
            problems.append(f"cases.tsv: {row['line']} {row['file']}:{number} holds no case \"{row['label']}\"")
    for (line, path), labels in sorted(held.items()):
        found = set(CASE_LABEL.findall("\n".join(texts[(line, path)])))
        if found != labels:
            problems.append(f"cases.tsv: the {line} {path} cases differ from its rows: {sorted(found ^ labels)[:6]}")
    return problems


def check_inventory():
    paths = [SPEC / "inventory" / name for name in INVENTORY_FILES]
    if any(not path.exists() for path in paths):
        return not_started(*paths)
    doors = table("doors.tsv")
    problems = inventory_door_problems(doors)
    for row in doors:
        expected = ("both" if row["package"] == row["main_package"] else "moved") if row["redesign_line"] and row["main_line"] else "redesign-only" if row["redesign_line"] else "main-only"
        if row["status"] != expected or not row["declaring_type"]:
            problems.append(f"door row {row['door'][:60]}: status {row['status']}, declaring type '{row['declaring_type']}'")
    tests = table("tests.tsv")
    for line, sha in {"redesign": BASE_SHA, "main": MAIN_SHA}.items():
        files = [path for path in git("ls-tree", "-r", "--name-only", sha, "--", "tests").splitlines() if path.endswith(".cs") and "PlaywrightTests/" in path]
        attributes = sum(len(TEST_ATTRIBUTE.findall(git("show", f"{sha}:{path}"))) for path in files)
        cases = sum(int(row["cases"]) for row in tests if row["line"] == line)
        if attributes != cases:
            problems.append(f"{line} tests: {attributes} test attributes, {cases} cases in rows")
    kinds = table("kinds.tsv")
    problems += inventory_kind_problems(kinds) + inventory_analyzer_problems(table("analyzers.tsv")) + inventory_case_problems(table("cases.tsv"))
    schema = {row["kind"] for row in kinds if row["source"] == "plan.schema.json"}
    status = {}
    for row in doors:
        status[row["status"]] = status.get(row["status"], 0) + 1
    return result(not problems, "; ".join(problems[:8]) or "every door, kind, analyzer and case row equals its source line by line, and every test count its attributes",
                  doors=len(doors), both=status.get("both", 0), moved=status.get("moved", 0), main_only=status.get("main-only", 0), redesign_only=status.get("redesign-only", 0),
                  tests=len(tests), kinds=len(schema), analyzers=len(table("analyzers.tsv")), cases=len(table("cases.tsv")))


RUNGS = {"type", "analyzer", "refusal", "behavior"}
CITE_EXTENSIONS = "cs|ts|tla|cfg|json|md|txt|mjs|csproj|props|targets|cshtml|js|sh|yml|css"
CITE = re.compile(rf"([A-Za-z0-9_.\-/]+\.(?:{CITE_EXTENSIONS})):(\d+)(?:-(\d+))?")
PROSE_CITE_TOKEN = re.compile(rf"(?P<path>@?[A-Za-z0-9_.\-/]+\.(?:{CITE_EXTENSIONS}))?:(?P<first>\d+)(?:-(?P<last>\d+))?|\bat\s+(?P<sha>[0-9a-f]{{7,40}})\b|(?P<stop>[.;]\s|\n\n)")
CHECKER = re.compile(r"`(?:([A-Za-z]+)\.tla:(\w+)|probe:([\w.\-]+)|check:(\w+))`")
DEFECT = re.compile(r"^- `?(X\d+\.\d+)`? (type|analyzer|refusal|behavior)(?: \([^)]*\))?: (.+)$")
NEW_DOOR = re.compile(r"`?(N\d+\.\d+)`?")
EVIDENCE_IDS = [f"G{n}" for n in range(1, 11)] + [f"B{n}" for n in range(1, 17)] + [f"P{n}" for n in range(1, 6)]
_line_counts = {}


def line_count(sha, path):
    key = (sha, path)
    if key not in _line_counts:
        if sha is None:
            target = REPO / path
            _line_counts[key] = len(target.read_text().splitlines()) if target.is_file() else -1
        else:
            completed = subprocess.run(["git", "-C", str(REPO), "show", f"{sha}:{path}"], capture_output=True, text=True)
            _line_counts[key] = len(completed.stdout.splitlines()) if completed.returncode == 0 else -1
    return _line_counts[key]


def cite_resolves(path, first, last):
    needed = max(first, last or first)
    sources = [None] if path.startswith("spec/") else [BASE_SHA, MAIN_SHA]
    return any(line_count(sha, path) >= needed for sha in sources)


def design_sections():
    path = SPEC / "Core" / "DESIGN.md"
    if not path.exists():
        return None
    sections = {}
    current = None
    for line in path.read_text().splitlines():
        heading = re.match(r"^## (Q\d+)\.", line)
        if heading:
            current = heading.group(1)
            sections[current] = []
        elif line.startswith("## "):
            current = None
        elif current:
            sections[current].append(line)
    return sections


def section_parts(lines):
    parts = {}
    current = None
    for line in lines:
        label = re.match(r"^\*\*(" + "|".join(re.escape(part) for part in DESIGN_PARTS) + r")\.\*\*\s*(.*)$", line)
        if label:
            current = label.group(1)
            parts[current] = [label.group(2)] if label.group(2) else []
        elif current:
            parts[current].append(line)
    return {name: "\n".join(body).strip() for name, body in parts.items()}


def bullets(text):
    items = []
    for line in text.splitlines():
        if line.startswith("- "):
            items.append(line)
        elif items and line.startswith("  "):
            items[-1] += " " + line.strip()
    return items


SPEC_PATH = re.compile(r"\bspec/[\w/.\-]+\.(?:md|tla|cfg|json|tsv|txt|py)\b")


def tla_file(name):
    found = sorted(SPEC.glob(f"*/{name}.tla"))
    return found[0] if len(found) == 1 else SPEC / "Core" / f"{name}.tla"


def tla_invariants(spec):
    tla = tla_file(spec)
    cfg = tla.with_suffix(".cfg")
    if not tla.exists():
        return None, set()
    defined = set(re.findall(r"^(\w+)\s*==", tla.read_text(), re.MULTILINE))
    listed = set()
    if cfg.exists():
        listed = set(re.findall(r"\b(\w+)\b", " ".join(re.findall(r"(?:INVARIANTS?|PROPERTY|PROPERTIES)\s+([\w\s]+?)(?=\n[A-Z]+\b|\Z)", cfg.read_text()))))
    return defined, listed


def probe_ids():
    path = SPEC / "probes" / "facts.json"
    return {fact["id"] for fact in json.loads(path.read_text())["facts"]} if path.exists() else set()


def all_new_doors(sections):
    doors = set()
    for lines in sections.values():
        doors |= set(NEW_DOOR.findall(section_parts(lines).get("Doors", "")))
    return doors


def judge_section(question, lines, new_doors, analyzer_ids, probes):
    parts = section_parts(lines)
    problems = []
    for part in DESIGN_PARTS:
        if not parts.get(part):
            problems.append(f"no {part}")
    if problems:
        return problems, []
    if not re.search(r"^- ", parts["Decision"], re.MULTILINE):
        problems.append("Decision has no closed statement")
    for named in sorted(set(SPEC_PATH.findall(parts["Decision"]))):
        if not (REPO / named).is_file():
            problems.append(f"Decision names {named}, which does not exist")
    cites = CITE.findall(parts["Evidence"])
    if not cites:
        problems.append("Evidence cites no file:line")
    for path, first, last in cites:
        if not cite_resolves(path, int(first), int(last) if last else None):
            problems.append(f"Evidence cite {path}:{first} resolves at neither SHA")
    checkers = CHECKER.findall(parts["Checker"])
    if not checkers:
        problems.append("Checker names no invariant or probe")
    for spec, invariant, probe, check in checkers:
        if check:
            if check not in {identifier for identifier, _, _ in CHECKS}:
                problems.append(f"Checker check:{check} is no check.py check")
            continue
        if probe:
            if probe not in probes:
                problems.append(f"Checker probe:{probe} is not in spec/probes/facts.json")
            continue
        defined, listed = tla_invariants(spec)
        if defined is None or invariant not in defined:
            problems.append(f"Checker {spec}.tla:{invariant} is not defined")
        elif invariant not in listed:
            problems.append(f"Checker {spec}.tla:{invariant} is not checked by {spec}.cfg")
    defects = []
    for line in bullets(parts["Defects"]):
        match = DEFECT.match(line)
        if not match:
            continue
        identifier, rung, text = match.groups()
        defects.append(identifier)
        if rung == "type" and not (CITE.search(text) and all(cite_resolves(p, int(f), int(l) if l else None) for p, f, l in CITE.findall(text))) and not any(door in new_doors for door in re.findall(r"new:(N\d+\.\d+)", text)):
            problems.append(f"{identifier} type cites no resolving file:line and no new:N door")
        if rung == "analyzer" and not (set(re.findall(r"\bALIS\d{3}\b", text)) & analyzer_ids or any(door in new_doors for door in re.findall(r"new:(N\d+\.\d+)", text))):
            problems.append(f"{identifier} analyzer names no existing ALIS id and no new:N door")
        if rung == "refusal" and not re.search(r"\"[^\"]{8,}\"", text):
            problems.append(f"{identifier} refusal quotes no message")
    if not defects:
        problems.append("Defects lists no defect with a rung")
    return problems, defects


def check_questions(questions):
    return lambda: answered_questions(questions)


def answered_questions(questions):
    sections = design_sections()
    if sections is None:
        return not_started(SPEC / "Core" / "DESIGN.md")
    analyzer_ids = {row["id"] for row in table("analyzers.tsv")}
    probes = probe_ids()
    new_doors = all_new_doors(sections)
    stated = module_statements()
    answered, problems, seen = [], [], {}
    for question in questions:
        if question not in sections:
            problems.append(f"{question} absent")
            continue
        found, defects = judge_section(question, sections[question], new_doors, analyzer_ids, probes)
        found += [f"names {named}, which no module states" for named in sorted(set(MODULE_STATEMENT.findall("\n".join(sections[question])))) if named not in stated]
        for identifier in defects:
            if identifier in seen:
                found.append(f"defect {identifier} also in {seen[identifier]}")
            seen[identifier] = question
        if found:
            problems.append(f"{question}: " + ", ".join(found[:3]))
        else:
            answered.append(question)
    return result(len(answered) == len(questions), "; ".join(problems[:6]) or "every question answered with resolving evidence, checker and rungs",
                  answered=len(answered), total=len(questions), defects=len(seen))


def check_evidence():
    sections = design_sections()
    if sections is None:
        return not_started(SPEC / "Core" / "DESIGN.md")
    text = "\n".join(section_parts(lines).get("Evidence", "") for lines in sections.values())
    missing = [identifier for identifier in EVIDENCE_IDS if not re.search(rf"\b{identifier}\b", text)]
    return result(not missing, "unplaced: " + ", ".join(missing) if missing else "every G, B and P item has a home",
                  placed=len(EVIDENCE_IDS) - len(missing), total=len(EVIDENCE_IDS))


def spec_hash(tla, cfg):
    return hashlib.sha256(tla.read_bytes() + b"\0" + cfg.read_bytes()).hexdigest()


def run_tlc(tla, cfg):
    with tempfile.TemporaryDirectory() as meta:
        completed = subprocess.run(
            [JAVA, f"-Djava.io.tmpdir={meta}", "-cp", str(TLA_JAR), "tlc2.TLC", "-deadlock", "-workers", "auto", "-metadir", meta, "-config", cfg.name, tla.name],
            cwd=tla.parent, capture_output=True, text=True,
        )
    lines = (completed.stdout + completed.stderr).splitlines()
    green = completed.returncode == 0 and any(TLC_GREEN in line for line in lines)
    violated = sorted({match.group(1) for line in lines for match in [re.search(r"Invariant (\w+) is violated", line)] if match})
    states = next((line.strip() for line in reversed(lines) if "distinct states found" in line), "")
    errors = [line.strip() for line in lines if line.startswith("Error") or "error" in line.lower() and "No error" not in line][:2]
    line = TLC_GREEN if green else ("violated: " + ", ".join(violated) if violated else "; ".join(errors) or (lines[-1] if lines else "no output"))
    return {"green": green, "violated": violated, "line": line, "states": states}


def tlc_configs(name):
    folder = tla_file(name).parent
    main = folder / f"{name}.cfg"
    violating = {match.group(1): path for path in sorted(folder.glob(f"{name}.violates.*.cfg")) for match in [re.match(rf"{name}\.violates\.(\w+)\.cfg$", path.name)] if match}
    holding = sorted(folder.glob(f"{name}.holds.*.cfg"))
    return main, violating, holding


def tla_jar_problems():
    pinned = json.loads(TLA_LOCK.read_text())["sha256"]
    if not TLA_JAR.exists():
        return [f"{TLA_JAR} missing, the lock pins {pinned}"]
    found = hashlib.sha256(TLA_JAR.read_bytes()).hexdigest()
    return [] if found == pinned else [f"{TLA_JAR} has SHA-256 {found}, the lock pins {pinned}"]


def check_tlc(brief, previous, step):
    cached = previous.get("checks", {}).get("tlc", {}).get("runs", {})
    runs = {}
    problems = tla_jar_problems()
    specs = {name: expectation for name, expectation in TLC_SPECS.items() if expectation["step"] <= step}
    met = 0
    for name, expectation in specs.items():
        tla = tla_file(name)
        main, violating, holding = tlc_configs(name)
        if not tla.exists():
            problems.append(f"{name} not started")
            continue
        planned = ([main] if expectation["expect"] == "green" else []) + list(violating.values()) + holding
        spec_runs = {}
        stale = False
        for cfg in planned:
            if not cfg.exists():
                continue
            digest = spec_hash(tla, cfg)
            run = cached.get(name, {}).get(cfg.name)
            if not run or run.get("hash") != digest:
                if brief:
                    stale = True
                    continue
                run = {"hash": digest, **run_tlc(tla, cfg)}
            spec_runs[cfg.name] = run
        runs[name] = spec_runs
        if stale:
            problems.append(f"{name} changed since its last run")
            continue
        own = []
        if expectation["expect"] == "green":
            if not main.exists():
                own.append("no main cfg")
            elif not spec_runs[main.name]["green"]:
                own.append(spec_runs[main.name]["line"])
            _, listed = tla_invariants(name)
            unmutated = sorted(invariant for invariant in listed if invariant not in violating)
            if unmutated:
                own.append("no violating cfg for " + ", ".join(unmutated))
            if not any(invariant.startswith("Reach") for invariant in violating):
                own.append("no Reach witness")
        elif len(violating) != expectation["gaps"]:
            own.append(f"{len(violating)} gap cfgs, expected {expectation['gaps']}")
        for invariant, cfg in violating.items():
            if spec_runs[cfg.name]["violated"] != [invariant]:
                own.append(f"{cfg.name}: expected {invariant} violated, saw {spec_runs[cfg.name]['line']}")
        for cfg in holding:
            if not spec_runs[cfg.name]["green"]:
                own.append(f"{cfg.name}: expected green, saw {spec_runs[cfg.name]['line']}")
        if own:
            problems.append(f"{name}: " + "; ".join(own[:3]))
        else:
            met += 1
    outcome = result(not problems, "; ".join(problems[:6]) or "every spec green, every invariant killed by its mutant, every witness reached", specs=len(specs), met=met)
    outcome["runs"] = runs
    return outcome


def check_probes():
    path = SPEC / "probes" / "facts.json"
    if not path.exists():
        return not_started(path)
    facts = json.loads(path.read_text())["facts"]
    feature_ids = {feature["id"] for path in feature_files() for feature in json.loads(path.read_text()).get("features", [])}
    outputs = {}
    problems = []
    for fact in facts:
        if fact["host"] == "node":
            script = SPEC / "probes" / fact["probe"]
            if script not in outputs:
                outputs[script] = probe_run(fact["probe"], ["node", str(script)], REPO)
            problems += probe_fact_problems(fact, outputs[script])
            continue
        if fact["host"] == "browser":
            script = SPEC / "probes" / fact["probe"]
            if script not in outputs:
                command, environment = browser_probe(script)
                outputs[script] = probe_run(fact["probe"], command, REPO, environment)
            problems += probe_fact_problems(fact, outputs[script]) + browser_proof_problems(fact, feature_ids)
            continue
        if fact["host"] == "net48":
            source, _, number = fact["source"].rpartition(":")
            lines = (REPO / source).read_text().splitlines() if (REPO / source).is_file() else []
            if not source.startswith("spec/probes/") or int(number) > len(lines) or fact["expect"] not in lines[int(number) - 1]:
                problems.append(f"{fact['id']}: \"{fact['expect']}\" is not at {fact['source']}")
            continue
        probe = SPEC / "probes" / fact["probe"]
        if probe not in outputs:
            outputs[probe] = probe_run(fact["probe"], ["dotnet", "run", "--project", str(probe)], probe)
        problems += probe_fact_problems(fact, outputs[probe])
    problems = sorted({failure for _, failure in outputs.values() if failure}) + problems
    return result(not problems, "; ".join(problems[:6]) or "every fact printed by its probe or found at its source line, every browser fact with the proof the rewrite ships",
                  facts=len(facts), net10=sum(1 for fact in facts if fact["host"] == "net10"), net48=sum(1 for fact in facts if fact["host"] == "net48"), node=sum(1 for fact in facts if fact["host"] == "node"), browser=sum(1 for fact in facts if fact["host"] == "browser"))


def probe_run(name, command, folder, environment=None):
    completed = subprocess.run(command, capture_output=True, text=True, cwd=folder, env=environment)
    if completed.returncode != 0:
        return [], f"{name} exited {completed.returncode}: " + " ".join(completed.stderr.strip().splitlines()[-2:])[-240:]
    return completed.stdout.splitlines(), None


def probe_fact_problems(fact, run):
    lines, failure = run
    return [] if failure or fact["expect"] in lines else [f"{fact['id']}: {fact['probe']} printed no line \"{fact['expect']}\""]


def browser_proof_problems(fact, feature_ids):
    proof = fact.get("proof") or {}
    missing = [field for field in ("page", "gesture", "outcome") if not is_prose(proof.get(field))]
    problems = [f"{fact['id']}: its proof lacks {', '.join(missing)}"] if missing else []
    if proof.get("feature") not in feature_ids:
        problems.append(f"{fact['id']}: its proof names {proof.get('feature')!r}, no feature")
    return problems


PLAYWRIGHT_PIN = re.compile(r'Include="Microsoft\.Playwright" Version="([0-9.]+)"')
PLAYWRIGHT_SYSTEMS = {"Darwin": "darwin", "Linux": "linux"}
PLAYWRIGHT_MACHINES = {"arm64": "arm64", "aarch64": "arm64", "x86_64": "x64"}


def browser_probe(script):
    host = REPO / "tests" / "Alis.Reactive.Playwright.TestHost" / "Alis.Reactive.Playwright.TestHost.csproj"
    bundled = Path.home() / ".nuget" / "packages" / "microsoft.playwright" / PLAYWRIGHT_PIN.search(host.read_text()).group(1) / ".playwright"
    node = bundled / "node" / f"{PLAYWRIGHT_SYSTEMS[platform.system()]}-{PLAYWRIGHT_MACHINES[platform.machine()]}" / "node"
    environment = {**os.environ, "PLAYWRIGHT_PACKAGE": str(bundled / "package"), "NODE_PATH": str(installed_modules())}
    return [str(node), str(script)], environment


def regression_shas():
    brief = (SPEC / "BRIEF.md").read_text()
    start = brief.index("**What partials cost: the regression list (P).**")
    end = brief.index("## 7.", start)
    return sorted(set(re.findall(r"\b[0-9a-f]{9}\b", brief[start:end])))


def spec_line_resolves(line):
    sections = design_sections() or {}
    if re.fullmatch(r"DESIGN\.md#Q\d+", line):
        return line.split("#")[1] in sections
    match = re.fullmatch(r"(\w+)\.tla:(\w+)", line)
    if match:
        defined, _ = tla_invariants(match.group(1))
        return defined is not None and match.group(2) in defined
    match = re.fullmatch(r"probe:([\w.\-]+)", line)
    if match:
        return match.group(1) in probe_ids()
    return False


def on_branch(sha, tip):
    return subprocess.run(["git", "-C", str(REPO), "merge-base", "--is-ancestor", sha, tip]).returncode == 0


def check_history():
    path = SPEC / "history.tsv"
    if not path.exists():
        return not_started(path)
    rows = [dict(zip(["sha", "branch", "group", "spec_line"], line.split("\t"))) for line in path.read_text().splitlines()[1:]]
    problems = []
    listed = {row["sha"][:9] for row in rows}
    missing = [sha for sha in regression_shas() if sha not in listed]
    if missing:
        problems.append("missing " + ", ".join(missing))
    for row in rows:
        full = git("rev-parse", "--verify", "-q", row["sha"] + "^{commit}")
        if not full:
            problems.append(f"{row['sha']} is no commit")
            continue
        branch = "both" if on_branch(full, MAIN_SHA) and on_branch(full, BASE_SHA) else "redesign" if on_branch(full, BASE_SHA) else "main" if on_branch(full, MAIN_SHA) else "neither"
        if branch != row["branch"]:
            problems.append(f"{row['sha']} is on {branch}, the row says {row['branch']}")
        if not spec_line_resolves(row["spec_line"]):
            problems.append(f"{row['sha']} spec line {row['spec_line']} resolves to nothing")
    return result(not problems, "; ".join(problems[:6]) or "every regression SHA on its branch and tied to a spec line",
                  rows=len(rows), required=len(regression_shas()))


def grammar_kinds(sha):
    marker = 'Kind => "'
    kinds = set()
    for path in git("ls-tree", "-r", "--name-only", sha, "--", "Alis.Reactive/Grammar").splitlines():
        if path.endswith(".cs"):
            for line in git("show", f"{sha}:{path}").splitlines():
                if marker in line:
                    kinds.add(line.split(marker, 1)[1].split('"', 1)[0])
    return kinds


def schema_json_kinds(sha):
    document = json.loads(git("show", f"{sha}:Alis.Reactive.Assets/runtime/types/plan.schema.json"))
    return {definition["properties"]["kind"]["const"] for definition in document["$defs"].values()
            if "const" in definition.get("properties", {}).get("kind", {})}


HEAD_CONTRACT = REPO / "Alis.Reactive.Assets" / "runtime" / "Schema" / "plan.schema.json"


def contract_kinds(document):
    definitions, followed, kinds, pending = document["$defs"], set(), set(), [{"$ref": document["$ref"]}]
    while pending:
        node = pending.pop()
        if isinstance(node, list):
            pending += node
        if not isinstance(node, dict):
            continue
        name = node["$ref"].rsplit("/", 1)[1] if isinstance(node.get("$ref"), str) else None
        if name is not None and name not in followed:
            followed.add(name)
            pending.append(definitions[name])
        properties = node.get("properties")
        kind = properties.get("kind") if isinstance(properties, dict) else None
        if isinstance(kind, dict) and "const" in kind:
            kinds.add(kind["const"])
        pending += node.values()
    return kinds


def table_cells(line):
    return [cell.strip() for cell in line.strip().strip("|").split("|")]


def schema_kind_rows(text):
    section = text.split("\n## Kinds\n", 1)[1].split("\n## ", 1)[0]
    rows = [table_cells(line) for line in section.splitlines() if line.startswith("| ")]
    header = rows[0]
    return [dict(zip(header, cells)) for cells in rows[1:] if not set(cells[0]) <= {"-"}]


def backticked(text):
    return text.split("`")[1::2]


def source_line(sha, cite):
    path, number = cite.rsplit(":", 1)
    lines = git("show", f"{sha}:{path}").splitlines()
    return lines[int(number) - 1] if 0 < int(number) <= len(lines) else ""


def spec_node_set():
    text = tla_file("Schema").read_text()
    kinds = set()
    for line in text.splitlines():
        if "Kinds ==" in line and line.split("==", 1)[0].strip().endswith("Kinds") and "{" in line:
            kinds |= {item.strip().strip('"') for item in line.split("{", 1)[1].split("}", 1)[0].split(",")}
    return kinds


def check_kinds():
    path = SPEC / "Core" / "schema.md"
    if not path.exists():
        return not_started(path)
    problems = []
    grammar = grammar_kinds(BASE_SHA)
    schema_json = schema_json_kinds(BASE_SHA)
    if grammar != schema_json:
        problems.append(f"grammar and plan.schema.json differ at the State SHA: {sorted(grammar ^ schema_json)}")
    rows = schema_kind_rows(path.read_text())
    kinds = [row["Kind"] for row in rows]
    repeated = sorted({kind for kind in kinds if kinds.count(kind) > 1})
    if repeated:
        problems.append(f"a kind named twice: {repeated}")
    today = {row["Kind"] for row in rows if row["Today"] != "absent"}
    if today != grammar:
        problems.append(f"schema.md's today column differs from the grammar: {sorted(today ^ grammar)}")
    designed = set((SPEC / "Core" / "DESIGN.md").read_text().split())
    several_today = []
    for row in rows:
        kind, cites, handler = row["Kind"], backticked(row["Today"]), row["Handler"]
        if row["Proposed"] not in {"kept", "added", "removed"}:
            problems.append(f"{kind}: proposed '{row['Proposed']}'")
        if (row["Proposed"] == "added") != (row["Today"] == "absent"):
            problems.append(f"{kind}: proposed {row['Proposed']} beside today '{row['Today']}'")
        if len(cites) > 1:
            several_today.append(kind)
        for cite in cites:
            line = source_line(BASE_SHA, cite)
            expected = ('"none"' if kind == "none" else PLUGIN_DATA_DISPATCH) if row["Union"] == "plugin data" else f'case "{kind}"'
            if expected not in line:
                problems.append(f"{kind}: {cite} holds no {expected} at the State SHA")
        if row["Proposed"] == "removed":
            continue
        if kind == "none":
            if handler != "none":
                problems.append("none has a handler")
        elif row["Union"] == "plugin data":
            if not any(name.startswith("alis.") for name in backticked(handler)):
                problems.append(f"{kind}: its handler names no plugin")
        elif len(backticked(handler)) != 1 or not line_count(BASE_SHA, backticked(handler)[0]) > 0:
            problems.append(f"{kind}: '{handler}' names not exactly one runtime file")
        if row["Proposed"] == "added" and not any(token.strip("`.,;:()") == row["Why"] for token in designed):
            problems.append(f"{kind}: added by {row['Why']}, which DESIGN.md does not state")
    proposed = {row["Kind"] for row in rows if row["Proposed"] != "removed"}
    spec_nodes = spec_node_set()
    if proposed != spec_nodes:
        problems.append(f"schema.md's proposed set differs from Schema.tla's Kinds: {sorted(proposed ^ spec_nodes)}")
    added = sorted(row["Kind"] for row in rows if row["Proposed"] == "added")
    removed = sorted(row["Kind"] for row in rows if row["Proposed"] == "removed")
    problems += closed_set_problems(len(proposed), len(grammar), added, removed)
    written = contract_kinds(json.loads(HEAD_CONTRACT.read_text())) if HEAD_CONTRACT.is_file() else set()
    if not HEAD_CONTRACT.is_file():
        problems.append(f"{HEAD_CONTRACT.relative_to(REPO)}, the contract the rewrite's grammar generates, is not in the tree")
    unlisted = sorted(written - proposed)
    if unlisted:
        problems.append(f"the rewrite's grammar writes a kind schema.md does not list: {unlisted}")
    detail = "; ".join(problems[:6]) or (
        f"grammar, plan.schema.json and schema.md agree on today's {len(grammar)}; proposed {len(proposed)} equals Schema.tla's"
        f" node set, one handler each; diff +{','.join(added)} -{','.join(removed) or 'none'};"
        f" {len(several_today)} kinds have two handlers today; the rewrite's grammar writes {len(written)} of the proposed set and no other")
    return result(not problems, detail, today=len(grammar), proposed=len(proposed), added=len(added), removed=len(removed),
                  several_handlers_today=len(several_today), written=len(written))


CLOSED_SET = re.compile(r"closed at (\d+) kinds: today's (\d+) but (.*?), and (.*?), the (\d+) `spec/Core/schema\.md` marks added")


def closed_set_problems(proposed, today, added, removed):
    stated = next((item for lines in (design_sections() or {}).values() for item in bullets(section_parts(lines).get("Decision", ""))
                   if item.startswith("- D19.2 ")), "")
    match = CLOSED_SET.search(stated)
    if match is None:
        return ["D19.2 states no closed set as 'closed at N kinds: today's N but .., and .., the N spec/Core/schema.md marks added'"]
    closed, today_stated, removed_stated, added_stated, added_count = match.groups()
    problems = [f"D19.2 closes the set at {closed} kinds, and schema.md proposes {proposed}"] if int(closed) != proposed else []
    problems += [f"D19.2 counts {today_stated} kinds today, and the grammar holds {today}"] if int(today_stated) != today else []
    problems += [f"D19.2 removes {sorted(backticked(removed_stated))}, and schema.md removes {removed}"] if sorted(backticked(removed_stated)) != removed else []
    problems += [f"D19.2 adds {sorted(backticked(added_stated))}, and schema.md adds {added}"] if sorted(backticked(added_stated)) != added else []
    problems += [f"D19.2 counts {added_count} added, and schema.md adds {len(added)}"] if int(added_count) != len(added) else []
    return problems


def gate_answered(number):
    notes = (SPEC / "NOTES.md").read_text() if (SPEC / "NOTES.md").exists() else ""
    return re.search(rf"^- \d{{4}}-\d{{2}}-\d{{2}} owner, gate {number}: (?i:yes)\b", notes, re.MULTILINE) is not None


def check_gate(number):
    return lambda: result(gate_answered(number), f"gate {number} " + ("answered yes" if gate_answered(number) else "not answered"))


def feature_files():
    return sorted(SPEC.glob("*/features.json"))


FEATURE_ID = re.compile(r"^M(\d+)\.F\d+$")
MODULE_DOOR = re.compile(r"^- `(M\d+\.N\d+)`", re.MULTILINE)
MODULE_DEFECT = re.compile(r"^M(\d+)\.X\d+$")
MODULE_MEANING = re.compile(r"^M(\d+)\.D\d+$")
CORE_DEFECT = re.compile(r"^X\d+\.\d+$")
DOOR_ID = re.compile(r"^(?:N\d+\.\d+|M\d+\.N\d+)$")
VERDICT_KINDS = {"kept", "renamed", "refused"}
SENTENCE_LINES = {"main": MAIN_SHA, "redesign": BASE_SHA, "proposed": None}
FEATURE_STATUSES = {"both", "main-only", "redesign-only", "internal", "new"}
MAIN_SIDE = {"main-only", "both", "moved"}
REDESIGN_SIDE = {"redesign-only", "both", "moved"}


CONSUMER_SEEN = {"build", "built on the core"}


def module_folder(name):
    return "".join(word[:1].upper() + word[1:] for word in re.split(r"[\s-]+", name) if word)


def module_rows():
    text = (SPEC / "CLAUDE.md").read_text()
    section = text[text.index("## Modules"):]
    cells = [[cell.strip() for cell in line.strip().strip("|").split("|")] for line in section[:section.find("\n## ")].splitlines()
             if line.startswith("|")]
    return [(row[0], row[1], row[2]) for row in cells if row[0].isdigit()]


def module_names():
    return {module_folder(name): int(number) for number, name, _ in module_rows()}


def core_modules():
    return {module_folder(name) for _, name, seen in module_rows() if seen not in CONSUMER_SEEN}


def internal_modules():
    return {int(number) for number, _, seen in module_rows() if seen == "internal"}


def expected_status(model, feature_id):
    owned = status_of(owned_rows(model, feature_id))
    match = FEATURE_ID.match(feature_id)
    if owned or not match:
        return owned
    return "internal" if int(match.group(1)) in internal_modules() else "new"


def module_doors():
    return {door for path in SPEC.glob("*/module.md") for door in MODULE_DOOR.findall(path.read_text())}


MODULE_STATEMENT = re.compile(r"\bM\d+\.[DNX]\d+\b")


def module_statements():
    stated = set(module_doors())
    for path in feature_files():
        for feature in json.loads(path.read_text()).get("features", []):
            stated |= {entry.get("id") for entry in feature.get("meanings", []) + feature.get("defects", [])}
    return stated


def core_defect_rungs():
    rungs = {}
    for lines in (design_sections() or {}).values():
        for line in bullets(section_parts(lines).get("Defects", "")):
            match = DEFECT.match(line)
            if match:
                rungs[match.group(1)] = match.group(2)
    return rungs


def is_prose(value):
    return isinstance(value, str) and value.strip() != ""


def cited_text(sha, cite):
    match = CITE.fullmatch(cite) if isinstance(cite, str) else None
    if not match:
        return ""
    path, first, last = match.groups()
    lines = git("show", f"{sha}:{path}").splitlines()
    return "".join(lines[int(first) - 1:int(last or first)])


def without_space(text):
    return re.sub(r"\s+", "", text)


def tagged_cites_problems(feature, field):
    cites = feature.get(field)
    if not isinstance(cites, list) or not cites:
        return [f"{feature['id']}: {field} cites nothing"]
    problems = []
    for cite in cites:
        line = cite.get("line") if isinstance(cite, dict) else None
        if line not in ("main", "redesign") or not cites_resolve([cite.get("cite")], SENTENCE_LINES[line]):
            problems.append(f"{feature['id']}: {field} {cite!r} resolves at no SHA of its line")
    if feature.get("status") in ("both", "redesign-only", "internal") and not any(isinstance(cite, dict) and cite.get("line") == "redesign" for cite in cites):
        problems.append(f"{feature['id']}: {field} cites nothing at the State SHA, the redesign's")
    return problems


def cites_resolve(cites, sha=None):
    found = []
    for cite in cites if isinstance(cites, list) else []:
        match = CITE.fullmatch(cite) if isinstance(cite, str) else None
        if not match:
            return False
        path, first, last = match.groups()
        needed = max(int(first), int(last or first))
        sources = [sha] if sha else [BASE_SHA, MAIN_SHA]
        found.append(any(line_count(each, path) >= needed for each in sources))
    return bool(found) and all(found)


def feature_model():
    names = module_names()
    rows = [row for row in table("doors.tsv") if row["scope"] == "spec"]
    model = {"problems": [], "features": [], "rows": rows, "owners": [[] for _ in rows]}
    seen = set()
    for path in feature_files():
        where = str(path.relative_to(REPO))
        if path.parent.name not in names:
            model["problems"].append(f"{where}: {path.parent.name} is no module of spec/CLAUDE.md")
            continue
        data = json.loads(path.read_text())
        if data.get("module") != path.parent.name:
            model["problems"].append(f"{where}: module is not {path.parent.name}")
        for feature in data.get("features", []):
            identifier = feature.get("id", "")
            match = FEATURE_ID.match(identifier)
            if not match or int(match.group(1)) != names[path.parent.name] or identifier in seen:
                model["problems"].append(f"{where}: feature id {identifier!r} is not a new M{names[path.parent.name]}.F<n>")
            seen.add(identifier)
            model["features"].append(feature)
    keys = {row["package"] + ":" + row["declaring_type"] for row in rows}
    doors = {row["package"] + ":" + row["door"] for row in rows}
    by_type = {}
    by_claim = {}
    for feature in model["features"]:
        for key in feature.get("types", []):
            if key not in keys:
                model["problems"].append(f"{feature['id']}: type {key} has no spec row in doors.tsv")
            by_type.setdefault(key, []).append(feature["id"])
        for door in feature.get("claims", []):
            if door not in doors:
                model["problems"].append(f"{feature['id']}: claim {door} is no spec door in doors.tsv")
            by_claim.setdefault(door, []).append(feature["id"])
    for index, row in enumerate(rows):
        claimed = by_claim.get(row["package"] + ":" + row["door"], [])
        typed = by_type.get(row["package"] + ":" + row["declaring_type"], [])
        if set(claimed) & set(typed):
            model["problems"].append(f"{row['door']}: claimed by the feature that maps its type")
        model["owners"][index] = claimed or typed
    return model


def owned_rows(model, feature_id):
    return [row for row, owners in zip(model["rows"], model["owners"]) if owners == [feature_id]]


def mapping_counts(model):
    unmapped = sum(1 for owners in model["owners"] if not owners)
    doubled = sum(1 for owners in model["owners"] if len(owners) > 1)
    return {"features": len(model["features"]), "rows": len(model["rows"]), "mapped": len(model["rows"]) - unmapped - doubled, "unmapped": unmapped, "doubled": doubled}


def unmapped_text(model):
    unmapped = [row["package"] + ":" + row["door"] for row, owners in zip(model["rows"], model["owners"]) if not owners]
    return f"every feature row valid; {len(unmapped)} spec doors in no feature" + (f", first {unmapped[0]}" if unmapped else "")


def feature_result(model, problems, counts, done):
    if problems:
        return result(False, "; ".join(problems[:8]), **counts)
    if counts["unmapped"]:
        return result(False, unmapped_text(model), **counts)
    return result(True, done, **counts)


def check_doors_mapped():
    if not feature_files():
        return result(False, "not started: no spec/<Module>/features.json")
    model = feature_model()
    counts = mapping_counts(model)
    problems = list(model["problems"])
    if counts["doubled"]:
        problems.append(f"{counts['doubled']} spec doors in two features")
    homeless = homeless_core_doors(all_new_doors(design_sections() or {}))
    counts["core_doors_homeless"] = len(homeless)
    if not problems and homeless:
        return result(False, f"{unmapped_text(model)}; core doors no module names: {', '.join(homeless)}", **counts)
    return feature_result(model, problems, counts, "every spec door in exactly one feature, every core door named by a module")


CORE_DOOR_NAMED = re.compile(r"\bN\d+\.\d+\b")


def homeless_core_doors(doors):
    named = set()
    for path in list(SPEC.glob("*/module.md")) + list(feature_files()):
        named |= set(CORE_DOOR_NAMED.findall(path.read_text()))
    return sorted(doors - named, key=lambda door: tuple(int(part) for part in door[1:].split(".")))


def door_target_problems(feature, verdict, field, doors):
    targets = verdict.get(field)
    if not isinstance(targets, list) or not targets:
        return [f"{feature['id']}: {verdict.get('door')} {verdict.get('verdict')} names no {field}"]
    return [f"{feature['id']}: {verdict.get('door')} {field} {target} is no core or module door" for target in targets if not (DOOR_ID.match(target) and target in doors)]


TIP_MEMBER = re.compile(r"\.(\w+)(?:<[^>]*>)?\(")
DELTA_ROOTS = ("Alis.Reactive/", "Alis.Reactive.Analyzers/", "Alis.Reactive.Assets/", "Alis.Reactive.Fusion/", "Alis.Reactive.FluentValidator/", "Alis.Reactive.Native/")
DELTA_MEMBER = {".cs": re.compile(r"^\+\s*public\s[^=;{(]*?\b(\w+)\s*(?:<[^>(]*>)?\s*(?:\(|\{|=>|=|;|:|$)"),
                ".ts": re.compile(r"^\+export\s+(?:default\s+)?(?:declare\s+)?(?:abstract\s+)?(?:async\s+)?(?:function\*?|const|let|var|class|interface|type|enum)\s+(\w+)")}


def tip_member(door):
    found = TIP_MEMBER.search(door) if isinstance(door, str) else None
    return found.group(1) if found else None


def tip_sentences(feature):
    return [sentence for sentence in feature.get("sentences", []) if sentence.get("line") == "main" and sentence.get("sha") == MAIN_TIP]


def tip_verdict(feature, verdict):
    member = tip_member(verdict.get("door"))
    return verdict.get("sha") == MAIN_TIP and member is not None and any(re.search(rf"\b{member}\b", sentence.get("text", "")) for sentence in tip_sentences(feature))


def main_delta_files():
    changed = git("diff", "--name-only", MAIN_SHA, MAIN_TIP).splitlines()
    return [path for path in changed if path.startswith(DELTA_ROOTS) and "/__tests__/" not in path and not path.endswith(".test.ts")]


def check_main_delta():
    features = [feature for path in feature_files() for feature in json.loads(path.read_text()).get("features", [])]
    cited = {sentence["cite"].split(":")[0] for feature in features for sentence in tip_sentences(feature) if isinstance(sentence.get("cite"), str)}
    given = {tip_member(verdict.get("door")) for feature in features for verdict in feature.get("verdicts", []) if verdict.get("sha") == MAIN_TIP}
    files = main_delta_files()
    problems = []
    members = []
    for path in files:
        if path not in cited:
            problems.append(f"{path} changed on main from {MAIN_SHA[:9]} to {MAIN_TIP[:9]} and no sentence cites it there")
        pattern = DELTA_MEMBER.get(Path(path).suffix)
        for line in git("diff", "-U0", MAIN_SHA, MAIN_TIP, "--", path).splitlines() if pattern else []:
            found = pattern.match(line)
            if found:
                members.append(found.group(1))
                if found.group(1) not in given:
                    problems.append(f"{path}: {found.group(1)}, added on main by {MAIN_TIP[:9]}, has no verdict at that SHA")
    detail = f"each of {len(files)} shipped files main changed from {MAIN_SHA[:9]} to {MAIN_TIP[:9]} is cited there by a feature's sentence, and each of {len(members)} public declarations or exports it added has a verdict"
    return result(not problems, "; ".join(problems[:4]) or detail, files=len(files), members=len(members), cited=len(cited))


def verdict_problems(model, doors):
    problems = []
    for feature in model["features"]:
        main_only = {row["door"] for row in owned_rows(model, feature["id"]) if row["status"] == "main-only"}
        given = {}
        for verdict in feature.get("verdicts", []):
            door = verdict.get("door")
            if door in given:
                problems.append(f"{feature['id']}: two verdicts for {door}")
            given[door] = verdict
            if door not in main_only and not tip_verdict(feature, verdict):
                problems.append(f"{feature['id']}: verdict for {door}, no main-only door of this feature")
            kind = verdict.get("verdict")
            if kind not in VERDICT_KINDS:
                problems.append(f"{feature['id']}: {door} verdict {kind!r} is not kept, renamed or refused")
            elif kind == "renamed":
                problems += door_target_problems(feature, verdict, "to", doors)
            elif kind == "refused":
                if not is_prose(verdict.get("reason")):
                    problems.append(f"{feature['id']}: {door} refused with no reason")
                problems += door_target_problems(feature, verdict, "carriers", doors)
            elif "as" in verdict:
                problems += door_target_problems(feature, {**verdict, "to": verdict["as"]}, "to", doors)
        problems += [f"{feature['id']}: no verdict for main-only {door}" for door in sorted(main_only - set(given))]
    return problems


def check_verdicts():
    if not feature_files():
        return result(False, "not started: no spec/<Module>/features.json")
    model = feature_model()
    doors = all_new_doors(design_sections() or {}) | module_doors()
    counts = mapping_counts(model)
    main_only = [owners for row, owners in zip(model["rows"], model["owners"]) if row["status"] == "main-only"]
    counts["main_only"] = len(main_only)
    counts["main_only_in_features"] = sum(1 for owners in main_only if len(owners) == 1)
    problems = model["problems"] + verdict_problems(model, doors)
    return feature_result(model, problems, counts, "every main-only door has a verdict, and every refusal names its carriers")


def checker_problems(feature):
    problems = []
    checkers = feature.get("checker")
    if not isinstance(checkers, list) or not checkers:
        return [f"{feature['id']}: no checker"]
    for checker in checkers:
        core = re.fullmatch(r"core:(Q\d+)", checker)
        if core:
            if core.group(1) not in feature.get("core", []):
                problems.append(f"{feature['id']}: checker {checker} names no core question the feature rests on")
            continue
        tla = re.fullmatch(r"(\w+)\.tla:(\w+)", checker)
        defined, listed = tla_invariants(tla.group(1)) if tla else (None, set())
        if not tla or defined is None or tla.group(2) not in defined or tla.group(2) not in listed:
            problems.append(f"{feature['id']}: checker {checker} is no core:Qn and no checked TLC invariant")
    return problems


def sentence_problems(feature, doors):
    problems = []
    sentences = feature.get("sentences")
    if not isinstance(sentences, list) or not sentences:
        return [f"{feature['id']}: no sentence"]
    for sentence in sentences:
        line = sentence.get("line")
        sha = sentence.get("sha")
        if line not in SENTENCE_LINES or not is_prose(sentence.get("text")):
            problems.append(f"{feature['id']}: sentence {sentence.get('text')!r} has no line of main, redesign or proposed")
        elif sha is not None and (sha != MAIN_TIP or line != "main"):
            problems.append(f"{feature['id']}: sentence {sentence['text']!r} names {sha!r}, and a sentence names only main's tip {MAIN_TIP[:9]}")
        elif line == "proposed":
            named = sentence.get("doors")
            if not isinstance(named, list) or not named or any(door not in doors for door in named):
                problems.append(f"{feature['id']}: proposed sentence {sentence['text']!r} names a door no core or module door")
        elif not cites_resolve([sentence.get("cite")], sha or SENTENCE_LINES[line]):
            problems.append(f"{feature['id']}: {line} sentence {sentence['text']!r} cites no line at {line}'s SHA")
        elif without_space(sentence["text"]) not in without_space(cited_text(sha or SENTENCE_LINES[line], sentence["cite"])):
            problems.append(f"{feature['id']}: {line} sentence {sentence['text']!r} is not at {sentence['cite']}")
    return problems


def status_of(rows):
    main = any(row["status"] in MAIN_SIDE for row in rows)
    redesign = any(row["status"] in REDESIGN_SIDE for row in rows)
    return "both" if main and redesign else "main-only" if main else "redesign-only" if redesign else None


def core_problems(model, doors):
    problems = []
    sections = design_sections() or {}
    kinds = spec_node_set()
    for feature in model["features"]:
        name = feature["id"]
        if not is_prose(feature.get("summary")) or not is_prose(feature.get("oracle")):
            problems.append(f"{name}: no summary or no oracle")
        status = feature.get("status")
        if status not in FEATURE_STATUSES or status != expected_status(model, name):
            problems.append(f"{name}: status {status!r}, its doors say {expected_status(model, name)!r}")
        core = feature.get("core")
        if not isinstance(core, list) or not core or any(question not in sections for question in core):
            problems.append(f"{name}: core {core!r} is no list of answered questions")
        nodes = feature.get("nodes")
        if not isinstance(nodes, list) or not nodes or set(nodes) - kinds:
            problems.append(f"{name}: nodes outside the schema: {sorted(set(nodes or []) - kinds)}")
        for field in ("writes", "executes"):
            problems += tagged_cites_problems(feature, field)
        for meaning in feature.get("meanings", []):
            match = MODULE_MEANING.match(meaning.get("id", ""))
            if not match or not is_prose(meaning.get("meaning")) or not name.startswith(f"M{match.group(1)}."):
                problems.append(f"{name}: meaning {meaning.get('id')!r} is no M<module>.D<n> with its meaning")
        problems += checker_problems(feature)
        problems += sentence_problems(feature, doors)
    return problems + new_feature_problems(model)


NEW_DOOR_BULLET = re.compile(r"^- `(N\d+\.\d+|M\d+\.N\d+)` new\b", re.MULTILINE)
DOORS_SECTION = re.compile(r"^## Doors\n(.*?)(?=^## |\Z)", re.MULTILINE | re.DOTALL)


def new_feature_problems(model):
    folders = {number: name for name, number in module_names().items()}
    problems = []
    for feature in model["features"]:
        match = FEATURE_ID.match(feature["id"])
        if feature.get("status") != "new" or not match:
            continue
        module = SPEC / folders.get(int(match.group(1)), "") / "module.md"
        section = DOORS_SECTION.search(module.read_text()) if module.is_file() else None
        kept_new = set(NEW_DOOR_BULLET.findall(section.group(1))) if section else set()
        proposed = {door for sentence in feature.get("sentences", []) if isinstance(sentence, dict) for door in sentence.get("doors") or []}
        if not proposed & kept_new:
            problems.append(f"{feature['id']}: a new feature whose proposed sentences name no door its module.md's Doors section marks new")
    return problems


def check_features_core():
    if not feature_files():
        return result(False, "not started: no spec/<Module>/features.json")
    model = feature_model()
    doors = all_new_doors(design_sections() or {}) | module_doors()
    counts = mapping_counts(model)
    return feature_result(model, model["problems"] + core_problems(model, doors), counts, "every feature rests on answered questions, schema nodes and checked invariants")


VERDICT_CARRIERS = {"renamed": "to", "refused": "carriers"}


def verdict_carried(feature, verdict, doors):
    kind = verdict.get("verdict")
    if kind == "kept":
        return "as" not in verdict or not door_target_problems(feature, {**verdict, "to": verdict["as"]}, "to", doors)
    return kind in VERDICT_CARRIERS and not door_target_problems(feature, verdict, VERDICT_CARRIERS[kind], doors)


def check_features_lost():
    if not feature_files():
        return result(False, "not started: no spec/<Module>/features.json")
    model = feature_model()
    doors = all_new_doors(design_sections() or {}) | module_doors()
    lost = []
    for feature in model["features"]:
        given = {verdict.get("door"): verdict for verdict in feature.get("verdicts", [])}
        main_only = {row["door"] for row in owned_rows(model, feature["id"]) if row["status"] == "main-only"}
        lost += [f"{feature['id']}: main-only {door} has no verdict" for door in sorted(main_only - set(given))]
        lost += [f"{feature['id']}: {door} {verdict.get('verdict')!r} is carried by no door" for door, verdict in given.items()
                 if not verdict_carried(feature, verdict, doors)]
    unmapped_types = sorted({row["package"] + ":" + row["declaring_type"] for row, owners in zip(model["rows"], model["owners"]) if not owners})
    counts = {"lost": len(lost) + len(unmapped_types), "uncarried_doors": len(lost), "types_in_no_feature": len(unmapped_types)}
    if model["problems"]:
        return result(False, f"features lost not counted while the feature rows are invalid: {'; '.join(model['problems'][:4])}", **counts)
    named = lost[:4] + ([f"{len(unmapped_types)} declaring types in no feature, first {', '.join(unmapped_types[:3])}"] if unmapped_types else [])
    return result(counts["lost"] == 0, "; ".join(named) or "features lost 0", **counts)


def defect_problems(model, core_rungs, doors, analyzer_ids):
    problems = []
    seen = set()
    owners = {}
    for feature in model["features"]:
        name = feature["id"]
        for defect in feature.get("defects", []):
            identifier, rung = defect.get("id", ""), defect.get("rung")
            if CORE_DEFECT.match(identifier) and owners.setdefault(identifier, name) != name:
                problems.append(f"{name}: {identifier} is placed in {owners[identifier]} too; a core defect has one feature")
            if rung not in RUNGS:
                problems.append(f"{name}: {identifier} rung {rung!r}")
            elif CORE_DEFECT.match(identifier):
                if core_rungs.get(identifier) != rung:
                    problems.append(f"{name}: {identifier} is no {rung} defect of spec/Core/DESIGN.md")
            elif MODULE_DEFECT.match(identifier) and name.startswith(f"M{MODULE_DEFECT.match(identifier).group(1)}.") and identifier not in seen:
                seen.add(identifier)
                text = defect.get("defect", "")
                named = set(re.findall(r"\b(?:N\d+\.\d+|M\d+\.N\d+)\b", text)) & doors
                if not is_prose(text):
                    problems.append(f"{name}: {identifier} states no defect")
                elif rung == "type" and not (cites_resolve([cite.group(0) for cite in CITE.finditer(text)]) or named):
                    problems.append(f"{name}: {identifier} type cites no resolving file:line and no door")
                elif rung == "analyzer" and not (set(re.findall(r"\bALIS\d{3}\b", text)) & analyzer_ids or named):
                    problems.append(f"{name}: {identifier} analyzer names no existing ALIS id and no door")
                elif rung == "refusal" and not re.search(r"\"[^\"]{8,}\"", text):
                    problems.append(f"{name}: {identifier} refusal quotes no message")
            else:
                problems.append(f"{name}: defect {identifier!r} is no core X<q>.<n> and no new M<module>.X<n>")
        rungs = {defect.get("id"): defect.get("rung") for defect in feature.get("defects", [])}
        problems += [f"{name}: promise for {promise.get('for')}, whose rung is {rungs[promise.get('for')]}, which the compiler proves" for promise in feature.get("promises", []) if rungs.get(promise.get("for")) in ("type", "analyzer")]
    return problems


def check_module_cites():
    modules = sorted(SPEC.glob("*/module.md"))
    if not modules:
        return result(False, "not started: no spec/<Module>/module.md")
    problems, cites = [], 0
    for path in modules:
        for cite, sha, line in prose_cites(path.read_text()):
            cites += 1
            if not prose_cite_resolves(cite, sha):
                where = "the installed node_modules" if cite[0].startswith("@") else sha or "HEAD, the redesign and main"
                problems.append(f"{path.relative_to(REPO)}:{line}: {written_cite(cite)} resolves in none of {where}")
    return result(not problems, "; ".join(problems[:8]) or "every cite a module.md makes resolves at the SHA it names, or at HEAD, the redesign or main, and a package's in the installed node_modules", cites=cites, unresolved=len(problems))


def prose_cites(text):
    found, pending, last_path, full_paths = [], [], None, {}
    for match in PROSE_CITE_TOKEN.finditer(text):
        if match.group("sha") or match.group("stop"):
            found += [(cite, match.group("sha"), line) for cite, line in pending]
            pending = []
            continue
        written = match.group("path")
        if written is None and (last_path is None or text[match.start() - 1:match.start()] not in (" ", "(", "`", ",")):
            continue
        path = written or last_path
        path = path if "/" in path else full_paths.get(path, path)
        last_path = path
        if "/" in path:
            full_paths[path.rsplit("/", 1)[1]] = path
        pending.append(((path, int(match.group("first")), int(match.group("last")) if match.group("last") else None), text.count("\n", 0, match.start()) + 1))
    return found + [(cite, None, line) for cite, line in pending]


def checkout_root():
    return Path(git("rev-parse", "--path-format=absolute", "--git-common-dir")).parent


def installed_modules():
    return next((folder / "node_modules" for folder in [REPO, *REPO.parents] if (folder / "node_modules").is_dir()), checkout_root() / "node_modules")


def installed_package_lines(path):
    for folder in [REPO, *REPO.parents, checkout_root()]:
        candidate = folder / "node_modules" / path
        if candidate.is_file():
            return len(candidate.read_text().splitlines())
    return -1


def written_cite(cite):
    path, first, last = cite
    return f"{path}:{first}-{last}" if last else f"{path}:{first}"


def prose_cite_resolves(cite, sha):
    path, first, last = cite
    needed = max(first, last or first)
    if path.startswith("@"):
        return installed_package_lines(path) >= needed
    return any(line_count(source, path) >= needed for source in ([sha] if sha else [None, BASE_SHA, MAIN_SHA]))


def check_defects():
    if not feature_files():
        return result(False, "not started: no spec/<Module>/features.json")
    model = feature_model()
    doors = all_new_doors(design_sections() or {}) | module_doors()
    analyzer_ids = {row["id"] for row in table("analyzers.tsv")}
    counts = mapping_counts(model)
    counts["defects"] = sum(len(feature.get("defects", [])) for feature in model["features"])
    rungs = core_defect_rungs()
    problems = model["problems"] + defect_problems(model, rungs, doors, analyzer_ids)
    unplaced = unplaced_core_defects(model, rungs)
    counts["core_defects_unplaced"] = len(unplaced)
    if not problems and unplaced:
        return result(False, f"{unmapped_text(model)}; core defects no feature places: {', '.join(unplaced)}", **counts)
    return feature_result(model, problems, counts, "every defect on its rung, every core defect in a feature; no promise for a type or analyzer defect")


CARRIER_ID = r"(?:M\d+\.[DNX]\d+|[DNXR]\d+\.\d+)"
CARRIER_GROUP = re.compile(rf"\(({CARRIER_ID}(?:, {CARRIER_ID})*)\)")
MEANING_NAMED = re.compile(r"\b(?:M\d+\.D\d+|D\d+\.\d+)\b")
DEFECT_NAMED = re.compile(r"\b(?:M\d+\.X\d+|X\d+\.\d+)\b")
CARRIER_NAMED = re.compile(r"\b(?:([A-Z][A-Za-z]+)\.tla:(\w+)|probe:([\w-]+(?:\.[\w-]+)*)|check:(\w+))")
RUNG_SLOT = re.compile(r"^- `?X\d+\.\d+`? (?:type|analyzer|refusal|behavior) \(([^)]*)\):")
DELIVERY_TESTS = re.compile(r"^- The delivery tests run on both fresh apps:(.*?)(?=^- |^#|\Z)", re.MULTILINE | re.DOTALL)
CARRIER_KINDS = ["defect", "invariant", "probe", "check", "cell", "delivery"]


def grouped(text, pattern):
    return {named for group in CARRIER_GROUP.findall(text) for named in pattern.findall(group)}


def stated_meanings(sections):
    lines, defects, problems = {}, {}, []
    for body in sections.values():
        parts = section_parts(body)
        for item in bullets(parts.get("Decision", "")):
            match = re.match(r"^- (D\d+\.\d+) ", item)
            if match:
                problems += [f"{match.group(1)} is stated twice"] if match.group(1) in lines else []
                lines[match.group(1)] = item
        for item in bullets(parts.get("Defects", "")):
            match, slot = DEFECT.match(item), RUNG_SLOT.match(item)
            if match:
                defects[match.group(1)] = grouped(item, MEANING_NAMED) | (set(MEANING_NAMED.findall(slot.group(1))) if slot else set())
    for path in feature_files():
        for feature in json.loads(path.read_text()).get("features", []):
            for meaning in feature.get("meanings", []):
                problems += [f"{meaning['id']} is stated twice"] if meaning["id"] in lines else []
                lines[meaning["id"]] = meaning["meaning"]
            defects.update({defect["id"]: grouped(defect["defect"], MEANING_NAMED) for defect in feature.get("defects", []) if "defect" in defect})
    return lines, defects, problems


def named_carrier(spec, invariant, probe, check, probes, checks):
    if spec:
        defined, listed = tla_invariants(spec)
        return "invariant", f"{spec}.tla:{invariant}", defined is not None and invariant in defined and invariant in listed
    if probe:
        return "probe", f"probe:{probe}", probe in probes
    return "check", f"check:{check}", check in checks


def check_carriers():
    sections = design_sections()
    if sections is None:
        return not_started(SPEC / "Core" / "DESIGN.md")
    lines, defects, problems = stated_meanings(sections)
    carriers = {identifier: set() for identifier in lines}

    def carry(meaning, kind, where):
        if meaning in carriers:
            carriers[meaning].add(kind)
        else:
            problems.append(f"{where} names {meaning}, which no D line or module meaning states")

    for defect, meanings in defects.items():
        for meaning in sorted(meanings):
            carry(meaning, "defect", defect)
    for cell in json.loads((SPEC / "Reference" / "matrix.json").read_text())["cells"]:
        for meaning in cell["meaning"]:
            if MEANING_NAMED.fullmatch(meaning):
                carry(meaning, "cell", f"cell {cell['id']}")
    delivery = DELIVERY_TESTS.search((SPEC / "REWRITE.md").read_text())
    problems += [] if delivery else ["REWRITE.md holds no bullet of the delivery tests"]
    for meaning in sorted(grouped(delivery.group(1), MEANING_NAMED) if delivery else set()):
        carry(meaning, "delivery", "REWRITE.md's delivery tests")
    probes, checks = probe_ids(), {identifier for identifier, _, _ in CHECKS}
    for identifier, text in lines.items():
        for named in sorted(grouped(text, DEFECT_NAMED)):
            if named in defects:
                carriers[identifier].add("defect")
            else:
                problems.append(f"{identifier} names {named}, which no defect is")
        for found in CARRIER_NAMED.findall(text):
            kind, name, resolves = named_carrier(*found, probes, checks)
            if resolves:
                carriers[identifier].add(kind)
            else:
                problems.append(f"{identifier} names {name}, which is no {'invariant its spec defines and its cfg checks' if kind == 'invariant' else kind + ' of spec/'}")
    for path in sorted(SPEC.glob("*/module.md")):
        problems += [f"{path.relative_to(REPO)} cites {named}, which no features.json meaning states"
                     for named in sorted(set(re.findall(r"\bM\d+\.D\d+\b", path.read_text()))) if named not in lines]
    uncarried = [identifier for identifier, kinds in carriers.items() if not kinds]
    problems += [f"carried by nothing: {', '.join(uncarried[:12])}" + (f" and {len(uncarried) - 12} more" if len(uncarried) > 12 else "")] if uncarried else []
    core = sum(1 for identifier in lines if identifier.startswith("D"))
    counts = {"d_lines": core, "meanings": len(lines) - core, "uncarried": len(uncarried)}
    counts.update({kind: sum(1 for kinds in carriers.values() if kind in kinds) for kind in CARRIER_KINDS})
    return result(not problems, "; ".join(problems[:6]) or
                  f"each of {core} D lines and {len(lines) - core} module meanings has at least one carrier that resolves: a defect naming it "
                  "in an id group, or an invariant, probe or check it names, or a matrix cell or delivery test resting on it", **counts)


def unplaced_core_defects(model, rungs):
    placed = {defect.get("id") for feature in model["features"] for defect in feature.get("defects", [])}
    return sorted(set(rungs) - placed, key=lambda defect: tuple(int(part) for part in defect[1:].split(".")))


def promise_problems(model, tests):
    problems = []
    for feature in model["features"]:
        name = feature["id"]
        promised = {}
        for promise in feature.get("promises", []):
            promised.setdefault(promise.get("for"), []).append(promise)
            missing = [field for field in ("page", "gesture", "outcome", "opposite", "defect") if not is_prose(promise.get(field))]
            if missing:
                problems.append(f"{name}: promise for {promise.get('for')} lacks {', '.join(missing)}")
            test = promise.get("existing_test")
            if test is not None and test not in tests:
                problems.append(f"{name}: promise for {promise.get('for')} names {test!r}, no test of tests.tsv")
        for defect in feature.get("defects", []):
            if defect.get("rung") in ("refusal", "behavior") and defect.get("id") not in promised:
                problems.append(f"{name}: {defect.get('rung')} defect {defect.get('id')} has no promise")
        problems += [f"{name}: promise for {key}, no defect of this feature" for key in promised if key not in {defect.get("id") for defect in feature.get("defects", [])}]
    return problems


def check_promises():
    if not feature_files():
        return result(False, "not started: no spec/<Module>/features.json")
    model = feature_model()
    tests = {f"{row['line']} {row['fixture']}.{row['test']}" for row in table("tests.tsv")}
    counts = mapping_counts(model)
    counts["promises"] = sum(len(feature.get("promises", [])) for feature in model["features"])
    cells, counts["matrix_runs"], counts["matrix_promised"] = matrix_promise_problems()
    return feature_result(model, model["problems"] + promise_problems(model, tests) + cells, counts,
                          "every refusal and behavior defect, every cell the native reference runs and each cell MultiSelect tests as its own, has a promise")


MATRIX_ASYNC_KINDS = {"request", "hub"}


def matrix_action_exists(observed):
    spec, _, action = observed.partition(".tla:")
    path = tla_file(spec)
    if not action or not path.exists():
        return False
    text = path.read_text()
    start = text.find("\nNext ==")
    end = text.find("\nSpec ==", start)
    return start >= 0 and re.search(rf"\b{re.escape(action)}\b", text[start:end]) is not None


def matrix_promise_problems():
    problems, unpromised, runs = [], [], 0
    matrix = SPEC / "Reference" / "matrix.json"
    if not matrix.exists():
        return ["not started: spec/Reference/matrix.json absent"], 0, 0
    rungs = core_defect_rungs()
    cells = {cell["id"]: cell for cell in json.loads(matrix.read_text())["cells"]}
    compiled = {name for name, cell in cells.items()
                if cell["outcome"] == "refused" and any(rungs.get(meaning) in MATRIX_COMPILER_RUNGS for meaning in cell["meaning"])}
    tests = {f"{row['line']} {row['fixture']}.{row['test']}" for row in table("tests.tsv")}
    for name, field in [("syncfusion.json", "member"), ("native.json", "members")]:
        path = SPEC / "Reference" / name
        if not path.exists():
            return [f"not started: spec/Reference/{name} absent"], 0, 0
        reference = json.loads(path.read_text())
        own = reference.get("own")
        for cell, entry in reference["cells"].items():
            if field not in entry or cell in compiled:
                continue
            if own is not None and cell not in own:
                problems += [f"spec/Reference/{name} {cell} holds a promise that re-proves a cell ConformanceComponent runs, "
                             "where MultiSelect tests only its own (spec/CLAUDE.md, 2026-11-10)"] if "promise" in entry else []
                continue
            runs += 1
            promise = entry.get("promise")
            missing = [part for part in ("page", "gesture", "outcome", "opposite", "defect") if not is_prose((promise or {}).get(part))]
            if promise is None:
                unpromised.append(f"spec/Reference/{name} {cell} has no promise")
                continue
            if missing:
                problems.append(f"spec/Reference/{name} {cell}: its promise lacks {', '.join(missing)}")
            problems += [f"spec/Reference/{name} {cell} observes {promise['observes']!r}, no action of its spec's Next"] if "observes" in promise and not matrix_action_exists(promise["observes"]) else []
            lane = set(cells[cell]["kinds"]) & MATRIX_ASYNC_KINDS
            problems += [f"spec/Reference/{name} {cell} sends on the {', '.join(sorted(lane))} lane and observes no action"] if lane and "observes" not in promise else []
            problems += [f"spec/Reference/{name} {cell} names {promise['existing_test']!r}, no test of tests.tsv"] if "existing_test" in promise and promise["existing_test"] not in tests else []
    promised = runs - len(unpromised) - len(problems)
    if promised == 0 and not problems:
        return [f"not started: none of the {runs} cells the reference components run has a promise"], runs, promised
    return problems + unpromised, runs, promised


FLUENT_STRING_KINDS = {"text", "url", "element-id", "header-name", "css-class", "js-name", "view-name"}
FLUENT_UNTYPED_KINDS = {"object", "type", "object[]", "type[]"}
FLUENT_TABLE_TITLE = "Callback parameter naming convention (locked):"
FLUENT_CONCEPT_ID = re.compile(r"^(?:D\d+\.\d+|N\d+\.\d+|M\d+\.[DN]\d+)$")
FLUENT_CELL_DECISIONS = {"D7.1": "Positions", "D7.2": "Sources"}
FLUENT_REFUSED_FORMS = re.compile(r"^refused: (\d+) of (\d+) fail as named$", re.M)


def check_fluent(plant=None):
    path = SPEC / "Fluent" / "sentences.json"
    if not path.exists():
        return not_started(path)
    surface, refused = fluent_surface(plant)
    if refused:
        return result(False, refused)
    sentences = json.loads(path.read_text())
    rows = sentences.get("strings", {})
    concepts, concept_counts = fluent_concept_problems(surface, sentences.get("concepts", {}))
    shapes, shape_counts = fluent_shape_problems(surface, rows, sentences.get("kept", {}))
    callbacks, callback_counts = fluent_callback_problems(surface, fluent_callback_table())
    strings, string_counts = fluent_string_problems(surface, rows)
    judged = {"F1": concepts}
    judged["F2"], cell_counts = fluent_cell_problems(surface["cells"], fluent_cell_rows(path))
    judged.update({"F3": shapes, "F4": callbacks, "F5": strings})
    counts = {"members": len(surface["members"]), "lambda_parameters": len(surface["lambdas"]), **concept_counts, **cell_counts,
              **shape_counts, **callback_counts, **string_counts}
    counts.update({check: "pass" if not found else "fail" for check, found in judged.items()})
    first = [problem for found in judged.values() for problem in found[:2]]
    return result(not first, "; ".join(first) or "one concept per name, every Q7 cell compiles, is refused as named or waits on a door the spec marks new, one shape per name, callbacks named as the locked table names them, every string parameter of a closed kind", **counts)


def fluent_surface(plant):
    for project in ["CshtmlStages", "Cells"]:
        build = subprocess.run(["dotnet", "build", str(SPEC / "api" / project), "-nologo", "-v", "q", "-p:EmitCompilerGeneratedFiles=true"],
                               capture_output=True, text=True, cwd=REPO)
        if build.returncode != 0:
            errors = [line.strip() for line in build.stdout.splitlines() if " error " in line]
            return None, f"spec/api/{project} does not build: " + (errors[0] if errors else build.stdout.strip()[-300:])
    with tempfile.TemporaryDirectory() as folder:
        written = Path(folder) / "surface.json"
        command = ["dotnet", "run", "--project", str(SPEC / "probes" / "FluentSurface"), "--", str(REPO), "--out", str(written)]
        completed = subprocess.run(command + (["--plant", str(plant)] if plant else []), capture_output=True, text=True, cwd=REPO)
        if completed.returncode != 0:
            return None, "the fluent console refused: " + next(iter(completed.stderr.strip().splitlines()), "exit " + str(completed.returncode))
        return json.loads(written.read_text()), None


def fluent_known_ids():
    design = (SPEC / "Core" / "DESIGN.md").read_text()
    decisions = set(re.findall(r"^- (D\d+\.\d+) ", design, re.MULTILINE))
    return decisions | all_new_doors(design_sections() or {}) | module_statements()


def fluent_synonym_groups(concepts):
    groups = {}
    for name, ids in sorted(concepts.items()):
        groups.setdefault(tuple(sorted(ids)), []).append(name)
    return [names for names in groups.values() if len(names) > 1]


def fluent_concept_problems(surface, concepts):
    names = sorted({member["member"] for member in surface["members"]})
    known = fluent_known_ids()
    problems = [f"F1 {name}: no concept row" for name in names if name not in concepts]
    problems += [f"F1 {name}: a row names no surface member" for name in sorted(set(concepts) - set(names))]
    for name, ids in sorted(concepts.items()):
        if not ids:
            problems.append(f"F1 {name}: a row names no meaning")
        problems += [f"F1 {name}: {ident} resolves nowhere" for ident in ids if not (FLUENT_CONCEPT_ID.match(ident) and ident in known)]
    return problems, {"F1_names": len(names), "F1_rows": len(concepts), "F1_synonym_groups": len(fluent_synonym_groups(concepts))}


def fluent_type_name(receiver):
    head, bracket, arguments = receiver.partition("<")
    return head.rsplit(".", 1)[-1] + bracket + arguments


def fluent_string_key(member, parameter, rows):
    qualified = f"{member['receiverName']}.{member['member']}({parameter['name']})"
    plain = f"{member['member']}({parameter['name']})"
    return qualified if qualified in rows else plain if plain in rows else None


def fluent_shape(member, rows):
    kinds = []
    for parameter in member["parameters"]:
        kind = parameter["kind"]
        if kind in ("string", "string[]"):
            key = fluent_string_key(member, parameter, rows)
            kind = "literal" if key and rows[key] == "literal" else kind.replace("string", "string:" + (rows[key] if key else "?"), 1)
        kinds.append(kind)
    if len(kinds) > 1 and kinds[0] == "path":
        kinds = kinds[1:]
    return "(" + ",".join(kinds) + ")>" + member["result"]


def fluent_shape_problems(surface, rows, kept):
    stages = fluent_stage_arguments(surface)
    receivers_by_name = {}
    for member in surface["members"]:
        if member["memberKind"] == "method":
            receivers_by_name.setdefault(member["member"], {}).setdefault(member["receiver"], set()).add(fluent_shape(member, rows))
    shared = {name: receivers for name, receivers in sorted(receivers_by_name.items()) if len(receivers) > 1}
    problems, differ, ruled, held = [], 0, 0, set()
    for name, receivers in shared.items():
        groups = {}
        for receiver, shapes in receivers.items():
            groups.setdefault(" | ".join(sorted(shapes)), []).append(fluent_type_name(receiver))
        if len(groups) == 1:
            continue
        differ += 1
        members = [member for member in surface["members"] if member["member"] == name and member["memberKind"] == "method"]
        if fluent_one_shape_under_rules(members, rows, stages):
            ruled += 1
        elif name in kept:
            held.add(name)
        else:
            problems.append(f"F3 {name}: " + "; ".join(f"{shapes} on {', '.join(sorted(names))}" for shapes, names in sorted(groups.items())))
    stale = sorted(set(kept) - held)
    problems += [f"F3 kept row '{name}' names no name that takes two shapes" for name in stale]
    return problems, {"F3_names": len(shared), "F3_differ": differ, "F3_same_under_rules": ruled, "F3_kept": len(held), "F3_stale_kept": len(stale)}


def fluent_stage_arguments(surface):
    arguments = {}
    for member in surface["members"]:
        arguments.setdefault(member["receiverName"], set()).add(fluent_type_arguments(member["receiver"]))
    return arguments


def fluent_type_arguments(receiver):
    return receiver.partition("<")[2].rstrip(">")


def fluent_split_kinds(text):
    kinds, depth, current = [], 0, ""
    for character in text:
        depth += character == "<"
        depth -= character == ">"
        if character == "," and depth == 0:
            kinds.append(current)
            current = ""
        else:
            current += character
    return kinds + [current] if current else kinds


def fluent_ruled_shape(member, rows, stages):
    kinds_text, result = fluent_shape(member, rows)[1:].split(")>", 1)
    kinds = fluent_split_kinds(kinds_text)
    if member["receiverName"] == "LiveObjects":
        kinds = ["path" if kind == "callback1" else kind for kind in kinds]
        result = "source" if result == "chain" else result
    own = fluent_type_arguments(member["receiver"])
    stage = result != member["receiverName"] and bool(own) and own in stages.get(result, set())
    return tuple(kinds), "continues" if result in ("void", "chain") or stage else result


def fluent_one_shape_under_rules(members, rows, stages):
    by_receiver = {}
    for member in members:
        by_receiver.setdefault(member["receiver"], set()).add(fluent_ruled_shape(member, rows, stages))
    every = [shape for shapes in by_receiver.values() for shape in shapes]
    return any(all(any(shape[1] == full[1] and fluent_within(shape[0], full[0]) for full in fullest) for shape in every)
               for fullest in by_receiver.values())


def fluent_within(small, large):
    remaining = iter(large)
    return all(kind in remaining for kind in small)


def fluent_callback_table():
    lines = (REPO / ".claude" / "rules" / "csharp.md").read_text().splitlines()
    table = [line for line in itertools.takewhile(lambda line: line.startswith("|"), itertools.dropwhile(lambda line: not line.startswith("|"), lines[lines.index(FLUENT_TABLE_TITLE) + 1:]))]
    rows = {}
    for line in table[2:]:
        cells = [re.findall(r"`([^`]+)`", cell) for cell in line.strip("|").split("|")]
        rows[cells[0][0]] = cells[1][0] if cells[1] else None
    return rows


def fluent_callback_problems(surface, table):
    rows = {key.partition("<")[0]: name for key, name in table.items() if not key.startswith(("Action<", "Func<"))}
    problems, used, judged, hides, unlisted, renamed, data = [], set(), 0, 0, 0, 0, 0
    for parameter in surface["lambdas"]:
        place = f"{parameter['file']}:{parameter['line']}"
        held = parameter["type"].partition("<")[0]
        nested_pipeline = held == "Pipeline" and parameter["name"] == rows.get("Pipeline") and set(parameter["hiddenTypes"]) <= {"Pipeline"}
        for hidden in [] if nested_pipeline else parameter["hides"]:
            hides += 1
            problems.append(f"F4 {place} '{parameter['name']}' hides the enclosing '{hidden}'")
        if not parameter["framework"]:
            continue
        judged += 1
        if held not in rows:
            unlisted += 1
            problems.append(f"F4 {place} '{parameter['name']}': {parameter['slot']} hands {held}, which has no row")
            continue
        used.add(held)
        if rows[held] is None:
            data += 1
        elif parameter["name"] != rows[held]:
            renamed += 1
            problems.append(f"F4 {place} '{parameter['name']}': {parameter['slot']} is '{rows[held]}'")
    return problems, {"F4_parameters": judged, "F4_slots": len(surface["slots"]), "F4_no_row": unlisted, "F4_other_name": renamed,
                      "F4_data": data, "F4_hides": hides, "F4_rows": len(rows), "F4_rows_unused": len(set(rows) - used)}


def fluent_string_problems(surface, rows):
    literal_positions = next(item for item in bullets("\n".join(design_sections()["Q7"])) if item.startswith("- D7.1 "))
    problems, used, judged, untyped = [], set(), 0, 0
    for member in surface["members"]:
        for parameter in member["parameters"]:
            qualified = f"{member['receiverName']}.{member['member']}({parameter['name']})"
            if parameter["kind"] in FLUENT_UNTYPED_KINDS:
                untyped += 1
                problems.append(f"F5 {qualified}: {parameter['type']}")
                continue
            if parameter["kind"] not in ("string", "string[]") or parameter["callerMemberName"]:
                continue
            judged += 1
            key = fluent_string_key(member, parameter, rows)
            if key is None:
                problems.append(f"F5 {qualified}: no string kind")
                continue
            used.add(key)
            if rows[key] == "literal" and f"`{qualified}`" not in literal_positions:
                problems.append(f"F5 {key}: 'literal' and D7.1 names no such position")
            elif rows[key] not in FLUENT_STRING_KINDS | {"literal"}:
                problems.append(f"F5 {key}: '{rows[key]}' is no string kind")
    stale = sorted(set(rows) - used)
    problems = list(dict.fromkeys(problems)) + [f"F5 row '{key}' names no string parameter" for key in stale]
    return problems, {"F5_parameters": judged, "F5_rows": len(rows), "F5_untyped": untyped, "F5_stale_rows": len(stale)}


def fluent_cell_rows(path):
    return json.loads(path.read_text()).get("cells", {}).get("refused", [])


def fluent_cell_problems(cells, refused):
    named = fluent_cell_names()
    problems = fluent_cell_set_problems(cells, named)
    judged, counts = fluent_cell_judgement(cells, refused)
    forms, passed, total = fluent_refused_forms()
    problems += judged + forms
    counts.update({
        "F2_positions": len(cells["positions"]), "F2_position_gaps": sum(1 for end in cells["positions"] if end["gap"]),
        "F2_sources": len(cells["sources"]), "F2_source_gaps": sum(1 for end in cells["sources"] if end["gap"]),
        "F2_forms": total, "F2_forms_as_named": passed,
    })
    return problems, counts


def fluent_cell_names():
    decisions = {item.split(" ", 2)[1]: item for item in bullets("\n".join(design_sections()["Q7"]))}
    return {decision: [name.split(".", 1)[1] for name in backticked(decisions[decision]) if re.fullmatch(holder + r"\.\w+", name)]
            for decision, holder in FLUENT_CELL_DECISIONS.items()}


def fluent_cell_set_problems(cells, named):
    problems = []
    for decision, holder in FLUENT_CELL_DECISIONS.items():
        ends = {end["name"]: end for end in cells[holder.lower()]}
        problems += [f"F2 {decision} names {name} twice" for name in sorted({name for name in named[decision] if named[decision].count(name) > 1})]
        problems += [f"F2 {decision} names {name}, which spec/api/Cells lacks" for name in named[decision] if name not in ends]
        problems += [f"F2 {end['file']}:{end['line']} {holder}.{name}: {decision} does not name it" for name, end in ends.items() if name not in named[decision]]
    return problems


def fluent_cell_judgement(cells, refused):
    rows = {(row["position"], row["source"]): row for row in refused}
    problems = [f"F2 refused row {position} x {source}: {row['reason']} states no such refusal" for (position, source), row in rows.items()
                if not fluent_decision_stated(row["reason"])]
    doors = fluent_designed_doors()
    cited = set()
    counts = {"F2_cells": len(cells["results"]), "F2_compiling": 0, "F2_refused": 0, "F2_gaps": 0, "F2_failing": 0}
    for cell in cells["results"]:
        label = f"F2 {cell['position']} x {cell['source']}"
        row = rows.pop((cell["position"], cell["source"]), None)
        errors = sorted(cell["errors"])
        if cell["gap"]:
            counts["F2_gaps"] += 1
            problems += [f"{label}: refused for {', '.join(row['errors'])} but has no door"] if row else []
            gap_doors, uncited = fluent_gap_doors(cell["gap"], doors)
            cited |= gap_doors
            problems += [f"{label}: {part} cites no door the spec marks new" for part in uncited]
        elif row is None and not errors:
            counts["F2_compiling"] += 1
        elif row is None:
            counts["F2_failing"] += 1
            problems.append(f"{label}: fails {', '.join(errors)} and is no refused cell")
        elif errors == sorted(row["errors"]):
            counts["F2_refused"] += 1
        elif not errors:
            problems.append(f"{label}: refused for {', '.join(row['errors'])} but compiles")
        else:
            problems.append(f"{label}: refused for {', '.join(row['errors'])} but fails {', '.join(errors)}")
    problems += [f"F2 refused row {position} x {source} names no cell" for position, source in rows]
    counts["F2_gap_doors"] = len(cited)
    return problems, counts


FLUENT_GAP = re.compile(r"^((?:N\d+\.\d+|M\d+\.N\d+)(?:, (?:N\d+\.\d+|M\d+\.N\d+))*): \S")


def fluent_designed_doors():
    texts = [(SPEC / "Core" / "DESIGN.md").read_text()] + [path.read_text() for path in SPEC.glob("*/module.md")]
    return {door for text in texts for door in NEW_DOOR_BULLET.findall(text)}


def fluent_gap_doors(gap, doors):
    cited, uncited = set(), []
    for part in gap.split("; "):
        named = FLUENT_GAP.match(part)
        ids = named.group(1).split(", ") if named else []
        if not ids or any(door not in doors for door in ids):
            uncited.append(f"'{part}'")
        cited |= set(ids) & doors
    return cited, uncited


def fluent_decision_stated(reason):
    starts = (f"- {reason} ", f"- `{reason}` ")
    return any(line.startswith(starts) for lines in design_sections().values() for line in lines)


def fluent_refused_forms():
    run = subprocess.run(["bash", str(SPEC / "api" / "refused.sh")], capture_output=True, text=True, cwd=REPO)
    summary = FLUENT_REFUSED_FORMS.search(run.stdout)
    if summary is None:
        return ["F2 spec/api/refused.sh printed no summary: " + (run.stdout + run.stderr).strip()[-200:]], 0, 0
    failing = [f"F2 spec/api/refused.sh {line}" for line in run.stdout.splitlines() if line.startswith("FAIL")]
    return failing, int(summary.group(1)), int(summary.group(2))


MATRIX_GROUPS = {
    "Properties": "properties", "Methods": "methods", "Events": "events", "Absence": "absence", "As a source": "source",
    "HTTP": "http", "Other lanes": "lanes", "Templates": "templates", "Validation": "validation",
}
MATRIX_OUTCOMES = {"runs", "refused"}
MATRIX_NATIVE_CARRIED = {"templates": "FusionMultiSelect"}
MATRIX_NO_KINDS = {"templates"}
MATRIX_COMPILER_RUNGS = {"type", "analyzer"}
MATRIX_REFUSING_RUNGS = {"type", "analyzer", "refusal"}
MATRIX_GAP = re.compile(r'public static void (\w+)\([^)]*\) => NoDoor\.Position\("([^":]+):')
VENDOR_LOCATE = {"path": {"path"}, "element": set(), "plugin": {"plugin"}}
VENDOR_LISTENING = {"pair": {"subscribe", "unsubscribe"}, "callbackProperty": set(), "returnedUnsubscriber": {"subscribe"}}
VENDOR_ELEMENT_PATHS = {"named", "element", "none"}
VENDOR_NATIVE = {"name": "native", "locate": {"kind": "element"},
                 "listening": {"kind": "pair", "subscribe": "addEventListener", "unsubscribe": "removeEventListener"}, "elementPath": ""}
MATRIX_POC15 = re.compile(r"^event\.poc15\.(\w+)\.(payload|held)\.(before|after)$")
MATRIX_TYPE_KINDS = {"component", "live", "payload", "data"}
MATRIX_CALLED = {"method", "methodValue", "methodAsync", "methodAsyncValue", "returnedObject", "returnedObjects"}
MATRIX_POSITIONAL = {"argument", "methodAt"}
MATRIX_DOORS = {
    "property": ("Property<",), "readOnly": ("ReadOnly<",), "writeOnly": ("WriteOnly<",), "liveObject": ("LiveObject<",),
    "liveObjects": ("LiveObjects<",), "method": ("Method",), "methodValue": ("Method<",), "methodAsync": ("MethodAsync",),
    "methodAsyncValue": ("MethodAsync<",), "returnedObject": ("ReturnedObject<",), "returnedObjects": ("ReturnedObjects<",),
    "callback": ("Callback<",), "event": ("Event", "Event<"), "component": ("LiveObject<",), "argument": ("Argument<",),
    "methodAt": ("MethodAt",),
}


def door_fits(kind, door):
    return any(door == form or (form.endswith("<") and door.startswith(form)) for form in MATRIX_DOORS.get(kind, ()))


def vendor_kinds(vendor):
    element_path = vendor.get("elementPath")
    return {"locate": vendor.get("locate", {}).get("kind"), "listening": vendor.get("listening", {}).get("kind"),
            "elementPath": "none" if element_path is None else "element" if element_path == "" else "named"}


def vendor_problems(name, vendor):
    if not vendor:
        return [f"the component {name} carries no vendor data"]
    problems = [f"the component {name}'s vendor has no name"] if not is_prose(vendor.get("name")) else []
    for axis, closed in [("locate", VENDOR_LOCATE), ("listening", VENDOR_LISTENING)]:
        held = vendor.get(axis) or {}
        needed = closed.get(held.get("kind"))
        if needed is None:
            problems.append(f"the component {name}'s vendor {axis} is {held.get('kind')!r}, none of D17.3's {sorted(closed)}")
            continue
        given = set(held) - {"kind"}
        problems += [f"the component {name}'s {held['kind']} {axis} names no {part}" for part in sorted(needed) if not is_prose(held.get(part))]
        problems += [f"the component {name}'s {held['kind']} {axis} carries {part}, which D17.3 gives it none of" for part in sorted(given - needed)]
    element_path = vendor.get("elementPath")
    problems += [f"the component {name}'s element path is {element_path!r}, no path"] if element_path is not None and not isinstance(element_path, str) else []
    native = vendor.get("name") == VENDOR_NATIVE["name"]
    problems += [f"the component {name}'s vendor is named native and is not ComponentVendor.Native's shape"] if native and vendor != VENDOR_NATIVE else []
    return problems


def matrix_brief_bullets():
    lines = (SPEC / "BRIEF.md").read_text().splitlines()
    start = next(number for number, line in enumerate(lines) if line.startswith("- **The conformance matrix**"))
    end = next(number for number, line in enumerate(lines) if number > start and line.startswith("- **A reference component**"))
    bullets, group = [], None
    for line in lines[start + 1:end]:
        header = re.match(r"^  - \*\*(.+?):\*\*(.*)$", line)
        item = re.match(r"^    - (.*)$", line)
        if header:
            group = MATRIX_GROUPS.get(header.group(1), header.group(1))
            text = header.group(2).strip().rstrip(".;")
            bullets += [(group, text)] if text else []
        elif item:
            bullets.append((group, item.group(1).rstrip(".;")))
    return bullets


def design_ids():
    text = (SPEC / "Core" / "DESIGN.md").read_text()
    return set(re.findall(r"^- (D\d+\.\d+) ", text, re.M)) | set(re.findall(r"^- `(X\d+\.\d+)`", text, re.M))


def matrix_problems(matrix):
    written = [(bullet["group"], bullet["brief"]) for bullet in matrix["bullets"]]
    brief = matrix_brief_bullets()
    problems = [f"matrix bullet {text!r} ({group}) is not BRIEF.md section 8's" for group, text in written if (group, text) not in brief]
    problems += [f"BRIEF.md section 8's {text!r} ({group}) has no matrix bullet" for group, text in brief if (group, text) not in written]
    ids = [cell["id"] for cell in matrix["cells"]]
    listed = {cell: bullet for bullet in matrix["bullets"] for cell in bullet["cells"]}
    counted = [cell for bullet in matrix["bullets"] for cell in bullet["cells"]]
    problems += [f"matrix cell {cell} written twice" for cell in sorted({cell for cell in ids if ids.count(cell) > 1})]
    problems += [f"matrix cell {cell} is under {counted.count(cell)} bullets" for cell in ids if counted.count(cell) != 1]
    problems += [f"a matrix bullet names {cell}, no cell" for cell in counted if cell not in ids]
    positions = [cell.get("position") for cell in matrix["cells"] if cell["group"] == "source"]
    named = fluent_cell_names()["D7.1"]
    problems += [f"D7.1 names Positions.{name}, which no source cell runs" for name in named if name not in positions]
    problems += [f"a source cell runs at Positions.{name}, which D7.1 does not name" for name in positions if name not in named]
    known, kinds, rungs = design_ids(), spec_node_set(), core_defect_rungs()
    gaps = dict(MATRIX_GAP.findall((SPEC / "api" / "Cells" / "Positions.cs").read_text()))
    for cell in matrix["cells"]:
        name = cell["id"]
        problems += ([f"{name} is in group {cell['group']}, its bullet in {listed[name]['group']}"] if name in listed and listed[name]["group"] != cell["group"] else [])
        problems += ([f"{name} states no meaning"] if not cell["meaning"] else [])
        problems += ([f"{name} writes no schema kind"] if not cell["kinds"] and cell["group"] not in MATRIX_NO_KINDS else [])
        problems += ([f"{name} writes {', '.join(cell['kinds'])}, where D15 gives a template no schema node"] if cell["kinds"] and cell["group"] in MATRIX_NO_KINDS else [])
        refusing = [meaning for meaning in cell["meaning"] if rungs.get(meaning) in MATRIX_REFUSING_RUNGS]
        problems += ([f"{name} is refused, and none of {', '.join(cell['meaning'])} is a type, analyzer or refusal defect"] if cell["outcome"] == "refused" and not refusing else [])
        problems += ([f"{name} at Positions.{cell.get('position')} carries the gap {cell.get('gap')!r}, where spec/api/Cells names {gaps.get(cell.get('position'))!r}"] if cell["group"] == "source" and cell.get("gap") != gaps.get(cell.get("position")) else [])
        problems += [f"{name} rests on {meaning}, which spec/Core/DESIGN.md does not state" for meaning in cell["meaning"] if meaning not in known]
        problems += [f"{name} writes {kind}, no kind of spec/Schema/Schema.tla" for kind in cell["kinds"] if kind not in kinds]
        problems += ([f"{name} has the outcome {cell['outcome']!r}"] if cell["outcome"] not in MATRIX_OUTCOMES else [])
    return problems


def native_declaration_problems(types):
    problems = []
    for declared in types.values():
        names = [member["js"] for member in declared["members"]]
        problems += ([f"ConformanceComponent's {declared['name']} is a {declared['kind']!r} type"] if declared["kind"] not in MATRIX_TYPE_KINDS else [])
        problems += vendor_problems(declared["name"], declared.get("vendor")) if declared["kind"] == "component" else []
        problems += [f"the {declared['kind']} {declared['name']} carries vendor data, which only a component does"] if declared["kind"] != "component" and "vendor" in declared else []
        problems += [f"{declared['name']} declares {name} twice" for name in sorted({name for name in names if names.count(name) > 1})]
        for member in declared["members"]:
            label = f"{declared['name']}.{member['js']}"
            problems += ([f"{label} is a {member['kind']} declared by {member['door']}"] if declared["kind"] != "data" and not door_fits(member["kind"], member["door"]) else [])
            positional = member["kind"] in MATRIX_POSITIONAL
            problems += ([f"{label} is declared with {member['door']}, which only a payload declaration takes (D17.4, X17.18)"] if positional and declared["kind"] != "payload" else [])
            problems += ([f"{label} names no argument position"] if positional and not isinstance(member.get("position"), int) else [])
            problems += ([f"{label} names an argument position, which only Argument<T>(n) and MethodAt(n) take"] if not positional and "position" in member else [])
            reached = types.get(member.get("type"))
            problems += ([f"{label} holds {member['type']}, no declared type"] if "type" in member and reached is None else [])
            problems += ([f"{label} holds a component, {member.get('type')} is no component"] if member["kind"] == "component" and (reached or {}).get("kind") != "component" else [])
            handed = member["kind"] == "component" and (reached or {}).get("kind") == "component"
            problems += ([f"{label} hands {member['type']}, whose vendor names no element path, so a listen on it is refused (D17.3)"]
                         if handed and (reached.get("vendor") or {}).get("elementPath") is None else [])
    return problems


def native_resolve(types, path):
    parts = path.split(".")
    current, chain = types.get(parts[0]), []
    for part in parts[1:]:
        if current is None:
            return None, chain, f"{path} reaches past a member that holds no declared type"
        called = part.endswith("()")
        name = part[:-2] if called else part
        member = next((member for member in current["members"] if member["js"] == name), None)
        if member is None:
            return None, chain, f"{path}: {current['name']} declares no {name}"
        if called != (member["kind"] in MATRIX_CALLED):
            return None, chain, f"{path}: {name} is a {member['kind']}"
        chain.append((current, member))
        current = types.get(member.get("type"))
    return (chain[-1][1] if chain else None), chain, None if chain else f"{path} names no member"


def native_door_problems(cell, door, holder, member, chain, types):
    root = chain[0][0]["name"]
    if door == "component" and holder == "payload":
        fits = member["kind"] == "event" and types[member["type"]]["kind"] == "component" and member["type"] != root
    elif door == "component":
        fits = member["kind"] == "component" and chain[-1][0]["kind"] == "payload" and member["type"] != root
    elif holder == "payload":
        fits = member["kind"] == door and chain[-1][0]["kind"] == "payload"
    else:
        fits = member["kind"] == door and chain[-1][0]["kind"] != "payload" and len(chain) > 1 and chain[-2][0]["kind"] == "payload"
    return [] if fits else [f"{cell}: {member['js']} is not a {door} {'on the payload' if holder == 'payload' else 'on an object the payload holds'}"]


def native_problems(native, matrix, syncfusion):
    types = {declared["name"]: declared for declared in native["types"]}
    versions = {version["id"]: version for version in native["versions"]}
    cells = {cell["id"]: cell for cell in matrix["cells"]}
    problems = native_declaration_problems(types)
    problems += [f"native.json runs {name}, no matrix cell" for name in native["cells"] if name not in cells]
    problems += [f"native.json lacks {name}" for name in cells if name not in native["cells"]]
    for version in versions.values():
        problems += [f"version {version['id']}: {error}" for path in version["differs"] for error in [native_resolve(types, path)[2]] if error]
    for name, entry in native["cells"].items():
        if "carriedBy" in entry:
            carrier = MATRIX_NATIVE_CARRIED.get(cells.get(name, {}).get("group"))
            problems += ([f"{name} is carried by {entry['carriedBy']}, which no closed group of check.py names"] if carrier != entry["carriedBy"] else [])
            problems += ([f"{name} is carried by FusionMultiSelect, whose contract lacks it"] if "member" not in syncfusion["cells"].get(name, {}) else [])
            problems += ([f"{name} is carried with no reason"] if not is_prose(entry.get("reason")) else [])
            continue
        version = versions.get(entry["version"])
        problems += ([f"{name} runs on version {entry['version']!r}, which native.json lacks"] if version is None else [])
        resolved = [(path, native_resolve(types, path)) for path in entry["members"]]
        problems += [f"{name}: {error}" for _, (_, _, error) in resolved if error]
        differs = version is not None and version["id"] != "current"
        problems += ([f"{name} runs on {entry['version']} through no member that version changes"] if differs and not set(entry["members"]) & set(version["differs"]) else [])
        door = MATRIX_POC15.match(name)
        if door and "door" in entry:
            member, chain, error = native_resolve(types, entry["door"])
            problems += [f"{name}: {error}"] if error else native_door_problems(name, door.group(1), door.group(2), member, chain, types)
        problems += ([f"{name} names no door member"] if door and "door" not in entry else [])
    return problems


def installed_package_line(path, number):
    for folder in [REPO, *REPO.parents, checkout_root()]:
        candidate = folder / "node_modules" / path
        if candidate.is_file():
            lines = candidate.read_text().splitlines()
            return lines[number - 1] if 0 < number <= len(lines) else None
    return None


def syncfusion_cite_problems(name, member, cites):
    words = set(re.findall(r"[A-Za-z_$][\w$]*", member))
    problems = []
    for cite in cites:
        path, number = cite.rsplit(":", 1)
        line = installed_package_line(path, int(number)) if path.startswith("@syncfusion/") and number.isdigit() else None
        if line is None:
            problems.append(f"{name}: {cite} resolves to no line of the installed package")
        elif not words & set(re.findall(r"[A-Za-z_$][\w$]*", line)):
            problems.append(f"{name}: {cite} reads {line.strip()[:60]!r}, which names none of {member!r}")
    return problems


def m21_n1_text():
    text = (SPEC / "FusionComponents" / "module.md").read_text()
    return text.split("- `M21.N1` kept:", 1)[1].split("- `M21.N2`", 1)[0]


def syncfusion_problems(syncfusion, matrix):
    cells = {cell["id"] for cell in matrix["cells"]}
    facts = {fact["id"] for fact in json.loads((SPEC / "probes" / "facts.json").read_text())["facts"]}
    problems = [f"syncfusion.json judges {name}, no matrix cell" for name in syncfusion["cells"] if name not in cells]
    problems += [f"syncfusion.json lacks {name}" for name in sorted(cells) if name not in syncfusion["cells"]]
    for name, entry in syncfusion["cells"].items():
        problems += [f"{name} rests on the fact {fact}, which spec/probes/facts.json lacks" for fact in entry.get("facts", []) if fact not in facts]
        if "member" in entry:
            problems += ([f"{name} cites nothing"] if not entry.get("cites") else [])
            problems += syncfusion_cite_problems(name, entry["member"], entry.get("cites", []))
        else:
            problems += ([f"{name} lacks a reason or its scan"] if not is_prose(entry.get("lacks")) or not is_prose(entry.get("scan")) else [])
    for name, reason in syncfusion.get("own", {}).items():
        entry = syncfusion["cells"].get(name, {})
        problems += ([f"own {name} is no cell MultiSelect covers"] if "member" not in entry else [])
        problems += ([f"own {name} gives no reason Syncfusion's onboarding needs its test"] if not is_prose(reason) else [])
    door = m21_n1_text()
    for kept in syncfusion["kept"]:
        problems += syncfusion_cite_problems(f"kept {kept['js']}", kept["js"], [kept["cite"]])
        problems += ([f"kept {kept['js']}: M21.N1 does not name `{kept['csharp']}`"] if f"`{kept['csharp']}" not in door else [])
    for added in syncfusion["added"]:
        problems += syncfusion_cite_problems(f"added {added['js']}", added["js"], [added["cite"]])
        name = added["csharp"].split("(", 1)[0]
        problems += ([f"added {added['js']}: M21.N1 in spec/FusionComponents/module.md does not name `{name}`"] if f"`{name}" not in door else [])
    return problems


def native_test_names():
    names = set()
    for path in feature_files():
        for feature in json.loads(path.read_text()).get("features", []):
            seen = Counter()
            for promise in feature.get("promises", []):
                names |= {promise["for"], f"{promise['for']} #{seen[promise['for']]}"}
                seen[promise["for"]] += 1
    return names | set(json.loads((SPEC / "Reference" / "native.json").read_text())["cells"])


def check_native_behaviors():
    native = json.loads((SPEC / "Reference" / "native.json").read_text())
    known = native_test_names()
    problems, count = [], 0
    for declared in native["types"]:
        for entry in declared.get("behaviors", []):
            count += 1
            problems += [f"{declared['name']} states a behavior with no sentence"] if not is_prose(entry.get("behavior")) else []
            problems += [f"{declared['name']}'s behavior {entry.get('behavior', '')[:40]!r} names no test"] if not entry.get("tests") else []
            problems += [f"{declared['name']}'s behavior names {test}, no test of the spec" for test in entry.get("tests", []) if test not in known]
    holders = sum(1 for declared in native["types"] if declared.get("behaviors"))
    detail = f"{count} behaviors on {holders} declarations of native.json, each naming tests of the spec"
    return result(not problems, "; ".join(problems[:6]) or detail, behaviors=count, problems=len(problems))


def check_matrix():
    paths = [SPEC / "Reference" / name for name in ["matrix.json", "syncfusion.json", "native.json"]]
    if any(not path.exists() for path in paths):
        return not_started(*paths)
    matrix, syncfusion, native = (json.loads(path.read_text()) for path in paths)
    problems = matrix_problems(matrix) + syncfusion_problems(syncfusion, matrix) + native_problems(native, matrix, syncfusion)
    total = len(matrix["cells"])
    covered = sum(1 for entry in syncfusion["cells"].values() if "member" in entry)
    carried = sum(1 for entry in native["cells"].values() if "carriedBy" in entry)
    own = len(syncfusion.get("own", {}))
    detail = (f"MultiSelect covers {covered} of {total} cells from its .d.ts and tests {own} of them as its own; "
              f"ConformanceComponent runs {total - carried} of {total}, and FusionMultiSelect carries the other {carried}")
    return result(not problems, "; ".join(problems[:6]) or detail, cells=total, bullets=len(matrix["bullets"]),
                  syncfusion=covered, own=own, native=total - carried, carried=carried, problems=len(problems))


REWRITE_UNIT = re.compile(r"^### (U(\d+))\. (.+)$")
REWRITE_FEATURES = re.compile(r"^Features: (.+)$")
REWRITE_LABEL = re.compile(r"^(Features|Doors): (.+)$")


def rewrite_units(text):
    units, current, listing = [], None, None
    for line in text.splitlines():
        heading = REWRITE_UNIT.match(line)
        labelled = REWRITE_LABEL.match(line)
        if heading:
            current, listing = {"id": heading.group(1), "number": int(heading.group(2)), "features": None, "doors": None}, None
            units.append(current)
        elif listing is not None and line.strip() and not line.startswith("#") and not labelled:
            listing.append(line.strip())
        else:
            listing = [labelled.group(2)] if labelled and current is not None else None
            if listing is not None:
                current[labelled.group(1).lower()] = listing
    for unit in units:
        for label in ("features", "doors"):
            if unit[label] is not None:
                joined = " ".join(unit[label])
                unit[label] = [] if joined == "none" else [name.strip() for name in joined.split(",")]
    return units


def part_module(app, part):
    return part.get("module") or app.get("partModules", {}).get(part["kind"])


def door_units(units):
    brought = {}
    for place, unit in enumerate(units):
        for door in unit.get("doors") or []:
            brought.setdefault(door, place)
    return brought


def part_unit(app, part, module_unit, brought):
    doors = part.get("doors")
    if doors and all(door in brought for door in doors):
        return max(brought[door] for door in doors)
    return module_unit.get(part_module(app, part))


def rewrite_landing(app, module_unit, start, entry, brought):
    parts = {part["id"]: part for part in next(screen for screen in app["screens"] if screen["id"] == entry["screen"])["parts"]}
    needed = [part_unit(app, parts[name], module_unit, brought) for name in entry["parts"] if name in parts]
    return max([start] + [place for place in needed if place is not None])


def unit_homes(units):
    home = {}
    for place, unit in enumerate(units):
        for feature in unit["features"] or []:
            home.setdefault(feature, place)
    return home


def module_units(home, modules):
    module_unit = {}
    for feature, place in home.items():
        if feature in modules:
            module_unit[modules[feature]] = min(place, module_unit.get(modules[feature], place))
    return module_unit


def test_landings(app, home, module_unit, brought):
    landed = Counter()
    for entry in app["placements"]:
        if entry["feature"] in home:
            landed[rewrite_landing(app, module_unit, home[entry["feature"]], entry, brought)] += 1
    for entry in app["cells"]:
        owner = module_unit.get("Vendors" if "cell" in entry else "FusionComponents")
        if owner is not None:
            landed[rewrite_landing(app, module_unit, owner, entry, brought)] += 1
    return landed


HARNESS_TEST_TEXT = ("page", "gesture", "outcome", "opposite", "defect")


def fresh_app_hosts(screens):
    return {host for screen in screens if screen["area"] == "delivery" for host in screen.get("hosts", [])}


def harness_test_problems(app, unit_ids):
    hosts, fresh = {host["id"] for host in app["hosts"]}, fresh_app_hosts(app["screens"])
    named = Counter((entry.get("fixture"), entry.get("test")) for entry in app.get("harnessTests", []))
    problems = [f"harness test {fixture}.{test} is listed {count} times" for (fixture, test), count in named.items() if count > 1]
    for entry in app.get("harnessTests", []):
        where = f"harness test {entry.get('fixture')}.{entry.get('test')}"
        problems += [f"{where}'s fixture is not named When and its situation"] if not re.fullmatch(r"When[A-Z]\w*", entry.get("fixture") or "") else []
        problems += [f"{where} runs under {host!r}, a fresh app's server, which holds no world" for host in entry.get("hosts", []) if host in fresh]
        problems += [f"{where} names {entry.get('unit')!r}, no unit of REWRITE.md"] if entry.get("unit") not in unit_ids else []
        problems += [f"{where} runs on its {key} {entry[key]!r}; a harness test runs on no framework part" for key in ("screen", "parts") if key in entry]
        problems += [f"{where} states no {field}" for field in HARNESS_TEST_TEXT if not is_prose(entry.get(field))]
        problems += [f"{where} runs under {host!r}, no host of app.json" for host in entry.get("hosts", []) if host not in hosts]
        problems += [f"{where} runs under no host"] if not entry.get("hosts") else []
        problems += [f"{where} uses {verb!r}, no harness verb of app.json" for verb in entry.get("harness", []) if verb not in app["harness"]]
        problems += app_held_problems(where, {**entry, "harness": entry.get("harness", [])})
    return problems


def check_unit_proofs():
    path = SPEC / "REWRITE.md"
    if not path.exists():
        return not_started(path)
    units = rewrite_units(path.read_text())
    features = {feature["id"]: (feature_path.parent.name, feature) for feature_path in feature_files()
                for feature in json.loads(feature_path.read_text()).get("features", [])}
    home = unit_homes(units)
    app = json.loads((SPEC / "Reference" / "app.json").read_text())
    landed = test_landings(app, home, module_units(home, {name: module for name, (module, _) in features.items()}), door_units(units))
    analyzer = Counter({place: 0 for place in range(len(units))})
    module, core = Counter({place: 0 for place in range(len(units))}), core_modules()
    for name, place in home.items():
        if name in features:
            analyzer[place] += sum(1 for defect in features[name][1].get("defects", []) if defect.get("rung") == "analyzer")
            module[place] += len(features[name][1].get("nodes", [])) if features[name][0] in core else 0
    places = {unit["id"]: place for place, unit in enumerate(units)}
    own = Counter(places[entry["unit"]] for entry in app.get("harnessTests", []) if entry.get("unit") in places)
    problems = harness_test_problems(app, places)
    problems += [f"{unit['id']} proves nothing: no browser test lands in it, no core feature of it writes a node a module test reads, none of its features has an analyzer defect, and app.json gives it no harness test"
                 for place, unit in enumerate(units) if not (landed[place] or module[place] or analyzer[place] or own[place])]
    proofs = [f"{unit['id']} " + " ".join(f"{count}{mark}" for count, mark in ((landed[place], "b"), (module[place], "m"), (analyzer[place], "a"), (own[place], "h")) if count)
              for place, unit in enumerate(units)]
    detail = (f"each of {len(units)} units proves something, as browser tests (b), the nodes its core features write, which its module tests read (m), "
              "analyzer cases (a) or harness tests (h): " + ", ".join(proofs))
    return result(not problems, "; ".join(problems[:6]) or detail, units=len(units), harness_tests=len(app.get("harnessTests", [])), problems=len(problems))


def check_rewrite():
    path = SPEC / "REWRITE.md"
    if not path.exists():
        return not_started(path)
    units = rewrite_units(path.read_text())
    modules = {feature["id"]: feature_path.parent.name for feature_path in feature_files()
               for feature in json.loads(feature_path.read_text()).get("features", [])}
    problems = [f"{unit['id']} stands at place {place}" for place, unit in enumerate(units, 1) if unit["number"] != place]
    problems += [f"{unit['id']} lists no Features line" for unit in units if unit["features"] is None]
    home = {}
    for place, unit in enumerate(units):
        for feature in unit["features"] or []:
            problems += [f"{unit['id']} lists {feature}, no feature of spec/*/features.json"] if feature not in modules else []
            problems += [f"{feature} is in {units[home[feature]]['id']} and {unit['id']}"] if feature in home else []
            home.setdefault(feature, place)
    problems += [f"{feature} is in no unit" for feature in sorted(modules) if feature not in home]
    module_unit = module_units(home, modules)
    app = json.loads((SPEC / "Reference" / "app.json").read_text())
    problems += [f"{screen['id']}'s {part['id']} needs {part_module(app, part)!r}, the module of no unit"
                 for screen in app["screens"] for part in screen["parts"] if part_module(app, part) not in module_unit]
    known = all_new_doors(design_sections() or {}) | module_doors()
    brought = {}
    for unit in units:
        for door in unit["doors"] or []:
            problems += [f"{unit['id']}'s Doors line names {door}, no core or module door"] if door not in known else []
            problems += [f"{door} is on the Doors lines of {brought[door]} and {unit['id']}"] if door in brought else []
            brought.setdefault(door, unit["id"])
    problems += [f"{screen['id']}'s {part['id']} lists {door}, no core or module door"
                 for screen in app["screens"] for part in screen["parts"] for door in part.get("doors", []) if door not in known]
    problems += [f"{screen['id']}'s {part['id']} lists {door}, which no unit's Doors line brings"
                 for screen in app["screens"] for part in screen["parts"] for door in part.get("doors", []) if door in known and door not in brought]
    landed = test_landings(app, home, module_unit, door_units(units))
    detail = (f"{len(units)} units hold {len(home)} features once; the browser tests land "
              + ", ".join(f"{unit['id']} {landed[place]}" for place, unit in enumerate(units)))
    return result(not problems, "; ".join(problems[:6]) or detail, units=len(units), features=len(home),
                  tests=sum(landed.values()), problems=len(problems))


TREE_NAME = re.compile(r"^[\w.<>*\-]+(?:/[\w.<>*\-]*)*$")
TABLE_RULE = re.compile(r"^\|[-|: ]+\|$")
PACKAGE_COUNT = re.compile(r"\*\*(\w+) packages, one version\.\*\*")
PACKED_COUNT = re.compile(r"the (\w+) packages packed")
COUNT_WORDS = {"four": 4, "five": 5, "six": 6, "seven": 7, "eight": 8}
PACKAGE_RETIRED = {"Alis.Reactive.FluentValidator": ("CLAUDE.md", "if not needed remove it that is better")}
PACKAGE_ADDED = {"Alis.Reactive.OpenTelemetry": ("Tracing/module.md", "The default: the package `Alis.Reactive.OpenTelemetry`")}
DECIDED_REFERENCES = {
    "System.Text.Json": ("CLAUDE.md", "set beside System.Text.Json's 10.0.12"),
    "Microsoft.Extensions.DependencyInjection.Abstractions": ("CLAUDE.md", "the DI abstractions take one exact version per target framework"),
    "System.Diagnostics.DiagnosticSource": ("Core/DESIGN.md", "`System.Diagnostics.DiagnosticSource` package, pinned to one version"),
}
PINNED_PACKAGE = re.compile(r"(@?[A-Za-z][\w.\-/]*) \[?(\d+\.\d+\.\d+)\]?")
SHIPPED_BUNDLE = re.compile(r"^alis-reactive(?:-[a-z0-9]+)*\.js$")
TARGET_NAMES = re.compile(r"net48|net10\.0")


def rewrite_section(text, heading):
    start = text.find(heading)
    if start < 0:
        return ""
    end = re.search(r"\n#{2,3} ", text[start + len(heading):])
    return text[start:start + len(heading) + end.start()] if end else text[start:]


def table_rows(section):
    rows = [[cell.strip() for cell in line.strip()[1:-1].split(" | ")] for line in section.splitlines()
            if line.startswith("| ") and not TABLE_RULE.match(line.strip())]
    return rows[1:]


def tree_names(part):
    tokens = part.split(" ")
    return tokens if all(TREE_NAME.match(token) and ("." in token or token.endswith("/")) for token in tokens) else []


def rewrite_tree(text):
    block = rewrite_section(text, "### 2.1 The tree")
    block = block[block.find("```") + 3:]
    block = block[:block.find("```")]
    held, under_tests = set(), False
    for line in block.splitlines():
        if not line.strip():
            continue
        indent = len(line) - len(line.lstrip(" "))
        names = []
        for part in re.split(r"\s{2,}", line.strip()):
            found = tree_names(part)
            if not found:
                break
            names += found
        if indent == 0:
            under_tests = names == ["tests/"]
            held |= {name.rstrip("/") for name in names}
        elif indent == 2 and under_tests:
            held |= {"tests/" + name.split("/<")[0].rstrip("/") for name in names}
    return held


def package_id(project):
    return "AlisReactive" + project[len("Alis.Reactive"):]


def stated(relative, phrase):
    return " ".join(phrase.split()) in " ".join((SPEC / relative).read_text().split())


CENTRAL_VERSIONS = "Directory.Packages.props"


def kept_references(held):
    folders = sorted(path for path in held if path not in ("tests", "spec"))
    references = {}
    for path in git("ls-files", "*.csproj", CENTRAL_VERSIONS).splitlines():
        if path == CENTRAL_VERSIONS or any(path.startswith(folder + "/") for folder in folders):
            text = (REPO / path).read_text()
            for name, version in re.findall(r'Include="([^"$]+)" Version(?:Override)?="([^"]+)"', text) + re.findall(r'Sdk="([^"/]+)/([^"]+)"', text):
                references.setdefault(name, set()).add((path, version))
    return references


def pin_problems(pins, name, version, where):
    rows = [pin for pin in pins if re.search(r"(?<![\w.@/])" + re.escape(name) + r"(?![\w.\-/])", pin)]
    if not rows:
        return [f"{where} references {name}, which section 2.7 pins nowhere"]
    exact = version.strip("[]")
    if re.fullmatch(r"\d+\.\d+\.\d+", exact) and not any(re.search(r"(?<![\d.])" + re.escape(exact) + r"(?![\d.])", pin) for pin in rows):
        return [f"{where} takes {name} {exact}, and section 2.7 pins another version"]
    return []


def check_packages():
    path = SPEC / "REWRITE.md"
    if not path.exists():
        return not_started(path)
    text = path.read_text()
    rows = {row[0]: row for row in table_rows(rewrite_section(text, "### 2.9 Packages and delivery")) if row[0].startswith("AlisReactive")}
    pin_rows = table_rows(rewrite_section(text, "### 2.7 Tools and pins"))
    pins = [row[1] for row in pin_rows if len(row) > 1]
    problems = [f"the decision {relative} states for {name} is no longer there" for decisions in (PACKAGE_RETIRED, PACKAGE_ADDED, DECIDED_REFERENCES)
                for name, (relative, phrase) in decisions.items() if not stated(relative, phrase)]
    expected = {package_id(name): MAIN_PACKAGES[name] for name in MAIN_PACKAGES if name not in PACKAGE_RETIRED}
    expected.update({package_id(name): None for name in PACKAGE_ADDED})
    problems += [f"section 2.9 has no row for {name}" for name in sorted(expected) if name not in rows]
    problems += [f"section 2.9 packs {name}, which no decision keeps or adds" for name in sorted(rows) if name not in expected]
    for name, row in sorted(rows.items()):
        targets = set(TARGET_NAMES.findall(row[1]))
        if (expected.get(name) and targets != set(expected[name])) or not targets:
            problems.append(f"{name} targets {sorted(targets)}, and main ships it for {expected.get(name)}")
        problems += [f"{name} ships {asset}, no {SHIPPED_BUNDLE.pattern} bundle" for asset in re.findall(r"`([^`]+)`", row[4]) if not SHIPPED_BUNDLE.match(asset)]
        for reference, version in PINNED_PACKAGE.findall(row[5]):
            problems += pin_problems(pins, reference, version, f"{name}'s dependencies")
    core = rows.get("AlisReactive", ["", "", "", "", "", ""])[5]
    problems += [f"AlisReactive's dependencies lack {name}, which {relative} decides" for name, (relative, _) in DECIDED_REFERENCES.items()
                 if not re.search(re.escape(name) + r" \d+\.\d+\.\d+", core)]
    problems += [f"section 2.7's {row[0]} pins no version" for row in pin_rows if len(row) > 1 and not re.search(r"\d+\.\d+|by SHA", row[1])]
    references = kept_references(rewrite_tree(text))
    for name, uses in sorted(references.items()):
        for project, version in sorted(uses):
            problems += pin_problems(pins, name, version, project)
    counts = [(where, found) for where, pattern in (("section 1", PACKAGE_COUNT), ("section 4", PACKED_COUNT)) for found in pattern.findall(text)]
    problems += [f"{where} says {word} packages, and section 2.9 packs {len(rows)}" for where, word in counts if COUNT_WORDS.get(word.lower()) != len(rows)]
    problems += [f"{where} states no package count" for where in ("section 1", "section 4") if where not in {where for where, _ in counts}]
    detail = (f"section 2.9 packs {len(rows)} ids, main's but {', '.join(PACKAGE_RETIRED)} and the decided {', '.join(PACKAGE_ADDED)}; "
              f"each ships only bundles, each dependency and each of {len(references)} references the kept projects take is pinned in section 2.7 at its version")
    return result(not problems, "; ".join(problems[:6]) or detail, packages=len(rows), pins=len(pin_rows), references=len(references), problems=len(problems))


PATH_EXPANDED = ["tests", "tools", "build", "examples", "scripts", ".github", ".github/workflows", ".claude", ".claude/rules",
                 ".claude/skills", ".claude/agents", ".claude/hooks", ".claude/commands", ".claude/workflows", ".claude/memory"]
PATH_FILES = "tests/Alis.Reactive.ArchitectureTests"
BACKTICKED_PATH = re.compile(r"`([^`\s]+)`")


def tree_entries(sha):
    entries = set(git("ls-tree", "--name-only", sha).splitlines())
    for folder in PATH_EXPANDED + [PATH_FILES]:
        if folder in entries or any(entry.startswith(folder + "/") for entry in entries):
            entries.discard(folder)
            entries |= set(git("ls-tree", "--name-only", sha, folder + "/").splitlines())
    return {entry for entry in entries if entry not in PATH_EXPANDED and entry != PATH_FILES}


def tree_line(text, path):
    block = rewrite_section(text, "### 2.1 The tree")
    name = path.split("/")[-1] + "/"
    lines = block.splitlines()
    for index, line in enumerate(lines):
        if line.strip().startswith(name):
            following = list(itertools.takewhile(lambda next_line: next_line.startswith(" " * 20), lines[index + 1:]))
            return " ".join([line] + following)
    return ""


def check_rewrite_cites():
    path = SPEC / "REWRITE.md"
    if not path.exists():
        return not_started(path)
    problems, cites = [], 0
    for cite, sha, line in prose_cites(path.read_text()):
        cites += 1
        if sha is None:
            problems.append(f"spec/REWRITE.md:{line}: {written_cite(cite)} names no SHA, and the cut and every later commit move the lines it cites")
        elif not prose_cite_resolves(cite, sha):
            problems.append(f"spec/REWRITE.md:{line}: {written_cite(cite)} resolves nowhere at {sha}")
    return result(not problems, "; ".join(problems[:6]) or f"each of {cites} cites REWRITE.md makes names its SHA and resolves there",
                  cites=cites, problems=len(problems))


def check_paths():
    path = SPEC / "REWRITE.md"
    if not path.exists():
        return not_started(path)
    text = path.read_text()
    kept_text = text[:text.find("\n## 3.")]
    retired_text = rewrite_section(text, "## 3. What retires")
    held = rewrite_tree(text) | set(BACKTICKED_PATH.findall(kept_text))
    retired = [token.rstrip("/") for token in BACKTICKED_PATH.findall(retired_text)]
    files_line = tree_line(text, PATH_FILES)
    judged = tree_entries("HEAD") | tree_entries(MAIN_TIP)
    problems = []
    for entry in sorted(judged):
        stem = entry.split("/")[-1].split(".")[0]
        keeps = entry in held or entry.rstrip("/") + "/" in held or (entry.startswith(PATH_FILES + "/") and re.search(r"\b" + re.escape(stem) + r"\b", files_line))
        if entry.startswith(PATH_FILES + "/") and entry.endswith(".csproj"):
            keeps = True
        retires = any(fnmatch.fnmatch(entry, token) for token in retired)
        if keeps and retires:
            problems.append(f"{entry} is both kept by section 2 and retired by section 3")
        elif not keeps and not retires:
            problems.append(f"{entry} has no verdict: section 2 does not keep it and section 3 does not retire it")
    detail = (f"each of {len(judged)} paths at HEAD and main's tip, down to {', '.join(PATH_EXPANDED[:6])} and .claude's folders, "
              "is kept by section 2 or retired by section 3, never both")
    return result(not problems, "; ".join(problems[:8]) or detail, paths=len(judged), problems=len(problems))


TEST_RULE = re.compile(r"^(\d+)\. \*\*(.+?)\.\*\*")
TEST_SMELL = re.compile(r"^- (.+) \(\d+(?:, \d+)*\)\.$")
TEST_ID_ROW = re.compile(r"^\| (TEST\d{3}) \| .+ \|$")
TEST_HELD_ROW = re.compile(r"^\| ([^|]+?) \| ([^|]+) \|$")
TEST_RANGE = re.compile(r"TEST001 to (TEST\d{3})")
TEST_CHECKS = {"the bindings check", "the planted-defect run", "check_promises", "check_app", "the harness"}


def logical_lines(text):
    lines = []
    for line in text.splitlines():
        if lines and lines[-1].strip() and line.startswith(" ") and line.strip():
            lines[-1] += " " + line.strip()
        else:
            lines.append(line)
    return lines


def test_rules_section(text):
    start = text.find("### 2.10 The test rules")
    return text[start:text.find("\n## 3.", start)] if start >= 0 else ""


TREE_FOLDER = re.compile(r"^([A-Z]\w*)/")
TLA_NAME = re.compile(r"`(\w+)\.tla`")


def tree_blocks(text):
    block = rewrite_section(text, "### 2.1 The tree")
    block = block[block.find("```") + 3:]
    blocks, current = {}, None
    for line in block[:block.find("```")].splitlines():
        if line[:1] not in ("", " "):
            first = line.split()[0]
            current = blocks.setdefault(first, []) if first.startswith("Alis.Reactive") and first.endswith("/") else None
        elif current is not None and line.strip():
            current += line.split()
    return blocks


def spec_line_names(text):
    return set(re.findall(r"(?<![\w/.])(\.?[\w-][\w.-]*\.\w+|\.[\w-]+|[\w-]+/)(?=[,\s]|$)", tree_line(text, "spec").split(None, 1)[-1]))


def names_problems(text):
    modules, core = set(module_names()), core_modules()
    blocks = tree_blocks(text)
    core_tree = [token.rstrip("/") for token in blocks.get("Alis.Reactive/", []) if re.fullmatch(r"[A-Z]\w*/", token)]
    homes = {found.group(1) for tokens in blocks.values() for token in tokens for found in [TREE_FOLDER.match(token)] if found}
    problems = [f"Alis.Reactive/{name}/ in section 2.1 is no core module of spec/CLAUDE.md's table" for name in core_tree if name not in core]
    problems += [f"core module {name} has no folder in section 2.1's Alis.Reactive/" for name in sorted(core - set(core_tree))]
    problems += [f"module {name} has no folder in the projects of section 2.1" for name in sorted(modules - homes)]
    rows = table_rows(rewrite_section(text, "### 2.3 The SliceTree"))
    problems += [f"SliceTree row {row[0]} stands at place {place}" for place, row in enumerate(rows, 1) if row[0] != str(place)]
    sliced = Counter(name for row in rows for name in row[1].split())
    problems += [f"SliceTree row {row[0]} holds {name}, no core module" for row in rows for name in row[1].split() if name not in core]
    problems += [f"{name} is in {sliced[name]} SliceTree rows" for name in sorted(core) if sliced[name] != 1]
    listed = [(name, row[1]) for row in table_rows(rewrite_section(text, "### 2.6 TLA+")) for name in TLA_NAME.findall(row[0])]
    on_disk = {path.stem: path.parent.relative_to(SPEC).as_posix() for path in SPEC.glob("**/*.tla")}
    shared = Counter(path.stem for path in SPEC.glob("**/*.tla"))
    problems += [f"{name}.tla is the name of {count} specs under spec/; each TLA+ spec's file name is its own" for name, count in sorted(shared.items()) if count > 1]
    problems += [f"section 2.6's owner {owner} is no module" for owner in sorted({owner for _, owner in listed} - modules)]
    problems += [f"section 2.6 lists {name}.tla {count} times" for name, count in Counter(name for name, _ in listed).items() if count > 1]
    for name, owner in listed:
        if name not in on_disk:
            problems.append(f"section 2.6 lists {name}.tla under {owner}; spec/{owner}/{name}.tla is absent")
        elif on_disk[name] != owner:
            problems.append(f"section 2.6 lists {name}.tla under {owner}; it is spec/{on_disk[name]}/{name}.tla")
    problems += [f"spec/{folder}/{name}.tla is in no row of section 2.6" for name, folder in sorted(on_disk.items()) if name not in dict(listed)]
    problems += [f"section 2.6 lists {name}.tla, which TLC_SPECS does not run" for name, _ in listed if name not in TLC_SPECS]
    problems += [f"TLC_SPECS runs {name}, in no row of section 2.6" for name in TLC_SPECS if name not in dict(listed)]
    entries = {name.split("/")[1] + ("/" if name.count("/") > 1 else "") for name in git("ls-files", "spec").splitlines()}
    named = spec_line_names(text)
    problems += [f"module {name} has no spec/{name}/module.md" for name in sorted(modules) if not (SPEC / name / "module.md").is_file()]
    problems += [f"spec/{entry} is neither a module folder nor named by section 2.1's spec line" for entry in sorted(entries)
                 if entry.rstrip("/") not in modules and entry not in named]
    problems += [f"section 2.1's spec line names spec/{name}, which is absent" for name in sorted(named - entries)]
    counts = {"modules": len(modules), "core": len(core), "slice_rows": len(rows), "tla": len(listed), "spec_entries": len(entries)}
    return problems, counts


def check_names():
    path = SPEC / "REWRITE.md"
    if not path.exists():
        return not_started(path)
    problems, counts = names_problems(path.read_text())
    detail = (f"each of the module table's {counts['core']} core modules is a folder of section 2.1's Alis.Reactive/ and in one of "
              f"{counts['slice_rows']} SliceTree rows, each of its {counts['modules']} modules a folder of section 2.1's projects and "
              f"spec/<Module>/module.md; each of {counts['tla']} TLA+ specs in section 2.6 sits at spec/<Owner>/ and TLC runs it; "
              f"each of spec/'s {counts['spec_entries']} entries is a module folder or named by section 2.1's spec line")
    return result(not problems, "; ".join(problems[:8]) or detail, problems=len(problems), **counts)


def check_test_rules():
    path = SPEC / "REWRITE.md"
    if not path.exists():
        return not_started(path)
    lines = logical_lines((SPEC.parent / ".claude" / "rules" / "tests.md").read_text())
    rules = [f"{found.group(1)}. {found.group(2)}" for found in map(TEST_RULE.match, lines) if found]
    smells = [found.group(1) for found in map(TEST_SMELL.match, lines) if found]
    text = path.read_text()
    section = test_rules_section(text).splitlines()
    ids = [found.group(1) for found in map(TEST_ID_ROW.match, section) if found]
    held = [(found.group(1), [name.strip() for name in found.group(2).split(",")]) for found in map(TEST_HELD_ROW.match, section)
            if found and not TEST_ID_ROW.match(found.group(0)) and found.group(1) not in ("Id", "---", "`.claude/rules/tests.md`")]
    problems = [] if section else ["REWRITE.md has no section 2.10 The test rules"]
    problems += [f"test rule {name} stands at place {place}" for place, name in enumerate(ids, 1) if name != f"TEST{place:03d}"]
    keys = Counter(key for key, _ in held)
    problems += [f"tests.md's {line!r} is held by {keys[line]} rows" for line in rules + smells if keys[line] != 1]
    problems += [f"REWRITE.md holds {key!r}, no rule or smell of tests.md" for key in keys if key not in rules + smells]
    used = {name for _, names in held for name in names}
    problems += [f"{key!r} is held by {name!r}, no test rule and no named check" for key, names in held for name in names
                 if name not in ids and name not in TEST_CHECKS]
    problems += [f"{name} holds no rule or smell of tests.md" for name in ids if name not in used]
    last = ids[-1] if ids else "TEST000"
    problems += [f"REWRITE.md says TEST001 to {named}, and the rules end at {last}" for named in TEST_RANGE.findall(text) if named != last]
    detail = (f"tests.md's {len(rules)} rules and {len(smells)} smells each held in REWRITE.md by test rules TEST001 to {last} "
              f"or a named check, every test rule holding one")
    return result(not problems, "; ".join(problems[:6]) or detail, rules=len(rules), smells=len(smells), ids=len(ids),
                  problems=len(problems))


CLAUDE_REPLACED = [
    ("CLAUDE.md", "(rare: the grammar is closed)"),
    (".claude/rules/csharp.md", "A fallback is a rare, deliberate, justified exception"),
    ("CLAUDE.md", "Do not build other gates, coverage maps, checklists or proof artifacts"),
    ("CLAUDE.md", "never by analogy or an upfront grid"),
    (".claude/rules/tests.md", "no component library appears on a framework test's page"),
    (".claude/rules/tests.md", "Watch the behavior in a real browser before writing the test that pins it"),
    ("CLAUDE.md", "code, its Playwright tests failing on the planted defect then passing"),
]
CLAUDE_ADDED = ["## Spec-first", "`spec/BRIEF.md`", "`spec/REWRITE.md`", "`spec/CLAUDE.md`", "TLC green", "stability rules"]
CLAUDE_ORDER = ["its spec lines", "TLC green", "written from their promises and failing", "the code until they pass",
                "failing on its planted defect", "commit"]


def check_claude_md():
    root = SPEC.parent
    problems = [f"{name} still holds {phrase!r}" for name, phrase in CLAUDE_REPLACED
                if " ".join(phrase.split()) in " ".join((root / name).read_text().split())]
    text = (root / "CLAUDE.md").read_text()
    problems += [f"CLAUDE.md lacks {needed!r}" for needed in CLAUDE_ADDED if needed not in text]
    order = " ".join(text.split("A unit runs in this order:", 1)[1].split("\n- ", 1)[0].split()) if "A unit runs in this order:" in text else ""
    places = [order.find(step) for step in CLAUDE_ORDER]
    problems += [f"CLAUDE.md's unit order lacks {step!r}" for step, place in zip(CLAUDE_ORDER, places) if place < 0]
    problems += [f"CLAUDE.md's unit order puts {CLAUDE_ORDER[index + 1]!r} before {CLAUDE_ORDER[index]!r}"
                 for index in range(len(places) - 1) if min(places[index], places[index + 1]) >= 0 and places[index + 1] < places[index]]
    detail = (f"root CLAUDE.md is spec-first: the {len(CLAUDE_REPLACED)} lines replaced, the thesis, principles, stability rules and modules named, "
              "and a unit's order test-first: spec lines, TLC, tests from their promises failing, code, planted defects, commit")
    return result(not problems, "; ".join(problems) or detail, problems=len(problems))


ACTION_SPECS = ["Request", "Run", "Composition", "Ownership", "Lanes", "Realtime", "Tracing", "CallbackProperty"]
ACTION_NAME = re.compile(r"([A-Z][A-Za-z]+)(?=\(|\s*\\/|\s*$)", re.M)


def spec_actions(spec):
    text = tla_file(spec).read_text()
    start = text.find("\nNext ==")
    end = text.find("\nSpec ==", start)
    return list(dict.fromkeys(ACTION_NAME.findall(text[start + len("\nNext =="):end])))


def observed_actions():
    observed = []
    for path in feature_files():
        for feature in json.loads(path.read_text()).get("features", []):
            observed += [(f"{feature['id']} promise for {promise.get('for')}", promise["observes"]) for promise in feature.get("promises", []) if "observes" in promise]
    for name in ["syncfusion.json", "native.json"]:
        path = SPEC / "Reference" / name
        cells = json.loads(path.read_text())["cells"] if path.exists() else {}
        observed += [(f"spec/Reference/{name} {cell}", entry["promise"]["observes"]) for cell, entry in cells.items() if "observes" in entry.get("promise", {})]
    return observed


def check_actions():
    actions = {f"{spec}.tla:{action}" for spec in ACTION_SPECS for action in spec_actions(spec)}
    observed = observed_actions()
    named = {action for _, action in observed}
    problems = [f"{where} observes {action!r}, no action of its spec's Next" for where, action in observed if action not in actions]
    unnamed = sorted(actions - named)
    detail = "; ".join(problems[:4] + ([f"{len(unnamed)} of {len(actions)} actions named by no promise: {', '.join(unnamed[:12])}"] if unnamed else []))
    return result(not problems and not unnamed, detail or "every action of the async lanes' specs is named by a promise, and every promise's action is its spec's",
                  actions=len(actions), named=len(actions & named), unnamed=len(unnamed), promises=len(observed))


APP_AREAS = {"care", "bench", "refusals", "delivery"}
APP_SLICE_KINDS = {"FusionComponents": "fusion", "FusionTemplates": "fusion", "FusionAppLevelObjects": "fusion",
                   "NativeComponents": "nativeSlice", "NativeAppLevelObjects": "nativeSlice"}
APP_REALTIME = {"stream", "hub"}
APP_FRAMEWORKS = {"net10", "net48"}


def app_tests():
    tests = {}
    for path in feature_files():
        for feature in json.loads(path.read_text()).get("features", []):
            rungs = {defect["id"]: defect["rung"] for defect in feature.get("defects", [])}
            seen = Counter()
            for promise in feature.get("promises", []):
                key = (feature["id"], promise["for"], seen[promise["for"]])
                seen[promise["for"]] += 1
                tests[key] = {"module": path.parent.name, "rung": rungs.get(promise["for"]), "page": promise.get("page", ""),
                              "gesture": promise.get("gesture", "")}
    return tests


NAMED_COMPONENT = re.compile(r"\b((?:Native|Fusion)[A-Z]\w*)")
NOT_COMPONENTS = ("FusionTemplate", "NativeComponent")


def app_named_part_problems(where, page, entry, screens):
    listed = " ".join(json.dumps(part) for part in screens.get(entry["screen"], {}).get("parts", []) if part["id"] in entry["parts"])
    held = set(NAMED_COMPONENT.findall(listed))
    named = {name for name in NAMED_COMPONENT.findall(page) if not name.startswith(NOT_COMPONENTS)}
    return [f"{where}'s page names {name}, which no part it lists holds" for name in sorted(named) if not any(name.startswith(part) for part in held)]


def app_reference_tests():
    native = json.loads((SPEC / "Reference" / "native.json").read_text())
    syncfusion = json.loads((SPEC / "Reference" / "syncfusion.json").read_text())
    cells = {("cell", name) for name, entry in native["cells"].items() if "promise" in entry}
    return cells | {("own", name) for name in syncfusion.get("own", {})}


def app_screen_problems(app, screens, hosts):
    problems = []
    for screen in screens.values():
        name = screen["id"]
        problems += [f"screen {name} is in no area of {sorted(APP_AREAS)}"] if screen.get("area") not in APP_AREAS else []
        problems += [f"screen {name} says nothing of what a user does"] if not is_prose(screen.get("user")) else []
        problems += [f"screen {name} is under {screen['under']}, no screen"] if "under" in screen and screen["under"] not in screens else []
        problems += [f"screen {name} runs on {host}, no host" for host in screen.get("hosts", []) if host not in hosts]
        problems += [f"screen {name} is a fresh app's page and names no host it runs under"] if screen.get("area") == "delivery" and not screen.get("hosts") else []
        problems += [f"screen {name}'s {part['id']} is a {part['kind']}, no part kind" for part in screen["parts"] if part["kind"] not in app["partKinds"]]
        problems += [f"screen {name}'s {part['id']} is a plain native part of InputsAndIds, and the core renders no input: a consumer's component does (M7.N2)"
                     for part in screen["parts"] if part["kind"] == "native" and part.get("module") == "InputsAndIds"]
        fusion = [part["id"] for part in screen["parts"] if part["kind"] == "fusion"]
        problems += ([f"care screen {name} loads Syncfusion ({', '.join(fusion)}) on its own page; it belongs on a page under it"]
                     if fusion and screen.get("area") == "care" and "under" not in screen else [])
    for host in hosts.values():
        problems += [f"host {host['id']} says nothing of its options"] if not is_prose(host.get("what")) else []
        problems += [f"host {host['id']} runs on {host.get('framework')!r}"] if host.get("framework") not in APP_FRAMEWORKS else []
    return problems


def app_placement_problems(app, where, entry, screens, hosts, module, rung):
    screen = screens.get(entry["screen"])
    if screen is None:
        return [f"{where} runs on {entry['screen']}, no screen"]
    parts = {part["id"]: part for part in screen["parts"]}
    kinds = {parts[part]["kind"] for part in entry["parts"] if part in parts}
    problems = [f"{where} uses {part}, which {screen['id']} lacks" for part in entry["parts"] if part not in parts]
    problems += [f"{where} runs on {host}, no host" for host in entry["hosts"] if host not in hosts]
    problems += [f"{where} runs on {host}, which {screen['id']} is not on" for host in entry["hosts"] if "hosts" in screen and host not in screen["hosts"]]
    problems += [f"{where} uses the harness verb {verb!r}, which app.json's harness lacks" for verb in entry["harness"] if verb not in app["harness"]]
    problems += [f"{where} uses a stream or hub on net48" for host in entry["hosts"] if kinds & APP_REALTIME and hosts.get(host, {}).get("framework") == "net48"]
    fresh_hosts = fresh_app_hosts(screens.values())
    problems += [f"{where} runs under no host"] if not entry["hosts"] else []
    problems += [f"{where} runs on {host}, a fresh app's server, which serves only a fresh app's page" for host in entry["hosts"]
                 if host in fresh_hosts and screen["area"] != "delivery"]
    problems += [f"{where} uses the harness verb {verb!r} on {screen['id']}, a fresh app's page, which holds no world"
                 for verb in entry["harness"] if screen["area"] == "delivery"]
    if module is None:
        return problems
    problems += [f"{where}, a {module} test, runs on {screen['id']}, a fresh app's page, where only RenderingAndDelivery's tests run"] if screen["area"] == "delivery" and module != "RenderingAndDelivery" else []
    refused = rung == "refusal"
    problems += [f"{where} is a refusal on {screen['id']}, where only refusal routes hold one"] if refused and screen["area"] != "refusals" else []
    problems += [f"{where} is a {rung} on the refusal routes"] if not refused and screen["area"] == "refusals" else []
    slice_kind = APP_SLICE_KINDS.get(module)
    if slice_kind is None:
        loaded = sorted(part["id"] for part in screen["parts"] if part["kind"] == "fusion")
        problems += [f"{where}, a core test, runs on {screen['id']}, whose page loads Syncfusion ({', '.join(loaded)})"] if loaded else []
        problems += [f"{where}, a core test, runs on {host}, whose layout is Syncfusion's" for host in entry["hosts"] if host == "fusionShell"]
    elif not refused:
        held = {part["kind"] for part in screen["parts"]}
        problems += [f"{where}, a {module} test, runs on {screen['id']}, which holds no {slice_kind} part"] if slice_kind not in held else []
    problems += [f"{where}, a behavior of InputsAndIds, lists no component that renders its field: Html.InputField builds on a "
                 "consumer's render helper (M7.N2), a conformance declaration, a Native input or a Fusion one"
                 ] if module == "InputsAndIds" and rung == "behavior" and not kinds & APP_FIELD_RENDERERS else []
    return problems


APP_FIELD_RENDERERS = {"conformance", "nativeSlice", "fusion"}
APP_SERVER_PARTS = {"endpoint", "form", "stream", "hub"}
APP_FIELD_GESTURE = re.compile(r"(?:^|; |, then |\bthen |, and |\band |, )(?:type|tick|untick|select|choose|pick|clear|enter|fill|set the \w+ to|set it to)\b", re.I)
APP_SERVER_READ = re.compile(r"\b(?:GETs?|POSTs?|PUTs?|DELETEs?)\b|\bresponse body|\bthe server writes|\bposts (?:the|its) (?:form|container|intake|page)\b|\bLoad (?:a|the) \w+ whose\b")


def app_door_part_problems(where, known, entry, screens):
    if known.get("rung") != "behavior" or entry["screen"] not in screens:
        return []
    parts = {part["id"]: part["kind"] for part in screens[entry["screen"]]["parts"]}
    kinds = {parts[name] for name in entry["parts"] if name in parts}
    gesture, page = known.get("gesture", ""), known.get("page", "")
    problems = [f"{where}'s gesture fills a field, and it lists no component that renders one: the core renders no input (M7.N2)"
                ] if APP_FIELD_GESTURE.search(gesture) and not kinds & APP_FIELD_RENDERERS else []
    problems += [f"{where} reads what a request or the server sends, and it lists no endpoint, form, stream or hub part"
                 ] if APP_SERVER_READ.search(f"{page} {gesture}") and not kinds & APP_SERVER_PARTS else []
    return problems
SLICE_MODULES = ("NativeComponents", "FusionComponents", "NativeAppLevelObjects", "FusionAppLevelObjects")
RENDER_HELPER_TYPE = re.compile(r"(?:HtmlExtensions|Builder)(?:<|$)")


def slice_renders(feature):
    return feature["status"] == "new" or any(RENDER_HELPER_TYPE.search(name) for name in feature["types"])


def check_slice_tests():
    tests = app_tests()
    app = json.loads((SPEC / "Reference" / "app.json").read_text())
    placed = Counter(entry["feature"] for entry in app["placements"]
                     if tests.get((entry["feature"], entry["for"], entry["index"]), {}).get("rung") == "behavior")
    slices = [feature for path in feature_files() if path.parent.name in SLICE_MODULES
              for feature in json.loads(path.read_text()).get("features", [])]
    rendered = [feature for feature in slices if slice_renders(feature)]
    unrendered = [feature for feature in slices if not slice_renders(feature)]
    problems = [f"{feature['id']} {feature['types'][0].split(':')[-1] if feature['types'] else feature['summary'].split(':')[0]} has no "
                "placed behavior test: its render helper's start and the vendor finding it at its id run in no browser (D17.13)"
                for feature in rendered if not placed[feature["id"]]]
    shown = "; ".join(problems[:6]) + (f"; and {len(problems) - 6} more" if len(problems) > 6 else "")
    detail = (f"{len(rendered)} rendered slices each with a placed behavior test; "
              f"{', '.join(feature['id'] for feature in unrendered)} are the bases slices declare on and render nothing")
    return result(not problems, shown or detail, slices=len(rendered), unrendered=len(unrendered), problems=len(problems))


APP_ACTION_VERBS = {"gate", "push", "drop", "refuse", "reopen", "restart"}


def app_held_problems(where, entry):
    harness = set(entry["harness"])
    held = entry.get("held", [])
    named = {step["verb"] for step in held}
    problems = [f"{where} uses {verb} and holds no step of it" for verb in sorted(harness - named - {"seed"})]
    problems += [f"{where} holds a {verb} step, and its harness lacks {verb}" for verb in sorted(named - harness)]
    for step in held:
        action = step.get("action")
        if action is None:
            problems += [f"{where}'s {step['verb']} step on {step.get('on')} names no action"] if step["verb"] in APP_ACTION_VERBS else []
            continue
        spec, _, name = action.partition(".tla:")
        known = spec in ACTION_SPECS and name in spec_actions(spec)
        problems += [f"{where} holds {action}, no action of {spec}.tla's Next"] if not known else []
    return problems


def app_vendor_problems(screens, used):
    native = json.loads((SPEC / "Reference" / "native.json").read_text())
    components = {declared["name"]: declared for declared in native["types"] if declared["kind"] == "component"}
    declared_by, problems = {}, []
    for screen in screens.values():
        for part in (part for part in screen["parts"] if part["kind"] == "conformance"):
            name = part.get("declares")
            problems += [f"{screen['id']}'s {part['id']} declares {name!r}, no component of native.json"] if name not in components else []
            declared_by.setdefault(name, []).append((screen["id"], part["id"]))
    problems += [f"native.json's {name} is the declaration of no conformance part" for name in components if name not in declared_by]
    axes = {"locate": set(VENDOR_LOCATE), "listening": set(VENDOR_LISTENING), "elementPath": VENDOR_ELEMENT_PATHS}
    held = {axis: set() for axis in axes}
    for name, places in declared_by.items():
        if name in components and any(place in used for place in places):
            for axis, kind in vendor_kinds(components[name].get("vendor") or {}).items():
                held[axis].add(kind)
    problems += [f"no placed test reaches a conformance declaration whose vendor {axis} is {kind} (D17.3)"
                 for axis, kinds in axes.items() for kind in sorted(kinds - held[axis])]
    return problems


def check_app():
    path = SPEC / "Reference" / "app.json"
    if not path.exists():
        return not_started(path)
    app = json.loads(path.read_text())
    screens = {screen["id"]: screen for screen in app["screens"]}
    hosts = {host["id"]: host for host in app["hosts"]}
    problems = app_screen_problems(app, screens, hosts)
    tests, references = app_tests(), app_reference_tests()
    placed, used = Counter(), set()
    for entry in app["placements"]:
        key = (entry["feature"], entry["for"], entry["index"])
        where = f"{entry['feature']} {entry['for']} #{entry['index']}"
        known = tests.get(key)
        problems += [f"{where} is placed, and no promise of features.json is it"] if known is None else []
        placed[key] += 1
        used |= {(entry["screen"], part) for part in entry["parts"]}
        problems += app_placement_problems(app, where, entry, screens, hosts, (known or {}).get("module"), (known or {}).get("rung"))
        problems += app_held_problems(where, entry)
        problems += app_named_part_problems(where, (known or {}).get("page", ""), entry, screens)
        problems += app_door_part_problems(where, known or {}, entry, screens)
    native_cells = json.loads((SPEC / "Reference" / "native.json").read_text())["cells"]
    reached = Counter()
    for entry in app["cells"]:
        key = ("cell", entry["cell"]) if "cell" in entry else ("own", entry["own"])
        reached[key] += 1
        used |= {(entry["screen"], part) for part in entry["parts"]}
        problems += [f"{key[0]} {key[1]} is placed, and the references promise no such cell"] if key not in references else []
        problems += [f"cell {key[1]} runs on {entry['screen']}, where the native reference's cells run on the bench"] if key[0] == "cell" and entry["screen"] != "bench" else []
        problems += app_placement_problems(app, f"{key[0]} {key[1]}", entry, screens, hosts, None, None)
        problems += app_held_problems(f"{key[0]} {key[1]}", entry)
        page = native_cells.get(key[1], {}).get("promise", {}).get("page", "") if key[0] == "cell" else ""
        problems += app_named_part_problems(f"{key[0]} {key[1]}", page, entry, screens)
    problems += [f"{feature} {defect} #{index} is placed {count} times" for (feature, defect, index), count in placed.items() if count > 1]
    problems += [f"{kind} {name} is placed {count} times" for (kind, name), count in reached.items() if count > 1]
    problems += [f"{feature} {defect} #{index} runs on no screen" for (feature, defect, index) in tests if (feature, defect, index) not in placed]
    problems += [f"{kind} {name} runs on no screen" for kind, name in sorted(references - set(reached))]
    problems += [f"{screen['id']}'s {part['id']} is used by no test" for screen in screens.values() for part in screen["parts"] if (screen["id"], part["id"]) not in used]
    problems += app_vendor_problems(screens, used)
    rewrite = sorted(f"{entry['feature']} {entry['for']} #{entry['index']}" for entry in app["placements"] if entry.get("rewrite"))
    problems += [f"{len(rewrite)} tests put the environment on a care screen, which the harness does: {', '.join(rewrite[:6])}"] if rewrite else []
    detail = (f"{len(placed)} feature tests and {len(reached)} cell tests on {len(screens)} screens under {len(hosts)} hosts; "
              "no core test loads Syncfusion, a placed test reaches every D17.3 vendor kind on a conformance declaration, "
              "each Native or Fusion component a test's page names is held by a part it lists, and a test that fills a field or "
              "reads what the server sends lists the part that does it")
    return result(not problems, "; ".join(problems[:6]) or detail, screens=len(screens), hosts=len(hosts), placed=len(placed),
                  cells=len(reached), rewrite=len(rewrite), problems=len(problems))


def check_hedges():
    files = prose_files()
    listed = git("ls-files", "--cached", "--others", "--exclude-standard", "spec").splitlines()
    json_files = sorted(REPO / name for name in listed if name.endswith(".json") and REPO / name != STATE)
    hits = []
    for path in files:
        text = without_code(path.read_text())
        for match in HEDGE.finditer(text):
            line = text.count("\n", 0, match.start()) + 1
            hits.append(f"{path.relative_to(REPO)}:{line} {' '.join(match.group(1).split())}")
    for path in json_files:
        for text in json_prose(json.loads(path.read_text())):
            for match in HEDGE.finditer(without_code(text)):
                hits.append(f"{path.relative_to(REPO)} {' '.join(match.group(1).split())}")
    design = SPEC / "Core" / "DESIGN.md"
    if not design.exists():
        return result(False, f"{len(hits)} hedge words; not started: spec/Core/DESIGN.md absent", hedges=len(hits), files=len(files) + len(json_files))
    return result(not hits, "; ".join(hits[:10]) or "no hedge word in the spec's prose", hedges=len(hits), files=len(files) + len(json_files))


def python_comments(path):
    hits = []
    tokens = tokenize.generate_tokens(io.StringIO(path.read_text()).readline)
    previous = None
    for token in tokens:
        if token.type == tokenize.COMMENT and not (token.start[0] == 1 and token.string.startswith("#!")):
            hits.append(f"{path.relative_to(REPO)}:{token.start[0]}")
        if token.type == tokenize.STRING and previous in (None, tokenize.NEWLINE, tokenize.INDENT, tokenize.DEDENT, tokenize.NL) and token.string.lstrip("rbuRBU").startswith(('"""', "\'\'\'")):
            hits.append(f"{path.relative_to(REPO)}:{token.start[0]} docstring")
        if token.type not in (tokenize.COMMENT, tokenize.NL):
            previous = token.type
    return hits


def check_no_comments():
    hits = []
    for path in sorted(SPEC.glob("**/*.py")):
        hits += python_comments(path)
    for path in sorted(SPEC.glob("**/*.tla")) + sorted(SPEC.glob("**/*.cfg")):
        for number, line in enumerate(path.read_text().splitlines(), 1):
            if "\\*" in line or "(*" in line:
                hits.append(f"{path.relative_to(REPO)}:{number}")
    return result(not hits, "; ".join(hits[:10]) or "no comment in the spec's Python or TLA+", comments=len(hits))


PLANTS = SPEC / "plants"
PLANT_WORKERS = 4
PLANT_EXEMPT = {"plants": "it runs every other check's plants"}
PLANT_PASSED = {"CAUGHT", "KEPT"}
PLANT_TIMEOUT = 1800


def plant_sets():
    return {path.stem: json.loads(path.read_text()) for path in sorted(PLANTS.glob("*.json"))}


def plant_shape_problems(check_id, plants):
    problems = []
    for plant in plants if isinstance(plants, list) else [None]:
        edits = plant.get("edits") if isinstance(plant, dict) else None
        if not isinstance(plant, dict) or not is_prose(plant.get("name")) or not isinstance(edits, list) or not edits:
            problems.append(f"spec/plants/{check_id}.json holds a plant with no name or no edits")
        elif any(not plant_edit_shaped(edit) for edit in edits):
            problems.append(f"spec/plants/{check_id}.json: {plant['name']} has an edit outside the repository or none of file, old and new text, file and new text for a file it adds, or file and remove")
        elif plant.get("expect") is not None and not is_prose(plant.get("expect")):
            problems.append(f"spec/plants/{check_id}.json: {plant['name']} expects no text; an accepted plant expects null")
    return problems


def plant_edit_shaped(edit):
    if not isinstance(edit, dict) or not isinstance(edit.get("file"), str):
        return False
    path = PurePosixPath(edit["file"])
    if not edit["file"] or path.is_absolute() or ".." in path.parts:
        return False
    if edit.get("remove") is True:
        return set(edit) == {"file", "remove"}
    if set(edit) == {"file", "new"}:
        return isinstance(edit["new"], str)
    return set(edit) == {"file", "old", "new"} and all(isinstance(edit[key], str) for key in ("old", "new")) and edit["old"] != ""


def worktree_changes():
    listed = subprocess.run(["git", "-C", str(REPO), "status", "--porcelain=v1", "-z", "--untracked-files=all"],
                            check=True, capture_output=True, text=True).stdout.split("\0")
    changes, index = [], 0
    while index < len(listed) and listed[index]:
        changes.append(listed[index][3:])
        if listed[index][0] in "RC":
            index += 1
            changes.append(listed[index])
        index += 1
    return changes


def snapshot_worktree(target):
    subprocess.run(["git", "-C", str(REPO), "worktree", "add", "--detach", "-q", str(target), "HEAD"], check=True, capture_output=True)
    for path in worktree_changes():
        source, copy = REPO / path, target / path
        if copy.is_symlink() or copy.is_file():
            copy.unlink()
        if source.is_symlink() or source.is_file():
            copy.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(source, copy, follow_symlinks=False)


def planted_outcome(tree, check_id, plant):
    originals = {}
    try:
        for edit in plant["edits"]:
            path = tree / edit["file"]
            if set(edit) == {"file", "new"}:
                if path.exists() or not path.parent.is_dir():
                    return {"pass": None, "detail": f"{edit['file']} is already in the plant tree, or its folder is not"}
                originals[path] = None
                path.write_text(edit["new"])
                continue
            if not path.is_file():
                return {"pass": None, "detail": f"{edit['file']} is not a file in the plant tree"}
            text = path.read_text()
            originals.setdefault(path, text)
            if edit.get("remove"):
                path.unlink()
                continue
            if text.count(edit["old"]) != 1:
                return {"pass": None, "detail": f"{edit['file']} holds {edit['old'][:50]!r} {text.count(edit['old'])} times"}
            path.write_text(text.replace(edit["old"], edit["new"]))
        try:
            completed = subprocess.run([sys.executable, str(tree / "spec" / "tools" / "check.py"), "--check", check_id],
                                       capture_output=True, text=True, cwd=tree, timeout=PLANT_TIMEOUT)
        except subprocess.TimeoutExpired:
            return {"pass": None, "detail": f"{check_id} ran past {PLANT_TIMEOUT} s"}
        if completed.returncode != 0:
            return {"pass": None, "detail": (completed.stderr.strip().splitlines() or ["the check printed nothing"])[-1]}
        outcome = json.loads(completed.stdout)
        return outcome if isinstance(outcome.get("pass"), bool) else {"pass": None, "detail": f"{check_id} returned pass {outcome.get('pass')!r}"}
    finally:
        for path, text in originals.items():
            path.unlink(missing_ok=True) if text is None else path.write_text(text)


def plant_verdict(plant, outcome):
    if outcome["pass"] is None:
        return "SETUP"
    if plant.get("expect") is None:
        return "KEPT" if outcome["pass"] else "BROKE"
    return "CAUGHT" if outcome["pass"] is False and plant["expect"] in outcome["detail"] else "MISSED"


def run_plants(sets):
    jobs = [(check_id, plant) for check_id, plants in sets.items() for plant in plants]
    if not jobs:
        return []
    (REPO / ".worktrees").mkdir(exist_ok=True)
    root = Path(tempfile.mkdtemp(prefix="check-plants-", dir=REPO / ".worktrees"))
    trees = [root / f"tree{number}" for number in range(min(PLANT_WORKERS, len(jobs)))]

    def work(index):
        found = []
        for check_id, plant in jobs[index::len(trees)]:
            outcome = planted_outcome(trees[index], check_id, plant)
            found.append((check_id, plant["name"], plant_verdict(plant, outcome), outcome["detail"]))
        return found

    try:
        for tree in trees:
            snapshot_worktree(tree)
        with ThreadPoolExecutor(len(trees)) as pool:
            return [verdict for found in pool.map(work, range(len(trees))) for verdict in found]
    finally:
        for tree in trees:
            subprocess.run(["git", "-C", str(REPO), "worktree", "remove", "--force", "--force", str(tree)], capture_output=True)
        shutil.rmtree(root, ignore_errors=True)
        subprocess.run(["git", "-C", str(REPO), "worktree", "prune"], capture_output=True)


def check_plants():
    sets = plant_sets()
    known = [check_id for check_id, _, _ in CHECKS if check_id not in PLANT_EXEMPT and check_id not in SKIPPED]
    problems = [f"spec/plants/{name}.json names no check of check.py" for name in sets if name not in known and name not in SKIPPED]
    for check_id, plants in sets.items():
        problems += plant_shape_problems(check_id, plants)
    if problems:
        return result(False, "; ".join(problems[:6]), problems=len(problems))
    verdicts = run_plants({name: plants for name, plants in sets.items() if name in known})
    problems += [f"{check_id} {verdict} {name}: {detail[:140]}" for check_id, name, verdict, detail in verdicts if verdict not in PLANT_PASSED]
    unrefused = [check_id for check_id in known if not any(plant.get("expect") for plant in sets.get(check_id, []))]
    unaccepted = [check_id for check_id in known if not any(plant.get("expect") is None for plant in sets.get(check_id, []))]
    problems += [f"{len(unrefused)} checks have no refused plant: {', '.join(unrefused)}"] if unrefused else []
    problems += [f"{len(unaccepted)} checks have no accepted plant: {', '.join(unaccepted)}"] if unaccepted else []
    caught = sum(1 for verdict in verdicts if verdict[2] == "CAUGHT")
    kept = sum(1 for verdict in verdicts if verdict[2] == "KEPT")
    proven = len(known) - len(set(unrefused) | set(unaccepted))
    detail = f"{caught} refused plants caught and {kept} accepted plants kept green; {proven} of {len(known)} checks proven both ways"
    return result(not problems, "; ".join(problems[:6]) or detail, caught=caught, kept=kept, proven=proven, checks=len(known),
                  failed=len(verdicts) - caught - kept, unrefused=len(unrefused), unaccepted=len(unaccepted))


WORLD_CONTRACT = REPO / "tests" / "Alis.Reactive.Conformance.World"
WORLD_NAMESPACE = "Alis.Reactive.Conformance.World"


def csharp_name(text):
    return re.sub(r"\W", "_", text)


def generated_gates():
    app = json.loads((SPEC / "Reference" / "app.json").read_text())
    held = [step["action"] for group in ("placements", "cells", "harnessTests") for entry in app[group] for step in entry.get("held", []) if step["action"]]
    specs = sorted({action.split(".tla:")[0] for action in held})
    enums = [f"public enum {spec}Gate\n{{\n" + "".join(f"    {action},\n" for action in spec_actions(spec)) + "}\n" for spec in specs]
    return f"namespace {WORLD_NAMESPACE};\n\n" + "\n".join(enums)


def held_gates(entry):
    gates = [step["action"].replace(".tla:", "Gate.") for step in entry.get("held", []) if step["verb"] == "gate" and step["action"]]
    return list(dict.fromkeys(gates))


def promise_constants():
    app = json.loads((SPEC / "Reference" / "app.json").read_text())
    groups = {}
    for placement in app["placements"]:
        named = (csharp_name(f"{placement['for']}_{placement['index']}"), f"{placement['feature']} {placement['for']} {placement['index']}", held_gates(placement))
        groups.setdefault(placement["feature"], []).append(named)
    features = sorted(groups, key=lambda feature: [int(number) for number in re.findall(r"\d+", feature)])
    ordered = {csharp_name(feature): groups[feature] for feature in features}
    ordered["Cells"] = [(csharp_name(cell["cell"]), f"cell {cell['cell']}", held_gates(cell)) for cell in app["cells"] if "cell" in cell]
    ordered["OwnCells"] = [(csharp_name(cell["own"]), f"own {cell['own']}", held_gates(cell)) for cell in app["cells"] if "own" in cell]
    ordered["Harness"] = [(f"{test['fixture']}_{test['test']}", f"harness {test['fixture']}.{test['test']}", held_gates(test)) for test in app["harnessTests"]]
    return ordered


def generated_promises():
    groups = promise_constants()
    twice = [f"{group}.{name}" for group, constants in groups.items() for name, count in Counter(name for name, _, _ in constants).items() if count > 1]
    if twice:
        raise SystemExit("check.py --generate: two tests of the spec take one constant: " + ", ".join(twice))
    classes = [f"    public static class {group}\n    {{\n"
               + "".join("".join(f"        [Holds({gate})]\n" for gate in gates) + f"        public const string {name} = \"{value}\";\n" for name, value, gates in constants)
               + "    }\n"
               for group, constants in groups.items()]
    return f"namespace {WORLD_NAMESPACE};\n\npublic static class Promises\n{{\n" + "\n".join(classes) + "}\n"


def check_bindings():
    try:
        written = {"Gates.g.cs": generated_gates(), "Promises.g.cs": generated_promises()}
    except SystemExit as stopped:
        return result(False, str(stopped), problems=1)
    problems = []
    for name, text in written.items():
        path = WORLD_CONTRACT / name
        where = path.relative_to(REPO)
        if not path.is_file():
            problems.append(f"{where} is absent; check.py --generate writes it")
            continue
        held = path.read_text()
        if held != text:
            line = next(number for number, (ours, theirs) in enumerate(itertools.zip_longest(held.splitlines(), text.splitlines()), 1) if ours != theirs)
            problems.append(f"{where} differs from what the spec writes at line {line}; a generated file is never edited by hand, and check.py --generate writes it")
    constants = sum(len(group) for group in promise_constants().values())
    gates, marks = written["Gates.g.cs"].count(",\n"), written["Promises.g.cs"].count("[Holds(")
    detail = f"Gates.g.cs holds {gates} gates and Promises.g.cs {constants} constants, {marks} of a held gate marked on them, each as the spec writes it"
    return result(not problems, "; ".join(problems) or detail, gates=gates, constants=constants, marks=marks, problems=len(problems))


def generate():
    written = {"Gates.g.cs": generated_gates(), "Promises.g.cs": generated_promises()}
    for name, text in written.items():
        (WORLD_CONTRACT / name).write_text(text)
    print("generated " + " and ".join(str((WORLD_CONTRACT / name).relative_to(REPO)) for name in written))
    return 0


CHECKS = [
    ("survival", "brief, notes, file map, check, compact hook and pushed branch", check_survival),
    ("main_surface", "main's PublicAPI text per package and target framework", check_main_surface),
    ("inventory", "doors, kinds, case labels, analyzer ids and tests as TSV", check_inventory),
    ("core_questions", "Q1 to Q18 answered", check_questions(QUESTIONS[:18])),
    ("core_tlc", "the TLC specs of Q4, Q9 with Q10, Q11, Q12, Q17 and the date wire form green", None),
    ("questions", "every core design question answered", check_questions(QUESTIONS)),
    ("evidence", "every G, B and P evidence item placed in a section's Evidence", check_evidence),
    ("tlc", "every TLC spec meets its expectation", None),
    ("probes", "every server fact a spec assumes is pinned by a probe, and every browser fact names the proof the rewrite ships", check_probes),
    ("history", "every regression SHA tied to a spec line", check_history),
    ("kinds", "grammar, schema and spec node sets equal; one handler per kind", check_kinds),
    ("gate1", "owner gate 1: the core design", check_gate(1)),
    ("doors_mapped", "every public door mapped to one feature", check_doors_mapped),
    ("verdicts", "every main-only door has a verdict; a refusal names its carriers", check_verdicts),
    ("main_delta", "every shipped file main changed after the spec's main SHA cited at main's tip, every public member it added with a verdict", check_main_delta),
    ("features_core", "every feature mapped to the core", check_features_core),
    ("features_lost", "every module and feature on either line carried: features lost 0", check_features_lost),
    ("module_cites", "every module.md cite resolves at the SHA it names", check_module_cites),
    ("fluent", "every module's sentences consistent", check_fluent),
    ("gate2", "owner gate 2: the fluent API", check_gate(2)),
    ("matrix", "the conformance matrix, every cell run by FusionMultiSelect's or ConformanceComponent's declaration", check_matrix),
    ("native_behaviors", "every behavior native.json states names tests of the spec", check_native_behaviors),
    ("defects", "every defect on its highest rung, every core defect in a feature; no test for a type or analyzer defect", check_defects),
    ("carriers", "every D line and module meaning carried by a resolving defect, invariant, probe, check, cell or delivery test", check_carriers),
    ("promises", "every refusal and behavior defect, every cell the native reference runs and each cell MultiSelect tests as its own, has a promise", check_promises),
    ("rewrite_units", "every feature in exactly one rewrite unit", check_rewrite),
    ("paths", "every path at HEAD and main's tip kept by REWRITE.md's section 2 or retired by its section 3, so the cut drops nothing by omission", check_paths),
    ("unit_proofs", "every rewrite unit proves something: a browser test lands in it, a module test reads a node its core features write, an analyzer case of its features, or a harness test", check_unit_proofs),
    ("names", "every module named alike in the module table, REWRITE.md's tree, SliceTree and TLA+ table, and spec/", check_names),
    ("rewrite_cites", "every cite REWRITE.md makes names its SHA and resolves there", check_rewrite_cites),
    ("packages", "every package REWRITE.md packs is main's or a decided door's, ships bundles only, and every dependency and kept reference is pinned at its version", check_packages),
    ("test_rules", "every rule and smell of tests.md held by a test rule or a named check", check_test_rules),
    ("claude_md", "root CLAUDE.md and rules carry the spec-first order", check_claude_md),
    ("hedges", "zero hedge words", check_hedges),
    ("no_comments", "no comment in the spec's Python or TLA+", check_no_comments),
    ("actions", "every action of the async lanes' specs named by one promise, the test app's endpoint holding it", check_actions),
    ("app", "every test placed on the care home app, no core test loading Syncfusion, no environment on a care screen", check_app),
    ("slice_tests", "every rendered slice of Native, Fusion and their app-level objects with a placed behavior test of its own", check_slice_tests),
    ("bindings", "the generated gates and promise constants are what the spec writes, never a hand's", check_bindings),
    ("plants", "every other check fails on each refused plant with its text and stays green on each accepted plant, run in throwaway worktrees", check_plants),
]

STEPS = [
    (1, "A", "Branches and survival", ["survival"], "Install the compact hook, commit by path, push, open .worktrees/main-read at 623dd4a27."),
    (2, "B", "Main's surface as text", ["main_surface"], "Wire PublicApiAnalyzers into main-read, run RS0016 one target framework at a time, copy to spec/inventory/main."),
    (3, "B", "Inventory", ["inventory"], "Write spec/tools/inventory.py and the TSVs."),
    (4, "C", "Q1 to Q18, TLA+ specs, probes, history", ["core_questions", "core_tlc", "probes", "history"], "Answer Q1 to Q18 in spec/Core/DESIGN.md with their TLA+ specs and probes; write spec/history.tsv."),
    (5, "C", "Q19, the schema, Q20, tracing, Q21, realtime, the validation spec, gate 1", ["questions", "evidence", "tlc", "kinds", "gate1"], "Write Schema.tla, schema.md and the validation specs; present gate 1."),
    (6, "D", "Feature rows", ["doors_mapped", "verdicts", "features_core", "features_lost", "module_cites"], "Write spec/<Module>/features.json and module.md for each module."),
    (7, "D", "The fluent API, gate 2", ["fluent", "gate2"], "Write spec/Fluent/sentences.json; present gate 2."),
    (8, "D", "The reference components", ["matrix"], "Write spec/Reference/matrix.json, syncfusion.json, native.json."),
    (9, "D", "The promises", ["defects", "promises"], "Give every refusal and behavior defect and every matrix cell its promise."),
    (10, "E", "check.py complete", ["hedges", "no_comments", "actions", "app", "carriers", "main_delta", "slice_tests", "bindings", "plants"], "Finish every check and make each pass."),
    (11, "E", "REWRITE.md", ["rewrite_units", "unit_proofs", "packages", "paths", "names", "rewrite_cites"], "Write spec/REWRITE.md in SliceTree order."),
    (12, "E", "Root CLAUDE.md on spec-first", ["claude_md"], "Add the thesis, principles and order; replace the five lines word for word."),
]


def run_checks(brief, previous):
    outcomes = {}
    for check_id, _, function in CHECKS:
        if check_id in SKIPPED:
            continue
        if check_id == "core_tlc":
            outcomes[check_id] = check_tlc(brief, previous, 4)
        elif check_id == "tlc":
            outcomes[check_id] = check_tlc(brief, {"checks": {"tlc": {"runs": {**previous.get("checks", {}).get("tlc", {}).get("runs", {}), **outcomes["core_tlc"]["runs"]}}}}, 5)
        else:
            outcomes[check_id] = function()
    return outcomes


def current_step(outcomes):
    for number, phase, name, gates, action in STEPS:
        if not all(outcomes[gate]["pass"] for gate in gates):
            return {"number": number, "phase": phase, "name": name, "next": action, "failing": [gate for gate in gates if not outcomes[gate]["pass"]]}
    return {"number": 13, "phase": "E", "name": "End of session", "next": "Write the report and copy the handoff with pbcopy.", "failing": []}


def write_state(outcomes, step):
    state = {
        "read_at_head": git("rev-parse", "HEAD"),
        "step": step,
        "passed": sum(1 for outcome in outcomes.values() if outcome["pass"]),
        "total": len(outcomes),
        "checks": outcomes,
    }
    STATE.write_text(json.dumps(state, indent=2, sort_keys=True) + "\n")


def counts_text(counts):
    return ", ".join(f"{name} {value}" for name, value in counts.items())


def report(outcomes, step, brief):
    passed = sum(1 for outcome in outcomes.values() if outcome["pass"])
    lines = [f"spec check at {git('rev-parse', '--short', 'HEAD')}: {passed} of {len(outcomes)} checks pass"]
    lines += [f"phase {step['phase']}, step {step['number']}: {step['name']}", f"next: {step['next']}"] if step else []
    for check_id, title, _ in CHECKS:
        if check_id not in outcomes:
            continue
        outcome = outcomes[check_id]
        mark = "PASS" if outcome["pass"] else "FAIL"
        if brief:
            lines.append(f"{mark} {check_id}")
        else:
            counts = counts_text(outcome["counts"])
            lines.append(f"{mark} {check_id}: {title}" + (f" [{counts}]" if counts else ""))
            lines.append(f"     {outcome['detail']}")
    if brief:
        failing = [check_id for check_id, _, _ in CHECKS if not outcomes[check_id]["pass"]]
        lines = lines[:3] + [f"failing: {', '.join(failing) or 'none'}"]
        lines += [f"  {check_id}: {outcomes[check_id]['detail']}" for check_id in step["failing"]]
        lines += ["standing decisions, spec/CLAUDE.md:"] + standing_decisions()
        lines += ["the vision, root CLAUDE.md:"] + root_vision()
    print("\n".join(lines))


def standing_decisions():
    text = (SPEC / "CLAUDE.md").read_text()
    section = text.split("## Standing decisions", 1)[1].split("\n## ", 1)[0]
    return [line for line in section.strip().splitlines() if line.strip()]


def root_vision():
    text = (REPO / "CLAUDE.md").read_text()
    section = text.split("## Vision", 1)[1].split("\n## ", 1)[0]
    return [line for line in section.strip().splitlines() if line.strip()]


def orient():
    head = git("rev-parse", "--short", "HEAD")
    remote = git("rev-parse", "--short", "-q", "--verify", f"refs/remotes/origin/{SPEC_BRANCH}")
    changed = [line.strip() for line in git("status", "--short", "--", ".", ":!examples/resident-intake").splitlines()]
    state = json.loads(STATE.read_text()) if STATE.exists() else {}
    read_at = state.get("read_at_head", "")[:9]
    behind = git("rev-list", "--count", f"{read_at}..HEAD") if read_at else "every"
    failing = [name for name, outcome in state.get("checks", {}).items() if not outcome.get("pass")]
    dated = [(number, found.group(1)) for number, found in
             ((number, OWNER_LINE.match(line)) for number, line in enumerate((SPEC / "CLAUDE.md").read_text().splitlines(), 1)) if found]
    latest = max((date for _, date in dated), default="")
    step = state.get("step", {})
    print("\n".join([
        f"orient: {git('rev-parse', '--abbrev-ref', 'HEAD')} at {head}; origin/{SPEC_BRANCH} at {remote or 'none'}"
        + ("" if remote == head else "; they differ, so push or read why before acting"),
        "uncommitted: " + (", ".join(changed[:12]) or "none"),
        f"last committed STATE: read at {read_at or 'no commit'}, {behind} commits behind HEAD, {state.get('passed', 0)} of "
        f"{state.get('total', 0)} pass, failing {', '.join(failing) or 'none'}, step {step.get('number', '?')} {step.get('name', '')}",
        f"owner decisions dated {latest}: spec/CLAUDE.md lines {', '.join(str(number) for number, date in dated if date == latest)}",
        f"handoff, {HANDOFF}:",
        HANDOFF.read_text() if HANDOFF.is_file() else "MISSING: write the handoff before any other work",
        "Before acting: read the handoff whole, the owner lines above and every file the next queue item touches; "
        "a claim stands on what is run now, never on what is recalled.",
    ]))


def run_one(check_id):
    previous = json.loads(STATE.read_text()) if STATE.exists() else {}
    if check_id in ("core_tlc", "tlc"):
        return check_tlc(False, previous, 4 if check_id == "core_tlc" else 5)
    return next(function for name, _, function in CHECKS if name == check_id)()


def report_plants(names):
    sets = plant_sets()
    known = {check_id for check_id, _, _ in CHECKS if check_id not in PLANT_EXEMPT}
    refused = [f"{name} is no check that has plants" for name in names if name not in known]
    refused += [f"spec/plants/{name}.json is absent" for name in names if name in known and name not in sets]
    refused += [problem for name in names or sets for problem in plant_shape_problems(name, sets.get(name, []))]
    if refused:
        print("\n".join("REFUSED " + problem for problem in refused))
        return 2
    verdicts = run_plants({name: sets.get(name, []) for name in names or sets})
    for check_id, name, verdict, detail in verdicts:
        print(f"{verdict} {check_id}: {name}: {detail[:200]}")
    passed = sum(1 for verdict in verdicts if verdict[2] in PLANT_PASSED)
    print(f"plants: {passed} of {len(verdicts)} as expected")
    return 0 if passed == len(verdicts) else 1


def run_ci():
    global SKIPPED
    unset = [name for name in CI_TOOLS if not os.environ.get(name)]
    if unset:
        print(f"check.py --ci reads java from JAVA_HOME and the jar from TLA2TOOLS_JAR; {' and '.join(unset)} is not set")
        return 1
    SKIPPED = CI_SKIPPED
    cache = Path(os.environ[CI_CACHE]) if os.environ.get(CI_CACHE) else None
    previous = json.loads(cache.read_text()) if cache and cache.is_file() else {}
    outcomes = run_checks(False, previous)
    if cache:
        cache.write_text(json.dumps({"checks": {"tlc": {"runs": outcomes["tlc"]["runs"]}}}, indent=2, sort_keys=True) + "\n")
    report(outcomes, None, False)
    return 0 if all(outcome["pass"] for outcome in outcomes.values()) else 1


def main():
    if "--orient" in sys.argv[1:]:
        orient()
        return 0
    if "--ci" in sys.argv[1:]:
        return run_ci()
    if "--generate" in sys.argv[1:]:
        return generate()
    if sys.argv[1:2] == ["--check"]:
        print(json.dumps(run_one(sys.argv[2])))
        return 0
    if sys.argv[1:2] == ["--plants"]:
        return report_plants(sys.argv[2:])
    brief = "--brief" in sys.argv[1:]
    previous = json.loads(STATE.read_text()) if STATE.exists() else {}
    outcomes = run_checks(brief, previous)
    step = current_step(outcomes)
    if not brief:
        write_state(outcomes, step)
    report(outcomes, step, brief)
    return 0 if all(outcome["pass"] for outcome in outcomes.values()) else 1


if __name__ == "__main__":
    sys.exit(main())
