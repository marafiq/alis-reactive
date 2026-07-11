# Breadcrumb Audit Report

Status: audited and closed. `FusionBreadcrumb` is fully onboarded. Every public
typed member has deterministic raw EJ2 evidence, an explicit C# name decision, an
authoritative primitive mapping, a vertical-slice file, and focused typed DSL
Playwright proof bound to a fails-when-broken assertion. The generated typed API
coverage matrix is `audited` with every row `row-proven`.

## Accepted Members

The Breadcrumb slice onboards the post-render runtime surface; initial render
configuration (items, the initial active item, navigation flags, overflow mode)
stays on the Syncfusion `BreadcrumbBuilder`.

- `FusionBreadcrumb(...)` — the render helper that renders the trail and carries its controlled component id into the plan.
- `FusionBreadcrumbEvents.ItemClick` — the `itemClick` event selector, with `Reactive(...)` wiring through `ComponentEventOnboarding.Wire`.
- `FusionBreadcrumbItemClickArgs.Item` (`FusionBreadcrumbItem`) — the clicked crumb, narrowed to the proven scalar fields.
- `FusionBreadcrumbItem.Text` — the clicked crumb's label, read from `event.item.text`.
- `FusionBreadcrumbItem.Id` — the clicked crumb's id, read from `event.item.id`.
- `FusionBreadcrumbItem.Url` — the clicked crumb's url, read from `event.item.url`.
- `FusionBreadcrumbItem.IconCss` — the clicked crumb's icon classes (nullable), read from `event.item.iconCss`.
- `FusionBreadcrumbItem.Disabled` — the clicked crumb's disabled flag, read from `event.item.disabled`.
- `ActiveItem(this ComponentRef<FusionBreadcrumb, TModel> self)` — reads `activeItem` as a typed `string` source for conditions and set text.
- `SetActiveItem(this ComponentRef<FusionBreadcrumb, TModel> self, string activeItem)` — writes `activeItem` and chains `dataBind()` to repaint the trail and move `aria-current`.

## Excluded Candidates

- `itemClick.element`, `itemClick.event` — browser-owned DOM `HTMLElement`/`Event`; excluded from the public typed payload per `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them`.
- `itemClick.cancel` — cancel-the-default-navigation hook; `enableNavigation=false` already governs navigation and no focused Senior Living use case requires cancelling a crumb click.
- `itemClick.name` (inherited `BaseEventArgs.name`) — duplicate event identity metadata; the `ItemClick` selector already owns the event identity.
- `beforeItemRender`, `created` events — `beforeItemRender` carries a browser-owned `element`; `created` is a DOM-native lifecycle event with no typed payload; no focused Senior Living use case.
- `cssClass`, `disabled`, `enableActiveItemNavigation`, `enableNavigation`, `items`, `itemTemplate`, `maxItems`, `overflowMode`, `separatorTemplate`, `url` — builder-owned static configuration (`discovery/public-api-surface.json` marks each `builder.covered = true`); no post-render read/write proven necessary. `activeItem` is the one builder-covered property that additionally onboards a proven post-render read (`ActiveItem`) and write (`SetActiveItem`).
- `locale` — vendor-private member: `breadcrumb.d.ts:207-210` marks it `@private @aspIgnore`, an internal globalization hook, not a public runtime value (`discovery/parity-accounting.json`).
- `destroy()` method — lifecycle cleanup, not plan behavior (`discovery/public-api-surface.json` classifies it `skip`).

## Skill Pattern Map

This audit confirms existing patterns and adds one new pattern row.

