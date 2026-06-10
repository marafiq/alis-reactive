# Grid Audit Report

Status: fail-closed. Grid is not audited. The `dataStateChange` sorting row,
page-number paging row, method-trigger filtering row, FilterBar typing
filtering row, method-trigger clear-filtering row, method-trigger searching row, method-trigger clear-search row, method-trigger clear-sorting row,
method-trigger grouping row, method-trigger ungrouping row, method-trigger clear-grouping row,
`recordClick` cell row, and `rowSelected` click
row have focused typed DSL proof. The `toolbarClick` custom item row also has
focused typed DSL proof. The
`actionBegin` save/edit variant row and `actionComplete` save/edit accepted
fields plus all listed public-contract exclusions also have focused typed DSL
proof. The `beginEdit` normal-edit row also has focused typed DSL proof for
accepted fields, cancel mutation, and listed public-contract exclusions. The
`cellSave` batch-edit row also has focused typed DSL proof for accepted fields,
cancel mutation, and listed public-contract exclusions. The `cellSaved`
batch-edit row also has focused typed DSL proof for accepted fields and listed
public-contract exclusions. The `beforeBatchSave` batch-edit row also has
focused typed DSL proof for accepted fields, cancel mutation, and the `Name`
exclusion. The
remote whole-response `{ result, count }` row and the data-source typed-array
row also have focused typed DSL proof. The component-wide typed API matrix
remains open.

## Current Finding

The `dataStateChange` sorting row, page-number paging row, method-trigger
filtering row, FilterBar typing filtering row, method-trigger clear-filtering row, method-trigger searching row,
method-trigger clear-search row, method-trigger clear-sorting row, method-trigger grouping row, method-trigger ungrouping row,
method-trigger clear-grouping row, and `recordClick`
cell row, `rowSelected` click row, and `toolbarClick` custom item row,
`actionBegin` save/edit variant row,
`actionComplete` save/edit accepted fields plus all listed public-contract
exclusions, `beginEdit` normal-edit accepted fields plus cancel mutation and
listed public-contract exclusions, `cellSave` batch-edit accepted fields plus
cancel mutation and listed public-contract exclusions, `cellSaved` batch-edit
accepted fields plus listed public-contract exclusions, `beforeBatchSave`
batch-edit accepted fields plus cancel mutation and the `Name` exclusion,
remote whole-response row, and data-source typed-array row have deterministic
raw EJ2 evidence, explicit C# judgment calls, and typed DSL proof.

Skill pattern feedback:

- `_skill/pattern-map.md#p001-event-variants-are-separate-rows-until-proven-equivalent`
- `_skill/pattern-map.md#p000-discovery-is-exhaustive-but-public-c-dsl-is-selective`
- `_skill/pattern-map.md#p002-custom-binding-data-events-require-custom-binding-data-shape`
- `_skill/pattern-map.md#p003-proper-array-primitive-means-whole-typed-array-source-or-array-operation`
- `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them`
- `_skill/pattern-map.md#p005-event-payload-coverage-is-property-level`
- `_skill/pattern-map.md#p006-static-type-discovery-must-follow-the-component-import-graph`
- `_skill/pattern-map.md#p007-remote-data-is-a-primary-behavior-lane`
- `_skill/pattern-map.md#p008-public-api-matrix-rows-need-stable-behavior-identity`
- `_skill/pattern-map.md#p009-variant-sensitive-payload-rows-must-not-be-coalesced`
- `_skill/pattern-map.md#p010-open-audit-lanes-must-be-matrix-rows-not-prose`
- `_skill/pattern-map.md#p011-generated-coverage-rows-must-be-artifact-derived`
- `_skill/pattern-map.md#p012-exclusion-rows-require-explicit-exclusion-proof`
- `_skill/pattern-map.md#p013-data-source-rows-split-by-value-scope-and-refresh-behavior`
- `_skill/pattern-map.md#p014-gesture-commit-semantics-are-part-of-event-proof`
- `_skill/pattern-map.md#p015-proof-references-must-be-current-after-any-row-affecting-change`
- `_skill/pattern-map.md#p016-shared-payload-types-do-not-prove-shared-event-rows`
- `_skill/pattern-map.md#p017-focused-proof-views-preserve-vertical-slice-accountability`
- `_skill/pattern-map.md#p018-writable-payload-fields-need-lifecycle-effect-proof`
- `_skill/pattern-map.md#p019-clear-and-reset-methods-must-not-be-masked-by-manual-reloads`

Observed sorting fields now accepted/proven:

- `requiresCounts`
- top-level `name`
- `action.cancel`
- `action.name`
- `action.type`
- `action.target` is intentionally excluded from the public typed payload because header-click sorting proves it can be a DOM `TH` element.

Observed paging candidates now classified:

- `action.currentPage`, `action.previousPage`, and `action.pageSize` are accepted typed scalars.
- `action.previousPageSize` is deferred to a page-size-change row.
- `action.rows` and `action.target` are excluded for the page-number paging row.

Observed filtering candidates now classified:

- `where` recursive predicate shape is accepted as the server filtering payload.
- FilterBar typing emits the same accepted `where` predicate shape as
  method-trigger filtering, with `startswith` and the committed text value.
