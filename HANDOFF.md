# Handoff

Written at `date`: Wed Nov 18 02:53:25 EST 2026

## State, as `scripts/state.py` reads it from git
unit: landed ../ci CiGreen
  next: git worktree remove ../ci
  leaves by: `git worktree remove {worktree}`
  goes to: gone
unit: landed ../ci Handoff
  next: git worktree remove ../ci
unit: ready ../ci KindsPlant
  next: scripts/land-unit.sh ../ci e9b49a1eb "spec: the kinds check's accepted plant edits the schema's paragraph as unit 3 left it" ".landing/KindsPlant"
  leaves by: `scripts/land-unit.sh {worktree} {base} "{title}" ".landing/{name}"`, while the lane is pushed
  exit line: "landed", with the browser run's result line
  goes to: unit: landed
unit: building ../movein1 MoveInSlice1: it lacks paths.txt or message.txt
  leaves by: the builder's work: the test red, the code, the test green, `scripts/plant-defect.sh`
  read first: the rule file of each kind of file, as it is opened
  exit line: each line of the brief's "Done"
  goes to: unit: ready, or unit: returned for an agent
unit: ready ../skill BuildAScreen
  next: scripts/land-unit.sh ../skill e9b49a1eb "skill: build-a-screen, a decision map from a developer's question to one door, its meaning, its ordering and a landed view" ".landing/BuildAScreen"
unit: returned ../typed-doors TypedDoors: reply.md waits for read.md
  leaves by: the lead writes "read.md"
  read first: the rule lines "reply.md" opens with, at their source; the change against its base
  exit line: "verdict: land"
  goes to: unit: ready
unit: ready ../typed-home TypedHome
  next: scripts/land-unit.sh ../typed-home e9b49a1eb "fresh apps and README: the home page has a model of its own, and no example opens a plan over dynamic" ".landing/TypedHome"
lane: pushed at ccae92cac on feature/rewrite: equal to the remote
  leaves by: the landing of a unit that is ready
  read first: the queue of `HANDOFF.md`
  exit line: "landed"
  goes to: lane: landed
ci: unread at ccae92cac
  leaves by: `python3 scripts/ci.py`
  exit line: "ci at" the commit, with each job
  goes to: ci: running, ci: failed or ci: passed
file: over CLAUDE.md: holds 325 lines, its limit 200
  leaves by: a unit that splits or cuts it; the mistakes are condensed, kinds merged, each line short enough to follow
  exit line: "landed"
  goes to: gone
file: over spec/CLAUDE.md: holds 2051 lines, its limit 150
file: over HANDOFF.md: holds 264 lines, its limit 150
file: over tests/Alis.Reactive.Conformance.App/CLAUDE.md: holds 197 lines, its limit 150
file: over tests/Alis.Reactive.Conformance.PlaywrightTests/CLAUDE.md: holds 234 lines, its limit 150

## Where it stands
- Branch: feature/rewrite
- HEAD: ccae92cac harness: the process as one state machine, the handoff in the repo, CI's causes fixed, and the kinds plant restated
- Full check: .work/logs/spec-check-ccae92cac.log: spec check at ccae92cac: 40 of 40 checks pass
- .work/last-run.txt: 20261118-024159-72751 ccae92cac

## Landed since the commit before the last spec: STATE commit
ccae92cac harness: the process as one state machine, the handoff in the repo, CI's causes fixed, and the kinds plant restated
09726c86f values (unit 3, piece 1): a shape kind enum for as, so a live object takes an enum the app's JSON names in the form it holds (the owner's yes, 2026-11-17)
e9b49a1eb spec: STATE and handoff at 949eefe47

## Open
open: 19 promises with no browser test, 7 runtime cases not built, 14 doors not implemented
feature promises: 533; named by a browser test: 514; with no browser test: 19
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
- Written from memory, not read at its source at HEAD: a design line, a signature, a path, a name, a page's expected value, a case's shape or a TLA+ ordering: 330 to 334
- A rule or a tool's list not applied before the change: 370 to 376
- A number, a proof line or an absence not read from the run that proves it: 51 to 53
- A shell command or a script wrong in its own terms: 519 to 529

## The owner's latest decisions
spec/CLAUDE.md:1926: - The owner, 2026-11-18, answering the lead's ask on what an element's text shows for an enum, unit 3 as built having
spec/CLAUDE.md:1936: - The owner, 2026-11-18, after the lead reported that the hook a compaction runs printed a file of the lead's private
spec/CLAUDE.md:1945: - The owner, 2026-11-18, of that machine (both verbatim in `spec/NOTES.md`): "Is there a mistake read part of the state
spec/CLAUDE.md:1956: - The owner, 2026-11-18, the same hour (verbatim in `spec/NOTES.md`): "What other steps are missing? How about quality
spec/CLAUDE.md:1966: - The owner, 2026-11-18, on the machine as built (verbatim in `spec/NOTES.md`): the mistakes are condensed past a
spec/CLAUDE.md:1979: - The owner, 2026-11-18, after a unit's delivery proof stopped before its browser step (verbatim in `spec/NOTES.md`):
spec/CLAUDE.md:1986: - The owner, 2026-11-18, on the machine as the lead had written it, one table with sentences around it and checks

