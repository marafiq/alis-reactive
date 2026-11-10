# Realtime

Server push on net10: a server-sent event stream and a SignalR hub, each declared once and shared by the server that
writes it and the view that listens to it; the triggers a message and a connection's lifecycle start; a connection
held by every owner whose plans name it; the reconnect offer while one has given up; and the hub's client as an asset
of its own. Module 10 of `spec/CLAUDE.md`.

## Core questions

- Q5: a pushed message is data, typed `Data<TMessage>` and read with `Read` (N5.1, D5.7), so a member it lacks reads
  nothing and it cannot be written to (X5.1).
- Q7: a stream and a hub are JS objects whose every member is a source: their state, a message's data and an invoke's
  result feed every value position (D21.12, N21.8).
- Q11: a message starts a run as a trigger does, under its handler's policy, in the order the messages arrive (D11.6,
  D11.7, D21.4); a hub's `invoke` and `send` are its async methods, awaited steps (D11.2), and an `invoke` a released
  owner waited on settles later and runs nothing (D11.8); a connection holds no busy indicator, and a request a
  message's run sends takes the request defaults (D21.10).
- Q12: a stream and a hub are held by every owner whose plans name it, opened at the first hold and closed at the last
  release, the reconnect offer leaving with them; a step reaches only a held connection (D12.6, D21.6).
- Q21: two transports, each by its own standard (D21.1), on net10 alone (D21.2, N21.6); one declaration per server
  event and per hub method (D21.3, N21.1, N21.2); what the framework did not produce is logged once and runs nothing
  (D21.4); the lifecycle triggers `reconnecting`, `reconnected` and `closed` on both transports (D21.5, N21.4); the
  reconnect contract and the app's startup choice (D21.11, N21.7); the hub's client linked only when a plan names a
  hub (D21.1, N21.5); each connection's waits stated in C# and carried: a stream's `retry:`, a hub's start and
  reconnection waits (D21.13, N21.9).

## Boundaries

The Native component that fulfils the reconnect contract is module 25's; this module owns the contract and the count
(D21.11). The busy indicator and the notice of a request a message's run sends are module 6's (D10.21). A `listen` on
an object a message holds is module 12's, with N12.1's listening kinds, as `spec/PartialsAndSlots/module.md` places
X12.9 and X12.10. The span of a message's run (D21.8) is module 26's, and linking the hub's asset by its content hash
is module 19's.

## Doors

Each name is gate 2's; each door is the capability it carries.

This module adds no door. Its doors are the core's: the message triggers typed `Data<TMessage>` (N5.1); a server event
and a hub's methods declared once (N21.1); the endpoint's typed event writer (N21.2); the plugin data `stream`
(N21.3); the lifecycle triggers (N21.4); the hub's own asset (N21.5); every realtime door on net10's surface alone
(N21.6); the reconnect contract and `AddReactiveRealtime`'s choice (N21.7); a stream's and a hub's members (N21.8);
a stream's reconnection wait (N21.9);
and the hub's `invoke` and `send`, kept (Q11).

The meanings `M10.D1` to `M10.D4` are in `spec/Realtime/features.json`, on the feature they give.

## Improvements

- One declaration names an event for the server and the view (D21.3, X21.6). Main names a server event and a hub
  method by strings (`Alis.Reactive/PlanAuthoring/Pipelines/TriggerBuilder.cs:75`,
  `Alis.Reactive/PlanAuthoring/Pipelines/TriggerBuilder.cs:102` at 623dd4a27). The redesign declares a stream and a
  hub once (`Alis.Reactive/PlanAuthoring/Events/ServerPushStream.cs:45`,
  `Alis.Reactive/PlanAuthoring/Events/SignalRHub.cs:35` at 65fdf118d) and its test endpoint writes the declared name,
  but with data from an anonymous object and the stream's text written by hand
  (`tests/Alis.Reactive.Framework.App/Realtime/RealtimeController.cs:46`,
  `tests/Alis.Reactive.Framework.App/Realtime/RealtimeController.cs:162` at 65fdf118d), so the message type binds
  nothing the endpoint writes. N21.2's writer takes the declaration and its message type.
- A message is data (D21.4, X5.1). Main reads a class the view builds with `new TPayload()`
  (`Alis.Reactive/PlanAuthoring/Pipelines/TriggerBuilder.cs:88-94` at 623dd4a27); the redesign declares a message as a
  `JsObject` of `ReadOnly<T>` members (`tests/Alis.Reactive.Framework.App/Realtime/RoundDeclarations.cs:10-15` at
  65fdf118d), a live object's vocabulary over JSON the server wrote.
