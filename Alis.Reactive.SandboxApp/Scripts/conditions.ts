import type { Guard, ValueGuard, ExecContext } from "./types";
import { resolveAs } from "./resolver";
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
  }
}

function evaluateValueGuard(guard: ValueGuard, ctx?: ExecContext): boolean {
  const resolved = resolveAs(guard.source, guard.coerceAs, ctx);
  const op = guard.op;
  const operand = guard.operand;

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
    case "is-null":  return resolved == null;
    case "not-null": return resolved != null;
  }
}
