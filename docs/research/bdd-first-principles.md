# BDD Testing — First Principles Research

> How to write tests that survive refactoring, assert user-visible behavior,
> and decouple from implementation details.

## Sources

- [Dan North — Introducing BDD](https://dannorth.net/blog/introducing-bdd/) (the original BDD article)
- [Dan North — BDD Interview (InfoQ)](https://www.infoq.com/interviews/dan-north-bdd/)
- [Kent Beck — Canon TDD](https://tidyfirst.substack.com/p/canon-tdd)
- [Kent Beck — Programmer Test Principles](https://medium.com/@kentbeck_7670/programmer-test-principles-d01c064d7934)
- [Ian Cooper — TDD: Where Did It All Go Wrong? (notes)](https://keyvanakbary.github.io/learning-notes/talks/tdd-where-did-it-all-go-wrong/)
- [Gojko Adzic — Specification by Example](https://gojko.net/books/specification-by-example/)

---

## 1. The Core Problem

Tests couple to implementation details. Refactoring breaks tests. Tests become
a parking brake on change. Teams stop refactoring because "the tests will break."

This is the opposite of what tests are for.

---

## 2. Dan North's BDD Rules

### Test naming = sentence describing behavior

```
The [class/system] should [expected behavior]
```

Not: `test_validate_returns_false`. Yes: `empty_name_blocks_submission_with_inline_error`.

If the name doesn't fit the sentence template, the behavior belongs in a different place.

### Given-When-Then

```
Given [starting state]
When  [action/event]
Then  [observable outcome]
```

- "Given" = context setup
- "When" = one action
- "Then" = what the USER sees change

### One sentence = one test

The sentence constrains granularity. If you can't describe it in one sentence,
split into multiple behaviors.

### Story template (for acceptance criteria)

```
As a [role]
I want [feature]
So that [business value]
```

Forces identification of WHY the behavior matters.

---

## 3. Kent Beck's Test Principles

### Sensitive to behavior, insensitive to structure

If the program's behavior is stable from an observer's perspective, NO tests
should change. Tests must be:

- **Sensitive** to behavior changes (a real bug breaks a test)
- **Insensitive** to structure changes (refactoring does NOT break tests)

### Tests are coupled to API, decoupled from internals

Tests talk through the public surface. Never test internal methods directly.
If you feel the need to test internals, extract them into their own module
with its own public API.

### Cheap to change

A single behavior change should not produce a pile of red tests. If each
broken test must be examined individually, tests are too coupled.

### Canon TDD cycle

1. Write a list of test scenarios
2. Turn exactly ONE into a concrete, runnable test
3. Make it pass (commit whatever sins necessary)
4. Optionally refactor (this is when clean code happens)
5. Repeat

**Refactoring does NOT add new tests.** Tests express behavior. Refactoring
changes structure, not behavior. Adding tests during refactoring couples
tests to structure.

---

## 4. Ian Cooper — "TDD: Where Did It All Go Wrong?"

### What went wrong

- People test operations (methods on classes) instead of behaviors (user scenarios)
- People confuse "test isolation" (tests run independently) with "class isolation"
  (mock every collaborator)
- This produces: more test code than implementation, fragile tests, unclear intent

### The fix

| Coupled (wrong) | Decoupled (right) |
|-----------------|-------------------|
| Test each method | Test each behavior/scenario |
| Mock internal collaborators | Mock only external ports |
| Test through class API | Test through system boundary |
| Add tests when refactoring | Refactor without adding tests |
| Make methods public for testing | Keep implementation hidden |
| Test how it works | Test what it does |

### What gets tested

- **Ports** — boundaries where the system meets the outside world
- **Use cases** — domain-level behaviors
- **Integration points** — external systems (tested separately)

### What does NOT get tested directly

- Private methods
- Internal helper classes
- Implementation details that might change during refactoring

### Mock strategy

- Mock things you don't own (external APIs, databases)
- Do NOT mock internal collaborators
- If you're mocking everything, your design has coupling problems

---

## 5. Gojko Adzic — Specification by Example

### Seven patterns

1. **Derive scope from goals** — tests trace back to business value
2. **Specify collaboratively** — developers + domain experts write examples together
3. **Illustrate using examples** — concrete data, not abstract rules
4. **Refine the specification** — remove ambiguity, find edge cases
5. **Automate without changing specs** — automation layer adapts, not the specification
6. **Validate frequently** — run continuously, not just before release
7. **Evolve documentation** — tests ARE the living documentation

### Key insight

Write specifications in **abstract, high-level language** that describes what the
system does, not how. The automation layer (test infrastructure) translates
abstract specs into concrete assertions. When implementation changes, only the
automation layer changes — specs stay stable.

---

## 6. Applying to Alis.Reactive

### What "behavior" means for a reactive framework

| Layer | Behavior (test this) | Implementation (don't test this) |
|-------|---------------------|--------------------------------|
| **View** | "Clicking Save posts form data and shows success message" | Which CSS class gets added, which DOM element gets mutated |
| **Conditions** | "When score >= 90, grade shows A" | Whether it's a ValueGuard or AllGuard internally |
| **HTTP** | "Parallel GETs both complete and spinner hides" | Whether fetch uses Promise.all or sequential awaits |
| **Validation** | "Empty name shows error inline, not in summary" | Whether orchestrator calls rule-engine first or condition first |

### Decoupling heuristic for Playwright tests

Ask: "If I refactored the C# builder internals or the TS runtime module,
would this test break?"

- **YES** → test is coupled to implementation. Rewrite it.
- **NO** → test is coupled to behavior. Keep it.

### Concrete rules for Alis.Reactive tests

1. **Test through the browser** (Playwright), not through function calls
2. **Assert DOM state** (text content, visibility, CSS classes the user sees)
3. **Never assert plan JSON shape** in Playwright — that's a C# unit test concern
4. **Never assert console.log content** as the primary assertion — trace is implementation
5. **One test = one user journey** with Given/When/Then structure
6. **Name tests as behaviors**: `saving_resident_with_empty_name_shows_inline_error`
7. **Mock only the server** (controller returns canned data) — never mock the runtime
8. **Refactoring the runtime should NOT break Playwright tests** — if it does, tests are coupled
