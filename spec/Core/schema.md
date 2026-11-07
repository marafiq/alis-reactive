# The schema

The plan grammar the rewrite writes and executes: Q19 of `spec/Core/DESIGN.md`, derived from Q1 to Q18, Q20 and Q21.
C# writes these nodes and nothing else; the browser executes these nodes and nothing else. Both sides work
from this file in parallel: the C# side by the nodes each builder writes, the browser side by each node's
meaning, and `spec/Core/Schema.tla` checks that what C# accepts is what the browser runs.

## Rules of the schema

- S1 The set is closed. A node is one of the kinds in the kinds table below, and a field holds exactly the
  nodes this file names for it.
- S2 Every field is required. No field is optional, and none holds `null` as a state: a value that can be absent is a closed
  union with a kind for each case (the owner, 2026-11-06). `null` appears only as a value data holds: a `lit`
  of null, and the JSON of a start.
- S3 A kind is one name across the whole schema, so a kind names its node without its union.
- S4 Every kind has exactly one handler in the browser: one `case` in the one switch over its union, ending in
  `assertNever`, or, for plugin data, the plugin the plan names. `none` has no handler: it is the plugin data of
  a plugin that contributes nothing.
- S5 A path is a list of path steps (Q1): a JSON string is a member, a non-negative JSON integer is an index, and
  `{"kind":"each"}` is every item of a list. `each` appears only in a form's registration paths.
- S6 A name in a path is the member's wire name for the host the view renders on (Q2).
- S7 The plan's `version` is 7. A plan of another version is JSON the framework did not produce, refused at
  boot with one error.
- S8 C# declares no enum, in the grammar or on any door (the owner, 2026-11-06: enums breed bugs). A C# enum is
  open: `default` is its first member, any integer casts to it, and a switch over every named member still
  warns CS8524. Instead:
  - a closed choice that carries no data is a sealed class with a private constructor and one static instance
    per case, written as its text;
  - a choice whose cases carry data is a kind.

  The TypeScript contract is the union of those texts, and every switch over it ends in `assertNever`. Today's
  `Operator`, `PathMembers` and the public `VendorEventPayload` become such classes. The texts below in code
  font (`required`, `overlap`, `fill`) are the cases of such classes.

## The document

`PlanDocument` is the root of every plan.

| Field | Holds | Meaning |
|-------|-------|---------|
| `version` | the integer 7 | S7 |
| `planId` | text | the plan's id, by which plans compose (Q14) |
| `culture` | `Culture` | the view's culture (D3.11, N3.4) |
| `timeZone` | a time zone: `clock` or `zone` | the zone the view renders under (D3.12, N3.5) |
| `trace` | a trace: `off` or `on` | the render's trace context (Q20) |
| `vendors` | a map from vendor name to `Vendor` | how a component's object is found and listened to (Q17) |
| `objects` | a map from object id to an object locator: `element` or `plugin` | every object the plan reads, writes, calls or listens to (Q5) |
| `handlers` | a list of `Handler` | what runs on which event (Q11) |

`Culture` carries `name` (the .NET culture name), `collation` (the tag the browser's collator orders text by: the
name, `en` for the invariant culture, D6.13), `decimal` and `group` (its separators), and `integer` and
`fraction`, each a `NumberText`: the text a host's binder accepts for that kind of .NET number, `group` and
`exponent`, two booleans C# writes for the host (D3.11: on net10 a fraction takes both, on net48 a fraction
takes the exponent only, an integer takes neither on both hosts).

### Time zone

| Kind | Carries | Meaning |
|------|---------|---------|
| `clock` | nothing | the browser's clock and its offset (D3.12's default until gate 1) |
| `zone` | `id`, an IANA time zone id | the app's zone for this request, written by C# from a `TimeZoneInfo` (N3.5) |

### Trace

| Kind | Carries | Meaning |
|------|---------|---------|
| `off` | nothing | the server did not record the render: nothing is traced (D20.1) |
| `on` | `traceparent` and `tracestate`, W3C Trace Context text; `console`, a boolean; `sources`, a list of `Source`, one per handler in order | the render's span context, which every run joins (D20.3) |

`Source` carries `view` (the view's path as MVC names it) and `line` (the line of the builder call that wrote
the handler) (D20.7).

## Handlers and vendors

