import * as signalR from "@microsoft/signalr";
import type { SignalRTrigger, Reaction, ComponentEntry } from "../types";
import { executeReaction } from "./execute";
import { showRetryIndicators, removeRetryIndicators, firstMutationTarget } from "./retry-indicator";
import { scope } from "../core/trace";

const log = scope("signalr");

interface ManagedConnection {
  readonly connection: signalR.HubConnection;
  startPromise: Promise<void>;
  readonly targetIds: Set<string>;
  stopping: boolean;
}

// Connection pool — singleton HubConnection per hubUrl
const hubs = new Map<string, ManagedConnection>();

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
  if (managed) showRetryIndicators(hubUrl, managed.targetIds, () => retryConnection(hubUrl));
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

  const targetIds = new Set<string>();

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
      showRetryIndicators(hubUrl, targetIds, () => retryConnection(hubUrl));
    }
  });

  const startPromise = startWithRetry(connection, hubUrl);

  managed = { connection, startPromise, targetIds, stopping: false };
  hubs.set(hubUrl, managed);

  if (signal) {
    signal.addEventListener("abort", () => {
      managed!.stopping = true;
      connection.stop();
    });
  }

  return managed;
}

export function wireSignalR(
  trigger: SignalRTrigger,
  reaction: Reaction,
  components?: Record<string, ComponentEntry>,
  signal?: AbortSignal
): void {
  const managed = getOrCreate(trigger.hubUrl, signal);
  const { connection, targetIds } = managed;

  // Track the first target element for retry indicator placement
  const target = firstMutationTarget(reaction);
  if (target) targetIds.add(target);

  // Handlers registered via .on() persist across automatic reconnects —
  // no re-registration needed (per Microsoft docs).
  // Trust the library's JSON deserialization — don't reshape the payload.
  connection.on(trigger.methodName, (...args: unknown[]) => {
    if (args.length !== 1 || typeof args[0] !== "object" || args[0] === null) {
      throw new Error(
        `[alis:signalr] ${trigger.hubUrl}/${trigger.methodName}: ` +
        `expected single object argument, got ${args.length} args (first: ${typeof args[0]})`
      );
    }

    const evt = args[0] as Record<string, unknown>;
    log.debug("method", { hubUrl: trigger.hubUrl, method: trigger.methodName });
    executeReaction(reaction, { evt, components }).catch(err =>
      log.error("reaction failed", { error: String(err) }));
  });

  log.debug("listening", { hubUrl: trigger.hubUrl, method: trigger.methodName });
}
