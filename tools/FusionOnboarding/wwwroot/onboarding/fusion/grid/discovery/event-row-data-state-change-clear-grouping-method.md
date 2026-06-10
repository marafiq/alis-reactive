# Grid Event Row: dataStateChange Clear Grouping Method

Status: proven for this row. Raw EJ2 discovery, judgment, typed DSL
Playwright proof, and generated matrix closure are complete for method-trigger
clear grouping. The component audit remains open.

## Row Boundary

`dataStateChange` fired by `grid.clearGrouping()` in custom-binding mode after
setup `grid.groupColumn("careLevel")` and `grid.groupColumn("wing")` calls.

This row covers only public method-trigger clearing of active grouping. Header
ungroup buttons, drag/drop ungrouping, nested lazy-load group expand/collapse,
initial group settings, and clear grouping combined with sorting/filtering/search
require separate focused rows before their payloads can be mapped or claimed
equivalent.

## Evidence

- Syncfusion local `clearGrouping` source:
  `node_modules/@syncfusion/ej2-grids/src/grid/actions/group.js`
- Method probe:
  `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/probes/raw-ej2-data-state-change-clear-grouping-method.html`
- Method trace:
  `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/traces/raw-ej2-data-state-change-clear-grouping-method.trace.json`
- Judgment calls:
  `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/discovery/judgment-calls-data-state-change-clear-grouping-method.md`

## Discovery Result

The probe starts with an ungrouped custom-binding Grid, calls
`groupColumn("careLevel")` and `groupColumn("wing")` to create a multi-column
grouped state, then calls `clearGrouping()`.

Syncfusion source backs the observed behavior: `clearGrouping()` clones
`groupSettings.columns`, loops through them, and delegates to `ungroupColumn`.
It sets `contentRefresh = true` only for the last ungroup call. The trace
therefore emits one clear-grouping data-state event for the last active grouped
column, `wing`.

The deterministic trace file hash from this pass:

`fc59803ac3cb75c0a3dc9f4a29dc536ae427fd9a86bdc79ee8293b7af7c004e8`

## Observed Clear-Grouping Payload

The clear-grouping method emitted these own keys:

| Key | Observed type | Observed sample | Mapping status |
| --- | --- | --- | --- |
| `action` | object | `{ requestType: "ungrouping", name: "actionBegin", type: "actionBegin", columnName: "wing" }` | accepted as action metadata for this row |
| `name` | string | `dataStateChange` | accepted |
| `requiresCounts` | boolean | `true` | accepted as visible event metadata |
| `skip` | number | `0` | accepted |
| `take` | number | `2` | accepted |

These checked keys were absent for this method-clear-grouping variant:

| Key | Reason |
| --- | --- |
| `group` | clearing grouping omits the grouped-field array; do not invent `[]` |
| `groups` | duplicate grouping alias is absent on the clear event |
| `sorted` | setup grouping emitted sorting state; clear grouping omitted it |
| `where` | belongs to filtering rows |
| `search` | belongs to searching rows |
| `aggregates` | not emitted by this clear-grouping row |

## Observed Action Payload

The `action` object emitted these keys:

| Key | Observed type/sample | Mapping status |
| --- | --- | --- |
| `requestType` | `ungrouping` | accepted through shared action metadata |
| `name` | `actionBegin` | accepted through shared action metadata |
| `type` | `actionBegin` | accepted through shared action metadata |
| `columnName` | `wing` | accepted as the last grouped column cleared by `clearGrouping()` |

`action.cancel` and `action.preventFocusOnGroup` were absent in the clear event.
Do not infer them from grouping setup or other lifecycle variants.

## Runtime Effect

After `clearGrouping()`:

- `grid.groupSettings.columns` is empty;
- group caption rows are absent;
- normal data rows are visible again;
- the first visible row cells are `1`, `Alpha`, `Memory Care`, `North`.

## Current C# Audit Boundary

The useful public C# member for this row is `FusionGrid.ClearGrouping()`, which
maps directly to Syncfusion `clearGrouping`. No new primitive is required.

Current accepted reads for this row:

- `FusionGridDataStateChangeArgs.Name`
- `FusionGridDataStateChangeArgs.Skip`
- `FusionGridDataStateChangeArgs.Take`
- `FusionGridDataStateChangeArgs.RequiresCounts`
- `FusionGridDataStateChangeArgs.Action`
- `FusionGridAction.RequestType`
- `FusionGridAction.Name`
- `FusionGridAction.Type`
- `FusionGridAction.ColumnName`

Current exclusions for this row:

- `FusionGridDataStateChangeArgs.Group`
- `FusionGridDataStateChangeArgs.Groups`
- `FusionGridDataStateChangeArgs.Sorted`
- `FusionGridDataStateChangeArgs.Where`
- `FusionGridDataStateChangeArgs.Search`
- `FusionGridDataStateChangeArgs.Aggregates`
- `FusionGridAction.Cancel`
- `FusionGridAction.PreventFocusOnGroup`

Do not infer `Group = []` from this row. The raw clear-grouping event omits the
top-level `group` key even after setup grouping events emitted it.
