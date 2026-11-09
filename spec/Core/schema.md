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
  `{"kind":"each"}` is every item of a list. `each` appears only in a registration's paths.
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
| `clock` | nothing | the browser's clock and its offset (D3.12's default, confirmed at gate 1, C23) |
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
| `assignment` | nothing | listen through one function of the framework's per object and member, assigned to the member the event names at its first listener, which calls the member's previous value, then each listener once in the order it subscribed; the last release puts that value back while the member still holds the function (D17.3) |
| `returned` | `subscribe`: the object's method name | listen by calling it with the event name and the listener; release calls the function it returned (D17.3) |

## Object locators

| Kind | Carries | Holds | Meaning |
|------|---------|-------|---------|
| `element` | `id`: an element id; `vendor`: a vendor name of `vendors` | | the component on that element at the vendor's object path. An element the page lacks, or one that holds no object at that path, stops the pipeline that reads it (D4.1). |
| `plugin` | `name`: the plugin's name; `key`: text the plugin reads; `listen`: a vendor listening, how its events are listened to (D17.14) | `data`: plugin data | the plugin's object for that key. Its object id is `$plugin:` and its name for a plugin an app or a component brings, and for a core plugin that reads a key, `$`, the plugin's name after `alis.`, a colon and the key (`$slot:card`, as `Alis.Reactive/PlanModel/Writing/Scope.cs:234` writes it), so no element id meets it and each key is its own object. A name no script provides, or two scripts provide, stops the pipeline that reads it (D17.14). |

The plugins the core's C# names are `alis.form`, `alis.model`, `alis.slot`, `alis.hub`, `alis.stream` and
`alis.document`, each provided by the core's own browser code; a request is the statement `request`, not a plugin
(D10.23). Any other name is a plugin an app or a component
brings. `alis.confirm` of 65fdf118d is Fusion's, so the core no longer names it (D17.9). The core also names three
contracts a component provides and the core alone calls: `busy`, with `show(target)` and `hide(target)` (D10.21);
`notice`, with `show(text, kind)` (D10.21); and `reconnect`, with `show(retry)` and `hide()` (D21.11). Each is reached
as any plugin is: a contract the app's choices name and no script provides stops the request before it sends (D10.21),
or the connection before it opens (D21.11).

## Plugin data

| Kind | Carries | Holds | Meaning |
|------|---------|-------|---------|
| `none` | nothing | | the plugin contributes nothing to the owner: document, slot (its key is the container id, D12.1), an app's plugin |
| `hub` | `offer`: whether the core shows the reconnect offer, the app's startup choice | | the owner holds the hub's connection while it lives (D12.6, D21.11) |
| `stream` | `offer`: as a hub's | | the owner holds the stream's connection while it lives, as a hub's (D21.6, D21.11) |
| `model` | `inputs`: a map from input id to `InputBinding`; `containers`: a map from element id to `Container` | | the inputs and the request containers this plan renders, by id (D13.2, D14.3, D14.8) |
| `form` | | `registration`: `Registration` | this plan's rules of one validator under one rule set, its key the validator's key and the rule set's name, so no element id meets it (D16.2) |

`InputBinding` carries:
- `name`: its binding name, the name the server speaks (D1.7);
- `path`: its body path, with no `each`;
- `value`: an expression C# writes as `coalesce(as(read, shape), blank)`, where blank is the member's start (N3.3);
- `committed`: the event that commits its value;
- `invalidClass`: the class an invalid input takes;
- `message`: the id of its message element (D16.13).

Its shape and list flag of 65fdf118d leave into `value`.

`Container` carries:
- `model`: the key of the model its block was rendered for (D14.8);
- `kind`: `form` or `element`, the element its block rendered;
- `summary`: its summary element's id (D13.6);
- `live`: none, or the `Live` its inputs are checked by (D16.15).

`Live` carries `validator` and `ruleSet`, the check live validation runs (D16.15), and `body`, an expression
that reads the container's inputs as the container sends them (D16.5).

`Registration` is one validator's rules under one rule set, for every check by them in its plan (D16.2). It carries:
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
- The item and value functions leave. A field is a path with `each`, read over one checked body, and the form
  plugin walks it (D1.5, D16.14).
