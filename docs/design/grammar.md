# The grammar: specification

This is the specification the code is written from: what the plan is, what it means, and what each
object in the vocabulary must do. The C# productions are in `Alis.Reactive/Grammar`, the generated
contract in `Alis.Reactive.Assets/runtime/types/plan.ts`, and the interpreter in
`Alis.Reactive.Assets/runtime/grammar`. How a component joins the vocabulary is
[`bring-your-own-component.md`](bring-your-own-component.md).

**The rule behind every decision:** the browser computes what the C# the developer wrote would
compute. Types, operators, evaluation order, scoping and empty values follow C#, and the server's
model binding and validation rules where those apply. Where JavaScript would answer differently,
the grammar says so explicitly instead of inheriting JavaScript's answer.

## 1. Syntax

### 1.1 The plan document

```
plan      := { version, planId, vendors, objects, handlers }
vendor    := name -> { objectPath, listen, payload: argument | element }
listen    := method(subscribe, unsubscribe)   object[subscribe](event, fn) adds a listener, object[unsubscribe](event, fn) removes it
           | assignment                       object[event] = fn; a function already there runs first, and is put back on unload
object    := id -> locator
locator   := element(id, vendor)            found on an element, through its vendor's data
           | plugin(name, key, data)        made by the plugin a bundle registered under `name`
handler   := { on: object, path, event, do: block }     the block reads the payload as `e`
```

That is the whole document. The grammar names no feature.

- **Handler path.** `path` is the member path from the object `on` names to the object whose
  `event` is heard: empty for the object itself, the holding member's path for an object it holds
  (`Object<T>()` on a property). The vendor subscribes on the object at that path; a plugin has no
  path.
- **Plugins.** A plugin is registered by name by a JavaScript bundle:
  - the framework's own bundle registers `alis.document`, `alis.http`, `alis.model`, `alis.form`,
    `alis.slot`, `alis.stream` and `alis.hub`;
  - an app registers its own plugins.

  `key` picks one object of the plugin (empty for a singleton): a form's container ID, a slot's
  container ID, a stream's URL. The framework's features use the same extension point an app does.
- **Plugin data.** `data` is JSON the plugin defines, and it may contain expressions. Each plan
  contributes its data under its owner, and the plugin joins the contributions per key. The C#
  declaration of each plugin states its data type, so the generated contract types it too.
- **Inputs and forms are plugin data.**
  - The inputs of a view are `alis.model` data: `{ inputs: { id: { name, body, value, shape,
    committed, list } } }`: the MVC binding name, the JSON body path, the value as an expression of
    the located object, the shape it reads as, the event that commits a value (M4), and whether the
    input binds a list (H2, H4).
  - A form's fields are `alis.form` data: `{ summary, fields: [{ id, name, value,
    tests: [{ rule, check: fn(v), when, message }] }] }`: one per member a rule is written on, and per
    rendered item of a collection a rule checks, whether or not the browser can check it (a rule under
    a condition only the server decides still names its members), so `tests` is empty when every
    check is the server's. An item's field carries the checks of every rule on its collection. `name` is
    the input's binding name, under the view's field prefix, as the server reports it. `rule` names the
    declaration a test came from, for traces only. An input
    does not name its form; membership is decided by containment (6.4), because a partial cannot
    know which form will contain it.
- **No owner field.** The owner of a plan is whoever discovered it: the page at boot, or a slot when
  it loads.

### 1.2 Statements and expressions

```
block   := stmt*
stmt    := set(target: expr, path, value: expr)
         | do(expr, await)
         | let(name, expr, await)
         | if(arm+, else: block)            arm := (expr, block)
         | all(block+)
         | try(block, finally: block)

expr    := lit(json)
         | obj(id)
         | ref(name)
         | get(expr, path)
         | as(expr, shape)                   the value converted to the shape (2.1)
         | call(expr, path, args: expr*)
         | fn(param+, expr)
         | record(field*)                    field := (name, expr)
         | list(expr*)
         | op(operator, expr+)
