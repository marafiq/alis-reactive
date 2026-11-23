# Handoff

Written at `date`: Mon Nov 23 17:14:14 EST 2026

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
note: open 2026-11-20 | direction | the queue of this file | queue | "please priortise framework over mvc5 or other. Fr
note: open 2026-11-20 | direction | the queue of this file | queue | "If you image if i decide to onboard *IF* TanStack
note: open 2026-11-20 | direction | the queue of this file | queue | "new session will re-discover, bring concrete and 
unit: ready ../ci-tlc-cold SpecCiCold
  next: scripts/land-unit.sh ../ci-tlc-cold 2ab0d5f00 "Publish independent TLC baselines for spec CI" ".landing/SpecCiCold"
  leaves by: `scripts/land-unit.sh {worktree} {base} "{title}" ".landing/{name}"`, while the lane is pushed
  exit line: "landed", with the browser run's result line
  goes to: unit: landed
unit: ready ../ci-tlc-mac MacTlc
  next: scripts/land-unit.sh ../ci-tlc-mac bbe7e4869 "Run model checking on the Mac and validate its evidence in CI" ".landing/MacTlc"
unit: ready ../framework-multipart-file-list MultipartFileList
  next: scripts/land-unit.sh ../framework-multipart-file-list 41c752166 "Send a bound list of files as repeated multipart parts" ".landing/MultipartFileList"
unit: unbriefed ../framework-native-date NativeDateInput: its Rules name DateWire.tla, which the worktree lacks
  leaves by: the brief is completed, before any work
  read first: the files of a unit, below
  goes to: unit: briefed
unit: landed ../framework-native-date-value NativeDateValue
  next: git worktree remove ../framework-native-date-value
  leaves by: `git worktree remove {worktree}`
  goes to: gone
unit: unbriefed ../fusion-api-operations FusionApiOperations: its Done holds no line "At the base:"
unit: ready ../fusion-audit FusionAudit
  next: scripts/land-unit.sh ../fusion-audit 9c77fd7d8 "Fusion audit: compare the pinned vendor contract and improve component onboarding" ".landing/FusionAudit"
unit: returned ../fusion-combobox-data-manager FusionComboBoxDataManager: reply.md waits for read.md
  leaves by: the lead writes "read.md"
  read first: the rule lines "reply.md" opens with, at their source; the change against its base
  exit line: "verdict: land"
  goes to: unit: ready
unit: returned ../fusion-combobox-equipment-availability FusionComboBoxEquipmentAvailability: reply.md waits for read.md
unit: ready ../fusion-combobox-follow-through FusionComboBoxFollowThrough
  next: scripts/land-unit.sh ../fusion-combobox-follow-through 5f77ce2a6 "Prove ComboBox search, live configuration and real EJ2 Promises" ".landing/FusionComboBoxFollowThrough"
unit: unbriefed ../fusion-combobox-follow-through IntegrationPrep: brief.md lacks Scope, Rules, Outcome, Done, Commands, Reply
unit: returned ../fusion-combobox-live-catalogue FusionComboBoxLiveCatalogue: reply.md waits for read.md
unit: ready ../fusion-combobox-onboarding-notes FusionOnboardingNotes
  next: scripts/land-unit.sh ../fusion-combobox-onboarding-notes d67e15d16 "Resolve pinned ComboBox citations and clarify DataManager onboarding" ".landing/FusionOnboardingNotes"
unit: returned ../fusion-combobox-onboarding-replay FusionComboBoxOnboardingReplay: reply.md waits for read.md
unit: ready ../fusion-combobox-payload FusionComboBoxPayload
  next: scripts/land-unit.sh ../fusion-combobox-payload 9c77fd7d8 "Prove typed ComboBox changes on Helen Martinez's communication plan" ".landing/FusionComboBoxPayload"
