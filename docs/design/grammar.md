# The grammar: specification

This is the specification the code is written from. [`architecture.md`](architecture.md) section 0
introduces the grammar and says why; this file says exactly what it is, what it means, and what each
object in the vocabulary must do. Where the two differ, this file governs.

It was checked against all 531 inventoried use cases, then reviewed adversarially against the code
(section 9). Every decision that changes something a user could see at `195d3a1` is listed in section 8,
with its rows.

**What is built** is recorded in section 11: which parts run as specified and which decisions are
applied.

**The rule behind every decision:** the browser computes what the C# the developer wrote would
compute. Types, operators, evaluation order, scoping and empty values follow C#, and the server's
model binding and validation rules where those apply. Where JavaScript would answer differently,
the grammar says so explicitly instead of inheriting JavaScript's answer.

## 1. Syntax

### 1.1 The plan document

```
plan      := { version, planId, vendors, objects, handlers }
vendor    := name -> { objectPath, subscribe, unsubscribe, payload: argument | element }
object    := id -> locator
locator   := element(id, vendor)            found on an element, through its vendor's data
           | plugin(name, key, data)        made by the module a bundle registered under `name`
handler   := { on: object, event, param, do: block }
```

That is the whole document. The grammar names no feature.

- **Plugins.** A module is registered by name by a JavaScript bundle:
  - the framework's own bundle registers `alis.document`, `alis.http`, `alis.model`, `alis.form` and
    `alis.slot`;
  - the realtime package registers `alis.stream` and `alis.hub`;
  - an app registers its own plugins.

  `key` picks one object of the module (empty for a singleton): a form's container ID, a slot's
  container ID, a stream's URL. The framework's features use the same extension point an app does.
- **Plugin data.** `data` is JSON the module defines, and it may contain expressions. Each plan
  contributes its data under its owner, and the module joins the contributions per key. The C#
  declaration of each module states its data type, so the generated contract types it too.
- **Inputs and forms are module data.**
  - The inputs of a view are `alis.model` data: `{ inputs: { id: { name, body, value, shape } } }`:
    the MVC binding name, the JSON body path, the value as an expression of the located object, and
    the shape it reads as.
  - A form's fields are `alis.form` data: `{ summary, fields: [{ id, name, value, committed,
    tests: [{ rule, check: fn(v), when, message }] }] }`. `committed` is the input's event that
    revalidates the field; `rule` names the declaration a test came from, for traces only. An input
    does not name its form; membership is decided by containment (6.4), because a partial cannot
    know which form will contain it.
- **No owner field.** The owner of a plan is whoever discovered it: the page at boot, or a slot when
  it loads.

### 1.2 Statements and expressions

```
block   := stmt*
stmt    := set(target: expr, path, shape, value: expr)
         | do(expr, await)
         | let(name, expr, await)
         | if(arm+, else: block)            arm := (expr, block)
         | all(block+)
         | try(block, finally: block)

expr    := lit(json, shape)
         | obj(id)
         | ref(name)
         | get(expr, path, shape)
         | call(expr, path, args: expr*, argShapes: shape*, shape)
         | fn(param+, expr)
         | record(entry*)                    entry := (name, expr) | spread(expr)
         | list(expr*)
         | op(operator, expr+)
