# Grid Judgment Calls: actionBegin Save Edit

Status: variant row decision recorded. The component audit remains open.

## Decision Summary

The `actionBegin` save/edit variant is accepted as a typed event source for
current row data, previous row data, save/edit metadata, and stable scalar row
coordinates. Discovery records every observed EJ2 payload member, but the public
C# DSL remains selective.

## Accepted Public C# Surface For This Variant

| EJ2 payload | C# member | Reason |
| --- | --- | --- |
| `name` | `FusionGridEditActionArgs<TRow>.Name` | stable event metadata observed in the trace |
| `requestType` | `FusionGridEditActionArgs<TRow>.RequestType` | `save` identifies the edit-save lifecycle point |
| `action` | `FusionGridEditActionArgs<TRow>.Action` | `edit` distinguishes save-edit from save-add |
| `type` | `FusionGridEditActionArgs<TRow>.Type` | Syncfusion action event type metadata |
| `cancel` | `FusionGridEditActionArgs<TRow>.Cancel` | observed flag; read is proven |
| `data` | `FusionGridEditActionArgs<TRow>.Data` | edited row DTO; typed and useful for conditions/gathers/status |
| `previousData` | `FusionGridEditActionArgs<TRow>.PreviousData` | original row DTO; typed and useful for before/after workflows |
| `rowIndex` | `FusionGridEditActionArgs<TRow>.RowIndex` | stable row coordinate in this variant |
| `selectedRow` | `FusionGridEditActionArgs<TRow>.SelectedRow` | existing public scalar carried by Syncfusion; no new semantics claimed |

## Excluded From Public C# Surface

| EJ2 payload | Reason |
| --- | --- |
| `row` | browser-owned DOM `TR` |
| `form` | browser-owned DOM `FORM` |
| `target` | undefined in this row; no typed use case |
| `foreignKeyData` | empty object; foreign-key row required |
| `isScroll` | internal virtualization/scroll metadata |
| `primaryKey` | discovered but no clear behavior use case for this row |
| `primaryKeyValue` | discovered but no clear behavior use case for this row |
| `rowData` | duplicates original row data already covered by `previousData` |
| `index` | not emitted as an own key in this save/edit trace; removed from public C# until a focused variant proves it |

## No Primitive Change

Existing event payload read primitives cover every accepted field:

- `data.<member>` and `previousData.<member>` are nested typed event payload reads.
- `requestType`, `action`, `type`, `name`, `cancel`, `rowIndex`, and
  `selectedRow` are scalar event payload reads.

Cancel mutation/default-action prevention is not proven by this row.

## Judgment Questions Applied

- Does this member support a realistic Senior Living workflow that is common
  enough to justify public typed DSL? Accepted fields support edit-save
  workflows such as resident directory corrections, billing row updates, care
  task edits, occupancy status maintenance, and before/after audit messaging.
- Is the member stable and predictable from the EJ2 trace for this specific
  variant? Accepted fields are own payload values with scalar or typed-row DTO
  shape. DOM objects, empty/internal metadata, duplicate original data, and
  absent fields are not accepted.
- Can C# express the member without stringly access or DOM leakage? `data` and
  `previousData` map to the generic row DTO; metadata fields map to scalar
  properties. Browser-owned `row`, `form`, and `target` do not.
- Does onboarding the member give developers a clear behavior they can prove in
  a vertical slice? Accepted fields are asserted through visible behavior in the
  ActionBegin save/edit Playwright row. Excluded fields are proven absent from
  the public typed contract in the same behavior row.
- Would adding the member pollute the DSL? The excluded members stay in
  discovery but out of C# until a focused variant proves a predictable typed use
  case.

## Matrix Boundary

`ActionBegin` can be linked to this variant row, but
`FusionGridEditActionArgs<TRow>` must remain unproven in the typed API coverage
matrix until all accepted public members and edit-action variants that share the
payload type are proven. `Index` is not an accepted public member for this row.
