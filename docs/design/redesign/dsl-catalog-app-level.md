# app-level

The page-wide service objects: **NativeDrawer**, **NativeLoader**, **FusionToast**,
**FusionConfirm**, and the per-link **NativeActionLink**. The first four are
*singletons* — one instance per page, each with a fixed well-known element id, so you
reference them with the **no-argument** `Component<T>()` overload (no model expression,
no id). `NativeActionLink` is a per-link component (its own generated id) rendered
through an `Html` factory, not a pipeline `Component<T>()` call.

These are ordinary browser objects: typed properties you set and methods you call, all
reached through `ComponentRef<TComponent, TModel>` inside a reactive pipeline. Every
verb below emits a **sync** reaction (a property `set` or a method `call`) — there is no
async lane here; the async lane belongs to the HTTP pipeline these services decorate.

Every workflow is attached the finalized way — **`Html.On(plan, t => t.Trigger(...))`**,
the single-lambda attach. The trigger carries its own pipeline (`t.PageLoad(p => ...)`,
`t.Event("name", p => ...)`); a guarded workflow uses the **flat** `.Confirm("...").Then(p => ...)`
form. All samples are TALL: one fluent call per line, read top-to-bottom.

Finalized names used throughout (from `09-dsl-naming-sheet.md`): drawer `SetSize` over the
`DrawerSize` enum (`DrawerPosition` is **deleted**); loader `SetAutoHide` (was `SetTimeout`);
toast `SetMessage` (was `SetContent`) and `SetDuration` (was `SetTimeout`), severities
`Success`/`Warning`/`Danger`/`Info` (the `ToastType`/`ToastPosition` enums are **deleted**);
confirm `SetMessage` (was `SetContent`) and the renderer `@Html.FusionConfirm()` (was
`FusionConfirmDialog`); action-link `CssClass`/`Attr`; the surface `Confirm` guard
(plan term `ConfirmGuard`, wire `kind:"confirm"`).

---

## Referencing an app-level service

### Component&lt;T&gt;() — the no-argument app-level overload

Resolves a layout-owned singleton (Drawer, Loader, Toast, Confirm) by its `DefaultId` —
no model expression, no explicit id. This is **the** way every chain below begins.

```csharp
Html.On(plan, t => t
    .Event("open-resident-drawer", p => p
        .Component<NativeDrawer>()
            .Open()));
```

It works from every pipeline scope — the page-load pipeline, an event pipeline, an HTTP
`WhileLoading` lane, and the `OnSuccess` / `OnError` response scopes:

```csharp
Html.On(plan, t => t
    .Event("save-resident", p => p
        .Post("/Sandbox/Residents/Create", g => g
            .IncludeAll())
        .WhileLoading(loading => loading
            .Component<NativeLoader>()
                .Show())
        .Response(r => r
            .OnSuccess(s => s
                .Component<NativeLoader>()
                    .Hide())
            .OnError(e => e
                .Component<NativeLoader>()
                    .Hide()))));
```

> The other `Component<T>(...)` overloads — `Component<T>(expr)`, `Component<T, TOtherModel>(expr)`,
> `Component<T>(id)` — identify *model-bound* components and are catalogued in the
> components area. App-level singletons use only the no-arg form.

---

## NativeDrawer

The slide-out side panel. One per page; render `@Html.NativeDrawer()` once in the
layout. Symmetric `Open` / `Close`, plus `SetSize`.

### Open

Makes the drawer visible (adds the visible class, clears `aria-hidden`).

```csharp
Html.On(plan, t => t
    .Event("admit-resident", p => p
        .Component<NativeDrawer>()
            .Open()));
```

### Close

Hides the panel.

```csharp
Html.On(plan, t => t
    .Event("cancel-admission", p => p
        .Component<NativeDrawer>()
            .Close()));
```

### SetSize

Sets the panel width to `Sm`, `Md`, or `Lg` (the `DrawerSize` enum). Chains naturally
before `Open` so the drawer is sized as it appears.

```csharp
Html.On(plan, t => t
    .Event("open-quick-note", p => p
        .Component<NativeDrawer>()
            .SetSize(DrawerSize.Sm)
            .Open()));
```

