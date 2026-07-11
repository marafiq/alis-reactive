# Tab C# Name Decisions

Status: active and proven. The `FusionTab` public C# names are decided and
implemented: the `FusionTab(...)` navigation render helper, the `Selected` event
selector with the `FusionTabSelectedArgs` payload (`SelectedIndex`,
`PreviousIndex`, `IsSwiped`), the `Select(int)` and `HideTab(int, bool)` method
calls, and the `SetSelectedItem(int)` property write. The component is fully
audited.

## Pass Rows

Close matrix row: `Html.FusionTab(plan, elementId, b => ...)` render helper -> Tab navigation surface carrying its controlled component id.

Close matrix row: `tab.Reactive(e => e.Selected, ...)` -> typed `FusionTabSelectedArgs` payload.

Close matrix row: `Select(int)`, `HideTab(int, bool)`, `SetSelectedItem(int)` -> typed Tab runtime members.

## Evidence Inputs

- Raw core trace: `traces/raw-ej2-core.trace.json`
- Raw core probe: `probes/raw-ej2-core.html`
- Syncfusion source type: `SelectEventArgs` (event), `Tab` (component)
- Public API surface: `discovery/public-api-surface.json`
- Event payload surface: `discovery/event-payload-surface.json`
- Parity accounting (onboarded versus excluded members): `discovery/parity-accounting.json`
- Blazor candidates: `discovery/blazor-candidates.md` (no Blazor package supplied; naming taken from EJ2 source only)
- Existing C# event args: `Alis.Reactive.Fusion/Components/FusionTab/Events/FusionTabOnSelected.cs`
- Existing event selector: `Alis.Reactive.Fusion/Components/FusionTab/FusionTabEvents.cs`
- Existing component members: `Alis.Reactive.Fusion/Components/FusionTab/FusionTabExtensions.cs`
- Existing render helper: `Alis.Reactive.Fusion/Components/FusionTab/FusionTabHtmlExtensions.cs`
- Existing sandbox usage: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Tab/Index.cshtml`

## Name Decision Matrix

| Syncfusion path | C# name | Decision | Reason |
| --- | --- | --- | --- |
| `new ej.navigations.Tab(options)` navigation render | `IHtmlHelper<TModel>.FusionTab(ReactivePlan<TModel> plan, string elementId, Action<TabBuilder> build)` | keep | Tab is a navigation surface, not a model-bound input; the render helper carries the controlled `elementId` into the plan and leaves initial items/headers/selection on `TabBuilder` |
| `selected` event | `FusionTabEvents.Selected` | keep | exact Syncfusion event name; selected through the typed `.Reactive(e => e.Selected, ...)` event lambda |
| `SelectEventArgs` | `FusionTabSelectedArgs` | keep | the Fusion payload type name states the event it belongs to; it carries only the proven, narrowed primitive members |
| `selected.selectedIndex` | `FusionTabSelectedArgs.SelectedIndex` | keep | exact Syncfusion key, typed as `int`; the newly active tab |
| `selected.previousIndex` | `FusionTabSelectedArgs.PreviousIndex` | keep | exact Syncfusion key, typed as `int`; the tab before the change |
| `selected.isSwiped` | `FusionTabSelectedArgs.IsSwiped` | keep | exact Syncfusion key, typed as `bool`; distinguishes a click from a swipe |
| `selected.selectedItem`, `selected.previousItem`, `selected.selectedContent` | none | exclude from public typed payload | browser-owned DOM `HTMLElement` nodes (core trace shows each as `[Element#...]`); exposing them as `object`/`dynamic` would pollute the public DSL and serialize as `[object Object]` (see `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them`) |
| `selected.preventFocus` | none | exclude for this row | write-only focus-suppression flag; no resident-facing read use case |
| `selected.name` (inherited `BaseEventArgs.name`) | none | exclude for this row | duplicate event identity metadata; the `Selected` selector already owns the event identity |
| `select(args, event?)` method | `Select(this ComponentRef<FusionTab, TModel> self, int index)` | keep | exact Syncfusion method name; the typed overload narrows `number | HTEle` to the authored zero-based `int` index, dropping the browser `event` argument |
| `hideTab(index, value?)` method | `HideTab(this ComponentRef<FusionTab, TModel> self, int index, bool isHidden = true)` | keep | exact Syncfusion method name; `isHidden` names the `value` argument's intent and defaults to hiding |
| `selectedItem` property write | `SetSelectedItem(this ComponentRef<FusionTab, TModel> self, int index)` | keep | states developer intent ("set the selected tab"); maps to a `selectedItem` property set, not a raw member string |
| `selecting`, `added`, `adding`, `removed`, `removing`, `dragged`, `dragging`, `onDragStart`, `created`, `destroyed` events | none | exclude for the current rows | cancelable pre-events and collection-mutation/drag events carry browser-owned payloads or builder-authoring concerns; `created`/`destroyed` are DOM-native lifecycle events; no focused Senior Living navigation use case |
| `addTab`, `removeTab`, `enableTab`, `disable`, `getItemIndex`, `refresh`, `refreshActiveTab`, `refreshActiveTabBorder`, `refreshOverflow` methods | none | exclude as builder-authoring or vendor housekeeping | `discovery/parity-accounting.json` records a source-grounded reason for each: collection mutation is builder-authoring; the refresh family is vendor layout housekeeping; `getItemIndex` is a redundant id-to-index lookup |
| `tabId` property | none | exclude as redundant identity | equals the `elementId` the developer already passes to `Html.FusionTab(plan, elementId, ...)` (`discovery/parity-accounting.json`) |
| `allowDragAndDrop`, `animation`, `cssClass`, `headerPlacement`, `height`, `heightAdjustMode`, `items`, `loadOn`, `overflowMode`, `reorderActiveTab`, `scrollStep`, `showCloseButton`, `swipeMode`, `width`, and the remaining static settings | none | exclude as builder-owned | `discovery/public-api-surface.json` marks each `builder.covered = true`; configured on `TabBuilder` at initial render, no post-render read/write proven necessary |
| `destroy()` method | none | exclude as lifecycle | `discovery/public-api-surface.json` classifies it `skip: lifecycle cleanup`, not plan behavior |

## Blazor Naming Rule

Blazor metadata may be used only as naming evidence after the EJ2 row is proven.
`discovery/blazor-candidates.md` records that no Syncfusion Blazor package was
supplied for this pass, so every accepted C# name above comes from the EJ2
source and the raw core trace, not from Blazor metadata.

## Discovery Versus C# DSL Boundary

All observed fields stay in discovery. Only fields with clear, typed,
predictable Fusion use cases are accepted into the public C# event args. The
DOM-element payload members (`selectedItem`, `previousItem`, `selectedContent`)
remain discovered but excluded because they are browser-owned `HTMLElement`
nodes; exposing them as `object` or `dynamic` would pollute the public DSL. The
builder-covered properties and the cancelable/drag/lifecycle events remain
discovered but excluded because the Syncfusion MVC builder owns initial render
configuration and no post-render read/write is proven necessary for them.

## Implementation Boundary

Implemented public surface for the Tab slice:

- the `FusionTab(...)` navigation render helper carrying the controlled `elementId`;
- the `Selected` event selector and `FusionTabSelectedArgs` payload with `SelectedIndex`, `PreviousIndex`, and `IsSwiped`;
- the `Select(int)` method call;
- the `HideTab(int, bool)` method call;
- the `SetSelectedItem(int)` property write.

Out of scope for the Tab slice: new primitives, builder-owned static
properties, collection-mutation and drag events, the cancelable pre-events, the
vendor refresh/enable family, and the lifecycle `destroy` method.
