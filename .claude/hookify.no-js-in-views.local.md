---
name: no-js-in-views
enabled: false
event: file
action: block
conditions:
  - field: file_path
    operator: regex_match
    pattern: \.cshtml$
  - field: new_text
    operator: regex_match
    pattern: <script[\s>]|document\.(addEventListener|querySelector|getElementById)|window\.(alis|addEventListener)|\.onclick\s*=
---

**BLOCKED: JavaScript detected in a .cshtml view.**

All browser behavior flows through the reactive plan. Views contain zero inline scripts.
`root.ts` auto-discovers `[data-reactive-plan]` elements and executes the plan.

**Instead of inline JS, use:**
- `Html.On(plan, trigger: t => t.DomReady(...))` for page-load behavior
- `Html.On(plan, trigger: t => t.CustomEvent("name", ...))` for event-driven behavior
- `.Reactive(plan, evt => evt.Changed, ...)` for component events
- `p.Dispatch("event-name")` for cross-component communication

The only allowed `<script>` in views is `@Html.RenderPlan(plan)` which emits a `<script type="application/json">` data element (not executable JS).