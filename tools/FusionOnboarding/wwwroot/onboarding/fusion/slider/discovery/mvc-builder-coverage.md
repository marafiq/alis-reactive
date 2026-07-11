# FusionSlider MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `Slider`
MVC builder: `SliderBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 32 |
| JS members with matching builder method | 23 |
| JS members without matching builder method | 5 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `Change` | `System.String` |
| `Changed` | `System.String` |
| `ColorRange` | `System.Object` |
| `Created` | `System.String` |
| `CssClass` | `System.String` |
| `CustomValues` | `System.Object` |
| `CustomValues` | `System.String[]` |
| `CustomValues` | `System.Double[]` |
| `EnableAnimation` | `System.Boolean` |
| `Enabled` | `System.Boolean` |
| `EnableHtmlSanitizer` | `System.Boolean` |
| `EnablePersistence` | `System.Boolean` |
| `EnableRtl` | `System.Boolean` |
| `HtmlAttributes` | `System.Object` |
| `Limits` | `Syncfusion.EJ2.Inputs.SliderLimitData` |
| `Locale` | `System.String` |
| `Max` | `System.Double` |
| `Min` | `System.Double` |
| `Orientation` | `Syncfusion.EJ2.Inputs.SliderOrientation` |
| `RenderedTicks` | `System.String` |
| `RenderingTicks` | `System.String` |
| `ShowButtons` | `System.Boolean` |
| `Step` | `System.Double` |
| `Ticks` | `Syncfusion.EJ2.Inputs.SliderTicksData` |
| `Tooltip` | `Syncfusion.EJ2.Inputs.SliderTooltipData` |
| `TooltipChange` | `System.String` |
| `Type` | `Syncfusion.EJ2.Inputs.SliderType` |
| `Value` | `System.Object` |
| `Value` | `System.Double` |
| `Value` | `System.Double[]` |
| `Width` | `System.String` |
| `Width` | `System.Double` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `change` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `changed` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `colorRange` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `created` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `cssClass` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `customValues` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `destroy` | method | no | skip: lifecycle cleanup, not Fusion plan behavior |
| `enableAnimation` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enabled` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableHtmlSanitizer` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `initialTooltip` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `limits` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `max` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `min` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `orientation` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `readonly` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `renderedTicks` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `renderingTicks` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `reposition` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `setTooltip` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `showButtons` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `step` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `ticks` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `tooltip` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `tooltipChange` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `type` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `value` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `width` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
