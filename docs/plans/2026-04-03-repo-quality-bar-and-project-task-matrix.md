# Repo Quality Bar And Project Task Matrix

## Decision Gate

Before every keep, rename, refactor, or delete decision, stop and ask:

1. Does this reduce tech debt relative to `main`, or am I hiding debt behind motion?
2. Does this keep exactly one active model in the system, or does it make us reason about two systems in parallel?
3. Is this class or module SOLID and properly encapsulated?
4. Can this be deleted instead of being carried forward?
5. Is this behavior explicit and dumb at runtime, or is the runtime compensating for weak design?
6. Does this test verify behavior the user cares about, or does it pin internals and dead terminology?

If the answer is wrong on any of these, treat it as a bug.

## Repo-Wide Rules

- No bridge, adapter, compatibility path, or fallback for deleted design concepts.
- No old vocabulary in active code, comments, tests, docs, traces, or helpers.
- No dead code kept "just in case".
- No class with multiple reasons to change.
- No test helper that changes page state in surprising ways.
- No test folder that preserves dead concepts after the redesign.
- Delete first. Keep only what is necessary, coherent, and better than `main`.

## Priority Order

1. Active product code must be V2-native and SOLID.
2. Runtime must be dumb, explicit, and fully green.
3. Test helpers must be SOLID and user-realistic.
4. Tests must be vertical-slice BDD and assert behavior.
5. Comments and XML docs are cleaned after behavior is stable, but misleading comments are bugs immediately.

## Product Projects

### `Alis.Reactive`

- Remove all remaining active old-model nouns where they still define the public or internal mental model.
- Rename or redesign `ValidationDescriptor`, `TypedEventDescriptor`, and similar survivors if they are still carrying dead "descriptor" language instead of the V2 domain language.
- Keep one clear responsibility per type across authoring, registration, validation, rendering, and serialization.
- Delete any helper, folder, or namespace that preserves deleted pre-V2 concepts as active architecture.
- Ensure authoring objects speak directly in bindings, contracts, objects, value expressions, predicates, actions, requests, and workflows.
- Ensure validation is binding-first everywhere and does not leak component enrichment baggage.
- Keep runtime-facing output V2-only and prevent any authoring path from producing generic or downgraded contracts when typed contracts are known.

### `Alis.Reactive.Native`

- Keep each native component wrapper focused on one capability surface.
- Remove duplicated or weakly-factored reactive extension patterns that blur rendering, capability declaration, and workflow authoring.
- Clean comments and names that talk about deleted cross-model ideas instead of current capability language.
- Ensure no native component extension leaks old vocabulary into the active API or tests.

### `Alis.Reactive.Fusion`

- Keep each Fusion component abstraction capability-first, not widget-implementation-first.
- Remove stale names and comments that still encode deleted authoring concepts.
- Ensure event args, member access, and property reads are expressed as V2 capability access only.
- Fix any browser-facing widget behavior that currently requires helper compensation instead of clean component semantics.

### `Alis.Reactive.NativeTagHelpers`

- Keep tag helpers limited to rendering and authoring orchestration.
- Remove any logic that mixes HTML concerns with runtime behavior decisions.
- Ensure tag helper tests verify rendered behavior and authored intent, not helper internals.

### `Alis.Reactive.FluentValidator`

- Rename extraction and transport concepts away from dead "descriptor" terminology if they remain active.
- Keep one binding-first validation contract from validator extraction to rendered plan.
- Delete any remaining enrichment-era concepts that were only necessary for the deleted schema.

### `Alis.Reactive.Analyzers`

- Keep analyzers aligned to the redesigned DSL and V2-native mental model.
- Remove analyzer fixtures or diagnostics that preserve dead architecture terms.
- Keep analyzer responsibilities narrow and explicit: one diagnostic concern, one rule surface, one clear failure reason.

### `Alis.Reactive.SandboxApp`

- Keep TS modules vertically sliced by concern: boot, merge, resolution, execution, validation, tracing, transport, component integrations.
- Remove old comments and messages that mention `readExpr`, reactions, walk-reaction behavior, or deleted runtime concepts.
- Rename ambiguous "fallback" semantics to explicit default-handler semantics where that is the real domain concept.
- Keep tracing OTel-shaped, consistent, and free of deleted terminology.
- Keep the runtime dumb: resolve, read, write, call, branch, request, validate. No guessing, no hidden compatibility logic.
- Ensure the browser path is fully green in real Playwright flows, not just unit tests.

