import type { Plan, Reaction, StartsWhen, ExecContext } from "../types";
import { resolveComponent, getJsType, wireEvent } from "../resolution/resolver";
import { executeReaction } from "./execute";
import { wireServerPush } from "./server-push";
import { wireSignalR } from "./signalr";
import { assertNever } from "../core/assert-never";
import { tracer, getRootSpan } from "../tracing";

const t = tracer("trigger");

function describeTrigger(trigger: StartsWhen): string {
  switch (trigger.kind) {
    case "page-ready":       return "page-ready";
    case "document-event":   return `document-event:${trigger.event}`;
    case "component-event":  return `component-event:${trigger.component}.${trigger.event}`;
    case "server-push":      return `server-push:${trigger.url}/${trigger.event}`;
    case "signalr":          return `signalr:${trigger.hubUrl}/${trigger.method}`;
  }
}

/**
 * Execute a reaction and handle errors for both sync and async paths.
 * The callback is synchronous. For pure sync reactions (set, call, branch
 * with compare conditions), execution completes before this returns —
 * in the same tick as the SF event callback. SF checks args.cancel AFTER
 * this returns, so the mutation is visible.
 */
function runReaction(reaction: Reaction, plan: Plan, ctx: ExecContext, triggerDesc: string): void {
  const scoped = t.withSpan(ctx?.span);
  try {
    const result = executeReaction(reaction, plan, ctx);
    if (result instanceof Promise) {
      result.catch(err => scoped.error("reaction.fail", { trigger: triggerDesc, planId: plan.planId }, err as Error));
    }
  } catch (err) {
    scoped.error("reaction.fail", { trigger: triggerDesc, planId: plan.planId }, err as Error);
  }
}

export function wireBehavior(
  trigger: StartsWhen,
  reaction: Reaction,
  plan: Plan,
  signal?: AbortSignal
): void {
  const opts: AddEventListenerOptions | undefined = signal ? { signal } : undefined;
  const triggerDesc = describeTrigger(trigger);

  switch (trigger.kind) {
    case "page-ready":
      if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", () => {
          const span = getRootSpan().child("trigger.page-ready");
          runReaction(reaction, plan, { span }, triggerDesc);
        }, opts);
      } else {
        const span = getRootSpan().child("trigger.page-ready");
        runReaction(reaction, plan, { span }, triggerDesc);
      }
      break;

    case "document-event":
      t.debug("trigger.wire", { kind: "document-event", event: trigger.event });
      document.addEventListener(trigger.event, (e: Event) => {
        const span = getRootSpan().child("trigger.document-event", { event: trigger.event });
        const ctx: ExecContext = { event: (e as CustomEvent).detail ?? e, span };
        runReaction(reaction, plan, ctx, triggerDesc);
      }, opts);
      break;

    case "component-event": {
      const comp = plan.components[trigger.component];
      if (!comp) throw new Error(`[alis] trigger component not found: "${trigger.component}" (available: ${Object.keys(plan.components).join(", ")})`);

      const jsType = getJsType(plan, trigger.component);
      const eventDef = jsType.events?.[trigger.event];
      const channel = eventDef?.channel ?? trigger.event;

      t.debug("trigger.wire", { kind: "component-event", component: trigger.component, event: trigger.event, channel });

      wireEvent(plan, trigger.component, channel, (eventData) => {
        const span = getRootSpan().child("trigger.component-event", { component: trigger.component, event: trigger.event });
        const ctx: ExecContext = { event: eventData, span };
        runReaction(reaction, plan, ctx, triggerDesc);
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
