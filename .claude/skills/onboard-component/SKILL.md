---
name: onboard-component
description: Declare a JavaScript component in C# so every framework feature works for it, by mirroring its typed API member for member in one declaration class and one render helper. Use when a component, an object it returns or an event's payload is declared, when a declaration changes, and when a view needs a member no declaration has.
---

# Onboard a component

A component is a JavaScript object an element holds. Onboarding it is one class derived from `Component` that
mirrors the object's typed API member for member, by JavaScript name, and one render helper. Nothing else
changes: no runtime code, no registry, no edit to the core. A component that seems to need one means the
declaration is missing information, and the C# carries it. The framework's own components onboard the way an
app's do, on the same public surface.

## Read the API before declaring

- The typed API is the contract: the vendor's .d.ts, or its source at the version the app pins. Declare what it
  declares public. A member it marks private, or one its docs name and its types lack, is a gap to raise with its
  use case, never a member to declare.
- Presence and nullability come from that API, on every object the declaration reaches, an event's payload
  included. A member the API declares is required: a page whose object lacks it stops the pipeline that reads,
  writes or calls it. A value the API types as text or null is `string?`.
- Sync or async is the declaration's, never what the browser finds: a method whose API returns a promise is
  `MethodAsync`. One declared with `Method` that returns a promise stops its pipeline.
- A member's JavaScript name is its C# name in camel case. When the names differ, pass `name:`, as
  `SetRateInMillilitresPerHour` passes "setRate".

## Find the object

The vendor says where the object lives on its element and how a listener attaches:

| The vendor | Declare |
|------------|---------|
| listens through a subscribe and an unsubscribe method | `ComponentVendor.Define`, with the object's path on the element and both method names |
| holds each event in a property that keeps one listener | `ComponentVendor.DefineCallbackProperties` |
| removes a listener by the function its subscribe method returns | `ComponentVendor.DefineReturnedUnsubscriber` |
| is the element itself, raising DOM events | `ComponentVendor.Native` |

A vendor's name is unique on a page, and an element holds one component.

## Map each member

| The API has | Declare |
|-------------|---------|
| a property read and written | `Property<T>` |
| a getter with no setter | `ReadOnly<T>` |
| a setter with no getter | `WriteOnly<T>` |
| an object with members of its own | `LiveObject<T>`, whose class derives from `JsObject` |
| a list of such objects | `LiveObjects<T>` |
| plain data, as JSON holds it | a record, its fields read through `Read` |
| a method that returns nothing | `Method` |
| a method that returns a value | `Method<T>`: the result is what the call returned at its line |
| a method that returns an object with members | `ReturnedObject<T>`, `ReturnedObjects<T>` |
| a method that returns a promise | `MethodAsync`, `MethodAsync<T>` |
| an event | `Event`, or `Event<T>` when its listener receives a payload |
| a listener's or a callback's arguments by position | `Argument<T>` and `MethodAt`, on the payload's or the arguments' class |
| a function it calls for a value | `Callback<TArgs, TResult>` as a property, `Answer` as a method's argument |

A parameter is `Arg<T>`, so a view passes a literal or any source of `T`. `Fixed` passes a value the declaration
supplies, and `Pass<TDeclared, TSource>` passes a source narrower than the declared type. Declarations as the
reference components write them:

```csharp
public void SetRateInMillilitresPerHour(Arg<decimal> volume) => Method([volume, Fixed("ml/h")], name: "setRate");
public MethodResult<int> Volume() => MethodAsync<int>();
public PumpChannel Channel(Arg<int> position) => ReturnedObject<PumpChannel>([position]);
public Event<MonitorAlarm> Alarm => Event<MonitorAlarm>();
public AlarmPatient Patient => LiveObject<AlarmPatient>();
public ReadOnly<string> Query => Argument<string>(0);
public Callback<ChartTick, string> TickLabel => Callback<ChartTick, string>();
public void SortTicks(Func<ChartTick, TypedSource<int>> key) => Method([Answer(key)]);
public ReadOnly<TickData> Tick => Argument<TickData>(1);
```

## Decide what the API leaves open

- An event or a callback: a vendor that ignores what its function returns raises an event; one that uses the
  return asks for an answer, a callback. A callback's answer is an expression the vendor gets at once, so a step or
  an awaited read written in it is refused where the view renders, and an answer that stops throws to the vendor's
  call.
- A payload is a live object: declare it as the listener receives it, members and methods alike. A write to it
  reaches the vendor only before the handler's first awaited step; a write after one is refused.
- A callback property holds one listener, so an object and its event take one handler.
- Versions: declare the version the app pins. A reference component that ships two builds proves the older one
  stops at the member it lacks, as `OlderInfusionPump` does.
- Slices: a declaration and its render helper are self-contained; duplication between two declarations beats a
  shared base class. In an app, an action that needs a model has a model of its own.

## Write the render helper

An extension method on `ReactivePlan<TModel>` builds the element with `TagBuilder` and returns the `Rendered<T>`
that `Render` gives, under an id the developer chooses. The vendor's own browser code creates the object on its
element, as `tests/Alis.Reactive.Poc.App/Components/components.ts` does for the reference components; the
framework finds the object by the vendor's path and never creates it.

## Prove it

Each member a view uses has a Playwright test of both sides: the member working, and a page that does not match
the declaration (a member the object lacks stops, a null the API allows reads as null). A reference component is
plain DOM with its own buttons for what the vendor does in its own turn. The defect each test names is planted
through the prove-behavior skill.

The check that reads the vendor's types at build and refuses a declaration that drifts from them is the vendor
contract file (POC 13); until it lands, reading the API is this skill's first step, done by hand.
