// ValueExpression reads stay centralized here so execution, gather, validation,
// and condition evaluation share one runtime resolver.

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

export function evaluateValue(
  expression: ValueExpression,
  planDocument: PlanDocument,
  context?: ExecContext,
): unknown {
  return ValueEvaluation.from(planDocument, context).evaluate(expression);
}

class ValueEvaluation {
  private constructor(
    private readonly planDocument: PlanDocument,
    private readonly runtimePlan: RuntimePlan,
    private readonly context: ExecutionContext,
  ) {}

  static from(planDocument: PlanDocument, context?: ExecContext): ValueEvaluation {
    return new ValueEvaluation(planDocument, RuntimePlan.from(planDocument), ExecutionContext.from(context));
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
          (valueExpression, item) => this.inElement(item).evaluate(valueExpression),
          (predicate, item) => this.elementMatches(predicate, item),
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
      return readFromUrl(expression, this.runtimePlan.urlParameters());
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
    const runtimeObject = this.runtimePlan.objectForSource(source);
    const value = this.resolveRuntimeObjectRead(expression, runtimeObject);
    return value.usingRequestedShape(expression.shape);
  }

  private resolveRuntimeObjectRead(
    expression: ObjectReadExpression,
    runtimeObject: RuntimeObject,
  ): RuntimeValue {
    switch (expression.access.kind) {
      case "property":
        return runtimeObject.read(expression.member);

      case "method": {
        const args = expression.access.args.map(arg => this.evaluate(arg));
        return runtimeObject.call(expression.member, args);
      }

      default:
        assertNever(expression.access, "value read access");
    }
  }

  // Element-scope methods use RuntimePath.call so owner binding and non-function
  // errors match component/plugin method reads.
  private readElementMethod(expression: ElementMethodReadExpression): unknown {
    const payloadRoot = this.context.resolvePayload(expression.from);
    const args = expression.access.args.map(arg => this.evaluate(arg));
    const rawValue = RuntimePath.from(expression.path).call(payloadRoot, args, `element method "${expression.member}"`);
    return applyShapeWhenPresent(rawValue, expression.shape);
  }

  private evaluateObject(fields: Record<string, ValueExpression>): Record<string, unknown> {
    const result: Record<string, unknown> = {};
    for (const [key, value] of Object.entries(fields)) {
      result[key] = this.evaluate(value);
    }
    return result;
  }

  /** Array predicates run in the immediate lane; confirm is not legal in this scope. */
  private elementMatches(predicate: ArrayOperationExpression["predicate"], item: unknown): boolean {
    if (predicate === undefined) {
      throw new Error("[alis] array-op predicate is required for this operation");
    }
    return evaluateSyncCondition(predicate, this.planDocument, this.context.withElement(item), evaluateValue);
  }

  private inElement(item: unknown): ValueEvaluation {
    return new ValueEvaluation(this.planDocument, this.runtimePlan, this.context.withElement(item));
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

/** DOM reads cross the DOM boundary by id; they are not component-contract reads. */
function readFromDom(expression: DomPropertyReadExpression): unknown {
  const element = document.getElementById(expression.from.element);
  if (element === null) {
    throw new Error(`[alis] dom source element "${expression.from.element}" not found`);
  }
  const rawValue = RuntimePath.from(expression.path).read(element);
  return applyShapeWhenPresent(rawValue, expression.shape);
}

function readFromUrl(
  expression: UrlParameterReadExpression, params: URLSearchParams,
): unknown {
  const rawValue = params.get(expression.member);
  return applyShapeWhenPresent(rawValue, expression.shape);
}

/** Whole payload and whole element reads return the current payload scope; path reads follow RuntimePath. */
function readFromPayload(
  expression: PayloadReadExpression, payloadRoot: unknown,
): unknown {
  const rawValue = isWholePayloadRead(expression) || isWholeElementRead(expression)
    ? payloadRoot
    : RuntimePath.from(expression.path).read(payloadRoot);

  return applyShapeWhenPresent(rawValue, expression.shape);
}

function isWholePayloadRead(expression: PayloadReadExpression): expression is WholePayloadReadExpression {
  return expression.member === "responseBody";
}

function isWholeElementRead(expression: PayloadReadExpression): expression is WholeElementReadExpression {
  return expression.member === "elementValue";
}
