# Grid Vertical Slice Plan

Status: active. This plan covers the `dataStateChange` sorting row, typed
`SortBy` method row, page-number
paging row, filtering-method row, clear-filtering-method row, searching-method row, clear-search-method row, clear-sorting-method row, grouping-method row,
ungrouping-method row, clear-grouping-method row, record-click cell row, row-selected click row, and toolbar-click custom row. It
also covers the action-begin save/edit variant row, action-complete save/edit
accepted fields plus all listed public-contract exclusions, begin-edit normal
row, before-batch-save batch-edit row, cell-save batch-edit row, cell-saved
batch-edit row, remote whole-response row, and data-source typed-array row. It
cannot be used to claim Grid audit completion.

## Pass Row

Close matrix row: `Html.FusionGrid(...).Reactive(evt => evt.DataStateChange, ...)` sorting trigger -> Grid data-state sorting payload -> async request gather plus visible grid refresh behavior.

Close matrix row: `FusionGrid.SortBy(...)` method trigger -> typed Grid sort method call -> Syncfusion `sortColumn` data-state refresh with typed sorted payload and visible grid refresh behavior.

Close matrix row: `Html.FusionGrid(...).Reactive(evt => evt.DataStateChange, ...)` paging trigger -> Grid data-state paging payload -> async request gather plus visible grid refresh behavior.

Close matrix row: `Html.FusionGrid(...).Reactive(evt => evt.DataStateChange, ...)` filtering method trigger -> Grid data-state where payload -> async request gather plus visible grid refresh behavior.

Close matrix row: `FusionGrid.ClearFiltering()` method trigger -> clear active Grid filters -> async request gather omits absent where payload plus visible grid refresh behavior.

Close matrix row: `Html.FusionGrid(...).Reactive(evt => evt.DataStateChange, ...)` searching method trigger -> Grid data-state search payload -> async request gather plus visible grid refresh behavior.

Close matrix row: `FusionGrid.ClearSearch()` method trigger -> clear active Grid search -> async request gather omits absent search payload plus visible grid refresh behavior.

Close matrix row: `FusionGrid.ClearSorting()` method trigger -> clear active Grid sort state -> async request gather omits absent sorted payload plus visible grid refresh behavior.

Close matrix row: `Html.FusionGrid(...).Reactive(evt => evt.DataStateChange, ...)` grouping method trigger -> Grid data-state group payload -> async request gather plus visible grid refresh behavior.

Close matrix row: `FusionGrid.UngroupBy(...)` method trigger -> Grid ungrouping action payload -> async request gather plus visible ungrouped grid refresh behavior.

Close matrix row: `FusionGrid.ClearGrouping()` method trigger -> clear all active Grid grouping -> async request gather omits absent group payload plus visible ungrouped grid refresh behavior.

Close matrix row: `Html.FusionGrid(...).Reactive(evt => evt.RecordClick<TRow>(), ...)` cell trigger -> Grid record-click typed row payload -> sync visible event field updates.

Close matrix row: `Html.FusionGrid(...).Reactive(evt => evt.RowSelected<TRow>(), ...)` click trigger -> Grid row-selected typed row payload -> sync visible event field updates plus async selection request gather.

Close matrix row: `Html.FusionGrid(...).Reactive(evt => evt.ToolbarClick, ...)` custom item trigger -> Grid toolbar-click typed item payload -> sync visible event field updates.

Close variant row: `Html.FusionGrid(...).Reactive(evt => evt.ActionBegin<TRow>(), ...)` normal save/edit trigger -> Grid edit action typed current/previous row payload -> sync visible event field updates.

Close variant row: `Html.FusionGrid(...).Reactive(evt => evt.ActionComplete<TRow>(), ...)` normal save/edit trigger -> Grid edit action typed current/previous row payload -> sync visible event field updates. Broad `ActionComplete` remains open until all relevant variants are traced and proven.

Close matrix row: `Html.FusionGrid(...).Reactive(evt => evt.BeginEdit<TRow>(), ...)` normal edit trigger -> Grid begin-edit typed row payload -> sync visible event field updates plus cancel mutation. Typed DSL proof passed through focused `BeginEditNormal` vertical slice.

Close matrix row: `Html.FusionGrid(...).Reactive(evt => evt.BeforeBatchSave<TRow>(), ...)` batch-edit commit trigger -> Grid before-batch-save typed batch payload -> sync visible event field updates plus cancel mutation.

Close matrix row: `Html.FusionGrid(...).Reactive(evt => evt.CellSave<TRow, TValue>(), ...)` batch-edit cell-save trigger -> Grid cell-save typed edit payload -> sync visible event field updates plus cancel mutation.

Close matrix row: `Html.FusionGrid(...).Reactive(evt => evt.CellSaved<TRow, TValue>(), ...)` batch-edit cell-saved trigger -> Grid cell-saved typed post-save payload -> sync visible event field updates.

Close matrix row: `s.Component<FusionGrid>(id).SetDataSource(json)` -> Grid whole remote response body custom-binding refresh -> async HTTP success writes `{ result, count }` to `grid.dataSource` and visibly refreshes rows/count.

Close matrix row: `p.Component<FusionGrid>(id).SetDataSource(current.Where(...).AsSource()).Refresh()` -> Grid typed array data-source rebind -> sync component property read/write plus refresh method call after initial async HTTP load.

## Slice Organization

Keep the current component vertical slice:

- `FusionGrid.cs`
- `FusionGridBuilder.cs`
- `FusionGridHtmlExtensions.cs`
- `FusionGridExtensions.cs`
- `FusionGridEvents.cs`
- `FusionGridReactiveExtensions.cs`
- `FusionGridDataSourceExtensions.cs`
- `Events/FusionGridOnDataStateChange.cs`

For this row, the payload contract belongs in `Events/FusionGridOnDataStateChange.cs`. Do not split yet unless the accepted payload contract becomes large enough that reviewability suffers. If split later, keep the split by Grid event payload use case, not by arbitrary helper type.

## DSL Graph

```text
Html.FusionGrid(..., "residents-grid", ...)
  -> FusionGridBuilder.ElementId
  -> .Reactive(evt => evt.DataStateChange, (args, p) => ...)
  -> FusionGridEvents.DataStateChange
  -> TypedEvent<FusionGridDataStateChangeArgs>("dataStateChange", args)
  -> ComponentEventOnboarding.Wire(plan, componentId, "syncfusion", typedEvent, pipeline)
  -> PlanBuildContext.WireComponentEvent(componentId, vendor, eventName, reaction)
  -> StartsWhen.ComponentEvent(componentId, "dataStateChange")
  -> PipelineBuilder reaction
  -> p.Post(...).Gather(g => g.FromEvent(args, x => x.Sorted, "sorted"))
  -> GatherRequestInput assignment: payload.sorted <- event.sorted
  -> runtime component event payload scope
  -> async HTTP request
  -> response route updates FusionGrid dataSource
```

```text
Html.FusionGrid(..., "residents-grid", ...)
  -> dataStateChange sorting trigger
  -> p.Post("/Sandbox/Components/Grid/Data")
  -> response.OnSuccess<ResidentGridResponse>((json, s) => ...)
  -> s.Component<FusionGrid>("residents-grid").SetDataSource(json)
  -> ValueExpression.ReadWholePayload(successScope)
  -> SetReaction component.dataSource = { result, count }
  -> EJ2 custom-binding consumes result rows and count total
  -> visible Grid rows and pager update
```

```text
button click "#grid-sort-risk"
  -> p.Component<FusionGrid>("resident-directory-grid")
  -> .SortBy((ResidentDirectoryGridItem x) => x.RiskLevel, Descending)
  -> ExpressionPathHelper.ToEventPath(...) = "riskLevel"
  -> ComponentMethod sortColumn("riskLevel", "Descending", false)
  -> runtime CallReaction invokes EJ2 grid.sortColumn(...)
  -> Syncfusion emits dataStateChange
  -> existing DataStateChange vertical slice gathers sorted/action state
  -> async HTTP request refreshes visible resident-directory rows
```

```text
Html.FusionGrid(..., "roster-grid", ...)
  -> DomReady HTTP load
  -> ResponseBody.Residents
  -> ReactiveArray<T>.OrderBy(...)
  -> TypedSource<T[]>
  -> Component<FusionGrid>("roster-grid").SetDataSource(typedArraySource)
  -> SetReaction component.dataSource
  -> button click
  -> Component<FusionGrid>("roster-grid").Data<TRow>()
  -> component.dataSource typed source
  -> ReactiveArray<T>.Where(...).OrderBy(...)
  -> SetDataSource(typedArraySource)
  -> Refresh()
  -> visible Grid rows update
```

## Input/Output Matrix Row

