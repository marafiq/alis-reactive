# Grid Event Row: beginEdit Normal Edit

Status: raw EJ2 discovery, judgment, and focused typed DSL Playwright proof are
complete for this normal-edit row. The component audit remains open.

## Row Boundary

`beginEdit` fired by starting normal edit mode on an existing row.

This row covers only normal edit of an existing row through `grid.startEdit()`.
Add mode, dialog edit, virtualization, foreign-key data, double-click edit,
adaptive edit, and cancel conditions based on other payload members require
separate focused rows before their payloads can be mapped or claimed
equivalent.

## Evidence

- Syncfusion local `BeginEditArgs` type source: `node_modules/@syncfusion/ej2-grids/src/grid/base/interface.d.ts:2140`
- Syncfusion local normal edit source: `node_modules/@syncfusion/ej2-grids/src/grid/actions/normal-edit.js:109`
- Syncfusion local `beginEdit` trigger source: `node_modules/@syncfusion/ej2-grids/src/grid/actions/normal-edit.js:148`
- Normal edit probe: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/probes/raw-ej2-begin-edit-normal.html`
- Normal edit trace: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/traces/raw-ej2-begin-edit-normal.trace.json`
- Judgment calls: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/discovery/judgment-calls-begin-edit-normal.md`

## Discovery Result

The probe instantiates EJ2 Grid directly, selects the first row, starts normal
edit mode, records the `beginEdit` payload, then runs a second Grid instance
where the handler sets `args.cancel = true` and verifies edit mode does not
start.

The deterministic trace file hash from this pass:

`b407faa560e6e996c0eded21088a7bb1386578e14f8939da745b49520655d4ac`

## Observed Payload

The normal edit variant emitted these own keys:

| Key | Observed type | Observed sample | Mapping status |
| --- | --- | --- | --- |
| `cancel` | boolean | `false`; writable to `true` | accepted as read flag and mutation source |
| `foreignKeyData` | object | `{}` | discovered but excluded for this row |
| `isScroll` | boolean | `false` | discovered but excluded internal metadata |
| `name` | string | `beginEdit` | discovered but excluded as event identity metadata for this row |
| `primaryKey` | array | `["id"]` | discovered but excluded duplicate key metadata |
| `primaryKeyValue` | array | `[1]` | discovered but excluded duplicate key metadata |
| `requestType` | string | `beginEdit` | discovered but excluded as event identity metadata for this row |
| `row` | DOM row | edited `TR` | excluded browser-owned object |
| `rowData` | object | row DTO before edit | accepted as generic typed row DTO |
| `rowIndex` | number | `0` | accepted row coordinate |
| `target` | undefined | undefined | excluded |
| `type` | string | `edit` | accepted edit-mode metadata |

## C# DSL Judgment Boundary

Public C# accepts the row data, row index, edit type, cancel read flag, and
cancel mutation method:

- `FusionGridBeginEditArgs<TRow>.RowData`
- `FusionGridBeginEditArgs<TRow>.RowIndex`
- `FusionGridBeginEditArgs<TRow>.Type`
- `FusionGridBeginEditArgs<TRow>.Cancel`
- `FusionGridBeginEditArgs<TRow>.Cancel()`

Do not add public `Row`, `ForeignKeyData`, `IsScroll`, `Name`, `PrimaryKey`,
`PrimaryKeyValue`, `RequestType`, or `Target` from this row. They are
browser-owned objects, duplicate key/event metadata, internal metadata, absent
values, or lack a clear C# DSL behavior use case for this variant.

## Current C# Audit

Current files:

- `Alis.Reactive.Fusion/Components/FusionGrid/Events/FusionGridOnEditing.cs`
- `Alis.Reactive.Fusion/Components/FusionGrid/FusionGridEvents.cs`

Current contract covers the accepted fields above. Focused typed DSL proof
proves visible reads and the `Cancel()` mutation for this normal-edit row.

## Typed DSL Proof

Completed proof:

`scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridBeginEditNormal.begin_edit_normal_reads_row_data_and_can_cancel_edit"`

Result: passed, TRX
`tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-225913.trx`.

The proof selects a real Grid row, starts normal edit mode, asserts visible
`RowIndex`, `Type`, `Cancel`, and `RowData.ResidentName` from the typed
`beginEdit` payload, triggers a second begin-edit path that calls
`args.Cancel(t)`, asserts edit mode is prevented, asserts excluded public
members are absent from the typed C# contract, and asserts no console errors.