- FilterBar typing explicitly checks declared-but-absent data-state candidates:
  `aggregates`, `dataSource`, `isLazyLoad`, `onDemandGroupInfo`, `select`, and
  `table`; those require separate behavior rows before any C# DSL mapping.
- `Predicate` and `MatchCase` were removed from the public filter criterion because they are not emitted by top-level `where`.
- `action.currentFilterObject`, `action.columns`, `action.currentFilteringColumn`, and `action.action` are excluded for the method-filtering row.
- `action.currentFilterObject`, `action.columns`, `action.currentFilteringColumn`, and `action.action` are also excluded for the FilterBar typing row.

Observed clear-filtering candidates now classified:

- `ClearFiltering()` is the typed C# method use case; it maps through the
  existing component method-call primitive, not a new primitive.
- the raw clear-filtering event uses `action.requestType=refresh` and omits
  top-level `where`; do not infer `Where = []`.
- `action.requestType` and `action.name` are accepted through shared action
  metadata.
- `action.type`, `action.cancel`, `action.action`,
  `action.currentFilteringColumn`, `action.currentFilterObject`, and
  `action.columns` are excluded for this row.
- the Directory sandbox no longer calls a manual `LoadDirectory(...)` after
  `ClearFiltering()`, so the proof must pass through the method's event lane.

Observed searching candidates now classified:

- `search` descriptor shape is accepted as the server searching payload.
- `action.searchString` is excluded because it duplicates `search[].key`.
- `action.cancel` is not accepted for the searching row because the search trace does not emit it.

Observed clear-search candidates now classified:

- `ClearSearch()` is the typed C# method use case; it maps through the existing
  search method primitive with an empty string, not a new primitive.
- the raw clear-search event omits top-level `search`; do not infer `Search = []`.
- `action.requestType`, `action.name`, and `action.type` are accepted through
  shared action metadata.
- `action.searchString` is excluded because the empty string is a duplicate
  signal for the clear method and does not justify a public C# payload member.
- `action.cancel`, `where`, `group`, and `sorted` are excluded for this row.

Observed grouping candidates now classified:

- `group` string array is accepted as the server grouping payload.
- `groups` is excluded because it duplicates `group` and is not the declared public `DataStateChangeEventArgs` member.
- `action.preventFocusOnGroup` is excluded as a Syncfusion internal focus flag.
- grouping auto-sort state is handled by the existing `Sorted` contract.

Observed ungrouping candidates now classified:

- `UngroupBy(...)` maps to Syncfusion `ungroupColumn(...)` through the typed
  component method DSL.
- `action.requestType`, `action.name`, `action.type`, and
  `action.columnName` are accepted for the ungrouping row.
- `name`, `skip`, `take`, and `requiresCounts` are accepted event reads for
  this row, but the Directory proof reads `requiresCounts` visibly and does
  not gather it into the remote request body.
- `group` is excluded for this row because the ungrouping trace omits the
  top-level key; do not infer `Group = []`.
- `sorted`, `where`, `search`, `aggregates`, `groups`,
  `action.cancel`, and `action.preventFocusOnGroup` are excluded for this row.

Observed clear-grouping candidates now classified:

- `ClearGrouping()` maps to Syncfusion `clearGrouping()` through the typed
  component method DSL.
- local Syncfusion source proves `clearGrouping()` delegates to `ungroupColumn`
  and enables content refresh only for the final grouped column.
- the two-group trace emits one final `dataStateChange` event with
  `action.requestType=ungrouping` and `action.columnName=wing`.
- `name`, `skip`, `take`, `requiresCounts`, and shared action metadata are
  accepted for this row.
- `group`, `groups`, and `sorted` are excluded for this row because the clear
  event omits the top-level keys; do not infer empty arrays.
- `where`, `search`, `aggregates`, `action.cancel`, and
  `action.preventFocusOnGroup` are excluded for this row.

Observed record-click candidates now classified:

- `rowData`, `rowIndex`, `cellIndex`, and `name` are accepted for the normal
  data-cell click row.
- `cell`, `row`, `target`, and `event` are excluded as browser-owned DOM/event
  objects.
- `column` is excluded from this row as a broad vendor-owned object until a
  separate typed column-source row proves a focused C# use case.
- `cancel` is excluded because this row does not prove useful cancel behavior
  and Syncfusion does not read it after `recordClick`.

Observed row-selected candidates now classified:

- `data`, `rowIndex`, `previousRowIndex`, `isInteracted`, and `name` are
  accepted for normal single row-click selection.
- `PreviousRowIndex` is nullable because the first click emits the own key with
  an undefined value and the second click emits `0`.
- `row`, `previousRow`, and `target` are excluded as browser-owned DOM objects.
- `foreignKeyData`, `isHeaderCheckBoxClicked`, and `rowIndexes` are discovered
  but excluded until separate foreign-key, checkbox, and multiple/range
  selection rows prove focused typed use cases.

Observed toolbar-click candidates now classified:

- `item.id`, `item.text`, `cancel`, and `name` are accepted for custom toolbar
  item clicks.
- `originalEvent` is excluded as a browser-owned `PointerEvent`.
- `item.tooltipText`, `item.prefixIcon`, `item.suffixIcon`, `item.disabled`,
  `item.visible`, `item.type`, and `item.align` are discovered but excluded
  until focused rows prove useful C# DSL behavior.
- `Cancel` is read-proven only; mutation/default-action prevention requires a
  built-in toolbar action row.