```

Six statements, ten expressions. JSON names each node by `kind` with exactly these names, and
carries nothing else: a node has exactly the members its production lists.

### 1.3 Paths

A path is a list of segments: a member name, an array index, or `[]`, which reads the rest of the
path on each item of the list before it (`filesData[].rawFile` is every file's `rawFile`; a list
inside a list is refused where it is declared). The C# side never mixes member and data segments in
one node:
- **member segments** come from declarations (`grid.Rows<Resident>()`, `schedule.EventSettings.Events<Shift>()`);
- **data segments** come from `Get(x => x.A.B)` over a data type.

A member read followed by a data read is two nested `get` nodes.

- **Member paths rooted at `obj(id)` are checked.** Every segment must exist, and an absent one is a
  boundary error that names the object, its ID and the path. A vendor version without a declared
  member fails loudly instead of reading `undefined`. The items a `[]` reads are checked the same
  way, so an upload whose files lost `rawFile` fails at the item instead of posting nothing.
- **Every other path is optional.** This covers paths off a handler's payload, a response, a record,
  a plugin's data (`document.query`, `model.values`) or a returned object. A missing link reads as
  missing, and a call on a missing target returns missing. This is JavaScript's `?.`.
- **Writes.**
  - A `set` rooted at `obj(id)` needs every segment, the final member included, so a write to a
    member the vendor does not have fails loudly instead of creating a property.
  - Any other `set` needs the object that holds its final member, and creates that member when absent, so a
    payload's `cancel` can always be written.

Where a path is rooted decides how it is read; the runtime decides nothing else.

### 1.4 Shapes

A shape is the C# type's view of a JavaScript value. The generated contract (`plan.ts`) has these
kinds:

`string`, `number`, `boolean`, `date`, `instant`, `array(shape)`, `nullable(shape)`,
`object(fields)`, `any`, `none`.

- `any` is a value taken as it is: a live JavaScript object, or data whose members the plan does not
  name. It is never converted.
- `object(fields)` is JSON-shaped data whose members the plan names: a record the DSL builds, and an
  argument of a method typed by the app's own class (a server's class, a row, an event), whose
  public members, by their JSON names, are its fields. A class that contains itself is `any` where it
  recurs; a declaration (`JsObject`), a dictionary and a framework type are `any`.
- An enum is `string`: its member name, which is how MVC binds it. A `Guid`, a `TimeSpan` and a
  `TimeOnly` are `string` too.
- `date` values are JavaScript `Date` objects. `DateTime` and `DateOnly` are `date`.
- `instant` values are `Date` objects too, for a `DateTimeOffset`: a moment, not a calendar day.

## 2. Values

### 2.1 Conversion happens at the boundary, by shape

A value changes type in exactly one place: `as(expr, shape)`, by the declared shape. The C# writes
it where a conversion is due and nowhere else:
- **Reading:** a member read (`get`) or a method's result is wrapped in `as` with the member's shape;
  a method result bound by a `let` is wrapped where the name is read.
- **Writing:** a `set` value and each `call` argument are wrapped in `as` with the target member's
  shape.
- **Literals:** a literal is wrapped only when it is not already a value of its shape: a date or an
  instant written as text, a list or a record with members to convert. Text under `string`, a boolean
  under `boolean`, a number under `number`, and a missing value are written bare.

A shape that converts nothing (`any`, `none`) is never written. Nothing else converts.

| Shape | From | Result |
|---|---|---|
| `number` | number | itself |
| | boolean | 1 or 0 |
| | text | `Number(t)` if finite. `""` and whitespace give 0, as MVC binds a non-nullable number. |
| | `Date` | its time |
| | otherwise | kept |
| `date` | `Date` | itself |
| | number | `new Date(n)` |
| | `YYYY-MM-DD` text | local midnight |
| | other text | `new Date(t)` when valid, else kept |
| `instant` | as `date` | the same moment, as an instant |
| `boolean` | text | false only for `""`, `"false"` in any case, and `"0"` |
| | number | false only for 0 and NaN |
| | array | `length > 0` |
| `string` | number or boolean | `${v}` |
| | instant | the moment in UTC: `YYYY-MM-DDTHH:mm:ss.fffZ` |
| | other `Date` | local calendar text: `YYYY-MM-DD` at midnight, else `YYYY-MM-DDTHH:mm:ss[.fff]`, no offset |
| | array | `String(a)` |
| | missing, when writing to an element's text | `""` |
| `array(s)` (reading) | array | each item converted to `s` |
| | `""` | `[]` |
| | another scalar | `[v]` |
| | missing | stays missing |
| `nullable(s)` | missing, or text that is empty or whitespace | `null` |
| | otherwise | converted to `s` |
| `object(fields)` | an object | each named member converted to its shape, every other member kept; the object itself when no member changed (a library that keeps it by reference still has it) |
| | otherwise | kept |
| `any`, `none` | any | itself |

- A missing value (`null` or `undefined`) passes through unchanged, except for `nullable`, where it
  becomes `null`.
- A value that cannot be converted is kept as it is.

These rules are `runtime/grammar/shape.ts`. A date value is a `Date`, not a timestamp, so text, JSON and form data see the local day and time picked, with no offset, as
an HTML form posts a date (a UTC instant would move a day picked east of UTC to the day before, and
a `DateOnly` member cannot bind a time), and every comparison sees the time (2.2). An instant is
the exception: a `DateTimeOffset` names a moment, so it is sent as that moment in UTC, which binds
exactly whatever the zone of the browser or the server.

**Binding reads.** Where the server binds a value, the browser reads it the way MVC binds it.
- **Where:** the `alis.model` values a request body carries (`values`, `fill`, `fields`, so
  `Form()` sends them; `Form(m => m.X)` reads one input with `model.boundValueOf(id)`), and every
  input a validation rule reads: its field, a peer, and each
  member its condition reads, through `model.boundValueOf(id)`.
- **What:** text that is empty or only whitespace is `null`, as MVC's `ConvertEmptyStringToNull`
  makes it; a non-nullable number, boolean or date left empty is its type's default (0, `false`,
  0001-01-01), the value the property keeps when there is nothing to bind; a nullable one is `null`.
- **Elsewhere** a read keeps its shape conversion alone, so `p.When(name.Value).IsEmpty()` still sees
  `""`.

### 2.2 Operators

The operator set is closed. Each operator gives the answer C# would give for the developer's
expression over the converted values.

| Operator | Meaning |
|---|---|
| `==`, `!=` | Both missing are equal; one missing is unequal; two `Date`s compare by time; otherwise `===`. This is C#'s lifted equality. |
| `<`, `<=`, `>`, `>=` | Defined for two finite numbers, two `Date`s by time, two texts by code unit, or two booleans. Every other pair is false, including missing, NaN and mixed kinds. This is C#'s lifted comparison. |
| `&&`, `\|\|`, `!` | Short-circuit, left to right. |
| `+ - * / %` | C# arithmetic over numbers; `+` over texts concatenates. |
| `??`, `?:` | As in C#. |
| `merge` | The fields of every record operand, left to right, in one record (a gathered request's values with the model's). |
| `-x` | C#'s unary minus over a number (`neg`). |
| `truthy` | JavaScript's truthiness, applied after conversion. It is used by `Truthy()` and `Falsy()`. |
| `empty` | True for `""`, missing, and an empty array. Nothing is trimmed. It is used by `IsEmpty()` and `NotEmpty()` in conditions. |
| `matches(a, pattern, flags)` | False when `a` is missing; otherwise `new RegExp(pattern, flags).test(text(a))`. |
| `toNumber`, `toDate`, `toBoolean`, `toText` | Explicit C# casts. They use the 2.1 table. |
| `toArray` | The target of a LINQ method. Missing gives `[]`; text or a number gives `[v]`; an iterable gives `Array.from`; anything else is a boundary error. |
| `finite` | `Number(v)` if finite, else 0. `Sum` uses it, so that text never concatenates. |
| `order(a, b)` | The `OrderBy` comparator. Finite numbers order by difference, with non-finite values last; everything else orders by `String(v)` in code-unit order. It is stable. |

- **An `if` arm** is taken when its condition is truthy. A missing value is false.
- **The validation rule `NotEmpty`** is not `empty`. It is a check chosen by the property's
  type (section 7.4).

### 2.3 Lambdas, arrays and text

The C# expression compiler turns a lambda into `fn`. Inside it:
- a parameter of a data type reads by path (`get`);
- a parameter of a declared object type (an element in `Children`, say) exposes the members its
  declaration names;
- the members below map to JavaScript.

The table below is the complete member table. A member that is not in it is an analyzer error at
compile time, and the compiler fails at render as a backstop. A new member is one row, with no
runtime change.

| C# | JavaScript |
|---|---|
| `Where(p)`, `Select(f)` | `toArray(r).filter(fn)`, `.map(fn)` |
| `OrderBy(k)`, `OrderByDescending(k)` | `.toSorted((a, b) => order(k(a), k(b)))`, negated for descending |
| `Count()`, `Count(p)` | `.length`, `.filter(fn).length` |
| `Any()`, `Any(p)`, `All(p)` | `.length > 0`, `.some(fn)`, `.every(fn)` |
| `Sum(f)` | `.reduce((s, x) => s + finite(f(x)), 0)` |
| `Find(p)`, `Find(p, f)` | `.find(fn) ?? null`, then `f` when found |
| `Enumerable.Contains(constant array, x)` | `[..].includes(x)` |
| `string.Contains`, `StartsWith`, `EndsWith` | `includes`, `startsWith`, `endsWith`; a missing target gives false |
| `string.Length`, `Trim()`, `ToUpper()`, `ToLower()` | `length`, `trim()`, `toUpperCase()`, `toLowerCase()` |
| `string.IsNullOrEmpty(s)` | `s == null \|\| s === ""` |
| `Regex.IsMatch(s, constant)` | `matches(s, pattern, flags)` |
| `DateTime.DayOfWeek`, `.Month`, `.Year`, `.Day`, `.Hour`, `.Minute`, `.Second` | `getDay()`, `getMonth() + 1`, `getFullYear()`, `getDate()`, `getHours()`, `getMinutes()`, `getSeconds()` |
| a declared element's `GetAttribute(n)`, `HasAttribute(n)` | `getAttribute(n)`, `hasAttribute(n)` |
| operators, `??`, `?:`, casts | the operators of 2.2 |

These are the members built (`Alis.Reactive/PlanAuthoring/Arrays/ReactiveArray.cs`, and the
allow-list and string operators of `Alis.Reactive/PlanAuthoring/Arrays/ElementExpressionCompiler.cs`)
plus those the validation `When` conditions need. That allow-list names JavaScript's
date methods directly (`getDay`, `getTime`) on any C# method of that name, and no view calls one;
here they are reached from `DateTime`'s own members. Members that
nothing uses yet (`First`, `Distinct`, `Skip`, `Take`, `SelectMany`, `Min`, `Max`, `Average`,
`ThenBy`, `Concat`) are added the same way, each with a journey that proves it.

## 3. Evaluation and execution

### 3.1 Bindings

A block runs in an environment of names:
- a handler binds its payload as `e`;
- `let` binds a name for the rest of its block;
- `fn` binds its parameters for its body.

A method that returns a value or an object is bound once per block: the `let` that holds its result
is hoisted before the statement that first reads it (with `await` when the method is async, 3.3),
and every later read in the block, or in a block inside it, is `ref` of that name. A result no
statement reads is bound at the method's own line. A read never runs the method again.

A `try` body's top-level bindings stay in scope after the `try`. Code after a `try` runs only when
the body completed, so each of them was bound.

A name is visible only where C# could see the variable that produced it. The builder enforces this
at render: a handle used outside the block that binds it throws, and names the handle.

### 3.2 Statements

| Statement | Behavior |
|---|---|
| `set` | Evaluates the target and the value, and writes it (1.3); the value's conversion is its own `as`. |
| `do` | Evaluates the call for its effect. |
| `let` | Binds the value. |
| `if` | Takes the first arm whose condition is truthy, else the `else` block. |
| `all` | Starts every block, waits until all have settled, and logs each failure without rethrowing. |
| `try` | Runs the body, then the `finally` block on every exit; an error from the body propagates after the `finally` block. |

### 3.3 Await

- `do` and `let` carry `await` only when their call is to an async member.
- A block runs synchronously until its first awaited statement, then continues when that settles.
  A handler with no awaited statement never returns a promise.
- Expressions never await, so there is one synchronous evaluator. Only the call at the top of a `do`
  or `let` may return a promise: an awaiting statement settles it, a `do` without `await` lets it
  run. A promise anywhere else comes from a member declared sync, and the call fails (3.4).

**A property is read where it is used.** A property read is a read, like a property getter, so each
use reads the current value: `var before = rate.Value;` then `rate.Value.Set(5)` makes `before`
read 5. Statements run in plan order, so a step written before the write reads the earlier value. A
method's result is not re-read: it is bound once (3.1).

**Plan order is C# evaluation order.** A returning method emits its `let` (`let … await` when the
method is async) before the statement that first reads the result, in the active scope, and returns
a value that names the result:
- the value is read, never invoked again, however often it is used;
- a call written as an argument runs before the statement it feeds, as C# evaluates arguments
  first.

The fluent builders that take callbacks (`p.Confirm`, groups, `ElseIf`) build their conditions in
short-circuit order. A Confirm inside a group or an `ElseIf` is asked only when it is reached: the
builder nests the `if`s, repeating the shared branch, the way the C# compiler lowers an `await`
inside `&&`.

### 3.4 Errors

The runtime raises errors only at real boundaries, and each error names the object, the path and
the plan. The boundaries are:
- object lookup;
- a checked path (1.3);
- a member declared sync that returns a promise (3.3);
- an unknown plugin;
- a browser API;
- the network;
- JSON the framework did not produce.

A failing handler is logged and stops no other handler.

### 3.5 Boot and lifetime

1. **Owners.** An owner is the page, or a slot. It holds disposers: its handler subscriptions, its
   plugin data contributions, and the connection subscriptions those plugins made for it. It knows
   nothing about what they are.
2. **Starting an owner.** For the page at boot, or a slot at load:
   1. discover the plan scripts and compose the ones that share a `planId`;
   2. locate the objects;
   3. pass each plugin its data contribution under the owner;
   4. subscribe every handler;
   5. start the owner, which runs the handlers waiting for this owner's start (`document`'s `ready`,
      D1).

   So a ready handler can raise custom events other handlers already hear. Loading a slot starts
   only the slot's handlers, so a ready handler that loads a slot does not run again.
3. **Booted mark.** After the page starts, the root element gets `data-alis-booted="true"`, the one
   mark of a booted page, readable by whoever arrives later (`Alis.Reactive.Assets/runtime/root.ts`).
   The Playwright tests wait on it.
4. **Unloading a slot** runs its owner's disposers, and the boot-owned ones stay. A block that is
   already running finishes; nothing aborts it (`Alis.Reactive.Assets/runtime/plugins/slot.ts:54-60`
   disposes the owner and nothing else).
5. **Mount events.** The app hears `alis:unmount` and `alis:mount` on the document, with
   `detail.root`, around every change of content. This is the public contract with app bundles
   that construct and destroy their own widgets.

## 4. Static checks: what makes a malformed sentence impossible

| Rule | Enforced by |
|---|---|
| `set` targets a writable member | C# type: `ReadOnly<T>` has no `Set` |
| Members, arguments and payload types exist and match | C# type |
| Ordering applies only to comparable types; text operators apply only to text | C# type: generic constraints |
| An event's payload type matches the callback | C# type |
| A member call that returns a value is not discarded | analyzer: a discarded `Value<T>` is an error |
| Every handle is used inside an active block, within its binding's scope | builder at render, naming the handle |
| A lambda uses only members the compiler maps | analyzer at compile time; render as a backstop |
| Every route placeholder is filled from the request | render |
| A regular expression is in JavaScript's dialect | render |
| Input IDs come from `IdGenerator`; other IDs are explicit; no ID repeats in a plan | render |
| A validation rule reaches the browser only when the page can evaluate it (7.5) | render |
| Enum literals are emitted as names, the same text the input reads | construction |
| A member path and a data path are never one node (1.3) | construction |
| Exact-status routes match before catch-alls; only the first `OnSuccess` route runs; error routes sit behind `!res.ok` | construction: the request builder orders them |

## 5. Lowering: each fluent method, as productions

The owner's fluent API uses each object as the object (`rate.Min.Set(0m)`, `grid.Refresh()`). Every
form lowers to fixed productions.

| Fluent API | Lowers to |
|---|---|
| `.Reactive(evt => evt.E, (args, p) => …)`, on a rendered component | `on(o, "e", args) { … }` |
| `t.DomReady(p => …)` | `on(document, "ready", _)` |
| `t.CustomEvent(appEvent, (e, p) => …)` | `on(document, name, e)`; the payload is `detail`, or the event when it has none |
| `t.ServerPush(ServerPushStream.At<S>(url), s => s.E, (args, p) => …)`, `t.SignalR(SignalRHub.At<H>(url), h => h.E, (args, p) => …)`, an `Event<TArgs>` of the stream or hub declaration | `on(stream, "e", args)`, `on(hub, "e", args)`; the hub's `reconnecting`, `reconnected` and `close` are declared the same way |
| `p.Stream(source).Close()`, `p.Stream(source).ReadyState`; `p.Hub(source).State`, `.ConnectionId` | `do(call(stream, ["close"], []))`, `get(stream, ["readyState"], number)`; `get(hub, ["state"], string)`, `get(hub, ["connectionId"], string)` |
| `h.M(a)`, a hub method declared with `Send(a)` | `do await call(hub, ["send"], [lit "m", a])` |
| `h.M(a)`, declared with `Invoke(a)`; `Invoke<T>(a)` | `do await call(hub, ["invoke"], [lit "m", a])`; `let t = await call(hub, ["invoke"], [lit "m", a], shape)` where it is called, and the value is `ref t` |
| `var c = p.Component<C>(…); c.P.Set(v)`, a `Property<T>` or `WriteOnly<T>` | `set(o, ["p"], as(v, shape))` |
| `c.M(a, b)`, a method declared with `Method(a, b)` | `do(call(o, ["m"], [a, b]))` |
| `c.P`, a `ReadOnly<T>` or `Property<T>` read | `as(get(o, ["p"]), shape)` |
| `c.F(a)`, a method declared with `Method<T>(a)` or `Object<T>(a)` | `let t = call(o, ["f"], [a])` before the statement that first reads it; the value is `as(ref t` (3.1) |
| a method declared with `MethodAsync()` or `MethodAsync<T>()` | `do await call(…)`; a read is `let t = await call(…)` before the statement that first reads it, and the value is `ref t` |
| `args.A.Get(x => x.B)`, `json.Get(x => x.A.B)` | `as(get(v, ["a", "b"]), shape)` |
| `args.Cancel.Set(true)`, `args.UpdateData(items)` | `set(v, ["cancel"], ..)`, `do(call(v, ["updateData"], [..]))` on the handler's parameter |
| `p.Element(id).Text.Set(v)` | `set(el, ["textContent"], text, v)` |
| `Html.Set(v)` | `set(el, ["innerHTML"], text, v)` |
| `Show()`, `Hide()` | `set(el, ["hidden"], boolean, false)` and `true` |
| `ClassList.Add`, `ClassList.Remove`, `ClassList.Toggle` | `do(call(el, ["classList", "add"], [..]))`, and so on |
| `p.Dispatch(appEvent, payload)`, `p.Dispatch(appEvent, d => d.Set(..))` | `do(call(document, ["raise"], [lit name, record]))` |
| `p.When(v).Op(x)…Then(..).ElseIf(..).Else(..)` | `if(op(..), …)`; `And`/`Or` become `&&`/`\|\|`; `Not(g)` becomes `!` |
| `p.Confirm(m).Then(t).Else(e)` | `let ok = await confirm.ask(m); if (ok) t else e` |
| `p.Get/Post/Put/Delete(url)` | see 5.1 |
| `.Into(elementId)` | `do(call(slot, ["load"], [ref res.body]))`, for the response of the request it follows |
| `p.ValidationErrors(container)` | inside an error route only: `do(call(form, ["showErrors"], [get(ref res, ["body"])]))` |
| `p.Parallel(a, b).OnAllSettled(c)` | `all(a, b); c` |
| `p.Url<TQuery>().Get(q => q.X)` | `as(get(get(document, ["query"]), ["X"]), shape)`, keyed by the member's binding name; an array member reads `queryAll` |
| `tools.Count(items, "status")`, a member of a plugin from `plan.RegisterPlugin<T>()` | `as(call(plugin, ["count"], [..]), shape)` |
| `Html.NativeActionLink(id, text, url, link => link.Post()…)` | the anchor plus its own plan script: `on(link, "click", e) { if (!modified(e)) { e.preventDefault(); … } }` |

### 5.1 A request

```js
if (form.validate()) {                            // .Validate<V>(container); outermost
  …WhileLoading…
  try {
    const req = record(…Gather…);                 // the gathered request
    const res = await http.send({ method, url, route, values: req, arrays, headers, format });
    if (res.ok) { …OnSuccess by status, exact first… }
    else if (res.status === 409) { …OnError(409)… }
    else { …untyped OnError… }                    // also reached by status 0
  } finally { …Finally… }                         // emitted only when Finally exists
  if (res.ok) { …Chained<TResponse>((res, next) => …)… }   // PlanAuthoring/Requests/ResponseBuilder.cs:135
}
```

- **Gather.** `Gather<TRequest>(g => ..)` builds the request the endpoint binds: `Set(x => x.M, v)`
  sets a member, `Form()` sends every input of the composed form, `Form(m => m.A, ..)` named inputs,
  and `Header(name, v)` a header. `.Gather(g => ..)` takes the view model as the request. Where a
  member goes is decided in C# when the request is built, by MVC's rule for route values: a top-level
  member named like a `{placeholder}` (ignoring case) fills the path (`route`, keyed by the
  placeholder's own spelling); every other member goes to `values`, which is the query string for GET
  and the body otherwise. Every placeholder must be filled, a member is set once, and a `Form` field
  must be a member of `TRequest` of a type its value binds to; each is checked at render.
- **Keys.** A member's key is its MVC binding name. `values` is a nested record for JSON
  (`{"Address":{"City":..}}`). For a query string or form data it is flat (`Address.City`,
  `Lines[0].Sku`), because only C# knows a dictionary from an object.
- **`Form()`** writes every input into the authored JSON record leaf by leaf with
  `model.fill(record)`, so an authored value beside an input (`Address.Verified` next to
  `Address.City`) is kept and an input wins only its own leaf. A query string or form data spreads
  `model.fields`.

### 5.2 Templates

A button inside a vendor template (`FusionTemplate.Create<TRow>()` … `.EventButton(text, event, e => e.Member, m => m.Row)`;
`Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/CareReview.cshtml:17` writes
`.EventButton("Start review", WingReviewEvents.StartReview, e => e.ResidentId, m => m.ResidentId, "e-small")`)
raises a declared `AppEvent<T>` and fills one payload member from the row. It renders
`data-alis-raise="name"` and its detail as attributes, not an inline `onclick`. The document plugin raises the event when it
is clicked (6.1), and handlers hear it with `on(document, name)`.

## 6. The vocabulary: built-in objects and what each must do

Each built-in object is a plugin, declared in C# through the object contract like any component,
with its data type, and implemented once as a plugin of its package's bundle. The interpreter
imports none of them. The obligations are numbered so that tests and code can cite them.

### 6.1 `document` (`plugin("alis.document")`)

- **D1 `ready`.** An event of the owner being started. A `ready` handler runs when its own owner
  starts, after every handler of that owner has subscribed (3.5), and never again.
- **D2 `raise(name, detail)`.** Synchronous. It dispatches a non-bubbling `CustomEvent` carrying the
  detail the plan gives it; a dispatch without a payload gives an empty record. A listener's payload
  is `detail`, or the event itself when the detail is missing
  (`Alis.Reactive.Assets/runtime/plugins/document.ts`).
- **D3 `query`.** The query string as a record, read the way MVC binds `[FromQuery]`:
  - keys match case-insensitively;
  - an array member reads every value;
  - a scalar member reads the first value;
  - an absent key reads as missing.
- **D4 Markup-raised events.** A click on an element carrying `data-alis-raise` raises that event
  with the element's detail. The listener is delegated, and it reads only attributes the framework
  stamped.

### 6.2 `http` (`plugin("alis.http")`)

`send({ method, url, route, values, arrays, headers, format })` is async and resolves to
`{ status, ok, body }`. `arrays` names the members of `values` that hold arrays, so a missing one sends
no key (H2) and no entry (H4).

- **H1 Route.** Each `{name}` is filled from `route[name]` with `encodeURIComponent`. A missing
  route value throws before anything is sent.
- **H2 GET.** `values` become the query string, joined with `&` when the URL already has a `?`.
  - An array becomes repeated keys.
  - A missing scalar is sent empty (`name=`).
  - A missing array sends no key, because `name=` binds as a collection holding one null
    (`Alis.Reactive.Assets/runtime/plugins/http.ts:112-128`).
  - A `File` cannot be sent in a GET; it is a boundary error.
- **H3 JSON.** `values` is the body. It is a binding read (2.1): dates serialize as their local text, and `""`
  is sent as `null` (`Alis.Reactive.Assets/runtime/plugins/http.ts:168-178`). An empty record sends no body.
- **H4 Form data.** One entry per key.
  - Arrays repeat the key.
  - A `File` is appended with its name, and a date is its local text.
  - A missing scalar is sent empty.
  - A missing array sends no entry (`Alis.Reactive.Assets/runtime/plugins/http.ts:136-153`).
- **H5 Headers.** A header whose value is missing is omitted.
- **H6 Body.** The body is read by Content-Type: JSON is parsed, text and HTML stay text, and an
  empty or other body is missing.
- **H7 Status 0.** Status 0 means no usable response: the network failed, or a JSON body could not
  be parsed. `ok` is false, and no exact-status route matches it.

### 6.3 `model` (`plugin("alis.model", planId)`)

- **M1 `values`.** Every mounted input of the composed plan, read through each input's `value`
  expression as a binding read (2.1), and nested by binding name. An unmounted input is skipped.
- **M2 `fields`.** The same inputs, flat, keyed by MVC binding name.
- **M3 `fill(record)`.** Writes the inputs of M1 into `record` at their body paths and returns it.
  A node already on the path keeps its other members; it is copied before it is written.
- **M5 `boundValueOf(id)`.** One input's value as a binding read (2.1), for a validation rule or a
  gathered `Form(m => ..)` field, wherever in the composition it was rendered; missing when no plan
  renders it.
- **M4 Commit.** Each input carries `committed`, the event its component declares as the one that
  commits a value (`ComponentInput.Of(Value, committedBy: ...)`,
  `Alis.Reactive/Components/Contracts/ComponentInput.cs:41`). When the plan that renders the
  input starts, the model subscribes it and re-raises it on the input's element as the bubbling DOM
  event `alis:commit`, once per element and for as long as the owner lives.

### 6.4 `form` (`plugin("alis.form", containerId)`)

`validate()` returns a bool; `showErrors(problem)`; the tests are the form's `alis.form` data (1.1).

- **F1 Scope.** A test applies only when its field's element is inside the container element. A
  missing container fails validation when the form has tests.
- **F3 First failure wins.** For each field, tests whose `when` is false are skipped, and the first
  failing `check` decides.
- **F4 Validate.** First clear every inline message and the summary, then evaluate every field,
  then return the AND.
- **F5 Unmounted field.** A field with no element posts no value, so each active test checks
  nothing, as the server checks a missing member: a required test fails, a length or a pattern
  passes. The first failing test's message goes to the summary.
- **F6 Display.** A message goes inline into `{id}_error` when that slot is renderable: it has a
  parent, and no ancestor has `hidden` or `display: none`. Then the field gets `alis-has-error`.
  Otherwise the message goes to the summary, `{planId with . and + as _}_validation_summary`, one
  entry per key, replaced and never duplicated.
- **F7 Live.** At creation (boot or slot load):
  - the DOM `input` event on a field clears its inline message;
  - DOM `blur` and `alis:commit` (M4) revalidate that one field, against every container that
    holds it (a section saved on its own sits inside the whole form), and the first failing test
    shows. A message the server placed (F8) stays on `blur`: no input and no commit came since, so
    the value it judged has not changed; typing clears it and a commit re-checks the field. The form never names a component's event, so the order in which the form and the
    input's plan start does not matter, and neither does the order the containers were declared.
- **F8 Server errors.** `showErrors(problem)` clears first.
  - When `problem` is a record with `errors`, it joins each field's messages with a space (each is a
    sentence) and matches a form field by its exact binding name, whatever its tests, then places
    the message as F6 does. Anything else goes to the summary.
  - When `problem` is not a record (an empty or non-JSON body), it logs `server-errors.not-a-record`
    and shows nothing: the server said nothing the form can place.
- **F9 Composition.** Tests from plans that share a `planId` are joined per owner. A slot's load or
  unload removes exactly its own tests and subscriptions, then clears and hides the summary of the
  plans it touched (the form plugin's contribution in `Alis.Reactive.Assets/runtime/plugins/form/form.ts`). Content rendered with the page belongs to the page even inside
  a slot's container: when a load replaces it, its tests stay, and its fields, no longer rendered,
  are checked as missing values (F5). A form whose steps replace each other in one slot therefore
  loads its first step into the slot too (the intake journey, `Validation/IntakeSteps`).

### 6.5 `slot` (`plugin("alis.slot", containerId)`)

`load(html)` is synchronous; `unload()`.

- **S1 Load order.** `load` works in this order:
  1. take the plan scripts out of the HTML;
  2. raise `alis:unmount` on the old content, which releases the owners of the slots inside it,
     innermost first, and the slot's own (3.5);
  3. replace the content;
  4. re-create the inserted `<script>` nodes in document order, so that vendor scripts run (this
     replaces `ej.base.append`);
  5. raise `alis:mount`;
  6. start a new owner for the slot with the plans it found (3.5).

  HTML with no plan only unloads.
- **S2 Unload.** `unload` raises `alis:unmount` on the content, releasing the same owners, and
  empties the slot. The DSL writes it as `p.Unload(elementId)`.
- **S4 Removed by a component.** Wired HTML a component removes itself is announced the same way,
  `alis:unmount` on its root (the drawer on the content it empties when it closes), and the owners
  inside it are released alike.
- **S3 Element.** The container is found with `getElementById` at every call, so a slot can sit
  inside content that appears after boot.

### 6.6 `stream` (`plugin("alis.stream", url)`)

- **R1 One connection.** One `EventSource` per URL, counted by subscriptions, and closed when the
  last one is released.
- **R2 Events.** A named event subscribes by its name, and unnamed messages arrive as `message`. The
  payload is `JSON.parse(data)`.
- **R3 Retry.** A permanent close adds a marker to `#alis-realtime-connection-retry-container`. One
  click retries every dropped connection, and the retried subscriptions keep their owners.
- **R4 Members.** `readyState` is the `EventSource`'s (0 connecting, 1 open, 2 closed); `close()`
  closes it, and a close is not a drop: nothing is retried.

### 6.7 `hub` (`plugin("alis.hub", url)`)

- **B1 One connection.** One `HubConnection` per URL, counted by subscriptions, with credentials and
  automatic reconnect.
- **B2 Start.** Starting retries at 0, 2, 10 and 30 seconds, then shows the retry indicator (R3).
- **B3 Messages.** Client methods are events, matched without regard to case. The payload is the
  method's single argument.
- **B4 Members.** `invoke(method, …args)` is async and returns a value; `send(method, …args)` is
  async; `state` and `connectionId` are properties; `reconnecting`, `reconnected` and `close` are
  events.

### 6.8 App objects (each its package's runtime plugin: `plugin("alis.confirm")`)

- **Confirm** (Fusion):
  - C1: `ask(message)` is async and resolves true on OK, and false on Cancel, close or Escape.
  - C2: Asks queue first in, first out.
  - C3: Escape inside the dialog never reaches the document.
  - C4: A missing dialog is a boundary error that names `FusionConfirmDialog`.
- **Loader** (Native):
  - L1: `show()` shows the loader inside the element its `target` property names, positioned over
    it, or over the viewport when `target` is empty; a `timeout` above 0 starts the auto-hide
    timer. `target` and `timeout` are properties the page sets before the show.
  - L2: `hide()` cancels the timer, returns the loader to `<body>` hidden, and clears `target` and
    `timeout`. It also returns there, hidden, when its target leaves the page.
- **Drawer** (Native):
  - W1: Closing announces `alis:unmount` on its content element and empties it, so what a plan
    loaded into it is released with it (6.5 S4).
- **Toast** (Fusion): its declared members, and no obligation beyond them.
- **Native CheckList and RadioGroup:** the native plugin defines `value` and `isInteracted` on the
  element (`Alis.Reactive.Assets/components/native/check-list.ts`, `radio-group.ts`), replacing the
  inline view scripts.

### 6.9 Elements and vendor objects

- **E1 Lookup.** An element or vendor object is looked up with `getElementById` at each use and is
  never cached. Absent is a boundary error.
- **E2 Payload.** The vendor's `payload` decides what a handler's parameter is:
  - `argument`: the vendor's listener argument (Syncfusion; the native link, whose listener argument
    is the click's DOM `Event`, from which it reads the button, the modifier keys and
    `preventDefault`);
  - `element`: the element itself, for native inputs whose values are read after an `await`, or a
    `CustomEvent`'s `detail`, or the event itself when it has none.
