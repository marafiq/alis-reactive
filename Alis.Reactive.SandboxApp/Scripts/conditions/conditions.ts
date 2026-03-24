import type { Guard, ValueGuard, ExecContext } from "../types";
import { resolveSource, resolveSourceAs } from "../resolution/resolver";
import { coerce, toString } from "../core/coerce";
import { scope } from "../core/trace";
import { assertNever } from "../core/assert-never";

const log = scope("conditions");

export function evaluateGuard(guard: Guard, ctx?: ExecContext): boolean {
  switch (guard.kind) {
    case "value":
      return evaluateValueGuard(guard, ctx);
    case "all":
      return guard.guards.every(g => evaluateGuard(g, ctx));
    case "any":
      return guard.guards.some(g => evaluateGuard(g, ctx));
    case "not":
      return !evaluateGuard(guard.inner, ctx);
    case "confirm":
      log.warn("ConfirmGuard in sync context — denying (callers should use async path)");
      return false;
    default:
      assertNever(guard, "guard kind");
  }
}

/**
 * Async guard evaluation — required when branches contain ConfirmGuard.
 * Calls window.alis.confirm(message) for confirm guards.
 */
export async function evaluateGuardAsync(guard: Guard, ctx?: ExecContext): Promise<boolean> {
  switch (guard.kind) {
    case "value":
      return evaluateValueGuard(guard, ctx);
    case "all":
      for (const g of guard.guards) {
        if (!await evaluateGuardAsync(g, ctx)) return false;
      }
      return true;
    case "any":
      for (const g of guard.guards) {
        if (await evaluateGuardAsync(g, ctx)) return true;
      }
      return false;
    case "not":
      return !(await evaluateGuardAsync(guard.inner, ctx));
    case "confirm":
      return (window as any).alis?.confirm?.(guard.message) ?? Promise.resolve(false);
    default:
      assertNever(guard, "guard kind");
  }
}

/**
 * Checks whether a guard tree contains any ConfirmGuard.
 * Used to decide between sync and async evaluation paths.
 */
export function isConfirmGuard(guard: Guard): boolean {
  if (guard.kind === "confirm") return true;
  if (guard.kind === "not") return isConfirmGuard(guard.inner);
  if (guard.kind === "all" || guard.kind === "any")
    return guard.guards.some(isConfirmGuard);
  return false;
}

/** Evaluates presence/emptiness operators using RAW resolution (no coercion). */
function evaluatePresenceOp(guard: ValueGuard, ctx?: ExecContext): boolean | undefined {
  const op = guard.op;
  if (op === "is-null" || op === "not-null") {
    const raw = resolveSource(guard.source, ctx);
    log.trace("eval-presence", { source: guard.source, op, raw });
    return op === "is-null" ? raw == null : raw != null;
  }
  if (op === "is-empty" || op === "not-empty") {
    const raw = resolveSource(guard.source, ctx);
    log.trace("eval-presence", { source: guard.source, op, raw });
    const isEmpty = raw === "" || raw === null || raw === undefined
      || (Array.isArray(raw) && raw.length === 0);
    return op === "is-empty" ? isEmpty : !isEmpty;
  }
  return undefined; // Not a presence op
}

interface ResolvedOperands {
  resolved: unknown;
  operand: unknown;
  items: unknown[] | undefined;
}

