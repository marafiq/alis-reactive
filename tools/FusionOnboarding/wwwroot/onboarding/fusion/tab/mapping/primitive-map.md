# Tab Primitive Map

Status: active and proven. This file maps the onboarded `FusionTab` runtime
surface: the `selected` event and its typed payload (`SelectedIndex`,
`PreviousIndex`, `IsSwiped`), the typed `Select` and `HideTab` method calls, the
`SetSelectedItem` property write, and the `FusionTab(...)` navigation render
helper. Tab is a non-input navigation component: it has no `Value()` read and no
`SetValue()`. Every mapped row uses an existing DSL primitive. The component is
fully audited.

## Pass Rows

Close matrix row: `Html.FusionTab(plan, elementId, b => ...)` -> Tab navigation render carrying its controlled component id into the plan -> sync component registration plus initial tab set, headers, and selected tab bound by the Syncfusion `TabBuilder`.

Close matrix row: `tab.Reactive(e => e.Selected, (args, p) => ...)` selected trigger -> Tab `selected` payload (`selectedIndex`, `previousIndex`, `isSwiped`) -> sync component-event reaction reading the typed payload.

Close matrix row: `p.Component<FusionTab>(id).Select(index)` -> typed Tab select method call -> sync `select` method call that activates the section at the given index.

Close matrix row: `p.Component<FusionTab>(id).HideTab(index, isHidden)` -> typed Tab hide/show method call -> sync `hideTab` method call that hides or restores the section header at the given index.

Close matrix row: `p.Component<FusionTab>(id).SetSelectedItem(index)` -> typed Tab selected-index write -> sync component property set on `selectedItem` that activates the section at the given index.

## DSL Source Requirements

- `Alis.Reactive.Fusion/Components/FusionTab/FusionTab.cs`
- `Alis.Reactive.Fusion/Components/FusionTab/FusionTabHtmlExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionTab/FusionTabExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionTab/FusionTabEvents.cs`
- `Alis.Reactive.Fusion/Components/FusionTab/FusionTabReactiveExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionTab/Events/FusionTabOnSelected.cs`
- `Alis.Reactive/Components/Contracts/ComponentRef.cs`
- `Alis.Reactive/Components/Contracts/ComponentProperty.cs`
- `Alis.Reactive/Components/Contracts/ComponentMethod.cs`
- `Alis.Reactive/Components/Onboarding/ComponentEventOnboarding.cs`
- `Alis.Reactive/PlanModel/Values/ValueExpression.cs`

Sync/async lane expectation: the tab navigation render is sync component
registration; the `selected` component-event trigger is sync; `Select`,
`HideTab`, and `SetSelectedItem` are sync component actions. The Tab slice
introduces no async boundary. Async only appears when a developer composes a
read payload member into an HTTP `Post(...).Gather(...)` pipeline, which is the
HTTP primitive, not a Tab concern.

## Authoritative Primitive Rows

