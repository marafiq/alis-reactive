# Request Payload Format Is Modeled On The Wrong Surface

## Verdict

**Rejected.** The current placement is architecturally correct. The issue confuses data selection with transport encoding and draws a false analogy to the response side.

## Why This Is Legit

The current codebase already distinguishes two different concerns:

- request payload shaping
- response body access

The problem is that those two lanes are not exposed with equally truthful API placement.

Request payload format is currently configured on `HttpRequestBuilder` through `AsJson()` and `AsFormData()`, but the actual JSON-versus-`FormData` decision is made later inside gather resolution. By contrast, response body path handling is already modeled on the response lane through typed success handlers and `ResponseBody<T>`.

So the runtime contract is already split correctly, but the request-side DSL surface places payload format on the outer request builder instead of on the body-shaping surface that actually consumes it.

## Proven By The Current Code

- `HttpRequestBuilder` owns `_contentType` and exposes `AsJson()` / `AsFormData()` in `Alis.Reactive/Builders/Requests/HttpRequestBuilder.cs`.
- That value is lowered into `RequestDescriptor.ContentType` in `Alis.Reactive/Descriptors/Requests/RequestDescriptor.cs`.
- `RequestDescriptor.ContentType` is consumed by `resolveGather(...)` through `execRequest(...)` in `Alis.Reactive.SandboxApp/Scripts/http.ts`.
- `Alis.Reactive.SandboxApp/Scripts/gather.ts` is where the actual payload decision happens:
  it chooses between nested JSON body and `FormData` based on `contentType`.
- `GatherBuilder<TModel>` is the surface that actually defines what request data exists in the first place, in `Alis.Reactive/Builders/Requests/GatherBuilder.cs`.

On the response side, the code is already separated cleanly:

- `ResponseBuilder<TModel>.OnSuccess<TResponse>(...)` is the typed response lane in `Alis.Reactive/Builders/Requests/ResponseBuilder.cs`.
- `ResponseBody<T>` exists only to model typed success payload access in `Alis.Reactive/ResponseBody.cs`.
- `ExpressionPathHelper` lowers those response paths to `responseBody.*`, which is a different concept from request payload formatting.

That means the current architecture already knows these are separate concepts. The mismatch is where the request payload format is modeled.

## Why This Matters

This is not just naming polish.

Putting payload format on `HttpRequestBuilder` makes the DSL suggest that body format is a generic request concern at the same level as validation, loading state, and response routing. The runtime does not actually treat it that way. It treats payload format as part of gather/body shaping.

Over time that makes the HTTP DSL easier to misunderstand:

- request payload shaping looks mixed into outer request orchestration
- response path handling looks like another similarly placed concern even though it belongs to a different lane
- analyzer or ordering rules become harder to define because the public chain does not reflect the real execution boundary

## Why This Is Strong

- It is based on the actual lowering path, not on preferred style.
- It does not depend on speculative runtime behavior.
- It explains a real architectural mismatch between public surface and execution surface.
- It keeps request payload formatting separate from response path handling instead of conflating them.

## Suggested Fix Direction

The truthful direction is:

- keep response path handling on the response lane
- move request payload format closer to `GatherBuilder<TModel>` or an equivalent body-shaping surface

If the DSL remains frozen, this should be treated as design debt and documented clearly rather than hidden behind analyzer rules that imply the current placement is ideal.

If DSL changes are allowed with strong reasoning, request body format should be declared where request body shape is actually defined.

## Required Proof

This issue should only be considered resolved with proof that:

- request payload formatting is modeled on the same lane that defines gathered request data
- response body access remains modeled through `ResponseBuilder<TModel>` and `ResponseBody<T>`
- the DSL no longer suggests that payload content-type and response-path access are sibling concerns on the same surface when they are not
- existing sandbox examples still demonstrate both lanes clearly:
  one request payload formatting example and one typed response-body access example

---

## Response: Why This Issue Is Wrong

**Status: Rejected — no code changes.**

The issue builds a superficially coherent argument but rests on three fundamental errors: it confuses data selection with transport encoding, draws a false analogy to the response side, and uses runtime data flow as evidence for DSL placement when it proves nothing of the sort.

### Error 1: Content-Type is a transport concern, not a data selection concern

`Content-Type` is literally an HTTP header (`Content-Type: application/json` or `Content-Type: multipart/form-data`). It tells the server how to deserialize the bytes on the wire. It is orthogonal to what data is being sent.

`GatherBuilder` answers: **what data do I collect?** (Include, IncludeAll, Static)
`AsFormData()` answers: **how do I encode that data for transport?**

