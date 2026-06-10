# FusionGrid Master Use Cases

Status: partial row proof, fail-closed. Some runtime traces, primitive mapping,
vertical slice decisions, and focused typed DSL proofs are complete, but audit
closeout is still pending for the unproven public API rows.

This file is the entry point for deterministic Fusion onboarding or audit of
`FusionGrid`. Existing C#, sandbox, tests, docs, and memory are evidence
only after raw EJ2 discovery and primitive mapping prove them.

Syncfusion target: `ej.grids.Grid`

No API member is accepted until the row is proven end to end:

```text
raw EJ2 probe -> trace JSON -> candidate classification -> primitive map ->
C# name decision -> vertical slice plan -> implementation -> typed proof matrix ->
Playwright proof -> audit report
```

## Current Counts

| Item | Count |
|---|---:|
| Static JS members | 484 |
| Static event members | 85 |
| Event payload entries | 85 |
| Typed C# API rows | 165 |
| Supplemental audit rows | 294 |
| Total typed coverage matrix rows | 459 |

## Use Case Rows

Rows below are source-index rows for this onboarding slice. Only one event,
property, method, or event-trigger variant may move through the full chain at a
time. Grid audit order is events first, then properties, then methods. Event
payload shape is part of the event row and must prove exact keys, missing keys,
nested objects, arrays through proper array primitives, writable fields,
payload methods, and typed C# event arg names.

Remote/custom-binding behavior is a primary Grid audit lane. The proven
`dataStateChange` rows cover realistic custom-binding refresh for those event
variants, including method-trigger grouping and ungrouping as distinct rows
because Syncfusion emits different payload shapes. The whole-response
`{ result, count }` `SetDataSource(json)` row and
the typed-array `SetDataSource`, `Data`, and `Refresh` row are also proven.
Response-path overloads, event-payload path overloads, DataManager/adaptor,
builder-owned initial dataSource, and nested data-source behavior remain open
until each has its own row-level raw trace, primitive mapping, vertical slice,
and typed DSL Playwright behavior proof.

The typed API coverage matrix now includes generated supplemental rows for
remote-data lanes, `dataStateChange` variant payload acceptance/exclusion, the
`beginEdit` normal-edit variant, the `beforeBatchSave` batch-edit variant, the
`cellSave` batch-edit variant, and the `cellSaved` batch-edit variant. The
variant rows are derived from the
judgment-call artifacts, then marked proven only when the current typed DSL
Playwright proof covers that member. Accepted or excluded judgment rows without
behavior proof remain unproven and are counted by the fail-closed gate so
remote behavior and variant-sensitive payload gaps cannot exist only in prose.

