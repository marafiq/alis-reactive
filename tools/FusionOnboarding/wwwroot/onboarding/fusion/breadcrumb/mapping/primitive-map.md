# Breadcrumb Primitive Map

Status: active and proven. This file maps the onboarded `FusionBreadcrumb` runtime
surface: the `FusionBreadcrumb(...)` render helper, the `itemClick` event and its
typed payload (`FusionBreadcrumbItemClickArgs.Item` with `Text`, `Id`, `Url`,
`IconCss`, `Disabled`), the `activeItem` read source, and the `activeItem` write
(`SetActiveItem`) followed by `dataBind`. Every mapped row uses an existing DSL
primitive. The component is fully audited.

## Pass Rows

Close matrix row: `Html.FusionBreadcrumb(plan, id, b => ...)` -> Breadcrumb trail render carrying the controlled component id -> sync render plus initial items/activeItem bound by the Syncfusion `BreadcrumbBuilder`.

Close matrix row: `breadcrumb.Reactive(e => e.ItemClick, (args, p) => ...)` item-click trigger -> Breadcrumb `itemClick` payload (`item.text`, `item.id`, `item.url`, `item.iconCss`, `item.disabled`) -> sync component-event reaction reading the typed nested payload.

Close matrix row: `p.Component<FusionBreadcrumb>(id).ActiveItem()` -> Breadcrumb active-item read source -> sync component property read of `activeItem` consumed by conditions/set text.

Close matrix row: `p.Component<FusionBreadcrumb>(id).SetActiveItem(url)` -> typed Breadcrumb active-item write -> sync component property set on `activeItem` followed by a `dataBind` method call that repaints the trail and moves `aria-current`.

## DSL Source Requirements

- `Alis.Reactive.Fusion/Components/FusionBreadcrumb/FusionBreadcrumb.cs`
- `Alis.Reactive.Fusion/Components/FusionBreadcrumb/FusionBreadcrumbHtmlExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionBreadcrumb/FusionBreadcrumbExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionBreadcrumb/FusionBreadcrumbEvents.cs`
- `Alis.Reactive.Fusion/Components/FusionBreadcrumb/FusionBreadcrumbReactiveExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionBreadcrumb/Events/FusionBreadcrumbOnItemClick.cs`
- `Alis.Reactive/Components/Contracts/ComponentRef.cs`
- `Alis.Reactive/Components/Contracts/ComponentProperty.cs`
- `Alis.Reactive/Components/Contracts/ComponentMember.cs`
- `Alis.Reactive/Components/Onboarding/ComponentEventOnboarding.cs`
- `Alis.Reactive/PlanModel/Values/ValueExpression.cs`

Sync/async lane expectation: the breadcrumb trail render is sync; the `itemClick`
component-event trigger is sync; `ActiveItem()` read and `SetActiveItem(...)` write
(property set plus `dataBind`) are sync component actions. The Breadcrumb slice
introduces no async boundary. Async only appears when a developer composes a click
payload value into an HTTP `Post(...).Gather(...)` pipeline, which is the HTTP
primitive, not a Breadcrumb concern.

## Authoritative Primitive Rows

