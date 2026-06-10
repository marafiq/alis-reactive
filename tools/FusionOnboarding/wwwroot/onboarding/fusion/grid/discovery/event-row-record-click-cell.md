# Grid Event Row: recordClick Cell

Status: discovery, mapping, and focused typed DSL Playwright proof passed for
this row. The component audit remains open.

## Row Boundary

`recordClick` fired by clicking a visible data cell.

This row covers only a normal data-cell click in an ungrouped Grid. Row-template
clicks, command-column clicks, checkbox cells, grouped rows, frozen columns,
virtualization, foreign-key data, and double-click behavior require separate
focused rows before their payloads can be mapped or claimed equivalent.

## Evidence

- Syncfusion local `recordClick` type source: `node_modules/@syncfusion/ej2-grids/src/grid/base/interface.d.ts:1421`
- Syncfusion local `recordClick` event source: `node_modules/@syncfusion/ej2-grids/src/grid/base/grid.js:5384`
- Syncfusion local row-info payload source: `node_modules/@syncfusion/ej2-grids/src/grid/base/grid.js:2517`
- Method probe: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/probes/raw-ej2-record-click-cell.html`
- Method trace: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/traces/raw-ej2-record-click-cell.trace.json`
- Judgment calls: `tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/discovery/judgment-calls-record-click-cell.md`

## Discovery Result

The probe instantiates EJ2 Grid directly, waits for rendered rows, clicks the
first row's `name` cell, and records the `recordClick` payload.

The deterministic trace file hash was stable across reruns:

`8d73e91ecb5d87db467fb37d4b411fd3490678ab0a10061ddf5d5cd9f83ade7f`

## Observed Payload

The cell-click variant emitted these own keys:

| Key | Observed type | Observed sample | Mapping status |
| --- | --- | --- | --- |
| `cancel` | boolean | `false` | discovered but excluded for this row |
| `cell` | DOM element | clicked `TD` | excluded browser-owned object |
| `cellIndex` | number | `1` | accepted |
| `column` | EJ2 column object | field `name`, header `Name`, index `1` | discovered but excluded for this row |
| `event` | MouseEvent | click event | excluded browser-owned object |
| `name` | string | `recordClick` | accepted |
| `row` | DOM element | clicked `TR` | excluded browser-owned object |
| `rowData` | object | `{ id: 1, name: "Alpha", careLevel: "Memory Care", wing: "North" }` | accepted as generic typed row DTO |
| `rowIndex` | number | `0` | accepted |
| `target` | DOM element | clicked `TD` | excluded browser-owned object |

## C# DSL Judgment Boundary

Public C# accepts only the stable typed row-data and scalar coordinates:

- `FusionGridRecordClickArgs<TRow>.RowData`
- `FusionGridRecordClickArgs<TRow>.RowIndex`
- `FusionGridRecordClickArgs<TRow>.CellIndex`
- `FusionGridRecordClickArgs<TRow>.Name`

Do not add public `Cell`, `Row`, `Target`, `Event`, or broad `Column` fields
from this row. They are browser-owned or vendor-owned objects and would pollute
the C# DSL with unbounded shapes.

Do not add `Cancel` from this row. Syncfusion injects it, but the row does not
prove useful cancel behavior and `grid.js` does not read it after
`recordClick`.

## Current C# Audit

Current files:

- `Alis.Reactive.Fusion/Components/FusionGrid/Events/FusionGridOnRecordClick.cs`
- `Alis.Reactive.Fusion/Components/FusionGrid/FusionGridEvents.cs`

Current contract covers the accepted cell-click row:

- `FusionGridEvents.RecordClick<TRow>()` maps event name `recordClick`
- `FusionGridRecordClickArgs<TRow>.RowData` maps `rowData`
- `FusionGridRecordClickArgs<TRow>.RowIndex` maps `rowIndex`
- `FusionGridRecordClickArgs<TRow>.CellIndex` maps `cellIndex`
- `FusionGridRecordClickArgs<TRow>.Name` maps `name`

## Typed DSL Proof

Passed proof:

`scripts/playwright.sh --filter "FullyQualifiedName~Alis.Reactive.PlaywrightTests.Components.Fusion.Grid.WhenUsingFusionGridDirectory.record_click_reads_typed_row_data_and_cell_coordinates"`

TRX: `tests/Alis.Reactive.PlaywrightTests/TestResults/observable/playwright-20260606-181927.trx`

The proof clicks a real rendered Grid cell through the typed Fusion page, reads
the visible cell text as the expected resident, asserts the displayed resident
name comes from `args.RowData.ResidentName`, asserts `args.RowIndex`, asserts
`args.CellIndex`, asserts `args.Name`, and asserts no console errors.
