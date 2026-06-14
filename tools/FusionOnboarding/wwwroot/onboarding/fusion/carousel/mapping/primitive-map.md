# Carousel Primitive Map

Status: active and proven. This file maps the onboarded `FusionCarousel`
runtime surface: the `selectedIndex` read, the `next`/`prev` navigation methods,
the `slideChanging` and `slideChanged` events with their typed payloads, the
writable `cancel` payload field, and the `FusionCarousel(...)` render helper.
Every mapped row uses an existing DSL primitive. The component is fully audited.

## Pass Rows

Close matrix row: `Html.FusionCarousel(plan, id, b => ...)` -> Carousel render with item slides -> sync render of the Syncfusion `CarouselBuilder`, the controlled component id carried into the plan for `.Reactive(...)` wiring.

Close matrix row: `carousel.Reactive(e => e.SlideChanging, (args, p) => ...)` before-change trigger -> Carousel `slideChanging` payload (`currentIndex`, `nextIndex`, `isSwiped`, `slideDirection`, writable `cancel`) -> sync component-event reaction reading the typed payload and optionally writing `cancel`.

Close matrix row: `carousel.Reactive(e => e.SlideChanged, (args, p) => ...)` after-change trigger -> Carousel `slideChanged` payload (`currentIndex`, `previousIndex`, `isSwiped`, `slideDirection`) -> sync component-event reaction reading the typed payload.

Close matrix row: `p.Component<FusionCarousel>(id).SelectedIndex()` -> Carousel selected-index read source -> sync component property read of `selectedIndex` consumed by conditions and set text.

Close matrix row: `p.Component<FusionCarousel>(id).Next()` / `.Previous()` -> typed Carousel navigation method calls -> sync `next`/`prev` method calls that advance or rewind the active slide.

Close matrix row: `args.PreventTransition(p)` inside a `slideChanging` reaction -> writable `cancel` payload field set to `true` -> sync event-arg set on `PayloadSource.Event()` that cancels the queued slide transition.

## DSL Source Requirements

- `Alis.Reactive.Fusion/Components/FusionCarousel/FusionCarousel.cs`
- `Alis.Reactive.Fusion/Components/FusionCarousel/FusionCarouselHtmlExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionCarousel/FusionCarouselExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionCarousel/FusionCarouselEvents.cs`
- `Alis.Reactive.Fusion/Components/FusionCarousel/FusionCarouselReactiveExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionCarousel/Events/FusionCarouselOnSlideChanging.cs`
- `Alis.Reactive.Fusion/Components/FusionCarousel/Events/FusionCarouselOnSlideChanged.cs`
- `Alis.Reactive/Components/Contracts/ComponentRef.cs`
- `Alis.Reactive/Components/Contracts/ComponentProperty.cs`
- `Alis.Reactive/Components/Contracts/ComponentMember.cs`
- `Alis.Reactive/Components/Onboarding/ComponentEventOnboarding.cs`
- `Alis.Reactive/PlanModel/Reactions/ReactionGraph.cs`
- `Alis.Reactive/PlanModel/Values/ValueExpression.cs`

Sync/async lane expectation: the carousel render is sync; the `slideChanging`
and `slideChanged` component-event triggers are sync; `SelectedIndex` read,
`Next`/`Previous` calls, and the `cancel` payload write are sync component
actions. The Carousel slice introduces no async boundary of its own. Async only
appears when a developer composes a payload read into an HTTP
`Post(...).Gather(...)` pipeline (the chart-record row), which is the HTTP
primitive, not a Carousel concern.

## Authoritative Primitive Rows

