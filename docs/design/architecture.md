# Architecture: one grammar, one object model, a lean runtime

Proposal (v6) on `feature/component-redesign`. Section 0 is the spine; every later section is
vocabulary on it. The exact specification (syntax, value semantics, operators, static checks,
lowering, and what each built-in object must do) is [`grammar.md`](grammar.md), checked against all
531 use cases; where the two differ, `grammar.md` governs. What is already built is described in
[`bring-your-own-component.md`](bring-your-own-component.md).

The constraints:
- everything is fully typed;
- every feature is preserved;
- the runtime stays a dumb executor of what the C# domain decides;
- greenfield otherwise.

## 0. The grammar

What does a behavior in this framework say? Exactly what a developer would otherwise write by hand
in a JavaScript event handler:

```js
link.addEventListener("click", async e => {
  if (e.defaultPrevented || e.button !== 0 || e.metaKey || e.ctrlKey || e.shiftKey || e.altKey) return;
  e.preventDefault();
  loader.show();
  try {
    const res = await http.send({ method: "POST", url: link.href, headers: { "X-Shift": "Night" } });
    if (res.ok) status.textContent = res.body.message;
    else if (res.status === 409) status.textContent = res.body.message;
  } finally {
    loader.hide();
  }
});
```

That handler uses objects and their members, expressions that produce values, and statements that
cause effects. JavaScript needs nothing more, and neither does the framework. **The plan's grammar
is JavaScript's own core, typed by C#.**

### 0.1 Grammar and vocabulary

- **Grammar** is the closed set of productions in 0.2. The runtime interprets them. It is as small
  as JavaScript's core and it never grows.
- **Vocabulary** is the open set of objects: components, elements, the document, plugins, streams,
  hubs, and the framework's own features (HTTP, forms, slots, Confirm, Toast, Drawer, Loader). Each
  object is declared through the one object contract (section 4.0), in the package that owns it.

A new component or a new feature adds vocabulary, never grammar. This is the inversion applied to
the framework itself: when HTTP, validation, partials and realtime are ordinary declarations, any
component can do what they do.

Today each feature brings a grammar of its own, and the generated contract has 136 interfaces, 60
unions and 65 kinds:
- gather has six verbs (`Include`, `IncludeAll`, `FromEvent`, `RouteParam`, `FromUrl`, `Static`);
- requests have their own routing (`RequestPlan`, `ResponseRoute`, `RequestChain`);
- conditions, validation rules and validation conditions are three boolean grammars with two
  evaluators;
- five trigger kinds each have their own shape;
- dispatch, inject and show-errors are reaction kinds;
- eight read kinds and five source kinds say "a value from somewhere".

### 0.2 The productions

```
plan     := object* handler*
object   := id -> locator
locator  := element(id, vendor)          found on an element, through its vendor's data
          | plugin(name, key, data)      made by the module a bundle registered under name:
                                         the framework's alis.document, alis.http, alis.model,
                                         alis.form, alis.slot; the realtime package's alis.stream,
                                         alis.hub; an app's own plugins
handler  := on(object, event, param) block

block    := stmt*
stmt     := set(expr, path, expr)        o.a.b = v
          | do(expr)           [await]   o.m(a);            await hub.start();
          | let(name, expr)    [await]   const n = o.m(a);  const r = await http.send(q);
          | if(arm+, block)              if (..) {..} else if (..) {..} else {..}
          | all(block+)                  await Promise.allSettled([..])
          | try(block, block)            try {..} finally {..}
arm      := (expr, block)

expr     := lit(json, shape)             42, "east", [1, 2]
          | obj(id)                      a located object
          | ref(name)                    a handler parameter, a let, a lambda parameter
          | get(expr, path, shape)       o.a.b[0]
          | call(expr, path, expr*, shape)  o.m(a), rows.filter(r => r.active)
          | fn(name+, expr)              r => r.active
          | record(entry*)               { a: v, ...o }
          | list(expr*)                  [a, b]
          | op(operator, expr+)          a > b, a && b, a ?? b, c ? a : b, toNumber(a), empty(a)
entry    := name: expr | ...expr
```

One handler form, six statements, nine expressions, two locators. The operator set is C#'s own
(comparison, logical, arithmetic, `??`, `?:`) plus conversions, `empty` and `matches` (a regular
expression test, since the grammar constructs no objects). It is closed because C#'s is.

### 0.3 Four laws

1. **Only statements await, and plan order is C# evaluation order.** Expressions are synchronous.
   An async member (an HTTP send, Confirm, a hub `invoke`, an async plugin method) is awaited where
   it is called: the C# call emits `let … await` (or `do … await`) into the active scope and returns
   a value naming the result, so the value is read, never invoked twice. Builders that take
   callbacks (`p.Confirm`, groups, `ElseIf`) build in short-circuit order, so a Confirm inside a
   group is asked only when reached, the way the C# compiler lowers `await` inside `&&`.
   - The runtime has one synchronous evaluator.
   - Every statement before the first `await` runs synchronously, so a payload's `cancel` lands
     before the vendor reads it.
   - The async lane is decided in C#. A handler without `await` never becomes asynchronous.
