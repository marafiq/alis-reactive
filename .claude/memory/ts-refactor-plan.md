---
name: ts-refactor-plan
description: Next session plan — build SOLID TS skill from credible sources, then plan TS module restructure
type: project
---

## Next Session: TS Quality Improvement

### Step 1: Build a superpowers skill for SOLID TS patterns

Credible sources the user approves:
- **Robert C. Martin (Uncle Bob)** — SOLID principles, Clean Architecture
- **Martin Fowler** — Refactoring catalog, enterprise patterns
- **Google TypeScript Style Guide** — TS-specific conventions
- **TypeScript Handbook** — language idioms
- **TanStack** — credible TS library patterns (user mentioned)

CRITICAL: S in SOLID is "one reason to change" (one actor), NOT "does one thing"

The skill should encode:
- SOLID principles with correct definitions (not watered-down versions)
- TS-specific patterns for each principle
- Anti-patterns to detect
- How to evaluate existing code against these principles

### Step 2: Research and enable TS linter incrementally

Find a credible TS linter (eslint + typescript-eslint likely). Enable rules incrementally — don't fix everything at once.

### Step 3: Audit TS modules with the skill

Concerns identified:
- **Vendor agnostic** — is component.ts the only vendor-aware module?
- **Cleanup** — event listener removal, plan lifecycle
- **Coercion** — resolver.ts coerce() quality
- **SOLID gaps** — module boundaries, dependency direction, reason-to-change analysis
- **Integration isolation** — bug in conditions should NOT cascade to other modules
- **Naming** — `data-alis` vs `data-reactive` inconsistency (must standardize to `data-reactive`)
- **Trace module** — "meh whatever" — low priority, user doesn't care about it

### Step 4: Plan refactored TS on paper

User MUST be in the loop for every single method. They want to get deep trained on the architecture.

### BDD Tests Ready

779 TS tests (110 new deep BDD) protect against regressions during refactor.
All via boot() — test behaviors not implementation. Safe to rip apart internals.

### What NOT to do
- Do NOT change TS runtime without user approval
- Do NOT assume — verify
- Do NOT use random GitHub as credible source
