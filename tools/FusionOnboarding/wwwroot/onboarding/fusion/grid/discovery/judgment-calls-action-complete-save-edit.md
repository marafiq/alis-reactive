# Grid Judgment Calls: actionComplete Save Edit

Status: variant row decision recorded. The component audit remains open.

## Decision Summary

The `actionComplete` save/edit variant is accepted as a typed event source for
current row data, previous row data, save/edit metadata, and stable scalar row
coordinates. Discovery records every observed EJ2 payload member, but the public
C# DSL remains selective.

## Judgment Questions Applied

- Senior Living 95% use-case value: current row, previous row, save/edit phase,
  cancel flag, and row coordinates support realistic resident directory,
  billing, schedule, staff operations, and remote-data edit workflows where the
  app displays or gathers before/after values after a save.
- Stability and predictability: accepted members are scalar metadata or typed
  row DTOs observed in the raw save/edit trace; DOM objects, undefined members,
  empty foreign-key metadata, duplicate row data, and internal scroll/key
  metadata are not stable public C# values for this row.
- Primitive fit: accepted members use existing event payload read primitives;
  no new primitive is needed or justified.
- Developer actionability: accepted values can drive conditions, visible
  status, request gathers, and audit-style before/after workflows. Excluded
  members do not add clear developer behavior in this variant.
- DSL pollution risk: broad DOM/internal/duplicate members would make the
  public typed DSL noisier without improving common Senior Living workflows, so
  they remain discovery evidence only.

## Accepted Public C# Surface For This Variant

| EJ2 payload | C# member | Reason |
| --- | --- | --- |
| `name` | `FusionGridEditActionArgs<TRow>.Name` | stable event metadata observed in the trace |
| `requestType` | `FusionGridEditActionArgs<TRow>.RequestType` | `save` identifies the edit-save lifecycle point |
| `action` | `FusionGridEditActionArgs<TRow>.Action` | `edit` distinguishes save-edit from save-add |
| `type` | `FusionGridEditActionArgs<TRow>.Type` | Syncfusion action event type metadata |
| `cancel` | `FusionGridEditActionArgs<TRow>.Cancel` | observed flag; read is proven by focused Playwright |
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
| `index` | not emitted as an own key in this save/edit trace; do not reintroduce public `Index` |
| `promise` | emitted as an own key but value is undefined; no predictable typed C# behavior for this row |

## No Primitive Change

Existing event payload read primitives cover every accepted field:

- `data.<member>` and `previousData.<member>` are nested typed event payload reads.
- `requestType`, `action`, `type`, `name`, `cancel`, `rowIndex`, and
  `selectedRow` are scalar event payload reads.

Cancel mutation/default-action prevention is not proven by this row.

## Matrix Boundary

`ActionComplete` is linked to this variant row by the focused typed DSL
Playwright proof. `FusionGridEditActionArgs<TRow>` must remain unproven in the
typed API coverage matrix until all accepted public members and edit-action
variants that share the payload type are proven. `Index` and `Promise` are not
accepted public members for this row.
