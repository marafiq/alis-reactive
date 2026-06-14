# Sidebar Primitive Map

Status: active and proven. This file maps the onboarded `FusionSidebar` runtime
surface: the `open` and `close` transition events with their typed
`FusionSidebarTransitionArgs.IsInteracted` payload, the `isOpen` read source,
and the `show`, `hide`, and `toggle` method calls. The `FusionSidebar(...)`
render helper carries the controlled component id into the plan. Every mapped
row uses an existing DSL primitive. The component is fully audited.

## Pass Rows

Close matrix row: `Html.FusionSidebar(plan, "id", b => ...)` -> Sidebar render that carries the controlled component id into the plan -> sync markup render plus component-id registration for `.Reactive(...)` wiring.

Close matrix row: `sidebar.Reactive(e => e.Opened, (args, p) => ...)` open trigger -> Sidebar `open` payload (`isInteracted`) -> sync component-event reaction reading the typed payload.

Close matrix row: `sidebar.Reactive(e => e.Closed, (args, p) => ...)` close trigger -> Sidebar `close` payload (`isInteracted`) -> sync component-event reaction reading the typed payload.

Close matrix row: `p.Component<FusionSidebar>(id).Show()` -> typed Sidebar show method call -> sync `show` method call that adds `e-open`, removes `e-close`, and sets `isOpen = true`.

Close matrix row: `p.Component<FusionSidebar>(id).Hide()` -> typed Sidebar hide method call -> sync `hide` method call that adds `e-close`, removes `e-open`, and sets `isOpen = false`.

Close matrix row: `p.Component<FusionSidebar>(id).Toggle()` -> typed Sidebar toggle method call -> sync `toggle` method call that flips the open state.

Close matrix row: `sidebar.IsOpen()` -> Sidebar open-state read source -> sync component property read of `isOpen` consumed by gather, conditions, or set text.

## DSL Source Requirements

- `Alis.Reactive.Fusion/Components/FusionSidebar/FusionSidebar.cs`
- `Alis.Reactive.Fusion/Components/FusionSidebar/FusionSidebarHtmlExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionSidebar/FusionSidebarExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionSidebar/FusionSidebarEvents.cs`
- `Alis.Reactive.Fusion/Components/FusionSidebar/FusionSidebarReactiveExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionSidebar/Events/FusionSidebarTransitionArgs.cs`
- `Alis.Reactive/Components/Contracts/ComponentRef.cs`
- `Alis.Reactive/Components/Contracts/ComponentProperty.cs`
- `Alis.Reactive/Components/Contracts/ComponentMethod.cs`
- `Alis.Reactive/Components/Onboarding/ComponentEventOnboarding.cs`
- `Alis.Reactive/PlanModel/Values/ValueExpression.cs`

Sync/async lane expectation: the sidebar render is sync markup render; the
`open` and `close` component-event triggers are sync; `Show`, `Hide`, `Toggle`,
and the `IsOpen` read are sync component actions. The Sidebar slice introduces
no async boundary. Async only appears when a developer composes the `IsOpen()`
read source into an HTTP `Post(...).Gather(...)` pipeline, which is the HTTP
primitive, not a Sidebar concern.

## Authoritative Primitive Rows

