# Handoff

Written at `date`: Thu Nov 19 02:28:35 EST 2026

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
unit: ready ../movein-contacts MoveInContacts
  next: scripts/land-unit.sh ../movein-contacts d13a4ce7f "The move-in page gains its contacts: MI.5 contact rows, MI.6 contact authority and MI.7 contact removal, on the foundation at fbea584b2." ".landing/MoveInContacts"
  leaves by: `scripts/land-unit.sh {worktree} {base} "{title}" ".landing/{name}"`, while the lane is pushed
  exit line: "landed", with the browser run's result line
  goes to: unit: landed
unit: building ../movein-owners MoveInOwners: it lacks paths.txt or message.txt
  leaves by: the builder's work: the test red, the code, the test green, `scripts/plant-defect.sh`
  read first: the rule file of each kind of file, as it is opened
  exit line: each line of the brief's "Done"
  goes to: unit: ready, or unit: returned for an agent
unit: waiting ../movein-scope MoveInSessionScope: for MoveInContacts
  leaves by: the unit it names lands; then it moves to the new head and each command of its "Done" runs there
  exit line: each line of the brief's "Done"
  goes to: unit: ready
unit: landed ../retire-picomatch-exception RetirePicomatchException
  next: git worktree remove ../retire-picomatch-exception
  leaves by: `git worktree remove {worktree}`
  goes to: gone
lane: pushed at a95511ce6 on feature/rewrite: equal to the remote
  leaves by: the landing of a unit that is ready
  read first: the queue of `HANDOFF.md`
  exit line: "landed"
  goes to: lane: landed
ci: unread at a95511ce6
  leaves by: `python3 scripts/ci.py`
  exit line: "ci at" the commit, with each job
  goes to: ci: running, ci: failed or ci: passed
file: over CLAUDE.md: holds 326 lines, its limit 200
  leaves by: a unit that splits or cuts it; the mistakes are condensed, kinds merged, each line short enough to follow
  exit line: "landed"
  goes to: gone
file: over spec/CLAUDE.md: holds 2074 lines, its limit 150
file: over HANDOFF.md: holds 290 lines, its limit 150
file: over tests/Alis.Reactive.Conformance.App/CLAUDE.md: holds 201 lines, its limit 150
file: over tests/Alis.Reactive.Conformance.PlaywrightTests/CLAUDE.md: holds 234 lines, its limit 150

## Where it stands
- Branch: feature/rewrite
- HEAD: a95511ce6 build: retire the withdrawn picomatch audit exception
- Full check: .work/logs/spec-check-a95511ce6.log: spec check at a95511ce6: 40 of 40 checks pass
- .work/last-run.txt: 20261119-021729-16074 a95511ce6

## Landed since the commit before the last spec: STATE commit
a95511ce6 build: retire the withdrawn picomatch audit exception
d13a4ce7f spec: STATE and handoff at e02fafe66

## Open
open: 19 promises with no browser test, 7 runtime cases not built, 14 doors not implemented
feature promises: 552; named by a browser test: 533; with no browser test: 19
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
The owner, 2026-11-19: "you can not change framework now while working in conformance." At startup, read the
move-in unit's brief and `spec/Reference/move-in.md`. Build and prove the app through the typed doors already on
the branch, including the supported MVC `Html.*` partial helpers. If a door falls short, name the use case in
part 9 of the move-in spec and raise a separate scope decision before any framework edit. The matching startup
rule is prepared in `../movein-scope` for landing after contacts.
Each builder on the latest Astra at xhigh reads `AGENTS.md`, `.claude/rules/process.md`, the owner decisions in
`spec/CLAUDE.md`, `spec/MISTAKES.md`, and the applicable project and file rules before editing. Its reply cites
the rules it applied by file and line. The lead verifies the source and browser and planted evidence against
each promise, and owns the integrated gate. The owner, 2026-11-19: "its not check and rubber stamp it- quality
bar has to be high."

