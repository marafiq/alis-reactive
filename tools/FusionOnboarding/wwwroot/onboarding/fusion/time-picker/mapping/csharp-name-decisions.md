# TimePicker C# Name Decisions

Status: active and proven. The `FusionTimePicker` public C# names are decided and
implemented: the `FusionTimePicker(...)` render helper, the `Changed` event
selector with the `FusionTimePickerChangeArgs` payload (`Value`, `IsInteracted`),
the typed `SetValue(DateTime)` write, the `FocusIn()`/`FocusOut()` methods, and
the `Value()` read source. The component is fully audited.

## Pass Rows

Close matrix row: `Html.InputField(m => m.MedicationTime).FusionTimePicker(b => ...)` render helper -> TimePicker field bound to a `DateTime?` model property.

Close matrix row: `picker.Reactive(e => e.Changed, ...)` -> typed `FusionTimePickerChangeArgs` payload.

Close matrix row: `SetValue(DateTime)`, `FocusIn()`, `FocusOut()`, `Value()` -> typed TimePicker runtime members.

## Evidence Inputs

- Raw core trace: `traces/raw-ej2-core.trace.json`
- Raw core probe: `probes/raw-ej2-core.html`
- Syncfusion source type: `ChangedEventArgs` (event), `TimePicker` (component), `node_modules/@syncfusion/ej2-calendars/src/timepicker/timepicker.d.ts`
- Public API surface: `discovery/public-api-surface.json`
- Event payload surface: `discovery/event-payload-surface.json`
- Blazor candidates: `discovery/blazor-candidates.md` (no Blazor package supplied; naming taken from EJ2 source only)
- Existing C# event args: `Alis.Reactive.Fusion/Components/FusionTimePicker/Events/FusionTimePickerOnChanged.cs`
- Existing event selector: `Alis.Reactive.Fusion/Components/FusionTimePicker/FusionTimePickerEvents.cs`
- Existing component members: `Alis.Reactive.Fusion/Components/FusionTimePicker/FusionTimePickerExtensions.cs`
- Existing render helper: `Alis.Reactive.Fusion/Components/FusionTimePicker/FusionTimePickerHtmlExtensions.cs`
- Existing sandbox usage: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/TimePicker/Index.cshtml`

## Name Decision Matrix

| Syncfusion path | C# name | Decision | Reason |
| --- | --- | --- | --- |
| `new ej.calendars.TimePicker(options)` field render | `InputBoundField<TModel, TProp>.FusionTimePicker(Action<TimePickerBuilder> build)` | keep | the render helper binds the EJ2 TimePicker to a model property through the standard `Html.InputField` field wrapper; initial options stay on `TimePickerBuilder` |
| `change` event | `FusionTimePickerEvents.Changed` | keep | maps the exact Syncfusion `change` event; the C# selector name `Changed` reads as the past-tense behavior, selected through the typed `.Reactive(e => e.Changed, ...)` event lambda |
| `ChangedEventArgs` | `FusionTimePickerChangeArgs` | keep | the Fusion payload type name states the event it belongs to; it carries only the proven, narrowed members |
| `change.value` | `FusionTimePickerChangeArgs.Value` | keep | exact Syncfusion key, typed as `DateTime?`; the newly selected time (`timepicker.js:1884`) |
| `change.isInteracted` | `FusionTimePickerChangeArgs.IsInteracted` | keep | exact Syncfusion key, typed as `bool`; distinguishes a user choice from a programmatic change (`timepicker.js:1881` `!isNullOrUndefined(e)`) |
| `change.event`, `change.element`, `change.name`, `change.text` | none | exclude from public typed payload | browser-owned DOM `Event`/element and duplicate metadata; exposing them as `object`/`dynamic` would pollute the public DSL (see `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them`) |
| `value` property write | `SetValue(this ComponentRef<FusionTimePicker, TModel> self, DateTime value)` | keep | states developer intent ("set the time"); maps to a `value` property set serialized `HH:mm` with `Shape.Date`, not raw member strings |
| `focusIn()` method | `FocusIn(this ComponentRef<FusionTimePicker, TModel> self)` | keep | exact Syncfusion method name (`timepicker.d.ts:640`); moves focus into the textbox |
| `focusOut()` method | `FocusOut(this ComponentRef<FusionTimePicker, TModel> self)` | keep | exact Syncfusion method name (`timepicker.d.ts:632`); removes focus from the textbox |
| `value` property read | `Value(this ComponentRef<FusionTimePicker, TModel> self)` | keep | concise read name returns a typed `DateTime` source for gather/conditions/set text |
| `show()`, `hide()` methods | none | exclude as interaction-level | popup open/close the resident drives by gesture; no plan reaction forces them (`discovery/parity-accounting.json`) |
| `readonly` property | none | exclude as builder-owned | render-time field config the MVC builder owns (`timepicker.d.ts:298`); no post-render read/write proven |
| `requiredModules()` method | none | exclude as vendor internals | Syncfusion module-loading metadata (`timepicker.d.ts:516`); no DSL primitive mapping |
| 30 builder-covered options (`format`, `step`, `min`, `max`, `placeholder`, `cssClass`, `enabled`, `strictMode`, `value`, templates, and the rest) | none | exclude as builder-owned | `discovery/public-api-surface.json` marks each `builder.covered = true`; configured on `TimePickerBuilder` at initial render, no post-render read/write proven necessary |

## Blazor Naming Rule

Blazor metadata may be used only as naming evidence after the EJ2 row is proven.
`discovery/blazor-candidates.md` records that no Syncfusion Blazor package was
supplied for this pass, so every accepted C# name above comes from the EJ2
source (`timepicker.d.ts`, `timepicker.js`) and the raw core trace, not from
Blazor metadata.

## Discovery Versus C# DSL Boundary

All observed fields stay in discovery. Only fields with clear, typed,
predictable Fusion use cases are accepted into the public C# event args.
`change.event`/`change.element` remain discovered but excluded because they are
browser-owned; exposing them as `object` or `dynamic` would pollute the public
DSL. The 30 builder-covered options remain discovered but excluded because the
Syncfusion MVC builder owns initial render configuration and no post-render
read/write is proven necessary. `show`/`hide`/`readonly`/`requiredModules` are
recorded in `discovery/parity-accounting.json` with source-grounded exclusion
reasons.

## Implementation Boundary

Implemented public surface for the TimePicker slice:

- the `FusionTimePicker(...)` render helper bound to a `DateTime?` model property;
- the `Changed` event selector and `FusionTimePickerChangeArgs` payload with `Value` and `IsInteracted`;
- the `SetValue(DateTime)` write (property set, `HH:mm`, `Shape.Date`);
- the `FocusIn()` and `FocusOut()` methods;
- the `Value()` read source.

Out of scope for the TimePicker slice: new primitives, builder-owned static
options, the `show`/`hide` popup toggles, the `readonly` render flag, and the
`requiredModules` lifecycle metadata.
