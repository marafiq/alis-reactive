// Shape conversion is centralized here so conditions, validation, gather, and
// execution all coerce values through the same runtime boundary.

import type { Shape } from "../types/index";
import { toJavaScriptString } from "./javascript-string";

export type ShapeConversionResult<T> = { ok: true; value: T } | { ok: false; error: string };

function ok<T>(value: T): ShapeConversionResult<T> { return { ok: true, value }; }
function err<T>(error: string): ShapeConversionResult<T> { return { ok: false, error }; }
function isMissingInput(value: unknown): boolean { return value === null || value === undefined; }

/** Best-effort shape application for runtime reads; failed conversions leave the original value intact. */
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

function applyScalar<T>(value: unknown, convert: (v: unknown) => ShapeConversionResult<T>): unknown {
  const conversion = convert(value);
  if (conversion.ok) return conversion.value;
  return value;
}

function applyArrayShape(value: unknown, shape: Extract<Shape, { kind: "array" }>): unknown {
  const conversion = toArray(value);
  if (!conversion.ok) return value;
  return applyArrayItemShape(conversion.value, shape);
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

  return applyObjectFields(record.value, shape);
}

/** Strict conversion for validation comparisons; callers receive conversion errors instead of fallback values. */
export function convertByShape(value: unknown, shape: Shape): ShapeConversionResult<unknown> {
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

function convertArrayShape(value: unknown, shape: Extract<Shape, { kind: "array" }>): ShapeConversionResult<unknown> {
  const conversion = toArray(value);
  if (!conversion.ok) return conversion;
  return ok(applyArrayItemShape(conversion.value, shape));
}

function convertObjectShape(value: unknown, shape: Extract<Shape, { kind: "object" }>): ShapeConversionResult<unknown> {
  const record = toPlainObject(value);
  if (!record.ok) return record;
  return ok(applyObjectFields(record.value, shape));
}

function convertNullableShape(value: unknown, inner: Shape): ShapeConversionResult<unknown> {
  if (isMissingInput(value)) return ok(null);
  return convertByShape(value, inner);
}

function applyObjectFields(
  input: Record<string, unknown>,
  shape: Extract<Shape, { kind: "object" }>,
): Record<string, unknown> {
  const keepsInputAsDeclared = shape.additional && Object.keys(shape.fields).length === 0;
  if (keepsInputAsDeclared) return input;

  const output: Record<string, unknown> = {};
  if (shape.additional) Object.assign(output, input);
  for (const [field, fieldShape] of Object.entries(shape.fields)) {
    if (!Object.prototype.hasOwnProperty.call(input, field)) continue;
    output[field] = applyShape(input[field], fieldShape);
  }
  return output;
}

export function toString(value: unknown): ShapeConversionResult<string> {
  if (isMissingInput(value)) return ok("");
  if (typeof value === "string") return ok(value);
  if (typeof value === "number" || typeof value === "boolean") return ok(`${value}`);
  if (value instanceof Date) return ok(value.toISOString());
  if (Array.isArray(value)) return ok(toJavaScriptString(value));
  return err(`toString: received object — missing shape or wrong member. Got: ${JSON.stringify(value)}`);
}

export function toNumber(value: unknown): ShapeConversionResult<number> {
  if (isMissingInput(value)) return ok(0);
  if (typeof value === "number") return finiteNumber(value, "number");
  if (typeof value === "boolean") return ok(booleanAsNumber(value));
  if (typeof value === "string") {
    return numberFromText(value);
  }
  if (value instanceof Date) return finiteNumber(value.getTime(), "date");
  return err(`toNumber: cannot convert ${typeof value} to number`);
}

export function toBoolean(value: unknown): ShapeConversionResult<boolean> {
  if (isMissingInput(value)) return ok(false);
  if (typeof value === "boolean") return ok(value);
  if (typeof value === "string") return ok(textIsTruthy(value));
  if (typeof value === "number") return ok(numberIsTruthy(value));
  if (value instanceof Date) return ok(true);
  if (Array.isArray(value)) return ok(value.length > 0);
  return err(`toBoolean: received object — cannot convert`);
}

export function toDate(value: unknown): ShapeConversionResult<number> {
  if (isMissingInput(value)) return ok(NaN);
  if (value instanceof Date) return finiteNumber(value.getTime(), "Date object");
  if (typeof value === "number") return finiteNumber(value, "date timestamp");
  if (typeof value === "string") {
    // Date-only "YYYY-MM-DD" parses as local midnight, not UTC.
    const textIsDateOnly = /^\d{4}-\d{2}-\d{2}$/.test(value);
    if (textIsDateOnly) {
      const dateOnly = parseDateOnlyText(value);
      if (dateOnly === undefined) return err(`toDate: invalid date-only text "${value}"`);
      return ok(localMidnightTimestamp(dateOnly));
    }
    return finiteNumber(new Date(value).getTime(), `date text "${value}"`);
  }
  return err(`toDate: received ${typeof value} — not a date or timestamp`);
}

type DateOnlyParts = {
  readonly year: number;
  readonly month: number;
  readonly day: number;
};

function parseDateOnlyText(value: string): DateOnlyParts | undefined {
  const [year, month, day] = value.split("-").map(Number);
  if (year === undefined || month === undefined || day === undefined) return undefined;
  const parsed = new Date(year, month - 1, day);
  const parsedDateMatchesInput =
    parsed.getFullYear() === year
    && parsed.getMonth() === month - 1
    && parsed.getDate() === day;
  if (!parsedDateMatchesInput) return undefined;

  return { year, month, day };
}

function localMidnightTimestamp(value: DateOnlyParts): number {
  return new Date(value.year, value.month - 1, value.day).getTime();
}

export function toArray(value: unknown): ShapeConversionResult<unknown[]> {
  if (Array.isArray(value)) return ok(value);
  const missingOrEmptyTextRepresentsEmptyArray = isMissingInput(value) || value === "";
  if (missingOrEmptyTextRepresentsEmptyArray) return ok([]);

  const objectCannotBecomeArray = typeof value === "object" && !(value instanceof Date);
  if (objectCannotBecomeArray) {
    return err(`toArray: received object — expected array or scalar`);
  }
  return ok([value]);
}

export function toPlainObject(value: unknown): ShapeConversionResult<Record<string, unknown>> {
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

function numberFromText(value: string): ShapeConversionResult<number> {
  const parsed = Number(value);
  return finiteNumber(parsed, `text "${value}"`);
}

function finiteNumber(value: number, source: string): ShapeConversionResult<number> {
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
