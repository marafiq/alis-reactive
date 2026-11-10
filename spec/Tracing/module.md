# Tracing

How the framework records a request from start to end, in the browser and on the server, by OpenTelemetry: the
plan's trace context, which the server decides; the browser's diagnostics port, with its closed spans and events and
its two sinks; the trace context each send attempt carries to the page's own origin; and the server's one activity
source. Tracing is optional and never always on. Module 26 of `spec/CLAUDE.md`.

## Core questions

- Q20: tracing is off unless the server recorded the render (D20.1); the core takes no telemetry library, and its one
  port has two sinks, the console and the OpenTelemetry adapter, a separate package the core names nowhere (D20.2);
  one trace runs from start to end by W3C Trace Context, its headers to the page's own origin only (D20.3); the spans
  are closed and a synchronous statement is an event (D20.4); no value is recorded (D20.5); every span ends once,
  after its children, and a sink's error is the one error the port catches (D20.6); under `on` each handler's source
  is written (D20.7); the server records on one `ActivitySource` (D20.8); tracing changes nothing a plan does
  (D20.9); a retry is always traced, with its resend count (D20.11); and its proof is Playwright on the sandbox with
  an echoing endpoint and a collector page (D20.10). X20.1 to X20.7 are placed here, and M26.D1 states the port's
  sink contract.
- Q19: the plan's `trace` is `off`, or `on` with `traceparent`, `tracestate`, `console` and one `Source` per handler,
  its view and its line (N19.3).
- Q11: a run ends completed, stopped or replaced (D11.7), and dispose ends its owner's runs first (D11.8); its span
  records that outcome.
- Q12: a slot's load and unload end the runs of the owners inside it (D12.4), which are spans of their own.
- Q10: the standard retry (D10.14, N10.3), whose attempts are spans and whose retries carry the trace.
- Q9: a request's own header named `traceparent`, `tracestate` or `baggage` is refused (D9.11), and the browser adds
  no header but the trace headers (D9.12).
- Q18: ALIS030 refuses those header names at build, and the view's render refuses them on code ALIS030 does not
  follow (D18.6, N18.7).
- Q21: a run a realtime message starts is a child of the plan's trace context (D21.8).

D20.7's first proof is the rewrite unit's: that each host's view compiler writes a `#line` directive at a builder
call (`probe:core-caller-line-follows-line-directive` pins the compiler's half on net10); D20.7 states the default
when a host writes none, the handler's position among its view's handlers, in the same integer.

## Boundaries

The plan's grammar is module 14's: the `trace` field, its kinds `off` and `on` and the `Source` record (N19.3). The
request, its attempts and the standard retry are module 6's (D10.14, N10.3), and the request analyzer ALIS030 is
module 20's (N18.7); this module places the trace header refusal X20.6, whose subject is tracing. A run's policy is
module 1's (D11.7) and its ending at dispose module 17's (D11.8), a slot's load and unload module 9's (D12.4), the validation door module
8's and the error door module 6's (N10.1); each records its span or activity through this module's doors. Realtime's
messages are module 10's (D21.8). The contracts `busy` and `notice` are module 6's (D10.21); the sink contract
`trace` follows their form and is this module's (M26.D1). Rendering a plan and linking an asset by its content hash
are module 19's.

## Doors

Each name is gate 2's; each door is the capability it carries. Neither line traces (`spec/Core/DESIGN.md`'s Q20
evidence), so no inventory row is this module's, and its doors are the core's three, each new.

- `N20.1` new: the browser's diagnostics port, internal, with its closed span and event vocabulary (D20.4) and its
  two sinks: the console, and the object a page registers under the sink contract `trace` (M26.D1). No view, validator
  or component author calls it.
- `N20.2` new: the OpenTelemetry adapter, a separate package built on the port that pins `@opentelemetry/api`, the
  web SDK and the OTLP HTTP exporter (D20.2), and fulfils the sink contract `trace`. Its name and its configuration
  are gate 2's. The default: the package `Alis.Reactive.OpenTelemetry`, beside `Alis.Reactive.Fusion` and
  `Alis.Reactive.Native`, whose one browser asset is the adapter, linked by its content-hash URL as every framework
  asset is (module 19), and whose one door is a layout helper the app calls once, taking the OTLP HTTP traces URL and
  the service name the app names, which writes the asset's link and those two options as its element's data.
- `N20.3` new: the `ActivitySource` "Alis.Reactive" with its activities for rendering a plan, the validation door and
  the error door (D20.8), and the app's tracing options, `console` among them, on both hosts. Their form is gate 2's.
  The default: the source's name as a public constant, `ReactiveTracing.SourceName`, so an app's tracer provider
  names it with no text of its own; `console` a boolean of the framework's one startup call (N16.4's), false unless
  the app sets it, which the docs set from the host's development setting.

