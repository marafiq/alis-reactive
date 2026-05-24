// server-push.ts — SSE (EventSource) trigger wiring.
// Uses ServerPushTrigger from the plan schema.

import type { ServerPushTrigger, Reaction, Plan } from "../types";
import { executeReaction, ReactionCompletion } from "./execute";
import { showRetryIndicators, removeRetryIndicators } from "./retry-indicator";
import { scope } from "../core/trace";
import { ExecutionContext } from "../domain/execution-context";

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
  const eventName = ServerPushEventName.from(trigger).value;

  managed.wired.push({ trigger, reaction, plan });

  const handler = (e: MessageEvent) => {
    const evt: Record<string, unknown> = JSON.parse(e.data);
    log.debug("message.received", { url: trigger.url, event: eventName });
    ReactionCompletion
      .from(executeReaction(reaction, plan, ExecutionContext.event(evt).raw))
      .catchAsync(err => log.error("reaction.failed", { url: trigger.url, event: eventName, error: String(err) }));
  };

  managed.es.addEventListener(eventName, handler as EventListener);
  log.debug("event.listening", { url: trigger.url, event: eventName });
}

class ServerPushEventName {
  private constructor(readonly value: string) {}

  static from(trigger: ServerPushTrigger): ServerPushEventName {
    const filter = trigger.eventFilter;
    if (filter.kind === "any") return new ServerPushEventName("message");

    return new ServerPushEventName(filter.event);
  }
}
