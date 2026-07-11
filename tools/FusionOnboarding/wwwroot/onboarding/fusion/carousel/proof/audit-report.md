# Carousel Audit Report

Status: audited and closed. `FusionCarousel` is fully onboarded. Every public
typed member has deterministic raw EJ2 evidence, an explicit C# name decision,
an authoritative primitive mapping, a vertical-slice file, and focused typed DSL
Playwright proof bound to a fails-when-broken assertion. The generated typed API
coverage matrix is `audited` with every row `row-proven`.

## Accepted Members

The Carousel slice onboards the post-render runtime surface; initial render
configuration stays on the Syncfusion `CarouselBuilder`. `FusionCarousel` is a
navigation/display component and registers no input binding.

- `FusionCarousel(...)` — the render helper carrying the controlled component id into the plan for `.Reactive(...)` wiring.
- `FusionCarouselEvents.SlideChanging` / `SlideChanged` — the `slideChanging` and `slideChanged` event selectors, wired through `ComponentEventOnboarding.Wire`.
- `FusionCarouselSlideChangingArgs` (`CurrentIndex`, `NextIndex`, `IsSwiped`, `SlideDirection`, writable `Cancel`) and `PreventTransition()` — the before-change payload and the cancel write.
- `FusionCarouselSlideChangedArgs` (`CurrentIndex`, `PreviousIndex`, `IsSwiped`, `SlideDirection`) — the after-change payload.
- `SelectedIndex(...)` — the selected-index read as a typed `int` source.
- `Next(...)` / `Previous(...)` — the `next`/`prev` navigation methods.

## Excluded Candidates

- `slideChanging.currentSlide` / `nextSlide`, `slideChanged.currentSlide` / `previousSlide` — browser-owned `HTMLElement`s; excluded from the public typed payload per `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them` (the raw trace resolves them to `[Element#...]`).
- `slideChanging.name` / `slideChanged.name` (inherited `BaseEventArgs.name`) — duplicate event identity metadata; the event selectors already own event identity.
- `play()` / `pause()` methods — autoplay transport; excluded with a source-grounded reason in `discovery/parity-accounting.json` (autoplay is off and undesirable in a deliberate, manually-navigated review; the runtime navigation need is covered by `Next()`/`Previous()`).
- `destroy()` method — lifecycle cleanup, not plan behavior (`discovery/public-api-surface.json` classifies it `skip`).
- `animationEffect`, `autoPlay`, `buttonsVisibility`, `cssClass`, `dataSource`, `enableTouchSwipe`, `height`, `width`, `htmlAttributes`, `indicatorsTemplate`, `indicatorsType`, `interval`, `items`, `itemTemplate`, `loop`, `nextButtonTemplate`, `partialVisible`, `pauseOnHover`, `playButtonTemplate`, `previousButtonTemplate`, `selectedIndex` (initial), `showIndicators`, `showPlayButton`, `swipeMode`, `allowKeyboardInteraction` — builder-owned static configuration (`discovery/public-api-surface.json` marks each `builder.covered = true`); no post-render read/write proven necessary beyond the `selectedIndex` runtime read, which is onboarded separately.

## Skill Pattern Map

This audit confirms the following reusable patterns in `_skill/pattern-map.md`.
No new pattern row was required: the Carousel slice is a clean application of
existing precedent, with every defect class already covered. One empirical note
worth carrying is recorded in the probe and primitive map: EJ2 does not raise
`slideChanging` for a `prev()` from index 0 (it short-circuits), so a cancel
guard for the first slide must key on `nextIndex === 0` (a Previous from index 1),
the move EJ2 actually raises — this is an application of P018 (writable payload
fields need a lifecycle-effect proof on the gesture that actually fires).

