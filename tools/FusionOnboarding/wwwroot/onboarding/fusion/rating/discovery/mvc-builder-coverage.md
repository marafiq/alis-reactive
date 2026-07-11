# FusionRating MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `Rating`
MVC builder: `RatingBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 26 |
| JS members with matching builder method | 22 |
| JS members without matching builder method | 2 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `AllowReset` | `System.Boolean` |
| `BeforeItemRender` | `System.String` |
| `Created` | `System.String` |
| `CssClass` | `System.String` |
| `Disabled` | `System.Boolean` |
| `EmptyTemplate` | `System.String` |
| `EnableAnimation` | `System.Boolean` |
| `EnablePersistence` | `System.Boolean` |
| `EnableRtl` | `System.Boolean` |
| `EnableSingleSelection` | `System.Boolean` |
| `FullTemplate` | `System.String` |
| `HtmlAttributes` | `System.Object` |
| `ItemsCount` | `System.Int32` |
| `LabelPosition` | `Syncfusion.EJ2.Inputs.LabelPosition` |
| `LabelTemplate` | `System.String` |
| `Locale` | `System.String` |
| `Min` | `System.Double` |
| `OnItemHover` | `System.String` |
| `Precision` | `Syncfusion.EJ2.Inputs.PrecisionType` |
| `ReadOnly` | `System.Boolean` |
| `ShowLabel` | `System.Boolean` |
| `ShowTooltip` | `System.Boolean` |
| `TooltipTemplate` | `System.String` |
| `Value` | `System.Double` |
| `ValueChanged` | `System.String` |
| `Visible` | `System.Boolean` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `allowReset` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `beforeItemRender` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `created` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `cssClass` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `destroy` | method | no | skip: lifecycle cleanup, not Fusion plan behavior |
| `disabled` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `emptyTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableAnimation` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableSingleSelection` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `fullTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `itemsCount` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `labelPosition` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `labelTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `min` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `onItemHover` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `precision` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `readOnly` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `reset` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `showLabel` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `showTooltip` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `tooltipTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `value` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `valueChanged` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `visible` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
