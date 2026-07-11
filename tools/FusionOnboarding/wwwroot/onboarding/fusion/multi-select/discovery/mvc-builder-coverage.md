# FusionMultiSelect MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `MultiSelect`
MVC builder: `MultiSelectBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 80 |
| JS members with matching builder method | 66 |
| JS members without matching builder method | 16 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `ActionBegin` | `System.String` |
| `ActionComplete` | `System.String` |
| `ActionFailure` | `System.String` |
| `ActionFailureTemplate` | `System.String` |
| `AddTagOnBlur` | `System.Boolean` |
| `AllowCustomValue` | `System.Boolean` |
| `AllowFiltering` | `System.Boolean` |
| `AllowObjectBinding` | `System.Boolean` |
| `AllowResize` | `System.Boolean` |
| `BeforeOpen` | `System.String` |
| `BeforeSelectAll` | `System.String` |
| `Blur` | `System.String` |
| `Change` | `System.String` |
| `ChangeOnBlur` | `System.Boolean` |
| `ChipSelection` | `System.String` |
| `Close` | `System.String` |
| `ClosePopupOnSelect` | `System.Boolean` |
| `Created` | `System.String` |
| `CssClass` | `System.String` |
| `CustomValueSelection` | `System.String` |
| `DataBound` | `System.String` |
| `DataSource` | `System.Object` |
| `DataSource` | `System.String[]` |
| `DataSource` | `System.Double[]` |
| `DebounceDelay` | `System.Double` |
| `DelimiterChar` | `System.String` |
| `Destroyed` | `System.String` |
| `Enabled` | `System.Boolean` |
| `EnableGroupCheckBox` | `System.Boolean` |
| `EnableHtmlSanitizer` | `System.Boolean` |
| `EnablePersistence` | `System.Boolean` |
| `EnableRtl` | `System.Boolean` |
| `EnableSelectionOrder` | `System.Boolean` |
| `EnableVirtualization` | `System.Boolean` |
| `Fields` | `Syncfusion.EJ2.DropDowns.MultiSelectFieldSettings` |
| `FilterBarPlaceholder` | `System.String` |
| `Filtering` | `System.String` |
| `FilterType` | `Syncfusion.EJ2.DropDowns.FilterType` |
| `FloatLabelType` | `Syncfusion.EJ2.Inputs.FloatLabelType` |
| `Focus` | `System.String` |
| `FooterTemplate` | `System.String` |
| `GroupTemplate` | `System.String` |
| `HeaderTemplate` | `System.String` |
| `HideSelectedItem` | `System.Boolean` |
| `HtmlAttributes` | `System.Object` |
| `IgnoreAccent` | `System.Boolean` |
| `IgnoreCase` | `System.Boolean` |
| `IsDeviceFullScreen` | `System.Boolean` |
| `ItemTemplate` | `System.String` |
| `Locale` | `System.String` |
| `MaximumSelectionLength` | `System.Double` |
| `Mode` | `Syncfusion.EJ2.DropDowns.VisualMode` |
| `NoRecordsTemplate` | `System.String` |
| `Open` | `System.String` |
| `OpenOnClick` | `System.Boolean` |
| `Placeholder` | `System.String` |
| `PopupHeight` | `System.String` |
| `PopupWidth` | `System.String` |
| `Query` | `System.String` |
| `Removed` | `System.String` |
| `Removing` | `System.String` |
| `ResizeStart` | `System.String` |
| `ResizeStop` | `System.String` |
| `Resizing` | `System.String` |
| `Select` | `System.String` |
| `SelectAllText` | `System.String` |
| `SelectedAll` | `System.String` |
| `ShowClearButton` | `System.Boolean` |
| `ShowDropDownIcon` | `System.Boolean` |
| `ShowSelectAll` | `System.Boolean` |
| `SortOrder` | `System.Object` |
| `Tagging` | `System.String` |
| `Text` | `System.String` |
| `UnSelectAllText` | `System.String` |
| `Value` | `System.Object` |
| `Value` | `System.Double[]` |
| `Value` | `System.String[]` |
| `ValueTemplate` | `System.String` |
| `Width` | `System.String` |
| `ZIndex` | `System.Double` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `actionFailureTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `addItem` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `addTagOnBlur` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `allowCustomValue` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `allowFiltering` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `allowObjectBinding` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `allowResize` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `beforeOpen` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `beforeSelectAll` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `blur` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `change` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `changeOnBlur` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `chipSelection` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `clear` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `close` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `closePopupOnSelect` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `cssClass` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `customValueSelection` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `debounceDelay` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `delimiterChar` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `destroy` | method | no | skip: lifecycle cleanup, not Fusion plan behavior |
| `disableItem` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `enabled` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableGroupCheckBox` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableHtmlSanitizer` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enablePersistence` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableSelectionOrder` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableVirtualization` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `fields` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `filter` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `filterBarPlaceholder` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `filtering` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `filterType` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `floatLabelType` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `focus` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `focusIn` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `focusOut` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `footerTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `getItems` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `groupTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `headerTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `hidePopup` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `hideSelectedItem` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `hideSpinner` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `ignoreAccent` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `ignoreCase` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `isDeviceFullScreen` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `itemTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `locale` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `maximumSelectionLength` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `mode` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `noRecordsTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `open` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `openOnClick` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `placeholder` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `popupHeight` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `popupWidth` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `query` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `readonly` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `removed` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `removing` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `requiredModules` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `resizeStart` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `resizeStop` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `resizing` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `selectAll` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `selectAllText` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `selectedAll` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `showClearButton` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `showDropDownIcon` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `showPopup` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `showSelectAll` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `showSpinner` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `sortOrder` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `tagging` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `text` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `ulElement` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `unSelectAllText` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `value` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `valueTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `width` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `zIndex` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
