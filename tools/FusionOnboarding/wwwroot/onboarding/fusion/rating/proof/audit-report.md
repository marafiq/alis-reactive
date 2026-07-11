# Rating Audit Report

Status: audited and closed. `FusionRating` is fully onboarded. Every public typed
member has deterministic raw EJ2 evidence, an explicit C# name decision, an
authoritative primitive mapping, a vertical-slice file, and focused typed DSL
Playwright proof bound to a fails-when-broken assertion. The generated typed API
coverage matrix is `audited` with every row `row-proven`.

## Accepted Members

The Rating slice onboards the post-render runtime surface; initial render
configuration stays on the Syncfusion `RatingBuilder`.

- `FusionRating(this InputBoundField<TModel, double> setup, Action<RatingBuilder> build)` — the field render helper, bound to a numeric model property.
- `FusionRatingEvents.ValueChanged` — the `valueChanged` event selector, with `Reactive(...)` wiring through `ComponentEventOnboarding.Wire`.
- `FusionRatingValueChangedArgs.Value` — the newly selected rating, read from `event.value`.
- `FusionRatingValueChangedArgs.PreviousValue` — the rating before the change, read from `event.previousValue`.
- `FusionRatingValueChangedArgs.IsInteracted` — user-choice versus programmatic-change flag, read from `event.isInteracted`.
- `SetValue(this ComponentRef<FusionRating, TModel> self, double value)` — writes `value` and chains `dataBind()` to repaint the stars.
- `Reset(this ComponentRef<FusionRating, TModel> self)` — calls `reset()` to clear the rating to its minimum.
- `Value(this ComponentRef<FusionRating, TModel> self)` — reads `value` as a typed `double` source for gather, conditions, and set text.

## Excluded Candidates

- `valueChanged.event` — browser-owned DOM `Event`; excluded from the public typed payload per `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them`.
- `valueChanged.name` (inherited `BaseEventArgs.name`) — duplicate event identity metadata; the `ValueChanged` selector already owns the event identity.
- `beforeItemRender`, `onItemHover`, `created` events — per-item render and hover carry browser-owned `element`/`event` payloads; `created` is a DOM-native lifecycle event with no typed payload; no focused Senior Living use case.
- `allowReset`, `cssClass`, `disabled`, `enableAnimation`, `enableSingleSelection`, `itemsCount`, `labelPosition`, `labelTemplate`, `min`, `precision`, `readOnly`, `showLabel`, `showTooltip`, `tooltipTemplate`, `emptyTemplate`, `fullTemplate`, `visible` — builder-owned static configuration (`discovery/public-api-surface.json` marks each `builder.covered = true`); no post-render read/write proven necessary.
- `destroy()` method — lifecycle cleanup, not plan behavior (`discovery/public-api-surface.json` classifies it `skip`).

## Skill Pattern Map

This audit confirms the following reusable patterns in `_skill/pattern-map.md`.
No new pattern row was required: the Rating slice is a clean application of
existing precedent, with every defect class already covered.

- `_skill/pattern-map.md#p000-discovery-is-exhaustive-but-public-c-dsl-is-selective` — 24 EJ2 members discovered; 8 public C# members accepted, the rest recorded and excluded with evidence.
- `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them` — `valueChanged.event` excluded as a browser-owned DOM object rather than exposed as `object`/`dynamic`.
- `_skill/pattern-map.md#p005-event-payload-coverage-is-property-level` — `FusionRatingValueChangedArgs` is proven property by property (`Value`, `PreviousValue`, `IsInteracted`), not as one class row.
- `_skill/pattern-map.md#p018-writable-payload-fields-need-lifecycle-effect-proof` — `SetValue` is proven by the visible rating changing, and `Reset` by the visible rating clearing to 0, not by a silent property set.
- `_skill/pattern-map.md#p020-public-methods-need-their-own-row-proof` — `SetValue`, `Reset`, and the `Value` read each have their own row and their own fails-when-broken proof.

## Remote Data Lane

`Rating` is a numeric single-value input, not a data-capable component: it binds
to one model property and emits one scalar. It has no remote binding, paging,
filtering, lookup, virtualization, or server-query lane to onboard. The realistic
remote workflow for a rating is sending the chosen score to the server, which the
`Value()` gather row proves: the survey POSTs `"satisfactionScore":5` and the
resident sees the server confirmation. This satisfies the remote-behavior
expectation for this component class (see `references/automation-gates.md` Gate 6
applicability — Rating is not a board/grid/scheduler/list/tree).

## Commands Run

- raw probe and trace, real headless browser:
  `node .claude/skills/onboard-fusion-component/scripts/run-fusion-probe-trace.mjs --component rating --api-set core`
  — wrote `traces/raw-ej2-core.trace.json` (9 trace rows, including real `valueChanged`-family discovery, `beforeItemRender` per-star payloads, `created`, the instance `own keys`, and `prototype methods` showing `dataBind` and `reset`).
- typed API coverage matrix, current against source:
  `node .claude/skills/onboard-fusion-component/scripts/write-fusion-typed-api-coverage.mjs --component rating --fusion-type FusionRating --check` — reported current.
- onboarding status inventory:
  `node .claude/skills/onboard-fusion-component/scripts/report-fusion-onboarding-status.mjs --write`.
- artifact gate verifier:
  `node .claude/skills/onboard-fusion-component/scripts/verify-fusion-artifact-gates.mjs --component rating`.
- behavior proof:
  `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Rating.WhenUsingFusionRating"` — the per-member outcome is recorded green in `proof/behavioral-coverage.json` and verified by the onboarding behavioral-coverage gate.

## Coverage Matrix Summary

Current generated matrix count: 10 typed C# API rows, 0 supplemental audit rows,
10 total rows. The latest count check shows 10 row-proven matrix rows and 0
matrix rows without `row-proven` status.

## Linked Artifacts

- Master index: `master-usecases-index.md`
- Source inventory: `discovery/source-inventory.md`
- Public API surface: `discovery/public-api-surface.json`
- Event payload surface: `discovery/event-payload-surface.json`
- MVC builder coverage: `discovery/mvc-builder-coverage.md`
- Blazor candidates: `discovery/blazor-candidates.md`
- Raw EJ2 core probe: `probes/raw-ej2-core.html`
- Raw EJ2 core trace: `traces/raw-ej2-core.trace.json`
- Primitive map: `mapping/primitive-map.md`
- C# name decisions: `mapping/csharp-name-decisions.md`
- Vertical slice plan: `mapping/vertical-slice-plan.md`
- Typed API coverage matrix: `proof/typed-api-coverage-matrix.md`
- Behavioral coverage: `proof/behavioral-coverage.json`
- Playwright proof: `proof/playwright-proof.md`
- Skill pattern map: `_skill/pattern-map.md`

## Commit Boundary

This audit closes the Rating component row. The artifacts agree end to end on the
same member shape, payload shape, sync lane, C# API, runtime behavior, and
Playwright proof. The discovery JSON, trace, and coverage matrix remain generated
by the skill scripts and were not hand-edited.
