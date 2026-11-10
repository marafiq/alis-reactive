#!/usr/bin/env python3
import hashlib
import io
import itertools
import json
import os
import platform
import tokenize
import re
import subprocess
import sys
import tempfile
from pathlib import Path

SPEC = Path(__file__).resolve().parent.parent
REPO = SPEC.parent
STATE = SPEC / "STATE.json"
MAIN_SHA = "623dd4a272f8134fb692985fd04ae7151954c9f0"
BASE_SHA = "65fdf118d1498a8285db84a43c4da81dfaa0ea3e"
SPEC_BRANCH = "feature/spec-first"
JAVA = "/opt/homebrew/opt/openjdk/bin/java"
TLA_JAR = Path.home() / ".local/share/tlaplus/tla2tools.jar"
TLC_GREEN = "Model checking completed. No error has been found."
LOCAL_SETTINGS = Path("/Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.claude/settings.local.json")
HOOK_COMMAND = (
    "cd /Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/spec-first; "
    "python3 spec/tools/check.py --brief; tail -20 spec/NOTES.md; "
    'echo "Re-read spec/BRIEF.md for the current step before acting."'
)

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
}
PLUGIN_DATA_DISPATCH = "pluginNamed(locator.name).contribute"
HEDGES = [
    "rare", "rarely", "usually", "may", "might", "maybe", "perhaps", "possibly", "probably", "likely",
    "typically", "generally", "often", "sometimes", "ideally", "should", "etc", "tbd", "todo",
    "when needed", "where possible", "if possible", "as needed", "as appropriate", "for now",
]
HEDGE = re.compile(r"\b(" + "|".join(re.escape(word).replace(r"\ ", r"\s+") for word in HEDGES) + r")\b", re.IGNORECASE)
PROSE_KEYS = {
    "meaning", "reason", "decision", "improvement", "page", "gesture", "outcome", "opposite", "defect", "oracle",
    "summary", "why", "statement", "description",
}


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
    files = [SPEC / "CLAUDE.md", SPEC / "Core" / "DESIGN.md", SPEC / "Core" / "schema.md", SPEC / "REWRITE.md"]
    files += sorted(SPEC.glob("*/module.md"))
    return [path for path in files if path.exists()]


def without_code(text):
    text = re.sub(r"```.*?```", "", text, flags=re.DOTALL)
    return re.sub(r"`[^`\n]*`", "", text)


def json_prose(node, key=""):
    if isinstance(node, dict):
        for name, value in node.items():
            yield from json_prose(value, name)
    elif isinstance(node, list):
        for value in node:
            yield from json_prose(value, key)
    elif isinstance(node, str) and key in PROSE_KEYS:
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


def check_inventory():
    paths = [SPEC / "inventory" / name for name in INVENTORY_FILES]
    if any(not path.exists() for path in paths):
        return not_started(*paths)
    problems = []
    doors = table("doors.tsv")
    for package, tfms in REDESIGN_PACKAGES.items():
        for tfm in tfms:
            source = sum(len(door_lines(git("show", f"{BASE_SHA}:{package}/PublicAPI/{tfm}/{name}"))) for name in ["PublicAPI.Unshipped.txt", "PublicAPI.Shipped.txt"])
            rows = sum(1 for row in doors if row["package"] == package and row["tfm"] == tfm and row["redesign_line"])
            if source != rows:
                problems.append(f"redesign {package} {tfm}: {source} doors, {rows} rows")
    for package, tfms in MAIN_PACKAGES.items():
        for tfm in tfms:
            source = len(door_lines((SPEC / "inventory" / "main" / package / tfm / "PublicAPI.Unshipped.txt").read_text()))
            rows = sum(1 for row in doors if row["main_package"] == package and row["tfm"] == tfm and row["main_line"])
            if source != rows:
                problems.append(f"main {package} {tfm}: {source} doors, {rows} rows")
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
    grammar = {row["kind"] for row in kinds if row["source"] == "grammar"}
    schema = {row["kind"] for row in kinds if row["source"] == "plan.schema.json"}
    if grammar != schema:
        problems.append(f"grammar and schema kinds differ: {sorted(grammar ^ schema)}")
    status = {}
    for row in doors:
        status[row["status"]] = status.get(row["status"], 0) + 1
    return result(not problems, "; ".join(problems[:8]) or "every door, test and kind reconciles with its source",
                  doors=len(doors), both=status.get("both", 0), moved=status.get("moved", 0), main_only=status.get("main-only", 0), redesign_only=status.get("redesign-only", 0),
                  tests=len(tests), kinds=len(schema), analyzers=len(table("analyzers.tsv")), cases=len(table("cases.tsv")))


