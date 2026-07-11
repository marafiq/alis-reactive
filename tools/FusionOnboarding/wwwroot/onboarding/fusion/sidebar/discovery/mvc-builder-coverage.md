# FusionSidebar MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `Sidebar`
MVC builder: `SidebarBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 23 |
| JS members with matching builder method | 20 |
| JS members without matching builder method | 7 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `Animate` | `System.Boolean` |
| `Change` | `System.String` |
| `Close` | `System.String` |
| `CloseOnDocumentClick` | `System.Boolean` |
| `Created` | `System.String` |
| `Destroyed` | `System.String` |
| `DockSize` | `System.String` |
| `DockSize` | `System.Double` |
| `EnableDock` | `System.Boolean` |
| `EnableGestures` | `System.Boolean` |
| `EnablePersistence` | `System.Boolean` |
| `EnableRtl` | `System.Boolean` |
| `HtmlAttributes` | `System.Object` |
| `IsOpen` | `System.Boolean` |
| `MediaQuery` | `System.String` |
| `Open` | `System.String` |
| `Position` | `Syncfusion.EJ2.Navigations.SidebarPosition` |
| `ShowBackdrop` | `System.Boolean` |
| `Target` | `System.String` |
| `Type` | `Syncfusion.EJ2.Navigations.SidebarType` |
| `Width` | `System.String` |
| `Width` | `System.Double` |
| `ZIndex` | `System.Double` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `animate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `change` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `close` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `closeOnDocumentClick` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `created` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `defaultBackdropDiv` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `destroy` | method | no | skip: lifecycle cleanup, not Fusion plan behavior |
| `destroyed` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `dockSize` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableDock` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableGestures` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enablePersistence` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableRtl` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `height` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `hide` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `isOpen` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `locale` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `mediaQuery` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `open` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `position` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `show` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `showBackdrop` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `target` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `toggle` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `type` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `width` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `zIndex` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
