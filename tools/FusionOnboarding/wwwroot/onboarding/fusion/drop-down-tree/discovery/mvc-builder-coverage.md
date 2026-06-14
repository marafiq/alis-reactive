# FusionDropDownTree MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `DropDownTree`
MVC builder: `DropDownTreeBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 58 |
| JS members with matching builder method | 51 |
| JS members without matching builder method | 9 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `ActionFailure` | `System.String` |
| `ActionFailureTemplate` | `System.String` |
| `AllowFiltering` | `System.Boolean` |
| `AllowMultiSelection` | `System.Boolean` |
| `BeforeOpen` | `System.String` |
| `Blur` | `System.String` |
| `Change` | `System.String` |
| `ChangeOnBlur` | `System.Boolean` |
| `Close` | `System.String` |
| `Created` | `System.String` |
| `CssClass` | `System.String` |
| `CustomTemplate` | `System.String` |
| `DataBound` | `System.String` |
| `DelimiterChar` | `System.String` |
| `Destroyed` | `System.String` |
| `DestroyPopupOnHide` | `System.Boolean` |
| `Enabled` | `System.Boolean` |
| `EnableHtmlSanitizer` | `System.Boolean` |
| `EnablePersistence` | `System.Boolean` |
| `EnableRtl` | `System.Boolean` |
| `Fields` | `Syncfusion.EJ2.DropDowns.DropDownTreeFields` |
| `FilterBarPlaceholder` | `System.String` |
| `Filtering` | `System.String` |
| `FilterType` | `Syncfusion.EJ2.DropDowns.TreeFilterType` |
| `FloatLabelType` | `System.Object` |
| `Focus` | `System.String` |
| `FooterTemplate` | `System.String` |
| `HeaderTemplate` | `System.String` |
| `HtmlAttributes` | `System.Object` |
| `IgnoreAccent` | `System.Boolean` |
| `IgnoreCase` | `System.Boolean` |
| `ItemTemplate` | `System.String` |
| `KeyPress` | `System.String` |
| `Locale` | `System.String` |
| `Mode` | `Syncfusion.EJ2.DropDowns.Mode` |
| `NoRecordsTemplate` | `System.String` |
| `Open` | `System.String` |
| `Placeholder` | `System.String` |
| `PopupHeight` | `System.String` |
| `PopupHeight` | `System.Double` |
| `PopupWidth` | `System.String` |
| `PopupWidth` | `System.Double` |
| `Select` | `System.String` |
| `SelectAllText` | `System.String` |
| `ShowCheckBox` | `System.Boolean` |
| `ShowClearButton` | `System.Boolean` |
| `ShowDropDownIcon` | `System.Boolean` |
| `ShowSelectAll` | `System.Boolean` |
| `SortOrder` | `Syncfusion.EJ2.DropDowns.SortOrder` |
| `Text` | `System.String` |
| `TreeSettings` | `Syncfusion.EJ2.DropDowns.DropDownTreeTreeSettings` |
| `UnSelectAllText` | `System.String` |
| `Value` | `System.Object` |
| `ValueTemplate` | `System.String` |
| `Width` | `System.String` |
| `Width` | `System.Double` |
| `WrapText` | `System.Boolean` |
| `ZIndex` | `System.Double` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `actionFailure` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `actionFailureTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `allowFiltering` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `allowMultiSelection` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `beforeOpen` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `blur` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `change` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `changeOnBlur` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `clear` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `close` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `created` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `cssClass` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `customTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `dataBound` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `delimiterChar` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `destroy` | method | no | skip: lifecycle cleanup, not Fusion plan behavior |
| `destroyed` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `destroyPopupOnHide` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `disableHtmlEncode` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `enabled` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableHtmlSanitizer` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `ensureVisible` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `fields` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `filterBarPlaceholder` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `filtering` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `filterType` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `floatLabelType` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `focus` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `footerTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `getLocaleName` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `headerTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `hidePopup` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `ignoreAccent` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `ignoreCase` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `itemTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `keyPress` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `mode` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `noRecordsTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `open` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `placeholder` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `popupHeight` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `popupWidth` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `readonly` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `select` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `selectAll` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `selectAllText` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `showCheckBox` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `showClearButton` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `showDropDownIcon` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `showPopup` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `showSelectAll` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `sortOrder` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `text` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `treeSettings` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `unSelectAllText` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `value` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `valueTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `width` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `wrapText` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `zIndex` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