| Source DSL call | Developer intent | C# domain term | JSON/generated TS term | Runtime behavior | Proof |
| --- | --- | --- | --- | --- | --- |
| `.Reactive(evt => evt.DataStateChange, (args, p) => p.Post(...).Gather(g => g.FromEvent(args, x => x.Sorted, "sorted")))` | When Grid sorting needs data, send the current sorted state to the server and refresh visible rows from the response | `TypedEvent<FusionGridDataStateChangeArgs>`, `FusionGridSortColumn`, `RequestInputAssignment`, `ValueExpression.ReadPayload` | `ComponentEventTrigger`, `GatherRequestInput`, `RequestPayloadTarget`, `ReadExpression` | event listener receives Syncfusion payload; gather reads event values; async request posts JSON; success sets Grid `dataSource` | focused typed DSL proof passed for sorting row |
| `p.Component<FusionGrid>(id).SortBy((ResidentDirectoryGridItem x) => x.RiskLevel, FusionGridSortDirection.Descending)` followed by `.Reactive(evt => evt.DataStateChange, ...)` event reads | When staff sort the resident directory by risk, call Syncfusion sorting through a typed row expression and refresh visible rows from the server sorted state | `ComponentMethod sortColumn`, `ExpressionPathHelper`, `FusionGridSortDirection`, `TypedEvent<FusionGridDataStateChangeArgs>`, `FusionGridSortColumn`, `ValueExpression.ReadPayload` | `CallReaction`, `ComponentEventTrigger`, `GatherRequestInput`, `RequestPayloadTarget` | runtime calls `sortColumn("riskLevel", "Descending", false)`, Syncfusion emits `dataStateChange`, gather sends whole `sorted` array, and success refreshes visible rows | focused typed DSL proof passed for `SortBy` method row |
| `.Reactive(evt => evt.DataStateChange, (args, p) => p.Post(...).Gather(g => g.FromEvent(args, x => x.Action.CurrentPage, "actionCurrentPage")))` | When Grid paging needs data, send page transition scalars to the server and refresh visible rows from the response | `TypedEvent<FusionGridDataStateChangeArgs>`, `FusionGridAction`, `RequestInputAssignment`, `ValueExpression.ReadPayload` | `ComponentEventTrigger`, `GatherRequestInput`, `RequestPayloadTarget`, `ReadExpression` | event listener receives Syncfusion payload; gather reads paging action values; async request posts JSON; success sets Grid `dataSource` | focused typed DSL proof passed for page-number paging row and paging variant exclusions |
| `.Reactive(evt => evt.DataStateChange, (args, p) => p.Post(...).Gather(g => g.FromEvent(args, x => x.Where, "where")))` | When Grid filtering needs data, send the current filter predicate tree to the server and refresh visible rows from the response | `TypedEvent<FusionGridDataStateChangeArgs>`, `FusionGridTextFilterCriterion`, `FusionGridTextFilterCriterion.IsComplex`, `RequestInputAssignment`, `ValueExpression.ReadPayload` | `ComponentEventTrigger`, `GatherRequestInput`, `RequestPayloadTarget`, `ReadExpression` | event listener receives Syncfusion payload; gather reads whole `where` array; async request posts JSON; server uses `IsComplex` to recurse; success sets Grid `dataSource` | focused typed DSL proof passed for filtering-method row |
| FilterBar typing `.Reactive(evt => evt.DataStateChange, (args, p) => p.Post(...).Gather(g => g.FromEvent(args, x => x.Where, "where")))` | When a user commits text in a Grid FilterBar input, send the current predicate tree to the server and refresh visible rows from the response | `TypedEvent<FusionGridDataStateChangeArgs>`, `FusionGridTextFilterCriterion`, `FusionGridTextFilterCriterion.IsComplex`, `RequestInputAssignment`, `ValueExpression.ReadPayload` | `ComponentEventTrigger`, `GatherRequestInput`, `RequestPayloadTarget`, `ReadExpression` | event listener receives Syncfusion FilterBar payload; gather reads whole `where` array; async request posts JSON; server uses `IsComplex` to recurse; success sets Grid `dataSource` | focused typed DSL proof passed for filterbar typing row |
| filtering-method exclusion boundary for settings-only action fields and variant-foreign `search`/`group`/`sorted` | Keep raw Syncfusion fields in discovery without adding public C# settings-object APIs for this row | no public settings-object action fields for this row | no request payload target for excluded action/settings members | runtime receives the raw event but the emitted typed request omits the raw `action` settings object and variant-foreign fields | focused typed DSL proof passed for filtering-method exclusion rows |
| filtering-filterbar exclusion boundary for settings-only action fields and variant-foreign data-state keys | Keep raw Syncfusion fields in discovery without adding public C# settings-object APIs for this row | no public settings-object action fields or absent declared data-state fields for this row | no request payload target for excluded action/settings members or declared-foreign data-state keys | runtime receives the raw event but the emitted typed request omits the raw `action` settings object and variant-foreign fields | focused typed DSL proof passed for filterbar typing exclusion rows |
| `p.Component<FusionGrid>(id).ClearFiltering()` followed by `.Reactive(evt => evt.DataStateChange, ...)` event reads | When staff clear active resident-directory filters, call Syncfusion clear filtering and refresh normal unfiltered rows without inventing empty `where` state | `ComponentMethod clearFiltering`, `TypedEvent<FusionGridDataStateChangeArgs>`, `FusionGridAction`, `ValueExpression.ReadPayload` | `CallReaction`, `ComponentEventTrigger`, `ReadExpression`, `GatherRequestInput` | runtime calls `clearFiltering`, Syncfusion emits `dataStateChange` with `action.requestType=refresh`, sync reactions read `Name`, `Skip`, `Take`, `RequiresCounts`, and action identity, and the async request refreshes normal rows while omitting absent `where` | focused typed DSL proof passed for clear-filtering-method row and clear-filtering variant exclusions |
| `.Reactive(evt => evt.DataStateChange, (args, p) => p.Post(...).Gather(g => g.FromEvent(args, x => x.Search, "search")))` | When Grid searching needs data, send the current search descriptor to the server and refresh visible rows from the response | `TypedEvent<FusionGridDataStateChangeArgs>`, `FusionGridSearchDescriptor`, `RequestInputAssignment`, `ValueExpression.ReadPayload` | `ComponentEventTrigger`, `GatherRequestInput`, `RequestPayloadTarget`, `ReadExpression` | event listener receives Syncfusion payload; gather reads whole `search` array; async request posts JSON; success sets Grid `dataSource` | focused typed DSL proof passed for searching-method row and searching variant exclusions |
| `p.Component<FusionGrid>(id).ClearSearch()` followed by `.Reactive(evt => evt.DataStateChange, ...)` event reads | When staff clear a resident-directory search, call Syncfusion search clearing and refresh normal unsearched rows without inventing empty search state | `ComponentMethod search("")`, `TypedEvent<FusionGridDataStateChangeArgs>`, `FusionGridAction`, `ValueExpression.ReadPayload` | `CallReaction`, `ComponentEventTrigger`, `ReadExpression`, `GatherRequestInput` | runtime calls the existing search method with empty string, Syncfusion emits `dataStateChange`, sync reactions read `Name`, `Skip`, `Take`, `RequiresCounts`, and action identity, and the async request refreshes normal rows while omitting absent `search` | focused typed DSL proof passed for clear-search-method row and clear-search variant exclusions |
| `p.Component<FusionGrid>(id).ClearSorting()` after typed `SortBy`, followed by `.Reactive(evt => evt.DataStateChange, ...)` event reads | When staff remove resident-directory sorting, call Syncfusion clear sorting and refresh default ordered rows without inventing empty `sorted` state | `ComponentMethod clearSorting`, `TypedEvent<FusionGridDataStateChangeArgs>`, `FusionGridAction`, `ValueExpression.ReadPayload` | `CallReaction`, `ComponentEventTrigger`, `ReadExpression`, `GatherRequestInput` | runtime calls `clearSorting`, Syncfusion emits `dataStateChange`, sync reactions read `Name`, `Skip`, `Take`, `RequiresCounts`, and action identity, and the async request refreshes default rows while omitting absent `sorted` | focused typed DSL proof passed for clear-sorting-method row and clear-sorting variant exclusions |
| `.Reactive(evt => evt.DataStateChange, (args, p) => p.Post(...).Gather(g => g.FromEvent(args, x => x.Group, "group")))` | When Grid grouping needs data, send the grouped fields to the server and refresh visible grouped rows from the response | `TypedEvent<FusionGridDataStateChangeArgs>`, `RequestInputAssignment`, `ValueExpression.ReadPayload` | `ComponentEventTrigger`, `GatherRequestInput`, `RequestPayloadTarget`, `ReadExpression` | event listener receives Syncfusion payload; gather reads whole `group` array; async request posts JSON; success sets Grid `dataSource` | focused typed DSL proof passed for grouping-method row and grouping variant exclusions |
| `p.Component<FusionGrid>(id).UngroupBy((ResidentDirectoryGridItem x) => x.CareLevel)` followed by `.Reactive(evt => evt.DataStateChange, ...)` event reads | When staff return a grouped resident directory to normal rows, call Syncfusion ungrouping and read the resulting action metadata without inventing absent group state | `ComponentMethod ungroupColumn`, `TypedEvent<FusionGridDataStateChangeArgs>`, `FusionGridAction`, `ValueExpression.ReadPayload` | `CallReaction`, `ComponentEventTrigger`, `ReadExpression`, `GatherRequestInput` | runtime calls `ungroupColumn`, Syncfusion emits `dataStateChange`, sync reactions read `Name`, `Skip`, `Take`, `RequiresCounts`, and action identity, and the async request refreshes normal rows | focused typed DSL proof passed for ungrouping-method row and ungrouping variant exclusions |
| `p.Component<FusionGrid>(id).ClearGrouping()` after typed `GroupBy` calls for care level and wing, followed by `.Reactive(evt => evt.DataStateChange, ...)` event reads | When staff remove all resident-directory grouping, call Syncfusion clear grouping and read the resulting final ungrouping action without inventing absent group state | `ComponentMethod clearGrouping`, `TypedEvent<FusionGridDataStateChangeArgs>`, `FusionGridAction`, `ValueExpression.ReadPayload` | `CallReaction`, `ComponentEventTrigger`, `ReadExpression`, `GatherRequestInput` | runtime calls `clearGrouping`, Syncfusion emits one final ungrouping `dataStateChange`, sync reactions read `Name`, `Skip`, `Take`, `RequiresCounts`, and final `Action.ColumnName`, and the async request refreshes normal rows while omitting absent `group` | focused typed DSL proof passed for clear-grouping-method row and clear-grouping variant exclusions |
| `.Reactive(evt => evt.RecordClick<ResidentDirectoryGridItem>(), (args, p) => p.Element(...).SetText(args, x => x.RowData.ResidentName))` | When a Grid data cell is clicked, read typed row data and coordinates into visible UI | `TypedEvent<FusionGridRecordClickArgs<TRow>>`, `ValueExpression.ReadPayload` | `ComponentEventTrigger`, `ReadExpression` | event listener receives Syncfusion payload; sync reactions read row data, row index, cell index, and event name | focused typed DSL proof passed for record-click cell row |
| `.Reactive(evt => evt.RowSelected<ResidentDirectoryGridItem>(), (args, p) => { p.Element(...).SetText(args, x => x.Data.ResidentName); p.Post(...).Gather(g => g.FromEvent(args, x => x.Data.ResidentId, "residentId")); })` | When a Grid data row is selected, read typed row data, transition scalars, interaction flag, and gather selected row id | `TypedEvent<FusionGridRowSelectedArgs<TRow>>`, `ValueExpression.ReadPayload`, `RequestInputAssignment` | `ComponentEventTrigger`, `ReadExpression`, `GatherRequestInput` | event listener receives Syncfusion payload; sync reactions read row data/scalars; async request posts selected row id and row index | focused typed DSL proof passed for row-selected click row |
| `.Reactive(evt => evt.ToolbarClick, (args, p) => p.When(args, x => x.Item.Id).Eq("emailStatements").Then(...))` | When a custom Grid toolbar command is clicked, branch by typed item id and read command metadata | `TypedEvent<FusionGridToolbarClickArgs>`, `FusionGridToolbarItem`, `ValueExpression.ReadPayload` | `ComponentEventTrigger`, `ReadExpression`, `ConditionGraph` | event listener receives Syncfusion payload; condition reads `item.id`; sync reactions read item text, cancel flag, and event name | focused typed DSL proof passed for toolbar-click custom row |
| `.Reactive(evt => evt.ActionBegin<ResidentDirectoryGridItem>(), (args, p) => p.When(args, x => x.RequestType).Eq("save").Then(...))` | When a normal Grid edit save begins, read current/previous row data and save/edit metadata | `TypedEvent<FusionGridEditActionArgs<TRow>>`, `ValueExpression.ReadPayload` | `ComponentEventTrigger`, `ReadExpression`, `ConditionGraph` | event listener receives Syncfusion payload; condition reads `requestType`; sync reactions read current data, previous data, row index, cancel flag, and event metadata | focused typed DSL proof passed for action-begin save/edit variant; shared payload still open |
| `.Reactive(evt => evt.ActionComplete<ResidentDirectoryGridItem>(), (args, p) => p.When(args, x => x.RequestType).Eq("save").Then(...))` | When a normal Grid edit save completes, read current/previous row data and save/edit metadata | `TypedEvent<FusionGridEditActionArgs<TRow>>`, `ValueExpression.ReadPayload` | `ComponentEventTrigger`, `ReadExpression`, `ConditionGraph` | event listener receives Syncfusion payload; condition reads `requestType`; sync reactions read current data, previous data, row index, cancel flag, and event metadata | focused typed DSL proof passed for action-complete save/edit accepted fields plus all listed public-contract exclusions; broad `ActionComplete` still open |
| `.Reactive(evt => evt.BeginEdit<ResidentDirectoryGridItem>(), (args, p) => ...)` | When a normal Grid edit begins, read the row context or cancel edit for locked records | `TypedEvent<FusionGridBeginEditArgs<TRow>>`, `ValueExpression.ReadPayload`, payload mutation via `Cancel()` | `ComponentEventTrigger`, `ReadExpression`, `SetReaction` to event payload `cancel` | event listener receives Syncfusion payload; sync reactions read row data, row index, type, cancel flag, or set cancel to prevent edit mode | focused typed DSL proof passed in `BeginEditNormal` |
| `.Reactive(evt => evt.BeforeBatchSave<ResidentDirectoryGridItem>(), (args, p) => ...)` | Before Syncfusion commits unsaved batch Grid changes, read the typed batch-change lists and optionally block the bulk save | `TypedEvent<FusionGridBeforeBatchSaveArgs<TRow>>`, `FusionGridBatchChanges<TRow>`, `ValueExpression.ReadPayload`, payload mutation via `Cancel()` | `ComponentEventTrigger`, `ReadExpression`, `SetReaction` to event payload `cancel` | event listener receives Syncfusion payload; sync reactions read changed records and cancel flag; condition on a changed value can set `cancel=true` to prevent the bulk-save lifecycle from reaching `actionComplete` | focused typed DSL proof passed for beforeBatchSave batch-edit accepted fields, cancel mutation, and `Name` exclusion; broad `BeforeBatchSave` still open |
| `.Reactive(evt => evt.CellSave<ResidentDirectoryGridItem, int>(), (args, p) => ...)` | When a batch Grid cell is saved, read the row context, edited field, new value, previous value, and optionally block invalid edits | `TypedEvent<FusionGridCellSaveArgs<TRow, TValue>>`, `ValueExpression.ReadPayload`, payload mutation via `Cancel()` | `ComponentEventTrigger`, `ReadExpression`, `ConditionGraph`, `SetReaction` to event payload `cancel` | event listener receives Syncfusion payload; sync reactions read row data, column name, value, previous value, and cancel flag; condition on value can set `cancel=true` to prevent the edited cell value | focused typed DSL proof passed for cellSave batch-edit accepted fields, cancel mutation, and listed exclusions; broad `CellSave` still open |
| `.Reactive(evt => evt.CellSaved<ResidentDirectoryGridItem, int>(), (args, p) => ...)` | After Syncfusion accepts a batch Grid cell save, read the row context, edited field, saved value, and previous value without exposing post-save cancellation | `TypedEvent<FusionGridCellSavedArgs<TRow, TValue>>`, `ValueExpression.ReadPayload` | `ComponentEventTrigger`, `ReadExpression` | event listener receives Syncfusion post-save payload; sync reactions read row data, column name, value, and previous value; no `Cancel()` mutation is exposed because raw EJ2 proves it is too late to prevent the saved value | focused typed DSL proof passed for cellSaved batch-edit accepted fields and listed exclusions; broad `CellSaved` still open |
| `s.Component<FusionGrid>("residents-grid").SetDataSource(json)` where `json` is `ResponseBody<ResidentGridResponse>` | Bind a server-shaped Syncfusion custom-binding response directly into the Grid after sorting, paging, or filtering | `ResponseBody<TResponse>`, `ComponentProperty<object> dataSource`, `ResidentGridResponse.Result`, `ResidentGridResponse.Count` | `ReadExpression` from success whole payload, JSON member `responseBody` with empty path, `SetReaction` | HTTP success route reads the whole response body and writes `{ result, count }` to `grid.dataSource`; Grid renders `result` and pager total from `count` | focused typed DSL proof passed for remote whole-response row |
| `p.Component<FusionGrid>("roster-grid").Data<ArrayGridModel, ResidentRow>()` then `.SetDataSource(current.Where(...).AsSource()).Refresh()` | Read current Grid rows, filter them client-side, rebind the Grid, and force visible refresh without a second HTTP request | `TypedComponentSource<TRow[]>`, `ReactiveArray<TRow>`, `ComponentProperty<object> dataSource`, `ComponentMethod refresh` | `ReadExpression`, `SetReaction`, `CallReaction` | runtime reads `grid.dataSource`, filters typed array values, writes `grid.dataSource`, then calls `grid.refresh()` | focused typed DSL proof passed for data-source typed-array row |

