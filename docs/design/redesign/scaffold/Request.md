# Request — Implementation Spec (Scaffold)

> **How to use this file.** This is the mechanical coding spec for the **Request**
> micro-module. Open it, read the Responsibility, copy the Skeleton, and fill each
> `// TODO` body by matching it to the named acceptance fixture in §7. Every type,
> signature, name, and JSON shape below was read from the actual source under
> `Alis.Reactive/Builders/Requests/`, `Alis.Reactive/PlanModel/`, and
> `Alis.Reactive.Assets/runtime/execution/` — not inferred. Names come from
> [`03-naming.md`](../03-naming.md); cases come from
> [`04-matrix-http-arrays-values.md`](../04-matrix-http-arrays-values.md) Part B.
>
> Source provenance (read before editing): `HttpRequestBuilder.cs`,
> `GatherBuilder.cs`, `GatherInputDraft.cs`, `ResponseBuilder.cs`,
> `ResponseRoutingDraft.cs`, `ParallelBuilder.cs`, `ParallelDraft.cs`,
> `RequestRouteTemplate.cs`, `RequestParameterNames.cs`, `RequestPlan.cs`,
> `RequestInput.cs`, `GatherRequestInput.cs`, `PlanTerms.cs` (HTTP value objects),
> `ReactionGraph.cs` (`RequestReaction`/`ParallelReaction`), `http.ts`, `gather.ts`,
> `request-payload-writer.ts`, `http-fetch.ts`, `plan.ts` (request block).

---

## 1. Responsibility, Ownership, Dependencies

**Responsibility (one sentence).** Request is the framework's *only network async
lane*: it authors a `Get/Post/Put/Delete` with gathered input (`target <- value`),
routes the success/error response into scopes, chains a follow-up after success,
and runs branches in parallel — lowering all of it to a `RequestPlan` the runtime
walks as a fixed `gather → fetch → route → finally → chain` pipeline.

**What it owns** (from [`02-micro-modules.md`](../02-micro-modules.md) row *Request*):

| Side | Owns |
|---|---|
| `→` author | `HttpRequestBuilder<TModel>` · `GatherBuilder<TModel>` + the internal `GatherInputDraft` · `ResponseBuilder<TModel>` + the internal `ResponseRoutingDraft` · `ParallelBuilder<TModel>` + the internal `ParallelDraft` · `RequestRouteTemplate` (placeholder validation) · the HTTP value objects (`RequestUrl`, `HttpMethodName`, `RequestBodyFormat`, `HeaderName`, `RouteParameterName`, `UrlParameterName`, `HttpResponseStatusCode`, `RequestScalarTarget`) |
| `→` node | `RequestPlan` · `RequestEndpoint` · `RequestInput`/`NoRequestInput`/`GatherRequestInput` · `RequestInputAssignment` + `RequestInputTarget` family (`RequestPayloadTarget`/`RequestHeaderTarget`/`RequestRouteParameterTarget`) · `RegisteredInputSelection` · `ResponseRouting`/`ResponseRoute`/`ResponseStatusMatch` family · `RequestChain` family (`TerminalRequestChain`/`FollowUpRequestChain`) · `RequestValidationTarget` family · `RequestReactions` (while-loading + finally) |
| `→` reaction nodes | `RequestReaction` (`kind:"request"`) and `ParallelReaction` (`kind:"parallel"`) + `ParallelCompletion` family — these *live in* `ReactionGraph.cs` and are *constructed by* Request via `ReactionGraph.Request(...)` / `ReactionGraph.Parallel(...)` |
| `⇒` runtime | `http.ts` pipeline · `gather.ts` (`resolveRequestInput`) · `request-payload-writer.ts` (the **one** FormData/File/JSON/query writer) · `http-fetch.ts` (`resolveFetch`) |

**What it depends on** (the redesign graph edges `Request → Value · Condition ·
Component · Shape · Kind`, plus the Plan-spine sink):

- **Value** — every gathered source and every response read is a `ValueExpression`
  produced through `TypedSource<T>`/`ValueExpression` and read back by `evaluateValue`.
  Request never builds a second value resolver.
- **Shape** — `RequestScalarTarget` infers `Shape.FromClrType` and enforces
  `IsScalar` for header/route-param/url targets; the runtime shapes **once** on the
  egress path (`RuntimeShape.formatForWire`, the SHAPE-ONCE rule).
- **Component** — `GatherBuilder.Include(...)` calls `IdGenerator`/the one id regime
  and declares the input through the Plan sink (`PlanBuildContext.DeclareInputComponent`
  / `RequireRegistrationById`); `IncludeAll()` reads every mounted registered input.
- **Condition** — sub-reactions inside `WhileLoading`/`Finally`/`OnSuccess`/`OnError`
  may carry guards; Request just nests a `ReactionGraph` and never evaluates it itself.
- **Kind** — every node carries its `Kind` string; the generated `plan.ts` and
  `assertNever` exhaustiveness come from the Kind kernel, not hand-authored here.