### Root `package.json`

- Keep root scripts as strict orchestration only.
- Remove duplicate build logic or hidden alternate execution paths.
- Ensure commands fail hard and do not mask partial failure.

### `docs-site`

- Remove stale docs that describe deleted plan shapes or outdated mental models.
- Keep examples aligned to the active V2 system and current DSL.
- Delete any example that forces readers to reason about both old and new designs.

## Test Projects

### `tests/Alis.Reactive.UnitTests`

- Delete or rename dead folders that preserve the pre-V2 model.
- Re-slice tests around behaviors and capabilities instead of deleted implementation categories.
- Remove tests that assert old terminology, old namespaces, or dead structural assumptions.
- Keep architecture tests focused on allowed boundaries, not old folder names that should already be gone.

### `tests/Alis.Reactive.Native.UnitTests`

- Keep tests organized by native capability slices, not mutation plumbing.
- Remove repeated setup patterns that should live in focused test helpers.
- Ensure assertions verify authored behavior and rendered V2 intent, not implementation trivia.

### `tests/Alis.Reactive.Fusion.UnitTests`

- Keep tests organized by Fusion behavior slices, not internal builder mechanics.
- Delete stale mutation-era assumptions where the new model is capability/member based.
- Ensure DatePicker, DateRangePicker, and similar components assert the behavior the browser runtime depends on.

### `tests/Alis.Reactive.NativeTagHelpers.Tests`

- Keep tests on rendered output and authored plan consequences.
- Remove infrastructure-heavy tests that expose helper internals unnecessarily.

### `tests/Alis.Reactive.FluentValidator.UnitTests`

- Keep tests centered on validation behavior and rendered contract output.
- Remove assumptions tied to deleted enrichment design.

### `tests/Alis.Reactive.Analyzers.Tests`

- Keep fixtures narrow, behavior-focused, and aligned to the active DSL.
- Rename fake helper types that preserve dead architecture language if they are still active in test mental models.
- Ensure tests read as analyzer behavior slices, not scaffolding demonstrations.

### `tests/Alis.Reactive.DriftDetection.Tests`

- Keep drift coverage strictly V2.
- Delete any expectation that references deleted schema structure or old naming.
- Expand coverage for merge/removal/source ownership only in the V2 vocabulary.

### `tests/Alis.Reactive.DesignSystem.Tests`

- Keep tests scoped to design-system behavior only.
- Prevent reactive-framework concerns from leaking into design-system assertions.

### `tests/Alis.Reactive.Playwright.Extensions`

- Every locator/helper must be SOLID and represent one widget or one interaction surface.
- Delete helpers that act as mini-frameworks or hide multiple page concerns.
- Remove generic retries or scroll logic that creates side effects unrelated to the user gesture.
- Keep helpers user-realistic and explicit, especially for Fusion widget popups and composite controls.

### `tests/Alis.Reactive.PlaywrightTests`

- Full suite passing is mandatory.
- Keep suites organized as vertical slices by user-facing scenario.
- Delete tests whose names or assertions pin dead plan vocabulary such as vendor/readExpr/entry-count style checks.
- Remove assertions on inactive internals unless the suite is explicitly an architecture or trace contract test.
- Ensure each test tells a business or user story, not a framework plumbing story.

## Current Known Defects To Treat As Bugs

- Active old "descriptor" language still exists in core and validation types.
- Test folders still preserve dead architecture language in `tests/Alis.Reactive.UnitTests`.
- Playwright helpers still contain generic interaction behavior that can mutate page state in the wrong way.
- Runtime comments and coercion messages still mention deleted `readExpr` language.
- Some tests and artifacts still preserve deleted plan terminology in names, assertions, and outputs.

## Acceptance Criteria

- No active project contains deleted-model concepts in code, naming, comments, traces, or tests.
- Every class across C#, TypeScript, Playwright helpers, and tests satisfies SOLID and proper encapsulation.
- Every kept test is a behavior test or a necessary boundary/contract test.
- Full Playwright passes.
- All `.csproj` test suites pass.
- App build passes.
- XML docs are cleaned last, but by the end they are accurate and aligned to the redesign.
