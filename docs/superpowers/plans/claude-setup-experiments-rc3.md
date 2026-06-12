# Claude Setup Experiments — rc3 Readiness

Experiments run 2026-06-10 against the new harness (nested CLAUDE.mds, two
PreToolUse hooks, one paths-scoped rule, restructured rules corpus) before the
rc3 loop is allowed to trust any of it. Every claim below was observed in this
repo, not inferred from docs. Where docs and observation disagreed, observation
won and the guidance files were corrected.

## The setup under test

| Tier | Mechanism | Contents |
|------|-----------|----------|
| Deterministic | PreToolUse hooks (`.claude/settings.json` → `.claude/hooks/*.mjs`) | protect generated files (`runtime/types/plan.ts`, onboarding `discovery/*.json` + `traces/*.trace.json`); force `scripts/playwright.sh` over raw `dotnet test` |
| Always-loaded | root `CLAUDE.md` (517) + `process-pipeline.md` (58) + `process-task-types.md` (116) + `agent-dispatch.md` (208) | 899 lines, down from 1,126; one canonical 5-layer model; live contradictions fixed |
| Lazy | 6 nested CLAUDE.mds + `plan-contract-boundary.md` (paths-scoped) | directory invariants + boundary rituals, load on file touch |

## Experiments and observed results

| # | Experiment | Method | Observed | Implication for rc3 |
|---|-----------|--------|----------|---------------------|
| E0 | Hook scripts, 12 unit cases | piped PreToolUse JSON into both scripts | 12/12 — plan.ts and generated JSON/traces DENY; judgment `.md` files (pattern map, name decisions, event rows), status JSON, other runtime TS all pass; raw `dotnet test …Playwright…` DENY; wrapper, domain tests, build pass | the skeptic's feared failure (hook blocks same-commit pattern-map write-back) cannot occur — `.md` never matches |
| E1 | Bash hook, real tool call | a Bash command merely *embedding* the forbidden string was issued | hook BLOCKED it mid-session | hooks enforce immediately (no restart); false-positive class: string-matching sees the whole command text — echoing/quoting a forbidden command also blocks. Acceptable: rephrase and continue |
| E2 | Edit hook, real tool call | attempted Edit on `runtime/types/plan.ts` | BLOCKED with the corrected reason text | hook scripts are re-read per invocation — script fixes take effect instantly; settings.json hook wiring also took effect mid-session |
| E3 | Nested CLAUDE.md loading | read a Fusion slice file in the session that CREATED the file → nothing; fresh `claude -p` session, same read | fresh session: injected (model quoted the exact heading); creating session: NOT injected | guidance discovery is snapshotted at session start. The rc3 fresh-context-per-iteration architecture gets every guidance file correctly; a session that writes guidance never sees it itself |
| E4 | `paths:`-scoped rule | same two-session method, reading a `PlanModel/**` file vs a non-matching file | fresh session + matching file: rule injected (heading quoted); non-matching file: NOT injected; creating session: not injected | path-scoping works and does not false-fire; boundary ritual will be present exactly when plan-domain files are touched |
| E5 | rc3 entry gate dry-run | `node .claude/skills/onboard-fusion-component/scripts/report-fusion-onboarding-status.mjs` | mechanically named the state: 51 components, 1 audited (459/459 rows proven), 50 next-staged `static-discovery` | the loop's "name the next red row" entry point works today, unattended |
| E6 | Explore agent context | Explore subagent probed for CLAUDE.md content before/after reading a Fusion file | nested CLAUDE.md ATTACHED after the read | docs' "Explore skips CLAUDE.md" applies to the startup hierarchy only; on-read directory attachment still reaches research agents. Root + artifact-tree guidance corrected to say exactly this |

## Not verified (honest gaps)

- Compaction re-attachment of skills (first 5,000 tokens, 25k shared budget) —
  docs-only claim, will be observed naturally during long rc3 sessions.
- Whether hooks fire for tool calls made by subagents — not yet tested; rc3
  read-only sweeps don't Edit/Write, so exposure is low. Test before any
  write-capable fan-out.
- `paths:` rule loading inside subagents — untested.
- The artifact-tree hook guard ran only against unit pipes + the real tree's
  paths, not yet against a full attended onboarding row. First attended rc3
  rows should confirm zero friction before the loop runs unattended
  (skeptic condition, accepted).

## Decisions taken from the judge pass (drift = top criterion)

- Adopted: two hooks (deterministic > prose); dissolve `process-layers.md`
  (unique content moved to `plan-contract-boundary.md`, `runtime/CLAUDE.md`,
  task-types docs section — both judges agreed); fix four live drift instances
  (4-vs-5 layer models → one canonical 5-layer in root; stale `resolver.ts`
  claim → three vendor roles; 8-step vs 9-step checklist → root Rule 3
  canonical; `PlanTypeGenerator` → `PlanContractGenerator`); dedupe BDD/plan-regen/
  war-story/protocol duplicates to one home each; root slimmed 561→517 with
  worked examples and the aspirations ledger moved to `.claude/memory/` canon.
- Rejected (skeptic won): lazy-converting `process-pipeline`/`process-task-types`/
  `agent-dispatch` — their rules are event-triggered (routing, speed gate,
  dispatch, escalation), not path-triggered; lazy loading would arrive too late.
  Revisit after rc3's verifier has earned trust.
