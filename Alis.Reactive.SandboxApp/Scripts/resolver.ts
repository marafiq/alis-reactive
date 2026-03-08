import type { ExecContext } from "./types";
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
export type CoercionType = "string" | "number" | "boolean" | "raw";

/**
 * Resolves a BindExpr against the execution context.
 * Returns the raw value at the path, or undefined if not found.
 */
export function resolve(expr: BindExpr, ctx?: ExecContext): unknown {
  if (!ctx) return undefined;

  const parts = expr.split(".");
  let current: unknown = ctx;

  for (const part of parts) {
    if (current == null || typeof current !== "object") return undefined;
    current = (current as Record<string, unknown>)[part];
  }

  log.trace("resolve", { expr, value: current });
  return current;
}

/**
 * Resolves a BindExpr and coerces the result to a specific type.
 */
export function resolveAs(expr: BindExpr, coerceAs: CoercionType, ctx?: ExecContext): unknown {
  const raw = resolve(expr, ctx);
  return coerce(raw, coerceAs);
}

/**
 * Resolves a BindExpr and coerces to string for DOM rendering.
 */
export function resolveToString(expr: BindExpr, ctx?: ExecContext): string {
  const raw = resolve(expr, ctx);
  return String(raw ?? "");
}

/**
 * Coerces a raw value to the specified type.
 *
 * | CoercionType | Behavior |
 * |-------------|----------|
 * | "string"    | String(value), null/undefined → "" |
 * | "number"    | Number(value), NaN → 0 |
 * | "boolean"   | truthy check, "false"/"0"/"" → false |
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

    case "raw":
      return value;
  }
}
