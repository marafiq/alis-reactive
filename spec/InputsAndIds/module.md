# Inputs and ids

An input bound to a member of the view's model, under MVC partials: the field that renders it, the declaration that
makes a component an input, the id the framework gives it, a pipeline's reach to it, and the refusals where the view
renders and where the page composes its plans. Module 7 of `spec/CLAUDE.md`.

## Core questions

- Q2: an input binds under MVC's binding name, the member's C# path under its view's prefix, on both hosts (D2.5).
- Q3: an input's value is the one expression C# writes, `coalesce(as(read, shape), blank)` (N3.3), and blank is what
  the member binds as (D3.5, D3.6).
- Q7: an input a member binds is a source at every value position (D7.2), reached by its member (N14.4).
- Q12: an input id a live plan holds is taken by no plan that starts later, a slot's content included (R14.6).
- Q13: a member's value as its container sends it is the container's read (N13.1), read in its step from the inputs
  the container holds then (`ContainerRead.tla`); a container that holds two inputs of one binding name sends nothing
  and its read stops (X13.2, D13.4).
- Q14: one binding name and one id per member in every view that renders it (D14.3), a partial for a member by one
  typed door (D14.4), and the closed refusals within one plan and across plans (D14.6).
- Q16: a failure shows at the message element of the input sent under its binding name, and in the summary place
  (D16.13); the input's commit runs its live check (D16.15).
- Q17: a component is one declaration and a render helper (D17.2), started in the state the model gives it (D17.13,
  N17.10).
- Q18: the declaring class of the member a lambda names is a type's identity, within the no-reflection rule (D18.1).

## Boundaries

`Html.PartialFor` (N14.1), the plan's two doors (N14.2), the request container (N14.3) and `pipeline.Input` (N14.4) are
the core's doors, and this module's rows carry them. The plan's own types (`ReactivePlan<TModel>`, `PlanExtensions`)
are module 9's. A component's render helper is its slice's, Fusion's in module 21 and Native's in module 24.
`RenderedComponent<TComponent, TModel>`, which `Render<TComponent>` returns and whose `Reactive` runs a pipeline on
the component's event, and both lines' `Component<TComponent>(string)` and `Component<TComponent>()`, which reach a
component by an id the developer chose, serve every component, an input among them, and are module 12's.

## Doors

Each name is gate 2's; each door is the capability it carries.

- `M7.N1` kept: `IdGenerator.For<TModel, TProp>(m => m.Member)`, the id of the input a member binds when rendered with
  no prefix, as D14.3 derives it: the class that declares the member, a generic one named as C# writes it, then MVC's
  id of the member's path. A view names an id by it: a label's `for`, or another view's input, which the page's input
  table holds by that id (D14.2). It replaces main's `IdGenerator.For(Type, string)` and `IdGenerator.TypeScope(Type)`;
  the object-typed `IdGenerator.For<TModel>(m => ..)` both lines have leaves for it, since ALIS009 refuses `object` on
  the public surface.
- `M7.N2` kept: `Html.InputField(plan, m => m.Member)` and `Html.InputField(plan, m => m.Member, options => ..)`, with
  `InputFieldOptions`'s `Label`, `Required` and `Hidden` (M7.D1). It returns the bound field a component's render
  helper builds on: the view's `Helper`, the member's `Expression`, its `ElementId` and `BindingPath`, N17.10's
  starting value and options, and `Render<TComponent>(content)`, which registers the component at the id and takes
  an input only (M7.X2). The base's `Plan` and `Options` getters leave as implementation details: the field registers
  its component and renders its own label and marker, and no view, test or render helper of Fusion or Native reads
  either at 65fdf118d.
- `M7.N3` kept: `IInputComponent.Input`, a `ComponentInput.Of(value, committedBy)` built from the declaration's typed
  value property and the event that commits it: the read an input's binding takes (N3.3) and the event its live check
  runs on (D16.15). It replaces main's `IInputComponent.ValueMember`, a string.
- `M7.N4` kept: the redesign's `services.AddReactiveClassNames(names => ..)` and `ReactiveClassNames`, the class names
  every field renders with, `Field`, `FieldLabel`, `FieldRequired`, `FieldMessage` and `InvalidInput`, chosen once at
  startup, each unset name its default; C# writes `InvalidInput` into each input's binding as its `invalidClass`
  (schema.md), so an app styles its fields with its own stylesheet (M7.X7). Its startup form is gate 2's.

The meaning `M7.D1` is in `spec/InputsAndIds/features.json`, on the feature it gives.

## Improvements

- One id for a member in every view, by the class that declares it (D14.3, M7.N1). Main scopes the id by the view's
  model class (`Alis.Reactive/PlanAuthoring/ExpressionPaths/IdGenerator.cs:33` at 623dd4a27), so a partial typed to a
  base class of the page's model renders an id the page's plan does not name (M7.X4); the redesign scopes it by the
  declaring class (`Alis.Reactive/Inputs/IdGenerator.cs:50-61` at 65fdf118d), which M7.N1 keeps.
- An input declares its value and its commit by type (M7.N3). Main names the value member by a string
  (`Alis.Reactive/Components/Contracts/IComponent.cs:20` at 623dd4a27), which its browser reads by name
  (`Alis.Reactive.Assets/runtime/execution/requests/gather.ts:168` at 623dd4a27), and declares no committing event; the
  redesign's `ComponentInput.Of` takes the declared `ReadOnly<T>` and `Event<TArgs>`
  (`Alis.Reactive/Components/ComponentInput.cs:41-47` at 65fdf118d), and its browser rechecks the input on that event
  (`Alis.Reactive.Assets/runtime/plugins/model.ts:203-216` at 65fdf118d).
- A field renders an input only, by type (M7.X2). Main throws where the view renders when a render helper did not
  register its component (`Alis.Reactive/InputField/InputBoundFieldBase.cs:71-75` at 623dd4a27), a step its own
  packages reach through internals; the redesign's `Render<TComponent>` requires an `IInputComponent`
  (`Alis.Reactive/Razor/InputBoundField.cs:59-60` at 65fdf118d).
- A partial for a member by one typed door (N14.1). Views copy `ViewData` and set the copy's prefix as text
  (`Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Patterns/MoveInContacts/Index.cshtml:17-18`,
  `tests/Alis.Reactive.Framework.App/Inputs/Circle.cshtml:8-11` at 65fdf118d), which on net48 moves the view's own
  prefix, since the copy shares its `TemplateInfo` (D14.4, X14.11).
- An id two plans render is met where the page composes them (D14.6, R14.6). The redesign refuses a field another plan
  rendered where the second view renders, through a per-request record
  (`Alis.Reactive/Razor/InputFieldExtensions.cs:89` at 65fdf118d), which D14.1 refuses; a slot's content that renders
  the id is met in the browser (X14.7).
- A hidden field's failure shows in the summary place (M7.D1), as the redesign's test pins
  (`tests/Alis.Reactive.Framework.PlaywrightTests/Inputs/WhenAFieldIsRendered.cs:71-80` at 65fdf118d); D16.13 names
  the summary for a `Set` member and a native control, and not for a framework input with no message element.