- **Plan (spine)** — Request is *constructed against* a `PlanBuildContext`: it
  receives the context, declares gathered components on it, and registers a
  validation job on it (`RegisterValidationJob`). Request does **not** depend on the
  Validation slice directly — it hands the job to the Plan sink, which the
  Validation slice reads. (Source: `HttpRequestBuilder.Validate` →
  `ClientValidationBeforeRequest.Register` → `PlanBuildContext.RegisterValidationJob`.)

**Async lane.** Request is the band's *only* async opener. `ReactionPipelineDraft`
(Reaction slice) stamps the async lane when it calls `BeginHttp` / `BeginParallel`;
Request itself produces no lane flag — it produces the nodes the stamped lane wraps.
The runtime `http.ts` is `async` end-to-end; everything Request authors is awaited.

---

## 2. Public Surface (exact signatures + intent)

> Visibility is load-bearing. Public builders are the frozen DSL surface; **all
> constructors are `internal`**; all plan-node properties are get-only with
> `internal` construction. Drafts and value objects are `internal sealed`. This
> mirrors the current source exactly — do not widen any of it.

### 2.1 `HttpRequestBuilder<TModel> where TModel : class` (public, ctor internal)

The fluent HTTP request. Obtained from `PipelineBuilder.Get/Post/Put/Delete` (which
call `ReactionPipelineDraft.BeginHttp(Context)`), or as a chained/parallel child.

```csharp
internal HttpRequestBuilder(PlanBuildContext context);

public HttpRequestBuilder<TModel> Get(string url);     // method = GET
public HttpRequestBuilder<TModel> Post(string url);    // method = POST
public HttpRequestBuilder<TModel> Put(string url);     // method = PUT
public HttpRequestBuilder<TModel> Delete(string url);  // method = DELETE

public HttpRequestBuilder<TModel> Gather(Action<GatherBuilder<TModel>> gather);
public HttpRequestBuilder<TModel> AsJson();      // body format = json (default)
public HttpRequestBuilder<TModel> AsFormData();  // body format = form-data
public HttpRequestBuilder<TModel> WhileLoading(Action<PipelineBuilder<TModel>> pipeline);
public HttpRequestBuilder<TModel> Finally(Action<PipelineBuilder<TModel>> pipeline);
public HttpRequestBuilder<TModel> Validate<TValidationSource>(string formId)
    where TValidationSource : class;
public HttpRequestBuilder<TModel> Response(Action<ResponseBuilder<TModel>> response);

internal RequestPlan BuildRequest();  // the one lowerer → RequestPlan
```

**Intent per method:**
- `Get/Post/Put/Delete` — *select the endpoint*. Each calls
  `RequestEndpoint.To(method, RequestUrl.Of(url))`; URL may contain `{placeholder}`.
- `Gather` — *configure request input*. Build a fresh `GatherInputDraft` through a
  `GatherBuilder`, then keep it as `_requestInput`.
- `AsJson`/`AsFormData` — *select body egress format*; default is `Json`. GET ignores
  the format (query string), but it is still carried.
- `WhileLoading` — *one* reaction graph run **before** fetch (replace, not append:
  `_whileLoading.Clear()` then add).
- `Finally` — *one* reaction graph run after settle regardless of outcome (replace).
- `Validate<TSource>(formId)` — record client validation before sending; produces a
  `RequestValidationTarget.DisplayIn(ComponentId.Of(formId))` *and* registers a
  validation job on the context.
- `Response` — open a `ResponseBuilder` and keep its draft for success/error/chain.
- `BuildRequest` — the single write path: assemble `RequestPlan.Create(endpoint,
  input, RequestReactions.From(whileLoading, finally), routing, validationTarget)`,
  registering the validation job after the request exists.

### 2.2 `GatherBuilder<TModel> where TModel : class` (public, ctor internal)

`target <- value` assignments. Obtained from `HttpRequestBuilder.Gather`. Every
overload writes into the shared internal `GatherInputDraft`.

```csharp
internal GatherBuilder(PlanBuildContext context, GatherInputDraft draft);

public GatherBuilder<TModel> IncludeAll();
public GatherBuilder<TModel> Static(string param, object value);
public GatherBuilder<TModel> FromEvent<TArgs, TProp>(TArgs args, Expression<Func<TArgs, TProp>> path, string param);

public GatherBuilder<TModel> Header(string name, string value);                                   // literal, non-null
public GatherBuilder<TModel> Header<TProp>(string name, TypedSource<TProp> source);               // scalar-enforced
public GatherBuilder<TModel> Header<TArgs, TProp>(string name, TArgs args, Expression<Func<TArgs, TProp>> path);

public GatherBuilder<TModel> RouteParam(string paramName, int value);
public GatherBuilder<TModel> RouteParam(string paramName, string value);                          // non-null
public GatherBuilder<TModel> RouteParam(string paramName, long value);
public GatherBuilder<TModel> RouteParam<TProp>(string paramName, TypedSource<TProp> source);      // scalar-enforced
public GatherBuilder<TModel> RouteParam<TArgs, TProp>(string paramName, TArgs args, Expression<Func<TArgs, TProp>> path);

public GatherBuilder<TModel> FromUrl(string paramName);
public GatherBuilder<TModel> FromUrl(string paramName, string asParam);
public GatherBuilder<TModel> FromUrl<T>(string paramName);                                        // scalar-enforced
public GatherBuilder<TModel> FromUrl<T>(string paramName, string asParam);

public GatherBuilder<TModel> Plugin<T>(TypedPluginSource<T> source, string paramName);

// component value gather — invoked by vendor extension methods, NOT public:
internal GatherBuilder<TModel> Include(string componentId, string vendor, string propertyName, string valueMember);
internal GatherBuilder<TModel> Include(string componentId, string vendor, string propertyName, string valueMember, Shape shape);
internal GatherBuilder<TModel> Include<TProp>(TypedComponentSource<TProp> source, string paramName);
```