2. **Bindings are lexical.** A handler's payload, a `let` and a lambda parameter are names in
   scope.
   - A payload method called after an `await` still reaches its payload.
   - A response is readable where it is bound, and a chained request reads it there.
   - The request snapshot is the `let` that holds the gathered record.
   - A `try` body's top-level bindings stay in scope after the `try`. Code after a `try` runs only
     when its body completed, so each of them was bound.
3. **A node carries what it needs.** `get`, `set` and `call` carry the JavaScript path and the shape
   that the C# declaration knows where it is used. The runtime looks nothing up by member name, so
   there is no declarations section, no member key and no contract merging.
4. **Lifetime belongs to the owner.** The page or a slot owns each handler, input, test and
   connection subscription it brought. Unloading a slot releases exactly those; the last
   subscription closes a connection. A block already running finishes, as it does today.

### 0.4 Every feature is a sentence

| Feature | Vocabulary | Grammar |
|---|---|---|
| Write a property, call a method | the object's members | `set`, `do` |
| A returning method as a value | the object's members | `call` |
| A component event and its payload | the component: `element(id, vendor)` | `on` |
| Page ready | the document's `ready` event | `on` |
| Custom events | `document.raise(name, detail)` and the event itself | `do`; `on` |
| Server-sent events | `plugin("alis.stream", url)` and its message events | `on` |
| SignalR | `plugin("alis.hub", url)`: client methods as events, `invoke` with a result, `send` | `on`; `let await`; `do await` |
| Payload write (`cancel`); payload method after an async gap | the payload object | `set` or `do` on the handler's parameter |
| When, ElseIf, Else, And, Or, Not, groups | none | `if`, `op` |
| Confirm | `confirm.ask(message)`: async, returns a bool | `let await`, then `if` |
| Arrays: Where, Select, OrderBy, Sum, Any … | JavaScript's array methods | `call` with `fn` |
| Text: Contains, StartsWith, Length … | JavaScript's string members | `call`, `get` |
| URL query | `document.query`: keys match case-insensitively; a repeated key reads as an array | `get` |
| Element text, HTML, classes, visibility | the element's own DOM members | `set`, `do`, `get` |
| Plugins | the plugin's members; a root function is a call with an empty path | `do`, `call`, `get` |
| Get, Post, Put, Delete | `http.send(request)`: async; resolves to `{ status, ok, body }`; status 0 when no response came | `let await` |
| Gather | the request record, split into route and values in C# at render; `Form()` spreads `model.values` | `record` |
| Response routes by status | the response's `ok` and `status` | `if` |
| Request snapshot | the gathered record | `let`, `ref` |
| WhileLoading, Finally | for example `loader.show()`, `loader.hide()` | statements before a `try`; its `finally` |
| Validate before sending | `form.validate()` returns a bool | `if` |
| Chained | none | `if (res.ok)` after the `try` |
| Parallel, with completion | none | `all`, then statements |
| Into, partial injection | `plugin("alis.slot", id)`: `load(html)` (synchronous), `unload()` | `do` |
| Server validation errors | `form.showErrors(problem)` | `do` |
| Validation rules | the form's tests `{ field, check, when, message }` | `fn`, `op`, `get` |
| Live clearing, revalidation | inside the form: each input's DOM `input` event clears; DOM `blur` and the declared `Committed` event revalidate | none |
| Action link | the anchor: `click`, `href`; the payload's `preventDefault` and modifier keys | `on`, `if`, `do`, `get` |
| Toast, Drawer, Loader | their members | `set`, `do` |

### 0.5 One sentence, lowered

The owner's fluent API keeps its shape. Each fluent method lowers to fixed productions, so the plan
says exactly what runs. The `ShiftHandover` link (`V/HttpPipeline/ShiftHandover/Index.cshtml:30-39`),
written with object handles:

```csharp
Html.NativeActionLink($"mark-read-{note.Key}", "Mark Read", markReadUrl, link => link
    .Post()
    .Gather(g => g.Header("X-Shift", "Night"))
    .WhileLoading(l => loader.Show())
    .Response(r => r
        .OnSuccess<HandoverNoteRead>((json, s) => status.Text.Set(json.Get(x => x.Message)))
        .OnError<HandoverNoteRead>(400, (json, e) => status.Text.Set(json.Get(x => x.Message)))
        .OnError<HandoverNoteRead>(409, (json, e) => status.Text.Set(json.Get(x => x.Message))))
    .Finally(f => loader.Hide()));
```

The plan it renders, written as JavaScript for reading (the JSON is the same tree). `link`,
`loader`, `status` and `http` stand for `obj(id)` nodes:

