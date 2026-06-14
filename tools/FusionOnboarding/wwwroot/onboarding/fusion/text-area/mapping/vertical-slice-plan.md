# TextArea Vertical Slice Plan

Status: active and proven. Every accepted `FusionTextArea` member maps to an
exact vertical slice file and the primitive-map row that permits it. The slice
adds zero TypeScript runtime changes. The component is fully audited.

## Accepted Member To File Map

| Accepted C# member | Vertical slice file | Primitive-map row |
| --- | --- | --- |
| `FusionTextArea(this InputBoundField<TModel, TProp> setup, Action<TextAreaBuilder> build)` | `Alis.Reactive.Fusion/Components/FusionTextArea/FusionTextAreaHtmlExtensions.cs` | TextArea field render row |
| `FusionTextAreaEvents.Input` | `Alis.Reactive.Fusion/Components/FusionTextArea/FusionTextAreaEvents.cs` | `input` event trigger row |
| `FusionTextAreaEvents.Changed` | `Alis.Reactive.Fusion/Components/FusionTextArea/FusionTextAreaEvents.cs` | `change` event trigger row |
| `FusionTextAreaEvents.Focus` | `Alis.Reactive.Fusion/Components/FusionTextArea/FusionTextAreaEvents.cs` | `focus` event trigger row |
| `FusionTextAreaEvents.Blur` | `Alis.Reactive.Fusion/Components/FusionTextArea/FusionTextAreaEvents.cs` | `blur` event trigger row |
| `Reactive(...)` event wiring | `Alis.Reactive.Fusion/Components/FusionTextArea/FusionTextAreaReactiveExtensions.cs` | the four event trigger rows |
| `FusionTextAreaInputArgs.Value` | `Alis.Reactive.Fusion/Components/FusionTextArea/Events/FusionTextAreaOnInput.cs` | `input.value` payload read row |
| `FusionTextAreaInputArgs.PreviousValue` | `Alis.Reactive.Fusion/Components/FusionTextArea/Events/FusionTextAreaOnInput.cs` | `input.previousValue` payload read row |
| `FusionTextAreaChangeArgs.Value` | `Alis.Reactive.Fusion/Components/FusionTextArea/Events/FusionTextAreaOnChanged.cs` | `change.value` payload read row |
| `FusionTextAreaChangeArgs.PreviousValue` | `Alis.Reactive.Fusion/Components/FusionTextArea/Events/FusionTextAreaOnChanged.cs` | `change.previousValue` payload read row |
| `FusionTextAreaChangeArgs.IsInteracted` | `Alis.Reactive.Fusion/Components/FusionTextArea/Events/FusionTextAreaOnChanged.cs` | `change.isInteracted` payload read row |
| `FusionTextAreaFocusArgs.Value` | `Alis.Reactive.Fusion/Components/FusionTextArea/Events/FusionTextAreaOnFocus.cs` | `focus.value` payload read row |
| `FusionTextAreaBlurArgs.Value` | `Alis.Reactive.Fusion/Components/FusionTextArea/Events/FusionTextAreaOnBlur.cs` | `blur.value` payload read row |
| `SetValue(this ComponentRef<FusionTextArea, TModel> self, string? value)` | `Alis.Reactive.Fusion/Components/FusionTextArea/FusionTextAreaExtensions.cs` | `value` property write row plus `dataBind` repaint row |
| `FocusIn(this ComponentRef<FusionTextArea, TModel> self)` | `Alis.Reactive.Fusion/Components/FusionTextArea/FusionTextAreaExtensions.cs` | `focusIn()` method row |
| `FocusOut(this ComponentRef<FusionTextArea, TModel> self)` | `Alis.Reactive.Fusion/Components/FusionTextArea/FusionTextAreaExtensions.cs` | `focusOut()` method row |
| `Value(this ComponentRef<FusionTextArea, TModel> self)` | `Alis.Reactive.Fusion/Components/FusionTextArea/FusionTextAreaExtensions.cs` | `value` property read row |
| component identity (`ValueMember`, registration) | `Alis.Reactive.Fusion/Components/FusionTextArea/FusionTextArea.cs` | n/a (component contract) |