unit: returned ../fusion-combobox-server-search FusionComboBoxServerSearch: reply.md waits for read.md
unit: unbriefed ../fusion-form-builder FusionFormBuilder: its Rules name AGENTS.md/CLAUDE.md, which the worktree lacks
unit: unbriefed ../fusion-grid-api FusionGridApi: brief.md lacks Scope, Rules, Outcome, Done, Commands, Reply
unit: unbriefed ../fusion-grid-bulk-actions GridBulkActions: its Rules name AGENTS.md/CLAUDE.md, grid.d.ts, which the worktree lacks
unit: unbriefed ../fusion-grid-cell-review GridCellReview: its Rules name AGENTS.md/CLAUDE.md, syncfusion/ej2-grids/src/grid/base/grid.d.ts, interface.d.ts, which the worktree lacks
unit: unbriefed ../fusion-grid-columns FusionGridColumns: its Rules name syncfusion/ej2-grids/src/grid/base/grid.d.ts, which the worktree lacks
unit: unbriefed ../fusion-grid-contract FusionGridContract: its Rules name FusionSource.cs, .d.ts, which the worktree lacks
unit: unbriefed ../fusion-grid-editor-api GridEditorApi: its Rules name AGENTS.md/CLAUDE.md, references/full-api.md, which the worktree lacks
unit: unbriefed ../fusion-grid-exports FusionGridExports: its Rules name /SKILL.md, which the worktree lacks
unit: unbriefed ../fusion-grid-large-census GridLargeCensus: its Done holds no line "At the base:"
unit: unbriefed ../fusion-grid-query-api GridQueryApi: its Rules name AGENTS.md/CLAUDE.md, references/full-api.md, which the worktree lacks
unit: unbriefed ../fusion-grid-remote FusionGridRemote: its Rules name /SKILL.md, grid.d.ts, interface.d.ts, FusionGrid.cs, which the worktree lacks
unit: unbriefed ../fusion-grid-selection FusionGridSelection: its Rules name grid.d.ts, which the worktree lacks
unit: unbriefed ../fusion-grid-ward-transfer GridWardTransfer: its Done holds no line "At the base:"
unit: unbriefed ../fusion-query-builder FusionQueryBuilder: its Rules name AGENTS.md/CLAUDE.md, which the worktree lacks
unit: ready ../movein-availability MoveInAvailability
  next: scripts/land-unit.sh ../movein-availability 6a418dc64 "MI.18.5, MI.20 and MI.21. Publish the world's bed changes to independently owned page and card streams. A held response checks current community access and preserves a newer displayed revision and holder. Reconnect re-reads the bed; the retry offer remains until every failed owner recovers or leaves." ".landing/MoveInAvailability"
unit: ready ../movein-contacts MoveInContacts
  next: scripts/land-unit.sh ../movein-contacts d13a4ce7f "The move-in page gains its contacts: MI.5 contact rows, MI.6 contact authority and MI.7 contact removal, on the foundation at fbea584b2." ".landing/MoveInContacts"
unit: waiting ../movein-coordinators MoveInCoordinators: for MoveInDocuments
  leaves by: the unit it names lands; then it moves to the new head and each command of its "Done" runs there
  exit line: each line of the brief's "Done"
  goes to: unit: ready
unit: waiting ../movein-defaults MoveInDefaults: for MoveInReception
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
unit: waiting ../movein-reception MoveInReception: for MoveInCoordinators
unit: ready ../movein-review-acceptance MoveInReviewAcceptance
  next: scripts/land-unit.sh ../movein-review-acceptance c91bae1fc "MI.14 and MI.15. Review the selected services and accept the displayed quote revision" ".landing/MoveInReviewAcceptance"
unit: ready ../movein-scope MoveInSessionScope
  next: scripts/land-unit.sh ../movein-scope d13a4ce7f "Keep move-in implementation and builder proof within scope" ".landing/MoveInSessionScope"
unit: waiting ../net48-browser-host Net48BrowserHost: for MoveInDraftAdmission
unit: waiting ../net48-twin-followthrough Net48TwinFollowThrough: for Net48BrowserHost
unit: ready ../retire-picomatch-exception RetirePicomatchException
  next: scripts/land-unit.sh ../retire-picomatch-exception d13a4ce7f "GitHub withdrew GHSA-fc5q-39xc-vjpf on November 18. npm no longer reports it, so retire its exception under rewrite section 2.7. Picomatch stays pinned at 4.0.7. The audit refuses reported advisories by ID, including the retired ID if it returns." ".landing/RetirePicomatchException"