**Intent:** each overload builds a `RequestInputAssignment` (`Payload`/`Header`/
`RouteParameter`) whose `source` is a `ValueExpression` from the Value spine, and
appends it to the draft. Header/route-param/url-typed overloads pass through
`RequestScalarTarget` to *reject non-scalar shapes at authoring*. The component
`Include` overloads additionally declare the input on the context so `IncludeAll`
and validation can see it.

### 2.3 `ResponseBuilder<TModel> where TModel : class` (public, ctor internal)

Success/error/chain routing. Obtained from `HttpRequestBuilder.Response`.

```csharp
internal ResponseBuilder(PlanBuildContext context);
internal ResponseRoutingDraft Draft { get; }

public ResponseBuilder<TModel> OnSuccess(Action<PipelineBuilder<TModel>> pipeline);
public ResponseBuilder<TModel> OnSuccess<TResponse>(Action<ResponseBody<TResponse>, PipelineBuilder<TModel>> pipeline) where TResponse : class;
public ResponseBuilder<TModel> OnError(Action<PipelineBuilder<TModel>> pipeline);
public ResponseBuilder<TModel> OnError(int statusCode, Action<PipelineBuilder<TModel>> pipeline);
public ResponseBuilder<TModel> OnError<TError>(Action<ResponseBody<TError>, PipelineBuilder<TModel>> pipeline) where TError : class;
public ResponseBuilder<TModel> OnError<TError>(int statusCode, Action<ResponseBody<TError>, PipelineBuilder<TModel>> pipeline) where TError : class;
public ResponseBuilder<TModel> Chained(Action<HttpRequestBuilder<TModel>> request);
```

**Intent:** typed overloads open a `ResponseBody<T>` over `PayloadSource.Success/
Error(PayloadContract.ForPayload(typeof(T)))` (Value spine) so `json.Read(r => r.X)`
reads in the right scope. `OnError(statusCode, ...)` adds an *exact-status* route;
status-less overloads add *any-status* routes. `Chained` builds a full child
`RequestPlan` and records it as the chain (exactly one follow-up allowed).

### 2.4 `ParallelBuilder<TModel> where TModel : class` (public, ctor internal)

Concurrent branches. Obtained from `PipelineBuilder.Parallel(params ...)` (which
calls `ReactionPipelineDraft.BeginParallel(Context)` then `AddBranch` per branch).

```csharp
internal ParallelBuilder(PlanBuildContext context);
internal void AddBranch(Action<HttpRequestBuilder<TModel>> request);
public ParallelBuilder<TModel> OnAllSettled(Action<PipelineBuilder<TModel>> pipeline);
internal ReactionGraph BuildReaction();   // → ParallelReaction via ParallelDraft.ToReaction()
```

### 2.5 Internal drafts (state accumulators, `internal sealed`)

```csharp
internal sealed class GatherInputDraft {
    internal RegisteredInputSelection RegisteredInputs { get; }      // default: ExplicitAssignments
    internal RequestInput BuildRequestInput(RequestBodyFormat bodyFormat, RequestUrl url);
    internal void IncludeAllRegisteredInputs();
    internal void AddAssignment(RequestInputAssignment assignment);
    internal void AddPayload(BindingPath path, ValueExpression value);
    internal void AddHeader(HeaderName name, ValueExpression value);
    internal void AddRouteParameter(RouteParameterName name, ValueExpression value);
}

internal sealed class ResponseRoutingDraft {
    internal ResponseRouting BuildRouting();
    internal void AddSuccessRoute(ReactionGraph reaction);            // any-status
    internal void AddErrorRoute(ReactionGraph reaction);              // any-status
    internal void AddErrorRoute(int statusCode, ReactionGraph reaction); // exact-status
    internal void ContinueWith(RequestPlan request);                  // one follow-up only
}

internal sealed class ParallelDraft {
    internal void AddBranch(RequestPlan request);
    internal void RunWhenAllSettled(ReactionGraph reaction);
    internal ReactionGraph ToReaction();                              // throws if zero branches
}

internal sealed class RequestRouteTemplate {
    internal static RequestRouteTemplate For(RequestUrl url);         // parses {placeholders}
    internal void RequireRouteParameters(IEnumerable<string> routeParameterNames); // bijection check
}
```