```

Six statements, nine expressions. JSON names each node by `kind` with exactly these names.

### 1.3 Paths

A path is a list of segments: a member name, an array index, or `[]`, which reads the rest of the
path on each item of the list before it (`filesData[].rawFile` is every file's `rawFile`; a list
inside a list is refused where it is declared). The C# side never mixes member and data segments in
one node:
- **member segments** come from declarations (`grid.DataSource`, `schedule.EventSettings.DataSource`);
- **data segments** come from `Get(x => x.A.B)` over a data type.

A member read followed by a data read is two nested `get` nodes.

- **Member paths rooted at `obj(id)` are checked.** Every segment must exist, and an absent one is a
  boundary error that names the object, its ID and the path. A vendor version without a declared
  member fails loudly instead of reading `undefined`. The items a `[]` reads are checked the same
  way, so an upload whose files lost `rawFile` fails at the item instead of posting nothing.
- **Every other path is optional.** This covers paths off a handler's payload, a response, a record,
  a module's data (`document.query`, `model.values`) or a returned object. A missing link reads as
  missing, and a call on a missing receiver returns missing. This is JavaScript's `?.`.
- **Writes.**
  - A `set` rooted at `obj(id)` needs every segment, the final member included, so a write to a
    member the vendor does not have fails loudly instead of creating a property.
  - Any other `set` needs the owner of its final member, and creates that member when absent, so a
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

A value changes type in exactly two places, and only by the declared shape:
- **Reading:** a `get` or `call` result is converted to the node's shape.
- **Writing:** a `set` value and each `call` argument are converted to the target member's shape.

Literals are emitted already in their shape. Nothing else converts.

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

These rules are `runtime/grammar/shape.ts`. Against the runtime the grammar replaced
(`shape-convert.ts`), a date value is a `Date`, not a timestamp. So text, JSON and form data see the local day and time picked, with no offset, as
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
| `truthy` | JavaScript's truthiness, applied after conversion. It is used by `Truthy()` and `Falsy()`. |
| `empty` | True for `""`, missing, and an empty array. Nothing is trimmed. It is used by `IsEmpty()` and `NotEmpty()` in conditions. |
| `matches(a, pattern, flags)` | False when `a` is missing; otherwise `new RegExp(pattern, flags).test(text(a))`. |
| `toNumber`, `toDate`, `toBoolean`, `toText` | Explicit C# casts. They use the 2.1 table. |
| `toArray` | The receiver of a LINQ method. Missing gives `[]`; text or a number gives `[v]`; an iterable gives `Array.from`; anything else is a boundary error. |
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
| `string.Contains`, `StartsWith`, `EndsWith` | `includes`, `startsWith`, `endsWith`; a missing receiver gives false |
| `string.Length`, `Trim()`, `ToUpper()`, `ToLower()` | `length`, `trim()`, `toUpperCase()`, `toLowerCase()` |
| `string.IsNullOrEmpty(s)` | `s == null \|\| s === ""` |
| `Regex.IsMatch(s, constant)` | `matches(s, pattern, flags)` |
| `DateTime.DayOfWeek`, `.Month`, `.Year`, `.Day`, `.Hour`, `.Minute`, `.Second` | `getDay()`, `getMonth() + 1`, `getFullYear()`, `getDate()`, `getHours()`, `getMinutes()`, `getSeconds()` |
| a declared element's `GetAttribute(n)`, `HasAttribute(n)` | `getAttribute(n)`, `hasAttribute(n)` |
| operators, `??`, `?:`, casts | the operators of 2.2 |

These are the members built at `e837ad94` (`Alis.Reactive/PlanAuthoring/Arrays/ReactiveArray.cs:33-149`,
and the allow-list and string operators of `Alis.Reactive/PlanAuthoring/Arrays/ElementExpressionCompiler.cs:21-26,
219-232`) plus those the validation `When` conditions need. That allow-list names JavaScript's
date methods directly (`getDay`, `getTime`) on any C# method of that name, and no view calls one;
here they are reached from `DateTime`'s own members. Members architecture.md once listed and
nothing uses yet (`First`, `Distinct`, `Skip`, `Take`, `SelectMany`, `Min`, `Max`, `Average`,
`ThenBy`, `Concat`) are added the same way, each with a journey that proves it.

## 3. Evaluation and execution

### 3.1 Bindings

A block runs in an environment of names:
- a handler binds its `param`;
- `let` binds a name for the rest of its block;
- `fn` binds its parameters for its body.

A `try` body's top-level bindings stay in scope after the `try`. Code after a `try` runs only when
the body completed, so each of them was bound.

A name is visible only where C# could see the variable that produced it. The builder enforces this
at render: a handle used outside the block that binds it throws, and names the handle.

### 3.2 Statements

| Statement | Behavior |
|---|---|
| `set` | Evaluates the target, converts the value to the target's shape, and writes it (1.3). |
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

**A sync value is read where it is used.** A `Value<T>` from a property or a returning method is a
read, like a property getter, so each use reads the current value. `var before = rate.Value;`
then `rate.Value.Set(5)` makes `before` read 5. To keep the value as it is now, bind it:
`var before = p.Let(rate.Value);` emits `let`, and `before` reads the binding. `p.Let` is specified,
not built: at `e837ad94` no builder has a `Let` member (`git grep -n -w Let -- Alis.Reactive` finds only
the grammar's internal `G.Let`, `Alis.Reactive/Grammar/G.cs:72`), so a value is always read where it is used.

**Plan order is C# evaluation order.** An async member emits its `let … await` where it is called,
in the active scope, and returns a value that names the result:
- the value is read, never invoked again, however often it is used;
- an async call written as an argument runs before the statement it feeds, as C# evaluates arguments
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
3. **Booted marks.** After the page starts, the root element gets `data-alis-booted="true"`,
   `window.__alisReactiveBoot` is set to `{ booted, planId }`, and the document raises `alis:booted`
   with `detail.planId` (`Alis.Reactive.Assets/runtime/root.ts:58-60`). The Playwright tests wait on these.
4. **Unloading a slot** runs its owner's disposers, and the boot-owned ones stay. A block that is
   already running finishes; nothing aborts it (`Alis.Reactive.Assets/runtime/modules/slot.ts:54-60`
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
| `t.ServerPush(ServerPushStream.At<S>(url), s => s.M, …)`, `t.SignalR(SignalRHub.At<H>(url), h => h.M, …)` | `on(stream, name, e)`, `on(hub, name, e)` |
| `var c = p.Component<C>(…); c.P.Set(v)`, a `Prop<T>` | `set(o, ["p"], shape, v)` |
| `c.M(a, b)`, a method declared with `Call` | `do(call(o, ["m"], [a, b]))` |
| `c.P`, a `ReadOnly<T>` or `Prop<T>` read | `get(o, ["p"], shape)` |
| `c.F(a)`, a method declared with `Invoke<T>` | `call(o, ["f"], [a], shape)` |
| a method declared with `CallAsync` or `InvokeAsync<T>` | `do await call(…)`; a read is `let t = await call(…)` where it is called, and the value is `ref t` |
| `args.A.Get(x => x.B)`, `json.Get(x => x.A.B)` | `get(v, ["a", "b"], shape)` |
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
| `p.ValidationErrors(container)` | in a response route: `do(call(form, ["showErrors"], [ref res.body]))`; elsewhere: `do(call(form, ["validate"]))` |
| `p.Parallel(a, b).OnAllSettled(c)` | `all(a, b); c` |
| `p.Url<TQuery>().Get(q => q.X)` | `get(get(document, ["query"]), ["X"], shape)`, keyed by the member's binding name; an array member reads `queryAll` |
| `tools.Count(items, "status")`, a member of a plugin from `plan.RegisterPlugin<T>()` | `call(plugin, ["count"], [..], shape)` |
| `Html.NativeActionLink(id, text, url, link => link.Post()…)` | the anchor plus its own plan script: `on(link, "click", e) { if (!modified(e)) { e.preventDefault(); … } }` |

### 5.1 A request

```js
if (form.validate()) {                            // .Validate<V>(container); outermost
  …WhileLoading…
  try {
    const req = record(…Gather…);                 // the request snapshot
    const res = await http.send({ method, url, route, values: req, headers, format });
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
`data-alis-raise="name"` and its detail as attributes, not an inline `onclick`. The document module raises the event when it
is clicked (6.1), and handlers hear it with `on(document, name)`.

## 6. The vocabulary: built-in objects and what each must do

Each built-in object is a plugin, declared in C# through the object contract like any component,
with its data type, and implemented once as a module of its package's bundle. The interpreter
imports none of them. The obligations are numbered so that tests and code can cite them.

### 6.1 `document` (`plugin("alis.document")`)

- **D1 `ready`.** An event of the owner being started. A `ready` handler runs when its own owner
  starts, after every handler of that owner has subscribed (3.5), and never again.
- **D2 `raise(name, detail)`.** Synchronous. It dispatches a non-bubbling `CustomEvent` with detail
  `{}` when none is given. A listener's payload is `detail`, or the event itself when the detail is
  missing (`Alis.Reactive.Assets/runtime/modules/document.ts:26-28, 93-96`).
- **D3 `query`.** The query string as a record, read the way MVC binds `[FromQuery]`:
  - keys match case-insensitively;
  - an array member reads every value;
  - a scalar member reads the first value;
  - an absent key reads as missing.
- **D4 Markup-raised events.** A click on an element carrying `data-alis-raise` raises that event
  with the element's detail. The listener is delegated, and it reads only attributes the framework
  stamped.

### 6.2 `http` (`plugin("alis.http")`)

`send({ method, url, route, values, headers, format })` is async and resolves to
`{ status, ok, body }`.

- **H1 Route.** Each `{name}` is filled from `route[name]` with `encodeURIComponent`. A missing
  route value throws before anything is sent.
- **H2 GET.** `values` become the query string, joined with `&` when the URL already has a `?`.
  - An array becomes repeated keys.
  - A missing scalar is sent empty (`name=`).
  - A missing array sends no key, because `name=` binds as a collection holding one null
    (`Alis.Reactive.Assets/runtime/modules/http.ts:112-128`).
  - A `File` cannot be sent in a GET; it is a boundary error.
- **H3 JSON.** `values` is the body. It is a binding read (2.1): dates serialize as their local text, and `""`
  is sent as `null` (`Alis.Reactive.Assets/runtime/modules/http.ts:168-178`). An empty record sends no body.
- **H4 Form data.** One entry per key.
  - Arrays repeat the key.
  - A `File` is appended with its name, and a date is its local text.
  - A missing scalar is sent empty.
  - A missing array sends no entry (`Alis.Reactive.Assets/runtime/modules/http.ts:136-153`).
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

`validate()` returns a bool; `showErrors(problem)`; tests come from the plan's `forms[containerId]`.

- **F1 Scope.** A test applies only when its field's element is inside the container element. A
  missing container fails validation when the form has tests.
- **F2 Model.** A test's `model` is `model.values` of the composed plan, read fresh at every
  evaluation.
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
    shows. The form never names a component's event, so the order in which the form and the
    input's plan start does not matter, and neither does the order the containers were declared.
- **F8 Server errors.** `showErrors(problem)` clears first.
  - When `problem` is a record with `errors`, it joins each field's messages with a space (each is a
    sentence) and matches a test field by its exact binding name. Anything else goes to the summary.
  - When `problem` is not a record (an empty or non-JSON body), it runs `validate()` instead
    (`Alis.Reactive.Assets/runtime/modules/form/form.ts:192-198`).
- **F9 Composition.** Tests from plans that share a `planId` are joined per owner. A slot's load or
  unload removes exactly its own tests and subscriptions, then clears and hides the summary of the
  plans it touched (`Alis.Reactive.Assets/runtime/modules/form/form.ts:273, 277-281`). Content rendered with the page belongs to the page even inside
  a slot's container: when a load replaces it, its tests stay, and its fields, no longer rendered,
  are checked as missing values (F5). A form whose steps replace each other in one slot therefore
  loads its first step into the slot too (the intake journey, `Validation/IntakeSteps`).

### 6.5 `slot` (`plugin("alis.slot", containerId)`)

`load(html)` is synchronous; `unload()`.

- **S1 Load order.** `load` works in this order:
  1. take the plan scripts out of the HTML;
  2. raise `alis:unmount` on the old content;
  3. replace the content;
  4. re-create the inserted `<script>` nodes in document order, so that vendor scripts run (this
     replaces `ej.base.append`);
  5. raise `alis:mount`;
  6. unload the previous owner;
  7. start a new owner for the slot with the plans it found (3.5).

  HTML with no plan only unloads.
- **S2 Unload.** `unload` releases the slot's owner (3.5).
- **S3 Element.** The container is found with `getElementById` at every call, so a slot can sit
  inside content that appears after boot.

### 6.6 `stream` (`plugin("alis.stream", url)`, realtime package)

- **R1 One connection.** One `EventSource` per URL, counted by subscriptions, and closed when the
  last one is released.
- **R2 Events.** A named event subscribes by its name, and unnamed messages arrive as `message`. The
  payload is `JSON.parse(data)`.
- **R3 Retry.** A permanent close adds a marker to `#alis-realtime-connection-retry-container`. One
  click retries every dropped connection, and the retried subscriptions keep their owners.

### 6.7 `hub` (`plugin("alis.hub", url)`, realtime package)

- **B1 One connection.** One `HubConnection` per URL, counted by subscriptions, with credentials and
  automatic reconnect.
- **B2 Start.** Starting retries at 0, 2, 10 and 30 seconds, then shows the retry indicator (R3).
- **B3 Messages.** Client methods are events, matched without regard to case. The payload is the
  method's single argument.
- **B4 Members.** `invoke(method, …args)` is async and returns a value; `send(method, …args)` is
  async; `state` and `connectionId` are properties; `reconnecting`, `reconnected` and `close` are
  events.

### 6.8 App objects (each its package's runtime module, element-located)

- **Confirm** (Fusion):
  - C1: `ask(message)` is async and resolves true on OK, and false on Cancel, close or Escape.
  - C2: Asks queue first in, first out.
  - C3: Escape inside the dialog never reaches the document.
  - C4: A missing dialog is a boundary error that names `FusionConfirmDialog`.
- **Loader** (Native):
  - L1: `show(target?, timeout?)` moves the loader into the target and starts the auto-hide timer.
  - L2: `hide()` cancels the timer and returns the loader to `<body>`, hidden. It also returns there
    when its target leaves the page.
- **Drawer** (Native):
  - W1: Closing empties its content element without unloading the slot loaded into it
    (`Alis.Reactive.Assets/components/native/drawer.ts:51-52`).
- **Toast** (Fusion): its declared members, and no obligation beyond them.
- **Native CheckList and RadioGroup:** the native module defines `value` and `isInteracted` on the
  element (`Alis.Reactive.Assets/components/native/check-list.ts`, `radio-group.ts`), replacing the
  inline view scripts of `195d3a1`.

### 6.9 Elements and vendor objects

- **E1 Lookup.** An element or vendor object is looked up with `getElementById` at each use and is
  never cached. Absent is a boundary error.
- **E2 Payload.** The vendor's `payload` decides what a handler's parameter is:
  - `argument`: the vendor's listener argument (Syncfusion; the native link, whose listener argument
    is the click's DOM `Event`, from which it reads the button, the modifier keys and
    `preventDefault`);
  - `element`: the element itself, for native inputs whose values are read after an `await`, or a
    `CustomEvent`'s `detail`.
