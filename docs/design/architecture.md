# Architecture: one object model, one value model, a lean runtime

Proposal on `feature/component-redesign` at `0452a4e`. It supersedes the proposal in the earlier
`redesign-decisions.md`. What is already built is described in
[`bring-your-own-component.md`](bring-your-own-component.md).

The constraints:
- everything is fully typed;
- every feature is preserved;
- the runtime stays a dumb executor of what the C# domain decides;
- greenfield otherwise.

## 1. What must survive

These are the features that make Alis.Reactive what it is. The design must keep every one of them:

1. **Behavior is typed C#, written where the component renders.** Views carry no JavaScript. The
   plan is data, and the generated TS contract turns drift between C# and the runtime into a
   typecheck failure.
2. **Validation is written once.** A FluentValidation rule runs on the server and in the browser,
   including conditional, nested and per-item rules, live clearing and revalidation, server
   errors mapped back to fields, and grid column rules from the same validator.
3. **A typed HTTP pipeline.**
   - Values gathered from any source, URL templates, typed responses routed by status.
   - Chained and parallel requests, `WhileLoading`, `Finally`, validation before send.
   - Injection into partial slots.
4. **Partial slots.** An injected partial brings its own plan, components, rules and realtime
   connections, and it unloads without leaking.
5. **Realtime as triggers:** ServerPush and SignalR.
6. **Every component, typed, from any vendor,** with deterministic IDs.
7. **Correct event semantics.** Payload writes such as `cancel` land synchronously, before the
   vendor reads them, and a payload method can be called after an async gap.
8. **Arrays over any source,** feeding components, requests, conditions and validation.
9. **Cross-model writes.** A partial can write a field of the shell's model.
10. **net48 and net10 from one source.**

## 2. What a JavaScript object is, measured

Measured from the 66 declarations at HEAD, plus the other objects the DSL reaches.

| Kind | Count or examples | What it needs |
|---|---|---|
| Property | 99, two of them nested (`eventSettings.dataSource`, `dataSourceSettings.dataSource`) | read; write from a literal or any runtime value |
| Method | 210: 146 with no arguments, 35 with one, 13 with two, 3 with three. Nested paths (`element.classList.add`) | call with literals or runtime values; overloads keyed by signature |
| Returning method | 13 | used as a value: in text, conditions, gather, arrays |
| Generic member | 3, plus 15 row-field arguments and 3 fixed mode arguments | typed by the app's row type |
| Event | 145, with 128 payload classes holding 370 data fields | sync payload reads and writes; payload methods (4) after an async gap |
| Returned object | `getColumnByField(f).visible` (vendor API; no view yet) | members on a value a method returned |
| App object | Drawer, Loader, Toast, Confirm | fixed ID; Confirm is asynchronous |
| DOM element | text, html, classes, show/hide; `classList`, `children` as arrays | members of the element |
| Plugin | functions, properties, commands, a root function | found by name; arguments of any kind |
| Document | custom events (dispatch and listen), page ready | events |
| Location | query parameters | typed reads |
| Network | HTTP request and response, EventSource, SignalR hub | data in and out; events |
| Data | response bodies, push messages, dispatch payloads, rows, model fields | read by path; arrays |

There are two kinds of thing here:
- **Objects** are live and have members: properties, methods and events.
- **Data** is JSON-shaped and read by path.

Every kind of value the DSL touches is one of these two.

## 3. Three ways to type an object in C#

**A. Descriptors plus verbs** (shipped in D1 to D4):

```csharp
rate.Set(c => c.Min, 0m);
grid.Call(c => c.SelectRow, 2);
grid.Get(c => c.GetSelectedRecords<Resident>());
```

**B. Interfaces plus expression trees** (the earlier proposal):

```csharp
grid.Call(g => g.SelectRow(2));
```

**C. Typed handles.** The declaration is the object, and its members are real C# members:

```csharp
grid.SelectRow(2);
rate.Min.Set(0m);
grid.GetSelectedRecords();
```

