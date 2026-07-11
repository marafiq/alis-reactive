# Toolbar Audit Report

Status: audited and closed. `FusionToolbar` is fully onboarded. Every public typed
member has deterministic raw EJ2 evidence, an explicit C# name decision, an
authoritative primitive mapping, a vertical-slice file, and focused typed DSL
Playwright proof bound to a fails-when-broken assertion. The generated typed API
coverage matrix is `audited` with every row `row-proven`.

## Accepted Members

The Toolbar slice onboards the post-render runtime surface; initial render
configuration stays on the Syncfusion `ToolbarBuilder`. The toolbar is a command
(navigation) component, not an input component, so it registers no form binding.

- `Html.FusionToolbar(plan, id, build)` — the render helper carrying the controlled component id into the plan.
- `FusionToolbarEvents.Clicked` — the `clicked` event selector, with `Reactive(...)` wiring through `ComponentEventOnboarding.Wire`.
- `FusionToolbarClickedArgs.Item` — the clicked item, narrowed to a typed `FusionToolbarItem` rather than the broad EJ2 `ItemModel`.
- `FusionToolbarItem.Id` — the clicked command id, read from `event.item.id`.
- `FusionToolbarItem.Text` — the clicked command label, read from `event.item.text`.
- `FusionToolbarItem.Disabled` — the clicked item's disabled flag, read from `event.item.disabled`.
- `Disable(this ComponentRef<FusionToolbar, TModel> self, bool value)` — calls `disable(value)`, which adds/removes `e-overlay` on the toolbar root.

## Excluded Candidates

- `clicked.cancel` — writable pre-click cancel flag; no Senior Living command-bar use case and no typed writable-payload row authored for the toolbar.
- `clicked.originalEvent` — browser-owned DOM `Event`; excluded per `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them`.
- `clicked.name` (inherited `BaseEventArgs.name`) — duplicate event identity; the `Clicked` selector already owns the event identity.
- `beforeCreate`, `created`, `destroyed`, `keyDown` events — lifecycle/keyboard hooks; `created`/`destroyed` are DOM-native (`discovery/event-payload-surface.json`); no focused command-bar use case.
- `enableItems`, `hideItem` methods — genuine per-item enable/hide runtime behaviors, recorded as DEFERRED per-item candidates in `discovery/parity-accounting.json`; onboarding them as typed DSL needs a new typed method on the framework slice, out of scope for this audit of the existing surface. The whole-toolbar `Disable(bool)` proves the enable/disable lane.
- `addItems`, `removeItems` methods — builder-owned item-collection mutation; the `items` set is `builder.covered = true` (`discovery/public-api-surface.json`); see `discovery/parity-accounting.json`.
- `refreshOverflow` method — layout-internal overflow reflow with no plan value; see `discovery/parity-accounting.json`.
- `changeOrientation` method — `@hidden` in Syncfusion source (`toolbar.d.ts:434`); orientation is builder-owned; see `discovery/parity-accounting.json`.
- `allowKeyboard`, `cssClass`, `enableCollision`, `enableHtmlSanitizer`, `height`, `items`, `overflowMode`, `scrollStep`, `width` — builder-owned static configuration (`discovery/public-api-surface.json` marks each `builder.covered = true`); no post-render read/write proven necessary.
- `destroy()` method — lifecycle cleanup, not plan behavior (`discovery/public-api-surface.json` classifies it `skip`).

## Skill Pattern Map

This audit confirms the following reusable patterns in `_skill/pattern-map.md`.
No new pattern row was required: the Toolbar slice is a clean application of
existing precedent, with every defect class already covered.

