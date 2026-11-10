import collections
import json
import subprocess
import sys
from pathlib import Path

page_in, page_out = sys.argv[1:3]
surface_file = sys.argv[3] if len(sys.argv) > 3 else None
sys.path.insert(0, str(Path(__file__).resolve().parent))
sys.argv = ["check.py"]
import check

if surface_file:
    surface = json.loads(Path(surface_file).read_text())
else:
    surface, refusal = check.fluent_surface(None)
    if refusal:
        sys.exit(refusal)

sentences_path = check.SPEC / "Fluent" / "sentences.json"
sentences = json.loads(sentences_path.read_text())
rows = sentences["strings"]
concepts = sentences["concepts"]

f1, f1c = check.fluent_concept_problems(surface, concepts)
f2, f2c = check.fluent_cell_problems(surface["cells"], check.fluent_cell_rows(sentences_path))
f3, f3c = check.fluent_shape_problems(surface, rows)
f4, f4c = check.fluent_callback_problems(surface, check.fluent_callback_table())
f5, f5c = check.fluent_string_problems(surface, rows)
counts = {**f1c, **f2c, **f3c, **f4c, **f5c}
head = subprocess.run(["git", "-C", str(check.REPO), "rev-parse", "--short", "HEAD"], capture_output=True, text=True).stdout.strip()

FIXED_NAMES = {
    "Pipeline": "pipeline",
    "BodyParts": "request",
    "QueryParts": "request",
    "JsonBody": "body",
    "MultipartBody": "body",
    "UrlEncodedBody": "body",
    "RequestDefaults": "requests",
}

KEPT_SHAPES = {
    "Get": "`Headers.Get(name)` is the Fetch API's `Headers.get` under its own name (N10.7), as every JavaScript object's member is named, and `Get(url)` is HTTP's GET",
    "Header": "settled by G7: once the default's value is the literal arm, its shape is one of the request's",
    "Input": "the input bound to a member: a pipeline hands it and a request sends it, as you named the request's parts on 2026-11-08 (`Form`, `Inputs`, `Input`)",
    "Json": "the JSON media type: a reply decoded as JSON (`Content.Json`), a body sent as JSON (`request.Json`), and a value written as JSON text for a render helper (N17.10)",
    "Plugin": "the plugin of a type: the plan listens to its events and a pipeline reaches its members (N17.11)",
    "Render": "markup the view writes: a field, an element by id, and a row action's control for one row (N17.12)",
    "Set": "a member's value, where a callback member's value is the function that answers it (D7.3, N17.9)",
}

ONE_NAME_TOO_MANY = {
    ("ShowErrors", "ShowValidationErrors"): "`ShowErrors(problem)` on a route and the defaults' `ShowValidationErrors()` both show a validation problem as the request's failures (D16.13). Both take `ShowValidationErrors`, the name the defaults need to say which errors.",
}


def cell_outcomes():
    refused = {(row["position"], row["source"]): row for row in sentences["cells"]["refused"]}
    outcome = {}
    for cell in surface["cells"]["results"]:
        key = (cell["position"], cell["source"])
        if cell["gap"]:
            outcome[key] = "gap"
        elif not cell["errors"]:
            outcome[key] = "compiles"
        elif key in refused and sorted(set(cell["errors"])) == sorted(refused[key]["errors"]):
            outcome[key] = "refused"
        else:
            outcome[key] = "failing:" + ",".join(sorted(set(cell["errors"])))
    return outcome


def gap_groups():
    position_gaps = {item["name"]: item["gap"] for item in surface["cells"]["positions"] if item["gap"]}
    source_gaps = {item["name"]: item["gap"] for item in surface["cells"]["sources"] if item["gap"]}
    groups = collections.Counter()
    for cell in surface["cells"]["results"]:
        if cell["gap"]:
            for label in {position_gaps.get(cell["position"]), source_gaps.get(cell["source"])} - {None}:
                groups[label] += 1
    return groups.most_common()