- **E3 Writes.** A write converts by the member's shape: a date member receives a `Date`.

## 7. Validation projection

### 7.1 One statement per rule

`ReactiveValidator<T>` is the framework's own implementation of FluentValidation's grammar:
`RuleFor`, `RuleForEach`, `When`, `Unless`, `Otherwise`, `Include`, `SetValidator`, `ChildRules`,
the checks and `WithMessage`, written as FluentValidation users write them. The FluentValidation
package is not used. Each check is one term of the vocabulary and defines both halves where both
exist: what the server decides, and, for a term the browser decides the same way, the `ClientRule`
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
- A condition that reads anything but model members and fixed values (a method call, `.Length`).
- A rule on a computed value (`x => x.Name.Trim()`), which has no input.
- `.ServerOnly()`, which keeps the checks written before it on the server, for a member the page has
  no input for.

### 7.3 Conditions

- `When(predicate, rules)`, `Unless(...)` and `Otherwise(...)` around a block, and `.When(predicate)`
  and `.Unless(predicate)` after a rule's checks, take an expression. The server compiles it;
  `BrowserCondition` translates it into a `ClientCondition` when every part has a browser form:
  comparisons of members with fixed values, `string.IsNullOrEmpty`, `Contains`, `StartsWith`,
  `EndsWith`, `Regex.IsMatch`, membership in a fixed set, joined by `&&`, `||` and `!`.
