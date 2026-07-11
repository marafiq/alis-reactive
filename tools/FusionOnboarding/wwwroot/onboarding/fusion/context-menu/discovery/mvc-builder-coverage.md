# FusionContextMenu MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `ContextMenu`
MVC builder: `ContextMenuBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 21 |
| JS members with matching builder method | 5 |
| JS members without matching builder method | 2 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `AnimationSettings` | `Syncfusion.EJ2.Navigations.ContextMenuAnimationSettings` |
| `BeforeClose` | `System.String` |
| `BeforeItemRender` | `System.String` |
| `BeforeOpen` | `System.String` |
| `Created` | `System.String` |
| `CssClass` | `System.String` |
| `EnableHtmlSanitizer` | `System.Boolean` |
| `EnablePersistence` | `System.Boolean` |
| `EnableRtl` | `System.Boolean` |
| `EnableScrolling` | `System.Boolean` |
| `Filter` | `System.String` |
| `HoverDelay` | `System.Double` |
| `HtmlAttributes` | `System.Object` |
| `Items` | `System.Object` |
| `ItemTemplate` | `System.String` |
| `Locale` | `System.String` |
| `OnClose` | `System.String` |
| `OnOpen` | `System.String` |
| `Select` | `System.String` |
| `ShowItemOnClick` | `System.Boolean` |
| `Target` | `System.String` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `close` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `enableScrolling` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `filter` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `items` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `itemTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `open` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `target` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
