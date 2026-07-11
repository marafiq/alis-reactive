# Accordion C# Name Decisions

Status: active and proven. The `FusionAccordion` public C# names are decided and
implemented: the `FusionAccordion(...)` render helper, the `Expanded` event selector
with the `FusionAccordionExpandedArgs` payload (`Index`, `IsExpanded`), the
`ExpandItem(bool, int)` method, and the `EnableItem(int, bool)` method.
`FusionAccordion` is a non-input container component, so there is no value member,
`SetValue`, or `Value` read. The component is fully audited.

## Pass Rows

Close matrix row: `Html.FusionAccordion(plan, id, b => ...)` render helper -> accordion container with panels declared on the Syncfusion builder.

Close matrix row: `accordion.Reactive(e => e.Expanded, ...)` -> typed `FusionAccordionExpandedArgs` payload.

Close matrix row: `ExpandItem(bool, int)`, `EnableItem(int, bool)` -> typed Accordion runtime methods.

## Evidence Inputs

- Raw core trace: `traces/raw-ej2-core.trace.json`
- Raw core probe: `probes/raw-ej2-core.html`
- Syncfusion source types: `ExpandedEventArgs` (event), `Accordion` (component)
- Syncfusion d.ts: `node_modules/@syncfusion/ej2-navigations/src/accordion/accordion.d.ts`
- Public API surface: `discovery/public-api-surface.json`
- Event payload surface: `discovery/event-payload-surface.json`
- Blazor candidates: `discovery/blazor-candidates.md` (no Blazor package supplied; naming taken from EJ2 source only)
- Existing C# event args: `Alis.Reactive.Fusion/Components/FusionAccordion/Events/FusionAccordionOnExpanded.cs`
- Existing event selector: `Alis.Reactive.Fusion/Components/FusionAccordion/FusionAccordionEvents.cs`
- Existing component members: `Alis.Reactive.Fusion/Components/FusionAccordion/FusionAccordionExtensions.cs`
- Existing render helper: `Alis.Reactive.Fusion/Components/FusionAccordion/FusionAccordionHtmlExtensions.cs`
- Existing sandbox usage: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Accordion/Index.cshtml`

## Name Decision Matrix

| Syncfusion path | C# name | Decision | Reason |
| --- | --- | --- | --- |
| `new ej.navigations.Accordion(options)` render | `IHtmlHelper<TModel>.FusionAccordion(ReactivePlan<TModel> plan, string elementId, Action<AccordionBuilder> build)` | keep | renders the EJ2 Accordion with panels declared on `AccordionBuilder` and carries the developer-chosen container id into the plan for `.Reactive` wiring and method calls |
| `expanded` event | `FusionAccordionEvents.Expanded` | keep | exact Syncfusion event name; selected through the typed `.Reactive(e => e.Expanded, ...)` event lambda; fires after a panel expands or collapses |
| `ExpandedEventArgs` | `FusionAccordionExpandedArgs` | keep | the Fusion payload type name states the event it belongs to; it carries only the proven, narrowed scalar members |
| `expanded.index` | `FusionAccordionExpandedArgs.Index` | keep | exact Syncfusion key, typed as `int`; the zero-based index of the panel that expanded or collapsed |
| `expanded.isExpanded` | `FusionAccordionExpandedArgs.IsExpanded` | keep | exact Syncfusion key, typed as `bool`; true on expand, false on collapse |
| `expanded.content` | none | exclude from public typed payload | browser-owned `HTMLElement`; exposing it as `object`/`dynamic` would pollute the public DSL |
| `expanded.element` | none | exclude from public typed payload | browser-owned `HTMLElement`; same reason as `content` |
| `expanded.item` | none | exclude from public typed payload | vendor `AccordionItemModel`; panel composition is builder-owned, not a runtime payload value |
| `expanded.name` (inherited `BaseEventArgs.name`) | none | exclude for this row | duplicate event identity metadata; the `Expanded` selector already owns the event identity |
| `expandItem(isExpand, index)` method | `ExpandItem(this ComponentRef<FusionAccordion, TModel> self, bool isExpand, int index)` | keep | exact Syncfusion method name; a typed two-argument call that expands or collapses the addressed panel |
| `enableItem(index, isEnable)` method | `EnableItem(this ComponentRef<FusionAccordion, TModel> self, int index, bool isEnable = true)` | keep | exact Syncfusion method name; a typed two-argument call that enables or disables the addressed panel (adds/removes the `e-overlay` disabled state) |
| `expanding`, `clicked`, `created`, `destroyed` events | none | exclude for the current rows | `expanded` is the proven panel-state event; `expanding`/`clicked` carry a `cancel` flag with no focused Senior Living use case yet; `created`/`destroyed` are DOM-native lifecycle events |
| `addItem`, `removeItem` methods | none | exclude as builder-owned composition | their argument is a vendor `AccordionItemModel`/loose `Object`; the panel set is declared on `AccordionBuilder` (`items`), and no DSL primitive constructs a panel descriptor; see `discovery/parity-accounting.json` |
| `hideItem` method | none | exclude as duplicate | `hideItem(index, isHidden)` is a near-duplicate of `enableItem` availability control; the journey locks/unlocks sections through `EnableItem`; see `discovery/parity-accounting.json` |
| `select` method | none | exclude as focus-only | `select(index)` only sets keyboard focus to a header and returns void; no value to branch/gather/display; see `discovery/parity-accounting.json` |
| `animation`, `dataSource`, `enableHtmlSanitizer`, `expandedIndices`, `expandMode`, `headerTemplate`, `height`, `items`, `itemTemplate`, `width` | none | exclude as builder-owned | `discovery/public-api-surface.json` marks each `builder.covered = true`; configured on `AccordionBuilder` at initial render, no post-render read/write proven necessary |
| `destroy()` method | none | exclude as lifecycle | `discovery/public-api-surface.json` classifies it `skip: lifecycle cleanup`, not plan behavior |

## Blazor Naming Rule

Blazor metadata may be used only as naming evidence after the EJ2 row is proven.
`discovery/blazor-candidates.md` records that no Syncfusion Blazor package was supplied
for this pass, so every accepted C# name above comes from the EJ2 source d.ts and the
raw core trace, not from Blazor metadata.

## Discovery Versus C# DSL Boundary

All observed fields stay in discovery. Only fields with clear, typed, predictable
Fusion use cases are accepted into the public C# event args. `expanded.content`,
`expanded.element`, and `expanded.item` remain discovered but excluded because they are
browser-owned DOM elements or a vendor composition model; exposing them as
`object`/`dynamic` would pollute the public DSL. The 10 builder-covered properties
remain discovered but excluded because the Syncfusion MVC builder owns initial render
configuration and no post-render read/write is proven necessary. The four structural or
focus methods (`addItem`, `removeItem`, `hideItem`, `select`) are excluded with
source-grounded reasons in `discovery/parity-accounting.json`.

## Implementation Boundary

Implemented public surface for the Accordion slice:

- the `FusionAccordion(...)` render helper carrying the controlled container id;
- the `Expanded` event selector and `FusionAccordionExpandedArgs` payload with `Index` and `IsExpanded`;
- the `ExpandItem(bool, int)` method;
- the `EnableItem(int, bool)` method.

Out of scope for the Accordion slice: new primitives, builder-owned static properties,
the `expanding`/`clicked`/`created`/`destroyed` events, the structural `addItem`/`removeItem`
methods, the `hideItem`/`select` methods, and the lifecycle `destroy` method.
