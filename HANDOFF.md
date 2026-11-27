# Handoff

Written at `date`: Fri Nov 27 01:40:02 EST 2026

## State, as `scripts/state.py` reads it from git
note: open: 29, each in the notes block below
note: closed 2026-11-25 | direction | the queue of this file | RowContainers | "1 seems more aligned, but make sure that : its follow-up RowContainers landed
unit: landed (84): BodilessCheck, CheckoutTrap, SpecCiCold, MacTlc, ContainerOnly, CoreLeads, NarrowedPlan, CoreStatus, DefinitionOfDone, DesignCounts, DesignedAsks, DesignerClaims, EmptyTextItem, FormDataBoundaryNet48, FlagsAttribute, FormDataBoundary, MultipartFileList, NativeDateValue, FreshAppSlice, FusionAudit, FusionComboBoxFollowThrough, FusionOnboardingNotes, FusionComboBoxPayload, G13Forms, InputForm, InputSent, InputSentForms, JsonNoContent, ListFromSources, ListPosition, MarkdownLand, MistakesAsRules, MistakesCarried, MistakesShort, MoveInDoorMap, MoveInAvailability, MoveInContacts, MoveInCoordinators, MoveInDefaults, MoveInDepositDiet, MoveInDiscard, MoveInDocuments, MoveInDraftAdmission, MoveInMedicationsPharmacy, MoveInNavigationReadiness, MoveInOwners, MoveInPayerPricing, MoveInReception, MoveInReviewAcceptance, MoveInSessionScope, Mvc5JsonNames, Mvc5Razor, Mvc5Roster, Mvc5ViewsCompiled, Net48BrowserHost, Net48TwinFollowThrough, OneDoor, Operators, QueueReach, RetirePicomatchException, FusionFullOnboardingHandoff, FusionGridDownloadEvidence, FusionNumericStep, FusionBreadcrumb, FusionConfirm, FusionMenu, FusionSidebar, FusionToast, FusionToolbar, FusionSmartPasteButton, FusionSwitchFull, RowContainers, Section4Close, SentPosition, SplitValue, StateSpecUnits, TextBodyCheck, UnitsClosed, UnitsReread, ValidationForms, WorldRecords, WriterMove, X37Net48, ZshModifier
unit: unbriefed (20): NativeDateInput, FreshAppDocs, FusionApiOperations, IntegrationPrep, FusionFormBuilder, FusionGridApi, GridBulkActions, GridCellReview, FusionGridColumns, FusionGridContract, GridEditorApi, FusionGridExports, GridLargeCensus, GridQueryApi, FusionGridRemote, FusionGridSelection, GridWardTransfer, FusionQueryBuilder, TanStackCharts, TanStackTable
unit: returned (6): FusionComboBoxDataManager, FusionComboBoxEquipmentAvailability, FusionComboBoxLiveCatalogue, FusionComboBoxOnboardingReplay, FusionComboBoxServerSearch, FusionNumericDecrement
unit: building (1): MoveInOwners
unit: ready (1): FusionNumericDecrementApi
Each unit's worktree, reason and next command: `python3 scripts/state.py`.
lane: pushed at 75ce40417 on feature/rewrite: equal to the remote
  leaves by: the landing of a unit that is ready
  read first: the queue of `HANDOFF.md`
  exit line: "landed"
  goes to: lane: landed
ci: unread at 75ce40417
  leaves by: `python3 scripts/ci.py`
  exit line: "ci at" the commit, with each job
  goes to: ci: running, ci: failed or ci: passed
file: over spec/CLAUDE.md: holds 2157 lines, its limit 150
  leaves by: a unit that splits or cuts it; the mistakes are condensed, kinds merged, each line short enough to follow
  exit line: "landed"
  goes to: gone
file: over tests/Alis.Reactive.Conformance.App/CLAUDE.md: holds 201 lines, its limit 150
file: over tests/Alis.Reactive.Conformance.PlaywrightTests/CLAUDE.md: holds 237 lines, its limit 150
machine: review due : 29 units have landed since .claude/rules/process.md last changed
  leaves by: a unit that changes this file, the lead's without waiting for the owner, the framework's quality bar never lowered
  read first: the units sent back, the kinds of mistake that grew, the states no unit met
  exit line: "landed"
  goes to: gone

## Where it stands
- Branch: feature/rewrite
- HEAD: 75ce40417 Fusion (FusionSwitch): prove live resident alert settings
- Full check: .work/logs/spec-check-75ce40417.log: spec check at 75ce40417: 40 of 40 checks pass
- .work/last-run.txt: 20261127-012332-37508 75ce40417

