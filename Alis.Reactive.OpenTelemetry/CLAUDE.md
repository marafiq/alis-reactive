# Alis.Reactive.OpenTelemetry

A consumer of the core: the trace sink a page registers and its layout helper, PackageId AlisReactive.OpenTelemetry.
It stands on the core's diagnostics port alone, through the sink contract `trace` and its one member `ended(span)`;
the core takes no OpenTelemetry package, and a page whose layout links no sink sends no span anywhere.

## The standard

- Its code is one slice, the folder Tracing: the layout helper, `Tracing/OpenTelemetryHtml.Net10.cs` and
  `Tracing/OpenTelemetryHtml.Net48.cs`, and the sink, `Tracing/trace-sink.ts`. The bundle's entry, `opentelemetry.ts`,
  sits beside `SliceTree.txt`, reads the two options the helper writes on its script and registers the sink through
  the page's plugin queue.
- The sink hands OpenTelemetry the span the page recorded, with the page's ids and times; it mints no id and starts
  no span of its own. `Tracing/trace-sink.test.ts` proves it on the SDK's in-memory exporter.
- The bundle holds the OpenTelemetry pins of `spec/REWRITE.md` section 2.7, which move together.
- A file compiles for both hosts or for one, by the core's rule (`Alis.Reactive/CLAUDE.md`). The net48 helper throws
  until module 19's door links a package's file by its content hash on MVC 5 (M19.N1).

## Commands

```bash
dotnet build Alis.Reactive.OpenTelemetry -c Release
npx tsc -b Alis.Reactive.OpenTelemetry
npx vitest run Alis.Reactive.OpenTelemetry
node build.mjs
scripts/check.sh --no-spec
```
