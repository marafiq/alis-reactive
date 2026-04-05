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
export function applyShape(value: unknown, shape?: Shape): unknown {
  if (!shape) return value;

  switch (shape.kind) {
    case "string":   { const r = toString(value); return r.ok ? r.value : value; }
    case "number":   { const r = toNumber(value); return r.ok ? r.value : value; }
    case "boolean":  { const r = toBoolean(value); return r.ok ? r.value : value; }
    case "date":     { const r = toDate(value); return r.ok ? r.value : value; }
    case "array":    { const r = toArray(value); return r.ok ? r.value : value; }
    case "nullable": return value == null ? null : applyShape(value, shape.inner);
    case "raw":      return value;
    case "any":      return value;
    case "object":   return value;
    default: {
      const _: never = shape;
      throw new Error(`[alis] unknown shape kind: "${(_ as Shape).kind}"`);
    }
  }
}

/**
 * Convert and unwrap — throws on failure.
 * For developer-facing contexts where a type mismatch is a plan bug.
 */
export function convertOrThrow(value: unknown, shape: Shape): unknown {
  const result = convertByShape(value, shape);
  if (!result.ok) throw new Error(`[alis] shape conversion failed: ${result.error}`);
  return result.value;
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
    case "array":    return toArray(value);
    case "nullable": return value == null ? ok(null) : convertByShape(value, shape.inner);
    case "raw":      return ok(value);
    case "any":      return ok(value);
    case "object":   return ok(value);
    default: {
      const _: never = shape;
      throw new Error(`[alis] unknown shape kind: "${(_ as Shape).kind}"`);
    }
  }
}

/**
 * Compare two values using Shape-aware conversion.
 * Used by conditions and validation rule engine.
 */
export function compareWithShape(a: unknown, b: unknown, shape?: Shape): { ca: unknown; cb: unknown } {
  if (!shape) return { ca: a, cb: b };
  return { ca: applyShape(a, shape), cb: applyShape(b, shape) };
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

export function toDate(value: unknown): ConvertResult<number> {
  if (value == null) return ok(NaN);
  if (value instanceof Date) return ok(value.getTime());
  if (typeof value === "number") return ok(value);
  if (typeof value === "string") {
    // Date-only "YYYY-MM-DD" — parse as LOCAL midnight, not UTC
    if (/^\d{4}-\d{2}-\d{2}$/.test(value)) {
      const [y, m, d] = value.split("-").map(Number);
      return ok(new Date(y, m - 1, d).getTime());
    }
    return ok(new Date(value).getTime());
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
