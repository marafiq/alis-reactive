import type { PlanDocument, ReactionGraph, StartsWhen } from "../../types/index";
import { wireEvent } from "../../events/resolver";
import { RuntimePlan } from "../../browser-objects/runtime-plan";
import { catchAsyncReactionFailure, executeReaction } from "../reactions/execute";
import { wireServerPush } from "../realtime/server-push";
import { wireSignalR } from "../realtime/signalr";
import { assertNever } from "../../shared/assert-never";
import { scope } from "../../diagnostics/trace";
import { ExecutionContext } from "../../browser-objects/execution-context";
import { componentEventChannel } from "../../browser-objects/component-event-contract";
import { toJavaScriptString } from "../../shared/javascript-string";

const log = scope("trigger");

// Pure sync reactions must return before Syncfusion inspects mutable event args
// such as args.cancel. The source tag ties failure logs to trigger wiring.
function runReaction(reaction: ReactionGraph, planDocument: PlanDocument, context: ExecutionContext, source: string): void {
  try {
    catchAsyncReactionFailure(
      executeReaction(reaction, planDocument, context.raw),
      err => log.error("reaction.failed", { source, sync: false, error: toJavaScriptString(err) }),
    );
  } catch (err) {
    log.error("reaction.failed", { source, sync: true, error: toJavaScriptString(err) });
  }
}

export function wireTrigger(
  trigger: StartsWhen,
  reaction: ReactionGraph,
  planDocument: PlanDocument,
  signal?: AbortSignal
): void {
  const eventOptions = listenerOptions(signal);

  switch (trigger.kind) {
    case "page-ready":
      if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", () => {
          runReaction(reaction, planDocument, ExecutionContext.empty(), "page-ready");
        }, eventOptions);
      } else {
        runReaction(reaction, planDocument, ExecutionContext.empty(), "page-ready");
      }
      break;

    case "document-event": {
      const source = `document-event:${trigger.event}`;
      log.debug("document-event.listening", { event: trigger.event });
      document.addEventListener(trigger.event, (documentEvent: Event) => {
        const context = ExecutionContext.event(documentEventPayload(documentEvent));
        runReaction(reaction, planDocument, context, source);
      }, eventOptions);
      break;
    }

    case "component-event": {
      const component = RuntimePlan.from(planDocument).components.component(trigger.component);
      const eventContract = componentEventChannel(component, trigger.event);

      const source = `component-event:${trigger.component}:${trigger.event}`;

      log.debug("component-event.listening", {
        component: trigger.component,
        event: eventContract.eventName,
        channel: eventContract.channel,
      });

      wireEvent(planDocument, trigger.component, eventContract.channel, (eventData) => {
        const context = ExecutionContext.event(eventData);
        runReaction(reaction, planDocument, context, source);
      }, eventOptions);
      break;
    }

    case "server-push":
      wireServerPush(trigger, reaction, planDocument, signal);
      break;

    case "signalr":
      wireSignalR(trigger, reaction, planDocument, signal);
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
