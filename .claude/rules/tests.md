---
paths:
  - "tests/**"
---

# Test principles

A test is the specification of one promise. A change that keeps every promise passes the tests; a change
that breaks a promise fails the test named for it. Tests are held to a higher bar than the code they test,
because every page an app builds rests on them. A test is a page authored the way a developer authors
one, run the way a browser runs it, with one gesture and one observable outcome. Never change framework code
to make a test pass: when a valid test fails, report the bug.

The rules govern every Playwright test. `tests/Alis.Reactive.Framework.PlaywrightTests` proves the
framework itself and adds the Framework tests section; `tests/Alis.Reactive.PlaywrightTests` proves the
shipped components and the sandbox journeys and adds the Component and journey tests section.

## The rules

1. **One promise, named as the promise.** A test proves one thing a developer or a user is promised: what a
   public member's XML docs say, a behavior of the plan grammar (`Alis.Reactive/Grammar`), or what a role sees on a journey.
   The fixture is the situation (`WhenAPageBranches`, `WhenCoordinatorAssignsABed`); the test is the promise
   as a sentence (`else_runs_only_when_no_arm_holds`). A name that describes code
   (`writes_a_let_statement`, `set_prop_mutation_writes_value_to_ej2_instance`) describes no promise.
2. **The public surface only.** Arrange as a developer does: Razor helpers, plan builders, validators
   registered through DI, components declared on `Component`. No `InternalsVisibleTo`, no internal
   constructor, no reflection. A behavior a developer cannot reach is given a public, typed door; it is
   never tested through internals.
3. **Real collaborators.** A real browser, a real server, real HTTP, the real DOM, and the gestures root
   CLAUDE.md names. No mocking library and no stub, spy or fake (not `vi.fn`, `vi.mock`, `vi.spyOn`,
   `vi.stubGlobal`, not an object whose methods the test writes). What a server answers comes from an endpoint
   of the test app, written as an app would.
4. **An outcome someone observes.** Assert what the user sees (text, value, visibility, focus, a state
   class), what the server received (the body a gather sent), or what the developer
   reads (the message a page is refused with). Never the plan JSON, internal state, call counts or log
   lines: a refactor that keeps every behavior keeps every test green.
5. **Fails for the defect it names.** Before a test is kept, its author introduces the defect it guards (drop
   the condition, skip the conversion, send the wrong member) and watches the test fail. An assertion the
   broken code also satisfies is no assertion: "nothing is shown" proves nothing unless the same page shows
   something when the promise holds.
6. **Both sides of every promise.** The case where the behavior happens and the case where it must not: the
   condition holds and fails; the rule refuses and accepts; the request is sent and is held back. A suite that
   proves only the happy path passes while the feature lies. A test written from a decision cannot find that
   decision's error: when a test pins a choice (a payload member the component does not pass reads as nothing),
   the other steps the same code decides (a write or a call through that path) are promises too, each with its
   own test.
7. **No tautology.** Expected values are literals read from the promise, never computed by the code under
   test, copied from its output, or accepted as a recorded snapshot. A test never asserts what it typed or
   set itself, or what the compiler already guarantees. `ToHaveTextAsync` when the text is known, not
   `ToContainTextAsync`.
8. **Independent and deterministic.** A fresh page for each test; no order between tests; no shared mutable
   data; per-page world keys, never process-global flags. No sleep, no retry, no skipped, ignored or
   commented-out test, no try/catch around an assertion. Wait for the observable state, never for time.
9. **Readable as the specification.** The test body shows the page, the gesture and the outcome, and holds
   every assertion. Helpers build pages and perform gestures; they never assert, branch or compute an
   expectation. A test has no conditional or loop.
10. **Complete.** Every public member and every behavior of the grammar has a test, and so does every
    refusal a developer can meet, with its message. Completeness is measured by defects, not lines: a defect
    no test catches is a missing test.

Watch the behavior in a real browser before writing the test that pins it.

## Smells

Each means a test proves less than its name claims; the number is the rule it breaks.

- A mock, stub, spy or fake, of framework code or of the boundary it talks to (3).
- A page arranged through internals, internal constructors or reflection (2).
- jsdom, or any stand-in for the browser, offered as proof (3).
- A golden, snapshot or plan-JSON assertion offered as proof of behavior (4, 7).
- Asserting the arrange: typing "Helen" and asserting the input says "Helen" (7).
- An expected value computed by framework code (7).
- An assertion the defect also satisfies, such as an empty element with no positive control (5).
- Only the happy path (6).
- One test proving several promises, or two tests proving one (1).
- A helper that asserts, or hides the gesture under test (9).
- A sleep, a retry, or an order between tests (8).
- A name that describes the code instead of the promise (1).
- A component library on a framework test's page (3).

## Framework tests

The system under test is the whole framework (the C# builders, the plan, the runtime and the browser
together) for the core package, its validation included, and the runtime. Their components are plain-DOM
declarations the test app brings through the extension point an app uses (`Component`, `ComponentVendor`):
the conformance library's components (spec/Reference/native.json) are the components on a framework test's page and
no Syncfusion component appears on one, so a Fusion change cannot fail one and a framework defect cannot hide behind one.

`tests/Alis.Reactive.PlanTests` arrange through the public surface but assert recorded plans (4, 7), and
the runtime's vitest suites run in jsdom with stubs (3). They are feedback while coding, not proof.

## Component and journey tests

**Nested vertical slices.** Every suite derives from one senior-living journey and owns its slice: its own
model, controller partial, view and fixture, under the same concern path in every tree, names aligned.
Grid's `Billing` slice is the exemplar. A component with many use cases fans out into many journeys, each a
full slice. The slice is the test's arrange; framework refactoring changes no slice, and a slice changes
only with its own journey.

1. Own model, referenced by no other journey.
2. Own view: one route that no other fixture opens. Dialog and drawer flows on a page belong to its journey.
3. Own data, unreachable from other journeys. No static mutable state, no shared fake-data class; immutable
   option lists are fine.
4. Own world: the fixture passes alone, in parallel and in any order.

**Seven behaviors per input component.** It renders with its initial state; a user action runs its
pipeline; invalid input shows its message inline; a condition turns a rule on and off; a valid correction
clears the message without a resubmit; its value is gathered into the request; a valid form submits and the
page shows the server's answer.

**Names.** Folders are the concern (`**/Patterns/BedAssignment/`); the folder carries the vendor, so names
carry no vendor prefix (`Components/Fusion/DatePicker/WhenDateSelected.cs`).

## Review

A test is reviewed blind: a reviewer given only these rules and the test opens the page and flags each
violation with the test's name, the rule, the evidence (the quoted assertion or name) and the test it should
be. The author fixes it or answers with evidence: a screenshot, the quoted assertion, the defect it catches.
The domain names the stakes, not the scope: this is a UI framework, so a reviewer does not reason about
HIPAA or PHI.
