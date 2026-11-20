# Handoff

Written at `date`: Fri Nov 20 02:23:52 EST 2026

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
note: open 2026-11-19 | direction | the queue of this file | queue | "please do not do patch fix, do proper root cause 
note: open 2026-11-19 | direction | the queue of this file | queue | "Produce the Fusion audit first." Then "we should 
note: open 2026-11-19 | quality rule | spec/FusionComponents/module.md and .claude/skills/onboard-component/SKILL.md | 
note: open 2026-11-19 | direction | the queue of this file | queue | "how about implementing server side search in comb
note: open 2026-11-19 | direction | the queue of this file | queue | "i would be curious to add more combox where we us
note: open 2026-11-19 | direction | the queue of this file | queue | "showspinner of compobox and hide when it succed" 
note: open 2026-11-19 | direction | the queue of this file | queue | "use this as refereces to the skill so it works" a
note: open 2026-11-19 | direction | the queue of this file | queue | "lets onboard datamanager only for combobox and le
unit: ready ../fusion-audit FusionAudit
  next: scripts/land-unit.sh ../fusion-audit 9c77fd7d8 "Fusion audit: compare the pinned vendor contract and improve component onboarding" ".landing/FusionAudit"
  leaves by: `scripts/land-unit.sh {worktree} {base} "{title}" ".landing/{name}"`, while the lane is pushed
  exit line: "landed", with the browser run's result line
  goes to: unit: landed
unit: returned ../fusion-combobox-data-manager FusionComboBoxDataManager: reply.md waits for read.md
  leaves by: the lead writes "read.md"
  read first: the rule lines "reply.md" opens with, at their source; the change against its base
  exit line: "verdict: land"
  goes to: unit: ready
unit: returned ../fusion-combobox-equipment-availability FusionComboBoxEquipmentAvailability: reply.md waits for read.md
unit: ready ../fusion-combobox-follow-through FusionComboBoxFollowThrough
  next: scripts/land-unit.sh ../fusion-combobox-follow-through 5f77ce2a6 "Prove ComboBox search, live configuration and real EJ2 Promises" ".landing/FusionComboBoxFollowThrough"
unit: unbriefed ../fusion-combobox-follow-through IntegrationPrep: brief.md lacks Scope, Rules, Outcome, Done, Commands, Reply
  leaves by: the brief is completed, before any work
  read first: the files of a unit, below
  goes to: unit: briefed
unit: returned ../fusion-combobox-live-catalogue FusionComboBoxLiveCatalogue: reply.md waits for read.md
unit: ready ../fusion-combobox-onboarding-notes FusionOnboardingNotes
  next: scripts/land-unit.sh ../fusion-combobox-onboarding-notes d67e15d16 "Resolve pinned ComboBox citations and clarify DataManager onboarding" ".landing/FusionOnboardingNotes"
unit: returned ../fusion-combobox-onboarding-replay FusionComboBoxOnboardingReplay: reply.md waits for read.md
unit: ready ../fusion-combobox-payload FusionComboBoxPayload
  next: scripts/land-unit.sh ../fusion-combobox-payload 9c77fd7d8 "Prove typed ComboBox changes on Helen Martinez's communication plan" ".landing/FusionComboBoxPayload"
unit: returned ../fusion-combobox-server-search FusionComboBoxServerSearch: reply.md waits for read.md
unit: ready ../movein-availability MoveInAvailability
  next: scripts/land-unit.sh ../movein-availability 6a418dc64 "MI.18.5, MI.20 and MI.21. Publish the world's bed changes to independently owned page and card streams. A held response checks current community access and preserves a newer displayed revision and holder. Reconnect re-reads the bed; the retry offer remains until every failed owner recovers or leaves." ".landing/MoveInAvailability"
unit: ready ../movein-contacts MoveInContacts
  next: scripts/land-unit.sh ../movein-contacts d13a4ce7f "The move-in page gains its contacts: MI.5 contact rows, MI.6 contact authority and MI.7 contact removal, on the foundation at fbea584b2." ".landing/MoveInContacts"
unit: waiting ../movein-coordinators MoveInCoordinators: for MoveInDocuments
  leaves by: the unit it names lands; then it moves to the new head and each command of its "Done" runs there
  exit line: each line of the brief's "Done"
  goes to: unit: ready