def slot_rows():
    slots = collections.defaultdict(collections.Counter)
    types = {}
    for parameter in surface["lambdas"]:
        if parameter["framework"]:
            slots[parameter["slot"]][parameter["name"]] += 1
            types[parameter["slot"]] = parameter["type"].partition("<")[0]
    table = []
    for slot, names in slots.items():
        fixed = FIXED_NAMES.get(types[slot])
        changes = sum(count for name, count in names.items() if fixed and name != fixed)
        table.append({"slot": slot, "uses": sum(names.values()), "names": dict(names.most_common()), "name": fixed, "changes": changes})
    return sorted(table, key=lambda row: (-row["uses"], row["slot"]))


def split_kinds(text):
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


def type_arguments(receiver):
    return receiver.partition("<")[2].rstrip(">")


def arguments_by_type():
    arguments = collections.defaultdict(set)
    for member in surface["members"]:
        arguments[member["receiverName"]].add(type_arguments(member["receiver"]))
    return arguments


def is_stage(member, result, arguments):
    own = type_arguments(member["receiver"])
    return result != member["receiverName"] and bool(own) and own in arguments.get(result, set())


def normal_shape(member, arguments):
    shape = check.fluent_shape(member, rows)
    kinds_text, result = shape[1:].split(")>", 1)
    kinds = split_kinds(kinds_text)
    if member["receiverName"] == "LiveObjects":
        kinds = ["path" if kind == "callback1" else kind for kind in kinds]
        result = "source" if result == "chain" else result
    if result in ("void", "chain") or is_stage(member, result, arguments):
        result = "continues"
    return tuple(kinds), result


def within(small, large):
    remaining = iter(large)
    return all(kind in remaining for kind in small)


def shape_verdicts():
    arguments = arguments_by_type()
    shapes = collections.defaultdict(lambda: collections.defaultdict(set))
    for member in surface["members"]:
        if member["memberKind"] == "method":
            shapes[member["member"]][member["receiver"]].add(normal_shape(member, arguments))
    exact = {line.split()[1].rstrip(":") for line in f3}
    verdicts = {}
    for name, receivers in sorted(shapes.items()):
        if len(receivers) < 2:
            continue
        fullest = [candidate for candidate in receivers.values()
                   if all(any(shape[1] == full[1] and within(shape[0], full[0]) for full in candidate)
                          for others in receivers.values() for shape in others)]
        verdicts[name] = "same" if name not in exact else "same under G3" if fullest else "G4"
    return verdicts


def synonym_rows(groups):
    known = {tuple(group) for group in groups}
    unknown = [group for group in ONE_NAME_TOO_MANY if group not in known]
    if unknown:
        sys.exit(f"ONE_NAME_TOO_MANY names a group F1 does not form: {unknown}")
    return [{"names": group, "verdict": ONE_NAME_TOO_MANY.get(tuple(group), "siblings")} for group in groups]


grid_outcome = cell_outcomes()
positions = [item["name"] for item in surface["cells"]["positions"]]
sources = [item["name"] for item in surface["cells"]["sources"]]
grid = [[grid_outcome[(position, source)] for source in sources] for position in positions]
slots = slot_rows()
fixed_uses = sum(row["uses"] for row in slots if row["name"])
renamed = sum(row["changes"] for row in slots)
data_uses = sum(row["uses"] for row in slots if not row["name"])
pipeline_names = {name for row in slots if row["name"] == "pipeline" for name in row["names"]}
shapes = shape_verdicts()
remaining = sorted(name for name, verdict in shapes.items() if verdict == "G4")
if set(remaining) != set(KEPT_SHAPES):
    sys.exit(f"G4 must give a verdict for exactly the names G3 leaves: left {remaining}, verdicts {sorted(KEPT_SHAPES)}")
same_under_rules = sorted(name for name, verdict in shapes.items() if verdict == "same under G3")
synonyms = synonym_rows(check.fluent_synonym_groups(concepts))
siblings = sum(1 for row in synonyms if row["verdict"] == "siblings")
gaps = gap_groups()

lines, verdict_counts, features = collections.Counter(), collections.Counter(), 0
for feature_path in sorted(check.SPEC.glob("*/features.json")):
    for feature in json.loads(feature_path.read_text())["features"]:
        features += 1
        lines.update(sentence["line"] for sentence in feature.get("sentences", []))
        verdict_counts.update(verdict.get("verdict") for verdict in feature.get("verdicts", []) or [])