unit: unbriefed ../tanstack-charts TanStackCharts: its Rules name /SKILL.md, Components/ConformanceComponent/ConformanceComponentHtml.cs, Care/Admission/RoomChoice.cshtml, which the worktree lacks
unit: unbriefed ../tanstack-table TanStackTable: its Rules name Care/Residents/ResidentDischarges.cshtml, Care/Round/LeastDoseRoundController.cs, Care/MoveIn/PayerDesk.cshtml, which the worktree lacks
unit: landed ../team-process TeamProcess
  next: git worktree remove ../team-process
lane: pushed at 5dc4ee24a on feature/rewrite: equal to the remote
  leaves by: the landing of a unit that is ready
  read first: the queue of `HANDOFF.md`
  exit line: "landed"
  goes to: lane: landed
ci: unread at 5dc4ee24a
  leaves by: `python3 scripts/ci.py`
  exit line: "ci at" the commit, with each job
  goes to: ci: running, ci: failed or ci: passed
file: over spec/CLAUDE.md: holds 2081 lines, its limit 150
  leaves by: a unit that splits or cuts it; the mistakes are condensed, kinds merged, each line short enough to follow
  exit line: "landed"
  goes to: gone
file: over HANDOFF.md: holds 294 lines, its limit 150
file: over tests/Alis.Reactive.Conformance.App/CLAUDE.md: holds 201 lines, its limit 150
file: over tests/Alis.Reactive.Conformance.PlaywrightTests/CLAUDE.md: holds 237 lines, its limit 150
machine: review due : 36 units have landed since .claude/rules/process.md last changed
  leaves by: a unit that changes this file, the lead's without waiting for the owner, the framework's quality bar never lowered
  read first: the units sent back, the kinds of mistake that grew, the states no unit met
  exit line: "landed"
  goes to: gone

## Where it stands
- Branch: feature/rewrite
- HEAD: 5dc4ee24a rules: root CLAUDE.md at its 200-line limit, agent hand-offs in the skill delegate, builders in Claude Code's own worktrees, a growing kind of mistake moves its holder up
- Full check: .work/logs/spec-check-5dc4ee24a.log: spec check at 5dc4ee24a: 40 of 40 checks pass
- .work/last-run.txt: 20261123-170112-80366 5dc4ee24a

## Landed since the commit before the last spec: STATE commit
5dc4ee24a rules: root CLAUDE.md at its 200-line limit, agent hand-offs in the skill delegate, builders in Claude Code's own worktrees, a growing kind of mistake moves its holder up
faf053a91 spec: STATE and handoff at afa0656b9

## Open
open: 19 promises with no browser test, 7 runtime cases not built, 14 doors not implemented
feature promises: 719; named by a browser test: 700; with no browser test: 19
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
- Written from memory, not read at its source at HEAD: a design line, a signature, a path, a name, a page's expected value, a case's shape or a TLA+ ordering: 431 to 434
- A rule or a tool's list not applied before the change: 436 to 437

## The owner's latest decisions
spec/CLAUDE.md:9: - The owner, 2026-11-20, correcting the browser validation proposal: preserve existing use cases and establish the one shared operation befo
spec/CLAUDE.md:10: - The owner, 2026-11-20: run TLC on the Mac to conserve hosted CI hours. The Mac checks changed models and configs

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
- 2026-11-20 | direction | the queue of this file | queue | "please priortise framework over mvc5 or other. Framework, then fusion or mvc should go in parallel. The validation is built to work regardlessof consumers. But if consumer need something specific, we should design it like row level action which has same framework permitives"
- 2026-11-20 | direction | the queue of this file | queue | "If you image if i decide to onboard *IF* TanStack components pure JS Table... then validation still work." "adapter is not default choice, i would prefer if none is needed for any consumer"
- 2026-11-20 | direction | the queue of this file | queue | "new session will re-discover, bring concrete and fully thought through one proposal to do which aligns with framework vision, and not add lower quality or hacks." "We stay on same branch, and follow the process. Quality bar will be never be lowered. But I am okay to adapt modern LLM patterns to arrive at correct, and well designed solution"
<!-- /notes -->

## Next
<!-- next -->
### Next session