- Deferred, recorded: `modern-csharp` skill split (1,272 lines vs 500 guidance)
  via progressive disclosure; deep root shrink toward ~210; AGENTS.md
  line-level dedupe (Codex consumer — resolved 2026-06-11 by direction:
  AGENTS.md is now a symlink to root `CLAUDE.md`, deleting the
  older-vocabulary duplicate); `bdd-testing` skill description cleanup
  (resolved 2026-06-11 by the nested-vertical-slices rewrite below).

## Re-verification 2026-06-11 (fresh look, observed)

Full evidence, fixes, and the goal work plan: `docs/superpowers/plans/rc3-goal-execution.md`.

Held on re-test: E0 spot re-piped 4/12 cases (plan.ts Edit DENY with corrected
reason; `_skill/pattern-map.md` Edit pass; raw Playwright `dotnet test` DENY;
`scripts/playwright.sh` pass); E5 re-run matched exactly (51 components, 1
audited, 459/459 rows proven); 6 nested CLAUDE.mds present; `paths:` frontmatter
present on `plan-contract-boundary.md`; hooks wired in `.claude/settings.json`.

Corrections:

- Line counts drifted since the log: root `CLAUDE.md` 520 (logged 517),
  always-loaded total 902 (logged 899).
- The drift fix `PlanTypeGenerator` → `PlanContractGenerator` had been recorded
  while only prose changed — an aspiration logged as fact. Ground truth: the
  generating class `Alis.Reactive.PlanModel.PlanContractGenerator` existed; the
  runner project still carried the legacy name (`high-quality-bar-tasks.md` T5:
  "one concept, three names"). RESOLVED 2026-06-11 by realizing the rename on
  disk: `tools/PlanContractGenerator/PlanContractGenerator.csproj`, assembly
  `Alis.Reactive.PlanContractGenerator`, runner namespace
  `Alis.Reactive.ContractGeneration` (the runner must not shadow the domain
  class name — CS0234 proved it); `Alis.Reactive.slnx`, `package.json` script,
  `InternalsVisibleTo`, and `plan-contract-boundary.md` `paths:` updated
  together. Verified: `npm run typecheck` green through the new path,
  regenerated `plan.ts` byte-identical, `dotnet build` 0 warnings 0 errors.
- Blind spot found: the setup-under-test table has no tier for user-level
  `~/.claude/skills/`. Tested 2026-06-11: four stale Alis-specific fusion skills
  (Apr 8) plus `syncfusion-slice` were model-invocable and taught stringly
  `self.Set("prop", v)` / `self.Call("method")` APIs — the exact pattern root
  CLAUDE.md forbids — while the canon skill is `disable-model-invocation: true`
  and therefore never auto-loads. Four more user-level duplicates
  (`conditions-dsl`, `http-pipeline`, `reactive-dsl`, `solid-ts-audit`) differed
  from the project copies under the same names (Mar vs May mtimes).
  All nine archived to `~/.claude/skills/_archive-alis-2026-06-11/`; project
  canon is now the only resolution for these names.
  CORRECTED same day by a blind fresh-session probe: an earlier draft of this
  bullet called ServerPush/SignalR "removed triggers" — DSL source refutes that
  (`TriggerBuilder.cs:65,77` ServerPush, `:102` SignalR, `plan.ts:444`
  `"server-push"`). That inference came from a skill-description diff, not from
  source. The actual gap is the project `reactive-dsl` skill omitting both
  triggers.

## Guidance edits before the restructure, recorded from the diff

A session before the 2026-06-11 root-CLAUDE.md restructure edited guidance
files and wrote no record (the pre-restructure tree was 525 lines against 520
at HEAD).

This entry reconstructs those edits from `git diff`. The diff is the
evidence; the session-time reasoning was not captured.

- Evidence-language pass — a claim carries its output:
  - `agent-dispatch.md` Template 1 now reads "Report each step in past tense
    with what it printed; a claim without its output is not made". Its
    guardrail for claiming done names the gesture performed and what the
    page showed.
  - `agent-dispatch.md` Template 3 reads "A finding without file:line and
    its consequence is not made".
  - `process-pipeline.md` dropped "pragmatic excellence" from its standards
    line; its Speed Gate commit line matches Template 1's done guardrail.
  - Root `CLAUDE.md` Rule 9 gained the past-tense reporting sentence,
    Pre-Flight gained "Rejected alternative named, with the one fact that
    killed it", and Post-Flight opens with "Each checked box carries its
    evidence".
  - Drift this pass left behind: `.claude/memory/quality-principles.md`
    still taught "Root yourself in pragmatic excellence" as the Right
    preamble — fixed 2026-06-11; the example now quotes Template 3.
- Onboarding-skill description — root `CLAUDE.md` skills table and
  `process-task-types.md` now describe `onboard-fusion-component` as
  artifact-gated with a fail-closed verifier, matching the skill after the
  E5 re-run.

## Scope steer 2026-06-11 — domain framing caused compliance drift

The "senior living communities / residents depend on it" framing in prompt
preambles pulls models toward HIPAA, PHI, and healthcare-compliance
reasoning. The framework is UI code; no guidance asks for compliance
analysis.

Every framing site now carries the counter-sentence "The domain names the
stakes, not the scope: this is a UI framework — do not reason about HIPAA,
PHI, or healthcare compliance": `agent-dispatch.md` (header, Templates 1, 3,
4), `process-pipeline.md`, `quality-principles.md` (header and the Positive
Framing example), `vision_working_principles.md`, and the cascade preamble
in `bdd-principles.md`.

