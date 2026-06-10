# Grid C# Name Decisions

Status: active. C# name decisions for the `dataStateChange` sorting row and
typed `SortBy` method row are implemented, the page-number paging row uses already-present typed C# fields,
the filtering-method row narrows `Where` to the proven query shape, the
clear-filtering-method row keeps the existing `ClearFiltering()` method and
removes a masking manual reload, the searching-method row accepts the current `Search` descriptor shape, the
clear-search-method row keeps the existing `ClearSearch()` method without a
new primitive, the clear-sorting-method row keeps the existing `ClearSorting()`
method without a new primitive, the grouping-method row accepts the current `Group` shape, the
ungrouping-method row keeps `UngroupBy`, the clear-grouping-method row keeps
`ClearGrouping()` and accepts the final ungrouping action metadata, the record-click cell
row accepts the current `RecordClick<TRow>` shape, and the row-selected click
row corrects the current `RowSelected<TRow>` shape, and the toolbar-click
custom row corrects the current `ToolbarClick` shape. The action-begin
save/edit variant corrects part of the shared edit-action shape. The component
audit remains open. The begin-edit normal row has raw discovery, name
decisions, and focused typed DSL proof recorded.

## Pass Row

Close matrix row: `Html.FusionGrid(...).Reactive(evt => evt.DataStateChange, ...)` sorting trigger -> Grid data-state sorting payload -> async request gather plus visible grid refresh behavior.

Close matrix row: `FusionGrid.SortBy(...)` method trigger -> typed Grid sort method call -> Syncfusion `sortColumn` data-state refresh with typed sorted payload and visible grid refresh behavior.

Close matrix row: `FusionGrid.ClearSorting()` method trigger -> clear active Grid sort state -> async request gather omits absent sorted payload plus visible grid refresh behavior.

## Evidence Inputs

