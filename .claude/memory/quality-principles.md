---
name: Quality Principles
description: Consolidated reference for evidence-based auditing, layered harness design, review standards, and process discipline
type: reference
---

# Quality Principles

This framework serves senior living communities. Residents depend on software built with it.
Correctness over speed. Evidence over assumptions. Pragmatic excellence at every layer.

---

## 1. The 9-Point Evidence Contract

Before ANY code change from an audit finding, the issue MUST satisfy ALL nine criteria:

| # | Criterion | What Counts |
|---|-----------|-------------|
| 1 | Describe the issue | Clear description of the problem |
| 2 | Identify root cause | Not symptom — trace to the origin |
| 3 | Cite evidence | Exact file:line, test output, commit hash |
| 4 | Confirm no other layer handles it | C# DSL, schema, tests, views — the finding may be prevented elsewhere |
| 5 | Provide reproduction | Concrete scenario in browser or test. If NOT REPRODUCIBLE, not a real bug |
| 6 | Propose fix with framework primitives | No hacks, no workarounds |
| 7 | State tangible outcome | What does fixing bring? |
| 8 | Verify framework alignment | Plan-driven architecture, fail-fast, vertical slices |
| 9 | Confirm all tests pass after fix | No regressions |

**Why this exists:** Agents flagged "issues" that were correct design decisions (sync executeReaction,
window.alis.confirm, evaluateCondition null blocking). Evidence-first prevents wasted effort and
accidental architecture reversals.

**How to apply:** Include these 9 criteria in every audit agent prompt. Reject any finding that
cannot satisfy all 9. Use the 3-layer judge pattern to enforce.

---

## 2. The 3-Layer Audit Pattern

### Layer 1: Module Readers (one per scope, run in parallel)

Each agent reads EVERY LINE in their scope. Reports per-file: Clean or finding with
file:line, severity, what is wrong, tangible outcome. Scopes:

- core + types
- execution core (execute, commands, element, trigger, inject)
- network + HTTP (gather, http, pipeline, retry-indicator, signalr, server-push)
- resolution + conditions
- validation
- lifecycle + boot
- components

### Layer 2: Three Judges (run after all readers complete, in parallel)

**Integration Judge** -- looks ACROSS module findings for systemic patterns: contract violations,
coupling, inconsistent error handling across boundaries. Finds what individual readers miss.

**Non-Dogmatic Judge** -- classifies ALL findings: MUST FIX / SHOULD FIX / DEFER / REJECT.
Asks: Is this a real bug users will hit? Does the fix prevent a class of bugs? Is the fix
worse than the problem? Is this already handled by another layer?

**Evidence-Based Prosecutor** -- proves each high-priority finding is real by tracing actual
code paths. Reads the code, checks if C# DSL prevents it, checks if tests catch it. Must
satisfy the 9-point contract. If it cannot be proven real, it is a FALSE ALARM.

### Layer 3: Actionable Findings

Only PROVEN findings that pass all 3 judges become work items.

**Why this structure:** Module readers over-report in isolation. The Integration Judge catches
systemic patterns readers miss. The Non-Dogmatic Judge filters academic violations. The
Prosecutor prevents false alarms from becoming wasted work (5 documented false alarms in
this repo).

---

## 3. Anti-Rubber-Stamping

Before writing "PASS" on any module, ask: "What would break if I added a new Syncfusion
component? Does this code handle it?" If you cannot answer from the code, investigate.

Rules:
- Trace actual runtime paths for BOTH SF and native components through every module.
- Challenge every silent return (`if (!x) return`) -- is the caller aware? Is this a swallowed bug?
- Challenge every querySelector -- is it ID-scoped or wide? Does it match plan-driven IDs?
- If you find yourself writing PASS without finding at least one real question, you are
  skimming, not auditing.

**Why:** During the SOLID audit of validation modules, "PASS" was given for error-display.ts
and live-clear.ts after only reading code structure -- without tracing whether SF component
events reach live-clear listeners or whether the DOM walk pattern works for vendor components.
Rushing to show completion is the root cause.

---

## 4. Review Language and Output Design

### Positive Framing

Frame stakes positively. Set the bar, do not list prohibitions.

