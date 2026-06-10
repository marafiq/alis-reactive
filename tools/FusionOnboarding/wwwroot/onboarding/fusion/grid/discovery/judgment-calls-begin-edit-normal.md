# Grid Judgment Calls: beginEdit Normal Edit

Status: variant row decision recorded. Focused typed DSL proof passed for this
normal-edit row. The component audit remains open.

## Decision Summary

The `beginEdit` normal-edit variant is accepted as a typed event source for the
row entering edit mode, the visible row coordinate, edit-mode metadata, and
cancel/default-action prevention. Discovery records every observed EJ2 payload
member, but the public C# DSL remains selective.

## Accepted Public C# Surface For This Variant

| EJ2 payload | C# member | Reason |
| --- | --- | --- |
| `rowData` | `FusionGridBeginEditArgs<TRow>.RowData` | typed row DTO before edit; useful for edit-gating conditions and visible context |
| `rowIndex` | `FusionGridBeginEditArgs<TRow>.RowIndex` | stable row coordinate in this variant |
| `type` | `FusionGridBeginEditArgs<TRow>.Type` | stable edit-mode metadata observed in trace and current public contract |
| `cancel` | `FusionGridBeginEditArgs<TRow>.Cancel` | observed flag; read is useful for visible audit/debug output |
| writable `cancel` | `FusionGridBeginEditArgs<TRow>.Cancel()` | raw probe proves setting `args.cancel = true` prevents the edited row from rendering |

## Excluded From Public C# Surface

| EJ2 payload | Reason |
| --- | --- |
| `row` | browser-owned DOM `TR` |
| `foreignKeyData` | empty object; foreign-key row required |
| `isScroll` | internal virtualization/scroll metadata |
| `name` | duplicate event identity metadata; no clear typed behavior use case for this row |
| `primaryKey` | duplicates typed row identity already available through `rowData` |
| `primaryKeyValue` | duplicates typed row identity already available through `rowData` |
| `requestType` | duplicate event identity metadata; this event row is already `beginEdit` |
| `target` | undefined in this row; no typed use case |

## No Primitive Change

Existing primitives cover every accepted field:

- `rowData.<member>` is a nested typed event payload read.
- `rowIndex`, `type`, and `cancel` are scalar event payload reads.
- `Cancel()` maps to payload mutation through existing event payload set
  primitive: `PayloadSource.Event()` path `cancel` literal `true`.

## Judgment Questions Applied

- Does this member support a realistic Senior Living workflow common enough to
  justify public typed DSL? Accepted fields support edit gating for resident
  directories, billing rows, care/task rows, occupancy rows, and staff
  operations where records may be locked, discharged, billed, or otherwise not
  editable.
- Is the member stable and predictable from the EJ2 trace for this variant?
  Accepted fields are own payload values with scalar or typed-row DTO shape;
  cancel mutation is proven by visible absence of an edited row.
- Can C# express the member without stringly access or DOM leakage? `rowData`
  maps to the generic row DTO; scalar fields map directly; `row` remains
  browser-owned.
- Does onboarding the member give developers a clear behavior they can prove in
  a vertical slice? Accepted reads can drive visible text and conditions;
  `Cancel()` can prevent edit mode for a locked row.
- Would adding the member pollute the DSL? Duplicate event metadata, key arrays,
  DOM objects, empty foreign-key metadata, and undefined values stay in
  discovery but out of public C# until focused variants prove a predictable
  typed use case.

## Matrix Boundary

The focused typed DSL proof links `BeginEdit` to this variant row for normal
edit of an existing row. The shared public `FusionGridBeginEditArgs<TRow>`
contract remains fail-closed for other `beginEdit` variants until each variant
is discovered, judged, and proven separately.