## Landed since the commit before the last spec: STATE commit
75ce40417 Fusion (FusionSwitch): prove live resident alert settings
5d341cc2e spec: STATE and handoff at cc05d5de4

## Open
open: 9 promises with no browser test, 0 runtime cases not built, 0 doors not implemented
Each, by file and line: `python3 scripts/open.py`.

## The owner's latest decisions
spec/CLAUDE.md:9: - The owner, 2026-11-26 (verbatim in `spec/NOTES.md`): FusionToast.Hide keeps EJ2's zero-argument call and
spec/CLAUDE.md:11: - The owner, 2026-11-26 (verbatim in `spec/NOTES.md`), closing the flags enum: an enum is flags to the framework when
spec/CLAUDE.md:16: - The owner, 2026-11-26 (verbatim in `spec/NOTES.md`), on the forms gate 2's G13 left to the owner: yes to
spec/CLAUDE.md:22: - The owner, 2026-11-26 (verbatim in `spec/NOTES.md`): one door for each list operation. An object JavaScript iterates
spec/CLAUDE.md:26: - The owner, 2026-11-26 (verbatim in `spec/NOTES.md`): the mistakes log stays short enough to stay in context, each kind

## The owner's notes that are open: date | kind | home | follow-up | the owner's words
<!-- notes -->
- 2026-11-26 | direction | the queue of this file | queue | "please note down you can not change framework code now. let me  know if that ever become a case."
- 2026-11-26 | direction | the queue of this file | queue | "yes, you should use the skill for onboarding with, with explicit scope and outcome, continusoly improve skill if it do not pass the quality bar."
- 2026-11-26 | direction | the queue of this file | queue | "why bring that to me, framework is complete for now, skip it if you can not onboard sir. Either previous sessions were wasted but agents spent 2 days covering that"
- 2026-11-26 | direction | the queue of this file | queue | "lets hold on tracing, finish every other framework, and then leave handoff codex will take over"
- 2026-11-18 | direction | the queue of this file | queue | "Think of real domain, which has all these complexities. Get rid of this desk thinking. Keep other pages, other browser tasks, but write this complex app from ground up with full scale complexity. This framework has features to handle it."
- 2026-11-18 | direction | the queue of this file | queue | "I want you to build a skill out of that TLA plus and schema, which will be used by every consumer as well." "This skill you are building should be a guiding principle for any developer to write any complex app by simple decisions, because it has a map, right? It has clear names, right?"
- 2026-11-18 | decision | spec/CLAUDE.md, the conformance app's decisions | MoveInSpec | "This should not be in the skill. This is for you to use the skill to write the app, but you have to first list down every feature the app will have ... The specs of the apps are a separate thing, but you should get help from the specs and the TLA plus that how that app should be written." "Why you need to do inventory again? You have TLA+, you have schema."
- 2026-11-18 | decision | spec/CLAUDE.md, the conformance app's decisions | MoveInSpec | "The framework is not getting new code except the current module in progress. You gotta build the features of the app, and you are already building a skill that will help you write quickly. What decision you are talking about?"
- 2026-11-18 | decision | spec/CLAUDE.md, the conformance app's decisions | MoveInSpec | "Yeah, I'm fine with the native packages." "Are you going to write components from scratch? That is what the TypeScript component was written for, right? So we can exercise the framework with conformance app, so it stay faster."
- 2026-11-18 | quality rule | spec/Reference/native.json and tests/Alis.Reactive.Conformance.App/CLAUDE.md | ConformanceMembers | "Does it have a property returning a complex object, method, multiple parameters? You know, all those combinations have been repeated like 100 times by me." "does conformance has promise. So you can test the lanes of sync and async."
- 2026-11-18 | quality rule | tests/Alis.Reactive.Conformance.App/CLAUDE.md | MoveInSpec | "Build an app which has all possible combinations." "Where is chain request? Where is parallel request? Where is complex conditions within each module? Where is header? Where is query string? Where is hidden fields? Where is getting value from other component?"
- 2026-11-17 | process rule | .claude/rules/process.md | queue | "your whole goal is to get the tooling, the process, the quality rules, how to engage agents, how to scope their work, how to explicitly state what done means, and even for yourself, handoff before compaction notes, keeping it live, how to update status, the whole harness has to be reviewed." Left of it: a planted-defect line for each promise read by the landing script; the conformance app's coverage of the core's doors as a number; the independent inspection of the runtime and the core.
- 2026-11-18 | decision | spec/CLAUDE.md | DecisionsByModule | "where you're gonna keep the decisions. So it cannot be one bulky file."
- 2026-11-18 | quality rule | .claude/rules/views.md, and an analyzer of Alis.Reactive.Analyzers that refuses a plan opened over dynamic or object | TypedDoors | "Dude, why would you use dynamic? The whole point of the framework is tight, and you end up using dynamic. What a pity."
- 2026-11-18 | quality rule | tests/Alis.Reactive.Conformance.App/CLAUDE.md | MoveInSlice1 | "For the app, you need to build fake data ... Sending request isn't enough. It has to send the correct payload so you can observe what is being returned, what is being changed ... you gotta build the domain model of the apps in memory so that you can observe real behavior rather than just assume, oh, I sent the request."
- 2026-11-18 | direction | the queue of this file | queue | "I don't need acceptance numbers, sir. I need that complex app behavior asserted and proved. I am done with hearing excuses or fake numbers."
- 2026-11-19 | direction | the queue of this file | queue | "please do not do patch fix, do proper root cause fix with proof, this is super important quality bar" for Fusion member and payload onboarding.
- 2026-11-19 | direction | the queue of this file | queue | "Produce the Fusion audit first." Then "we should be doing audit, and fixing skill, and checks, so we do not re-work like crazy all the time."
- 2026-11-19 | direction | the queue of this file | queue | "how about implementing server side search in combobox as 5th one" and "use multiple events on one comobobox as well - infact all complex". The server-search unit follows the four-choice payload unit.
- 2026-11-19 | direction | the queue of this file | queue | "i would be curious to add more combox where we use some of its useful meethods" and "some props as well, especially complex ones.." and "use diff comoboxes so u do not get confused or future readers". Keep the transport catalogue properties and methods, interpreter search, and real vendor Promise examples distinct.
- 2026-11-19 | direction | the queue of this file | queue | "showspinner of compobox and hide when it succed" and "comobobox has methods showspinner and hidespinner". The interpreter-search fixture holds a request and checks the actual ComboBox spinner.
- 2026-11-19 | direction | the queue of this file | queue | "use this as refereces to the skill so it works" and "and then test the skill actually" and "yeah lets make skill perform with very high confidence so you can onboard other components". The replay checks the pinned ComboBox and held-out NumericTextBox.
- 2026-11-19 | direction | the queue of this file | queue | "lets onboard datamanager only for combobox and leave it for the rest, but have it in skill so if i ask onboard for that component". The direct, component-local DataManager example builds in ../fusion-combobox-data-manager.
- 2026-11-20 | direction | the queue of this file | queue | "please priortise framework over mvc5 or other. Framework, then fusion or mvc should go in parallel. The validation is built to work regardlessof consumers. But if consumer need something specific, we should design it like row level action which has same framework permitives"
- 2026-11-20 | direction | the queue of this file | queue | "If you image if i decide to onboard *IF* TanStack components pure JS Table... then validation still work." "adapter is not default choice, i would prefer if none is needed for any consumer"
- 2026-11-20 | direction | the queue of this file | queue | "new session will re-discover, bring concrete and fully thought through one proposal to do which aligns with framework vision, and not add lower quality or hacks." "We stay on same branch, and follow the process. Quality bar will be never be lowered. But I am okay to adapt modern LLM patterns to arrive at correct, and well designed solution"
- 2026-11-23 | direction | the queue of this file | queue | "keep worktree around"
- 2026-11-23 | direction | the queue of this file | queue | "nah splitting inputbinidng now is such a huge change, explain in more detail, that could set us back to zero", then "yes, on input i will not approve that change in isolation, must have thought through design so prepare that, adhoc recommondations are forbidden"
- 2026-11-24 | direction | the queue of this file | queue | "never lower the quality - just reminder"
- 2026-11-25 | direction | the queue of this file | RowContainers | "1 seems more aligned, but make sure that all use cases are satified, including where bulk edit say row gets validated. I would like to see each use case form here before you action it." "Its very critical to get this right with high quality code and keep grammer and dev experience." "Bulk Edit is not about checking one row alone, it has two use cases, if action is in row, then row validate, if its applying to all rows then all validate." "write these use cases a md document, take this to fable xhigh designer, ask it to deliver on correctness, grammer consistency, quality, typed and your document must contain small but complete use examples. so design from fable is expected to be complete, then it gets tla+ and then it lands without loosing quality"
<!-- /notes -->