| Need from section 2 | A | B | C |
|---|---|---|---|
| Runtime value as an argument | `Argument<T>`, up to 3 arguments | not possible: a `T` parameter cannot carry a runtime value without a marker | `Arg<T>` parameters, any arity |
| Nested object (`eventSettings.dataSource`) | a dotted string | expression path | a property that is itself an object |
| Returned object | not modelled | not possible: the receiver must be a parameter | a method returns an object handle |
| Row-typed members | `Field.Of((Row r) => ..)`, generic methods, `.Passing` | expression arguments | generic declaration `FusionGrid<TRow>`, as Syncfusion Blazor's `SfGrid<TValue>` |
| Event in the declaration | `TypedEvent` property | events cannot appear in expression trees | `Event<T>` property |
| Payload writes and methods | `p.Payload(args).Set(..)`, with reads in another form (`p.When(args, x => x.V)`) | as for objects | `args.Cancel.Set(true)`, `args.UpdateData(items)` |
| Reads like the vendor's API | no: `Call(c => c.Show)` | close | yes: `dialog.Show()`, `grid.Refresh()` |
| Pattern an agent repeats | `ref.Verb(c => c.M, args)` | `ref.Verb(o => o.M(args))` | `obj.M(args)`, `obj.P.Set(v)` |
| Per-use ceremony | verb and lambda | verb and lambda | none |
| Scope correctness | a ref is bound to the pipeline that made it, so a write inside a response scope lands in the trigger pipeline (`ComponentRef.cs:16,185`) | same as A | writes land in the active scope (section 4.4) |
| net48 | yes | yes | yes: no `Reflection.Emit`, no static abstracts |

**Choice: C.** It is the only approach that covers every row:
- Syncfusion's own JavaScript API reads one-to-one: `dialog.Show()`, `grid.DataSource.Set(rows)`,
  `schedule.EventSettings.DataSource.Set(rows)`.
- No shortcut layer is needed and no generator.
- A JavaScript method is one C# member that states its own types.

## 4. The core domain (C#)

### 4.1 Objects

```csharp
public abstract class JsObject                          // a live JavaScript object
{
    protected Prop<T> Prop<T>([CallerMemberName] string name = "");
    protected TObject Object<TObject>([CallerMemberName] string name = "") where TObject : JsObject, new();
    protected void Call(IArg a1, /* ... a5 */ [CallerMemberName] string name = "");
    protected Value<TResult> Invoke<TResult>(IArg a1, /* ... */ [CallerMemberName] string name = "");
    protected Value<TResult> InvokeAsync<TResult>(IArg a1, /* ... */ [CallerMemberName] string name = "");
    protected TObject InvokeObject<TObject>(IArg a1, /* ... */ [CallerMemberName] string name = "") where TObject : JsObject, new();
    protected Event<TPayload> Event<TPayload>([CallerMemberName] string name = "") where TPayload : JsObject, new();
}
```

- **Names.** A C# name camel-cases to the JavaScript name. Pass `name:` only when the JavaScript
  name differs, for example `"input.value"` or `"showColumns"`.
- **Arguments.**
  - The base takes `IArg`, and each public member types its parameters as `Arg<T>`.
  - C# cannot define a conversion to an interface, so a fixed literal inside a declaration is
    written `Fixed("field")`.
  - `Column(field)` turns a row expression into the column's camel-case name.
- **Member keys.** A member's plan key is its name plus its argument shapes, as today, so overloads
  and generic instantiations never collide.

```csharp
public sealed class FusionGrid<TRow> : FusionComponent
{
    public Prop<TRow[]> DataSource => Prop<TRow[]>();
    public void SelectRow(Arg<int> index) => Call(index);
    public void Refresh() => Call();
    public Value<TRow[]> GetSelectedRecords() => Invoke<TRow[]>();
    public void SetCellValue<TValue>(Arg<object> key, Expression<Func<TRow, TValue>> field, Arg<TValue> value)
        => Call(key, Column(field), value);
    public void ShowColumn(Expression<Func<TRow, object>> field) => Call(Column(field), Fixed("field"), name: "showColumns");
    public FusionGridColumn GetColumnByField(Expression<Func<TRow, object>> field) => InvokeObject<FusionGridColumn>(Column(field));
    public Event<FusionGridRowSelected<TRow>> RowSelected => Event<FusionGridRowSelected<TRow>>();
}

public sealed class FusionSchedule<TEvent> : FusionComponent
{
    public FusionScheduleEventSettings<TEvent> EventSettings => Object<FusionScheduleEventSettings<TEvent>>();
}

public sealed class FusionGridRowSelected<TRow> : JsObject      // a payload is an object too
{
    public Prop<TRow> Data => Prop<TRow>();                      // the row is data
    public Prop<int> RowIndex => Prop<int>();
    public Prop<bool> Cancel => Prop<bool>();
}
```

