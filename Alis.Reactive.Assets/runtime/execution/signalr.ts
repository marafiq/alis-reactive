// signalr.ts — SignalR trigger wiring.
// Uses SignalRTrigger from the generated plan contract.

import * as signalR from "@microsoft/signalr";
import type { SignalRTrigger, Reaction, Plan } from "../types";
import { catchAsyncReactionFailure, executeReaction } from "./execute";
import { showRetryIndicators, removeRetryIndicators } from "./retry-indicator";
import { scope } from "../core/trace";
import { ExecutionContext } from "../domain/execution-context";
import { objectRecordFrom } from "../domain/object-record";

const log = scope("signalr");

interface ManagedConnection {
  readonly connection: signalR.HubConnection;
  startPromise: Promise<void>;
  readonly targetIds: Set<string>;
  subscriptionCount: number;
  stopping: boolean;
}

// Connection pool — singleton HubConnection per hubUrl
const hubs = new Map<string, ManagedConnection>();
const reconnectDelays = [0, 2000, 10000, 30000] as const;

async function startWithRetry(connection: signalR.HubConnection, hubUrl: string): Promise<void> {
  for (let attempt = 0; attempt < reconnectDelays.length; attempt++) {
    try {
      await connection.start();
      log.info("connected", { hubUrl });
      return;
    } catch (err) {
      const delay = reconnectDelayForAttempt(attempt);
      log.warn("start.retry", { hubUrl, attempt: attempt + 1, delay, error: String(err) });
      await new Promise(r => setTimeout(r, delay));
    }
  }

  log.error("start.failed", { hubUrl, attempts: reconnectDelays.length });
  const managed = hubs.get(hubUrl);
  if (managed) showRetryIndicators(hubUrl, managed.targetIds, () => retryConnection(hubUrl));
}

function retryConnection(hubUrl: string): void {
  const managed = hubs.get(hubUrl);
  if (!managed) {
    log.warn("retry.no-connection", { hubUrl });
    removeRetryIndicators(hubUrl);
    return;
  }

  const { connection } = managed;
  if (connection.state !== signalR.HubConnectionState.Disconnected) {
    log.debug("retry.skipped", { hubUrl, state: connection.state });
    return;
  }

  log.info("retry.manual", { hubUrl });
  removeRetryIndicators(hubUrl);
  managed.startPromise = startWithRetry(connection, hubUrl);
}

function getOrCreate(hubUrl: string): ManagedConnection {
  let managed = hubs.get(hubUrl);
  if (managed) return managed;

  const connection = new signalR.HubConnectionBuilder()
    .withUrl(hubUrl, { withCredentials: true })
    .withAutomaticReconnect()
    .configureLogging({
      log: (level: signalR.LogLevel, message: string) => {
        if (level >= signalR.LogLevel.Warning) log.debug("lib.forwarded", { level: "warn", message });
        else if (level >= signalR.LogLevel.Information) log.debug("lib.forwarded", { level: "info", message });
      }
    })
    .build();

  const targetIds = new Set<string>();

  connection.onreconnecting(err => {
    log.warn("connection.reconnecting", { hubUrl, error: signalRErrorMessage(err) });
  });

  connection.onreconnected(connectionId => {
    log.info("connection.reconnected", { hubUrl, connectionId });
    removeRetryIndicators(hubUrl);
  });

  connection.onclose(err => {
    const currentConnection = managed;
    if (currentConnection === undefined) return;

    if (currentConnection.stopping) {
      log.debug("connection.stopped", { hubUrl });
      hubs.delete(hubUrl);
    } else {
      log.warn("connection.disconnected", { hubUrl, error: signalRErrorMessage(err) });
      showRetryIndicators(hubUrl, targetIds, () => retryConnection(hubUrl));
    }
  });

  const startPromise = startWithRetry(connection, hubUrl);

  managed = { connection, startPromise, targetIds, subscriptionCount: 0, stopping: false };
  hubs.set(hubUrl, managed);

  return managed;
}

export function wireSignalR(
  trigger: SignalRTrigger,
  reaction: Reaction,
  plan: Plan,
  signal?: AbortSignal,
): void {
  const managed = getOrCreate(trigger.hubUrl);
  const { connection } = managed;

  const handler = (...args: unknown[]) => {
    const evt = signalRInvocationPayload(args, trigger);
    log.debug("method.received", { hubUrl: trigger.hubUrl, method: trigger.method });
    catchAsyncReactionFailure(
      executeReaction(reaction, plan, ExecutionContext.event(evt).raw),
      err => log.error("reaction.failed", { hubUrl: trigger.hubUrl, method: trigger.method, error: String(err) }),
    );
  };

  connection.on(trigger.method, handler);
  managed.subscriptionCount += 1;
  signal?.addEventListener("abort", () => {
    connection.off(trigger.method, handler);
    releaseSignalRSubscription(trigger.hubUrl, managed);
  }, { once: true });

  log.debug("method.listening", { hubUrl: trigger.hubUrl, method: trigger.method });
}

function releaseSignalRSubscription(hubUrl: string, managed: ManagedConnection): void {
  managed.subscriptionCount -= 1;
  if (managed.subscriptionCount > 0) return;

  managed.stopping = true;
  managed.connection.stop();
  hubs.delete(hubUrl);
}

function reconnectDelayForAttempt(attempt: number): number {
  const configuredDelay = reconnectDelays[attempt];
  if (configuredDelay !== undefined) return configuredDelay;

  throw new Error(`[alis:signalr] retry attempt ${attempt} is outside the reconnect delay schedule`);
}

function signalRErrorMessage(error: unknown): string | undefined {
  const errorWasProvided = error !== null && error !== undefined;
  if (!errorWasProvided) return undefined;

  return String(error);
}

function signalRInvocationPayload(args: unknown[], trigger: SignalRTrigger): Record<string, unknown> {
  const payload = args[0];
  const payloadIsSingleArgument = args.length === 1;
  const payloadRecord = objectRecordFrom(payload);
  const payloadMatchesContract = payloadIsSingleArgument && payloadRecord !== undefined;

  if (!payloadMatchesContract) {
    throw new Error(
      `[alis:signalr] ${trigger.hubUrl}/${trigger.method}: ` +
      `expected single object argument, got ${args.length} args (first: ${typeof payload})`
    );
  }

  return payloadRecord;
}
