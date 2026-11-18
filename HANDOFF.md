# Handoff

Written at `date`: Wed Nov 18 05:43:16 EST 2026

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
unit: landed ../ci3 CiGreen3
  next: git worktree remove ../ci3
  leaves by: `git worktree remove {worktree}`
  goes to: gone
unit: building ../movein-care MoveInCare: it lacks paths.txt or message.txt
  leaves by: the builder's work: the test red, the code, the test green, `scripts/plant-defect.sh`
  read first: the rule file of each kind of file, as it is opened
  exit line: each line of the brief's "Done"
  goes to: unit: ready, or unit: returned for an agent
unit: building ../movein-contacts MoveInContacts: it lacks paths.txt or message.txt
lane: pushed at 7b552b3fb on feature/rewrite: equal to the remote
  leaves by: the landing of a unit that is ready
  read first: the queue of `HANDOFF.md`
  exit line: "landed"
  goes to: lane: landed
ci: unread at 7b552b3fb
  leaves by: `python3 scripts/ci.py`
  exit line: "ci at" the commit, with each job
  goes to: ci: running, ci: failed or ci: passed
file: over CLAUDE.md: holds 326 lines, its limit 200
  leaves by: a unit that splits or cuts it; the mistakes are condensed, kinds merged, each line short enough to follow
  exit line: "landed"
  goes to: gone
file: over spec/CLAUDE.md: holds 2074 lines, its limit 150
file: over HANDOFF.md: holds 220 lines, its limit 150
file: over tests/Alis.Reactive.Conformance.App/CLAUDE.md: holds 201 lines, its limit 150
file: over tests/Alis.Reactive.Conformance.PlaywrightTests/CLAUDE.md: holds 234 lines, its limit 150

## Where it stands
- Branch: feature/rewrite
- HEAD: 7b552b3fb ci: the Windows job installs Chromium at the pin before its browser tests
- Full check: .work/logs/spec-check-7b552b3fb.log: spec check at 7b552b3fb: 40 of 40 checks pass
- .work/last-run.txt: 20261118-053247-59614 7b552b3fb

## Landed since the commit before the last spec: STATE commit
7b552b3fb ci: the Windows job installs Chromium at the pin before its browser tests
077ff795c spec: STATE and handoff at cff7dd154

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
None.

## The owner's latest decisions
spec/CLAUDE.md:1927: - The owner, 2026-11-18, answering the lead's ask on what an element's text shows for an enum, unit 3 as built having
spec/CLAUDE.md:1937: - The owner, 2026-11-18, after the lead reported that the hook a compaction runs printed a file of the lead's private
spec/CLAUDE.md:1946: - The owner, 2026-11-18, of that machine (both verbatim in `spec/NOTES.md`): "Is there a mistake read part of the state
spec/CLAUDE.md:1957: - The owner, 2026-11-18, the same hour (verbatim in `spec/NOTES.md`): "What other steps are missing? How about quality
spec/CLAUDE.md:1967: - The owner, 2026-11-18, on the machine as built (verbatim in `spec/NOTES.md`): the mistakes are condensed past a
spec/CLAUDE.md:1980: - The owner, 2026-11-18, after a unit's delivery proof stopped before its browser step (verbatim in `spec/NOTES.md`):
spec/CLAUDE.md:1987: - The owner, 2026-11-18, on the machine as the lead had written it, one table with sentences around it and checks
spec/CLAUDE.md:1994: - The owner, 2026-11-18 (verbatim in `spec/NOTES.md`): "I want you to build a skill out of that TLA plus and schema,
spec/CLAUDE.md:2001: - The owner, 2026-11-18, on README's first example and both fresh apps' home views (verbatim in `spec/NOTES.md`):
spec/CLAUDE.md:2007: - The owner, 2026-11-18, on the app that proves the framework as an app uses it (verbatim in `spec/NOTES.md`): "you

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
and spends little: it acts on a notification, never polls, and adds no process.

WHAT THE OWNER GETS ON HIS RETURN, in his words: "I don't need acceptance numbers, sir. I need that complex app behavior
asserted and proved. I am done with hearing excuses or fake numbers." So the report names each behavior of the
move-in app as proven in a browser against the in-memory model, by its test's result line, or as not proven. No
score of a spec and no count is offered as the result.

ONE BUILD AT A TIME on this Mac (the owner, 2026-11-17). Every build and browser run of a builder and of the lane is
started through one lock, `/usr/bin/lockf -k "$HOME/.local/share/alis-toolchain/build.lock" <the command>`; the lane's
chains under ".work" of this worktree are started so. The machine bounds the work, about a minute a run.

THE APP: resident move-in at a senior living community, a vertical slice inside the conformance app, under
`tests/Alis.Reactive.Conformance.App/Care/MoveIn/`. Its only spec is `spec/Reference/move-in.md` once the unit
MoveInSpec lands (it is in `.worktrees/ci2`, landing with the CI fix); until then each builder holds a copy beside
its brief. 31 features MI.1 to MI.31, five situations a feature, each situation one browser test that reads the
page, what the model holds after and what the next read shows.

