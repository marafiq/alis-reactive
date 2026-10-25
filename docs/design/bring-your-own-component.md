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

What a developer writes to bring a component, shown on the thermostat the sandbox wrote itself
(`Alis.Reactive.SandboxApp/Scripts/thermostat.ts`). It is a JavaScript object with every kind of
member once, so the whole method shows on one object. The widget knows nothing of Alis.Reactive: it
keeps itself on its host element as `thermostat`, adds listeners with `on(name, fn)`, removes them
with `off(name, fn)`, and hands a listener the event's payload.

| The object has | Members |
|----------------|---------|
| properties read and written | `value` (the target), `mode`, `schedule.night` |
| properties read | `current`, `locked`, `schedule.active` |
| a property written only | `pin`, a setter without a getter |
| methods that return nothing | `increase()`, `hold(minutes)`, `setRange(min, max)`, `applyPlan(plan)`, `schedule.apply()` |
| methods that return a value | `isComfortable()`, `distanceTo(target)`, `estimate(target, mode)`, `describe(plan)`, `schedule.nextChange()` |
| a method that returns an object | `reading()`: `value`, `unit`, `label()` |
| an event without a payload | `unlocked` |
| an event whose payload is an object | `targetChanging`: `requested`, `previous`, `cancel`, `reason`, `clamp()`, `accept()` |
| the event that commits the value | `targetChanged`: `value`, `previous` |

**Step 1: say where the object is.** The vendor is the three facts the framework needs, and the
only strings in the declaration:

```csharp
public sealed class Thermostat : Component, IInputComponent
{
    public static ComponentVendor ThermostatVendor { get; } = ComponentVendor.Define(
        "thermostat",
        objectPath: "thermostat",              // element.thermostat holds the object
        subscribe: "on", unsubscribe: "off",   // thermostat.on("targetChanged", fn), thermostat.off(..)
        payload: VendorEventPayload.Argument); // a listener receives the payload itself
    public override ComponentVendor Vendor => ThermostatVendor;
```

The app constructs and destroys the widget as it would anyway, in listeners for `alis:mount` and
`alis:unmount` (`Alis.Reactive.SandboxApp/Scripts/sandbox-components.ts`); the framework raises them
for the page at boot and for every injected partial.

**Step 2: declare each member under its JavaScript name.** The C# member's name, camel-cased, is
the JavaScript name: the `Call()` inside `Increase()` calls `increase()`, because the compiler hands
the declaring member's name to every `Prop`, `ReadOnly`, `WriteOnly`, `Call`, `Invoke` and `Event`.
`name:` gives another name where JavaScript differs. The kind of C# member says what the JavaScript
member is (`Alis.Reactive.SandboxApp/BroughtComponents/Thermostat/Thermostat.cs`, doc comments left
out):

