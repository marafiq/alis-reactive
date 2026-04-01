import type { ServerPushTrigger, Reaction, ComponentEntry } from "../types";
import { executeReactionSequence } from "./execute";
import { showRetryIndicators, removeRetryIndicators, firstMutationTarget } from "./retry-indicator";
import { scope } from "../core/trace";

const log = scope("server-push");

interface WiredEntry {
  readonly trigger: ServerPushTrigger;
  readonly reactions: readonly Reaction[];
  readonly components?: Record<string, ComponentEntry>;
}

interface Subscription extends WiredEntry {
  readonly eventName: string;
  readonly targetIds: ReadonlySet<string>;
  readonly handler: EventListener;
  active: boolean;
}

interface ManagedSource {
  es: EventSource;
  readonly subscriptions: Set<Subscription>;
  stopping: boolean;
}

// Connection pool — singleton EventSource per URL
const sources = new Map<string, ManagedSource>();

function isClosed(es: EventSource): boolean {
  const CLOSED = (es as EventSource & { readonly CLOSED?: number }).CLOSED ?? EventSource.CLOSED;
  return es.readyState === CLOSED;
}

function activeSubscriptions(managed: ManagedSource): Subscription[] {
  return Array.from(managed.subscriptions).filter(subscription => subscription.active);
}

function collectTargetIds(managed: ManagedSource): Set<string> {
  const targetIds = new Set<string>();
  for (const subscription of activeSubscriptions(managed)) {
    for (const targetId of subscription.targetIds) targetIds.add(targetId);
  }
  return targetIds;
}

function bindSubscription(managed: ManagedSource, subscription: Subscription): void {
  managed.es.addEventListener(subscription.eventName, subscription.handler);
}

function unbindSubscription(managed: ManagedSource, subscription: Subscription): void {
  managed.es.removeEventListener(subscription.eventName, subscription.handler);
}

function showRetryForActiveSubscribers(url: string, managed: ManagedSource): void {
  const targetIds = collectTargetIds(managed);
  if (targetIds.size === 0) {
    removeRetryIndicators(url);
    return;
  }
  showRetryIndicators(url, targetIds, () => retrySSE(url));
}

function createEventSource(url: string, managed: ManagedSource): EventSource {
  const es = new EventSource(url);

  es.onopen = () => {
    log.debug("connected", { url });
    removeRetryIndicators(url);
  };

  es.onerror = () => {
    if (managed.stopping) return;

    if (isClosed(es)) {
      log.error("connection closed permanently", { url });
      if (activeSubscriptions(managed).length === 0) {
        removeRetryIndicators(url);
        sources.delete(url);
        return;
      }
      showRetryForActiveSubscribers(url, managed);
    } else {
      log.warn("connection error (reconnecting)", { url });
    }
  };

  return es;
}

function rebindActiveSubscriptions(managed: ManagedSource): void {
  for (const subscription of activeSubscriptions(managed)) {
    bindSubscription(managed, subscription);
  }
}

function retrySSE(url: string): void {
  const managed = sources.get(url);
  if (!managed) return;

  removeRetryIndicators(url);
  log.info("manual retry", { url });

  managed.stopping = false;
  managed.es.close();
  managed.es = createEventSource(url, managed);
  rebindActiveSubscriptions(managed);
}

function removeSubscription(url: string, subscription: Subscription): void {
  const managed = sources.get(url);
  if (!managed || !managed.subscriptions.has(subscription)) return;

  subscription.active = false;
  unbindSubscription(managed, subscription);
  managed.subscriptions.delete(subscription);

  if (managed.subscriptions.size === 0) {
    managed.stopping = true;
    managed.es.close();
    sources.delete(url);
    removeRetryIndicators(url);
    log.debug("closed", { url });
    return;
  }

  if (!managed.stopping && isClosed(managed.es)) {
    showRetryForActiveSubscribers(url, managed);
  }
}

function getOrCreate(url: string): ManagedSource {
  const cached = sources.get(url);
  if (cached) return cached;

  const managed: ManagedSource = {
    es: null as unknown as EventSource,
    subscriptions: new Set<Subscription>(),
    stopping: false,
  };
  managed.es = createEventSource(url, managed);
  sources.set(url, managed);

  log.debug("created", { url });
  return managed;
}

export function wireServerPush(
  trigger: ServerPushTrigger,
  reactionOrReactions: Reaction | readonly Reaction[],
  components?: Record<string, ComponentEntry>,
  signal?: AbortSignal
): void {
  const reactions = Array.isArray(reactionOrReactions) ? reactionOrReactions : [reactionOrReactions];
  const managed = getOrCreate(trigger.url);
  const targetIds = new Set<string>();
  for (const reaction of reactions) {
    const target = firstMutationTarget(reaction);
    if (target) targetIds.add(target);
  }

  const eventName = trigger.eventType ?? "message";
  const subscription: Subscription = {
    trigger,
    reactions,
    components,
    eventName,
    targetIds,
    active: true,
    handler: ((e: MessageEvent) => {
      if (!subscription.active) return;

    // Framework only supports JSON payloads — non-JSON is a server-side bug.
    // Throw immediately so the developer fixes their SSE endpoint.
    const evt: Record<string, unknown> = JSON.parse(e.data);
    log.debug("message", { url: trigger.url, eventType: trigger.eventType });
    executeReactionSequence(reactions, { evt, components }).catch(err =>
      log.error("reaction failed", { error: String(err) }));
    }) as EventListener,
  };

  managed.subscriptions.add(subscription);
  bindSubscription(managed, subscription);

  log.debug("listening", { url: trigger.url, eventType: eventName });

  if (signal) {
    signal.addEventListener("abort", () => removeSubscription(trigger.url, subscription), { once: true });
  }
}
