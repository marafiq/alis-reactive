# CheckBox Primitive Map

Status: active and proven. This file maps the onboarded `FusionCheckBox` runtime
surface: the `change` event and its typed payload (`Checked`), the `checked`,
`indeterminate`, and `disabled` property reads and writes, the `click` and
`focusIn` method calls, and the `FusionCheckBox(...)` field render helper. Every
mapped row uses an existing DSL primitive. The component is fully audited.

## Pass Rows

Close matrix row: `Html.InputField(m => m.AgreementAccepted).FusionCheckBox(b => ...)` -> CheckBox field render bound to a boolean model property -> sync input registration plus initial state bound by the Syncfusion builder.

Close matrix row: `checkbox.Reactive(e => e.Changed, (args, p) => ...)` change trigger -> CheckBox `change` payload (`checked`) -> sync component-event reaction reading the typed payload.

Close matrix row: `p.Component<FusionCheckBox>(id).SetChecked(value)` / `SetIndeterminate(value)` / `SetDisabled(value)` -> typed CheckBox property write -> sync component property set on `checked` / `indeterminate` / `disabled` followed by a `dataBind` method call that flushes the visible state.

Close matrix row: `p.Component<FusionCheckBox>(id).Click()` -> typed CheckBox click method call -> sync `click` method call that toggles the checked state and fires `change`.

Close matrix row: `p.Component<FusionCheckBox>(id).FocusIn()` -> typed CheckBox focus method call -> sync `focusIn` method call that moves focus into the input.

Close matrix row: `checkbox.Checked()` / `Indeterminate()` / `Disabled()` -> CheckBox property read source -> sync component property read consumed by gather, conditions, or set text.

## DSL Source Requirements

- `Alis.Reactive.Fusion/Components/FusionCheckBox/FusionCheckBox.cs`
- `Alis.Reactive.Fusion/Components/FusionCheckBox/FusionCheckBoxHtmlExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionCheckBox/FusionCheckBoxExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionCheckBox/FusionCheckBoxEvents.cs`
- `Alis.Reactive.Fusion/Components/FusionCheckBox/FusionCheckBoxReactiveExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionCheckBox/Events/FusionCheckBoxOnChanged.cs`
- `Alis.Reactive/Components/Contracts/ComponentRef.cs`
- `Alis.Reactive/Components/Contracts/ComponentProperty.cs`
- `Alis.Reactive/Components/Contracts/ComponentMember.cs`
- `Alis.Reactive/Components/Onboarding/ComponentEventOnboarding.cs`
- `Alis.Reactive/PlanModel/Values/ValueExpression.cs`

Sync/async lane expectation: the checkbox field render is sync input registration;
the `change` component-event trigger is sync; `SetChecked`, `SetIndeterminate`,
`SetDisabled`, `Click`, `FocusIn`, and the `Checked`, `Indeterminate`, `Disabled`
reads are sync component actions. The CheckBox slice introduces no async boundary.
Async only appears when a developer composes a read source into an HTTP
`Post(...).Gather(...)` pipeline, which is the HTTP primitive, not a CheckBox concern.

## Authoritative Primitive Rows

