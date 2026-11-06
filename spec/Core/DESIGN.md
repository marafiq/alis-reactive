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
  System.Text.Json contract name under the app's JSON options. On net10 those are MVC's `JsonOptions`. On
  net48 they are the options the app registers with the framework, which the framework's JSON door writes
  with (N10.6). MVC 5's `JsonResult` writes CLR names and dates as `\/Date(ms)\/` through
  `JavaScriptSerializer` (`probe:mvc5-json-result-clr-names`), so it is no framework response (D3.10).
- D2.9 The JSON naming is the app's, not camel case alone (the owner, 2026-11-06: Syncfusion, one consumer of
  the framework, works with camel case and with other naming). Any naming System.Text.Json supports, a
  naming policy or none, with `JsonPropertyName` above it, is read from the one set of options D2.4 and D2.6
  name. The plan's JSON names follow them, and no name is camel-cased by a rule of the framework's own.
- D2.7 A component's member is named by its JavaScript name in its declaration; that name is the vendor
  boundary and is not translated.
- D2.8 D2.4 to D2.6 and D2.9 replace the root CLAUDE.md meaning "A member is named as System.Text.Json names it: its
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
- D3.9 Every date text is ISO 8601-1:2019's extended format, and an instant's is the RFC 3339 profile of it
  (the owner, 2026-11-06: dates to the standards).
  - A `DateTime` is a local date and time with no offset. Both hosts bind that text as an Unspecified
    `DateTime`, the same wall time on any server clock (`probe:core-form-date-kind`, `probe:mvc5-datetime-parse`).
  - A `DateTimeOffset` is an RFC 3339 date-time with its offset, `Z` at zero.
  - A `DateTime` is never sent with an offset: System.Text.Json reads an offset into a `DateTime` as the
    server's local time (`probe:core-stj-offset-into-datetime`), which moves the wall time by the server's
    clock.
- D3.10 A JSON response is System.Text.Json's on both hosts, written by a framework door. That means a
  `DateTime` with no offset, a UTC `DateTime` with `Z`, and a `DateTimeOffset` with its offset
  (`probe:core-stj-date-forms`).
  - On net48, MVC's `JsonResult` writes a `DateTime` as `\/Date(ms)\/` after `ToUniversalTime`
    (`probe:mvc5-json-result-date-form`, `probe:mvc5-json-result-date-utc`). That reads an Unspecified date
    as the server's local time, so a date of birth moves by the server's offset and can show the day
    before.
  - The net48 door N10.6 writes System.Text.Json instead. A text no row of `spec/Core/conversions.md`
    converts to a date reads nothing.
- D3.11 Culture is the server's, never the browser's (the owner, 2026-11-06). C# writes the view's culture
  into the plan once: its name, its decimal separator and its group separator. Every culture-dependent
  conversion in the browser uses it, and none reads the browser's locale:
  - An input's text converts to a number as the endpoint's host binds that number's .NET type under the
    view's culture. On net10, a decimal, double or float takes the group separator, an exponent and
    surrounding white space (`probe:core-form-decimal-group`). On net48, it takes `NumberStyles.Float`, which
    refuses a group separator (`probe:mvc5-decimal-float-style`, `probe:mvc5-number-format-from-culture`).
    An integer refuses both separators on both hosts (`probe:core-form-int-culture`, `probe:mvc5-int-integer-style`).
    So under de-DE, `1.5` reads 15 on net10 and nothing on net48, as each server binds it.
  - A URL parameter's text converts to a number under the invariant culture, as both hosts bind the query.
  - A number shows as text the way .NET writes it under the view's culture: JavaScript's shortest digits,
    the culture's decimal separator, no group separator. A double below 1E-04 or from 1E+17 up is written in
    .NET's exponent form, `E`, a sign and at least two digits (`probe:core-number-text-culture`,
    `probe:core-number-text-exponent`).
  - A number goes into form data as D3.7 states.
- D3.12 The time zone a view renders under is the app's choice, made per request where the view renders, as
  culture is (the owner, 2026-11-06: the framework allows the server to decide). The framework knows no
  reason for the choice: a community's configured zone, a user's or a facility's are the app's.
  - When the app makes no choice, an instant uses the browser's clock, the decision of 2026-11-05; gate 1
    confirms that default.
  - The app gives a `TimeZoneInfo` on both hosts, and the plan carries the zone as an IANA id. The browser
    computes a zone's offsets only from an IANA id (`probe:intl-iana-zone-offsets`) and refuses a Windows id
    (`probe:intl-windows-zone-id`).
    - On net10, C# writes the IANA id .NET converts the zone to (`probe:core-windows-zone-to-iana`).
    - On net48, `TimeZoneInfo` knows only the Windows registry's ids (`probe:net48-time-zones-from-registry`).
      So the framework maps the Windows id to IANA through a table generated once from CLDR's
      `windowsZones.xml` at a pinned release (`probe:cldr-windows-zone-default`). That is the same mapping
      net10 gives: `India Standard Time` becomes `Asia/Calcutta` in both.
    - A Windows id the table does not hold is refused where the view renders, naming the id.
  - Every conversion of an instant uses the view's zone. An instant shows as its wall time and offset in
    that zone for its own day, and a wall time a user picks becomes an instant at that zone's offset for
    that day. DateWire's offset per day is the zone's.
  - A `DateTime` is a wall time with no zone, so it is shown and sent as it is under any zone (D3.9).

**Schema nodes.** `as`, the shapes of D3.1, `InputBinding.value`.

**Checker.**
- `Absence.tla:FailedConversionReadsNothing`
- `DateWire.tla:DateTimeCarriesNoOffset`
- `DateWire.tla:InstantCarriesItsClockOffset`
- `probe:core-form-decimal-culture`
- `probe:core-query-decimal-invariant`
- `probe:mvc5-form-current-culture`
- `probe:mvc5-query-invariant-culture`
- `probe:core-form-date-kind`
- `probe:mvc5-datetime-parse`
- `probe:core-stj-offset-into-datetime`
- `probe:core-stj-date-forms`
- `probe:mvc5-json-result-date-form`
- `probe:mvc5-json-result-date-utc`
- `probe:core-form-decimal-group`
- `probe:mvc5-decimal-float-style`
- `probe:core-form-int-culture`
- `probe:mvc5-int-integer-style`
- `probe:core-number-text-culture`
- `probe:core-number-text-exponent`
- `probe:intl-iana-zone-offsets`
- `probe:intl-windows-zone-id`
- `probe:core-windows-zone-to-iana`
- `probe:net48-time-zones-from-registry`
- `probe:cldr-windows-zone-default`

**Doors.**
- `N3.1` new: the grammar's `Operator` without `ToNumber`, `ToDate`, `ToBoolean` and `ToText`.
- `N3.2` new: `internal sealed class FileShape` in `Alis.Reactive/Grammar/Shape.cs`, kind `file`.
- `N3.3` new: `InputBinding.value`, one `Expr` C# writes as `coalesce(as(read, shape), blank)`, replacing
  `value`, `shape` and `list`.
- `N3.4` new: the view's culture in the plan, its name, decimal separator and group separator, written once
  by C# where the view renders, with the endpoint host's number styles per .NET type.
- `N3.5` new: the view's time zone, the app's choice per request (D3.12), one door on both hosts that takes a
  `TimeZoneInfo`; C# writes its IANA id into the plan, through .NET on net10 and through the CLDR table
  generated once on net48. Its door's form goes to gate 2.

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
- `X3.6` behavior: under `de-DE`, `1,5` typed in a decimal input passes `GreaterThan(1)` in the browser, as the
  server passes it, whatever the browser's own locale.
- `X3.7` behavior: a date of birth a net48 endpoint answers through N10.6 shows its own day in a browser whose
  clock is behind the server's.
- `X3.8` behavior: a decimal set as an element's text under `de-DE` shows `1234,5`, as the server's view shows
  it.
- `X3.9` behavior: a view rendered under the zone `America/New_York` shows a visit's `DateTimeOffset` at its New
  York wall time and offset in a browser whose clock is in another zone, and sends a picked time back with
  New York's offset for that day.

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
- D8.6 A pipeline condition has one syntax: `When(source)`, `ElseIf(source)`, `And(source)` and `Or(source)`,
  where the source is any typed source of Q7, an event payload's member included (`When(args.ClosedBy)`).
  Main's second form, a payload and a path lambda (`When(args, x => x.IsExpanded)`), is refused, and
  `When(source)` carries it: the payload's member is a typed source (the owner, 2026-11-06).