## Sync/Async Lane

- Event trigger dispatch: sync.
- Condition/set text reads inside the event pipeline: sync.
- HTTP `Post(...).Gather(...).Response(...)`: async.
- Grid data source update after response: sync reaction after async success route.
- Whole-response `SetDataSource(json)`: sync component property set inside the
  async HTTP success lane.
- Initial ArrayGrid roster load: async HTTP.
- Typed array data-source rebind and refresh: sync after the value source is available.
- Record-click row field updates: sync.
- Row-selected field updates: sync.
- Row-selected `Post(...).Gather(...)`: async request lane.
- Toolbar-click field updates and conditions: sync.
- Action-begin save/edit field updates and conditions: sync.
- Begin-edit normal field updates and cancel mutation: sync.

## Code To Delete Or Simplify

No deletion was needed for the currently proven rows. The accepted event
payload fields and data-source rows map to the existing typed event payload,
response body, component property, and method-call graph. If a future row starts
preserving helper code only because tests reference it, stop and re-check
whether the helper maps to the graph above.

## Behavior Proof

The typed DSL proof uses a real sandbox page, not raw probe HTML.

Sorting row proof:

1. Navigate through `scripts/playwright.sh`.
2. Trigger Grid sorting from visible UI.
3. Capture the server request and assert body contains the accepted `dataStateChange` sorting fields.
4. Assert the page-visible status fields read from event payload match the trace-backed fields.
5. Assert sorting-foreign `where`, `search`, and `group` payloads are absent from the typed request.
6. Keep `action.target` out of the typed public payload contract because it is a DOM object in the header-click trace.
7. Assert no console errors.

