import type { ValueExpression, ArrayOperationExpression, ValidationCondition } from "../types/index";
import { assertNever } from "../shared/assert-never";
import { RuntimeValue } from "../browser-objects/runtime-value";

type ProjectElementValue = (expression: ValueExpression, item: unknown) => unknown;

type MatchElementPredicate = (predicate: ValidationCondition, item: unknown) => boolean;

/**
 * Executes array operations after evaluate.ts supplies element-scope value and
 * predicate readers.
 */
export function runArrayOp(
  expression: ArrayOperationExpression,
  source: unknown,
  projectElementValue: ProjectElementValue,
  matchElementPredicate: MatchElementPredicate,
): unknown {
  const items = normalizeToArray(source, expression.op);
  switch (expression.op) {
    case "count":
      // Count is unconditional length; Count(predicate) compiles to filter -> count (ReactiveArray.cs),
      // so the count node never carries a predicate.
      return items.length;
    case "filter":
      return shapedArray(items.filter(item => match(matchElementPredicate, expression.predicate, item)), expression);
    case "map":
      return shapedArray(items.map(item => projected(projectElementValue, expression.projection, item)), expression);
    case "sum":
      return items.reduce<number>(
        (total, item) => total + toNumber(projectedOrSelf(projectElementValue, expression.projection, item)), 0);
    case "any":
      return expression.predicate === undefined
        ? items.length > 0
        : items.some(item => match(matchElementPredicate, expression.predicate, item));
    case "all":
      return items.every(item => match(matchElementPredicate, expression.predicate, item));
    case "find":
      return findElement(expression, items, projectElementValue, matchElementPredicate);
    case "orderBy":
      return shapedArray(ordered(items, expression.projection, false, projectElementValue), expression);
    case "orderByDescending":
      return shapedArray(ordered(items, expression.projection, true, projectElementValue), expression);
    default:
      return assertNever(expression.op, "array-op kind");
  }
}

function findElement(
  expression: ArrayOperationExpression,
  items: unknown[],
  projectElementValue: ProjectElementValue,
  matchElementPredicate: MatchElementPredicate,
): unknown {
  if (expression.predicate === undefined) {
    throw new Error("[alis] array-op 'find' requires a predicate");
  }
  const found = items.find(item => match(matchElementPredicate, expression.predicate, item));
  if (found === undefined) return null;
  return expression.projection === undefined ? found : projected(projectElementValue, expression.projection, found);
}

function ordered(
  items: unknown[],
  key: ArrayOperationExpression["projection"],
  descending: boolean,
  projectElementValue: ProjectElementValue,
): unknown[] {
  if (key === undefined) throw new Error("[alis] array-op orderBy requires a key projection");
  const decorated = items.map(item => ({ item, sortKey: projectElementValue(key, item) }));
  decorated.sort((a, b) => compareKeys(a.sortKey, b.sortKey) * (descending ? -1 : 1));
  return decorated.map(entry => entry.item);
}

function shapedArray(result: unknown[], expression: ArrayOperationExpression): unknown {
  return RuntimeValue.declared(result, expression.shape).usingDeclaredShape();
}

function match(
  matchElementPredicate: MatchElementPredicate,
  predicate: ArrayOperationExpression["predicate"],
  item: unknown,
): boolean {
  if (predicate === undefined) {
    throw new Error("[alis] array-op predicate is required for this operation");
  }
  return matchElementPredicate(predicate, item);
}

function projected(
  projectElementValue: ProjectElementValue,
  projection: ArrayOperationExpression["projection"],
  item: unknown,
): unknown {
  if (projection === undefined) {
    throw new Error("[alis] array-op projection is required for this operation");
  }
  return projectElementValue(projection, item);
}

function projectedOrSelf(
  projectElementValue: ProjectElementValue,
  projection: ArrayOperationExpression["projection"],
  item: unknown,
): unknown {
  return projection === undefined ? item : projectElementValue(projection, item);
}

/**
 * Normalize an array-op source to a JS array at the input boundary.
 * Browser/EJ2 JS APIs return an underdetermined union (Array | array-like | iterable
 * | scalar | null) that the C# T[] type cannot constrain at authoring time. This is an
 * external-boundary normalization — the same category as getElementById returning null —
 * not a plan validator or fallback. DOMStringMap (dataset) has no Symbol.iterator and
 * fails fast at the throw, keeping it in the plugin escape hatch's domain.
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
  const n = Number(value);
  return Number.isFinite(n) ? n : 0;
}

function compareKeys(a: unknown, b: unknown): number {
  if (typeof a === "number" && typeof b === "number") {
    const aFinite = Number.isFinite(a);
    const bFinite = Number.isFinite(b);
    if (aFinite && bFinite) return a - b;
    // Non-finite keys (NaN/Infinity from a missing or non-numeric field) sort last,
    // deterministically — never feed NaN to Array.sort (engine-defined behavior).
    if (aFinite === bFinite) return 0;
    return aFinite ? -1 : 1;
  }
  const left = String(a);
  const right = String(b);
  return left < right ? -1 : left > right ? 1 : 0;
}