- D8.7 A branch (`Then`, `Else`) is a block: it holds any number of steps, written in one lambda, as a chain
  or as statements in a `{}` body, and runs them in authored order.
- D8.8 A block is a lexical scope, as a C# `{}` block is. A value a block binds is readable in that block and
  in the blocks inside it, and nowhere else. The values a block binds: its handler's event payload, a
  route's response body, an array operation's item, a method's result read in it, and a `Confirm`'s answer.
  A read of a bound value outside its block is refused where the view renders, naming the value and the
  block that binds it. That one rule covers today's two refusals of this kind, an array item and a response
  read outside their blocks. The browser runs each block in a child scope, so a value bound inside a branch
  never reaches the steps after it.

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
- Two syntaxes on main: `git grep` over its views at 623dd4a27 finds 201 conditions of the form
  `When(payload, x => ..)` and 119 of `When(source)`; at 65fdf118d it finds none of the first and 714 of the second.
  The redesign has 239 branches with a `{}` body, main 178.
- Block scope today: the C# gives each block its own frame of results (`Alis.Reactive/PlanModel/Writing/Scope.cs:29`) and refuses
  an array item read outside an array operation (`Alis.Reactive/PlanModel/Writing/Scope.cs:38`) and a response read outside its
  request's routes (`Alis.Reactive/PlanModel/Writing/Scope.cs:78-79`); the browser runs each branch in a child environment
  (`Alis.Reactive.Assets/runtime/grammar/execute.ts:60-65`, `Alis.Reactive.Assets/runtime/grammar/evaluate.ts:17-19`).

**Defects.**
- `X8.1` type: a condition on an operand of the wrong type does not compile: the operator is generic over
  the source's type (`Alis.Reactive/PlanAuthoring/Conditions/ConditionOperators.cs:16`).
- `X8.2` refusal: an array predicate with a term the lambda door cannot translate: "{term} has no browser form
  in an array predicate; use a member the framework translates, or move the test to the server."
- `X8.3` behavior: a validator condition with a term the lambda door cannot translate gates its rules on the
  server only; the browser sends the form and shows the server's message.
- `X8.4` behavior: `When(a).ElseIf(b).Else()` runs the first branch whose condition holds and no other.
- `X8.5` refusal: a response body captured in a C# variable inside `OnSuccess` and read after the request is
  refused with "The response body of the request to {url} is read outside the block that binds it: read it
  inside that request's routes or the blocks they hold."
- `X8.6` behavior: a branch with three steps in a `{}` body runs all three, in order, and none when its
  condition does not hold.

## Q9. The request

**Decision.**
- D9.1 A request is one step, `let res = await $http.send(arg)`, and `arg` is the typed plugin argument
  `HttpSend`, a contract generated into the plan's TypeScript beside the grammar. The browser declares no
  request type of its own. The grammar gains no node (G2).
- D9.2 The builder is staged: each part is written at most once, a GET has no body format, and `Form()` exists
  only on a request that checks a form.
