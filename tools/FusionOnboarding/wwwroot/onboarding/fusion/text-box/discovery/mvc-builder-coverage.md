# FusionTextBox MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `TextBox`
MVC builder: `TextBoxBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 23 |
| JS members with matching builder method | 19 |
| JS members without matching builder method | 7 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `AppendTemplate` | `System.String` |
| `Autocomplete` | `System.String` |
| `Blur` | `System.String` |
| `Change` | `System.String` |
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
| `Multiline` | `System.Boolean` |
| `Placeholder` | `System.String` |
| `PrependTemplate` | `System.String` |
| `ShowClearButton` | `System.Boolean` |
| `Type` | `System.String` |
| `Value` | `System.String` |
| `Width` | `System.String` |
| `Width` | `System.Double` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `addAttributes` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `addIcon` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `appendTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `autocomplete` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `blur` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `change` | event | yes | candidate: typed event; payload and browser gesture proof required |
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
| `multiline` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `placeholder` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `prependTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `readonly` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `removeAttributes` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `showClearButton` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `type` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `value` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `width` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