```js
on(link, "click", e) {
  if (!(e.defaultPrevented || e.button !== 0 || e.metaKey || e.ctrlKey || e.shiftKey || e.altKey)) {
    e.preventDefault();
    loader.show();                                          // WhileLoading
    try {
      const req = {};                                       // the gathered record: the request snapshot
      const res = await http.send({ method: "POST", url: link.href, values: req,
                                    headers: { "X-Shift": "Night" }, format: "json" });
      if (res.ok) status.textContent = res.body.message;    // OnSuccess
      else if (res.status === 400) status.textContent = res.body.message;
      else if (res.status === 409) status.textContent = res.body.message;
    } finally {
      loader.hide();                                        // Finally
    }
  }
}
```

The inventoried rows (`use-cases/`) fall out of the shape instead of special code:

| Rows | Behavior | Why the sentence does it |
|---|---|---|
| http 41 | A route parameter with no value: nothing is sent, Finally runs, the error is reported | `http.send` throws before sending; `finally` runs; the error propagates |
| http 63 | An unmatched status runs no route, and Finally still runs | no arm matches |
| http 64, 80 | A network failure reaches no status route, and Finally runs | status 0 matches no status; an untyped `OnError` would be the final `else` |
| http 68, 69, 70, 81 | Chained runs only after success, after the parent's Finally, and reads the response | `if (res.ok) { … }` follows the `try` and reads `res` |
| conditions 76, 77, 78 | Confirm OK runs Then; Cancel runs Else; Cancel stops a gated request | `const ok = await confirm.ask(..)`, then `if (ok)` |
| conditions 69 | Confirm inside a grouped condition | the builder nests the `if`, repeating the shared branch, so the question is asked only when reached |

A validation rule lowers the same way. `RuleFor(x => x.DischargeDate).GreaterThan(x => x.AdmitDate)`
inside `When(x => x.Status == Status.Discharged, ..)` becomes one test of the form:

```js
{ field: "Ns_Admission__DischargeDate",
  check: (value, model) => toTime(value) > toTime(model.admitDate),
  when:  model => model.status === "Discharged",
  message: "…" }
```

The check and the condition are expressions, so the browser evaluates them with the same evaluator
as conditions and arrays. There is no rule engine.

### 0.6 What the grammar removes

| Today | Becomes |
|---|---|
| Five trigger kinds | `on` |
| Nine reaction kinds | six statements; `request`, `dispatch`, `inject` and `show-validation-errors` are calls on vocabulary |
| `RequestPlan`, `ResponseRoute`, `ResponseStatusMatch`, `RequestChain`, `RequestInput`, three gather targets, `RegisteredInputSelection`, `ParallelCompletion` | `let`, `if`, `try`, `record`, `all` |
| `ConditionGraph`, `ValidationCondition`, `ValidationRule` with its executions and activations | `op` and `fn` |
| Eight read kinds, five source kinds, four literal kinds | `obj`, `ref`, `get`, `call`, `lit` |
| Per-element `types`, member contracts, contract merging | nothing: nodes carry their path and shape |
| The runtime's comparison engine, array engine, rule engine, rule operands and condition evaluator | one evaluator |

**C#.** The plan domain is the grammar: one sealed, immutable class per production, with its
invariants in its constructor, serialized one to one. Builders and vocabulary declarations are its
only producers, so a malformed sentence cannot be built.

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

### 4.0 The object contract: one shape for every JavaScript object

A JavaScript object has properties, methods and events.
- Some properties are readable, some writable, some both.
- Some methods take arguments and some don't; some return a value and some don't; some are
  asynchronous.
- Some events carry a payload and some don't, and a payload can itself be a JavaScript object with
  members.

That is the whole objective. Each combination has one C# form, so anything JavaScript does not allow
fails to compile:

| JavaScript | Declared as | Used in a view | Plan node |
|---|---|---|---|
| read-write property | `public Prop<decimal> Min => Prop<decimal>();` | `rate.Min` is a value; `rate.Min.Set(0m)` | `get`, `set` |
| read-only property | `public ReadOnly<int> ReadyState => ReadOnly<int>();` | a value; `.Set` does not exist | `get` |
| method: no arguments, no return | `public void Refresh() => Call();` | `grid.Refresh();` | `do(call)` |
| method: arguments, no return | `public void SelectRow(Arg<int> index) => Call(index);` | `grid.SelectRow(2);` | `do(call)` |
| method: returns a value | `public Value<TRow[]> GetSelectedRecords() => Invoke<TRow[]>();` | a value: in conditions, sets, gather, arrays | `call` |
| method: arguments and a return | `public Value<int> GetActualIndex(Arg<int> a, Arg<int> b) => Invoke<int>(a, b);` | a value | `call` |
| method: returns an object | `public FusionGridColumn GetColumnByField(..) => InvokeObject<FusionGridColumn>(..);` | `grid.GetColumnByField(c => c.Status).Visible.Set(false)` | `call` as a `set`'s target |
| async method with a value | `public Value<int> GetCensus(Arg<string> wing) => InvokeAsync<int>(wing);` | a value, awaited where it is written | `let await` |
| async method without a value | `public void Start() => CallAsync();` | `hub.Start();`, awaited where it is written | `do await` |
| event, no payload | `public Event Ready => Event();` | `(p) => ..` | `on` |
| event, data payload | `public Message<CensusUpdate> Census => Message<CensusUpdate>();` | `(msg, p) => msg.Get(x => x.Occupied)` | `on`; the payload is data |
| event, object payload | `public Event<FusionGridActionBegin<TRow>> ActionBegin => Event<FusionGridActionBegin<TRow>>();` | `(args, p) => { args.Cancel.Set(true); args.Data.Get(r => r.Id); }` | `on`; the payload is an object |

