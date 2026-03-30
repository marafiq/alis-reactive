---
name: Plan must always be rendered
description: Every view with a plan MUST call RenderPlan — ResolvePlan vs ReactivePlan only differs in how the plan is created, not whether it renders
type: feedback
---

Every view that creates or resolves a plan MUST call `@Html.RenderPlan(plan)`. The plan is ALWAYS rendered.

- `ReactivePlan<T>()` — creates a new plan (parent view or independent partial with different model)
- `ResolvePlan<T>()` — creates a plan that merges by planId (partial sharing same model as parent)

Both MUST call `@Html.RenderPlan(plan)` at the end. The runtime discovers all `[data-reactive-plan]` blocks and merges them by planId. Without RenderPlan, the entries are lost — components register but reactive behaviors (conditions, HTTP, validation) don't serialize.

**Why:** `ResolvePlan` creates a SEPARATE plan instance (same planId, different object). Its entries are independent. The runtime merges multiple plan blocks with the same planId. If a sub-partial doesn't call RenderPlan, its entries (conditions, HTTP calls, save buttons) are never emitted to the DOM.

**How to apply:** When writing any `.cshtml` that calls `ReactivePlan` or `ResolvePlan`, ALWAYS end with `@Html.RenderPlan(plan)`.
