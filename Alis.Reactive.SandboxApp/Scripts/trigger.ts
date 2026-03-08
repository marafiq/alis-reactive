import type { Trigger, Reaction } from "./types";
import { scope } from "./trace";
import { executeReaction } from "./execute";

const log = scope("trigger");

export function wireTrigger(trigger: Trigger, reaction: Reaction): void {
  switch (trigger.kind) {
    case "dom-ready":
      if (document.readyState === "complete" || document.readyState === "interactive") {
        log.debug("dom-ready: immediate");
        executeReaction(reaction);
      } else {
        log.debug("dom-ready: deferred");
        document.addEventListener("DOMContentLoaded", () => executeReaction(reaction));
      }
      break;

    case "custom-event":
      log.debug("custom-event: listening", { event: trigger.event });
      document.addEventListener(trigger.event, () => executeReaction(reaction));
      break;
  }
}