Contacts integration passed the build, page-doors comparison and all move-in browser scenarios on November 19
(see the contacts landing folder's `note.txt`). The preview stopped on schedule. The contacts landing gate stopped
at the npm audit, with the batch staged and no contacts commit. The owner had excused GHSA-fc5q-39xc-vjpf in
`spec/CLAUDE.md`, November 17. The exception expires when npm stops reporting the advisory. GitHub withdrew it on
November 18 and a live audit confirmed zero findings with picomatch still pinned to 4.0.7. The gate log is
`.work/logs/land-unit-d13a4ce7f-14028.log`. The owner authorized retiring the exception. The separate audit
unit landed at `a95511ce6`. The live audit reported no advisories, the three focused cases passed, the full
browser run printed 651 passed with no failures or over-budget tests, and the committed spec check passed
40 of 40. Retry contacts from `../movein-contacts` through the landing script. Its previous staged tree is
verified at `.work/movein-contacts-before-audit-yv_ws7bs/contacts.patch`. Land the startup scope unit after
contacts.

RESUME UPDATE, 2026-11-19: The owner asked Codex to resume from this handoff. Read the root `CLAUDE.md`/`AGENTS.md`,
all repo `CLAUDE.md` files and the six `.claude/rules/` files before acting. The rewrite branch is at
`d13a4ce7f`; remote matched that head at the last read. Preserve the existing modified `HANDOFF.md` until this
resume note is committed through the prescribed landing path. No implementation or tests were changed by the status
review.

The owner reports Rider's toolset is now fixed. The previous Rider MCP build (before that correction) selected Mono
MSBuild 15.0, rejected `/langversion:14.0`, and misread `net10.0` as `.NETFramework,Version=v10.0`. SDK 11.0.100
is pinned in `global.json` and installed at `/Users/muhammadadnanrafiq/.local/share/alis-toolchain/dotnet11/dotnet`.
A new Rider MCP build request reported that another build is already running; no result from that run is available
yet. Read its final Rider MCP build state before treating the IDE build as verified. The UI connection previously
failed with ScreenCaptureKit error -3811.
A fresh Rider MCP session on November 19 completed successfully with zero reported problems. The CLI
Release browser smoke test also passed from the canonical `Documents/Code` worktree path.

Resume the app in order. Contacts MI.5–MI.7 is preserved in `../movein-contacts` and `.landing/MoveInContacts`.
Its earlier isolated report was 15 tests green and 15 planted defects caught, but integration on the current head
failed: `MoveInDesk.Today()` and `MoveInReads.Day(DateTime)` duplicate members already supplied by MI.2. The
page-doors comparison did not run; `read.md`'s land verdict is premature. Read the merged files at source, resolve
the duplicates, build, compare page-doors, run the unit and full browser/gate checks, then land and push only through
the prescribed scripts. `../movein-owners` has the MI.18/28/29 brief but no implementation; start a fresh scoped
builder for it after contacts lands. Continue the app queue recorded below. Saved results (not rerun here): 651
browser tests and 40/40 spec checks passed for the landed MI.2–MI.4 batch. CI core passed 499/499 on d13a4ce7f;
overall CI failed net48 browser verification and cancelled the spec job. The pinned Java 27.0.1 is not available
in the current shell, so TLC readiness must be checked before a spec run.

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
`tests/Alis.Reactive.Conformance.App/Care/MoveIn/`. Its only spec is `spec/Reference/move-in.md` (landed
cff7dd154); a unit adds to its ninth part each place the source won over it and each gap it met. 31 features MI.1 to MI.31, five situations a feature, each situation one browser test that reads the
page, what the model holds after and what the next read shows.

Proven, each by a test green in a browser when run alone and caught on its planted defect:
- The foundation (f4cd5af46): the domain model and its seed, one test world's own; the main page with one section an
  area; the draft's read and its save by PUT.
- MI.1, residence selection, its five situations (f4cd5af46): each list offers what the choices before it hold and
  a choice alone saves nothing; an empty community is refused beside its list and sends no lookup; a building the
  world holds closed is refused by the server with its reason; a lookup whose connection drops says so and offers
  nothing it did not load; a lookup a newer choice replaced loads nothing, and the draft moves only at its save.
- MI.2, care-specific admission, its five (e02fafe66, "651 selected, 651 ran, 651 passed", "40 of 40 checks pass"):
  a confirmed level loads its own questions and a save keeps that level alone; a declined change keeps the answers
  on the page; a level the server refuses says why and leaves the saved care; questions whose connection drops say
  so and the saved answers come back at the next read; questions a newer level replaced never join the page.
- MI.3, resident details, four of its five: a verified identity names who checked it and its receipt opens only
  where the server asks; an email confirmation that differs is refused in the browser and no review leaves; a
  resident who already holds an admission is refused by the server with its reason; an identity review whose
  connection drops says so and leaves no review. Its fifth situation waits on MI.9's drawer.
- MI.4, allocation review, its five: a review is posted once readiness answers and its reply fills the panel and
  the instructions; a panel that is not ready asks the server for nothing; a bed taken since it was listed shows who
  holds it and no review is kept; a readiness call that never answers ends the review and posts nothing; a review a
  newer selection replaced posts nothing when its readiness answers.
Inside those, not proven: MI.2.2's condition on contact roles (waits on the contacts); MI.4.3's entry to the
waitlist (MI.18) and MI.4's fourth situation's quote (MI.13 to MI.15); that a dropped POST is not sent again, which
M9.X23 asserts and which failed on no planted defect; each endpoint's other refusals, listed in each unit's message.
The harness's waits were measured again on 2026-11-18 with the move-in page in place: a navigation 704 ms (it was
204 ms, and no move-in test ran alone under it), a step 740 ms, a test 7.44 s.
Not proven: every feature from MI.5 on. Two gaps of the framework the foundation met, no code written for either: a Native
input over a date renders the server's culture text, which the browser's code does not read as a date (Native holds
no date input); an input over an enum whose zero no name holds. Three members a builder added to the conformance
component with no test are held out as a patch in the lead's durable scratch, "conformance-members", for the unit
that brings each with its cell and its test.

