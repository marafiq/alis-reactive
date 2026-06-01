# Reactions

The complete reaction surface of the Alis.Reactive DSL: every way to mutate the
browser, dispatch events, branch, parallelize, and read a value source when a
trigger fires. This is the **green-field rewrite** vocabulary — every name on this
page is the finalized one from `09-dsl-naming-sheet.md`. Legacy spellings
(`DomReady`, `CustomEvent`, `DispatchWith`, `ValidationErrors`, `FocusIn`,
`AsSource`, `Finally`) are gone; do not write them.

A reaction is the `Then(p => ...)` body of a trigger. You author it on a
`PipelineBuilder` (the `p` parameter). Reactions run **in declaration order** —
top-to-bottom is execution order. Sync reactions (`Set`, `Call`, `Dispatch`,
`Inject`, `ShowValidationErrors`, branch evaluation) stay synchronous; only HTTP
requests and `Parallel` cross the async boundary. Each reaction carries a
`ReactionTiming` (`sync` / `async`) the runtime routes on — you never set it.

> **Reading orientation.** Every sample is a full fluent chain, one call per line,
> read top-to-bottom. Domain model throughout: residents, care levels, facilities,
> billing, assessments.

---

## On — attaching reactions to a trigger

### On

Adds one or more trigger→reaction workflows to a plan. The entry point for every
reaction on the page. Chain trigger methods on `t`; build the reaction on `p`.

```csharp
@{
    var plan = Html.ReactivePlan<ResidentModel>();
}

@Html.On(plan, t => t
    .PageLoad(p => p
        .Element("intake-banner")
        .SetText("Resident intake open")));

@Html.RenderPlan(plan)
```

### StartsWhen — the trigger surface a reaction hangs off

Each trigger method takes the reaction pipeline as its last argument. The reaction
is identical regardless of trigger; only the entry differs.

```csharp
@Html.On(plan, t => t
    .PageLoad(p => p
        .Element("care-summary")
        .Show())
    .Event("resident-admitted", p => p
        .Element("admit-toast")
        .Show())
    .ServerPush("/sse/billing", p => p
        .Element("billing-ticker")
        .AddClass("pulse"))
    .SignalR("/hubs/assessments", "AssessmentScored", p => p
        .Element("score-badge")
        .AddClass("updated")));
```

A typed-payload trigger (`Event<TPayload>`) hands the reaction a typed `args`
object whose members feed value sources (`SetText(args, ...)`, `When(args, ...)`).

```csharp
@Html.On(plan, t => t
    .Event<AdmissionPayload>("resident-admitted", (args, p) => p
        .Element("admit-name")
        .SetText(args, a => a.ResidentName)));
```

---

## Set — assign a property on an element or component

`Set` writes a property value. On a DOM element it is spelled `SetText` / `SetHtml`
(the two writable element properties) plus `Show` / `Hide`; on a component it is the
vendor `SetValue` / `SetText` / `SetDataSource` member. One verb over distinct
**value-source arities** — literal, event payload, response body, and `TypedSource`.
The arities are deliberately not collapsed.

### SetText

Sets an element's text content. Four value-source overloads.

Literal string:

```csharp
@Html.On(plan, t => t
    .PageLoad(p => p
        .Element("facility-name")
        .SetText("Lakeside Memory Care")));
```

From an event payload property (`Event<TPayload>` `args`):

```csharp
@Html.On(plan, t => t
    .Event<AdmissionPayload>("resident-admitted", (args, p) => p
        .Element("welcome-line")
        .SetText(args, a => a.ResidentName)));
```

From an HTTP response body property (inside `OnSuccess`):

```csharp
@Html.On(plan, t => t
    .Event("load-resident", p => p
        .Get("/api/residents/{id}")
        .RouteParam("id", m => m.ResidentId)
        .Response(r => r
            .OnSuccess<ResidentDto>(ok => ok
                .Element("resident-care-level")
                .SetText(ok.Body, b => b.CareLevel)))));
```

From a `TypedSource` (component read, URL param, plugin read, or array fold).
This overload returns the `ElementBuilder` so element mutations keep chaining:

```csharp
@Html.On(plan, t => t
    .Event("recompute", p => p
        .Element("selected-care-level")
        .SetText(p.Component<FusionDropDownList>(m => m.CareLevel).Value())
        .AddClass("highlight")));
```

