# Grid Event Row: cellSaved Batch Edit

Status: raw EJ2 discovery, judgment, and focused typed DSL Playwright proof are
complete for this batch-edit row. The component audit remains open.

## Row Boundary

`cellSaved` fired after saving a changed cell in batch edit mode.

This row covers only an existing-row batch cell edit for the `openTasks`
numeric column through `grid.editCell(0, "openTasks")`, user value entry, and
`grid.saveCell()`. It must not be treated as equivalent to `cellSave`: this
event fires after the cell value has already been accepted into batch changes.
Other edit modes, add-row batch editing, foreign-key columns, validation
failure, keyboard navigation, frozen columns, and other trigger variants require
separate focused rows.

## Evidence

- Syncfusion local `CellSaveArgs` type source: `node_modules/@syncfusion/ej2-grids/src/grid/base/interface.d.ts:2313`
- Syncfusion local public `editCell` method source: `node_modules/@syncfusion/ej2-grids/src/grid/base/grid.d.ts:3349`
- Syncfusion local public `saveCell` method source: `node_modules/@syncfusion/ej2-grids/src/grid/base/grid.d.ts:3356`
- Batch edit probe: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/probes/raw-ej2-cell-saved-batch-edit.html`
- Batch edit trace: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/traces/raw-ej2-cell-saved-batch-edit.trace.json`
- Judgment calls: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/discovery/judgment-calls-cell-saved-batch-edit.md`

## Discovery Result

The probe instantiates EJ2 Grid directly, enters batch edit mode for the first
row's `openTasks` cell, changes the value from `2` to `6`, calls `saveCell`,
and records the `cellSaved` payload. It then edits the same cell from `6` to
`8`, records the `cellSaved` payload before and after setting
`args.cancel = true`, and records the resulting visible row and
`getBatchChanges()` output.

The deterministic trace file hash from this pass:

`a7c2edd8f9a9ee470a867b68d67a306dcf1c5f4c7ae5e4a2e2de1681c3be31db`

## Observed Payload

The batch-edit variant emitted these own keys:

| Key | Observed type | Observed sample | Mapping status |
| --- | --- | --- | --- |
| `cancel` | boolean | `false`; writable to `true` after handler mutation, but saved value remains accepted | excluded from public post-save C# contract |
| `cell` | DOM cell | edited `TD` | excluded browser-owned object |
| `column` | object | column metadata for `openTasks` | discovered but excluded for this row |
| `columnName` | string | `openTasks` | accepted as edited field name |
| `columnObject` | object | duplicate column metadata for `openTasks` | discovered but excluded for this row |
| `isForeignKey` | boolean | `false` | discovered but excluded for this row |
| `name` | string | `cellSaved` | discovered but excluded as event identity metadata for this row |
| `previousValue` | number | `2` and `6` in raw trace | accepted as typed previous value |
| `rowData` | object | row DTO before the post-save event | accepted as generic typed row DTO |
| `value` | number | `6` and `8` in raw trace | accepted as typed saved cell value |

## C# DSL Judgment Boundary

Public C# accepts only stable post-save reads:

- `FusionGridCellSavedArgs<TRow, TValue>.RowData`
- `FusionGridCellSavedArgs<TRow, TValue>.ColumnName`
- `FusionGridCellSavedArgs<TRow, TValue>.Value`
- `FusionGridCellSavedArgs<TRow, TValue>.PreviousValue`

Do not expose public `Cancel` or `Cancel()` for `cellSaved`. The raw trace shows
that setting `args.cancel = true` inside `cellSaved` mutates the payload object
but does not prevent the saved value: `cancelPreventedSavedValue=false`,
visible first-row value is `8`, and `getBatchChanges().changedRecords[0]`
contains `openTasks=8`.

Do not add public `Cell`, `Column`, `ColumnObject`, `IsForeignKey`, or `Name`
from this row. They are browser-owned objects, Syncfusion column metadata,
foreign-key variant metadata, or duplicate event identity metadata without a
clear typed Senior Living workflow in this variant.

## Current C# Audit

Current files:

- `Alis.Reactive.Fusion/Components/FusionGrid/Events/FusionGridOnEditing.cs`
- `Alis.Reactive.Fusion/Components/FusionGrid/FusionGridEvents.cs`
- `Alis.Reactive.Fusion/Components/FusionGrid/FusionGridEditingExtensions.cs`

The previous shared `FusionGridCellSaveArgs<TRow, TValue>` contract was too
broad for `cellSaved` because it exposed `Cancel()` for a post-save event. This
row requires a separate `FusionGridCellSavedArgs<TRow, TValue>` contract.

## Typed DSL Proof

Completed proof:

`dotnet build tests/Alis.Reactive.PlaywrightTests/Alis.Reactive.PlaywrightTests.csproj -c Debug`
then
`scripts/playwright.sh --no-build --filter "FullyQualifiedName=Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridEditing.batch_editing_exposes_cell_events_indexed_batch_payload_and_batch_changes_source"`

Result: passed, TRX
`tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260607-023421.trx`.

The proof loads a real Fusion Grid page, enters batch edit mode through the
typed `EditCell` method, saves a changed `openTasks` value through the typed
`SaveCell` method, asserts visible `CellSaved` reads for `ColumnName`, `Value`,
`PreviousValue`, and `RowData.ResidentName`, and asserts excluded public
members including `Cancel` and `Cancel()` are absent from the typed C#
contract.
