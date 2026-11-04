---
paths:
  - "**/*.cshtml"
---

# Views

Every view, in the sandbox, the test apps and the examples. A view carries no JavaScript (root CLAUDE.md).

- `ReactivePlan<T>()` is the one plan call: a page, and a partial with its own model, each start a plan with
  it, and every plan stands alone; its id keys nothing (`docs/adr/0003-partials-compose-by-member-and-form.md`, rule 3). A view that starts a plan ends
  with `@Html.RenderPlan(plan)`; without it the plan's components register but its triggers, requests,
  conditions and rules never reach the page. Each plan starts under its owner: the page, or the slot whose
  element holds the plan's script.
- A partial's inputs join the page's forms by the ids their members give them and by the element that holds
  them, whichever plan rendered them.
- Every input goes through `Html.InputField(plan, m => m.X, o => ...)`, with its label and rules; every other
  element a plan names has an id the developer chose.
- Use the framework, never a workaround: `Html.On` for load, `Html.NativeButton(plan, id, text)` and
  `.Reactive(evt => evt.Click, ...)` for a button, `.Validate<TValidator>(formId)` for validation,
  `p.Element(id)` for the page's elements, `.Gather(g => g.Form())` for a form's values.
- Validators register through DI before the view renders; a form's scope is its validator's scope.
- A page carries only what a real application page would: no echo spans, no debug divs, nothing that exists
  only to be asserted. It reads as a product screen, not a test rig.
