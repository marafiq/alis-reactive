---
name: BDD Framework Primitives Rule
description: Sandbox views must ALWAYS use framework primitives (Html.On, Html.TextBoxFor, Component<T>, .Reactive(), .Validate<T>) — never raw HTML, never workarounds
type: feedback
---

## Rule

Sandbox pages must use framework primitives to build views. Never work around the framework.
The existing codebase has extensive evidence of correct usage — follow those patterns.

**Why:** Sandbox pages ARE the test bed. If a page uses raw HTML instead of `Html.TextBoxFor()`,
the Playwright test can't verify that the builder, IdGenerator, validation extraction, and
gather pipeline work correctly. The page IS the test — it must exercise the real framework.

**How to apply:**
- Always: `Html.On(plan, t => t.DomReady(...))` — never raw `<script>`
- Always: `Html.TextBoxFor(m => m.Name)` — never raw `<input type="text">`
- Always: `Component<NativeButton>().Reactive(evt => evt.Click, ...)` — never raw onclick
- Always: `.Validate<TValidator>()` — never manual validation logic
- Always: `p.Element("id").SetText(...)` — never raw DOM manipulation
- Always: `.Gather(g => g.IncludeAll())` — never manual form serialization
- Always: `Html.Field(m => m.Name).Required().Label("Name")` — never raw label/input combos

Look at existing views for patterns: Events/Index.cshtml, Validation/Index.cshtml,
DropDownList/Index.cshtml, ComponentGather/Index.cshtml.
