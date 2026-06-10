# Grid Playwright Proof

Status: partial. The `dataStateChange` sorting row, typed `SortBy` method row,
page-number paging row,
method-trigger filtering row, method-trigger clear-filtering row, method-trigger searching row, method-trigger
clear-search row, method-trigger clear-sorting row, method-trigger grouping row, method-trigger ungrouping row, method-trigger clear-grouping row, `recordClick` cell row, and
`rowSelected` click row have focused typed DSL Playwright proof. The
`toolbarClick` custom item row also has focused
typed DSL Playwright proof. The `actionBegin` save/edit variant row also has
focused typed DSL Playwright proof. The `actionComplete` save/edit variant row
also has focused typed DSL Playwright proof through its own vertical-slice view.
The `beginEdit` normal edit variant row also has focused typed DSL Playwright
proof through its own vertical-slice view. The `cellSave` batch-edit variant
row and `cellSaved` batch-edit variant row also have focused typed DSL
Playwright proof. The `beforeBatchSave` batch-edit row also has focused typed
DSL Playwright proof. The remote whole-response
`{ result, count }` row and the data-source typed-array read/rebind/refresh row
also have focused typed DSL Playwright proof. The Grid component audit remains
fail-closed because the full typed API coverage matrix is open.

## Required First Proof Row

`dataStateChange` sorting must be proven through a real Fusion Grid DSL page and `scripts/playwright.sh`.

Raw EJ2 traces do not count as this proof. They only prove vendor discovery.

## Proof Criteria

- trigger sorting through visible Grid behavior or a typed DSL command;
- prove visible row/data refresh;
- prove request body values gathered from `args.Skip`, `args.Take`, `args.Sorted`, and every accepted action field;
- prove newly accepted fields such as `RequiresCounts` if implemented;
- prove no console errors;
- link the Playwright test and command output here.

## Completed Proof Rows

| Row | Test | Command | Result |
| --- | --- | --- | --- |
| `dataStateChange` sorting | `Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGrid.sorting_a_column_fetches_sorted_data_and_echoes_action` | `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGrid.sorting_a_column_fetches_sorted_data_and_echoes_action"` | passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-231608.trx` |
| `FusionGrid.SortBy` method | `Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.sort_by_method_sends_typed_sorted_payload_and_refreshes_grid` | `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.sort_by_method_sends_typed_sorted_payload_and_refreshes_grid"` | passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260607-024311.trx` |
| `dataStateChange` page-number paging | `Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGrid.paging_fetches_next_page_with_correct_skip` | `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGrid.paging_fetches_next_page_with_correct_skip"` | passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-232421.trx` |
| `dataStateChange` filtering method | `Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.filtering_method_sends_typed_where_payload_and_refreshes_grid` | `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.filtering_method_sends_typed_where_payload_and_refreshes_grid"` | passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-205951.trx` |
| `dataStateChange` filtering FilterBar typing | `Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.filtering_filterbar_typing_sends_typed_where_payload_and_refreshes_grid` | `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.filtering_filterbar_typing_sends_typed_where_payload_and_refreshes_grid"` | passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-213206.trx` |
| `dataStateChange` clear-filtering method | `Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.clear_filtering_method_clears_active_filter_and_refreshes_grid` | `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.clear_filtering_method_clears_active_filter_and_refreshes_grid"` | passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260607-014720.trx` |
| `dataStateChange` searching method | `Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.searching_method_sends_typed_search_payload_and_refreshes_grid` | `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.searching_method_sends_typed_search_payload_and_refreshes_grid"` | passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-233449.trx` |
| `dataStateChange` clear-search method | `Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.clear_search_method_clears_active_search_and_refreshes_grid` | `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.clear_search_method_clears_active_search_and_refreshes_grid"` | passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260607-013501.trx` |
| `dataStateChange` clear-sorting method | `Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.clear_sorting_method_clears_active_sort_and_refreshes_grid` | `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.clear_sorting_method_clears_active_sort_and_refreshes_grid"` | passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260607-023440.trx` |
| `dataStateChange` grouping method | `Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.grouping_method_sends_typed_group_payload_and_refreshes_grid` | `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.grouping_method_sends_typed_group_payload_and_refreshes_grid"` | passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-234123.trx` |
| `dataStateChange` ungrouping method | `Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.ungrouping_method_sends_typed_action_payload_and_refreshes_grid` | `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.ungrouping_method_sends_typed_action_payload_and_refreshes_grid"` | passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260607-011802.trx` |
| `dataStateChange` clear-grouping method | `Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.clear_grouping_method_clears_all_active_groups_and_refreshes_grid` | `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.clear_grouping_method_clears_all_active_groups_and_refreshes_grid"` | passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260607-020337.trx` |
| `recordClick` cell | `Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.record_click_reads_typed_row_data_and_cell_coordinates` | `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.record_click_reads_typed_row_data_and_cell_coordinates"` | passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-181927.trx` |
| `rowSelected` click | `Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.row_selected_reads_typed_row_data_previous_index_and_interaction_state` | `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.row_selected_reads_typed_row_data_previous_index_and_interaction_state"` | passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-182350.trx` |
| `toolbarClick` custom item | `Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridBilling.clicking_email_statements_toolbar_item_runs_the_toolbar_workflow` | `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridBilling.clicking_email_statements_toolbar_item_runs_the_toolbar_workflow"` | passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-182943.trx` |
| `actionBegin` save/edit | `Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridEditing.action_begin_save_edit_reads_typed_current_previous_and_action_fields` | `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridEditing.action_begin_save_edit_reads_typed_current_previous_and_action_fields"` | passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-223102.trx` |
| `actionComplete` save/edit | `Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridActionCompleteSaveEdit.action_complete_save_edit_reads_typed_current_previous_and_action_fields` | `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridActionCompleteSaveEdit.action_complete_save_edit_reads_typed_current_previous_and_action_fields"` | passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-221837.trx` |
| `beginEdit` normal edit | `Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridBeginEditNormal.begin_edit_normal_reads_row_data_and_can_cancel_edit` | `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridBeginEditNormal.begin_edit_normal_reads_row_data_and_can_cancel_edit"` | passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-225913.trx` |
| `beforeBatchSave` batch edit | `Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridEditing.batch_editing_exposes_cell_events_indexed_batch_payload_and_batch_changes_source` | `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridEditing.batch_editing_exposes_cell_events_indexed_batch_payload_and_batch_changes_source"` | passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260607-004231.trx` |
| `cellSave` batch edit | `Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridEditing.batch_editing_exposes_cell_events_indexed_batch_payload_and_batch_changes_source` | `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridEditing.batch_editing_exposes_cell_events_indexed_batch_payload_and_batch_changes_source"` | passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-235426.trx` |
| `cellSaved` batch edit | `Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridEditing.batch_editing_exposes_cell_events_indexed_batch_payload_and_batch_changes_source` | `dotnet build tests/Alis.Reactive.PlaywrightTests/Alis.Reactive.PlaywrightTests.csproj -c Debug` then `scripts/playwright.sh --no-build --filter "FullyQualifiedName=Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridEditing.batch_editing_exposes_cell_events_indexed_batch_payload_and_batch_changes_source"` | passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260607-023421.trx` |
| remote whole-response `{ result, count }` | `Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGrid.sorting_a_column_fetches_sorted_data_and_echoes_action` | `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGrid.sorting_a_column_fetches_sorted_data_and_echoes_action"` | passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-203627.trx` |
| data-source typed-array read/rebind/refresh | `Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenBindingArrayToGrid` | `scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenBindingArrayToGrid"` | passed, TRX `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-201013.trx` |