| Breadcrumb row member | Syncfusion evidence | DSL primitive | Domain/JSON term | Runtime behavior | Status |
| --- | --- | --- | --- | --- | --- |
| trail render helper | `traces/raw-ej2-core.trace.json` constructs `Breadcrumb({ items: [...] })`; the `ol > li` trail renders from `items` | `Html.FusionBreadcrumb(plan, id, Action<BreadcrumbBuilder>)` | render carries the component id into the plan; no plan node beyond the `Reactive(...)` wiring | runtime boots the plan and the Syncfusion builder renders the trail | accepted and proven |
| `itemClick` event trigger | core trace `prototype methods`/instance shows `itemClick`; `event-payload-surface.json` resolves `BreadcrumbClickEventArgs` | `TypedEvent<FusionBreadcrumbItemClickArgs>` selected by `.Reactive(...)` | `StartsWhen.ComponentEvent(componentId, "itemClick")` | runtime wires the Syncfusion object event and starts the reaction with event payload scope | accepted and proven |
| `itemClick.item` | `event-payload-surface.json` `BreadcrumbClickEventArgs.item: BreadcrumbItemModel` | event payload read (nested object) | `FusionBreadcrumbItemClickArgs.Item` -> nested `ValueExpression.ReadPayload(PayloadSource.Event(), "item.*")` | runtime reads `event.item` as the clicked crumb object | accepted and proven |
| `itemClick.item.text` | `breadcrumb-model.d.ts` `BreadcrumbItemModel.text?: string` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "item.text", Shape.String)` from `FusionBreadcrumbItem.Text` | runtime reads `event.item.text` into the opened-section heading | accepted and proven |
| `itemClick.item.id` | `breadcrumb-model.d.ts` `BreadcrumbItemModel.id?: string` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "item.id", Shape.String)` from `FusionBreadcrumbItem.Id` | runtime reads `event.item.id` into the gather body; server resolves the record code | accepted and proven |
| `itemClick.item.url` | `breadcrumb-model.d.ts` `BreadcrumbItemModel.url?: string` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "item.url", Shape.String)` from `FusionBreadcrumbItem.Url` | runtime reads `event.item.url` into the gather body; server resolves the section summary | accepted and proven |
| `itemClick.item.iconCss` | `breadcrumb-model.d.ts` `BreadcrumbItemModel.iconCss?: string` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "item.iconCss", Shape.String)` from `FusionBreadcrumbItem.IconCss` | runtime reads `event.item.iconCss` into the opened-section icon tag | accepted and proven |
| `itemClick.item.disabled` | `breadcrumb-model.d.ts` `BreadcrumbItemModel.disabled?: boolean` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "item.disabled", Shape.Boolean)` from `FusionBreadcrumbItem.Disabled` | runtime reads `event.item.disabled` into the gather body (`"disabled":false` for an open crumb) | accepted and proven |
| `activeItem` property read | core trace `prototype`/instance carries `activeItem`; reads the url/text of the current crumb | `ComponentProperty<string>.Named("activeItem")` + `self.Read(property)` | `ValueExpression.Read(ComponentObject, "activeItem", Shape.String)` from `FusionBreadcrumbExtensions.ActiveItem(...)` | runtime reads `breadcrumb.activeItem` into a typed string source consumed by a condition | accepted and proven |
| `activeItem` property write | core trace `prototype methods` includes `dataBind`; setting `activeItem` + `dataBind()` re-renders the trail and moves `aria-current` | `ComponentProperty<string>` + `self.EmitSet(property, ValueExpression.Literal(url))` then `EmitCall("dataBind")` | `SetReaction` targeting `activeItem`, then `CallReaction` for `dataBind` | runtime writes `breadcrumb.activeItem = literal` and calls `dataBind()` so the current crumb visibly moves | accepted and proven |
| `dataBind()` method | core trace `prototype methods` includes `dataBind` | `ComponentMethod.Named("dataBind")` + `self.EmitCall(method)` | `CallReaction` targeting component method `dataBind` | runtime invokes `breadcrumb.dataBind()` to flush the `activeItem` set to the DOM; chained after the set only | accepted as the repaint companion of the `activeItem` write |
| `itemClick.element` | `event-payload-surface.json` `BreadcrumbClickEventArgs.element: HTMLElement` | excluded browser-owned element | no public C# payload property | runtime must not serialize or expose this through broad typed event args | excluded; browser-owned DOM element, see `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them` |
| `itemClick.event` | `event-payload-surface.json` `BreadcrumbClickEventArgs.event: Event` | excluded browser-owned event object | no public C# payload property | runtime must not serialize or expose this through broad typed event args | excluded; browser-owned DOM event, see `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them` |
| `itemClick.cancel` | `event-payload-surface.json` `BreadcrumbClickEventArgs.cancel: boolean` | not accepted for the current rows | no public C# payload property | no runtime mapping for this row | excluded; cancel-the-default-navigation hook with no focused Senior Living use case (`enableNavigation=false` already governs navigation) |
| `itemClick.name` | `event-payload-surface.json` inherits `BaseEventArgs.name: string` | excluded duplicate event metadata | no public C# payload property | no runtime mapping for this row | excluded; the `ItemClick` selector already owns event identity |
| `beforeItemRender` event | `public-api-surface.json` `BreadcrumbBeforeItemRenderEventArgs` with `element`/`item`/`cancel` | not accepted for the current rows | no public C# event selector | no runtime mapping for this row | excluded; per-item render hook carries a browser-owned `element` and no focused typed use case |
| `created` event | `event-payload-surface.json` row marks it `dom-native` (`Event`) | not accepted for the current rows | no public C# event selector | no runtime mapping for this row | excluded; DOM-native lifecycle event with no typed payload |
| `activeItem`, `cssClass`, `disabled`, `enableActiveItemNavigation`, `enableNavigation`, `items`, `itemTemplate`, `maxItems`, `overflowMode`, `separatorTemplate`, `url` | `public-api-surface.json` marks each `builder.covered = true` | builder-owned static configuration | no runtime DSL member (except the accepted `activeItem` post-render read/write rows above) | initial render configured on `BreadcrumbBuilder` | excluded for initial render; `activeItem` additionally onboards the proven post-render read/write rows above |
| `locale` property | `breadcrumb.d.ts:207-210` marks it `@private @aspIgnore` | not a public runtime value | no runtime DSL member | runtime never reads it from a plan | excluded; vendor-private internal globalization hook (`discovery/parity-accounting.json`) |
| `destroy()` method | `public-api-surface.json` classifies it `skip: lifecycle cleanup` | not a Fusion plan behavior | no runtime DSL member | runtime never calls it from a plan | excluded; lifecycle cleanup, not plan behavior |

## Primitive Decision

No new primitive is needed for the mapped Breadcrumb rows. Current primitives
already cover every onboarded member:

- component event trigger (`itemClick`);
- nested event payload read (`item.text`, `item.id`, `item.url`, `item.iconCss`, `item.disabled`);
- component property read (`activeItem`);
- component property write from a literal (`activeItem`) followed by the `dataBind` repaint call.

Any future failure to read one of these accepted members is a
discovery/mapping/typed-contract problem first, never permission to add a
primitive.

## Code To Delete Or Simplify

None identified for the primitive layer. The slice keeps `SetActiveItem` paired
with `dataBind` rather than introducing a setter that silently repaints, so the
repaint is an explicit mapped row rather than hidden behavior.

## Behavior Proof Required Before Commit

The Breadcrumb rows are proven by
`tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Breadcrumb/WhenUsingFusionBreadcrumb.cs`
through `scripts/playwright.sh`, with each member tied to a fails-when-broken
assertion in `proof/behavioral-coverage.json`:

1. `FusionBreadcrumb(...)` render shows the full trail and the DomReady `ActiveItem()` read confirms the Care Plan is current;
2. `itemClick` fires the reaction and `item.text` becomes the opened-section heading;
3. `item.iconCss` tags the opened section with its record icon;
4. `item.url` resolves the section summary on the server;
5. `item.id` resolves the record code on the server;
6. `item.disabled` rides the gather body as `"disabled":false`;
7. `SetActiveItem(url)` moves the current crumb to the resident overview.
