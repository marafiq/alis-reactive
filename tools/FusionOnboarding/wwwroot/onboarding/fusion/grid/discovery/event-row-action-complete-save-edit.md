# Grid Event Row: actionComplete Save Edit

Status: accepted fields plus all listed public-contract exclusions are proven
for this variant row. The component audit remains open.

## Row Boundary

`actionComplete` fired after saving a normal edit of an existing row.

This row covers only `requestType=save` with `action=edit` in normal edit mode.
Add, delete, cancel, batch editing, dialog editing, validation failure,
foreign-key data, virtualization, and other `actionComplete` request types
require separate focused rows before their payloads can be mapped or claimed
equivalent.

## Evidence

- Syncfusion local `SaveEventArgs` type source: `node_modules/@syncfusion/ej2-grids/src/grid/base/interface.d.ts:2205`
- Syncfusion local normal edit `actionComplete` source: `node_modules/@syncfusion/ej2-grids/src/grid/actions/normal-edit.js:240`
- Save/edit probe: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/probes/raw-ej2-action-complete-save-edit.html`
- Save/edit trace: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/traces/raw-ej2-action-complete-save-edit.trace.json`
- Judgment calls: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/discovery/judgment-calls-action-complete-save-edit.md`

## Discovery Result

The probe instantiates EJ2 Grid directly, selects the first row, starts normal
edit mode, changes the `name` field, ends edit mode, waits for the
`actionComplete` save/edit event, and records the payload.

The deterministic trace file hash for this run is:

`69bfad7a44b26b04810c6e9a40884d68c5e93d5a12e58f10967b9fc5ffe2c06a`

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
| `name` | string | `actionComplete` | accepted |
| `previousData` | object | original row DTO | accepted as generic typed previous row DTO |
| `primaryKey` | array | `["id"]` | discovered but excluded for this row |
| `primaryKeyValue` | array | `[1]` | discovered but excluded for this row |
| `promise` | undefined | undefined | discovered but excluded for this row |
| `requestType` | string | `save` | accepted |
| `row` | DOM row | edited `TR` | excluded browser-owned object |
| `rowData` | object | original row DTO | excluded duplicate for this row |
| `rowIndex` | number | `0` | accepted |
| `selectedRow` | number | `-1` raw trace | accepted only as existing field; no new semantics claimed |
| `target` | undefined | undefined | excluded |
| `type` | string | `actionComplete` | accepted |

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

The shared public C# args type is reused because the accepted member names are
identical to the `actionBegin` save/edit row and remain useful for typed
workflow behavior. This does not make the two event rows equivalent; the
`name` and `type` values differ and this row has its own trace and proof
surface.

Do not add public `Row`, `Form`, `Target`, `ForeignKeyData`, `IsScroll`,
`PrimaryKey`, `PrimaryKeyValue`, `Promise`, or `RowData` from this row. They are
browser-owned objects, internal metadata, undefined, duplicate original data,
or lack a clear C# DSL behavior use case in this variant.

All listed exclusions have focused public-contract absence proof in this row.
That proof does not claim runtime behavior for the excluded DOM/internal/variant
metadata; it proves only that these discovered payload members are deliberately
not part of the typed public C# DSL for this variant.

## Current C# Audit

Current files:

- `Alis.Reactive.Fusion/Components/FusionGrid/Events/FusionGridOnEditing.cs`
- `Alis.Reactive.Fusion/Components/FusionGrid/FusionGridEvents.cs`

Current contract covers the accepted save/edit variant fields above, while the
shared `FusionGridEditActionArgs` contract remains not fully audited.

## Typed DSL Proof

Passed proof:

`scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridActionCompleteSaveEdit.action_complete_save_edit_reads_typed_current_previous_and_action_fields"`

TRX: `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-221837.trx`

The proof uses the focused `/Sandbox/Components/Grid/ActionCompleteSaveEdit`
vertical slice, selects a real Grid row, starts normal edit mode, changes the
resident name, saves the row, asserts visible `RequestType`, `Action`, `Type`,
`Name`, `Cancel`, `RowIndex`, `SelectedRow`, current `Data.ResidentName`,
previous `PreviousData.ResidentName`, visible row update, no public `Row`,
`Form`, `Target`, `ForeignKeyData`, `IsScroll`, `PrimaryKey`,
`PrimaryKeyValue`, `RowData`, `Index`, or `Promise` member on the typed args
contract, and no console errors.