- `Field.Of`, `Field<TValue>`, `.Passing`, `ComponentMethod<T1..T3>`, `ComponentFunction<..>`,
  `Argument<T>` and `PayloadRef` are all deleted. A member's own C# signature replaces them.

### 4.2 Inputs

Gather, validation and live revalidation need two facts about an input. The declaration states
both, with types:

```csharp
public abstract class Input<TValue> : JsObject
{
    public abstract Prop<TValue> Value { get; }   // what is posted, gathered and validated (Tom Select: name: "input.value")
    public abstract Event Committed { get; }      // after this event the value is final: drives revalidation
}
```

This replaces two things:
- the string `IInputComponent.ValueMember`;
- the runtime's hard-coded `"change"` (`live-clear.ts:102`).

The uploader declares its value as its files. That removes the special case for Syncfusion's
`rawFile` (`request-payload-writer.ts:163`).

### 4.3 Values and data

- **`IValue<out T>` and `Value<T>`.**
  - `Value<T>` is a read with a C# type. The type decides the shape: number, text, bool, date,
    array or object.
  - It replaces the eight `TypedSource` subclasses.
  - Because it is covariant, LINQ works over arrays, lists and enumerables alike.
- **`Arg<T>`** converts implicitly from a `T` literal and from any `IValue<T>`. Literals and runtime
  values take the same position in every call.
- **Data paths.** `value.Get(x => x.A.B[0].C)` reads into data such as a response, a row or a
  query. A missing link reads as `undefined`, which is JavaScript's own semantics. So C#'s `?.`,
  which expression trees reject, is never needed.
- **Sources of values:**

  | Source | Form |
  |---|---|
  | object members | `rate.Value`, `grid.GetSelectedRecords()` |
  | a model field (the input bound to it, resolved at plan render, so no component type is needed) | `p.Field(m => m.Age)` |
  | the URL's query string, typed by the same class the action binds with `[FromQuery]` | `p.Url<CensusQuery>()` |
  | a response | `(res, s) => res.Get(x => x.Rows)` |
  | a push message | `(msg, p) => msg.Get(..)` |
  | a dispatched event's payload | `(e, p) => e.Get(..)` |
  | the request snapshot | `p.Request` |
  | a literal | implicit |
- **Arrays.** `IValue<IEnumerable<T>>` carries the full LINQ set:
  - `Where`, `Select`, `SelectMany`;
  - `OrderBy`, `OrderByDescending`, `ThenBy`;
  - `Any`, `All`, `Count`;
  - `Sum`, `Min`, `Max`, `Average`;
  - `First`, `FirstOrDefault`, `Contains`;
  - `Distinct`, `Skip`, `Take`, `Concat`.

  Each one lowers to JavaScript's own array method with a compiled lambda (section 6).
- **Text.** `IValue<string>` has `Length`, `Contains`, `StartsWith`, `EndsWith`, `Trim`, `ToUpper`
  and `ToLower`.

### 4.4 Scopes and the active-scope invariant

- **Where a write lands.** A handle holds no pipeline. `Set` and `Call` append to the plan's active
  scope.
- **The scope stack.** Every builder that takes a callback pushes the callback's scope, invokes the
  callback immediately, and pops the scope. This covers triggers, `.Reactive`, `Then`, `ElseIf`,
  `Else`, `OnSuccess`, `OnError`, `Chained`, `WhileLoading`, `Finally`, `Parallel` branches and the
  link's request.
- **What that guarantees:**
  - A write lands in the scope it is lexically written in, whichever variable holds the handle.
  - A handle can be declared once at the top of a view and used everywhere.
  - Using one outside any scope throws, naming the member.
- **Why this is safe.** Plan building is synchronous and single-threaded. The stack lives on the
  plan's build context, not in thread-static state.

### 4.5 Conditions: the owner's fluent grammar with the defects removed