In flight, two builder agents, each in a worktree with a six-part brief and the foundation's reply beside it:
- `.worktrees/movein-contacts` at fbea584b2, ".landing/MoveInContacts": MI.5 contact rows, MI.6 contact authority,
  MI.7 contact removal; promise ids M9.X40 to M9.X64. The new "measured.json" is copied into its worktree and stays
  out of its paths.
- `.worktrees/movein-owners` at d13a4ce7f, ".landing/MoveInOwners": MI.18 bed conflict and waitlist, MI.28
  assessment objects and their owners, MI.29 room handoff callback; promise ids M9.X65 to M9.X89.
When a "reply.md" stands: read the feature in the spec, read the tests and the views at the source, hold each line
of "Done", write "read.md", land through the lock, push, remove the worktree; then open the next unit for that
builder's line from the new head with a fresh agent, by the brief template and the script in the lead's scratchpad
folder "fanout" (a copy of both is kept in the durable scratch). The lines, in the order the owner's concerns give:
- after MoveInOwners: MI.20 and MI.21, bed availability by server push and its reconnect;
- after MoveInContacts: MI.8 primary and secondary payer, MI.9 independent payer verification, MI.13 services and
  price calculation; then MI.14 service review component, MI.15 price acceptance, MI.25 deposit, MI.26 diet;
- then MI.10 to MI.12 medications and pharmacy, MI.16 save draft, MI.17 submit admission, MI.19 discard, MI.22 to
  MI.24 documents and the packet, MI.27 the coordinators' line, MI.30 reception's native form, MI.31 request defaults.
Promise ids are given in blocks of 25 from M9.X90 on, so two units never take one id. A new unit is opened by
"open-unit.sh" of the lead's durable scratch folder "fanout", which fills "brief-template.md" from the lane's head.

THE LANE, nothing in flight. CI at 4b69b305d, each failed job read from its own log
(`gh api repos/marafiq/alis-reactive/actions/jobs/<id>/logs`):
- checks and the Syncfusion upgrade pass (cff7dd154).
- playwright core: one test failed of 485 at 4b69b305d, MI.1's closed building, "Timeout 204ms exceeded" at its
  navigation. The waits measured again landed in e02fafe66; the job is read again at that push.
- verify-net48: Visual Studio's MSBuild builds the fresh MVC 5 app with no warning and its 12 consumer tests pass
  (2836ae7e4, cff7dd154); Chromium installs at the pin (7b552b3fb). Its browser run under the twin then fails each of
  its 11 tests: "The conformance app serves no host 'net48'". The harness starts the net10 app under that name;
  `tests/Alis.Reactive.Conformance.PlaywrightTests/Harness/ConformanceHost.cs` holds no start of the twin under IIS
  Express. That start is the rewrite's own net48 work, a unit of its plan and no CI fix: MVC 5 in a browser stays
  unproven until it is built. Also there: "ps: unknown option -- o", the runner's sweep for a server left alive,
  which reads nothing under Git's bash.
- A landed unit whose file the landing merged reads as "ready" and never "landed", since its worktree's file no
  longer equals the head's: it is removed by hand once each of its paths is found in its commit. A defect of the
  guard paths-equal-head, for the machine's next review.

Landed 2026-11-18 beside the app: the process as a state machine and this file in the repo; CI's first three causes;
the enum shape kind; the skill `.claude/skills/build-a-screen/SKILL.md`; the fresh apps' typed home page; ALIS044, a
door of the framework takes no dynamic, with the fresh net10 app built under the packed rule; CI's second and
third fixes; the app's spec, `spec/Reference/move-in.md`.

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