- **E3 Writes.** A write's value is converted by the member's shape (`as`): a date member receives a `Date`.
- **E4 Listening.** The vendor's `listen` decides how a handler attaches to the object, or to the
  nested object the handler's path names:
  - `method`: `object[subscribe](event, fn)` adds the handler and `object[unsubscribe](event, fn)`
    removes it when its owner unloads;
  - `assignment`: `object[event] = fn`, for a callback property such as a chart's `options.onClick`.
    The property must exist on the object (a checked member, 1.3). Assigning adds a listener as
    `method` would: the function the property already held runs first, with the same `this` and
    arguments, and keeps running, so a widget's own callback and an earlier handler both stay and two
    handlers of one event run in plan order. When the handler's owner unloads, the property gets back
    what it held, skipping handlers whose owners unloaded before it, whatever order owners unload in.

## 7. Validation projection

### 7.1 One statement per rule

`ReactiveValidator<T>` is the framework's own implementation of FluentValidation's grammar:
`RuleFor`, `RuleForEach`, `When`, `Unless`, `Otherwise`, `Include`, `SetValidator`, `ChildRules`,
the checks and `WithMessage`, written as FluentValidation users write them. The FluentValidation
package is not used. Each check is one term of the vocabulary and defines both halves where both
exist: what the server decides, and, for a term the browser decides the same way, the `BrowserRule`
the plan carries. The rules are data, like the `rules` schema of Syncfusion's React form, but they
are typed C#, bound to inputs by component ID, and conditional. The browser gets the rules of the
validator instance the server resolves for that request.

