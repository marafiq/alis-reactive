# FusionRadioButton MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `RadioButton`
MVC builder: `RadioButtonBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 13 |
| JS members with matching builder method | 9 |
| JS members without matching builder method | 5 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `Change` | `System.String` |
| `Created` | `System.String` |
| `CssClass` | `System.String` |
| `Disabled` | `System.Boolean` |
| `EnableHtmlSanitizer` | `System.Boolean` |
| `EnablePersistence` | `System.Boolean` |
| `EnableRtl` | `System.Boolean` |
| `HtmlAttributes` | `System.Object` |
| `Label` | `System.String` |
| `LabelPosition` | `Syncfusion.EJ2.Buttons.RadioLabelPosition` |
| `Locale` | `System.String` |
| `Name` | `System.String` |
| `Value` | `System.String` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `change` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `checked` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `click` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `created` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `cssClass` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `destroy` | method | no | skip: lifecycle cleanup, not Fusion plan behavior |
| `disabled` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableHtmlSanitizer` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `focusIn` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getSelectedValue` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `label` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `labelPosition` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `name` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `value` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
