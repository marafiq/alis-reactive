// value/array-op-engine.ts — The eight array ops, extracted from core/evaluate.ts.
// Pure, sync, leaf-clean: the per-element predicate (sync condition) and projection
// (value-in-element-scope) are PASSED IN as callbacks, so this engine imports no
// Condition/Request/Reaction. evaluate.ts owns the element scope and hands it back through
// `project` and `elementMatches`. Behavior is identical to the inline evaluateArrayOp it replaces.

import type { ValueExpression, ArrayOperationExpression, ValidationCondition } from "../types";
import { assertNever } from "../core/assert-never";
import { RuntimeValue } from "../domain/runtime-value";

/** Projection callback: evaluate a value expression against a single element's scope. */
type Project = (expression: ValueExpression, item: unknown) => unknown;

/** Predicate callback: evaluate a sync condition against a single element's scope. */
type ElementMatches = (predicate: ValidationCondition, item: unknown) => boolean;

/**
 * Run an array operation over an already-evaluated source array. The eight ops
 * (count·filter·map·sum·any·all·find·orderBy/orderByDescending) read verbatim from the
 * inline evaluateArrayOp they replace — same semantics, error messages, and edge behavior.
 */
export function runArrayOp(
  expression: ArrayOperationExpression,
  source: unknown,
  project: Project,
  elementMatches: ElementMatches,
): unknown {
  const items = normalizeToArray(source, expression.op);
  switch (expression.op) {
    case "count":
      // Count is unconditional length; Count(predicate) compiles to filter -> count (ReactiveArray.cs),
      // so the count node never carries a predicate.
      return items.length;
    case "filter":
      return shapedArray(items.filter(item => match(elementMatches, expression.predicate, item)), expression);
    case "map":
      return shapedArray(items.map(item => projected(project, expression.projection, item)), expression);
    case "sum":
      return items.reduce<number>(
        (total, item) => total + toNumber(projectedOrSelf(project, expression.projection, item)), 0);
    case "any":
      return expression.predicate === undefined
        ? items.length > 0
        : items.some(item => match(elementMatches, expression.predicate, item));
    case "all":
      return items.every(item => match(elementMatches, expression.predicate, item));
    case "find":
      return findElement(expression, items, project, elementMatches);
    case "orderBy":
      return shapedArray(ordered(items, expression.projection, false, project), expression);
    case "orderByDescending":
      return shapedArray(ordered(items, expression.projection, true, project), expression);
    default:
      return assertNever(expression.op, "array-op kind");
  }
}

function findElement(
  expression: ArrayOperationExpression,
  items: unknown[],
  project: Project,
  elementMatches: ElementMatches,
): unknown {
  if (expression.predicate === undefined) {
    throw new Error("[alis] array-op 'find' requires a predicate");
  }
  const found = items.find(item => match(elementMatches, expression.predicate, item));
  if (found === undefined) return null;
  return expression.projection === undefined ? found : projected(project, expression.projection, found);
}

function ordered(
  items: unknown[],
  key: ArrayOperationExpression["projection"],
  descending: boolean,
  project: Project,
): unknown[] {
  if (key === undefined) throw new Error("[alis] array-op orderBy requires a key projection");
  const decorated = items.map(item => ({ item, sortKey: project(key, item) }));
  decorated.sort((a, b) => compareKeys(a.sortKey, b.sortKey) * (descending ? -1 : 1));
  return decorated.map(entry => entry.item);
}

function shapedArray(result: unknown[], expression: ArrayOperationExpression): unknown {
  return RuntimeValue.declared(result, expression.shape).usingDeclaredShape();
}

/** Evaluate a per-element predicate against the element scope (immediate lane, sync subset). */
function match(elementMatches: ElementMatches, predicate: ArrayOperationExpression["predicate"], item: unknown): boolean {
  if (predicate === undefined) {
    throw new Error("[alis] array-op predicate is required for this operation");
  }
  return elementMatches(predicate, item);
}

function projected(project: Project, projection: ArrayOperationExpression["projection"], item: unknown): unknown {
  if (projection === undefined) {
    throw new Error("[alis] array-op projection is required for this operation");
  }
  return project(projection, item);
}

function projectedOrSelf(project: Project, projection: ArrayOperationExpression["projection"], item: unknown): unknown {
  return projection === undefined ? item : project(projection, item);
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

/** Coerce a projected value to a number for sum; non-finite contributes 0. */
function toNumber(value: unknown): number {
  const n = Number(value);
  return Number.isFinite(n) ? n : 0;
}

/** Deterministic ordering of sort keys: numeric when both numbers, else lexicographic. */
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
