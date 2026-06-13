# Grid Runtime Row: Column Visibility & Reorder Methods

Status: vendor evidence committed; typed DSL Playwright proof pending.

Scenario (senior-living): care-staff directory column view — staff hide non-essential
columns, restore them, and reorder to surface what matters for a shift. Lane:
component method — void, 2 string args (authoritative map Lane 2,
`self.EmitCall(ComponentMethod.WithArgs<string,string>, args)`).

## Members (typed Fusion API -> EJ2 call)

| Typed C# | EJ2 call | Source |
| --- | --- | --- |
| `ShowColumn(field)` | `grid.showColumns(fieldName, "field")` | `FusionGridColumnExtensions.cs:24` |
| `HideColumn(field)` | `grid.hideColumns(fieldName, "field")` | `FusionGridColumnExtensions.cs:41` |
| `ReorderColumnBefore(from, before)` | `grid.reorderColumns(fromField, beforeField)` | `FusionGridColumnExtensions.cs:58` |

## Raw EJ2 trace evidence

Probe `probes/raw-ej2-column-visibility.html` -> trace
`traces/raw-ej2-column-visibility.trace.json`:

| Step | visibleFields after |
| --- | --- |
| ready | `id, name, wing, careLevel, riskLevel` |
| `hideColumns("careLevel","field")` | `id, name, wing, riskLevel` (careLevel removed) |
| `showColumns("careLevel","field")` | `id, name, wing, careLevel, riskLevel` (restored) |
| `reorderColumns("riskLevel","wing")` | `id, name, riskLevel, wing, careLevel` (moved before) |

## Finding (recorded — list of reasons)

`ReorderColumnBefore` works only when the grid enables `allowReordering`:

- the EJ2 d.ts declares `reorderColumns(fromFName, toFName)` by field name, so the C#
  argument shape (`fieldName`, `beforeFieldName`) is correct;
- the first trace (grid without `allowReordering`) left the order unchanged after a
  250 ms settle — proving it was not a timing artifact;
- enabling `allowReordering: true` (which injects the EJ2 Reorder module) made the
  same call reorder the columns;
- therefore `reorderColumns` is a silent no-op unless the Reorder module is present;
- `ShowColumn`/`HideColumn` do NOT need `allowReordering` — they operate on column
  visibility, proven standalone.

Implication for the vertical slice: any view proving `ReorderColumnBefore` must build
the grid with `.AllowReordering(true)`. `Operations.cshtml` already does
(`ReorderColumnBefore` at `:125`, `.AllowReordering(true)` at `:214`), so this is a
required-builder-option finding, not a Fusion bug.

## Proof status

Proven: focused view `/Sandbox/Components/Grid/CareStaffColumns` +
`WhenUsingFusionGridColumns` (3/3 passed on a fresh build,
`playwright-20260607-055541.trx`) assert visible hide / show / reorder of the
rendered header cells through the typed Fusion DSL. The three matrix rows are
`row-proven`.
