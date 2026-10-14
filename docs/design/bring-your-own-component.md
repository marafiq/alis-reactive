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
   members (`FusionNumericTextBox.cs`): `public Prop<decimal> Min => Prop<decimal>();`,
   `public void Increment() => Call();`,
   `public TypedEvent<FusionNumericTextBoxChangeArgs> Change => Event<FusionNumericTextBoxChangeArgs>();`,
   and says which vendor it comes from. The JavaScript name is the C# name camel-cased unless `name:` gives another. Its event
   payloads are declarations too. Nothing else is written per component: no per-member extension
   methods, no events class, no reactive extension, no registration profile.

3. **Behavior hangs off `.Reactive`, where the component is rendered.** The render call returns a
   handle that knows the plan and the ID, so one core `.Reactive(evt => evt.Change, (args, p) =>
   ...)` serves every component, Fusion, Native or brought.

4. **A JavaScript object is used as the object.** `p.Component<T>(..)` returns the declaration,
   live in the callback: `p.Component<FusionNumericTextBox>(m => m.MealsPerWeek).Min.Set(2)`,
   `p.Component<FusionPivotView>(ReportId).Refresh()`, `p.When(hr.Value).Gte(threshold.Value)`. A
   payload is a JavaScript object too: `args.PreventDefaultAction.Set(true)`, `p.When(args.Text).Eq(away.Name)`,
   `args.UpdateData(found.Get(x => x.Physicians))`. (Sandbox views: `Fusion/NumericTextBox/Index.cshtml:72`,
   `Fusion/PivotView/Index.cshtml:41`, `Conditions/NumericCondition/Index.cshtml:91`,
   `Fusion/AutoComplete/Index.cshtml:65, 75`, `Fusion/ListView/Index.cshtml:35`.) Every value is a literal or any typed source the DSL
   already has: another component's member, a method's result, a payload or response read, a URL
   parameter, a plugin, an array.

5. **The framework dictates identity.** Input IDs from `IdGenerator`, other IDs explicit, `name`
   is the binding path, errors render into `{id}_error`, app-level objects have fixed IDs, views
   carry no JavaScript.

6. **The core names no vendor.** A vendor is data its package declares (`ComponentVendor`): where
   the object lives on the element, how to subscribe and unsubscribe, and what an event hands the
   listener. The plan carries it; the runtime has one driver. An input declares its value (a
   property, which may belong to an object the component holds, such as Tom Select's
   `Input.Value`, declared with `Object<T>()`) and the event that commits it
   (`ComponentInput.Of(Value, committedBy: ...)`). The model re-raises that event as `alis:commit`
   on the input's element, and a form re-checks the field on it; no event name is assumed.

7. **Playwright proves it, one component at a time.** A component is brought when its journeys
   pass in a real browser.

## The method

What a developer writes to bring a component. Syncfusion's NumericTextBox, as it ships at `e837ad94`
(`Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBox.cs`: lines 16-41 and 50,
54-64, and 88-97, with the doc comments and the `FocusIn`, `FocusOut`, `Focus` and `Blur` members left out). The
last method is the render helper: the framework supplies the ID and name, and the markup is the
component's own:

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

public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionNumericTextBox, TModel> FusionNumericTextBox<TModel, TProp>(
    this InputBoundField<TModel, TProp> field,
    Action<NumericTextBoxBuilder> build)
    where TModel : class
{
    var builder = field.Helper.EJS().NumericTextBoxFor(field.Expression)
        .HtmlAttributes(new Dictionary<string, object> { ["id"] = field.ElementId, ["name"] = field.BindingPath });
    build(builder);
    return field.Render<global::Alis.Reactive.Fusion.Components.FusionNumericTextBox>(builder.Render());
}
```

What every view then has, with no further code from the component author
(`Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/NumericTextBox/Index.cshtml:26-39` and `:68-74`):

```csharp
@{ Html.InputField(plan, m => m.MealsPerWeek, o => o.Label("Catered meals per week"))
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
    }); }

