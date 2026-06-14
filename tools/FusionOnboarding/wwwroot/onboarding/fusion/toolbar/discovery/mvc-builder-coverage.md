# FusionToolbar MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `Toolbar`
MVC builder: `ToolbarBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 20 |
| JS members with matching builder method | 14 |
| JS members without matching builder method | 8 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `AllowKeyboard` | `System.Boolean` |
| `BeforeCreate` | `System.String` |
| `Clicked` | `System.String` |
| `Created` | `System.String` |
| `CssClass` | `System.String` |
| `Destroyed` | `System.String` |
| `EnableCollision` | `System.Boolean` |
| `EnableHtmlSanitizer` | `System.Boolean` |
| `EnablePersistence` | `System.Boolean` |
| `EnableRtl` | `System.Boolean` |
| `Height` | `System.String` |
| `Height` | `System.Double` |
| `HtmlAttributes` | `System.Object` |
| `Items` | `System.Collections.Generic.List{Syncfusion.EJ2.Navigations.ToolbarItem}` |
| `KeyDown` | `System.String` |
| `Locale` | `System.String` |
| `OverflowMode` | `Syncfusion.EJ2.Navigations.OverflowMode` |
| `ScrollStep` | `System.Double` |
| `Width` | `System.String` |
| `Width` | `System.Double` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `addItems` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `allowKeyboard` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `beforeCreate` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `changeOrientation` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `clicked` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `created` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `cssClass` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `destroy` | method | no | skip: lifecycle cleanup, not Fusion plan behavior |
| `destroyed` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `disable` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `enableCollision` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableHtmlSanitizer` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableItems` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `height` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `hideItem` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `items` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `keyDown` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `overflowMode` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `refreshOverflow` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `removeItems` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `scrollStep` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `width` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
