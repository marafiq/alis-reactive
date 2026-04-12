// server-push.ts — SSE (EventSource) trigger wiring.
// Uses ServerPushTrigger from the plan schema.

import type { ServerPushTrigger, Reaction, Plan } from "../types";
import { executeReaction } from "./execute";
import { showRetryIndicators, removeRetryIndicators } from "./retry-indicator";
import { tracer } from "../tracing";

const t = tracer("server-push");

interface WiredBehavior {
  readonly trigger: ServerPushTrigger;
  readonly reaction: Reaction;
  readonly plan: Plan;
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
  t.info("sse.retry", { url });

  for (const b of behaviors) {
    wireServerPush(b.trigger, b.reaction, b.plan);
  }
}

function getOrCreate(url: string, signal?: AbortSignal): ManagedSource {
  const cached = sources.get(url);
  if (cached) return cached;

  const es = new EventSource(url);
  const targetIds = new Set<string>();
  const wired: WiredBehavior[] = [];

  es.onopen = () => {
    t.debug("sse.connection.open", { url });
    removeRetryIndicators(url);
  };

  es.onerror = () => {
    const managed = sources.get(url);
    if (managed?.stopping) return;

    if (es.readyState === EventSource.CLOSED) {
      t.error("sse.connection.close", { url, permanent: true });
      sources.delete(url);
      if (managed && managed.targetIds.size > 0) {
        const wiredBehaviors = managed.wired;
        showRetryIndicators(url, managed.targetIds, () => retrySSE(url, wiredBehaviors));
      }
    } else {
      t.warn("sse.reconnect", { url });
    }
  };

  const managed: ManagedSource = { es, targetIds, wired, stopping: false };
  sources.set(url, managed);

  if (signal) {
    signal.addEventListener("abort", () => {
      managed.stopping = true;
      es.close();
      sources.delete(url);
      t.debug("sse.connection.close", { url, permanent: false });
    });
  }

  t.debug("sse.connection.new", { url });
  return managed;
}

export function wireServerPush(
  trigger: ServerPushTrigger,
  reaction: Reaction,
  plan: Plan,
  signal?: AbortSignal,
): void {
  const managed = getOrCreate(trigger.url, signal);

  managed.wired.push({ trigger, reaction, plan });

  const handler = (e: MessageEvent) => {
    const evt: Record<string, unknown> = JSON.parse(e.data);
    t.debug("sse.message", { url: trigger.url, event: trigger.event });
    const result = executeReaction(reaction, plan, { event: evt });
    if (result instanceof Promise) {
      result.catch(err => t.error("sse.reaction.fail", { url: trigger.url, event: trigger.event }, err as Error));
    }
  };

  const eventName = trigger.event ?? "message";
  managed.es.addEventListener(eventName, handler as EventListener);
  t.debug("sse.listen", { url: trigger.url, event: eventName });
}