- `_skill/pattern-map.md#p000-discovery-is-exhaustive-but-public-c-dsl-is-selective` — 16 EJ2 members discovered; 2 builder-covered members additionally onboard runtime behavior (`activeItem` read/write, `itemClick` event), the rest recorded and excluded with evidence.
- `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them` — `itemClick.element` and `itemClick.event` excluded as browser-owned DOM objects rather than exposed as `object`/`dynamic`.
- `_skill/pattern-map.md#p005-event-payload-coverage-is-property-level` — `FusionBreadcrumbItemClickArgs` is proven property by property (`Item.Text`, `Item.Id`, `Item.Url`, `Item.IconCss`, `Item.Disabled`), not as one class row.
- `_skill/pattern-map.md#p018-writable-payload-fields-need-lifecycle-effect-proof` — `SetActiveItem` is proven by the visible current crumb moving (`aria-current` from Care Plan to Eleanor Hughes), not by a silent property set.
- `_skill/pattern-map.md#p020-public-methods-need-their-own-row-proof` — `ActiveItem` read and `SetActiveItem` write each have their own row and their own fails-when-broken proof.
- NEW `_skill/pattern-map.md#p025-a-disabled-item-only-payload-member-is-proven-through-the-gather-body-not-a-disabled-item-click` — `FusionBreadcrumbItem.Disabled` is reachable only as `false` by a real click (Syncfusion's `pointer-events:none` on `.e-disabled` crumbs suppresses `itemClick`, verified in a real browser), so it is proven through the gather body (`"disabled":false` in `request.PostData`) rather than a disabled-crumb click or a non-discriminating Truthy/Else branch.

## Remote Data Lane

`Breadcrumb` is a navigation/display component: it renders a fixed hierarchical
trail and emits a click payload. It has no remote binding, paging, filtering,
lookup, virtualization, or server-query lane to onboard. The realistic remote
workflow for a breadcrumb is fetching the section a clicked crumb resolves to, which
the journey proves: clicking a crumb POSTs the crumb identity (`text`/`id`/`url`/`disabled`)
and the coordinator sees the server-resolved section summary and record code. This
satisfies the remote-behavior expectation for this component class (see
`references/automation-gates.md` Gate 6 applicability — Breadcrumb is not a
board/grid/scheduler/list/tree).

## Commands Run

- raw probe and trace, real headless browser:
  `node .claude/skills/onboard-fusion-component/scripts/run-fusion-probe-trace.mjs --component breadcrumb --api-set core`
  — wrote `traces/raw-ej2-core.trace.json` (15 trace rows).
- parity, generated by tool:
  `node .claude/skills/onboard-fusion-component/scripts/compute-fusion-parity.mjs --component breadcrumb` — reported `16/16 = 100.0% -> PASS` (14 builder-owned, 2 excluded-with-evidence: `destroy` skip + `locale` vendor-private).
- typed API coverage matrix, current against source:
  `node .claude/skills/onboard-fusion-component/scripts/write-fusion-typed-api-coverage.mjs --component breadcrumb --fusion-type FusionBreadcrumb --write` — 13 typed rows, all `row-proven`, `Status: audited`.
- onboarding status inventory:
  `node .claude/skills/onboard-fusion-component/scripts/report-fusion-onboarding-status.mjs --write`.
- behavior proof:
  `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Breadcrumb.WhenUsingFusionBreadcrumb"` — 7/7 passed.
- behavioral coverage gate (0b):
  `node .claude/skills/onboard-fusion-component/scripts/verify-behavioral-coverage.mjs --component breadcrumb` — `[PASS] breadcrumb — 13/13 members mapped, 7 proving test(s)`.
- artifact gate verifier:
  `node .claude/skills/onboard-fusion-component/scripts/verify-fusion-artifact-gates.mjs --component breadcrumb`.

## Coverage Matrix Summary

Current generated matrix count: 13 typed C# API rows, 0 supplemental audit rows,
13 total rows. The latest count check shows 13 row-proven matrix rows and 0
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

This audit closes the Breadcrumb component row. The artifacts agree end to end on
the same member shape, payload shape, sync lane, C# API, runtime behavior, and
Playwright proof. The discovery JSON, trace, and coverage matrix remain generated
by the skill scripts and were not hand-edited. No framework code (the
`FusionBreadcrumb` slice, runtime, or DSL) was changed; the rework is confined to
the sandbox view/controller/model, the Playwright test, and this artifact tree.
