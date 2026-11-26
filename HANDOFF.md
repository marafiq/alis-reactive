# Handoff

Written at `date`: Thu Nov 26 14:58:48 EST 2026

## State, as `scripts/state.py` reads it from git
note: open: 26, each in the notes block below
note: closed 2026-11-25 | direction | the queue of this file | RowContainers | "1 seems more aligned, but make sure that : its follow-up RowContainers landed
unit: landed (63): BodilessCheck, CheckoutTrap, SpecCiCold, MacTlc, ContainerOnly, CoreLeads, NarrowedPlan, DefinitionOfDone, DesignCounts, DesignedAsks, DesignerClaims, EmptyTextItem, FormDataBoundaryNet48, FormDataBoundary, MultipartFileList, NativeDateValue, FusionAudit, FusionComboBoxFollowThrough, FusionOnboardingNotes, FusionComboBoxPayload, InputForm, InputSent, InputSentForms, JsonNoContent, ListFromSources, ListPosition, MarkdownLand, MistakesAsRules, MistakesCarried, MistakesShort, MoveInDoorMap, MoveInAvailability, MoveInContacts, MoveInCoordinators, MoveInDefaults, MoveInDepositDiet, MoveInDiscard, MoveInDocuments, MoveInDraftAdmission, MoveInMedicationsPharmacy, MoveInNavigationReadiness, MoveInOwners, MoveInPayerPricing, MoveInReception, MoveInReviewAcceptance, MoveInSessionScope, Mvc5Razor, Mvc5Roster, Mvc5ViewsCompiled, Net48BrowserHost, Net48TwinFollowThrough, OneDoor, Operators, QueueReach, RetirePicomatchException, RowContainers, SentPosition, SplitValue, TextBodyCheck, ValidationForms, WriterMove, X37Net48, ZshModifier
unit: ready (3): FlagsAttribute, G13Forms, Section4Close
unit: unbriefed (19): NativeDateInput, FusionApiOperations, IntegrationPrep, FusionFormBuilder, FusionGridApi, GridBulkActions, GridCellReview, FusionGridColumns, FusionGridContract, GridEditorApi, FusionGridExports, GridLargeCensus, GridQueryApi, FusionGridRemote, FusionGridSelection, GridWardTransfer, FusionQueryBuilder, TanStackCharts, TanStackTable
unit: returned (5): FusionComboBoxDataManager, FusionComboBoxEquipmentAvailability, FusionComboBoxLiveCatalogue, FusionComboBoxOnboardingReplay, FusionComboBoxServerSearch
unit: building (1): MoveInOwners
Each unit's worktree, reason and next command: `python3 scripts/state.py`.
lane: pushed at 03c79d4aa on feature/rewrite: equal to the remote
  leaves by: the landing of a unit that is ready
  read first: the queue of `HANDOFF.md`
  exit line: "landed"
  goes to: lane: landed
ci: unread at 03c79d4aa
  leaves by: `python3 scripts/ci.py`
  exit line: "ci at" the commit, with each job
  goes to: ci: running, ci: failed or ci: passed
file: over spec/CLAUDE.md: holds 2148 lines, its limit 150
  leaves by: a unit that splits or cuts it; the mistakes are condensed, kinds merged, each line short enough to follow
  exit line: "landed"
  goes to: gone
file: over HANDOFF.md: holds 153 lines, its limit 150
file: over tests/Alis.Reactive.Conformance.App/CLAUDE.md: holds 201 lines, its limit 150
file: over tests/Alis.Reactive.Conformance.PlaywrightTests/CLAUDE.md: holds 237 lines, its limit 150
machine: review due : 10 units have landed since .claude/rules/process.md last changed
  leaves by: a unit that changes this file, the lead's without waiting for the owner, the framework's quality bar never lowered
  read first: the units sent back, the kinds of mistake that grew, the states no unit met
  exit line: "landed"
  goes to: gone

## Where it stands
- Branch: feature/rewrite
- HEAD: 03c79d4aa harness (Mvc5ViewsCompiled): MVC 5 compiles every view of the twin before the browser tests start
- Full check: .work/logs/spec-check-03c79d4aa.log: spec check at 03c79d4aa: 40 of 40 checks pass
- .work/last-run.txt: 20261126-143305-40967 03c79d4aa

