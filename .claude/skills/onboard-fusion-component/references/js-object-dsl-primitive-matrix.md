# JS Object DSL Primitive Matrix

Use this reference when a Syncfusion component exposes a JavaScript object API
and you need to decide which Alis.Reactive DSL primitive to use.

Core rule: every onboarded Syncfusion surface is a typed C# facade over a proven
JavaScript object member. The component instance is a JS object. The event
payload is also a JS object. Both are supported by the DSL, but they use
different runtime roots:

| JS Object Root | Runtime Root | Use For |
|---|---|---|
| Rendered component instance, for example `ej2.value` or `ej2.show()` | `ComponentSource` through `ComponentRef<TComponent, TModel>` | component property reads/writes, component method calls, component method return values |
| Current event payload, for example `args.text`, `args.cancel`, or `args.updateData(...)` | `PayloadSource.Event()` | event payload reads, event payload mutations, event payload method calls |

Do not expose arbitrary member strings in a public Fusion slice. Public Fusion
APIs stay typed even though internal plan members and JavaScript paths are
strings.

Component onboarding cannot change DSL primitives. If a member appears
unmappable, assume discovery or mapping is wrong first, re-read the current DSL
source, and record the unresolved row in the component audit report. Any real
primitive/helper gap is a separate plan/runtime design pass, not part of the
component slice.

## Grilled Matrix Checks

Before writing a component slice, answer every applicable row in the trace
matrix. If a row cannot be answered from Syncfusion source plus raw browser
proof, do not onboard it.

| Question | Accept Only When |
|---|---|
| Is this a component instance member or an event payload member? | The root object is explicit: component instance or `args` payload. |
| Is it initial render configuration? | Builder-only static options stay on the Syncfusion MVC builder. |
| Is it a post-render property read? | A reactive consumer needs it and raw proof shows the runtime value. |
| Is it a post-render property write? | Raw proof shows the write changes behavior; add a follow-up call such as `dataBind()` only when the trace proves it is required. |
| Is it a method call? | The argument list, order, sync behavior, and visible effect are proven. |
| Is it a method return source? | The return shape is proven and a real gather, condition, array, or component binding consumes it. |
| Is it an event? | The actual Syncfusion event name and payload type are known. |
| Is it a payload read? | A typed event args property path exists and a real consumer uses it. |
| Is it a payload mutation or call? | It runs inside the event lifecycle and changes visible/runtime behavior before Syncfusion inspects the payload. |
| Is it an array? | The element type is modeled and either a typed path/index or whole-array consumer is proven. |
| Is it overloaded or loosely typed? | Each supported shape gets a distinct typed C# API and deterministic plan member name. |
| Is it more than the current typed helper surface supports? | Stop and extend the internal DSL primitive deliberately; do not fake it with object bags or public strings. |

## Coverage Audit

This document covers these JS object combinations. If a new Syncfusion member
does not fit one of these rows, treat that as a design gap and stop before
adding public API.

| Surface | Covered Combinations |
|---|---|
| Component properties | scalar/object/array reads, scalar/object/array writes, nested paths, writes that require a follow-up method |
| Component methods | no args, one arg, two args, three args, four-plus args, void return, value return, overloads, object args, array args |
| Component events | empty payload events, typed payload events, generic row-shaped payloads, builder `.Reactive(...)` wiring |
| Event payload reads | scalar, nested object, indexed array element, whole typed array/list |
| Event payload writes | literal value, response-derived value, lifecycle-sensitive vendor flags |
| Event payload calls | no args, one arg, multiple args, visible lifecycle effect |
| Event payload consumers | conditions, HTTP body/header/route gather, plugin args, array transforms, DOM text/html helpers |
| Exclusions | builder-only configuration, hidden/internal members, unproven loose shapes, payload method return values, helper-surface gaps |

## Component Instance Members

