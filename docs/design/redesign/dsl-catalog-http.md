# http

The HTTP pipeline grammar for Alis.Reactive, catalogued exhaustively against the
source under `Alis.Reactive/Builders/Requests/` (HttpRequestBuilder, GatherBuilder,
GatherExtensions, ResponseBuilder, ParallelBuilder) plus `PipelineBuilder.Http.cs`,
`PipelineBuilder.Into`, and `ResponseBody<T>`.

Every request is a tall fluent chain inside a reaction pipeline: a verb
(`p.Get/Post/Put/Delete(url)`) opens the request, `.Gather(...)` collects values from
components, events, the URL, plugins, and constants into payload / headers / route
params, `.AsJson()`/`.AsFormData()` picks the body format, `.Response(...)` routes
success/error into scopes, `.Chained(...)` sequences a follow-up, `.WhileLoading(...)`
and `.OnSettled(...)` bracket the in-flight window, and `.Validate<TSource>(formId)`
gates the call on client validation. `p.Parallel(...)` fans out concurrently and
`.OnAllSettled(...)` joins. `.Into(elementId)` (a pipeline command used inside a
success route) injects the whole response body into a DOM element.

Names are the finalized green-field names from `09-dsl-naming-sheet.md` §3.4:
two renames apply — gather `Static → Literal`, and HTTP `Finally → OnSettled`.
Everything else keeps its current name. Senior-living domain throughout (residents,
care levels, facilities, billing, assessments).

> Form note: triggers use the finalized single-lambda attach `Html.On(plan, t => t.Trigger(...))`
> with finalized trigger names (`PageLoad`, `Event`). The HTTP chain is the body of
> the reaction lambda `p => ...`.

---

## Verbs

### Get
HTTP GET; gathered values are emitted as query-string parameters; the URL may carry `{placeholder}` route templates.

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Get("/api/facilities/mystery-manor/residents")
        .Response(r => r
            .OnSuccess<ResidentRosterResponse>((json, s) => s
                .Component<FusionGrid>("resident-grid")
                    .SetDataSource(json, j => j.Residents))));
```

### Post
HTTP POST; gathered values become the request body.

```csharp
Html.On(plan, t => t.Event("save-resident", e => e.Click()))
    .Then(p => p
        .Post("/api/residents")
        .Gather(g => g
            .IncludeAll())
        .Response(r => r
            .OnSuccess(s => s
                .Element("status").SetText("Resident admitted"))));
```

### Post (inline gather overload)
`Post(url, gather)` opens the request and configures the gather in one call.

```csharp
Html.On(plan, t => t.Event("save-resident", e => e.Click()))
    .Then(p => p
        .Post("/api/residents", g => g
            .IncludeAll()
            .Literal("source", "intake-form"))
        .Response(r => r
            .OnSuccess(s => s
                .Element("status").SetText("Resident admitted"))));
```

### Put
HTTP PUT to replace a resource.

```csharp
Html.On(plan, t => t.Event("update-resident", e => e.Click()))
    .Then(p => p
        .Put("/api/residents/{id}")
        .Gather(g => g
            .RouteParam("id", m => m.ResidentId)
            .Include<FusionTextBox, ResidentModel>(m => m.FullName))
        .Response(r => r
            .OnSuccess(s => s
                .Element("status").SetText("Resident updated"))));
```

### Put (inline gather overload)
`Put(url, gather)` opens the PUT and configures the gather in one call.

```csharp
Html.On(plan, t => t.Event("update-rate", e => e.Click()))
    .Then(p => p
        .Put("/api/billing/rate", g => g
            .Literal("careLevel", "memory-care")
            .Literal("monthlyRate", 7850))
        .Response(r => r
            .OnSuccess(s => s
                .Element("status").SetText("Rate updated"))));
