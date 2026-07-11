# FusionOtpInput MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `OtpInput`
MVC builder: `OtpInputBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 20 |
| JS members with matching builder method | 16 |
| JS members without matching builder method | 3 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `AriaLabels` | `System.String[]` |
| `AutoFocus` | `System.Boolean` |
| `Blur` | `System.String` |
| `Created` | `System.String` |
| `CssClass` | `System.String` |
| `Disabled` | `System.Boolean` |
| `EnablePersistence` | `System.Boolean` |
| `EnableRtl` | `System.Boolean` |
| `Focus` | `System.String` |
| `HtmlAttributes` | `System.Object` |
| `Input` | `System.String` |
| `Length` | `System.Double` |
| `Locale` | `System.String` |
| `Placeholder` | `System.String` |
| `Separator` | `System.String` |
| `StylingMode` | `Syncfusion.EJ2.Inputs.OtpInputStyle` |
| `TextTransform` | `Syncfusion.EJ2.Inputs.TextTransform` |
| `Type` | `Syncfusion.EJ2.Inputs.OtpInputType` |
| `Value` | `System.String` |
| `ValueChanged` | `System.String` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `ariaLabels` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `autoFocus` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `blur` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `created` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `cssClass` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `destroy` | method | no | skip: lifecycle cleanup, not Fusion plan behavior |
| `disabled` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `focus` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `focusIn` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `focusOut` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `input` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `length` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `placeholder` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `separator` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `stylingMode` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `textTransform` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `type` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `value` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `valueChanged` | event | yes | candidate: typed event; payload and browser gesture proof required |
