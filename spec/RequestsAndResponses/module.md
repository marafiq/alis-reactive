# Requests and responses

A request and what comes back: the typed request, its method, URL, query, headers and body, and what it sends; its
flow, from the check through `WhileLoading`, the send and its retries, one route, `Finally`, `Chained` and
`Parallel`; and the reply, its status, its headers and its body as the route's kind decodes it, with the error body
each host sends. Module 6 of `spec/CLAUDE.md`.

## Core questions

- Q2: a sent key is the member's binding name, and a JSON body's member is named as Q2 names it (D9.6, D9.7).
- Q3: a number and a date reach each part in the form its host binds (D3.7, D3.8, D9.6), and a reply's text and HTML
  are decoded by their charset (D10.6).
- Q4: a path value that reads nothing stops its request before it is sent (D9.5), and a stop in a route ends its
  pipeline after `Finally` (D10.3).
- Q5: a reply is a typed object whose status, headers and body are members (D10.19).
- Q7: every part a request sends takes a literal or any source, a response's body included (D9.15, N10.7).
- Q8: a condition works in every block of a request, and a part, a chained request or a parallel branch takes one
  (D10.16).
- Q9: the request: its methods, its URL and placeholders, what each body format carries, what it sends and how each
  value reaches the endpoint on both hosts (D9.1 to D9.23).
- Q10: the flow and the response: one lifecycle (D10.1, D10.23), closed outcomes and body kinds (D10.5, D10.6), the
  error body (D10.8), `Chained` and `Parallel` (D10.9, D10.10), defaults and retry (D10.12 to D10.15), the busy
  indicator and the notice (D10.21).
- Q11: a request is a statement of its run, whose driver alone touches a promise (D10.23, D11.11, D11.12).
- Q12: `Into` loads a slot, and an owner's dispose ends a request in flight with its `Finally` layers (D10.11,
  D10.3).
- Q16: a request's check reads the body it sends, and the server's verdict shows as the browser's does (D10.2,
  D10.8, D16.13).
- Q18: what an endpoint binds is read at build, with no run-time reflection (D9.20).

## Boundaries

