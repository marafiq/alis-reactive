import type { PlanDocument, ReactionGraph, StartsWhen } from "../types";
import { wireEvent } from "../resolution/resolver";
import { RuntimePlan } from "../domain/runtime-plan";
import { catchAsyncReactionFailure, executeReaction } from "./execute";
import { wireServerPush } from "./server-push";
import { wireSignalR } from "./signalr";
import { assertNever } from "../core/assert-never";
import { scope } from "../core/trace";
import { ExecutionContext } from "../domain/execution-context";
import { ComponentEventContract } from "../domain/component-event-contract";

const log = scope("trigger");

/**
 * Execute a reaction and handle errors for both sync and async paths.
 * `source` identifies the originating trigger so failure logs point at
 * which wiring broke. Execution is synchronous for pure sync reactions
 * (set, call, branch with compare conditions) — required so SF event
 * mutations like args.cancel are visible when SF checks them after return.
 */
function runReaction(reaction: ReactionGraph, plan: PlanDocument, context: ExecutionContext, source: string): void {
  try {
    catchAsyncReactionFailure(
      executeReaction(reaction, plan, context.raw),
      err => log.error("reaction.failed", { source, sync: false, error: String(err) }),
    );
  } catch (err) {
    log.error("reaction.failed", { source, sync: true, error: String(err) });
  }
}

export function wireBehavior(
  trigger: StartsWhen,
  reaction: ReactionGraph,
  plan: PlanDocument,
  signal?: AbortSignal
): void {
  const opts = listenerOptions(signal);

  switch (trigger.kind) {
    case "page-ready":
      if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", () => {
          runReaction(reaction, plan, ExecutionContext.empty(), "page-ready");
        }, opts);
      } else {
        runReaction(reaction, plan, ExecutionContext.empty(), "page-ready");
      }
      break;

    case "document-event": {
      const source = `document-event:${trigger.event}`;
      log.debug("document-event.listening", { event: trigger.event });
      document.addEventListener(trigger.event, (e: Event) => {
        const context = ExecutionContext.event(documentEventPayload(e));
        runReaction(reaction, plan, context, source);
      }, opts);
      break;
    }

    case "component-event": {
      const component = RuntimePlan.from(plan).components.component(trigger.component);
      const eventContract = ComponentEventContract.declaredBy(component, trigger.event);

      const source = `component-event:${trigger.component}:${trigger.event}`;

      log.debug("component-event.listening", {
        component: trigger.component,
        event: eventContract.eventName,
        channel: eventContract.channel,
      });

      wireEvent(plan, trigger.component, eventContract.channel, (eventData) => {
        const context = ExecutionContext.event(eventData);
        runReaction(reaction, plan, context, source);
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

function listenerOptions(signal: AbortSignal | undefined): AddEventListenerOptions | undefined {
  if (signal === undefined) return undefined;

  return { signal };
}

function documentEventPayload(event: Event): unknown {
  const detail = (event as CustomEvent).detail;
  const eventCarriesDetail = detail !== null && detail !== undefined;
  if (eventCarriesDetail) return detail;

  return event;
}
