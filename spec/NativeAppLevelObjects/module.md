# Native app-level objects

The framework's own objects of the page, in Alis.Reactive.Native: `NativeDrawer`, `NativeConfirm`, `NativeLoader`,
`NativeToast` and `NativeReconnectOffer`, each one declaration and a render helper the layout calls once, and the
row actions, whose `NativeActionButton` and `NativeActionLink` serve any grid-like list. Each is built only on the
core's public surface, as an app's object is, so the core names none of them and they prove that surface (D17.10). The
DOM is their vendor: each member is named as lib.dom names it, and each object renders semantic markup with stable
`alis-` class names and no style, which is the app's. Module 25 of `spec/CLAUDE.md`.

## Core questions

- Q11: a user's decision is an awaited step (D11.2): `NativeConfirm.Ask` is a `MethodAsync<bool>`, read before the
  branch that reads it, and an ask whose run ends is closed through the block's signal (D17.11). A drawer's `Cancel`
  is written before the first await, since the drawer reads it when its event returns (D11.4). A toast's time is its
  own, and no step waits for it (D11.9).
- Q17: the core names no component (D17.9). NativeConfirm and NativeToast are the framework's own (D17.10, N17.4);
  each object's browser code reaches the runtime only through `alis:mount`, `alis:unmount` and the page's plugin
  queue (D17.14). The row actions are D17.15's, with the plugin `native-row-actions` (N17.12).
- Q10: NativeLoader fulfils the busy contract and NativeToast the notice contract, which the core alone calls
  (D10.21); a view shows busy by a request's targets and a toast by `Notice()` (N10.7).
- Q21: NativeReconnectOffer fulfils the reconnect contract the core counts (D21.11, N21.7).
- Q12 and Q13: each object is the page's and no slot's; a slot's handler of one leaves with the slot (D13.1, D12.8).
- Q5 and Q4: an object is found by its fixed id or its contract's name, and a member it lacks stops the pipeline that
  reaches it (D5.1, D4.1); a message or a text that reads nothing shows nothing (D4.5).

## Boundaries

The busy, notice and reconnect contracts, their counts and the request doors that hold them (`ShowBusy()`, `Busy`,
`NoBusy()`, `Notice()`) are modules 6's and 10's (D10.21, D21.11, N10.7, N21.7); this module owns the Native objects
that fulfil them. The condition over an awaited `MethodAsync<bool>` is the core's (N17.3), the abort signal's argument
kind module 12's (N17.5), and FusionConfirm and FusionToast module 23's, which implement `IAppLevelComponent` and so
take this module's reach with no id. `plan.Rows<TRow>(rowsId)` and `Rows<TModel, TRow>` are the core's (N17.12), and
ALIS035's code module 20's (N18.13); `RowActivated<TRow>`, `RowAction<TModel, TRow>.Render`, `NativeActionButton`,
`NativeActionLink` and `native-row-actions` are this module's. The action link's typed attribute doors are module
16's (N18.4, M16.F1's claim of `NativeActionLinkBuilder<TModel>.Attr`). `NativeComponent` and the native inputs are
module 24's. `ReactivePlan<TModel>.Component<TComponent>()`, a door of the plan's types
(`spec/PartialsAndSlots/module.md`), is claimed by M25.F6. The core defects X17.6 to X17.9 are placed in M25.F4 and
M25.F5, and X17.31 to X17.38 in M25.F3.

## Doors

Each name is gate 2's; each door is the capability it carries.

- `M25.N1` kept: `NativeDrawer`, a native component whose object is its element, `IAppLevelComponent` with
  `ElementId`, "alis-drawer", and `DefaultId`: `Open()`, `Close()`, `Size` as `Property<DrawerSize>`, and
  `BeforeClose` as `Event<BeforeCloseArgs>`, its payload the class `NativeDrawer.BeforeCloseArgs` nested in the
  drawer, with `ClosedBy` as `ReadOnly<DrawerClosedBy>` and `Cancel` as `Property<bool>`, and `Closed` as `Event`,
  raised once a close is decided, as M25.D1 states; the enums
  `DrawerSize` (`Sm`, `Md`, `Lg`) and `DrawerClosedBy` (`Escape`, `CloseButton`, `Close`), each member written as the
  drawer code's text; `TitleId` and `ContentId`, the heading's and the content's ids, so a view names no drawer id as
  text; and `Html.NativeDrawer(closeText)`, its form gate 2's. It keeps the redesign's members
  (`Alis.Reactive.Native/AppLevel/NativeDrawer/NativeDrawer.cs:41-61` at 65fdf118d) with the payload nested and renamed
  from `NativeDrawerBeforeCloseArgs`, since a type named as the event would collide with it. Main's
  `NativeDrawerExtensions` are renamed to it: `SetSize`, `Open` and `Close` are `Size`, `Open()` and `Close()`, and
  `KeepOpenOnEscape`, a class a pipeline adds
  (`Alis.Reactive.Native/AppLevel/NativeDrawer/NativeDrawerExtensions.cs:62-69` at 623dd4a27), is a `BeforeClose`
  handler that writes `Cancel` when `ClosedBy` is `Escape`, as the redesign replaced it (766b976af). The drawer's
  content is a slot, released only by a load or an unload (D12.4): the drawer code empties none, and a view unloads
  it with `Unload(NativeDrawer.ContentId)` in a handler of `Closed`, or its next `Into` replaces it.
