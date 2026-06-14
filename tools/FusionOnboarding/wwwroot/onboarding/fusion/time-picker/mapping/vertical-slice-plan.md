# TimePicker Vertical Slice Plan

Status: active and proven. Every accepted `FusionTimePicker` member maps to an
exact vertical slice file and the primitive-map row that permits it. The slice
adds zero TypeScript runtime changes. The component is fully audited.

## Accepted Member To File Map

| Accepted C# member | Vertical slice file | Primitive-map row |
| --- | --- | --- |
| `FusionTimePicker(this InputBoundField<TModel, TProp> setup, Action<TimePickerBuilder> build)` | `Alis.Reactive.Fusion/Components/FusionTimePicker/FusionTimePickerHtmlExtensions.cs` | TimePicker field render row |
| `FusionTimePickerEvents.Changed` | `Alis.Reactive.Fusion/Components/FusionTimePicker/FusionTimePickerEvents.cs` | `change` event trigger row |
| `Reactive(...)` event wiring | `Alis.Reactive.Fusion/Components/FusionTimePicker/FusionTimePickerReactiveExtensions.cs` | `change` event trigger row |
| `FusionTimePickerChangeArgs.Value` | `Alis.Reactive.Fusion/Components/FusionTimePicker/Events/FusionTimePickerOnChanged.cs` | `change.value` payload read row |
| `FusionTimePickerChangeArgs.IsInteracted` | `Alis.Reactive.Fusion/Components/FusionTimePicker/Events/FusionTimePickerOnChanged.cs` | `change.isInteracted` payload read row |
| `SetValue(this ComponentRef<FusionTimePicker, TModel> self, DateTime value)` | `Alis.Reactive.Fusion/Components/FusionTimePicker/FusionTimePickerExtensions.cs` | `value` property write row |
| `FocusIn(this ComponentRef<FusionTimePicker, TModel> self)` | `Alis.Reactive.Fusion/Components/FusionTimePicker/FusionTimePickerExtensions.cs` | `focusIn()` method row |
| `FocusOut(this ComponentRef<FusionTimePicker, TModel> self)` | `Alis.Reactive.Fusion/Components/FusionTimePicker/FusionTimePickerExtensions.cs` | `focusOut()` method row |
| `Value(this ComponentRef<FusionTimePicker, TModel> self)` | `Alis.Reactive.Fusion/Components/FusionTimePicker/FusionTimePickerExtensions.cs` | `value` property read row |
| component identity (`ValueMember`, registration) | `Alis.Reactive.Fusion/Components/FusionTimePicker/FusionTimePicker.cs` | n/a (component contract) |

## Slice File Inventory

The TimePicker slice follows the input-component isolation pattern. It does not
move behavior into shared base classes; duplication with other slices is
intentional.

- `Alis.Reactive.Fusion/Components/FusionTimePicker/FusionTimePicker.cs` — the sealed `FusionTimePicker : FusionComponent, IInputComponent`, its `value` member name, and input-component registration.
- `Alis.Reactive.Fusion/Components/FusionTimePicker/FusionTimePickerHtmlExtensions.cs` — the `FusionTimePicker(...)` render helper that registers the input component and renders the `TimePickerBuilder` bound to the model property.
- `Alis.Reactive.Fusion/Components/FusionTimePicker/FusionTimePickerExtensions.cs` — the post-render members `SetValue`, `FocusIn`, `FocusOut`, and `Value`.
- `Alis.Reactive.Fusion/Components/FusionTimePicker/FusionTimePickerEvents.cs` — the `Changed` event selector.
- `Alis.Reactive.Fusion/Components/FusionTimePicker/FusionTimePickerReactiveExtensions.cs` — the `Reactive(...)` event-to-plan wiring through `ComponentEventOnboarding.Wire`.
- `Alis.Reactive.Fusion/Components/FusionTimePicker/Events/FusionTimePickerOnChanged.cs` — the typed `FusionTimePickerChangeArgs` payload with `Value` and `IsInteracted`.

## Sandbox Surface

The behavior is exercised against a real sandbox view through the typed DSL.

- View: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/TimePicker/Index.cshtml`
- Controller: `Alis.Reactive.SandboxApp/Areas/Sandbox/Controllers/Components/Fusion/TimePickerController.cs`
- Model: `Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Components/Fusion/TimePicker/TimePickerModel.cs`
- Route: `http://localhost:5220/Sandbox/Components/TimePicker`

## Playwright Proof Surface

- Test file: `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/TimePicker/WhenTimeSelected.cs`
- Test class: `Alis.Reactive.PlaywrightTests.Components.Fusion.TimePicker.WhenTimeSelected`
- Per-member fails-when-broken mapping: `proof/behavioral-coverage.json`
- Detailed proof table: `proof/playwright-proof.md`

## Sync And Async Lanes

Every TimePicker row is sync: the field render is sync input registration; the
`change` component event trigger is sync; `SetValue` (property set), `FocusIn`,
`FocusOut`, and the `Value` read are sync component actions. The slice
introduces no async boundary of its own; async appears only when a developer
composes the `Value()` source into an HTTP pipeline, which is the HTTP
primitive.

## Out Of Scope

- TypeScript runtime changes (zero, by the vendor-isolation rule).
- New or broadened DSL primitives.
- Builder-owned static options (`format`, `step`, `min`, `max`, `placeholder`, `strictMode`, `cssClass`, `value`, templates, and the rest).
- The `show`/`hide` popup toggle methods, the `readonly` render flag, and the `requiredModules` lifecycle metadata.