The owner's order, 2026-11-23: the team process, then the framework core to 1.0, then everything else.
Builds stay serial on this Mac; source `scripts/toolchain-env.sh` first.

**First, the owner's question on the operators.** The owner will revisit why the operators are hard to
implement. Facts for that talk, with no recommendation and no operator work before it:
- The seven: add, sub, mul, mod, neg, finite, merge.
- `Alis.Reactive/Schema/Operator.cs` declares them, and no C# door writes any of them.
- `spec/Schema/Schema.tla` lists them in `Operators`, so any change to them starts there and reruns TLC.
- `Alis.Reactive.Assets/runtime/Conditions/operators.ts` throws "not built" for each.
- Nearby code: ArrayLambdas' summed ints, doubles and decimals, and `Conditions/decimals.ts`.

**The team process.** Landed: root CLAUDE.md at its 200-line limit, the rules for handing work to agents in
`.claude/skills/delegate/SKILL.md`, `worktree.baseRef` set to head, the isolation field on builder and page-writer,
and a kind of mistake that grows moves its holder up a rung. Agent teams are on: a named call without isolation is an
in-process teammate in the lead's folder, so a builder's call passes isolation as worktree. Open, each a small unit of
its own: a `scripts/push.sh` refusal while a kind of `spec/MISTAKES.md` grew and its holder did not change, so the
rule is held by a tool; `scripts/state.py` reading as landed a unit whose paths a landed commit holds (MoveInContacts,
MacTlc and RetirePicomatch read as ready today); the notes whose words `spec/NOTES.md` holds closed, so
`HANDOFF.md` comes back under its 150 lines.

**The framework core to 1.0 (U1 to U22).** Done when:
- `scripts/open.py` prints no core runtime case: `InputsAndIds/model.ts` three, `RequestsAndResponses/request.ts`
  one, `Validation/forms.ts` two, and the operators line after the owner's talk;
- X3.7, a date in a JSON response on MVC 5, passes on CI's Windows job;
- `spec/REWRITE.md` section 4 states each of U1 to U22 closed with its result line (today it states U3, U4, U5,
  U8 to U13 closed and U1, U2, U14 to U16 landed; U6, U7, U17 to U22 are not stated);
- the core's public surface text is read as the 1.0 surface, and README's examples come from tested pages.
Its units, in order:
1. The plan narrowed to what C# writes: an input's value read and a rule's test, each case's own error text
   names the form C# writes.
2. The form-data boundary: an enum sent in form data (the move-in app met it; the designer decides its form),
   an object stays refused until after 1.0 (the owner's yes of 2026-11-17), a text or file body's check
   verified against the request writer.
3. X3.7 written on the net48 host; CI's Windows job proves it.
4. Ten parked core leads, each one verdict by the owner's test of 2026-11-20 (a reproduced defect, a reachable
   incomplete contract or an unproved promise is in; otherwise a row of the after-1.0 table): the raw text and
   single-file body check refusal, the named enum in a pushed message, typed list construction from separate
   sources, plan shape alignment, standalone named rule sets, activated-row replacement, handler cleanup, the
   attached-partial validation summary, an input over an enum whose zero has no name, and a row's "removing"
   state after a failed removal (move-in part 9).
5. Core freeze: section 4 rewritten unit by unit, the surface read, README checked.

**Then:** Native (date entry first, then its follow-ups), Fusion U26 and U27 (14 render helpers, 18 promises),
the release tag and PR #247. The six packages pack at one version, so the tag comes once, at the end.

**Parked, each where it lives:** browser validation's shared key
(`spec/Validation/browser-validation-proposal.md`, the owner's condition stands); TanStack Table
(`../tanstack-table`, uncommitted) and Charts (`../tanstack-charts`, a brief); Form Builder and Query Builder
(their pushed branches); the Fusion Grid API worktree (`../fusion-grid-api`, uncommitted); five ComboBox
units whose replies wait for a read. The history this block held is the HANDOFF.md of the previous STATE commit.

**Waiting for the owner:** whether units land by pull request into `feature/rewrite`, CI the gate and
`/steward` watching it, piloted on one unit (about 10 to 12 minutes of hosted CI a unit; net48 proven per unit;
the Mac's build lock stops gating landings).
<!-- /next -->
