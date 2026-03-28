# Layer Details & Boundary Crossings

Related: `process-pipeline.md` (overview) | `process-task-types.md` (tasks by layer)

## Layer 1 — C# Descriptors & Builders

**Skills:** `modern-csharp`, `dotnet-xml-docs`, `superpowers:test-driven-development`

**Before writing code, verify:**
- Confirm this is a Value Object with invariants enforced by the constructor.
- Write out the expected JSON shape. Check whether it requires a schema change.
- Confirm visibility is deliberate: `internal` for API protection, `public` only for exposed surface.
- Confirm the plan carries all information the runtime needs — runtime is a dumb executor.
- Confirm adding a new component does not require modifying existing descriptors (OCP).

**Harness:** Write failing test first. `VerifyJson()` captures exact JSON. `AssertSchemaValid()`
validates against schema. Both run every commit.

**Known gaps:** 53 tests construct internal types directly. No reverse schema validation.

## Boundary: C# → Schema

Schema changes require a failing test as evidence. Do not hand-edit the schema
to match C# output — let a failing `AssertSchemaValid()` test drive the update.

**Process:**
1. Write C# descriptor/builder change
2. Run `VerifyJson()` — snapshot shows new JSON shape
3. Run `AssertSchemaValid()` — if it fails, that proves the schema needs updating
4. Review failing test output — does new shape make sense as a contract?
5. Update schema to match
6. Run all schema tests — confirm alignment
7. Commit C# + schema + tests together

**Past drift:** `b5bb10b` (planId), `d1fa967` (enriched props), `4be3e5e` (TS componentType).
Each discovered by accident, not by process.

## Layer 2 — JSON Schema

The contract between C# and TS. Soul of the framework.

**Before editing, verify:**
- Does this express intent minimally?
- Confirm `additionalProperties: false` on every new object.
- Confirm required fields are truly required.
- Check whether this change affects TS types (if yes → boundary crossing).

**Harness:** 310 `AssertSchemaValid()` calls. 26 focused tests in `AllPlansConformToSchema.cs`.

**Gaps:** No TS-to-schema validation. No reverse validation. Drift detection tool not yet built.

## Boundary: Schema → TS Types

A schema change means TS types likely need updating. Write a failing vitest that expects
the new type shape, then update the TS type definition.

**Process:**
1. Schema updated (driven by failing C# test)
2. Write failing vitest using new type — TS compiler error or assertion failure
3. Update TS type in `Scripts/types/`
4. Run `npm test` — confirm alignment
5. Check: is TS type as narrow as schema? Or is it wider/narrower than what C# produces?

**Blind spot:** Zero automation validates TS types match schema. `componentType` was missing
from TS while present in C# and schema for weeks.

## Layer 3 — TS Types & Runtime

**Skills:** `solid-ts-audit`

**Before writing code, verify:**
- Confirm this is the runtime's job, not information the plan should carry.
- Confirm no vendor knowledge leaks outside `component.ts`. Adding a third vendor must only touch component.ts — leaks force changes across the entire runtime.
- Confirm no fallbacks. Fallbacks hide bugs for hours because wrong values propagate silently.
- Confirm no DOM scanning. Plan carries IDs; DOM scanning breaks when IDs change.
- SOLID: SRP ("who requests changes?"), OCP (one switch case + assertNever), LSP (no vendor checks downstream), ISP (narrow exports), DIP (depend inward).

**Harness:** vitest + jsdom via `boot()`. Architecture enforcement tests. `npm run typecheck`.

**Known gaps:** Vendor leaks in trigger.ts:45, live-clear.ts:44. Dual condition evaluators
(21 ops vs 4 ops) can diverge. PlanRegistry over-exported. 3 "ForTests" functions in prod.

## Boundary: Runtime → Browser

Browser first. Not Playwright first. Eyes before automation.
"Tests pass" is necessary but not sufficient. Browser is truth.

**Process:**
1. `npm run build:all && dotnet build` → start SandboxApp
2. Open browser → fill form → click → observe with own eyes
3. Confirm the USER sees the right thing
4. Then write Playwright BDD test

C# unit tests caught 11 bugs. Playwright caught 1 in comparable time. Quick browser
smoke test catches more than elaborate Playwright infrastructure.

## Layer 4 — Browser & Playwright

**Skills:** `bdd-testing`, `superpowers:test-driven-development`

**The 5 BDD Rules** (from `feedback_bdd_constitution`):
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