unit: ready ../movein-deposit-diet MoveInDepositDiet
  next: scripts/land-unit.sh ../movein-deposit-diet a46b1ca90 "MI.25 admission deposits and MI.26 dietary plans" ".landing/MoveInDepositDiet"
unit: waiting ../movein-discard MoveInDiscard: for MoveInNavigationReadiness
unit: waiting ../movein-documents MoveInDocuments: for MoveInDiscard
unit: ready ../movein-draft-admission MoveInDraftAdmission
  next: scripts/land-unit.sh ../movein-draft-admission fa45060db "Prove move-in draft saving and admission submission" ".landing/MoveInDraftAdmission"
unit: ready ../movein-medications-pharmacy MoveInMedicationsPharmacy
  next: scripts/land-unit.sh ../movein-medications-pharmacy c2ec598d3 "Prove medication editing, pharmacy clearance and held validation in move-in" ".landing/MoveInMedicationsPharmacy"
unit: waiting ../movein-navigation-readiness MoveInNavigationReadiness: for Net48TwinFollowThrough
unit: building ../movein-owners MoveInOwners: it lacks paths.txt or message.txt
  leaves by: the builder's work: the test red, the code, the test green, `scripts/plant-defect.sh`
  read first: the rule file of each kind of file, as it is opened
  exit line: each line of the brief's "Done"
  goes to: unit: ready, or unit: returned for an agent
unit: ready ../movein-owners-restarted MoveInOwners
  next: scripts/land-unit.sh ../movein-owners-restarted 21d6a2ec4 "Implement MI.18 waitlisting and MI.28–29 assessment and room owners" ".landing/MoveInOwners"
unit: ready ../movein-payer-pricing MoveInPayerPricing
  next: scripts/land-unit.sh ../movein-payer-pricing 940dda15b "MI.8, MI.9 and MI.13 — payer coverage, verification and quotes" ".landing/MoveInPayerPricing"
unit: landed ../movein-reception MoveInReception
  next: git worktree remove ../movein-reception
  leaves by: `git worktree remove {worktree}`
  goes to: gone
unit: ready ../movein-review-acceptance MoveInReviewAcceptance
  next: scripts/land-unit.sh ../movein-review-acceptance c91bae1fc "MI.14 and MI.15. Review the selected services and accept the displayed quote revision" ".landing/MoveInReviewAcceptance"
unit: ready ../movein-scope MoveInSessionScope
  next: scripts/land-unit.sh ../movein-scope d13a4ce7f "Keep move-in implementation and builder proof within scope" ".landing/MoveInSessionScope"
unit: waiting ../net48-browser-host Net48BrowserHost: for MoveInDraftAdmission
unit: waiting ../net48-twin-followthrough Net48TwinFollowThrough: for Net48BrowserHost
unit: ready ../retire-picomatch-exception RetirePicomatchException
  next: scripts/land-unit.sh ../retire-picomatch-exception d13a4ce7f "GitHub withdrew GHSA-fc5q-39xc-vjpf on November 18. npm no longer reports it, so retire its exception under rewrite section 2.7. Picomatch stays pinned at 4.0.7. The audit refuses reported advisories by ID, including the retired ID if it returns." ".landing/RetirePicomatchException"
lane: checking at 1c09bcf83 on feature/rewrite
  leaves by: the last line `scripts/full-check.sh` prints
  exit line: "spec check at" its commit
  goes to: lane: checked
ci: unread at 1c09bcf83
  leaves by: `python3 scripts/ci.py`
  exit line: "ci at" the commit, with each job
  goes to: ci: running, ci: failed or ci: passed
file: over CLAUDE.md: holds 336 lines, its limit 200
  leaves by: a unit that splits or cuts it; the mistakes are condensed, kinds merged, each line short enough to follow
  exit line: "landed"
  goes to: gone
file: over spec/CLAUDE.md: holds 2077 lines, its limit 150
file: over HANDOFF.md: holds 706 lines, its limit 150
file: over tests/Alis.Reactive.Conformance.App/CLAUDE.md: holds 201 lines, its limit 150
file: over tests/Alis.Reactive.Conformance.PlaywrightTests/CLAUDE.md: holds 237 lines, its limit 150
machine: review due : 28 units have landed since .claude/rules/process.md last changed
  leaves by: a unit that changes this file, the lead's without waiting for the owner, the framework's quality bar never lowered
  read first: the units sent back, the kinds of mistake that grew, the states no unit met
  exit line: "landed"
  goes to: gone

