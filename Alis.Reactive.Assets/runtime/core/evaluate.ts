// core/evaluate.ts — Unified value evaluation.
// The ONE way to read a value from any source: component, event, response, literal.
// Every module that needs a value calls evaluateValue(). No parallel paths.

import type { Plan, ValueProducer, ExecContext, Source } from "../types";
import { RuntimePlan } from "../domain/runtime-plan";
import { applyShape } from "./shape-convert";
import { assertNever } from "./assert-never";
import { RuntimeValue, applyShapeWhenPresent } from "../domain/runtime-value";
import { RuntimePath } from "../domain/runtime-path";
import { ExecutionContext } from "../domain/execution-context";
import { RuntimeObject } from "../domain/runtime-object";

type RuntimeObjectReadableSource =
  | Extract<Source, { kind: "component" }>
  | Extract<Source, { kind: "plugin" }>;

type ReadValueProducer = Extract<ValueProducer, { kind: "read" }>;

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

  private evaluateRead(producer: ReadValueProducer): unknown {
    if (RuntimeObjectSource.matches(producer.from)) {
      return this.readFromRuntimeObject(producer);
    }
    if (producer.from.kind === "url") {
      return readFromUrl(producer, this.plan.urlParameters());
    }
    return readFromPayload(producer, this.context.resolvePayload(producer.from));
  }

  private readFromRuntimeObject(producer: ReadValueProducer): unknown {
    const object = this.plan.objectForSource(producer.from);
    const value = this.resolveRuntimeObjectRead(producer, object);
    return value.usingRequestedShape(producer.shape);
  }

  private resolveRuntimeObjectRead(
    producer: ReadValueProducer,
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

/** Read a query parameter from URL source. */
function readFromUrl(
  producer: ReadValueProducer, params: URLSearchParams,
): unknown {
  requirePropertyRead(producer, "URL parameters");
  const raw = params.get(producer.member);
  return applyShapeWhenPresent(raw, producer.shape);
}

/** Read from a payload source through its structured path or explicit whole-body member. */
function readFromPayload(
  producer: ReadValueProducer, root: unknown,
): unknown {
  requirePropertyRead(producer, "payload sources");
  return PayloadRead.from(producer, root).usingRequestedShape();
}

function requirePropertyRead(producer: ReadValueProducer, sourceLabel: string): void {
  const readUsesPropertyAccess = producer.access.kind === "property";
  if (readUsesPropertyAccess) return;

  throw new Error(`[alis] ${sourceLabel} only support property reads, got ${producer.access.kind}`);
}

class PayloadRead {
  private constructor(
    private readonly raw: unknown,
    private readonly shape: ReadValueProducer["shape"],
  ) {}

  static from(producer: ReadValueProducer, root: unknown): PayloadRead {
    const structuredPathWasProvided = producer.path.length > 0;
    if (structuredPathWasProvided) {
      return new PayloadRead(RuntimePath.from(producer.path).read(root), producer.shape);
    }

    const wholeResponseBodyWasRequested = producer.member === "responseBody";
    if (wholeResponseBodyWasRequested) {
      return new PayloadRead(root, producer.shape);
    }

    throw new Error(`[alis] payload read "${producer.member}" requires a structured path`);
  }

  usingRequestedShape(): unknown {
    return applyShapeWhenPresent(this.raw, this.shape);
  }
}

class RuntimeObjectSource {
  static matches(source: Source): source is RuntimeObjectReadableSource {
    switch (source.kind) {
      case "component":
      case "plugin":
        return true;
      case "payload":
      case "url":
        return false;
      default:
        assertNever(source, "runtime object source");
    }
  }
}
