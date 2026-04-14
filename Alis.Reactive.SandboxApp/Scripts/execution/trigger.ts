import type { Plan, Reaction, StartsWhen, ExecContext } from "../types";
import { resolveComponent, getJsType, wireEvent } from "../resolution/resolver";
import { executeReaction } from "./execute";
import { wireServerPush } from "./server-push";
import { wireSignalR } from "./signalr";
import { assertNever } from "../core/assert-never";
import { scope } from "../core/trace";

const log = scope("trigger");

/**
 * Execute a reaction and handle errors for both sync and async paths.
 * `source` identifies the originating trigger so failure logs point at
 * which wiring broke. Execution is synchronous for pure sync reactions
 * (set, call, branch with compare conditions) — required so SF event
 * mutations like args.cancel are visible when SF checks them after return.
 */
function runReaction(reaction: Reaction, plan: Plan, ctx: ExecContext, source: string): void {
  try {
    const result = executeReaction(reaction, plan, ctx);
    if (result instanceof Promise) {
      result.catch(err => log.error("reaction.failed", { source, sync: false, error: String(err) }));
    }
  } catch (err) {
    log.error("reaction.failed", { source, sync: true, error: String(err) });
  }
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
          runReaction(reaction, plan, {}, "page-ready");
        }, opts);
      } else {
        runReaction(reaction, plan, {}, "page-ready");
      }
      break;

    case "document-event": {
      const source = `document-event:${trigger.event}`;
      log.debug("document-event.listening", { event: trigger.event });
      document.addEventListener(trigger.event, (e: Event) => {
        log.trace("document-event.fired", { event: trigger.event });
        const ctx: ExecContext = { event: (e as CustomEvent).detail ?? e };
        runReaction(reaction, plan, ctx, source);
      }, opts);
      break;
    }

    case "component-event": {
      const comp = plan.components[trigger.component];
      if (!comp) {
        log.error("trigger.component-not-found", { component: trigger.component, event: trigger.event });
        throw new Error(`[alis] trigger component not found: ${trigger.component}`);
      }

      const jsType = getJsType(plan, trigger.component);
      const eventDef = jsType.events[trigger.event];
      const channel = eventDef?.channel ?? trigger.event;
      const source = `component-event:${trigger.component}:${trigger.event}`;

      log.debug("component-event.listening", { component: trigger.component, event: trigger.event, channel });

      wireEvent(plan, trigger.component, channel, (eventData) => {
        log.trace("component-event.fired", { component: trigger.component, event: trigger.event });
        const ctx: ExecContext = { event: eventData };
        runReaction(reaction, plan, ctx, source);
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
