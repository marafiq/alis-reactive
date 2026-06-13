# Grid Event Row: dataStateChange Sorting

Status: proven for this row. Raw EJ2 discovery, C# accepted-field implementation, and focused typed DSL Playwright proof are complete for the sorting row. The component audit remains open.

## Row Boundary

`dataStateChange` fired by a `sortColumn("name", "Descending", false)` Grid operation in custom-binding mode.

This row is only about the sorting trigger variant of `dataStateChange`. Paging, filtering, searching, grouping, and other trigger variants require separate focused probes and separate row artifacts before they can be mapped.

## Evidence

- Official Syncfusion API docs: `https://ej2.syncfusion.com/javascript/documentation/api/grid/datastatechangeeventargs`
- Official Syncfusion remote/custom-binding docs: `https://ej2.syncfusion.com/javascript/documentation/grid/data-binding/remote-data`
- Syncfusion local type source: `node_modules/@syncfusion/ej2-grids/src/grid/base/interface.d.ts:2470`
- Syncfusion local event declaration: `node_modules/@syncfusion/ej2-grids/src/grid/base/grid-model.d.ts:2072`
- Syncfusion local trigger path: `node_modules/@syncfusion/ej2-grids/src/grid/actions/data.js:594`
- Syncfusion local sort action path: `node_modules/@syncfusion/ej2-grids/src/grid/actions/sort.js:175`
- Probe: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/probes/raw-ej2-data-state-change-sorting.html`
- Trace: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/traces/raw-ej2-data-state-change-sorting.trace.json`
- Judgment calls: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/discovery/judgment-calls-data-state-change-sorting.md`

## Discovery Result

The first probe attempt used a plain array `dataSource`; sorting updated `sortSettings.columns` but did not fire `dataStateChange`. That setup was rejected because Syncfusion only emits the custom-binding `dataStateChange` action path when the grid is using the custom binding shape.

The corrected probe binds `dataSource` as `{ result, count }`, handles `dataStateChange`, and writes the next `{ result, count }` back into `grid.dataSource`.

Skill pattern feedback:

- `_skill/pattern-map.md#p001-event-variants-are-separate-rows-until-proven-equivalent`
- `_skill/pattern-map.md#p002-custom-binding-data-events-require-custom-binding-data-shape`
- `_skill/pattern-map.md#p003-proper-array-primitive-means-whole-typed-array-source-or-array-operation`
- `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them`

The deterministic trace hash was stable across reruns:

`bbe8b3be43b13c533732f7599adcf1814c53b216042757c53372e62d9d129fa7`

## Observed Sorting Payload

The sorting event emitted these top-level keys:

| Key | Observed type | Observed sample | Mapping status |
| --- | --- | --- | --- |
| `action` | object | sorting action object | partially typed today |
| `name` | string | `dataStateChange` | accepted and implemented |
| `requiresCounts` | boolean | `true` | accepted and implemented |
| `skip` | number | `0` | typed today |
| `sorted` | array | `[{"direction":"descending","name":"name"}]` | typed today as array/list |
| `take` | number | `2` | typed today |

Header-click sorting produced the same top-level keys as method-fired sorting.
The header-click trace is `traces/raw-ej2-data-state-change-sorting-header-click.trace.json`
with deterministic hash
`acc3f175469b5b04be8af51eab5a768f31fd1150dfb7a43c23be3f8cd3843fd1`.

These checked keys were absent for this sorting variant:

| Key | Reason |
| --- | --- |
| `where` | not emitted by sorting-only trigger |
| `search` | not emitted by sorting-only trigger |
| `group` | not emitted by sorting-only trigger |

## Observed Action Payload

The `action` object emitted these keys:

| Key | Observed type/sample | Mapping status |
| --- | --- | --- |
| `cancel` | `false` | accepted and implemented for read only |
| `columnName` | `name` | typed today |
| `direction` | `Descending` | typed today |
| `name` | `actionBegin` | accepted and implemented |
| `requestType` | `sorting` | typed today |
| `target` | method-fired trace: `null`; header-click trace: DOM `TH` element | excluded from public typed C# contract as browser-owned DOM object |
| `type` | `actionBegin` | accepted and implemented |

## Primitive Mapping

Use the existing typed component event payload primitive for `dataStateChange`.

Use the proper array primitive for `sorted`, with item type `{ name: string, direction: string }`. Do not model this as indexed paths.

Use object property primitives for `action.*`.

Do not add or change primitives for this row. Missing coverage means the typed Grid payload contract is incomplete, not that the primitive set is insufficient.

## C# DSL Judgment Boundary

Discovery records every observed payload field. C# DSL accepts only stable,
typed fields with a clear use case. See
`discovery/judgment-calls-data-state-change-sorting.md`.

## Current C# Audit

Current files:

- `Alis.Reactive.Fusion/Components/FusionGrid/Events/FusionGridOnDataStateChange.cs`
- `Alis.Reactive.Fusion/Components/FusionGrid/FusionGridEvents.cs`

Current contract covers the accepted sorting row:

- `FusionGridDataStateChangeArgs.Skip` maps `skip`
- `FusionGridDataStateChangeArgs.Take` maps `take`
- `FusionGridDataStateChangeArgs.Sorted` maps `sorted[]`
- `FusionGridSortColumn.Name` maps `sorted[].name`
- `FusionGridSortColumn.Direction` maps `sorted[].direction`
- `FusionGridAction.RequestType` maps `action.requestType`
- `FusionGridAction.ColumnName` maps `action.columnName`
- `FusionGridAction.Direction` maps `action.direction`

Additional accepted fields are implemented:

- `FusionGridDataStateChangeArgs.RequiresCounts`
- `FusionGridDataStateChangeArgs.Name`
- `FusionGridAction.Cancel`
- `FusionGridAction.Name`
- `FusionGridAction.Type`

`FusionGridAction.Target` is intentionally excluded because header-click sorting
proves it can be a DOM element. Do not expose it as `object` or `dynamic`.

## Typed DSL Proof

The row is proven by:

`scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGrid.sorting_a_column_fetches_sorted_data_and_echoes_action"`

Result: passed, TRX
`tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-173108.trx`.

This closes the sorting row only. It does not close paging, filtering,
searching, grouping, properties, methods, or the full Grid audit.
