# Bring your own component

Branch `feature/component-redesign` (PR #177). Greenfield: no backward compatibility is kept.
Verification is Playwright only. What the framework must keep doing:
[`capability-catalog.md`](capability-catalog.md). The next design, which connects components,
conditions, validation, arrays, plugins and gather through one core:
[`architecture.md`](architecture.md).

## Goal

A developer takes a dependency on Alis.Reactive and brings any component that already works in the
browser. They install, bundle and construct it. The framework locates its JavaScript object and
gives typed access to its properties, methods and events through the same fluent API every
framework feature speaks. Syncfusion and native components are brought the same way; they are
simply the first ones.

## Principles

1. **The app owns the widget, the framework owns the plan.** The app installs, bundles,
   constructs and destroys its widgets. The framework renders the element with its ID and name,
   locates the object on it, and runs the plan against it. The runtime raises `alis:mount` before
   it wires a piece of HTML and `alis:unmount` before it removes one, so the app constructs and
   destroys at the right moment, including inside partials.

2. **A component is declared once, by its own API.** One class names the object's members as C#
   members: `public Prop<decimal> Min => Prop<decimal>();`, `public void Increment() => Call();`,
   `public TypedEvent<TArgs> Change => Event<TArgs>();`, and says which vendor it comes
   from. The JavaScript name is the C# name camel-cased unless `name:` gives another. Its event
   payloads are declarations too. Nothing else is written per component: no per-member extension
   methods, no events class, no reactive extension, no registration profile.

3. **Behavior hangs off `.Reactive`, where the component is rendered.** The render call returns a
   handle that knows the plan and the ID, so one core `.Reactive(evt => evt.Change, (args, p) =>
   ...)` serves every component, Fusion, Native or brought.

4. **A JavaScript object is used as the object.** `p.Component<T>(..)` returns the declaration,
   live in the callback: `rate.Min.Set(0m)`, `grid.Refresh()`, `p.When(rate.Value).Gt(100m)`. A
   payload is a JavaScript object too: `args.PreventDefaultAction.Set(true)`, `p.When(args.Text)`,
   `args.UpdateData(json.Get(x => x.Items))`. Every value is a literal or any typed source the DSL
   already has: another component's member, a method's result, a payload or response read, a URL
   parameter, a plugin, an array.

5. **The framework dictates identity.** Input IDs from `IdGenerator`, other IDs explicit, `name`
   is the binding path, errors render into `{id}_error`, app-level objects have fixed IDs, views
   carry no JavaScript.

6. **The core names no vendor.** A vendor is data its package declares (`ComponentVendor`): where
   the object lives on the element, how to subscribe and unsubscribe, and what an event hands the
   listener. The plan carries it; the runtime has one driver. An input declares its value (a
   property, which may be nested, such as `input.value`) and the event that commits it
   (`ComponentInput.Of(Value, committedBy: ...)`). The model re-raises that event as `alis:commit`
   on the input's element, and a form re-checks the field on it; no event name is assumed.

7. **Playwright proves it, one component at a time.** A component is brought when its journeys
   pass in a real browser.

## The method

What a developer writes to bring a component. Syncfusion's NumericTextBox, as it now ships (an
excerpt of `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBox.cs`):

```csharp
public sealed class FusionNumericTextBox : FusionComponent, IInputComponent
{
    public Prop<decimal> Value => Prop<decimal>();
    public Prop<decimal> Min => Prop<decimal>();
    public void Increment() => Call();
    public void Decrement() => Call();
    public TypedEvent<FusionNumericTextBoxChangeArgs> Change =>
        Event<FusionNumericTextBoxChangeArgs>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);
}

public class FusionNumericTextBoxChangeArgs : JsObject
{
    public ReadOnly<decimal> Value => ReadOnly<decimal>();
    public ReadOnly<decimal> PreviousValue => ReadOnly<decimal>();
    public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
}

// Render: the framework supplies the ID and name; the markup is the component's own.
public static RenderedComponent<FusionNumericTextBox, TModel> FusionNumericTextBox<TModel, TProp>(
    this InputBoundField<TModel, TProp> field, Action<NumericTextBoxBuilder> build)
    where TModel : class
{
    var builder = field.Helper.EJS().NumericTextBoxFor(field.Expression)
        .HtmlAttributes(new Dictionary<string, object> { ["id"] = field.ElementId, ["name"] = field.BindingPath });
    build(builder);
    return field.Render<FusionNumericTextBox>(builder.Render());
}
```

