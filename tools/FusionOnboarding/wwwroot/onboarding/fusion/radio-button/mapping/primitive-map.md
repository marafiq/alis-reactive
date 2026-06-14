# RadioButton Primitive Map

Status: active and proven. This file maps the onboarded `FusionRadioButton`
runtime surface: the `change` event and its typed payload (`Value`), the
`checked` and `disabled` property reads and writes, the `getSelectedValue`
group read, the `click` and `focusIn` method calls, and the
`FusionRadioButton(...)` element render helper. Every mapped row uses an
existing DSL primitive. The component is fully audited.

## Pass Rows

Close matrix row: `Html.FusionRadioButton(plan, "room-companion", b => b.Label("Shared Companion Suite").Name("room").Value("Shared Companion Suite"))` -> RadioButton element render with builder-owned options -> sync element registration plus initial options bound by the Syncfusion `RadioButtonBuilder`.

Close matrix row: `radio.Reactive(e => e.Changed, (args, p) => ...)` change trigger -> RadioButton `change` payload (`value`) -> sync component-event reaction reading the typed payload value.

Close matrix row: `p.Component<FusionRadioButton>(id).SetChecked(true)` -> typed RadioButton checked write -> sync component property set on `checked` followed by a `dataBind` method call that repaints the radio.

Close matrix row: `p.Component<FusionRadioButton>(id).SetDisabled(true)` -> typed RadioButton disabled write -> sync component property set on `disabled` followed by a `dataBind` method call that repaints the radio.

Close matrix row: `radio.Checked()` -> RadioButton checked read source -> sync component property read of `checked` consumed by gather, conditions, or set text.

Close matrix row: `radio.Disabled()` -> RadioButton disabled read source -> sync component property read of `disabled` consumed by conditions or set text.

Close matrix row: `radio.SelectedValue()` -> RadioButton group selection read source -> sync component method call of `getSelectedValue` whose string return is consumed by gather, conditions, or set text.

Close matrix row: `p.Component<FusionRadioButton>(id).Click()` -> typed RadioButton click method call -> sync `click` method call that selects the radio.

Close matrix row: `p.Component<FusionRadioButton>(id).FocusIn()` -> typed RadioButton focus method call -> sync `focusIn` method call that moves keyboard focus into the radio.

## DSL Source Requirements

- `Alis.Reactive.Fusion/Components/FusionRadioButton/FusionRadioButton.cs`
- `Alis.Reactive.Fusion/Components/FusionRadioButton/FusionRadioButtonHtmlExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionRadioButton/FusionRadioButtonExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionRadioButton/FusionRadioButtonEvents.cs`
- `Alis.Reactive.Fusion/Components/FusionRadioButton/FusionRadioButtonReactiveExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionRadioButton/Events/FusionRadioButtonOnChanged.cs`
- `Alis.Reactive/Components/Contracts/ComponentRef.cs`
- `Alis.Reactive/Components/Contracts/ComponentProperty.cs`
- `Alis.Reactive/Components/Contracts/ComponentMember.cs`
- `Alis.Reactive/Components/Onboarding/ComponentEventOnboarding.cs`
- `Alis.Reactive/PlanModel/Values/ValueExpression.cs`

Sync/async lane expectation: the RadioButton element render is sync element
registration; the `change` component-event trigger is sync; `SetChecked`,
`SetDisabled`, the `Checked`, `Disabled`, and `SelectedValue` reads, and the
`Click` and `FocusIn` calls are sync component actions. The RadioButton slice
introduces no async boundary. Async only appears when a developer composes a
read source such as `Checked()` or `SelectedValue()` into an HTTP
`Post(...).Gather(...)` pipeline, which is the HTTP primitive, not a RadioButton
concern.

## Authoritative Primitive Rows