Observed action-begin save/edit candidates now classified:

- `name`, `requestType`, `action`, `type`, `cancel`, `data`, `previousData`,
  `rowIndex`, and existing `selectedRow` are accepted for the save/edit variant.
- `row`, `form`, and `target` are excluded as browser-owned or absent DOM
  payload surfaces.
- `foreignKeyData`, `isScroll`, `primaryKey`, `primaryKeyValue`, and `rowData`
  are discovered but excluded for this variant.
- `Index` was removed from the shared public type because this variant does not
  emit it as an own payload key and no focused behavior row proves it.
- The focused proof reads each accepted field through typed `ActionBegin` DSL,
  updates visible UI, and asserts no public typed args properties exist for
  `Row`, `Form`, `Target`, `ForeignKeyData`, `IsScroll`, `PrimaryKey`,
  `PrimaryKeyValue`, `RowData`, or `Index`.
- The broad `ActionBegin` selector is now row-proven: the admission-audit slice
  proves the add and delete edit-action variants alongside save/edit, and
  `FusionGridEditActionArgs` is row-proven across those variants with the
  variant-sensitive `Data`/`PreviousData`/`Action` members recorded per P024.

Observed action-complete save/edit candidates now classified:

- `name`, `requestType`, `action`, `type`, `cancel`, `data`, `previousData`,
  `rowIndex`, and existing `selectedRow` are accepted for the save/edit
  variant.
- The focused proof reads each accepted field through typed `ActionComplete`
  DSL and updates visible UI in the `ActionCompleteSaveEdit` vertical slice.
- `row`, `form`, `target`, `foreignKeyData`, `isScroll`, `primaryKey`,
  `primaryKeyValue`, `rowData`, `index`, and `promise` public-contract
  exclusions are explicitly proven by the focused test asserting no public
  typed args properties.
- The broad `ActionComplete` selector is now row-proven across the save/edit, add,
  and delete edit-action variants through the action-complete and admission-audit
  slices.

Observed data-source candidates now classified:

- `SetDataSource(ResponseBody<TResponse>)` is accepted for whole response
  custom-binding shape `{ result, count }`.
- `SetDataSource(TypedSource<T[]>)` is accepted for the typed-array row.
- `Data<TRow>()` is accepted for reading current `grid.dataSource` as a typed
  array source.
- `Refresh()` is accepted for applying the post-render rebind visibly.
- Response-path `SetDataSource`, event-payload path `SetDataSource`,
  DataManager/adaptor binding, builder-owned initial dataSource, and nested
  data-source paths are now each row-proven through their own focused slices.

Current audit-tooling findings:

- The typed API coverage matrix now emits property-level payload rows. This
  exposed the then-open `FusionGridEditActionArgs.Index`, which has now been removed
  from the public C# DSL instead of being hidden behind the partially proven
  `FusionGridEditActionArgs` class.
- The artifact gate now checks that `typed-api-coverage-matrix.md` is current
  against the C# source and artifact judgment decisions. It also fails on
  absent master count rows, absent audit count summary, and absent cited
  Playwright TRX files instead of silently accepting stale proof bookkeeping.
- The Grid proof workflow now uses focused vertical-slice proof views when a
  broad sandbox page would make row ownership ambiguous. The
  `ActionCompleteSaveEdit` view is linked from the Grid sandbox surfaces and is
  the proof surface for the `actionComplete` save/edit row.
- The typed API coverage matrix now distinguishes overload/helper behavior
  identity. Data-source rows are split into `SetDataSource [response path]`,
  `SetDataSource [whole response body]`, `SetDataSource [event payload path]`,
  `SetDataSource [typed array source]`, `Data [component dataSource read]`, and
  `Refresh [component refresh method]`; payload mutation helpers are
  owner-qualified, such as `FusionGridCellSaveArgs.Cancel()`.
- Coalesced `dataStateChange` payload class/property rows are no longer marked
  `row-proven`. They resolved once a variant matrix named accepted,
  absent, excluded, or deferred status for sorting, paging, filtering,
  searching, grouping, and later variants.
- The typed API coverage matrix now emits generated `dataStateChange/<variant>`
  rows for accepted and excluded payload members by reading the judgment-call
  artifacts. Proven variant rows stay `row-proven`; accepted/excluded judgment
  rows without focused typed DSL behavior proof stayed open, and shared
  class/property rows resolved once every relevant variant had explicit
  status.
- Exclusion rows now fail closed unless an explicit exclusion proof exists for
  the exact public contract member. Request-body absence, vertical-slice
  removal, and judgment-call text remain supporting evidence, not proof.
- The filtering-method pass resolved a real contradiction: `isComplex` is not a
  settings-only field, because Syncfusion declares `DataStateChangeEventArgs.where`
  as `Predicate[]`, and `Predicate` declares and serializes `isComplex`. The
  public C# row now accepts `FusionGridTextFilterCriterion.IsComplex` and the
  focused proof shows the sandbox server uses it before recursive flattening.
  The same focused proof closes settings-only action fields, `matchCase`,
  `predicate`, and variant-foreign `search`/`group`/`sorted` for this
  filtering-method variant.
- The generated matrix does not mark the two `IsComplex` variant rows proven
  from a label alone. The generator checks for the public C# property, the
  server `filter.IsComplex` recursion guard, the Playwright parent/leaf
  `isComplex` assertions, and the proof artifact text before assigning
  `row-proven` to those rows.
