// core/coerce.ts — Value coercion for plan-driven resolution
//
// Each coercion type is a named function so tests can target it directly.
// The main coerce() dispatches by type.
// Open for extension — new types are added as new functions + a case.

export type CoercionType = "string" | "number" | "boolean" | "date" | "raw" | "array";

/**
 * Coerce a raw value to the target type.
 * Called by resolver (resolveSourceAs), element (mutation coerce),
 * and conditions (operand coercion).
 */
export function coerce(value: unknown, type: CoercionType): unknown {
  switch (type) {
    case "string":  return toString(value);
    case "number":  return toNumber(value);
    case "boolean": return toBoolean(value);
    case "date":    return toDate(value);
    case "array":   return toArray(value);
    case "raw":     return value;
  }
}

/** Type-aware string conversion — THE single source of truth for value→string. */
export function toString(value: unknown): string {
  if (value == null) return "";
  if (typeof value === "string") return value;
  if (typeof value === "number" || typeof value === "boolean") return String(value);
  if (value instanceof Date) return value.toISOString();
  if (Array.isArray(value)) return JSON.stringify(value);
  throw new Error(
    `[alis:coerce] toString() received a plain object — ` +
    `missing coerceAs or wrong readExpr. Got: ${JSON.stringify(value)}`
  );
}

/** NaN → 0. null/undefined → 0. Everything else → Number(). */
export function toNumber(value: unknown): number {
  const n = Number(value);
  return Number.isNaN(n) ? 0 : n;
}

/**
 * String-aware boolean coercion.
 * HTML form values arrive as strings — "false", "0", "" must be false.
 * Non-string values use standard Boolean().
 */
export function toBoolean(value: unknown): boolean {
  if (typeof value === "string") {
    return value !== "" && value !== "false" && value !== "0";
  }
  return Boolean(value);
}

/**
 * Date coercion → millisecond timestamp (number).
 *
 * null/undefined → NaN (fail-fast, caller must handle).
 * Date object → getTime() (Syncfusion components return Date objects).
 * "YYYY-MM-DD" string → LOCAL midnight (not UTC — avoids the off-by-one day bug).
 * ISO datetime string → parsed as spec says.
 */
export function toDate(value: unknown): number {
  if (value == null) return NaN;
  if (value instanceof Date) return value.getTime();
  if (typeof value === "number") return value; // already a timestamp

  const str = String(value);

  // Date-only "YYYY-MM-DD" — parse as LOCAL midnight, not UTC
  // new Date("2025-03-19") parses as UTC midnight which shifts to previous day in western timezones
  if (/^\d{4}-\d{2}-\d{2}$/.test(str)) {
    const [y, m, d] = str.split("-").map(Number);
    return new Date(y, m - 1, d).getTime();
  }

  return new Date(str).getTime();
}

/** Wrap scalars in array. null/"" → []. Arrays pass through. */
export function toArray(value: unknown): unknown[] {
  if (Array.isArray(value)) return value;
  if (value == null || value === "") return [];
  return [value];
}
