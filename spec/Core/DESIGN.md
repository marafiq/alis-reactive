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
- D1.5 `each` appears only in a form's registration paths (Q16, Q19). No read, write, call or subscription carries
  `each`; a projection over a list is `map` (Q6).
- D1.9 On a read, a write or a call, an index step and a member step of the same digits reach the same item:
  JavaScript reads `a[1]` and `a["1"]` alike. POC 4 planted the position of `LiveObjects<T>.At(1)` as the member
  `"1"`, and the test that writes through it passed. The index step has its meaning where a body is built: an
  input's body path writes the body a form sends, and a position there makes an array where a member makes an
  object (`Alis.Reactive.Assets/runtime/plugins/model.ts:156-162`, `assignAt`). POC 10, inputs, proves it in the
  browser. If POC 10 builds the body with no path, the index step leaves the grammar and a position is a member step.
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
  into the plan once: its name, its decimal separator, its group separator and its collation, the tag the
  browser's collator orders text by (the culture's name, `en` for the invariant culture, D6.13). Every
  culture-dependent conversion and comparison in the browser uses it, and none reads the browser's locale:
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
- D4.2 Data reads its start when absent (Decision 1, the owner, 2026-11-06): a member a present holder lacks reads the
  value System.Text.Json binds for it under the app's options, on both hosts: the contract that wrote the data and
  names its members (D2.9). C# binds the path down to the member with the member absent and reads the rest of the
  path on that object, so a populated member reads its holder's own start and a replaced one a new object of its
  type (`spec/NOTES.md`, the second start probe). A holder that holds null or reads nothing reads nothing, and so does
  every member past it. A data read is the `data` node (schema.md): each step carries what it reads when its member is
  absent, `start` (the value), `nothing` (a holder on the rest of the path starts null) or `unbound`, which stops the
  pipeline: the binding cannot create a holder on the path (a private constructor, an abstract type, an interface),
  or a holder on it is polymorphic, whose start hangs on the discriminator the data holds. A present holder that lacks
  a member its type requires (`[JsonRequired]`, `required`, a constructor parameter under
  `RespectRequiredConstructorParameters`) stops every read through it; a holder that is not an object stops. A
  member is present when the value holds it, other than what every object inherits (`constructor`), with a value
  other than `undefined`. This covers a member of data, data past a declared member (a field of the value a property
  holds, of each item of a list it holds) and the field a find returns. A form's check reads the starts the endpoint
  that binds its body leaves, by host and format (D16.6); a data read follows the contract of the data.
- D4.3 A read that yields a promise where a value is declared stops the pipeline. A method declared
  `MethodAsync` is awaited, and its settled value is its result. A rejected promise stops the pipeline. A
  `MethodAsync` whose vendor returns a value that is not a promise is awaited as that value, since the declaration
  decides (D11.3). The result binds at its line (D5.11); an async method called inside a list's lambda, whose
  operation reads every item in one turn, is refused where the view renders (X4.12).
- D4.4 A call of a method declared to return nothing that returns a promise stops the pipeline after the call,
  naming `MethodAsync`. There is no call whose promise nobody awaits: the stop follows it, and a later rejection of
  that promise adds no second error.
- D4.5 A missing value set as an element's text or HTML shows nothing; a missing class name changes no class.
- D4.6 A route value that reads nothing stops its request before it is sent, and its `Finally` runs. C# writes
  this as a guard before the send and the statement `stop(reason)` (Q19).
- D4.7 A method called inside a condition runs only when its term is reached: `and`, `or` and `cond` evaluate
  their terms in order and stop at the first that decides, and an `ElseIf`'s condition runs only when no branch
  before it ran. C# writes a method's result as a `let` at its line (D5.11). A result called after the block's
  previous step and read once by the next step, under a term reached later (the right term of `and`, `or` or
  `coalesce`, or a branch of `cond`), is that term's own call: its
  `let` binds with `binds: "read"`, and the call runs where its read is reached. An `ElseIf` is the `else` block of
  the `if` before it, holding its condition's results at their lines and an `if` of its own, so its calls, awaited
  ones included, run when it is reached (D8.6). An async method's result binds only at its line, so one called under
  a term reached later is refused where the view renders (X8.16).
- D4.9 A value JavaScript holds as `undefined` reads as nothing wherever it is read: a member that holds it, a
  method that returns it (`find` when no item matched). It never reaches a write as `undefined`; a write of
  nothing writes null (D4.5).
- D4.8 A stop ends its own handler only. Other handlers, and the other blocks of a `Parallel`, run on (Q10). A stop
  never reaches the vendor that raised the event: its listener returns, and the vendor goes on with the work it does
  after its listeners.

**Schema nodes.** `get`, `call`, `set` with `members`; `data` with its steps; `do` and `let` with `await`; `try`; `op`
(`and`, `or`, `cond`, `coalesce`); the new statement `stop`.

**Checker.**
- `Absence.tla:RequiredAbsentStops`
- `Absence.tla:DataAbsentReadsItsStart`
- `Absence.tla:NullHolderReadsNothing`
- `Absence.tla:LackingRequiredStops`
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
- `X4.1` type: a method that returns a promise is declared apart from one that returns a value: its declaration
  names `MethodAsync`, never `Method` (`Alis.Reactive/Components/JsObject.cs:252`). Both results are a method's
  result source, because the await happens at the result's line (D5.11) and the binding is a value after it; a
  source type of its own would refuse nothing a list's lambda or a value position could not take.
- `X4.2` behavior: a component that lacks a declared property stops the pipeline at the read, and the steps after
  it do not run.
- `X4.3` behavior: an event payload that lacks a member its declaration names stops the pipeline (D4.1, D5.3), and a
  field of data the payload holds that the data lacks reads its start (D4.2).
- `X4.4` behavior: a route value that reads nothing sends no request, and the `Finally` hides the loader.
- `X4.5` behavior: a `Method()` that returns a promise stops its pipeline after the call and names `MethodAsync`.
- `X4.6` behavior: a dose that lacks `refills`, which the app omits at its default, reads 0; one that lacks `unit`,
  omitted when null, reads its initializer's "mg", and so does a find's field; an on-call nurse a component holds that
  lacks `callback` reads "switchboard" (D4.2).
- `X4.7` behavior: a populated member a dose's store lacks reads the store's own start, "fridge", a member the store
  only reads (getter-only) keeps the store's own object, "fridge", and a replaced one reads a new object's,
  "cupboard" (D4.2).
- `X4.8` behavior: a dose that lacks the member named `constructor` reads its start, null, never what every object
  inherits (D4.2).
- `X4.9` behavior: a read through a batch that lacks its required `lot` stops the pipeline, and one through a batch
  that holds it reads on; a member a seal (no constructor the binding can use) or a polymorphic delivery lacks stops
  the pipeline; an on-call nurse whose pager line is a list where the declaration names an object stops a read of
  its number; a data member that holds a promise stops the pipeline (D4.2, D4.3).
- `X4.11` behavior (D4.3, D11.1): the step after a `MethodAsync` runs only once its promise resolves, and never
  when it rejects; a `MethodAsync<T>`'s result is the value its promise resolves to, a member of that data reads
  past it, a resolution to nothing reaches a method as null, and an older pump whose method returns a plain value
  is awaited as that value.
- `X4.12` refusal (D4.3, D5.11): an async method's result read inside a list's lambda: "An async method is called
  inside a list's lambda, which reads every item in one turn and waits for nothing. Call the method in the pipeline
  and read its result in the lambda."
- `X4.10` behavior: in an app whose options set `RespectRequiredConstructorParameters`, a dose that lacks a
  constructor parameter (Heparin's omitted `ward`) stops a read through it, and a dose that holds every parameter
  reads the start of a member it lacks, 0 (D4.2).

## Q5. The object model

**Decision.**
- D5.1 A live object is a `JsObject`: an element's component, a plugin, the object a `LiveObject<T>` member holds
  or a `ReturnedObject<T>` method returns, each item of a `LiveObjects<T>` list, an event's payload declared as one
  (D5.6), and every object reached from them. Its members are `Property`, `ReadOnly`, `WriteOnly`,
  `Method`, `Method<T>`, `MethodAsync`, `MethodAsync<T>`, `LiveObject<T>`, `LiveObjects<T>` and `Event<T>`.
- D5.2 Data is `Data<T>`: event payloads declared as plain data, response bodies, pushed messages, values a
  method returns, and items of lists of data.
- D5.3 A declaration mirrors the typed API of the object it declares, member for member (the owner, 2026-11-06:
  "when we onboard a component, we onboard the typed API exactly what that component has"). The build refuses a
  member the API lacks (D17.5), so the framework never calls one. Two marks come from that API, the same on every
  live object whatever holds it: a component, a plugin, a returned object, an item of a list, an event's payload,
  and every object reached from them.
  - Presence. A member is required unless the API marks it optional (`?` in a TypeScript declaration). Reading,
    writing or calling an absent required member stops its pipeline. An absent optional member reads nothing, a
    call of it runs nothing and its result reads nothing, and a write to it sets it.
  - Value. The member's C# type is the API's value type, nullable where the API's value is: `string | null` is
    `string?`, `number | null` is `decimal?`. Null is a value that data holds and a declaration states, never a
    state of the framework (the owner, 2026-11-06: "That is about the framework, not about the value"). A payload
    whose API is `{ name: string; dateOfBirth: Date; birthCountry: string | null }` declares `ReadOnly<string> Name`,
    `ReadOnly<DateTime> DateOfBirth` and `ReadOnly<string?> BirthCountry`; a null country reads as null, and a text
    set from it shows nothing.
  - The vendor contract analyzer (D17.5) checks both marks against the API, a reference type's nullability by its
    nullable annotation, so neither drifts from the vendor.
  - Data the app types with its own C# type (a response body, a pushed message, the value past a declared member,
    the value a method returns) reads its start where a field is absent and null where the field holds null (D4.2);
    on an event's payload X4.3 states both.

- D5.4 A dotted member at any depth reads the declared steps as required and every step past the declared
  member as optional, as two nested `get`s split at the boundary.
- D5.5 A returned live object (`ReturnedObject<T>`) reads and calls its declared members as required. A returned value
  (`Method<T>`) is data.
- D5.6 An event's payload is declared as what its listener receives, with the whole vocabulary. A live payload is
  a `JsObject` declaration: every door of D5.1 works on it and on every object it holds, at any depth (properties
  read, written or write-only, `LiveObject<T>`, `LiveObjects<T>`, every method kind, returned objects and lists, the events
  of an object it holds by `listen` (D12.8), and callbacks (D17.12)), with presence and value as D5.3 states. Plain
  data is `Data<T>`. A pushed message is `Data<T>` only, since it arrives as JSON (D5.7). What a payload's reads,
  writes and calls do before and after an awaited step is D11.4.
- D5.7 A server-pushed message is `Data<T>`, the same as a custom event's payload and a response body.
- D5.8 The declaration door names the kind of JavaScript member it declares. `LiveObject<T>()` and `LiveObjects<T>()`
  are the object and the list a property holds; `ReturnedObject<T>(..)` and `ReturnedObjects<T>(..)` are what
  a method returns. No door infers the kind from the C# member that declares it.
- D5.9 Every method kind (`Method`, `Method<T>`, `MethodAsync`, `MethodAsync<T>`, `ReturnedObject<T>`,
  `ReturnedObjects<T>`) takes its arguments as one list of typed `Arg`s, written as a collection expression,
  `Method([position, icons])`, so a declaration expresses a vendor method of any arity. The declaration's own
  C# signature keeps each argument typed (`Arg<string> position`).
- D5.10 A read of data past a declared member, and of a member of `Data<T>`, is `Read(x => x.Pager.Number)`. `Get`
  names only the HTTP verb, which a view writes beside it, so one word never carries two concepts.
- D5.11 A method's result (`Method<T>`, `ReturnedObject<T>`, `ReturnedObjects<T>`) is what the call returned when
  the pipeline reached the line that calls it. The plan binds it there with `let`; the call runs once, and every read
  of the result reads that binding, so a later step that changes the object leaves the result as it was. Inside a
  list's lambda the call is the lambda's own and runs once for each item the operation reads, and inside a
  callback's answer it is the answer's own and runs on each call of the answer (D17.12). A method called in a
  condition's term runs when its term is reached (D4.7), and every other result binds at its line: a result called
  before a step and read by a term, and one a term and its branch both read, are what the call returned at its line
  (X8.15). A method declared to return nothing is a `do` at its line;
  written inside a list's lambda, which writes nothing, it is refused where the view renders. Each argument is read
  when its line runs, and one that reads nothing passes null (D7.6). A result is read in the block that called its
  method and in the blocks inside it (D8.8); a C# variable that carries it into another pipeline, or out of its
  block, is refused where the view renders (X5.14). A `MethodAsync<T>` result binds by `let` with `await` (D4.3).

**Schema nodes.** `obj`, `get`, `call` with `members`; `Handler.path`.

**Checker.**
- `Absence.tla:RequiredAbsentStops`
- `Absence.tla:DataAbsentReadsItsStart`
- `probe:ej2-max-public-method-arity`
- `probe:ej2-methods-past-four-arguments`
- `probe:ej2-event-args-presence-marks`

**Doors.**
- kept, public for the component author: `JsObject`'s `Property`, `ReadOnly`, `WriteOnly`, `Method`,
  `Method<T>`, `MethodAsync`, `MethodAsync<T>`, `Event`, `Event<T>`, `Fixed` and `Pass`
  (`Alis.Reactive/Components/JsObject.cs:74-260`).
- `N5.6` new: `Read(..)` on a property's read and on `Data<T>`, replacing `Get(..)`
  (`Alis.Reactive/Components/JsMembers.cs:37`, `Alis.Reactive/PlanAuthoring/Events/Data.cs:40`), since `Get` is
  the HTTP verb in the same views (D5.10).
- `N5.5` new: `LiveObject<T>()` and `LiveObjects<T>()`, the live object and the list a property holds, replacing
  `Object<T>()` and `Objects<T>()` (the owner, 2026-11-06: "Object which collide with the built-in C# object"). A
  member named `Object` on `JsObject` shadows `System.Object` in every declaration, and the build refuses it
  (CA1720, seen in the POC).
- `N5.2` new: `ReturnedObject<T>(..)` and `ReturnedObjects<T>(..)` on `JsObject`, replacing `Object<T>(Arg..)`
  and the method form of `Object<T>()` and `Objects<T>()`.
- `N5.3` new: one door per method kind taking `IReadOnlyList<Arg>`, beside the door with no arguments,
  replacing the overloads of one to four arguments (`Alis.Reactive/Components/JsObject.cs:116-269`).
- refused: the `GetMembers` scan in `JsObject.DeclaresMethod` (`Alis.Reactive/Components/JsObject.cs:366-368`),
  which reads non-public members to tell a property from a method; N5.2 carries the capability.
- `N5.1` new: `ServerPushStream` and `SignalRHub` message triggers typed `Data<TMessage>`, replacing the
  `JsObject` with `ReadOnly<T>` members.
- `N5.4` new: a member's presence mark on every declaration door, optional where the typed API marks `?` (D5.3);
  its form goes to gate 2.
- kept on a new holder: the whole declaration vocabulary on an event's payload, at any depth (D5.6), where the
  redesign's payload is already a `JsObject` (`Alis.Reactive/Components/Event.cs:14`).

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
- `X5.7` behavior: a payload field its API marks optional, which the vendor leaves out, reads nothing and the text
  it sets shows nothing; a required field the vendor leaves out stops its pipeline, and the step after it does not
  run.
- `X5.8` behavior: a payload's `string?` field holding null reads as null: the text it sets shows nothing, and the
  pipeline goes on.
- `X5.9` analyzer: a declaration whose presence or nullability differs from its vendor's typed API: new:N17.2.
- `X5.10` behavior: a property of an object a live payload holds, written by the handler, changes that object as
  the page shows, and its method, called, runs.
- `X5.11` behavior (D5.11): a pump's summary read twice calls `summary()` once, two calls of it run it twice, and a
  summary read before a step that changes the rate still holds the rate it returned; the running channels counted in
  a list's lambda call `isRunning()` on each channel as it is when the step runs.
- `X5.12` behavior (D4.1, D4.3, D4.4, D7.6): a method the older pump lacks, a member the pump holds as text, a
  method declared to return nothing and one declared to return a value, each of which returns a promise, stop the
  pipeline at the call, and the step after it does not run; a null literal, a null the returned data holds and a
  result that reads nothing each reach the method as null.
- `X5.13` refusal (D5.11): a method declared to return nothing, called inside a list's lambda: "A step is written
  inside a list's lambda, which reads each item and writes nothing. Write the step in the pipeline."
- `X5.14` refusal (D5.11): a result, a returned object or a returned list that a C# variable carries into another
  pipeline: "A method's result is read outside the block that called the method. Call the method in the block that
  reads its result, or in a block around it."

