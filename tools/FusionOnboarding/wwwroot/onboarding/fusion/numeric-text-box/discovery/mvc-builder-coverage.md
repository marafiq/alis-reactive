# FusionNumericTextBox MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `NumericTextBox`
MVC builder: `NumericTextBoxBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 29 |
| JS members with matching builder method | 25 |
| JS members without matching builder method | 7 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `AllowMouseWheel` | `System.Boolean` |
| `AppendTemplate` | `System.String` |
| `Blur` | `System.String` |
| `Change` | `System.String` |
| `Created` | `System.String` |
| `CssClass` | `System.String` |
| `Currency` | `System.String` |
| `Decimals` | `System.Double` |
| `Destroyed` | `System.String` |
| `Enabled` | `System.Boolean` |
| `EnablePersistence` | `System.Boolean` |
| `EnableRtl` | `System.Boolean` |
| `FloatLabelType` | `Syncfusion.EJ2.Inputs.FloatLabelType` |
| `Focus` | `System.String` |
| `Format` | `System.String` |
| `HtmlAttributes` | `System.Object` |
| `Locale` | `System.String` |
| `Max` | `System.Object` |
| `Min` | `System.Object` |
| `Placeholder` | `System.String` |
| `PrependTemplate` | `System.String` |
| `ShowClearButton` | `System.Boolean` |
| `ShowSpinButton` | `System.Boolean` |
| `Step` | `System.Double` |
| `StrictMode` | `System.Boolean` |
| `ValidateDecimalOnType` | `System.Boolean` |
| `Value` | `System.Object` |
| `Width` | `System.String` |
| `Width` | `System.Double` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `allowMouseWheel` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `appendTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `blur` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `change` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `created` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `cssClass` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `currency` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `decimals` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `decrement` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `destroy` | method | no | skip: lifecycle cleanup, not Fusion plan behavior |
| `destroyed` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `enabled` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enablePersistence` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `floatLabelType` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `focus` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `focusIn` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `focusOut` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `format` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `getText` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `increment` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `max` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `min` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `placeholder` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `prependTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `readonly` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `showClearButton` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `showSpinButton` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `step` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `strictMode` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `validateDecimalOnType` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `value` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `width` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