### 2.6 Plan-model nodes (public sealed, ctor internal — the wire shape)

`RequestPlan` (sealed, `Create` factory), `RequestEndpoint` (internal sealed),
`RequestReactions`/`ResponseRouting` (internal sealed), and the polymorphic node
families below. Each abstract base carries `[JsonConverter(typeof(
WriteOnlyPolymorphicConverter<...>))]` and an `internal` factory; each concrete node
carries a literal `Kind` string (matched by `plan.ts`):

| Base | Concrete · `Kind` | Carries |
|---|---|---|
| `RequestInput` | `NoRequestInput` · `"none"` ; `GatherRequestInput` · `"gather"` | gather: `Assignments`, `BodyFormat`, `RegisteredInputs` |
| `RequestInputTarget` | `RequestPayloadTarget` · `"payload"` (`Name`,`Path`) ; `RequestHeaderTarget` · `"header"` (`Name`) ; `RequestRouteParameterTarget` · `"route-param"` (`Name`) | the gather target |
| `ResponseStatusMatch` | `AnyResponseStatusMatch` · `"any"` ; `ExactResponseStatusMatch` · `"status"` (`Status`) | route status match |
| `RequestChain` | `TerminalRequestChain` · `"terminal"` ; `FollowUpRequestChain` · `"follow-up"` (`Next`) | follow-up after success |
| `RequestValidationTarget` | `NoRequestValidationTarget` · `"none"` ; `ContainerRequestValidationTarget` · `"container"` (`Container`) | pre-send validation |
| `RequestReaction : ReactionGraph` | `"request"` (`Request`) | a reaction that sends a request |
| `ParallelReaction : ReactionGraph` | `"parallel"` (`Steps`, `Completion`) | concurrent branches |
| `ParallelCompletion` | `NoParallelCompletion` · `"none"` ; `SettledParallelCompletion` · `"on-settled"` (`Reaction`) | after-all-settle |

> **Collision fix (carry from `03-naming.md`).** `RequestReaction.Request` is
> currently declared `public new RequestPlan Request { get; }` (`ReactionGraph.cs:314`)
> to shadow a base member. In the rewrite the base `ReactionGraph` exposes **no**
> `Request` member, so the `new` keyword and the shadow are deleted — `Request` is
> the only `Request` on the node.

### 2.7 TS counterpart (`⇒`, where Request crosses the contract)

The contract is **generated** by the Kind kernel; do not hand-edit `plan.ts`. These
are the generated shapes (mirror of §2.6) the runtime reads — listed so the runtime
skeleton (§5.2) type-checks. Source: `plan.ts` lines 579–696.

```ts
export type HttpMethod = "GET" | "POST" | "PUT" | "DELETE";
export interface RequestPlan {
  method: HttpMethod; url: string;
  validation: RequestValidationTarget;
  input: RequestInput;
  whileLoading: ReactionGraph[];
  success: ResponseRoute[]; error: ResponseRoute[]; finally: ReactionGraph[];
  chain: RequestChain;
}
export type RequestInput = NoRequestInput | GatherRequestInput;          // "none" | "gather"
export type RequestInputTarget = RequestPayloadTarget | RequestHeaderTarget | RequestRouteParameterTarget;
export type ResponseStatusMatch = AnyResponseStatusMatch | ExactResponseStatusMatch; // "any" | "status"
export type RequestChain = TerminalRequestChain | FollowUpRequestChain;  // "terminal" | "follow-up"
export type RequestValidationTarget = NoRequestValidationTarget | ContainerRequestValidationTarget;
export interface RequestReaction { kind: "request"; request: RequestPlan; }
export interface ParallelReaction { kind: "parallel"; steps: ReactionGraph[]; completion: ParallelCompletion; }
```

Runtime entry points (existing names, kept):

```ts
// http.ts
export async function executeRequest(req: RequestPlan, plan: PlanDocument, ctx?: ExecContext): Promise<void>;
// gather.ts
export function resolveRequestInput(input: RequestInput, method: HttpMethod, plan: PlanDocument, ctx: ExecContext): ResolvedRequestInput;
// request-payload-writer.ts
export function requestPayloadWriterFor(requestInput: ResolvedRequestInput, bodyFormat: RequestBodyFormat, method: HttpMethod): RequestPayloadWriter;
export function writeRequestPayloadValue(target: RequestPayloadTarget, raw: unknown, shape: RuntimeShape, writer: RequestPayloadWriter): void;
// http-fetch.ts
export function resolveFetch(request: RequestPlan, resolvedInput: ResolvedRequestInput): ResolvedFetch;
```

---

## 3. Input → Output Contract + Invariants

**Flows in:** a `PlanBuildContext` (the Plan sink) and the developer's chained DSL
calls. **Produces:** exactly one `RequestPlan` per `BuildRequest()`, wrapped by the
Reaction slice into a `RequestReaction`/`ParallelReaction` node. **Runtime in:** that
node + the live `PlanDocument` + `ExecContext`. **Runtime out:** a real `fetch`, the
routed reaction effects, the finally effects, and an optional chained request.

