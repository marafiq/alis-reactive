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
function isMissingInput(value: unknown): boolean { return value === null || value === undefined; }

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
    case "object":   return applyObjectShape(value, shape);
    case "nullable": return applyNullableShape(value, shape.inner);
    case "raw":
    case "any":
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
  if (r.ok) return r.value;
  return value;
}

/** Apply array shape — convert to array, then recursively apply item shape. */
function applyArrayShape(value: unknown, shape: Extract<Shape, { kind: "array" }>): unknown {
  const r = toArray(value);
  if (!r.ok) return value;
  return applyArrayItemShape(r.value, shape);
}

function applyNullableShape(value: unknown, inner: Shape): unknown {
  if (isMissingInput(value)) return null;
  return applyShape(value, inner);
}

function applyArrayItemShape(items: unknown[], shape: Extract<Shape, { kind: "array" }>): unknown[] {
  return items.map(item => applyShape(item, shape.item));
}

function applyObjectShape(value: unknown, shape: Extract<Shape, { kind: "object" }>): unknown {
  const record = toPlainObject(value);
  if (!record.ok) return value;

  return ObjectShapeProjection.from(record.value, shape).apply();
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
    case "object":   return convertObjectShape(value, shape);
    case "nullable": return convertNullableShape(value, shape.inner);
    case "raw":
    case "any":
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
  return ok(applyArrayItemShape(r.value, shape));
}

function convertObjectShape(value: unknown, shape: Extract<Shape, { kind: "object" }>): ConvertResult<unknown> {
  const record = toPlainObject(value);
  if (!record.ok) return record;
  return ok(ObjectShapeProjection.from(record.value, shape).apply());
}

function convertNullableShape(value: unknown, inner: Shape): ConvertResult<unknown> {
  if (isMissingInput(value)) return ok(null);
  return convertByShape(value, inner);
}

class ObjectShapeProjection {
  private constructor(
    private readonly input: Record<string, unknown>,
    private readonly shape: Extract<Shape, { kind: "object" }>,
  ) {}

  static from(
    input: Record<string, unknown>,
    shape: Extract<Shape, { kind: "object" }>,
  ): ObjectShapeProjection {
    return new ObjectShapeProjection(input, shape);
  }

  apply(): Record<string, unknown> {
    if (this.keepsInputAsDeclared()) return this.input;

    const output: Record<string, unknown> = {};
    if (this.shape.additional) this.copyAdditionalFieldsTo(output);
    this.applyDeclaredFieldsTo(output);
    return output;
  }

  private keepsInputAsDeclared(): boolean {
    return this.shape.additional && Object.keys(this.shape.fields).length === 0;
  }

  private copyAdditionalFieldsTo(output: Record<string, unknown>): void {
    Object.assign(output, this.input);
  }

  private applyDeclaredFieldsTo(output: Record<string, unknown>): void {
    for (const [field, fieldShape] of Object.entries(this.shape.fields)) {
      if (!Object.prototype.hasOwnProperty.call(this.input, field)) continue;
      output[field] = applyShape(this.input[field], fieldShape);
    }
  }
}

// ── Conversion functions ──────────────────────────────────

export function toString(value: unknown): ConvertResult<string> {
  if (isMissingInput(value)) return ok("");
  if (typeof value === "string") return ok(value);
  if (typeof value === "number" || typeof value === "boolean") return ok(`${value}`);
  if (value instanceof Date) return ok(value.toISOString());
  if (Array.isArray(value)) return ok(String(value));
  return err(`toString: received object — missing shape or wrong member. Got: ${JSON.stringify(value)}`);
}

export function toNumber(value: unknown): ConvertResult<number> {
  if (isMissingInput(value)) return ok(0);
  if (typeof value === "number") return finiteNumber(value, "number");
  if (typeof value === "boolean") return ok(booleanAsNumber(value));
  if (typeof value === "string") {
    return numberFromText(value);
  }
  if (value instanceof Date) return finiteNumber(value.getTime(), "date");
  return err(`toNumber: cannot convert ${typeof value} to number`);
}