## Where it stands
- Branch: feature/rewrite
- HEAD: 1c09bcf83 Prove native and reactive reception sign-in (MI.30)
- Full check: .work/logs/spec-check-1c09bcf83.log: spec check at 1c09bcf83: 40 of 40 checks pass
- .work/last-run.txt: 20261120-021105-78367 1c09bcf83

## Landed since the commit before the last spec: STATE commit
1c09bcf83 Prove native and reactive reception sign-in (MI.30)
06b369941 spec: STATE and handoff at 0cb3d7355

## Open
open: 19 promises with no browser test, 7 runtime cases not built, 14 doors not implemented
feature promises: 701; named by a browser test: 682; with no browser test: 19
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
- Written from memory, not read at its source at HEAD: a design line, a signature, a path, a name, a page's expected value, a case's shape or a TLA+ ordering: 422 to 427
- A rule or a tool's list not applied before the change: 428 to 430
- A shell command or a script wrong in its own terms: 596 to 597

## The owner's latest decisions
spec/CLAUDE.md:1783: - The owner, 2026-11-19: "well fix net48 as well so we gain high confidence". Windows CI runs the written MVC 5

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
- 2026-11-19 | direction | the queue of this file | queue | "please do not do patch fix, do proper root cause fix with proof, this is super important quality bar" for Fusion member and payload onboarding.
- 2026-11-19 | direction | the queue of this file | queue | "Produce the Fusion audit first." Then "we should be doing audit, and fixing skill, and checks, so we do not re-work like crazy all the time."
- 2026-11-19 | quality rule | spec/FusionComponents/module.md and .claude/skills/onboard-component/SKILL.md | FusionAudit | "each component is vertical slice, and we do not share even event classes, if its complex, it uses nested classes."
- 2026-11-19 | direction | the queue of this file | queue | "how about implementing server side search in combobox as 5th one" and "use multiple events on one comobobox as well - infact all complex". The server-search unit follows the four-choice payload unit.
- 2026-11-19 | direction | the queue of this file | queue | "i would be curious to add more combox where we use some of its useful meethods" and "some props as well, especially complex ones.." and "use diff comoboxes so u do not get confused or future readers". Keep the transport catalogue properties and methods, interpreter search, and real vendor Promise examples distinct.
- 2026-11-19 | direction | the queue of this file | queue | "showspinner of compobox and hide when it succed" and "comobobox has methods showspinner and hidespinner". The interpreter-search fixture holds a request and checks the actual ComboBox spinner.
- 2026-11-19 | direction | the queue of this file | queue | "use this as refereces to the skill so it works" and "and then test the skill actually" and "yeah lets make skill perform with very high confidence so you can onboard other components". The replay checks the pinned ComboBox and held-out NumericTextBox.
- 2026-11-19 | direction | the queue of this file | queue | "lets onboard datamanager only for combobox and leave it for the rest, but have it in skill so if i ask onboard for that component". The direct, component-local DataManager example builds in ../fusion-combobox-data-manager.
2026-11-19 | direction | spec/CLAUDE.md | Net48BrowserHost | "well fix net48 as well so we gain high confidence"
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
40 of 40. Contacts resumed from the preserved worktree and landed at `240edd3be`. The full browser run
printed 666 passed, 0 failed and 0 over budget. The comparison preserved all 651 previous passes and added
15 for contact rows, contact authority and removal. The full spec check passed 40 of 40. MI.16 still owns the
shared Save draft command, and MI.2.2's care eligibility condition remains open. The startup scope rule is ready
in `../movein-scope`, and the owner asked for the Fusion audit before the skill and check changes.

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

The November 19 Fusion audit found that EJ2 33.1.47 ComboBox inherits the live `dataSource` property from
`DropDownBase`. The rewrite's two public API surfaces omit a typed live data-source door for ComboBox, and
M21.N13 covers the initial Razor builder and other live members. ComboBox's `Change` exposes `Value` and
`IsInteracted`. The pinned code also emits `itemData` and `previousItemData`, which its types describe as
`FieldSettingsModel`. Fusion's project references the analyzers and does not import `VendorContract.targets`.
`RootMemberDoor` skips those checks when the table is absent. The Fusion audit behind the app compares each
inherited public member and each payload recursively against the pinned API, using live evidence where the type is
ambiguous. Schedule's typed popup data and MultiSelect's typed tagging data are existing examples.
The Fusion audit and onboarding skill landed at `88bab362a`. Its full browser run passed all 666 tests and
preserved every prior pass. The audit comparison under `spec/FusionComponents/audit` replays the baseline and
compares changed declarations with both Fusion host manifests. The existing vendor contract extractor still
needs its synthesized-symbol failure addressed before a Fusion build can consume its table.

