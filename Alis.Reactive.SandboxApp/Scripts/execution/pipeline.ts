import type { HttpReaction, ParallelHttpReaction, RequestDescriptor, ExecContext } from "../types";
import { execRequest, routeHandlers } from "./http";
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
  try {
    if (reaction.preFetch) {
      executeCommands(reaction.preFetch, ctx);
    }
    if (!passesValidation(reaction.request)) return;
  } catch (err) {
    log.error("pre-request error", { error: String(err) });
    await routeHandlers(reaction.request.onError, -1, ctx);
    return;
  }
  await execRequest(reaction.request, ctx);
}

/** Execute a ParallelHttpReaction: preFetch → validate each → all requests concurrently → onAllSettled. */
export async function executeParallelHttpReaction(reaction: ParallelHttpReaction, ctx?: ExecContext): Promise<void> {
  try {
    if (reaction.preFetch) {
      executeCommands(reaction.preFetch, ctx);
    }
  } catch (err) {
    log.error("pre-request error", { error: String(err) });
    return;
  }

  const validRequests = reaction.requests.filter(req => passesValidation(req));

  log.debug("parallel", { count: validRequests.length });

  await Promise.all(validRequests.map(req => execRequest(req, ctx)));

  if (reaction.onAllSettled) {
    executeCommands(reaction.onAllSettled, ctx);
  }
}