Payload kinds:
- **An object payload** (`Event<T>`, where `T : JsObject`) has members of its own. Syncfusion's
  event args are objects: `cancel` and `preventDefaultAction` are writable, and `updateData` and
  `endEdit` are methods.
- **A data payload** (`Message<T>`) is JSON-shaped data typed by a plain class. It can be the
  server's own DTO, so the hub and the page share one type. Stream messages, hub methods and custom
  event `detail` are data.

### 4.0.1 Objects differ only in where they live

What differs between objects is the **locator**: how the object comes to exist and how long it
lives. The grammar has two (section 0.2): an element found through its vendor's data, and a plugin
made by a module that a bundle registered by name. The framework's own built-ins are plugins, so
they use the same extension point an app does.

| Object | Locator | Lifetime | Events subscribe with |
|---|---|---|---|
| Component (Fusion, Native, brought) | `element(id, vendor)`: the vendor's `objectPath` on the element | the element's: page or slot | the vendor's subscribe and unsubscribe methods |
| App object (Drawer, Loader, Toast, Confirm) | `element(id, vendor)` with a fixed ID; the package's own runtime module defines its members on the element | page | the vendor's |
| Partial slot | `plugin("alis.slot", containerId)`: `load(html)`, `unload()`; the container is looked up at each call | its own owner | none |
| Model | `plugin("alis.model", planId)`: `values` (nested by binding name), `fields` (flat MVC names): every mounted input of the composed plan | the plan's | none |
| DOM element | `element(id, dom)` | the element's | `addEventListener` |
| App plugin | `plugin(name)`, registered by the app's bundle | page | none |
| Document | `plugin("alis.document")`: `ready`, `raise`, `query` | page | `ready`: the start of the handler's own owner; custom events: `addEventListener` (payload `detail`) |
| HTTP | `plugin("alis.http")`: `send` | page | none |
| Form | `plugin("alis.form", containerId)`: `validate`, `showErrors`; its tests apply to fields inside the container | its owners' | none |
| **Server-sent event stream** | `plugin("alis.stream", url)` | shared per URL, counted per owner (page or slot), closed by the last owner | `addEventListener`; payload `JSON.parse(event.data)` |
| **SignalR hub connection** | `plugin("alis.hub", url)` | shared per URL, counted per owner, automatic reconnect | `on` / `off`; payload is the single argument |
| Event payload | not located: the handler's parameter | the handler, across every `await` in it | none |
| Response, request snapshot | not located: a `let` | its block | none |
| Returned object, nested object | not located: an object-valued expression | the expression | none |

Streams and hub connections are therefore vocabulary, not trigger kinds of their own.
- The connection policy the runtime has today stays, and it belongs to their modules:
  - one connection per URL;
  - connections shared across slots;
  - start retries;
  - the retry indicator: one marker per dropped connection, and one click retries all of them.
- Their members become typed for free: `readyState`, `close()`, `state`, `connectionId`,
  `invoke()` returning a value, `send()`, and the `reconnecting`, `reconnected` and `close` events.

### 4.1 Objects

```csharp
public abstract class JsObject                          // a live JavaScript object
{
    // properties
    protected Prop<T> Prop<T>([CallerMemberName] string name = "");                 // read and write
    protected ReadOnly<T> ReadOnly<T>([CallerMemberName] string name = "");         // read
    protected TObject Object<TObject>([CallerMemberName] string name = "") where TObject : JsObject, new();

    // methods: 0 to 5 arguments each (IArg a1 ... a5), then the name
    protected void Call(/* args */ [CallerMemberName] string name = "");
    protected void CallAsync(/* args */ [CallerMemberName] string name = "");
    protected Value<TResult> Invoke<TResult>(/* args */ [CallerMemberName] string name = "");
    protected Value<TResult> InvokeAsync<TResult>(/* args */ [CallerMemberName] string name = "");
    protected TObject InvokeObject<TObject>(/* args */ [CallerMemberName] string name = "") where TObject : JsObject, new();

    // events
    protected Event Event([CallerMemberName] string name = "");                                          // no payload
    protected Event<TPayload> Event<TPayload>([CallerMemberName] string name = "") where TPayload : JsObject, new(); // object payload
    protected Message<TData> Message<TData>([CallerMemberName] string name = "");                       // data payload
}
```

