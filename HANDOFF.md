# Handoff

Written at `date`: Tue Nov 24 22:42:06 EST 2026

## State, as `scripts/state.py` reads it from git
note: open: 25, each in the notes block below
unit: landed (43): BodilessCheck, SpecCiCold, MacTlc, NarrowedPlan, DefinitionOfDone, DesignCounts, DesignedAsks, DesignerClaims, MultipartFileList, NativeDateValue, FusionAudit, FusionComboBoxFollowThrough, FusionOnboardingNotes, FusionComboBoxPayload, InputSent, ListPosition, MarkdownLand, MistakesCarried, MoveInAvailability, MoveInContacts, MoveInCoordinators, MoveInDefaults, MoveInDepositDiet, MoveInDiscard, MoveInDocuments, MoveInDraftAdmission, MoveInMedicationsPharmacy, MoveInNavigationReadiness, MoveInOwners, MoveInPayerPricing, MoveInReception, MoveInReviewAcceptance, MoveInSessionScope, Net48BrowserHost, Net48TwinFollowThrough, Operators, QueueReach, RetirePicomatchException, SentPosition, SplitValue, TextBodyCheck, WriterMove, ZshModifier
unit: unbriefed (19): NativeDateInput, FusionApiOperations, IntegrationPrep, FusionFormBuilder, FusionGridApi, GridBulkActions, GridCellReview, FusionGridColumns, FusionGridContract, GridEditorApi, FusionGridExports, GridLargeCensus, GridQueryApi, FusionGridRemote, FusionGridSelection, GridWardTransfer, FusionQueryBuilder, TanStackCharts, TanStackTable
unit: returned (7): FusionComboBoxDataManager, FusionComboBoxEquipmentAvailability, FusionComboBoxLiveCatalogue, FusionComboBoxOnboardingReplay, FusionComboBoxServerSearch, InputForm, InputSentForms
unit: building (1): MoveInOwners
Each unit's worktree, reason and next command: `python3 scripts/state.py`.
lane: pushed at 45b14dc30 on feature/rewrite: equal to the remote
  leaves by: the landing of a unit that is ready
  read first: the queue of `HANDOFF.md`
  exit line: "landed"
  goes to: lane: landed
ci: unread at 45b14dc30
  leaves by: `python3 scripts/ci.py`
  exit line: "ci at" the commit, with each job
  goes to: ci: running, ci: failed or ci: passed
file: over spec/CLAUDE.md: holds 2108 lines, its limit 150
  leaves by: a unit that splits or cuts it; the mistakes are condensed, kinds merged, each line short enough to follow
  exit line: "landed"
  goes to: gone
file: over tests/Alis.Reactive.Conformance.App/CLAUDE.md: holds 201 lines, its limit 150
file: over tests/Alis.Reactive.Conformance.PlaywrightTests/CLAUDE.md: holds 237 lines, its limit 150

## Where it stands
- Branch: feature/rewrite
- HEAD: 45b14dc30 land: every unit's mistake counts are carried from its worktree, and a spec.py that edits one is refused
- Full check: .work/logs/spec-check-45b14dc30.log: spec check at 45b14dc30: 40 of 40 checks pass
- .work/last-run.txt: 20261124-223246-3320 45b14dc30

## Landed since the commit before the last spec: STATE commit
45b14dc30 land: every unit's mistake counts are carried from its worktree, and a spec.py that edits one is refused
4e65460bb spec: STATE and handoff at 5d9328da2

## Open
open: 19 promises with no browser test, 0 runtime cases not built, 14 doors not implemented
Each, by file and line: `python3 scripts/open.py`.

## Mistakes counted since the last push, by kind: a kind that grew names a fix to its tool or its rule file
- Written from memory, not read at its source at HEAD: a design line, a signature, a path, a name, a page's expected value, a case's shape or a TLA+ ordering: 453 to 461
- A rule or a tool's list not applied before the change: 448 to 452
- A shell command or a script wrong in its own terms: 649 to 655