The four-value preview ran in Chromium and ended on schedule. The payload unit landed at `5f77ce2a6`
after eight caught defects and a byte-identical compiled page-doors comparison. The integrated run
passed all 672 tests, with 6 new passes and all 666 prior passes preserved. The full spec check passed
40 of 40, and the push reported `5f77ce2a6` and `21d6a2ec4` with remote equal to HEAD.
The separate examples are the fifth interpreter-service box in `../fusion-combobox-server-search`, the
live transport catalogue in `../fusion-combobox-live-catalogue`, and the real EJ2 Query Promise in
`../fusion-combobox-equipment-availability`. Each scoped Astra xhigh builder uses the shared build lock.
The verified previews opened in the browser and their Razor views in Rider. The combined landing worktree
is `../fusion-combobox-follow-through`, including the direct DataManager proof from `../fusion-combobox-data-manager`.
The independent skill replay used ComboBox and NumericTextBox, and the integrated inventory matched 18 operations
on both host manifests. The checks caught a deleted live dataSource door and a synchronous Query.execute
declaration. The loopback vendor probes reported zero page errors.
The combined unit landed at `d67e15d16`. The full browser run passed all 682 tests, with 10 new passes
and all 672 prior passes preserved. The supplier desk awaited the actual EJ2 DataManager result and
showed distinct delivery instructions for Northbank and Harbor. Its nested declarations belong to ComboBox.
The onboarding skill records the same component-local procedure for a future component the owner requests.
The Fusion implementation and proof used Codex with scoped GPT-6.2-Astra xhigh builders. The content
commit inherited Claude attribution from the previous session's landing file. The landing file now records Codex.
The first committed check reported 38 of 40. The unqualified FieldSettingsModel cites broke module_cites
and its accepted plant. The follow-up uses the installed package paths, makes the DataManager request
boundary explicit and corrects the replay count by naming the covered operations. The focused citation
check resolved all 1,052 citations and its three existing plants returned the expected verdicts.

The fresh Astra xhigh builder for MI.18/28/29 has started in `../movein-owners-restarted` at `21d6a2ec4`, with
the 17 current part 6 situations and the landed Contacts dependency. The original `../movein-owners` brief
and modified `spec/STATE.json` stay preserved. All proof commands queue behind the same build lock.
The retry completed the builder work. The isolated screen passed all 50 cases and the 16 new situations
caught their planted defects. The compiled file matched all 681 page-door entries in the builder worktree.
The integrated replay carries 691 entries and adds exactly 16 MoveIn promises. The landing lane now
takes the waitlist, nested assessment owners and room handoff batch. E25 follows in MI.17, E30 and the
reserved waitlist scenario X69 follow in MI.20, and the authority-specific E44 shape is recorded in part 9.
The original worktrees and saved evidence stay available.
The integrated lane landed the batch at `2c77b1a04`. The full browser run passed all 698 with zero
failures or overruns, including all 682 prior passes and the 16 new situations. The root build had
zero warnings or errors and matched all 691 compiled page-door entries. Waitlist history rereads W100,
the draft stays Open and BED4 remains held. The replaced assessment owner records once after the
real nested unsubscribe refusal. Room history reopens ROOM7 and the payer contact is a separate
successful E10 read. The next app batch is MI.20 and MI.21, including the pending X69 availability
read. Earlier saved proofs retain their original results.
The committed check passed 40 of 40. The repository scripts pushed the batch and its handoff to
`6a418dc64` with remote equal to HEAD. A fresh GPT-6.2-Astra xhigh builder is working from that
base in `../movein-availability`, with the MI.20/21 and X69 brief in `.landing/MoveInAvailability`.
The new builder passed its fresh one-case baseline within budget. The startup and review rule in
`../movein-scope` takes the shared lane while the availability builder prepares its failing tests.
The startup and review rule landed at `03287bb21`. The existing AGENTS.md symlink reads the same
CLAUDE.md rules. The rule-name check passed its 20 cases and the full local regression preserved
all 698 prior passes. The availability builder remains confined to app code and its browser proof.
The startup rule passed all 40 committed checks and pushed with its handoff to `980dee5ed`.
The availability builder then passed the restored move-in screen, 61 cases with zero failures or
overruns, and the twelve app-only plants. Its compiled page-door table matched all 702 entries.
The lead reproduced the four spec files from the current lane and read the planted failures and
the app code. The shared landing gate now takes MI.20, MI.21 and the reserved X69 from MI.18.
The next app batch after it is MI.8, MI.9 and MI.13 under the existing queue.
The availability unit landed at `e53943447`. The integrated gate passed with 702 compiled
page-door entries. The browser run passed all 709 tests, including eleven newly proven cases,
and preserved every previous pass. The independently owned card can update while the main
snapshot waits, and releasing an older snapshot preserves the newer bed holder. The access
check at response release and the two-client reconnect offer are held by their browser cases.
X69 proves the waitlist join saves W100 and leaves BED4 available at revision 14 on the next read.
MI.17 still owns the admission conflict entry, and MI.13 extends the E18 catalogue into pricing.

