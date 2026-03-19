import type { HttpReaction, ParallelHttpReaction, RequestDescriptor, ExecContext } from "../types";
import { execRequest } from "./http";
import { executeCommands } from "./commands";
import { validate } from "../validation";
import { scope } from "../core/trace";

const log = scope("pipeline");

/** Pre-request validation gate. Returns true if the request should proceed. */
function passesValidation(req: RequestDescriptor): boolean {
  if (!req.validation) return true;
  if (!validate(req.validation)) {
    log.debug("validation failed, aborting request");
    return false;
  }
  return true;
}

/** Execute an HttpReaction: preFetch → validate → single request. */
export async function executeHttpReaction(reaction: HttpReaction, ctx?: ExecContext): Promise<void> {
  if (reaction.preFetch) {
    executeCommands(reaction.preFetch, ctx);
  }
  if (!passesValidation(reaction.request)) return;
  await execRequest(reaction.request, ctx);
}

/** Execute a ParallelHttpReaction: preFetch → validate each → all requests concurrently → onAllSuccess. */
export async function executeParallelHttpReaction(reaction: ParallelHttpReaction, ctx?: ExecContext): Promise<void> {
  if (reaction.preFetch) {
    executeCommands(reaction.preFetch, ctx);
  }

  // Validate each request independently — only fire those that pass
  const validRequests = reaction.requests.filter(req => passesValidation(req));

  log.debug("parallel", { count: validRequests.length });

  // Fire all valid requests concurrently — allSettled so one failure doesn't lose others
  const results = await Promise.allSettled(validRequests.map(req => execRequest(req, ctx)));
  for (const r of results) {
    if (r.status === "rejected") log.error("parallel branch error", { reason: String(r.reason) });
  }

  if (reaction.onAllSettled) {
    executeCommands(reaction.onAllSettled, ctx);
  }
}