## Q6. Arrays

**Decision.**
- D6.1 One plan form per operation. A projection is `call(list, ["map"], [fn])`; `each` in a read leaves (D1.5).
- D6.2 One name per operation, the names `ReactiveArray` has on both lines: `Where`, `Select`, `OrderBy`,
  `OrderByDescending`, `Count()`, `Count(predicate)`, `Any()`, `Any(predicate)`, `All(predicate)`, `Sum` over
  `int`, `decimal` and `double`, `Find(predicate)` and `Find(predicate, field)`. The property `Count` and the
  method `Each` on a declared list (`LiveObjects<T>`) leave, renamed to `Count()` and `Select`.
- D6.3 An operation over a list that reads nothing reads nothing; `Any()` and `All()` over nothing read nothing,
  so a condition on them does not hold. An operation over a value that lacks the operation's JavaScript member
  stops the pipeline: a paged result (`{ result, count }`) where the declaration says a list has no `length` and
  no `filter`. A value that has the member is read as JavaScript reads it, as every value is: a text where a list
  is declared counts its characters, since the plan checks presence, never a value's type (D5.3). The plan writes
  the operation's JavaScript member with `members` `requiredIfHeld` (schema.md), which reads nothing on a holder
  that reads nothing and stops on a holder that lacks the member. A `set` never carries `requiredIfHeld`: its C# type is
  `Presence`, which has no such value, and only `get` and `call` take `ReadPresence`.
- D6.4 In an array lambda, a member .NET defines on the value's type (`Trim`, `Length`, `Year`, a `DayOfWeek`
  compared) reads nothing on a missing value and stops the pipeline on a value that lacks it, written `requiredIfHeld`
  as D6.3 writes a list's operation.
- D6.5 The .NET members an array lambda translates are the closed table `spec/Core/translated-members.md`,
  keyed by the definition of the member the expression tree carries, its module and metadata token, which every
  instantiation of a generic member shares, and built from typed lambdas; no attribute is scanned.
- D6.7 A value the view holds inside an array lambda (`r => r.Ward == Model.Ward`, a local the lambda captures,
  a `const`, an enum member) is read when the view renders and written as a literal. A constant is read from the
  tree; a captured local or a member of the view is read by walking its chain of fields and properties with
  `FieldInfo.GetValue` and `PropertyInfo.GetValue`, the owner's exception of 2026-11-06 ("capturing the model value
  is important"). No expression is compiled at render and no accessor is cached. A view value that calls a method,
  indexes or computes (`Model.Ward.ToUpper()`, `ward + "x"`) is refused, naming its door, a local computed before
  the lambda (the owner, 2026-11-06: "if var works i do not see any downside"). The redesign compiles such a value
  today (`Alis.Reactive/Members/Expressions.cs:28`); the local keeps the capability.
- D6.8 A list's lambda takes one of two forms, by what an item is. An item of `LiveObjects<T>` is a live object:
  its lambda is a delegate over the item's declaration, `bed => bed.Resident`, run once when the view renders with
  the item bound to the `fn`'s parameter, and its members are required (D4.1). An item of a list of data is a C#
  value: its lambda is an expression tree, `round => round.Carer.Name`, its members named as Q2 names them and
  read as D4.2 reads data. A lambda reads; a step written inside one is refused where the view renders (X6.8). Each `fn`
  parameter has a name of its own in the plan, so an inner lambda never hides the item of the lambda around it.
- D6.11 `Sum` agrees with .NET for each number type. Over `double` it is JavaScript's addition, the same IEEE 754
  addition .NET does, left to right. Over `decimal` it is decimal arithmetic, the owner's finance standard
  (2026-11-06): each term's places are read from its shortest text, the terms are added as `BigInt` whole numbers
  at the largest scale among them, and the point is placed once, so 0.1 + 0.2 is 0.3 as in .NET. An exact sum
  rounds nothing. The owner set two app-wide knobs, places (2) and rounding (`MidpointRounding.ToEven` unless the
  app chooses `AwayFromZero`). They apply only where an operation must round: a division, an average, a value
  fitted to its places. Sums and comparisons stay exact, so a dose of 0.125 mg stays 0.125 (spec/CLAUDE.md). A decimal the app sends as a JSON number keeps
  about 15 significant digits when the browser parses it; an app whose amounts need more sends decimals as text,
  and the same addition holds. Over `int` the running total, added left to right, stops the pipeline once it passes
  the range of `int`, as .NET's checked `Sum` throws `OverflowException` (the owner, 2026-11-06): .NET throws on
  2,147,483,000, 1,000 and -1,000, whose total fits, and on a list of 64 (`spec/NOTES.md`, the probe of 2026-11-06).
  A `decimal` total past `decimal.MaxValue` stops the pipeline, as .NET throws `OverflowException`. The plan writes
  `Sum` as the operator `sumInt`, `sumDouble` or `sumDecimal` over the `map` of its term, one operator over the whole
  list, so a decimal's point is placed once and an int's range is checked at each term. A list that reads nothing
  sums to nothing (D6.3); a list with no item sums to 0, as .NET's does. A `decimal` term is a JSON number or the
  decimal text the app writes for it (`JsonNumberHandling.WriteAsString` writes 0.10m as "0.10"), added the same way.
  Any other term, null among them, stops the pipeline, as these three types have no null to add; a term that reads
  nothing, past a holder that holds null, is null in the `map`, so it stops too. A term member the item lacks reads
  its start first (D4.2), so `Sum(d => d.Refills)` adds 0 for a dose the app omitted at 0.
- D6.6 The JavaScript array members the plan reads are `map`, `filter`, `some`, `every`, `find`, `toSorted` and
  `length`, named at the vendor boundary of the built-in array and read `requiredIfHeld` (D6.3). The browser adds no
  array function, and no operation changes the vendor's own array: a sort is `toSorted`, never `sort`. The redesign
  calls `toSorted` and `reduce` (`Alis.Reactive/PlanAuthoring/Arrays/ArrayOperations.cs`); `reduce` leaves, because a
  sum is an operator over the `map` of its terms (D6.11), and a list the view supplies is looked in by the
  `includes` operator (schema.md).
  `Any()` is `some` over a `fn` whose body is the literal `true`, so it needs no operator; `Count(predicate)` is the
  `length` of `filter`.
- D6.9 `Find(predicate)` over a list of live objects is the first item the predicate keeps, a live object.
  Reading, writing or calling its member when no item matched stops the pipeline, as a member of a null object
  fails in .NET. `Find(predicate, field)` is the door that reads nothing when no item matched: `find` stops at the
  first item kept, as .NET's `FirstOrDefault` does, and the field is read once, from that item, with
  `requiredIfHeld`, so no match reads nothing and a match that lacks a declared member still stops (D4.1). Over a
  list of data, `Find(predicate)` is the first item kept, or nothing, and `Find(predicate, field)` reads the field's
  path from it, optional as data is.
- D6.10 The operations over a list of data take every list a model declares: `List<T>`, `T[]`, `IList<T>`,
  `IReadOnlyList<T>`, `IEnumerable<T>` and every other `IEnumerable<T>`. They are extension methods on
  `ITypedSource<IEnumerable<TItem>>`, the covariant view `TypedSource<T>` implements, and each returns a
  `TypedSource<T>`, so the sources stay the closed class D7.2 names. A text is no list in the browser: each
  operation over a `string` is refused at compile time by an `[Obsolete(error: true)]` overload (CS0619).
- D6.12 A comparison in a list's lambda over data compares the item's members with each other or with the values the
  view holds (D6.7), by closed rules C# checks where the view renders:
  - A value the view holds is written through the app's JSON options as the type of the member it is compared with,
    taken under the conversion the compiler adds to compare an enum or a `char` as its number: `DoseStatus.Due` is
    "due" when the app writes enums by name, and `'I'` is "I".
  - Equality (`==`, `!=`) compares a member with a value written as a JSON scalar or null, or two members of one type
    written as a scalar: a number, text, `bool`, `char`, `Guid` or an enum. A value written as an object or a list is
    refused, and so are two members of a type that is not a scalar: the browser compares objects by reference.
  - Ordering (`<`, `<=`, `>`, `>=`) compares a member with a value written as a JSON number or null, or a `char` with a
    `char`. A value written as text is refused, which covers an enum the app writes by name and a number the app writes
    as text. Between two members it compares number types and `char`, and refuses every other type.
  - A date or a time (`DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly`, `TimeSpan`) is refused in a comparison:
    its text neither orders nor equals as the instant does. `as date` and `as instant` (D3.2) carry it, built with
    Q3's conversions.
  - A cast that narrows (`(int)d.Milligrams`) is refused: it changes the value as C# computes it, and the browser's
    number does not.
  - A member with a JSON converter or number handling of its own is the contract analyzer's (N18.5, POC 13), which
    refuses it in a comparison; until that analyzer is built, such a member compares as the app's options write its
    type.
- D6.13 `OrderBy(key)` and `OrderByDescending(key)` write `call(list, ["toSorted"], [fn(a, b)])` whose body compares
  the key read from each item, the two items swapped for descending, so items with equal keys keep their list order
  as .NET's stable sort keeps it. C# picks the comparison from the key's type and how the app's JSON writes it, where
  the view renders: a key written as a JSON number compares as numbers (`compareNumbers`), a `bool` puts false first
  (`compareBooleans`), a `char` compares by its code unit (`compareOrdinal`), and null comes first, as
  `Comparer<T>.Default` puts it. A nullable value key binds the overload whose sample is the underlying type's value.
  Text with no comparer, or with the comparer of the view's culture (`StringComparer.CurrentCulture`), orders by
  the view's culture (Decision 3, a, the owner, 2026-11-06): `compareCulture`, the browser's `Intl.Collator` under
  the plan's collation (D3.11). It matches .NET 10 on ICU on every word and every BMP character probed outside the
  Han ideographs, whose order differs between ICU builds, and differs from net48's NLS word sort on hyphens,
  apostrophes and ligatures (`co-op` beside `coop`) (`spec/NOTES.md`, 2026-11-06). C# reads the view's culture once,
  where the view renders, and asks .NET how it compares: where .NET orders `"a"` after `"B"` (invariant
  globalization mode) the text compares by code unit (`compareOrdinal`); a culture with an alternate sort
  (`de-DE_phoneb`), whose tag the browser does not take on every host, is refused (X6.36). A culture the browser
  has no collation for (`haw-US`) stops the pipeline at its first comparison, since the browser would otherwise
  order by its own locale. `OrderBy(key, StringComparer.Ordinal)` orders by UTF-16 code unit, as .NET's ordinal
  comparer does. A key member the item lacks reads its start first (D4.2), so `OrderBy(d => d.Refills)` sorts a dose the app
  omitted at 0 as 0. A key that reads nothing, past a holder that holds null, or holds a value its comparison does not
  compare, stops the pipeline.
- D6.14 A list's lambda chooses between values as C# does, each term read only when it is reached (D4.7).
  `a ?? b` writes `op(coalesce, a, b)`: it reads `a` when `a` holds a value, and `b` when `a` holds null or reads
  nothing, so `d.Carer.Name ?? "unassigned"` is "unassigned" for a dose whose carer is null, where .NET throws.
  `test ? a : b` writes `op(cond, test, a, b)`: it reads `a` when the test reads true, and `b` when it reads false or
  nothing, as a condition holds only on true (D8.9). A nullable's `HasValue` is `neq` of it and null, so it reads
  nothing past a holder that holds null (D8.9); its `Value` is `op(nullableValue, x)`, the value, or nothing where the
  nullable holds null and .NET throws, as a text operator reads nothing on null (`spec/Core/translated-members.md`).
  A `??` whose left side the compiler converts by a user-defined conversion is refused (X6.33): the browser has no
  conversion the app wrote. Outside a lambda, at any value position, `source.IfNull(fallback)` writes the same
  `coalesce` and `test.Choose(whenTrue, whenFalse)` the same `cond` (N7.4), each term read only when it is reached.
- D6.15 `Regex.IsMatch(text, pattern)` and `Regex.IsMatch(text, pattern, options)` in a list's lambda write
  `op(matches, text, lit)`, the pattern's browser copy, which D16.11's one reader writes where the view renders from a
  pattern the view holds (Decision 2, the owner, 2026-11-06). A pattern read from the item, or null when the view
  renders, is refused (X6.38). A text that reads nothing or holds null reads nothing, as every text operator does
  (`spec/Core/translated-members.md`), where .NET throws `ArgumentNullException`.

**Schema nodes.** `call` with `fn`; `get` of `length`; `op` (`coalesce`, `cond`, `nullableValue`).

**Checker.**
- `Absence.tla:MissingListReadsNothing`
- `check:fluent`

**Doors.**
- existing: `ReactiveArray` (`Alis.Reactive/PlanAuthoring/Arrays/ReactiveArray.cs:44`, `Alis.Reactive/PlanAuthoring/Arrays/ReactiveArray.cs:93`).
- `N6.1` new: `LiveObjects<TItem>.Count()` and `LiveObjects<TItem>.Select(..)`, replacing `Count` and `Each`, beside
  `At(n)` and `Where(..)`; the same operations over a list of data are extension methods on
  `ITypedSource<IEnumerable<TItem>>` (`DataLists`, POC 4, D6.10).
- `N6.4` new: `public interface ITypedSource<out T>`, which `TypedSource<T>` implements and nothing else in the
  framework does; the list operations over data extend it (D6.10). A source the framework did not make is refused
  where the view renders, and the analyzers of POC 13 refuse a class that implements it.
- `N6.3` new: a `DiagnosticSuppressor` that suppresses CS8602 inside the lambda of `Read`, `Where` and `Select`,
  where a member read past a holder that holds null reads nothing (D4.2). An expression tree refuses `?.` (CS8072), so without it a
  view writes `round.Carer!.Name`, a `!` that claims what the plan does not need. Built with the analyzers in POC 13.
- `N6.2` new: `internal static class TranslatedMembers`, the closed table of D6.5.

**Evidence.**
- B8: `Each` and the property `Count` on a declared list (`Alis.Reactive/Components/JsMembers.cs:135`,
  `Alis.Reactive/Components/JsMembers.cs:140`) beside `Select` and `Count()` on an array
  (`Alis.Reactive/PlanAuthoring/Arrays/ReactiveArray.cs:44`, `Alis.Reactive/PlanAuthoring/Arrays/ReactiveArray.cs:93`).
- B11: the method table is scanned for `PureMethodAttribute` (`Alis.Reactive/PlanAuthoring/Arrays/ElementExpressionCompiler.cs:51-55`).

**Defects.**
- `X6.1` type: a list cannot be counted two ways: new:N6.1 leaves one `Count()`.
- `X6.2` refusal: a list's lambda that calls a member outside the table: "'{term}' has no browser form in a
  list's lambda; use a member the framework translates, or move the work to the server."
- `X6.3` behavior: `Where(x => x.Name.Trim() == "")` over a list whose item lacks `Name`, whose start is null,
  reads that item's test as nothing, so the item is not kept (D4.2, D6.4).
- `X6.4` behavior: `Any()` over a list that reads nothing does not satisfy `When(list.Any())`.
- `X6.9` behavior: `Sum` over `decimal` terms 0.1 and 0.2 reads 0.3, equal in value to .NET's 0.30 (how a decimal
  shows as text is Q3's); over `double` it reads 0.30000000000000004, as .NET's `double` does; over `int` a running
  total past 2,147,483,647 stops the pipeline and the step after it does not run, though the terms after it bring the
  total back into range; over `decimal` a total past `decimal.MaxValue` stops the same way; over a list with no item
  it reads 0.
- `X6.8` refusal: a step written inside a list's lambda: "A step is written inside a list's lambda, which reads
  each item and writes nothing. Write the step in the pipeline."
- `X6.5` behavior: `Where(r => r.Ward == Model.Ward)` keeps the items of the ward the view's model names, and a
  local the lambda captures is read the same way.
- `X6.6` refusal (D6.7): a view value that calls a method or computes inside an
  array lambda: "'{value}' computes a value
  inside a list's lambda; compute it into a local before the lambda, such as var ward = Model.Ward.ToUpper();."
- `X6.10` refusal (D6.12): a comparison of objects: "'{term}' compares objects, and the browser compares objects by
  reference; compare a member of each, such as d => d.Carer.Name == lead."
- `X6.11` refusal (D6.12): a date or a time in a comparison: "'{term}' compares dates or times, whose text neither
  orders nor equals as the instant does; compare them on the server."
- `X6.12` refusal (D6.12): an ordering against a value the app's JSON writes as text: "'{term}' orders a value the
  app's JSON writes as text, which does not order as the value does in .NET; compare it with == or !=, or order on
  the server."
- `X6.13` refusal (D6.12): an ordering of two members outside numbers and `char`: "'{term}' orders values the browser
  cannot order as .NET does; order numbers or a char, or order on the server."
