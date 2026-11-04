---
paths:
  - "**/*.cs"
---

# C#

## The public surface

- Before a change to the surface, read every caller and every affected file.
- Making an internal member public widens the surface: it needs the same reason and review as any
  other surface change (root CLAUDE.md, Rules).
- Fusion HtmlExtension methods use the `Fusion` prefix (e.g., `.FusionDropDownList()`).

Callback parameter naming convention (locked):

| Callback type | Parameter name |
|---|---|
| `Action<PipelineBuilder>` | `pipeline` |
| `Action<XxxBuilder>` (components) | `build` |
| `Action<TriggerBuilder>` | `trigger` |
| `Action<GatherBuilder>` | `gather` |
| `Action<ResponseBuilder>` | `response` |
| `Func<ConditionStart<TModel>, Guard<TModel>>` | `group` |

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

## Validation Design

The reference is `docs/design/grammar.md#7-validation-projection`: the validator grammar (`ReactiveValidator<T>`, FluentValidation's
syntax), what each check decides on the server and the browser form the plan carries, and what stays on the server by
what it is. The principles that hold:

- Every rule either reaches the browser or is the server's by what it is (a C# predicate or custom check, a message
  built from the model, a condition with no browser form, `.ServerOnly()`). No fallbacks, no silent drops: a condition
  the browser cannot read and that is not declared the server's is refused where the page describes the rules.
- A form is validated by one validator and one rule set; the plan carries that set's rules already filtered, and the
  endpoint validates by the same set.
- Errors are reported under the member's path, as model binding names it; `WithName` changes only the name a message
  says. A nested object's rules come through `SetValidator`, `ChildRules` or `Include`, prefixed by the member's path.
- A date reaches the browser as the text MVC binds (`"yyyy-MM-dd"` at midnight); the facility's timezone is the app's.

### Live Re-Validation
- Industry standard: clear on input, re-validate on blur and on the input's commit event.
- Submit, blur and commit use the same rule evaluation (`runtime/plugins/form/form.ts`, `listenToInputs`).
- Typing clears the message in the capture phase, before the component hears the keystroke, so a commit the component raises from that same input event (EJ2's MaskedTextBox on a paste) re-checks after the clear and its message stays.

## Code Hygiene — Worked Examples

Revealing names, variables close to use, no nesting, no private code no feature reaches, small
single-purpose methods.
These are the worked examples.

**Revealing names over complex conditions.** Extract boolean expressions into named
variables that explain intent. The name is the documentation.

```csharp illustration
// Wrong: condition is opaque
if (field.Shape != null && field.Shape.Kind != "none" && !field.IsServerOnly)
    ExtractRule(field);

// Right: name reveals intent
var requiresClientValidation = field.Shape != null && field.Shape.Kind != "none" && !field.IsServerOnly;
if (requiresClientValidation)
    ExtractRule(field);
```

**Avoid nesting.** Use early returns and guard clauses to flatten control flow.

```csharp illustration
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
