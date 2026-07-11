# Carousel Vertical Slice Plan

Status: active and proven. Every accepted `FusionCarousel` member maps to an
exact vertical slice file and the primitive-map row that permits it. The slice
adds zero TypeScript runtime changes. The component is fully audited.

## Accepted Member To File Map

| Accepted C# member | Vertical slice file | Primitive-map row |
| --- | --- | --- |
| `FusionCarousel(...)` render helper | `Alis.Reactive.Fusion/Components/FusionCarousel/FusionCarouselHtmlExtensions.cs` | Carousel render row |
| `FusionCarouselEvents.SlideChanging` | `Alis.Reactive.Fusion/Components/FusionCarousel/FusionCarouselEvents.cs` | `slideChanging` event trigger row |
| `FusionCarouselEvents.SlideChanged` | `Alis.Reactive.Fusion/Components/FusionCarousel/FusionCarouselEvents.cs` | `slideChanged` event trigger row |
| `Reactive(...)` event wiring | `Alis.Reactive.Fusion/Components/FusionCarousel/FusionCarouselReactiveExtensions.cs` | both event trigger rows |
| `FusionCarouselSlideChangingArgs.CurrentIndex` | `Alis.Reactive.Fusion/Components/FusionCarousel/Events/FusionCarouselOnSlideChanging.cs` | `slideChanging.currentIndex` payload read row |
| `FusionCarouselSlideChangingArgs.NextIndex` | `Alis.Reactive.Fusion/Components/FusionCarousel/Events/FusionCarouselOnSlideChanging.cs` | `slideChanging.nextIndex` payload read row |
| `FusionCarouselSlideChangingArgs.IsSwiped` | `Alis.Reactive.Fusion/Components/FusionCarousel/Events/FusionCarouselOnSlideChanging.cs` | `slideChanging.isSwiped` payload read row |
| `FusionCarouselSlideChangingArgs.SlideDirection` | `Alis.Reactive.Fusion/Components/FusionCarousel/Events/FusionCarouselOnSlideChanging.cs` | `slideChanging.slideDirection` payload read row |
| `FusionCarouselSlideChangingArgs.Cancel` + `PreventTransition()` | `Alis.Reactive.Fusion/Components/FusionCarousel/Events/FusionCarouselOnSlideChanging.cs` | `slideChanging.cancel` write row |
| `FusionCarouselSlideChangedArgs.CurrentIndex` | `Alis.Reactive.Fusion/Components/FusionCarousel/Events/FusionCarouselOnSlideChanged.cs` | `slideChanged.currentIndex` payload read row |
| `FusionCarouselSlideChangedArgs.PreviousIndex` | `Alis.Reactive.Fusion/Components/FusionCarousel/Events/FusionCarouselOnSlideChanged.cs` | `slideChanged.previousIndex` payload read row |
| `FusionCarouselSlideChangedArgs.IsSwiped` | `Alis.Reactive.Fusion/Components/FusionCarousel/Events/FusionCarouselOnSlideChanged.cs` | `slideChanged.isSwiped` payload read row |
| `FusionCarouselSlideChangedArgs.SlideDirection` | `Alis.Reactive.Fusion/Components/FusionCarousel/Events/FusionCarouselOnSlideChanged.cs` | `slideChanged.slideDirection` payload read row |
| `SelectedIndex(this ComponentRef<FusionCarousel, TModel> self)` | `Alis.Reactive.Fusion/Components/FusionCarousel/FusionCarouselExtensions.cs` | `selectedIndex` property read row |
| `Next(this ComponentRef<FusionCarousel, TModel> self)` | `Alis.Reactive.Fusion/Components/FusionCarousel/FusionCarouselExtensions.cs` | `next()` method row |
| `Previous(this ComponentRef<FusionCarousel, TModel> self)` | `Alis.Reactive.Fusion/Components/FusionCarousel/FusionCarouselExtensions.cs` | `prev()` method row |
| component identity (vendor, type) | `Alis.Reactive.Fusion/Components/FusionCarousel/FusionCarousel.cs` | n/a (component contract) |

## Slice File Inventory

The Carousel slice follows the display/navigation-component isolation pattern.
It does not register an input binding, and it does not move behavior into shared
base classes; duplication with other slices is intentional.

- `Alis.Reactive.Fusion/Components/FusionCarousel/FusionCarousel.cs` — the sealed `FusionCarousel : FusionComponent` (navigation/display component, not an input component).
- `Alis.Reactive.Fusion/Components/FusionCarousel/FusionCarouselBuilder.cs` — the `FusionCarouselBuilder<TModel>` that carries plan metadata for `.Reactive(...)` chaining while rendering the Syncfusion markup.
- `Alis.Reactive.Fusion/Components/FusionCarousel/FusionCarouselHtmlExtensions.cs` — the `FusionCarousel(...)` render helper.
- `Alis.Reactive.Fusion/Components/FusionCarousel/FusionCarouselExtensions.cs` — the post-render members `SelectedIndex`, `Next`, `Previous`.
- `Alis.Reactive.Fusion/Components/FusionCarousel/FusionCarouselEvents.cs` — the `SlideChanging` and `SlideChanged` event selectors.
- `Alis.Reactive.Fusion/Components/FusionCarousel/FusionCarouselReactiveExtensions.cs` — the `Reactive(...)` event-to-plan wiring through `ComponentEventOnboarding.Wire`.
- `Alis.Reactive.Fusion/Components/FusionCarousel/Events/FusionCarouselOnSlideChanging.cs` — the typed `FusionCarouselSlideChangingArgs` payload and `PreventTransition()`.
- `Alis.Reactive.Fusion/Components/FusionCarousel/Events/FusionCarouselOnSlideChanged.cs` — the typed `FusionCarouselSlideChangedArgs` payload.

## Sandbox Surface

The behavior is exercised against a real sandbox view through the typed DSL: a
Guided Care-Plan Review where a nurse walks a resident through care-plan
sections.

- View: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Carousel/Index.cshtml`
- Controller: `Alis.Reactive.SandboxApp/Areas/Sandbox/Controllers/Components/Fusion/CarouselController.cs`
- Model: `Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Components/Fusion/Carousel/FusionCarouselModel.cs`
- Route: `http://localhost:5220/Sandbox/Components/Carousel`

## Playwright Proof Surface

- Test file: `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Carousel/WhenUsingFusionCarousel.cs`
- Test class: `Alis.Reactive.PlaywrightTests.Components.Fusion.Carousel.WhenUsingFusionCarousel`
- Locator: `tests/Alis.Reactive.Playwright.Extensions/FusionCarouselLocator.cs`
- Per-member fails-when-broken mapping: `proof/behavioral-coverage.json`
- Detailed proof table: `proof/playwright-proof.md`

## Sync And Async Lanes

Every Carousel row is sync: the render is sync; the `slideChanging` and
`slideChanged` component-event triggers are sync; `SelectedIndex` read,
`Next`/`Previous` calls, and the `cancel` payload write (`PreventTransition`) are
sync component actions. The slice introduces no async boundary of its own; async
appears only when a developer composes a payload read into an HTTP gather (the
chart-record row), which is the HTTP primitive.

## Out Of Scope

- TypeScript runtime changes (zero, by the vendor-isolation rule).
- New or broadened DSL primitives.
- Builder-owned static properties (`animationEffect`, `autoPlay`, `buttonsVisibility`, `items`, templates, `loop`, and the rest).
- The autoplay `play`/`pause` methods and the lifecycle `destroy` method.
- The DOM-element payload fields (`currentSlide`, `nextSlide`, `previousSlide`).
