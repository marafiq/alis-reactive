// signalr.ts — SignalR trigger wiring.
// Uses SignalRTrigger from the plan schema.

import * as signalR from "@microsoft/signalr";
import type { SignalRTrigger, Reaction, Plan } from "../types";
import { runReaction } from "./trigger";
import { showRetryIndicators, removeRetryIndicators } from "./retry-indicator";
import { tracer } from "../tracing";

const t = tracer("signalr");

interface ManagedConnection {
  readonly connection: signalR.HubConnection;
  startPromise: Promise<void>;
  readonly targetIds: Set<string>;
  stopping: boolean;
}

// Connection pool — singleton HubConnection per hubUrl
const hubs = new Map<string, ManagedConnection>();

async function startWithRetry(connection: signalR.HubConnection, hubUrl: string): Promise<void> {
  const maxAttempts = 4;
  const delays = [0, 2000, 10000, 30000];

  for (let attempt = 0; attempt < maxAttempts; attempt++) {
    try {
      await connection.start();
      t.info("signalr.connection.open", { hubUrl });
      return;
    } catch (err) {
      const delay = delays[attempt] ?? 30000;
      t.warn(
        "signalr.start.retry",
        { hubUrl, attempt: attempt + 1, delay },
        err instanceof Error ? err : new Error(String(err)),
      );
      await new Promise(r => setTimeout(r, delay));
    }
  }

  t.error(
    "signalr.start.fail",
    { hubUrl, attempts: maxAttempts },
    new Error(`SignalR connection failed after ${maxAttempts} retry attempts`),
  );
  const managed = hubs.get(hubUrl);
  if (managed) showRetryIndicators(hubUrl, managed.targetIds, () => retryConnection(hubUrl));
}

function retryConnection(hubUrl: string): void {
  const managed = hubs.get(hubUrl);
  if (!managed) {
    t.warn("signalr.retry.no-connection", { hubUrl });
    removeRetryIndicators(hubUrl);
    return;
  }

  const { connection } = managed;
  if (connection.state !== signalR.HubConnectionState.Disconnected) {
    t.debug("signalr.retry.skip", { hubUrl, state: connection.state });
    return;
  }

  t.info("signalr.retry", { hubUrl });
  removeRetryIndicators(hubUrl);
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
        if (level >= signalR.LogLevel.Warning) t.warn("signalr.lib", { message });
        else if (level >= signalR.LogLevel.Information) t.debug("signalr.lib", { message });
      }
    })
    .build();

  const targetIds = new Set<string>();

  connection.onreconnecting(err => {
    t.warn(
      "signalr.reconnect",
      { hubUrl },
      err ? (err instanceof Error ? err : new Error(String(err))) : undefined,
    );
  });

  connection.onreconnected(connectionId => {
    t.info("signalr.connection.restore", { hubUrl, connectionId });
    removeRetryIndicators(hubUrl);
  });

  connection.onclose(err => {
    if (managed!.stopping) {
      t.debug("signalr.connection.stop", { hubUrl });
      hubs.delete(hubUrl);
    } else {
      t.warn(
        "signalr.connection.drop",
        { hubUrl },
        err ? (err instanceof Error ? err : new Error(String(err))) : undefined,
      );
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
  plan: Plan,
  signal?: AbortSignal,
): void {
  const managed = getOrCreate(trigger.hubUrl, signal);
  const { connection } = managed;

  connection.on(trigger.method, (...args: unknown[]) => {
    if (args.length !== 1 || typeof args[0] !== "object" || args[0] === null) {
      throw new Error(
        `[alis:signalr] ${trigger.hubUrl}/${trigger.method}: ` +
        `expected single object argument, got ${args.length} args (first: ${typeof args[0]})`
      );
    }

    const evt = args[0] as Record<string, unknown>;
    t.debug("signalr.method", { hubUrl: trigger.hubUrl, method: trigger.method });
    runReaction(reaction, plan, { event: evt }, "signalr", {
      hubUrl: trigger.hubUrl,
      method: trigger.method,
    });
  });

  t.debug("signalr.listen", { hubUrl: trigger.hubUrl, method: trigger.method });
}