The test proves:

- visible sorting action status changes to `data refreshed`;
- visible `action.requestType` is `sorting`;
- visible event `name` is `dataStateChange`;
- visible `requiresCounts` is `true`;
- visible `action.name` is `actionBegin`;
- visible `action.type` is `actionBegin`;
- visible `action.cancel` is `false`;
- visible `action.columnName` is `name`;
- visible `action.direction` is `Ascending`;
- request body contains `eventName`, `requiresCounts`, `actionName`, `actionType`, `actionCancel`, and the whole `sorted` array with `{ name, direction }`.
- request body omits sorting-foreign `where`, `search`, and `group` payloads;
- the public typed `FusionGridAction` contract does not expose excluded
  `Target`.

The remote whole-response test also proves:

- the sorting request receives HTTP 200;
- the response parses as JSON with own keys `result` and `count`;
- `result` contains ten returned rows;
- `count` is `200`;
- the first returned row name is rendered as the first visible Grid row after
  `SetDataSource(json)`;
- the Grid pager displays `200 items`, proving EJ2 consumes `count`;
- the generated plan contains `member=responseBody` with an empty path and no
  response-body member-path read for this page.

SortBy method proof:

- typed `SortBy((ResidentDirectoryGridItem x) => x.RiskLevel, Descending)`
  calls Syncfusion `sortColumn` through the public C# method row;
- request body parses as JSON with `skip=0` and `take=8`;
- `sorted[0]` contains `name=riskLevel` and `direction=descending`;
- request body omits `where`, `search`, `group`, `aggregates`, untyped
  `action`, and `actionTarget`;
- visible method status is `sortColumn called`;
- visible event `name` is `dataStateChange`;
- visible `skip` is `0`;
- visible `take` is `8`;
- visible `requiresCounts` is `true`;
- visible `action.requestType` is `sorting`;
- visible `action.name` is `actionBegin`;
- visible `action.type` is `actionBegin`;
- visible `action.cancel` is `false`;
- visible `action.columnName` is `riskLevel`;
- visible `action.direction` is `Descending`;
- visible summary is `240 residents matched`;
- first visible row contains `Grace Bennett` and the risk cell is `Moderate`;
- no console errors are emitted.

The paging test proves:

- visible data refresh after pager click;
- visible `skip` is `10`;
- visible `take` is `10`;
- visible `requiresCounts` is `true`;
- visible `action.requestType` is `paging`;
- visible `action.currentPage` is `2`;
- visible `action.previousPage` is `1`;
- visible `action.pageSize` is `10`;
- request body contains `eventName`, `skip`, `take`, `requiresCounts`, `actionName`, `actionType`, `actionCancel`, `actionRequestType`, `actionCurrentPage`, `actionPreviousPage`, and `actionPageSize`.
- request body omits paging-foreign `where`, `search`, `group`, and `sorted`
  payloads;
- request body omits page-size-change/browser-owned `actionPreviousPageSize`,
  `actionRows`, and `actionTarget`;
- the public typed `FusionGridAction` contract does not expose excluded
  `PreviousPageSize`, `Rows`, or `Target`.

The filtering-method test proves:

- typed `FilterTextBy` triggers `dataStateChange`;
- request body parses as JSON with a `where` array;
- visible event `name` is `dataStateChange`;
- visible `skip` is `0`;
- visible `take` is `8`;
- visible `requiresCounts` is `true`;
- visible `action.requestType` is `filtering`;
- visible `action.name` is `actionBegin`;
- visible `action.type` is `actionBegin`;
- visible `action.cancel` is `false`;
- `where[0].condition` is `and`;
- `where[0].ignoreCase` is `true`;
- `where[0].ignoreAccent` is `false`;
- `where[0].predicates[0]` contains `field=wing`, `operator=equal`, and `value=North`;
- the emitted typed request omits the raw `action` settings object and the
  variant-foreign `search`, `group`, and `sorted` payloads;
- `isComplex` is present as the accepted typed predicate discriminator:
  `true` on the composite node and `false` on the leaf predicate;
- the sandbox server checks `FusionGridTextFilterCriterion.IsComplex` before
  flattening nested predicates, so visible filtering proves C# model binding
  preserved the discriminator;
- stale `matchCase` and `predicate` fields are absent from the emitted typed
  `where` request payload;
- visible `grid-action` is `filtering`;
- visible summary is `60 residents matched`;
- every visible Grid row has Wing `North`.

FilterBar typing proof:

- trigger uses the real Grid FilterBar input `#wing_filterBarcell`;
- proof types `N` and presses Enter because the sandbox FilterBar commits on
  Enter, while the raw probe used Immediate mode to make tracing deterministic;
- the request wait predicate is payload-scoped and only captures a same-endpoint
  POST when the body already contains `field=wing`, `operator=startswith`,
  `value=N`, composite `isComplex=true`, and leaf `isComplex=false`;
- request body omits raw `action`, declared-foreign `aggregates`,
  `dataSource`, `search`, `group`, `isLazyLoad`, `onDemandGroupInfo`,
  `select`, `sorted`, and `table`;
- `where[0].condition` is `and`;
- `where[0].ignoreCase` is `true`;
- `where[0].ignoreAccent` is `false`;
- `where[0].predicates[0]` contains `field=wing`, `operator=startswith`, and
  `value=N`;
- `isComplex` is present as the accepted typed predicate discriminator:
  `true` on the composite node and `false` on the leaf predicate;
- stale `matchCase` and `predicate` fields are absent from the emitted typed
  `where` request payload;
- visible summary is `60 residents matched`;
- every visible Grid row has Wing `North`.