- D9.3 The method is GET, POST, PUT or DELETE. A GET sends no body; POST, PUT and DELETE send one.
- D9.4 The URL is parsed once, in C#, into text parts and placeholders. A URL that holds `#`, a brace without
  its pair, or a placeholder no member fills is refused where the view renders. A placeholder is filled by
  the top-level member of the same name of the request or of its query, compared ignoring case. A list, an object, a file, a date
  or an instant in a placeholder is refused, on both hosts, because IIS refuses a time in a path segment on
  net48 (#237). A route value is sent as its invariant text, encoded as one path segment.
- D9.5 A route value that reads nothing or reads empty text stops its request before it is sent (D4.6).
- D9.6 Every method sends a query. A GET sends there every member of its request that fills no placeholder;
  POST, PUT and DELETE send there the members of a typed query, `Query<TQuery>(q => q.Set(..))`, beside the body.
  A member is static (a literal) or read from a source. The URL's own query text is kept, and C# appends the
  members after it with `?` or `&`. A key is the binding name, a list sends one key per item, a number is its
  invariant text and a date is the wire form of `spec/Core/DateWire.tla`.
- D9.7 A JSON body is one object. Its member names are Q2's, its nesting follows Q1's steps, and a dictionary
  entry is a member named by its key. A JSON request that sets nothing sends `{}`, because on net10 a request
  with no body answers 415 or binds null by the endpoint parameter's nullability (`probe:core-frombody-no-body`).
- D9.8 Form data is multipart, written by the browser. A field is named by its binding name, a list sends one
  field per item, a number carries the decimal separator of the view's culture (D3.7) and a file is sent as
  the file.
- D9.9 DELETE sends its body as POST and PUT do. A member that fills a placeholder is also sent in the body of a
  POST, PUT or DELETE.
- D9.10 A member that reads nothing is sent by this closed table, which C# writes into the plan:

  | The member's type | JSON body | Query or form data |
  |-------------------|-----------|--------------------|
  | a non-nullable value type | left out | left out |
  | a list | left out | left out |
  | a nullable value type, text or a class | `null` | an empty field, `name=` |

  A member left out keeps its starting value; a JSON `null` for a non-nullable value type binds no instance
  at all, and an empty field for one is a model error (`probe:core-json-null-member`, `probe:core-form-empty-field`).
- D9.11 A header is fixed text or a scalar source sent as text. A header that reads nothing is not sent. A
  header name is an HTTP token by RFC 9110, a name given twice ignoring case is refused, and `Content-Type` is
  refused because the body format owns it.
- D9.12 The headers every request sends are configured once in C#, and C# writes them into each request's
  `HttpSend` where the view renders; a request's own header of the same name, compared ignoring case, replaces
  the configured one. An app that sends `X-Requested-With: XMLHttpRequest` on every request configures it there.
  The browser adds no header.
- D9.13 A literal member, `Set(member, value)`, is written where the view renders, encoded as the endpoint
  binds it: an enum by the System.Text.Json contract under the endpoint's options on net10 and by its name on
  net48 (Q18).
- D9.14 A hidden input is an input: it binds by its binding name and shape, and a form sends it when the sent
  element holds it.
- D9.15 What a request sends is closed: members it sets from sources (`Set`), the inputs of the form it checks
  (`Form()`), the inputs of one element (`Form(formId)`), and named inputs, each typed by its own lambda. When
  an input and a set member send the same binding name, the input's value is sent, as the body the check reads
  (Q16). An element `Form(formId)` names that the page lacks stops the request.
- D9.16 A file is the shape `file`, sent only in form data. A set member of file shape in a GET or a JSON body
  is refused by an analyzer; a file input a form sends in a GET or a JSON body stops the request when it is sent.
- D9.17 Requests in one pipeline run one after another, each after the one before it settles. ALIS008 keeps
  refusing a second request in one pipeline, with its reason corrected: `Chained` runs a request after another
  succeeds, and `Parallel` sends them at once.
- D9.18 `p.Url<TQuery>()` reads the page's own URL. It is a source (Q7), not a part of the request.
- D9.19 A member whose source holds an object or a list of objects is sent in a query or in form data as its
  fields, each by its binding name (`Address.City`, `Lines[0].Name`), as the binder reads them
  (`probe:core-form-binds-indexed-name`); in a JSON body it is nested (D9.7). A component's member, a method's
  result, a page URL member, a hidden input and a literal fill a member the same way (Q7).

**Schema nodes.** `let` with `await`, `call` on the plugin `http`, `record`, `list`, `lit`, `get`, `as`, `op`; the
shape `file`; the contract `HttpSend`, which is no node kind.

**Checker.**
- `Request.tla:EveryPlaceholderFilled`
- `Request.tla:RouteHoldsScalarText`
- `Request.tla:FileOnlyInFormData`
- `Request.tla:NothingBindsAsDeclared`
- `probe:core-json-null-member`
- `probe:core-form-empty-field`
- `probe:core-frombody-no-body`
- `probe:core-frombody-binds-json-name`

**Doors.**
- kept: `Get`, `Post`, `Put` and `Delete` (`Alis.Reactive/PlanAuthoring/Requests/RequestExtensions.cs:58-62`), `Gather` and
  `Gather<TRequest>`, `GatherBuilder.Set` (`Alis.Reactive/PlanAuthoring/Requests/GatherBuilder.cs:70-73`), `Header`, `Form()`,
  `Form(formId)`, `AsJson`, `AsFormData`.
- `N9.1` new: `internal sealed class HttpSend` in `Alis.Reactive/PluginArguments`, with its parts `PathPart`,
  `SentField`, `SentBody` and `SentHeader`, generated into the plan's TypeScript; it replaces the hand-typed
  `HttpRequest` (`Alis.Reactive.Assets/runtime/plugins/http.ts:10-18`).
- `N9.2` new: the staged builder: a GET builder with no body format, and `Form()` only on a request that checks.
- `N9.3` new: named inputs typed one by one, `Field(m => m.Member)`, replacing
  `Form(params Expression<Func<TModel, object?>>[])` (`Alis.Reactive/PlanAuthoring/Requests/GatherBuilder.cs:167`).
- `N9.4` new: analyzer ALIS016, a file-shaped set member in a GET or a JSON body.
- `N9.5` new: ALIS008's corrected message
  (`Alis.Reactive.Analyzers/HttpPipeline/MultipleHttpRequestsInPipelineAnalyzer.cs:17`).
- `N9.6` new: the request defaults of D9.12 and D10.12, configured once in C#; their door's form goes to gate 2.
- `N9.7` new: `Query<TQuery>(q => q.Set(x => x.Member, value or source))` on POST, PUT and DELETE.

**Evidence.**
- B6: the request is a record of string-named fields (`Alis.Reactive/PlanModel/Requests/RequestStep.cs:101`) typed by
  hand in the browser (`Alis.Reactive.Assets/runtime/plugins/http.ts:10-18`).
- B10: a named input's type is erased (`Alis.Reactive/PlanAuthoring/Requests/GatherBuilder.cs:167`).
- B12: a missing route value throws inside the plugin (`Alis.Reactive.Assets/runtime/plugins/http.ts:83`).
- B14: a request member is written with its CLR name (`Alis.Reactive/Members/ExpressionPathHelper.cs:105`), and a net10
  `[FromBody]` endpoint does not bind a `[JsonPropertyName]` member by its CLR name (`probe:core-frombody-ignores-clr-name`).
  ALIS008 says "only the last request survives" (`Alis.Reactive.Analyzers/HttpPipeline/MultipleHttpRequestsInPipelineAnalyzer.cs:17`),
  while each request is appended as its own step (`Alis.Reactive/PlanModel/Document/PlanBuildContext.cs:109`), read and not run.
- G8: a set member takes any source of its type (`Alis.Reactive/PlanAuthoring/Requests/GatherBuilder.cs:70-73`).
- P1: 8046bc4c5 checked a request for the unprefixed name; b24c65775 left a number-keyed dictionary unsent in JSON.

**Defects.**
- `X9.1` type: a body format on a GET does not compile: new:N9.2.
- `X9.2` type: `Form()` on a request that checks no form does not compile: new:N9.2.
- `X9.3` type: a named input whose type the request member does not hold does not compile: new:N9.3.
- `X9.4` analyzer: a file-shaped set member in a GET or a JSON body: new:N9.4.
- `X9.5` refusal: a placeholder no member fills: "The URL '{url}' has the placeholder {name}, and the request has no
  member named {name}; add the member, or take the placeholder out of the URL."
- `X9.6` refusal: a date in a placeholder: "{member} is a date, and a date cannot fill the placeholder {name}: IIS
  refuses a time in a path segment. Name the member so it goes in the query."
- `X9.7` refusal: `Content-Type` as a header: "Content-Type is set by the body format; use AsJson() or AsFormData()."
- `X9.8` behavior: a non-nullable member that reads nothing is left out of the JSON body, and the endpoint binds
  its starting value with no model error.
- `X9.9` behavior: a route value that reads nothing sends no request, runs its `Finally`, and runs no step after it.

## Q10. The request's flow and its response

**Decision.**
- D10.1 A request runs in this closed order: its check, then `WhileLoading`, then, inside a `try` whose
  `finally` is the request's `Finally`, its guards (route values, the element `Form(formId)` names), the send
  and one response route; then `Chained`; then the steps after the request.
- D10.2 A form check that fails shows its messages, sends nothing, runs neither `WhileLoading` nor `Finally`,
  and stops its pipeline: no step after the request runs.
- D10.3 A `WhileLoading` that stops sends nothing, runs no `Finally` and stops its pipeline. `Finally` runs
  exactly when `WhileLoading` ran, after every outcome, a stop inside a route included.
- D10.4 A request follows a redirect that ends on the page's own origin, and the response it ends at is the
  request's answer: its status and body pick the route (`probe:fetch-follows-relative-redirect`). A redirect
  that ends on another origin is refused.
- D10.5 The outcomes are closed, and exactly one route runs for each, by D10.13:

  | Outcome | Route |
  |---------|-------|
  | a 2xx status and a body of the declared kind | `OnSuccess` |
  | a status that is not 2xx | the route for that status, else the catch-all |
  | the network failed | the catch-all |
  | a redirect that ended on another origin | the catch-all |
  | a 2xx status and a body not of the declared kind | the catch-all |

  The last three each log one error naming the request and the outcome, and their data reads nothing. A
  request with no route for its outcome runs none.
- D10.6 The plan declares each route's body kind: JSON for `OnSuccess<T>` and `OnError<T>`, HTML for `Into`, and
  none for a route with no data, whose body is not read. JSON is a body whose media type is `application/json`
  or ends in `+json`; an empty JSON body is data that reads nothing. A JSON body that does not parse is a body
  not of the declared kind. An error route reads its body as data: a body not of its kind reads nothing, and
  the route runs.
- D10.7 On net48 the plan carries that a response's `X-Responded-JSON` status replaces its own, so Katana's
  answer to a request that sends `X-Requested-With` runs the route of the status it withheld.
- D10.8 A server's validation errors have one body on both hosts, `{"errors": {"<binding name>": ["message"]}}`,
  status 400, written by a framework door: on net10 MVC's `ValidationProblemDetails`
  (`probe:core-validation-problem-shape`), on net48 the same JSON with `TrySkipIisCustomErrors` set, the switch
  System.Web gives a response that keeps its own error body under IIS (`probe:systemweb-try-skip-iis-custom-errors`).
  The browser reads that body only: a message shows at the input whose binding name is its key, else in the
  form's summary.
- D10.9 `Chained` runs after the first request's success route and its `Finally`, only on success, and reads the
  first response's data.
- D10.10 `Parallel` sends its requests at once. Each runs its own route as its response arrives, a stop in one
  ends that one only, and the steps after `Parallel` run once, after every request settled, whatever each
  outcome. `OnAllSettled` writes the same plan as those steps, after the `all` statement in the same block
  (`Alis.Reactive/PlanModel/Requests/ParallelStep.cs:28-35`): one meaning behind two doors, whose verdict goes
  to gate 2.
- D10.11 `Into(element)` loads the success body's HTML into the element as a slot (Q12): what the element held
  unloads first.
- D10.12 Request defaults are configured once in C# and hold headers (D9.12), `WhileLoading` steps, `Finally`
  steps, error routes by status, a catch-all error route and a retry policy. C# composes them into every
  request where the view renders; the browser holds no defaults. A request's `WhileLoading` runs after the
  defaults', and its `Finally` before the defaults'. A request's retry policy replaces the defaults'. Success
  has no default: `OnSuccess` is the request's own. A request takes no defaults when it says so, whole.
- D10.13 For an outcome that is not success, the route is the request's route for its status, else the
  defaults' route for its status, else the request's catch-all, else the defaults' catch-all, else none. So a
  request routes only the statuses it cares about.
- D10.14 Retry is a policy, the defaults' or the request's, and no request retries without one. It retries a
  network failure and the statuses it names, from 408, 429, 502, 503 and 504, and no other outcome. It retries
  GET, PUT and DELETE, which HTTP defines as idempotent (RFC 9110, section 9.2.2), and a POST only when its policy
  says the POST is repeatable. It makes at most the attempts it states, waits the delay it states between
  attempts, and waits what a 429 or 503 response's `Retry-After` states, up to its longest wait; a longer wait
  ends the retries.
- D10.15 Across attempts, the request's check, guards and argument run once, and every attempt sends the same
  argument. `WhileLoading` runs once before the first attempt, the route once for the last attempt's outcome,
  and `Finally` once after it. An owner disposed between attempts ends the retries (Q11).
- D10.16 Conditions work in every block of a request, through the one condition core (Q8), and mean one thing
  in each:
  - A block that holds steps is a pipeline and takes `p.When` as every pipeline does: `WhileLoading`,
    `Finally`, `OnSuccess`, each `OnError`, the block `Chained` runs and each `Parallel` branch. Today a
    `Chained` request and a `Parallel` branch are one request each, with no pipeline around them
    (`Alis.Reactive/PlanAuthoring/Requests/ResponseBuilder.cs:116`,
    `Alis.Reactive/PlanAuthoring/Requests/RequestExtensions.cs:42`).
  - A part the request sends takes a condition: a member `Set`, a form's inputs, a named field and a header.
    When the condition fails, the part is left out, as if it were never gathered. The endpoint binds the
    member's starting value (D9.10), and the form's check reads the member as the endpoint binds it (Q16).
    A part whose condition fails is never sent as nothing, which a nullable member would bind as null.
  - A chained request whose condition fails is not sent, and the steps after the first request run. A
    parallel branch whose condition fails sends nothing and settles at once.
  - The check takes no condition. The server checks by the same validator on every request, so a browser
    check a condition skipped would disagree with the server's verdict. A check that depends on the model
    is the validator's own `When` or `Unless` (Q8, Q16).
  - The method, the URL and the body format take no condition: a request that differs in them is a second
    request under a pipeline's `When`. The routes take none: the outcome picks the route (D10.13), and the
    steps inside it take `p.When`.
  - A part's condition is evaluated by the one evaluator before the check reads the body, once per request,
    across every attempt (D10.15).
- D10.17 A request has one lifecycle with every part, and a condensed form for the common request. A request
  that writes only its method, its URL, what it sends and its success takes every other part from its
  defaults (D10.12): the loading steps, `Finally`, the error routes and the retry policy. A default part is
  written for each request where the view renders and reads that request's own parts: its method, its URL,
  the form it checks and the form it sends. So a default route for 400 shows the errors at the request's own
  form. A part the request writes replaces or adds to its default, as D10.12 and D10.13 state. The condensed
  form and the full form write the same plan. The condensed form's syntax goes to gate 2 (step 7), with what
  what a `.cshtml` MVC view makes hard in its lambdas as its input.

**Schema nodes.** `let` with `await`, `try`, `if`, `get` of `ok` and `status` on the response, the contract
`HttpSend` and its result, `HttpResponse`.

**Checker.**
- `Request.tla:FailedCheckSendsNothing`
- `Request.tla:FinallyRunsWhenLoadingRan`
- `Request.tla:OneRouteRuns`
- `Request.tla:FailureRunsTheCatchAll`
- `Request.tla:ChainedOnlyAfterSuccess`
- `Request.tla:StepsAfterRunUnlessStopped`
- `Request.tla:RetriesOnlyWhatIsRetryable`
- `Request.tla:RetryStopsAtItsAttempts`
- `Request.tla:OneLifecycleAcrossAttempts`
- `Request.tla:FailedConditionLeavesThePartOut`
- `Request.tla:ChainedOnlyWhenItsConditionHolds`
- `Lanes.tla:AllSettledAfterEveryRequest`
- `probe:fetch-follows-relative-redirect`
- `probe:core-cookie-auth-ajax`
- `probe:katana-ajax-redirect-is-200`
- `probe:core-validation-problem-shape`

**Doors.**
- kept: `Response`, `OnSuccess`, `OnSuccess<T>`, `OnError`, `OnError(status)` and their typed forms, `Chained`,
  `Parallel` with `OnAllSettled`, `WhileLoading`, `Finally`, `Into`, `Validate` and `ValidationErrors(formId)`.
- `N10.1` new: the error door on each host, `ValidationResult.ToErrorResponse()`: net10 a `ValidationProblemDetails`
  result, net48 a JSON result with status 400 and `TrySkipIisCustomErrors`, replacing the body each app builds by hand.
- `N10.2` new: `HttpResponse`, the contract of what `send` returns (`ok`, `status`, `outcome`, `body`).
- `N10.3` new: the retry policy of D10.14, on the defaults and on a request; its door's form goes to gate 2.
- `N10.4` new: a condition on a part a request sends, on a chained request and on a parallel branch (D10.16),
  through the one condition core; its door's form goes to gate 2.
- `N10.5` new: the condensed request of D10.17, whose default parts read the request's own parts; its form goes
  to gate 2.
- `N10.6` new: the net48 JSON response door, a result that writes its body through System.Text.Json under the
  options the app registers with the framework (D2.6, D2.9), so dates are ISO 8601 (D3.10) and names follow
  the app's naming on both hosts.

**Evidence.**
- The flow today: the check guards the sentence (`Alis.Reactive/PlanModel/Requests/RequestStep.cs:85`), the exchange
  sits in a `try` with `Finally` (`Alis.Reactive/PlanModel/Requests/RequestStep.cs:108`), and `Chained` runs on `ok` after it
  (`Alis.Reactive/PlanModel/Requests/RequestStep.cs:111`); the routes split on `ok` (`Alis.Reactive/PlanModel/Requests/RequestStep.cs:220`).
- A network failure and an unreadable body both read status 0 and run the error route
  (`Alis.Reactive.Assets/runtime/plugins/http.ts:37`, `Alis.Reactive.Assets/runtime/plugins/http.ts:53`).
- The body's kind is decided by its media type in the browser (`Alis.Reactive.Assets/runtime/plugins/http.ts:168`), so a
  login page that answers 200 after a redirect reaches `OnSuccess` as text; fetch follows the redirect
  (`Alis.Reactive.Assets/runtime/plugins/http.ts:35`).
- B12: the error body's shape is guessed, a list, else text, else `""` (`Alis.Reactive.Assets/runtime/plugins/form/form.ts:180`),
  and each app writes it by hand (`Alis.Reactive.SandboxApp/Areas/Sandbox/Controllers/Components/AppLevel/DrawerController.cs:51`).
- `Finally` is stated only in its XML doc (`Alis.Reactive/PlanAuthoring/Requests/HttpRequestBuilder.cs:69-73`).

**Defects.**
- `X10.1` type: a route's data is typed by the route that declares it, `Data<TResponse>` and `Data<TError>`
  (`Alis.Reactive/PlanModel/Requests/RequestStep.cs:220`).
- `X10.2` behavior: with `X-Requested-With` among the configured headers, a request whose cookie session expired
  runs the route for 401 on net10 (`probe:core-cookie-auth-ajax`) and on net48 under Katana
  (`probe:katana-ajax-redirect-is-200`). Without it the server redirects to its login page, and the request
  ends there: a route that declares JSON runs the catch-all, and a route with no body runs `OnSuccess`.
- `X10.3` behavior: a network failure runs `OnError()` and its `Finally` hides the loader.
- `X10.4` behavior: a form check that fails sends nothing and runs no step after the request.
- `X10.5` behavior: a server's validation errors show at the inputs whose binding names they key, on both hosts.
- `X10.6` behavior: `OnAllSettled` runs once, after the slower of two parallel requests answers.
- `X10.8` behavior: a GET whose policy retries 503 answers the second attempt's success, shows its loader once
  and runs its `Finally` once.
- `X10.9` type: a POST's retry policy that does not say the POST is repeatable retries nothing: new:N10.3.
- `X10.7` behavior: on net48 under IIS, a 400 with validation errors shows each message at its input; the
  `verify-net48` run proves it.
- `X10.10` behavior: a nullable member whose condition fails is not in the body the request sends, and the
  endpoint answers with the member's starting value, not null.
- `X10.11` behavior: a chained request whose condition fails is not sent, and the first request's success
  steps and the steps after it ran.
- `X10.12` behavior: a request that writes only its method, URL, form and success shows a 400's errors at its
  own form, through the defaults' route.

## Q11. Lanes and time

**Decision.**
- D11.1 Sync is the default. A pipeline runs its steps in authored order in the turn its trigger fires, and
  waits only at an awaited step. A pipeline with no awaited step finishes in that turn.
- D11.2 The awaited steps are closed, and the C# marks each as awaited where it writes it:
  - an HTTP send, each `Parallel` branch's included, and the send whose HTML `Into` loads (the HTTP,
    parallel HTTP and partial-injection lanes);
  - a user decision, `Confirm`;
  - a hub's `invoke` and `send`, the hub's async methods (the remote-trigger lane; a server push or a hub
    message starts a run as a trigger, D11.6);
  - a method declared with `MethodAsync`, called as a step or read as a source, on a component, a plugin, an
    event payload or a returned object.
