import type { ValueExpression, ArrayOperationExpression, ValidationCondition } from "../types/index";
import { assertNever } from "../shared/assert-never";
import { RuntimeValue } from "../browser-objects/runtime-value";
import { toJavaScriptString } from "../shared/javascript-string";

type EvaluateElementValue = (expression: ValueExpression, item: unknown) => unknown;

type EvaluateElementPredicate = (predicate: ValidationCondition, item: unknown) => boolean;

export function runArrayOp(
  expression: ArrayOperationExpression,
  source: unknown,
  evaluateElementValue: EvaluateElementValue,
  evaluateElementPredicate: EvaluateElementPredicate,
): unknown {
  const items = normalizeToArray(source, expression.op);
  switch (expression.op) {
    case "count":
      // Count(predicate) compiles as filter -> count, so count nodes are unconditional.
      return items.length;
    case "filter":
      return shapedArray(items.filter(item => match(evaluateElementPredicate, expression.predicate, item)), expression);
    case "map":
      return shapedArray(items.map(item => projected(evaluateElementValue, expression.projection, item)), expression);
    case "sum":
      return items.reduce<number>(
        (total, item) => total + toNumber(projectedOrSelf(evaluateElementValue, expression.projection, item)), 0);
    case "any":
      return expression.predicate === undefined
        ? items.length > 0
        : items.some(item => match(evaluateElementPredicate, expression.predicate, item));
    case "all":
      return items.every(item => match(evaluateElementPredicate, expression.predicate, item));
    case "find":
      return findElement(expression, items, evaluateElementValue, evaluateElementPredicate);
    case "orderBy":
      return shapedArray(ordered(items, expression.projection, false, evaluateElementValue), expression);
    case "orderByDescending":
      return shapedArray(ordered(items, expression.projection, true, evaluateElementValue), expression);
    default:
      return assertNever(expression.op, "array-op kind");
  }
}

function findElement(
  expression: ArrayOperationExpression,
  items: unknown[],
  evaluateElementValue: EvaluateElementValue,
  evaluateElementPredicate: EvaluateElementPredicate,
): unknown {
  if (expression.predicate === undefined) {
    throw new Error("[alis] array-op 'find' requires a predicate");
  }
  const found = items.find(item => match(evaluateElementPredicate, expression.predicate, item));
  if (found === undefined) return null;
  return expression.projection === undefined ? found : projected(evaluateElementValue, expression.projection, found);
}

function ordered(
  items: unknown[],
  keyProjection: ArrayOperationExpression["projection"],
  descending: boolean,
  evaluateElementValue: EvaluateElementValue,
): unknown[] {
  if (keyProjection === undefined) throw new Error("[alis] array-op orderBy requires a key projection");
  const keyedItems = items.map(item => ({ item, sortKey: evaluateElementValue(keyProjection, item) }));
  keyedItems.sort((left, right) => compareKeys(left.sortKey, right.sortKey) * (descending ? -1 : 1));
  return keyedItems.map(keyedItem => keyedItem.item);
}

function shapedArray(result: unknown[], expression: ArrayOperationExpression): unknown {
  return RuntimeValue.declared(result, expression.shape).usingDeclaredShape();
}

function match(
  evaluateElementPredicate: EvaluateElementPredicate,
  predicate: ArrayOperationExpression["predicate"],
  item: unknown,
): boolean {
  if (predicate === undefined) {
    throw new Error("[alis] array-op predicate is required for this operation");
  }
  return evaluateElementPredicate(predicate, item);
}

function projected(
  evaluateElementValue: EvaluateElementValue,
  projection: ArrayOperationExpression["projection"],
  item: unknown,
): unknown {
  if (projection === undefined) {
    throw new Error("[alis] array-op projection is required for this operation");
  }
  return evaluateElementValue(projection, item);
}

function projectedOrSelf(
  evaluateElementValue: EvaluateElementValue,
  projection: ArrayOperationExpression["projection"],
  item: unknown,
): unknown {
  return projection === undefined ? item : evaluateElementValue(projection, item);
}

/**
 * Normalize array-op input after runtime-object, DOM, or vendor reads, where C# T[]
 * cannot constrain the JavaScript value. This boundary normalization is not a plan fallback;
 * non-iterable objects fail fast instead of being guessed into arrays.
 */
function normalizeToArray(value: unknown, label: string): unknown[] {
  if (Array.isArray(value)) return value;
  if (value === null || value === undefined) return [];
  if (typeof value === "number" || typeof value === "string") return [value];
  if (typeof value === "object" && Symbol.iterator in (value as object)) {
    return Array.from(value as Iterable<unknown>);
  }
  throw new Error(`[alis] array-op source is not iterable: ${label} (got ${typeof value})`);
}

function toNumber(value: unknown): number {
  const numericValue = Number(value);
  return Number.isFinite(numericValue) ? numericValue : 0;
}

function compareKeys(a: unknown, b: unknown): number {
  if (typeof a === "number" && typeof b === "number") {
    const aFinite = Number.isFinite(a);
    const bFinite = Number.isFinite(b);
    if (aFinite && bFinite) return a - b;
    // Non-finite keys sort last; never feed NaN to Array.sort's comparator.
    if (aFinite === bFinite) return 0;
    return aFinite ? -1 : 1;
  }
  const left = toJavaScriptString(a);
  const right = toJavaScriptString(b);
  return left < right ? -1 : left > right ? 1 : 0;
}