- `X6.14` refusal (D6.12): a cast that narrows: "'{term}' casts a number to a narrower one, which changes the value as
  C# computes it and not as the browser does; compare the member itself."
- `X6.15` refusal (D6.7): a view value read through a null: "'{value}' reads a member of a value that is null when the
  view renders."
- `X6.16` behavior (D6.12): `d.Status == DoseStatus.Due` keeps the due doses when the app writes enums by name, and
  `d.Route == 'I'` keeps the injected ones. `d.Status == d.Planned` keeps the doses whose two members hold one value,
  `d.Milligrams > d.Priority` orders two numbers and leaves out a dose whose priority is null, and `d.Route < 'L'`
  orders a `char` as .NET does.
- `X6.17` refusal (D6.5): `StartsWith(text)` or `EndsWith(text)`, which compare by the culture in .NET: "'{term}'
  compares by the culture in .NET, which the browser cannot match; pass StringComparison.Ordinal, such as
  d.Ward.StartsWith(\"N\", StringComparison.Ordinal)."
- `X6.18` refusal (D6.5): a `StringComparison` other than `Ordinal`: "'{term}' compares text by {comparison}; a
  list's lambda compares text by StringComparison.Ordinal."
- `X6.19` behavior (D6.5): the text members agree with .NET where JavaScript's own methods do not: `Trim()` removes
  U+0085 and keeps U+FEFF, `ToUpperInvariant()` keeps `ß` and `ı` and maps U+1FB3 to U+1FBC, `ToLowerInvariant()`
  lowers a final `Σ` to `σ` and keeps `İ`, `string.IsNullOrEmpty` holds on a text that reads nothing, and
  `string.IsNullOrWhiteSpace` holds on U+0085. `TrimEnd(new char[] { })` removes white space as .NET does, and
  `Contains` given null to look for stops the pipeline, as .NET throws `ArgumentNullException`.
- `X6.20` behavior (D6.5, D6.8): `d => d.Carer != null && d.Checks.Any(c => c.Name == d.Carer.Name)` keeps the doses
  their carer also checked, the inner lambda reading its own item and the outer one; `Any()`, `All`, `Count()` and
  `Count(predicate)` over a list an item holds count as .NET counts them, `All` holding on an empty list.
- `X6.21` refusal (D6.5): `Enumerable.Contains` over a collection that is not an array, and a `HashSet<T>`'s own
  `Contains`: "'{term}' looks in a
  collection that can compare by its own rules, as a HashSet's comparer does; copy it into an array before the
  lambda, such as var wards = Model.Wards.ToArray();."
- `X6.22` refusal (D6.5): `Contains` over a list the item holds: "'{term}' looks for a value in a list the item
  holds; use Any, such as d.Hours.Any(hour => hour == 8)."
- `X6.23` refusal (D6.5): `Contains` of an object: "'{term}' looks for an object, and the browser compares objects
  by reference; look for a member, such as names.Contains(d.Carer.Name)."
- `X6.24` refusal (D6.5): `Contains` over a list that is null when the view renders: "'{term}' looks in a list that
  is null when the view renders."
- `X6.25` refusal (D6.5): `Contains` with an equality comparer: "'{term}' passes an equality comparer, which the
  browser does not have; look up without it, or move the work to the server."
- `X6.26` behavior (D6.5): `wards.Contains(d.Ward)` over an array, an enum array written by name, a `List<T>` and
  an array typed as a sequence keeps the items the view lists; `d.Medicine.Contains('o')` finds a letter; a list's
  `Count` and an array's `Length` count their items.
- `X6.27` behavior (D6.13): a text key with no comparer orders as .NET orders it under the view's culture: en-US and
  the invariant culture list "DR İNCE" before "Dr Işık", tr-TR lists "Dr Işık" first; `OrderByDescending` reverses the
  order; `StringComparer.CurrentCulture` orders as no comparer does; in invariant globalization mode the text orders
  by code unit, as .NET compares it there; under `haw-US`, which the browser has no collation for, the sort and the
  steps after it do not run.
- `X6.28` refusal (D6.13): a key the app's JSON writes as text (an enum by name, a number written as text, a `Guid`,
  a `TimeSpan`): "'{key}' orders a value the app's JSON writes as text, which does not order as the value does in
  .NET; order by a member the JSON writes as a number, or order on the server."
- `X6.29` refusal (D6.13): a date or a time key: "'{key}' orders dates or times, whose text does not order as the
  instant does; order them on the server."
- `X6.30` refusal (D6.13): an object key: "'{key}' orders objects, which the browser does not order; order by a
  member, such as OrderBy(d => d.Carer.Name, StringComparer.Ordinal)."
- `X6.31` refusal (D6.13): a comparer other than `StringComparer.Ordinal` and the view's culture's: "'{key}' orders
  text by a comparer the browser does not have; pass StringComparer.Ordinal, or no comparer to order by the view's
  culture, or order on the server."
- `X6.32` behavior (D6.13): `OrderBy` lists a number key smallest first, a nullable number with null first, a `bool`
  false first and text by code unit with `StringComparer.Ordinal`; `OrderByDescending` lists null last and keeps the
  list order of equal keys.
- `X6.33` refusal (D6.14): a `??` whose left side the compiler converts by a user-defined conversion: "'{term}'
  converts its left side by a user-defined conversion, which the browser does not have; coalesce values of one
  type, or move the work to the server."
- `X6.34` behavior (D6.14): `HasValue` holds for a number and fails for null, and reads nothing past a carer that is
  null; `Value` of a priority that holds null reads nothing, so `d.Priority.Value != 2` drops it where
  `d.Priority != 2` keeps it; `??` reads its right side for a ward the dose omits at null, a priority that holds null
  and a carer's name past a carer that is null, and never reads it when its left side holds a value; `?:` reads its first branch when its
  test holds, its second when the test fails or reads nothing, and never reads the branch it does not take.
- `X6.35` behavior (D6.5): `Contains` over a list the view holds finds null in it, and an item that reads nothing
  reads nothing, so it is not found even in a list that holds null.
- `X6.37` behavior (D6.15, D16.11): `Regex.IsMatch` keeps the doses whose text matches; `food$` keeps a text that
  ends in a line break as .NET's `$` does, and not one that ends in two; `\$` matches the character; a pattern the
  view holds in a local matches as one written in place; a lookbehind matches as .NET reads it; `CultureInvariant`
  matches as no option does; a note that holds null reads nothing.
