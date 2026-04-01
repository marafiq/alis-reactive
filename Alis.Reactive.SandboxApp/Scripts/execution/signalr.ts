import * as signalR from "@microsoft/signalr";
import type { SignalRTrigger, Reaction, ComponentEntry } from "../types";
import { executeReactionSequence } from "./execute";
import { showRetryIndicators, removeRetryIndicators, firstMutationTarget } from "./retry-indicator";
import { scope } from "../core/trace";

const log = scope("signalr");

interface ManagedConnection {
  readonly connection: signalR.HubConnection;
  startPromise: Promise<void>;
  readonly subscriptions: Set<Subscription>;
  readonly dispatchers: Map<string, (...args: unknown[]) => void>;
  stopping: boolean;
}

interface Subscription {
  readonly trigger: SignalRTrigger;
  readonly reactions: readonly Reaction[];
  readonly components?: Record<string, ComponentEntry>;
  readonly targetIds: ReadonlySet<string>;
  active: boolean;
}

// Connection pool — singleton HubConnection per hubUrl
const hubs = new Map<string, ManagedConnection>();

function activeSubscriptions(managed: ManagedConnection): Subscription[] {
  return Array.from(managed.subscriptions).filter(subscription => subscription.active);
}

function collectTargetIds(managed: ManagedConnection): Set<string> {
  const targetIds = new Set<string>();
  for (const subscription of activeSubscriptions(managed)) {
    for (const targetId of subscription.targetIds) targetIds.add(targetId);
  }
  return targetIds;
}

function showRetryForActiveSubscribers(hubUrl: string, managed: ManagedConnection): void {
  const targetIds = collectTargetIds(managed);
  if (targetIds.size === 0) {
    removeRetryIndicators(hubUrl);
    return;
  }
  showRetryIndicators(hubUrl, targetIds, () => retryConnection(hubUrl));
}

/**
 * Starts the connection with retry for initial connection failures.
 * withAutomaticReconnect() only handles reconnection AFTER a successful start —
 * initial start() failures must be retried manually (per Microsoft docs).
 */
async function startWithRetry(connection: signalR.HubConnection, hubUrl: string): Promise<void> {
  // Aligned with library's withAutomaticReconnect() default: [0, 2000, 10000, 30000]
  const maxAttempts = 4;
  const delays = [0, 2000, 10000, 30000];

  for (let attempt = 0; attempt < maxAttempts; attempt++) {
    try {
      await connection.start();
      log.info("connected", { hubUrl });
      return;
    } catch (err) {
      const delay = delays[attempt] ?? 30000;
      log.warn("start failed, retrying", { hubUrl, attempt: attempt + 1, delay, error: String(err) });
      await new Promise(r => setTimeout(r, delay));
    }
  }

  // All retries exhausted — show retry indicators so the user can retry manually.
  // The connection is in Disconnected state; handlers persist for restart.
  log.error("start failed after all retries", { hubUrl, attempts: maxAttempts });
  const managed = hubs.get(hubUrl);
  if (managed) showRetryForActiveSubscribers(hubUrl, managed);
}

function retryConnection(hubUrl: string): void {
  const managed = hubs.get(hubUrl);
  if (!managed) {
    log.warn("retry requested but no connection found", { hubUrl });
    removeRetryIndicators(hubUrl);
    return;
  }

  const { connection } = managed;
  if (connection.state !== signalR.HubConnectionState.Disconnected) {
    log.debug("retry skipped — not disconnected", { hubUrl, state: connection.state });
    return;
  }

  log.info("manual retry", { hubUrl });
  removeRetryIndicators(hubUrl);

  // Handlers persist on the connection — just restart it
  managed.startPromise = startWithRetry(connection, hubUrl);
}

