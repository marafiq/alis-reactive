// alis.hub (grammar.md 6.7), keyed by hub URL. Shipped with the realtime bundle.
// B1 One HubConnection per URL, counted by subscriptions, with credentials and automatic reconnect.
// B2 Starting retries at 0, 2, 10 and 30 seconds, then shows the retry indicator.
// B3 Client methods are events, matched without regard to case; the payload is the single argument.
// B4 invoke(method, ...args) and send(method, ...args) are async; state and connectionId are
//    properties; reconnecting, reconnected and close are events.

import * as signalR from "@microsoft/signalr";
import type { Module } from "../objects/modules";
import type { Listener } from "../objects/vendor-driver";
import { showRetryIndicator, removeRetryIndicator } from "./retry-indicator";
import { scope } from "../diagnostics/trace";
import { toJavaScriptString } from "../shared/javascript-string";

const log = scope("signalr");

interface Managed {
  readonly connection: signalR.HubConnection;
  started: Promise<void>;
  subscriptions: number;
  stopping: boolean;
  readonly lifecycle: Map<string, Set<Listener>>;
}

const hubs = new Map<string, Managed>();
const startDelays = [0, 2000, 10000, 30000] as const;

async function startWithRetry(managed: Managed, url: string): Promise<void> {
  for (let attempt = 0; attempt < startDelays.length; attempt++) {
    try {
      await managed.connection.start();
      log.info("connected", { hubUrl: url });
      removeRetryIndicator(url);
      return;
    } catch (error) {
      const delay = startDelays[attempt]!;
      log.warn("start.retry", { hubUrl: url, attempt: attempt + 1, delay, error: toJavaScriptString(error) });
      await new Promise(resolve => setTimeout(resolve, delay));
    }
  }
  log.error("start.failed", { hubUrl: url, attempts: startDelays.length });
  if (hubs.get(url) === managed) showRetryIndicator(url, () => retry(url));
}

function retry(url: string): void {
  const managed = hubs.get(url);
  if (!managed) {
    removeRetryIndicator(url);
    return;
  }
  if (managed.connection.state !== signalR.HubConnectionState.Disconnected) return;
  log.info("retry.manual", { hubUrl: url });
  managed.started = startWithRetry(managed, url);
}

function emitLifecycle(managed: Managed, event: string, payload: unknown): void {
  for (const listener of managed.lifecycle.get(event) ?? []) listener(payload);
}

function managedFor(url: string): Managed {
  const existing = hubs.get(url);
  if (existing) return existing;

  const connection = new signalR.HubConnectionBuilder()
    .withUrl(url, { withCredentials: true })
    .withAutomaticReconnect()
    .configureLogging({
      log: (level: signalR.LogLevel, message: string) => {
        if (level >= signalR.LogLevel.Information) log.debug("lib.forwarded", { level, message });
      },
    })
    .build();

  const managed: Managed = {
    connection,
    started: Promise.resolve(),
    subscriptions: 0,
    stopping: false,
    lifecycle: new Map(),
  };

  connection.onreconnecting(error => {
    log.warn("connection.reconnecting", { hubUrl: url, error: error ? toJavaScriptString(error) : undefined });
    emitLifecycle(managed, "reconnecting", {});
  });
  connection.onreconnected(connectionId => {
    log.info("connection.reconnected", { hubUrl: url, connectionId });
    removeRetryIndicator(url);
    emitLifecycle(managed, "reconnected", { connectionId });
  });
  connection.onclose(error => {
    if (managed.stopping) {
      log.debug("connection.stopped", { hubUrl: url });
      return;
    }
    log.warn("connection.disconnected", { hubUrl: url, error: error ? toJavaScriptString(error) : undefined });
    showRetryIndicator(url, () => retry(url));
    emitLifecycle(managed, "close", {});
  });

  hubs.set(url, managed);
  managed.started = startWithRetry(managed, url);
  return managed;
}

function release(url: string, managed: Managed): void {
  managed.subscriptions -= 1;
  if (managed.subscriptions > 0) return;
  managed.stopping = true;
  void managed.connection.stop();
  if (hubs.get(url) === managed) hubs.delete(url);
}

const LIFECYCLE_EVENTS = new Set(["reconnecting", "reconnected", "close"]);

export const hubModule: Module = {
  object: url => {
    const managed = managedFor(url);
    return {
      get state() {
        return managed.connection.state;
      },
      get connectionId() {
        return managed.connection.connectionId;
      },
      invoke: async (method: string, ...args: unknown[]) => {
        await managed.started;
        return managed.connection.invoke(method, ...args);
      },
      send: async (method: string, ...args: unknown[]) => {
        await managed.started;
        return managed.connection.send(method, ...args);
      },
    };
  },
  subscribe(url: string, event: string, listener: Listener, owner) {
    const managed = managedFor(url);
    managed.subscriptions += 1;

    if (LIFECYCLE_EVENTS.has(event)) {
      let listeners = managed.lifecycle.get(event);
      if (!listeners) managed.lifecycle.set(event, (listeners = new Set()));
      listeners.add(listener);
      owner.onDispose(() => {
        listeners!.delete(listener);
        release(url, managed);
      });
      return;
    }

    const handler = (...args: unknown[]): void => {
      log.debug("method.received", { hubUrl: url, method: event });
      const payload = args[0];
      const isSingleObject = args.length === 1 && typeof payload === "object" && payload !== null && !Array.isArray(payload);
      if (!isSingleObject) {
        log.error("method.payload-unexpected", { hubUrl: url, method: event, count: args.length });
        return;
      }
      listener(payload);
    };
    managed.connection.on(event, handler);
    owner.onDispose(() => {
      managed.connection.off(event, handler);
      release(url, managed);
    });
    log.debug("method.listening", { hubUrl: url, method: event });
  },
};