Paging row proof:

1. Navigate through `scripts/playwright.sh`.
2. Trigger Grid paging from visible pager UI.
3. Capture the server request and assert body contains `actionRequestType`, `actionCurrentPage`, `actionPreviousPage`, and `actionPageSize`.
4. Assert page-visible spans show the same paging values.
5. Assert request body omits paging-foreign `where`, `search`, `group`, and
   `sorted`.
6. Assert request body omits `actionPreviousPageSize`, `actionRows`, and
   `actionTarget`.
7. Assert public `FusionGridAction` omits `PreviousPageSize`, `Rows`, and
   `Target`.
8. Assert no console errors.

Filtering method row proof:

1. Navigate through `scripts/playwright.sh`.
2. Trigger filtering through the typed `FilterTextBy` method.
3. Parse the POST body as JSON and assert the `where` predicate tree.
4. Assert visible common metadata fields read from typed payload: `Name`, `Skip`,
   `Take`, `RequiresCounts`, `Action.RequestType`, `Action.Name`, `Action.Type`,
   and `Action.Cancel`.
5. Assert the visible directory summary and visible Grid rows are filtered to the requested wing.
6. Assert stale fields `matchCase` and `predicate` are absent from the request body.
7. Assert no console errors.

Clear-filtering method row proof:

1. Navigate through `scripts/playwright.sh`.
2. Trigger filtering through the typed `FilterTextBy` method and wait for filtered rows.
3. Trigger clear filtering through the typed `ClearFiltering` method.
4. Parse the clear POST body as JSON and assert `skip` and `take`.
5. Assert request body omits `where`, `search`, `group`, `sorted`, untyped
   `action`, and filter settings action fields.
6. Assert visible common metadata fields read from typed payload: `Name`, `Skip`,
   `Take`, `RequiresCounts`, `Action.RequestType`, and `Action.Name`.
7. Assert `where` is absent rather than an empty array for this row.
8. Assert visible summary returns to `240 residents matched`, and the first
   normal row renders.
9. Assert no console errors.

Searching method row proof:

1. Navigate through `scripts/playwright.sh`.
2. Trigger searching through the typed `Search` method.
3. Parse the POST body as JSON and assert the `search` descriptor.
4. Assert visible common metadata fields read from typed payload: `Name`, `Skip`,
   `Take`, `RequiresCounts`, `Action.RequestType`, `Action.Name`, and
   `Action.Type`.
5. Assert the visible directory summary and visible Grid rows are searched to Memory Care.
6. Assert duplicate `actionSearchString`, absent `actionCancel`, and
   searching-foreign `where`, `group`, and `sorted` are absent from the request.
7. Assert public `FusionGridAction` omits `SearchString`.
8. Assert no console errors.

Clear-search method row proof:

1. Navigate through `scripts/playwright.sh`.
2. Trigger searching through the typed `Search` method and wait for the searched state.
3. Trigger clear-search through the typed `ClearSearch` method.
4. Parse the clear POST body as JSON and assert `skip` and `take`.
5. Assert request body omits `search`, `where`, `group`, `sorted`, untyped
   `action`, `actionSearchString`, and `actionCancel`.
6. Assert visible common metadata fields read from typed payload: `Name`, `Skip`,
   `Take`, `RequiresCounts`, `Action.RequestType`, `Action.Name`, and
   `Action.Type`.
7. Assert `search` is absent rather than an empty array for this row.
8. Assert visible summary returns to `240 residents matched`, and the first
   normal row renders.
9. Assert no console errors.

SortBy method row proof:

1. Navigate through `scripts/playwright.sh`.
2. Trigger sorting through the typed `SortBy((ResidentDirectoryGridItem x) => x.RiskLevel, Descending)` method.
3. Parse the POST body as JSON and assert `skip=0`, `take=8`, and one
   `sorted` entry with `name=riskLevel` and `direction=descending`.
4. Assert request body omits `where`, `search`, `group`, `aggregates`, untyped
   `action`, and `actionTarget`.
5. Assert visible common metadata fields read from typed payload: `Name`, `Skip`,
   `Take`, `RequiresCounts`, `Action.RequestType`, `Action.Name`,
   `Action.Type`, `Action.Cancel`, `Action.ColumnName`, and `Action.Direction`.
