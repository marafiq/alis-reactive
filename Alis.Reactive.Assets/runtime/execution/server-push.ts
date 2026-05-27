// server-push.ts — SSE (EventSource) trigger wiring.
// Uses ServerPushTrigger from the generated plan contract.

import type { ServerPushTrigger, Reaction, Plan } from "../types";
import { catchAsyncReactionFailure, executeReaction } from "./execute";
import { showRetryIndicators, removeRetryIndicators } from "./retry-indicator";
import { scope } from "../core/trace";
import { ExecutionContext } from "../domain/execution-context";

const log = scope("server-push");

interface WiredBehavior {
  readonly trigger: ServerPushTrigger;
  readonly reaction: Reaction;
  readonly plan: Plan;
  readonly handler: EventListener;
}

interface ManagedSource {
  readonly es: EventSource;
  readonly targetIds: Set<string>;
  readonly wired: WiredBehavior[];
  stopping: boolean;
}

// Connection pool — singleton EventSource per URL
const sources = new Map<string, ManagedSource>();

function retrySSE(url: string, behaviors: readonly WiredBehavior[]): void {
  removeRetryIndicators(url);
  log.info("retry.manual", { url });

  for (const b of behaviors) {
    wireServerPush(b.trigger, b.reaction, b.plan);
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
  reaction: Reaction,
  plan: Plan,
  signal?: AbortSignal,
): void {
  const managed = getOrCreate(trigger.url);
  const eventName = serverPushEventName(trigger);

  const handler = (e: MessageEvent) => {
    const evt: Record<string, unknown> = JSON.parse(e.data);
    log.debug("message.received", { url: trigger.url, event: eventName });
    catchAsyncReactionFailure(
      executeReaction(reaction, plan, ExecutionContext.event(evt).raw),
      err => log.error("reaction.failed", { url: trigger.url, event: eventName, error: String(err) }),
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