- D11.3 Sync and async are decided by the declaration, never by what the browser finds (the owner,
  2026-11-06). Any JavaScript API can return a promise: 120 documented-public EJ2 methods declare one
  (`probe:ej2-methods-returning-promise`).
  - A property, a data member and a method declared with `Method` are sync. A promise where one of them
    was declared stops the pipeline with one logged error (Q4).
  - A method declared with `MethodAsync` is awaited every time it runs. A rejection stops the pipeline,
    logs one error and runs the enclosing request's `Finally`. The value it resolves to is read as data
    (Q4).
  - Today a call that was not awaited and gets a promise continues and logs only the rejection; the
    rewrite refuses that meaning.
  - A component declaration's `Method` and `MethodAsync` agree with the vendor's declared return type,
    checked by the vendor-drift rule (Q17, Q20).
- D11.4 The steps before a handler's first awaited step run inside the vendor's callback, so the vendor
  sees what they write to the event's payload. A write to the trigger's payload after an awaited step is
  refused where the view renders: the vendor read the payload when its callback returned. To keep a
  drawer open while the user decides, the handler writes `Cancel` first, then asks, then calls the
  drawer's close method on yes. Reading the payload after an awaited step stays allowed.
- D11.5 The order is closed:
  - The steps after an awaited step run after it settles.
  - Two handlers of one event run in the order the plan subscribed them, each up to its first awaited
    step before the next starts. After that, their runs go on as their awaits settle, and a stop ends
    only its own handler (Q4).
  - `Parallel` starts its branches in authored order in one turn, each up to its first awaited step. The
    steps after `Parallel` run once, after every branch settled, and a stop in a branch ends only that
    branch (D10.10).