export function toBoolean(value: unknown): ConvertResult<boolean> {
  if (isMissingInput(value)) return ok(false);
  if (typeof value === "boolean") return ok(value);
  if (typeof value === "string") return ok(textIsTruthy(value));
  if (typeof value === "number") return ok(numberIsTruthy(value));
  if (value instanceof Date) return ok(true);
  if (Array.isArray(value)) return ok(value.length > 0);
  return err(`toBoolean: received object — cannot convert`);
}

export function toDate(value: unknown): ConvertResult<number> {
  if (isMissingInput(value)) return ok(NaN);
  if (value instanceof Date) return finiteNumber(value.getTime(), "Date object");
  if (typeof value === "number") return finiteNumber(value, "date timestamp");
  if (typeof value === "string") {
    // Date-only "YYYY-MM-DD" — parse as LOCAL midnight, not UTC
    const textIsDateOnly = /^\d{4}-\d{2}-\d{2}$/.test(value);
    if (textIsDateOnly) {
      const dateOnly = DateOnlyText.parse(value);
      if (dateOnly === undefined) return err(`toDate: invalid date-only text "${value}"`);
      return ok(dateOnly.toLocalMidnightTimestamp());
    }
    return finiteNumber(new Date(value).getTime(), `date text "${value}"`);
  }
  return err(`toDate: received ${typeof value} — not a date or timestamp`);
}

class DateOnlyText {
  private constructor(
    private readonly year: number,
    private readonly month: number,
    private readonly day: number,
  ) {}

  static parse(value: string): DateOnlyText | undefined {
    const [year, month, day] = value.split("-").map(Number);
    if (year === undefined || month === undefined || day === undefined) return undefined;
    const parsed = new Date(year, month - 1, day);
    const parsedDateMatchesInput =
      parsed.getFullYear() === year
      && parsed.getMonth() === month - 1
      && parsed.getDate() === day;
    if (!parsedDateMatchesInput) return undefined;

    return new DateOnlyText(year, month, day);
  }

  toLocalMidnightTimestamp(): number {
    return new Date(this.year, this.month - 1, this.day).getTime();
  }
}

export function toArray(value: unknown): ConvertResult<unknown[]> {
  if (Array.isArray(value)) return ok(value);
  const missingOrEmptyTextRepresentsEmptyArray = isMissingInput(value) || value === "";
  if (missingOrEmptyTextRepresentsEmptyArray) return ok([]);

  const objectCannotBecomeArray = typeof value === "object" && !(value instanceof Date);
  if (objectCannotBecomeArray) {
    return err(`toArray: received object — expected array or scalar`);
  }
  return ok([value]);
}

export function toPlainObject(value: unknown): ConvertResult<Record<string, unknown>> {
  const valueIsPlainObject =
    typeof value === "object"
    && value !== null
    && !Array.isArray(value)
    && !(value instanceof Date);
  if (valueIsPlainObject) return ok(value as Record<string, unknown>);

  return err(`toObject: received ${typeof value} — expected object`);
}

function booleanAsNumber(value: boolean): number {
  if (value) return 1;
  return 0;
}

function numberFromText(value: string): ConvertResult<number> {
  const parsed = Number(value);
  return finiteNumber(parsed, `text "${value}"`);
}

function finiteNumber(value: number, source: string): ConvertResult<number> {
  if (Number.isFinite(value)) return ok(value);
  return err(`toNumber: ${source} is not a finite number`);
}

function textIsTruthy(value: string): boolean {
  const textRepresentsFalse = value === "" || value === "false" || value === "0";
  return !textRepresentsFalse;
}

function numberIsTruthy(value: number): boolean {
  const numberRepresentsFalse = Number.isNaN(value) || value === 0;
  return !numberRepresentsFalse;
}