```

### Delete
HTTP DELETE to remove a resource; route templates fill from the gather.

```csharp
Html.On(plan, t => t.Event("discharge-resident", e => e.Click()))
    .Confirm("Discharge this resident?")
    .Then(p => p
        .Delete("/api/residents/{id}")
        .Gather(g => g
            .RouteParam("id", m => m.ResidentId))
        .Response(r => r
            .OnSuccess(s => s
                .Element("status").SetText("Resident discharged"))));
```

---

## Gather

`.Gather(g => ...)` collects values into three target families: **payload** (`Include`,
`IncludeAll`, `Literal`, `FromEvent`, `FromUrl`, `Plugin`), **headers** (`Header`), and
**route params** (`RouteParam`). All read through the one `ValueExpression` value spine.

### Include — by model expression
Typed component value, identified by the model property; the property name becomes the HTTP parameter name. `TComponent` names the vendor/value member.

```csharp
Html.On(plan, t => t.Event("reload-roster", e => e.Click()))
    .Then(p => p
        .Get("/api/schedule/assignments")
        .Gather(g => g
            .Include<FusionDropDownList, ScheduleModel>(m => m.SelectedFacilityId))
        .Response(r => r
            .OnSuccess<ScheduleDataResponse>((json, s) => s
                .Component<FusionSchedule>("shift-schedule")
                    .SetDataSource(json, j => j.Assignments))));
```

### Include — by typed component member source
Reads a typed component member (property or method return) as the payload value; the member name becomes the parameter name.

```csharp
Html.On(plan, t => t.Event("reload-roster", e => e.Click()))
    .Then(p => p
        .Get("/api/schedule/assignments")
        .Gather(g => g
            .Include(p.Component<FusionSchedule>("shift-schedule").CurrentView()))
        .Response(r => r
            .OnSuccess<ScheduleDataResponse>((json, s) => s
                .Component<FusionSchedule>("shift-schedule")
                    .SetDataSource(json, j => j.Assignments))));
```

### Include — typed component source with explicit param name
Same as above, but renames the HTTP parameter when it differs from the member name.

```csharp
Html.On(plan, t => t.Event("reload-roster", e => e.Click()))
    .Then(p => p
        .Get("/api/schedule/assignments")
        .Gather(g => g
            .Include(p.Component<FusionSchedule>("shift-schedule").SelectedDate(), "currentDate")
            .Include(p.Component<FusionSchedule>("shift-schedule").GetEvents(), "events"))
        .Response(r => r
            .OnSuccess<ScheduleDataResponse>((json, s) => s
                .Element("count").SetText(json, j => j.Count))));
```

### Include — by explicit element id and property name
Reads a component's value by element id and named member; works for input and display components.

```csharp
Html.On(plan, t => t.Event("audit", e => e.Click()))
    .Then(p => p
        .Post("/api/schedule/events/audit")
        .Gather(g => g
            .Include<FusionGrid, ScheduleModel>("resident-grid", "selectedRowIndex"))
        .Response(r => r
            .OnSuccess(s => s
                .Element("status").SetText("Audited"))));
```

### IncludeAll
Includes every registered input component value in the payload.

```csharp
Html.On(plan, t => t.Event("submit-assessment", e => e.Click()))
    .Then(p => p
        .Post("/api/assessments")
        .Gather(g => g
            .IncludeAll())
        .Response(r => r
            .OnSuccess(s => s
                .Element("status").SetText("Assessment submitted"))));
```

### Literal
Includes a constant key/value pair in the payload (renamed from `Static`).

```csharp
Html.On(plan, t => t.Event("save-resident", e => e.Click()))
    .Then(p => p
        .Post("/api/residents")
        .Gather(g => g
            .IncludeAll()
            .Literal("source", "intake-form")
            .Literal("intakeStep", 4))
        .Response(r => r
            .OnSuccess(s => s
                .Element("status").SetText("Saved"))));