## Landed since the commit before the last spec: STATE commit
03c79d4aa harness (Mvc5ViewsCompiled): MVC 5 compiles every view of the twin before the browser tests start
99a67ded1 spec: STATE and handoff at 9f1f3a2b2

## Open
open: 18 promises with no browser test, 0 runtime cases not built, 14 doors not implemented
Each, by file and line: `python3 scripts/open.py`.

## The owner's latest decisions
spec/CLAUDE.md:9: - The owner, 2026-11-26 (verbatim in `spec/NOTES.md`), on the forms gate 2's G13 left to the owner: yes to
spec/CLAUDE.md:15: - The owner, 2026-11-26 (verbatim in `spec/NOTES.md`): one door for each list operation. An object JavaScript iterates
spec/CLAUDE.md:19: - The owner, 2026-11-26 (verbatim in `spec/NOTES.md`): the mistakes log stays short enough to stay in context, each kind

## The owner's notes that are open: date | kind | home | follow-up | the owner's words
<!-- notes -->
- 2026-11-26 | decision | spec/CLAUDE.md, the G13 forms | G13Forms | "1-will dev have to type one of those every sngle time? 2-ok 3-ok, non issue. 4-where it tell its? 6-ok, does dev experience degraded."
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
### Next session: the framework package to 1.0

The owner's order, 2026-11-23: the team process (landed), then the framework core to 1.0, then everything else. Every
unit lands on `feature/rewrite`; its one pull request, #247, ships 1.0; landed units' worktrees stay (the owner,
2026-11-23). Builds stay serial on this Mac; source `scripts/toolchain-env.sh` first. A unit of Markdown alone lands
by `scripts/land.sh`'s Markdown path (the owner, 2026-11-24). 1.0's definition of done (the owner, 2026-11-23): "1.0
DoD is when all components of fusion, and selective native, and core framework are done where fusion will be onboarded
one component with its full powers using skill. right now focus is on getting framework package to 1.0".

**1. Start inside the lane,** `cd /Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/rewrite && claude`,
spelled Code as on disk: from Documents/code, bash's pwd -P and Python's getcwd differ in case and state.py finds no
landing folder, so land-unit.sh runs from a shell cd'd to the Code spelling (2026-11-26).

**2. The framework core to 1.0 (U1 to U22).** Done when `scripts/open.py` prints no core runtime case (met at
b824783d3); X3.7 passes on CI's Windows job (met at f2986ea66); `spec/REWRITE.md` section 4 states each of U1 to U22
closed with its result line (U3, U4, U5, U8 to U13 closed, U1, U2, U14 to U16 landed, U6, U7, U17 to U22 not stated);
the core's surface text is read as the 1.0 surface and README's examples come from tested pages. Its units, in order:
1. The plan narrowed to what C# writes: landed, the last at 5d9328da2 (ListPosition). Its gap: an empty list or a files
   input with no file sends no part, so an item whose only field is one of these still ends the list.
