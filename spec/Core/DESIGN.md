# Core design

The core design of the Alis.Reactive rewrite, one section per question of `spec/BRIEF.md` section 5. Each
section states its decision as closed statements, the schema nodes it uses, the TLA+ invariant, probe fact
or check that checks it, the C# doors it implies, the evidence it resolves, and the defects it prevents with
the rung of principle 0 that holds each one.

Terms used throughout:

- A live object is a browser object a declaration names: an element's component, a plugin, or an object a
  live member returns. Its declared members are required.
- Data is a value with no declared methods: an event payload, a response body, a pushed message, an item of
  a list of data, a value a method returns. Its members are optional.
- Nothing is the value JavaScript holds as `null` or `undefined`. The plan never tells the two apart.
- To stop a pipeline is to run none of its later statements, log one error naming the handler and the
  reason, and run the `Finally` of every request the stop happens inside.
- A door is a line of a PublicAPI text. A door marked `N<q>.<k>` is a declaration this design proposes.

## Q1. Paths

**Decision.**
- D1.1 A path is a list of steps, and a step is exactly one of three: `member(name)`, `index(n)` with `n` a
  non-negative integer, or `each`.
- D1.2 In plan JSON a member is a JSON string, an index is a non-negative JSON integer, and each is the object
  `{"kind":"each"}`. A string is a member name whatever its text: `"[]"`, `"[*]"`, `"*"` and `"0"` are member
  names, never markers.
- D1.3 Every path the plan carries uses this step: `get`, `call` and `set` paths, a handler's path, a
  vendor's object path, an input binding's body path, a form field's path, and the member paths of a
  request.
- D1.4 A dictionary entry is a member step whose name is the key's text as System.Text.Json writes the key
  as a property name. A dictionary whose key type System.Text.Json writes no property name for is refused
  where the view renders.
- D1.5 `each` appears only in a form field's path (Q16). No read, write, call or subscription carries
  `each`; a projection over a list is `map` (Q6).
- D1.6 The MVC binding name is one function of a CLR member path, `BindingName`: member names joined by
  `.`, and an index written `[n]` directly after the step before it, as MVC's own
  `CreateSubPropertyName` and `CreateSubIndexName` write them. It is computed only in C#.
- D1.7 Binding name text appears only where the server speaks it: an input's `name` attribute, form-data and
  query keys, and the error keys a server's validation returns. The browser compares a server key with a
  binding name as whole text and never parses a name.
- D1.8 The member names in a path are the wire names of Q2 for the host the view renders on.

**Schema nodes.** `PathStep` (a string, an integer, or `each`) in `get`, `call`, `set`, `Handler.path`,
`Vendor.objectPath`, `InputBinding.path`, `FormField.path` and the request argument's member paths (Q9).

**Checker.**
- `probe:mvc5-sub-index-name`
- `probe:mvc5-sub-property-name`
- `probe:core-form-binds-indexed-name`

**Doors.**
- `N1.1` new: `internal readonly struct PathStep` in `Alis.Reactive/Grammar`, with the factories `Member`,
  `At` and `Each` and a private constructor, serialized as D1.2. It replaces every `IReadOnlyList<string>`
  path of the grammar.
- `N1.2` new: `internal static class BindingName` with `Of(Path clrPath)`, the one function of D1.6.
- internal at the State SHA, and internal in the rewrite: `Alis.Reactive.WireTerms.Path`, on no PublicAPI text.
  N1.1 replaces its segments; its `Text`, `Parse` and `[]` text form leave.

**Evidence.**
- B1: each item is spelled three ways: `Alis.Reactive/WireTerms/Path.cs:31`, `Alis.Reactive.Assets/runtime/grammar/paths.ts:5`,
  `Alis.Reactive/PlanModel/Validation/ModelDefaults.cs:202`, `Alis.Reactive/PlanModel/Validation/FormChecks.cs:54`,
  `Alis.Reactive.Assets/runtime/plugins/model.ts:75`, and `*` in `Alis.Reactive/WireTerms/Path.cs:129`.
- B2: a position is the text `"0"` in a path (`Alis.Reactive/WireTerms/Path.cs:28`) and the number `0` in a
  body (`Alis.Reactive/Grammar/BodyStep.cs:42`); the binding carries its path twice
  (`Alis.Reactive/Grammar/PlanDocument.cs:154`, `Alis.Reactive/Grammar/PlanDocument.cs:165`).
