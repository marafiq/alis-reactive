// core/evaluate.ts — Unified value evaluation.
// The ONE way to read a value from any source: component, event, response, literal.
// Every module that needs a value calls evaluateValue(). No parallel paths.

import type {
  PlanDocument, ValueExpression, ExecContext, ReadExpression, RuntimeObjectSource,
  ObjectPropertyReadExpression, ObjectMethodReadExpression,
  UrlParameterReadExpression, PayloadPathReadExpression, WholePayloadReadExpression,
  WholeElementReadExpression,
} from "../types";
import { RuntimePlan } from "../domain/runtime-plan";
import { applyShape } from "./shape-convert";
import { assertNever } from "./assert-never";
import { RuntimeValue, applyShapeWhenPresent } from "../domain/runtime-value";
import { RuntimePath } from "../domain/runtime-path";
import { ExecutionContext } from "../domain/execution-context";
import { RuntimeObject } from "../domain/runtime-object";

type ObjectReadExpression = ObjectPropertyReadExpression | ObjectMethodReadExpression;
type PayloadReadExpression = PayloadPathReadExpression | WholePayloadReadExpression | WholeElementReadExpression;

export function evaluateValue(expression: ValueExpression, plan: PlanDocument, ctx?: ExecContext): unknown {
  return ValueEvaluation.from(plan, ctx).evaluate(expression);
}

class ValueEvaluation {
  private constructor(
    private readonly plan: RuntimePlan,
    private readonly context: ExecutionContext,
  ) {}

  static from(plan: PlanDocument, ctx?: ExecContext): ValueEvaluation {
    return new ValueEvaluation(RuntimePlan.from(plan), ExecutionContext.from(ctx));
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
