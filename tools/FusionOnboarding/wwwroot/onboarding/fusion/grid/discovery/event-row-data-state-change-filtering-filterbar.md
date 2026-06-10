# Grid Event Row: dataStateChange FilterBar Typing

Status: accepted fields and named exclusion subset proven for this row. Raw EJ2
discovery, C# accepted-field mapping, and focused typed DSL Playwright proof are
complete for FilterBar typing. The component audit remains open.

## Row Boundary

`dataStateChange` fired by typing into the `status` FilterBar input in
custom-binding mode.

This row covers only the FilterBar typing variant. Method-trigger filtering,
menu filtering, Excel/checkbox filtering, and clear-filter actions remain
separate rows unless raw traces prove payload equivalence for those gestures.

## Evidence

- Syncfusion FilterBar render path: `node_modules/@syncfusion/ej2-grids/src/grid/actions/filter.js:75`
- Syncfusion FilterBar keyup path: `node_modules/@syncfusion/ej2-grids/src/grid/actions/filter.js:938`
- Syncfusion data-state type source: `node_modules/@syncfusion/ej2-grids/src/grid/base/interface.d.ts:2470`
- Syncfusion predicate source: `node_modules/@syncfusion/ej2-data/src/query.d.ts:249`
- Syncfusion predicate serializer: `node_modules/@syncfusion/ej2-data/src/query.js:697`
- Syncfusion data-state trigger path: `node_modules/@syncfusion/ej2-grids/src/grid/actions/data.js:594`
- FilterBar probe: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/probes/raw-ej2-data-state-change-filtering-filterbar.html`
- FilterBar trace: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/traces/raw-ej2-data-state-change-filtering-filterbar.trace.json`
- Judgment calls: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/discovery/judgment-calls-data-state-change-filtering-filterbar.md`

## Discovery Result

The probe binds `dataSource` as `{ result, count }`, handles
`dataStateChange`, and writes the next `{ result, count }` back into
`grid.dataSource`. It types into the actual FilterBar input and captures the
custom-binding data-state event.

The deterministic trace file hash:

`6912da2d3f92fc308d8d67be8e2a12933725d45df79de7a7560984efbde823bb`

## Observed Filtering Payload

The FilterBar typing variant emitted these top-level keys:

| Key | Observed type | Observed sample | Mapping status |
| --- | --- | --- | --- |
| `action` | object | filtering action object | only shared action metadata accepted for this row |
| `name` | string | `dataStateChange` | accepted |
| `requiresCounts` | boolean | `true` | accepted |
| `skip` | number | `0` | accepted |
| `take` | number | `2` | accepted |
| `where` | array | complex predicate tree | accepted as whole typed array |

These checked keys were absent for this FilterBar typing variant:

| Key | Reason |
| --- | --- |
| `aggregates` | declared data-state candidate, absent for this row; aggregation requires its own row |
| `dataSource` | declared data-state candidate, absent for this row; data-source lanes are separate rows |
| `search` | belongs to searching row |
| `group` | belongs to grouping row |
| `isLazyLoad` | declared data-state candidate, absent for this row; lazy-load behavior requires its own row |
| `onDemandGroupInfo` | declared data-state candidate, absent for this row; on-demand grouping requires its own row |
| `select` | declared data-state candidate, absent for this row; selection behavior requires its own row |
| `sorted` | absent because this row starts from an unsorted grid; sorting plus filtering requires a combination row |
| `table` | declared data-state candidate, absent for this row; no typed server query use case in this variant |

## Observed Where Payload

The `where` payload was a complex predicate tree:

| Path | Observed sample | Mapping status |
| --- | --- | --- |
| `where[].condition` | `and` | accepted |
| `where[].ignoreCase` | `true` | accepted |
| `where[].ignoreAccent` | `false` | accepted |
| `where[].predicates[]` | nested predicate item | accepted as recursive typed array |
| `where[].predicates[].field` | `status` | accepted |
| `where[].predicates[].operator` | `startswith` | accepted |
| `where[].predicates[].value` | `Op` | accepted |
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

The excluded `action.currentFilterObject` and `action.columns[]` settings
objects carried these nested keys. They are recorded for audit completeness but
not onboarded into public C# for this row:

| Nested settings path | Observed sample | Exclusion reason |
| --- | --- | --- |
| `action.currentFilterObject.actualFilterValue` | `{}` | settings object detail; no durable typed server query use case |
| `action.currentFilterObject.actualOperator` | `{}` | settings object detail; no durable typed server query use case |
| `action.currentFilterObject.field` | `status` | duplicates `where[].predicates[].field` |
| `action.currentFilterObject.ignoreAccent` | `false` | duplicates `where[].predicates[].ignoreAccent` |
| `action.currentFilterObject.isForeignKey` | `false` | no foreign-key behavior proven for this text-filter row |
| `action.currentFilterObject.matchCase` | `false` | settings-only field; top-level query uses `ignoreCase` |
| `action.currentFilterObject.operator` | `startswith` | duplicates `where[].predicates[].operator` |
| `action.currentFilterObject.predicate` | `and` | settings-only field; data-state query uses `where[].condition` |
| `action.currentFilterObject.predicates` | `[]` | settings-only array; data-state query uses `where[].predicates[]` |
| `action.currentFilterObject.uid` | `grid-column2` | browser-generated column uid, not durable typed DSL |
| `action.currentFilterObject.value` | `Op` | duplicates `where[].predicates[].value` |
| `action.columns[].actualFilterValue` | `{}` | settings object detail; no durable typed server query use case |
| `action.columns[].actualOperator` | `{}` | settings object detail; no durable typed server query use case |
| `action.columns[].field` | `status` | duplicates `where[].predicates[].field` |
| `action.columns[].ignoreAccent` | `false` | duplicates `where[].predicates[].ignoreAccent` |
| `action.columns[].isForeignKey` | `false` | no foreign-key behavior proven for this text-filter row |
| `action.columns[].matchCase` | `false` | settings-only field; top-level query uses `ignoreCase` |
| `action.columns[].operator` | `startswith` | duplicates `where[].predicates[].operator` |
| `action.columns[].predicate` | `and` | settings-only field; data-state query uses `where[].condition` |
| `action.columns[].predicates` | `[]` | settings-only array; data-state query uses `where[].predicates[]` |
| `action.columns[].uid` | `grid-column2` | browser-generated column uid, not durable typed DSL |
| `action.columns[].value` | `Op` | duplicates `where[].predicates[].value` |

## C# DSL Judgment Boundary

Discovery records every observed payload field and source candidate. Public C#
accepts the same `where` query shape as method-trigger filtering: field,
operator, value, condition, isComplex, recursive predicates, ignoreCase, and
ignoreAccent.

The row does not add `Predicate`, `MatchCase`, `action.currentFilterObject`,
`action.columns`, `action.currentFilteringColumn`, or `action.action` to the
public typed payload. Those belong to the raw settings/action object, not the
typed server query shape.

## Typed DSL Proof

The row is proven by:

`scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.filtering_filterbar_typing_sends_typed_where_payload_and_refreshes_grid"`

Result: passed, TRX
`tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-213206.trx`.

This proof types into the real sandbox FilterBar input, presses Enter to commit
the Syncfusion FilterBar gesture, waits only for a same-endpoint POST whose
body already contains the expected `wing startswith N` predicate, gathers the
whole typed `Where` array, parses the POST body as JSON, asserts the predicate
tree, including `startswith` and the parent/leaf `isComplex` discriminator,
visibly reads `Name`, `Skip`, `Take`, `RequiresCounts`,
`Action.RequestType`, `Action.Name`, `Action.Type`, and `Action.Cancel` from
the typed event payload, and asserts visible Grid rows are filtered to the
requested wing. It also proves the raw `action` settings object,
settings-only `matchCase`/`predicate`, declared-foreign `aggregates`,
`dataSource`, `isLazyLoad`, `onDemandGroupInfo`, `select`, and `table`, and
variant-foreign `search`/`group`/`sorted` fields are not emitted by this typed
request.
