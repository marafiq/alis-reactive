# TextBox Primitive Map

Status: active and proven. This file maps the onboarded `FusionTextBox` runtime
surface: the `input`, `change`, `focus`, and `blur` events with their typed
payloads, the typed `SetValue` write, the `focusIn`/`focusOut` method calls, the
`addIcon` append-icon call, the `value` read source, and the `FusionTextBox(...)`
field render helper. Every mapped row uses an existing DSL primitive. The
component is fully audited; no DSL primitive was added, removed, or broadened.

## Pass Rows

Close matrix row: `Html.InputField(m => m.PreferredName).FusionTextBox(b => ...)` -> TextBox field render bound to a string model property -> sync input registration plus initial value/placeholder/clear-button bound by the Syncfusion builder.

Close matrix row: `tb.Reactive(e => e.Input, (args, p) => ...)` input trigger -> TextBox `input` payload (`value`, `previousValue`) -> sync component-event reaction reading the typed payload while editing.

Close matrix row: `tb.Reactive(e => e.Changed, (args, p) => ...)` change trigger -> TextBox `change` payload (`value`, `previousValue`, `isInteracted`) -> sync component-event reaction firing when the committed text changes on blur.

Close matrix row: `tb.Reactive(e => e.Focus, (args, p) => ...)` focus trigger -> TextBox `focus` payload (`value`) -> sync component-event reaction reading the value present at focus.

Close matrix row: `tb.Reactive(e => e.Blur, (args, p) => ...)` blur trigger -> TextBox `blur` payload (`value`) -> sync component-event reaction reading the value present at blur.

Close matrix row: `p.Component<FusionTextBox>(id).SetValue(value)` -> typed TextBox value write -> sync component property set on `value` followed by a `dataBind` method call that repaints the input.

Close matrix row: `p.Component<FusionTextBox>(id).FocusIn()` -> typed TextBox focusIn method call -> sync `focusIn` method call that moves focus into the input.

Close matrix row: `p.Component<FusionTextBox>(id).FocusOut()` -> typed TextBox focusOut method call -> sync `focusOut` method call that removes focus from the input.

Close matrix row: `p.Component<FusionTextBox>(id).AddAppendIcon(css)` -> typed TextBox append-icon call -> sync `addIcon` method call with the fixed `"append"` position literal and the developer's icon CSS classes.

Close matrix row: `tb.Value()` -> TextBox value read source -> sync component property read of `value` consumed by gather, conditions, or set text.

## DSL Source Requirements

- `Alis.Reactive.Fusion/Components/FusionTextBox/FusionTextBox.cs`
- `Alis.Reactive.Fusion/Components/FusionTextBox/FusionTextBoxHtmlExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionTextBox/FusionTextBoxExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionTextBox/FusionTextBoxEvents.cs`
- `Alis.Reactive.Fusion/Components/FusionTextBox/FusionTextBoxReactiveExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionTextBox/Events/FusionTextBoxOnInput.cs`
- `Alis.Reactive.Fusion/Components/FusionTextBox/Events/FusionTextBoxOnChanged.cs`
- `Alis.Reactive.Fusion/Components/FusionTextBox/Events/FusionTextBoxOnFocus.cs`
- `Alis.Reactive.Fusion/Components/FusionTextBox/Events/FusionTextBoxOnBlur.cs`
- `Alis.Reactive/Components/Contracts/ComponentRef.cs`
- `Alis.Reactive/Components/Contracts/ComponentProperty.cs`
- `Alis.Reactive/Components/Contracts/ComponentMember.cs`
- `Alis.Reactive/Components/Onboarding/ComponentEventOnboarding.cs`
- `Alis.Reactive/PlanModel/Values/ValueExpression.cs`

Sync/async lane expectation: the textbox field render is sync input
registration; the `input`/`change`/`focus`/`blur` component-event triggers are
sync; `SetValue`, `FocusIn`, `FocusOut`, `AddAppendIcon`, and the `Value` read
are sync component actions. The TextBox slice introduces no async boundary.
Async only appears when a developer composes the read `Value()` source into an
HTTP `Post(...).Gather(...)` pipeline, which is the HTTP primitive, not a
TextBox concern.

## Authoritative Primitive Rows

