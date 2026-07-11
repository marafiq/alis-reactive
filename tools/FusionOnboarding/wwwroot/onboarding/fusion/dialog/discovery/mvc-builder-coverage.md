# FusionDialog MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `Dialog`
MVC builder: `DialogBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 41 |
| JS members with matching builder method | 35 |
| JS members without matching builder method | 7 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `AllowDragging` | `System.Boolean` |
| `AnimationSettings` | `Syncfusion.EJ2.Popups.DialogAnimationSettings` |
| `BeforeClose` | `System.String` |
| `BeforeOpen` | `System.String` |
| `BeforeSanitizeHtml` | `System.String` |
| `Buttons` | `System.Collections.Generic.List{Syncfusion.EJ2.Popups.DialogDialogButton}` |
| `Close` | `System.String` |
| `CloseOnEscape` | `System.Boolean` |
| `Content` | `System.String` |
| `Created` | `System.String` |
| `CssClass` | `System.String` |
| `Destroyed` | `System.String` |
| `Drag` | `System.String` |
| `DragStart` | `System.String` |
| `DragStop` | `System.String` |
| `EnableHtmlSanitizer` | `System.Boolean` |
| `EnablePersistence` | `System.Boolean` |
| `EnableResize` | `System.Boolean` |
| `EnableRtl` | `System.Boolean` |
| `FooterTemplate` | `System.String` |
| `Header` | `System.String` |
| `Height` | `System.String` |
| `Height` | `System.Double` |
| `HtmlAttributes` | `System.Object` |
| `IsModal` | `System.Boolean` |
| `Locale` | `System.String` |
| `MinHeight` | `System.String` |
| `MinHeight` | `System.Double` |
| `Open` | `System.String` |
| `OverlayClick` | `System.String` |
| `Position` | `Syncfusion.EJ2.Popups.DialogPositionData` |
| `ResizeHandles` | `System.Object` |
| `ResizeStart` | `System.String` |
| `ResizeStop` | `System.String` |
| `Resizing` | `System.String` |
| `ShowCloseIcon` | `System.Boolean` |
| `Target` | `System.String` |
| `Visible` | `System.Boolean` |
| `Width` | `System.String` |
| `Width` | `System.Double` |
| `ZIndex` | `System.Double` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `allowDragging` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `animationSettings` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `beforeClose` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `beforeOpen` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `beforeSanitizeHtml` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `buttons` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `close` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `closeOnEscape` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `content` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `created` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `cssClass` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `destroy` | method | no | skip: lifecycle cleanup, not Fusion plan behavior |
| `destroyed` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `drag` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `dragStart` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `dragStop` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `enableHtmlSanitizer` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enablePersistence` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableResize` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `footerTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `getButtons` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getDimension` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `header` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `height` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `hide` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `isModal` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `minHeight` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `open` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `overlayClick` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `position` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `refreshPosition` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `resizeHandles` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `resizeStart` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `resizeStop` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `resizing` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `sanitizeHelper` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `show` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `showCloseIcon` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `target` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `visible` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `width` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `zIndex` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
