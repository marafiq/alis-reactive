# Declarations

The vocabulary a component author declares a JavaScript object with: a live object's declaration, member for member as
the vendor's typed API has it, with each member's presence and value marks; an event and the payload its listener
receives; and data, which the plan reads and never writes or calls. Module 11 of `spec/CLAUDE.md`.

## Core questions

- Q2: a declared member is named by its JavaScript name, the vendor boundary (D2.7); data read past it is named as the
  app's JSON options name it (D2.6, D2.9).
- Q4: reading, writing or calling a member a live object lacks stops the pipeline (D4.1); data reads its start where a
  member is absent and nothing past a holder that holds null (D4.2); a promise where a value is declared stops (D4.3),
  and so does a promise from a method declared to return nothing (D4.4).
- Q5: a live object is a `JsObject` (D5.1) and data is `Data<T>` (D5.2); a declaration mirrors the vendor's typed API
  with its presence and value marks (D5.3); the door names the member's JavaScript kind (D5.8); a method of any arity
  takes one list of typed `Arg`s (D5.9); a result binds at its line (D5.11); the core makes a declaration with `new T()`
  (D5.12); an iterator is declared as any member is (D5.13).
- Q7: an argument is a value position, so it takes a literal or any source of its type, and a nullable property takes a
  source of the type without null (D7.3, N7.6).
- Q11: a method declared with `Method` whose vendor returns a promise stops its pipeline (D11.3).
- Q17: an event's payload sits at the place its declaration states in the listener's arguments (D17.4), and the vendor
  contract checks every declaration against the vendor's typed API (D17.5).
- Q18: a declaration door is the body of the member it declares, its arguments constants the build reads (N18.2).

## Boundaries

`Component`, `IComponent`, `ComponentVendor`, `VendorEventPayload`, `ComponentRef` and `RenderedComponent`, a
component's vendor and render helper, are module 12's, and so are the vendor contract analyzer (N17.2), the listening
kinds and the defects of Q17 (X17.*), among them an event its vendor raises with no argument (X17.15) and a `listen` on
an object a payload hands over (X12.9, X12.10). `IAppLevelComponent` goes with the app-level objects, modules 23 and 25.
The plugin types are module 13's. `Objects<TItem>` and `ListPropertyExtensions` are module 4's (M4.F2, M4.F1), and
`JsObject.Objects<TItem>()`, the declaration door, is this module's. The declaration analyzer (N18.2, X18.2) and the
override reads (X18.7, X18.9) are Q18's, modules 16 and 20; a member's four names (D2.3) are module 15's. A route's data
(`Data<TResponse>`, X10.1) is module 6's and a pushed message (X5.1) module 10's; the type `Data<T>` is this module's.
The wait for a `MethodAsync` (X4.11) and a payload's object after an awaited step (X11.9) are module 17's. A data member
a holder's type requires (X4.9, X4.10) and a populated or initialized start (X4.6, X4.7, X4.8) are module 16's.

## Doors

Each name is gate 2's; each door is the capability it carries.

This module adds no door. Its doors are the core's: `JsObject` and its declaration doors `Property`, `ReadOnly`,
`WriteOnly`, `Method`, `Method<T>`, `MethodAsync`, `MethodAsync<T>`, `Event`, `Event<T>`, `Fixed` and `Pass`, kept
(Q5); `ReturnedObject<T>` and `ReturnedObjects<T>` (N5.2); one door per method kind over a list of `Arg`s (N5.3); the
presence and nullability marks (N5.4); `LiveObject<T>` and `LiveObjects<T>` (N5.5); `Read` (N5.6); an iterator (N5.7);
`NullableWrites.Set` (N7.6); the payload's place (N17.1); and `Field.Of`, kept as an argument a declared method takes.

The meaning `M11.D1` is in `spec/Declarations/features.json`, on the feature it gives.

## Improvements

- One declaration per object, member for member (D5.3). Main declares a component's members as static extensions over
  `ComponentRef`, each named by a string
  (`Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBoxExtensions.cs:13-29` at 623dd4a27), so a
  component the app brings has no door; the redesign declares them on `JsObject` by kind
  (`Alis.Reactive/Components/JsObject.cs:74-293` at 65fdf118d), the vocabulary the rewrite keeps.
