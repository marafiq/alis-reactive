# Grid Judgment Calls: rowSelected Click

Status: row decision recorded. The component audit remains open.

## Decision Summary

The `rowSelected` click row is accepted as a typed event source for row data and
selection scalars. Discovery records every observed EJ2 payload member, but the
public C# DSL remains selective.

## Accepted Public C# Surface

| EJ2 payload | C# member | Reason |
| --- | --- | --- |
| `data` | `FusionGridRowSelectedArgs<TRow>.Data` | exact row DTO selected by the event; typed and useful for conditions, gathers, and visible text |
| `rowIndex` | `FusionGridRowSelectedArgs<TRow>.RowIndex` | stable scalar selection coordinate |
| `previousRowIndex` | `FusionGridRowSelectedArgs<TRow>.PreviousRowIndex` | useful scalar for selection transitions; nullable because first selection emits undefined |
| `isInteracted` | `FusionGridRowSelectedArgs<TRow>.IsInteracted` | stable boolean proving user interaction for click-triggered selection |
| `name` | `FusionGridRowSelectedArgs<TRow>.Name` | stable event metadata observed in both click variants |

## Excluded From Public C# Surface

| EJ2 payload | Reason |
| --- | --- |
| `row` | DOM `TR`; browser-owned object |
| `previousRow` | DOM `TR` or undefined; browser-owned object |
| `target` | DOM `TD`; browser-owned object |
| `foreignKeyData` | empty object in this row; foreign-key row required before useful typed contract |
| `isHeaderCheckBoxClicked` | checkbox-specific variant; not part of normal row-click selection |
| `rowIndexes` | duplicate scalar for this single-selection row; multiple/range selection row required before array semantics are accepted |

## No Primitive Change

Existing event payload read primitives cover every accepted field:

- `data.<member>` is a nested typed event payload read.
- `rowIndex`, `previousRowIndex`, `isInteracted`, and `name` are scalar event
  payload reads.
- The proof includes an async request only because the row uses
  `Post(...).Gather(...)`; no new gather primitive is needed.

## Variant Boundary

The first click proves `previousRowIndex` can be undefined. The second click
proves it can be numeric. The C# contract must preserve that variant by using a
nullable scalar rather than a non-nullable `int`.
