# Grid Judgment Calls: beforeBatchSave Batch Edit

Status: variant row decision recorded. Focused typed DSL proof passed for this
batch-edit commit row. The component audit remains open.

## Decision Summary

The `beforeBatchSave` batch-edit commit variant is accepted as a typed event
source for pending batch changes and commit cancellation. Discovery records
every observed EJ2 payload member, but the public C# DSL remains selective.

## Accepted Public C# Surface For This Variant

| EJ2 payload | C# member | Reason |
| --- | --- | --- |
| `batchChanges` | `FusionGridBeforeBatchSaveArgs<TRow>.BatchChanges` | typed added/changed/deleted records are the primary before-commit review workflow |
| `cancel` | `FusionGridBeforeBatchSaveArgs<TRow>.Cancel` | observed flag; useful for visible audit/debug output |
| writable `cancel` | `FusionGridBeforeBatchSaveArgs<TRow>.Cancel()` | raw EJ2 probe proves setting `args.cancel = true` prevents the bulk-save commit lifecycle |

## Excluded From Public C# Surface

| EJ2 payload | Reason |
| --- | --- |
| `name` | duplicate event identity metadata; event selector already owns `beforeBatchSave` |

## No Primitive Change

Existing primitives cover every accepted field:

- `batchChanges.<collection>.<member>` is a nested typed event payload read
  through the existing event payload source and proper array/list member shape.
- `cancel` is a scalar event payload read.
- `Cancel()` maps to payload mutation through existing event payload set
  primitive: `PayloadSource.Event()` path `cancel` literal `true`.

## Judgment Questions Applied

- Does this member support a realistic Senior Living workflow common enough to
  justify public typed DSL? Accepted fields support pre-commit review for care
  tasks, medication worklists, billing rows, census updates, and resident
  directory maintenance where a batch must be blocked before commit.
- Is the member stable and predictable from the EJ2 trace for this variant?
  Accepted fields are own payload values and the batch-change object uses
  predictable `addedRecords`, `changedRecords`, and `deletedRecords` arrays.
- Can C# express the member without stringly access or DOM leakage?
  `BatchChanges` maps to the existing typed batch-change DTO; `Cancel` maps to
  a scalar flag and an existing payload mutation helper.
- Does onboarding the member give developers a clear behavior they can prove in
  a vertical slice? Accepted reads drive visible UI and `Cancel()` prevents the
  second bulk-save commit from reaching `actionComplete`.
- Would adding the member pollute the DSL? `Name` stays in discovery but out of
  public C# because it duplicates event identity and does not support a clearer
  typed workflow than the event selector.

## Matrix Boundary

The focused typed DSL proof will link `BeforeBatchSave` to this variant row for
batch commit of an existing numeric cell. The shared public
`FusionGridBeforeBatchSaveArgs<TRow>` contract remains fail-closed for other
`beforeBatchSave` variants until each variant is discovered, judged, and proven
separately.