### SetHtml

Sets an element's inner HTML. Literal, event-payload, and `TypedSource` overloads
(same value-source family as `SetText`).

Literal HTML:

```csharp
@Html.On(plan, t => t
    .PageLoad(p => p
        .Element("care-plan-panel")
        .SetHtml("<em>No care plan on file.</em>")));
```

From an event payload property:

```csharp
@Html.On(plan, t => t
    .Event<CarePlanPayload>("care-plan-rendered", (args, p) => p
        .Element("care-plan-panel")
        .SetHtml(args, a => a.RenderedHtml)));
```

From a `TypedSource` (returns the `ElementBuilder` for further chaining):

```csharp
@Html.On(plan, t => t
    .Event("refresh-note", p => p
        .Element("clinical-note")
        .SetHtml(p.FromUrl("noteHtml"))
        .RemoveClass("stale")));
```

### Show / Hide

Visibility writes — `Set(hidden=false)` and `Set(hidden=true)`. No arguments.

```csharp
@Html.On(plan, t => t
    .Event("show-billing", p => p
        .Element("billing-section")
        .Show())
    .Event("hide-billing", p => p
        .Element("billing-section")
        .Hide()));
```

### Component SetValue — writing a component property

On a `ComponentRef`, `SetValue` is the typed property write. It carries the same
value-source arities as `SetText`: literal, event payload, response body, and
`TypedSource` (used for `SetDataSource`-style array routing).

Literal:

```csharp
@Html.On(plan, t => t
    .Event("preset-care-level", p => p
        .Component<FusionDropDownList>(m => m.CareLevel)
        .SetValue("memory-care")));
```

From an HTTP response body, feeding a component's data source:

```csharp
@Html.On(plan, t => t
    .Event("load-physicians", p => p
        .Get("/api/facilities/{facilityId}/physicians")
        .RouteParam("facilityId", m => m.FacilityId)
        .Response(r => r
            .OnSuccess<PhysicianListDto>(ok => ok
                .Component<FusionAutoComplete>(m => m.Physician)
                .SetDataSource(ok.Body, b => b.Physicians)
                .DataBind()))));
```

From a typed array source (a client-side `ReactiveArray` fold via
`AsArraySource()` — no HTTP round trip):

```csharp
@Html.On(plan, t => t
    .Event<TagPayload>("tags-changed", (args, p) => p
        .Component<FusionAutoComplete>(m => m.Allergies)
        .SetDataSource(p
            .From(args, a => a.AllTags)
            .Where(tag => tag.IsActive)
            .OrderBy(tag => tag.Label)
            .AsArraySource())
        .DataBind()));
```

---

## Call — invoke a method on an element or component

`Call` invokes a member. On a DOM element the class-mutation verbs lower to a call;
on a component the vendor method verbs (`Focus`, `ShowPopup`, `DataBind`) emit a
`CallReaction`.

### AddClass / RemoveClass / ToggleClass

Direct CSS-class mutation verbs on a live DOM element.

```csharp
@Html.On(plan, t => t
    .Event("flag-overdue", p => p
        .Element("invoice-row")
        .AddClass("overdue")
        .RemoveClass("paid"))
    .Event("toggle-expand", p => p
        .Element("assessment-card")
        .ToggleClass("expanded")));
```

### Focus — move focus to a component

The native DOM `focus` verb on a `ComponentRef`. (Finalized from `FocusIn`, which
lied — `focusin` is a different bubbling event.)

```csharp
@Html.On(plan, t => t
    .Event("start-intake", p => p
        .Component<FusionAutoComplete>(m => m.ResidentName)
        .Focus()));
```

### Component method calls

Vendor method verbs emit a `CallReaction`. They chain with property sets.

```csharp
@Html.On(plan, t => t
    .Event("open-physician-picker", p => p
        .Component<FusionAutoComplete>(m => m.Physician)
        .Enable()
        .ShowPopup()
        .Focus()));
```

---

## Dispatch — emit a custom browser event

`Dispatch` emits a named event that any `Event(name, ...)` trigger listens for.
The emit/listen pair reads consistently: `p.Dispatch("x")` ⇄ `t.Event("x", ...)`.
Three lanes: no payload, **literal** payload, and **source-backed** payload
(`DispatchFrom`).

### Dispatch (no payload)

Fire-and-forget event by name.