@(Html.NativeButton("meals-reduced-diet", "Allow a reduced-diet plan (fewer meals)")
    .CssClass(ButtonSecondary)
    .Reactive(plan, evt => evt.Click, (args, p) =>
    {
        p.Component<FusionNumericTextBox>(m => m.MealsPerWeek).Min.Set(2);
        p.Element("meals-floor-note").Text.Set("A reduced-diet plan is now allowed — you can enter as few as 2 meals.");
    }))
```

Alis.Reactive.PlaywrightTests.Components.Fusion.NumericTextBox.WhenNumericValueEntered.a_meal_count_applied_from_a_template_is_marked_as_applied
proves the first: `await Expect(MealsSource).ToHaveTextAsync("This number of meals was applied from a plan template.");`
(tests/Alis.Reactive.PlaywrightTests/Components/Fusion/NumericTextBox/WhenNumericValueEntered.cs:108);
Alis.Reactive.PlaywrightTests.Components.Fusion.NumericTextBox.WhenNumericValueEntered.allowing_a_reduced_diet_lets_the_meal_count_drop_below_the_standard_floor
the second: `await Expect(Meals.Input).ToHaveValueAsync("3.00");`
(tests/Alis.Reactive.PlaywrightTests/Components/Fusion/NumericTextBox/WhenNumericValueEntered.cs:171), where the same entry
was raised to `4.00` before the click (:163).

A payload with behavior declares its methods the same way
(`FusionAutoCompleteFilteringArgs`, `Alis.Reactive.Fusion/Components/FusionAutoComplete/FusionAutoComplete.cs:98-114`, doc comments left out), and
the Notify Physician page uses them
(`Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/AutoComplete/Index.cshtml:62-77`):

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
    // The directory answers for what was typed; until it does, her care team is not offered.
    args.PreventDefaultAction.Set(true);
    p.Get(Url.SandboxRoute("Search", "AutoComplete"))
        .Gather<DirectorySearch>(g => g.Set(x => x.Name, args.Text))
        .WhileLoading(l => l.Element("directory-status").Text.Set("Searching the physician directory..."))
        .Response(r => r.OnSuccess<DirectoryMatches>((found, s) =>
        {
            s.Element("directory-status").Text.Set(found.Get(x => x.Summary));
            // A name nobody in the directory has closes the list the last search left open.
            s.When(found.Get(x => x.Count)).Eq(0)
                .Then(none => none.Component<FusionAutoComplete>(m => m.Physician).HidePopup())
                .Else(some => args.UpdateData(found.Get(x => x.Physicians)));
        }));
})
```

Alis.Reactive.PlaywrightTests.Components.Fusion.AutoComplete.WhenUsingFusionAutoComplete.typing_part_of_a_name_offers_the_physicians_the_directory_matches proves `UpdateData`:
`await Expect(Physician.PopupItems).ToHaveTextAsync(new[] { "Dr. Anita Patel", "Dr. Lena Patterson" });`
(tests/Alis.Reactive.PlaywrightTests/Components/Fusion/AutoComplete/WhenUsingFusionAutoComplete.cs:88); Alis.Reactive.PlaywrightTests.Components.Fusion.AutoComplete.WhenUsingFusionAutoComplete.while_the_directory_is_searched_her_care_team_is_not_offered_for_what_was_typed proves
`PreventDefaultAction`: `await Expect(Physician.PopupItems).ToHaveCountAsync(0);` (tests/Alis.Reactive.PlaywrightTests/Components/Fusion/AutoComplete/WhenUsingFusionAutoComplete.cs:102), while the search is
still running (:101, :103); Alis.Reactive.PlaywrightTests.Components.Fusion.AutoComplete.WhenUsingFusionAutoComplete.a_name_nobody_in_the_directory_has_closes_the_list_and_says_so proves the `Then`
branch: `await Expect(Physician.Popup).ToBeHiddenAsync();` (tests/Alis.Reactive.PlaywrightTests/Components/Fusion/AutoComplete/WhenUsingFusionAutoComplete.cs:119).

Each write or call is its own statement and lands in the callback it is written in, whichever
variable holds the object: `args.UpdateData(..)` above runs in the success callback's `Else` branch,
after the response arrives, against the same payload object.

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
| plugin | a class deriving `Plugin`, registered with `plan.RegisterPlugin<T>()` | `slugify.Slug(planner.Wing)` |

