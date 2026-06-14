# FusionStepper MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `Stepper`
MVC builder: `StepperBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 21 |
| JS members with matching builder method | 12 |
| JS members without matching builder method | 5 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `ActiveStep` | `System.Int32` |
| `Animation` | `Syncfusion.EJ2.Navigations.StepperAnimationSettings` |
| `BeforeStepRender` | `System.String` |
| `Created` | `System.String` |
| `CssClass` | `System.String` |
| `EnablePersistence` | `System.Boolean` |
| `EnableRtl` | `System.Boolean` |
| `HtmlAttributes` | `System.Object` |
| `LabelPosition` | `Syncfusion.EJ2.Navigations.StepLabelPosition` |
| `Linear` | `System.Boolean` |
| `Locale` | `System.String` |
| `Orientation` | `Syncfusion.EJ2.Navigations.StepperOrientation` |
| `ReadOnly` | `System.Boolean` |
| `ShowTooltip` | `System.Boolean` |
| `StepChanged` | `System.String` |
| `StepChanging` | `System.String` |
| `StepClick` | `System.String` |
| `Steps` | `System.Collections.Generic.List{Syncfusion.EJ2.Navigations.Step}` |
| `StepType` | `Syncfusion.EJ2.Navigations.StepType` |
| `Template` | `System.String` |
| `TooltipTemplate` | `System.String` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `activeStep` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `animation` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `beforeStepRender` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `destroy` | method | no | skip: lifecycle cleanup, not Fusion plan behavior |
| `labelPosition` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `linear` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `nextStep` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `previousStep` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `refreshProgressbar` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `reset` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `showTooltip` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `stepChanged` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `stepChanging` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `stepClick` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `stepType` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `template` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `tooltipTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
