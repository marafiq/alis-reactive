# NumericTextBox Vertical Slice Plan

Status: active and proven. Every accepted `FusionNumericTextBox` member maps to an
exact vertical slice file and the primitive-map row that permits it. The slice
adds zero TypeScript runtime changes. The component is fully audited.

## Accepted Member To File Map

| Accepted C# member | Vertical slice file | Primitive-map row |
| --- | --- | --- |
| `FusionNumericTextBox(this InputBoundField<TModel, TProp> setup, Action<NumericTextBoxBuilder> build)` | `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBoxHtmlExtensions.cs` | NumericTextBox field render row |
| `FusionNumericTextBoxEvents.Changed` | `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBoxEvents.cs` | `change` event trigger row |
| `FusionNumericTextBoxEvents.Focus` | `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBoxEvents.cs` | `focus` event trigger row |
| `FusionNumericTextBoxEvents.Blur` | `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBoxEvents.cs` | `blur` event trigger row |
| `Reactive(...)` event wiring | `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBoxReactiveExtensions.cs` | `change`/`focus`/`blur` event trigger rows |
| `FusionNumericTextBoxChangeArgs.Value` | `Alis.Reactive.Fusion/Components/FusionNumericTextBox/Events/FusionNumericTextBoxOnChanged.cs` | `change.value` payload read row |
| `FusionNumericTextBoxChangeArgs.PreviousValue` | `Alis.Reactive.Fusion/Components/FusionNumericTextBox/Events/FusionNumericTextBoxOnChanged.cs` | `change.previousValue` payload read row |
| `FusionNumericTextBoxChangeArgs.IsInteracted` | `Alis.Reactive.Fusion/Components/FusionNumericTextBox/Events/FusionNumericTextBoxOnChanged.cs` | `change.isInteracted` payload read row |
| `FusionNumericTextBoxFocusArgs` | `Alis.Reactive.Fusion/Components/FusionNumericTextBox/Events/FusionNumericTextBoxOnFocus.cs` | `focus` event trigger row |
| `FusionNumericTextBoxBlurArgs` | `Alis.Reactive.Fusion/Components/FusionNumericTextBox/Events/FusionNumericTextBoxOnBlur.cs` | `blur` event trigger row |
| `SetValue(this ComponentRef<FusionNumericTextBox, TModel> self, decimal value)` | `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBoxExtensions.cs` | `value` property write row |
| `SetMin(this ComponentRef<FusionNumericTextBox, TModel> self, decimal min)` | `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBoxExtensions.cs` | `min` property write row |
| `Increment(this ComponentRef<FusionNumericTextBox, TModel> self)` | `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBoxExtensions.cs` | `increment()` method row |
| `Decrement(this ComponentRef<FusionNumericTextBox, TModel> self)` | `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBoxExtensions.cs` | `decrement()` method row |
| `FocusIn(this ComponentRef<FusionNumericTextBox, TModel> self)` | `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBoxExtensions.cs` | `focusIn()` method row |
| `FocusOut(this ComponentRef<FusionNumericTextBox, TModel> self)` | `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBoxExtensions.cs` | `focusOut()` method row |
| `Value(this ComponentRef<FusionNumericTextBox, TModel> self)` | `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBoxExtensions.cs` | `value` property read row |
| component identity (`ValueMember`, registration) | `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBox.cs` | n/a (component contract) |

## Slice File Inventory

The NumericTextBox slice follows the input-component isolation pattern. It does
not move behavior into shared base classes; duplication with other slices is
intentional.

- `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBox.cs` — the sealed `FusionNumericTextBox : FusionComponent, IInputComponent`, its `value` member name, and input-component registration.
- `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBoxHtmlExtensions.cs` — the `FusionNumericTextBox(...)` render helper that registers the input component and renders the `NumericTextBoxBuilder` bound to the model property.
- `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBoxExtensions.cs` — the post-render members `SetValue`, `SetMin`, `Increment`, `Decrement`, `FocusIn`, `FocusOut`, and `Value`.
- `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBoxEvents.cs` — the `Changed`, `Focus`, and `Blur` event selectors.
- `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBoxReactiveExtensions.cs` — the `Reactive(...)` event-to-plan wiring through `ComponentEventOnboarding.Wire`.
- `Alis.Reactive.Fusion/Components/FusionNumericTextBox/Events/FusionNumericTextBoxOnChanged.cs` — the typed `FusionNumericTextBoxChangeArgs` payload with `Value`, `PreviousValue`, and `IsInteracted`.
- `Alis.Reactive.Fusion/Components/FusionNumericTextBox/Events/FusionNumericTextBoxOnFocus.cs` — the empty typed `FusionNumericTextBoxFocusArgs`.
- `Alis.Reactive.Fusion/Components/FusionNumericTextBox/Events/FusionNumericTextBoxOnBlur.cs` — the empty typed `FusionNumericTextBoxBlurArgs`.

## Sandbox Surface

The behavior is exercised against a real sandbox view through the typed DSL.

- View: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/NumericTextBox/Index.cshtml`
- Controller: `Alis.Reactive.SandboxApp/Areas/Sandbox/Controllers/Components/Fusion/NumericTextBoxController.cs`
- Model: `Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Components/Fusion/NumericTextBox/NumericTextBoxModel.cs`
- Route: `http://localhost:5220/Sandbox/Components/NumericTextBox`

## Playwright Proof Surface

- Test file: `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/NumericTextBox/WhenNumericValueEntered.cs`
- Test class: `Alis.Reactive.PlaywrightTests.Components.Fusion.NumericTextBox.WhenNumericValueEntered`
- Per-member fails-when-broken mapping: `proof/behavioral-coverage.json`
- Detailed proof table: `proof/playwright-proof.md`

## Sync And Async Lanes

Every NumericTextBox row is sync: the field render is sync input registration;
the `change`/`focus`/`blur` component event triggers are sync; `SetValue`,
`SetMin`, `Increment`, `Decrement`, `FocusIn`, `FocusOut`, and the `Value` read
are sync component actions. The slice introduces no async boundary of its own;
async appears only when a developer composes the `Value()` source into an HTTP
pipeline, which is the HTTP primitive.

## Out Of Scope

- TypeScript runtime changes (zero, by the vendor-isolation rule).
- New or broadened DSL primitives.
- Builder-owned static properties (`allowMouseWheel`, `cssClass`, `currency`, `decimals`, `format`, `max`, `placeholder`, `showClearButton`, `showSpinButton`, `step`, `strictMode`, `width`, templates, and the rest).
- The `getText` formatted-string read, the `readonly` property, the `created`/`destroyed` lifecycle events, and the `destroy` lifecycle method.
