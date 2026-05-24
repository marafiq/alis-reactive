// url-template.ts — URL template parameter resolution.
// Pure function: resolves {param} placeholders using evaluated ValueProducers.
// Fail-fast: throws on unresolved placeholder, null value, or stringify failure.

import { type Plan, type ValueProducer, type ExecContext } from "../types";
import { evaluateValue } from "./evaluate";
import { formatForWire } from "./wire-format";
import { toString } from "./shape-convert";

const ROUTE_PARAM_RE = /\{(\w+)\}/g;

/** Resolve {param} placeholders in a URL template using evaluated ValueProducers. */
export function resolveRouteParams(
  urlTemplate: string,
  routeParams: Record<string, ValueProducer>,
  plan: Plan,
  ctx: ExecContext,
): string {
  return RouteTemplate
    .from(urlTemplate)
    .bind(RouteParameterBindings.from(routeParams, plan, ctx));
}

class RouteTemplate {
  private constructor(private readonly value: string) {}

  static from(value: string): RouteTemplate {
    return new RouteTemplate(value);
  }

  bind(bindings: RouteParameterBindings): string {
    return this.value.replace(ROUTE_PARAM_RE, (_match, paramName: string) =>
      bindings.require(paramName, this.value).encodedValue());
  }
}

class RouteParameterBindings {
  private constructor(
    private readonly routeParams: Record<string, ValueProducer>,
    private readonly plan: Plan,
    private readonly context: ExecContext,
  ) {}

  static from(
    routeParams: Record<string, ValueProducer>,
    plan: Plan,
    context: ExecContext,
  ): RouteParameterBindings {
    return new RouteParameterBindings(routeParams, plan, context);
  }

  require(paramName: string, urlTemplate: string): RouteParameterBinding {
    const producer = this.routeParams[paramName];
    if (producer === undefined) {
      throw new Error(`[alis] unresolved route param: "${paramName}" in URL template "${urlTemplate}"`);
    }

    return RouteParameterBinding.from(paramName, producer, this.plan, this.context);
  }
}

class RouteParameterBinding {
  private constructor(
    private readonly paramName: string,
    private readonly producer: ValueProducer,
    private readonly plan: Plan,
    private readonly context: ExecContext,
  ) {}

  static from(
    paramName: string,
    producer: ValueProducer,
    plan: Plan,
    context: ExecContext,
  ): RouteParameterBinding {
    return new RouteParameterBinding(paramName, producer, plan, context);
  }

  encodedValue(): string {
    const raw = RouteParameterValue.from(this.paramName, evaluateValue(this.producer, this.plan, this.context));
    const wire = formatForWire(raw.value, this.producer.shape);
    const result = toString(wire);
    if (!result.ok) {
      throw new Error(`[alis] route param "${this.paramName}" could not be stringified: ${result.error}`);
    }

    return encodeURIComponent(result.value);
  }
}

class RouteParameterValue {
  private constructor(readonly value: unknown) {}

  static from(paramName: string, value: unknown): RouteParameterValue {
    const valueIsMissing = value === null || value === undefined;
    if (valueIsMissing) {
      throw new Error(`[alis] route param "${paramName}" evaluated to null — cannot build URL`);
    }

    return new RouteParameterValue(value);
  }
}
