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
import { scope } from "../core/trace";

const log = scope("http");

interface ResolvedFetch {
  readonly url: string;
  readonly init: RequestInit;
}

function buildFetch(req: Request, gatherResult: GatherResult, plan: Plan, ctx?: ExecContext): ResolvedFetch {
  let url = Object.keys(req.routeParams).length > 0
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
  const headerEntries = Object.entries(req.headers);
  if (headerEntries.length > 0) {
    const existing = (init.headers as Record<string, string>) ?? {};
    for (const [name, producer] of headerEntries) {
      const value = evaluateValue(producer, plan, ctx);
      if (value != null) {
        const shape = "shape" in producer ? producer.shape : { kind: "none" as const };
        const wire = formatForWire(value, shape);
        existing[name] = String(wire);
      }
    }
    init.headers = existing;
  }

  return { url, init };
}

/** Execute a single HTTP request with gather, before, response routing, complete, and chaining. */
export async function executeRequest(req: Request, plan: Plan, ctx?: ExecContext): Promise<void> {
  try {
    // 1. Validation gate (if container specified)
    if (req.container) {
      const valid = validateContainer(plan, req.container, ctx);
      if (!valid) {
        log.debug("validation.aborted", { id: req.container, url: req.url });
        return;
      }
    }

    // 2. Before reactions
    for (const r of req.before) {
      await executeReaction(r, plan, ctx);
    }

    // 3. Gather -> freeze
    const gatherResult = resolveGather(req.input, req.method, plan, ctx);
    const resolved = buildFetch(req, gatherResult, plan, ctx);

    // Write gathered payload to ctx so PayloadSource(scope: "request") resolves correctly
    const requestPayload = gatherResult.body instanceof FormData ? {} : gatherResult.body;
    ctx = { ...ctx, request: requestPayload };

    log.debug("fetch.send", { method: req.method, url: resolved.url });

    // 4. Fetch
    const start = performance.now();
    const response = await fetch(resolved.url, resolved.init);
    log.debug("fetch.response", {
      method: req.method,
      url: resolved.url,
      status: response.status,
      ms: Math.round(performance.now() - start),
    });

    // 5. Route response
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
    log.error(status === 0 ? "fetch.network-error" : "fetch.client-error", { method: req.method, url: req.url, error: String(err) });
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
  for (const r of req.complete) {
    await executeReaction(r, plan, ctx);
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
  handlers: ResponseHandler[],
  status: number,
  plan: Plan,
  ctx?: ExecContext,
): Promise<void> {
  if (handlers.length === 0) return;

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

  // No handler matched — silent miss unless traced.
  log.warn("response.unhandled", { status, handlerCount: handlers.length });
}