- The door names the kind (D5.8, N5.2, N5.5). The redesign's `Object<T>` tells a property's object from a method's by
  scanning the declaration's non-public members (`Alis.Reactive/Components/JsObject.cs:365-380` at 65fdf118d), which
  D18.3 refuses; `LiveObject<T>` and `ReturnedObject<T>` name it.
- Any arity (N5.3, X5.6). The redesign's `Method` takes four arguments at most, `Method<T>` three and `MethodAsync`
  two (`Alis.Reactive/Components/JsObject.cs:182`, `Alis.Reactive/Components/JsObject.cs:241`,
  `Alis.Reactive/Components/JsObject.cs:203` at 65fdf118d).
- Presence by the vendor's typed API (D5.3, D5.5, N5.4). The redesign has no mark: it reads a payload's members as
  optional (`Alis.Reactive/PlanModel/Values/Target.cs:80` at 65fdf118d), so a required member the vendor leaves out
  reads nothing (X4.3, X5.7), and reads and writes a returned object's members as optional
  (`Alis.Reactive/PlanModel/Values/Target.cs:123`, `Alis.Reactive/PlanModel/Reactions/Steps.cs:88` at 65fdf118d)
  while it calls them as required (`Alis.Reactive/PlanModel/Reactions/Steps.cs:114` at 65fdf118d), so a write gives
  the object a member it lacks (M11.X3).
- A plain method's promise stops (D4.4, X4.5, X11.3). The redesign logs the promise's rejection and runs the next step
  (`Alis.Reactive.Assets/runtime/grammar/execute.ts:45-47` at 65fdf118d), and its Methods page writes the next step
  after `StartSaving()`, a `Method` over the slate's `save`
  (`tests/Alis.Reactive.Framework.App/Members/Methods.cshtml:44-50`,
  `tests/Alis.Reactive.Framework.App/Components/Slate/Slate.cs:56` at 65fdf118d).
- `Read` replaces `Get` (N5.6, D5.10), and names a member past the declared one as the app's options name it (D2.9).
  The redesign's `Get` names it by its `JsonPropertyName`, else camel case
  (`Alis.Reactive/Components/JsMembers.cs:37-44`, `Alis.Reactive/Members/JsonMemberName.cs:11` at 65fdf118d).
- A nullable property is written from a source of its type without null by N7.6, on `WriteOnly<T?>` too; the
  redesign's `PropertyExtensions.Set` takes `Property<T?>` only (`Alis.Reactive/Components/JsMembers.cs:183` at
  65fdf118d).
- A `Field` names its member as the app's options name it (D2.9, M11.X2). The redesign's `Field.Of` fixes the name
  when it is made, by `JsonPropertyName` else camel case (`Alis.Reactive/Members/Field.cs:38`,
  `Alis.Reactive/Members/JsonMemberName.cs:11` at 65fdf118d), and Fusion's render options take that name
  (`Alis.Reactive.Fusion/Components/FusionDropDownList/FusionDropDownList.cs:103` at 65fdf118d).
- An event is declared on the object's own declaration and its payload with the whole vocabulary (D5.6). Main selects
  an event through `TypedEvent<TArgs>`, built only by the framework's packages with the event's name as a string
  (`Alis.Reactive.Fusion/Components/FusionAccordion/FusionAccordionEvents.cs:13-15` at 623dd4a27), and writes to a
  payload only through `IReactionEmitter`, by member names as strings
  (`Alis.Reactive.Fusion/Components/FusionAutoComplete/Events/FusionAutoCompleteOnFiltering.cs:44` at 623dd4a27).
- A declared member's default name is its declaring member's in camel case, whatever the app's JSON naming (M11.D1,
  M11.X1). The redesign derives it so (`Alis.Reactive/Components/JsObject.cs:385-391` at 65fdf118d), and so does the
  POC (`poc/Alis.Reactive.Poc/Components/JsObject.cs:233`); no core line states it.
