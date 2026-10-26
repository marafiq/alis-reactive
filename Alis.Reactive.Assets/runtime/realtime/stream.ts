// alis.stream (grammar.md 6.6), keyed by URL, in the framework's bundle.
// R1 One EventSource per URL, counted by subscriptions, closed when the last one is released.
// R2 A named event subscribes by its name; unnamed messages arrive as "message". The payload is
//    JSON.parse(data).
// R3 A permanent close adds a marker to the retry indicator; one click retries every dropped
//    connection, and the retried subscriptions keep their owners.

import type { Module } from "../objects/modules";
import type { Listener } from "../objects/vendor-driver";
import { showRetryIndicator, removeRetryIndicator } from "./retry-indicator";
import { logger } from "../diagnostics/trace";
import { toJavaScriptString } from "../shared/javascript-string";

const log = logger("server-push");

interface Subscription {
  readonly event: string;
  readonly listener: EventListener;
}

interface Managed {
  source: EventSource;
  readonly subscriptions: Set<Subscription>;
  stopping: boolean;
}

const streams = new Map<string, Managed>();

function open(url: string, managed?: Managed): EventSource {
  const source = new EventSource(url);
  source.onopen = () => {
    log.debug("connected", { url });
    removeRetryIndicator(url);
  };
  source.onerror = () => {
    const current = streams.get(url);
    if (!current || current.stopping || current.source !== source) return;
    if (source.readyState === EventSource.CLOSED) {
      log.error("connection.closed-permanent", { url });
      showRetryIndicator(url, () => retry(url));
    } else {
      log.warn("connection.reconnecting", { url });
    }
  };
  if (managed) {
    for (const subscription of managed.subscriptions) source.addEventListener(subscription.event, subscription.listener);
  }
  return source;
}

function managedFor(url: string): Managed {
  let managed = streams.get(url);
  if (!managed) {
    managed = { source: open(url), subscriptions: new Set(), stopping: false };
    streams.set(url, managed);
    log.debug("source.created", { url });
  }
  return managed;
}

function retry(url: string): void {
  const managed = streams.get(url);
  if (!managed) return;
  log.info("retry.manual", { url });
  managed.source.close();
  managed.source = open(url, managed);
}

function release(url: string, managed: Managed, subscription: Subscription): void {
  managed.source.removeEventListener(subscription.event, subscription.listener);
  managed.subscriptions.delete(subscription);
  if (managed.subscriptions.size > 0) return;

  managed.stopping = true;
  managed.source.close();
  if (streams.get(url) === managed) streams.delete(url);
  removeRetryIndicator(url);
  log.debug("connection.closed", { url });
}

export const streamModule: Module = {
  object: url => ({
    // A URL no handler listens to has no source: it reads as closed.
    get readyState() {
      return streams.get(url)?.source.readyState ?? EventSource.CLOSED;
    },
    close: () => streams.get(url)?.source.close(),
  }),
  subscribe(url: string, event: string, listener: Listener, owner) {
    const managed = managedFor(url);
    const onMessage = ((message: MessageEvent) => {
      log.debug("message.received", { url, event });
      let payload: unknown;
      try {
        payload = JSON.parse(message.data);
      } catch (error) {
        log.error("message.unreadable", { error: toJavaScriptString(error), url, event });
        return;
      }
      listener(payload);
    }) as EventListener;
    const subscription: Subscription = { event, listener: onMessage };
    managed.subscriptions.add(subscription);
    managed.source.addEventListener(event, onMessage);
    owner.onDispose(() => release(url, managed, subscription));
    log.debug("event.listening", { url, event });
  },
};

export function resetStreamsForTests(): void {
  for (const managed of streams.values()) {
    managed.stopping = true;
    managed.source.close();
  }
  streams.clear();
}