- `live` stays, as a container's `Live`, with the validator and rule set it runs.

## Statements

A block is a list of statements and a lexical scope (D8.8): a binding made in a block is read in that block
and in the blocks inside it, and nowhere else.

| Kind | Carries | Holds | Meaning |
|------|---------|-------|---------|
| `set` | `path`, at least one step; `members`: `required` or `optional`; `content`: a boolean, true for an element's text or HTML | `target`, `value`: expressions | write the value at the path below the target (Q2, Q4): a `required` member the holder lacks stops the pipeline, an `optional` one is written whether the holder has it or not, and a holder that holds nothing stops either way (D5.3); a `content` write stops when its element is, or holds, a slot a live plan names (D12.9, R14.7) |
| `do` | `await`: a boolean | `expr`: an expression | evaluate it; with `await`, wait for its promise (D4.3, D11.1) |
| `let` | `name`; `await`: a boolean; `binds`: `line` or `read` | `expr`: an expression | bind its value, or its awaited value, in the block (D8.8); at `line` the expression runs there, once, and every `ref` of the name reads the binding (D5.11); at `read` the expression runs where the name's one `ref` is read, in a term reached later: the right term of `and`, `or` or `coalesce`, or a branch of `cond` (D4.7) |
| `if` | | `when`: an expression; `then`, `else`: blocks | run `then` when `when` holds, else the `else` block; an `ElseIf` is an `else` block holding its condition's results and an `if` (D8.7) |
| `all` | | `blocks`: a list of blocks | run every block at once; the next statement runs after every block settled (D10.10) |
| `request` | `argument`, `reply`: the names its blocks read the argument and the reply under | `send`: an expression whose value is `HttpSend`; `check`: a block; `layers`: a list of layers, the defaults' first; `success`: a route; `statuses`: a list of status routes; `otherwise`: an otherwise route; `chained`: a block | run the request's states in order (D10.23): read `send` once and bind it as `argument` in `check`, run `check`, then each layer's `whileLoading`; check what the read recorded and the contracts `HttpSend` names, and hold its busy targets; send; run the one route the outcome picks, its reply bound as `reply`; run each layer's `finally` whose `whileLoading` completed, the last layer's first, then release; run `chained` after a success. Its later statements run after all of it |
| `listen` | `event`: the event's JavaScript name; `listen`: a vendor listening (a payload's object has no element for a vendor's `locate`); `release`: a listen release; `payload`: a path into the listener's arguments; `members`, as a handler's; `policy`; `payloadName`, `argumentsName`: the names the block binds them under | `object`: an expression; `do`: a block | listen to the event of the live object the expression reads, once per object and statement, for the owner of the plan; run the block on each raise, with the payload and the arguments bound under their names, in the scope of the block that holds the statement (D12.8) |
| `stop` | `reason`: text C# wrote | | stop the pipeline: run none of its later statements, log the reason; each `request` it is inside runs its layers' `finally` blocks whose `whileLoading` completed, the innermost request's first, and nothing else (D10.23, N4.1) |

A request's parts are closed (D10.23, S2):

| Part | Carries | Holds |
|------|---------|-------|
| layer | | `whileLoading`, `finally`: blocks |
| route | `kind`: the body kind it decodes, none, JSON, text, HTML or file for a success, none, JSON, text or HTML for an error (D10.6) | `do`: a block |
| status route | `status`: a number from 300 to 599 (D10.13); `kind`, as a route's | `do`: a block |
| otherwise route | `kind`: none, its reply a `Failure`, or JSON, its reply a `Failure<TError>` whose `Body` reads the body of a status outcome (N10.7) | `do`: a block |

A request that takes no defaults has one layer, its own. A route the view wrote no block for has kind none and an empty
`do`, so every part is present.

A listen release says when release removes the listener the step added (D12.8):

| Kind | Carries | Meaning |
|------|---------|---------|
| `living` | nothing | remove it while the object and the listener live |
| `held` | `vendor`: a vendor's name in `vendors`; `element`: a path from the object to its element | read the element when the step listens; remove the listener while the object and the listener live and that element still holds the object at the vendor's object path (D12.5) |

