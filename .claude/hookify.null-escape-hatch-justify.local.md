---
name: null-escape-hatch-justify
enabled: false
event: file
action: warn
conditions:
  - field: file_path
    operator: regex_match
    pattern: Alis\.Reactive/PlanModel/.*\.cs$
  - field: new_text
    operator: regex_match
    pattern: JsonIgnore\(Condition\s*=\s*JsonIgnoreCondition\.WhenWritingNull\)
---

**WARNING: You are adding a `[JsonIgnore(WhenWritingNull)]` attribute to a plan model property.**

Before committing this, answer in writing (in a code comment, commit message, or chat):

1. **Could this property have a domain default instead?** (Shape.None, Path.None, ValueProducer.None, Condition.None, Array.Empty, empty Dictionary, empty string, false, etc.)
2. **If yes — why am I taking the shortcut?** Don't answer "for consistency with other nullable properties" — that propagates debt.
3. **If no — what domain meaning would the sentinel collide with?** Be specific. "Empty string is a valid value here" → why? "0 is a valid status code" → trace the actual usage.

Properties where null is GENUINELY the only honest representation in this domain:
- Discriminator fields where exactly-one-of-two is the constraint (PathSegment.Name vs Index)
- Recursive plan structures where a sentinel creates infinite recursion (Request.Next)
- Numeric optionals where 0 is meaningful (HTTP Status code)

Properties where "null" is usually laziness:
- Identifier strings (PartId, BindingPath, ValueMember) — empty string is the "not set" semantic
- Recursive Shape sub-properties (Item, Inner, Fields) — Shape.None recursively works
- Boolean optionals — `bool` with default false is fine
- Conditions with else-cases — `Condition.None` (always-true) works

**Why this rule exists:** In the `fix/null-design-smell` branch (2026-04-12), 14 new `[JsonIgnore(WhenWritingNull)]` attributes were added without per-property justification. The user's critique was that this LOOKED like adding tech debt even though the total count of null sites went down. The lesson: when removing null tech debt, every NEW null escape hatch must be PROVEN necessary, not mechanically added.

This rule warns (not blocks) — you can still add the attribute, but you must commit a justification. A reviewer will check.