RUNGS = {"type", "analyzer", "refusal", "behavior"}
CITE_EXTENSIONS = "cs|ts|tla|cfg|json|md|txt|mjs|csproj|props|targets|cshtml|js|sh|yml"
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


def tla_invariants(spec):
    tla = SPEC / "Core" / f"{spec}.tla"
    cfg = SPEC / "Core" / f"{spec}.cfg"
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
            [JAVA, "-cp", str(TLA_JAR), "tlc2.TLC", "-deadlock", "-workers", "auto", "-metadir", meta, "-config", cfg.name, tla.name],
            cwd=tla.parent, capture_output=True, text=True,
        )
    lines = (completed.stdout + completed.stderr).splitlines()
    green = any(TLC_GREEN in line for line in lines)
    violated = sorted({match.group(1) for line in lines for match in [re.search(r"Invariant (\w+) is violated", line)] if match})
    states = next((line.strip() for line in reversed(lines) if "distinct states found" in line), "")
    errors = [line.strip() for line in lines if line.startswith("Error") or "error" in line.lower() and "No error" not in line][:2]
    line = TLC_GREEN if green else ("violated: " + ", ".join(violated) if violated else "; ".join(errors) or (lines[-1] if lines else "no output"))
    return {"green": green, "violated": violated, "line": line, "states": states}


def tlc_configs(name):
    folder = SPEC / "Core"
    main = folder / f"{name}.cfg"
    violating = {match.group(1): path for path in sorted(folder.glob(f"{name}.violates.*.cfg")) for match in [re.match(rf"{name}\.violates\.(\w+)\.cfg$", path.name)] if match}
    return main, violating


def check_tlc(brief, previous, step):
    cached = previous.get("checks", {}).get("tlc", {}).get("runs", {})
    runs = {}
    problems = []
    specs = {name: expectation for name, expectation in TLC_SPECS.items() if expectation["step"] <= step}
    met = 0
    for name, expectation in specs.items():
        tla = SPEC / "Core" / f"{name}.tla"
        main, violating = tlc_configs(name)
        if not tla.exists():
            problems.append(f"{name} not started")
            continue
        planned = ([main] if expectation["expect"] == "green" else []) + list(violating.values())
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
                completed = subprocess.run(["node", str(script)], capture_output=True, text=True, cwd=REPO)
                outputs[script] = completed.stdout.splitlines() if completed.returncode == 0 else ["exit " + str(completed.returncode) + ": " + completed.stderr[-300:]]
            if fact["expect"] not in outputs[script]:
                problems.append(f"{fact['id']}: {fact['probe']} printed no line \"{fact['expect']}\"")
            continue
        if fact["host"] == "browser":
            script = SPEC / "probes" / fact["probe"]
            if script not in outputs:
                command, environment = browser_probe(script)
                completed = subprocess.run(command, capture_output=True, text=True, cwd=REPO, env=environment)
                outputs[script] = completed.stdout.splitlines() if completed.returncode == 0 else ["exit " + str(completed.returncode) + ": " + completed.stderr[-300:]]
            if fact["expect"] not in outputs[script]:
                problems.append(f"{fact['id']}: {fact['probe']} printed no line \"{fact['expect']}\"")
            problems += browser_proof_problems(fact, feature_ids)
            continue
        if fact["host"] == "net48":
            source, _, number = fact["source"].rpartition(":")
            lines = (REPO / source).read_text().splitlines() if (REPO / source).is_file() else []
            if not source.startswith("spec/probes/") or int(number) > len(lines) or fact["expect"] not in lines[int(number) - 1]:
                problems.append(f"{fact['id']}: \"{fact['expect']}\" is not at {fact['source']}")
            continue
        probe = SPEC / "probes" / fact["probe"]
        if probe not in outputs:
            completed = subprocess.run(["dotnet", "run", "--project", str(probe)], capture_output=True, text=True, cwd=probe)
            outputs[probe] = completed.stdout.splitlines() if completed.returncode == 0 else ["exit " + str(completed.returncode) + ": " + completed.stderr[-300:]]
        if fact["expect"] not in outputs[probe]:
            problems.append(f"{fact['id']}: {fact['probe']} printed no line \"{fact['expect']}\"")
    return result(not problems, "; ".join(problems[:6]) or "every fact printed by its probe or found at its source line, every browser fact with the proof the rewrite ships",
                  facts=len(facts), net10=sum(1 for fact in facts if fact["host"] == "net10"), net48=sum(1 for fact in facts if fact["host"] == "net48"), node=sum(1 for fact in facts if fact["host"] == "node"), browser=sum(1 for fact in facts if fact["host"] == "browser"))


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
    environment = {**os.environ, "PLAYWRIGHT_PACKAGE": str(bundled / "package"), "NODE_PATH": str(checkout_root() / "node_modules")}
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
    text = (SPEC / "Core" / "Schema.tla").read_text()
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
    detail = "; ".join(problems[:6]) or (
        f"grammar, plan.schema.json and schema.md agree on today's {len(grammar)}; proposed {len(proposed)} equals Schema.tla's"
        f" node set, one handler each; diff +{','.join(added)} -{','.join(removed) or 'none'};"
        f" {len(several_today)} kinds have two handlers today")
    return result(not problems, detail, today=len(grammar), proposed=len(proposed), added=len(added), removed=len(removed),
                  several_handlers_today=len(several_today))


