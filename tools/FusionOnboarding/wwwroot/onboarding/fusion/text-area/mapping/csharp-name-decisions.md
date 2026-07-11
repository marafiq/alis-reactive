# TextArea C# Name Decisions

Status: active and proven. The `FusionTextArea` public C# names are decided and
implemented: the `FusionTextArea(...)` render helper, the four typed event
selectors (`Input`, `Changed`, `Focus`, `Blur`) with their narrowed payloads,
the typed `SetValue(string?)` write, the `FocusIn()` and `FocusOut()` methods,
and the `Value()` read source. The component is fully audited.

## Pass Rows

Close matrix row: `Html.InputField(m => m.CareNote).FusionTextArea(b => ...)` render helper -> TextArea field bound to a string model property.

Close matrix row: `textArea.Reactive(e => e.Input, ...)` -> typed `FusionTextAreaInputArgs` payload; `e.Changed` -> `FusionTextAreaChangeArgs`; `e.Focus` -> `FusionTextAreaFocusArgs`; `e.Blur` -> `FusionTextAreaBlurArgs`.

Close matrix row: `SetValue(string?)`, `FocusIn()`, `FocusOut()`, `Value()` -> typed TextArea runtime members.

## Evidence Inputs

- Raw core trace: `traces/raw-ej2-core.trace.json`
- Raw core probe: `probes/raw-ej2-core.html`
- Syncfusion source type: `InputEventArgs`, `ChangedEventArgs`, `FocusInEventArgs`, `FocusOutEventArgs` (events), `TextArea` (component)
- Public API surface: `discovery/public-api-surface.json`
- Event payload surface: `discovery/event-payload-surface.json`
- Blazor candidates: `discovery/blazor-candidates.md` (no Blazor package supplied; naming taken from EJ2 source only)
- Existing C# event args: `Alis.Reactive.Fusion/Components/FusionTextArea/Events/FusionTextAreaOnInput.cs`, `FusionTextAreaOnChanged.cs`, `FusionTextAreaOnFocus.cs`, `FusionTextAreaOnBlur.cs`
- Existing event selectors: `Alis.Reactive.Fusion/Components/FusionTextArea/FusionTextAreaEvents.cs`
- Existing component members: `Alis.Reactive.Fusion/Components/FusionTextArea/FusionTextAreaExtensions.cs`
- Existing render helper: `Alis.Reactive.Fusion/Components/FusionTextArea/FusionTextAreaHtmlExtensions.cs`
- Existing sandbox usage: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/TextArea/Index.cshtml`

## Name Decision Matrix

| Syncfusion path | C# name | Decision | Reason |
| --- | --- | --- | --- |
| `new ej.inputs.TextArea(options)` field render | `InputBoundField<TModel, TProp>.FusionTextArea(Action<TextAreaBuilder> build)` | keep | the render helper binds the EJ2 TextArea to a string model property through the standard `Html.InputField` field wrapper; initial options stay on `TextAreaBuilder` |
| `input` event | `FusionTextAreaEvents.Input` | keep | exact Syncfusion event name; selected through the typed `.Reactive(e => e.Input, ...)` event lambda |
| `change` event | `FusionTextAreaEvents.Changed` | keep | past-tense Fusion selector for the Syncfusion `change` event; the underlying event key stays `"change"`, matching the framework's committed-value naming |
| `focus` event | `FusionTextAreaEvents.Focus` | keep | exact Syncfusion event name; the focus-arrived event |
| `blur` event | `FusionTextAreaEvents.Blur` | keep | exact Syncfusion event name; the focus-left event |
| `InputEventArgs` | `FusionTextAreaInputArgs` | keep | the Fusion payload type name states the event it belongs to; it carries only the proven, narrowed members |
| `ChangedEventArgs` | `FusionTextAreaChangeArgs` | keep | the Fusion payload type name states the event it belongs to; it carries only the proven, narrowed members |
| `FocusInEventArgs` | `FusionTextAreaFocusArgs` | keep | the Fusion payload type name states the event it belongs to; it carries only the proven, narrowed members |
| `FocusOutEventArgs` | `FusionTextAreaBlurArgs` | keep | the Fusion payload type name states the event it belongs to; it carries only the proven, narrowed members |
| `input.value`, `change.value`, `focus.value`, `blur.value` | `Fusion...Args.Value` | keep | exact Syncfusion key, typed as `string`; the text the event carries |
| `input.previousValue`, `change.previousValue` | `Fusion...Args.PreviousValue` | keep | exact Syncfusion key, typed as `string`; the text before the change |
| `change.isInteracted` | `FusionTextAreaChangeArgs.IsInteracted` | keep | exact Syncfusion key, typed as `bool`; distinguishes a hand edit from a programmatic change |
| `change.isInteraction` | none | exclude for this row | deprecated misspelled alias of `isInteracted`; exposing both would duplicate the same flag |
| `*.container` (`HTMLElement`) | none | exclude from public typed payload | browser-owned DOM element; exposing it as `object`/`dynamic` would pollute the public DSL (see `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them`) |
| `*.event` (`Event`) | none | exclude from public typed payload | browser-owned DOM `Event`; exposing it as `object`/`dynamic` would pollute the public DSL (see `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them`) |
| `value` property write | `SetValue(this ComponentRef<FusionTextArea, TModel> self, string? value)` | keep | states developer intent ("set the textarea value"); maps to a `value` property set plus a `dataBind()` repaint, not raw member strings; nullable `string?` clears the field |
| `focusIn()` method | `FocusIn(this ComponentRef<FusionTextArea, TModel> self)` | keep | exact Syncfusion method name; moves focus into the textarea |
| `focusOut()` method | `FocusOut(this ComponentRef<FusionTextArea, TModel> self)` | keep | exact Syncfusion method name; removes focus from the textarea |
| `value` property read | `Value(this ComponentRef<FusionTextArea, TModel> self)` | keep | concise read name returns a typed `string` source for gather/conditions/set text |
| `dataBind()` method | none (internal repaint companion of `SetValue`) | keep internal | not a standalone public member; chained after the `value` set so the visible text updates; exposing it alone has no proven typed use case |
| `created`, `destroyed` events | none | exclude for the current rows | DOM-native lifecycle events typed `Object` with no narrowed payload; no focused Senior Living use case |
| `addAttributes`, `removeAttributes`, `readonly` | none | exclude for the current rows | raw attribute and read-only manipulation; no focused Senior Living use case and the DSL does not express attribute bags |
| `adornmentFlow`, `adornmentOrientation`, `appendTemplate`, `cols`, `cssClass`, `enabled`, `enablePersistence`, `floatLabelType`, `maxLength`, `placeholder`, `prependTemplate`, `resizeMode`, `rows`, `showClearButton`, `width` | none | exclude as builder-owned | `discovery/public-api-surface.json` marks each `builder.covered = true`; configured on `TextAreaBuilder` at initial render, no post-render read/write proven necessary |
| `destroy()` method | none | exclude as lifecycle | `discovery/public-api-surface.json` classifies it `skip: lifecycle cleanup`, not plan behavior |

## Blazor Naming Rule

Blazor metadata may be used only as naming evidence after the EJ2 row is proven.
`discovery/blazor-candidates.md` records that no Syncfusion Blazor package was
supplied for this pass, so every accepted C# name above comes from the EJ2
source and the raw core trace, not from Blazor metadata.

## Discovery Versus C# DSL Boundary

All observed fields stay in discovery. Only fields with clear, typed,
predictable Fusion use cases are accepted into the public C# event args. The
`*.event` and `*.container` payload fields remain discovered but excluded
because they are browser-owned DOM objects; exposing them as `object` or
`dynamic` would pollute the public DSL. The deprecated `isInteraction` alias
remains discovered but excluded in favor of `isInteracted`. The 15
builder-covered properties remain discovered but excluded because the Syncfusion
MVC builder owns initial render configuration and no post-render read/write is
proven necessary.

## Implementation Boundary

Implemented public surface for the TextArea slice:

- the `FusionTextArea(...)` render helper bound to a string model property;
- the `Input`, `Changed`, `Focus`, and `Blur` event selectors with their narrowed payload types;
- the `SetValue(string?)` write (property set plus `dataBind` repaint);
- the `FocusIn()` and `FocusOut()` focus methods;
- the `Value()` read source.

Out of scope for the TextArea slice: new primitives, builder-owned static
properties, raw attribute methods, the `readonly` toggle, the lifecycle
`created`/`destroyed` events, and the lifecycle `destroy` method.