```csharp
Html.On(plan, t => t
    .Event("open-assessment", p => p
        .Component<NativeDrawer>()
            .SetSize(DrawerSize.Md)
            .Open()));
```

```csharp
Html.On(plan, t => t
    .Event("open-full-care-plan", p => p
        .Component<NativeDrawer>()
            .SetSize(DrawerSize.Lg)
            .Open()));
```

> `DrawerSize` variants: `Sm`, `Md` (default), `Lg`. The `DrawerPosition` enum is
> deleted in the rewrite — dead, no setter consumed it.

### Close from the drawer's own close button

The close button dispatches; a workflow listens and closes — symmetric with the
opener event.

```csharp
Html.On(plan, t => t
    .DispatchFrom("alis-drawer-close", p => p
        .Component<NativeDrawer>()
            .Close()));
```

### Drawer holding a posted form — full four-service composition

The drawer commonly holds an injected form partial that posts, shows the loader, then
toasts and closes the drawer on success — the canonical Drawer + Loader + Toast + Validate pattern.

```csharp
Html.On(plan, t => t
    .Event("save-resident", p => p
        .Post("/Sandbox/Components/Drawer/SubmitResident", g => g
            .IncludeAll())
        .Validate<DrawerResidentValidator>("drawer-resident-form")
        .WhileLoading(loading => loading
            .Component<NativeLoader>()
                .SetTarget("drawer-resident-form")
                .SetAutoHide(1000)
                .Show())
        .Response(r => r
            .OnSuccess(s => s
                .Component<NativeLoader>()
                    .Hide()
                .Component<FusionToast>()
                    .SetTitle("Admission")
                    .SetMessage("Resident saved successfully")
                    .Success()
                    .Show()
                .Component<NativeDrawer>()
                    .Close())
            .OnError(400, e => e
                .Component<NativeLoader>()
                    .Hide()
                .ShowValidationErrors("drawer-resident-form")))));
```

---

## NativeLoader

The loading overlay. One per page; render `@Html.NativeLoader()` once in the layout.
Covers the whole viewport by default, or a target container via `SetTarget`. Symmetric
`Show` / `Hide`, plus `SetTarget` and `SetAutoHide`.

### Show

Reveals the overlay (adds the visible class, clears `aria-hidden`).

```csharp
Html.On(plan, t => t
    .Event("refresh-roster", p => p
        .Component<NativeLoader>()
            .Show()));
```

### Hide

Hides the overlay (removes the visible class, sets `aria-hidden="true"`).

```csharp
Html.On(plan, t => t
    .Event("roster-loaded", p => p
        .Component<NativeLoader>()
            .Hide()));
```

### SetTarget

Moves the loader inside a container so it covers only that element. Without it, the
loader covers the entire viewport. Chains before `Show`.

```csharp
Html.On(plan, t => t
    .Event("recalculate-billing", p => p
        .Component<NativeLoader>()
            .SetTarget("billing-summary-panel")
            .Show()));
```

### SetAutoHide

Sets an auto-hide timeout (milliseconds) so the loader disappears on its own after the
duration — a safety net around a request. Renamed from `SetTimeout`, which collided with
JS `setTimeout` and the Toast timer.

```csharp
Html.On(plan, t => t
    .Event("export-assessments", p => p
        .Component<NativeLoader>()
            .SetTarget("assessment-grid")
            .SetAutoHide(1000)
            .Show()));
```

### Page-load loader for the initial roster fetch

A loader can bracket a `PageLoad` request just as well as an event-driven one.

```csharp
Html.On(plan, t => t
    .PageLoad(p => p
        .Get("/Sandbox/Residents/Roster", g => g
            .IncludeAll())
        .WhileLoading(loading => loading
            .Component<NativeLoader>()
                .SetTarget("roster-grid")
                .Show())
        .Response(r => r
            .OnSuccess(s => s
                .Component<NativeLoader>()
                    .Hide())
            .OnError(e => e
                .Component<NativeLoader>()
                    .Hide()))));
```

