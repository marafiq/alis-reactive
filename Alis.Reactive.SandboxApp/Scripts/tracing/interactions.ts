/**
 * Interaction lifecycle — the choke point for span-scoped context.
 *
 * Internal to the tracing module. Not exported from `index.ts`.
 *
 * The runtime calls `run(name, attrs, fn)` at every entry-point to wrap a
 * reaction execution in a trace context. `run` emits `interaction.start`
 * before the call, `interaction.end` on success, or `interaction.fail` on
 * error — synchronous, asynchronous, resolved, rejected, and sync-throw
 * outcomes are all handled. Nested `run` calls reuse the outer trace
 * context so the same trace-id threads through sub-reactions.
 *
 * `currentTraceparent()` is read by `http.ts` immediately before `fetch`
 * (and before any `await`) to inject a W3C header. When no interaction is
 * active, it returns undefined and nothing is injected.
 *
 * No `Span` type is exposed and no `.end()` method exists. The lifecycle
 * is private to this module; consumers only ever see start/end/fail events.
 */

import {
  formatTraceparent,
  generateSpanId,
  generateTraceId,
  parseTraceparent,
} from "./context";
import { tracer } from "./trace";

interface InteractionRoot {
  readonly traceId: string;
  readonly flags: string;
}

let current: InteractionRoot | undefined;
let configuredFromTraceparent: InteractionRoot | undefined;

/**
 * Called by `configure()` when the server plan carries a W3C traceparent.
 * The parsed root becomes the default for any interaction that starts
 * without an existing context — so a server-initiated request thread
 * can continue into the client runtime without losing correlation.
 *
 * Passing undefined clears the configured root.
 */
export function setRootFromTraceparent(traceparent: string | undefined): void {
  if (!traceparent) {
    configuredFromTraceparent = undefined;
    return;
  }
  const parsed = parseTraceparent(traceparent);
  configuredFromTraceparent = parsed
    ? { traceId: parsed.traceId, flags: parsed.flags }
    : undefined;
}

/**
 * Wrap an entry-point execution in an interaction.
 *
 * Emits `interaction.start` before calling `fn`, then `interaction.end`
 * on success or `interaction.fail` on error. Handles all four outcome
 * shapes: synchronous return, synchronous throw, resolved promise, and
 * rejected promise. Errors are rethrown so callers see them as usual.
 *
 * Nested calls to `run` reuse the outer interaction — the inner
 * invocation does not start a new trace-id. This keeps the entire
 * reaction tree correlated under one trace.
 */
export function run<T>(
  name: string,
  attrs: Record<string, unknown>,
  fn: () => T | Promise<T>,
): T | Promise<T> {
  const prev = current;
  current = current ?? configuredFromTraceparent ?? newRoot();
  const t = tracer("interaction");
  const start = performance.now();
  t.debug("interaction.start", { name, ...attrs });

  const onSuccess = (): void => {
    t.debug("interaction.end", { name, ms: performance.now() - start });
    current = prev;
  };

  const onFailure = (err: unknown): void => {
    t.error(
      "interaction.fail",
      { name, ms: performance.now() - start, ...attrs },
      err instanceof Error ? err : new Error(String(err)),
    );
    current = prev;
  };

  try {
    const result = fn();
    if (result instanceof Promise) {
      return result.then(
        (value) => {
          onSuccess();
          return value;
        },
        (err) => {
          onFailure(err);
          throw err;
        },
      );
    }
    onSuccess();
    return result;
  } catch (err) {
    onFailure(err);
    throw err;
  }
}

/**
 * The current W3C traceparent header for the active interaction, or
 * undefined if no interaction is running.
 *
 * HTTP integration MUST call this synchronously before any `await` so
 * the captured header reflects the interaction that initiated the
 * request, not whichever interaction happens to be running when the
 * promise resolves.
 *
 * A fresh span-id is generated on every call; the trace-id is constant
 * for the lifetime of the interaction.
 */
export function currentTraceparent(): string | undefined {
  if (!current) return undefined;
  return formatTraceparent(current.traceId, generateSpanId(), current.flags);
}

/**
 * The trace-id of the active interaction, or undefined if none is
 * running. Read lazily by `trace.ts` at emit time so events carry the
 * trace-id of the interaction currently executing.
 */
export function getCurrentTraceId(): string | undefined {
  return current?.traceId;
}

/**
 * A fresh span-id for each emission. Emitted events do not share a
 * span-id within an interaction — each event is its own span point.
 */
export function getCurrentSpanId(): string | undefined {
  return current ? generateSpanId() : undefined;
}

function newRoot(): InteractionRoot {
  return { traceId: generateTraceId(), flags: "01" };
}

/**
 * Reset module-level state. Test-only hook — production code must not
 * call this. Exists so vitest files can isolate cases from each other.
 */
export function resetForTests(): void {
  current = undefined;
  configuredFromTraceparent = undefined;
}