MI.8 payer coverage, MI.9 independent verification and MI.13 service pricing landed together at
`de1d8f40c`, including MI.3 situation 5 and the persisted quote check in MI.4.4. The scoped
Astra xhigh builder restored 77 passing move-in cases and caught 22 app-only planted defects.
The compiled page-door table matched all 718 entries. An independent replay matched all four
reference and feature files byte for byte. The integrated non-browser gate passed and the full
browser run passed all 725 without a budget overrun, preserving all 709 earlier passes.
Harbor Mutual at 80 percent and the entered policy at 20 percent reopen together. Private
funding excludes the late secondary reply and cannot inherit an old verification. The independent
drawer checks POL-100 while the main draft stays editable, and its errors stay with the drawer.
The exact 3040.00 quote and 2432.00 assistance wait for both catalogue and terms. An identical
recalculation preserves acceptance and a changed selection clears it. Part 9 records the
missing payer document and exemption facts, and the posted funding snapshot.

Next is MI.14 service review and MI.15 acceptance of the exact current quote. Open their scoped
builder after the checked push. MI.25 deposit and MI.26 diet follow in the existing queue.

MI.14 service review and MI.15 exact quote acceptance landed at `bfea7b1e4`. The scoped
worktree passed all 87 move-in cases, caught ten new app defects and the strengthened X91 from
MI.20, and matched 728 compiled page-door entries. The lead replayed all four spec files byte
for byte. The integrated non-browser gate passed and the full browser run passed all 735,
preserving all 725 earlier passes without a budget overrun.
E19's awaited component record and count use the working service selection. Removing pharmacy
reopens the 2980.00 offer with Dining and two visits. A failed record clears busy, and releasing
a replaced panel cannot change the new price. Identical terms keep acceptance. A real price
push retires the previous offer before one refresh. A late revision-3 receipt cannot change
the replacement's status or start another assistance read. Revision 4 at 3060.00 is unaccepted
until the coordinator agrees again. A foreign community price event leaves the active offer alone.

Open MI.25 admission deposit and MI.26 diet after this checked push. Keep the working quote
and its exact accepted revision as the payment authority. The medications and pharmacy batch
follows in the existing queue.

MI.25 admission deposit and MI.26 diet landed at `3666f023d`. The scoped 97-case move-in
screen and private gate passed, 11 app-only plants failed at their named observations, and
all 738 compiled page-door entries matched the four-file spec replay byte for byte. In the
integrated browser run all 745 passed, preserving every one of the prior 735 passes without
an overrun.

An unaccepted quote or stale amount cannot charge. A genuine post-commit connection loss
recovers PAY100 through E39. A second explicit POST yields PAY100 again and the next read
shows one payment. The updated 650.00 offer can be accepted while the older 500.00 request
waits and the old amount still cannot charge. Diet saves the working flags, note, repeated
document kinds and each label position. Invalid imported flags and kitchen restrictions leave
the stored diet in place. The main Save uses a preceding diet save's new version, and a late
five-meal preview cannot overwrite the confirmed Memory care or its four-meal plan.

Open MI.10 medication rows, MI.11 pharmacy review and MI.12 late validation from the checked
push. Twelve feature IDs remain in the app queue, including these three. The earlier specific
gaps stay in part 9 of the app spec.

MI.10 medication rows, MI.11 pharmacy review and MI.12 late validation landed at
`86f782c1b`. The scoped move-in screen ran all 112 without overruns, all 19 app plants were
caught on their specific assertions, and all four files replayed with the 753 compiled
page-door entries. The integrated check and all 760 browser cases passed, preserving all
745 prior passes.