`Handler` carries:
- `on`: an object id of `objects`;
- `path`: a path to the object that raises the event, below the located object;
- `event`: the event's JavaScript name;
- `payload`: a path into the listener's arguments, `[0]` unless the event's declaration names another position,
  `[0, "detail"]` for a `CustomEvent` (D17.4), `[]` for an event declared with no payload, carried per handler since
  the vendors map is shared by name across plans;
- `members`: the presence the payload's path reads with, as `get` reads it; C# writes `required`, since a declared
  payload is what the listener receives (D5.3, D17.4);
- `policy`: what a trigger does while this handler's previous run is in flight, one of `overlap`, `replace`
  or `skip` (D11.7, N11.1);
- `do`: a list of statements, the handler's body block.

The payload is bound as `e` in the body block, and the listener's arguments as `arguments`, which a payload's
`Argument<T>(n)` and `MethodAt(n, ..)` read (D17.4).

`Vendor` carries `locate`, a `fn` of the element that returns the component's object: a `get` of a path below
the element, or a `call` of the plugin the vendor registers as its lookup (D17.3); and `listen`, a vendor
listening: `method`, `assignment` or `returned`. `Vendor.payload` of 65fdf118d leaves: it moves to each handler.

| Kind | Carries | Meaning |
|------|---------|---------|
| `method` | `subscribe`, `unsubscribe`: the object's method names | listen and unlisten by calling them with the event name and the listener |
| `assignment` | nothing | listen by assigning the listener to the object's member named by the event, and put the member's previous value back at release |
| `returned` | `subscribe`: the object's method name | listen by calling it with the event name and the listener; release calls the function it returned (D17.3) |

## Object locators

| Kind | Carries | Holds | Meaning |
|------|---------|-------|---------|
| `element` | `id`: an element id; `vendor`: a vendor name of `vendors` | | the component on that element at the vendor's object path. An element the page lacks, or one that holds no object at that path, stops the pipeline that reads it (D4.1). |
| `plugin` | `name`: the plugin's name; `key`: text the plugin reads | `data`: plugin data | the plugin's object for that key |

The plugins the core's C# names are `alis.http`, `alis.form`, `alis.model`, `alis.slot`, `alis.hub`, `alis.stream`
and `alis.document`, each provided by the core's own browser code. Any other name is a plugin an app or a component
brings. `alis.confirm` of 65fdf118d is Fusion's, so the core no longer names it (D17.9).

## Plugin data

| Kind | Carries | Holds | Meaning |
|------|---------|-------|---------|
| `none` | nothing | | the plugin contributes nothing to the owner: http, document, slot (its key is the container id, D12.1), an app's plugin |
| `hub` | nothing | | the owner holds the hub's connection while it lives (D12.6) |
| `stream` | nothing | | the owner holds the stream's connection while it lives, as a hub's (D21.6) |
| `model` | `inputs`: a map from input id to `InputBinding` | | the inputs this plan renders, by id (D13.2, D14.3) |
| `form` | `summary`: the summary element's id | `live`: `Live`; `registrations`: a list of `Registration` | this plan's rules for the form whose element id is the key (Q16) |

`InputBinding` carries:
- `name`: its binding name, the name the server speaks (D1.7);
- `path`: its body path, with no `each`;
- `value`: an expression C# writes as `coalesce(as(read, shape), blank)`, where blank is the member's start (N3.3);
- `committed`: the event that commits its value;
- `invalidClass`: the class an invalid input takes;
- `message`: the id of its message element (D16.13).

Its shape and list flag of 65fdf118d leave into `value`.

`Live` carries `validator` and `ruleSet`, the check live validation runs (D16.15), and `body`, an expression
that reads the form's inputs as the form sends them (D16.5).

`Registration` is one validator's rules under one rule set, for the form (D16.2). It carries:
- `validator`: the validator's key;
- `ruleSet`: the rule set's name;
- `start`: the JSON of the model's start (D16.6);
- `scalars`: a list of `BoundScalar`;
- `objects`: a list of `BoundObject`;
- `lists`: a list of `BoundList`;
- `rules`: a `RuleGroup`.

`BoundScalar` carries:
- `path`;
- `shape`: the member's shape;
- `before` and `after`: the text around the typed text in the binder's message, "The value '" and "' is not valid
  for Age." (D16.7).

`BoundObject` carries:
- `path`;
- `start`: the JSON of a new object of the member's type;
- `json`: `fill` or `replace`, what a JSON body does to the object the member starts with, for the host and the
  member's creation handling (D16.6).

