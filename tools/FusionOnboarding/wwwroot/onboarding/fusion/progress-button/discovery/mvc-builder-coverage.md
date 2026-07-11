# FusionProgressButton MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `ProgressButton`
MVC builder: `ProgressButtonBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 18 |
| JS members with matching builder method | 17 |
| JS members without matching builder method | 5 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `AnimationSettings` | `Syncfusion.EJ2.SplitButtons.ProgressButtonAnimationSettings` |
| `Begin` | `System.String` |
| `Content` | `System.String` |
| `Created` | `System.String` |
| `CssClass` | `System.String` |
| `Disabled` | `System.Boolean` |
| `Duration` | `System.Double` |
| `EnableHtmlSanitizer` | `System.Boolean` |
| `EnableProgress` | `System.Boolean` |
| `End` | `System.String` |
| `Fail` | `System.String` |
| `HtmlAttributes` | `System.Object` |
| `IconCss` | `System.String` |
| `IconPosition` | `Syncfusion.EJ2.Buttons.IconPosition` |
| `IsPrimary` | `System.Boolean` |
| `IsToggle` | `System.Boolean` |
| `Progress` | `System.String` |
| `SpinSettings` | `Syncfusion.EJ2.SplitButtons.ProgressButtonSpinSettings` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `animationSettings` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `begin` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `content` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `created` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `cssClass` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `destroy` | method | no | skip: lifecycle cleanup, not Fusion plan behavior |
| `disabled` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `duration` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableHtmlSanitizer` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableProgress` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `end` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `fail` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `focusIn` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `iconCss` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `iconPosition` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `isPrimary` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `isToggle` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `progress` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `progressComplete` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `spinSettings` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `start` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `stop` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