- **Names.** A C# name camel-cases to the JavaScript name. Pass `name:` only when the JavaScript
  name differs, for example `"input.value"` or `"showColumns"`.
- **Arguments.**
  - The base takes `IArg`, and each public member types its parameters as `Arg<T>`.
  - C# cannot define a conversion to an interface, so a fixed literal inside a declaration is
    written `Fixed("field")`.
  - `Column(field)` turns a row expression into the column's camel-case name.
- **Paths, not keys.** Every node that uses a member carries its JavaScript path and shape
  (section 0.3), so overloads and generic instantiations need no plan key.

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

As built: `IInputComponent.Input` returns `ComponentInput.Of(Value, committedBy: SomeEvent)`; the
model carries each input's commit event and re-raises it as `alis:commit`.

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
  | a literal | implicit |
- **Arrays and text.** `IValue<IEnumerable<T>>` and `IValue<string>` carry the members of the one
  member table (`grammar.md` section 2.3): today's `Where`, `Select`, `OrderBy`,
  `OrderByDescending`, `Count`, `Any`, `All`, `Sum`, `Find`, and the text members. Each lowers to
  JavaScript's own method with a compiled lambda. `First`, `Distinct`, `Skip`, `Take`,
  `SelectMany`, `Min`, `Max`, `Average`, `ThenBy` and `Concat` are added as table rows, each with a
  journey that proves it.

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

### 4.5 Conditions: the owner's fluent API with the defects removed

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
  - `Form()`, or `Form(m => m.A, m => m.B)`, includes every mounted input of the composed plan
    by binding name, as `IncludeAll` does today: `model.values` for JSON, `model.fields` for form
    data.
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
- **As built (typed gather).**
  - `Gather<TRequest>(g => g.Set(x => x.M, v).Form(..).Header(name, v))`; `.Gather(g => ..)` takes the
    view model as the request. `Set` takes any value source or a literal. A source's type must be the
    member's type or derive from it, so an `int?` value cannot fill an `int` member; a `T[]` fills `T[]`,
    `List<T>` or `IEnumerable<T>`; a nullable member is left empty with a typed null.
  - An event payload member is read with `p.Payload(args).Get(a => a.M)`, since the handle model of
    section 3 is not built (section 13).
  - Rendering checks that every placeholder is filled, that a member is set once, and that a
    `Form(m => m.X)` field is a member of `TRequest` of a type its value binds to.
  - In ASP.NET Core an action that binds a JSON body takes its route values as parameters beside the
    `[FromBody]` class (`Hold(string slotId, [FromBody] HoldRequest hold)`). The page's request class
    is then the body class plus the route members (`HoldVisitRequest : HoldRequest { SlotId }`).
  - Keys are binding names: `{"Address":{"City":..}}` in JSON, `Address.City` in a query or form data.
  - The deleted surface: every `Include` form, `IncludeAll`, `FromEvent`, `RouteParam`, `FromUrl`
    (gather and `p.FromUrl`), `Static`, `GatherBuilder.Plugin`, `p.Post(url, g => ..)` and
    `p.Put(url, g => ..)`, and the namespace `Alis.Reactive.Native.Extensions`.
- **How a request runs.** It lowers to `let`, `if` and `try` around `http.send` (section 0.5). The
  builder owns that shape. The runtime's `http` module only fills the URL and the query or body by
  MVC's route-value rule, sends, and reads the body.
- **Links.** `Html.NativeActionLink(id, "Discharge", url, link => link.Confirm("..").Post()..)`:
  - The link is an element object with an ID the developer chooses; links in a loop use the row's
    key. Its behavior is an ordinary `on(link, "click")` handler (section 0.5), so the delegated
    document listener, the plan carried in `data-reactive-link`, and the runtime's rewriting of
    the request URL (`native-action-link.ts:65`) go.
  - The request takes the link's `href`, so the URL is written once.
  - The single-request rule is a type: the link offers exactly one request. Analyzer `ALIS002` and
    the render-time checks go.
  - Modifier-clicks stay with the browser: the handler's first `if` reads the payload's modifier
    keys.

### 4.7 Events on any object: components, the document, streams and hubs

Every trigger has one form: **an object, its event, a callback.** A rendered component uses
`.Reactive(c => c.Change, (args, p) => ..)`, where the object is the one just rendered. Every other
object uses `t.On(object, o => o.Event, (payload, p) => ..)`.

