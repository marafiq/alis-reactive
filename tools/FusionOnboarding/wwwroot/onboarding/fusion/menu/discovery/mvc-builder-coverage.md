# FusionMenu MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `Menu`
MVC builder: `MenuBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 25 |
| JS members with matching builder method | 8 |
| JS members without matching builder method | 2 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `AnimationSettings` | `Syncfusion.EJ2.Navigations.MenuAnimationSettings` |
| `BeforeClose` | `System.String` |
| `BeforeItemRender` | `System.String` |
| `BeforeOpen` | `System.String` |
| `Created` | `System.String` |
| `CssClass` | `System.String` |
| `EnableHtmlSanitizer` | `System.Boolean` |
| `EnablePersistence` | `System.Boolean` |
| `EnableRtl` | `System.Boolean` |
| `EnableScrolling` | `System.Boolean` |
| `Fields` | `Syncfusion.EJ2.Navigations.MenuFieldSettings` |
| `HamburgerMode` | `System.Boolean` |
| `HoverDelay` | `System.Double` |
| `HtmlAttributes` | `System.Object` |
| `Items` | `System.Object` |
| `Items` | `System.Collections.Generic.List{Syncfusion.EJ2.Navigations.MenuItem}` |
| `Locale` | `System.String` |
| `OnClose` | `System.String` |
| `OnOpen` | `System.String` |
| `Orientation` | `Syncfusion.EJ2.Navigations.Orientation` |
| `Select` | `System.String` |
| `ShowItemOnClick` | `System.Boolean` |
| `Target` | `System.String` |
| `Template` | `System.String` |
| `Title` | `System.String` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `close` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `enableHtmlSanitizer` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableScrolling` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `fields` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `hamburgerMode` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `open` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `orientation` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `target` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `template` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `title` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
