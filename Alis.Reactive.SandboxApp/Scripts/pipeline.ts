import type { HttpReaction, ParallelHttpReaction, ExecContext } from "./types";
import { execRequest } from "./http";
import { executeCommands } from "./commands";
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

  // Fire all requests concurrently — allSettled so one failure doesn't lose others
  const results = await Promise.allSettled(reaction.requests.map(req => execRequest(req, ctx)));
  for (const r of results) {
    if (r.status === "rejected") log.error("parallel branch error", { reason: String(r.reason) });
  }

  // After ALL complete, fire onAllSuccess handlers
  if (reaction.onAllSuccess) {
    for (const handler of reaction.onAllSuccess) {
      executeCommands(handler.commands, ctx);
    }
  }
}