```csharp
// A server-sent event stream: an EventSource at a URL.
public sealed class CensusStream : EventStream
{
    public Message<CensusUpdate> Census => Message<CensusUpdate>();          // the named SSE event "census"
    public Message<CensusUpdate> Unnamed => Message<CensusUpdate>("message"); // messages sent without an event name
}

// A SignalR hub: a HubConnection at a URL. Server-to-client methods are its events.
public sealed class ResidentHub : HubConnection
{
    public Message<ResidentStatus> StatusChanged => Message<ResidentStatus>(nameof(IResidentClient.StatusChanged));
    public Event Reconnected => Event();
    public ReadOnly<string> ConnectionId => ReadOnly<string>();
    public Value<int> GetCensus(Arg<string> wing) => InvokeAsync<int>(wing);   // client to server; returns a value
    public void Acknowledge(Arg<int> alertId) => Send(alertId);                // client to server; returns nothing
}

public static class CareEvents { public static readonly AppEvent<ResidentFlagged> Flagged = new("resident-flagged"); }

var census = p.Stream<CensusStream>(Url.Action("Census", "Live"));
var residents = p.Hub<ResidentHub>("/hubs/resident");

Html.On(plan, t => t
    .DomReady(p => ..)                                             // document.ready
    .On(CareEvents.Flagged, (e, p) => ..)                          // a document custom event
    .On(census, s => s.Census, (msg, p) => occupancy.Value.Set(msg.Get(x => x.Occupied)))
    .On(residents, h => h.StatusChanged, (msg, p) =>
        p.When(msg.Get(x => x.Status)).Eq("Critical").Then(t => residents.Acknowledge(msg.Get(x => x.AlertId))))
    .On(residents, h => h.Reconnected, (e, p) => ..));
p.Dispatch(CareEvents.Flagged, e => e.Set(x => x.ResidentId, row.Get(r => r.Id)));
```

- **One trigger kind.** Triggers are events on objects. `ServerPush` and `SignalR` stop being
  separate trigger kinds, and their URL, event-type and method-name strings move into
  declarations.
  - The SSE event name is the member name.
  - The hub method name comes from the hub's typed client interface, as with ASP.NET Core's
    strongly typed hubs.
- **Payloads.** A message payload is data typed by the declaration. The runtime's existing rules
  stay: SSE data is JSON-parsed, and a hub method takes a single object argument.
- **Ownership.** An `On` inside an injected partial is owned by its slot. Unloading the slot
  releases the subscription, and the connection closes when its last owner is gone. This is
  today's reference counting, now a property of the locator.
- **Custom events.** A custom event is one declared value that the dispatcher and the listener
  share, so they cannot disagree on its name or its payload type.
- **What realtime gains.** Calls from client to server (`invoke`, `send`), the connection's own
  events (`Reconnecting`, `Reconnected`, `Closed`) and its properties (`State`, `ReadyState`) come
  from the same model, with no realtime-specific grammar. No view uses them yet, so each one ships
  with a journey that proves it.

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
  `PluginMemberBuilder` and `PluginContract.cs` are deleted. Each use carries its path and shape,
  as it does for components. A root function is `Invoke<T>(.., name: "")`.
- **Built-ins.** The document, HTTP, the model, forms and slots are declarations too, located as
  plugins of the framework's bundle (`plugin("alis.document")` and so on):
  - the document's `Ready` event, `Raise(event, detail)` and `Query`;
  - HTTP's `Send(request)`;
  - the model's `Values` and `Fields`;
  - a form's `Validate()` and `ShowErrors(problem)`;
  - a slot's `Load(html)` and `Unload()`.

  The fluent request, dispatch and URL APIs are sugar over these members.
- **App objects.**
  - `p.Component<NativeDrawer>()`, `p.Component<FusionToast>()` and `p.Component<Confirm>()`.
  - Confirm's `Ask` is an async member. The Fusion package's confirm module defines it on the dialog
    element, so the `window.alis.confirm` global (`confirm.ts:103`) goes.

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
  - Empty input follows the property's CLR type, as model binding and FluentValidation do on the
    server. A non-nullable number reads an empty input as its default, so `GreaterThan(0)` fails on
    empty. A nullable property's comparison passes on empty.
    - Today the browser fails `gt` on empty but passes `min`, `max`, `lt` and ranges
      (`rule-engine.ts:185-215`).
    - A frozen journey that depended on that difference fails at the validation step and comes to
      the owner for a decision; it is not edited.
  - Messages keep today's browser text through `.WithMessage`, because FluentValidation's defaults
    differ. For example, its default `'Age' must be greater than '0'.` differs from today's
    `'Age' must be greater than 0.`.
  - Dates compare as timestamps.
  - Regex patterns are checked at render against JavaScript's dialect, so a .NET-only construct
    fails in C#.
- **The form is an object.** `plugin("alis.form", id)` holds the projected tests and the inputs bound to
  it. `Validate()`, `ShowErrors(problem)` and `Values` are its members, and live clearing and
  revalidation happen inside it. Each test is `{ field, check, when, message }`, where `check` and
  `when` are expressions (section 0.5).
- **Deleted:** `ClientRule` and its builders, the 18 `WhenField*` methods and `WhenFields`, the
  field-condition builders, the operand and activation types, three interfaces,
  `AddReactiveClientValidation`, and the whole runtime rule engine.

