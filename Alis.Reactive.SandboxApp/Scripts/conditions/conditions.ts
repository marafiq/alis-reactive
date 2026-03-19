import type { Guard, ValueGuard, ExecContext } from "../types";
import { resolveSource, resolveSourceAs, coerce } from "../resolution/resolver";
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

function evaluateValueGuard(guard: ValueGuard, ctx?: ExecContext): boolean {
  const op = guard.op;

  // Presence operators (is-null, not-null) use RAW resolution — coercion
  // would destroy null/undefined (e.g. coerce(null, "string") → "").
  if (op === "is-null" || op === "not-null") {
    const raw = resolveSource(guard.source, ctx);
    log.trace("eval-presence", { source: guard.source, op, raw });
    return op === "is-null" ? raw == null : raw != null;
  }

  // is-empty / not-empty also use raw resolution
  if (op === "is-empty" || op === "not-empty") {
    const raw = resolveSource(guard.source, ctx);
    log.trace("eval-presence", { source: guard.source, op, raw });
    const isEmpty = raw === "" || raw === null || raw === undefined
      || (Array.isArray(raw) && raw.length === 0);
    return op === "is-empty" ? isEmpty : !isEmpty;
  }

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
  const operand = rawOp != null && !guard.rightSource
    ? (Array.isArray(rawOp) ? rawOp.map(v => coerce(v, opCoerceAs)) : coerce(rawOp, opCoerceAs))
    : rawOp;

  // For array sources with element coercion: pre-coerce elements so switch cases stay pure.
  // For non-array operators elementCoerceAs is null → items is undefined → unused.
  const items = guard.elementCoerceAs != null && Array.isArray(resolved)
    ? (resolved as unknown[]).map(item => coerce(item, guard.elementCoerceAs!))
    : undefined;

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
      return items != null && items.includes(operand);

    // Text
    case "contains":
      return String(resolved ?? "").includes(String(operand));
    case "starts-with":
      return String(resolved ?? "").startsWith(String(operand));
    case "ends-with":
      return String(resolved ?? "").endsWith(String(operand));
    case "matches": {
      try {
        return new RegExp(String(operand)).test(String(resolved ?? ""));
      } catch {
        log.warn("invalid guard regex", { operand });
        return false;
      }
    }
    case "min-length":
      return String(resolved ?? "").length >= Number(operand);

    default:
      throw new Error(`Unknown guard operator: ${op}`);
  }
}