- `X6.38` refusal (D6.15, D16.11): each construct outside D16.11's closed set, with its message: "'{term}' holds
  '{construct}', which .NET and the browser read differently; write {spelling}." for `\d`, `\D`, `\w`, `\W`, `\s`,
  `\S`, `\A`, `\Z` and `.`, and for `\d`, `\w` and `\s` inside a class; "'{term}' holds '{construct}', which the
  browser does not read as .NET does; move the work to the server." for every other letter escape and every group
  opener but `(?:`, `(?=`, `(?!`, `(?<=` and `(?<!`; an escape that needs none, a `\u` or `\x` short of its hex
  digits, a trailing `\`, a `]` first in a class, a `[` inside one, a range that runs backwards, a `-` right after a
  range, a class or a group left open, a `)` or a `]` that closes nothing, a quantifier or a count with nothing to
  repeat, a `{` that is no count and a count whose least is above its most, each naming the escape that writes it;
  `IgnoreCase`, naming the spelled cases; any other option but `CultureInvariant`; a pattern read from the item; a
  pattern that is null when the view renders.
- `X6.36` refusal (D6.13): text ordered by the culture under a culture with an alternate sort: "'{key}' orders text by
  the alternate sort '{sort}', which the browser has no tag for; pass StringComparer.Ordinal, or order on the
  server."
- `X6.7` behavior: a list that holds a paged result (`{ result, count }`) where the declaration says a list stops
  the pipeline at its operation; a list that reads nothing reads nothing and the pipeline goes on (D6.3).

## Q7. Every value position takes every source

**Decision.**
- D7.1 The positions are closed: a property or write-only property set, an element's text, HTML and class name,
  both operands of every condition operator, a request member (which Q9 places in the route, the
  query, the body or a header), a header, a dispatch payload member, and a method's, plugin's or hub's argument.
- D7.2 The sources are closed, each a `TypedSource<T>`: a component's or plugin's member, a method's result
  (`Method<T>`, `MethodAsync<T>`, a hub's `Invoke<T>`), a page URL member (`p.Url<TQuery>()`), a field of an event
  payload or a pushed message, a field of a response body, an input's bound value (`p.Bound`), the result of an
  array operation, a literal, an object built from sources, a list built from sources, and a value chosen from
  sources by `IfNull` or `Choose` (D6.14).
- D7.3 Every position takes a literal of its type and every source whose type is assignable to it:
  `TypedSource<TSource>` with `TSource : TValue`, and a `TValue` source at a `TValue?` position. A set takes a source
  as `ITypedSource<out T>`, so a narrower reference type and a text without null reach it as they are. C# converts a
  method's `Arg<T>` only from a `TypedSource<T>`: it refuses a conversion from the covariant interface (CS0552), warns
  where only nullability differs (CS8620), has no conversion from a struct's source to its nullable's (CS1503), takes no
  conversion operator in an extension block (CS9282), and a second operator on `TypedSource<T>` makes the plain case
  ambiguous (CS0457). So a source reaches its nullable, at an argument and at a struct's set or comparison, through
  `OrNull()` (N7.5), which writes nothing of its own, and a narrower reference type reaches an argument through the
  declaration's `Pass`, a door Q5 keeps for the component author.
- D7.4 One builder makes an object from sources, member by typed member, and the dispatch payload, a method
  or plugin argument and a property write all use it. One door makes a list from sources.
- D7.5 Every condition operator takes a source for each operand: the text tests, `MinLength`, `Between`,
  membership and the comparisons. `Matches` takes a literal pattern only, because the pattern is checked
  where the view renders for the constructs JavaScript reads differently from .NET; a pattern read in the
  browser cannot be checked.
- D7.6 A source that reads nothing writes `null` into a property, an argument and a payload member. At an
  element's text and HTML it shows nothing, and at a class it changes nothing (D4.5): a class name is bound by a
  `let`, and the element's `classList` method runs only where it holds a value, as the redesign's class change does
  (`Alis.Reactive/PlanModel/Reactions/Steps.cs:133-140`). An element's attribute is no value position: neither line
  writes one from a source; the redesign reads a child's attribute inside a lambda over the element's children. In a request it is
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
- `N7.5` new, its form gate 2's: `OrNull()` on a source of a struct or a reference type, returning the same read as
  a source of its nullable type (D7.3).
- `N7.4` new, its form gate 2's: `IfNull` on a source of a nullable type, taking a literal or a source of the type
  without null and returning a source of it, and `Choose` on a `TypedSource<bool>`, taking a literal or a source for
  each branch; extension methods beside the comparisons of N8.2.

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
- `X7.8` behavior (D7.6): a pump card's class is added, removed, toggled on and toggled off, and a class name that
  reads nothing leaves the card's class as it was, where `classList.add(null)` adds "null"; HTML set on an element
  shows its markup, and HTML from a value that reads nothing shows nothing, where `innerHTML = undefined` shows
  "undefined".
- `X7.7` behavior (D7.3, N7.5): a fluid balance's intake widened to `int?` is written to its nullable target and
  ordered against it, over a 1000 ml target and within it, and an ordering with a null target does not hold, where
  JavaScript's `500 > null` does; a pump's summary widened to `string?` is passed as the caption's argument.
- `X7.6` behavior (N7.4): an alarm's note that holds null shows its fallback, a pump's summary, and one that holds
  a value shows it with no summary taken; a pump's last alarm that reads nothing passes its fallback as a method's
  argument; a condition compares a null weight's fallback and a held weight itself; a fluid target chooses its
  first value when the test holds and its second, a measured intake, when the test reads nothing, and measures
  nothing when the second is not chosen.

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
  or as statements in a `{}` body, and runs them in authored order. An `if` holds one condition, its `then` block
  and its `else` block; an `ElseIf` is the `else` block of the `if` it continues, holding the results its condition
  called, at their lines, and an `if` of its own, as C# and JavaScript nest an `else if`. An `ElseIf` or an `Else`
  continues its `When` before any other step of the block, a method its condition does not read included (X8.9),
  and an `Else` ends its `When` (X8.18).
- D8.8 A block is a lexical scope, as a C# `{}` block is. A value a block binds is readable in that block and
  in the blocks inside it, and nowhere else. The values a block binds: its handler's event payload, a
  route's response body, an array operation's item, a method's result read in it, and a `Confirm`'s answer.
  A read of a bound value outside its block is refused where the view renders, naming the value and the
  block that binds it. That one rule covers today's two refusals of this kind, an array item and a response
  read outside their blocks. The browser runs each block in a child scope, so a value bound inside a branch
  never reaches the steps after it.
- D8.9 One meaning of comparison and logic, which every condition reads (D8.1). A comparison (`eq`, `neq`, `lt`,
  `lte`, `gt`, `gte`) whose operand reads nothing reads nothing, `neq` included: a value past a holder that holds
  null holds none, and no comparison with it holds (D8.5). Null is a value the data holds, compared as .NET's lifted operators
  compare it: `eq` of null and null holds, `neq` of null and a value holds, and an ordering with null does not hold,
  though JavaScript's `null <= 2` is true. Equality is JavaScript's `===`; an ordering is JavaScript's `<` over the
  numbers and one-character texts D6.12 admits. `not`, `and` and `or` are SQL's three-valued logic with nothing as its
  unknown: `not` of nothing is nothing; `and` is false when a term is false, true when both are true, and nothing
  otherwise; `or` is true when a term is true, false when both are false, and nothing otherwise. A term runs only when
  it is reached (D4.7): `and` reads its right term unless its left is false, `or` unless its left is true. A condition
  holds only when it reads true: a branch, an array predicate (`filter`, `some`, `every`, `find`) and a validator
  condition. So `d.Carer.Name != "Ruth"` and `!(d.Carer.Name == "Ruth")` agree over a dose whose carer is null:
  neither holds, where .NET throws. A member the item lacks reads its start first (D4.2): `d.Ward != "North"` holds
  over a dose the app wrote with no `ward`, as it holds over the null the endpoint binds for it.

**Schema nodes.** `op` (the comparison, text, membership and value tests, `and`, `or`, `not`); `if`.

**Checker.**
- `Absence.tla:ConditionTermsLazy`
- `check:fluent`

**Doors.**
- kept: the pipeline door, `ConditionSourceBuilder` and `ConditionOperators`
  (`Alis.Reactive/PlanAuthoring/Conditions/ConditionOperators.cs:16`).
- `N8.2` new, its form gate 2's: the comparisons (`Eq`, `NotEq`, `Gt`, `Gte`, `Lt`, `Lte`) and `And`, `Or` and `Not`
  are extension methods on a typed source that return a `TypedSource<bool>`, which `When` and `ElseIf` take and a live
  list's predicate returns, so one comparison API serves both. Each refuses where the view renders what the lambda
  door refuses for its type: dates and times, objects compared by reference, and an ordering of what the browser
  cannot order as .NET does. A branch writes an `if` (POC 9a).
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
- `X8.7` behavior: over a dose that lacks `ward`, whose start is null, `d.Ward != "North"` and `!(d.Ward == "North")`
  both keep it (D4.2);
  a dose whose `priority` holds null is kept by `d.Priority != 2` and left out by `d.Priority <= 2` and
  `d.Priority < 2`, as .NET's lifted operators decide (D8.9).
- `X8.8` behavior: `or` keeps an item whose left term reads nothing when its right term holds, and `and` whose left
  term reads nothing and right term is false reads false, so `not` of it holds (D8.9).
- `X8.9` refusal (D8.7): an `ElseIf` or `Else` written after another step of its block, or after a method its
  condition does not read: "An ElseIf or an Else continues the When it follows, before any other step of its block:
  write it in the same sentence."
- `X8.10` refusal (N8.2): a pipeline condition that orders dates, orders flags or compares objects, each with the
  reason the lambda door gives for it.
- `X8.11` behavior (D11.5): the steps after a branch that awaits run once its method settles, and at once when the
  branch is not taken.
- `X8.12` behavior (D8.9): a branch whose condition orders a null runs none of its steps, where JavaScript's
  `null < 80` holds.
- `X8.13` behavior (D4.7): a method called in the right term of `Or` runs only when the left term does not hold, and
  in the right term of `And` only when the left term holds.
- `X8.14` behavior (D4.7, D8.7): a method called in an `ElseIf`'s condition runs only when no branch before it ran,
  so a second `ElseIf`'s does not run when the first holds; an async one is awaited there, and its branch runs once it
  settles.
- `X8.15` behavior (D5.11): a fluid balance measured, then set to 999 ml, and read in a term reached after the set,
  is the 100 ml measured at its line; a result a term and its branch both read is the call at its line, so a branch
  that sets 999 ml and then the result restores the 600 ml measured.
- `X8.16` refusal (D4.7): an async method called under a term reached later, the right term of `Or` or the fallback
  of `IfNull`: "An async method is called in a term that runs only when it is reached (the right term of And, Or or
  IfNull, or a branch of Choose), and the pipeline waits for an async method at its line: call it in a When of its
  own, in the branch where its term is reached."
- `X8.17` refusal (D8.8): a result called in an `ElseIf`'s condition, read after its `When`: "A method's result
  called in an ElseIf's condition is read after its When, and the call runs only when the ElseIf is reached: call
  the method before the When to read its result after it."
- `X8.18` refusal (D8.7): a second `Else`, or an `ElseIf` after the `Else`, of one `When`: "An Else ends its When:
  write each ElseIf before it, and every step of the remaining case in that one Else."

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
  The browser adds no header but W3C Trace Context's `traceparent` and `tracestate` while the plan's trace is on
  (D20.3), and `traceparent` and `baggage` on a retry (D20.11), each to the page's own origin only.
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

- D9.13 A request's URL, query and headers follow their standards, and C# writes each part where the view renders.
  - The URL is an RFC 3986 URI reference: a path on the page's origin, or an absolute `http` or `https` URI, split
    into the parts of RFC 3986, section 3. Its path is segments, its query is the query, and a fragment is refused
    (D9.4).
  - A route value fills one path segment, percent-encoded as UTF-8 outside RFC 3986's unreserved characters
    (`probe:js-route-segment-encoding`). A route value whose text holds `/` stops its request, as one that reads
    nothing does (D9.5): net10 hands the action `%2F` still encoded (`probe:core-route-segment-decoding`), so the
    value cannot reach the endpoint as it was.
  - The query is `application/x-www-form-urlencoded` by the URL Standard, written by the browser's
    `URLSearchParams`, so `+` is a space and a `+` in a value is `%2B` (`probe:js-query-encoding`). Both hosts read
    it back as written (`probe:core-query-decoding`, `probe:net48-query-plus-space`).
  - A header field value is RFC 9110's (section 5.5): visible US-ASCII, spaces and tabs, and the white space
    around it is not part of it, as fetch sends it (`probe:js-header-trim`). A fixed value outside that set is
    refused where the view renders, and a value read outside it, a line break or a character past US-ASCII, stops
    its request before it is sent; fetch itself refuses a line break and a character past Latin-1
    (`probe:js-header-values`).
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

- `probe:js-route-segment-encoding`
- `probe:core-route-segment-decoding`
- `probe:js-query-encoding`
- `probe:core-query-decoding`
- `probe:net48-query-plus-space`
- `probe:js-header-values`
- `probe:js-header-trim`
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
- `X9.10` behavior: a route value read as "a/b" sends no request and runs its `Finally`.
- `X9.11` behavior: a query member read as "a b+c&d" reaches the endpoint as "a b+c&d".
- `X9.12` refusal: a fixed header value with a line break: "The header {name} holds a line break, which HTTP does
  not carry in a field value; send it in the body."
- `X9.13` behavior: a header read as text past US-ASCII sends no request, and the console names the header.

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
- D10.14 Retry is one standard policy, carried by the request defaults and taken by every request that does not opt
  out. It retries what a second attempt can answer differently, and nothing else:
  - a network failure, and the statuses 408 and 429, which ask the client to try again (RFC 9110, section 15.5.9;
    RFC 6585, section 4), and 502, 503 and 504, a gateway or an overloaded server (RFC 9110, section 15.6);
  - never another client error: 400, 401, 403, 404, 409 and 422 say the request is wrong, and a second attempt
    gets the same answer;
  - never 500, 501 or 505: in an MVC app the server is the app, its 500 is its own exception, and the same request
    meets it again, after a wait the user spends behind the loader;
  - only GET, PUT and DELETE, which HTTP defines as idempotent (RFC 9110, section 9.2.2). A POST is never retried:
    a failed POST's effect can stand on the server, and a second attempt would repeat it.

  Its numbers are .NET's standard resilience handler's: 3 retries after the first attempt, an exponential wait with
  jitter from 2 seconds, a 429's or 503's `Retry-After` in place of the wait, and 30 seconds in all, past which the
  retries end. The app sets the attempts and the first wait once, in the defaults. Another policy is a later
  extension, the owner's.
  - `Retry-After` is the server's word when it sends one (RFC 9110, section 10.2.3): its seconds, or its HTTP date
    taken against the response's own `Date`, so the browser's clock does not enter. A wait that would pass the
    30 seconds ends the retries at once, and the error route runs. A value in neither form is the server's
    defect: it is logged once as an error, and the standard wait runs.
  - Retrying only when `Retry-After` is sent is refused: a network failure has no response, and a gateway's 502 or
    504 comes from no app server, so neither carries the header, and those are the failures a retry exists for.
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

- D10.18 What each step reads is stated by when it reads, and nothing is frozen.
  - A request's argument is read once, in the turn the request starts, and serialized as it is sent, so the body is
    a snapshot by construction. A `Parallel`'s requests start in one turn and send what the page held then.
  - A response's data belongs to its run. The plan has no door that writes into it, and the generated contract types
    it read-only, so the runtime's own code cannot write into it either. A step that hands it, or any data value, to a
    vendor or a plugin (a `set` or a call argument) hands a structured copy (`probe:js-structured-clone-separate`).
    What a component does with what it is given is its own: one that edits it edits its copy, and a `Chained`
    request's gather, a `Finally` and the steps after a `Parallel` read the response as the server sent it. Evidence
    that a consumer edits what it is given: `probe:ej2-data-manager-edits-source`, a fact only; the core takes no
    vendor library.
  - Freezing is refused: a write into a frozen record throws (`probe:js-frozen-write-throws`), so a frozen value
    breaks any component that edits what it is given.
  - The page is shared and live. A step reads a component or an element when it runs. `Parallel`'s routes run in
    the order their responses arrive, which the server decides, and each reads the page as the routes before it left
    it; a developer who needs an order writes `Chained`. The steps after `Parallel` read the page once every branch
    settled.
  - A `let` binds once, in its block (D8.8). After an awaited step, the binding holds what was bound, and a read of
    the page holds the page now.
  - An event's payload is the vendor's object, written in the vendor's turn (D11.4) and copied like any data when
    handed on.
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
- `N10.3` new: the standard retry policy of D10.14 in the defaults, its attempts and first wait, and a request's
  opting out; the door's form goes to gate 2.
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
- `X10.9` type: a policy that retries a POST cannot be written: new:N10.3.
- `X10.7` behavior: on net48 under IIS, a 400 with validation errors shows each message at its input; the
  `verify-net48` run proves it.
- `X10.10` behavior: a nullable member whose condition fails is not in the body the request sends, and the
  endpoint answers with the member's starting value, not null.
- `X10.11` behavior: a chained request whose condition fails is not sent, and the first request's success
  steps and the steps after it ran.
- `X10.12` behavior: a request that writes only its method, URL, form and success shows a 400's errors at its
  own form, through the defaults' route.
- `X10.13` behavior: a component that edits a row it was given from a response leaves the response as sent, and
  a `Chained` request that reads the response's rows sends the server's rows, not the component's.
- `X10.14` behavior: two `Parallel` requests whose routes write one text: the page shows the text of the route whose
  response the test app sends last.
- `X10.15` behavior: a GET answered 500 runs its error route once, with no second attempt; a GET answered 503 and
  then 200 runs its success route.

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
  sees what they write to the event's payload and what they call on it. After an awaited step, each member
  kind of the payload has one meaning:
  - a read reads the payload as it is then;
  - a call runs: a vendor that hands over a method expects it called later, as EJ2's filtering payload's
    `updateData` once the page has fetched the matching items;
  - a write to a property of the payload itself is refused where the view renders, since the vendor read it
    when its callback returned (`cancel`, `preventDefaultAction`). To keep a drawer open while the user
    decides, the handler writes `Cancel` first, then asks, then calls the drawer's close method on yes;
  - a write to a property of an object the payload holds is a write on that object, and runs.

  The refusal is each handler's own: an await in one handler refuses no write in another. It follows the path
  the handler runs: a branch reads the state before its `if`, so an `Else` writes the payload in the vendor's turn
  when its sibling `Then` awaits, and a step after the `if` reads the payload as awaited when any of its branches
  awaits. An `ElseIf`'s branch and the `Else` after it read the state after the conditions before them, so an
  `Else` after an `ElseIf` whose condition awaits reads the payload as awaited (X11.13).
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
- kept: `Method`, `Method<T>`, `MethodAsync`, `MethodAsync<T>`, `Parallel`, the hub's `invoke` and `send`; `Confirm`
  is carried by N17.3 over a component's `MethodAsync<bool>` (D17.9).
- `N11.1` new: a handler's policy, `overlap`, `replace` or `skip` (D11.7); its door's form goes to gate 2.
- `N11.2` new: the refusal at render of a write to a property of the trigger's payload itself after an awaited
  step (D11.4).

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
- `X11.1` refusal: a handler that writes a property of its payload after an awaited step (an async method, a
  request, a `Confirm`) is refused where the view renders with "A write to the event's payload after an awaited
  step reaches no vendor: write it before the first request, Confirm or async method."
- `X11.2` behavior: a drawer whose close handler writes `Cancel` and then asks stays open while the dialog
  asks, and closes on yes.
- `X11.3` behavior: a method declared with `Method` whose vendor returns a promise stops its pipeline, and the
  step after it does not run. The console holds the stop's one error (D11.3), which a test does not read
  (`.claude/rules/tests.md`, rule 4).
- `X11.4` behavior: a `MethodAsync` that rejects stops its pipeline, and its request's `Finally` hides the
  loader.
- `X11.5` behavior: a slot unloaded while its request is in flight runs no route of that request, and the
  page's loader its `WhileLoading` showed is hidden.
- `X11.6` behavior: under `replace`, a cascade whose first answer arrives after the second shows the
  second's items.
- `X11.7` behavior: under `skip`, a save clicked twice while its request is in flight sends one request.
- `X11.8` behavior: a filtering handler writes `PreventDefaultAction`, fetches the matching items and calls
  `UpdateData` with them, and the list shows the fetched items (`probe:ej2-filtering-args-members`).
- `X11.9` behavior: after an awaited step, a write to a property of an object the payload holds changes that
  object.
- `X11.10` behavior (D11.5): two handlers of one click run in the order the plan subscribed them, the first to its
  awaited step and the second to its end, and the first goes on when its promise resolves.
- `X11.11` behavior (D11.4): a monitor reads the `cancel` its alarm's handler wrote before its first await, and
  sounds when the handler writes none; after the await, the patient the alarm holds is written, the alarm's own
  method runs and a read of the alarm reads the bed the monitor moved it to; a handler written after one that
  awaits writes its own alarm before its own await.
- `X11.12` behavior (D11.4): an alarm's `Else` writes `cancel` in the monitor's turn while its sibling `Then` awaits;
  a write after an `if` whose branch awaits is refused with X11.1's message.
- `X11.13` behavior (D11.4): an alarm's `Else` after an `ElseIf` writes `cancel` in the monitor's turn; after an
  `ElseIf` whose condition awaits an async method, the write is refused with X11.1's message.

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
- D12.8 An event of a live object a pipeline reaches (an object an event's payload holds, an object a method
  returns, an item of a list) is listened to by a step written in that pipeline, `listen`, with a vendor's
  listening, a payload declaration and a policy, as a handler has. Its listener:
  - belongs to the owner of the plan whose pipeline added it, and leaves at that owner's release (D12.5);
  - is added once per object and step: the same object reached again adds no second listener, and another
    object gets its own;
  - holds its object weakly, so the object lives as long as its vendor keeps it: at release a living object's
    listener is removed by its listening kind, and a collected object took its listener with it;
  - runs its own pipeline, whose payload is its own listener's arguments.
  Ownership.tla gains the step and its invariants with POC 14, and each becomes a Playwright test.

**Schema nodes.** The plugin data `form` and `hub`, the plugin `alis.slot` (data `none`, its key the container id),
the statement `listen`, and `handler`. Ownership is not a node: a plan's owner is where its script is, and an owner's holds are what its
plans name.

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
- `N12.1` new: `listen`, the handler of an event of a live object a pipeline reaches (D12.8); its form goes to
  gate 2. It replaces the refusal of an event declared on an object a method returns.

**Evidence.**
- The redesign refuses an event declared on an object a method returns and states nothing for an object a payload
  holds (`Alis.Reactive/Components/JsObject.cs:334-341`); D12.8's `listen` carries both.
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
- `X12.9` behavior: an object a payload hands over raises its own event, and the pipeline its `listen` step names
  runs once per raise; handed over again, it still runs once per raise.
- `X12.10` behavior: after the slot whose handler listened to a payload's object unloads, that object's event runs
  nothing.

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

- D13.2 A plan's id keys nothing: it names the plan in traces only, and many plans of a page share one id freely.
- D13.3 A read of a member by its binding name belongs to a form. It reads the input inside the form's element
  that binds the name, as the form sends it. A page-wide read by binding name is refused: two views can bind
  one name, so on a page a name names no single input. That ends the chain of guesses that read the last
  contributed input, then the plan's own. A read of a member's input outside a form is the component read
  by its element id, which Q14 derives the same way in every view that renders the member.
- D13.4 Two inputs inside one form that bind one scalar binding name stop the request or the check that reads
  them, with one logged error that names both element ids. Form data binds the first value and JSON the last
  on both hosts (`probe:core-form-duplicate-name`, `probe:core-json-duplicate-name`,
  `probe:mvc5-form-duplicate-first`, `probe:mvc5-json-duplicate-last`), so the browser cannot know which one
  the endpoint binds. A list's inputs carry indexed names and are not duplicates.
- D13.5 A form's rule for each item is written once, by a path with `each` (D1.5). It checks every item row inside
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
  - A lambda is bound only when MVC's name names each step it takes from the model: a member, a single-argument
    indexer, an array position. MVC stops its name at any other call (`Find(..)`, `Substring(1)`) and keeps what lies
    outside it, so the form would post the input under a name that binds nothing; the framework counts the steps
    MVC's name names against the lambda's and refuses a lambda they differ for, with no read of the call's method.
  - No view on either line pairs MVC's `LabelFor` or `IdFor` with a framework input (`git grep` finds none at
    623dd4a27 or 65fdf118d), so the framework's id needs no agreement with them, and a view names the id by
    `IdGenerator.For`.
  - The framework controls every input's id (the owner, 2026-11-06): without it the browser guesses which
    input a rule, a message or a read means, and views of different partials that share an id conflict.
  - Reading the declaring class of the member the expression tree names is a type's identity, within the
    no-reflection rule, and the same read D18.3 names a member by (D18.1).
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
    - R14.1 an input or a partial whose lambda MVC's name does not name step by step from the model (a call that
      is no indexer, a value the view holds), an input whose lambda names no member, or a member an interface
      declares;
    - R14.2 an input, or a pipeline's read of one, under a prefix its view took after its plan started;
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
- `N14.4` new: `pipeline.Input(m => m.Member)`, the input a member binds, reached by the id D14.3 gives it
  (`.Component<T>()`). It replaces both lines' `Component<TComponent>(Expression<Func<TModel, object?>>)`, whose
  object-typed lambda ALIS009 refuses; its form goes to gate 2.

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
- D15.1 The core depends on no consumer. Syncfusion's components, the native components and every component an app
  brings are consumers, built on the core's public surface with the dependency inverted (the owner, 2026-11-06).
  The core has no template node, no template plugin and no vendor's name, and it exposes nothing past what
  onboarding a component needs.
- D15.2 Templates are the Fusion module's, and it stays as it is, not rewritten (the owner, 2026-11-06): the typed
  HTML builder `FusionTemplate.Create<T>()`, its element doors and its conditions, rendered to text that
  Syncfusion's engine compiles (`Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:25`). The engine is a
  JavaScript dependency that the bundle always holds, and the server takes none. D8.4 stands: template conditions
  are the engine's, not the core's.
- D15.3 What the engine does is pinned, so the module's three fixes are exact:
  - It writes a read into the HTML unencoded, so markup in data becomes live elements
    (`probe:ej2-template-unencoded`). The builder writes each read through a text helper that the Fusion module's
    browser code supplies, which the engine calls by name (`probe:ej2-template-helper-encodes`). Data is then text.
  - It throws on a member past a missing holder (`probe:ej2-template-missing-holder`), so one row stops the grid's
    rendering. The builder writes a read past a holder the model declares nullable inside the engine's condition on
    that holder (`probe:ej2-template-guarded-holder`), so the row shows nothing there and the others render.
  - Member names are the data's JSON names under the app's options (D2.9), not the camel case the writer
    hard-codes today.
- D15.4 Each of the three fixes lands in the Fusion module, and none reaches the core.

**Schema nodes.** None: a template is a component property's text value, written by a consumer.

**Checker.**
- `probe:ej2-template-unencoded`
- `probe:ej2-template-helper-encodes`
- `probe:ej2-template-missing-holder`
- `probe:ej2-template-guarded-holder`
- `probe:ej2-template-condition`
- `probe:core-json-result-names`

**Doors.**
- kept: `FusionTemplate.Create<T>()` and its element doors (`Text`, `Span`, `Div`, `Badge`, `Icon`, `Img`, `Link`,
  `EventButton`, `Raw`), its conditions, and the template properties of each component, all in
  Alis.Reactive.Fusion.

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

## Q16. Validation

**Decision.**
- D16.1 One fact. A rule is written once, on a `ReactiveValidator<T>` in FluentValidation's grammar. The server
  runs it, and C# derives the browser's rules from it where the view renders; nothing else writes a browser rule.
  Every mechanism below serves the cases it names and no other (the owner, 2026-11-06).
- D16.2 A check is one validator under one rule set, and that validator is the request's model's.
  - The request's model is the type its endpoint binds. A part of a model saved alone is its own model with its
    own validator (the owner, at 65fdf118d). `Validate<TValidator>` takes only `TValidator : ReactiveValidator<TRequest>`.
  - For a check, the browser runs that validator's and rule set's registrations on the form, from every live plan,
    each under its plan's prefix. A registration of another validator on the same form takes no part in the check.
    The registrations are keyed by form, validator and rule set, so composing plans needs no merge by plan id (D14.2).
  - The condensed default (D10.17): a request whose model has a registered validator is checked by it under the
    default rule set, with its messages in the form it sends. Naming a rule set, or no check, overrides it; the
    form of the sentence goes to gate 2.
- D16.3 A partial is attached or independent, and both nest (the owner, 2026-11-06).
  - Attached: a partial rendered for a member (`Html.PartialFor`, D14.4) has its own model and its own validator,
    and the parent's validator holds it with `SetValidator` or `ChildRules` on that member. One model has as many
    associations as its members, at any depth, and each partial's rules are written once and read by both sides.
  - Independent: a partial loaded into a slot where a condition holds (a state whose form needs 10 fields,
    another's 20) is its own model, plan, validator and requests. Its checks run its own validator over what its
    requests send, and its registrations leave with its slot (D12.4), whatever the page's validator holds.
  - A partial inside a partial takes the full prefix chain as its binding names (D14.3), and the release order
    of nested owners is Q12's.
  - A member a partial no longer sends binds null. `SetValidator` skips a null member on the server
    (`Alis.Reactive/Validation/Rules.cs:547`), so the browser's rules of a child validator hold only while the
    member is present in the checked body.
- D16.4 The server's verdict is one framework door on the endpoint, on both hosts (N16.1). It binds the request's
  model, runs the request's validator under the check's rule set on the bound model, and answers D10.8's body.
  - It adds the binder's refusal of a text a member's type cannot hold, under the member's binding name with the
    message of D16.7, and runs no rule of that member.
  - MVC's own validation adds nothing to it: neither data annotations nor the required error net10 adds for a
    non-nullable text posted empty (`probe:core-form-implicit-required`). Under `[ApiController]` that error answers
    400 before the action runs (`probe:core-api-controller-implicit-required`), so on net10 the framework's
    registration marks each type a registered `ReactiveValidator` validates as one MVC does not validate, through a
    metadata provider, and the binder's own errors stay. The mechanism is a gate-1 line. On net48 the framework's
    bodies never meet MVC 5's implicit required: a value type that reads nothing is left out (D9.10).
  - The sandbox's endpoints call `Validate` and write `ErrorsByField` themselves, and read no binder refusal
    (`Alis.Reactive.SandboxApp/Areas/Sandbox/Controllers/Components/AppLevel/DrawerController.cs:48-51`).
- D16.5 The browser decides on the body its request sends, and its verdict equals the door's on the model the
  endpoint binds from that body.
  - The checked body is the endpoint's binding of the sent body. A sent member reads as the endpoint binds it:
    in form data an empty text binds null on both hosts (`probe:core-form-implicit-required`,
    `probe:mvc5-empty-text-null`). A member the body lacks reads its starting value (D16.6).
  - Decision C: a request that sends no form element is checked on the members it sends beside it and on the
    starting values of the rest, and the form's inputs it does not send are not read. This replaces 65fdf118d,
    which read the validated form's inputs for such a request.
  - A check with no request (`Validate`) and live validation read the form's inputs, as the form sends them.
- D16.6 Decision B: a starting value is the one the endpoint's binder leaves, for the request's host and body
  format. A member the body lacks reads from its nearest holder the body sends, at any depth, and that holder
  starts as this table says:

  | The nearest holder the body sends | form data, and JSON on net48 | JSON on net10, the member replaced | JSON on net10, the member populated |
  |-----------------------------------|------------------------------|-------------------------------------|-------------------------------------|
  | none: the model itself | the model's start | the model's start | the model's start |
  | an object the member starts with | that object, as the holder above starts it | a new object of its type | that object, as the holder above starts it |
  | an object that starts null | a new object of its type | a new object of its type | a new object of its type |
  | a list item | a new item of its type | a new item of its type | a new item of its type, after the items the list starts with |

  - On net10 each JSON member is replaced or populated as the app's contract says for it, by its own attribute or
    the options' `PreferredObjectCreationHandling` (`probe:core-stj-populate-start`); replaced is the default
    (`probe:core-json-nested-start`). A populated list keeps the items it starts with and adds the sent ones.

  - Facts: `probe:core-form-nested-start`, `probe:core-json-nested-start`, `probe:core-form-unsent-keeps-start`,
    `probe:mvc5-nested-fills-start`, `probe:mvc5-list-item-new`, `probe:mvc5-list-replaced`, and MVC 5 binds JSON
    through the same binder (`probe:mvc5-json-binds-through-binder`).
  - A new object's start is the JSON System.Text.Json writes, under the app's options (D2.9), for the object it
    reads from `{}` (`probe:core-stj-empty-object-start`). C# writes the start of each member a check reads, where the
    view renders, from the binding of the path down to it, as D4.2 reads data: a populated member's start hangs on
    its holder's, so one start per type cannot hold it (`spec/NOTES.md`, the second start probe). The model's path
    starts at the validator's `T`, an item's at `RuleForEach`'s `TElement`. The plan carries each in the form's data.
  - No hand reflection builds a start: the `Activator`, `GetConstructors` and `GetValue` reads of
    `ModelDefaults` leave (B11).
  - A type the endpoint's binder cannot create is refused where the view renders: on net48, a type with no
    public parameterless constructor (`probe:mvc5-create-model-parameterless`).
- D16.7 A text an input holds that its member's type cannot hold fails that member before its rules, with the
  binder's message, "The value '{text}' is not valid for {name}." (`probe:core-form-value-messages`,
  `probe:mvc5-value-invalid-message`), and the request is not sent.
  - {name} is the member's name as the validator names it, and the door writes the same text.
  - The member's other readers read its start, as the endpoint binds it.
  - This is the one binder behavior the browser repeats. It closes the silent save: today such a text reads
    nothing (D3.3), is left out of the body (D9.10), and the endpoint saves the start.
- D16.8 Conditions are Q8's. A validator's `When`, `Unless` and `Otherwise` and a rule's `When` and `Unless`
  write the one condition core over the checked body.
  - What reaches the browser is the closed list the translator reads today (`Alis.Reactive/Validation/Rules.cs:76-80`).
    A comparison of two members joins it, since the core takes a source on both sides (B9).
  - A condition outside the list is refused where the view renders unless `WhenOnServer` or `UnlessOnServer`
    declares it the server's. A rule under a server's condition stays whole on the server.
- D16.9 What stays on the server by what it is, decided by the door alone, with no browser rule:
  - `Must`, `MustAsync`, `Custom` and `CustomAsync`;
  - `WhenAsync`, `UnlessAsync`, `WhenOnServer` and `UnlessOnServer`;
  - a message built from the model;
  - `.ServerOnly()`;
  - a child validator that is not a `ReactiveValidator`.
- D16.10 The framework writes no regular expression (the owner, 2026-11-06: regexes are never a good choice and
  are used sparingly). Each check the framework writes on both sides is one algorithm in C# and in the browser, or
  the platform's standard parser on each side, and its probe rows give one verdict on both:
  - `EmailAddress`: one `@`, neither first nor last, as the server counts it today
    (`Alis.Reactive/Validation/Checks.cs:166-170`);
  - `CreditCard`: the Luhn sum over the digits once spaces and dashes are taken out (`Alis.Reactive/Validation/Checks.cs:180-193`);
  - `PrecisionScale`: the digits and decimals of the number;
  - `IsInEnum`: membership in the enum's names and numbers;
  - `Url()`: an http or https URL by the standards, parsed by `System.Uri` on the server and by the URL Standard's
    `URL` in the browser, scheme `http` or `https`. The two parsers agree on every probe row, where today's browser
    pattern disagrees with the server on two (`probe:core-uri-absolute`, `probe:whatwg-url-http`, `probe:js-url-pattern`).

  The browser patterns of today's checks leave (`Alis.Reactive/PlanModel/Validation/BrowserRule.cs:150-152`), and so
  does the expression that reads a developer's pattern for .NET-only syntax (`Alis.Reactive/Validation/Checks.cs:149-151`).
- D16.11 A regular expression reaches the browser only where a developer writes `Matches` or `Regex.IsMatch` in a
  list's lambda (D6.15), and only as one that both engines read the same. One reader in C# reads every pattern where
  the view renders. .NET and JavaScript read `\d`, `\w`, `\s`, `.` and `$` differently, and `IgnoreCase` follows
  the view's culture on .NET (`probe:core-regex-dotnet`, `probe:net48-regex-digit-unicode`, `probe:js-regex-raw`,
  `probe:core-regex-space-and-case`, `probe:js-regex-space-and-case`).
  - The browser's copy carries no flag, so JavaScript reads it by UTF-16 code unit, as .NET does. The constructs the
    reader copies are closed: a character, an escaped metacharacter (`\^$.|?*+()[]{}/`, and `-` inside a class),
    `\t`, `\n`, `\r`, `\f`, `\v`, `\uXXXX` and `\xXX`; a class and a negated class of those, with ranges; a group,
    `(?:)`, lookahead and lookbehind; `|`, `^`, and the quantifiers `*`, `+`, `?`, `{n}`, `{n,}` and `{n,m}`, each
    with its lazy form. Over 53 patterns and their inputs, .NET 10 and V8 agree on every pattern of that set
    (`spec/NOTES.md`, 2026-11-06, the probes for unit 3d).
  - The framework rewrites no pattern but `$`. A pattern that holds any other construct, or an option other than
    `CultureInvariant`, is refused where the view renders, naming the construct and the text that means the same in
    both engines (`[0-9]` for `\d`, `^` for `\A`, `$` for `\Z`, `[^\n]` for `.`), or the server when no text does,
    unless the rule is `.ServerOnly()`.
  - `IgnoreCase` is refused, naming the door, both cases spelled (`[Aa]spirin`): no JavaScript flag folds case as
    .NET's `IgnoreCase` with `CultureInvariant` does. Over the 2,474 case pairs of the BMP the two engines' mappings
    reach, .NET differs from JavaScript's `i` on 166 (the Kelvin sign against `k`) and from `iu` on 100 (`ſ` against
    `s`, `µ` against `μ`, the Greek final sigma and symbol forms), and the `u` flag reads a negated class by code
    point (`spec/NOTES.md`, 2026-11-06). The probe this decision first rested on checked the Kelvin sign and the
    dotted I only.
  - `$` is the one construct with no spelling a developer writes the same for both engines, so the framework writes
    the browser's copy of an unescaped `$` outside a character class as `(?=\n?$)`, the form that reads as .NET's
    (`probe:js-regex-translated`). Every other construct is refused with its common spelling. A `$` under
    `Multiline` is refused like any other option.
  - A single-line input's value holds no line break (`probe:html-input-line-breaks`), so that copy matters for a
    text area and for a member no input holds.
- D16.12 The validation spec of step 5 checks that the two sides give one verdict on every body of its domain, for
  each check of D16.10 and each condition of D16.8.
- D16.13 A message's place is carried in the plan, never read from the page (B5).
  - C# writes each input's message element id beside the input's id, so the `_error` id convention leaves.
  - The form's summary lists every failure of the check, and each failure also shows at its input's message
    element when the checked body's member has an input in the form. Both are MVC's own pattern: a validation
    summary that holds property errors, and a message for each field.
  - No DOM visibility is read, so a message inside a hidden panel is still in the summary. The form of the
    summary goes to gate 2.
- D16.14 `FormField` is designed once (P5):
  - its path as Q1's steps, every item as `each` (D13.5);
  - its value, the one read (Q2) of the checked body;
  - its tests, each a condition (Q8), a check and a message.

  The form's data carries the starts of D16.6 once. `$model.sentValueOf` and `$model.sends` leave, since every
  read is a `get` over one checked body (B3), and the form's plugin argument is a typed contract (D9.1).
- D16.15 Live validation is D12.7's: typing clears an input's message, and blur and the input's commit check it
  against the check with no request.

**Schema nodes.** `fn`, `get`, `as`, `op`, `if`, `record`, `list`, `lit`; the plugin data `form`, with its
registrations and starts as a typed contract (Q19); `$form.validate` and `$form.showErrors` as plugin calls. The
grammar gains no node (G2).

**Checker.**
- `probe:core-form-nested-start`
- `probe:core-json-nested-start`
- `probe:core-form-unsent-keeps-start`
- `probe:mvc5-nested-fills-start`
- `probe:mvc5-list-item-new`
- `probe:mvc5-list-replaced`
- `probe:mvc5-json-binds-through-binder`
- `probe:core-stj-empty-object-start`
- `probe:mvc5-create-model-parameterless`
- `probe:core-form-implicit-required`
- `probe:core-api-controller-implicit-required`
- `probe:core-stj-populate-start`
- `probe:mvc5-empty-text-null`
- `probe:core-form-value-messages`
- `probe:mvc5-value-invalid-message`
- `probe:core-regex-dotnet`
- `probe:net48-regex-digit-unicode`
- `probe:js-regex-raw`
- `probe:js-regex-translated`
- `probe:core-regex-space-and-case`
- `probe:js-regex-space-and-case`
- `probe:core-uri-absolute`
- `probe:js-url-pattern`
- `probe:whatwg-url-http`
- `probe:js-regex-translated`
- `Ownership.tla:RulesLeaveWithTheirOwner`
- `Validation.tla:Agreement`, the browser's failures equal the door's over host, body format, creation handling,
  holder start, list item, decision C, the validator's scope and a typed text, with one invariant per aspect, each
  broken by a mutant that is today's reading at 65fdf118d:
  - `Validation.tla:ConditionAgrees`
  - `Validation.tla:ObjectStartAgrees`
  - `Validation.tla:NewHolderAgrees`
  - `Validation.tla:ChildSkipAgrees`
  - `Validation.tla:ListStartAgrees`
  - `Validation.tla:EmptyTextAgrees`
  - `Validation.tla:UnsentInputsUnread`
  - `Validation.tla:OnlyTheNamedValidator`
  - `Validation.tla:NoTypedValueLost`
- The four gaps the code had at bff9eb6c0 are ValidationToday's invariants, each a counterexample TLC prints from
  its own cfg: AnUnrenderedMemberRendersItsView, AListNotSentReadsItsStart, AConditionReadsTheObjectsStart and
  AnItemsListNotSentReadsTheItemsStart.

**Doors.**
- kept: the validator grammar of `ReactiveValidator<T>`:
  - `RuleFor`, `RuleForEach`, `Include`, `RuleSet`, `When`, `Unless`, `WhenOnServer`, `UnlessOnServer`,
    `WhenAsync`, `UnlessAsync`, `Otherwise`, `Validate`, `ValidateAsync`, `InlineValidator<T>`;
  - the checks `NotNull`, `NotEmpty`, `Empty`, `Equal`, `NotEqual`, `GreaterThan`, `GreaterThanOrEqualTo`,
    `LessThan`, `LessThanOrEqualTo`, `InclusiveBetween`, `ExclusiveBetween`, `Length`, `MinimumLength`,
    `MaximumLength`, `Matches`, `EmailAddress`, `CreditCard`, `Url`, `PrecisionScale`, `IsInEnum`, `Must`,
    `MustAsync`, `Custom`, `CustomAsync`, `Add`;
  - the options `WithMessage`, `WithName`, `When`, `Unless`, `WhenOnServer`, `UnlessOnServer`, `ServerOnly`,
    `Cascade`, `DependentRules`, `SetValidator`, `ChildRules`;
  - `AddReactiveFluentValidation`, `ReactiveFluentValidationBuilder.Add<TValidator>()`, `ValidationResult`.
- refused: `AddFromAssembly` and `AddFromAssemblyContaining<T>`, an assembly scan by reflection (B11). Their
  capability is carried by `Add<TValidator>()` per validator; an exception is gate 1's.
- refused as public: `BrowserRuleSet`, `BrowserRule`, `BrowserField` and `IBrowserRuleSource`, the plan writer's
  inside, which no view, validator or component author writes; the browser rules are carried by the validator
  grammar.
- `N16.1` new: the endpoint's validation door on both hosts. It binds, checks under the same validator and rule
  set, and answers D10.8's body or the bound model; its form goes to gate 2. It carries the endpoints' own
  `Validate` with `ErrorsByField`.
- `N16.2` new: the form's typed plugin data, its registrations keyed by validator and rule set, and the starts of
  D16.6.

**Evidence.**
- G10: a rule written once reaches both sides. `NotEmpty` reaches the browser, and `Must` stays on the server
  (`Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Validation/EmergencyContacts/EmergencyContactsValidator.cs:9-13`),
  and one item validator serves two request validators by `SetValidator` (`:17-34`).
- P5: `FormField` took three shapes in a week: with an id and a value; by binding name only (34c99d608); with
  `items` and `value` functions (65fdf118d), written at `Alis.Reactive/PlanModel/Validation/FormWriter.cs:29`.
- The unsent read starts a level only at an item step (`Alis.Reactive/PlanModel/Validation/ModelDefaults.cs:32-44`),
  and reads a holder a new model leaves null as null (`:61`). So a member of an object that starts null, and the
  JSON body on net10, read starts the endpoint does not bind.
- Starts are built by reflection (`Alis.Reactive/PlanModel/Validation/ModelDefaults.cs:115-155`).
- A child validator's browser rules carry the rule's scope and no holder condition
  (`Alis.Reactive/PlanModel/Validation/BrowserField.cs:131-143`), while the server skips a null member
  (`Alis.Reactive/Validation/Rules.cs:547`).
- The browser runs every registration on the form, whichever validator the check names
  (`Alis.Reactive.Assets/runtime/plugins/form/form.ts:128-130`).
- B5: inline or summary is read from DOM visibility (`Alis.Reactive.Assets/runtime/plugins/form/display.ts:64-71`),
  and the message element by the `_error` id (`:73-75`).
- The server runs a condition, a member read and a fixed operand by `Expression.Compile`
  (`Alis.Reactive/Validation/Rules.cs:52`, `Alis.Reactive/Validation/Rules.cs:61`,
  `Alis.Reactive/Validation/ModelMember.cs:62`, `Alis.Reactive/Members/Expressions.cs:28`); the four go to gate 1's
  list of reflection exceptions.
- A pattern with no JavaScript form stays on the server unannounced (`Alis.Reactive/Validation/Checks.cs:128-129`),
  and the rest pass to the browser with .NET's flags (`Alis.Reactive/Validation/Checks.cs:156-159`).
- The browser's Url pattern (`Alis.Reactive/PlanModel/Validation/BrowserRule.cs:152`) against the server's
  `Uri.TryCreate` (`Alis.Reactive/Validation/Checks.cs:206-207`).
- G2: validation adds no grammar node; a check is `$form.validate` over plugin data, and the body it checks is a
  `fn` the plan writes (`Alis.Reactive/PlanModel/Validation/FormWriter.cs:19-30`).

**Defects.**
- `X16.1` type: a request checked by a validator of another model does not compile: new:N16.2.
- `X16.2` behavior: on net10, a JSON save whose model starts its lead with a name, and whose lead type starts the
  name empty, sends only the lead's phone; the browser shows "Lead name is required." before sending, as the
  endpoint answers.
- `X16.3` behavior: a form-data save sends one member of a deputy that starts null, whose type starts its name with
  text; the browser lets the save through, and the endpoint saves it.
- `X16.4` behavior: typing "12a" in a count bound to an int shows "The value '12a' is not valid for Count." and
  sends nothing.
- `X16.5` behavior: with the Texas partial loaded the save is checked by the Texas rules; after the page loads
  the Massachusetts partial instead, the save is checked by the Massachusetts rules only, and the endpoint agrees
  on each.
- `X16.6` behavior: a request that sends only the status beside the form is checked on the status and the starts,
  and the name typed in the form is not read.
- `X16.7` behavior: a failure whose message element sits in a hidden tab shows in the summary.
- `X16.8` behavior: on net10, a non-nullable text posted empty with no rule on it saves, and the door adds no
  "The Name field is required.".
- `X16.9` behavior: "http://a.com/b c" passes `Url()` in the browser and on the server, and "http://a.com:99999"
  fails both.
- `X16.10` behavior: a `Matches(@"^[0-9]+$")` on a single-line input refuses Arabic-Indic digits in the browser and on
  the server alike.
- `X16.11` refusal: a pattern with `IgnoreCase` and no `CultureInvariant`: "The pattern '{pattern}' ignores case by
  the view's culture, which the browser cannot follow; add RegexOptions.CultureInvariant, or mark the rule
  .ServerOnly()."
- `X16.12` refusal: a pattern with a construct the engines read differently: "The pattern '{pattern}' uses
  {construct}, which .NET and the browser read differently; write {same} instead, or mark the rule .ServerOnly()."
- `X16.13` refusal: a condition with no browser form: "The condition '{condition}' has no browser form. Write it as
  tests of model members against fixed values or other members, or declare that the server alone decides it:
  WhenOnServer or UnlessOnServer."
- `X16.14` refusal: on net48, a start the binder cannot create: "{Type} has no public parameterless constructor, so
  the endpoint cannot bind it; give {Type} a public parameterless constructor."

## Q17. Vendors and inversion

**Decision.**
- D17.1 The dependency is inverted (the owner, 2026-11-06). The core depends on no consumer, and no consumer is
  special:
  - Syncfusion's components, the native components and every component an app brings are consumers, built on the
    core's public surface. Fusion and Native get nothing an app does not.
  - The core exposes only what onboarding a component needs, and it takes no vendor library: the framework's own
    typed request lifecycle feeds components, and no vendor's data layer is the core's.
  - The framework onboards no component itself; any developer onboards one.
- D17.2 A component, shipped or brought, is one declaration class and a render helper. The declaration names the
  component's members and events by their JavaScript names, and the render helper renders its element. Every
  component reaches the browser through the core, with no TypeScript change: a component that seems to need one
  means the plan is missing information, and C# carries it.
- D17.3 A vendor is data that a declaration carries (G5), in closed sets the one driver follows:
  - where the object lives, one of two: a path from the element (Q1), empty for the element itself, a member
    such as `tomselect`, an index such as `ej2_instances.0`; or a lookup the vendor's browser code registers as
    a plugin, called with the element, for a library that keeps its objects off the element (`Chart.getChart(canvas)`,
    `bootstrap.Modal.getInstance(element)`, a registry a component's own code keeps). The plan writes either as a
    `fn` of the element, read by the one evaluator, so finding an object is no second resolver, and the plan
    reads no global by name;
  - how a listener subscribes, one of three: a subscribe and unsubscribe method pair; a property the driver
    assigns the listener to, its previous value put back at release; a subscribe method that returns the
    function that removes the listener (`const off = store.subscribe(listener)`), which release calls. A property
    holds one listener, so a second handler of the same object's callback property is refused where the view renders
    (X17.16); the first would be replaced without a word. The doors are `ComponentVendor.Define(name, objectPath,
    subscribe, unsubscribe)`, `ComponentVendor.DefineCallbackProperties(name, objectPath)` and
    `ComponentVendor.DefineReturnedUnsubscriber(name, objectPath, subscribe)` (N17.7);
  - what a listener receives (D17.4).

  An object is found when a step reaches it and when a handler subscribes; a component constructed on
  `alis:mount` is subscribed once it is (Q12). The runtime names no vendor. The core knows the DOM's `EventTarget` as the platform's own vendor, and no
  library's.
- D17.4 A listener receives its arguments, and the payload's declaration reads them (D5.6). The payload is the
  first argument unless a member names its position, `Argument<T>(n)`. A function the vendor hands over as an
  argument (Tom Select's `load(query, callback)`) is a method of the payload at its position, called as any method
  is. A `CustomEvent`'s `detail` is a member of the first argument. The plan carries a handler's payload as a path
  into its arguments, so the closed set of `argument`, `event` and `detail`, the `VendorEventPayload` enum (S8)
  and the runtime's `instanceof` reading of the payload (B12) leave. An `Event<TArgs>` reads its payload at `[0]` with
  its members required, since the declaration says the listener receives it; an `Event` with no payload reads none,
  `[]`, so a vendor that raises it with no argument runs its handler. The handler carries both, the path and its
  presence, and the runtime writes neither. A payload declaration reads the argument at a position with
  `Argument<T>(n)` and calls the function the vendor hands over at a position with `MethodAt(n, [..])`, as
  `arguments[n](..)` calls it in JavaScript; the handler binds the listener's arguments beside the payload. A
  callback's arguments read the arguments the vendor calls it with the same way (D17.12). Either door on any other
  declaration is refused where the view renders (X17.18).
- D17.5 A declaration follows its vendor's public contract, and the build checks it:
  - A member is declared only when the vendor's type declarations make it public. A `@private` member is a gap to
    name, and public members compose what it would do.
  - A method whose declared return is a `Promise` is declared with `MethodAsync`, and every other method with
    `Method` (Q4, Q11). FusionGrid's three exports drift today (B15).
  - The check is an analyzer over a contract file. The TypeScript compiler reads the vendor's type declarations at
    the pinned version and writes each public member, with its kind and whether it returns a promise, into a JSON
    contract kept in the repo. The analyzer reads that contract as an additional file and fails a declaration that
    differs from it. A developer who brings a component runs the same script on their vendor.
- D17.6 Every vendor package is pinned to one exact version, its sub-packages included, and its npm and NuGet
  versions move together (B16). The contract file of D17.5 is written again at every upgrade, so a vendor's drift
  fails the build at the upgrade.
- D17.7 A vendor's name is checked by its characters, a letter and then letters, digits, `_` or `-`, with no regular
  expression (D16.10).
- D17.8 Two reference components prove the vendor contract on paper in step 8, FusionMultiSelect and a native
  reference, each declaration against the schema, and later in the browser.
- D17.9 The core's C# names no plugin and no component a consumer provides. At 65fdf118d it does: the `Confirm` doors
  write a call to `alis.confirm`, which only Fusion's browser code provides, so a page without Fusion cannot confirm.
  Those doors leave the core. A user's decision is a component's declared `MethodAsync<bool>`, read in a condition as
  any source is (D8.6), and C# writes the awaited read before the branch, where its term is reached (D4.7).
- D17.10 The framework shapes its own app-level components (the owner, 2026-11-06: control them fully, leave the
  styling to the app), built only on the core's public surface, as an app's component is, so the core names none of
  them and they prove that surface. They live in Alis.Reactive.Native beside NativeDrawer and NativeLoader, and render
  semantic markup with stable `alis-` class names and no visual styling:
  - NativeConfirm is the platform's modal `<dialog>`, opened by `showModal`: the top layer, the page inert behind it,
    focus kept inside. `Ask` is `MethodAsync<bool>`. Escape is the dialog's `cancel`: it answers false and goes no
    further, so a drawer behind stays open. Asks take their turn in order, as today's do. Its title and button texts
    are the app's, configured once, where today's are fixed English. A run that ends while its ask is open closes it
    (D17.11).
  - NativeToast is one region per page in the top layer (`popover="manual"`): a status region for success and
    information, an alert for warnings and errors. Its text is set by `textContent`. A toast leaves after the app's
    time, paused while it is hovered or focused, and an error stays until it is dismissed.
  - FusionConfirm and FusionToast stay, as the Fusion module's.
- D17.11 A declared method receives the run's abort signal only where its declaration names an argument of that kind,
  so no vendor's method receives an argument it does not declare. The plan writes it as the expression `signal`,
  which reads the run's own signal (Q11, Q19).
- D17.12 A callback the vendor calls for an answer (a label formatter, a comparer, a predicate) is declared as a
  property or an argument whose value is a function, `Callback<TArgs, TResult>`, and a view writes the answer as an
  expression over the callback's arguments: `chart.TickLabel.Set(tick => tick.Label)`. The arguments' declaration is
  the first argument the vendor passes and reads the others by position with `Argument<T>(n)`, as an event's
  payload does (D17.4): a value accessor called with a field and a row reads the row at position 1. The plan writes
  an `answer` as the value, a function whose body reads the list of arguments it is called with by the name the plan
  gives. The browser assigns or passes the function, and each call evaluates its body with that list bound and
  returns the result to the vendor in its turn. A method that takes a callback declares the argument with
  `Answer(key)`, so `chart.SortTicks(tick => tick.Doses)` passes the function the same way. A body is an expression
  with no steps: a step written inside it is refused where the view renders, and so is an awaited read, since the
  body cannot wait. A method's result read in a body is its call, run on each call of the body (D5.11). A body
  that stops gives its vendor no value, so the stop is thrown to the vendor's call: a step whose write or call
  made the vendor ask stops its pipeline there, and a vendor that asks in its own turn receives the stop as the
  error its call throws. An event's listener owes its vendor nothing, so D4.8 keeps its stop from the vendor; an
  answer owes it the value, and no value is the plan's to invent. A callback that only reacts is an event, listened
  to by a method pair, an assignment or a returned unsubscriber (D17.3), and runs a pipeline as any handler does.
- D17.13 A render helper starts its component in the state the model gives it, written as the plan writes a literal.
  `field.ValueJson` is the member's value, read through MVC with no compile and written by the plan's one literal
  writer under the app's JSON options, `"null"` when a holder on the way holds nothing; `field.Json(options)` writes
  the helper's starting options the same way, so a date, an enum and a number reach the component's code in the
  forms the plan's own values take. After a failed post the value is the model's, as the action leaves it, on both
  hosts: net10 keeps no typed form of an attempted value (its converter is internal, and a value provider's result
  converts nothing), and MVC 5 converts one only by a `System.Type` (`ValueProviderResult.ConvertTo(Type)`), a type
  read the owner's list lacks. A typed component shows what the server decided; a text box keeps MVC's attempted text. A
  component that posts through a hidden input writes a number in the view's culture, the decimal separator the
  helper gives it and no grouping, since the server binds the body under the culture the view rendered under (D3.11).

**Schema nodes.** The plan's `vendors` map, one `Vendor` per name, holding its object path and its listening kind;
`handler`, which names its event and carries its payload kind, since the vendors map is shared by name across
plans (Q19).

**Checker.**
- `Ownership.tla:NoUnsubscribeOnDestroyed`
- `Ownership.tla:ListenersDoNotAccumulate`
- `Lanes.tla:PayloadWritesInTheVendorsTurn`
- `probe:ej2-methods-returning-promise`
- `probe:ej2-destroy-removes-instance`
- `probe:ej2-max-public-method-arity`

**Doors.**
- kept: `ComponentVendor.Define`, `ComponentVendor.DefineCallbackProperties`, `ComponentVendor.Native`,
  `ComponentVendor.Name`, `VendorEventPayload`, and the declaration vocabulary of `JsObject` (`Method`,
  `MethodAsync`, properties, events).
- `N17.1` new: the payload's place in its listener's arguments, declared with the event (D17.4); its form goes to
  gate 2.
- `N17.6` new: a vendor's lookup, a plugin called with the element, beside the path from the element (D17.3).
- `N17.7` new: the listening kind whose subscribe method returns the function that removes the listener (D17.3).
- `N17.8` new: `Argument<T>(n)`, and a payload method at a position (D17.4).
- `N17.9` new: `Callback<TArgs, TResult>` and its `Set` from an expression, and `Answer`, an answer passed as a
  method's argument (D17.12).
- `N17.2` new: the vendor contract file and the analyzer that checks declarations against it (D17.5).
- `N17.3` new: a condition over an awaited `MethodAsync<bool>` source, replacing `ConditionStart<TModel>.Confirm` and
  `ConditionExtensions.Confirm`; its form goes to gate 2.
- `N17.4` new: `NativeConfirm` and `NativeToast` in Alis.Reactive.Native, each a declaration and a render helper.
- `N17.5` new: the declaration's argument kind that carries the run's abort signal (D17.11).
- `N17.10` new: an input field's `ValueJson` and `Json(options)`, the starting value and options a render helper
  writes (D17.13); their names go to gate 2.

**Evidence.**
- A vendor's object is found by a path from the element alone (`Alis.Reactive/Vendors/ComponentVendor.cs:78-93`,
  `Alis.Reactive/Vendors/ComponentVendor.cs:109-122`), so a library that keeps its objects off the element cannot be
  declared today: N17.6.
- G5: the driver switches only on the listening kind (`Alis.Reactive.Assets/runtime/objects/vendor-driver.ts:28-33`),
  and a vendor is the data `ComponentVendor` carries (`Alis.Reactive/Vendors/ComponentVendor.cs:38-52`).
- B12: what a listener receives is read by `instanceof` (`Alis.Reactive.Assets/runtime/objects/vendor-driver.ts:89-92`).
- B15: FusionGrid declares its three exports with `Method()` (`Alis.Reactive.Fusion/Components/FusionGrid/FusionGrid.cs:207-213`),
  while EJ2's grid declaration returns a `Promise` from `excelExport` at line 3849 of its `grid.d.ts`, and 120 EJ2
  public methods declare one (`probe:ej2-methods-returning-promise`).
- B16: only the meta-package is exact (`Alis.Reactive.SandboxApp/package.json:25`); its sub-packages follow tilde
  ranges (`package-lock.json:1717-1726`), so `ej2-base` resolves to 33.1.45 under 33.1.47.
- A regular expression checks a vendor's name (`Alis.Reactive/Vendors/ComponentVendor.cs:10-11`).

- The core writes a call to a plugin only Fusion provides: `Alis.Reactive/PlanModel/Conditions/Condition.cs:504`,
  `Alis.Reactive/PlanModel/Writing/Scope.cs:209`, `Alis.Reactive.Assets/components/fusion/confirm.ts:94`.
- Today's confirm queues its asks (`Alis.Reactive.Assets/components/fusion/confirm.ts:88`), keeps Escape inside
  (`Alis.Reactive.Assets/components/fusion/confirm.ts:67`) and writes fixed English
  (`Alis.Reactive.Assets/components/fusion/confirm.ts:50`, `Alis.Reactive.Assets/components/fusion/confirm.ts:55`).

**Defects.**
- `X17.1` analyzer: a method declared with `Method` whose vendor contract returns a promise: new:N17.2.
- `X17.2` analyzer: a declared member the vendor contract does not list as public: new:N17.2.
- `X17.3` behavior: a CustomEvent with no detail reaches its handler as the declaration says, the same on every
  dispatch.
- `X17.4` behavior: a component an app brings, declared and rendered outside the framework's packages, runs every
  step kind (a read, a write, a call, an event) with no change to the framework.
- `X17.5` refusal: a vendor name outside its characters: "Vendor name '{name}' must start with a letter and contain
  only ASCII letters, digits, underscore, or hyphen."
- `X17.6` behavior: a page that loads no Fusion asset asks NativeConfirm in a `When`; the branch runs on OK and
  not on Cancel.
- `X17.7` behavior: Escape in a confirm opened over a drawer answers false, and the drawer stays open.
- `X17.8` behavior: a save replaced while its confirm is open closes that dialog, and only the second save's
  confirm shows.
- `X17.9` behavior: an error toast stays until it is dismissed, and an information toast leaves after its time
  unless it is hovered.
- `X17.10` behavior: a component found by a registered lookup, as `Chart.getChart(canvas)` finds a chart, is read,
  written, called and listened to as one found by a path.
- `X17.11` behavior: a listener added by a subscribe method that returns its unsubscriber is removed at release by
  that function, and the event runs nothing after.
- `X17.12` behavior: a listener of two arguments reads the first and calls the second, the vendor's continuation,
  with the page's items, and the component shows them.
- `X17.13` refusal: an awaited read inside a callback's answer is refused where the view renders with "A callback
  answers its vendor at once, so its answer cannot wait for an async method or a request."
- `X17.16` refusal (D17.3): a second handler of one object's callback property: "'{event}' is a callback property,
  which holds one listener, so a second handler would replace the first. Write both pipelines in one handler."
- `X17.17` behavior (D17.3): a call bell whose event is a callback property runs its handler with the ring's room,
  and a ward feed whose subscribe method returns its unsubscriber runs its handler with the update's message.
- `X17.18` refusal (D17.4): an argument at a position read on a declaration that is neither an event's payload nor a
  callback's arguments: "An argument at a position is what a listener or a callback receives: declare it on an
  event's payload or a callback's arguments."
- `X17.15` behavior (D4.8, D17.4): an event declared with a payload that its vendor raises with no argument stops
  before its first step, and the vendor goes on after its listeners; an event declared with none runs its handler.
- `X17.14` behavior: a vendor's label callback shows, for each label, the text the view's expression answers.
- `X17.19` refusal (D17.12): a step written inside a callback's answer: "A step is written inside a callback's answer,
  which answers its vendor with one value. Write the step in a pipeline."
- `X17.20` behavior (D17.12): a callback's answer reads the page as it is on each call, a text box's value and a
  method's result alike, and a method's argument written as an answer is the key the vendor sorts by.
- `X17.21` behavior (D17.12): an answer that stops while a step's write made the vendor ask stops that pipeline
  before its next step, and the vendor keeps what it drew before.
- `X17.22` behavior (D17.12): an answer that stops when the vendor asks in its own turn fails the vendor's call,
  and the vendor keeps what it drew before.
- `X17.23` behavior (D17.4, D17.12): a value accessor's answer reads the row its vendor passes at position 1, and the
  chart shows each row's label as its value.

## Q18. Type knowledge without reflection

**Decision.**
- D18.1 The framework knows a model from three sources and runs no reflection of its own:
  - expression trees, for the member a view, a validator or a request names, its declaring class (D14.3) and its
    static type;
  - generics, for the types a door is written over: a validator's `T`, `RuleForEach`'s `TElement`, a request's
    `TRequest`, a literal's `T`, a payload's `TPayload`;
  - the app's System.Text.Json contract, for what a type holds on the wire: its members, their JSON names under
    the app's options (D2.9), the members the options ignore, a collection's item type, and a type's starting
    values (D16.6).

  The contract is the app's options' `JsonTypeInfo`, from a source-generated context or from System.Text.Json's
  own resolver, so whatever reflection exists is System.Text.Json's, not the framework's.

  The reflection the framework refuses is run-time discovery and use of members (the owner, 2026-11-06: "a list of
  methods and properties and all kind of stuff. That is bad."): listing members, reading attributes, reading,
  writing or invoking a member through reflection, and `Activator`. A type's identity is not: its name, and the
  class that declares the member a typed lambda names (D14.3, D18.3), are within the rule. Each such read is the
  owner's, listed by file and member in `tests/Alis.Reactive.Poc.ArchitectureTests/WhenTheCoreIsCompiled.cs`,
  which fails on any other read of `System.Reflection`, of a member of `System.Type`, `Activator`, `Delegate` or
  `TypeDescriptor` (the owner, 2026-11-06: "Use caution really, really good so we don't trip over"). The namespace
  ban of D18.4 misses the members of `Type`, so `typeof(T).GetProperties()` builds; that list refuses it.
  The owner's exceptions, each in one file and on that list by name:
  - the class that declares a lambda's member, `MemberInfo.DeclaringType` (D18.3);
  - a value the view holds inside an array lambda, read along its chain of fields and properties with
    `FieldInfo.GetValue` and `PropertyInfo.GetValue` (D6.7; the owner, 2026-11-06: "capturing the model value is
    important");
  - the identity of a .NET member an array lambda calls, compared with the closed table of D6.5, never listed
    or invoked: `MemberInfo.Module` and `MemberInfo.MetadataToken`, which name a member's definition for every
    instantiation of a generic member, in `poc/Alis.Reactive.Poc/Conditions/TranslatedMembers.cs`.
- D18.2 One item-type finder: the contract's element type for a collection, and the key and element types for a
  dictionary. The three finders of today leave.
- D18.3 Each reflection site of today gets its verdict (B11):
  - a new model built by `Activator` and constructors: System.Text.Json's object read from `{}` (D16.6);
  - object and enum literals read member by member: the literal is written by System.Text.Json under the app's
    options, through the literal's `T`;
  - a markup payload read by `GetValue`: the payload is written by System.Text.Json through its `TPayload`;
  - a request member found by name, ignoring case: a request sends a form typed by the request's model, so the
    compiler matches the form's inputs to the request's members (N18.3), and a placeholder is filled by a member
    the request sets, named by its expression (D9.4);
  - members listed with `GetProperties` and `JsonIgnore` read: the contract's members;
  - `JsonPropertyName` read from a member: the contract's JSON name, which also follows the app's naming policy;
  - the assembly scan of validator registration: refused (Q16), `Add<TValidator>()` per validator;
  - the member scan that confirms a declaration's caller (`JsObject`): an analyzer checks at build time that a
    `Method`, `MethodAsync` or property declaration is called from the member it declares (N18.2);
  - the method scan for `PureMethodAttribute` and the `FlagsAttribute` read of `IsInEnum`: gate 1 decides each,
    as an owner-listed exception or a typed door;
  - `Expression.Compile` in validation (Q16) and in the template writer (Q15): gate 1 decides each.
  - an overridden member that a view's expression names (the owner, 2026-11-06: "Override cannot change it"). The
    expression carries the declaration (`d => d.Room` names `Person.Room`), and the derived class's contract holds
    the override. The member is named as the contract of the class that declares it names it, found up the member's
    declaring class (`MemberInfo.DeclaringType`, D18.1), and the class the read starts from must write that name,
    or the read is refused as unwritten. No `MemberInfo.Name` is read. The owner chose the declaring class over a
    walk up `Type.BaseType`; a probe of eight member kinds on both resolvers found the same contract by both.
  - System.Text.Json's contracts for an override (probe, net10, the reflection resolver): an override that keeps
    its declaration's name is one member of the derived contract; one that renames is written twice, under both
    names; one marked `JsonIgnore` leaves the name unwritten; a second member that takes the declaration's name is
    refused by System.Text.Json itself. A renamed override, and one that writes a member its declaration ignores,
    read correctly by the declaration's name but break the owner's rule, so the core's analyzer refuses both in the
    app's model at build time (N18.5). A model is a `.cs` file the build compiles on both hosts, so the analyzer
    sees it on net48 too.
- D18.4 An analyzer fails the build on any reflection call in a shipped package outside the exceptions the owner
  lists at gate 1 (N18.1): the members of `System.Reflection` that list, read, write or invoke members or read
  attributes, `Activator`, `Type.GetType` by name and `Expression.Compile`, except the owner's exceptions D18.1
  lists. A type's identity stays open to the framework (D18.1). The POC bans the whole `System.Reflection`
  namespace; each owner-listed read of it sits in a method that carries `SuppressMessage` for RS0030, and the list
  of D18.1 holds every read.
- D18.5 Every door on the public surface is typed (ALIS009). The three strings of B13 become typed doors:
  - a button's type is HTML's closed set, `button`, `submit` or `reset`;
  - a text box's type is HTML's closed set of text input types;
  - an action link's attribute is a typed name, with `data-` and `aria-` attributes by their own doors.

  Their forms go to gate 2.

**Schema nodes.** None: type knowledge lives in C#, and the plan carries only what it decides: JSON names,
shapes, starts and the payload kind of each event.

**Checker.**
- `probe:core-stj-empty-object-start`
- `probe:core-json-result-names`
- `probe:core-frombody-binds-json-name`
- `probe:core-frombody-ignores-clr-name`
- `probe:core-form-binds-clr-name`

**Doors.**
- kept: `LiveObject<T>()` and `LiveObjects<T>()` (renamed by N5.5), `Method`, `MethodAsync` and the property
  declarations of `JsObject`; every door written over a generic type.
- `N18.1` new: the reflection analyzer of D18.4, with the owner's exception list.
- `N18.2` new: the declaration analyzer of D18.3, which replaces `JsObject`'s run-time member scan.
- `N18.3` new: a form typed by its model, and a request's `Form` that takes only a form of the request's model.
- `N18.4` new: the typed doors of D18.5 for the button type, the text box type and the action link's attributes.
- `N18.5` new: the override analyzer of D18.3: an override in an app's model carries the JSON name and the
  `JsonIgnore` of the member it overrides. Built in POC 13, the contract analyzer.

**Evidence.**
- Three item-type finders (`Alis.Reactive/Inputs/FieldReference.cs:50-58`, `Alis.Reactive/Members/ClrShape.cs:82-90`,
  `Alis.Reactive/PlanAuthoring/Requests/RequestMembers.cs:66-72`).
- B11, read at HEAD:
  - `Alis.Reactive/PlanModel/Validation/ModelDefaults.cs:76-154`;
  - `Alis.Reactive/PlanModel/Values/ValueExpression.cs:76-104`;
  - `Alis.Reactive/Razor/MarkupRaise.cs:27`;
  - `Alis.Reactive/PlanAuthoring/Requests/RequestMembers.cs:34`;
  - `Alis.Reactive/Members/ClrShape.cs:40-43`;
  - `Alis.Reactive/Members/JsonMemberName.cs:11`, which also hard-codes camel case;
  - `Alis.Reactive/PlanAuthoring/Arrays/ElementExpressionCompiler.cs:51-55`;
  - `Alis.Reactive/Validation/ReactiveFluentValidationServiceCollectionExtensions.cs:66`;
  - `Alis.Reactive/Components/JsObject.cs:366-369`;
  - `Alis.Reactive/Validation/Checks.cs:420`.
- B13: `Alis.Reactive.Native/PublicAPI/net48/PublicAPI.Unshipped.txt:20`, `:52` and `:176` take a string for an
  attribute name and two input types, with no exemption.
- G1: one switch per union, ending in `assertNever` (`Alis.Reactive.Assets/runtime/grammar/evaluate.ts:25-37`), so a
  node C# adds fails typecheck; the generated contract is the TypeScript side of the same type knowledge.

**Defects.**
- `X18.1` analyzer: a `GetProperties` call in a shipped package outside the owner's list: new:N18.1.
- `X18.2` analyzer: `Method()` called from a member other than the one it declares: new:N18.2.
- `X18.3` type: a request that sends a form of another model does not compile: new:N18.3.
- `X18.4` type: a button's type written as text does not compile: new:N18.4.
- `X18.5` behavior: with the app's JSON options naming members in snake case, a request's JSON body sends
  `given_name`, and the endpoint binds it.
- `X18.6` behavior: a member the app's JSON options ignore is not in a JSON body, and the form's check reads its
  start.
- `X18.7` behavior: a read of a member an override implements reads it by its declaration's JSON name
  (`WhenARosterIsHandedOver`, POC 3).
- `X18.8` refusal: a read of an override the app's JSON options ignore is refused with "'{lambda}' reads a member
  of '{type}' that the app's JSON options do not write, so the browser never holds it." (`WhenARosterReadIsRefused`).
- `X18.9` analyzer: an override that renames, ignores or writes what its declaration does not: new:N18.5.

## Q19. The schema

**Decision.**
- D19.1 The schema is `spec/Core/schema.md`: every node, what it carries, the nodes it holds and its meaning in the
  browser, derived from Q1 to Q18, Q20 and Q21. C# writes those nodes and nothing else, and the browser runs those nodes
  and nothing else, so the runtime and the builders proceed in parallel from that file. The builders and the JSON they
  write are one track: one C# model writes the grammar, with no lowering pass (the owner, 2026-11-06, and the answer in
  `spec/NOTES.md`).
- D19.2 The set is closed at 49 kinds: today's 33, and `stop`, `listen`, `data`, `start`, `nothing`, `unbound`,
  `signal`, `answer`, `file`, `each`, `returned`, `stream`, `clock`, `zone`, `off` and `on`. No kind leaves. After gate 1 a unit never changes the set: a behavior the nodes cannot express stops the unit and goes
  to the owner with its use case.
- D19.3 Every field is required, and none holds null as a state; a value that can be absent is a closed union with a kind per case. Null
  is only a value data holds: a `lit` of null and the JSON of a start (the owner, 2026-11-06).
- D19.4 C# declares no enum (schema.md S8). A closed choice with no data is a sealed class with a private constructor
  and one static instance per case, written as its text; a choice whose cases carry data is a kind. `Operator`,
  `PathMembers` and the public `VendorEventPayload` become such classes.
- D19.5 Every kind has exactly one handler. The switch that picks a default by shape leaves, since C# writes each
  input's blank (D3.6, N3.3), and the object locator's two switches become one switch that returns the locator's
  driver (G1).
- D19.6 A handler carries its payload kind (D17.4), and the vendor does not: the vendors map is shared by name across
  plans, so two plans that listen to different events of one component, read differently, agree on the vendor.
- D19.7 A form's data is a list of registrations, each one validator under one rule set, holding the model's start,
  the bound scalars, objects and lists of D16.6 and D16.7, and a rule tree whose groups follow the validator's child
  validators (D16.3). The brief's item raised after the fact is decided: `FormField`'s `items` and `value` functions
  leave for paths with `each`, and `live` stays, as `Live`, naming the check it runs.
- D19.8 A number shape carries what a text converts under: `of`, an integer or a fraction, which picks the culture's
  number text for the host, and `text`, the view's culture or the invariant one (D3.11).
- D19.9 `HttpSend`, `HttpResponse` and `FormCheck` are typed contracts generated beside the grammar, not kinds (G2).
- D19.10 The plan's version is 7. A plan of another version is JSON the framework did not produce, refused at boot with
  one error.

**Schema nodes.** Every node of `spec/Core/schema.md`.

**Checker.**
- `Schema.tla:KindsHandledOnce`
- `Schema.tla:OperatorsClosed`
- `Schema.tla:ReadsTakeNoEach`
- `Schema.tla:ConversionsConvert`
- `Schema.tla:PatternsAreLiterals`
- `Schema.tla:NullableWrapsOnce`
- `Schema.tla:SetsWriteAMember`
- `Schema.tla:RefsReadTheirBlock`
- `check:kinds`, which reads the grammar and `plan.schema.json` at 65fdf118d, schema.md's kinds table and Schema.tla's
  node set, and checks each today line holds its case.

Each Schema.tla invariant is the browser's need, and each mutant drops the one refusal of the C# writer that serves it.

**Doors.**
- `N19.1` new: the sealed choice classes of D19.4 in `Alis.Reactive/Grammar`, replacing `Operator`, `PathMembers` and
  `VendorEventPayload`; the public payload class has the cases `Argument`, `Event` and `Detail` of D17.4.
- `N19.2` new: `Registration`, `BoundScalar`, `BoundObject`, `BoundList`, `RuleGroup`, `RuleField` and `RuleTest` in
  `Alis.Reactive/Grammar/PlanDocument.cs`, replacing `FormField` and `FormTest`; they carry N16.2.
- `N19.3` new: `Culture`, `NumberText`, the time zone kinds and the trace kinds, as `PlanDocument`'s fields.

**Evidence.**
- Today's kinds are written once per node class: `Alis.Reactive/Grammar/Statements.cs:24`,
  `Alis.Reactive/Grammar/Expressions.cs:20`, `Alis.Reactive/Grammar/Shape.cs:93`, `Alis.Reactive/Grammar/PlanDocument.cs:67`.
- A shape has a second handler that picks a default by shape: `Alis.Reactive.Assets/runtime/grammar/shape.ts:31-45`.
- An object locator has two switches: `Alis.Reactive.Assets/runtime/objects/plan-objects.ts:14-18` and
  `Alis.Reactive.Assets/runtime/objects/plan-objects.ts:27-31`.
- A vendor is merged across plans by its parts, its payload among them: `Alis.Reactive/Grammar/PlanDocument.cs:43`.
- The payload kind is a public enum: `Alis.Reactive/Grammar/VendorEventPayload.cs:8`.
- A slot's plugin data is `none`, its key the container id: `Alis.Reactive/PlanModel/Writing/Scope.cs:234`.
- The form's fields raised after the fact: `Alis.Reactive/Grammar/PlanDocument.cs:202-206` and
  `Alis.Reactive/Grammar/PlanDocument.cs:184-187`.

**Defects.**
- `X19.1` type: an operator, a path's members or a payload kind made from an integer does not compile: new:N19.1.
- `X19.2` type: a grammar node built with a field left out does not compile, since each constructor takes every field
  and none is nullable: new:N19.3.
- `X19.3` refusal: a plan of version 6 is refused at boot with "This plan was written for plan version 6; this page
  runs version 7."
- `X19.4` behavior: two partials whose plans listen to different events of one component, one reading its argument
  and one its event's detail, each run their steps with the payload they declare.

## Q20. Tracing

**Decision.**
- D20.1 Tracing is off unless the server recorded the render. C# writes the plan's `trace` as `on` only when the
  activity of the request that renders the view is recorded, W3C Trace Context's sampled flag, and as `off` otherwise.
  The browser decides nothing but D20.11's: no attribute, no query text, no storage, so the level read from
  `data-alis-trace` leaves. In development the app's sampler records every request; in production the app sets its own ratio (the
  owner, 2026-11-06: optional, never always on).
- D20.2 The core takes no telemetry library. The browser has one diagnostics port, called at the dispatch of each
  union, which Q19 makes one place per kind, and two sinks:
  - the console, which prints every boundary error, and each ended span when the trace is on and its `console` is set,
    as C# writes it from the app's options for development;
  - the OpenTelemetry adapter, a separate package built on the port, which pins `@opentelemetry/api`, the web SDK and
    the OTLP HTTP exporter, and which the app configures. The core names it nowhere, as it names no consumer.
- D20.3 One trace runs from start to end by W3C Trace Context. A handler run's span is a child of the plan's
  `traceparent`. Each send attempt sends `traceparent` and `tracestate` naming its own span to the page's own origin,
  and to no other origin. The endpoint's activities, the validation door's among them, join that trace. A slot's
  content is rendered by a traced request, so its plan's trace context is the server's span, and nested partials nest
  in the trace. The trace context is the run's, as its abort signal is (Q11), and `HttpSend` carries none.
- D20.4 The spans are closed: a plan's boot, a handler run, a send and each of its attempts, a `MethodAsync`, a
  `Confirm`, a hub invoke, each block of an `all`, and a slot's load and unload. A synchronous statement is no span: a
  `stop` with its reason, a required member a holder lacks, a refused check and a boundary error are events on the span
  that holds them. An attempt follows OpenTelemetry's HTTP client conventions: the method, the status, the resend count
  and the URL as its route template.
- D20.5 No value is recorded. A span records ids, kinds, member wire names, event names, the route template, the status,
  the attempt and the outcome, never an input's text, a body, a payload, a query value or a route value, so the app's
  data stays out of its telemetry.
- D20.6 Every span ends exactly once and after its children, however its run ends: completed, stopped, replaced (D11.7)
  or disposed (D12.4). A sink's error is the one error the port catches: it logs it once as an error and the run goes
  on, since telemetry observes the page and never decides what it does.
- D20.7 Under `on`, C# writes each handler's source: the view, as MVC names it, and the line of the builder call, read by
  `CallerFilePath` and `CallerLineNumber` at the builder door, which read the line a `#line` directive sets
  (`probe:core-caller-line-follows-line-directive`). That each host's view compiler writes that directive at a builder
  call is not yet observed, and the unit's first proof is that probe on both hosts. If a host writes none, `line`
  carries the handler's position among the handlers its view writes: the same integer, so the grammar does not change.
  Under `off` no source is written, so a page carries no view path unless its render was traced.
- D20.8 On the server the framework has one `ActivitySource`, named "Alis.Reactive", from `System.Diagnostics`, with an
  activity for rendering a plan, the validation door (validator, rule set, failure count) and the error door. The app
  adds its own OpenTelemetry SDK and listens to that source. On net48 the source comes from the
  `System.Diagnostics.DiagnosticSource` package, pinned, a gate-1 line, and joining an inbound `traceparent` takes the
  app's ASP.NET instrumentation module, a fact the unit probes.
- D20.9 Tracing changes nothing a plan does: on and off run the same statements in the same order with the same
  results, and the port returns nothing the runtime reads.
- D20.11 A retry is always traced (the owner, 2026-11-06: downstream services act on it, by standard headers). Every
  attempt after a send's first sends `traceparent` with the sampled flag set, so the server's parent-based sampler
  records the endpoint and what it calls, and W3C `baggage` with `http.request.resend_count`, OpenTelemetry's name
  for the attempt count, so the server copies it into its span. With the trace on, the context is the attempt's own
  span (D20.3); with it off, the send mints one trace id for its retries, and the first attempt carries nothing, since
  no one knows it will fail. Both headers go to the page's own origin only; the services behind the endpoint receive
  them from the server's own propagation.
- D20.10 Its proof is Playwright on the sandbox, whose endpoint echoes the `traceparent` it received into the page and
  whose collector page lists the spans it received: with the trace off nothing is listed and no header is echoed; with
  it on, the closed span names of a handler with a check, a retried GET and a chained request are listed under the
  page's trace id.

**Schema nodes.** `PlanDocument.trace`, the kinds `off` and `on`, and `Source` (Q19).

**Checker.**
- `Tracing.tla:NothingRecordedWhenOff`
- `Tracing.tla:EveryStartedSpanEnds`
- `Tracing.tla:ChildEndsBeforeParent`
- `Tracing.tla:AttemptsJoinTheTrace`
- `Tracing.tla:TraceHeadersStayHome`
- `Tracing.tla:SinkFailureChangesNothing`
- `Tracing.tla:RetriesAlwaysTraced`
- `Tracing.tla:ResendCountSent`
- `probe:core-caller-line-follows-line-directive`

**Doors.**
- `N20.1` new: the browser's diagnostics port, internal, with its closed span and event vocabulary (D20.4).
- `N20.2` new: the OpenTelemetry adapter package built on the port; its name and its configuration go to gate 2.
- `N20.3` new: the `ActivitySource` "Alis.Reactive" and the app's tracing options (`console`) on both hosts; their form
  goes to gate 2.

**Evidence.**
- Today the browser reads its log level from the page, undefined meaning warn:
  `Alis.Reactive.Assets/runtime/diagnostics/trace.ts:13-18`.
- Today a log line prints the values it is given: `Alis.Reactive.Assets/runtime/diagnostics/trace.ts:46`.
- Neither line traces: `git grep` finds no `traceparent`, OpenTelemetry or `ActivitySource` at 65fdf118d or 623dd4a27.

**Defects.**
- `X20.1` behavior: with the trace off, a save lists no span on the collector page, and its endpoint echoes no
  `traceparent`.
- `X20.2` behavior: with the trace on, a GET retried once after a 503 lists two attempt spans under one send span,
  under the page's trace id.
- `X20.3` behavior: a handler replaced while its request is in flight lists its run span ended with the outcome replaced.
- `X20.4` behavior: with the trace on, a request to another origin carries no `traceparent`, which that endpoint echoes.
- `X20.5` behavior: with an exporter that throws on every span, a save succeeds and the console shows one error.
- `X20.7` behavior: with the trace off, a GET retried after a 503 reaches the endpoint the second time with a sampled
  `traceparent` and `baggage` holding `http.request.resend_count=1`, which the endpoint echoes, and the first time with
  neither.
- `X20.6` refusal: a request header named `traceparent` is refused where the view renders with "The traceparent header
  belongs to tracing: remove it from this request's headers."

## Q21. Realtime: server-sent events and SignalR

**Decision.**
- D21.1 Two transports, each by its own standard, each its own module of the browser's code (the owner, 2026-11-06:
  both are mandatory, and SignalR is a known dependency, kept modular):
  - server-sent events, by the HTML standard's `EventSource` over `text/event-stream`, with no library. A message is
    named by its `event:` field and its `data:` is JSON. The browser reconnects by itself after the server's `retry:`,
    sending `Last-Event-ID` with the last `id:` it saw.
  - SignalR, by ASP.NET Core SignalR's protocol through `@microsoft/signalr`, pinned, the one realtime dependency. Its
    module is its own asset, which the view links only when a plan names a hub, so a page with no hub ships no
    SignalR client.
