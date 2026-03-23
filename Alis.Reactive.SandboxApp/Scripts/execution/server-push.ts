import type { ServerPushTrigger, Reaction, ComponentEntry } from "../types";
import { executeReaction } from "./execute";
import { showRetryIndicators, removeRetryIndicators, firstMutationTarget } from "./retry-indicator";
import { scope } from "../core/trace";

const log = scope("server-push");

interface WiredEntry {
  readonly trigger: ServerPushTrigger;
  readonly reaction: Reaction;
  readonly components?: Record<string, ComponentEntry>;
}

interface ManagedSource {
  readonly es: EventSource;
  readonly targetIds: Set<string>;
  readonly wired: WiredEntry[];
  stopping: boolean;
}

// Connection pool — singleton EventSource per URL
const sources = new Map<string, ManagedSource>();

function retrySSE(url: string, entries: readonly WiredEntry[]): void {
  removeRetryIndicators(url);
  log.info("manual retry", { url });

  // Re-wire all triggers — creates a fresh EventSource and re-registers handlers.
  // Signal is intentionally omitted — the original abort context is no longer
  // meaningful for a fresh connection after manual retry.
  for (const entry of entries) {
    wireServerPush(entry.trigger, entry.reaction, entry.components);
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
    const managed = sources.get(url);
    if (managed?.stopping) return;

    if (es.readyState === EventSource.CLOSED) {
      log.error("connection closed permanently", { url });
      sources.delete(url);
      if (managed && managed.targetIds.size > 0) {
        const entries = managed.wired;
        showRetryIndicators(url, managed.targetIds, () => retrySSE(url, entries));
      }
    } else {
      log.warn("connection error (reconnecting)", { url });
    }
  };

  const managed: ManagedSource = { es, targetIds, wired, stopping: false };
  sources.set(url, managed);

  if (signal) {
    signal.addEventListener("abort", () => {
      managed.stopping = true;
      es.close();
      sources.delete(url);
      log.debug("closed", { url });
    });
  }

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

  // Store wiring info for retry re-registration (signal omitted — see retrySSE)
  managed.wired.push({ trigger, reaction, components });

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
