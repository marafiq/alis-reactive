# FusionTab MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `Tab`
MVC builder: `TabBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 35 |
| JS members with matching builder method | 30 |
| JS members without matching builder method | 13 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `Added` | `System.String` |
| `Adding` | `System.String` |
| `AllowDragAndDrop` | `System.Boolean` |
| `Animation` | `Syncfusion.EJ2.Navigations.TabAnimationSettings` |
| `ClearTemplates` | `System.Boolean` |
| `Created` | `System.String` |
| `CssClass` | `System.String` |
| `Destroyed` | `System.String` |
| `DragArea` | `System.String` |
| `Dragged` | `System.String` |
| `Dragging` | `System.String` |
| `EnableHtmlSanitizer` | `System.Boolean` |
| `EnablePersistence` | `System.Boolean` |
| `EnableRtl` | `System.Boolean` |
| `HeaderPlacement` | `Syncfusion.EJ2.Navigations.HeaderPosition` |
| `Height` | `System.String` |
| `Height` | `System.Double` |
| `HeightAdjustMode` | `Syncfusion.EJ2.Navigations.HeightStyles` |
| `HtmlAttributes` | `System.Object` |
| `Items` | `System.Collections.Generic.List{Syncfusion.EJ2.Navigations.TabItem}` |
| `LoadOn` | `Syncfusion.EJ2.Navigations.ContentLoad` |
| `Locale` | `System.String` |
| `OnDragStart` | `System.String` |
| `OverflowMode` | `Syncfusion.EJ2.Navigations.OverflowMode` |
| `Removed` | `System.String` |
| `Removing` | `System.String` |
| `ReorderActiveTab` | `System.Boolean` |
| `ScrollStep` | `System.Double` |
| `Selected` | `System.String` |
| `SelectedItem` | `System.Double` |
| `Selecting` | `System.String` |
| `ShowCloseButton` | `System.Boolean` |
| `SwipeMode` | `Syncfusion.EJ2.Navigations.TabSwipeMode` |
| `Width` | `System.String` |
| `Width` | `System.Double` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `added` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `adding` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `addTab` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `allowDragAndDrop` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `animation` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `clearTemplates` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `created` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `cssClass` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `destroy` | method | no | skip: lifecycle cleanup, not Fusion plan behavior |
| `destroyed` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `disable` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `dragArea` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `dragged` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `dragging` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `enableHtmlSanitizer` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enablePersistence` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableTab` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getItemIndex` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `headerPlacement` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `height` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `heightAdjustMode` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `hideTab` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `items` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `loadOn` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `onDragStart` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `overflowMode` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `refresh` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `refreshActiveTab` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `refreshActiveTabBorder` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `refreshOverflow` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `removed` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `removeTab` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `removing` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `reorderActiveTab` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `scrollStep` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `select` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `selected` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `selectedItem` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `selecting` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `showCloseButton` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `swipeMode` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `tabId` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `width` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