- D21.2 Realtime is net10's alone (the owner, 2026-11-06: "Let's drop real time from .NET 4.8"). Its doors are on
  net10's public surface only, so a net48 view that writes a stream, a hub or a message trigger does not compile.
  - Server-sent events come from the app's own endpoint through ASP.NET Core 10's `TypedResults.ServerSentEvents`.
  - A hub is an ASP.NET Core SignalR hub; `@microsoft/signalr` speaks only its protocol, which is compatible with no
    client or server of ASP.NET SignalR (learn.microsoft.com/aspnet/core/signalr/version-differences).
  - The cut is the owner's, and it holds by what System.Web is: an open stream holds its request for its whole life,
    IIS buffering and compression break `text/event-stream`, and a hub would need an ASP.NET Core app beside the MVC 5
    app. A net48 page has no server push in 1.0.
- D21.3 A server event is declared once, its name and its message type, and the endpoint that writes it and the view
  that listens to it share that declaration, as an app event's dispatcher and listener do (G9). A hub's methods are
  declared once the same way, for the calls a view makes and the pushes it receives.
- D21.4 A message is data (D5.7): a member it lacks reads nothing.
  - A server-sent event whose data is not JSON, and a hub push that holds more than one argument, or one that is not
    an object, is a message the framework did not produce. It is logged once as an error, no handler runs, and the
    next message runs.
  - Each message starts a run under its handler's policy (D11.7), in the order the messages arrive.
