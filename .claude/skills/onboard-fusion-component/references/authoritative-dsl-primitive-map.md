# Authoritative DSL Primitive Map (Stage 0)

Status: **foundation**. This map must exist before any component onboarding runs.
Without it, an agent re-reads the frozen-core DSL per run, misreads it, and then
either invents a primitive or declares a false gap. The whole skill exists to
remove that re-interpretation. **Consult this map by lookup. Do not free-read the
DSL and improvise.**

Authoritative because every row cites the exact frozen-core symbol with
`file:line`, read from source on 2026-06-07 and cross-checked against real usage
(`ArrayGrid/Index.cshtml`, `FusionGridDataSourceExtensions.cs`,
`FusionStepperChangingArgs.cs`). The public DSL core is frozen (root `CLAUDE.md`
Rule 8); only the per-component typed surface grows. This file is committed with
the skill, so it survives session end — no component onboarding ever re-discovers
the core.

## How To Use This Map

1. Classify the traced JS member into a **lane** (property / method / event /
   event-payload / array), then a **shape** (scalar / object / array; void / args
   / returns).
2. Read the lane row. The primitive and its source anchor are the answer.
3. If the member has **no row here**, it is one of exactly two things, never a
   third:
   - a **new member shape** that earns a new source-cited row added to this map
     once, deliberately, by reading the frozen core again; or
   - a **genuine DSL gap** → a fail-closed matrix row + separate architecture
     work, surfaced for an explicit decision. **Never** patched inside an
     onboarding slice, never a public `string`/`object` escape hatch.
