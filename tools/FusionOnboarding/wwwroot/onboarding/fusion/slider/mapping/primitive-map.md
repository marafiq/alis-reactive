# Slider Primitive Map

Status: active and proven. This file maps the onboarded `FusionSlider` runtime
surface: the `change` and `changed` events with their typed payload (`Value`,
`PreviousValue`, `Text`, `Action`, `IsInteracted`), the scalar `SetValue` write,
the two-handle `SetRangeValue` write, the scalar `Value` read source, the
`RangeValue` read source, and the `FusionSlider(...)` field render helper. Every
mapped row uses an existing DSL primitive. The component is fully audited.

## Pass Rows

Close matrix row: `Html.InputField(m => m.RoomTemperature).FusionSlider(b => ...)` -> Slider field render bound to a numeric model property -> sync input registration plus initial value bound by the Syncfusion builder.

Close matrix row: `slider.Reactive(e => e.Change, (args, p) => ...)` change trigger -> Slider `change` payload (`value`, `previousValue`, `text`, `action`, `isInteracted`) -> sync component-event reaction reading the typed payload as the handle moves.

Close matrix row: `slider.Reactive(e => e.Changed, (args, p) => ...)` changed trigger -> Slider `changed` payload (same keys; `previousValue` carries the value before the settled change) -> sync component-event reaction reading the typed payload when the handle settles.

Close matrix row: `p.Component<FusionSlider>(id).SetValue(value)` -> typed Slider scalar value write -> sync component property set on `value` followed by a `dataBind` method call that repaints the handle.

Close matrix row: `p.Component<FusionSlider>(id).SetRangeValue(start, end)` -> typed Slider two-handle value write -> sync component property set on `value` (raw number array) under the distinct plan member `rangeValue`, followed by a `dataBind` call that repaints both handles.

Close matrix row: `slider.Value()` -> Slider scalar value read source -> sync component property read of `value` consumed by gather, conditions, or set text.

Close matrix row: `slider.RangeValue()` -> Slider two-handle value read source -> sync component property read of `value` as a number array consumed by gather or set text.

## DSL Source Requirements

- `Alis.Reactive.Fusion/Components/FusionSlider/FusionSlider.cs`
- `Alis.Reactive.Fusion/Components/FusionSlider/FusionSliderHtmlExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionSlider/FusionSliderExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionSlider/FusionSliderEvents.cs`
- `Alis.Reactive.Fusion/Components/FusionSlider/FusionSliderReactiveExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionSlider/Events/FusionSliderOnChanged.cs`
- `Alis.Reactive/Components/Contracts/ComponentRef.cs`
- `Alis.Reactive/Components/Contracts/ComponentProperty.cs`
- `Alis.Reactive/Components/Contracts/ComponentMember.cs`
- `Alis.Reactive/Components/Onboarding/ComponentEventOnboarding.cs`
- `Alis.Reactive/PlanModel/Values/ValueExpression.cs`

Sync/async lane expectation: the slider field render is sync input registration;
the `change` and `changed` component-event triggers are sync; `SetValue`,
`SetRangeValue`, the `Value` read, and the `RangeValue` read are sync component
actions. The Slider slice introduces no async boundary. Async only appears when a
developer composes the read `Value()`/`RangeValue()` sources into an HTTP
`Post(...).Gather(...)` pipeline, which is the HTTP primitive, not a Slider concern.

## Authoritative Primitive Rows

