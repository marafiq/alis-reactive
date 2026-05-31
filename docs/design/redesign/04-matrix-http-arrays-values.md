# Determinism Matrix — HTTP, Arrays, Values

> Source-grounded determinism matrix for the **fresh** redesign. Modules and
> names are from [`02-micro-modules.md`](./02-micro-modules.md) and
> [`03-naming.md`](./03-naming.md). Every row's INPUT, plan-JSON OUTPUT, and
> runtime behavior were read from the actual DSL/plan/runtime source under
> `Alis.Reactive/` and `Alis.Reactive.Assets/runtime/`, not inferred from docs.

## How to read this matrix

The framework's whole reason to exist is that a **deterministic DSL input**,
walked through a fixed sequence of micro-modules, produces **exactly one** plan
JSON and **exactly one** browser behavior. If that is true, generating code for a
case is mechanical: pick the case, fill the parameters, emit. This document
proves it case-by-case.

Each row has five columns:

- **Feature / variant** — the public DSL feature and the specific variant.
- **Input** — the C# a developer writes (frozen DSL surface, Layer 1).
- **Module path** — the ordered redesign micro-modules the data flows through,
  and what each does to it. `→` = C# authoring/plan side, `⇒` = TS runtime side.
- **Output** — the exact plan-JSON node shape (camelCase, as `PlanSerializer`
  emits) plus the browser behavior the runtime must produce.
- **Good default** — the value/behavior chosen when the developer says nothing.

### The three fixed module spines every row in this band reuses

These appear so often they are stated once here and referenced by name:

- **VALUE-SPINE** = `TypedSource<T>` *(Value →)* lowers to one `ValueExpression`
  node; the runtime reads it back with the one `evaluateValue` *(Value ⇒)*
  dispatcher. `Shape` rides on the node (kernel). `Kind` is the discriminator
  (kernel). **One write path, one read path.** This is the parameter that makes
  thousands of value cases collapse to a handful of node shapes.
- **SHAPE-ONCE** = a value is shaped exactly once, on the gather egress path,
  by `ShapeConverter` *(Shape ⇒, `formatForWire`)*. Authoring infers `Shape`
  from the CLR type via `Shape.FromClrType`; the runtime converts to wire form
  once. No re-derivation per stage.
- **REACTION-LANE** = `ReactionPipelineDraft` *(Reaction →)* stamps each node's
  lane; `executeReaction` *(Reaction ⇒)* routes on the carried lane. **Request is
  the only feature in this band that opens the async lane.** Arrays and Values
  are pure sync reads inside `evaluateValue`.

### Why this scales to thousands of generated cases

