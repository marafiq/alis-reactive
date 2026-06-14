# Slider Vertical Slice Plan

Status: active and proven. Every accepted `FusionSlider` member maps to an exact
vertical slice file and the primitive-map row that permits it. The slice adds
zero TypeScript runtime changes. The component is fully audited.

## Accepted Member To File Map

| Accepted C# member | Vertical slice file | Primitive-map row |
| --- | --- | --- |
| `FusionSlider(this InputBoundField<TModel, TProp> setup, Action<SliderBuilder> build)` | `Alis.Reactive.Fusion/Components/FusionSlider/FusionSliderHtmlExtensions.cs` | Slider field render row |
| `FusionSliderEvents.Change` | `Alis.Reactive.Fusion/Components/FusionSlider/FusionSliderEvents.cs` | `change` event trigger row |
| `FusionSliderEvents.Changed` | `Alis.Reactive.Fusion/Components/FusionSlider/FusionSliderEvents.cs` | `changed` event trigger row |
| `Reactive(...)` event wiring | `Alis.Reactive.Fusion/Components/FusionSlider/FusionSliderReactiveExtensions.cs` | `change`/`changed` event trigger rows |
| `FusionSliderChangeArgs.Value` | `Alis.Reactive.Fusion/Components/FusionSlider/Events/FusionSliderOnChanged.cs` | `change/changed.value` payload read row |
| `FusionSliderChangeArgs.PreviousValue` | `Alis.Reactive.Fusion/Components/FusionSlider/Events/FusionSliderOnChanged.cs` | `change/changed.previousValue` payload read row |
| `FusionSliderChangeArgs.Text` | `Alis.Reactive.Fusion/Components/FusionSlider/Events/FusionSliderOnChanged.cs` | `change/changed.text` payload read row |
| `FusionSliderChangeArgs.Action` | `Alis.Reactive.Fusion/Components/FusionSlider/Events/FusionSliderOnChanged.cs` | `change/changed.action` payload read row |
| `FusionSliderChangeArgs.IsInteracted` | `Alis.Reactive.Fusion/Components/FusionSlider/Events/FusionSliderOnChanged.cs` | `change/changed.isInteracted` payload read row |
| `SetValue(this ComponentRef<FusionSlider, TModel> self, double value)` | `Alis.Reactive.Fusion/Components/FusionSlider/FusionSliderExtensions.cs` | `value` scalar property write row plus `dataBind` repaint row |
| `SetRangeValue(this ComponentRef<FusionSlider, TModel> self, double start, double end)` | `Alis.Reactive.Fusion/Components/FusionSlider/FusionSliderExtensions.cs` | `value` range property write row plus `dataBind` repaint row |
| `Value(this ComponentRef<FusionSlider, TModel> self)` | `Alis.Reactive.Fusion/Components/FusionSlider/FusionSliderExtensions.cs` | `value` scalar property read row |
| `RangeValue(this ComponentRef<FusionSlider, TModel> self)` | `Alis.Reactive.Fusion/Components/FusionSlider/FusionSliderExtensions.cs` | `value` range property read row |
| component identity (`ValueMember`, registration) | `Alis.Reactive.Fusion/Components/FusionSlider/FusionSlider.cs` | n/a (component contract) |

## Slice File Inventory

The Slider slice follows the input-component isolation pattern. It does not move
behavior into shared base classes; duplication with other slices is intentional.

- `Alis.Reactive.Fusion/Components/FusionSlider/FusionSlider.cs` — the sealed `FusionSlider : FusionComponent, IInputComponent`, its `value` member name, and input-component registration.
- `Alis.Reactive.Fusion/Components/FusionSlider/FusionSliderHtmlExtensions.cs` — the `FusionSlider(...)` render helper that registers the input component and renders the `SliderBuilder` bound to the model property.
- `Alis.Reactive.Fusion/Components/FusionSlider/FusionSliderExtensions.cs` — the post-render members `SetValue`, `SetRangeValue`, `Value`, and `RangeValue`.
- `Alis.Reactive.Fusion/Components/FusionSlider/FusionSliderEvents.cs` — the `Change` and `Changed` event selectors.
- `Alis.Reactive.Fusion/Components/FusionSlider/FusionSliderReactiveExtensions.cs` — the `Reactive(...)` event-to-plan wiring through `ComponentEventOnboarding.Wire`.
- `Alis.Reactive.Fusion/Components/FusionSlider/Events/FusionSliderOnChanged.cs` — the typed `FusionSliderChangeArgs` payload with `Value`, `PreviousValue`, `Text`, `Action`, and `IsInteracted`.

## Sandbox Surface

The behavior is exercised against a real sandbox view through the typed DSL — the
"Comfort & Care Preferences" journey (room temperature + afternoon rest window).

- View: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Slider/Index.cshtml`
- Controller: `Alis.Reactive.SandboxApp/Areas/Sandbox/Controllers/Components/Fusion/SliderController.cs`
- Model: `Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Components/Fusion/Slider/SliderModel.cs`
- Route: `http://localhost:5220/Sandbox/Components/Slider`

## Playwright Proof Surface

- Test file: `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Slider/WhenUsingFusionSlider.cs`
- Test class: `Alis.Reactive.PlaywrightTests.Components.Fusion.Slider.WhenUsingFusionSlider`
- Test-infra locator: `tests/Alis.Reactive.Playwright.Extensions/FusionSliderLocator.cs` (trusted handle click + ArrowRight nudge; reads `aria-valuenow`)
- Per-member fails-when-broken mapping: `proof/behavioral-coverage.json`
- Detailed proof table: `proof/playwright-proof.md`

## Sync And Async Lanes

Every Slider row is sync: the field render is sync input registration; the
`change` and `changed` component event triggers are sync; `SetValue`,
`SetRangeValue` (property set plus `dataBind`), the `Value` read, and the
`RangeValue` read are sync component actions. The slice introduces no async
boundary of its own; async appears only when a developer composes the `Value()`
or `RangeValue()` source into an HTTP pipeline, which is the HTTP primitive.

## Out Of Scope

- TypeScript runtime changes (zero, by the vendor-isolation rule).
- New or broadened DSL primitives.
- Builder-owned static properties (`colorRange`, `cssClass`, `customValues`, `enableAnimation`, `enabled`, `enableHtmlSanitizer`, `limits`, `max`, `min`, `orientation`, `showButtons`, `step`, `ticks`, `tooltip`, `type`, `width`).
- The render/tick/tooltip event hooks (`created`, `renderedTicks`, `renderingTicks`, `tooltipChange`).
- The excluded `initialTooltip`/`readonly` properties and the `reposition`/`setTooltip`/`destroy` methods.