- A condition after a rule's checks applies to every check written before it, as in
  FluentValidation.
- Blocks nest: a check's browser condition is the AND of its blocks' conditions and its own.
- A condition with no browser form keeps the checks under it on the server.

### 7.4 The vocabulary

| Term | Server | Browser check `(v, m)` |
|---|---|---|
| `NotNull` | `v != null` | `v != null` |
| `NotEmpty` | not null, blank text, an empty list or the type's default | by the property's type: text `v != null && v.trim() !== ""`; number `v != null && v !== 0`; collection `v != null && v.length > 0`; otherwise `v != null && v != default` |
| `Empty` | the negation of `NotEmpty` | the negation of the row above |
| `MinimumLength`, `MaximumLength`, `Length` | null passes; the length is within bounds | `v == null` or the length is within bounds |
| `EmailAddress` | null passes; exactly one `@`, neither first nor last | the same |
| `Url` | null or empty passes; an absolute `http` or `https` address | the same |
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
  index; the index is literal in the field ID and in every model path.

### 7.5 Which rules reach the browser

A rule reaches the browser when the page can evaluate it:
- its property is bound to an input on the page;
- every member its condition or peer reads is bound to an input or is a closure literal;
- nothing in it is the server's alone (7.2).

Every other rule stays server-only, with no extra code: `ServerFlag` has no input, and a collection
of complex items has none. The decision is made at render and is deterministic.