| Sidebar row member | Syncfusion evidence | DSL primitive | Domain/JSON term | Runtime behavior | Status |
| --- | --- | --- | --- | --- | --- |
| `open` event trigger | `traces/raw-ej2-core.trace.json` row `open` fires with `ownKeys` `cancel,element,event,isInteracted,model,name`; `sidebar.js` `show()` calls `this.trigger('open', ...)` | `TypedEvent<FusionSidebarTransitionArgs>` named `open`, selected by `.Reactive(e => e.Opened, ...)` | `StartsWhen.ComponentEvent(componentId, "open")` | runtime wires the Syncfusion `open` event and starts the reaction with event payload scope | accepted and proven |
| `close` event trigger | core trace row `close` fires with the same `EventArgs` keys; `sidebar.js` `hide()` calls `this.trigger('close', ...)` | `TypedEvent<FusionSidebarTransitionArgs>` named `close`, selected by `.Reactive(e => e.Closed, ...)` | `StartsWhen.ComponentEvent(componentId, "close")` | runtime wires the Syncfusion `close` event and starts the reaction with event payload scope | accepted and proven |
| `open`/`close` `.isInteracted` | core trace `open`/`close` payload `isInteracted: false` for programmatic `show()/hide()`; `sidebar.js` sets `isInteracted: !isNullOrUndefined(e)` (true only when a DOM event drove the transition) | event payload read | `ValueExpression.ReadPayload(PayloadSource.Event(), "isInteracted", Shape.Boolean)` from `FusionSidebarTransitionArgs.IsInteracted` | runtime reads `event.isInteracted` to distinguish a user-driven dismissal (true) from an API close (false) | accepted and proven |
| `isOpen` property read | core trace rows `isOpen after show` -> `true`, `isOpen after hide` -> `false`, `isOpen after toggle` -> `true`; `isOpen` is a `@Property` on the instance | `ComponentProperty<bool>.Named("isOpen")` + `self.Read(property)` | `ValueExpression.Read(ComponentObject, "isOpen", Shape.Boolean)` from `FusionSidebarExtensions.IsOpen(...)` | runtime reads `sidebar.isOpen` into a typed bool source | accepted and proven |
| `show()` method | core trace `show()` call followed by `isOpen after show` -> `true`; `sidebar.js` `show()` adds `e-open`, removes `e-close`, sets `isOpen=true` | `ComponentMethod.Named("show")` + `self.EmitCall(method)` | `CallReaction` targeting component method `show` | runtime invokes `sidebar.show()` and the panel slides into view | accepted and proven |
| `hide()` method | core trace `hide()` call followed by `isOpen after hide` -> `false`; `sidebar.js` `hide()` adds `e-close`, removes `e-open`, sets `isOpen=false` | `ComponentMethod.Named("hide")` + `self.EmitCall(method)` | `CallReaction` targeting component method `hide` | runtime invokes `sidebar.hide()` and the panel tucks away | accepted and proven |
| `toggle()` method | core trace `toggle()` call followed by `isOpen after toggle` -> `true` (flipped from the closed state); `sidebar.js` `toggle()` calls `show()` or `hide()` by current state | `ComponentMethod.Named("toggle")` + `self.EmitCall(method)` | `CallReaction` targeting component method `toggle` | runtime invokes `sidebar.toggle()` and the panel flips open or shut | accepted and proven |
| `open`/`close` `.cancel` | core trace payload `cancel: false`; EJ2 honors a cancel write to abort the transition | excluded writable transition guard | no public C# payload member | no runtime mapping for this row | excluded; no Senior Living use case cancels the slide transition, and a writable cancel would need its own lifecycle-effect proof (`_skill/pattern-map.md#p018-writable-payload-fields-need-lifecycle-effect-proof`) |
| `open`/`close` `.element` | core trace payload `element: [Element#sidebar]` | excluded browser-owned element | no public C# payload member | runtime must not serialize or expose this through typed event args | excluded; browser-owned DOM element, see `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them` |
| `open`/`close` `.event` | core trace payload `event: null` for programmatic calls; `MouseEvent | Event` when DOM-driven | excluded browser-owned event object | no public C# payload member | runtime must not serialize or expose this | excluded; browser-owned DOM event, see `_skill/pattern-map.md#p004-dom-payload-objects-are-browser-owned-until-a-dom-source-row-proves-them` |
| `open`/`close` `.model` | core trace payload `model` is the whole Sidebar instance graph | excluded broad vendor object | no public C# payload member | runtime must not serialize the component instance | excluded; broad vendor object, the component ref already owns the instance join |
| `open`/`close` `.name` | core trace payload `name: "open"` / `"close"` | excluded duplicate event metadata | no public C# payload member | no runtime mapping for this row | excluded; the event selector already owns event identity |
| `change` event | core trace `change` fires `ChangeEventArgs` with `element,name` only | not accepted for the current rows | no public C# event selector | no runtime mapping for this row | excluded; `change` carries only a browser-owned `element` and duplicate `name`, no `isInteracted`; the `open`/`close` pair already covers the transition with the proven `isInteracted` payload |
| `created`, `destroyed` events | core trace `created` fires an empty/undefined payload; `event-payload-surface.json` marks `created`/`destroyed` broad `Object` | not accepted for the current rows | no public C# event selector | no runtime mapping for this row | excluded; lifecycle-only, broad vendor object payloads with no typed members |
| `height`, `locale`, `defaultBackdropDiv` properties | `probes/raw-ej2-core.html` candidate rows mark these runtime properties; no proven post-render read/write use case | not accepted for the current rows | no runtime DSL member | runtime never reads or writes them from a plan | excluded; no focused Senior Living use case; `defaultBackdropDiv` is a browser-owned element |
| `type`, `position`, `width`, `isOpen` (initial), `closeOnDocumentClick`, `animate`, `enableDock`, `enableGestures`, `showBackdrop`, `target`, `zIndex`, and the rest of the model | `discovery/mvc-builder-coverage.md` shows these on the MVC `SidebarBuilder`; the builder owns initial configuration | builder-owned static configuration | no runtime DSL member | initial render configured on `SidebarBuilder`; no post-render read/write proven necessary (other than the `isOpen` read row above) | excluded; builder-owned, configured at render through `Html.FusionSidebar(..., b => ...)` |
| `destroy()` method | `probes/raw-ej2-core.html` classifies it `skip: lifecycle cleanup` | not a Fusion plan behavior | no runtime DSL member | runtime never calls it from a plan | excluded; lifecycle cleanup, not plan behavior |

## Primitive Decision

No new primitive is needed for the mapped Sidebar rows. Current primitives
already cover every onboarded member:

- component event trigger (`open`, `close`);
- event payload read (`isInteracted`);
- component property read (`isOpen`);
- component method call (`show`, `hide`, `toggle`).

Any future failure to read one of these accepted members is a
discovery/mapping/typed-contract problem first, never permission to add a
primitive.

## Code To Delete Or Simplify

None identified for the primitive layer. The slice keeps the open and close
transitions as two distinct event selectors (`Opened`, `Closed`) rather than a
single `change` selector, because only `open`/`close` carry the proven
`isInteracted` payload that distinguishes a user dismissal from an API close.

## Behavior Proof Required Before Commit

The Sidebar rows are proven by
`tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Sidebar/WhenUsingFusionSidebar.cs`
through `scripts/playwright.sh`, with each member tied to a fails-when-broken
assertion in `proof/behavioral-coverage.json`:

1. `FusionSidebar(...)` render places the panel on the dashboard tucked away (`e-close`) at first paint;
2. `Show()` slides the panel into view (`e-open`) and its nav links become reachable;
3. the `Opened` event fires the reaction through `.Reactive`, posting and showing the live service list;
4. `Closed` fires the reaction when `Hide()` tucks the panel away;
5. `IsInteracted` distinguishes a coordinator dismissal (tapping the dashboard, true) from the button's API close (false);
6. `Hide()` returns the panel to `e-close`;
7. `IsOpen()` yields the panel state into the close POST gather body;
8. `Toggle()` flips the panel open then shut from the header and in-panel buttons.