## Next
<!-- next -->
### Open Fusion promises

The 2026-11-26 tracing hold stands. PR #247 ships the core, Fusion and selected Native slices.
Use the landed shared API. Land on `feature/rewrite` and retain worktrees.

**1. Start.** Read `AGENTS.md` and `.claude/rules/process.md`, source `scripts/toolchain-env.sh`, set
`JAVA_HOME=/opt/homebrew/opt/openjdk`, and run `scripts/doctor.sh` and `python3 scripts/state.py`. Read this handoff whole. Use
`/Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/rewrite` with uppercase Code.

**2. Core.** U1 to U6 and U8 to U22 are closed in `spec/REWRITE.md` section 4. The last core landing passed
853 of 853. CI at 39de1909b passed 10 of 10 jobs, including net48 and both consumer hosts.

**3. U7, held.** Eight tests and code stand. The open wiring, span docs, console harness and net48
traceparent proof are in `.worktrees/fresh-app-docs/.landing/FreshAppDocs/design.md` sections 2.1 and 2.2.
The design is unrun at 3a8ec7120 and backed up on origin at `backup/rewrite/fresh-app-docs`. Await the owner.

**4. After the core: Fusion and Native (U23 to U27).** U23 has 307 of 307 placed tests, U24 57 of 57, U25 38 of 38,
U26 114 of 123, and U27 is closed with all 4 placed browser cases passing. M23.X1 and X4 prove literal notes
and oldest-first dismissal. M23.X2 and X3 prove the confirmation text and localized layout.
   1. Fusion shell proofs include the selected command (X1032), Residents visibility (X1024), ward trail
      item (X1002) and menu focus on opening and Escape (X406).
      M21.X1110 proves the NumericTextBox's explicit and configured steps on net10. Both defects were caught.
   2. FusionGrid has eight open cases. The retained Evening Handoff page at `724901000` matched all three
      filenames, then all three M21.X201 diagnostics saw Exported where they expected Exporting. EJ2 33.1.47
      resolves XLSX before the async save starts the download. CSV and PDF
      dispatch their links before settlement. Blob mode only returns data. The later Playwright read misses
      initiation. A sync declaration stops the pipeline under D11.3 and leaves Exporting. All three stay open.
      The three M21.X203 refusals and two M21.X205 inline edits need the validator's rules at
      `GridColumn.ValidationRules`. N16.2 registers them internally, N16.3 checks a request container, and the
      public validator returns check results. The typed projection is absent. X203_0's opposite and X203_1
      also disagree on whether the same unconditional GreaterThan(0) renders or is refused.
   3. Smart Paste net10 proves the protected POST and late slot mount. M21.X803_1 needs Fusion assets, a
      protected MVC 5 intake route and fixture, and IIS Express for its proofs.

