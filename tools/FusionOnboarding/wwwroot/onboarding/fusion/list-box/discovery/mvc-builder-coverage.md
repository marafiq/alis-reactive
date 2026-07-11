# FusionListBox MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `ListBox`
MVC builder: `ListBoxBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 38 |
| JS members with matching builder method | 21 |
| JS members without matching builder method | 24 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `ActionBegin` | `System.String` |
| `ActionComplete` | `System.String` |
| `ActionFailure` | `System.String` |
| `AllowDragAndDrop` | `System.Boolean` |
| `AllowFiltering` | `System.Boolean` |
| `BeforeDrop` | `System.String` |
| `BeforeItemRender` | `System.String` |
| `Change` | `System.String` |
| `Created` | `System.String` |
| `CssClass` | `System.String` |
| `DataBound` | `System.String` |
| `DataSource` | `System.Object` |
| `DataSource` | `System.String[]` |
| `DataSource` | `System.Double[]` |
| `Destroyed` | `System.String` |
| `Drag` | `System.String` |
| `DragStart` | `System.String` |
| `Drop` | `System.String` |
| `Enabled` | `System.Boolean` |
| `EnablePersistence` | `System.Boolean` |
| `EnableRtl` | `System.Boolean` |
| `Fields` | `Syncfusion.EJ2.DropDowns.ListBoxFieldSettings` |
| `FilterBarPlaceholder` | `System.String` |
| `Filtering` | `System.String` |
| `FilterType` | `Syncfusion.EJ2.DropDowns.FilterType` |
| `Height` | `System.String` |
| `Height` | `System.Double` |
| `HtmlAttributes` | `System.Object` |
| `ItemTemplate` | `System.String` |
| `Locale` | `System.String` |
| `MaximumSelectionLength` | `System.Double` |
| `NoRecordsTemplate` | `System.String` |
| `Query` | `System.String` |
| `Scope` | `System.String` |
| `SelectionSettings` | `Syncfusion.EJ2.DropDowns.ListBoxSelectionSettings` |
| `SortOrder` | `System.Object` |
| `ToolbarSettings` | `Syncfusion.EJ2.DropDowns.ListBoxToolbarSettings` |
| `Value` | `System.Object` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `actionFailureTemplate` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `addItem` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `addItems` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `allowDragAndDrop` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `allowFiltering` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `beforeDrop` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `beforeItemRender` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `change` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `cssClass` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `dataBound` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `destroy` | method | no | skip: lifecycle cleanup, not Fusion plan behavior |
| `drag` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `dragStart` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `drop` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `enabled` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableItems` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `enablePersistence` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `filter` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `filterBarPlaceholder` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `filtering` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `groupTemplate` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `height` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `hideSpinner` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `ignoreAccent` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `ignoreCase` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `maximumSelectionLength` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `moveAllTo` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `moveBottom` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `moveDown` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `moveTo` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `moveTop` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `moveUp` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `removeItem` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `removeItems` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `requiredModules` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `scope` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `select` | event | no | candidate: typed event; payload and browser gesture proof required |
| `selectAll` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `selectionSettings` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `selectItems` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `showSpinner` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `sortOrder` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `toolbarSettings` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `value` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `zIndex` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