- D11.6 A request is a step, so it runs wherever a step runs:
  - in a handler of any trigger: on load, an element's or a component's event, a declared app event, a
    dispatch, a server push, a hub message;
  - in a route, a `Confirm` branch, the block `Chained` runs, a `Parallel` branch, `WhileLoading` and
    `Finally`.

  A run belongs to the owner of the handler that started it: the page, or the slot whose plan holds the
  handler (Q12). A plugin's member is read, written and called in the same blocks, the request's included,
  through its typed declaration (Q5), and its async methods are awaited steps (D11.2).
- D11.7 A handler's policy decides what a trigger does while that handler's previous run is in flight. The
  policy belongs to the handler, not to a request.
  - `overlap`, the default, as today, starts another run.
  - `replace` aborts the run in flight and starts the new one, so a cascade's or a search's late answer
    never overwrites a newer one. The aborted request runs no route, and its `Finally` runs once.
  - `skip` starts nothing while a run is in flight, so a save clicked twice sends once.
- D11.8 Dispose ends its owner's runs first, then releases what the owner holds.
  - A request in flight is aborted through its fetch's abort signal and runs no route. Its `Finally` runs
    once. A retry's wait ends (D10.15).
  - An await the browser cannot abort (a `MethodAsync` promise, `Confirm`, a hub's `invoke`) settles
    later, and its settling runs nothing.
  - No route and no step after runs for a disposed owner.
  - Every `Finally` runs before the owner releases its listeners, components and content, while its
    elements are still on the page. So a `Finally` that hides the page's loader, or re-enables a button in
    the slot, finds what it names. A `Finally`'s steps after its own first awaited step do not run for a
    disposed owner.
- D11.9 No step waits for time. The only waits the core owns are a retry's delay and a `Retry-After`
  (D10.14). A component's own timers, such as a toast's or a loader's timeout, are its properties.

**Schema nodes.** `do` and `let` with `await`, `all`, `try`, and the handler's policy, a field of `handler`
(Q19). The abort signal is the run's, not the plan's: `HttpSend` carries none.

**Checker.**
- `Lanes.tla:DisposedRunsNothingMore`
- `Lanes.tla:FinallyRunsWhenAborted`
- `Lanes.tla:FinallyRunsOnce`
- `Lanes.tla:FinallyRunsBeforeRelease`
- `Lanes.tla:ReplacedRunRunsNoRoute`
- `Lanes.tla:SkippedTriggerStartsNothing`
- `Lanes.tla:AllSettledAfterEveryRequest`
- `Lanes.tla:BranchStopEndsOnlyItself`
- `Lanes.tla:PayloadWritesInTheVendorsTurn`
- `Absence.tla:UnawaitedPromiseStops`
- `Absence.tla:PromiseWhereValueStops`
- `Absence.tla:StopEndsOnlyItsHandler`
- `probe:ej2-methods-returning-promise`

**Doors.**
- kept: `Method`, `Method<T>`, `MethodAsync`, `MethodAsync<T>`, `Confirm`, `Parallel`, the hub's `invoke` and
  `send`.
- `N11.1` new: a handler's policy, `overlap`, `replace` or `skip` (D11.7); its door's form goes to gate 2.
- `N11.2` new: the refusal at render of a write to the trigger's payload after an awaited step (D11.4).

**Evidence.**
- A block runs sync until a statement returns a promise (`Alis.Reactive.Assets/runtime/grammar/execute.ts:16-21`).
- A call that was not awaited and gets a promise continues and logs the rejection
  (`Alis.Reactive.Assets/runtime/grammar/execute.ts:41-47`).
- `all` waits for every block with `Promise.allSettled` (`Alis.Reactive.Assets/runtime/grammar/execute.ts:66-81`).
- Handlers subscribe in plan order, and a handler's completion is never returned to the vendor
  (`Alis.Reactive.Assets/runtime/plans/start.ts:43-66`).
- The awaited steps today: the send (`Alis.Reactive/PlanModel/Requests/RequestStep.cs:101`), `Confirm`
  (`Alis.Reactive/PlanModel/Conditions/Condition.cs:504`), `MethodAsync` (`Alis.Reactive/Components/JsObject.cs:188`,
  `Alis.Reactive/Components/JsObject.cs:252`), and the hub's `invoke` and `send` (`Alis.Reactive.Assets/runtime/realtime/hub.ts:128-135`).
- A payload a vendor reads in its turn: `Cancel` on the drawer's close event (`Alis.Reactive.Native/AppLevel/NativeDrawer/NativeDrawer.cs:61`).
- Dispose runs the owner's disposers and ends no run in flight (`Alis.Reactive.Assets/runtime/objects/owner.ts:32-40`);
  `grep -rn AbortController Alis.Reactive.Assets/runtime` prints nothing at 65fdf118d.
- A slot announces its unmount before its content is replaced (`Alis.Reactive.Assets/runtime/plugins/slot.ts:24-38`).
- B15: a declaration drifts from its vendor's return type today: `FusionGrid` declares its exports with `Method()`
  (`Alis.Reactive.Fusion/Components/FusionGrid/FusionGrid.cs:207-213`), while the grid's `.d.ts` declares
  `excelExport` and `pdfExport` returning a Promise. Under D11.3 each such call stops its pipeline until the
  declaration says `MethodAsync`.
- G9: an app event is declared once and shared by its dispatcher and its listener
  (`Alis.Reactive/PlanAuthoring/Events/AppEvent.cs:41-43`), so a dispatch is a trigger like any other (D11.6).

**Defects.**
- `X11.1` refusal: a handler that writes `Cancel` after a `Confirm` is refused where the view renders with
  "A write to the event's payload after an awaited step reaches no vendor: write it before the first
  request, Confirm or async method."
- `X11.2` behavior: a drawer whose close handler writes `Cancel` and then asks stays open while the dialog
  asks, and closes on yes.
- `X11.3` behavior: a method declared with `Method` whose vendor returns a promise stops its pipeline with
  one logged error, and the step after it does not run.
- `X11.4` behavior: a `MethodAsync` that rejects stops its pipeline, and its request's `Finally` hides the
  loader.
- `X11.5` behavior: a slot unloaded while its request is in flight runs no route of that request, and the
  page's loader its `WhileLoading` showed is hidden.
- `X11.6` behavior: under `replace`, a cascade whose first answer arrives after the second shows the
  second's items.
- `X11.7` behavior: under `skip`, a save clicked twice while its request is in flight sends one request.

## Q12. Ownership

**Decision.**
- D12.1 The owners are closed: the page and a slot. A slot is an element some plan loads into, named in the
  plan by its container id (the `alis.slot` key), never guessed from the page. A plan is not an owner. Each
  plan belongs to one owner: the slot whose container is the innermost around the plan's script, else the
  page. The page is never released.
- D12.2 What a plan asks for belongs to its plan's owner:
  - its handlers' listeners, on any element or object, the page's included;
  - its holds on a hub or a stream;
  - its forms' field registrations, with their rules;
  - its runs (Q11).
- D12.3 What HTML carries belongs to the owner of the innermost slot around it, else the page:
  - the components mounted in it, which construct on `alis:mount` and destroy themselves on `alis:unmount`;
  - the slots whose containers it holds;
  - the plans whose scripts it holds, content the server rendered inside a slot's container included. Such
    content belongs to that slot from boot, so the slot's first load or unload releases it.
- D12.4 A slot's load replaces its content, and its unload empties it. Before either, the slot releases every
  owner whose container is inside its content, innermost first, then itself. An owner's release runs in a
  closed order:
  1. it ends its runs, and each `Finally` runs (Q11, D11.8);
  2. it announces the unmount of its content;
  3. it removes its listeners;
  4. it releases its holds;
  5. it drops its field registrations.

  Then the content is replaced. A slot's next load starts a new owner, which subscribes once, so no listener
  piles up across loads.
- D12.5 Release removes exactly what the owner holds, and nothing another owner holds. A listener is removed
  only while its element still holds, at the vendor's object path, the object the listener was added to. A
  component its host destroyed took its listeners with it, and the owner calls nothing on it. EJ2's destroy
  takes the component off its element (`probe:ej2-destroy-removes-instance`), so the rule names no vendor.
  Main's check of Syncfusion's `isDestroyed` in the runtime is refused for that reason.
- D12.6 A hub or a stream is held by every owner whose plans name it. The connection opens at the first hold
  and closes at the last release, and its retry offer goes with it. A step reaches only a held connection.
- D12.7 Live validation is one capture-phase listener per page. An input an event reaches is checked by the
  field registrations of the live plans that name it. Nothing is wired per element, so nothing per element
  is released. A field registration leaves with its plan's owner, so a slot's required field no longer holds
  the page's save once the slot unloads. What a check reads for a member whose input is gone is Q16's.

**Schema nodes.** The plugin data `form`, `hub` and `slot`, and `handler`. Ownership is not a node: a plan's
owner is where its script is, and an owner's holds are what its plans name.

**Checker.**
- `Ownership.tla:NoSlotOutlivesItsContainer`
- `Ownership.tla:ListenersDoNotAccumulate`
- `Ownership.tla:BootContentBelongsToItsSlot`
- `Ownership.tla:RegistrationsDoNotAccumulate`
- `Ownership.tla:EveryLoadedFieldRegistered`
- `Ownership.tla:RulesLeaveWithTheirOwner`
- `Ownership.tla:HubKeptWhileHeld`
- `Ownership.tla:OfferOnlyWhileHeld`
- `Ownership.tla:NoUnsubscribeOnDestroyed`
- `Lanes.tla:FinallyRunsBeforeRelease`
- `probe:ej2-destroy-removes-instance`
- `probe:ej2-destroy-sets-is-destroyed`

**Doors.**
- kept: `Into(element)` and the slot's load and unload, `Html.RenderPlan`, and the hub's and stream's triggers.
- No door names an owner: a developer chooses where a plan renders, and ownership follows.

**Evidence.**
- P3, each row with the meaning that answers it:
  - 885652213 guessed a slot from `container.id.length > 0`, and 527138248 declared slots in the plan: D12.1
    (`Alis.Reactive/PlanModel/Writing/Scope.cs:234`).
  - 0b47eafd7 let one listener's abort stop a shared hub, and d07a89928 made holds plan data: D12.6
    (`Alis.Reactive.Assets/runtime/realtime/hub.ts:95-107`).
  - bc45c3b9e left a retry offer after unload: D12.6 (`Alis.Reactive.Assets/runtime/realtime/hub.ts:101-107`).
  - 6478b4a4a, e02294ae4 and a168e014d left rules and wiring behind after a reload, with e02294ae4's "orphaned
    rules are not removed" reversed by a168e014d: D12.2, D12.7
    (`Alis.Reactive.Assets/runtime/plugins/form/form.ts:232-245`).
  - d6251825e found listeners added on every slot load: D12.4, D12.7
    (`Alis.Reactive.Assets/runtime/plugins/form/form.ts:206-221`).
  - f26146b8f released the slots nested in replaced content, innermost first: D12.4
    (`Alis.Reactive.Assets/runtime/plugins/slot.ts:56-63`).
  - e83040182 gave server-rendered content its slot's owner at boot: D12.1, D12.3
    (`Alis.Reactive.Assets/runtime/root.ts:58-77`, `Alis.Reactive.Assets/runtime/root.ts:84-89`).
  - 893ed624a on main checks `isDestroyed`, and acd5f47fe answers it with `stillHolds`: D12.5
    (`Alis.Reactive.Assets/runtime/objects/vendor-driver.ts:16-18`).
- An owner runs its disposers in reverse and knows nothing of runs (`Alis.Reactive.Assets/runtime/objects/owner.ts:32-40`).
- A slot announces the unmount of its content before replacing it (`Alis.Reactive.Assets/runtime/plugins/slot.ts:24-38`).

**Defects.**
- `X12.1` behavior: after the outer slot unloads, a click on a page button that an inner slot's plan listened to
  runs nothing.
- `X12.2` behavior: after a slot loads three times, a click on its button runs its handler once.
- `X12.3` behavior: a field a reloaded slot renders shows its message on blur.
- `X12.4` behavior: after a slot with a required field unloads, the page's form saves.
- `X12.5` behavior: two slots listen to one hub; after one unloads, the other still shows the server's pushes.
- `X12.6` behavior: unloading the slot whose hub the server hung up on hides the retry offer.
- `X12.7` behavior: a slot whose component its host destroyed unloads with no console error, and the next load
  listens.
- `X12.8` behavior: content the server rendered inside a slot's container stops listening at the slot's first
  load.

## Q13. Where state lives

**Decision.**
- D13.1 Browser state lives in exactly one of four places, and each concept has one:

  | Concept | Lives in |
  |---------|----------|
  | the table of inputs by element id, each entry under the owner that contributed it | the page |
  | a hub's or a stream's connection, by URL, with its holders (Q12) | the page |
  | the app-level objects (loader, toast, confirm, drawer), by fixed id | the page |
  | the one live-validation listener (D12.7) | the page |
  | the form's field registrations, by binding name, item fields by `[*]` | the form |
  | the inputs the form sends or checks: those inside its element at that moment | the form |
  | the server's messages, each at the input inside the form that binds its key, else the summary | the form |
  | the summary, `{formId}_validation_summary`, one per form | the form |
  | listeners, holds, registrations and runs | the owner (Q12) |
  | a plan's culture and time zone, written by C# for the view (D3.11, D3.12) | the owner, by its plan |
  | the argument, the attempts, the response and the route of one request | the request, until it settles |

- D13.2 A plan's id keys nothing: it names the plan in traces only, and many plans of a page may share it.
- D13.3 A read of a member by its binding name belongs to a form. It reads the input inside the form's element
  that binds the name, as the form sends it. A page-wide read by binding name is refused: two views may bind
  one name, so on a page a name names no single input. That ends the chain of guesses that read the last
  contributed input, then the plan's own. A read of a member's input outside a form is the component read
  by its element id, which Q14 derives the same way in every view that renders the member.
- D13.4 Two inputs inside one form that bind one scalar binding name stop the request or the check that reads
  them, with one logged error that names both element ids. Form data binds the first value and JSON the last
  on both hosts (`probe:core-form-duplicate-name`, `probe:core-json-duplicate-name`,
  `probe:mvc5-form-duplicate-first`, `probe:mvc5-json-duplicate-last`), so the browser cannot know which one
  the endpoint binds. A list's inputs carry indexed names and are not duplicates.
- D13.5 A form's rule for each item is written once, by binding name with `[*]`. It checks every item row inside
  the form when the form checks, whoever rendered the row: the page, a list partial, or a slot's content.
- D13.6 No plan renders a summary of its own. The summary is rendered once per form by the door that renders
  the form (Q14), and a form's messages outlive a slot that joined the form and left.

**Schema nodes.** The plugin data `form` (its field registrations and summary id) and `model` (the inputs table),
and `request` state, which the schema does not hold: a request's state is its run's.

**Checker.**
- `Ownership.tla:RulesLeaveWithTheirOwner`
- `Ownership.tla:RegistrationsDoNotAccumulate`
- `probe:core-form-duplicate-name`
- `probe:core-json-duplicate-name`
- `probe:mvc5-form-duplicate-first`
- `probe:mvc5-json-duplicate-last`

**Doors.**
- kept: `Form()`, `Form(formId)`, `Form(fields)`, `Validate<TValidator>(formId)`, `ValidationErrors(formId)`.
- `N13.1` new: a form's typed read of a member as the form sends it, the one read by binding name (D13.3); its
  form goes to gate 2. It carries `p.Bound(m => ..)`, whose page-wide read by name is refused.

**Evidence.**
- P2, each row with the meaning that answers it:
  - 053b50d84 made one page-wide table of inputs by id, each entry under its owner: D13.1.
  - 34c99d608 checks every row of an item rule, whoever rendered it: D13.5.
  - 01d00553a places a server message at the input inside the form that binds its name: D13.1.
  - a893ca7aa, 0d32aa743 and f38d0d278 on main rendered and found a summary per plan, and 76ca89036 and
    532de0002 replaced it with one per form, the plan id keying nothing: D13.2, D13.6.
  - 92dce45ba read a member page-wide by its input's id, a019367c5 by name, taking the input contributed last,
    and fda82e5b4 the plan's own input when it rendered exactly one, else as before: D13.3.
- The read takes the last matching input (`Alis.Reactive.Assets/runtime/plugins/model.ts:95-100`).
- Each entry of a form's registrations leaves with its owner (`Alis.Reactive.Assets/runtime/plugins/form/form.ts:232-245`).

**Defects.**
- `X13.1` behavior: two partials of different models each render an input that binds `Value`, each in its own
  form; each card's save sends its own value.
- `X13.2` behavior: a form holding two inputs that bind one name sends nothing on save, and the console names
  both element ids.
- `X13.3` behavior: a row a slot loads into the form is checked by the page's item rule when the form saves.
- `X13.4` behavior: a server message for a member an editor template rendered shows beside that input.
- `X13.5` behavior: two forms on one page keep their own summaries, and a form's summary stays after a slot that
  joined it unloads.

## Q14. MVC partials and ids

**Decision.**
- D14.1 Each view's plan is its own. It is started in the view with the view's model, in a root view, a
  partial, an editor template or a slot's content.
  - Nothing carries plan state between views on the server: no `HttpContext.Items`, no `ViewData`, no
    static. ResolvePlan and the plan primitives were designed to stop poisoning the HTTP request (the owner,
    2026-11-06).
  - The redesign's per-request record (`Alis.Reactive/Razor/RenderedPage.cs:29`) is refused for that
    reason, and each of its uses moves to D14.6 to D14.8.
- D14.2 Plans compose in the browser without merging. Each plan belongs to its owner (Q12), and it reaches
  what another view rendered through the page's input table by element id, or through the form by binding
  name (Q13).
  - Main merged the plans of one plan id: components replaced by key, behaviors appended in order.
  - The rewrite carries each part. A component another view renders is located by its element id, with
    the vendor data of the reading plan's own typed declaration. Behaviors stay their own plan's. Rules and
    messages are the form's.
  - So the plan id keys nothing (D13.2).
- D14.3 A member has one binding name and one element id in every view that renders it:
  - The binding name is MVC's full field name under the view's `TemplateInfo` prefix (`probe:core-template-prefix-name`,
    `probe:mvc5-template-full-id`).
  - The element id is the class that declares the member, then MVC's id of the full name: `{Namespace_Class}__{Path}`
    (`probe:core-sanitized-id`). The declaring class is the one the expression tree's member names.
  - So the page, a validator, an `EditorFor` template, a list-item partial and a partial for a member reach
    one id without knowing each other, and two views of different models that bind one name keep two ids.
  - A lambda that names no member (`m => m`), and a member an interface declares, are refused.
  - No view on either line pairs MVC's `LabelFor` or `IdFor` with a framework input (`git grep` finds none at
    623dd4a27 or 65fdf118d), so the framework's id needs no agreement with them, and a view names the id by
    `IdGenerator.For`.
  - The framework controls every input's id (the owner, 2026-11-06): without it the browser guesses which
    input a rule, a message or a read means, and views of different partials that share an id conflict.
  - Reading the declaring class from the expression tree goes to gate 1's list of reflection exceptions.
