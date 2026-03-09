import type { HttpReaction, ParallelHttpReaction, Command, StatusHandler, ExecContext } from "./types";
import { execRequest } from "./http";
import { scope } from "./trace";

const log = scope("pipeline");

/** Execute an HttpReaction: preFetch → single request. */
export async function executeHttpReaction(reaction: HttpReaction, ctx?: ExecContext): Promise<void> {
  if (reaction.preFetch) {
    executeCommands(reaction.preFetch, ctx);
  }
  await execRequest(reaction.request, ctx);
}

/** Execute a ParallelHttpReaction: preFetch → all requests concurrently → onAllSuccess. */
export async function executeParallelHttpReaction(reaction: ParallelHttpReaction, ctx?: ExecContext): Promise<void> {
  if (reaction.preFetch) {
    executeCommands(reaction.preFetch, ctx);
  }

  log.debug("parallel", { count: reaction.requests.length });

  // Fire all requests concurrently — each handles its own onSuccess/onError
  await Promise.all(reaction.requests.map(req => execRequest(req, ctx)));

  // After ALL complete, fire onAllSuccess handlers
  if (reaction.onAllSuccess) {
    for (const handler of reaction.onAllSuccess) {
      executeCommands(handler.commands, ctx);
    }
  }
}

/** Execute commands synchronously (same logic as http.ts inline executor). */
function executeCommands(commands: Command[], ctx?: ExecContext): void {
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
