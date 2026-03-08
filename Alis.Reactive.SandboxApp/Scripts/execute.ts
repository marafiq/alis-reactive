import type { Reaction, Command } from "./types";
import { scope } from "./trace";
import { mutateElement } from "./element";

const log = scope("command");

export function executeReaction(reaction: Reaction): void {
  switch (reaction.kind) {
    case "sequential":
      log.debug("sequential", { commands: reaction.commands.length });
      for (const cmd of reaction.commands) {
        executeCommand(cmd);
      }
      break;
  }
}

function executeCommand(cmd: Command): void {
  switch (cmd.kind) {
    case "dispatch":
      log.trace("dispatch", { event: cmd.event, payload: cmd.payload });
      document.dispatchEvent(
        new CustomEvent(cmd.event, { detail: cmd.payload ?? {} })
      );
      break;

    case "mutate-element":
      log.trace("mutate-element", { target: cmd.target, action: cmd.action, value: cmd.value });
      mutateElement(cmd);
      break;
  }
}
