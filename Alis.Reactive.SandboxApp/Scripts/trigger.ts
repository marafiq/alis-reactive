import type { Trigger, Reaction } from "./types";
import { resolveRoot } from "./component";
import { walk } from "./walk";
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
      document.addEventListener(trigger.event, (e) => {
        const detail = (e as CustomEvent).detail;
        executeReaction(reaction, { evt: detail ?? {} });
      });
      break;

    case "component-event": {
      const el = document.getElementById(trigger.componentId);
      if (!el) {
        log.warn("component-event: element not found", { componentId: trigger.componentId });
        break;
      }
      const root = resolveRoot(el, trigger.vendor);
      if (!root) {
        log.warn("component-event: no root", { componentId: trigger.componentId, vendor: trigger.vendor });
        break;
      }
      log.debug("component-event", { componentId: trigger.componentId, jsEvent: trigger.jsEvent, vendor: trigger.vendor });
      (root as EventTarget).addEventListener(trigger.jsEvent, (e: any) => {
        const expr = trigger.readExpr ?? "value";
        const detail = trigger.vendor === "native"
          ? { [expr]: walk(el, expr), event: e }
          : (e ?? {});
        executeReaction(reaction, { evt: detail });
      });
      break;
    }
  }
}