```

### FromEvent
Includes a value pulled from the triggering event payload, with an explicit parameter name.

```csharp
Html.On(plan, t => t.Event<ScheduleActionPayload>("schedule:edit"))
    .Then((payload, p) => p
        .Get("/Sandbox/Components/Schedule/EditForm")
        .Gather(g => g
            .FromEvent(payload, x => x.Id, "assignmentId"))
        .Response(r => r
            .OnSuccess(s => s
                .Into("alis-drawer-content"))));
```

### FromUrl
Includes a URL query parameter; the parameter name is both the URL key read and the payload key.

```csharp
Html.On(plan, t => t.Event("apply-filter", e => e.Click()))
    .Then(p => p
        .Get("/api/residents")
        .Gather(g => g
            .FromUrl("tab")
            .FromUrl("facilityId"))
        .Response(r => r
            .OnSuccess(s => s
                .Element("status").SetText("Filtered"))));
```

### FromUrl — explicit payload name
Reads the URL param but emits it under a different HTTP parameter name.

```csharp
Html.On(plan, t => t.Event("apply-filter", e => e.Click()))
    .Then(p => p
        .Get("/api/residents")
        .Gather(g => g
            .FromUrl("facilityId", "facility"))
        .Response(r => r
            .OnSuccess(s => s
                .Element("status").SetText("Filtered"))));
```

### FromUrl<T> — typed
Reads the URL param with shape conversion (e.g. `int`); the param name is the payload key.

```csharp
Html.On(plan, t => t.Event("apply-filter", e => e.Click()))
    .Then(p => p
        .Get("/api/residents")
        .Gather(g => g
            .FromUrl<int>("page"))
        .Response(r => r
            .OnSuccess(s => s
                .Element("status").SetText("Page loaded"))));
```

### FromUrl<T> — typed with explicit payload name
Typed URL param with shape conversion and a renamed payload key.

```csharp
Html.On(plan, t => t.Event("apply-filter", e => e.Click()))
    .Then(p => p
        .Get("/api/residents")
        .Gather(g => g
            .FromUrl<int>("page", "pageNumber"))
        .Response(r => r
            .OnSuccess(s => s
                .Element("status").SetText("Page loaded"))));
```

### Plugin
Includes a typed plugin function result (which may carry args) as a payload value.

```csharp
Html.On(plan, t => t.Event("export", e => e.Click()))
    .Then(p => p
        .Post("/api/residents/export")
        .Gather(g => g
            .Plugin(p.Plugin<string>("csv", x => x.BuildFilename()), "filename"))
        .Response(r => r
            .OnSuccess(s => s
                .Element("status").SetText("Exported"))));
```

### Header — literal string
Adds a constant request header. Literal headers require a concrete value (null throws — use a typed/event overload for dynamic values).

```csharp
Html.On(plan, t => t.Event("save-resident", e => e.Click()))
    .Then(p => p
        .Post("/api/residents")
        .Gather(g => g
            .IncludeAll()
            .Header("X-Api-Version", "2024-01-15")
            .Header("X-Tenant-Id", "facility-42"))
        .Response(r => r
            .OnSuccess(s => s
                .Element("status").SetText("Saved"))));
```

### Header — typed source
Adds a header from a typed source (component / URL / plugin). Headers are scalar; arrays and objects are rejected at build time.

```csharp
Html.On(plan, t => t.Event("apply-filter", e => e.Click()))
    .Then(p => p
        .Get("/api/residents")
        .Gather(g => g
            .Header("X-Tab", p.FromUrl("tab"))
            .Header("X-Facility", p.Component<FusionDropDownList>("facility-picker").Value()))
        .Response(r => r
            .OnSuccess(s => s
                .Element("status").SetText("Filtered"))));
```

### Header — from event arg
Adds a header read from the triggering event payload.

```csharp
Html.On(plan, t => t.Event<ScheduleActionPayload>("schedule:edit"))
    .Then((payload, p) => p
        .Get("/api/schedule/assignment")
        .Gather(g => g
            .Header("X-Assignment-Id", payload, x => x.Id))
        .Response(r => r
            .OnSuccess(s => s
                .Into("alis-drawer-content"))));