- `_skill/pattern-map.md#p000-discovery-is-exhaustive-but-public-c-dsl-is-selective` — 32 EJ2 members discovered; 16 public C# members accepted (2 selectors, 2 payload contracts, 9 payload members, the `PreventTransition` write, the `SelectedIndex` read, `Next`/`Previous`, and the render helper), the rest recorded and excluded with evidence.
- `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them` — the `currentSlide`/`nextSlide`/`previousSlide` DOM elements are excluded rather than exposed as `object`/`dynamic`.
- `_skill/pattern-map.md#p005-event-payload-coverage-is-property-level` — both payload contracts are proven property by property, not as one class row.
- `_skill/pattern-map.md#p018-writable-payload-fields-need-lifecycle-effect-proof` — `PreventTransition` (the `Cancel` write) is proven by the transition NOT happening (the carousel stays on Therapy Goals), not by a silent property set.
- `_skill/pattern-map.md#p020-public-methods-need-their-own-row-proof` — `Next`, `Previous`, and the `SelectedIndex` read each have their own row and their own fails-when-broken proof.

## Remote Data Lane

`Carousel` is a navigation/display component, not a data-capable grid/board/
scheduler/list/tree. It has no remote binding, paging, filtering, lookup,
virtualization, or server-query lane to onboard. The realistic remote workflow
for a guided review is recording each reviewed section to the resident's chart,
which the `slideChanged` gather row proves: the review POSTs the slide-change
payload (`sectionIndex`, `cameFromIndex`, `direction`, `bySwipe`) and the nurse
sees the server-built chart line. This satisfies the remote-behavior expectation
for this component class (see `references/automation-gates.md` Gate 6
applicability — Carousel is not a board/grid/scheduler/list/tree).

## Commands Run

- raw probe and trace, real headless browser:
  `node .claude/skills/onboard-fusion-component/scripts/run-fusion-probe-trace.mjs --component carousel --api-set core`
  — wrote `traces/raw-ej2-core.trace.json` (19 trace rows: initial `selectedIndex`, `slideChanging`/`slideChanged` payloads for forward and back moves, the `nextIndex === 0` cancel suppressing a move, and the instance own-keys plus prototype methods showing `next`/`prev`/`play`/`pause`).
- typed API coverage matrix, current against source:
  `node .claude/skills/onboard-fusion-component/scripts/write-fusion-typed-api-coverage.mjs --component carousel --fusion-type FusionCarousel --check` — reported current.
- parity:
  `node .claude/skills/onboard-fusion-component/scripts/compute-fusion-parity.mjs --component carousel` — "parity: 32/32 = 100.0% -> PASS".
- artifact gate verifier:
  `node .claude/skills/onboard-fusion-component/scripts/verify-fusion-artifact-gates.mjs --component carousel`.
- behavior proof:
  `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Carousel.WhenUsingFusionCarousel"` — "Total tests: 6, Passed: 6"; the per-member outcome is recorded green in `proof/behavioral-coverage.json` and verified by the onboarding behavioral-coverage gate (`verify-behavioral-coverage.mjs --component carousel` — "19/19 members mapped").

## Coverage Matrix Summary

Current generated matrix count: 19 typed C# API rows, 0 supplemental audit rows,
19 total rows. The latest count check shows 19 row-proven matrix rows and 0
matrix rows without `row-proven` status.

## Linked Artifacts

- Master index: `master-usecases-index.md`
- Source inventory: `discovery/source-inventory.md`
- Public API surface: `discovery/public-api-surface.json`
- Event payload surface: `discovery/event-payload-surface.json`
- MVC builder coverage: `discovery/mvc-builder-coverage.md`
- Blazor candidates: `discovery/blazor-candidates.md`
- Parity accounting: `discovery/parity-accounting.json`
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

This audit closes the Carousel component row. The artifacts agree end to end on
the same member shape, payload shape, sync lane, C# API, runtime behavior, and
Playwright proof. The discovery JSON, trace, and coverage matrix remain generated
by the skill scripts and were not hand-edited.
