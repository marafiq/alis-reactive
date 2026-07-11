# FusionChipList MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `ChipList`
MVC builder: `ChipListBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 33 |
| JS members with matching builder method | 23 |
| JS members without matching builder method | 7 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `AllowDragAndDrop` | `System.Boolean` |
| `AvatarIconCss` | `System.String` |
| `AvatarText` | `System.String` |
| `BeforeClick` | `System.String` |
| `Chips` | `System.Object` |
| `Chips` | `System.String[]` |
| `Chips` | `System.Double[]` |
| `Chips` | `System.Collections.Generic.List{Syncfusion.EJ2.Buttons.ChipCollection}` |
| `Click` | `System.String` |
| `Created` | `System.String` |
| `CssClass` | `System.String` |
| `Delete` | `System.String` |
| `Deleted` | `System.String` |
| `DragArea` | `System.String` |
| `Dragging` | `System.String` |
| `DragStart` | `System.String` |
| `DragStop` | `System.String` |
| `Enabled` | `System.Boolean` |
| `EnableDelete` | `System.Boolean` |
| `EnablePersistence` | `System.Boolean` |
| `EnableRtl` | `System.Boolean` |
| `HtmlAttributes` | `System.Object` |
| `LeadingIconCss` | `System.String` |
| `LeadingIconUrl` | `System.String` |
| `Locale` | `System.String` |
| `SelectedChips` | `System.Object` |
| `SelectedChips` | `System.String[]` |
| `SelectedChips` | `System.Double[]` |
| `SelectedChips` | `System.Double` |
| `Selection` | `Syncfusion.EJ2.Buttons.Selection` |
| `Text` | `System.String` |
| `TrailingIconCss` | `System.String` |
| `TrailingIconUrl` | `System.String` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `add` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `allowDragAndDrop` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `avatarIconCss` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `avatarText` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `beforeClick` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `chips` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `click` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `created` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `cssClass` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `delete` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `deleted` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `destroy` | method | no | skip: lifecycle cleanup, not Fusion plan behavior |
| `dragArea` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `dragging` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `dragStart` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `dragStop` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `enabled` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableDelete` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `find` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getSelectedChips` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `leadingIconCss` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `leadingIconUrl` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `multiSelectedChip` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `remove` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `select` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `selectedChips` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `selection` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `text` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `trailingIconCss` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `trailingIconUrl` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
