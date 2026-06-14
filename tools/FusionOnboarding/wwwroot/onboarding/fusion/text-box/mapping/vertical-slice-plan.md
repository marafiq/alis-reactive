# TextBox Vertical Slice Plan

Status: active and proven. Every accepted `FusionTextBox` member maps to an exact
vertical slice file and the primitive-map row that permits it. The slice adds
zero TypeScript runtime changes. The component is fully audited.

## Accepted Member To File Map

| Accepted C# member | Vertical slice file | Primitive-map row |
| --- | --- | --- |
| `FusionTextBox(this InputBoundField<TModel, TProp> setup, Action<TextBoxBuilder> build)` | `Alis.Reactive.Fusion/Components/FusionTextBox/FusionTextBoxHtmlExtensions.cs` | TextBox field render row |
| `FusionTextBoxEvents.Input` | `Alis.Reactive.Fusion/Components/FusionTextBox/FusionTextBoxEvents.cs` | `input` event trigger row |
| `FusionTextBoxEvents.Changed` | `Alis.Reactive.Fusion/Components/FusionTextBox/FusionTextBoxEvents.cs` | `change` event trigger row |
| `FusionTextBoxEvents.Focus` | `Alis.Reactive.Fusion/Components/FusionTextBox/FusionTextBoxEvents.cs` | `focus` event trigger row |
| `FusionTextBoxEvents.Blur` | `Alis.Reactive.Fusion/Components/FusionTextBox/FusionTextBoxEvents.cs` | `blur` event trigger row |
| `Reactive(...)` event wiring | `Alis.Reactive.Fusion/Components/FusionTextBox/FusionTextBoxReactiveExtensions.cs` | each event trigger row |
| `FusionTextBoxInputArgs.Value` | `Alis.Reactive.Fusion/Components/FusionTextBox/Events/FusionTextBoxOnInput.cs` | `input.value` payload read row |
| `FusionTextBoxInputArgs.PreviousValue` | `Alis.Reactive.Fusion/Components/FusionTextBox/Events/FusionTextBoxOnInput.cs` | `input.previousValue` payload read row |
| `FusionTextBoxChangeArgs.Value` | `Alis.Reactive.Fusion/Components/FusionTextBox/Events/FusionTextBoxOnChanged.cs` | `change.value` payload read row |
| `FusionTextBoxChangeArgs.PreviousValue` | `Alis.Reactive.Fusion/Components/FusionTextBox/Events/FusionTextBoxOnChanged.cs` | `change.previousValue` payload read row |
| `FusionTextBoxChangeArgs.IsInteracted` | `Alis.Reactive.Fusion/Components/FusionTextBox/Events/FusionTextBoxOnChanged.cs` | `change.isInteracted` payload read row |
| `FusionTextBoxFocusArgs.Value` | `Alis.Reactive.Fusion/Components/FusionTextBox/Events/FusionTextBoxOnFocus.cs` | `focus.value` payload read row |
| `FusionTextBoxBlurArgs.Value` | `Alis.Reactive.Fusion/Components/FusionTextBox/Events/FusionTextBoxOnBlur.cs` | `blur.value` payload read row |
| `SetValue(this ComponentRef<FusionTextBox, TModel> self, string? value)` | `Alis.Reactive.Fusion/Components/FusionTextBox/FusionTextBoxExtensions.cs` | `value` property write row plus `dataBind` repaint row |
| `FocusIn(this ComponentRef<FusionTextBox, TModel> self)` | `Alis.Reactive.Fusion/Components/FusionTextBox/FusionTextBoxExtensions.cs` | `focusIn()` method row |
| `FocusOut(this ComponentRef<FusionTextBox, TModel> self)` | `Alis.Reactive.Fusion/Components/FusionTextBox/FusionTextBoxExtensions.cs` | `focusOut()` method row |
| `AddAppendIcon(this ComponentRef<FusionTextBox, TModel> self, string iconCssClass)` | `Alis.Reactive.Fusion/Components/FusionTextBox/FusionTextBoxExtensions.cs` | `addIcon(position, icons)` method row |
| `Value(this ComponentRef<FusionTextBox, TModel> self)` | `Alis.Reactive.Fusion/Components/FusionTextBox/FusionTextBoxExtensions.cs` | `value` property read row |
| component identity (`ValueMember`, registration) | `Alis.Reactive.Fusion/Components/FusionTextBox/FusionTextBox.cs` | n/a (component contract) |

## Slice File Inventory

The TextBox slice follows the input-component isolation pattern. It does not move
behavior into shared base classes; duplication with other slices is intentional.

- `Alis.Reactive.Fusion/Components/FusionTextBox/FusionTextBox.cs` — the sealed `FusionTextBox : FusionComponent, IInputComponent`, its `value` member name, and input-component registration.
- `Alis.Reactive.Fusion/Components/FusionTextBox/FusionTextBoxHtmlExtensions.cs` — the `FusionTextBox(...)` render helper that registers the input component and renders the `TextBoxBuilder` bound to the model property.
- `Alis.Reactive.Fusion/Components/FusionTextBox/FusionTextBoxExtensions.cs` — the post-render members `SetValue`, `FocusIn`, `FocusOut`, `AddAppendIcon`, and `Value`.
- `Alis.Reactive.Fusion/Components/FusionTextBox/FusionTextBoxEvents.cs` — the `Input`, `Changed`, `Focus`, and `Blur` event selectors.
- `Alis.Reactive.Fusion/Components/FusionTextBox/FusionTextBoxReactiveExtensions.cs` — the `Reactive(...)` event-to-plan wiring through `ComponentEventOnboarding.Wire`.
- `Alis.Reactive.Fusion/Components/FusionTextBox/Events/FusionTextBoxOnInput.cs` — the typed `FusionTextBoxInputArgs` payload (`Value`, `PreviousValue`).
- `Alis.Reactive.Fusion/Components/FusionTextBox/Events/FusionTextBoxOnChanged.cs` — the typed `FusionTextBoxChangeArgs` payload (`Value`, `PreviousValue`, `IsInteracted`).
- `Alis.Reactive.Fusion/Components/FusionTextBox/Events/FusionTextBoxOnFocus.cs` — the typed `FusionTextBoxFocusArgs` payload (`Value`).
- `Alis.Reactive.Fusion/Components/FusionTextBox/Events/FusionTextBoxOnBlur.cs` — the typed `FusionTextBoxBlurArgs` payload (`Value`).

## Sandbox Surface

The behavior is exercised against a real sandbox view through the typed DSL — a
care coordinator updating a resident's profile card.

- View: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/TextBox/Index.cshtml`
- Controller: `Alis.Reactive.SandboxApp/Areas/Sandbox/Controllers/Components/Fusion/TextBoxController.cs`
- Model: `Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Components/Fusion/TextBox/TextBoxModel.cs`

## TypeScript Runtime

Zero TypeScript runtime changes. Every member maps to an existing DSL primitive
(component event trigger, event payload read, component property read, component
property write plus `dataBind`, component method call). The runtime stays
vendor-blind; the plan carries every member name the runtime resolves.