## Expressions

| Kind | Carries | Holds | Meaning |
|------|---------|-------|---------|
| `lit` | `value`: a JSON scalar or null | | itself |
| `obj` | `id`: an object id of `objects` | | the located object (Q5) |
| `ref` | `name` | | the binding of that name in scope (D8.8) |
| `get` | `path`; `members`: `required`, `optional`, `requiredIfHeld` or `optionalMember` | `target`: an expression | the one read (Q2): a required step the holder lacks stops the pipeline, an optional one reads nothing (D4.1), and a `requiredIfHeld` one reads nothing on a holder that reads nothing and stops on a holder that lacks it: an array's `length` and methods (D6.3, D6.4), the members of the live item a find returns, which is nothing when no item matched (D6.9), and the members of a live object declared optional or nullable (D5.3); an `optionalMember` one reads nothing on a holder that lacks it and stops on a holder that reads nothing: a member its declaration marks optional (D5.3) |
| `data` | `path`: a list of `DataStep` | `target`: an expression | a typed read of data (D4.2): at each step a holder that holds null or reads nothing reads nothing; a holder that is not an object, or that lacks a name of the step's `requires`, stops the pipeline; a member the holder lacks reads the step's `absent`; a member it holds reads on, and a promise stops the pipeline (D4.3) |
| `as` | | `expr`: an expression; `shape`: a shape that converts | the one conversion by `spec/Core/conversions.md` (D3.2, D3.4); a value the shape cannot hold reads nothing (D3.3) |
| `call` | `path`; `members`: `required`, `optional`, `requiredIfHeld` or `optionalMember`, as `get` reads them | `target`, `args`: expressions | call the method at the path; a promise where a value is declared stops the pipeline (D4.3). Under `optional` and `optionalMember` a method the holder holds as undefined is absent and runs nothing, as `?` types it (D5.3). An empty path calls the target itself, applied with `this` undefined, and a target that is no function stops the pipeline (D17.14) |
| `fn` | `params`: names | `body`: an expression | a function the framework calls, whose parameters are bound in its body: a list operation's item read, a sort's comparison, a vendor's locate |
| `answer` | `arguments`: a name | `body`: an expression | the function a vendor calls for an answer, set into a property or passed as an argument: each call binds the list of its arguments to the name and answers its body's value, and a stop its body raises is thrown to the vendor's call (D17.12) |
| `record` | | `fields`: a list of `RecordField` | an object built member by member (D7.4) |
| `list` | | `items`: expressions | a list built from sources (N7.2) |
| `op` | `op`: an operator | `args`: expressions | the operator over its arguments; `and`, `or` and `cond` evaluate a term only when it is reached (D4.7); a comparison over an argument that reads nothing reads nothing, null compares as .NET's lifted operators compare it, and `not`, `and` and `or` are SQL's three-valued logic with nothing as its unknown (D8.9) |
| `signal` | nothing | | the abort signal of the block it is read in (D11.12), passed only to a method whose declaration names that argument (D17.11) |

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
`neg`, `coalesce`, `cond`, `nullableValue`, `nonNull`, `empty`, `matches`, `toArray`, `finite`, `compareNumbers`, `compareOrdinal`, `compareBooleans`, `compareCulture`, `merge`, `trim`, `trimStart`,
`trimEnd`, `upperInvariant`, `lowerInvariant`, `upper`, `lower`, `upperTurkic`, `lowerTurkic`, `contains`, `startsWith`, `endsWith`, `nullOrEmpty`, `includes`, `sumInt`,
`sumDouble`, `sumDecimal`, `eqDecimal`, `dateKey`, `wallKey`, `instantKey`, `dayKey`, `timeKey` and `spanKey`.
`toNumber`,
`toDate`, `toBoolean` and `toText` leave: `as` converts (D3.2, N3.1). The pattern of `matches` is a `lit` (D7.5,
D16.11). The text operators carry .NET's meaning (`spec/Core/translated-members.md`) where JavaScript's own methods
differ: `trim` removes another set of characters, `toUpperCase` turns `ß` into two letters, and `includes` reads an
argument that is nothing as the text "null". `includes` looks for an item in a list the view supplies, written as a
`lit`, and reads nothing when the item reads nothing, where a `call` of JavaScript's `includes` would pass null.
`sumInt`, `sumDouble` and `sumDecimal` take the list a `map` of the term reads and add it as .NET adds that number
type (D6.11): one operator over the whole list, where JavaScript's `reduce` would add a decimal as a double and
carry no range. `sumDecimal` reads each term by the one decimal reader: a JSON number at its double's shortest text,
or a text of an optional `-`, digits and an optional `.` with one to 28 digits, whose digits without the point are at
most decimal.MaxValue's; any other term, and one past 28 places or those digits, stops the pipeline.
`eqDecimal` compares two decimals by value, as .NET's decimal equality does (D6.12): it reads each argument by the
same reader and compares the two as whole numbers at the larger scale, so "0.10" equals "0.1" where `eq` compares two
texts. An argument that reads nothing makes it read nothing, null equals null and no decimal, and any other value
stops the pipeline. C# writes it for `==` over a decimal written as text, and `not` of it for `!=`; a decimal written
as a JSON number keeps `eq` and `neq`.
`dateKey`, `wallKey`, `instantKey`, `dayKey`, `timeKey` and `spanKey` read a date or a time as `spec/Core/conversions.md`'s
Keys read it, to the tick: nothing reads nothing, null is null, a value in the key's forms is its key text, and any
other value stops the pipeline unless C# wrote a second argument, a `lit` of the member's start, which the key reads
in its place; C# writes that argument for a page URL member and an input (N10.7, N3.3). C# writes a key only as an
argument of `eq`, `neq`, `lt`, `lte`, `gt`, `gte` or `compareOrdinal` whose arguments are keys of one type (D6.12,
D6.13), so no value position takes one.
`toArray` is the array of the items its argument's `[Symbol.iterator]` yields, as `Array.from` reads them, where C#
reads a list an object's iterator yields (D6.17): an argument that reads nothing or holds null reads nothing, and one
with no iterator stops the pipeline; C# writes it over no other list.
`nonNull` is its one argument's value, and stops the pipeline where that argument reads nothing or holds null. C#
writes it for a `Select` field whose type holds no null, so the list a `Select` builds holds no value its item type
cannot hold (D6.16); a `Sum`'s term is checked by its own operator.
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

