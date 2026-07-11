# Accordion Vertical Slice Plan

Status: active and proven. Every accepted `FusionAccordion` member maps to an exact
vertical slice file and the primitive-map row that permits it. The slice adds zero
TypeScript runtime changes. `FusionAccordion` is a non-input container component, so it
does not implement the input registration pattern. The component is fully audited.

## Accepted Member To File Map

| Accepted C# member | Vertical slice file | Primitive-map row |
| --- | --- | --- |
| `FusionAccordion(#if NET48 this HtmlHelper<TModel> html, #else this IHtmlHelper<TModel> html, #endif ReactivePlan<TModel> plan, string elementId, Action<AccordionBuilder> build)` | `Alis.Reactive.Fusion/Components/FusionAccordion/FusionAccordionHtmlExtensions.cs` | accordion render helper row |
| `FusionAccordionEvents.Expanded` | `Alis.Reactive.Fusion/Components/FusionAccordion/FusionAccordionEvents.cs` | `expanded` event trigger row |
| `Reactive(...)` event wiring | `Alis.Reactive.Fusion/Components/FusionAccordion/FusionAccordionReactiveExtensions.cs` | `expanded` event trigger row |
| `FusionAccordionExpandedArgs.Index` | `Alis.Reactive.Fusion/Components/FusionAccordion/Events/FusionAccordionOnExpanded.cs` | `expanded.index` payload read row |
| `FusionAccordionExpandedArgs.IsExpanded` | `Alis.Reactive.Fusion/Components/FusionAccordion/Events/FusionAccordionOnExpanded.cs` | `expanded.isExpanded` payload read row |
| `ExpandItem(this ComponentRef<FusionAccordion, TModel> self, bool isExpand, int index)` | `Alis.Reactive.Fusion/Components/FusionAccordion/FusionAccordionExtensions.cs` | `expandItem(isExpand, index)` method row |
| `EnableItem(this ComponentRef<FusionAccordion, TModel> self, int index, bool isEnable = true)` | `Alis.Reactive.Fusion/Components/FusionAccordion/FusionAccordionExtensions.cs` | `enableItem(index, isEnable)` method row |
| component identity (vendor, render builder) | `Alis.Reactive.Fusion/Components/FusionAccordion/FusionAccordion.cs`, `Alis.Reactive.Fusion/Components/FusionAccordion/FusionAccordionBuilder.cs` | n/a (component contract) |

## Slice File Inventory

The Accordion slice follows the component isolation pattern. It does not move behavior
into shared base classes; duplication with other slices is intentional.

- `Alis.Reactive.Fusion/Components/FusionAccordion/FusionAccordion.cs` — the sealed `FusionAccordion : FusionComponent` container component (no input registration).
- `Alis.Reactive.Fusion/Components/FusionAccordion/FusionAccordionBuilder.cs` — the `IHtmlContent`/`IHtmlString` wrapper that carries the rendered markup plus plan + element id for `.Reactive` chaining.
- `Alis.Reactive.Fusion/Components/FusionAccordion/FusionAccordionHtmlExtensions.cs` — the `FusionAccordion(...)` render helper that builds the `AccordionBuilder` and carries the controlled id.
- `Alis.Reactive.Fusion/Components/FusionAccordion/FusionAccordionExtensions.cs` — the post-render methods `ExpandItem` and `EnableItem`, each a typed two-argument `ComponentMethod`.
- `Alis.Reactive.Fusion/Components/FusionAccordion/FusionAccordionEvents.cs` — the `Expanded` event selector.
- `Alis.Reactive.Fusion/Components/FusionAccordion/FusionAccordionReactiveExtensions.cs` — the `Reactive(...)` event-to-plan wiring through `ComponentEventOnboarding.Wire`.
- `Alis.Reactive.Fusion/Components/FusionAccordion/Events/FusionAccordionOnExpanded.cs` — the typed `FusionAccordionExpandedArgs` payload with `Index` and `IsExpanded`.

## Sandbox Surface

The behavior is exercised against a real sandbox view through the typed DSL — the
resident "My Care Plan" journey, with real-app elements only (no echo spans, no debug
panels).

- View: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Accordion/Index.cshtml`
- Lazy-load partial: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Accordion/_MonthlyChargesPartial.cshtml`
- Controller: `Alis.Reactive.SandboxApp/Areas/Sandbox/Controllers/Components/Fusion/AccordionController.cs`
- Model: `Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Components/Fusion/Accordion/AccordionModel.cs`
- Route: `http://localhost:5220/Sandbox/Components/Accordion`

## Playwright Proof Surface

- Test file: `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Accordion/WhenUsingFusionAccordion.cs`
- Test class: `Alis.Reactive.PlaywrightTests.Components.Fusion.Accordion.WhenUsingFusionAccordion`
- Locator: `tests/Alis.Reactive.Playwright.Extensions/FusionAccordionLocator.cs`
- Per-member fails-when-broken mapping: `proof/behavioral-coverage.json`
- Detailed proof table: `proof/playwright-proof.md`

## Sync And Async Lanes

Every Accordion row is sync: the render is sync; the `expanded` component event trigger
is sync; `ExpandItem` and `EnableItem` are sync component method calls. The slice
introduces no async boundary of its own; async appears only when a developer composes
the `expanded` event into an HTTP pipeline (the charges lazy-load-on-expand journey),
which is the HTTP primitive.

## Out Of Scope

- TypeScript runtime changes (zero, by the vendor-isolation rule).
- New or broadened DSL primitives.
- Builder-owned static properties (`animation`, `dataSource`, `expandMode`, `expandedIndices`, `items`, templates, `height`, `width`, and the rest).
- The `expanding`/`clicked`/`created`/`destroyed` events.
- The structural `addItem`/`removeItem` methods, the `hideItem`/`select` methods, and the lifecycle `destroy` method.
