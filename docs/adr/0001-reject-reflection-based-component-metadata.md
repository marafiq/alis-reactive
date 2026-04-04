# ADR-0001: Reject Reflection-Based Component Metadata

- Date: 2026-04-04
- Status: corrected
- Scope: `Alis.Reactive`, `Alis.Reactive.Native`, `Alis.Reactive.Fusion`

## Bad Decision

Component metadata was declared through `ReactiveComponentAttribute` and discovered through reflection in the active framework path.

## Why It Was Wrong

This violated the no-reflection rule and hid slice ownership behind framework magic.
It also weakened intent at the component line level because metadata was not explicitly owned by the slice class that depends on it.

## Violated Rules

- `AGENTS.md` section 6: Reflection Policy
- `AGENTS.md` section 8: Vertical Slice Rule
- `AGENTS.md` section 11: Decision Gate

## Impact Introduced

- reduced clarity of slice ownership
- made metadata discovery implicit instead of explicit
- made reviewer reasoning harder
- encouraged future convenience-driven architecture instead of explicit declarations

## Corrective Decision

Replace attribute/reflection discovery with explicit slice-owned metadata declarations on component types through the shared kernel contract.

## Proof Of Correction

- deleted `Alis.Reactive/ReactiveComponentAttribute.cs`
- added explicit metadata contract in `Alis.Reactive/ComponentMetadata.cs`
- updated component slice classes to declare metadata directly

## Follow-Up Obligations

- continue removing any remaining reflection-based authoring behavior
- ensure new component slices follow the explicit declaration pattern only