| TextBox row member | Syncfusion evidence | DSL primitive | Domain/JSON term | Runtime behavior | Status |
| --- | --- | --- | --- | --- | --- |
| `value` property read | `traces/raw-ej2-core.trace.json` constructs `ej.inputs.TextBox`; `value: string` on the instance (`textbox.d.ts:81`) | `ComponentProperty<string>.Named("value")` + `self.Read(property)` | `ValueExpression.Read(ComponentObject, "value", Shape.String)` from `FusionTextBoxExtensions.Value(...)` | runtime reads `tb.value` into a typed string source | accepted and proven |
| `value` property write | core trace `prototype methods` includes `dataBind`-companion repaint pattern; `value` set repaints the input | `ComponentProperty<string>` + `self.EmitSet(property, ValueExpression.LiteralRaw(value, Shape.String))` then `EmitCall("dataBind")` | `SetReaction` targeting component property `value`, then `CallReaction` for `dataBind` | runtime writes `tb.value = literal` and calls `tb.dataBind()` so the visible input updates | accepted and proven |
| `focusIn()` method | core trace `prototype methods` includes `focusIn` (`textbox.d.ts:314`) | `ComponentMethod.Named("focusIn")` + `self.EmitCall(method)` | `CallReaction` targeting component method `focusIn` | runtime invokes `tb.focusIn()` and the input gains focus | accepted and proven |
| `focusOut()` method | core trace `prototype methods` includes `focusOut` (`textbox.d.ts:320`) | `ComponentMethod.Named("focusOut")` + `self.EmitCall(method)` | `CallReaction` targeting component method `focusOut` | runtime invokes `tb.focusOut()` and the input loses focus | accepted and proven |
| `addIcon(position, icons)` method | core trace `prototype methods` includes `addIcon` (`textbox.d.ts:286`); signature `(position: string, icons: string | string[])` | `ComponentMethod.Mapped("addAppendIcon","addIcon").WithArgs<string,string>()` + `self.EmitCall(method, [Literal("append"), Literal(css)])` | `CallReaction` targeting component method `addIcon` with two literal args | runtime invokes `tb.addIcon("append", css)` and the append icon renders inside the input group | accepted and proven |
| `dataBind()` method | core trace `prototype methods` includes the EJ2 base `dataBind` flush | `ComponentMethod.Named("dataBind")` + `self.EmitCall(method)` | `CallReaction` targeting component method `dataBind` | runtime invokes `tb.dataBind()` to flush the `value` set to the DOM; chained after the `value` set only | accepted as the repaint companion of the `value` write |
| `input` event trigger | core trace `prototype methods` includes `input` handler wiring; `input: EmitType<InputEventArgs>` (`textbox.d.ts`) | `TypedEvent<FusionTextBoxInputArgs>` selected by `.Reactive(...)` | `StartsWhen.ComponentEvent(componentId, "input")` | runtime wires the Syncfusion object `input` event and starts the reaction with event payload scope | accepted and proven |
| `input.value` | `event-payload-surface.json` `InputEventArgs.value: string` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "value", Shape.String)` from `FusionTextBoxInputArgs.Value` | runtime reads `event.value` (the text as typed) into set text or condition | accepted and proven |
| `input.previousValue` | `own keys` includes `inputPreviousValue`; `InputEventArgs.previousValue: string` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "previousValue", Shape.String)` from `FusionTextBoxInputArgs.PreviousValue` | runtime reads `event.previousValue` (text before this input batch) into visible text | accepted and proven |
| `change` event trigger | core trace `prototype methods` includes `change`/`changeHandler`; `change: EmitType<ChangedEventArgs>` | `TypedEvent<FusionTextBoxChangeArgs>` selected by `.Reactive(...)` | `StartsWhen.ComponentEvent(componentId, "change")` | runtime wires the `change` event, fired when committed text changes on blur | accepted and proven |
| `change.value` | `event-payload-surface.json` `ChangedEventArgs.value: string` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "value", Shape.String)` from `FusionTextBoxChangeArgs.Value` | runtime reads the committed text into the saved-name record | accepted and proven |
| `change.previousValue` | `event-payload-surface.json` `ChangedEventArgs.previousValue: string` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "previousValue", Shape.String)` from `FusionTextBoxChangeArgs.PreviousValue` | runtime reads the value before the commit into the changed-from line | accepted and proven |
| `change.isInteracted` | `event-payload-surface.json` `ChangedEventArgs.isInteracted: boolean` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "isInteracted", Shape.Boolean)` from `FusionTextBoxChangeArgs.IsInteracted` | runtime reads `event.isInteracted` to distinguish a hand-typed edit (true) from a programmatic SetValue (false) | accepted and proven |
| `focus` event trigger | core trace `prototype methods` includes focus wiring; `focus: EmitType<FocusInEventArgs>` | `TypedEvent<FusionTextBoxFocusArgs>` selected by `.Reactive(...)` | `StartsWhen.ComponentEvent(componentId, "focus")` | runtime wires the `focus` event, fired when the input receives focus | accepted and proven |
| `focus.value` | `event-payload-surface.json` `FocusInEventArgs.value: string` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "value", Shape.String)` from `FusionTextBoxFocusArgs.Value` | runtime reads the value present at focus into the on-file note line | accepted and proven |
| `blur` event trigger | core trace `prototype methods` includes `blur`; `blur: EmitType<FocusOutEventArgs>` | `TypedEvent<FusionTextBoxBlurArgs>` selected by `.Reactive(...)` | `StartsWhen.ComponentEvent(componentId, "blur")` | runtime wires the `blur` event, fired when the input loses focus | accepted and proven |
| `blur.value` | `event-payload-surface.json` `FocusOutEventArgs.value: string` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "value", Shape.String)` from `FusionTextBoxBlurArgs.Value` | runtime reads the value present at blur into the captured-note line | accepted and proven |
| `addAttributes({[k]:string})` method | core trace `prototype methods` includes `addAttributes`; signature takes an arbitrary string->string dictionary (`textbox.d.ts:299`) | none — arbitrary attribute dictionary is a stringly surface | no public C# member | runtime never calls it from the typed slice | excluded; stringly attribute map, builder owns initial `HtmlAttributes`, see `discovery/parity-accounting.json` |
| `removeAttributes(string[])` method | core trace `prototype methods` includes `removeAttributes`; signature takes an arbitrary array of attribute-name strings (`textbox.d.ts:308`) | none — arbitrary attribute-name array is a stringly surface | no public C# member | runtime never calls it from the typed slice | excluded; stringly attribute-name list, plugin-boundary concern, see `discovery/parity-accounting.json` |
| `readonly` property | `textbox.d.ts:75` `readonly: boolean`, controls whether the user may change the text | builder-owned at render; runtime toggle not onboarded | no public C# member | initial read-only configured on `TextBoxBuilder`; runtime disable served by builder-owned `enabled` | excluded; builder-owned initial render, deferred runtime-toggle candidate, see `discovery/parity-accounting.json` |
| `created`/`destroyed` events, `destroy()` method | `public-api-surface.json` classifies `destroy` as `skip: lifecycle cleanup`; `created`/`destroyed` carry no typed payload | not a Fusion plan behavior | no public C# member | runtime never wires them from a plan | excluded; lifecycle-only, not plan behavior |
| `appendTemplate`, `autocomplete`, `cssClass`, `enabled`, `enablePersistence`, `floatLabelType`, `multiline`, `placeholder`, `prependTemplate`, `showClearButton`, `type`, `value` (initial), `width` | `public-api-surface.json` marks each `builder.covered = true` | builder-owned static configuration | no runtime DSL member beyond the proven `value` read/write | initial render configured on `TextBoxBuilder`; no further post-render read/write proven necessary | excluded; builder-owned per `references/automation-gates.md` Gate 5 builder-owned exclusion |

## Primitive Decision

No new primitive is needed for the mapped TextBox rows. Current primitives
already cover every onboarded member:

- component event trigger (`input`, `change`, `focus`, `blur`);
- event payload read (`value`, `previousValue`, `isInteracted`);
- component property read (`value`);
- component property write from a literal (`value`) followed by the `dataBind` repaint call;
- component method call (`focusIn`, `focusOut`, `addIcon`).

Any future failure to read one of these accepted members is a
discovery/mapping/typed-contract problem first, never permission to add a
primitive.

## Code To Delete Or Simplify

None identified for the primitive layer. The slice keeps `SetValue` paired with
`dataBind` rather than introducing a setter that silently repaints, so the
repaint is an explicit mapped row rather than hidden behavior. `AddAppendIcon`
fixes the `"append"` position literal so the public API exposes a single typed
intent rather than the raw `position` string.

## Behavior Proof Required Before Commit

The TextBox rows are proven by
`tests/Alis.Reactive.PlaywrightTests/Components/Fusion/TextBox/WhenUsingFusionTextBox.cs`
through `scripts/playwright.sh`, with each member tied to a fails-when-broken
assertion in `proof/behavioral-coverage.json`:

1. `FusionTextBox(...)` render binds the model value and `AddAppendIcon` adds the search affordance (the profile opens showing the name on file with the search icon);
2. `input` fires the reaction and `value`/`previousValue` drive the live preview and the replacing-name line;
3. `change` records the committed `value`, the `previousValue`, and routes on `isInteracted`;
4. `isInteracted=false` distinguishes a programmatic `SetValue` from a hand-typed edit;
5. `FocusIn()` focuses the field and `FocusOut()` removes focus;
6. `focus`/`blur` read the `value` present at focus and at blur;
7. `Value()` yields the current text into a POST gather body.
