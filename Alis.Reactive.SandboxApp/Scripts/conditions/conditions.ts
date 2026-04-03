import type { ComparePredicate, ExecContext, PlanPredicate, ValueShape } from "../types";
import { applyShape, evaluateValue } from "../resolution/values";
import { scope } from "../core/trace";
import { toString } from "../core/coerce";

const log = scope("conditions");

export function evaluatePredicate(predicate: PlanPredicate, ctx: ExecContext): boolean {
  switch (predicate.kind) {
    case "compare":
      return evaluateCompare(predicate, ctx);
    case "all":
      return predicate.terms.every(term => evaluatePredicate(term, ctx));
    case "any":
      return predicate.terms.some(term => evaluatePredicate(term, ctx));
    case "not":
      return !evaluatePredicate(predicate.term, ctx);
    case "confirm":
      log.warn("confirm predicate in sync context — denying");
      return false;
    default:
      return false;
  }
}

export async function evaluatePredicateAsync(predicate: PlanPredicate, ctx: ExecContext): Promise<boolean> {
  switch (predicate.kind) {
    case "compare":
      return evaluateCompare(predicate, ctx);
    case "all":
      for (const term of predicate.terms) {
        if (!await evaluatePredicateAsync(term, ctx)) return false;
      }
      return true;
    case "any":
      for (const term of predicate.terms) {
        if (await evaluatePredicateAsync(term, ctx)) return true;
      }
      return false;
    case "not":
      return !(await evaluatePredicateAsync(predicate.term, ctx));
    case "confirm":
      return (window as any).alis?.confirm?.(predicate.message) ?? Promise.resolve(false);
    default:
      return false;
  }
}

function evaluateCompare(predicate: ComparePredicate, ctx: ExecContext): boolean {
  const rawLeft = evaluateValue(predicate.left, ctx);
  const left = coerceWithShape(rawLeft, predicate.as);

  switch (predicate.op) {
    case "is-null":
      return rawLeft == null;
    case "not-null":
      return rawLeft != null;
    case "is-empty":
      return isEmpty(rawLeft);
    case "not-empty":
      return !isEmpty(rawLeft);
    case "truthy":
      return !!left;
    case "falsy":
      return !left;
  }

  const rawRight = predicate.right ? evaluateValue(predicate.right, ctx) : undefined;
  const right = coerceOperand(rawRight, predicate.as, predicate.itemAs);

  switch (predicate.op) {
    case "eq":
      return left === right;
    case "neq":
      return left !== right;
    case "gt":
      return compareNumbers(left, right) > 0;
    case "gte":
      return compareNumbers(left, right) >= 0;
    case "lt":
      return compareNumbers(left, right) < 0;
    case "lte":
      return compareNumbers(left, right) <= 0;
    case "in":
      return Array.isArray(right) && right.includes(left);
    case "not-in":
      return !Array.isArray(right) || !right.includes(left);
    case "between":
      return Array.isArray(right) && right.length >= 2
        && compareNumbers(left, right[0]) >= 0
        && compareNumbers(left, right[1]) <= 0;
    case "array-contains": {
      const items = Array.isArray(rawLeft)
        ? rawLeft.map(item => coerceWithShape(item, predicate.itemAs ?? predicate.as))
        : [];
      const target = coerceWithShape(rawRight, predicate.itemAs ?? predicate.as);
      return items.includes(target);
    }
    case "contains":
      return textValue(left).includes(textValue(right));
    case "starts-with":
      return textValue(left).startsWith(textValue(right));
    case "ends-with":
      return textValue(left).endsWith(textValue(right));
    case "matches":
      try {
        return new RegExp(textValue(right)).test(textValue(left));
      } catch {
        log.warn("invalid predicate regex", { right });
        return false;
      }
    case "min-length":
      return textValue(left).length >= Number(right);
    default:
      return false;
  }
}

function coerceOperand(value: unknown, shape?: ValueShape, itemShape?: ValueShape): unknown {
  if (Array.isArray(value)) {
    return value.map(item => coerceWithShape(item, itemShape ?? shape));
  }
  return coerceWithShape(value, shape);
}

function coerceWithShape(value: unknown, shape?: ValueShape): unknown {
  if (!shape) return value;
  return applyShape(value, shape);
}

function compareNumbers(left: unknown, right: unknown): number {
  const a = Number(left);
  const b = Number(right);
  if (Number.isNaN(a) || Number.isNaN(b)) return Number.NaN;
  return a - b;
}

function isEmpty(value: unknown): boolean {
  return value === "" || value == null || (Array.isArray(value) && value.length === 0);
}

function textValue(value: unknown): string {
  const result = toString(value);
  return result.ok ? result.value : "";
}