## 5. Authoring rules: one parameter order everywhere

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
- **Operators.** C#'s operators map one-to-one onto the closed operator table (`grammar.md`
  section 2.2), which gives each one the answer C# gives. Values are converted by shape where they
  are read and written (section 2.1); explicit C# casts become conversion operators.
- **Members** of `string`, `DateTime`, `IEnumerable<T>` and declared objects map through the one
  member table (`grammar.md` section 2.3). A new member is one table row and no runtime change.
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

## 8. The runtime: an interpreter and its modules

| Part | What it does | What it knows |
|---|---|---|
| Interpreter | evaluates the nine expressions synchronously, executes the six statements (waiting only where the plan says `await`), registers handlers | the grammar and nothing else |
| Locators | `element` follows the vendor's data; `plugin` asks the module registered under the name for the object with that key, and hands it the plan's data under the owner | the two locator kinds |
| Owners | the page and each slot: disposers for their subscriptions and plugin contributions | lifetime, nothing about features |
| Plans | discovery, composition by `planId`, slot load and unload | plan scripts |
| Modules | the framework bundle's `alis.document`, `alis.http`, `alis.model`, `alis.form`, `alis.slot`; the realtime bundle's `alis.stream`, `alis.hub`; the packages' element modules for Drawer, Loader and Confirm | each its own members and data |

- **Dependency rule.** The interpreter imports no module and no vendor. A module that evaluates
  expressions (the form, for its tests) is handed the evaluator. The architecture
  test enforces both directions.
- **Deleted outright:** about 1,800 of today's 6,200 lines.
  - `compare-engine.ts` (426), `rule-engine.ts` (296), `rule-operands.ts` (198), `array-op-engine.ts`
    (135) and `conditions.ts` (129): one evaluator replaces them.
  - `gather.ts` (180): `record`.
  - `object-contracts.ts` (160), `merge-policy.ts` (75) and `component-event-contract.ts` (14):
    nodes carry their path and shape.
  - `native-action-link.ts` (107): the link is an ordinary handler.
  - `execution-context.ts` (94): lexical bindings replace the event, response and request context.
  - `http.ts` keeps sending and reading the body; its routing becomes `if` and `try`.
- **The async lane is decided in C#.** Only `do … await` and `let … await` wait: HTTP, Confirm,
  and async plugin and hub methods. Slot loading is synchronous, as today. A stream or hub message
  starts its handler synchronously, like any other event.

## 9. The plan document

```json
{
  "version": 4, "planId": "…", "owner": { "kind": "page" },
  "vendors": { "fusion": { "objectPath": ["ej2_instances", 0], "subscribe": "addEventListener", "unsubscribe": "removeEventListener", "payload": "argument" } },
  "objects": {
    "roster":            { "kind": "element", "id": "roster", "vendor": "fusion" },
    "Ns_Admission__Age": { "kind": "element", "id": "Ns_Admission__Age", "vendor": "fusion" },
    "http":              { "kind": "plugin", "name": "alis.http", "key": "", "data": {} },
    "model":             { "kind": "plugin", "name": "alis.model", "key": "…", "data": { "inputs": {
      "Ns_Admission__Age": { "name": "Age", "value": { "kind": "fn", "params": ["self"], "body": { "kind": "get", "target": { "kind": "ref", "name": "self" }, "path": ["value"], "shape": "number" } }, "committed": "change" } } } },
    "admission-form":    { "kind": "plugin", "name": "alis.form", "key": "admission-form", "data": { "tests": [
      { "field": "Ns_Admission__Age", "check": { "kind": "fn", "…": "…" }, "when": { "kind": "lit", "value": true }, "message": "Enter the resident's age." } ] } },
    "residents":         { "kind": "plugin", "name": "alis.hub", "key": "/hubs/resident", "data": {} }
  },
  "handlers": [
    { "on": "roster", "event": "actionBegin", "param": "args",
      "do": [ { "kind": "set", "target": { "kind": "ref", "name": "args" }, "path": ["cancel"], "value": { "kind": "lit", "value": true, "shape": "boolean" } } ] }
  ]
}
```

- **Where things come from, and what happens.** `objects` say where each object comes from, and a
  plugin object carries the data its module needs (the model's inputs, a form's tests). `handlers`
  are the behavior. Nothing else is in a plan, and nothing in it names a feature.
- **What goes:**
  - the per-element `types`, member keys and signature keys, and contract merging across partials
    (`merge-policy.ts`);
  - `components` with roles and `none` placeholders (`binding: {kind:"none"}`,
    `container: {kind:"none"}`, `plan.ts:80-170`);
  - the plan-side registries `DeclareProperty` and `DeclareMethod`;
  - every translation layer (`ToPlanRule`, `ToPlanCondition`): the C# domain is the JSON.
- **Composition.** Plans with one `planId` join their maps and handlers. A slot owns the plans it
  loaded.
