# Accordion Primitive Map

Status: active and proven. This file maps the onboarded `FusionAccordion` runtime
surface: the `expanded` event and its typed payload (`Index`, `IsExpanded`), the
two-argument `expandItem` and `enableItem` method calls, the render helper, and the
`.Reactive(...)` event wiring. `FusionAccordion` is a non-input container component:
it has no value to register, read, or write. Every mapped row uses an existing DSL
primitive. The component is fully audited.

## Pass Rows

Close matrix row: `Html.FusionAccordion(plan, "care-plan", b => ...)` -> accordion container render with the panel set declared on the Syncfusion builder -> sync render that carries the controlled component id into the plan.

Close matrix row: `accordion.Reactive(e => e.Expanded, (args, p) => ...)` expanded trigger -> Accordion `expanded` payload (`index`, `isExpanded`) -> sync component-event reaction reading the typed payload.

Close matrix row: `p.Component<FusionAccordion>(id).ExpandItem(isExpand, index)` -> typed two-argument accordion method call -> sync `expandItem(bool, number)` method call that expands or collapses the addressed panel.

Close matrix row: `p.Component<FusionAccordion>(id).EnableItem(index, isEnable)` -> typed two-argument accordion method call -> sync `enableItem(number, bool)` method call that adds or removes the disabled overlay on the addressed panel.

## DSL Source Requirements

- `Alis.Reactive.Fusion/Components/FusionAccordion/FusionAccordion.cs`
- `Alis.Reactive.Fusion/Components/FusionAccordion/FusionAccordionHtmlExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionAccordion/FusionAccordionExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionAccordion/FusionAccordionEvents.cs`
- `Alis.Reactive.Fusion/Components/FusionAccordion/FusionAccordionReactiveExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionAccordion/Events/FusionAccordionOnExpanded.cs`
- `Alis.Reactive/Components/Contracts/ComponentRef.cs`
- `Alis.Reactive/Components/Contracts/ComponentMethod.cs`
- `Alis.Reactive/Components/Onboarding/ComponentEventOnboarding.cs`
- `Alis.Reactive/PlanModel/Values/ValueExpression.cs`

Sync/async lane expectation: the accordion render is sync; the `expanded`
component-event trigger is sync; `ExpandItem` and `EnableItem` are sync component
method calls. The Accordion slice introduces no async boundary. Async only appears
when a developer composes the `expanded` event into an HTTP `Get(...).Response(...)`
pipeline (the lazy-load-on-expand journey), which is the HTTP primitive, not an
Accordion concern.

## Authoritative Primitive Rows