- A message the framework did not produce runs nothing and logs once (D21.4, X21.1, X21.4). Main parses a stream
  message inside its listener with no guard and throws on a hub push of another shape
  (`Alis.Reactive.Assets/runtime/execution/realtime/server-push.ts:81`,
  `Alis.Reactive.Assets/runtime/execution/realtime/signalr.ts:157-170` at 623dd4a27); the redesign logs and drops both
  (`Alis.Reactive.Assets/runtime/realtime/stream.ts:84-89`, `Alis.Reactive.Assets/runtime/realtime/hub.ts:155-160` at
  65fdf118d), the meaning the rewrite keeps.
- A stream is held as a hub is (D21.6, X21.2). The redesign holds a hub for every plan that names it
  (`Alis.Reactive/PlanAuthoring/Pipelines/PipelineBuilder.cs:212-214`,
  `Alis.Reactive.Assets/runtime/realtime/hub.ts:95-108` at 65fdf118d) and a stream only through its subscriptions,
  reading it closed when none holds it (`Alis.Reactive.Assets/runtime/realtime/stream.ts:60-70`,
  `Alis.Reactive.Assets/runtime/realtime/stream.ts:75` at 65fdf118d). Main holds both through their subscriptions and
  releases a hub without withdrawing its offer (`Alis.Reactive.Assets/runtime/execution/realtime/signalr.ts:134-141`
  at 623dd4a27, X12.6).
- The lifecycle triggers are one closed set on both transports (D21.5, X21.3). Main raises none; the redesign raises
  them for a hub alone (`Alis.Reactive.Assets/runtime/realtime/hub.ts:71-88`,
  `Alis.Reactive.Assets/runtime/realtime/hub.ts:116` at 65fdf118d). Neither runs closed steps for a first start that
  gave up (`Alis.Reactive.Assets/runtime/execution/realtime/signalr.ts:38-40` at 623dd4a27,
  `Alis.Reactive.Assets/runtime/realtime/hub.ts:39-40` at 65fdf118d), and the redesign's retry after closed raises no
  `reconnected` (`Alis.Reactive.Assets/runtime/realtime/hub.ts:43-46` at 65fdf118d): M10.D1 and M10.D2 state both.
  Main waits 0, 2, 10 and 30 seconds between first starts and the redesign 0, 2 and 10
  (`Alis.Reactive.Assets/runtime/execution/realtime/signalr.ts:22` at 623dd4a27,
  `Alis.Reactive.Assets/runtime/realtime/hub.ts:21` at 65fdf118d); M10.D1 takes the client's own reconnect delays.
- The reconnect offer is a contract a component fulfils, counted across the page (D21.11, X21.9, X21.10, X21.12). Both
  lines find one layout element by its id (`Alis.Reactive.Assets/runtime/realtime/retry-indicator.ts:5` at 65fdf118d,
  `Alis.Reactive.Assets/runtime/execution/realtime/retry-indicator.ts:12` at 623dd4a27), log a missing one only at the
  first outage (`Alis.Reactive.Assets/runtime/realtime/retry-indicator.ts:10-17` at 65fdf118d), and hide it on a click
  before any connection opens (`Alis.Reactive.Assets/runtime/realtime/retry-indicator.ts:47-53` at 65fdf118d), which
  the redesign's test pins (`tests/Alis.Reactive.Framework.PlaywrightTests/Realtime/WhenTheServerDropsAHub.cs:99-108`
  at 65fdf118d). The offer stays until a connection opens, and the startup choice refuses a page with none (X21.11).
- The hub's client is its own asset (D21.1, X21.5). Both lines ship the SignalR client in the framework's script
  (`Alis.Reactive/PlanAuthoring/Events/SignalRHub.cs:28`, `Alis.Reactive.Assets/runtime/realtime/hub.ts:1` at
  65fdf118d; `Alis.Reactive.Assets/runtime/execution/realtime/signalr.ts:1` at 623dd4a27), so a page with no hub loads
  it.
- Realtime is net10's alone (D21.2, X21.7). The redesign puts every realtime door on net48's surface too
  (`Alis.Reactive/PublicAPI/net48/PublicAPI.Unshipped.txt:586` at 65fdf118d), where `@microsoft/signalr` reaches no
  ASP.NET SignalR server; N21.6 keeps them on net10's text alone.
- A hub call made before its first start settles waits for it (M10.D3). The redesign awaits the hub's start before
  each `invoke` and `send` (`Alis.Reactive.Assets/runtime/realtime/hub.ts:128-135` at 65fdf118d), which M10.D3 keeps
  and bounds: during a reconnection and after closed the call is rejected and its pipeline stops (D11.3). Main has no
  `invoke`.
- A stream a step closes stays closed and is no outage (M10.D4). The redesign's `Close` closes the browser's source
  and leaves the stream held with its handlers subscribed
  (`Alis.Reactive/PlanAuthoring/Events/ServerPushStream.cs:52-53`,
  `Alis.Reactive.Assets/runtime/realtime/stream.ts:77` at 65fdf118d), and D21.12 names no close; M10.D4 states it.
