---
paths:
  - "**/*.cshtml"
---

# Views

Every view, in the conformance app and in an app. A view is a `.cshtml` MVC view, on MVC 5 and on ASP.NET Core MVC,
never a Razor Page, and it carries no JavaScript: `WhenAViewIsWritten` refuses an inline script, a handler attribute
and a script URL in any tracked view, and a layout links a script by its file.

- A view writes the doors its unit has landed, in the form `spec/Fluent/sentences.json` gives each and `spec/api/`
  compiles. A door a view needs and the DSL lacks is a gap to raise with its use case, never a workaround in the
  view.
- A view opens its plan with `Html.ReactivePlan<TModel>()` and writes it with `Html.RenderPlan(plan)`, its last
  line, after every door that takes the plan. `ALIS041` refuses at build a plan no line hands to `Html.RenderPlan`,
  a plan rendered twice and a door that takes the plan after it rendered. Where the build does not follow a view's
  code, a `@section` among it, rendering the view refuses the last two with the same sentence
  (`spec/Core/DESIGN.md`, D14.7 and D18.6).
- The framework controls every input's id; every other element a plan names has an id the developer chose.
- A partial has a tight model of its own, and plan state never travels by weak typing: no `ViewData`, nothing stored
  on the request.
- A screen that needs a model has a model of its own, shared with no other view.
- A page carries only what a real application page would: no echo spans, no debug divs, nothing that exists
  only to be asserted. It reads as a product screen, not a test rig.

Two meanings every view meets, restated from their D lines in `spec/Core/DESIGN.md`:
- A member's name (D2.4 to D2.6, D2.9). A request body names each member as the endpoint's binder reads it: on
  net10 the name System.Text.Json's contract gives it under the app's JSON options, on net48 its C# name. Form-data
  and query keys are the member's C# path on both hosts. Data read from a response body or a pushed message is
  named as the endpoint writes it, under the app's JSON options. No name is camel-cased by a rule of the
  framework's own.
- Absent data (D4.2). A member a present holder lacks reads its start, the value System.Text.Json binds for it
  under the app's options. A holder that holds null or reads nothing reads nothing, and so does every member past
  it.