Sandbox and onboarding docs keep "Senior Living" as the example domain for
names and workflows — that usage names data, not regulation, and stays.

## bdd-testing rewrite 2026-06-11 — nested vertical slices

Directives: the skill was too prescriptive; the structure is set; one journey
= one isolated, nested vertical slice (own domain model, view, controller
partial, fixture) derived from a senior-living user journey; views carry no
elements that would never appear in a real application page. The Grid
`Billing` slice already practiced the nesting and is named as the exemplar
(`BillingModel.cs`, `GridController.Billing.cs`, `Billing.cshtml`,
`WhenUsingFusionGridBilling.cs` — the journey name joins the four trees).

What changed:

- `SKILL.md` 241 → 124 lines: it now carries the method only — journey →
  slice → criteria → tests. The component gesture/surface/assertion grammar
  moved to `references/gestures.md`. Failure triage already lived in
  `tests/Alis.Reactive.PlaywrightTests/CLAUDE.md`; the skill's copy was
  deleted and the local file gained the `Playwright.Extensions` pointer.
- The frontmatter description (its trigger text was duplicated) was replaced —
  closes the deferred description cleanup above.
- Enforcement home: `memory/bdd-principles.md` gained "Nested Vertical
  Slices" so blind reviewers, who receive only the constitution and the test
  code, enforce it; both cascade preambles (constitution and
  `agent-dispatch.md` Template 4) gained the one-line restate.
- Pointer updates: root `CLAUDE.md` skills row and Rule 12, sandbox
  `CLAUDE.md` view rules.
- The pattern is named: Nested Vertical Slices. A component with many use
  cases fans out into many journeys, each a full slice. Grid proves it at
  scale — about thirty journey views and fixtures, per-journey models,
  per-journey session-keyed stores (`BillingSessionKey`). In-memory data is
  journey-owned; no fake-data class serves two journeys.
- Existing echo-span pages predate the rule; they migrate as they are
  touched (scout rule), not in a mass pass.
- Gap found while grounding the pattern: Schedule is pre-pattern — one
  `Index.cshtml`, one `ScheduleController.cs`, one fixture
  (`WhenUsingFusionSchedule.cs`), and a shared `FakeScheduleData.cs` store.
  Migration target when Schedule is next touched: fan its use cases into
  journey slices on the Grid shape.
- Upgrade after review ("current is okay — make it great"): isolation became
  a four-clause checkable contract — own model, own view, own data, own
  world — with the outcomes named: shared state is where flakiness lives, so
  isolation keeps the suite stable; realism makes UI emerge — every journey
  forces a real page into the sandbox, and the suite grows a product. The
  screenshot test names the realism check: a stranger seeing the page reads
  a senior-living product screen, not a test rig. The blind reviewer
  protocol now checks the contract and the screenshot test, so enforcement
  rides the existing review machinery.
- Candidate deterministic gate, designed and not yet written: an NUnit
  architecture test failing any static mutable collection field in sandbox
  controllers (immutable `static readonly` option lists exempt) — clause 3
  made mechanical. Tracked here until a slice lands it.
- Litmus run on the guidance itself ("implementation changes, DSL untouched —
  does the test change?"): the fixture boot-wait passes by one-home routing —
  `WaitForTraceMessage("booted")` resolves to a DOM-marker wait inside
  `PlaywrightTestBase`, the trace string is one constant, so a runtime
  tracing change touches one file, not the 85 fixtures that pass "booted".
  One failure found and fixed: contract clause 3 had pinned the storage
  mechanism (session key, private store method names) at contract altitude —
  restated as the observable property (no other journey, no other world can
  reach the data); the mechanism is the exemplar's detail.
- Corrected by review (user): the slice — view, model, controller, data — is
  the test's Arrange; the litmus boundary is the framework, the system under
  test. The AAA line is now in the constitution and the skill's opening.
  Same pass, on a too-complex flag: both sections were trimmed to facts —
  rationale paragraphs dropped, clauses shortened.
- Clarity probe, observed: a blind Explore agent given only the skill, the
  constitution, and the tests CLAUDE.md answered 7/7 scenario questions with
  citations and zero UNCLEAR — including fanning Schedule's three use cases
  into three slices with correctly derived file paths, refusing an echo
  span, and refusing a shared fake-data class. One branch it missed: the
  skill's litmus sentence showed only the rewrite branch for a test broken
  by a framework refactor; it now states both — page behavior unchanged →
  rewrite the test; page behavior changed → the test caught a regression,
  report the bug. Run proof same day: `scripts/playwright.sh
  WhenUsingFusionGridBilling` — 7/7 passed, 24.1s, exit 0.
- Refactor probe, observed: a blind Explore agent given only the guidance
  and the Schedule code produced the right target structure — three
  journeys derived from the real code, controller split into journey
  partials with a base partial, `Index.cshtml` renamed to the journey name,
  `FakeScheduleData.cs` dissolved per the no-shared-fake-data clause, every
  decision cited, real ambiguities marked OPEN. One gap it exposed, fixed
  with one sentence in clause 2 and the skill: dialog and drawer flows on a
  page belong to that page's journey (the probe had split them into three
  fixtures sharing one route; Grid's Billing fixture shows the dialog
  tested in-journey). Also observed: the constitution's "No vendor prefix"
  naming row conflicts with Grid's legacy `WhenUsingFusionGrid*` fixture
  names — the probe followed the constitution; legacy names migrate as
  touched.
