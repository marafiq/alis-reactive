# FusionDropDownButton MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `DropDownButton`
MVC builder: `DropDownButtonBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 25 |
| JS members with matching builder method | 20 |
| JS members without matching builder method | 8 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `AnimationSettings` | `System.Object` |
| `BeforeClose` | `System.String` |
| `BeforeItemRender` | `System.String` |
| `BeforeOpen` | `System.String` |
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
| `activeElem` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `addItems` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `animationSettings` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `beforeClose` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `beforeItemRender` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `beforeOpen` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `close` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `closeActionEvents` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `content` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `created` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `createPopupOnClick` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `cssClass` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `destroy` | method | no | skip: lifecycle cleanup, not Fusion plan behavior |
| `disabled` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `dropDown` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `enableHtmlSanitizer` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `focusIn` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `iconCss` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `iconPosition` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `items` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `itemTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `keyBoardHandler` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `open` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `popupWidth` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `removeItems` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `select` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `target` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `toggle` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
