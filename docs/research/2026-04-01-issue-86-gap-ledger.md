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

### 5. Component-event payloads are still legacy-shaped in the current runtime

The final schema design now requires trigger payloads to be explicit. The
current runtime still does not execute that end-state contract directly:

- native component events still invent legacy payload objects in
  [trigger.ts](/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/.codex-worktrees/issue-86-capability-matrix/Alis.Reactive.SandboxApp/Scripts/execution/trigger.ts)
- fusion component events still forward callback payloads through the current
  plan shape

So:

- this is not a schema-fit gap
- this is still an implementation gap before the hidden end-state contract is
  real

### 6. Binding participation proof is narrower than the schema's extension surface

The schema intentionally leaves room for any component slice to opt into
canonical semantic participation through `binding`.

What is actually proven today is narrower:

- current readable participation is centered on registered/readable bound
  components
- the current public C# gather surface still hard-requires `IInputComponent`
  semantics even for the raw-id escape hatch
- non-input component command semantics are proven
- non-input component binding participation remains an extension surface, not a
  current-runtime fact

### 7. Response-based chained gather is runtime-proven but not DSL-proven

The runtime now preserves success context into chained requests, so response
scope is readable for chained gather in:

- [http.ts](/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/.codex-worktrees/issue-86-capability-matrix/Alis.Reactive.SandboxApp/Scripts/execution/http.ts)
- [when-chaining-http-requests.test.ts](/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/.codex-worktrees/issue-86-capability-matrix/Alis.Reactive.SandboxApp/Scripts/__tests__/when-chaining-http-requests.test.ts)

But the current public C# DSL does not yet directly prove response-based
`chained.gather` lowering. Typed response reads are proven for `OnSuccess`, not
for chained gather configuration itself.

### 8. Duplicate binding identity is still a current-contract limitation

The current emitted/runtime contract still keys registered components by binding
path and rejects duplicates in:

- [ReactivePlan.cs](/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/.codex-worktrees/issue-86-capability-matrix/Alis.Reactive/ReactivePlan.cs)

That means repeater/grid-like duplicate binding identity is still a real current
implementation limitation. The recent merge/runtime fixes now preserve a
surviving sibling registration when one partial with the same key is removed,
but the emitted/current plan contract still cannot model simultaneous duplicate
binding identities as first-class distinct components. The end-state schema
direction separates component identity from optional binding participation more
cleanly, but the hidden end-state contract is not implemented yet.

### 9. `WhileLoading` is commands-only and not self-reverting by itself

The current public DSL and runtime prove:

- `WhileLoading(...)` is commands-only
- the runtime executes those commands before fetch
- success/error handlers explicitly undo state where needed

So any documentation implying automatic self-revert behavior in the runtime
would be inaccurate.

### 10. JSON gather still applies sink-specific empty-string normalization

The current runtime still treats empty string differently by transport sink:

- JSON body gather normalizes `""` to `null`
- GET query and `formData` keep `""` as string data

Current seam:

- [gather.ts](/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/.codex-worktrees/issue-86-capability-matrix/Alis.Reactive.SandboxApp/Scripts/execution/gather.ts)

That is a current-contract behavior leak, not an end-state schema concept. The
hidden end-state runtime should either remove that leak or make the policy
explicit at one deliberate seam.

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