```csharp
p.When(args.Value).Gt(100m).Then(t => ceiling.Value.Set(100m))
 .ElseIf(p.Field(m => m.CareLevel)).Eq(CareLevel.Memory).Then(..)
 .Else(..);
```

- **Sources.** Every source is one argument: any `IValue<T>`. The `(args, x => x.Path)` pair form,
  used 146 times today, goes.
- **Operators are typed by `T`:**
  - `Eq` and `NotEq` take an `Arg<T>`.
  - `Gt`, `Gte`, `Lt`, `Lte` and `Between` exist only where `T : struct, IComparable<T>` (numbers,
    dates, times, and their nullable forms). No view orders text today.
  - `In` and `NotIn`.
  - `Truthy`, `Falsy`, `IsNull`, `NotNull`, `IsEmpty`, `NotEmpty`.
  - Text sources add `Contains`, `StartsWith`, `EndsWith`, `Matches` and `MinLength`. They lower
    to calls to string methods, not to new runtime operators.
- **Precedence by type.**
  - After `.And(..)` only `And` and `Then` are offered; after `.Or(..)` only `Or` and `Then`.
  - Mixing the two takes a group: `.Or(g => g.When(b).Eq(1).And(c).Gt(2))`.
  - `Not(g => ..)` negates one group.
  - A group has no `Then`.
  - Today's left fold (`ConditionContinuation.cs:43-57`) and the render-time throw inside groups
    (`:134-140`) become impossible to write.
- **Confirm.** `p.Confirm("..")` stays as shorthand for `p.When(confirm.Ask("..")).Truthy()`.

### 4.6 Requests, gather and the URL

```csharp
p.Get("/suites/{suiteType}/rate")
 .Gather<SuiteRateQuery>(g => g.Set(x => x.SuiteType, suite.Value))   // fills {suiteType}; the other members go to the query
 .Response(r => r
     .OnSuccess<SuiteRate>((rate, s) => monthly.Value.Set(rate.Get(x => x.MonthlyRate)))
     .OnError<ProblemDetails>(422, (err, s) => ..)
     .Chained(next => ..));

p.Post(url)
 .Gather<DischargeRequest>(g => g
     .Form()                                                           // every input on the form, by binding name
     .Set(x => x.SelectedRecords, grid.GetSelectedRecords())
     .Set(x => x.FacilityId, p.Url<CensusQuery>().Get(q => q.FacilityId))
     .Header("X-Category", args.Category))
 .Validate<DischargeValidator>("discharge-form")
 .WhileLoading(l => loader.Show())
 .Response(..);
```

- **Gather has three members:** `Set`, `Form` and `Header`.
  - `Set(target, source)` builds the request object.
  - `Form()`, or `Form(m => m.A, m => m.B)`, includes model fields by binding name.
  - `Header(name, source)`.

  They replace 20 overloads: `Include` ×4, `IncludeAll`, `FromEvent`, `RouteParam` ×5, `FromUrl`
  ×4, `Static`, `Header` ×3 and the plugin-only `GatherBuilder.Plugin`.
- **Where each member goes follows MVC's own rule for route values:**
  - A member whose name matches a `{placeholder}` fills the path. Every placeholder must be
    filled, and the existing render-time check (`RequestRouteTemplate`) stays.
  - The remaining members go to the query string for GET, and to the JSON or form-data body
    otherwise.
- **Typed names.** The request class is the shape the action binds, so parameter names are typed
  and no string names a parameter. The only strings left are protocol names: header names, a push
  event type and URL template placeholders.
- **URL reads.**
  - `p.Url<TQuery>()` matches keys case-insensitively, as ASP.NET binding does.
  - Array members read every value of a key.
- **Links.** `Html.NativeActionLink("Discharge", url, link => link.Confirm("..").Post(r => ..))`:
  - The request takes the link's `href`, so the URL is written once.
  - The single-request rule is a type: the link offers exactly one request. Analyzer `ALIS002` and
    the render-time checks go.
  - Modifier-clicks stay with the browser.

### 4.7 Events, dispatch and realtime