Clear-filtering method proof:

- typed `ClearFiltering()` starts from an active `FilterTextBy(...Wing == "North")` state;
- typed `ClearFiltering()` triggers `dataStateChange` with visible `action.requestType=refresh`;
- request body parses as JSON with `skip=0` and `take=8`;
- `where` is absent from the clear request body, not an empty array;
- request body omits `search`, `group`, `sorted`, untyped `action`,
  `actionAction`, `actionColumns`, `actionCurrentFilterObject`, and
  `actionCurrentFilteringColumn`;
- visible event `name` is `dataStateChange`;
- visible `skip` is `0`;
- visible `take` is `8`;
- visible `requiresCounts` is `true`;
- visible `action.name` is `actionBegin`;
- visible method status is `filters cleared`;
- visible summary is `240 residents matched`;
- first visible row contains `Amina Patel`;
- no console errors are emitted.

The searching-method test proves:

- typed `Search` triggers `dataStateChange`;
- request body parses as JSON with a `search` array;
- visible event `name` is `dataStateChange`;
- visible `skip` is `0`;
- visible `take` is `8`;
- visible `requiresCounts` is `true`;
- visible `action.requestType` is `searching`;
- visible `action.name` is `actionBegin`;
- visible `action.type` is `actionBegin`;
- `search[0]` contains `key=Memory`, `operator=contains`, `ignoreCase=true`, and `ignoreAccent=false`;
- `search[0].fields` contains `residentName`, `careLevel`, and `wing`;
- duplicate descriptor-level `searchString` is absent from the typed `Search`
  descriptor;
- request body omits duplicate `actionSearchString`, absent `actionCancel`, and
  searching-foreign `where`, `group`, and `sorted`;
- the public typed `FusionGridAction` contract does not expose excluded
  `SearchString`;
- visible `grid-action` is `searching`;
- visible summary is `60 residents matched`;
- every visible Grid row has Care Level `Memory Care`.

ClearSearch method proof:

- typed `ClearSearch()` starts from an active `Search("Memory")` state;
- typed `ClearSearch()` triggers `dataStateChange`;
- request body parses as JSON with `skip=0` and `take=8`;
- `search` is absent from the clear request body, not an empty array;
- request body omits `where`, `group`, `sorted`, untyped `action`,
  `actionSearchString`, and `actionCancel`;
- `action.searchString` remains excluded from public C#;
- visible event `name` is `dataStateChange`;
- visible `skip` is `0`;
- visible `take` is `8`;
- visible `requiresCounts` is `true`;
- visible `action.requestType` is `searching`;
- visible `action.name` is `actionBegin`;
- visible `action.type` is `actionBegin`;
- visible method status is `search cleared`;
- visible summary is `240 residents matched`;
- first visible row contains `Amina Patel`;
- no console errors are emitted.

ClearSorting method proof:

- typed `ClearSorting()` starts from an active `SortBy` state;
- typed `SortBy((ResidentDirectoryGridItem x) => x.RiskLevel, Descending)`
  first proves a visible sorted state: first row contains `Grace Bennett` and
  the risk cell is `Moderate`;
- typed `ClearSorting()` triggers `dataStateChange`;
- request body parses as JSON with `skip=0` and `take=8`;
- `sorted` is absent from the clear request body, not an empty array;
- request body omits `where`, `search`, `group`, `aggregates`, untyped
  `action`, `actionColumnName`, `actionDirection`, `actionCancel`, and
  `actionTarget`;
- `action.requestType` is `sorting`;
- visible event `name` is `dataStateChange`;
- visible `skip` is `0`;
- visible `take` is `8`;
- visible `requiresCounts` is `true`;
- visible `action.name` is `actionBegin`;
- visible `action.type` is `actionBegin`;
- `action.columnName`, `action.direction`, and `action.target` remain excluded for this clear row;
- visible `grid-column`, `grid-direction`, and `grid-action-cancel` do not keep
  the previous sort row values after clear;
- visible method status is `sorting cleared`;
- visible summary is `240 residents matched`;
- first visible row contains `Amina Patel`;
- first visible risk cell is `Low`;
- no console errors are emitted.

The grouping-method test proves:

- typed `GroupBy` triggers `dataStateChange`;
- request body parses as JSON with a `group` array;
- visible event `name` is `dataStateChange`;
- visible `skip` is `0`;
- visible `take` is `8`;
- visible `requiresCounts` is `true`;
- visible `action.requestType` is `grouping`;
- visible `action.name` is `actionBegin`;
- visible `action.type` is `actionBegin`;
- `group[0]` is `careLevel`;
- duplicate `groups`, internal `actionPreventFocusOnGroup`, absent
  `actionCancel`, grouping-foreign `where` and `search`, no-aggregate
  `aggregates`, and an untyped `action` object are absent from the typed
  request;
- public typed contracts omit `FusionGridDataStateChangeArgs.Groups` and
  `FusionGridAction.PreventFocusOnGroup`;
- grouping auto-sort state is gathered as `sorted[0]` with `name=careLevel` and `direction=ascending`;
- visible `grid-action` is `grouping`;
- visible `grid-column` is `careLevel`;
- visible summary is `240 residents grouped by care level`;
- rendered Grid captions include `Care Level: Assisted Living` and `Care Level: Memory Care`.

Ungrouping method proof:

- typed `GroupBy` first triggers grouping so the page has visible group captions;
- typed `UngroupBy` triggers `dataStateChange`;
- request body parses as JSON with `skip=0` and `take=8`;
- `requiresCounts` is read from the typed event payload but is not gathered into the Directory request body;
- `group` is absent rather than an empty array;
- request body omits duplicate/internal or variant-foreign `groups`, `sorted`,
  `where`, `search`, `aggregates`, untyped `action`, `actionCancel`, and
  `actionPreventFocusOnGroup`;
- public typed contracts omit `FusionGridDataStateChangeArgs.Groups`,
  `FusionGridDataStateChangeArgs.Aggregates`, and
  `FusionGridAction.PreventFocusOnGroup`;
- visible event `name` is `dataStateChange`;
- visible `skip` is `0`;
- visible `take` is `8`;
- visible `requiresCounts` is `true`;
- `action.requestType` is `ungrouping`;
- visible `action.name` is `actionBegin`;
- visible `action.type` is `actionBegin`;
- visible `action.columnName` is `careLevel`;
- visible summary is `240 residents matched`;
- group caption count is `0`;
- the first normal Grid row contains `Amina Patel`;
- no console errors are emitted.

ClearGrouping method proof:

- typed `ClearGrouping()` starts from active `GroupBy` calls for care level and wing;
- typed `ClearGrouping()` triggers `dataStateChange`;
- request body parses as JSON with `skip=0` and `take=8`;
- `requiresCounts` is read from the typed event payload but is not gathered into the Directory request body;
- `group` is absent from the clear request body, not an empty array;
- request body omits duplicate/internal or variant-foreign `groups`, `sorted`,
  `where`, `search`, `aggregates`, untyped `action`, `actionCancel`, and
  `actionPreventFocusOnGroup`;
- public typed contracts omit `FusionGridDataStateChangeArgs.Groups`,
  `FusionGridDataStateChangeArgs.Aggregates`, and
  `FusionGridAction.PreventFocusOnGroup`;
- visible event `name` is `dataStateChange`;
- visible `skip` is `0`;
- visible `take` is `8`;
- visible `requiresCounts` is `true`;
- `action.requestType` is `ungrouping`;
- visible `action.name` is `actionBegin`;
- visible `action.type` is `actionBegin`;
- `action.columnName` is the final active group column `wing`;
- visible summary is `240 residents matched`;
- group caption count is `0`;
- the first normal Grid row contains `Amina Patel`;
- no console errors are emitted.

The record-click cell test proves:

- a real rendered Grid data cell is clicked;
- the expected resident value is read from the visible cell that was clicked;
- visible `clicked-resident` comes from typed `args.RowData.ResidentName`;
- visible `clicked-row` is `0`;
- visible `clicked-cell` is `1`;
- visible `clicked-event` is `recordClick`;
- no console errors are emitted.

The row-selected click test proves:

- two real rendered Grid rows are clicked so the previous-selection variant is exercised;
- request body contains `residentId` from typed `args.Data.ResidentId`;
- request body contains `rowIndex=1` from typed `args.RowIndex`;
- visible `selected-resident-local` comes from typed `args.Data.ResidentName`;
- visible `selected-row-index` is `1`;
- visible nullable `selected-previous-row-index` is `0`;
- visible `selected-interacted` is `true`;
- visible `selected-event` is `rowSelected`;
- async response updates selected resident and selection summary;
- no console errors are emitted.

The toolbar-click custom item test proves:

- a real custom Grid toolbar item is clicked;
- the workflow branches on typed `args.Item.Id`;
- visible `toolbar-item-id` comes from typed `args.Item.Id`;
- visible `toolbar-item-text` comes from typed `args.Item.Text`;
- visible `toolbar-cancel` comes from typed `args.Cancel`;
- visible `toolbar-event` comes from typed `args.Name`;
- no console errors are emitted.

The action-begin save/edit test proves:

- a real Grid row is selected and edited;
- save is triggered through the typed `EndEdit` command;
- visible `inline-action-begin` is `save`;
- visible `inline-action-begin-action` is `edit`;
- visible `inline-action-begin-type` is `actionBegin`;
- visible `inline-action-begin-event` is `actionBegin`;
- visible `inline-action-begin-cancel` is `false`;
- visible `inline-action-begin-row` is `0`;
- visible `inline-action-begin-selected-row` is `-1`;
- visible current row data is the edited resident name;
- visible previous row data is the original resident name;
- the rendered Grid row contains the edited value;
- the public typed args contract does not expose removed/excluded `Row`,
  `Form`, `Target`, `ForeignKeyData`, `IsScroll`, `PrimaryKey`,
  `PrimaryKeyValue`, `RowData`, or `Index` members;
- no console errors are emitted.

The beginEdit normal edit test proves:

- a focused vertical-slice page loads real Grid data through the typed DSL;
- a real Grid row is selected and normal edit is started through the typed
  `StartEdit` command;
- visible `begin-edit-resident` comes from typed `args.RowData.ResidentName`;
- visible `begin-edit-row` is `0` for the normal row;
- visible `begin-edit-type` is `edit`;
- visible `begin-edit-cancel` is `false`;
- the edited row is visibly present after normal start edit;
- a second realistic locked-row path selects row `1` and calls `args.Cancel(t)`
  from a typed `BeginEdit<TRow>` condition;
- visible `begin-edit-cancelled` is `edit cancelled`;
- after the locked-row path, the Grid has zero `.e-editedrow` elements,
  proving the event-payload cancel mutation prevented edit mode;
- the public typed args contract does not expose excluded `Row`,
  `ForeignKeyData`, `IsScroll`, `Name`, `PrimaryKey`, `PrimaryKeyValue`,
  `RequestType`, or `Target` members;
- no console errors are emitted.

The cellSave batch-edit test proves:

- a real Fusion Grid page loads resident editing data through the typed DSL;
- batch cell edit starts through the typed `EditCell` command for
  `ResidentDirectoryGridItem.OpenTasks`;
- cell save runs through the typed `SaveCell` command;
- visible `batch-cell-save-column` is `openTasks`;
- visible `batch-cell-save-value` is `4` for the first save;
- visible `batch-cell-save-previous` is `0` for the first resident's original
  open-task count;
- visible `batch-cell-save-resident` is `Amina Patel` from typed
  `args.RowData.ResidentName`;
- visible `batch-cell-save-cancel` is `false`;
- visible `batch-cell-saved-value` is `4`, proving the non-canceled save
  continues through Syncfusion;
- typed `UpdateCell` changes the same cell to `6`;
- typed `BatchChanges()` is gathered into an HTTP request and the server
  returns `batch added 0, changed 1, deleted 0`;
- typed `EndEdit` fires `BeforeBatchSave` and visible
  `batch-before-save-tasks` is `6`;
- a second edit sets value `99`, the typed `CellSave` condition calls
  `args.Cancel(t)`, and visible `batch-cell-save-cancelled` is `blocked 99`;
- the Grid still contains `6` and does not contain `99`, proving the
  event-payload cancel mutation prevented the blocked cell value from being
  accepted;
- the public typed args contract does not expose excluded `Cell`, `Column`,
  `ColumnObject`, `IsForeignKey`, or `Name` members;
- no console errors are emitted.

The `cellSaved` batch-edit test proves:

- a real Fusion Grid page loads resident editing data through the typed DSL;
- batch cell edit starts through the typed `EditCell` command for
  `ResidentDirectoryGridItem.OpenTasks`;
- cell save runs through the typed `SaveCell` command;
- visible `batch-cell-saved-column` is `openTasks`;
- visible `batch-cell-saved-value` is `4`;
- visible `batch-cell-saved-previous` is `0`;
- visible `batch-cell-saved-resident` is `Amina Patel` from typed
  `args.RowData.ResidentName`;
- the public typed args contract does not expose excluded `Cancel`, `Cell`,
  `Column`, `ColumnObject`, `IsForeignKey`, or `Name` members;
