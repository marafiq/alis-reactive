# FusionMultiColumnComboBox MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `MultiColumnComboBox`
MVC builder: `MultiColumnComboBoxBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 41 |
| JS members with matching builder method | 37 |
| JS members without matching builder method | 8 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `ActionBegin` | `System.String` |
| `ActionComplete` | `System.String` |
| `ActionFailure` | `System.String` |
| `ActionFailureTemplate` | `System.String` |
| `AllowFiltering` | `System.Boolean` |
| `AllowSorting` | `System.Boolean` |
| `Change` | `System.String` |
| `Close` | `System.String` |
| `Columns` | `System.Collections.Generic.List{Syncfusion.EJ2.MultiColumnComboBox.Column}` |
| `Created` | `System.String` |
| `CssClass` | `System.String` |
| `DataSource` | `System.Object` |
| `Disabled` | `System.Boolean` |
| `EnablePersistence` | `System.Boolean` |
| `EnableRtl` | `System.Boolean` |
| `EnableVirtualization` | `System.Boolean` |
| `Fields` | `Syncfusion.EJ2.MultiColumnComboBox.MultiColumnComboBoxFieldSettings` |
| `Filtering` | `System.String` |
| `FilterType` | `Syncfusion.EJ2.MultiColumnComboBox.FilterType` |
| `FloatLabelType` | `Syncfusion.EJ2.MultiColumnComboBox.FloatLabelType` |
| `FooterTemplate` | `System.String` |
| `GridSettings` | `Syncfusion.EJ2.MultiColumnComboBox.MultiColumnComboBoxGridSettings` |
| `GroupTemplate` | `System.String` |
| `HtmlAttributes` | `System.Object` |
| `Index` | `System.Object` |
| `Index` | `System.Double` |
| `ItemTemplate` | `System.String` |
| `Locale` | `System.String` |
| `NoRecordsTemplate` | `System.String` |
| `Open` | `System.String` |
| `Placeholder` | `System.String` |
| `PopupHeight` | `System.String` |
| `PopupWidth` | `System.String` |
| `Query` | `System.String` |
| `Select` | `System.String` |
| `ShowClearButton` | `System.Boolean` |
| `SortOrder` | `Syncfusion.EJ2.MultiColumnComboBox.SortOrder` |
| `SortType` | `Syncfusion.EJ2.MultiColumnComboBox.SortType` |
| `Text` | `System.String` |
| `Value` | `System.String` |
| `Width` | `System.String` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `actionBegin` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `actionComplete` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `actionFailure` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `actionFailureTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `addItems` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `allowFiltering` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `allowSorting` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `change` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `close` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `columns` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `created` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `cssClass` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `dataSource` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `destroy` | method | no | skip: lifecycle cleanup, not Fusion plan behavior |
| `disabled` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enablePersistence` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableVirtualization` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `fields` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `filtering` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `filterType` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `floatLabelType` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `focusIn` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `focusOut` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `footerTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `getItems` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `gridSettings` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `groupTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `hidePopup` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `index` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `itemTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `noRecordsTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `open` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `placeholder` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `popupHeight` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `popupWidth` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `query` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `readonly` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `select` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `showClearButton` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `showPopup` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `sortOrder` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `sortType` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `text` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `value` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `width` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
