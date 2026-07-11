# NumericTextBox Primitive Map

Status: active and proven. This file maps the onboarded `FusionNumericTextBox`
runtime surface: the `change`, `focus`, and `blur` events with their typed
payloads, the typed `value` and `min` writes, the `increment`/`decrement`/
`focusIn`/`focusOut` methods, the `value` read source, and the
`FusionNumericTextBox(...)` field render helper. Every mapped row uses an
existing DSL primitive. The component is fully audited.

## Pass Rows

Close matrix row: `Html.InputField(m => m.MealsPerWeek).FusionNumericTextBox(b => ...)` -> NumericTextBox field render bound to a numeric model property -> sync input registration plus initial value/min/max/step bound by the Syncfusion builder.

Close matrix row: `field.Reactive(e => e.Changed, (args, p) => ...)` value-change trigger -> NumericTextBox `change` payload (`value`, `previousValue`, `isInteracted`) -> sync component-event reaction reading the typed payload.

Close matrix row: `field.Reactive(e => e.Focus, ...)` / `.Reactive(e => e.Blur, ...)` -> NumericTextBox `focus` / `blur` events (no payload data) -> sync component-event reactions that run side effects on focus gain/loss.

Close matrix row: `p.Component<FusionNumericTextBox>(m => m.MealsPerWeek).SetValue(value)` -> typed NumericTextBox value write -> sync component property set on `value`.

Close matrix row: `p.Component<FusionNumericTextBox>(m => m.MealsPerWeek).SetMin(min)` -> typed NumericTextBox minimum write -> sync component property set on `min` that re-clamps subsequent input.

Close matrix row: `p.Component<FusionNumericTextBox>(id).Increment()` / `.Decrement()` -> typed NumericTextBox step methods -> sync `increment` / `decrement` method calls that change the value by one step and emit a `change` event.

Close matrix row: `p.Component<FusionNumericTextBox>(id).FocusIn()` / `.FocusOut()` -> typed NumericTextBox focus methods -> sync `focusIn` / `focusOut` method calls that move the cursor into/out of the field and emit `focus` / `blur`.

Close matrix row: `field.Value()` -> NumericTextBox value read source -> sync component property read of `value` consumed by gather, conditions, or set text.

## DSL Source Requirements

- `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBox.cs`
- `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBoxHtmlExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBoxExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBoxEvents.cs`
- `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBoxReactiveExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionNumericTextBox/Events/FusionNumericTextBoxOnChanged.cs`
- `Alis.Reactive.Fusion/Components/FusionNumericTextBox/Events/FusionNumericTextBoxOnFocus.cs`
- `Alis.Reactive.Fusion/Components/FusionNumericTextBox/Events/FusionNumericTextBoxOnBlur.cs`
- `Alis.Reactive/Components/Contracts/ComponentRef.cs`
- `Alis.Reactive/Components/Contracts/ComponentProperty.cs`
- `Alis.Reactive/Components/Contracts/ComponentMember.cs`
- `Alis.Reactive/Components/Onboarding/ComponentEventOnboarding.cs`
- `Alis.Reactive/PlanModel/Values/ValueExpression.cs`

Sync/async lane expectation: the field render is sync input registration; the
`change`/`focus`/`blur` component-event triggers are sync; `SetValue`, `SetMin`,
`Increment`, `Decrement`, `FocusIn`, `FocusOut`, and the `Value` read are sync
component actions. The NumericTextBox slice introduces no async boundary. Async
only appears when a developer composes the read `Value()` source into an HTTP
`Post(...).Gather(...)` pipeline, which is the HTTP primitive, not a
NumericTextBox concern.

## Authoritative Primitive Rows

