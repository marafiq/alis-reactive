// server-push.ts — SSE (EventSource) trigger wiring.
// Uses ServerPushTrigger from the plan schema.

import type { ServerPushTrigger, Reaction, Plan } from "../types";
import { executeReaction } from "./execute";
import { showRetryIndicators, removeRetryIndicators } from "./retry-indicator";
import { scope } from "../core/trace";

const log = scope("server-push");

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
  log.info("retry.manual", { url });

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
    log.debug("connection.opened", { url });
    removeRetryIndicators(url);
  };

  es.onerror = () => {
    const managed = sources.get(url);
    if (managed?.stopping) return;

    if (es.readyState === EventSource.CLOSED) {
      log.error("connection.closed-permanent", { url });
      sources.delete(url);
      if (managed && managed.targetIds.size > 0) {
        const wiredBehaviors = managed.wired;
        showRetryIndicators(url, managed.targetIds, () => retrySSE(url, wiredBehaviors));
      }
    } else {
      log.warn("connection.reconnecting", { url });
    }
  };

  const managed: ManagedSource = { es, targetIds, wired, stopping: false };
  sources.set(url, managed);

  if (signal) {
    signal.addEventListener("abort", () => {
      managed.stopping = true;
      es.close();
      sources.delete(url);
      log.debug("connection.closed", { url });
    });
  }

  log.debug("source.created", { url });
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
    log.debug("message.received", { url: trigger.url, event: trigger.event });
    const result = executeReaction(reaction, plan, { event: evt });
    if (result instanceof Promise) {
      result.catch(err => log.error("reaction.failed", { url: trigger.url, event: trigger.event, error: String(err) }));
    }
  };

  const eventName = trigger.event ?? "message";
  managed.es.addEventListener(eventName, handler as EventListener);
  log.debug("event.listening", { url: trigger.url, event: eventName });
}