- `M25.N2` kept: `NativeLoader`, the declaration of the busy contract its code provides as the plugin `busy`, with
  `show` and `hide` and an optional target, none a member a pipeline reaches (D10.21), and
  `Html.NativeLoader(text)`, its form gate 2's, as M25.D2 states. Both lines' pipeline doors leave: main's `Show`,
  `Hide`, `SetTarget` and `SetTimeout` are refused for N10.7, and the redesign's `Show()`, `Hide()`, `Target` and
  `Timeout` (`Alis.Reactive.Native/AppLevel/NativeLoader/NativeLoader.cs:37-46` at 65fdf118d) leave for the same
  doors. A request's own message, which both lines write into `alis-loader-message` while the loader covers a target
  (`Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/AppLevel/Drawer/_AddResidentFormPartial.cshtml:35` at
  65fdf118d), is the view's own element, set in the request's `WhileLoading` and emptied in its `Finally` (N10.7,
  M5.N1), so it shows beside the target it names; the page indicator's message is the app's text from the render.
  The loader is found by its contract's name and no longer implements `IAppLevelComponent`, so its `ElementId` and
  `DefaultId` leave the public surface: no author reaches it.
- `M25.N3` new: the row actions' Native helpers on N17.12: `NativeActionButton(action)` and `NativeActionLink(action)`
  over `Rows<TModel, TRow>`, each declaring an action whose block's payload is `RowActivated<TRow>`, `Row` and
  `Element`; `RowAction<TModel, TRow>.Render(row, text)`, the control, whose `CssClass(css)` keeps both lines'
  builder's; and the plugin `native-row-actions`, as M25.D3 states. They replace both lines' action link: main's
  helper, which writes a whole plan into each link
  (`Alis.Reactive.Native/Components/NativeActionLink/NativeActionLinkSerializer.cs:44-46` at 623dd4a27), and the
  redesign's `NativeActionLink`, `NativeActionLinkClickArgs`, `NativeActionLinkRequest` and `NativeActionLinkConfirm`,
  which write a plan per link (`Alis.Reactive.Native/Components/NativeActionLink/NativeActionLinkBuilder.cs:142-146`
  at 65fdf118d). A link whose block posts, puts or deletes is a button: ALIS035 holds a link to one GET, since a new
  tab opens only what a GET loads (N18.13). The redesign's `Confirm(message).Then(..)` is a condition over
  `NativeConfirm.Ask` or `FusionConfirm.Ask` in the action's block (N17.3), its `Component<TComponent>(refId)` the
  block's own reach (M12.N2), and its click payload's `Button`, modifier keys and `PreventDefault()` the plugin's own
  reading of the click; its `DefaultPrevented` is refused, since the plugin infers no intent from another script.
