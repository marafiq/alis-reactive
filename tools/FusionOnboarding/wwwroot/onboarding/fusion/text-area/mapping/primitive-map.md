# TextArea Primitive Map

Status: active and proven. This file maps the onboarded `FusionTextArea` runtime
surface: the four typed events (`input`, `change`, `focus`, `blur`) and their
narrowed payloads, the typed `SetValue` write, the `FocusIn` and `FocusOut`
focus methods, and the `Value` read source. Every mapped row uses an existing
DSL primitive. The component is fully audited.

## Pass Rows

Close matrix row: `Html.InputField(m => m.CareNote).FusionTextArea(b => ...)` -> TextArea field render bound to a string model property -> sync input registration plus initial value bound by the Syncfusion builder.

Close matrix row: `textArea.Reactive(e => e.Input, (args, p) => ...)` input trigger -> TextArea `input` payload (`value`, `previousValue`) -> sync component-event reaction reading the typed payload while editing.

Close matrix row: `textArea.Reactive(e => e.Changed, (args, p) => ...)` change trigger -> TextArea `change` payload (`value`, `previousValue`, `isInteracted`) -> sync component-event reaction reading the committed-text payload after focus leaves.

Close matrix row: `textArea.Reactive(e => e.Focus, (args, p) => ...)` focus trigger -> TextArea `focus` payload (`value`) -> sync component-event reaction reading the value snapshot when focus arrives.

Close matrix row: `textArea.Reactive(e => e.Blur, (args, p) => ...)` blur trigger -> TextArea `blur` payload (`value`) -> sync component-event reaction reading the value snapshot when focus leaves.

Close matrix row: `p.Component<FusionTextArea>(id).SetValue(value)` -> typed TextArea value write -> sync component property set on `value` followed by a `dataBind` method call that repaints the textarea.

Close matrix row: `p.Component<FusionTextArea>(id).FocusIn()` -> typed TextArea focus method call -> sync `focusIn` method call that moves focus into the textarea.

Close matrix row: `p.Component<FusionTextArea>(id).FocusOut()` -> typed TextArea blur method call -> sync `focusOut` method call that removes focus from the textarea.

Close matrix row: `textArea.Value()` -> TextArea value read source -> sync component property read of `value` consumed by gather, conditions, or set text.

## DSL Source Requirements

- `Alis.Reactive.Fusion/Components/FusionTextArea/FusionTextArea.cs`
- `Alis.Reactive.Fusion/Components/FusionTextArea/FusionTextAreaHtmlExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionTextArea/FusionTextAreaExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionTextArea/FusionTextAreaEvents.cs`
- `Alis.Reactive.Fusion/Components/FusionTextArea/FusionTextAreaReactiveExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionTextArea/Events/FusionTextAreaOnInput.cs`
- `Alis.Reactive.Fusion/Components/FusionTextArea/Events/FusionTextAreaOnChanged.cs`
- `Alis.Reactive.Fusion/Components/FusionTextArea/Events/FusionTextAreaOnFocus.cs`
- `Alis.Reactive.Fusion/Components/FusionTextArea/Events/FusionTextAreaOnBlur.cs`
- `Alis.Reactive/Components/Contracts/ComponentRef.cs`
- `Alis.Reactive/Components/Contracts/ComponentProperty.cs`
- `Alis.Reactive/Components/Contracts/ComponentMember.cs`
- `Alis.Reactive/Components/Onboarding/ComponentEventOnboarding.cs`
- `Alis.Reactive/PlanModel/Values/ValueExpression.cs`

Sync/async lane expectation: the TextArea field render is sync input
registration; the `input`, `change`, `focus`, and `blur` component-event
triggers are sync; `SetValue`, `FocusIn`, `FocusOut`, and the `Value` read are
sync component actions. The TextArea slice introduces no async boundary. Async
only appears when a developer composes the read `Value()` source into an HTTP
`Post(...).Gather(...)` pipeline, which is the HTTP primitive, not a TextArea
concern.

## Authoritative Primitive Rows