- P1: 57f173f58 parsed `Members[1]` as one member; b24c65775 left a number-keyed dictionary documented only.
- The browser parses names today: `Alis.Reactive.Assets/runtime/plugins/model.ts:113`.

**Defects.**
- `X1.1` type: a step that is not a member, an index or each cannot be constructed: new:N1.1.
- `X1.2` type: a member whose name looks like a marker is still a member: new:N1.1 writes members as strings only.
- `X1.3` type: writing, calling or subscribing to a projection over items does not compile; the projection is a
  `ReadOnly<IReadOnlyList<TField>>`, which has no `Set` (`Alis.Reactive/Components/JsMembers.cs:140`).
- `X1.4` refusal: a dictionary keyed by a type System.Text.Json cannot write as a property name: "A dictionary
  keyed by {type} cannot be sent: System.Text.Json writes no property name for its keys; key it by text or a number."
- `X1.5` behavior: a server error key `Members[1].Name` shows its message at the input rendered with that
  binding name, and in the summary when no input has it.

## Q2. One read

**Decision.**
- D2.1 Every value the plan reads is `get(target, path, members)` or `call(target, path, args, members)`, with
  typed paths (Q1). `members` is `required` or `optional`, and C# decides it from the holder (Q5).
- D2.2 No plugin takes a member path as text. The model plugin's `valueOf(pattern)` and `sentValueOf(sent,
  pattern)` leave; a form's check reads the body it sends with `get` over a typed path (Q16).
- D2.3 A member's four names come from one derivation, `MemberPath`, built from the typed lambda, the
  declaring view's `HtmlFieldPrefix` and the member steps: the binding name (`BindingName`, D1.6), the body
  path (wire names, D2.4), the JSON name of a data read (D2.5), and the element id (Q14).
- D2.4 A request body names each member as the endpoint's binder reads it, one rule per host:
  - net10: the name System.Text.Json's contract gives the member under the endpoint's MVC `JsonOptions`
    (`JsonPropertyName`, else the naming policy), read from `JsonSerializerOptions.GetTypeInfo`.
  - net48: the CLR name, which MVC 5's `JsonValueProviderFactory` and `DefaultModelBinder` match ignoring case.
- D2.5 Form-data and query keys are binding names (CLR names) on both hosts, since MVC binds form and query
  values by property name and ignores `JsonPropertyName`.
- D2.6 A data member read from a response body or a pushed message is named as the endpoint writes it: the
  System.Text.Json contract name under the endpoint's `JsonOptions` on net10, and the CLR name MVC 5's
  `JsonResult` writes through `JavaScriptSerializer` on net48.
- D2.7 A component's member is named by its JavaScript name in its declaration; that name is the vendor
  boundary and is not translated.
- D2.8 D2.4 to D2.6 replace the root CLAUDE.md meaning "A member is named as System.Text.Json names it: its
  JsonPropertyName, else camel case", which holds for a net10 JSON body only. Gate 1 carries the change.

**Schema nodes.** `get`, `call` (Q1 paths, `members`); the request argument's members (Q9).

**Checker.**
- `probe:core-frombody-binds-json-name`
- `probe:core-frombody-ignores-clr-name`
- `probe:core-form-ignores-json-name`
- `probe:core-form-binds-clr-name`
- `probe:core-json-result-names`
- `probe:mvc5-json-value-provider-case`
- `probe:mvc5-json-result-clr-names`

**Doors.**
- `N2.1` new: `internal sealed class MemberPath` in `Alis.Reactive/Members`, the one derivation of D2.3, with
  `BindingName`, `BodyPath`, `DataPath` and `ElementId`.
- `N2.2` new: `internal sealed class WireNames`, which answers a member's wire name per D2.4 to D2.6 for the
  host the view renders on.

**Evidence.**
- B3: a second resolver beside `get` parses a member path with a regex: `Alis.Reactive/PlanModel/Validation/FormRead.cs:36`,
  `Alis.Reactive/PlanModel/Values/ValueExpression.cs:323`, `Alis.Reactive.Assets/runtime/plugins/model.ts:112-114`.
- B14: the body is written with CLR names (`Alis.Reactive/Members/ExpressionPathHelper.cs:105`) while JSON reads
  use the JSON name (`Alis.Reactive/Members/ExpressionPathHelper.cs:100`).
- P1: the four names drifted under partials in 31aacf71e, 0090a6adc, a0a95e0bd and 8046bc4c5.
- G3: the holder decides requiredness (`Alis.Reactive/PlanModel/Values/Target.cs:57`, `Alis.Reactive/PlanModel/Values/Target.cs:80`).

**Defects.**
- `X2.1` type: reading a member a model or declaration does not have does not compile; every read starts from a
  typed lambda (`Alis.Reactive/Components/JsMembers.cs:37`).
- `X2.2` type: a plugin cannot be handed a member path as text: new:N2.1 gives the model plugin no text door.
- `X2.3` behavior: a request member declared `[JsonPropertyName("given")]` reaches a net10 `[FromBody]` endpoint
  bound.
- `X2.4` behavior: an input a partial renders under the prefix `Kin` has an id, a name, a body path and an error
  key that agree, so the server's message for `Kin.Name` shows at that input.
- `X2.5` refusal: a member lambda that names the model itself: "m => m names the model, not a member of it; name
  the member the input binds, as m => m.Name."

## Q3. Shapes and conversion

**Decision.**
- D3.1 A shape is one of `string`, `number`, `boolean`, `date`, `instant`, `file`, `any`, `array(item)`,
  `nullable(inner)`, `object(fields)`. `file` is new (Q9).
- D3.2 There is one conversion node, `as(expr, shape)`. The operators `toNumber`, `toDate`, `toBoolean` and
  `toText` leave the grammar, and a validation rule converts with `as` like every read.
- D3.3 A conversion of a present value the shape cannot hold yields nothing. No conversion passes the
  unconverted value on.
- D3.4 The conversions are the table of `spec/Core/conversions.md`, one row per source kind and shape; the
  browser implements exactly that table.
- D3.5 Blank is nothing, or text that is empty or white space as .NET's `string.IsNullOrWhiteSpace` defines it.
- D3.6 What a blank input binds as is a literal C# writes into the input's binding: the member's value in a
  new instance of the item or model that holds it, encoded as the endpoint encodes it (Q18). The browser
  chooses no default by shape.
- D3.7 A number goes into JSON as a JSON number. Into form data it goes as its text with the decimal
  separator of the culture the view renders under, which C# writes into the request argument, because both
  hosts bind form values under the request's culture. Into the query and the route it goes as its invariant
  text, because both hosts bind those under the invariant culture.
- D3.8 A date and an instant are shown and sent as `spec/Core/DateWire.tla` states (`yyyy-MM-dd`, the time
  when not midnight, an instant's clock offset, `0001-01-01T00:00:00Z` for an instant that reads nothing).

**Schema nodes.** `as`, the shapes of D3.1, `InputBinding.value`.

**Checker.**
- `Absence.tla:FailedConversionReadsNothing`
- `DateWire.tla:DateTimeCarriesNoOffset`
- `DateWire.tla:InstantCarriesItsClockOffset`
- `probe:core-form-decimal-culture`
- `probe:core-query-decimal-invariant`
- `probe:mvc5-form-current-culture`
- `probe:mvc5-query-invariant-culture`

**Doors.**
- `N3.1` new: the grammar's `Operator` without `ToNumber`, `ToDate`, `ToBoolean` and `ToText`.
- `N3.2` new: `internal sealed class FileShape` in `Alis.Reactive/Grammar/Shape.cs`, kind `file`.
- `N3.3` new: `InputBinding.value`, one `Expr` C# writes as `coalesce(as(read, shape), blank)`, replacing
  `value`, `shape` and `list`.

**Evidence.**
- B4: two conversions, `Alis.Reactive/Grammar/Operator.cs:27-30` and `Alis.Reactive.Assets/runtime/grammar/evaluate.ts:112-115`;
  validation converts through them (`Alis.Reactive/PlanModel/Validation/BrowserRule.cs:193-198`).
- B5: what a blank input binds as is chosen by shape kind in `Alis.Reactive.Assets/runtime/grammar/shape.ts:33-43`.
- B12: a failed conversion passes the raw value on, `Alis.Reactive.Assets/runtime/grammar/shape.ts:17-21`.
- B2: `value`, `shape` and `list` carry one fact (`Alis.Reactive/Grammar/PlanDocument.cs:165`).

**Defects.**
- `X3.1` type: a second conversion cannot be written: new:N3.1.
- `X3.2` type: a file cannot be read as text or sent as JSON by accident: new:N3.2 gives it its own shape, and
  the request argument (Q9) accepts a `file` member only in form data.
- `X3.3` behavior: the text `abc` in an input bound to an `int` reads nothing, so `GreaterThan(0)` fails in the
  browser as it fails on the server.
- `X3.4` behavior: a blank input bound to an `int` member initialized to 5 sends 5 and validates as 5.
- `X3.5` behavior: a decimal sent as form data from a view rendered under `de-DE` binds `1,5` on the endpoint.

## Q4. Absence and promises

**Decision.**
- D4.1 Reading, writing or calling a member a declaration names on a live object, when the object lacks the
  member, stops the pipeline.
- D4.2 Data reads as nothing when absent: a member of data, a holder of data that is nothing, and data past a
  declared member (a field of the value a property holds, of each item of a list it holds).
- D4.3 A read that yields a promise where a value is declared stops the pipeline. A method declared
  `MethodAsync` is awaited, and its settled value is its result. A rejected promise stops the pipeline.
- D4.4 A call of a method declared to return nothing that returns a promise stops the pipeline after the call,
  naming `MethodAsync`. There is no call whose promise nobody awaits.
- D4.5 A missing value set as an element's text or HTML shows nothing; a missing class name changes no class.
- D4.6 A route value that reads nothing stops its request before it is sent, and its `Finally` runs. C# writes
  this as a guard before the send and the statement `stop(reason)` (Q19).
- D4.7 A method called inside a condition runs only when its term is reached: `and`, `or` and `cond` evaluate
  their terms in order and stop at the first that decides.
- D4.8 A stop ends its own handler only. Other handlers, and the other blocks of a `Parallel`, run on (Q10).

**Schema nodes.** `get`, `call`, `set` with `members`; `do` and `let` with `await`; `try`; `op` (`and`, `or`,
`cond`, `coalesce`); the new statement `stop`.

**Checker.**
- `Absence.tla:RequiredAbsentStops`
- `Absence.tla:DataAbsentReadsNothing`
- `Absence.tla:PromiseWhereValueStops`
- `Absence.tla:UnawaitedPromiseStops`
- `Absence.tla:MissingTextShowsNothing`
- `Absence.tla:MissingClassChangesNothing`
- `Absence.tla:MissingRouteStopsAndFinallyRuns`
- `Absence.tla:StopEndsOnlyItsHandler`
- `Absence.tla:ConditionTermsLazy`

**Doors.**
- `N4.1` new: `internal sealed class StopStatement` in `Alis.Reactive/Grammar/Statements.cs`, kind `stop`, with
  the reason C# writes.
- existing: `MethodAsync` and `MethodAsync<T>` on `JsObject` (`Alis.Reactive/Components/JsObject.cs:188`).

**Evidence.**
- G3: `members` follows the holder (`Alis.Reactive/PlanModel/Values/Target.cs:57`) and the browser only follows it
  (`Alis.Reactive.Assets/runtime/grammar/paths.ts:24`).
- G4: a promise is declared and awaited (`Alis.Reactive/Grammar/Statements.cs:33`); a held promise throws
  (`Alis.Reactive.Assets/runtime/grammar/evaluate.ts:65`).
- G6: a missing text shows `coalesce(v, "")` (`Alis.Reactive/PlanAuthoring/Elements/HtmlElement.cs:104`) and a
  missing class name changes nothing (`Alis.Reactive/PlanModel/Reactions/Steps.cs:139-140`).
- B12: an unawaited promise's rejection is logged and the pipeline goes on
  (`Alis.Reactive.Assets/runtime/grammar/execute.ts:42-48`); a missing route value throws inside the plugin
  (`Alis.Reactive.Assets/runtime/plugins/http.ts:83`), stated only by `Finally`'s doc
  (`Alis.Reactive/PlanAuthoring/Requests/HttpRequestBuilder.cs:69-73`).

**Defects.**
- `X4.1` type: a method that returns a promise is declared apart from one that returns a value:
  `MethodAsync<T>` returns a different source type than `Method<T>` (`Alis.Reactive/Components/JsObject.cs:252`).
- `X4.2` behavior: a component that lacks a declared property stops the pipeline at the read, and the steps after
  it do not run.
- `X4.3` behavior: an event payload that lacks a field reads nothing, and the text it sets shows nothing.
- `X4.4` behavior: a route value that reads nothing sends no request, and the `Finally` hides the loader.
- `X4.5` behavior: a `Method()` that returns a promise stops its pipeline after the call and names `MethodAsync`.

## Q5. The object model

**Decision.**
- D5.1 A live object is a `JsObject`: an element's component, a plugin, the object an `Object<T>` member or
  method returns, and each item of an `Objects<T>` list. Its members are `Property`, `ReadOnly`, `WriteOnly`,
  `Method`, `Method<T>`, `MethodAsync`, `MethodAsync<T>`, `Object<T>`, `Objects<T>` and `Event<T>`.
- D5.2 Data is `Data<T>`: event payloads that declare no method, response bodies, pushed messages, values a
  method returns, and items of lists of data.
- D5.3 Requiredness, closed:

  | Holder | Member | Members |
  |--------|--------|---------|
  | live object | any declared member | required |
  | a payload type that declares a method | that method | required |
  | data | a field | optional |
  | the value a declared property holds | anything past the property | optional |

- D5.4 A dotted member at any depth reads the declared steps as required and every step past the declared
  member as optional, as two nested `get`s split at the boundary.
- D5.5 A returned live object (`Object<T>`) reads and calls its declared members as required. A returned value
  (`Method<T>`) is data.
- D5.6 An event payload that carries a method is declared as a `JsObject` payload; its declared methods are
  required, and its data fields are optional.
- D5.7 A server-pushed message is `Data<T>`, the same as a custom event's payload and a response body.
- D5.8 The declaration door names the kind of JavaScript member it declares. `Object<T>()` and `Objects<T>()`
  are the object and the list a property holds; `ReturnedObject<T>(..)` and `ReturnedObjects<T>(..)` are what
  a method returns. No door infers the kind from the C# member that declares it.
- D5.9 Every method kind (`Method`, `Method<T>`, `MethodAsync`, `MethodAsync<T>`, `ReturnedObject<T>`,
  `ReturnedObjects<T>`) takes its arguments as one list of typed `Arg`s, written as a collection expression,
  `Method([position, icons])`, so a declaration expresses a vendor method of any arity. The declaration's own
  C# signature keeps each argument typed (`Arg<string> position`).

**Schema nodes.** `obj`, `get`, `call` with `members`; `Handler.path`.

**Checker.**
- `Absence.tla:RequiredAbsentStops`
- `Absence.tla:DataAbsentReadsNothing`
- `probe:ej2-max-public-method-arity`
- `probe:ej2-methods-past-four-arguments`

**Doors.**
- kept, public for the component author: `JsObject`'s `Property`, `ReadOnly`, `WriteOnly`, `Method`,
  `Method<T>`, `MethodAsync`, `MethodAsync<T>`, `Object<T>()`, `Objects<T>()`, `Event`, `Event<T>`, `Fixed` and
  `Pass` (`Alis.Reactive/Components/JsObject.cs:74-260`).
- `N5.2` new: `ReturnedObject<T>(..)` and `ReturnedObjects<T>(..)` on `JsObject`, replacing `Object<T>(Arg..)`
  and the method form of `Object<T>()` and `Objects<T>()`.
- `N5.3` new: one door per method kind taking `IReadOnlyList<Arg>`, beside the door with no arguments,
  replacing the overloads of one to four arguments (`Alis.Reactive/Components/JsObject.cs:116-269`).
- refused: the `GetMembers` scan in `JsObject.DeclaresMethod` (`Alis.Reactive/Components/JsObject.cs:366-368`),
  which reads non-public members to tell a property from a method; N5.2 carries the capability.
- `N5.1` new: `ServerPushStream` and `SignalRHub` message triggers typed `Data<TMessage>`, replacing the
  `JsObject` with `ReadOnly<T>` members.

**Evidence.**
- B7: a custom event or a response is `Data<T>` (`Alis.Reactive/PlanAuthoring/Pipelines/TriggerBuilder.cs:50`) while
  server-pushed JSON is a `JsObject` with `ReadOnly<T>` members (`Alis.Reactive/PlanAuthoring/Pipelines/TriggerBuilder.cs:109-111`,
  `Alis.Reactive.SandboxApp/Areas/Sandbox/Models/HttpPipeline/RealTime/RealTimeModel.cs:8-10`).
- G7: one recursive vocabulary, `Object<T>`, `Objects<T>`, `MethodAsync<int>`
  (`tests/Alis.Reactive.Framework.App/Components/Slate/Slate.cs:20-28`).
- A returned object reads optional today (`Alis.Reactive/PlanModel/Values/Target.cs:123`), so a declared method on
  it reads as nothing when absent; D5.5 makes it required.

**Defects.**
- `X5.1` type: a pushed message cannot be written to: new:N5.1 types it `Data<TMessage>`, which has no `Set`.
- `X5.2` type: a method cannot be called on data with no declared methods; `Data<T>` exposes reads only
  (`Alis.Reactive/PlanAuthoring/Pipelines/TriggerBuilder.cs:50`).
- `X5.3` behavior: a returned object that lacks a declared method stops the pipeline at the call.
- `X5.5` type: a property's object cannot be mistaken for a method's returned object, and no member scan
  decides it: new:N5.2.
- `X5.6` type: a vendor method with more arguments than a declaration door takes: new:N5.3 takes any number.
- `X5.4` behavior: a field of the object a property holds reads nothing when the object lacks it.

## Q6. Arrays

**Decision.**
- D6.1 One plan form per operation. A projection is `call(list, ["map"], [fn])`; `each` in a read leaves (D1.5).
- D6.2 One name per operation, the names `ReactiveArray` has on both lines: `Where`, `Select`, `OrderBy`,
  `OrderByDescending`, `Count()`, `Count(predicate)`, `Any()`, `Any(predicate)`, `All(predicate)`, `Sum` over
  `int`, `decimal` and `double`, `Find(predicate)` and `Find(predicate, field)`. The property `Count` and the
  method `Each` on a declared list (`Objects<T>`) leave, renamed to `Count()` and `Select`.
- D6.3 An operation over a list that reads nothing reads nothing; `Any()` and `All()` over nothing read nothing,
  so a condition on them does not hold.
- D6.4 In an array lambda, a member .NET defines on the value's type (`Trim`, `Length`, `Year`, a `DayOfWeek`
  compared) reads nothing on a missing value and stops the pipeline on a value that lacks it.
- D6.5 The .NET members an array lambda translates are the closed table `spec/Core/translated-members.md`,
  keyed by the `MethodInfo` and `MemberInfo` the expression tree carries and built from typed lambdas; no
  attribute is scanned.
- D6.6 The JavaScript array methods the plan calls (`map`, `filter`, `some`, `every`, `find`, `includes`,
  `length`) are named at the vendor boundary of the built-in array; the browser adds no array function.

**Schema nodes.** `call` with `fn`; `get` of `length`; `op` (`coalesce`).

**Checker.**
- `Absence.tla:MissingListReadsNothing`
- `check:fluent`

**Doors.**
- existing: `ReactiveArray` (`Alis.Reactive/PlanAuthoring/Arrays/ReactiveArray.cs:44`, `Alis.Reactive/PlanAuthoring/Arrays/ReactiveArray.cs:93`).
- `N6.1` new: `Objects<TItem>.Count()` and `Objects<TItem>.Select(..)`, replacing `Count` and `Each`.
- `N6.2` new: `internal static class TranslatedMembers`, the closed table of D6.5.

**Evidence.**
- B8: `Each` and the property `Count` on a declared list (`Alis.Reactive/Components/JsMembers.cs:135`,
  `Alis.Reactive/Components/JsMembers.cs:140`) beside `Select` and `Count()` on an array
  (`Alis.Reactive/PlanAuthoring/Arrays/ReactiveArray.cs:44`, `Alis.Reactive/PlanAuthoring/Arrays/ReactiveArray.cs:93`).
- B11: the method table is scanned for `PureMethodAttribute` (`Alis.Reactive/PlanAuthoring/Arrays/ElementExpressionCompiler.cs:51-55`).

**Defects.**
- `X6.1` type: a list cannot be counted two ways: new:N6.1 leaves one `Count()`.
- `X6.2` refusal: an array lambda that calls a member outside the table: "{member} has no browser form in an
  array lambda; use a member the framework translates, or move the work to the server."
- `X6.3` behavior: `Where(x => x.Name.Trim() == "")` over a list whose item lacks `Name` reads that item's test
  as nothing, so the item is not kept.
- `X6.4` behavior: `Any()` over a list that reads nothing does not satisfy `When(list.Any())`.

## Q7. Every value position takes every source

**Decision.**
- D7.1 The positions are closed: a property or write-only property set, an element's text, HTML, class and
  attribute, both operands of every condition operator, a request member (which Q9 places in the route, the
  query, the body or a header), a header, a dispatch payload member, and a method's, plugin's or hub's argument.
- D7.2 The sources are closed, each a `TypedSource<T>`: a component's or plugin's member, a method's result
  (`Method<T>`, `MethodAsync<T>`, a hub's `Invoke<T>`), a page URL member (`p.Url<TQuery>()`), a field of an event
  payload or a pushed message, a field of a response body, an input's bound value (`p.Bound`), the result of an
  array operation, a literal, an object built from sources, and a list built from sources.
- D7.3 Every position takes a literal of its type and every source whose type is assignable to it:
  `TypedSource<TSource>` with `TSource : TValue`, and a `TValue` source at a `TValue?` position.
- D7.4 One builder makes an object from sources, member by typed member, and the dispatch payload, a method
  or plugin argument and a property write all use it. One door makes a list from sources.
- D7.5 Every condition operator takes a source for each operand: the text tests, `MinLength`, `Between`,
  membership and the comparisons. `Matches` takes a literal pattern only, because the pattern is checked
  where the view renders for the constructs JavaScript reads differently from .NET; a pattern read in the
  browser cannot be checked.
- D7.6 A source that reads nothing writes `null` into a property, an argument and a payload member. At an
  element's text and HTML it shows nothing, and at a class it changes nothing (D4.5). In a request it is
  decided by Q9.

**Schema nodes.** `lit`, `obj`, `list`, `get`, `call`, `set`, `op`; the request argument's members (Q9).

**Checker.**
- `Absence.tla:MissingValueWritesNull`
- `check:matrix`
- `check:fluent`

**Doors.**
- kept: `TypedSource<T>` (`Alis.Reactive/Components/TypedSource.cs:1`) and each position's literal and source
  doors, as `GatherBuilder.Set` writes them (`Alis.Reactive/PlanAuthoring/Requests/GatherBuilder.cs:70-73`).
- `N7.1` new: `p.Object<T>(o => o.Set(x => x.Member, source))`, a `TypedSource<T>` built member by member,
  replacing `DispatchPayloadBuilder<TPayload, TModel>` as a builder of its own.
- `N7.2` new: `p.List<T>([first, second])`, a `TypedSource<IReadOnlyList<T>>` built from sources.
- `N7.3` new: a source overload on `Contains`, `StartsWith`, `EndsWith`, `MinLength`, `Between` and the
  membership tests in `ConditionOperators`.

**Evidence.**
- G8: a source of an assignable type fills a request member (`Alis.Reactive/PlanAuthoring/Requests/GatherBuilder.cs:70-73`).
- B9: the text, length, range and membership tests take literals only
  (`Alis.Reactive/PlanAuthoring/Conditions/ConditionOperators.cs:106`, `Alis.Reactive/PlanAuthoring/Conditions/ConditionOperators.cs:126`,
  `Alis.Reactive/PlanModel/Conditions/Condition.cs:331`, `Alis.Reactive/PlanModel/Conditions/Condition.cs:343`), while the
  comparisons take both (`Alis.Reactive/PlanAuthoring/Conditions/ConditionOperators.cs:16`).
- B10: a gathered field's type is erased (`Alis.Reactive/PlanAuthoring/Requests/GatherBuilder.cs:167`) while
  `p.Bound` keeps it (`Alis.Reactive/PlanAuthoring/Pipelines/PipelineBuilder.cs:142`); Q9 takes the gather side.
- Two builders make an object from sources: `GatherBuilder` and `DispatchPayloadBuilder`
  (`Alis.Reactive/PlanAuthoring/Pipelines/DispatchPayloadBuilder.cs:31`, `Alis.Reactive/PlanAuthoring/Pipelines/DispatchPayloadBuilder.cs:45`).
- A write of a source that reads nothing writes whatever the browser read, `undefined` included
  (`Alis.Reactive.Assets/runtime/grammar/execute.ts:35-40`); no C# states it.

**Defects.**
- `X7.1` type: a source of a type the position cannot hold does not compile: `where TSource : TValue`
  (`Alis.Reactive/PlanAuthoring/Requests/GatherBuilder.cs:70-73`).
- `X7.2` type: a text test against a source that is not text does not compile: new:N7.3.
- `X7.3` type: an object argument built from sources is typed member by member, so a member of the wrong type
  or a member the type lacks does not compile: new:N7.1.
- `X7.4` type: a pattern read in the browser cannot reach `Matches`: new:N7.3 gives `Matches` no source overload.
- `X7.5` behavior: a property set from a payload field the payload lacks writes `null`, and the component
  shows its empty state.

## Q8. Conditions: three kinds

**Decision.**
- D8.1 One condition core: the `Condition` nodes (comparison, text test, membership, value test, all, any,
  not), the operators of Q7 and the sources of Q7. The browser evaluates every pipeline condition, array
  predicate and validator condition with it.
- D8.2 Two doors write it. The pipeline door is the typed builder (`p.When(source)`, `ElseIf`, `Else`, `And`,
  `Or`, groups). The lambda door translates an expression tree with the one table
  `spec/Core/translated-members.md`; array predicates and validator conditions use it.
- D8.3 They differ in exactly these ways, by what each is:
  - The operands. A pipeline condition reads any source. An array predicate reads the item and values the
    view supplies. A validator condition reads the members of the body its form's request sends (Q16).
  - A term the lambda door cannot translate. In an array predicate it is refused where the view renders.
    In a validator condition it leaves the rule to the server, which decides it by what it is.
  - Order. A pipeline's `When`, `ElseIf` and `Else` run in authored order, and the first that holds wins.
    A validator condition gates its rules and has no branches.
- D8.4 Template conditionals are not this core. Syncfusion's template engine evaluates them on its own
  data, and Q15 states what that engine evaluates.
- D8.5 Every term reads by Q4: a term over a source that reads nothing does not hold, and a term with a
  method call runs only when it is reached.

**Schema nodes.** `op` (the comparison, text, membership and value tests, `and`, `or`, `not`); `if`.

**Checker.**
- `Absence.tla:ConditionTermsLazy`
- `check:fluent`

**Doors.**
- kept: the pipeline door, `ConditionSourceBuilder` and `ConditionOperators`
  (`Alis.Reactive/PlanAuthoring/Conditions/ConditionOperators.cs:16`).
- `N8.1` new: `internal static class LambdaCondition`, the one translator from an expression tree to
  `Condition` nodes, with a result that says which term it could not translate. It replaces
  `ElementExpressionCompiler.CompilePredicate` (`Alis.Reactive/PlanAuthoring/Arrays/ElementExpressionCompiler.cs:68`)
  and `BrowserCondition` (`Alis.Reactive/Validation/BrowserCondition.cs:11`).

**Evidence.**
- Two translators of one lambda into one condition, each with its own member list: `ElementExpressionCompiler`
  refuses a term it cannot translate (`Alis.Reactive/PlanAuthoring/Arrays/ElementExpressionCompiler.cs:113`), and
  `BrowserCondition` returns no condition for one (`Alis.Reactive/Validation/BrowserCondition.cs:53`).
- A third translator writes template text (`Alis.Reactive.Fusion/Templates/FusionTemplateExpression.cs:9`).
- B9: the pipeline door's operators (`Alis.Reactive/PlanAuthoring/Conditions/ConditionOperators.cs:106`).

**Defects.**
- `X8.1` type: a condition on an operand of the wrong type does not compile: the operator is generic over
  the source's type (`Alis.Reactive/PlanAuthoring/Conditions/ConditionOperators.cs:16`).
- `X8.2` refusal: an array predicate with a term the lambda door cannot translate: "{term} has no browser form
  in an array predicate; use a member the framework translates, or move the test to the server."
- `X8.3` behavior: a validator condition with a term the lambda door cannot translate gates its rules on the
  server only; the browser sends the form and shows the server's message.
- `X8.4` behavior: `When(a).ElseIf(b).Else()` runs the first branch whose condition holds and no other.