## Contracts

These are typed contracts, not node kinds (G2): a `request`'s `send` value and its reply, and a plugin's arguments. S2 and S8 hold for them as for the grammar: every part is required, a
part that can be absent is a closed union (`retry` is none or the standard policy, `body` is one of six formats),
and a closed choice (`method`, `outcome`) is a sealed class. C# writes each as a `record` whose evaluated value is the contract;
the contract's C# classes are generated into the plan's TypeScript beside the grammar, so the browser declares
no argument type of its own (D9.1).

- `HttpSend`, the value of a `request`'s `send` (D9.1, N9.1):
  - `method`: GET, HEAD, OPTIONS, POST, PUT or DELETE (D9.3);
  - `url`, a page path or an absolute `http` or `https` URI, as path parts and the URL's own query (D9.23);
  - `query`, which a GET's `Form`, `Inputs` and `Input`s fill (N10.7);
  - `body`: none, JSON, URL-encoded, multipart with its fields and file parts, text, or one file as the whole body,
    with the inputs it sends: a native form's submission set, a container's framework inputs, its `Input`s (D9.6 to
    D9.10, D9.15, D9.16, D9.21);
  - `headers`, the `Accept` its success route's kind takes among them (D9.11, D9.12, D10.6);
  - `antiforgery`: none, or the token C# wrote where the view rendered and its place, the header or the field, on a
    POST, PUT or DELETE to the page's origin (D9.22);
  - `retry`: none, or the standard policy with the attempts and first wait the defaults set, never a request's own
    (D10.14, N10.3);
  - `busy`: none, the page, or its targets as the C# derived them, each an element id or an expression that reads a
    component a payload holds, such as a row action's control, resolved when the request starts (D10.21, D17.15);
  - `respondedJson`: true on net48 (D10.7);
  - `contracts`: the contracts its defaults name, `busy` under `ShowBusy()` and `notice` for a default route's notice,
    which its `guard` state looks up before the send (D10.21, D10.23).

  The body kind each route decodes is its route's, not `HttpSend`'s (D10.23).

  A request's trace context is its run's and is not in `HttpSend` (D20.3).

  Each member `HttpSend` sends in its path, query, headers or form data carries its wire form, how the browser writes
  the value it holds as text (D9.6, D9.10, D9.13, D9.23). C# writes it as a `record`. The forms are closed:

  | Kind | Carries | Meaning |
  |------|---------|---------|
  | `text` | nothing | a text, sent as it is |
  | `number` | nothing | a finite number, sent as its invariant text (D9.6) |
  | `boolean` | nothing | `true` or `false` |
  | `date` | nothing | a date, as `spec/Core/DateWire.tla` writes it with no offset (D3.8) |
  | `instant` | nothing | an instant, as `spec/Core/DateWire.tla` writes it with the clock's offset (D3.8) |
  | `written` | nothing | a value System.Text.Json writes as one text or number, sent as it is held |
  | `enum` | `values`: a list of `EnumValue` | the `name` of the value whose `held` equals what the source holds; a held form no value lists stops the request (D9.13) |
  | `flags` | `values`: a list of `FlagValue`, the largest `number` first | the names of the values `Enum.ToString`'s split takes from what the source holds, the smallest first, joined by ", "; what the split cannot read stops the request (D9.13, N16.4) |
  | `list` | `item`: a scalar form; `itemWhenNothing`: `empty` or `stop` | one field per item, each in the item's form; an item that reads nothing is an empty field or stops the request (D9.10) |

  A scalar form is every form but `list`. `EnumValue` carries `held`, the JSON text, number or boolean System.Text.Json
  writes for one value of the enum, at the source's type under the source's held form (D4.2), and `name`, that value's
  C# name. A value written as anything else, and two values the browser reads as one `held`, are refused where the view
  renders, so every `held` is a `lit` of a scalar. A value that holds nothing, or null, reads nothing, and the member is
  sent by D9.10's `whenNothing`: `leave` or `empty`. `FlagValue` carries `held` and `name` as `EnumValue` does, and
  `number`, the value's number, an integer from 0 to 9,007,199,254,740,991 (N16.5); `flags` is written only for an
  enum the app names as flags (N16.4).