| RadioButton row member | Syncfusion evidence | DSL primitive | Domain/JSON term | Runtime behavior | Status |
| --- | --- | --- | --- | --- | --- |
| `change` event trigger | `traces/raw-ej2-core.trace.json` row 7 fires `change` with `ChangeArgs`; `event-payload-surface.json` resolves the payload | `TypedEvent<FusionRadioButtonChangeArgs>` named `change`, selected by `.Reactive(...)` | `StartsWhen.ComponentEvent(componentId, "change")` | runtime wires the Syncfusion object `change` event and starts the reaction with event payload scope | accepted and proven |
| `change.value` | core trace row 7 `change.value: "Shared Companion Suite"` (string); `event-payload-surface.json` `ChangeArgs.value: string` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "value", Shape.String)` from `FusionRadioButtonChangeArgs.Value` | runtime reads `event.value` (the selected radio value) into set text, condition, or gather | accepted and proven |
| `change.event` | `event-payload-surface.json` `ChangeArgs.event: Event`; core trace row 7 `event.sample { isTrusted: true }` | excluded browser-owned event object | no public C# payload property | runtime must not serialize or expose this through broad typed event args | excluded; browser-owned DOM event, see `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them` |
| `change.name` | `event-payload-surface.json` inherits `BaseEventArgs.name: string`; core trace row 7 `name.sample "change"` | excluded duplicate event metadata | no public C# payload property | no runtime mapping for this row | excluded; the `Changed` selector already owns event identity |
| `checked` property read | core trace row 3 `checked [studio, initial] = true`, row 4 `checked [companion, initial] = false`, row 10 `checked [companion, after click] = true` | `ComponentProperty<bool>.Named("checked")` + `self.Read(property)` | `ValueExpression.Read(ComponentObject, "checked", Shape.Boolean)` from `FusionRadioButtonExtensions.Checked(...)` | runtime reads `radio.checked` into a typed boolean source | accepted and proven |
| `checked` property write | `SetChecked` set + repaint; core trace `prototype methods` includes `dataBind`; selection flips the visible radio | `ComponentProperty<bool>` + `self.EmitSet(property, ValueExpression.Literal(isChecked))` then `EmitCall("dataBind")` | `SetReaction` targeting component property `checked`, then `CallReaction` for `dataBind` | runtime writes `radio.checked = literal` and calls `radio.dataBind()` so the radio repaints | accepted and proven |
| `disabled` property read | core trace row 5 `disabled [companion, initial] = false`, row 11 `disabled [companion, after set + dataBind] = true` | `ComponentProperty<bool>.Named("disabled")` + `self.Read(property)` | `ValueExpression.Read(ComponentObject, "disabled", Shape.Boolean)` from `FusionRadioButtonExtensions.Disabled(...)` | runtime reads `radio.disabled` into a typed boolean source | accepted and proven |
| `disabled` property write | core trace rows 5 and 11 show `disabled` flips from `false` to `true` after the property set plus `dataBind()` | `ComponentProperty<bool>` + `self.EmitSet(property, ValueExpression.Literal(disabled))` then `EmitCall("dataBind")` | `SetReaction` targeting component property `disabled`, then `CallReaction` for `dataBind` | runtime writes `radio.disabled = literal` and calls `radio.dataBind()` so the radio repaints disabled | accepted and proven |
| `getSelectedValue()` method | core trace row 9 `getSelectedValue [after companion click] = "Shared Companion Suite"`; `prototype methods` includes the method | `ComponentMethod.Named("getSelectedValue")` + `self.Read<string>(method)` | `ValueExpression.Read(ComponentObject, "getSelectedValue", Shape.String)` (typed method-return source) from `FusionRadioButtonExtensions.SelectedValue(...)` | runtime calls `radio.getSelectedValue()` and yields the group's selected value as a typed string source | accepted and proven |
| `click()` method | core trace row 8 `click [companion]` invoked; row 10 then reads `checked = true`; `prototype methods` includes `click` | `ComponentMethod.Named("click")` + `self.EmitCall(method)` | `CallReaction` targeting component method `click` | runtime invokes `radio.click()` and the radio becomes selected | accepted and proven |
| `focusIn()` method | core trace row 12 `focusIn [studio]` invoked; `prototype methods` includes `focusIn`; `own keys` includes `isFocused` | `ComponentMethod.Named("focusIn")` + `self.EmitCall(method)` | `CallReaction` targeting component method `focusIn` | runtime invokes `radio.focusIn()` and keyboard focus moves into the radio | accepted and proven |
| `dataBind()` method | core trace `prototype methods` includes `dataBind`; core trace row 11 confirms the `disabled` set takes effect after it | `ComponentMethod.Named("dataBind")` + `self.EmitCall(method)` | `CallReaction` targeting component method `dataBind` | runtime invokes `radio.dataBind()` to flush a property set to the DOM; chained after the `checked`/`disabled` set only | accepted as the repaint companion of the `checked`/`disabled` writes |
| `created` event | core trace row 1 fires `created` with an undefined DOM-native payload | not accepted for the current rows | no public C# event selector | no runtime mapping for this row | excluded; lifecycle-only, no typed payload (`event-payload-surface.json` marks it dom-native) |
| `value` property | `public-api-surface.json` marks `builder.covered = true`; core trace row 6 `value [companion] = "Shared Companion Suite"` reads the per-button value | builder-owned per-button value, surfaced through `change.value` and `getSelectedValue()` | no standalone runtime DSL member | initial value set on `RadioButtonBuilder`; the selected value is read through the change payload and the group read | excluded as a standalone member; the value is covered by the `change.value` payload row and the `getSelectedValue` group read |
| `name`, `label`, `labelPosition`, `cssClass`, `enableHtmlSanitizer` | `public-api-surface.json` marks each `builder.covered = true` | builder-owned static configuration | no runtime DSL member | initial render configured on `RadioButtonBuilder`; no post-render read/write proven necessary | excluded; builder-owned per `references/automation-gates.md` Gate 5 builder-owned exclusion |
| `destroy()` method | `public-api-surface.json` classifies it `skip: lifecycle cleanup` | not a Fusion plan behavior | no runtime DSL member | runtime never calls it from a plan | excluded; lifecycle cleanup, not plan behavior |

## Primitive Decision

No new primitive is needed for the mapped RadioButton rows. Current primitives
already cover every onboarded member:

- component event trigger (`change`);
- event payload read (`value`);
- component property read (`checked`, `disabled`);
- component property write from a literal (`checked`, `disabled`) followed by the `dataBind` repaint call;
- component method-return read (`getSelectedValue`);
- component method call (`click`, `focusIn`).

Any future failure to read one of these accepted members is a
discovery/mapping/typed-contract problem first, never permission to add a
primitive.

## Code To Delete Or Simplify

None identified for the primitive layer. The slice keeps `SetChecked` and
`SetDisabled` paired with `dataBind` rather than introducing a setter that
silently repaints, so each repaint is an explicit mapped row rather than hidden
behavior.

## Behavior Proof Required Before Commit

The RadioButton rows are proven by
`tests/Alis.Reactive.PlaywrightTests/Components/Fusion/RadioButton/WhenUsingFusionRadioButton.cs`
through `scripts/playwright.sh`, with each member tied to a fails-when-broken
assertion in `proof/behavioral-coverage.json`:

1. `FusionRadioButton(...)` render builds the room options from the model (the intake opens with every room option listed);
2. `change` fires the reaction and `value` shows the chosen room with its condition-routed detail;
3. `SetChecked(true)` and `SelectedValue()` apply and read back the recommended room without a click;
4. `Checked()` yields the chosen state into a POST gather body;
5. `SetDisabled(true)` and `Disabled()` take a room off the list and route the unavailable notice;
6. `Click()` and `FocusIn()` select and focus the recommended studio.