```csharp
    public Prop<int> Value => Prop<int>();                                        // read and written
    public Prop<ThermostatMode> Mode => Prop<ThermostatMode>();                   // an enum, by its JavaScript text
    public ReadOnly<int> Current => ReadOnly<int>();                              // read
    public ReadOnly<bool> Locked => ReadOnly<bool>();
    public WriteOnly<string> Pin => WriteOnly<string>();                          // written only: a read does not compile
    public ThermostatSchedule Schedule => Object<ThermostatSchedule>();           // an object with members of its own

    public void Increase() => Call();                                             // no argument
    public void Hold(Arg<decimal> minutes) => Call(minutes);                      // one
    public void SetRange(Arg<int> min, Arg<int> max) => Call(min, max);           // several
    public void ApplyPlan(Arg<ComfortPlan> plan) => Call(plan);                   // an object

    public TypedSource<bool> IsComfortable() => Invoke<bool>();                   // returns a value
    public TypedSource<int> DistanceTo(Arg<int> target) => Invoke<int>(target);
    public TypedSource<int> Estimate(Arg<int> target, Arg<ThermostatMode> mode) => Invoke<int>(target, mode);
    public TypedSource<string> Describe(Arg<ComfortPlan> plan) => Invoke<string>(plan);
    public ThermostatReading Reading() => InvokeObject<ThermostatReading>();      // returns an object

    public TypedEvent<ThermostatUnlockedArgs> Unlocked => Event<ThermostatUnlockedArgs>();
    public TypedEvent<ThermostatTargetChangingArgs> TargetChanging => Event<ThermostatTargetChangingArgs>();
    public TypedEvent<ThermostatTargetChangedArgs> TargetChanged => Event<ThermostatTargetChangedArgs>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: TargetChanged);
}

public sealed class ThermostatSchedule : JsObject                   // thermostat.schedule
{
    public Prop<int> Night => Prop<int>();
    public ReadOnly<bool> Active => ReadOnly<bool>();
    public void Apply() => Call();                                   // schedule.apply()
    public TypedSource<string> NextChange() => Invoke<string>();     // schedule.nextChange()
}

public sealed class ThermostatReading : JsObject                    // what reading() returns
{
    public ReadOnly<int> Value => ReadOnly<int>();
    public ReadOnly<string> Unit => ReadOnly<string>();
    public TypedSource<string> Label() => Invoke<string>();
}

public class ThermostatUnlockedArgs : JsObject                      // an event that carries nothing
{
}

public class ThermostatTargetChangingArgs : JsObject                // a payload the page reads, writes and calls
{
    public ReadOnly<int> Requested => ReadOnly<int>();
    public ReadOnly<int> Previous => ReadOnly<int>();
    public Prop<bool> Cancel => Prop<bool>();
    public Prop<string> Reason => Prop<string>();
    public TypedSource<int> Clamp() => Invoke<int>();
    public void Accept() => Call();
}

public class ThermostatTargetChangedArgs : JsObject
{
    public ReadOnly<int> Value => ReadOnly<int>();
    public ReadOnly<int> Previous => ReadOnly<int>();
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ThermostatMode
{
    [JsonStringEnumMemberName("heat")] Heat,
    [JsonStringEnumMemberName("cool")] Cool,
    [JsonStringEnumMemberName("off")] Off,
}
```

The last line of the component is what forms need: which property holds the value and which event
commits it. Validation, gather and `alis:commit` follow from it with no further code. `ComfortPlan`
is an ordinary C# data class; an argument of that type reaches the method as the object of its
members by their JSON names, whether the page wrote it or a response carried it.

**Step 3: render it, then use it as the object.** The render helper writes the component's own
markup with the id and name the framework derives from the model, and hands the markup back:

```csharp
public static RenderedComponent<Thermostat, TModel> Thermostat<TModel>(
    this InputBoundField<TModel, int> field, int currentTemperature, string pin, bool locked)
    where TModel : class
{
    var html = new StringBuilder()
        .Append("<div class=\"thermostat\" id=\"").Append(Encode(field.ElementId))
        .Append("\" data-name=\"").Append(Encode(field.BindingPath))
        .Append("\" data-target=\"").Append(Target(field))
        .Append("\" data-current=\"").Append(currentTemperature)
        .Append("\" data-pin=\"").Append(Encode(pin))
        .Append("\" data-locked=\"").Append(locked ? "true" : "false")
        .Append("\"></div>");
    return field.Render<Thermostat>(new HtmlString(html.ToString()));
}
```

Anything that is not a form value renders against the plan with the id the developer chose,
`plan.Render<T>(elementId, markup)`. Every view then has the object
(`Components/Brought/Thermostat/Index.cshtml`):

