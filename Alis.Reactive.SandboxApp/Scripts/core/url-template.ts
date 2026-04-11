// url-template.ts — URL template parameter resolution.
// Pure function: resolves {param} placeholders using evaluated ValueProducers.
// Fail-fast: throws on unresolved placeholder, null value, or stringify failure.

import type { Plan, ValueProducer, ExecContext } from "../types";
import { evaluateValue } from "./evaluate";
import { formatForWire } from "./wire-format";
import { toString } from "./shape-convert";

const ROUTE_PARAM_RE = /\{(\w+)\}/g;

/** Resolve {param} placeholders in a URL template using evaluated ValueProducers. */
export function resolveRouteParams(
  urlTemplate: string,
  routeParams: Record<string, ValueProducer>,
  plan: Plan,
  ctx?: ExecContext,
): string {
  return urlTemplate.replace(ROUTE_PARAM_RE, (_match, paramName: string) => {
    const producer = routeParams[paramName];
    if (!producer) {
      // Build-time validation ensures every RouteParam matches a placeholder.
      // If we get here, the plan JSON is malformed — fail fast.
      throw new Error(`[alis] unresolved route param: "${paramName}" in URL template "${urlTemplate}"`);
    }
    const raw = evaluateValue(producer, plan, ctx);
    if (raw == null) {
      // Null means a component hasn't been selected or an event arg is missing.
      // An empty path segment corrupts the URL silently — fail fast instead.
      throw new Error(`[alis] route param "${paramName}" evaluated to null — cannot build URL`);
    }
    const wire = formatForWire(raw, producer.shape);
    const result = toString(wire);
    if (!result.ok) {
      throw new Error(`[alis] route param "${paramName}" could not be stringified: ${result.error}`);
    }
    return encodeURIComponent(result.value);
  });
}
