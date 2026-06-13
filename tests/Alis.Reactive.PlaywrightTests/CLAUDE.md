# Playwright Behavior Tests

Root CLAUDE.md is authoritative. Load the `bdd-testing` skill before writing or
modifying any test here. Fixtures are C# — `modern-csharp` standards apply to
them like any production code.

## What a test is

One user-visible behavior, full stack: the view renders the plan, the runtime
boots it, a real gesture happens, the browser visibly changes. Isolated (fresh
navigation, no ordering, no shared state), vertical (no mocking), behavior-named
(one sentence the user role would say). A suite derives from one senior-living
journey and owns its nested slice — model, view, fixture
(`memory/bdd-principles.md` → Nested Vertical Slices).

## Non-negotiables

- Real gestures only. No `page.evaluate()`, no `ej2_instances`, no DOM poking.
  One exception: framework gather-pipeline tests may assert `request.PostData`.
- Unhappy-path criteria are mandatory. A suite that only proves the happy path
  passes while the feature lies. Every assertion must be unsatisfiable by the
  defect it guards — ask "what failure would still pass this test?"
- Eyes before automation. Observe the behavior in a real browser before writing
  the test that pins it.
- Deterministic and hermetic. A test owns its world — per-page world keys, no
  process-global flags, no dependence on ambient server events. If a test
  passes only in isolation or only in sequence, it is broken.
- Locators: prefer `PagePlan<TModel>` expressions; `ComponentScope` when the
  model type is unavailable; raw locators only for explicit-ID elements.

## When a test fails

Triage in order: is the criterion right, the arrangement right, the tooling
right? All yes → verify manually in browser to classify locator bug vs app bug.
Locator bugs are fixed in `Playwright.Extensions`, app bugs in the app.
Never hack the test to pass. After two fail-fix rounds, stop coding and
research.

## Running

Always through `scripts/playwright.sh` (use `--filter` to focus). It rejects
stale browser assets — rebuild runtime assets after any TS change. Raw
`dotnet test` on this project is unsupported. Never kill the sandbox by process
name; a parallel suite may own a sibling instance — kill by port.

Why this file exists: the forensic record shows weeks spent on Playwright
infrastructure that caught one bug, and suites that passed while the feature
lied. Behavior-first, defect-unsatisfiable assertions are the correction.
