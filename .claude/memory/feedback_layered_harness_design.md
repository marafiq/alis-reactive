---
name: feedback_layered_harness_design
description: User's vision for layered harness system — each layer has its own skills, tests, thinking process. Not one flow, but guided decision-making at each boundary.
type: feedback
---

## Layered Harness — User's Vision (2026-03-28)

### Core Insight
Each layer has its OWN quality process. The process guides critical thinking at each boundary — it's never one flat flow. Decisions must be made with evidence at every transition.

### Layer 1: C# Plan Authoring & Builders
- **Skills**: TDD (pragmatic, not dogmatic), modern-csharp, dotnet-xml-docs
- **Thinking**: Value Objects, Encapsulation, SOLID in the V2 authoring layer
- **Key**: What you write here DIRECTLY impacts schema. Think about serialization, business logic coordination with plan
- **Tests**: Write FAILING unit test FIRST (Red), review it, THEN write code (Green)
- **Harness**: VerifyJson snapshots + AssertSchemaValid

### Layer 1→2 Boundary: C# → Schema
- **Rule**: You CANNOT directly edit schema file just because you wrote C# code
- **Process**: Write C# → Write failing unit test → Test proves schema needs updating → Review with evidence-based input/output → THEN update schema
- **Why**: Schema is the CONTRACT. Changing it requires proof (a failing test), not just "I changed C#"

### Layer 2→3 Boundary: Schema → TS Types
- **Same rigorous process**: Schema change → think about TS type impact → write failing TS test → review → update types
- **Key**: TS types must match schema exactly. Currently no automation for this (gap)

### Layer 3: TS Runtime
- **Skills**: solid-ts-audit
- **Thinking**: SOLID, encapsulation, ID over DOM scanning, correctness over fallbacks
- **Tests**: vitest unit tests via boot()
- **Key**: Runtime is dumb executor. If you're adding logic here, the plan is probably missing information

### Layer 4: Browser
- **Browser-first verification** before heavy Playwright
- **Tests**: Playwright BDD (5 rules from constitution)

### Layer 5: Docs/Skills
- **Verify against code** before writing
- **Sandbox-first** for code examples

### The Meta-Rule
It's never one flow. At each layer and each boundary, there are DECISIONS that require critical thinking. The process GUIDES those decisions — it doesn't replace thinking with checklists.

**Why:** "Correctness over sugar rush" — the root cause of most mistakes is going too fast and skipping the thinking at each boundary.

**How to apply:** When touching any layer, ask: "What boundary am I crossing? What does the layer I'm leaving expect? What does the layer I'm entering require? Where's my failing test that PROVES the change is needed?"