| Carousel row member | Syncfusion evidence | DSL primitive | Domain/JSON term | Runtime behavior | Status |
| --- | --- | --- | --- | --- | --- |
| `selectedIndex` property read | `traces/raw-ej2-core.trace.json` constructs `Carousel({ selectedIndex: 0 })`; reads track navigation 0 -> 1 -> 2 -> 1 | `ComponentProperty<int>.Named("selectedIndex")` + `self.Read(property)` | `ValueExpression.Read(ComponentObject, "selectedIndex", Shape.Number)` from `FusionCarouselExtensions.SelectedIndex(...)` | runtime reads `carousel.selectedIndex` into a typed int source | accepted and proven |
| `next()` method | core trace `prototype methods` includes `next`; `after.next.selectedIndex` shows 0 -> 1 | `ComponentMethod.Named("next")` + `self.EmitCall(method)` | `CallReaction` targeting component method `next` | runtime invokes `carousel.next()` and the active slide advances | accepted and proven |
| `prev()` method | core trace `prototype methods` includes `prev`; `after.prev.selectedIndex` shows 2 -> 1 | `ComponentMethod.Named("prev")` + `self.EmitCall(method)` | `CallReaction` targeting component method `prev` | runtime invokes `carousel.prev()` and the active slide rewinds | accepted and proven |
| `slideChanging` event trigger | core trace rows show `slideChanging` firing before each move | `TypedEvent<FusionCarouselSlideChangingArgs>` selected by `.Reactive(...)` | `StartsWhen.ComponentEvent(componentId, "slideChanging")` | runtime wires the Syncfusion object event and starts the reaction with event payload scope | accepted and proven |
| `slideChanging.currentIndex` | core trace `slideChanging` ownKeys include `currentIndex: number` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "currentIndex", Shape.Number)` from `FusionCarouselSlideChangingArgs.CurrentIndex` | runtime reads `event.currentIndex` (section being left) into a condition / set text | accepted and proven |
| `slideChanging.nextIndex` | core trace `slideChanging` ownKeys include `nextIndex: number` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "nextIndex", Shape.Number)` from `FusionCarouselSlideChangingArgs.NextIndex` | runtime reads `event.nextIndex` (target section) into the `Eq(0)` lock guard | accepted and proven |
| `slideChanging.isSwiped` | core trace `slideChanging` ownKeys include `isSwiped: boolean` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "isSwiped", Shape.Boolean)` from `FusionCarouselSlideChangingArgs.IsSwiped` | runtime reads `event.isSwiped` to distinguish a blocked swipe from a blocked button press | accepted and proven |
| `slideChanging.slideDirection` | core trace `slideChanging` ownKeys include `slideDirection` with values `Next`/`Previous` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "slideDirection", Shape.Text)` from `FusionCarouselSlideChangingArgs.SlideDirection` | runtime reads `event.slideDirection` to explain the attempted direction | accepted and proven |
| `slideChanging.cancel` write | core trace `slideChanging.cancel` row sets `cancel: true` and the move is suppressed (selectedIndex stays 1) | `ReactionGraph.Set(PayloadSource.Event(), "cancel", ValueExpression.Literal(true))` via `FusionCarouselSlideChangingArgsExtensions.PreventTransition` | `SetReaction` targeting event-payload `cancel` | runtime sets `event.cancel = true` and the queued slide transition does not happen | accepted and proven |
| `slideChanged` event trigger | core trace rows show `slideChanged` firing after each completed move | `TypedEvent<FusionCarouselSlideChangedArgs>` selected by `.Reactive(...)` | `StartsWhen.ComponentEvent(componentId, "slideChanged")` | runtime wires the Syncfusion object event and starts the reaction with event payload scope | accepted and proven |
| `slideChanged.currentIndex` | core trace `slideChanged` ownKeys include `currentIndex: number` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "currentIndex", Shape.Number)` from `FusionCarouselSlideChangedArgs.CurrentIndex` | runtime reads `event.currentIndex` (section reached) into a condition, set text, and gather | accepted and proven |
| `slideChanged.previousIndex` | core trace `slideChanged` ownKeys include `previousIndex: number` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "previousIndex", Shape.Number)` from `FusionCarouselSlideChangedArgs.PreviousIndex` | runtime reads `event.previousIndex` (section left) into a condition and gather | accepted and proven |
| `slideChanged.isSwiped` | core trace `slideChanged` ownKeys include `isSwiped: boolean` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "isSwiped", Shape.Boolean)` from `FusionCarouselSlideChangedArgs.IsSwiped` | runtime reads `event.isSwiped` to record a button move vs swipe | accepted and proven |
| `slideChanged.slideDirection` | core trace `slideChanged` ownKeys include `slideDirection` with values `Next`/`Previous` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "slideDirection", Shape.Text)` from `FusionCarouselSlideChangedArgs.SlideDirection` | runtime reads `event.slideDirection` to narrate forward/back and to gather | accepted and proven |
| `Html.FusionCarousel(...)` render | `discovery/public-api-surface.json` `items` is `builder.covered = true`; the render helper is the Fusion plan-wiring entry point | render helper carrying the controlled id into the plan | `FusionCarouselHtmlExtensions.FusionCarousel` builds the `CarouselBuilder` and returns `FusionCarouselBuilder<TModel>` | runtime renders the carousel; behavior is wired by `.Reactive(...)` against the controlled id | accepted and proven |
| `slideChanging.currentSlide` / `nextSlide` | core trace `slideChanging` ownKeys include `currentSlide`/`nextSlide` resolved to `[Element#...]` | excluded browser-owned DOM elements | no public C# payload property | runtime must not serialize or expose raw DOM nodes through typed event args | excluded; browser-owned DOM elements, see `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them` |
| `slideChanged.currentSlide` / `previousSlide` | core trace `slideChanged` ownKeys include `currentSlide`/`previousSlide` resolved to `[Element#...]` | excluded browser-owned DOM elements | no public C# payload property | same as above | excluded; browser-owned DOM elements |
| `slideChanging.name` / `slideChanged.name` | `event-payload-surface.json` inherits `BaseEventArgs.name` | excluded duplicate event metadata | no public C# payload property | no runtime mapping | excluded; the event selector already owns event identity |
| `play()` / `pause()` methods | core trace `prototype methods` include `play`/`pause`; `carousel.d.ts:411,417` | autoplay transport methods | no runtime DSL member | runtime never calls them from a plan in this journey | excluded with evidence in `discovery/parity-accounting.json`; autoplay is off and undesirable in a deliberate review |
| `destroy()` method | `discovery/public-api-surface.json` classifies it `skip: lifecycle cleanup` | not a Fusion plan behavior | no runtime DSL member | runtime never calls it from a plan | excluded; lifecycle cleanup, not plan behavior |
| builder-owned static properties (`animationEffect`, `autoPlay`, `buttonsVisibility`, `cssClass`, `dataSource`, `enableTouchSwipe`, `height`/`width`, `htmlAttributes`, `interval`, `items`, templates, `loop`, `partialVisible`, `pauseOnHover`, `selectedIndex` initial, `showIndicators`, `showPlayButton`, `swipeMode`, `allowKeyboardInteraction`, `indicatorsType`) | `discovery/public-api-surface.json` marks each `builder.covered = true` | builder-owned static configuration | no runtime DSL member | initial render configured on `CarouselBuilder` | excluded; builder-owned per `references/automation-gates.md` Gate 5 |