Arguments. Each parameter is an `Arg<T>`: a literal converts to it, and a `TypedSource<T>` is one,
so literals and runtime values mix: `p.Component<FusionMention>(HandoffMentionId).Search(nextNurse.Value, 64, 640)`
(`Fusion/Mention/Index.cshtml:99`). A generic method infers its type argument from a runtime value:
`s.Component<FusionKanban>(BoardId).AddCard(json.Get(x => x.WorkOrder), 0)` (`Fusion/Kanban/Index.cshtml:77`);
given only literals, it names it: `p.Component<FusionGrid>(RosterId).AddRecordAt<MapleRosterResident>(respiteGuest, 0)`
(`Fusion/Grid/Editing.cshtml:102`). An enum literal is its member name, or the text its
`[JsonStringEnumMemberName]` gives (`"notequal"`, `Alis.Reactive.Fusion/Components/FusionGrid/FusionGrid.cs:287`);
a `DateTime` travels as ISO text and reaches the method as a `Date`, the shape its parameter declares
(`Alis.Reactive.Assets/runtime/grammar/evaluate.ts:58`); a response `json` is itself the whole body. Name each
parameter as the JavaScript API names it.

Row and item members. `Field.Of((DirectoryResident r) => r.RiskLevel)` is the camel-case name a
component uses for a member (`"riskLevel"`; nested members are dotted, `"address.city"`,
`Alis.Reactive/Components/Contracts/Field.cs:17`). `Field<TValue>` carries the member's type, so
`SetCellValue(count.Get(x => x.ResidentId), Field.Of((EveningRoundsResident x) => x.OpenTasks), count.Get(x => x.OpenTasks))`
(`Fusion/Grid/KeyedUpdate.cshtml:48-49`) infers `int` from the field and only compiles with a value of that type. A declaration takes a field as `Field` or
`Field<TValue>`, not an `Arg`: a field is named when the view renders.

Generic members. A member typed by the app's own class is a generic method of the declaration:
`grid.GetSelectedRecords<DirectoryResident>()`, `.Reactive(evt => evt.RowSelected<DirectoryResident>(), (args, p) =>`
(`Fusion/Grid/Directory.cshtml:212, 169`), `AddCard(json.Get(x => x.WorkOrder), 0)`.

Fixed arguments. When the JavaScript method always takes a mode the page never chooses, the
declaration passes it: `public void ShowColumn(Field key) => Call(key, Fixed("field"), name:
"showColumns");` (`Alis.Reactive.Fusion/Components/FusionGrid/FusionGrid.cs:107`), so the page passes only the field. Where the JavaScript method takes a wider type
than the page's own list, `Pass<TDeclared, TSource>(items)` passes it (`UpdateData` above).

Names are the component's own: a member's JavaScript name is its C# name camel-cased, and `name:`
gives another, one JavaScript name; a plugin that is itself a function declares it with `name: ""`
(`Invoke<string>(text, name: "")`, `Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Components/Brought/VisitPlanner/VisitBoardModel.cs:84`).
A field of each item of a list is read through the list property, typed by its items:
`OfferedDays => Slots.Each(slot => slot.Day)` reads every slot's `day`
(`Alis.Reactive.SandboxApp/BroughtComponents/VisitPlanner/VisitPlanner.cs:56`). A property that holds
an object with members of its own is declared as that object, `Object<T>()`
(`schedule.EventSettings.DataSource`, `Fusion/Schedule/Index.cshtml:47`), and a method that returns one as
`InvokeObject<T>(..)` (`sheet.GetColumnByField(wing).HeaderText`, `Fusion/Grid/CareStaffColumns.cshtml:71`). Events are named like every other member, by the C# name
camel-cased (Tooltip's `AfterOpen` is EJ2's `afterOpen`, `Alis.Reactive.Fusion/Components/FusionTooltip/FusionTooltip.cs:41`). Every use of a member writes its
JavaScript path into the plan, so two C# members can type two signatures of one JavaScript method
and a generic member's type arguments never conflict. A brought vendor and its
components live in the app's own assembly; nothing needs framework internals, and the Fusion and
Native assemblies are built the same way.