def gate_answered(number):
    notes = (SPEC / "NOTES.md").read_text() if (SPEC / "NOTES.md").exists() else ""
    return re.search(rf"^- \d{{4}}-\d{{2}}-\d{{2}} owner, gate {number}: yes\b", notes, re.MULTILINE) is not None


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


def module_names():
    text = (SPEC / "CLAUDE.md").read_text()
    section = text[text.index("## Modules"):]
    names = {}
    for number, name in re.findall(r"^\| (\d+) \| ([^|]+?) \|", section, re.MULTILINE):
        names["".join(word[:1].upper() + word[1:] for word in re.split(r"[\s-]+", name) if word)] = int(number)
    return names


def internal_modules():
    text = (SPEC / "CLAUDE.md").read_text()
    section = text[text.index("## Modules"):]
    return {int(number) for number, seen in re.findall(r"^\| (\d+) \| [^|]+? \| ([^|]+?) \|", section, re.MULTILINE) if seen == "internal"}


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


def feature_result(problems, counts, complete, done):
    if problems:
        return result(False, "; ".join(problems[:8]), **counts)
    if not complete:
        return result(False, f"every feature row valid; {counts['unmapped']} of {counts['rows']} spec doors in no feature", **counts)
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
        return result(False, f"every feature row valid; {counts['unmapped']} of {counts['rows']} spec doors in no feature; core doors no module names: {', '.join(homeless)}", **counts)
    return feature_result(problems, counts, counts["unmapped"] == 0, "every spec door in exactly one feature, every core door named by a module")


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
            if door not in main_only:
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
    return feature_result(problems, counts, counts["unmapped"] == 0, "every main-only door has a verdict, and every refusal names its carriers")


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
        if line not in SENTENCE_LINES or not is_prose(sentence.get("text")):
            problems.append(f"{feature['id']}: sentence {sentence.get('text')!r} has no line of main, redesign or proposed")
        elif line == "proposed":
            named = sentence.get("doors")
            if not isinstance(named, list) or not named or any(door not in doors for door in named):
                problems.append(f"{feature['id']}: proposed sentence {sentence['text']!r} names a door no core or module door")
        elif not cites_resolve([sentence.get("cite")], SENTENCE_LINES[line]):
            problems.append(f"{feature['id']}: {line} sentence {sentence['text']!r} cites no line at {line}'s SHA")
        elif without_space(sentence["text"]) not in without_space(cited_text(SENTENCE_LINES[line], sentence["cite"])):
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
    return feature_result(model["problems"] + core_problems(model, doors), counts, counts["unmapped"] == 0, "every feature rests on answered questions, schema nodes and checked invariants")