```csharp
public static class CareEvents { public static readonly AppEvent<ResidentFlagged> Flagged = new("resident-flagged"); }

p.Dispatch(CareEvents.Flagged, e => e.Set(x => x.ResidentId, row.Get(r => r.Id)));
Html.On(plan, t => t
    .DomReady(p => ..)
    .On(CareEvents.Flagged, (e, p) => ..)
    .ServerPush<CensusUpdate>(pushUrl, "census", (msg, p) => ..)
    .SignalR<ResidentFlagged>(hubUrl, nameof(IResidentClient.Flagged), (msg, p) => ..));
```

- **One declared value for a custom event.** The dispatcher and the listener share it, so they
  cannot disagree on the name or the payload type.
- **SignalR.** The method name comes from the hub's typed client interface, as with ASP.NET Core's
  strongly typed hubs.
- **Component events** keep `.Reactive(c => c.Change, (args, p) => ..)` on the rendered handle.

### 4.8 Elements, plugins and app objects

These are all declarations, used the same way as components.

- **Elements.** `p.Element("status")` is an `HtmlElement` with:
  - `Text` and `Html` (properties);
  - `Show()` and `Hide()`;
  - `ClassList.Add`, `ClassList.Remove` and `ClassList.Toggle`;
  - `ClassList.Items` and `Children`, which are arrays.

  This deletes `FromDom(id, "member")`.
- **Plugins.** A plugin is a declaration found by name:

  ```csharp
  [Plugin("addressLookup")]
  public sealed class AddressLookup : JsObject
  {
      public Value<Address> Find(Arg<string> postcode) => InvokeAsync<Address>(postcode);
  }
  ```

  The name is the plugin's only string. `RegisterPlugin`, `PluginTypeBuilder`,
  `PluginMemberBuilder` and `PluginContract.cs` are deleted. The plan declares a contract when a
  member is used, as it does for components. A root function is `Invoke<T>(.., name: "")`.
- **App objects.**
  - `p.Component<NativeDrawer>()`, `p.Component<FusionToast>()` and `p.Component<Confirm>()`.
  - Confirm's `Ask` is an async member. The app's dialog module defines it on the confirm element,
    so the `window.alis.confirm` global (`confirm.ts:103`) goes.

### 4.9 Validation: FluentValidation's grammar, projected to the browser

```csharp
public sealed class AdmissionValidator : ReactiveValidator<Admission>
{
    public AdmissionValidator()
    {
        RuleFor(x => x.Age).GreaterThan(0).WithMessage("Enter the resident's age.");
        RuleFor(x => x.DischargeDate).GreaterThan(x => x.AdmitDate);
        When(x => x.CareLevel == CareLevel.Memory, () =>
            RuleFor(x => x.Physician).NotEmpty());
        RuleForEach(x => x.Contacts).SetValidator(new ContactValidator());
        RuleFor(x => x.Website).Url();                 // a framework validator that projects itself
    }
}
```

- **One statement per rule.** FluentValidation's own statement serves server and browser. Today
  about 100 properties in 20 sandbox validators state each rule twice.
- **Projection.** The framework reads FluentValidation's public rule descriptors.
  `IValidationRule.Components`, `HasCondition` and `HasAsyncCondition`, and
  `IComparisonValidator.Comparison`, `MemberToCompare` and `ValueToCompare` were confirmed in
  FluentValidation's source; 11.12 is checked in step 0.
- **Which rules reach the browser.** Each built-in validator the browser can run projects to a test
  expression:
  - `NotNull`, `NotEmpty`, `Empty`;
  - `Length`, `MinimumLength`, `MaximumLength`;
  - the six comparisons against a literal or a peer;
  - `InclusiveBetween`, `ExclusiveBetween`;
  - `Matches`, `EmailAddress`, `CreditCard`;
  - `NotEmpty` on a collection, which is "at least one".
- **Custom validators opt in** by implementing `IBrowserRule<T>`; the framework's `Url()` is one.
- **Server-only rules.** `Must`, `MustAsync`, `Custom` and delegate conditions stay server-only
  with no extra code. An analyzer warns when a `ReactiveValidator` uses a rule-level delegate
  `.When`, which the browser cannot see.
- **Conditions.** `When` and `Unless` blocks take an expression. The server compiles it, and the
  browser gets it through the same compiler as array predicates.
- **Peers.** A peer (`MemberToCompare`) resolves against the validated property's parent. The 7
  peer rules in the sandbox are all siblings.
