# Issue #86 Next Implementation Steps

This is the clean implementation sequence implied by the current package.

It is **not** a migration bridge plan and **not** a compatibility plan. The
goal is to replace the hidden runtime contract cleanly once issue #86 moves from
design/proof into implementation.

## 1. Introduce Internal End-State Schema Types

Create internal schema types that match
[2026-03-31-issue-86-final-schema-shape.md](./2026-03-31-issue-86-final-schema-shape.md)
directly.

They should not reuse the current `entries`/`readExpr`/`componentType` object
model and should not be named after current DTO leftovers.

## 2. Lower Public DSL Surfaces Into Those Types

Use the proven lowering seams:

- component refs
- typed component sources
- request stages
- validation extraction/enrichment
- trigger builders

The public DSL remains the same. Only the hidden emitted contract changes.

## 3. Replace `ReactivePlan.Render()` Emission

Current file:

- [ReactivePlan.cs](/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/.codex-worktrees/issue-86-capability-matrix/Alis.Reactive/ReactivePlan.cs)

This should stop serializing:

- `entries`
- binding-path-keyed component registrations
- `readExpr`
- `componentType`
- `coerceAs`

and instead emit the hidden end-state schema directly.

## 4. Replace Runtime Consumption With End-State Semantics

Current runtime files:

- [boot.ts](/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/.codex-worktrees/issue-86-capability-matrix/Alis.Reactive.SandboxApp/Scripts/lifecycle/boot.ts)
- [trigger.ts](/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/.codex-worktrees/issue-86-capability-matrix/Alis.Reactive.SandboxApp/Scripts/execution/trigger.ts)
- [http.ts](/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/.codex-worktrees/issue-86-capability-matrix/Alis.Reactive.SandboxApp/Scripts/execution/http.ts)
- [values.ts](/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/.codex-worktrees/issue-86-capability-matrix/Alis.Reactive.SandboxApp/Scripts/execution/values.ts)

Runtime should be rewritten around:

- `components` registry
- explicit `TriggerPayload`
- ordered `pipeline.steps[]`
- request-owned stages
- compositional `access.steps[]`
- `apply` + `mutation`

## 5. Delete Runtime-Invented Meaning

The implementation pass should explicitly remove:

- invented native trigger payload structures
- validation DTO enrichment leakage
- any transport/value duplication lanes

If a piece of meaning is still invented in TS, issue #86 is not done.

## 6. Re-Prove Using The Canonical Harnesses

Use:

- [proof-surfaces.ts](/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/.codex-worktrees/issue-86-capability-matrix/Alis.Reactive.SandboxApp/Scripts/components/lab/proof-surfaces.ts)
- [when-proving-canonical-proof-surfaces.test.ts](/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/.codex-worktrees/issue-86-capability-matrix/Alis.Reactive.SandboxApp/Scripts/__tests__/when-proving-canonical-proof-surfaces.test.ts)
- [WhenLoweringDslSurfacesToRuntimeAlgebra.cs](/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/.codex-worktrees/issue-86-capability-matrix/tests/Alis.Reactive.UnitTests/Architecture/WhenLoweringDslSurfacesToRuntimeAlgebra.cs)

Those are the clean proof harnesses for the replacement.

## 7. Replace The Stale End-State Harness

Retire or replace:

- [when-proving-end-state-schema.test.ts](/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/.codex-worktrees/issue-86-capability-matrix/Alis.Reactive.SandboxApp/Scripts/__tests__/when-proving-end-state-schema.test.ts)
- [end-state-plan-fixtures.ts](/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/.codex-worktrees/issue-86-capability-matrix/Alis.Reactive.SandboxApp/Scripts/architecture-proof/end-state-plan-fixtures.ts)
- [end-state-plan-types.ts](/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/.codex-worktrees/issue-86-capability-matrix/Alis.Reactive.SandboxApp/Scripts/architecture-proof/end-state-plan-types.ts)

They should not be patched forward.

## 8. Run Final Hostile Review Rounds

Only after the new serializer and new runtime are in place:

- rerun the exhaustive proof packet
- ask reviewers to break the final shape with DSL-supported cases
- keep iterating until the remaining gaps are implementation bugs, not schema
  design bugs
