# FusionBreadcrumb MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `Breadcrumb`
MVC builder: `BreadcrumbBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 17 |
| JS members with matching builder method | 14 |
| JS members without matching builder method | 2 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `ActiveItem` | `System.String` |
| `BeforeItemRender` | `System.String` |
| `Created` | `System.String` |
| `CssClass` | `System.String` |
| `Disabled` | `System.Boolean` |
| `EnableActiveItemNavigation` | `System.Boolean` |
| `EnableNavigation` | `System.Boolean` |
| `EnablePersistence` | `System.Boolean` |
| `EnableRtl` | `System.Boolean` |
| `HtmlAttributes` | `System.Object` |
| `ItemClick` | `System.String` |
| `Items` | `System.Collections.Generic.List{Syncfusion.EJ2.Navigations.BreadcrumbItem}` |
| `ItemTemplate` | `System.String` |
| `MaxItems` | `System.Int32` |
| `OverflowMode` | `Syncfusion.EJ2.Navigations.BreadcrumbOverflowMode` |
| `SeparatorTemplate` | `System.String` |
| `Url` | `System.String` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `activeItem` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `beforeItemRender` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `created` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `cssClass` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `destroy` | method | no | skip: lifecycle cleanup, not Fusion plan behavior |
| `disabled` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableActiveItemNavigation` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableNavigation` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `itemClick` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `items` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `itemTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `locale` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `maxItems` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `overflowMode` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `separatorTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `url` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