- `FusionGridCellSavedArgs` has no public `Cancel()` extension, because raw
  EJ2 proves post-save `args.cancel = true` does not prevent the saved value;
- no console errors are emitted.

The `beforeBatchSave` batch-edit test proves:

- a real Fusion Grid page loads resident editing data through the typed DSL;
- batch cell edit starts through the typed `EditCell` command for
  `ResidentDirectoryGridItem.OpenTasks`;
- cell save runs through the typed `SaveCell` command;
- typed `EndEdit` fires `BeforeBatchSave` before the bulk-save commit;
- visible `batch-before-save-resident` is `Amina Patel` from
  `args.BatchChanges.ChangedRecords[0].ResidentName`;
- visible `batch-before-save-tasks` is `8` for the cancelled second commit path;
- visible `batch-before-save-cancel` is `false` before mutation;
- the typed `BeforeBatchSave` condition calls `args.Cancel(t)` for value `8`;
- visible `batch-before-save-cancelled` is `blocked batch 8`;
- visible `batch-action-complete` remains `waiting after cancelled batch`,
  immediately after `EndEdit` and again after the later `BatchChanges()` gather,
  proving the cancelled commit did not reach a delayed `actionComplete`;
- gathering `BatchChanges()` after the cancelled commit still reports one
  changed row;
- the public typed args contract does not expose excluded `Name`;
- no console errors are emitted.

The data-source typed-array tests prove:

- initial HTTP response data is read from `json.Residents`;
- the server fixture is deliberately not name-sorted;
- the typed array DSL sorts the roster by `Name`, proven by the visible first
  row after load;
- `SetDataSource(TypedSource<T[]>)` binds five visible Grid rows;
- clicking `Show Active Only` runs a client-side workflow;
- the test counts `/Sandbox/Components/ArrayGrid/Residents` requests after the
  click until the filtered Grid is visibly proven and asserts zero, proving no
  second roster fetch is used for that workflow;
- `Data()` reads the current Grid `dataSource` as a typed array source;
- the typed array DSL filters the current rows to `Status == "active"` and
  orders them by name;
- `SetDataSource(TypedSource<T[]>)` writes the filtered array back;
- `Refresh()` applies the post-render rebind visibly;
- visible Grid rows drop from five to three;
- discharged and critical rows are absent;
- no console errors are emitted.

## Column Visibility And Reorder Methods Proof

Focused scenario view `/Sandbox/Components/Grid/CareStaffColumns` (column
management only, P017). Raw EJ2 evidence:
`traces/raw-ej2-column-visibility.trace.json`. Fresh-build Playwright
`WhenUsingFusionGridColumns` (3/3 passed, `playwright-20260607-055541.trx`).
Assertions read the rendered visible header cells, not a status text.

### HideColumn proof

Typed `HideColumn((ResidentDirectoryGridItem x) => x.PrimaryNurse)` and
`HideColumn(... x.NextReviewDate)` lower to `grid.hideColumns(field, "field")`.
Test `hiding_care_columns_removes_their_headers` clicks Hide and asserts the
visible headers drop to `Resident, Risk, Wing, Care Level`.

### ShowColumn proof

Typed `ShowColumn((ResidentDirectoryGridItem x) => x.PrimaryNurse)` and
`ShowColumn(... x.NextReviewDate)` lower to `grid.showColumns(field, "field")`.
Test `showing_care_columns_restores_their_headers` hides then shows and asserts the
visible headers return to `Resident, Risk, Primary Nurse, Next Review, Wing, Care
Level`.

### ReorderColumnBefore proof

Typed `ReorderColumnBefore((x) => x.RiskLevel, (x) => x.ResidentName)` lowers to
`grid.reorderColumns(from, before)` and requires `.AllowReordering(true)` (the raw
trace proved it is a silent no-op without the Reorder module). Test
`reordering_moves_risk_before_resident` asserts the visible header order becomes
`Risk, Resident, Primary Nurse, Next Review, Wing, Care Level`.

## Batch Cell Edit Methods Proof

Focused scenario view `/Sandbox/Components/Grid/BatchTaskUpdate` (batch cell edit
only, P017). Raw EJ2 evidence:
`traces/raw-ej2-cell-save-batch-edit.trace.json`. Fresh-build Playwright
`WhenUsingFusionGridBatchEdit`. EJ2 batch semantics: `EditCell`/`SaveCell` are an
open/commit pair; `UpdateCell` sets a cell value directly.

### EditCell proof

Typed `EditCell(0, (ResidentDirectoryGridItem x) => x.OpenTasks)` lowers to
`grid.editCell(0, "openTasks")`. Clicking Edit opens the cell editor (an input
appears in the grid body).

### SaveCell proof

Typed `SaveCell()` lowers to `grid.saveCell()`. Clicking Save commits the open
editor and closes it (the cell editor input is removed).

### UpdateCell proof

Typed `UpdateCell(0, (ResidentDirectoryGridItem x) => x.OpenTasks, 6)` (int
overload) lowers to `grid.updateCell(0, "openTasks", 6)`. Clicking Set marks the
cell with EJ2's `.e-updatedtd` batch-change class and shows the new value `6`. The
string `UpdateCell` overload remains its own open row (P008).

## Inline Edit Lifecycle Methods Proof

Focused scenario view `/Sandbox/Components/Grid/InlineEdit` (inline edit
open/cancel/commit only, P017). Raw EJ2 evidence:
`traces/raw-ej2-begin-edit-normal.trace.json`. Fresh-build Playwright
`WhenUsingFusionGridInlineEdit`. Inline `EditMode.Normal`.

### StartEdit proof

Typed `StartEdit()` lowers to `grid.startEdit()`. After selecting row 0, clicking
Edit opens the inline row editor (an input appears in the grid body).

### CloseEdit proof

Typed `CloseEdit()` lowers to `grid.closeEdit()`. Clicking Cancel closes the inline
editor (the editor inputs are removed).

### EndEdit proof

Typed `EndEdit()` lowers to `grid.endEdit()`. Clicking Save commits the inline edit
and closes the editor (the editor inputs are removed).

## Roster CRUD Methods Proof (client overloads)

Focused scenario view `/Sandbox/Components/Grid/RosterCrud` (add/update/delete
only, P017). Raw EJ2 evidence:
`traces/raw-ej2-begin-edit-normal.trace.json`. Fresh-build Playwright
`WhenUsingFusionGridRosterCrud`. The server-backed `AddRecord`/`UpdateRow`
overloads (`ResponseBody<TResponse>`) are proven by their own server-roster slice
(see Server-backed roster proof).