| TextArea row member | Syncfusion evidence | DSL primitive | Domain/JSON term | Runtime behavior | Status |
| --- | --- | --- | --- | --- | --- |
| `value` property read | `traces/raw-ej2-core.trace.json` constructs `ej.inputs.TextArea({ value: "..." })`; `value` is a string on the instance and `discovery/public-api-surface.json` types it `string` | `ComponentProperty<string>.Named("value")` + `self.Read(property)` | `ValueExpression.Read(ComponentObject, "value", Shape.String)` from `FusionTextAreaExtensions.Value(...)` | runtime reads `textArea.value` into a typed string source | accepted and proven |
| `value` property write | core trace `prototype methods` includes `dataBind`; the EJ2 TextArea repaints after `value` set + `dataBind()` | `ComponentProperty<string>` + `self.EmitSet(property, ValueExpression.LiteralRaw(value, Shape.String))` then `EmitCall("dataBind")` | `SetReaction` targeting component property `value`, then `CallReaction` for `dataBind` | runtime writes `textArea.value = literal` and calls `textArea.dataBind()` so the visible text updates | accepted and proven |
| `focusIn()` method | `discovery/public-api-surface.json` lists `focusIn` as a runtime method (`) => void`) | `ComponentMethod.Named("focusIn")` + `self.EmitCall(method)` | `CallReaction` targeting component method `focusIn` | runtime invokes `textArea.focusIn()` and focus moves into the textarea | accepted and proven |
| `focusOut()` method | `discovery/public-api-surface.json` lists `focusOut` as a runtime method (`) => void`) | `ComponentMethod.Named("focusOut")` + `self.EmitCall(method)` | `CallReaction` targeting component method `focusOut` | runtime invokes `textArea.focusOut()` and focus leaves the textarea | accepted and proven |
| `dataBind()` method | core trace `prototype methods` includes `dataBind` | `ComponentMethod.Named("dataBind")` + `self.EmitCall(method)` | `CallReaction` targeting component method `dataBind` | runtime invokes `textArea.dataBind()` to flush the property set to the DOM; chained after the `value` set only | accepted as the repaint companion of the `value` write |
| `input` event trigger | core trace `prototype methods` includes the `input` handler wiring; `event-payload-surface.json` resolves `InputEventArgs` | `TypedEvent<FusionTextAreaInputArgs>` selected by `.Reactive(...)` | `StartsWhen.ComponentEvent(componentId, "input")` | runtime wires the Syncfusion `input` event and starts the reaction with event payload scope while editing | accepted and proven |
| `input.value` | `event-payload-surface.json` `InputEventArgs.value: string` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "value", Shape.String)` from `FusionTextAreaInputArgs.Value` | runtime reads `event.value` (the freshly typed text) into set text, condition, or gather | accepted and proven |
| `input.previousValue` | `event-payload-surface.json` `InputEventArgs.previousValue: string` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "previousValue", Shape.String)` from `FusionTextAreaInputArgs.PreviousValue` | runtime reads `event.previousValue` (the text before the keystroke) into visible text | accepted and proven |
| `change` event trigger | core trace `prototype methods` includes `change` and `changeHandler`; `event-payload-surface.json` resolves `ChangedEventArgs` | `TypedEvent<FusionTextAreaChangeArgs>` selected by `.Reactive(...)` | `StartsWhen.ComponentEvent(componentId, "change")` | runtime wires the Syncfusion `change` event and starts the reaction with event payload scope after focus leaves | accepted and proven |
| `change.value` | `event-payload-surface.json` `ChangedEventArgs.value: string` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "value", Shape.String)` from `FusionTextAreaChangeArgs.Value` | runtime reads `event.value` (the committed text) into visible text or gather | accepted and proven |
| `change.previousValue` | `event-payload-surface.json` `ChangedEventArgs.previousValue: string` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "previousValue", Shape.String)` from `FusionTextAreaChangeArgs.PreviousValue` | runtime reads `event.previousValue` (the prior committed text) into visible text | accepted and proven |
| `change.isInteracted` | `event-payload-surface.json` `ChangedEventArgs.isInteracted: boolean` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "isInteracted", Shape.Boolean)` from `FusionTextAreaChangeArgs.IsInteracted` | runtime reads `event.isInteracted` to distinguish a hand edit (true) from a programmatic `SetValue` (false) | accepted and proven |
| `focus` event trigger | core trace `prototype methods` includes the focus wiring; `event-payload-surface.json` resolves `FocusInEventArgs` | `TypedEvent<FusionTextAreaFocusArgs>` selected by `.Reactive(...)` | `StartsWhen.ComponentEvent(componentId, "focus")` | runtime wires the Syncfusion `focus` event and starts the reaction with event payload scope when focus arrives | accepted and proven |
| `focus.value` | `event-payload-surface.json` `FocusInEventArgs.value: string` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "value", Shape.String)` from `FusionTextAreaFocusArgs.Value` | runtime reads `event.value` (the text on file when focus arrives) into visible text | accepted and proven |
| `blur` event trigger | core trace `prototype methods` includes `blur`; `event-payload-surface.json` resolves `FocusOutEventArgs` | `TypedEvent<FusionTextAreaBlurArgs>` selected by `.Reactive(...)` | `StartsWhen.ComponentEvent(componentId, "blur")` | runtime wires the Syncfusion `blur` event and starts the reaction with event payload scope when focus leaves | accepted and proven |
| `blur.value` | `event-payload-surface.json` `FocusOutEventArgs.value: string` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "value", Shape.String)` from `FusionTextAreaBlurArgs.Value` | runtime reads `event.value` (the text held in the field when focus leaves) into visible text | accepted and proven |
| `input.container`, `change.container`, `focus.container`, `blur.container` | `event-payload-surface.json` each carries `container: HTMLElement` | excluded browser-owned DOM element | no public C# payload property | runtime must not serialize or expose this through broad typed event args | excluded; browser-owned DOM element, see `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them` |
| `input.event`, `change.event`, `focus.event`, `blur.event` | `event-payload-surface.json` each carries `event: Event` | excluded browser-owned DOM event object | no public C# payload property | runtime must not serialize or expose this through broad typed event args | excluded; browser-owned DOM event, see `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them` |
| `change.isInteraction` | `event-payload-surface.json` `ChangedEventArgs.isInteraction: boolean` | excluded deprecated duplicate of `isInteracted` | no public C# payload property | no runtime mapping for this row | excluded; deprecated misspelled alias superseded by `isInteracted` |
| `created`, `destroyed` events | `discovery/public-api-surface.json` types each payload `Object`; core trace shows `created` fires with an undefined DOM-native payload | not accepted for the current rows | no public C# event selector | no runtime mapping for this row | excluded; lifecycle-only, no typed payload (`event-payload-surface.json` marks each `builtin-object`) |
| `addAttributes`, `removeAttributes` methods | `discovery/public-api-surface.json` marks each a runtime method candidate requiring visible-effect proof | not accepted for the current rows | no runtime DSL member | runtime never calls these from a plan | excluded; raw attribute manipulation, no focused Senior Living use case and the DSL does not express attribute bags |
| `readonly` property | `discovery/public-api-surface.json` marks it a runtime property candidate requiring proof | not accepted for the current rows | no runtime DSL member | no post-render read/write proven necessary | excluded; no focused Senior Living use case for toggling read-only post-render |
| `adornmentFlow`, `adornmentOrientation`, `appendTemplate`, `cols`, `cssClass`, `enabled`, `enablePersistence`, `floatLabelType`, `maxLength`, `placeholder`, `prependTemplate`, `resizeMode`, `rows`, `showClearButton`, `width` | `public-api-surface.json` marks each `builder.covered = true` | builder-owned static configuration | no runtime DSL member | initial render configured on `TextAreaBuilder`; no post-render read/write proven necessary | excluded; builder-owned per `references/automation-gates.md` Gate 5 builder-owned exclusion |
| `destroy()` method | `public-api-surface.json` classifies it `skip: lifecycle cleanup` | not a Fusion plan behavior | no runtime DSL member | runtime never calls it from a plan | excluded; lifecycle cleanup, not plan behavior |

## Primitive Decision

No new primitive is needed for the mapped TextArea rows. Current primitives
already cover every onboarded member:

- component event trigger (`input`, `change`, `focus`, `blur`);
- event payload read (`value`, `previousValue`, `isInteracted`);
- component property read (`value`);
- component property write from a literal (`value`) followed by the `dataBind` repaint call;
- component method call (`focusIn`, `focusOut`).

Any future failure to read one of these accepted members is a
discovery/mapping/typed-contract problem first, never permission to add a
primitive.

## Code To Delete Or Simplify

None identified for the primitive layer. The slice keeps `SetValue` paired with
`dataBind` rather than introducing a setter that silently repaints, so the
repaint is an explicit mapped row rather than hidden behavior.

## Behavior Proof Required Before Commit

The TextArea rows are proven by `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/TextArea/WhenUsingFusionTextArea.cs`
through `scripts/playwright.sh`, with each member tied to a fails-when-broken
assertion in `proof/behavioral-coverage.json`:

1. `FusionTextArea(...)` render binds the model value (the care log opens showing last shift's note already in the field);
2. `input` fires the reaction and `value`/`previousValue` show the live preview and the prior text while editing;
3. `change` fires after focus leaves and `value`/`previousValue`/`isInteracted` record the committed note, what it replaced, and whether a person edited it;
4. `focus` and `blur` record the value snapshot when focus arrives and when it leaves;
5. `SetValue(value)` writes a given value back onto the textarea (restoring last shift's note);
6. `FocusIn()` moves focus into the textarea and `FocusOut()` removes it;
7. `Value()` yields the current note into a POST gather body.
