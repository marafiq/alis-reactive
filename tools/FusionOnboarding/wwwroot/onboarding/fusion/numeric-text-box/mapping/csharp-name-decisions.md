# NumericTextBox C# Name Decisions

Status: active and proven. The `FusionNumericTextBox` public C# names are decided
and implemented: the `FusionNumericTextBox(...)` render helper, the `Changed`,
`Focus`, and `Blur` event selectors with their typed payloads, the typed
`SetValue(decimal)` and `SetMin(decimal)` writes, the `Increment()`,
`Decrement()`, `FocusIn()`, and `FocusOut()` methods, and the `Value()` read
source. The component is fully audited.

## Pass Rows

Close matrix row: `Html.InputField(m => m.MealsPerWeek).FusionNumericTextBox(b => ...)` render helper -> NumericTextBox field bound to a numeric model property.

Close matrix row: `field.Reactive(e => e.Changed, ...)` -> typed `FusionNumericTextBoxChangeArgs` payload; `.Focus` / `.Blur` -> typed `FusionNumericTextBoxFocusArgs` / `FusionNumericTextBoxBlurArgs`.

Close matrix row: `SetValue(decimal)`, `SetMin(decimal)`, `Increment()`, `Decrement()`, `FocusIn()`, `FocusOut()`, `Value()` -> typed NumericTextBox runtime members.

## Evidence Inputs

