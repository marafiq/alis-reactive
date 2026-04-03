import type { ExecContext, PlanAction, PlanSubscription, ServerPushSubscription } from "../types";
import { executeAction } from "./execute";
import { firstMutationTarget, removeRetryIndicators, showRetryIndicators } from "./retry-indicator";
import { scope } from "../core/trace";

const log = scope("server-push");

interface ManagedSource {
  readonly es: EventSource;
  readonly targetIds: Set<string>;
  readonly wired: WiredWorkflow[];
  stopping: boolean;
}

const sources = new Map<string, ManagedSource>();

interface WiredWorkflow {
  readonly subscription: ServerPushSubscription;
  readonly action: PlanAction;
  readonly ctx: ExecContext;
}

function retrySSE(url: string, workflows: readonly WiredWorkflow[]): void {
  removeRetryIndicators(url);
  log.info("manual retry", { url });

  for (const workflow of workflows) {
    wireServerPush(workflow.subscription, workflow.action, workflow.ctx);
  }
}

function getOrCreate(url: string, signal?: AbortSignal): ManagedSource {
  const cached = sources.get(url);
  if (cached) return cached;

  const es = new EventSource(url);
  const targetIds = new Set<string>();
  const wired: WiredWorkflow[] = [];

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
        showRetryIndicators(url, managed.targetIds, () => retrySSE(url, managed.wired));
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

  return managed;
}

export function wireServerPush(
  subscription: ServerPushSubscription,
  action: PlanAction,
  ctx: ExecContext,
  signal?: AbortSignal
): void {
  const managed = getOrCreate(subscription.url, signal);
  const target = firstMutationTarget(action, ctx);
  if (target) managed.targetIds.add(target);
  managed.wired.push({ subscription, action, ctx });

  const handler = (event: MessageEvent) => {
    const detail: Record<string, unknown> = JSON.parse(event.data);
    executeAction(action, { ...ctx, event: detail }).catch(error =>
      log.error("action failed", { error: String(error) })
    );
  };

  const eventName = subscription.eventType ?? "message";
  managed.es.addEventListener(eventName, handler as EventListener);
  log.debug("listening", { url: subscription.url, eventType: eventName });
}
