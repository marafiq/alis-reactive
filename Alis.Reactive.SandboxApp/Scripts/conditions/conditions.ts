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
    case "confirm":
      return (window as any).alis?.confirm?.(condition.message) ?? Promise.resolve(false);
    default:
      assertNever(condition, "condition kind");
  }
}

// -- Compare evaluation --

function evaluateCompare(cond: CompareCondition, plan: Plan, ctx?: ExecContext): boolean {
  const evalValue = getEvaluateValue();
  const left = evalValue(cond.left, plan, ctx);
  const coercedLeft = applyShape(left, cond.shape);

  // Presence operators — no right operand needed
  switch (cond.op) {
    case "is-null":   return coercedLeft == null;
    case "not-null":  return coercedLeft != null;
    case "is-empty":  return isEmpty(coercedLeft);
    case "not-empty": return !isEmpty(coercedLeft);
    case "truthy":    return !!coercedLeft;
    case "falsy":     return !coercedLeft;
  }

  // Binary operators — need right operand
  const right = cond.right ? evalValue(cond.right, plan, ctx) : undefined;
  const coercedRight = cond.right ? applyShape(right, cond.shape) : undefined;

  log.trace("eval", { op: cond.op, left: coercedLeft, right: coercedRight });

  switch (cond.op) {
    case "eq":  return coercedLeft === coercedRight;
    case "neq": return coercedLeft !== coercedRight;
    case "gt":  return (coercedLeft as number) > (coercedRight as number);
    case "gte": return (coercedLeft as number) >= (coercedRight as number);
    case "lt":  return (coercedLeft as number) < (coercedRight as number);
    case "lte": return (coercedLeft as number) <= (coercedRight as number);

    case "in":
      return Array.isArray(coercedRight) && coercedRight.includes(coercedLeft);
    case "not-in":
      return !Array.isArray(coercedRight) || !coercedRight.includes(coercedLeft);

    case "between":
      return Array.isArray(coercedRight)
        && (coercedLeft as number) >= coercedRight[0]
        && (coercedLeft as number) <= coercedRight[1];

    case "array-contains": {
      const items = cond.itemShape && Array.isArray(coercedLeft)
        ? (coercedLeft as unknown[]).map(item => applyShape(item, cond.itemShape))
        : coercedLeft;
      return Array.isArray(items) && items.includes(coercedRight);
    }

    case "contains": {
      const str = asString(coercedLeft);
      const op = asString(coercedRight);
      return str != null && op != null && str.includes(op);
    }
    case "starts-with": {
      const str = asString(coercedLeft);
      const op = asString(coercedRight);
      return str != null && op != null && str.startsWith(op);
    }
    case "ends-with": {
      const str = asString(coercedLeft);
      const op = asString(coercedRight);
      return str != null && op != null && str.endsWith(op);
    }
    case "matches": {
      const str = asString(coercedLeft);
      const op = asString(coercedRight);
      if (str == null || op == null) return false;
      try {
        return new RegExp(op).test(str);
      } catch {
        log.warn("invalid condition regex", { operand: coercedRight });
        return false;
      }
    }
    case "min-length": {
      const str = asString(coercedLeft);
      return str != null && str.length >= Number(coercedRight);
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