**Wrong:** "Do NOT rubber-stamp. If everything is perfect, explain WHY."

**Right:** "You are reviewing instructions for a Senior Living App Framework. Any lack of
focus, guessing, or critical thinking will cost dearly. Root yourself in pragmatic excellence."

Every review agent prompt opens with: what the system is (senior living framework), what
the stakes are (residents depend on it), what standard to meet (pragmatic excellence).
Rewrite prohibitions as standards.

### Rank by Value, Not Fixed Caps

**Wrong:** "Max 10 findings, numbered."

**Right:** "Report findings ranked by the value they bring. Most impactful first. Stop when
you have nothing valuable left to say."

A fixed cap forces two failure modes: padding with low-value findings when fewer exist,
or truncating high-value findings when more exist. Ranking by value lets the reviewer
self-organize. The consumer reads top-down and stops when the signal drops.

Each finding needs evidence (file:line, commit hash, or doc citation).

---

## 5. Layered Harness Design

Each layer has its OWN quality process. The process guides critical thinking at each
boundary -- it is never one flat flow.

### Layer 1: C# Descriptors and Builders

- **Skills:** TDD, modern-csharp, dotnet-xml-docs
- **Thinking:** Value Objects, Encapsulation, SOLID, serialization impact on schema
- **Tests:** Write FAILING unit test FIRST (Red), review it, THEN write code (Green)
- **Harness:** VerifyJson snapshots + AssertSchemaValid

### Boundary 1-2: C# to Schema

A failing `AssertSchemaValid()` test is the ONLY reason to edit the schema. Write C# code,
write failing unit test, test proves schema needs updating, review with evidence, THEN
update schema. Schema is the CONTRACT.

### Boundary 2-3: Schema to TS Types

Same rigorous process. Schema change triggers thinking about TS type impact. Write failing
vitest. Update types. Currently no automation validates TS types match schema (known gap).

### Layer 3: TS Runtime

- **Skills:** solid-ts-audit
- **Thinking:** SOLID, encapsulation, ID over DOM scanning, fail-fast over fallbacks
- **Tests:** vitest unit tests via `boot()`
- **Key:** Runtime is a dumb executor. If adding logic here, the plan is probably missing information.

### Boundary 3-4: Runtime to Browser

Browser first. Not Playwright first. Eyes before automation. "Tests pass" is necessary but
not sufficient. Browser is truth.

### Layer 4: Browser

- **Skills:** bdd-testing
- **Tests:** Playwright BDD (5 rules from BDD constitution)

### Layer 5: Docs and Skills

- Verify against code before writing. Sandbox-first for code examples.

### The Meta-Rule

At each layer and each boundary, there are DECISIONS that require critical thinking.
The process GUIDES those decisions -- it does not replace thinking with checklists.
Ask: "What boundary am I crossing? What does the layer I am leaving expect? What does
the layer I am entering require? Where is my failing test that PROVES the change is needed?"

---

## 6. Process Discipline

### Unexpected Boundary = Wrong Plan

If touching an area you did not plan for, the PLAN is wrong or the TASK is wrong.
STOP. Save learnings. Return to planning. Revert commits if needed, but save lessons first.
Pushing through a wrong plan creates patch-fix cascades.

### One Task Can Touch All Boundaries

A new primitive touches C# through Schema through TS through Browser through Docs. That is
one task, all layers. It may complete across multiple sessions. The process acts as HARNESS
across sessions -- tracking which layers are verified and which still need work.
Atomic does not mean "done in one commit." It means "verified end-to-end."

### Pattern Match, Do Not Predict

It is impossible to predict all task types. With enough pattern matching in the repo,
design a system that gets you to the right answer. When confused, explain the problem
step by step -- do NOT dump walls of text.

### User Is Final Guider, Not First Resort

Do the deep thinking. Present problems step by step. Ask specific questions. Propose
solutions and ask "is this direction right?" -- do not ask "what should I do?"

### Speed Is the Root Cause

"Correctness over sugar rush." Most mistakes trace back to going too fast and skipping
critical thinking at layer boundaries. Save research and feedback BEFORE executing.
Present system-level analysis before editing. Get user alignment on the DESIGN before
writing code. Think in LAYERS, not in flat task types.
