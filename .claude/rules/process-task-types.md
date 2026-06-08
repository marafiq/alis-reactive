# Task Types by Layers Touched

Related: `process-pipeline.md` (overview) | `process-layers.md` (layer details)

Tasks are categorized by which pipeline layers they cross.
The harness for each layer applies automatically.

## Single-Layer Tasks

| Task | Layer | Key Thinking |
|------|-------|-------------|
| Refactoring TS | 3 | SOLID audit, vendor isolation, preserve behavior |
| Refactoring C# | 1 | SOLID, encapsulation, API surface unchanged |
| SOLID enforcement | 1 or 3 | Evidence-based (file:line + consequence) |
| SonarQube cleanup | 1 or 3 | Root cause fix. BDD coverage on touched code |
| Rider warnings (CS1591) | 1+5 | Load `dotnet-xml-docs` first. Dev-facing voice |

## Multi-Layer Tasks

| Task | Layers | Notes |
|------|--------|-------|
| Writing a view | 1, 4 | Views usually don't change plan shape |
| Fixing a bug | ? | Trace to identify layer. Fix at root |
| New primitive | 1→2→3→4→5 | All boundaries. May span sessions |
| Component onboarding | 1→2→3→4→5 | 7-file vertical slice. Zero TS runtime changes |
| Writing docs | 4→5 | Verify before documenting |
| Writing tests | Matches layer | Test harness matches layer it guards |
| Code review | All PR layers | Verify each boundary crossing |

## View Writing (Layers 1, 4)

**Layer 1:** Confirm model, fields, component per field. Validator scope = form scope
(create before the view). Nested properties use `SetValidator()`. End with `@Html.RenderPlan(plan)`.
All inputs through `Html.InputField()`.

**Layer 4:** Open browser, fill form, submit — verify with own eyes. Then write Playwright.

**Skills:** `reactive-dsl`, `http-pipeline`, `conditions-dsl`, `validation-rules`

## Bug Fixing (Layer depends on where bug is)

1. Reproduce — see the bug in browser or test output.
2. Trace — identify which layer the bug is in.
3. Read — full code path end-to-end at that layer.
4. Research if stuck — after 2 fail rounds, stop coding and WebSearch.
5. Fix at root layer — one correct change, not patches.
6. Verify downstream — does fix flow correctly through boundaries?

If touching an unexpected layer, the plan is wrong. Stop and return to planning.

## New Primitive (All Layers, 8-Step Checklist)

| Step | Layer | What |
|------|-------|------|
| 1. C# plan model | 1 | sealed class, `internal` constructor |
| 2. Polymorphic registration | 1 | `PlanNodeDiscriminator<T>` writes the concrete type; its `Kind` is the discriminator |
| 3. Builder method | 1 | PipelineBuilder, ElementBuilder, or TriggerBuilder |
| 4. Generated TS contract | 1→2 | Regenerate `runtime/types/plan.ts`; `npm run typecheck` |
| 5. Runtime handler | 3 | New switch case + `assertNever` |
| 6. TS runtime test | 3 | Runtime behavior via `boot()` in vitest |
| 7. Playwright test | 4 | Full user journey in browser — the C# + page-behavior proof |
| 8. Sandbox view | 4→5 | Demonstrate the primitive |

May span sessions. Track which steps are complete.

## Component Onboarding (All Layers)

Load `onboard-fusion-component` skill first. 7-file vertical slice. Zero TS runtime changes.
If TS changes seem needed, the plan is missing information — fix the C# descriptor.
`resolver.ts` owns all vendor knowledge.

## Writing Tests (Matches Tested Layer)

| Test Type | Layer | Harness |
|-----------|-------|---------|
| Contract typecheck | 1→2 | `npm run typecheck` regenerates `plan.ts`; mismatch with C# fails |
| TS runtime vitest | 3 | `boot()` / module tests in jsdom |
| Playwright BDD | 4 | 5 BDD rules + blind review — the C# + page-behavior proof |

Arrange using public DSL: `Html.On`, `CreatePlan()`, `Trigger()`, builders.

### Coverage Completeness Gate

A test suite is not complete until every item in its scope is either tested or explicitly
justified as untestable. "Tests pass" proves quality of what exists — not completeness.

**Before any test suite is declared done:**
1. List every item in scope (plan-node kinds, generated TS union variants, public API members, component behaviors, etc.)
2. Map each item to the test that covers it — by name, not by assumption
3. Items with zero coverage must be marked with a justification and tracked as a gap
4. Produce the coverage matrix BEFORE requesting review

**Reviewers MUST verify the matrix** — not just the tests. If the matrix is missing,
the review is incomplete. This gate exists because a past suite shipped 59 passing
tests while roughly a third of the items in its scope had zero coverage, and two
full review rounds missed it.

Why: Reviewing what IS written is necessary. Checking what is NOT written catches
the gaps that passing tests hide.

## Writing Docs (Layers 4→5)

1. Verify feature in browser first.
2. Create sandbox example — build, run, verify.
3. Copy verified code to docs.
4. Question → answer structure. Progressive disclosure.
5. Dev-facing language only.

## Code Review (All PR Layers)

For each layer the PR touches:
1. Confirm the layer's harness is satisfied (tests pass).
2. Confirm each boundary crossing was driven by a failing test.
3. Apply 9-point evidence criteria to every finding.
4. Trace actual runtime paths — both SF and native components.
5. **Verify coverage completeness** — list every item in scope, map each to a test.
   Report uncovered items. "Tests pass" is not sign-off; "all items covered or justified" is.
   Why: a past PR shipped 59 passing tests with roughly a third of its scope uncovered.
   Two review rounds checked test quality but never asked what was missing.

## Agent Dispatch Template

See `agent-dispatch.md` — four task-specific templates with filled examples.
