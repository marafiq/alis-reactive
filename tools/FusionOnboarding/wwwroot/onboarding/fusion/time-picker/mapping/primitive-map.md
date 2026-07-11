# TimePicker Primitive Map

Status: active and proven. This file maps the onboarded `FusionTimePicker`
runtime surface: the `change` event and its typed payload (`Value`,
`IsInteracted`), the typed `SetValue` write, the `FocusIn`/`FocusOut` method
calls, the `Value` read source, and the `FusionTimePicker(...)` field render
helper. Every mapped row uses an existing DSL primitive. The component is fully
audited.

## Pass Rows

Close matrix row: `Html.InputField(m => m.MedicationTime).FusionTimePicker(b => ...)` -> TimePicker field render bound to a `DateTime?` model property -> sync input registration plus initial value bound by the Syncfusion builder.

Close matrix row: `picker.Reactive(e => e.Changed, (args, p) => ...)` change trigger -> TimePicker `change` payload (`value`, `isInteracted`) -> sync component-event reaction reading the typed payload.

Close matrix row: `p.Component<FusionTimePicker>(id).SetValue(value)` -> typed TimePicker value write -> sync component property set on `value` (serialized `HH:mm`, `Shape.Date`).

Close matrix row: `p.Component<FusionTimePicker>(id).FocusIn()` / `.FocusOut()` -> typed TimePicker focus method calls -> sync `focusIn`/`focusOut` method calls that move focus into / out of the textbox.

Close matrix row: `picker.Value()` -> TimePicker value read source -> sync component property read of `value` consumed by gather, conditions, or set text.

## DSL Source Requirements

- `Alis.Reactive.Fusion/Components/FusionTimePicker/FusionTimePicker.cs`
- `Alis.Reactive.Fusion/Components/FusionTimePicker/FusionTimePickerHtmlExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionTimePicker/FusionTimePickerExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionTimePicker/FusionTimePickerEvents.cs`
- `Alis.Reactive.Fusion/Components/FusionTimePicker/FusionTimePickerReactiveExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionTimePicker/Events/FusionTimePickerOnChanged.cs`
- `Alis.Reactive/Components/Contracts/ComponentRef.cs`
- `Alis.Reactive/Components/Contracts/ComponentProperty.cs`
- `Alis.Reactive/Components/Contracts/ComponentMember.cs`
- `Alis.Reactive/Components/Onboarding/ComponentEventOnboarding.cs`
- `Alis.Reactive/PlanModel/Values/ValueExpression.cs`

Sync/async lane expectation: the time picker field render is sync input
registration; the `change` component-event trigger is sync; `SetValue`,
`FocusIn`, `FocusOut`, and the `Value` read are sync component actions. The
TimePicker slice introduces no async boundary. Async only appears when a
developer composes the read `Value()` source into an HTTP
`Post(...).Gather(...)` pipeline, which is the HTTP primitive, not a TimePicker
concern.

## Authoritative Primitive Rows

