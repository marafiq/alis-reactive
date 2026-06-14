# Toolbar C# Name Decisions

Status: active and proven. One row per public C# member of the `FusionToolbar`
slice, each grounded in raw EJ2 evidence and the Syncfusion source of record.
The component is fully audited.

## Accepted Public Members

| Public C# member | Syncfusion source name | Decision | Evidence |
| --- | --- | --- | --- |
| `Html.FusionToolbar(plan, id, build)` | `ToolbarBuilder` (MVC) | render helper named for the component; keeps initial options on the Syncfusion builder | `discovery/public-api-surface.json`, `discovery/mvc-builder-coverage.md` |
| `FusionToolbarEvents.Clicked` | `clicked` (`ClickEventArgs`) | the one accepted event; named `Clicked` to match the EJ2 event | `discovery/event-payload-surface.json`, probe `clicked` handler |
| `Reactive(...)` | n/a (Alis wiring) | event-to-plan wiring through `ComponentEventOnboarding.Wire`, consistent with every Fusion slice | `Alis.Reactive.Fusion/Components/FusionToolbar/FusionToolbarReactiveExtensions.cs` |
| `FusionToolbarClickedArgs.Item` | `ClickEventArgs.item` (`ItemModel`) | narrowed to a typed `FusionToolbarItem`, not the broad `ItemModel` | `discovery/event-payload-surface.json` `ClickEventArgs.item` |
| `FusionToolbarItem.Id` | `ItemModel.id` (`string`) | typed `string`; the clicked command identity | `discovery/event-payload-surface.json` `ItemModel.id` |
| `FusionToolbarItem.Text` | `ItemModel.text` (`string`) | typed `string`; the clicked command label | `discovery/event-payload-surface.json` `ItemModel.text` |
| `FusionToolbarItem.Disabled` | `ItemModel.disabled` (`boolean`) | typed `bool`; per-item disabled flag | `discovery/event-payload-surface.json` `ItemModel.disabled` |
| `Disable(this ComponentRef<FusionToolbar, TModel> self, bool value)` | `disable(value: boolean)` | typed `Disable(bool)`; named for the EJ2 method | `toolbar.d.ts:414`, `traces/raw-ej2-core.trace.json` `after-disable-true/false` |

## Excluded / Not Named As Public Members

| Candidate | Syncfusion source | Reason |
| --- | --- | --- |
| `ClickEventArgs.cancel` | toolbar.d.ts | writable pre-click cancel flag; no Senior Living command-bar use case and no typed writable-payload row authored |
| `ClickEventArgs.originalEvent` | toolbar.d.ts | browser-owned DOM `Event`; `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them` |
| `BaseEventArgs.name` | ej2-base | duplicate event identity; the `Clicked` selector owns it |
| `enableItems`, `hideItem` | toolbar.d.ts:485,541 | genuine per-item runtime behaviors, but onboarding them as typed DSL needs a NEW typed method on the framework slice; deferred per-item candidates recorded in `discovery/parity-accounting.json` |
| `addItems`, `removeItems` | toolbar.d.ts:494,502 | builder-owned item-collection mutation (`items` is `builder.covered = true`); see `discovery/parity-accounting.json` |
| `refreshOverflow` | toolbar.d.ts:449 | layout-internal reflow with no plan value; see `discovery/parity-accounting.json` |
| `changeOrientation` | toolbar.d.ts:434 (`@hidden`) | hidden Syncfusion member; orientation is builder-owned; see `discovery/parity-accounting.json` |
| `beforeCreate`, `created`, `destroyed`, `keyDown` | toolbar.d.ts | lifecycle/keyboard events; `created`/`destroyed` are DOM-native; no focused command-bar use case |
| builder-owned properties (`allowKeyboard`, `cssClass`, `enableCollision`, `enableHtmlSanitizer`, `height`, `items`, `overflowMode`, `scrollStep`, `width`) | toolbar.d.ts | `builder.covered = true`; initial render configuration on `ToolbarBuilder` |
| `destroy()` | toolbar.d.ts | `skip:` lifecycle cleanup, not plan behavior |

## Blazor Cross-Check

`discovery/blazor-candidates.md` records the Syncfusion Blazor `SfToolbar`
vocabulary. The accepted EJ2 names (`disable`, `clicked`, `item.id/text/disabled`)
are direct EJ2 overlaps proven in raw HTML; no bridge-computed Blazor-only member
was promoted into the slice.