flags = []
grep = subprocess.run(["git", "-C", str(check.REPO), "grep", "-n", "-i", "-E", "gate 2|gate2", "--", "spec/*.md", "spec/*/*.md"],
                      capture_output=True, text=True).stdout.splitlines()
for line in grep:
    file, number, text = line.split(":", 2)
    if file != "spec/BRIEF.md":
        flags.append({"file": file, "line": int(number), "text": text.strip()[:600]})

decisions = [
    {"n": "G0", "group": "Dates",
     "question": "A form field sends a `DateTime` or `DateOnly` as its day alone, which a culture whose calendar is not Gregorian binds in that calendar: th-TH binds 2026-10-27 as 1483-10-27 (probes core-form-day-calendar, core-form-date-forms-every-culture).",
     "recommendation": "Form data sends a date with its time, which binds Gregorian. It changes root CLAUDE.md's line \"A DateTime goes to the server as that text\", which is yours.",
     "refs": ["NOTES:747"]},
    {"n": "G1", "group": "Callback names",
     "question": f"The locked table in `.claude/rules/csharp.md` names a callback's parameter by the type it hands. Its six types are the current code's and none is on the rewrite's surface, so all {counts['F4_parameters']} framework callback parameters, in {counts['F4_slots']} slots, match no row (F4).",
     "recommendation": f"Keep the table keyed by the type a callback hands, and add the rewrite's rows: `Pipeline<TModel>` takes `pipeline`, as `PipelineBuilder` does today; `BodyParts<TRequest>` and `QueryParts<TRequest>` take `request`; `JsonBody`, `MultipartBody` and `UrlEncodedBody` take `body`; `RequestDefaults` takes `requests`. Every other slot hands data (a reply, a failure, a row's values, an event's payload) and takes the app's word for what it holds (`created`, `refused`): {data_uses} parameters. The six rows stay while the current code holds their types. Of the {fixed_uses} parameters in the fixed slots, {renamed} change name; the slot table below names each slot's.",
     "refs": ["F4"]},
    {"n": "G2", "group": "Callback names",
     "question": f"Callbacks nest: a route holds a request whose routes hold more. Today {counts['F4_hides']} parameters hide an enclosing name, so every nested pipeline takes a name of its own: {len(pipeline_names)} names for the one type (`north`, `admitted`, `done`, `shown`, `next` and more).",
     "recommendation": "Every slot that hands a pipeline names it `pipeline`, and a nested one hides the enclosing one. That loses nothing: a pipeline is the block it is written in, so a step lands where it sits whichever pipeline names it, and a source reads the same through any (the POC's `Pipeline` holds only the build, and each route and branch wraps the same build, which writes to the block being built: poc/Alis.Reactive.Poc/Pipelines/Branches.cs:21-22, GetRequests.cs:147). Every other slot never hides an enclosing name, since nested callbacks read the replies around them (spec/api/HttpSurface/EveryStage.cs:52-70). A lambda's parameter hides an enclosing one from C# 8 on (CS0136 refuses it under 7.3; a probe compiled both), and your 2026-11-07 line assumes C# 8 for an MVC 5 app.",
     "refs": ["F4"]},
    {"n": "G3", "group": "Names",
     "question": f"F3 compares a name's overloads across receivers exactly: {counts['F3_differ']} of {counts['F3_names']} names on several receivers differ, most only where a receiver continues its sentence through a stage type, offers fewer overloads, holds an operand, or is a live list.",
     "recommendation": f"F3 reads four things as the same shape. A door that continues (void, its receiver, or its next stage, a type that carries the receiver's type arguments, as `BodyParts<TRequest>` goes to `BodiedParts<TRequest>`) is one result. Each receiver's overloads are the fullest receiver's, less what its type cannot take (HEAD has no success body; one file is a whole body, several are multipart parts) or the operand it holds (a bound input holds its element, a request its container and its reply). A live list's queries read as a data list's: an item lambda is an expression on data and a callback on live objects. Under these {len(same_under_rules)} more names agree, and {len(remaining)} still differ (G4).",
     "refs": ["F3", "D6.2", "N5.5"]},
    {"n": "G4", "group": "Names",
     "question": f"Under G3, {len(remaining)} names still take two shapes: " + ", ".join(f"`{name}`" for name in remaining) + ".",
     "recommendation": "Keep each, with its reason as a row in spec/Fluent/sentences.json, as the refused cells are, so a new name with two shapes fails F3 until you approve it. " + "; ".join(f"`{name}`: {KEPT_SHAPES[name]}" for name in remaining) + ".",
     "refs": ["F3", "N10.7"]},
    {"n": "G5", "group": "Names",
     "question": f"F1 groups the names whose concept rows cite the same decisions: {len(synonyms)} groups.",
     "recommendation": f"{siblings} groups are siblings a decision lists together, each name doing a different thing (`Eq` and `NotEq`, `Then` and `Else`, `Get` and `Post`). " + " ".join(row["verdict"] for row in synonyms if row["verdict"] != "siblings"),
     "refs": ["F1", "D16.13"]},
    {"n": "G6", "group": "Strings",
     "question": "`Html.PartialFor(m => m.Member, view)` (D14.4) names its view with a string, which is none of the six string kinds root CLAUDE.md allows.",
     "recommendation": "`Html.PartialFor(m => m.Discharge)` renders the partial named after the member's type (a member typed `DischargeForm` renders the view `DischargeForm`), found where MVC finds a view by that name on each host, as `EditorFor` finds a template by type. Each partial has its own tight model (your 2026-11-06 line), so a second view of one model is a second model type, and no string names a view.",
     "refs": ["F5", "N14.1", "D14.4"]},
    {"n": "G7", "group": "Strings",
     "question": "`RequestDefaults.Header(value)` is a header value that only a literal reaches, because defaults are configured once in C# (D9.12).",
     "recommendation": "D7.1 names it a value position whose only arm is a literal.",
     "refs": ["F5", "D9.12"]},
    {"n": "G8", "group": "Every position takes every source",
     "question": f"{counts['F2_refused']} cells are refused only by C#'s nullability warning, CS8620: IfNull at a class, a comparison's right operand, a method's argument and a plugin's argument. spec/api builds with warnings as errors, and an app's views build as the app sets them.",
     "recommendation": "An analyzer refuses each as an error in every app; no overload can, since nullability is not part of a C# signature. `.OrNull()` stays the way through.",
     "refs": ["F2", "D7.3"]},
    {"n": "G9", "group": "Every position takes every source",
     "question": "`ElementClass` takes `Arg<string?>`, where an element's text and HTML take `ITypedSource<out T>`: one concept with two source rules. And a comparison's left operand takes no literal (CS1929), against D7.3.",
     "recommendation": "One rule for all three, the property set's `ITypedSource`. D7 states that a comparison's left operand is the source it tests, as `p.When(source)` reads (D8.2), with the literal on the right.",
     "refs": ["F2", "D7.3", "D8.2"]},
    {"n": "G10", "group": "Every position takes every source",
     "question": f"{counts['F2_gaps']} cells have no door yet. By the door each lacks: " + "; ".join(f"{label} ({count})" for label, count in gaps) + ". A cell that lacks both its position's door and its source's counts in both.",
     "recommendation": "These are the doors the rewrite adds, each in the module that owns it. No cell is refused for want of a door.",
     "refs": ["F2", "N7.1", "N7.2", "N7.3", "N21.1", "M1.N4"]},
    {"n": "G11", "group": "The sentences",
     "question": f"BRIEF step 7 says sentences.json holds every sentence. The {lines['main'] + lines['redesign'] + lines['proposed']} sentences of {features} features ({lines['main']} from main, {lines['redesign']} from the redesign, {lines['proposed']} proposed) live in the features' rows, beside {verdict_counts['renamed']} renamed, {verdict_counts['refused']} refused and {verdict_counts['kept']} kept doors.",
     "recommendation": "Keep them in the features' rows. A copy in sentences.json would be a second carrier of the same text, and sentences.json holds only what the compiler cannot know.",
     "refs": ["BRIEF step 7"]},
    {"n": "G12", "group": "Since gate 1",
     "question": "Core text and models that changed after your gate 1 yes.",
     "recommendation": "D5.13, D6.17, D17.5's iterator bullet and D15.3's two amendments are new meanings, named in this list. Three changes carry no new meaning: D11.9 now states, as D10.21 does, that a loader has no timeout; Tracing.tla keeps the trace headers when a sink fails, as D20.6 and D20.9 state; D7.1 and D7.2 name the Cells methods.",
     "refs": ["D5.13", "D6.17", "D17.5", "D15.3", "D11.9", "D20.6", "D7.1"]},
]

