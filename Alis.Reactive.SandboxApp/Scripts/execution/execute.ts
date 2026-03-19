import type { Reaction, ExecContext } from "../types";
import { scope } from "../core/trace";
import { executeCommand } from "./commands";
import { evaluateGuardAsync } from "../conditions/conditions";
import { executeHttpReaction, executeParallelHttpReaction } from "./pipeline";
import { assertNever } from "../core/assert-never";

const log = scope("execute");

export async function executeReaction(reaction: Reaction, ctx?: ExecContext): Promise<void> {
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
        if (branch.guard == null || await evaluateGuardAsync(branch.guard, ctx)) {
          log.trace("branch-taken", { guard: branch.guard?.kind ?? "else" });
          await executeReaction(branch.reaction, ctx);
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
