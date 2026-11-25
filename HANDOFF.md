# Handoff

Written at `date`: Wed Nov 25 17:13:57 EST 2026

## State, as `scripts/state.py` reads it from git
note: open: 29, each in the notes block below
unit: landed (52): BodilessCheck, CheckoutTrap, SpecCiCold, MacTlc, ContainerOnly, CoreLeads, NarrowedPlan, DefinitionOfDone, DesignCounts, DesignedAsks, DesignerClaims, MultipartFileList, NativeDateValue, FusionAudit, FusionComboBoxFollowThrough, FusionOnboardingNotes, FusionComboBoxPayload, InputForm, InputSent, InputSentForms, ListFromSources, ListPosition, MarkdownLand, MistakesAsRules, MistakesCarried, MoveInDoorMap, MoveInAvailability, MoveInContacts, MoveInCoordinators, MoveInDefaults, MoveInDepositDiet, MoveInDiscard, MoveInDocuments, MoveInDraftAdmission, MoveInMedicationsPharmacy, MoveInNavigationReadiness, MoveInOwners, MoveInPayerPricing, MoveInReception, MoveInReviewAcceptance, MoveInSessionScope, Net48BrowserHost, Net48TwinFollowThrough, Operators, QueueReach, RetirePicomatchException, SentPosition, SplitValue, TextBodyCheck, WriterMove, X37Net48, ZshModifier
unit: building (2): FormDataBoundary, MoveInOwners
unit: unbriefed (19): NativeDateInput, FusionApiOperations, IntegrationPrep, FusionFormBuilder, FusionGridApi, GridBulkActions, GridCellReview, FusionGridColumns, FusionGridContract, GridEditorApi, FusionGridExports, GridLargeCensus, GridQueryApi, FusionGridRemote, FusionGridSelection, GridWardTransfer, FusionQueryBuilder, TanStackCharts, TanStackTable
unit: returned (5): FusionComboBoxDataManager, FusionComboBoxEquipmentAvailability, FusionComboBoxLiveCatalogue, FusionComboBoxOnboardingReplay, FusionComboBoxServerSearch
unit: waiting (2): RowContainers, ValidationForms
Each unit's worktree, reason and next command: `python3 scripts/state.py`.
lane: pushing at bfe8076c8 on feature/rewrite
  leaves by: the last line `scripts/push.sh` prints
  exit line: "remote equals HEAD"
  goes to: lane: pushed
ci: unread at bfe8076c8
  leaves by: `python3 scripts/ci.py`
  exit line: "ci at" the commit, with each job
  goes to: ci: running, ci: failed or ci: passed
file: over spec/CLAUDE.md: holds 2115 lines, its limit 150
  leaves by: a unit that splits or cuts it; the mistakes are condensed, kinds merged, each line short enough to follow
  exit line: "landed"
  goes to: gone
file: over tests/Alis.Reactive.Conformance.App/CLAUDE.md: holds 201 lines, its limit 150
file: over tests/Alis.Reactive.Conformance.PlaywrightTests/CLAUDE.md: holds 237 lines, its limit 150

## Where it stands
- Branch: feature/rewrite
- HEAD: bfe8076c8 spec (MoveInDoorMap): the move-in door map names 141 doors, Form gone with ContainerOnly; a worker lands once its turn ends
- Full check: .work/logs/spec-check-bfe8076c8.log: spec check at bfe8076c8: 40 of 40 checks pass
- .work/last-run.txt: 20261125-164038-7773 49dd77295

## Landed since the commit before the last spec: STATE commit
bfe8076c8 spec (MoveInDoorMap): the move-in door map names 141 doors, Form gone with ContainerOnly; a worker lands once its turn ends
2dcfabb3e spec: STATE and handoff at 49dd77295

## Open
open: 18 promises with no browser test, 0 runtime cases not built, 14 doors not implemented
Each, by file and line: `python3 scripts/open.py`.