A second gate 2 item is the span and activity names, the vocabulary an operator reads in a trace. The default: each
attempt named by OpenTelemetry's HTTP client convention, its method and its route template (D20.4), and every other
span and activity named `alis.` and its kind: `alis.boot`, `alis.run`, `alis.send`, `alis.method`, `alis.confirm`,
`alis.hub`, `alis.all`, `alis.slot.load`, `alis.slot.unload`, `alis.render`, `alis.validate` and `alis.error`.

A third gate 2 item is M26.D1's sink contract. The default: the contract name `trace`, registered through the page's
plugin queue as `busy` and `notice` are (D10.21), its one member `ended(span)`, and a sink whose call throws logged
once and called no more on the page. The last goes past D20.6's words, which say only that the error is logged once
and the run goes on; it is the one catch D20.6 allows, against the owner's fail loudly (`spec/NOTES.md`, 2026-11-06).

The meaning `M26.D1` is in `spec/Tracing/features.json`, on the feature it gives.

## Improvements

- No app data leaves the page through its diagnostics (D20.5). Main writes `data-trace="trace"` on every plan
  (`Alis.Reactive/Razor/Extensions/PlanExtensions.cs:98` at 623dd4a27,
  `Alis.Reactive/Razor/Extensions/PlanExtensions.cs:115` at 623dd4a27), and its runtime takes its level from it
  (`Alis.Reactive.Assets/runtime/root.ts:61-62` at 623dd4a27), so every page logs at trace level, and its log lines
  carry the value a step sets, the values a condition compares and the arguments a call passes
  (`Alis.Reactive.Assets/runtime/execution/reactions/execute.ts:159` at 623dd4a27,
  `Alis.Reactive.Assets/runtime/execution/reactions/execute.ts:185` at 623dd4a27,
  `Alis.Reactive.Assets/runtime/conditions/compare-engine.ts:176` at 623dd4a27). The redesign takes its level from
  the page's own attribute, warn when absent (`Alis.Reactive.Assets/runtime/diagnostics/trace.ts:13-18` at
  65fdf118d), and at trace prints the value a step sets and the value a failed rule read
  (`Alis.Reactive.Assets/runtime/grammar/execute.ts:38` at 65fdf118d,
  `Alis.Reactive.Assets/runtime/plugins/form/form.ts:75` at 65fdf118d), and at debug each URL with its query
  (`Alis.Reactive.Assets/runtime/plugins/http.ts:30` at 65fdf118d). Both lines print the details they are given
  (`Alis.Reactive.Assets/runtime/diagnostics/trace.ts:40` at 623dd4a27,
  `Alis.Reactive.Assets/runtime/diagnostics/trace.ts:46` at 65fdf118d). A span records ids, kinds, names, the route
  template, the status, the attempt and the outcome only.
- Whether a page traces is the server's decision (D20.1, X20.1). Both lines decide a level in the page, main in its
  plan's markup for every page and the redesign by an attribute the app's layout writes
  (`Alis.Reactive.SandboxApp/Views/Shared/_Layout.cshtml:2` at 65fdf118d); the rewrite writes `on` only for a render
  the server recorded, so the page's trace and the server's sampling agree.
- A retry reaches the server traced (D20.11, X20.7). Neither line sends a trace header: main's fetch and the
  redesign's take only the request's own headers
  (`Alis.Reactive.Assets/runtime/execution/requests/http.ts:92` at 623dd4a27,
  `Alis.Reactive.Assets/runtime/plugins/http.ts:88-95` at 65fdf118d), so a retried request is recorded only when
  the server's own sampler happens to pick it.
- The trace headers are the framework's (D9.11, X20.6). Both lines' `Header(name, value)` refuses only an empty name
  (`Alis.Reactive/PlanModel/WireTerms/PlanTerms.cs:199-203` at 623dd4a27,
  `Alis.Reactive/WireTerms/PlanTerms.cs:87-91` at 65fdf118d), so a view sends a `traceparent` of its own.
- The trace headers stay home (D20.3, X20.4). Chromium sends a cross-origin GET that carries `traceparent` only after
  an OPTIONS preflight asking for it, and when the other origin does not allow the header the GET is never sent and
  fetch rejects with a TypeError (`probe:browser-cross-origin-traceparent-preflight`); the same GET without it is
  sent with no preflight (`probe:browser-cross-origin-plain-get-no-preflight`), and to the page's own origin the
  browser sends `traceparent`, `tracestate` and `baggage` as written (`probe:browser-same-origin-trace-headers-sent`).
  A runtime that sent them everywhere would hand the page's trace id to another party and break that party's GETs.