- **Semantics decided in C#:**
  - `NotEmpty` means FluentValidation's empty: null, blank text, an empty collection, or the
    type's default value.
  - A rule other than "required" passes on an empty value.
  - Dates compare as timestamps.
  - Regex patterns are checked at render against JavaScript's dialect, so a .NET-only construct
    fails in C#.
- **Deleted:** `ClientRule` and its builders, the 18 `WhenField*` methods and `WhenFields`, the
  field-condition builders, the operand and activation types, three interfaces,
  `AddReactiveClientValidation`, and the whole runtime rule engine.

## 5. Grammar rules: one parameter order everywhere

Six rules, for developers and for agents:

1. **Target before source, callback last.** `Set(target, source)`, `Header(name, source)`,
   `OnError<T>(status, callback)`, `On(event, callback)`, `ServerPush<T>(url, type, callback)`,
   `.Reactive(c => c.Event, callback)`.
2. **A callback's parameters are the payload or result, then the scope:** `(args, p)`,
   `(res, s)`, `(msg, p)`, `(e, p)`, or just `(p)`.
3. **One source is one argument,** an `IValue<T>`. There are no `(source, x => x.Path)` pairs.
4. **A literal and a runtime value take the same position** (`Arg<T>`).
5. **Names come from types** wherever a C# type exists: the request class, the query class, the
   event value, the hub interface, members.
6. **One verb, one meaning.**
   - An object's member is used directly.
   - `Set` writes.
   - `Get` reads a data path.

| Today | Proposed |
|---|---|
| `p.When(args, x => x.Value).Gt(100m)` | `p.When(args.Value).Gt(100m)` |
| `p.Component<FusionNumericTextBox>(m => m.Rate).Set(c => c.Min, 0m)` | `rate.Min.Set(0m)` |
| `grid.Call(c => c.SelectRow, 2)` | `grid.SelectRow(2)` |
| `p.Payload(args).Set(a => a.PreventDefaultAction, true)` | `args.PreventDefaultAction.Set(true)` |
| `s.Payload(args).Call(a => a.UpdateData, res.Read(x => x.Items))` | `args.UpdateData(res.Get(x => x.Items))` |
| `g.Include(grid.Get(c => c.GetSelectedRecords<R>()), "selectedRecords")` | `g.Set(x => x.SelectedRecords, grid.GetSelectedRecords())` |
| `g.RouteParam("cardId", args, x => x.Data.Id)` | `g.Set(x => x.CardId, args.Data.Get(d => d.Id))` |
| `g.FromEvent(args, x => x.Text, "q")` | `g.Set(x => x.Q, args.Text)` |
| `g.FromUrl("facilityId")` | `g.Set(x => x.FacilityId, url.Get(q => q.FacilityId))` |
| `.Header("X-Category", args, x => x.Category)` | `g.Header("X-Category", args.Category)` |
| `p.When(p.FromUrl<int>("page")).Gt(1)` | `p.When(url.Get(q => q.Page)).Gt(1)` |
| `p.FromDom("dom-card", "classList").Count()` | `p.Element("dom-card").ClassList.Items.Count()` |
| `p.Plugin(tools.Count).Arg(items).Arg("status")` | `tools.Count(items, "status")` |
| `NativeActionLink(text, url, p => p.Post(url)..)` | `NativeActionLink(text, url, link => link.Post(r => ..))` |
| `DispatchWith<T>("flagged", d => ..)` + `CustomEvent<T>("flagged", ..)` | `Dispatch(CareEvents.Flagged, e => ..)` + `On(CareEvents.Flagged, ..)` |
| `ClientRule(x => x.Age).GreaterThan(0m, "msg")` + `RuleFor(x => x.Age).GreaterThan(0m)` | `RuleFor(x => x.Age).GreaterThan(0m).WithMessage("msg")` |

## 6. One expression compiler

The compiler turns C# lambdas into plan expressions. It serves:
- data paths;
- LINQ lambdas;
- validator `When` expressions;
- request-class member selection.

How it works:

- **Two kinds of subtree.**
  - Parameter-rooted subtrees become runtime reads.
  - Closure-rooted subtrees are evaluated at render into literals, as EF Core does with its
    parameter extraction. `row.ResidentId` inside a Razor loop is a literal.
