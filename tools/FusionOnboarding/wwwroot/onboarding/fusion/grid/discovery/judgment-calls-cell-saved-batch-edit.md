# Grid Judgment Calls: cellSaved Batch Edit

Status: variant row decision recorded. Focused typed DSL proof passed for this
batch-edit row. The component audit remains open.

## Decision Summary

The `cellSaved` batch-edit variant is accepted as a typed post-save event source
for the row being edited, edited column name, saved value, and previous value.
Discovery records every observed EJ2 payload member, but the public C# DSL
stays selective. `cancel` is deliberately excluded from the post-save public
contract because the raw trace proves mutation does not prevent the saved value.

## Accepted Public C# Surface For This Variant

| EJ2 payload | C# member | Reason |
| --- | --- | --- |
| `rowData` | `FusionGridCellSavedArgs<TRow, TValue>.RowData` | typed row DTO before the post-save event; useful for audit output and row-specific follow-up reactions |
| `columnName` | `FusionGridCellSavedArgs<TRow, TValue>.ColumnName` | stable field identity for care/task/billing edit workflows |
| `value` | `FusionGridCellSavedArgs<TRow, TValue>.Value` | typed saved cell value; useful for post-save feedback and audit workflows |
| `previousValue` | `FusionGridCellSavedArgs<TRow, TValue>.PreviousValue` | typed previous cell value; useful for before/after audit workflows |

## Excluded From Public C# Surface

| EJ2 payload | Reason |
| --- | --- |
| `cancel` | post-save lifecycle flag is not behaviorally useful; raw trace proves setting it to `true` after save does not prevent `openTasks=8` from being accepted |
| writable `cancel` | no public `Cancel()` for `cellSaved`; mutation is too late in the lifecycle and would mislead developers |
| `cell` | browser-owned DOM `TD` |
| `column` | Syncfusion column object; no focused typed Senior Living behavior need in this row |
| `columnObject` | duplicate Syncfusion column object; no focused typed Senior Living behavior need in this row |
| `isForeignKey` | `false` in this row; foreign-key column row required before accepting any typed member |
| `name` | duplicate event identity metadata; event selector already owns `cellSaved` |

## No Primitive Change

Existing primitives cover every accepted field:

- `rowData.<member>` is a nested typed event payload read.
- `columnName`, `value`, and `previousValue` are scalar event payload reads.

No payload mutation primitive is accepted for this row. The existing primitive
still supports cancellable events such as `cellSave`, but this row proves
`cellSaved` must not expose it.

## Judgment Questions Applied

- Does this member support a realistic Senior Living workflow common enough to
  justify public typed DSL? Accepted fields support audit trails, visible
  follow-up status, task-value history, billing adjustment review, and resident
  directory edit feedback after Syncfusion accepts a cell value.
- Is the member stable and predictable from the EJ2 trace for this variant?
  Accepted fields are own payload values with scalar or typed-row DTO shape.
- Can C# express the member without stringly access or DOM leakage? `rowData`
  maps to the generic row DTO; scalar fields map directly; `cell`, `column`,
  and `columnObject` stay out of public C#.
- Does onboarding the member give developers a clear behavior they can prove in
  a vertical slice? Accepted reads drive visible UI after a real batch save.
- Would adding the member pollute the DSL? Yes for `Cancel`/`Cancel()` on this
  post-save event, because the raw trace proves it is mutable but not
  behaviorally useful. DOM objects, Syncfusion column objects, foreign-key
  metadata not exercised by this row, and duplicate event identity metadata
  also stay out of public C#.

## Matrix Boundary

The focused typed DSL proof will link `CellSaved` to this variant row for batch
edit of an existing numeric cell. The shared public
`FusionGridCellSavedArgs<TRow, TValue>` contract remains fail-closed for other
`cellSaved` variants until each variant is discovered, judged, and proven
separately.