`BoundList` carries:
- `path`;
- `item`: the JSON of a new item;
- `json`: `append` or `replace` (D16.6).

Form data replaces every list and fills every object on both hosts. An object that starts null is the type's
start under every format.

`RuleGroup` carries:
- `path`: a path below the instance of the group that holds it, `each` for every item;
- `when`: a `fn` of one parameter, the group's instance, whose body is the group's condition (D16.8);
- `fields`: a list of `RuleField`;
- `groups`: a list of `RuleGroup`.

The top group's path is empty and its instance is the checked body. A group for a child validator is held by
the group of its member, and its rules hold only while that member is present (D16.3).

`RuleField` carries `path` (below its group's instance, `each` for every item of a list of scalars) and `tests`,
a list of `RuleTest`. `RuleTest` carries:
- `check`: a `fn` of the value and the instance that is true when the value passes;
- `when`: a `fn` of the instance, the rule's own condition;
- `message`: the text C# wrote.

The verdict on 65fdf118d's `FormField` (`items` as `fn(sent)`, `value` as `fn(i, sent)`) and `FormPluginData.live`,
raised after the fact in the brief:
- The item and value functions leave. A field is a path with `each`, read over one checked body, and the form's
  plugin walks it (D1.5, D16.14).
- `live` stays, as `Live`, with the validator and rule set it runs.

## Statements

A block is a list of statements and a lexical scope (D8.8): a binding made in a block is read in that block
and in the blocks inside it, and nowhere else.

| Kind | Carries | Holds | Meaning |
|------|---------|-------|---------|
| `set` | `path`, at least one step; `members`: `required` or `optional` | `target`, `value`: expressions | write the value at the path below the target (Q2, Q4) |
| `do` | `await`: a boolean | `expr`: an expression | evaluate it; with `await`, wait for its promise (D4.3, D11.1) |
| `let` | `name`; `await`: a boolean | `expr`: an expression | bind its value, or its awaited value, in the block (D8.8); the expression runs once, and every `ref` of the name reads the binding (D5.11) |
| `if` | | `arms`: at least one `Arm`; `else`: a block | run the first arm whose `when` holds, else the else block (D8.6) |
| `all` | | `blocks`: a list of blocks | run every block at once; the next statement runs after every block settled (D10.10) |
| `try` | | `body`, `finally`: blocks | run the body; run `finally` once, however the body ends (D11.8) |
| `listen` | `event`: the event's JavaScript name; `vendor`: a vendor name of `vendors`; `payload`: a path into the listener's arguments; `policy` | `object`: an expression; `do`: a block | listen to the event of the live object the expression reads, once per object and statement, for the owner of the plan; run the block on each raise (D12.8) |
| `stop` | `reason`: text C# wrote | | stop the pipeline: run none of its later statements, log the reason, run the `finally` of every `try` it is inside (D4.6, N4.1) |

`Arm` carries `when`, an expression, and `then`, a block.

## Expressions

| Kind | Carries | Holds | Meaning |
|------|---------|-------|---------|
| `lit` | `value`: a JSON scalar or null | | itself |
| `obj` | `id`: an object id of `objects` | | the located object (Q5) |
| `ref` | `name` | | the binding of that name in scope (D8.8) |
| `get` | `path`; `members`: `required`, `optional` or `requiredIfHeld` | `target`: an expression | the one read (Q2): a required step the holder lacks stops the pipeline, an optional one reads nothing (D4.1), and a `requiredIfHeld` one reads nothing on a holder that reads nothing and stops on a holder that lacks it: an array's `length` and methods (D6.3, D6.4), and the members of the live item a find returns, which is nothing when no item matched (D6.9) |
| `data` | `path`: a list of `DataStep` | `target`: an expression | a typed read of data (D4.2): at each step a holder that holds null or reads nothing reads nothing; a holder that is not an object, or that lacks a name of the step's `requires`, stops the pipeline; a member the holder lacks reads the step's `absent`; a member it holds reads on, and a promise stops the pipeline (D4.3) |
| `as` | | `expr`: an expression; `shape`: a shape that converts | the one conversion by `spec/Core/conversions.md` (D3.2, D3.4); a value the shape cannot hold reads nothing (D3.3) |
| `call` | `path`; `members`: `required`, `optional` or `requiredIfHeld`, as `get` reads them | `target`, `args`: expressions | call the method at the path; a promise where a value is declared stops the pipeline (D4.3) |
| `fn` | `params`: names | `body`: an expression | a function whose parameters are bound in its body; as a value set or passed, the function a vendor calls, which answers its body's value (D17.12) |
| `record` | | `fields`: a list of `RecordField` | an object built member by member (D7.4) |
| `list` | | `items`: expressions | a list built from sources (N7.2) |
| `op` | `op`: an operator | `args`: expressions | the operator over its arguments; `and`, `or` and `cond` evaluate a term only when it is reached (D4.7); a comparison over an argument that reads nothing reads nothing, null compares as .NET's lifted operators compare it, and `not`, `and` and `or` are SQL's three-valued logic with nothing as its unknown (D8.9) |
| `signal` | nothing | | the run's abort signal, passed only to a method whose declaration names that argument (D17.11) |

`RecordField` carries `name`, a wire name, and `value`, an expression.

`DataStep` carries `name`, the member's wire name (S6); `requires`, the wire names the holder's type requires
(`[JsonRequired]`, `required`, a constructor parameter under `RespectRequiredConstructorParameters`); and `absent`, an
`Absence`, what the read is when the holder lacks the member (D4.2):

| Kind | Carries | Meaning |
|------|---------|---------|
| `start` | `value`: the JSON of a start | the read is this value: the rest of the path read on the binding of the path down to the holder, the member absent |
| `nothing` | nothing | the read is nothing: a holder on the rest of the path starts null |
| `unbound` | nothing | the pipeline stops: the binding cannot create a holder on the path, or a holder on it is polymorphic |

The operators are closed: `eq`, `neq`, `lt`, `lte`, `gt`, `gte`, `and`, `or`, `not`, `add`, `sub`, `mul`, `mod`,
`neg`, `coalesce`, `cond`, `nullableValue`, `truthy`, `empty`, `matches`, `toArray`, `finite`, `compareNumbers`, `compareOrdinal`, `compareBooleans`, `compareCulture`, `merge`, `trim`, `trimStart`,
`trimEnd`, `upperInvariant`, `lowerInvariant`, `upper`, `lower`, `upperTurkic`, `lowerTurkic`, `contains`, `startsWith`, `endsWith`, `nullOrEmpty`, `includes`, `sumInt`,
`sumDouble` and `sumDecimal`.
`toNumber`,
`toDate`, `toBoolean` and `toText` leave: `as` converts (D3.2, N3.1). The pattern of `matches` is a `lit` (D7.5,
D16.11). The text operators carry .NET's meaning (`spec/Core/translated-members.md`) where JavaScript's own methods
differ: `trim` removes another set of characters, `toUpperCase` turns `ß` into two letters, and `includes` reads an
argument that is nothing as the text "null". `includes` looks for an item in a list the view supplies, written as a
`lit`, and reads nothing when the item reads nothing, where a `call` of JavaScript's `includes` would pass null.
`sumInt`, `sumDouble` and `sumDecimal` take the list a `map` of the term reads and add it as .NET adds that number
type (D6.11): one operator over the whole list, where JavaScript's `reduce` would add a decimal as a double and
carry no range.
`compareNumbers`, `compareOrdinal`, `compareBooleans` and `compareCulture` compare two sort keys to -1, 0 or 1, null
first, by the comparison C# picks from the key's type (D6.13); the redesign's `order` compared whatever came by its
JavaScript type. `compareCulture` compares two texts with the browser's `Intl.Collator` under the plan's
`culture.collation`, and stops the pipeline when the browser has no collation for it. `matches` tests its text against
the pattern C# copied for the browser (D16.11), a `lit` with no flag, and reads nothing on a text that reads nothing
or holds null.
`coalesce` reads its second argument only when its first holds null or reads nothing; `cond` reads its second
argument when its first reads true and its third when it reads false or nothing; `nullableValue` reads its argument,
and nothing where it holds null (D6.14).

## Shapes

| Kind | Carries | Holds | Converts |
|------|---------|-------|----------|
| `string` | | | yes |
| `number` | `of`: `integer` or `fraction`; `text`: `view` or `invariant` | | yes: a text under the view's culture with the culture's number text for `of`, or under the invariant culture with `NumberStyles.Float` (D3.11) |
| `boolean` | | | yes |
| `date` | | | yes |
| `instant` | | | yes |
| `file` | | | yes: a file input's chosen file; sent only in form data (D9.16, N3.2) |
| `any` | | | no: `as` never carries it |
| `array` | | `item`: a shape | yes |
| `nullable` | | `inner`: a shape that is not `nullable` | yes |
| `object` | | `fields`: a map from wire name to shape | yes |

## Plugin arguments

These are typed contracts, not node kinds (G2). S2 and S8 hold for them as for the grammar: every part is required, a
part that can be absent is a closed union (`retry` is none or the standard policy, `body` is none, JSON or form data),
and a closed choice (`method`, `outcome`) is a sealed class. C# writes each as a `record` whose evaluated value is the contract;
the contract's C# classes are generated into the plan's TypeScript beside the grammar, so the browser declares
no argument type of its own (D9.1).

- `HttpSend`, the argument of `alis.http`'s `send` (N9.1):
  - `method`;
  - `url`, a page path or an absolute `http` or `https` URI, as path parts and the URL's own query (D9.13);
  - `query`;
  - `body`: none, JSON or form data, with the inputs of an element it sends (D9.6 to D9.10, D9.15);
  - `headers` (D9.11, D9.12);
  - `retry`: none, or the standard policy with its attempts and first wait (D10.14, N10.3);
  - `respondedJson`: true on net48 (D10.7);
  - the body kind of each route (D10.6).

  A request's trace context is its run's and is not in `HttpSend` (D20.3).
- `HttpResponse`, what `send` returns (N10.2): `ok`, `status`, `outcome` and `body`.
- `FormCheck`, the argument of `alis.form`'s `validate`: the validator, the rule set and the sent body with its
  format (D16.5, N16.2).

## Kinds

The kinds at the State SHA, 65fdf118d, and the proposed set. The today column cites each case line at
65fdf118d; the handler column names the one handler of the proposed set (S4). The diff is the rows whose
proposed column says `added`.

| Union | Kind | Today | Proposed | Handler | Why |
|-------|------|-------|----------|---------|-----|
| statement | set | `Alis.Reactive.Assets/runtime/grammar/execute.ts:25` | kept | the statement switch of `Alis.Reactive.Assets/runtime/grammar/execute.ts` | Q2 |
| statement | do | `Alis.Reactive.Assets/runtime/grammar/execute.ts:26` | kept | the statement switch of `Alis.Reactive.Assets/runtime/grammar/execute.ts` | Q4 |
| statement | let | `Alis.Reactive.Assets/runtime/grammar/execute.ts:27` | kept | the statement switch of `Alis.Reactive.Assets/runtime/grammar/execute.ts` | D8.8 |
| statement | if | `Alis.Reactive.Assets/runtime/grammar/execute.ts:28` | kept | the statement switch of `Alis.Reactive.Assets/runtime/grammar/execute.ts` | D8.6 |
| statement | all | `Alis.Reactive.Assets/runtime/grammar/execute.ts:29` | kept | the statement switch of `Alis.Reactive.Assets/runtime/grammar/execute.ts` | D10.10 |
| statement | try | `Alis.Reactive.Assets/runtime/grammar/execute.ts:30` | kept | the statement switch of `Alis.Reactive.Assets/runtime/grammar/execute.ts` | D11.8 |
| statement | stop | absent | added | the statement switch of `Alis.Reactive.Assets/runtime/grammar/execute.ts` | N4.1 |
| statement | listen | absent | added | the statement switch of `Alis.Reactive.Assets/runtime/grammar/execute.ts` | D12.8 |
| expression | lit | `Alis.Reactive.Assets/runtime/grammar/evaluate.ts:27` | kept | the expression switch of `Alis.Reactive.Assets/runtime/grammar/evaluate.ts` | Q7 |
| expression | obj | `Alis.Reactive.Assets/runtime/grammar/evaluate.ts:28` | kept | the expression switch of `Alis.Reactive.Assets/runtime/grammar/evaluate.ts` | Q5 |
| expression | ref | `Alis.Reactive.Assets/runtime/grammar/evaluate.ts:29` | kept | the expression switch of `Alis.Reactive.Assets/runtime/grammar/evaluate.ts` | D8.8 |
| expression | get | `Alis.Reactive.Assets/runtime/grammar/evaluate.ts:30` | kept | the expression switch of `Alis.Reactive.Assets/runtime/grammar/evaluate.ts` | Q2 |
| expression | as | `Alis.Reactive.Assets/runtime/grammar/evaluate.ts:31` | kept | the expression switch of `Alis.Reactive.Assets/runtime/grammar/evaluate.ts` | D3.2 |
| expression | call | `Alis.Reactive.Assets/runtime/grammar/evaluate.ts:32` | kept | the expression switch of `Alis.Reactive.Assets/runtime/grammar/evaluate.ts` | Q2 |
| expression | fn | `Alis.Reactive.Assets/runtime/grammar/evaluate.ts:33` | kept | the expression switch of `Alis.Reactive.Assets/runtime/grammar/evaluate.ts` | Q6 |
| expression | record | `Alis.Reactive.Assets/runtime/grammar/evaluate.ts:34` | kept | the expression switch of `Alis.Reactive.Assets/runtime/grammar/evaluate.ts` | D7.4 |
| expression | list | `Alis.Reactive.Assets/runtime/grammar/evaluate.ts:35` | kept | the expression switch of `Alis.Reactive.Assets/runtime/grammar/evaluate.ts` | N7.2 |
| expression | op | `Alis.Reactive.Assets/runtime/grammar/evaluate.ts:36` | kept | the expression switch of `Alis.Reactive.Assets/runtime/grammar/evaluate.ts` | Q8 |
| expression | data | absent | added | the expression switch of `Alis.Reactive.Assets/runtime/grammar/evaluate.ts` | D4.2 |
| absence | start | absent | added | the absence switch of the data read in `Alis.Reactive.Assets/runtime/grammar/evaluate.ts` | D4.2 |
| absence | nothing | absent | added | the absence switch of the data read in `Alis.Reactive.Assets/runtime/grammar/evaluate.ts` | D4.2 |
| absence | unbound | absent | added | the absence switch of the data read in `Alis.Reactive.Assets/runtime/grammar/evaluate.ts` | D4.2 |
| expression | signal | absent | added | the expression switch of `Alis.Reactive.Assets/runtime/grammar/evaluate.ts` | D17.11 |
| shape | string | `Alis.Reactive.Assets/runtime/grammar/shape.ts:17`, `Alis.Reactive.Assets/runtime/grammar/shape.ts:34` | kept | the conversion switch of `Alis.Reactive.Assets/runtime/grammar/shape.ts` | D3.6 |
| shape | number | `Alis.Reactive.Assets/runtime/grammar/shape.ts:18`, `Alis.Reactive.Assets/runtime/grammar/shape.ts:36` | kept | the conversion switch of `Alis.Reactive.Assets/runtime/grammar/shape.ts` | D3.11 |
| shape | boolean | `Alis.Reactive.Assets/runtime/grammar/shape.ts:19`, `Alis.Reactive.Assets/runtime/grammar/shape.ts:37` | kept | the conversion switch of `Alis.Reactive.Assets/runtime/grammar/shape.ts` | D3.6 |
| shape | date | `Alis.Reactive.Assets/runtime/grammar/shape.ts:20`, `Alis.Reactive.Assets/runtime/grammar/shape.ts:38` | kept | the conversion switch of `Alis.Reactive.Assets/runtime/grammar/shape.ts` | D3.6 |
| shape | instant | `Alis.Reactive.Assets/runtime/grammar/shape.ts:21`, `Alis.Reactive.Assets/runtime/grammar/shape.ts:39` | kept | the conversion switch of `Alis.Reactive.Assets/runtime/grammar/shape.ts` | D3.6 |
| shape | array | `Alis.Reactive.Assets/runtime/grammar/shape.ts:22`, `Alis.Reactive.Assets/runtime/grammar/shape.ts:40` | kept | the conversion switch of `Alis.Reactive.Assets/runtime/grammar/shape.ts` | D3.6 |
| shape | object | `Alis.Reactive.Assets/runtime/grammar/shape.ts:23`, `Alis.Reactive.Assets/runtime/grammar/shape.ts:41` | kept | the conversion switch of `Alis.Reactive.Assets/runtime/grammar/shape.ts` | D3.6 |
| shape | nullable | `Alis.Reactive.Assets/runtime/grammar/shape.ts:24`, `Alis.Reactive.Assets/runtime/grammar/shape.ts:42` | kept | the conversion switch of `Alis.Reactive.Assets/runtime/grammar/shape.ts` | D3.6 |
| shape | any | `Alis.Reactive.Assets/runtime/grammar/shape.ts:25`, `Alis.Reactive.Assets/runtime/grammar/shape.ts:35` | kept | the conversion switch of `Alis.Reactive.Assets/runtime/grammar/shape.ts` | D3.6 |
| shape | file | absent | added | the conversion switch of `Alis.Reactive.Assets/runtime/grammar/shape.ts` | N3.2 |
| path step | each | absent | added | the path walk of the form plugin, `Alis.Reactive.Assets/runtime/plugins/form/form.ts` | N1.1 |
| object locator | element | `Alis.Reactive.Assets/runtime/objects/plan-objects.ts:16`, `Alis.Reactive.Assets/runtime/objects/plan-objects.ts:29` | kept | the one locator switch of `Alis.Reactive.Assets/runtime/objects/plan-objects.ts` | G1 |
| object locator | plugin | `Alis.Reactive.Assets/runtime/objects/plan-objects.ts:15`, `Alis.Reactive.Assets/runtime/objects/plan-objects.ts:28` | kept | the one locator switch of `Alis.Reactive.Assets/runtime/objects/plan-objects.ts` | G1 |
| vendor listening | method | `Alis.Reactive.Assets/runtime/objects/vendor-driver.ts:30` | kept | the listening switch of `Alis.Reactive.Assets/runtime/objects/vendor-driver.ts` | Q17 |
| vendor listening | assignment | `Alis.Reactive.Assets/runtime/objects/vendor-driver.ts:31` | kept | the listening switch of `Alis.Reactive.Assets/runtime/objects/vendor-driver.ts` | Q17 |
| vendor listening | returned | absent | added | the listening switch of `Alis.Reactive.Assets/runtime/objects/vendor-driver.ts` | D17.3 |
| plugin data | none | `Alis.Reactive.Assets/runtime/plans/start.ts:38` | kept | none | S4 |
| plugin data | hub | `Alis.Reactive.Assets/runtime/plans/start.ts:39` | kept | the plugin the plan names, `alis.hub` | D12.6 |
| plugin data | model | `Alis.Reactive.Assets/runtime/plans/start.ts:39` | kept | the plugin the plan names, `alis.model` | D13.2 |
| plugin data | form | `Alis.Reactive.Assets/runtime/plans/start.ts:39` | kept | the plugin the plan names, `alis.form` | N16.2 |
| plugin data | stream | absent | added | the plugin the plan names, `alis.stream` | N21.3 |
| time zone | clock | absent | added | the zone switch of `Alis.Reactive.Assets/runtime/grammar/shape.ts` | D3.12 |
| time zone | zone | absent | added | the zone switch of `Alis.Reactive.Assets/runtime/grammar/shape.ts` | N3.5 |
| trace | off | absent | added | the trace switch of `Alis.Reactive.Assets/runtime/diagnostics/trace.ts` | D20.1 |
| trace | on | absent | added | the trace switch of `Alis.Reactive.Assets/runtime/diagnostics/trace.ts` | D20.1 |

Today 33 kinds, proposed 44: eleven added, none removed.

Today, each shape has two handlers: the conversion switch and the switch that picks a default by shape, `bound`
(`Alis.Reactive.Assets/runtime/grammar/shape.ts:31-45`). That second switch leaves, because the browser chooses
no default by shape: C# writes the blank into each input's value (D3.6, N3.3).

Each object locator has two switches, the object at `Alis.Reactive.Assets/runtime/objects/plan-objects.ts:14-18`
and the subscription at `:27-31`. They become one switch that returns the locator's driver, its object and its
subscription, so a locator kind is one handler (G1).

## Fields that change

The diff beyond the kinds, each with the decision that makes it:

- every `IReadOnlyList<string>` path, and `InputBinding.body`'s `BodyStep`, become path steps (N1.1);
- `Handler` gains `payload`, a path into the listener's arguments (D17.4), and `policy` (N11.1);
- `Vendor.objectPath` becomes `Vendor.locate`, a `fn` of the element (D17.3, N17.6);
- `fn` is a value wherever a value goes, set into a property or passed as an argument (D17.12);
- `Vendor.payload` leaves (D17.4);
- `InputBinding` gains `message` (D16.13), and `shape` and `list` leave into `value` (N3.3);
- `FormPluginData.fields` becomes `registrations`, and `FormField`'s `items` and `value` functions leave (D16.14);
- `PlanDocument` gains `culture` (N3.4), `timeZone` (N3.5) and `trace` (Q20), and `version` becomes 7;
- `number` gains `of` and `text` (D3.11);
- the operator set loses four conversions (N3.1).