Proven, each by a test green in a browser and caught on its planted defect (landed f4cd5af46, "637 selected, 637
ran, 637 passed", "40 of 40 checks pass"):
- The foundation: the domain model and its seed, one test world's own; the main page with one section an area; the
  draft's read and its save by PUT.
- MI.1, residence selection, its five situations: each list offers what the choices before it hold and a choice
  alone saves nothing; an empty community is refused beside its list and sends no lookup; a building the world holds
  closed is refused by the server with its reason; a lookup whose connection drops says so and offers nothing it did
  not load; a lookup a newer choice replaced loads nothing, and the draft moves only at its save.
Not proven: every other feature. Two gaps of the framework the foundation met, no code written for either: a Native
input over a date renders the server's culture text, which the browser's code does not read as a date (Native holds
no date input); an input over an enum whose zero no name holds. Three members a builder added to the conformance
component with no test are held out as a patch in the lead's durable scratch, "conformance-members", for the unit
that brings each with its cell and its test.

In flight, two builder agents, each in a worktree at fbea584b2 with a six-part brief, the spec and the foundation's
reply beside it, each replying when its three features stand:
- `.worktrees/movein-care`, ".landing/MoveInCare": MI.2 care-specific admission, MI.3 resident details, MI.4
  allocation review; promise ids M9.X15 to M9.X39.
- `.worktrees/movein-contacts`, ".landing/MoveInContacts": MI.5 contact rows, MI.6 contact authority, MI.7 contact
  removal; promise ids M9.X40 to M9.X64.
When a "reply.md" stands: read the feature in the spec, read the tests and the views at the source, hold each line
of "Done", write "read.md", land through the lock, push, remove the worktree; then open the next unit for that
builder's line from the new head with a fresh agent, by the brief template and the script in the lead's scratchpad
folder "fanout" (a copy of both is kept in the durable scratch). The lines, in the order the owner's concerns give:
- after MoveInCare: MI.18 bed conflict and waitlist, MI.28 assessment objects and their owners, MI.29 room handoff
  callback; then MI.20 and MI.21, bed availability by server push and its reconnect;
- after MoveInContacts: MI.8 primary and secondary payer, MI.9 independent payer verification, MI.13 services and
  price calculation; then MI.14 service review component, MI.15 price acceptance, MI.25 deposit, MI.26 diet;
- then MI.10 to MI.12 medications and pharmacy, MI.16 save draft, MI.17 submit admission, MI.19 discard, MI.22 to
  MI.24 documents and the packet, MI.27 the coordinators' line, MI.30 reception's native form, MI.31 request defaults.
Promise ids are given in blocks of 25 from M9.X65 on, so two units never take one id.

THE LANE:
- `.worktrees/ci2`, two units landing as one commit by ".work/chain-ci2.sh": ".landing/CiGreen2", CI's three causes
  at 5ed0e0a5e (a zone test that read the machine's zone files; the Windows job's msbuild under bash; the Syncfusion
  probes on the POC's Playwright pin), and ".landing/MoveInSpec", the app's spec as a file of the repo.
- After its push: `python3 scripts/ci.py` about fifteen minutes later, and each failed job's log read by
  `gh api repos/marafiq/alis-reactive/actions/jobs/<id>/logs`, since a run's logs wait for the whole run to end. MVC 5
  on net48 is proven by the job verify-net48 alone.
- A landed unit whose file the landing merged reads as "ready" and never "landed", since its worktree's file no
  longer equals the head's: it is removed by hand once each of its paths is found in its commit. A defect of the
  guard paths-equal-head, for the machine's next review.

Landed 2026-11-18 beside the app: the process as a state machine and this file in the repo; CI's first three causes;
the enum shape kind; the skill `.claude/skills/build-a-screen/SKILL.md`; the fresh apps' typed home page; ALIS044, a
door of the framework takes no dynamic, with the fresh net10 app built under the packed rule.

Deferred, no process work goes ahead of the app: the decisions split by module (`spec/CLAUDE.md` is over 2,000 lines;
its brief is written); root CLAUDE.md cut to its limit; this file cut to its limit; the conformance component's
three members with their cells.

Behind the app, the framework's open list: a Native date input and an input over a date (the app's gap 1); a list
of files in a request form (D14.3; the runtime throws notBuilt); a validation check on a text or a file body (refused
where the view renders); the seven operators no C# writes leave the grammar (the owner's yes, 2026-11-17), with the
table "After 1.0" in spec/NOTES.md; the plan contract narrowed to what the C# writes; an enum in a pushed message's
member. Then unit 3's second piece (the Fusion template pages), and Fusion's own: an equality of two members in a
template, the grid's validation door and exports, SmartPaste, U27.
<!-- /next -->