### AddRecord proof

Typed `AddRecord(new ResidentDirectoryGridItem { ... }, 0)` (client `TRow` overload)
lowers to `grid.addRecord(data, 0)`. Clicking Add inserts a new resident row
(`Zara Inline` becomes visible at the top).

### UpdateRow proof

Typed `UpdateRow(0, new ResidentDirectoryGridItem { ... })` (client `TRow` overload)
lowers to `grid.updateRow(0, data)`. Clicking Replace swaps the top row data
(`Amina Updated` becomes visible, `Zara Inline` is gone).

### DeleteSelectedRecord proof

Typed `DeleteSelectedRecord()` lowers to `grid.deleteRecord()`. After selecting the
top row, clicking Delete removes it (`Amina Updated` is gone).

## Selection Methods Proof

Focused scenario view `/Sandbox/Components/Grid/RosterSelection` (selection only,
P017). Raw EJ2 evidence:
`traces/raw-ej2-row-selected-click.trace.json`. Fresh-build Playwright
`WhenUsingFusionGridSelection`. Assertions count rows with `aria-selected="true"`.

### SelectRowsByRange proof

Typed `SelectRowsByRange(1, 3)` lowers to `grid.selectRowsByRange(1, 3)`. Clicking
Select Range selects three resident rows.

### ClearSelection proof

Typed `ClearSelection()` lowers to `grid.clearSelection()`. Clicking Clear removes
all selection (zero selected rows).

### SelectRow proof

Typed `SelectRow(0)` lowers to `grid.selectRow(0)`. Clicking Select First selects a
single resident row.

## Keyed Update Methods Proof (client overloads)

Focused scenario view `/Sandbox/Components/Grid/KeyedUpdate` (primary-key update
only, P017). Raw EJ2 evidence:
`traces/raw-ej2-begin-edit-normal.trace.json`. Fresh-build Playwright
`WhenUsingFusionGridKeyedUpdate`. Updates target a resident by primary key, not
row index. The server-backed `SetRowData(ResponseBody<TResponse>)` overload is
proven by the server-roster slice (see Server-backed roster proof).

### SetCellValue int proof

Typed `SetCellValue(6000, (ResidentDirectoryGridItem x) => x.OpenTasks, 99)` (int
overload) lowers to `grid.setCellValue(6000, "openTasks", 99)`. Clicking Flag shows
`99` in the grid.

### SetCellValue string proof

Typed `SetCellValue(6001, (ResidentDirectoryGridItem x) => x.RiskLevel,
"Quarantine")` (string overload) lowers to
`grid.setCellValue(6001, "riskLevel", "Quarantine")`. Clicking Quarantine shows
`Quarantine` in the grid.

### SetRowData client proof

Typed `SetRowData(6002, new ResidentDirectoryGridItem { ... })` (client `TRow`
overload) lowers to `grid.setRowData(6002, data)`. Clicking Replace shows
`Keyed Row` in the grid.

## Review Read Sources Proof

Focused scenario view `/Sandbox/Components/Grid/CareReview` (read sources gathered
to the server, P017). Raw EJ2 evidence:
`traces/raw-ej2-row-selected-click.trace.json`. Fresh-build Playwright
`WhenUsingFusionGridReview`. Each read is a typed source consumed by a gather; the
response summary asserts the gathered data (the justified gather-pipeline proof).

### CurrentViewRecords proof

Typed `CurrentViewRecords<...>()` (array source) is gathered to the current-view
endpoint; the response confirms `current view has 12 residents`.

### RowIndexByPrimaryKey proof

Typed `RowIndexByPrimaryKey(6005)` (scalar source) is gathered; the response
confirms resident 6005 is at `row index 5`.

### SelectedRecords proof

Typed `SelectedRecords<...>()` (array source) is gathered after selecting a range;
the response confirms `selected records:` with the selected residents.

### SelectedRowIndexes proof

Typed `SelectedRowIndexes<...>()` (int-array source) is gathered; the response
confirms `selected row indexes: 0, 1`.

## Grid Tooling Methods Proof

Focused scenario view `/Sandbox/Components/Grid/GridTooling` (paging jump + column
chooser only, P017). Fresh-build Playwright `WhenUsingFusionGridTooling`.

### GoToPage proof

Typed `GoToPage(2)` lowers to `grid.goToPage(2)`. Clicking Go moves the pager to
page 2 (the active numeric pager item becomes `2`). Raw paging behavior:
`traces/raw-ej2-data-state-change-paging-method.trace.json`.

### ShowColumnChooser proof

Typed `ShowColumnChooser()` lowers to `grid.openColumnChooser()`. Clicking Open
shows the EJ2 column chooser dialog (`.e-ccdlg`). The vendor behavior is the
visible dialog, asserted directly in Playwright.

## Export Methods Proof

Focused scenario view `/Sandbox/Components/Grid/RosterExport` (export only, P017).
Fresh-build Playwright `WhenUsingFusionGridExport` captures the browser download for
each export. Reporting lane.

### CsvExport proof

Typed `CsvExport()` lowers to `grid.csvExport()`. Clicking Export CSV downloads a
`.csv` file (captured by Playwright).

### ExcelExport proof

Typed `ExcelExport()` lowers to `grid.excelExport()`. Clicking Export Excel
downloads a `.xlsx` file (captured by Playwright).

### PdfExport proof

Typed `PdfExport()` lowers to `grid.pdfExport()`. Clicking Export PDF downloads a
`.pdf` file (captured by Playwright).

## Column Auto-Fit Methods Proof

Focused scenario view `/Sandbox/Components/Grid/ColumnFit` (auto-fit only, P017).
Fresh-build Playwright `WhenUsingFusionGridColumnFit` measures rendered header widths
(two columns start at 400px).

### AutoFitColumn proof

Typed `AutoFitColumn((x) => x.RiskLevel)` lowers to `grid.autoFitColumns("riskLevel")`.
Clicking Fit Risk shrinks the Risk column below 300px while the Resident column stays
wide (one column fitted).

### AutoFitColumns proof

Typed `AutoFitColumns()` lowers to `grid.autoFitColumns()`. Clicking Fit All then
shrinks the Resident column below 300px too (all columns fitted).

## FusionGrid Render Helper Proof

The `Html.FusionGrid<TModel, TRow>(plan, elementId, build)` render helper is proven
by every focused scenario: each renders a grid from the plan and asserts its
rendered rows/headers. The column scenario `CareStaffColumns.cshtml` +
`WhenUsingFusionGridColumns` is the witness — `Html.FusionGrid<...>(plan, GridId,
...)` produces `#care-staff-columns-grid` whose header cells the test asserts
visible. FusionGrid render helper proof: the typed builder renders a working,
interactable grid.

