// core/evaluate.ts — Unified value evaluation.
// The ONE way to read a value from any source: component, event, response, literal.
// Every module that needs a value calls evaluateValue(). No parallel paths.

import type {
  PlanDocument, ValueExpression, ExecContext, ReadExpression, RuntimeObjectSource,
  ObjectPropertyReadExpression, ObjectMethodReadExpression,
  UrlParameterReadExpression, PayloadPathReadExpression, WholePayloadReadExpression,
  WholeElementReadExpression, ArrayOperationExpression, DomPropertyReadExpression,
} from "../types";
import { RuntimePlan } from "../domain/runtime-plan";
import { applyShape } from "./shape-convert";
import { assertNever } from "./assert-never";
import { RuntimeValue, applyShapeWhenPresent } from "../domain/runtime-value";
import { RuntimePath } from "../domain/runtime-path";
import { ExecutionContext } from "../domain/execution-context";
import { RuntimeObject } from "../domain/runtime-object";
import { evaluateSyncCondition } from "../conditions/sync-condition";

type ObjectReadExpression = ObjectPropertyReadExpression | ObjectMethodReadExpression;
type PayloadReadExpression = PayloadPathReadExpression | WholePayloadReadExpression | WholeElementReadExpression;

export function evaluateValue(expression: ValueExpression, plan: PlanDocument, ctx?: ExecContext): unknown {
  return ValueEvaluation.from(plan, ctx).evaluate(expression);
}

class ValueEvaluation {
  private constructor(
    private readonly document: PlanDocument,
    private readonly plan: RuntimePlan,
    private readonly context: ExecutionContext,
  ) {}

  static from(plan: PlanDocument, ctx?: ExecContext): ValueEvaluation {
    return new ValueEvaluation(plan, RuntimePlan.from(plan), ExecutionContext.from(ctx));
  }

  evaluate(expression: ValueExpression): unknown {
    switch (expression.kind) {
      case "literal":
        return applyShape(expression.value, expression.shape);

      case "read":
        return this.evaluateRead(expression);

      case "object":
        return RuntimeValue
          .declared(this.evaluateObject(expression.fields), expression.shape)
          .usingDeclaredShape();

      case "array":
        return RuntimeValue
          .declared(expression.items.map(item => this.evaluate(item)), expression.shape)
          .usingDeclaredShape();

      case "array-op":
        return this.evaluateArrayOp(expression);

      default:
        assertNever(expression, "value expression kind");
    }
  }

  private evaluateRead(expression: ReadExpression): unknown {
    if (isObjectRead(expression)) {
      return this.readFromRuntimeObject(expression, expression.from);
    }
    if (isUrlRead(expression)) {
      return readFromUrl(expression, this.plan.urlParameters());
    }
    if (isPayloadRead(expression)) {
      return readFromPayload(expression, this.context.resolvePayload(expression.from));
    }
    if (isDomRead(expression)) {
      return readFromDom(expression);
    }

    return assertNever(expression, "read expression");
  }

  private readFromRuntimeObject(expression: ObjectReadExpression, source: RuntimeObjectSource): unknown {
    const object = this.plan.objectForSource(source);
    const value = this.resolveRuntimeObjectRead(expression, object);
    return value.usingRequestedShape(expression.shape);
  }

  private resolveRuntimeObjectRead(
    expression: ObjectReadExpression,
    object: RuntimeObject,
  ): RuntimeValue {
    switch (expression.access.kind) {
      case "property":
        return object.read(expression.member);

      case "method": {
        const args = expression.access.args.map(arg => this.evaluate(arg));
        return object.call(expression.member, args);
      }

      default:
        assertNever(expression.access, "value read access");
    }
  }

  private evaluateObject(fields: Record<string, ValueExpression>): Record<string, unknown> {
    const result: Record<string, unknown> = {};
    for (const [key, value] of Object.entries(fields)) {
      result[key] = this.evaluate(value);
    }
    return result;
  }

  private evaluateArrayOp(expression: ArrayOperationExpression): unknown {
    const items = normalizeToArray(this.evaluate(expression.source), expression.op);
    switch (expression.op) {
      case "count":
        return expression.predicate === undefined
          ? items.length
          : items.filter(item => this.elementMatches(expression.predicate, item)).length;
      case "filter":
        return this.shapedArray(items.filter(item => this.elementMatches(expression.predicate, item)), expression);
      case "map":
        return this.shapedArray(items.map(item => this.project(expression.projection, item)), expression);
      case "sum":
        return items.reduce<number>(
          (total, item) => total + toNumber(this.projectedOrSelf(expression.projection, item)), 0);
      case "any":
        return expression.predicate === undefined
          ? items.length > 0
          : items.some(item => this.elementMatches(expression.predicate, item));
      case "all":
        return items.every(item => this.elementMatches(expression.predicate, item));
      case "find":
        return this.findElement(expression, items);
      case "orderBy":
        return this.shapedArray(this.ordered(items, expression.projection, false), expression);
      case "orderByDescending":
        return this.shapedArray(this.ordered(items, expression.projection, true), expression);
      default:
        return assertNever(expression.op, "array-op kind");
    }
  }