| Tab row member | Syncfusion evidence | DSL primitive | Domain/JSON term | Runtime behavior | Status |
| --- | --- | --- | --- | --- | --- |
| `selected` event trigger | `traces/raw-ej2-core.trace.json` sequence 4 fires `selected` with `selectedIndex`/`previousIndex`/`isSwiped`; `event-payload-surface.json` resolves `SelectEventArgs` | `TypedEvent<FusionTabSelectedArgs>` selected by `.Reactive(...)` | `StartsWhen.ComponentEvent(componentId, "selected")` | runtime wires the Syncfusion object event and starts the reaction with event payload scope | accepted and proven |
| `selected.selectedIndex` | core trace sequence 4 `selectedIndex: 1` (number); `event-payload-surface.json` `SelectEventArgs.selectedIndex: number` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "selectedIndex", Shape.Number)` from `FusionTabSelectedArgs.SelectedIndex` | runtime reads `event.selectedIndex` (the newly active tab) into set text, condition, or gather | accepted and proven |
| `selected.previousIndex` | core trace sequence 4 `previousIndex: 0` (number); `event-payload-surface.json` `SelectEventArgs.previousIndex: number` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "previousIndex", Shape.Number)` from `FusionTabSelectedArgs.PreviousIndex` | runtime reads `event.previousIndex` (the tab just left) into visible text | accepted and proven |
| `selected.isSwiped` | core trace sequence 4 `isSwiped: false` (boolean); `event-payload-surface.json` `SelectEventArgs.isSwiped: boolean` | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "isSwiped", Shape.Boolean)` from `FusionTabSelectedArgs.IsSwiped` | runtime reads `event.isSwiped` to distinguish a click (false) from a swipe (true) | accepted and proven |
| `select(index)` method | core trace `prototype methods` includes `select`; raw EJ2 `select(1)` activated tab index 1 (the `selected` payload reports `selectedIndex: 1`) | `ComponentMethod.Named("select").WithArgs<int>()` + `self.EmitCall(method, [Literal(index)])` | `CallReaction` targeting component method `select` | runtime invokes `tab.select(index)` and the visible active section changes | accepted and proven |
| `hideTab(index, value)` method | core trace `prototype methods` includes `hideTab`; raw EJ2 `hideTab(2, true)` ran without error to hide that header | `ComponentMethod.Named("hideTab").WithArgs<int, bool>()` + `self.EmitCall(method, [Literal(index), Literal(isHidden)])` | `CallReaction` targeting component method `hideTab` | runtime invokes `tab.hideTab(index, isHidden)` and the visible header is hidden or restored | accepted and proven |
| `selectedItem` property write | core trace `ready` constructs `Tab({ selectedItem: 0 })`; `selectedItem` is a number on the instance (`own keys`) | `ComponentProperty<int>.Named("selectedItem")` + `self.EmitSet(property, ValueExpression.Literal(index))` | `SetReaction` targeting component property `selectedItem` | runtime writes `tab.selectedItem = literal` and the activated section changes | accepted and proven |
| `selected.selectedItem` / `selected.previousItem` / `selected.selectedContent` | core trace sequence 4 carries each as `[Element#...]` DOM nodes | excluded browser-owned DOM objects | no public C# payload property | runtime must not serialize or expose these through broad typed event args | excluded; browser-owned DOM elements, see `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them` |
| `selected.preventFocus` | core trace sequence 4 `preventFocus: false`; a write-only focus-suppression flag | excluded write-only presentation flag | no public C# payload property | no runtime read mapping for this row | excluded; presentation-only writable flag with no resident-facing read use case |
| `selected.name` | `event-payload-surface.json` inherits `BaseEventArgs.name: string` | excluded duplicate event metadata | no public C# payload property | no runtime mapping for this row | excluded; the event selector already owns event identity |
| `selecting`, `added`, `adding`, `removed`, `removing`, `dragged`, `dragging`, `onDragStart`, `created`, `destroyed` events | `public-api-surface.json` lists each; core trace shows `selecting` carries a browser-owned `event`/DOM items, drag/add/remove require tab-collection mutation | not accepted for the current rows | no public C# event selector | no runtime mapping for this row | excluded; cancelable pre-events, collection-mutation events, drag events, and DOM-native lifecycle events carry browser-owned payloads or builder-authoring concerns, with no focused Senior Living navigation use case |
| `addTab`, `removeTab`, `enableTab`, `disable`, `getItemIndex`, `refresh`, `refreshActiveTab`, `refreshActiveTabBorder`, `refreshOverflow` methods | `discovery/parity-accounting.json` records each with a source-grounded exclusion reason | builder-authoring or vendor layout housekeeping | no runtime DSL member | runtime never calls these from a plan | excluded; collection mutation is builder-authoring and the refresh family is vendor layout housekeeping (see `discovery/parity-accounting.json`) |
| `tabId` property | `public-api-surface.json` marks it a runtime-source candidate; `discovery/parity-accounting.json` excludes it | excluded redundant identity | no runtime DSL member | runtime never reads it from a plan | excluded; equals the `elementId` the developer already passes to `Html.FusionTab(plan, elementId, ...)` |
| `allowDragAndDrop`, `animation`, `cssClass`, `headerPlacement`, `height`, `heightAdjustMode`, `items`, `loadOn`, `overflowMode`, `reorderActiveTab`, `scrollStep`, `showCloseButton`, `swipeMode`, `width`, and the remaining static settings | `public-api-surface.json` marks each `builder.covered = true` | builder-owned static configuration | no runtime DSL member | initial render configured on `TabBuilder`; no post-render read/write proven necessary | excluded; builder-owned per `references/automation-gates.md` Gate 5 builder-owned exclusion |
| `destroy()` method | `public-api-surface.json` classifies it `skip: lifecycle cleanup` | not a Fusion plan behavior | no runtime DSL member | runtime never calls it from a plan | excluded; lifecycle cleanup, not plan behavior |

## Primitive Decision

No new primitive is needed for the mapped Tab rows. Current primitives already
cover every onboarded member:

- component event trigger (`selected`);
- event payload read (`selectedIndex`, `previousIndex`, `isSwiped`);
- component method call (`select`, `hideTab`);
- component property write from a literal (`selectedItem`);
- component navigation render helper (`FusionTab`).

Any future failure to read one of these accepted members is a
discovery/mapping/typed-contract problem first, never permission to add a
primitive.

## Code To Delete Or Simplify

None identified for the primitive layer. Tab keeps `SetSelectedItem` as an
explicit `selectedItem` property write distinct from the `Select` method call,
so each navigation path is a mapped row rather than a hidden alias.

## Behavior Proof Required Before Commit

The Tab rows are proven by `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Tab/WhenTabSwitches.cs`
through `scripts/playwright.sh`, with each member tied to a fails-when-broken
assertion in `proof/behavioral-coverage.json`:

1. `FusionTab(...)` render opens the care workspace on the Care Schedule section;
2. `selected` fires the reaction and the active-section line plus the section content change together;
3. `selectedIndex` carries the newly active tab index;
4. `previousIndex` records the section the coordinator came from;
5. `isSwiped` marks a click as a deliberate selection rather than a swipe;
6. `Select(index)` jumps straight to the Incident Reports section;
7. `HideTab(index, isHidden)` hides Billing from the workspace and restores it;
8. `SetSelectedItem(index)` resumes the coordinator on the Medications section.