| NumericTextBox row member | Syncfusion evidence | DSL primitive | Domain/JSON term | Runtime behavior | Status |
| --- | --- | --- | --- | --- | --- |
| `value` property read | `traces/raw-ej2-core.trace.json` instantiates `new ej.inputs.NumericTextBox`; `numerictextbox.d.ts:value: number`; `value` is a number on the instance | `ComponentProperty<decimal>.Named("value")` + `self.Read(property)` | `ValueExpression.Read(ComponentObject, "value", Shape.Number)` from `FusionNumericTextBoxExtensions.Value(...)` | runtime reads `numerictextbox.value` into a typed decimal source | accepted and proven |
| `value` property write | core trace `prototype methods` includes `changeValue`/`change`; raw EJ2 reflects a `value` set into the visible input | `ComponentProperty<decimal>` + `self.EmitSet(property, ValueExpression.Literal(value))` | `SetReaction` targeting component property `value` | runtime writes `numerictextbox.value = literal`; the field repaints to the new number | accepted and proven |
| `min` property write | `numerictextbox.d.ts:min: number`; raw EJ2 re-clamps typed input to the new minimum after a `min` set | `ComponentProperty<decimal>.Named("min")` + `self.EmitSet(property, ValueExpression.Literal(min))` | `SetReaction` targeting component property `min` | runtime writes `numerictextbox.min = literal`; subsequent below-floor entries clamp to the new minimum | accepted and proven |
| `increment()` method | core trace `prototype methods` includes `increment` family; `numerictextbox.d.ts:373 increment(step?: number): void` | `ComponentMethod.Named("increment")` + `self.EmitCall(method)` | `CallReaction` targeting component method `increment` | runtime invokes `numerictextbox.increment()`; the value rises one step and a `change` event fires | accepted and proven |
| `decrement()` method | `numerictextbox.d.ts:381 decrement(step?: number): void` | `ComponentMethod.Named("decrement")` + `self.EmitCall(method)` | `CallReaction` targeting component method `decrement` | runtime invokes `numerictextbox.decrement()`; the value drops one step and a `change` event fires | accepted and proven |
| `focusIn()` method | core trace `prototype methods` includes `focusIn`; `numerictextbox.d.ts:401 focusIn(): void` | `ComponentMethod.Named("focusIn")` + `self.EmitCall(method)` | `CallReaction` targeting component method `focusIn` | runtime invokes `numerictextbox.focusIn()`; the field gains focus and a `focus` event fires | accepted and proven |
| `focusOut()` method | core trace `prototype methods` includes `focusOut`; `numerictextbox.d.ts:407 focusOut(): void` | `ComponentMethod.Named("focusOut")` + `self.EmitCall(method)` | `CallReaction` targeting component method `focusOut` | runtime invokes `numerictextbox.focusOut()`; the field loses focus and a `blur` event fires | accepted and proven |
| `change` event trigger | core trace `prototype methods` includes `change`; `numerictextbox.d.ts:change: EmitType<ChangeEventArgs>` | `TypedEvent<FusionNumericTextBoxChangeArgs>` selected by `.Reactive(...)` | `StartsWhen.ComponentEvent(componentId, "change")` | runtime wires the Syncfusion object `change` event and starts the reaction with event payload scope | accepted and proven |
| `change.value` | `event-payload-surface.json` `ChangeEventArgs.value: number` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "value", Shape.Number)` from `FusionNumericTextBoxChangeArgs.Value` | runtime reads `event.value` (the new number) into set text, condition, or gather | accepted and proven |
| `change.previousValue` | `event-payload-surface.json` `ChangeEventArgs.previousValue: number` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "previousValue", Shape.Number)` from `FusionNumericTextBoxChangeArgs.PreviousValue` | runtime reads `event.previousValue` (the value before the change) into visible text | accepted and proven |
| `change.isInteracted` | `event-payload-surface.json` `ChangeEventArgs.isInteracted: boolean` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "isInteracted", Shape.Boolean)` from `FusionNumericTextBoxChangeArgs.IsInteracted` | runtime reads `event.isInteracted` to distinguish a user-typed change (true) from a programmatic change (false) | accepted and proven |
| `focus` event trigger | core trace `prototype methods` exposes the focus lane; `numerictextbox.d.ts:focus: EmitType<NumericFocusEventArgs>` | `TypedEvent<FusionNumericTextBoxFocusArgs>` selected by `.Reactive(...)` | `StartsWhen.ComponentEvent(componentId, "focus")` | runtime wires the Syncfusion object `focus` event and starts the reaction; payload carries no data the DSL reads | accepted and proven |
| `blur` event trigger | core trace `prototype methods` includes `blur`; `numerictextbox.d.ts:blur: EmitType<NumericBlurEventArgs>` | `TypedEvent<FusionNumericTextBoxBlurArgs>` selected by `.Reactive(...)` | `StartsWhen.ComponentEvent(componentId, "blur")` | runtime wires the Syncfusion object `blur` event and starts the reaction; payload carries no data the DSL reads | accepted and proven |
| `getText()` method | `numerictextbox.d.ts:395 getText(): string` | not accepted for the current rows | no public C# member | no runtime mapping for this row | excluded; returns the vendor-formatted display string the builder already controls (Format/Currency/Decimals), duplicating the typed `Value()` read with no distinct typed use case — see `discovery/parity-accounting.json` |
| `readonly` property | `numerictextbox.d.ts:139 readonly: boolean` | builder-owned static configuration | no runtime DSL member | initial read-only state configured on `NumericTextBoxBuilder`; no post-render read/write proven | excluded; builder-owned, no proven post-render read/write — see `discovery/parity-accounting.json` |
| 24 builder-covered properties (`allowMouseWheel`, `appendTemplate`, `cssClass`, `currency`, `decimals`, `enabled`, `enablePersistence`, `floatLabelType`, `format`, `max`, `min` initial, `placeholder`, `prependTemplate`, `showClearButton`, `showSpinButton`, `step`, `strictMode`, `validateDecimalOnType`, `value` initial, `width`, and the `created`/`destroyed` lifecycle events) | `public-api-surface.json` marks each `builder.covered = true` | builder-owned static configuration | no runtime DSL member | initial render configured on `NumericTextBoxBuilder`; no post-render read/write proven necessary beyond the accepted `value`/`min` writes | excluded; builder-owned per `references/automation-gates.md` Gate 5 builder-owned exclusion |
| `destroy()` method | `public-api-surface.json` classifies it `skip: lifecycle cleanup` | not a Fusion plan behavior | no runtime DSL member | runtime never calls it from a plan | excluded; lifecycle cleanup, not plan behavior |

## Primitive Decision

No new primitive is needed for the mapped NumericTextBox rows. Current primitives
already cover every onboarded member:

- component event trigger (`change`, `focus`, `blur`);
- event payload read (`value`, `previousValue`, `isInteracted`);
- component property read (`value`);
- component property write from a literal (`value`, `min`);
- component method call (`increment`, `decrement`, `focusIn`, `focusOut`).

Any future failure to read one of these accepted members is a
discovery/mapping/typed-contract problem first, never permission to add a
primitive.

## Code To Delete Or Simplify

None identified for the primitive layer. The slice keeps `SetMin` as a typed
write of the `min` property rather than a stringly setter, so the re-clamp is an
explicit mapped row rather than hidden behavior.

## Behavior Proof Required Before Commit

The NumericTextBox rows are proven by
`tests/Alis.Reactive.PlaywrightTests/Components/Fusion/NumericTextBox/WhenNumericValueEntered.cs`
through `scripts/playwright.sh`, with each member tied to a fails-when-broken
assertion in `proof/behavioral-coverage.json`:

1. `FusionNumericTextBox(...)` render binds the model value (the plan opens showing the carried-over meals/wellness counts);
2. `change` fires the reaction and `value` shows the new number with its summary;
3. `previousValue` records what the count changed from;
4. `isInteracted` distinguishes a typed entry from a template-applied value;
5. `SetValue(value)` writes the standard-plan count; `SetMin(min)` lowers the floor so a below-floor value sticks;
6. `Increment()`/`Decrement()` change the value by one step;
7. `focus`/`Focus`/`FocusIn` show the guidance; `blur`/`Blur`/`FocusOut` tidy it;
8. `Value()` yields the current count into a POST gather body.
