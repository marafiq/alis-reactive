# Toolbar Primitive Map

Status: active and proven. This file maps the onboarded `FusionToolbar` runtime
surface: the `clicked` event and its typed `Item` payload (`Id`, `Text`,
`Disabled`), the typed `Disable(bool)` method, and the `FusionToolbar(...)`
render helper. Every mapped row uses an existing DSL primitive. The component is
fully audited.

## Pass Rows

Close matrix row: `Html.FusionToolbar(plan, id, b => b.Items(...))` -> toolbar render carrying its controlled component id -> sync render of the Syncfusion `ToolbarBuilder` output plus plan wiring metadata.

Close matrix row: `toolbar.Reactive(e => e.Clicked, (args, p) => ...)` clicked trigger -> Toolbar `clicked` payload (`item.id`, `item.text`, `item.disabled`) -> sync component-event reaction reading the typed payload.

Close matrix row: `p.Component<FusionToolbar>(id).Disable(value)` -> typed Toolbar disable method -> sync component method call on `disable` that adds/removes `e-overlay` on the root.

## DSL Source Requirements

- `Alis.Reactive.Fusion/Components/FusionToolbar/FusionToolbar.cs`
- `Alis.Reactive.Fusion/Components/FusionToolbar/FusionToolbarHtmlExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionToolbar/FusionToolbarExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionToolbar/FusionToolbarEvents.cs`
- `Alis.Reactive.Fusion/Components/FusionToolbar/FusionToolbarReactiveExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionToolbar/Events/FusionToolbarClickedArgs.cs`
- `Alis.Reactive/Components/Contracts/ComponentRef.cs`
- `Alis.Reactive/Components/Contracts/ComponentMember.cs`
- `Alis.Reactive/Components/Onboarding/ComponentEventOnboarding.cs`
- `Alis.Reactive/PlanAuthoring/Events/TypedEvent.cs`
- `Alis.Reactive/PlanModel/Values/ValueExpression.cs`
- `Alis.Reactive/PlanAuthoring/Requests/GatherBuilder.cs`

Sync/async lane expectation: the toolbar render is sync; the `clicked`
component-event trigger is sync; `Disable` is a sync component method call. The
Toolbar slice introduces no async boundary of its own. Async appears only when a
developer composes a `clicked`-payload value into an HTTP `Post(...).Gather(...)`
pipeline (the Pay-balance journey), which is the HTTP primitive, not a Toolbar
concern.

## Authoritative Primitive Rows