6. Assert visible method status is `sortColumn called`.
7. Assert visible rows refresh to the server-sorted resident-directory order:
   first row `Grace Bennett` with risk `Moderate`.
8. Assert no console errors.

ClearSorting method row proof:

1. Navigate through `scripts/playwright.sh`.
2. Trigger sorting through the typed `SortBy` method and wait for the visibly
   sorted first row `Grace Bennett` with risk `Moderate`.
3. Trigger clear sorting through typed `ClearSorting()`.
4. Parse the clear POST body as JSON and assert `skip` and `take`.
5. Assert request body omits `sorted`, `where`, `search`, `group`,
   `aggregates`, untyped `action`, `actionColumnName`, `actionDirection`,
   `actionCancel`, and `actionTarget`.
6. Assert visible common metadata fields read from typed payload: `Name`, `Skip`,
   `Take`, `RequiresCounts`, `Action.RequestType`, `Action.Name`, and
   `Action.Type`.
7. Assert `sorted` is absent rather than an empty array for this row.
8. Assert visible summary returns to `240 residents matched`, the first normal
   row renders as `Amina Patel`, and the risk cell returns to `Low`.
9. Assert no console errors.

Grouping method row proof:

1. Navigate through `scripts/playwright.sh`.
2. Trigger grouping through the typed `GroupBy` method.
3. Parse the POST body as JSON and assert the `group` array.
4. Assert visible common metadata fields read from typed payload: `Name`, `Skip`,
   `Take`, `RequiresCounts`, `Action.RequestType`, `Action.Name`, `Action.Type`,
   and `Action.ColumnName`.
5. Assert duplicate `groups`, internal `actionPreventFocusOnGroup`, absent
   `actionCancel`, grouping-foreign `where` and `search`, and no-aggregate
   `aggregates` are absent from the request.
6. Assert public `FusionGridDataStateChangeArgs` omits `Groups` and public
   `FusionGridAction` omits `PreventFocusOnGroup`.
7. Assert the visible directory summary and rendered Grid caption rows prove server-returned grouping behavior.
8. Assert no console errors.

Ungrouping method row proof:

1. Navigate through `scripts/playwright.sh`.
2. Trigger grouping through the typed `GroupBy` method so the page is actually grouped.
3. Trigger ungrouping through the typed `UngroupBy` method.
4. Parse the POST body as JSON and assert `skip` and `take`.
5. Assert request body omits `requiresCounts`, `group`, `groups`, `sorted`,
   `where`, `search`, `aggregates`, untyped `action`, `actionCancel`, and
   `actionPreventFocusOnGroup`.
6. Assert visible common metadata fields read from typed payload: `Name`, `Skip`,
   `Take`, `RequiresCounts`, `Action.RequestType`, `Action.Name`,
   `Action.Type`, and `Action.ColumnName`.
7. Assert `group` is absent rather than an empty array for this row.
8. Assert rendered group caption count is `0`, visible summary returns to
   `240 residents matched`, and the first normal row renders.
9. Assert no console errors.

ClearGrouping method row proof:

1. Navigate through `scripts/playwright.sh`.
2. Trigger care-level grouping through typed `GroupBy`.
3. Trigger wing grouping through typed `GroupBy` so the page has two active
   grouped columns.
4. Trigger clear grouping through typed `ClearGrouping()`.
5. Parse the clear POST body as JSON and assert `skip` and `take`.
6. Assert request body omits `requiresCounts`, `group`, `groups`, `sorted`,
   `where`, `search`, `aggregates`, untyped `action`, `actionCancel`, and
   `actionPreventFocusOnGroup`.
7. Assert visible common metadata fields read from typed payload: `Name`, `Skip`,
   `Take`, `RequiresCounts`, `Action.RequestType`, `Action.Name`,
   `Action.Type`, and `Action.ColumnName`.
8. Assert `group` is absent rather than an empty array for this row, and
   `Action.ColumnName` is the final active group column `wing`.
9. Assert rendered group caption count is `0`, visible summary returns to
   `240 residents matched`, and the first normal row renders.
10. Assert no console errors.

Remote whole-response row proof:

1. Navigate through `scripts/playwright.sh`.
2. Trigger a real Grid header sort that posts to `/Sandbox/Components/Grid/Data`.
3. Capture the HTTP response and parse it as JSON.
4. Assert the response has `result` and `count`, with ten result rows and
   `count == 200`.
5. Assert `SetDataSource(json)` renders the first returned row into the Grid.
6. Assert the pager displays `200 items`, proving `count` is consumed by EJ2.
7. Assert no console errors.

Data-source typed-array row proof:

1. Navigate through `scripts/playwright.sh`.
2. Let the initial HTTP roster response flow through `p.From(json.Read(...))`.
3. Assert five visible Grid rows render and the first row is `Ada`, proving the
   typed array source was sorted and bound through `SetDataSource`.
4. Click the real `Show Active Only` button.
5. Assert `Data()` read the current Grid `dataSource`, the client-side array
   pipeline filtered it, `SetDataSource(TypedSource<T[]>)` wrote it back, and
   `Refresh()` visibly reduced the Grid to three active rows.
6. Assert discharged/critical rows are absent and no console errors are emitted.

RecordClick cell row proof:

1. Navigate through `scripts/playwright.sh`.
2. Click a real Grid data cell.
3. Assert visible resident text comes from typed `args.RowData.ResidentName`.
4. Assert visible row index, cell index, and event name come from the typed event payload.
5. Assert no console errors.

Passed command:

`scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.record_click_reads_typed_row_data_and_cell_coordinates"`