### 7.2 Server-only by what a rule is

A rule reaches the browser unless something in it only the server can do. No list is kept:
- `Must` and `MustAsync` are C# (a lookup, a service); they have no browser term.
- `WithMessage(model => ...)` builds its text from the model; only the server can say it.
- A message with nothing left to say once the sentence naming the entered value is dropped.
- `Matches` with an option JavaScript lacks or syntax it reads differently (`\A`, `\z`, `\G`,
  inline `(?i)`, atomic and comment groups, `\p{..}`, class subtraction).
- A condition declared `WhenOnServer` or `UnlessOnServer`: one the server alone decides (a method call,
  a service, a query over a collection).
- A rule on a computed value (`x => x.Name.Trim()`), which has no input.
- `.ServerOnly()`, which keeps the checks written before it on the server, for a member the page has
  no input for.

### 7.3 Conditions

- `When(predicate, rules)`, `Unless(...)` and `Otherwise(...)` around a block, and `.When(predicate)`
  and `.Unless(predicate)` after a rule's checks, take an expression. The server compiles it;
  `BrowserCondition` translates it into a `Condition` over the model's members when every part has a browser form:
  comparisons of members with fixed values, `string.IsNullOrEmpty`, `Contains`, `StartsWith`,
  `EndsWith`, `Regex.IsMatch`, membership in a fixed set, joined by `&&`, `||` and `!`.
  `WhenOnServer(predicate, rules)`, `UnlessOnServer(...)`, `.WhenOnServer(predicate)` and
  `.UnlessOnServer(predicate)` declare a condition the server alone decides.
