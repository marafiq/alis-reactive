import type { Plan, Reaction, StartsWhen, ExecContext } from "../types";
import { resolveComponent, getJsType } from "../resolution/resolver";
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

      const root = resolveComponent(plan, trigger.component);

      log.debug("component-event", { component: trigger.component, event: trigger.event, channel });

      // Vendor-specific event wiring:
      // - Native: standard DOM addEventListener on the element
      // - Fusion: SF addEventListener on the component instance (NO opts — SF doesn't support AbortSignal)
      if (comp.vendor === "fusion") {
        // SF's addEventListener doesn't support DOM options like {signal}.
        // Pass handler only — SF manages its own event lifecycle.
        (root as any).addEventListener(channel, (args: any) => {
          const ctx: ExecContext = { event: args ?? {} };
          executeReaction(reaction, plan, ctx).catch(err =>
            log.error("reaction failed", { error: String(err) }));
        });
      } else {
        // Native: standard DOM addEventListener with abort signal support
        (root as EventTarget).addEventListener(channel, (e: Event) => {
          const eventData = e instanceof CustomEvent
            ? (e.detail ?? {})
            : (e.currentTarget ?? e.target ?? e);
          const ctx: ExecContext = { event: eventData };
          executeReaction(reaction, plan, ctx).catch(err =>
            log.error("reaction failed", { error: String(err) }));
        }, opts);
      }
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