- **Today versus target.** Today's contract has 136 interfaces, 60 unions and 65 kinds. The grammar
  needs about 25 node types: six statements, nine expressions, two locators, and the handler,
  plan, input, form, test, shape and path. The real count is measured when the generator runs.

**Diagrams and traces:** the design review page "Alis.Reactive Object Model"
(https://claude.ai/artifact/MhD8NHsoMNBKmuNXABsdbu). It draws the grammar and lowers real views
through it.

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

Realtime is part of step 0. Its gaps, from `use-cases/http.md`:

| Status | Rows | What is missing |
|---|---|---|
| No test | 103 | a stream ignoring other event types |
| No test | 104 | unnamed stream messages |
| No test | 105 | one `EventSource` per URL |
| No test | 108 | the untyped hub overload |
| Weak | 109 | one hub connection shared with a partial, proven only by a console trace |
| Weak | 110 | two hubs delivering independently |
| Weak | 111 | repeated pushes updating the page |

Each one gets a visible-outcome test before the stream and hub locators replace today's trigger
kinds.

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
- **Realtime as trigger kinds of its own** (`ServerPush`, `SignalR`, and the previous draft). These leave `EventSource` and
  `HubConnection` outside the object model, so their properties, their methods (`close`, `invoke`,
  `send`) and their connection events have no typed access, and the runtime keeps two extra trigger
  kinds.
- **Lambdas instead of the fluent conditions** (`p.When(m => m.Age > 65)`). They would replace the
  owner's fluent API and give branch conditions a second way to be written; the fluent API keeps
  its shape, gets types, and lowers to `op`.
- **Operators on values** (`rate > 100m`). These need `==` overloaded on a class, cannot restrict
  ordering by `T`, and arrays would still need lambdas.
- **Freezing arrays and pushing the rest to plugins.** It contradicts array-driven components.
- **A source generator.** The owner rejected it.
- **Feature nodes in the plan** (`request`, `dispatch`, `inject`, `show-validation-errors`, a
  `confirm` condition, one trigger kind per source). Each feature brings a sub-grammar, the runtime
  must know every feature, and a brought component can never do what a built-in feature does.
- **`await` inside expressions.** The evaluator would become asynchronous everywhere, and a
  payload write could land after the vendor reads it. Spilling into `let` keeps one synchronous
  evaluator and keeps the async lane visible in the plan.
- **Blocks as values** (`http.send(request, onSuccess, onError)`). The plan would become a general
  programming language and hide control flow inside objects. `if`, `try` and `all` express every
  lifecycle the DSL has.

## 12. Order of work

Grammar first, then the object model, then views area by area. While areas move, the runtime boots
each plan by its `version`: grammar plans go to the interpreter and today's plans to today's
executor. The switch is deleted with today's executor when the last area has moved.

0. **Step 0 proof gaps.** The 162 rows, closed on the current code.
1. **The grammar.** One C# node per production and the generated contract; the interpreter,
   locators, owners, discovery and composition.
2. **The object model on it.** `JsObject` members, `Value<T>`, `Arg<T>`, the active-scope stack,
   `p.Field`, and the expression compiler (data paths, arrays, text).
3. **Areas, each moved whole (a view and its partials) and green in CI before the next.** Each area
   brings the vocabulary it first needs, declared in C# and implemented in the runtime once:

   | Order | Area | Vocabulary it brings |
   |---|---|---|
   | 1 | HTTP pipeline | `http`, elements, Loader, the action link, the request API |
   | 2 | Conditions, arrays, dispatch, plugins | the fluent conditions, Confirm, the document, plugins |
   | 3 | Fusion Grid journeys | the Fusion vendor, Grid and its payloads, slots and `Into` |
   | 4 | Validation | `form`, the FluentValidation projection, inputs and `Committed` events |
   | 5 | Realtime | `stream`, `hub` |
   | 6 | Patterns and every remaining component page | the rest of the 66 declarations, Native, Tom Select |

4. **Delete today's executor, plan domain, builders and the version switch.**

The fixtures stay frozen throughout (section 10), so each green area proves that its use cases
behave as before.

## 13. Decisions after the audit of October 2026

An audit of every claim above against the code found the grammar, the plan document and the runtime
built as written, and the C# object model of step 2 not built. These decisions settle what of it is
built and what is not.

- **The handle model (sections 3, 4.0, 4.1, 4.3 values, rules 3, 4 and 6 of section 5) is not built.**
  The owner keeps the typed fluent API of descriptors and verbs (`p.Component<T>(..).Set(c => c.X, v)`,
  `.Get(c => c.X)`, `p.Payload(args).Get(a => a.X)`, the `(source, x => x.Path)` pairs). It reaches
  every member of every object, shipped or brought, with the compiler checking each one; the handle
  model would rewrite 70 declarations and about 2,000 call sites for the same reach. Approach A of
  section 11 is therefore what ships, with its defects removed (below).
- **Built from the audit:**
  - typed gather and `p.Url<TQuery>()` (section 4.6, as built).
