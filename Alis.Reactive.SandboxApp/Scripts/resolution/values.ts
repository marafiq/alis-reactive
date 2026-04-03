import type {
  BindingMapValueExpr,
  ExecContext,
  Plan,
  RequestInputValue,
  ValueExpr,
  ValueShape,
} from "../types";
import { coerce, toArray } from "../core/coerce";
import { walkSegments } from "../core/walk";
import { getBindingShape, getBindingValue, readMemberValue } from "./contracts";

function convertScalar(value: unknown, type: string): unknown {
  const result = coerce(value, type as any);
  return result.ok ? result.value : undefined;
}

function convertRequestScalar(value: unknown, type: string): unknown {
  if (type !== "date") {
    return convertScalar(value, type);
  }

  if (value == null || value === "") return undefined;
  if (value instanceof Date) return value;
  if (typeof value === "number") {
    return Number.isFinite(value) ? new Date(value) : undefined;
  }
  if (typeof value === "string") return value;
  return undefined;
}

export function applyShape(value: unknown, shape: ValueShape): unknown {
  switch (shape.kind) {
    case "any":
      return value;
    case "scalar":
      return convertScalar(value, shape.type);
    case "array": {
      const result = toArray(value);
      if (!result.ok) return undefined;
      return result.value.map(item => applyShape(item, shape.item));
    }
    case "object": {
      if (value == null || typeof value !== "object" || Array.isArray(value)) {
        return undefined;
      }

      const fields = shape.fields;
      if (!fields) return value;

      const source = value as Record<string, unknown>;
      const projected: Record<string, unknown> = {};
      for (const [name, fieldShape] of Object.entries(fields)) {
        projected[name] = applyShape(source[name], fieldShape);
      }

      if (shape.additional) {
        for (const [name, item] of Object.entries(source)) {
          if (!(name in projected)) projected[name] = item;
        }
      }

      return projected;
    }
    default:
      return value;
  }
}

export function applyRequestShape(value: unknown, shape: ValueShape): unknown {
  switch (shape.kind) {
    case "any":
      return value;
    case "scalar":
      return convertRequestScalar(value, shape.type);
    case "array": {
      const result = toArray(value);
      if (!result.ok) return undefined;
      return result.value.map(item => applyRequestShape(item, shape.item));
    }
    case "object": {
      if (value == null || typeof value !== "object" || Array.isArray(value)) {
        return undefined;
      }

      const fields = shape.fields;
      if (!fields) return value;

      const source = value as Record<string, unknown>;
      const projected: Record<string, unknown> = {};
      for (const [name, fieldShape] of Object.entries(fields)) {
        projected[name] = applyRequestShape(source[name], fieldShape);
      }

      if (shape.additional) {
        for (const [name, item] of Object.entries(source)) {
          if (!(name in projected)) projected[name] = item;
        }
      }

      return projected;
    }
    default:
      return value;
  }
}

export function evaluateValue(expr: ValueExpr, ctx: ExecContext): unknown {
  switch (expr.kind) {
    case "literal":
      return Object.prototype.hasOwnProperty.call(expr, "value") ? expr.value : null;

    case "binding":
      return getBindingValue(ctx.plan, expr.binding, ctx);

    case "member":
      return readMemberValue(ctx.plan, expr.object, expr.member, ctx);

    case "context": {
      const scopeRoot = resolveContextScope(expr.scope, ctx);
      return walkSegments(scopeRoot, expr.path);
    }

    case "object": {
      const value: Record<string, unknown> = {};
      for (const [key, child] of Object.entries(expr.fields)) {
        value[key] = evaluateValue(child, ctx);
      }
      return value;
    }

    case "array":
      return expr.items.map(item => evaluateValue(item, ctx));

    case "convert":
      return applyShape(evaluateValue(expr.value, ctx), expr.to);

    default:
      return undefined;
  }
}

export function evaluateRequestValue(expr: ValueExpr, ctx: ExecContext): unknown {
  switch (expr.kind) {
    case "literal":
      return Object.prototype.hasOwnProperty.call(expr, "value") ? expr.value : null;

    case "binding":
      return getBindingValue(ctx.plan, expr.binding, ctx);

    case "member":
      return readMemberValue(ctx.plan, expr.object, expr.member, ctx);

    case "context": {
      const scopeRoot = resolveContextScope(expr.scope, ctx);
      return walkSegments(scopeRoot, expr.path);
    }

    case "object": {
      const value: Record<string, unknown> = {};
      for (const [key, child] of Object.entries(expr.fields)) {
        value[key] = evaluateRequestValue(child, ctx);
      }
      return value;
    }

    case "array":
      return expr.items.map(item => evaluateRequestValue(item, ctx));

    case "convert":
      return applyRequestShape(evaluateRequestValue(expr.value, ctx), expr.to);

    default:
      return undefined;
  }
}

export function evaluateRequestInputValue(value: RequestInputValue, ctx: ExecContext): unknown {
  if ((value as BindingMapValueExpr).kind === "binding-map") {
    return evaluateBindingMap(value as BindingMapValueExpr, ctx.plan, ctx);
  }
  return evaluateRequestValue(value as ValueExpr, ctx);
}

export function evaluateBindingMap(map: BindingMapValueExpr, plan: Plan, ctx: ExecContext): Record<string, unknown> {
  const bindings = map.include === "all"
    ? Object.keys(plan.bindings)
    : map.include;

  const result: Record<string, unknown> = {};
  for (const binding of bindings) {
    setNested(result, binding, applyRequestShape(getBindingValue(plan, binding, ctx), getBindingShape(plan, binding)));
  }
  return result;
}

export function setNested(target: Record<string, unknown>, path: string, value: unknown): void {
  const parts = path.split(".");
  let current: Record<string, unknown> = target;

  for (let index = 0; index < parts.length - 1; index++) {
    const key = parts[index];
    const next = current[key];
    if (typeof next !== "object" || next == null || Array.isArray(next)) {
      current[key] = {};
    }
    current = current[key] as Record<string, unknown>;
  }

  current[parts[parts.length - 1]] = value;
}

function resolveContextScope(scope: "event" | "response" | "request" | "local", ctx: ExecContext): unknown {
  switch (scope) {
    case "event":
      return ctx.event;
    case "response":
      return ctx.response;
    case "request":
      return ctx.request;
    case "local":
      return ctx.local;
    default:
      return undefined;
  }
}