| TimePicker row member | Syncfusion evidence | DSL primitive | Domain/JSON term | Runtime behavior | Status |
| --- | --- | --- | --- | --- | --- |
| `value` property read | `traces/raw-ej2-core.trace.json` `own keys`/`prototype methods` show the `value` member on `ej.calendars.TimePicker`; the sandbox reads `09:00` back from the carried-over model value | `ComponentProperty<DateTime>.Named("value")` + `self.Read(property)` | `ValueExpression.Read(ComponentObject, "value", Shape.Date)` from `FusionTimePickerExtensions.Value(...)` | runtime reads `timepicker.value` into a typed date source | accepted and proven |
| `value` property write | core trace `prototype methods` include `dataBind`; raw EJ2 sets `value` from a parsable `HH:mm` string and the input repaints (verified: a `h:mm a` display leaves an `HH:mm` write unparsed, so the slice + sandbox use a matching `HH:mm` display) | `ComponentProperty<DateTime>` + `self.EmitSet(property, ValueExpression.LiteralRaw(value.ToString("HH:mm"), Shape.Date))` | `SetReaction` targeting component property `value` | runtime writes `timepicker.value = literal` and the visible field updates to the written time | accepted and proven |
| `focusIn()` method | `timepicker.d.ts:636-640` `focusIn(): void` "Focused the TimePicker textbox element"; core trace `prototype methods` include `focusIn` | `ComponentMethod.Named("focusIn")` + `self.EmitCall(method)` | `CallReaction` targeting component method `focusIn` | runtime invokes `timepicker.focusIn()` and the textbox receives focus | accepted and proven |
| `focusOut()` method | `timepicker.d.ts:626-632` `focusOut(): void` "Focuses out the TimePicker textbox element"; core trace `prototype methods` include `focusOut` | `ComponentMethod.Named("focusOut")` + `self.EmitCall(method)` | `CallReaction` targeting component method `focusOut` | runtime invokes `timepicker.focusOut()` and the textbox loses focus | accepted and proven |
| `change` event trigger | core trace `prototype methods` include `changeEvent`; `timepicker.js:1874` builds `changeEvent` with `value`/`isInteracted`; `event-payload-surface.json` resolves the payload | `TypedEvent<FusionTimePickerChangeArgs>` selected by `.Reactive(...)` | `StartsWhen.ComponentEvent(componentId, "change")` | runtime wires the Syncfusion object event and starts the reaction with event payload scope | accepted and proven |
| `change.value` | `timepicker.js:1884` `eventArgs.value = this.valueWithMinutes ...`; `event-payload-surface.json` `value: Date` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "value", Shape.Date)` from `FusionTimePickerChangeArgs.Value` | runtime reads `event.value` (the newly selected time) into set text, condition, or gather | accepted and proven |
| `change.isInteracted` | `timepicker.js:1881` `isInteracted: !isNullOrUndefined(e)`; `event-payload-surface.json` `isInteracted: boolean` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "isInteracted", Shape.Boolean)` from `FusionTimePickerChangeArgs.IsInteracted` | runtime reads `event.isInteracted` to distinguish a user choice (true) from a programmatic `SetValue` (false) | accepted and proven |
| `change.event`, `change.element`, `change.name`, `change.isInteracted`-siblings (`text`) | `event-payload-surface.json` records the remaining `ChangedEventArgs` members | excluded browser/metadata payload | no public C# payload property | runtime must not serialize or expose these through broad typed event args | excluded; browser-owned DOM/event metadata, see `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them` |
| `show()` / `hide()` methods | `timepicker.d.ts:648-654` `show(event?)` "Opens the popup to show the list items"; `:642-647` `hide()` "Hides the TimePicker popup" | not accepted; popup toggles | no runtime DSL member | resident opens/closes the time list by gesture; no plan reaction forces it | excluded; see `discovery/parity-accounting.json` (interaction-level vendor control) |
| `readonly` property | `timepicker.d.ts:297-302` `readonly: boolean` "Specifies the component in readonly state", `@default false` | builder-owned render config | no runtime DSL member | initial render configured on `TimePickerBuilder`; no post-render read/write proven | excluded; render-time field config, `discovery/parity-accounting.json` |
| `requiredModules()` method | `timepicker.d.ts:516` `requiredModules(): ModuleDeclaration[]` | not a Fusion plan behavior | no runtime DSL member | Syncfusion module-loading metadata | excluded; vendor internals, `discovery/parity-accounting.json` |
| the 30 builder-covered options (`format`, `step`, `min`, `max`, `placeholder`, `cssClass`, `enabled`, `strictMode`, `value`, templates, and the rest) | `public-api-surface.json` marks each `builder.covered = true` | builder-owned static configuration | no runtime DSL member | initial render configured on `TimePickerBuilder`; no post-render read/write proven necessary | excluded; builder-owned per `references/automation-gates.md` Gate 5 builder-owned exclusion |

## Primitive Decision

No new primitive is needed for the mapped TimePicker rows. Current primitives
already cover every onboarded member:

- component event trigger (`change`);
- event payload read (`value`, `isInteracted`);
- component property read (`value`);
- component property write from a literal (`value`);
- component method call (`focusIn`, `focusOut`).

Any future failure to read one of these accepted members is a
discovery/mapping/typed-contract problem first, never permission to add a
primitive.

## Code To Delete Or Simplify

None identified for the primitive layer. The slice keeps the `value` write
serialized as `HH:mm` (`Shape.Date`); the matching `HH:mm` display format is a
sandbox render decision so a programmatic `SetValue` parses (a `h:mm a` display
leaves an `HH:mm` write unparsed — proven in the browser).

## Behavior Proof Required Before Commit

The TimePicker rows are proven by `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/TimePicker/WhenTimeSelected.cs`
through `scripts/playwright.sh`, with each member tied to a fails-when-broken
assertion in `proof/behavioral-coverage.json`:

1. `FusionTimePicker(...)` render binds the model value (the scheduler opens showing 09:00 carried over);
2. `change` fires the reaction and `value` shows the newly selected time with its ready-to-confirm status;
3. `isInteracted` distinguishes a time the coordinator chose from a programmatic `SetValue`;
4. `SetValue(DateTime)` writes the standard morning round (08:00) onto the field;
5. `FocusIn()` moves focus into the field; `FocusOut()` releases it after a pick;
6. `Value()` yields the current time into a POST gather body.
