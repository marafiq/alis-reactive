import type {
  DocumentEventSubscription,
  EventContract,
  ExecContext,
  ObjectEventSubscription,
  PlanAction,
  PlanSubscription,
  ServerPushSubscription,
  SignalRSubscription,
} from "../types";
import { evaluateValue } from "../resolution/values";
import { getContract, getObject, resolveObjectRoot } from "../resolution/contracts";
import { executeAction } from "./execute";
import { wireServerPush } from "./server-push";
import { wireSignalR } from "./signalr";
import {
  addSpanEvent,
  attachEventTraceContext,
  endSpan,
  getEventTraceContext,
  recordException,
  scope,
  setSpanStatus,
  startSpan,
} from "../core/trace";

const log = scope("trigger");

export function wireWorkflow(
  subscription: PlanSubscription,
  action: PlanAction,
  ctx: ExecContext,
  signal?: AbortSignal
): void {
  const opts = signal ? { signal } : undefined;
  traceWorkflowListening(subscription, ctx);

  switch (subscription.kind) {
    case "dom-ready":
      if (document.readyState === "complete" || document.readyState === "interactive") {
        void runWorkflow(subscription, action, ctx);
      } else {
        document.addEventListener("DOMContentLoaded", () => {
          void runWorkflow(subscription, action, ctx);
        }, opts);
      }
      return;

    case "document-event":
      wireDocumentEvent(subscription, action, ctx, opts);
      return;

    case "object-event":
      wireObjectEvent(subscription, action, ctx, opts);
      return;

    case "server-push":
      wireServerPush(subscription, action, ctx, signal);
      return;

    case "signalr":
      wireSignalR(subscription, action, ctx, signal);
      return;

    default:
      throw new Error(`[alis] unsupported subscription kind: ${(subscription as { kind?: string }).kind}`);
  }
}

function wireDocumentEvent(
  subscription: DocumentEventSubscription,
  action: PlanAction,
  ctx: ExecContext,
  opts?: AddEventListenerOptions
): void {
  document.addEventListener(subscription.name, event => {
    const detail = (event as CustomEvent).detail ?? {};
    void runWorkflow(subscription, action, { ...ctx, event: detail }, event as Event);
  }, opts);
}

function wireObjectEvent(
  subscription: ObjectEventSubscription,
  action: PlanAction,
  ctx: ExecContext,
  opts?: AddEventListenerOptions
): void {
  const objectRef = getObject(ctx.plan, subscription.object);
  const contract = getContract(ctx.plan, objectRef.contract);
  const eventContract = contract.events?.[subscription.event];
  if (!eventContract) {
    throw new Error(`[alis] event "${subscription.event}" not found on object "${subscription.object}"`);
  }

  const root = resolveObjectRoot(ctx.plan, subscription.object, ctx) as EventTarget;
  root.addEventListener(eventContract.channel, eventObject => {
    const eventCtx = createEventContext(ctx, eventContract, eventObject);
    void runWorkflow(subscription, action, eventCtx, eventObject as Event);
  }, opts);
}

function createEventContext(base: ExecContext, eventContract: EventContract, eventObject: unknown): ExecContext {
  const plan = base.plan;
  const nextPlan = eventContract.eventObject
    ? {
        ...plan,
        contracts: {
          ...plan.contracts,
          $eventObject: getContract(plan, eventContract.eventObject.contract),
        }
      }
    : plan;

  const event =
    eventContract.data
      ? Object.fromEntries(
          Object.entries(eventContract.data).map(([name, expr]) => [
            name,
            evaluateValue(expr, { ...base, plan: nextPlan, eventObject })
          ])
        )
      : ((eventObject as Record<string, unknown> | null) ?? {});

  return {
    ...base,
    plan: nextPlan,
    event,
    eventObject,
  };
}

async function runWorkflow(
  subscription: PlanSubscription,
  action: PlanAction,
  ctx: ExecContext,
  eventObject?: Event
): Promise<void> {
  const parent = eventObject ? getEventTraceContext(eventObject) : undefined;
  const span = startSpan("alis.workflow.run", {
    parent,
    attributes: workflowAttributes(subscription, ctx),
  });

  try {
    if (eventObject) {
      attachEventTraceContext(eventObject, span.context);
    }

    addSpanEvent(span, "workflow.started");
    await executeAction(action, { ...ctx, trace: span.context });
    setSpanStatus(span, "ok");
  } catch (error) {
    recordException(span, error);
    setSpanStatus(span, "error", error instanceof Error ? error.message : String(error));
    log.error("action failed", { error: String(error) });
  } finally {
    endSpan(span);
  }
}

function traceWorkflowListening(subscription: PlanSubscription, ctx: ExecContext): void {
  const span = startSpan("alis.workflow.listen", {
    attributes: workflowAttributes(subscription, ctx),
  });
  addSpanEvent(span, "workflow.wired");
  setSpanStatus(span, "ok");
  endSpan(span);
}

function workflowAttributes(subscription: PlanSubscription, ctx: ExecContext): Record<string, unknown> {
  switch (subscription.kind) {
    case "dom-ready":
      return {
        "alis.plan_id": ctx.plan.planId,
        "alis.subscription.kind": subscription.kind,
      };

    case "document-event":
      return {
        "alis.plan_id": ctx.plan.planId,
        "alis.subscription.kind": subscription.kind,
        "alis.subscription.name": subscription.name,
      };

    case "object-event":
      return {
        "alis.plan_id": ctx.plan.planId,
        "alis.subscription.kind": subscription.kind,
        "alis.object": subscription.object,
        "alis.event": subscription.event,
      };

    case "server-push":
      return {
        "alis.plan_id": ctx.plan.planId,
        "alis.subscription.kind": subscription.kind,
        "alis.url": subscription.url,
        "alis.event_type": subscription.eventType,
      };

    case "signalr":
      return {
        "alis.plan_id": ctx.plan.planId,
        "alis.subscription.kind": subscription.kind,
        "alis.hub_url": subscription.hubUrl,
        "alis.method": subscription.method,
      };

    default:
      return {
        "alis.plan_id": ctx.plan.planId,
      };
  }
}