```csharp
@Html.On(plan, t => t
    .Event("save-resident", p => p
        .Post("/api/residents")
        .Gather(g => g.IncludeAll())
        .Response(r => r
            .OnSuccess(ok => ok
                .Dispatch("resident-saved")))));
```

### Dispatch (literal payload)

Carries a compile-time literal payload object. Listeners consume it via
`Event<TPayload>`.

```csharp
@Html.On(plan, t => t
    .Event("admit", p => p
        .Dispatch("resident-admitted", new AdmissionPayload
        {
            ResidentName = "Pending",
            CareLevel = "assessment",
            Admitted = false
        })));
```

### DispatchFrom (source-backed payload)

Each payload field comes from a **live** source resolved at dispatch time —
component value, URL param, plugin read, or a literal. The verb name marks the
"from a source" idea (mirrors `FromUrl`, `SetText(source)`). Distinct from the
literal `Dispatch<TPayload>(name, payload)` lane above.

```csharp
@Html.On(plan, t => t
    .Event("publish-care-level", p => p
        .DispatchFrom<CareLevelPayload>("care-level-changed", b => b
            .Set(x => x.CareLevel, p.Component<FusionDropDownList>(m => m.CareLevel).Value())
            .Set(x => x.FacilityId, p.FromUrl<int>("facilityId"))
            .Set(x => x.Source, "intake-form")
            .Set(x => x.Priority, 1)
            .Set(x => x.Confirmed, true))));
```

The `DispatchFrom` payload builder has four `Set` overloads — one source-backed
(`TypedSource<TProp>`) and three literal-typed (`string`, `int`, `bool`) — shown
together in the chain above. Nested payload paths are addressed with dotted
expressions (`x => x.Billing.Amount`).

---

## Inject — put response HTML into an element

`Inject` writes a value into a DOM target. The authored surface is `Into`: it takes
an HTTP success response body and injects it as the element's content. Must follow
a request inside a response scope.

### Into

Injects the whole success response body into an element. The whole-body read lowers
to the `WholeResponseBody` value node (never the old `responseBody` sentinel).

```csharp
@Html.On(plan, t => t
    .Event("load-care-plan", p => p
        .Get("/partials/care-plan/{residentId}")
        .RouteParam("residentId", m => m.ResidentId)
        .Response(r => r
            .OnSuccess(ok => ok
                .Into("care-plan-container")))));
```

`Into` works in an error scope too — inject a server-rendered error fragment:

```csharp
@Html.On(plan, t => t
    .Event("submit-assessment", p => p
        .Post("/api/assessments")
        .Gather(g => g.IncludeAll())
        .Response(r => r
            .OnSuccess(ok => ok
                .Into("assessment-result"))
            .OnError(err => err
                .Into("assessment-errors")))));
```

---

## Component / ComponentRef — typed handle to a component

`Component<TComponent>(...)` resolves a typed `ComponentRef` you can `SetValue` /
`Call` / `Read` on. Four overloads, split on **how you identify the component**.

### Component(model expression)

Bind to a component by the model property it renders (the deterministic-id path).

```csharp
@Html.On(plan, t => t
    .PageLoad(p => p
        .Component<FusionDropDownList>(m => m.CareLevel)
        .SetValue("assisted-living")));
```

### Component(cross-model expression)

Reference a component bound to a **different** model (cross-partial scenarios).

```csharp
@Html.On(plan, t => t
    .Event("sync-facility", p => p
        .Component<FusionDropDownList, FacilityModel>(f => f.FacilityName)
        .SetValue("Lakeside Memory Care")));
```

### Component(explicit id)

Reference a component by an explicit element id.

```csharp
@Html.On(plan, t => t
    .Event("focus-search", p => p
        .Component<FusionAutoComplete>("resident-search-box")
        .Focus()));
```

### Component (layout singleton)

Reference a layout-owned app-level component (Toast, Confirm) by its default id —
no expression, no id argument.

```csharp
@Html.On(plan, t => t
    .Event("save-resident", p => p
        .Post("/api/residents")
        .Gather(g => g.IncludeAll())
        .Response(r => r
            .OnSuccess(ok => ok
                .Component<FusionToast>()
                .SetMessage("Resident saved")
                .Success()
                .Show()))));
```

### ComponentRef.Value() — reading a component as a source

