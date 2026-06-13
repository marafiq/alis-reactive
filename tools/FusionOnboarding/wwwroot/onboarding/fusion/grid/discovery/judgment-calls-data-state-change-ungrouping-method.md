# Grid Judgment Calls: dataStateChange Ungrouping Method

Status: active for the ungrouping-method row.

## Accepted Members

| Raw member | C# member | Decision | Reason |
| --- | --- | --- | --- |
| `name` | `FusionGridDataStateChangeArgs.Name` | accepted | stable event identity, sample `dataStateChange` |
| `skip` | `FusionGridDataStateChangeArgs.Skip` | accepted | required by custom-binding response request |
| `take` | `FusionGridDataStateChangeArgs.Take` | accepted | required by custom-binding response request |
| `requiresCounts` | `FusionGridDataStateChangeArgs.RequiresCounts` | accepted | required by custom-binding `{ result, count }` response |
| `action.requestType` | `FusionGridAction.RequestType` | accepted | identifies the ungrouping behavior branch |
| `action.name` | `FusionGridAction.Name` | accepted | observed Syncfusion action identity `actionBegin` |
| `action.type` | `FusionGridAction.Type` | accepted | observed Syncfusion action lifecycle `actionBegin` |
| `action.columnName` | `FusionGridAction.ColumnName` | accepted | identifies the ungrouped field |

## Excluded Members

| Raw member | C# member | Decision | Reason |
| --- | --- | --- | --- |
| `group` | `FusionGridDataStateChangeArgs.Group` | excluded for this row | ungrouping event omits the top-level key; do not invent `[]` |
| `sorted` | `FusionGridDataStateChangeArgs.Sorted` | excluded for this row | ungrouping trace did not carry sorted state |
| `where` | `FusionGridDataStateChangeArgs.Where` | excluded for this row | ungrouping trace did not carry filter predicates |
| `search` | `FusionGridDataStateChangeArgs.Search` | excluded for this row | ungrouping trace did not carry search descriptors |
| `groups` | `FusionGridDataStateChangeArgs.Groups` | excluded globally | duplicate/internal group-state field; no public C# member |
| `aggregates` | `FusionGridDataStateChangeArgs.Aggregates` | excluded globally | no focused Senior Living use case in this row |
| `action.cancel` | `FusionGridAction.Cancel` | excluded for this row | ungrouping action did not expose cancel |
| `action.preventFocusOnGroup` | `FusionGridAction.PreventFocusOnGroup` | excluded globally | setup grouping emitted it, ungrouping did not; it is an internal focus hint |

## Senior Living Judgment

Ungrouping a resident directory by care level is a realistic operations
workflow: staff may group by care level to compare service cohorts, then
ungroup to return to a normal paged resident list. Useful typed API coverage is
the action identity, ungrouped column, request shape, and visible row refresh.
DOM-level grouped header details are not useful public C# DSL shape.
