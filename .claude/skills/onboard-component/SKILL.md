---
name: onboard-component
description: Declare a JavaScript component or plugin in C# so every framework feature works for it, as one slice: a declaration class that mirrors its typed API member for member, a render helper that starts it, and its own browser code beside them. Use when a component, a plugin, an object it returns or an event's payload is declared, when a declaration changes, and when a view needs a member no declaration has.
---

# Onboard a component

A component is a JavaScript object an element holds; a plugin is one the page provides by name, with no element.
Onboarding either is one slice: a declaration class that mirrors the object's typed API member for member, by
JavaScript name, a render helper for a component, and the object's own browser code beside them. Nothing else
changes: no runtime code, no registry, no edit to the core. A component that seems to need one means the
declaration is missing information, and the C# carries it. The framework's own components onboard the way an app's
do, on the same public surface, and the conformance library is the exemplar of every form below
(`tests/Alis.Reactive.Conformance.App/Components/`).

## Read the API before declaring

- The typed API is the contract: the vendor's .d.ts, or its source at the version the app pins. Declare what it
  declares public. A member it marks private or hidden, or one its docs name and its types lack, is a gap to raise
  with its use case, never a member to declare.
- Presence and nullability come from that API, on every object the declaration reaches, an event's payload
  included. A member the API declares is required: a page whose object lacks it stops the pipeline that reads,
  writes or calls it. A member it marks optional is `optional: true`; a value it types as text or null is
  `string?`; an object it types as possibly null or undefined is `nullable: true`.
- Sync or async is the declaration's, never what the browser finds: a method whose API returns a promise is
  `MethodAsync`. One declared with `Method` that returns a promise stops its pipeline.
- A member's JavaScript name is its C# name in camel case. When the names differ, pass `name:`.
- The build writes a table of the vendor's types from the project's own TypeScript before the compiler runs
  (`Alis.Reactive.Analyzers/Vendors/VendorContract.targets`). Until the rules that read it are built, reading the
  API is this skill's first step, done by hand.

## Find the object

The vendor says where the object lives and how a listener attaches; a declaration carries one vendor:

| The vendor | Declare |
|------------|---------|
| puts the object at a path on its element and listens through a subscribe and an unsubscribe method | `ComponentVendor.Define`, with the object's path and both method names |
| keeps the object off its element, found by a plugin the page provides | `ComponentVendor.Define` with `VendorLookup.ByPlugin` |
| holds each event in a property that keeps one listener | `ComponentVendor.DefineCallbackProperties` |
| removes a listener by the function its subscribe method returns | `ComponentVendor.DefineReturnedUnsubscriber` |
| is the element itself, raising DOM events | `ComponentVendor.Native` |

A vendor whose components an event hands over names `elementPath:`, where its object holds its element, so a
listen on such a component is released only while that element still holds it. A vendor's name is unique on a
page, and an element holds one component. A plugin derives from `Plugin`, named once with its JavaScript name, and
names its `EventListening` when its events are not an `EventTarget`'s.

## Map each member

| The API has | Declare |
|-------------|---------|
| a property read and written | `Property<T>` |
| a getter with no setter | `ReadOnly<T>` |
| a setter with no getter | `WriteOnly<T>` |
| an object with members of its own | `LiveObject<T>`, whose class derives from `JsObject` |
| a list of such objects | `LiveObjects<T>` |
| plain data, as JSON holds it | a class or record, its members read through `Read` |
| a method that returns nothing, or a value | `Method`, `Method<T>`: the result is what the call returned at its line |
| a method that returns an object with members | `ReturnedObject<T>`, `ReturnedObjects<T>` |
| a method that returns a promise | `MethodAsync`, `MethodAsync<T>` |
| an object that is itself a function | `CallItself` |
| an event | `Event`, or `Event<T>` when its listener receives a payload |
| a listener's or a callback's arguments by position | `Argument<T>` and `MethodAt`, on the payload's or the arguments' class |
| a function it calls for a value | `Callback<TArgs, TResult>` as a property, `Answer` as a method's argument |

