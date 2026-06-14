# FusionAutoComplete MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `AutoComplete`
MVC builder: `AutoCompleteBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 61 |
| JS members with matching builder method | 10 |
| JS members without matching builder method | 8 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `ActionBegin` | `System.String` |
| `ActionComplete` | `System.String` |
| `ActionFailure` | `System.String` |
| `ActionFailureTemplate` | `System.String` |
| `AllowCustom` | `System.Boolean` |
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
| `Fields` | `Syncfusion.EJ2.DropDowns.AutoCompleteFieldSettings` |
| `Filtering` | `System.String` |
| `FilterType` | `Syncfusion.EJ2.DropDowns.FilterType` |
| `FloatLabelType` | `Syncfusion.EJ2.Inputs.FloatLabelType` |
| `Focus` | `System.String` |
| `FooterTemplate` | `System.String` |
| `GroupTemplate` | `System.String` |
| `HeaderTemplate` | `System.String` |
| `Highlight` | `System.Boolean` |
| `HtmlAttributes` | `System.Object` |
| `IgnoreAccent` | `System.Boolean` |
| `IgnoreCase` | `System.Boolean` |
| `IsDeviceFullScreen` | `System.Boolean` |
| `ItemTemplate` | `System.String` |
| `Locale` | `System.String` |
| `MinLength` | `System.Double` |
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
| `ShowPopupButton` | `System.Boolean` |
| `SortOrder` | `System.Object` |
| `SuggestionCount` | `System.Double` |
| `Value` | `System.Object` |
| `Value` | `System.Double` |
| `Value` | `System.String` |
| `Value` | `System.Boolean` |
| `Width` | `System.String` |
| `ZIndex` | `System.Double` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `allowFiltering` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `fields` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `filter` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `filterBarPlaceholder` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `filtering` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `filterType` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `floatLabelType` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `hidePopup` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `highlight` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `ignoreCase` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `index` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `minLength` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `query` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `showPopup` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `showPopupButton` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `suggestionCount` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `text` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `valueTemplate` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
