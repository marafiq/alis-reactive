// values/evaluate.ts - Unified value evaluation.
// The ONE way to read a value from any source: component, event, response, literal.
// Every module that needs a value calls evaluateValue(). No parallel paths.

import type {
  PlanDocument, ValueExpression, ExecContext, ReadExpression, RuntimeObjectSource,
  ObjectPropertyReadExpression, ObjectMethodReadExpression,
  UrlParameterReadExpression, PayloadPathReadExpression, WholePayloadReadExpression,
  WholeElementReadExpression, ArrayOperationExpression, DomPropertyReadExpression,
  ElementMethodReadExpression,
} from "../types/index";
import { RuntimePlan } from "../browser-objects/runtime-plan";
import { applyShape } from "../shared/shape-convert";
import { assertNever } from "../shared/assert-never";
import { RuntimeValue, applyShapeWhenPresent } from "../browser-objects/runtime-value";
import { RuntimePath } from "../browser-objects/runtime-path";
import { ExecutionContext } from "../browser-objects/execution-context";
import { RuntimeObject } from "../browser-objects/runtime-object";
import { evaluateSyncCondition } from "../conditions/compare-engine";
import { runArrayOp } from "./array-op-engine";

type ObjectReadExpression = ObjectPropertyReadExpression | ObjectMethodReadExpression;
type PayloadReadExpression = PayloadPathReadExpression | WholePayloadReadExpression | WholeElementReadExpression | ElementMethodReadExpression;

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
        return runArrayOp(
          expression,
          this.evaluate(expression.source),
          (e, item) => this.inElement(item).evaluate(e),
          (p, item) => this.elementMatches(p, item),
        );

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
      if (isElementMethodRead(expression)) {
        return this.readElementMethod(expression);
      }
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

  /**
   * Invoke a method on the current element (element scope) — the same RuntimePath.call engine
   * (fn.apply(owner, args)) that component/plugin method reads use. Args are full value
   * expressions evaluated in the current element context. A non-function member is a true
   * external-boundary error, surfaced by RuntimePath.call.
   */
  private readElementMethod(expression: ElementMethodReadExpression): unknown {
    const root = this.context.resolvePayload(expression.from);
    const args = expression.access.args.map(arg => this.evaluate(arg));
    const raw = RuntimePath.from(expression.path).call(root, args, `element method "${expression.member}"`);
    return applyShapeWhenPresent(raw, expression.shape);
  }

  private evaluateObject(fields: Record<string, ValueExpression>): Record<string, unknown> {
    const result: Record<string, unknown> = {};
    for (const [key, value] of Object.entries(fields)) {
      result[key] = this.evaluate(value);
    }
    return result;
  }

  /** Evaluate a per-element predicate against the element scope (immediate lane, sync subset). */
  private elementMatches(predicate: ArrayOperationExpression["predicate"], item: unknown): boolean {
    if (predicate === undefined) {
      throw new Error("[alis] array-op predicate is required for this operation");
    }
    return evaluateSyncCondition(predicate, this.document, this.context.withElement(item), evaluateValue);
  }

  private inElement(item: unknown): ValueEvaluation {
    return new ValueEvaluation(this.document, this.plan, this.context.withElement(item));
  }
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

function isElementMethodRead(expression: PayloadReadExpression): expression is ElementMethodReadExpression {
  return expression.access.kind === "method";
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
