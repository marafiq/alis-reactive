#!/usr/bin/env python3
import hashlib
import json
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
}
INVENTORY_FILES = ["doors.tsv", "kinds.tsv", "cases.tsv", "analyzers.tsv", "tests.tsv"]
QUESTIONS = [f"Q{n}" for n in range(1, 20)]
DESIGN_PARTS = ["Decision", "Schema nodes", "Checker", "Doors", "Evidence", "Defects"]
TLC_SPECS = {
    "Absence": {"question": "Q4", "step": 4, "expect": "green"},
    "Request": {"question": "Q9 Q10", "step": 4, "expect": "green"},
    "Lanes": {"question": "Q11", "step": 4, "expect": "green"},
    "Ownership": {"question": "Q12", "step": 4, "expect": "green"},
    "DateWire": {"question": "date wire form", "step": 4, "expect": "green"},
    "Schema": {"question": "Q19", "step": 5, "expect": "green"},
    "ValidationToday": {"question": "Q16 at bff9eb6c0", "step": 5, "expect": "four counterexamples"},
    "Validation": {"question": "Q16", "step": 5, "expect": "green"},
}
HEDGES = [
    "rare", "rarely", "usually", "may", "might", "maybe", "perhaps", "possibly", "probably", "likely",
    "typically", "generally", "often", "sometimes", "ideally", "should", "etc", "tbd", "todo",
    "when needed", "where possible", "if possible", "as needed", "as appropriate", "for now",
]
HEDGE = re.compile(r"\b(" + "|".join(re.escape(word).replace(r"\ ", r"\s+") for word in HEDGES) + r")\b", re.IGNORECASE)
PROSE_KEYS = {
    "meaning", "reason", "decision", "improvement", "page", "gesture", "outcome", "opposite", "defect",
    "summary", "why", "statement", "description",
}


def git(*args):
    return subprocess.run(["git", "-C", str(REPO), *args], capture_output=True, text=True).stdout.strip()


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


def check_inventory():
    paths = [SPEC / "inventory" / name for name in INVENTORY_FILES]
    if any(not path.exists() for path in paths):
        return not_started(*paths)
    return not_built("the inventory")


def design_sections():
    path = SPEC / "Core" / "DESIGN.md"
    if not path.exists():
        return None
    sections = {}
    current = None
    for line in path.read_text().splitlines():
        heading = re.match(r"^## (Q\d+)\b", line)
        if heading:
            current = heading.group(1)
            sections[current] = []
        elif current:
            sections[current].append(line)
    return sections


def check_questions(questions):
    return lambda: answered_questions(questions)


def answered_questions(questions):
    sections = design_sections()
    if sections is None:
        return not_started(SPEC / "Core" / "DESIGN.md")
    answered = []
    unanswered = []
    for question in questions:
        body = "\n".join(sections.get(question, []))
        missing = [part for part in DESIGN_PARTS if not re.search(r"^\*\*" + re.escape(part) + r"\.\*\*", body, re.MULTILINE)]
        (unanswered if question not in sections or missing else answered).append(question if not missing else f"{question} lacks {', '.join(missing)}")
    return result(len(answered) == len(questions), "; ".join(unanswered) or "every question answered", answered=len(answered), total=len(questions))


def spec_hash(tla, cfg):
    return hashlib.sha256(tla.read_bytes() + b"\0" + cfg.read_bytes()).hexdigest()


def run_tlc(name, tla, cfg):
    with tempfile.TemporaryDirectory() as meta:
        completed = subprocess.run(
            [JAVA, "-cp", str(TLA_JAR), "tlc2.TLC", "-deadlock", "-metadir", meta, "-config", cfg.name, tla.name],
            cwd=tla.parent, capture_output=True, text=True,
        )
    lines = (completed.stdout + completed.stderr).splitlines()
    green = any(TLC_GREEN in line for line in lines)
    violated = sorted({match.group(1) for line in lines for match in [re.search(r"Invariant (\w+) is violated", line)] if match})
    states = next((line.strip() for line in reversed(lines) if "distinct states found" in line), "")
    return {"green": green, "violated": violated, "line": TLC_GREEN if green else ("violated: " + ", ".join(violated) if violated else lines[-1] if lines else "no output"), "states": states}


def check_tlc(brief, previous, step):
    cached = previous.get("checks", {}).get("tlc", {}).get("runs", {})
    runs = {}
    problems = []
    specs = {name: expectation for name, expectation in TLC_SPECS.items() if expectation["step"] <= step}
    for name, expectation in specs.items():
        tla = SPEC / "Core" / f"{name}.tla"
        cfg = SPEC / "Core" / f"{name}.cfg"
        if not tla.exists() or not cfg.exists():
            problems.append(f"{name} not started")
            continue
        digest = spec_hash(tla, cfg)
        run = cached.get(name)
        if not run or run.get("hash") != digest:
            if brief:
                problems.append(f"{name} changed since its last run")
                continue
            run = {"hash": digest, **run_tlc(name, tla, cfg)}
        runs[name] = run
        if expectation["expect"] == "green" and not run["green"]:
            problems.append(f"{name}: {run['line']}")
        if expectation["expect"] == "four counterexamples" and len(run["violated"]) != 4:
            problems.append(f"{name}: expected four violated invariants, saw {run['violated']}")
    outcome = result(not problems, "; ".join(problems) or "every spec meets its expectation", specs=len(specs), met=len(specs) - len(problems))
    outcome["runs"] = runs
    return outcome