4. You may not conclude "the DSL cannot do this" without first locating the
   absence in [Frozen-Core Edges](#frozen-core-edges). If it is not listed there
   as a wall, the primitive exists — find its row.

## Classification Matrix (the deterministic decision table)

Every traced JS member resolves through this table to **exactly one** primitive.
The input space is **closed**: a JS object exposes only **properties, methods, and
events**; an event payload is itself an object whose members recurse into
properties and methods. These rows are therefore exhaustive over the taxonomy — a
member that matches none is a new shape (add a source-cited row) or a
[Frozen-Core Edge](#frozen-core-edges), never an improvisation.

| If the member is… | …then the primitive is | Lane |
|---|---|---|
| component property — read, scalar | `self.Read(ComponentProperty<T>.Named)` | 1 |
| component property — read, object/nested | `self.Read(ComponentProperty<T>.Mapped(name,"a.b"))` | 1 |
| component property — read, array | `self.Read(ComponentProperty<T[]>)` then `p.From(...)` | 1 → 5 |
| component property — write (any shape) | `self.EmitSet(ComponentProperty<T>, value)` | 1 |
| component method — void, 0–3 args | `self.EmitCall(ComponentMethod[.WithArgs<…>], args)` | 2 |
| component method — returns value | `self.Read<TReturn>(ComponentMethod, args)` | 2 |
| component event | `new TypedEvent<TArgs>(name,…)` + `Wire(...)` | 3 |
| payload member — read, scalar/object | `ReadPayload(PayloadSource.Event(), path)` via typed `args` | 4 |
| payload member — read, array | `p.From(args, e => e.Items)` / `ReadWholePayload(Event)` | 4 → 5 |
| payload member — write | `Set(PayloadSource.Event(), member, value)` | 4 |
| payload member — method, void | `Call(PayloadSource.Event(), method, args)` | 4 |
| array value — transform/aggregate | `p.From(source).Where/Select/OrderBy/Count/Any/Sum/Find/...` | 5 |

**Plugin is intentionally absent from this table.** It is a last-resort escape
hatch for operations the typed DSL genuinely cannot express — not a mapping
choice. Reaching for it means a real primitive was skipped. Never map a member to
plugin here; a genuine need is a [Frozen-Core Edge](#frozen-core-edges) decision,
surfaced explicitly.

## Two JS Object Roots

A component instance and an event payload are both JS objects, but they bind to
different runtime roots.

| JS object root | DSL source root | Anchor |
|---|---|---|
| Rendered component instance (`ej2.value`, `ej2.refresh()`) | `ComponentSource` via `ComponentRef<TComponent, TModel>` | `Source.cs:22,41` · `ComponentRef.cs:11` |
| Current event payload (`args.cancel`, `args.rowData`) | `PayloadSource.Event()` | `Source.cs:46,74` |

Other frozen-core roots a value read may target: HTTP `Success`/`Error`/`Request`
scopes, `Dispatch`, array `Element`, `Url`, direct `Dom`
(`Source.cs:78,82,86,90,94,97,121,135`).

**Reads are scope-parametric.** `ReadPayload`/`ReadWholePayload` are the *same*
mechanism across every `PayloadSource` scope — only the scope differs:
`Event` (component events), `Dispatch` (custom events — `t.CustomEvent[<TPayload>]`,
`TriggerBuilder.cs:38,50`, dispatched by `ReactionGraph.Dispatch`,
`ReactionGraph.cs:35-41`), `Success`/`Error` (HTTP response), `Request` (request
snapshot), `Element` (array element). Custom events are a **plan-trigger** lane,
not a component member, but they read payloads identically — proof that one value
path serves all scopes (root `CLAUDE.md`: "one domain concept reads all values").

## Lane 1 — Component Property

Declared with `ComponentProperty<TValue>`; `Named` when plan member == JS path,
`Mapped` when the JS path differs (`ComponentMember.cs:47,51`). Nested JS paths
are carried internally via `Mapped("planName","a.b")` — never expose path strings.

| JS shape | Primitive | Anchor |
|---|---|---|
| `ej2.prop` read (scalar) | `self.Read(ComponentProperty<T>.Named("prop"))` → `TypedComponentSource<T>` | `ComponentRef.cs:67` |
| `ej2.a.b` read (object/nested) | `self.Read(ComponentProperty<T>.Mapped("name","a.b"))` | `ComponentRef.cs:67` · `ComponentMember.cs:51` |
| `ej2.items` read (array) | `self.Read(ComponentProperty<TItem[]>.Named("items"))` → `TypedComponentSource<TItem[]>`; consume with `p.From(...)` (Lane 5) | `ComponentRef.cs:67` · witnesses: `FusionMultiSelectExtensions.cs:19` (`value: string[]`), `FusionGridDataSourceExtensions.cs:72` (`Data<TRow>()`) |
| `ej2.prop = v` write | `self.EmitSet(ComponentProperty<T>, ValueExpression)` → `SetReaction` | `ComponentRef.cs:31` · `ReactionGraph.cs:26` |
| `ej2.a.b = v` write (nested) | `EmitSet(ComponentProperty<T>.Mapped("name","a.b"), value)` | `ComponentRef.cs:31` · `ComponentMember.cs:51` |
| `ej2.items = [...]` write (array) | `EmitSet(prop, <array value source>)` — see [Array value-source scopes](#array-value-source-scopes) | `ComponentRef.cs:31` |
| write needs `ej2.dataBind()` | `EmitSet(...)` then `EmitCall(ComponentMethod.Named("dataBind"))` — only when the trace proves it | `ComponentRef.cs:31,44` |

## Lane 2 — Component Method

Declared with `ComponentMethod`; `Named`/`Mapped` for the JS path, `WithArgs<...>`
for typed arguments in JS order (`ComponentMember.cs:99,103,106`).

| JS shape | Primitive | Anchor |
|---|---|---|
| `ej2.m()` → void | `self.EmitCall(ComponentMethod.Named("m"))` → `CallReaction` | `ComponentRef.cs:44` · `ReactionGraph.cs:29` |
| `ej2.m(a)` → void | `ComponentMethod.Named("m").WithArgs<T1>()` + `EmitCall(method, args)` | `ComponentRef.cs:47` · `ComponentMember.cs:106` |
| `ej2.m(a,b)` → void | `WithArgs<T1,T2>()` + ordered `ValueExpression` list | `ComponentMember.cs:109` |
| `ej2.m(a,b,c)` → void | `WithArgs<T1,T2,T3>()` + ordered list | `ComponentMember.cs:112` |
| `ej2.m(...)` → value | `self.Read<TReturn>(ComponentMethod, args)` → `TypedComponentSource<TReturn>` | `ComponentRef.cs:86` |
| overloaded `ej2.m(...)` | distinct `ComponentMethod.Mapped("mForShape","m")` per shape (deterministic plan names) | `ComponentMember.cs:103` |
| object arg | `ValueExpression.Object(fields)` or `LiteralRaw(v, Shape.FromClrType(...))` | `ValueExpression.cs:112,44` |
| array arg | `ValueExpression.Array(items)` | `ValueExpression.cs:124` |

## Lane 3 — Component Event

| JS shape | Primitive | Anchor |
|---|---|---|
| `eventName: EmitType<TArgs>` | `new TypedEvent<TArgs>("eventName", new TArgs())` | `TypedEvent.cs:12` |
| wire event into plan | `ComponentEventOnboarding.Wire(plan, id, vendor, typedEvent, (args, p) => ...)` | `ComponentEventOnboarding.cs:13` |
| empty payload event | `TypedEvent<FusionXEmptyArgs>` | `TypedEvent.cs:12` |
| generic row-shaped payload | `TypedEvent<FusionXArgs<TRow>>` | `TypedEvent.cs:12` |

## Lane 4 — Event Payload Member

Scoped to the current event object via `PayloadSource.Event()` (`Source.cs:74`).
Payload members do **not** use `ComponentProperty`/`ComponentMethod`. Mutations and
method calls are authored as event-args **extension methods** that take the
pipeline explicitly (args carries no pipeline context):
`static void M(this FusionXArgs args, IReactionEmitter pipeline, ...)` →
`pipeline.AddStep(ReactionGraph.Set/Call(PayloadSource.Event(), ...))`
(`FusionAutoCompleteOnFiltering.cs:40,55`).

| JS shape | Primitive | Anchor |
|---|---|---|
| `args.prop` read (scalar) | `ReadPayload(PayloadSource.Event(), "prop", shape)`; public DSL via the typed `args` placeholder (`p.When(args, a => a.Prop)`, `g.FromEvent(args, a => a.Prop, "f")`) | `ValueExpression.cs:80` · `GatherBuilder.cs:55` |
| `args.a.b` read (object/nested) | `ReadPayload(PayloadSource.Event(), "a.b", shape)` | `ValueExpression.cs:80` |
| `args.items` read (array) | `p.From(args, a => a.Items)` for element ops, or `ReadWholePayload(PayloadSource.Event())` / `g.FromEvent(args, a => a.Items, "field")` for whole-array gather | `PipelineBuilder.Arrays.cs:31` · `ValueExpression.cs:86` |
| `args.prop = v` mutation | `ReactionGraph.Set(PayloadSource.Event(), "prop", value)` — value is **any** `ValueExpression`: literal (`Cancel()`/`PreventDefault()`) or cross-scope read (`Read(success.Scope, path)`) | `ReactionGraph.cs:26` · `FusionAutoCompleteOnFiltering.cs:44` |
| `args.method(...)` → void | `ReactionGraph.Call(PayloadSource.Event(), "method", args)` — each arg is **any** `ValueExpression`, incl. an HTTP response read fed into the payload method (`args.updateData(response.items)`) | `ReactionGraph.cs:29` · `FusionAutoCompleteOnFiltering.cs:63` |

**Cross-scope is the key fact.** Payload mutation values and payload-method
arguments are universal `ValueExpression`s from *any* live scope. A payload method
can be fed a value read from an HTTP success response
(`updateData(Read(success.Scope, path))`) because the event payload and the
response scope are both live inside `OnSuccess`. The value-argument lane is the
same everywhere (Lane 5), independent of the target.

**Open-shape payload fields.** A payload field whose runtime shape is genuinely
open (server JSON via a vendor adaptor) may stay public `object?` — e.g.
`FusionInPlaceEditorActionSuccessArgs.Data` (`object?`, "empty `{}` when no url").
This is a **judgement exposure** weighed by the rubric (a justified server-response
pass-through), not a frozen-core gap. Prefer a typed shape when the trace proves
one; justify any retained `object?` in the judgment-call artifact.

## Lane 5 — Array Transform (`ReactiveArray<TElement>`)

The **public authoring surface** for arrays. Entered with `p.From(...)`; operators
capture intent as `array-op` plan nodes (they do **not** run on the server, and
the type is deliberately not `IEnumerable`/`IQueryable`, so LINQ does not collide).
Operators lower to `ValueExpression.Array*` (`ValueExpression.cs:136-163`) — author
through `ReactiveArray`, not the raw nodes. **Never model an array as an indexed
path** (P003).

Enter the lane (`PipelineBuilder.Arrays.cs`):

| Array source | Entry | Anchor |
|---|---|---|
| any `TypedSource<T[]>` (component `.Data()`, response `.Read(sel)`, another `.AsSource()`) | `p.From<T>(TypedSource<T[]>)` | `PipelineBuilder.Arrays.cs:18` |
| event-payload array | `p.From<TPayload,T>(args, e => e.Items)` | `PipelineBuilder.Arrays.cs:31` |
| DOM array-like member | `p.FromDom([<T>]"id","member")` | `PipelineBuilder.Arrays.cs:50,58` |

Operators (`ReactiveArray.cs`):

| Op | Returns | Anchor |
|---|---|---|
| `.Where(x => pred)` | `ReactiveArray<T>` | `ReactiveArray.cs:28` |
| `.Select(x => proj)` | `ReactiveArray<TResult>` | `ReactiveArray.cs:34` |
| `.OrderBy / .OrderByDescending(x => scalarKey)` | `ReactiveArray<T>` | `ReactiveArray.cs:43,47` |
| `.Count() / .Count(pred)` | `ReactiveValue<int>` | `ReactiveArray.cs:70,74` |
| `.Any() / .Any(pred)` | `ReactiveValue<bool>` | `ReactiveArray.cs:78,82` |
| `.All(pred)` | `ReactiveValue<bool>` | `ReactiveArray.cs:86` |
| `.Sum(x => int/decimal/double)` | `ReactiveValue<int/decimal/double>` | `ReactiveArray.cs:90,94,98` |
| `.Find(pred) / .Find(pred, sel)` | `ReactiveValue<T> / ReactiveValue<TField>` | `ReactiveArray.cs:102,107` |
| `.AsSource()` | `TypedSource<T[]>` — rebind a transformed array into a component data source, no HTTP | `ReactiveArray.cs:121` |

Witness (`ArrayGrid/Index.cshtml:18,20,28,30`):
`p.From(json.Read(x => x.Residents)).OrderBy(x => x.Name).AsSource()` → grid
`SetDataSource`; and client-side
`p.From(p.Component<FusionGrid>("roster-grid").Data<…>()).Where(x => x.Status == "active").OrderBy(x => x.Name).AsSource()`.

### Array value-source scopes

An array write / data-source replace reads from one of four value scopes — pure
`ValueExpression` routing (this is what FusionGrid's four `SetDataSource` overloads
are, P013):

| Scope | Value primitive | Anchor |
|---|---|---|
| whole HTTP response `{ result, count }` | `ValueExpression.ReadWholePayload(success.Scope)` | `FusionGridDataSourceExtensions.cs:35` |
| response path | `ValueExpression.Read(success.Scope, responsePath)` | `FusionGridDataSourceExtensions.cs:20` |
| event payload path | `ValueExpression.Read(PayloadSource.Event(), eventPath)` | `FusionGridDataSourceExtensions.cs:45` |
| typed array source (incl. `ReactiveArray.AsSource()`) | `source.ToValueExpression()` | `FusionGridDataSourceExtensions.cs:60` |

**The `object` vendor sink is internal, not a public-surface violation.** The
data-source member is declared `private static readonly ComponentProperty<object>.Named("dataSource")`
because the vendor sink genuinely accepts array | `{ result, count }` |
DataManager. Public typing is recovered by the typed `SetDataSource(...)` overload
set — the typing lives in the overloads, not the member. This recurs identically
across Grid/MultiSelect/AutoComplete/DropDownList (`FusionGridDataSourceExtensions.cs:10`,
`FusionMultiSelectExtensions.cs:22`). Internal plan members may be `object`/string;
**public DSL must stay typed** (root `CLAUDE.md`).

## Reaction Verbs

`Set` · `Call` · `Request` · `Dispatch` · `Inject` · `ShowValidationErrors` ·
`Sequence` · `Parallel` · `Branch` — `ReactionGraph.cs:17-47`. Onboarding a
component member only ever emits `Set` (write/mutation) or `Call` (method); the
other verbs belong to HTTP, conditions, slots, and validation lanes.

## Consumer Entry Points (pointers, not walls)

An onboarded value source is a `TypedSource<T>` (`TypedSource.cs:9`) — produced as
`TypedComponentSource<T>` (property/method read, `TypedComponentSource.cs:6`),
`ReactiveValue<T>` (array op, `ReactiveValue.cs:13`), or `PayloadTypedSource`
(event/response read, `PayloadTypedSource.cs:11`). It flows into the existing,
heavily-exampled consumer lanes. **None of these limit onboarding** — if a value
seems to "not fit," it routes, it does not gap:

- **conditions** — `p.When(args, a => a.Prop)` / `When(body, r => r.X)` / `When(typedSource)` (`ConditionStart.cs:20,33,45`)
- **gather** — body via `Include`/`FromEvent`/`FromUrl`/`Static`; header & route accept any `TypedSource<T>` (`GatherBuilder.cs:55,87,152,290`)
- **element / dispatch** — `SetText`, `Dispatch("evt", value)` (`ReactionGraph.cs:35`)

A computed `ReactiveValue<T>` (e.g. a count) feeds `SetText`/`When`/dispatch
directly, and reaches a request **body** by carrying it through a named payload
field — a routing detail, not a wall. Path casing is camelCase, scope-relative
(`ExpressionPathHelper.cs:27,39`). Consumer grammar lives in the `conditions-dsl` /
`http-pipeline` skills; this map owns the member→primitive mapping.

## Frozen-Core Edges

The real walls. An agent may declare "no primitive" **only** by citing one of
these; otherwise the primitive exists and must be found above. A member at a wall
is **surfaced for an explicit decision** (separate DSL architecture work), never
faked.

| Wall | What the source proves | Anchor | Negotiable? |
|---|---|---|---|
| Component method typed args cap at **3** | `WithArgs` defined for `<T1>`, `<T1,T2>`, `<T1,T2,T3>` only | `ComponentMember.cs:106,109,112` | **Yes** — a real ≥4-arg method is *noted and surfaced*; a 4-arg overload may be approved as a deliberate core extension. Do not fake it. |
| `OrderBy` key must be a **scalar** | non-scalar key throws `InvalidOperationException` (object/collection keys serialize as `Any` → wrong runtime sort) | `ReactiveArray.cs:56-62` | No — project a scalar field (`.OrderBy(x => x.StartDate)`). |
| **No** event-payload method that *returns a value as a source* | `ValueExpression.Invoke` requires `RuntimeObjectSource`; `PayloadSource` is not one. Payload methods are void-only via `Call` | `ValueExpression.cs:99` · `Source.cs:16,46` | No — design core support separately if a component truly needs it. |
| Value-read method-invocation roots are `Component`/array `Element` only | `Invoke(RuntimeObjectSource,...)` + `InvokeElement(PayloadSource.Element(),...)` | `ValueExpression.cs:99,106` | No |
| **Function / callback** as a method arg or property value (predicate, comparer, formatter, accessor) | `ValueExpression` has no function node — kinds are `literal/read/object/array/array-op` only; a JS function cannot be built | `ValueExpression.cs:19-163` | → builder-owned template/config or plugin · recorded structurally |
| **Vendor class *instance*** with behavior (`new DataManager(...)`, `new Query().where(...)`) | `ValueExpression.Object` builds a *literal* JSON object, not a behavior-bearing instance. A plain config object **is** supported via `Object` | `ValueExpression.cs:112` | → builder-owned or plugin · recorded structurally |
| **Async / `Promise`-returning method** whose resolved value is needed as a source | `Read<T>(ComponentMethod)` lowers to a **sync** `Invoke`; component calls are not an async lane | `ValueExpression.cs:99,344` | → surface for explicit core decision · recorded structurally |

**Not a wall — method-return collections support nested field reads.** A method
returning a collection feeds `p.From(methodReturn).Select/Where/Find(x => x.Field)`;
array projection reads the nested field off each element. Witness:
`FusionScheduleExtensions.GetEvents() → FusionScheduleEventData[]` consumed via
`p.From(...)` (`FusionScheduleExtensions.cs:87`). Only a *single-object* return
needing a sub-field would be a gap; collection returns are covered.

**Every gap-hit is recorded structurally.** When onboarding meets a wall above, it
writes a structured deferred/edge row (`member · wall · reason`) into the component
matrix to revisit — fail-closed, never improvised, never silently dropped.

## Machine-Checkable Anchors

A use-enforcement guard resolves each symbol below against frozen-core source and
fails if any is missing or renamed. (The map is authored-once and trusted; this
list lets the guard confirm onboarding rows cite real primitives — see Q9.)

```
Alis.Reactive/Components/Contracts/ComponentRef.cs        EmitSet, EmitCall, Read
Alis.Reactive/Components/Contracts/ComponentMember.cs     ComponentProperty.Named/Mapped/WithShape, ComponentMethod.Named/Mapped/WithArgs
Alis.Reactive/PlanAuthoring/Events/TypedEvent.cs          TypedEvent`1 (ObjectEvent, Args)
Alis.Reactive/Components/Onboarding/ComponentEventOnboarding.cs  Wire
Alis.Reactive/PlanModel/Values/ValueExpression.cs         Literal*, LiteralRaw, Read, ReadPayload, ReadWholePayload, ReadWholeElement, Invoke, InvokeElement, Object, Array, Array{Count,Filter,Map,Sum,Any,All,Find,OrderBy}
Alis.Reactive/PlanAuthoring/Arrays/ReactiveArray.cs       ReactiveArray`1 Where/Select/OrderBy/OrderByDescending/Count/Any/All/Sum/Find/AsSource
Alis.Reactive/PlanAuthoring/Pipelines/PipelineBuilder.Arrays.cs  From, FromDom
Alis.Reactive/PlanModel/Reactions/ReactionGraph.cs        Set, Call, Request, Dispatch, Inject, ShowValidationErrors, Sequence, Parallel, Branch
Alis.Reactive/PlanModel/Values/Source.cs                  ComponentSource.Of, PayloadSource.{Event,Success,Error,Request,Dispatch,Local,Element}, UrlSource.Instance, DomSource.Of
Alis.Reactive/PlanAuthoring/Events/ResponseBody.cs        ResponseBody`1 (Scope, Read)
Alis.Reactive/PlanAuthoring/Conditions/TypedSource.cs     TypedSource`1 (ToValueExpression)
Alis.Reactive/PlanAuthoring/Conditions/TypedComponentSource.cs  TypedComponentSource`1 (FromMethod)
Alis.Reactive/PlanAuthoring/Conditions/PayloadTypedSource.cs  PayloadTypedSource`2 (FromEvent)
Alis.Reactive/PlanAuthoring/Arrays/ReactiveValue.cs       ReactiveValue`1
Alis.Reactive/PlanAuthoring/Pipelines/IReactionEmitter.cs  IReactionEmitter (AddStep)
Alis.Reactive/PlanAuthoring/ExpressionPaths/ExpressionPathHelper.cs  ToEventPath, ToResponsePath
Alis.Reactive/PlanAuthoring/Requests/GatherBuilder.cs     IncludeAll, Static, FromEvent, Header, RouteParam, FromUrl, Include
Alis.Reactive/PlanAuthoring/Conditions/ConditionStart.cs  When, Confirm
```
