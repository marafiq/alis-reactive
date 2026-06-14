# Rating C# Name Decisions

Status: active and proven. The `FusionRating` public C# names are decided and
implemented: the `FusionRating(...)` render helper, the `ValueChanged` event
selector with the `FusionRatingValueChangedArgs` payload (`Value`,
`PreviousValue`, `IsInteracted`), the typed `SetValue(double)` write, the
`Reset()` method, and the `Value()` read source. The component is fully audited.

## Pass Rows

Close matrix row: `Html.InputField(m => m.Score).FusionRating(b => ...)` render helper -> Rating field bound to a numeric model property.

Close matrix row: `rating.Reactive(e => e.ValueChanged, ...)` -> typed `FusionRatingValueChangedArgs` payload.

Close matrix row: `SetValue(double)`, `Reset()`, `Value()` -> typed Rating runtime members.

## Evidence Inputs

- Raw core trace: `traces/raw-ej2-core.trace.json`
- Raw core probe: `probes/raw-ej2-core.html`
- Syncfusion source type: `RatingChangedEventArgs` (event), `Rating` (component)
- Public API surface: `discovery/public-api-surface.json`
- Event payload surface: `discovery/event-payload-surface.json`
- Blazor candidates: `discovery/blazor-candidates.md` (no Blazor package supplied; naming taken from EJ2 source only)
- Existing C# event args: `Alis.Reactive.Fusion/Components/FusionRating/Events/FusionRatingOnValueChanged.cs`
- Existing event selector: `Alis.Reactive.Fusion/Components/FusionRating/FusionRatingEvents.cs`
- Existing component members: `Alis.Reactive.Fusion/Components/FusionRating/FusionRatingExtensions.cs`
- Existing render helper: `Alis.Reactive.Fusion/Components/FusionRating/FusionRatingHtmlExtensions.cs`
- Existing sandbox usage: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Rating/Index.cshtml`

## Name Decision Matrix

| Syncfusion path | C# name | Decision | Reason |
| --- | --- | --- | --- |
| `new ej.inputs.Rating(options)` field render | `InputBoundField<TModel, double>.FusionRating(Action<RatingBuilder> build)` | keep | the render helper binds the EJ2 Rating to a numeric model property through the standard `Html.InputField` field wrapper; initial options stay on `RatingBuilder` |
| `valueChanged` event | `FusionRatingEvents.ValueChanged` | keep | exact Syncfusion event name; selected through the typed `.Reactive(e => e.ValueChanged, ...)` event lambda |
| `RatingChangedEventArgs` | `FusionRatingValueChangedArgs` | keep | the Fusion payload type name states the event it belongs to; it carries only the proven, narrowed members |
| `valueChanged.value` | `FusionRatingValueChangedArgs.Value` | keep | exact Syncfusion key, typed as `double`; the newly selected rating |
| `valueChanged.previousValue` | `FusionRatingValueChangedArgs.PreviousValue` | keep | exact Syncfusion key, typed as `double`; the rating before the change |
| `valueChanged.isInteracted` | `FusionRatingValueChangedArgs.IsInteracted` | keep | exact Syncfusion key, typed as `bool`; distinguishes a user choice from a programmatic change |
| `valueChanged.event` | none | exclude from public typed payload | browser-owned DOM `Event`; exposing it as `object`/`dynamic` would pollute the public DSL (see `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them`) |
| `valueChanged.name` (inherited `BaseEventArgs.name`) | none | exclude for this row | duplicate event identity metadata; the `ValueChanged` selector already owns the event identity |
| `value` property write | `SetValue(this ComponentRef<FusionRating, TModel> self, double value)` | keep | states developer intent ("set the rating value"); maps to a `value` property set plus a `dataBind()` repaint, not raw member strings |
| `reset()` method | `Reset(this ComponentRef<FusionRating, TModel> self)` | keep | exact Syncfusion method name; clears the rating to its minimum |
| `value` property read | `Value(this ComponentRef<FusionRating, TModel> self)` | keep | concise read name returns a typed `double` source for gather/conditions/set text |
| `dataBind()` method | none (internal repaint companion of `SetValue`) | keep internal | not a standalone public member; chained after the `value` set so the visible stars update; exposing it alone has no proven typed use case |
| `beforeItemRender`, `onItemHover`, `created` events | none | exclude for the current rows | per-item render and hover carry browser-owned `element`/`event` payloads; `created` is a DOM-native lifecycle event with no typed payload; no focused Senior Living use case |
| `allowReset`, `cssClass`, `disabled`, `enableAnimation`, `enableSingleSelection`, `itemsCount`, `labelPosition`, `min`, `precision`, `readOnly`, `showLabel`, `showTooltip`, `visible`, templates | none | exclude as builder-owned | `discovery/public-api-surface.json` marks each `builder.covered = true`; configured on `RatingBuilder` at initial render, no post-render read/write proven necessary |
| `destroy()` method | none | exclude as lifecycle | `discovery/public-api-surface.json` classifies it `skip: lifecycle cleanup`, not plan behavior |

## Blazor Naming Rule

Blazor metadata may be used only as naming evidence after the EJ2 row is proven.
`discovery/blazor-candidates.md` records that no Syncfusion Blazor package was
supplied for this pass, so every accepted C# name above comes from the EJ2
source and the raw core trace, not from Blazor metadata.

## Discovery Versus C# DSL Boundary

All observed fields stay in discovery. Only fields with clear, typed,
predictable Fusion use cases are accepted into the public C# event args.
`valueChanged.event` remains discovered but excluded because it is a browser-owned
DOM `Event`; exposing it as `object` or `dynamic` would pollute the public DSL.
The 18 builder-covered properties and the four template properties remain
discovered but excluded because the Syncfusion MVC builder owns initial render
configuration and no post-render read/write is proven necessary.

## Implementation Boundary

Implemented public surface for the Rating slice:

- the `FusionRating(...)` render helper bound to a numeric model property;
- the `ValueChanged` event selector and `FusionRatingValueChangedArgs` payload with `Value`, `PreviousValue`, and `IsInteracted`;
- the `SetValue(double)` write (property set plus `dataBind` repaint);
- the `Reset()` method;
- the `Value()` read source.

Out of scope for the Rating slice: new primitives, builder-owned static
properties, per-item render or hover events, and the lifecycle `destroy` method.