- `HttpResponse`, the reply a `request` binds in its routes and `chained` block (N10.2, N10.7, D10.23): `status`,
  `outcome`, `headers`, the headers of the answer the browser hands the page, lib.dom's `Headers`, whose `get` a `call`
  reads (N10.7), `request`, the members as the request sent them, `inputs`, the inputs it sent, each its binding name
  and the entry the inputs table held (D10.18), and `body`, absent for kind none and every HEAD route.
- `FormCheck`, the argument of `alis.form`'s `validate`: the registration's key, the body the request sends with its
  format and the inputs it read (D10.18), and the request's `FormPlace` (D16.5, N16.2). A check with no request reads
  its container's inputs as the body (N16.3).
- `FormPlace`, where a request's failures show (D16.13): the summary of its container, by the container's id, or the
  notice with the separator that joins the messages, a space. `alis.form`'s `showErrors` takes a `ValidationProblem`,
  the response's `inputs` and the `FormPlace`.

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
| statement | try | `Alis.Reactive.Assets/runtime/grammar/execute.ts:30` | removed | | D10.23 |
| statement | stop | absent | added | the statement switch of `Alis.Reactive.Assets/runtime/grammar/execute.ts` | N4.1 |
| statement | listen | absent | added | the statement switch of `Alis.Reactive.Assets/runtime/grammar/execute.ts` | D12.8 |
| statement | request | absent | added | the statement switch of `Alis.Reactive.Assets/runtime/grammar/execute.ts` | D10.23 |
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
| expression | answer | absent | added | the expression switch of `Alis.Reactive.Assets/runtime/grammar/evaluate.ts` | D17.12 |
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
| listen release | living | absent | added | the release switch of `Alis.Reactive.Assets/runtime/objects/vendor-driver.ts` | D12.8 |
| listen release | held | absent | added | the release switch of `Alis.Reactive.Assets/runtime/objects/vendor-driver.ts` | D12.8 |
| plugin data | none | `Alis.Reactive.Assets/runtime/plans/start.ts:38` | kept | none | S4 |
| plugin data | hub | `Alis.Reactive.Assets/runtime/plans/start.ts:39` | kept | the plugin the plan names, `alis.hub` | D12.6 |
| plugin data | model | `Alis.Reactive.Assets/runtime/plans/start.ts:39` | kept | the plugin the plan names, `alis.model` | D13.2 |
| plugin data | form | `Alis.Reactive.Assets/runtime/plans/start.ts:39` | kept | the plugin the plan names, `alis.form` | N16.2 |
| plugin data | stream | absent | added | the plugin the plan names, `alis.stream` | N21.3 |
| time zone | clock | absent | added | the zone switch of `Alis.Reactive.Assets/runtime/grammar/shape.ts` | D3.12 |
| time zone | zone | absent | added | the zone switch of `Alis.Reactive.Assets/runtime/grammar/shape.ts` | N3.5 |
| trace | off | absent | added | the trace switch of `Alis.Reactive.Assets/runtime/diagnostics/trace.ts` | D20.1 |
| trace | on | absent | added | the trace switch of `Alis.Reactive.Assets/runtime/diagnostics/trace.ts` | D20.1 |