- D14.4 A partial is rendered for a member by one typed door, `Html.PartialFor(m => m.Member, view)` (N14.1), on
  both hosts.
  - The member's value is the partial's model, and a new `TemplateInfo` carries the member's full binding
    name as the prefix.
  - A list item, `m => m.Members[i]`, takes `Members[i]`, a position (Q1). A dictionary entry,
    `m => m.Doses[key]`, takes `Doses[key]`.
  - It replaces the idiom of copying `ViewData` and setting `TemplateInfo.HtmlFieldPrefix` (16 view lines at
    65fdf118d). On net48 that idiom moves the view's own prefix, since a copy shares `TemplateInfo`
    (`probe:mvc5-viewdata-copy-shares-template-info`); on net10 a copy has its own
    (`probe:core-viewdata-copy-template-info`).
- D14.5 A dictionary entry keyed by a number has no JSON form, since JSON reads a number in the prefix as a list
  position. A JSON request whose body carries a member under such a prefix is refused where the view renders,
  and form data sends it.
- D14.6 The refusals of partials and ids are closed. Within one plan they are refused where the view renders.
  Across plans, the page meets them where it composes the plans, and each is one error there.
  - Composition is a real boundary: the plans were rendered apart, no plan can state what another holds,
    and no server object sees every view of a page (D14.1).
  - That moves two refusals from render time to the browser, a gate-1 line of its own.
  - At render, within one plan:
    - R14.1 an input whose lambda names no member, or a member an interface declares;
    - R14.2 an input under a prefix its view took after its plan started;
    - R14.3 an element id the plan renders twice;
    - R14.4 a step that sets the text or HTML of an element the same plan loads or unloads as a slot;
    - R14.5 a JSON request over a number-keyed dictionary entry (D14.5).
  - At composition, in the browser:
    - R14.6 a plan holding an input id a live plan already holds is not started. One error names the id and
      both plans' views. A slot's content that renders an id the page holds is the same case.
    - R14.7 a step that sets the text or HTML of an element a live plan holds as a slot stops, with one error
      naming the slot.
