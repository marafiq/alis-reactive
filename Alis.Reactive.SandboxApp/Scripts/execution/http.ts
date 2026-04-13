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
import { tracer } from "../tracing";
import { currentTraceparent } from "../tracing/interactions";

const t = tracer("http");

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

/** Execute a single HTTP request with gather, before, response routing, complete, and chaining. */
export async function executeRequest(req: Request, plan: Plan, ctx?: ExecContext): Promise<void> {
  // Capture W3C traceparent BEFORE any await so the header reflects the
  // interaction currently executing, not whichever interaction happens to
  // be active when the first `await` resolves. See Lesson 2 in the
  // structured tracing plan — this exact ordering was a Phase 2 BLOCK on
  // the abandoned branch.
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

    // 2. Before reactions
    if (req.before) {
      for (const r of req.before) {
        await executeReaction(r, plan, ctx);
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
      await routeHandlers(req.success, response.status, plan, successCtx);
    } else {
      const errorCtx: ExecContext = { ...ctx, response: body ?? undefined };
      await routeHandlers(req.error, response.status, plan, errorCtx);
      await runComplete(req, plan, ctx);
      return; // no chained on error
    }
  } catch (err) {
    const status = err instanceof TypeError ? 0 : -1;
    t.error(
      "http.request.fail",
      { url: req.url, method: req.method, status },
      err instanceof Error ? err : new Error(String(err)),
    );
    await routeHandlers(req.error, status, plan, ctx);
    await runComplete(req, plan, ctx);
    return; // no chained on error
  }

  // 6. Complete
  await runComplete(req, plan, ctx);

  // 7. Chained — only after success
  if (req.next) {
    await executeRequest(req.next, plan, ctx);
  }
}

async function runComplete(req: Request, plan: Plan, ctx?: ExecContext): Promise<void> {
  if (req.complete) {
    for (const r of req.complete) {
      await executeReaction(r, plan, ctx);
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

  // First pass: exact status match
  for (const h of handlers) {
    if (h.status != null && h.status === status) {
      await executeReaction(h.reaction, plan, ctx);
      return;
    }
  }

  // Second pass: default handler (no status)
  for (const h of handlers) {
    if (h.status == null) {
      await executeReaction(h.reaction, plan, ctx);
      return;
    }
  }
}