### 7.6 Grid column rules

The Fusion grid maps a validator's browser rules (`plan.ClientRules<TValidator>()`) to Syncfusion's
column rules, keeping only those EJ2 decides as the server does: `required`, `minLength`,
`maxLength`, numeric `min`, `max` and `range`, and one pattern per column. `EmailAddress()` and
`Url()` reach the cell as EJ2's `regex` with the patterns the form tests them with (`ClientRule.TextPattern`,
`Alis.Reactive/Validation/ClientRule.cs:52`),
because EJ2's own `email` and `url` rules decide differently. It reads the rules, not the plan.

## 8. Decisions that change what a user can see

Each change is deliberate and named here, so that nothing changes silently. The rows are in
`use-cases/`. A frozen fixture that fails on one of these comes to the owner; it is not edited.
"Today" in the Why column means the code at `195d3a1`, when this table was written; section 11 says
which decisions are applied.

| Change | Rows | Why |
|---|---|---|
| Empty values follow the server: `ExclusiveBetween` on a non-nullable number fails on empty; `GreaterThan(date)` on `DateTime?` passes on empty; `Equal`/`NotEqual` no longer skip empty; `When(x => x.Age < 18)` on `int` fires on empty | validation 22, 28, 33, 34, 37, 39, 48 | Today the browser accepts what the server rejects, or rejects what it accepts. Written once means one answer. |
| Email and credit-card checks follow the server's | validation rows using them | Same reason. Today the browser requires a dot that the server does not, and strips letters that the server rejects. |
| `NotEmpty` on a collection of complex items is server-only | validation 74 | It has no input. Today it always blocks, which is a defect. |
| `Include` enforces on the server as well | validation 76 | Today `ClientRulesFrom` is browser-only. |
| `AddReactiveClientValidation` is replaced by `ReactiveValidator` validators registered through DI | validation 77 | A browser rule always has a server rule behind it. |
| A URL query read (`p.Url<TQuery>()`) matches keys case-insensitively, and an array reads every value | conditions 14-16, 68, 138; http 26, 38, 42-50 | It reads the query the way MVC binds it. Today it is case-sensitive and reads the first value. This widens matching only. |
| A response handle reads the response it names | http (ambiguity 9) | Today a handle reads whichever response is current, so an outer handle inside a nested route reads the inner response. No view relies on that. |
| A link is an element with an ID the view chooses, and it carries its own plan script | components 117; http 113-121 | It replaces the delegated listener, the plan in `data-reactive-link`, and the runtime rewriting the request URL. The fixtures locate links by `data-testid`. |
| Template buttons raise events through `data-alis-raise` | components (ambiguity 9) | Views and templates carry no JavaScript. |
| A stream retry keeps each subscription's owner | http 105, 106 | Today a retried slot subscription is never released. |
| Enum literals are names | conditions (ambiguity 13) | Today `Eq(enum)` emits the number while the input reads the name, so it can never match. |
| The Pivot audit view sends the current view as two members, route and body | `Components/Fusion/PivotView/Index.cshtml:59-61` at `195d3a1`; at `e837ad94` the occupancy report's board packet sends the view in the path only and the layout in the body (`Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/PivotView/Index.cshtml:182-186`) | A placeholder member fills only the path. |
| `p.Request` is deleted | none | No builder exposed it (`Source.cs:73` had no caller); `Source.cs` was deleted in `6d573d39`, and no builder has a `Request` member at `e837ad94`. |
| A plugin method's date arguments arrive as `Date` objects | conditions 152 | Every plugin member is declared with typed arguments. Today an open-argument plugin call receives a date literal as a number (the old runtime's `evaluate.ts:46`, deleted in `9e66edea`). |
| Three fixtures that parse plan JSON or call deleted APIs are rewritten to assert behavior in step 0, before the assertions freeze | `WhenComplexArrayItemRulesApply.cs:76-82`, `WhenReactiveValidatorsDeclareClientRules.cs:166-185`, `WhenAGridColumnGeneratesNativeValidationFromClientRules.cs:17-25`, at `195d3a1`; the last two were deleted in `52c62881`, replaced by journeys | A test of plan shape is not a test of behavior. |