- D21.5 A connection's lifecycle is one closed set of triggers on both transports: `reconnecting`, `reconnected` and
  `closed`.
  - For server-sent events, the browser's own reconnection is `reconnecting`, the next open is `reconnected`, and a
    source the browser gave up on is `closed`. For SignalR they are the client's own events.
  - After `closed`, the retry offer shows until a retry connects or the last holder releases (D12.6).
  - The framework replays no message the page missed. An endpoint replays from the `Last-Event-ID` it receives, and a
    page that must not miss a hub push reloads its state on `reconnected`.
- D21.6 A stream is held as a hub is, by every owner whose plans name it (D12.6), so a stream's plugin data is
  `stream` as a hub's is `hub`. Today a stream is held only through its subscriptions, so a plan that only reads its
  state reads it closed.
- D21.7 Authentication is the page's own: a stream and a hub send the page's cookies, and the server authorizes each
  connection and each hub method. A bearer token is not sent: `EventSource` sends no header, and SignalR puts a
  token in the URL for WebSockets and server-sent events. A token door is a gate-1 line.
- D21.8 Tracing (Q20): a run a message starts is a child of the plan's trace context and records its transport and
  event. The server span that sent the message is not linked, since neither wire carries a trace context per
  message. Whether a hub invoke joins the server's hub activity depends on `@microsoft/signalr` carrying trace
  context, a fact the unit probes before it claims it.
