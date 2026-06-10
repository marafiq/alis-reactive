# Grid Primitive Map

Status: active. This file maps the proven `dataStateChange` sorting row,
typed `SortBy` method row,
page-number paging row, filtering-method row, clear-filtering-method row, searching-method row,
clear-search-method row, clear-sorting-method row, grouping-method row, ungrouping-method row, clear-grouping-method row,
record-click cell row, row-selected click row,
toolbar-click custom row, action-begin save/edit variant row, action-complete
save/edit accepted fields plus all listed public-contract exclusions, remote
whole-response row, and data-source typed-array row. The component audit
remains open.

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

Close matrix row: `FusionGrid.UngroupBy(...)` method trigger -> Grid data-state ungrouping action payload -> async request gather plus visible ungrouped grid refresh behavior.

Close matrix row: `FusionGrid.ClearGrouping()` method trigger -> clear all active Grid grouping -> async request gather omits absent group payload plus visible ungrouped grid refresh behavior.

Close matrix row: `Html.FusionGrid(...).Reactive(evt => evt.RecordClick<TRow>(), ...)` cell trigger -> Grid record-click typed row payload -> sync visible event field updates.

Close matrix row: `Html.FusionGrid(...).Reactive(evt => evt.RowSelected<TRow>(), ...)` click trigger -> Grid row-selected typed row payload -> sync visible event field updates plus async selection request gather.

Close matrix row: `Html.FusionGrid(...).Reactive(evt => evt.ToolbarClick, ...)` custom item trigger -> Grid toolbar-click item identity payload -> sync visible event field updates.

Close variant row: `Html.FusionGrid(...).Reactive(evt => evt.ActionBegin<TRow>(), ...)` normal save/edit trigger -> Grid edit action current/previous row payload -> sync visible event field updates.

Close matrix row: `s.Component<FusionGrid>(id).SetDataSource(json)` -> Grid whole remote response body custom-binding refresh -> async HTTP success writes `{ result, count }` to `grid.dataSource` and visibly refreshes rows/count.

Close matrix row: `p.Component<FusionGrid>(id).SetDataSource(current.Where(...).AsSource()).Refresh()` -> Grid typed array data-source rebind -> sync component property read/write plus refresh method call after initial async HTTP load.

## DSL Source Requirements

- `Alis.Reactive.Fusion/Components/FusionGrid/FusionGridReactiveExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionGrid/FusionGridQueryExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionGrid/FusionGridEvents.cs`
- `Alis.Reactive.Fusion/Components/FusionGrid/Events/FusionGridOnDataStateChange.cs`
- `Alis.Reactive.Fusion/Components/FusionGrid/FusionGridDataSourceExtensions.cs`
- `Alis.Reactive/Components/Onboarding/ComponentEventOnboarding.cs`
- `Alis.Reactive/PlanAuthoring/Requests/GatherBuilder.cs`
- `Alis.Reactive/PlanAuthoring/Pipelines/PipelineBuilder.Arrays.cs`
- `Alis.Reactive/PlanAuthoring/Conditions/PayloadTypedSource.cs`
- `Alis.Reactive/PlanModel/Values/ValueExpression.cs`

Sync/async lane expectation: component event trigger is sync; the row's request reaction is async because the DSL uses `Post(...).Gather(...).Response(...)`.

## Authoritative Primitive Rows