| Use Case | API Members | Event Payloads | Builder-Owned? | Primitive | C# Target | Proof Status |
|---|---|---|---|---|---|---|
| component inventory | current Fusion source, sandbox, and tests inventoried | n/a | n/a | n/a | n/a | local artifact linked; uncommitted by instruction |
| shipped EJ2 static discovery | [public-api-surface.json](discovery/public-api-surface.json) | [event-payload-surface.json](discovery/event-payload-surface.json) | [mvc-builder-coverage.md](discovery/mvc-builder-coverage.md) | pending mapping | pending naming | static-discovery only |
| raw EJ2 core probe | [raw-ej2-core.html](probes/raw-ej2-core.html) | [raw-ej2-core.trace.json](traces/raw-ej2-core.trace.json) | browser trace confirms core lifecycle payload capture | pending mapping | pending naming | local trace artifact linked; DSL proof pending |
| event row: `dataStateChange` sorting | [method probe](probes/raw-ej2-data-state-change-sorting.html) + [header-click probe](probes/raw-ej2-data-state-change-sorting-header-click.html) | [row artifact](discovery/event-row-data-state-change-sorting.md) + [method trace](traces/raw-ej2-data-state-change-sorting.trace.json) + [header-click trace](traces/raw-ej2-data-state-change-sorting-header-click.trace.json) | event is builder-connectable only | array primitive for `sorted[]`; object property primitives for `action.*`; DOM `action.target` excluded; sorting-foreign `where`, `search`, and `group` omitted | accepted C# fields implemented; `action.target` excluded | focused typed DSL proof passed for accepted fields and sorting variant exclusions; component audit still incomplete |
| method row: `FusionGrid.SortBy(...)` | [method probe](probes/raw-ej2-data-state-change-sorting.html) | [method trace](traces/raw-ej2-data-state-change-sorting.trace.json) + Syncfusion `sortColumn(columnName, direction, isMultiSort?)` source declaration | runtime method call, followed by builder-connected `dataStateChange` | method-call primitive to `sortColumn`; existing event payload primitives gather `sorted` and action metadata; no new primitive | existing typed C# `SortBy` maps a row expression to field name and preserves `FusionGridSortDirection` plus `keepExistingSorts` | focused typed DSL proof passed for public method row; header-click/multi-sort/custom-comparer rows remain open |
| event row: `dataStateChange` paging | [method probe](probes/raw-ej2-data-state-change-paging-method.html) + [pager-click probe](probes/raw-ej2-data-state-change-paging-pager-click.html) | [row artifact](discovery/event-row-data-state-change-paging.md) + [method trace](traces/raw-ej2-data-state-change-paging-method.trace.json) + [pager-click trace](traces/raw-ej2-data-state-change-paging-pager-click.trace.json) | event is builder-connectable only | object property primitives for `action.*`; paging-foreign `where`, `search`, `group`, and `sorted` omitted; source candidates `previousPageSize`, `rows`, `target` excluded/deferred | accepted C# paging fields already present; `PreviousPageSize`, `Rows`, and `Target` excluded from public action contract | focused typed DSL proof passed for accepted fields and paging variant exclusions; component audit still incomplete |
| event row: `dataStateChange` filtering method | [method probe](probes/raw-ej2-data-state-change-filtering-method.html) | [row artifact](discovery/event-row-data-state-change-filtering-method.md) + [method trace](traces/raw-ej2-data-state-change-filtering-method.trace.json) | event is builder-connectable only | whole typed array primitive for `where`; `IsComplex` accepted as predicate discriminator; settings/internal action objects excluded | accepted C# `Where` shape narrowed; stale `Predicate`/`MatchCase` removed | focused typed DSL proof passed; component audit still open |
| event row: `dataStateChange` filtering FilterBar typing | [filterbar probe](probes/raw-ej2-data-state-change-filtering-filterbar.html) | [row artifact](discovery/event-row-data-state-change-filtering-filterbar.md) + [filterbar trace](traces/raw-ej2-data-state-change-filtering-filterbar.trace.json) | event is builder-connectable only | whole typed array primitive for `where`; `IsComplex` accepted as predicate discriminator; settings/internal action objects excluded; Enter commit gesture recorded | existing C# `Where` shape reused; no new public settings fields | focused typed DSL proof passed; component audit still open |
| event row: `dataStateChange` clear-filtering method | [method probe](probes/raw-ej2-data-state-change-clear-filtering-method.html) | [row artifact](discovery/event-row-data-state-change-clear-filtering-method.md) + [method trace](traces/raw-ej2-data-state-change-clear-filtering-method.trace.json) | event is builder-connectable only | method call primitive plus `refresh` action metadata reads; top-level `where` is absent rather than an empty array; settings/internal action objects excluded | existing `ClearFiltering()` maps to Syncfusion `clearFiltering`; refresh guard/manual reload masking removed from Directory slice | focused typed DSL proof passed for accepted fields and clear-filtering variant exclusions; component audit still incomplete |
| event row: `dataStateChange` searching method | [method probe](probes/raw-ej2-data-state-change-searching-method.html) | [row artifact](discovery/event-row-data-state-change-searching-method.md) + [method trace](traces/raw-ej2-data-state-change-searching-method.trace.json) | event is builder-connectable only | whole typed array primitive for `search`; duplicate `action.searchString` and searching-foreign `where`, `group`, `sorted` omitted | current C# `Search` shape accepted; no `Action.SearchString`; search-row request omits `actionCancel` | focused typed DSL proof passed for accepted fields and searching variant exclusions; component audit still incomplete |
| event row: `dataStateChange` clear-search method | [method probe](probes/raw-ej2-data-state-change-clear-search-method.html) | [row artifact](discovery/event-row-data-state-change-clear-search-method.md) + [method trace](traces/raw-ej2-data-state-change-clear-search-method.trace.json) | event is builder-connectable only | method call primitive plus common action metadata reads; top-level `search` is absent rather than an empty array; searching/filtering/grouping/sorting fields omitted | existing `ClearSearch()` maps through `Search(string.Empty)`; no `Action.SearchString`; no new primitive | focused typed DSL proof passed for accepted fields and clear-search variant exclusions; component audit still incomplete |
| event row: `dataStateChange` grouping method | [method probe](probes/raw-ej2-data-state-change-grouping-method.html) | [row artifact](discovery/event-row-data-state-change-grouping-method.md) + [method trace](traces/raw-ej2-data-state-change-grouping-method.trace.json) | event is builder-connectable only | whole typed array primitive for `group`; duplicate `groups`, internal `actionPreventFocusOnGroup`, and grouping-foreign fields omitted | current C# `Group` shape accepted; no `Groups` or `PreventFocusOnGroup`; group-row request omits `actionCancel` | focused typed DSL proof passed for accepted fields and grouping variant exclusions; component audit still incomplete |
| event row: `dataStateChange` ungrouping method | [method probe](probes/raw-ej2-data-state-change-ungrouping-method.html) | [row artifact](discovery/event-row-data-state-change-ungrouping-method.md) + [method trace](traces/raw-ej2-data-state-change-ungrouping-method.trace.json) | event is builder-connectable only | action identity and ungrouped column reads; top-level `group` is absent rather than an empty array; grouping/searching/filtering/aggregate fields omitted | existing `UngroupBy` and `Action.ColumnName` shape accepted; no `Group` claim for this row; no `Groups` or `PreventFocusOnGroup` | focused typed DSL proof passed for accepted fields and ungrouping variant exclusions; component audit still incomplete |
| event row: `dataStateChange` clear-grouping method | [method probe](probes/raw-ej2-data-state-change-clear-grouping-method.html) | [row artifact](discovery/event-row-data-state-change-clear-grouping-method.md) + [method trace](traces/raw-ej2-data-state-change-clear-grouping-method.trace.json) | event is builder-connectable only | method call primitive plus final ungrouping action metadata reads; top-level `group`, `groups`, and `sorted` are absent rather than empty arrays | existing `ClearGrouping()` maps to Syncfusion `clearGrouping`; final `Action.ColumnName` accepted as `wing` after two setup groups; no `Groups`, `Aggregates`, or `PreventFocusOnGroup` | focused typed DSL proof passed for accepted fields and clear-grouping variant exclusions; component audit still incomplete |
| event row: `recordClick` cell | [cell probe](probes/raw-ej2-record-click-cell.html) | [row artifact](discovery/event-row-record-click-cell.md) + [cell trace](traces/raw-ej2-record-click-cell.trace.json) | event is builder-connectable only | typed row DTO source plus scalar event payload reads; DOM/vendor objects excluded | current C# `RecordClick<TRow>` shape accepted; no DOM/event/column/cancel fields | focused typed DSL proof passed; component audit still open |
| event row: `rowSelected` click | [click probe](probes/raw-ej2-row-selected-click.html) | [row artifact](discovery/event-row-row-selected-click.md) + [click trace](traces/raw-ej2-row-selected-click.trace.json) | event is builder-connectable only | typed row DTO source plus nullable/scalar event payload reads; DOM/checkbox/foreign-key objects excluded | C# `RowSelected<TRow>` shape corrected; `PreviousRowIndex` nullable; no DOM/checkbox fields | focused typed DSL proof passed; component audit still open |
| event row: `toolbarClick` custom item | [custom probe](probes/raw-ej2-toolbar-click-custom.html) | [row artifact](discovery/event-row-toolbar-click-custom.md) + [custom trace](traces/raw-ej2-toolbar-click-custom.trace.json) | event is builder-connectable only | nested toolbar item identity plus scalar metadata reads; browser event/config metadata excluded | C# `ToolbarClick` shape corrected with `Name`; no `OriginalEvent` or extra item config fields | focused typed DSL proof passed; component audit still open |
| event variant row: `actionBegin` save/edit | [save-edit probe](probes/raw-ej2-action-begin-save-edit.html) | [row artifact](discovery/event-row-action-begin-save-edit.md) + [save-edit trace](traces/raw-ej2-action-begin-save-edit.trace.json) | event is builder-connectable only | typed current/previous row DTOs plus scalar event metadata reads; all listed public-contract exclusions are behavior-proven as absent from typed C# | C# `ActionBegin<TRow>` shape corrected with `Name` and `RowIndex`; no DOM/internal/duplicate/absent payload members | focused typed DSL proof passed for save/edit accepted fields and listed exclusions; broad `ActionBegin` and shared edit-action payload still open |
| event variant row: `actionComplete` save/edit | [save-edit probe](probes/raw-ej2-action-complete-save-edit.html) | [row artifact](discovery/event-row-action-complete-save-edit.md) + [save-edit trace](traces/raw-ej2-action-complete-save-edit.trace.json) | event is builder-connectable only | typed current/previous row DTOs plus scalar event metadata reads; all listed public-contract exclusions are behavior-proven as absent from typed C# | C# `ActionComplete<TRow>` reuses shared edit-action args; no DOM/internal/duplicate/undefined payload members | focused typed DSL proof passed for save/edit accepted fields and listed exclusions; broad `ActionComplete` and shared edit-action payload still open |
| event row: `beginEdit` normal edit | [normal-edit probe](probes/raw-ej2-begin-edit-normal.html) | [row artifact](discovery/event-row-begin-edit-normal.md) + [normal-edit trace](traces/raw-ej2-begin-edit-normal.trace.json) | event is builder-connectable only | typed row DTO, row coordinate, edit metadata, and cancel payload mutation; DOM/internal/duplicate metadata excluded | current C# `BeginEdit<TRow>` shape matches accepted fields and `Cancel()` mutation | focused typed DSL proof passed for accepted fields, cancel mutation, and listed exclusions; broad `BeginEdit` still open |
| event row: `beforeBatchSave` batch edit | [batch-edit probe](probes/raw-ej2-before-batch-save-batch-edit.html) | [row artifact](discovery/event-row-before-batch-save-batch-edit.md) + [batch-edit trace](traces/raw-ej2-before-batch-save-batch-edit.trace.json) | event is builder-connectable only | typed batch-change DTO, cancel flag, and cancel payload mutation; duplicate event name metadata excluded | current C# `BeforeBatchSave<TRow>` shape matches accepted fields and `Cancel()` mutation | focused typed DSL proof passed for accepted fields, cancel mutation, and `Name` exclusion; broad `BeforeBatchSave` still open |
| event row: `cellSave` batch edit | [batch-edit probe](probes/raw-ej2-cell-save-batch-edit.html) | [row artifact](discovery/event-row-cell-save-batch-edit.md) + [batch-edit trace](traces/raw-ej2-cell-save-batch-edit.trace.json) | event is builder-connectable only | typed row DTO, edited field, typed current/previous values, and cancel payload mutation; DOM/vendor column metadata excluded | current C# `CellSave<TRow, TValue>` shape matches accepted fields and `Cancel()` mutation | focused typed DSL proof passed for accepted fields, cancel mutation, and listed exclusions; broad `CellSave` still open |
| event row: `cellSaved` batch edit | [batch-edit probe](probes/raw-ej2-cell-saved-batch-edit.html) | [row artifact](discovery/event-row-cell-saved-batch-edit.md) + [batch-edit trace](traces/raw-ej2-cell-saved-batch-edit.trace.json) | event is builder-connectable only | typed row DTO, edited field, typed saved/previous values; post-save `cancel` mutation excluded by trace | C# `CellSaved<TRow, TValue>` split to `FusionGridCellSavedArgs<TRow, TValue>` with no `Cancel()` | focused typed DSL proof passed for accepted fields and listed exclusions; broad `CellSaved` still open |
| remote data row: whole response `{ result, count }` | [remote response probe](probes/raw-ej2-remote-response-shape.html) | [row artifact](discovery/runtime-row-remote-response-shape.md) + [trace](traces/raw-ej2-remote-response-shape.trace.json) | post-render response body is runtime-owned, not builder-owned | response body read plus component property set; no new primitive | `SetDataSource(ResponseBody<TResponse>)` accepted for whole custom-binding response body | focused typed DSL proof passed; response-path/event-payload/adaptor/nested lanes still open |
| data-source row: typed array read/rebind/refresh | [data-source probe](probes/raw-ej2-data-source-read-refresh.html) | [row artifact](discovery/runtime-row-data-source-read-refresh.md) + [trace](traces/raw-ej2-data-source-read-refresh.trace.json) | `dataSource` is builder-owned initially, but post-render read/write is runtime-owned | component property read/write plus method-call primitives; no new primitive | `SetDataSource(TypedSource<T[]>)`, `Data<TRow>()`, and `Refresh()` accepted for this row only | focused typed DSL proof passed; response-path/adaptor/event-payload overloads still open |
| remote data lane: custom binding/data source | `dataSource`, `dataStateChange`, whole-response `SetDataSource`, response-path `SetDataSource`, event-payload path `SetDataSource`, DataManager/adaptor candidates | current event rows and whole-response row prove only their named source scopes | builder-owned initial `dataSource`; path/adaptor/nested paths still require runtime proof | response-body read, event payload read, component property set/read, and method-call primitives; no new primitive allowed | whole-response `SetDataSource`, typed-array `SetDataSource`, `Data`, and `Refresh` are proven separately; response-path/adaptor/event-payload lanes remain matrix-open | fail-closed until realistic remote/custom-binding behavior is covered member by member |

