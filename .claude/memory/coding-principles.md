---
name: Coding Principles
description: Consolidated principles for Alis.Reactive development — API surface, fail-fast, plan-render, research discipline, secrets, validation design, and session mistake patterns
type: reference
---

# Coding Principles

## API Surface Is Frozen

The public API surface does not change without explicit user approval and full downstream
analysis. A single parameter rename cascaded across 170+ files in 6 commits.

- A hookify rule blocks API surface edits at the tool level.
- Before any change: grep all call sites, read every affected file, present evidence.
- `internal` to `public` is strictly forbidden. Internal members protect the surface deliberately.
- Fusion HtmlExtension methods use the `Fusion` prefix (e.g., `.FusionDropDownList()`).
- Event args properties: `{ get; private set; }` for Fusion, "Gets" voice in XML docs.
- No "Syncfusion" prefix in XML docs — use framework class names (e.g., "FusionAutoComplete").

Callback parameter naming convention (locked):

| Callback type | Parameter name |
|---|---|
| `Action<PipelineBuilder>` | `pipeline` |
| `Action<XxxBuilder>` (components) | `build` |
| `Action<TriggerBuilder>` | `trigger` |
| `Action<GatherBuilder>` | `gather` |
| `Action<ResponseBuilder>` | `response` |
| `Action<HttpRequestBuilder>` | `request` |
| `Func<ConditionSourceBuilder, GuardBuilder>` | `guard` |
| `Func<ConditionStart, GuardBuilder>` | `inner` |

## Fail-Fast, Not Fallback

Default thinking is throw, not fallback. Fallbacks hide bugs for hours because wrong
values propagate silently. A fallback is a rare, deliberate, justified exception.

- No silent fallbacks or default values for missing data — throw immediately.
- No string matching on type names (`GetType().Name.Contains(...)`) — create proper interfaces.
- No reflection hacks — use compile-time type safety.
- No "backward compat" shims — if the schema changes, update all consumers.
- No `// TODO` or `// FIXME` — fix it now or don't write it.
- If a third-party library lacks the interface you need, write your own.
- If it feels like a workaround, it IS a workaround — find the clean solution.

## Plan-Render Rule

Every view that creates or resolves a plan MUST call `@Html.RenderPlan(plan)`.

- `ReactivePlan<T>()` — creates a new plan (parent view or independent partial).
- `ResolvePlan<T>()` — creates a plan that merges by planId (partial sharing same model).
- Both MUST end with `@Html.RenderPlan(plan)`.

The runtime discovers all `[data-reactive-plan]` blocks and merges them by planId. Without
RenderPlan, entries are lost — components register but reactive behaviors (conditions, HTTP,
validation) never serialize.

No manual JS in views. No `document.addEventListener` in `.cshtml`. No `window.alis`.
No inline `<script>` blocks. `root.ts` handles discovery and boot automatically.

## Research Before Iterate

After 2 rounds of fail-fix-fail, STOP coding and use WebSearch.

The loop:
1. Run 1 failing test.
2. See the error.
3. Check browser / find where same pattern works.
4. Make the change.
5. Run that 1 test.
6. If fail, go back to step 2.
7. After 2 rounds of fail: research on the internet.

Search for: `[component name] [framework] playwright [specific behavior]`.
Chrome MCP tools behave differently from Playwright — do not trust MCP for SF component
debugging. Run Playwright diagnostic tests instead of manual browser clicking for SF components.

This saved 2 days of debugging on SF DropDownList Playwright interactions. The ArrowDown
behavior was documented online and findable in 5 minutes.

## Never Read Secrets

Never run `dotnet user-secrets list` or any command that reads/displays secret values.
When the user says a secret is set up, trust them and move on. If something fails due to
a missing secret, mention the possibility without reading the secret store.

## Validation Design

### Schema Principles
- `shape` on ValidationRule is derived from `TProperty` at C# compile time. Never guess or infer at runtime.
- `field` on ValidationRule for cross-property comparisons uses the same deterministic ID system as everything else: TModel, prop expression, known type, predictable ID.
- Schema must be deterministic — no fallbacks, no silent drops. Every FluentValidation rule either extracts to a client rule or is explicitly documented as server-only.
- `Empty()` and `Null()` are extractable. PrecisionScale, IsInEnum, IsEnumName are server-only.

### Cross-Property Comparisons
- Same `min`/`max`/`gt`/`lt` rule types — `constraint` for fixed value, `field` for cross-property. Mutually exclusive.
- Cross-property reads use the same mechanism: binding path, enriched fieldId, resolveRoot, walk(readExpr). No scanning, no new concepts.

### Date Handling
- `shape: "date"` uses `toDate()` from `core/shape-convert.ts` — handles Date objects (SF), ISO strings, date-only strings with timezone safety.
- DateTime constraints serialize as `"YYYY-MM-DD"` when time is midnight — parsed as local midnight, timezone-safe.
- Facility timezone is the application's responsibility.

### Validator Scope
- Validator scope = form scope. Always. List the fields a view renders before creating the validator.
- Nested properties require `SetValidator()`. Direct chain (`RuleFor(x => x.Address.Street)`) is silently dropped by the FluentValidation adapter.
- Verify the extracted field list in the plan JSON.

### Live Re-Validation
- Industry standard: clear on input, re-validate on blur/change.
- `validateContainer()` handles submit; `revalidateField()` handles blur. Both use the same rule evaluation logic.

## Validation Session Mistake Patterns

These patterns are forbidden. Each was learned the hard way.

1. **Surface tests disguised as BDD.** Tests that assert true/false on functions are not BDD. Every Playwright test must be a full user journey: fill form, submit, see errors, fix, resubmit, success. Verify in the actual browser before committing.

2. **Changing core behavior without understanding existing design.** Before changing behavior, trace the full lifecycle in existing code. Understand WHY the current behavior exists. The code may already handle the case through a different mechanism.

3. **Patch-fix cycle instead of root cause analysis.** When something breaks, STOP. Read the code path end-to-end. Identify the exact line. Fix THAT line. Run ALL tests and verify in browser BEFORE committing. Ten patches is ten mistakes.

4. **Never testing in the actual browser.** After any validation or UI change, open the browser. Fill the form. Click submit. See with your own eyes. Playwright tests only test what they are written to test.

5. **Claiming "all tests pass" while the browser is broken.** Passing tests are necessary but not sufficient. Done means: tests pass AND browser works AND user confirms. Test count means nothing — test quality catches bugs.

6. **Ignoring user feedback and continuing to patch.** When told to stop patching, ACTUALLY STOP. Step back, read the code, think, then make ONE correct change.