```

### RouteParam — from model expression
Fills a `{placeholder}` in the URL template from a model-bound component value.

```csharp
Html.On(plan, t => t.Event("update-resident", e => e.Click()))
    .Then(p => p
        .Put("/api/residents/{id}")
        .Gather(g => g
            .RouteParam("id", m => m.ResidentId)
            .IncludeAll())
        .Response(r => r
            .OnSuccess(s => s
                .Element("status").SetText("Updated"))));
```

### RouteParam — static int
Fills a route placeholder with a constant int.

```csharp
Html.On(plan, t => t.Event("load-resident", e => e.Click()))
    .Then(p => p
        .Get("/api/residents/{id}")
        .Gather(g => g
            .RouteParam("id", 42))
        .Response(r => r
            .OnSuccess<ResidentResponse>((json, s) => s
                .Element("name").SetText(json, x => x.FullName))));
```

### RouteParam — static long
Fills a route placeholder with a constant long.

```csharp
Html.On(plan, t => t.Event("load-billing", e => e.Click()))
    .Then(p => p
        .Get("/api/billing/{accountId}")
        .Gather(g => g
            .RouteParam("accountId", 90000000001L))
        .Response(r => r
            .OnSuccess<BillingResponse>((json, s) => s
                .Element("balance").SetText(json, x => x.Balance))));
```

### RouteParam — static string
Fills a route placeholder with a constant string (null throws — use a typed/event overload for dynamic values).

```csharp
Html.On(plan, t => t.Event("load-facility", e => e.Click()))
    .Then(p => p
        .Get("/api/facilities/{slug}")
        .Gather(g => g
            .RouteParam("slug", "mystery-manor"))
        .Response(r => r
            .OnSuccess<FacilityResponse>((json, s) => s
                .Element("facility-name").SetText(json, x => x.Name))));
```

### RouteParam — from typed source
Fills a route placeholder from a typed source (component / URL / plugin). Route params are scalar.

```csharp
Html.On(plan, t => t.Event("audit-view", e => e.Click()))
    .Then(p => p
        .Get("/api/schedule/view/{currentView}/echo")
        .Gather(g => g
            .RouteParam("currentView", p.Component<FusionSchedule>("shift-schedule").CurrentView()))
        .Response(r => r
            .OnSuccess<ScheduleViewRouteResponse>((json, s) => s
                .Element("route-view").SetText(json, x => x.CurrentView))));
```

### RouteParam — from typed URL source
Fills a route placeholder from a typed URL query parameter.

```csharp
Html.On(plan, t => t.Event("load-resident", e => e.Click()))
    .Then(p => p
        .Get("/api/residents/{id}")
        .Gather(g => g
            .RouteParam("id", p.FromUrl<int>("residentId")))
        .Response(r => r
            .OnSuccess<ResidentResponse>((json, s) => s
                .Element("name").SetText(json, x => x.FullName))));
```

### RouteParam — from event arg
Fills a route placeholder read from the triggering event payload.

```csharp
Html.On(plan, t => t.Event<ScheduleActionPayload>("schedule:edit"))
    .Then((payload, p) => p
        .Get("/api/schedule/{assignmentId}")
        .Gather(g => g
            .RouteParam("assignmentId", payload, x => x.Id))
        .Response(r => r
            .OnSuccess(s => s
                .Into("alis-drawer-content"))));
```

---

## Body format

### AsJson
Sends the request body as JSON (the default for POST/PUT — explicit when you want it stated).

```csharp
Html.On(plan, t => t.Event("save-resident", e => e.Click()))
    .Then(p => p
        .Post("/api/residents")
        .Gather(g => g
            .IncludeAll())
        .AsJson()
        .Response(r => r
            .OnSuccess(s => s
                .Element("status").SetText("Saved"))));
