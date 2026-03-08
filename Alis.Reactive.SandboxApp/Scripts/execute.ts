import type { Reaction, Command, ExecContext } from "./types";
import { scope } from "./trace";
import { mutateElement } from "./element";
import { evaluateGuard } from "./conditions";

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
      for (const branch of reaction.branches) {
        if (branch.guard == null || evaluateGuard(branch.guard, ctx)) {
          log.trace("branch-taken", { guard: branch.guard?.kind ?? "else" });
          executeReaction(branch.reaction, ctx);
          return; // first match wins
        }
      }
      log.trace("no-branch-taken");
      break;
  }
}

function executeCommand(cmd: Command, ctx?: ExecContext): void {
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
