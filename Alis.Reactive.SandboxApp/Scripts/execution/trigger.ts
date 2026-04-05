// trigger.ts — Wire StartsWhen to Reaction execution.
// Dispatches on StartsWhen.kind to set up the appropriate event listener.
// Uses the SHARED resolver for component event wiring.

import type { StartsWhen, Reaction, Plan } from "../types";
import { resolveComponent, getJsType } from "../resolution/resolver";
import { scope } from "../core/trace";
import { executeReaction } from "./execute";
import { assertNever } from "../core/assert-never";
import { wireServerPush } from "./server-push";
import { wireSignalR } from "./signalr";

const log = scope("trigger");

export function wireTrigger(
  trigger: StartsWhen,
  reaction: Reaction,
  plan: Plan,
  signal?: AbortSignal,
): void {
  const opts = signal ? { signal } : undefined;

  switch (trigger.kind) {
    case "page-ready":
      if (document.readyState === "complete" || document.readyState === "interactive") {
        executeReaction(reaction, plan, {}).catch(err =>
          log.error("reaction failed", { error: String(err) }));
      } else {
        document.addEventListener("DOMContentLoaded", () =>
          executeReaction(reaction, plan, {}).catch(err =>
            log.error("reaction failed", { error: String(err) })), opts);
      }
      break;

    case "document-event":
      log.debug("document-event: listening", { event: trigger.event });
      document.addEventListener(trigger.event, (e) => {
        const detail = (e as CustomEvent).detail;
        const ctx = { event: detail ?? {} };
        executeReaction(reaction, plan, ctx).catch(err =>
          log.error("reaction failed", { error: String(err) }));
      }, opts);
      break;

    case "component-event": {
      const comp = plan.components[trigger.component];
      if (!comp) throw new Error(`[alis] trigger component not found: ${trigger.component}`);
      const jsType = getJsType(plan, trigger.component);
      const eventDef = jsType.events?.[trigger.event];
      if (!eventDef) throw new Error(`[alis] event "${trigger.event}" not found on type for ${trigger.component}`);

      const root = resolveComponent(plan, trigger.component);
      const channel = eventDef.channel;

      log.debug("component-event", { component: trigger.component, event: trigger.event, channel });

      (root as EventTarget).addEventListener(channel, (e: any) => {
        // For native components, e is a DOM Event; for fusion, e is the event args object
        const ctx = { event: comp.vendor === "native" ? e : (e ?? {}) };
        executeReaction(reaction, plan, ctx).catch(err =>
          log.error("reaction failed", { error: String(err) }));
      }, opts);
      break;
    }

    case "server-push":
      wireServerPush(trigger, reaction, plan, signal);
      break;

    case "signalr":
      wireSignalR(trigger, reaction, plan, signal);
      break;

    default:
      assertNever(trigger, "trigger kind");
  }
}
