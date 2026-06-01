---
name: bdd-public-api-only
enabled: false
event: file
action: warn
conditions:
  - field: file_path
    operator: regex_match
    pattern: (tests/|Tests/).*\.cs$
  - field: new_text
    operator: regex_match
    pattern: new\s+(ComponentEventTrigger|DomReadyTrigger|CustomEventTrigger|Entry|DispatchCommand|SetPropCommand|MutateElementCommand|ElementCommand|AllGuard|AnyGuard|NotGuard)\s*\(
---

**Internal constructor used in test code.**

Tests should arrange using the public DSL only. Internal constructors bypass builders
and create fragile tests that break on refactors even when behavior is unchanged.

**Public DSL alternatives:**

| Internal Type | Use Instead |
|---------------|-------------|
| `ComponentEventTrigger` | `.Reactive()` extension on the component builder |
| `Entry` | `Html.On(plan, t => t.DomReady(...))` via TriggerBuilder |
| `DomReadyTrigger` | `Trigger(plan).DomReady(p => ...)` |
| `CustomEventTrigger` | `Trigger(plan).CustomEvent("name", p => ...)` |
| `DispatchCommand` | `pipeline.Dispatch("event-name")` |
| `SetPropCommand` | `pipeline.Component(ref).SetProp(...)` |
| `ElementCommand` | `pipeline.Element("#id").SetText(...)` |
| Guards (`All/Any/Not`) | `When(source).Eq(value).And(...)` via condition builders |

50 existing violations tracked for gradual migration (9 test files in Native + Fusion).
Write new tests using the public API. See `memory/bdd-principles.md`.
