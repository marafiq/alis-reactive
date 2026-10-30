---
name: Framework test principles
description: What a test of the framework itself is — one promise, proven through the public surface against real collaborators, failing for the defect it names — and the smells that make a test prove less than its name
type: reference
---

# Framework tests

The framework's tests are its specification. A change that keeps every promise passes them; a change
that breaks a promise fails the test named for it. They are held to a higher bar than the code they
test, because every page an app builds rests on them.

These rules govern the tests of the framework itself: the core packages (`Alis.Reactive`,
`Alis.Reactive.FluentValidator`) and the runtime. Tests of a component, shipped (Fusion, Native) or
brought by an app, follow `bdd-principles.md`; they are consumers of the framework, and a framework
test never depends on them.

## What a framework test is

A page authored the way a developer authors one, run the way a browser runs it, with one gesture and
one observable outcome. The system under test is the whole framework: the C# DSL, the plan, the
runtime and the browser together, never one class of it.

Its components are plain-DOM declarations the framework's test project brings through the same
extension point an app uses (`Component`, `ComponentVendor`). No component library appears in a
framework test: a Fusion change must not fail one, and a framework defect must not hide behind one.

## The rules

1. **One promise, named as the promise.** A test proves one thing the framework promises a developer:
   what a public member's XML docs say, or a behavior of `docs/design/grammar.md` a page can show. Its
   name is that promise as a sentence (`a_branch_runs_only_when_its_condition_holds`); its fixture is
   the situation (`WhenAPageBranches`). A name that describes code (`writes_a_let_statement`)
   describes no promise.
2. **The public surface only.** Arrange as a developer does: Razor helpers, plan builders, validators
   registered through DI, components declared on `Component`. No `InternalsVisibleTo`, no internal
   constructor, no reflection. A behavior a developer cannot reach is not a behavior: delete the code,
   or make it public and typed.
3. **Real collaborators, no doubles.** A real browser (Playwright), a real server, real HTTP, the
   real DOM. No mocking library and no stub, spy or fake: not `vi.fn`, `vi.mock`, `vi.spyOn` or
   `vi.stubGlobal`, and not an object whose methods the test writes. What a server answers comes from
   an endpoint of the test app, written as an app would write it.
4. **An outcome someone observes.** Assert what the user sees (text, value, visibility, focus, a state
   class), what the server received (the body a gather sent), or what the developer reads (the
   message a page is refused with when it renders). Never the plan JSON, internal state, call counts
   or log lines: a refactor that keeps every behavior keeps every test green.
5. **Fails for the defect it names.** Before a test is kept, its author introduces the defect it
   guards (drop the condition, skip the conversion, send the wrong member) and watches the test fail.
   An assertion the broken code also satisfies is no assertion: "nothing is shown" proves nothing
   unless the same page shows something when the promise holds.
6. **Both sides of every promise.** The case where the behavior happens and the case where it must
   not: the condition holds and fails; the rule refuses and accepts; the request is sent and is held
   back.
7. **No tautology.** Expected values are literals read from the promise. They are never computed by
   the code under test, copied from its output, or accepted as a recorded snapshot. A test never
   asserts what it typed or set itself, or what the compiler already guarantees.
8. **Independent and deterministic.** A fresh page for each test; no order between tests; no shared
   mutable data; no sleep and no retry. Wait for the observable state, never for time.
9. **Readable as the specification.** The test shows the page, the gesture and the outcome. Helpers
   build pages and perform gestures; they never assert, branch or compute an expectation. A test has
   no conditional, loop or try/catch.
10. **Complete.** Every public member of the DSL and every behavior of the grammar has a test, and so
    does every refusal a developer can meet, with its message. Completeness is measured by defects,
    not lines: a defect introduced in framework code that no test catches is a missing test, or the
    code is dead.

## Smells

Each of these means a test proves less than its name claims. The number is the rule it breaks.

- A mock, stub, spy or fake, of framework code or of the boundary it talks to (3).
- A page arranged through internals, internal constructors or reflection (2).
- jsdom, or any stand-in for the browser, offered as proof (3).
- A component library on a framework test's page (3).
- A golden, snapshot or plan-JSON assertion offered as proof of behavior (4, 7).
- Asserting the arrange: typing "Helen" and asserting the input says "Helen" (7).
- An expected value computed by framework code (7).
- An assertion the defect also satisfies, such as an empty element with no positive control (5).
- Only the happy path (6).
- One test proving several promises, or two tests proving one (1).
- A helper that asserts, or hides the gesture under test (9).
- A sleep, a retry, or an order between tests (8).
- A name that describes the code instead of the promise (1).

## Where the framework's tests stand (2026-10-27, HEAD f02b08971)

Measured against these rules; each later unit reports these counts again.

| Tests | Count | Breaks |
|---|---|---|
| `tests/Alis.Reactive.PlanTests` | 49 | 2: all 49 arrange through internals (`Support/Page.cs`). 4 and 7: 18 assert a recorded plan. |
| `Alis.Reactive.Assets/runtime/__tests__` | 96 | 3: all run in jsdom; 21 sites use a stub, spy or fake. |
| Playwright framework suites (CoreBehaviors, Plugins, HttpPipeline, Validation, Conditions, Patterns) | 844 | 3: 28 of their 108 pages render Fusion components. |

Until a test meets the rules it is feedback while coding, not proof of the framework.
