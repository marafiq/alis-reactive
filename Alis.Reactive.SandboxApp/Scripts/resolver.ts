import type { BindSource, ExecContext } from "./types";
import { walk } from "./walk";
import { evalRead } from "./component";
import { scope } from "./trace";

const log = scope("resolver");

/**
 * A BindExpr is a dot-notation path into the execution context.
 * Examples: "evt.intValue", "evt.address.city", "evt.boolValue"
 *
 * The resolver walks the path against the context object and returns
 * the raw value. Callers decide how to use it (string coercion, etc.).
 */
export type BindExpr = string;

/**
 * Coercion types for resolved values. The resolver can coerce raw values
 * to specific types for use in conditions, comparisons, and DOM rendering.
 */
export type CoercionType = "string" | "number" | "boolean" | "date" | "raw" | "array";

/**
 * Unified entry point — dispatches by source kind.
 * Handles both "event" (walk execution context) and "component" (read from DOM component).
 */
export function resolveSource(source: BindSource, ctx?: ExecContext): unknown {
  switch (source.kind) {
    case "event":
      return resolveEventPath(source.path, ctx);
    case "component":
      return evalRead(source.componentId, source.vendor, source.readExpr);
    default:
      return undefined;
  }
}

/**
 * Event path resolution — walks dot-notation path against execution context.
 * Uses walk() from walk.ts (shared primitive).
 */
export function resolveEventPath(path: string, ctx?: ExecContext): unknown {
  if (!ctx) return undefined;
  const result = walk(ctx, path);
  log.trace("resolve", { path, value: result });
  return result;
}

/**
 * Resolves a BindSource and coerces the result to a specific type.
 */
export function resolveSourceAs(source: BindSource, coerceAs: CoercionType, ctx?: ExecContext): unknown {
  const raw = resolveSource(source, ctx);
  return coerce(raw, coerceAs);
}

/**
 * Coerces a raw value to the specified type.
 *
 * | CoercionType | Behavior |
 * |-------------|----------|
 * | "string"    | String(value), null/undefined -> "" |
 * | "number"    | Number(value), NaN -> 0 |
 * | "boolean"   | truthy check, "false"/"0"/"" -> false |
 * | "raw"       | no coercion, return as-is |
 */
export function coerce(value: unknown, coerceAs: CoercionType): unknown {
  switch (coerceAs) {
    case "string":
      return String(value ?? "");

    case "number": {
      const n = Number(value);
      return Number.isNaN(n) ? 0 : n;
    }

    case "boolean":
      if (typeof value === "string") {
        return value !== "" && value !== "false" && value !== "0";
      }
      return Boolean(value);

    case "date": {
      if (value == null) return NaN;
      const d = new Date(value as string);
      return d.getTime();
    }

    case "array":
      if (Array.isArray(value)) return value;
      if (value == null || value === "") return [];
      return [value];

    case "raw":
      return value;
  }
}