def check_probes():
    folder = SPEC / "probes"
    if not folder.exists():
        return not_started(folder)
    return not_built("the probes")


def check_history():
    path = SPEC / "history.tsv"
    if not path.exists():
        return not_started(path)
    return not_built("the regression history")


def check_kinds():
    path = SPEC / "Core" / "schema.md"
    if not path.exists():
        return not_started(path)
    return not_built("the kind sets and their handlers")


def gate_answered(number):
    notes = (SPEC / "NOTES.md").read_text() if (SPEC / "NOTES.md").exists() else ""
    return re.search(rf"^- \d{{4}}-\d{{2}}-\d{{2}} owner, gate {number}: yes\b", notes, re.MULTILINE) is not None


def check_gate(number):
    return lambda: result(gate_answered(number), f"gate {number} " + ("answered yes" if gate_answered(number) else "not answered"))


def feature_files():
    return sorted(SPEC.glob("*/features.json"))


def check_features(what):
    def run():
        if not feature_files():
            return result(False, "not started: no spec/<Module>/features.json")
        return not_built(what)
    return run


def check_fluent():
    path = SPEC / "Fluent" / "sentences.json"
    if not path.exists():
        return not_started(path)
    return not_built("the fluent checks")


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
    json_files = sorted(SPEC.glob("**/*.json"))
    json_files = [path for path in json_files if path != STATE]
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


def check_no_comments():
    hits = []
    for path in sorted(SPEC.glob("**/*.py")):
        for number, line in enumerate(path.read_text().splitlines(), 1):
            stripped = line.strip()
            if number == 1 and stripped.startswith("#!"):
                continue
            if stripped.startswith("#") or re.search(r"\s#\s", line) and not re.search(r"(['\"]).*#.*\1", line):
                hits.append(f"{path.relative_to(REPO)}:{number}")
            if stripped.startswith(('"""', "'''")):
                hits.append(f"{path.relative_to(REPO)}:{number} docstring")
    for path in sorted(SPEC.glob("**/*.tla")):
        for number, line in enumerate(path.read_text().splitlines(), 1):
            if "\\*" in line or "(*" in line:
                hits.append(f"{path.relative_to(REPO)}:{number}")
    return result(not hits, "; ".join(hits[:10]) or "no comment in the spec's code", comments=len(hits))


CHECKS = [
    ("survival", "brief, notes, file map, check, compact hook and pushed branch", check_survival),
    ("main_surface", "main's PublicAPI text per package and target framework", check_main_surface),
    ("inventory", "doors, kinds, case labels, analyzer ids and tests as TSV", check_inventory),
    ("core_questions", "Q1 to Q18 answered", check_questions(QUESTIONS[:18])),
    ("core_tlc", "the TLC specs of Q4, Q9 with Q10, Q11, Q12 and the date wire form green", None),
    ("questions", "every core design question answered", check_questions(QUESTIONS)),
    ("tlc", "every TLC spec meets its expectation", None),
    ("probes", "every server fact a spec assumes is pinned by a probe", check_probes),
    ("history", "every regression SHA tied to a spec line", check_history),
    ("kinds", "grammar, schema and spec node sets equal; one handler per kind", check_kinds),
    ("gate1", "owner gate 1: the core design", check_gate(1)),
    ("doors_mapped", "every public door mapped to one feature", check_features("the door mapping")),
    ("verdicts", "every main-only door has a verdict; a refusal names its carriers", check_features("the main-only verdicts")),
    ("features_core", "every feature mapped to the core", check_features("the core mapping")),
    ("features_lost", "every module and feature on either line carried: features lost 0", check_features("the lost-feature count")),
    ("fluent", "every module's sentences consistent", check_fluent),
    ("gate2", "owner gate 2: the fluent API", check_gate(2)),
    ("matrix", "every conformance cell promised for both reference components", check_matrix),
    ("defects", "every defect on its highest rung; no test for a type or analyzer defect", check_features("the defect rungs")),
    ("promises", "every refusal and behavior defect has a promise", check_features("the promises")),
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
    (5, "C", "Q19, the schema, the validation spec, gate 1", ["questions", "tlc", "kinds", "gate1"], "Write Schema.tla, schema.md and the validation specs; present gate 1."),
    (6, "D", "Feature rows", ["doors_mapped", "verdicts", "features_core", "features_lost"], "Write spec/<Module>/features.json and module.md for each module."),
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
    print("\n".join(lines))


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
