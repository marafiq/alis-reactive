# Triggers and pipelines

A handler and the pipeline it runs: on load, on a declared app event, under its handler's policy; and the app event
itself, declared once, raised by a dispatch step or from markup, and heard with its typed payload. Module 1 of
`spec/CLAUDE.md`.

## Core questions

- Q3: a value held as text reaches a typed position through `as` with its shape (D3.13), the read a handler gives each
  scalar member of an app event's payload, so markup's text and `Dispatch`'s live form read alike (M1.D3).
- Q4: a stop ends its own handler only, and never reaches what raised the event, a dispatch step included (D4.8); an
  app event's payload is data, so a member it lacks reads its start, and a payload that holds nothing reads nothing
  (D4.2).
- Q5: an app event's payload is data read with `Read` (N5.6); an event declared with no payload hands none (D17.4).
- Q7: a dispatch's payload is a value position, so it takes a literal or any source of D7.2, an object built member by
  member among them (N7.1).
- Q11: a handler runs one run (D11.11) under its policy, `overlap`, `replace` or `skip` (D11.7, N11.1); its steps
  run in the trigger's turn until one waits (D11.1); two handlers of one event start in the order they subscribed
  (D11.5); a dispatch is a trigger like any other (D11.6, G9); every stage is a block (D11.10).
- Q12: a run belongs to the owner of the plan that holds its handler, the page or a slot, and its owner's dispose ends
  it (D11.8).
- Q15: a Fusion template's event button raises an app event by filling markup's payload attributes in the browser,
  and D15.2 keeps templates as they are, so markup's payload form stays the one that button fills.
- Q17: a handler's payload is a path into its listener's arguments, `[0, "detail"]` for an app event and `[]` for one
  declared with no payload (D17.4); listeners subscribe in the order D17.3 states: a page's plans in the order the page
  holds their scripts, one plan's handlers in its order, a slot's plans when its load starts them.
- Q18: no run-time reflection of the core's own reads a payload's members (D18.1); a payload is written from what the
  app's JSON contract writes, through the plan that holds the app's options.

## Doors

Each name is gate 2's; each door is the capability it carries.

- `M1.N1` new: the on-load handler, which runs its pipeline once its owner has started (M1.D1). It replaces
  `TriggerBuilder<TModel>.DomReady` on both lines; in a slot it runs at the slot's load, not at the document's
  `DOMContentLoaded`, so its name is gate 2's.
- `M1.N2` new: `AppEvent` and `AppEvent<TPayload>`, a document event declared once as a static member, its name the
  member's name in camel case or the `name:` it gives, so a dispatcher and a handler name the declaration, never a
  string. Kept from the redesign; it replaces main's event-name strings.
- `M1.N3` new: the handler of a declared app event, `CustomEvent(appEvent, pipeline)` and `CustomEvent(appEvent,
  (payload, pipeline) => ..)`, whose payload is a `Data<TPayload>` read at `[0, "detail"]` (D17.4) with `Read` (N5.6).
  It replaces `CustomEvent` on both lines.
- `M1.N4` new: `Dispatch(appEvent)` and `Dispatch(appEvent, payload)`, a step that raises the declared event on the
  document (M1.D2), its payload a literal of `TPayload` or a `TypedSource<TPayload>` (N7.1 builds one from sources).
  It replaces the redesign's four `Dispatch` overloads and `DispatchPayloadBuilder`, and main's `Dispatch(string)`,
  `Dispatch<TPayload>(string, ..)` and `DispatchWith`.
- `M1.N5` new: `Html.Raises(plan, appEvent)` and `Html.Raises(plan, appEvent, payload)`, markup that raises a declared
  app event on a click at or inside the element it marks (M1.D3). It takes the plan, which carries the app's JSON options on
  both hosts, so the helper names and writes each member from no other store. It replaces the
  redesign's `Raises`, which takes no plan, reads its members by run-time reflection and names them in camel case
  whatever the app's options.
- `M1.N6` new: ALIS039, the app event analyzer: two `AppEvent` declarations whose event names are equal, in the
  compilation that declares them, the name read from the constant `name:` or the declaring member's name. The
  framework's packages declare no app event, so an app's own compilation holds every declaration ALIS039 reads; two of
  an app's assemblies that each declare one name are outside it, an exception gate 2 puts to the owner.

The meanings `M1.D1` to `M1.D3` are in `spec/TriggersAndPipelines/features.json`, each on the feature it gives.

## Improvements

- Markup keeps one attribute per payload member, since a Fusion template's event button fills the same attributes in
  the browser (`Alis.Reactive.Fusion/Templates/TemplateElements.cs:34-56` at 65fdf118d) and D15.2 keeps templates as
  they are; how C# writes them and how a handler reads them change. The redesign writes each member by run-time
  reflection (`Alis.Reactive/Razor/MarkupRaise.cs:24-31` at 65fdf118d, B11), names it in camel case whatever the app's
  options, and the browser hands every member over as text (`Alis.Reactive.Assets/runtime/plugins/document.ts:65-73`
  at 65fdf118d). M1.D3 writes the members from what the app's JSON contract writes, through the plan, and a handler
  reads each scalar member through `as` with its shape, one read for markup and `Dispatch`. A member that holds null is
  named in the core's `data-alis-null`: the redesign drops it (`Alis.Reactive/Razor/MarkupRaise.cs:26` at 65fdf118d) and
  hands `{}` for markup with no payload (`Alis.Reactive.Assets/runtime/plugins/document.ts:65-73` at 65fdf118d), which
  its reads take as missing, like null, while D4.2 reads a member a present payload lacks as its start. The refusals of a record, a
  list and a name an attribute cannot carry back stay (`Alis.Reactive/Razor/MarkupRaise.cs:36-56` at 65fdf118d); under
  an app whose naming starts a name with a capital letter, every markup payload member is refused, naming `Dispatch`.
- Two declarations of one name are one DOM event, so a handler of one hears the other's dispatch with another payload
  type. The redesign's own test app declares `DeskEvents.Rang` as an `AppEvent<BellRang>` and `HallEvents.Rang` as an
  `AppEvent<HallBellRang>` (`tests/Alis.Reactive.Framework.App/Slots/DeskPage.cs:43`,
  `tests/Alis.Reactive.Framework.App/Slots/HallPage.cs:16` at 65fdf118d), and `MoveInEvents.Opened` with a payload
  beside `GateEvents.Opened` with none (`tests/Alis.Reactive.Framework.App/Slots/MoveInPage.cs:54`,
  `tests/Alis.Reactive.Framework.App/Subscriptions/GatePage.cs:43` at 65fdf118d). ALIS039 refuses both pairs at build
  (M1.N6). The framework's packages declare no app event; they take one as an argument
  (`Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:242` at 65fdf118d).
- A dispatch's payload takes every source and every literal (D7.1). Main's `DispatchPayloadBuilder.Set` takes a
  literal only as a `bool`, an `int` or a `string` (`Alis.Reactive/PlanAuthoring/Pipelines/DispatchPayloadBuilder.cs:40-71`
  at 623dd4a27), and both lines build the object with a builder of its own beside `GatherBuilder`; N7.1 is the one
  object builder.
- On load waits for every handler its start subscribes (M1.D1). The redesign subscribes every plan of every owner
  before it starts any owner (`Alis.Reactive.Assets/runtime/plans/start.ts:18-30` at 65fdf118d); main defers its
  page-ready behaviors within the one plan it boots (`Alis.Reactive.Assets/runtime/lifecycle/boot.ts:46-60` at
  623dd4a27). M1.D1 states the order the redesign has, by D17.3's subscription order.
