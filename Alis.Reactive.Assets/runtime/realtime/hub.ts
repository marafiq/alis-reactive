import * as signalR from "@microsoft/signalr";
import type { Plugin } from "../objects/plugins";
import type { Owner } from "../objects/owner";
import type { Listener } from "../objects/vendor-driver";
import { showRetryIndicator, removeRetryIndicator } from "./retry-indicator";
import { logger } from "../diagnostics/trace";
import { isRecord } from "../shared/record";
import { toJavaScriptString } from "../shared/javascript-string";

const log = logger("signalr");

interface Managed {
  readonly connection: signalR.HubConnection;
  started: Promise<void>;
  holders: number;
  stopping: boolean;
  readonly lifecycle: Map<string, Set<Listener>>;
}

const hubs = new Map<string, Managed>();
const retryDelays = [0, 2000, 10000] as const;
const tries = retryDelays.length + 1;

async function startWithRetry(managed: Managed, url: string): Promise<void> {
  for (let attempt = 1; ; attempt += 1) {
    try {
      await managed.connection.start();
      log.info("connected", { hubUrl: url });
      removeRetryIndicator(url);
      return;
    } catch (error) {
      log.warn("start.retry", { hubUrl: url, attempt, error: toJavaScriptString(error) });
    }
    if (managed.stopping) return;
    if (attempt === tries) break;
    await new Promise(resolve => setTimeout(resolve, retryDelays[attempt - 1]));
    if (managed.stopping) return;
  }
  log.error("start.failed", { hubUrl: url, attempts: tries });
  showRetryIndicator(url, () => retry(managed, url));
}

function retry(managed: Managed, url: string): void {
  log.info("retry.manual", { hubUrl: url });
  managed.started = startWithRetry(managed, url);
}

function emitLifecycle(managed: Managed, event: string, payload: unknown): void {
  for (const listener of managed.lifecycle.get(event) ?? []) listener(payload);
}

function connect(url: string): Managed {
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
    holders: 0,
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
    showRetryIndicator(url, () => retry(managed, url));
    emitLifecycle(managed, "close", {});
  });

  hubs.set(url, managed);
  managed.started = startWithRetry(managed, url);
  return managed;
}

function hold(url: string, owner: Owner): void {
  const managed = hubs.get(url) ?? connect(url);
  managed.holders += 1;
  owner.onDispose(() => release(url, managed));
}

function release(url: string, managed: Managed): void {
  managed.holders -= 1;
  if (managed.holders > 0) return;
  managed.stopping = true;
  void managed.connection.stop();
  hubs.delete(url);
  removeRetryIndicator(url);
}

function held(url: string): Managed {
  const managed = hubs.get(url);
  if (!managed) throw new Error(`[alis] hub "${url}" is held by no owner: the plan that named it was released`);
  return managed;
}

const LIFECYCLE_EVENTS = new Set(["reconnecting", "reconnected", "close"]);

export const hubPlugin: Plugin = {
  object: url => {
    const managed = held(url);
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
  contribute({ key, owner }) {
    hold(key, owner);
  },
  subscribe(url: string, event: string, listener: Listener, owner) {
    const managed = held(url);

    if (LIFECYCLE_EVENTS.has(event)) {
      const listeners = managed.lifecycle.get(event) ?? new Set<Listener>();
      managed.lifecycle.set(event, listeners);
      listeners.add(listener);
      owner.onDispose(() => listeners.delete(listener));
      return;
    }

    const onMethod = (...args: unknown[]): void => {
      log.debug("method.received", { hubUrl: url, method: event });
      const payload = args[0];
      const sentNothing = args.length === 0;
      const sentOneObject = args.length === 1 && isRecord(payload);
      if (!sentNothing && !sentOneObject) {
        log.error("method.payload-unexpected", { hubUrl: url, method: event, count: args.length });
        return;
      }
      listener(payload);
    };
    managed.connection.on(event, onMethod);
    owner.onDispose(() => managed.connection.off(event, onMethod));
    log.debug("method.listening", { hubUrl: url, method: event });
  },
};
