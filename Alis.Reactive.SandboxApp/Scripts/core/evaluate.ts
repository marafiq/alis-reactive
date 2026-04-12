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

    case "read": {
      const root = resolveSource(plan, producer.from, ctx);

      // Component or Plugin source: look up member in JsType
      if (producer.from.kind === "component" || producer.from.kind === "plugin") {
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
      // URL source: read query parameter by name
      if (producer.from.kind === "url") {
        const params = root as URLSearchParams;
        const raw = params.get(producer.member);
        return raw == null ? raw : applyShape(raw, producer.shape);
      }
      // Payload source: walk member as dot-path on resolved payload.
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
