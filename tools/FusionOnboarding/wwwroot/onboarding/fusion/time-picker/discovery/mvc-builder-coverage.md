# FusionTimePicker MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `TimePicker`
MVC builder: `TimePickerBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/33.2.10/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 35 |
| JS members with matching builder method | 30 |
| JS members without matching builder method | 7 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `AllowEdit` | `System.Boolean` |
| `Blur` | `System.String` |
| `Change` | `System.String` |
| `Cleared` | `System.String` |
| `Close` | `System.String` |
| `Created` | `System.String` |
| `CssClass` | `System.String` |
| `Destroyed` | `System.String` |
| `Enabled` | `System.Boolean` |
| `EnableMask` | `System.Boolean` |
| `EnablePersistence` | `System.Boolean` |
| `EnableRtl` | `System.Boolean` |
| `FloatLabelType` | `Syncfusion.EJ2.Inputs.FloatLabelType` |
| `Focus` | `System.String` |
| `Format` | `System.String` |
| `FullScreenMode` | `System.Boolean` |
| `HtmlAttributes` | `System.Object` |
| `ItemRender` | `System.String` |
| `KeyConfigs` | `System.Object` |
| `Locale` | `System.String` |
| `MaskPlaceholder` | `Syncfusion.EJ2.Calendars.TimePickerMaskPlaceholder` |
| `Max` | `System.Object` |
| `Min` | `System.Object` |
| `Open` | `System.String` |
| `OpenOnFocus` | `System.Boolean` |
| `Placeholder` | `System.String` |
| `ScrollTo` | `System.Object` |
| `ServerTimezoneOffset` | `System.Double` |
| `ShowClearButton` | `System.Boolean` |
| `Step` | `System.Double` |
| `StrictMode` | `System.Boolean` |
| `Value` | `System.Object` |
| `Width` | `System.String` |
| `Width` | `System.Double` |
| `ZIndex` | `System.Int32` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `allowEdit` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `blur` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `change` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `cleared` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `close` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `created` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `cssClass` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `destroy` | method | no | skip: lifecycle cleanup, not Fusion plan behavior |
| `destroyed` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `enabled` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableMask` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enablePersistence` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `floatLabelType` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `focus` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `focusIn` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `focusOut` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `format` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `fullScreenMode` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `hide` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `itemRender` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `maskPlaceholder` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `max` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `min` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `open` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `openOnFocus` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `placeholder` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `readonly` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `requiredModules` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `scrollTo` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `serverTimezoneOffset` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `show` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `showClearButton` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `step` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `strictMode` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `value` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `width` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `zIndex` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
