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

The rules govern every Playwright test. A compiler rule or a named check holds each of them and each smell
(`spec/REWRITE.md`, section 2.10, the test rules TEST001 to TEST013, which U2 lands before the first feature test).

## The rules

1. **One promise, named as the promise.** A test proves one thing a developer or a user is promised: what a
   public member's XML docs say, a behavior of the plan grammar (`spec/Core/schema.md`), or what a role sees on a
   screen. The fixture is the situation, When and what holds; the test is the promise as a sentence, as its line in
   the spec states it. A name that describes code (`writes_a_let_statement`,
   `set_prop_mutation_writes_value_to_ej2_instance`) describes no promise.
2. **The public surface only.** Arrange as a developer does: Razor helpers, plan builders, validators
   registered through DI, components declared on `Component`. No `InternalsVisibleTo`, which
   `WhenAnInternalIsExposed` refuses in every project, no internal constructor, no reflection. A behavior a developer cannot reach is given a public, typed door; it is
   never tested through internals.
3. **Real collaborators.** A real browser, a real server, real HTTP, the real DOM, and the gestures root
   CLAUDE.md names. No mocking library and no stub, spy or fake, of the framework's code or of the boundary it talks
   to, and no object whose methods the test writes. What a server answers comes from an endpoint of the test app,
   written as an app would.
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

Write the test from its promise before the code it needs, watch it fail, then watch it pass in a real browser.

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

## Core tests

The system under test is the whole framework (the C# builders, the plan, the runtime and the browser
together) for the core package, its validation included, and the runtime. Their components are plain-DOM
declarations the test app brings through the extension point an app uses (`Component`, `ComponentVendor`):
the conformance library's components (`spec/Reference/native.json`) are the components on a core test's page and
no Syncfusion component appears on one, so a Fusion change cannot fail one and a framework defect cannot hide behind one.

A module's pure tests (`spec/REWRITE.md`, section 1) prove one half of a module against the schema, through its
public surface alone, and prove no behavior in a browser: a behavior is proven by a Playwright test, and a recorded
plan proves nothing.

## Where a test lives

Every browser test runs on one screen of the care home conformance app, in a world of its own:
`spec/Reference/app.json` names each test's screen, hosts, parts and harness verbs. A test sits in the folder of the
module whose test it is, under the screen it runs on, and no module's tests use another's helpers
(`spec/REWRITE.md`, sections 2.3 and 2.4).

A shipped slice's tests prove only what its declaration adds: its members' names and shapes, its browser facts and
its own defects. A framework behavior is proven once, on the conformance library, and never again through a slice.

## Review

A test is reviewed blind: a reviewer given only these rules and the test opens the page and flags each
violation with the test's name, the rule, the evidence (the quoted assertion or name) and the test it should
be. The author fixes it or answers with evidence: a screenshot, the quoted assertion, the defect it catches.
The domain names the stakes, not the scope: this is a UI framework, so a reviewer does not reason about
HIPAA or PHI.