2. The forms, one unit at a time, each its spec lines, TLA+, tests and code in one commit; every design question is
   answered, so nothing goes to the owner unless a run prints other than its brief states.
   - Landed: ContainerOnly at 49dd77295 (one request container, its hidden inputs in a form body); FormDataBoundary at
     15b480e1e, "842 selected, 842 ran, 842 passed, 0 failed, 0 over budget", spec check 40 of 40 (choice C of D16.5,
     the quirk in the Query doors' XML doc, X16.46 4, 7 and 8 net48 alone, the twin's DELETE through IIS Express).
   - Landed: ValidationForms at 96955defa, "845 selected, 845 ran, 845 passed, 0 failed, 0 over budget", spec check
     40 of 40 (pipeline.Validate(container, ruleSet); a set no rule runs under refused; X16.48 to X16.50).
   - Landed on 2026-11-26: MistakesShort f3e2c7d9d, OneDoor ca7055154 (Iterables' forwarders gone), FormDataBoundaryNet48
     4f1a073cb, RowContainers 7b4d6c306, JsonNoContent ddf83d814 (X10.30), Mvc5Roster 9ccf7a367 (a container under no
     prefix reads no JSON options; the twin logs a page's exception to run-hosts/iis.err.log), EmptyTextItem 3710785c9
     (BoundList.emptyField, X16.57), Mvc5Razor 6feddb6e3 (no "@for" inside a code block), Section4Close 720499148
     (U6, U7, U19 to U21 closed). The last land: "853 selected, 853 ran, 853 passed"; CI at 57b8d0fd3 verify-net48
     "33 selected, 33 ran, 32 passed, 1 failed", least-dose alone.
   - Open, the owner's decision put on 2026-11-26: least-dose's MVC 5 navigation outlasts the 704 ms wait while IIS
     Express compiles its view at first render (its GET the run's last logged line, no exception); the lead recommends
     scripts/playwright-windows.ps1 precompiling the twin in place with aspnet_compiler before IIS Express starts.
   - Open: U18, a whole object a JSON body sends on MVC 5 is named by its JSON names where the endpoint binds C# names
     (the requests module), and X10.2's and X10.29's endpoints answer constants against the app's CLAUDE.md:19; U22,
     README read at the freeze; the 1.0 surface read with the six G13 forms.
   A waiting unit moves once the one before it is landed and pushed: `bash .landing/<Name>/move.sh <the lane's head>`
   in its worktree, then its brief's "At the base:" line rewritten at that head, a low-effort probe, and its builder: a
   general-purpose agent with no name and no isolation, its first read .claude/agents/builder.md, on Opus at xhigh
   (.claude/skills/delegate/SKILL.md). Then read.md, the unit staged and `scripts/check.sh --no-spec` in its
   worktree, `scripts/land-unit.sh`, `scripts/full-check.sh`, `scripts/push.sh`. A unit whose paths hold
   scripts/land-unit.sh lands from a copy, `cp ../<worktree>/scripts/land-unit.sh .work/land-unit-run.sh`.
   Each unlanded worktree is backed up
   before a session ends: `bash .work/tools/backup-worktree.sh ../<worktree> <worktree>`.
3. Eight parked core leads, judged at cad8e3581 (CoreLeads): typed list construction landed at a00125348; N16.3, a
   named rule set for a check with no request, is ValidationForms' F5; five after 1.0, as rows of spec/NOTES.md's table.
4. A divergence read at the source (2026-11-26), decided before the freeze: MVC 5 binds an empty item of a list of
   texts as an empty text (spec/probes/aspnetwebstack/ValueProviderResult.cs.txt:50-52), while forms.ts:220 reads it
   as null in form data and DESIGN.md:1827's table, citing mvc5-empty-list-item-null, says null for text; the fact
   holds for nullable values alone (:156, :57). The JSON arm on MVC 5 meets it for a list query member whose item reads
   nothing. What net10 binds for an empty text item no probe has read: probe:core-query-nullable-enum-items reads enums.
5. Core freeze: section 4 rewritten unit by unit, the surface read, README checked. Decided first: a 204 to a JSON route
   is a content failure, since response.ts reads no Content-Type as no JSON before it reads the body, and no D line
   states a 204 under `OnSuccess<T>` (a search of spec/Core/DESIGN.md for "204" and "No Content" printed nothing).

**3. After the framework package:** the selected Native components (date entry first), then every Fusion component,
one at a time with its full powers through `.claude/skills/onboard-component/SKILL.md`, then PR #247 out of draft.
The six packages pack at one version, so the tag comes once, at the end.

**Parked, each where it lives:** browser validation's shared key (`spec/Validation/browser-validation-proposal.md`,
the owner's condition stands); TanStack Table (`../tanstack-table`, uncommitted) and Charts (`../tanstack-charts`, a
brief); Form Builder and Query Builder (their pushed branches); the Fusion Grid API worktree (`../fusion-grid-api`,
uncommitted); five ComboBox units whose replies wait for a read. Older history is the previous STATE commit's HANDOFF.md.

<!-- /next -->
