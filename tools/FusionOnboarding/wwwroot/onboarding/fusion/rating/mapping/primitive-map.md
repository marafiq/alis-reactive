# Rating Primitive Map

Status: active and proven. This file maps the onboarded `FusionRating` runtime
surface: the `valueChanged` event and its typed payload (`Value`,
`PreviousValue`, `IsInteracted`), the typed `SetValue` write, the `Reset` method,
the `Value` read source, and the `FusionRating(...)` field render helper. Every
mapped row uses an existing DSL primitive. The component is fully audited.

## Pass Rows

Close matrix row: `Html.InputField(m => m.Score).FusionRating(b => ...)` -> Rating field render bound to a numeric model property -> sync input registration plus initial value bound by the Syncfusion builder.

Close matrix row: `rating.Reactive(e => e.ValueChanged, (args, p) => ...)` value-changed trigger -> Rating `valueChanged` payload (`value`, `previousValue`, `isInteracted`) -> sync component-event reaction reading the typed payload.

Close matrix row: `p.Component<FusionRating>(id).SetValue(value)` -> typed Rating value write -> sync component property set on `value` followed by a `dataBind` method call that repaints the stars.

Close matrix row: `p.Component<FusionRating>(id).Reset()` -> typed Rating reset method call -> sync `reset` method call that returns the rating to its minimum.

Close matrix row: `rating.Value()` -> Rating value read source -> sync component property read of `value` consumed by gather, conditions, or set text.

## DSL Source Requirements

- `Alis.Reactive.Fusion/Components/FusionRating/FusionRating.cs`
- `Alis.Reactive.Fusion/Components/FusionRating/FusionRatingHtmlExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionRating/FusionRatingExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionRating/FusionRatingEvents.cs`
- `Alis.Reactive.Fusion/Components/FusionRating/FusionRatingReactiveExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionRating/Events/FusionRatingOnValueChanged.cs`
- `Alis.Reactive/Components/Contracts/ComponentRef.cs`
- `Alis.Reactive/Components/Contracts/ComponentProperty.cs`
- `Alis.Reactive/Components/Contracts/ComponentMember.cs`
- `Alis.Reactive/Components/Onboarding/ComponentEventOnboarding.cs`
- `Alis.Reactive/PlanModel/Values/ValueExpression.cs`

Sync/async lane expectation: the rating field render is sync input registration;
the `valueChanged` component-event trigger is sync; `SetValue`, `Reset`, and the
`Value` read are sync component actions. The Rating slice introduces no async
boundary. Async only appears when a developer composes the read `Value()` source
into an HTTP `Post(...).Gather(...)` pipeline, which is the HTTP primitive, not a
Rating concern.

## Authoritative Primitive Rows

