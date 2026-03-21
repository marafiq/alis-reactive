---
name: bdd-testing
description: >
  This skill should be used when writing Playwright browser tests, TS unit tests,
  or adding new test scenarios for Alis.Reactive. Also use when the user asks to
  "write a test", "add Playwright tests", "test this component", "fix a failing test",
  "add a test for this view", or "why is this test failing". Applies BDD first
  principles: test behavior not implementation, decouple tests from structure,
  name tests as sentences.
---

# BDD Testing — Alis.Reactive

## First Principle

> Tests must be **sensitive to behavior** and **insensitive to structure**.
> — Kent Beck

Refactoring internals must NEVER break tests. If it does, the test is coupled
to implementation. Rewrite the test, not the code.

## The Decision: What Kind of Test?

```
QUESTION                              TEST LAYER
─────────                             ──────────
"Does the user see the right thing?"  → Playwright (browser, DOM assertions)
"Does this pure function compute      → Vitest (jsdom, boundary values)
 correctly at every edge?"
"Does the C# DSL serialize the         → NUnit + Verify (JSON snapshots)
 correct plan shape?"
```

Playwright tests own **behavior**. Unit tests own **boundaries**. Snapshot tests own **contracts**.

## Playwright — Behavior Tests

### Shape: Given → When → Then

Every test is ONE user journey, named as a sentence:

```
Given  the page loads with empty form
When   user clicks Submit
Then   inline error appears on Name, no phantom summary
When   user fills Name and resubmits
Then   Name error disappears, remaining errors persist
When   user fills all fields and submits
Then   success message appears
```

### What to Assert

```
ASSERT                                    NEVER ASSERT
──────                                    ────────────
Text content the user reads               Plan JSON shape
Element visibility (shown/hidden)         Console trace messages
CSS classes that affect appearance         Internal function return values
Error message text and placement          Which guard type was used
HTTP response reflected in DOM            How many commands executed
```

### The Decoupling Test

Before writing any assertion, ask:

> "If I refactored the C# builder or TS runtime internals,
> would this assertion break?"

**YES** → assertion is coupled to structure. Remove it.
**NO** → assertion is coupled to behavior. Keep it.

### Fixture Design

Each scenario gets its own fixture class, own URL, parallel execution.
No shared state between fixtures. Helper methods scoped to that page's fields.

See **`references/patterns.md`** for fixture code patterns and locator helpers.

## Vitest — Boundary Tests

Test pure modules (resolver, conditions, rule-engine) at every edge:

```
INPUT                    EXPECTED          WHY
─────                    ────────          ───
"" for required          fail              empty is absence
null for required        fail              null is absence
0 for required           pass              zero is a value
unknown rule type        block (fail-closed)  safety default
broken regex             block (fail-closed)  safety default
eq on empty source       false             no intent expressed
```

DOM setup in jsdom must match production structure. See **`references/patterns.md`**.

## NUnit + Verify — Contract Tests

Snapshot-verify the JSON plan shape. These tests own the **contract** between
C# DSL and JS runtime. A snapshot diff means the contract changed — review
whether the change is intentional.

## Domain — Senior Living

All models use realistic entities: `ResidentModel`, `CareLevel`, `VeteranId`,
`FacilityModel`. Never generic "TestModel" or "User". Labels and error messages
use domain language ("Resident name", "Care Level", not "Username", "Category").

## When a Test Fails

1. **STOP.** Read the failure message. Do not touch code.
2. **Trace** the full path from trigger (click, event) to outcome (DOM state).
3. **Identify the exact line** producing the wrong result.
4. **Ask WHY** — it may be correct for a different scenario.
5. **Fix root cause**, not symptom. If unsure, **ask the user**.

See **`references/patterns.md`** for common symptom → root cause mappings.

## Validation Criteria

Before merging any test:

- [ ] **Behavior-named**: test name is a sentence describing user-visible outcome
- [ ] **Full journey**: covers error state → correction → success (not just one assertion)
- [ ] **Decoupled**: would survive a refactoring of builder internals or runtime modules
- [ ] **Observable**: asserts only what the user can see (text, visibility, placement)
- [ ] **No implementation leakage**: never asserts plan JSON, trace output, or internal types
- [ ] **Parallel-safe**: own fixture, own URL, no shared state
- [ ] **Domain-realistic**: senior living entities, not test stubs

## Additional Resources

- **`references/patterns.md`** — Fixture code, locator helpers, validator scoping, AJAX lifecycle, root cause table
- **`references/first-principles.md`** — Dan North, Kent Beck, Ian Cooper, Gojko Adzic research
