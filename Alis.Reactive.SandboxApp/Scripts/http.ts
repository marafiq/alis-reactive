import type { RequestDescriptor, StatusHandler, Command, ExecContext } from "./types";
import { resolveGather } from "./gather";
import { scope } from "./trace";

const log = scope("http");

/** Execute a single HTTP request with gather, whileLoading, response routing, and chaining. */
export async function execRequest(req: RequestDescriptor, ctx?: ExecContext): Promise<void> {
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

    // 5. Route response
    if (response.ok) {
      routeHandlers(req.onSuccess, response.status, ctx);
    } else {
      routeHandlers(req.onError, response.status, ctx);
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

/** Execute a list of commands synchronously — delegates to the same execution logic as sequential reactions. */
function executeCommands(commands: Command[], ctx?: ExecContext): void {
  // Inline execution to avoid circular dependency with execute.ts
  for (const cmd of commands) {
    switch (cmd.kind) {
      case "dispatch":
        document.dispatchEvent(new CustomEvent(cmd.event, { detail: cmd.payload ?? {} }));
        break;
      case "mutate-element": {
        const el = document.getElementById(cmd.target);
        if (!el) break;
        const val = cmd.value;
        new Function("el", "val", cmd.jsEmit)(el, val);
        break;
      }
    }
  }
}