| Grid row member | Syncfusion evidence | DSL primitive | Domain/JSON term | Runtime behavior | Status |
| --- | --- | --- | --- | --- | --- |
| `dataStateChange` event trigger | `event:dataStateChange:sorting` trace row | `TypedEvent<FusionGridDataStateChangeArgs>` selected by `.Reactive(...)` | `StartsWhen.ComponentEvent(componentId, "dataStateChange")` | runtime wires object event and starts reaction with event payload scope | accepted for this row |
| `skip` | top-level number, sample `0` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "skip", Shape.Number)` | gather reads `event.skip` into request body path `skip` | accepted for this row |
| `take` | top-level number, sample `2` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "take", Shape.Number)` | gather reads `event.take` into request body path `take` | accepted for this row |
| `sorted` | top-level array, sample `[{ name, direction }]` | event payload read with proper array shape | `ValueExpression.ReadPayload(PayloadSource.Event(), "sorted", Shape.ArrayOf(object))` from CLR `List<FusionGridSortColumn>` | gather reads whole `event.sorted` array into request body path `sorted`; no indexed paths | accepted for this row |
| `sorted[].name` | array item string, sample `name` | typed array item property when array operations are authored | `ReactiveArray<T>.From(args, x => x.SortedArray)` only if row adds an array-operation proof surface | runtime array-op normalizes source array and reads item property | not accepted yet; current C# uses `List<T>`, while `PipelineBuilder.From` currently requires `TElement[]` |
| `sorted[].direction` | array item string, sample `descending` | typed array item property when array operations are authored | same as `sorted[].name` | same as `sorted[].name` | not accepted yet; requires C# shape decision |
| `action.requestType` | nested string, sample `sorting` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "action.requestType", Shape.String)` | set/gather/condition reads nested event action member | accepted for this row |
| `action.columnName` | nested string, sample `name` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "action.columnName", Shape.String)` | set/gather reads sorting column | accepted for this row |
| `action.direction` | nested string, sample `Descending` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "action.direction", Shape.String)` | set/gather reads sorting direction | accepted for this row |
| `requiresCounts` | top-level boolean, sample `true` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "requiresCounts", Shape.Boolean)` from `FusionGridDataStateChangeArgs.RequiresCounts` | runtime reads `event.requiresCounts` | accepted and implemented |
| `name` | top-level string, sample `dataStateChange` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "name", Shape.String)` from `FusionGridDataStateChangeArgs.Name` | runtime reads `event.name` | accepted and implemented |
| `action.cancel` | nested boolean, sample `false` | event payload read and possible event payload mutation | `ValueExpression.ReadPayload(PayloadSource.Event(), "action.cancel", Shape.Boolean)` from `FusionGridAction.Cancel` | runtime reads `event.action.cancel`; mutation remains a separate row if needed | accepted for read and implemented |
| `action.name` | nested string, sample `actionBegin` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "action.name", Shape.String)` from `FusionGridAction.Name` | runtime reads `event.action.name` | accepted and implemented |
| `action.type` | nested string, sample `actionBegin` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "action.type", Shape.String)` from `FusionGridAction.Type` | runtime reads `event.action.type` | accepted and implemented |
| `action.target` | method-fired trace: `null`; header-click trace: DOM `TH` element | excluded browser-owned DOM payload object | no public C# payload property | runtime must not serialize or expose this through broad typed event args | excluded by P004 |
| sort trigger | `grid.sortColumn("name", "Descending", false)` raw trace and Syncfusion `sort.d.ts` `sortColumn(columnName, direction, isMultiSort?)`; sandbox typed call maps `RiskLevel` expression to `riskLevel` | typed component method `SortBy((ResidentDirectoryGridItem x) => x.RiskLevel, Descending)` using existing method-call primitive | `CallReaction` to Syncfusion `sortColumn(fieldName, direction, keepExistingSorts)` | runtime calls `sortColumn`, Syncfusion emits `dataStateChange`, request gathers typed `sorted`, and response refreshes visible rows | accepted for `SortBy` method row; no new primitive |
| `action.currentPage` | paging traces: nested number, sample `2` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "action.currentPage", Shape.Number)` from `FusionGridAction.CurrentPage` | gather/set text reads current page number | accepted for paging row |
| `action.previousPage` | paging traces: nested number, sample `1` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "action.previousPage", Shape.Number)` from `FusionGridAction.PreviousPage` | gather/set text reads previous page number | accepted for paging row |
| `action.pageSize` | paging traces: nested number, sample `2` raw / `10` sandbox | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "action.pageSize", Shape.Number)` from `FusionGridAction.PageSize` | gather/set text reads page size | accepted for paging row |
| `action.previousPageSize` | source candidate only for page-size changes; absent in page-number traces | deferred | no public C# payload property for this row | no runtime mapping for this row | separate page-size-change row required |
| `action.rows` | declared by `PageEventArgs`; absent in page-number traces | excluded | no public C# payload property for this row | no runtime mapping for this row | no observed typed use case |
| `where` | filtering method/filterbar traces: top-level array with complex predicate | event payload read with proper array shape | `ValueExpression.ReadPayload(PayloadSource.Event(), "where", Shape.ArrayOf(object))` from CLR `List<FusionGridTextFilterCriterion>` | gather reads whole `event.where` array into request body path `where`; no indexed paths | accepted for filtering method and filterbar rows |
| `where[].condition` | filtering method/filterbar traces: parent string `and` | typed array item property when array operations are authored | current rows use whole typed array gather | server receives property in `where` request body | accepted for filtering method and filterbar rows |
| `where[].ignoreCase` | filtering method/filterbar traces: boolean `true` | typed array item property when array operations are authored | current rows use whole typed array gather | server receives property in `where` request body | accepted for filtering method and filterbar rows |
| `where[].ignoreAccent` | filtering method/filterbar traces: boolean `false` | typed array item property when array operations are authored | current rows use whole typed array gather | server receives property in `where` request body | accepted for filtering method and filterbar rows |
| `where[].predicates` | filtering method/filterbar traces: nested predicate array | proper nested typed array source | current rows use recursive whole-array gather | server receives recursive predicate array | accepted for filtering method and filterbar rows |
| `where[].predicates[].field` | filtering traces: string `status` raw / `wing` sandbox | typed nested array item property when array operations are authored | current rows use whole typed array gather | server filters by field | accepted for filtering method and filterbar rows |
| `where[].predicates[].operator` | filtering traces: method `equal`; filterbar `startswith` | typed nested array item property when array operations are authored | current rows use whole typed array gather | server applies filter operator | accepted for filtering method and filterbar rows |
| `where[].predicates[].value` | filtering traces: method `Open`/`North`; filterbar `Op`/`N` | typed nested array item property when array operations are authored | current rows use whole typed array gather | server applies filter value and visible rows change | accepted for filtering method and filterbar rows |
| `where[].isComplex` | filtering method/filterbar traces: boolean `true` on composite node; declared by `@syncfusion/ej2-data` `Predicate` | typed predicate discriminator | current rows use whole typed array gather from CLR `FusionGridTextFilterCriterion.IsComplex` | server checks `IsComplex` before recursive flattening | accepted and proven for filtering method and filterbar rows |
| `where[].predicates[].isComplex` | filtering method/filterbar traces: boolean `false` on leaf predicate; declared by `@syncfusion/ej2-data` `Predicate` | typed predicate discriminator | current rows use recursive whole-array gather from CLR `FusionGridTextFilterCriterion.IsComplex` | server treats leaf predicate as filter criterion | accepted and proven for filtering method and filterbar rows |
| `action.currentFilterObject` | filtering method/filterbar traces: settings object | excluded | no public C# payload property | no runtime mapping for this row | duplicates `where` and carries settings/internal shape |
| `action.columns` | filtering method/filterbar traces: settings columns array | excluded | no public C# payload property | no runtime mapping for this row | duplicates `where` and carries settings/internal shape |
| `action.currentFilteringColumn` | filtering method/filterbar traces: string `status` | excluded for this row | no public C# payload property | no runtime mapping for this row | duplicates `where[].predicates[].field` |
| `action.action` | filtering method/filterbar traces: string `filter` | excluded for this row | no public C# payload property | no runtime mapping for this row | clear-filter variant must prove usefulness |
| `matchCase` / `predicate` settings-only fields | filtering method/filterbar traces: only under `action.currentFilterObject` / `action.columns[]`, not top-level `where` | excluded from `FusionGridTextFilterCriterion` for this row | no public C# payload property | no runtime mapping for this row | avoids conflating filter settings objects with server query predicates |
| filtering declared-foreign data-state keys | filtering method/filterbar traces: absent top-level keys such as `aggregates`, `dataSource`, `search`, `group`, `isLazyLoad`, `onDemandGroupInfo`, `select`, `sorted`, and `table` | variant-foreign payloads | no filtering public use in these rows | no request payload target from filtering proofs | separate behavior rows own these fields |
| `where[].predicate` | absent from top-level `where` trace | removed from public C# filter criterion for this row | no public C# payload property | no runtime mapping for this row | belongs to excluded filter settings object, not data-state `where` |
| `where[].matchCase` | absent from top-level `where` trace | removed from public C# filter criterion for this row | no public C# payload property | no runtime mapping for this row | data-state `where` uses `ignoreCase` |
| clear-filtering trigger | `grid.clearFiltering()` trace row after setup `grid.filterByColumn(...)` | typed component method `ClearFiltering()` using existing method-call primitive | `CallReaction` to Syncfusion `clearFiltering` method | runtime clears active filters and Syncfusion emits `dataStateChange` with `action.requestType=refresh` | accepted for clear-filtering-method row; no new primitive |
| clear-filtering `where` | absent top-level key in clear-filtering trace | excluded for this row | no request payload target for `where` | typed request omits `where`; runtime must not invent `[]` | excluded for clear-filtering-method row |
| clear-filtering `action.requestType` / `action.name` | nested samples `refresh`, `actionBegin` | event payload reads | existing `FusionGridAction.RequestType` and `Name` mappings | visible UI reads the clear-filter action identity | accepted for clear-filtering-method row |
| clear-filtering `requiresCounts` | top-level boolean sample `true` | event payload read | existing `FusionGridDataStateChangeArgs.RequiresCounts` mapping | Directory proof reads it into visible UI; this row does not gather it into the remote POST body | accepted for visible event read only in this row |
| clear-filtering `action.type`, `action.cancel`, `action.action`, `action.currentFilteringColumn` | absent from clear-filtering action payload | excluded for this row | no public clear-filtering mapping | typed request omits these action fields | excluded for clear-filtering-method row |
| clear-filtering `action.currentFilterObject` / `action.columns` | `null` / empty array in clear-filtering action payload | excluded settings/internal payloads | no public C# payload property | no runtime mapping for this row | no typed use case after filters are cleared |
| clear-filtering `search`, `group`, `sorted` | absent from clear-filtering trace | variant-foreign payloads | no request payload target from the clear-filtering proof | typed request omits these fields | excluded for clear-filtering-method row |
| `search` | searching trace: top-level array with search settings descriptor | event payload read with proper array shape | `ValueExpression.ReadPayload(PayloadSource.Event(), "search", Shape.ArrayOf(object))` from CLR `List<FusionGridSearchDescriptor>` | gather reads whole `event.search` array into request body path `search`; no indexed paths | accepted for searching-method row |
| `search[].fields` | searching trace: string array | typed nested array property when array operations are authored | current row uses whole typed array gather | server receives field scope in request body | accepted for searching-method row |
| `search[].key` | searching trace: string `Memory` | typed array item property when array operations are authored | current row uses whole typed array gather | server searches by key and visible rows change | accepted for searching-method row |
| `search[].operator` | searching trace: string `contains` | typed array item property when array operations are authored | current row uses whole typed array gather | server applies operator | accepted for searching-method row |
| `search[].ignoreCase` | searching trace: boolean `true` | typed array item property when array operations are authored | current row uses whole typed array gather | server receives comparison setting | accepted for searching-method row |
| `search[].ignoreAccent` | searching trace: boolean `false` | typed array item property when array operations are authored | current row uses whole typed array gather | server receives accent setting | accepted for searching-method row |
| `action.searchString` | searching trace: string `Memory` | excluded duplicate | no public C# payload property | no runtime mapping for this row | duplicates `search[].key`; no distinct typed DSL use case |
| `action.cancel` | absent from searching trace | not accepted for this row | no search-specific proof | no runtime mapping for this row | do not infer from other action variants |
| clear-search trigger | `grid.search("")` trace row after setup `grid.search("Memory")` | typed component method `ClearSearch()` using existing method-call primitive | `CallReaction` to existing Syncfusion `search` method with empty string | runtime clears active search and Syncfusion emits `dataStateChange` | accepted for clear-search-method row; no new primitive |
| clear-search `search` | absent top-level key in clear-search trace | excluded for this row | no request payload target for `search` | typed request omits `search`; runtime must not invent `[]` | excluded for clear-search-method row |
| clear-search `action.requestType` / `action.name` / `action.type` | nested samples `searching`, `actionBegin`, `actionBegin` | event payload reads | existing `FusionGridAction.RequestType`, `Name`, and `Type` mappings | visible UI reads the action identity after clear | accepted for clear-search-method row |
| clear-search `requiresCounts` | top-level boolean sample `true` | event payload read | existing `FusionGridDataStateChangeArgs.RequiresCounts` mapping | Directory proof reads it into visible UI; this row does not gather it into the remote POST body | accepted for visible event read only in this row |
| clear-search `action.searchString` | nested empty string | excluded duplicate/derived signal | no public C# payload property | no runtime mapping for this row | clear behavior is proven by `ClearSearch()` and absent top-level `search` |
| clear-search `action.cancel`, `where`, `group`, `sorted` | absent from clear-search trace | variant-foreign or absent action payloads | no request payload target from the clear-search proof | typed request omits these fields | excluded for clear-search-method row |
| clear-sorting trigger | `grid.clearSorting()` trace row after setup `grid.sortColumn("risk", "Descending", false)`; Syncfusion source removes sort columns before emitting model-change action | typed component method `ClearSorting()` using existing method-call primitive | `CallReaction` to Syncfusion `clearSorting` method | runtime clears active sorting and Syncfusion emits `dataStateChange` with `action.requestType=sorting` | accepted for clear-sorting-method row; no new primitive |
| clear-sorting `action.requestType` / `action.name` / `action.type` | nested samples `sorting`, `actionBegin`, `actionBegin` | event payload reads | existing `FusionGridAction.RequestType`, `Name`, and `Type` mappings | visible UI reads the action identity after clear | accepted for clear-sorting-method row |
| clear-sorting `requiresCounts` | top-level boolean sample `true` | event payload read | existing `FusionGridDataStateChangeArgs.RequiresCounts` mapping | Directory proof reads it into visible UI; this row does not gather it into the remote POST body | accepted for visible event read only in this row |
| clear-sorting `sorted` | absent top-level key in clear-sorting trace; setup sorting event emitted `sorted` | excluded for this row | no request payload target for `sorted` | typed request omits `sorted`; runtime must not invent `[]` | excluded for clear-sorting-method row |
| clear-sorting `action.columnName` / `action.direction` / `action.cancel` | absent from clear-sorting action payload | excluded for this row | no clear-sorting mapping for these action members | typed request omits these action fields | excluded for clear-sorting-method row |
| clear-sorting `action.target` | observed as `null` for method-trigger clear sorting | excluded browser-owned/gesture-owned payload object | no public C# payload property | runtime must not serialize or expose this through broad typed event args | excluded for clear-sorting-method row |
| clear-sorting `where`, `search`, `group`, `aggregates` | absent from clear-sorting trace | variant-foreign or no-use payloads | no request payload target from the clear-sorting proof | typed request omits these fields | excluded for clear-sorting-method row |
| `group` | grouping trace: top-level string array `["careLevel"]` | event payload read with proper array shape | `ValueExpression.ReadPayload(PayloadSource.Event(), "group", Shape.ArrayOf(string))` from CLR `List<string>` | gather reads whole `event.group` array into request body path `group`; no indexed paths | accepted for grouping-method row |
| `groups` | grouping trace: duplicate string array `["careLevel"]` | excluded duplicate alias | no public C# payload property | no runtime mapping for this row | `DataStateChangeEventArgs` declares `group`; no distinct typed use case |
| grouping `sorted` | grouping trace: top-level array `[{ name: "careLevel", direction: "ascending" }]` | event payload read with proper array shape | same `Sorted` mapping proven by sorting row | gather can send current sort state together with grouping | accepted for grouping-method row |
| `action.columnName` | grouping trace: nested string `careLevel` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "action.columnName", Shape.String)` from `FusionGridAction.ColumnName` | set/gather reads grouped column | accepted for grouping-method row |
| `action.preventFocusOnGroup` | grouping trace: nested boolean `false` | excluded internal UI flag | no public C# payload property | no runtime mapping for this row | no server data-state or visible typed DSL use case |
| grouping `action.cancel` | absent from grouping trace | not accepted for this row | no grouping-specific proof | no runtime mapping for this row | do not infer from other action variants |
| ungrouping trigger | `grid.ungroupColumn("careLevel")` trace row after setup grouping | typed component method `UngroupBy` plus `DataStateChange` payload reads | `CallReaction` to component method and `StartsWhen.ComponentEvent(componentId, "dataStateChange")` | runtime calls Syncfusion `ungroupColumn`, then the event pipeline reads the resulting payload | accepted for ungrouping-method row |
| ungrouping `action.requestType` | nested string sample `ungrouping` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "action.requestType", Shape.String)` from `FusionGridAction.RequestType` | set/gather reads ungrouping action identity | accepted for ungrouping-method row |
| ungrouping `action.columnName` | nested string sample `careLevel` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "action.columnName", Shape.String)` from `FusionGridAction.ColumnName` | set/gather reads ungrouped column | accepted for ungrouping-method row |
| ungrouping `action.name` / `action.type` | nested string samples `actionBegin` | event payload read | existing `FusionGridAction.Name` and `FusionGridAction.Type` mappings | runtime reads action lifecycle identity | accepted for ungrouping-method row |
| ungrouping `requiresCounts` | top-level boolean sample `true` | event payload read | existing `FusionGridDataStateChangeArgs.RequiresCounts` mapping | Directory proof reads it into visible UI; this row does not gather it into the remote POST body | accepted for visible event read only in this row |
| ungrouping `group` | absent top-level key in ungrouping trace; setup grouping event emitted `group` | excluded for this row | no `group` request target for ungrouping | runtime must not invent `[]` from an absent payload key | excluded for ungrouping-method row |
| ungrouping `sorted`, `where`, `search`, `aggregates`, `groups` | absent from ungrouping trace | variant-foreign or duplicate/internal payloads | no request payload target from the ungrouping proof | typed request omits these fields | excluded for ungrouping-method row |
| ungrouping `action.cancel` / `action.preventFocusOnGroup` | absent from ungrouping action payload | excluded for this row | no cancel or internal focus public mapping | typed request omits these action fields; `PreventFocusOnGroup` is not public C# | excluded for ungrouping-method row |
| clear-grouping trigger | `grid.clearGrouping()` trace row after setup `grid.groupColumn("careLevel")` and `grid.groupColumn("wing")`; Syncfusion source delegates to final `ungroupColumn` with refresh enabled | typed component method `ClearGrouping()` plus `DataStateChange` payload reads | `CallReaction` to component method and `StartsWhen.ComponentEvent(componentId, "dataStateChange")` | runtime calls Syncfusion `clearGrouping`, then the event pipeline reads the resulting final ungrouping payload | accepted for clear-grouping-method row; no new primitive |
| clear-grouping `action.requestType` | nested string sample `ungrouping` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "action.requestType", Shape.String)` from `FusionGridAction.RequestType` | set/gather reads clear-grouping action identity | accepted for clear-grouping-method row |
| clear-grouping `action.columnName` | nested string sample `wing` after two setup groups | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "action.columnName", Shape.String)` from `FusionGridAction.ColumnName` | set/gather reads the final group column cleared by `clearGrouping()` | accepted for clear-grouping-method row |
| clear-grouping `action.name` / `action.type` | nested string samples `actionBegin` | event payload read | existing `FusionGridAction.Name` and `Type` mappings | runtime reads action lifecycle identity | accepted for clear-grouping-method row |
| clear-grouping `requiresCounts` | top-level boolean sample `true` | event payload read | existing `FusionGridDataStateChangeArgs.RequiresCounts` mapping | Directory proof reads it into visible UI; this row does not gather it into the remote POST body | accepted for visible event read only in this row |
| clear-grouping `group` | absent top-level key in clear-grouping trace; setup grouping events emitted `group` | excluded for this row | no `group` request target for clear grouping | runtime must not invent `[]` from an absent payload key | excluded for clear-grouping-method row |
| clear-grouping `groups`, `sorted`, `where`, `search`, `aggregates` | absent from clear-grouping trace | variant-foreign or duplicate/internal payloads | no request payload target from the clear-grouping proof | typed request omits these fields | excluded for clear-grouping-method row |
| clear-grouping `action.cancel` / `action.preventFocusOnGroup` | absent from clear-grouping action payload | excluded for this row | no cancel or internal focus public mapping | typed request omits these action fields; `PreventFocusOnGroup` is not public C# | excluded for clear-grouping-method row |
| `recordClick` event trigger | `recordClick` cell trace row | `TypedEvent<FusionGridRecordClickArgs<TRow>>` selected by `.Reactive(...)` | `StartsWhen.ComponentEvent(componentId, "recordClick")` | runtime wires object event and starts reaction with event payload scope | accepted for record-click cell row |
| `recordClick.rowData` | object row DTO sample | event payload read through generic typed row DTO | `ValueExpression.ReadPayload(PayloadSource.Event(), "rowData.<member>", Shape.FromMember)` | set/gather/condition reads typed row data members from event row data | accepted for record-click cell row |
| `recordClick.rowData.*` | row DTO member sample | event payload nested typed member read | `ValueExpression.ReadPayload(PayloadSource.Event(), "rowData.residentName", Shape.String)` for sandbox proof | runtime reads nested row DTO member into visible text | accepted for record-click cell row |
| `recordClick.rowIndex` | number sample `0` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "rowIndex", Shape.Number)` | runtime reads clicked row coordinate | accepted for record-click cell row |
| `recordClick.cellIndex` | number sample `1` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "cellIndex", Shape.Number)` | runtime reads clicked cell coordinate | accepted for record-click cell row |
| `recordClick.name` | string sample `recordClick` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "name", Shape.String)` | runtime reads event name metadata | accepted for record-click cell row |
| `recordClick.cancel` | boolean sample `false` | excluded | no public C# payload property | no runtime mapping for this row | no cancel behavior proved |
| `recordClick.cell` | DOM `TD` | excluded browser-owned DOM payload object | no public C# payload property | no runtime mapping for this row | excluded by P004 |
| `recordClick.row` | DOM `TR` | excluded browser-owned DOM payload object | no public C# payload property | no runtime mapping for this row | excluded by P004 |
| `recordClick.target` | DOM `TD` | excluded browser-owned DOM payload object | no public C# payload property | no runtime mapping for this row | excluded by P004 |
| `recordClick.event` | MouseEvent | excluded browser-owned event object | no public C# payload property | no runtime mapping for this row | excluded by P004 |
| `recordClick.column` | EJ2 column object | excluded broad vendor object for this row | no public C# payload property | no runtime mapping for this row | no typed column-source row proved |
| `rowSelected` event trigger | `rowSelected` click trace row | `TypedEvent<FusionGridRowSelectedArgs<TRow>>` selected by `.Reactive(...)` | `StartsWhen.ComponentEvent(componentId, "rowSelected")` | runtime wires object event and starts reaction with event payload scope | accepted for row-selected click row |
| `rowSelected.data` | object row DTO sample | event payload read through generic typed row DTO | `ValueExpression.ReadPayload(PayloadSource.Event(), "data.<member>", Shape.FromMember)` | set/gather/condition reads typed row data members from event data | accepted for row-selected click row |
| `rowSelected.data.*` | row DTO member sample | event payload nested typed member read | `ValueExpression.ReadPayload(PayloadSource.Event(), "data.residentName", Shape.String)` and `data.residentId` for sandbox proof | runtime reads nested row DTO members into visible text and request body | accepted for row-selected click row |
| `rowSelected.rowIndex` | number samples `0`, `1` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "rowIndex", Shape.Number)` | runtime reads selected row coordinate | accepted for row-selected click row |
| `rowSelected.previousRowIndex` | first click undefined; second click number `0` | nullable event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "previousRowIndex", Shape.NullableNumber)` | runtime reads previous selected row coordinate when present | accepted as nullable for row-selected click row |
| `rowSelected.isInteracted` | boolean sample `true` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "isInteracted", Shape.Boolean)` | runtime reads click interaction flag | accepted for row-selected click row |
| `rowSelected.name` | string sample `rowSelected` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "name", Shape.String)` | runtime reads event name metadata | accepted for row-selected click row |
| `rowSelected.row` | DOM `TR` | excluded browser-owned DOM payload object | no public C# payload property | no runtime mapping for this row | excluded by P004 |
| `rowSelected.previousRow` | DOM `TR` or undefined | excluded browser-owned DOM payload object | no public C# payload property | no runtime mapping for this row | excluded by P004 |
| `rowSelected.target` | DOM `TD` | excluded browser-owned DOM payload object | no public C# payload property | no runtime mapping for this row | excluded by P004 |
| `rowSelected.foreignKeyData` | empty object sample | excluded for this row | no public C# payload property | no runtime mapping for this row | foreign-key row required |
| `rowSelected.isHeaderCheckBoxClicked` | boolean sample `false` | excluded for this row | no public C# payload property | no runtime mapping for this row | checkbox-selection row required |
| `rowSelected.rowIndexes` | number duplicate in single-selection row | excluded duplicate | no public C# payload property | no runtime mapping for this row | multiple/range selection row required before array semantics |
| `toolbarClick` event trigger | `toolbarClick` custom trace row | `TypedEvent<FusionGridToolbarClickArgs>` selected by `.Reactive(...)` | `StartsWhen.ComponentEvent(componentId, "toolbarClick")` | runtime wires object event and starts reaction with event payload scope | accepted for toolbar-click custom row |
| `toolbarClick.item.id` | string sample `emailStatements` | nested event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "item.id", Shape.String)` | condition/set text reads selected toolbar command id | accepted for toolbar-click custom row |
| `toolbarClick.item.text` | string sample `Email Statements` | nested event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "item.text", Shape.String)` | set text reads selected toolbar command text | accepted for toolbar-click custom row |
| `toolbarClick.cancel` | boolean sample `false` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "cancel", Shape.Boolean)` | runtime reads current cancel flag | accepted for read only in toolbar-click custom row |
| `toolbarClick.name` | string sample `toolbarClick` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "name", Shape.String)` | runtime reads event name metadata | accepted for toolbar-click custom row |
| `toolbarClick.originalEvent` | PointerEvent | excluded browser-owned event object | no public C# payload property | no runtime mapping for this row | excluded by P004 |
| `toolbarClick.item.tooltipText` | string sample `Email statements` | excluded item config metadata | no public C# payload property | no runtime mapping for this row | no focused behavior use case |
| `toolbarClick.item.prefixIcon` | string sample `e-icons e-send-1` | excluded presentation metadata | no public C# payload property | no runtime mapping for this row | no focused behavior use case |
| `toolbarClick.item.suffixIcon` | empty string sample | excluded presentation metadata | no public C# payload property | no runtime mapping for this row | no focused behavior use case |
| `toolbarClick.item.disabled` | boolean sample `false` | excluded for this row | no public C# payload property | no runtime mapping for this row | disabled-item row required |
| `toolbarClick.item.visible` | boolean sample `true` | excluded for this row | no public C# payload property | no runtime mapping for this row | visibility row required |
| `toolbarClick.item.type` | string sample `Button` | excluded toolbar rendering metadata | no public C# payload property | no runtime mapping for this row | no focused behavior use case |
| `toolbarClick.item.align` | string sample `Left` | excluded toolbar layout metadata | no public C# payload property | no runtime mapping for this row | no focused behavior use case |
| `beginEdit` normal trigger | `beginEdit` normal trace row | `TypedEvent<FusionGridBeginEditArgs<TRow>>` selected by `.Reactive(...)` | `StartsWhen.ComponentEvent(componentId, "beginEdit")` | runtime wires object event and starts reaction with event payload scope | focused typed DSL proof passed |
| `beginEdit.rowData` | object row DTO | event payload read through generic typed row DTO | `ValueExpression.ReadPayload(PayloadSource.Event(), "rowData.<member>", Shape.FromMember)` | runtime reads row values before edit mode starts | accepted and row-proven for normal edit row |
| `beginEdit.rowIndex` | number sample `0` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "rowIndex", Shape.Number)` | runtime reads edited row coordinate | accepted and row-proven for normal edit row |
| `beginEdit.type` | string sample `edit` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "type", Shape.String)` | runtime reads edit-mode metadata | accepted and row-proven for normal edit row |
| `beginEdit.cancel` | boolean sample `false`, writable to `true` | event payload read and mutation | read: `ValueExpression.ReadPayload(PayloadSource.Event(), "cancel", Shape.Boolean)`; mutation: `ReactionGraph.Set(PayloadSource.Event(), "cancel", true)` | runtime reads cancel flag or sets it to prevent edit mode | accepted and row-proven for normal edit row |
| `beginEdit.row` | DOM `TR` | excluded browser-owned DOM payload object | no public C# payload property | no runtime mapping for this row | excluded by P004 |
| `beginEdit.foreignKeyData` | empty object sample | excluded for this row | no public C# payload property | no runtime mapping for this row | foreign-key row required |
| `beginEdit.isScroll` | boolean sample `false` | excluded internal metadata | no public C# payload property | no runtime mapping for this row | no typed use case |
| `beginEdit.name` | string sample `beginEdit` | excluded duplicate event metadata | no public C# payload property | no runtime mapping for this row | event selector already owns event identity |
| `beginEdit.primaryKey` | string array sample `["id"]` | excluded duplicate key metadata | no public C# payload property | no runtime mapping for this row | use accepted typed `rowData` |
| `beginEdit.primaryKeyValue` | array sample `[1]` | excluded duplicate key metadata | no public C# payload property | no runtime mapping for this row | use accepted typed `rowData` |
| `beginEdit.requestType` | string sample `beginEdit` | excluded duplicate event metadata | no public C# payload property | no runtime mapping for this row | event selector already owns lifecycle identity |
| `beginEdit.target` | undefined | excluded | no public C# payload property | no runtime mapping for this row | no typed use case |
| `cellSave` batch-edit trigger | `cellSave` batch-edit trace row | `TypedEvent<FusionGridCellSaveArgs<TRow, TValue>>` selected by `.Reactive(...)` | `StartsWhen.ComponentEvent(componentId, "cellSave")` | runtime wires object event and starts reaction with event payload scope | focused typed DSL proof passed |
| `cellSave.rowData` | object row DTO | event payload read through generic typed row DTO | `ValueExpression.ReadPayload(PayloadSource.Event(), "rowData.<member>", Shape.FromMember)` | runtime reads row values for edit policy and visible audit output | accepted and row-proven for batch-edit row |
| `cellSave.columnName` | string sample `openTasks` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "columnName", Shape.String)` | runtime reads edited field identity | accepted and row-proven for batch-edit row |
| `cellSave.value` | number samples `6`, `4`, `99` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "value", Shape.FromClrType(typeof(TValue)))` | runtime reads typed edited value for visible output and conditions | accepted and row-proven for batch-edit row |
| `cellSave.previousValue` | number samples `2`, `0`, `6` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "previousValue", Shape.FromClrType(typeof(TValue)))` | runtime reads typed previous value | accepted and row-proven for batch-edit row |
| `cellSave.cancel` | boolean sample `false`, writable to `true` | event payload read and mutation | read: `ValueExpression.ReadPayload(PayloadSource.Event(), "cancel", Shape.Boolean)`; mutation: `ReactionGraph.Set(PayloadSource.Event(), "cancel", true)` | runtime reads cancel flag or sets it to prevent the edited value from being accepted | accepted and row-proven for batch-edit row |
| `cellSave.cell` | DOM `TD` | excluded browser-owned DOM payload object | no public C# payload property | no runtime mapping for this row | excluded by P004 |
| `cellSave.column` | Syncfusion column object | excluded vendor configuration object | no public C# payload property | no runtime mapping for this row | no focused typed Senior Living behavior |
| `cellSave.columnObject` | Syncfusion column object | excluded duplicate vendor configuration object | no public C# payload property | no runtime mapping for this row | no focused typed Senior Living behavior |
| `cellSave.isForeignKey` | boolean sample `false` | excluded for this row | no public C# payload property | no runtime mapping for this row | foreign-key column row required |
| `cellSave.name` | string sample `cellSave` | excluded duplicate event metadata | no public C# payload property | no runtime mapping for this row | event selector already owns event identity |
| `cellSaved` event trigger | `event:cellSaved:batch-edit` trace row | `TypedEvent<FusionGridCellSavedArgs<TRow, TValue>>` selected by `.Reactive(...)` | `StartsWhen.ComponentEvent(componentId, "cellSaved")` | runtime wires object event and starts reaction with event payload scope | accepted for batch-edit row |
| `cellSaved.rowData` | object row DTO | event payload read through generic typed row DTO | `ValueExpression.ReadPayload(PayloadSource.Event(), "rowData.<member>", Shape.FromMember)` | runtime reads row values for post-save audit output | accepted and row-proven for batch-edit row |
| `cellSaved.columnName` | string sample `openTasks` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "columnName", Shape.String)` | runtime reads edited field identity after save | accepted and row-proven for batch-edit row |
| `cellSaved.value` | number samples `6`, `8` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "value", Shape.FromClrType(typeof(TValue)))` | runtime reads typed saved value for visible output | accepted and row-proven for batch-edit row |
| `cellSaved.previousValue` | number samples `2`, `6` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "previousValue", Shape.FromClrType(typeof(TValue)))` | runtime reads typed previous value for audit output | accepted and row-proven for batch-edit row |
| `cellSaved.cancel` | boolean sample `false`, writable to `true` but saved value remains accepted | excluded post-save lifecycle flag | no public C# payload property | no runtime mapping for this row | raw trace proves mutation is not behaviorally useful after save |
| `cellSaved.cell` | DOM `TD` | excluded browser-owned DOM payload object | no public C# payload property | no runtime mapping for this row | excluded by P004 |
| `cellSaved.column` | Syncfusion column object | excluded vendor configuration object | no public C# payload property | no runtime mapping for this row | no focused typed Senior Living behavior |
| `cellSaved.columnObject` | Syncfusion column object | excluded duplicate vendor configuration object | no public C# payload property | no runtime mapping for this row | no focused typed Senior Living behavior |
| `cellSaved.isForeignKey` | boolean sample `false` | excluded for this row | no public C# payload property | no runtime mapping for this row | foreign-key column row required |
| `cellSaved.name` | string sample `cellSaved` | excluded duplicate event metadata | no public C# payload property | no runtime mapping for this row | event selector already owns event identity |
| `beforeBatchSave` event trigger | `event:beforeBatchSave:batch-edit` trace row | `TypedEvent<FusionGridBeforeBatchSaveArgs<TRow>>` selected by `.Reactive(...)` | `StartsWhen.ComponentEvent(componentId, "beforeBatchSave")` | runtime wires object event and starts reaction with event payload scope | accepted for batch-edit commit row |
| `beforeBatchSave.batchChanges` | object with `addedRecords`, `changedRecords`, and `deletedRecords` arrays | event payload read through generic typed batch-change DTO | `ValueExpression.ReadPayload(PayloadSource.Event(), "batchChanges.<member>", Shape.FromMember)` | runtime reads unsaved batch lists before commit | accepted and row-proven for batch-edit row |
| `beforeBatchSave.cancel` | boolean sample `false`, writable to `true` | event payload read and mutation | read: `ValueExpression.ReadPayload(PayloadSource.Event(), "cancel", Shape.Boolean)`; mutation: `ReactionGraph.Set(PayloadSource.Event(), "cancel", true)` | runtime reads cancel flag or sets it to prevent the bulk-save lifecycle | accepted and row-proven for batch-edit row |
| `beforeBatchSave.name` | string sample `beforeBatchSave` | excluded duplicate event metadata | no public C# payload property | no runtime mapping for this row | event selector already owns event identity |
| `actionBegin` save/edit trigger | `actionBegin` save/edit trace row | `TypedEvent<FusionGridEditActionArgs<TRow>>` selected by `.Reactive(...)` | `StartsWhen.ComponentEvent(componentId, "actionBegin")` | runtime wires object event and starts reaction with event payload scope | accepted for save/edit variant row |
| `actionBegin.name` | string sample `actionBegin` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "name", Shape.String)` | runtime reads event name metadata | accepted for save/edit variant row |
| `actionBegin.requestType` | string sample `save` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "requestType", Shape.String)` | runtime reads edit action lifecycle point | accepted for save/edit variant row |
| `actionBegin.action` | string sample `edit` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "action", Shape.String)` | runtime distinguishes save-edit from save-add | accepted for save/edit variant row |
| `actionBegin.type` | string sample `actionBegin` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "type", Shape.String)` | runtime reads Syncfusion action event type metadata | accepted for save/edit variant row |
| `actionBegin.cancel` | boolean sample `false` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "cancel", Shape.Boolean)` | runtime reads current cancel flag | accepted for read only in save/edit variant row |
| `actionBegin.data` | object edited row DTO | event payload read through generic typed row DTO | `ValueExpression.ReadPayload(PayloadSource.Event(), "data.<member>", Shape.FromMember)` | runtime reads edited row values | accepted for save/edit variant row |
| `actionBegin.previousData` | object original row DTO | event payload read through generic typed row DTO | `ValueExpression.ReadPayload(PayloadSource.Event(), "previousData.<member>", Shape.FromMember)` | runtime reads original row values | accepted for save/edit variant row |
| `actionBegin.rowIndex` | number sample `0` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "rowIndex", Shape.Number)` | runtime reads edited row coordinate | accepted for save/edit variant row |
| `actionBegin.selectedRow` | number sample `-1` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "selectedRow", Shape.Number)` | runtime can read existing Syncfusion scalar; no stronger semantics claimed | accepted as existing field for save/edit variant row |
| `actionBegin.index` | not emitted as an own key in save/edit trace; probe read is undefined | not accepted for this variant | focused proof asserts no public `Index` property | no runtime mapping for this row | add-row variant required |
| `actionBegin.row` | DOM `TR` | excluded browser-owned DOM payload object | focused proof asserts no public `Row` property | no runtime mapping for this row | excluded by P004 |
| `actionBegin.form` | DOM `FORM` | excluded browser-owned DOM payload object | focused proof asserts no public `Form` property | no runtime mapping for this row | excluded by P004 |
| `actionBegin.target` | undefined | excluded | focused proof asserts no public `Target` property | no runtime mapping for this row | no typed use case |
| `actionBegin.foreignKeyData` | empty object sample | excluded for this row | focused proof asserts no public `ForeignKeyData` property | no runtime mapping for this row | foreign-key row required |
| `actionBegin.isScroll` | boolean sample `false` | excluded internal metadata | focused proof asserts no public `IsScroll` property | no runtime mapping for this row | no typed use case |
| `actionBegin.primaryKey` | string array sample `["id"]` | excluded for this row | focused proof asserts no public `PrimaryKey` property | no runtime mapping for this row | no clear C# behavior use case |
| `actionBegin.primaryKeyValue` | array sample `[1]` | excluded for this row | focused proof asserts no public `PrimaryKeyValue` property | no runtime mapping for this row | no clear C# behavior use case |
| `actionBegin.rowData` | original row DTO duplicate | excluded duplicate | focused proof asserts no public `RowData` property | no runtime mapping for this row | use accepted `previousData` |
| `actionComplete` save/edit trigger | `actionComplete` save/edit trace row | `TypedEvent<FusionGridEditActionArgs<TRow>>` selected by `.Reactive(...)` | `StartsWhen.ComponentEvent(componentId, "actionComplete")` | runtime wires object event and starts reaction with event payload scope | accepted for save/edit variant row |
| `actionComplete.name` | string sample `actionComplete` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "name", Shape.String)` | runtime reads event name metadata | accepted for save/edit variant row |
| `actionComplete.requestType` | string sample `save` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "requestType", Shape.String)` | runtime reads edit action lifecycle point | accepted for save/edit variant row |
| `actionComplete.action` | string sample `edit` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "action", Shape.String)` | runtime distinguishes save-edit from save-add | accepted for save/edit variant row |
| `actionComplete.type` | string sample `actionComplete` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "type", Shape.String)` | runtime reads Syncfusion action event type metadata | accepted for save/edit variant row |
| `actionComplete.cancel` | boolean sample `false` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "cancel", Shape.Boolean)` | runtime reads current cancel flag | accepted for read only in save/edit variant row |
| `actionComplete.data` | object edited row DTO | event payload read through generic typed row DTO | `ValueExpression.ReadPayload(PayloadSource.Event(), "data.<member>", Shape.FromMember)` | runtime reads edited row values | accepted for save/edit variant row |
| `actionComplete.previousData` | object original row DTO | event payload read through generic typed row DTO | `ValueExpression.ReadPayload(PayloadSource.Event(), "previousData.<member>", Shape.FromMember)` | runtime reads original row values | accepted for save/edit variant row |
| `actionComplete.rowIndex` | number sample `0` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "rowIndex", Shape.Number)` | runtime reads edited row coordinate | accepted for save/edit variant row |
| `actionComplete.selectedRow` | number sample `-1` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "selectedRow", Shape.Number)` | runtime can read existing Syncfusion scalar; no stronger semantics claimed | accepted as existing field for save/edit variant row |
| `actionComplete.index` | not emitted as an own key in save/edit trace; probe read is undefined | not accepted for this variant | focused proof asserts no public `Index` property | no runtime mapping for this row | do not reintroduce public `Index` |
| `actionComplete.promise` | emitted as own key with undefined value | not accepted for this variant | focused proof asserts no public `Promise` property | no runtime mapping for this row | no predictable typed C# behavior |
| `actionComplete.row` | DOM `TR` | excluded browser-owned DOM payload object | focused proof asserts no public `Row` property | no runtime mapping for this row | excluded by P004 |
| `actionComplete.form` | DOM `FORM` | excluded browser-owned DOM payload object | focused proof asserts no public `Form` property | no runtime mapping for this row | excluded by P004 |
| `actionComplete.target` | undefined | excluded | focused proof asserts no public `Target` property | no runtime mapping for this row | no typed use case |
| `actionComplete.foreignKeyData` | empty object sample | excluded for this row | focused proof asserts no public `ForeignKeyData` property | no runtime mapping for this row | foreign-key row required |
| `actionComplete.isScroll` | boolean sample `false` | excluded internal metadata | focused proof asserts no public `IsScroll` property | no runtime mapping for this row | no typed use case |
| `actionComplete.primaryKey` | string array sample `["id"]` | excluded for this row | focused proof asserts no public `PrimaryKey` property | no runtime mapping for this row | no clear C# behavior use case |
| `actionComplete.primaryKeyValue` | array sample `[1]` | excluded for this row | focused proof asserts no public `PrimaryKeyValue` property | no runtime mapping for this row | no clear C# behavior use case |
| `actionComplete.rowData` | original row DTO duplicate | excluded duplicate | focused proof asserts no public `RowData` property | no runtime mapping for this row | use accepted `previousData` |
| `{ result, count }` whole response body | raw remote-response trace assigns whole response object and typed Grid proof parses `result.length == 10`, `count == 200` | HTTP success whole-payload read | `ValueExpression.ReadWholePayload(source.Scope)` from `SetDataSource(ResponseBody<TResponse>)`; JSON member is `responseBody` with empty path | success route assigns whole response object to `grid.dataSource` | accepted for whole-response remote row |
| `result` | raw trace observes response key and visible first row after assignment | Syncfusion-owned custom-binding row collection inside whole body | no separate public path for this row | Grid consumes `result` when `dataSource` is assigned the whole response object | accepted as part of whole response body |
| `count` | raw trace observes response key and pager text; typed proof asserts `count == 200` and pager shows `200 items` | Syncfusion-owned custom-binding total inside whole body | no separate public path for this row | Grid consumes `count` for pager total | accepted as part of whole response body |
| `dataSource` read | raw data-source trace reads `grid.dataSource` before and after replacement | component property read | `ValueExpression.Read(ComponentObject, "dataSource")` from `Data<TRow>()` | runtime reads current component `dataSource` into a typed array source | accepted for data-source typed-array row |
| `dataSource` write from typed array source | raw data-source trace assigns a replacement array and typed ArrayGrid row writes `SetDataSource(current.Where(...).AsSource())` | component property set from `TypedSource<T[]>` | `SetReaction` targeting component property `dataSource` | runtime assigns the typed array value to `grid.dataSource` | accepted for data-source typed-array row |
| `refresh()` | raw data-source trace calls `grid.refresh()` and visible rows update; return is `undefined` | component method call | `CallReaction` targeting component method `refresh` | runtime invokes `grid.refresh()` after the data-source set | accepted for data-source typed-array row |
| response-path `SetDataSource` overload | not exercised by the whole-response or typed-array rows | deferred | no closure from this row | no runtime mapping closed by this row | separate response-path row required |
| event-payload `SetDataSource` overload | not exercised by the whole-response or typed-array rows | deferred | no closure from this row | no runtime mapping closed by this row | separate event-payload row required |
| DataManager/adaptor `dataSource` | not exercised by the whole-response or typed-array rows | deferred | no closure from this row | no runtime mapping closed by this row | separate remote/adaptor row required |

