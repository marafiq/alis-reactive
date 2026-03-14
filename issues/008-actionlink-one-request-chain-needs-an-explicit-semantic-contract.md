# ActionLink Defines One Complete Request Unit

## Verdict

This document is the design specification for `ActionLink`.

## Problem

Many screens have repeated row-level actions in grids, lists, and tables.

In those screens, the interaction shape is usually identical for every row:

- the user activates a link
- one complete request unit is executed
- only concrete per-row values differ, such as URL and declared request inputs
- the response is handled through the same bounded success and error semantics every time

That is a strong use case for a native vertical slice.

The framework does not require every repeated link action to become a full generic plan entry when the author wants a link-shaped component with one fixed complete-request behavior.

## The Core Direction

`ActionLink` is a native component vertical slice that supports an `Html.ActionLink` builder-style authoring experience and renders a normal anchor enriched with `data-reactive-*` attributes.

The feature reuses existing request-unit semantics that already exist in the framework, and it does not expose the full generic HTTP pipeline on the element.

This must stay:

- one element
- one activation
- one complete request unit
- one declared gather/input lane
- one typed success payload lane
- one bounded success lane
- one bounded error lane
- one request execution truth

It must not become:

- a generic per-element reactive DSL
- a broader workflow surface than this slice needs
- a client-side route-construction mechanism
- a multi-request workflow surface

## Critical Constraint: MVC Still Owns URL Resolution

This point is fundamental.

`ActionLink` keeps route resolution in MVC, not in the browser.

That means:

- if the author uses a direct URL, the builder renders that final URL
- if the author uses action/controller/route values, MVC resolves them during render
- the browser receives only concrete `href` and concrete `data-reactive-*` values

The runtime executes concrete request metadata. It does not reconstruct MVC semantics from action names, controller names, or route templates.

This aligns with the existing framework contract:

- `ReactivePlan.Render()` serializes a concrete JSON plan in `Alis.Reactive/IReactivePlan.cs`
- `RequestDescriptor.Url` is already just a final string in `Alis.Reactive/Descriptors/Requests/RequestDescriptor.cs`
- `GatherBuilder<TModel>` already models request inputs on the C# side in `Alis.Reactive/Builders/Requests/GatherBuilder.cs`
- `resolveGather(...)` executes against already-serialized gather items in `Alis.Reactive.SandboxApp/Scripts/gather.ts`
- `ResponseBuilder.OnSuccess<TResponse>(...)` already models a typed success payload lane in `Alis.Reactive/Builders/Requests/ResponseBuilder.cs`
- `ResponseBody<T>` already carries compile-time response typing in `Alis.Reactive/ResponseBody.cs`

That discipline matters because it preserves long-term cacheability. If the rendered contract remains a pure product of C# render-time data, the framework can later cache that output much more safely than a design that requires the browser to synthesize request metadata.

## Public Shape

The public shape follows an existing native builder, not a generic pipeline builder.

Example:

```csharp
@Html.ActionLink("Next", "/orders/page/2")
    .CssClass("paging-link")
    .Post()
    .Gather(g => g.IncludeAll().Static("page", 2))
    .Into("orders-grid")
```

Surface:

```csharp
public static ActionLinkBuilder<TModel> ActionLink<TModel>(
    this IHtmlHelper<TModel> html,
    string linkText,
    string url)
    where TModel : class;

public sealed class ActionLinkBuilder<TModel> : IHtmlContent
    where TModel : class
{
    public ActionLinkBuilder<TModel> CssClass(string css);
    public ActionLinkBuilder<TModel> Attr(string name, string value);

    public ActionLinkBuilder<TModel> Get();
    public ActionLinkBuilder<TModel> Post();
    public ActionLinkBuilder<TModel> Put();
    public ActionLinkBuilder<TModel> Delete();

    public ActionLinkBuilder<TModel> Gather(Action<GatherBuilder<TModel>> configure);
    public ActionLinkBuilder<TModel> AsFormData();
    public ActionLinkBuilder<TModel> Validate(string formId);

    public ActionLinkBuilder<TModel> Into(string targetId);
    public ActionLinkBuilder<TModel> OnSuccess<TResponse>(
        Action<ResponseBody<TResponse>, ActionLinkSuccessBuilder<TModel>> configure)
        where TResponse : class, new();
    public ActionLinkBuilder<TModel> OnSuccessDispatch(string eventName);
    public ActionLinkBuilder<TModel> OnErrorDispatch(string eventName);
}
```

There is one shape only in this slice: `ActionLink` receives the final URL it renders and executes.

If a team uses MVC action/controller/route generation, that resolution happens before this builder receives the URL. That remains a normal server concern, not part of the `ActionLink` contract and not part of the browser contract.

## What Must Be Deliberately Excluded

This slice is intentionally smaller than `HttpRequestBuilder<TModel>` and `PipelineBuilder<TModel>`.

It does not support:

- chained requests
- parallel requests
- conditional branches
- arbitrary command lists
- generic nested `Response(...)` workflow trees
- client-side route generation

If those capabilities leak in, `ActionLink` stops being a clean native slice and starts duplicating responsibilities that already belong to the generic reactive HTTP surface.

## Why This Stays SOLID

### Single Responsibility

`ActionLink` models one thing only: a native anchor that executes one complete request unit.

### Open/Closed

The existing request engine remains the source of truth for request execution. The new feature adds a narrow adapter over existing behavior rather than changing unrelated workflow concepts.

### Liskov Substitution

The builder still renders valid anchor markup and behaves like a bounded enhancement of a link, not a fundamentally different concept hidden behind an `<a>`.

