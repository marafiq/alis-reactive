# FusionTooltip MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `Tooltip`
MVC builder: `TooltipBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 34 |
| JS members with matching builder method | 28 |
| JS members without matching builder method | 4 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `AfterClose` | `System.String` |
| `AfterOpen` | `System.String` |
| `Animation` | `System.Object` |
| `BeforeClose` | `System.String` |
| `BeforeCollision` | `System.String` |
| `BeforeOpen` | `System.String` |
| `BeforeRender` | `System.String` |
| `CloseDelay` | `System.Double` |
| `Container` | `System.String` |
| `Content` | `System.String` |
| `Created` | `System.String` |
| `CssClass` | `System.String` |
| `Destroyed` | `System.String` |
| `EnableHtmlParse` | `System.Boolean` |
| `EnableHtmlSanitizer` | `System.Boolean` |
| `EnablePersistence` | `System.Boolean` |
| `EnableRtl` | `System.Boolean` |
| `Height` | `System.String` |
| `Height` | `System.Double` |
| `HtmlAttributes` | `System.Object` |
| `IsSticky` | `System.Boolean` |
| `Locale` | `System.String` |
| `MouseTrail` | `System.Boolean` |
| `OffsetX` | `System.Double` |
| `OffsetY` | `System.Double` |
| `OpenDelay` | `System.Double` |
| `OpensOn` | `System.String` |
| `Position` | `Syncfusion.EJ2.Popups.Position` |
| `ShowTipPointer` | `System.Boolean` |
| `Target` | `System.String` |
| `TipPointerPosition` | `Syncfusion.EJ2.Popups.TipPointerPosition` |
| `Width` | `System.String` |
| `Width` | `System.Double` |
| `WindowCollision` | `System.Boolean` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `afterClose` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `afterOpen` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `animation` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `beforeClose` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `beforeCollision` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `beforeOpen` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `beforeRender` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `close` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `closeDelay` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `container` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `content` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `created` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `cssClass` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `destroy` | method | no | skip: lifecycle cleanup, not Fusion plan behavior |
| `destroyed` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `enableHtmlParse` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableHtmlSanitizer` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `height` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `isSticky` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `mouseTrail` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `offsetX` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `offsetY` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `open` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `openDelay` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `opensOn` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `position` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `refresh` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `showTipPointer` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `target` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `tipPointerPosition` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `width` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `windowCollision` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
