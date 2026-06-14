# Tab Audit Report

Status: audited and closed. `FusionTab` is fully onboarded. Every public typed
member has deterministic raw EJ2 evidence, an explicit C# name decision, an
authoritative primitive mapping, a vertical-slice file, and focused typed DSL
Playwright proof bound to a fails-when-broken assertion. The generated typed API
coverage matrix is `audited` with every row `row-proven`.

## Accepted Members

The Tab slice onboards the post-render navigation surface; initial render
configuration (items, headers, the first selected tab) stays on the Syncfusion
`TabBuilder`. Tab is a non-input navigation component, so it has no `Value()`
read and no `SetValue()`.

- `FusionTab(this IHtmlHelper<TModel> html, ReactivePlan<TModel> plan, string elementId, Action<TabBuilder> build)` — the navigation render helper, carrying the controlled `elementId` into the plan.
- `FusionTabEvents.Selected` — the `selected` event selector, with `Reactive(...)` wiring through `ComponentEventOnboarding.Wire`.
- `FusionTabSelectedArgs.SelectedIndex` — the newly active tab index, read from `event.selectedIndex`.
- `FusionTabSelectedArgs.PreviousIndex` — the tab index before the change, read from `event.previousIndex`.
- `FusionTabSelectedArgs.IsSwiped` — click-versus-swipe flag, read from `event.isSwiped`.
- `Select(this ComponentRef<FusionTab, TModel> self, int index)` — calls `select(index)` to activate the section at the given index.
- `HideTab(this ComponentRef<FusionTab, TModel> self, int index, bool isHidden = true)` — calls `hideTab(index, isHidden)` to hide or restore the section header at the given index.
- `SetSelectedItem(this ComponentRef<FusionTab, TModel> self, int index)` — writes `selectedItem` to activate the section at the given index.

## Excluded Candidates

- `selected.selectedItem`, `selected.previousItem`, `selected.selectedContent` — browser-owned DOM `HTMLElement` nodes (the core trace shows each as `[Element#...]`); excluded from the public typed payload per `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them`.
- `selected.preventFocus` — write-only focus-suppression flag; no resident-facing read use case.
- `selected.name` (inherited `BaseEventArgs.name`) — duplicate event identity metadata; the `Selected` selector already owns the event identity.
- `selecting`, `added`, `adding`, `removed`, `removing`, `dragged`, `dragging`, `onDragStart`, `created`, `destroyed` events — cancelable pre-events and collection-mutation/drag events carry browser-owned payloads or builder-authoring concerns; `created`/`destroyed` are DOM-native lifecycle events; no focused Senior Living navigation use case.
- `addTab`, `removeTab`, `enableTab`, `disable`, `getItemIndex`, `refresh`, `refreshActiveTab`, `refreshActiveTabBorder`, `refreshOverflow` methods — `discovery/parity-accounting.json` records a source-grounded reason for each: collection mutation is builder-authoring; the refresh family is vendor layout housekeeping; `getItemIndex` is a redundant id-to-index lookup.
- `tabId` property — equals the `elementId` the developer already passes to `Html.FusionTab(plan, elementId, ...)` (`discovery/parity-accounting.json`).
- `allowDragAndDrop`, `animation`, `cssClass`, `headerPlacement`, `height`, `heightAdjustMode`, `items`, `loadOn`, `overflowMode`, `reorderActiveTab`, `scrollStep`, `showCloseButton`, `swipeMode`, `width`, and the remaining static settings — builder-owned static configuration (`discovery/public-api-surface.json` marks each `builder.covered = true`); no post-render read/write proven necessary.
- `destroy()` method — lifecycle cleanup, not plan behavior (`discovery/public-api-surface.json` classifies it `skip`).

## Skill Pattern Map

This audit confirms the following reusable patterns in `_skill/pattern-map.md`.
No new pattern row was required: the Tab slice is a clean application of existing
precedent, with every defect class already covered.

- `_skill/pattern-map.md#p000-discovery-is-exhaustive-but-public-c-dsl-is-selective` — 43 EJ2 members discovered; 8 public C# members accepted, the rest recorded and excluded with evidence.
- `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them` — `selected.selectedItem`/`previousItem`/`selectedContent` excluded as browser-owned DOM objects rather than exposed as `object`/`dynamic`.
- `_skill/pattern-map.md#p005-event-payload-coverage-is-property-level` — `FusionTabSelectedArgs` is proven property by property (`SelectedIndex`, `PreviousIndex`, `IsSwiped`), not as one class row.
- `_skill/pattern-map.md#p020-public-methods-need-their-own-row-proof` — `Select`, `HideTab`, and `SetSelectedItem` each have their own row and their own fails-when-broken proof.

## Remote Data Lane

`Tab` is a navigation container, not a data-capable component: it switches
between authored sections and emits the selected index. It has no remote
binding, paging, filtering, lookup, virtualization, or server-query lane to
onboard. The realistic remote workflow for a tab is loading a section's content,
which the section views render directly. This satisfies the remote-behavior
expectation for this component class (see `references/automation-gates.md` Gate 6
applicability — Tab is not a board/grid/scheduler/list/tree).

## Commands Run

- raw probe and trace, real headless browser:
  `node .claude/skills/onboard-fusion-component/scripts/run-fusion-probe-trace.mjs --component tab --api-set core`
  — wrote `traces/raw-ej2-core.trace.json` (6 trace rows, including the real `selected` payload with `selectedIndex: 1` / `previousIndex: 0` / `isSwiped: false` after a `select(1)` gesture, the `selecting` pre-event, the instance `own keys`, and `prototype methods` showing `select`, `hideTab`, and `addTab`).
- typed API coverage matrix, current against source:
  `node .claude/skills/onboard-fusion-component/scripts/write-fusion-typed-api-coverage.mjs --component tab --fusion-type FusionTab --check` — reported current.
- onboarding status inventory:
  `node .claude/skills/onboard-fusion-component/scripts/report-fusion-onboarding-status.mjs --write`.
- artifact gate verifier:
  `node .claude/skills/onboard-fusion-component/scripts/verify-fusion-artifact-gates.mjs --component tab`.
- behavior proof:
  `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Tab.WhenTabSwitches"` — the per-member outcome is recorded green in `proof/behavioral-coverage.json` and verified by the onboarding behavioral-coverage gate.

## Coverage Matrix Summary

Current generated matrix count: 10 typed C# API rows, 0 supplemental audit rows,
10 total rows. The latest count check shows 10 row-proven matrix rows and 0
matrix rows without `row-proven` status.

| Item | Count |
|---|---:|
| Typed C# API rows | 10 |
| Supplemental audit rows | 0 |
| Total typed coverage matrix rows | 10 |

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
- Blind reviewer verdict: `proof/blind-review.md`
- Skill pattern map: `_skill/pattern-map.md`

## Commit Boundary

This audit closes the Tab component row. The artifacts agree end to end on the
same member shape, payload shape, sync lane, C# API, runtime behavior, and
Playwright proof. The discovery JSON, trace, and coverage matrix remain generated
by the skill scripts and were not hand-edited.
