// url-template.ts — URL template parameter resolution.
// Pure function: resolves {param} placeholders using evaluated ValueProducers.
// Throws when a dynamic route value is missing or cannot become URL text.

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
  return urlTemplate.replace(ROUTE_PARAM_RE, (_match, paramName: string) =>
    encodeRouteParameter(paramName, routeParams[paramName]!, plan, ctx));
}

function encodeRouteParameter(
  paramName: string,
  producer: ValueProducer,
  plan: Plan,
  ctx: ExecContext,
): string {
  const raw = evaluateValue(producer, plan, ctx);
  const valueIsMissing = raw === null || raw === undefined;
  if (valueIsMissing) {
    throw new Error(`[alis] route param "${paramName}" evaluated to null; cannot build URL`);
  }

  const wire = formatForWire(raw, producer.shape);
  const result = toString(wire);
  if (!result.ok) {
    throw new Error(`[alis] route param "${paramName}" could not be stringified: ${result.error}`);
  }

  return encodeURIComponent(result.value);
}