function getOrCreate(hubUrl: string, signal?: AbortSignal): ManagedConnection {
  let managed = hubs.get(hubUrl);
  if (managed) return managed;

  const connection = new signalR.HubConnectionBuilder()
    .withUrl(hubUrl, { withCredentials: true })
    .withAutomaticReconnect()
    .configureLogging({
      log: (level: signalR.LogLevel, message: string) => {
        if (level >= signalR.LogLevel.Warning) log.warn("lib", { message });
        else if (level >= signalR.LogLevel.Information) log.debug("lib", { message });
      }
    })
    .build();

  // Library handles reconnection natively — handlers persist across reconnects.
  connection.onreconnecting(err => {
    log.warn("reconnecting", { hubUrl, error: err ? String(err) : undefined });
  });

  connection.onreconnected(connectionId => {
    log.info("reconnected", { hubUrl, connectionId });
    removeRetryIndicators(hubUrl);
  });

  connection.onclose(err => {
    // onclose fires for both intentional stop() AND retry exhaustion.
    // SignalR may or may not pass an error — we use the `stopping` flag
    // to distinguish intentional cleanup from connection loss.
    if (managed!.stopping) {
      log.debug("stopped", { hubUrl });
      hubs.delete(hubUrl);
    } else {
      log.warn("disconnected", { hubUrl, error: err ? String(err) : undefined });
      showRetryForActiveSubscribers(hubUrl, managed!);
    }
  });

  const startPromise = startWithRetry(connection, hubUrl);

  managed = {
    connection,
    startPromise,
      subscriptions: new Set<Subscription>(),
      dispatchers: new Map<string, (...args: unknown[]) => void>(),
      stopping: false,
  };
  hubs.set(hubUrl, managed);

  return managed;
}

function ensureDispatcher(managed: ManagedConnection, trigger: SignalRTrigger): void {
  if (managed.dispatchers.has(trigger.methodName)) return;

  const dispatcher = (...args: unknown[]) => {
    if (args.length !== 1 || typeof args[0] !== "object" || args[0] === null) {
      throw new Error(
        `[alis:signalr] ${trigger.hubUrl}/${trigger.methodName}: ` +
        `expected single object argument, got ${args.length} args (first: ${typeof args[0]})`
      );
    }

    const evt = args[0] as Record<string, unknown>;
    log.debug("method", { hubUrl: trigger.hubUrl, method: trigger.methodName });

    for (const subscription of activeSubscriptions(managed)) {
      if (subscription.trigger.methodName !== trigger.methodName) continue;
      executeReactionSequence(subscription.reactions, { evt, components: subscription.components }).catch(err =>
        log.error("reaction failed", { error: String(err) }));
    }
  };

  managed.connection.on(trigger.methodName, dispatcher);
  managed.dispatchers.set(trigger.methodName, dispatcher);
}

function removeSubscription(hubUrl: string, subscription: Subscription): void {
  const managed = hubs.get(hubUrl);
  if (!managed || !managed.subscriptions.has(subscription)) return;

  subscription.active = false;
  managed.subscriptions.delete(subscription);
  if (!activeSubscriptions(managed).some(active => active.trigger.methodName === subscription.trigger.methodName)) {
    const dispatcher = managed.dispatchers.get(subscription.trigger.methodName);
    if (dispatcher) {
      managed.connection.off(subscription.trigger.methodName, dispatcher);
      managed.dispatchers.delete(subscription.trigger.methodName);
    }
  }

  if (managed.subscriptions.size === 0) {
    managed.stopping = true;
    hubs.delete(hubUrl);
    void managed.connection.stop();
    removeRetryIndicators(hubUrl);
    return;
  }

  if (managed.connection.state === signalR.HubConnectionState.Disconnected && !managed.stopping) {
    showRetryForActiveSubscribers(hubUrl, managed);
  }
}

export function wireSignalR(
  trigger: SignalRTrigger,
  reactionOrReactions: Reaction | readonly Reaction[],
  components?: Record<string, ComponentEntry>,
  signal?: AbortSignal
): void {
  const reactions = Array.isArray(reactionOrReactions) ? reactionOrReactions : [reactionOrReactions];
  const managed = getOrCreate(trigger.hubUrl, signal);

  const targetIds = new Set<string>();
  for (const reaction of reactions) {
    const target = firstMutationTarget(reaction);
    if (target) targetIds.add(target);
  }

  const subscription: Subscription = {
    trigger,
    reactions,
    components,
    targetIds,
    active: true,
  };

  managed.subscriptions.add(subscription);
  ensureDispatcher(managed, trigger);

  log.debug("listening", { hubUrl: trigger.hubUrl, method: trigger.methodName });

  if (signal) {
    signal.addEventListener("abort", () => removeSubscription(trigger.hubUrl, subscription), { once: true });
  }
}
