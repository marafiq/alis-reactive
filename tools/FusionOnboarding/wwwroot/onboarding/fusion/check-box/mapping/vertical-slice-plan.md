# CheckBox Vertical Slice Plan

Status: active and proven. Every accepted `FusionCheckBox` member maps to an
exact vertical slice file and the primitive-map row that permits it. The slice
adds zero TypeScript runtime changes. The component is fully audited.

## Accepted Member To File Map

| Accepted C# member | Vertical slice file | Primitive-map row |
| --- | --- | --- |
| `FusionCheckBox(this InputBoundField<TModel, bool> setup, Action<CheckBoxBuilder> build)` | `Alis.Reactive.Fusion/Components/FusionCheckBox/FusionCheckBoxHtmlExtensions.cs` | CheckBox field render row |
| `FusionCheckBoxEvents.Changed` | `Alis.Reactive.Fusion/Components/FusionCheckBox/FusionCheckBoxEvents.cs` | `change` event trigger row |
| `Reactive(...)` event wiring | `Alis.Reactive.Fusion/Components/FusionCheckBox/FusionCheckBoxReactiveExtensions.cs` | `change` event trigger row |
| `FusionCheckBoxChangeArgs.Checked` | `Alis.Reactive.Fusion/Components/FusionCheckBox/Events/FusionCheckBoxOnChanged.cs` | `change.checked` payload read row |
| `SetChecked(this ComponentRef<FusionCheckBox, TModel> self, bool isChecked)` | `Alis.Reactive.Fusion/Components/FusionCheckBox/FusionCheckBoxExtensions.cs` | `checked` property write row plus `dataBind` repaint row |
| `SetIndeterminate(this ComponentRef<FusionCheckBox, TModel> self, bool isIndeterminate)` | `Alis.Reactive.Fusion/Components/FusionCheckBox/FusionCheckBoxExtensions.cs` | `indeterminate` property write row plus `dataBind` repaint row |
| `SetDisabled(this ComponentRef<FusionCheckBox, TModel> self, bool disabled)` | `Alis.Reactive.Fusion/Components/FusionCheckBox/FusionCheckBoxExtensions.cs` | `disabled` property write row plus `dataBind` repaint row |
| `Click(this ComponentRef<FusionCheckBox, TModel> self)` | `Alis.Reactive.Fusion/Components/FusionCheckBox/FusionCheckBoxExtensions.cs` | `click()` method row |
| `FocusIn(this ComponentRef<FusionCheckBox, TModel> self)` | `Alis.Reactive.Fusion/Components/FusionCheckBox/FusionCheckBoxExtensions.cs` | `focusIn()` method row |
| `Checked(this ComponentRef<FusionCheckBox, TModel> self)` | `Alis.Reactive.Fusion/Components/FusionCheckBox/FusionCheckBoxExtensions.cs` | `checked` property read row |
| `Indeterminate(this ComponentRef<FusionCheckBox, TModel> self)` | `Alis.Reactive.Fusion/Components/FusionCheckBox/FusionCheckBoxExtensions.cs` | `indeterminate` property read row |
| `Disabled(this ComponentRef<FusionCheckBox, TModel> self)` | `Alis.Reactive.Fusion/Components/FusionCheckBox/FusionCheckBoxExtensions.cs` | `disabled` property read row |
| component identity (`ValueMember`, registration) | `Alis.Reactive.Fusion/Components/FusionCheckBox/FusionCheckBox.cs` | n/a (component contract) |

## Slice File Inventory

The CheckBox slice follows the input-component isolation pattern. It does not move
behavior into shared base classes; duplication with other slices is intentional.

- `Alis.Reactive.Fusion/Components/FusionCheckBox/FusionCheckBox.cs` — the sealed `FusionCheckBox : FusionComponent, IInputComponent`, its `checked` value member, and input-component registration.
- `Alis.Reactive.Fusion/Components/FusionCheckBox/FusionCheckBoxHtmlExtensions.cs` — the `FusionCheckBox(...)` render helper that registers the input component and renders the `CheckBoxBuilder` bound to the model property.
- `Alis.Reactive.Fusion/Components/FusionCheckBox/FusionCheckBoxExtensions.cs` — the post-render members `SetChecked`, `SetIndeterminate`, `SetDisabled`, `Click`, `FocusIn`, `Checked`, `Indeterminate`, and `Disabled`.
- `Alis.Reactive.Fusion/Components/FusionCheckBox/FusionCheckBoxEvents.cs` — the `Changed` event selector.
- `Alis.Reactive.Fusion/Components/FusionCheckBox/FusionCheckBoxReactiveExtensions.cs` — the `Reactive(...)` event-to-plan wiring through `ComponentEventOnboarding.Wire`.
- `Alis.Reactive.Fusion/Components/FusionCheckBox/Events/FusionCheckBoxOnChanged.cs` — the typed `FusionCheckBoxChangeArgs` payload with `Checked`.

## Sandbox Surface

The behavior is exercised against a real sandbox view through the typed DSL.

- View: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/CheckBox/Index.cshtml`
- Controller: `Alis.Reactive.SandboxApp/Areas/Sandbox/Controllers/Components/Fusion/FusionCheckBoxController.cs`
- Model: `Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Components/Fusion/CheckBox/FusionCheckBoxModel.cs`
- Route: `http://localhost:5220/Sandbox/Components/FusionCheckBox`

## Playwright Proof Surface

- Test file: `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/CheckBox/WhenUsingFusionCheckBox.cs`
- Test class: `Alis.Reactive.PlaywrightTests.Components.Fusion.CheckBox.WhenUsingFusionCheckBox`
- Per-member fails-when-broken mapping: `proof/behavioral-coverage.json`
- Detailed proof table: `proof/playwright-proof.md`

## Sync And Async Lanes

Every CheckBox row is sync: the field render is sync input registration; the
`change` component event trigger is sync; `SetChecked`, `SetIndeterminate`,
`SetDisabled` (each a property set plus `dataBind`), `Click`, `FocusIn`, and the
`Checked`, `Indeterminate`, `Disabled` reads are sync component actions. The slice
introduces no async boundary of its own; async appears only when a developer
composes a read source into an HTTP pipeline, which is the HTTP primitive.

## Out Of Scope

- TypeScript runtime changes (zero, by the vendor-isolation rule).
- New or broadened DSL primitives.
- Builder-owned initial-render static properties (`cssClass`, `enableHtmlSanitizer`, `label`, `labelPosition`, `name`, `value`, and the first-paint values of `indeterminate`/`disabled`).
- The `created` lifecycle event and the `destroy` lifecycle method.