- A condition after a rule's checks applies to every check written before it, as in
  FluentValidation.
- Blocks nest: a check's browser condition is the AND of its blocks' conditions and its own.
- A condition declared the server's keeps the checks under it on the server. A condition the translator
  cannot read that is not declared so is refused when a page describes the rules, naming the
  expression: no rule changes sides in silence.

### 7.4 The vocabulary

| Term | Server | Browser check `(v)` |
|---|---|---|
| `NotNull` | `v != null` | text or a nullable: not blank (blank text binds as null); a number, a boolean, a date or an enum that cannot be null always binds, so no browser check |
| `NotEmpty` | not null, blank text, an empty list or the type's default | by the property's type: text not blank; number not blank and not 0; boolean not blank and not false; date not blank and not 0001-01-01; enum not blank and not the member its default binds as; collection at least one item |
| `Empty` | the negation of `NotEmpty` | the negation of the row above |
| `MinimumLength`, `MaximumLength`, `Length` | null passes; the length is within bounds | `v == null` or the length is within bounds |
| `EmailAddress` | null passes; exactly one `@`, neither first nor last | the same |
| `Url` | null or empty passes; an absolute `http` or `https` address | null or empty passes; an `http` or `https` scheme and an authority with no whitespace; the server checks the rest |
| `CreditCard` | null passes; dashes and spaces dropped, every other character a digit, then Luhn | the same, written with `call` and `fn` |
| `Matches` | null passes; the regex matches | `v == null` or `matches(v, pattern, flags)`, flags from `RegexOptions`, when JavaScript reads the pattern the same way |
| comparison with a literal | ordered: null passes; equality: no null skip | ordered: `v == null` or `v op literal`; equality: `v op literal` |
| comparison with another member | the member is read by its path from the model | the peer is read by that path; ordered: `v == null` or (peer present and `v op peer`); equality: `v op peer` |
| `InclusiveBetween`, `ExclusiveBetween` | null passes; within the bounds | `v == null` or within the bounds |
| `SetValidator`, `ChildRules`, `RuleForEach(..).SetValidator`, `Include` | the child's rules, paths prefixed | the child's browser rules, prefixed, under the enclosing conditions |
| `Must`, `MustAsync` | the predicate | none: server-only |