- The typed API coverage matrix now emits generated remote/data-source rows for
  builder-owned `dataSource`, DataManager/adaptor, nested data-source paths,
  `{ result, count }` response shape, all four `SetDataSource` lanes, `Data`,
  and `Refresh`. The whole-response `SetDataSource` and `{ result, count }`
  rows are row-proven from the remote response trace and focused Grid proof.
  The typed-array `SetDataSource`, `Data`, and `Refresh` rows are row-proven
  from the focused data-source trace and ArrayGrid proof. The response-path,
  event-payload, adaptor, builder-owned, and nested-path rows are now row-proven
  through their own focused slices.
- Static event payload discovery must resolve TypeScript types through the Grid
  import graph. Prior broad discovery polluted Grid with same-named payload
  types from FileManager, Buttons, and Chips; the discovery script now treats
  unresolved multiple candidates as ambiguous instead of silently accepting the
  first match.
- Static event payload discovery now has zero `ambiguous`, `not-found`, or
  `cycle` payload types for Grid. Broad `Object` payloads remain classified as
  `builtin-object` discovery entries and require focused runtime rows before
  any public C# DSL can be considered.
- Remote/custom-binding behavior remains a first-class lane. Whole-response
  `{ result, count }`, typed-array `SetDataSource`, `Data`, and `Refresh` are
  proven, and response-path, event-payload path, DataManager/adaptor,
  builder-owned initial dataSource, and nested data-source behavior are now each
  proven through their own focused server-roster, batch-change-review,
  remote-adaptor, builder-roster, and nested-page slices.
- BeginEdit normal-edit accepted/excluded judgments are now generated matrix
  rows instead of prose-only notes. Focused typed DSL behavior proves
  the accepted fields, cancel mutation, and explicit public C# exclusions for
  the normal-edit existing-row variant.
- Current generated matrix count: 165 typed C# API rows, 294 supplemental audit
  rows, 459 total rows. The latest count check correctly shows 459 row-proven
  matrix rows and 0 matrix rows without `row-proven` status.

## Reviewer Simulation Snapshot

| Reviewer | Finding |
| --- | --- |
| Principal DSL reviewer | Existing event payload read and array primitives are enough; no primitive change is justified. |
| Fusion discovery reviewer | Sorting method/header-click traces and paging method/pager-click traces are valid custom-binding rows. |
| C# API reviewer | Sorting metadata/cancel fields and paging scalar fields have explicit accept/exclude decisions. |
| Vertical slice reviewer | Keep payload types under `Events/FusionGridOnDataStateChange.cs` for this row. |
| Playwright behavior reviewer | Sorting, typed `SortBy`, page-number paging, method-trigger filtering, FilterBar typing filtering, method-trigger clear-filtering, method-trigger searching, method-trigger clear-search, method-trigger clear-sorting, method-trigger grouping, method-trigger ungrouping, method-trigger clear-grouping, record-click cell, row-selected click, toolbar-click custom item, action-begin save/edit, action-complete save/edit, begin-edit normal edit, cell-save batch edit, remote whole-response, and data-source typed-array proofs passed through `scripts/playwright.sh`. |
| Artifact consistency reviewer | Master index, row artifacts, primitive map, name decisions, and vertical slice plan agree that full Grid audit remains open. |

## Proven Row

`dataStateChange` sorting:

