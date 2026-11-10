# Vendors

How a component's object is found, heard and handed to its own browser code: a vendor, the data a component's
declaration carries in closed sets the one driver follows; a component's declaration base, its render and the typed
reach to it; and `alis:mount` and `alis:unmount`, the two events through which a component's own code builds and
destroys it. Module 12 of `spec/CLAUDE.md`.

## Core questions

- Q17: the dependency is inverted, so a component an app brings is onboarded as a shipped one is (D17.1); a component
  is one declaration and a render helper (D17.2); a vendor is data in closed sets: where the object lives, a path from
  the element or a lookup (N17.6), how a listener subscribes, a method pair, a callback property or a returned
  unsubscriber (N17.7), and where the object holds its element (D17.3); a listener's arguments and the payload's place
  in them (D17.4, N17.1, N17.8); the vendor contract (D17.5, N17.2); a vendor's name by its characters (D17.7); a
  callback the vendor calls for an answer (D17.12, N17.9).
- Q12: a component in content constructs on `alis:mount` and destroys itself on `alis:unmount` (D12.3); a listener is
  removed only while its element still holds the object it was added to (D12.5); a `listen` on an object a pipeline
  reaches (D12.8, N12.1).
- Q4: a member a live object lacks stops the pipeline (D4.1), and a stop never reaches the vendor (D4.8).
- Q5: a component is a live object (D5.1), and its events' payloads are declared with the whole vocabulary (D5.6).
- Q18: ALIS034 checks a constant vendor name (N18.12), and ALIS032 a callback's answer (N18.9).

## Boundaries

A declaration's members, its events and their payloads, and data are module 11's; how an event is heard, the listening
kinds and the payload's place, and the defects of Q17 are this module's. The bridge is this module's: `alis:mount`,
`alis:unmount` and the page's plugin queue, through which a component's code provides a plugin, read at boot before
`alis:mount` (D17.14). A plugin's declaration, its members and its lookup by name (D17.14, N17.11) are module 13's, with
X17.24 to X17.27, X17.25's late push among them; a vendor's lookup is a plugin found as module 13 finds one. What an
owner holds and releases is module 18's, X12.15 and X12.17 among it; the driver's side of a release, the check that the
element still holds the object (X12.7, X12.13) and a listener marked released before its removal (X12.16), is this
module's. NativeConfirm and NativeToast (N17.4, X17.6 to X17.9) and the row actions (D17.15, N17.12, X17.31 to X17.38)
are module 25's; Syncfusion's vendor and the native components are modules 21 and 24. The analyzers' code (N17.2, N18.9,
N18.12) is module 20's, and the rules they hold a vendor to are this module's defects. Boot's order is module 17's, the
schema's `vendors` map module 14's with X19.4, and a render helper's starting state (D17.13, N17.10) module 7's.

## Doors

Each name is gate 2's; each door is the capability it carries.

Kept from the core (Q17): `ComponentVendor.Define`, `ComponentVendor.DefineCallbackProperties`,
`ComponentVendor.Native` and `ComponentVendor.Name`, a vendor written as text reading as its name; the lookup (N17.6),
the returned unsubscriber (N17.7), the payload's place (N17.1), `Argument<T>(n)` and `MethodAt(n, ..)` (N17.8),
`Callback<TArgs, TResult>` and `Answer` (N17.9), the vendor contract (N17.2) and `listen` (N12.1). The redesign's
`VendorEventPayload`, a payload kind on the vendor, leaves with D17.4: the payload's place is each handler's.

- `M12.N1` kept: `Component`, the base of a component's declaration, a live object whose listening is its vendor's
  (D12.8), and `IComponent.Vendor`, the `ComponentVendor` a declaration carries (D17.3). It replaces main's
  `IComponent.Vendor`, a string main's runtime picks a driver by
  (`Alis.Reactive.Assets/runtime/browser-objects/component-driver.ts:89-90` at 623dd4a27). The POC's form:
  `poc/Alis.Reactive.Poc/Components/Component.cs:8-12`.
- `M12.N2` kept: `pipeline.Component<TComponent>(id)`, the component on the element of that id, typed by its
  declaration, whose members a step reads, writes and calls as any live object's (D5.1). Main's
  `Component<TComponent>(refId)` is this door with a `ComponentRef` as its result, which writes and calls only through
  internal members (`Alis.Reactive/Components/Contracts/ComponentRef.cs:31-65` at 623dd4a27). The POC's form:
  `poc/Alis.Reactive.Poc/Pipelines/Pipeline.cs:20`.
- `M12.N3` kept: `plan.Render<TComponent>(id, markup)`, which writes the markup a render helper gives, and its
  `Reactive(event, pipeline)`, a handler of a declared event from the plan's start, which names the element as that
  declaration's component in the plan, as a reach does (M12.X2). The POC's form:
  `poc/Alis.Reactive.Poc/Hosting/ReactivePlan.cs:35-37`, `poc/Alis.Reactive.Poc/Hosting/ReactivePlan.cs:79-85`.