checks = [
    {"id": "F1", "title": "One concept, one name", "status": "pass" if not f1 else "fail",
     "line": f"{counts['F1_names']} names, {counts['F1_rows']} concept rows, {counts['F1_synonym_groups']} synonym groups", "problems": f1[:20]},
    {"id": "F2", "title": "Every position takes every source", "status": "pass" if not f2 else "fail",
     "line": f"{counts['F2_cells']} cells: {counts['F2_compiling']} compile, {counts['F2_refused']} refused as named, {counts['F2_gaps']} no door yet, {counts['F2_failing']} failing; refused forms {counts['F2_forms_as_named']} of {counts['F2_forms']}",
     "problems": [problem for problem in f2 if "no door" not in problem][:20]},
    {"id": "F3", "title": "The same meaning has the same shape", "status": "pass" if not f3 else "fail",
     "line": f"{counts['F3_differ']} of {counts['F3_names']} names differ", "problems": f3},
    {"id": "F4", "title": "Callback names follow the locked table", "status": "pass" if not f4 else "fail",
     "line": f"{counts['F4_parameters']} parameters in {counts['F4_slots']} slots; {counts['F4_no_row']} without a row, {counts['F4_hides']} hide an enclosing name", "problems": []},
    {"id": "F5", "title": "No string names a member", "status": "pass" if not f5 else "fail",
     "line": f"{counts['F5_parameters']} string parameters, {counts['F5_rows']} kinds written, {counts['F5_untyped']} object or Type", "problems": f5},
]

