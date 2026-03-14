# DESIGN-012: data-trace="trace" Is Hardcoded — Trace Always Enabled in Production

## Status: Design Issue

## File
`Alis.Reactive.Native/Extensions/PlanExtensions.cs:43`

## How to Reproduce

1. Deploy the application to production.
2. Open browser DevTools → Console.
3. Every reactive interaction emits trace messages:
   ```
   [alis:trigger] custom-event: listening {"event":"submit"}
   [alis:execute] sequential {"commands":3}
   [alis:element] exec {"target":"status","jsEmit":"el.textContent = val","val":"loaded"}
   ```
4. This happens because `RenderPlan()` hardcodes `data-trace="trace"`:
   ```csharp
   $"<script ... data-trace=\"trace\">{json}</script>"
   ```
5. `auto-boot.ts:13` reads this attribute and calls `trace.setLevel("trace")` — the most verbose level.

## Deep Reasoning: Why This Is a Design Issue

The `data-trace` attribute was added for development and Playwright testing — tests use `WaitForTraceMessage()` and `AssertTraceContains()` to verify runtime behavior by reading console output.

Hardcoding it to `"trace"` means:
1. **Performance**: Every command execution emits `console.log` calls with `JSON.stringify`. In a form with 50 fields and live validation, this generates hundreds of console messages per user interaction.
2. **Information leakage**: Trace output reveals internal plan structure, event names, payload data, and component IDs to anyone who opens DevTools. In a sensitive application (banking, healthcare), this is an information disclosure concern.
3. **Console noise**: Developers using DevTools for their own debugging have their console flooded with alis trace messages, making it harder to find their own logs.

The framework's `trace.ts` architecture already supports all 6 levels (`off`, `error`, `warn`, `info`, `debug`, `trace`). The infrastructure for conditional tracing exists — only the `RenderPlan` output is hardcoded.

## How Fixing This Improves the Codebase

1. **Production safety**: Default to `data-trace="off"` or omit the attribute entirely (auto-boot already handles missing attribute — line 12-13 in auto-boot.ts: `if (traceLevel) trace.setLevel(traceLevel)`).
2. **Developer control**: Make trace level configurable via `IReactivePlan` or an environment-based flag (e.g., `IHostEnvironment.IsDevelopment()`).
3. **Playwright tests**: Tests can continue to work by either setting `data-trace="trace"` explicitly in the test sandbox layout, or by configuring the plan to emit trace in the test environment.

## How This Fix Will Not Break Existing Features

- Playwright tests currently rely on trace messages for assertions. The fix should ensure the sandbox app's layout (used by Playwright) still emits `data-trace="trace"`. This can be done by making the trace level configurable and setting it to "trace" in the sandbox project.
- The `auto-boot.ts` code already handles the absence of `data-trace` — `setLevel` is simply not called, and `active` remains `LEVELS.off` (the default on line 7 of trace.ts).
- No runtime behavior depends on trace — it is purely observational logging.