## Batch Risk Review Methods Proof

Focused scenario view `/Sandbox/Components/Grid/BatchRiskReview` (batch string
update + change gather, P017). Raw EJ2 evidence:
`traces/raw-ej2-cell-save-batch-edit.trace.json`. Fresh-build Playwright
`WhenUsingFusionGridBatchRisk`.

### UpdateCell string proof

Typed `UpdateCell(0, (ResidentDirectoryGridItem x) => x.RiskLevel, "Critical")`
(string overload) lowers to `grid.updateCell(0, "riskLevel", "Critical")`. Clicking
Flag marks the risk cell `.e-updatedtd` and shows `Critical`.

### BatchChanges proof

Typed `BatchChanges<...>()` (source) returns the staged batch changes; gathered to
the server, the response confirms `changed 1` after the flagged edit.

## BeginEdit Cancel Mutation Proof

Focused scenario view `/Sandbox/Components/Grid/LockedResidentEdit` (P018 payload
mutation, P017). Raw EJ2 evidence:
`traces/raw-ej2-begin-edit-normal.trace.json`. Fresh-build Playwright
`WhenUsingFusionGridLockedEdit`.

### BeginEdit Cancel proof

Typed `args.Cancel(t)` on `FusionGridBeginEditArgs` emits
`Set(PayloadSource.Event(), "cancel", true)` inside the `beginEdit` handler. For the
locked resident 6001 the mutation **prevents the editor from opening** (zero edit
inputs); for resident 6000 the editor opens normally. This proves the mutation
changes Syncfusion behavior at the event lifecycle (P018), not just that the flag is
writable.

### BeginEdit equivalence proof

`beginEdit` fires from two edit-mode triggers the facility uses: the inline
editor (`EditMode.Normal`) and the dialog editor (`EditMode.Dialog`). Raw EJ2
traces `raw-ej2-begin-edit-normal` and `raw-ej2-begin-edit-dialog` are
byte-identical across the typed surface (`rowData`, `rowIndex`, `type`,
`cancel`), so the variants are proven equivalent (P022, exit from the P001
"separate until proven equivalent" rule).

The single focused slice `BeginEditNormal.cshtml` proves both variants:
`begin_edit_normal_reads_row_data_and_can_cancel_edit` reads
`RowData.ResidentName`=`Amina Patel`, `RowIndex`=`0`, `Type`=`edit` from the
inline editor, and `begin_edit_dialog_reads_match_the_normal_edit_variant` opens
the dialog wrapper `#grid-begin-edit-dialog_dialogEdit_wrapper` and reads the
identical `Amina Patel` / `0` / `edit` from the dialog editor's own
`beginEdit` payload. Identical visible reads from each variant's own payload
resolve the broad `BeginEdit` selector and the read-only payload rows
(`FusionGridBeginEditArgs` contract, `RowData`, `RowIndex`, `Type`); the writable
`Cancel` flag stays proven by the mutation behavior above (P018), never by read
equivalence.

### Batch cell save proof

Focused slice `BatchCellEdit.cshtml` (care-ops batch task update). A nurse manager
edits Amina Patel's open-tasks cell to 4 in batch mode and saves it. `cellSave`
reads the typed cell fields before commit (`ColumnName`=`openTasks`, `Value`=`4`,
`PreviousValue`=`0`, `RowData.ResidentName`=`Amina Patel`, `Cancel`=`false`), and
`cellSaved` reads the same fields after commit. An impossible per-cell value (99)
triggers the `cellSave` Cancel mutation: `args.Cancel(t)` blocks the save, the
grid keeps 6 and never shows 99 - proving the mutation changes Syncfusion behavior
(P018), not just that the flag is writable. `cellSave`/`cellSaved` fire only in
batch edit mode, so this single batch trigger context is the complete variant set.
Proof: `WhenUsingFusionGridBatchCellEdit.batch_cell_save_reads_typed_cell_fields_and_blocks_an_impossible_value`.

### Before batch save proof

Same slice. Committing a staged change fires `beforeBatchSave`, which reads
`BatchChanges.ChangedRecords[0].ResidentName`=`Amina Patel` and `.OpenTasks`=`6`
and `Cancel`=`false`, then allows the commit so `actionComplete` reports. Staging
an oversized change (8 open tasks) and committing triggers the `beforeBatchSave`
Cancel mutation: `args.Cancel(t)` blocks the whole batch commit (`blocked batch 8`)
and `actionComplete` never reports a fresh request type. The empty `AddedRecords`
and `DeletedRecords` lists are not proven here; they require a focused batch
admit/discharge slice and stay open matrix rows.
Proof: `WhenUsingFusionGridBatchCellEdit.before_batch_save_reads_batch_changes_and_blocks_an_oversized_batch`.

### Batch roster change proof

Focused slice `BatchRosterChange.cshtml` (care director batch roster change). The
director discharges the first resident (`SelectRow(0)` + `DeleteSelectedRecord`)
and admits a new one (`AddRecord` of `Zara Admitted`) in one batch, then reviews
it. `beforeBatchSave` reads both list members of the batch:
`BatchChanges.AddedRecords[0].ResidentName`=`Zara Admitted` and
`BatchChanges.DeletedRecords[0].ResidentName`=`Amina Patel`, completing the
`BatchChanges` value type (ChangedRecords was proven by the batch task-update
slice). Raw EJ2 evidence: `raw-ej2-before-batch-save-add-delete` shows
`addedRecords` and `deletedRecords` populate (counts added 1, deleted 1).
Proof: `WhenUsingFusionGridBatchRosterChange.before_batch_save_reads_added_and_deleted_records_of_a_roster_change`.

### Resident edit form templates proof

Focused slice `ResidentEditForm.cshtml`. The facility edits residents two ways and
each typed edit template renders its own editor. The inline-cell grid (Normal edit)
proves the column `EditTemplate` builders: 2-arg `Select` renders a `careLevel`
`<select>` containing `Memory Care`, the typed 4-arg `Select` renders a
`primaryNurse` `<select>` containing `Nora Ellis`, and `DateInput` renders a native
`<input type="date" name="nextReviewDate">`. The dialog grid (Dialog edit) proves
`DialogForm` plus its labeled field builders: `Text` (`residentName`), `Number`
(`<input type="number" name="openTasks">`), `Date` (`<input type="date">`), and
3-arg `Select` (`riskLevel` `<select>` containing `Moderate`). Proofs:
`WhenUsingFusionGridResidentEditForm.inline_cell_editors_render_typed_select_and_date_templates`
and `.dialog_admission_form_renders_text_number_date_and_select_templates`.

