# Grid Judgment Calls: toolbarClick Custom Item

Status: row decision recorded. The component audit remains open.

## Decision Summary

The `toolbarClick` custom-item row is accepted as a typed event source for
command identity and event metadata. Discovery records the full top-level EJ2
payload and the useful item fields, but the public C# DSL remains selective.

## Accepted Public C# Surface

| EJ2 payload | C# member | Reason |
| --- | --- | --- |
| `item.id` | `FusionGridToolbarItem.Id` | stable command identity; already used for branch logic |
| `item.text` | `FusionGridToolbarItem.Text` | stable display text; useful for visible status and diagnostics |
| `cancel` | `FusionGridToolbarClickArgs.Cancel` | observed flag carried by EJ2; read is proven |
| `name` | `FusionGridToolbarClickArgs.Name` | stable event metadata observed in trace |

## Excluded From Public C# Surface

| EJ2 payload | Reason |
| --- | --- |
| `originalEvent` | browser-owned `PointerEvent`; exposing it would pollute the C# DSL |
| `item.tooltipText` | item configuration metadata; no focused behavior use case in this row |
| `item.prefixIcon` | presentation metadata; no focused behavior use case in this row |
| `item.suffixIcon` | presentation metadata; no focused behavior use case in this row |
| `item.disabled` | state/config metadata; disabled-item row required before useful typed contract |
| `item.visible` | state/config metadata; visibility row required before useful typed contract |
| `item.type` | toolbar rendering metadata; no focused behavior use case in this row |
| `item.align` | toolbar layout metadata; no focused behavior use case in this row |

## No Primitive Change

Existing event payload read primitives cover every accepted field:

- `item.id` and `item.text` are nested event payload reads.
- `cancel` is a boolean event payload read.
- `name` is a string event payload read.

No mapped row currently requires a new primitive. Cancel mutation/default-action
prevention is not proven by this row.

## Variant Boundary

This row is custom toolbar item only. Built-in toolbar items such as Search,
ColumnChooser, Print, ExcelExport, PdfExport, and responsive toolbar overflow
must be separate rows because `toolbar.js` applies default action behavior after
the event callback and may depend on `cancel`.