def check_features_lost():
    if not feature_files():
        return result(False, "not started: no spec/<Module>/features.json")
    model = feature_model()
    lost = []
    for feature in model["features"]:
        for verdict in feature.get("verdicts", []):
            carried = verdict.get("carriers") if verdict.get("verdict") == "refused" else verdict.get("to") if verdict.get("verdict") == "renamed" else [None]
            if not carried:
                lost.append(f"{feature['id']}: {verdict.get('door')}")
    unmapped_types = sorted({row["package"] + ":" + row["declaring_type"] for row, owners in zip(model["rows"], model["owners"]) if not owners})
    counts = {"lost": len(lost) + len(unmapped_types), "uncarried_doors": len(lost), "types_in_no_feature": len(unmapped_types)}
    detail = "; ".join(lost[:4] + [f"{len(unmapped_types)} declaring types in no feature"]) if counts["lost"] else "features lost 0"
    return result(counts["lost"] == 0 and not model["problems"], detail, **counts)


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
        return result(False, f"every feature row valid; {counts['unmapped']} of {counts['rows']} spec doors in no feature; core defects no feature places: {', '.join(unplaced)}", **counts)
    return feature_result(problems, counts, counts["unmapped"] == 0, "every defect on its rung, every core defect in a feature; no promise for a type or analyzer defect")


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
    return feature_result(model["problems"] + promise_problems(model, tests), counts, counts["unmapped"] == 0, "every refusal and behavior defect has a promise")


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
    return result(not first, "; ".join(first) or "one concept per name, every Q7 cell compiles or is refused as named, one shape per name, callbacks named as the locked table names them, every string parameter of a closed kind", **counts)


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
    judged, gaps, counts = fluent_cell_judgement(cells, refused)
    forms, passed, total = fluent_refused_forms()
    problems += judged + forms + gaps
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
    gaps = []
    counts = {"F2_cells": len(cells["results"]), "F2_compiling": 0, "F2_refused": 0, "F2_gaps": 0, "F2_failing": 0}
    for cell in cells["results"]:
        label = f"F2 {cell['position']} x {cell['source']}"
        row = rows.pop((cell["position"], cell["source"]), None)
        errors = sorted(cell["errors"])
        if cell["gap"]:
            counts["F2_gaps"] += 1
            problems += [f"{label}: refused for {', '.join(row['errors'])} but has no door"] if row else []
            gaps.append(f"{label}: no door ({cell['gap']})")
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
    return problems, gaps, counts


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


def check_matrix():
    paths = [SPEC / "Reference" / name for name in ["matrix.json", "syncfusion.json", "native.json"]]
    if any(not path.exists() for path in paths):
        return not_started(*paths)
    return not_built("the conformance matrix")


def check_rewrite():
    path = SPEC / "REWRITE.md"
    if not path.exists():
        return not_started(path)
    return not_built("the rewrite units")


def check_claude_md():
    return result(False, "not started: step 12 replaces the five lines and adds the spec-first order")


def check_hedges():
    files = prose_files()
    listed = git("ls-files", "--cached", "--others", "--exclude-standard", "spec").splitlines()
    json_files = sorted(REPO / name for name in listed if name.endswith(".json") and REPO / name != STATE)
    hits = []
    for path in files:
        for number, line in enumerate(without_code(path.read_text()).splitlines(), 1):
            for match in HEDGE.finditer(line):
                hits.append(f"{path.relative_to(REPO)}:{number} {match.group(1)}")
    for path in json_files:
        for text in json_prose(json.loads(path.read_text())):
            for match in HEDGE.finditer(without_code(text)):
                hits.append(f"{path.relative_to(REPO)} {match.group(1)}")
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
    ("features_core", "every feature mapped to the core", check_features_core),
    ("features_lost", "every module and feature on either line carried: features lost 0", check_features_lost),
    ("module_cites", "every module.md cite resolves at the SHA it names", check_module_cites),
    ("fluent", "every module's sentences consistent", check_fluent),
    ("gate2", "owner gate 2: the fluent API", check_gate(2)),
    ("matrix", "every conformance cell promised for both reference components", check_matrix),
    ("defects", "every defect on its highest rung, every core defect in a feature; no test for a type or analyzer defect", check_defects),
    ("promises", "every refusal and behavior defect has a promise", check_promises),
    ("rewrite_units", "every feature in exactly one rewrite unit", check_rewrite),
    ("claude_md", "root CLAUDE.md and rules carry the spec-first order", check_claude_md),
    ("hedges", "zero hedge words", check_hedges),
    ("no_comments", "no comment in the spec's Python or TLA+", check_no_comments),
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
    (10, "E", "check.py complete", ["hedges", "no_comments"], "Finish every check and make each pass."),
    (11, "E", "REWRITE.md", ["rewrite_units"], "Write spec/REWRITE.md in SliceTree order."),
    (12, "E", "Root CLAUDE.md on spec-first", ["claude_md"], "Add the thesis, principles and order; replace the five lines word for word."),
]


def run_checks(brief, previous):
    outcomes = {}
    for check_id, _, function in CHECKS:
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
    lines = [f"spec check at {git('rev-parse', '--short', 'HEAD')}: {passed} of {len(outcomes)} checks pass",
             f"phase {step['phase']}, step {step['number']}: {step['name']}",
             f"next: {step['next']}"]
    for check_id, title, _ in CHECKS:
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


def main():
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