### Loader bracketing an HTTP request — full pattern

`Show` during the in-flight window, `Hide` on every settled outcome.

```csharp
Html.On(plan, t => t
    .Event("recalculate-billing", p => p
        .Post("/Sandbox/Billing/Recalculate", g => g
            .IncludeAll())
        .WhileLoading(loading => loading
            .Component<NativeLoader>()
                .SetTarget("billing-summary-panel")
                .Show())
        .Response(r => r
            .OnSuccess(s => s
                .Component<NativeLoader>()
                    .Hide()
                .Element("billing-summary-panel")
                    .SetText("Billing updated"))
            .OnError(e => e
                .Component<NativeLoader>()
                    .Hide()))));
```

---

## FusionToast

The corner notification, backed by Syncfusion Toast. One per page; render
`@Html.FusionToast()` once in the layout. You set title/message, pick a severity, tune
display options, then `Show`. `Hide` dismisses programmatically.

### SetTitle

Sets the toast heading.

```csharp
Html.On(plan, t => t
    .Event("resident-admitted", p => p
        .Component<FusionToast>()
            .SetTitle("Admission")
            .SetMessage("Resident admitted to Memory Care")
            .Success()
            .Show()));
```

### SetMessage

Sets the toast body text (the SF `content` prop). Renamed from `SetContent` so
"content" no longer means two things across services.

```csharp
Html.On(plan, t => t
    .Event("assessment-saved", p => p
        .Component<FusionToast>()
            .SetMessage("Care assessment saved")
            .Success()
            .Show()));
```

### Success severity

A completed action. One severity verb per toast.

```csharp
Html.On(plan, t => t
    .Event("resident-saved", p => p
        .Component<FusionToast>()
            .SetTitle("Saved")
            .SetMessage("Resident record saved")
            .Success()
            .Show()));
```

### Warning severity

Needs attention but not blocking.

```csharp
Html.On(plan, t => t
    .Event("care-plan-stale", p => p
        .Component<FusionToast>()
            .SetTitle("Review Due")
            .SetMessage("This care plan is overdue for review")
            .Warning()
            .Show()));
```

### Danger severity

A failure or hard stop.

```csharp
Html.On(plan, t => t
    .Event("billing-failed", p => p
        .Component<FusionToast>()
            .SetTitle("Billing Error")
            .SetMessage("Could not post charges to the resident ledger")
            .Danger()
            .Show()));
```

### Info severity

Neutral status.

```csharp
Html.On(plan, t => t
    .Event("sync-started", p => p
        .Component<FusionToast>()
            .SetTitle("Syncing")
            .SetMessage("Facility roster is syncing in the background")
            .Info()
            .Show()));
```

> The severity verbs are the real severity vocabulary; the orphan `ToastType` enum is
> deleted. Pick exactly one per toast.

### SetDuration

Sets how long the toast stays visible, in milliseconds (the SF `timeOut` prop). Renamed
from `SetTimeout` — it is a display duration, not a scheduler. `0` keeps it up until
dismissed.

```csharp
Html.On(plan, t => t
    .Event("med-pass-reminder", p => p
        .Component<FusionToast>()
            .SetTitle("Reminder")
            .SetMessage("Evening med pass starts in 15 minutes")
            .Info()
            .SetDuration(8000)
            .Show()));
```

### Sticky toast — stays until closed

`SetDuration(0)` with a close button.

```csharp
Html.On(plan, t => t
    .Event("incident-reported", p => p
        .Component<FusionToast>()
            .SetTitle("Incident Logged")
            .SetMessage("Fall incident requires supervisor sign-off")
            .Danger()
            .SetDuration(0)
            .ShowCloseButton()
            .Show()));
```

### ShowCloseButton

Adds the dismiss "×" to the toast. No-argument toggle (sets the SF flag true).

```csharp
Html.On(plan, t => t
    .Event("policy-updated", p => p
        .Component<FusionToast>()
            .SetTitle("Policy")
            .SetMessage("Visitation policy was updated")
            .Info()
            .ShowCloseButton()
            .Show()));
```

### ShowProgressBar

Adds the countdown progress bar to the toast. No-argument toggle.

