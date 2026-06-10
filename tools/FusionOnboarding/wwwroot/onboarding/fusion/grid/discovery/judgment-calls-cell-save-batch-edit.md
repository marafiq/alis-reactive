# Grid Judgment Calls: cellSave Batch Edit

Status: variant row decision recorded. Focused typed DSL proof passed for this
batch-edit row. The component audit remains open.

## Decision Summary

The `cellSave` batch-edit variant is accepted as a typed event source for the
row being edited, edited column name, current value, previous value, and
cancel/default-action prevention. Discovery records every observed EJ2 payload
member, but the public C# DSL remains selective.

## Accepted Public C# Surface For This Variant

| EJ2 payload | C# member | Reason |
| --- | --- | --- |
| `rowData` | `FusionGridCellSaveArgs<TRow, TValue>.RowData` | typed row DTO before the cell save; useful for row-specific edit policy and audit output |
| `columnName` | `FusionGridCellSaveArgs<TRow, TValue>.ColumnName` | stable field identity for care/task/billing edit workflows |
| `value` | `FusionGridCellSaveArgs<TRow, TValue>.Value` | typed new cell value; useful for policy checks and visible feedback |
| `previousValue` | `FusionGridCellSaveArgs<TRow, TValue>.PreviousValue` | typed previous cell value; useful for before/after audit workflows |
| `cancel` | `FusionGridCellSaveArgs<TRow, TValue>.Cancel` | observed flag; read is useful for visible audit/debug output |
| writable `cancel` | `FusionGridCellSaveArgs<TRow, TValue>.Cancel()` | raw EJ2 probe and typed DSL proof both show setting `args.cancel = true` prevents the blocked cell value from being accepted |

## Excluded From Public C# Surface

| EJ2 payload | Reason |
| --- | --- |
| `cell` | browser-owned DOM `TD` |
| `column` | Syncfusion column object; no focused typed Senior Living behavior need in this row |
| `columnObject` | duplicate Syncfusion column object; no focused typed Senior Living behavior need in this row |
| `isForeignKey` | `false` in this row; foreign-key column row required before accepting any typed member |
| `name` | duplicate event identity metadata; event selector already owns `cellSave` |

## No Primitive Change

Existing primitives cover every accepted field:

- `rowData.<member>` is a nested typed event payload read.
- `columnName`, `value`, `previousValue`, and `cancel` are scalar event payload
  reads.
- `Cancel()` maps to payload mutation through existing event payload set
  primitive: `PayloadSource.Event()` path `cancel` literal `true`.

## Judgment Questions Applied

- Does this member support a realistic Senior Living workflow common enough to
  justify public typed DSL? Accepted fields support care task edits, medication
  administration worklists, billing adjustments, occupancy row maintenance, and
  resident directory edits where the row, edited field, new value, previous
  value, and prevent-save behavior are useful.
- Is the member stable and predictable from the EJ2 trace for this variant?
  Accepted fields are own payload values with scalar or typed-row DTO shape;
  cancel mutation is proven by raw `args.cancel = true` trace rows and
  `blockedValueAccepted=false`.
- Can C# express the member without stringly access or DOM leakage? `rowData`
  maps to the generic row DTO; scalar fields map directly; `cell`, `column`,
  and `columnObject` stay out of public C#.
- Does onboarding the member give developers a clear behavior they can prove in
  a vertical slice? Accepted reads drive visible UI and `Cancel()` prevents a
  blocked task count from being accepted in both raw EJ2 and typed DSL proof.
- Would adding the member pollute the DSL? DOM objects, Syncfusion column
  objects, foreign-key metadata not exercised by this row, and duplicate event
  identity metadata stay in discovery but out of public C# until a focused row
  proves a predictable typed use case.

## Matrix Boundary

The focused typed DSL proof links `CellSave` to this variant row for batch edit
of an existing numeric cell. The shared public
`FusionGridCellSaveArgs<TRow, TValue>` contract remains fail-closed for other
`cellSave` variants until each variant is discovered, judged, and proven
separately.
