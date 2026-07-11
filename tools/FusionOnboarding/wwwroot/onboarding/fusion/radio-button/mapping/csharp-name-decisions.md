# RadioButton C# Name Decisions

Status: active and proven. The `FusionRadioButton` public C# names are decided
and implemented: the `FusionRadioButton(...)` element render helper, the
`Changed` event selector with the `FusionRadioButtonChangeArgs` payload
(`Value`), the typed `SetChecked(bool)` and `SetDisabled(bool)` writes, the
`Checked()`, `Disabled()`, and `SelectedValue()` read sources, and the
`Click()` and `FocusIn()` methods. The component is fully audited.

## Pass Rows

Close matrix row: `Html.FusionRadioButton(plan, id, b => ...)` render helper -> RadioButton element with builder-owned options.

Close matrix row: `radio.Reactive(e => e.Changed, ...)` -> typed `FusionRadioButtonChangeArgs` payload.

Close matrix row: `SetChecked(bool)`, `SetDisabled(bool)`, `Checked()`, `Disabled()`, `SelectedValue()`, `Click()`, `FocusIn()` -> typed RadioButton runtime members.

## Evidence Inputs

- Raw core trace: `traces/raw-ej2-core.trace.json`
- Raw core probe: `probes/raw-ej2-core.html`
- Syncfusion source type: `ChangeArgs` (event), `RadioButton` (component)
- Public API surface: `discovery/public-api-surface.json`
- Event payload surface: `discovery/event-payload-surface.json`
- MVC builder coverage: `discovery/mvc-builder-coverage.md`
- Blazor candidates: `discovery/blazor-candidates.md` (no Blazor package supplied; naming taken from EJ2 source only)
- Existing C# event args: `Alis.Reactive.Fusion/Components/FusionRadioButton/Events/FusionRadioButtonOnChanged.cs`
- Existing event selector: `Alis.Reactive.Fusion/Components/FusionRadioButton/FusionRadioButtonEvents.cs`
- Existing component members: `Alis.Reactive.Fusion/Components/FusionRadioButton/FusionRadioButtonExtensions.cs`
- Existing render helper: `Alis.Reactive.Fusion/Components/FusionRadioButton/FusionRadioButtonHtmlExtensions.cs`
- Existing sandbox usage: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/RadioButton/Index.cshtml`

## Name Decision Matrix

| Syncfusion path | C# name | Decision | Reason |
| --- | --- | --- | --- |
| `new ej.buttons.RadioButton(options)` element render | `IHtmlHelper<TModel>.FusionRadioButton(ReactivePlan<TModel> plan, string elementId, Action<RadioButtonBuilder> build)` | keep | the render helper renders one EJ2 RadioButton with a stable element id; initial options (label, name, value, checked) stay on `RadioButtonBuilder`. A radio is a group choice rendered by element id, not bound through `Html.InputField`, so the helper takes an explicit element id like `Html.NativeButton` does |
| `change` event | `FusionRadioButtonEvents.Changed` | keep | the typed selector reads cleaner as `Changed`; the underlying EJ2 event name stays `change`, declared in `TypedEvent<FusionRadioButtonChangeArgs>("change", ...)`, selected through `.Reactive(e => e.Changed, ...)` |
| `ChangeArgs` | `FusionRadioButtonChangeArgs` | keep | the Fusion payload type name states the event it belongs to; it carries only the proven, narrowed member |
| `change.value` | `FusionRadioButtonChangeArgs.Value` | keep | exact Syncfusion key, typed as `string`; the selected radio value (core trace row 7 shows `"Shared Companion Suite"`) |
| `change.event` | none | exclude from public typed payload | browser-owned DOM `Event` (core trace row 7 `event.sample { isTrusted: true }`); exposing it as `object`/`dynamic` would pollute the public DSL (see `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them`) |
| `change.name` (inherited `BaseEventArgs.name`) | none | exclude for this row | duplicate event identity metadata; the `Changed` selector already owns the event identity |
| `checked` property write | `SetChecked(this ComponentRef<FusionRadioButton, TModel> self, bool isChecked)` | keep | states developer intent ("set this radio checked"); maps to a `checked` property set plus a `dataBind()` repaint, not raw member strings |
| `disabled` property write | `SetDisabled(this ComponentRef<FusionRadioButton, TModel> self, bool disabled)` | keep | states developer intent ("disable this radio"); maps to a `disabled` property set plus a `dataBind()` repaint |
| `checked` property read | `Checked(this ComponentRef<FusionRadioButton, TModel> self)` | keep | concise read name returns a typed `bool` source for gather/conditions/set text |
| `disabled` property read | `Disabled(this ComponentRef<FusionRadioButton, TModel> self)` | keep | concise read name returns a typed `bool` source for conditions/set text |
| `getSelectedValue()` method | `SelectedValue(this ComponentRef<FusionRadioButton, TModel> self)` | keep | `SelectedValue` reads cleaner than the raw `GetSelectedValue`; returns a typed `string` source reading the group's selected value (core trace row 9) |
| `click()` method | `Click(this ComponentRef<FusionRadioButton, TModel> self)` | keep | exact Syncfusion method name; selects the radio (core trace rows 8 and 10) |
| `focusIn()` method | `FocusIn(this ComponentRef<FusionRadioButton, TModel> self)` | keep | exact Syncfusion method name; moves keyboard focus into the radio (core trace row 12) |
| `dataBind()` method | none (internal repaint companion of `SetChecked`/`SetDisabled`) | keep internal | not a standalone public member; chained after the `checked`/`disabled` set so the radio repaints; exposing it alone has no proven typed use case |
| `value` property | none (read through `change.value` and `getSelectedValue`) | exclude as standalone | `discovery/public-api-surface.json` marks it `builder.covered = true`; the per-button value is configured on `RadioButtonBuilder` and surfaced through the change payload and the group read, so no standalone read/write member is proven necessary |
| `created` event | none | exclude for the current rows | DOM-native lifecycle event with an undefined payload (core trace row 1); no focused Senior Living use case |
| `name`, `label`, `labelPosition`, `cssClass`, `enableHtmlSanitizer` | none | exclude as builder-owned | `discovery/public-api-surface.json` marks each `builder.covered = true`; configured on `RadioButtonBuilder` at initial render, no post-render read/write proven necessary |
| `destroy()` method | none | exclude as lifecycle | `discovery/public-api-surface.json` classifies it `skip: lifecycle cleanup`, not plan behavior |

## Blazor Naming Rule

Blazor metadata may be used only as naming evidence after the EJ2 row is proven.
`discovery/blazor-candidates.md` records that no Syncfusion Blazor package was
supplied for this pass, so every accepted C# name above comes from the EJ2
source and the raw core trace, not from Blazor metadata.

## Discovery Versus C# DSL Boundary

All observed fields stay in discovery. Only fields with clear, typed,
predictable Fusion use cases are accepted into the public C# surface.
`change.event` remains discovered but excluded because it is a browser-owned DOM
`Event`; exposing it as `object` or `dynamic` would pollute the public DSL. The
builder-covered properties (`name`, `label`, `labelPosition`, `cssClass`,
`enableHtmlSanitizer`, and the per-button `value`) remain discovered but
excluded because the Syncfusion MVC builder owns initial render configuration and
no post-render read/write is proven necessary.

## Implementation Boundary

Implemented public surface for the RadioButton slice:

- the `FusionRadioButton(...)` element render helper with builder-owned options;
- the `Changed` event selector and `FusionRadioButtonChangeArgs` payload with `Value`;
- the `SetChecked(bool)` and `SetDisabled(bool)` writes (property set plus `dataBind` repaint);
- the `Checked()`, `Disabled()`, and `SelectedValue()` read sources;
- the `Click()` and `FocusIn()` methods.

Out of scope for the RadioButton slice: new primitives, builder-owned static
properties, the per-button `value` as a standalone member, the `created`
lifecycle event, and the lifecycle `destroy` method.
