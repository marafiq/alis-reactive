# Breadcrumb Vertical Slice Plan

Status: active and proven. Every accepted `FusionBreadcrumb` member maps to an
exact vertical slice file and the primitive-map row that permits it. The slice adds
zero TypeScript runtime changes. The component is fully audited.

## Accepted Member To File Map

| Accepted C# member | Vertical slice file | Primitive-map row |
| --- | --- | --- |
| `FusionBreadcrumb(...)` render helper | `Alis.Reactive.Fusion/Components/FusionBreadcrumb/FusionBreadcrumbHtmlExtensions.cs` | trail render helper row |
| `FusionBreadcrumbEvents.ItemClick` | `Alis.Reactive.Fusion/Components/FusionBreadcrumb/FusionBreadcrumbEvents.cs` | `itemClick` event trigger row |
| `Reactive(...)` event wiring | `Alis.Reactive.Fusion/Components/FusionBreadcrumb/FusionBreadcrumbReactiveExtensions.cs` | `itemClick` event trigger row |
| `FusionBreadcrumbItemClickArgs` | `Alis.Reactive.Fusion/Components/FusionBreadcrumb/Events/FusionBreadcrumbOnItemClick.cs` | `itemClick.item` payload read row |
| `FusionBreadcrumbItemClickArgs.Item` (`FusionBreadcrumbItem`) | `Alis.Reactive.Fusion/Components/FusionBreadcrumb/Events/FusionBreadcrumbOnItemClick.cs` | `itemClick.item` payload read row |
| `FusionBreadcrumbItem.Text` | `Alis.Reactive.Fusion/Components/FusionBreadcrumb/Events/FusionBreadcrumbOnItemClick.cs` | `itemClick.item.text` payload read row |
| `FusionBreadcrumbItem.Id` | `Alis.Reactive.Fusion/Components/FusionBreadcrumb/Events/FusionBreadcrumbOnItemClick.cs` | `itemClick.item.id` payload read row |
| `FusionBreadcrumbItem.Url` | `Alis.Reactive.Fusion/Components/FusionBreadcrumb/Events/FusionBreadcrumbOnItemClick.cs` | `itemClick.item.url` payload read row |
| `FusionBreadcrumbItem.IconCss` | `Alis.Reactive.Fusion/Components/FusionBreadcrumb/Events/FusionBreadcrumbOnItemClick.cs` | `itemClick.item.iconCss` payload read row |
| `FusionBreadcrumbItem.Disabled` | `Alis.Reactive.Fusion/Components/FusionBreadcrumb/Events/FusionBreadcrumbOnItemClick.cs` | `itemClick.item.disabled` payload read row |
| `ActiveItem(this ComponentRef<FusionBreadcrumb, TModel> self)` | `Alis.Reactive.Fusion/Components/FusionBreadcrumb/FusionBreadcrumbExtensions.cs` | `activeItem` property read row |
| `SetActiveItem(this ComponentRef<FusionBreadcrumb, TModel> self, string activeItem)` | `Alis.Reactive.Fusion/Components/FusionBreadcrumb/FusionBreadcrumbExtensions.cs` | `activeItem` property write row plus `dataBind` repaint row |
| component identity (vendor, type) | `Alis.Reactive.Fusion/Components/FusionBreadcrumb/FusionBreadcrumb.cs` | n/a (component contract) |

## Slice File Inventory

The Breadcrumb slice follows the display-component isolation pattern (it is not an
input component: it registers no form binding). It does not move behavior into
shared base classes; duplication with other slices is intentional.

- `Alis.Reactive.Fusion/Components/FusionBreadcrumb/FusionBreadcrumb.cs` — the sealed `FusionBreadcrumb : FusionComponent`, the active-item/item-click display component.
- `Alis.Reactive.Fusion/Components/FusionBreadcrumb/FusionBreadcrumbBuilder.cs` — the typed builder wrapper that carries plan metadata for `.Reactive(...)` chaining.
- `Alis.Reactive.Fusion/Components/FusionBreadcrumb/FusionBreadcrumbHtmlExtensions.cs` — the `FusionBreadcrumb(...)` render helper that renders the `BreadcrumbBuilder` and carries the controlled component id into the plan.
- `Alis.Reactive.Fusion/Components/FusionBreadcrumb/FusionBreadcrumbExtensions.cs` — the post-render members `ActiveItem` (read) and `SetActiveItem` (write plus `dataBind`).
- `Alis.Reactive.Fusion/Components/FusionBreadcrumb/FusionBreadcrumbEvents.cs` — the `ItemClick` event selector.
- `Alis.Reactive.Fusion/Components/FusionBreadcrumb/FusionBreadcrumbReactiveExtensions.cs` — the `Reactive(...)` event-to-plan wiring through `ComponentEventOnboarding.Wire`.
- `Alis.Reactive.Fusion/Components/FusionBreadcrumb/Events/FusionBreadcrumbOnItemClick.cs` — the typed `FusionBreadcrumbItemClickArgs` payload with the nested `FusionBreadcrumbItem` (`Text`, `Id`, `Url`, `IconCss`, `Disabled`).

## Sandbox Surface

The behavior is exercised against a real sandbox view through the typed DSL — a
care coordinator stepping up a resident's care record. Real-app elements only; no
echo spans, no Plan-JSON panel, no debug buttons.

- View: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Breadcrumb/Index.cshtml`
- Controller: `Alis.Reactive.SandboxApp/Areas/Sandbox/Controllers/Components/Fusion/BreadcrumbController.cs`
- Model: `Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Components/Fusion/Breadcrumb/FusionBreadcrumbModel.cs`
- Route: `http://localhost:5220/Sandbox/Components/CareRecordBreadcrumb`

## Playwright Proof Surface

- Test file: `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Breadcrumb/WhenUsingFusionBreadcrumb.cs`
- Test class: `Alis.Reactive.PlaywrightTests.Components.Fusion.Breadcrumb.WhenUsingFusionBreadcrumb`
- Per-member fails-when-broken mapping: `proof/behavioral-coverage.json`
- Detailed proof table: `proof/playwright-proof.md`

## Sync And Async Lanes

Every Breadcrumb row is sync: the trail render is sync; the `itemClick` component
event trigger is sync; `ActiveItem()` read and `SetActiveItem` (property set plus
`dataBind`) are sync component actions. The slice introduces no async boundary of
its own; async appears only when a developer composes a clicked crumb's payload
value into an HTTP `Post(...).Gather(...)` pipeline, which is the HTTP primitive
(the sandbox journey does exactly this to load each section's summary).

## Out Of Scope

- TypeScript runtime changes (zero, by the vendor-isolation rule).
- New or broadened DSL primitives.
- Builder-owned static properties (`cssClass`, `disabled`, `enableActiveItemNavigation`, `enableNavigation`, `items`, `itemTemplate`, `maxItems`, `overflowMode`, `separatorTemplate`, `url`).
- The `beforeItemRender`/`created` events, the browser-owned `element`/`event` payload objects, the `cancel`/`name` payload metadata, the vendor-private `locale`, and the `destroy` lifecycle method.
