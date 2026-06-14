# Carousel C# Name Decisions

Status: active and proven. The `FusionCarousel` public C# names are decided and
implemented: the `FusionCarousel(...)` render helper, the `SlideChanging` and
`SlideChanged` event selectors with the `FusionCarouselSlideChangingArgs` and
`FusionCarouselSlideChangedArgs` payloads, the `PreventTransition()` event-arg
operation, and the `SelectedIndex()` read with the `Next()`/`Previous()`
navigation methods. The component is fully audited.

## Pass Rows

Close matrix row: `Html.FusionCarousel(plan, id, b => ...)` render helper -> Carousel rendered with item slides, controlled id carried into the plan.

Close matrix row: `carousel.Reactive(e => e.SlideChanging, ...)` -> typed `FusionCarouselSlideChangingArgs` payload with the writable `Cancel` field and `PreventTransition()`.

Close matrix row: `carousel.Reactive(e => e.SlideChanged, ...)` -> typed `FusionCarouselSlideChangedArgs` payload.

Close matrix row: `SelectedIndex()`, `Next()`, `Previous()` -> typed Carousel runtime members.

## Evidence Inputs

- Raw core trace: `traces/raw-ej2-core.trace.json`
- Raw core probe: `probes/raw-ej2-core.html`
- Syncfusion source types: `SlideChangingEventArgs`, `SlideChangedEventArgs` (events), `Carousel` (component)
- Public API surface: `discovery/public-api-surface.json`
- Event payload surface: `discovery/event-payload-surface.json`
- Parity accounting (play/pause exclusion judgment): `discovery/parity-accounting.json`
- Blazor candidates: `discovery/blazor-candidates.md` (no Blazor package supplied; naming taken from EJ2 source only)
- Existing C# event args: `Alis.Reactive.Fusion/Components/FusionCarousel/Events/FusionCarouselOnSlideChanging.cs`, `.../Events/FusionCarouselOnSlideChanged.cs`
- Existing event selectors: `Alis.Reactive.Fusion/Components/FusionCarousel/FusionCarouselEvents.cs`
- Existing component members: `Alis.Reactive.Fusion/Components/FusionCarousel/FusionCarouselExtensions.cs`
- Existing render helper: `Alis.Reactive.Fusion/Components/FusionCarousel/FusionCarouselHtmlExtensions.cs`
- Existing sandbox usage: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Carousel/Index.cshtml`

## Name Decision Matrix

| Syncfusion path | C# name | Decision | Reason |
| --- | --- | --- | --- |
| `html.EJS().Carousel(id)` render | `IHtmlHelper<TModel>.FusionCarousel(ReactivePlan<TModel>, string elementId, Action<CarouselBuilder>)` | keep | render helper wraps the Syncfusion `CarouselBuilder` and carries the controlled id into the plan for `.Reactive(...)`; initial options stay on `CarouselBuilder` |
| `slideChanging` event | `FusionCarouselEvents.SlideChanging` | keep | exact Syncfusion event name; selected through the typed `.Reactive(e => e.SlideChanging, ...)` lambda |
| `slideChanged` event | `FusionCarouselEvents.SlideChanged` | keep | exact Syncfusion event name; selected through the typed `.Reactive(e => e.SlideChanged, ...)` lambda |
| `SlideChangingEventArgs` | `FusionCarouselSlideChangingArgs` | keep | the Fusion payload type name states the event it belongs to; carries only the proven, narrowed members |
| `SlideChangedEventArgs` | `FusionCarouselSlideChangedArgs` | keep | same naming rule for the after-change payload |
| `slideChanging.currentIndex` | `FusionCarouselSlideChangingArgs.CurrentIndex` | keep | exact Syncfusion key, typed as `int`; the section being left |
| `slideChanging.nextIndex` | `FusionCarouselSlideChangingArgs.NextIndex` | keep | exact Syncfusion key, typed as `int`; the section the move would land on (drives the lock guard) |
| `slideChanging.isSwiped` | `FusionCarouselSlideChangingArgs.IsSwiped` | keep | exact Syncfusion key, typed as `bool`; swipe vs button for the blocked gesture |
| `slideChanging.slideDirection` | `FusionCarouselSlideChangingArgs.SlideDirection` | keep | exact Syncfusion key, typed as `string`; serialized enum values `Next`/`Previous` |
| `slideChanging.cancel` (writable) + setting `cancel = true` | `FusionCarouselSlideChangingArgs.Cancel` + `PreventTransition(IReactionEmitter)` | keep | `Cancel` is the exact writable Syncfusion key; `PreventTransition()` names developer intent and emits the `cancel = true` set rather than exposing the raw member string |
| `slideChanged.currentIndex` | `FusionCarouselSlideChangedArgs.CurrentIndex` | keep | exact Syncfusion key, typed `int`; the section reached |
| `slideChanged.previousIndex` | `FusionCarouselSlideChangedArgs.PreviousIndex` | keep | exact Syncfusion key, typed `int`; the section left |
| `slideChanged.isSwiped` | `FusionCarouselSlideChangedArgs.IsSwiped` | keep | exact Syncfusion key, typed `bool`; swipe vs button for the completed move |
| `slideChanged.slideDirection` | `FusionCarouselSlideChangedArgs.SlideDirection` | keep | exact Syncfusion key, typed `string`; serialized enum values `Next`/`Previous` |
| `selectedIndex` property read | `SelectedIndex(this ComponentRef<FusionCarousel, TModel> self)` | keep | concise read name returns a typed `int` source for conditions and set text |
| `next()` method | `Next(this ComponentRef<FusionCarousel, TModel> self)` | keep | exact Syncfusion method name; advances one slide |
| `prev()` method | `Previous(this ComponentRef<FusionCarousel, TModel> self)` | keep | clearer developer-facing name than `prev`; maps to the `prev` method path |
| `slideChanging.currentSlide`/`nextSlide`, `slideChanged.currentSlide`/`previousSlide` | none | exclude from public typed payload | browser-owned DOM `HTMLElement`s; exposing them as `object`/`dynamic` would pollute the public DSL (`_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them`) |
| `slideChanging.name` / `slideChanged.name` (inherited `BaseEventArgs.name`) | none | exclude for these rows | duplicate event identity metadata; the event selector already owns event identity |
| `play()` / `pause()` methods | none | exclude with evidence | autoplay transport; `discovery/parity-accounting.json` records the source-grounded reason (autoplay is off and undesirable in a deliberate review) |
| `destroy()` method | none | exclude as lifecycle | `discovery/public-api-surface.json` classifies it `skip: lifecycle cleanup`, not plan behavior |
| builder-owned static properties (`animationEffect`, `autoPlay`, `buttonsVisibility`, `cssClass`, `dataSource`, `enableTouchSwipe`, `height`/`width`, `htmlAttributes`, `interval`, `items`, templates, `loop`, `partialVisible`, `pauseOnHover`, `showIndicators`, `showPlayButton`, `swipeMode`, `allowKeyboardInteraction`, `indicatorsType`) | none | exclude as builder-owned | `discovery/public-api-surface.json` marks each `builder.covered = true`; configured on `CarouselBuilder` at initial render, no post-render read/write proven necessary |

## Blazor Naming Rule

Blazor metadata may be used only as naming evidence after the EJ2 row is proven.
`discovery/blazor-candidates.md` records that no Syncfusion Blazor package was
supplied for this pass, so every accepted C# name above comes from the EJ2
source and the raw core trace, not from Blazor metadata.

## Discovery Versus C# DSL Boundary

All observed fields stay in discovery. Only fields with clear, typed,
predictable Fusion use cases are accepted into the public C# event args. The
DOM-element payload fields (`currentSlide`, `nextSlide`, `previousSlide`) remain
discovered but excluded because they are browser-owned `HTMLElement`s. The
builder-covered properties remain discovered but excluded because the Syncfusion
MVC builder owns initial render configuration and no post-render read/write is
proven necessary. `play`/`pause` remain discovered but excluded because autoplay
transport has no role in the deliberate, manually-navigated review.

## Implementation Boundary

Implemented public surface for the Carousel slice:

- the `FusionCarousel(...)` render helper carrying the controlled id;
- the `SlideChanging` selector and `FusionCarouselSlideChangingArgs` payload with `CurrentIndex`, `NextIndex`, `IsSwiped`, `SlideDirection`, the writable `Cancel`, and `PreventTransition()`;
- the `SlideChanged` selector and `FusionCarouselSlideChangedArgs` payload with `CurrentIndex`, `PreviousIndex`, `IsSwiped`, `SlideDirection`;
- the `SelectedIndex()` read source and the `Next()`/`Previous()` navigation methods.

Out of scope for the Carousel slice: new primitives, builder-owned static
properties, the autoplay `play`/`pause` methods, the DOM-element payload fields,
and the lifecycle `destroy` method.
