# Grid Event Row: dataStateChange Clear Sorting Method

Status: raw EJ2 discovery, judgment, typed DSL Playwright proof, and generated
matrix closure are complete for this row.

## Row Boundary

`dataStateChange` fired by `grid.clearSorting()` in custom-binding mode after a
setup `grid.sortColumn("risk", "Descending", false)` call.

This row covers only public method-trigger clearing of active sorting. Header
sort cycling, column menu sort clearing, grouped-sort clearing, multi-sort
clearing, and clear sorting combined with filter/search/group state require
separate focused rows before their payloads can be mapped or claimed
equivalent.

## Evidence

- Syncfusion local `clearSorting` and `removeSortColumn` source:
  `node_modules/@syncfusion/ej2-grids/src/grid/actions/sort.js`
- Method probe:
  `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/probes/raw-ej2-data-state-change-clear-sorting-method.html`
- Method trace:
  `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/traces/raw-ej2-data-state-change-clear-sorting-method.trace.json`
- Judgment calls:
  `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/discovery/judgment-calls-data-state-change-clear-sorting-method.md`
- Typed DSL proof:
  `tests/Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.clear_sorting_method_clears_active_sort_and_refreshes_grid`
- Playwright TRX:
  `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260607-023440.trx`
- Skill pattern:
  `tools/FusionOnboarding/wwwroot/onboarding/fusion/_skill/pattern-map.md#p019-clear-and-reset-methods-must-not-be-masked-by-manual-reloads`

## Discovery Result

The probe starts with an unsorted custom-binding Grid, calls
`sortColumn("risk", "Descending", false)` to create an active sorted state, then
calls `clearSorting()`. The setup sorting event proves the probe can observe the
top-level `sorted` array before clear.

Syncfusion source backs the observed behavior: `clearSorting()` iterates the
current sort settings and delegates to `removeSortColumn`. `removeSortColumn`
emits a model-change action with `requestType="sorting"` after removing the
sort column, so the clear event carries sorting action identity without the
cleared top-level `sorted` descriptor.

The deterministic trace file hash is:

`771356b66a51ccfd5c0edaa399f2b6765e758168b98409b0106214fd3a3cba3c`

## Observed Clear-Sorting Payload

The clear-sorting method emitted these own keys:

| Key | Observed type | Observed sample | Mapping status |
| --- | --- | --- | --- |
| `action` | object | `{ requestType: "sorting", name: "actionBegin", type: "actionBegin", target: null }` | only stable action metadata accepted for this row |
| `name` | string | `dataStateChange` | accepted |
| `requiresCounts` | boolean | `true` | accepted as visible event metadata |
| `skip` | number | `0` | accepted |
| `take` | number | `2` | accepted |

These checked keys were absent for this method-clear-sorting variant:

| Key | Reason |
| --- | --- |
| `sorted` | clearing sorting omits the top-level sort descriptor; do not invent `[]` |
| `where` | belongs to filtering rows |
| `search` | belongs to searching rows |
| `group` | belongs to grouping rows |
| `aggregates` | not emitted by this clear-sorting row |

## Observed Action Payload

The `action` object emitted these keys:

| Key | Observed type/sample | Mapping status |
| --- | --- | --- |
| `requestType` | `sorting` | accepted through shared action metadata |
| `name` | `actionBegin` | accepted through shared action metadata |
| `type` | `actionBegin` | accepted through shared action metadata |
| `target` | `null` | excluded for this row; browser-owned/gesture-owned target has no typed use case |

`action.columnName`, `action.direction`, and `action.cancel` were absent in the
clear event. Do not infer them from sort-apply variants.

## Runtime Effect

After `clearSorting()`:

- `grid.sortSettings.columns` is empty;
- normal data rows are visible again in default order;
- the first visible row cells are `1`, `Alpha`, `Open`, `Low`.

## Current C# Audit Boundary

The useful public C# member for this row is `FusionGrid.ClearSorting()`, which
maps directly to Syncfusion `clearSorting`. No new primitive is required.

Current accepted reads for this row:

- `FusionGridDataStateChangeArgs.Name`
- `FusionGridDataStateChangeArgs.Skip`
- `FusionGridDataStateChangeArgs.Take`
- `FusionGridDataStateChangeArgs.RequiresCounts`
- `FusionGridDataStateChangeArgs.Action`
- `FusionGridAction.RequestType`
- `FusionGridAction.Name`
- `FusionGridAction.Type`

Current exclusions for this row:

- `FusionGridDataStateChangeArgs.Sorted`
- `FusionGridDataStateChangeArgs.Where`
- `FusionGridDataStateChangeArgs.Search`
- `FusionGridDataStateChangeArgs.Group`
- `FusionGridDataStateChangeArgs.Aggregates`
- `FusionGridAction.ColumnName`
- `FusionGridAction.Direction`
- `FusionGridAction.Cancel`
- `FusionGridAction.Target`

Do not infer `Sorted = []` from this row. The raw clear-sorting event omits the
top-level `sorted` key even after the setup sorting event emitted it.