The meanings `M12.D1` and `M12.D2` are in `spec/Vendors/features.json`, on the features they give.

## Improvements

- One driver follows a vendor's data (D17.1, D17.3). Main's runtime holds a driver per vendor name, native and fusion,
  and reads Syncfusion's object by name (`Alis.Reactive.Assets/runtime/browser-objects/component-driver.ts:79-90` at
  623dd4a27), so a component an app brings has no driver (X17.4). The redesign writes a vendor's path and listening
  into the plan (`Alis.Reactive/Vendors/ComponentVendor.cs:123` at 65fdf118d), and one driver follows them
  (`Alis.Reactive.Assets/runtime/objects/vendor-driver.ts:20-34` at 65fdf118d).
- A release asks the element, not the vendor (D12.5). Main asks Syncfusion's `isDestroyed` before it removes a
  listener (`Alis.Reactive.Assets/runtime/events/event-fusion.ts:34` at 623dd4a27); the redesign removes one only while
  the element still holds the object (`Alis.Reactive.Assets/runtime/objects/plan-objects.ts:41`,
  `Alis.Reactive.Assets/runtime/objects/vendor-driver.ts:16-18` at 65fdf118d), the rule the rewrite keeps.
- Three listening kinds and a lookup (N17.6, N17.7). The redesign has a method pair and a callback property
  (`Alis.Reactive.Assets/runtime/objects/vendor-driver.ts:28-33` at 65fdf118d) and finds an object only at a path from
  its element (`Alis.Reactive.Assets/runtime/objects/vendor-driver.ts:10-14` at 65fdf118d); the POC adds the returned
  unsubscriber and the lookup (`poc/Alis.Reactive.Poc/Components/Component.cs:48-58`).
- The payload's place is the handler's (D17.4, D19.6, N17.1). The redesign carries a payload kind on the vendor and
  hands a CustomEvent's listener its detail, or the event when the detail is missing, and any other event's listener
  its element (`Alis.Reactive.Assets/runtime/objects/vendor-driver.ts:81-92` at 65fdf118d), so one event raised with a
  detail and without one reaches its handler as two shapes (X17.3).
- A throw from a component's own callback skips no listener (D17.3, X17.49). The redesign calls the value it replaced
  before its listener, unguarded (`Alis.Reactive.Assets/runtime/objects/vendor-driver.ts:59` at 65fdf118d), so that
  throw skips every listener after it.
- A listener whose removal threw runs nothing (D12.5, X12.16), a plugin's too, since a plugin's events listen by a
  vendor's listening kinds (D17.14). The redesign's method pair release is the unsubscribe call alone, with no mark
  (`Alis.Reactive.Assets/runtime/objects/vendor-driver.ts:44-46` at 65fdf118d), and a plugin subscribes through its
  own code (`Alis.Reactive.Assets/runtime/objects/plan-objects.ts:60-62` at 65fdf118d).
- A vendor's name is checked by its characters with no regular expression (D17.7). The redesign matches a regular
  expression (`Alis.Reactive/Vendors/ComponentVendor.cs:33-34` at 65fdf118d).
- A vendor name names one vendor (M12.D1, M12.X1). The redesign refuses two in one plan where the view renders
  (`Alis.Reactive/PlanModel/Writing/Scope.cs:270` at 65fdf118d); no core line states it.
- A plugin the page's code provides after boot registers at once (D17.14). The redesign deletes the queue at boot
  (`Alis.Reactive.Assets/runtime/root.ts:44-45` at 65fdf118d), so a later push is dropped with no word.
- A handler whose element holds no object when its plan starts does not start and stops no other (M12.D2, M12.X4).
  The redesign throws where it finds the object (`Alis.Reactive.Assets/runtime/objects/vendor-driver.ts:13`,
  `Alis.Reactive.Assets/runtime/objects/plan-objects.ts:55` at 65fdf118d), and its Unbuilt page shows another handler
  still listening (`tests/Alis.Reactive.Framework.PlaywrightTests/Vendors/WhenAComponentIsNotBuiltYet.cs:9-16` at
  65fdf118d); no core line states it.
- An element holds one component in a plan (M12.X2). The redesign refuses one named by two vendors where the view
  renders, at a reach and at a `Reactive` alike (`Alis.Reactive/PlanModel/Document/PlanBuildContext.cs:120-136`,
  `Alis.Reactive/Components/ObjectBinding.cs:61`, `Alis.Reactive/PlanAuthoring/RenderedComponent.cs:94` at
  65fdf118d); no core line states it.
- A reach is typed by its declaration (M12.N2). Main's `ComponentRef` writes a property and calls a method by names
  as strings through internal members (`Alis.Reactive/Components/Contracts/ComponentRef.cs:31-65` at 623dd4a27), so
  only the framework's packages declare a component's members.