gate2 = {
    "head": head,
    "branch": "feature/spec-first",
    "decision": "Gate 2 asks for a yes or no on each of G0 to G12 below: the fluent API as the compiler reads it today. spec/probes/FluentSurface compiles the POC and spec/api, and check_fluent decides the five checks of BRIEF step 7 from that output. F1 passes. F2 to F5 fail only on what these decisions settle and on the doors the rewrite adds, and a yes to each leaves no name or rule for me to choose. Each decision comes with my recommendation. spec/tools/gate2_page.py computes every count on this tab from check_fluent's output; the date facts cite their probes.",
    "changed": "Changed since the first version of this tab: its G2 gave counts for OnSuccess, OnError and Chained that came from an architect's report, not from check_fluent, and named only three doors; G1 and G2 now name every slot by one rule. Its G3 renamed the header read; G4 now keeps it, since it is the Fetch API's own `get`. The names G3 left without a recommendation now each have one (G3, G4), and so does each synonym group (G5).",
    "checks": checks,
    "decisions": decisions,
    "positions": [{"name": item["name"], "gap": item["gap"]} for item in surface["cells"]["positions"]],
    "sources": [{"name": item["name"], "gap": item["gap"]} for item in surface["cells"]["sources"]],
    "grid": grid,
    "slots": slots,
    "shapes": [{"name": name, "verdict": verdict} for name, verdict in sorted(shapes.items())],
    "synonyms": synonyms,
    "work": [
        "After your yes: the table's new rows in .claude/rules/csharp.md, F4 reading them by type with G2's nesting rule, and the spec/api lambdas renamed to them, in one commit; F3's rules and the kept rows (G3, G4); one show-errors name (G5); PartialFor's typed door (G6); the D7 lines (G7, G9); the CS8620 refusals (G8); the date line (G0).",
        "Step 8: spec/Reference, a custom TypeScript component and the real MultiSelect, carrying every conformance cell (BRIEF section 8).",
        "Steps 9 to 12: the promises, check.py complete, spec/REWRITE.md in SliceTree order, the root CLAUDE.md.",
    ],
    "flags": flags,
}

