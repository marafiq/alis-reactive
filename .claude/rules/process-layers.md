# Layer Details & Boundary Crossings

Related: `process-pipeline.md` (overview) | `process-task-types.md` (tasks by layer)

## Layer 1 — C# Descriptors & Builders

**Skills:** `modern-csharp`, `dotnet-xml-docs`, `superpowers:test-driven-development`

**Before writing code, verify:**
- Confirm this is a Value Object with invariants enforced by the constructor.
  Why: Invalid state propagates through the pipeline and surfaces as wrong behavior in the browser.
- Write out the expected plan shape. Check whether it changes the generated TS contract.
  Why: The C# plan domain is the contract — regenerate `runtime/types/plan.ts` when the shape
  changes so the runtime stays aligned.
- Confirm visibility is deliberate: `internal` for API protection, `public` only for exposed surface.
  Why: 5 constructors left public cascaded across 170+ files (M17).
- Confirm the plan carries all information the runtime needs — runtime is a dumb executor.
  Why: Logic in the runtime splits behavior across two languages and lives outside the C# plan
  domain where it can be made unrepresentable — keep it in the plan.
- Confirm adding a new component does not require modifying existing descriptors (OCP).
  Why: 100+ vertical slices planned — each shared descriptor change risks all existing slices.

**Harness:** Author the plan through the public DSL; its correctness is proven end-to-end by the
Playwright suite (the plan boots and the browser behaves). Regenerate `runtime/types/plan.ts` and
run `npm run typecheck` whenever the plan shape changes. There is no separate C# snapshot/schema harness.

**Known gaps:** No C# unit harness for plan shape — correctness rides on Playwright + typecheck, so a
regression that still type-checks and still renders could slip past the gate.

## Boundary: C# Plan Domain → Generated TS Contract

A C# plan-shape change must flow to the generated TypeScript contract. Do not hand-edit
`runtime/types/plan.ts` — it is regenerated from the C# plan domain by `PlanTypeGenerator`.

**Process:**
1. Change the C# plan model / builder
2. Regenerate `runtime/types/plan.ts` (PlanTypeGenerator runs at the front of `npm run typecheck`)
3. Run `npm run typecheck` — a mismatch between the contract and the runtime fails here
4. Add the runtime handler for the new shape (new switch case + `assertNever`)
5. Prove the behavior in vitest, then Playwright
6. Commit C# + regenerated contract + runtime + tests together

**Why generated, not authored:** a hand-maintained contract drifts from C# silently. Generation
makes drift a typecheck failure instead of a runtime surprise.

## Layer 2 — Generated TypeScript Plan Contract

The contract between C# and the runtime: `runtime/types/plan.ts`, generated from the C# plan
domain by `PlanTypeGenerator`. JSON schema is retired — this generated file is the only contract.

**Before relying on it, verify:**
- The C# plan model carries the discriminator (`Kind`) and every field the runtime needs.
- `PlanNodeDiscriminator<T>` writes the concrete type so its `Kind` property becomes the JSON discriminator.
- The change regenerates cleanly — never hand-edit `plan.ts`.

**Harness:** `npm run typecheck` regenerates `plan.ts` and fails if the runtime no longer matches
the C# plan domain. That typecheck is the alignment gate.

**Gaps:** Alignment is checked structurally by typecheck, not semantically — a field that
type-checks but means the wrong thing still rides on Playwright to catch it.

## Boundary: Generated Contract → Runtime

A contract change means the runtime needs a handler for the new shape. Write a failing vitest
that exercises the new shape, then implement the handler.

**Process:**
1. Contract regenerated from C# (`runtime/types/plan.ts`)
2. Write a failing vitest using the new shape — type error or assertion failure
3. Add the runtime handler (new switch case + `assertNever` keeps the union exhaustive)
4. Run `npm test` and `npm run typecheck` — confirm alignment
5. Check: does the handler cover every variant of the new union?

**Blind spot:** typecheck proves the shapes line up, not that the runtime does the right thing
with them — prove that in vitest, then Playwright.

## Layer 3 — TS Types & Runtime

**Skills:** `solid-ts-audit`

**Before writing code, verify:**
- Confirm this is the runtime's job, not information the plan should carry.
  Why: Logic in the runtime is invisible to the C# plan domain and typecheck — only vitest and
  Playwright can see it.
- Confirm no vendor knowledge leaks outside `resolver.ts`.
  Why: Adding a third vendor must only touch resolver.ts — leaks force changes across entire runtime.
- Confirm no fallbacks. Wrong values propagate silently for hours before surfacing.
  Why: Fail-fast surfaces errors at the source. Fallbacks hide them until they reach the browser.
- Confirm no DOM scanning. Plan carries IDs; runtime uses getElementById only.
  Why: DOM scanning breaks when IDs change. Plan-driven IDs are stable by construction.
- Apply SOLID: SRP ("who requests changes?"), OCP (one switch case + assertNever), LSP (no vendor checks downstream), ISP (narrow exports), DIP (depend inward).

**Harness:** vitest + jsdom via `boot()`. Architecture enforcement tests. `npm run typecheck`.

**Known gaps:** Vendor leaks in trigger.ts:45, live-clear.ts:44. Dual condition evaluators
(21 ops vs 4 ops) can diverge. PlanRegistry over-exported. 3 "ForTests" functions in prod.

## Boundary: Runtime → Browser

<important>Browser first. Not Playwright first. Eyes before automation.</important>
"Tests pass" is necessary but not sufficient. Browser is truth.
Why: a quick manual browser smoke test repeatedly caught more real bugs than elaborate test
infrastructure did in comparable time. See it work before you automate it.

**Process:**
1. `npm run build:all && dotnet build` → start SandboxApp
2. Open browser → fill form → click → observe with own eyes
3. Confirm the USER sees the right thing
4. Then write Playwright BDD test

A quick manual browser check catches more than elaborate Playwright infrastructure. Eyes first.

## Layer 4 — Browser & Playwright

**Skills:** `bdd-testing`, `superpowers:test-driven-development`

**The 5 BDD Rules** (from `bdd-principles.md`):
1. Test describes BEHAVIOR, not implementation
2. Test is independently understandable
3. Test FAILS when behavior breaks
4. Test uses REAL interactions only (no `page.evaluate()`)
5. Test is blind-reviewed

**7-behavior contract per component:** Renders, Interacts, Validates, Conditionally
Validates, Live-Clears, Gathers, Submits.

**Research escalation:** After 2 fail-fix-fail rounds, stop coding and WebSearch.

## Boundary: Browser → Docs

Every code example comes from a working, verified sandbox page.
"If syntax is wrong, users will not use it. It will never come back."

## Layer 5 — Documentation & Skills

**Skills:** `dotnet-xml-docs`, domain skills for the topic

**Before writing, verify:**
- Confirm dev-facing language. No "runtime", "script tags", "descriptors" in user docs.
- Structure as question → answer. Progressive disclosure.
- No em-dashes in XML docs (Rider flags them).

**Harness:** Rider diagnostics on every file. Sandbox-verified examples.

**Gaps:** 5 docs-site pages reference deleted `IReactivePlan`. 50/78 docs files obsolete.
No skill usage audit trail.
