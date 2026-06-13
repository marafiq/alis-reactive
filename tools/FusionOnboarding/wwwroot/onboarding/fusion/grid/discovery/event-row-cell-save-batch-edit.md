# Grid Event Row: cellSave Batch Edit

Status: raw EJ2 discovery, judgment, and focused typed DSL Playwright proof are
complete for this batch-edit row. The component audit remains open.

## Row Boundary

`cellSave` fired by saving a changed cell in batch edit mode.

This row covers only an existing-row batch cell edit for the `openTasks`
numeric column through `grid.editCell(0, "openTasks")`, user value entry, and
`grid.saveCell()`. Cell edit start, `cellSaved`, add-row batch editing,
foreign-key columns, validation failure, keyboard navigation, frozen columns,
and other edit modes require separate focused rows before their payloads can be
mapped or claimed equivalent.

## Evidence

- Syncfusion local `CellEditSameArgs` type source: `node_modules/@syncfusion/ej2-grids/src/grid/base/interface.d.ts:2254`
- Syncfusion local `CellSaveArgs` type source: `node_modules/@syncfusion/ej2-grids/src/grid/base/interface.d.ts:2313`
- Syncfusion local public `editCell` method source: `node_modules/@syncfusion/ej2-grids/src/grid/base/grid.d.ts:3349`
- Syncfusion local public `saveCell` method source: `node_modules/@syncfusion/ej2-grids/src/grid/base/grid.d.ts:3356`
- Batch edit probe: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/probes/raw-ej2-cell-save-batch-edit.html`
- Batch edit trace: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/traces/raw-ej2-cell-save-batch-edit.trace.json`
- Judgment calls: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/discovery/judgment-calls-cell-save-batch-edit.md`

## Discovery Result

The probe instantiates EJ2 Grid directly, enters batch edit mode for the first
row's `openTasks` cell, changes the value from `2` to `6`, calls `saveCell`,
records the `cellSave` payload, then records the resulting visible row and
`getBatchChanges()` output. It then enters the same cell again, changes the
value from `6` to `99`, records the `cellSave` payload before and after setting
`args.cancel = true`, and verifies the blocked value was not accepted into
`getBatchChanges()`.

The deterministic trace file hash from this pass:

`c612901fff750b54c961a3d1c7445fea15da562f0f5b6923c3789920b299538f`

## Observed Payload

The batch-edit variant emitted these own keys:

| Key | Observed type | Observed sample | Mapping status |
| --- | --- | --- | --- |
| `cancel` | boolean | `false`; after handler mutation `true`; blocked value not accepted | accepted as read flag and mutation source |
| `cell` | DOM cell | edited `TD` | excluded browser-owned object |
| `column` | object | column metadata for `openTasks` | discovered but excluded for this row |
| `columnName` | string | `openTasks` | accepted as edited field name |
| `columnObject` | object | duplicate column metadata for `openTasks` | discovered but excluded for this row |
| `isForeignKey` | boolean | `false` | discovered but excluded for this row |
| `name` | string | `cellSave` | discovered but excluded as event identity metadata for this row |
| `previousValue` | number | `2` and `6` in raw trace; `0` and `6` in sandbox proof | accepted as typed previous value |
| `rowData` | object | row DTO before cell save | accepted as generic typed row DTO |
| `value` | number | `6` and blocked `99` in raw trace; `4` and blocked `99` in sandbox proof | accepted as typed cell value |

## C# DSL Judgment Boundary

Public C# accepts the row data, edited column name, current value, previous
value, cancel read flag, and cancel mutation method:

- `FusionGridCellSaveArgs<TRow, TValue>.RowData`
- `FusionGridCellSaveArgs<TRow, TValue>.ColumnName`
- `FusionGridCellSaveArgs<TRow, TValue>.Value`
- `FusionGridCellSaveArgs<TRow, TValue>.PreviousValue`
- `FusionGridCellSaveArgs<TRow, TValue>.Cancel`
- `FusionGridCellSaveArgs<TRow, TValue>.Cancel()`

Do not add public `Cell`, `Column`, `ColumnObject`, `IsForeignKey`, or `Name`
from this row. They are browser-owned objects, Syncfusion column metadata,
foreign-key variant metadata, or duplicate event identity metadata without a
clear typed Senior Living workflow in this variant.

## Current C# Audit

Current files:

- `Alis.Reactive.Fusion/Components/FusionGrid/Events/FusionGridOnEditing.cs`
- `Alis.Reactive.Fusion/Components/FusionGrid/FusionGridEvents.cs`
- `Alis.Reactive.Fusion/Components/FusionGrid/FusionGridEditingExtensions.cs`

Current contract covers the accepted fields above. Focused typed DSL proof
proves visible reads, batch-change behavior, and the `Cancel()` mutation for
this batch-edit row.

## Typed DSL Proof

Completed proof:

`scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridEditing.batch_editing_exposes_cell_events_indexed_batch_payload_and_batch_changes_source"`

Result: passed, TRX
`tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-235426.trx`.

The proof loads a real Fusion Grid page, enters batch edit mode through the
typed `EditCell` method, saves a changed `openTasks` value through the typed
`SaveCell` method, asserts visible `ColumnName`, `Value`, `PreviousValue`,
`RowData.ResidentName`, and `Cancel`, asserts batch changes can be gathered
after the save, then starts a second edit where value `99` triggers
`args.Cancel(t)` and verifies the visible Grid does not accept `99`. The same
test asserts excluded public members are absent from the typed C# contract and
asserts no console errors.