### Full Fusion onboarding

Owner, 2026-11-27: "One complete component first." FusionSwitch has 13 new net10 cases and 29 caught defects.
NumericTextBox increment landed. The decrement draft stays at `.worktrees/fusion-numeric-decrement`.

**Next slice.** Continue Switch from the pushed base. Start with C13 in `spec/FusionComponents/audit/switch-coverage.md`
and verify the public DOM-value carrier against pinned source and a visible typed read. Then work through the other
high-confidence open members. Read the slice spec and project rules, `onboard-component` and its `references/full-api.md`, and `prove-behavior`.

**Definition of done.** One builder inventories the complete pinned API, inherited operations and reachable payloads.
Every eligible operation needs typed C#, matching host manifests and exact browser and defect evidence. Record sourced
exclusions and open operations. Skip EJ2's `created` across Fusion. Prepare the gesture, opposite and plants before building.

After Switch, brief three builders on separate components and worktrees. Prioritize inputs, then editable grids.
Overlap source and case preparation. Keep builds serial with net10 browser proofs and both shipped target builds.
The lead owns spec placement and generated files and reconciles shared manifest edits against the pushed base.
Review each stopped builder at the source. Land with `scripts/land-unit.sh`, `scripts/full-check.sh` and `scripts/push.sh`
in order, verify the clean lane against origin, and review and improve the handoff after each pushed wave.

**5. Gaps named, not built:** an object held as a dictionary's value in a JSON body on MVC 5 (no probe of MVC 5's
dictionary binding); `Console` false has no browser promise; the fresh MVC 5 app's net48 consumer fixture fails on this
Mac for "-tl:off" (CI's Windows job runs it). The net48 `OpenTelemetryExporter` belongs to the held tracing work.
<!-- /next -->