| Accordion row member | Syncfusion evidence | DSL primitive | Domain/JSON term | Runtime behavior | Status |
| --- | --- | --- | --- | --- | --- |
| accordion render helper | `traces/raw-ej2-core.trace.json` constructs `new ej.navigations.Accordion(...)`; `public-api-surface.json` `items` is `builder.covered = true` | `Html.FusionAccordion(plan, id, build)` carrying the controlled id; panels declared on `AccordionBuilder` | component render that registers the id for `.Reactive` wiring | runtime discovers the rendered accordion by id for event wiring and method calls | accepted and proven |
| `expanded` event trigger | core trace `prototype methods` includes `expanded`; `event-payload-surface.json` resolves `ExpandedEventArgs` | `TypedEvent<FusionAccordionExpandedArgs>` selected by `.Reactive(...)` | `StartsWhen.ComponentEvent(componentId, "expanded")` | runtime wires the Syncfusion object event and starts the reaction with event payload scope | accepted and proven |
| `expanded.index` | `event-payload-surface.json` `ExpandedEventArgs.index: number` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "index", Shape.Number)` from `FusionAccordionExpandedArgs.Index` | runtime reads `event.index` (which panel expanded/collapsed) into a condition branch | accepted and proven |
| `expanded.isExpanded` | `event-payload-surface.json` `ExpandedEventArgs.isExpanded: boolean` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "isExpanded", Shape.Boolean)` from `FusionAccordionExpandedArgs.IsExpanded` | runtime reads `event.isExpanded` to distinguish expand (true) from collapse (false) in a condition branch | accepted and proven |
| `expandItem(isExpand, index)` method | d.ts:435-441 `expandItem(isExpand: boolean, index?: number): void`; core trace `prototype methods` includes `expandItem` | `ComponentMethod.Named("expandItem").WithArgs<bool, int>()` + `self.EmitCall(method, [Literal(isExpand), Literal(index)])` | `CallReaction` targeting component method `expandItem` with two literal args | runtime invokes `accordion.expandItem(isExpand, index)` and the addressed panel expands/collapses | accepted and proven |
| `enableItem(index, isEnable)` method | d.ts:424-432 `enableItem(index: number, isEnable: boolean): void`; core trace `prototype methods` includes `enableItem`; `accordion.js:1086` adds/removes `e-overlay` | `ComponentMethod.Named("enableItem").WithArgs<int, bool>()` + `self.EmitCall(method, [Literal(index), Literal(isEnable)])` | `CallReaction` targeting component method `enableItem` with two literal args | runtime invokes `accordion.enableItem(index, isEnable)`; the addressed panel gains or loses its disabled overlay | accepted and proven |
| `expanded.content` | `event-payload-surface.json` `ExpandedEventArgs.content: HTMLElement` | excluded browser-owned DOM element | no public C# payload property | runtime must not serialize or expose this through broad typed event args | excluded; browser-owned DOM element, see `_skill/pattern-map.md` |
| `expanded.element` | `event-payload-surface.json` `ExpandedEventArgs.element: HTMLElement` | excluded browser-owned DOM element | no public C# payload property | no runtime mapping for this row | excluded; browser-owned DOM element |
| `expanded.item` | `event-payload-surface.json` `ExpandedEventArgs.item: AccordionItemModel` | excluded vendor model object | no public C# payload property | no runtime mapping for this row | excluded; vendor builder model, panel composition is builder-owned |
| `expanded.name` | `event-payload-surface.json` inherits `BaseEventArgs.name: string` | excluded duplicate event metadata | no public C# payload property | no runtime mapping for this row | excluded; the event selector already owns event identity |
| `expanding`, `clicked`, `created`, `destroyed` events | `public-api-surface.json` events; `created`/`destroyed` are DOM-native per `event-payload-surface.json` | not accepted for the current rows | no public C# event selector | no runtime mapping for these rows | excluded; `expanded` is the proven panel-state event; `expanding`/`clicked` carry a `cancel` flag with no focused Senior Living use case yet; `created`/`destroyed` are DOM-native lifecycle |
| `addItem`, `removeItem` methods | d.ts:391-407; argument is `AccordionItemModel`/loose `Object` | structural composition; no DSL primitive constructs a panel descriptor | no runtime DSL member | runtime never calls them from a plan | excluded; builder-owned panel composition, see `discovery/parity-accounting.json` |
| `hideItem` method | d.ts:415-423 `hideItem(index, isHidden?)` | duplicate of enable/disable availability control | no runtime DSL member | no runtime mapping for this row | excluded; near-duplicate of `enableItem`, see `discovery/parity-accounting.json` |
| `select` method | d.ts:408-414 `select(index)` "sets focus to the specified index item header" | browser focus management; no value source | no runtime DSL member | no runtime mapping for this row | excluded; DOM focus only, see `discovery/parity-accounting.json` |
| `animation`, `dataSource`, `enableHtmlSanitizer`, `expandedIndices`, `expandMode`, `headerTemplate`, `height`, `items`, `itemTemplate`, `width` | `public-api-surface.json` marks each `builder.covered = true` | builder-owned static configuration | no runtime DSL member | initial render configured on `AccordionBuilder`; no post-render read/write proven necessary | excluded; builder-owned per `references/automation-gates.md` Gate 5 builder-owned exclusion |
| `destroy()` method | `public-api-surface.json` classifies it `skip: lifecycle cleanup` | not a Fusion plan behavior | no runtime DSL member | runtime never calls it from a plan | excluded; lifecycle cleanup, not plan behavior |

## Primitive Decision

No new primitive is needed for the mapped Accordion rows. Current primitives already
cover every onboarded member:

- component event trigger (`expanded`);
- event payload read (`index`, `isExpanded`);
- two-argument component method call (`expandItem`, `enableItem`).

Any future failure to read or call one of these accepted members is a
discovery/mapping/typed-contract problem first, never permission to add a primitive.

## Code To Delete Or Simplify

None identified for the primitive layer. The slice keeps the two component methods as
distinct typed two-argument calls (`ComponentMethod.WithArgs<...>`), so each maps to a
single deterministic plan member with no overload ambiguity.

## Behavior Proof Required Before Commit

The Accordion rows are proven by
`tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Accordion/WhenUsingFusionAccordion.cs`
through `scripts/playwright.sh`, with each member tied to a fails-when-broken assertion
in `proof/behavioral-coverage.json`:

1. `FusionAccordion(...)` render shows the three care-plan sections (the locked one overlaid);
2. `expanded` fires the reaction and the page names the section the resident opened;
3. `index` routes the branch that names a different section when a different panel opens;
4. `isExpanded` routes the Else branch so collapsing a section shows no section open;
5. `ExpandItem(true, 0)` expands the care-team section from the summary button;
6. `EnableItem(2, true)` removes the disabled overlay so the locked section becomes readable.