```csharp
Html.InputField(plan, m => m.Target, o => o.Required().Label("Target temperature"))
    .Thermostat(currentTemperature: 68, pin: "1234", locked: true)
    .Reactive(evt => evt.TargetChanging, (args, p) =>
        // The wing's range is the limit: a target outside it is capped at the nearest allowed degree.
        p.When(args.Requested).NotEq(args.Clamp())
            .Then(outside =>
            {
                args.Cancel.Set(true);
                args.Reason.Set("Capped at the wing's limit");
                outside.Component<Thermostat>(m => m.Target).Value.Set(args.Clamp());
                outside.Element("target").Text.Set(args.Clamp());
                outside.Element("capped").Show();
            })
            .Else(inside =>
            {
                args.Accept();
                inside.Element("capped").Hide();
            }))
    .Reactive(evt => evt.TargetChanged, (args, p) =>
    {
        var thermostat = p.Component<Thermostat>(m => m.Target);
        p.Element("target").Text.Set(args.Value);
        p.Element("minutes").Text.Set(thermostat.Estimate(args.Value, thermostat.Mode));
        p.When(thermostat.IsComfortable()).Truthy()
            .Then(near => near.Element("comfortable").Show())
            .Else(far => far.Element("comfortable").Hide());
    })
    .Reactive(evt => evt.Unlocked, (args, p) =>
        p.Element("lock-status").Text.Set("Thermostat unlocked for Margaret's room."));

// On load, the wing's policy from the server sets the range and the night setting.
Html.On(plan, t => t.DomReady(p =>
    p.Get(Url.SandboxRoute("Policy", "Thermostat"))
        .Response(r => r.OnSuccess<ComfortPolicy>((policy, s) =>
        {
            var thermostat = s.Component<Thermostat>(m => m.Target);
            thermostat.SetRange(policy.Get(x => x.Min), policy.Get(x => x.Max));
            thermostat.Schedule.Night.Set(policy.Get(x => x.Night));
            s.Element("next-change").Text.Set(thermostat.Schedule.NextChange());
        }))));

// Elsewhere on the page, one statement each.
thermostat.Increase();
thermostat.Hold(p.Component<FusionNumericTextBox>(m => m.HoldMinutes).Value);
thermostat.Pin.Set(p.Component<NativeTextBox>(m => m.Pin).Value);           // written only
thermostat.Mode.Set(ThermostatMode.Cool);
thermostat.Schedule.Apply();                                                 // a nested method
p.Element("reading").Text.Set(thermostat.Reading().Label());                 // a method on a returned object
thermostat.ApplyPlan(nightPlan);                                             // the response body as the argument
thermostat.ApplyPlan(new ComfortPlan { Target = 72, Mode = ThermostatMode.Cool, Nights = new List<NightSetting> { new NightSetting { Day = "Every night", Target = 72 } } });
```

The plan carries each use as the member's JavaScript path on the located object. `thermostat.Increase()`
is `{"kind":"call","target":{"kind":"obj","id":"..RoomComfortModel__Target"},"path":["increase"],"args":[]}`,
the thermostat is `{"kind":"element","id":"..RoomComfortModel__Target","vendor":"thermostat"}`, and the
vendor entry `{"objectPath":["thermostat"],"subscribe":"on","unsubscribe":"off","payload":"argument"}`
tells the runtime where the object is and how to listen. The runtime names neither the thermostat nor
any vendor.

**Step 4: prove it in a browser.** `WhenSettingRoomComfort`
(`tests/Alis.Reactive.PlaywrightTests/Components/Brought/Thermostat/`) drives one kind of member per
test, each a real gesture and a visible outcome: the range and night setting the policy sets on
load; a degree warmer accepted through the payload; a target above the limit capped through
`Cancel`, `Reason` and `Clamp()`; the room comfortable within a degree; the hold with the minutes
from another component; the right PIN unlocking (a write-only property and an event with no
payload) and a wrong one refused (a read-only property); night mode through the nested object; the
mode as an enum and the reading through the returned object; the wing's plan from the server and
the summer plan written in the page as object arguments; and the save that posts the value.
ALIS009 fails the build if a declaration exposes `object`, `System.Type` or a string that names a
member; the only strings above are the vendor's JavaScript names.

A Syncfusion component is declared the same way
(`Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBox.cs`), its render helper
wrapping the EJ2 builder; `FusionAutoCompleteFilteringArgs`
(`Alis.Reactive.Fusion/Components/FusionAutoComplete/FusionAutoComplete.cs`) is a payload with
behavior like `ThermostatTargetChangingArgs`, and the Notify Physician page
(`Components/Fusion/AutoComplete/Index.cshtml`) uses it. Each write or call is its own statement and
lands in the callback it is written in, whichever variable holds the object.

Vocabulary, by what the JavaScript object has:

| JavaScript | Declared as | Used as |
|------------|-------------|---------|
| property read and written | `public Prop<T> X => Prop<T>();` | `x.X.Set(value or source)`; `x.X` is a source |
| property read | `public ReadOnly<T> X => ReadOnly<T>();` | `x.X` is a source; `x.X.Get(v => v.Member)` reads into its value |
| property written only | `public WriteOnly<T> X => WriteOnly<T>();` | `x.X.Set(value or source)`; it is not a source, so a read does not compile |
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
`Pass<TDeclared, TSource>(items)` passes it (`UpdateData` in `FusionAutoCompleteFilteringArgs`).

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
other. The brought components the sandbox carries, Tom Select and a visit planner and a thermostat it wrote
itself (`Alis.Reactive.SandboxApp/BroughtComponents/`), prove the method with zero runtime changes; the
thermostat has every kind of member once, and `WhenSettingRoomComfort` drives each through the page.

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
