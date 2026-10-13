// A value changes type in exactly two places, and only by its declared shape: where it is read
// (a get or call result) and where it is written (a set value, a call argument). These are the
// conversions (grammar.md section 2.1). A missing value passes through, except into a nullable
// shape; a value that cannot be converted is kept as it is.

import type { Shape } from "../types/index";
import { assertNever } from "../shared/assert-never";
import { toJavaScriptString } from "../shared/javascript-string";

export function isMissing(value: unknown): value is null | undefined {
  return value === null || value === undefined;
}

/** Converts a value read or written under a shape. Missing values pass through. */
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
    case "any":
    case "none":     return value;
    default:         return assertNever(shape, "shape");
  }
}

/**
 * A value as MVC binds it into the model the server validates (grammar.md 2.1, binding reads): text
 * that is empty or only whitespace is null, as ConvertEmptyStringToNull makes it; a non-nullable
 * number, boolean or date left empty is its type's default (0, false, 0001-01-01), the value the
 * property keeps when binding has nothing to bind. A nullable shape already reads blank as null.
 */
export function bound(value: unknown, shape: Shape): unknown {
  const converted = shaped(value, shape);
  const nothingBound = isMissing(converted) || isBlankText(converted);
  switch (shape.kind) {
    case "string":  return nothingBound ? null : converted;
    case "number":  return nothingBound ? 0 : converted;
    case "boolean": return nothingBound ? false : converted;
    case "date":    return nothingBound ? earliestDate() : converted;
    case "instant": return nothingBound ? new Instant(EARLIEST_INSTANT) : converted;
    default:        return converted;
  }
}

// DateTime.MinValue: 0001-01-01 at midnight, local for a date, UTC for an instant.
const EARLIEST_INSTANT = -62_135_596_800_000;

function earliestDate(): Date {
  return localMidnight(1, 1, 1)!;
}

/** Number shape: finite numbers; booleans as 1 or 0; text through Number (blank text is 0); dates as their time. */
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

/** Boolean shape: text is false only for "", "false" in any case and "0"; numbers for 0 and NaN. */
export function asBoolean(value: unknown): boolean | undefined {
  if (typeof value === "boolean") return value;
  if (typeof value === "string") return !(value === "" || value.toLowerCase() === "false" || value === "0");
  if (typeof value === "number") return !(value === 0 || Number.isNaN(value));
  if (value instanceof Date) return true;
  if (Array.isArray(value)) return value.length > 0;
  return undefined;
}

/**
 * Text shape: numbers and booleans as text, an instant as its UTC instant, other dates as local
 * calendar text, arrays as JavaScript's own text.
 */
export function asText(value: unknown): string | undefined {
  if (typeof value === "string") return value;
  if (typeof value === "number" || typeof value === "boolean") return `${value}`;
  if (value instanceof Date && !Number.isFinite(value.getTime())) return undefined;
  if (value instanceof Instant) return value.toISOString();
  if (value instanceof Date) return localDateText(value);
  if (Array.isArray(value)) return toJavaScriptString(value);
  return undefined;
}

/** Date shape: a Date; a number as a time; "YYYY-MM-DD" as local midnight; other text as Date parses it. */
export function asDate(value: unknown): Date | undefined {
  if (value instanceof Date) return Number.isFinite(value.getTime()) ? value : undefined;
  if (typeof value === "number") return Number.isFinite(value) ? new Date(value) : undefined;
  if (typeof value !== "string") return undefined;

  const dateOnly = /^(\d{4})-(\d{2})-(\d{2})$/.exec(value);
  if (dateOnly) return localMidnight(Number(dateOnly[1]), Number(dateOnly[2]), Number(dateOnly[3]));

  const parsed = new Date(value);
  return Number.isFinite(parsed.getTime()) ? parsed : undefined;
}

/**
 * A moment in time, read under the instant shape (a DateTimeOffset): a Date whose text is the exact
 * instant in UTC, so the server binds the same moment whatever the zone of the browser or the server.
 */
export class Instant extends Date {
  override toJSON(): string {
    return this.toISOString();
  }
}

/** Instant shape: what the date shape reads, as an Instant. */
export function asInstant(value: unknown): Instant | undefined {
  if (value instanceof Instant) return value;
  const date = asDate(value);
  return date === undefined ? undefined : new Instant(date.getTime());
}

// A date as the person picked it, with no offset, the way an HTML form posts one: "YYYY-MM-DD" at
// local midnight, else "YYYY-MM-DDTHH:mm:ss" with milliseconds when it has them. As a UTC instant a
// day picked east of UTC would read as the day before, and a DateOnly member could not bind it.
// asDate reads both forms back as the same local date.
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

// setFullYear, because the Date constructor reads years below 100 as 19xx.
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
  const isPlainRecord =
    typeof value === "object" && value !== null && !Array.isArray(value) && !(value instanceof Date);
  if (!isPlainRecord) return value;

  const record = value as Record<string, unknown>;
  const output: Record<string, unknown> = {};
  for (const [name, fieldShape] of Object.entries(shape.fields)) {
    if (Object.prototype.hasOwnProperty.call(record, name)) output[name] = shaped(record[name], fieldShape);
  }
  return output;
}

// A nullable shape is a CLR Nullable<T>. Blank text is no number, boolean or date: MVC binds it as
// null, so the browser does too. Text-represented values keep their blank text.
function asNullable(value: unknown, inner: Shape): unknown {
  if (isBlankText(value) && innerParsesText(inner)) return null;
  return convert(value, inner);
}

function innerParsesText(inner: Shape): boolean {
  return inner.kind === "number" || inner.kind === "boolean" || inner.kind === "date" || inner.kind === "instant";
}

// Whitespace as .NET's char.IsWhiteSpace defines it (string.IsNullOrWhiteSpace, which MVC binding
// and FluentValidation use); JavaScript's trim differs on U+FEFF and U+0085.
const dotNetWhitespaceOnly = /^[\t\n\v\f\r \u0085\u00A0\u1680\u2000-\u200A\u2028\u2029\u202F\u205F\u3000]*$/;

export function isBlankText(value: unknown): value is string {
  return typeof value === "string" && dotNetWhitespaceOnly.test(value);
}