An E13 dose or version refusal keeps the saved order. E14 retains the actual edited M2
while it deletes M1. E15 reads the current recorded allergy and the live review panel's
awaited record. It clears an eligible order and leaves that clearance on file when the
instructions fail or the payer changes. A held 400 marks its original input while it lives.
After replacement or unload, the main summary still receives the failure and the new row
stays clean. A fresh E15 uses the new row. The tested narrower E15 envelope loses the main
summary and exact input capture. Part 9 records the current typed main envelope and its
server-side PharmacyRequest projection.

Open MI.16 save draft and MI.17 submit admission after the checked push. MI.19 discard
follows. Nine feature IDs remain in the main app queue, including these two.

MI.16 save draft and MI.17 submit admission landed at `5440174c6`. The two feature fixtures
passed ten cases. The move-in screen passed 122 of 122 with zero overruns. Sixteen app
plants failed at their named assertions, and the four-file replay matched the 763 compiled
page-door entries. The integrated gate and all 770 browser cases passed. All 760 prior
passes remain. The optional cross-command plant's initial navigation timeout is preserved
in its worktree. A quiet repeat and restored run passed with the existing budgets.

A well-formed draft without a bed saves version 8, returns its ETag and referral, and
reopens with its recorded notes. A real contact edit followed by a later save proves the
compare panel reads the exact stale version without overwriting current edits. Admission
rejects an ineligible building or mismatched pharmacy review. The successful E25 writes
ADM100 and occupies BED4 atomically. The held E26 checklist blocks an additional POST
and PUT at the shared handler. Explicit replay returns the same admission and the next
read shows one allocation and one history entry. A held bed takes the existing waitlist.

The owner requested Windows net48 proof before the remaining app work. Use the scoped
Net48BrowserHost brief after this push. Windows job 135959330912 built both MVC 5 projects.
The twin's 11 written cases failed at startup because the Core launcher refused the net48
host. The fresh browser step was skipped. The read-only review found the late bundle
copy, unsupported Windows process listing, and two missing twin routes used by written
cases. Verify both MVC 5 browser suites under IIS Express on the checked Windows job.
MI.19 discard then resumes the app queue. Seven feature IDs remain, and the earlier
specific gaps stay in part 9 of the app spec.

The Windows launch repair landed locally at `926994175`. The private gate, 70 focused
runner and delivery cases, the three net10 browser controls and the integrated 770-case
regression all passed. The SDK twin compiled on its packed dependencies with zero
warnings and errors. Its site contains the two shared views, the MVC assembly, the Razor
compiler and three required scripts. Removing the conformance bundle failed the layout
build. The multipart and room validation browser cases caught their named plants.

Windows CI still owns the live evidence. The twin and the classic consumer start as
separate sites under CLR v4 IIS Express, wait for their real MVC readiness pages and
report exit of the servers they started. The same Windows job must show 11 written twin
cases and the one fresh MVC 5 case. Source selection and Mac checks establish no Windows
runtime result. A repeat Linux core run on the preceding app state also stopped the
revoked-contact case during initial navigation at the unchanged 704 ms wait, before
its validation gesture. Local app and landing runs pass that case. Read the newest
run before deciding what either host still needs.

The first repaired Windows job, 136003981430, started and stopped both IIS Express
sites. The fresh MVC 5 app passed its single browser case. The twin ran all 11 and
passed nine, including multipart binding and the shared room validator. X14.11 stopped
on the first contact navigation at the 704 ms wait. M19.X4 reached the JSON startup
refusal in its resolver-unset world before the intended request-registration refusal.

The follow-through landed locally at `117368f44`. The twin's HTTP readiness now reads
Contact move-in and requires its nested name input before browser timing starts.
The intake declares its existing request before the typed field so the unchanged
missing-resolver world can reach the request guard. The packed build had zero warnings
and errors, the related net10 submission passed, and the integrated gate passed all
770 browser cases with zero overruns and no lost passes. Read the new Windows job's
two result lines and process exits before claiming the Windows proof.

The current Linux and two preceding core runs failed the same first D100 navigation
before the revoked-authority gesture. All later move-in cases passed. Measure document
render and browser load on the same Linux host before changing that path.

