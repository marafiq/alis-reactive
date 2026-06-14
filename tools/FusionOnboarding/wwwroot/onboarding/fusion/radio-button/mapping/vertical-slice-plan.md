# RadioButton Vertical Slice Plan

Status: active and proven. Every accepted `FusionRadioButton` member maps to an
exact vertical slice file and the primitive-map row that permits it. The slice
adds zero TypeScript runtime changes. The component is fully audited.

## Accepted Member To File Map

| Accepted C# member | Vertical slice file | Primitive-map row |
| --- | --- | --- |
| `FusionRadioButton(#if NET48 this HtmlHelper<TModel> html, #else this IHtmlHelper<TModel> html, #endif ReactivePlan<TModel> plan, string elementId, Action<RadioButtonBuilder> build)` | `Alis.Reactive.Fusion/Components/FusionRadioButton/FusionRadioButtonHtmlExtensions.cs` | RadioButton element render row |
| `FusionRadioButtonEvents.Changed` | `Alis.Reactive.Fusion/Components/FusionRadioButton/FusionRadioButtonEvents.cs` | `change` event trigger row |
| `Reactive(...)` event wiring | `Alis.Reactive.Fusion/Components/FusionRadioButton/FusionRadioButtonReactiveExtensions.cs` | `change` event trigger row |
| `FusionRadioButtonChangeArgs.Value` | `Alis.Reactive.Fusion/Components/FusionRadioButton/Events/FusionRadioButtonOnChanged.cs` | `change.value` payload read row |
| `SetChecked(this ComponentRef<FusionRadioButton, TModel> self, bool isChecked)` | `Alis.Reactive.Fusion/Components/FusionRadioButton/FusionRadioButtonExtensions.cs` | `checked` property write row plus `dataBind` repaint row |
| `SetDisabled(this ComponentRef<FusionRadioButton, TModel> self, bool disabled)` | `Alis.Reactive.Fusion/Components/FusionRadioButton/FusionRadioButtonExtensions.cs` | `disabled` property write row plus `dataBind` repaint row |
| `Checked(this ComponentRef<FusionRadioButton, TModel> self)` | `Alis.Reactive.Fusion/Components/FusionRadioButton/FusionRadioButtonExtensions.cs` | `checked` property read row |
| `Disabled(this ComponentRef<FusionRadioButton, TModel> self)` | `Alis.Reactive.Fusion/Components/FusionRadioButton/FusionRadioButtonExtensions.cs` | `disabled` property read row |
| `SelectedValue(this ComponentRef<FusionRadioButton, TModel> self)` | `Alis.Reactive.Fusion/Components/FusionRadioButton/FusionRadioButtonExtensions.cs` | `getSelectedValue()` group read row |
| `Click(this ComponentRef<FusionRadioButton, TModel> self)` | `Alis.Reactive.Fusion/Components/FusionRadioButton/FusionRadioButtonExtensions.cs` | `click()` method row |
| `FocusIn(this ComponentRef<FusionRadioButton, TModel> self)` | `Alis.Reactive.Fusion/Components/FusionRadioButton/FusionRadioButtonExtensions.cs` | `focusIn()` method row |
| component identity (`Vendor`, registration) | `Alis.Reactive.Fusion/Components/FusionRadioButton/FusionRadioButton.cs` | n/a (component contract) |

## Slice File Inventory

The RadioButton slice follows the input-component isolation pattern. It does not
move behavior into shared base classes; duplication with other slices is
intentional.

- `Alis.Reactive.Fusion/Components/FusionRadioButton/FusionRadioButton.cs` — the sealed `FusionRadioButton : FusionComponent` that scopes typed post-render behavior and event wiring.
- `Alis.Reactive.Fusion/Components/FusionRadioButton/FusionRadioButtonBuilder.cs` — the `FusionRadioButtonBuilder<TModel>` that carries the rendered Syncfusion output plus the component id and plan for `.Reactive(...)`.
- `Alis.Reactive.Fusion/Components/FusionRadioButton/FusionRadioButtonHtmlExtensions.cs` — the `FusionRadioButton(...)` element render helper that renders the `RadioButtonBuilder` with a stable element id.
- `Alis.Reactive.Fusion/Components/FusionRadioButton/FusionRadioButtonExtensions.cs` — the post-render members `SetChecked`, `SetDisabled`, `Checked`, `Disabled`, `SelectedValue`, `Click`, and `FocusIn`.
- `Alis.Reactive.Fusion/Components/FusionRadioButton/FusionRadioButtonEvents.cs` — the `Changed` event selector over the EJ2 `change` event.
- `Alis.Reactive.Fusion/Components/FusionRadioButton/FusionRadioButtonReactiveExtensions.cs` — the `Reactive(...)` event-to-plan wiring through `ComponentEventOnboarding.Wire`.
- `Alis.Reactive.Fusion/Components/FusionRadioButton/Events/FusionRadioButtonOnChanged.cs` — the typed `FusionRadioButtonChangeArgs` payload with `Value`.

## Sandbox Surface

The behavior is exercised against a real sandbox view through the typed DSL.

- View: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/RadioButton/Index.cshtml`
- Controller: `Alis.Reactive.SandboxApp/Areas/Sandbox/Controllers/Components/Fusion/RadioButtonController.cs`
- Model: `Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Components/Fusion/RadioButton/FusionRadioButtonModel.cs`
- Route: `http://localhost:5220/Sandbox/Components/FusionRadioButton`

## Playwright Proof Surface

- Test file: `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/RadioButton/WhenUsingFusionRadioButton.cs`
- Test class: `Alis.Reactive.PlaywrightTests.Components.Fusion.RadioButton.WhenUsingFusionRadioButton`
- Per-member fails-when-broken mapping: `proof/behavioral-coverage.json`
- Detailed proof table: `proof/playwright-proof.md`

## Sync And Async Lanes

Every RadioButton row is sync: the element render is sync element registration;
the `change` component event trigger is sync; `SetChecked` and `SetDisabled`
(property set plus `dataBind`), the `Checked`, `Disabled`, and `SelectedValue`
reads, and the `Click` and `FocusIn` calls are sync component actions. The slice
introduces no async boundary of its own; async appears only when a developer
composes a read source such as `Checked()` into an HTTP pipeline, which is the
HTTP primitive.

## Out Of Scope

- TypeScript runtime changes (zero, by the vendor-isolation rule).
- New or broadened DSL primitives.
- Builder-owned static properties (`name`, `label`, `labelPosition`, `cssClass`, `enableHtmlSanitizer`, and the per-button `value`).
- The `created` lifecycle event and the `destroy` lifecycle method.