- `_skill/pattern-map.md#p000-discovery-is-exhaustive-but-public-c-dsl-is-selective` — 22 EJ2 members discovered; 7 public C# members accepted, the rest recorded and excluded with evidence or accounted in `discovery/parity-accounting.json`.
- `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them` — `clicked.originalEvent` excluded as a browser-owned DOM object rather than exposed as `object`.
- `_skill/pattern-map.md#p005-event-payload-coverage-is-property-level` — `FusionToolbarClickedArgs`/`FusionToolbarItem` proven property by property (`Item`, `Id`, `Text`, `Disabled`), not as one class row.
- `_skill/pattern-map.md#p018-writable-payload-fields-need-lifecycle-effect-proof` — `Disable(true)` is proven by the visible `e-overlay` lock appearing and `Disable(false)` by it being removed, not by a silent property set.
- `_skill/pattern-map.md#p020-public-methods-need-their-own-row-proof` — `Disable` has its own row and its own fails-when-broken proof.
- `_skill/pattern-map.md#p025-a-disabled-item-only-payload-member-is-proven-through-the-gather-body-not-a-disabled-item-click` — `FusionToolbarItem.Disabled` is proven through the POST gather body (`"commandDisabled":false`) because a trusted toolbar click only ever lands on an enabled item.

## Remote Data Lane

`Toolbar` is a command (navigation) component, not a data-capable component: it
renders a fixed set of command items and emits a typed `clicked` payload. It has
no remote binding, paging, filtering, lookup, virtualization, or server-query lane
to onboard. The realistic remote workflow for a command bar is sending the clicked
command to the server, which the Pay-balance gather row proves: paying POSTs
`"commandId":"pay-balance"` and the resident sees the server confirmation. This
satisfies the remote-behavior expectation for this component class (see
`references/automation-gates.md` Gate 6 applicability — Toolbar is not a
board/grid/scheduler/list/tree).

## Commands Run

- raw probe and trace, real headless browser:
  `node .claude/skills/onboard-fusion-component/scripts/run-fusion-probe-trace.mjs --component toolbar --api-set core`
  — wrote `traces/raw-ej2-core.trace.json` (13 trace rows, including `after-disable-true` showing `e-overlay` added to the root, `after-disable-false` removing it, and the `addItems`/`removeItems` item-id mutations).
- vendor-parity accounting (generated, not hand-counted):
  `node .claude/skills/onboard-fusion-component/scripts/compute-fusion-parity.mjs --component toolbar` — `22/22 = 100.0% -> PASS`.
- typed API coverage matrix, current against source:
  `node .claude/skills/onboard-fusion-component/scripts/write-fusion-typed-api-coverage.mjs --component toolbar --fusion-type FusionToolbar --check` — reported current.
- behavioral coverage gate (0b):
  `node .claude/skills/onboard-fusion-component/scripts/verify-behavioral-coverage.mjs --component toolbar` — `[PASS] toolbar — 10/10 members mapped, 6 proving test(s)`.
- behavior proof:
  `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Toolbar.WhenUsingFusionToolbar"` — 6/6 passed; the per-member outcome is recorded green in `proof/behavioral-coverage.json` and verified by the onboarding behavioral-coverage gate.

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
- Parity accounting: `discovery/parity-accounting.json`
- Raw EJ2 core probe: `probes/raw-ej2-core.html`
- Raw EJ2 core trace: `traces/raw-ej2-core.trace.json`
- Primitive map: `mapping/primitive-map.md`
- C# name decisions: `mapping/csharp-name-decisions.md`
- Vertical slice plan: `mapping/vertical-slice-plan.md`
- Typed API coverage matrix: `proof/typed-api-coverage-matrix.md`
- Behavioral coverage: `proof/behavioral-coverage.json`
- Playwright proof: `proof/playwright-proof.md`
- Blind review: `proof/blind-review.md`
- Skill pattern map: `_skill/pattern-map.md`

## Commit Boundary

This audit closes the Toolbar component row. The artifacts agree end to end on the
same member shape, payload shape, sync lane, C# API, runtime behavior, and
Playwright proof. The discovery JSON, trace, and coverage matrix remain generated
by the skill scripts and were not hand-edited; the parity accounting is the
hand-authored per-component judgment the parity tool consumes.
