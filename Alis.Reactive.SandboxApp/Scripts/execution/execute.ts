import type { Reaction, ExecContext } from "../types";
import { scope } from "../core/trace";
import { executeCommand } from "./commands";
import { evaluateGuard, evaluateGuardAsync, isConfirmGuard } from "../conditions/conditions";
import { executeHttpReaction, executeParallelHttpReaction } from "../http/pipeline";
import { assertNever } from "../core/assert-never";

const log = scope("execute");

/**
 * Checks whether a reaction tree contains any ConfirmGuard anywhere.
 * Used at the top of executeReaction to route to the async path once,
 * rather than duplicating the check inside each reaction kind.
 */
function needsAsync(reaction: Reaction): boolean {
  if (reaction.kind === "conditional") {
    return reaction.branches.some(b =>
      (b.guard != null && isConfirmGuard(b.guard)) || needsAsync(b.reaction)
    );
  }
  return false;
}

export function executeReaction(reaction: Reaction, ctx?: ExecContext): void {
  // Single async check at the top — if any guard in the tree is a ConfirmGuard,
  // delegate to the async path. Zero overhead for the common sync case.
  if (needsAsync(reaction)) {
    dispatchAsync(reaction, ctx);
    return;
  }

  switch (reaction.kind) {
    case "sequential":
      log.debug("sequential", { commands: reaction.commands.length });
      for (const cmd of reaction.commands) {
        executeCommand(cmd, ctx);
      }
      break;

    case "conditional":
      log.debug("conditional", { commands: reaction.commands?.length ?? 0, branches: reaction.branches.length });
      if (reaction.commands) {
        for (const cmd of reaction.commands) {
          executeCommand(cmd, ctx);
        }
      }
      for (const branch of reaction.branches) {
        if (branch.guard == null || evaluateGuard(branch.guard, ctx)) {
          log.trace("branch-taken", { guard: branch.guard?.kind ?? "else" });
          executeReaction(branch.reaction, ctx);
          return;
        }
      }
      log.trace("no-branch-taken");
      break;

    case "http":
      log.debug("http", { url: reaction.request.url });
      executeHttpReaction(reaction, ctx);
      break;

    case "parallel-http":
      log.debug("parallel-http", { count: reaction.requests.length });
      executeParallelHttpReaction(reaction, ctx);
      break;

    default:
      assertNever(reaction, "reaction kind");
  }
}

/**
 * Async execution path — only invoked when needsAsync() detects a ConfirmGuard
 * somewhere in the reaction tree. Zero overhead for non-confirm paths.
 */
async function dispatchAsync(reaction: Reaction, ctx?: ExecContext): Promise<void> {
  switch (reaction.kind) {
    case "sequential":
      log.debug("sequential", { commands: reaction.commands.length });
      for (const cmd of reaction.commands) {
        executeCommand(cmd, ctx);
      }
      return;

    case "conditional":
      log.debug("conditional", { commands: reaction.commands?.length ?? 0, branches: reaction.branches.length });
      if (reaction.commands) {
        for (const cmd of reaction.commands) {
          executeCommand(cmd, ctx);
        }
      }
      for (const branch of reaction.branches) {
        if (branch.guard == null) {
          await dispatchAsync(branch.reaction, ctx);
          return;
        }
        if (await evaluateGuardAsync(branch.guard, ctx)) {
          log.trace("branch-taken", { guard: branch.guard.kind });
          await dispatchAsync(branch.reaction, ctx);
          return;
        }
      }
      log.trace("no-branch-taken");
      return;

    case "http":
      log.debug("http", { url: reaction.request.url });
      await executeHttpReaction(reaction, ctx);
      return;

    case "parallel-http":
      log.debug("parallel-http", { count: reaction.requests.length });
      await executeParallelHttpReaction(reaction, ctx);
      return;

    default:
      assertNever(reaction, "reaction kind");
  }
}