A parameter is `Arg<T>`, so a view passes a literal or any source of `T`. `Fixed` passes a value the declaration
supplies, `Pass<TDeclared, TSource>` a source narrower than the declared type, and `AbortSignal()` the signal of the
block that calls it. Declarations as the conformance component writes them:

```csharp
public static ComponentVendor Declared { get; } = ComponentVendor.Define("conformance-component", objectPath: "conformanceComponent", subscribe: "addEventListener", unsubscribe: "removeEventListener");
public Property<string?> OptionalText => Property<string?>(optional: true);
public Outer NullableOuter => LiveObject<Outer>(nullable: true);
public MethodResult<string> Echo(Arg<string> text) => Method<string>([text]);
public Item Current() => ReturnedObject<Item>();
public MethodResult<int> SettleCount() => MethodAsync<int>();
public Callback<Item, string> Answer => Callback<Item, string>();
public void SortItems(Func<Item, TypedSource<int>> key) => Method([Answer(key)]);
public Event<Raised> Raised => Event<Raised>();
```

## Decide what the API leaves open

- An event or a callback: a vendor that ignores what its function returns raises an event; one that uses the
  return asks for an answer, a callback. A callback's answer is an expression the vendor gets at once, so a step or
  an awaited read written in it is refused at build, and an answer that stops throws to the vendor's call.
- A payload is a live object: declare it as the listener receives it, members and methods alike, as a class of its
  own beside its component, named for its event. A write to it reaches the vendor only before the handler's first
  awaited step; a write after one is refused.
- A callback property holds one listener, so an object and its event take one handler.
- Versions: declare the version the app pins. A component that ships several builds proves an older one stops at
  the member it lacks, as the conformance component's builds do.
- Slices: a declaration, its render helper and its code are self-contained; duplication between two slices beats a
  shared base class. In an app, an action that needs a model has a model of its own.

## Write the render helper

An extension method on `ReactivePlan<TModel>` builds the component's element with `TagBuilder` under an id the
developer chooses, writes the component's start into an attribute its code reads with `plan.Json(start)`, and
returns what `plan.Render<TComponent>(id, host)` gives; a component that is an input has a second helper on
`InputField<TModel, TValue>`, under the field's id (`tests/Alis.Reactive.Conformance.App/Components/ConformanceComponent/ConformanceComponentHtml.cs`).
The start is a class of its own, and each member its code reads by name carries `[JsonPropertyName]` with that name,
so the component starts the same under every app's JSON naming; a member a page leaves out is written absent, by
`JsonIgnore` with `WhenWritingNull`, never as a null the code would hold
(`tests/Alis.Reactive.Conformance.App/Components/ConformanceComponent/ConformanceStart.cs`).

## Write the component's code

The object's browser code sits beside its declaration, a TypeScript file named for it in kebab case
(`tests/Alis.Reactive.Conformance.App/Components/ConformancePeer/conformance-peer.ts`), held to the runtime's strict
compiler options and typed lint. It reaches the framework through two events and one queue, and nothing else:
- On `alis:mount`, whose detail names a root, it builds an object for each element below that root its marker
  attribute names, from the start that attribute holds, and puts the object where its vendor's path finds it. On
  `alis:unmount` it destroys each one below the root and takes it off its element
  (`tests/Alis.Reactive.Conformance.App/Components/component-view.ts`).
- A plugin's code pushes `{ name, instance }` onto `window.__alisPlugins`, the component library's through
  `tests/Alis.Reactive.Conformance.App/Components/plugin-queue.ts` and a page's own script beside its page
  (`tests/Alis.Reactive.Conformance.App/Bench/Sources/page-log.ts`); a name two scripts provide holds no plugin.
- A page links the component's script before the framework's, so every listener the code adds is in place at boot.

## Prove it

Each member a view uses has a Playwright test of both sides: the member working, and a page that does not match
the declaration (a member the object lacks stops, a null the API allows reads as null). A component shows what a
test reads by role: a status, a list, a heading, a button for what the vendor does in its own turn. The defect each
test names is planted through the prove-behavior skill.