## The owner's latest decisions
spec/CLAUDE.md:9: - The owner, 2026-11-24 (verbatim in `spec/NOTES.md`), yes to choice A for a field left out under a list index: a field
spec/CLAUDE.md:2036: - The owner, 2026-11-24 (verbatim in `spec/NOTES.md`), to the design of an input's binding in the plan, put as one

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

**1. The builder's worktree, first.** In a session started inside the lane, spawn the `builder` agent with a two-line
brief: print `git rev-parse HEAD` and change nothing. It prints the lane's head; any other commit means
`worktree.baseRef` is not applied, and that is fixed before a builder takes work. A session started at the
repository's root lists no `builder` and reads the root's `.claude/settings.json`, which holds no `worktree.baseRef`,
so the probe proves nothing there; the sessions that landed DefinitionOfDone and MarkdownLand started at the root
and left it, the second with its commands run through the traps hook by hand (counted at 1bce0ab2e).

**2. The framework core to 1.0 (U1 to U22).** Done when:
- `scripts/open.py` prints no core runtime case: met at b824783d3, "0 runtime cases not built";
- X3.7, a date in a JSON response on MVC 5, passes on CI's Windows job;
- `spec/REWRITE.md` section 4 states each of U1 to U22 closed with its result line (today it states U3, U4, U5,
  U8 to U13 closed and U1, U2, U14 to U16 landed; U6, U7, U17 to U22 are not stated);
- the core's public surface text is read as the 1.0 surface, and README's examples come from tested pages.
Its units, in order:
1. The plan narrowed to what C# writes: landed in six units, the last at 5d9328da2, ListPosition, the owner's yes of
   2026-11-24 to choice A: a field left out under a list index that leaves its item no field stops the request, its
   container's read and check with it ("leaveUnlessAlone" on WhenBlank), or is refused where the view renders a query or
   form data member that could empty its item (X9.43 to X9.46, each side failing on its own plant;
   ContainerRead.tla's BlankAloneStopsTheSendAlone and ReachBlankAloneStopsTheRead). Its gap, named in its reply: an
   empty list or a files input with no file sends no part, so an item whose only field is one of these still ends the
   list; choice A covers whenBlank and whenNothing alone.
2. The form-data boundary: an enum input in form data is sent by its rows since b824783d3 (the design's defect 1,
   X9.36, X9.37, X9.39 and X16.47), an object stays refused until after 1.0 (the owner's yes of 2026-11-17); a text or file body's check landed
   at 6da8232ec (X16.46). On MVC 5 an action's model binds route values and the query beside the body
   (probe mvc5-value-provider-order), so a rule over a member a request sends in its path or query is checked at its
   start in the browser and bound on that endpoint; the meaning by host is designed whole and put to the owner
   before the core freeze (D16.5, D16.6).
3. X3.7 written on the net48 host; CI's Windows job proves it.
4. Eight parked core leads, each one verdict by the owner's test of 2026-11-20 (a reproduced defect, a reachable
   incomplete contract or an unproved promise is in; otherwise a row of the after-1.0 table), the raw text and
   single-file body check having landed at 6da8232ec as X16.46: the named enum in a pushed message, typed list construction from separate
   sources, plan shape alignment, standalone named rule sets, activated-row replacement, handler cleanup, the
   attached-partial validation summary, and a row's "removing" state after a failed removal (move-in part 9). An
   input over an enum whose zero has no name landed at b824783d3 as X9.39.
5. Core freeze: section 4 rewritten unit by unit, the surface read, README checked.

**3. After the framework package:** the selected Native components (date entry first), then every Fusion component,
one at a time with its full powers through `.claude/skills/onboard-component/SKILL.md`, then PR #247 out of draft. The six packages pack at one version, so the tag comes once, at the end.

**Parked, each where it lives:** browser validation's shared key
(`spec/Validation/browser-validation-proposal.md`, the owner's condition stands); TanStack Table
(`../tanstack-table`, uncommitted) and Charts (`../tanstack-charts`, a brief); Form Builder and Query Builder
(their pushed branches); the Fusion Grid API worktree (`../fusion-grid-api`, uncommitted); five ComboBox
units whose replies wait for a read. The history this block held is the HANDOFF.md of the previous STATE commit.

<!-- /next -->
