# Type knowledge

What the framework knows of a model, a request, a literal and a payload, with no run-time reflection of its own:
members, JSON names, shapes, item types and starts. Module 16 of `spec/CLAUDE.md`. The module is internal; its public
rows are the three doors D18.5 types.

## Core questions

- Q18: the framework knows a model from three sources, expression trees, generics and the app's System.Text.Json
  contract, and runs no reflection of its own outside the owner's list (D18.1); one item-type finder, the contract's
  element type, and its key and element types for a dictionary (D18.2); each reflection site of both lines has its
  verdict, an override read by the JSON name its declaration's contract gives it among them (D18.3); the three strings
  of B13 become typed closed sets (D18.5, N18.4).
- Q2: a request body names a member as the endpoint's binder reads it, the contract's JSON name on net10 and the CLR
  name on net48 (D2.4); the JSON naming is the app's, and no name is camel-cased by a rule of the framework's own
  (D2.9).
- Q16: a start is the one the endpoint's binder leaves for the request's host and body format (D16.6), the object
  System.Text.Json reads from `{}` for a JSON body on net10.

## Boundaries

A member's four names and the wire that carries them (D2.3, `MemberPath`) are module 15's; this module gives the JSON
name the contract holds. The analyzers' code is module 20's: the reflection analyzer (N18.1), the declaration analyzer
(N18.2), the override analyzer ALIS025 (N18.5) and the rungs of D18.6, with `TypedDslExemptionAttribute`, ALIS009's
exemption. A declaration's vocabulary, `LiveObject<T>()`, `LiveObjects<T>()`, `Method`, `MethodAsync` and the property
declarations Q18 keeps, is module 11's. A check's starts, the table of D16.6 by host and format, are module
8's; this module says where a start comes from. The request's container and its `Form` and `Inputs` (N18.3) are
module 6's and module 7's doors; X18.3 sits here as the verdict on the request member found by name. This module
claims three doors by door: `NativeButtonBuilder<TModel>.Type`, `NativeTextBoxBuilder<TModel, TProp>.Type` and
`NativeActionLinkBuilder<TModel>.Attr`. The rest of each builder is module 24's, the action link module 25's.

## Doors

Kept from the core (Q18): the typed doors of D18.5 for a button's type, a text box's type and an action link's
attributes (N18.4), whose forms are gate 2's. This module adds no door: every door it needs is a core door. It owns no
main-only door, so it gives no verdict. Every meaning is a core line (D18.1 to D18.3, D2.4, D2.9, D16.6), so it adds
none.

The defects X18.1 to X18.9 are in `spec/TypeKnowledge/features.json`, on the feature they give.

## Improvements

- Names come from the app's contract (D2.9, D18.3, X18.5). Main names every member in camel case
  (`Alis.Reactive/PlanAuthoring/ExpressionPaths/JsonMemberName.cs:9` at 623dd4a27); the redesign reads a member's
  `JsonPropertyName` and camel-cases the rest (`Alis.Reactive/Members/JsonMemberName.cs:9-11` at 65fdf118d), so
  neither follows an app whose options name members in snake case.
- Members come from the contract (D18.3, X18.6). The redesign lists public properties and drops those that carry
  `JsonIgnore` (`Alis.Reactive/Members/ClrShape.cs:39-43` at 65fdf118d), so a member the app's options leave out is
  still listed.
- One item-type finder (D18.2). Main finds a collection's item type and a dictionary by walking interfaces
  (`Alis.Reactive/PlanModel/Values/Shape.cs:145` and
  `Alis.Reactive/PlanAuthoring/ExpressionPaths/ExpressionPathHelper.cs:113` at 623dd4a27); the redesign does it three
  times (`Alis.Reactive/Inputs/FieldReference.cs:50-58`, `Alis.Reactive/Members/ClrShape.cs:82-101`,
  `Alis.Reactive/PlanAuthoring/Requests/RequestMembers.cs:66-72` at 65fdf118d).
- A start is the object System.Text.Json reads from `{}` (D16.6, D18.3). The redesign builds it with `Activator` and a
  constructor it invokes (`Alis.Reactive/PlanModel/Validation/ModelDefaults.cs:124-155` at 65fdf118d).
- Literals and payloads are written by System.Text.Json through their `T` (D18.3). The redesign reads a literal and a
  markup payload member by member (`Alis.Reactive/PlanModel/Values/ValueExpression.cs:76`,
  `Alis.Reactive/Razor/MarkupRaise.cs:27` at 65fdf118d).
- A request's members are matched by the compiler (N18.3, X18.3). The redesign finds a sent input's member by name,
  ignoring case (`Alis.Reactive/PlanAuthoring/Requests/RequestMembers.cs:34` at 65fdf118d).
- A declaration's caller is checked at build (N18.2, X18.2). The redesign scans the declaration's members at run time
  (`Alis.Reactive/Components/JsObject.cs:365-370` at 65fdf118d).
- An override reads by its declaration's JSON name, and a read of one the app's options ignore is refused (D18.3,
  X18.7, X18.8), as POC 3 proves (`tests/Alis.Reactive.Poc.PlaywrightTests/DotNotation/WhenARosterIsHandedOver.cs:30`,
  `tests/Alis.Reactive.Poc.PlaywrightTests/DotNotation/WhenARosterReadIsRefused.cs:25`).
- A closed set is a typed door (D18.5, N18.4, X18.4). Both lines take a string for a button's type, a text box's type
  and an action link's attribute (`Alis.Reactive.Native/Components/NativeButton/NativeButtonBuilder.cs:42`,
  `Alis.Reactive.Native/Components/NativeTextBox/NativeTextBoxBuilder.cs:73`,
  `Alis.Reactive.Native/Components/NativeActionLink/NativeActionLinkBuilder.cs:63` at 623dd4a27;
  `Alis.Reactive.Native/Components/NativeButton/NativeButtonBuilder.cs:59`,
  `Alis.Reactive.Native/Components/NativeTextBox/NativeTextBoxBuilder.cs:65`,
  `Alis.Reactive.Native/Components/NativeActionLink/NativeActionLinkBuilder.cs:81` at 65fdf118d).