- raw method trace: `traces/raw-ej2-data-state-change-sorting.trace.json`
- raw header-click trace: `traces/raw-ej2-data-state-change-sorting-header-click.trace.json`
- judgment calls: `discovery/judgment-calls-data-state-change-sorting.md`
- primitive map: `mapping/primitive-map.md`
- name decisions: `mapping/csharp-name-decisions.md`
- typed DSL proof: `tests/Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGrid.sorting_a_column_fetches_sorted_data_and_echoes_action`
- command: `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGrid.sorting_a_column_fetches_sorted_data_and_echoes_action"`
- result: passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-231608.trx`
- exclusion proof: request body omits sorting-foreign `where`, `search`, and
  `group`; public `FusionGridAction` contract omits browser-owned `Target`.

## Proven Row

`FusionGrid.SortBy` method:

- raw method trace: `traces/raw-ej2-data-state-change-sorting.trace.json`
- Syncfusion public method evidence:
  `node_modules/@syncfusion/ej2-grids/src/grid/actions/sort.d.ts`
- primitive map: `mapping/primitive-map.md`
- name decisions: `mapping/csharp-name-decisions.md`
- vertical slice: `mapping/vertical-slice-plan.md`
- typed DSL proof target: `tests/Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.sort_by_method_sends_typed_sorted_payload_and_refreshes_grid`
- command: `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.sort_by_method_sends_typed_sorted_payload_and_refreshes_grid"`
- result: passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260607-024311.trx`
- accepted proof: the public C# method
  `SortBy((ResidentDirectoryGridItem x) => x.RiskLevel, Descending)` maps to
  Syncfusion `sortColumn("riskLevel", "Descending", false)`, emits
  `dataStateChange`, gathers `sorted[0].name=riskLevel` and
  `sorted[0].direction=descending`, and visibly refreshes the first row to
  `Grace Bennett` with risk `Moderate`.
- exclusion proof: request body omits sorting-foreign `where`, `search`,
  `group`, `aggregates`, untyped `action`, and browser-owned `actionTarget`.
- explicit non-closure: this method row does not prove header-click sorting,
  multi-sort retention, custom comparer sorting, or other public query methods.

## Proven Row

`dataStateChange` page-number paging:

- raw method trace: `traces/raw-ej2-data-state-change-paging-method.trace.json`
- raw pager-click trace: `traces/raw-ej2-data-state-change-paging-pager-click.trace.json`
- judgment calls: `discovery/judgment-calls-data-state-change-paging.md`
- primitive map: `mapping/primitive-map.md`
- name decisions: `mapping/csharp-name-decisions.md`
- typed DSL proof target: `tests/Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGrid.paging_fetches_next_page_with_correct_skip`
- command: `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGrid.paging_fetches_next_page_with_correct_skip"`
- result: passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-232421.trx`
- exclusion proof: request body omits paging-foreign `where`, `search`, `group`,
  and `sorted`; request body omits page-size-change/browser-owned
  `actionPreviousPageSize`, `actionRows`, and `actionTarget`; public
  `FusionGridAction` contract omits `PreviousPageSize`, `Rows`, and `Target`.

## Proven Row

`dataStateChange` method-trigger filtering:

- raw method trace: `traces/raw-ej2-data-state-change-filtering-method.trace.json`
- judgment calls: `discovery/judgment-calls-data-state-change-filtering-method.md`
- primitive map: `mapping/primitive-map.md`
- name decisions: `mapping/csharp-name-decisions.md`
- typed DSL proof: `tests/Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.filtering_method_sends_typed_where_payload_and_refreshes_grid`
- command: `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.filtering_method_sends_typed_where_payload_and_refreshes_grid"`
- result: passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-205951.trx`

## Proven Row

`dataStateChange` method-trigger clear-filtering:

- raw method trace: `traces/raw-ej2-data-state-change-clear-filtering-method.trace.json`
- trace hash: `80755b925a130bb8eb9c445756fcfb48022dca3c612b0d169083d608def20537`
- judgment calls: `discovery/judgment-calls-data-state-change-clear-filtering-method.md`
- primitive map: `mapping/primitive-map.md`
- name decisions: `mapping/csharp-name-decisions.md`
- skill pattern: `_skill/pattern-map.md#p019-clear-and-reset-methods-must-not-be-masked-by-manual-reloads`
- typed DSL proof: `tests/Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.clear_filtering_method_clears_active_filter_and_refreshes_grid`
- command: `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.clear_filtering_method_clears_active_filter_and_refreshes_grid"`
- result: passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260607-014720.trx`
- accepted proof: visible `dataStateChange`, `skip=0`, `take=8`,
  `requiresCounts=true`, `action.requestType=refresh`, and
  `action.name=actionBegin`.
- exclusion proof: request body omits `where`, `search`, `group`, `sorted`,
  untyped `action`, `actionAction`, `actionColumns`,
  `actionCurrentFilterObject`, and `actionCurrentFilteringColumn`.
- behavior proof: the test starts from an active `FilterTextBy` state, then
  typed `ClearFiltering()` restores `240 residents matched` and first visible
  row `Amina Patel`.

## Proven Row

`dataStateChange` method-trigger searching:

- raw method trace: `traces/raw-ej2-data-state-change-searching-method.trace.json`
- judgment calls: `discovery/judgment-calls-data-state-change-searching-method.md`
- primitive map: `mapping/primitive-map.md`
- name decisions: `mapping/csharp-name-decisions.md`
- typed DSL proof: `tests/Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.searching_method_sends_typed_search_payload_and_refreshes_grid`
- command: `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.searching_method_sends_typed_search_payload_and_refreshes_grid"`
- result: passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-233449.trx`
- exclusion proof: request body omits duplicate `actionSearchString`, absent
  `actionCancel`, and searching-foreign `where`, `group`, and `sorted`; public
  `FusionGridAction` contract omits `SearchString`.

## Proven Row

`dataStateChange` method-trigger clear-search:

- raw method trace: `traces/raw-ej2-data-state-change-clear-search-method.trace.json`
- trace hash: `c855222fc79fa260062381aff53b269b9d7f34581cd91234f0a119d6e4b3e5b1`
- judgment calls: `discovery/judgment-calls-data-state-change-clear-search-method.md`
- primitive map: `mapping/primitive-map.md`
- name decisions: `mapping/csharp-name-decisions.md`
- typed DSL proof: `tests/Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.clear_search_method_clears_active_search_and_refreshes_grid`
- command: `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.clear_search_method_clears_active_search_and_refreshes_grid"`
- result: passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260607-013501.trx`
- accepted proof: visible `dataStateChange`, `skip=0`, `take=8`,
  `requiresCounts=true`, `action.requestType=searching`,
  `action.name=actionBegin`, and `action.type=actionBegin`.
- exclusion proof: request body omits `search`, `where`, `group`, `sorted`,
  untyped `action`, `actionSearchString`, and `actionCancel`; public
  `FusionGridAction` contract omits `SearchString`.
- behavior proof: the test starts from an active `Search("Memory")` state,
  then typed `ClearSearch()` restores `240 residents matched` and first visible
  row `Amina Patel`.

## Proven Row

`dataStateChange` method-trigger clear-sorting:

- raw method trace: `traces/raw-ej2-data-state-change-clear-sorting-method.trace.json`
- trace hash:
  `771356b66a51ccfd5c0edaa399f2b6765e758168b98409b0106214fd3a3cba3c`
- judgment calls: `discovery/judgment-calls-data-state-change-clear-sorting-method.md`
- primitive map: `mapping/primitive-map.md`
- name decisions: `mapping/csharp-name-decisions.md`
- typed DSL proof: `tests/Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.clear_sorting_method_clears_active_sort_and_refreshes_grid`
- command: `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.clear_sorting_method_clears_active_sort_and_refreshes_grid"`
- result: passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260607-023440.trx`
- accepted proof: visible `dataStateChange`, `skip=0`, `take=8`,
  `requiresCounts=true`, `action.requestType=sorting`,
  `action.name=actionBegin`, and `action.type=actionBegin`.
- exclusion proof: request body omits `sorted`, `where`, `search`, `group`,
  `aggregates`, untyped `action`, `actionColumnName`, `actionDirection`,
  `actionCancel`, and `actionTarget`; public `FusionGridAction` contract omits
  `Target`.
- behavior proof: the test starts from an active typed `SortBy` state, then
  typed `ClearSorting()` restores default resident-directory order with first
  visible row `Amina Patel` and risk `Low`.
- stale-field proof: visible `grid-column`, `grid-direction`, and
  `grid-action-cancel` do not keep the previous sort row values after clear.
- failed-first-attempt proof:
  `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260607-022415.trx`
  failed because the test guessed that descending risk would show `High` first.
  Actual server behavior sorted the first row to `Grace Bennett` with risk
  `Moderate`; the final proof now asserts observed behavior rather than a
  guessed domain ordering.
- explicit non-closure: header sort clear, column-menu sort clear, multi-sort
  clear, grouped-sort clear, and clear sorting combined with filter/search/group
  state remain open.

## Proven Row

`dataStateChange` method-trigger grouping:

- raw method trace: `traces/raw-ej2-data-state-change-grouping-method.trace.json`
- judgment calls: `discovery/judgment-calls-data-state-change-grouping-method.md`
- primitive map: `mapping/primitive-map.md`
- name decisions: `mapping/csharp-name-decisions.md`
- typed DSL proof: `tests/Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.grouping_method_sends_typed_group_payload_and_refreshes_grid`
- command: `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.grouping_method_sends_typed_group_payload_and_refreshes_grid"`
- result: passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-234123.trx`
- exclusion proof: request body omits duplicate `groups`, internal
  `actionPreventFocusOnGroup`, absent `actionCancel`, grouping-foreign `where`
  and `search`, and no-aggregate `aggregates`; public contracts omit
  `FusionGridDataStateChangeArgs.Groups` and
  `FusionGridAction.PreventFocusOnGroup`.

## Proven Row

`dataStateChange` method-trigger ungrouping:

- raw method trace: `traces/raw-ej2-data-state-change-ungrouping-method.trace.json`
- trace hash: `d0eddc3d1bada900f469005040478b1fbb74ef013a062eb7d84e5fdc1d606902`
- judgment calls: `discovery/judgment-calls-data-state-change-ungrouping-method.md`
- primitive map: `mapping/primitive-map.md`
- name decisions: `mapping/csharp-name-decisions.md`
- typed DSL proof: `tests/Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.ungrouping_method_sends_typed_action_payload_and_refreshes_grid`
- command: `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.ungrouping_method_sends_typed_action_payload_and_refreshes_grid"`
- result: passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260607-011802.trx`
- accepted proof: visible `dataStateChange`, `skip=0`, `take=8`,
  `requiresCounts=true`, `action.requestType=ungrouping`,
  `action.name=actionBegin`, `action.type=actionBegin`,
  `action.columnName=careLevel`, and visible ungrouped summary
  `240 residents matched`.
- exclusion proof: request body omits `requiresCounts`, `group`, `groups`,
  `sorted`, `where`, `search`, `aggregates`, untyped `action`,
  `actionCancel`, and `actionPreventFocusOnGroup`; public contracts omit
  `FusionGridDataStateChangeArgs.Groups`,
  `FusionGridDataStateChangeArgs.Aggregates`, and
  `FusionGridAction.PreventFocusOnGroup`.
- behavior proof: typed `GroupBy` first creates group captions, typed
  `UngroupBy` removes them, group caption count becomes `0`, and the first
  normal row contains `Amina Patel`.

## Proven Row

`dataStateChange` method-trigger clear grouping:

- raw method trace: `traces/raw-ej2-data-state-change-clear-grouping-method.trace.json`
- trace hash: `fc59803ac3cb75c0a3dc9f4a29dc536ae427fd9a86bdc79ee8293b7af7c004e8`
- judgment calls: `discovery/judgment-calls-data-state-change-clear-grouping-method.md`
- primitive map: `mapping/primitive-map.md`
- name decisions: `mapping/csharp-name-decisions.md`
- typed DSL proof: `tests/Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.clear_grouping_method_clears_all_active_groups_and_refreshes_grid`
- command: `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.clear_grouping_method_clears_all_active_groups_and_refreshes_grid"`
- result: passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260607-020337.trx`
- accepted proof: visible `dataStateChange`, `skip=0`, `take=8`,
  `requiresCounts=true`, `action.requestType=ungrouping`,
  `action.name=actionBegin`, `action.type=actionBegin`,
  `action.columnName=wing`, and visible ungrouped summary
  `240 residents matched`.
- exclusion proof: request body omits `requiresCounts`, `group`, `groups`,
  `sorted`, `where`, `search`, `aggregates`, untyped `action`,
  `actionCancel`, and `actionPreventFocusOnGroup`; public contracts omit
  `FusionGridDataStateChangeArgs.Groups`,
  `FusionGridDataStateChangeArgs.Aggregates`, and
  `FusionGridAction.PreventFocusOnGroup`.
- behavior proof: typed `GroupBy` creates care-level and wing grouping first,
  typed `ClearGrouping()` removes all grouping, group caption count becomes
  `0`, and the first normal row contains `Amina Patel`.

## Proven Row

`recordClick` cell:

- raw cell trace: `traces/raw-ej2-record-click-cell.trace.json`
- judgment calls: `discovery/judgment-calls-record-click-cell.md`
- primitive map: `mapping/primitive-map.md`
- name decisions: `mapping/csharp-name-decisions.md`
- typed DSL proof: `tests/Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.record_click_reads_typed_row_data_and_cell_coordinates`
- command: `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.record_click_reads_typed_row_data_and_cell_coordinates"`
- result: passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-181927.trx`

## Proven Row

`rowSelected` click:

- raw click trace: `traces/raw-ej2-row-selected-click.trace.json`
- judgment calls: `discovery/judgment-calls-row-selected-click.md`
- primitive map: `mapping/primitive-map.md`
- name decisions: `mapping/csharp-name-decisions.md`
- typed DSL proof: `tests/Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.row_selected_reads_typed_row_data_previous_index_and_interaction_state`
- command: `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.row_selected_reads_typed_row_data_previous_index_and_interaction_state"`
- result: passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-182350.trx`

## Proven Row

`toolbarClick` custom item:

- raw custom trace: `traces/raw-ej2-toolbar-click-custom.trace.json`
- judgment calls: `discovery/judgment-calls-toolbar-click-custom.md`
- primitive map: `mapping/primitive-map.md`
- name decisions: `mapping/csharp-name-decisions.md`
- typed DSL proof: `tests/Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridBilling.clicking_email_statements_toolbar_item_runs_the_toolbar_workflow`
- command: `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridBilling.clicking_email_statements_toolbar_item_runs_the_toolbar_workflow"`
- result: passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-182943.trx`

## Proven Variant Row

`actionBegin` save/edit accepted fields plus all listed public-contract
exclusions:

- raw save/edit trace: `traces/raw-ej2-action-begin-save-edit.trace.json`
- judgment calls: `discovery/judgment-calls-action-begin-save-edit.md`
- primitive map: `mapping/primitive-map.md`
- name decisions: `mapping/csharp-name-decisions.md`
- typed DSL proof: `tests/Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridEditing.action_begin_save_edit_reads_typed_current_previous_and_action_fields`
- command: `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridEditing.action_begin_save_edit_reads_typed_current_previous_and_action_fields"`
- result: passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-223102.trx`

## Proven Variant Row

`actionComplete` save/edit accepted fields plus all listed public-contract
exclusions:

- raw save/edit trace: `traces/raw-ej2-action-complete-save-edit.trace.json`
- judgment calls: `discovery/judgment-calls-action-complete-save-edit.md`
- primitive map: `mapping/primitive-map.md`
- name decisions: `mapping/csharp-name-decisions.md`
- typed DSL proof: `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridActionCompleteSaveEdit.cs`
- command: `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridActionCompleteSaveEdit.action_complete_save_edit_reads_typed_current_previous_and_action_fields"`
- result: passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-221837.trx`
- explicit non-closure: broad `ActionComplete`, other `actionComplete`
  variants, and undocumented exclusions such as DOM/internal metadata remain
  open matrix rows.

## Proven Row

`cellSave` batch edit:

- raw trace: `traces/raw-ej2-cell-save-batch-edit.trace.json`
- judgment calls: `discovery/judgment-calls-cell-save-batch-edit.md`
- primitive map: `mapping/primitive-map.md`
- vertical slice plan: `mapping/vertical-slice-plan.md`
- typed DSL proof: `tests/Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridEditing.batch_editing_exposes_cell_events_indexed_batch_payload_and_batch_changes_source`
- command: `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridEditing.batch_editing_exposes_cell_events_indexed_batch_payload_and_batch_changes_source"`
- result: passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-235426.trx`
- accepted fields proven: `RowData`, `ColumnName`, `Value`,
  `PreviousValue`, `Cancel`, and `Cancel()`.
- excluded public-contract members proven absent: `Cell`, `Column`,
  `ColumnObject`, `IsForeignKey`, and `Name`.
- raw cancellation proof: the raw EJ2 trace records value `99` before
  cancellation, then records `cancel=true` after `args.cancel = true`, with
  `blockedValueAccepted=false` and batch changes still carrying `openTasks=6`.
- behavior proof: a valid batch edit changes `openTasks` from `0` to `4`,
  typed `UpdateCell` changes it to `6`, `BatchChanges()` gathers one changed
  row, `EndEdit` reaches `BeforeBatchSave`, and a second edit with value `99`
  calls `args.Cancel(t)` and leaves the visible Grid at `6` with no `99`.
- explicit non-closure: cell edit start, add-row batch editing, foreign-key
  columns, validation failure, keyboard navigation, frozen columns, and other
  edit modes remain open.

## Proven Row

`cellSaved` batch edit:

- raw trace: `traces/raw-ej2-cell-saved-batch-edit.trace.json`
- judgment calls: `discovery/judgment-calls-cell-saved-batch-edit.md`
- primitive map: `mapping/primitive-map.md`
- vertical slice plan: `mapping/vertical-slice-plan.md`
- typed DSL proof: `tests/Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridEditing.batch_editing_exposes_cell_events_indexed_batch_payload_and_batch_changes_source`
- command: `dotnet build tests/Alis.Reactive.PlaywrightTests/Alis.Reactive.PlaywrightTests.csproj -c Debug` then `scripts/playwright.sh --no-build --filter "FullyQualifiedName=Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridEditing.batch_editing_exposes_cell_events_indexed_batch_payload_and_batch_changes_source"`
- result: passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260607-023421.trx`
- accepted fields proven: `RowData`, `ColumnName`, `Value`, and
  `PreviousValue`.
