# Handoff

Written at `date`: Wed Nov 18 04:40:04 EST 2026

## State, as `scripts/state.py` reads it from git
note: open 2026-11-18 | direction | the queue of this file | queue | "Think of real domain, which has all these complex
  leaves by: its follow-up lands, with the line in its home and the owner's words in `spec/NOTES.md`
  read first: the home the entry names
  exit line: "landed"
  goes to: note: closed
note: open 2026-11-18 | direction | the queue of this file | queue | "I want you to build a skill out of that TLA plus 
note: open 2026-11-18 | decision | spec/CLAUDE.md, the conformance app's decisions | MoveInSpec | "This should not be i
note: open 2026-11-18 | decision | spec/CLAUDE.md, the conformance app's decisions | MoveInSpec | "The framework is not
note: open 2026-11-18 | decision | spec/CLAUDE.md, the conformance app's decisions | MoveInSpec | "Yeah, I'm fine with 
note: open 2026-11-18 | quality rule | spec/Reference/native.json and tests/Alis.Reactive.Conformance.App/CLAUDE.md | C
note: open 2026-11-18 | quality rule | tests/Alis.Reactive.Conformance.App/CLAUDE.md | MoveInSpec | "Build an app which
note: open 2026-11-17 | decision | spec/CLAUDE.md, the Conditions module's decisions, with a table "After 1.0" in spec/
note: open 2026-11-17 | process rule | .claude/rules/process.md | queue | "your whole goal is to get the tooling, the p
note: open 2026-11-18 | decision | spec/CLAUDE.md | DecisionsByModule | "where you're gonna keep the decisions. So it c
note: open 2026-11-18 | quality rule | .claude/rules/views.md, and an analyzer of Alis.Reactive.Analyzers that refuses 
note: open 2026-11-18 | quality rule | tests/Alis.Reactive.Conformance.App/CLAUDE.md | MoveInSlice1 | "For the app, you
note: open 2026-11-18 | direction | the queue of this file | queue | "I don't need acceptance numbers, sir. I need that
unit: building ../ci2 CiGreen2: it lacks paths.txt or message.txt
  leaves by: the builder's work: the test red, the code, the test green, `scripts/plant-defect.sh`
  read first: the rule file of each kind of file, as it is opened
  exit line: each line of the brief's "Done"
  goes to: unit: ready, or unit: returned for an agent
unit: waiting ../ci2 MoveInSpec: for MoveInSlice1
  leaves by: the unit it names lands; then it moves to the new head and each command of its "Done" runs there
  exit line: each line of the brief's "Done"
  goes to: unit: ready
unit: ready ../movein1 MoveInSlice1
  next: scripts/land-unit.sh ../movein1 e9b49a1eb "conformance app: the move-in's foundation and MI.1, a residence chosen through three lookups over a model one world owns" ".landing/MoveInSlice1"
  leaves by: `scripts/land-unit.sh {worktree} {base} "{title}" ".landing/{name}"`, while the lane is pushed
  exit line: "landed", with the browser run's result line
  goes to: unit: landed
lane: pushed at f4cd5af46 on feature/rewrite: equal to the remote
  leaves by: the landing of a unit that is ready
  read first: the queue of `HANDOFF.md`
  exit line: "landed"
  goes to: lane: landed
ci: unread at f4cd5af46
  leaves by: `python3 scripts/ci.py`
  exit line: "ci at" the commit, with each job
  goes to: ci: running, ci: failed or ci: passed
file: over CLAUDE.md: holds 326 lines, its limit 200
  leaves by: a unit that splits or cuts it; the mistakes are condensed, kinds merged, each line short enough to follow
  exit line: "landed"
  goes to: gone
file: over spec/CLAUDE.md: holds 2064 lines, its limit 150
file: over HANDOFF.md: holds 202 lines, its limit 150
file: over tests/Alis.Reactive.Conformance.App/CLAUDE.md: holds 201 lines, its limit 150
file: over tests/Alis.Reactive.Conformance.PlaywrightTests/CLAUDE.md: holds 234 lines, its limit 150

