# Breadcrumb C# Name Decisions

Status: active and proven. The `FusionBreadcrumb` public C# names are decided and
implemented: the `FusionBreadcrumb(...)` render helper, the `ItemClick` event
selector with the `FusionBreadcrumbItemClickArgs` payload (`Item` ->
`FusionBreadcrumbItem` with `Text`, `Id`, `Url`, `IconCss`, `Disabled`), the
`ActiveItem()` read source, and the `SetActiveItem(string)` write. The component is
fully audited.

## Pass Rows

Close matrix row: `Html.FusionBreadcrumb(plan, id, b => ...)` render helper -> Breadcrumb trail carrying the controlled component id.

Close matrix row: `breadcrumb.Reactive(e => e.ItemClick, ...)` -> typed `FusionBreadcrumbItemClickArgs` payload with the nested `FusionBreadcrumbItem`.

Close matrix row: `ActiveItem()`, `SetActiveItem(string)` -> typed Breadcrumb runtime members.

## Evidence Inputs

- Raw core trace: `traces/raw-ej2-core.trace.json`
- Raw core probe: `probes/raw-ej2-core.html`
- Syncfusion source types: `BreadcrumbClickEventArgs` (event), `BreadcrumbItemModel` (item), `Breadcrumb` (component)
- Public API surface: `discovery/public-api-surface.json`
- Event payload surface: `discovery/event-payload-surface.json`
- Blazor candidates: `discovery/blazor-candidates.md` (no Blazor package supplied; naming taken from EJ2 source only)
- Existing C# event args: `Alis.Reactive.Fusion/Components/FusionBreadcrumb/Events/FusionBreadcrumbOnItemClick.cs`
- Existing event selector: `Alis.Reactive.Fusion/Components/FusionBreadcrumb/FusionBreadcrumbEvents.cs`
- Existing component members: `Alis.Reactive.Fusion/Components/FusionBreadcrumb/FusionBreadcrumbExtensions.cs`
- Existing render helper: `Alis.Reactive.Fusion/Components/FusionBreadcrumb/FusionBreadcrumbHtmlExtensions.cs`
- Existing sandbox usage: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Breadcrumb/Index.cshtml`

## Name Decision Matrix

| Syncfusion path | C# name | Decision | Reason |
| --- | --- | --- | --- |
| `new ej.navigations.Breadcrumb(options)` render | `IHtmlHelper<TModel>.FusionBreadcrumb(ReactivePlan<TModel>, string elementId, Action<BreadcrumbBuilder> build)` | keep | the render helper renders the EJ2 Breadcrumb and carries its controlled component id into the plan; initial options stay on `BreadcrumbBuilder` |
| `itemClick` event | `FusionBreadcrumbEvents.ItemClick` | keep | exact Syncfusion event name; selected through the typed `.Reactive(e => e.ItemClick, ...)` event lambda |
| `BreadcrumbClickEventArgs` | `FusionBreadcrumbItemClickArgs` | keep | the Fusion payload type name states the event it belongs to; it carries only the proven, narrowed members |
| `BreadcrumbClickEventArgs.item` (`BreadcrumbItemModel`) | `FusionBreadcrumbItemClickArgs.Item` (`FusionBreadcrumbItem`) | keep | the clicked crumb is the central payload; narrowed to a typed item with only the proven scalar fields |
| `item.text` | `FusionBreadcrumbItem.Text` | keep | exact Syncfusion key, typed as `string`; the clicked crumb's label |
| `item.id` | `FusionBreadcrumbItem.Id` | keep | exact Syncfusion key, typed as `string`; the clicked crumb's id |
| `item.url` | `FusionBreadcrumbItem.Url` | keep | exact Syncfusion key, typed as `string`; the clicked crumb's url |
| `item.iconCss` | `FusionBreadcrumbItem.IconCss` | keep | exact Syncfusion key, typed as nullable `string`; the clicked crumb's icon classes (a crumb may omit an icon) |
| `item.disabled` | `FusionBreadcrumbItem.Disabled` | keep | exact Syncfusion key, typed as `bool`; the clicked crumb's disabled flag, gathered into the request body |
| `itemClick.element` | none | exclude from public typed payload | browser-owned DOM `HTMLElement`; exposing it as `object`/`dynamic` would pollute the public DSL (see `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them`) |
| `itemClick.event` | none | exclude from public typed payload | browser-owned DOM `Event`; same DOM-payload exclusion |
| `itemClick.cancel` | none | exclude for this row | cancel-the-default-navigation hook; `enableNavigation=false` already governs navigation and no focused Senior Living use case requires cancelling a crumb click |
| `itemClick.name` (inherited `BaseEventArgs.name`) | none | exclude for this row | duplicate event identity metadata; the `ItemClick` selector already owns the event identity |
| `activeItem` property read | `ActiveItem(this ComponentRef<FusionBreadcrumb, TModel> self)` | keep | concise read name returns a typed `string` source for conditions/set text; reads the url/text of the current crumb |
| `activeItem` property write | `SetActiveItem(this ComponentRef<FusionBreadcrumb, TModel> self, string activeItem)` | keep | states developer intent ("set the active crumb"); maps to an `activeItem` property set plus a `dataBind()` repaint, not raw member strings |
| `dataBind()` method | none (internal repaint companion of `SetActiveItem`) | keep internal | not a standalone public member; chained after the `activeItem` set so the visible trail repaints; exposing it alone has no proven typed use case |
| `beforeItemRender`, `created` events | none | exclude for the current rows | `beforeItemRender` carries a browser-owned `element`; `created` is a DOM-native lifecycle event with no typed payload; no focused Senior Living use case |
| `cssClass`, `disabled`, `enableActiveItemNavigation`, `enableNavigation`, `items`, `itemTemplate`, `maxItems`, `overflowMode`, `separatorTemplate`, `url` | none | exclude as builder-owned | `discovery/public-api-surface.json` marks each `builder.covered = true`; configured on `BreadcrumbBuilder` at initial render, no post-render read/write proven necessary |
| `locale` property | none | exclude as vendor-private | `breadcrumb.d.ts:207-210` marks it `@private @aspIgnore`; an internal globalization hook, not a public runtime value (`discovery/parity-accounting.json`) |
| `destroy()` method | none | exclude as lifecycle | `discovery/public-api-surface.json` classifies it `skip: lifecycle cleanup`, not plan behavior |

## Blazor Naming Rule

Blazor metadata may be used only as naming evidence after the EJ2 row is proven.
`discovery/blazor-candidates.md` records that no Syncfusion Blazor package was
supplied for this pass, so every accepted C# name above comes from the EJ2 source
and the raw core trace, not from Blazor metadata.

## Discovery Versus C# DSL Boundary

All observed fields stay in discovery. Only fields with clear, typed, predictable
Fusion use cases are accepted into the public C# event args. `itemClick.element`
and `itemClick.event` remain discovered but excluded because they are browser-owned
DOM objects; exposing them as `object`/`dynamic` would pollute the public DSL.
`itemClick.cancel` is discovered but excluded for this row (no use case while
`enableNavigation=false`). The builder-covered properties remain discovered but
excluded because the Syncfusion MVC builder owns initial render configuration; the
one exception is `activeItem`, which additionally onboards a proven post-render read
(`ActiveItem`) and write (`SetActiveItem`). `locale` is discovered but excluded as a
vendor-private member.

## Implementation Boundary

Implemented public surface for the Breadcrumb slice:

- the `FusionBreadcrumb(...)` render helper carrying the controlled component id;
- the `ItemClick` event selector and `FusionBreadcrumbItemClickArgs` payload with the nested `FusionBreadcrumbItem` (`Text`, `Id`, `Url`, `IconCss`, `Disabled`);
- the `ActiveItem()` read source;
- the `SetActiveItem(string)` write (property set plus `dataBind` repaint).

Out of scope for the Breadcrumb slice: new primitives, builder-owned static
properties, the `beforeItemRender`/`created` events, the browser-owned
`element`/`event` payload objects, the `cancel`/`name` payload metadata, the
vendor-private `locale`, and the lifecycle `destroy` method.
