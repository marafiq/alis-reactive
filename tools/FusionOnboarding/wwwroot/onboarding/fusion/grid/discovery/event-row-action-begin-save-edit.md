# Grid Event Row: actionBegin Save Edit

Status: discovery, mapping, and focused typed DSL Playwright proof passed for
this variant row's accepted fields and all listed public-contract exclusions.
The component audit remains open.

## Row Boundary

`actionBegin` fired by saving a normal edit of an existing row.

This row covers only `requestType=save` with `action=edit` in normal edit mode.
Add, delete, cancel, batch editing, dialog editing, validation failure, foreign
key data, virtualization, and built-in toolbar default-action cancellation
require separate focused rows before their payloads can be mapped or claimed
equivalent.

## Evidence

- Syncfusion local `SaveEventArgs` type source: `node_modules/@syncfusion/ej2-grids/src/grid/base/interface.d.ts:2205`
- Syncfusion local normal edit save source: `node_modules/@syncfusion/ej2-grids/src/grid/actions/normal-edit.js:297`
- Save/edit probe: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/probes/raw-ej2-action-begin-save-edit.html`
- Save/edit trace: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/traces/raw-ej2-action-begin-save-edit.trace.json`
- Judgment calls: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/discovery/judgment-calls-action-begin-save-edit.md`

## Discovery Result

The probe instantiates EJ2 Grid directly, selects the first row, starts normal
edit mode, changes the `name` field, ends edit mode, and records the
`actionBegin` payload for the `save`/`edit` variant.

The deterministic trace file hash was stable across reruns:

`541e88de04f96832b8b372750d63eced801f1552be41ec66238f8d1735ef66a3`

## Observed Payload

The normal save/edit variant emitted these own keys:

| Key | Observed type | Observed sample | Mapping status |
| --- | --- | --- | --- |
| `action` | string | `edit` | accepted |
| `cancel` | boolean | `false` | accepted as read flag; mutation behavior not claimed by this row |
| `data` | object | edited row DTO | accepted as generic typed current row DTO |
| `foreignKeyData` | object | `{}` | discovered but excluded for this row |
| `form` | DOM form | edit form | excluded browser-owned object |
| `isScroll` | boolean | `false` | discovered but excluded for this row |
| `name` | string | `actionBegin` | accepted |
| `previousData` | object | original row DTO | accepted as generic typed previous row DTO |
| `primaryKey` | array | `["id"]` | discovered but excluded for this row |
| `primaryKeyValue` | array | `[1]` | discovered but excluded for this row |
| `requestType` | string | `save` | accepted |
| `row` | DOM row | edited `TR` | excluded browser-owned object |
| `rowData` | object | original row DTO | excluded duplicate for this row |
| `rowIndex` | number | `0` | accepted |
| `selectedRow` | number | `-1` raw trace | accepted only as existing field; no new semantics claimed |
| `target` | undefined | undefined | excluded |
| `type` | string | `actionBegin` | accepted |

## C# DSL Judgment Boundary

Public C# accepts the typed row data, previous row data, save/edit metadata, and
stable scalar coordinates:

- `FusionGridEditActionArgs<TRow>.Name`
- `FusionGridEditActionArgs<TRow>.RequestType`
- `FusionGridEditActionArgs<TRow>.Action`
- `FusionGridEditActionArgs<TRow>.Type`
- `FusionGridEditActionArgs<TRow>.Cancel`
- `FusionGridEditActionArgs<TRow>.Data`
- `FusionGridEditActionArgs<TRow>.PreviousData`
- `FusionGridEditActionArgs<TRow>.RowIndex`
- `FusionGridEditActionArgs<TRow>.SelectedRow`

`Index` was removed from the shared public type during this audit because the
save/edit trace does not emit it as an own payload key and no current typed DSL
usage proves a useful C# behavior for it. A future edit variant may add a
different typed member only after raw discovery proves the payload shape and a
focused behavior row proves its use.

The generated typed API matrix must keep `FusionGridEditActionArgs` unproven
until all accepted public members and variants on that shared payload are
covered.

Do not add public `Row`, `Form`, `Target`, `ForeignKeyData`, `IsScroll`,
`PrimaryKey`, `PrimaryKeyValue`, `RowData`, or `Index` from this row. They are
browser-owned objects, internal metadata, duplicate original data, absent from
the trace as own keys, or lack a clear C# DSL behavior use case in this
variant.

## Current C# Audit

Current files:

- `Alis.Reactive.Fusion/Components/FusionGrid/Events/FusionGridOnEditing.cs`
- `Alis.Reactive.Fusion/Components/FusionGrid/FusionGridEvents.cs`

Current contract covers the accepted save/edit variant fields above, while the
shared `FusionGridEditActionArgs` contract remains not fully audited.

## Typed DSL Proof

Passed proof:

`scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridEditing.action_begin_save_edit_reads_typed_current_previous_and_action_fields"`

TRX: `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-223102.trx`

The proof selects a real Grid row, starts normal edit mode, changes the resident
name, saves the row, asserts visible `RequestType`, `Action`, `Type`, `Name`,
`Cancel`, `RowIndex`, `SelectedRow`, current `Data.ResidentName`, previous
`PreviousData.ResidentName`, visible row update, no public `Row`, `Form`,
`Target`, `ForeignKeyData`, `IsScroll`, `PrimaryKey`, `PrimaryKeyValue`,
`RowData`, or `Index` typed C# properties, and no console errors.
