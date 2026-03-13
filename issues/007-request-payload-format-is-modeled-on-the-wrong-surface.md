# Request Payload Format Is Modeled On The Wrong Surface

## Verdict

This is a legitimate open issue.

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
