// conditions.ts — V3 Condition evaluation.
// Uses SHARED resolver for value resolution.
// Condition is a discriminated union: compare, all, any, not, confirm.

import type { Condition, CompareCondition, Plan, ValueProducer } from "../types";
import type { ExecContext } from "../types";
import { scope } from "../core/trace";
import { assertNever } from "../core/assert-never";
import { applyShape, toString } from "../core/shape-convert";

const log = scope("conditions");

// evaluateValue lives in execute.ts which imports conditions.ts.
// Break the cycle: callers set the evaluator once at boot time.
let _evaluateValue: ((producer: ValueProducer, plan: Plan, ctx?: ExecContext) => unknown) | undefined;

/** Called once by execute.ts to inject the evaluateValue function and break the circular dependency. */
export function setValueEvaluator(fn: (producer: ValueProducer, plan: Plan, ctx?: ExecContext) => unknown): void {
  _evaluateValue = fn;
}

function getEvaluateValue(): (producer: ValueProducer, plan: Plan, ctx?: ExecContext) => unknown {
  if (!_evaluateValue) throw new Error("[alis] conditions: evaluateValue not set — was setValueEvaluator called?");
  return _evaluateValue;
}

/** Sync condition evaluation. Confirm conditions return false in sync context. */
export function evaluateCondition(condition: Condition, plan: Plan, ctx?: ExecContext): boolean {
  switch (condition.kind) {
    case "compare":
      return evaluateCompare(condition, plan, ctx);
    case "all":
      return condition.terms.every(t => evaluateCondition(t, plan, ctx));
    case "any":
      return condition.terms.some(t => evaluateCondition(t, plan, ctx));
    case "not":
      return !evaluateCondition(condition.term, plan, ctx);
    case "confirm":
      log.warn("ConfirmCondition in sync context — denying (callers should use async path)");
      return false;
    default:
      assertNever(condition, "condition kind");
  }
}

/** Async condition evaluation — required when conditions contain ConfirmCondition. */
export async function evaluateConditionAsync(condition: Condition, plan: Plan, ctx?: ExecContext): Promise<boolean> {
  switch (condition.kind) {
    case "compare":
      return evaluateCompare(condition, plan, ctx);
    case "all":
      for (const t of condition.terms) {
        if (!await evaluateConditionAsync(t, plan, ctx)) return false;
      }
      return true;
    case "any":
      for (const t of condition.terms) {
        if (await evaluateConditionAsync(t, plan, ctx)) return true;
      }
      return false;
    case "not":
      return !(await evaluateConditionAsync(condition.term, plan, ctx));
    case "confirm": {
      const confirmFn = (window as any).alis?.confirm;
      if (!confirmFn) throw new Error("[alis] confirm condition requires @Html.FusionConfirmDialog() in layout");
      return confirmFn(condition.message);
    }
    default:
      assertNever(condition, "condition kind");
  }
}

// -- Compare evaluation --

function evaluateCompare(cond: CompareCondition, plan: Plan, ctx?: ExecContext): boolean {
  const evalValue = getEvaluateValue();
  const left = evalValue(cond.left, plan, ctx);
  const shapedLeft = applyShape(left, cond.shape);

  // Presence operators — use RAW value for null checks (shape conversion turns
  // null/undefined into defaults like "" or 0 which defeats null detection).
  switch (cond.op) {
    case "is-null":   return left == null;
    case "not-null":  return left != null;
    case "is-empty":  return isEmpty(left);
    case "not-empty": return !isEmpty(left);
    case "truthy":    return !!shapedLeft;
    case "falsy":     return !shapedLeft;
  }

  // Binary operators — need right operand
  const right = cond.right ? evalValue(cond.right, plan, ctx) : undefined;
  // For operators that expect array right operands, apply shape to each item individually
  // instead of applying shape to the whole array (which would stringify it).
  let shapedRight: unknown;
  if (Array.isArray(right) && (cond.op === "in" || cond.op === "not-in" || cond.op === "between")) {
    shapedRight = right.map(item => applyShape(item, cond.shape));
  } else {
    shapedRight = cond.right ? applyShape(right, cond.shape) : undefined;
  }

  log.trace("eval", { op: cond.op, left: shapedLeft, right: shapedRight });

  switch (cond.op) {
    case "eq":  return shapedLeft === shapedRight;
    case "neq": return shapedLeft !== shapedRight;
    case "gt":  return (shapedLeft as number) > (shapedRight as number);
    case "gte": return (shapedLeft as number) >= (shapedRight as number);
    case "lt":  return (shapedLeft as number) < (shapedRight as number);
    case "lte": return (shapedLeft as number) <= (shapedRight as number);

    case "in":
      return Array.isArray(shapedRight) && shapedRight.includes(shapedLeft);
    case "not-in":
      return !Array.isArray(shapedRight) || !shapedRight.includes(shapedLeft);

    case "between":
      return Array.isArray(shapedRight)
        && (shapedLeft as number) >= shapedRight[0]
        && (shapedLeft as number) <= shapedRight[1];

    case "array-contains": {
      const items = cond.itemShape && Array.isArray(shapedLeft)
        ? (shapedLeft as unknown[]).map(item => applyShape(item, cond.itemShape))
        : shapedLeft;
      return Array.isArray(items) && items.includes(shapedRight);
    }

    case "contains": {
      const str = asString(shapedLeft);
      const op = asString(shapedRight);
      return str != null && op != null && str.includes(op);
    }
    case "starts-with": {
      const str = asString(shapedLeft);
      const op = asString(shapedRight);
      return str != null && op != null && str.startsWith(op);
    }
    case "ends-with": {
      const str = asString(shapedLeft);
      const op = asString(shapedRight);
      return str != null && op != null && str.endsWith(op);
    }
    case "matches": {
      const str = asString(shapedLeft);
      const op = asString(shapedRight);
      if (str == null || op == null) return false;
      try {
        return new RegExp(op).test(str);
      } catch {
        log.warn("invalid condition regex", { operand: shapedRight });
        return false;
      }
    }
    case "min-length": {
      const str = asString(shapedLeft);
      return str != null && str.length >= Number(shapedRight);
    }

    default:
      throw new Error(`[alis] Unknown condition operator: ${cond.op}`);
  }
}

function isEmpty(value: unknown): boolean {
  return value === "" || value === null || value === undefined
    || (Array.isArray(value) && value.length === 0);
}

function asString(value: unknown): string | null {
  if (value == null) return null;
  const result = toString(value);
  return result.ok ? result.value : null;
}
