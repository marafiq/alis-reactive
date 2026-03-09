import type { Reaction, Command, ExecContext } from "./types";
import { scope } from "./trace";
import { mutateElement } from "./element";
import { evaluateGuard, evaluateGuardAsync, isConfirmGuard } from "./conditions";
import { executeHttpReaction, executeParallelHttpReaction } from "./pipeline";

const log = scope("command");

export function executeReaction(reaction: Reaction, ctx?: ExecContext): void {
  switch (reaction.kind) {
    case "sequential":
      log.debug("sequential", { commands: reaction.commands.length });
      for (const cmd of reaction.commands) {
        executeCommand(cmd, ctx);
      }
      break;

    case "conditional":
      log.debug("conditional", { branches: reaction.branches.length });
      // Check if any branch guard contains a ConfirmGuard — use async path if so
      const hasConfirm = reaction.branches.some(b =>
        b.guard != null && isConfirmGuard(b.guard)
      );
      if (hasConfirm) {
        executeReactionAsync(reaction, ctx);
        return;
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
  }
}

/**
 * Async execution path — only invoked when branches contain ConfirmGuard.
 * Zero overhead for non-confirm paths (never called).
 */
export async function executeReactionAsync(reaction: Reaction, ctx?: ExecContext): Promise<void> {
  switch (reaction.kind) {
    case "sequential":
      for (const cmd of reaction.commands) {
        executeCommand(cmd, ctx);
      }
      return;

    case "conditional":
      for (const branch of reaction.branches) {
        if (branch.guard == null) {
          await executeReactionAsync(branch.reaction, ctx);
          return;
        }
        if (await evaluateGuardAsync(branch.guard, ctx)) {
          await executeReactionAsync(branch.reaction, ctx);
          return;
        }
      }
      break;
  }
}

function executeCommand(cmd: Command, ctx?: ExecContext): void {
  // Per-action When guard — skip this command if guard evaluates false
  if (cmd.when) {
    if (isConfirmGuard(cmd.when)) {
      log.warn("ConfirmGuard on per-action When is not supported. Use branch-level Confirm.");
      return;
    }
    if (!evaluateGuard(cmd.when, ctx)) {
      log.trace("per-action-when-skipped", { kind: cmd.kind });
      return;
    }
  }

  switch (cmd.kind) {
    case "dispatch":
      log.trace("dispatch", { event: cmd.event, payload: cmd.payload });
      document.dispatchEvent(
        new CustomEvent(cmd.event, { detail: cmd.payload ?? {} })
      );
      break;

    case "mutate-element":
      log.trace("mutate-element", { target: cmd.target, jsEmit: cmd.jsEmit });
      mutateElement(cmd, ctx);
      break;
  }
}