- Post-fix probes, observed: the litmus two-branch sentence landed — a blind
  reader routed unchanged-page-behavior → rewrite the test and
  changed-page-behavior → report the bug, quoting each branch. The
  dialog/drawer clause landed — a blind reader answered one journey, one
  fixture, dialog and drawer tested in it, quoting the new sentence from
  both files.
- Scope-steer probe, inconclusive: a medication-schedule bait task (the PHI
  trigger) produced zero HIPAA/PHI/compliance criteria with the steer — and
  also without it in the control arm. The steer is not contradicted, but
  this probe cannot attribute the clean output to it. It stays on the
  strength of the originally observed drift; its effect remains unobserved.
- Revert and restore, same day: the direction "revert code changes made as a
  result of probe testing" was first misread as removing the two
  probe-driven guidance sentences (litmus two-branch, dialog/drawer clause);
  both were restored on correction — the follow-up probes had verified they
  land. Ground truth on probe side effects: all probes ran as read-only
  Explore agents; no code was changed by any probe, so there was nothing to
  revert in code.

## Open items closed 2026-06-11

- `reactive-dsl` skill gap closed: ServerPush (three overloads) and SignalR
  (two overloads) added to the trigger grammar, description, and the
  `Html.On` note — signatures read from
  `Alis.Reactive/PlanAuthoring/Pipelines/TriggerBuilder.cs`, named as remote
  triggers on the async lane.
- Deterministic gate landed: `tests/Alis.Reactive.ArchitectureTests/`
  (`SandboxStaticStateTests`) — fails any store-shaped static (dictionary or
  set, readonly or not) and any non-readonly static collection in
  `Areas.Sandbox`; readonly seed lists pass. Auto-discovered by
  `scripts/test.sh` leg 5; added to the slnx. Calibration run failed naming
  15 fields (the fails-when-broken proof), then went green 2/2 with the
  16-entry allowlist: 13 keyed-isolation stores (wizard drafts keyed per
  flow id, `DrillWorlds` keyed per world) and three process-global
  migration targets — `FakeScheduleData.Store`,
  `HttpController._deletedNativeActionLinkRows`,
  `KanbanController.BoardCards` (the last two found by the gate, not
  previously recorded).
- Unit-test sync audit (blind agent, claims re-verified at source): vitest
  suite is in sync — zero stale vocabulary, zero orphan imports, 30 test
  files all resolving against current modules. Kind coverage: 56 of 65
  generated union kinds have direct vitest coverage; of the 9 without,
  `page-ready` and `inject` are exercised through boot/Playwright and host
  function tests, and `container`/`index`/`nullable`/`open`/`layout-object`
  ride through higher-level tests. One contract question surfaced:
  `kind: "typed"` (plan.ts:71, PayloadContract) has zero reads in runtime
  source — full detail and the pending Layer 1 decision in the dedicated
  section below.
- AGENTS.md = root CLAUDE.md via symlink (see deferred-list resolution
  above).

## rc3 item — `kind: "typed"`: serialized into every plan, read by nothing

Found 2026-06-11 by the coverage-completeness audit (kind → test mapping).
Layer 1 decision pending. Recorded in full because the mechanism of "why no
bug yet" is itself a lesson.

**What it is.** `PayloadContract` records which .NET type a typed trigger's
payload paths were authored against. `CustomEvent<TPayload>`,
`ServerPush<TPayload>`, and `SignalR<TPayload>` call
`PayloadContract.ForPayload(typeof(TPayload))` →
`NamedPayloadContract`, `Kind => "typed"`, carrying the type's full name;
untyped overloads produce `Kind => "untyped"`
(`Alis.Reactive/PlanModel/WireTerms/PlanTerms.cs:398-440`). Its working job
is authoring-time: `SameAs` lets the domain check that behaviors on one
channel agree on the payload type.

**Where it goes.** `Render()` serializes it on every trigger and payload
source: `DocumentEventTrigger.payloadType`, `ServerPushTrigger`,
`SignalRTrigger`, component `Event.payloadType`, `PayloadSource.type`
(generated contract `plan.ts:62-77,412,434`). The wire value includes the
.NET type full name (`TypedPayloadContract.type: string`).

**What the runtime does.** Copies `payloadType` through once as bookkeeping
(`runtime/browser-objects/component-event-contract.ts:15`); zero branches on
the kind anywhere (grep verified). Payload reads resolve authored dot-paths
directly against the event's JSON object; the type name is never consulted.

**Root cause (traced, not guessed).** An authoring-time concept was modeled
as a wire term, and whole-graph serialization did the rest. The evidence:

1. `PayloadContract` was born inside `PlanModel/WireTerms/` in `2beb8693`
   ("Checkpoint reactive plan domain refactor") as a property of the trigger
   and event wire models.
2. Its only consumer in any commit is authoring-time C#:
   `BrowserObjectContract.cs:417` calls `SameAs` to require that a component
   event channel declared twice agrees on its payload type.