## Primitive Decision

No new primitive is needed for the mapped Carousel rows. Current primitives
already cover every onboarded member:

- component event triggers (`slideChanging`, `slideChanged`);
- event payload reads (`currentIndex`, `previousIndex`, `nextIndex`, `isSwiped`, `slideDirection`);
- event payload write of a literal (`cancel`);
- component property read (`selectedIndex`);
- component method calls (`next`, `prev`).

Any future failure to read one of these accepted members is a
discovery/mapping/typed-contract problem first, never permission to add a
primitive.

## Code To Delete Or Simplify

None identified for the primitive layer. `PreventTransition(p)` keeps the
`cancel` write explicit at the call site rather than hiding it inside the event
wiring, so the cancellation is a mapped row rather than implicit behavior.

## Behavior Proof Required Before Commit

The Carousel rows are proven by
`tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Carousel/WhenUsingFusionCarousel.cs`
through `scripts/playwright.sh`, with each member tied to a fails-when-broken
assertion in `proof/behavioral-coverage.json`:

1. `FusionCarousel(...)` renders the review on the first section;
2. `SelectedIndex()` reports the section on open and after each move;
3. `Next()` / `Previous()` advance and rewind the active slide;
4. `slideChanged` payload (`currentIndex`, `previousIndex`, `slideDirection`, `isSwiped`) narrates the move and feeds the chart gather;
5. `slideChanging` payload (`currentIndex`, `nextIndex`, `slideDirection`, `isSwiped`) drives the lock guard, and `PreventTransition` (the `cancel` write) blocks the move onto the medications section.
