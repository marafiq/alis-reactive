# FusionButton MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `Button`
MVC builder: `ButtonBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 15 |
| JS members with matching builder method | 10 |
| JS members without matching builder method | 3 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `Click` | `System.String` |
| `Content` | `System.String` |
| `Created` | `System.String` |
| `CssClass` | `System.String` |
| `Disabled` | `System.Boolean` |
| `EnableHtmlSanitizer` | `System.Boolean` |
| `EnablePersistence` | `System.Boolean` |
| `EnableRtl` | `System.Boolean` |
| `HtmlAttributes` | `System.Collections.Generic.IDictionary{System.String,System.Object}` |
| `HtmlAttributes` | `System.Object` |
| `IconCss` | `System.String` |
| `IconPosition` | `Syncfusion.EJ2.Buttons.IconPosition` |
| `IsPrimary` | `System.Boolean` |
| `IsToggle` | `System.Boolean` |
| `Type` | `System.String` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `click` | method | yes | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `content` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `created` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `cssClass` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `destroy` | method | no | skip: lifecycle cleanup, not Fusion plan behavior |
| `disabled` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableHtmlSanitizer` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `focusIn` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `iconCss` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `iconPosition` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `isPrimary` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `isToggle` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `locale` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
