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
- Nothing is a read with no value: a value JavaScript holds as `undefined` (D4.9), a member past a holder that holds
  null or reads nothing (D4.2), and a conversion that fails (D3.3). Null is a value the data holds, apart from nothing:
  a comparison reads it as .NET's lifted operators do (D8.9), and a write of nothing writes null (D4.9).
- To stop a pipeline is to run none of its later statements, log one error naming the handler and the
  reason, and run, innermost first, each `Finally` layer whose `WhileLoading` completed of every request the stop
  happens inside (D10.23).
- A door is a line of a PublicAPI text. A door marked `N<q>.<k>` is a declaration this design proposes.

## Q1. Paths

**Decision.**
- D1.1 A path is a list of steps, and a step is exactly one of three: `member(name)`, `index(n)` with `n` a
  non-negative integer, or `each`.
- D1.2 In plan JSON a member is a JSON string, an index is a non-negative JSON integer, and each is the object
  `{"kind":"each"}`. A string is a member name whatever its text: `"[]"`, `"[*]"`, `"*"` and `"0"` are member
  names, never markers.
- D1.3 Every path the plan carries uses this step: `get`, `call` and `set` paths, a handler's path, the path a vendor's
  `locate` reads from its element (D17.3), an input binding's body path, a registration field's path, and the member
  paths of a request.
- D1.4 A dictionary entry is a member step whose name is the key's text as System.Text.Json writes the key
  as a property name. A dictionary whose key type System.Text.Json writes no property name for is refused
  where the view renders.
- D1.5 `each` appears only in a registration's paths (Q16, Q19). No read, write, call or subscription carries
  `each`; a projection over a list is `map` (Q6).
- D1.9 On a read, a write or a call, an index step and a member step of the same digits reach the same item:
  JavaScript reads `a[1]` and `a["1"]` alike. POC 4 planted the position of `LiveObjects<T>.At(1)` as the member
  `"1"`, and the test that writes through it passed. The index step has its meaning where a body is built: an
  input's body path writes the body a request sends, and a position there makes an array where a member makes an
  object (`Alis.Reactive.Assets/runtime/plugins/model.ts:156-162`, `assignAt`). So the grammar keeps the index step:
  an input's body path is a list of path steps (`spec/Core/schema.md` S5, `InputBinding`'s `path`), and a list item's
  inputs reach a JSON body at their position (X14.10).
- D1.6 The MVC binding name is one function of a CLR member path, `BindingName`: member names joined by
  `.`, and an index written `[n]` directly after the step before it, as MVC's own
  `CreateSubPropertyName` and `CreateSubIndexName` write them. It is computed only in C#
  (`probe:mvc5-sub-property-name`, `probe:mvc5-sub-index-name`).
- D1.7 Binding name text appears only where the server speaks it: an input's `name` attribute, form-data and
  query keys, and the error keys a server's validation returns. The browser compares a server key with a
  binding name as whole text and never parses a name.
- D1.8 The member names in a path are the wire names of Q2 for the host the view renders on.

**Schema nodes.** `PathStep` (a string, an integer, or `each`) in `get`, `call`, `set`, `Handler.path`,
`Vendor.locate`, `InputBinding.path`, `RuleField.path` and the request argument's member paths (Q9).

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
- `X1.1` type (D1.1, D1.3): a step that is not a member, an index or each cannot be constructed: new:N1.1.
- `X1.2` type (D1.2): a member whose name looks like a marker is still a member: new:N1.1 writes members as strings only.
- `X1.3` type (D1.5): writing, calling or subscribing to a projection over items does not compile; the projection is a
  `ReadOnly<IReadOnlyList<TField>>`, which has no `Set` (`Alis.Reactive/Components/JsMembers.cs:140`).
- `X1.4` refusal (D1.4): a dictionary keyed by a type System.Text.Json cannot write as a property name: "A dictionary
  keyed by {type} cannot be sent: System.Text.Json writes no property name for its keys; key it by text or a number."
- `X1.5` behavior (D1.6, D1.7): a server error key `Members[1].Name` shows its message in the request's summary, and at the
  input the request sent under that binding name.

## Q2. One read

**Decision.**
- D2.1 Every value the plan reads is `get(target, path, members)` or `call(target, path, args, members)`, with
  typed paths (Q1). `members` is `required` or `optional`, and C# decides it from the holder (Q5).
- D2.2 No plugin takes a member path as text. The model plugin's `valueOf(pattern)` and `sentValueOf(sent,
  pattern)` leave; a request's check reads the body it sends with `get` over a typed path (Q16).
- D2.3 A member's four names come from one derivation, `MemberPath`, built from the typed lambda, the
  declaring view's `HtmlFieldPrefix` and the member steps: the binding name (`BindingName`, D1.6), the body
  path (wire names, D2.4), the JSON name of a data read (D2.5), and the element id (Q14).
- D2.4 A request body names each member as the endpoint's binder reads it, one rule per host:
  - net10: the name System.Text.Json's contract gives the member under the endpoint's MVC `JsonOptions`
    (`JsonPropertyName`, else the naming policy), read from `JsonSerializerOptions.GetTypeInfo`
    (`probe:core-frombody-binds-json-name`).
  - net48: the CLR name, which MVC 5's `JsonValueProviderFactory` and `DefaultModelBinder` match ignoring case
    (`probe:mvc5-json-value-provider-case`).
- D2.5 Form-data and query keys are binding names (CLR names) on both hosts, since MVC binds form and query
  values by property name and ignores `JsonPropertyName` (`probe:core-query-object-json-name`). A member net10
  binds under another name (`[FromQuery(Name)]`, `[BindProperty(Name)]`, `[ModelBinder(Name)]`) binds nothing
  under its CLR name, and a `[BindNever]` member binds under none (`probe:core-query-member-names`); MVC 5 renames
  no member (`probe:mvc5-member-key-clr-name`). So a key is the member's C# path on both hosts, and a request that
  sends a member bound under another name is refused where the view renders (D9.20).
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
  JsonPropertyName, else camel case", which holds for a net10 JSON body only. Gate 1 approved the change
  (2026-11-09), and that root line described main and the redesign; the rewrite's rule file for views,
  `.claude/rules/views.md`, restates D2.4 to D2.6 and D2.9 in its place. A snake-case app's net10 body sends its members by that policy (X18.5).

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
- `X2.1` type (D2.1): reading a member a model or declaration does not have does not compile; every read starts from a
  typed lambda (`Alis.Reactive/Components/JsMembers.cs:37`).
- `X2.2` type (D2.2): a plugin cannot be handed a member path as text: new:N2.1 gives the model plugin no text door.
- `X2.3` behavior (D1.8, D2.4): a request member declared `[JsonPropertyName("given")]` reaches a net10 `[FromBody]` endpoint
  bound.
- `X2.4` behavior: an input a partial renders under the prefix `Kin` has an id, a name, a body path and an error
  key that agree, so the server's message for `Kin.Name` shows at that input.
- `X2.5` analyzer: a member lambda that names the model itself: "m => m names the model, not a member of it; name
  the member the input binds, as m => m.Name." At build by ALIS033, and where the view renders on code it does not follow (D18.6): new:N18.10.

## Q3. Shapes and conversion

**Decision.**
- D3.1 A shape is one of `string`, `number`, `boolean`, `date`, `instant`, `file`, `any`, `array(item)`,
  `nullable(inner)`, `object(fields)`. `file` is new (Q9).
- D3.2 There is one conversion node, `as(expr, shape)`. The operators `toNumber`, `toDate`, `toBoolean` and
  `toText` leave the grammar, and a validation rule converts with `as` like every read. A comparison or a sort of a
  date or a time reads each operand through its key (D6.12), an operator C# writes only as an argument of a
  comparison or a sort's comparison, so a key is no conversion: no value position takes it (`Schema.tla:KeysOnlyCompared`).
- D3.3 A conversion of a present value the shape cannot hold yields nothing. No conversion passes the
  unconverted value on (`Absence.tla:FailedConversionReadsNothing`).
- D3.4 The conversions are the table of `spec/Core/conversions.md`, one row per source kind and shape; the
  browser implements exactly that table.
- D3.5 Blank is nothing, or text that is empty or white space as .NET's `string.IsNullOrWhiteSpace` defines it.
- D3.6 What a blank input binds as is a literal C# writes into the input's binding: the member's value in a
  new instance of the item or model that holds it, encoded as the endpoint encodes it (Q18). The browser
  chooses no default by shape.
- D3.7 A number goes into JSON as a JSON number. Into form data it goes as its text with the decimal
  separator of the culture the view renders under, which C# writes into the request argument, because both
  hosts bind form values under the request's culture (`probe:core-form-decimal-culture`,
  `probe:mvc5-form-current-culture`). Into the query and the route it goes as its invariant text, because both hosts
  bind those under the invariant culture (`probe:core-query-decimal-invariant`, `probe:mvc5-query-invariant-culture`).
- D3.8 A date and an instant are shown and sent as `spec/PathsAndWire/DateWire.tla` states (`yyyy-MM-dd`, the time
  when not midnight, an instant's clock offset; in form data a date or a day with its time, D3.9). A date or an
  instant that reads nothing writes no text: a request
  sends it by D9.10's table, so a `DateTimeOffset` is left out and binds its start, and a `DateTimeOffset?` binds
  `null` (`DateWire.tla:NothingWritesNoText`, `DateWire.tla:InstantCarriesItsClockOffset`).
- D3.9 Every date text is ISO 8601-1:2019's extended format, and an instant's is the RFC 3339 profile of it
  (the owner, 2026-11-06: dates to the standards).
  - A `DateTime` is a local date and time with no offset. Both hosts bind that text as an Unspecified
    `DateTime`, the same wall time on any server clock (`probe:core-form-date-kind`, `probe:mvc5-datetime-parse`).
  - A `DateTimeOffset` is an RFC 3339 date-time with its offset, `Z` at zero.
  - In form data a `DateTime` and a `DateOnly` are sent with their time, `2026-10-27T00:00:00` at midnight, never as the
    day alone (gate 2's G0), since both hosts bind form values under the request's culture (D3.7). The day alone binds
    in that culture's calendar, th-TH's 2026-10-27 as 1483-10-27, and a date with its time binds Gregorian under every
    culture net10 knows (`probe:core-form-day-calendar`, `probe:core-form-date-forms-every-culture`). On net48 MVC 5
    binds it by `DateTime.Parse` under the culture (`probe:mvc5-datetime-parse`), which reads a date followed by `T` as
    ISO 8601 (`probe:net48-parse-t-is-iso8601`) in the Gregorian calendar (`probe:net48-iso8601-gregorian`).
  - A `DateTime` is never sent with an offset: System.Text.Json reads an offset into a `DateTime` as the
    server's local time (`probe:core-stj-offset-into-datetime`), which moves the wall time by the server's
    clock.
  - A `DateTime` is its wall fields whatever its `Kind` (gate 1's P9): a `DateTime` C# writes into the plan (a
    literal, a member of an object literal, a request member a literal fills) is those fields with no offset, unless
    a converter of the app's or of the member writes it, so `DateTime.Now` is the server's wall time and
    `DateTime.UtcNow` UTC's, in every browser. System.Text.Json writes a
    Local `DateTime` with the server's offset and a Utc one with `Z` (`probe:core-stj-datetime-kind`), and each host
    reads an offset in a `DateTime`'s text its own way (`probe:core-bound-offset-into-datetime`), so the plan's writer
    adds, after the app's converters, one that writes a `DateTime` as
    Unspecified: it reaches every member, a `DateTime?` too, while a converter the app registers for `DateTime` comes
    first and a member's own `[JsonConverter]` keeps its form (D4.2) (`probe:core-stj-datetime-nested-kind`); the live
    form takes no converter of the app's, so there the plan's own writes every `DateTime` (D7.7). The
    framework decides by the type, never by a value's `Kind`: an instant is a `DateTimeOffset`. A `DateTime` text
    with an offset that data in the app's form holds is read as its wall fields, the offset dropped, since they are
    the `Ticks` System.Text.Json wrote (`probe:core-stj-date-time-forms`, `spec/Core/conversions.md`), where a moment
    shows another wall time (`probe:js-date-utc-text-as-moment`).
  - On net10 a `DateOnly` is the text `yyyy-MM-dd` and a `TimeOnly` the text `HH:mm:ss`, with seven places of a
    second's fraction when it has one, as System.Text.Json writes them, and net10 binds each text in a query
    (`probe:core-query-day-time`). Each holds no date (D3.13), so it reaches an element's text, a live member and the
    wire as that text, and a comparison reads either by its key (D6.12). net48 has neither type.
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
  - When the app makes no choice, an instant uses the browser's clock, the decision of 2026-11-05,
    confirmed at gate 1 (C23).
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
- D3.13 A value a step writes into a live object, a property's set and a method's argument, reaches it in the shape
  of the type its declaration gives, so a date reaches the vendor as a JavaScript `Date`, as its typed API holds one
  (D5.3).
  - C# writes `as(value, shape)` where that type holds a date: `DateTime` is `date` and `DateTimeOffset` is
    `instant`, each `nullable` when it is nullable, inside `object(fields)` that names only the members holding one,
    and `array(item)`. The same walk writes `number{of, text: invariant}` at each number a source in the app's form
    holds as text, so it reaches the live member as its number (D7.7). A type that holds no date and no such number is
    written as the plan writes it, with no `as`.
  - The shape comes from the app's contract (D18.1): a type test of the member's `JsonTypeInfo` against
    `JsonTypeInfo<DateTime>` and its kin, its members and its item type, with no read of a `System.Type` member.
  - Refused where the view renders: a dictionary whose values hold dates, since `object(fields)` names its members
    ahead, and a type that holds itself and a date, since its shape would not end.
  - A value whose dates are already `Date`s reaches the member as it is, so an object a live method returns, a class
    instance, keeps its identity and prototype; only a plain object or array that holds a date as text is copied
    with it converted (`spec/Core/conversions.md`).
  - Data a step sends to the server keeps the wire text (D3.8).
  - The shape is the declared type's: a type the app's options make polymorphic is shaped by the members of the type
    the declaration names, so a date only a derived type holds reaches the vendor as its text. A date a callback's
    answer returns reaches its vendor as text (D17.12), refused where the vendor's function returns a `Date` (X17.41).
  - The redesign wrote these shapes by reflection over the type (`Alis.Reactive/Components/ArgumentShape.cs`,
    `ClrShape.SerializedProperties`); the contract carries the same feature.

**Schema nodes.** `as`, the shapes of D3.1, `InputBinding.value`.

**Checker.**
- `Absence.tla:FailedConversionReadsNothing`
- `DateWire.tla:DateTimeCarriesNoOffset`
- `DateWire.tla:InstantCarriesItsClockOffset`
- `DateWire.tla:NothingWritesNoText`
- `DateWire.tla:DayIsItsDayAlone`
- `DateWire.tla:TimeIsItsTimeAlone`
- `DateWire.tla:DateInFormCarriesItsTime`
- `probe:core-stj-datetime-kind`
- `probe:core-stj-datetime-nested-kind`
- `probe:core-query-day-time`
- `probe:core-form-decimal-culture`
- `probe:core-query-decimal-invariant`
- `probe:mvc5-form-current-culture`
- `probe:mvc5-query-invariant-culture`
- `probe:core-form-date-kind`
- `probe:mvc5-datetime-parse`
- `probe:core-stj-offset-into-datetime`
- `probe:core-bound-offset-into-datetime`
- `probe:core-stj-date-time-forms`
- `probe:js-date-utc-text-as-moment`
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
- `X3.1` type (D3.2): a second conversion cannot be written: new:N3.1.
- `X3.2` type (D3.1): a file cannot be read as text or sent as JSON by accident: new:N3.2 gives it its own shape, and
  the request argument (Q9) accepts a `file` member only in form data.
- `X3.3` behavior (D3.3, D3.4): the text `abc` in an input bound to an `int` member that starts at 5 reads 5 wherever the input is
  a source, never `abc` or `NaN`, and its check fails the member with the binder's message before `GreaterThan(0)`
  runs (N3.3, D16.7).
- `X3.4` behavior (D3.5, D3.6): a blank input bound to an `int` member initialized to 5 sends 5 and validates as 5.
- `X3.5` behavior (D3.7): a decimal sent as form data from a view rendered under `de-DE` binds `1,5` on the endpoint.
- `X3.6` behavior (D19.8): under `de-DE`, `1,5` typed in a decimal input passes `GreaterThan(1)` in the browser, as the
  server passes it, whatever the browser's own locale.
- `X3.7` behavior: a date of birth a net48 endpoint answers through N10.6 shows its own day in a browser whose
  clock is behind the server's.
- `X3.8` behavior: a decimal set as an element's text under `de-DE` shows `1234,5`, as the server's view shows
  it.
- `X3.9` behavior (D3.8): a view rendered under the zone `America/New_York` shows a visit's `DateTimeOffset` at its New
  York wall time and offset in a browser whose clock is in another zone, and sends a picked time back with
  New York's offset for that day.
- `X3.10` behavior (D3.13): a visit planner receives as a `Date` a date a step passes, one inside an object, each
  date of a list, a date a step sets as its property and a date an event carries as text; it receives an instant as
  that moment, a nullable date's null as null, and the suggestion its own method returns, a class instance, as it is.
- `X3.13` behavior (D3.9): a view that sets an element's text from `DateTime.UtcNow` and `DateTime.Now`, rendered
  at a known moment, shows each one's wall time with no offset in a browser whose clock is in another zone, and a
  request member a Utc `DateTime` literal fills binds that wall time as an Unspecified `DateTime`.
- `X3.14` behavior (D3.9): on net10 a request member of type `DateOnly` a literal fills sends `2026-11-09` and one of
  type `TimeOnly` sends `13:30:00`, and the endpoint binds both; an element's text set from them shows the same text.
- `X3.11` analyzer (D3.13): a dictionary of dates: "A value written into a live member is a dictionary whose values
  hold dates. A date reaches a live member by its member's name: pass the entries as a list of objects that name each
  key." At build by ALIS033, and where the view renders on code it does not follow (D18.6): new:N18.10.
- `X3.12` refusal (D3.13): a type that holds itself and a date: "A value written into a live member holds its own
  type and a date in it. A date reaches a live member through a shape that ends: pass the dates as a list of their
  own."

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
  holds, of each item of a list it holds) and the field a find returns. A check reads the starts the endpoint
  that binds its body leaves, by host and format (D16.6); a data read follows the contract of the data. A member
  written by its own JSON converter (`[JsonConverter]` on the member) holds what the converter writes, not its
  members, so a path that reads past it is refused where the view renders (X4.13), in a list's lambda, a `Find`'s
  field and a member's `Read`; a converter on the member's type writes no members a read can name, and is refused as
  a member the options do not write. A start is written as its member is written, in the member's own form (D6.12),
  so a `Charted` omitted at `Due` starts "Due" where its own converter writes "Due" and the app's options "due".
- D4.3 A read that yields a promise where a value is declared stops the pipeline. A method declared
  `MethodAsync` is awaited, and its settled value is its result. A rejected promise stops the pipeline. A
  `MethodAsync` whose vendor returns a value that is not a promise is awaited as that value, since the declaration
  decides (D11.3). The result binds at its line (D5.11); an async method called inside a list's lambda, whose
  operation reads every item in one turn, is refused where the view renders (X4.12).
- D4.4 A call of a method declared to return nothing that returns a promise stops the pipeline after the call,
  naming `MethodAsync`. There is no call whose promise nobody awaits: the stop follows it, and a later rejection of
  that promise adds no second error.
- D4.5 A missing value set as an element's text or HTML shows nothing; a missing class name changes no class.
- D4.6 A path value that reads nothing stops its request before it is sent, and its `Finally` runs: the request's
  `guard` state stops it on what its `check` state read (D10.23) (`Absence.tla:MissingPathValueStopsAndFinallyRuns`).
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

**Schema nodes.** `get`, `call`, `set` with `members`; `data` with its steps; `do` and `let` with `await`; `request`; `op`
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
- `Absence.tla:MissingPathValueStopsAndFinallyRuns`
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
  (`Alis.Reactive.Assets/runtime/grammar/execute.ts:42-48`); a missing path value throws inside the plugin
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
- `X4.4` behavior (D4.6): a path value that reads nothing sends no request, shows none of its busy targets, and runs its
  `Finally`.
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
- `X4.12` analyzer (D4.3, D5.11): an async method's result read inside a list's lambda: "An async method is called
  inside a list's lambda, which reads every item in one turn and waits for nothing. Call the method in the pipeline
  and read its result in the lambda." At build by ALIS033, and where the view renders on code it does not follow (D18.6): new:N18.10.
- `X4.13` refusal (D4.2): a path past a member its own converter writes, or past a list's item its converter writes
  (`Select(d => d.Charter).Count(c => c.Name == "Ruth")`), in a list's lambda and in a `Find`'s field:
  "'{path}' reads past '{member}', which its own JSON converter writes, so the browser holds what the converter
  writes, not its members; read '{member}' itself, or read past it on the server."
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
  - Presence. A member is required unless the API marks it optional (`?` in a TypeScript declaration), and its
    declaration says which with `optional: true` (N5.4). Reading, writing or calling an absent required member stops
    its pipeline. An optional member is absent when its holder lacks it or holds it as undefined, since `?` admits
    both (a class field compiled to ES2022 holds undefined): it reads nothing, a call of it runs nothing and its
    result reads nothing, and a write to it sets it. The mark is the member's, not its holder's: an optional member
    of a live object declared neither optional nor nullable stops when that object holds nothing, as any member of it
    does, and the members of a live object declared optional or nullable read nothing while it holds nothing.
  - Value. The member's C# type is the API's value type, nullable where the API's value is: `string | null` is
    `string?`, `number | null` is `decimal?`. A value the API types with no null and documents `@default null` holds
    null until it is set, so it is nullable too: NumericTextBox's `value: number` is `decimal?`, as the contract reads
    it (D17.5). Null is a value that data holds and a declaration states, never a
    state of the framework (the owner, 2026-11-06: "That is about the framework, not about the value"). A payload
    whose API is `{ name: string; dateOfBirth: Date; birthCountry: string | null }` declares `ReadOnly<string> Name`,
    `ReadOnly<DateTime> DateOfBirth` and `ReadOnly<string?> BirthCountry`; a null country reads as null, and a text
    set from it shows nothing. A live object a member holds or a method returns, which the API types `T | null` or
    `T | undefined`, is declared with `nullable: true`, and its members read nothing while it holds nothing.
  - The vendor contract analyzer (D17.5) checks both marks against the API, a reference type's nullability by its
    nullable annotation, so neither drifts from the vendor: nullability by variance (ALIS019), a live object's
    `nullable: true` included, the type by its kind (D17.5's type check), and presence both ways (ALIS022).
  - Data the app types with its own C# type (a response body, a pushed message, the value past a declared member,
    the value a method returns) reads its start where a field is absent and null where the field holds null (D4.2);
    on an event's payload X4.3 states both.

- D5.4 A dotted member at any depth reads the declared steps as required and every step past the declared
  member as data, which reads its start when absent (D4.2): a `get` of the declared steps and a `data` node past them.
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
- D5.12 The core makes a declaration (a component, a plugin, a live payload, every `JsObject` a member reaches)
  by `new T()` under the `new()` constraint each door that names one carries, and C# compiles `new T()` to
  `Activator.CreateInstance<T>()` on both targets, and on net10 a constructor's throw reaches the developer wrapped in
  `TargetInvocationException` (`probe:core-new-constraint-wraps`). So no refusal of the core is thrown from a
  declaration's constructor, and a base class's constructor (`Component`'s, `Plugin`'s) keeps what it is given. A
  declaration's arguments are refused at build when the build reads them (ALIS024, ALIS034), and where the view
  renders when the plan writes the declaration: a plugin's name with a dot is X17.27 in both places. `new T()` is a
  read of `Activator`, which D18.1 lists since gate 1 (C12).
- D5.13 A live object whose type JavaScript iterates declares its iterator as it declares any member (D5.3): the type
  declares `[Symbol.iterator]` and is no array, and the iterator yields live objects of a declaration or data of a C#
  type. lib.dom's `HTMLCollection`, which an element's `children` holds, yields `Element`, and the `DOMTokenList` its
  `classList` holds yields `string`; neither is an array, and neither has `filter`, `map`, `some` or `find`
  (`probe:dom-children-iterable`, `probe:dom-classlist-iterable`), so `LiveObjects<T>`, the array a property holds
  (D5.8), mirrors neither. Such an object takes the list operations of D6.2 over the items its iterator yields when the
  operation runs (D6.17), beside its own members (`DOMTokenList`'s `add`, `HTMLCollection`'s `length`), and its items are
  live objects or data as its iterator declares them (D6.8). The vendor contract writes the type each iterator yields
  (D17.5), and the build refuses an iterator declared on a type that has none, items the iterator does not yield, and
  `LiveObjects<T>` on a member whose type is no array.

**Schema nodes.** `obj`, `get`, `call` with `members`; `data` past a declared member (D5.4); `Handler.path`; `op`
(`toArray`, D6.17).

**Checker.**
- `probe:core-new-constraint-wraps`
- `Absence.tla:RequiredAbsentStops`
- `Absence.tla:DataAbsentReadsItsStart`
- `probe:ej2-max-public-method-arity`
- `probe:ej2-methods-past-four-arguments`
- `probe:ej2-event-args-presence-marks`
- `probe:dom-children-iterable`
- `probe:dom-classlist-iterable`

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
- `N5.4` new: a member's presence mark on every declaration door, optional where the typed API marks `?` (D5.3),
  and `nullable: true` on `LiveObject`, `LiveObjects`, `ReturnedObject` and `ReturnedObjects`; built in POC 13d as
  named arguments, `Property<string>(optional: true)`, and their form goes to gate 2.
- kept on a new holder: the whole declaration vocabulary on an event's payload, at any depth (D5.6), where the
  redesign's payload is already a `JsObject` (`Alis.Reactive/Components/Event.cs:14`).
- `N5.7` new, its form gate 2's: a `JsObject`'s iterator, declared with the declaration of its items, live or data
  (D5.13); the list operations of N6.1 and D6.10 take an object that declares one. It replaces the redesign's
  `HtmlElement.Children` and `ElementClassList.Items`, each a `ReactiveArray` over a member read with no declaration
 
  (`Alis.Reactive/PlanAuthoring/Elements/HtmlElement.cs:52-53`, `Alis.Reactive/PlanAuthoring/Elements/HtmlElement.cs:120-121`).

**Evidence.**
- B7: a custom event or a response is `Data<T>` (`Alis.Reactive/PlanAuthoring/Pipelines/TriggerBuilder.cs:50`) while
  server-pushed JSON is a `JsObject` with `ReadOnly<T>` members (`Alis.Reactive/PlanAuthoring/Pipelines/TriggerBuilder.cs:109-111`,
  `Alis.Reactive.SandboxApp/Areas/Sandbox/Models/HttpPipeline/RealTime/RealTimeModel.cs:8-10`).
- G7: one recursive vocabulary, `Object<T>`, `Objects<T>`, `MethodAsync<int>`
  (`tests/Alis.Reactive.Framework.App/Components/Slate/Slate.cs:20-28`).
- A returned object's members read and write as optional today (`Alis.Reactive/PlanModel/Values/Target.cs:123`,
  `Alis.Reactive/PlanModel/Reactions/Steps.cs:88`), while its methods are called as required
  (`Alis.Reactive/PlanModel/Reactions/Steps.cs:114`); D5.5 makes every member required.

**Defects.**
- `X5.1` type (D5.7): a pushed message cannot be written to: new:N5.1 types it `Data<TMessage>`, which has no `Set`.
- `X5.2` type (D5.2): a method cannot be called on data with no declared methods; `Data<T>` exposes reads only
  (`Alis.Reactive/PlanAuthoring/Pipelines/TriggerBuilder.cs:50`).
- `X5.3` behavior: a returned object that lacks a declared method stops the pipeline at the call.
- `X5.5` type (D5.8): a property's object cannot be mistaken for a method's returned object, and no member scan
  decides it: new:N5.2.
- `X5.6` type: a vendor method with more arguments than a declaration door takes: new:N5.3 takes any number.
- `X5.4` behavior: a field of the object a property holds reads its start when the object lacks it (D4.2), and
  nothing when the property holds null.
- `X5.7` behavior: a payload field its API marks optional, which the vendor leaves out, reads nothing and the text
  it sets shows nothing; a required field the vendor leaves out stops its pipeline, and the step after it does not
  run.
- `X5.8` behavior: a payload's `string?` field holding null reads as null: the text it sets shows nothing, and the
  pipeline goes on.
- `X5.9` analyzer: a declaration whose presence or nullability differs from its vendor's typed API, nullability by
  ALIS019 (POC 13a, a live object's in 13d) and presence by ALIS022 (13d): new:N17.2.
- `X5.10` behavior: a property of an object a live payload holds, written by the handler, changes that object as
  the page shows, and its method, called, runs.
- `X5.11` behavior (D5.11): a pump's summary read twice calls `summary()` once, two calls of it run it twice, and a
  summary read before a step that changes the rate still holds the rate it returned; the running channels counted in
  a list's lambda call `isRunning()` on each channel as it is when the step runs.
- `X5.12` behavior (D4.1, D4.3, D4.4, D7.6): a method the older pump lacks, a member the pump holds as text, a
  method declared to return nothing and one declared to return a value, each of which returns a promise, stop the
  pipeline at the call, and the step after it does not run; a null literal, a null the returned data holds and a
  result that reads nothing each reach the method as null.
- `X5.13` analyzer (D5.11): a method declared to return nothing, called inside a list's lambda: "A step is written
  inside a list's lambda, which reads each item and writes nothing. Write the step in the pipeline." At build by ALIS032, and where the view renders on code it does not follow (D18.6): new:N18.9.
- `X5.14` analyzer (D5.11): a result, a returned object or a returned list that a C# variable carries into another
  pipeline: "A method's result is read outside the block that called the method. Call the method in the block that
  reads its result, or in a block around it." At build by ALIS032, and where the view renders on code it does not follow (D18.6): new:N18.9.
- `X5.15` analyzer (D5.13): `LiveObjects<T>` declared on an element's `children`, whose type `HTMLCollection` is no
  array, an iterator declared on a type that has none, and an iterator whose items are not what the type yields, each
  refused at build against the vendor contract: new:N5.7.

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
  sums to nothing (D6.3); a list with no item sums to 0, as .NET's does. Each total is a JavaScript number, in the
  live form (D7.7), as `poc/runtime/src/sums.ts` returns it. A `decimal` term is a JSON number or the
  decimal text the app writes for it (`JsonNumberHandling.WriteAsString` writes 0.10m as "0.10"), added the same way;
  a term its own converter writes, or any other term written unlike the options, is refused (D6.12, X6.39). Each term
  is read by the one decimal reader, which `eqDecimal` shares (D6.12): a JSON number at its double's shortest text, or
  a text of an optional `-`, digits and an optional `.` with one to 28 digits, whose digits without the point are at
  most decimal.MaxValue's 79228162514264337593543950335, the form System.Text.Json writes a decimal as text in at every
  scale (`probe:core-json-decimal-text-written`); leading zeros and a minus on zero read as .NET reads them, "007" as 7
  and "-0" as 0. A text or a number past 28 places or past those digits is no decimal the app writes and stops the
  pipeline, where .NET rounds a text past 28 places when it reads it and refuses one past decimal.MaxValue
  (`probe:core-json-decimal-text-rounded`); the POC's sum reads such a text with no limit on its places or digits
  (`poc/runtime/src/sums.ts:7`). Any other term, null among them, stops the pipeline, as these three types have no null to add; a term that reads
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
  - A value the view holds is written in the list's base form (D7.7), through the app's JSON options for data the
    server wrote and in the live form for a list a live object holds, as the type of the member it is compared with,
    taken under the conversion the compiler adds to compare an enum or a `char` as its number: `DoseStatus.Due` is
    "due" when the app writes enums by name, and `'I'` is "I". A member written unlike the options is compared in its
    own form (the last rule).
  - Equality (`==`, `!=`) compares a member with a value written as a JSON scalar or null, or two members of one type
    written as a scalar: a number, text, `bool`, `char`, `Guid` or an enum, and a date or a time by its key (below). A
    value written as an object or a list is refused, and so are two members of a type that is not a scalar: the browser
    compares objects by reference.
  - Ordering (`<`, `<=`, `>`, `>=`) compares a member with a value written as a JSON number or null, or a `char` with a
    `char`. A value written as text is refused, which covers an enum the app writes by name and a number the app writes
    as text; a date or a time is ordered by its key (below). Between two members it compares number types, `char`, and
    a date or a time of one type, and refuses every other type.
  - A date or a time (`DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly`, `TimeSpan`, each nullable too) compares as
    .NET compares it, to the tick: a `DateTime` by its `Ticks`, its wall time whatever its `Kind`; a `DateTimeOffset`
    by its UTC instant; a `DateOnly` by its `DayNumber`; a `TimeOnly` and a `TimeSpan` by their `Ticks`; a nullable by
    the lifted operators (`probe:core-date-compares-ticks`, `probe:net48-datetime-compares-ticks`,
    `probe:net48-datetime-ticks-drop-kind`, `probe:net48-datetimeoffset-compares-utc`,
    `probe:net48-timespan-compares-ticks`). C# writes each operand as its type's key over the value as its source holds
    it, never over `as date` or `as instant`, whose `Date` holds milliseconds and moves a wall time daylight saving
    skips or repeats (`probe:js-date-milliseconds`, `probe:js-date-wall-gap`, `probe:js-date-wall-overlap`):
    `dateKey` for a `DateTime`, `wallKey` for a `DateTime` in the app's form, `instantKey` for a `DateTimeOffset`,
    `dayKey` for a `DateOnly`, `timeKey` for a `TimeOnly` and `spanKey` for a `TimeSpan`, each read as
    `spec/Core/conversions.md`'s Keys read it. It compares the two keys by `eq`, `neq`, `lt`, `lte`, `gt` or `gte`, a
    member with a value or two members of one type. A value the view holds is written in its partner's form (D7.7) and
    keyed as its partner. A lookup in a list the view holds (`Contains`), and `In` and `NotIn` (M3.D1), compare by keys
    too: `or` of `eq` of the keys, one term per value, and with no value `neq` of the source's key with itself, which
    reads nothing over nothing and fails over any other value, as `includes` over an empty list reads. A key stops the
    pipeline, naming the text, on a value outside its forms, data the framework did not produce, the boundary the root
    `CLAUDE.md` names for errors, except where C# writes the member's start for it to read in that value's place:
    a page URL member (N10.7) and an input's text (N3.3) (X6.58). Text in a form's shape that names no value, a 30th of February, reads nothing (D3.3), as the endpoint binds none, or the start where C# writes
    it. Neither the view's time zone nor the browser's clock decides. In a pipeline condition, a `DateTime` value
    given where a `DateTimeOffset` is compared converts by C#'s implicit operator where the view renders, at the server
    clock's offset for its wall time, .NET's own meaning (`probe:core-implicit-instant-server-clock`,
    `probe:net48-implicit-instant-local-offset`); in a list's lambda that conversion is refused, of an item by X6.49
    and of a view value by X6.6, whose door is a local of
    `DateTimeOffset` computed before the lambda. Any other pair of these types does not compile (X8.1), and its door is
    a local of the other type, as `DateOnly.FromDateTime(..)` computes one. A date or a time a converter writes is
    refused, the app's options' (X6.57) and its own (X6.43).
  - A cast that narrows (`(int)d.Milligrams`) is refused: it changes the value as C# computes it, and the browser's
    number does not.
  - A cast between `decimal` and `float` or `double` is refused (X6.50): .NET rounds `(decimal)0.30000000000000004` to
    0.3 and `(double)1E-28m` to 1.0000000000000001E-28, where the browser keeps the number it parsed. A whole number
    or a `char` converted to `decimal` keeps its value, and the lambda reads through it (X6.51).
  - A conversion a type declares for itself, a user-defined conversion in C#'s terms (`d.Seal == "S4"` through
    `DoseSeal`'s implicit operator to text), is refused at every position of a list's lambda (X6.49), as it is in `??`
    (X6.33): the browser has no conversion the app wrote. `decimal`'s exact conversions are the closed set the lambda
    reads through, keyed as `TranslatedMembers` keys its table.
  - A `decimal` that the app's options or number handling, the member's or its class's, write as text is compared by
    `==` and `!=` by value, as .NET's decimal equality compares it (gate 1's P15): equal decimals write unequal text,
    0.10m "0.10" and 0.1m "0.1". C# writes `op(eqDecimal, a, b)` for `==` and `op(not, op(eqDecimal, a, b))` for
    `!=` wherever they take such a decimal: a member and a value, two members, and in a pipeline condition a source
    and a value or two sources (D8.10). `eqDecimal` reads each argument by D6.11's decimal reader and compares the two
    as whole numbers at the larger of their scales, with no binary fraction: "0.10" equals "0.1", "-0" equals
    "0.00", and "0.1000000000000000000000000001" differs from "0.1", which one double holds. An argument that reads
    nothing makes it read nothing, so `!=` reads nothing there too (D8.9's `not`); null equals null and no decimal,
    as .NET's lifted `==` decides; a value in which the reader holds no decimal stops the pipeline, as a `Sum` term
    does. A decimal written as a JSON number keeps `eq` and `neq`, since 0.10 and 0.1 parse to one number. A
    `decimal` its own converter writes as text is refused (X6.43): the converter decides its text, so the browser does
    not hold the value .NET compares. Ordering a decimal written as text stays refused (X6.12, X6.28, X6.39, X6.40): it needs a comparator under a
    sort's null-first order and four orderings under .NET's lifted one, where equality needs one operator. The POC
    refuses decimal text in equality until the rewrite builds `eqDecimal` (X6.53, X8.26).
  - A `float` or `double` written as text is refused in equality as in ordering (X6.43): -0.0 writes "-0" where it
    equals 0.0, and `double.NaN` written "NaN" equals its own text where .NET's NaN equals nothing
    (`probe:core-json-double-text`). A fraction written as text is refused in a lookup of a list the view holds
    (X6.54), since `includes` compares the texts. A whole number written as text equals as its number does, and is
    ordered by no rule.
  - A member written unlike its list's base form (D7.7) holds its own form, and a refusal names that base as {base}:
    "the app's options" for data the server wrote, "JavaScript's value" for a list a live object holds. A member holds
    its own form by its own JSON converter (`[JsonConverter]` on
    the member, `JsonPropertyInfo.CustomConverter`), or by number handling whose writing bits (`WriteAsString`,
    `AllowNamedFloatingPointLiterals`) differ from the options', set on the member or on its class, for a number or
    a list or dictionary of numbers, the members it reaches (chosen within the owner's yes, 2026-11-07, built in
    POC 13f-ii). C# reads the form from the options' contract where the view renders, with no reflection. A value
    compared with such a member by `==` or `!=` is written in that form, the options copied with the member's
    converter first or its number handling set, so `d.Charted == DoseStatus.Given` compares "Given" where the app's
    options write "given". Equality takes the member only when the compared type is a scalar (X6.41), and the
    fraction rules above hold for its form. Every other use refuses it, because the browser does not hold the value
    .NET computes with: an ordering (X6.40), a comparison of two members (X6.42), and any other computation in the
    lambda, a predicate's bool, a .NET member it is the receiver or the argument of (`charted.Contains(d.Charted)`),
    `??`, `?:`, an order key (D6.13) and a `Sum` term (D6.11) (X6.39). Two reads take the member as the browser
    holds it: the whole body of a `Select`, and a `decimal` `Sum`'s term written as decimal text by number handling
    (D6.11). A list's item is held in its list's form, so `Select(d => d.Charted).Count(s => s == DoseStatus.Given)`
    compares each item in the converter's form, and a lambda reading past an item its converter writes is refused
    (X4.13). Outside a list's lambda, a source carries its form (D7.7).
- D6.13 `OrderBy(key)` and `OrderByDescending(key)` write `call(list, ["toSorted"], [fn(a, b)])` whose body compares
  the key read from each item, the two items swapped for descending, so items with equal keys keep their list order
  as .NET's stable sort keeps it. C# picks the comparison from the key's type and how the app's JSON writes it, where
  the view renders: a key written as a JSON number compares as numbers (`compareNumbers`), a `bool` puts false first
  (`compareBooleans`), a `char` compares by its code unit (`compareOrdinal`), a date or a time compares its key
  (D6.12) by code unit (`compareOrdinal`, `probe:js-key-text-order`, `probe:js-span-key-order`), and null comes first, as
  `Comparer<T>.Default` puts it. A nullable value key binds the overload whose sample is the underlying type's value.
  A key written unlike its list's base form (D7.7), by its own converter or number handling, is refused (D6.12, X6.39).
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
  A cast from a nullable to a type that holds no null, `(int)d.Priority`, is its `Value`, as the compiler writes it,
  so `(int)d.Priority != 2` drops a dose whose priority is null (X6.47); a narrowing cast stays refused (X6.14). A
  value the view holds, cast so and null when the view renders, is refused there, where .NET throws (X6.48).
  A `??` whose left side the compiler converts by a user-defined conversion is refused (X6.33): the browser has no
  conversion the app wrote. Outside a lambda, at any value position, `source.IfNull(fallback)` writes the same
  `coalesce` and `test.Choose(whenTrue, whenFalse)` the same `cond` (N7.4), each term read only when it is reached.
- D6.15 `Regex.IsMatch(text, pattern)` and `Regex.IsMatch(text, pattern, options)` in a list's lambda write
  `op(matches, text, lit)`, the pattern's browser copy, which D16.11's one reader writes where the view renders from a
  pattern the view holds (Decision 2, the owner, 2026-11-06). A pattern read from the item, or null when the view
  renders, is refused (X6.38). A text that reads nothing or holds null reads nothing, as every text operator does
  (`spec/Core/translated-members.md`), where .NET throws `ArgumentNullException`.
- D6.16 A `Select` builds its whole list where it is read, one item per item of its list, so a list never holds a value
  its item type cannot hold (gate 1's P6). A field that reads nothing, past a holder that holds null or as a cast of a
  null nullable (D6.14), is null in its list when the field's C# type holds null (a reference type, a `Nullable<T>`),
  as D6.14 reads nothing where .NET throws, and stops the pipeline when it holds none (`int`, `decimal`, `bool`, an
  enum, a date, any other struct), as a `Sum` term (D6.11) and an `OrderBy` key (D6.13) stop. C# writes the field of
  such a type as `op(nonNull, field)`, which is the field's value and stops the pipeline where it reads nothing or
  holds null; C# decides by the field's type argument itself, `default(TField) is null`, which reads no type
  information. So `Select(d => d.Supply.Vials)` stops on a dose whose supply is null, and
  `Select(d => (int?)d.Supply.Vials)` holds null for it. .NET's `Select` is lazy and throws when its consumer runs the
  field on an item: `ToList`, `Sum` and `Count()` throw past a null (`probe:core-select-past-null`), so the plan stops
  where .NET throws, except a `Select(..).Any()` over a list, which net10 answers from the list's count alone; `Any()`
  over the list itself is the door for that. A `Sum` keeps its own term: `sumInt`, `sumDouble` and `sumDecimal` stop on
  a term that reads nothing, so a `Sum`'s map writes no `nonNull`.

- D6.17 A list an object's iterator yields (D5.13) is read by `toArray`, the one operator that turns a value JavaScript
  iterates into an array, and every operation of D6.2 reads that array: `call(op(toArray, classes), ["filter"], [fn])`.
  C# writes `toArray` only over an object that declares an iterator, never over a list a declaration or the app's data
  declares, whose operations call the list's own member (D6.3). It reads its value as D6.3 reads an operation's member,
  with the iterator as the member: a value that reads nothing or holds null reads nothing, so the operation reads
  nothing; a value that holds `[Symbol.iterator]` is the array of the items it yields when the operation runs, in
  order, as `Array.from` gives them; and a value that lacks it stops the pipeline. A value that has the member is read as JavaScript reads it: a text iterates its
  characters by code point (`probe:js-array-from-text`). The redesign writes `toArray` before every operation
  (`Alis.Reactive/PlanAuthoring/Arrays/ArrayOperations.cs:108`) and reads a missing value as a list of no item and a
  text or a number as a list of one (`Alis.Reactive.Assets/runtime/grammar/operators.ts:86-93`), so its count over a
  list that reads nothing is 0, where D6.3 reads nothing.

**Schema nodes.** `call` with `fn`; `get` of `length`; `op` (`coalesce`, `cond`, `nullableValue`, `eqDecimal`, `nonNull`, `toArray`, `dateKey`, `wallKey`, `instantKey`, `dayKey`, `timeKey`, `spanKey`).

**Checker.**
- `Absence.tla:MissingListReadsNothing`
- `Schema.tla:DecimalTextComparedByValue`: no `eq` or `neq` the C# writes takes a decimal written as text
- `Schema.tla:SelectFieldHoldsItsType`: no `Select` field of a type that holds no null is written bare
- `Schema.tla:DatesComparedByKey`: no comparison or sort's comparison the C# writes takes a date or a time bare
- `Schema.tla:KeysOnlyCompared`: a key is written only as an argument of a comparison or a sort's comparison whose
  arguments are keys
- `Schema.tla:KeyFallbackIsALiteral`: a key's second argument, where C# writes one, is a literal of the member's start
- `probe:core-date-compares-ticks`
- `probe:net48-datetime-compares-ticks`
- `probe:net48-datetime-equals-ticks`
- `probe:net48-datetime-ticks-drop-kind`
- `probe:net48-datetimeoffset-compares-utc`
- `probe:net48-datetimeoffset-equals-utc`
- `probe:net48-timespan-compares-ticks`
- `probe:core-stj-date-time-forms`
- `probe:core-query-date-forms`
- `probe:core-form-date-forms-every-culture`
- `probe:core-bound-refused-values`
- `probe:core-bound-offset-into-datetime`
- `probe:core-bound-lenient-forms`
- `probe:core-implicit-instant-server-clock`
- `probe:net48-implicit-instant-local-offset`
- `probe:core-stj-own-date-converters`
- `probe:mvc5-timespan-parse`
- `probe:mvc5-datetimeoffset-parse`
- `probe:js-dates-zone`
- `probe:js-date-wall-gap`
- `probe:js-date-wall-overlap`
- `probe:js-date-milliseconds`
- `probe:js-key-text-order`
- `probe:js-span-key-order`
- `probe:core-select-past-null`
- `probe:core-json-decimal-text-written`
- `probe:core-json-decimal-text-rounded`
- `probe:core-json-double-text`
- `probe:dom-children-iterable`
- `probe:js-array-from-text`
- `check:fluent`

**Doors.**
- kept: `ReactiveArray`'s operation names (D6.2; `Alis.Reactive/PlanAuthoring/Arrays/ReactiveArray.cs:44`,
  `Alis.Reactive/PlanAuthoring/Arrays/ReactiveArray.cs:93`), as extension methods on the source (D6.10, N6.1), so
  `ReactiveArray`, `ReactiveValue` and `p.From` leave (module 4, `spec/ArrayLambdas/module.md`).
- `N6.1` new: `LiveObjects<TItem>.Count()` and `LiveObjects<TItem>.Select(..)`, replacing `Count` and `Each`, beside
  `At(n)` and `Where(..)`; the same operations over a list of data are extension methods on
  `ITypedSource<IEnumerable<TItem>>` (`DataLists`, POC 4, D6.10).
- `N6.4` new, its analyzer built in POC 13h as ALIS028: `public interface ITypedSource<out T>`, which `TypedSource<T>`
  implements and nothing else in the framework does; the list operations over data extend it (D6.10). A type outside
  the framework's assembly that implements it, directly or through an interface of its own, is refused at build; the
  framework's own sources are not. A source the build does not see, as a type a library generates when the app
  runs, is refused where the view renders.
- `N6.3` new, built in POC 13g as ALIS026 and ALIS027: `ReadsNothingSuppressor`, a `DiagnosticSuppressor` that
  suppresses CS8602, and CS8629 on a nullable's `.Value`, where a data lambda reads its item in the browser and a read
  past a holder that holds null reads nothing (D4.2, D6.14, D8.9). An expression tree refuses `?.` (CS8072), so
  without it a view writes `round.Carer!.Name`, a `!` that claims what the plan does not need. The doors are a closed
  set, matched by the method's definition and never by its name alone: the predicate of `Count`, `Where`, `Any`, `All`
  and `Find`, the field of `Select` whose type holds null (D6.16) and of `Find(predicate, field)` (D6.9), the path of
  `ReadOnly<T>.Read` and of
  `MethodResult<T>.Read`, and an `Any`, `All` or `Count` predicate nested in any data lambda (D6.8). The warning is
  suppressed only on an expression that reads an item, the split `LambdaCondition` reads by: a value the view holds
  (`Model.Cover.Name`, a captured local) is read where the view renders and refused there when it is null (D6.7), so
  its warning stays. It stays where null stops the pipeline: a `Sum` term (D6.11), an `OrderBy` or
  `OrderByDescending` key (D6.13) and a `Select` field whose type holds no null (D6.16). It stays on an argument (CS8604, `Contains(null)` stops, X6.19), on a lambda over a
  list the server holds, and on an input named by a member path (`InputField`, `pipeline.Input`), where MVC's own
  `TextBoxFor` warns the same in the same view. A cast from a nullable to its value, `(int)d.Priority`, reads as its
  `Value` does (D6.14), so CS8629 on it is suppressed in the same doors (POC 13g-ii).
- `N6.2` new: `internal static class TranslatedMembers`, the closed table of D6.5.

**Evidence.**
- B8: `Each` and the property `Count` on a declared list (`Alis.Reactive/Components/JsMembers.cs:135`,
  `Alis.Reactive/Components/JsMembers.cs:140`) beside `Select` and `Count()` on an array
  (`Alis.Reactive/PlanAuthoring/Arrays/ReactiveArray.cs:44`, `Alis.Reactive/PlanAuthoring/Arrays/ReactiveArray.cs:93`).
- B11: the method table is scanned for `PureMethodAttribute` (`Alis.Reactive/PlanAuthoring/Arrays/ElementExpressionCompiler.cs:51-55`).
- An element's children and classes are read as lists with no declaration and counted at lunch seating, its seats by
  their attributes and its flags by name
  (`Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Native/DomOps/Index.cshtml:16-32`), through a `toArray`
  the redesign writes before every operation (`Alis.Reactive/PlanAuthoring/Arrays/ArrayOperations.cs:108`).

**Defects.**
- `X6.1` type (D6.2): a list cannot be counted two ways: new:N6.1 leaves one `Count()`.
- `X6.2` analyzer: a list's lambda that calls a member outside the table: "'{term}' has no browser form in a
  list's lambda; use a member the framework translates, or move the work to the server." At build by ALIS033, and where the view renders on code it does not follow (D18.6): new:N18.10.
- `X6.3` behavior: `Where(x => x.Name.Trim() == "")` over a list whose item lacks `Name`, whose start is null,
  reads that item's test as nothing, so the item is not kept (D4.2, D6.4).
- `X6.4` behavior: `Any()` over a list that reads nothing does not satisfy `When(list.Any())`.
- `X6.9` behavior: `Sum` over `decimal` terms 0.1 and 0.2 reads 0.3, equal in value to .NET's 0.30 (how a decimal
  shows as text is Q3's); over `double` it reads 0.30000000000000004, as .NET's `double` does; over `int` a running
  total past 2,147,483,647 stops the pipeline and the step after it does not run, though the terms after it bring the
  total back into range; over `decimal` a total past `decimal.MaxValue` stops the same way; over a list with no item
  it reads 0.
- `X6.8` analyzer: a step written inside a list's lambda: "A step is written inside a list's lambda, which reads
  each item and writes nothing. Write the step in the pipeline." At build by ALIS032, and where the view renders on code it does not follow (D18.6): new:N18.9.
- `X6.5` behavior: `Where(r => r.Ward == Model.Ward)` keeps the items of the ward the view's model names, and a
  local the lambda captures is read the same way.
- `X6.6` analyzer (D6.7): a view value that calls a method or computes inside an
  array lambda: "'{value}' computes a value
  inside a list's lambda; compute it into a local before the lambda, such as var ward = Model.Ward.ToUpper();." At build by ALIS033, and where the view renders on code it does not follow (D18.6): new:N18.10.
- `X6.10` analyzer (D6.12): a comparison of objects: "'{term}' compares objects, and the browser compares objects by
  reference; compare a member of each, such as d => d.Carer.Name == lead." At build by ALIS033, and where the view renders on code it does not follow (D18.6): new:N18.10.
- `X6.11` behavior (D6.12): over visits the app's JSON writes, in New York,
  `Count(v => v.At < new DateTime(2026, 3, 8, 3, 0, 0))` counts the visit at 02:30 on 2026-03-08, a wall time daylight
  saving skips, and not the one at 03:15; `Count(v => v.At == new DateTime(2026, 11, 9, 8, 30, 0))` counts a Utc visit
  written 2026-11-09T08:30:00Z, a Local one written 2026-11-09T08:30:00-05:00 and an Unspecified one written
  2026-11-09T08:30:00, and not one at 08:30:00.0000001, which `Count(v => v.At > new DateTime(2026, 11, 9, 8, 30, 0))`
  counts; `Count(v => seen.Contains(v.Seen))`, over a view's `DateTimeOffset` array that holds 2026-11-09T18:00:00Z,
  counts a visit seen at 2026-11-09T13:00:00-05:00, one instant at another offset; each count is the same in Kolkata.
- `X6.12` refusal (D6.12): an ordering against a value the app's JSON writes as text: "'{term}' orders a value the
  app's JSON writes as text, which does not order as the value does in .NET; {compare it with == or !=, or order on
  the server, for a whole number, a decimal or an enum; order it on the server, for a float or a double, whose text
  does not equal as the number does either}." A float's or double's cure names no equality, which X6.43 refuses.
- `X6.13` analyzer (D6.12): an ordering of two members outside numbers, `char`, and a date or a time of one type:
  "'{term}' orders values the browser cannot order as .NET does; order numbers, a char, or a date or a time, or order
  on the server." At build by ALIS033, and where the view renders on code it does not follow (D18.6): new:N18.10.
- `X6.14` analyzer (D6.12): a cast that narrows: "'{term}' casts a number to a narrower one, which changes the value as
  C# computes it and not as the browser does; compare the member itself." At build by ALIS033, and where the view renders on code it does not follow (D18.6): new:N18.10.
- `X6.15` refusal (D6.7): a view value read through a null: "'{value}' reads a member of a value that is null when the
  view renders."
- `X6.16` behavior (D6.12): `d.Status == DoseStatus.Due` keeps the due doses when the app writes enums by name, and
  `d.Route == 'I'` keeps the injected ones. `d.Status == d.Planned` keeps the doses whose two members hold one value,
  `d.Milligrams > d.Priority` orders two numbers and leaves out a dose whose priority is null, and `d.Route < 'L'`
  orders a `char` as .NET does.
- `X6.17` analyzer (D6.5): `StartsWith(text)` or `EndsWith(text)`, which compare by the culture in .NET: "'{term}'
  compares by the culture in .NET, which the browser cannot match; pass StringComparison.Ordinal, such as
  d.Ward.StartsWith(\"N\", StringComparison.Ordinal)." At build by ALIS033, and where the view renders on code it does not follow (D18.6): new:N18.10.
- `X6.18` analyzer (D6.5): a `StringComparison` other than `Ordinal`: "'{term}' compares text by {comparison}; a
  list's lambda compares text by StringComparison.Ordinal." At build by ALIS033, and where the view renders on code it does not follow (D18.6): new:N18.10.
- `X6.19` behavior (D6.5): the text members agree with .NET where JavaScript's own methods do not: `Trim()` removes
  U+0085 and keeps U+FEFF, `ToUpperInvariant()` keeps `ß` and `ı` and maps U+1FB3 to U+1FBC, `ToLowerInvariant()`
  lowers a final `Σ` to `σ` and keeps `İ`, `string.IsNullOrEmpty` holds on a text that reads nothing, and
  `string.IsNullOrWhiteSpace` holds on U+0085. `TrimEnd(new char[] { })` removes white space as .NET does, and
  `Contains` given null to look for stops the pipeline, as .NET throws `ArgumentNullException`.
- `X6.20` behavior (D6.5, D6.8): `d => d.Carer != null && d.Checks.Any(c => c.Name == d.Carer.Name)` keeps the doses
  their carer also checked, the inner lambda reading its own item and the outer one; `Any()`, `All`, `Count()` and
  `Count(predicate)` over a list an item holds count as .NET counts them, `All` holding on an empty list.
- `X6.21` analyzer (D6.5): `Enumerable.Contains` over a collection that is not an array, and a `HashSet<T>`'s own
  `Contains`: "'{term}' looks in a
  collection that can compare by its own rules, as a HashSet's comparer does; copy it into an array before the
  lambda, such as var wards = Model.Wards.ToArray();." At build by ALIS033, and where the view renders on code it does not follow (D18.6): new:N18.10.
- `X6.22` analyzer (D6.5): `Contains` over a list the item holds: "'{term}' looks for a value in a list the item
  holds; use Any, such as d.Hours.Any(hour => hour == 8)." At build by ALIS033, and where the view renders on code it does not follow (D18.6): new:N18.10.
- `X6.23` analyzer (D6.5): `Contains` of an object: "'{term}' looks for an object, and the browser compares objects
  by reference; look for a member, such as names.Contains(d.Carer.Name)." At build by ALIS033, and where the view renders on code it does not follow (D18.6): new:N18.10.
- `X6.24` refusal (D6.5): `Contains` over a list that is null when the view renders: "'{term}' looks in a list that
  is null when the view renders."
- `X6.25` analyzer (D6.5): `Contains` with an equality comparer: "'{term}' passes an equality comparer, which the
  browser does not have; look up without it, or move the work to the server." At build by ALIS033, and where the view renders on code it does not follow (D18.6): new:N18.10.
- `X6.26` behavior (D6.5, D18.2): `wards.Contains(d.Ward)` over an array, an enum array written by name, a `List<T>` and
  an array typed as a sequence keeps the items the view lists; `d.Medicine.Contains('o')` finds a letter; a list's
  `Count` and an array's `Length` count their items.
- `X6.27` behavior (D6.13): a text key with no comparer orders as .NET orders it under the view's culture: en-US and
  the invariant culture list "DR İNCE" before "Dr Işık", tr-TR lists "Dr Işık" first; `OrderByDescending` reverses the
  order; `StringComparer.CurrentCulture` orders as no comparer does; in invariant globalization mode the text orders
  by code unit, as .NET compares it there; under `haw-US`, which the browser has no collation for, the sort and the
  steps after it do not run.
- `X6.28` refusal (D6.13): a key the app's JSON writes as text (an enum by name, a number written as text, a
  `Guid`): "'{key}' orders a value the app's JSON writes as text, which does not order as the value does in
  .NET; order by a member the JSON writes as a number, or order on the server."
- `X6.29` behavior (D6.13): in New York, `OrderBy(v => v.At)` lists 01:30, 02:30 and 03:15 of 2026-03-08 in that
  order; `OrderBy(v => v.Seen)` lists 2026-11-09T10:00:00-08:00 and 2026-11-09T13:00:00-05:00, one instant, in their
  list order, then 2026-11-09T18:00:00.0000001+00:00, then 2026-11-09T12:00:00-08:00; and
  `OrderByDescending(v => v.Length)` lists 1.02:00:00, 23:00:00 and -00:00:00.5000000.
- `X6.30` analyzer (D6.13): an object key: "'{key}' orders objects, which the browser does not order; order by a
  member, such as OrderBy(d => d.Carer.Name, StringComparer.Ordinal)." At build by ALIS033, and where the view renders on code it does not follow (D18.6): new:N18.10.
- `X6.31` analyzer (D6.13): a comparer other than `StringComparer.Ordinal` and the view's culture's: "'{key}' orders
  text by a comparer the browser does not have; pass StringComparer.Ordinal, or no comparer to order by the view's
  culture, or order on the server." At build by ALIS033, and where the view renders on code it does not follow (D18.6): new:N18.10.
- `X6.32` behavior (D6.13): `OrderBy` lists a number key smallest first, a nullable number with null first, a `bool`
  false first and text by code unit with `StringComparer.Ordinal`; `OrderByDescending` lists null last and keeps the
  list order of equal keys.
- `X6.33` analyzer (D6.14): a `??` whose left side the compiler converts by a user-defined conversion: "'{term}'
  converts its left side by a user-defined conversion, which the browser does not have; coalesce values of one
  type, or move the work to the server." At build by ALIS033, and where the view renders on code it does not follow (D18.6): new:N18.10.
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
- `X6.38` analyzer (D6.15, D16.11): each construct outside D16.11's closed set, with its message: "'{term}' holds
  '{construct}', which .NET and the browser read differently; write {spelling}." for `\d`, `\D`, `\w`, `\W`, `\s`,
  `\S`, `\A`, `\Z` and `.`, and for `\d`, `\w` and `\s` inside a class; "'{term}' holds '{construct}', which the
  browser does not read as .NET does; move the work to the server." for every other letter escape and every group
  opener but `(?:`, `(?=`, `(?!`, `(?<=` and `(?<!`; an escape that needs none, a `\u` or `\x` short of its hex
  digits, a trailing `\`, a `]` first in a class, a `[` inside one, a range that runs backwards, a `-` right after a
  range, a class or a group left open, a `)` or a `]` that closes nothing, a quantifier or a count with nothing to
  repeat, a `{` that is no count and a count whose least is above its most, each naming the escape that writes it;
  `IgnoreCase`, naming the spelled cases; any other option but `CultureInvariant`; a pattern read from the item; a
  pattern that is null when the view renders. At build by ALIS033, and where the view renders on code it does not follow (D18.6): new:N18.10.
- `X6.36` refusal (D6.13): text ordered by the culture under a culture with an alternate sort: "'{key}' orders text by
  the alternate sort '{sort}', which the browser has no tag for; pass StringComparer.Ordinal, or order on the
  server."
- `X6.39` refusal (D6.12): a member written unlike the app's options, used in a computation of a list's lambda (a
  sort key, a `Sum` term through a cast, a lookup in a list the view holds): "'{member}' is written by {its own JSON
  converter, its own JSON number handling, its class's JSON number handling}, unlike {base}, so the
  browser does not hold the value .NET computes with; take it whole in a Select, or compute on the server."
- `X6.40` refusal (D6.12): an ordering of a member written unlike the app's options, by its own converter or by its
  class's number handling: "'{term}' orders '{member}', which {cause} writes unlike {base}, so the
  browser does not order it as .NET does; order it on the server."
- `X6.41` refusal (D6.12): a member of a type that is not a scalar, written by its own converter, compared with a
  value: "'{term}' compares '{member}', which {cause} writes unlike {base}, and .NET compares the object
  it holds; compare it on the server."
- `X6.42` refusal (D6.12): two members compared where one is written unlike the app's options: "'{term}' compares
  two members, and {cause} writes '{member}' unlike {base}, so the browser does not compare them as .NET
  does; compare them on the server."
- `X6.43` refusal (D6.12): a `float` or `double` the JSON writes as text, a `decimal` its own converter writes as
  text, or a date or a time its own converter writes, compared: "'{term}' compares {a float or double the JSON writes
  as text, whose text does not equal as the number does, as -0 and 0 or NaN and NaN; a decimal its own converter
  writes as text, whose text the converter decides; a date or a time its own converter writes, whose text the
  converter decides}; compare it on the server."
- `X6.45` behavior (D6.12): `Select(d => d.Charted).Count(s => s == DoseStatus.Given)` counts the three charts the
  converter writes "Given"; `Select(d => d.Supply.Vials).Sum(v => v)` is refused by X6.39's message, the item held as
  text.
- `X6.44` behavior (D6.12): `d.Charted == DoseStatus.Given` keeps the three doses whose own `JsonStringEnumConverter`
  writes "Given" where the app writes enums in camel case, and `!=` keeps the other two; `d.Supply.Vials == 2` keeps
  the one dose whose supply's class writes its numbers as text; that class's text member computes as .NET does
  (`StartsWith`); `d.Charted == DoseStatus.Due` keeps the two doses that omit `Charted` at its default, read at its
  start in the converter's form; `Select(d => d.Copay).Sum(copay => copay)` adds the copays as the browser holds
  them, "0.3".
- `X6.47` behavior (D6.14): `(int)d.Priority != 2` counts the two doses whose priority is not 2 and drops the one
  whose priority is null, as `d.Priority.Value != 2` does; `d.Priority == (int)routine`, a view's `int?` that holds 2,
  counts the two doses of priority 2. `OrderBy(d => (int)d.Priority!)` and `Sum(d => (int)d.Priority!)` stop the
  pipeline at the dose whose priority is null, where .NET throws, and so do `Select(d => (int)d.Priority)` and
  `Select(d => d.Priority.Value)`, a field whose type holds no null (D6.16); `Select(d => d.Priority)` holds null for it
  in its list, so `Count(p => p != 2)` counts it.
- `X6.48` refusal (D6.14): a value the view holds, null when the view renders, cast to its value: "'none' is null when
  the view renders, and the cast takes its value, which .NET refuses; compare the nullable itself, such as d =>
  d.Priority == limit."
- `X6.52` analyzer: a class, a struct or an interface outside the framework that implements `ITypedSource<T>`, and a
  class that implements it through an interface of its own, are each refused at build, and a type that only holds
  the framework's sources is not: new:N6.4 (POC 13h, ALIS028).
- `X6.49` analyzer (D6.12): a comparison through a user-defined conversion: "'d.Seal' is converted by a
  user-defined conversion, which the browser does not have; compare a member it reads, such as d => d.Seal.Tag ==
  "S4", or move the work to the server." At build by ALIS033, and where the view renders on code it does not follow (D18.6): new:N18.10.
- `X6.50` analyzer (D6.12): a cast between `decimal` and a binary fraction: "'d.Volume' is cast to or from decimal,
  which rounds as C# computes it and not as the browser does; keep the member's own type, or move the work to the
  server." At build by ALIS033, and where the view renders on code it does not follow (D18.6): new:N18.10.
- `X6.51` behavior (D6.12): `d.Charge > freeCharge`, a `decimal` member and a view's `int`, counts the two charged
  doses through `decimal`'s own conversion from `int`.
- `X6.53` behavior (D6.12, gate 1's P15): `Count(d => d.Copay == 0.10m)`, over copays their own number handling
  writes as text, counts the doses holding "0.10" and "0.1" and not one holding "0.1000000000000000000000000001", and
  `!=` counts the others; under options that write numbers as text, `d.Charge == d.Claim` keeps a dose whose two hold
  0.10 and 0.1; a decimal read past a holder that holds null is kept by neither `==` nor `!=`; a copay that holds
  "abc" stops the pipeline, and the step after it does not run. The POC refuses the comparison
  (`tests/Alis.Reactive.Poc.PlaywrightTests/Comparisons/WhenAMemberWrittenByItsOwnJsonIsRefused.cs:34-39`) until the
  rewrite builds `eqDecimal`, whose unit turns that test into this one.
- `X6.54` refusal (D6.12): `Contains` of a fraction the JSON writes as text, over a list the view holds: "'{term}'
  looks for a fraction the JSON writes as text, and the browser's lookup compares its text, which does not equal as
  the number does, as 0.10 and 0.1 or -0 and 0; compare a decimal with == for each value, such as d => d.Copay ==
  0.10m || d.Copay == 0.25m, or look it up on the server."
- `X6.46` analyzer: a read past a nullable member in the predicate, field or path of a door that reads nothing past
  null builds with no `!` under warnings as errors, and so does a cast of a nullable there; the same read in a `Sum`
  term, an `OrderBy` key, a `Select` field whose type holds no null (D6.16), a value the view holds, a lambda over a list the server holds or an input's member path
  still warns: new:N6.3 (POC 13g, 13g-ii).
- `X6.55` behavior (D6.16): over three doses, one of whose supply is null, a pipeline that shows
  `Select(d => d.Supply.Vials).Count()` stops before it shows a count and logs the field, where the same page's
  `Select(d => (int?)d.Supply.Vials).Count(v => v == null)` shows 1.
- `X6.7` behavior: a list that holds a paged result (`{ result, count }`) where the declaration says a list stops
  the pipeline at its operation; a list that reads nothing reads nothing and the pipeline goes on (D6.3).
- `X6.56` behavior (D6.17): at lunch seating, a table's seats are counted from its seat list's children and those
  needing close supervision by each seat's `data-supervision`, and its flags from its card's classes; a fall risk added
  to the card is counted in the next step; and an object that declares an iterator but holds a paged result stops the
  pipeline at the operation, and the step after it does not run.
- `X6.57` refusal (D6.12): a date or a time the app's JSON options write by a converter of their own, compared or
  ordered, in a list's lambda, a pipeline condition or a validator's check (D16.16): "'{term}' compares a {type} the
  app's JSON options write by a converter of their own, whose text the browser cannot read as the value .NET holds;
  compare it on the server." C# tells the app's converter from System.Text.Json's own by the contract's converter,
  reference-equal to `JsonMetadataServices`' instance when the app adds none (`probe:core-stj-own-date-converters`).
- `X6.58` behavior (D6.12): a comparison of a date or a time stops its pipeline, logging the text, on a value a live
  object holds outside its key's forms: a vendor's `DateTime` member that holds the text "10/27/2026", and a
  `TimeOnly` payload member that holds "7:00 PM"; the step after it does not run, and the same pages with "2026-10-27"
  and "19:00" compare and run it, as they do with "2026-02-30" and "24:00", which name no value and read nothing.

## Q7. Every value position takes every source

**Decision.**
- D7.1 The positions are closed: a property or write-only property set (`Positions.PropertySet`,
  `Positions.WriteOnlySet`), an element's text, HTML and class name (`Positions.ElementText`, `Positions.ElementHtml`,
  `Positions.ElementClass`), both operands of every condition operator (a comparison's `Positions.ConditionLeft` and
  `Positions.ConditionRight`, the text tests', `MinLength`'s, `Between`'s and membership's `Positions.TextTestOperand`,
  `Matches`'s `Positions.MatchesPattern`), a request member (which Q9 places in the route, the query, the body or a
  header, `Positions.RequestMember`), a header (`Positions.Header`), a dispatch payload member
  (`Positions.DispatchPayloadMember`), and a method's, plugin's or hub's argument (`Positions.MethodArgument`,
  `Positions.PluginArgument`, `Positions.HubArgument`). A default header's value, `RequestDefaults.Header(value)`, is a
  header that takes a literal only: the defaults are configured at startup, before any page exists (D9.12).
- D7.2 The sources are closed, each a `TypedSource<T>`: a component's or plugin's member
  (`Sources.ComponentMember`, `Sources.PluginMember`), a method's result (`Method<T>`'s `Sources.MethodResult`,
  `MethodAsync<T>`'s `Sources.MethodAsyncResult`, a hub's `Invoke<T>`'s `Sources.HubInvokeResult`), a page URL member
  (`pipeline.PageUrl<TPage>()`, N10.7, `Sources.PageUrlMember`), a field of an event payload or a pushed message
  (`Sources.EventPayloadField`, `Sources.PushedMessageField`), a field of a response body
  (`Sources.ResponseBodyField`), a member as its container sends it (N13.1, `Sources.ContainerMember`), the result of
  an array operation (`Sources.ArrayOperationResult`), a literal (`Sources.Literal`), an object built from sources
  (`Sources.Object`), a list built from sources (`Sources.List`), and a value chosen from sources by `IfNull` or
  `Choose` (D6.14, `Sources.IfNull`, `Sources.Choose`).
- D7.3 Every position takes a literal of its type and every source whose type is assignable to it:
  `TypedSource<TSource>` with `TSource : TValue`, and a `TValue` source at a `TValue?` position. A condition operator's
  left operand is the one position that takes no literal: it is the source the operator tests, as `p.When(source)`
  reads (D8.2), and the literal stands on the right (`Positions.ConditionLeft`, gate 2's G9). A set takes a source
  as `ITypedSource<out T>`, and so does an element's class name, `ClassList`'s `Add`, `Remove` and `Toggle` (M5.N2,
  gate 2's G9), so a narrower reference type and a text without null reach them as they are. C# converts a
  method's `Arg<T>` only from a `TypedSource<T>`: it refuses a conversion from the covariant interface (CS0552), warns
  where only nullability differs (CS8620), which ALIS040 refuses as an error in every app (N18.14, gate 2's G8), has
  no conversion from a struct's source to its nullable's (CS1503), takes no
  conversion operator in an extension block (CS9282), and a second operator on `TypedSource<T>` makes the plain case
  ambiguous (CS0457). A struct's source reaches its nullable at a set as C# assignment widens it: `Set` on a
  `Property<T?>` or `WriteOnly<T?>` takes a `T` source (N7.6). At an argument and in a comparison, where C# offers no
  such overload, it goes through `OrNull()` (N7.5), which writes nothing of its own, and a narrower reference type
  reaches an argument through the declaration's `Pass`, a door Q5 keeps for the component author.
- D7.4 One builder makes an object from sources, member by typed member, and the dispatch payload, a method
  or plugin argument and a property write all use it. One door makes a list from sources.
- D7.5 Every condition operator takes a source for each operand: the text tests, `MinLength`, `Between`,
  membership and the comparisons. `Matches` takes a literal pattern only, because the pattern is checked
  where the view renders for the constructs JavaScript reads differently from .NET; a pattern read in the
  browser cannot be checked.
- D7.7 A source carries the form its value is held in (D6.12, built in POC 13f-iii, chosen within the owner's yes,
  2026-11-07), and who wrote the value decides its base form (gate 1's P8, decided within the owner's yes):
  - the app's form, the app's options' writing of its type: a response body, success or error (Q10), a pushed
    message (Q21), and data read past either;
  - the live form, what a live object holds or hands: a member of a component, plugin or element, a method's result,
    an event's payload, argument or callback argument, one declared as data included (D5.6), since the vendor's
    JavaScript wrote it, and data read past any of these; a computed value (a count, a sum, a comparison), which the
    browser computes as a JavaScript number or boolean (D6.11); and an object or list built from sources (N7.2).
  The live form is System.Text.Json's writing of the type with the app's naming policies and its contract (its
  resolver, a source-generated context's included) and nothing else the options set: every number a JSON number,
  every null member written, and no converter but the plan's own `DateTime` converter (D3.9)
  (`probe:core-stj-live-writer`, `probe:core-stj-live-keeps-naming`, `probe:core-stj-live-source-generated`). The
  names stay the app's, by the owner's line on JSON naming (`spec/CLAUDE.md`, 2026-11-06), so an app's own rows set into
  a grid and read back from its event keep their names. A null member is written because a `T?` write the type check
  takes (ALIS019) must reach the vendor, where an app's `WhenWritingNull` drops it. What a type states for itself
  holds in both forms: `[JsonPropertyName]`, `[JsonIgnore]`, a member's or class's `[JsonNumberHandling]`, a member's
  `[JsonConverter]`, and an enum type's own `[JsonConverter]` with its `[JsonStringEnumMemberName]`
  (`probe:core-stj-live-keeps-type-statements`). So an enum is its number in the live form unless its type carries a
  string converter, while in the app's form a converter the options add takes precedence over the type's
  (`probe:core-stj-options-converter-beats-type`).
  - A member's `Read`, a `Find`'s field and a `Select`'s list take the form of the member they read; `OrNull`,
    `Where`, `OrderBy` and `Find` pass their source's form on. A value written beside a source is written in its
    form: a condition's literal (D8.10), `IfNull`'s fallback and `Choose`'s literal arm (X7.12).
  - What a step writes into a live object (a property set, a method's or plugin's argument, a vendor's answer, an
    element's text and HTML, a render helper's start, D17.13) is in the live form. A literal is written in it, and a
    source in the live form as it is held. A source in the app's form is written as held where both forms write its
    type alike; a number the app's form writes as text is converted to the live form's number by `as` (D3.13,
    X7.14); an enum the two forms write otherwise is mapped through D9.13's rows, whose enum read D18.1 lists since
    gate 1 (C37); any other difference, a converter the
    app's options add or a member's own converter, is refused where the view renders (X7.10), and a value its own
    converter writes is shown by the server.
  - Two sources a choice or a comparison joins (`IfNull`, `Choose`, D8.10) are taken in one form by the same rules:
    a number the app's form writes as text is converted, an enum is mapped by D9.13's rows, and
    any other difference is refused (X7.11, X8.21). A date or a time a comparison joins is read by its key (D6.12),
    so a `DateTime` in the app's form meets a live one by its wall time.
  - A live number is a JavaScript number, so a literal written into one that JavaScript would read as another value
    is refused where the view renders, and by ALIS033 at build when it is a constant (X7.13): NaN and ±Infinity,
    which no JSON number holds (`probe:core-stj-live-nan`); a `decimal` of more than 15 significant digits, the
    digits every double carries back (`probe:js-number-fifteen-digits`); and a whole number past ±9007199254740991,
    `Number.MAX_SAFE_INTEGER` (`probe:js-number-safe-integer`). The rule refuses some longer literals JavaScript
    reads back (`12.345678901234567`), and in return it needs no double arithmetic, so it holds alike on both hosts.
    A number data holds reaches the vendor as the nearest double, as its `number` holds it.
  - A request sends a source to a member its endpoint binds by its own contract: a member with the same converter or
    number handling reads it back, so the request unit (Q9, the rewrite) compares the source's form, the live form
    for a live source, with the target member's and refuses a difference.
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
- `N7.3` new: a source overload on `Contains`, `StartsWith`, `EndsWith`, `MinLength`, `Between`, `In` and `NotIn`,
  module 3's tests (`M3.N1` in `spec/Conditions/module.md`).
- `N7.6` new, POC 13a: `NullableWrites.Set` on `Property<T?>` and `WriteOnly<T?>` taking a source of `T`, an
  extension in `Alis.Reactive.Components` beside the members it widens. The contract check makes every member a vendor
  types `T | null` a `T?` (Syncfusion's DropDownList types `value`, `text` and `index` so), and 178 writes of the POC
  would each have needed `OrNull()`. A view imports `Alis.Reactive.Components`; one that does not meets a bare CS1503
  that names no import.
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
- `X7.4` type (D7.5): a pattern read in the browser cannot reach `Matches`: new:N7.3 gives `Matches` no source overload.
- `X7.5` behavior (D4.9): a property set from a payload field the payload lacks writes `null`, and the component
  shows its empty state.
- `X7.8` behavior (D7.6): a pump card's class is added, removed, toggled on and toggled off, and a class name that
  reads nothing leaves the card's class as it was, where `classList.add(null)` adds "null"; HTML set on an element
  shows its markup, and HTML from a value that reads nothing shows nothing, where `innerHTML = undefined` shows
  "undefined".
- `X7.9` behavior (D7.3, N7.6): a census whose count is `int?`, as its TypeScript reads `number | null`, shows the
  count a step writes from an `int` sum or count.
- `X7.10` refusal (D7.7): a source in the app's form, or in its own form, that the live form writes otherwise, set
  into a property, passed as an argument or answered to a vendor: "A live object takes '{type}' as JavaScript holds
  it, and '{read}' is written by {its own JSON converter | the app's JSON converter}; pass the value from the server."
- `X7.11` refusal (D7.7): `IfNull` or `Choose` between two sources the two forms write otherwise: "A choice joins
  '{read}', which {cause} writes unlike {base}, with another value read from the page; choose between it and a value
  the view writes, or choose on the server."
- `X7.12` behavior (D7.7): `IfNull(DoseStatus.Given)` over a found chart that reads nothing compares equal to
  `DoseStatus.Given`, its fallback written as the member's own converter writes it, and so does `Choose`'s literal arm
  beside a found chart.
- `X7.13` refusal (D7.7): a literal written into a live number that JavaScript reads as another value: "'{member}'
  holds a JavaScript number, and {value} is {NaN or an infinity, which no JSON number holds | a decimal of more than 15
  significant digits | a whole number past 9007199254740991}, which JavaScript reads as another value; write a finite
  number of at most 15 significant digits." At build by ALIS033 for a constant: new:N18.10.
- `X7.14` behavior (D7.7): in an app whose options write numbers as text, a found supply's vial count of 5 is passed
  to a component's method that adds it to the 1 it holds and shows the total: it shows 6, where the text "5" would
  show "15"; set into the component's number member, it orders above 4.
- `X7.7` behavior (D7.3, N7.5): a fluid balance's intake widened to `int?` is written to its nullable target and
  ordered against it, over a 1000 ml target and within it, and an ordering with a null target does not hold, where
  JavaScript's `500 > null` does; a pump's summary widened to `string?` is passed as the caption's argument.
- `X7.6` behavior (N7.4): an alarm's note that holds null shows its fallback, a pump's summary, and one that holds
  a value shows it with no summary taken; a pump's last alarm that reads nothing passes its fallback as a method's
  argument; a condition compares a null weight's fallback and a held weight itself; a fluid target chooses its
  first value when the test holds and its second, a measured intake, when the test reads nothing, and measures
  nothing when the second is not chosen.
- `X7.15` analyzer (D7.3): a pump's last alarm with its fallback, a `string` source, passed to the caption's `string?`
  argument or compared with the pump's `string?` note, in an app that leaves CS8620 a warning: "'{source}' is a source
  of '{type}', and '{position}' takes '{type}?'; write .OrNull() to pass it as '{type}?'." At build by ALIS040:
  new:N18.14.
- `X7.16` type (D7.3): a literal at a condition operator's left operand does not compile: the comparisons are extension
  methods on a typed source, so `"North".Eq(wing)` finds no `Eq` on a string (CS1929): new:N8.2.

## Q8. Conditions: three kinds

**Decision.**
- D8.1 One condition core: the `Condition` nodes (comparison, text test, membership, value test, all, any,
  not), the operators of Q7 and the sources of Q7. The browser evaluates every pipeline condition, array
  predicate and validator condition with it (`Validation.tla:ConditionAgrees`).
- D8.2 Two doors write it. The pipeline door is the typed builder (`p.When(source)`, `ElseIf`, `Else`, `And`,
  `Or`, groups). The lambda door translates an expression tree with the one table
  `spec/Core/translated-members.md`; array predicates and validator conditions use it.
- D8.3 They differ in exactly these ways, by what each is:
  - The operands. A pipeline condition reads any source. An array predicate reads the item and values the
    view supplies. A validator condition reads the members of the body its request sends (Q16).
  - A term the lambda door cannot translate. In an array predicate it is refused where the view renders.
    In a validator condition it is refused (X16.13) unless `WhenOnServer` or `UnlessOnServer` declares it the
    server's, which then decides the rule by what it is (D16.8).
  - Order. A pipeline's `When`, `ElseIf` and `Else` run in authored order, and the first that holds wins.
    A validator condition gates its rules and has no branches.
- D8.4 Template conditionals are not this core. Syncfusion's template engine evaluates them on its own
  data, and Q15 states what that engine evaluates.
- D8.5 Every term reads by Q4: a term over a source that reads nothing does not hold, and a term with a
  method call runs only when it is reached (`Absence.tla:ConditionTermsLazy`).
- D8.6 A pipeline condition has one syntax: `When(source)`, `ElseIf(source)`, `And(source)` and `Or(source)`,
  where the source is any typed source of Q7, an event payload's member included (`When(args.ClosedBy)`).
  Main's second form, a payload and a path lambda (`When(args, x => x.IsExpanded)`), is refused, and
  `When(source)` carries it: the payload's member is a typed source (the owner, 2026-11-06).
- D8.7 A branch (`Then`, `Else`) is a block, as every stage is (D11.10): it holds any number of steps, written in one
  lambda, as a chain or as statements in a `{}` body, and runs them in authored order. An `if` holds one condition, its `then` block
  and its `else` block; an `ElseIf` is the `else` block of the `if` it continues, holding the results its condition
  called, at their lines, and an `if` of its own, as C# and JavaScript nest an `else if`. An `ElseIf` or an `Else`
  continues its `When` before any other step of the block, a method its condition does not read included (X8.9),
  and an `Else` ends its `When` (X8.18).
- D8.8 A block is a lexical scope, as a C# `{}` block is. A value a block binds is readable in that block and
  in the blocks inside it, and nowhere else. The values a block binds: its handler's event payload, a
  route's reply (its status, its headers, its body and the request as it was sent), the reply the block `Chained`
  runs receives, an array operation's item, a method's result read in it, and a `Confirm`'s answer. A block inside a
  route or a `Chained` block reads every reply around it, so a chained request sends the first reply's body and the
  first request's members (`spec/api/HttpSurface/EveryStage.cs`). A read outside its block does not compile where it
  names the block's parameter (CS0103, `ReplyOutsideItsRoute` in `spec/api/refused.sh`); a bound value carried out
  through a variable declared outside its block is the block analyzer's (N18.9), and the render refuses it too until
  N18.9 is built; one carried out any other way (a field, a collection, a helper that stores it) is refused where the
  view renders, naming the value and the block that binds it, since only the render sees where it went. That one rule
  covers today's two refusals of this kind, an array item and a response read outside their blocks, and every value
  a block binds by a name of its own: a reply in its routes, a method's result in its block, a list's item in its
  lambda, a Listen's payload in its Listen and a callback's arguments in its answer, at whatever depth a step's value
  reads it. The browser runs each block in a child scope, so a value bound inside a branch
  never reaches the steps after it.
- D8.9 One meaning of comparison and logic, which every condition reads (D8.1). A comparison (`eq`, `neq`, `lt`,
  `lte`, `gt`, `gte`) whose operand reads nothing reads nothing, `neq` included: a value past a holder that holds
  null holds none, and no comparison with it holds (D8.5). Null is a value the data holds, compared as .NET's lifted operators
  compare it: `eq` of null and null holds, `neq` of null and a value holds, and an ordering with null does not hold,
  though JavaScript's `null <= 2` is true. Equality is JavaScript's `===`, over a decimal written as text `eqDecimal`,
  by value, and over a date or a time `===` of its keys (D6.12); an ordering is JavaScript's `<` over the numbers and
  one-character texts D6.12 admits and over the keys of a date or a time. `not`, `and` and `or` are SQL's three-valued
  logic with nothing as its unknown: `not` of nothing is nothing; `and` is false when a term is false, true when both
  are true, and nothing otherwise; `or` is true when a term is true, false when both are false, and nothing otherwise.
  A term runs only when it is reached (D4.7): `and` reads its right term unless its left is false, `or` unless its
  left is true. A condition holds only when it reads true: a branch, an array predicate (`filter`, `some`, `every`,
  `find`) and a validator condition. So `d.Carer.Name != "Ruth"` and `!(d.Carer.Name == "Ruth")` agree over a dose
  whose carer is null: neither holds, where .NET throws. A member the item lacks reads its start first (D4.2):
  `d.Ward != "North"` holds over a dose the app wrote with no `ward`, as it holds over the null the endpoint binds for
  it.
- D8.10 A pipeline condition over a source reads it in its form, the app's or the live form (D7.7), and over a source
  in its own form follows D6.12's rules (POC 13f-iii); two sources in the two base forms meet by D7.7's rules: a literal it is
  compared with by `Eq` or `NotEq` is written in its form, and an ordering (X8.19), a type that is not a scalar (X8.20),
  two sources where either is in its own form (X8.21), and a `float` or `double` written as text or a `decimal` its own
  converter writes as text (X8.22) are refused. A `decimal` the options or number handling write as text is compared
  by `Eq` and `NotEq` through `eqDecimal`, by value (D6.12, X8.26). A date or a time compares by its key (D6.12), so a
  `DateTime` in the app's form meets a live one by its wall time, and the redesign's date pages hold as their tests
  expect (X8.27).

**Schema nodes.** `op` (the comparison, text, membership and value tests, the date keys, `and`, `or`, `not`); `if`.

**Checker.**
- `Absence.tla:ConditionTermsLazy`
- `Schema.tla:DatesComparedByKey`
- `Schema.tla:KeysOnlyCompared`
- `Schema.tla:KeyFallbackIsALiteral`
- `probe:core-implicit-instant-server-clock`
- `probe:net48-implicit-instant-local-offset`
- `check:fluent`

**Doors.**
- kept: the pipeline door, `When`, `ElseIf` and `Else` (`Alis.Reactive/PlanAuthoring/Conditions/ConditionExtensions.cs:17`);
  `ConditionSourceBuilder` and `ConditionOperators` go to N8.2's comparisons and module 3's tests (`M3.N1`).
- `N8.2` new, its form gate 2's: the comparisons (`Eq`, `NotEq`, `Gt`, `Gte`, `Lt`, `Lte`) and `And`, `Or` and `Not`
  are extension methods on a typed source that return a `TypedSource<bool>`, which `When` and `ElseIf` take and a live
  list's predicate returns, so one comparison API serves both. Each refuses where the view renders what the lambda
  door refuses for its type: objects compared by reference, an ordering of what the browser cannot order as .NET
  does, and a date or a time a converter writes (X6.57, X8.22); a date or a time compares by its key (D6.12). A branch
  writes an `if` (POC 9a).
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
- `X8.1` type (D8.2): a condition on an operand of the wrong type does not compile: the operator is generic over
  the source's type (`Alis.Reactive/PlanAuthoring/Conditions/ConditionOperators.cs:16`).
- `X8.2` analyzer (D8.2): an array predicate with a term the lambda door cannot translate: "{term} has no browser form
  in an array predicate; use a member the framework translates, or move the test to the server." At build by ALIS033, and where the view renders on code it does not follow (D18.6): new:N18.10.
- `X8.3` behavior (D16.9): a validator condition `WhenOnServer` declares, with a term the lambda door cannot translate,
  gates its rules on the server only; the browser sends the request and shows the server's message.
- `X8.4` behavior: `When(a).ElseIf(b).Else()` runs the first branch whose condition holds and no other.
- `X8.5` analyzer: a response body captured in a C# variable inside `OnSuccess` and read after the request is
  refused with "The response body of the request to {url} is read outside the block that binds it: read it
  inside that request's routes or the blocks they hold." At build by ALIS032, and where the view renders on code it does not follow (D18.6): new:N18.9.
- `X8.6` behavior: a branch with three steps in a `{}` body runs all three, in order, and none when its
  condition does not hold.
- `X8.7` behavior (D8.1): over a dose that lacks `ward`, whose start is null, `d.Ward != "North"` and `!(d.Ward == "North")`
  both keep it (D4.2);
  a dose whose `priority` holds null is kept by `d.Priority != 2` and left out by `d.Priority <= 2` and
  `d.Priority < 2`, as .NET's lifted operators decide (D8.9).
- `X8.8` behavior (D8.5): `or` keeps an item whose left term reads nothing when its right term holds, and `and` whose left
  term reads nothing and right term is false reads false, so `not` of it holds (D8.9).
- `X8.9` analyzer (D8.7): an `ElseIf` or `Else` written after another step of its block, or after a method its
  condition does not read: "An ElseIf or an Else continues the When it follows, before any other step of its block:
  write it in the same sentence." At build by ALIS032, and where the view renders on code it does not follow (D18.6): new:N18.9.
- `X8.10` analyzer (N8.2): a pipeline condition that orders flags or compares objects, each with the reason the
  lambda door gives for it. At build by ALIS033, and where the view renders on code it does not follow (D18.6): new:N18.10.
- `X8.11` behavior (D11.5): the steps after a branch that awaits run once its method settles, and at once when the
  branch is not taken.
- `X8.12` behavior (D8.9, D8.1): a branch whose condition orders a null runs none of its steps, where JavaScript's
  `null < 80` holds.
- `X8.13` behavior (D4.7, D8.5): a method called in the right term of `Or` runs only when the left term does not hold, and
  in the right term of `And` only when the left term holds.
- `X8.14` behavior (D4.7, D8.7): a method called in an `ElseIf`'s condition runs only when no branch before it ran,
  so a second `ElseIf`'s does not run when the first holds; an async one is awaited there, and its branch runs once it
  settles.
- `X8.15` behavior (D5.11): a fluid balance measured, then set to 999 ml, and read in a term reached after the set,
  is the 100 ml measured at its line; a result a term and its branch both read is the call at its line, so a branch
  that sets 999 ml and then the result restores the 600 ml measured.
- `X8.16` analyzer (D4.7): an async method called under a term reached later, the right term of `Or` or the fallback
  of `IfNull`: "An async method is called in a term that runs only when it is reached (the right term of And, Or or
  IfNull, or a branch of Choose), and the pipeline waits for an async method at its line: call it in a When of its
  own, in the branch where its term is reached." At build by ALIS032, and where the view renders on code it does not follow (D18.6): new:N18.9.
- `X8.17` analyzer (D8.8): a result called in an `ElseIf`'s condition, read after its `When`: "A method's result
  called in an ElseIf's condition is read after its When, and the call runs only when the ElseIf is reached: call
  the method before the When to read its result after it." At build by ALIS032, and where the view renders on code it does not follow (D18.6): new:N18.9.
- `X8.18` analyzer (D8.7): a second `Else`, or an `ElseIf` after the `Else`, of one `When`: "An Else ends its When:
  write each ElseIf before it, and every step of the remaining case in that one Else." At build by ALIS032, and where the view renders on code it does not follow (D18.6): new:N18.9.
- `X8.19` refusal (D8.10): an ordering of a source in its own form: "A condition orders '{read}', which {cause} writes
  unlike {base}, so the browser does not order it as .NET does; order it on the server."
- `X8.20` refusal (D8.10): a source of a type that is not a scalar, in its own converter's form, compared with a
  value: "A condition compares '{read}', which {cause} writes unlike {base}, and .NET compares the object
  it holds; compare it on the server."
- `X8.21` refusal (D8.10): two sources compared where either is in its own form: "A condition compares two values,
  and {cause} writes '{read}' unlike {base}, so the browser does not compare them as .NET does; compare
  them on the server."
- `X8.22` refusal (D8.10): a `float` or `double` the JSON writes as text, a `decimal` its own converter writes as
  text, or a date or a time its own converter writes, compared with a value: "A condition compares {a float or double
  the JSON writes as text, whose text does not equal as the number does, as -0 and 0 or NaN and NaN; a decimal its own
  converter writes as text, whose text the converter decides; a date or a time its own converter writes, whose text
  the converter decides}; compare it on the server."
- `X8.23` behavior (D8.10): a found chart and a component's data read to `Charted` equal `DoseStatus.Given` as the
  member's own converter writes it, "Given" where the app writes "given".
- `X8.26` behavior (D8.10, gate 1's P15): `When(cart.Doses.Find(d => d.Medicine == "Aspirin", d => d.Copay).Eq(0.10m))`
  runs its `Then` for an aspirin dose whose copay, written as text by its own number handling, holds "0.1", and its
  `Else` for one that holds "0.125"; `NotEq(0.10m)` runs the other branch of each. The POC refuses the condition
  (`tests/Alis.Reactive.Poc.PlaywrightTests/Comparisons/WhenASourceInItsOwnJsonIsRefused.cs:34-39`) until the rewrite
  builds `eqDecimal`, whose unit turns that test into this one.
- `X8.27` behavior (D8.9, D8.10): the redesign's 16 date tests hold as they expect at 65fdf118d:
  `WhenAPageComparesADayThatMayBeMissing`'s 10 over the Visit page (a day picker's `DateTime?` against the open day by
  `IsNull`, `NotNull`, each comparison and `Between`), `WhenAPageComparesTwoDaysItReads`'s 4 over the Stay page (two
  picked days compared) and `WhenAPageOrdersATimeOrAFlag`'s 2 time tests over the Shifts page (a `TimeOnly` from the
  page's query against noon), these two net10's alone, since net48 has no `TimeOnly`. Beyond them, `?starts=12:00`
  equals `new TimeOnly(12, 0)` where `?starts=12:00:00.0000001` does not, and a response's 2026-11-09T10:00:00-08:00
  equals `new DateTimeOffset(2026, 11, 9, 18, 0, 0, TimeSpan.Zero)`. Two outcomes of the redesign's own tests change:
  that file's flag test, `Gt(false)`, is refused, since C# has no `<` on `bool` (module 3), and the SpanOrder page,
  which the redesign refuses (`WhenABranchIsWrittenWrongly.an_order_of_a_length_of_time_is_refused`,
  `Alis.Reactive/Members/BrowserOrder.cs:10-15`), orders a `TimeSpan` by its key.
- `X8.24` type (D8.8): a route's reply read after its request, outside the route, does not compile:
  `spec/api/HttpSurface/refused/ReplyOutsideItsRoute.CS0103.cs.txt:13`.
- `X8.25` analyzer (D11.10, D8.8): a step written on the pipeline of a block around its own, and a bound value
  carried out of its block through a variable declared outside it: new:N18.9.
- `X8.28` type (D8.6): a pipeline condition written as a payload and a path lambda, `When(args, x => x.IsExpanded)`,
  does not compile: `When` takes one `TypedSource<bool>`, and the payload's member is a source of its own: new:N8.2.

## Q9. The request

**Decision.**
- D9.1 A request is one statement, `request` (D10.23), whose `send` is an expression whose value is the typed
  contract `HttpSend`, generated into the plan's TypeScript beside the grammar. The browser declares no request type
  of its own. The grammar gains this one node, by the owner's amendment of G2 (2026-11-08): a request's lifecycle is a
  state machine whose state, the busy targets it holds from its guards to the end of its defaults' `Finally`, does not
  nest in `let`, `try` and `if` (`spec/NOTES.md`, 2026-11-08).
- D9.2 The builder is staged: each part is written at most once, a GET has no body format, and every door that
  sends inputs names what it sends (D9.15): no door sends the inputs of a form the request is taken to check.
- D9.3 The method is GET, HEAD, OPTIONS, POST, PUT or DELETE. GET, HEAD and OPTIONS send no body; POST, PUT and
  DELETE send one. PATCH is not a method of 1.0 (spec/CLAUDE.md, the owner, 2026-11-07). An action answers HEAD only
  under `[HttpHead]` and OPTIONS only under `[HttpOptions]`, on both hosts: net10 answers HEAD to an `[HttpGet]`
  action 405 (`probe:core-head-on-get-action`) and MVC 5 selects an action by its verbs alone
  (`probe:mvc5-accept-verbs-exact`, `probe:mvc5-http-head-attribute`, `probe:mvc5-http-options-attribute`). The app's
  OPTIONS reaches its action and its `Allow` header; a preflight, which carries `Access-Control-Request-Method`, is
  the CORS middleware's (`probe:core-options-reaches-action`).
- D9.4 The URL is parsed once, in C#, into text parts and placeholders. A URL that holds `#`, a brace without
  its pair, or a placeholder no member fills is refused where the view renders. A placeholder is filled by
  the top-level member of the same name of the request or of its query, compared ignoring case. A list, an object, a file, a date
  or an instant in a placeholder is refused, on both hosts, because IIS refuses a time in a path segment on
  net48 (#237). A path value is sent as its invariant text, encoded as one path segment. `Path` names its placeholder
  by its member, so a `Path` whose member names no placeholder or belongs to an object the request holds, and a
  placeholder that only `Query` names, are refused where the view renders. `Query` and `Set` take any member path,
  `r => r.NextOfKin.Name`, keyed by its C# path and written with no `!` past a holder that holds null (N9.5), and `Set` places a member of the request itself named like a
  placeholder in the path, every other in the query; a member lambda that is not a member path is refused.
- D9.5 A path value that reads nothing or reads empty text stops its request before it is sent (D4.6).
- D9.6 Every method sends a query. A request with no body sends there every member that fills no placeholder;
  POST, PUT and DELETE send there the members of a typed query, `Query<TQuery>(q => q.Set(..))`, beside the body.
  A member is static (a literal) or read from a source. The URL's own query text is kept, and C# appends the
  members after it with `?` or `&`. A key is the binding name, a list sends one key per item, a number is its
  invariant text and a date is the wire form of `spec/PathsAndWire/DateWire.tla`.
- D9.7 A JSON body is one object. Its member names are Q2's, its nesting follows Q1's steps, and a dictionary
  entry is a member named by its key. A JSON request that sets nothing sends `{}`, because on net10 a request
  with no body answers 415 or binds null by the endpoint parameter's nullability (`probe:core-frombody-no-body`).
- D9.8 Form data is multipart, written by the browser: an ordered list of parts, each a name and either text or a file
  with its name and media type. A field is named by its binding name, a list sends one part per item, a number
  carries the decimal separator of the view's culture (D3.7) and a file is sent as the file. Fields and files go in
  one body: several files under one name, and files nested by C# path beside their fields (`Documents[0].File`,
  `Documents[0].Caption`), bind on both hosts (`probe:core-multipart-files-and-fields`,
  `probe:mvc5-files-keyed-by-part-name`). A file's name is sent in UTF-8 with a double quote percent-encoded
  (`probe:fetch-multipart-file-name`), and binds as written (`probe:core-multipart-utf8-file-name`).
  A body past the host's limit is refused before the action runs: System.Web's default limit is 4 MB and it throws
  before MVC (`probe:mvc5-max-request-length`, `probe:mvc5-request-too-large-throws`), and past Kestrel's default
  30,000,000 bytes a client reads no response (`probe:core-upload-over-limit`). Either answer runs exactly one route
  (D10.5): a status the host writes picks its route, and a connection the host closes is a network failure, the
  catch-all's. The app sets the limit in its host's configuration.
- D9.9 DELETE sends its body as POST and PUT do. A member that fills a placeholder is also sent in the body of a
  POST, PUT or DELETE: a path value binds into a member of a model bound from the form, but never into a
  `[FromBody]` model (`probe:core-route-value-binds-member`), and on both hosts a value the body sends wins over the
  route's (`probe:mvc5-value-provider-order`), so the body carries the same value the path does.
- D9.10 A member that reads nothing is sent by this closed table, which C# writes into the plan:

  | The member's type | JSON body | Query or form data |
  |-------------------|-----------|--------------------|
  | a non-nullable value type | left out | left out |
  | a list | left out | left out |
  | an object | `null` | left out |
  | a nullable value type or text | `null` | an empty field, `name=` |

  A `DateTime` and a `DateTimeOffset` are value types, and their nullable forms nullable value types (D3.8).
  A member left out keeps its starting value; a JSON `null` for a non-nullable value type binds no instance
  at all, and an empty field for one is a model error (`probe:core-json-null-member`, `probe:core-form-empty-field`).
  An object is left out of a query or form data because no field binds it as nothing on both hosts: MVC 5 binds a
  value at the member's own key as one value and converts an empty one to null (`probe:mvc5-value-at-member-key-simple`,
  `probe:mvc5-empty-value-converts-null`, `probe:mvc5-null-sets-reference-member`), while net10 keeps the member's
  start, an instance included (`probe:core-query-object-empty-field`). Left out, it binds its start on both. A JSON
  `null` binds null over the start (`probe:core-json-null-object`).

  A list item that reads nothing, in a query or form data, is sent by this closed table, which C# writes into the
  plan from the item type the app's JSON contract names (D18.2), a nullable enum among the nullable value types:

  | The item's type | Query or form data |
  |-----------------|--------------------|
  | a nullable value type or text | an empty field, which binds a null item (`probe:core-query-nullable-enum-items`, `probe:mvc5-empty-list-item-null`) |
  | a non-nullable value type | the request stops, since no field binds back as nothing |
  | an object | the request stops: an empty field ends the list at that item on net10 (`probe:core-query-list-of-objects-empty-item`) and binds a null item on MVC 5, and an item left out ends the list there on both hosts (`probe:core-query-list-of-objects-gap`, `probe:mvc5-list-ends-at-missing-index`) |
- D9.11 A header is fixed text or a scalar source sent as text. A header that reads nothing is not sent. A
  header name is an HTTP token by RFC 9110, a name given twice ignoring case is refused, `Content-Type` is
  refused because the body format owns it, and `Accept` is refused because the success route's kind owns it (D10.6).
  The headers the framework writes are refused too: `RequestVerificationToken` (D9.22) and `traceparent`, `tracestate`
  and `baggage` (D9.12, D20.11). So are the Fetch Standard's forbidden request headers, which fetch drops without a
  word: its named list, any name under `Proxy-` or `Sec-`, and `X-HTTP-Method`, `X-HTTP-Method-Override` and
  `X-Method-Override`, which the Standard forbids when they name CONNECT, TRACE or TRACK and which the request's verb
  makes needless whatever they name. Each is refused where the view renders, a list or an object source too.
- D9.12 The headers every request sends are configured once in C#, and C# writes them into each request's
  `HttpSend` where the view renders; a request's own header of the same name, compared ignoring case, replaces
  the configured one. An app that sends `X-Requested-With: XMLHttpRequest` on every request configures it there.
  The browser adds no header but W3C Trace Context's `traceparent` and `tracestate` while the plan's trace is on
  (D20.3), and `traceparent` and `baggage` on a retry (D20.11), each to the page's own origin only.
- D9.13 A literal member, `Set(member, value)`, is written where the view renders, encoded as the endpoint binds it.
  In a query, form data, a path segment or a header an enum is its C# name on both hosts, which net10 and MVC 5 bind
  ignoring case; the name `JsonStringEnumMemberName` gives it binds nothing there
  (`probe:core-query-enum-member-name`, `probe:mvc5-enum-name-ignores-case`). C# writes a literal enum by its C# name,
  `Enum`'s own text for it, so a flags combination is its comma form, smallest first, which both hosts bind
  (`probe:core-query-flags-enum`, `probe:mvc5-enum-flags-comma`), whether or not the startup names the enum: C# holds
  a literal's value, so it needs no rows, and the `FlagsAttribute` read inside `Enum.ToString` is .NET's, as
  System.Text.Json's reflection is its own (D18.1, whose list holds `Enum.ToString` since gate 1, C37). A literal whose text is
  digits, a value no value and no combination names, is refused where the view renders (X9.22). An enum a source holds
  is in its held form (D4.2): the form its member's own `[JsonConverter]` writes, else its base form, the app's or the
  live form (D7.7). So C#
  writes into the plan, beside the member, a row for each value `Enum.GetValues` lists (D18.1, C37): the value as
  System.Text.Json writes it at the source's type (a `T?` when the source is one, since a nullable member's own
  converter writes names only there, `probe:core-json-enum-nullable-member-converter`) under the source's held form,
  and its C# name. The browser sends the C# name of the row whose form it holds. A held form no row lists stops the
  request, naming the member; a combination of an enum the app does not name as flags is among them (X9.19). Two
  values written in one form the browser cannot tell apart (`probe:core-json-enum-held-alike`), and a value written as
  anything but a JSON text, number or boolean, are refused where the view renders. In a JSON body an enum is the form
  its endpoint binds: on net10 the request member's System.Text.Json form under MVC's `JsonOptions`
  (`probe:core-frombody-enum-forms`), on net48 its C# name, since MVC 5 binds a JSON body through its model binder
  (`probe:mvc5-json-binds-through-binder`); C# writes the rows from the held form to it, with the JSON body's door.
  An enum the app names as flags (N16.4, gate 1's P2) is sent by the wire form `flags`:
  - C# writes each row with the value's number too, the largest number first, the order in which `Enum.ToString`
    splits a flags value.
  - The browser reads what the source holds as one number: a held text is split at ", ", each part the held form of a
    row, and is the union of the parts' numbers; a held number, under rows held as numbers, is itself. It splits that
    number as `Enum.ToString` does, taking in order each row whose bits all lie in what is left until nothing is, and
    zero as the row whose number is zero, and sends the C# names it took, the smallest first, joined by ", ":
    `Enum.ToString`'s own text for the value, which both hosts bind (`probe:core-query-flags-closure`,
    `probe:mvc5-enum-flags-comma`). So a held "Weekend, Mon" or "Mon, Sat, Sun" under a string converter, and a held 97
    under the app's numbers, are each sent as `Mon, Weekend`.
  - What the split cannot read stops the request, naming the member: a part no row holds, a held number under rows
    held as text and a held text under rows held as numbers, a held number that is no integer from 0 to
    9,007,199,254,740,991, bits no row takes, and zero with no row for zero. net10 binds nothing for a bit no value
    names or for zero with no value for zero (`probe:core-query-flags-closure`), and System.Text.Json writes either
    as its number under every form (`probe:core-json-flags-forms`).
  - The split at ", " reads every held text System.Text.Json writes, since it joins a combination's held forms by ", "
    (`probe:core-json-flags-forms`) and refuses a flags enum whose member name holds a comma
    (`probe:core-json-flags-comma-name`).
  - The browser tests bits on exact integers, since JavaScript's `&` reads 32 bits (`probe:js-bitwise-int32`), and
    ALIS038 admits only an enum whose values are from 0 to 9,007,199,254,740,991 and pairwise disjoint or one inside
    the other (N16.5): JavaScript holds no larger integer exactly, and a negative value's bits are its underlying
    type's, which a held number does not carry. Over such an enum the split takes a value exactly when
    FluentValidation's `IsInEnum` passes it and net10 binds it, since the largest value inside what is left holds every
    value of a union it shares a bit with, so taking it leaves a union of the rest (`probe:core-flags-split-agrees`
    checks it on every value of four such enums, and shows `X = 5, Y = 6, Z = 3` parting at 7).
  - A literal is `Enum.ToString`'s text for its value, which is this split's text, so a literal and a source holding one
    value send one text where both apply (`probe:core-flags-split-agrees`). A list's items are split one by one, and in
    a JSON body the split value is written in the form its endpoint binds, with the JSON body's door.
- D9.14 A hidden input is an input: it binds by its binding name and shape, and a request sends it when the
  container it sends holds it (D9.15).
- D9.15 What a request sends is closed, and holds on a page with many forms or none (the owner, 2026-11-08):
  - `Set(r => r.Member, value)`: a member from any source (Q7), a response's body included.
  - `Form(form)`: a native `<form>`'s submission set as the browser builds it, `new FormData(form)`: every
    form-associated control the form owns, one outside its element through the `form` attribute included,
    hand-written inputs and file inputs among them, each under its `name`. It is sent only as form data
    (URL-encoded or multipart) or as a GET's query, since a submission set is form data. In form-field mode the
    framework's antiforgery field replaces any the form holds (`FormData.set`, D9.22), so the endpoint reads one.
    Each framework input the form owns replaces its name's entries with its wire form (`FormData.set`), so the body
    the check reads is the body sent; a hand-written control is sent as the browser holds it, and its failures show
    in the container's summary only (D10.18).
  - `Inputs(container)`: the framework inputs inside one element of any kind (a form, a fieldset, a table row, a
    drawer's body), rendered by any plan, nested elements included, each under its binding name, in any body.
  - `Input(r => r.Member, pipeline.Input(m => m.Member))`: one input of the request's own view, wherever it sits,
    sent under the request member's binding name. The value type both lambdas share makes a member of another type
    not compile (X9.3).
  - A request sends at most one container, through `Form` or `Inputs`, and that container is the request's: its
    summary is where the request's failures show (D16.13). The types hold it: the full form's body door takes the
    container as its first argument (`Json(container, body => ..)`, `UrlEncoded(form, body => ..)`), a GET's and the
    condensed `Form` and `Inputs` return a stage that offers no second, and no body offers either
    (`SecondContainerOnABody`, `SecondContainerOnGet`, `ContainerInsideABody` in `spec/api/refused.sh`), in the chain; a
    parts object two statements write is ALIS030's (N18.7). A container is typed by its model, and a request takes
    only a container of its own model (N18.3).
  - When an input and a set member send the same binding name, the input's value is sent, as the body the check
    reads (Q16); two inputs that send one scalar binding name are D13.4's.
  - A container whose element the page lacks stops the request, and so does an `Input` whose input the page lacks.
  - The run records each input it sent, under the binding name it sent it by (D10.18).
- D9.16 A file is the shape `file`, sent only in form data or as the whole body. A set member of file shape in a
  request with no body, a JSON, URL-encoded or text body
  is refused by an analyzer; a file input a request sends in a GET or a JSON body stops the request when it is sent
  (`Request.tla:FileOnlyInFormData`).
- D9.17 A block holds one request at its own level. A request in a block inside it (a branch, a route, `Finally`,
  a `Parallel` branch) is that block's, and requests in nested blocks run one after another, each after the one
  before it settles (D11.5). ALIS008 keeps refusing a second request in one block, retargeted to the verbs of
  `Pipeline<TModel>` in every stage, both lambda forms and every file (N9.9), with its reason corrected: `Chained`
  runs a request after another succeeds, `Parallel` sends them at once, and a request in `Finally` runs after every
  outcome.
- D9.18 `pipeline.PageUrl<TPage>()` (N10.7) reads the page's own URL. It is a source (Q7), not a part of the request (M2.X1, M2.X3).
- D9.19 A member whose source holds an object or a list of objects is sent in a query or in form data as its
  fields, each keyed by its C# path (`Address.City`, `Lines[0].Name`, and a list of scalars inside an item as the
  repeated `Lines[0].Beds`) and read from the source by its JSON name (D2.6), as the binder reads them
  (`probe:core-form-binds-indexed-name`, `probe:core-query-object-fields`, `probe:core-query-list-of-objects`,
  `probe:mvc5-sub-property-name`, `probe:mvc5-sub-index-name`); in a JSON body it is nested (D9.7). A field that reads
  nothing follows D9.10, and a list of objects is indexed from 0 with no gap. An object's fields are the members the
  generated table lists (D9.20); a field the app's JSON options do not write is never in the browser and is left out,
  so the endpoint binds its start. A type that holds itself, at any depth, is refused where the view renders, since
  its keys have no bound. A component's member, a method's result, a page URL member, a hidden input and a literal
  fill a member the same way (Q7).
- D9.20 What an endpoint binds is read at build, never at run time (D18.1). A source generator in the analyzers
  package walks the complex parameters of each controller action and every type reachable from them, and writes,
  for each type, the members the binder fills by their C# name as typed lambdas, and for each enum its values, in C#
  7.3 syntax, so it compiles in an MVC 5 app at any language version the owner assumes (C# 8). The app hands the
  tables to the framework with one startup call, the same on both hosts (the owner, 2026-11-07). The actions are the public methods of a net10
  `Microsoft.AspNetCore.Mvc.ControllerBase` and of a net48 `System.Web.Mvc.Controller`, less `[NonAction]`; a net10
  parameter bound `[FromBody]`, `[FromServices]`, `[FromKeyedServices]` or `[FromHeader]` is not walked, nor a complex
  parameter with no binding attribute under `[ApiController]` on its class, a base class or its assembly, which net10
  infers `[FromBody]`. The core names each field by its lambda's expression text (D2.5) and reads it by `JsonPath`
  (D2.6), and reads a list item's type from the lambda's type argument, never from a type's members.
  - Two assemblies whose controllers reach one type each write its table. A second table is accepted when it lists the
    same fields with the same marks, and any difference is refused where a view first reads the type, naming it.
  - A request type with no table is refused where the view renders, naming its cause and cure.
  - A list member is sent only when declared `List<T>`, the one declared type both binders fill whatever it starts
    with: net10 binds nothing into a read-only list a member starts with (`probe:core-query-read-only-lists`), and
    MVC 5 creates a `List<T>` only for `IEnumerable<>`, `ICollection<>` and `IList<>` and clears the collection a
    member starts with, which an array refuses (`probe:mvc5-list-interfaces-created`,
    `probe:mvc5-repeated-keys-new-collection`, `probe:mvc5-list-cleared-and-refilled`).
  - The table marks a member of another list type, a member net10 binds under another name and a `[BindNever]`
    member (`probe:core-query-member-names`). Refused where the view renders, each naming its cure: a request type
    the table lacks (no action takes it or a type that holds it), a set of a marked member, and an object sent whose
    type holds a marked list or a renamed member. A `[BindNever]` field of a sent object is left out, since the
    endpoint binds it under no name.
  - The keys carry no parameter prefix. A parameter with no binding name binds them when no key is named like it
    (`probe:core-query-parameter-prefix`, `probe:mvc5-prefix-exact-key`); a parameter that names a prefix
    (`[Bind(Prefix)]`, net10's `[FromQuery(Name)]`) binds only prefixed keys (`probe:core-query-bind-prefix`,
    `probe:core-query-parameter-name`, `probe:mvc5-parameter-binding-name`, `probe:mvc5-prefix-no-fallback`), and a
    parameter named like one of its type's members binds none of them (`probe:core-query-parameter-name-collision`).
    Both are per action, and a view names its action only by URL text, so no build check refuses them, since none links a
    request to its action; gate 1 accepted both as MVC's binding, which the request door's XML doc names (C25).

- D9.21 The body formats are closed: none, JSON (D9.7), URL-encoded, multipart (D9.8), text and one file. Each
  sets its own `Content-Type` as fetch writes it: `application/json`, `application/x-www-form-urlencoded;charset=UTF-8`
  from `URLSearchParams` (`probe:fetch-urlencoded-body`), `multipart/form-data` with the browser's boundary,
  `text/plain;charset=UTF-8` from a string and a file's own type from a `Blob` (`probe:fetch-body-media-types`). A
  URL-encoded body binds as multipart does on both hosts, and an action under `[Consumes]` takes its own format
  alone: net10 answers a multipart body to a URL-encoded action 415 (`probe:core-consumes-urlencoded`). Its fields are
  named and written as the query's (D9.23). A text body and a file body are one value each, read by the action
  from its body, since no binder on either host reads them.
- D9.22 Antiforgery is configuration every unsafe request to the page's origin carries, not a default: a request that
  takes no defaults (D10.12) still sends it, since the server refuses a POST without it. The app chooses its place, or
  `NoAntiforgery()`, once; a view that renders a POST, PUT or DELETE in an app that made no choice is refused, naming
  both doors. C# writes the token into the plan where the view renders, from the host's own antiforgery
  (`IAntiforgery.GetAndStoreTokens` on net10, `AntiForgery.GetTokens` on net48), and every POST, PUT and DELETE to the
  page's origin sends it, the same token on every attempt; GET, HEAD and OPTIONS never do, as neither host asks a
  token of them (`probe:core-antiforgery-safe-methods`). A partial loaded into a slot carries the token its own GET was
  rendered with, a new one that validates against the page's cookie as the first does
  (`probe:core-antiforgery-later-token`). Its place:
  - the header `RequestVerificationToken`, which net10 reads before the form and with every body
    (`probe:core-antiforgery-names`, `probe:core-antiforgery-placement`);
  - or the field `__RequestVerificationToken` of a URL-encoded or multipart body, which both hosts read
    (`probe:mvc5-antiforgery-field-name`). A JSON, text or file body under the field place is refused where the view
    renders, since no host reads a token there (`probe:core-antiforgery-placement`). A request that sends no body
    under the field place sends a URL-encoded body holding the field alone, which MVC 5 reads for PUT and DELETE as
    for POST (`probe:mvc5-form-filled-any-method`).
  MVC 5's `[ValidateAntiForgeryToken]` reads the form alone (`probe:mvc5-antiforgery-reads-form-only`), so on net48 the
  header is read only by a filter that hands it to `AntiForgery.Validate(cookieToken, formToken)`
  (`probe:mvc5-antiforgery-validate-tokens`). The framework ships that filter, `[ValidateAntiForgeryHeader]`,
  in the net48 host (gate 1, C10).
- D9.23 A request's URL, query and headers follow their standards, and C# writes each part where the view renders.
  - The URL is an RFC 3986 URI reference: a path on the page's origin, or an absolute `http` or `https` URI, split
    into the parts of RFC 3986, section 3. Its path is segments, its query is the query, and a fragment is refused
    (D9.4).
  - A path value fills one path segment, percent-encoded as UTF-8 outside RFC 3986's unreserved characters
    (`probe:js-path-segment-encoding`). A path value whose text holds `/` stops its request, as one that reads
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
**Schema nodes.** `request`, its `send` a `record`, `list`, `lit`, `get`, `as` and `op`; the shape `file`; the
contract `HttpSend`, which is no node kind, and its wire forms, `enum` and `flags` among them (D9.13).

**Checker.**
- `probe:core-frombody-enum-forms`
- `probe:core-json-enum-nullable-member-converter`
- `probe:core-json-enum-held-alike`
- `probe:core-json-flags-forms`
- `probe:core-json-flags-comma-name`
- `probe:core-query-flags-closure`
- `probe:core-flags-split-agrees`
- `probe:js-bitwise-int32`
- `Request.tla:EveryPlaceholderFilled`
- `Request.tla:PathHoldsScalarText`
- `Request.tla:FileOnlyInFormData`
- `Request.tla:NothingBindsAsDeclared`
- `Request.tla:ObjectNothingBindsAsDeclared`
- `Request.tla:ItemNothingBindsAsDeclared`
- `probe:core-json-null-member`
- `probe:core-form-empty-field`
- `probe:core-frombody-no-body`
- `probe:core-frombody-binds-json-name`

- `probe:js-path-segment-encoding`
- `probe:core-route-segment-decoding`
- `probe:js-query-encoding`
- `probe:core-query-decoding`
- `probe:net48-query-plus-space`
- `probe:js-header-values`
- `probe:js-header-trim`
- `probe:core-head-on-get-action`
- `probe:mvc5-accept-verbs-exact`
- `probe:mvc5-http-head-attribute`
- `probe:mvc5-http-options-attribute`
- `probe:core-options-reaches-action`
- `probe:core-multipart-files-and-fields`
- `probe:mvc5-files-keyed-by-part-name`
- `probe:fetch-multipart-file-name`
- `probe:core-multipart-utf8-file-name`
- `probe:fetch-urlencoded-body`
- `probe:fetch-body-media-types`
- `probe:core-consumes-urlencoded`
- `probe:core-antiforgery-names`
- `probe:core-antiforgery-placement`
- `probe:core-antiforgery-safe-methods`
- `probe:mvc5-antiforgery-field-name`
- `probe:mvc5-antiforgery-reads-form-only`
- `probe:mvc5-antiforgery-validate-tokens`
- `probe:mvc5-max-request-length`
- `probe:mvc5-request-too-large-throws`
- `probe:core-upload-over-limit`
- `probe:core-route-value-binds-member`
- `probe:mvc5-value-provider-order`
- `probe:core-antiforgery-later-token`
- `probe:mvc5-form-filled-any-method`
- `probe:core-query-key-case`
- `probe:mvc5-query-key-case`
- `probe:js-query-get-case`
**Doors.**
- kept: `Get`, `Post`, `Put` and `Delete` (`Alis.Reactive/PlanAuthoring/Requests/RequestExtensions.cs:14-32`), as N10.7's
  verbs; `GatherBuilder.Set` (`Alis.Reactive/PlanAuthoring/Requests/GatherBuilder.cs:70-73`) as N10.7's `Set`; `Header`,
  in the request lambda; and `Form(formId)` (`Alis.Reactive/PlanAuthoring/Requests/GatherBuilder.cs:146`), its meaning
  narrowed to a native form's submission set and its argument the form's typed handle (D9.15, N18.3). `Gather` and
  `Gather<TRequest>` go to N10.7's request lambda, and `AsJson` and `AsFormData` to its body doors. The inputs of an
  element of another kind that it sent are `Inputs(container)`'s.
- refused: `Form()`, the inputs of the form the request checks, since a request names what it sends; its feature is
  carried by `Form(form)` and `Inputs(container)` (D9.15).
- `N9.1` new: `internal sealed class HttpSend` in `Alis.Reactive/Requests`, with its parts `PathPart`,
  `SentField`, `SentBody` and `SentHeader`, generated into the plan's TypeScript, the value of a `request`'s `send`
  (D10.23); it replaces the hand-typed `HttpRequest` (`Alis.Reactive.Assets/runtime/plugins/http.ts:10-18`).
- `N9.2` new: the staged builder: a GET builder with no body format, `Form` only over a native form and only in form
  data or a GET's query, and one container a request.
- `N9.3` new: a named input typed by the member it fills, `Input(r => r.Member, pipeline.Input(m => m.Member))`
  (N10.7, N14.4), replacing `Form(params Expression<Func<TModel, object?>>[])`
  (`Alis.Reactive/PlanAuthoring/Requests/GatherBuilder.cs:167`). Its fallback to another view's input of the same
  name, the one that joined the page last, leaves for `Inputs(container)` (D13.3).
- `N9.4` new: analyzer ALIS029, a file-shaped set member in a GET or a JSON body.
- `N9.5` new, built in POC 17c(a3-i) as ALIS036: `RequestKeysSuppressor`, a `DiagnosticSuppressor` that suppresses
  CS8602 in the member lambda of `Query` and `Set`, which names the key a request sends by its C# path and is never
  run (D9.4), on a read of the lambda's own parameter. The doors are matched by the method's definition. The warning
  stays on `Path`, whose member is the request's own, on a value the view holds, inside the key or beside it, and on
  every other lambda; an input's member path keeps it for MVC's helpers (N6.3).
- `N9.6` new: the request defaults of D9.12 and D10.12, configured once in C#; their door's form goes to gate 2.
- `N9.7` new: `Query<TQuery>(q => q.Set(x => x.Member, value or source))` on POST, PUT and DELETE.
- `N9.8` new: the wire forms `enum` and `flags` of `HttpSend`, written by `EnumNames.cs`, the file the rewrite's request
  unit adds to the Types module; no public door. `flags` serves a source whose enum the app names with N16.4; a literal
  enum is `Enum.ToString`'s text (D9.13).
- `N9.9` new: ALIS008's corrected message
  (`Alis.Reactive.Analyzers/HttpPipeline/MultipleHttpRequestsInPipelineAnalyzer.cs:17`), retargeted from
  `PipelineBuilder` and `RequestExtensions` (`Alis.Reactive.Analyzers/HttpPipeline/MultipleHttpRequestsInPipelineAnalyzer.cs:37-40`)
  in the block lambdas of Razor files only (`Alis.Reactive.Analyzers/HttpPipeline/MultipleHttpRequestsInPipelineAnalyzer.cs:59`)
  to the verbs of `Pipeline<TModel>` in every block, expression and `{}` lambdas both, in every file, since the
  defaults live in a startup file.

**Evidence.**
- B6: the request is a record of string-named fields (`Alis.Reactive/PlanModel/Requests/RequestStep.cs:101`) typed by
  hand in the browser (`Alis.Reactive.Assets/runtime/plugins/http.ts:10-18`).
- B10: a named input's type is erased (`Alis.Reactive/PlanAuthoring/Requests/GatherBuilder.cs:167`).
- B12: a missing path value throws inside the plugin (`Alis.Reactive.Assets/runtime/plugins/http.ts:83`).
- B14: a request member is written with its CLR name (`Alis.Reactive/Members/ExpressionPathHelper.cs:105`), and a net10
  `[FromBody]` endpoint does not bind a `[JsonPropertyName]` member by its CLR name (`probe:core-frombody-ignores-clr-name`).
  ALIS008 says "only the last request survives" (`Alis.Reactive.Analyzers/HttpPipeline/MultipleHttpRequestsInPipelineAnalyzer.cs:17`),
  while each request is appended as its own step (`Alis.Reactive/PlanModel/Document/PlanBuildContext.cs:109`), read and not run.
- G8: a set member takes any source of its type (`Alis.Reactive/PlanAuthoring/Requests/GatherBuilder.cs:70-73`).
- P1: 8046bc4c5 checked a request for the unprefixed name; b24c65775 left a number-keyed dictionary unsent in JSON.

**Defects.**
- `X9.1` type (D9.2): a body format on a GET does not compile: new:N9.2.
- `X9.2` type: `Form()` naming no form does not compile, and neither does `Form` given a container that is not a
  form: new:N9.2.
- `X9.3` type: a named input whose type the request member does not hold does not compile: new:N9.3.
- `X9.4` analyzer (D9.16): a file-shaped set member in a GET or a JSON body: new:N9.4.
- `X9.5` analyzer: a placeholder no member fills: "The URL '{url}' has the placeholder {name}, and the request has no
  member named {name}; add the member, or take the placeholder out of the URL." At build by ALIS030, and where the view renders on code it does not follow (D18.6): new:N18.7.
- `X9.6` analyzer: a date in a placeholder: "{member} is a date, and a date cannot fill the placeholder {name}: IIS
  refuses a time in a path segment. Name the member so it goes in the query." At build by ALIS030, and where the view renders on code it does not follow (D18.6): new:N18.7.
- `X9.7` analyzer: `Content-Type` as a header: "Content-Type is set by the body door; write Json, UrlEncoded, Multipart,
  Text or File." At build by ALIS030, and where the view renders on code it does not follow (D18.6): new:N18.7.
- `X9.8` behavior: a non-nullable member that reads nothing is left out of the JSON body, and the endpoint binds
  its starting value with no model error.
- `X9.9` behavior (D9.5): a path value that reads nothing sends no request, runs its `Finally`, and runs no step after it.
- `X9.10` behavior (D9.5): a path value read as "a/b" sends no request and runs its `Finally`.
- `X9.11` behavior: a query member read as "a b+c&d" reaches the endpoint as "a b+c&d".
- `X9.12` analyzer: a fixed header value with a line break: "The header {name} holds a line break, which HTTP does
  not carry in a field value; send it in the body." At build by ALIS030, and where the view renders on code it does not follow (D18.6): new:N18.7.
- `X9.13` behavior: a header read as text past US-ASCII sends no request, and the console names the header.
- `X9.14` type: `Form` inside a JSON body does not compile: new:N9.2.
- `X9.15` behavior: a native form's `<select form="intake">` outside the form's element is sent with the form's other
  controls and its hand-written `<input name="Code">`, and the endpoint reads one antiforgery field.
- `X9.16` behavior (D14.8): a fieldset container inside the intake form: the fieldset's request sends the fieldset's inputs
  only, and the form's request sends both.
- `X9.17` behavior (D14.8): a request that reaches a container by its id under a model it was not rendered for stops before
  it sends, the network log holds no request, and the console names the id and both models.
- `X9.18` behavior: a component holding `"night-shift"`, the held form of `Night` under `JsonStringEnumMemberName`, sets
  a request member, and the endpoint reads `Night`.
- `X9.19` behavior: a component holding a form no value of its enum is held as sends nothing, no later step runs, and
  the console names the member and the forms its enum is held as.
- `X9.20` refusal: an enum two of whose values are written in one form is refused where the view renders with
  "'r => r.Ward' holds EastWing and EastAnnex in one form, \"east\", under the app's options, so the browser cannot tell
  which one it holds; give one of them a JsonStringEnumMemberName of its own."
- `X9.21` refusal: an enum whose held form writes a value as an object is refused where the view renders with
  "'r => r.Shift' holds Day as {\"code\":0} under its own JSON converter, and a request sends one text or number per
  member; send a member written as text or a number."
- `X9.22` refusal: a literal its enum does not name is refused where the view renders with "'r => r.Shift' is set to
  5, which no value of its enum names: net10 refuses it and MVC 5 binds it, so the hosts bind it apart; set a value
  its enum names." At build by ALIS030 for a constant: new:N18.7.
- `X9.23` behavior: a `List<Shift?>` member whose source holds an item that reads nothing sends an empty field there,
  and the endpoint binds a null item (D9.10).
- `X9.24` behavior: a response body holding "Weekend, Mon", the held form of `Mon | Sat | Sun` under the app's string
  converter, sets a request member whose enum the app names as flags; the URL holds `Care=Mon%2C+Weekend` and the
  endpoint reads Mon, Weekend. Under the app's numbers a body holding 97 sends the same, and so does a component's
  member under either, since the live form holds a flags enum as its number whatever the app's options (D7.7,
  `probe:core-json-flags-forms`).
- `X9.25` behavior: a component holding 129, `Mon` with the bit 128 that no value of its flags enum names, sets a request
  member; the request sends nothing, no later step runs, and the console names the member and the value it holds.
- `X9.26` refusal (D9.22): a POST, PUT or DELETE rendered in an app whose `AddReactiveRequests` makes no antiforgery
  choice is refused where the view renders with "'{method} {url}' sends no antiforgery token, since the app chose
  none: call Antiforgery(AntiforgeryIn.Header), Antiforgery(AntiforgeryIn.FormField) or NoAntiforgery() in
  AddReactiveRequests."
- `X9.27` behavior (D9.8): a multipart body with two documents, each a file nested by C# path beside its caption, one
  file name past US-ASCII, and a text field binds on both hosts: the endpoint reads both files with their names, sizes
  and captions, and the field.
- `X9.28` behavior (D9.12): with `X-Requested-With: XMLHttpRequest` among the configured headers, a request whose own
  header `x-requested-with` holds `Fetch` reaches the endpoint with one `X-Requested-With`, `Fetch`, and a request
  with no header of its own sends `XMLHttpRequest`.

## Q10. The request's flow and its response

**Decision.**
- D10.1 A request runs in this closed order, the states of D10.23: the read of its argument and its check; the
  defaults' `WhileLoading`, then its own; its guards (path values, the container it sends, its `Input`s, its busy
  targets' elements, the contracts the defaults name), the send and one response route; its own `Finally`, then the
  defaults', each when its `WhileLoading` completed; then `Chained`; then the steps after the request
  (`Request.tla:StepsAfterRunUnlessStopped`).
- D10.2 A check that fails shows its failures (D16.13), sends nothing, runs neither `WhileLoading` nor `Finally`,
  and stops its pipeline: no step after the request runs (`Request.tla:FailedCheckSendsNothing`).
- D10.3 A `WhileLoading` that stops sends nothing, runs no `Finally` and stops its pipeline. `Finally` runs
  exactly when `WhileLoading` ran, after every outcome, a stop inside a route included. The defaults' layer wraps the
  request's: each layer's `Finally` runs exactly when its own `WhileLoading` completed, the request's first, so a stop
  in the request's `WhileLoading` still runs the defaults' `Finally` (`Request.tla:EachLayerPairs`). `Finally` runs
  after its route's whole block, the awaited steps in it included, and a request that block sends is the block's own:
  an abort or a dispose while it is in flight runs its `Finally` layers before the enclosing request's, each once,
  its own before the defaults' (`Run.tla:NothingRunsWhileItWaits`, `Run.tla:InnerFinallyFirst`,
  `Run.tla:FinallyRunsOnce`). An abort pairs the layers as a stop does: one that lands while a `WhileLoading` awaits
  runs that layer's `Finally` not at all, and the defaults' when theirs completed (`Run.tla:FinallyRunsWhenLoaded`,
  `Run.tla:FinallyOnlyWhenLoaded`).
- D10.4 A request follows a redirect that ends on the page's own origin, and the response it ends at is the
  request's answer: its status and body pick the route (`probe:fetch-follows-relative-redirect`). A redirect
  that ends on another origin is refused.
- D10.5 The outcomes are closed, and exactly one route runs for each, by D10.13:

  | Outcome | Route |
  |---------|-------|
  | a 2xx status and a body of the declared kind | `OnSuccess` |
  | a status that is not 2xx | the route for that status, else the catch-all |
  | the network failed, or a body broke off while it was read | the catch-all |
  | a redirect that ended on another origin | the catch-all |
  | a 2xx status and a body not of the declared kind | the catch-all |

  The last three each log one error naming the request and the outcome, and their data reads nothing. A
  request with no route for its outcome runs none.
- D10.6 The plan declares each route's body kind, from a closed set, and the media types each takes:
  - JSON, `application/json` or a type ending in `+json`, decoded as UTF-8 (RFC 8259); an empty JSON body is data that
    reads nothing, and a JSON body that does not parse is a body not of the declared kind;
  - text, any `text/*` type, read as a `string`;
  - HTML, `text/html` or `application/xhtml+xml`, which `Into` loads (D10.11); a `text/plain` body is not HTML, so an
    endpoint that answers a string writes it as `Content(html, "text/html")` or a partial view on net10, where a
    string action answers `text/plain` (`probe:core-string-result-media-type`), and MVC 5 answers it `text/html`
    (`probe:mvc5-string-result-content`, `probe:mvc5-content-type-kept-when-unset`, `probe:mvc5-response-default-text-html`);
  - a file, any media type, saved by the browser (D10.20);
  - none, a route with no data, whose body is not read.

  Text and HTML are decoded by the `charset` their `Content-Type` names, else as UTF-8, through `TextDecoder`, since
  fetch's `text()` decodes UTF-8 whatever the header says (`probe:fetch-text-ignores-charset`). A HEAD answer has no
  body, so a HEAD request's routes are of kind none. C# writes `Accept` from the success route's kind alone, with no
  `*/*`, because net10 does not respect an `Accept` that holds `*/*` and answers a string action as JSON only to
  `application/json` (`probe:core-accept-weighted`): `application/json` for JSON, `text/*` for text, `text/html` for
  HTML, and `*/*` for a file and for none. A validation error body is `application/problem+json` under any `Accept`
  (`probe:core-accept-weighted`). An error route declares its kind from the same set less a file: none, JSON, text or
  HTML, so a 409's text and an invalid post's partial view are read as they are sent. It reads its body as data: a
  body not of its kind reads nothing, and the route runs.
- D10.7 On net48 the plan carries that a response's `X-Responded-JSON` status replaces its own, so Katana's
  answer to a request that sends `X-Requested-With` runs the route of the status it withheld.
- D10.8 A server's validation errors have one body on both hosts, `{"errors": {"<binding name>": ["message"]}}`,
  status 400, written by a framework door: on net10 MVC's `ValidationProblemDetails`
  (`probe:core-validation-problem-shape`), on net48 the same JSON with `TrySkipIisCustomErrors` set, the switch
  System.Web gives a response that keeps its own error body under IIS (`probe:systemweb-try-skip-iis-custom-errors`).
  The browser reads that body only and shows it as it shows its own verdict (D16.13): every message in the
  request's summary place, and each also at the input the request sent under its key.
- D10.9 `Chained` runs after the first request's success route and its `Finally`, only on success, and reads the
  first response's data.
- D10.10 `Parallel` sends its requests at once. Each runs its own route as its response arrives, a stop in one
  ends that one only, and the steps after `Parallel` run once, after every request settled, whatever each
  outcome. `OnAllSettled` writes the same plan as those steps, after the `all` statement in the same block
  (`Alis.Reactive/PlanModel/Requests/ParallelStep.cs:28-35`): one meaning behind two doors, so it leaves and the steps
  after `Parallel` carry it (N10.7).
- D10.11 `Into(element)` loads the success body's HTML into the element as a slot (Q12): what the element held
  unloads first.
- D10.12 Request defaults are configured once in C# and hold headers (D9.12), the busy indicator and the notice
  (D10.21), the 400 route that shows a request's failures (D10.17), `WhileLoading` steps, `Finally`
  steps, error routes by status, a catch-all error route and a retry policy. C# composes them into every
  request where the view renders; the browser holds no defaults. A request's `WhileLoading` runs after the
  defaults', and its `Finally` before the defaults'. A request takes the defaults' retry policy or `NoRetry()`, and
  sets no numbers of its own (D10.14). Success
  has no default: `OnSuccess` is the request's own. A request takes no defaults when it says so, whole. A request a
  default's block sends (a 401 route that renews a session) takes no defaults, so a default never wraps the request
  it sends.
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
  - only GET, HEAD, OPTIONS, PUT and DELETE, which HTTP defines as idempotent (RFC 9110, section 9.2.2). A POST is never retried:
    a failed POST's effect can stand on the server, and a second attempt would repeat it.

  Its numbers are .NET's standard resilience handler's: 3 retries after the first attempt, an exponential wait with
  jitter from 2 seconds, a 429's or 503's `Retry-After` in place of the wait, and 30 seconds in all, past which the
  retries end. The app sets the attempts and the first wait once, in the defaults' `Retry(attempts, firstWait)`; a
  request takes the policy or `NoRetry()`, and sets no numbers of its own. Another policy is a later extension, the
  owner's.
  - `Retry-After` is the server's word when it sends one (RFC 9110, section 10.2.3): its seconds, or its HTTP date
    taken against the response's own `Date`, so the browser's clock does not enter. A wait that would pass the
    30 seconds ends the retries at once, and the error route runs. A value in neither form is the server's
    defect: it is logged once as an error, and the standard wait runs.
  - Retrying only when `Retry-After` is sent is refused: a network failure hands the page no header, a body that
    breaks off included, and a gateway's 502 or 504 comes from no app server, so neither carries the header, and those
    are the failures a retry exists for.
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
  - A part the request sends takes a condition: a member `Set`, a container's inputs, a named input and a header.
    When the condition fails, the part is left out, as if it were never gathered. The endpoint binds the
    member's starting value (D9.10), and the check reads the member as the endpoint binds it (Q16).
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
  its check and what it sends. So a default route for 400 shows the errors at the request's own summary place and
  the inputs it sent (D16.13). A part the request writes replaces or adds to its default, as D10.12 and D10.13 state. The condensed
  form and the full form write the same plan. The condensed form's syntax goes to gate 2 (step 7), with what
  what a `.cshtml` MVC view makes hard in its lambdas as its input.

- D10.18 What each step reads is stated by when it reads, and nothing is frozen.
  - A request's argument is read once, in the turn the request starts and before its check and its `WhileLoading`
    (D10.23), and serialized as it is sent, so the body is a snapshot by construction and the check reads that body.
    Main checks a container's inputs before `WhileLoading` and gathers the body after it
    (`Alis.Reactive.Assets/runtime/execution/requests/http.ts:40-48` at 7b9e987cc), so a `WhileLoading` that writes a
    sent input sends what was not checked; the read moves first because the check reads the body it sends (D16.5), and a
    value the request must send is one of its own parts (`Set`), which takes any source.
    A `Parallel`'s requests start in one turn and send what the page held then.
  - A response's data belongs to its run. The plan has no door that writes into it, and the generated contract types
    it read-only, so the runtime's own code cannot write into it either. A step that hands it, or any data value, to a
    vendor or a plugin (a `set` or a call argument) hands a structured copy (`probe:js-structured-clone-separate`).
  - The run records, as it sends, each input it sent: the binding name it sent it under and the entry the inputs
    table held for it (D13.1). An answer shows a message inline only at an entry the table still holds, so an input
    that left with its slot, or one a reload of its slot mounted again under the same id, shows nothing inline, and
    the summary place shows every message (`Ownership.tla:FailureReachesSummary`,
    `Ownership.tla:InlineOnlyAtTheSentInput`).
    What a component does with what it is given is its own: one that edits it edits its copy, and the later steps
    of its route and the request `Chained` sends read the response as the server sent it. Evidence
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
- D10.19 A response is a typed object, as a component is (Q5): its status, a number; each header by its name, text
  that reads nothing when the response lacks it; and its body by the route's kind (D10.6), whose members dot-walk as
  every data path does (Q1, Q2). Each is a source (Q7) in its route and in the block `Chained` runs (D10.9), and in
  every block inside them (D8.8); the steps after the request do not read it, since no door hands them the reply
  (`ReplyOutsideItsRoute`, `Schema.tla:RefsReadTheirBlock`). A HEAD route reads the status and the headers, and an OPTIONS route reads its
  `Allow` header (`probe:core-options-reaches-action`).
- D10.20 A file route saves the body as the browser's download, named by its `Content-Disposition`: the `filename*`
  of RFC 6266, section 4.3, before its `filename`, since MVC 5 writes a non-ASCII name as `filename*` alone
  (`probe:mvc5-file-name-rfc2231`) and net10 writes both, the `filename` with underscores
  (`probe:core-file-disposition`); a response with neither is named by the last segment of the request's path, as a
  browser names a link's download.
- D10.21 The defaults name a busy indicator and a notice, each a contract the core owns and a component fulfils: the
  busy indicator is the plugin `busy`, with `show(target)` and `hide(target)`, which set a state on one target and never
  hide by themselves; the notice is the plugin `notice`, with `show(text, kind)`, its kind one of success, error, warning
  and info. The framework ships NativeLoader and NativeToast declaring them, and an app registers its own object under
  the same name; the core depends on neither one's markup or CSS (the owner, 2026-11-08). The core alone calls their
  members: their declarations give a pipeline no door to them, since a step's hide while a request holds the indicator
  is the early hide `Composition.tla:IndicatorNeverHidesEarly` forbids.
  - Busy is a state of what a request modifies, as WAI-ARIA's `aria-busy` is of an element being modified (the owner,
    2026-11-08: the loader shows on the right target, parallel requests of 5 and 1 seconds updating one target or two).
    A request holds the indicator on its targets, which the C# derives where the view renders and the plan carries, so
    the browser infers nothing: each element and component its success route writes outside any condition (a text, an
    HTML, a class, a component's member or method, a component a payload holds among them, such as a row action's
    control, read when the request starts), the element its `Into(elementId)` loads and the element of the
    container its `Form` or `Inputs` sends. An `Input`, a notice, a plugin, a dispatch and a step inside a branch add
    none, and a request whose route
    writes nothing so holds the page's own target, an indicator with no element. On the full form `Busy(elementId, ..)`
    names the targets in place of those, `Busy()` names the page, and `NoBusy()` holds none. The cost is said plainly: a
    route that writes five elements marks five busy, as aria-busy means of each element being modified, and one
    `Busy("panel")` replaces them. The two parallel requests that set one component each from a JSON body hold that
    component's element by default, with no `Busy` written.
  - The core counts each target across every pipeline of the page, calls `show(target)` when its count leaves 0 and
    `hide(target)` when it returns to 0, `target` the element's id, and `show()` and `hide()` with no target for the
    page, and sets `aria-busy="true"` on a target's element while its count is above 0.
    Two requests on one target keep it shown until the later one settles, whichever is slower; on two targets each
    hides when its own request settles; a request on two targets holds both (`Composition.tla:IndicatorNeverHidesEarly`,
    `Composition.tla:IndicatorNeverStaysShown`, `Composition.tla:HoldOncePerRequest`).
  - A target whose element the page lacks when the request starts stops it before it sends, as a missing element stops
    an element step's pipeline. A target's element that leaves with its slot while held takes its `aria-busy` with it,
    and the core still calls `hide(target)` when the count returns to 0. `Busy` on a request with `NoDefaults()`, and a
    second `Busy` or `NoBusy` on one request, are refused where the view renders.
  - A contract the defaults name and no script provides (`busy` under `ShowBusy()`, `notice` for a default route's
    notice) is a plugin lookup that fails, so it stops the request before it sends and logs one error, as a plugin a
    step names and no script provides stops its pipeline (D17.14); the missing component shows at the first request,
    not at the first failure.
  - A request holds its targets once across its attempts (D10.15), from its guards, once both `WhileLoading` layers
    completed, to the end of its defaults' `Finally`, on every outcome, a replaced or unloaded run's included (Q11,
    Q12). A failed check, a stopped `WhileLoading` or a failed guard holds nothing (`Request.tla:HoldSpansTheSend`,
    `Request.tla:FailedGuardHoldsNothing`), and an aborted or disposed run releases what it held after its defaults'
    `Finally` (`Run.tla:ReleasedWhenEnded`). A failed request
    whose route is a default shows one notice, after its last attempt; a replaced or unloaded request shows none. A
    request that takes no defaults, or whose busy is `NoBusy()`, covers no indicator; one that takes no defaults shows no
    default notice.
- D10.22 The condensed body is multipart. Multipart binds with no attribute on both hosts and carries files, and where it
  is wrong it fails loud: a `[FromBody]` action answers it 415, which runs the request's routes
  (`probe:core-frombody-multipart`). JSON as the condensed body would fail silent: net10 answers a JSON body to an
  action whose model is bound from the form 200, with nothing of the body bound (`probe:core-json-to-form-bound`). Main's
  default is JSON (`Alis.Reactive/PlanAuthoring/Requests/HttpRequestBuilder.cs:21` at 7b9e987cc); its feature is kept by
  `Json(..)`, which a `[FromBody]` or `[ApiController]` endpoint takes.
- D10.23 A request is one statement of the plan, `request`, and one state machine, the one `spec/RequestsAndResponses/Request.tla`
  checks and `spec/Execution/Run.tla` runs as a frame (the owner, 2026-11-08: "treat request as a unit", "there is implicit
  state machine here"). It is `idle` until its statement runs, then in the states `check`, `defaultLoading`, `loading`,
  `guard`, `attempt`, `route`, `finally`, `defaultFinally`, `chained` and `after`, and `done` however it ended, which
  its completion tells (D11.12). The statement's executor holds the request's state (the argument it
  read, its attempt, the layers whose `WhileLoading` completed, the busy targets it holds), and no other code holds
  any of it. C# writes the statement whole, each part in its state whatever order the view wrote the parts in (D10.1).

  | State | Runs | Then | A stop | An abort |
  |-------|------|------|--------|----------|
  | `check` | reads the request's argument once (D10.18), recording a path value that reads nothing, a container, an input or a busy target's element the page lacks; then its check, on the body it read (D10.2, D16.5) | `defaultLoading`, or `loading` for a request that takes no defaults | a source that stops when read (D4.1), or a failed check, whose failures show: nothing is held and no layer runs | none: the state holds no await |
  | `defaultLoading` | the defaults' `WhileLoading` | `loading` | the pipeline stops; neither `Finally` runs (D10.3) | as a stop |
  | `loading` | the request's `WhileLoading` | `guard` | the defaults' `Finally` runs, then the pipeline stops | as a stop: the request's own `Finally` does not run, since its `WhileLoading` did not complete |
  | `guard` | checks what the read recorded (D4.6, D9.5, D9.15), each value's wire form (D9.10, D9.13, D9.23) and the contracts `HttpSend` names (D10.21); then holds its busy targets | `attempt` | nothing is held; `finally` and `defaultFinally` run, then the pipeline stops, its error naming the guard and the part | none: the state holds no await |
  | `attempt` | sends under its block's abort signal (D11.12); a retryable outcome under the policy waits and sends again (D10.14, D10.15) | `route` | | the fetch or the wait ends and no route runs; then `finally`. Under `replace`, a request inside a `Finally` goes on (D11.11) |
  | `route` | the one route the outcome picks (D10.5, D10.13), the reply bound in its block; its awaited steps and requests are its tail (D11.5) | `finally` | `finally` and `defaultFinally` run, then the pipeline stops | the route's later steps do not run; a request of its tail runs its own layers first; then `finally`. Under `replace`, a route inside a `Finally` goes on (D11.11) |
  | `finally` | the request's `Finally`, exactly when `loading` completed | `defaultFinally` | its later steps do not run; `defaultFinally` runs, then the pipeline stops | one that lands while it awaits leaves it running under `replace`; under a dispose it runs to its first awaited step (D11.8) |
  | `defaultFinally` | the defaults' `Finally`, exactly when `defaultLoading` completed; then the busy targets it holds are released (D10.21) | `chained` after a success, else `after` | the release, then the pipeline stops | as at `finally`, then the release |
  | `chained` | the block `Chained` runs, the reply bound; a condition on it is an `if` C# writes inside it (D10.9, D10.16) | `after` | the pipeline stops | as in any block |
  | `after` | the steps after the request in its block, which do not read the reply (D10.19) | `done` | | |

  A request inside a route or a `Chained` block is a statement of that block, so it runs inside the enclosing
  request's state, and its layers end before the enclosing state goes on: a stop or an abort runs the inner request's
  own `Finally`, the inner defaults', then the outer's own and the outer defaults', each once and each when its
  `WhileLoading` completed (`Run.tla:InnerFinallyFirst`, `Run.tla:FinallyRunsOnce`, `Run.tla:FinallyRunsWhenLoaded`,
  `Run.tla:FinallyOnlyWhenLoaded`). A request in a `Parallel` branch runs in that branch's block of `all`
  (`Lanes.tla:JoinAfterEveryBranchTail`). Every busy target a request held is released by the time its run ends,
  aborted or disposed (`Run.tla:ReleasedWhenEnded`).

**Schema nodes.** `request`, its routes and its layers (D10.23), `get` of `status`, `outcome`, `body` and `request` on
the reply and `call` of `get` on its `headers`, the contract `HttpSend` and the reply, `HttpResponse`, with the fields
`schema.md` lists for them, and the contract plugins `busy` and `notice` (D10.21).

**Checker.**
- `Request.tla:FailedCheckSendsNothing`
- `Request.tla:FinallyRunsWhenLoadingRan`
- `Request.tla:OneRouteRuns`
- `Request.tla:SuccessWithNoRouteRunsNone`
- `Request.tla:FailureRunsTheCatchAll`
- `Ownership.tla:FailureReachesSummary`
- `Ownership.tla:InlineOnlyAtTheSentInput`
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
- `Composition.tla:IndicatorNeverHidesEarly`
- `Composition.tla:IndicatorNeverStaysShown`
- `Composition.tla:NoticeOncePerFailure`
- `Request.tla:EachLayerPairs`
- `Request.tla:HoldSpansTheSend`
- `Request.tla:FailedGuardHoldsNothing`
- `Run.tla:InnerFinallyFirst`
- `Run.tla:FinallyRunsWhenLoaded`
- `Run.tla:FinallyOnlyWhenLoaded`
- `Run.tla:ReleasedWhenEnded`
- `probe:core-frombody-multipart`
- `probe:core-json-to-form-bound`
- `probe:core-string-result-media-type`
- `probe:mvc5-string-result-content`
- `probe:mvc5-content-type-kept-when-unset`
- `probe:mvc5-response-default-text-html`
- `probe:fetch-text-ignores-charset`
- `probe:core-accept-weighted`
- `probe:fetch-null-body-reads-empty`
- `probe:core-file-disposition`
- `probe:mvc5-file-name-rfc2231`

**Doors.**
- kept: `OnSuccess`, `OnSuccess<T>`, `OnError` and `OnError(status)` with its typed form, as routes on the request
  (N10.7), `Chained`, `Parallel`, `WhileLoading`, `Finally`, `Into` and `Validate`, which checks the body its request
  sends (D16.2); `Response(..)`'s builder goes to those routes, and `OnAllSettled` to the steps after `Parallel` (D10.10). `ValidationErrors(formId)` (`Alis.Reactive/PlanAuthoring/Pipelines/PipelineBuilder.cs:291`) becomes
  `ShowValidationErrors(problem)`, a step of a request's route that shows the problem as that request's failures (D16.13); a
  route shows no other container's failures, since a message maps only to the inputs its own request sent (D13.3).
- `N10.1` new: the error door on each host, `ValidationResult.ToErrorResponse()`: net10 a `ValidationProblemDetails`
  result, net48 a JSON result with status 400 and `TrySkipIisCustomErrors`, replacing the body each app builds by hand.
- `N10.2` new: `HttpResponse`, the contract of what `send` returns (`status`, `outcome`, `body`).
- `N10.3` new: the standard retry policy of D10.14 in the defaults, its attempts and first wait, and a request's
  opting out; the door's form goes to gate 2.
- `N10.4` new: a condition on a part a request sends, on a chained request and on a parallel branch (D10.16),
  through the one condition core; its door's form goes to gate 2.
- `N10.5` new: the condensed request of D10.17, whose default parts read the request's own parts; its form goes
  to gate 2.
- `N10.7` new, the HTTP surface chosen 2026-11-08 from two independent designs (verdicts per axis in `spec/NOTES.md`),
  replacing N9.3, N9.7, N10.2 and N10.5's open forms:
  - startup, once on each host: `services.AddReactiveRequests(requests => ..)` with `Antiforgery(AntiforgeryIn.Header)`,
    `Antiforgery(AntiforgeryIn.FormField)` or `NoAntiforgery()`, `Header(name, value)`, `ShowBusy()`,
    `ShowValidationErrors()`, `WhileLoading`, `Finally`, `OnError(status, ..)`, `OnError(..)` or `OnError<TError>(..)` and `Retry(attempts,
    firstWait)`; a defaults pipeline is `Pipeline<NoModel>`, so it names no view member. The call is required: a view
    that renders a request in an app without it is refused, naming it. A default `Header(name, value)` takes text
    only: no page exists at startup, so there is no source to read; a header read from the page goes on the request. A
    default route's reply is `Response<NoModel>` or `Response<NoModel, TError>`: the defaults cannot know a request's
    type, and `NoModel` has no member, so its `Request` offers nothing to read, and the reply stays the one family every
    route reads.
  - where the doors live: a call that names only the method's own type argument does not bind to an extension of a
    generic receiver. The classic form binds only when the call names the receiver's type argument too
    (`Post<IntakeView, IntakeRequest>`, as `spec/api/HttpSurface` writes) and fails CS1501 without it; a C# 14 extension
    block fails CS1061 (`spec/api/refused.sh`: `VerbAsExtension`, `VerbAsExtensionBlock`, `PageUrlAsExtension`, on SDK
    10.0.101; the block form also on 11.0.100-rc.1). An instance member binds it
    (`poc/Alis.Reactive.Poc/Pipelines/Pipeline.cs:39`), and the stub's `PipelineVerbs<TModel>`, which stands in for
    `Pipeline<TModel>` since the POC's is sealed, compiles every verb, `PageUrl` and `Container` so, each checked against the classic
    form's types in `VerbsAgree`. So a door the view calls with one type
    argument is an instance member of the type it is called on: the verbs, `PageUrl<TPage>()` and
    `Container<TContainer>(containerId)` on `Pipeline<TModel>`, `OnSuccess<T>` and `OnError<T>` on the request types. A
    door with no type argument to name (`Set`, `Form`, `Inputs`, `Input`, `OnSuccess(pipeline => ..)`, `Into`,
    `Download`, `Notice`, `ShowValidationErrors`, `Validate(container)`) is an extension method over them. The modules: the verbs,
    the request types, `RequestDefaults` and `NoModel` are `Pipelines`', as the POC's `Pipelines/Requests.cs` is, since
    each takes a pipeline; the objects a request reads (`Response`, `Failure`, `Content`, `Markup`, `ResponseFile`,
    `BrowserFile`, `PageUrl`, `ValidationProblem`, `RequestContainer`, `RequestForm`) are `Requests`', which names no
    pipeline; `AddReactiveRequests` and
    the net48 filter are `Hosting`'s. `Hosting` depends on `Pipelines`, `Pipelines` on `Requests`, and none returns
    (`spec/api/HttpSurface` builds with each type in its module's namespace).
  - the verbs, instance members of `Pipeline<TModel>`: `Get`, `Head`, `Options`, `Post`, `Put`
    and `Delete`, each `(url)` or `<TRequest>(url, request => ..)`. The request lambda holds the HTTP parts: `Path(r =>
    r.M, value)`, `Query(r => r.M, value)`, `Header(name, value)`, and on POST, PUT and DELETE one body door, `Json`,
    `UrlEncoded`, `Multipart`, `Text` or `File`. The doors that send inputs (D9.15) are `Inputs(container)` and
    `Input(r => r.M, input)` in every body and on a GET, and `Form(form)` in a URL-encoded or multipart body and on a
    GET; a GET's inputs go to its query. A value is a literal or any source (Q7); a member lambda dot-walks
    (`r => r.Handover.Round`) in `Query` and `Set`, while `Path` takes a member of the request itself (D9.4). `Header`
    refuses `Content-Type`, `Accept`, the headers the framework writes and the Fetch Standard's forbidden request
    headers (D9.11).
  - staged by type: GET, HEAD and OPTIONS offer no body door; `NoRetry()` exists on GET, HEAD, OPTIONS, PUT and DELETE
    only, since the one policy (D10.14) never retries a POST (X10.9), and no request sets the policy's numbers, which
    the defaults' `Retry(attempts, firstWait)` sets once (`RetryPerRequest`, `NoRetryOnPost` in `spec/api/refused.sh`); HEAD offers no content reader; `File` exists only inside `Multipart` or as the whole body; a
    success route returns the request carrying its reply's type, which offers no second success route and whose
    `Chained` receives that reply. A body door returns a stage that offers no second body door (`SecondBodyDoor`), a
    `Text` or `File` body one that offers no `Set` and a `Set` one that offers no whole body (`SetAfterAWholeBody`,
    `AWholeBodyAfterSet`), the full form's body door takes the container (`ContainerInsideABody`), a GET's and the
    condensed `Form` and `Inputs` return a stage that offers no second (`SecondContainerOnGet`,
    `SecondContainerOnABody`), and `Chained` ends the sentence (`SecondChained`), each in `spec/api/refused.sh`. A stage
    holds in the chain; a staged value used twice, a parts object two statements write or a request held in a variable
    and written twice, is ALIS030's, so a request's parts are one chain (N18.7).
    ALIS030 refuses what no type stages: a second `Busy` or `NoBusy`, `Busy` beside `NoDefaults()`, a status outside
    300 to 599, a second route for one status, a second catch-all and a second `WhileLoading` or `Finally` (N18.7). A `Set` on a file member stays D9.16's analyzer, since `Set` takes a member of any
    type, and a file input a GET's `Form` or `Inputs` sends stops the request when it is sent (D9.16). A file member is `IFormFile`
    or a list of it on net10 and MVC 5's posted-file base or a list of it on net48, behind `#if NET48`. A request
    `Header(name, source)` whose source's wire form is a list or an object is refused where the view renders, as the
    POC refuses a list in a path segment (`poc/Alis.Reactive.Poc/Types/WireForms.cs`), since no C# constraint names a
    scalar.
  - the routes, on the request: `OnSuccess(Content.<kind>, (reply, pipeline) => ..)`, `OnError(status, Content.<kind>,
    (reply, pipeline) => ..)` and the catch-all `OnError((failed, pipeline) => ..)` or, typed, `OnError<TError>((failed,
    pipeline) => ..)`, with the readers `Content.None`,
    `Content.Json<T>()`, `Content.Text`, `Content.Html` and `Content.File` (D10.6). `OnSuccess<T>` returns a request that
    carries `T`, so its `Chained((reply, pipeline) => ..)` receives the typed reply by construction. `Validate<TValidator>()`,
    `TValidator : ReactiveValidator<TRequest>` (D16.1), checks the body the request sends (Q16); `Busy(elementId, ..)`,
    `Busy()` and `NoBusy()` say where its busy indicator shows (D10.21); `WhileLoading`, `Finally`, `NoDefaults()` and
    `NoValidation()` complete it. `ShowValidationErrors(problem)` is a step of a route's pipeline that shows any
    `ValidationProblem` source, a 400's body or another route's, as the request's failures (D16.13), refused by ALIS031
    in a block no route encloses (N18.8).
  - the objects, each a JS object whose members are sources: `Response<TRequest>` (`Status`, `Headers.Get(name)` and
    `Request`, the request's members as it sent them, read as `TRequest`, which a read of the input cannot give once the
    user edits it during the send), the reply of a route of kind none and of every HEAD route, and
    `Response<TRequest, TBody>`, which adds `Body`, a data read that dot-walks; `Failure` (`Outcome`: status, network, redirect or
    content, `FailureOutcome`, a closed choice (S8) whose own converter writes each case as the plan's outcome,
    `status`, `network`, `redirect` and `content`, and which no converter the app's JSON options add for enums
    reaches, so a condition on it agrees with the browser; `Status`, 0 for a
    network failure, a body that breaks off while it is read included, the Fetch Standard's network error; `Headers`);
    `Failure<TError>`, the typed catch-all's reply, which adds `Body`, a data read of the JSON body when its outcome is
    status and the body parses, and nothing for a network, redirect or content outcome (D4.2); `ResponseFile`, the data of a file body (`Name`, `Size`, `Type`); `Markup`, an HTML
    body, the only text `Into(elementId, source)` takes; `BrowserFile` (lib.dom `File`), held by a file input's member;
    and `pipeline.PageUrl<TPage>()` or
    `PageUrl<TPage>(template)`, whose `Read(q => q.A.B)` reads a placeholder member from the path and any other from the
    query by its C# path, as the action that rendered the page bound it, the member's start when the key is absent or
    its text does not convert. Keys are compared ignoring case, as both hosts bind them (`probe:core-query-key-case`,
    `probe:mvc5-query-key-case`), which `URLSearchParams.get` does not (`probe:js-query-get-case`).
  - a reply's headers, on `Response<TRequest>` and `Failure`: `Headers.Get(name)` reads a header by the browser's
    `Headers.get`, so a name matches in any case, a header the answer repeats reads its values joined by a comma and a
    space, and a header the answer lacks reads null, as `Headers.get` types it (`string | null`, so
    `MethodResult<string?>`), and a text set from it shows nothing (D5.3). Each outcome reads the headers of the answer
    the browser hands the page (the Fetch Standard): a success, a status and a content outcome, the final answer's after
    any redirect within the page's origin; a redirect to another origin, the final answer's as CORS filters them, the
    CORS-safelisted response headers and those that origin names in `Access-Control-Expose-Headers`, so a header it does
    not expose reads null; a network failure, the network error, none, so every header reads null. Refused where the
    view renders (`poc/Alis.Reactive.Poc/Requests/HeaderNames.cs`): a name that is no token, by the token rule a request
    header's name follows (D9.11), and `Set-Cookie` and `Set-Cookie2` in any case, the Fetch Standard's forbidden
    response header names, which the browser never hands a page, so a read of either reads null on every answer.
  - the condensed doors, which write the plan the full form writes: `Set(r => r.M, value)`, placed by
    D9.4, D9.6 and D9.9 into `Path`, `Query` or the body the request names, else multipart (D10.22); `Form(form)` and
    `Inputs(container)`, sent as multipart fields and file parts, a GET's into its query; `Input(r => r.M, input)`,
    placed as `Set` is. Every request is checked by the validator registered for `TRequest` (D16.2), whatever it sends;
    `OnSuccess(pipeline => ..)` of kind none, `OnSuccess<T>((reply, pipeline) => ..)` and `OnError<T>(status, ..)` of kind
    JSON, `Into(elementId)` of kind HTML and `Download()` of kind file; and `Notice()` with `Success`, `Error`, `Warning`
    and `Info` over the notice contract (D10.21).
  - `Parallel(params Action<Pipeline<TModel>>[] branches)`, each branch a pipeline, the steps after it its all-settled
    block; `OnAllSettled` leaves (D10.10).
  - net48 only: `[ValidateAntiForgeryHeader]`, a filter that reads the header, else the field, and hands it to
    `AntiForgery.Validate(cookieToken, formToken)`; shipped in the net48 host (gate 1, C10, D9.22).
  - it replaces the POC's request doors (`poc/Alis.Reactive.Poc/Pipelines/Requests.cs`), each guard given its home:
    `Get(url)` becomes `Get(url)` on every verb, keeping the refusal of a placeholder no member fills, a fragment, and a
    URL that is neither a page path nor an http or https URI (`RequestUrls.Checked`). `Request.Into(id)` becomes the
    condensed `Into(elementId)`, keeping its required id and its load on success only; in a chain its one `Into` holds by
    type, since the stage it returns offers no success route. `Get<TRequest>(url, members)` becomes `Get<TRequest>(url, request => ..)`,
    keeping a member set once ignoring case, a placeholder with no member, and a list or a time refused in a path
    segment; its refusal of a member past the request's own (`r => r.A.B`) gives way to D9.19's C# path, as the binder
    reads it. `SentRequest.Response(routes)` gives way to the routes on the request; its one `Response` and its same
    sentence become a stage that takes one door and a route written in its request's sentence, which a request held in a
    variable can break, so ALIS030 refuses both (N18.7) and, until it is built, the view refuses them where it renders:
    a stage written a second time and a route written after another step. `Routes.OnSuccess` becomes `OnSuccess(Content.<kind>, ..)` and `OnSuccess<T>`, one
    by type; `Routes.OnError(status, ..)` becomes `OnError(status, Content.<kind>, ..)` and `OnError<T>(status, ..)`,
    keeping a status from 300 to 599 and one route a status, ALIS030's (N18.7); `Routes.OnError(otherwise)`
    becomes `OnError((failed, pipeline) => ..)` with `Failure`, and main's typed catch-all `OnError<TError>(..)` becomes
    `OnError<TError>((failed, pipeline) => ..)` with `Failure<TError>`, keeping one catch-all, ALIS030's.
    `ResponseBody<TBody>.Read(path)` becomes `Body.Read(path)`, data that reads its start when absent (D4.2). The POC's kinds by
    door (HTML for `Get(url)`, JSON for `Get<TRequest>`) become the kind each route names, which the plan carries on the
    route, and `Accept` is written from the success route's kind (D10.6): a request with no success route reads none.

  ```csharp illustration
  pipeline.Post<IntakeRequest>("/Wards/{ward}/Intake", request => request
          .Set(r => r.Ward, pipeline.PageUrl<IntakePage>().Read(q => q.Ward))
          .Form(intake))
      .OnSuccess<IntakeCreated>((created, pipeline) => pipeline.Notice().Success("Resident admitted"))
      .Chained((created, next) => next
          .Get<SummaryQuery>("/Residents/{residentId}/Summary", query => query
              .Set(q => q.ResidentId, created.Body.Read(c => c.Resident.Id)))
          .Into("resident-summary"));

  pipeline.Post<IntakeRequest>("/Wards/{ward}/Intake", request => request
          .Path(r => r.Ward, pipeline.PageUrl<IntakePage>().Read(q => q.Ward))
          .Multipart(body => body
              .Set(r => r.Ward, pipeline.PageUrl<IntakePage>().Read(q => q.Ward))
              .Form(intake)))
      .Validate<IntakeValidator>()
      .OnSuccess(Content.Json<IntakeCreated>(), (created, pipeline) => pipeline.Notice().Success("Resident admitted"))
      .OnError(400, Content.Json<ValidationProblem>(), (refused, pipeline) => pipeline.ShowValidationErrors(refused.Body))
      .Chained((created, next) => next
          .Get<SummaryQuery>("/Residents/{residentId}/Summary", query => query
              .Path(q => q.ResidentId, created.Body.Read(c => c.Resident.Id)))
          .OnSuccess(Content.Html, (summary, pipeline) => pipeline.Into("resident-summary", summary.Body)));
  ```
  The first is the condensed form under defaults that hold `ShowBusy()`, `ShowValidationErrors()` and a catch-all notice;
  the second writes the same plan less those defaults' parts, which both compose identically. `intake` is the
  `RequestForm<IntakeRequest>` the view's form block returns (N14.3).
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
- `X10.2` behavior (D9.12, D10.7): with `X-Requested-With` among the configured headers, a request whose cookie session expired
  runs the route for 401 on net10 (`probe:core-cookie-auth-ajax`) and on net48 under Katana
  (`probe:katana-ajax-redirect-is-200`). Without it the server redirects to its login page, and the request
  ends there: a route that declares JSON runs the catch-all, and a route with no body runs `OnSuccess`.
- `X10.3` behavior (D10.1): a network failure runs `OnError()`, then its `Finally`, and its busy targets hide after the
  defaults' `Finally`.
- `X10.4` behavior (D10.2): a form check that fails sends nothing and runs no step after the request.
- `X10.5` behavior: a server's validation errors show at the inputs whose binding names they key, on both hosts.
- `X10.6` behavior: `OnAllSettled` runs once, after the slower of two parallel requests answers.
- `X10.8` behavior (D10.15): a GET whose policy retries 503 answers the second attempt's success, shows its busy targets once
  and runs its `Finally` once.
- `X10.9` type (D10.14): a policy that retries a POST cannot be written: new:N10.3.
- `X10.7` behavior: on net48 under IIS, a 400 with validation errors shows each message at its input; the
  `verify-net48` run proves it.
- `X10.10` behavior (D10.16): a nullable member whose condition fails is not in the body the request sends, and the
  endpoint answers with the member's starting value, not null.
- `X10.11` behavior (D10.16): a chained request whose condition fails is not sent, and the first request's success
  steps and the steps after it ran.
- `X10.12` behavior (D10.12, D10.17): a request that writes only its method, URL, form and success shows a 400's errors at its
  own form, through the defaults' route.
- `X10.13` behavior: a component that edits a row it was given from a response leaves the response as sent, and
  a `Chained` request that reads the response's rows sends the server's rows, not the component's.
- `X10.14` behavior: two `Parallel` requests whose routes write one text: the page shows the text of the route whose
  response the test app sends last.
- `X10.15` behavior (D10.14): a GET answered 500 runs its error route once, with no second attempt; a GET answered 503 and
  then 200 runs its success route.
- `X10.16` behavior (D11.9): two `Parallel` requests on one target, the test app answering one after 1 second and the other
  after 5: the target's indicator and `aria-busy` stay until the second answers; on two targets, the first target's
  indicator hides at 1 second while the second's stays to 5.
- `X10.17` behavior: a request whose busy target is an element the page lacks stops before it sends, and the network
  log holds no request.
- `X10.18` analyzer: `Busy` on a request with `NoDefaults()`: "Busy says where the default busy indicator shows, and
  this request takes no defaults; take out NoDefaults() or Busy." At build by ALIS030, and where the view renders on code it does not follow (D18.6): new:N18.7.
- `X10.19` behavior: two `Parallel` requests with no `Busy`, each success route setting one component from a JSON body:
  each component's element shows the indicator until its own request settles, and the page's indicator never shows; a
  route that writes inside a `When` only shows the page's.
- `X10.20` behavior: under `ShowBusy()` in an app whose page provides no `busy`, a request stops before it sends, the
  network log holds no request, and the console one error.
- `X10.21` type (N10.7): a second body door does not compile:
  `spec/api/HttpSurface/refused/SecondBodyDoor.CS1061.cs.txt:12`. The chain does not compile; a staged value used twice is ALIS030's: new:N18.7.
- `X10.22` type (N10.7): a `Set` after a `Text` body and a `Text` after a `Set` do not compile:
  `spec/api/HttpSurface/refused/SetAfterAWholeBody.CS1061.cs.txt:10`,
  `spec/api/HttpSurface/refused/AWholeBodyAfterSet.CS1061.cs.txt:10`. The chain does not compile; a staged value used twice is ALIS030's: new:N18.7.
- `X10.23` type (D9.15): a second container on a body or a GET, and a container inside a body, do not compile:
  `spec/api/HttpSurface/refused/SecondContainerOnABody.CS1929.cs.txt:10`,
  `spec/api/HttpSurface/refused/SecondContainerOnGet.CS1929.cs.txt:10`,
  `spec/api/HttpSurface/refused/ContainerInsideABody.CS1929.cs.txt:10`. The chain does not compile; a staged value used twice is ALIS030's: new:N18.7.
- `X10.24` type (N10.7): a second `Chained` does not compile: `spec/api/HttpSurface/refused/SecondChained.CS0023.cs.txt:13`. The chain does not compile; a staged value used twice is ALIS030's: new:N18.7.
- `X10.25` analyzer (D10.21, D10.13): a second `Busy` or `NoBusy`, a status outside 300 to 599, a second route for
  one status and a second catch-all: new:N18.7.
- `X10.26` analyzer (D9.17, D11.6): a second request in one block, at its own level, in every stage and file: ALIS008,
  new:N9.9.
- `X10.27` analyzer (N10.7): a request's parts object written by two statements, and a request held in a variable and
  written twice (a second `Chained` among them), each of which compiles: new:N18.7.
- `X10.28` refusal (N10.7): a request rendered in an app that never called `AddReactiveRequests` is refused where the
  view renders with "'{method} {url}' is rendered in an app that never called AddReactiveRequests; call
  services.AddReactiveRequests(requests => ..) once at startup." On net48, where an MVC 5 view reaches the app's
  registrations only through MVC's resolver (M19.D4), the text is "'{method} {url}' is rendered in an app that never
  called AddReactiveRequests or set MVC's resolver; call services.AddReactiveRequests(requests => ..) once at startup
  and set the resolver over the container with DependencyResolver.SetResolver in Application_Start."
- `X10.29` behavior (N10.7): a typed catch-all, `OnError<TError>((failed, pipeline) => ..)`, reads the JSON body of a
  status no route names through `failed.Body`, and `Body` reads nothing after a network failure, while `Outcome` reads
  `network`.

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
    logs one error and runs the `Finally` layers of every request it is inside (D10.23). The value it resolves to is read as data
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

  A payload declared as a component (`Component`) is that component, not arguments its vendor reads when its
  callback returns: its setter takes a write whenever the step runs, so a write to its own property runs after an
  awaited step and inside a `listen` (D12.8). The meaning is the declaration's, never the expression's: a payload
  declared any other way is refused as above.

  The refusal is each handler's own: an await in one handler refuses no write in another. It follows the path
  the handler runs: a branch reads the state before its `if`, so an `Else` writes the payload in the vendor's turn
  when its sibling `Then` awaits, and a step after the `if` reads the payload as awaited when any of its branches
  awaits. An `ElseIf`'s branch and the `Else` after it read the state after the conditions before them, so an
  `Else` after an `ElseIf` whose condition awaits reads the payload as awaited (X11.13). A request waits at its send,
  so its routes, its `Finally` layers and `Chained` read the payload as awaited, and its `WhileLoading` reads it as
  the defaults' `WhileLoading` leaves it: the defaults are the app's options, so where their `WhileLoading` awaits, a
  payload write in a request's own `WhileLoading` is refused where the view renders (X11.18,
  `Run.tla:WritesAfterTheTurnAreRefused`).
- D11.5 The order is closed:
  - The steps after an awaited step run after it settles.
  - Two handlers of one event run in the order the plan subscribed them, each up to its first awaited
    step before the next starts. After that, their runs go on as their awaits settle, and a stop ends
    only its own handler (Q4).
  - `Parallel` starts its branches in authored order in one turn, each up to its first awaited step. The
    steps after `Parallel` run once, after every branch settled, and a stop in a branch ends only that
    branch (D10.10).
  - A step settles when every block it ran settles. A branch, a route, `WhileLoading`, `Finally`, the block
    `Chained` runs and a `Parallel` branch hold awaited steps of their own, so the steps after an `if`, a request's
    `Finally` and `Chained`, the steps after the request and the steps after `Parallel` wait for the whole of each
    block they follow (`Run.tla:NothingRunsWhileItWaits`, `Lanes.tla:JoinAfterEveryBranchTail`; one level is
    proven at `tests/Alis.Reactive.Poc.PlaywrightTests/Conditions/WhenAPipelineBranches.cs:77`). A run is in flight
    until its last block settles, so `replace`, `skip` (D11.7) and dispose (D11.8) meet a route's awaited tail as in
    flight.
- D11.6 A request is a step, so it runs in every block (D11.10), one at each block's own level (D9.17).

  A run belongs to the owner of the handler that started it: the page, or the slot whose plan holds the
  handler (Q12). A plugin's member is read, written and called in the same blocks, the request's included,
  through its typed declaration (Q5), and its async methods are awaited steps (D11.2).
- D11.7 A handler's policy decides what a trigger does while that handler's previous run is in flight. The
  policy belongs to the handler, not to a request. A run is in flight from its trigger until it ends, its `Finally`
  layers included, so `replace` and `skip` read a run in its `Finally` as in flight.
  - `overlap`, the default, as today, starts another run.
  - `replace` aborts the run in flight and starts the new one, so a cascade's or a search's late answer
    never overwrites a newer one. The aborted request runs no route, and each of its `Finally` layers whose
    `WhileLoading` completed runs once (D10.23). A run it reaches in a `Finally` finishes that `Finally`
    (`Run.tla:ReplacedFinallyGoesOn`) and runs nothing after it (`Lanes.tla:ReplacedRunRunsNothingAfter`).
  - `skip` starts nothing while a run is in flight, so a save clicked twice sends once.
- D11.8 Dispose ends its owner's runs first, then releases what the owner holds.
  - A request in flight is aborted through its fetch's abort signal and runs no route. Each of its `Finally`
    layers whose `WhileLoading` completed runs once, and its busy targets are released after the defaults'
    (`Run.tla:ReleasedWhenEnded`). A retry's wait ends (D10.15).
  - An await the browser cannot abort (a `MethodAsync` promise, `Confirm`, a hub's `invoke`) settles
    later, and its settling runs nothing.
  - No route and no step after runs for a disposed owner.
  - Every `Finally` runs before the owner releases its listeners, components and content, while its
    elements are still on the page. So a `Finally` that clears what its `WhileLoading` showed, or re-enables a
    button in the slot, finds what it names. A `Finally`'s steps after its own first awaited step do not run for a
    disposed owner, so the run never waits again (`Run.tla:DisposedRunNeverWaits`). A request a disposed run's
    `Finally` reaches sends nothing, and its own `Finally` runs.
- D11.9 No step waits for time. The only waits the core owns are a retry's delay and a `Retry-After`
  (D10.14). A component's own timers, such as a toast's time shown, are the component's; the busy indicator has none
  and hides only when its count returns to 0 (D10.21).
- D11.10 Every stage is a block, as D8.7 and D8.8 state, on every view kind, MVC 5 and ASP.NET Core MVC (the owner,
  2026-11-08). The stages are closed: a handler of any trigger (on load, an element's or a component's event, a
  declared app event, a dispatch, a server push, a hub message), each branch (`Then`, an `ElseIf`'s, `Else`), each
  `Parallel` branch, a request's `WhileLoading`, `Finally`, each route (`OnSuccess`, `OnError` by status, the
  catch-all) and the block `Chained` runs, a `listen` (D12.8), and each block of the request defaults
  (`WhileLoading`, `Finally`, each error route). Each is a `Pipeline<TModel>`, the defaults' a `Pipeline<NoModel>`.
  - A block holds any number of steps of every module: a set, a call, an element's text, HTML and classes, a
    condition, a request, `Parallel`, `Confirm`, `Into`, `Unload`, `Validate`, a dispatch, a notice, and in a route
    `ShowValidationErrors`. They are written as a chain or as statements in a `{}` body and run in authored order (D11.1,
    D11.5), and each reads every value the blocks around it bind (D8.8).
  - A block's C# runs once, where the view renders: a statement writes a step, and a loop over the view's data
    writes a step for each item. C#'s own branching (`if`, `switch`, `?:`, a switch expression, `try`, `throw`,
    `return`, `goto`, `lock`, `using`) is refused by ALIS004, since it reads like a browser condition and runs once at
    render; `When` is the door (N18.6).
  - A step is written on its own block's pipeline. A step on the pipeline of a block around it is the block
    analyzer's (N18.9); a read of that pipeline's sources (`PageUrl`, an element's text) stays free.
  - Proof: `spec/api/HttpSurface/EveryStage.cs` writes every module in every stage, and `ManyStatementsInEveryStage`
    writes several statements in each, a request with steps after it in a handler, a route and a `Chained` block, a
    condition inside a route inside a condition, and an inner block reading the outer reply.
    `spec/api/CshtmlStages/Views/Stages/Index.cshtml` writes the same stages and a row action's loop in an ASP.NET Core
    MVC view the Razor SDK compiles at build, and `spec/api/CshtmlStages/Program.cs` the defaults' blocks in a startup
    file; an MVC 5 view is compiled where it first renders, so it stays the verify-net48 run's.
- D11.11 A pipeline runs as one run, a state machine (`spec/Execution/Run.tla`). Its frames are the blocks and requests
  in progress, innermost last, and its status is running, waiting or ended. Only the run's driver continues on a
  promise, in one place, its wait: a step completes, opens a frame, waits on one promise or stops, and the driver runs
  steps in the trigger's turn until one waits. So a run with no awaited step ends in that turn, the second handler of
  an event starts in it (`Run.tla:SyncRunEndsInItsTurn`, `Run.tla:SecondHandlerStartsInTheTurn`), and while a frame
  waits no frame around it runs (`Run.tla:NothingRunsWhileItWaits`). A call that returns a promise its declaration
  does not await stops its step, which handles that promise's rejection and continues nothing on it (D4.3, D4.4).
  - The C# refuses each payload write the run would make after the vendor's turn, by the path the run takes: a
    request waits at its send, its `WhileLoading` runs after the defaults' `WhileLoading`, and the steps after an `if`
    run after any branch's wait (D11.4, `Run.tla:WritesAfterTheTurnAreRefused`).
  - A stop unwinds the frames innermost first, and each request frame on the way runs its `Finally` layers as frames
    of their own (D10.23, `Run.tla:NothingRunsAfterAStop`).
  - An abort, by `replace` (D11.7) or a dispose (D11.8), unwinds the same way from the wait it lands on and runs no
    route outside a `Finally` (`Run.tla:NothingRunsAfterAnAbort`). Under `replace`, a run waiting anywhere inside a
    `Finally` (in its branch, or in a request inside it) goes on to that `Finally`'s end, that request's route included
    (`Run.tla:ReplacedFinallyGoesOn`). Under a dispose, each `Finally` runs to its first awaited step, whose settling
    runs nothing, so the run never waits again; a dispose after a `replace` overrides it
    (`Run.tla:DisposedRunNeverWaits`).
  - An abort the run's own step causes lands when that step returns, before the next (`Run.tla:NothingRunsAfterAnAbort`):
    a step that loads or unloads the slot its owner holds, whose release waits for that return (D12.4), and a step
    that raises its handler's own event under `replace`. The new run starts after the old run's cut has landed and its
    `Finally` layers have run to their first awaited step, as when a `replace` lands on a wait, so what an old
    `Finally` does before an await never undoes what the new run's `WhileLoading` shows. What it does after an await
    runs after that `WhileLoading`, since under `replace` it goes on to its end (X11.22): an element an app shows in
    `WhileLoading` and hides in `Finally` after an await is hidden by the old run while the new one loads, and the
    door for that is the busy indicator, which the core counts per target (D10.21, `spec/RequestsAndResponses/Composition.tla`). A
    hide before the old `Finally`'s first await runs before the new run starts. Neither TLA+ spec models the second
    run, so the order is X11.24's, a Playwright promise of the rewrite's run unit.
  - `all` ends when each of its blocks ended (D10.10, `Lanes.tla:JoinAfterEveryBranchTail`).
- D11.12 The core's state machines are closed, each held in one place, its transitions its TLA+ spec's actions:
  - a run (D11.11, `spec/Execution/Run.tla`), under its handler's policy and its owner's dispose (`spec/Execution/Lanes.tla`);
  - a request (D10.23, `spec/RequestsAndResponses/Request.tla`), a frame of its run;
  - an owner and its slot (Q12, `spec/OwnershipAndState/Ownership.tla`);
  - a connection (Q21, `spec/Realtime/Realtime.tla`);
  - a busy target's count (D10.21, `spec/RequestsAndResponses/Composition.tla`).
  Each machine's states have one set of names, which every spec and the runtime's types use:
  - a run is `idle`, `running`, `waiting` or `ended`, and ends completed, stopped, replaced or disposed (D20.6); a
    trigger the `skip` policy refuses leaves its run `idle` (D11.7);
  - a request is `idle`, then in D10.23's states, and `done` however it ended;
  - how a request's body ended is its completion, `completed`, `stopped` or `aborted`, as ECMA-262's completion record
    carries how a `try` block ended into its `finally`. It is one closed value, carried by `finally`, `defaultFinally`
    and `done` and read where they exit, never a boolean beside every state. A run's cut, `replace` or `dispose`, is
    the run's own state, and a dispose overrides a replace (D11.7, D11.8). Each block holds an abort signal, a
    `Finally` layer a new one and any other block its enclosing block's; a `replace` aborts the signal of each block
    outside the outermost `Finally` it lands in, and a dispose every signal of the run, a `Finally` layer's that runs
    after it included, so a send is aborted exactly when `Run.tla`'s `Abrupt` marks its frame. One signal per run cannot carry this: a signal keeps its first reason
    (`probe:abort-signal-keeps-first-reason`), and a fetch under an aborted signal rejects and sends nothing
    (`probe:fetch-aborted-signal-sends-nothing`), so a `Finally`'s request after a `replace` would never be sent;
  - a spec that models part of a machine names the states it models by that machine's names and steps over the
    states that change nothing it checks. So `Composition.tla`, `Realtime.tla` and `Ownership.tla` step from `idle`
    to `attempt`, a run's wait at `attempt` is `Tracing.tla`'s `waiting`, and a route waiting on its tail is
    `Lanes.tla`'s `route`. `Run.tla` names a layer's state whose block is open by the state and `Running`
    (`routeRunning`) and keeps of a frame's completion the one fact its steps read, `abrupt`, a stop's or an abort's;
    `Request.tla`, which models no abort, keeps `stopped`.
  Nothing else holds state across a turn: a step reads the page when it runs (D10.18), a listener is a removal its
  owner holds, a plan's boot is one pass, and the C# writer's position is its stack of open blocks, each with its stage
  and whether a step before it awaited (D11.4).

**Schema nodes.** `do` and `let` with `await`, `all`, `request`, and the handler's policy, a field of `Handler`
(Q19). The abort signals are the run's blocks' (D11.12), not the plan's: `HttpSend` carries none.

**Checker.**
- `Lanes.tla:DisposedRunsNothingMore`
- `Lanes.tla:FinallyRunsBeforeRelease`
- `Lanes.tla:ReplacedRunRunsNoRoute`
- `Lanes.tla:ReplacedRunRunsNothingAfter`
- `Lanes.tla:SkippedTriggerStartsNothing`
- `Lanes.tla:AllSettledAfterEveryRequest`
- `Lanes.tla:BranchStopEndsOnlyItself`
- `Lanes.tla:JoinAfterEveryBranchTail`
- `Run.tla:WritesAfterTheTurnAreRefused`
- `Run.tla:NothingRunsWhileItWaits`
- `Run.tla:SyncRunEndsInItsTurn`
- `Run.tla:SecondHandlerStartsInTheTurn`
- `Run.tla:NothingRunsAfterAStop`
- `Run.tla:NothingRunsAfterAnAbort`
- `Run.tla:FinallyRunsWhenLoaded`
- `Run.tla:FinallyOnlyWhenLoaded`
- `Run.tla:FinallyRunsOnce`
- `Run.tla:InnerFinallyFirst`
- `Run.tla:ReleasedWhenEnded`
- `Run.tla:DisposedRunNeverWaits`
- `Run.tla:ReplacedFinallyGoesOn`
- `probe:abort-signal-keeps-first-reason`
- `probe:fetch-aborted-signal-sends-nothing`
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
  `excelExport`, `csvExport` and `pdfExport` returning a Promise (EJ2 33.1.47's `grid.d.ts`, lines 3849, 3859 and
  3870). Under D11.3 each such call stops its pipeline until the
  declaration says `MethodAsync`.
- G9: an app event is declared once and shared by its dispatcher and its listener
  (`Alis.Reactive/PlanAuthoring/Events/AppEvent.cs:41-43`), so a dispatch is a trigger like any other (D11.6).

**Defects.**
- `X11.1` analyzer: a handler that writes a property of its payload after an awaited step (an async method, a
  request, a `Confirm`) is refused where the view renders with "A write to the event's payload after an awaited
  step reaches no vendor: write it before the first request, Confirm or async method." At build by ALIS032, and where the view renders on code it does not follow (D18.6): new:N18.9.
- `X11.2` behavior (D11.2): a drawer whose close handler writes `Cancel` and then asks stays open while the dialog
  asks, and closes on yes.
- `X11.3` behavior: a method declared with `Method` whose vendor returns a promise stops its pipeline, and the
  step after it does not run. The console holds the stop's one error (D11.3), which a test does not read
  (`.claude/rules/tests.md`, rule 4).
- `X11.4` behavior (D11.2): a `MethodAsync` that rejects stops its pipeline, its request's `Finally` runs, and its busy
  targets hide.
- `X11.5` behavior (D11.6): a slot unloaded while its request is in flight runs no route of that request, and its busy
  targets hide.
- `X11.6` behavior: under `replace`, a cascade whose first answer arrives after the second shows the
  second's items.
- `X11.7` behavior: under `skip`, a save clicked twice while its request is in flight sends one request.
- `X11.8` behavior (D11.4): a handler writes a payload member before its first await, which the vendor reads when its
  listeners return, then sends a request and in its success route calls a payload method with the answer: the
  component shows what the method was given.
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
- `X11.14` behavior (D11.5, D10.3): a success route that writes, sends a second request and writes again: the
  request's `Finally`, its `Chained` and the step after the request run after the second request settles and the
  route's last write shows (`Run.tla:NothingRunsWhileItWaits`).
- `X11.15` behavior (D10.3, D11.7): a second click under `replace` while a route's own request is in flight, both
  requests taking the defaults: that request is aborted, and its `Finally`, its defaults' `Finally`, the outer
  `Finally` and the outer defaults' `Finally` run in that order, once each, and no step of either route runs after
  (`Run.tla:InnerFinallyFirst`, `Run.tla:NothingRunsAfterAnAbort`).
- `X11.16` behavior (D11.5): `Parallel` whose first branch's route sends a request: the steps after `Parallel` run
  after that request settles (`Lanes.tla:JoinAfterEveryBranchTail`).
- `X11.18` refusal (D11.4): in an app whose defaults' `WhileLoading` awaits, a request whose own `WhileLoading` writes
  its event's `Cancel` is refused where the view renders with "A write to the event's payload after an awaited step
  reaches no vendor: write it before the first request, Confirm or async method." (`Run.tla:WritesAfterTheTurnAreRefused`).
- `X11.19` behavior (D11.11): a click whose handler runs no awaited step and a second handler of the same click both
  write before the click's own handling reads them: the second starts in the click's turn, and the first has ended
  (`Run.tla:SyncRunEndsInItsTurn`, `Run.tla:SecondHandlerStartsInTheTurn`).
- `X11.20` behavior (D10.23, D9.1): a stop in the route of a request a route sends: the inner request's `Finally` and its
  defaults' run, then the outer's own and its defaults', and no step after either request runs
  (`Run.tla:NothingRunsAfterAStop`).
- `X11.21` behavior (D10.3, D11.7): a second click under `replace` while the first request's own `WhileLoading`
  awaits an async method: that request's `Finally` does not run, its defaults' `Finally` does, its busy target is
  never shown and no route runs (`Run.tla:FinallyRunsWhenLoaded`, `Run.tla:FinallyOnlyWhenLoaded`).
- `X11.22` behavior (D11.7): a second click under `replace` while the first request's `Finally` awaits an async
  method: that `Finally` writes its last text once, after its await settles, its defaults' `Finally` runs after it,
  and no step after the request runs (`Run.tla:FinallyRunsOnce`).
- `X11.23` behavior (D11.8): a slot unloaded while its request's defaults' `Finally` awaits: the step after that
  await does not run, and the request's busy target hides (`Run.tla:DisposedRunNeverWaits`,
  `Run.tla:ReleasedWhenEnded`).
- `X11.24` behavior (D11.11): under `replace`, a bell's handler logs "rang" and its round, then sends a request whose
  route rings the same bell on round 1 and whose `Finally` logs "finally" and the round, then logs "after" and the
  round: the log reads "rang 1 finally 1 rang 2 finally 2 after 2", the old run's `Finally` before the new run's first
  step and no step after the old request (`Run.tla:NothingRunsAfterAnAbort`).
- `X11.17` analyzer (D11.10): C#'s own branching (`if`, `?:`, `switch`, `try`, `return`) in any block: ALIS004,
  new:N18.6.

## Q12. Ownership

**Decision.**
- D12.1 The owners are closed: the page and a slot. A slot is an element some plan loads into, named in the
  plan by its container id (the `alis.slot` key), never guessed from the page. A plan is not an owner. Each
  plan belongs to one owner: the slot whose container is the innermost around the plan's script, else the
  page. The page is never released.
- D12.2 What a plan asks for belongs to its plan's owner:
  - its handlers' listeners, on any element or object, the page's included;
  - its holds on a hub or a stream;
  - its registrations, with their rules, and its containers, each with its summary and live check
    (`Ownership.tla:RulesLeaveWithTheirOwner`);
  - its runs (Q11).
- D12.3 What HTML carries belongs to the owner of the innermost slot around it, else the page:
  - the components mounted in it, which construct on `alis:mount` and destroy themselves on `alis:unmount`;
  - the slots whose containers it holds;
  - the plans whose scripts it holds, content the server rendered inside a slot's container included. Such
    content belongs to that slot from boot, so the slot's first load or unload releases it.
- D12.4 A slot's load replaces its content, and its unload empties it. Before either, the slot releases every
  owner whose container is inside its content, innermost first, then itself. An owner's release runs in a
  closed order, and what each step calls is closed:
  1. it ends its runs, and each `Finally` runs (Q11, D11.8). An abort runs its signal's abort steps, the fetch's own
     algorithm and an `abort` event, a dispatch as in step 2 (DOM Standard 3.2), and what a `Finally` meets is its
     own run's (Q11), so this step throws nothing to the release;
  2. it announces the unmount of its content with `dispatchEvent`. The DOM Standard's inner invoke reports a
     listener's exception and goes on to the next listener (DOM Standard 2.9, inner invoke, step 2.13.1: "Report
     exception"), so a component whose unmount throws reaches the page's `error` event and the console, never the
     release;
  3. it removes its listeners. Each removal runs the vendor's code, which can throw: an unsubscribe method, a returned
     remover, a callback property's read and put-back (EJ2's `@Event` setter calls the component's observer, which
     throws once the component is destroyed, `probe:ej2-event-put-back-after-destroy`), and D12.5's check, which reads
     the vendor's object path or calls its lookup (D17.3);
  4. it releases its holds. The last hold of a connection closes it: `EventSource.close()` throws nothing (HTML
     Standard 9.2.2), a hub's `stop()` is an async function, so its rejection settles after the release and is logged
     as one error naming the hub, and the reconnect offer's `hide()` (D21.11) runs a component's code, which can throw;
  5. it drops its field registrations, the core's own bookkeeping, which calls nothing.

  Every step of every owner runs, whatever a step or an owner before it threw, so an inner owner's error stops neither
  the owner around it nor a sibling (gate 1's P3). The release keeps its own bookkeeping before each vendor call (the
  owner leaves its slot, a hold leaves its count, a listener is marked released, D12.5) and keeps each error with its
  owner and its step. Then the content is replaced, which throws to no one: a custom element's callbacks are reactions
  the HTML Standard invokes with "report" (HTML Standard 4.13.6), and `alis:mount` is a dispatch, as in step 2. A load
  mounts the content and starts its new owner, which subscribes once, so no listener piles up across loads, a throwing
  release's included. Then the step that loaded or unloaded stops its pipeline with one error that names each owner
  and the step that threw, and each `Finally` of the requests the step is inside runs, as any stop runs them. The page
  is consistent, every owner released and the new one started, and the pipeline does not go on as if the release had
  been clean. This is no catch-and-continue (principle 8): no error is dropped or given a default, and each reaches
  the pipeline whose step asked for the release, which stops.

  A release starts at two steps: a load (`Into`, D10.11) and an unload (`Unload`, D12.9). A slot nested in the content
  is released by that step's release, and content the server rendered in a slot by the slot's first load or unload
  (D12.3), so their errors stop that one step; the page is never released (D12.1). When the step's run belongs to an
  owner its release ends (a card's `Unload("card")` from the card, or the card's `Into` of the slot around it), the
  release waits for that step to return: the run's cut lands there (D11.11), its `Finally` layers run once each, to
  their first awaited step, the run ends, and then the release runs, so each `Finally` runs before the unmount (D11.8)
  and nothing after the step runs (`Run.tla:NothingRunsAfterAnAbort`, `Lanes.tla:FinallyRunsBeforeRelease`); the
  release's errors are then logged as one error, since the run that asked for the release has ended.
  `spec/OwnershipAndState/Ownership.tla` checks the release over the interleavings of a nested slot and the slot around it, each
  step issued by the page or by the slot it releases: every owner released past a throw
  (`EveryOwnerReleasedPastAThrow`), a raise running no listener whose removal threw (`ThrownListenerRunsNothing`),
  and the content replaced (`StepReplacesTheContent`). That the step stops after a release that met a throw, and that
  its `Finally` runs once, are browser promises (X12.15, X12.17), since no spec models a release inside a run.
- D12.5 Release removes exactly what the owner holds, and nothing another owner holds. A listener is removed only while
  its element still holds, where the vendor's `locate` finds it (D17.3), the object the listener was added to. A
  component its host destroyed took its listeners with it, and the owner calls nothing on it. EJ2's destroy takes the
  component off its element (`probe:ej2-destroy-removes-instance`), so the rule names no vendor. Main's check of
  Syncfusion's `isDestroyed` in the runtime is refused for that reason. A handler holds the object it subscribed to when
  its plan started (D12.4), so after a restart (its host destroys the component and puts a new object where the vendor's
  `locate` finds it on the same element) the new object hears none of the plan's handlers, and the release calls nothing
  on it. A `listen` step that reaches the new object adds its own listener, as for any other object (D12.8). An element
  that no longer holds the object a handler subscribed to is read as one whose host destroyed that object, listeners and
  all. A listener is marked released before its removal runs, so one whose removal threw, which its vendor still holds,
  runs nothing when its object raises, and a callback property whose put-back threw keeps the framework's function,
  which calls the value it replaced and no listener (D17.3). A check that throws is that listener's error, and its
  removal is not called.
- D12.6 A hub or a stream is held by every owner whose plans name it. The connection opens at the first hold
  and closes at the last release, and its retry offer goes with it. A step reaches only a held connection
  (`Ownership.tla:HubKeptWhileHeld`).
- D12.7 Live validation is one capture-phase listener per page. An input an event reaches is checked by the
  live checks of the containers that hold it (D16.15). Nothing is wired per element, so nothing per element
  is released. A registration and a container leave with their plan's owner, so a slot's own checks leave with
  the slot. What a check reads for a member whose input is gone is Q16's.
- D12.8 An event of a live object a pipeline reaches (an object an event's payload holds, an object a method
  returns, an item of a list) is listened to by a step written in that pipeline, `listen`, with a vendor's
  listening, a payload declaration and a policy, as a handler has. Its listener:
  - belongs to the owner of the plan whose pipeline added it, and leaves at that owner's release (D12.5);
  - is added once per object and step: the same object reached again adds no second listener, and another
    object gets its own;
  - holds its object weakly, and its listener too, since the listener's scope can hold the payload that holds the
    object: the object lives as long as its vendor keeps it, at release a living object's listener is removed by its
    listening kind, and a collected object took its listener with it. A returned subscribe's remover is the vendor's
    own function, held until release with whatever it holds;
  - runs its own pipeline, whose payload is its own listener's arguments, bound under a name of its own, and
    which reads the bindings of the blocks around the step, as any block inside another does (schema.md);
  - is refused where the view renders for a write to the payload of the event around the step: the vendor read
    that payload when its listener returned, before the listened object raised (D11.4). A payload that is a
    component takes the write, as D11.4 states. Its own pipeline starts
    in the listened object's turn, so a write to its own payload before its own awaited step reaches its vendor,
    whatever the pipeline around it awaited;
  - is refused where the view renders on an object declared optional or nullable: a listen needs its object when
    the step runs;
  - on a component an event's payload is or holds, is removed at release only while the element the component held when
    the step listened still holds it where the vendor's `locate` finds it, D12.5's rule: the vendor names where its
    object holds its element (D17.3), the step reads that element when it listens and holds it weakly beside the object
    and the listener, and a host that destroyed or restarted the component took the listener with it. A vendor whose
    destroy clears the element (Chart.js nulls its canvas) changes nothing, since the release never reads it again;
  - on a component reached by its element's id is the door a slot's plan has to a component the page rendered (gate 1's
    P5): the step holds that element weakly beside the object and the listener, and the release removes the listener
    only while the element still holds the object where the vendor's `locate` finds it, D12.5's rule, so its vendor
    needs no element path. A plan hears a component it renders with `.Reactive` from the plan's start; a `listen` on it
    starts when its step runs and leaves at its owner's release, the page's included (X12.14);
  - is refused where the view renders on a component a payload or a returned object hands over whose vendor names no
    element path, since its release could not tell a destroyed component from a living one;
  - stops the pipeline the step is in when its object lacks a listening member (a subscribe or an unsubscribe, a
    returned subscribe that gives back no remover, or a callback property it lacks or that refuses the framework's
    function, D17.3), as a member a declaration names must be there; the listener a returned subscribe added runs
    nothing. A handler meets the same when it starts: its unsubscribe is looked up with its subscribe, and a handler
    whose object lacks either, whose returned subscribe gives back no remover, or whose callback property is absent or
    refuses the framework's function, does not start and the listener that subscribe added runs nothing, since no
    release could remove what it added (the POC 14 decision in `spec/CLAUDE.md`). The build refuses each of these where
    it reads the listening (D17.5); an object frozen at run time, or a setter that throws, is met in the browser alone.
  The object's declaration carries its listening, a vendor's listening kind (D17.3), as a plugin's does: an
  `EventedObject` declares its `EventListening`, and a component, an `EventedObject` too, has its vendor's.
  Ownership.tla carries the step (`ListenA`) and a collection (`Collect`) under three invariants, each with its
  mutant: one listener per object, gone at its owner's release, and nothing called on a collected object. They are
  X12.9, X12.10 and X12.18, whose test forces the collection with Playwright's `Page.RequestGCAsync` and confirms it
  by the page's own weak hold before the release, as a timer runs on Playwright's clock.
- D12.9 A slot's content is what its loads put there. A step that sets the text or the HTML of an element a plan
  loads into or unloads would leave what the slot loaded running under content it no longer shows, and D14.6's
  split refuses it: within one plan where the view renders (R14.4), and across plans the step stops in the browser
  with one error naming the slot (R14.7), on the slot's element or on an element that holds it, which only the
  browser sees. No server object sees every plan of a response (D14.1). `Unload(element)` empties a slot.
  - An element's `Text` and `Html` mark their `set` as a content write (`content`, schema.md).
  - The browser holds each slot a live plan names, as its plan starts, and each slot whose content has a live owner
    (what a load or the server put there); each hold leaves with its owner. So content a closed card loaded into a
    slot outside it stays held until that slot unloads.

**Schema nodes.** The plugin data `form` and `hub`, the plugin `alis.slot` (data `none`, its key the container id),
the statement `listen`, and `Handler`. Ownership is not a node: a plan's owner is where its script is, and an owner's holds are what its
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
- `Ownership.tla:ListenOncePerObject`
- `Ownership.tla:ListenLeavesWithItsOwner`
- `Ownership.tla:NoRemoveOnCollected`
- `Ownership.tla:EveryOwnerReleasedPastAThrow`
- `Ownership.tla:ThrownListenerRunsNothing`
- `Ownership.tla:StepReplacesTheContent`
- `Lanes.tla:FinallyRunsBeforeRelease`
- `Run.tla:NothingRunsAfterAnAbort`
- `probe:ej2-destroy-removes-instance`
- `probe:ej2-destroy-sets-is-destroyed`
- `probe:ej2-event-put-back-after-destroy`

**Doors.**
- kept: `Into(element)`, `Unload(element)`, and the hub's and stream's triggers; `Html.RenderPlan` leaves, since the
  plan block's end writes the plan (N14.2).
- No door names an owner: a developer chooses where a plan renders, and ownership follows.
- `N12.1` new: `listen`, the handler of an event of a live object a pipeline reaches (D12.8); its form goes to
  gate 2. It replaces the refusal of an event declared on an object a method returns, and the refusal of a listen on
  a component reached by its element's id (gate 1's P5). POC 14d's form:
  `pipeline.Listen(object, o => o.Event, (args, p) => ..)` on an `EventedObject`, a `JsObject` whose constructor
  takes its `EventListening`; a listen on an object whose declaration carries no listening does not compile.

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
- Both lines subscribe a handler once, to the object its element holds when its plan is wired, and remove the
  listener from that object alone, so the new object a restart puts on the same element hears none of the plan's
  handlers: D12.5. Main reads the object
  (`Alis.Reactive.Assets/runtime/browser-objects/component-driver.ts:81`, through
  `Alis.Reactive.Assets/runtime/events/resolver.ts:16-21`) when a plan's behaviors are wired
  (`Alis.Reactive.Assets/runtime/lifecycle/boot.ts:38`) and adds and removes its listener on that object
  (`Alis.Reactive.Assets/runtime/events/event-fusion.ts:23-37`); the redesign subscribes each handler as its owner
  starts (`Alis.Reactive.Assets/runtime/plans/start.ts:46`) and removes it only while the element still holds the
  object it read then (`Alis.Reactive.Assets/runtime/objects/plan-objects.ts:37-42`).
- An owner runs its disposers in reverse and knows nothing of runs (`Alis.Reactive.Assets/runtime/objects/owner.ts:32-40`).
- A release that meets a throw has three answers today, and D12.4 gives a fourth. The redesign catches and logs each
  disposer's error and goes on, so every owner is released and the step that released goes on as if the release had
  been clean (`Alis.Reactive.Assets/runtime/objects/owner.ts:32-40`), and it drops a hub's `stop()` with `void`
  (`Alis.Reactive.Assets/runtime/realtime/hub.ts:101-108`). Main replaces the content before it unloads the slot's
  plans (`Alis.Reactive.Assets/runtime/execution/partials/inject.ts:30-42`) and removes a component's listener from
  its signal's `abort` event (`Alis.Reactive.Assets/runtime/events/event-fusion.ts:19-37`,
  `Alis.Reactive.Assets/runtime/lifecycle/applied-plans.ts:73-80`), so a removal that throws is reported by the DOM
  and the rest run, and a listener whose removal threw still runs. The POC lets the first throw end the release, so
  the slot never empties and the next load never runs (14c's C2, `spec/NOTES.md:491-492`).
- A slot announces the unmount of its content before replacing it (`Alis.Reactive.Assets/runtime/plugins/slot.ts:24-38`).
- The redesign refuses a plan that sets a slot's text or HTML, across the plans one response renders
  (`Alis.Reactive/Razor/RenderedPage.cs:70-87`, `Alis.Reactive/PlanAuthoring/Elements/HtmlElement.cs:66-69`): D12.9.

**Defects.**
- `X12.1` behavior (D12.2): after the outer slot unloads, a click on a page button that an inner slot's plan listened to
  runs nothing.
- `X12.2` behavior: after a slot loads three times, a click on its button runs its handler once.
- `X12.3` behavior: a field a reloaded slot renders shows its message on blur.
- `X12.4` behavior (D16.3, D16.5, D16.6): after a slot that rendered the input of a member the page's validator
  requires unloads, the page's save reads that member's start, as the endpoint binds a body that lacks it, and the
  page's rule holds the save with its message in the page container's summary; the slot's own validator's
  registration leaves with the slot (D16.3), so its rules check nothing after it.
- `X12.5` behavior (D12.6): two slots listen to one hub; after one unloads, the other still shows the server's pushes.
- `X12.6` behavior (D12.6): unloading the slot whose hub the server hung up on hides the retry offer.
- `X12.7` behavior: a slot whose component its host destroyed unloads with no console error, and the next load
  listens.
- `X12.8` behavior: content the server rendered inside a slot's container stops listening at the slot's first
  load.
- `X12.9` behavior: an object a payload hands over raises its own event, and the pipeline its `listen` step names
  runs once per raise; handed over again, it still runs once per raise.
- `X12.10` behavior: after the slot whose handler listened to a payload's object unloads, that object's event runs
  nothing.
- `X12.11` refusal (D12.9, R14.4): a plan that sets the text of an element it loads into, or the HTML of one it
  unloads, in either order: "The plan sets the text of element 'card', a slot this plan loads or unloads, so what
  the slot loaded would keep running under content it no longer shows. Empty the slot with Unload(\"card\"), and
  write the text into an element beside it." The HTML's reads "the HTML" for "the text".
- `X12.12` behavior (D12.9, R14.7): a card's step that writes over the slot the card was loaded into stops, and so
  does a page's step that writes over an element holding that slot, or over a slot an open card's plan unloads; the
  steps before it ran and the steps after it run nothing; so does one that writes over notes a closed card loaded
  into a slot outside it. Once the only plan that named a slot leaves and the slot holds no loaded content, a step
  writes over it.
- `X12.14` behavior (D12.8, D14.2): a card a slot loads listens to the page's monitor, reached by its element's id, and
  shows each reading the monitor raises; after the slot unloads, the monitor's next reading changes no card text,
  and the page's own `.Reactive` handler of the same event still shows it.
- `X12.13` behavior (D12.5): after a monitor's host restarts it, destroying the component and putting a new object
  on its element, the new object's event runs none of the plan's handlers, and closing the card that held it calls
  nothing on it.
- `X12.15` behavior (D12.4): a card whose inner slot's plan listens to a page plugin whose unsubscribe throws is loaded
  over: the new card shows and its button runs its handler once, the plugin's event runs neither the old card's
  handler nor its inner slot's, and the step after the request does not run while the request's `Finally` does
  (`Ownership.tla:EveryOwnerReleasedPastAThrow`, `Ownership.tla:StepReplacesTheContent`).
- `X12.16` behavior (D12.5): a card whose page plugin's unsubscribe throws is closed and opened again: the plugin's
  event runs the card's handler once (`Ownership.tla:ThrownListenerRunsNothing`).
- `X12.17` behavior (D12.4, D11.8): a card's save whose success route unloads the card, while the card's page
  plugin's unsubscribe throws: the request's `Finally`, which writes a note in the card and then the page's status,
  writes the status once, so it ran while the card was on the page; the card empties, and opening it again loads it
  (`Run.tla:NothingRunsAfterAnAbort`, `Lanes.tla:FinallyRunsBeforeRelease`).
- `X12.18` behavior (D12.8): a payload's object a slot's `listen` step listened to, dropped by its host and collected,
  is let go: the page's own weak hold of it reads collected, and the slot's unload calls nothing on it
  (`Ownership.tla:NoRemoveOnCollected`).

## Q13. Where state lives

**Decision.**
- D13.1 Browser state lives in exactly one of four places, and each concept has one:

  | Concept | Lives in |
  |---------|----------|
  | the table of inputs by element id, each entry under the owner that contributed it | the page |
  | a hub's or a stream's connection, by URL, with its holders (Q12) | the page |
  | the contracts `busy`, `notice` and `reconnect`, by plugin name (D10.21, D21.11), and the app-level confirm and drawer, by fixed id | the page |
  | the one live-validation listener (D12.7) | the page |
  | a validator's registration under a rule set, its fields by binding name, item fields by `[*]` | the plan that renders the request or the container that checks by it, keyed by validator and rule set |
  | the table of containers by element id, each with its model, its kind, its summary and its live check | the page, each entry under its owner |
  | the inputs a container sends or checks: those inside its element at that moment | the container |
  | the inputs a request sent, by binding name, each as the inputs table held it | the request, until it settles |
  | a request's failures: every one in its summary place, each also at the input it sent under that key | the request's container's summary, else the notice |
  | the summary, one per container, rendered by the container's block | the container |
  | listeners, holds, registrations and runs | the owner (Q12) |
  | a plan's culture and time zone, written by C# for the view (D3.11, D3.12) | the owner, by its plan (M18.X1) |
  | the argument, the attempts, the response and the route of one request | the request, until it settles |

- D13.2 A plan's id keys nothing: it names the plan in traces only, and many plans of a page share one id freely.
- D13.3 A read of a member by its binding name belongs to a container. It reads the input inside the container's
  element that binds the name, as the container sends it. A page-wide read by binding name is refused: two views can
  bind one name, so on a page a name names no single input. That ends the chain of guesses that read the last
  contributed input, then the plan's own. A read of a member's input outside a container is `pipeline.Input(m =>
  m.Member)` (N14.4), by the element id Q14 derives the same way in every view that renders the member.
- D13.4 Two inputs a request or a check sends under one scalar binding name (two inside its container, or an
  `Input` and one inside its container) stop the request or the check, with one logged error that names both
  element ids. Form data binds the first value and JSON the last
  on both hosts (`probe:core-form-duplicate-name`, `probe:core-json-duplicate-name`,
  `probe:mvc5-form-duplicate-first`, `probe:mvc5-json-duplicate-last`), so the browser cannot know which one
  the endpoint binds. A list's inputs carry indexed names and are not duplicates.
- D13.5 A rule for each item is written once, by a path with `each` (D1.5). A check runs it on every item row its
  body holds, the rows inside the container it sends, whoever rendered the row: the page, a list partial, or a
  slot's content.
- D13.6 No plan renders a summary of its own. A container's one summary is rendered by the block that renders the
  container (Q14), and its messages outlive a slot that joined the container and left. A request that sends no
  container shows its failures in the notice (D16.13).

**Schema nodes.** The plugin data `form` (a registration, keyed by validator and rule set) and `model` (the
inputs table and the containers table),
and a request's state, which no node holds: it is its `request` statement's, in its run (D10.23, D11.11).

**Checker.**
- `Ownership.tla:RulesLeaveWithTheirOwner`
- `Ownership.tla:RegistrationsDoNotAccumulate`
- `probe:core-form-duplicate-name`
- `probe:core-json-duplicate-name`
- `probe:mvc5-form-duplicate-first`
- `probe:mvc5-json-duplicate-last`

**Doors.**
- kept: `Form(formId)`, its meaning narrowed to a native form's submission set and its argument the form's typed
  handle (D9.15, N18.3).
- refused: `Form()`, carried by `Form(form)` and `Inputs(container)`, which name what they send (D9.2); `Form(fields)`,
  carried by `Input(r => r.Member, input)` (N9.3); `Validate<TValidator>(formId)`, carried by a request's own check
  (D16.2) and the check with no request, `Validate(container)` (N16.3); `ValidationErrors(formId)`, carried by
  `ShowValidationErrors(problem)` (N10.7); `PageForm`, which no door returns, since a request's failures show at its own summary
  place (D16.13); `Html.FormSummary(formId)` (`Alis.Reactive/Razor/PlanExtensions.cs:98`), carried by the container's
  block (N14.3).
- `N13.1` new: a container's typed read of a member as the container sends it, the one read by binding name (D13.3); its
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
- `X13.1` behavior (D13.3): two partials of different models each render an input that binds `Value`, each in its own
  form; each card's save sends its own value.
- `X13.2` behavior (D13.3): a container holding two inputs that bind one name sends nothing on save, and the console names
  both element ids.
- `X13.3` behavior (D13.5, D16.14): a row a slot loads into the container is checked by the page's item rule when its request saves.
- `X13.4` behavior: a server message for a member an editor template rendered shows beside that input.
- `X13.5` behavior (D13.6): two containers on one page keep their own summaries, and a container's summary stays after a
  slot that joined it unloads.

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
  what another view rendered through the page's input table by element id, or through a container by binding
  name (Q13).
  - Main merged the plans of one plan id: components replaced by key, behaviors appended in order.
  - The rewrite carries each part. A component another view renders is located by its element id, with
    the vendor data of the reading plan's own typed declaration. Behaviors stay their own plan's. Rules are the
    registrations of the plan that checks by them, and messages are the request's (Q16).
  - So the plan id keys nothing (D13.2).
- D14.3 A member has one binding name and one element id in every view that renders it:
  - The binding name is MVC's full field name under the view's `TemplateInfo` prefix (`probe:core-template-prefix-name`,
    `probe:mvc5-template-full-id`).
  - The element id is the class that declares the member, then MVC's id of the full name: `{Namespace_Class}__{Path}`
    (`probe:core-sanitized-id`). The declaring class is the one the expression tree's member names.
  - A generic declaring class is named as C# writes it, with its type arguments' names, never with the assembly names
    and versions its full name carries in them (C11, `probe:net48-type-fullname-closed-generic`): its `Type.FullName` up
    to the first bracket, each level's arity mark replaced by that level's arguments from `Type.GetGenericArguments`,
    the outer level first (`probe:core-type-fullname-generic-prefix`). A type argument is named by its `Type.Name`, a
    nullable by its underlying type's name and `?` (`Nullable.GetUnderlyingType`), an array by its own `Type.Name`
    (`Int32[]`, ``List`1[]``), which `Type.IsArray` finds before `Type.GetGenericArguments` is read, since that gives an
    array its element's arguments (`probe:core-type-array-generic-arguments`), and any other generic type by its name
    and its arguments' names. So `ChartEntry<FluidsReading>` in `Alis.Reactive.Framework.App.Partials` scopes
    `Alis_Reactive_Framework_App_Partials_ChartEntry_FluidsReading_`, the redesign's id (X14.12), and a class nested in
    a generic one is `Holder<Int32>.Inner`, where the redesign's `CSharpTypeName` wrote `Holder<T>.Inner<Int32>`.
  - Two declaring classes share a scope only when they differ in a type argument's namespace alone, or in the
    arguments of an array's element type; a page that renders inputs of both meets R14.3 in one plan and R14.6 across
    plans.
  - So the page, a validator, an `EditorFor` template, a list-item partial and a partial for a member reach
    one id without knowing each other, and two views of different models that bind one name keep two ids.
  - A lambda that names no member (`m => m`), and a member an interface declares, are refused. `Type.IsInterface` finds
    the interface, and `Type.GetInterfaceMap`, which would map the member to the class's, is refused (C11, X14.13).
  - An input binds a member of a class the app's JSON contract writes as an object, one rule on both hosts, since D2.4
    reads the member's body path from that contract on net10: a type the contract writes as one value (`decimal?`,
    `string`, `DateTime`, a class with its own converter) or as a list (`List<T>`, an array) holds no member there, so
    an input of its member (`Value`, `Length`, `Count`) is refused (`probe:core-contract-kind-declarers`). A member `Nullable<T>` declares is
    refused naming the nullable member to bind (X14.14), every other one naming the type and how the contract writes it
    (X14.15). Chosen within the owner's yes at gate 1: C11 lifts the refusal of a generic declaring class, which also
    refused `m => m.Dose.Value` and `m => m.Items.Count`, with the generic class's message. The owner can overrule it.
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
- D14.4 A partial is rendered for a member by one typed door, `Html.PartialFor(m => m.Member)` (N14.1), on both
  hosts. It renders the view named after the member's type, found where MVC finds a view by that name on each host,
  as `EditorFor` finds a template by type; `Html.PartialFor(m => m.Member, view)` names the view, for a type with
  several views or a generic type, which names none. A view name is a string kind, MVC's own locator (gate 2's G6),
  and a name no view answers fails where the view renders, with MVC's own error.
  - The member's value is the partial's model, and a new `TemplateInfo` carries the member's full binding
    name as the prefix.
  - A list item, `m => m.Members[i]`, takes `Members[i]`, a position (Q1). A dictionary entry,
    `m => m.Doses[key]`, takes `Doses[key]`.
  - It replaces the idiom of copying `ViewData` and setting `TemplateInfo.HtmlFieldPrefix` (16 view lines at
    65fdf118d). On net48 that idiom moves the view's own prefix, since a copy shares `TemplateInfo`
    (`probe:mvc5-viewdata-copy-shares-template-info`); on net10 a copy has its own
    (`probe:core-viewdata-copy-template-info`). Inside a plan's block on net48 the idiom is refused where the block
    ends (R14.2, X14.16).
- D14.5 A dictionary entry keyed by a number has no JSON form, since JSON reads a number in the prefix as a list
  position. A JSON request whose body carries a member under such a prefix is refused where the view renders,
  and form data sends it.
- D14.6 The refusals of partials and ids are closed. Within one plan they are refused where the view renders.
  Across plans, the page meets them where it composes the plans, and each is one error there.
  - Composition is a real boundary: the plans were rendered apart, no plan can state what another holds,
    and no server object sees every view of a page (D14.1).
  - That moves two refusals from render time to the browser, confirmed at gate 1 (C29).
  - At render, within one plan:
    - R14.1 an input or a partial whose lambda MVC's name does not name step by step from the model (a call that
      is no indexer, a value the view holds), an input whose lambda names no member, a member an interface declares,
      or a member of a type the app's JSON contract writes as one value or a list (D14.3);
    - R14.2 an input, or a pipeline's read of one, under a prefix its view took after its plan started, and a plan's
      block that ends under such a prefix, since MVC's own doors after the move (`EditorFor` for the next member)
      render under it while no input of the view need follow (`Partials.tla:PageReadFindsEachAttachedInput`);
    - R14.3 an element id the plan renders twice;
    - R14.4 a step that sets the text or HTML of an element the same plan loads or unloads as a slot;
    - R14.5 a JSON request over a number-keyed dictionary entry (D14.5).
  - At composition, in the browser:
    - R14.6 a plan holding an input id a live plan already holds is not started. One error names the id and
      both plans' views. A slot's content that renders an id the page holds is the same case.
    - R14.7 a step that sets the text or HTML of an element a live plan holds as a slot, or of an element that holds
      one, stops, with one error naming the slot (X12.12).
- D14.7 A plan is a block in its view: the plan door opens the block and the block's end writes the plan
  (N14.2).
  - The plan is a variable of the block, so an input outside the block cannot name it, and a plan that is
    never rendered cannot be written.
  - A block written whole inside a partial's `@section` renders nothing, inputs included, since MVC drops a
    partial's sections on both hosts (`probe:core-partial-section-dropped`,
    `probe:mvc5-sections-checked-in-layouts-only`). The page shows that defect.
  - A refusal by render order is refused: three commits guessed it, narrowed it and removed it.
- D14.8 A request container is a block in its view as well (N14.3): a native form, or an element of any other kind.
  It renders the container's element and its one summary, so no record of summaries rendered is needed (D13.6), and
  returns the container's handle, typed by the view's model: `RequestForm<TModel>` for a form, and
  `RequestContainer<TModel>` for any container, a form included. Another view reaches a container by
  `pipeline.Container<TModel>(containerId)`, which `Inputs` sends. A request that sends a container no live plan
  renders under that id and model stops before it sends, with one error naming the id and both models, as a plan
  composition defect (D14.6).

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
- `probe:core-type-fullname-generic-prefix`
- `probe:net48-type-fullname-closed-generic`
- `probe:core-type-array-generic-arguments`
- `probe:core-contract-kind-declarers`
- `Ownership.tla:BootContentBelongsToItsSlot`
- `Partials.tla:OneIdPerMember`
- `Partials.tla:PageReadFindsEachAttachedInput`
- `Partials.tla:AttachedInputBindsItsMember`
- `Partials.tla:ReadNeverFindsAnotherMember`
- `Partials.tla:EverySentValueIsTheOneBound`

**Doors.**
- kept: `Html.ReactivePlan`, `Html.InputField` (M7.N2), `IdGenerator.For<TModel, TProp>` (M7.N1), and MVC's own
  `Html.EditorFor`; the object-typed `IdGenerator.For<TModel>` both lines have leaves, since ALIS009 refuses `object` on
  the public surface.
- `N14.1` new: `Html.PartialFor(m => m.Member)` and `Html.PartialFor(m => m.Member, view)`, the typed partial for a
  member, on both hosts; the first renders the view named after the member's type (D14.4).
- `N14.2` new: the plan as a block of its view, whose end renders it; it replaces `Html.RenderPlan` on both lines;
  its form goes to gate 2.
- `N14.3` new: the request container as a block of its view, which renders the container's element and its summary
  and returns its typed handle (D14.8), and `pipeline.Container<TModel>(containerId)` for another view; their forms go
  to gate 2.
- `N14.4` new: `pipeline.Input(m => m.Member)`, the input a member binds, typed by the member's value as
  `BoundInput<TValue>`, which a request sends (`Input`, D9.15) and whose `.Component<T>()` reaches the component by the
  id D14.3 gives it. It replaces both lines' `Component<TComponent>(Expression<Func<TModel, object?>>)`, whose
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
- The redesign names a generic declaring class by `CSharpTypeName` (`Alis.Reactive/Members/CSharpTypeName.cs:8-17`),
  which reads `IsGenericType`, `GetElementType` and `GetArrayRank` beside C11's reads, and its scope reads `Namespace`
  and `DeclaringType` (`Alis.Reactive/Inputs/IdGenerator.cs:63-73`); it refuses an interface declarer
  (`Alis.Reactive/Inputs/IdGenerator.cs:52-57`). The POC refuses a generic declaring class and reads no contract for an
  input, and stands until the rewrite's inputs unit (`spec/NOTES.md`, 2026-11-09, C11).
- MVC's own doors that give a member's view its prefix need no framework door: on net10 the page's plan reads an input
  that `EditorFor` renders by type, by a template it names or the model's `[UIHint]` names, and for each item of a list,
  that MVC's `<partial for>` renders, and that a controller's partial renders under the member's prefix into a slot
  (`tests/Alis.Reactive.Poc.PlaywrightTests/Partials/WhenMvcRendersAMembersView.cs`,
  `tests/Alis.Reactive.Poc.PlaywrightTests/Partials/WhenAPartialIsRenderedForAMember.cs`); on net48 a template's
  prefix is the member's full name (`probe:mvc5-template-prefix`).
- A partial rendered with a model and no prefix names its inputs as MVC's own helpers do, under the view's prefix and
  no member's name, so its post binds nothing and the page's plan stops at the member's id: on net10 it keeps the
  view's prefix (`probe:core-partial-model-prefix`), and on net48 it starts with none, inside a template too
  (`probe:mvc5-partial-model-fresh-prefix`).
- `EditorFor` renders nothing for a model already rendered up its template chain, or for a null of a type the chain
  rendered as null, on both hosts (`probe:core-template-visited`, `probe:mvc5-template-visited`); `PartialFor` has no
  such guard.
- `spec/PartialsAndSlots/Partials.tla` renders a family's next of kin and guardian, two members of one type, each by one of MVC's
  doors (`EditorFor`, `PartialFor`, `<partial for>` on net10, the `ViewData` copy, a slot under the member's prefix, a
  partial with a model, a slot with none), each kin's address inside it by `EditorFor`, `PartialFor` or a partial with
  a model, on each host as its probes and sources state, slots loaded in either order. On every door that gives a member
  its prefix, the page's read finds the member's input, the post binds it to its member, and two members of one type
  keep two ids. At d3d91fc87 R14.2 checked inputs and reads alone, and TLC found its hole on net48: the next of kin
  rendered by the `ViewData` copy, then `Html.EditorFor(m => m.Guardian)`, with no input of the page after them,
  renders the guardian under `NextOfKin.Guardian`, so the page's read of `Guardian.Name` stops and the post binds
  nothing; R14.2 now refuses where the block ends (X14.16).
- The prefix design leaves a view rendered with a model and no prefix to MVC: inside another model's container its
  inputs post as MVC names them, and TLC reaches a kin's `Name` bound as the family's own `Name`
  (`Partials.violates.ReachOwnPlansValueLandsInAnotherMember.cfg`). A container's send that stops on an input of a plan
  that started with no prefix and another model closes that case (`Partials.holds.OwnPlanStops.cfg`) and leaves net10's
  partial rendered with a model inside a template, which inherits the template's prefix
  (`Partials.violates.ReachInheritedPrefixMisnames.cfg`). That stop is the owner's to take.

**Defects.**
- `X14.1` type: an input outside its plan's block does not compile: new:N14.2.
- `X14.2` type: a partial for a member takes the member as a typed lambda, so no prefix is written as text:
  new:N14.1.
- `X14.3` analyzer: an input bound to `m => m` is refused with "Input 'm => m' names no member of {model}: bind
  a member of the view's model, or render the member with Html.EditorFor from the view that holds it." At build by ALIS033, and where the view renders on code it does not follow (D18.6): new:N18.10.
- `X14.4` refusal: an input after the view's prefix moved is refused with "The field prefix of {view} changed
  from {old} to {new} after its plan started, so input '{name}' would post under a name its plan does not send
  or check."
- `X14.5` refusal: an id rendered twice by one plan is refused with "Input '{name}' renders element id '{id}' a
  second time in this plan: render the field once, or each copy for its own member with Html.PartialFor."
- `X14.6` analyzer (D14.5): a JSON request over a number-keyed dictionary entry is refused with "{member} sits under
  '{prefix}', a dictionary entry keyed by a number, which JSON reads as a list position: send this request
  with AsFormData()." At build by ALIS030, and where the view renders on code it does not follow (D18.6): new:N18.7.
- `X14.7` behavior: a slot's content that renders an input id the page holds runs none of its handlers, and the
  console names the id and both views.
- `X14.8` behavior: an `EditorFor` template's input of `NextOfKin.Name` shows the page's server message for
  `NextOfKin.Name` beside it, and the page's rule checks it.
- `X14.9` behavior (D13.2, D14.1, D14.2): one partial rendered by `PartialFor` for two members posts each card's own values.
- `X14.10` behavior: a list-item partial under `Members[1]` sends its inputs at position 1 in a JSON body.
- `X14.11` behavior: on net48 a partial rendered by `PartialFor` leaves the page's own inputs posting under their
  names; the `verify-net48` run proves it.
- `X14.12` behavior (D14.3): an input of `ChartEntry<FluidsReading>`'s `Initials` has the id
  `Alis_Reactive_Framework_App_Partials_ChartEntry_FluidsReading___Initials`, so a label written with that id fills it,
  and the endpoint binds what was typed; the redesign's
  `tests/Alis.Reactive.Framework.PlaywrightTests/Partials/WhenAMembersClassIsGeneric.cs` proves it at 65fdf118d.
- `X14.13` analyzer (D14.3, R14.1): an input of a member an interface declares is refused with "Input '{name}' binds a
  member that {interface} declares. An input's id is scoped by the class that declares its member, so the page's
  rules, sends and reads of it would not find it: type the view to the class, or a base class, that declares it." At
  build by ALIS033, and where the view renders on code it does not follow (D18.6): new:N18.10. The redesign's
  `tests/Alis.Reactive.Framework.PlaywrightTests/Partials/WhenAnInputsIdCannotFollowItsMember.cs` proves the message
  where the view renders at 65fdf118d.
- `X14.14` analyzer (D14.3, R14.1): an input of `m => m.Dose.Value` over a `decimal?` is refused with "Input
  'm => m.Dose.Value' binds Value, which Decimal? declares, not a member of {model}: bind 'm => m.Dose', which takes
  a Decimal?." At build by ALIS033, and where the view renders on code it does not follow
  (D18.6): new:N18.10.
- `X14.15` refusal (D14.3, R14.1, D18.2): an input of `m => m.Items.Count` is refused with "Input 'm => m.Items.Count' binds
  Count, which {List<Item>} declares, and the app's JSON options write {List<Item>} as {a list | one value}, so the
  input has no body path: bind a member of a class they write as an object."
- `X14.16` refusal (R14.2): a plan's block that ends with its view's prefix moved is refused with "The field prefix of
  {view} changed from {old} to {new} inside its plan, so what the view renders after the change posts under names its
  plan does not send or check: render a member's view with Html.PartialFor, which leaves the view's prefix as it is."

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
    The builder writes the helper's argument as `data.` and the path itself: the engine prefixes an argument only
    when it holds neither "data" nor "window" and reads any other as a global, so a member named like `windowSeat`
    fails the grid (`probe:ej2-grid-template-helper-prefix`).
  - It throws on a member past a missing holder (`probe:ej2-template-missing-holder`), so one row stops the grid's
    rendering. The builder writes a read inside the engine's condition on every holder of its path, since data reads
    as nothing when absent whatever the model declares (`probe:ej2-template-guarded-holder`,
    `probe:ej2-grid-template-guarded-holders`), so the row shows nothing there and the others render.
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
- `X15.1` behavior (D15.4): a grid row whose name holds `<img src=x onerror=alert(1)>` shows that text in its template
  cell, and no image element renders.
- `X15.2` behavior (D15.4): a row with no address shows an empty cell where the template reads the address's city, and
  the grid's other rows render.
- `X15.3` behavior: a template condition on the item's status shows the badge for an active row and the other
  branch for an inactive one.
- `X15.4` behavior: a template's event button raises its app event with the row's id, and the page's handler of
  that event runs.
- `X15.5` behavior (D15.4, M22.D3): with the app's JSON options naming members in Pascal case, a template reads `Name` from a
  row the server sent.

## Q16. Validation

**Decision.**
- D16.1 One fact. A rule is written once, on a `ReactiveValidator<T>` in FluentValidation's grammar. The server
  runs it, and C# derives the browser's rules from it where the view renders; nothing else writes a browser rule.
  Every mechanism below serves the cases it names and no other (the owner, 2026-11-06).
- D16.2 A check is one validator under one rule set, and that validator is the request's model's.
  - The request's model is the type its endpoint binds. A part of a model saved alone is its own model with its
    own validator (the owner, at 65fdf118d). `Validate<TValidator>` takes only `TValidator : ReactiveValidator<TRequest>`.
  - The plan that renders a request carries its validator's registration under its rule set, keyed by validator and
    rule set and by no element, and the check runs it over the body the request sends. A child validator's rules are
    in it, under the member that holds them (D16.3), so no other plan's registration joins the check, and composing
    plans needs no merge by plan id (D14.2). A check with no request runs the registration its container's plan
    carries.
  - The condensed default (D10.17): a request whose model has a registered validator is checked by it under the
    default rule set, whatever it sends, a container or none, with its failures placed by D16.13. Naming a rule set,
    or no check (`NoValidation()`), overrides it; the form of the sentence goes to gate 2
    (`Validation.tla:OnlyTheNamedValidator`).
- D16.3 A partial is attached or independent, and both nest (the owner, 2026-11-06).
  - Attached: a partial rendered for a member under MVC's prefix, by any of MVC's own doors (`EditorFor` by the
    member's type, by a template it names or the model's `[UIHint]` names, and for each item of a list; `<partial
    for>`; a controller that sets the prefix before its partial renders) or by `Html.PartialFor` (D14.4), has its
    own model and its own validator,
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
    metadata provider, and the binder's own errors stay. Gate 1 confirmed the mechanism (C31). On net48 the framework's
    bodies never meet MVC 5's implicit required: a value type that reads nothing is left out (D9.10).
  - The sandbox's endpoints call `Validate` and write `ErrorsByField` themselves, and read no binder refusal
    (`Alis.Reactive.SandboxApp/Areas/Sandbox/Controllers/Components/AppLevel/DrawerController.cs:48-51`).
- D16.5 The browser decides on the body its request sends, and its verdict equals the door's on the model the
  endpoint binds from that body.
  - The checked body is the endpoint's binding of the sent body. A sent member reads as the endpoint binds it:
    in form data an empty text binds null on both hosts (`probe:core-form-implicit-required`,
    `probe:mvc5-empty-text-null`). A member the body lacks reads its starting value (D16.6).
  - Decision C: a request is checked on what it sends and on the starting values of the rest. A request that sends
    no container is checked on its `Set` members and its `Input`s, and no container's inputs are read. This replaces
    65fdf118d, which read the validated form's inputs for such a request.
  - The check with no request, `Validate(container)` (N16.3), and live validation read their container's inputs, as
    the container sends them.
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
    starts at the validator's `T`, an item's at `RuleForEach`'s `TElement`. The plan carries each in the registration.
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
  - `IsInEnum`: membership in the enum's names and numbers, as `Enum.IsDefined` reads it. Over an enum the app names
    as flags (N16.4) it is FluentValidation's flags meaning, which FluentValidation keys on `FlagsAttribute`: a value
    is in the enum when it is the union of the enum's values whose bits it holds all, so zero is in it only when a
    value is zero (`probe:fv-flags-is-in-enum`). C# writes each value's number into the rule, and the browser tests
    the bits as D9.13's split does. ALIS038 admits the door only over an enum `[Flags]` marks, whose values are
    disjoint or one inside the other (N16.5), so the rule, net10's binding and D9.13's split agree on every value
    (`probe:core-flags-split-agrees`);
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
  each check of D16.10 and each condition of D16.8 (`Validation.tla:Agreement`).
- D16.13 A message's place is carried in the plan, never read from the page (B5).
  - C# writes each input's message element id beside the input's id, so the `_error` id convention leaves.
  - Every failure of a check, the browser's verdict and the server's 400 alike, shows in the request's summary
    place: the summary of the container it sends (D13.6), else the notice (D10.21), one notice of kind error whose
    text joins the messages by the separator the plan carries, a space, since each message is a sentence. A request
    whose summary place is the notice names the `notice` contract (D10.21).
  - A failure also shows at the message element of the framework input the request sent under its binding name
    (D10.18): an input inside its container, or its `Input`. A `Set` member, and a control of a native form's
    submission set that is no framework input, have no message element, so their failures show in the summary place
    only. An input that left the page before the answer shows nothing inline.
  - Both are MVC's own pattern: a validation summary that holds property errors, and a message for each field.
  - No DOM visibility is read, so a message inside a hidden panel is still in the summary. The form of the
    summary goes to gate 2.
  - `ShowValidationErrors(problem)` places a `ValidationProblem` from any source the same way, a response's body included
    (N10.7).
- D16.14 `FormField` is designed once (P5):
  - its path as Q1's steps, every item as `each` (D13.5);
  - its value, the one read (Q2) of the checked body;
  - its tests, each a condition (Q8), a check and a message.

  A registration carries the starts of D16.6 once. `$model.sentValueOf` and `$model.sends` leave, since every
  read is a `get` over one checked body (B3), and the form plugin's argument is a typed contract (D9.1).
- D16.15 Live validation is D12.7's, per container. A container's block carries its live check: its model's
  registered validator under the default rule set, the rule set the block names, or none (the form goes to gate 2).
  Typing clears an input's message, and blur and the input's commit check the input's member under the live check of
  each container that holds it, each over its own container's inputs; a message two of them give shows once.
- D16.16 A check that compares a member with a fixed value or another member (`Equal`, `NotEqual`, `GreaterThan`,
  `GreaterThanOrEqualTo`, `LessThan`, `LessThanOrEqualTo`, `InclusiveBetween`, `ExclusiveBetween`) compares a date or a
  time (`DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly`, `TimeSpan`, each nullable too) as .NET does, to the tick,
  through its key, as a condition does (D6.12, D8.10).
  - C# writes each operand as its type's key over the checked body's value, picked by the member's source as
    `spec/Core/conversions.md`'s Keys pick it, and a member the body lacks over its start (D16.6); a fixed value is
    written in the member's form and keyed as the member (D7.7).
  - A null member passes an ordering and a range, fails `Equal` and passes `NotEqual`, as FluentValidation's
    comparison checks read it (`probe:fv-date-comparisons`), where a condition holds no ordering with null (D8.9).
  - A text the member's type cannot hold fails first, with the binder's message (D16.7). A date or a time a converter
    writes is refused, the app's options' (X6.57) and the member's own (X6.43).
  - Both lines read a date in a check as a `Date`'s `getTime`, to the millisecond, and leave a `TimeOnly` and a
    `TimeSpan` ordering to the server alone; the keys give each a browser rule that agrees with the server's.

**Schema nodes.** `fn`, `get`, `as`, `op`, `if`, `record`, `list`, `lit`; the plugin data `form`, a registration with
its starts as a typed contract (Q19), and `model`'s containers; `$form.validate` and `$form.showErrors` as plugin
calls, each with the request's sent inputs and summary place. The grammar gains no node (G2).

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
- `probe:fv-flags-is-in-enum`
- `probe:core-flags-split-agrees`
- `probe:fv-date-comparisons`
- `Schema.tla:DatesComparedByKey`
- `Schema.tla:KeysOnlyCompared`
- `Ownership.tla:RulesLeaveWithTheirOwner`
- `Ownership.tla:FailureReachesSummary`
- `Ownership.tla:InlineOnlyAtTheSentInput`
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
- `N16.2` new: the form plugin's typed data, a registration keyed by validator and rule set and by no element, with
  the starts of D16.6, and the containers table of `model`'s data, each with its summary and live check.
- `N16.3` new: `pipeline.Validate(container)`, the check with no request: the container model's registered validator
  under the default rule set, over the container's inputs as it sends them; a failure shows as D16.13 places it and
  stops its pipeline. It carries `Validate<TValidator>(formId)` of both lines; the form of naming a rule set goes to
  gate 2.
- `N16.4` new: `Flags<TEnum>()`, `where TEnum : struct, Enum`, which names a flags enum once at startup, on both hosts.
  The framework knows an enum is flags by it alone, through the door's type argument (D18.1's generics), and reads no
  `FlagsAttribute` (D18.3). An enum it does not name is one value to the framework: `IsInEnum` reads it as
  `Enum.IsDefined` does on both sides, and a combination a source holds stops its request (D9.13). A literal is
  `Enum.ToString`'s text whether or not the door names its enum (D9.13). D9.13's wire form and D16.10's `IsInEnum`, on
  the server and in the browser, read the same declaration. Its form, and the startup call it is written in, go to gate 2.
- `N16.5` new: ALIS038, the flags analyzer, which reads `[Flags]` and an enum's constants at build. It refuses
  `Flags<TEnum>()` over an enum `[Flags]` does not mark, over one with a value outside 0 to 9,007,199,254,740,991
  (D9.13), over one with two values that share a bit with neither holding the other, and over a type parameter, which
  names no enum the build sees. The rule on shared bits is the one the split's agreement rests on: under it the split,
  FluentValidation's `IsInEnum` and net10's binding agree on every value, and it refuses some enums on which they agree
  too (`X = 1, Y = 3, Z = 6`, `probe:core-flags-split-agrees`). At the compilation's end, in a compilation that makes
  the startup call, it refuses an `IsInEnum`, and a request member a source fills, over an enum `[Flags]` marks that no
  `Flags<TEnum>()` names. A use in a compilation without the startup call (a validator in another assembly, a view MVC
  5 compiles on its first request) meets the meaning of an enum the app does not name (N16.4).

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
  `Alis.Reactive/Validation/ModelMember.cs:62`, `Alis.Reactive/Members/Expressions.cs:28`); the four are on D18.1's
  list since gate 1 (C13), since the server runs the tree the browser reads.
- A pattern with no JavaScript form stays on the server unannounced (`Alis.Reactive/Validation/Checks.cs:128-129`),
  and the rest pass to the browser with .NET's flags (`Alis.Reactive/Validation/Checks.cs:156-159`).
- The browser's Url pattern (`Alis.Reactive/PlanModel/Validation/BrowserRule.cs:152`) against the server's
  `Uri.TryCreate` (`Alis.Reactive/Validation/Checks.cs:206-207`).
- `IsInEnum` reads `FlagsAttribute` (`Alis.Reactive/Validation/Checks.cs:420`); over a flags enum it passes a value
  whose bits all lie in its values, zero among them where FluentValidation fails zero of an enum with no value for
  zero (`Alis.Reactive/Validation/Checks.cs:428`), and it leaves a flags enum to the server
  (`Alis.Reactive/Validation/Checks.cs:432`).
- G2: validation adds no grammar node; a check is `$form.validate` over plugin data, and the body it checks is a
  `fn` the plan writes (`Alis.Reactive/PlanModel/Validation/FormWriter.cs:19-30`).
- A check orders a date by a `Date`'s `getTime` (`Alis.Reactive/PlanModel/Validation/BrowserRule.cs:194-196`; main's
  `Alis.Reactive.Assets/runtime/shared/shape-convert.ts:167`), and orders only numbers, booleans and dates
  (`Alis.Reactive/PlanModel/Validation/BrowserRule.cs:179-183`,
  `Alis.Reactive/PlanModel/Validation/BrowserRules.cs:268`), so a `TimeOnly` and a `TimeSpan`, written as text
  (`Alis.Reactive/Members/ClrShape.cs:75-80`), have no browser rule.

**Defects.**
- `X16.1` type (D16.2): a request checked by a validator of another model does not compile: new:N16.2.
- `X16.2` behavior: on net10, a JSON save whose model starts its lead with a name, and whose lead type starts the
  name empty, sends only the lead's phone; the browser shows "Lead name is required." before sending, as the
  endpoint answers.
- `X16.3` behavior: a form-data save sends one member of a deputy that starts null, whose type starts its name with
  text; the browser lets the save through, and the endpoint saves it.
- `X16.4` behavior (D19.8): typing "12a" in a count bound to an int shows "The value '12a' is not valid for Count." and
  sends nothing.
- `X16.5` behavior (D16.2): with the Texas partial loaded the save is checked by the Texas rules; after the page loads
  the Massachusetts partial instead, the save is checked by the Massachusetts rules only, and the endpoint agrees
  on each.
- `X16.6` behavior: a request that sends only the status, set beside the intake container it does not send, is
  checked on the status and the starts, and the name typed in the container is not read.
- `X16.7` behavior: a failure whose message element sits in a hidden tab shows in the summary.
- `X16.8` behavior: on net10, a non-nullable text posted empty with no rule on it saves, and the door adds no
  "The Name field is required.".
- `X16.9` behavior: "http://a.com/b c" passes `Url()` in the browser and on the server, and "http://a.com:99999"
  fails both.
- `X16.10` behavior: a `Matches(@"^[0-9]+$")` on a single-line input refuses Arabic-Indic digits in the browser and on
  the server alike.
- `X16.11` analyzer: a pattern with `IgnoreCase` (D16.11): "The pattern '{pattern}' ignores case, which no browser flag
  folds as .NET does; spell both cases, as [Aa]spirin, or mark the rule .ServerOnly()." At build by ALIS006, and where the view renders on code it does not follow (D18.6): new:N18.11.
- `X16.12` analyzer: a pattern with a construct the engines read differently: "The pattern '{pattern}' uses
  {construct}, which .NET and the browser read differently; write {same} instead, or mark the rule .ServerOnly()." At build by ALIS006, and where the view renders on code it does not follow (D18.6): new:N18.11.
- `X16.13` analyzer: a condition with no browser form: "The condition '{condition}' has no browser form. Write it as
  tests of model members against fixed values or other members, or declare that the server alone decides it:
  WhenOnServer or UnlessOnServer." At build by ALIS006, and where the view renders on code it does not follow (D18.6): new:N18.11.
- `X16.14` analyzer: on net48, a start the binder cannot create: "{Type} has no public parameterless constructor, so
  the endpoint cannot bind it; give {Type} a public parameterless constructor." At build by ALIS006, and where the view renders on code it does not follow (D18.6): new:N18.11.
- `X16.15` behavior: a request that sends only set members, on a page with no form, shows "Ward is required." in
  the notice and sends nothing.
- `X16.16` behavior: an `Input` of the request's view outside every container, sent under `r => r.Ward`, shows the
  server's "Ward is required." at its own message element and in the notice.
- `X16.17` behavior (D19.7): the page and a slot's partial each render an input that binds `Name`; the slot's request sends
  its own container, and its 400 for `Name` shows at the slot's input and in the slot container's summary only.
- `X16.18` behavior: a native form's hand-written `<input name="Code">` fails on the server, and its message shows
  in the form's summary only.
- `X16.19` behavior: the slot that holds the sent input reloads while the request is in flight; the 400's message
  shows in the page container's summary and not at the reloaded input.
- `X16.20` behavior (D19.7): a live fieldset container inside a live form container: blurring the empty name both hold
  shows "Name is required." once.
- `X16.21` analyzer: `ShowValidationErrors` on a pipeline that is no request's route: "ShowValidationErrors shows a request's failures,
  and this pipeline is no request's route; call it inside OnError or OnSuccess." At build by ALIS031, and where the view renders on code it does not follow (D18.6): new:N18.8.
- `X16.22` behavior (M8.D1): a save that leaves out `Care`, an enum the app names as flags whose model starts it at
  `Mon | Tue`, passes `IsInEnum` in the browser, and the endpoint saves Mon, Tue; a save that leaves out `Rota`, an enum
  the app names as flags with no value for zero, which starts at 0, shows "'Rota' has a range of values which does not
  include '0'." in the browser and sends nothing, as FluentValidation's `IsInEnum` answers on the server
  (`probe:fv-flags-is-in-enum`).
- `X16.23` analyzer: `Flags<TEnum>()` over an enum `[Flags]` does not mark: "Flags<Shift>() names Shift as a flags
  enum, and Shift has no [Flags], so FluentValidation and System.Text.Json read it as one value; mark Shift [Flags], or
  take the call out." At build by ALIS038: new:N16.5.
- `X16.24` analyzer: `Flags<TEnum>()` over two values that share a bit with neither holding the other:
  "Flags<Overlap>() names Overlap, whose values X (5) and Y (6) share a bit and neither holds the other; the framework
  splits a combination only over values that are disjoint or one inside the other, where its split, FluentValidation's
  IsInEnum and net10's binding agree on every value; give each value bits no other value shares, or make one hold the
  other." At build by ALIS038: new:N16.5.
- `X16.25` analyzer: `Flags<TEnum>()` over a value outside 0 to 9,007,199,254,740,991: "Flags<Access>() names Access,
  whose value Archive is 9007199254740992, outside 0 to 9007199254740991, the range the browser's split reads, since
  JavaScript holds no larger integer exactly and a held number does not carry a negative value's bits; keep every value
  of a flags enum inside it, and a negative one as the union of the values it stands for." At build by ALIS038:
  new:N16.5.
- `X16.26` analyzer: in a compilation that makes the startup call, an `IsInEnum` over an enum `[Flags]` marks that no
  `Flags<TEnum>()` names: "IsInEnum reads CareDays, which [Flags] marks and the startup does not name, as one value,
  where FluentValidation reads it as flags; name it with Flags<CareDays>() at startup.", and a request member a source
  fills over one:
  "'r => r.Care' sends CareDays, which [Flags] marks and the startup does not name, so a combination its source holds
  stops the request; name it with Flags<CareDays>() at startup." At build by ALIS038: new:N16.5.
- `X16.27` behavior (D16.16): on net10, a shift whose `Starts`, a `TimeOnly` typed 06:30, fails
  `GreaterThanOrEqualTo(new TimeOnly(7, 0))`, and whose `Length`, a `TimeSpan` typed 1.02:00:00, fails
  `LessThanOrEqualTo(TimeSpan.FromHours(23))`, in the browser before sending, as the endpoint answers; with `Starts`
  left empty the ordering passes on both sides.

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
    reads no global by name. A lookup is `VendorLookup.ByPlugin(name)` in place of the object path, and its `fn` is a
    `call` of the plugin itself with the element (D17.14). Each plan that names the vendor also names the lookup's
    plugin object, since the vendors map is shared by name across plans and the plugin is found in the plan's own
    objects. An element whose lookup finds nothing stops the pipeline that reaches it, as one that holds no object at a
    path does (D4.1). The runtime holds no code for a lookup: the reference oxygen gauge, whose library keeps its
    gauges in a `WeakMap` of its own, needed none;
  - how a listener subscribes, one of three: a subscribe and unsubscribe method pair; a callback property, which
    holds one function of the framework's per object and property, its previous value put back at release; a
    subscribe method that returns the function that removes the listener (`const off = store.subscribe(listener)`),
    which release calls. The doors are `ComponentVendor.Define(name, objectPath, subscribe, unsubscribe)`,
    `ComponentVendor.DefineCallbackProperties(name, objectPath)` and `ComponentVendor.DefineReturnedUnsubscriber(name,
    objectPath, subscribe)` (N17.7). A callback property's listeners chain, gate 1's P4 (`spec/Vendors/CallbackProperty.tla`):
    - The first listener of an object's property, a handler's or a `listen` step's (D12.8), assigns the framework's
      function, which keeps the value it replaced; every later listener joins that function's list, and the property
      is not written again. The vendor's call runs the replaced value first, with the vendor's `this` and arguments,
      then each listener once, in the order it subscribed: a page's plans in the order the page holds their scripts,
      two handlers of one plan in the plan's order (D11.5), a slot's plans when its load starts them, a nested slot's
      where its script sits in that content, a `listen` step's when the step runs, a slot's plan on a component the
      page rendered included (D12.8), and a reloaded slot's after every listener still live. The call returns
      what the replaced value returned, and nothing when it held no function, since a listener answers nothing
      (D4.8); a throw from the replaced value reaches the vendor's call after every listener ran, as it reached it
      before any listened. A function whose declared return takes no `undefined` is a callback, and an event on it is
      refused at build (D17.5, X17.16), so no listener owes the vendor an answer.
    - A release removes its own listener from any place in the list. The last release puts the replaced value back,
      and only while the property still holds the framework's function, so a value a script assigned meanwhile stays.
    - A script that assigns the property replaces the framework's function and its listeners with it, as assignment
      does: they run nothing more, and their releases write nothing. The next listener assigns a new function over
      the script's value, which that function calls first, and it holds only the listeners that subscribe after it.
    - An object that lacks the property or refuses the assignment (a frozen object, a property that is not writable or
      has a getter and no setter, a setter that throws) meets the first listener as an object that lacks a listening
      member does (D12.8): a handler does not start and a `listen` step stops its pipeline, with one error naming the
      object and the property, and nothing is assigned. A put-back the object refuses is that listener's release error
      (D12.4, D12.5): the framework's function stays, calling the replaced value and no listener.
    - The function is its object's: after its host destroys or restarts the component, a release meets D12.5's check
      and writes nothing, to the old object or the new, and the first listener on the new object assigns that object
      its own function.
  - what a listener receives (D17.4);
  - where the object holds its element, a path from the object (`elementPath:` on each door, `element` for EJ2's
    components, empty for the element itself), which a listen on a component a payload hands over reads to tell when
    its host destroyed it (D12.8). A vendor that names none is a whole vendor: its components are reached by id and
    heard with `.Reactive` or a `listen`, and only a `listen` on one a payload or a returned object hands over is
    refused.

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
  - The check is an analyzer over a contract table (`poc/tools/vendor-contract.mjs`, POC 13a). The TypeScript compiler
    reads the app's TypeScript program (its own files, the packages it loads at their pinned versions, the libs its
    tsconfig names) and writes one line per type each module declares and per own public member: its kind, whether
    it returns a promise, whether it is optional, whether its read admits null or undefined, whether its write takes
    null, whether it is callable, and the type's bases by name. TypeScript's `private` and `protected`, `#private` and
    a JSDoc `@private` member are left out; a getter alone is read-only and a setter alone write-only.
  - A type's `[Symbol.iterator]` is a member the table writes with the type it yields, as it writes an array's items,
    so a declared iterator is checked as a declared list is (D5.13): on a type that has none, with items it does not
    yield, and `LiveObjects<T>` on a type that is no array. The POC's script writes no iterator
    (`poc/tools/vendor-contract.mjs:136-157`) and its `Element` declares no children, so X5.15 is the rewrite's
    contract unit, not the POC's.
  - An MSBuild target (`poc/VendorContract.targets`) writes the table into obj/ before CoreCompile, incremental on the
    TypeScript inputs, so no `dotnet build` reads a stale one, and it is not committed: lib.dom alone holds 60,695
    members, and the table is a function of inputs review already reads. A developer who brings a component imports
    the same target.
  - A component or plugin declaration names the type it mirrors with `[Mirrors(module, type)]` (its form is gate 2's);
    the module is a package's name, a lib's file name (`lib.dom`) or a file of the app's TypeScript project. The
    analyzer reports a component or plugin that names none (ALIS014), a type the table lacks (ALIS015), a member the
    type and its bases do not make public (ALIS016), `Method` against a promise and `MethodAsync` against none
    (ALIS017), a door of the wrong kind (ALIS018), and nullability by variance (ALIS019): what the member reads fits
    the C# type, so a read that admits null is `T?`, and what the C# type writes fits the member, so a `T?` write needs
    a member that takes null. A live object or a returned object the API can hold as null or undefined is checked at
    the object it holds otherwise; when it holds nothing, a read through it stops its pipeline (D5.5).
  - A member whose JSDoc `@default` text is `null` holds null until a value is set, and the vendor's documentation is
    part of its API (D5.3): the contract reads its type with null added, so its read admits null and its write, where
    it has one, takes null, and writes beside it the tag's file, relative to its package, and line, which ALIS019's
    message cites where the null is the documentation's. NumericTextBox's `value: number`
    (`@syncfusion/ej2-inputs/src/numerictextbox/numerictextbox.d.ts:67`, `probe:ej2-numerictextbox-value-default-null`)
    is `Property<decimal?>` with no suppression, and `Property<decimal>` of it is ALIS019, as on any member that reads
    null; DatePicker's `value: Date` (`@syncfusion/ej2-calendars/src/datepicker/datepicker.d.ts:83`) is
    `Property<DateTime?>`; a live object such a member holds is declared `nullable: true`. A member whose type and
    documentation both say non-null keeps ALIS019 for a `T?` write. The 15 Syncfusion packages a program of these two,
    DropDownList, MultiSelect and Grid loads hold 271 public writable class properties documented `@default null` and
    typed with no null (`probe:ej2-default-null-under-stated`), each read so with no suppression. The POC's script
    reads JSDoc only for `@private` (`poc/tools/vendor-contract.mjs:56`) and writes nullability from the type alone
    (`poc/tools/vendor-contract.mjs:245`, `poc/tools/vendor-contract.mjs:250`, `poc/tools/vendor-contract.mjs:265-266`),
    so its analyzer refuses a `decimal?` write of `value` (`poc/Alis.Reactive.Poc.Analyzers/MirrorsAnalyzer.cs:310-313`).
  - A drift a browser test arranges on purpose carries `SuppressMessage` whose justification names the test, and
    `WhenAContractDriftIsSuppressed` fails on a name no Playwright test declares.
  - A declared event is one the type declares (ALIS020, POC 13b), read from the type alone: a property of the event's
    name that holds a function (a callback property, Syncfusion's `EmitType<T>` members), or a literal a method takes as
    its first parameter when its second is a function, as a literal union (`on(event: "alarm" | "silenced", ..)`) or the
    keys of a map (`addEventListener<K extends keyof HTMLElementEventMap>`), the map's bases included. A `string` first
    parameter names no event, so a vendor that dispatches events its types do not name is typed first: an event map
    on an EventTarget, a literal union on its subscribe method, and `declare global { interface HTMLElementEventMap }`
    for an element's own event. The contract writes each method's events by reference (`keys:lib.dom:HTMLElementEventMap`)
    rather than expanded, which would add 33,725 lines.
  - A declaration that declares an event has its listening (D17.3) checked against its type and bases at build,
    since a handler subscribes only to a declared event:
    - The build reads a listening in three places, its names constants as ALIS034 reads a vendor's name: a
      component's `Vendor` body, which is `ComponentVendor.Native`, a `ComponentVendor.Define`,
      `DefineCallbackProperties` or `DefineReturnedUnsubscriber` call, or a read of a static property or field whose
      initializer in the same compilation is that call; a plugin's base constructor argument, an `EventListening`, or
      none, which is `EventTarget`; and an `EventedObject`'s `Listening` override. `ComponentVendor.Native` and
      `EventListening.EventTarget` are the method pair `addEventListener` and `removeEventListener`.
    - By kind: a method pair's subscribe and unsubscribe are each a member the type or its bases make public (ALIS016)
      that can be called (ALIS018), since the driver calls both with the event's name and the listener
      (`poc/runtime/src/objects.ts:195-199`). A returned unsubscriber's subscribe is the same, and what its signatures
      return holds a function and nothing else: a return that holds null or undefined is ALIS019, a function that
      returns a promise ALIS017, a gap to name as a `MethodAt` that returns one is, and any other type ALIS018, since
      the driver keeps that function and calls it at release (`poc/runtime/src/objects.ts:208-213`). A callback
      property's event is a property the type lets the driver write, the framework's function assigned and the
      previous value put back (`poc/runtime/src/objects.ts:201-206`), so a read-only one is ALIS018. Its function's
      declared return takes `undefined` (`void`, `undefined`, a union that holds either, or an untyped shape, which
      takes any type), since a listener answers nothing (D4.8): a return that takes none is an answer its vendor
      reads, a callback (D17.12), so an event on it is ALIS018 (X17.16). EJ2's `EmitType<T>` returns `void` and lib.dom's
      event handler properties return `any`, so both pass.
    - A reached `EventedObject` is checked at each type the reach binds it to, as its members are.
    - A vendor a referenced assembly defines is checked by the build that compiles it, on that build's
      declarations, since its initializer is metadata to an analyzer in any other. On a declaration in another
      build, and for names or a vendor computed at run time, the listening is met in the browser: a handler whose
      object lacks a listening member does not start, and a `listen` on one stops its pipeline (D12.8).
    - The contract already writes what the check reads: an unsubscribe is a member line, its events read as a
      subscribe's are (`poc/tools/vendor-contract.mjs:194-209`, `poc/tools/vendor-contract.mjs:255-259`), and a
      method's return is its `value` holds (`poc/tools/vendor-contract.mjs:258`), a function with no members written
      `function` (`poc/tools/vendor-contract.mjs:151-153`) and a return that admits null flagged `returnsnull`
      (`poc/tools/vendor-contract.mjs:50`); the POC app's contract lists `VitalsMonitor off method callable
      name:alarm,name:silenced` and `WardFeed subscribe value function`. The POC's analyzer reads no listening: no file
      of `poc/Alis.Reactive.Poc.Analyzers` names `Listening`, `ComponentVendor` or `Define`, so today each of these
      is met only in the browser.
  - Every `vendor-contract.tsv` the build passes is read, and a type two of them list is listed twice (ALIS015).
  - A declaration a component or plugin reaches is checked at the type its vendor's API gives what reaches it (POC
    13c, D5.3): `LiveObject` at the member's read type and `LiveObjects` at its element, `ReturnedObject` at the
    method's return and `ReturnedObjects` at its element, an event's payload at its listener's parameters (a callable
    property's function, the listener of each signature that takes the event's literal, or an event map's member), and
    a callback's arguments at its property's function's parameters. Null and undefined removed, a union is checked at
    each type it holds, a polymorphic `this` at the type it is read on and a type parameter at its constraint; each
    declaration at each type once, with ALIS016 to ALIS020 as on a root. A payload read by its members needs an object
    at the first parameter. `Argument(n)` and `MethodAt(n)` are checked against the parameter list, a rest parameter
    giving every later position: present (ALIS016), nullability by variance (ALIS019), a function for `MethodAt`
    (ALIS018), one that returns a promise a gap to name (ALIS017). A positional door on a declaration no listener or
    callback hands, a component's or one a member holds, is ALIS018, at the build rather than where the view renders.
    A primitive, `unknown`, `any`, a tuple or a function where a live object is declared, one object where the API
    holds a list and a list where it holds one object are ALIS021, the cure naming the vendor's TypeScript. A callback
    answer declared `T?` where the function's result takes no null is ALIS019.
  - A callable property whose declared type is a reference to a type alias with one type parameter that the alias's
    own type does not name, and whose function types its first parameter `any`, hands its listener at position 0 the
    type argument the reference passes, the payload the alias documents; every later position is the function's.
    Syncfusion types every event so: `EmitType<T> = AngularEventEmitter & ((arg?: any, ...rest: any[]) => void)`
    (`@syncfusion/ej2-base/src/base.d.ts:8`, `probe:ej2-emit-type-phantom`), and `change: EmitType<ChangeEventArgs>`
    hands `ChangeEventArgs`, with null beside it, since `arg?` is optional and an optional parameter holds null. The
    argument is read from the member's declared type node, as a base is read from its heritage clause's: TypeScript
    gives the member's type as `EmitType<T>`, the argument dropped (`probe:ej2-emit-type-resolved`). It is checked as
    any type the reach meets: a named type at its members, a union at each type it holds, a list as a list, and an
    untyped shape as it is, so a payload read by its members is ALIS016 at `Object`, lib.es5's interface (191 of
    Syncfusion's 895 public `EmitType` members), and ALIS021 at `any` (2), and a payload that reads no member passes,
    as the reach checks only a payload read by its members (`probe:ej2-emit-type-arguments`). Every other alias, and a
    function whose first parameter is typed, reads as its resolved type.
  - A declaration's type is a type its vendor's API holds, D5.3's value mark, checked by the type check (POC 16a,
    `poc/Alis.Reactive.Poc.Analyzers/HeldValues.cs`). POC 16a gave it ALIS025
    (`poc/Alis.Reactive.Poc.Analyzers/Rules.cs:20`), the id POC 13f gave the override analyzer of N18.5 first
    (`poc/Alis.Reactive.Poc.Analyzers/OverridesAnalyzer.cs:10`): the override analyzer keeps ALIS025, and the
    type check is ALIS037, an id no other rule holds. It checks by kind in a closed set: text
    (`string`, `char`, `Guid`, an enum whose type carries a string converter), a number (the numeric types, any other
    enum), a boolean, a date (`DateTime`,
    `DateTimeOffset`, which a read also takes from text, since data carries a date as text), a list of these, and an
    object whose members, each by its JSON name, the vendor's type declares or takes through a string index signature
    as the index's type; a member the vendor's type does not hold is refused with its path. The vendor's untyped
    shapes (`any`, `unknown`, `object`, `{}`, `Object`) take any type. The check reads the live form a value takes
    (D7.7, gate 1's P8): an enum against a literal union or an enum of the vendor's fits when every value it writes
    (its `[JsonStringEnumMemberName]`, its name or its number) is one the vendor takes, for a write, and every
    literal the vendor holds is one it writes, for a read (X17.46); a member or class whose own `[JsonNumberHandling]`
    writes a number as text, and a member's own `[JsonConverter]` or a converter on a type that is no enum, fit only
    the untyped shapes (X17.47); and a data member checked against a member the vendor's type declares by name
    carries `[JsonPropertyName]` with that name (X17.48), since the live form keeps the app's naming, which the build
    cannot see, and a declaration names its members by their JavaScript names. A member the vendor reaches through
    an index signature or an untyped shape keeps the app's naming, so an app's own rows keep their names. The POC
    takes an enum as text or a number (`poc/Alis.Reactive.Poc.Analyzers/HeldValues.cs:123`) and names a data member
    that has no `[JsonPropertyName]` by camel case (`poc/Alis.Reactive.Poc.Analyzers/HeldValues.cs:303`), so a
    Pascal-case app passes the build and writes "Name" where the vendor reads "name". Refused, each a gap to name: a value that holds
    a promise, a function, nothing, more than one kind, or a type no C# kind mirrors. A read fits every type the API
    holds, and a write fits one of them. The POC checks a property's value both ways and the result of a method or a
    callable property, awaited or not (`poc/Alis.Reactive.Poc.Analyzers/MirrorsAnalyzer.cs:103-111`). The rewrite's
    analyzer unit checks every other value a call passes or a callback answers against the same contract:
    - Each argument of a call by D5.9's six method kinds, `CallItself` and `MethodAt`, as a write: item n of the list
      at parameter n of the function the call reaches (a method's, the declared type's own for `CallItself`, the
      function a listener or callback hands at its position for `MethodAt`), a rest parameter taking every later
      item. A list agrees when one of the function's signatures takes it whole, as TypeScript picks an overload, so
      the contract writes each signature's parameters apart. A parameter typed by the function's own type parameter
      is checked at its constraint, at any depth, and one with no constraint takes any type. An item is typed by its
      door: `Arg<T>` and `Fixed<T>` at `T`; `Pass<TDeclared, TSource>` at `TDeclared`, the type the declaration
      states, since `TSource` is the view's and derives from it; the abort signal (D17.11) at lib.dom's
      `AbortSignal`; an `Answer` as a callback's answer. Its type fits by the type check; a `T?` item needs a
      parameter that takes null or undefined (ALIS019); an item past the parameters is ALIS016; a parameter
      TypeScript marks neither optional (`?` or a default value) nor rest, given no item, is ALIS022; an `Answer`
      fits only a function the parameter takes, and no other item fits a function (ALIS018). The POC reads no
      argument list (`poc/Alis.Reactive.Poc.Analyzers/Use.cs:43-52`).
    - A callback's answer, of a `Callback<TArgs, TResult>` property and of an `Answer` an argument passes: `TResult`
      fits the function's return by the type check as a write, its nullability by ALIS019, and `TArgs` is reached at
      the function's parameters as the bullet above reaches a callback's arguments, so an `Answer` a method takes
      reaches its arguments through that method's parameter. A date the answer holds, against a `Date` the function
      returns, is refused, a gap to name: an answer reaches its vendor as the plan writes it, a date as text (POC
      12c), where D3.13 gives a property's set and a method's argument their `Date`. The POC checks an answer's
      nullability alone (`MirrorsAnalyzer.cs:93-97`, `MirrorsAnalyzer.cs:280-286`).
    - `Argument<T>(n)`'s `T` fits the parameter it reads by the type check as a read, as a payload's member does; the
      POC checks its presence and nullability alone (`MirrorsAnalyzer.cs:122-135`).
    - `CallItself<T>`'s result fits the return of the declared type's own call by the type check and ALIS019; the
      POC checks only that the type is callable and returns no promise (`MirrorsAnalyzer.cs:266-278`).
  - The contract writes what each member holds as `holds` lines: a read's type, a method's return (a promise's
    value), a callable's parameters and a listener's parameters for each event it takes, each part a named type, a
    list's element, the object itself, a function, null, or another type by its text. For the arguments and answers
    above it also writes a method's own parameters and the parameters and return of a type's own call, each
    parameter with TypeScript's optional mark and its rest mark, and the parameters and return of a function any
    parameter holds. It writes a callable property's position 0 from its alias's type argument by the bullet above.
    The POC's script writes no method's parameters (`poc/tools/vendor-contract.mjs:255-260`), a
    type's own call as flags alone (`poc/tools/vendor-contract.mjs:306-311`), and a function a parameter holds as
    `function` alone (`poc/tools/vendor-contract.mjs:151-153`). It reads an alias's type arguments only to name a type
    (`poc/tools/vendor-contract.mjs:93`) and to find a free type parameter (`poc/tools/vendor-contract.mjs:120`), and
    writes a callable property's parameters from its resolved function (`poc/tools/vendor-contract.mjs:167-180`,
    `poc/tools/vendor-contract.mjs:268`), so DropDownList's `change` holds `parameter:0 other any`, every Syncfusion
    payload read by its members reaches `any` at its first parameter
    (`poc/Alis.Reactive.Poc.Analyzers/VendorContract.cs:89-93`, `poc/Alis.Reactive.Poc.Analyzers/ReachWalk.cs:118`),
    and the reach reports ALIS021 (`poc/Alis.Reactive.Poc.Analyzers/ReachWalk.cs:151`). A type the vendor does not name
    (an instantiation such as `CustomEvent<Noted>`, a base such as `InfusionPump<PumpChannel>`, an object literal type)
    is listed as its own type, members resolved and flattened, named `CustomEvent#210` with its text shown in messages.
    A type that holds a free type parameter (a method's own generic) is written by its text only: C# never supplies a
    vendor method's type arguments, and listing each instantiation never ends. A name two files of a package declare
    is listed once for each declaration under its own name, so it can be reached, and mirroring it by name stays
    ALIS015. A base is read from its heritage clause's type node, since the type at the clause carries the derived
    class's `this`.
  - A root's checks run as the code is typed. A reached declaration is checked at the end of the compilation, since
    what reaches it can be anywhere in it, so the IDE shows those diagnostics on build (accepted at gate 1, C32). A declaration a
    referenced assembly holds is checked by the build that compiles it.
- D17.6 Every vendor package is pinned to one exact version, its sub-packages included, and its npm and NuGet
  versions move together (B16). The contract table of D17.5 is written by every build whose TypeScript inputs
  changed, so a vendor's drift fails the build at the upgrade.
- D17.7 A vendor's name is checked by its characters, a letter and then letters, digits, `_` or `-`, with no regular
  expression (D16.10).
- D17.8 The conformance library's six declarations (`spec/Reference/native.json`) and FusionMultiSelect
  (`spec/Reference/syncfusion.json`) prove the vendor contract: on paper, every cell of the matrix against the
  declaration that runs it (`check:matrix`), and in the browser, each cell by its own test on the bench and each of
  MultiSelect's own cells by its slice's test (the owner, 2026-11-10).
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
- D17.11 A declared method receives an abort signal only where its declaration names an argument of that kind,
  so no vendor's method receives an argument it does not declare. The plan writes it as the expression `signal`,
  which reads the signal of the block it runs in (D11.12, Q19).
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
  writer in the live form (D7.7), `"null"` when a holder on the way holds nothing; `field.Json(options)` writes
  the helper's starting options the same way, so a date, an enum and a number reach the component's code in the
  forms the plan's own values take. After a failed post the value is the model's, as the action leaves it, on both
  hosts: net10 keeps no typed form of an attempted value (its converter is internal, and a value provider's result
  converts nothing), and MVC 5 converts one only by a `System.Type` (`ValueProviderResult.ConvertTo(Type)`), a type
  read the owner's list lacks. A typed component shows what the server decided; a text box keeps MVC's attempted text. A
  component that posts through a hidden input writes a number in the view's culture, the decimal separator the
  helper gives it and no grouping, since the server binds the body under the culture the view rendered under (D3.11).
- D17.14 A plugin is a live object the page's own code provides by name, with no element (D5.1).
  - Its declaration is a `Plugin`, named once with a vendor's characters (D17.7), so a dot is refused and the `alis.`
    names stay the framework's. The name is checked where the plan takes the plugin, outside the declaration's
    constructor.
  - A pipeline reaches it with `p.Plugin<TPlugin>()`, and a plan handles its events with
    `plan.Plugin<TPlugin>().Reactive(..)`, in every block a component's member is used in: a handler, a condition's
    term and its branches, and a callback's answer.
  - Its events listen by a vendor's listening kinds (D17.3): `EventTarget`, `addEventListener` and
    `removeEventListener`, unless the declaration names another with `EventListening`. The plan's locator carries the
    plugin's name and its listening, and its object id is `$plugin:` and its name, so no element id meets it. It
    carries the declaration's listening for a plugin that declares no event too, a core plugin as `alis.model` among
    them (gate 1's P14, kept): no handler reads it there, since a handler names an event the declaration declares, and
    the build checks a listening it reads only on a declaration that declares an event (X17.45). A `none` there would
    name a kind twice (S3) and make the runtime narrow a shape the plan controls, for no behavior.
  - A declaration calls the object itself with `CallItself`, a `call` with no path, applied with `this` undefined:
    the doors of a plugin that is a function (main's `Command()` and `Function<T>()`, the redesign's `name: ""`).
  - The page provides a plugin by pushing `{ name, instance }` onto `window.__alisPlugins`. Boot reads the queue before
    `alis:mount`, and from then on the queue's push registers at once, so a plugin a script provides after the page
    started is found by the next step that reaches it. The redesign drops such a push with no word: it deletes the
    queue at boot (`Alis.Reactive.Assets/runtime/root.ts:44-45`).
  - A plugin's handlers subscribe when their plan starts, as a component's do, so the events of a plugin provided
    after the page started reach no handler, and each of its handlers is reported at start as one that did not start;
    a step reaches its members once it is provided.
  - A step reaching a name no script provides stops its pipeline, naming the plugin. A name two scripts provide holds
    no plugin, and each step that reaches it stops from then on, so a plugin provided again after steps used it
    stops those steps from then on. The instance is any value, read as a required read reads any present value
    (D4.1). The redesign stops the whole page for a name provided twice or a value that is no object
    (`Alis.Reactive.Assets/runtime/objects/plugins.ts:23-36`); here a plugin's fault stops only the pipelines that
    reach it, as an element's does (D4.1).
- D17.15 A row action serves any grid-like list, never one vendor's (the owner, 2026-11-08): it is declared once,
  outside its rows, and each row renders it with its own values.
  - The core's door is `plan.Rows<TRow>(rowsId)`, an instance member of the plan, since a call that names only its own
    type argument binds no extension (N10.7). A render helper built on it declares each action once, in declaration
    order, as a block (D11.10) whose payload is `RowActivated<TRow>`: `Row`, the row it was rendered with, read as data
    (D4.2), and `Element`, the control the user activated, a component (D11.4). Every module works in that block.
  - `Render(row, text)` writes the control with the id `{rowsId}-{position}-{n}`, n counting that action's renders
    from 0, and records one entry: the id and the values of the row's members the action's block reads, written by
    the plan's literal writer in the live form (D7.7), since the helper's plugin receives them and hands each back as
    its payload's `Row`. The entries are one literal at the plan's end (D14.7), so
    the plan grows by one entry per control and holds no member the block does not read. The handler is written once
    per action, where both lines write a whole plan per link.
  - The action's handler starts on load (D11.6), calls its helper's plugin with the entries, and listens to the one
    object it returns (D12.8): one listen per action, released with its plan's owner, so rows in a slot leave with
    it. Its policy is the handler's across its rows (D11.7): `overlap` by default, `replace` so the row clicked last
    wins, `skip` so one run holds every row.
  - Native's helpers (D17.10): `NativeActionButton(action)` renders `<button type="button">` for any block;
    `NativeActionLink(action)` renders `<a href>` for a block whose one request at its own level is a GET. Its href is
    that GET's URL, filled in C# from the row's and the view's values by the encoding the request sends (D9.4,
    D9.23), so a new tab, a middle click or a copied link opens what the click loads. An unmodified primary click
    raises the action and prevents the navigation; a modified or middle click stays the browser's. What another
    script did with the click is not read (`defaultPrevented`), since the runtime infers no intent from the page.
  - Native's browser code, the plugin `native-row-actions`, finds each control by `getElementById` and raises
    `activated` with its entry's row and its element. The core reads no data attribute and walks no DOM.
  - Compiler rules: a row of another type does not compile (`RowOfAnotherType` in `spec/api/refused.sh`), nor a render
    outside the plan's block (N14.2); ALIS008 holds one request per block at its own level; ALIS035 refuses a link
    whose block's one request is no GET, whose route or query value is neither a C# value nor its row's member, or
    that sends a container, an input or a header, since a navigation carries none of them (N18.13). Refused where the
    view renders, by what only the render sees: an id two controls render (D14.6) and D9.4's checks on a URL the view
    computes.
  - A component grid whose vendor raises a command declares the event's payload with its row as `ReadOnly<TRow>`
    (D17.4) and handles it with `Reactive`, every module in the block; `Rows` serves markup the view writes.
  - A gap, named: a response loaded into the activated row (`Into` the row's own element). A slot is keyed by text C#
    writes (D12) and an entry's element is the row's control, not its row; the use case is a row re-rendered from the
    response's partial. Until it has a door, a view reloads the list with a chained GET `Into` its container.

**Schema nodes.** The plan's `vendors` map, one `Vendor` per name, holding its `locate` and its listening kind;
`Handler`, which names its event and carries its payload's path, since the vendors map is shared by name across
plans (Q19).

**Checker.**
- `Ownership.tla:NoUnsubscribeOnDestroyed`
- `Ownership.tla:ListenersDoNotAccumulate`
- `Run.tla:WritesAfterTheTurnAreRefused`
- `CallbackProperty.tla:EveryHearerHearsOnceInOrder`
- `CallbackProperty.tla:AThrowSkipsNoHandler`
- `CallbackProperty.tla:ALiveHandlerKeepsTheFrameworksFunction`
- `CallbackProperty.tla:TheLastReleasePutsBackItsValue`
- `CallbackProperty.tla:ARefusedAssignmentStartsNoHandler`
- `CallbackProperty.tla:AReplacedObjectIsNotWritten`
- `probe:ej2-methods-returning-promise`
- `probe:ej2-destroy-removes-instance`
- `probe:ej2-max-public-method-arity`
- `probe:ej2-emit-type-phantom`
- `probe:ej2-emit-type-resolved`
- `probe:ej2-emit-type-arguments`
- `probe:ej2-numerictextbox-value-default-null`
- `probe:ej2-default-null-under-stated`

**Doors.**
- kept: `ComponentVendor.Define`, `ComponentVendor.DefineCallbackProperties`, `ComponentVendor.Native`,
  `ComponentVendor.Name`, and the declaration vocabulary of `JsObject` (`Method`, `MethodAsync`, properties,
  events); `VendorEventPayload` leaves for N17.1's path (D17.4).
- `N17.1` new: the payload's place in its listener's arguments, declared with the event (D17.4); its form goes to
  gate 2.
- `N17.6` new: a vendor's lookup, a plugin called with the element, beside the path from the element (D17.3):
  `VendorLookup.ByPlugin(name)` and an overload of `ComponentVendor.Define`, `DefineCallbackProperties` and
  `DefineReturnedUnsubscriber` that takes it; their forms go to gate 2.
- `N17.7` new: the listening kind whose subscribe method returns the function that removes the listener (D17.3).
- `N17.8` new: `Argument<T>(n)`, and a payload method at a position (D17.4).
- `N17.9` new: `Callback<TArgs, TResult>` and its `Set` from an expression, and `Answer`, an answer passed as a
  method's argument (D17.12).
- `N17.2` new: the vendor contract table and the analyzer that checks declarations against it (D17.5), built in POC 13a
  as ALIS014 to ALIS019 with `[Mirrors(module, type)]`.
- `N17.3` new: a condition over an awaited `MethodAsync<bool>` source, replacing `ConditionStart<TModel>.Confirm` and
  `ConditionExtensions.Confirm`; its form goes to gate 2.
- `N17.4` new: `NativeConfirm` and `NativeToast` in Alis.Reactive.Native, each a declaration and a render helper.
- `N17.5` new: the declaration's argument kind that carries its block's abort signal (D17.11).
- `N17.10` new: an input field's `ValueJson` and `Json(options)`, the starting value and options a render helper
  writes (D17.13); their names go to gate 2.
- kept: `Plugin`, `Plugin.Name` and `Plugin(string name)` (both lines).
- `N17.12` new: `plan.Rows<TRow>(rowsId)` and `Rows<TModel, TRow>` in the core; `RowActivated<TRow>`, `RowAction<TModel,
  TRow>.Render(row, text)`, `NativeActionButton(action)` and `NativeActionLink(action)` in Alis.Reactive.Native, with
  the plugin `native-row-actions` (D17.15). They replace main's `NativeActionLink` with its serializer and id generator,
  and the redesign's `NativeActionLink`, `NativeActionLinkRequest` and `NativeActionLinkConfirm`; their forms go to gate 2.
- `N17.11` new: `Plugin(string name, EventListening events)` and `EventListening` (`EventTarget`, `CallbackProperties`,
  `Methods`, `ReturnedUnsubscriber`; `PluginEvents` until POC 14d gave it a second holder, `EventedObject`), `p.Plugin<TPlugin>()`, `plan.Plugin<TPlugin>().Reactive(..)` and `CallItself`
  (D17.14); their forms go to gate 2. They replace the redesign's `ReactivePlan.RegisterPlugin<TPlugin>()`, so a plugin
  registered twice on one plan is no longer representable, and carry main's string doors by the declaration
  vocabulary: `p.Plugin(name)` and `p.Plugin(name, member)` by a declaration's members, `Plugin.Command()` and
  `Plugin.Function<T>()` by `CallItself`, `Plugin.Property<T>(member)` by `Property<T>`, and the `Arg` overloads from a
  payload path or a response path by `Arg<T>` from any source. `GatherBuilder.Plugin` is the request's, which has no
  POC (the gate 1 report).

**Evidence.**
- A vendor's object is found by a path from the element alone (`Alis.Reactive/Vendors/ComponentVendor.cs:78-93`,
  `Alis.Reactive/Vendors/ComponentVendor.cs:109-122`), so a library that keeps its objects off the element cannot be
  declared today: N17.6.
- G5: the driver switches only on the listening kind (`Alis.Reactive.Assets/runtime/objects/vendor-driver.ts:28-33`),
  and a vendor is the data `ComponentVendor` carries (`Alis.Reactive/Vendors/ComponentVendor.cs:38-52`).
- B12: what a listener receives is read by `instanceof` (`Alis.Reactive.Assets/runtime/objects/vendor-driver.ts:89-92`).
- The redesign chains a callback property's listeners: each subscribe wraps the value the property holds and calls it
  first, and a release marks its wrapper and, only while the property holds that wrapper, puts back the first live value
  under it (`Alis.Reactive.Assets/runtime/objects/vendor-driver.ts:55-75`). Its tests pin the replaced value called
  first, two handlers in plan order, either owner released first and an absent property as a boundary error
  (`Alis.Reactive.Assets/runtime/__tests__/vendor-driver.test.ts:45`,
  `Alis.Reactive.Assets/runtime/__tests__/vendor-driver.test.ts:61`,
  `Alis.Reactive.Assets/runtime/__tests__/vendor-driver.test.ts:92`,
  `Alis.Reactive.Assets/runtime/__tests__/vendor-driver.test.ts:112`), and in the browser a slot's handler after the
  page's, a slot's release that leaves the page's, and a release from the middle
  (`tests/Alis.Reactive.Framework.PlaywrightTests/Vendors/WhenListenersShareACallbackProperty.cs:11`,
  `tests/Alis.Reactive.Framework.PlaywrightTests/Vendors/WhenListenersShareACallbackProperty.cs:24`,
  `tests/Alis.Reactive.Framework.PlaywrightTests/Vendors/WhenListenersShareACallbackProperty.cs:43`). Each subscribe
  writes the property (`Alis.Reactive.Assets/runtime/objects/vendor-driver.ts:64`), a write that throws in a module's
  strict code on a frozen object or a read-only property (`Alis.Reactive.Assets/runtime/grammar/paths.ts:42`), so a
  later listener meets a refusal its first did not; and a throw from the replaced value leaves through every wrapper,
  so no listener runs. P4 keeps the chain and its order, with one function per object and property that writes at its
  first listener alone and runs every listener past that throw.
- B15: FusionGrid declares its three exports with `Method()` (`Alis.Reactive.Fusion/Components/FusionGrid/FusionGrid.cs:207-213`),
  while EJ2's grid declaration returns a `Promise` from `excelExport` at line 3849 of its `grid.d.ts`, and 120 EJ2
  public methods declare one (`probe:ej2-methods-returning-promise`).
- B16: only the meta-package is exact (`Alis.Reactive.SandboxApp/package.json:25`); its sub-packages follow tilde
  ranges (`package-lock.json:1717-1726`), so `ej2-base` resolves to 33.1.45 under 33.1.47.
- A regular expression checks a vendor's name (`Alis.Reactive/Vendors/ComponentVendor.cs:33-34`).
- Both lines write a whole plan for each action link: main serializes the plan into each anchor's payload
  (`Alis.Reactive.Native/Components/NativeActionLink/NativeActionLinkSerializer.cs:45`), and the redesign renders a plan per
  link (`Alis.Reactive.Native/Components/NativeActionLink/NativeActionLink.cs:108`). Main refuses validation, Parallel,
  chained requests, path values and IncludeAll in a link
  (`Alis.Reactive.Native/Components/NativeActionLink/NativeActionLinkSerializer.cs:32`,
  `Alis.Reactive.Native/Components/NativeActionLink/NativeActionLinkSerializer.cs:82`,
  `Alis.Reactive.Native/Components/NativeActionLink/NativeActionLinkSerializer.cs:94`,
  `Alis.Reactive.Native/Components/NativeActionLink/NativeActionLinkSerializer.cs:115`,
  `Alis.Reactive.Native/Components/NativeActionLink/NativeActionLinkSerializer.cs:120`), finds the link by a document
  listener and `closest()` (`Alis.Reactive.Assets/runtime/components/native/native-action-link.ts:23`,
  `Alis.Reactive.Assets/runtime/components/native/native-action-link.ts:37`) and counts ids on the HTTP request
  (`Alis.Reactive.Native/Components/NativeActionLink/NativeActionLinkIdGenerator.cs:20-31`). The redesign repeats the
  pipeline's verbs on the link (`Alis.Reactive.Native/Components/NativeActionLink/NativeActionLinkRequest.cs:29-38`) and
  reads `DefaultPrevented` in its guard (`Alis.Reactive.Native/Components/NativeActionLink/NativeActionLink.cs:113-121`).
  Verdicts (D17.15): the plan per link is refused for its size; validation, Parallel, `Chained`, path values and
  containers are lifted, since an action is a block, a link's containers staying ALIS035's; the document listener,
  `closest()` and `DefaultPrevented` leave; the id counter on the HTTP request is refused by D14.1; the link's own
  verbs and `Component` are refused as parallel names; its confirm is NativeConfirm's `Ask` in a `When` (D17.10);
  `LinkRequestCount` gives way to ALIS008 at build; the click guard moves into Native's plugin.

- The core writes a call to a plugin only Fusion provides: `Alis.Reactive/PlanModel/Conditions/Condition.cs:504`,
  `Alis.Reactive/PlanModel/Writing/Scope.cs:209`, `Alis.Reactive.Assets/components/fusion/confirm.ts:94`.
- Today's confirm queues its asks (`Alis.Reactive.Assets/components/fusion/confirm.ts:88`), keeps Escape inside
  (`Alis.Reactive.Assets/components/fusion/confirm.ts:67`) and writes fixed English
  (`Alis.Reactive.Assets/components/fusion/confirm.ts:50`, `Alis.Reactive.Assets/components/fusion/confirm.ts:55`).

**Defects.**
- `X17.1` analyzer: ALIS017 reports a method declared with `Method` whose vendor contract returns a promise: new:N17.2.
- `X17.2` analyzer: ALIS016 reports a declared member the vendor contract does not list as public: new:N17.2.
- `X17.28` analyzer: ALIS020 reports a declared event its vendor's typed API does not declare: new:N17.2.
- `X17.29` analyzer: a declaration a component or plugin reaches is checked at the type the vendor's API gives what
  reaches it, and ALIS021 reports a reach that type gives no object, no list or a list: new:N17.2.
- `X17.30` analyzer: `Argument(n)` and `MethodAt(n)` are checked against the parameters a listener or callback is
  handed, and ALIS018 reports one on a declaration handed none: new:N17.2.
- `X17.39` analyzer (D5.3, D17.5): ALIS037, the type check, reports a type its vendor's API does not hold, on a property's value
  read or written and on the result of a method or a callable property, awaited or not, and on the data past them
  member by member: new:N17.2.
- `X17.40` analyzer (D5.9, D17.5): an argument that disagrees with the parameter of the function its call reaches, by
  `Method`, `MethodAsync`, `ReturnedObject`, `ReturnedObjects`, `CallItself` or `MethodAt`, where no signature takes
  the whole list: its type (the type check), a `T?` where the parameter takes no null (ALIS019), an argument past the
  parameters (ALIS016), a required parameter given none (ALIS022), and an `Answer` where the parameter takes no
  function, or another argument where it takes only a function (ALIS018): new:N17.2.
- `X17.41` analyzer (D17.5, D17.12): a callback's answer, of a `Callback` property or an `Answer` a method takes,
  whose type the function's return does not take (the type check), whose arguments' declaration the function's
  parameters do not hand (ALIS016 to ALIS022, as a reach), or which holds a date where the function returns a `Date`
  (the type check): new:N17.2.
- `X17.42` analyzer (D17.4, D17.5): `Argument<T>(n)` whose `T` is no type the parameter at `n` holds, and
  `CallItself<T>` whose `T` is no type the declared type's own call returns (the type check): new:N17.2.
- `X17.43` analyzer (D17.4, D17.5): a payload a Syncfusion event hands, `change: EmitType<ChangeEventArgs>`, is
  checked at `ChangeEventArgs`, the type argument its alias does not name: a member that type does not declare is
  ALIS016, a nullability that disagrees ALIS019, a type it does not hold ALIS037, and a payload read by its members
  where the argument is `any` ALIS021; the contract writes it in the rewrite's analyzer unit, before Fusion's
  onboarding declares a payload: new:N17.2.
- `X17.44` analyzer (D5.3, D17.5): a `Property<decimal?>` declaration of NumericTextBox's `value: number`, documented
  `@default null`, builds with no suppression, the contract citing
  `@syncfusion/ej2-inputs/src/numerictextbox/numerictextbox.d.ts:67`; `Property<decimal>` of it is ALIS019, its message
  citing that line; a `T?` write to a member whose type and documentation both say non-null stays ALIS019: new:N17.2.
- `X17.45` analyzer (D17.3, D17.5): on a declaration that declares an event, a listening the build reads whose
  subscribe or unsubscribe its type and bases do not make public (ALIS016) or that cannot be called (ALIS018); a
  returned unsubscriber's subscribe whose return holds null or undefined (ALIS019), a function that returns a promise
  (ALIS017) or anything but a function (ALIS018); a callback property's event its type makes read-only (ALIS018):
  new:N17.2.
- `X17.46` analyzer (D7.7, D17.5): ALIS037 on an enum against the vendor's literal union or enum: "'{member}' takes
  {literals}, and '{Enum}' is written {as its number | by its names}: {give '{Enum}' [JsonConverter(typeof(JsonStringEnumConverter<{Enum}>))] | remove its converter | name '{Enum}.{Value}' [JsonStringEnumMemberName(\"{literal}\")] | declare a value that writes '{literal}'}": new:N17.2.
- `X17.47` analyzer (D7.7, D17.5): ALIS037 on a member written by its own JSON converter or number handling against a
  vendor member that declares a kind: "'{path}' is written by its own JSON {converter | number handling}, and
  '{vendor}' holds {kind}: a live member holds JavaScript's value; remove it from '{path}'": new:N17.2.
- `X17.48` analyzer (D7.7, D17.5): ALIS037 on a data member checked against a member the vendor's type declares by
  name, with no `[JsonPropertyName]`: "'{path}' is named by the app's JSON naming, which the build cannot see, and
  '{vendor}' names it '{name}': give it [JsonPropertyName(\"{name}\")]": new:N17.2.
- `X17.3` behavior: a CustomEvent with no detail reaches its handler as the declaration says, the same on every
  dispatch.
- `X17.4` behavior (D15.1, D17.1, D17.2): a component an app brings, declared and rendered outside the framework's packages, runs every
  step kind (a read, a write, a call, an event) with no change to the framework.
- `X17.5` analyzer (D17.7): a vendor name outside its characters: "Vendor name '{name}' must start with a letter and contain
  only ASCII letters, digits, underscore, or hyphen." At build by ALIS034, and where the view renders on code it does not follow (D18.6): new:N18.12.
- `X17.6` behavior (D15.1, D17.1): a page that loads no Fusion asset asks NativeConfirm in a `When`; the branch runs on OK and
  not on Cancel.
- `X17.7` behavior: Escape in a confirm opened over a drawer answers false, and the drawer stays open.
- `X17.8` behavior: a save replaced while its confirm is open closes that dialog, and only the second save's
  confirm shows.
- `X17.9` behavior: an error toast stays until it is dismissed, and an information toast leaves after its time
  unless it is hovered.
- `X17.10` behavior: a component found by a registered lookup, as `Chart.getChart(canvas)` finds a chart, is read,
  written, called and listened to as one found by a path: an oxygen gauge its library keeps off the element shows its
  reading, takes the threshold a step writes, lowers and raises its alarm; a step reaching a gauge its library never
  made stops only its own pipeline.
- `X17.11` behavior: a listener added by a subscribe method that returns its unsubscriber is removed at release by
  that function, and the event runs nothing after.
- `X17.12` behavior: a listener of two arguments reads the first and calls the second, the vendor's continuation,
  with the page's items, and the component shows them.
- `X17.13` analyzer: an awaited read inside a callback's answer is refused where the view renders with "A callback
  answers its vendor at once, so its answer cannot wait for an async method or a request." At build by ALIS032, and where the view renders on code it does not follow (D18.6): new:N18.9.
- `X17.16` analyzer (D17.3, D17.5, D17.12): an event declared on a callback property whose function's declared return
  takes no `undefined`, an answer its vendor reads, is ALIS018 with "'{event}' returns {type}, an answer its vendor
  reads, and a handler answers nothing. Declare it as Callback<TArgs, TResult> and write the answer as an expression.":
  new:N17.2.
- `X17.17` behavior (D17.3): a call bell whose event is a callback property runs its handler with the ring's room,
  and a ward feed whose subscribe method returns its unsubscriber runs its handler with the update's message.
- `X17.49` behavior (D17.3; `CallbackProperty.tla:EveryHearerHearsOnceInOrder`, `CallbackProperty.tla:AThrowSkipsNoHandler`):
  a page bell's callback property heard by two handlers of the page's plan and by a `listen` in a slot's plan and in a
  slot's nested in it (D12.8): a ring runs the bell's own function first, then each listener once in the order it
  subscribed; a closed slot's listener stops, from the middle of the list or its end, and the rest go on; a reloaded
  slot's listener runs after the others; a ring whose own function throws runs every listener, and the bell meets the
  throw.
- `X17.50` behavior (D17.3; `CallbackProperty.tla:ALiveHandlerKeepsTheFrameworksFunction`,
  `CallbackProperty.tla:TheLastReleasePutsBackItsValue`): the bell's property holds the framework's function while any
  handler listens and its own function again after the last slot closes; a script that assigns it silences every
  handler, a slot loaded after it runs the script's function first, then its handler, and the last release leaves the
  script's function.
- `X17.51` behavior (D17.3, D12.8; `CallbackProperty.tla:ARefusedAssignmentStartsNoHandler`): a frozen bell starts no
  handler of a slot loaded after the freeze and shows one error naming the bell and its property, and a `listen` on it
  stops its pipeline; a bell frozen while handlers listen keeps them, and the last release's put-back is its error,
  after which a ring runs the bell's own function and no handler.
- `X17.52` behavior (D17.3, D12.5; `CallbackProperty.tla:AReplacedObjectIsNotWritten`): a bell its host restarts while a
  slot listens: the slot's release writes nothing to the old bell or the new, and a slot loaded after the restart hears
  the new bell.
- `X17.18` analyzer (D17.4): an argument at a position read on a declaration that is neither an event's payload nor a
  callback's arguments: "An argument at a position is what a listener or a callback receives: declare it on an
  event's payload or a callback's arguments." At build by ALIS034, and where the view renders on code it does not follow (D18.6): new:N18.12.
- `X17.15` behavior (D4.8, D17.4): an event declared with a payload that its vendor raises with no argument stops
  before its first step, and the vendor goes on after its listeners; an event declared with none runs its handler.
- `X17.14` behavior: a vendor's label callback shows, for each label, the text the view's expression answers.
- `X17.19` analyzer (D17.12): a step written inside a callback's answer: "A step is written inside a callback's answer,
  which answers its vendor with one value. Write the step in a pipeline." At build by ALIS032, and where the view renders on code it does not follow (D18.6): new:N18.9.
- `X17.20` behavior (D17.12): a callback's answer reads the page as it is on each call, a text box's value and a
  method's result alike, and a method's argument written as an answer is the key the vendor sorts by.
- `X17.21` behavior (D17.12): an answer that stops while a step's write made the vendor ask stops that pipeline
  before its next step, and the vendor keeps what it drew before.
- `X17.22` behavior (D17.12): an answer that stops when the vendor asks in its own turn fails the vendor's call,
  and the vendor keeps what it drew before.
- `X17.23` behavior (D17.4, D17.12): a value accessor's answer reads the row its vendor passes at position 1, and the
  chart shows each row's label as its value.
- `X17.24` behavior (D17.14): a shift log the page provides is read, written and called from a handler, a condition's
  term and branch, and a dose chart's answer; it raises its events as an EventTarget, a night clock through a callback
  property and a bed store to a subscriber whose unsubscribe it returns; a plugin that is a function answers the
  resident's initials.
- `X17.25` behavior (D17.14): a step reaching the visitor book before a script provides it stops its own pipeline and
  the page's other pipelines run; once the page's code provides it, after the page started, the next step finds it,
  and the book's signed event reaches no handler of the page.
- `X17.26` behavior (D17.14): a shift log two scripts provide stops each step that reaches it, and a ward lantern
  provided as a number stops each step that reads a colour a number lacks; the page's other pipelines run.
- `X17.27` analyzer (D17.14): a plugin named with a dot: "Plugin name 'alis.log' must start with a letter and contain
  only ASCII letters, digits, underscore, or hyphen. A name with a dot is the framework's own." At build by ALIS034, and where the view renders on code it does not follow (D18.6): new:N18.12.
- `X17.31` type (D17.15): a row of another type rendered by an action does not compile:
  `spec/api/HttpSurface/refused/RowOfAnotherType.CS1503.cs.txt:12`.
- `X17.32` analyzer (D17.15): a link whose block's one request is no GET, whose route or query value is neither a C#
  value nor its row's member, or that sends a container, an input or a header: new:N18.13.
- `X17.33` behavior (D17.15): a list of rows with three actions; a click on one row's Discharge posts that row's id,
  and its control shows Discharged while the other rows' controls are unchanged.
- `X17.34` behavior (D17.15): an unmodified click on a row's link loads its panel and the address bar stays; a
  modified or middle click sends no request and opens the link's page in a new tab.
- `X17.35` behavior (D17.15, D9.4): a link's href and the URL its click requests are equal for a key holding a space,
  `+`, `'`, `(`, `~` and "é".
- `X17.36` behavior (D17.15, D11.7): under `replace`, row A's link and then row B's are clicked while A's request is in
  flight: the panel shows row B.
- `X17.37` behavior (D17.15, D12.8): rows rendered in a slot; after the slot reloads, one click on a row's control
  sends one request.
- `X17.38` behavior (D17.15, D10.21): a row's control shows busy while its request runs, and only that row's.
- `X17.53` analyzer (D17.6): a declaration built again after an upgrade changed its vendor's TypeScript, with no C#
  changed, is checked against the changed TypeScript: a member the upgrade removes fails that build with ALIS016, so
  the analyzer's test builds one project twice with the TypeScript changed between the builds: new:N17.2.
- `X17.54` type (D17.11): a vendor's method is handed an abort signal only at an argument its declaration declares of
  N17.5's kind, so a call passes no argument its declaration does not name: new:N17.5.

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
  Gate 1 granted four more, which the rewrite's list holds and the POC's does not:
  - a declaring class's name for an input's id and its refusals, `Type.Name`, `Type.GetGenericArguments`,
    `Type.IsArray`, `Nullable.GetUnderlyingType` and `Type.IsInterface`, beside the `Type.FullName` the POC's list holds,
    in the file the rewrite's inputs unit names ids in (C11, D14.3); `Type.GetInterfaceMap`, which maps members, is
    refused;
  - an enum's values and names for D9.13's rows, `Enum.GetValues(Type)` and `Enum.ToString`, with `Type.IsEnum` and
    `Nullable.GetUnderlyingType` to find the enum, in `EnumNames.cs`, the file the rewrite's request unit adds to the
    Types module (C37). The generic `Enum.GetValues<TEnum>()` is absent on net48, and no door reaches a
    `TEnum : struct, Enum`;
  - `Activator.CreateInstance<T>()` for D5.12's declarations, which C# writes for `new T()` under a `new()` constraint
    on net48 and net10 alike, calling the public parameterless constructor the compiler checked and listing no member
    (C12). The typed alternatives each cost the developer: a factory every door takes
    (`pipeline.Component(() => new Monitor(), id)`), written at every reach of a declaration, or a static abstract
    factory on the declaration, which net48's runtime does not support;
  - `Expression.Compile` in validation, where the server runs a condition, a member read and a fixed operand, the tree
    the browser reads (Q16, C13).
  The POC stands until the rewrite's request, declarations, validation and inputs units: it sends an enum a source
  holds as it is held, its `Enum.ToString` of a literal and its `new T()` are on no list in `WhenTheCoreIsCompiled`,
  and it refuses a member a generic class declares.
- D18.2 One item-type finder: the contract's element type for a collection, and the key and element types for a
  dictionary. The three finders of today leave.
- D18.3 Each reflection site of today gets its verdict (B11):
  - a new model built by `Activator` and constructors: System.Text.Json's object read from `{}` (D16.6);
  - object and enum literals read member by member: the literal is written by System.Text.Json under the app's
    options, through the literal's `T`;
  - a markup payload read by `GetValue`: the payload is written by System.Text.Json through its `TPayload`;
  - a request member found by name, ignoring case: a request sends a container typed by the request's model and
    `Input`s typed by the members they fill, so the compiler matches what it sends to the request's members (N18.3),
    and a placeholder is filled by a member
    the request sets, named by its expression (D9.4);
  - members listed with `GetProperties` and `JsonIgnore` read: the contract's members;
  - `JsonPropertyName` read from a member: the contract's JSON name, which also follows the app's naming policy;
  - the assembly scan of validator registration: refused (Q16), `Add<TValidator>()` per validator;
  - the member scan that confirms a declaration's caller (`JsObject`): an analyzer checks at build time that a
    `Method`, `MethodAsync` or property declaration is called from the member it declares (N18.2). Built in POC 13e:
    a door is the body of the property or method it declares, never in a constructor, finalizer, setter, indexer,
    event accessor or static member, nor inside a condition, a lambda, another door's arguments or a body with other
    statements, and its `declaredBy:` is the compiler's (ALIS023); its `name:`, `optional:`, `nullable:` and
    `position:` are constants the build reads (ALIS024). An explicit interface member declares the JavaScript member
    of its simple name, as `[CallerMemberName]` names it;
  - the method scan for `PureMethodAttribute`: refused at gate 1 (C14); a model method carried into the plan is
    decided when a real case raises it;
  - the `FlagsAttribute` read of `IsInEnum`: the typed door `Flags<TEnum>()` (N16.4, gate 1's P2), which ALIS038 holds
    to the `[Flags]` mark at build (N16.5);
  - `Expression.Compile` in validation (Q16): listed at gate 1 (C13, D18.1); in the template writer (Q15): not listed,
    since the writer is the Fusion templates module's, which asks the owner for its own exception when it is onboarded.
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
    sees it on net48 too. The probe of POC 13f (net10, the reflection resolver) found more: an override with no
    `JsonPropertyName` over a declaration that has one is written under both names (`{"p":"v","x":"v"}`), at every
    link of a chain, and a `JsonIgnore` condition on the override alone changes when it is written. So ALIS025
    refuses an override whose `JsonPropertyName` or `JsonIgnore`, its condition included, is not the one of the
    member it overrides, each link checked against the member it overrides.
- D18.4 An analyzer fails the build on any reflection call in a shipped package outside the exceptions the owner
  listed at gate 1 (D18.1, N18.1): the members of `System.Reflection` that list, read, write or invoke members or read
  attributes, `Activator`, `Type.GetType` by name and `Expression.Compile`, except the owner's exceptions D18.1
  lists. It reads operations, not names: `new T()` over a type parameter, an `ITypeParameterObjectCreationOperation`
  whose source never names `Activator`, is an `Activator` read, which `WhenTheCoreIsCompiled`, reading the names the
  source writes, misses. A type's identity stays open to the framework (D18.1). The POC bans the whole `System.Reflection`
  namespace; each owner-listed read of it sits in a method that carries `SuppressMessage` for RS0030, and the list
  of D18.1 holds every read.
- D18.5 Every door on the public surface is typed (ALIS009). The three strings of B13 become typed doors:
  - a button's type is HTML's closed set, `button`, `submit` or `reset`;
  - a text box's type is HTML's closed set of text input types;
  - an action link's attribute is a typed name, with `data-` and `aria-` attributes by their own doors.

  Their forms go to gate 2.
- D18.6 A rule the source states sits on its highest rung (the owner, 2026-11-08): a type stages the door, else an
  analyzer refuses it at build, else it is refused where the view renders. The render takes only what the build
  cannot see, a closed set: the app's options (its JSON options, its culture, its startup choices), a value the view
  holds when it renders, and the plan as the render composes it (partials, slots, ids). Each analyzer runs the same
  check, with the same message, that the render keeps for code the analyzer does not follow (a helper method a block
  calls); a line of this design that says a rule the build sees is refused where the view renders means both.
  - Moved to an analyzer, by family: ALIS030, the request (X9.5, X9.6, X9.7, X9.12, X10.18, X10.25, X14.6, X20.6;
    N18.7); ALIS031, `ShowValidationErrors` outside a route (X16.21; N18.8); ALIS032, blocks (X5.13, X5.14, X6.8, X8.5, X8.9,
    X8.16, X8.17, X8.18, X8.25, X11.1, X17.13, X17.19; N18.9); ALIS033, lambdas, member lambdas and literals (X2.5,
    X3.11, X4.12, X6.2, X6.6, X6.10, X6.13, X6.14, X6.17, X6.18, X6.21, X6.22, X6.23, X6.25, X6.30,
    X6.31, X6.33, X6.38, X6.49, X6.50, X8.2, X8.10, X14.3, X14.13, X14.14; N18.10); ALIS006, the validator (X16.11, X16.12, X16.13,
    X16.14; N18.11); ALIS034, declarations (X17.5, X17.18, X17.27; N18.12); ALIS035, a row action's link (X17.32;
    N18.13), and ALIS038, the flags door (X16.23 to X16.26; N16.5), new beside the moved ones.
  - Kept where the view renders, each by what only the render knows: the app's JSON options (X1.4, X3.12, X4.13,
    X6.12, X6.28, X6.39, X6.40, X6.41, X6.42, X6.43, X6.54, X6.57, X7.10, X7.11, X7.13, X8.19, X8.20, X8.21, X8.22, X14.15, X18.8), its culture
    (X6.36), its startup choices (X21.11), a value the view holds when it renders (X6.15, X6.24, X6.48, X14.4, and a
    URL, status, header or pattern computed there), and the plan as composed (X12.11, X14.5). A plan's version
    is the browser's, at boot (X19.3).
  - Every analyzer of both lines has its verdict. ALIS001, a `When` given no `Then`: kept, retargeted to the
    conditions of `Pipeline<TModel>` in every block and file. ALIS002, main's one request per action link: kept as
    ALIS008 on the action's block, since an action's handler is a block (D11.10). ALIS003, a second `Reactive` for
    one event on one builder chain (`Alis.Reactive.Analyzers/ReactiveEvent/DuplicateReactiveEventAnalyzer.cs:21`):
    refused, since two handlers of one event are D11.5's meaning, each with its own policy, whatever the vendor's
    listening kind (`tests/Alis.Reactive.Poc.PlaywrightTests/Events/WhenAnAlarmIsAcknowledged.cs:18`), a callback
    property's chained as D17.3 states (gate 1's P4); one pipeline for both is one `Reactive`, the door its message
    names, and an event on a callback property that answers its vendor is ALIS018 (X17.16). ALIS004: kept, retargeted
    (N18.6). ALIS005, a rule only the server runs, reported as information: kept on `ReactiveValidator<T>`, so the
    developer sees where each rule runs. ALIS006, main's warning on a condition with no browser form: kept, an error
    (N18.11). ALIS007, a second `Chained`: the chain does not compile, since `Chained` ends the request's sentence
    (`SecondChained` in `spec/api/refused.sh`), and a request held and written twice is ALIS030's single use, which
    carries it (N18.7). ALIS008: kept, retargeted (D9.17, N9.9). ALIS009: kept (D18.5).
    ALIS010 to ALIS013, the slice tree (a use of a later row, a namespace other than the folder's, a public class
    left unsealed, a table that disagrees with its project; M20.D2 to M20.D6): kept for each project that carries a
    slice table, since a module is a namespace and a folder (the owner, 2026-11-06). ALIS014 to ALIS029 are the POC's, each at its own line.

**Schema nodes.** None: type knowledge lives in C#, and the plan carries only what it decides: JSON names,
shapes, starts and the payload's path of each event.

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
- `N18.2` new: the declaration analyzer of D18.3, which replaces `JsObject`'s run-time member scan. Built in POC 13e
  (ALIS023, ALIS024).
- `N18.3` new: a container typed by its model (N14.3), and a request's `Form` and `Inputs` that take only a
  container of the request's model.
- `N18.4` new: the typed doors of D18.5 for the button type, the text box type and the action link's attributes.
- `N18.5` new: the override analyzer of D18.3: an override in an app's model carries the JSON name and the
  `JsonIgnore` of the member it overrides. Built in POC 13f as ALIS025.
- `N18.6` new: ALIS004 retargeted from `PipelineBuilder` and `TriggerBuilder` lambdas in Razor files
  (`Alis.Reactive.Analyzers/ControlFlow/ControlFlowInReactiveCallbackAnalyzer.cs:98`) to every block of
  `Pipeline<TModel>` (D11.10), expression and `{}` lambdas both, in every file, keeping its allowed statements (a
  block, an expression, a local, a local function and the loops) and its refused ones.
- `N18.7` new: ALIS030, the request analyzer: on a constant URL, a fragment, a brace without its pair, a placeholder no
  member fills and a URL neither a page path nor an http or https URI (D9.4); a list, an object, a file, a date or an
  instant member in a placeholder; a `Path` whose member names no placeholder or belongs to an object the request
  holds, a placeholder only `Query` names, and a member lambda that is not a member path (D9.4); on a constant header
  name, one that is no token, one given twice, `Content-Type`, `Accept`, the antiforgery header, `traceparent`,
  `tracestate`, `baggage`, the Fetch Standard's forbidden request headers and the method overrides (D9.11), and on a
  constant value one outside RFC 9110's field value (D9.23); a constant value no member of its enum names (X9.22); a JSON body member under a number-keyed dictionary (D14.5); a second `Busy` or `NoBusy`,
  `Busy` beside `NoDefaults()` (D10.21), a status outside 300 to 599, a second route for one status and a second
  catch-all (D10.13); a staged value used more than once, a request's parts object written by two statements or a
  request held in a variable and written twice or after another step, so a door a stage removes cannot come back
  through a second use and a route stays in the sentence of its request, which carries ALIS007's second `Chained`
  (N10.7).
- `N18.8` new: ALIS031, `ShowValidationErrors` in a block that no route encloses (D16.13).
- `N18.9` new: ALIS032, the block analyzer: a step written on the pipeline of a block around its own (D11.10); a
  bound value carried out of its block through a variable declared outside it (D8.8, D5.11); an `ElseIf` or `Else`
  after another step, a second `Else` (D8.7); a result called in an `ElseIf`'s condition read after its `When`, and an
  async method under a term reached later (D4.7); a write to the payload after an awaited step, a request's own
  `WhileLoading` before its send and the rest of the request after it (D11.4), and inside a `listen` (D12.8); a step or an awaited read inside a list's lambda or a callback's answer (D6.8, D17.12).
- `N18.10` new: ALIS033, the lambda analyzer: a member lambda that names the model (D2.5, D14.2); an input of a member
  an interface or `Nullable<T>` declares (D14.3); a dictionary of dates
  written into a live member (D3.13); a constant literal no live number holds (D7.7, X7.13); in a list's lambda and a pipeline condition, a term outside the identity table
  (D6.5), a view value that computes (D6.7), and each comparison, ordering, cast, coalesce and lookup D6.12 to D6.14
  and N8.2 refuse by the types they read; a constant pattern outside D16.11's closed set (D6.15); in a pipeline condition, a constant `MinLength` below zero (M3.D1, M3.X4); a constant position below zero given to `At` (D1.1, M4.X10).
- `N18.11` new: ALIS006 kept as an error, the validator analyzer: a condition with no browser form outside
  `WhenOnServer` and `UnlessOnServer`, a constant pattern outside D16.11's closed set or with an option other than
  `CultureInvariant`, and on net48 a model type with no public parameterless constructor (D16.6, D16.11).
- `N18.13` new: ALIS035, the row-action analyzer: a `NativeActionLink` whose block's one request at its own level is no
  GET, whose route or query value is neither a C# value nor `Row.Read(..)` of its payload, or that sends `Form`,
  `Inputs`, `Input` or a header (D17.15).
- `N18.14` new: ALIS040, the nullable source analyzer: a source passed where a method's or a plugin's argument or a
  condition operator's right operand takes its type with other nullability, which C# reports as the warning CS8620,
  is refused as an error in every compilation that references the core, whatever the app does with warnings (D7.3,
  gate 2's G8). No overload can refuse it, since nullability is no part of a C# signature. The analyzer reads the
  annotations the compiler reads, so where a file's nullable context is disabled the two types are one and the source
  is passed as held. `OrNull()` (N7.5) widens a source to its nullable type, and `IfNull` (N7.4) narrows one.
- `N18.12` new: ALIS034, the declaration analyzer: a constant vendor or plugin name outside its characters or with a
  dot (D17.3, D17.14), an argument at a position on a declaration that is neither an event's payload nor a
  callback's arguments (D17.4), two constant definitions of one vendor name whose data differ in one compilation
  (M12.D1, M12.X1), a constant object path or element path with an empty step (M12.X3), and two constant declarations
  of one plugin name, each declaring an event, whose listening differs in one compilation (M13.D1, M13.X1).

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
- `X18.1` analyzer (D18.1, D18.3): a `GetProperties` call in a shipped package outside the owner's list: new:N18.1.
- `X18.2` analyzer: `Method()` called from a member other than the one it declares: new:N18.2 (POC 13e).
- `X18.3` type: a request that sends a container of another model does not compile: new:N18.3.
- `X18.4` type (D18.5): a button's type written as text does not compile: new:N18.4.
- `X18.5` behavior: on net10, with the app's JSON options naming members in snake case, a request's JSON body sends
  `given_name`, and the endpoint binds it (`probe:core-frombody-naming-policy`).
- `X18.6` behavior: a member the app's JSON options ignore is not in a JSON body, and the check reads its
  start, as the endpoint binds it (`probe:core-frombody-left-out-member-starts`).
- `X18.7` behavior: a read of a member an override implements reads it by its declaration's JSON name
  (`WhenARosterIsHandedOver`, POC 3).
- `X18.8` refusal: a read of an override the app's JSON options ignore is refused with "'{lambda}' reads a member
  of '{type}' that the app's JSON options do not write, so the browser never holds it." (`WhenARosterReadIsRefused`).
- `X18.9` analyzer: an override that renames, ignores or writes what its declaration does not: new:N18.5 (POC 13f).

## Q19. The schema

**Decision.**
- D19.1 The schema is `spec/Core/schema.md`: every node, what it carries, the nodes it holds and its meaning in the
  browser, derived from Q1 to Q18, Q20 and Q21. C# writes those nodes and nothing else, and the browser runs those nodes
  and nothing else, so the runtime and the builders proceed in parallel from that file. The builders and the JSON they
  write are one track: one C# model writes the grammar, with no lowering pass (the owner, 2026-11-06, and the answer in
  `spec/NOTES.md`) (`check:kinds`, `Schema.tla:KindsHandledOnce`).
- D19.2 The set is closed at 51 kinds: today's 33 but `try`, and `request`, `stop`, `listen`, `data`, `start`,
  `nothing`, `unbound`, `signal`, `answer`, `file`, `each`, `returned`, `living`, `held`, `stream`, `clock`, `zone`,
  `off` and `on`, the 19 `spec/Core/schema.md` marks added and `spec/Schema/Schema.tla` checks. `try` leaves, its
  `finally` carried by a `request`'s layers, since nothing but a request's `Finally` wrote it (the owner's amendment of
  G2, 2026-11-08, D10.23); no other kind leaves. After gate 1 a unit never changes the set: a behavior the nodes cannot express stops the unit and goes
  to the owner with its use case (`check:kinds`).
- D19.3 Every field is required, and none holds null as a state; a value that can be absent is a closed union with a kind per case. Null
  is only a value data holds: a `lit` of null and the JSON of a start (the owner, 2026-11-06).
- D19.4 C# declares no enum (schema.md S8). A closed choice with no data is a sealed class with a private constructor
  and one static instance per case, written as its text; a choice whose cases carry data is a kind. `Operator`
  and `PathMembers` become such classes; the public `VendorEventPayload` leaves for N17.1's path (D17.4).
- D19.5 Every kind has exactly one handler. The switch that picks a default by shape leaves, since C# writes each
  input's blank (D3.6, N3.3), and the object locator's two switches become one switch that returns the locator's
  driver (G1) (`Schema.tla:KindsHandledOnce`).
- D19.6 A handler carries its payload's path (D17.4), and the vendor does not: the vendors map is shared by name across
  plans, so two plans that listen to different events of one component, read differently, agree on the vendor.
- D19.7 The form plugin's data is one registration, one validator under one rule set and keyed by both, holding the
  model's start, the bound scalars, objects and lists of D16.6 and D16.7, and a rule tree whose groups follow the
  validator's child validators (D16.3). A container is an entry of `model`'s data, keyed by its element id, with its
  model, its kind (a form or another element), its summary's id and its live check. The brief's item raised after the
  fact is decided: `FormField`'s `items` and `value` functions leave for paths with `each`, and `live` stays, as
  `Live`, a container's, naming the check it runs.
- D19.8 A number shape carries what a text converts under: `of`, an integer or a fraction, which picks the culture's
  number text for the host, and `text`, the view's culture or the invariant one (D3.11).
- D19.9 `HttpSend`, `HttpResponse`, `FormCheck` and `FormPlace` are typed contracts generated beside the grammar, not
  kinds (G2): `HttpSend` is a `request`'s `send` value and `HttpResponse` the reply it binds (D10.23) (`check:kinds`).
- D19.10 The plan's version is 7. A plan of another version is JSON the framework did not produce, refused at boot with
  one error (M14.D1), and at a slot's load by stopping the load before its release (M9.D2).

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
- `Schema.tla:DecimalTextComparedByValue`
- `check:kinds`, which reads the grammar and `plan.schema.json` at 65fdf118d, schema.md's kinds table and Schema.tla's
  node set, and checks each today line holds its case.

Each Schema.tla invariant is the browser's need, and each mutant drops the one refusal of the C# writer that serves it.

**Doors.**
- `N19.1` new: the sealed choice classes of D19.4 in `Alis.Reactive/Grammar`, replacing `Operator` and
  `PathMembers`; `VendorEventPayload` leaves for N17.1's path (D17.4).
- `N19.2` new: `Registration`, `BoundScalar`, `BoundObject`, `BoundList`, `RuleGroup`, `RuleField`, `RuleTest` and
  `Container` in `Alis.Reactive/Grammar/PlanDocument.cs`, replacing `FormField` and `FormTest`; they carry N16.2.
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
- `X19.1` type (D19.4): an operator or a path's members made from an integer does not compile: new:N19.1.
- `X19.2` type (D19.3): a grammar node built with a field left out does not compile, since each constructor takes every field
  and none is nullable: new:N19.3.
- `X19.3` refusal: a plan of version 6 is refused at boot with "This plan was written for plan version 6; this page
  runs version 7."
- `X19.4` behavior (D19.6): two partials whose plans listen to different events of one component, one reading its argument
  and one its event's detail, each run their steps with the payload they declare.

## Q20. Tracing

**Decision.**
- D20.1 Tracing is off unless the server recorded the render. C# writes the plan's `trace` as `on` only when the
  activity of the request that renders the view is recorded, W3C Trace Context's sampled flag, and as `off` otherwise.
  The browser decides nothing but D20.11's: no attribute, no query text, no storage, so the level read from
  `data-alis-trace` leaves. In development the app's sampler records every request; in production the app sets its own ratio (the
  owner, 2026-11-06: optional, never always on) (`Tracing.tla:NothingRecordedWhenOff`).
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
  in the trace. The trace context is the run's, as its blocks' abort signals are (Q11), and `HttpSend` carries none.
- D20.4 The spans are closed: a plan's boot, a handler run, a send and each of its attempts, a `MethodAsync`, a
  `Confirm`, a hub invoke, each block of an `all`, and a slot's load and unload. A synchronous statement is no span: a
  `stop` with its reason, a required member a holder lacks, a refused check and a boundary error are events on the span
  that holds them. An attempt follows OpenTelemetry's HTTP client conventions: the method, the status, the resend count
  and the URL as its route template. A send starts with its first attempt, after the request's `WhileLoading` and its
  guard (`Request.tla`'s `loading` and `guard`), so a request stopped or cut before it sends records no send, and a
  `MethodAsync` its `WhileLoading` awaits is a child of the handler run's span (`Tracing.tla`'s `Attempt`).
- D20.5 No value is recorded. A span records ids, kinds, member wire names, event names, the route template, the status,
  the attempt and the outcome, never an input's text, a body, a payload, a query value or a path value, so the app's
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
  `System.Diagnostics.DiagnosticSource` package, pinned to one version (gate 1, C8), and joining an inbound `traceparent` takes the
  app's ASP.NET instrumentation module, a fact U7's first step probes on verify-net48 before the unit claims it.
- D20.9 Tracing changes nothing a plan does: on and off run the same statements in the same order with the same
  results, and the port returns nothing the runtime reads.
- D20.11 A retry is always traced (the owner, 2026-11-06: downstream services act on it, by standard headers). Every
  attempt after a send's first sends `traceparent` with the sampled flag set, so the server's parent-based sampler
  records the endpoint and what it calls, and W3C `baggage` with `http.request.resend_count`, OpenTelemetry's name
  for the attempt count, so the server copies it into its span. With the trace on, the context is the attempt's own
  span (D20.3); with it off, the send mints one trace id for its retries, and the first attempt carries nothing, since
  no one knows it will fail. Both headers go to the page's own origin only; the services behind the endpoint receive
  them from the server's own propagation.
- D20.10 Its proof is Playwright on the conformance app's tracing hosts, whose endpoint echoes the `traceparent` it
  received into the page and whose collector page lists the spans it received: with the trace off nothing is listed
  and no header is echoed (X20.1); with it on, the closed span names of a retried GET (X20.2) and of a handler with a
  check and a chained request (X20.8) are listed under the page's trace id.

**Schema nodes.** `PlanDocument.trace`, the kinds `off` and `on`, and `Source` (Q19).

**Checker.**
- `Tracing.tla:NothingRecordedWhenOff`
- `Tracing.tla:EveryStartedSpanEnds`
- `Tracing.tla:ChildEndsBeforeParent`
- `Tracing.tla:AttemptsJoinTheTrace`
- `Tracing.tla:TraceHeadersStayHome`
- `Tracing.tla:SinkFailureChangesNothing`
- `Tracing.tla:SinkFailureKeepsHeaders`
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
- `X20.1` behavior (D20.1): with the trace off, a save lists no span on the collector page, and its endpoint echoes no
  `traceparent`.
- `X20.2` behavior (D20.3, D20.4, D20.9): with the trace on, a GET retried once after a 503 lists two attempt spans under one send span,
  under the page's trace id.
- `X20.3` behavior (D20.4, D20.6, M26.D1): a handler replaced while its request is in flight lists its run span ended with the outcome replaced.
- `X20.4` behavior (D20.3): with the trace on, a request to another origin carries no `traceparent`, which that endpoint echoes.
- `X20.5` behavior (D20.2, D20.6, D20.9, M26.D1): with an exporter that throws on every span, a save succeeds and the console shows one error.
- `X20.7` behavior (D20.11): with the trace off, a GET retried after a 503 reaches the endpoint the second time with a sampled
  `traceparent` and `baggage` holding `http.request.resend_count=1`, which the endpoint echoes, and the first time with
  neither.
- `X20.6` analyzer: a request header named `traceparent` is refused where the view renders with "The traceparent header
  belongs to tracing: remove it from this request's headers." At build by ALIS030, and where the view renders on code it does not follow (D18.6): new:N18.7.
- `X20.8` behavior (D20.5): with the trace on, a dose saved for Ruth on the ward North, whose check passes and whose
  success chains a GET of the ward's census, lists under the page's trace id a run span holding the POST's send and
  its attempt, by the route template `/Wards/{ward}/Doses`, and the chained GET's send and its attempt, and no span
  or event holds Ruth or North.
- `X20.9` behavior (D20.8): with the trace on, a save the endpoint's validation door refuses lists under the page's
  trace id the plan's render activity and, under the save's attempt span, the endpoint's validation activity naming
  its validator, its rule set and one failure.

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
  token in the URL for WebSockets and server-sent events. 1.0 has no token door (gate 1, C26).
- D21.8 Tracing (Q20): a run a message starts is a child of the plan's trace context and records its transport and
  event. The server span that sent the message is not linked, since neither wire carries a trace context per
  message. A hub invoke does not join the server's hub activity either: `@microsoft/signalr` 10.0.11 sends no trace
  header by long polling or on its WebSocket upgrade, and its invocation message holds only arguments, invocationId,
  target and type on both transports (`probe:signalr-requests-carry-no-trace-context`,
  `probe:signalr-invocation-message-keys`).
- D21.9 Proof:
  - Playwright on net10 covers both transports, every lifecycle trigger, the holds, the reconnect offer and a
    request a message's run sends under the defaults.
  - The verify-net48 run covers that a net48 view writing a realtime door does not compile.
- D21.10 Realtime and the request defaults (Q10). A connection, its reconnection and its messages hold no busy indicator
  and show no default notice: a connection lives as long as its holders, so an indicator that covered a reconnection
  would show for the whole outage while the page stays usable; the connection's state is its own member and its
  lifecycle triggers (D21.5). A message starts a run as any trigger does (D11.7), and a request that run sends takes the
  request defaults and is counted with every other request of the page (`Realtime.tla:IndicatorNeverHidesEarly`,
  `Realtime.tla:IndicatorNeverStaysShown`, over the page's target; `Composition.tla` counts each target). A hub's `invoke` and `send` are calls, not HTTP requests, and hold nothing.
- D21.11 The reconnect offer is a contract the core owns and a component fulfils, as the busy indicator and the notice
  are (D10.21): the plugin `reconnect`, with `show(retry)`, `retry` a function that reconnects every connection the page
  holds that gave up, and `hide()`. The framework ships a Native component declaring it, and an app registers its own
  under the same name; the core depends on neither's markup or CSS.
  - The core counts the held connections that gave up or are retrying after one, across every plan of the page, and
    calls show when the count leaves 0 and hide when it returns to 0. A connection leaves the count when it opens or its
    last holder releases it (D12.6), so a retry keeps the offer until a connection opens, and a retry that fails keeps it
    shown (`Realtime.tla:OfferNeverHidesEarly`, `Realtime.tla:OfferNeverStaysShown`).
  - An app that renders a stream or a hub chooses at startup, `ShowReconnect()` or `NoReconnectOffer()`, as it chooses
    antiforgery (D9.22). A view that names one in an app without the choice is refused, naming both, since a page whose
    connection gave up, with no offer and no `closed` steps, goes stale and shows nothing. Under `ShowReconnect()`, a
    page whose scripts provide no `reconnect` stops the connection before it opens and logs one error, so the missing
    offer shows at the first load, not at the first outage.
  - Main's one layout element, `alis-realtime-connection-retry-container`, whose click retries every dropped
    connection, is carried by this contract.
- D21.12 A stream and a hub are JS objects, and every member is a source (Q7). Their state (connecting, open,
  reconnecting, closed) is a member a condition or a step reads; their lifecycle triggers are events (D21.5); each
  declared message (D21.3) is an event whose payload's `Data` dot-walks (D21.4); a hub adds its declared methods,
  `invoke`, whose result is a value read as a method's result is (a method that returns a promise, Q11), and `send`. A
  message's data and an invoke's result feed every module: a condition, a request's route, query, header or body, an
  element's text, a component's member and a dispatch. Server push is in 1.0; a response body read as a stream (a
  fetch `ReadableStream`, an AI or chat stream) stays out by the owner's 2026-06-10 decision, so D10.6's closed set of
  response kinds holds no stream kind, and the two decisions do not overlap.
- D21.13 A connection's waits are stated in C# and carried, never a default nobody wrote:
  - A stream's reconnection waits the `retry:` the typed event writer (N21.2) sends with the stream's first event, as
    `SseItem<T>.ReconnectionInterval`: the app's `ReconnectionInterval(TimeSpan)` on `AddReactiveRealtime` (N21.9), and
    3 seconds when the app states none, written so every browser waits the same, since the HTML standard leaves the
    default to the browser. The browser waits it on its own timer, which Playwright's clock does not drive, so the test
    app states 100 milliseconds.
  - A hub's plugin data carries its waits, 0, 2, 10 and 30 seconds, the SignalR client's own default
    (`node_modules/@microsoft/signalr/dist/esm/DefaultReconnectPolicy.js:4`, 10.0.11), written by the C#. The browser
    hands them to `withAutomaticReconnect` and tries a first start again on the same waits (M10.D1); after the last the
    hub gives up and runs its closed steps. The client waits each on a page timer
    (`node_modules/@microsoft/signalr/dist/esm/HubConnection.js:717`), so a test runs them on Playwright's clock (X21.13).

**Schema nodes.** The plugins `alis.stream` and `alis.hub`, their plugin data `stream` (new) and `hub`, each carrying
`offer` (D21.11), a hub's `waits` (D21.13), the contract plugin `reconnect`, `Handler` for message and lifecycle triggers, and `call` for a hub's
invoke and send.

**Checker.**
- `Ownership.tla:HubKeptWhileHeld`
- `Ownership.tla:OfferOnlyWhileHeld`
- `Lanes.tla:SkippedTriggerStartsNothing`
- `Lanes.tla:ReplacedRunRunsNoRoute`
- `Realtime.tla:IndicatorNeverHidesEarly`, killed by a message's request left out of the count
- `Realtime.tla:IndicatorNeverStaysShown`, killed by a reconnection that holds the busy indicator
- `Realtime.tla:OfferNeverHidesEarly`, killed by an offer that is a flag, not a count
- `Realtime.tla:OfferNeverStaysShown`, killed by an unload that keeps the offer

**Doors.**
- kept: N5.1's message triggers typed `Data<TMessage>`, and the hub's `invoke` and `send` (Q11).
- `N21.1` new: a server event and a hub's methods declared once and shared by the endpoint and the view (D21.3); their
  form goes to gate 2.
- `N21.2` new: the endpoint's typed event writer over `TypedResults.ServerSentEvents`, net10 only.
- `N21.6` new: every realtime door under net10's PublicAPI text only, behind the host split, none on net48's.
- `N21.3` new: the plugin data `stream` (D21.6).
- `N21.4` new: the lifecycle triggers `reconnecting`, `reconnected` and `closed` on a stream and a hub (D21.5).
- `N21.5` new: the hub's module as its own asset, linked when a plan names a hub (D21.1).
- `N21.7` new: the reconnect contract, the plugin `reconnect` with `show(retry)` and `hide()`, a Native component
  declaring it, and the startup choice on net10's realtime configuration, `services.AddReactiveRealtime(live =>
  live.ShowReconnect())` or `NoReconnectOffer()`, a `Hosting` door beside `AddReactiveRequests` (D21.11).
- `N21.8` new: a stream's and a hub's members as a JS object's, its state, its lifecycle and message events and a hub's
  methods, each a source (D21.12); their form goes to gate 2.
- `N21.9` new: `ReconnectionInterval(TimeSpan)` on net10's realtime configuration, the wait a stream's writer sends as
  its first event's `retry:`, 3 seconds when the app states none (D21.13).

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
- `X21.1` behavior (D21.1): a stream message whose data is not JSON runs no handler and logs one error, and the next message
  runs its handler.
- `X21.2` behavior (D21.6, D21.12): a slot that only reads a stream's state reads it open while the slot is loaded.
- `X21.3` behavior (D21.5, D21.9): when the server restarts, a page runs its `reconnecting` steps, then its `reconnected` steps, and the
  next message runs its handler.
- `X21.4` behavior: a hub push with two arguments runs no handler and logs one error.
- `X21.5` behavior (D21.1): a page whose plans name no hub requests no SignalR asset.
- `X21.6` type (D21.3): an endpoint and a view that name one server event differently, or give it two message types, do not
  compile: new:N21.1.
- `X21.7` type (D21.9): a net48 view that writes a stream, a hub or a message trigger does not compile: new:N21.6.
- `X21.8` behavior: while a stream reconnects and no request is in flight, the page shows no busy indicator, and a
  request a message's run sends shows it until that request settles.
- `X21.9` behavior (D21.5, M25.D7): with the page's stream and a slot's stream both given up, the reconnect offer shows once; unloading
  the slot keeps it, and the page's stream opening hides it.
- `X21.10` behavior (M25.D7): a retry keeps the reconnect offer shown until a connection opens, and a retry that fails keeps it
  shown.
- `X21.11` refusal: a view that names a stream or a hub in an app with no reconnect choice: "A page that names a stream
  or a hub needs the app's reconnect choice: call ShowReconnect() or NoReconnectOffer() in AddReactiveRealtime."
- `X21.12` behavior: under `ShowReconnect()`, a page that provides no `reconnect` opens no stream, and the console holds
  one error.
- `X21.13` behavior (D21.13): a hub the server refuses from its first start tries again after 0, 2, 10 and 30 seconds of
  the page's clock, then runs its closed steps and shows the reconnect offer.
- `X21.14` type (D21.7): a stream or a hub cannot be handed a bearer token: N21.8's members and N21.7's configuration
  take none, so the page's cookies are a connection's only credential: new:N21.8.
- `X21.15` behavior (D21.8): with the trace on, a message a stream pushes lists under the page's trace id a run span
  whose parent is the plan's trace context, recording its transport, server-sent events, and its event's name.
- `X21.16` behavior (D21.8): with the trace on, a click that invokes a hub method lists its run span under the page's
  trace id, and no request of the hub's connection, its negotiate included, carries a trace header, so no invoke's server
  activity takes the page's trace as its parent.