### Interface Segregation

Authors get a purpose-built link API rather than the full generic request/pipeline API.

### Dependency Inversion

The builder emits a small declarative attribute contract, and the runtime adapter depends on the existing request executor instead of embedding fetch logic in the component slice.

## Runtime Shape

The runtime remains minimal.

The runtime shape is:

- one document-level delegated click handler
- one narrow attribute parser
- one shared request-unit execution entry point
- reuse of the existing request engine

This avoids per-element boot complexity and keeps `ActionLink` on the same fetch and request execution path the framework already trusts.

This also matches the existing runtime style:

- `auto-boot.ts` already performs one-time runtime initialization on page load
- `inject.ts` already injects partial HTML without per-element event registration infrastructure
- `boot.ts` and trigger wiring are reserved for plan-backed entries, not for this native attribute slice

`ActionLink` therefore adds one startup-time delegated listener, not one listener per rendered anchor.

### Runtime flow

```mermaid
flowchart LR
  razor[RazorView] --> builder[ActionLinkBuilder]
  builder --> mvc[MVCResolvesUrl]
  mvc --> html[AnchorWithConcreteDataReactiveAttrs]
  html --> boot[auto-boot.ts]
  boot --> delegate[action-link.ts]
  delegate --> parse[ParseAttrsToRequestUnit]
  parse --> gate[SharedValidationGate]
  gate --> exec[execRequest]
  exec --> handle[BoundedSuccessOrErrorHandling]
  handle --> dom[DOMUpdateOrNoOp]
```

## Delegated Activation

The runtime uses exactly one delegated click listener for `a[data-reactive-link]` at the document level.

The capture contract is:

1. Runtime startup installs the listener once from `auto-boot.ts`.
2. A click bubbles from the DOM target to that listener.
3. The listener resolves the nearest owning anchor with `closest('a[data-reactive-link]')`.
4. If no such anchor exists, the listener does nothing.
5. If such an anchor exists, the listener reads the concrete `data-reactive-*` attributes from that anchor.
6. The listener prevents native navigation for that reactive case.
7. The listener builds the bounded complete-request-unit shape.
8. The listener runs the shared gather, validation, request, and success/error path.

As a result:

- initial page render works
- injected partial content works
- rerendered row content works
- large repeated grids work without per-element wiring
- 100 rendered `ActionLink` rows still use one click listener, not 100 listeners

`ActionLink` does not participate in the plan boot/merge lifecycle for its base behavior.

`MutationObserver` is not part of this design. The repeated-row use case is served by delegated activation.

## Concrete Runtime Reuse

The implementation reuses what already exists:

- `execRequest(...)` in `Alis.Reactive.SandboxApp/Scripts/http.ts` remains the request executor
- the validation gate pattern in `Alis.Reactive.SandboxApp/Scripts/pipeline.ts` becomes a shared request-unit entry point
- gather semantics stay consistent with `Alis.Reactive.SandboxApp/Scripts/gather.ts`
- typed success payload semantics stay consistent with `ResponseBuilder.OnSuccess<TResponse>(...)` and `ResponseBody<T>`
- bounded success/error handling reuses existing command paths where possible rather than creating a new response engine

The implementation does not create:

- another fetch path
- another validation path
- another gather path
- another DOM injection path

## Required File-Level Changes

### C# slice

- add `Alis.Reactive.Native/Components/NativeActionLink/NativeActionLink.cs`
- add `Alis.Reactive.Native/Components/NativeActionLink/ActionLinkBuilder.cs`
- add `Alis.Reactive.Native/Components/NativeActionLink/ActionLinkSuccessBuilder.cs`
- add a small internal request-unit/options model for the slice
- add `Html.ActionLink(...)` overloads returning `ActionLinkBuilder<TModel>`

### TypeScript runtime

- add `Alis.Reactive.SandboxApp/Scripts/action-link.ts`
- modify `Alis.Reactive.SandboxApp/Scripts/auto-boot.ts` to initialize the delegated handler once for the entire page
- refactor `Alis.Reactive.SandboxApp/Scripts/pipeline.ts` so plan-backed HTTP and `ActionLink` share the same validation-plus-request entry point

### Excluded Dependencies

- no dependency on `planId`
- no dependency on `mergePlan(...)`
- no dependency on partial-plan re-enrichment
- no browser-side MVC route interpretation

## Attribute Contract Principles

The attribute contract must be explicit and closed.

Good fields:

- marker attribute for the slice
- final URL
- HTTP verb
- serialized gather descriptors
- validation form id
- payload format
- bounded success descriptors
- bounded error descriptors

Bad fields:

- action name
- controller name
- route template fragments
- anything that requires the browser to understand MVC routing concepts
- anything that turns attributes into a generic workflow language

## Resolution Criteria

This specification is only satisfied when there is proof for all of the following:

- `Html.ActionLink`-style authoring works as a native builder
- the rendered output contains only concrete server-resolved values
- the runtime executes one complete request unit only
- declared gather inputs outside the anchor participate in that request unit correctly
- typed success payload access is available inside the bounded success lane
- repeated grid/list row actions work without per-element complexity
- 100 rendered `ActionLink` rows still result in one delegated document listener
- injected partial content behaves correctly
- validation and bounded response handling stay on the existing request path rather than branching into a separate execution truth

The key proof is not just that attributes can be rendered. The key proof is that `ActionLink` remains a narrow, deterministic, cache-friendly native slice that fits the framework’s existing “C# builds the contract, runtime executes the contract” architecture.
