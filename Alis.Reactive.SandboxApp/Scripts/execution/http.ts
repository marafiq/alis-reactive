// http.ts — HTTP request execution using V3 Request type.
// Uses the SHARED resolver via gather.ts for value gathering.
// Supports before/success/error/complete handlers and chained requests.

import type { Request, ResponseHandler, Plan, ExecContext } from "../types";
import { resolveGather, type GatherResult } from "./gather";
import { executeReaction } from "./execute";
import { validateContainer } from "../validation";
import { evaluateValue } from "../core/evaluate";
import { formatForWire } from "../core/wire-format";
import { resolveRouteParams } from "../core/url-template";
import { boundTracer } from "../tracing/trace";
import {
  currentTraceparent,
  getCurrentRoot,
  runWithRoot,
  type InteractionRoot,
} from "../tracing/interactions";

interface ResolvedFetch {
  readonly url: string;
  readonly init: RequestInit;
}

function buildFetch(req: Request, gatherResult: GatherResult, plan: Plan, ctx?: ExecContext): ResolvedFetch {
  let url = req.routeParams
    ? resolveRouteParams(req.url, req.routeParams, plan, ctx)
    : req.url;
  const init: RequestInit = { method: req.method };

  if (gatherResult.urlParams.length > 0) {
    const sep = url.includes("?") ? "&" : "?";
    url = url + sep + gatherResult.urlParams.join("&");
  }

  if (req.method !== "GET") {
    if (gatherResult.body instanceof FormData) {
      init.body = gatherResult.body;
    } else if (Object.keys(gatherResult.body).length > 0) {
      init.headers = { "Content-Type": "application/json" };
      init.body = JSON.stringify(gatherResult.body);
    }
  }

  // Evaluate and set custom headers from plan ValueProducers.
  // Applied AFTER Content-Type — user headers can override if needed.
  if (req.headers) {
    const existing = (init.headers as Record<string, string>) ?? {};
    for (const [name, producer] of Object.entries(req.headers)) {
      const value = evaluateValue(producer, plan, ctx);
      if (value != null) {
        const wire = formatForWire(value, producer.shape);
        existing[name] = String(wire);
      }
    }
    init.headers = existing;
  }

  return { url, init };
}

/**
 * Execute a single HTTP request with gather, before, response routing,
 * complete, and chaining.
 *
 * Async-context discipline (see interactions.ts JSDoc): `executeRequest`
 * captures the active interaction root at entry, builds a `boundTracer`
 * pinned to that root for its OWN emits, and wraps every sub-call await
 * with `runWithRoot(root, …)` so the sub-call's synchronous body runs
 * under the captured root even if a concurrent unrelated interaction
 * has overwritten the global `current` between awaits.
 */
export async function executeRequest(req: Request, plan: Plan, ctx?: ExecContext): Promise<void> {
  // Capture root + traceparent BEFORE any await. This is the cross-async
  // anchor for the entire request: every emit and every sub-call below
  // re-enters `root` so it cannot be displaced by a concurrent interaction.
  const root: InteractionRoot | undefined = getCurrentRoot();
  const t = boundTracer("http", root);
  const tp = currentTraceparent();

  try {
    // 1. Validation gate (if container specified)
    if (req.container) {
      const valid = validateContainer(plan, req.container, ctx);
      if (!valid) {
        t.debug("http.validation.fail", { url: req.url, method: req.method });
        return;
      }
    }

    // 2. Before reactions — sub-reactions need root re-entered for their
    //    sync bodies' emits.
    if (req.before) {
      for (const r of req.before) {
        await runWithRoot(root, () => executeReaction(r, plan, ctx));
      }
    }

    // 3. Gather -> freeze
    const gatherResult = resolveGather(req.input, req.method, plan, ctx);
    const resolved = buildFetch(req, gatherResult, plan, ctx);

    // 4. Inject traceparent header if an interaction is active
    if (tp) {
      const headers = (resolved.init.headers as Record<string, string>) ?? {};
      headers["traceparent"] = tp;
      (resolved.init as { headers: Record<string, string> }).headers = headers;
    }

    // Write gathered payload to ctx so PayloadSource(scope: "request") resolves correctly
    const requestPayload = gatherResult.body instanceof FormData ? {} : gatherResult.body;
    ctx = { ...ctx, request: requestPayload };

    // 5. Fetch
    const start = performance.now();
    t.debug("http.request.send", { method: req.method, url: resolved.url });
    const response = await fetch(resolved.url, resolved.init);
    t.debug("http.response", {
      method: req.method,
      url: resolved.url,
      status: response.status,
      ms: performance.now() - start,
    });

    // 6. Route response
    const body = await readResponseBody(response);
    if (response.ok) {
      const successCtx: ExecContext = { ...ctx, response: body ?? undefined };
      await runWithRoot(root, () => routeHandlers(req.success, response.status, plan, successCtx));
    } else {
      const errorCtx: ExecContext = { ...ctx, response: body ?? undefined };
      await runWithRoot(root, () => routeHandlers(req.error, response.status, plan, errorCtx));
      await runWithRoot(root, () => runComplete(req, plan, ctx));
      return; // no chained on error
    }
  } catch (err) {
    const status = err instanceof TypeError ? 0 : -1;
    t.error(
      "http.request.fail",
      { url: req.url, method: req.method, status },
      err instanceof Error ? err : new Error(String(err)),
    );
    await runWithRoot(root, () => routeHandlers(req.error, status, plan, ctx));
    await runWithRoot(root, () => runComplete(req, plan, ctx));
    return; // no chained on error
  }

  // 7. Complete
  await runWithRoot(root, () => runComplete(req, plan, ctx));

  // 8. Chained — only after success
  if (req.next) {
    await runWithRoot(root, () => executeRequest(req.next!, plan, ctx));
  }
}

async function runComplete(req: Request, plan: Plan, ctx?: ExecContext): Promise<void> {
  // Capture root at entry so awaited sub-reaction emits stay correlated.
  const root = getCurrentRoot();
  if (req.complete) {
    for (const r of req.complete) {
      await runWithRoot(root, () => executeReaction(r, plan, ctx));
    }
  }
}

async function readResponseBody(response: Response): Promise<unknown> {
  const ct = response.headers.get("Content-Type") ?? "";
  if (ct.includes("application/json")) return response.json();
  if (ct.includes("text/")) return response.text();
  if (ct.includes("html")) return response.text();
  return null;
}

export async function routeHandlers(
  handlers: ResponseHandler[] | undefined,
  status: number,
  plan: Plan,
  ctx?: ExecContext,
): Promise<void> {
  if (!handlers || handlers.length === 0) return;

  // Capture root at entry — sub-reactions called via `await executeReaction`
  // need the captured root re-entered for their sync-body emits.
  const root = getCurrentRoot();

  // First pass: exact status match
  for (const h of handlers) {
    if (h.status != null && h.status === status) {
      await runWithRoot(root, () => executeReaction(h.reaction, plan, ctx));
      return;
    }
  }

  // Second pass: default handler (no status)
  for (const h of handlers) {
    if (h.status == null) {
      await runWithRoot(root, () => executeReaction(h.reaction, plan, ctx));
      return;
    }
  }
}
