# FusionComboBox MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `ComboBox`
MVC builder: `ComboBoxBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 60 |
| JS members with matching builder method | 20 |
| JS members without matching builder method | 13 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `ActionBegin` | `System.String` |
| `ActionComplete` | `System.String` |
| `ActionFailure` | `System.String` |
| `ActionFailureTemplate` | `System.String` |
| `AllowCustom` | `System.Boolean` |
| `AllowFiltering` | `System.Boolean` |
| `AllowObjectBinding` | `System.Boolean` |
| `AllowResize` | `System.Boolean` |
| `Autofill` | `System.Boolean` |
| `BeforeOpen` | `System.String` |
| `Blur` | `System.String` |
| `Change` | `System.String` |
| `Close` | `System.String` |
| `Created` | `System.String` |
| `CssClass` | `System.String` |
| `CustomValueSpecifier` | `System.String` |
| `DataBound` | `System.String` |
| `DataSource` | `System.Object` |
| `DataSource` | `System.String[]` |
| `DataSource` | `System.Double[]` |
| `DebounceDelay` | `System.Double` |
| `Destroyed` | `System.String` |
| `Enabled` | `System.Boolean` |
| `EnablePersistence` | `System.Boolean` |
| `EnableRtl` | `System.Boolean` |
| `EnableVirtualization` | `System.Boolean` |
| `Fields` | `Syncfusion.EJ2.DropDowns.ComboBoxFieldSettings` |
| `Filtering` | `System.String` |
| `FilterType` | `Syncfusion.EJ2.DropDowns.FilterType` |
| `FloatLabelType` | `Syncfusion.EJ2.Inputs.FloatLabelType` |
| `Focus` | `System.String` |
| `FooterTemplate` | `System.String` |
| `GroupTemplate` | `System.String` |
| `HeaderTemplate` | `System.String` |
| `HtmlAttributes` | `System.Object` |
| `IgnoreAccent` | `System.Boolean` |
| `IgnoreCase` | `System.Boolean` |
| `Index` | `System.Double` |
| `IsDeviceFullScreen` | `System.Boolean` |
| `ItemTemplate` | `System.String` |
| `Locale` | `System.String` |
| `NoRecordsTemplate` | `System.String` |
| `Open` | `System.String` |
| `Placeholder` | `System.String` |
| `PopupHeight` | `System.String` |
| `PopupWidth` | `System.String` |
| `Query` | `System.String` |
| `ResizeStart` | `System.String` |
| `ResizeStop` | `System.String` |
| `Resizing` | `System.String` |
| `Select` | `System.String` |
| `ShowClearButton` | `System.Boolean` |
| `SortOrder` | `System.Object` |
| `Text` | `System.String` |
| `Value` | `System.Object` |
| `Value` | `System.Double` |
| `Value` | `System.String` |
| `Value` | `System.Boolean` |
| `Width` | `System.String` |
| `ZIndex` | `System.Double` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `addItem` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `allowCustom` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `allowFiltering` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `allowObjectBinding` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `autofill` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `clear` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `cssClass` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `customValueSpecifier` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `filter` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `filterBarPlaceholder` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `filtering` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `floatLabelType` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `focusIn` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `focusOut` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `footerTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `getItems` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `headerTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `hidePopup` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `hideSpinner` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `index` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `isDeviceFullScreen` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `placeholder` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `popupHeight` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `popupWidth` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `query` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `readonly` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `showClearButton` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `showPopup` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `showSpinner` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `text` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `value` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `valueTemplate` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `width` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