/** Resolves source, operand, and items with conditional coercion for value comparison. */
function resolveGuardOperands(guard: ValueGuard, ctx?: ExecContext): ResolvedOperands {
  // Truthy/falsy use typed coercion — correctly maps "false" → false for booleans.
  // Comparison operators use typed coercion on BOTH source and operand so that
  // comparisons are type-consistent (e.g. both sides are numbers).
  const resolved = resolveSourceAs(guard.source, guard.coerceAs, ctx);

  // Operand coercion: elementCoerceAs (for array operators) or coerceAs (for scalars).
  // For non-array operators elementCoerceAs is null, so opCoerceAs === coerceAs — same behavior.
  const opCoerceAs = guard.elementCoerceAs ?? guard.coerceAs;

  // Source-vs-source: if rightSource present, resolve it instead of literal operand.
  // For array operands (in, not-in, between), coerce each element individually.
  // For scalar operands, coerce the whole value.
  const rawOp = guard.rightSource
    ? resolveSourceAs(guard.rightSource, opCoerceAs, ctx)
    : guard.operand;
  let operand: unknown = rawOp;
  if (rawOp != null && !guard.rightSource) {
    operand = Array.isArray(rawOp)
      ? rawOp.map(v => { const r = coerce(v, opCoerceAs); return r.ok ? r.value : undefined; })
      : (() => { const r = coerce(rawOp, opCoerceAs); return r.ok ? r.value : undefined; })();
  }

  // For array sources with element coercion: pre-coerce elements so switch cases stay pure.
  // For non-array operators elementCoerceAs is null → items is undefined → unused.
  const items = guard.elementCoerceAs != null && Array.isArray(resolved)
    ? (resolved as unknown[]).map(item => { const r = coerce(item, guard.elementCoerceAs!); return r.ok ? r.value : undefined; })
    : undefined;

  return { resolved, operand, items };
}

function evaluateValueGuard(guard: ValueGuard, ctx?: ExecContext): boolean {
  const presenceResult = evaluatePresenceOp(guard, ctx);
  if (presenceResult !== undefined) return presenceResult;

  const { resolved, operand, items } = resolveGuardOperands(guard, ctx);
  log.trace("eval", { source: guard.source, op: guard.op, resolved, operand });

  switch (guard.op) {
    case "eq":       return resolved === operand;
    case "neq":      return resolved !== operand;
    case "gt":       return (resolved as number) > (operand as number);
    case "gte":      return (resolved as number) >= (operand as number);
    case "lt":       return (resolved as number) < (operand as number);
    case "lte":      return (resolved as number) <= (operand as number);
    case "truthy":   return !!resolved;
    case "falsy":    return !resolved;

    // Membership — operand is coerced per-element above
    case "in":
      return Array.isArray(operand) && operand.includes(resolved);
    case "not-in":
      return !Array.isArray(operand) || !operand.includes(resolved);

    // Range — operand is coerced per-element above
    case "between":
      return Array.isArray(operand) && (resolved as number) >= operand[0] && (resolved as number) <= operand[1];

    // Array membership — elements and operand pre-coerced via elementCoerceAs above
    case "array-contains":
      return items?.includes(operand) ?? false;

    // Text — resolve source as STRING, not as guard.coerceAs.
    // guard.coerceAs may be "date" (→ timestamp number) or "number" — text ops
    // need the string form. resolveSourceAs with "string" calls toString() which
    // handles Date→ISO, number→digits, boolean→"true"/"false".
    case "contains": {
      const str = resolveSourceAs(guard.source, "string", ctx);
      if (str == null) return false;
      const opResult = toString(operand);
      if (!opResult.ok) return false;
      return (str as string).includes(opResult.value);
    }
    case "starts-with": {
      const str = resolveSourceAs(guard.source, "string", ctx);
      if (str == null) return false;
      const opResult = toString(operand);
      if (!opResult.ok) return false;
      return (str as string).startsWith(opResult.value);
    }
    case "ends-with": {
      const str = resolveSourceAs(guard.source, "string", ctx);
      if (str == null) return false;
      const opResult = toString(operand);
      if (!opResult.ok) return false;
      return (str as string).endsWith(opResult.value);
    }
    case "matches": {
      const str = resolveSourceAs(guard.source, "string", ctx);
      if (str == null) return false;
      const opResult = toString(operand);
      if (!opResult.ok) return false;
      try {
        return new RegExp(opResult.value).test(str as string);
      } catch {
        log.warn("invalid guard regex", { operand });
        return false;
      }
    }
    case "min-length": {
      const str = resolveSourceAs(guard.source, "string", ctx);
      if (str == null) return false;
      return (str as string).length >= Number(operand);
    }

    default:
      throw new Error(`Unknown guard operator: ${guard.op}`);
  }
}