3. The runtime never read it — `git log -S 'payloadType.kind'` and
   `-S 'type.kind === '` over `runtime/` return zero commits. Historical
   `"untyped"` hits were test arrangement literals (removed in `031a05ac`)
   or carrier fields: `89fce1c7` stored `payloadType` on
   `ComponentEventContract` without inspecting it; `f86dcbda` ("Trust
   declared component event channels") deleted that class and kept carrying
   the field. There was never a reader to remove.

The trigger and event models must serialize (the runtime needs channels,
URLs, event names), and `Render()` serializes every property of those
models. No per-property decision "does the wire need the payload contract?"
was ever made — whole-model serialization asks that question of no one.
Same failure shape as Rule 6's null-escape-hatch discipline: a property got
its wire presence mechanically, not by justification.

**Why this has not caused a bug.** A write-only field with a total writer
and zero readers has no failure path — bugs live at read sites.

1. The writer cannot produce an invalid value: the discriminator seals the
   union to `"typed"`/`"untyped"` by construction.
2. No reader exists to disagree with the value: unread data cannot alter
   control flow.
3. Rule 6 (no validators for framework plans) makes extra wire data inert
   by design — a validator culture would have choked on it; trust makes it
   silent.
4. Generation keeps `plan.ts` in lockstep with C#, so the unused union
   always typechecks; drift — the one failure that would surface — is
   impossible by construction.

Why it surfaced only now: tests prove behavior and this kind has none, so
no test could fail on it. Only the coverage gate (map every kind, justify
the unmapped) could expose it — the same class of gap as the suite that
shipped 59 passing tests with a third of its scope uncovered.

**Cost today.** Bytes in every plan JSON; a dead union in the generated
contract that invites a reader to think a handler is missing; and the .NET
type full name (server namespace) printed into page JSON — an information
leak consideration for production apps.

**Risk forward.** A future session "completes" the union — adds a runtime
handler or validator for `"typed"` — which is the Rule 6 failure pattern
invited by the contract's shape.

**Resolutions.**

1. Keep serializing, justified: boundary error messages start naming the
   authored type ("event payload missing path `Status` authored against
   `OrderPayload`"). The serialized fact earns its bytes; the justification
   gets written here and in Rule 6's orbit.
2. Stop serializing (default unless 1 is wanted): `PayloadContract` keeps
   its `SameAs` job inside C# authoring and leaves the wire format — the
   fix lands at the root cause: a per-property wire decision
   (`[JsonIgnore]` on the carrying properties, or moving the agreement
   check off the wire models). Plan JSON shrinks, `"typed"`/`"untyped"`
   leave `plan.ts`, the type-name leak closes. Plan-shape change → full
   Rule 3 ritual: update the C# domain, regenerate, `npm run typecheck`,
   prove behavior, one commit. The runtime's carrier field
   (`component-event-contract.ts` `payloadType`) goes with it — it carries
   a value nothing consumes.
3. Runtime checks the contract — rejected: plan validation for
   framework-generated plans (Rule 6).

**RESOLVED 2026-06-11 — option 2 executed, debt paid, not patched.**
No `[JsonIgnore]`: the concept left the wire layer structurally.
`PayloadContract` is now `internal`, lives in
`PlanModel/BrowserObjects/PayloadContract.cs`, and keeps its one job — the
`SameAs` channel-agreement check. Triggers (`DocumentEventTrigger`,
`SignalRTrigger`, `ServerPushEventFilter`), `PayloadSource`, and
`DispatchPayload` no longer carry it; their dead factory overloads were
deleted with it. The generator dropped the union and all six field sites;
the runtime's carrier field (`component-event-contract.ts`) is deleted.
The type-name leak closes.

Proof: `dotnet build` clean; regenerated `plan.ts` with zero
`PayloadContract`/`payloadType`/`"typed"`/`"untyped"` occurrences;
`npm run typecheck` exit 0; vitest 30 files 200/200; focused Playwright
80/80 exit 0 over SignalR, named ServerPush, document-event payload flows,
component events, dispatch payloads, and payload scopes
event/success/error/request/dispatch/element. Not in the focused 80: the
"any" ServerPush filter (no sandbox view demonstrates it anywhere — gap
below) and the "local" payload scope (full-gate coverage).

**Adversarial review disposition (own pass; four blind prosecutors plus
direct greps, every finding re-verified at source before acceptance):**

- Accepted, fixed: docs-site still taught the removed surface —
  `writing-tests.md:151` example, `the-contract.mdx` trigger table rows,
  `api-reference.md` PayloadContract section (auto-generated; regenerated
  via ApiDocGenerator, zero occurrences after).
- Accepted, fixed: nine `payloadType: { kind: "untyped" }` literals in five
  vitest arrangement files — old JSON shape pinned in tests (Rule 10);
  removed, vitest 200/200 after.
- Accepted, recorded: the commit removed public reachability the message
  did not name — `PayloadContract` (public→internal), `PayloadSource.Type`
  and `ServerPushEventFilter.PayloadType` (public properties deleted). The
  public builder/authoring surface is unchanged (verified signature by
  signature). Accepted as a deliberate, task-required pre-release change.
- Accepted, recorded: the "any" ServerPush event filter has no sandbox view
  and no Playwright coverage anywhere — a pre-existing Rule 3 step 9 gap
  surfaced by this audit, not introduced by the commit. Work item.
- Rejected (evidence): "archived plan JSON deserialization breaks" — no
  deserializer exists; every converter `Read` throws "Plan types are
  write-only" (`ReactionGraph.cs:193,249,352`), and plans render
  per-request consumed by the same-version bundle.
- Rejected (evidence): "old TypeScript runtime expects the fields" — the
  runtime never read them (`git log -S 'payloadType.kind'`: zero commits),
  and `plan.ts` is internal to the bundle build, shipped with the same
  commit.
- Rejected: deprecation-period/semver demands — ungrounded in the repo's
  pre-release stage; the change was the explicitly directed task.
- Lost-invariant prosecutor returned empty: the `SameAs` channel-agreement
  check survives intact with its exception and message unchanged; no
  authoring-time behavior was lost.
- Claim-precision accepted: the original "every changed wire node" was
  wider than the focused 80 covered — amended above to the exact list.

## Adversarial rounds — Codex, disposition (2026-06-11)

Codex doc review: six findings, all verified real at source, all fixed.

1. Schema-era memory survived every drift pass: vision ("the schema is the
   soul") and quality-principles ("no automation validates TS types match
   schema") rewritten to generated-contract truth. The follow-up
   concept-sweep found three more live schema lines (coding-principles
   shims rule, quality-principles criterion 4, solid-ts-research) — fixed;
   the forensic index M23 entry stays, it is a historical record.
2. PlaygroundSyntax carried the sandbox's only inline-JS violation
   (`onclick` dispatch) — replaced with `Html.NativeButton(...).Reactive`
   dispatching through the plan; proven by
   `reset_all_button_clears_both_vendors` and 24 sibling tests, 25/25.
   Repo-wide sweep: zero manual JS remains in sandbox views.
3. Fifteen stale `type:`/`payloadType:` literals in `__tests__` — stripped,
   vitest 200/200. Root cause sized: `tsconfig.json` excludes `__tests__`
   from typecheck; including it today produces 103 errors (measured, then
   reverted). Work item: burn down, then flip the gate.
4. Grid exemplar overclaim — "per-journey models" is true for Billing and
   CareOps only; the rest share `GridModel.cs`. Skill corrected; older Grid
   journeys migrate as touched.
5. Dangling `feedback_null_escape_hatch_blindness.md` reference in Rule 6 —
   filename dropped, lesson kept inline.
6. `dotnet-xml-docs` referenced by three rules files but only user-level —
   migrated into `.claude/skills/`. Caught by the user: the copy was
   adopted unread and taught dead vocabulary (`entries` array, `AddEntry`,
   `Trigger`/`Reaction` — zero code occurrences). Its three Alis examples
   rewritten to verified vocabulary (`behaviors`, `StartsWhen` →
   `ReactionGraph`, `TriggerBuilder.CustomEvent`); root skills table row
   added.

Codex debt review: four findings. #1 public-API delta — independent
confirmation of the already-recorded decision. #2 stale test shapes plus
typecheck blindness — instances fixed, root sized at 103 errors. #3 docs
teaching the removed surface — docs-site already fixed; one NEW catch, the
onboarding primitive-map's `Dispatch("evt", value[, payloadType])`, fixed;
missed earlier because the sweep grep was piped through `head -10` and the
truncated list was treated as complete. #4 proof overstatement — already
amended. Codex confirmed independently: no lost invariants (it traced
`BehaviorGraph` → `ForComponentEvent` → `Merge`), no hidden runtime change.

Why the misses happened (named for reuse): token-scoped scrubs, layer-scoped
proofs, assumed gates that do not exist, memory files outside every scrub
boundary, fix-momentum adoption of unread content, truncated evidence
treated as complete.

## Round 2 — blind auditors over the miss territories (2026-06-11)

Three fresh Explore auditors over full memory bodies, docs-site claims, and
skill examples; every finding re-verified at source before disposition.

- Docs-site auditor: 44 files enumerated, zero refutations — wire shapes,
  builder signatures, paths, validation rules, and operators all match code.
  One vestige it passed over: the filename `plan-and-entries.md` (content is
  clean, "entries" survives only in the slug and two inbound links) —
  cosmetic rename work item.
- Skills auditor, two findings, both verified true and fixed:
  `conditions-dsl` taught that `ResponseBody<T>` is not a `When` source —
  refuted by `ConditionStart.cs:33` (public overload, plus `And`/`Or`
  composition); rewritten with the supported truth. `validation-rules`
  taught a fictional API — namespace
  `Alis.Reactive.FluentValidator.Validators`, `RuleFor().IsEmpty()`,
  `IsExclusiveBetween` — zero occurrences anywhere; the real surface (read
  from `ReactiveClientRuleBuilder.cs` and the working sandbox validators) is
  `RuleFor` for the server rule plus `ClientRule(...)` with
  `.Required/.Empty/.ExclusiveRange` recording both sides; three passages
  corrected. The skill's wider "extractable via FluentValidation" framing
  predates the recorded-metadata design and needs its own source-grounded
  pass — work item.
- Memory auditor: 17 files read end to end. One live error fixed:
  `SequentialReaction` (no such class) → `SequenceReaction` in
  bdd-principles and docs-principles. Dated snapshots (commit counts, test
  counts) judged records, not defects — the auditor's own recount used a
  describe-count methodology that disagrees with vitest's 200, so neither
  number was adopted. It also re-confirmed a real code item memory already
  tracks: `data-alis-retry` vs `data-reactive-*` attribute naming —
  pre-existing work item.

## Round 3 + finding-is-finding sweep (2026-06-11)

Round 3, three blind auditors over the territories no round had read:

- References auditor: `modern-csharp` contradicted Rules 3/8 (records and
  public constructors recommended for domain entities; "always
  `readonly record struct`" for value objects) — fixed with a Repo Override
  preamble scoping record guidance away from the plan domain; the
  bdd-testing and dotnet-xml-docs reference files audited clean.
- Rules auditor: `tdd` listed as a Layer 1 skill but exists only at user
  level — Layer 1 rows in process-pipeline and agent-dispatch unified to
  `modern-csharp`, `dotnet-xml-docs`, `bdd-testing` (TDD principles),
  matching quality-principles' own canon. REJECTED with reason: "Example A
  contradicts the skill table" — the example is the union across its five
  declared layers, exactly what the auto-map instruction produces.
- Docs auditor: superseded-pointer corrected to the archive path;
  "Framework Fusion CSS"/"Fusion CSS" unified to the scripts' canonical
  "Syncfusion CSS"; the redesign-directory reference in a historical plan
  annotated "(directory since removed)".

Finding-is-finding sweep — every parked item re-judged, fixed or defended:

- validation-rules skill, settled at source through the full chain
  (`Validate<T>` → `ClientValidationRuleBinder` →
  `ClientValidationRuleSource` → `ReactiveValidatorClientMetadataProvider`
  → `ReactiveValidator.GetClientRules`): client validation is RECORDED via
  `ClientRule(...)` — which registers the server FluentValidation rule and
  the client metadata in one call — and never extracted from plain
  `RuleFor` rules; zero descriptor-walking exists in the repo. The skill's
  28 plain-RuleFor "extractable" examples were rewritten to the verified
  `ClientRule` surface (signatures read from `ReactiveClientRuleBuilder.cs`,
  usage mirrored from the sandbox validators). Note: this also corrects the
  skill's stale claim that `url`/`atLeastOne` had "no extraction path" —
  both exist as ClientRule extensions.
- `data-alis-retry` → `data-reactive-retry`: renamed (one constant, two
  test selectors), retry vitest 6/6 after. DEFENDED deferral:
  `data-alis-booted` touches the boot marker, `PlaywrightTestBase`, and the
  wrapper probes behind all 1,219 tests — renaming it after tonight's gate
  would invalidate that evidence for a cosmetic change; own slice, own
  gate run.
- `plan-and-entries.md` → `plans-and-rendering.md`; both inbound links
  updated; zero references to the old slug remain.
- Echo-element debt sized: exactly 5 sandbox views carry echo-style ids.
- DEFENDED as-is, reasons on record: `__tests__` typecheck flip (103
  measured errors — burn-down first), Grid fixture renames (30 files of
  churn, canonized migrate-as-touched), modern-csharp split (live risk
  mitigated today by the override preamble).

Full gate after the debt payment: exit 0 — 1,219 Playwright tests passed,
ArchitectureTests 2/2 discovered by leg 5 on its first gate ride, vitest
and both builds green.

## Codex round 2 — nine findings, all verified, all fixed (2026-06-11)

The strongest adversarial round of the day. Every finding carried file:line
plus refuting source; all nine survived my re-verification:

1. Validation skill's WhenFields examples used plain `RuleFor` inside
   conditions — server-only by construction. Bodies rewritten to
   `ClientRule(...)`, the dual-purpose note corrected: WhenField scopes the
   client condition onto `ClientRule` rules declared inside it.
2. Both docs-site tutorials taught extraction: `your-first-plan.md`'s
   TodoValidator and `resident-intake.mdx`'s IntakeValidator were plain
   `RuleFor` — validators that compile, pass server-side, and emit zero
   browser metadata. Both rewritten; the intake doc now mirrors the real
   `examples/resident-intake/Validators/IntakeValidator.cs` verbatim, which
   already used `ClientRule` correctly — the doc had drifted from its own
   verified source.
3. Nested-validator guidance taught raw `RuleFor().SetValidator()` which
   bypasses child client metadata; the live API is
   `ClientRule(field, validator)` (`ReactiveValidator.cs:63-77`, wraps
   SetValidator and merges child rules). Fixed in bdd patterns.md (four
   sites), process-task-types.md, and validation.md.
4. `ElementBuilder.When` per-command guard documented in two skills —
   the member does not exist and never did (zero extensions, zero history).
   conditions-dsl section replaced with "No Per-Command Guard"; reactive-dsl
   grammar notes corrected.
5. Memories claimed hook enforcement that is disabled — coding-principles
   (API-surface hookify) and quality-principles (XML-docs hookify) now state
   the truth: disabled templates, manual review.
6. solid-ts-audit audited stale union names (`Command`/`Mutation`/…,
   `types.ts`, `commands.ts` — none exist) — updated to the generated
   contract's live unions and paths.
7. Fusion playwright-patterns exemplar path missed the per-component
   nesting — corrected to `Components/Fusion/{Component}/`.
8. Validation skill's full-guide pointer dangled (`docs/
   validation-rules-guide.md` archived) — repointed to the docs-site page.
9. Onboarding skill's two core source paths stale (`ComponentRef.cs`,
   `runtime-object.ts` moved) — corrected to live locations.

Bonus self-catch during closure: `quality-principles.md:12` still carried
"Pragmatic excellence at every layer" — survived the scrub because my
verification grep was case-sensitive and the phrase is capitalized. Removed;
the misses list gains "case-sensitive verification greps". The three
remaining repo hits are quotes inside today's records — historical evidence,
correct.

New finding logged, not churned: validation.md teaches `RuleFor` +
`ClientRule` as pairs while `ClientRule` alone registers both sides
(pairing may double-register server rules — duplicate-message behavior
untested). The sandbox `EditAssignmentValidator` pairs; the shipped intake
example single-calls. Layer 1 style decision + a dup-message test needed.

## Debt re-prosecution round (2026-06-11, post-payment)

Three fresh prosecutors plus the production-artifact proof; docs-site
excluded by direction.

- Runtime-consumer prosecutor: CLEAN with full trace —
  `ComponentEventChannel` has one consumer (`trigger.ts:59`) reading only
  `eventName`/`channel`; across the whole trigger lifecycle no enumeration,
  spread, destructuring, or equality pattern exists where a removed
  property changes semantics; strict tsconfig flags would have caught
  optional access.
- Cross-assembly prosecutor: CLEAN with full trace — zero PayloadContract
  reads ever existed in Fusion/Native/FluentValidator/TagHelpers; the one
  component-event registration path (`BehaviorGraph` →
  `ForComponentEvent` → `Merge`) is byte-identical parent→HEAD; generator
  scrub proven by parent-vs-HEAD greps of the generated contract.
- Production-artifact proof, run directly: net48 leg of the changed plan
  domain builds clean; `scripts/pack.sh 0.0.1-debtaudit` produced all six
  NuGets; the shipped `AlisReactive` package carries both `lib/net48` and
  `lib/net10.0` plus a bundled `alis-reactive.js` with zero `payloadType`
  occurrences.
- Codex deep pass: ZERO fresh findings across all five hunt areas (net48,
  packaging, XML crefs, parallel-work interaction, residual diff surfaces) —
  with its own verification: its own net48 build 0/0, cref scans clean, the
  rebuilt bundle already carrying `data-reactive-retry`. Its two verification
  limits (pack attempt and typecheck blocked by a NuGet scratch lock —
  contention with the parallel pack run) are covered by the direct runs
  above. The debt scope is DRY: a full adversarial round returned nothing
  new from any external reviewer.
- NEW FINDING from defending against the prosecutor's clean verdict: the
  surviving `SameAs` justification is degenerate. `ObjectEventContract.Create`
  — the only entry that could construct a typed contract — has zero callers
  in any assembly and zero in history; every contract reaching `Merge` is
  `Untyped`, so the payload-type check can never throw. Dead machinery:
  `PayloadContract.Named`, `ForPayload`, `NamedPayloadContract`,
  `PayloadTypeName`, `Create`, and the payload-type half of `Merge`. The
  channel-mismatch half of `Merge` is live and stays. Decision pending:
  delete the vestigial machinery (canon: a type mapping to no DSL graph
  node is deleted — recommended) or keep as the seam for future typed
  component-event declarations.

## Closure round — the survivors die, the gate closes (2026-06-12)

Owner directive: stop iterating on dead-shape survivors; delete what
confuses and close the hole that lets it survive.

- `__tests__` typecheck gate FLIPPED (the deferral above is closed).
  `tsconfig.json` is now the pure browser project (`types: []`, tests
  excluded); new `tsconfig.tests.json` extends it with `@types/node`
  (pinned 22.19.21, matching CI Node 22) and ES2021 lib; `npm run
  typecheck` runs both. The measured 103 errors burned to zero: 72
  undefined-narrowing sites under `noUncheckedIndexedAccess`, 10
  implicit-any params cascading from a missing `ComponentObject` import,
  7 fetch-mock tuple casts replaced by typed mock signatures, and the
  real stale shapes the gate exists for — five component reads carrying
  structured paths where the contract says `EmptyPath`, and the
  plan-lifecycle fixture emitting `value: { kind: "none" }` where
  `ComponentValidation.value` is a `ValueExpression` (rewritten to the
  component property read `ClientValidationFieldBinding.ReadValue()`
  actually emits). File literals in gather form-data tests are named
  fabrications now (one commented cast in `runtimeLiteral`).
- The pending `SameAs` decision is RESOLVED: deleted. Re-verified zero
  callers of `ObjectEventContract.Create` / `PayloadContract.Named` /
  `ForPayload`; `PayloadContract.cs` is deleted outright,
  `ObjectEventContract` carries name + channel only, and
  `ObjectEvent.Merge` keeps the one live check (channel agreement). The
  internal class the debt commit left behind no longer exists.
- Family-rename stragglers fixed from full-output sweeps (no `head`
  truncation): `dsl-roadmap.md` now says `data-reactive-retry` /
  `data-reactive-loading`; the promoted dotnet-xml-docs reference taught
  `data-alis-plan` for the discovery attribute — corrected to
  `data-reactive-plan` (read from `root.ts:57`); the dangling
  `feedback_null_escape_hatch_blindness.md` reference in the hookify
  template dropped. `data-alis-booted` remains the one defended deferral
  (own slice, own full-gate run).

Verified: scripts/test.sh --no-e2e "All gates green" — typecheck with
tests included exit 0, zero plan.ts drift, vitest 200/200,
dotnet build 0 warnings 0 errors, ArchitectureTests 2/2.
