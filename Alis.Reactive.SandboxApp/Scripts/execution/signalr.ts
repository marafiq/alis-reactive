import * as signalR from "@microsoft/signalr";
import type { ExecContext, PlanAction, SignalRSubscription } from "../types";
import { executeAction } from "./execute";
import { firstMutationTarget, removeRetryIndicators, showRetryIndicators } from "./retry-indicator";
import { scope } from "../core/trace";

const log = scope("signalr");

interface ManagedConnection {
  readonly connection: signalR.HubConnection;
  startPromise: Promise<void>;
  readonly targetIds: Set<string>;
  stopping: boolean;
}

const hubs = new Map<string, ManagedConnection>();

async function startWithRetry(connection: signalR.HubConnection, hubUrl: string): Promise<void> {
  const maxAttempts = 4;
  const delays = [0, 2000, 10000, 30000];

  for (let attempt = 0; attempt < maxAttempts; attempt++) {
    try {
      await connection.start();
      log.info("connected", { hubUrl });
      return;
    } catch (error) {
      const delay = delays[attempt] ?? 30000;
      log.warn("start failed, retrying", { hubUrl, attempt: attempt + 1, delay, error: String(error) });
      await new Promise(resolve => setTimeout(resolve, delay));
    }
  }

  log.error("start failed after all retries", { hubUrl, attempts: maxAttempts });
  const managed = hubs.get(hubUrl);
  if (managed) showRetryIndicators(hubUrl, managed.targetIds, () => retryConnection(hubUrl));
}

function retryConnection(hubUrl: string): void {
  const managed = hubs.get(hubUrl);
  if (!managed) {
    removeRetryIndicators(hubUrl);
    return;
  }

  if (managed.connection.state !== signalR.HubConnectionState.Disconnected) {
    return;
  }

  removeRetryIndicators(hubUrl);
  managed.startPromise = startWithRetry(managed.connection, hubUrl);
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

  connection.onreconnecting(error => {
    log.warn("reconnecting", { hubUrl, error: error ? String(error) : undefined });
  });

  connection.onreconnected(connectionId => {
    log.info("reconnected", { hubUrl, connectionId });
    removeRetryIndicators(hubUrl);
  });

  connection.onclose(error => {
    if (managed!.stopping) {
      hubs.delete(hubUrl);
      return;
    }

    log.warn("disconnected", { hubUrl, error: error ? String(error) : undefined });
    showRetryIndicators(hubUrl, targetIds, () => retryConnection(hubUrl));
  });

  managed = {
    connection,
    startPromise: startWithRetry(connection, hubUrl),
    targetIds,
    stopping: false,
  };

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
  subscription: SignalRSubscription,
  action: PlanAction,
  ctx: ExecContext,
  signal?: AbortSignal
): void {
  const managed = getOrCreate(subscription.hubUrl, signal);
  const target = firstMutationTarget(action, ctx);
  if (target) managed.targetIds.add(target);

  managed.connection.on(subscription.method, (...args: unknown[]) => {
    if (args.length !== 1 || typeof args[0] !== "object" || args[0] === null) {
      throw new Error(
        `[alis:signalr] ${subscription.hubUrl}/${subscription.method}: expected single object argument`
      );
    }

    executeAction(action, { ...ctx, event: args[0] }).catch(error =>
      log.error("action failed", { error: String(error) })
    );
  });
}
