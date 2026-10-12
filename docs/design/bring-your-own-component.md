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

2. **A component is declared once, by its own API.** One class names the object's members with
   the JavaScript names: `Property<T>("min")`, `Method("increment")`, `Event<TArgs>("change")`,
   and says which vendor it comes from. Its event payloads are plain classes. Nothing else is
   written per component: no per-member extension methods, no events class, no reactive
   extension, no registration profile.

3. **Behavior hangs off `.Reactive`, where the component is rendered.** The render call returns a
   handle that knows the plan and the ID, so one core `.Reactive(evt => evt.Change, (args, p) =>
   ...)` serves every component, Fusion, Native or brought.

4. **Three verbs touch any JavaScript object.** `Set`, `Call` and `Get` over declared members.
   Every value is a literal or any typed source the DSL already has: another component, a payload
   read, a response read, a URL parameter, a plugin, an array. A payload is a JavaScript object
   too: `p.Payload(args).Set(a => a.PreventDefaultAction, true)`. Payload fields stay plain C#
   properties, read as today with `p.When(args, x => x.Text)`.

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

What a developer writes to bring a component. Syncfusion's NumericTextBox, as it now ships:

```csharp
public sealed class FusionNumericTextBox : FusionComponent, IInputComponent
{
    public ComponentProperty<decimal> Value => Property<decimal>("value");
    public ComponentProperty<decimal> Min => Property<decimal>("min");
    public ComponentMethod Increment => Method("increment");
    public TypedEvent<FusionNumericTextBoxChangeArgs> Change =>
        Event<FusionNumericTextBoxChangeArgs>("change");

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);
}

public class FusionNumericTextBoxChangeArgs
{
    public decimal Value { get; set; }
    public decimal PreviousValue { get; set; }
    public bool IsInteracted { get; set; }
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

What every view then has, with no further code from the component author:

```csharp
Html.InputField(plan, m => m.Rate)
    .FusionNumericTextBox(b => b.Min(0))
    .Reactive(evt => evt.Change, (args, p) =>
    {
        p.When(args, x => x.Value).Gt(100m)
            .Then(t => t.Component<FusionNumericTextBox>(m => m.Ceiling).Set(c => c.Value, 100m));
        p.Get("/rates").Gather(g => g.Include<FusionNumericTextBox, RateModel>(m => m.Rate))
            .Response(r => r.OnSuccess<RateResponse>((res, s) =>
                s.Component<FusionNumericTextBox>(m => m.Ceiling).Set(c => c.Min, res.Read(x => x.Floor))));
    });
```

A payload with behavior declares its methods the same way:

```csharp
public class FusionAutoCompleteFilteringArgs : JsObject
{
    public string Text { get; set; } = "";
    public bool PreventDefaultAction { get; set; }
    public ComponentMethod<IEnumerable<object>> UpdateData => Method<IEnumerable<object>>("updateData");
}

.Reactive(evt => evt.Filtering, (args, p) =>
{
    p.Payload(args).Set(a => a.PreventDefaultAction, true);
    p.Get("/physicians").Gather(g => g.FromEvent(args, x => x.Text, "q"))
        .Response(r => r.OnSuccess<PhysicianList>((res, s) =>
            s.Payload(args).Call(a => a.UpdateData, res.Read(x => x.Items))));
})
```

Vocabulary, by what the JavaScript object has:

| JavaScript | Declared as | Used with |
|------------|-------------|-----------|
| property | `ComponentProperty<T>` via `Property<T>("name")` | `Set(c => c.X, value or source)`, `Get(c => c.X)` |
| method | `ComponentMethod`, `ComponentMethod<T1[, T2[, T3]]>` via `Method<..>("name")` | `Call(c => c.X, args...)` |
| method that returns | `ComponentFunction<[T1, [T2,]] TResult>` via `Function<..>("name")` | `Get(c => c.X, args...)`; each argument a literal or a source |
| method that returns a promise | `Method(..).Awaited()`, `Function<..>(..).Awaited()` | `Call` waits for it; `Get` is the settled value |
| event | `TypedEvent<TArgs>` via `Event<TArgs>("name")` | `.Reactive(evt => evt.X, (args, p) => ...)` |
| payload field | a plain C# property | `p.When(args, x => x.F)`, `p.Payload(args).Set(a => a.F, value)` |
| payload method | `Method("name")` on a payload that derives from `JsObject` | `p.Payload(args).Call(a => a.EndEdit)` |

Arguments. A one-argument call takes a literal or a source. With two or three arguments each
position is an `Argument<T>`, which a literal or a `TypedSource<T>` converts to, so literals and
runtime values mix: `Call(c => c.AddRecordAt<Resident>(), json.Read(x => x.Row), 0)`. An enum
literal is its member name, or the text its `[JsonStringEnumMemberName]` gives (`"notequal"`); a
`DateTime` is ISO text; `json.Read(x => x)` reads the whole body.

Row and item members. `Field.Of((Resident r) => r.CareLevel)` is the camel-case name a
component uses for a member (`"careLevel"`, nested `"address.city"`). `Field<TValue>` carries the
member's type, so `Call(c => c.SetCellValue<int>(), 6001, Field.Of((Resident r) => r.OpenTasks), 4)`
only compiles with a value of that type.

Generic members. A member typed by the app's own class is a generic method of the declaration:
`Get(c => c.GetSelectedRecords<Resident>())`, `.Reactive(evt => evt.RowSelected<Resident>(), ...)`,
`Call(c => c.AddCard<CareCard>(), json.Read(x => x.Card), 0)`.

Fixed arguments. When the JavaScript method always takes a mode after the typed arguments, the
declaration fixes it: `Method<Field>("showColumns").Passing("field")`, so the call passes only the
field.

Names are the component's own; dotted paths reach nested members (`"eventSettings.dataSource"`,
`"dataset.target"`). Events take their JavaScript name; where an event shares its name with a
method, the event takes the past tense (Tooltip's `Opened` beside its `Open` method). The C#
member name keys the declaration, and a method's key also carries its argument shapes
(`setCellValue(number,string,number)`), so two C# members can type two signatures of one
JavaScript method and a generic member's type arguments never conflict. A brought vendor and its
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
use, `Set`/`Call`/`Get` with literals and sources, `p.Payload(args)`, the render handle with the
core `.Reactive`.
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
  What a link cannot carry (another URL, route parameters, IncludeAll, validation, more or fewer
  than one request) is refused in the view by ALIS002 instead of at render.
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
  `Field`, generic members, `Passing`, signature keys, `plan.Render`, hidden fields.
- **A widget the sandbox wrote itself** (`532b2fa` onward): the visit planner
  (`Scripts/visit-planner.ts`, `BroughtComponents/VisitPlanner`) has properties of every shape, functions,
  methods of zero to three arguments, awaited members, and events with nested and cancelable payloads.
  Its Family Visits journey drives each through the generic grammar. It added two capabilities to the
  method, both generic: function reads take source arguments, and `.Awaited()` members.
