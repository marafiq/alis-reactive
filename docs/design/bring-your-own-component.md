# Bring your own component

A developer takes a dependency on Alis.Reactive and brings any component that already works in the
browser: a Syncfusion widget, a native input, a third-party library, or one the app wrote itself.
They install, bundle and construct it. The framework locates its JavaScript object and gives typed
access to its properties, methods and events through the one fluent API every framework feature
speaks. The Fusion and Native packages are brought the same way; they are simply the first
components. The plan a page renders is specified in [`grammar.md`](grammar.md).

## Principles

1. **The app owns the widget, the framework owns the plan.** The app installs, bundles,
   constructs and destroys its widgets. The framework renders the element with its id and name,
   locates the object on it, and runs the plan against it. The runtime raises `alis:mount` before
   it wires a piece of HTML and `alis:unmount` before it removes one, so the app constructs and
   destroys at the right moment, including inside partials.

2. **A component is declared once, by its own API.** One class names the object's members as C#
   members: `public Prop<decimal> Min => Prop<decimal>();`, `public void Increment() => Call();`,
   `public TypedEvent<FusionNumericTextBoxChangeArgs> Change => Event<FusionNumericTextBoxChangeArgs>();`,
   and says which vendor it comes from. The JavaScript name is the C# name camel-cased unless
   `name:` gives another. Its event payloads are declarations too. Nothing else is written per
   component: no per-member extension methods, no events class, no reactive extension, no
   registration profile.

3. **Behavior hangs off `.Reactive`, where the component is rendered.** The render call returns a
   handle that knows the plan and the id, so one core `.Reactive(evt => evt.Change, (args, p) =>
   ...)` serves every component, Fusion, Native or brought.

4. **A JavaScript object is used as the object.** `p.Component<T>(..)` returns the declaration,
   live in the callback: `p.Component<FusionNumericTextBox>(m => m.MealsPerWeek).Min.Set(2)`,
   `p.Component<FusionPivotView>(ReportId).Refresh()`, `p.When(hr.Value).Gte(threshold.Value)`. A
   payload is a JavaScript object too: `args.PreventDefaultAction.Set(true)`,
   `p.When(args.Text).Eq(away.Name)`, `args.UpdateData(found.Get(x => x.Physicians))`. Every value
   is a literal or any typed source the DSL already has: another component's member, a method's
   result, a payload or response read, a URL parameter, a plugin, an array.

5. **The framework dictates identity.** Input ids come from `IdGenerator`, other ids are explicit,
   `name` is the binding path, errors render into `{id}_error`, app-level objects have fixed ids,
   and views carry no JavaScript.

6. **The core names no vendor.** A vendor is data its package declares (`ComponentVendor`): where
   the object lives on the element, how to subscribe and unsubscribe, and what an event hands the
   listener. The plan carries it; the runtime has one driver. An input declares its value (a
   property, which may belong to an object the component holds, such as Tom Select's `Input.Value`,
   declared with `Object<T>()`) and the event that commits it
   (`ComponentInput.Of(Value, committedBy: ...)`). The model re-raises that event as `alis:commit`
   on the input's element, and a form re-checks the field on it; no event name is assumed.

7. **Playwright proves it, one component at a time.** A component is brought when its journeys
   pass in a real browser.

## The method

What a developer writes to bring a component, shown on Syncfusion's NumericTextBox
(`Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBox.cs`, doc comments and
the focus members left out). The last method is the render helper: the framework supplies the id
and name, and the markup is the component's own:

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

What every view then has, with no further code from the component author. Sandbox view paths
below are under `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/`; this one is
`Components/Fusion/NumericTextBox/Index.cshtml`:

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

The `WhenNumericValueEntered` fixture under
`tests/Alis.Reactive.PlaywrightTests/Components/Fusion/NumericTextBox/` proves both: a meal count
applied from a template is marked as applied, and after the click a count below the standard floor
is accepted.

A payload with behavior declares its methods the same way (`FusionAutoCompleteFilteringArgs` in
`Alis.Reactive.Fusion/Components/FusionAutoComplete/FusionAutoComplete.cs`), and the Notify
Physician page uses them (`Components/Fusion/AutoComplete/Index.cshtml`):

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