| CheckBox row member | Syncfusion evidence | DSL primitive | Domain/JSON term | Runtime behavior | Status |
| --- | --- | --- | --- | --- | --- |
| `checked` property read | `traces/raw-ej2-core.trace.json` rows `checked initial read` (false) and `checked after click` (true); `checked` is a boolean on the instance | `ComponentProperty<bool>.Named("checked")` + `self.Read(property)` | `ValueExpression.Read(ComponentObject, "checked", Shape.Boolean)` from `FusionCheckBoxExtensions.Checked(...)` | runtime reads `checkbox.checked` into a typed boolean source | accepted and proven |
| `checked` property write | core trace `prototype methods` includes `dataBind`; raw EJ2 reflects the box after `checked` set + `dataBind()` | `ComponentProperty<bool>` + `self.EmitSet(property, ValueExpression.Literal(value))` then `EmitCall("dataBind")` | `SetReaction` targeting component property `checked`, then `CallReaction` for `dataBind` | runtime writes `checkbox.checked = literal` and calls `checkbox.dataBind()` so the visible box updates | accepted and proven |
| `indeterminate` property read | core trace row `indeterminate read` (true) after the set | `ComponentProperty<bool>.Named("indeterminate")` + `self.Read(property)` | `ValueExpression.Read(ComponentObject, "indeterminate", Shape.Boolean)` from `FusionCheckBoxExtensions.Indeterminate(...)` | runtime reads `checkbox.indeterminate` into a typed boolean source | accepted and proven |
| `indeterminate` property write | core trace `frame class after indeterminate` reads `e-icons e-frame e-stop` only after the set + `dataBind()` | `ComponentProperty<bool>` + `self.EmitSet(property, ...)` then `EmitCall("dataBind")` | `SetReaction` targeting `indeterminate`, then `CallReaction` for `dataBind` | runtime writes `checkbox.indeterminate = literal` and `dataBind()`; the box shows the `e-stop` dash | accepted and proven |
| `disabled` property read | core trace row `disabled read` (true) after the set | `ComponentProperty<bool>.Named("disabled")` + `self.Read(property)` | `ValueExpression.Read(ComponentObject, "disabled", Shape.Boolean)` from `FusionCheckBoxExtensions.Disabled(...)` | runtime reads `checkbox.disabled` into a typed boolean source | accepted and proven |
| `disabled` property write | core trace `wrapper class after disabled` reads `e-checkbox-wrapper e-wrapper e-checkbox-disabled` only after the set + `dataBind()` | `ComponentProperty<bool>` + `self.EmitSet(property, ...)` then `EmitCall("dataBind")` | `SetReaction` targeting `disabled`, then `CallReaction` for `dataBind` | runtime writes `checkbox.disabled = literal` and `dataBind()`; the wrapper carries `e-checkbox-disabled` | accepted and proven |
| `click()` method | core trace `click() method` returns the toggled `checked` (true); `prototype methods` includes `click` | `ComponentMethod.Named("click")` + `self.EmitCall(method)` | `CallReaction` targeting component method `click` | runtime invokes `checkbox.click()`; the box toggles and `change` fires | accepted and proven |
| `focusIn()` method | core trace `focusIn() method` then `active element after focusIn` reads the input id; `prototype methods` includes `focusIn` | `ComponentMethod.Named("focusIn")` + `self.EmitCall(method)` | `CallReaction` targeting component method `focusIn` | runtime invokes `checkbox.focusIn()`; focus moves into the input | accepted and proven |
| `dataBind()` method | core trace `prototype methods` includes `dataBind`; class evidence appears only after `dataBind()` | `ComponentMethod.Named("dataBind")` + `self.EmitCall(method)` | `CallReaction` targeting component method `dataBind` | runtime invokes `checkbox.dataBind()` to flush a property set to the DOM; chained after each `Set*` write only | accepted as the repaint companion of the property writes |
| `change` event trigger | core trace candidate row `change: ChangeEventArgs`; `event-payload-surface.json` resolves the payload | `TypedEvent<FusionCheckBoxChangeArgs>` selected by `.Reactive(...)` | `StartsWhen.ComponentEvent(componentId, "change")` | runtime wires the Syncfusion object event and starts the reaction with event payload scope | accepted and proven |
| `change.checked` | core trace `change` payload `ownKeys` includes `checked: boolean` (sample true); `event-payload-surface.json` `ChangeEventArgs.checked: boolean` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "checked", Shape.Boolean)` from `FusionCheckBoxChangeArgs.Checked` | runtime reads `event.checked` (the state after the change) into set text, condition, or gather | accepted and proven |
| `change.event` | core trace `change` payload `ownKeys` includes `event` (DOM Event, `isTrusted` sample); `event-payload-surface.json` `ChangeEventArgs.event: Event` | excluded browser-owned event object | no public C# payload property | runtime must not serialize or expose this through broad typed event args | excluded; browser-owned DOM event, see `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them` |
| `change.name` | core trace `change` payload `ownKeys` includes `name: "change"`; inherited `BaseEventArgs.name: string` | excluded duplicate event metadata | no public C# payload property | no runtime mapping for this row | excluded; the event selector already owns event identity |
| `created` event | `public-api-surface.json` lists `created`; `event-payload-surface.json` marks its payload dom-native | not accepted for the current rows | no public C# event selector | no runtime mapping for this row | excluded; lifecycle-only, no typed payload (`event-payload-surface.json` marks it dom-native) |
| `cssClass`, `enableHtmlSanitizer`, `indeterminate`, `disabled`, `label`, `labelPosition`, `name`, `value` static configuration | `public-api-surface.json` marks each `builder.covered = true` | builder-owned static configuration | no runtime DSL member for initial render | initial render configured on `CheckBoxBuilder`; only `indeterminate`/`disabled` add a proven post-render read/write row above | excluded as initial-render config per `references/automation-gates.md` Gate 5 builder-owned exclusion |
| `destroy()` method | `public-api-surface.json` classifies it `skip: lifecycle cleanup` | not a Fusion plan behavior | no runtime DSL member | runtime never calls it from a plan | excluded; lifecycle cleanup, not plan behavior |

## Primitive Decision

No new primitive is needed for the mapped CheckBox rows. Current primitives
already cover every onboarded member:

- component event trigger (`change`);
- event payload read (`checked`);
- component property read (`checked`, `indeterminate`, `disabled`);
- component property write from a literal (`checked`, `indeterminate`, `disabled`) followed by the `dataBind` repaint call;
- component method call (`click`, `focusIn`).

Any future failure to read or write one of these accepted members is a
discovery/mapping/typed-contract problem first, never permission to add a
primitive.

## Code To Delete Or Simplify

None identified for the primitive layer. The slice keeps every `Set*` write
paired with `dataBind` rather than introducing a setter that silently repaints,
so the repaint is an explicit mapped row rather than hidden behavior.

## Behavior Proof Required Before Commit

The CheckBox rows are proven by `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/CheckBox/WhenUsingFusionCheckBox.cs`
through `scripts/playwright.sh`, with each member tied to a fails-when-broken
assertion in `proof/behavioral-coverage.json`:

1. `FusionCheckBox(...)` render binds the model state (the move-in form opens with both boxes rendered and housekeeping locked);
2. `change` fires the reaction and `Checked` routes the accepted/declined message;
3. `SetChecked(true)` checks the box for the resident;
4. `SetIndeterminate(true)` and `Indeterminate()` show the follow-up dash and message;
5. `SetDisabled(true/false)` and `Disabled()` lock then unlock the optional service;
6. `Click()` toggles the box on the resident's behalf;
7. `FocusIn()` moves focus into the agreement checkbox;
8. `Checked()` and `Indeterminate()` yield into the POST gather body.
