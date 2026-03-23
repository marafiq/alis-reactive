import type { ServerPushTrigger, Reaction, ComponentEntry } from "../types";
import { executeReaction } from "./execute";
import { showRetryIndicators, removeRetryIndicators, firstMutationTarget } from "./retry-indicator";
import { scope } from "../core/trace";

const log = scope("server-push");

interface WiredEntry {
  trigger: ServerPushTrigger;
  reaction: Reaction;
  components?: Record<string, ComponentEntry>;
  signal?: AbortSignal;
}

interface ManagedSource {
  es: EventSource;
  targetIds: Set<string>;
  wired: WiredEntry[];
}

// Connection pool — singleton EventSource per URL
const sources = new Map<string, ManagedSource>();

function retrySSE(url: string, entries: WiredEntry[]): void {
  removeRetryIndicators(url);
  log.info("manual retry", { url });

  // Re-wire all triggers — creates a fresh EventSource and re-registers handlers
  for (const entry of entries) {
    wireServerPush(entry.trigger, entry.reaction, entry.components, entry.signal);
  }
}

function getOrCreate(url: string, signal?: AbortSignal): ManagedSource {
  const cached = sources.get(url);
  if (cached) return cached;

  const es = new EventSource(url);
  const targetIds = new Set<string>();
  const wired: WiredEntry[] = [];

  es.onopen = () => {
    log.debug("connected", { url });
    removeRetryIndicators(url);
  };

  es.onerror = () => {
    if (es.readyState === EventSource.CLOSED) {
      log.error("connection closed permanently", { url });
      const managed = sources.get(url);
      sources.delete(url);
      if (managed && managed.targetIds.size > 0) {
        const entries = managed.wired;
        showRetryIndicators(url, managed.targetIds, () => retrySSE(url, entries));
      }
    } else {
      log.warn("connection error (reconnecting)", { url });
    }
  };

  if (signal) {
    signal.addEventListener("abort", () => {
      es.close();
      sources.delete(url);
      log.debug("closed", { url });
    });
  }

  const managed: ManagedSource = { es, targetIds, wired };
  sources.set(url, managed);
  log.debug("created", { url });
  return managed;
}

export function wireServerPush(
  trigger: ServerPushTrigger,
  reaction: Reaction,
  components?: Record<string, ComponentEntry>,
  signal?: AbortSignal
): void {
  const managed = getOrCreate(trigger.url, signal);

  // Track the first target element for retry indicator placement
  const target = firstMutationTarget(reaction);
  if (target) managed.targetIds.add(target);

  // Store wiring info for retry re-registration
  managed.wired.push({ trigger, reaction, components, signal });

  const handler = (e: MessageEvent) => {
    // Framework only supports JSON payloads — non-JSON is a server-side bug.
    // Throw immediately so the developer fixes their SSE endpoint.
    const evt: Record<string, unknown> = JSON.parse(e.data);
    log.debug("message", { url: trigger.url, eventType: trigger.eventType });
    executeReaction(reaction, { evt, components }).catch(err =>
      log.error("reaction failed", { error: String(err) }));
  };

  // Always use addEventListener — onmessage assignment overwrites previous handlers
  // when multiple triggers share the same URL without an eventType.
  const eventName = trigger.eventType ?? "message";
  managed.es.addEventListener(eventName, handler as EventListener);
  log.debug("listening", { url: trigger.url, eventType: eventName });
}
