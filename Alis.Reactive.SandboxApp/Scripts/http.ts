import type { RequestDescriptor, StatusHandler, ExecContext } from "./types";
import { resolveGather } from "./gather";
import { executeCommands } from "./commands";
import { register, validate } from "./forms";
import { scope } from "./trace";

const log = scope("http");

/** Execute a single HTTP request with gather, whileLoading, response routing, and chaining. */
export async function execRequest(req: RequestDescriptor, ctx?: ExecContext): Promise<void> {
  // 0. Pre-request validation — register + validate, abort if fails
  if (req.validation) {
    register(req.validation);
    if (!validate(req.validation.formId)) {
      log.debug("validation failed, aborting request");
      return;
    }
  }

  // 1. WhileLoading — execute commands immediately (revert is the caller's responsibility via onSuccess/onError)
  if (req.whileLoading) {
    executeCommands(req.whileLoading, ctx);
  }

  // 2. Gather
  const gatherResult = resolveGather(req.gather ?? [], req.verb);

  // 3. Build fetch options
  let url = req.url;
  const init: RequestInit = { method: req.verb };

  if (gatherResult.urlParams.length > 0) {
    const sep = url.includes("?") ? "&" : "?";
    url = url + sep + gatherResult.urlParams.join("&");
  }

  if (req.verb !== "GET" && Object.keys(gatherResult.body).length > 0) {
    init.headers = { "Content-Type": "application/json" };
    init.body = JSON.stringify(gatherResult.body);
  }

  log.debug("fetch", { verb: req.verb, url });

  try {
    // 4. Execute fetch
    const response = await fetch(url, init);

    // 5. Route response — thread response body for error handlers (validation errors)
    if (response.ok) {
      routeHandlers(req.onSuccess, response.status, ctx);
    } else {
      let errorBody: unknown;
      try { errorBody = await response.json(); } catch { /* no JSON body */ }
      const errorCtx = errorBody ? { ...ctx, responseBody: errorBody } : ctx;
      routeHandlers(req.onError, response.status, errorCtx);
    }

    // 6. Chained request — fires after current request completes successfully
    if (req.chained && response.ok) {
      await execRequest(req.chained, ctx);
    }
  } catch (err) {
    log.error("network error", { url, error: String(err) });
    // Route to catch-all error handler if available
    routeHandlers(req.onError, 0, ctx);
  }
}

function routeHandlers(handlers: StatusHandler[] | undefined, status: number, ctx?: ExecContext): void {
  if (!handlers || handlers.length === 0) return;

  // Try specific status match first
  for (const h of handlers) {
    if (h.statusCode != null && h.statusCode === status) {
      executeCommands(h.commands, ctx);
      return;
    }
  }

  // Fall through to catch-all (no statusCode)
  for (const h of handlers) {
    if (h.statusCode == null) {
      executeCommands(h.commands, ctx);
      return;
    }
  }
}

