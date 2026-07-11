# Sidebar C# Name Decisions

Status: active and proven. The `FusionSidebar` public C# names are decided and
implemented: the `FusionSidebar(...)` render helper, the `Opened` and `Closed`
event selectors with the `FusionSidebarTransitionArgs` payload (`IsInteracted`),
the `Show()`, `Hide()`, and `Toggle()` methods, and the `IsOpen()` read source.
The component is fully audited.

## Pass Rows

Close matrix row: `Html.FusionSidebar(plan, "id", b => ...)` render helper -> Sidebar carrying its controlled component id into the plan.

Close matrix row: `sidebar.Reactive(e => e.Opened, ...)` / `e.Closed` -> typed `FusionSidebarTransitionArgs` payload.

Close matrix row: `Show()`, `Hide()`, `Toggle()`, `IsOpen()` -> typed Sidebar runtime members.

## Evidence Inputs

- Raw core trace: `traces/raw-ej2-core.trace.json`
- Raw core probe: `probes/raw-ej2-core.html`
- Syncfusion source type: `EventArgs` (open/close event), `Sidebar` (component)
- Public API surface: `discovery/public-api-surface.json`
- Event payload surface: `discovery/event-payload-surface.json`
- Blazor candidates: `discovery/blazor-candidates.md` (no Blazor package supplied; naming taken from EJ2 source only)
- Existing C# event args: `Alis.Reactive.Fusion/Components/FusionSidebar/Events/FusionSidebarTransitionArgs.cs`
- Existing event selector: `Alis.Reactive.Fusion/Components/FusionSidebar/FusionSidebarEvents.cs`
- Existing component members: `Alis.Reactive.Fusion/Components/FusionSidebar/FusionSidebarExtensions.cs`
- Existing render helper: `Alis.Reactive.Fusion/Components/FusionSidebar/FusionSidebarHtmlExtensions.cs`
- Existing sandbox usage: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Sidebar/Index.cshtml`

## Name Decision Matrix

| Syncfusion path | C# name | Decision | Reason |
| --- | --- | --- | --- |
| `new ej.navigations.Sidebar(options)` render | `IHtmlHelper<TModel>.FusionSidebar(ReactivePlan<TModel> plan, string elementId, Action<SidebarBuilder> build)` | keep | the render helper renders the EJ2 Sidebar and carries the controlled component id into the plan for `.Reactive(...)` wiring; initial options stay on `SidebarBuilder` |
| `open` event | `FusionSidebarEvents.Opened` | keep | reads as developer intent ("the panel opened"); selected through the typed `.Reactive(e => e.Opened, ...)` lambda; maps to the exact Syncfusion `open` event |
| `close` event | `FusionSidebarEvents.Closed` | keep | reads as developer intent ("the panel closed"); selected through `.Reactive(e => e.Closed, ...)`; maps to the exact Syncfusion `close` event |
| `EventArgs` (open/close payload) | `FusionSidebarTransitionArgs` | keep | the Fusion payload type name states it is a transition payload shared by both `open` and `close`; it carries only the proven, narrowed member |
| `open`/`close` `.isInteracted` | `FusionSidebarTransitionArgs.IsInteracted` | keep | exact Syncfusion key, typed as `bool`; distinguishes a user-driven dismissal from an API close |
| `open`/`close` `.cancel` | none | exclude for this row | writable transition guard; no Senior Living use case cancels the slide, and a writable cancel needs its own lifecycle-effect proof |
| `open`/`close` `.element` | none | exclude from public typed payload | browser-owned DOM `HTMLElement` (see `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them`) |
| `open`/`close` `.event` | none | exclude from public typed payload | browser-owned DOM `MouseEvent | Event`; exposing it as `object`/`dynamic` would pollute the public DSL |
| `open`/`close` `.model` | none | exclude from public typed payload | broad vendor object (the whole Sidebar instance); the component ref already owns the instance join |
| `open`/`close` `.name` | none | exclude for this row | duplicate event identity metadata; the `Opened`/`Closed` selector already owns the event identity |
| `isOpen` property read | `IsOpen(this ComponentRef<FusionSidebar, TModel> self)` | keep | concise read name returns a typed `bool` source for gather/conditions/set text |
| `show()` method | `Show(this ComponentRef<FusionSidebar, TModel> self)` | keep | states developer intent ("open the panel"); maps to the `show` method call |
| `hide()` method | `Hide(this ComponentRef<FusionSidebar, TModel> self)` | keep | states developer intent ("close the panel"); maps to the `hide` method call |
| `toggle()` method | `Toggle(this ComponentRef<FusionSidebar, TModel> self)` | keep | exact Syncfusion method name; flips the open state |
| `change` event | none | exclude for the current rows | carries only a browser-owned `element` and duplicate `name` and no `isInteracted`; the `open`/`close` pair already covers the transition with the proven `isInteracted` payload |
| `created`, `destroyed` events | none | exclude for the current rows | broad vendor `Object` lifecycle payloads with no typed members |
| `height`, `locale`, `defaultBackdropDiv` | none | exclude | no proven post-render read/write use case; `defaultBackdropDiv` is a browser-owned element |
| `type`, `position`, `width`, `isOpen` (initial), `closeOnDocumentClick`, `animate`, `enableDock`, `enableGestures`, `showBackdrop`, `target`, `zIndex` | none | exclude as builder-owned | configured on `SidebarBuilder` at render through `Html.FusionSidebar(..., b => ...)`; no post-render read/write proven necessary (the `isOpen` read is its own accepted row) |
| `destroy()` method | none | exclude as lifecycle | `probes/raw-ej2-core.html` classifies it `skip: lifecycle cleanup`, not plan behavior |

## Blazor Naming Rule

Blazor metadata may be used only as naming evidence after the EJ2 row is proven.
`discovery/blazor-candidates.md` records that no Syncfusion Blazor package was
supplied for this pass, so every accepted C# name above comes from the EJ2
source and the raw core trace, not from Blazor metadata.

## Discovery Versus C# DSL Boundary

All observed fields stay in discovery. Only fields with clear, typed,
predictable Fusion use cases are accepted into the public C# event args. The
`open`/`close` payload narrows to `IsInteracted` alone; `cancel`, `element`,
`event`, `model`, and `name` remain discovered but excluded as a writable guard
without a proven use case, browser-owned objects, a broad vendor instance, and
duplicate identity metadata. The `change`, `created`, and `destroyed` events and
the builder-owned model properties remain discovered but excluded because the
Syncfusion MVC builder owns initial render configuration and no post-render
read/write beyond `isOpen` is proven necessary.

## Implementation Boundary

Implemented public surface for the Sidebar slice:

- the `FusionSidebar(...)` render helper carrying the controlled component id;
- the `Opened` and `Closed` event selectors and the `FusionSidebarTransitionArgs` payload with `IsInteracted`;
- the `Show()`, `Hide()`, and `Toggle()` method calls;
- the `IsOpen()` read source.

Out of scope for the Sidebar slice: new primitives, builder-owned static
properties, the `change`/`created`/`destroyed` events, and the lifecycle
`destroy` method.
