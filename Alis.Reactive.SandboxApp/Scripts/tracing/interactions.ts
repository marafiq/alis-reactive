/**
 * Interaction lifecycle — the choke point for span-scoped context.
 *
 * Internal to the tracing module. Not exported from `index.ts`.
 *
 * The runtime calls `run(name, attrs, fn)` at every entry-point to wrap a
 * reaction execution in a trace context. `run` emits `interaction.start`
 * before the call, `interaction.end` on success, or `interaction.fail` on
 * error — synchronous, asynchronous, resolved, rejected, and sync-throw
 * outcomes are all handled. Synchronously nested `run` calls reuse the
 * outer trace context so a sub-reaction inherits the same trace-id.
 *
 * Concurrency model: nesting is detected via a synchronous call-stack
 * depth counter, NOT by reading `current` and reusing whatever is set.
 * The reason — JavaScript is single-threaded but async. While interaction
 * A is awaiting, an unrelated DOM event can fire and start interaction B.
 * If B reused A's trace-id (because `current` was still set), every event
 * from B would be misattributed to A's distributed trace.
 *
 * Async-context discipline for framework code: a single module-global
 * `current` cannot survive arbitrary async resumption order, so files
 * that emit AFTER an `await` must capture the active root at entry via
 * `getCurrentRoot()` and either pin a tracer with `boundTracer(scope, root)`
 * (preferred for the function's own emits) or wrap sub-call invocations
 * with `runWithRoot(root, fn)` so the sub-call's synchronous body runs
 * under the captured root. The pattern chains: every framework async
 * function captures its own root at entry and wraps its awaits, so the
 * correct root propagates through the entire call graph without relying
 * on any global mutable state.
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

export interface InteractionRoot {
  readonly traceId: string;
  readonly flags: string;
}

let current: InteractionRoot | undefined;
/**
 * Monotonic frame id of whichever `run()` invocation last assigned to
 * `current`. Used at finish() time to detect "we still own current" vs
 * "another interaction has taken over since we entered", which lets us
 * avoid clobbering an unrelated interaction when a late-settling async
 * nested run's `prev` is already stale.
 *
 * 0 means "no active owner" — same meaning as `current === undefined`.
 */
let currentOwner = 0;
let depth = 0;
let nextFrameId = 0;
let configuredFromTraceparent: InteractionRoot | undefined;

/**
 * Called by `configure()` when the server plan carries a W3C traceparent.
 * The parsed root is used as the shared root for the entire initial
 * boot phase — every `page-ready` behavior that fires before the first
 * user interaction inherits the same server trace so the full startup
 * batch stitches back to the server response. The boot phase ends
 * on the first non-nested, non-`page-ready` `run()` (a click, an SSE
 * message, an action-link activation) — at that moment the configured
 * root is cleared so subsequent interactions cannot silently inherit
 * a stale page-load trace.
 *
 * Passing undefined clears any pending configured root.
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
 * rejected promise. Errors are rethrown so callers can choose to contain
 * them — `runReaction` in `trigger.ts` is the framework's containment
 * layer for fire-and-forget entry points.
 *
 * Synchronously nested calls (inner `run` invoked from inside outer `run`'s
 * `fn` body, before any `await`) reuse the outer interaction — the inner
 * invocation does not start a new trace-id. Concurrent async interactions
 * (a fresh entry-point firing while another is awaiting) each get their
 * own root, even though `current` may still be set from the awaiting one.
 */