**Invariants (enforced by construction at authoring time — no runtime plan-validation):**

1. **Endpoint required.** `BuildRequest()` throws `InvalidOperationException` if no
   `Get/Post/Put/Delete` was called. *(source: `HttpRequestBuilder.BuildRequest`.)*
2. **Method is closed.** `HttpMethodName` is one of the four interned instances; the
   wire token is exactly `GET/POST/PUT/DELETE`. *(no open enum.)*
3. **URL/route-param bijection.** `RequestRouteTemplate.RequireRouteParameters` runs
   at build and throws if any `{placeholder}` lacks a `RouteParam`, or any
   `RouteParam` lacks a placeholder. **No silent blank substitution.**
4. **Scalar-only headers/route-params/typed-url.** `RequestScalarTarget` throws if
   `Shape.FromClrType(TProp)` is not `IsScalar`. Header literal and string route-param
   reject `null` at the overload (a typed source is required for nullable).
5. **Body presence is data-driven.** `GatherInputDraft.BuildRequestInput` returns
   `RequestInput.None` (`"none"`, *not* `{}`) when there are zero assignments and no
   `IncludeAll`. A bodiless `Delete("/x/{id}")` still substitutes the route param.
   *Good default: `none`, never an empty object.*
6. **One chain per response.** `ResponseRoutingDraft.ContinueWith` throws if a
   follow-up already exists. Default chain is `RequestChain.Terminal` (`"terminal"`).
7. **At least one parallel branch.** `ParallelDraft.ToReaction` throws on zero
   branches. Completion defaults to `ParallelCompletion.None` (`"none"`).
8. **Default body format is JSON.** `_bodyFormat = RequestBodyFormat.Json`.
9. **Default status match is any.** A route added without a status is `AnyStatus`;
   a status route is `ForStatus` with an `HttpResponseStatusCode` validated to 100–599.
10. **Default validation target is none.** `Validate` is opt-in; absent → always sends.
11. **`null` is unrepresentable by construction, not guarded.** Bodiless = the
    `NoRequestInput` *variant*; no chain = the `TerminalRequestChain` *variant*; no
    completion = `NoParallelCompletion` *variant*; no validation = `NoRequestValidationTarget`
    *variant*. There are **no nullable plan fields and no `?? fallback`** on the
    Request nodes — absence is a typed empty/terminal/none node. (Whole-payload reads
    are explicit `WholePayload`/`WholeElement` *variants*, not magic member strings —
    Value slice owns those; Request just nests the `ValueExpression`.)

**Runtime contract (`http.ts`, all async, awaited):**
- `requestCanSend` — if validation is `"container"`, run `validateContainer`; on
  failure **abort, no fetch** (boundary decision, not a fallback).
- `whileLoading` reactions run **before** fetch.
- `resolveRequestInput` → `requestPayloadWriterFor` picks the **query-string** writer
  for GET, else the JSON/FormData body writer; `writeRequestPayloadValue` shapes each
  value **once** (`RuntimeShape.formatForWire`) and writes; `""`→`null` on JSON scalar.
- status routing is **first-match**: an exact-status route wins, else the any-status
  route; a network failure (`response-unavailable`) routes only the **any-status
  error** route and never a success route.
- `finally` runs in a `try/finally` after routing **regardless of outcome** (incl.
  network failure); it has **no response-body access**.
- chain runs **only after success** (`runFollowUpRequest` on `routeSuccess`), in the
  response context (can read the prior success body).
- runtime errors are **boundary-only**: a `null` route param throws
  (`cannot build URL`); an unknown plugin throws in `PluginCatalog`; a missing DOM id
  throws in `getElementById`. No defensive plan validation anywhere.

---

## 4. File Layout

The slice stays where it is today (the redesign keeps the Request folder cohesive).
Author + node files under `Alis.Reactive/`, runtime under `Alis.Reactive.Assets/`.

```
Alis.Reactive/
  Builders/
    PipelineBuilder.Http.cs              # Get/Post/Put/Delete/Parallel entry on PipelineBuilder (Reaction seam → Request)
    Requests/
      HttpRequestBuilder.cs              # §2.1 + ClientValidationBeforeRequest
      GatherBuilder.cs                   # §2.2
      GatherInputDraft.cs                # §2.5
      ResponseBuilder.cs                 # §2.3
      ResponseRoutingDraft.cs            # §2.5
      ParallelBuilder.cs                 # §2.4
      ParallelDraft.cs                   # §2.5
      RequestRouteTemplate.cs            # §2.5 placeholder bijection
      RequestParameterNames.cs           # UrlParameterName + RequestScalarTarget
  PlanModel/
    RequestPlan.cs                       # RequestPlan + RequestEndpoint + RequestReactions + ResponseRouting
                                         #   + RequestChain/Terminal/FollowUp + RequestValidationTarget/No/Container
                                         #   + ResponseRoute + ResponseStatusMatch/Any/Exact
    RequestInput.cs                      # RequestInput base + NoRequestInput
    GatherRequestInput.cs                # GatherRequestInput + RegisteredInputSelection + RequestInputAssignment + RequestInputTarget family
    ReactionGraph.cs                     # (shared) RequestReaction + ParallelReaction + ParallelCompletion family — constructed by Request
    PlanTerms.cs                         # (shared) RequestUrl/HttpMethodName/RequestBodyFormat/HeaderName/RouteParameterName/HttpResponseStatusCode

Alis.Reactive.Assets/runtime/execution/
  http.ts                                # §5.2 pipeline: gather → fetch → route → finally → chain
  gather.ts                              # resolveRequestInput (assignments + registered inputs)
  request-payload-writer.ts             # the ONE writer: query-string | json | form-data, files
  http-fetch.ts                          # resolveFetch (url + init)
```