- Raw row artifact: `discovery/event-row-data-state-change-sorting.md`
- Raw trace: `traces/raw-ej2-data-state-change-sorting.trace.json`
- Header-click trace: `traces/raw-ej2-data-state-change-sorting-header-click.trace.json`
- Syncfusion source type: `DataStateChangeEventArgs`
- Existing C# event args: `Alis.Reactive.Fusion/Components/FusionGrid/Events/FusionGridOnDataStateChange.cs`
- Existing event selector: `Alis.Reactive.Fusion/Components/FusionGrid/FusionGridEvents.cs`
- Existing sandbox usage: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/Index.cshtml`
- Judgment calls: `discovery/judgment-calls-data-state-change-sorting.md`
- Paging row artifact: `discovery/event-row-data-state-change-paging.md`
- Paging judgment calls: `discovery/judgment-calls-data-state-change-paging.md`
- Filtering method row artifact: `discovery/event-row-data-state-change-filtering-method.md`
- Filtering method judgment calls: `discovery/judgment-calls-data-state-change-filtering-method.md`
- Clear-filtering method row artifact: `discovery/event-row-data-state-change-clear-filtering-method.md`
- Clear-filtering method judgment calls: `discovery/judgment-calls-data-state-change-clear-filtering-method.md`
- Searching method row artifact: `discovery/event-row-data-state-change-searching-method.md`
- Searching method judgment calls: `discovery/judgment-calls-data-state-change-searching-method.md`
- Clear-search method row artifact: `discovery/event-row-data-state-change-clear-search-method.md`
- Clear-search method judgment calls: `discovery/judgment-calls-data-state-change-clear-search-method.md`
- Clear-sorting method row artifact: `discovery/event-row-data-state-change-clear-sorting-method.md`
- Clear-sorting method judgment calls: `discovery/judgment-calls-data-state-change-clear-sorting-method.md`
- Grouping method row artifact: `discovery/event-row-data-state-change-grouping-method.md`
- Grouping method judgment calls: `discovery/judgment-calls-data-state-change-grouping-method.md`
- Ungrouping method row artifact: `discovery/event-row-data-state-change-ungrouping-method.md`
- Ungrouping method judgment calls: `discovery/judgment-calls-data-state-change-ungrouping-method.md`
- Clear-grouping method row artifact: `discovery/event-row-data-state-change-clear-grouping-method.md`
- Clear-grouping method judgment calls: `discovery/judgment-calls-data-state-change-clear-grouping-method.md`
- RecordClick cell row artifact: `discovery/event-row-record-click-cell.md`
- RecordClick cell judgment calls: `discovery/judgment-calls-record-click-cell.md`
- RowSelected click row artifact: `discovery/event-row-row-selected-click.md`
- RowSelected click judgment calls: `discovery/judgment-calls-row-selected-click.md`
- ToolbarClick custom row artifact: `discovery/event-row-toolbar-click-custom.md`
- ToolbarClick custom judgment calls: `discovery/judgment-calls-toolbar-click-custom.md`
- ActionBegin save/edit row artifact: `discovery/event-row-action-begin-save-edit.md`
- ActionBegin save/edit judgment calls: `discovery/judgment-calls-action-begin-save-edit.md`
- BeginEdit normal row artifact: `discovery/event-row-begin-edit-normal.md`
- BeginEdit normal judgment calls: `discovery/judgment-calls-begin-edit-normal.md`

## Name Decision Matrix

| Syncfusion payload path | Current C# name | Decision | Reason |
| --- | --- | --- | --- |
| `skip` | `FusionGridDataStateChangeArgs.Skip` | keep | exact semantic match, typed as number/int |
| `take` | `FusionGridDataStateChangeArgs.Take` | keep | exact semantic match, typed as number/int |
| `sorted` | `FusionGridDataStateChangeArgs.Sorted` | keep field name | exact Syncfusion name and existing sandbox usage |
| `sorted[].name` | `FusionGridSortColumn.Name` | keep | exact Syncfusion item key; do not rename to `Field` unless a row proves the API should hide vendor shape |
| `sorted[].direction` | `FusionGridSortColumn.Direction` | keep | exact Syncfusion item key |
| `action` | `FusionGridDataStateChangeArgs.Action` | keep | exact top-level concept; supports conditions and visible status |
| `action.requestType` | `FusionGridAction.RequestType` | keep | exact semantic match |
| `action.columnName` | `FusionGridAction.ColumnName` | keep | exact semantic match |
| `action.direction` | `FusionGridAction.Direction` | keep | exact semantic match |
| `requiresCounts` | none | add `RequiresCounts` | observed top-level own key and Syncfusion `DataStateChangeEventArgs` declares it |
| top-level `name` | none | add `Name` | Syncfusion `Observer.notify` injects `argument.name = property`; both traces observed `dataStateChange` |
| `action.cancel` | none | add `Cancel` | observed nested own key and Syncfusion `GridActionEventArgs` declares it |
| `action.name` | none | add `Name` | observed nested own key from Syncfusion event observer; keeps payload key coverage complete |
| `action.type` | none | add `Type` | observed nested own key and Syncfusion `GridActionEventArgs` declares it |
| `action.target` | none | exclude from public typed payload | method-fired trace emits `null`, header-click trace emits a DOM `TH` element; no public `object`/`dynamic` field and no stringly DOM shortcut |
| `sortColumn(columnName, direction, isMultiSort?)` | `FusionGrid.SortBy(Expression<Func<TRow, TField>> field, FusionGridSortDirection direction, bool keepExistingSorts = false)` | keep typed wrapper | public C# should take a typed row expression, map it through `ExpressionPathHelper`, and preserve Syncfusion's method semantics without exposing stringly field names |
| `sortColumn` direction argument | `FusionGridSortDirection` | keep | small enum protects the two real sort directions while `ToSyncfusion` writes the vendor spelling used by EJ2 |
| `sortColumn` `isMultiSort` argument | `keepExistingSorts` | keep | name states developer intent and maps directly to Syncfusion's "previously sorted columns are maintained" parameter |
| `action.currentPage` | `FusionGridAction.CurrentPage` | keep | observed in both paging traces as a number and created by `page.js` page-number action |
| `action.previousPage` | `FusionGridAction.PreviousPage` | keep | observed in both paging traces as a number and created by `page.js` page-number action |
| `action.pageSize` | `FusionGridAction.PageSize` | keep | observed in both paging traces as a number and created by `page.js` page-number action |
| `action.previousPageSize` | none | exclude for this row | source adds it only for page-size changes; absent in page-number paging traces |
| `action.rows` | none | exclude for this row | declared by `PageEventArgs` but absent in both traces and no typed DSL use case has been proven |
| `where` | `FusionGridDataStateChangeArgs.Where` | keep | exact data-state query key for server filtering |
| `where[].condition` | `FusionGridTextFilterCriterion.Condition` | keep | exact data-state query key |
| `where[].ignoreCase` | `FusionGridTextFilterCriterion.IgnoreCase` | keep | exact data-state query key |
| `where[].ignoreAccent` | `FusionGridTextFilterCriterion.IgnoreAccent` | keep | exact data-state query key |
| `where[].predicates` | `FusionGridTextFilterCriterion.Predicates` | keep | recursive predicate array; proper array primitive, no indexed paths |
| `where[].predicates[].field` | `FusionGridTextFilterCriterion.Field` | keep | exact data-state query key |
| `where[].predicates[].operator` | `FusionGridTextFilterCriterion.Operator` | keep | exact data-state query key |
| `where[].predicates[].value` | `FusionGridTextFilterCriterion.Value` | keep for text filter row | text-filter method row proves string values |
| `where[].isComplex` | `FusionGridTextFilterCriterion.IsComplex` | keep | source-backed `Predicate` node discriminator; server recursive filtering uses it to distinguish composite nodes from leaf predicates |
| `where[].predicate` | `FusionGridTextFilterCriterion.Predicate` before this row | remove | not emitted by top-level `where`; only appears in excluded filter settings object |
| `where[].matchCase` | `FusionGridTextFilterCriterion.MatchCase` before this row | remove | not emitted by top-level `where`; data-state query uses `ignoreCase` |
| `action.currentFilterObject` | none | exclude for this row | duplicates `where` and carries settings/internal shape |
| `action.columns` | none | exclude for this row | duplicates `where` and carries settings/internal shape |
| `action.currentFilteringColumn` | none | exclude for this row | duplicates `where[].predicates[].field` |
| `action.action` | none | exclude for this row | clear-filter row must prove usefulness before exposing |
| `search` | `FusionGridDataStateChangeArgs.Search` | keep | exact data-state query key for server searching |
| `search[].fields` | `FusionGridSearchDescriptor.Fields` | keep | exact search settings key |
| `search[].key` | `FusionGridSearchDescriptor.Key` | keep | exact search settings key |
| `search[].operator` | `FusionGridSearchDescriptor.Operator` | keep | exact search settings key |
| `search[].ignoreCase` | `FusionGridSearchDescriptor.IgnoreCase` | keep | exact search settings key |
| `search[].ignoreAccent` | `FusionGridSearchDescriptor.IgnoreAccent` | keep | exact search settings key |
| `action.searchString` | none | exclude for this row | duplicates `search[].key`; no distinct typed DSL use case |
| `clearSorting()` | `FusionGrid.ClearSorting()` | keep | exact Syncfusion method name; raw trace and local source prove it removes active sort columns and emits a sorting data-state event |
| clear-sorting `action.requestType` | `FusionGridAction.RequestType` | keep | observed as `sorting`; reuse shared action metadata rather than a new clear-sorting wrapper |
| clear-sorting `action.name` | `FusionGridAction.Name` | keep | observed as `actionBegin` |
| clear-sorting `action.type` | `FusionGridAction.Type` | keep | observed as `actionBegin` |
| clear-sorting `sorted` | current `Sorted` exists but not accepted for this row | exclude for this row | top-level key is absent after clear sorting; do not invent an empty array |
| clear-sorting `action.columnName` / `action.direction` / `action.cancel` | existing shared members, not accepted for this row | exclude for this row | these action keys are absent in the clear-sorting event; do not infer from sort-apply rows |
| clear-sorting `action.target` | none | exclude from public typed payload | method-fired trace emits `null`; no useful typed C# use case and target remains browser/gesture-owned by rule |
| clear-sorting `where` / `search` / `group` / `aggregates` | current event contracts where applicable | exclude for this row | variant-foreign or no-use keys are absent from this row |
| `group` | `FusionGridDataStateChangeArgs.Group` | keep | exact declared data-state grouped field array |
| `groups` | none | exclude for this row | duplicate observed alias; `DataStateChangeEventArgs` declares `group` and no distinct use case is proven |
| grouping `sorted` | `FusionGridDataStateChangeArgs.Sorted` | keep | grouping emits the active group sort state and existing `Sorted` shape matches it |
| grouping `action.columnName` | `FusionGridAction.ColumnName` | keep | exact group event field name from `GroupEventArgs` |
| `action.preventFocusOnGroup` | none | exclude for this row | Syncfusion internal focus flag; not a server data-state or DSL behavior input |
| `clearGrouping()` | `FusionGrid.ClearGrouping()` | keep | exact Syncfusion method name; raw trace and local source prove it delegates to final `ungroupColumn` and emits one final ungrouping data-state event |
| clear-grouping `action.requestType` | `FusionGridAction.RequestType` | keep | observed as `ungrouping`; reuse shared action metadata rather than a new clear-grouping wrapper |
| clear-grouping `action.name` | `FusionGridAction.Name` | keep | observed as `actionBegin` |
| clear-grouping `action.type` | `FusionGridAction.Type` | keep | observed as `actionBegin` |
| clear-grouping `action.columnName` | `FusionGridAction.ColumnName` | keep | observed as the final active grouped column `wing` after two setup groups; Syncfusion source explains final-column behavior |
| clear-grouping `group` / `groups` / `sorted` | current `Group`/`Sorted`, no `Groups` | exclude for this row | top-level keys are absent on clear grouping; do not invent empty arrays |
| clear-grouping `action.cancel` / `action.preventFocusOnGroup` | no `PreventFocusOnGroup`; existing `Cancel` not accepted for this row | exclude for this row | both are absent from the clear-grouping action payload |
| `recordClick.rowData` | `FusionGridRecordClickArgs<TRow>.RowData` | keep | exact Syncfusion key and typed row DTO use case |
| `recordClick.rowIndex` | `FusionGridRecordClickArgs<TRow>.RowIndex` | keep | exact Syncfusion key and stable scalar coordinate |
| `recordClick.cellIndex` | `FusionGridRecordClickArgs<TRow>.CellIndex` | keep | exact Syncfusion key and stable scalar coordinate |
| `recordClick.name` | `FusionGridRecordClickArgs<TRow>.Name` | keep | exact Syncfusion event metadata key |
| `recordClick.cancel` | none | exclude for this row | no cancel behavior proved; not useful typed DSL data |
| `recordClick.cell` | none | exclude for this row | browser-owned DOM element |
| `recordClick.row` | none | exclude for this row | browser-owned DOM element |
| `recordClick.target` | none | exclude for this row | browser-owned DOM element |
| `recordClick.event` | none | exclude for this row | browser-owned mouse event |
| `recordClick.column` | none | exclude for this row | broad EJ2 column object; separate column-source row required before exposing |
| `rowSelected.data` | `FusionGridRowSelectedArgs<TRow>.Data` | keep | exact Syncfusion key and typed selected row DTO use case |
| `rowSelected.rowIndex` | `FusionGridRowSelectedArgs<TRow>.RowIndex` | keep | exact Syncfusion key and stable scalar coordinate |
| `rowSelected.previousRowIndex` | `FusionGridRowSelectedArgs<TRow>.PreviousRowIndex` | change from `int` to `int?` | first click emits undefined; second click emits number, so nullable is the truthful typed shape |
| `rowSelected.isInteracted` | `FusionGridRowSelectedArgs<TRow>.IsInteracted` | keep | exact Syncfusion key and useful user-interaction flag for click row |
| `rowSelected.name` | none | add `Name` | observed top-level own key in both click variants; matches other accepted event metadata rows |
| `rowSelected.row` | none | exclude for this row | browser-owned DOM element |
| `rowSelected.previousRow` | none | exclude for this row | browser-owned DOM element when present |
| `rowSelected.target` | none | exclude for this row | browser-owned DOM element |
| `rowSelected.foreignKeyData` | none | exclude for this row | empty object in this row; separate foreign-key row required |
| `rowSelected.isHeaderCheckBoxClicked` | none | exclude for this row | checkbox-specific variant |
| `rowSelected.rowIndexes` | none | exclude for this row | duplicate scalar in single-selection row; multiple/range row required before exposing |
| `toolbarClick.item.id` | `FusionGridToolbarItem.Id` | keep | exact Syncfusion item key and stable command identity |
| `toolbarClick.item.text` | `FusionGridToolbarItem.Text` | keep | exact Syncfusion item key and useful command display text |
| `toolbarClick.cancel` | `FusionGridToolbarClickArgs.Cancel` | keep read flag | observed top-level flag; mutation/default-action prevention requires separate built-in action row |
| `toolbarClick.name` | none | add `Name` | observed top-level own key; matches accepted event metadata convention |
| `toolbarClick.originalEvent` | none | exclude for this row | browser-owned pointer event |
| `toolbarClick.item.tooltipText` | none | exclude for this row | item config metadata; no focused behavior use case |
| `toolbarClick.item.prefixIcon` | none | exclude for this row | presentation metadata |
| `toolbarClick.item.suffixIcon` | none | exclude for this row | presentation metadata |
| `toolbarClick.item.disabled` | none | exclude for this row | disabled-item row required |
| `toolbarClick.item.visible` | none | exclude for this row | visibility row required |
| `toolbarClick.item.type` | none | exclude for this row | toolbar rendering metadata |
| `toolbarClick.item.align` | none | exclude for this row | toolbar layout metadata |
| `beginEdit.rowData` | `FusionGridBeginEditArgs<TRow>.RowData` | keep | typed row DTO before edit; useful for edit-gating conditions |
| `beginEdit.rowIndex` | `FusionGridBeginEditArgs<TRow>.RowIndex` | keep | observed row coordinate in normal edit trace |
| `beginEdit.type` | `FusionGridBeginEditArgs<TRow>.Type` | keep | observed edit-mode metadata and current public contract |
| `beginEdit.cancel` | `FusionGridBeginEditArgs<TRow>.Cancel` | keep read flag | observed top-level flag; cancel mutation is separately exposed by `Cancel()` |
| writable `beginEdit.cancel` | `FusionGridBeginEditArgs<TRow>.Cancel()` | keep mutation method | raw trace proves setting `args.cancel = true` prevents the edited row from rendering |
| `beginEdit.row` | none | exclude for this row | browser-owned DOM row |
| `beginEdit.foreignKeyData` | none | exclude for this row | empty object; foreign-key row required |
| `beginEdit.isScroll` | none | exclude for this row | internal scroll metadata |
| `beginEdit.name` | none | exclude for this row | duplicate event identity metadata; no clear C# behavior use case |
| `beginEdit.primaryKey` | none | exclude for this row | duplicate key metadata; use typed `RowData` |
| `beginEdit.primaryKeyValue` | none | exclude for this row | duplicate key metadata; use typed `RowData` |
| `beginEdit.requestType` | none | exclude for this row | duplicate event lifecycle metadata; event selector already owns `beginEdit` identity |
| `beginEdit.target` | none | exclude for this row | undefined; no typed use case |
| `actionBegin.name` | none | add `Name` | observed top-level own key in save/edit trace |
| `actionBegin.requestType` | `FusionGridEditActionArgs<TRow>.RequestType` | keep | exact Syncfusion key; save/edit trace value is `save` |
| `actionBegin.action` | `FusionGridEditActionArgs<TRow>.Action` | keep | exact Syncfusion key; save/edit trace value is `edit` |
| `actionBegin.type` | `FusionGridEditActionArgs<TRow>.Type` | keep | exact Syncfusion key; save/edit trace value is `actionBegin` |
| `actionBegin.cancel` | `FusionGridEditActionArgs<TRow>.Cancel` | keep read flag | observed top-level flag; mutation/default-action prevention requires separate row |
| `actionBegin.data` | `FusionGridEditActionArgs<TRow>.Data` | keep | edited row DTO after save; useful typed current value source |
| `actionBegin.previousData` | `FusionGridEditActionArgs<TRow>.PreviousData` | keep | original row DTO before save; useful typed previous value source |
| `actionBegin.rowIndex` | none | add `RowIndex` as nullable int | observed row coordinate in save/edit trace; other variants may omit it |
| `actionBegin.selectedRow` | `FusionGridEditActionArgs<TRow>.SelectedRow` | keep existing | observed scalar, but no stronger semantics than Syncfusion payload read are claimed |
| `actionBegin.index` | none | remove `FusionGridEditActionArgs<TRow>.Index` | not emitted as an own key in save/edit trace; future variant must prove a useful typed member before reintroducing |
| `actionBegin.row` | none | exclude for this row | browser-owned DOM row |
| `actionBegin.form` | none | exclude for this row | browser-owned DOM form |
| `actionBegin.target` | none | exclude for this row | undefined; no typed use case |
| `actionBegin.foreignKeyData` | none | exclude for this row | empty object; foreign-key row required |
| `actionBegin.isScroll` | none | exclude for this row | internal scroll metadata |
| `actionBegin.primaryKey` | none | exclude for this row | no clear C# DSL behavior use case |
| `actionBegin.primaryKeyValue` | none | exclude for this row | no clear C# DSL behavior use case |
| `actionBegin.rowData` | none | exclude for this row | duplicates original row data covered by `PreviousData` |
| `actionComplete.name` | `FusionGridEditActionArgs<TRow>.Name` | keep shared member | observed top-level own key in save/edit trace; value is `actionComplete` |
| `actionComplete.requestType` | `FusionGridEditActionArgs<TRow>.RequestType` | keep shared member | exact Syncfusion key; save/edit trace value is `save` |
| `actionComplete.action` | `FusionGridEditActionArgs<TRow>.Action` | keep shared member | exact Syncfusion key; save/edit trace value is `edit` |
| `actionComplete.type` | `FusionGridEditActionArgs<TRow>.Type` | keep shared member | exact Syncfusion key; save/edit trace value is `actionComplete` |
| `actionComplete.cancel` | `FusionGridEditActionArgs<TRow>.Cancel` | keep read flag | observed top-level flag; mutation/default-action prevention requires separate row |
| `actionComplete.data` | `FusionGridEditActionArgs<TRow>.Data` | keep shared member | edited row DTO after save; useful typed current value source |
| `actionComplete.previousData` | `FusionGridEditActionArgs<TRow>.PreviousData` | keep shared member | original row DTO before save; useful typed previous value source |
| `actionComplete.rowIndex` | `FusionGridEditActionArgs<TRow>.RowIndex` | keep shared member | observed row coordinate in save/edit trace; shared member is nullable for variant safety |
| `actionComplete.selectedRow` | `FusionGridEditActionArgs<TRow>.SelectedRow` | keep existing shared member | observed scalar, but no stronger semantics than Syncfusion payload read are claimed |
| `actionComplete.index` | none | keep removed | not emitted as an own key in save/edit trace; do not reintroduce |
| `actionComplete.promise` | none | exclude for this row | emitted as own key with undefined value; no predictable typed C# behavior |
| `actionComplete.row` | none | exclude for this row | browser-owned DOM row |
| `actionComplete.form` | none | exclude for this row | browser-owned DOM form |
| `actionComplete.target` | none | exclude for this row | undefined; no typed use case |
| `actionComplete.foreignKeyData` | none | exclude for this row | empty object; foreign-key row required |
| `actionComplete.isScroll` | none | exclude for this row | internal scroll metadata |
| `actionComplete.primaryKey` | none | exclude for this row | no clear C# DSL behavior use case |
| `actionComplete.primaryKeyValue` | none | exclude for this row | no clear C# DSL behavior use case |
| `actionComplete.rowData` | none | exclude for this row | duplicates original row data covered by `PreviousData` |

## Blazor Naming Rule

Blazor metadata may be used only as naming evidence after the EJ2 row is proven. It must not override the observed EJ2 payload keys above. This row has not yet completed a Blazor metadata review.

## Discovery Versus C# DSL Boundary

All observed fields stay in discovery. Only fields with clear, typed, predictable
Fusion use cases are accepted into public C# event args. `action.target` remains
discovered but excluded because the header-click trace proves it can be a DOM
element; exposing it as `object` or `dynamic` would pollute the public DSL.

## Required Reviewer Findings Before Implementation

- Principal DSL reviewer must confirm no primitive changes are needed.
- Fusion discovery reviewer must confirm the sorting trace covers both method-fired and user header-click sorting if the row claims both triggers are equivalent.
- C# API reviewer decision: expose safe scalar/boolean metadata fields; exclude browser-owned DOM `action.target` per `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them`.
- Vertical slice reviewer must confirm whether `FusionGridAction` remains in `Events/FusionGridOnDataStateChange.cs` or should split into a partial/nested event contract file.
- Playwright behavior reviewer must confirm each accepted property has a user-visible/request-body proof.

## Implementation Boundary

Allowed implementation for this row only:

- add accepted properties to `FusionGridDataStateChangeArgs` and `FusionGridAction`;
- adjust comments only when they describe proven payload shape;
- add typed DSL proof page/test for sorting only.

Not allowed in this row:

- adding new primitives;
- changing other Grid event variants;
- broad Grid API cleanup;
- Kanban/Schedule implementation;
- product raw-probe test routes.

## Paging Boundary

For page-number paging, no C# implementation change is needed. The accepted
fields are already `CurrentPage`, `PreviousPage`, and `PageSize` on
`FusionGridAction`. Do not add `PreviousPageSize`, `Rows`, or `Target` from this
row.

## Filtering Boundary

For method-trigger filtering, public `Where` is the data-state query shape, not
the Syncfusion filter-settings object. Keep `Field`, `Operator`, `Value`,
`Condition`, `IsComplex`, `Predicates`, `IgnoreCase`, and `IgnoreAccent`. Remove or keep out
`Predicate`, `MatchCase`, `Uid`, `IsForeignKey`,
`ActualFilterValue`, `ActualOperator`, `action.currentFilterObject`,
`action.columns`, `action.currentFilteringColumn`, and `action.action` unless a
future row proves a distinct typed use case.

For method-trigger clear filtering, keep the existing public `ClearFiltering()`
method name because it maps directly to Syncfusion `clearFiltering()`. Do not
invent a clear-filter payload shape: raw EJ2 emits `requestType=refresh` and
omits top-level `where`. Do not expose filter-settings internals such as
`action.columns` or `action.currentFilterObject` for this row. The Directory
vertical slice must let the clear-filtering event lane refresh rows, not a
manual reload after the method call.

## Searching Boundary

For method-trigger searching, public `Search` is the data-state query shape:
`Fields`, `Key`, `Operator`, `IgnoreCase`, and `IgnoreAccent`. Do not add
`FusionGridAction.SearchString` from this row because it duplicates `Search.Key`.
Do not infer `Action.Cancel` from other action variants; the searching trace did
not emit it.

For method-trigger clear-search, keep the existing public `ClearSearch()` method
name. It is the typed DSL use case and maps to Syncfusion search clearing
through the existing `Search(string.Empty)` implementation path. Do not create a
new primitive and do not expose `FusionGridAction.SearchString`; the trace shows
an empty nested `action.searchString`, while the top-level `search` key is absent.

## ClearSorting Boundary

For method-trigger clear sorting, keep the existing public `ClearSorting()`
method name because it maps directly to Syncfusion `clearSorting()`.
Syncfusion source removes active sort columns before emitting the model-change
action. The raw trace therefore emits `action.requestType=sorting` while
omitting top-level `sorted`. Keep the shared action metadata reads, but do not
claim `Sorted`, `Action.ColumnName`, `Action.Direction`, `Action.Cancel`, or
`Action.Target` for this clear row. The setup sort event proves the probe can
observe `sorted`; the clear event's absence is therefore meaningful and must
not be modeled as an empty array.

## Grouping Boundary

For method-trigger grouping, public `Group` is the data-state query shape: a
whole typed array of grouped field names. Keep `Action.ColumnName` for the
field that was just grouped, and keep the existing `Sorted` mapping because
Syncfusion emits the group auto-sort state. Do not add
`FusionGridDataStateChangeArgs.Groups` or
`FusionGridAction.PreventFocusOnGroup`; both are observed, documented, and
excluded for this row.

## Ungrouping Boundary

For method-trigger ungrouping, keep the existing typed method name
`UngroupBy(...)` because it maps directly to Syncfusion `ungroupColumn(...)`
while preserving C# expression-based field selection. Keep
`Action.RequestType`, `Action.Name`, `Action.Type`, and `Action.ColumnName`
for this row. Do not claim `Group` as `[]`: the raw ungrouping event omits the
top-level `group` key, while the setup grouping event proves the probe was
capable of observing it. Do not add `Groups`, `Aggregates`, or
`PreventFocusOnGroup` from this row.

## ClearGrouping Boundary

For method-trigger clear grouping, keep the existing typed method name
`ClearGrouping()` because it maps directly to Syncfusion `clearGrouping()`.
Syncfusion source loops over active grouped columns and delegates to
`ungroupColumn`, enabling content refresh only for the final column. The raw
two-group trace therefore emits one `dataStateChange` event with
`action.requestType=ungrouping` and `action.columnName=wing`. Keep the shared
action metadata reads, but do not claim `Group`, `Groups`, `Sorted`,
`Aggregates`, `Cancel`, or `PreventFocusOnGroup` for this row.

## RecordClick Boundary

For data-cell `recordClick`, public C# exposes `RowData`, `RowIndex`,
`CellIndex`, and `Name`. Keep DOM/event/vendor objects out of this row:
`Cell`, `Row`, `Target`, `Event`, and `Column` are discovered but excluded.
Do not add `Cancel` without a separate row that proves cancellable behavior.

## RowSelected Boundary

For click-triggered `rowSelected`, public C# exposes `Data`, `RowIndex`,
nullable `PreviousRowIndex`, `IsInteracted`, and `Name`. Keep DOM objects and
variant-specific checkbox/foreign-key payloads out of this row. Do not expose
`RowIndexes` until a multiple or range selection row proves its array semantics
and use case.

## ToolbarClick Boundary

For custom-item `toolbarClick`, public C# exposes `Item.Id`, `Item.Text`,
`Cancel`, and `Name`. Keep `OriginalEvent` out as browser-owned event data.
Keep extra item configuration fields out until a focused row proves a useful C#
DSL behavior. Do not claim cancel mutation/default-action prevention until a
built-in toolbar action row proves it.

## ActionBegin Save/Edit Boundary

For normal edit-save `actionBegin`, public C# exposes event/action metadata,
current row `Data`, previous row `PreviousData`, nullable `RowIndex`, and the
existing `SelectedRow` scalar. Keep DOM/internal/duplicate payloads out.
`FusionGridEditActionArgs<TRow>` remains a shared payload type and cannot be
called fully audited until add/delete/cancel/actionComplete-related variants
are proven.

## Remote Whole Response Boundary

For Syncfusion custom binding, keep the existing whole-response overload:

| Source behavior | C# name | Decision | Reason |
| --- | --- | --- | --- |
| assign whole HTTP success body shaped as `{ result, count }` to `grid.dataSource` | `SetDataSource(ResponseBody<TResponse>)` | keep | the whole response object is the Syncfusion custom-binding data source and must emit `ReadWholePayload`; splitting into `Result` would lose the `count` pager total |
| `result` rows inside the response body | no separate public C# member for this row | keep inside whole response body | EJ2 consumes `result` after whole-body assignment; response-path overload remains separate |
| `count` total inside the response body | no separate public C# member for this row | keep inside whole response body | EJ2 consumes `count` for pager total; typed proof checks `200 items` |

Do not use this row to close `SetDataSource(ResponseBody<T>, path)`,
`SetDataSource(eventPayload, path)`, DataManager/adaptor, nested data-source
paths, or builder-owned initial `dataSource`.

## Data-Source Typed Array Boundary

For the focused data-source row, keep the existing names:

| Source behavior | C# name | Decision | Reason |
| --- | --- | --- | --- |
| read current `grid.dataSource` | `Data<TRow>()` | keep | concise component-source name already returns a typed array source |
| write typed array source to `grid.dataSource` | `SetDataSource(TypedSource<T[]>)` | keep | matches the component property being assigned and distinguishes the typed source overload |
| call `grid.refresh()` | `Refresh()` | keep | exact Syncfusion method name and visible post-render effect |

Do not use this row to rename or close `SetDataSource(ResponseBody<T>)`,
`SetDataSource(ResponseBody<T>, path)`, or `SetDataSource(eventPayload, path)`.
Those overloads have different value scopes and need separate proof.
