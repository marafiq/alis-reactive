import type { Plan, Reaction, StartsWhen, ExecContext } from "../types";
import { resolveComponent, getJsType, wireEvent } from "../resolution/resolver";
import { executeReaction } from "./execute";
import { wireServerPush } from "./server-push";
import { wireSignalR } from "./signalr";
import { assertNever } from "../core/assert-never";
import { scope } from "../core/trace";

const log = scope("trigger");

export function wireBehavior(
  trigger: StartsWhen,
  reaction: Reaction,
  plan: Plan,
  signal?: AbortSignal
): void {
  const opts: AddEventListenerOptions | undefined = signal ? { signal } : undefined;

  switch (trigger.kind) {
    case "page-ready":
      if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", () => {
          executeReaction(reaction, plan, {}).catch(err =>
            log.error("reaction failed", { error: String(err) }));
        }, opts);
      } else {
        executeReaction(reaction, plan, {}).catch(err =>
          log.error("reaction failed", { error: String(err) }));
      }
      break;

    case "document-event":
      log.debug("document-event: listening", { event: trigger.event });
      document.addEventListener(trigger.event, (e: Event) => {
        const ctx: ExecContext = { event: (e as CustomEvent).detail ?? e };
        executeReaction(reaction, plan, ctx).catch(err =>
          log.error("reaction failed", { error: String(err) }));
      }, opts);
      break;

    case "component-event": {
      const comp = plan.components[trigger.component];
      if (!comp) throw new Error(`[alis] trigger component not found: ${trigger.component}`);

      const jsType = getJsType(plan, trigger.component);
      const eventDef = jsType.events?.[trigger.event];
      const channel = eventDef?.channel ?? trigger.event;

      log.debug("component-event", { component: trigger.component, event: trigger.event, channel });

      wireEvent(plan, trigger.component, channel, (eventData) => {
        const ctx: ExecContext = { event: eventData };
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