Acceptance fixtures (§7) live with the slice's tests:
`Alis.Reactive.Assets/runtime/__tests__/http.test.ts` + `gather.test.ts` (runtime),
and the C# domain tests in the Request unit-test file.

---

## 5. Compile-Ready Skeleton

> Fill each `// TODO` by reading the named fixture in §7. The signatures, field
> names, factory names, and `Kind` strings are exact — do not change them; they are
> the contract `plan.ts` and the runtime already agree on.

### 5.1 C# author + node (the lowerer)

```csharp
// HttpRequestBuilder.cs
public class HttpRequestBuilder<TModel> where TModel : class
{
    private readonly PlanBuildContext _context;
    private RequestEndpoint? _endpoint;
    private GatherInputDraft _requestInput = new GatherInputDraft();
    private RequestBodyFormat _bodyFormat = RequestBodyFormat.Json;        // INV-8
    private readonly List<ReactionGraph> _whileLoading = new();
    private readonly List<ReactionGraph> _finally = new();
    private ResponseBuilder<TModel> _response;
    private ClientValidationBeforeRequest? _validation;

    internal HttpRequestBuilder(PlanBuildContext context)
    {
        // TODO: store context (null-throw), seed _response = new ResponseBuilder<TModel>(context).
        //       Fixture: "GET no body → none input".
    }

    public HttpRequestBuilder<TModel> Get(string url)    { /* TODO: SelectEndpoint(GET, url) */ return this; }
    public HttpRequestBuilder<TModel> Post(string url)   { /* TODO */ return this; }
    public HttpRequestBuilder<TModel> Put(string url)    { /* TODO */ return this; }
    public HttpRequestBuilder<TModel> Delete(string url) { /* TODO */ return this; }

    public HttpRequestBuilder<TModel> Gather(Action<GatherBuilder<TModel>> gather)
    {
        // TODO: new GatherInputDraft → new GatherBuilder(context, draft) → invoke → keep draft.
        //       Fixtures: "Gather payload typed component", "Gather header literal", "IncludeAll".
        return this;
    }

    public HttpRequestBuilder<TModel> AsJson()     { _bodyFormat = RequestBodyFormat.Json; return this; }
    public HttpRequestBuilder<TModel> AsFormData() { _bodyFormat = RequestBodyFormat.FormData; return this; }

    public HttpRequestBuilder<TModel> WhileLoading(Action<PipelineBuilder<TModel>> pipeline)
    {
        // TODO: build a PipelineBuilder reaction, Clear() then Add (replace semantics).
        //       Fixture: "WhileLoading runs before fetch".
        return this;
    }

    public HttpRequestBuilder<TModel> Finally(Action<PipelineBuilder<TModel>> pipeline)
    {
        // TODO: same replace semantics as WhileLoading. Fixture: "Finally always runs".
        return this;
    }

    public HttpRequestBuilder<TModel> Validate<TValidationSource>(string formId)
        where TValidationSource : class
    {
        // TODO: _validation = ClientValidationBeforeRequest.Using(typeof(TValidationSource), ComponentId.Of(formId)).
        //       Fixture: "Validate aborts on invalid".
        return this;
    }

    public HttpRequestBuilder<TModel> Response(Action<ResponseBuilder<TModel>> response)
    {
        // TODO: build a fresh ResponseBuilder, invoke, keep it. Fixtures: OnSuccess/OnError/Chained.
        return this;
    }

    internal RequestPlan BuildRequest()
    {
        // TODO: endpoint ?? throw (INV-1); input = _requestInput.BuildRequestInput(_bodyFormat, endpoint.Url);
        //       request = RequestPlan.Create(endpoint, input, RequestReactions.From(_whileLoading, _finally),
        //                                     _response.Draft.BuildRouting(),
        //                                     _validation?.Target ?? RequestValidationTarget.None);
        //       if (_validation != null) _validation.Register(_context, request);  // hands job to Plan sink
        //       Fixture: every Part-B row terminates here.
        throw new NotImplementedException();
    }

    private void SelectEndpoint(HttpMethodName method, string url) =>
        _endpoint = RequestEndpoint.To(method, RequestUrl.Of(url));
}
```

