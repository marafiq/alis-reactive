# Alis.Reactive.Fusion — Syncfusion Vertical Slices

Root `../CLAUDE.md` is authoritative. This file adds only what is specific to
working inside a Fusion component slice.

## What this project is

One self-contained vertical slice per Syncfusion component. Duplication between
slices is intentional — no shared behavior base classes. A slice is the C# typed
surface for one EJ2 component: the component class, its builder, HTML and
reactive extensions, and typed event definitions with typed payloads.

## Invariants when editing here

- Zero TypeScript runtime changes. Onboarding, auditing, or upgrading a
  component must not touch `Alis.Reactive.Assets/runtime/`. If a runtime change
  seems needed, the plan is missing information — fix the C# descriptor so the
  plan carries it.
- API surface is frozen: internal constructors, internal setters, builder-only
  authoring. Changing anything shared outside your slice risks every existing
  slice — trace downstream usage first.
  Why: one shared-surface change without downstream analysis once cascaded
  across 170+ files.
- Event and payload member names come from the EJ2 source of record captured in
  the onboarding artifacts — never from memory or vendor docs alone. A payload
  property like `cancel` is behavior to prove, not a string to copy.
- Input component IDs derive from the model expression; every non-input ID is
  the developer's explicit choice. No fallback IDs.
- Stringly APIs stop at the plugin boundary. Component members are typed.

## A slice row is not done until

Discovery artifacts, primitive map, slice code, sandbox exercise page, and
behavior proof exist for the row being closed. The onboarding skill
(`.claude/skills/onboard-fusion-component`) owns the gate chain and the
artifact tree under `tools/FusionOnboarding/`. Trust its verifier, not memory.

## Harness

`dotnet build` proves shape only. Behavior is proven exclusively by the
Playwright suite driving a sandbox page through the public DSL.
