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

  es.onerror = () => log.warn("connection error", { url });

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
    const evt: Record<string, unknown> = JSON.parse(e.data);
    log.debug("message", { url: trigger.url, eventType: trigger.eventType });
    executeReaction(reaction, { evt, components }).catch(err =>
      log.error("reaction failed", { error: String(err) }));
  };

  if (trigger.eventType) {
    es.addEventListener(trigger.eventType, handler as EventListener);
    log.debug("listening", { url: trigger.url, eventType: trigger.eventType });
  } else {
    es.onmessage = handler;
    log.debug("listening", { url: trigger.url, eventType: "(all)" });
  }
}