Almost every case below is **parameterized over the value-source kind** and/or
**over the CLR type → Shape**. The matrix lists the *generators* (e.g. "any of
the 6 source kinds", "any scalar/array/object Shape"), so one row stands for the
full Cartesian product. A code generator iterates the parameter axes and emits
each combination from the single node template named in the Output column.

---

## Part A — Values / ValueExpression (every source kind)

**Module owner:** Value (depends on Shape, Kind). Pure sync read — no Request
lane. Authoring: `TypedSource<T>` → one `ValueExpression` variant. Runtime:
`evaluateValue` (the slim dispatcher) + `ArrayOpEngine` for array-op.

`ValueExpression` is a **flat five-variant family** carrying `Shape`:
`Literal` · `Read` · `ObjectValue` · `ArrayValue` · `ArrayOp`. A `Read` carries a
`Source` (one of six) and a `ValueReadAccess` (`property` | `method`).

> **Parameter axis P-SOURCE** (the six `Source` kinds, `Source.cs`):
> `component` · `plugin` · `payload` (event/success/error/request/dispatch/element)
> · `url` · `dom`. Every `Read` row below is one value of this axis.
> **Parameter axis P-SHAPE** (`Shape.cs`): `string · number · boolean · date ·
> nullable<scalar> · array<item> · object{fields} · raw · any · none`.

### A.1 — Literals (`LiteralExpression`)

| Feature / variant | Input (DSL) | Module path | Output (plan JSON + browser behavior) | Good default |
|---|---|---|---|---|
| **Literal — scalar** (string/int/long/decimal/double/bool/DateTime) — *parameterized over CLR scalar type* | A literal passed to any sink, e.g. `.SetText("hello")`, `.Eq(5)`, `.Static("k", 12.5m)` | Value → `ValueExpression.Literal(value)` infers `Shape` from the overload (`Shape.String/Number/Boolean/Date`); DateTime → ISO `"O"` string + `Shape.Date`. ⇒ `evaluateValue` case `"literal"` → `applyShape(value, shape)` (SHAPE-ONCE on read) | `{ "kind":"literal", "value":<v>, "shape":{"kind":"string\|number\|boolean\|date"} }`. Browser: yields the constant, shape-coerced. | DateTime serialized round-trip-safe (ISO-8601 `"O"`). |
| **Literal — null** | A null reaches `LiteralFromValue(null)` (e.g. `Static("k", null)`) | Value → `ValueExpression.Null()` → `LiteralExpression(null, Shape.None)` | `{ "kind":"literal", "value":null, "shape":{"kind":"none"} }`. Browser: yields `null`; on JSON-body egress `""`→`null` is *not* applied (already null). | `Shape.None` (absence, not a typed default). |
| **Literal — arbitrary value** (boxed `object` — enum, Guid, date, collection, …) | `ArgValue(value)` / `Static(k, value)` | Value → `LiteralFromValue(value)` → `Shape.FromValue(value)` → `null`→`None` (`Shape.cs:96-97`), else `FromClrType(value.GetType())` (`Shape.cs:70-89`). Full dispatch, matching the typed scalar row: `Nullable<T>`→`Nullable(inner)` (`Shape.cs:74-76`); `string`→`String` (`:78`); `bool`→`Boolean` (`:79`); `DateTime`/`DateTimeOffset`/`DateOnly`→`Date` (`:80`, `IsDateType` `:99-104`); numeric (byte…decimal)→`Number` (`:81`); `Guid`/`TimeSpan`/`TimeOnly`→`String` (`:82`, `IsStringSerializedType` `:113-118`); enum→`String` (`:83`); supported collection→`ArrayOf(item)` (`:85-86`); else→`Any` (`:88`) | `{ "kind":"literal", "value":<json>, "shape":<inferred> }`. STJ serializes the value at Render. | `any` only when the type is unclassifiable — never a guessed scalar; `none` for a null literal. |

### A.2 — Reads (`ReadExpression`) — parameterized over P-SOURCE

A `Read` is ONE node shape; the `from` discriminant and `access` select behavior.
This single template, ×6 sources ×2 accesses, covers the entire read surface.

| Feature / variant | Input (DSL) | Module path | Output (plan JSON + browser behavior) | Good default |
|---|---|---|---|---|
| **Read — component property** | `p.Component<T>("id").Value()` / typed slice read → `TypedComponentSource<TProp>` | VALUE-SPINE: Value → `ValueExpression.Read(ComponentSource.Of(id), member, shape=FromClrType(TProp))`, `access=property`. ⇒ `evaluateValue` `"read"` → `isObjectRead` → `RuntimeObject.read(member)` via `ComponentDriver` (the sole vendor seam) → `usingRequestedShape(shape)` | `{ "kind":"read", "from":{"kind":"component","component":"<id>"}, "member":"<m>", "path":[], "shape":<TProp>, "access":{"kind":"property"} }`. Browser: reads the live component member by `getElementById` + vendor driver. | `shape` from `TProp`; `path` empty (direct member). |
| **Read — component method** | typed slice method source (e.g. `schedule.GetEvents()`) → `TypedComponentSource.FromMethod` | Value → `ValueExpression.Invoke(ComponentSource, method, returns, args)`, `access=method(args)`. Each arg is itself a `ValueExpression` (recursion). ⇒ `RuntimeObject.call(member, args.map(evaluate))` | `…"access":{"kind":"method","args":[<ValueExpression>…]}`. Browser: `fn.apply(object, args)`. | `args:[]` when no args; return `shape` from `TReturn`. |
| **Read — plugin method** | `p.Plugin<T>(name, member).Arg(...)` → `TypedPluginSource<T>` (implicit) | Value → `Invoke(PluginSource.Of(name), method, shape, args)`. Plugin declared in `PluginContract` (Plugin module) so the contract carries the member. ⇒ `objectForSource` → `PluginCatalog` resolves the host instance (throws at the real boundary if unknown) → `.call` | `{ "kind":"read","from":{"kind":"plugin","name":"<n>","type":"plugin.<n>"},"member":"<m>","access":{"kind":"method","args":[…]},"shape":<T> }`. Browser: invoke registered plugin method. | `args:[]`; unknown plugin → boundary throw (not a fallback). |
| **Read — plugin property** | `p.PluginProperty<T>(name, member)` → `TypedPluginPropertySource<T>` | Value → `Read(PluginSource.Of(name), member, shape)`, `access=property` | `…"from":{"kind":"plugin",…},"access":{"kind":"property"}`. Browser: read plugin property. | shape from `T`. |
| **Read — URL query param (untyped)** | `p.FromUrl("page")` → `TypedUrlSource<string>` | Value → `ValueExpression.ReadUrl(name)` = `Read(UrlSource.Instance, name, Shape.String)`. ⇒ `readFromUrl` → `URLSearchParams.get(name)` → `applyShapeWhenPresent(raw, String)` | `{ "kind":"read","from":{"kind":"url"},"member":"page","path":[],"shape":{"kind":"string"},"access":{"kind":"property"} }`. Browser: read current location query string. | `Shape.String` — URL params are inherently strings. |
| **Read — URL query param (typed)** | `p.FromUrl<int>("page")` | Value → `RequestScalarTarget.UrlQueryParameter<int>` enforces scalar at build; `ReadUrl(name, Shape.Number)`. ⇒ same as above, `applyShape` coerces `"3"`→`3` | `…"shape":{"kind":"number"}`. Browser: read + coerce to declared scalar. | scalar-only (non-scalar `T` rejected at authoring). |
| **Read — payload (event/success/error/request/dispatch)** — *parameterized over PayloadScope* | `.OnSuccess<R>((json,s)=> … json.Read(r=>r.Data.Name))`; `FromEvent` overloads; `body.Read(...)` | VALUE-SPINE: Value → `ReadPayload(PayloadSource.<scope>(contract), path, shape)`; the dotted member is parsed into `Path`. ⇒ `evaluateValue` `isPayloadRead` → `readFromPayload(expr, ctx.resolvePayload(scope))` → `RuntimePath.read(root)` | `{ "kind":"read","from":{"kind":"payload","scope":"success","type":{…}},"member":"data.name","path":[{"kind":"property","name":"data"},{"kind":"property","name":"name"}],"shape":<TProp>,"access":{"kind":"property"} }`. Browser: walk path on the scope's payload object. | path parsed from the member; scope from where the source was created. |
| **Read — WHOLE payload** | response body bound directly (whole-body read), e.g. `Into(elementId)` → `ReadWholePayload(Success)` | Value → `ReadWholePayload(scope)` → a **distinct `WholePayload` node KIND** (redesign). **current: encoded as the magic member string `member:"responseBody"`, `path` forced to `Path.None` (`ValueExpression.cs:379,399-400`), and the runtime discriminates on `expression.member === "responseBody"` ignoring `path` (`evaluate.ts:287,294-296`) → redesign: a distinct node kind (deterministic, because the read intent rides on the node KIND, not a reserved member string, so it can never collide with a member name).** | Redesign output: `{ "kind":"whole-payload","from":{"kind":"payload",…} }` → the entire scope object, returned unwalked. Browser: returns the root payload. | whole-body is an explicit node kind, never a magic member name. |
| **Read — WHOLE element** | identity `x => x` inside an array op over primitives | Value → `ReadWholeElement()` → a **distinct `WholeElement` node KIND** (redesign). **current: magic member `member:"elementValue"`, discriminated on `expression.member === "elementValue"` (`ValueExpression.cs:380,402-403`; `evaluate.ts:298-300`) → redesign: distinct node kind (deterministic, same reasoning as whole-payload).** | `{ "kind":"whole-element","from":{"kind":"payload","scope":"element"} }`. Browser: returns the current element itself. | element-identity is an explicit node kind. |
| **Read — DOM member** | `p.FromDom("card","classList")` (array entry) / dom member read | Value → `ValueExpression.ReadDom(id, member, shape)` = `Read(DomSource.Of(id), member, Path.Parse(member), shape)`. ⇒ `readFromDom` → `getElementById(id)` (boundary throw if null) → `RuntimePath.read(element)` | `{ "kind":"read","from":{"kind":"dom","element":"card"},"member":"classList","path":[{"kind":"property","name":"classList"}],"shape":<>,"access":{"kind":"property"} }`. Browser: read member off the DOM element. | member name is stringly at the DOM boundary (like plugin names); id plan-carried (Rule 7). |

### A.3 — Composite values

| Feature / variant | Input (DSL) | Module path | Output (plan JSON + browser behavior) | Good default |
|---|---|---|---|---|
| **Object value** | `p.DispatchWith<P>(name, b => b.Set(x=>x.A, src).Set(x=>x.B, "lit"))` | Value → `ValueExpression.Object(fields)`; dotted field paths build a nested object tree (`DispatchPayloadDraft`); object `Shape` is `ObjectOf(field→shape)` derived from each field's `OutputShape` | `{ "kind":"object","fields":{"a":<ValueExpr>,"b":{"kind":"literal",…}},"shape":{"kind":"object","fields":{…},"additional":false} }`. Browser: each field evaluated, assembled into a JS object. | closed object shape; field name conflict (leaf vs parent) → authoring throw. |
| **Array value (literal items)** | items composed into an array value | Value → `ValueExpression.Array(items)`; if all items share a shape → `array<itemShape>`, else `array<any>` | `{ "kind":"array","items":[<ValueExpr>…],"shape":{"kind":"array","item":{…}} }`. Browser: items evaluated in order into a JS array. | shared item shape when homogeneous, `array<any>` when mixed/empty. |

> **Determinism note (Values) — the sentinel FIX is the headline determinism win
> of this band.** Every readable value funnels through exactly one `ValueExpression`
> variant and is read back by exactly one `evaluateValue` case. The redesign closes
> the two representable-but-invalid holes:
>
> 1. **`responseBody`/`elementValue` magic-member sentinels → distinct node KINDS.**
>    current: whole-payload and whole-element are encoded as the reserved member
>    strings `member:"responseBody"` / `member:"elementValue"` with `path` forced to
>    `Path.None` (`ValueExpression.cs:379-380,399-403`), and the runtime
>    discriminates **only** on `expression.member === "responseBody"` /
>    `"elementValue"`, ignoring `path` (`evaluate.ts:287,294-300`). This is a
>    many-to-one input collision: a legal public-DSL read of a response/event/element
>    property **literally named `ResponseBody`** camelCases to exactly `responseBody`
>    and the runtime returns the whole object instead of the `.ResponseBody`
>    sub-field. → redesign: `WholePayload` and `WholeElement` are **separate
>    `ValueExpression` node kinds** (`kind:"whole-payload"` / `kind:"whole-element"`),
>    NOT `member="responseBody"` / `member="elementValue"`. Therefore a DSL property
>    literally named `ResponseBody` lowers to an **ordinary `Read` with
>    `member:"responseBody"`** that walks the path normally and is **distinct from a
>    whole-payload read** (deterministic, because read intent rides on the node kind,
>    not a reserved member string — the current collision at `ValueExpression.cs:379`
>    and `evaluate.ts:287` is resolved; a C# generation typo can no longer silently
>    change read semantics).
> 2. **The gather-source hole closes** — a read in a gather flows the same
>    `TypedSource` path as every other value (see B.3 and Arrays C.Terminal).

---

## Part B — HTTP Pipeline (verbs, gather, request input, response routes, chained, parallel, loading)

**Module owner:** Request (depends on Value, Condition, Component, Shape, Kind).
**This is the only async lane in the band.** Authoring: `HttpRequestBuilder` →
`RequestPlan`. Runtime: `http` pipeline (`gather → httpFetch → response routing →
finally → chain`), all `await`ed, REACTION-LANE = async.

### B.1 — Verbs (`Get` / `Post` / `Put` / `Delete`)

> **Parameter axis P-VERB** = `GET · POST · PUT · DELETE`. The verb selects the
> egress: GET → query string, others → JSON/form-data body (`request-payload-writer.ts`).

| Feature / variant | Input (DSL) | Module path | Output (plan JSON + browser behavior) | Good default |
|---|---|---|---|---|
| **GET** | `p.Get("/api/x")` | Reaction → `ReactionPipelineDraft.BeginHttp` (stamps async lane) → Request → `HttpRequestBuilder.Get` → `RequestEndpoint.To(Get, url)` → `RequestPlan.Create`. ⇒ `http.executeRequest` → `requestPayloadWriterFor` chooses **query-string** writer (`sendsInputInQueryString(GET)=true`) | `{ "kind":"request"(reaction) … "method":"GET","url":"/api/x", … }`. Browser: `fetch(url + ?query, {method:"GET"})`, no body. | body format irrelevant for GET (always query string). |
| **POST / PUT / DELETE** — *parameterized over P-VERB ≠ GET* | `p.Post("/api/x")` etc. | same path, method token differs. ⇒ writer chooses **body** writer | `…"method":"POST"…`. Browser: `fetch(url, {method, body:<json\|formdata>})`; JSON body sets `Content-Type: application/json`. | body sent only when it has fields (`http-fetch.ts` `bodyHasFields`). |
| **Bare PUT** | `p.Put("/api/x")` | **Current** (`PipelineBuilder.Http.cs:11-42`): there is NO bare `Put(url)` on `PipelineBuilder` — the pipeline entries are `Get`/`Post`/`Post(url,gather)`/`Put(url,gather)`/`Delete`; a bare PUT is reachable only as the endpoint selector `HttpRequestBuilder.Put(url)` (`HttpRequestBuilder.cs:42`). **→ Redesign**: a bare `p.Put(url)` pipeline entry exists for verb symmetry (deterministic, because the 4 verbs become one uniform 2-form set — bare + inline-gather — so the generator emits the same two templates per body verb). Path otherwise identical to POST, method token `PUT`. | `…"method":"PUT"…`. Browser: `fetch(url, {method:"PUT", body:<json\|formdata>})`. | restores the GET/POST/PUT/DELETE bare-verb symmetry the matrix's B.1 framing assumes. |
| **Inline gather overload — POST / PUT** | `p.Post(url, g => g.Include(...))` (`PipelineBuilder.Http.cs:25`) / `p.Put(url, g => g.Include(...))` (`PipelineBuilder.Http.cs:31`) | `PipelineBuilder.Post(url, gather)` = `.Post(url).Gather(gather)`; `PipelineBuilder.Put(url, gather)` = `.Put(url).Gather(gather)` — pure sugar over verb + `.Gather(...)`. **Current**: inline-gather sugar exists for POST and PUT only (`PipelineBuilder.Http.cs:25,31`). **→ Redesign**: extends the inline-gather form to every body verb, pairing with the bare-verb entries above. | identical to the bare verb + a gather input node | inline-gather is the second of the two uniform per-verb templates (bare + inline-gather). |

### B.2 — Endpoint + URL template

| Feature / variant | Input (DSL) | Module path | Output (plan JSON + browser behavior) | Good default |
|---|---|---|---|---|
| **URL with `{placeholder}`** | `p.Get("/residents/{id}")` + a route param | Request → `RequestRouteTemplate.For(url).RequireRouteParameters(...)` validates every `{x}` has a matching route-param assignment at **authoring time**. ⇒ `resolveRouteParams(url, routeParams)` substitutes | URL string carried verbatim in `"url"`. Browser: `{id}` replaced by the resolved route-param value before fetch. | every `{placeholder}` must be supplied (authoring error if not) — no silent blank substitution. |

### B.3 — Gather (`target <- value`) — request input assignments

> **Parameter axis P-TARGET** = `payload · header · route-param · url-query`. Each
> reads a value through VALUE-SPINE and writes to one target. **One read path** —
> the redesign closes the gather-source hole so `ReactiveValue`/`ReactiveArray`
> reach gather like every other `TypedSource`.

| Feature / variant | Input (DSL) | Module path | Output (plan JSON + browser behavior) | Good default |
|---|---|---|---|---|
| **Gather payload — typed component (by expression)** | `g.Include<TComp,TModel>(m => m.Name)` | Request `GatherBuilder.Include` → `IdGenerator.For` (Component module, the one id regime) → `RequestInputAssignment.Payload(BindingPath(prop), Read(ComponentSource, valueMember, shape))`; declares the input on `PlanBuildContext`. ⇒ `gather.resolveGatherRequestInput` → `evaluateValue(source)` → writer | `{ "kind":"gather","assignments":[{"target":{"kind":"payload","name":"name","path":[…]},"source":{"kind":"read",…}}],"bodyFormat":"json","registeredInputs":{"kind":"explicit"} }`. Browser: read component → write into body/query by verb. | param name = property name; component value member read. |
| **Gather payload — typed component member source** | `g.Include(schedule.CurrentView())` / `…, "as")` | same, source = `TypedComponentSource` (property or method); default param name = the read member (`DefaultPayloadName`) | one payload assignment per source | param = member name unless overridden. |
| **Gather payload — by-ref display OR input component** | `g.Include<TComp,TModel>(refId, name)` (`GatherExtensions.cs:36`) | Request `GatherExtensions.Include(refId, name)`: reads the named property; when `TComp is IInputComponent` the read member is the input's `ValueMember`, otherwise it is the named property `name` itself (display component) → `self.Include(refId, component.Vendor, name, valueMember)`. ⇒ same gather egress | one payload assignment keyed by `name`; for a display component the runtime reads the **named property** (not a value member). | works for BOTH input and display components: input → `ValueMember`, display → the named property (`GatherExtensions.cs:45-48`). Param key = `name`. |
| **Gather payload — static literal** | `g.Static("token", "abc")` | `GatherBuilder.Static` → `AddPayload(BindingPath(param), LiteralFromValue(value))` | `…"source":{"kind":"literal",…}`. Browser: constant written to target. | — |
| **Gather payload — from event arg** | `g.FromEvent(args, e => e.Id, "id")` | `ReadPayload(PayloadSource.Event(), eventPath, Shape.FromClrType(TProp))` | `…"source":{"kind":"read","from":{"kind":"payload","scope":"event"},…}`. Browser: read trigger payload → write. | shape from the event-arg property type. |
| **Gather payload — from URL query** | `g.FromUrl("page")` / `g.FromUrl<int>("page","p")` / `(…, asParam)` | `RequestInputAssignment.Payload(BindingPath(asParam??param), ReadUrl(param[, shape]))` | payload assignment whose source is a `url` read | param name doubles as payload key unless `asParam` given; default `Shape.String`, typed overload coerces. |
| **Gather payload — plugin read** | `g.Plugin(pluginSource, "name")` | `AddAssignment(Payload(BindingPath(name), source.ToValueExpression()))` | payload assignment, source = plugin `Invoke` read | — |
| **Gather header — literal** | `g.Header("X-Key", "v")` | Request → `RequestInputAssignment.Header(HeaderName, Literal(v))`; null rejected at authoring | `{"target":{"kind":"header","name":"X-Key"},"source":{"kind":"literal",…}}`. ⇒ `writeRequestHeader` → SHAPE-ONCE `formatForWire` → `toString` (must be scalar) | non-null literal required for the string overload. |
| **Gather header — typed source / event arg** | `g.Header("X-Key", src)` / `g.Header("X-Key", args, e=>e.X)` | `RequestScalarTarget.Header<TProp>` enforces **scalar** at authoring (array/object rejected); source via VALUE-SPINE | header assignment; runtime: missing value → header omitted (`isMissingRuntimeValue` skip). | scalar-only; missing → header simply not sent (not `""`). |
| **Gather route-param — static (int/long/string)** | `g.RouteParam("id", 5)` | `RequestInputAssignment.RouteParameter(RouteParameterName, Literal)`; null string rejected | `{"target":{"kind":"route-param","name":"id"},"source":{"kind":"literal",…}}` | non-null literal. |
| **Gather route-param — typed source / event arg** | `g.RouteParam("id", src)` | `RequestScalarTarget.RouteParameter<TProp>` enforces scalar; VALUE-SPINE | ⇒ `writeRequestRouteParam`: null **throws** (`cannot build URL`) — a real boundary, not a fallback | scalar-only; null route param is an error (the URL cannot be formed). |
| **Gather — include all registered inputs** | `g.IncludeAll()` | `GatherInputDraft.IncludeAllRegisteredInputs()` → `RegisteredInputSelection.AllRegisteredInputs`. ⇒ `writeRuntimeSelectedInputs` iterates **mounted** registered components, reads each value member | `…"registeredInputs":{"kind":"all-registered-inputs"}`. Browser: every mounted registered input's value written to body. | only **mounted** inputs (unmounted skipped); declared shape per input. |
| **No gather (bodiless)** | `p.Delete("/x/{id}")` with only a route param | `GatherInputDraft.BuildRequestInput` → if no assignments and no registered selection → `RequestInput.None` | `…"input":{"kind":"none"}`. Browser: no body, route param still substitutes URL. | `none` strategy — empty body, not `{}`. |

> **Body format axis** (`AsJson` default / `AsFormData`): selects the writer for
> POST/PUT/DELETE (`request-payload-writer.ts`). GET ignores it (query string).
> Files (`FileList` / `{rawFile}`) force form-data; GET-with-file and JSON-with-file
> throw at the egress boundary. One `RequestPayloadWriter` owns all of this.

### B.4 — Body egress determinism (the writer)

| Feature / variant | Input → resolved value | Module path | Output (browser wire form) | Good default |
|---|---|---|---|---|
| **Body format — AsJson (default)** | `.AsJson()` or no call (`HttpRequestBuilder.cs:62`) | Request → `_bodyFormat = RequestBodyFormat.Json`. ⇒ `requestPayloadWriterFor` picks the JSON writer for non-GET verbs | `"bodyFormat":"json"`. Browser: JSON body, `Content-Type: application/json`. | JSON is the default body format when `AsFormData` is not called. |
| **Body format — AsFormData** | `.AsFormData()` (`HttpRequestBuilder.cs:65`) | Request → `_bodyFormat = RequestBodyFormat.FormData`. ⇒ writer picks the form-data writer for non-GET verbs | `"bodyFormat":"formdata"`. Browser: `FormData` body; `Content-Type` left to the browser. | explicit opt-in; files (`FileList`/`{rawFile}`) also force form-data at the egress boundary. GET ignores `bodyFormat` (always query string). |
| **Scalar → JSON body** | scalar value, JSON format, non-GET | ⇒ `createJsonBodyWriter.emitScalar` → SHAPE-ONCE `formatForWire` → `jsonBodyValue` (`""`→`null`) → nested `assignJsonBodyValue` by `path` | `{ "<path>": <value> }`; empty string becomes `null`. | cleared field (`""`) → `null` (one named policy). |
| **Array → JSON body** | array value | ⇒ `emitArray` → reject `File` items → each item `formatForWire(itemShape)` | `{ "<name>": [<item>…] }`. | items shaped by declared item shape. |
| **Scalar/array → query string (GET)** | any value, GET | ⇒ `createQueryStringWriter` → `encodeURIComponent`; arrays repeat the key; `File` in GET → throw | `?name=a&name=b`. | repeated key per array item. |
| **Scalar/array/File → form-data** | any value, `AsFormData` | ⇒ `createFormDataWriter`; `File`/`{rawFile}` appended with filename | `FormData` entries; `Content-Type` left to the browser. | files keep filename. |

### B.5 — Response routes (success / error scopes)

| Feature / variant | Input (DSL) | Module path | Output (plan JSON + browser behavior) | Good default |
|---|---|---|---|---|
| **OnSuccess (untyped)** | `r => r.OnSuccess(s => { … })` | Request `ResponseBuilder.OnSuccess` → sub-`ReactionPipelineDraft` → `AddSuccessRoute(reaction, match=Any)`. ⇒ `routeSuccess` → `routeResponseRoutes(success, status)` picks exact-status route else any-status | `"success":[{"match":{"kind":"any"},"reaction":{…}}]`. Browser: on `response.ok`, run reaction in success scope. | match = `any` when no status given. |
| **OnSuccess<TResponse> (typed body)** | `r => r.OnSuccess<R>((json, s) => …)` | success scope opened with `PayloadContract.ForPayload(R)`; `json.Read(r=>r.X)` → payload read in success scope (VALUE-SPINE) | success route + reads with `from.scope="success"`, typed contract. Browser: body parsed (json/text) → success scope. | typed body provides compile-time paths; runtime still reads JSON. |
| **OnError — any-status** | `r => r.OnError(p => …)` (`ResponseBuilder.cs`, no status) | `AddErrorRoute(reaction)` → `match=Any`. ⇒ `routeError` → `routeResponseRoutes` (`http.ts:263`) | `"error":[{"match":{"kind":"any"},"reaction":{…}}]`. Browser: runs only when no exact-status error route matches the response status (see routing rule below). | match = `any`. |
| **OnError — by status** | `r => r.OnError(404, p => …)` (`ResponseBuilder.cs:67`) | `AddErrorRoute(statusCode, reaction)`; status → `ExactResponseStatusMatch`. ⇒ same `routeResponseRoutes` | `"error":[{"match":{"kind":"status","status":404},…}]`. Browser: runs when the response status equals 404 (preferred over any any-status route). | exact status match. |
| **OnError<TError> — typed body, any-status** | `r => r.OnError<E>((err, p) => …)` (`ResponseBuilder.cs:79`) | error scope opened with `PayloadContract.ForPayload(E)`; `match=Any`. Body reads via VALUE-SPINE in error scope. | typed any-status error route; reads with `from.scope="error"`. Browser: typed error body, any-status routing. | typed body, match = `any`. |
| **OnError<TError> — typed body, by status (4th overload)** | `r => r.OnError<E>(404, (err, p) => …)` (`ResponseBuilder.cs:96`) | `AddErrorRoute(statusCode, reaction)` with an error scope opened from `PayloadContract.ForPayload(E)` AND `ExactResponseStatusMatch(404)` — the typed-body × exact-status combination. ⇒ same `routeResponseRoutes` | `"error":[{"match":{"kind":"status","status":404},"reaction":{…with from.scope="error"…}}]`. Browser: runs when status equals 404, with the typed error body parsed into the error scope. | typed body + exact status; this is the 4th `OnError` overload (`ResponseBuilder.cs:96`), distinct from the typed any-status form. |
| **Response unavailable (network failure)** | (no DSL — runtime path) | ⇒ `exchangeOutcomeFromClientFailure` → `routeResponseUnavailable` → only the **any-status** error route runs (no body) | runs the any-status error reaction; never a success route. | network failure routes to any-error only; finally still runs. |

> **Response-route selection rule (deterministic; the redesign keeps it).**
> Both success and error routing run through the one `routeResponseRoutes`
> function (`runtime/execution/http.ts:263`), which is
> `routes.find(routeMatchesStatus(status)) ?? routes.find(routeMatchesAnyStatus)`.
> The rule is **exact-status-preferred, then first any-status** — NOT positional
> first-match. For a given response status the runtime first looks for a route
> whose `match` is an exact status equal to the response status; only if none
> matches does it fall back to the **first** any-status route. So an any-status
> `OnError(p => …)` authored *before* an `OnError(404, …)` still **loses** to the
> 404 route on a 404 response, and wins only when no exact-status route matches.
> (The earlier "first match wins" phrasing was wrong against
> `http.ts:263` — that would imply authored order decides, which it does not.)

### B.6 — Loading / finally

| Feature / variant | Input (DSL) | Module path | Output (plan JSON + browser behavior) | Good default |
|---|---|---|---|---|
| **WhileLoading** | `.WhileLoading(p => p.Element("spinner").Show())` | Request → sub-pipeline → `RequestReactions.From(whileLoading, …)` | `"whileLoading":[{…}]`. ⇒ `runRequestReactions(whileLoading)` **awaited before fetch**. Browser: spinner shown before the request sends. | replaces prior whileLoading (single block, `_whileLoading.Clear()`). |
| **Finally** | `.Finally(p => p.Element("spinner").Hide())` | Request → `RequestReactions.From(…, finally)` | `"finally":[{…}]`. ⇒ `routeAndComplete` runs finally in a `try/finally` **after routing, regardless of outcome** (incl. network failure). Browser: spinner always hidden. | always runs; no response-body access (body may not exist). |
| **Validate<TSource>(formId)** | `.Validate<V>("resident-form")` | Request → `ClientValidationBeforeRequest` → `RequestValidationTarget.DisplayIn(ComponentId)` + registers a validation job (Validation module). ⇒ `requestCanSend` → `validateContainer`; invalid → **abort, no fetch** | `"validation":{"kind":"container","container":"resident-form"}`. Browser: client rules run; failure shows errors and the request never sends. | `none` target when not called → always sends. |

### B.7 — Chained & parallel

| Feature / variant | Input (DSL) | Module path | Output (plan JSON + browser behavior) | Good default |
|---|---|---|---|---|
| **Chained** | `r.Chained(req => req.Get("/next/{id}").Gather(g => g.RouteParam("id", json.Read(...))))` | Request `ResponseBuilder.Chained` → builds a full child `RequestPlan` → `ResponseRouting.Chain = FollowUpRequestChain(next)`. ⇒ `runFollowUpRequest` runs the next request **only after success**, in the success/response context (can gather from the prior body) | `"chain":{"kind":"follow-up","next":{…full request…}}`. Browser: success → run next request; the next may read the prior response. | terminal (`{"kind":"terminal"}`) when no `Chained`. Chain fires on success only. |
| **Parallel** | `p.Parallel(b1 => b1.Get(...), b2 => b2.Post(...)).OnAllSettled(p => …)` | Reaction → `ReactionPipelineDraft.BeginParallel` (async lane) → `ParallelBuilder.AddBranch` builds each `RequestPlan` → `ParallelDraft.ToReaction`. ⇒ branches started concurrently (`Promise.all`), completion runs after **all settle** | a `parallel` reaction node carrying branch requests + an `onAllSettled` reaction. Browser: all branches fire concurrently; completion runs once all settle (success or error). | no completion reaction if `OnAllSettled` not called; all branches always started. |

### B.8 — Inject success body (`Into` — the only string-shaped success-body sink)

`Into(elementId)` follows a request and injects the **whole success body** into an
element's `innerHTML`. Its value is fixed (no value axis — see the triggers file
inject row); the only determinism question is the **shape** of that value.