## Where it stands
- Branch: feature/rewrite
- HEAD: f4cd5af46 conformance app: the move-in's foundation and MI.1, a residence chosen through three lookups over a model one world owns
- Full check: .work/logs/spec-check-f4cd5af46.log: spec check at f4cd5af46: 40 of 40 checks pass
- .work/last-run.txt: 20261118-042953-62784 f4cd5af46

## Landed since the commit before the last spec: STATE commit
f4cd5af46 conformance app: the move-in's foundation and MI.1, a residence chosen through three lookups over a model one world owns
c044853ea spec: STATE and handoff at 4c5ae8e1c

## Open
open: 19 promises with no browser test, 7 runtime cases not built, 14 doors not implemented
feature promises: 538; named by a browser test: 519; with no browser test: 19
  FusionAppLevelObjects  The confirm                                  M23_F2.M23_X2_0
  FusionAppLevelObjects  The confirm                                  M23_F2.M23_X3_0
  FusionAppLevelObjects  The page's toast                             M23_F1.M23_X1_0
  FusionComponents       FusionBreadcrumb                             M21_F6.M21_X1002_0
  FusionComponents       FusionGrid                                   M21_F23.M21_X201_0
  FusionComponents       FusionGrid                                   M21_F23.M21_X201_1
  FusionComponents       FusionGrid                                   M21_F23.M21_X201_2
  FusionComponents       FusionGrid                                   M21_F23.M21_X203_0
  FusionComponents       FusionGrid                                   M21_F23.M21_X203_1
  FusionComponents       FusionGrid                                   M21_F23.M21_X203_2
  FusionComponents       FusionGrid                                   M21_F23.M21_X205_0
  FusionComponents       FusionGrid                                   M21_F23.M21_X205_1
  FusionComponents       FusionMenu                                   M21_F30.M21_X406_0
  FusionComponents       FusionSidebar                                M21_F40.M21_X1024_0
  FusionComponents       FusionSmartPasteButton                       M21_F42.M21_X803_0
  FusionComponents       FusionSmartPasteButton                       M21_F42.M21_X803_1
  FusionComponents       FusionSmartPasteButton                       M21_F42.M21_X804_0
  FusionComponents       FusionToolbar                                M21_F51.M21_X1032_0
  PathsAndWire           Paths and wire                               M15_F1.X3_7_0
runtime cases not built:
Alis.Reactive.Assets/runtime/Conditions/operators.ts:43
Alis.Reactive.Assets/runtime/InputsAndIds/model.ts:240
Alis.Reactive.Assets/runtime/InputsAndIds/model.ts:248
Alis.Reactive.Assets/runtime/InputsAndIds/model.ts:264
Alis.Reactive.Assets/runtime/RequestsAndResponses/request.ts:274
Alis.Reactive.Assets/runtime/Validation/forms.ts:194
Alis.Reactive.Assets/runtime/Validation/forms.ts:231
doors not implemented:
Alis.Reactive.Fusion/FusionAppLevelObjects/FusionConfirm/FusionConfirmHtml.Net10.cs:16
Alis.Reactive.Fusion/FusionAppLevelObjects/FusionConfirm/FusionConfirmHtml.Net10.cs:25
Alis.Reactive.Fusion/FusionAppLevelObjects/FusionConfirm/FusionConfirmHtml.Net48.cs:16
Alis.Reactive.Fusion/FusionAppLevelObjects/FusionConfirm/FusionConfirmHtml.Net48.cs:25
Alis.Reactive.Fusion/FusionAppLevelObjects/FusionToast/FusionToastHtml.Net10.cs:16
Alis.Reactive.Fusion/FusionAppLevelObjects/FusionToast/FusionToastHtml.Net48.cs:16
Alis.Reactive.Fusion/FusionComponents/FusionBreadcrumb/FusionBreadcrumbHtml.Net10.cs:21
Alis.Reactive.Fusion/FusionComponents/FusionBreadcrumb/FusionBreadcrumbHtml.Net48.cs:21
Alis.Reactive.Fusion/FusionComponents/FusionMenu/FusionMenuHtml.Net10.cs:21
Alis.Reactive.Fusion/FusionComponents/FusionMenu/FusionMenuHtml.Net48.cs:21
Alis.Reactive.Fusion/FusionComponents/FusionSidebar/FusionSidebarHtml.Net10.cs:21
Alis.Reactive.Fusion/FusionComponents/FusionSidebar/FusionSidebarHtml.Net48.cs:21
Alis.Reactive.Fusion/FusionComponents/FusionToolbar/FusionToolbarHtml.Net10.cs:21
Alis.Reactive.Fusion/FusionComponents/FusionToolbar/FusionToolbarHtml.Net48.cs:21