render = r'''function renderGate2() {
  const box = document.getElementById("gate2");
  if (box.dataset.done) return;
  box.dataset.done = "1";
  const G = SDK.gate2;
  const code = s => esc(s).replace(/`([^`]+)`/g, "<code>$1</code>");
  const refs = d => d.refs.map(r => `<code>${esc(r)}</code>`).join(" ");
  const decisionRows = G.decisions.map(d => `<tr><td>${d.n}</td><td>${esc(d.group)}</td><td>${code(d.question)}</td><td>${code(d.recommendation)}</td><td class="refs">${refs(d)}</td></tr>`).join("");
  const checkRows = G.checks.map(c => `<tr><td>${c.id}</td><td>${esc(c.title)}</td><td><span class="status ${c.status}">${c.status}</span></td><td>${esc(c.line)}</td></tr>`).join("");
  const problems = G.checks.filter(c => c.problems.length).map(c => `<details class="flags"><summary>${c.id}: ${c.problems.length} ${c.problems.length === 1 ? "line" : "lines"}</summary>${c.problems.map(p => `<div class="flag">${code(p)}</div>`).join("")}</details>`).join("");
  const mark = { compiles: "✓", refused: "R", gap: "·" };
  const label = { compiles: "compiles", refused: "refused as named", gap: "no door yet" };
  const head = `<tr><th></th>${G.sources.map(s => `<th title="${esc(s.gap || "")}">${esc(s.name)}${s.gap ? " ·" : ""}</th>`).join("")}</tr>`;
  const body = G.positions.map((p, i) => `<tr><th title="${esc(p.gap || "")}">${esc(p.name)}${p.gap ? " ·" : ""}</th>${G.grid[i].map((o, j) => {
    const kind = o.startsWith("failing") ? "failing" : o;
    const text = kind === "failing" ? "✕" : mark[kind];
    const title = `${p.name} × ${G.sources[j].name}: ${kind === "failing" ? "fails " + o.slice(8) : label[kind]}`;
    return `<td class="${kind}" title="${esc(title)}" aria-label="${esc(title)}">${text}</td>`;
  }).join("")}</tr>`).join("");
  const slotName = s => s.name ? `<code>${esc(s.name)}</code>` : "the app's word, never hiding";
  const slotRows = G.slots.map(s => `<tr><td><code>${esc(s.slot)}</code></td><td class="num">${s.uses}</td><td class="slotnames">${Object.entries(s.names).map(([n, c]) => `<code>${esc(n)}</code> ${c}`).join(" ")}</td><td>${slotName(s)}</td><td class="num">${s.name ? s.changes : ""}</td></tr>`).join("");
  const shapeLabel = { "same": "same shape", "same under G3": "same under G3's rules", "G4": "two shapes, kept by G4" };
  const shapeRows = G.shapes.map(s => `<tr><td><code>${esc(s.name)}</code></td><td>${esc(shapeLabel[s.verdict])}</td></tr>`).join("");
  const synonymRows = G.synonyms.map(g => `<li>${g.names.map(n => `<code>${esc(n)}</code>`).join(" ")}${g.verdict === "siblings" ? "" : ` <b>${code(g.verdict)}</b>`}</li>`).join("");
  box.innerHTML = `
    <section>
      <h2>The decision</h2>
      <div class="ask"><p style="color: var(--ink); margin: 0">${code(G.decision)}</p></div>
      <p>Read from <code>${esc(G.branch)}</code> at <code>${esc(G.head)}</code>. The type cards on the other tabs show the POC at 108f520fa and the HTTP design at 751f5d6e4; the POC is unchanged since, and the HTTP design gained <code>OnError&lt;TError&gt;</code> (e592e3e6d).</p>
      <p>${code(G.changed)}</p>
    </section>
    <section>
      <h2>The five checks</h2>
      <div class="scroll"><table class="metrics"><thead><tr><th>#</th><th>Check</th><th>Today</th><th>What it found</th></tr></thead><tbody>${checkRows}</tbody></table></div>
      ${problems}
    </section>
    <section>
      <h2>Your decisions</h2>
      <p>${G.decisions.length} questions. Answer each with yes, no, or the alternative you want.</p>
      <div class="scroll"><table class="metrics decisions"><thead><tr><th>#</th><th>Area</th><th>Question</th><th>Recommendation</th><th>From</th></tr></thead><tbody>${decisionRows}</tbody></table></div>
    </section>
    <section>
      <h2>Every position takes every source</h2>
      <p>Each row is a value position from D7.1, each column a source from D7.2, and each cell the source compiled into the position, with the POC and spec/api. A dot after a name means it has no door yet. Hover a cell for its reading.</p>
      <div class="cell-key"><span><i style="background: var(--author-soft)"></i>compiles</span><span><i style="background: var(--accent-soft)"></i>refused as named (R)</span><span><i style="background: var(--bg)"></i>no door yet</span><span><i style="background: var(--event-soft)"></i>fails (✕)</span></div>
      <div class="scroll"><table class="cells"><thead>${head}</thead><tbody>${body}</tbody></table></div>
    </section>
    <section>
      <h2>Callback slots</h2>
      <p>Each slot is a door, a delegate parameter's index and the type it hands, with the names views give that parameter today and how often, the name G1 gives it, and how many uses change. A slot that hands data takes the app's word for what it holds and never hides an enclosing name (G2).</p>
      <div class="scroll"><table class="metrics"><thead><tr><th>Slot</th><th class="num">Uses</th><th>Names today</th><th>G1's name</th><th class="num">Change</th></tr></thead><tbody>${slotRows}</tbody></table></div>
    </section>
    <section>
      <h2>Names on several receivers</h2>
      <p>Each name F3 compares, and what it is under G3 and G4.</p>
      <div class="scroll"><table class="metrics"><thead><tr><th>Name</th><th>Shape</th></tr></thead><tbody>${shapeRows}</tbody></table></div>
    </section>
    <section>
      <h2>Names that state the same meaning</h2>
      <p>${G.synonyms.length} groups, from the concept rows in <code>spec/Fluent/sentences.json</code> (G5). A group with no note is siblings: each name does a different thing its decision lists.</p>
      <ul class="plain">${synonymRows}</ul>
    </section>
    <section>
      <h2>The work after your yes</h2>
      <ul class="plain">${G.work.map(n => `<li>${code(n)}</li>`).join("")}</ul>
    </section>
    <section>
      <h2>Every line that names gate 2</h2>
      <p>Each line of the spec's prose that names gate 2, verbatim, with its file and line: the module items, each carrying its designed default.</p>
      <details class="flags"><summary>${G.flags.length} lines</summary>${G.flags.map(f => `<div class="flag"><b>${esc(f.file)}:${f.line}</b> ${code(f.text)}</div>`).join("")}</details>
    </section>`;
}
'''