What every view then has, with no further code from the component author
(`Views/Components/Fusion/NumericTextBox/Index.cshtml`):

```csharp
Html.InputField(plan, m => m.MealsPerWeek, o => o.Label("Catered meals per week"))
    .FusionNumericTextBox(b => b
        .Min(4).Max(21).Step(1).Value(Model.MealsPerWeek).ShowSpinButton(false))
    .Reactive(evt => evt.Change, (args, p) =>
    {
        p.Element("meals-summary").Text.Set(args.Value);
        p.Element("meals-previous").Text.Set(args.PreviousValue);

        p.When(args.IsInteracted).Truthy()
            .Then(chosen => chosen.Element("meals-source")
                .Text.Set("You entered this number of meals."))
            .Else(applied => applied.Element("meals-source")
                .Text.Set("This number of meals was applied from a plan template."));
    });

Html.NativeButton("meals-reduced-diet", "Allow a reduced-diet plan (fewer meals)")
    .Reactive(plan, evt => evt.Click, (args, p) =>
    {
        p.Component<FusionNumericTextBox>(m => m.MealsPerWeek).Min.Set(2);
        p.Element("meals-floor-note").Text.Set("A reduced-diet plan is now allowed — you can enter as few as 2 meals.");
    });
```

A payload with behavior declares its methods the same way
(`FusionAutoComplete.cs`, used by `Views/Components/Fusion/AutoComplete/Index.cshtml`):

```csharp
public class FusionAutoCompleteFilteringArgs : JsObject
{
    public ReadOnly<string> Text => ReadOnly<string>();
    public Prop<bool> PreventDefaultAction => Prop<bool>();
    public void UpdateData<TItems>(TypedSource<TItems> items) where TItems : IEnumerable<object> =>
        Call(Pass<IEnumerable<object>, TItems>(items));
}

.Reactive(evt => evt.Filtering, (args, p) =>
{
    args.PreventDefaultAction.Set(true);
    p.Get(Url.SandboxRoute("Medications", "AutoComplete"))
     .Gather<MedicationSearchQuery>(g => g.Set(x => x.MedicationType, args.Text))
     .Response(r => r.OnSuccess<MedicationSearchResponse>((json, s) =>
         args.UpdateData(json.Get(j => j.Medications))));
})
```

Each write or call is its own statement and lands in the callback it is written in, whichever
variable holds the object: `args.UpdateData(..)` above runs in the success callback, after the
response arrives, against the same payload object.

Vocabulary, by what the JavaScript object has:

| JavaScript | Declared as | Used as |
|------------|-------------|---------|
| property read and written | `public Prop<T> X => Prop<T>();` | `x.X.Set(value or source)`; `x.X` is a source |
| property read | `public ReadOnly<T> X => ReadOnly<T>();` | `x.X` is a source; `x.X.Get(v => v.Member)` reads into its value |
| method | `public void X(Arg<T1> a, ..) => Call(a, ..);` | `x.X(..)` |
| method that returns | `public TypedSource<R> X(Arg<T1> a, ..) => Invoke<R>(a, ..);` | `x.X(..)` is a source |
| method that returns a promise | `CallAsync(..)` or `InvokeAsync<R>(..)` | the steps after it wait; the source is the settled value |
| event | `public TypedEvent<TArgs> X => Event<TArgs>();` | `.Reactive(evt => evt.X, (args, p) => ...)` |
| payload field | `ReadOnly<T>`, or `Prop<T>` when the component reads it back (`Cancel`) | `p.When(args.F)`, `args.Cancel.Set(true)` |
| payload method | a method on a payload that derives from `JsObject` | `args.EndEdit()` |
| plugin | a class deriving `Plugin`, registered with `plan.RegisterPlugin<T>()` | `slug.Slug(name.Value)` |

Arguments. Each parameter is an `Arg<T>`: a literal converts to it, and a `TypedSource<T>` is one,
so literals and runtime values mix: `planner.Book(args.Added, visitors.Value, "Bring the photo album")`.
A generic method infers its type argument from a runtime value: `kanban.AddCard(json.Get(x => x.Card), 0)`;
given only literals, it names it: `kanban.AddCard<CareCard>(card, 0)`. An enum literal is its member name, or the
text its `[JsonStringEnumMemberName]` gives (`"notequal"`); a `DateTime` is ISO text; a response
`json` is itself the whole body. Name each parameter as the JavaScript API names it.

