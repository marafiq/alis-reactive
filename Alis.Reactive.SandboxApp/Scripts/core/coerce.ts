// core/coerce.ts — Value coercion for plan-driven resolution
//
// Contract: each function returns CoerceResult<T> — never throws.
// Ok(value) for meaningful conversions. Err(message) for type mismatches.
// Callers decide how to handle Err:
//   - Conditions: guard evaluates to false (else branch)
//   - Validation: rule fails (shows error message to user)
//   - Mutations: coerceOrThrow() — developer error → throw with stack trace
//   - Gather: log warning, skip field
//
// Open/Closed: new coercion type = one case + one function. Zero consumer changes.

export type CoercionType = "string" | "number" | "boolean" | "date" | "raw" | "array";

/** Discriminated union — caller MUST check .ok before using .value. */
export type CoerceResult<T> = { ok: true; value: T } | { ok: false; error: string };

/** Helper to create Ok result. */
function ok<T>(value: T): CoerceResult<T> { return { ok: true, value }; }

/** Helper to create Err result. */
function err<T>(error: string): CoerceResult<T> { return { ok: false, error }; }

/**
 * Coerce a raw value to the target type. Returns Result — never throws.
 * Called by resolver (resolveSourceAs), conditions (operand coercion),
 * validation (comparison rules), and gather (daterange decomposition).
 */
export function coerce(value: unknown, type: CoercionType): CoerceResult<unknown> {
  switch (type) {
    case "string":  return toString(value);
    case "number":  return toNumber(value);
    case "boolean": return toBoolean(value);
    case "date":    return toDate(value);
    case "array":   return toArray(value);
    case "raw":     return ok(value);
  }
}

/**
 * Coerce and unwrap — throws on Err.
 * For developer-facing contexts (mutations, tests) where a type mismatch
 * is a plan bug that needs a stack trace.
 */
export function coerceOrThrow(value: unknown, type: CoercionType): unknown {
  const result = coerce(value, type);
  if (!result.ok) throw new Error(`[alis:coerce] ${result.error}`);
  return result.value;
}

/**
 * Type-aware string conversion — THE single source of truth for value→string.
 *
 * Ok("") for null/undefined (empty = no value)
 * Ok(identity) for string
 * Ok(String(v)) for number/boolean
 * Ok(toISOString()) for Date (ISO 8601, server-parseable, round-trips with toDate)
 * Ok(JSON.stringify()) for Array (preserves structure)
 * Err for plain Object (walk should have decomposed via readExpr)
 */
export function toString(value: unknown): CoerceResult<string> {
  if (value == null) return ok("");
  if (typeof value === "string") return ok(value);
  if (typeof value === "number" || typeof value === "boolean") return ok(String(value));
  if (value instanceof Date) return ok(value.toISOString());
  if (Array.isArray(value)) return ok(JSON.stringify(value));
  return err(
    `toString() received a plain object — ` +
    `missing coerceAs or wrong readExpr. Got: ${JSON.stringify(value)}`
  );
}

/**
 * Numeric coercion.
 *
 * Ok(0) for null/undefined
 * Ok(parsed) for string (NaN → 0)
 * Ok(identity) for number (NaN → 0)
 * Ok(0|1) for boolean
 * Ok(getTime()) for Date (timestamp in ms)
 * Err for Array/Object
 */
export function toNumber(value: unknown): CoerceResult<number> {
  if (value == null) return ok(0);
  if (typeof value === "number") return ok(Number.isNaN(value) ? 0 : value);
  if (typeof value === "boolean") return ok(value ? 1 : 0);
  if (typeof value === "string") {
    const n = Number(value);
    return ok(Number.isNaN(n) ? 0 : n);
  }
  if (value instanceof Date) return ok(value.getTime());
  return err(
    `toNumber() received ${Array.isArray(value) ? "an array" : "a plain object"} — ` +
    `cannot convert to a single number. Got: ${JSON.stringify(value)}`
  );
}

/**
 * String-aware boolean coercion.
 *
 * Ok(false) for null/undefined
 * Ok(identity) for boolean
 * Ok(!"" && !"false" && !"0") for string (HTML form values)
 * Ok(!NaN && !0) for number
 * Ok(true) for Date (has a value = truthy)
 * Ok(length > 0) for Array (non-empty = truthy)
 * Err for plain Object
 */
export function toBoolean(value: unknown): CoerceResult<boolean> {
  if (value == null) return ok(false);
  if (typeof value === "boolean") return ok(value);
  if (typeof value === "string") return ok(value !== "" && value !== "false" && value !== "0");
  if (typeof value === "number") return ok(!Number.isNaN(value) && value !== 0);
  if (value instanceof Date) return ok(true);
  if (Array.isArray(value)) return ok(value.length > 0);
  return err(
    `toBoolean() received a plain object — ` +
    `missing coerceAs or wrong readExpr. Got: ${JSON.stringify(value)}`
  );
}

/**
 * Date coercion → millisecond timestamp (number).
 *
 * Ok(NaN) for null/undefined (callers check isNaN)
 * Ok(getTime()) for Date object (Syncfusion components return Date objects)
 * Ok(identity) for number (already a timestamp)
 * Ok(parsed) for string "YYYY-MM-DD" (LOCAL midnight, avoids UTC off-by-one)
 * Ok(parsed) for string ISO datetime
 * Err for boolean/Array/Object
 */
export function toDate(value: unknown): CoerceResult<number> {
  if (value == null) return ok(NaN);
  if (value instanceof Date) return ok(value.getTime());
  if (typeof value === "number") return ok(value); // already a timestamp

  if (typeof value === "string") {
    // Date-only "YYYY-MM-DD" — parse as LOCAL midnight, not UTC
    // new Date("2025-03-19") parses as UTC midnight which shifts to previous day in western timezones
    if (/^\d{4}-\d{2}-\d{2}$/.test(value)) {
      const [y, m, d] = value.split("-").map(Number);
      return ok(new Date(y, m - 1, d).getTime());
    }
    return ok(new Date(value).getTime());
  }

  return err(
    `toDate() received ${typeof value} — ` +
    `not a date, string, or timestamp. Got: ${JSON.stringify(value)}`
  );
}

/**
 * Array normalization — wraps scalars, passes arrays through.
 *
 * Ok(identity) for Array
 * Ok([]) for null/undefined/""
 * Ok([scalar]) for any scalar (wrap in single-element array)
 * Err for plain Object (walk should have decomposed via readExpr)
 */
export function toArray(value: unknown): CoerceResult<unknown[]> {
  if (Array.isArray(value)) return ok(value);
  if (value == null || value === "") return ok([]);
  if (typeof value === "object" && !(value instanceof Date)) {
    return err(
      `toArray() received a plain object — ` +
      `expected an array or scalar. Got: ${JSON.stringify(value)}`
    );
  }
  return ok([value]);
}