## Primitive Decision

No new primitive is needed for the mapped Grid rows. Current primitives already cover:

- component event trigger;
- event payload read;
- nested event payload read;
- event payload array gathered as a whole typed source;
- typed array operations when the payload property is shaped as an array accepted by `PipelineBuilder.From`.
- component property read;
- component property write from a typed source;
- component method call.

No mapped row currently requires a new primitive. Any future failure to read one
of these accepted members is a discovery/mapping/typed-contract problem first,
not permission to add a primitive.

## Code To Delete Or Simplify

None identified yet for the primitive layer. If implementation starts preserving old helper paths only because tests reference them, this row must stop and re-check the DSL graph.

## Behavior Proof Required Before Commit

Use `scripts/playwright.sh --filter "..."` against a typed Fusion Grid DSL page that:

1. sorts the grid through a user-visible Grid interaction or typed `SortBy` command that visibly changes rows;
2. gathers `skip`, `take`, `sorted`, and accepted action fields through `FromEvent`;
3. proves request payload/body values;
4. proves the response updates the Grid data source visibly;
5. proves any newly accepted payload fields such as `requiresCounts`;
6. for grouping, proves `Group` is gathered as a typed whole array and visible Grid caption rows render from the server response;
7. for row selection, proves first and second real row clicks, typed row-data reads, nullable previous-row index behavior, interaction flag, event name, and async gather from `args.Data.ResidentId`;
8. for toolbar click, proves real custom toolbar click, branch by `Item.Id`, visible reads for `Item.Id`, `Item.Text`, `Cancel`, and `Name`;
9. for action-begin save/edit, proves real row edit/save, visible reads for `RequestType`, `Action`, `Type`, `Name`, `Cancel`, `RowIndex`, `Data`, and `PreviousData`, plus public-contract absence for all listed exclusions.
10. for remote whole-response custom binding, parses the actual HTTP success
    body, proves it has `{ result, count }`, proves `SetDataSource(json)` writes
    the whole response body to `grid.dataSource`, proves returned rows render,
    and proves `count` drives visible pager total;
11. for data-source typed array, proves initial HTTP array load through a typed
    array transform into `SetDataSource`, then proves `Data()` reads the current
    Grid `dataSource`, a client-side array transform filters it, `SetDataSource`
    writes it back, and `Refresh()` applies the visible row change.

Exact commit boundary under the user's current instruction: do not commit until
the final goal is reached. If commit discipline resumes later, keep each row as
an auditable closed-row patch.
