# Plugins

The escape hatch: a JavaScript object the page's own code provides by name, with no element, for work the DSL does
not express (URL, DOM and array APIs), declared as a `Plugin` whose members are typed and reached by its type.
Module 13 of `spec/CLAUDE.md`.

## Core questions

- Q17: a plugin is a live object the page's code provides by name, with no element (D17.14); its name has a vendor's
  characters and no dot, so the `alis.` names stay the framework's (D17.7, D17.14); its events listen by a vendor's
  listening kinds, `EventTarget` unless its `EventListening` names another (D17.3, D17.14); boot reads the page's
  queue and a later push registers at once; a name no script provides, or two provide, stops only the steps that
  reach it; a plugin that is a function is called with `CallItself` (D17.14).
- Q7: a plugin's argument is a value position (D7.1) and its member a source (D7.2); a method's `Arg<T>` takes a
  literal of its type or a `TypedSource<T>` (D7.3).
- Q5: a plugin is a live object (D5.1) declared member for member with the whole vocabulary (D5.3), a method of any
  arity over one list of `Arg`s (D5.9), its result bound at its line (D5.11); the core makes a declaration with
  `new T()`, and a plugin's name is refused where the build or the plan reads it, never in its constructor (D5.12).
- Q4: a member a plugin lacks, and a plugin no script provides, stop the pipeline that reaches it (D4.1).
- Q12: a plugin's handlers subscribe when their plan starts and leave at its owner's release (D12.4, D12.5).
- Q2: no plugin takes a member path as text (D2.2).
- Q18: ALIS034 checks a constant plugin name (N18.12).

## Boundaries

A vendor's data, its listening kinds and the one driver are module 12's, and so is the bridge: `alis:mount`,
`alis:unmount` and the page's plugin queue, with its improvement, a push after boot registered at once. A vendor's
lookup (N17.6) is module 12's door, and the plugin it finds is found as this module finds any plugin, under M13.D1.
The declaration vocabulary, `JsObject`, its doors and their presence marks, is module 11's. The app-level objects are
modules 23 and 25's: FusionConfirm and FusionToast, NativeConfirm, NativeToast, NativeDrawer and NativeLoader, and
the row actions' plugin `native-row-actions` (N17.12). Realtime's plugins `alis.hub` and `alis.stream` and the
contract `reconnect` are module 10's (D21.6, D21.11); the request's contracts `busy` and `notice` are module 6's
(D10.21), and `GatherBuilder.Plugin` is M6.F1's. `alis.form` is module 8's, `alis.slot` module 9's, `alis.model`'s
inputs module 7's and `alis.document`'s events module 1's. `TypedPluginSource` and `TypedPluginPropertySource` are
M2.F1's, carried by M2.N1. The locator `plugin` and the plugin data `none` are module 14's grammar, boot's order is
module 17's, and ALIS034's code (N18.12) is module 20's. A page plugin's listener released by a card, X12.15 to
X12.17, is placed with modules 9 and 12 (M9.F2, M12.F1). X2.2 is here because its subject is a plugin; N2.1, the
derivation that leaves the core's `alis.model` no text door, is module 15's.

## Doors

Each name is gate 2's; each door is the capability it carries.

This module adds no door. Its doors are the core's: `Plugin`, `Plugin.Name` and `Plugin(string name)`, kept on both
lines (Q17); `Plugin(string name, EventListening events)`, `EventListening` with `EventTarget`,
`CallbackProperties`, `Methods` and `ReturnedUnsubscriber`, `p.Plugin<TPlugin>()`,
`plan.Plugin<TPlugin>().Reactive(..)` and `CallItself` (N17.11); the declaration vocabulary a plugin declares its
members with, `Property`, `ReadOnly`, `WriteOnly`, `Method`, `Method<T>`, `MethodAsync`, `MethodAsync<T>`, `Event`
and `Event<T>` (Q5), over one list of `Arg`s (N5.3), with presence marks (N5.4) and `Read` past a member (N5.6); and
ALIS034 (N18.12). The POC's forms: `poc/Alis.Reactive.Poc/Components/Plugin.cs:7-16`,
`poc/Alis.Reactive.Poc/Components/EventListening.cs:12-24`, `poc/Alis.Reactive.Poc/Pipelines/Pipeline.cs:27-29`,
`poc/Alis.Reactive.Poc/Hosting/ReactivePlan.cs:46-47`, `poc/Alis.Reactive.Poc/Hosting/ReactivePlan.cs:109-120` and
`poc/Alis.Reactive.Poc/Components/JsObject.cs:154-161`.

