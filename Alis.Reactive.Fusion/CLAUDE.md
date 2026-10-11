# Alis.Reactive.Fusion — Syncfusion Slices

Root `../CLAUDE.md` is authoritative. This file adds only what is specific to a Fusion slice.

One self-contained file per Syncfusion EJ2 component: the declaration (its properties, methods and
events by their JavaScript names, `FusionX : FusionComponent`), its payload classes, and its render
helper (`field.Render<FusionX>(...)` for an input, `plan.Render<FusionX>(id, ...)` otherwise). No
per-member extension methods, events classes or reactive extensions: `Set`, `Call`, `Get` and the
core `.Reactive` serve every slice. Slices compile against the core's public surface only, as a
third-party component would. Duplication between slices is intentional; there are no shared
behavior base classes.

## When editing here

- Zero changes in `Alis.Reactive.Assets/runtime/`. If a runtime change seems needed, the plan is
  missing information; make the C# slice carry it.
- New typed surface (members, events, builder methods) stays inside the slice. Changing anything
  shared risks every slice, so trace downstream use first. Why: one shared-surface change once
  cascaded across 170+ files.
- Members typed by the app's class are generic methods (`GetSelectedRecords<TRow>()`,
  `RowSelected<TRow>()`); row members are `Field` arguments; a mode argument the JavaScript method
  always takes is fixed with `.Passing(...)`. See `docs/design/bring-your-own-component.md`.
- Member and event payload names come from the EJ2 source, not from memory or vendor docs alone. A
  payload property like `cancel` is behavior to prove, not a string to copy.
- Input IDs derive from the model expression; every other ID is the developer's explicit choice.
- Typed members only (ALIS009). Strings stop at the plugin boundary.

## Proof

`dotnet build` proves shape only. A slice member is supported when a Playwright test drives it on a
sandbox page and fails if it breaks.
