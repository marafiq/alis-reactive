import type { Trigger, Reaction, ComponentEntry } from "./types";
import { resolveRoot } from "./component";
import { walk } from "./walk";
import { scope } from "./trace";
import { executeReaction } from "./execute";

const log = scope("trigger");

export function wireTrigger(
  trigger: Trigger,
  reaction: Reaction,
  components?: Record<string, ComponentEntry>,
  signal?: AbortSignal
): void {
  const opts = signal ? { signal } : undefined;

  switch (trigger.kind) {
    case "dom-ready":
      if (document.readyState === "complete" || document.readyState === "interactive") {
        executeReaction(reaction, { components });
      } else {
        document.addEventListener("DOMContentLoaded", () => executeReaction(reaction, { components }), opts);
      }
      break;

    case "custom-event":
      log.debug("custom-event: listening", { event: trigger.event });
      document.addEventListener(trigger.event, (e) => {
        const detail = (e as CustomEvent).detail;
        executeReaction(reaction, { evt: detail ?? {}, components });
      }, opts);
      break;

    case "component-event": {
      const el = document.getElementById(trigger.componentId);
      if (!el) throw new Error(`[alis] element not found: ${trigger.componentId}`);
      const root = resolveRoot(el, trigger.vendor);
      log.debug("component-event", { componentId: trigger.componentId, jsEvent: trigger.jsEvent, vendor: trigger.vendor });
      (root as EventTarget).addEventListener(trigger.jsEvent, (e: any) => {
        const expr = trigger.readExpr ?? "value";
        const detail = trigger.vendor === "native"
          ? { [expr]: walk(el, expr), event: e }
          : (e ?? {});
        executeReaction(reaction, { evt: detail, components });
      }, opts);
      break;
    }
  }
}
