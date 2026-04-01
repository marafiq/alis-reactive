# Issue #86 Gap Ledger

## Purpose

This file separates:

- schema-fit gaps
- current implementation gaps
- review/closure gaps

Those are not the same thing, and issue #86 should not hide one category
behind another.

## Schema-Fit Gaps

Current status: **none identified** against the required workflow list.

The current end-state schema design, as documented in
[2026-03-31-issue-86-final-schema-shape.md](./2026-03-31-issue-86-final-schema-shape.md),
still fits the required feature/workflow set when checked against:

- [2026-03-31-issue-86-runtime-schema-proof.md](./2026-03-31-issue-86-runtime-schema-proof.md)
- [2026-03-31-issue-86-exhaustive-feature-proof.md](./2026-03-31-issue-86-exhaustive-feature-proof.md)

That means:

- no required workflow is currently known to require a second schema family
- no required workflow is currently known to require a fallback lane
- no required workflow is currently known to require adapter scaffolding

## Current Implementation Gaps

These are real, but they are **implementation** gaps, not end-state schema-fit
gaps.

### 1. `ReactivePlan.Render()` still emits the current runtime shape

Current code:

- [ReactivePlan.cs](/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/.codex-worktrees/issue-86-capability-matrix/Alis.Reactive/ReactivePlan.cs)

Still emits:

- `components` keyed by binding path
- `entries`
- current descriptor vocabulary such as `readExpr`, `componentType`,
  `coerceAs`, `set-prop`

So the hidden end-state schema is designed and proved, but not yet the emitted
runtime contract.

### 2. TS runtime still executes the current plan contract

Current code:

- [boot.ts](/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/.codex-worktrees/issue-86-capability-matrix/Alis.Reactive.SandboxApp/Scripts/lifecycle/boot.ts)
- [trigger.ts](/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/.codex-worktrees/issue-86-capability-matrix/Alis.Reactive.SandboxApp/Scripts/execution/trigger.ts)
- [http.ts](/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/.codex-worktrees/issue-86-capability-matrix/Alis.Reactive.SandboxApp/Scripts/execution/http.ts)

This means:

- the design is now ahead of the implementation
- the runtime proofs are proving the current algebra and lifecycle truths
- they are not yet proving execution of the hidden final schema directly

### 3. Compositional access is designed beyond the current runtime read model

The end-state schema now uses compositional `access.steps[]` because it is the
clean design for:

- member access
- invoke access
- invoke then continue walking

The current runtime still uses the older read-path shape. So:

- this is not a schema gap
- this is a planned implementation step

### 4. The stale end-state harness is still present in the repo

These files remain non-authoritative:

- [when-proving-end-state-schema.test.ts](/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/.codex-worktrees/issue-86-capability-matrix/Alis.Reactive.SandboxApp/Scripts/__tests__/when-proving-end-state-schema.test.ts)
- [end-state-plan-fixtures.ts](/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/.codex-worktrees/issue-86-capability-matrix/Alis.Reactive.SandboxApp/Scripts/architecture-proof/end-state-plan-fixtures.ts)
- [end-state-plan-types.ts](/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/.codex-worktrees/issue-86-capability-matrix/Alis.Reactive.SandboxApp/Scripts/architecture-proof/end-state-plan-types.ts)

Those should be replaced or retired once the hidden end-state contract is
implemented for real.

## Closure Gaps

These are the remaining gates before issue #86 can honestly be called done.

### 1. Hidden end-state serializer implementation

We still need a real lowering path from public C# DSL surfaces into the hidden
end-state schema.

### 2. Hidden end-state runtime implementation

We still need the TS runtime to execute the hidden end-state schema directly,
not just the current contract.

### 3. Final proof rerun on the implemented end-state contract

After implementation, the full proof loop must be rerun against:

- canonical native/fusion proof surfaces
- request stages
- validation lifecycle
- merge lifecycle
- SSE and SignalR
- non-input component surfaces

### 4. Final hostile review rounds

After the implementation and proof rerun are complete, the reviewer challenge
must be rerun in multiple rounds and asked to break the final state using
DSL-supported cases only.