## The owner's notes that are open: date | kind | home | follow-up | the owner's words
<!-- notes -->
None open.
<!-- /notes -->

## Next
<!-- next -->
The owner's direction, 2026-11-18, which orders everything below: "The framework is not getting new code except the
current module in progress." What the framework cannot express is a named gap in the app's spec, and the app is
written without it; nothing of it is put to the owner as a decision. Three separate things: the skill, the app's spec,
the app.

Landed, 2026-11-18: unit 3's first piece, the shape kind `enum` for `as`. A live object's members and a method's or a
plugin's arguments take an enum in the live form; an element's text and HTML, a joined text and a notice show the
name the app's JSON writes (the owner: "Yes, I agree with the enum.").

The owner is away for 24 hours from 2026-11-18 ("Hopefully you have the clarity to what to work on, and again we are
burning tokens at such high speed"). The lead works alone, the app first, and spends little: it acts on a
notification, never polls, and starts no agent beyond the three running until one returns.

WHAT THE OWNER GETS ON HIS RETURN, in his words: "I don't need acceptance numbers, sir. I need that complex app behavior
asserted and proved. I am done with hearing excuses or fake numbers." So: the move-in app running, each behavior of
its spec asserted in a browser on the page and in what the server stored, named as proven or as not proven. No
score of a spec and no count offered as the result; the coverage script is the lead's own check before building.

The app's spec: the owner's lists were examples ("I did not iterate complete set of features ... you have specs, you
have TLA+"). A read-only Codex run derives the complete spec in `.worktrees/movein1`, ".landing/MoveInSlice1"
("consult2-answer.md"); "coverage.py" beside it accepts it or names what goes back: every door name, rule door,
schema kind and TLA+ action named and in the coverage table, a row left out for a stated fact alone.

In flight, the lead's notes of 2026-11-18, each to its exit line:
- `.worktrees/ci`, base e9b49a1eb, holds two units, both ready, that land as one commit once the lane is pushed, by
  scripts/land-unit.sh with both folders. ".landing/CiGreen": CI's four causes, three fixed (the typecheck red then
  green in a clean copy; "[delivery] every delivery check passed."). ".landing/Handoff": the owner's four messages
  of 2026-11-18 on the process. It writes .claude/rules/process.md, one state machine whose table scripts/state.py
  reads as its data (a note of the owner's, a unit from its brief to its landing, an agent's hand-out and return,
  the lane, CI at the pushed commit, each file against its limit of lines), the tracked compaction hook, and the
  notes block of this file. After it lands: run scripts/state.py and act on what it prints; run scripts/ci.py
  about fifteen minutes after the push and read each failed job's log.
- The owner's notes of 2026-11-17 and 2026-11-18 that no commit has homed yet are entered in the notes block of this
  file the moment that block exists, each with its kind, its home and its follow-up: the requirement for a care
  screen and the real-domain app; the skill for every consumer; no decision put to him for the app; Native's
  components as the app's inputs; no new TypeScript component. Their words are whole in the lead's scratch folder
  until then, the last of this work outside the repo.
- Next unit, the owner's "it cannot be one bulky file": spec/CLAUDE.md is 2,000 lines, 274 decisions oldest first,
  and the harness shows a reader its first 1,394, so no decision since 2026-11-17 (line 1,849 on) reaches one. Its
  decisions move, word for word, into one file a module under spec/decisions, newest first; spec/CLAUDE.md keeps
  the module table, the files and the index. An agent builds it from a brief; a script proves each decision moved
  once. Readers to change with it: spec/tools/check.py (module_rows, orient, the two quoted citations) and
  scripts/handoff.py. Then root CLAUDE.md is cut to its limit.
- `.worktrees/skill`, base e9b49a1eb, holds the skill's unit, ready: ".landing/BuildAScreen" writes
  .claude/skills/build-a-screen/SKILL.md, twelve questions, each answered by one door, its meaning, its TLA+
  ordering and a landed view, with seven view examples copied by line range; and teaches WhenADocShowsAnExample and
  WhenARuleNamesTheCode to hold a skill's view examples (each new case failed on its planted defect; "Failed: 0,
  Passed: 55"). It lands after the harness unit, by the command scripts/state.py prints for it.
- `.worktrees/typed-home`, base e9b49a1eb, WAITING for CiGreen: ".landing/TypedHome". The owner, 2026-11-18: "why
  would you use dynamic? The whole point of the framework is tight". README's first example and both fresh apps'
  home views opened a plan over dynamic; each has a model of its own now. Its delivery proof is red at its base for
  a cause CiGreen fixes, so after the harness commit lands it moves to the new head and scripts/delivery.sh is run
  whole there, then it lands. Its browser step alone, run by hand: "1 selected, 1 ran, 1 passed".
- `.worktrees/typed-doors`, base e9b49a1eb, with a builder agent: ".landing/TypedDoors/brief.md", ALIS044, a door
  of the framework takes no dynamic (the owner, 2026-11-18: "Yeah, write the analyzer"). It lands after TypedHome.
  When its "reply.md" stands, the lead reads it at the source and writes "read.md" with its verdict.
- The owner, 2026-11-18: "the browser never ran because your process was wrong ... Get it into the process." The
  process unit now holds it: a brief's Done carries a line "At the base:", and a unit whose proof is red at its base
  for another unit's cause names it on a line "After:" and is read as waiting.
- THE APP, the owner's first concern (2026-11-18: "we haven't really got any work done since last 40 minutes. We're
  just working on the process"): resident move-in, 21 features MI.1 to MI.21 in six slices, a Codex consult's draft
  the lead showed the owner. `.worktrees/movein1`, base e9b49a1eb, with a builder agent: ".landing/MoveInSlice1"
  (brief.md, the consult's brief and answer). Slice 1 is MI.1 to MI.4 on one combined page: the residence cascade,
  the level of care with its confirm, the resident's nested and two-member rules, and the allocation review over the
  conformance component. The owner, the same hour: "you need to build fake data ... Sending request isn't enough. It
  has to send the correct payload so you can observe what is being returned, what is being changed ... you gotta
  build the domain model of the apps in memory", and the work is cut too small. So the unit builds the app's domain
  model first, in memory, seeded, one test world's own, shaped for all 21 features; every endpoint answers from it
  and none answers a constant; every test reads what the server stored and what changed. When its "reply.md"
  stands the lead reads it at the source and writes "read.md". The other 17 features follow on that model as two
  units built side by side, never five: the forms (contacts, payer, medications, services and price), and the
  lifecycle (save, submit, the bed conflict and waitlist, discard, live availability). No process work goes ahead.
- The decisions split has its brief written, six parts, for an agent; its worktree opens at the head the harness
  unit is pushed as.

In this order:
1. CI green on GitHub's machines, the MVC 5 job running its build. At 421d7227e, the last run that finished, these
   failed: checks (after npm audit), playwright fusion (35 of 86), delivery-net10 (one architecture test) and
   verify-net48 (msbuild not found). Each cause is read from its job's log and its fix is built in `.worktrees/ci`,
   landing next: the lock takes Node's types for the typecheck, the delivery test names both browser files of
   the net10 pack, the delivery job sets git's safe.directory, and the Windows job finds MSBuild by vswhere. A
   job's result is read from its own log, since the next push cancels a run.
2. The conformance component gains the two members it lacks, each a cell with its test and its planted defect: a
   method of two and of three parameters of mixed kinds, and an async method that takes an argument. No new
   TypeScript component is written (the owner, 2026-11-18).
3. The move-in app's spec, a file under `spec/`, written before the app: every feature in the domain's words with
   the doors, the schema's nodes and the TLA+ actions it uses; where the features are mixed on one screen, in one
   request and in one check; its coverage read against `spec/Core/schema.md` and the TLA+ specs as they stand, with
   no second inventory of them; and the gaps. The owner reviews it once.
4. The skill under `.claude/skills/`: general, for every consumer of the framework, a decision map from the schema
   and the TLA+ specs to the door by its one name. It holds no feature of the app.
5. The app, written with the skill from its spec: resident move-in at a senior living community, a new screen set of
   the conformance app, its inputs in request containers, Native's components its inputs, the conformance component
   its one custom component, no Fusion, every landed page and test kept. Six slices, each promise a browser test
   with its planted defect: where they live and level of care; contacts and payer; medications; services and
   pricing; save, submit and discard; live bed availability. The arrival desk (`Care/Admission/ArrivalDesk.cshtml`,
   M9.X6 to M9.X9) is four partial promises and is not the app's model.

The harness lines still open (the owner, 2026-11-17: "the whole harness has to be reviewed"):
- The handoff a compaction reads is this file. Today a hook this Mac alone holds prints a file outside the repo
  (`spec/tools/check.py`, its orient): the hook is tracked and prints this file.
- This file's list of unlanded work names every old worktree; each is landed or removed, so the list is what is open.
- The landing script reads a planted-defect line for each promise a unit adds.
- One agent brief form in the repo; one build at a time, a read-only consult beside it.
- The runtime's TypeScript and the C# core read against the rules by an independent inspector.

Behind the app, the framework's open list: a list of files in a request form (D14.3; the runtime throws notBuilt); a
validation check on a text or a file body (refused where the view renders); the seven operators no C# writes leave
the grammar (the owner's yes, 2026-11-17), with the table "After 1.0" in spec/NOTES.md; the plan contract narrowed to
what the C# writes; an enum in a pushed message's member. Then unit 3's second piece (the Fusion template pages), and
Fusion's own: an equality of two members in a template, the grid's validation door and exports, SmartPaste, U27.
<!-- /next -->