- D14.7 A plan is a block in its view: the plan door opens the block and the block's end writes the plan
  (N14.2).
  - The plan is a variable of the block, so an input outside the block cannot name it, and a plan that is
    never rendered cannot be written.
  - A block written whole inside a partial's `@section` renders nothing, inputs included, since MVC drops a
    partial's sections on both hosts (`probe:core-partial-section-dropped`,
    `probe:mvc5-sections-checked-in-layouts-only`). The page shows that defect.
  - A refusal by render order is refused: three commits guessed it, narrowed it and removed it.
- D14.8 A form is a block in its view as well (N14.3). It renders the form's element and the form's one
  summary, so no record of summaries rendered is needed (D13.6).

**Schema nodes.** None new: the plan's object locators by element id, the plugin data `model` (inputs by id) and
`slot` (container ids).

**Checker.**
- `probe:core-template-prefix-name`
- `probe:mvc5-template-full-id`
- `probe:core-sanitized-id`
- `probe:core-viewdata-copy-template-info`
- `probe:mvc5-viewdata-copy-shares-template-info`
- `probe:core-partial-section-dropped`
- `probe:mvc5-sections-checked-in-layouts-only`
- `Ownership.tla:BootContentBelongsToItsSlot`

**Doors.**
- kept: `Html.ReactivePlan`, `Html.InputField`, `IdGenerator.For`, and MVC's own `Html.EditorFor`.
- `N14.1` new: `Html.PartialFor(m => m.Member, view)`, the typed partial for a member, on both hosts.
- `N14.2` new: the plan as a block of its view, whose end renders it; its form goes to gate 2.
- `N14.3` new: the form as a block of its view, which renders the form's element and its summary; its form goes
  to gate 2.