- **Operators.** C#'s operators map one-to-one: comparison, `&& || !`, arithmetic, `??`, `?:`.
  Conversions are explicit nodes (`toNumber`, `toTime`), chosen from the C# types.
- **Members of `string` and `IEnumerable<T>`** map through one table to JavaScript built-ins:
  - `Where`→`filter`, `Select`→`map`, `Any`→`some`, `All`→`every`, `First`→`find`;
  - `OrderBy`→`toSorted` with a comparator lambda the compiler builds;
  - `Sum`→`reduce`, `Distinct`→`filter` by first index, `Skip`/`Take`→`slice`;
  - `StartsWith`→`startsWith`, and so on.

  A new member is one table entry and no runtime change.
- **Anything else** fails at render, naming the construct. An analyzer reports it at compile time.

## 7. Component onboarding, inverted

A component author writes three things. The core never names the component.

1. **A vendor, once per library:** `ComponentVendor.Define(name, objectPath, subscribe, unsubscribe, payload)`.
2. **Declarations** that mirror the JavaScript API. They are generic over the app's types where the
   API is (`FusionGrid<TRow>`, `FusionDropDownList<TValue, TItem>`). Inputs derive
   `Input<TValue>`. Payloads are declarations.
3. **A render helper.**
   - `field.Identity` writes the framework's `id` and `name` into hand-written markup, and hands
     vendor builders the same pair: `.HtmlAttributes(field.Identity.Attributes)`.
   - `field.Render<T>(markup)` and `plan.Render<T>(id, markup)` check that the markup carries the
     expected `id`, and the expected `name` for an input. If it does not, they throw at render and
     name the expected value.

The core and the runtime carry no knowledge of any component. These leftovers go:

| Today | After |
|---|---|
| `live-clear.ts:102` hard-codes `"change"` | the declared `Committed` event |
| `inject.ts:36` calls `ej.base.append` to run Syncfusion's partial scripts | vendor-neutral script execution: inserted `<script>` nodes are re-created in document order |
| `request-payload-writer.ts:163` unwraps Syncfusion's `rawFile` | the uploader declares its value as its files |
| `confirm.ts:103` installs `window.alis.confirm` | the declared `Confirm.Ask` member |
| Native CheckList and RadioGroup render inline `<script>` (`NativeCheckListBuilder.cs:195`, `NativeRadioGroupBuilder.cs:190`) | the native runtime modules define those members on the element, as the drawer and loader do |

**Proof.** Tom Select, and a second brought component from a different library, ship with zero edits
to the core or the runtime. The architecture test fails if the core or runtime names a vendor or a
component.

## 8. The runtime: a lean, dumb executor

Its whole vocabulary:

| Concept | Kinds |
|---|---|
| Object | located by `element+vendor`, `fixed id`, `plugin name`, `payload`, `document`, `location`, or `the result of an expression`; members are names |
| Expression | `literal`, `read(source, path)`, `call(target, member, args, await)`, `lambda(params, body)`, `object`, `array`, `op`, `convert` |
| Op | C#'s operator set (comparison, logical, arithmetic, `??`, `?:`) plus `empty`. It is closed, because C#'s operators are |
| Reaction | `sequence`, `set`, `call`, `branch`, `request`, `parallel`, `dispatch`, `inject`, `show-errors` |
| Trigger | `ready`, `event(object, name)`, `push(url, type)`, `hub(url, method)` |
| Form | field tests `{field, value, test, message, when}`; error display; live wiring (input clears; blur and the declared committed event revalidate); server errors by name |
| Slot | load, unload |

- **Deleted from the runtime:**
  - the comparison engine's 21 cases;
  - the array-operation engine;
  - the validation rule engine;
  - the duplicate all/any/not evaluator;
  - four null helpers;
  - all vendor knowledge.
- **The async lane is decided in C#.** A `call` is awaited only when its member was declared async,
  and the plan says so. HTTP, Confirm, async plugins, push and injection are async; everything else
  stays sync.

## 9. The plan document

```json
{
  "planId": "…", "scope": {},
  "vendors":      { "fusion": {} },
  "declarations": { "FusionGrid": { "vendor": "fusion", "members": {} } },
  "objects": {
    "Ns_Admission__Age": { "kind": "input", "declaration": "FusionNumericTextBox", "name": "Age", "value": "value", "committed": "change" },
    "roster":            { "kind": "component", "declaration": "FusionGrid" },
    "addressLookup":     { "kind": "plugin", "declaration": "AddressLookup" },
    "admission-form":    { "kind": "form", "tests": [] }
  },
  "behaviors": [ { "trigger": {}, "reaction": {} } ]
}
```