- **Custom checks** are extension methods that compose the vocabulary, so they run in both places:
  `WristbandCode()` is `Matches(@"^WB-\d{6}$").WithMessage(..)`
  (`Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Validation/MoveInReview/MoveInReviewValidator.cs:107-108`).
- **Empty values** follow model binding: `v` and `model` are binding reads (2.1). A non-nullable
  number reads an empty input as 0, a nullable one as `null`, and text that is empty or only
  whitespace as `null`. So `MinimumLength`, `Matches` and `EmailAddress` pass on an empty optional
  field and `NotNull` fails on one, as on the server.
- **Messages.** The defaults are FluentValidation's English templates. The server fills every
  placeholder (`{PropertyName}`, `{PropertyValue}`, `{ComparisonValue}`, `{MaxLength}`, `{From}` and
  the rest, with `{Name:format}`); the browser fills those known at render and drops a sentence that
  names the entered value (`{PropertyValue}`, `{TotalLength}`).
- **Collections.** `RuleForEach(x => x.Lines).SetValidator(V)` emits V's tests once per rendered item
  index, and a check written on the items themselves (`RuleForEach(x => x.Tags).NotEmpty()`) is a test of
  each rendered item; the index is literal in the field ID and in every model path.

### 7.5 Which rules reach the browser