## 9. Coverage

All 531 use cases were lowered into this grammar at `195d3a1`. Each table gives, for every row, the
vocabulary used, the sentence, and a verdict:
[`use-cases/lowering-components.md`](use-cases/lowering-components.md),
[`use-cases/lowering-http.md`](use-cases/lowering-http.md),
[`use-cases/lowering-conditions.md`](use-cases/lowering-conditions.md),
[`use-cases/lowering-validation.md`](use-cases/lowering-validation.md).

| Area | Rows | Expressed | Needs a named obligation | Render only | Gap at first lowering | Gaps left |
|---|---|---|---|---|---|---|
| Components | 128 | 104 | 13 | 10 | 1 | 0: the uploader's value is an expression (1.1) |
| HTTP, URL, realtime, partials, links | 121 | 74 | 44 | 3 | 0 | 0 |
| Conditions, arrays, dispatch, elements, plugins | 165 | 37 | 124 | 3 | 1 | 0: declared object members inside lambdas (2.3) |
| Validation | 117 | 47 | 51 | 8 | 11 | 0: rows 68 and 115 resolved by 7.4 and F7; the other 9 are decisions in section 8 |
| **Total** | **531** | **262** | **232** | **24** | **13** | **0** |

- **"Needs a named obligation"** means the sentence is in the grammar, and the behavior depends on a
  rule section 2 or 6 now states exactly.
- **Where the obligations land.** Most of the conditions rows depend on the operator table (2.2),
  which states the comparison semantics of `195d3a1` instead of borrowing JavaScript's.
- **How the tables were made.** Each table was produced by one reader per area against the code, and
  spot-checked. They record the verdicts before this file's decisions; the last two columns above
  apply the decisions.
