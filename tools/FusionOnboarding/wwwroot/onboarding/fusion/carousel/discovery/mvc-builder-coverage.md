# FusionCarousel MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `Carousel`
MVC builder: `CarouselBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 32 |
| JS members with matching builder method | 27 |
| JS members without matching builder method | 5 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `AllowKeyboardInteraction` | `System.Boolean` |
| `AnimationEffect` | `Syncfusion.EJ2.Navigations.CarouselAnimationEffect` |
| `AutoPlay` | `System.Boolean` |
| `ButtonsVisibility` | `Syncfusion.EJ2.Navigations.CarouselButtonVisibility` |
| `CssClass` | `System.String` |
| `DataSource` | `System.Object` |
| `EnablePersistence` | `System.Boolean` |
| `EnableRtl` | `System.Boolean` |
| `EnableTouchSwipe` | `System.Boolean` |
| `Height` | `System.String` |
| `Height` | `System.Double` |
| `HtmlAttributes` | `System.Object` |
| `IndicatorsTemplate` | `System.String` |
| `IndicatorsType` | `Syncfusion.EJ2.Navigations.CarouselIndicatorsType` |
| `Interval` | `System.Double` |
| `Items` | `System.Collections.Generic.List{Syncfusion.EJ2.Navigations.CarouselItem}` |
| `ItemTemplate` | `System.String` |
| `Locale` | `System.String` |
| `Loop` | `System.Boolean` |
| `NextButtonTemplate` | `System.String` |
| `PartialVisible` | `System.Boolean` |
| `PauseOnHover` | `System.Boolean` |
| `PlayButtonTemplate` | `System.String` |
| `PreviousButtonTemplate` | `System.String` |
| `SelectedIndex` | `System.Double` |
| `ShowIndicators` | `System.Boolean` |
| `ShowPlayButton` | `System.Boolean` |
| `SlideChanged` | `System.String` |
| `SlideChanging` | `System.String` |
| `SwipeMode` | `Syncfusion.EJ2.Navigations.CarouselSwipeMode` |
| `Width` | `System.String` |
| `Width` | `System.Double` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `allowKeyboardInteraction` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `animationEffect` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `autoPlay` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `buttonsVisibility` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `cssClass` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `dataSource` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `destroy` | method | no | skip: lifecycle cleanup, not Fusion plan behavior |
| `enableTouchSwipe` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `height` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `htmlAttributes` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `indicatorsTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `indicatorsType` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `interval` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `items` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `itemTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `loop` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `next` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `nextButtonTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `partialVisible` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `pause` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `pauseOnHover` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `play` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `playButtonTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `prev` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `previousButtonTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `selectedIndex` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `showIndicators` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `showPlayButton` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `slideChanged` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `slideChanging` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `swipeMode` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `width` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
