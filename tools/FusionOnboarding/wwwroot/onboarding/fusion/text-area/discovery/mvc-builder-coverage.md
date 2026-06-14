# FusionTextArea MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `TextArea`
MVC builder: `TextAreaBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 26 |
| JS members with matching builder method | 22 |
| JS members without matching builder method | 6 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `AdornmentFlow` | `Syncfusion.EJ2.Inputs.AdornmentsDirection` |
| `AdornmentOrientation` | `Syncfusion.EJ2.Inputs.AdornmentsDirection` |
| `AppendTemplate` | `System.String` |
| `Blur` | `System.String` |
| `Change` | `System.String` |
| `Cols` | `System.Nullable{System.Int32}` |
| `Created` | `System.String` |
| `CssClass` | `System.String` |
| `Destroyed` | `System.String` |
| `Enabled` | `System.Boolean` |
| `EnablePersistence` | `System.Boolean` |
| `EnableRtl` | `System.Boolean` |
| `FloatLabelType` | `Syncfusion.EJ2.Inputs.FloatLabelType` |
| `Focus` | `System.String` |
| `HtmlAttributes` | `System.Object` |
| `Input` | `System.String` |
| `Locale` | `System.String` |
| `MaxLength` | `System.Nullable{System.Int32}` |
| `Placeholder` | `System.String` |
| `PrependTemplate` | `System.String` |
| `ResizeMode` | `Syncfusion.EJ2.Inputs.Resize` |
| `Rows` | `System.Nullable{System.Int32}` |
| `ShowClearButton` | `System.Boolean` |
| `Value` | `System.String` |
| `Width` | `System.String` |
| `Width` | `System.Double` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `addAttributes` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `adornmentFlow` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `adornmentOrientation` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `appendTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `blur` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `change` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `cols` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `created` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `cssClass` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `destroy` | method | no | skip: lifecycle cleanup, not Fusion plan behavior |
| `destroyed` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `enabled` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enablePersistence` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `floatLabelType` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `focus` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `focusIn` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `focusOut` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `input` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `maxLength` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `placeholder` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `prependTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `readonly` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `removeAttributes` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `resizeMode` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `rows` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `showClearButton` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `value` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `width` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