| Proven JS Shape | DSL Primitive | Public Fusion API Shape | Notes |
|---|---|---|---|
| `ej2.prop` read | `ComponentProperty<T>.Named("prop")` + `self.Read(property)` | `TypedComponentSource<T> Prop<TModel>(this ComponentRef<FusionX, TModel> self)` | Use when the value feeds conditions, gather, array transforms, or another component. |
| `ej2.a.b` read | `ComponentProperty<T>.Mapped("planName", "a.b")` + `self.Read(property)` | Intent-named typed source, not `A_B()` | `planName` must be stable and distinct from other members. |
| `ej2.prop = value` | `ComponentProperty<T>` + `self.EmitSet(property, ValueExpression...)` | `ComponentRef<FusionX, TModel> SetProp(..., T value)` | Use literals, typed sources, response reads, or event reads as the value expression. |
| `ej2.a.b = value` | `ComponentProperty<T>.Mapped("planName", "a.b")` + `EmitSet` | Intent-named setter | Map nested JavaScript paths internally; do not expose path strings. |
| write requires `ej2.dataBind()` | `EmitSet(...)` then `EmitCall(ComponentMethod.Named("dataBind"))` | One public method that encapsulates both steps | Only add the follow-up call when browser proof shows the write does not apply otherwise. |
| `ej2.arrayProp` read | `ComponentProperty<TElement[]>.Named("arrayProp")` + `self.Read(property)` | `TypedComponentSource<TElement[]>` | Use `p.From(source)` for array operations. |
| `ej2.arrayProp = [...]` | `EmitSet` with `ValueExpression.Array(...)`, response read, event read, or `ReactiveArray<T>.AsSource()` | Typed setter such as `SetDataSource(...)` | Prefer domain row types over `object` when the shape is known. |
| `ej2.method()` returns `void` | `ComponentMethod.Named("method")` + `self.EmitCall(method)` | Command-style extension | Normal component commands are sync unless the JS behavior is inherently async. |
| `ej2.method(arg)` returns `void` | `ComponentMethod.Named("method").WithArgs<TArg>()` + `EmitCall(method, args)` | Typed method parameter | Use `ValueExpression.Literal(...)` for literals or reads for dynamic values. |
| `ej2.method(arg1, arg2)` returns `void` | `WithArgs<T1, T2>()` + ordered `ValueExpression` list | Typed parameters in JS order | Do not reorder for nicer C# names. |
| `ej2.method(arg1, arg2, arg3)` returns `void` | `WithArgs<T1, T2, T3>()` + ordered `ValueExpression` list | Typed parameters in JS order | Existing helper surface covers three arguments. |
| `ej2.method(arg1, arg2, arg3, ...)` | Stop component onboarding and prove whether the current DSL helper surface has the exact typed shape | Typed parameters in JS order only after separate DSL work closes | The current component helper surface covers up to three typed arguments. Four-plus support is plan/runtime design work, not an onboarding shortcut. |
| `ej2.method(...)` returns value | `ComponentMethod...` + `self.Read<TReturn>(method, args)` | `TypedComponentSource<TReturn>` | Prove the return shape and at least one real consumer. |
| overloaded `ej2.method(...)` | `ComponentMethod.Mapped("methodForShape", "method").WithArgs<...>()` per shape | Distinct C# overloads or method names | Plan member names must not collide when signatures differ. |
| method takes object arg | `ValueExpression.Object(...)` or `LiteralRaw(value, Shape.FromClrType(...))` | Domain-shaped argument type or explicit object builder | Confirm browser JSON casing and fields in rendered plan/probe. |
| method takes array arg | `ValueExpression.Array(...)`, literal typed collection, or source read | Typed collection or `TElement[]`/`List<TElement>` parameter | Keep element shape typed; avoid public `object[]` unless JS shape is genuinely open. |
| member exists only for static setup | No Fusion runtime primitive | Keep on Syncfusion MVC builder | Do not rewrap the builder as reactive DSL. |
| hidden/internal member | No public API | Exclude or record as deferred | Use only public JS APIs unless an explicit bridge is designed and proven. |

## Event Onboarding

| Proven JS Event Shape | DSL Primitive | Public Fusion API Shape | Notes |
|---|---|---|---|
| `eventName: EmitType<TArgs>` | `TypedEvent<TArgs>("eventName", new TArgs())` | property/method on `FusionXEvents` | Use the real Syncfusion event name, not a renamed channel. |
| Component builder wires event | `.Reactive(plan, evt => evt.EventName, (args, p) => ...)` + `ComponentEventOnboarding.Wire(...)` | `FusionXReactiveExtensions.Reactive(...)` | The builder overload locates the rendered component id and wires the event into the plan. |
| event has no useful payload | `TypedEvent<FusionXEmptyArgs>` | empty args type | Use when the event is meaningful but no payload members are consumed. |
| event payload is loosely typed in d.ts | Typed C# event args narrowed by raw proof | concrete `FusionXxxArgs` class | Loose Syncfusion types are evidence, not the Alis public contract. |
| same event has multiple generic row shapes | generic event selector method, e.g. `ActionBegin<TRow>()` | `TypedEvent<FusionXActionArgs<TRow>>` | Use when payload records carry application row shape. |

## Event Payload Members

Payload members are scoped to the current event object. They do not use
`ComponentProperty<T>` or `ComponentMethod`; those declare component-instance
contracts.

