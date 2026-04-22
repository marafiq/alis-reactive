// core/shape-convert.ts — Shape-based value conversion
//
// Shape is the foundational type contract. This module is the SINGLE place
// where Shape → value conversion happens. Every module that needs type
// conversion calls applyShape(). No other conversion path exists.
//
// Contract: conversion functions return ConvertResult<T> — never throw.
// Ok(value) for meaningful conversions. Err(message) for type mismatches.
// applyShape() is the main entry point — returns converted value or original on failure.

import type { Shape } from "../types";

/** Discriminated result — caller MUST check .ok before using .value. */
export type ConvertResult<T> = { ok: true; value: T } | { ok: false; error: string };

function ok<T>(value: T): ConvertResult<T> { return { ok: true, value }; }
function err<T>(error: string): ConvertResult<T> { return { ok: false, error }; }

/**
 * Apply a Shape to a raw value. This is the ONE entry point for Shape conversion.
 * Every module calls this — conditions, validation, gather, execution.
 */
export function applyShape(value: unknown, shape: Shape): unknown {
  switch (shape.kind) {
    case "string":   return applyScalar(value, toString);
    case "number":   return applyScalar(value, toNumber);
    case "boolean":  return applyScalar(value, toBoolean);
    case "date":     return applyScalar(value, toDate);
    case "array":    return applyArrayShape(value, shape);
    case "nullable": return value == null ? null : applyShape(value, shape.inner);
    case "raw":
    case "any":
    case "object":
    case "none":     return value;
    default: {
      const _: never = shape;
      throw new Error(`[alis] unknown shape kind: "${(_ as Shape).kind}"`);
    }
  }
}

/** Apply a scalar conversion, returning original value on failure. */
function applyScalar<T>(value: unknown, convert: (v: unknown) => ConvertResult<T>): unknown {
  const r = convert(value);
  return r.ok ? r.value : value;
}

/**
 * Shape-aware value equality. Required because shape-applied domain values
 * are not all === comparable: Date objects with the same instant are distinct
 * references, so ===/!==/Array.includes silently misreport equality. Ordering
 * operators (>,>=,<,<=) are unaffected — JS calls valueOf on Date, so epoch-ms
 * comparison falls out naturally. Recurses on nullable/array compositions.
 */
export function shapeEquals(a: unknown, b: unknown, shape: Shape): boolean {
  if (a == null || b == null) return a === b;
  switch (shape.kind) {
    case "date":
      return a instanceof Date && b instanceof Date && a.getTime() === b.getTime();
    case "nullable":
      return shapeEquals(a, b, shape.inner);
    case "array":
      return Array.isArray(a) && Array.isArray(b)
        && a.length === b.length
        && a.every((v, i) => shapeEquals(v, b[i], shape.item));
    default:
      return a === b;
  }
}

/** Apply array shape — convert to array, then recursively apply item shape. */
function applyArrayShape(value: unknown, shape: Extract<Shape, { kind: "array" }>): unknown {
  const r = toArray(value);
  if (!r.ok) return value;
  return shape.item ? r.value.map(v => applyShape(v, shape.item)) : r.value;
}

/**
 * Convert a value according to a Shape. Returns Result — never throws.
 */
export function convertByShape(value: unknown, shape: Shape): ConvertResult<unknown> {
  switch (shape.kind) {
    case "string":   return toString(value);
    case "number":   return toNumber(value);
    case "boolean":  return toBoolean(value);
    case "date":     return toDate(value);
    case "array":    return convertArrayShape(value, shape);
    case "nullable": return value == null ? ok(null) : convertByShape(value, shape.inner);
    case "raw":
    case "any":
    case "object":
    case "none":     return ok(value);
    default: {
      const _: never = shape;
      throw new Error(`[alis] unknown shape kind: "${(_ as Shape).kind}"`);
    }
  }
}

/** Convert array shape — convert to array, then recursively apply item shape. */
function convertArrayShape(value: unknown, shape: Extract<Shape, { kind: "array" }>): ConvertResult<unknown> {
  const r = toArray(value);
  if (!r.ok) return r;
  return shape.item ? ok(r.value.map(v => applyShape(v, shape.item))) : r;
}

// ── Conversion functions ──────────────────────────────────

export function toString(value: unknown): ConvertResult<string> {
  if (value == null) return ok("");
  if (typeof value === "string") return ok(value);
  if (typeof value === "number" || typeof value === "boolean") return ok(String(value));
  if (value instanceof Date) return ok(value.toISOString());
  if (Array.isArray(value)) return ok(JSON.stringify(value));
  return err(`toString: received object — missing shape or wrong member. Got: ${JSON.stringify(value)}`);
}

export function toNumber(value: unknown): ConvertResult<number> {
  if (value == null) return ok(0);
  if (typeof value === "number") return ok(Number.isNaN(value) ? 0 : value);
  if (typeof value === "boolean") return ok(value ? 1 : 0);
  if (typeof value === "string") {
    const n = Number(value);
    return ok(Number.isNaN(n) ? 0 : n);
  }
  if (value instanceof Date) return ok(value.getTime());
  return err(`toNumber: cannot convert ${typeof value} to number`);
}

export function toBoolean(value: unknown): ConvertResult<boolean> {
  if (value == null) return ok(false);
  if (typeof value === "boolean") return ok(value);
  if (typeof value === "string") return ok(value !== "" && value !== "false" && value !== "0");
  if (typeof value === "number") return ok(!Number.isNaN(value) && value !== 0);
  if (value instanceof Date) return ok(true);
  if (Array.isArray(value)) return ok(value.length > 0);
  return err(`toBoolean: received object — cannot convert`);
}

export function toDate(value: unknown): ConvertResult<Date> {
  if (value == null) return err(`toDate: null is not a date`);
  if (value instanceof Date) return ok(value);
  if (typeof value === "number") {
    const d = new Date(value);
    return isNaN(d.getTime()) ? err(`toDate: NaN number`) : ok(d);
  }
  if (typeof value === "string") {
    // Date-only "YYYY-MM-DD" — parse as LOCAL midnight, not UTC
    if (/^\d{4}-\d{2}-\d{2}$/.test(value)) {
      const [y, m, d] = value.split("-").map(Number);
      return ok(new Date(y, m - 1, d));
    }
    const d = new Date(value);
    return isNaN(d.getTime()) ? err(`toDate: unparseable string "${value}"`) : ok(d);
  }
  return err(`toDate: received ${typeof value} — not a date or timestamp`);
}

export function toArray(value: unknown): ConvertResult<unknown[]> {
  if (Array.isArray(value)) return ok(value);
  if (value == null || value === "") return ok([]);
  if (typeof value === "object" && !(value instanceof Date)) {
    return err(`toArray: received object — expected array or scalar`);
  }
  return ok([value]);
}
