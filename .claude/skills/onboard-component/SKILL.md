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

- Start with the vendor's declarations and source at the version the app pins. Enumerate public instance
  properties, methods, overloads and events, including inherited members. Follow each event argument and returned
  object to the members a view reaches. Keep a verdict with the spec for each uncovered member: a typed
  declaration and proof, evidence still needed, or an exclusion with its reason. Vendor members marked private
  or hidden, and names found only in docs, need a use case raised to the owner.
- A weak type such as `Object`, `any` or a loose union needs evidence from the installed vendor. Start with the
  code that constructs or consumes the value. Run the applicable real gesture on the pinned version and record
  the member's exact name, shape and presence in each lane the declaration covers. Use a rendered page on
  an HTTP origin and make unexpected browser errors fail the probe. Observe absent, null and
  falsy values distinctly. An object row and its selected DOM element have different contracts.
- Record presence and nullability separately on every object the declaration reaches. Read a vendor
  member's documented `@default null` even when its type declaration omits null. Include null in the
  declaration and verify the initial value on a rendered control. Read the code that constructs an event
  payload and observe the relevant gestures on the pinned build. An always-emitted member that holds null is
  required and nullable. For example, ComboBox emits both row keys, and clearing a selection gives a null
  current row. Use the nullable data type for plain data and `nullable: true` for a live holder. Reserve
  `optional: true` for a member that can be absent.
- Read the return type of the actual public call. A method that starts asynchronous work and returns void
  uses `Method`. A call that returns a Promise uses `MethodAsync`, and a view can read its resolved data through
  the typed result. Follow a live object to its own public methods. ComboBox's spinner methods return void.
  Its configured EJ2 Query's execute method returns the DataManager's Promise.
- A member's JavaScript name is its C# name in camel case. When the names differ, pass `name:`.
- Check whether the project imports `Alis.Reactive.Analyzers/Vendors/VendorContract.targets` and supplies a
  vendor TypeScript program on each target. A configured build checks declarations it sees. Compare the vendor
  list against the C# declarations as well to find omitted doors. For Fusion, follow the per-slice inventory in
  `spec/FusionComponents/audit/README.md`, then read the recorded vendor source for the slice's rows.
- Separate the render helper's initial options from the live object exposed after it renders. For a live
  union, decide reads and writes independently. An array write to ComboBox's inherited dataSource has a
  typed list lane. A list read needs evidence for the active data source because the same property can hold
  a DataManager. Record and prove each exposed read or write lane, including cases that share a vendor name.
- Onboard DataManager only for the component that needs it. Keep its live declaration, query and result
  holders with that component's slice. Verify the configured source, query argument and adaptor envelope.
  A live browser object passed to a method needs a typed source that carries the same instance. A C# live
  declaration describes the object and its member paths. Read the supported value as a source for an
  argument, and prove the actual Query reaches the vendor call before adding the door.

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
- Declare a live payload as the listener receives it, including its methods. Each Fusion event owns its nested
  payload types inside its component's declaration. A vendor interface shared by two controls gives each control
  a starting point for inspection. Check the actual gesture and payload separately for each. A write reaches the
  vendor only before the handler's first awaited step; a write after one is refused.
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

Use a real render and gesture to exercise the typed read, write or call and assert its visible outcome.
Cover absence or null as the contract permits. For a loose vendor type, keep the probe that established the
shape and the Playwright test that uses the resulting C# door. Rerun both on a vendor upgrade. For a payload
method, exercise the supported argument forms and observe the vendor effect. Plant the defect each test names
through the prove-behavior skill.

## How does this look in a real Fusion slice?

Use the pinned ComboBox examples as a cross-check after reading the vendor source:

- Four value shapes, priority branches, a current row, a previous row and the selected list element.
  The same page separately handles focus, blur and server filtering. While the lookup is active it shows
  the vendor spinner. Apply the typed result through the event's updateData method and hide the spinner when
  the active lookup settles. The returned list and its selected row give independent observable outcomes.
  `tests/Alis.Reactive.Conformance.App/Care/AdmissionClinical/CommunicationPreferences.cshtml`
  `tests/Alis.Reactive.Conformance.PlaywrightTests/FusionComponents/FusionComboBox/WhenHelenChoosesHerCommunicationPreferences.cs`
  `tests/Alis.Reactive.Conformance.PlaywrightTests/FusionComponents/FusionComboBox/WhenHelenSearchesForAnInterpreter.cs`
- A separate box writes a typed catalogue and nested label and grouping settings, binds the change and
  shows the actual grouped options. Clearing it changes what the form saves.
  `tests/Alis.Reactive.Conformance.App/Care/AdmissionClinical/TransportProviders.cshtml`
  `tests/Alis.Reactive.Conformance.PlaywrightTests/FusionComponents/FusionComboBox/WhenStaffArrangeResidentTransport.cs`
- A separate box runs its actual configured Query. The typed handler waits for the vendor's Promise,
  filters the returned rows and reads a nested dispatch instruction. The test holds the endpoint before
  releasing it. Its planted synchronous call and wrong row selection fail at the visible result.
  `tests/Alis.Reactive.Conformance.App/Care/AdmissionClinical/EquipmentAvailability.cshtml`
  `tests/Alis.Reactive.Conformance.PlaywrightTests/FusionComponents/FusionComboBox/WhenTransportEquipmentIsRequested.cs`

- A manager-backed box tests the actual DataManager call with a typed live Query argument. The provider
  receives the selected supplier's query and returns its delivery rows. The handler waits for the result,
  reads the bookable row's dispatch instruction and shows it on the page. The local array write and the live
  manager read each have a separate declaration on this one component.
  `tests/Alis.Reactive.Conformance.App/Care/AdmissionClinical/SupplierDelivery.cshtml`
  `tests/Alis.Reactive.Conformance.PlaywrightTests/FusionComponents/FusionComboBox/WhenASupplierDeliveryIsChecked.cs`

Replay the ComboBox declaration checks from `spec/FusionComponents/audit/README.md`. The baseline and current
checks validate different facts. Use the current inventory when comparing an onboarded slice.