```csharp
Html.On(plan, t => t
    .Event("auto-logout-warning", p => p
        .Component<FusionToast>()
            .SetTitle("Session")
            .SetMessage("You will be signed out shortly")
            .Warning()
            .SetDuration(10000)
            .ShowProgressBar()
            .Show()));
```

### Show

Binds the configured props (`dataBind`) and displays the toast. Always the terminal call.

```csharp
Html.On(plan, t => t
    .Event("note-added", p => p
        .Component<FusionToast>()
            .SetMessage("Care note added")
            .Success()
            .Show()));
```

### Hide

Programmatically dismisses the visible toast(s).

```csharp
Html.On(plan, t => t
    .Event("dismiss-notifications", p => p
        .Component<FusionToast>()
            .Hide()));
```

### Full toast — every option together

```csharp
Html.On(plan, t => t
    .Event("admission-complete", p => p
        .Component<FusionToast>()
            .SetTitle("Admission Complete")
            .SetMessage("Eleanor Hayes admitted to Assisted Living, Room 214")
            .Success()
            .SetDuration(6000)
            .ShowCloseButton()
            .ShowProgressBar()
            .Show()));
```

### Toast on HTTP success and error

```csharp
Html.On(plan, t => t
    .Event("save-care-plan", p => p
        .Post("/Sandbox/CarePlans/Save", g => g
            .IncludeAll())
        .Response(r => r
            .OnSuccess(s => s
                .Component<FusionToast>()
                    .SetTitle("Saved")
                    .SetMessage("Care plan saved")
                    .Success()
                    .Show())
            .OnError(e => e
                .Component<FusionToast>()
                    .SetTitle("Save failed")
                    .SetMessage("The care plan could not be saved")
                    .Danger()
                    .Show()))));
```

---

## FusionConfirm

The confirm dialog *service object*, backed by Syncfusion Dialog. One per page; render
`@Html.FusionConfirm()` once in the layout. You set the message and `Show` / `Hide`
the dialog as a service.

> This is distinct from the **`Confirm` guard** (below): `FusionConfirm` is the dialog
> object you imperatively drive; `.Confirm("...")` is the user-decision async guard that
> *gates* a pipeline. They cooperate but are different surfaces.

### SetMessage

Sets the confirmation prompt text (the SF `content` prop, followed by `dataBind`).
Renamed from `SetContent` so "content" no longer means two things across services.

```csharp
Html.On(plan, t => t
    .Event("ask-discharge", p => p
        .Component<FusionConfirm>()
            .SetMessage("Discharge this resident from the facility?")
            .Show()));
```

### Show

Opens the confirm dialog.

```csharp
Html.On(plan, t => t
    .Event("confirm-medication-change", p => p
        .Component<FusionConfirm>()
            .SetMessage("Apply the new medication schedule to this resident?")
            .Show()));
```

### Hide

Closes the confirm dialog programmatically.

```csharp
Html.On(plan, t => t
    .Event("dismiss-confirm", p => p
        .Component<FusionConfirm>()
            .Hide()));
```

### The flat `.Confirm` guard — gating a pipeline (cross-area)

The user-decision async guard `.Confirm(message)` (plan term `ConfirmGuard`, wire
`kind:"confirm"`) pauses the pipeline until the user decides; only on "yes" does the
`.Then(...)` branch run. The finalized form is **flat** — `Html.On(...).Confirm("...").Then(p => ...)`
— not a nested `When(p.Confirm(...))`. Use it to protect destructive operations.

```csharp
Html.On(plan, t => t.DispatchFrom("discharge-resident"))
    .Confirm("Discharge this resident? This cannot be undone.")
    .Then(p => p
        .Set(m => m.ResidentStatus, "Discharged")
        .Dispatch("resident-discharged"));
```

### Confirm guard before a destructive HTTP delete

The guarded `.Then(...)` branch is a full pipeline — request, response scopes, and
app-level toasts all compose inside it.