```

### AsFormData
Sends the request body as form-data.

```csharp
Html.On(plan, t => t.Event("submit-intake", e => e.Click()))
    .Then(p => p
        .Post("/api/residents/intake")
        .Gather(g => g
            .Literal("FirstName", "John")
            .Literal("LastName", "Doe")
            .Literal("Email", "john@example.com"))
        .AsFormData()
        .Response(r => r
            .OnSuccess(s => s
                .Element("status").SetText("Intake submitted"))));
```

---

## Response routing

`.Response(r => ...)` opens success/error scopes. Inside a typed scope the first lambda
parameter (`json` / `err`) is a `ResponseBody<T>` for typed body reads.

### OnSuccess
Runs a reaction pipeline on a 2xx response (no typed body).

```csharp
Html.On(plan, t => t.Event("save-resident", e => e.Click()))
    .Then(p => p
        .Post("/api/residents")
        .Gather(g => g
            .IncludeAll())
        .Response(r => r
            .OnSuccess(s => s
                .Element("status").SetText("Resident admitted")
                .Dispatch("roster:refresh"))));
```

### OnSuccess<TResponse> — typed body
Runs a reaction pipeline with a typed `ResponseBody<TResponse>` for reading body properties.

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Get("/api/schedule/assignments")
        .Response(r => r
            .OnSuccess<ScheduleDataResponse>((json, s) => s
                .Component<FusionSchedule>("shift-schedule")
                    .SetDataSource(json, j => j.Assignments)
                .Element("unassigned-count").SetText(json, j => j.UnassignedCount))));
```

### OnError
Runs a reaction pipeline on any non-2xx response (no typed body).

```csharp
Html.On(plan, t => t.Event("save-resident", e => e.Click()))
    .Then(p => p
        .Post("/api/residents")
        .Gather(g => g
            .IncludeAll())
        .Response(r => r
            .OnSuccess(s => s
                .Element("status").SetText("Saved"))
            .OnError(e => e
                .Element("status").SetText("Save failed"))));
```

### OnError(statusCode)
Routes a specific HTTP status code to its own reaction pipeline (first-match status routing).

```csharp
Html.On(plan, t => t.Event("save-resident", e => e.Click()))
    .Then(p => p
        .Post("/api/residents")
        .Gather(g => g
            .IncludeAll())
        .Validate<ResidentValidator>("resident-form")
        .Response(r => r
            .OnSuccess(s => s
                .Element("status").SetText("Saved"))
            .OnError(400, e => e
                .ShowValidationErrors("resident-form"))
            .OnError(422, e => e
                .Element("status").SetText("Validation failed"))
            .OnError(500, e => e
                .Element("status").SetText("Server error"))));
```

### OnError<TError> — typed error body
Runs a reaction pipeline with a typed `ResponseBody<TError>` for reading error-body properties.

```csharp
Html.On(plan, t => t.Event("save-resident", e => e.Click()))
    .Then(p => p
        .Post("/api/residents")
        .Gather(g => g
            .IncludeAll())
        .Response(r => r
            .OnSuccess(s => s
                .Element("status").SetText("Saved"))
            .OnError<ErrorDetailResponse>((err, e) => e
                .Element("status").SetText(err, x => x.Message))));
```

### OnError<TError>(statusCode) — typed error body for a status
Routes a specific status code to a reaction pipeline with a typed error body.

```csharp
Html.On(plan, t => t.Event("save-resident", e => e.Click()))
    .Then(p => p
        .Post("/api/residents")
        .Gather(g => g
            .IncludeAll())
        .Response(r => r
            .OnSuccess(s => s
                .Element("status").SetText("Saved"))
            .OnError<ErrorDetailResponse>(409, (err, e) => e
                .Element("status").SetText(err, x => x.Conflict))));
```

### ResponseBody.Read
`json.Read(x => x.Prop)` yields a `TypedSource<TProp>` from the response body — usable in conditions, source-vs-source comparisons, route params of a chained request, and array sources.

