# Grid Event Row: dataStateChange Ungrouping Method

Status: raw EJ2 discovery, judgment, generated typed coverage rows, and focused
typed DSL Playwright proof are complete for this ungrouping trigger row. The
component audit remains open.

## Row Boundary

`dataStateChange` fired by `grid.ungroupColumn("careLevel")` in custom-binding
mode after a setup `grid.groupColumn("careLevel")` call.

This row covers only the public method-trigger ungrouping variant. Header
ungroup buttons, drag/drop ungrouping, `clearGrouping()`, nested grouping,
grouped sort, lazy-load group expand/collapse, and initial group settings
require separate focused rows before their payloads can be mapped or claimed
equivalent.

## Evidence

- Syncfusion local `ungroupColumn` public method source:
  `node_modules/@syncfusion/ej2-grids/src/grid/actions/group.js`
- Ungrouping probe:
  `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/probes/raw-ej2-data-state-change-ungrouping-method.html`
- Ungrouping trace:
  `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/traces/raw-ej2-data-state-change-ungrouping-method.trace.json`
- Judgment calls:
  `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/discovery/judgment-calls-data-state-change-ungrouping-method.md`

## Discovery Result

The probe starts with an ungrouped custom-binding Grid, calls
`groupColumn("careLevel")` to create a grouped state, then calls
`ungroupColumn("careLevel")`. The setup grouping event proves the Grid is
actually grouped before the ungrouping call. The ungrouping event emits
`dataStateChange` with `action.requestType="ungrouping"`.

The deterministic trace file hash from this pass:

`d0eddc3d1bada900f469005040478b1fbb74ef013a062eb7d84e5fdc1d606902`

## Observed Payload

The ungrouping-method variant emitted these own keys:

| Key | Observed type | Observed sample | Mapping status |
| --- | --- | --- | --- |
| `action` | object | `{ requestType: "ungrouping", name: "actionBegin", type: "actionBegin", columnName: "careLevel" }` | accepted as `FusionGridAction` reads for this row |
| `name` | string | `dataStateChange` | accepted |
| `requiresCounts` | boolean | `true` | accepted |
| `skip` | number | `0` | accepted |
| `take` | number | `2` | accepted |

The ungrouping-method variant did not emit top-level `group`, `groups`,
`sorted`, `where`, `search`, or `aggregates`. The setup grouping event emitted
`group`, `groups`, and `sorted`, so absence on the ungrouping event is not a
probe limitation.

## Runtime Effect

After `ungroupColumn("careLevel")`:

- `grid.groupSettings.columns` is empty;
- group caption rows are absent;
- normal data rows are visible again;
- the first visible row cells are `1`, `Alpha`, `Memory Care`, `North`.

## Current C# Audit Boundary

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
- `FusionGridDataStateChangeArgs.Sorted`
- `FusionGridDataStateChangeArgs.Where`
- `FusionGridDataStateChangeArgs.Search`
- `FusionGridDataStateChangeArgs.Aggregates`
- `FusionGridDataStateChangeArgs.Groups`
- `FusionGridAction.Cancel`
- `FusionGridAction.PreventFocusOnGroup`

Do not infer a `Group = []` read from this row; the raw ungrouping event omits
the top-level `group` key.
