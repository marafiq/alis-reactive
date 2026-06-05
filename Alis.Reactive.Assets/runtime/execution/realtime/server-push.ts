import type { ServerPushTrigger, ReactionGraph, PlanDocument } from "../../types/index";
import { catchAsyncReactionFailure, executeReaction } from "../reactions/execute";
import { showRetryIndicators, removeRetryIndicators } from "../requests/retry-indicator";
import { scope } from "../../diagnostics/trace";
import { ExecutionContext } from "../../browser-objects/execution-context";
import { toJavaScriptString } from "../../shared/javascript-string";

const log = scope("server-push");

interface WiredBehavior {
  readonly trigger: ServerPushTrigger;
  readonly reaction: ReactionGraph;
  readonly plan: PlanDocument;
  readonly handler: EventListener;
}

interface ManagedSource {
  readonly es: EventSource;
  readonly targetIds: Set<string>;
  readonly wired: WiredBehavior[];
  stopping: boolean;
}

// One EventSource is shared per URL so retry wiring can restore all behaviors
// attached to that stream.
const sources = new Map<string, ManagedSource>();

function retrySSE(url: string, behaviors: readonly WiredBehavior[]): void {
  removeRetryIndicators(url);
  log.info("retry.manual", { url });

  for (const wiredBehavior of behaviors) {
    wireServerPush(wiredBehavior.trigger, wiredBehavior.reaction, wiredBehavior.plan);
  }
}

function getOrCreate(url: string): ManagedSource {
  const cached = sources.get(url);
  if (cached) return cached;

  const es = new EventSource(url);
  const targetIds = new Set<string>();
  const wired: WiredBehavior[] = [];

  es.onopen = () => {
    log.debug("connected", { url });
    removeRetryIndicators(url);
  };

  es.onerror = () => {
    const managed = sources.get(url);
    const connectionIsStopping = managed !== undefined && managed.stopping;
    if (connectionIsStopping) return;

    if (es.readyState === EventSource.CLOSED) {
      log.error("connection.closed-permanent", { url });
      sources.delete(url);
      const retryCanBeShown = managed !== undefined && managed.targetIds.size > 0;
      if (retryCanBeShown) {
        const wiredBehaviors = managed.wired;
        showRetryIndicators(url, managed.targetIds, () => retrySSE(url, wiredBehaviors));
      }
    } else {
      log.warn("connection.reconnecting", { url });
    }
  };

  const managed: ManagedSource = { es, targetIds, wired, stopping: false };
  sources.set(url, managed);

  log.debug("source.created", { url });
  return managed;
}

export function wireServerPush(
  trigger: ServerPushTrigger,
  reaction: ReactionGraph,
  plan: PlanDocument,
  signal?: AbortSignal,
): void {
  const managed = getOrCreate(trigger.url);
  const eventName = serverPushEventName(trigger);

  const handler = (messageEvent: MessageEvent) => {
    const eventPayload: Record<string, unknown> = JSON.parse(messageEvent.data);
    log.debug("message.received", { url: trigger.url, event: eventName });
    catchAsyncReactionFailure(
      executeReaction(reaction, plan, ExecutionContext.event(eventPayload).raw),
      executionError => log.error("reaction.failed", { url: trigger.url, event: eventName, error: toJavaScriptString(executionError) }),
    );
  };
  const wiredBehavior = { trigger, reaction, plan, handler: handler as EventListener };

  managed.wired.push(wiredBehavior);

  managed.es.addEventListener(eventName, wiredBehavior.handler);
  signal?.addEventListener("abort", () => {
    releaseServerPushSubscription(managed, eventName, wiredBehavior);
  }, { once: true });
  log.debug("event.listening", { url: trigger.url, event: eventName });
}

function releaseServerPushSubscription(
  managed: ManagedSource,
  eventName: string,
  wiredBehavior: WiredBehavior,
): void {
  managed.es.removeEventListener(eventName, wiredBehavior.handler);

  const index = managed.wired.indexOf(wiredBehavior);
  if (index >= 0) managed.wired.splice(index, 1);

  if (managed.wired.length > 0) return;

  managed.stopping = true;
  managed.es.close();
  sources.delete(wiredBehavior.trigger.url);
  log.debug("connection.closed", { url: wiredBehavior.trigger.url });
}

function serverPushEventName(trigger: ServerPushTrigger): string {
  const filter = trigger.eventFilter;
  if (filter.kind === "any") return "message";

  return filter.event;
}