```csharp
Html.On(plan, t => t.Event("approve", e => e.Click()))
    .Then(p => p
        .Post("/api/residents/screen")
        .Gather(g => g
            .IncludeAll())
        .Response(r => r
            .OnSuccess<ScreeningResponse>((json, s) => s
                .When(json.Read(x => x.Status)).Eq("approved")
                    .Then(t => t
                        .Element("status").SetText("Approved")))));
```

---

## Into

### Into
A pipeline command (used inside a success route) that injects the whole response body into a DOM element as HTML. Its whole-read lowers to the `WholeResponseBody` kind.

```csharp
Html.On(plan, t => t.Event<ScheduleActionPayload>("schedule:edit"))
    .Then((payload, p) => p
        .Get("/Sandbox/Components/Schedule/EditForm")
        .Gather(g => g
            .FromEvent(payload, x => x.Id, "assignmentId"))
        .Response(r => r
            .OnSuccess(s => s
                .Into("alis-drawer-content"))));
```

---

## Chained

### Chained
After the current response, runs a follow-up request. The chained request may gather from the previous success body via `json.Read(...)`.

```csharp
Html.On(plan, t => t.Event("audit-view", e => e.Click()))
    .Then(p => p
        .Get("/api/schedule/view/{currentView}/echo")
        .Gather(g => g
            .RouteParam("currentView", p.Component<FusionSchedule>("shift-schedule").CurrentView()))
        .Response(r => r
            .OnSuccess<ScheduleViewRouteResponse>((json, s) => s
                .Element("route-view").SetText(json, x => x.CurrentView))
            .Chained(c => c
                .Get("/api/schedule/view/{currentView}/summary")
                .Gather(g => g
                    .RouteParam("currentView", p.Component<FusionSchedule>("shift-schedule").CurrentView()))
                .Response(r2 => r2
                    .OnSuccess<ScheduleViewRouteResponse>((json2, s2) => s2
                        .Element("route-view-summary").SetText(json2, x => x.Summary))))));
```

### Chained — gather route param from the previous response body
The chained request reads `json.Read(...)` of the parent success body into its own route param.

```csharp
Html.On(plan, t => t.Event("create-then-load", e => e.Click()))
    .Then(p => p
        .Post("/api/residents")
        .Gather(g => g
            .IncludeAll())
        .Response(r => r
            .OnSuccess<CreatedResidentResponse>((json, s) => s
                .Element("status").SetText("Created"))
            .Chained(c => c
                .Get("/api/facilities/{facilityId}/residents/{residentId}")
                .Gather(g => g
                    .RouteParam("facilityId", 3)
                    .RouteParam("residentId", json.Read(x => x.ResidentId)))
                .Response(r2 => r2
                    .OnSuccess<ResidentResponse>((json2, s2) => s2
                        .Element("loaded-name").SetText(json2, x => x.FullName))))));
```

---

## Parallel

### Parallel
Runs multiple HTTP requests concurrently. Each branch is its own request lambda passed as a `params` argument.

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Parallel(
            b => b
                .Get("/api/residents")
                .Response(r => r
                    .OnSuccess<ResidentRosterResponse>((json, s) => s
                        .Component<FusionGrid>("resident-grid")
                            .SetDataSource(json, j => j.Residents))),
            b => b
                .Get("/api/facilities")
                .Response(r => r
                    .OnSuccess<FacilityListResponse>((json, s) => s
                        .Component<FusionDropDownList>("facility-picker")
                            .SetDataSource(json, j => j.Facilities)))));
```

### OnAllSettled
Runs a reaction pipeline after every parallel branch settles (success, error, or network failure) — borrows `Promise.allSettled` semantics.

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Parallel(
            b => b
                .Get("/api/residents")
                .Response(r => r
                    .OnSuccess<ResidentRosterResponse>((json, s) => s
                        .Component<FusionGrid>("resident-grid")
                            .SetDataSource(json, j => j.Residents))),
            b => b
                .Get("/api/facilities")
                .Response(r => r
                    .OnSuccess<FacilityListResponse>((json, s) => s
                        .Component<FusionDropDownList>("facility-picker")
                            .SetDataSource(json, j => j.Facilities)))
        )
        .OnAllSettled(s => s
            .Element("dashboard-status").SetText("Dashboard ready")));
```

