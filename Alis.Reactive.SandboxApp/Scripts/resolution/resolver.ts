import type { BindSource, ExecContext } from "../types";
import { walk } from "../core/walk";
import { evalRead } from "./component";
import { scope } from "../core/trace";
import { assertNever } from "../core/assert-never";
import { coerce } from "../core/coerce";
import type { CoercionType } from "../core/coerce";

const log = scope("resolver");

/**
 * A BindExpr is a dot-notation path into the execution context.
 * Examples: "evt.intValue", "evt.address.city", "evt.boolValue"
 */
export type BindExpr = string;

// Re-export coerce and CoercionType — callers can import from resolver or core/coerce
export { coerce, type CoercionType };

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
      assertNever(source, "source kind");
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