- excluded public-contract members proven absent: `Cancel`, `Cell`, `Column`,
  `ColumnObject`, `IsForeignKey`, `Name`, and `Cancel()`.
- raw cancellation proof: the raw EJ2 trace records value `8`, then records
  `cancel=true` after `args.cancel = true`, but
  `cancelPreventedSavedValue=false`, first-row visible value is `8`, and batch
  changes carry `openTasks=8`.
- discovered source defect: `CellSaved` previously reused
  `FusionGridCellSaveArgs<TRow, TValue>`, which over-exposed `Cancel()` for a
  post-save event. This row corrects the contract by introducing
  `FusionGridCellSavedArgs<TRow, TValue>`.
- explicit non-closure: add-row batch editing, foreign-key columns, validation
  failure, keyboard navigation, frozen columns, and other edit modes remain
  open.

## Proven Row

`beforeBatchSave` batch edit:

- raw trace: `traces/raw-ej2-before-batch-save-batch-edit.trace.json`
- judgment calls: `discovery/judgment-calls-before-batch-save-batch-edit.md`
- primitive map: `mapping/primitive-map.md`
- vertical slice plan: `mapping/vertical-slice-plan.md`
- typed DSL proof: `tests/Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridEditing.batch_editing_exposes_cell_events_indexed_batch_payload_and_batch_changes_source`
- command: `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridEditing.batch_editing_exposes_cell_events_indexed_batch_payload_and_batch_changes_source"`
- result: passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260607-004231.trx`
- accepted fields proven: `BatchChanges`, `Cancel`, and `Cancel()`.
- excluded public-contract member proven absent: `Name`.
- raw cancellation proof: the raw EJ2 trace records `batchChanges` with
  `openTasks=8`, then records `cancel=true` after `args.cancel = true`; after
  the cancelled end-edit path, `actionCompleteCount` remains `1`, unsaved
  batch changes still carry `openTasks=8`, and the backing `dataSource` remains
  at the previously committed `openTasks=6`.
- explicit non-closure: add-row, delete-row, multiple changed rows, validation
  failure, toolbar click commit, keyboard commit, and other edit modes remain
  open.

## Proven Row

remote whole-response `{ result, count }`:

- raw trace: `traces/raw-ej2-remote-response-shape.trace.json`
- judgment calls: `discovery/judgment-calls-remote-response-shape.md`
- primitive map: `mapping/primitive-map.md`
- vertical slice plan: `mapping/vertical-slice-plan.md`
- typed DSL proof: `tests/Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGrid.sorting_a_column_fetches_sorted_data_and_echoes_action`
- command: `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGrid.sorting_a_column_fetches_sorted_data_and_echoes_action"`
- result: passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-203627.trx`
- primitive proof: generated plan contains `member=responseBody` with empty path
  for the whole response-body overload and no response-body member-path read for
  this Grid page.
- explicit non-closure: response-path `SetDataSource`, event-payload path
  `SetDataSource`, DataManager/adaptor, builder-owned initial dataSource, and
  nested data-source path rows remain open.

## Proven Row

data-source typed-array read/rebind/refresh:

- raw trace: `traces/raw-ej2-data-source-read-refresh.trace.json`
- judgment calls: `discovery/judgment-calls-data-source-read-refresh.md`
- primitive map: `mapping/primitive-map.md`
- vertical slice plan: `mapping/vertical-slice-plan.md`
- typed DSL proof: `tests/Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenBindingArrayToGrid`
- command: `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenBindingArrayToGrid"`
- result: passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-201013.trx`
- strengthened assertions: deliberately unsorted HTTP fixture proves the typed
  `OrderBy` path, and request counting from `Show Active Only` through the
  visible filtered Grid proof proves no second roster fetch is used for the
  client-side rebind workflow.
- explicit non-closure: response-path `SetDataSource`, event-payload path
  `SetDataSource`, DataManager/adaptor, builder-owned initial dataSource, and
  nested data-source path rows remain open.

## Required Closeout

This audit can close only after the generated coverage matrix no longer contains open markers for accepted Grid APIs and every accepted row has typed DSL behavior proof.
