import type { Shape } from "../types/index";
import { assertNever } from "../shared/assert-never";
import { toJavaScriptString } from "../shared/javascript-string";
import { isRecord } from "../shared/record";

export function isMissing(value: unknown): value is null | undefined {
  return value === null || value === undefined;
}

export function shaped(value: unknown, shape: Shape): unknown {
  if (isMissing(value)) return shape.kind === "nullable" ? null : value;
  return convert(value, shape);
}

function convert(value: unknown, shape: Shape): unknown {
  switch (shape.kind) {
    case "string":   return asText(value) ?? value;
    case "number":   return asNumber(value) ?? value;
    case "boolean":  return asBoolean(value) ?? value;
    case "date":     return asDate(value) ?? value;
    case "instant":  return asInstant(value) ?? value;
    case "array":    return asArrayOf(value, shape.item);
    case "object":   return asObjectOf(value, shape);
    case "nullable": return asNullable(value, shape.inner);
    case "any":      return value;
    default:         return assertNever(shape, "shape");
  }
}

export function bound(value: unknown, shape: Shape): unknown {
  const converted = shaped(value, shape);
  const nothingBound = isMissing(converted) || isBlankText(converted);
  switch (shape.kind) {
    case "string":
    case "any":     return nothingBound ? null : converted;
    case "number":  return nothingBound ? 0 : converted;
    case "boolean": return nothingBound ? false : converted;
    case "date":    return nothingBound ? earliestDate() : converted;
    case "instant": return nothingBound ? new Instant(EARLIEST_INSTANT) : converted;
    case "array":
    case "object":
    case "nullable": return converted;
    default:        return assertNever(shape, "shape");
  }
}

const EARLIEST_INSTANT = -62_135_596_800_000;

function earliestDate(): Date {
  const date = new Date(2000, 0, 1);
  date.setFullYear(1, 0, 1);
  return date;
}

export function asNumber(value: unknown): number | undefined {
  if (typeof value === "number") return Number.isFinite(value) ? value : undefined;
  if (typeof value === "boolean") return value ? 1 : 0;
  if (typeof value === "string") {
    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : undefined;
  }
  if (value instanceof Date) return Number.isFinite(value.getTime()) ? value.getTime() : undefined;
  return undefined;
}

export function asBoolean(value: unknown): boolean | undefined {
  if (typeof value === "boolean") return value;
  if (typeof value === "string") return !(value === "" || value.toLowerCase() === "false" || value === "0");
  if (typeof value === "number") return !(value === 0 || Number.isNaN(value));
  if (value instanceof Date) return true;
  if (Array.isArray(value)) return value.length > 0;
  return undefined;
}

export function asText(value: unknown): string | undefined {
  if (typeof value === "string") return value;
  if (typeof value === "number" || typeof value === "boolean") return `${value}`;
  if (value instanceof Date && !Number.isFinite(value.getTime())) return undefined;
  if (value instanceof Instant) return value.toISOString();
  if (value instanceof Date) return localDateText(value);
  if (Array.isArray(value)) return toJavaScriptString(value);
  return undefined;
}

export function asDate(value: unknown): Date | undefined {
  if (value instanceof Date) return Number.isFinite(value.getTime()) ? value : undefined;
  if (typeof value === "number") return Number.isFinite(value) ? new Date(value) : undefined;
  if (typeof value !== "string") return undefined;

  const dateOnly = /^(\d{4})-(\d{2})-(\d{2})$/.exec(value);
  if (dateOnly) return localMidnight(Number(dateOnly[1]), Number(dateOnly[2]), Number(dateOnly[3]));

  const parsed = new Date(value);
  return Number.isFinite(parsed.getTime()) ? parsed : undefined;
}

export class Instant extends Date {
  override toJSON(): string {
    return this.toISOString();
  }
}

export function asInstant(value: unknown): Instant | undefined {
  if (value instanceof Instant) return value;
  const date = asDate(value);
  return date === undefined ? undefined : new Instant(date.getTime());
}

function localDateText(date: Date): string {
  const day = `${`${date.getFullYear()}`.padStart(4, "0")}-${twoDigits(date.getMonth() + 1)}-${twoDigits(date.getDate())}`;
  const isMidnight =
    date.getHours() === 0 && date.getMinutes() === 0 && date.getSeconds() === 0 && date.getMilliseconds() === 0;
  if (isMidnight) return day;

  const time = `${twoDigits(date.getHours())}:${twoDigits(date.getMinutes())}:${twoDigits(date.getSeconds())}`;
  const milliseconds = date.getMilliseconds() === 0 ? "" : `.${`${date.getMilliseconds()}`.padStart(3, "0")}`;
  return `${day}T${time}${milliseconds}`;
}

function twoDigits(value: number): string {
  return `${value}`.padStart(2, "0");
}

function localMidnight(year: number, month: number, day: number): Date | undefined {
  const date = new Date(2000, 0, 1);
  date.setFullYear(year, month - 1, day);
  const isTheSameCalendarDay =
    date.getFullYear() === year && date.getMonth() === month - 1 && date.getDate() === day;
  return isTheSameCalendarDay ? date : undefined;
}

function asArrayOf(value: unknown, item: Shape): unknown {
  if (Array.isArray(value)) return value.map(element => shaped(element, item));
  if (value === "") return [];
  const objectCannotBecomeAnArray = typeof value === "object" && !(value instanceof Date);
  if (objectCannotBecomeAnArray) return value;
  return [shaped(value, item)];
}

function asObjectOf(value: unknown, shape: Extract<Shape, { kind: "object" }>): unknown {
  if (!isRecord(value) || value instanceof Date) return value;

  const record = value;
  let converted: Record<string, unknown> | undefined;
  for (const [name, fieldShape] of Object.entries(shape.fields)) {
    if (!Object.prototype.hasOwnProperty.call(record, name)) continue;
    const member = shaped(record[name], fieldShape);
    if (member === record[name]) continue;
    converted ??= { ...record };
    converted[name] = member;
  }
  return converted ?? record;
}

function asNullable(value: unknown, inner: Shape): unknown {
  if (isBlankText(value)) return null;
  return convert(value, inner);
}

const dotNetWhitespaceOnly = /^[\t\n\v\f\r \u0085\u00A0\u1680\u2000-\u200A\u2028\u2029\u202F\u205F\u3000]*$/;

export function isBlankText(value: unknown): value is string {
  return typeof value === "string" && dotNetWhitespaceOnly.test(value);
}