The `WhenUsingFusionAutoComplete` fixture proves `UpdateData` (the physicians the directory matches
are offered), `PreventDefaultAction` (nothing is offered while the search runs) and the `Then`
branch (a name nobody has closes the list).

Each write or call is its own statement and lands in the callback it is written in, whichever
variable holds the object: `args.UpdateData(..)` above runs in the success callback's `Else`
branch, after the response arrives, against the same payload object.

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

**Arguments.** Each parameter is an `Arg<T>`: a literal converts to it, and a `TypedSource<T>` is
one, so literals and runtime values mix:
`p.Component<FusionMention>(HandoffMentionId).Search(nextNurse.Value, 64, 640)`
(`Components/Fusion/Mention/Index.cshtml`). A generic method infers its type argument from a
runtime value: `s.Component<FusionKanban>(BoardId).AddCard(json.Get(x => x.WorkOrder), 0)`
(`Components/Fusion/Kanban/Index.cshtml`); given only literals, it names it:
`p.Component<FusionGrid>(RosterId).AddRecordAt<MapleRosterResident>(respiteGuest, 0)`
(`Components/Fusion/Grid/Editing.cshtml`). An enum literal is its member name, or the text its
`[JsonStringEnumMemberName]` gives; a `DateTime` travels as ISO text and reaches the method as a
`Date`, the shape its parameter declares; a response `json` is itself the whole body. Name each
parameter as the JavaScript API names it.

**Row and item members.** `Field.Of((DirectoryResident r) => r.RiskLevel)` is the camel-case name a
component uses for a member (`"riskLevel"`; nested members are dotted, `"address.city"`).
`Field<TValue>` carries the member's type, so
`SetCellValue(count.Get(x => x.ResidentId), Field.Of((EveningRoundsResident x) => x.OpenTasks), count.Get(x => x.OpenTasks))`
(`Components/Fusion/Grid/KeyedUpdate.cshtml`) infers `int` from the field and only compiles with a
value of that type. A declaration takes a field as `Field` or `Field<TValue>`, not an `Arg`: a field
is named when the view renders.

**Generic members.** A member typed by the app's own class is a generic method of the declaration:
`grid.GetSelectedRecords<DirectoryResident>()`,
`.Reactive(evt => evt.RowSelected<DirectoryResident>(), (args, p) =>`
(`Components/Fusion/Grid/Directory.cshtml`), `AddCard(json.Get(x => x.WorkOrder), 0)`.

**Fixed arguments.** When the JavaScript method always takes a mode the page never chooses, the
declaration passes it: `public void ShowColumn(Field key) => Call(key, Fixed("field"), name: "showColumns");`
(`Alis.Reactive.Fusion/Components/FusionGrid/FusionGrid.cs`), so the page passes only the field.
Where the JavaScript method takes a wider type than the page's own list,
`Pass<TDeclared, TSource>(items)` passes it (`UpdateData` above).

**Names** are the component's own: a member's JavaScript name is its C# name camel-cased, and
`name:` gives another, one JavaScript name; a plugin that is itself a function declares it with
`name: ""` (`Invoke<string>(text, name: "")`, the visit board's slug plugin under
`Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Components/Brought/VisitPlanner/`). A field of each
item of a list is read through the list property, typed by its items:
`OfferedDays => Slots.Each(slot => slot.Day)` reads every slot's `day`
(`Alis.Reactive.SandboxApp/BroughtComponents/VisitPlanner/VisitPlanner.cs`). A property that holds
an object with members of its own is declared as that object, `Object<T>()`
(`schedule.EventSettings.Events<ShiftAssignment>()`, `Components/Fusion/Schedule/Index.cshtml`), and a method that
returns one as `InvokeObject<T>(..)` (`sheet.GetColumnByField(wing).HeaderText`,
`Components/Fusion/Grid/CareStaffColumns.cshtml`). Events are named like every other member, by the
C# name camel-cased (Tooltip's `AfterOpen` is EJ2's `afterOpen`). Every use of a member writes its
JavaScript path into the plan, so two C# members can type two signatures of one JavaScript method
and a generic member's type arguments never conflict. A brought vendor and its components live in
the app's own assembly; nothing needs framework internals, and the Fusion and Native assemblies are
built the same way.