A validator, its rules and its rule sets are module 8's, and so are `ValidationResult` and `ValidationFailure`; the
request's `Validate` names the validator and is here (D16.2). What `Into` does to the slot it loads is module 9's
(Q12). The page's URL as a source is module 2's (N10.7's `PageUrl<TPage>()`), and a file the person chose, N10.7's
`BrowserFile`, module 15's (D3.1, N3.2). Native's `NativeGatherExtensions` and
the action link's `NativeActionLinkRequest<TModel>` are modules 24 and 25's, which onboard on these doors (D17.15).

## Doors

Module 6 adds no door of its own: Q9 and Q10 give every capability its door, N10.7 the surface. Beside N10.7 it keeps
`HttpSend` and its parts (N9.1) with the wire forms `enum` and `flags` (N9.8), `HttpResponse` (N10.2), `Query<TQuery>`
on POST, PUT and DELETE (N9.7), a condition on a part, a chained request and a parallel branch (N10.4), and the
condensed request (N10.5); ALIS029 (N9.4) and `RequestKeysSuppressor` (N9.5) are its analyzers, whose code is module
20's.

- The verbs: main's `Get`, `Post`, `Put` and `Delete` on `PipelineBuilder<TModel>` and on `HttpRequestBuilder<TModel>`
  inside `Chained` and `Parallel`, and the redesign's `RequestExtensions` and `RequestStart<TModel>`, go to N10.7's
  instance verbs on `Pipeline<TModel>`, with `Head` and `Options` beside them (D9.3).
- What a request sends: main's `GatherBuilder<TModel>` and `GatherExtensions`, the redesign's
  `GatherBuilder<TRequest, TModel>`, and both lines' `Gather` go to N10.7's request lambda: `Path`, `Query`, `Header`,
  `Set`, one body door, `Form(form)`, `Inputs(container)` and `Input(r => r.Member, input)`. Refused, each with its
  carriers in `spec/RequestsAndResponses/features.json`: main's `Static`, `RouteParam`, `FromUrl`, `Plugin<T>` and
  `Include(refId, name)`, whose strings name members; main's `FromEvent` and the payload forms of `Header` and
  `RouteParam`, main's second syntax for a source; and main's `IncludeAll` and the redesign's `Form()`, which name no
  form or container they send (D9.15). The redesign's `Form(params Expression<Func<TModel, object?>>[])` goes to
  `Input` (N9.3), and its `Form(formId)` to `Form(form)` over the form's typed handle (N18.3).
- The body format: both lines' `AsJson` and `AsFormData` go to the body doors `Json`, `UrlEncoded`, `Multipart`,
  `Text` and `File` (D9.21), and the condensed body is multipart (D10.22).
- The routes: both lines' `Response(r => ..)` and `ResponseBuilder<TModel>` go to the routes on the request (N10.7):
  `OnSuccess(Content.<kind>, ..)`, `OnSuccess<T>`, `OnError(status, Content.<kind>, ..)`, `OnError<T>(status, ..)` and
  the catch-all `OnError((failed, pipeline) => ..)` over `Failure`. Main's `ResponseBody<TResponse>` and the redesign's
  `Data<TResponse>` as a route's body go to `Response<TRequest, TBody>.Body` (N10.7), read with `Read` (N5.6).
- `Chained`: main's `Chained(Action<HttpRequestBuilder<TModel>>)` and the redesign's two `Chained` overloads go to
  `Chained((reply, next) => ..)`, a pipeline that receives the typed reply (D10.9, D10.16).
- `Parallel`: main's `Parallel(params Action<HttpRequestBuilder<TModel>>[])`, the redesign's `Parallel` over
  `RequestStart<TModel>` and both lines' `OnAllSettled` go to `Parallel(params Action<Pipeline<TModel>>[] branches)`,
  each branch a pipeline and the steps after it the all-settled block (D10.10).
- Kept: `WhileLoading`, `Finally` and `Into(elementId)` (D10.3, D10.11). Both lines'
  `Validate<TValidationSource>(formId)` goes to `Validate<TValidator>()` over the body the request sends (D16.1,
  D16.2), and both lines' `ValidationErrors(formId)` to `ShowErrors(problem)` (D16.13).
- Added on the evidence of both lines: `ValidationResult.ToErrorResponse()` on each host (N10.1), the net48 JSON
  result door (N10.6), the defaults (N9.6, N10.3), `Busy`, `NoBusy`, `NoDefaults`, `Download` and `Notice` (N10.7), and
  net48's `[ValidateAntiForgeryHeader]` (D9.22).

## Improvements

- A request names what it sends. Main's `IncludeAll` sends every input registered on the page
  (`Alis.Reactive/PlanAuthoring/Requests/GatherBuilder.cs:31-35` at 623dd4a27), and the redesign's `Form()` the
  inputs of the form the request is taken to check (`Alis.Reactive/PlanAuthoring/Requests/GatherBuilder.cs:129` at
  65fdf118d);
  `Form(form)`, `Inputs(container)` and `Input` each name it, on a page with many forms or none (D9.15).
- A sent member is typed by the request class. Main names it by a string and takes its value as an object
  (`Alis.Reactive/PlanAuthoring/Requests/GatherBuilder.cs:40` at 623dd4a27), a placeholder by a string
  (`Alis.Reactive/PlanAuthoring/Requests/GatherBuilder.cs:117` at 623dd4a27) and a page query key by a string
  (`Alis.Reactive/PlanAuthoring/Requests/GatherBuilder.cs:185` at 623dd4a27); `Path`, `Query` and `Set` take a member
  lambda over the request's type, and a placeholder no member fills is refused (D9.4).
- The outcomes are closed, and each runs exactly one route (D10.5). The redesign reads a network failure and a body
  that does not parse as status 0 (`Alis.Reactive.Assets/runtime/plugins/http.ts:35-40` and
  `Alis.Reactive.Assets/runtime/plugins/http.ts:50-56` at 65fdf118d), and main as a response it calls unavailable
  (`Alis.Reactive.Assets/runtime/execution/requests/http.ts:113-121` at 623dd4a27); a redirect to another origin and a
  body not of the declared kind are told apart by `Failure.Outcome`.
- A route declares its body kind (D10.6). Both lines pick the kind from the response's media type in the browser
  (`Alis.Reactive.Assets/runtime/plugins/http.ts:160-168` at 65fdf118d,
  `Alis.Reactive.Assets/runtime/execution/requests/http.ts:256-266` at 623dd4a27), so a login page that answers 200
  after a redirect reaches the success route as text, and both decode every text as UTF-8 through `text()`
  (`Alis.Reactive.Assets/runtime/plugins/http.ts:165` at 65fdf118d), whatever its charset.
- `Chained` and every `Parallel` branch are pipelines (D10.16). Main's `Chained` takes one request builder
  (`Alis.Reactive/PlanAuthoring/Requests/ResponseBuilder.cs:121` at 623dd4a27), the redesign's one request
  (`Alis.Reactive/PlanAuthoring/Requests/ResponseBuilder.cs:116` at 65fdf118d), and a `Parallel` branch is one request
  on both lines (`Alis.Reactive/PlanAuthoring/Pipelines/PipelineBuilder.Http.cs:53` at 623dd4a27,
  `Alis.Reactive/PlanAuthoring/Requests/RequestExtensions.cs:42` at 65fdf118d), so a branch holds no
  condition and no second step.
- Defaults, retry, the busy indicator and the notice are configured once (D10.12, D10.14, D10.21). Every request of
  both lines shows and hides its own spinner in its own steps
  (`Alis.Reactive.SandboxApp/Areas/Sandbox/Views/HttpPipeline/Http/Index.cshtml:11` at 623dd4a27,
  `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Brought/VisitPlanner/FrontDesk.cshtml:35` and
  `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Brought/VisitPlanner/FrontDesk.cshtml:62` at 65fdf118d),
  and neither retries.
- A route's body is one source type. Main hands a route `ResponseBody<TResponse>`, a type of its own read with a
  lambda (`Alis.Reactive/PlanAuthoring/Events/ResponseBody.cs:36` at 623dd4a27), and writes text from it by a second
  syntax, `SetText(json, x => x.First)`
  (`Alis.Reactive.SandboxApp/Areas/Sandbox/Views/HttpPipeline/Http/Index.cshtml:16` at 623dd4a27); the reply's `Body` is
  data read with `Read`, as every payload is (N5.6).
- One error body on both hosts, written by a framework door (D10.8, N10.1), where each app writes its own by hand
  and the browser guesses its shape.