| Rating row member | Syncfusion evidence | DSL primitive | Domain/JSON term | Runtime behavior | Status |
| --- | --- | --- | --- | --- | --- |
| `value` property read | `traces/raw-ej2-core.trace.json` constructs `Rating({ value: 3 })`; `value` is a number on the instance | `ComponentProperty<double>.Named("value")` + `self.Read(property)` | `ValueExpression.Read(ComponentObject, "value", Shape.Number)` from `FusionRatingExtensions.Value(...)` | runtime reads `rating.value` into a typed double source | accepted and proven |
| `value` property write | core trace `prototype methods` includes `dataBind`; raw EJ2 repaints stars after `value` set + `dataBind()` | `ComponentProperty<double>` + `self.EmitSet(property, ValueExpression.Literal(value))` then `EmitCall("dataBind")` | `SetReaction` targeting component property `value`, then `CallReaction` for `dataBind` | runtime writes `rating.value = literal` and calls `rating.dataBind()` so the visible stars update | accepted and proven |
| `reset()` method | core trace `prototype methods` includes `reset`; raw EJ2 `reset()` returns the value to its minimum | `ComponentMethod.Named("reset")` + `self.EmitCall(method)` | `CallReaction` targeting component method `reset` | runtime invokes `rating.reset()` and the visible rating clears to 0 | accepted and proven |
| `dataBind()` method | core trace `prototype methods` includes `dataBind` | `ComponentMethod.Named("dataBind")` + `self.EmitCall(method)` | `CallReaction` targeting component method `dataBind` | runtime invokes `rating.dataBind()` to flush the property set to the DOM; chained after the `value` set only | accepted as the repaint companion of the `value` write |
| `valueChanged` event trigger | core trace candidate row `valueChanged: RatingChangedEventArgs`; `event-payload-surface.json` resolves the payload | `TypedEvent<FusionRatingValueChangedArgs>` selected by `.Reactive(...)` | `StartsWhen.ComponentEvent(componentId, "valueChanged")` | runtime wires the Syncfusion object event and starts the reaction with event payload scope | accepted and proven |
| `valueChanged.value` | `event-payload-surface.json` `RatingChangedEventArgs.value: number` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "value", Shape.Number)` from `FusionRatingValueChangedArgs.Value` | runtime reads `event.value` (the newly selected rating) into set text, condition, or gather | accepted and proven |
| `valueChanged.previousValue` | `event-payload-surface.json` `RatingChangedEventArgs.previousValue: number` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "previousValue", Shape.Number)` from `FusionRatingValueChangedArgs.PreviousValue` | runtime reads `event.previousValue` (the value before the change) into visible text | accepted and proven |
| `valueChanged.isInteracted` | `event-payload-surface.json` `RatingChangedEventArgs.isInteracted: boolean` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "isInteracted", Shape.Boolean)` from `FusionRatingValueChangedArgs.IsInteracted` | runtime reads `event.isInteracted` to distinguish a user choice (true) from a programmatic change (false) | accepted and proven |
| `valueChanged.event` | `event-payload-surface.json` `RatingChangedEventArgs.event: Event` | excluded browser-owned event object | no public C# payload property | runtime must not serialize or expose this through broad typed event args | excluded; browser-owned DOM event, see `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them` |
| `valueChanged.name` | `event-payload-surface.json` inherits `BaseEventArgs.name: string` | excluded duplicate event metadata | no public C# payload property | no runtime mapping for this row | excluded; the event selector already owns event identity |
| `beforeItemRender` event | core trace rows 1-5 emit `RatingItemEventArgs` with `element`/`name`/`value` per star | not accepted for the current rows | no public C# event selector | no runtime mapping for this row | excluded; per-item render hook carries a browser-owned `element` and a separate item-template row would be required before any typed use |
| `onItemHover` event | `event-payload-surface.json` `RatingHoverEventArgs` with `element`/`event`/`name`/`value` | not accepted for the current rows | no public C# event selector | no runtime mapping for this row | excluded; hover carries browser-owned `element`/`event` and no focused Senior Living use case |
| `created` event | core trace row 6 fires `created` with an undefined DOM-native payload | not accepted for the current rows | no public C# event selector | no runtime mapping for this row | excluded; lifecycle-only, no typed payload (`event-payload-surface.json` marks it dom-native) |
| `allowReset`, `cssClass`, `disabled`, `enableAnimation`, `enableSingleSelection`, `itemsCount`, `labelPosition`, `min`, `precision`, `readOnly`, `showLabel`, `showTooltip`, `visible`, and the four templates | `public-api-surface.json` marks each `builder.covered = true` | builder-owned static configuration | no runtime DSL member | initial render configured on `RatingBuilder`; no post-render read/write proven necessary | excluded; builder-owned per `references/automation-gates.md` Gate 5 builder-owned exclusion |
| `destroy()` method | `public-api-surface.json` classifies it `skip: lifecycle cleanup` | not a Fusion plan behavior | no runtime DSL member | runtime never calls it from a plan | excluded; lifecycle cleanup, not plan behavior |

## Primitive Decision

No new primitive is needed for the mapped Rating rows. Current primitives already
cover every onboarded member:

- component event trigger (`valueChanged`);
- event payload read (`value`, `previousValue`, `isInteracted`);
- component property read (`value`);
- component property write from a literal (`value`) followed by the `dataBind` repaint call;
- component method call (`reset`).

Any future failure to read one of these accepted members is a
discovery/mapping/typed-contract problem first, never permission to add a
primitive.

## Code To Delete Or Simplify

None identified for the primitive layer. The slice keeps `SetValue` paired with
`dataBind` rather than introducing a setter that silently repaints, so the
repaint is an explicit mapped row rather than hidden behavior.

## Behavior Proof Required Before Commit

The Rating rows are proven by `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Rating/WhenUsingFusionRating.cs`
through `scripts/playwright.sh`, with each member tied to a fails-when-broken
assertion in `proof/behavioral-coverage.json`:

1. `FusionRating(...)` render binds the model value (the survey opens showing the carried-over rating);
2. `valueChanged` fires the reaction and `value` shows the newly selected score with its message;
3. `previousValue` records what the rating changed from;
4. `isInteracted` distinguishes a user choice from a programmatic clear;
5. `Reset()` clears the visible rating and score to 0;
6. `SetValue(value)` writes a given value back onto the rating;
7. `Value()` yields the current rating into a POST gather body.
