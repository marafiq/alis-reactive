# Grid Event Row: dataStateChange Searching Method

Status: proven for this row. Raw EJ2 discovery, C# accepted-field mapping, and focused typed DSL Playwright proof are complete for method-trigger searching. The component audit remains open.

## Row Boundary

`dataStateChange` fired by `grid.search("Memory")` in custom-binding mode.

This row covers only the method-trigger search variant. Toolbar search input,
toolbar clear-search, initial search settings, and search combined with
sort/filter/group state require separate rows before their payloads can be
mapped or claimed equivalent. Method-trigger `ClearSearch()` is covered by its
own clear-search row.

## Evidence

- Syncfusion local search action source: `node_modules/@syncfusion/ej2-grids/src/grid/actions/search.js:52`
- Syncfusion local search model-changed source: `node_modules/@syncfusion/ej2-grids/src/grid/actions/search.js:131`
- Syncfusion local search event type: `node_modules/@syncfusion/ej2-grids/src/grid/base/interface.d.ts:1294`
- Syncfusion local data-state type source: `node_modules/@syncfusion/ej2-grids/src/grid/base/interface.d.ts:2470`
- Syncfusion local search settings source: `node_modules/@syncfusion/ej2-grids/src/grid/base/grid-model.d.ts:422`
- Syncfusion local data-state trigger path: `node_modules/@syncfusion/ej2-grids/src/grid/actions/data.js:594`
- Method probe: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/probes/raw-ej2-data-state-change-searching-method.html`
- Method trace: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/traces/raw-ej2-data-state-change-searching-method.trace.json`
- Judgment calls: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/discovery/judgment-calls-data-state-change-searching-method.md`

## Discovery Result

The probe binds `dataSource` as `{ result, count }`, handles
`dataStateChange`, and writes the next `{ result, count }` back into
`grid.dataSource`. This preserves the custom-binding path that emits
`dataStateChange`.

The deterministic trace file hash was stable across reruns:

`a8ae3eab1763ad7d849b752248b020a785b584a39f1654a352833f8635fef994`

## Observed Searching Payload

The method-searching variant emitted these top-level keys:

| Key | Observed type | Observed sample | Mapping status |
| --- | --- | --- | --- |
| `action` | object | searching action object | only shared action metadata accepted for this row |
| `name` | string | `dataStateChange` | accepted |
| `requiresCounts` | boolean | `true` | accepted |
| `search` | array | `[{ fields, key, operator, ignoreCase, ignoreAccent }]` | accepted as whole typed array |
| `skip` | number | `0` | accepted |
| `take` | number | `2` | accepted |

These checked keys were absent for this method-searching variant:

| Key | Reason |
| --- | --- |
| `where` | belongs to filtering row |
| `group` | belongs to grouping row |
| `sorted` | absent because this row starts from an unsorted grid; sorting plus searching requires a combination row |

## Observed Search Payload

| Path | Observed sample | Mapping status |
| --- | --- | --- |
| `search[].fields` | `["name", "careLevel", "wing"]` | accepted |
| `search[].key` | `Memory` | accepted |
| `search[].operator` | `contains` | accepted |
| `search[].ignoreCase` | `true` | accepted |
| `search[].ignoreAccent` | `false` | accepted |

## Observed Action Payload

The `action` object emitted these keys:

| Key | Observed type/sample | Mapping status |
| --- | --- | --- |
| `requestType` | `searching` | accepted through shared action metadata |
| `name` | `actionBegin` | accepted through shared action metadata |
| `type` | `actionBegin` | accepted through shared action metadata |
| `searchString` | `Memory` | excluded for this row; duplicates `search[].key` |

`action.cancel` was absent in this row even though it is present on sorting,
paging, and filtering rows. This row must not use the sorting/paging/filtering
presence of `cancel` to claim search emits it.

## C# DSL Judgment Boundary

Discovery records every observed payload field and source candidate. Public C#
accepts the top-level `search` query shape that the server/custom-binding use
case needs: fields, key, operator, ignoreCase, and ignoreAccent.

Do not add `FusionGridAction.SearchString` from this row. It duplicates
`search[].key` and does not provide a distinct typed use case.

## Current C# Audit

Current files:

- `Alis.Reactive.Fusion/Components/FusionGrid/Events/FusionGridOnDataStateChange.cs`
- `Alis.Reactive.Fusion/Components/FusionGrid/FusionGridEvents.cs`

Current contract covers the accepted searching-method row:

- `FusionGridDataStateChangeArgs.Name` maps `name`
- `FusionGridDataStateChangeArgs.Skip` maps `skip`
- `FusionGridDataStateChangeArgs.Take` maps `take`
- `FusionGridDataStateChangeArgs.RequiresCounts` maps `requiresCounts`
- `FusionGridDataStateChangeArgs.Search` maps `search`
- `FusionGridAction.RequestType` maps nested `action.requestType`
- `FusionGridAction.Name` maps nested `action.name`
- `FusionGridAction.Type` maps nested `action.type`
- `FusionGridSearchDescriptor.Fields` maps `search[].fields`
- `FusionGridSearchDescriptor.Key` maps `search[].key`
- `FusionGridSearchDescriptor.Operator` maps `search[].operator`
- `FusionGridSearchDescriptor.IgnoreCase` maps `search[].ignoreCase`
- `FusionGridSearchDescriptor.IgnoreAccent` maps `search[].ignoreAccent`

The contract intentionally does not expose `SearchString` for this row.

## Typed DSL Proof

The row is proven by:

`scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.searching_method_sends_typed_search_payload_and_refreshes_grid"`

Result: passed, TRX
`tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-233449.trx`.

This proof gathers the whole typed `Search` array, parses the POST body as JSON,
asserts the search descriptor, visibly reads `Name`, `Skip`, `Take`,
`RequiresCounts`, `Action.RequestType`, `Action.Name`, and `Action.Type` from
the typed event payload, and asserts visible Grid rows are searched to Memory
Care. It also asserts descriptor-level duplicate `searchString` absence, request
body absence for duplicate `actionSearchString`, absent `actionCancel`, and
searching-foreign `where`, `group`, and `sorted`; public `FusionGridAction`
omits `SearchString`. It closes the method-trigger searching row only. It does
not close toolbar search input, toolbar clear-search, searching combined with
other state, grouping, properties, methods, or the full Grid audit.