Row and item members. `Field.Of((Resident r) => r.CareLevel)` is the camel-case name a
component uses for a member (`"careLevel"`, nested `"address.city"`). `Field<TValue>` carries the
member's type, so `grid.SetCellValue(6001, Field.Of((Resident r) => r.OpenTasks), 4)` infers `int` from
the field and only compiles with a value of that type. A declaration takes a field as `Field` or
`Field<TValue>`, not an `Arg`: a field is named when the view renders.

Generic members. A member typed by the app's own class is a generic method of the declaration:
`grid.GetSelectedRecords<Resident>()`, `.Reactive(evt => evt.RowSelected<Resident>(), ...)`,
`kanban.AddCard(json.Get(x => x.Card), 0)`.

Fixed arguments. When the JavaScript method always takes a mode the page never chooses, the
declaration passes it: `public void ShowColumn(Field key) => Call(key, Fixed("field"), name:
"showColumns");`, so the page passes only the field. Where the JavaScript method takes a wider type
than the page's own list, `Pass<TDeclared, TSource>(items)` passes it (`UpdateData` above).

Names are the component's own: a member's JavaScript name is its C# name camel-cased, and `name:`
gives another, or a path to one deeper member (`"dataset.target"`, or `"slots[].day"` to read each
item's field); a plugin that is itself a function declares it with `name: ""`. A property that holds
an object with members of its own is declared as that object, `Object<T>()`
(`schedule.EventSettings.DataSource`), and a method that returns one as `InvokeObject<T>(..)`
(`grid.GetColumnByField(..).HeaderText`). Events are named like every other member, by the C# name
camel-cased (Tooltip's `AfterOpen` is EJ2's `afterOpen`). Every use of a member writes its
JavaScript path into the plan, so two C# members can type two signatures of one JavaScript method
and a generic member's type arguments never conflict. A brought vendor and its
components live in the app's own assembly; nothing needs framework internals, and the Fusion and
Native assemblies are built the same way.

Rendering. An input renders through its field, `field.Render<T>(markup)` (or a `TextWriter`
callback); a hidden one through `Html.InputField(plan, m => m.X, o => o.Hidden())`, which drops the
label and message wrapper. Anything else renders against the plan with the ID the developer
chose, `plan.Render<T>(elementId, markup)`. Each returns the handle `.Reactive` hangs off.

App-level objects are objects too. The drawer and loader elements carry the members their
declarations name (`open()`, `close()`, `size`; `show()`, `hide()`, `target`, `timeout`),
defined by their runtime modules; the toast declares Syncfusion's own members, its
look typed as `FusionToastTone`. `p.Confirm("...")` asks through the confirm dialog's layout host.

## Deliverables

Each lands on this branch, proven by the Playwright suites named, and leaves CI green
(`gate / test`, five Playwright shards, `verify-net48`).

**D1. The method, in C#.** `JsObject`, `Component`, the member types the two components below
use, writes, calls and reads with literals and sources, payloads as objects, the render handle
with the core `.Reactive`. (D1 shipped it as three verbs over descriptors, `Set(c => c.Min, 0m)`;
the handle model below replaced them with the members themselves.)
FusionNumericTextBox and FusionAutoComplete are brought with it: one file each, their per-member
extensions, events classes and reactive extensions deleted, their views moved to the verbs.
Plan JSON keeps its shape; no runtime change. Proof: the NumericTextBox and AutoComplete suites
and every journey whose view uses them (Conditions, AdmissionAssessment, AdmissionWizard,
VitalsAlert, EscalationRouting, PlaygroundSyntax, ComponentGather, BddExperiment).

**D2. Vendors as data.** The plan carries each vendor (object path, subscribe, unsubscribe,
payload); one runtime driver replaces the per-vendor drivers and event adapters and their
registry. The runtime raises `alis:mount` before it wires HTML and `alis:unmount` before it removes
it. Proof: the full suite.

**D3. A brought component.** Tom Select, installed in the sandbox the way an app installs it,
declared in the sandbox assembly, constructed on `alis:mount`. One journey across the breadth:
value from a model expression and from a response, a payload read, chained and parallel requests,
required validation into `{id}_error` cleared on change, gather on submit, loaded in a partial and
unloaded without leaking. Proof: that journey.

**D4. Every component brought the same way.** All Fusion and Native slices move to the method,
which grows the members they need as each one lands (methods with typed arguments, returning
methods, member-name keys for overloads). `InternalsVisibleTo` for Fusion and Native is removed;
the core C# and the runtime name no vendor and no component. Proof: the full suite.

## Status

- Lean `CLAUDE.md`, old onboarding tooling removed, hooks fixed.
- Capability catalog from all 176 suites (`2c37ab5`).
- **D1 done:** FusionNumericTextBox and FusionAutoComplete brought with the method (`92875d7`).
- **D2 done:** vendors are plan data, one runtime driver, `alis:mount`/`alis:unmount` (`4a8ac1c`).
- **D3 done:** Tom Select brought in the sandbox assembly (`714e96e`); its reservation test now
  asserts the binding name include-all posts (`71eecfd`).
- **D4 done:** all 51 Fusion slices and the 8 Native slices are one declaration each
  (`7698bda` through `e48f207`); the drawer, loader and toast are objects with declared members
  (`49f3bec`, `fc8b4d2`); Fusion and Native compile on the core's public surface, with
  `InternalsVisibleTo` for both removed (`c597a3a`). The core names no component; the runtime names
  no vendor, as the architecture test enforces.
- **Identity is not checked at render.** A vendor's builder can write its element under another id
  and move the component's id onto it from its init script (Syncfusion's `XxxFor` helpers carry it in
  `htmlAttributes`), and its output is an `HtmlString` like a helper's, so a render-time check
  cannot tell a forgotten id from a vendor's. Two attempts (`05e9f32`, `08eed7a`) broke Syncfusion
  pages in CI and were reverted. A missing element is reported in the browser, where the plan looks
  the component up, naming the id.