- `M25.N4` new: `NativeConfirm`, a native component whose object is its `<dialog>`, `IAppLevelComponent` with
  `ElementId`, "alis-confirm", and `DefaultId`: `Ask(message)`, a `MethodAsync<bool>` whose message is any source,
  declared with the block's abort signal as its second argument (N17.5), and `Html.NativeConfirm(title, ok,
  cancel)`, the app's texts, its form gate 2's, as M25.D4 states (N17.4).
- `M25.N5` new: `NativeToast`, the declaration of the notice contract its code provides as the plugin `notice`, with
  `show(text, kind)`, no member a pipeline reaches (D10.21), and `Html.NativeToast(shownFor, dismiss)`, the app's
  time and dismiss text, its form gate 2's, as M25.D5 states (N17.4). A view shows a toast with `Notice()` (N10.7).
- `M25.N6` kept: `IAppLevelComponent` with `DefaultId`, `pipeline.Component<TComponent>()`, the declaration bound to
  its `DefaultId`, and `plan.Component<TComponent>()`, the same object for the plan's handlers with `Reactive(event,
  pipeline)` (M12.N3), as M25.D6 states. Each stays public by the owner's rule of 2026-11-05, since an author needs
  each: a component author declares an app-level object with `IAppLevelComponent` and `DefaultId`, an app's as the
  framework's (D17.10), and a view author reaches it with no id, so the fixed id is written once, in its declaration.
  Main's `Component<TComponent>()`, which returns a `ComponentRef` written through internal members
  (`Alis.Reactive/PlanAuthoring/Pipelines/PipelineBuilder.cs:136-142` at 623dd4a27), is renamed to it.
- `M25.N7` new: `NativeReconnectOffer`, the declaration of the reconnect contract its code provides as the plugin
  `reconnect`, with `show(retry)` and `hide()`, no member a pipeline reaches, and `Html.NativeReconnectOffer(text,
  retry)`, the app's texts, its name and form gate 2's, as M25.D7 states (N21.7). It replaces main's layout element
  the developer writes (`Alis.Reactive.Assets/runtime/execution/realtime/retry-indicator.ts:12` at 623dd4a27).

The meanings `M25.D1` to `M25.D7` are in `spec/NativeAppLevelObjects/features.json`, each on the feature it gives.

## Improvements

- A drawer resets its own state and raises `Closed` when it closes, whatever the app's CSS animates, and its content
  leaves by the core's unload, which releases the partial's handlers (M25.D1, M25.X1). Both lines reset its content,
  heading, size and `aria-hidden` only on `transitionend`
  (`Alis.Reactive.Assets/runtime/components/native/drawer.ts:41` at 623dd4a27,
  `Alis.Reactive.Assets/components/native/drawer.ts:77-81` at 65fdf118d), which fires only where a transition runs
  (`probe:dom-transitionend-needs-a-transition`), and the redesign's transition is its own stylesheet's
  (`Alis.Reactive.Native/css/native.css:9` at 65fdf118d), which the rewrite does not ship.
- A drawer's size always names a `DrawerSize` (M25.D1, M25.X2). The redesign's size reads "" once its reset removed
  the size classes (`Alis.Reactive.Assets/components/native/drawer.ts:33`, `:55` at 65fdf118d), a default the runtime
  invents (BRIEF B12); the rewrite's drawer holds md from the render and after each reset.
- The drawer's ids and its close text are typed and the app's (M25.N1). Both lines' views write
  "alis-drawer-content" and "alis-drawer-title" as text, and both helpers write a close button labelled "Close" with
  an icon (`Alis.Reactive.Native/AppLevel/NativeDrawer/NativeDrawer.cs:113-115` at 65fdf118d).
- Each busy target shows its own overlay, and no style is written on a target (M25.D2, M25.X3). Both lines move one
  loader element into the one target set last (`Alis.Reactive.Assets/components/native/loader.ts:58` at 65fdf118d)
  and leave `position: relative` on each target's inline style (`Alis.Reactive.Assets/components/native/loader.ts:57`
  at 65fdf118d, `Alis.Reactive.Assets/runtime/components/native/loader.ts:47` at 623dd4a27), and both hide the loader
  by a timeout a view sets while its request can still run
  (`Alis.Reactive.Assets/components/native/loader.ts:68` at 65fdf118d), the early hide D10.21 forbids.
- A page without Fusion asks (D17.9, D17.10, X17.6). Main's runtime reads a global only Fusion's confirm sets
  (`Alis.Reactive.Assets/runtime/conditions/conditions.ts:120-123` at 623dd4a27), and the redesign's plan calls
  `alis.confirm` (`Alis.Reactive/PlanModel/Conditions/Condition.cs:504` at 65fdf118d), which only Fusion's confirm
  code provides (`Alis.Reactive.Assets/components/fusion/confirm.ts:94` at 65fdf118d). NativeConfirm is the platform's
  modal dialog, so the page behind is inert and Escape cancels it (`probe:dom-dialog-modal-click-behind`,
  `probe:dom-dialog-escape`), and an Escape stays in it, as the redesign's Fusion confirm keeps one
  (`Alis.Reactive.Assets/components/fusion/confirm.ts:67-69` at 65fdf118d), so a drawer behind stays open (X17.7,
  `probe:dom-dialog-escape-stopped-at-dialog`).
- A page has a toast of its own, which the core's notice reaches (D10.21, X17.9). Neither line ships one: the only
  toast is FusionToast, which a view shows by setting its members
  (`Alis.Reactive.Fusion/AppLevel/FusionToast/FusionToast.cs:27` at 65fdf118d). NativeToast's region is a manual
  popover, which neither Escape nor a click elsewhere hides
  (`probe:dom-popover-manual-escape-and-click-elsewhere`), and it is shown once: a region shown again over a modal
  dialog takes no click until the dialog closes (`probe:dom-popover-reshown-over-modal-dismiss-click`), so a toast
  that arrives during an ask is dismissed after it.
- A row action is declared once, and a link's click is read by the plugin, not by the page's other scripts (D17.15,
  M25.X4). Main finds a link from a document click with `closest` and skips a click another listener prevented
  (`Alis.Reactive.Assets/runtime/components/native/native-action-link.ts:33`, `:37` at 623dd4a27); the redesign
  writes a plan per link whose condition reads `DefaultPrevented`
  (`Alis.Reactive.Native/Components/NativeActionLink/NativeActionLink.cs:114` at 65fdf118d). A middle click raises no
  click and a modified click opens the link (`probe:dom-link-middle-click`, `probe:dom-link-modified-click`), so the
  plugin raises the action for an unmodified primary click only.
- An app brings its own app-level object on the core's public surface (D17.10, M25.N6). Main marks one a layout
  object in the plan with a role of its own (`Alis.Reactive/Components/Onboarding/ComponentObjectTarget.cs:38-45` at
  623dd4a27); the rewrite binds it as any reach binds a component, the page holding it (D13.1).
- The reconnect offer is a component the core counts for (D21.11, M25.N7). Main's runtime looks for a layout element
  the developer writes by its id and logs when it is missing
  (`Alis.Reactive.Assets/runtime/execution/realtime/retry-indicator.ts:17-24` at 623dd4a27).