## Mistakes counted since the last push, by kind: a kind that grew names a fix to its tool or its rule file
- A rule or a tool's list not applied before the change: 473 to 474

## The owner's latest decisions
spec/CLAUDE.md:9: - The owner, 2026-11-25 (verbatim in `spec/NOTES.md`): one concept on the developer's surface, the request container.
spec/CLAUDE.md:13: - The owner, 2026-11-25 (verbatim in `spec/NOTES.md`): the container takes care of hidden fields in the browser, since

## The owner's notes that are open: date | kind | home | follow-up | the owner's words
<!-- notes -->
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
- 2026-11-25 | decision | spec/CLAUDE.md, the validation decisions | FormDataBoundary | "C (Recommended)", to D16.5's question of a member sent outside the body (form-data-boundary-design.md, section 6)
- 2026-11-25 | direction | the queue of this file | ValidationForms | "1 seems more aligned, but make sure that all use cases are satified, including where bulk edit say row gets validated. I would like to see each use case form here before you action it." "Its very critical to get this right with high quality code and keep grammer and dev experience." "Bulk Edit is not about checking one row alone, it has two use cases, if action is in row, then row validate, if its applying to all rows then all validate." "write these use cases a md document, take this to fable xhigh designer, ask it to deliver on correctness, grammer consistency, quality, typed and your document must contain small but complete use examples. so design from fable is expected to be complete, then it gets tla+ and then it lands without loosing quality"
- 2026-11-25 | decision | spec/CLAUDE.md, the validation decisions | ValidationForms | "i approve it", to the five lines of .worktrees/validation-forms/.landing/ValidationForms/design.md, section 10
- 2026-11-25 | decision | spec/CLAUDE.md, the validation decisions | FormDataBoundary | "if a input is part of body and validator on server already works great. and if dev also sent in query, it dev business logic how it will be validated. There is no solution around this problem is there. so i lean toward no"; then "yes lets go with no. and note down the quirk", to the design's section 8 (a query member carried in the body too): not built, and the quirk written where D16.5 and the Query door's XML doc state it
<!-- /notes -->

## Next
<!-- next -->
### Next session: the framework package to 1.0

The owner's order, 2026-11-23: the team process (landed), then the framework core to 1.0, then everything else. Every
unit lands on `feature/rewrite`; its one pull request, #247, ships 1.0 (the owner, 2026-11-23). The worktrees of
landed units stay (the owner, 2026-11-23: "keep worktree around"). Builds stay serial on this Mac; source
`scripts/toolchain-env.sh` first. A unit of Markdown alone lands by `scripts/land.sh`'s Markdown path since 1bce0ab2e
(the owner, 2026-11-24: "for md file changes u do not need whole flow to run").

1.0's definition of done is the owner's (2026-11-23): "1.0 DoD is when all components of fusion, and selective native,
and core framework are done where fusion will be onboarded one component with its full powers using skill. right now
focus is on getting framework package to 1.0". So the focus now is the framework package, the core (U1 to U22).

**1. Start inside the lane.** `cd /Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/rewrite && claude`:
the lane's agents, hooks and rules load only there (the 2026-11-25 session started at the root, a mistake). First spawn
`builder` with a two-line brief: print `git rev-parse HEAD`, change nothing; any commit but the lane's head means
`worktree.baseRef` is not applied.

**2. The framework core to 1.0 (U1 to U22).** Done when:
- `scripts/open.py` prints no core runtime case: met at b824783d3, "0 runtime cases not built";
- X3.7, a date in a JSON response on MVC 5, passes on CI's Windows job;
- `spec/REWRITE.md` section 4 states each of U1 to U22 closed with its result line (today it states U3, U4, U5,
  U8 to U13 closed and U1, U2, U14 to U16 landed; U6, U7, U17 to U22 are not stated);