Passed TRX:

`tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-181927.trx`

RowSelected click row proof:

1. Navigate through `scripts/playwright.sh`.
2. Click the first real Grid data row to establish previous selection state.
3. Click the second real Grid data row.
4. Assert request JSON contains `residentId` from `args.Data.ResidentId` and `rowIndex` from `args.RowIndex`.
5. Assert visible resident text comes from typed `args.Data.ResidentName`.
6. Assert visible row index, previous row index, interaction flag, and event name come from the typed event payload.
7. Assert the async response updates the selected resident and summary.
8. Assert no console errors.

Passed command:

`scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.row_selected_reads_typed_row_data_previous_index_and_interaction_state"`

Passed TRX:

`tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-182350.trx`

ToolbarClick custom row proof:

1. Navigate through `scripts/playwright.sh`.
2. Click the real custom Grid toolbar item.
3. Assert the condition on `args.Item.Id` runs the toolbar workflow.
4. Assert visible item id and text come from typed `args.Item`.
5. Assert visible cancel flag and event name come from typed event payload.
6. Assert no console errors.

Passed command:

`scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridBilling.clicking_email_statements_toolbar_item_runs_the_toolbar_workflow"`

Passed TRX:

`tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-182943.trx`

ActionBegin save/edit variant proof:

1. Navigate through `scripts/playwright.sh`.
2. Select a real Grid row.
3. Start normal edit mode and change the resident name in the rendered edit input.
4. Save the row through the typed `EndEdit` command.
5. Assert visible request type, action, type, event name, cancel flag, row index, selected row, current row data, and previous row data come from the typed event payload.
6. Assert the visible Grid row contains the edited value.
7. Assert the public typed args contract does not expose removed/excluded
   `Row`, `Form`, `Target`, `ForeignKeyData`, `IsScroll`, `PrimaryKey`,
   `PrimaryKeyValue`, `RowData`, or `Index` members.
8. Assert no console errors.

Passed command:

`scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridEditing.action_begin_save_edit_reads_typed_current_previous_and_action_fields"`

Passed TRX:

`tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-223102.trx`

ActionComplete save/edit variant proof:

1. Navigate through `scripts/playwright.sh` to the focused
   `/Sandbox/Components/Grid/ActionCompleteSaveEdit` vertical slice.
2. Load rows through the page-owned typed `SetDataSource` path.
3. Select a real Grid row.
4. Start normal edit mode and change the resident name in the rendered edit
   input.
5. Save the row through the typed `EndEdit` command.
6. Assert visible request type, action, type, event name, cancel flag, row
   index, selected row, current row data, and previous row data come from the
   typed `actionComplete` event payload.
7. Assert the visible Grid row contains the edited value.
8. Assert the public typed args contract does not expose removed/excluded
   `Row`, `Form`, `Target`, `ForeignKeyData`, `IsScroll`, `PrimaryKey`,
   `PrimaryKeyValue`, `RowData`, `Index`, or `Promise` members.
9. Assert no console errors.

Passed command:

`scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridActionCompleteSaveEdit.action_complete_save_edit_reads_typed_current_previous_and_action_fields"`

Passed TRX:

`tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-221837.trx`

Remote whole-response row proof:

Passed command:

`scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGrid.sorting_a_column_fetches_sorted_data_and_echoes_action"`

Passed TRX:

`tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-203627.trx`

The proof parses the actual HTTP response, verifies the `{ result, count }`
shape, asserts ten returned rows and `count == 200`, then checks that the first
returned row renders in the Grid and that the pager shows `200 items`. It also
asserts the generated plan uses `member=responseBody` with an empty path and no
response-body member-path read for this Grid page.

Data-source typed-array row proof:

Passed command:

`scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenBindingArrayToGrid"`

Passed TRX:

`tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-201013.trx`

The server fixture is deliberately not name-sorted, so the visible row order
proves the typed `OrderBy` transformation. The click workflow also counts
requests to `/Sandbox/Components/ArrayGrid/Residents` from the click through the
visible filtered Grid assertions and asserts zero, proving `Data()` +
`SetDataSource(...)` + `Refresh()` rebinding is client-side for this row.

The proof may inspect request JSON as supporting evidence, but it cannot rely
only on plan JSON.

## Commit Boundary

The user instruction is currently: do not commit until the final goal is reached.

If commit discipline resumes later, the auditable row boundaries are:

1. `Close FusionGrid dataStateChange sorting row`
2. `Close FusionGrid dataStateChange paging row`
3. `Close FusionGrid dataStateChange filtering method row`
4. `Close FusionGrid ClearFiltering method row`
5. `Close FusionGrid dataStateChange searching method row`
6. `Close FusionGrid ClearSearch method row`
7. `Close FusionGrid dataStateChange grouping method row`
8. `Close FusionGrid dataStateChange ungrouping method row`
9. `Close FusionGrid recordClick cell row`
10. `Close FusionGrid rowSelected click row`
11. `Close FusionGrid toolbarClick custom item row`
12. `Close FusionGrid actionBegin save/edit variant row`
13. `Close FusionGrid actionComplete save/edit variant accepted fields and public-contract exclusion row`
14. `Close FusionGrid beginEdit normal edit row`
13. `Close FusionGrid beforeBatchSave batch-edit row`
14. `Close FusionGrid cellSave batch-edit row`
15. `Close FusionGrid cellSaved batch-edit row`
16. `Close FusionGrid remote whole-response data-source row`
17. `Close FusionGrid data-source typed-array read/rebind/refresh row`
