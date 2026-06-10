# Grid Event Row: dataStateChange Clear Search Method

Status: active for this row. Raw EJ2 discovery is complete; typed DSL proof is required before this row may be marked proven.

## Row Boundary

`dataStateChange` fired by `grid.search("")` after an active `grid.search("Memory")` in custom-binding mode.

This row covers only the method-trigger clear-search variant. It does not prove toolbar search input clearing, initial empty search settings, searching combined with sort/filter/group state, or a public `action.searchString` contract.

## Evidence

- Method probe: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/probes/raw-ej2-data-state-change-clear-search-method.html`
- Method trace: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/traces/raw-ej2-data-state-change-clear-search-method.trace.json`
- Judgment calls: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/discovery/judgment-calls-data-state-change-clear-search-method.md`

## Discovery Result

The probe first calls `grid.search("Memory")`, applies the emitted custom-binding state to the Grid, then calls `grid.search("")`. This proves the clear method from a real searched state, not an already-empty grid.

The deterministic trace file hash for the corrected trace is:

`c855222fc79fa260062381aff53b269b9d7f34581cd91234f0a119d6e4b3e5b1`

## Observed Clear-Search Payload

The clear-search variant emitted these top-level keys:

| Key | Observed type | Observed sample | Mapping status |
| --- | --- | --- | --- |
| `action` | object | searching action object with empty `searchString` | only shared action metadata accepted for this row |
| `name` | string | `dataStateChange` | accepted |
| `requiresCounts` | boolean | `true` | accepted as visible event metadata |
| `skip` | number | `0` | accepted |
| `take` | number | `2` | accepted |

These checked keys were absent for this method-clear-search variant:

| Key | Reason |
| --- | --- |
| `search` | clearing search omits the top-level search descriptor; do not invent `[]` |
| `where` | belongs to filtering row |
| `group` | belongs to grouping row |
| `sorted` | belongs to sorting/grouping rows |

## Observed Action Payload

The `action` object emitted these keys:

| Key | Observed type/sample | Mapping status |
| --- | --- | --- |
| `requestType` | `searching` | accepted through shared action metadata |
| `name` | `actionBegin` | accepted through shared action metadata |
| `type` | `actionBegin` | accepted through shared action metadata |
| `searchString` | empty string | excluded for this row; clear behavior is proven by `ClearSearch()` and absent top-level `search`, not by a public duplicate field |

`action.cancel` was absent in this row. Do not infer it from sorting, paging, or filtering variants.

## C# DSL Judgment Boundary

The useful public C# member for this row is `FusionGrid.ClearSearch()`. The method maps to Syncfusion search clearing by calling the existing `Search(string.Empty)` primitive path. It does not require a new primitive and does not justify exposing `FusionGridAction.SearchString`.

## Typed DSL Proof Required

The row must be proven by:

`scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.clear_search_method_clears_active_search_and_refreshes_grid"`

The proof must:

- start from an active typed `Search("Memory")` state;
- trigger typed `ClearSearch()`;
- assert the request body has `skip=0` and `take=8`;
- assert the request body omits `search`, `where`, `group`, `sorted`, untyped `action`, `actionSearchString`, and `actionCancel`;
- assert visible typed event metadata reads `Name`, `Skip`, `Take`, `RequiresCounts`, `Action.RequestType`, `Action.Name`, and `Action.Type`;
- assert visible rows and summary return to the unsearched directory.