**Rendering.** An input renders through its field, `field.Render<T>(markup)` (or a `TextWriter`
callback, as `FusionRichTextEditor` does); a hidden one through `o.Hidden()`
(`Alis.Reactive/InputField/InputFieldOptions.cs`), which drops the label and message wrapper; pages
write it `@Html.HiddenFieldFor(plan, m => m.Wing)` (`Components/Native/NativeHiddenField/Index.cshtml`).
Anything else renders against the plan with the id the developer chose,
`plan.Render<T>(elementId, markup)`. Each returns the handle `.Reactive` hangs off.

**App-level objects are objects too.** The drawer and loader elements carry the members their
declarations name (`open()`, `close()`, `size`; `show()`, `hide()`, `target`, `timeout`), defined by
the framework's component code (`Alis.Reactive.Assets/components/native/drawer.ts`, `loader.ts`);
the toast declares Syncfusion's own members, its look typed as `FusionToastTone`
(`toast.Tone.Set(FusionToastTone.Success)`, `Patterns/Todo/Index.cshtml`). `p.Confirm("...")` asks
the `alis.confirm` plugin: the Fusion confirm on a page whose layout renders its dialog
(`Alis.Reactive.Assets/components/fusion/confirm.ts`), or a page's own, as the Room Holds page gives
one with a native `<dialog>`.

**The framework's own components are built like brought ones.** Their JavaScript lives in
`Alis.Reactive.Assets/components/`, beside the runtime, and reaches it only through `alis:mount`,
`alis:unmount` and `window.__alisPlugins`: CheckList, RadioGroup, HiddenField, Drawer, Loader,
Confirm, FileUpload, SmartTextArea, SmartPasteButton and Mention's element bridge. Their helpers
render data attributes, never an inline `<script>`. The architecture test
(`Alis.Reactive.Assets/runtime/__tests__/architecture.test.ts`) fails if either side imports the
other. The brought components the sandbox carries, Tom Select and a visit planner it wrote itself
(`Alis.Reactive.SandboxApp/BroughtComponents/`), prove the method with zero framework changes.

## Limits

What the method does not yet do. Each is a known gap, not a hidden one.

- A view that renders a component under the wrong element id is reported in the browser, where the
  plan looks the component up, not when the view renders: Syncfusion's helpers put the id on the
  element from their init script, so a render-time check cannot tell a forgotten id from a vendor's.
- A declaration is not generic over the app's row type (`FusionGrid<TRow>`); row-typed members are
  generic methods with `Field` arguments.
- The validation grammar covers what the sandbox uses. Rule sets, cascade modes, dependent rules,
  custom validators, enum membership and precision checks are not built; an app that needs one
  adds it as a check.
- Four declared members have no journey yet: Dialog `RefreshPosition`, Kanban `CancelEdit`,
  Schedule `CloseEditor` and `NativeHiddenField.Change`.

## Decisions

Alternatives that were tried or weighed and rejected, kept so they are not re-litigated.

- **Descriptors plus verbs** (`Set(c => c.Min, 0m)`, `Call(c => c.Refresh)`): ceremony on every
  use, reads and writes that differed by source kind, no returned objects. Replaced by the members
  themselves.
- **Interfaces plus expression trees, or recording proxies:** a runtime value cannot pass through
  a `T` parameter, events cannot appear in expression trees, and proxies need `Reflection.Emit` on
  net48.
- **Realtime as trigger kinds of their own:** they leave `EventSource` and `HubConnection` outside
  the object model.
- **Lambdas instead of the fluent conditions** (`p.When(m => m.Age > 65)`): a second way to write
  a branch. The fluent API keeps its shape and lowers to `op`.
- **Operators on values** (`rate > 100m`): needs `==` overloaded on a class and cannot restrict
  ordering by type.
- **A source generator:** rejected by the owner.
- **Feature nodes in the plan** (`request`, `dispatch`, `inject`, a `confirm` condition, a trigger
  kind per source): each brings a sub-grammar the runtime must know, and a brought component could
  never do what a built-in feature does.
- **`await` inside expressions:** the evaluator would become asynchronous everywhere and a payload
  write could land after the vendor reads it. `let … await` keeps one synchronous evaluator and the
  async lane visible in the plan.
- **Blocks as values** (`http.send(request, onSuccess, onError)`): the plan would become a general
  language. `if`, `try` and `all` express every lifecycle the DSL has.
