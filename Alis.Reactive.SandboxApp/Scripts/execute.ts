import type { Reaction, Command, ExecContext } from "./types";
import { scope } from "./trace";
import { mutateElement } from "./element";

const log = scope("command");

export function executeReaction(reaction: Reaction, ctx?: ExecContext): void {
  switch (reaction.kind) {
    case "sequential":
      log.debug("sequential", { commands: reaction.commands.length });
      for (const cmd of reaction.commands) {
        executeCommand(cmd, ctx);
      }
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
