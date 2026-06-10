# Grid Event Row: dataStateChange Grouping Method

Status: proven for this row. Raw EJ2 discovery, C# accepted-field mapping, and focused typed DSL Playwright proof are complete for method-trigger grouping. The component audit remains open.

## Row Boundary

`dataStateChange` fired by `grid.groupColumn("careLevel")` in custom-binding mode.

This row covers only the method-trigger grouping variant. Header drag/drop grouping, grouping toggle buttons, ungrouping, clear grouping, group sort, lazy-load group expand/collapse, initial group settings, and grouping combined with filter/search state require separate focused rows before their payloads can be mapped or claimed equivalent.

## Evidence

- Syncfusion local group method source: `node_modules/@syncfusion/ej2-grids/src/grid/actions/group.js:838`
- Syncfusion local group model update source: `node_modules/@syncfusion/ej2-grids/src/grid/actions/group.js:906`
- Syncfusion local group model-changed source: `node_modules/@syncfusion/ej2-grids/src/grid/actions/group.js:1133`
- Syncfusion local data-state trigger path: `node_modules/@syncfusion/ej2-grids/src/grid/actions/data.js:601`
- Syncfusion local group event type: `node_modules/@syncfusion/ej2-grids/src/grid/base/interface.d.ts:1269`
- Syncfusion local data-state type source: `node_modules/@syncfusion/ej2-grids/src/grid/base/interface.d.ts:2470`
- Syncfusion local group settings source: `node_modules/@syncfusion/ej2-grids/src/grid/base/grid-model.d.ts:541`
- Method probe: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/probes/raw-ej2-data-state-change-grouping-method.html`
- Method trace: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/traces/raw-ej2-data-state-change-grouping-method.trace.json`
- Judgment calls: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/discovery/judgment-calls-data-state-change-grouping-method.md`

## Discovery Result

The probe binds `dataSource` as `{ result, count }`, handles
`dataStateChange`, and writes the next `{ result, count }` back into
`grid.dataSource`. This preserves the custom-binding path that emits
`dataStateChange`.

The deterministic trace file hash was stable across reruns:

`df82ecbe08629b9a9e8ceb608136588dbd3483489777e28690e402cdcfd44fbd`

## Observed Grouping Payload

The method-grouping variant emitted these top-level keys:

| Key | Observed type | Observed sample | Mapping status |
| --- | --- | --- | --- |
| `action` | object | grouping action object | shared action metadata and `columnName` accepted |
| `group` | array | `["careLevel"]` | accepted as the declared data-state grouped field array |
| `groups` | array | `["careLevel"]` | discovered but excluded from public C# for this row |
| `name` | string | `dataStateChange` | accepted |
| `requiresCounts` | boolean | `true` | accepted |
| `skip` | number | `0` | accepted |
| `sorted` | array | `[{ name: "careLevel", direction: "ascending" }]` | accepted as existing sorted state emitted by grouping |
| `take` | number | `2` | accepted |

These checked keys were absent for this method-grouping variant:

| Key | Reason |
| --- | --- |
| `where` | belongs to filtering or a combination row |
| `search` | belongs to searching or a combination row |
| `aggregates` | not emitted by this no-aggregate grouping row |

## Observed Action Payload

The `action` object emitted these keys:

| Key | Observed type/sample | Mapping status |
| --- | --- | --- |
| `requestType` | `grouping` | accepted through shared action metadata |
| `name` | `actionBegin` | accepted through shared action metadata |
| `type` | `actionBegin` | accepted through shared action metadata |
| `columnName` | `careLevel` | accepted as grouping action column |
| `preventFocusOnGroup` | `false` | excluded from public C# for this row |

`action.cancel` was absent in this row even though it is present on sorting,
paging, and filtering rows. This row must not use other action variants to claim
grouping emits it.

## C# DSL Judgment Boundary

Discovery records every observed payload field and source candidate. Public C#
accepts the declared top-level `group` query shape that the server/custom-binding
use case needs: a typed array of grouped field names.

Do not add `FusionGridDataStateChangeArgs.Groups` from this row. It duplicates
`group`, is not declared by the public `DataStateChangeEventArgs`, and does not
provide a distinct typed use case.

Do not add `FusionGridAction.PreventFocusOnGroup`. It is an internal UI focus
flag with no server data-state or visible DSL behavior use case in this row.

## Current C# Audit

Current files:

- `Alis.Reactive.Fusion/Components/FusionGrid/Events/FusionGridOnDataStateChange.cs`
- `Alis.Reactive.Fusion/Components/FusionGrid/FusionGridEvents.cs`
- `Alis.Reactive.Fusion/Components/FusionGrid/FusionGridQueryExtensions.cs`

Current contract covers the accepted grouping-method row:

- `FusionGridDataStateChangeArgs.Name` maps `name`
- `FusionGridDataStateChangeArgs.Skip` maps `skip`
- `FusionGridDataStateChangeArgs.Take` maps `take`
- `FusionGridDataStateChangeArgs.RequiresCounts` maps `requiresCounts`
- `FusionGridDataStateChangeArgs.Group` maps `group`
- `FusionGridDataStateChangeArgs.Sorted` maps the auto-sort emitted by grouping
- `FusionGridAction.RequestType` maps `action.requestType`
- `FusionGridAction.ColumnName` maps `action.columnName`
- `FusionGridAction.Name` maps `action.name`
- `FusionGridAction.Type` maps `action.type`

The contract intentionally does not expose `Groups` or `PreventFocusOnGroup`
for this row.

## Typed DSL Proof

The row is proven by:

`scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.grouping_method_sends_typed_group_payload_and_refreshes_grid"`

Result: passed, TRX
`tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-234123.trx`.

This proof gathers the whole typed `Group` array, parses the POST body as JSON,
asserts the emitted grouping payload, visibly reads `Name`, `Skip`, `Take`,
`RequiresCounts`, `Action.RequestType`, `Action.Name`, `Action.Type`, and
`Action.ColumnName` from the typed event payload, asserts the duplicate `groups`
alias, internal `actionPreventFocusOnGroup`, absent `actionCancel`,
grouping-foreign `where` and `search`, and no-aggregate `aggregates` are not
part of the typed request, and proves visible Grid grouping behavior through
rendered caption rows. It also asserts public C# contracts omit
`FusionGridDataStateChangeArgs.Groups` and
`FusionGridAction.PreventFocusOnGroup`.

It closes the method-trigger grouping row only. It does not close header
drag/drop grouping, grouping toggle buttons, ungrouping, clear grouping, group
sort, lazy-load group expand/collapse, grouped aggregates, initial group
settings, grouping combined with filter/search state, properties, methods, or
the full Grid audit.
