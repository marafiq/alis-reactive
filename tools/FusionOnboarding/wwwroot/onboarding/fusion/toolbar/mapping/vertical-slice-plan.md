# Toolbar Vertical Slice Plan

Status: active and proven. Every accepted `FusionToolbar` member maps to an exact
vertical slice file and the primitive-map row that permits it. The slice adds zero
TypeScript runtime changes. The component is fully audited.

## Accepted Member To File Map

| Accepted C# member | Vertical slice file | Primitive-map row |
| --- | --- | --- |
| `Html.FusionToolbar(plan, id, build)` | `Alis.Reactive.Fusion/Components/FusionToolbar/FusionToolbarHtmlExtensions.cs` | toolbar render helper row |
| `FusionToolbarEvents.Clicked` | `Alis.Reactive.Fusion/Components/FusionToolbar/FusionToolbarEvents.cs` | `clicked` event trigger row |
| `Reactive(...)` event wiring | `Alis.Reactive.Fusion/Components/FusionToolbar/FusionToolbarReactiveExtensions.cs` | `clicked` event trigger row |
| `FusionToolbarClickedArgs` / `FusionToolbarClickedArgs.Item` | `Alis.Reactive.Fusion/Components/FusionToolbar/Events/FusionToolbarClickedArgs.cs` | `clicked` event trigger row |
| `FusionToolbarItem.Id` | `Alis.Reactive.Fusion/Components/FusionToolbar/Events/FusionToolbarClickedArgs.cs` | `clicked.item.id` payload read row |
| `FusionToolbarItem.Text` | `Alis.Reactive.Fusion/Components/FusionToolbar/Events/FusionToolbarClickedArgs.cs` | `clicked.item.text` payload read row |
| `FusionToolbarItem.Disabled` | `Alis.Reactive.Fusion/Components/FusionToolbar/Events/FusionToolbarClickedArgs.cs` | `clicked.item.disabled` payload read row |
| `Disable(this ComponentRef<FusionToolbar, TModel> self, bool value)` | `Alis.Reactive.Fusion/Components/FusionToolbar/FusionToolbarExtensions.cs` | `disable(value)` method row |
| component identity (vendor, registration) | `Alis.Reactive.Fusion/Components/FusionToolbar/FusionToolbar.cs` | n/a (component contract) |

## Slice File Inventory

The Toolbar slice follows the display/command component isolation pattern (it is
not an input component, so it registers no form binding). It does not move
behavior into shared base classes; duplication with other slices is intentional.

- `Alis.Reactive.Fusion/Components/FusionToolbar/FusionToolbar.cs` — the sealed `FusionToolbar : FusionComponent`.
- `Alis.Reactive.Fusion/Components/FusionToolbar/FusionToolbarBuilder.cs` — the typed builder wrapper carrying the component id and plan for reactive chaining.
- `Alis.Reactive.Fusion/Components/FusionToolbar/FusionToolbarHtmlExtensions.cs` — the `FusionToolbar(...)` render helper rendering the Syncfusion `ToolbarBuilder`.
- `Alis.Reactive.Fusion/Components/FusionToolbar/FusionToolbarExtensions.cs` — the post-render `Disable(bool)` method.
- `Alis.Reactive.Fusion/Components/FusionToolbar/FusionToolbarEvents.cs` — the `Clicked` event selector.
- `Alis.Reactive.Fusion/Components/FusionToolbar/FusionToolbarReactiveExtensions.cs` — the `Reactive(...)` event-to-plan wiring through `ComponentEventOnboarding.Wire`.
- `Alis.Reactive.Fusion/Components/FusionToolbar/Events/FusionToolbarClickedArgs.cs` — the typed `FusionToolbarClickedArgs` with `Item`, and `FusionToolbarItem` with `Id`, `Text`, `Disabled`.

## Sandbox Surface

The behavior is exercised against a real sandbox view through the typed DSL — the
resident account command bar.

- View: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Toolbar/Index.cshtml`
- Controller: `Alis.Reactive.SandboxApp/Areas/Sandbox/Controllers/Components/Fusion/ToolbarController.cs`
- Model: `Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Components/Fusion/Toolbar/FusionToolbarModel.cs`
- Route: `http://localhost:5220/Sandbox/Components/FusionToolbar`

## Playwright Proof Surface

- Test file: `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Toolbar/WhenUsingFusionToolbar.cs`
- Test class: `Alis.Reactive.PlaywrightTests.Components.Fusion.Toolbar.WhenUsingFusionToolbar`
- Per-member fails-when-broken mapping: `proof/behavioral-coverage.json`
- Detailed proof table: `proof/playwright-proof.md`

## Sync And Async Lanes

Every Toolbar row is sync: the render is sync; the `clicked` component-event
trigger is sync; `Disable` is a sync component method call. The slice introduces no
async boundary of its own. Async appears only when the Pay-balance journey composes
the clicked payload into an HTTP `Post(...).Gather(...)` pipeline, which is the
HTTP primitive.

## Out Of Scope

- TypeScript runtime changes (zero, by the vendor-isolation rule).
- New or broadened DSL primitives.
- Builder-owned static properties (`items`, `cssClass`, `width`, `height`, `overflowMode`, `scrollStep`, `allowKeyboard`, `enableCollision`, `enableHtmlSanitizer`).
- Per-item `enableItems`/`hideItem` (deferred candidates), item-collection `addItems`/`removeItems`, `refreshOverflow`, the `@hidden` `changeOrientation`, the lifecycle/keyboard events, and the `destroy` lifecycle method — all accounted in `discovery/parity-accounting.json`.
