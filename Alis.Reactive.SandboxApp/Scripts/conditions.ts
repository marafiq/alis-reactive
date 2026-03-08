import type { Guard, ValueGuard, ExecContext } from "./types";
import { resolve, resolveAs, coerce } from "./resolver";
import { scope } from "./trace";

const log = scope("conditions");

export function evaluateGuard(guard: Guard, ctx?: ExecContext): boolean {
  switch (guard.kind) {
    case "value":
      return evaluateValueGuard(guard, ctx);
    case "all":
      return guard.guards.every(g => evaluateGuard(g, ctx));
    case "any":
      return guard.guards.some(g => evaluateGuard(g, ctx));
    default:
      throw new Error(`Unknown guard kind: ${(guard as any).kind}`);
  }
}

function evaluateValueGuard(guard: ValueGuard, ctx?: ExecContext): boolean {
  const op = guard.op;

  // Presence operators (is-null, not-null) use RAW resolution — coercion
  // would destroy null/undefined (e.g. coerce(null, "string") → "").
  if (op === "is-null" || op === "not-null") {
    const raw = resolve(guard.source, ctx);
    log.trace("eval-presence", { source: guard.source, op, raw });
    return op === "is-null" ? raw == null : raw != null;
  }

  // Truthy/falsy use typed coercion — correctly maps "false" → false for booleans.
  // Comparison operators use typed coercion on BOTH source and operand so that
  // comparisons are type-consistent (e.g. both sides are numbers).
  const resolved = resolveAs(guard.source, guard.coerceAs, ctx);
  const operand = guard.operand != null
    ? coerce(guard.operand, guard.coerceAs)
    : guard.operand;

  log.trace("eval", { source: guard.source, op, resolved, operand });

  switch (op) {
    case "eq":       return resolved === operand;
    case "neq":      return resolved !== operand;
    case "gt":       return (resolved as number) > (operand as number);
    case "gte":      return (resolved as number) >= (operand as number);
    case "lt":       return (resolved as number) < (operand as number);
    case "lte":      return (resolved as number) <= (operand as number);
    case "truthy":   return !!resolved;
    case "falsy":    return !resolved;
    default:
      throw new Error(`Unknown guard operator: ${op}`);
  }
}
