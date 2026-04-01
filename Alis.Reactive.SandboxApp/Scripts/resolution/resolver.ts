import type { BindSource, ExecContext } from "../types";
import { walk } from "../core/walk";
import { evalRead } from "./component";
import { scope } from "../core/trace";
import { assertNever } from "../core/assert-never";
import { coerce } from "../core/coerce";
import type { CoercionType } from "../core/coerce";

const log = scope("resolver");

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
 * Only the public runtime roots are visible here: evt and responseBody.
 */
export function resolveEventPath(path: string, ctx?: ExecContext): unknown {
  const scoped = resolveScopedContextPath(path, ctx);
  if (!scoped) return undefined;

  const result = scoped.path ? walk(scoped.root, scoped.path) : scoped.root;
  log.trace("resolve", { path, value: result });
  return result;
}

function resolveScopedContextPath(
  path: string,
  ctx?: ExecContext
): { root: unknown; path: string } | undefined {
  if (!ctx) return undefined;
  if (path === "evt") return { root: ctx.evt, path: "" };
  if (path === "responseBody") return { root: ctx.responseBody, path: "" };
  if (path.startsWith("evt.")) return { root: ctx.evt, path: path.slice(4) };
  if (path.startsWith("responseBody.")) {
    return { root: ctx.responseBody, path: path.slice("responseBody.".length) };
  }
  return undefined;
}

/**
 * Resolves a BindSource and coerces the result to a specific type.
 * Returns undefined on coercion failure and logs a warning.
 */
export function resolveSourceAs(source: BindSource, coerceAs: CoercionType, ctx?: ExecContext): unknown {
  const raw = resolveSource(source, ctx);
  const result = coerce(raw, coerceAs);
  if (!result.ok) {
    log.warn("resolveSourceAs coerce failed", { source, coerceAs, error: result.error });
    return undefined;
  }
  return result.value;
}
