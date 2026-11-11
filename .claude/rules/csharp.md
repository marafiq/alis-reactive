---
paths:
  - "**/*.cs"
---

# C#

## Formatting

`dotnet format` formats C# by the root `.editorconfig`: Roslyn's own, changed only where this repo's rules
differ. A guard clause stays on one line, and CA1822 looks only at private members, because a plan node's
`Kind` must stay an instance member for the serializer to write it. `dotnet build` fails on every rule the
`.editorconfig` raises to a warning, and `dotnet format --verify-no-changes` over the solution fails on any file
the formatter would change. Never format by hand or by script.

## The public surface

- Before a change to the surface, read every caller and every affected file.
- Making an internal member public widens the surface: it needs the same reason and review as any
  other surface change (root CLAUDE.md, Rules).
- A Fusion class is Fusion and the EJ2 class it renders; a Native class is Native and what it renders
  (`spec/REWRITE.md`, section 2.4).

Callback parameter naming convention (locked):

| Callback type | Parameter name |
|---|---|
| `Pipeline<TModel>` | `pipeline` |
| `BodyParts<TRequest>` | `request` |
| `QueryParts<TRequest>` | `request` |
| `JsonBody<TRequest>` | `body` |
| `MultipartBody<TRequest>` | `body` |
| `UrlEncodedBody<TRequest>` | `body` |
| `RequestDefaults` | `requests` |
| `Response<TRequest, TReply>` | the app's word for what it holds |
| `Failure<TError>` | the app's word for what it holds |
| `RowActivated<TRow>` | the app's word for what it holds |

The rows name the types the callbacks hand (gate 2, `spec/NOTES.md`). A nested callback's `pipeline` hides the
enclosing one, since every pipeline writes to the block it is written in; no other parameter hides an enclosing
name, since nested callbacks read the replies around them.

## Fail-Fast, Not Fallback

Default thinking is throw, not fallback. Fallbacks hide bugs for hours because wrong
values propagate silently. A fallback exists only where the C# states it and the plan carries it, each the owner's
decision with its reason; none is an agent's.

- No silent fallbacks or default values for missing data — throw immediately.
- No string matching on type names (`GetType().Name.Contains(...)`) — create proper interfaces.
- No reflection hacks — use compile-time type safety.
- No "backward compat" shims — if the contract changes, update all consumers.
- If a third-party library lacks the interface you need, write your own.
- If it feels like a workaround, it IS a workaround — find the clean solution.

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