- D21.9 Proof:
  - Playwright on net10 covers both transports, every lifecycle trigger and the holds.
  - The verify-net48 run covers that a net48 view writing a realtime door does not compile.

**Schema nodes.** The plugins `alis.stream` and `alis.hub`, their plugin data `stream` (new) and `hub`, `handler` for
message and lifecycle triggers, and `call` for a hub's invoke and send.

**Checker.**
- `Ownership.tla:HubKeptWhileHeld`
- `Ownership.tla:OfferOnlyWhileHeld`
- `Lanes.tla:SkippedTriggerStartsNothing`
- `Lanes.tla:ReplacedRunRunsNoRoute`

**Doors.**
- kept: N5.1's message triggers typed `Data<TMessage>`, and the hub's `invoke` and `send` (Q11).
- `N21.1` new: a server event and a hub's methods declared once and shared by the endpoint and the view (D21.3); their
  form goes to gate 2.
- `N21.2` new: the endpoint's typed event writer over `TypedResults.ServerSentEvents`, net10 only.
- `N21.6` new: every realtime door under net10's PublicAPI text only, behind the host split, none on net48's.
- `N21.3` new: the plugin data `stream` (D21.6).
- `N21.4` new: the lifecycle triggers `reconnecting`, `reconnected` and `closed` on a stream and a hub (D21.5).
- `N21.5` new: the hub's module as its own asset, linked when a plan names a hub (D21.1).

