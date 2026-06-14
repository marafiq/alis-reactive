# CheckBox C# Name Decisions

Status: active and proven. The `FusionCheckBox` public C# names are decided and
implemented: the `FusionCheckBox(...)` render helper, the `Changed` event
selector with the `FusionCheckBoxChangeArgs` payload (`Checked`), the typed
`SetChecked(bool)`, `SetIndeterminate(bool)`, and `SetDisabled(bool)` writes, the
`Click()` and `FocusIn()` methods, and the `Checked()`, `Indeterminate()`, and
`Disabled()` read sources. The component is fully audited.

## Pass Rows

Close matrix row: `Html.InputField(m => m.AgreementAccepted).FusionCheckBox(b => ...)` render helper -> CheckBox field bound to a boolean model property.

Close matrix row: `checkbox.Reactive(e => e.Changed, ...)` -> typed `FusionCheckBoxChangeArgs` payload.

Close matrix row: `SetChecked(bool)`, `SetIndeterminate(bool)`, `SetDisabled(bool)`, `Click()`, `FocusIn()`, `Checked()`, `Indeterminate()`, `Disabled()` -> typed CheckBox runtime members.

## Evidence Inputs

- Raw core trace: `traces/raw-ej2-core.trace.json`
- Raw core probe: `probes/raw-ej2-core.html`
- Syncfusion source type: `ChangeEventArgs` (event), `CheckBox` (component)
- Public API surface: `discovery/public-api-surface.json`
- Event payload surface: `discovery/event-payload-surface.json`
- Blazor candidates: `discovery/blazor-candidates.md` (no Blazor package supplied; naming taken from EJ2 source only)
- Existing C# event args: `Alis.Reactive.Fusion/Components/FusionCheckBox/Events/FusionCheckBoxOnChanged.cs`
- Existing event selector: `Alis.Reactive.Fusion/Components/FusionCheckBox/FusionCheckBoxEvents.cs`
- Existing component members: `Alis.Reactive.Fusion/Components/FusionCheckBox/FusionCheckBoxExtensions.cs`
- Existing render helper: `Alis.Reactive.Fusion/Components/FusionCheckBox/FusionCheckBoxHtmlExtensions.cs`
- Existing sandbox usage: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/CheckBox/Index.cshtml`

## Name Decision Matrix

| Syncfusion path | C# name | Decision | Reason |
| --- | --- | --- | --- |
| `new ej.buttons.CheckBox(options)` field render | `InputBoundField<TModel, bool>.FusionCheckBox(Action<CheckBoxBuilder> build)` | keep | the render helper binds the EJ2 CheckBox to a boolean model property through the standard `Html.InputField` field wrapper; initial options stay on `CheckBoxBuilder` |
| `change` event | `FusionCheckBoxEvents.Changed` | keep | the Fusion selector name reads as the developer intent ("when it changed"); selected through the typed `.Reactive(e => e.Changed, ...)` event lambda over the exact `change` event |
| `ChangeEventArgs` | `FusionCheckBoxChangeArgs` | keep | the Fusion payload type name states the event it belongs to; it carries only the proven, narrowed member |
| `change.checked` | `FusionCheckBoxChangeArgs.Checked` | keep | exact Syncfusion key, typed as `bool`; the checked state after the change (trace `change` payload `checked: true`) |
| `change.event` | none | exclude from public typed payload | browser-owned DOM `Event` (trace sample `isTrusted`); exposing it as `object`/`dynamic` would pollute the public DSL (see `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them`) |
| `change.name` (inherited `BaseEventArgs.name`) | none | exclude for this row | duplicate event identity metadata (trace `name: "change"`); the `Changed` selector already owns the event identity |
| `checked` property write | `SetChecked(this ComponentRef<FusionCheckBox, TModel> self, bool isChecked)` | keep | states developer intent ("set the checked state"); maps to a `checked` property set plus a `dataBind()` repaint, not raw member strings |
| `indeterminate` property write | `SetIndeterminate(this ComponentRef<FusionCheckBox, TModel> self, bool isIndeterminate)` | keep | exact Syncfusion property name; maps to an `indeterminate` set plus `dataBind()`; the box shows the `e-stop` dash |
| `disabled` property write | `SetDisabled(this ComponentRef<FusionCheckBox, TModel> self, bool disabled)` | keep | exact Syncfusion property name; maps to a `disabled` set plus `dataBind()`; the wrapper carries `e-checkbox-disabled` |
| `click()` method | `Click(this ComponentRef<FusionCheckBox, TModel> self)` | keep | exact Syncfusion method name; toggles the checked state and fires `change` |
| `focusIn()` method | `FocusIn(this ComponentRef<FusionCheckBox, TModel> self)` | keep | exact Syncfusion method name; moves focus into the input |
| `checked` property read | `Checked(this ComponentRef<FusionCheckBox, TModel> self)` | keep | concise read name returns a typed `bool` source for gather/conditions/set text |
| `indeterminate` property read | `Indeterminate(this ComponentRef<FusionCheckBox, TModel> self)` | keep | exact Syncfusion property name; returns a typed `bool` source |
| `disabled` property read | `Disabled(this ComponentRef<FusionCheckBox, TModel> self)` | keep | exact Syncfusion property name; returns a typed `bool` source |
| `dataBind()` method | none (internal repaint companion of `Set*`) | keep internal | not a standalone public member; chained after each property set so the visible box updates; exposing it alone has no proven typed use case |
| `created` event | none | exclude for the current rows | DOM-native lifecycle event with no typed payload (`event-payload-surface.json` marks it dom-native); no focused Senior Living use case |
| `cssClass`, `enableHtmlSanitizer`, `label`, `labelPosition`, `name`, `value` | none | exclude as builder-owned | `discovery/public-api-surface.json` marks each `builder.covered = true`; configured on `CheckBoxBuilder` at initial render, no post-render read/write proven necessary |
| `destroy()` method | none | exclude as lifecycle | `discovery/public-api-surface.json` classifies it `skip: lifecycle cleanup`, not plan behavior |

## Builder-Owned Versus Post-Render Property Note

`indeterminate` and `disabled` appear both as `builder.covered = true` (initial
render configuration on `CheckBoxBuilder`) AND as accepted post-render runtime
read/write members. There is no conflict: the builder owns the value at first
paint, and the Fusion slice owns reading and mutating it after render, which the
raw trace proves (the `e-stop` and `e-checkbox-disabled` classes appear only
after a post-render set followed by `dataBind()`). `checked` is NOT builder-owned
in the surface (`builder.covered = false`) and is accounted as onboarded-typed in
`discovery/parity-accounting.json`.

## Blazor Naming Rule

Blazor metadata may be used only as naming evidence after the EJ2 row is proven.
`discovery/blazor-candidates.md` records that no Syncfusion Blazor package was
supplied for this pass, so every accepted C# name above comes from the EJ2 source
and the raw core trace, not from Blazor metadata.

## Discovery Versus C# DSL Boundary

All observed fields stay in discovery. Only fields with clear, typed, predictable
Fusion use cases are accepted into the public C# surface. `change.event` remains
discovered but excluded because it is a browser-owned DOM `Event`; exposing it as
`object` or `dynamic` would pollute the public DSL. The builder-covered static
properties remain discovered but excluded as initial-render configuration except
where a post-render read/write is proven necessary (`indeterminate`, `disabled`).

## Implementation Boundary

Implemented public surface for the CheckBox slice:

- the `FusionCheckBox(...)` render helper bound to a boolean model property;
- the `Changed` event selector and `FusionCheckBoxChangeArgs` payload with `Checked`;
- the `SetChecked(bool)`, `SetIndeterminate(bool)`, and `SetDisabled(bool)` writes (property set plus `dataBind` repaint);
- the `Click()` and `FocusIn()` methods;
- the `Checked()`, `Indeterminate()`, and `Disabled()` read sources.

Out of scope for the CheckBox slice: new primitives, builder-owned initial-render
static properties, the `created` lifecycle event, and the lifecycle `destroy` method.
