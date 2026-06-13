# Grid Event Row: rowSelected Click

Status: discovery, mapping, and focused typed DSL Playwright proof passed for
this row. The component audit remains open.

## Row Boundary

`rowSelected` fired by selecting visible data rows through real row clicks.

This row covers single row-selection mode for normal data rows. Checkbox
selection, multiple selection, range selection, persisted selection,
virtualized selection, keyboard selection, foreign-key data, and row deselect
require separate focused rows before their payloads can be mapped or claimed
equivalent.

## Evidence

- Syncfusion local `rowSelected` type source: `node_modules/@syncfusion/ej2-grids/src/grid/base/interface.d.ts:1388`
- Syncfusion local selection source: `node_modules/@syncfusion/ej2-grids/src/grid/actions/selection.js:355`
- Syncfusion local selected event trigger source: `node_modules/@syncfusion/ej2-grids/src/grid/actions/selection.js:365`
- Click probe: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/probes/raw-ej2-row-selected-click.html`
- Click trace: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/traces/raw-ej2-row-selected-click.trace.json`
- Judgment calls: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/discovery/judgment-calls-row-selected-click.md`

## Discovery Result

The probe instantiates EJ2 Grid directly, waits for rendered rows, clicks the
first row, then clicks the second row. This records both the first-selection
variant and the later-selection variant where `previousRowIndex` exists.

The deterministic trace file hash was stable across reruns:

`27946a47ff8610fbb0bae2355640876265e3ff722122a708812126bf50f366ed`

## Observed Payload

Both click variants emitted these own keys:

| Key | First click sample | Second click sample | Mapping status |
| --- | --- | --- | --- |
| `data` | `{ id: 1, name: "Alpha", careLevel: "Memory Care", wing: "North" }` | `{ id: 2, name: "Beta", careLevel: "Assisted Living", wing: "South" }` | accepted as generic typed row DTO |
| `foreignKeyData` | `{}` | `{}` | discovered but excluded for this row |
| `isHeaderCheckBoxClicked` | `false` | `false` | discovered but excluded; checkbox row required |
| `isInteracted` | `true` | `true` | accepted for click row |
| `name` | `rowSelected` | `rowSelected` | accepted |
| `previousRow` | undefined | previous `TR` | excluded browser-owned object |
| `previousRowIndex` | undefined | `0` | accepted as nullable scalar |
| `row` | selected `TR` | selected `TR` | excluded browser-owned object |
| `rowIndex` | `0` | `1` | accepted |
| `rowIndexes` | `0` | `1` | excluded duplicate scalar for single-row selection |
| `target` | clicked `TD` | clicked `TD` | excluded browser-owned object |

## C# DSL Judgment Boundary

Public C# accepts only the stable typed row data, scalar coordinates, interaction
flag, and event name:

- `FusionGridRowSelectedArgs<TRow>.Data`
- `FusionGridRowSelectedArgs<TRow>.RowIndex`
- `FusionGridRowSelectedArgs<TRow>.PreviousRowIndex`
- `FusionGridRowSelectedArgs<TRow>.IsInteracted`
- `FusionGridRowSelectedArgs<TRow>.Name`

`PreviousRowIndex` is nullable because the first-selection payload includes the
own key with an undefined value. Mapping it as non-nullable would hide a real
variant discovered in the raw EJ2 trace.

Do not add public `Row`, `PreviousRow`, `Target`, `ForeignKeyData`,
`IsHeaderCheckBoxClicked`, or `RowIndexes` from this row. They are either
browser-owned objects, variant-specific checkbox/foreign-key surfaces, or
duplicates with no separate typed DSL use case in this row.

## Current C# Audit

Current files:

- `Alis.Reactive.Fusion/Components/FusionGrid/Events/FusionGridOnRowSelected.cs`
- `Alis.Reactive.Fusion/Components/FusionGrid/FusionGridEvents.cs`

Current contract covers the accepted click row:

- `FusionGridEvents.RowSelected<TRow>()` maps event name `rowSelected`
- `FusionGridRowSelectedArgs<TRow>.Data` maps `data`
- `FusionGridRowSelectedArgs<TRow>.RowIndex` maps `rowIndex`
- `FusionGridRowSelectedArgs<TRow>.PreviousRowIndex` maps nullable `previousRowIndex`
- `FusionGridRowSelectedArgs<TRow>.IsInteracted` maps `isInteracted`
- `FusionGridRowSelectedArgs<TRow>.Name` maps `name`

## Typed DSL Proof

Passed proof:

`scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.row_selected_reads_typed_row_data_previous_index_and_interaction_state"`

TRX: `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-182350.trx`

The proof clicks two real rendered Grid rows through the typed Fusion page,
reads the expected resident and id from the visible second row, asserts the
second click posts `args.Data.ResidentId` and `args.RowIndex`, asserts visible
`args.Data.ResidentName`, `args.RowIndex`, `args.PreviousRowIndex`,
`args.IsInteracted`, and `args.Name`, asserts the response updates the selected
resident summary, and asserts no console errors.