## Slice File Inventory

The TextArea slice follows the input-component isolation pattern. It does not
move behavior into shared base classes; duplication with other slices is
intentional.

- `Alis.Reactive.Fusion/Components/FusionTextArea/FusionTextArea.cs` — the sealed `FusionTextArea : FusionComponent, IInputComponent`, its `value` member name, and input-component registration.
- `Alis.Reactive.Fusion/Components/FusionTextArea/FusionTextAreaHtmlExtensions.cs` — the `FusionTextArea(...)` render helper that registers the input component and renders the `TextAreaBuilder` bound to the model property.
- `Alis.Reactive.Fusion/Components/FusionTextArea/FusionTextAreaExtensions.cs` — the post-render members `SetValue`, `FocusIn`, `FocusOut`, and `Value`.
- `Alis.Reactive.Fusion/Components/FusionTextArea/FusionTextAreaEvents.cs` — the `Input`, `Changed`, `Focus`, and `Blur` event selectors.
- `Alis.Reactive.Fusion/Components/FusionTextArea/FusionTextAreaReactiveExtensions.cs` — the `Reactive(...)` event-to-plan wiring through `ComponentEventOnboarding.Wire`.
- `Alis.Reactive.Fusion/Components/FusionTextArea/Events/FusionTextAreaOnInput.cs` — the typed `FusionTextAreaInputArgs` payload with `Value` and `PreviousValue`.
- `Alis.Reactive.Fusion/Components/FusionTextArea/Events/FusionTextAreaOnChanged.cs` — the typed `FusionTextAreaChangeArgs` payload with `Value`, `PreviousValue`, and `IsInteracted`.
- `Alis.Reactive.Fusion/Components/FusionTextArea/Events/FusionTextAreaOnFocus.cs` — the typed `FusionTextAreaFocusArgs` payload with `Value`.
- `Alis.Reactive.Fusion/Components/FusionTextArea/Events/FusionTextAreaOnBlur.cs` — the typed `FusionTextAreaBlurArgs` payload with `Value`.

## Sandbox Surface

The behavior is exercised against a real sandbox view through the typed DSL.

- View: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/TextArea/Index.cshtml`
- Controller: `Alis.Reactive.SandboxApp/Areas/Sandbox/Controllers/Components/Fusion/TextAreaController.cs`
- Model: `Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Components/Fusion/TextArea/TextAreaModel.cs` (`CareNote` string property)
- Route: `http://localhost:5220/Sandbox/Components/Fusion/TextArea`

## Playwright Proof Surface

- Test file: `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/TextArea/WhenUsingFusionTextArea.cs`
- Test class: `Alis.Reactive.PlaywrightTests.Components.Fusion.TextArea.WhenUsingFusionTextArea`
- Per-member fails-when-broken mapping: `proof/behavioral-coverage.json`
- Detailed proof table: `proof/playwright-proof.md`

## Sync And Async Lanes

Every TextArea row is sync: the field render is sync input registration; the
`input`, `change`, `focus`, and `blur` component event triggers are sync;
`SetValue` (property set plus `dataBind`), `FocusIn`, `FocusOut`, and the
`Value` read are sync component actions. The slice introduces no async boundary
of its own; async appears only when a developer composes the `Value()` source
into an HTTP pipeline, which is the HTTP primitive.

## Out Of Scope

- TypeScript runtime changes (zero, by the vendor-isolation rule).
- New or broadened DSL primitives.
- Builder-owned static properties (`adornmentFlow`, `cols`, `cssClass`, `enabled`, `floatLabelType`, `maxLength`, `placeholder`, `resizeMode`, `rows`, `showClearButton`, `width`, and the rest).
- The raw attribute methods, the `readonly` toggle, the `created`/`destroyed` lifecycle events, and the `destroy` lifecycle method.