- **The core names no component.** `NativeActionLink` moved to the Native package, built on the
  public surface: a declaration whose vendor hands a click handler the DOM event, and
  `Html.ElementPlan(id)`, a plan of its own for one element so it works wherever its HTML lands.
  The id is the developer's (a link in a loop takes its row's key). The callback's type,
  `NativeActionLinkRequest<TModel>`, offers one request to the link's own href (`link.Post()`,
  after `link.Confirm(..).Then(..)` when it asks first), so another URL cannot be written and a
  second request, or none, throws at render. ALIS002 refuses in the view only what a link's own
  plan cannot have: validating a form or sending `Form()`.
- **ALIS009 covers brought components.** The typed-API analyzer applies to any `Component` or
  `JsObject` declaration and any helper that returns `RenderedComponent<,>` or extends
  `InputBoundField<,>`, wherever it is declared, not only the shipped slices' namespaces.
- **Shipped components construct like brought ones.** The framework's own component code lives in
  `Alis.Reactive.Assets/components/`, beside the runtime, and reaches it only through `alis:mount`,
  `alis:unmount` and `window.__alisPlugins`: CheckList, RadioGroup, Drawer, Loader, Confirm (a
  plugin), SmartTextArea, SmartPasteButton and Mention's element bridge. Their helpers render data
  attributes, never an inline `<script>`. The architecture test fails if either side imports the
  other.
- Proof on `615a49b`: CI run 38124316949 green, `gate / test` and all five Playwright shards,
  with no failed or flaky-test annotations; `verify-net48` green (net48 build and pack without
  `InternalsVisibleTo`, IIS Express boot).
- The method grew, as the slices needed it: arguments that mix literals and sources,
  `Field`, generic members, fixed arguments, signature keys, `plan.Render`, hidden fields.
- **A widget the sandbox wrote itself** (`532b2fa` onward): the visit planner
  (`Scripts/visit-planner.ts`, `BroughtComponents/VisitPlanner`) has properties of every shape, functions,
  methods of zero to three arguments, members that return a promise, and events with nested and
  cancelable payloads. Its Family Visits journey drives each through the generic grammar. It added two
  capabilities to the method, both generic: function reads take source arguments, and promise-returning
  members (`CallAsync`, `InvokeAsync<T>`).
- **The handle model** (`f36f62e`): a declaration's members are real C# members (`Prop<T>`,
  `ReadOnly<T>`, methods with `Arg<T>` parameters that call `Call`, `CallAsync`, `Invoke<T>` or
  `InvokeAsync<T>`), and `p.Component<T>(..)` returns the declaration live in the callback. Event
  payloads, response and custom-event data (`Data<T>`), elements (`HtmlElement`) and plugins are used
  the same way. The descriptor types (`ComponentProperty<T>`, `ComponentMethod`, `ComponentFunction`)
  and the verbs over them (`Set(c => ..)`, `Call(c => ..)`, `Get(c => ..)`, `p.Payload(args)`,
  `p.Plugin(..)`, `.Passing`, `.Awaited()`) are gone. The plan did not change: 20 harness pages
  rendered byte-identical plan JSON before and after.
