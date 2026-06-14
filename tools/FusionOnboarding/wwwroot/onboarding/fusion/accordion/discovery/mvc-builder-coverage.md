# FusionAccordion MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `Accordion`
MVC builder: `AccordionBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 21 |
| JS members with matching builder method | 15 |
| JS members without matching builder method | 7 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `Animation` | `Syncfusion.EJ2.Navigations.AccordionAnimationSettings` |
| `Clicked` | `System.String` |
| `Created` | `System.String` |
| `DataSource` | `System.Object` |
| `Destroyed` | `System.String` |
| `EnableHtmlSanitizer` | `System.Boolean` |
| `EnablePersistence` | `System.Boolean` |
| `EnableRtl` | `System.Boolean` |
| `Expanded` | `System.String` |
| `ExpandedIndices` | `System.Double[]` |
| `Expanding` | `System.String` |
| `ExpandMode` | `Syncfusion.EJ2.Navigations.ExpandMode` |
| `HeaderTemplate` | `System.String` |
| `Height` | `System.String` |
| `Height` | `System.Double` |
| `HtmlAttributes` | `System.Object` |
| `Items` | `System.Collections.Generic.List{Syncfusion.EJ2.Navigations.AccordionItem}` |
| `ItemTemplate` | `System.String` |
| `Locale` | `System.String` |
| `Width` | `System.String` |
| `Width` | `System.Double` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `addItem` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `animation` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `clicked` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `created` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `dataSource` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `destroy` | method | no | skip: lifecycle cleanup, not Fusion plan behavior |
| `destroyed` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `enableHtmlSanitizer` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableItem` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `expanded` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `expandedIndices` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `expanding` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `expandItem` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `expandMode` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `headerTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `height` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `hideItem` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `items` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `itemTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `removeItem` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `select` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `width` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