**Evidence.**
- The core's browser code imports the SignalR client: `Alis.Reactive.Assets/runtime/realtime/hub.ts:1`, pinned at
  `Alis.Reactive.Assets/package.json:23`; it connects with the page's cookies and reconnects by itself
  (`Alis.Reactive.Assets/runtime/realtime/hub.ts:54-55`).
- A hub's lifecycle triggers exist and a stream's do not: `Alis.Reactive.Assets/runtime/realtime/hub.ts:116`.
- A hub push of another shape is logged and dropped: `Alis.Reactive.Assets/runtime/realtime/hub.ts:158`; so is a stream
  message that is not JSON: `Alis.Reactive.Assets/runtime/realtime/stream.ts:87`.
- A stream uses the browser's `EventSource`: `Alis.Reactive.Assets/runtime/realtime/stream.ts:23`; it is held through
  its subscriptions only (`Alis.Reactive.Assets/runtime/realtime/stream.ts:95`), and reads closed when none holds it
  (`Alis.Reactive.Assets/runtime/realtime/stream.ts:75`).
- Realtime is proven on net10 alone: `Alis.Reactive.SandboxApp/Alis.Reactive.SandboxApp.csproj:4`,
  `tests/Alis.Reactive.Framework.App/Alis.Reactive.Framework.App.csproj:4`.

**Defects.**
- `X21.1` behavior: a stream message whose data is not JSON runs no handler and logs one error, and the next message
  runs its handler.
- `X21.2` behavior: a slot that only reads a stream's state reads it open while the slot is loaded.
- `X21.3` behavior: when the server restarts, a page runs its `reconnecting` steps, then its `reconnected` steps, and the
  next message runs its handler.
- `X21.4` behavior: a hub push with two arguments runs no handler and logs one error.
- `X21.5` behavior: a page whose plans name no hub requests no SignalR asset.
- `X21.6` type: an endpoint and a view that name one server event differently, or give it two message types, do not
  compile: new:N21.1.
- `X21.7` type: a net48 view that writes a stream, a hub or a message trigger does not compile: new:N21.6.
