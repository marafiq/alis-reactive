import type { Plan, Reaction, StartsWhen, ExecContext } from "../types";
import { resolveComponent, getJsType, wireEvent } from "../resolution/resolver";
import { executeReaction } from "./execute";
import { wireServerPush } from "./server-push";
import { wireSignalR } from "./signalr";
import { assertNever } from "../core/assert-never";
import { tracer } from "../tracing";
import { run as runInteraction } from "../tracing/interactions";

const t = tracer("trigger");

/**
 * The single entry-point choke point for executing a reaction.
 *
 * Wraps `executeReaction` in an interaction so the lifecycle
 * (`interaction.start`, `interaction.end`, `interaction.fail`) is
 * emitted automatically and the W3C trace-id propagates to every
 * event and outbound HTTP request inside the reaction. All entry
 * points — document events, page-ready, component events, server-push,
 * signalr, native action links — route through this function.
 *
 * No try/catch here: the interactions module handles sync return,
 * sync throw, async resolve, and async reject outcomes and restores
 * the previous interaction context in every path.
 */
export function runReaction(
  reaction: Reaction,
  plan: Plan,
  ctx: ExecContext,
  triggerKind: string,
  triggerAttrs: Record<string, unknown>,
): void | Promise<void> {
  return runInteraction(triggerKind, { ...triggerAttrs, planId: plan.planId }, () =>
    executeReaction(reaction, plan, ctx),
  );
}

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
          runReaction(reaction, plan, {}, "page-ready", {});
        }, opts);
      } else {
        runReaction(reaction, plan, {}, "page-ready", {});
      }
      break;

    case "document-event":
      t.debug("trigger.wire", { kind: "document-event", event: trigger.event });
      document.addEventListener(trigger.event, (e: Event) => {
        const ctx: ExecContext = { event: (e as CustomEvent).detail ?? e };
        runReaction(reaction, plan, ctx, "document-event", { event: trigger.event });
      }, opts);
      break;

    case "component-event": {
      const comp = plan.components[trigger.component];
      if (!comp) throw new Error(`[alis] trigger component not found: ${trigger.component}`);

      const jsType = getJsType(plan, trigger.component);
      const eventDef = jsType.events?.[trigger.event];
      const channel = eventDef?.channel ?? trigger.event;

      t.debug("trigger.wire", {
        kind: "component-event",
        component: trigger.component,
        event: trigger.event,
        channel,
      });

      wireEvent(plan, trigger.component, channel, (eventData) => {
        const ctx: ExecContext = { event: eventData };
        runReaction(reaction, plan, ctx, "component-event", {
          component: trigger.component,
          event: trigger.event,
        });
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