- **One entry per declaration class.** Today the types repeat per element:
  `fusion.component.<id>` (`BrowserObjectId.cs:13`).
- **No placeholders.** Each object kind carries only its own fields. `binding:{kind:"none"}` and
  `container:{kind:"none"}` (`plan.ts:86-120,148`) go.
- **One expression union** serves values, conditions, lambdas and validation tests.

## 10. Proof that nothing is lost

The use cases were inventoried from the DSL source and the views, not from the tests. Each was then
mapped to the Playwright test that fails if it breaks. The lists are in
[`use-cases/`](use-cases/).

| Area | Use cases | Proven | Weak | Plan-only | No test |
|---|---|---|---|---|---|
| Components, payloads, rendering, app objects | 128 | 93 | 16 | 1 | 18 |
| HTTP, URL, realtime, partials, links | 121 | 87 | 12 | 3 | 19 |
| Conditions, arrays, dispatch, elements, plugins | 165 | 117 | 16 | 8 | 24 |
| Validation | 117 | 72 | 7 | 9 | 29 |
| **Total** | **531** | **369** | **51** | **21** | **90** |

1. **Step 0, before any redesign.**
   - Every weak, plan-only and missing row gets a Playwright test that fails when the behavior
     breaks: 162 rows in all.
   - These tests are written against the current code, and CI must be green.
   - A declared member that no view uses gets a journey that uses it, or is removed from the
     declaration with the reason recorded.
2. **Assertions frozen.** From step 1 on, the test fixtures (`tests/**/When*.cs`) are not edited.
   - Views and the framework change; test infrastructure (`PagePlan`) may adapt to the plan
     document.
   - Each step reports `git diff --stat <step-0 commit> -- 'tests/**/When*.cs'`, and it must be
     empty.
   - A green suite with unchanged assertions shows every use case still behaves as before.
3. **Every step is green in CI:** `gate / test`, the five Playwright shards and `verify-net48`.

## 11. Rejected

- **A, descriptors plus verbs.** No returned objects. Ceremony on every use. Reads and writes
  differ by source kind. Refs bound to their creating pipeline.
- **B, interfaces plus expression trees.** A runtime value cannot pass through a `T` parameter, and
  events cannot appear in expression trees.
- **Recording proxies over interfaces** (`DispatchProxy`). These need `Reflection.Emit` on net48 and
  have the same `T`-parameter problem.
- **Lambdas instead of the fluent conditions** (`p.When(m => m.Age > 65)`). They would replace the
  owner's grammar and give branch conditions a second way to be written; the fluent grammar keeps
  its shape and gets types.
- **Operators on values** (`rate > 100m`). These need `==` overloaded on a class, cannot restrict
  ordering by `T`, and arrays would still need lambdas.
- **Freezing arrays and pushing the rest to plugins.** It contradicts array-driven components.
- **A source generator.** The owner rejected it.

## 12. Order of work

0. **Step 0 proof gaps.** The 162 rows, closed on the current code.
1. **Value model and scopes.**
   - `IValue<T>`, `Arg<T>`, data `Get`.
   - The active-scope stack.
   - `p.Field`, `p.Url<T>`.
2. **Object model C.**
   - `JsObject` members.
   - The 66 declarations and 128 payloads.
   - Identity.
   - Views move to the handles.
   - `ComponentRef`'s verbs are deleted.
3. **Compiler and lean expressions.**
   - Arrays and text.
   - Typed conditions.
   - Runtime: `lambda`, `call` on values, `op`, `convert`.
   - The comparison and array engines are deleted.
4. **Requests and events.**
   - `Gather<T>`.
   - The link inherits its `href`.
   - Typed `AppEvent`, push and hub triggers.
5. **Validation on FluentValidation descriptors.** The rule engine and the `ClientRule` grammar are
   deleted.
6. **Onboarding leftovers.** The committed event, vendor-neutral scripts, uploader files, Confirm,
   and the native modules.
7. **Plan document names.**