`RegisterPlugin<TPlugin>()`, on both lines, leaves for N17.11: a plugin is reached by its type, so a registration is
no step a view writes, and a second registration on one plan is not representable. Main's two other
`RegisterPlugin` overloads, its member declarations by name (`Plugin.Command`, `Plugin.Function<T>`,
`Plugin.Property<T>`, `PluginTypeBuilder` and the declared members they return) and its reach by name
(`p.Plugin(name, member)` and its kin, with their argument builders) are refused, each with its carriers, in
`spec/Plugins/features.json`.

The meaning `M13.D1` is in `spec/Plugins/features.json`, on the feature it gives.

## Improvements

- A plugin's members are typed by its declaration and checked by the compiler (D17.14, D5.9). Main declares them by
  strings and matches a call's arguments to shapes read with `typeof` where the view renders
  (`Alis.Reactive/PlanAuthoring/Plugins/PluginArguments.cs:83`,
  `Alis.Reactive/PlanAuthoring/Plugins/PluginTypeBuilder.cs:183` at 623dd4a27); an inline member takes any
  arguments unless they are listed (`Alis.Reactive/PlanAuthoring/Plugins/PluginTypeBuilder.cs:24-27` at 623dd4a27),
  and its runtime checks the call again against the plan's contract
  (`Alis.Reactive.Assets/runtime/browser-objects/runtime-plan.ts:180-186` at 623dd4a27).
- A plugin's events are heard by its declaration's listening (D17.14, N17.11, X17.24). The redesign's plugin raises
  no events a page subscribes to (`Alis.Reactive/Components/Plugin.cs:8` at 65fdf118d): a page's plugin is an object
  with no `subscribe` (`Alis.Reactive.Assets/runtime/objects/plugins.ts:36` at 65fdf118d), which its runtime calls to
  subscribe a handler (`Alis.Reactive.Assets/runtime/objects/plan-objects.ts:61` at 65fdf118d).
- A plugin's fault stops only the steps that reach it (D17.14, X17.26). Both lines stop the page's start for a name
  provided twice or a value that is no object: main in its catalog
  (`Alis.Reactive.Assets/runtime/plugins/catalog.ts:14`, `Alis.Reactive.Assets/runtime/plugins/catalog.ts:63-64` at
  623dd4a27), which its boot calls before any plan (`Alis.Reactive.Assets/runtime/root.ts:35` at 623dd4a27), and the
  redesign in its registry (`Alis.Reactive.Assets/runtime/objects/plugins.ts:24`,
  `Alis.Reactive.Assets/runtime/objects/plugins.ts:35` at 65fdf118d), as its tests assert
  (`tests/Alis.Reactive.Framework.PlaywrightTests/Plugins/WhenThePageMeetsAPluginItCannotUse.cs:22-38` at 65fdf118d).
- The `alis.` names stay the framework's (D17.14, X17.27). Both lines refuse only whitespace in a plugin's name
  (`Alis.Reactive/PlanModel/WireTerms/PlanTerms.cs:176` at 623dd4a27, `Alis.Reactive/WireTerms/PlanTerms.cs:71` at
  65fdf118d), so a declaration named `alis.form` renders on both lines, and a page script that provides that name
  stops the redesign's boot, which registers it itself (`Alis.Reactive.Assets/runtime/root.ts:30` at 65fdf118d), as
  registered twice (`Alis.Reactive.Assets/runtime/objects/plugins.ts:24` at 65fdf118d).
- A plugin that is a function is called by `CallItself`, with `this` undefined (D17.14). Main declares it by a
  target it reports as `root` (`Alis.Reactive/PlanAuthoring/Plugins/Plugin.cs:204-205` at 623dd4a27), and the
  redesign by a member named with empty text (`Alis.Reactive/Components/Plugin.cs:16` at 65fdf118d).
- A call is a step where it is written (D5.11). Main writes a plugin command only at `Fire()`
  (`Alis.Reactive/PlanAuthoring/Plugins/PluginMemberBuilder.cs:269-274` at 623dd4a27), so a call whose `Fire` is left
  off writes nothing and compiles.
- One plugin name is one object with one listening in a plan (M13.D1, M13.X1, M13.X2). Main and the redesign refuse
  a name registered twice on one plan, whatever its declarations
  (`Alis.Reactive/PlanModel/BrowserObjects/BrowserObjectContracts.cs:44-45` at 623dd4a27,
  `Alis.Reactive/PlanModel/Document/PlanBuildContext.cs:155-159` at 65fdf118d). Reached by its type, two declarations
  of one name meet in one plan, and the POC writes the last reach's listening over the first
  (`poc/Alis.Reactive.Poc/Writing/PlanBuild.cs:436-441`); no core line states which listening holds.
