# Grid Event Row: dataStateChange Clear Filtering Method

Status: proven for this row. Raw EJ2 discovery, C# method mapping, sandbox vertical-slice correction, and focused typed DSL Playwright proof are complete for method-trigger clear filtering. The component audit remains open.

## Row Boundary

`dataStateChange` fired by `grid.clearFiltering()` after an active `grid.filterByColumn("status", "equal", "Open")` in custom-binding mode.

This row covers only method-trigger clearing of an active filter. Menu clear-filter gestures, toolbar/filterbar clear gestures, clear filtering combined with sort/search/group state, and filter settings objects require separate rows before their payloads can be mapped or claimed equivalent.

## Evidence

- Method probe: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/probes/raw-ej2-data-state-change-clear-filtering-method.html`
- Method trace: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/traces/raw-ej2-data-state-change-clear-filtering-method.trace.json`
- Judgment calls: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/discovery/judgment-calls-data-state-change-clear-filtering-method.md`
- Skill pattern: `tools/FusionOnboarding/wwwroot/onboarding/fusion/_skill/pattern-map.md#p019-clear-and-reset-methods-must-not-be-masked-by-manual-reloads`

## Discovery Result

The probe first calls `grid.filterByColumn("status", "equal", "Open")`, applies the emitted custom-binding state to the Grid, then calls `grid.clearFiltering()`. This proves the clear method from a real filtered state.

The deterministic trace file hash is:

`80755b925a130bb8eb9c445756fcfb48022dca3c612b0d169083d608def20537`

## Observed Clear-Filtering Payload

The clear-filtering variant emitted these top-level keys:

| Key | Observed type | Observed sample | Mapping status |
| --- | --- | --- | --- |
| `action` | object | refresh action object | only `requestType` and `name` accepted for this row |
| `name` | string | `dataStateChange` | accepted |
| `requiresCounts` | boolean | `true` | accepted as visible event metadata |
| `skip` | number | `0` | accepted |
| `take` | number | `2` | accepted |

These checked keys were absent for this method-clear-filtering variant:

| Key | Reason |
| --- | --- |
| `where` | clearing filters omits the top-level filter descriptor; do not invent `[]` |
| `search` | belongs to searching rows |
| `group` | belongs to grouping rows |
| `sorted` | belongs to sorting/grouping rows |

## Observed Action Payload

The `action` object emitted these keys:

| Key | Observed type/sample | Mapping status |
| --- | --- | --- |
| `requestType` | `refresh` | accepted through shared action metadata |
| `name` | `actionBegin` | accepted through shared action metadata |
| `columns` | empty array | excluded for this row; settings/internal filter metadata, no public typed use case |
| `currentFilterObject` | `null` | excluded for this row; settings/internal filter metadata, no public typed use case |

`action.type`, `action.cancel`, `action.action`, and `action.currentFilteringColumn` were absent in this row. Do not infer them from method-filtering or FilterBar variants.

## C# DSL Judgment Boundary

The useful public C# member for this row is `FusionGrid.ClearFiltering()`, which maps directly to Syncfusion `clearFiltering`. No new primitive is required.

The previous Directory sandbox implementation called `LoadDirectory(...)` after `ClearFiltering()`. That manual reload was removed because it could make visible rows pass without proving the method's actual `dataStateChange` refresh lane.

## Typed DSL Proof

The row is proven by:

`scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.clear_filtering_method_clears_active_filter_and_refreshes_grid"`

Result: passed, TRX
`tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260607-014720.trx`.

This proof starts from an active typed `FilterTextBy(...Wing == "North")` state, triggers typed `ClearFiltering()`, captures the clear request from the component event lane, asserts `where`, `search`, `group`, `sorted`, raw `action`, and filter settings fields are omitted from the request, reads visible `dataStateChange` metadata, and asserts the directory returns to `240 residents matched` with first row `Amina Patel`.