```csharp
// RequestInput.cs / GatherRequestInput.cs / RequestPlan.cs (node shapes — exact Kind strings)
public abstract class RequestInput { internal static RequestInput None { get; } /* NoRequestInput */ }
public sealed class NoRequestInput : RequestInput { public string Kind => "none"; }              // INV-5

internal sealed class GatherRequestInput : RequestInput
{
    public string Kind => "gather";
    public IReadOnlyList<RequestInputAssignment> Assignments { get; }
    public string BodyFormat { get; }                 // "json" | "form-data"
    public RegisteredInputSelection RegisteredInputs { get; }
    // TODO: From(assignments, bodyFormat, registeredInputs). Fixture: "Gather payload typed component".
}

public sealed class TerminalRequestChain : RequestChain { public override string Kind => "terminal"; }        // INV-6 default
public sealed class FollowUpRequestChain : RequestChain { public override string Kind => "follow-up"; public RequestPlan Next { get; } }
public sealed class AnyResponseStatusMatch : ResponseStatusMatch { public override string Kind => "any"; }    // INV-9 default
public sealed class ExactResponseStatusMatch : ResponseStatusMatch { public override string Kind => "status"; public int Status { get; } }
public sealed class NoRequestValidationTarget : RequestValidationTarget { public override string Kind => "none"; } // INV-10 default
public sealed class ContainerRequestValidationTarget : RequestValidationTarget { public override string Kind => "container"; public string Container { get; } }

// ReactionGraph.cs — constructed by Request via ReactionGraph.Request(...) / ReactionGraph.Parallel(...)
public sealed class RequestReaction : ReactionGraph { public string Kind => "request"; public RequestPlan Request { get; } /* drop `new` — collision fix */ }
public sealed class ParallelReaction : ReactionGraph { public string Kind => "parallel"; public IReadOnlyList<ReactionGraph> Steps { get; } public ParallelCompletion Completion { get; } }
public sealed class NoParallelCompletion : ParallelCompletion { public override string Kind => "none"; }       // INV-7 default
public sealed class SettledParallelCompletion : ParallelCompletion { public override string Kind => "on-settled"; public ReactionGraph Reaction { get; } }
```

```csharp
// GatherInputDraft.cs — the assignment accumulator (INV-5)
internal sealed class GatherInputDraft
{
    private readonly List<RequestInputAssignment> _assignments = new();
    internal RegisteredInputSelection RegisteredInputs { get; private set; } = RegisteredInputSelection.ExplicitAssignments;

    internal RequestInput BuildRequestInput(RequestBodyFormat bodyFormat, RequestUrl url)
    {
        // TODO: RequestRouteTemplate.For(url).RequireRouteParameters(RouteParameterNames());  // INV-3
        //       if no assignments AND not selectsRegisteredInputs → return RequestInput.None;  // INV-5
        //       else GatherRequestInput.From(_assignments, bodyFormat, RegisteredInputs).
        //       Fixtures: "GET no body → none input", "URL placeholder requires route param".
        throw new NotImplementedException();
    }

    internal void IncludeAllRegisteredInputs() { /* TODO: RegisteredInputs = AllRegisteredInputs. Fixture: "IncludeAll". */ }
    internal void AddAssignment(RequestInputAssignment a) => _assignments.Add(a);
    internal void AddPayload(BindingPath p, ValueExpression v) => _assignments.Add(RequestInputAssignment.Payload(p, v));
    internal void AddHeader(HeaderName n, ValueExpression v) => _assignments.Add(RequestInputAssignment.Header(n, v));
    internal void AddRouteParameter(RouteParameterName n, ValueExpression v) => _assignments.Add(RequestInputAssignment.RouteParameter(n, v));
    private IEnumerable<string> RouteParameterNames() { /* TODO: yield each RequestRouteParameterTarget.Name */ yield break; }
}
```

### 5.2 TS runtime (the reader — existing module, keep behavior)

```ts
// http.ts — the one async pipeline: gather → fetch → route → finally → chain
export async function executeRequest(req: RequestPlan, plan: PlanDocument, ctx?: ExecContext): Promise<void> {
  // TODO: await runHttpRequest(req, plan, ExecutionContext.from(ctx)). Fixture: "GET fires fetch".
}

async function runHttpRequest(request: RequestPlan, plan: PlanDocument, context: ExecutionContext): Promise<void> {
  // TODO: if (!requestCanSend(...)) return;                              // INV-10 / "Validate aborts"
  //       await runRequestReactions(request.whileLoading, ...);          // "WhileLoading before fetch"
  //       const prepared = prepareHttpRequest(...);                      // gather + ResolvedFetch
  //       const outcome = await sendHttpRequest(request, prepared.fetch);
  //       await routeExchangeOutcome(outcome, request, plan, prepared.context);
}

// switch on outcome.kind: "success" → routeSuccess (+ chain), "error" → routeError,
// "response-unavailable" → routeResponseUnavailable; default assertNever(outcome, ...).
// routeAndComplete wraps the route stage in try/finally running request.finally.   // "Finally always runs"
// routeResponseRoutes: first exact-status match, else any-status.                   // "First-match status routing"
// runFollowUpRequest: chain.kind "terminal" → return; "follow-up" → runHttpRequest(chain.next). // "Chain after success"
```

