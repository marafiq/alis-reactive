import type { ExecContext, PlanAction } from "../types";
import { evaluatePredicateAsync } from "../conditions/conditions";
import { callMember, getObject, setMemberValue } from "../resolution/contracts";
import { evaluateValue } from "../resolution/values";
import { showServerErrors } from "../validation";
import { injectIntoObject } from "./inject";
import { executeRequest } from "./http";
import {
  addSpanEvent,
  attachEventTraceContext,
  endSpan,
  recordException,
  scope,
  setSpanStatus,
  startSpan,
} from "../core/trace";

const log = scope("execute");

export async function executeAction(action: PlanAction, ctx: ExecContext): Promise<void> {
  const span = startSpan(`alis.action.${action.kind}`, {
    parent: ctx.trace,
    attributes: actionAttributes(action, ctx),
  });
  const actionCtx = { ...ctx, trace: span.context };

  try {
    switch (action.kind) {
      case "sequence":
        for (const step of action.steps) {
          await executeAction(step, actionCtx);
        }
        setSpanStatus(span, "ok");
        return;

      case "branch":
        for (const branch of action.cases) {
          if (!branch.when || await evaluatePredicateAsync(branch.when, actionCtx)) {
            await executeAction(branch.run, actionCtx);
            setSpanStatus(span, "ok");
            return;
          }
        }
        setSpanStatus(span, "ok");
        return;

      case "parallel":
        try {
          await Promise.all(action.steps.map(step => executeAction(step, actionCtx)));
        } finally {
          if (action.onSettled) {
            await executeAction(action.onSettled, actionCtx);
          }
        }
        setSpanStatus(span, "ok");
        return;

      case "set":
        setMemberValue(ctx.plan, action.target.object, action.target.member, evaluateValue(action.value, actionCtx), actionCtx);
        setSpanStatus(span, "ok");
        return;

      case "call": {
        const args = (action.args ?? []).map(arg => evaluateValue(arg, actionCtx));
        callMember(ctx.plan, action.target.object, action.target.member, args, actionCtx);
        setSpanStatus(span, "ok");
        return;
      }

      case "dispatch": {
        const detail = action.detail ? evaluateValue(action.detail, actionCtx) : {};
        const event = new CustomEvent(action.name, { detail });
        const payloadAttributes = flattenPayloadAttributes(detail);
        if (Object.keys(payloadAttributes).length > 0) {
          addSpanEvent(span, "dispatch.payload", payloadAttributes);
        }
        attachEventTraceContext(event, span.context);
        document.dispatchEvent(event);
        setSpanStatus(span, "ok");
        return;
      }

      case "request":
        await executeRequest(action.request, actionCtx);
        setSpanStatus(span, "ok");
        return;

      case "inject": {
        const value = action.value ? evaluateValue(action.value, actionCtx) : actionCtx.response;
        if (typeof value !== "string") {
          throw new Error(`[alis] inject requires an html string payload, got ${typeof value}`);
        }
        injectIntoObject(actionCtx.plan, action.object, value);
        setSpanStatus(span, "ok");
        return;
      }

      case "show-validation-errors":
        if (actionCtx.response == null) {
          setSpanStatus(span, "ok");
          return;
        }
        if (!actionCtx.validation) {
          throw new Error(
            `[alis] show-validation-errors(\"${action.formId}\") requires request validation metadata.`
          );
        }
        showServerErrors(actionCtx.plan, actionCtx.validation, actionCtx.response);
        setSpanStatus(span, "ok");
        return;

      default:
        log.warn("unknown action kind", { kind: (action as { kind?: string }).kind });
        setSpanStatus(span, "ok");
        return;
    }
  } catch (error) {
    recordException(span, error);
    setSpanStatus(span, "error", error instanceof Error ? error.message : String(error));
    throw error;
  } finally {
    endSpan(span);
  }
}

export function firstRenderableTarget(action: PlanAction, ctx: ExecContext): string | undefined {
  switch (action.kind) {
    case "sequence":
      for (const step of action.steps) {
        const target = firstRenderableTarget(step, ctx);
        if (target) return target;
      }
      return undefined;

    case "branch":
      for (const branch of action.cases) {
        const target = firstRenderableTarget(branch.run, ctx);
        if (target) return target;
      }
      return undefined;

    case "parallel":
      for (const step of action.steps) {
        const target = firstRenderableTarget(step, ctx);
        if (target) return target;
      }
      return action.onSettled ? firstRenderableTarget(action.onSettled, ctx) : undefined;

    case "set":
    case "call":
      return resolveTargetElement(action.target.object, ctx);

    case "inject":
      return resolveTargetElement(action.object, ctx);

    case "request":
      return findRequestTarget(action.request, ctx);

    default:
      return undefined;
  }
}

function findRequestTarget(request: import("../types").RequestPlan, ctx: ExecContext): string | undefined {
  for (const action of request.before ?? []) {
    const target = firstRenderableTarget(action, ctx);
    if (target) return target;
  }

  for (const handler of request.onSuccess ?? []) {
    const target = firstRenderableTarget(handler.run, ctx);
    if (target) return target;
  }

  for (const handler of request.onError ?? []) {
    const target = firstRenderableTarget(handler.run, ctx);
    if (target) return target;
  }

  for (const action of request.onSettled ?? []) {
    const target = firstRenderableTarget(action, ctx);
    if (target) return target;
  }

  return request.next ? findRequestTarget(request.next, ctx) : undefined;
}

function resolveTargetElement(objectName: string, ctx: ExecContext): string | undefined {
  if (objectName === "$eventObject") return undefined;
  return getObject(ctx.plan, objectName).elementId;
}

function actionAttributes(action: PlanAction, ctx: ExecContext): Record<string, unknown> {
  const base = {
    "alis.plan_id": ctx.plan.planId,
    "alis.action.kind": action.kind,
  };

  switch (action.kind) {
    case "set":
    case "call":
      return {
        ...base,
        "alis.object": action.target.object,
        "alis.member": action.target.member,
        "alis.target_element_id": resolveTargetElement(action.target.object, ctx),
      };

    case "dispatch":
      return {
        ...base,
        "alis.event_name": action.name,
      };

    case "inject":
      return {
        ...base,
        "alis.object": action.object,
        "alis.target_element_id": resolveTargetElement(action.object, ctx),
      };

    case "request":
      return {
        ...base,
        "http.request.method": action.request.method,
        "url.full": action.request.url,
      };

    case "show-validation-errors":
      return {
        ...base,
        "alis.form_id": action.formId,
      };

    case "sequence":
      return {
        ...base,
        "alis.step_count": action.steps.length,
      };

    case "parallel":
      return {
        ...base,
        "alis.step_count": action.steps.length,
      };

    case "branch":
      return {
        ...base,
        "alis.case_count": action.cases.length,
      };

    default:
      return base;
  }
}

function flattenPayloadAttributes(detail: unknown): Record<string, unknown> {
  if (detail == null || typeof detail !== "object" || Array.isArray(detail)) {
    return {};
  }

  const attributes: Record<string, unknown> = {};
  for (const [key, value] of Object.entries(detail)) {
    if (value == null) {
      attributes[`alis.payload.${key}`] = null;
      continue;
    }

    if (typeof value === "string"
      || typeof value === "number"
      || typeof value === "boolean") {
      attributes[`alis.payload.${key}`] = value;
      continue;
    }

    if (value instanceof Date) {
      attributes[`alis.payload.${key}`] = value.toISOString();
    }
  }

  return attributes;
}