- **Review.** An independent reviewer checked this file against the code. The verdict was "accept
  with fixes": 23 findings raised, 12 confirmed and 11 dropped by the reviewer, each confirmed
  citation re-opened here. All 12 are applied:
  - paths split into member and data reads (1.3);
  - binding reads (2.1);
  - the wire rules of `195d3a1` for GET, JSON and form data (H2-H4);
  - `ValidationErrors` without a body validates (F8);
  - `ready` belongs to the owner being started (D1);
  - the complete member table (2.3);
  - modules as plugins, so the grammar names no feature (1.1);
  - peers read by path, and `Otherwise` (7.3, 7.4);
  - booted marks and summary clearing (3.5, F9);
  - sync values read at use, with `p.Let` (3.3);
  - the document-event payload and plugin date arguments (D2, section 8).

## 10. Where the code goes

**C#**
- `Alis.Reactive/Grammar`: one sealed, immutable class per production in 1.1 and 1.2, with internal
  constructors, invariants checked in the constructor, and one-to-one serialization. This is the
  plan contract, and the generator reads these classes.
- `Alis.Reactive/Objects`: the object contract and the built-in declarations.
  - `JsObject`, `Prop<T>`, `ReadOnly<T>`, `Value<T>`, `Arg<T>`, `Event`, `Event<T>`, `Message<T>`;
  - `Document`, `Http`, `Model<T>`, `Form`, `Slot`, `EventStream`, `HubConnection`, `HtmlElement`
    and the DOM event payloads.
- `Alis.Reactive/Authoring`:
  - the plan, the block stack and binding scopes;
  - the fluent API of section 5: handlers, conditions, requests, dispatch, parallel and URL.
- `Alis.Reactive/Compiler`: lambdas to `fn`, through the one member table (2.3).
- `Alis.Reactive.FluentValidator`: the projection (section 7) and its typed model.
- `Alis.Reactive.Fusion`, `Alis.Reactive.Native`: vendors, declarations, render helpers, Confirm,
  Toast, Drawer, Loader, and the grid's column rules.

**TypeScript runtime**
- `runtime/grammar`: `evaluate` (the nine expressions, the operators, the shapes), `execute` (the six
  statements, await) and `environment`. It imports nothing below.
- `runtime/objects`: the two locators, the vendor driver, the plugin modules registered by name, and
  owners.
- `runtime/plans`: discovery, composition, boot and mount events.
- `runtime/modules`: `alis.document`, `alis.http`, `alis.model`, `alis.form`, `alis.slot`. They are
  registered by the framework bundle, receive the evaluator, and do not import the executor.
- `runtime/realtime`: `alis.stream`, `alis.hub` and the retry indicator. The hub module ships the
  SignalR client inside the framework's bundle, so a page with a hub trigger needs no other script.
- `components/` (beside `runtime/`, in the same shipped bundle): the framework's own component code,
  built only on `alis:mount`, `alis:unmount` and `window.__alisPlugins`, as an app's is: CheckList,
  RadioGroup, Drawer, Loader, Confirm, SmartTextArea, SmartPasteButton, Mention.

The architecture test enforces the import directions, that the runtime names no vendor, and that
`components/` and `runtime/` import nothing from each other.

## 11. Implementation status

As built on `feature/component-redesign`. Playwright is the proof of each line; the CI run that
proves it is named in the pull request.

**Built as specified**
- The plan document, the two locators, the six statements and nine expressions (`Alis.Reactive/Grammar`),
  the generated contract (`tools/PlanContractGenerator`), and the interpreter with its modules
  (`Alis.Reactive.Assets/runtime`). At `e837ad94` the runtime is 2,495 lines of TypeScript (its tracked
  `.ts` files outside `__tests__` and the generated `types/`), down from 6,134, and the framework's own
  components add 548 in `components/`, which now also hold the uploader's `fusion/file-upload.ts` and the
  native `hidden-field.ts`.
- C# has one model: the builders place steps (`PlanModel/Reactions/Steps.cs`,
  `PlanModel/Requests/RequestStep.cs`) where they are written, and each step, value
  (`ValueExpression`) and condition (`ConditionGraph`) writes its own productions in the scope it is
  used in (`PlanModel/Writing/Scope.cs`). `PlanBuildContext.Write` writes the plan at render: the
  objects, the model's inputs, each form's rules (`FormWriter`) and the handlers. There is no
  separate semantic graph and no lowering pass.
