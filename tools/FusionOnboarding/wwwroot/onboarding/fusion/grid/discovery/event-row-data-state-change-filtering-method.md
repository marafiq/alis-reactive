# Grid Event Row: dataStateChange Filtering Method

Status: accepted fields and named exclusion subset proven for this row. Raw EJ2 discovery, C# accepted-field mapping, and focused typed DSL Playwright proof are complete for method-trigger filtering. `isComplex` is accepted as the source-backed `Predicate` node discriminator and is proven through typed binding plus visible filtering behavior. The component audit remains open.

## Row Boundary

`dataStateChange` fired by `grid.filterByColumn("status", "equal", "Open")`
in custom-binding mode.

This row covers only the method-trigger filtering variant. Filter-bar typing,
menu filtering, Excel/checkbox filtering, and clear-filter actions require
separate rows before their payloads can be mapped or claimed equivalent.

## Evidence

- Syncfusion local filter type source: `node_modules/@syncfusion/ej2-grids/src/grid/base/interface.d.ts:1261`
- Syncfusion data-state type source: `node_modules/@syncfusion/ej2-grids/src/grid/base/interface.d.ts:2470`
- Syncfusion predicate source: `node_modules/@syncfusion/ej2-data/src/query.d.ts:249`
- Syncfusion predicate serializer: `node_modules/@syncfusion/ej2-data/src/query.js:697`
- Syncfusion filter method path: `node_modules/@syncfusion/ej2-grids/src/grid/actions/filter.js:476`
- Syncfusion filter action path: `node_modules/@syncfusion/ej2-grids/src/grid/actions/filter.js:604`
- Syncfusion data-state trigger path: `node_modules/@syncfusion/ej2-grids/src/grid/actions/data.js:594`
- Method probe: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/probes/raw-ej2-data-state-change-filtering-method.html`
- Method trace: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/traces/raw-ej2-data-state-change-filtering-method.trace.json`
- Judgment calls: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/discovery/judgment-calls-data-state-change-filtering-method.md`

## Discovery Result

The probe binds `dataSource` as `{ result, count }`, handles
`dataStateChange`, and writes the next `{ result, count }` back into
`grid.dataSource`. This preserves the custom-binding path that emits
`dataStateChange`.

The deterministic trace file hash was stable across reruns:

`2029da94a371fba969228b4fb29fd9dbbb7e668a5098ac479dcd35c20ba6f7e6`

## Observed Filtering Payload

The method-filtering variant emitted these top-level keys:

| Key | Observed type | Observed sample | Mapping status |
| --- | --- | --- | --- |
| `action` | object | filtering action object | only shared action metadata accepted for this row |
| `name` | string | `dataStateChange` | accepted |
| `requiresCounts` | boolean | `true` | accepted |
| `skip` | number | `0` | accepted |
| `take` | number | `2` | accepted |
| `where` | array | complex predicate tree | accepted as whole typed array |

These checked keys were absent for this method-filtering variant:

| Key | Reason |
| --- | --- |
| `search` | belongs to searching row |
| `group` | belongs to grouping row |
| `sorted` | absent because this row starts from an unsorted grid; sorting plus filtering requires a combination row |

## Observed Where Payload

The `where` payload was a complex predicate tree:

| Path | Observed sample | Mapping status |
| --- | --- | --- |
| `where[].condition` | `and` | accepted |
| `where[].ignoreCase` | `true` | accepted |
| `where[].ignoreAccent` | `false` | accepted |
| `where[].predicates[]` | nested predicate item | accepted as recursive typed array |
| `where[].predicates[].field` | `status` | accepted |
| `where[].predicates[].operator` | `equal` | accepted |
| `where[].predicates[].value` | `Open` | accepted |
| `where[].predicates[].ignoreCase` | `true` | accepted |
| `where[].predicates[].ignoreAccent` | `false` | accepted |
| `where[].isComplex` | `true` | accepted as `FusionGridTextFilterCriterion.IsComplex` |
| `where[].predicates[].isComplex` | `false` | accepted as `FusionGridTextFilterCriterion.IsComplex` |

## Observed Action Payload

The `action` object emitted these keys:

| Key | Observed type/sample | Mapping status |
| --- | --- | --- |
| `cancel` | `false` | accepted for read only through shared action metadata |
| `requestType` | `filtering` | accepted through shared action metadata |
| `name` | `actionBegin` | accepted through shared action metadata |
| `type` | `actionBegin` | accepted through shared action metadata |
| `action` | `filter` | excluded for this row; clear-filter variant must prove whether it is useful |
| `currentFilteringColumn` | `status` | excluded for this row; duplicates `where[].predicates[].field` for this use case |
| `currentFilterObject` | filter settings object | excluded for this row; duplicates `where` and carries settings/internal shape |
| `columns` | filter settings columns array | excluded for this row; duplicates `where` and carries settings/internal shape |

## C# DSL Judgment Boundary

Discovery records every observed payload field and source candidate. Public C#
accepts the `where` query shape that the server/custom-binding use case needs:
field, operator, value, condition, isComplex, recursive predicates, ignoreCase,
and ignoreAccent. Syncfusion declares `DataStateChangeEventArgs.where` as
`Predicate[]`; `Predicate` declares `isComplex` and `predicates`, and its
serializer emits `isComplex` for both composite and leaf nodes.

The previous public C# fields `Predicate` and `MatchCase` were removed from
`FusionGridTextFilterCriterion` for this row because the top-level `where`
payload does not emit them. `matchCase` appears only in `action.currentFilterObject`,
which this row excludes from the public typed payload.

## Current C# Audit

Current files:

- `Alis.Reactive.Fusion/Components/FusionGrid/Events/FusionGridOnDataStateChange.cs`
- `Alis.Reactive.Fusion/Components/FusionGrid/FusionGridEvents.cs`

Current contract covers the accepted filtering-method row:

- `FusionGridDataStateChangeArgs.Name` maps `name`
- `FusionGridDataStateChangeArgs.Skip` maps `skip`
- `FusionGridDataStateChangeArgs.Take` maps `take`
- `FusionGridDataStateChangeArgs.RequiresCounts` maps `requiresCounts`
- `FusionGridDataStateChangeArgs.Where` maps `where`
- `FusionGridAction.RequestType` maps nested `action.requestType`
- `FusionGridAction.Name` maps nested `action.name`
- `FusionGridAction.Type` maps nested `action.type`
- `FusionGridAction.Cancel` maps nested `action.cancel`
- `FusionGridTextFilterCriterion.Field` maps nested `field`
- `FusionGridTextFilterCriterion.Operator` maps nested `operator`
- `FusionGridTextFilterCriterion.Value` maps nested `value`
- `FusionGridTextFilterCriterion.Condition` maps `condition`
- `FusionGridTextFilterCriterion.IsComplex` maps `isComplex`
- `FusionGridTextFilterCriterion.Predicates` maps recursive nested predicates
- `FusionGridTextFilterCriterion.IgnoreCase` maps `ignoreCase`
- `FusionGridTextFilterCriterion.IgnoreAccent` maps `ignoreAccent`

The contract intentionally does not expose `action.currentFilterObject`,
`action.columns`, `action.currentFilteringColumn`, or `action.action` for this
row.

## Typed DSL Proof

The row is proven by:

`scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.filtering_method_sends_typed_where_payload_and_refreshes_grid"`

Result: passed, TRX
`tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-205951.trx`.

This proof gathers the whole typed `Where` array, parses the POST body as JSON,
asserts the predicate tree, including `isComplex=true` for the composite node
and `isComplex=false` for the leaf predicate, visibly reads `Name`, `Skip`, `Take`,
`RequiresCounts`, `Action.RequestType`, `Action.Name`, `Action.Type`, and
`Action.Cancel` from the typed event payload, and asserts visible Grid rows are
filtered to the requested wing. It also proves the raw `action` settings object,
settings-only `matchCase`/`predicate`, and variant-foreign `search`/`group`/
`sorted` fields are not emitted by this typed request. The sandbox server uses
`FusionGridTextFilterCriterion.IsComplex` before recursing into nested
predicates, so visible row filtering fails if model binding does not preserve
the discriminator. It closes the method-trigger filtering accepted row and the
proven exclusion subset only. It does not close filter-bar typing, menu
filtering, Excel/checkbox filtering, clear-filter actions, searching, grouping,
properties, methods, or the full Grid audit.