## Mistakes counted since the last push, by kind: a kind that grew names a fix to its tool or its rule file
- Written from memory, not read at its source at HEAD: a design line, a signature, a path, a name, a page's expected value, a case's shape or a TLA+ ordering: 334 to 346
- A rule or a tool's list not applied before the change: 381 to 384
- A number, a proof line or an absence not read from the run that proves it: 53 to 54
- A shell command or a script wrong in its own terms: 533 to 542
- A fault found in an agent's work whose instruction the brief did not give: 2 to 5

## The owner's latest decisions
spec/CLAUDE.md:1926: - The owner, 2026-11-18, answering the lead's ask on what an element's text shows for an enum, unit 3 as built having
spec/CLAUDE.md:1936: - The owner, 2026-11-18, after the lead reported that the hook a compaction runs printed a file of the lead's private
spec/CLAUDE.md:1945: - The owner, 2026-11-18, of that machine (both verbatim in `spec/NOTES.md`): "Is there a mistake read part of the state
spec/CLAUDE.md:1956: - The owner, 2026-11-18, the same hour (verbatim in `spec/NOTES.md`): "What other steps are missing? How about quality
spec/CLAUDE.md:1966: - The owner, 2026-11-18, on the machine as built (verbatim in `spec/NOTES.md`): the mistakes are condensed past a
spec/CLAUDE.md:1979: - The owner, 2026-11-18, after a unit's delivery proof stopped before its browser step (verbatim in `spec/NOTES.md`):
spec/CLAUDE.md:1986: - The owner, 2026-11-18, on the machine as the lead had written it, one table with sentences around it and checks
spec/CLAUDE.md:1993: - The owner, 2026-11-18 (verbatim in `spec/NOTES.md`): "I want you to build a skill out of that TLA plus and schema,
spec/CLAUDE.md:2000: - The owner, 2026-11-18, on README's first example and both fresh apps' home views (verbatim in `spec/NOTES.md`):