Windows job 136022062883 verified both MVC 5 sites under IIS Express. The twin passed
11 of 11 and the fresh MVC 5 app passed one of one, each with zero overruns. The
contact partial readiness returned HTTP 200 with NextOfKin.Name, and the run reported
both IIS processes and the nested net10 server exiting. The written Windows proof is
complete. Two unwritten net48 placements remain in the existing rewrite work.

A probe in the pinned Linux ARM64 image reproduced the first D100 timeout. Document
headers arrived at 648 ms and the load event came at 706 ms. The next browser
context loaded in 211 ms. Scripts finished shortly after the document both times.
On a fresh process, an isolated HTTP render took 666 ms and ended its world with
zero parked gates. The first and next browser loads then took 231 and 225 ms.
The first MVC render took 596.9778 ms, and the first subsequent render took
164.1887 ms. Prepare only runs that select the actual net10 move-in screen, then
run the original authority test and the Linux core gate. MI.19 follows.

Move-in browser readiness landed locally at `75e986a5a`. Runs that select the net10
move-in screen prepare the actual D100 view and end the separate readiness world before
browser timing begins. The selector control proved the request is skipped for unrelated
screens, shards and hosts. A wrong-heading plant failed before the browser and still
ended the readiness world. The integrated run passed all 770 cases, including the
revoked-authority submission, with zero overruns and all 770 prior passes preserved.
The browser wait budgets are unchanged. Read Linux CI after the checked push. Resume
MI.19 discard from that pushed base. Seven feature IDs are open in the app queue.

The full check at `75e986a5a` passed 40 of 40, and the push checked the remote at
`26d1b178b`. Linux core job 136039900515 logged the actual D100 render and world
closure with zero parked gates. The original revoked-authority case passed, and the
602-case shard finished with zero failures and zero overruns.
Windows job 136040754074 at the same pushed state passed the 11 twin cases and
one fresh MVC 5 case with zero overruns. Both IIS Express sites and the nested
net10 server exited.

MI.19 discard landed locally at `e1047be7d`. The integrated browser gate ran all
775 cases with zero failures or overruns and preserved all 770 prior passes. The final
scoped screen passed 127 cases. Five app plants caught their named defects and the
four-file replay matched all 768 compiled page-door entries.

The coordinator sees the stored resident name and only Yes sends a bodyless DELETE
with the current If-Match value. A stale version retains Luis's notes. Successful
discard advances the version once, unloads the editing workspace and reopens as
Discarded. Cancel and connection loss before the server acts preserve the open draft.
D300 stays Submitted at version 9 with ADM203, BED12 and Omar King's saved name.
Replacing the real draft-actions partial closes its open question without a DELETE.

After the checked push, open MI.22 documents, MI.23 document metadata, and MI.24
packet in that order. MI.27 coordinators, MI.30 reception and MI.31 request defaults
follow. Six feature IDs remain. Keep the app on the landed typed doors and record
any missing capability in part 9 of the app spec.

The spec check at `e1047be7d` passed 40 of 40, and the push checked the remote at
`63d678f50`. Linux core job 136059969843 passed all 607 cases, including all five
MI.19 cases, with zero overruns. Windows job 136060885899 passed all 11 twin cases
and the fresh MVC 5 case, each with zero overruns. It reported both IIS Express
sites and the nested net10 server exiting.

MI.22 document upload, MI.23 signature metadata and replacement, and the MI.24 packet
app work landed locally at `56741aa3d`. The integrated run passed all 790 cases, with
no overruns or lost passes. The final move-in screen passed all 142. Fifteen named
plants and two capture-time plants failed at their actual observations. The four-file
replay matched all 783 entries in the compiled page-door table.

A real FileList supplies repeated multipart files and their metadata. The upload
policy and signature HEAD can wait while a file changes, and the fresh guard refuses
the changed selection before transport. The server stores an accepted batch atomically
and the receipt reads the committed files. A closed upload owner receives no later
route, while a next read resolves the committed batch. The signature PUT uses the
stored ETag and the actual file body. Changing the server version keeps the original
until the next successful replacement. The next read asserts the saved text and line
feed. The real packet filename and printable preview are visible. Notes and accepted
print intents persist. A superseded quote cannot print.

The isolated E36 response matched all 107 literal bytes and its world and server
exited. Inspection of the browser-saved file itself is still open under the current
Playwright allowlist. Part 9 records this exact evidence gap. The pending decision is
whether the harness may read that downloaded artifact with its existing Playwright
stream API. The app code and tests use the current rules.