| Feature / variant | Input (DSL) | Module path | Output (plan JSON + browser behavior) | Good default |
|---|---|---|---|---|
| **Inject — success body is string-shaped** | `p.Get("/card").Into("card-host")` | Request success scope → `Into` builds `value = ReadWholePayload(Success)` and emits `ReactionGraph.Inject(slot, value)` (SYNC, within the awaited request's success scope). ⇒ `executeInject` (`execute.ts:207-218`): `evaluateValue(reaction.value)`; if `typeof value === "string"` → `injectHtml(container, value, slot)` (`execute.ts:210-213`); otherwise `log.error("inject.wrong-type")` then **throws** `[alis] inject expects string HTML, got <type>` (`execute.ts:216-217`) | **Current** (`ValueExpression.cs:379,399-400`; `runtime/types/plan.ts:783-786`): `{ "kind":"inject","slot":"card-host","value":{"kind":"read","from":{"kind":"payload","scope":"success"},"member":"responseBody","path":null} }` — the magic-member sentinel (see Values determinism note). **→ Redesign** (distinct node kind, identical to the triggers-file inject row and the Part A whole-payload shape at line 93): `{ "kind":"inject","slot":"card-host","value":{"kind":"whole-payload","from":{"kind":"payload","scope":"success"}} }`. Browser (both): when the success body is a string (HTML/text response) the element's `innerHTML` is replaced; when the success body parsed to a non-string (a JSON object/array — `application/json` response) the inject **throws a typed shape error at the egress boundary** (`execute.ts:216-217`), it does NOT silently inject `[object Object]`. | inject's value is **string-shaped**: the value axis is fixed (always the whole success body), so the only determinism question is shape. A non-string evaluated value is a typed/shape boundary error, not silent coercion. The deterministic contract is "this endpoint must return an HTML/text body"; a JSON body is a developer/endpoint mismatch surfaced at the boundary — the *same category* as `getElementById` returning null, not a plan-validator. |

> **Determinism note (HTTP):** the request graph is fully decided at authoring —
> verb, URL, every gather assignment, every response route + its status match,
> the chain, the parallel branches, validation target, loading/finally. The
> runtime walks a fixed pipeline (`gather → fetch → route → finally → chain`)
> with **exact-status-preferred-then-first-any** status routing
> (`routeResponseRoutes`, `http.ts:263` = `find(exactStatus) ?? find(anyStatus)`)
> and **only** the documented async lane. The
> redesign folds the 7-scope-onto-3-field `PayloadScope` to the scopes that
> actually carry data (drops the dead `local`), and consolidates all FormData/File
> body knowledge into the one `RequestPayloadWriter`.

---

## Part C — Arrays DSL (every op)

**Module owner:** Value (the `ArrayOp` variant) + `ArrayOpEngine` (⇒). Pure sync —
NO Request lane, NO Promise. A `ReactiveArray<T>` is a deferred transform whose
operators compile to `array-op` `ValueExpression` nodes; nothing executes on the
server. Terminal scalars become `ReactiveValue<T>` (a `TypedSource`), so they plug
into every value sink.

> **Parameter axis P-ENTRY** (how an array source is obtained, `PipelineBuilder.Arrays.cs`):
> `From(TypedSource<T[]>)` (component/method/url/plugin array) · `From(args, e=>e.Data)`
> (event-payload array) · `FromDom(id, member)` (DOM array-like). Each yields the
> initial `source` `ValueExpression` for the op chain.
> **Parameter axis P-OP** (the eight ops): `count · filter · map · sum · any · all
> · find · orderBy(/Descending)`. Each is **one `array-op` node**, op as
> sub-discriminator (mirrors `CompareCondition`).

Every `array-op` node has the same envelope:
`{ "kind":"array-op", "op":"<op>", "source":<ValueExpr>, "itemShape":<Shape>, "shape":<outShape>, ["predicate":<ConditionGraph>], ["projection":<ValueExpr>] }`.
Per-element `predicate`/`projection` read the **element scope** (`PayloadSource.Element`).
In the redesign these become **per-op variants** (each op carries only the fields it
uses) instead of one node with nullable+`[JsonIgnore]` predicate/projection pairs.

| Feature / variant | Input (DSL) | Module path | Output (plan JSON + browser behavior) | Good default |
|---|---|---|---|---|
| **Source — From(TypedSource<T[]>)** | `p.From(p.Component<Multi>(m=>m.Tags).Value())` | Value → `new ReactiveArray<T>(source.ToValueExpression(), Shape.FromClrType(T))` | the op chain's `source` is the inner read node; `itemShape` from `T`. | element type carried through chain. |
| **Source — From(event arg array)** | `p.From(args, e => e.Data)` | `PayloadTypedSource.FromEvent(selector).ToValueExpression()` → payload read in event scope | `source` = event payload read. | — |
| **Source — FromDom(id, member)** | `p.FromDom("card","classList")` | `ReadDom(id, member, None)`; element type `string` (or typed overload) | `source` = dom read; runtime normalizes DOMTokenList/HTMLCollection/NodeList to a JS array. | `string` elements by default. |
| **count (unconditional)** | `.Count()` | Value → `ArrayCount(source, itemShape)` → out `Shape.Number`; result is `ReactiveValue<int>`. ⇒ `ArrayOpEngine` `count` → `items.length` | `{ "op":"count","shape":{"kind":"number"} }` (no predicate). Browser: array length. | no predicate node. |
| **count (predicated)** | `.Count(x => x.Active)` | `Where(pred).Count()` — compiles to **filter → count** (count never carries a predicate) | a `filter` array-op feeding a `count` array-op. Browser: filtered length. | predicated count is sugar over filter+count. |
| **filter** | `.Where(x => x.Active)` | Value → `ArrayFilter(source, ConditionGraph predicate, itemShape)` (predicate = sync condition subset compiled by `ElementExpressionCompiler`) → out `array<itemShape>`. ⇒ engine `filter` → `items.filter(elementMatches(predicate))` | `{ "op":"filter","predicate":<ConditionGraph>,"shape":{"kind":"array","item":<itemShape>} }`. Browser: kept elements, shaped. | predicate evaluated per element on the immediate/sync lane (never confirm). |
| **map** | `.Select(x => x.Name)` | `ArrayMap(source, projection, itemShape, resultItemShape=FromClrType(TResult))` → out `array<resultItem>`. ⇒ engine `map` → `items.map(project(projection))` | `{ "op":"map","projection":<ValueExpr>,"shape":{"kind":"array","item":<result>} }`. Browser: projected array. | result element type from `TResult`. |
| **sum — int selector** | `.Sum(x => x.Count)` (`ReactiveArray.cs:90`) | `ArraySum(source, Projection(selector), itemShape)` → out `Shape.Number`; result `ReactiveValue<int>`. ⇒ engine `sum` → `reduce(total + toNumber(projectedOrSelf))` | `{ "op":"sum","projection":<ValueExpr>,"shape":{"kind":"number"} }`. Browser: numeric sum; non-finite contributes 0. | int selector → `ReactiveValue<int>`. One `sum` op node — the three numeric overloads differ only in the C# CLR return type, not the wire node. |
| **sum — decimal selector** | `.Sum(x => x.Amount)` (`ReactiveArray.cs:94`) | `ArraySum(source, Projection(selector), itemShape)` → out `Shape.Number`; result `ReactiveValue<decimal>`. ⇒ same engine `sum` | `{ "op":"sum","projection":<ValueExpr>,"shape":{"kind":"number"} }`. Browser: numeric sum. | decimal selector → `ReactiveValue<decimal>`; same `sum` node shape. |
| **sum — double selector** | `.Sum(x => x.Weight)` (`ReactiveArray.cs:98`) | `ArraySum(source, Projection(selector), itemShape)` → out `Shape.Number`; result `ReactiveValue<double>`. ⇒ same engine `sum` | `{ "op":"sum","projection":<ValueExpr>,"shape":{"kind":"number"} }`. Browser: numeric sum. | double selector → `ReactiveValue<double>`; same `sum` node shape. The three rows share one node — the generator emits one `sum` template; the CLR return type only types the terminal `ReactiveValue<T>`. |
| **any (unconditional)** | `.Any()` | `ArrayAny(source, predicate:null, itemShape)` → `Shape.Boolean`. ⇒ engine `any` (predicate undefined) → `items.length > 0` | `{ "op":"any","shape":{"kind":"boolean"} }` (no predicate). Browser: non-empty? | no predicate → "is non-empty". |
| **any (predicated)** | `.Any(x => x.Active)` | `ArrayAny(source, predicate, itemShape)`. ⇒ `items.some(elementMatches)` | `{ "op":"any","predicate":<ConditionGraph>,… }`. Browser: any match? | — |
| **all** | `.All(x => x.Valid)` | `ArrayAll(source, predicate, itemShape)` → `Shape.Boolean`. ⇒ `items.every(elementMatches)` | `{ "op":"all","predicate":<ConditionGraph>,"shape":{"kind":"boolean"} }`. Browser: every match? (vacuously true on empty). | predicate required. |
| **find (element)** | `.Find(x => x.Id == 3)` | `ArrayFind(source, predicate, projection:null, itemShape, resultShape=itemShape)`; `ReactiveValue<T>`. ⇒ engine `find` → first match or `null` | `{ "op":"find","predicate":<ConditionGraph>,"shape":<itemShape> }`. Browser: first matching element or `null`. | `null` when none match (find requires a predicate — runtime throws if absent, a generation invariant). |
| **find (projected field)** | `.Find(x => x.Id==3, x => x.Name)` | `ArrayFind(…, projection, itemShape, fieldShape=FromClrType(TField))`; `ReactiveValue<TField>`. ⇒ first match then `project(projection)` | `{ "op":"find","predicate":…,"projection":<ValueExpr>,"shape":<field> }`. Browser: projected field of first match, or `null`. | result shape from `TField`. |
| **orderBy / orderByDescending** | `.OrderBy(x => x.StartDate)` / `.OrderByDescending(...)` | `ArrayOrderBy(source, key, itemShape, descending)` → out `array<itemShape>`; **key must be a sortable scalar** (string/number/bool/date/nullable) — non-scalar key **rejected at authoring**. ⇒ engine `orderBy/orderByDescending` → decorate-sort-undecorate with `compareKeys` (numeric when both numbers, else lexicographic; NaN/Infinity sort last deterministically) | `{ "op":"orderBy"\|"orderByDescending","projection":<key ValueExpr>,"shape":{"kind":"array","item":<itemShape>} }`. Browser: stable, deterministic order. | ascending by default; non-scalar key is a compile-time error (prevents `"[object Object]"` mis-sort). |
| **Chained ops** | `.Where(...).Select(...).OrderBy(...)` | each op wraps the prior op node as its `source` (composition) | nested `array-op` nodes, outermost = last op. Browser: sequential transforms. | — |
| **Terminal as TypedSource** | `arr.AsSource()` → `TypedSource<T[]>` | `ReactiveArraySource` exposes the composed op node as a value source (closes the gather hole in the redesign) | the same `array-op` node, usable in `SetText`/`When`/gather/dispatch. Browser: evaluated where the sink reads it. | the transformed array binds with no HTTP round-trip. |

> **Determinism note (Arrays):** the entire transform is decided at authoring and
> compiled to `array-op` nodes; the runtime `ArrayOpEngine` runs a fixed switch
> over the eight ops with deterministic element-scope evaluation. Predicates are
> the **sync condition subset only** (compare/all/any/not — never confirm), so the
> whole array DSL stays on the immediate lane. The redesign extracts the array-op
> engine out of the 300-line `evaluate.ts` god-class into its own module and turns
> the nullable+`[JsonIgnore]` predicate/projection pair into per-op variants.

---

## Coverage summary

### Counts (features + variants made deterministic)

| Band | Features + variants | Deterministic? |
|---|---|---|
| **Values** — literals (3), reads ×P-SOURCE (10), composites (2) | **15** | Yes |
| **HTTP** — verbs (3), endpoint/template (1), gather ×P-TARGET (11), body egress (4), response routes (4), loading/finally (3), chained+parallel (2) | **28** | Yes |
| **Arrays** — entries (3), ops (count×2, filter, map, sum, any×2, all, find×2, orderBy) (11), chaining+terminal (2) | **16** | Yes |
| **Total** | **59** | **59 deterministic, 0 non-deterministic** |

Counting parameter axes as their full products (P-SOURCE ×6, P-VERB ×4,
P-TARGET ×4, P-OP ×8, P-SHAPE ×10, P-ENTRY ×3) the 59 written rows stand for
**thousands** of concrete generated cases from the same node templates — which is
the point: the matrix is the generator spec.

### How each case parameterizes (scales to thousands)

- **Values** scale on **P-SOURCE × P-SHAPE** (6 sources × ~10 shapes) over one
  `Read` template + one `Literal` template + `Object`/`Array`. One row per source
  kind ⇒ ~60 read combinations from a single template.
- **HTTP gather** scales on **P-TARGET × (any value source) × P-VERB × body
  format** — each assignment is `(target, value)` where `value` is any Values
  case, so the gather product is `4 targets × 15 value cases × 4 verbs`.
- **Response routes** scale on **status code × scope** (any-status + N exact
  statuses × success/error) — one route template, **exact-status-preferred-then-first-any**
  routing (`routeResponseRoutes`, `http.ts:263` = `find(exactStatus) ?? find(anyStatus)`),
  NOT positional first-match.
- **Arrays** scale on **P-ENTRY × P-OP × element type** and **composition depth**
  (any op's `source` is any prior op or any array-shaped Values case), so chains
  are the free monoid over the 8 ops.

---

## Cases that could NOT be made fully deterministic (and why)

All 59 rows are deterministic at the **plan-shape** level: a given DSL input
produces exactly one plan JSON. The following are **runtime-environment**
non-determinism that is *correctly* handled as a true external boundary (per the
core rule — boundary checks are allowed; they are not plan-validators or
fallbacks). They are called out so the generator does not try to "make them
deterministic" by inventing plan information:

1. **Array-op source normalization at the input boundary** (`evaluate.ts`
   `normalizeToArray`). A browser/EJ2 array source may return `Array | array-like
   | iterable | scalar | null`; the C# `T[]` type cannot constrain the live JS
   value. The runtime normalizes (wrap scalar, `[]` for null/undefined,
   `Array.from` for iterables) and **throws** for a genuinely non-iterable object
   (e.g. `DOMStringMap`). This is the *same category* as `getElementById`
   returning null — an external boundary, deterministic in outcome (always the
   same rule), but the **input** value is environment-determined, not plan-determined.

2. **HTTP response body presence / content type** (`http.ts` `readResponseBody`,
   `responseContentKind`). Whether a response carries JSON, text, or an empty body
   is server-determined at runtime. The routing (status match, success/error) is
   fully deterministic; the *body availability* is a network boundary. A network
   failure deterministically routes to the any-status error route + finally, but
   *whether* a failure occurs is environmental.

3. **`IncludeAll()` mounted-input set** (`gather.ts` `writeRuntimeSelectedInputs`).
   The plan deterministically says "all registered inputs"; *which* inputs are
   currently **mounted** (and thus included) depends on slot load/unload state at
   request time. This is deterministic given the active composed plan, but the
   composed plan is a runtime fact (Slot module), not a single static plan node.

None of these is a DSL-feature non-determinism: every one is an explicit,
single-rule external boundary. There are **no DSL features in this band that the
redesign cannot make produce exactly one plan JSON.** The two representable-but-
invalid holes the *old* design carried (the `responseBody`/`elementValue` magic
sentinels and the gather-source hole) are closed by the redesign — see the Values
and Arrays determinism notes — so they are no longer non-deterministic risks.
