# Issue #86 Task-Level Plan

> This plan is derived from the saved transcript and the architecture
> understanding document in this same folder. It is intentionally task-focused:
> what to do next, in what order, and under which architectural constraints.

## Objective

Produce a code-backed, architecturally correct issue #86 package that:

1. explains the current framework model accurately
2. shows exactly where value-flow continuity stops today
3. proposes the smallest strengthening change without system rot
4. sets up refactoring and tests in a way that preserves the few-command model

## Non-Negotiable Constraints

- No fallbacks.
- No duplicate old/new lanes left behind.
- Refactors must be surgical, but cleanup is part of the refactor.
- Source is truth.
- Runtime should get dumber, not smarter.
- Component events and custom events are one flow model with different scope.
- `readExpr` is member access after root resolution, not a special subsystem.
- `coerce` is the shaping step between raw JS value and typed DSL semantics.
- New support should fall out of the shared model, not from another vertical
  slice.

## Phase 1: Finish The Current-State Architecture Proof

Create the final current-state package before proposing any design change.

### Deliverables

- A capability matrix organized around the shared model:
  - root resolution
  - member access
  - value shaping
  - command consumption
  - event scope
- Architecture diagrams that trace the real current paths across:
  - C# DSL/builders
  - descriptors/schema
  - TS runtime
  - sandbox/test evidence
- A short narrative that explains the framework as a coherent model rather than
  as disconnected features

### Required current-state questions

- Which roots can be resolved today?
- Which member-access paths are supported today?
- Where does shaping/coercion live today?
- Which command consumers accept source-driven values today?
- Which flows are public-DSL reachable vs descriptor/runtime-only?
- Where does continuity stop?

### Rules

- Do not invent target abstractions in the matrix.
- Separate “supported”, “partial”, and “runtime/descriptor only”.
- Treat event-scope differences as scope differences, not separate capability
  families.

## Phase 2: Identify The Shared Architectural Pressure Point

After the matrix is stable, state the real duplication clearly and narrowly.

### Focus

Document that the architectural duplication is in command value consumption, not
in:

- root resolution
- member access
- `readExpr`
- `walk(...)`
- validation enrichment
- gather
- trigger scope

### Required outcome

A small table showing how command consumers currently accept values:

- mutate-element
- mutate-event
- call args
- dispatch

This table should make the duplication visible without yet locking the final
refactor naming.

## Phase 3: Define The Refactor Target At The Concept Level

Before writing code, define the target in architectural terms.

### Required answer

Identify the minimum stable concepts that should remain after cleanup:

- root resolution
- member access
- value shaping
- command execution
- event scope

### Required design question

What is the single shared concept for:

> how a command obtains, shapes, and consumes a value?

This must be answered cleanly and in a way that can be named without hand-wavey
helper language.

### Guardrails

- Do not add a dispatch-only value model.
- Do not make dispatch a special case.
- Do not create compatibility lanes that survive the refactor.
- Prefer descriptors that encapsulate meaning cleanly and serialize clearly.

## Phase 4: Red-First BDD Specification

Only after the architectural target is written down should tests be designed.

### Rules for test authoring

- New BDD test files only; do not overload old specs.
- Respect vertical slices.
- Keep tests behavior-centered and architecture-revealing.
- Write failing tests first.

### Test packs to define

#### 1. C# plan/descriptor tests

New spec files proving:

- current supported source-driven flows
- the exact missing continuity point for issue #86
- the final descriptor shape expected after refactor

#### 2. Schema tests

New spec files proving:

- new descriptor shapes serialize and validate cleanly
- no fallback dual-wire contract remains

#### 3. TS pure runtime tests

New spec files for pure seams:

- root resolution
- member access
- value shaping/coercion
- command value consumption

These should stay as pure and deterministic as possible.

#### 4. TS execution tests

New spec files proving:

- command execution uses the same shared mechanics across consumers
- issue #86’s target continuity works at runtime once implemented

#### 5. Playwright tests

New end-to-end BDD specs proving:

- current boundary behavior
- explicit event scope handoff behavior
- no false/global leakage from local component events

## Phase 5: Rewrite Issue #86

Only after phases 1 through 4 are written down should the issue be rewritten.

### Issue structure

1. Start with the capability matrix.
2. Explain the current architecture in the shared model vocabulary.
3. State the exact continuity gap without exaggeration.
4. Explain why the gap is not “absence of values”.
5. Propose the smallest strengthening change consistent with the architecture.
6. Explicitly state anti-goals:
   - no storage subsystem
   - no fallback lanes
   - no dispatch-only mini-model

### Tone

- architect-level
- evidence-backed
- nuanced
- traceable to code
- no speculative feature pitch language

## Expected Outcome

If this plan is followed correctly, the result should be:

- one saved architectural baseline
- one raw transcript preserving nuance
- one accurate current-state matrix
- one clear identification of the real duplication point
- one red-first test plan that does not fight the existing suite
- one issue rewrite that is narrow, correct, and future-safe