```csharp
Html.On(plan, t => t.DispatchFrom("delete-care-plan"))
    .Confirm("Delete this care plan permanently?")
    .Then(p => p
        .Delete("/Sandbox/CarePlans/Active", g => g
            .IncludeAll())
        .Response(r => r
            .OnSuccess(s => s
                .Component<FusionToast>()
                    .SetTitle("Deleted")
                    .SetMessage("Care plan deleted")
                    .Warning()
                    .Show())));
```

---

## NativeActionLink

A per-link anchor (`<a>`) that carries a reactive pipeline in a `data-reactive-link`
payload — the row-action / inline-link primitive. Rendered through the
`Html.NativeActionLink(...)` factory, **not** a pipeline `Component<T>()` call. It has
its own generated id; the pipeline you pass fires when the link is clicked. The returned
builder appends presentation via `CssClass` and `Attr`.

### Html.NativeActionLink — link text, url, pipeline

The factory takes the visible text, the `href`, and the reactive pipeline to run.

```csharp
@(Html.NativeActionLink(
    "View Care Plan",
    Url.Action("CarePlan", "Residents", new { id = Model.ResidentId }),
    p => p
        .Component<NativeDrawer>()
            .SetSize(DrawerSize.Lg)
            .Open()))
```

### CssClass

Sets the anchor's `class` attribute. Builder method, chained on the factory result.

```csharp
@(Html.NativeActionLink(
    "Reassign Caregiver",
    Url.Action("Reassign", "Assignments", new { id = Model.AssignmentId }),
    p => p
        .Component<NativeDrawer>()
            .SetSize(DrawerSize.Md)
            .Open())
    .CssClass("text-sm font-medium text-primary hover:underline"))
```

### Attr

Adds an arbitrary HTML attribute to the anchor (ARIA, `data-*`, `title`). Reserved
attributes (`id`, `href`, `data-reactive-link`) are rejected; `class` routes to
`CssClass`. Multiple `Attr` calls chain.

```csharp
@(Html.NativeActionLink(
    "Print Assessment",
    Url.Action("Print", "Assessments", new { id = Model.AssessmentId }),
    p => p
        .Component<NativeLoader>()
            .SetTarget("assessment-grid")
            .SetAutoHide(1500)
            .Show())
    .CssClass("btn-link")
    .Attr("title", "Open the printable care assessment")
    .Attr("aria-label", "Print care assessment")
    .Attr("data-resident-id", Model.ResidentId.ToString()))
```

### NativeActionLink driving an HTTP request — full pattern

The link's pipeline is a full reactive pipeline: gather, request, response scopes, and
app-level service calls all compose inside it.

```csharp
@(Html.NativeActionLink(
    "Recalculate Billing",
    Url.Action("Billing", "Residents", new { id = Model.ResidentId }),
    p => p
        .Post("/Sandbox/Billing/Recalculate", g => g
            .Literal("residentId", Model.ResidentId))
        .WhileLoading(loading => loading
            .Component<NativeLoader>()
                .SetTarget("billing-summary-panel")
                .Show())
        .Response(r => r
            .OnSuccess(s => s
                .Component<NativeLoader>()
                    .Hide()
                .Component<FusionToast>()
                    .SetTitle("Billing")
                    .SetMessage("Ledger recalculated")
                    .Success()
                    .Show())
            .OnError(e => e
                .Component<NativeLoader>()
                    .Hide()
                .Component<FusionToast>()
                    .SetTitle("Billing Error")
                    .SetMessage("Recalculation failed")
                    .Danger()
                    .Show())))
    .CssClass("text-sm text-primary hover:underline"))
```

---

## Layout renderers (call once, in `_Layout.cshtml`)

Each singleton service ships a one-line layout renderer that emits its hidden host
element. Render once; the reactive pipelines above drive it. Renderer name == type name
for every service (the old `FusionConfirmDialog` spelling is dropped).

```cshtml
@* The slide-out drawer host *@
@Html.NativeDrawer()

@* The loading overlay host *@
@Html.NativeLoader()

@* The corner toast host (Syncfusion Toast, bottom-right) *@
@Html.FusionToast()

@* The confirm dialog host (Syncfusion Dialog) *@
@Html.FusionConfirm()
```

---