| Toolbar row member | Syncfusion evidence | DSL primitive | Domain/JSON term | Runtime behavior | Status |
| --- | --- | --- | --- | --- | --- |
| toolbar render helper | `discovery/public-api-surface.json` builder `ToolbarBuilder`; `items` is `builder.covered = true` | `Html.FusionToolbar(plan, id, build)` rendering `ToolbarBuilder.Render()` | render of `<div id>` toolbar carrying the component id into the plan | runtime boots the rendered toolbar and discovers the plan that wires its event | accepted and proven |
| `disable(value)` method | `traces/raw-ej2-core.trace.json` `after-disable-true` shows `e-overlay` added to the root (`hasOverlay: true`), `after-disable-false` removes it; `ej2-navigations` toolbar.js `CLS_DISABLE = 'e-overlay'` | `ComponentMethod.Named("disable").WithArgs<bool>()` + `self.EmitCall(method, [ValueExpression.Literal(value)])` | `CallReaction` targeting component method `disable` with one boolean argument | runtime invokes `toolbar.disable(value)`; the root gains/loses `e-overlay` | accepted and proven |
| `clicked` event trigger | `discovery/event-payload-surface.json` `clicked: ClickEventArgs`; probe shell instantiates `new ej.navigations.Toolbar({ clicked })` | `TypedEvent<FusionToolbarClickedArgs>` selected by `.Reactive(...)` | `StartsWhen.ComponentEvent(componentId, "clicked")` | runtime wires the Syncfusion object event and starts the reaction with event-payload scope | accepted and proven |
| `clicked.item.id` | `discovery/event-payload-surface.json` `ClickEventArgs.item: ItemModel`; `ItemModel.id: string` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "item.id", Shape.String)` from `FusionToolbarItem.Id` | runtime reads `event.item.id` into a condition guard and the gather body | accepted and proven |
| `clicked.item.text` | `discovery/event-payload-surface.json` `ItemModel.text: string` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "item.text", Shape.String)` from `FusionToolbarItem.Text` | runtime reads `event.item.text` into set text and the gather body | accepted and proven |
| `clicked.item.disabled` | `discovery/event-payload-surface.json` `ItemModel.disabled: boolean` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "item.disabled", Shape.Boolean)` from `FusionToolbarItem.Disabled` | runtime reads `event.item.disabled` into the gather body; a trusted click only lands on an enabled item, so the reachable value is `false` | accepted and proven (gather body, per `_skill/pattern-map.md#p025-a-disabled-item-only-payload-member-is-proven-through-the-gather-body-not-a-disabled-item-click`) |
| `clicked.cancel` | `discovery/event-payload-surface.json` `ClickEventArgs.cancel: boolean` | excluded writable payload | no public C# payload member | no runtime mapping for this row | excluded; pre-click cancel flag with no Senior Living command-bar use case, and no typed writable-payload row authored for the toolbar |
| `clicked.originalEvent` | `discovery/event-payload-surface.json` `ClickEventArgs.originalEvent: Event` | excluded browser-owned event object | no public C# payload member | runtime must not expose this through broad typed event args | excluded; browser-owned DOM event, see `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them` |
| `clicked.name` | `discovery/event-payload-surface.json` inherits `BaseEventArgs.name: string` | excluded duplicate event metadata | no public C# payload member | no runtime mapping for this row | excluded; the `Clicked` selector already owns event identity |
| `disable`, `enableItems`, `hideItem`, `addItems`, `removeItems`, `refreshOverflow`, `changeOrientation` methods | `discovery/public-api-surface.json` (`builder.covered = false`); `traces/raw-ej2-core.trace.json` confirms `disable` (proven), `addItems`/`removeItems` mutate the item-id list, `changeOrientation` is `@hidden` in `toolbar.d.ts:434` | only `disable` is mapped to a typed primitive; the rest are accounted in `discovery/parity-accounting.json` | typed `Disable` only; the others have no public C# member | runtime calls only `disable` from a plan | `disable` accepted and proven; the other six accounted as builder-owned/layout-internal/hidden/deferred per `discovery/parity-accounting.json` |
| `beforeCreate`, `created`, `destroyed`, `keyDown` events | `discovery/event-payload-surface.json`: `created`/`destroyed` are dom-native; `beforeCreate`/`keyDown` carry lifecycle/keyboard payloads | not accepted for the current rows | no public C# event selector | no runtime mapping for these rows | excluded; lifecycle/keyboard events with no focused Senior Living command-bar use case, `created`/`destroyed` are DOM-native |
| `allowKeyboard`, `cssClass`, `enableCollision`, `enableHtmlSanitizer`, `height`, `items`, `overflowMode`, `scrollStep`, `width` | `discovery/public-api-surface.json` marks each `builder.covered = true` | builder-owned static configuration | no runtime DSL member | initial render configured on `ToolbarBuilder`; no post-render read/write proven necessary | excluded; builder-owned per `references/automation-gates.md` Gate 5 builder-owned exclusion |
| `destroy()` method | `discovery/public-api-surface.json` classifies it `skip: lifecycle cleanup` | not a Fusion plan behavior | no runtime DSL member | runtime never calls it from a plan | excluded; lifecycle cleanup, not plan behavior |

## Primitive Decision

No new primitive is needed for the mapped Toolbar rows. Current primitives
already cover every onboarded member:

- toolbar render helper (component render + plan wiring);
- component event trigger (`clicked`);
- event payload read (`item.id`, `item.text`, `item.disabled`);
- component method call with one boolean argument (`disable`).

Any future failure to read one of these accepted members is a
discovery/mapping/typed-contract problem first, never permission to add a
primitive. A per-item enable/hide capability (`enableItems`/`hideItem`) would be
a new typed method on the slice and is recorded as a deferred candidate in
`discovery/parity-accounting.json`, not a primitive gap.

## Code To Delete Or Simplify

None identified for the primitive layer. The slice keeps `Disable(bool)` as the
single proven runtime mutation and does not wrap builder-owned item configuration
as reactive methods.

## Behavior Proof Required Before Commit

The Toolbar rows are proven by
`tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Toolbar/WhenUsingFusionToolbar.cs`
through `scripts/playwright.sh`, with each member tied to a fails-when-broken
assertion in `proof/behavioral-coverage.json`:

1. `FusionToolbar(...)` render shows the account command bar with its three actions;
2. `clicked` fires the reaction and `Item.Text` names the started action in the status banner;
3. `Item.Id` routes Pay balance to the payment workflow rather than the status branch;
4. `Disable(true)` locks the command bar (`e-overlay`) and `Disable(false)` unlocks it;
5. `Item.Id`, `Item.Text`, and `Item.Disabled` ride the POST gather body under their declared keys.