page = Path(page_in).read_text()
if page.count('id="tab-gate2"') != 1 or page.count("\nfunction renderGate2() {") != 1:
    sys.exit("the page holds no Gate 2 tab to update: pass the SDK Map page as the Artifact read saves it")
data_start = page.index("const SDK = ")
data_end = page.index("\n", data_start)
data_line = page[data_start:data_end]
sdk = json.loads(data_line[len("const SDK = "):].rstrip(";"))
sdk["gate2"] = gate2
page = page[:data_start] + "const SDK = " + json.dumps(sdk, ensure_ascii=False, separators=(",", ":")) + (";" if data_line.endswith(";") else "") + page[data_end:]
render_start = page.index("\nfunction renderGate2() {") + 1
render_end = page.index("\nfunction ", render_start) + 1
page = page[:render_start] + render + page[render_end:]
Path(page_out).write_text(page)
print(f"gate 2 at {head}: F3 {counts['F3_differ']} of {counts['F3_names']} differ, {len(same_under_rules)} same under G3, {len(remaining)} kept by G4; "
      f"F4 {counts['F4_parameters']} parameters, {fixed_uses} in fixed slots, {renamed} renamed, {data_uses} data; "
      f"F5 {len(f5)} lines; synonyms {len(synonyms)} groups, {siblings} siblings; gaps {gaps}")
