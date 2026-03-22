import type { ServerPushTrigger, Reaction, ComponentEntry } from "../types";
import { executeReaction } from "./execute";
import { scope } from "../core/trace";

const log = scope("server-push");

// Connection pool — singleton EventSource per URL
const sources = new Map<string, EventSource>();

function getOrCreate(url: string, signal?: AbortSignal): EventSource {
  let es = sources.get(url);
  if (es) return es;

  es = new EventSource(url);
  sources.set(url, es);

  es.onerror = () => {
    if (es!.readyState === EventSource.CLOSED) {
      log.error("connection closed permanently", { url });
      sources.delete(url);
    } else {
      log.warn("connection error (reconnecting)", { url });
    }
  };

  if (signal) {
    signal.addEventListener("abort", () => {
      es!.close();
      sources.delete(url);
      log.debug("closed", { url });
    });
  }

  log.debug("connected", { url });
  return es;
}

export function wireServerPush(
  trigger: ServerPushTrigger,
  reaction: Reaction,
  components?: Record<string, ComponentEntry>,
  signal?: AbortSignal
): void {
  const es = getOrCreate(trigger.url, signal);

  const handler = (e: MessageEvent) => {
    let evt: Record<string, unknown>;
    try {
      evt = JSON.parse(e.data);
    } catch (err) {
      log.error("failed to parse SSE message", { url: trigger.url, error: String(err) });
      return;
    }
    log.debug("message", { url: trigger.url, eventType: trigger.eventType });
    executeReaction(reaction, { evt, components }).catch(err =>
      log.error("reaction failed", { error: String(err) }));
  };

  // Always use addEventListener — onmessage assignment overwrites previous handlers
  // when multiple triggers share the same URL without an eventType.
  const eventName = trigger.eventType ?? "message";
  es.addEventListener(eventName, handler as EventListener);
  log.debug("listening", { url: trigger.url, eventType: eventName });
}