Today 33 kinds, proposed 51: nineteen added, one removed (`try`, whose `finally` a `request`'s layers carry).

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
- `answer` is the function a vendor calls for an answer, a value wherever a value goes, set into a property or
  passed as an argument (D17.12);
- `Vendor.payload` leaves (D17.4);
- `InputBinding` gains `message` (D16.13), and `shape` and `list` leave into `value` (N3.3);
- `FormPluginData` becomes one `Registration`, keyed by validator and rule set and by no element; its `summary` and
  `live` move to `model`'s `containers` as `Container`; and `FormField`'s `items` and `value` functions leave (D16.2,
  D14.8, D16.14);
- `PlanDocument` gains `culture` (N3.4), `timeZone` (N3.5) and `trace` (Q20), and `version` becomes 7;
- `number` gains `of` and `text` (D3.11);
- the operator set loses four conversions (N3.1), `truthy`, whose writers `Truthy` and `Falsy` are refused (M3.D1), and
  `order`, which compared whatever came (D6.13); it gains `nullableValue` (D6.14), `nonNull` (D6.16), `compareNumbers`,
  `compareOrdinal`, `compareBooleans` and `compareCulture` (D6.13), the text operators `trim`, `trimStart`, `trimEnd`,
  `upperInvariant`, `lowerInvariant`, `upper`, `lower`, `upperTurkic`, `lowerTurkic`, `contains`, `startsWith`,
  `endsWith` and `nullOrEmpty` (D6.5), `includes` (D6.6), `sumInt`, `sumDouble` and `sumDecimal` (D6.11), `eqDecimal`
  (D6.12) and the six date keys `dateKey`, `wallKey`, `instantKey`, `dayKey`, `timeKey` and `spanKey` (D6.12);
- `let` gains `binds`, `line` or `read`, so a term's own method call runs where its read is reached (D4.7);
- `if` holds one `when` and its `then` in place of `arms`, and an `ElseIf` nests in `else` with its condition's results
  (D8.7), so `Arm` leaves;
- `try` leaves and `request` joins the statements (D10.23): `HttpSend` becomes a `request`'s `send` value, the body
  kind each route decodes moves from `HttpSend` to the route, and `HttpSend` gains `contracts` (D10.21);
- `HttpSend`'s methods become six and its bodies six formats, and it gains `antiforgery` and `busy`; its `retry`
  takes the defaults' numbers only (D9.3, D9.21, D9.22, D10.14, D10.21);
- `HttpResponse` gains `headers`, `request` and `inputs` (N10.7, D10.18), and `FormPlace` joins `FormCheck` (D16.13);
- `HttpSend`'s wire forms gain `enum` and `flags`, and a list's `item` is a scalar form in place of a kind's text (D9.13,
  N16.4);
- plugin data `hub` and `stream` gain `offer` (D21.11).