## The owner's notes that are open: date | kind | home | follow-up | the owner's words
<!-- notes -->
- 2026-11-18 | direction | the queue of this file | queue | "Think of real domain, which has all these complexities. Get rid of this desk thinking. Keep other pages, other browser tasks, but write this complex app from ground up with full scale complexity. This framework has features to handle it."
- 2026-11-18 | direction | the queue of this file | queue | "I want you to build a skill out of that TLA plus and schema, which will be used by every consumer as well." "This skill you are building should be a guiding principle for any developer to write any complex app by simple decisions, because it has a map, right? It has clear names, right?"
- 2026-11-18 | decision | spec/CLAUDE.md, the conformance app's decisions | MoveInSpec | "This should not be in the skill. This is for you to use the skill to write the app, but you have to first list down every feature the app will have ... The specs of the apps are a separate thing, but you should get help from the specs and the TLA plus that how that app should be written." "Why you need to do inventory again? You have TLA+, you have schema."
- 2026-11-18 | decision | spec/CLAUDE.md, the conformance app's decisions | MoveInSpec | "The framework is not getting new code except the current module in progress. You gotta build the features of the app, and you are already building a skill that will help you write quickly. What decision you are talking about?"
- 2026-11-18 | decision | spec/CLAUDE.md, the conformance app's decisions | MoveInSpec | "Yeah, I'm fine with the native packages." "Are you going to write components from scratch? That is what the TypeScript component was written for, right? So we can exercise the framework with conformance app, so it stay faster."
- 2026-11-18 | quality rule | spec/Reference/native.json and tests/Alis.Reactive.Conformance.App/CLAUDE.md | ConformanceMembers | "Does it have a property returning a complex object, method, multiple parameters? You know, all those combinations have been repeated like 100 times by me." "does conformance has promise. So you can test the lanes of sync and async."
- 2026-11-18 | quality rule | tests/Alis.Reactive.Conformance.App/CLAUDE.md | MoveInSpec | "Build an app which has all possible combinations." "Where is chain request? Where is parallel request? Where is complex conditions within each module? Where is header? Where is query string? Where is hidden fields? Where is getting value from other component?"
- 2026-11-17 | decision | spec/CLAUDE.md, the Conditions module's decisions, with a table "After 1.0" in spec/NOTES.md | Operators | "Yeah, let's remove it. Let me help you to close the framework, and it can come back later, but note it down somewhere in a structured way so we can follow up."
- 2026-11-17 | process rule | .claude/rules/process.md | queue | "your whole goal is to get the tooling, the process, the quality rules, how to engage agents, how to scope their work, how to explicitly state what done means, and even for yourself, handoff before compaction notes, keeping it live, how to update status, the whole harness has to be reviewed." Left of it: a planted-defect line for each promise read by the landing script; the conformance app's coverage of the core's doors as a number; the independent inspection of the runtime and the core.
- 2026-11-18 | decision | spec/CLAUDE.md | DecisionsByModule | "where you're gonna keep the decisions. So it cannot be one bulky file."
- 2026-11-18 | quality rule | .claude/rules/views.md, and an analyzer of Alis.Reactive.Analyzers that refuses a plan opened over dynamic or object | TypedDoors | "Dude, why would you use dynamic? The whole point of the framework is tight, and you end up using dynamic. What a pity."
- 2026-11-18 | quality rule | tests/Alis.Reactive.Conformance.App/CLAUDE.md | MoveInSlice1 | "For the app, you need to build fake data ... Sending request isn't enough. It has to send the correct payload so you can observe what is being returned, what is being changed ... you gotta build the domain model of the apps in memory so that you can observe real behavior rather than just assume, oh, I sent the request."
- 2026-11-18 | direction | the queue of this file | queue | "I don't need acceptance numbers, sir. I need that complex app behavior asserted and proved. I am done with hearing excuses or fake numbers."
<!-- /notes -->

## Next
<!-- next -->
The owner is away for 24 hours from the early morning of 2026-11-18. The lead works alone, THE APP FIRST AND ONLY,
and spends little: it acts on a notification, never polls, runs one build at a time on this Mac, and adds no process.

WHAT THE OWNER GETS ON HIS RETURN, in his words: "I don't need acceptance numbers, sir. I need that complex app behavior
asserted and proved. I am done with hearing excuses or fake numbers." So the report names each behavior of the
move-in app as proven in a browser against the in-memory model, by its test's result line, or as not proven. No
score of a spec and no count is offered as the result.

THE APP: resident move-in at a senior living community, a vertical slice inside the conformance app (the owner:
"It must have its domain model with the fake in-memory data. It must have real endpoints with typed payload").
- Its ONLY spec is "consult2-answer.md" in `.worktrees/movein1`, ".landing/MoveInSlice1": part 1 the domain and its
  literal seed (one MoveInWorld a test; Harbor House and Maple Court; draft D100 at version 7; Helen Ward), 2 the
  endpoints, 3 the screens and partials, 4 the features MI.1 to MI.31 with their interactions, 5 the validators,
  6 FIVE SITUATIONS A FEATURE (a success with what the model holds after and what the next read shows, a refusal by
  the browser, a refusal by the server alone, a transport failure, two features meeting), 7 coverage, 8 gaps. A
  read-only Codex run derived it from the door map, the rule grammar, the schema and the TLA+ specs. "scenarios.md"
  and "spec.md" beside it are withdrawn. It lands under `spec/` with the app's first unit.
