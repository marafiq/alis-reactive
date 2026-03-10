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

      if (trigger.vendor === "fusion") {
        // Syncfusion: access the EJ2 component instance and wire via its event API
        const comp = (el as any).ej2_instances?.[0];
        if (!comp) {
          log.warn("component-event: no ej2 instance", { componentId: trigger.componentId });
          break;
        }
        log.debug("component-event: fusion", { componentId: trigger.componentId, jsEvent: trigger.jsEvent });
        comp.addEventListener(trigger.jsEvent, (args: any) => {
          executeReaction(reaction, { evt: args ?? {} });
        });
      } else {
        // Native: standard DOM event listener
        log.debug("component-event: native", { componentId: trigger.componentId, jsEvent: trigger.jsEvent });
        el.addEventListener(trigger.jsEvent, (e: Event) => {
          const target = e.target as HTMLInputElement;
          executeReaction(reaction, { evt: { value: target?.value, checked: target?.checked, event: e } });
        });
      }
      break;
    }
  }
}