Rendering. An input renders through its field, `field.Render<T>(markup)` (or a `TextWriter`
callback, `Alis.Reactive.Fusion/Components/FusionRichTextEditor/FusionRichTextEditor.cs:80`); a hidden one through
`o.Hidden()` (`Alis.Reactive/InputField/InputFieldOptions.cs:33`), which drops the label and message wrapper;
pages write it `@Html.HiddenFieldFor(plan, m => m.Wing)` (`Native/NativeHiddenField/Index.cshtml:24`). Anything else renders against the plan with the ID the developer
chose, `plan.Render<T>(elementId, markup)`. Each returns the handle `.Reactive` hangs off.

App-level objects are objects too. The drawer and loader elements carry the members their
declarations name (`open()`, `close()`, `size`; `show()`, `hide()`, `target`, `timeout`),
defined by the framework's component code (`Alis.Reactive.Assets/components/native/drawer.ts`, `loader.ts`);
the toast declares Syncfusion's own members, its look typed as `FusionToastTone`
(`toast.Tone.Set(FusionToastTone.Success)`, `Patterns/Todo/Index.cshtml:52`). `p.Confirm("...")` asks the
`alis.confirm` plugin: the Fusion confirm on a page whose layout renders its dialog
(`Alis.Reactive.Assets/components/fusion/confirm.ts:104-112`), or a page's own, as the Room Holds page gives one.

## Deliverables

Each lands on this branch, proven by the Playwright suites named, and leaves CI green
(`gate / test`, five Playwright shards, `verify-net48`).

**D1. The method, in C#.** `JsObject`, `Component`, the member types the two components below
use, writes, calls and reads with literals and sources, payloads as objects, the render handle
with the core `.Reactive`. (D1 shipped it as three verbs over descriptors, `Set(c => c.Min, 0m)`;
the handle model below replaced them with the members themselves.)
FusionNumericTextBox and FusionAutoComplete are brought with it: one file each, their per-member
extensions, events classes and reactive extensions deleted, their views moved to the verbs.
Plan JSON keeps its shape; no runtime change. Proof at `92875d7`: the NumericTextBox and AutoComplete suites
and every journey whose view uses them (Conditions, AdmissionAssessment, AdmissionWizard,
VitalsAlert, EscalationRouting, PlaygroundSyntax, ComponentGather, BddExperiment). PlaygroundSyntax and
ComponentGather were replaced since by Room Offer (`17420df1`) and Move-in Packet (`7c373175`).

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
  (`7698bda` through `e48f207`; nine Native slices at `e837ad94`, with NativeActionLink, below); the drawer, loader and toast are objects with declared members
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
  `alis:unmount` and `window.__alisPlugins`: CheckList, RadioGroup, HiddenField, Drawer, Loader, Confirm
  (a plugin), FileUpload, SmartTextArea, SmartPasteButton and Mention's element bridge
  (`Alis.Reactive.Assets/components/native/`, `components/fusion/`). Their helpers render data
  attributes, never an inline `<script>`. The architecture test fails if either side imports the
  other (`Alis.Reactive.Assets/runtime/__tests__/architecture.test.ts:120-134`). Since `e4fd8d32` a value
  the page writes to a NativeCheckList or NativeRadioGroup checks exactly the boxes or the radio it names, and a
  NativeHiddenField raises `change` for a new value; since `beab8f92` the uploader's input commits on
  `FilesChanged`, which its component code raises once EJ2 holds the chosen files.
- Proof on `615a49b`: CI run 38124316949 green, `gate / test` and all five Playwright shards,
  with no failed or flaky-test annotations; `verify-net48` green (net48 build and pack without
  `InternalsVisibleTo`, IIS Express boot). At `e837ad94`: CI run 38568507905 green, 1,583 tests
  passing at the first attempt and two InPlaceEditor medical-record-number tests only on a re-run
  (flaky-test annotations); `verify-net48` run 38568508122 green.
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
