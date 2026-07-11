# FusionSplitButton MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `SplitButton`
MVC builder: `SplitButtonBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 26 |
| JS members with matching builder method | 16 |
| JS members without matching builder method | 5 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `AnimationSettings` | `System.Object` |
| `BeforeClose` | `System.String` |
| `BeforeItemRender` | `System.String` |
| `BeforeOpen` | `System.String` |
| `Click` | `System.String` |
| `Close` | `System.String` |
| `CloseActionEvents` | `System.String` |
| `Content` | `System.String` |
| `Created` | `System.String` |
| `CreatePopupOnClick` | `System.Boolean` |
| `CssClass` | `System.String` |
| `Disabled` | `System.Boolean` |
| `EnableHtmlSanitizer` | `System.Boolean` |
| `EnablePersistence` | `System.Boolean` |
| `EnableRtl` | `System.Boolean` |
| `HtmlAttributes` | `System.Object` |
| `IconCss` | `System.String` |
| `IconPosition` | `Syncfusion.EJ2.SplitButtons.SplitButtonIconPosition` |
| `Items` | `System.Object` |
| `ItemTemplate` | `System.String` |
| `Locale` | `System.String` |
| `Open` | `System.String` |
| `PopupWidth` | `System.String` |
| `PopupWidth` | `System.Double` |
| `Select` | `System.String` |
| `Target` | `System.String` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `addItems` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `beforeClose` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `beforeItemRender` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `beforeOpen` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `click` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `close` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `content` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `created` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `createPopupOnClick` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `cssClass` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `destroy` | method | no | skip: lifecycle cleanup, not Fusion plan behavior |
| `disabled` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `focusIn` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `iconCss` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `iconPosition` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `items` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `open` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `removeItems` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `select` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `target` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `toggle` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