| Slider row member | Syncfusion evidence | DSL primitive | Domain/JSON term | Runtime behavior | Status |
| --- | --- | --- | --- | --- | --- |
| `value` scalar read | `traces/raw-ej2-core.trace.json` constructs `Slider({ value })`; `value` is a number on the instance | `ComponentProperty<double>.Named("value")` + `self.Read(property)` | `ValueExpression.Read(ComponentObject, "value", Shape.Number)` from `FusionSliderExtensions.Value(...)` | runtime reads `slider.value` into a typed double source | accepted and proven |
| `value` scalar write | core trace `prototype methods` includes `dataBind`; raw EJ2 repaints the handle after `value` set + `dataBind()` | `ComponentProperty<double>` + `self.EmitSet(property, ValueExpression.Literal(value))` then `EmitCall("dataBind")` | `SetReaction` targeting component property `value`, then `CallReaction` for `dataBind` | runtime writes `slider.value = literal` and calls `slider.dataBind()` so the visible handle moves | accepted and proven |
| `value` range read | `slider.d.ts` `value: number \| number[]`; range slider exposes `value` as a two-number array | `ComponentProperty<double[]>.Mapped("rangeValue", "value")` + `self.Read(property)` | `ValueExpression.Read(ComponentObject, "value", Shape.ArrayOf(Shape.Number))` from `FusionSliderExtensions.RangeValue(...)` | runtime reads `slider.value` as a number array into a typed source | accepted and proven |
| `value` range write | core trace `prototype methods` includes `dataBind`; raw EJ2 repaints both handles after array `value` set + `dataBind()` | `ComponentProperty<double[]>.Mapped("rangeValue", "value")` + `self.EmitSet(property, ValueExpression.LiteralRaw(double[], Shape.ArrayOf(Shape.Number)))` then `EmitCall("dataBind")` | `SetReaction` targeting component property `value` (plan member `rangeValue`), then `CallReaction` for `dataBind` | runtime writes the number array onto `slider.value` and calls `slider.dataBind()` so both handles move | accepted and proven |
| `dataBind()` method | core trace `prototype methods` includes `dataBind` | `ComponentMethod.Named("dataBind")` + `self.EmitCall(method)` | `CallReaction` targeting component method `dataBind` | runtime invokes `slider.dataBind()` to flush the property set to the DOM; chained after the `value` set only | accepted as the repaint companion of the `value` writes |
| `change` event trigger | core trace `prototype methods` includes `change`/`changeEvent`; `slider.js` fires `changeEvent('change', event)` on a user gesture; `event-payload-surface.json` resolves the payload | `TypedEvent<FusionSliderChangeArgs>` selected by `.Reactive(e => e.Change, ...)` | `StartsWhen.ComponentEvent(componentId, "change")` | runtime wires the Syncfusion object `change` event and starts the reaction with event payload scope as the handle moves | accepted and proven |
| `changed` event trigger | core trace `prototype methods` includes `changed`; `slider.js` fires `changeEvent('changed', ...)` when the handle settles and from `setValue()` | `TypedEvent<FusionSliderChangeArgs>` selected by `.Reactive(e => e.Changed, ...)` | `StartsWhen.ComponentEvent(componentId, "changed")` | runtime wires the Syncfusion object `changed` event and starts the reaction with event payload scope when the handle settles | accepted and proven |
| `change/changed.value` | `event-payload-surface.json` `SliderChangeEventArgs.value: number`; `slider.js changeEventArgs` sets `value: this.value` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "value", Shape.Number)` from `FusionSliderChangeArgs.Value` | runtime reads `event.value` (the slider value after the change) into set text, condition, or gather | accepted and proven |
| `change/changed.previousValue` | `slider.js changeEventArgs` sets `previousValue` to `previousVal` for `change` and `previousChanged` for `changed` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "previousValue", Shape.Number)` from `FusionSliderChangeArgs.PreviousValue` | runtime reads `event.previousValue` (the value before the change) into visible text | accepted and proven |
| `change/changed.text` | `slider.js changeEventArgs` sets `text` to the formatted value string (`this.value.toString()` with no tick format) | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "text", Shape.Text)` from `FusionSliderChangeArgs.Text` | runtime reads `event.text` (the formatted slider value) into the live reading | accepted and proven |
| `change/changed.action` | `slider.js changeEventArgs` sets `action: eventName` (`"change"` / `"changed"`) | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "action", Shape.Text)` from `FusionSliderChangeArgs.Action` | runtime reads `event.action` (the Syncfusion change-action name) into visible text | accepted and proven |
| `change/changed.isInteracted` | `slider.js changeEventArgs` sets `isInteracted: isNullOrUndefined(e) ? false : true` — true for a user gesture, false for `setValue()` programmatic change | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "isInteracted", Shape.Boolean)` from `FusionSliderChangeArgs.IsInteracted` | runtime reads `event.isInteracted` to distinguish a value the resident chose (true) from one applied for them (false) | accepted and proven |
| `created`, `renderedTicks`, `renderingTicks`, `tooltipChange` events | `public-api-surface.json` lists each `builder.covered = true` event | builder-owned, not accepted for current rows | no public C# event selector | initial render hooks configured on `SliderBuilder`; no focused Senior Living runtime use case | excluded; builder-owned event hooks, no typed runtime use proven |
| `colorRange`, `cssClass`, `customValues`, `enableAnimation`, `enabled`, `enableHtmlSanitizer`, `limits`, `max`, `min`, `orientation`, `showButtons`, `step`, `ticks`, `tooltip`, `type`, `width` properties | `public-api-surface.json` marks each `builder.covered = true` | builder-owned static configuration | no runtime DSL member | initial render configured on `SliderBuilder`; no post-render read/write proven necessary | excluded; builder-owned per `references/automation-gates.md` Gate 5 builder-owned exclusion |
| `initialTooltip`, `readonly` properties | `discovery/parity-accounting.json` records each excluded with a source-grounded reason (`slider.d.ts:356`, `slider.d.ts:413`) | not accepted | no runtime DSL member | render-time tooltip/read-only configuration owned by the builder | excluded; see parity-accounting reasons |
| `reposition()`, `setTooltip()` methods | `discovery/parity-accounting.json` records each excluded (`slider.d.ts:653`, `slider.d.ts:713`) | not accepted | no runtime DSL member | imperative layout/tooltip-text helpers with no visible domain outcome a plan asserts | excluded; see parity-accounting reasons |
| `destroy()` method | `public-api-surface.json` classifies it `skip: lifecycle cleanup` | not a Fusion plan behavior | no runtime DSL member | runtime never calls it from a plan | excluded; lifecycle cleanup, not plan behavior |

## Primitive Decision

No new primitive is needed for the mapped Slider rows. Current primitives already
cover every onboarded member:

- component event trigger (`change`, `changed`);
- event payload read (`value`, `previousValue`, `text`, `action`, `isInteracted`);
- component property read (`value` scalar and `value` as a number array);
- component property write from a literal and from a raw number array (`value`), each followed by the `dataBind` repaint call.

The scalar and range writes/reads share the JS `value` member but use distinct
plan member names (`value` versus `rangeValue`) so the contract merge stays
deterministic for the overloaded vendor property. Any future failure to read one
of these accepted members is a discovery/mapping/typed-contract problem first,
never permission to add a primitive.

## Code To Delete Or Simplify

None identified for the primitive layer. The slice keeps `SetValue`/`SetRangeValue`
paired with `dataBind` rather than introducing a setter that silently repaints, so
the repaint is an explicit mapped row rather than hidden behavior.

## Behavior Proof Required Before Commit

The Slider rows are proven by `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Slider/WhenUsingFusionSlider.cs`
through `scripts/playwright.sh`, with each member tied to a fails-when-broken
assertion in `proof/behavioral-coverage.json`:

1. `FusionSlider(...)` render binds the model value (the page opens showing the carried-over temperature);
2. `change` fires the reaction and `value`/`text` drive the live reading and comfort-zone note as the handle moves;
3. `changed` fires on settle, and `previousValue`/`action` record what it changed from and how;
4. `isInteracted` distinguishes a value the resident chose from one applied for them;
5. `SetValue(value)` writes a given value back onto the slider and repaints the handle;
6. `SetRangeValue(start, end)` writes both range handles;
7. `Value()` yields the current value into a condition and a POST gather body;
8. `RangeValue()` yields the current window into the saved summary and a POST gather body.