These are separate responsibilities. The current DSL keeps them separated:

```csharp
// Current — clean: gather = data selection, AsFormData = encoding
p.Post("/url", g => g.Include(m => m.Name)).AsFormData()
```

The proposed fix would mix them:

```csharp
// Proposed — mixed: gather lambda handles both data AND encoding
p.Post("/url", g => g.Include(m => m.Name).AsFormData())
```

This violates the Single Responsibility Principle. The gather lambda becomes responsible for two unrelated concerns: which fields to include, and how to serialize them. That is objectively worse separation than the current design.

### Error 2: The response analogy is false

The issue claims a symmetry: "response body access is on the response surface, so request body format should be on the request body surface."

But the two things being compared are not analogous:

| Concern | What it does | Category |
|---------|-------------|----------|
| `ResponseBody<T>` | Typed access to response **data fields** | Data access |
| `AsFormData()` | Encoding **format** for request transport | Transport encoding |

`ResponseBody<T>` is about **what's in the response** — it provides typed paths like `r => r.Data.Name`. The correct request-side analogy would be `GatherBuilder.Include(m => m.Name)` — which already IS on the gather surface. Both are data selection.

`AsFormData()` is about **how the data is serialized** — JSON vs multipart. There is no response-side equivalent because the server decides response encoding, not the client. If there were a `.ExpectJson()` on `ResponseBuilder`, it would also feel wrong — because encoding format belongs at the transport level, not the data level.

The issue conflates "what data" with "how encoded" and then draws a symmetry that only holds if you ignore the distinction.

### Error 3: Runtime data flow does not prove DSL placement

The issue argues: "`contentType` is consumed by `resolveGather()`, therefore it should be declared on the gather surface."

By this exact logic, `verb` should also be on the gather surface — because `resolveGather()` also receives `verb` to decide between URL params and request body:

```typescript
export function resolveGather(
  items: GatherItem[],
  verb: string,          // ← also consumed by resolveGather
  components: Record<string, ComponentEntry>,
  contentType?: string   // ← also consumed by resolveGather
): GatherResult
```

Nobody would argue that `GET`/`POST`/`PUT`/`DELETE` should be declared inside the gather lambda. A function receiving a parameter doesn't mean that parameter should be declared at the call site. `resolveGather()` needs both `verb` and `contentType` to encode correctly — that's normal parameterization, not evidence of misplacement.

### Error 4: Gather can be absent

The current DSL allows:

```csharp
p.Post("/url").AsFormData()  // No gather, but encoding still matters
```

If `AsFormData()` lived on `GatherBuilder`, this pattern would require:

```csharp
p.Post("/url", g => g.AsFormData())  // Empty gather lambda just to set encoding
```

A gather lambda whose only purpose is to set encoding format is semantically wrong. It forces a data-selection surface to exist when there is no data to select.

### Error 5: Every HTTP framework agrees

- **Fetch API:** `init.headers = { "Content-Type": "..." }` — request level
- **HttpClient (.NET):** `content.Headers.ContentType` — request level
- **axios:** `{ headers: { "Content-Type": "..." } }` — request level

Content-Type is universally modeled as a request property, not a body-builder property. The codebase follows established convention.

### Error 6: Scale does not justify the change

Exactly 1 call site across the entire codebase uses `.AsFormData()`:

```
Alis.Reactive.SandboxApp/Areas/Sandbox/Views/IdGenerator/Index.cshtml:114
```

One call site is not causing confusion. There is no pattern of misuse, no developer complaints, and no analyzer ambiguity. The issue describes a hypothetical future problem that has not materialized.

### What the issue gets right

The observation that `resolveGather()` is where encoding happens is correct. The observation that request and response are different lanes is correct. But the conclusion — that encoding format should be declared on the data-selection surface — does not follow from those observations.

### Summary

| Issue claim | Reality |
|-------------|---------|
| "Payload format is a body-shaping concern" | It's a transport encoding concern — HTTP Content-Type header |
| "Response side proves the pattern" | ResponseBody is data access, not encoding — false analogy |
| "resolveGather consumes it, so declare it there" | resolveGather also consumes verb — same argument would put verb on gather |
| "Current placement mixes concerns" | Moving it to gather would mix data selection + encoding in one lambda |
| "DSL becomes easier to misunderstand" | 1 call site, zero evidence of confusion |

The current placement of `AsJson()`/`AsFormData()` on `HttpRequestBuilder` is correct. No changes required.