---

## Loading bracket

### WhileLoading
Runs a reaction pipeline before the request is sent / during the in-flight window (e.g. show a spinner).

```csharp
Html.On(plan, t => t.Event("save-resident", e => e.Click()))
    .Then(p => p
        .Post("/api/residents")
        .Gather(g => g
            .IncludeAll())
        .WhileLoading(l => l
            .Element("save-spinner").Show())
        .Response(r => r
            .OnSuccess(s => s
                .Element("status").SetText("Saved"))));
```

### OnSettled
Runs a cleanup reaction pipeline after the request settles, regardless of success, error, or network failure. No response body is available (renamed from `Finally`). Pairs with `WhileLoading` as the bracket.

```csharp
Html.On(plan, t => t.Event("save-resident", e => e.Click()))
    .Then(p => p
        .Post("/api/residents")
        .Gather(g => g
            .IncludeAll())
        .WhileLoading(l => l
            .Element("save-spinner").Show())
        .OnSettled(s => s
            .Element("save-spinner").Hide())
        .Response(r => r
            .OnSuccess(s => s
                .Element("status").SetText("Saved"))));
```

---

## Validate

### Validate<TValidationSource>(formId)
Gates the request on client-side validation passing; `TValidationSource` is the validator type, `formId` is the DOM container where errors display. Cross-area: the same `Validate` verb as the validation gate — pair with `OnError(400, e => e.ShowValidationErrors(formId))`.

```csharp
Html.On(plan, t => t.Event("submit-screening", e => e.Click()))
    .Then(p => p
        .Post("/api/residents/screen")
        .Gather(g => g
            .IncludeAll()
            .Literal("step", 4))
        .Validate<Step4Validator>("screening-form")
        .Response(r => r
            .OnSuccess(s => s
                .Element("status").SetText("Screening accepted"))
            .OnError(400, e => e
                .ShowValidationErrors("screening-form"))));
```

---

## Full assembled chain

Every section combined — the request as a developer actually writes it: validate, gather
from many sources, set headers and route params, pick body format, bracket with a loading
spinner, route success/error/status, then chain a dependent request reading the parent body.

```csharp
Html.On(plan, t => t.Event("admit-resident", e => e.Click()))
    .Confirm("Admit this resident to memory care?")
    .Then(p => p
        .Post("/api/facilities/{facilityId}/residents")
        .Gather(g => g
            .RouteParam("facilityId", m => m.SelectedFacilityId)
            .IncludeAll()
            .Include(p.Component<FusionDatePicker>("admit-date").Value(), "admittedOn")
            .FromUrl("referralSource", "referral")
            .Literal("careLevel", "memory-care")
            .Header("X-Api-Version", "2024-01-15"))
        .AsJson()
        .Validate<AdmissionValidator>("admission-form")
        .WhileLoading(l => l
            .Element("admit-spinner").Show())
        .OnSettled(s => s
            .Element("admit-spinner").Hide())
        .Response(r => r
            .OnSuccess<AdmissionResponse>((json, s) => s
                .Element("status").SetText("Resident admitted"))
            .OnError(400, e => e
                .ShowValidationErrors("admission-form"))
            .OnError<ErrorDetailResponse>(500, (err, e) => e
                .Element("status").SetText(err, x => x.Message))
            .Chained(c => c
                .Post("/api/billing/account")
                .Gather(g => g
                    .Literal("careLevel", "memory-care")
                    .Header("X-Resident-Id", "new"))
                .Response(r2 => r2
                    .OnSuccess<BillingResponse>((json2, s2) => s2
                        .Element("billing-status").SetText(json2, x => x.Balance))))));
```