## Linked Artifacts

- [Source inventory](discovery/source-inventory.md)
- [Skill pattern map](../_skill/pattern-map.md)
- [MVC builder coverage](discovery/mvc-builder-coverage.md)
- [Blazor candidates](discovery/blazor-candidates.md)
- [Public API surface](discovery/public-api-surface.json)
- [Event payload surface](discovery/event-payload-surface.json)
- [Raw EJ2 core probe](probes/raw-ej2-core.html)
- [Raw EJ2 core trace](traces/raw-ej2-core.trace.json)
- [DataStateChange sorting row](discovery/event-row-data-state-change-sorting.md)
- [DataStateChange sorting judgment calls](discovery/judgment-calls-data-state-change-sorting.md)
- [DataStateChange sorting probe](probes/raw-ej2-data-state-change-sorting.html)
- [DataStateChange sorting trace](traces/raw-ej2-data-state-change-sorting.trace.json)
- [DataStateChange sorting header-click probe](probes/raw-ej2-data-state-change-sorting-header-click.html)
- [DataStateChange sorting header-click trace](traces/raw-ej2-data-state-change-sorting-header-click.trace.json)
- [DataStateChange paging row](discovery/event-row-data-state-change-paging.md)
- [DataStateChange paging judgment calls](discovery/judgment-calls-data-state-change-paging.md)
- [DataStateChange paging method probe](probes/raw-ej2-data-state-change-paging-method.html)
- [DataStateChange paging method trace](traces/raw-ej2-data-state-change-paging-method.trace.json)
- [DataStateChange paging pager-click probe](probes/raw-ej2-data-state-change-paging-pager-click.html)
- [DataStateChange paging pager-click trace](traces/raw-ej2-data-state-change-paging-pager-click.trace.json)
- [DataStateChange filtering method row](discovery/event-row-data-state-change-filtering-method.md)
- [DataStateChange filtering method judgment calls](discovery/judgment-calls-data-state-change-filtering-method.md)
- [DataStateChange filtering method probe](probes/raw-ej2-data-state-change-filtering-method.html)
- [DataStateChange filtering method trace](traces/raw-ej2-data-state-change-filtering-method.trace.json)
- [DataStateChange filtering FilterBar row](discovery/event-row-data-state-change-filtering-filterbar.md)
- [DataStateChange filtering FilterBar judgment calls](discovery/judgment-calls-data-state-change-filtering-filterbar.md)
- [DataStateChange filtering FilterBar probe](probes/raw-ej2-data-state-change-filtering-filterbar.html)
- [DataStateChange filtering FilterBar trace](traces/raw-ej2-data-state-change-filtering-filterbar.trace.json)
- [DataStateChange clear-filtering method row](discovery/event-row-data-state-change-clear-filtering-method.md)
- [DataStateChange clear-filtering method judgment calls](discovery/judgment-calls-data-state-change-clear-filtering-method.md)
- [DataStateChange clear-filtering method probe](probes/raw-ej2-data-state-change-clear-filtering-method.html)
- [DataStateChange clear-filtering method trace](traces/raw-ej2-data-state-change-clear-filtering-method.trace.json)
- [DataStateChange searching method row](discovery/event-row-data-state-change-searching-method.md)
- [DataStateChange searching method judgment calls](discovery/judgment-calls-data-state-change-searching-method.md)
- [DataStateChange searching method probe](probes/raw-ej2-data-state-change-searching-method.html)
- [DataStateChange searching method trace](traces/raw-ej2-data-state-change-searching-method.trace.json)
- [DataStateChange clear-search method row](discovery/event-row-data-state-change-clear-search-method.md)
- [DataStateChange clear-search method judgment calls](discovery/judgment-calls-data-state-change-clear-search-method.md)
- [DataStateChange clear-search method probe](probes/raw-ej2-data-state-change-clear-search-method.html)
- [DataStateChange clear-search method trace](traces/raw-ej2-data-state-change-clear-search-method.trace.json)
- [DataStateChange clear-sorting method row](discovery/event-row-data-state-change-clear-sorting-method.md)
- [DataStateChange clear-sorting method judgment calls](discovery/judgment-calls-data-state-change-clear-sorting-method.md)
- [DataStateChange clear-sorting method probe](probes/raw-ej2-data-state-change-clear-sorting-method.html)
- [DataStateChange clear-sorting method trace](traces/raw-ej2-data-state-change-clear-sorting-method.trace.json)
- [DataStateChange grouping method row](discovery/event-row-data-state-change-grouping-method.md)
- [DataStateChange grouping method judgment calls](discovery/judgment-calls-data-state-change-grouping-method.md)
- [DataStateChange grouping method probe](probes/raw-ej2-data-state-change-grouping-method.html)
- [DataStateChange grouping method trace](traces/raw-ej2-data-state-change-grouping-method.trace.json)
- [DataStateChange ungrouping method row](discovery/event-row-data-state-change-ungrouping-method.md)
- [DataStateChange ungrouping method judgment calls](discovery/judgment-calls-data-state-change-ungrouping-method.md)
- [DataStateChange ungrouping method probe](probes/raw-ej2-data-state-change-ungrouping-method.html)
- [DataStateChange ungrouping method trace](traces/raw-ej2-data-state-change-ungrouping-method.trace.json)
- [DataStateChange clear-grouping method row](discovery/event-row-data-state-change-clear-grouping-method.md)
- [DataStateChange clear-grouping method judgment calls](discovery/judgment-calls-data-state-change-clear-grouping-method.md)
- [DataStateChange clear-grouping method probe](probes/raw-ej2-data-state-change-clear-grouping-method.html)
- [DataStateChange clear-grouping method trace](traces/raw-ej2-data-state-change-clear-grouping-method.trace.json)
- [RecordClick cell row](discovery/event-row-record-click-cell.md)
- [RecordClick cell judgment calls](discovery/judgment-calls-record-click-cell.md)
- [RecordClick cell probe](probes/raw-ej2-record-click-cell.html)
- [RecordClick cell trace](traces/raw-ej2-record-click-cell.trace.json)
- [RowSelected click row](discovery/event-row-row-selected-click.md)
- [RowSelected click judgment calls](discovery/judgment-calls-row-selected-click.md)
- [RowSelected click probe](probes/raw-ej2-row-selected-click.html)
- [RowSelected click trace](traces/raw-ej2-row-selected-click.trace.json)
- [ToolbarClick custom row](discovery/event-row-toolbar-click-custom.md)
- [ToolbarClick custom judgment calls](discovery/judgment-calls-toolbar-click-custom.md)
- [ToolbarClick custom probe](probes/raw-ej2-toolbar-click-custom.html)
- [ToolbarClick custom trace](traces/raw-ej2-toolbar-click-custom.trace.json)
- [ActionBegin save/edit row](discovery/event-row-action-begin-save-edit.md)
- [ActionBegin save/edit judgment calls](discovery/judgment-calls-action-begin-save-edit.md)
- [ActionBegin save/edit probe](probes/raw-ej2-action-begin-save-edit.html)
- [ActionBegin save/edit trace](traces/raw-ej2-action-begin-save-edit.trace.json)
- [ActionComplete save/edit row](discovery/event-row-action-complete-save-edit.md)
- [ActionComplete save/edit judgment calls](discovery/judgment-calls-action-complete-save-edit.md)
- [ActionComplete save/edit probe](probes/raw-ej2-action-complete-save-edit.html)
- [ActionComplete save/edit trace](traces/raw-ej2-action-complete-save-edit.trace.json)
- [BeginEdit normal edit row](discovery/event-row-begin-edit-normal.md)
- [BeginEdit normal edit judgment calls](discovery/judgment-calls-begin-edit-normal.md)
- [BeginEdit normal edit probe](probes/raw-ej2-begin-edit-normal.html)
- [BeginEdit normal edit trace](traces/raw-ej2-begin-edit-normal.trace.json)
- [BeforeBatchSave batch edit row](discovery/event-row-before-batch-save-batch-edit.md)
- [BeforeBatchSave batch edit judgment calls](discovery/judgment-calls-before-batch-save-batch-edit.md)
- [BeforeBatchSave batch edit probe](probes/raw-ej2-before-batch-save-batch-edit.html)
- [BeforeBatchSave batch edit trace](traces/raw-ej2-before-batch-save-batch-edit.trace.json)
- [CellSave batch edit row](discovery/event-row-cell-save-batch-edit.md)
- [CellSave batch edit judgment calls](discovery/judgment-calls-cell-save-batch-edit.md)
- [CellSave batch edit probe](probes/raw-ej2-cell-save-batch-edit.html)
- [CellSave batch edit trace](traces/raw-ej2-cell-save-batch-edit.trace.json)
- [CellSaved batch edit row](discovery/event-row-cell-saved-batch-edit.md)
- [CellSaved batch edit judgment calls](discovery/judgment-calls-cell-saved-batch-edit.md)
- [CellSaved batch edit probe](probes/raw-ej2-cell-saved-batch-edit.html)
- [CellSaved batch edit trace](traces/raw-ej2-cell-saved-batch-edit.trace.json)
- [Remote response shape row](discovery/runtime-row-remote-response-shape.md)
- [Remote response shape judgment calls](discovery/judgment-calls-remote-response-shape.md)
- [Remote response shape probe](probes/raw-ej2-remote-response-shape.html)
- [Remote response shape trace](traces/raw-ej2-remote-response-shape.trace.json)
- [Data-source read/rebind/refresh row](discovery/runtime-row-data-source-read-refresh.md)
- [Data-source read/rebind/refresh judgment calls](discovery/judgment-calls-data-source-read-refresh.md)
- [Data-source read/rebind/refresh probe](probes/raw-ej2-data-source-read-refresh.html)
- [Data-source read/rebind/refresh trace](traces/raw-ej2-data-source-read-refresh.trace.json)
- [Primitive map](mapping/primitive-map.md)
- [C# name decisions](mapping/csharp-name-decisions.md)
- [Vertical slice plan](mapping/vertical-slice-plan.md)
- [Typed API coverage matrix](proof/typed-api-coverage-matrix.md)
- [Playwright proof](proof/playwright-proof.md)
- [Audit report](proof/audit-report.md)