A rule reaches the browser when the page can evaluate it:
- its property is bound to an input on the page;
- every member its condition or peer reads is bound to an input or is a closure literal;
- nothing in it is the server's alone (7.2).

Every other rule stays server-only, with no extra code: `ServerFlag` has no input, and a collection
of complex items has none. The decision is made at render and is deterministic.

### 7.6 Grid column rules

The Fusion grid maps a validator's browser rules (`plan.BrowserRules<TValidator>()`) to Syncfusion's
column rules, keeping only those EJ2 decides as the server does: `required`, `minLength`,
`maxLength`, numeric `min`, `max` and `range`, and one pattern per column. `EmailAddress()` and
`Url()` reach the cell as EJ2's `regex` with the patterns the form tests them with (`PatternRule.Pattern`,
`Alis.Reactive/Validation/BrowserRules.cs`), because EJ2's own `email` and `url` rules decide differently.
It reads the rules by their classes, not the plan.

## 8. Where the code is

**C#** (`Alis.Reactive`)
- `Grammar`: one sealed class per production in 1.1 and 1.2, with internal constructors, invariants
  checked in the constructor, and one-to-one serialization. This is the plan contract; the generator
  (`tools/PlanContractGenerator`) reads these classes.
- `Components/Contracts`: the object contract every declaration is written with (`JsObject`,
  `Component`, `Property<T>`, `ReadOnly<T>`, `WriteOnly<T>`, `Objects<T>`, `Arg<T>`,
  `ComponentVendor`, `ComponentInput`; `Event<TArgs>` and `Event` are in `PlanAuthoring/Events`).
