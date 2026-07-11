# Sidebar Vertical Slice Plan

Status: active and proven. Every accepted `FusionSidebar` member maps to an
exact vertical slice file and the primitive-map row that permits it. The slice
adds zero TypeScript runtime changes. The component is fully audited.

## Accepted Member To File Map

| Accepted C# member | Vertical slice file | Primitive-map row |
| --- | --- | --- |
| `FusionSidebar(this IHtmlHelper<TModel> html, ReactivePlan<TModel> plan, string elementId, Action<SidebarBuilder> build)` | `Alis.Reactive.Fusion/Components/FusionSidebar/FusionSidebarHtmlExtensions.cs` | Sidebar render row |
| `FusionSidebarEvents.Opened` | `Alis.Reactive.Fusion/Components/FusionSidebar/FusionSidebarEvents.cs` | `open` event trigger row |
| `FusionSidebarEvents.Closed` | `Alis.Reactive.Fusion/Components/FusionSidebar/FusionSidebarEvents.cs` | `close` event trigger row |
| `Reactive(...)` event wiring | `Alis.Reactive.Fusion/Components/FusionSidebar/FusionSidebarReactiveExtensions.cs` | `open`/`close` event trigger rows |
| `FusionSidebarTransitionArgs.IsInteracted` | `Alis.Reactive.Fusion/Components/FusionSidebar/Events/FusionSidebarTransitionArgs.cs` | `open`/`close` `.isInteracted` payload read row |
| `Show(this ComponentRef<FusionSidebar, TModel> self)` | `Alis.Reactive.Fusion/Components/FusionSidebar/FusionSidebarExtensions.cs` | `show()` method row |
| `Hide(this ComponentRef<FusionSidebar, TModel> self)` | `Alis.Reactive.Fusion/Components/FusionSidebar/FusionSidebarExtensions.cs` | `hide()` method row |
| `Toggle(this ComponentRef<FusionSidebar, TModel> self)` | `Alis.Reactive.Fusion/Components/FusionSidebar/FusionSidebarExtensions.cs` | `toggle()` method row |
| `IsOpen(this ComponentRef<FusionSidebar, TModel> self)` | `Alis.Reactive.Fusion/Components/FusionSidebar/FusionSidebarExtensions.cs` | `isOpen` property read row |
| component identity (`Vendor`, registration) | `Alis.Reactive.Fusion/Components/FusionSidebar/FusionSidebar.cs` | n/a (component contract) |
| render carrier (`Plan`, `ElementId` join keys) | `Alis.Reactive.Fusion/Components/FusionSidebar/FusionSidebarBuilder.cs` | n/a (render-to-plan carrier) |

## Slice File Inventory

The Sidebar slice follows the component isolation pattern. It does not move
behavior into shared base classes; duplication with other slices is intentional.
The Sidebar is a navigation component, not an input component, so there is no
`IInputComponent` registration and no `Html.InputField` field wrapper.

- `Alis.Reactive.Fusion/Components/FusionSidebar/FusionSidebar.cs` — the sealed `FusionSidebar : FusionComponent` and its vendor identity for the runtime object join.
- `Alis.Reactive.Fusion/Components/FusionSidebar/FusionSidebarHtmlExtensions.cs` — the `FusionSidebar(...)` render helper that renders the `SidebarBuilder` and carries the controlled component id into the plan.
- `Alis.Reactive.Fusion/Components/FusionSidebar/FusionSidebarBuilder.cs` — the rendered-markup carrier that holds the `Plan` and `ElementId` for `.Reactive(...)` chaining.
- `Alis.Reactive.Fusion/Components/FusionSidebar/FusionSidebarExtensions.cs` — the post-render members `IsOpen`, `Show`, `Hide`, and `Toggle`.
- `Alis.Reactive.Fusion/Components/FusionSidebar/FusionSidebarEvents.cs` — the `Opened` and `Closed` event selectors.
- `Alis.Reactive.Fusion/Components/FusionSidebar/FusionSidebarReactiveExtensions.cs` — the `Reactive(...)` event-to-plan wiring through `ComponentEventOnboarding.Wire`.
- `Alis.Reactive.Fusion/Components/FusionSidebar/Events/FusionSidebarTransitionArgs.cs` — the typed `FusionSidebarTransitionArgs` payload with `IsInteracted`.

## Sandbox Surface

The behavior is exercised against a real sandbox view through the typed DSL — a
Resident Care Dashboard whose care-services navigation lives in a slide-out
panel.

- View: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Sidebar/Index.cshtml`
- Controller: `Alis.Reactive.SandboxApp/Areas/Sandbox/Controllers/Components/Fusion/SidebarController.cs`
- Model: `Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Components/Fusion/Sidebar/FusionSidebarModel.cs`
- Route: `http://localhost:5220/Sandbox/Components/FusionSidebar`
- Server endpoints: `POST /Sandbox/Components/FusionSidebar/OpenPanel` (returns the live service list), `POST /Sandbox/Components/FusionSidebar/CloseActivity` (returns the hidden-services note from the gathered `IsOpen()` value)

## Playwright Proof Surface

- Test file: `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Sidebar/WhenUsingFusionSidebar.cs`
- Test class: `Alis.Reactive.PlaywrightTests.Components.Fusion.Sidebar.WhenUsingFusionSidebar`
- Locator: `tests/Alis.Reactive.Playwright.Extensions/FusionSidebarLocator.cs`
- Per-member fails-when-broken mapping: `proof/behavioral-coverage.json`
- Detailed proof table: `proof/playwright-proof.md`

## Sync And Async Lanes

Every Sidebar row is sync: the render is sync markup render; the `open` and
`close` component-event triggers are sync; `Show`, `Hide`, `Toggle`, and the
`IsOpen` read are sync component actions. The slice introduces no async boundary
of its own; async appears only when a developer composes the `IsOpen()` source
into an HTTP pipeline (the close POST), which is the HTTP primitive.

## Out Of Scope

- TypeScript runtime changes (zero, by the vendor-isolation rule).
- New or broadened DSL primitives.
- Builder-owned static properties (`type`, `position`, `width`, `closeOnDocumentClick`, `animate`, `enableDock`, `enableGestures`, `showBackdrop`, `target`, `zIndex`, and the rest).
- The `change`, `created`, and `destroyed` events, and the `destroy` lifecycle method.
