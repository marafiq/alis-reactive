// core/evaluate.ts — Unified value evaluation.
// The ONE way to read a value from any source: component, event, response, literal.
// Every module that needs a value calls evaluateValue(). No parallel paths.

import type { Plan, ValueProducer, ExecContext } from "../types";
import {
  resolveSource, getJsTypeForSource, readProperty as resolverReadProperty,
  callMethod,
} from "../resolution/resolver";
import { applyShape } from "./shape-convert";
import { walk, walkPath } from "./walk";
import { assertNever } from "./assert-never";

export function evaluateValue(producer: ValueProducer, plan: Plan, ctx?: ExecContext): unknown {
  switch (producer.kind) {
    case "literal":
      return applyShape(producer.value, producer.shape);

    case "read":
      return evaluateReadProducer(producer, plan, ctx);

    case "object": {
      const result: Record<string, unknown> = {};
      for (const [key, val] of Object.entries(producer.fields)) {
        result[key] = evaluateValue(val, plan, ctx);
      }
      return result;
    }

    case "array":
      return producer.items.map(i => evaluateValue(i, plan, ctx));

    default:
      assertNever(producer, "value producer kind");
  }
}

/** Evaluate a "read" kind ValueProducer — dispatches by source kind. */
function evaluateReadProducer(
  producer: Extract<ValueProducer, { kind: "read" }>, plan: Plan, ctx?: ExecContext,
): unknown {
  const root = resolveSource(plan, producer.from, ctx);

  if (producer.from.kind === "component" || producer.from.kind === "plugin") {
    return readFromTypedSource(producer, root, plan, ctx);
  }
  if (producer.from.kind === "url") {
    return readFromUrl(producer, root as URLSearchParams);
  }
  return readFromPayload(producer, root);
}

/** Read a member from a component or plugin source using JsType lookup. */
function readFromTypedSource(
  producer: Extract<ValueProducer, { kind: "read" }>, root: unknown, plan: Plan, ctx?: ExecContext,
): unknown {
  const jsType = getJsTypeForSource(plan, producer.from);

  const prop = jsType.properties?.[producer.member];
  if (prop) {
    const raw = resolverReadProperty(root, prop);
    return raw == null ? raw : applyShape(raw, producer.shape ?? prop.shape);
  }

  const method = jsType.methods?.[producer.member];
  if (method) {
    const evaluatedArgs = producer.args ? producer.args.map(a => evaluateValue(a, plan, ctx)) : [];
    const raw = callMethod(root, method, evaluatedArgs);
    return raw == null ? raw : applyShape(raw, producer.shape ?? method.returns);
  }

  const sourceName = producer.from.kind === "component"
    ? producer.from.component
    : (producer.from as import("../types").PluginSource).name;
  throw new Error(`[alis] member "${producer.member}" not found on ${producer.from.kind} "${sourceName}"`);
}

/** Read a query parameter from URL source. */
function readFromUrl(
  producer: Extract<ValueProducer, { kind: "read" }>, params: URLSearchParams,
): unknown {
  const raw = params.get(producer.member);
  return raw == null ? raw : applyShape(raw, producer.shape);
}

/** Read from a payload source — walk member as dot-path or direct walk. */
function readFromPayload(
  producer: Extract<ValueProducer, { kind: "read" }>, root: unknown,
): unknown {
  if (producer.path) {
    const walked = walkPath(root as any, producer.path);
    return walked == null ? walked : applyShape(walked, producer.shape);
  }
  if (producer.member === "responseBody") {
    return root == null ? root : applyShape(root, producer.shape);
  }
  const walked = walk(root as any, producer.member);
  return walked == null ? walked : applyShape(walked, producer.shape);
}
