# Grid Event Row: dataStateChange Paging

Status: proven for this row. Raw EJ2 discovery, C# accepted-field mapping, and focused typed DSL Playwright proof are complete for page-number paging. The component audit remains open.

## Row Boundary

`dataStateChange` fired by moving from page 1 to page 2 in custom-binding mode.

This row covers two trigger variants:

- method trigger: `grid.goToPage(2)`;
- visible pager trigger: click page `2`.

The two traces produced equivalent `dataStateChange` payload shape for this row.
Page-size changes, virtual scrolling, filtering, searching, grouping, and sorting
state combinations require separate rows before they can be mapped.

## Evidence

- Syncfusion local type source: `node_modules/@syncfusion/ej2-grids/src/grid/base/interface.d.ts:2470`
- Syncfusion local page action type: `node_modules/@syncfusion/ej2-grids/src/grid/base/interface.d.ts:1273`
- Syncfusion local trigger path: `node_modules/@syncfusion/ej2-grids/src/grid/actions/page.js:173`
- Syncfusion local state trigger path: `node_modules/@syncfusion/ej2-grids/src/grid/actions/data.js:594`
- Method probe: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/probes/raw-ej2-data-state-change-paging-method.html`
- Method trace: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/traces/raw-ej2-data-state-change-paging-method.trace.json`
- Pager-click probe: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/probes/raw-ej2-data-state-change-paging-pager-click.html`
- Pager-click trace: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/traces/raw-ej2-data-state-change-paging-pager-click.trace.json`
- Judgment calls: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/discovery/judgment-calls-data-state-change-paging.md`

## Discovery Result

Both probes bind `dataSource` as `{ result, count }`, handle
`dataStateChange`, and write the next `{ result, count }` back into
`grid.dataSource`. This preserves the custom-binding path that emits
`dataStateChange`.

The deterministic trace file hashes were:

- method trigger: `8d78d4006529dbeb8b20a2dcab4ee4d321143bb6cd3ce51402f3bd186c924001`
- pager click: `17a856f355a3b3dc3d79dc0ef7cb951ddb51e1697b8a32c1ce0556885e9bc8b8`

## Observed Paging Payload

Both paging variants emitted these top-level keys:

| Key | Observed type | Observed sample | Mapping status |
| --- | --- | --- | --- |
| `action` | object | paging action object | accepted scalar members only |
| `name` | string | `dataStateChange` | accepted |
| `requiresCounts` | boolean | `true` | accepted |
| `skip` | number | `2` in raw probe, `10` in sandbox page | accepted |
| `take` | number | `2` in raw probe, `10` in sandbox page | accepted |

These checked keys were absent for this page-number paging variant:

| Key | Reason |
| --- | --- |
| `where` | belongs to filtering row |
| `search` | belongs to searching row |
| `group` | belongs to grouping row |
| `sorted` | absent because this row starts from an unsorted grid; sorting plus paging requires a combination row |

## Observed Action Payload

Both paging variants emitted these `action` keys:

| Key | Observed type/sample | Mapping status |
| --- | --- | --- |
| `cancel` | `false` | accepted for read only |
| `currentPage` | `2` | accepted |
| `name` | `actionBegin` | accepted |
| `pageSize` | `2` in raw probe, `10` in sandbox page | accepted |
| `previousPage` | `1` | accepted |
| `requestType` | `paging` | accepted |
| `type` | `actionBegin` | accepted |

## Discovery But Not C# For This Row

Discovery also checked related paging candidates from Syncfusion source:

| Candidate | Source/evidence | Decision for this row |
| --- | --- | --- |
| `action.previousPageSize` | `page.js:207` adds it only when page size changes | deferred to a page-size-change row |
| `action.rows` | declared on `PageEventArgs` but absent from both traces | excluded until a row proves a stable typed use case |
| `action.target` | not observed in either paging trace | excluded; no broad DOM/object field |

## Primitive Mapping

Use the existing typed component event payload primitive for
`dataStateChange`.

Use object property primitives for `action.*` paging scalars.

Do not add or change primitives for this row. The accepted fields are all
reachable through the existing typed event payload read and nested payload read
primitives.

## C# DSL Judgment Boundary

Discovery records every observed payload field and source candidate. C# DSL
accepts only stable, typed fields with a clear use case. See
`discovery/judgment-calls-data-state-change-paging.md`.

## Current C# Audit

Current files:

- `Alis.Reactive.Fusion/Components/FusionGrid/Events/FusionGridOnDataStateChange.cs`
- `Alis.Reactive.Fusion/Components/FusionGrid/FusionGridEvents.cs`

Current contract covers the accepted paging row:

- `FusionGridDataStateChangeArgs.Name` maps `name`
- `FusionGridDataStateChangeArgs.Skip` maps `skip`
- `FusionGridDataStateChangeArgs.Take` maps `take`
- `FusionGridDataStateChangeArgs.RequiresCounts` maps `requiresCounts`
- `FusionGridAction.RequestType` maps `action.requestType`
- `FusionGridAction.Name` maps `action.name`
- `FusionGridAction.Type` maps `action.type`
- `FusionGridAction.Cancel` maps `action.cancel`
- `FusionGridAction.CurrentPage` maps `action.currentPage`
- `FusionGridAction.PreviousPage` maps `action.previousPage`
- `FusionGridAction.PageSize` maps `action.pageSize`

The contract intentionally does not expose `PreviousPageSize`, `Rows`, or
`Target` for this row.

## Typed DSL Proof

The row is proven by:

`scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGrid.paging_fetches_next_page_with_correct_skip"`

Result: passed, TRX
`tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-232421.trx`.

This proof gathers every accepted paging scalar through typed event args and
asserts both request payload values and page-visible values. It also asserts
that the emitted typed request body omits paging-foreign `where`, `search`,
`group`, and `sorted`, omits deferred action keys `actionPreviousPageSize`,
`actionRows`, and `actionTarget`, and that public `FusionGridAction` omits
`PreviousPageSize`, `Rows`, and `Target`. It closes the page-number paging row
only. It does not close page-size changes, virtual scrolling, filtering,
searching, grouping, properties, methods, or the full Grid audit.