`Value()` (and other vendor reads) returns a `TypedSource` you pass to `When`,
`SetText`, gather, or another component mutation. The read feeds the value spine;
it is not itself a reaction.

```csharp
@Html.On(plan, t => t
    .Event("recompute-billing", p => p
        .When(p.Component<FusionDropDownList>(m => m.CareLevel).Value())
        .Eq("memory-care")
        .Then(p => p
            .Element("billing-amount")
            .SetText("$8,200 / month"))));
```

---

## ShowValidationErrors — render accumulated errors

### ShowValidationErrors

Displays accumulated client-validation errors in a container element. A noun-free
verb (finalized from `ValidationErrors`, which read like a getter). Sync reaction;
typically the failure branch of a validated submit.

```csharp
@Html.On(plan, t => t
    .Event("submit-intake", p => p
        .Post("/api/residents")
        .Validate<ResidentModel>("intake-form")
        .Gather(g => g.IncludeAll())
        .Response(r => r
            .OnError(err => err
                .ShowValidationErrors("intake-form-errors")))));
```

Stand-alone (e.g. surface server-side errors already merged into the plan):

```csharp
@Html.On(plan, t => t
    .Event("reveal-errors", p => p
        .ShowValidationErrors("intake-form-errors")));
```

---

## FromUrl — read a value from the URL

### FromUrl

Reads a URL query parameter as a `TypedSource` for conditions, `SetText`, gather,
or `DispatchFrom`. Two overloads: string (default) and typed.

As a string:

```csharp
@Html.On(plan, t => t
    .PageLoad(p => p
        .Element("active-facility")
        .SetText(p.FromUrl("facility"))));
```

Typed (`int`, `decimal`, `bool`, …) — the type flows into operators and gather:

```csharp
@Html.On(plan, t => t
    .PageLoad(p => p
        .When(p.FromUrl<int>("page"))
        .Gt(1)
        .Then(p => p
            .Element("prev-page-link")
            .Show())));
```

---

## Sequence — ordered reactions

### Sequence (declaration order)

A pipeline body is a sequence: every reaction you add runs in the order written.
There is no explicit `Sequence(...)` verb — the order of the chain **is** the
sequence (it lowers to a `SequenceReaction`).

```csharp
@Html.On(plan, t => t
    .Event("complete-admission", p => p
        .Element("admit-spinner")
        .Show()
        .Element("admit-button")
        .AddClass("disabled")
        .Component<FusionToast>()
        .SetMessage("Admitting resident…")
        .Info()
        .Show()
        .Dispatch("admission-started")));
```

---

## Branch — first-match conditional flow (When / Then / ElseIf / Else)

A branch evaluates guards top-to-bottom and runs the **first** matching case. Author
it with `When(...).<operator>().Then(...)`, chain `ElseIf(...)`, end with `Else(...)`.
Multiple branch blocks may sit between other reactions in one pipeline.

### When + Then

The minimal branch: one guard, one body. `When` has three source overloads —
event payload, response body, and `TypedSource`.

`When(TypedSource)`:

```csharp
@Html.On(plan, t => t
    .Event("evaluate-care", p => p
        .When(p.Component<FusionNumericTextBox>(m => m.AssessmentScore).Value())
        .Gte(40)
        .Then(p => p
            .Element("memory-care-notice")
            .Show())));
```

`When(args, path)` — from an event payload:

```csharp
@Html.On(plan, t => t
    .Event<AdmissionPayload>("resident-admitted", (args, p) => p
        .When(args, a => a.CareLevel)
        .Eq("memory-care")
        .Then(p => p
            .Element("secure-unit-banner")
            .Show())));
```

`When(responseBody, path)` — from an HTTP response body (inside `OnSuccess`):

```csharp
@Html.On(plan, t => t
    .Event("load-resident", p => p
        .Get("/api/residents/{id}")
        .RouteParam("id", m => m.ResidentId)
        .Response(r => r
            .OnSuccess<ResidentDto>(ok => ok
                .When(ok.Body, b => b.OutstandingBalance)
                .Gt(0m)
                .Then(p => p
                    .Element("balance-warning")
                    .Show())))));
```

### When + Then + ElseIf + Else

The full first-match chain. `ElseIf` carries the same three source overloads as
`When`; `Else` is the unguarded default and must come last.