After the checked push, take MI.27 coordinators, then MI.30 reception, then MI.31
request defaults. Three application feature IDs and the one MI.24 download observation
remain. Use the landed typed framework APIs and MVC partial composition.

The full check at `56741aa3d` passed 40 of 40, and the checked push ended at
`245eaf302`. Linux core job 136094794931 passed all 622 cases with zero overruns.
Windows job 136095665659 passed all 11 MVC 5 twin cases and the fresh MVC 5 case
with zero overruns. Both IIS sites and the nested net10 server exited.

MI.27 coordinators and ward preparation landed locally at `0cb3d7355`. The integrated
run passed all 795 cases with zero failures or overruns and preserved all 790 prior
passes. The scoped move-in screen passed all 147. Six app plants failed at the named
observations, and the independent four-file replay matched 788 compiled page-door
entries.

The real SignalR card joins the draft, reads the Maya and Luis roster, records MSG1
with sequence 1, and rejoins after a changed connection ID. A separate world remains
empty. Short and forbidden messages and a revoked membership cannot post. The real
ward store sends the multipart request with CO1 and the operation header, and the next
bed read distinguishes an accepted instruction from a lost request. Closing the card
releases the hub and removes its subscription from the surviving store while the held
quote finishes. Reopening attaches once. Both the quote-owner defect and the retained
subscriber failed independently. The app uses the existing located returned-unsubscriber
behavior, as recorded in part 9.

After the checked push take MI.30 reception, then MI.31 request defaults. Two app
feature IDs remain, plus the separately open direct browser download observation from
MI.24. Keep the closed Playwright rule pending the owner’s decision.

The full check at `0cb3d7355` passed all 40 checks and the checked push ended at
`06b369941`. Linux core job 136119760216 passed all 627 cases with zero overruns,
including the new line and ward cases. Windows job 136120846454 passed the 11
MVC 5 twin cases and the fresh app case with zero overruns. Both IIS sites and
the nested net10 server exited. The separate checks job 136119760470 built with no
warnings or errors and matched 788 compiled page-door entries. The runner received
a shutdown signal during architecture testing. The job printed no test verdict.
The same runner interruption ended the preceding documents checks job 136094794978.

MI.30 reception landed locally at `1c09bcf83`. The integrated gate passed all 800
browser cases with zero failures or overruns and preserved all 795 prior passes.
The focused fixture passed five cases and the move-in screen passed all 152.
Seven named plants failed at their expected assertions. The four-file spec replay
matched the builder and all 793 compiled page-door entries. The integrated build
had zero warnings or errors, and all 327 architecture cases passed.

The real native form includes its external Desk and excludes the disabled, unchecked
and foreign controls. It navigates to Alice Ward's receipt and E47 reads REC100 for Helen.
The typed reactive form keeps its receipt beside the form and leaves the separate
medication error visible. Copies zero sends nothing. A barred visitor and a lost
transport leave history empty. The room reader distinguishes absent detail and Enter.
The companion multipart sends a real file and the next draft read retains the
original prescription.

After the checked push, take MI.31 request defaults and recovery. One application
feature remains in the queue. The MI.24 browser-saved artifact observation remains
open under the installed test rule. Use the landed typed framework APIs and MVC
partial composition.


At c91bae1fc CI reported seven passing jobs, the spec job running, and verify-net48 failed in
its launcher with “The conformance app serves no host 'net48'.” That existing host work remains
behind the app.







At the Fusion push the local browser run passed all 682 and the committed spec check passed 40 of 40.
CI on that pushed state had one host test over its 7.44 second budget at 7.69 seconds. The Windows
browser job is still refused at startup because the conformance harness has no net48 host. The spec job
was cancelled and the proof job reported its cancelled prerequisite. This app batch leaves the harness
and framework unchanged.


Behind the app, the framework's open list: a Native date input and an input over a date (the app's gap 1); a list
of files in a request form (D14.3; the runtime throws notBuilt); a validation check on a text or a file body (refused
where the view renders); the seven operators no C# writes leave the grammar (the owner's yes, 2026-11-17), with the
table "After 1.0" in spec/NOTES.md; the plan contract narrowed to what the C# writes; an enum in a pushed message's
member. Then unit 3's second piece (the Fusion template pages), and Fusion's own: an equality of two members in a
template, the grid's validation door and exports, SmartPaste, U27.
<!-- /next -->
