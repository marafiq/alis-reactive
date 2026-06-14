# FusionColorPicker MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `ColorPicker`
MVC builder: `ColorPickerBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 26 |
| JS members with matching builder method | 22 |
| JS members without matching builder method | 4 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `BeforeClose` | `System.String` |
| `BeforeModeSwitch` | `System.String` |
| `BeforeOpen` | `System.String` |
| `BeforeTileRender` | `System.String` |
| `Change` | `System.String` |
| `Columns` | `System.Double` |
| `Created` | `System.String` |
| `CreatePopupOnClick` | `System.Boolean` |
| `CssClass` | `System.String` |
| `Disabled` | `System.Boolean` |
| `EnableOpacity` | `System.Boolean` |
| `EnablePersistence` | `System.Boolean` |
| `EnableRtl` | `System.Boolean` |
| `HtmlAttributes` | `System.Object` |
| `Inline` | `System.Boolean` |
| `Locale` | `System.String` |
| `Mode` | `Syncfusion.EJ2.Inputs.ColorPickerMode` |
| `ModeSwitcher` | `System.Boolean` |
| `NoColor` | `System.Boolean` |
| `OnModeSwitch` | `System.String` |
| `Open` | `System.String` |
| `PresetColors` | `System.Object` |
| `Select` | `System.String` |
| `ShowButtons` | `System.Boolean` |
| `ShowRecentColors` | `System.Boolean` |
| `Value` | `System.String` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `beforeClose` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `beforeModeSwitch` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `beforeOpen` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `beforeTileRender` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `change` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `columns` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `created` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `createPopupOnClick` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `cssClass` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `destroy` | method | no | skip: lifecycle cleanup, not Fusion plan behavior |
| `disabled` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableOpacity` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enablePersistence` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `focusIn` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getValue` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `inline` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `mode` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `modeSwitcher` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `noColor` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `onModeSwitch` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `open` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `select` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `showButtons` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `showRecentColors` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `toggle` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `value` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
