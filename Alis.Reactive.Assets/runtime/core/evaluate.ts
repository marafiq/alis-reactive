// core/evaluate.ts — Unified value evaluation.
// The ONE way to read a value from any source: component, event, response, literal.
// Every module that needs a value calls evaluateValue(). No parallel paths.

import type {
  Plan, ValueProducer, ExecContext, ReadProducer, RuntimeObjectSource,
  ObjectPropertyReadProducer, ObjectMethodReadProducer,
  UrlParameterReadProducer, PayloadPathReadProducer, WholePayloadReadProducer,
} from "../types";
import { RuntimePlan } from "../domain/runtime-plan";
import { applyShape } from "./shape-convert";
import { assertNever } from "./assert-never";
import { RuntimeValue, applyShapeWhenPresent } from "../domain/runtime-value";
import { RuntimePath } from "../domain/runtime-path";
import { ExecutionContext } from "../domain/execution-context";
import { RuntimeObject } from "../domain/runtime-object";

type ObjectReadProducer = ObjectPropertyReadProducer | ObjectMethodReadProducer;
type PayloadReadProducer = PayloadPathReadProducer | WholePayloadReadProducer;

export function evaluateValue(producer: ValueProducer, plan: Plan, ctx?: ExecContext): unknown {
  return ValueEvaluation.from(plan, ctx).evaluate(producer);
}

class ValueEvaluation {
  private constructor(
    private readonly plan: RuntimePlan,
    private readonly context: ExecutionContext,
  ) {}

  static from(plan: Plan, ctx?: ExecContext): ValueEvaluation {
    return new ValueEvaluation(RuntimePlan.from(plan), ExecutionContext.from(ctx));
  }

  evaluate(producer: ValueProducer): unknown {
    switch (producer.kind) {
      case "literal":
        return applyShape(producer.value, producer.shape);

      case "read":
        return this.evaluateRead(producer);

      case "object":
        return RuntimeValue
          .declared(this.evaluateObject(producer.fields), producer.shape)
          .usingDeclaredShape();

      case "array":
        return RuntimeValue
          .declared(producer.items.map(item => this.evaluate(item)), producer.shape)
          .usingDeclaredShape();

      default:
        assertNever(producer, "value producer kind");
    }
  }

  private evaluateRead(producer: ReadProducer): unknown {
    if (isObjectRead(producer)) {
      return this.readFromRuntimeObject(producer, producer.from);
    }
    if (isUrlRead(producer)) {
      return readFromUrl(producer, this.plan.urlParameters());
    }
    if (isPayloadRead(producer)) {
      return readFromPayload(producer, this.context.resolvePayload(producer.from));
    }

    return assertNever(producer, "read producer");
  }

  private readFromRuntimeObject(producer: ObjectReadProducer, source: RuntimeObjectSource): unknown {
    const object = this.plan.objectForSource(source);
    const value = this.resolveRuntimeObjectRead(producer, object);
    return value.usingRequestedShape(producer.shape);
  }

  private resolveRuntimeObjectRead(
    producer: ObjectReadProducer,
    object: RuntimeObject,
  ): RuntimeValue {
    switch (producer.access.kind) {
      case "property":
        return object.read(producer.member);

      case "method": {
        const args = producer.access.args.map(arg => this.evaluate(arg));
        return object.call(producer.member, args);
      }

      default:
        assertNever(producer.access, "value read access");
    }
  }

  private evaluateObject(fields: Record<string, ValueProducer>): Record<string, unknown> {
    const result: Record<string, unknown> = {};
    for (const [key, value] of Object.entries(fields)) {
      result[key] = this.evaluate(value);
    }
    return result;
  }
}

function isObjectRead(producer: ReadProducer): producer is ObjectReadProducer {
  return producer.from.kind === "component" || producer.from.kind === "plugin";
}

function isUrlRead(producer: ReadProducer): producer is UrlParameterReadProducer {
  return producer.from.kind === "url";
}

function isPayloadRead(producer: ReadProducer): producer is PayloadReadProducer {
  return producer.from.kind === "payload";
}

/** Read a query parameter from URL source. */
function readFromUrl(
  producer: UrlParameterReadProducer, params: URLSearchParams,
): unknown {
  const raw = params.get(producer.member);
  return applyShapeWhenPresent(raw, producer.shape);
}

/** Read from a payload source through its structured path or explicit whole-body member. */
function readFromPayload(
  producer: PayloadReadProducer, root: unknown,
): unknown {
  const raw = readsWholePayload(producer)
    ? root
    : RuntimePath.from(producer.path).read(root);

  return applyShapeWhenPresent(raw, producer.shape);
}

function readsWholePayload(producer: PayloadReadProducer): producer is WholePayloadReadProducer {
  return producer.member === "responseBody";
}