**Evidence.**
- P1, each row with the meaning that answers it:
  - 31aacf71e fell back to an id with no prefix for a rule on a partial's field: D14.3.
  - 0090a6adc gave the plan its view's prefix, which only `Html.InputField` knew: D14.3.
  - aba543d9c gave two cards of one partial one id: D14.3, D14.4.
  - a82264ab8 scoped the id by the declaring class, so a page reaches its member's template: D14.3
    (`Alis.Reactive/Inputs/IdGenerator.cs:50-58`).
  - 45259f0eb refused `m => m`: R14.1.
  - a0a95e0bd and 8046bc4c5 named a partial's fields and sends without the prefix: D14.3.
  - 57f173f58 read `Members[1]` as one member: D14.4, Q1.
  - b24c65775 documented a number-keyed dictionary entry: D14.5, R14.5.
- P4, each row with its home:
  - c5421ac4b refused a slot's content set by a step: R14.4 within a plan, R14.7 across plans.
  - 58c12947c refused an element id rendered twice on a page: R14.3 within a plan, R14.6 across plans.
  - eecfb7336 refused a prefix taken late: R14.2, and N14.1 takes the idiom away
    (`Alis.Reactive/Razor/InputFieldExtensions.cs:87`).
  - f3c2a6e10, a0bd5f0d2 and 2227c4eb5 refused, narrowed and removed a plan that never renders: D14.7.
- Main's partial plan is a new plan in partial scope (`Alis.Reactive/Razor/Extensions/PlanExtensions.cs:58-62`),
  merged in the browser by plan id (`Alis.Reactive.Assets/runtime/lifecycle/merge-policy.ts:2-31`).
- The redesign keeps a per-request record of ids, summaries, slots and contents set
  (`Alis.Reactive/Razor/RenderedPage.cs:29`).

**Defects.**
- `X14.1` type: an input outside its plan's block does not compile: new:N14.2.
- `X14.2` type: a partial for a member takes the member as a typed lambda, so no prefix is written as text:
  new:N14.1.
- `X14.3` refusal: an input bound to `m => m` is refused with "Input 'm => m' names no member of {model}: bind
  a member of the view's model, or render the member with Html.EditorFor from the view that holds it."
- `X14.4` refusal: an input after the view's prefix moved is refused with "The field prefix of {view} changed
  from {old} to {new} after its plan started, so input '{name}' would post under a name its plan does not send
  or check."
- `X14.5` refusal: an id rendered twice by one plan is refused with "Input '{name}' renders element id '{id}' a
  second time in this plan: render the field once, or each copy for its own member with Html.PartialFor."
- `X14.6` refusal: a JSON request over a number-keyed dictionary entry is refused with "{member} sits under
  '{prefix}', a dictionary entry keyed by a number, which JSON reads as a list position: send this request
  with AsFormData()."
- `X14.7` behavior: a slot's content that renders an input id the page holds runs none of its handlers, and the
  console names the id and both views.
- `X14.8` behavior: an `EditorFor` template's input of `NextOfKin.Name` shows the page's server message for
  `NextOfKin.Name` beside it, and the page's rule checks it.
- `X14.9` behavior: one partial rendered by `PartialFor` for two members posts each card's own values.
- `X14.10` behavior: a list-item partial under `Members[1]` sends its inputs at position 1 in a JSON body.
- `X14.11` behavior: on net48 a partial rendered by `PartialFor` leaves the page's own inputs posting under their
  names; the `verify-net48` run proves it.

## Q15. Templates

**Decision.**
- D15.1 A template is plan data, never vendor template text. C# writes it as a typed tree, and the framework's
  template plugin turns the tree into a function from an item to DOM nodes, which the component's property
  takes.
  - EJ2 takes such a function. The text nodes it returns stay text and its elements render as they are
    (`probe:ej2-template-function-text-node`, `probe:ej2-template-function-elements`).
  - So one template form serves every vendor that takes a function template, and the native reference
    renders its cells with the same plugin.
- D15.2 Syncfusion's string engine is refused as the framework's template form, for what it does:
  - it writes a data value into the HTML unencoded, so markup in data becomes live elements
    (`probe:ej2-template-unencoded`);
  - it throws on a member past a missing holder (`probe:ej2-template-missing-holder`), where Q4 reads nothing;
  - it evaluates its conditions as JavaScript compiled with `new Function` (`probe:ej2-template-condition`).
- D15.3 A template's tree is closed. Its nodes are elements of a closed set (text, span, div, badge, icon,
  image, link, event button), each with classes and attributes; text and attribute values read from the
  item; and a condition with a then and an else branch.
  - A value is read from the item with the one read (Q2), its members optional, so a member past a missing
    holder reads nothing and shows nothing (Q4).
  - Text is written with `textContent` and attributes with `setAttribute`, so data is never markup. A link's
    address is read as a URL and refused, reading nothing, when its scheme is not `http`, `https`, `mailto` or
    `tel`.
  - Member names are the data's JSON names under the app's options (D2.9). The camel case the template writer
    hard-codes today is refused.
- D15.4 A template's condition is the one condition core (Q8) over the item, written with the one syntax
  (D8.6), with a branch that is a block (D8.7). The template's second condition syntax, a lambda of the item,
  and its third translator are refused.
- D15.5 An event button raises a declared app event with a payload read from the item. The payload is data,
  and the event's handlers are plan handlers like any other (Q11).
- D15.6 Developer-written markup stays: `Raw(html)` takes static HTML the developer writes, never a read from
  the item.
- D15.7 The typed HTML builder is the template's door, kept whole and grown (the owner, 2026-11-06): its
  element doors, typed member reads and nesting stay as a developer writes them today, and only what it
  compiles to changes, from Syncfusion's engine text to the typed tree of D15.3. Since the tree serves any
  component that takes a function template, the builder belongs to the core, and Fusion and Native use it
  as an app does.

**Schema nodes.** `fn` (the item to the tree), `record`, `list`, `get`, `as`, `op`, the plugin `alis.template`, and the
template tree as a typed plugin argument (Q19), so the grammar gains no node.

**Checker.**
- `probe:ej2-template-unencoded`
- `probe:ej2-template-condition`
- `probe:ej2-template-missing-holder`
- `probe:ej2-template-function-text-node`
- `probe:ej2-template-function-elements`
- `Absence.tla:DataAbsentReadsNothing`

**Doors.**
- kept: `FusionTemplate.Create<T>()` and its element doors (`Text`, `Span`, `Div`, `Badge`, `Icon`, `Img`, `Link`,
  `EventButton`, `Raw`), the template properties of each component.
- `N15.1` new: the template tree, a typed plugin argument of `alis.template`, and the plugin that renders it.
- `N15.2` new: the template's condition, written with the one condition syntax over the item; its form goes to
  gate 2. It carries `When(Expression<Func<TModel, bool>>)` and `FusionConditionalBuilder`, whose lambda form is
  refused.
- `N15.3` new: the typed HTML builder in the core, the one template door for every component (D15.7).

**Evidence.**
- The builder renders a string that Syncfusion's template engine compiles
  (`Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:25`), and each element door adds one more piece of that text
  (`Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:78-310`).
- The template writer names every member in camel case (`Alis.Reactive.Fusion/Templates/FusionTemplateExpression.cs:44`)
  and compiles a captured value with `Compile().DynamicInvoke()` (`Alis.Reactive.Fusion/Templates/FusionTemplateExpression.cs:85-90`).
- The template's condition is a lambda of the item (`Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:322-337`),
  translated to the engine's text by a third translator (`Alis.Reactive.Fusion/Templates/FusionTemplateExpression.cs:22-25`).

**Defects.**
- `X15.1` behavior: a grid row whose name holds `<img src=x onerror=alert(1)>` shows that text in its template
  cell, and no image element renders.
- `X15.2` behavior: a row with no address shows an empty cell where the template reads the address's city, and
  the grid's other rows render.
- `X15.3` behavior: a template condition on the item's status shows the badge for an active row and the other
  branch for an inactive one.
- `X15.4` behavior: a template's event button raises its app event with the row's id, and the page's handler of
  that event runs.
- `X15.5` behavior: with the app's JSON options naming members in Pascal case, a template reads `Name` from a
  row the server sent.