- In flight: a builder agent in `.worktrees/movein1` (base e9b49a1eb) builds THE FOUNDATION AND MI.1: the domain and
  seed whole, the world wiring, the main page with one empty element for each section of part 3, the draft's read
  and its save, and MI.1's five situations, each test green and failing on its planted defect. It was told to lay
  every shared file (the world's addresses, the store, the controller, the validators' registration) in files by
  area, so a later unit adds a file and edits none of the foundation's.
- When its "reply.md" stands: read MI.1 in the spec, read the change at the source, hold each line of "Done",
  write "read.md", land, push.
- Then the other thirty features, on the landed head, by two fresh builders side by side, each from a six-part
  brief that names its features by number and its files by area, each reply a batch of three or four features that
  lands before the next. A build is one at a time (the owner, 2026-11-17), so each build and browser run of a
  builder and of the lane is taken through one lock; the machine is what bounds the work, about a minute a run.
  First the features that carry what the owner named: rules under a condition, a condition inside an event and
  inside a request's reply, a chained and a parallel request, a callback, an event's payload, a live object walked
  by its members, a value of one plan read by another, a partial under a condition and an independent one.
  By area: care, resident, allocation, bed conflict and live availability (MI.2 to MI.4, MI.18, MI.20, MI.21, MI.28,
  MI.29); contacts, payer, services and price, deposit, diet (MI.5 to MI.9, MI.13 to MI.15, MI.25, MI.26);
  medications, documents, save, submit and discard, the coordinators' line, reception, request defaults (MI.10 to
  MI.12, MI.16, MI.17, MI.19, MI.22 to MI.24, MI.27, MI.30, MI.31).

THE LANE, small units finished beside the app, each landed by the command `scripts/state.py` prints for it:
- ".landing/BuildAScreen" in `.worktrees/skill`: the skill for every consumer. Its gate passed in its worktree.
- ".landing/TypedHome" in `.worktrees/typed-home`, moved to 176de4cd3: the fresh apps' home page has a model of its
  own. Its "Done" at that head: scripts/delivery.sh whole, read from ".work/logs/delivery-at-head.log" there.
- ".landing/TypedDoors" in `.worktrees/typed-doors`: ALIS044, a door of the framework takes no dynamic (the owner:
  "Yeah, write the analyzer"). Read at the source; "read.md" sends it back for one thing: sixteen of its eighteen
  planted defects ran on an earlier form of the file. When they have run on the file that lands: it moves to the
  head that holds TypedHome, scripts/delivery.sh runs whole there (the packed analyzer reads the fresh app), it lands.
- After each push: `python3 scripts/ci.py` about fifteen minutes later, and each failed job's log read. MVC 5 on
  net48 is proven by the job verify-net48 alone; the Syncfusion-upgrade probe's Playwright pin is a later unit.

Deferred, no process work goes ahead of the app: the decisions split by module (`spec/CLAUDE.md` is 2,000 lines; its
brief is written); root CLAUDE.md cut to its limit; this file cut to its limit.

Behind the app, the framework's open list: a list of files in a request form (D14.3; the runtime throws notBuilt); a
validation check on a text or a file body (refused where the view renders); the seven operators no C# writes leave
the grammar (the owner's yes, 2026-11-17), with the table "After 1.0" in spec/NOTES.md; the plan contract narrowed to
what the C# writes; an enum in a pushed message's member. Then unit 3's second piece (the Fusion template pages), and
Fusion's own: an equality of two members in a template, the grid's validation door and exports, SmartPaste, U27.
<!-- /next -->
