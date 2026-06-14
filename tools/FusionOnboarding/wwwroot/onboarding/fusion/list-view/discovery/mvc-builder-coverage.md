# FusionListView MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `ListView`
MVC builder: `ListViewBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 33 |
| JS members with matching builder method | 24 |
| JS members without matching builder method | 24 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `ActionBegin` | `System.String` |
| `ActionComplete` | `System.String` |
| `ActionFailure` | `System.String` |
| `Animation` | `System.Object` |
| `CheckBoxPosition` | `Syncfusion.EJ2.Lists.CheckBoxPosition` |
| `CssClass` | `System.String` |
| `DataSource` | `System.Object` |
| `DataSource` | `System.String[]` |
| `DataSource` | `System.Double[]` |
| `Enable` | `System.Boolean` |
| `Enabled` | `System.Boolean` |
| `EnableHtmlSanitizer` | `System.Boolean` |
| `EnablePersistence` | `System.Boolean` |
| `EnableRtl` | `System.Boolean` |
| `EnableVirtualization` | `System.Boolean` |
| `Fields` | `Syncfusion.EJ2.Lists.ListViewFieldSettings` |
| `GroupTemplate` | `System.String` |
| `HeaderTemplate` | `System.String` |
| `HeaderTitle` | `System.String` |
| `Height` | `System.String` |
| `Height` | `System.Double` |
| `HtmlAttributes` | `System.Object` |
| `Locale` | `System.String` |
| `Query` | `System.String` |
| `Scroll` | `System.String` |
| `Select` | `System.String` |
| `ShowCheckBox` | `System.Boolean` |
| `ShowHeader` | `System.Boolean` |
| `ShowIcon` | `System.Boolean` |
| `SortOrder` | `Syncfusion.EJ2.Lists.SortOrder` |
| `Template` | `System.String` |
| `Width` | `System.String` |
| `Width` | `System.Double` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `actionBegin` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `actionComplete` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `actionFailure` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `addItem` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `animation` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `back` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `checkAllItems` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `checkBoxPosition` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `checkItem` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `cssClass` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `destroy` | method | no | skip: lifecycle cleanup, not Fusion plan behavior |
| `disableHtmlEncode` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `disableItem` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `enable` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enabled` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableHtmlSanitizer` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableItem` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `enableVirtualization` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `fields` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `findItem` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getSelectedItems` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `groupTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `headerTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `headerTitle` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `height` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `hideItem` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `localData` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `query` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `refreshItemHeight` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `removeItem` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `removeMultipleItems` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `requiredModules` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `scroll` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `select` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `selectItem` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `selectMultipleItems` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `showCheckBox` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `showHeader` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `showIcon` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `showItem` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `sortOrder` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `targetElement` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `template` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `uncheckAllItems` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `uncheckItem` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `unselectItem` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `virtualizationModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `width` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
