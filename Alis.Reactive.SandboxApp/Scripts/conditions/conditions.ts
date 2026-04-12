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
    case "none":
      return true;
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
    case "none":
      return true;
    default:
      assertNever(condition, "condition kind");
  }
}

// -- Compare evaluation --

function evaluateCompare(cond: CompareCondition, plan: Plan, ctx?: ExecContext): boolean {
  const evalValue = getEvaluateValue();
  const left = evalValue(cond.left, plan, ctx);
  const shapedLeft = applyShape(left, cond.shape);

  const unary = evaluateUnaryOp(cond.op, left, shapedLeft);
  if (unary !== undefined) return unary;

  const shapedRight = resolveRight(cond, evalValue, plan, ctx);
  log.trace("eval", { op: cond.op, left: shapedLeft, right: shapedRight });

  return evaluateBinaryOp(cond, shapedLeft, shapedRight);
}

/**
 * Presence operators — use RAW value for null checks (shape conversion turns
 * null/undefined into defaults like "" or 0 which defeats null detection).
 * Returns undefined if the operator is not unary.
 */
function evaluateUnaryOp(op: string, rawLeft: unknown, shapedLeft: unknown): boolean | undefined {
  switch (op) {
    case "is-null":   return rawLeft == null;
    case "not-null":  return rawLeft != null;
    case "is-empty":  return isEmpty(rawLeft);
    case "not-empty": return !isEmpty(rawLeft);
    case "truthy":    return !!shapedLeft;
    case "falsy":     return !shapedLeft;
    default:          return undefined;
  }
}

/** Resolve and shape the right operand for binary comparison. */
function resolveRight(
  cond: CompareCondition,
  evalValue: (p: ValueProducer, plan: Plan, ctx?: ExecContext) => unknown,
  plan: Plan, ctx?: ExecContext,
): unknown {
  const right = cond.right ? evalValue(cond.right, plan, ctx) : undefined;
  if (Array.isArray(right) && (cond.op === "in" || cond.op === "not-in" || cond.op === "between")) {
    return right.map(item => applyShape(item, cond.shape));
  }
  return cond.right ? applyShape(right, cond.shape) : undefined;
}

/** Evaluate binary operators that require both left and right operands. */
function evaluateBinaryOp(cond: CompareCondition, shapedLeft: unknown, shapedRight: unknown): boolean {
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

    case "array-contains":
      return evaluateArrayContains(cond, shapedLeft, shapedRight);

    case "contains":    return stringOp(shapedLeft, shapedRight, (s, o) => s.includes(o));
    case "starts-with": return stringOp(shapedLeft, shapedRight, (s, o) => s.startsWith(o));
    case "ends-with":   return stringOp(shapedLeft, shapedRight, (s, o) => s.endsWith(o));
    case "matches":     return matchesRegex(shapedLeft, shapedRight);
    case "min-length":  return evalMinLength(shapedLeft, shapedRight);

    default:
      throw new Error(`[alis] Unknown condition operator: ${cond.op}`);
  }
}

function evaluateArrayContains(cond: CompareCondition, shapedLeft: unknown, shapedRight: unknown): boolean {
  const items = cond.itemShape && Array.isArray(shapedLeft)
    ? (shapedLeft as unknown[]).map(item => applyShape(item, cond.itemShape))
    : shapedLeft;
  return Array.isArray(items) && items.includes(shapedRight);
}

function stringOp(left: unknown, right: unknown, fn: (s: string, o: string) => boolean): boolean {
  const str = asString(left);
  const op = asString(right);
  return str != null && op != null && fn(str, op);
}

function matchesRegex(left: unknown, right: unknown): boolean {
  const str = asString(left);
  const op = asString(right);
  if (str == null || op == null) return false;
  try {
    return new RegExp(op).test(str);
  } catch {
    log.warn("invalid condition regex", { operand: right });
    return false;
  }
}

function evalMinLength(left: unknown, right: unknown): boolean {
  const str = asString(left);
  return str != null && str.length >= Number(right);
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