- `PlanAuthoring`: the fluent API (handlers, conditions, requests, dispatch, parallel, URL, plugins)
  and the lambda compiler (`Arrays/ElementExpressionCompiler.cs`) through the one member table (2.3).
- `PlanModel`: the one model that writes the grammar: steps (`Reactions/Steps.cs`), requests
  (`Requests/RequestStep.cs`), values (`Values/ValueExpression.cs`), conditions
  (`Conditions/Condition.cs`, one model for a validator's conditions over members and a reaction's over values), the form writer and the scope that binds responses and method
  results (`Writing/`). `PlanBuildContext.Write` writes the plan once at render.
- `Alis.Reactive.FluentValidator`: the projection of section 7 and its typed model.
- `Alis.Reactive.Fusion`, `Alis.Reactive.Native`: vendors, declarations, render helpers, Confirm,
  Toast, Drawer, Loader, and the grid's column rules.

**TypeScript** (`Alis.Reactive.Assets`)
- `runtime/grammar`: `evaluate` (the nine expressions), `execute` (the six statements, await),
  `operators`, `paths`, `shape` and `environment`. It imports nothing below.
- `runtime/objects`: the two locators (`plan-objects`), the vendor driver, the plugins registered by name,
  and owners.
- `runtime/plans`: discovery, start and the mount events.
- `runtime/plugins`: `alis.document`, `alis.http`, `alis.model`, `alis.form`, `alis.slot`;
  `runtime/realtime`: `alis.stream`, `alis.hub` and the retry indicator. The framework bundle
  registers all seven; they receive the evaluator and do not import the executor.
- `components/` (beside `runtime/`, in the same shipped bundle): the framework's own component code,
  built only on `alis:mount`, `alis:unmount` and `window.__alisPlugins`, as an app's is.

The architecture test enforces the import directions, that the runtime names no vendor, and that
`components/` and `runtime/` import nothing from each other.