### Grid edit validation proof

Slice `CareOps.cshtml` (care operations board). The `openTasks` column declares
`ValidationRules = careValidation.Field(r => r.OpenTasks)`, where `careValidation`
comes from `FusionGridValidation.From<ResidentCareItemValidator, ResidentCareItem>(ClientRules)`
- the same `ReactiveValidator` client metadata that powers form validation, with no
second hand-written ruleset. Editing the `openTasks` cell to `99` and leaving it runs
EJ2's native cell validation, which surfaces `Open tasks must be between 0 and 7.`
from the validator's `Range(0, 7)` rule, proving `From` reads the metadata and
`Field` emits the EJ2 column rule that reaches the in-cell editor.
Proof: `WhenUsingFusionGridCareOps.an_out_of_range_open_tasks_edit_is_blocked_by_the_generated_care_rule`.

### Resident admission audit proof

Focused slice `ResidentAdmissionAudit.cshtml` proves the add and delete edit-action
variants that the save-edit slice does not cover (P024). Admitting a resident
(`AddRecord` literal) fires the add edit-action: `actionBegin`/`actionComplete` read
`RequestType`=`save`, `Action`=`add`, `Type`/`Name`=`actionBegin`/`actionComplete`,
and `Data.ResidentName`=`Zara Added`. Discharging (`SelectRow` + `DeleteSelectedRecord`)
fires the delete edit-action: the payload reads `RequestType`=`delete`,
`Type`/`Name`=`actionBegin`, `Cancel`=`false`. Admitting a `Blocked Admission`
triggers the `Cancel()` mutation (`args.Cancel`) inside `actionBegin`, which blocks the
add before it persists (the grid never shows `Blocked Admission`), proving P018 for the
edit-action. Per P024 the variant-sensitive `RowIndex`, `SelectedRow`, and
`PreviousData` are read by the save-edit slice (where the typed surface applies) and
are absent for add/delete by raw EJ2 evidence. Proofs:
`WhenUsingFusionGridResidentAdmissionAudit.admitting_a_resident_reads_the_add_edit_action_payload`,
`.discharging_a_resident_reads_the_delete_edit_action_payload`,
`.blocking_an_admission_cancels_the_add_edit_action`.

### Server-backed roster proof

Focused slice `ServerRoster.cshtml`. The admit grid binds via `SetDataSource(json, x => x.Result)`
- the **[response path]** overload reading the `Result` array out of the server's
`{ Result, Count }` body (`server-load-status`=`loaded via response path`). Server-driven
CRUD reads the typed `Row` out of each response: `AddRecord(json, x => x.Row, 0)` admits
`Sofia Server`; `UpdateRow(0, json, x => x.Row)` updates row 0 in place to
`Amina Server Updated` from a key-preserving response (a key-changing response cannot
re-identify the row); `SetRowData(6005, json, x => x.Row)` patches resident 6005 to
`Lena Server Patch`. Proofs:
`WhenUsingFusionGridServerRoster.admitting_a_server_resident_reads_the_row_from_the_response`,
`.updating_row_zero_reads_the_row_from_the_response`,
`.patching_a_keyed_resident_reads_the_row_from_the_response`. The same slice also binds the keyed
grid from a **nested data-source property path** - `SetDataSource(json, x => x.Page.Result)` reads
the `Result` array nested under the `Page` envelope, rendering `Amina Patel` and `Henry Liu`
(`loading_a_nested_page_binds_from_the_nested_data_source_path`).

### Event-payload data source proof

Focused slice `BatchChangeReview.cshtml`. A nurse manager batch-edits an open-tasks cell and
commits. On `beforeBatchSave`, the review grid binds straight from the **event payload** -
`SetDataSource(args, x => x.BatchChanges.ChangedRecords)` reads the changed-records array out of
the event args (the [event payload path] overload, a payload-driven refresh), rendering the
changed `Amina Patel` row in the review grid. Proof:
`WhenUsingFusionGridBatchChangeReview.committing_a_batch_binds_the_review_grid_from_the_event_payload`.

### Printable roster proof

Focused slice `PrintableRoster.cshtml`. The charge nurse clicks Print Roster, which calls
`grid.Print()`. EJ2 opens the browser print view in a popup window populated with the grid rows;
the popup contains `Amina Patel` and `Memory Care`, and the status reads `print issued`. The
behavior is proven through a real popup interaction, with no `page.evaluate`. Proof:
`WhenUsingFusionGridPrintableRoster.printing_the_roster_opens_the_print_view_with_the_rows`.

### Remote DataManager adaptor proof

Focused slice `RemoteAdaptorRoster.cshtml`. The census grid binds to a remote endpoint through a
Syncfusion `DataManager { Url = "/Sandbox/Components/Grid/Data", Adaptor = "UrlAdaptor" }`. On load
the grid fetches the paged `{ result, count }` roster itself - no manual fetch wiring - and renders
real residents (`Memory Care`) with a server pager. Proof:
`WhenUsingFusionGridRemoteAdaptorRoster.data_manager_adaptor_fetches_the_remote_roster_on_load`.

### Builder-owned roster proof

Focused slice `BuilderRoster.cshtml`. A fixed shift roster is bound at render time through the
Syncfusion grid builder's own `b.DataSource(roster)` - no fetch, no reaction. The grid renders
`Amina Patel`, `Grace Bennett`, and `Memory Care` straight from the builder-owned data source.
Proof: `WhenUsingFusionGridBuilderRoster.builder_owned_data_source_renders_the_roster_without_a_fetch`.

## Coverage Status

Every row of the generated typed public API coverage matrix for FusionGrid is
`row-proven`, across all lanes: property read/write (scalar/object/array), method
(void/1-3 args/returns), event plus `Wire`, payload read (scalar/object/array),
payload mutation and payload void-method, array transform via `p.From` and
`AsSource`, and remote/custom-binding - the `{ result, count }` whole-response
shape, every `dataStateChange` trigger variant (sort/page/filter/search/group plus
clear/reset), all four `SetDataSource` scopes (whole response body, response path,
event payload path, typed array source), `Data()`/`Refresh()`, builder-owned and
DataManager-adaptor data sources, the nested data-source path, and every editing
variant (inline normal/dialog begin-edit, batch cell save and batch save, add and
delete edit-actions, server-backed `AddRecord`/`UpdateRow`/`SetRowData`, edit
templates, and grid validation). Each row is proven through a focused per-scenario
senior-living Playwright slice that exercises real user behavior with no
`page.evaluate`. The authoritative status is the coverage matrix and the gate
output, not this prose.