| Proven Payload Shape | DSL Primitive | Public Fusion API Shape | Notes |
|---|---|---|---|
| `args.prop` read | property on event args type + `PayloadTypedSource<TPayload, TProp>.FromEvent(...)` through public DSL | `args.Prop` in `.Reactive(...)` lambda | Public consumers use condition/gather/plugin/array overloads that accept the typed args placeholder. |
| `args.a.b` read | nested event args type + expression path | `args.Action.RequestType` | `ExpressionPathHelper.ToEventPath` lowers to runtime path such as `action.requestType`. |
| `args.items[0].name` read | `List<T>`/array property on args + constant index expression | `args.Items[0].Name` | Index paths are supported for constant indexes. |
| whole `args.items` read | `p.From(args, e => e.Items)` or `GatherBuilder.FromEvent(args, e => e.Items, "...")` | typed array/list property | Use for array transforms or request body gather. |
| `args.prop = literal` | `ReactionGraph.Set(PayloadSource.Event(), "prop", ValueExpression.Literal(...))` | event-args extension method such as `Cancel(p)` or `PreventDefault(p)` | Run inside the event pipeline before vendor code observes the value. |
| `args.prop = response.value` | `ReactionGraph.Set(PayloadSource.Event(), "prop", ValueExpression.Read(response.Scope, path))` | event-args extension accepting `ResponseBody<T>` and expression path | Use for event payload writes driven by HTTP response data. |
| `args.method()` | `ReactionGraph.Call(PayloadSource.Event(), "method", [])` | event-args extension method | Payload commands are not component commands. |
| `args.method(arg)` | `ReactionGraph.Call(PayloadSource.Event(), "method", [ValueExpression...])` | event-args extension with typed parameter/source | Use literals or value reads for arguments. |
| `args.method(arg1, arg2, ...)` | `ReactionGraph.Call(PayloadSource.Event(), "method", ordered ValueExpression list)` | event-args extension with typed parameters in JS order | The plan/runtime accepts argument lists; keep the C# extension typed. |
| `args.method(...)` returns value | No current public payload method-read primitive | Defer and design core support if a real component requires it | Do not pretend a command reaction can feed gather/conditions. |
| payload method mutates vendor UI lifecycle | `ReactionGraph.Call(PayloadSource.Event(), ...)` | event-args extension | Prove inside the real event lifecycle; late calls may not affect Syncfusion. |

## Event Payload Consumers

Use these public DSL consumers after the payload member exists on the typed args
class:

| Developer Intent | DSL Consumer |
|---|---|
| Branch on payload value | `p.When(args, a => a.Prop)...`, `guard.And(args, a => a.Prop)`, `branch.ElseIf(args, a => a.Prop)` |
| Send payload value in request body | `g.FromEvent(args, a => a.Prop, "field")` |
| Send payload value as header | `g.Header("Header-Name", args, a => a.Prop)` |
| Send payload value as route parameter | `g.RouteParam("id", args, a => a.Id)` |
| Pass payload value to plugin | `plugin.Arg(args, a => a.Prop)` |
| Transform payload array | `p.From(args, a => a.Items).Where(...).Select(...).AsSource()` |
| Display or set DOM text/html from payload | existing element helpers that read `PayloadSource.Event()` |

## Value Expressions For Arguments And Writes

| Value Source Needed | Primitive |
|---|---|
| literal scalar | `ValueExpression.Literal(...)` |
| literal object with known fields | `ValueExpression.Object(...)` |
| literal array | `ValueExpression.Array(...)` or `LiteralRaw` with a typed collection |
| HTTP response value | `ValueExpression.Read(response.Scope, ExpressionPathHelper.ToResponsePath(...), shape)` |
| event payload value | `ValueExpression.ReadPayload(PayloadSource.Event(), ExpressionPathHelper.ToEventPath(...), shape)` |
| component property or method return | `TypedComponentSource<T>.ToValueExpression()` through accepted public consumers |
| transformed array | `ReactiveArray<T>.AsSource()` |

## Stop Conditions

Stop onboarding that member and write a follow-up note instead when:

- the raw browser probe cannot prove the member, argument order, return shape, or visible effect;
- the member is already fully covered by the Syncfusion MVC builder as static initial render configuration;
- the d.ts type is broad but runtime proof does not narrow it;
- a payload method return is needed as a value source;
- a component method needs more typed arguments than the current helper surface exposes;
- the only working path requires hidden/internal Syncfusion members;
- the API would require public string member names, unbounded `object`, or an escape hatch that is not intentionally a plugin.

## Examples To Compare Against

- Component method return source and array source: `FusionScheduleExtensions.GetEvents`.
- Component overloaded/mapped method shapes: `FusionAIAssistViewExtensions.AddPromptResponse`.
- Payload property write and payload method call: `FusionAutoCompleteFilteringArgsExtensions`.
- Payload arrays and generic row payloads: `FusionKanbanEventArgs` and `FusionGridDataStateChangeArgs`.