```csharp
@Html.On(plan, t => t
    .Event("classify-care", p => p
        .When(p.Component<FusionNumericTextBox>(m => m.AssessmentScore).Value())
        .Gte(70)
        .Then(p => p
            .Element("care-recommendation")
            .SetText("Memory Care"))
        .ElseIf(p.Component<FusionNumericTextBox>(m => m.AssessmentScore).Value())
        .Gte(40)
        .Then(p => p
            .Element("care-recommendation")
            .SetText("Assisted Living"))
        .Else(p => p
            .Element("care-recommendation")
            .SetText("Independent Living"))));
```

### Compare operators on a guard

`When(source).<op>(...)` — the full deterministic compare surface. Each is a guard
producing a branch.

Equality and ordering (typed operand):

```csharp
@Html.On(plan, t => t
    .Event("billing-check", p => p
        .When(p.Component<FusionNumericTextBox>(m => m.MonthlyRate).Value())
        .Between(2000m, 9000m)
        .Then(p => p
            .Element("rate-ok")
            .Show())));
```

Presence — the six orthogonal poles (`Truthy` / `Falsy` / `IsNull` / `NotNull` /
`IsEmpty` / `NotEmpty`):

```csharp
@Html.On(plan, t => t
    .Event("validate-physician", p => p
        .When(p.Component<FusionAutoComplete>(m => m.Physician).Value())
        .NotNull()
        .Then(p => p
            .Element("physician-ok")
            .Show())));
```

Membership and range (`In` / `NotIn` / `Between`):

```csharp
@Html.On(plan, t => t
    .Event("route-by-level", p => p
        .When(p.Component<FusionDropDownList>(m => m.CareLevel).Value())
        .In("memory-care", "skilled-nursing")
        .Then(p => p
            .Element("clinical-staffing-note")
            .Show())));
```

Text predicates (`Contains` / `StartsWith` / `EndsWith` / `Matches` / `MinLength` /
`MaxLength`):

```csharp
@Html.On(plan, t => t
    .Event<NotePayload>("note-typed", (args, p) => p
        .When(args, a => a.Text)
        .MinLength(10)
        .Then(p => p
            .Element("note-save-button")
            .RemoveClass("disabled"))));
```

Array membership (`ArrayContains`):

```csharp
@Html.On(plan, t => t
    .Event<TagPayload>("tags-changed", (args, p) => p
        .When(args, a => a.Tags)
        .ArrayContains("fall-risk")
        .Then(p => p
            .Element("fall-risk-banner")
            .Show())));
```

### Source-vs-source compare

A guard operator can take another `TypedSource` as its right operand (compare two
live values, not a literal).

```csharp
@Html.On(plan, t => t
    .Event("compare-rates", p => p
        .When(p.Component<FusionNumericTextBox>(m => m.QuotedRate).Value())
        .Gt(p.Component<FusionNumericTextBox>(m => m.BaseRate).Value())
        .Then(p => p
            .Element("upcharge-note")
            .Show())));
```

### And / Or / Not — composing guards

`And` / `Or` have **two** shapes: the flat `TypedSource` shape and the grouped
nested-lambda shape (`(a OR b) AND c`). `Not` inverts.

Flat `And` (event-payload and `TypedSource` overloads both fold through here):

```csharp
@Html.On(plan, t => t
    .Event("eligibility-check", p => p
        .When(p.Component<FusionNumericTextBox>(m => m.Age).Value())
        .Gte(65)
        .And(p.Component<FusionDropDownList>(m => m.CareLevel).Value())
        .Eq("memory-care")
        .Then(p => p
            .Element("eligible-banner")
            .Show())));
```

Grouped `Or` with a nested condition — `(memory-care OR skilled-nursing) AND age >= 65`:

```csharp
@Html.On(plan, t => t
    .Event("staffing-check", p => p
        .When(p.Component<FusionNumericTextBox>(m => m.Age).Value())
        .Gte(65)
        .And(c => c
            .When(p.Component<FusionDropDownList>(m => m.CareLevel).Value())
            .Eq("memory-care")
            .Or(p.Component<FusionDropDownList>(m => m.CareLevel).Value())
            .Eq("skilled-nursing"))
        .Then(p => p
            .Element("clinical-staffing-note")
            .Show())));
```

`Not` — invert a guard:

```csharp
@Html.On(plan, t => t
    .Event("gate-discharge", p => p
        .When(p.Component<FusionCheckBox>(m => m.BillingCleared).Value())
        .Truthy()
        .Not()
        .Then(p => p
            .Element("discharge-blocked")
            .Show())));
```

### Confirm — user-decision guard

`Confirm(message)` is a **user-decision async guard** authored on the same guard
surface, so it composes with `And` and ends with `Then`. The distinct name marks
the async lane (plan term `ConfirmGuard`, wire `confirm`).

```csharp
@Html.On(plan, t => t
    .Event("discharge-resident", p => p
        .Confirm("Discharge this resident? This cannot be undone.")
        .Then(p => p
            .Post("/api/residents/{id}/discharge")
            .RouteParam("id", m => m.ResidentId)
            .Response(r => r
                .OnSuccess(ok => ok
                    .Dispatch("resident-discharged"))))));
```

`Confirm` composed with a data guard via `And`:

```csharp
@Html.On(plan, t => t
    .Event("delete-assessment", p => p
        .Confirm("Permanently delete this assessment?")
        .And(p.Component<FusionCheckBox>(m => m.IsLocked).Value())
        .Falsy()
        .Then(p => p
            .Delete("/api/assessments/{id}")
            .RouteParam("id", m => m.AssessmentId)
            .Response(r => r
                .OnSuccess(ok => ok
                    .Dispatch("assessment-deleted"))))));
```

---

## Parallel — concurrent requests

### Parallel + OnAllSettled

`Parallel(...branches)` starts multiple HTTP requests concurrently; `OnAllSettled`
runs after **every** branch settles (success, error, or network failure — borrows
`Promise.allSettled`). This is the async lane.

```csharp
@Html.On(plan, t => t
    .Event("load-resident-dashboard", p => p
        .Parallel(
            b => b
                .Get("/api/residents/{id}/profile")
                .RouteParam("id", m => m.ResidentId)
                .Response(r => r
                    .OnSuccess(ok => ok
                        .Into("profile-panel"))),
            b => b
                .Get("/api/residents/{id}/billing")
                .RouteParam("id", m => m.ResidentId)
                .Response(r => r
                    .OnSuccess(ok => ok
                        .Into("billing-panel"))),
            b => b
                .Get("/api/residents/{id}/assessments")
                .RouteParam("id", m => m.ResidentId)
                .Response(r => r
                    .OnSuccess(ok => ok
                        .Into("assessments-panel"))))
        .OnAllSettled(done => done
            .Element("dashboard-spinner")
            .Hide())));
```

A `Parallel` block with no completion runs each branch fire-and-forget:

```csharp
@Html.On(plan, t => t
    .Event("prefetch", p => p
        .Parallel(
            b => b
                .Get("/api/facilities/{facilityId}/staff")
                .RouteParam("facilityId", m => m.FacilityId),
            b => b
                .Get("/api/facilities/{facilityId}/rooms")
                .RouteParam("facilityId", m => m.FacilityId))));
```

---

## Mixed pipeline — sets, branch, dispatch, and HTTP together

A single reaction can interleave every lane. Branch blocks, sets, dispatches, and
requests all sit in one ordered pipeline; sync reactions stay sync, the request
crosses the async boundary, and reactions in its response scope resume sync.

```csharp
@Html.On(plan, t => t
    .Event("finalize-intake", p => p
        .Element("finalize-spinner")
        .Show()
        .When(p.Component<FusionNumericTextBox>(m => m.AssessmentScore).Value())
        .Gte(70)
        .Then(p => p
            .Component<FusionDropDownList>(m => m.CareLevel)
            .SetValue("memory-care"))
        .Else(p => p
            .Component<FusionDropDownList>(m => m.CareLevel)
            .SetValue("assisted-living"))
        .Confirm("Submit this intake for review?")
        .Then(p => p
            .Post("/api/residents/intake")
            .Validate<ResidentModel>("intake-form")
            .Gather(g => g.IncludeAll())
            .Response(r => r
                .OnSuccess(ok => ok
                    .Element("finalize-spinner")
                    .Hide()
                    .Component<FusionToast>()
                    .SetMessage("Intake submitted")
                    .Success()
                    .Show()
                    .Dispatch("intake-submitted"))
                .OnError(err => err
                    .Element("finalize-spinner")
                    .Hide()
                    .ShowValidationErrors("intake-form-errors"))))));
```
