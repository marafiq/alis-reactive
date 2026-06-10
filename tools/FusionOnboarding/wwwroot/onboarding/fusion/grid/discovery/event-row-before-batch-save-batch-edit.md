# Grid Event Row: beforeBatchSave Batch Edit

Status: raw EJ2 discovery, judgment, and focused typed DSL Playwright proof are
complete for this batch-edit commit row. The component audit remains open.

## Row Boundary

`beforeBatchSave` fired by committing unsaved batch-edit changes.

This row covers only an existing-row batch cell edit for the `openTasks`
numeric column followed by `grid.endEdit()`. It proves the `beforeBatchSave`
payload for pending changed records and the lifecycle effect of setting
`args.cancel = true` before Syncfusion performs the bulk save. Add-row,
delete-row, multiple changed rows, validation failure, toolbar click commit,
keyboard commit, and other edit modes require separate focused rows.

## Evidence

- Syncfusion local `BeforeBatchSaveArgs` type source: `node_modules/@syncfusion/ej2-grids/src/grid/base/interface.d.ts:2109`
- Syncfusion local `beforeBatchSave` trigger source: `node_modules/@syncfusion/ej2-grids/src/grid/actions/batch-edit.js:339`
- Batch edit probe: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/probes/raw-ej2-before-batch-save-batch-edit.html`
- Batch edit trace: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/traces/raw-ej2-before-batch-save-batch-edit.trace.json`
- Judgment calls: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/discovery/judgment-calls-before-batch-save-batch-edit.md`

## Discovery Result

The probe instantiates EJ2 Grid directly, edits the first row's `openTasks`
cell from `2` to `6`, saves the cell into pending batch changes, and calls
`grid.endEdit()`. The first `beforeBatchSave` payload contains one changed
record and `cancel=false`. After the normal commit, `getBatchChanges()` is
empty, the backing `dataSource` contains `openTasks=6`, and one
`actionComplete` event has fired.

The probe then edits the same cell from `6` to `8`, calls `grid.endEdit()`,
records the `beforeBatchSave` payload before and after setting
`args.cancel = true`, and records the resulting visible row, pending batch
changes, and backing `dataSource`.

The deterministic trace file hash from this pass:

`24ccd38a900e58c261ed4b2b573a737a33ef404f58939309f44a98f1c2baf882`

## Observed Payload

The batch-edit commit variant emitted these own keys:

| Key | Observed type | Observed sample | Mapping status |
| --- | --- | --- | --- |
| `batchChanges` | object | `{ addedRecords: [], changedRecords: [{ id: 1, openTasks: 6 }], deletedRecords: [] }` | accepted as typed batch-change DTO |
| `cancel` | boolean | `false`; after handler mutation `true`; second commit prevented | accepted as read flag and mutation source |
| `name` | string | `beforeBatchSave` | discovered but excluded as event identity metadata for this row |

## C# DSL Judgment Boundary

Public C# accepts the batch changes, cancel read flag, and cancel mutation
method:

- `FusionGridBeforeBatchSaveArgs<TRow>.BatchChanges`
- `FusionGridBeforeBatchSaveArgs<TRow>.Cancel`
- `FusionGridBeforeBatchSaveArgs<TRow>.Cancel()`

Do not add public `Name` from this row. It duplicates the event selector and
does not add a focused Senior Living workflow beyond `BeforeBatchSave<TRow>()`.

## Current C# Audit

Current files:

- `Alis.Reactive.Fusion/Components/FusionGrid/Events/FusionGridOnEditing.cs`
- `Alis.Reactive.Fusion/Components/FusionGrid/FusionGridEvents.cs`
- `Alis.Reactive.Fusion/Components/FusionGrid/FusionGridEditingExtensions.cs`

Current contract covers the accepted fields above. Focused typed DSL proof
proves visible reads, the `Cancel()` mutation, and the excluded `Name` member.

## Typed DSL Proof

Completed proof:

`scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridEditing.batch_editing_exposes_cell_events_indexed_batch_payload_and_batch_changes_source"`

Result: passed, TRX
`tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260607-004231.trx`.

The proof loads a real Fusion Grid page, enters batch edit mode through the
typed `EditCell` method, saves a changed `openTasks` value through the typed
`SaveCell` method, commits pending changes through typed `EndEdit`, asserts
visible `BatchChanges.ChangedRecords[0]` reads, then starts a second commit
path where `BeforeBatchSave<TRow>` calls `args.Cancel(t)` and proves the batch
commit lifecycle was stopped. The same proof asserts excluded public members
such as `Name` are absent from the typed C# contract.