export function run<T>(
  name: string,
  attrs: Record<string, unknown>,
  fn: () => T | Promise<T>,
): T | Promise<T> {
  // True nesting only: an outer run is on the synchronous call stack.
  // Async-concurrent interactions see depth === 0 and get fresh roots.
  const frameId = ++nextFrameId;
  const isNested = depth > 0;
  const prev = current;
  const prevOwner = currentOwner;
  let localRoot: InteractionRoot;
  if (isNested) {
    // Sync nested inside an active interaction — reuse current root so
    // inner dispatches / sub-reactions correlate under one trace.
    localRoot = current!;
  } else if (name === "page-ready" && configuredFromTraceparent) {
    // Initial boot phase. Every `page-ready` behavior (including multiple
    // DomReady behaviors wired from one plan, or from several plans under
    // `composeInitialPlans`) inherits the server-issued trace so the full
    // startup batch correlates back to the server response. We do NOT
    // clear `configuredFromTraceparent` here — other page-ready behaviors
    // in the same boot burst must also inherit.
    localRoot = configuredFromTraceparent;
  } else {
    // Either no server traceparent, or the first non-page-ready entry
    // point has fired (click, SSE, signalr, action-link). Either way
    // this is a fresh client-initiated interaction. If a configured
    // traceparent is still hanging around from boot, clear it now so
    // future runs cannot incorrectly inherit a stale page-load trace.
    configuredFromTraceparent = undefined;
    localRoot = newRoot();
  }
  current = localRoot;
  currentOwner = frameId;

  depth++;
  const t = tracer("interaction");
  const start = performance.now();
  t.debug("interaction.start", { name, ...attrs });

  const finish = (success: boolean, err: unknown, isAsync: boolean): void => {
    // Snapshot the settle-time state BEFORE we install our root for the
    // emit. If another interaction has taken over `current` since we
    // entered `run()`, we must restore to this snapshot instead of
    // writing our stale `prev` back.
    const settleCurrent = current;
    const settleOwner = currentOwner;

    // Temporarily install our root + owner for the emit so the event
    // carries this interaction's trace-id even if another concurrent
    // interaction has updated `current` between our entry and this
    // finish callback firing (out-of-order async completion).
    current = localRoot;
    currentOwner = frameId;
    if (success) {
      t.debug("interaction.end", { name, ms: performance.now() - start });
    } else {
      t.error(
        "interaction.fail",
        { name, ms: performance.now() - start, ...attrs },
        err instanceof Error ? err : new Error(String(err)),
      );
    }

    if (settleOwner === frameId) {
      // We still own `current` at settle time. Safe to restore our
      // recorded prev (for sync nested) or clear (for non-nested).
      //
      // Exception: async runs always clear instead of restoring prev,
      // because by the time an async run settles its outer frame has
      // long since unwound from the synchronous call stack. `prev`
      // captured at our entry refers to a frame that is no longer
      // logically alive; writing it back would leave a stale root
      // ghosting `current` until the next interaction arrives.
      if (isAsync || !isNested) {
        current = undefined;
        currentOwner = 0;
      } else {
        current = prev;
        currentOwner = prevOwner;
      }
    } else {
      // Another frame owns `current`. Our `prev` is stale (or never
      // existed relative to the current owner). Restore the settle-time
      // state exactly and do NOT clobber whoever is active now.
      current = settleCurrent;
      currentOwner = settleOwner;
    }
  };

  let result: T | Promise<T>;
  try {
    result = fn();
  } catch (err) {
    depth--;
    finish(false, err, false);
    throw err;
  }
  // Decrement depth as soon as `fn` returns, so the awaiting promise
  // body is no longer treated as "inside" this run for nesting purposes.
  depth--;

  if (result instanceof Promise) {
    return result.then(
      (value) => {
        finish(true, undefined, true);
        return value;
      },
      (err) => {
        finish(false, err, true);
        throw err;
      },
    );
  }

  finish(true, undefined, false);
  return result;
}

/**
 * Run `fn` synchronously with `root` installed as the active interaction
 * context AND with the synchronous nesting depth incremented for the
 * duration of `fn`. Restores both on exit.
 *
 * Used by framework async functions to re-enter their captured root
 * before invoking sub-calls whose synchronous body would otherwise read
 * whichever unrelated interaction last touched the global `current`.
 *
 * Why depth is bracketed and not just `current`: a re-entered sync body
 * is logically still inside the originating interaction, so any `run()`
 * fired from within that body (e.g., a `dispatch` reaction synchronously
 * triggering a `document-event` listener that calls `runReaction`) must
 * be treated as nested and must reuse the captured root. Without the
 * depth bracket, such a nested `run()` would see `depth === 0`, mint a
 * fresh trace-id, and split what is actually one logical user action
 * into multiple uncorrelated distributed traces.
 *
 * `fn` may itself return a Promise — `runWithRoot` does NOT await it.
 * The synchronous portion of `fn` runs under `root` with depth elevated.
 * If `fn` schedules async work, that work must take responsibility for
 * re-entering its own root (the standard pattern is: capture at entry,
 * wrap awaits with another `runWithRoot`, use `boundTracer` for own
 * emits).
 *
 * If `root` is undefined, `runWithRoot` is a no-op pass-through.
 */
export function runWithRoot<T>(
  root: InteractionRoot | undefined,
  fn: () => T,
): T {
  if (!root) return fn();
  const frameId = ++nextFrameId;
  const prev = current;
  const prevOwner = currentOwner;
  current = root;
  currentOwner = frameId;
  depth++;
  try {
    return fn();
  } finally {
    depth--;
    // Only restore prev if we still own current. If a nested run
    // inside fn took over and did not hand ownership back, leave its
    // state alone.
    if (currentOwner === frameId) {
      current = prev;
      currentOwner = prevOwner;
    }
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
 * Snapshot of the active interaction root, or undefined if none is
 * running. Used by framework async functions to capture root at entry
 * for later re-entry around sub-calls.
 */
export function getCurrentRoot(): InteractionRoot | undefined {
  return current;
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
  currentOwner = 0;
  depth = 0;
  nextFrameId = 0;
  configuredFromTraceparent = undefined;
}
