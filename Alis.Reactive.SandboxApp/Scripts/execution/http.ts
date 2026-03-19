import type { RequestDescriptor, StatusHandler, ExecContext } from "../types";
import { resolveGather, type GatherResult } from "./gather";
import { executeCommands } from "./commands";
import { executeReaction } from "./execute";
import { scope } from "../core/trace";

const log = scope("http");

interface ResolvedFetch {
  readonly url: string;
  readonly init: RequestInit;
}

function buildFetch(req: RequestDescriptor, gatherResult: GatherResult): ResolvedFetch {
  let url = req.url;
  const init: RequestInit = { method: req.verb };

  if (gatherResult.urlParams.length > 0) {
    const sep = url.includes("?") ? "&" : "?";
    url = url + sep + gatherResult.urlParams.join("&");
  }

  if (req.verb !== "GET") {
    if (gatherResult.body instanceof FormData) {
      init.body = gatherResult.body;
    } else if (Object.keys(gatherResult.body).length > 0) {
      init.headers = { "Content-Type": "application/json" };
      init.body = JSON.stringify(gatherResult.body);
    }
  }

  return { url, init };
}

/** Execute a single HTTP request with gather, whileLoading, response routing, and chaining. */
export async function execRequest(req: RequestDescriptor, ctx?: ExecContext): Promise<void> {
  try {
    // 1. WhileLoading
    if (req.whileLoading) {
      executeCommands(req.whileLoading, ctx);
    }

    // 2. Gather → freeze
    const gatherResult = resolveGather(req.gather ?? [], req.verb, ctx?.components ?? {}, req.contentType);
    const resolved = buildFetch(req, gatherResult);

    log.debug("fetch", { verb: req.verb, url: resolved.url });

    // 3. Fetch
    const response = await fetch(resolved.url, resolved.init);

    // 4. Route response
    const body = await readResponseBody(response);
    if (response.ok) {
      const successCtx: ExecContext = body != null ? { ...ctx, responseBody: body } : ctx ?? {};
      await routeHandlers(req.onSuccess, response.status, successCtx);
    } else {
      const errorCtx: ExecContext = {
        ...ctx,
        responseBody: body ?? undefined,
        validationDesc: req.validation,
      };
      await routeHandlers(req.onError, response.status, errorCtx);
      return; // no chained on error
    }
  } catch (err) {
    const status = err instanceof TypeError ? 0 : -1;
    log.error(status === 0 ? "network error" : "client error", { url: req.url, error: String(err) });
    await routeHandlers(req.onError, status, ctx);
    return; // no chained on error
  }

  // 5. Chained — only after success
  if (req.chained) {
    await execRequest(req.chained, ctx);
  }
}

async function readResponseBody(response: Response): Promise<unknown> {
  const ct = response.headers.get("Content-Type") ?? "";
  if (ct.includes("application/json")) return response.json();
  if (ct.includes("text/")) return response.text();
  if (ct.includes("html")) return response.text();
  return null;
}

export async function routeHandlers(handlers: StatusHandler[] | undefined, status: number, ctx?: ExecContext): Promise<void> {
  if (!handlers || handlers.length === 0) return;

  for (const h of handlers) {
    if (h.statusCode != null && h.statusCode === status) {
      await executeHandler(h, ctx);
      return;
    }
  }

  for (const h of handlers) {
    if (h.statusCode == null) {
      await executeHandler(h, ctx);
      return;
    }
  }
}

async function executeHandler(h: StatusHandler, ctx?: ExecContext): Promise<void> {
  if (h.reaction) {
    await executeReaction(h.reaction, ctx);
  } else if (h.commands) {
    executeCommands(h.commands, ctx);
  }
}