- Validation projection (section 7): `Alis.Reactive.FluentValidator` implements FluentValidation's
  grammar itself and does not reference the FluentValidation package. Each check is a term with a
  server half (`Check<T, TValue>.Passes`) and, when the browser decides it the same way, a browser
  half (`IBrowserCheck.ForBrowser`) that returns the core's public `ClientRule`; the C# predicates
  `Must` and `MustAsync` have none. Conditions are expressions: the server compiles them and
  `BrowserCondition` translates them into `ClientCondition` when it can, for blocks and for a rule's
  own `.When`. `SetValidator`, `RuleForEach(...).SetValidator`, `Include` and `ChildRules` bring
  the child validator's rules under the enclosing conditions. `Matches` carries `IgnoreCase`,
  `Multiline` and `Singleline` as flags. A comparison's other member is read by its path from the
  model, wherever it is. The rules are read per request, from the validator that request resolves
  (7.1). A differential run checked the change: the 83 sandbox validators and six probes of every
  term, condition form and message placeholder, each on 1,500 generated models, gave the same browser
  rules and the same server messages as the FluentValidation-based engine it replaced. The
  differences were deliberate: a rule's own `.When` now reaches the browser; a browser message fills
  `{Name:format}` placeholders, and a check whose message is only about the entered value stays on
  the server instead of showing an empty or raw message; and the sandbox's wristband check, now
  composed from `Matches`, refuses an explicit `""` as `Matches` does (a cleared input is sent as
  null, so the page is unaffected).

- Promise-returning members (section 5): a method declared with `CallAsync` writes a call as
  `do await call(…)`, on a component, a plugin or an event's payload alike, and a read of one
  declared with `InvokeAsync<T>` as `let r = await call(…)` before the statement that reads it. A
  comparison that reads one is written as its own `if`, so else-if order and short-circuit hold. Inside an array
  operation's lambda or a validation rule such a read is refused at render.

- List paths: a member path may read each item of a list, `filesData[].rawFile`, written as
  `get(o, ["filesData", "[]", "rawFile"])`; the runtime reads the rest of the path on each item, in
  the path's own mode (1.3). The uploader declares its value that way (`FilesData.Each(file =>
  file.RawFile)` writes the path), so `alis.http` sends `File`
  objects and names no vendor. A list path can be read, not written, called or subscribed to, and
  reads at most one list.

**Where the build differs from the specification**
- `p.Let` (3.3) is not built: no builder has a `Let` member.
- Where the C# sits at `e837ad94`: the productions in `Alis.Reactive/Grammar`, the object contract in
  `Alis.Reactive/Components/Contracts`, the fluent API and the lambda compiler
  (`Arrays/ElementExpressionCompiler.cs`) in `Alis.Reactive/PlanAuthoring`, and the one model that writes
  the grammar in `Alis.Reactive/PlanModel`. Section 10's `Objects`, `Authoring` and `Compiler` folders
  were not made.
- `alis.stream` and `alis.hub` are registered by the framework's own bundle with the other modules
  (`Alis.Reactive.Assets/runtime/root.ts:32-38`), not by a separate realtime package as 1.1 and 6.6 to
  6.7 name it.

**Decisions of section 8 applied**
- `AddReactiveClientValidation` is gone: a browser rule always has the server rule it came from.
- `Include` enforces on the server as well.
- URL query keys (`p.Url<TQuery>()`) match case-insensitively, and an array member reads every
  value of its key (D3).
- A link is an element with a view-chosen id carrying its own plan script.
- Template buttons raise events through `data-alis-raise`.
- A stream retry keeps each subscription's owner.
- Enum literals in validation conditions are names.
- The three fixtures that parsed plan JSON were rewritten; so were the other fixtures that read the
  retired plan JSON: plan boots, behavior graph, gather input, capital members, grid, native action
  link, client rules (harness and two fixtures, the app-level one deleted with its API), array item
  rules, and the plan-JSON assertions of nine pages (AI assist view, date range picker, progress
  button, stepper, native checkbox, checklist, dropdown, radio group, array plugin). Each now reads
  the grammar plan; none asserts less than before except an overload key (`startAt(number)`,
  `addPromptResponse(string,boolean)`) and an access mode (`readwrite`) the grammar does not carry.
  Five client-rule tests were renamed from the retired `WhenField` vocabulary. Later, `52c62881` deleted
  the client-rule, behavior-graph, gather-input, link-request and capital-members fixtures, each row
  they stood in for now proven by a journey in a browser.

- Binding reads (2.1): request bodies and validation read each input as MVC binds it.
- Empty values follow the server: every check is the server rule's verdict on the bound value
  (7.4), so `ExclusiveBetween` on an empty non-nullable number fails (it is 0), `GreaterThan` on an
  empty nullable passes, and `Equal`/`NotEqual` compare an empty value instead of skipping it.
  `NotEmpty` on a collection reads as `atLeastOne`.
- Email and credit-card checks are the server's: one `@` with something on each side; dashes
  and spaces dropped, every other character a digit, then Luhn, with no minimum length.
- A response handle reads the response it names: each request's routes bind its handle, so a handle
  read inside a nested request's route reads its own request's response (`ResponseHandle`).
  `Into(...)`, which names no handle, injects the response of the request it follows.