- Raw core trace: `traces/raw-ej2-core.trace.json`
- Raw core probe: `probes/raw-ej2-core.html`
- Syncfusion source type: `ChangeEventArgs`, `NumericFocusEventArgs`, `NumericBlurEventArgs` (events), `NumericTextBox` (component)
- Public API surface: `discovery/public-api-surface.json`
- Event payload surface: `discovery/event-payload-surface.json`
- Blazor candidates: `discovery/blazor-candidates.md` (no Blazor package supplied; naming taken from EJ2 source only)
- Existing C# event args: `Alis.Reactive.Fusion/Components/FusionNumericTextBox/Events/FusionNumericTextBoxOnChanged.cs`, `.../FusionNumericTextBoxOnFocus.cs`, `.../FusionNumericTextBoxOnBlur.cs`
- Existing event selector: `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBoxEvents.cs`
- Existing component members: `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBoxExtensions.cs`
- Existing render helper: `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBoxHtmlExtensions.cs`
- Existing sandbox usage: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/NumericTextBox/Index.cshtml`

## Name Decision Matrix

| Syncfusion path | C# name | Decision | Reason |
| --- | --- | --- | --- |
| `new ej.inputs.NumericTextBox(options)` field render | `InputBoundField<TModel, TProp>.FusionNumericTextBox(Action<NumericTextBoxBuilder> build)` | keep | the render helper binds the EJ2 NumericTextBox to a numeric model property through the standard `Html.InputField` field wrapper; initial options stay on `NumericTextBoxBuilder` |
| `change` event | `FusionNumericTextBoxEvents.Changed` | keep | the C# selector `Changed` reads as the value-change event; selected through the typed `.Reactive(e => e.Changed, ...)` lambda. EJ2 path stays `change` in the plan member name |
| `focus` event | `FusionNumericTextBoxEvents.Focus` | keep | exact Syncfusion event name; fires on focus gain |
| `blur` event | `FusionNumericTextBoxEvents.Blur` | keep | exact Syncfusion event name; fires on focus loss |
| `ChangeEventArgs` | `FusionNumericTextBoxChangeArgs` | keep | the Fusion payload type name states the event it belongs to; it carries only the proven, narrowed members |
| `change.value` | `FusionNumericTextBoxChangeArgs.Value` | keep | exact Syncfusion key, typed as `decimal`; the new number |
| `change.previousValue` | `FusionNumericTextBoxChangeArgs.PreviousValue` | keep | exact Syncfusion key, typed as `decimal`; the number before the change |
| `change.isInteracted` | `FusionNumericTextBoxChangeArgs.IsInteracted` | keep | exact Syncfusion key, typed as `bool`; distinguishes a user-typed change from a programmatic change |
| `NumericFocusEventArgs` | `FusionNumericTextBoxFocusArgs` | keep | typed focus payload; carries no data the DSL reads, so the args type is intentionally empty |
| `NumericBlurEventArgs` | `FusionNumericTextBoxBlurArgs` | keep | typed blur payload; carries no data the DSL reads, so the args type is intentionally empty |
| `value` property write | `SetValue(this ComponentRef<FusionNumericTextBox, TModel> self, decimal value)` | keep | states developer intent ("set the value"); maps to a `value` property set |
| `min` property write | `SetMin(this ComponentRef<FusionNumericTextBox, TModel> self, decimal min)` | keep | states developer intent ("set the minimum"); maps to a `min` property set that re-clamps subsequent input |
| `increment()` method | `Increment(this ComponentRef<FusionNumericTextBox, TModel> self)` | keep | exact Syncfusion method name; raises the value by one step |
| `decrement()` method | `Decrement(this ComponentRef<FusionNumericTextBox, TModel> self)` | keep | exact Syncfusion method name; lowers the value by one step |
| `focusIn()` method | `FocusIn(this ComponentRef<FusionNumericTextBox, TModel> self)` | keep | exact Syncfusion method name; moves the cursor into the field |
| `focusOut()` method | `FocusOut(this ComponentRef<FusionNumericTextBox, TModel> self)` | keep | exact Syncfusion method name; moves the cursor out of the field |
| `value` property read | `Value(this ComponentRef<FusionNumericTextBox, TModel> self)` | keep | concise read name returns a typed `decimal` source for gather/conditions/set text |
| `getText()` method | none | exclude from public typed surface | returns the vendor-formatted display string (Format/Currency/Decimals applied), which the builder already owns; duplicates the typed `Value()` read with no distinct typed use case (see `discovery/parity-accounting.json`) |
| `readonly` property | none | exclude as builder-owned | `numerictextbox.d.ts:139` initial read-only configuration; the MVC builder owns it at render, no post-render read/write proven |
| `created`, `destroyed` events | none | exclude for the current rows | lifecycle-only events with no typed payload the DSL reads |
| 24 builder-covered properties (`allowMouseWheel`, `appendTemplate`, `cssClass`, `currency`, `decimals`, `enabled`, `enablePersistence`, `floatLabelType`, `format`, `max`, `placeholder`, `prependTemplate`, `showClearButton`, `showSpinButton`, `step`, `strictMode`, `validateDecimalOnType`, `width`, initial `value`/`min`, templates) | none | exclude as builder-owned | `discovery/public-api-surface.json` marks each `builder.covered = true`; configured on `NumericTextBoxBuilder` at initial render, no post-render read/write proven necessary beyond the accepted `value`/`min` writes |
| `destroy()` method | none | exclude as lifecycle | `discovery/public-api-surface.json` classifies it `skip: lifecycle cleanup`, not plan behavior |

## Blazor Naming Rule

Blazor metadata may be used only as naming evidence after the EJ2 row is proven.
`discovery/blazor-candidates.md` records that no Syncfusion Blazor package was
supplied for this pass, so every accepted C# name above comes from the EJ2 source
and the raw core trace, not from Blazor metadata.

## Discovery Versus C# DSL Boundary

All observed fields stay in discovery. Only fields with clear, typed,
predictable Fusion use cases are accepted into the public C# surface. `getText`
remains discovered but excluded because it is a format-coupled string read the
builder already owns and the typed `Value()` source already covers. `readonly`
and the 24 builder-covered properties remain discovered but excluded because the
Syncfusion MVC builder owns initial render configuration and no post-render
read/write is proven necessary. The `created`/`destroyed` lifecycle events carry
no typed payload the DSL reads.

## Implementation Boundary

Implemented public surface for the NumericTextBox slice:

- the `FusionNumericTextBox(...)` render helper bound to a numeric model property;
- the `Changed` event selector and `FusionNumericTextBoxChangeArgs` payload with `Value`, `PreviousValue`, and `IsInteracted`;
- the `Focus` and `Blur` event selectors with their empty typed payloads;
- the `SetValue(decimal)` and `SetMin(decimal)` writes;
- the `Increment()`, `Decrement()`, `FocusIn()`, and `FocusOut()` methods;
- the `Value()` read source.

Out of scope for the NumericTextBox slice: new primitives, builder-owned static
properties, the `getText` formatted-string read, the `readonly` property, the
`created`/`destroyed` lifecycle events, and the `destroy` lifecycle method.
