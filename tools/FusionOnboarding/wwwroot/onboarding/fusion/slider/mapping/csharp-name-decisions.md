# Slider C# Name Decisions

Status: active and proven. The `FusionSlider` public C# names are decided and
implemented: the `FusionSlider(...)` render helper, the `Change` and `Changed`
event selectors with the `FusionSliderChangeArgs` payload (`Value`,
`PreviousValue`, `Text`, `Action`, `IsInteracted`), the typed `SetValue(double)`
and `SetRangeValue(double, double)` writes, and the `Value()` and `RangeValue()`
read sources. The component is fully audited.

## Pass Rows

Close matrix row: `Html.InputField(m => m.RoomTemperature).FusionSlider(b => ...)` render helper -> Slider field bound to a numeric model property.

Close matrix row: `slider.Reactive(e => e.Change, ...)` and `slider.Reactive(e => e.Changed, ...)` -> typed `FusionSliderChangeArgs` payload.

Close matrix row: `SetValue(double)`, `SetRangeValue(double, double)`, `Value()`, `RangeValue()` -> typed Slider runtime members.

## Evidence Inputs

- Raw core trace: `traces/raw-ej2-core.trace.json`
- Raw core probe: `probes/raw-ej2-core.html`
- Syncfusion source type: `SliderChangeEventArgs` (event), `Slider` (component)
- Public API surface: `discovery/public-api-surface.json`
- Event payload surface: `discovery/event-payload-surface.json`
- Parity accounting (excluded members): `discovery/parity-accounting.json`
- Blazor candidates: `discovery/blazor-candidates.md` (no Blazor package supplied; naming taken from EJ2 source only)
- Existing C# event args: `Alis.Reactive.Fusion/Components/FusionSlider/Events/FusionSliderOnChanged.cs`
- Existing event selector: `Alis.Reactive.Fusion/Components/FusionSlider/FusionSliderEvents.cs`
- Existing component members: `Alis.Reactive.Fusion/Components/FusionSlider/FusionSliderExtensions.cs`
- Existing render helper: `Alis.Reactive.Fusion/Components/FusionSlider/FusionSliderHtmlExtensions.cs`
- Existing sandbox usage: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Slider/Index.cshtml`

## Name Decision Matrix

| Syncfusion path | C# name | Decision | Reason |
| --- | --- | --- | --- |
| `new ej.inputs.Slider(options)` field render | `InputBoundField<TModel, TProp>.FusionSlider(Action<SliderBuilder> build)` | keep | the render helper binds the EJ2 Slider to a numeric model property through the standard `Html.InputField` field wrapper; initial options stay on `SliderBuilder` |
| `change` event | `FusionSliderEvents.Change` | keep | exact Syncfusion event name; fires as the handle moves; selected through `.Reactive(e => e.Change, ...)` |
| `changed` event | `FusionSliderEvents.Changed` | keep | exact Syncfusion event name; fires when the handle settles (and from `setValue()`); selected through `.Reactive(e => e.Changed, ...)` |
| `SliderChangeEventArgs` | `FusionSliderChangeArgs` | keep | the Fusion payload type name states the change events it belongs to; it carries only the proven, narrowed members; both `Change` and `Changed` share this contract because `slider.js changeEventArgs` builds the same shape for both |
| `change/changed.value` | `FusionSliderChangeArgs.Value` | keep | exact Syncfusion key, typed as `double`; the slider value after the change |
| `change/changed.previousValue` | `FusionSliderChangeArgs.PreviousValue` | keep | exact Syncfusion key, typed as `double`; `previousVal` for `change`, `previousChanged` for `changed` |
| `change/changed.text` | `FusionSliderChangeArgs.Text` | keep | exact Syncfusion key, typed as nullable `string`; the formatted value string |
| `change/changed.action` | `FusionSliderChangeArgs.Action` | keep | exact Syncfusion key, typed as nullable `string`; the Syncfusion change-action name (`"change"`/`"changed"`) |
| `change/changed.isInteracted` | `FusionSliderChangeArgs.IsInteracted` | keep | exact Syncfusion key, typed as `bool`; distinguishes a value the resident chose (true) from one applied for them (false) |
| `value` property write (scalar) | `SetValue(this ComponentRef<FusionSlider, TModel> self, double value)` | keep | states developer intent ("set the slider value"); maps to a `value` property set plus a `dataBind()` repaint, not raw member strings |
| `value` property write (range) | `SetRangeValue(this ComponentRef<FusionSlider, TModel> self, double start, double end)` | keep | a distinct typed method and the distinct plan member name `rangeValue` mapped to the same JS `value` path, so the overloaded vendor property stays deterministic in the contract merge |
| `value` property read (scalar) | `Value(this ComponentRef<FusionSlider, TModel> self)` | keep | concise read name returns a typed `double` source for gather/conditions/set text |
| `value` property read (range) | `RangeValue(this ComponentRef<FusionSlider, TModel> self)` | keep | concise read name returns a typed `double[]` source for gather/set text; distinct plan member `rangeValue` |
| `dataBind()` method | none (internal repaint companion of `SetValue`/`SetRangeValue`) | keep internal | not a standalone public member; chained after the `value` set so the visible handle moves; exposing it alone has no proven typed use case |
| `created`, `renderedTicks`, `renderingTicks`, `tooltipChange` events | none | exclude for the current rows | builder-owned render/tick/tooltip hooks (`public-api-surface.json` marks each `builder.covered = true`); no focused Senior Living runtime use case |
| `colorRange`, `cssClass`, `customValues`, `enableAnimation`, `enabled`, `enableHtmlSanitizer`, `limits`, `max`, `min`, `orientation`, `showButtons`, `step`, `ticks`, `tooltip`, `type`, `width` | none | exclude as builder-owned | `discovery/public-api-surface.json` marks each `builder.covered = true`; configured on `SliderBuilder` at initial render, no post-render read/write proven necessary |
| `initialTooltip`, `readonly` properties | none | exclude | `discovery/parity-accounting.json` records each with a source-grounded reason (`slider.d.ts:356` undocumented tooltip-init flag, `slider.d.ts:413` read-only render mode) |
| `reposition()`, `setTooltip()` methods | none | exclude | `discovery/parity-accounting.json` records each with a source-grounded reason (`slider.d.ts:653` layout-recovery, `slider.d.ts:713` tooltip text); imperative helpers with no visible domain outcome a plan asserts |
| `destroy()` method | none | exclude as lifecycle | `discovery/public-api-surface.json` classifies it `skip: lifecycle cleanup`, not plan behavior |

## Blazor Naming Rule

Blazor metadata may be used only as naming evidence after the EJ2 row is proven.
`discovery/blazor-candidates.md` records that no Syncfusion Blazor package was
supplied for this pass, so every accepted C# name above comes from the EJ2
source and the raw core trace, not from Blazor metadata.

## Discovery Versus C# DSL Boundary

All observed fields stay in discovery. Only fields with clear, typed,
predictable Fusion use cases are accepted into the public C# event args. The
`change`/`changed` payload is accepted property by property (`Value`,
`PreviousValue`, `Text`, `Action`, `IsInteracted`). The builder-covered
properties, the render/tick/tooltip event hooks, the two undocumented/read-only
properties, and the two imperative methods (`reposition`, `setTooltip`) remain
discovered but excluded with source-grounded reasons because the Syncfusion MVC
builder owns initial render configuration and no post-render read/write/call is
proven necessary for a Senior Living preferences workflow.

## Implementation Boundary

Implemented public surface for the Slider slice:

- the `FusionSlider(...)` render helper bound to a numeric model property;
- the `Change` and `Changed` event selectors and `FusionSliderChangeArgs` payload with `Value`, `PreviousValue`, `Text`, `Action`, and `IsInteracted`;
- the `SetValue(double)` and `SetRangeValue(double, double)` writes (property set plus `dataBind` repaint);
- the `Value()` and `RangeValue()` read sources.

Out of scope for the Slider slice: new primitives, builder-owned static
properties, render/tick/tooltip event hooks, the two excluded properties, and the
`reposition`/`setTooltip`/`destroy` methods.
