import type { BindSource, ExecContext, Vendor } from "./types";
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
export type CoercionType = "string" | "number" | "boolean" | "date" | "raw";

/**
 * Unified entry point — dispatches by source kind.
 * Currently only handles "event" kind. "component" is a future extension.
 */
export function resolveSource(source: BindSource, ctx?: ExecContext): unknown {
  switch (source.kind) {
    case "event":
      return resolveEventPath(source.path, ctx);
    // Future: case "component": return resolveComponentValue(source);
    default:
      return undefined;
  }
}

/**
 * Event path resolution — walks dot-notation path against execution context.
 */
export function resolveEventPath(path: string, ctx?: ExecContext): unknown {
  if (!ctx) return undefined;

  const parts = path.split(".");
  let current: unknown = ctx;

  for (const part of parts) {
    if (current == null || typeof current !== "object") return undefined;
    current = (current as Record<string, unknown>)[part];
  }

  log.trace("resolve", { path, value: current });
  return current;
}

/**
 * Resolves a BindExpr (plain string path) against the execution context.
 * Kept for backward compatibility with MutateElementCommand.source (string).
 */
export function resolve(expr: BindExpr, ctx?: ExecContext): unknown {
  return resolveEventPath(expr, ctx);
}

/**
 * Resolves a BindSource and coerces the result to a specific type.
 */
export function resolveSourceAs(source: BindSource, coerceAs: CoercionType, ctx?: ExecContext): unknown {
  const raw = resolveSource(source, ctx);
  return coerce(raw, coerceAs);
}

/**
 * Resolves a BindExpr and coerces the result to a specific type.
 * Kept for backward compatibility.
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
 * Reads the current value from a DOM component, vendor-agnostically.
 * readExpr is a property path from the plan: "el.value", "el.checked", "comp.value".
 * The vertical slice owns the expression — runtime just evaluates it.
 */
export function evalRead(id: string, vendor: Vendor, readExpr?: string | null): unknown {
  const el = document.getElementById(id) as any;
  if (!el) return undefined;
  if (!readExpr) {
    if (el.type === "checkbox") return el.checked;
    return el.value;
  }
  const comp = vendor === "fusion" ? el.ej2_instances?.[0] : null;
  if (readExpr.startsWith("comp.")) {
    if (!comp) { log.warn("comp. readExpr on non-fusion element", { id, vendor }); return undefined; }
    return comp[readExpr.substring(5)];
  }
  if (readExpr.startsWith("el.")) return el[readExpr.substring(3)];
  return el.value;
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

    case "date": {
      if (value == null) return NaN;
      const d = new Date(value as string);
      return d.getTime();
    }

    case "raw":
      return value;
  }
}