  private findElement(expression: ArrayOperationExpression, items: unknown[]): unknown {
    const found = expression.predicate === undefined
      ? items[0]
      : items.find(item => this.elementMatches(expression.predicate, item));
    if (found === undefined) return null;
    return expression.projection === undefined ? found : this.project(expression.projection, found);
  }

  private ordered(items: unknown[], key: ArrayOperationExpression["projection"], descending: boolean): unknown[] {
    if (key === undefined) throw new Error("[alis] array-op orderBy requires a key projection");
    const decorated = items.map(item => ({ item, sortKey: this.inElement(item).evaluate(key) }));
    decorated.sort((a, b) => compareKeys(a.sortKey, b.sortKey) * (descending ? -1 : 1));
    return decorated.map(entry => entry.item);
  }

  private shapedArray(result: unknown[], expression: ArrayOperationExpression): unknown {
    return RuntimeValue.declared(result, expression.shape).usingDeclaredShape();
  }

  /** Evaluate a per-element predicate against the element scope (immediate lane, sync subset). */
  private elementMatches(predicate: ArrayOperationExpression["predicate"], item: unknown): boolean {
    if (predicate === undefined) {
      throw new Error("[alis] array-op predicate is required for this operation");
    }
    return evaluateSyncCondition(predicate, this.document, this.context.withElement(item), evaluateValue);
  }

  private project(projection: ArrayOperationExpression["projection"], item: unknown): unknown {
    if (projection === undefined) {
      throw new Error("[alis] array-op projection is required for this operation");
    }
    return this.inElement(item).evaluate(projection);
  }

  private projectedOrSelf(projection: ArrayOperationExpression["projection"], item: unknown): unknown {
    return projection === undefined ? item : this.inElement(item).evaluate(projection);
  }

  private inElement(item: unknown): ValueEvaluation {
    return new ValueEvaluation(this.document, this.plan, this.context.withElement(item));
  }
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
  if (typeof a === "number" && typeof b === "number") return a - b;
  const left = String(a);
  const right = String(b);
  return left < right ? -1 : left > right ? 1 : 0;
}

function isObjectRead(expression: ReadExpression): expression is ObjectReadExpression {
  return expression.from.kind === "component" || expression.from.kind === "plugin";
}

function isUrlRead(expression: ReadExpression): expression is UrlParameterReadExpression {
  return expression.from.kind === "url";
}

function isPayloadRead(expression: ReadExpression): expression is PayloadReadExpression {
  return expression.from.kind === "payload";
}

function isDomRead(expression: ReadExpression): expression is DomPropertyReadExpression {
  return expression.from.kind === "dom";
}

/** Read a member off a DOM element resolved by id — same RuntimePath primitive, no contract. */
function readFromDom(expression: DomPropertyReadExpression): unknown {
  const element = document.getElementById(expression.from.element);
  if (element === null) {
    throw new Error(`[alis] dom source element "${expression.from.element}" not found`);
  }
  const raw = RuntimePath.from(expression.path).read(element);
  return applyShapeWhenPresent(raw, expression.shape);
}

/** Read a query parameter from URL source. */
function readFromUrl(
  expression: UrlParameterReadExpression, params: URLSearchParams,
): unknown {
  const raw = params.get(expression.member);
  return applyShapeWhenPresent(raw, expression.shape);
}

/** Read from a payload source through its structured path or explicit whole-body member. */
function readFromPayload(
  expression: PayloadReadExpression, root: unknown,
): unknown {
  const raw = readsWholePayload(expression) || readsWholeElement(expression)
    ? root
    : RuntimePath.from(expression.path).read(root);

  return applyShapeWhenPresent(raw, expression.shape);
}

function readsWholePayload(expression: PayloadReadExpression): expression is WholePayloadReadExpression {
  return expression.member === "responseBody";
}

function readsWholeElement(expression: PayloadReadExpression): expression is WholeElementReadExpression {
  return expression.member === "elementValue";
}