- the core's public surface text is read as the 1.0 surface, and README's examples come from tested pages.
Its units, in order:
1. The plan narrowed to what C# writes: landed in six units, the last at 5d9328da2 (ListPosition, choice A of
   2026-11-24, X9.43 to X9.46). Its gap: an empty list or a files input with no file sends no part, so an item whose
   only field is one of these still ends the list.
2. The forms, four units, one at a time, each its spec lines, TLA+, tests and code in one commit, as ListPosition
   landed at 5d9328da2. Every design question is answered and every probe ran before its build (2026-11-25), each
   brief naming what it rests on, so no question goes to the owner unless a run prints other than its brief states.
   Each worktree and its .landing is backed up as backup/rewrite/<worktree> on GitHub, rerun before a session ends:
   `bash .work/tools/backup-worktree.sh ../<worktree> <worktree>`.
   - ContainerOnly landed at 49dd77295, "839 selected, 839 ran, 839 passed", spec check 40 of 40: one request container,
     its hidden inputs sent in a form body (the owner, 2026-11-25). A unit whose paths hold scripts/land-unit.sh lands from
     a copy, `cp ../<worktree>/scripts/land-unit.sh .work/land-unit-run.sh`, since cp rewrites the script bash is reading.
   - FormDataBoundary, ../form-data-boundary, next: ContainerOnly is landed, so it moves first by its move.sh to the
     lane's head; ValidationForms, ../validation-forms, After it; RowContainers, ../row-containers, After ValidationForms.
   A waiting unit moves once the unit before it is landed and pushed, in its worktree by one command,
   `bash .landing/<Name>/move.sh <the lane's head>`: spec/MISTAKES.md from HEAD, the head checked out, npm ci, the
   lead's counts re-applied by counts.py beside it. Then its brief's "At the base:" commands, a `builder` on Opus at
   xhigh with the brief, read.md, `scripts/land-unit.sh`, `scripts/full-check.sh`, `scripts/push.sh`, `scripts/ci.py`.
3. X3.7 on the net48 host: landed at c3aeae654, proven by CI's Windows job at f2986ea66 ("ci at f2986ea66: 10 of 10
   jobs passed"; verify-net48 "Passed the_date_of_birth_shows_its_own_day_in_a_browser_whose_clock_is_behind_the_servers").
4. Eight parked core leads: judged at cad8e3581 (CoreLeads), two in, one closed, five after 1.0. In: typed list
   construction from separate sources, landed at a00125348 (ListFromSources, X7.21 and X7.22); a named rule set for a
   check with no request, N16.3, which ValidationForms builds as F5's door. Closed: plan shape alignment. After 1.0, as
   rows of the table in spec/NOTES.md: the named enum in a pushed message; activated-row replacement, a row's
   "removing" state and handler cleanup; the attached-partial validation summary.
5. Core freeze: section 4 rewritten unit by unit, the surface read, README checked. One gap is decided first: a 204 to
   a JSON route is a content failure, since response.ts reads no Content-Type as no JSON before it reads the body, while
   an empty JSON body reads as none, and no D line states a 204 under `OnSuccess<T>` (a search of spec/Core/DESIGN.md
   for "204" and "No Content" printed nothing; raised by ListFromSources' builder, read at the source in its read.md).

**3. After the framework package:** the selected Native components (date entry first), then every Fusion component,
one at a time with its full powers through `.claude/skills/onboard-component/SKILL.md`, then PR #247 out of draft. The six packages pack at one version, so the tag comes once, at the end.

**Parked, each where it lives:** browser validation's shared key
(`spec/Validation/browser-validation-proposal.md`, the owner's condition stands); TanStack Table
(`../tanstack-table`, uncommitted) and Charts (`../tanstack-charts`, a brief); Form Builder and Query Builder
(their pushed branches); the Fusion Grid API worktree (`../fusion-grid-api`, uncommitted); five ComboBox
units whose replies wait for a read. The history this block held is the HANDOFF.md of the previous STATE commit.

<!-- /next -->