```ts
// gather.ts — resolve assignments + registered inputs through the Value spine
export function resolveRequestInput(input: RequestInput, method: HttpMethod, plan: PlanDocument, ctx: ExecContext): ResolvedRequestInput {
  // TODO: switch input.kind: "none" → emptyRequestInput(); "gather" → resolveGatherRequestInput(...);
  //       default assertNever(input, "request input"). Fixture: "GET no body → none input".
}
// resolveGatherRequestInput: writer = requestPayloadWriterFor(reqInput, input.bodyFormat, method);
//   write each assignment (payload via writeRequestPayloadValue; header/route via scalar wire);
//   then writeRuntimeSelectedInputs (IncludeAll iterates mounted registered inputs).
```

---

## 6. Determinism / Default Notes (carry into review)

- The whole request graph is decided at authoring; the runtime walks a *fixed*
  pipeline with first-match routing and the single documented async lane.
- Good defaults are typed-empty variants, never sentinels: `none` input, `terminal`
  chain, `none`/`on-settled` completion, `none` validation, `any` status, `json` body.
- The redesign folds the 7-scope `PayloadScope` to scopes that carry data (drops the
  dead `local`) and consolidates all FormData/File knowledge into the **one**
  `RequestPayloadWriter` — verify no vendor `{rawFile}` knowledge leaks elsewhere.

---

## 7. Acceptance Fixtures (matrix cases by name)

From [`04-matrix-http-arrays-values.md`](../04-matrix-http-arrays-values.md) Part B.
Each row below is a fixture the slice must satisfy. Prove C# lowering with a domain
test (the DSL call → the exact node) and runtime behavior with a vitest in
`http.test.ts` / `gather.test.ts`; Playwright proves the page-visible ones.

**B.1 Verbs**
1. `GET` — `p.Get("/api/x")` → `method:"GET"`, query-string egress, no body.
2. `POST / PUT / DELETE` — body writer; body only when fields present (`bodyHasFields`).
3. `Inline gather overload` — `p.Post(url, g => ...)` ≡ `.Post(url).Gather(...)` (sugar).

**B.2 Endpoint + URL template**
4. `URL with {placeholder}` — `RequireRouteParameters` bijection; missing param throws at authoring.

**B.3 Gather (`target <- value`)**
5. `Gather payload — typed component (by expression)` — `g.Include<TComp,TModel>(m => m.Name)`.
6. `Gather payload — typed component member source` — `g.Include(schedule.CurrentView())`.
7. `Gather payload — static literal` — `g.Static("token", "abc")`.
8. `Gather payload — from event arg` — `g.FromEvent(args, e => e.Id, "id")`.
9. `Gather payload — from URL query` — `g.FromUrl("page")` / `g.FromUrl<int>("page","p")`.
10. `Gather payload — plugin read` — `g.Plugin(pluginSource, "name")`.
11. `Gather header — literal` — `g.Header("X-Key", "v")` (non-null).
12. `Gather header — typed source / event arg` — scalar-enforced; missing → header omitted.
13. `Gather route-param — static (int/long/string)` — `g.RouteParam("id", 5)`.
14. `Gather route-param — typed source / event arg` — scalar-enforced; null → throws (`cannot build URL`).
15. `Gather — include all registered inputs` — `g.IncludeAll()` → only mounted inputs.
16. `No gather (bodiless)` — `p.Delete("/x/{id}")` route-param only → `RequestInput.None` (`"none"`).

**B.4 Body egress (the writer)**
17. `Scalar → JSON body` — SHAPE-ONCE `formatForWire`; `""`→`null`.
18. `Array → JSON body` — reject `File` items; shape each by item shape.
19. `Scalar/array → query string (GET)` — `encodeURIComponent`; arrays repeat the key; `File` in GET throws.
20. `Scalar/array/File → form-data` — `AsFormData`; files keep filename.

**B.5 Response routes**
21. `OnSuccess (untyped)` — any-status success route on `response.ok`.
22. `OnSuccess<TResponse> (typed body)` — success scope `PayloadContract.ForPayload(R)`; `json.Read`.
23. `OnError (any / by status / typed)` — exact-status preferred, else any; first-match.
24. `Response unavailable (network failure)` — only any-status error route; never success.

**B.6 Loading / finally / validate**
25. `WhileLoading` — runs **before** fetch.
26. `Finally` — runs after routing in `try/finally`, regardless of outcome; no body access.
27. `Validate<TSource>(formId)` — invalid → abort, no fetch.

**B.7 Chained & parallel**
28. `Chained` — `r.Chained(...)`; runs only after success, in the response context; one follow-up only.
29. `Parallel` — `p.Parallel(b1, b2).OnAllSettled(...)`; branches concurrent (`Promise.all`); completion after all settle; ≥1 branch required.

> **Coverage gate.** All 28 Part-B rows (29 fixtures with parallel) map to a named
> test before the slice is declared done. The three documented runtime-environment
> boundaries — array-source normalization, response body presence/content-type, and
> the `IncludeAll` mounted-input set — are external edges, not Request plan
> non-determinism; do not write plan information to "fix" them.
