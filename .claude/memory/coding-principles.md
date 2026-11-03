---
name: Coding Principles
description: Consolidated principles for Alis.Reactive development — API surface, fail-fast, plan-render, research discipline, secrets, validation design, and session mistake patterns
type: reference
---

# Coding Principles

## API Surface Is Frozen

The public API surface does not change without explicit user approval and full downstream
analysis. A single parameter rename cascaded across 170+ files in 6 commits.

- No hook blocks API surface edits — review manually.
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
| `Func<ConditionStart<TModel>, Condition<TModel>>` | `group` |

## Fail-Fast, Not Fallback

Default thinking is throw, not fallback. Fallbacks hide bugs for hours because wrong
values propagate silently. A fallback is a rare, deliberate, justified exception.

- No silent fallbacks or default values for missing data — throw immediately.
- No string matching on type names (`GetType().Name.Contains(...)`) — create proper interfaces.
- No reflection hacks — use compile-time type safety.
- No "backward compat" shims — if the contract changes, update all consumers.
- No `// TODO` or `// FIXME` — fix it now or don't write it.
- If a third-party library lacks the interface you need, write your own.
- If it feels like a workaround, it IS a workaround — find the clean solution.

## Plan-Render Rule

Every view that starts a plan MUST call `@Html.RenderPlan(plan)`.

- `ReactivePlan<T>()` is the one plan call: a page, and a partial with its own model, each start a plan
  with it (ADR-0003 rule 3). Every plan stands alone; the plan id keys nothing.
- A partial's inputs join the page's forms by the ids their members give them and by the element that
  holds them, whichever plan rendered them.

The runtime discovers every `[data-reactive-plan]` block and starts it under its owner (the page, or the
slot whose element holds its script). Without RenderPlan, entries are lost: components register but
reactive behaviors (conditions, HTTP, validation) never serialize.

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

The reference is `docs/design/grammar.md` section 7: the validator grammar (`ReactiveValidator<T>`, FluentValidation's
syntax), what each check decides on the server and the browser form the plan carries, and what stays on the server by
what it is. The principles that hold:

- Every rule either reaches the browser or is the server's by what it is (a C# predicate or custom check, a message
  built from the model, a condition with no browser form, `.ServerOnly()`). No fallbacks, no silent drops: a condition
  the browser cannot read and that is not declared the server's is refused where the page describes the rules.
- A form is validated by one validator and one rule set; the plan carries that set's rules already filtered, and the
  endpoint validates by the same set.
- Errors are reported under the member's path, as model binding names it; `WithName` changes only the name a message
  says. A nested object's rules come through `SetValidator`, `ChildRules` or `Include`, prefixed by the member's path.
- A date reaches the browser as the text MVC binds (`yyyy-MM-dd` at midnight); the facility's timezone is the app's.

### Live Re-Validation
- Industry standard: clear on input, re-validate on blur and on the input's commit event.
- Submit, blur and commit use the same rule evaluation (`runtime/plugins/form/form.ts`, `wireLive`).
- Typing clears the message in the capture phase, before the component hears the keystroke, so a commit the component raises from that same input event (EJ2's MaskedTextBox on a paste) re-checks after the clear and its message stays.

## Validation Session Mistake Patterns

These patterns are forbidden. Each was learned the hard way.

1. **Surface tests disguised as BDD.** Tests that assert true/false on functions are not BDD. Every Playwright test must be a full user journey: fill form, submit, see errors, fix, resubmit, success. Verify in the actual browser before committing.

2. **Changing core behavior without understanding existing design.** Before changing behavior, trace the full lifecycle in existing code. Understand WHY the current behavior exists. The code may already handle the case through a different mechanism.

3. **Patch-fix cycle instead of root cause analysis.** When something breaks, STOP. Read the code path end-to-end. Identify the exact line. Fix THAT line. Run ALL tests and verify in browser BEFORE committing. Ten patches is ten mistakes.

4. **Never testing in the actual browser.** After any validation or UI change, open the browser. Fill the form. Click submit. See with your own eyes. Playwright tests only test what they are written to test.

5. **Claiming "all tests pass" while the browser is broken.** Passing tests are necessary but not sufficient. Done means: tests pass AND browser works AND user confirms. Test count means nothing — test quality catches bugs.

6. **Ignoring user feedback and continuing to patch.** When told to stop patching, ACTUALLY STOP. Step back, read the code, think, then make ONE correct change.

## Code Hygiene — Worked Examples

Revealing names, variables close to use, no nesting, no dead code, small single-purpose methods.
These are the worked examples.

**Revealing names over complex conditions.** Extract boolean expressions into named
variables that explain intent. The name is the documentation.

```csharp
// Wrong: condition is opaque
if (field.Shape != null && field.Shape.Kind != "none" && !field.IsServerOnly)
    ExtractRule(field);

// Right: name reveals intent
var requiresClientValidation = field.Shape != null && field.Shape.Kind != "none" && !field.IsServerOnly;
if (requiresClientValidation)
    ExtractRule(field);
```

**Avoid nesting.** Use early returns and guard clauses to flatten control flow.

```csharp
// Wrong: nested
if (component != null)
{
    if (component.Vendor == "fusion")
    {
        // 20 lines of logic
    }
}

// Right: guard clause, flat
if (component == null) return;
if (component.Vendor != "fusion") return;
// 20 lines of logic
```
