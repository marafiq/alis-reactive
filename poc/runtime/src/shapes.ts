import type { Shape } from "./plan";
import { assertNever, heldOrNull, nothing, value, type Read } from "./read";

interface DateText {
  readonly year: number;
  readonly month: number;
  readonly day: number;
  readonly hour: number;
  readonly minute: number;
  readonly second: number;
  readonly millisecond: number;
  readonly offset: Offset;
}

type Offset = { readonly kind: "none" } | { readonly kind: "minutes"; readonly east: number };

const noOffset: Offset = { kind: "none" };

export function converted(read: Read, shape: Shape): Read {
  if (read.kind === "nothing" || read.value === null) return nothing;
  const held = read.value;
  switch (shape.kind) {
    case "date":
    case "instant":
      return asDate(held);
    case "nullable":
      return typeof held === "string" && held.trim().length === 0 ? nothing : converted(read, shape.inner);
    case "array":
      return Array.isArray(held) ? value(held.map((item: unknown) => heldOrNull(converted(value(item), shape.item)))) : nothing;
    case "object":
      return isPlainObject(held) ? value(withFields(held, shape.fields)) : nothing;
    default:
      return assertNever(shape);
  }
}

function asDate(held: unknown): Read {
  if (held instanceof Date) return Number.isNaN(held.getTime()) ? nothing : value(held);
  if (typeof held !== "string") return nothing;
  const text = dateText(held);
  return text === undefined ? nothing : value(dateOf(text));
}

function withFields(held: Record<string, unknown>, fields: { readonly [name: string]: Shape }): Record<string, unknown> {
  const copy: Record<string, unknown> = { ...held };
  for (const [name, shape] of Object.entries(fields)) {
    if (name in held) copy[name] = heldOrNull(converted(value(held[name]), shape));
  }
  return copy;
}

function isPlainObject(held: unknown): held is Record<string, unknown> {
  if (typeof held !== "object" || held === null || Array.isArray(held)) return false;
  const prototype: unknown = Object.getPrototypeOf(held);
  return prototype === Object.prototype || prototype === null;
}

function dateOf(text: DateText): Date {
  if (text.offset.kind === "none") {
    const local = new Date(2000, 0, 1, text.hour, text.minute, text.second, text.millisecond);
    local.setFullYear(text.year, text.month - 1, text.day);
    return local;
  }
  const moment = new Date(0);
  moment.setUTCFullYear(text.year, text.month - 1, text.day);
  moment.setUTCHours(text.hour, text.minute, text.second, text.millisecond);
  return new Date(moment.getTime() - text.offset.east * 60_000);
}

function dateText(text: string): DateText | undefined {
  const year = digits(text, 0, 4);
  const month = digits(text, 5, 2);
  const day = digits(text, 8, 2);
  if (year === undefined || month === undefined || day === undefined || text[4] !== "-" || text[7] !== "-") return undefined;
  if (!isCalendarDay(year, month, day)) return undefined;
  if (text.length === 10) return { year, month, day, hour: 0, minute: 0, second: 0, millisecond: 0, offset: noOffset };
  if (text[10] !== "T") return undefined;
  const hour = digits(text, 11, 2);
  const minute = digits(text, 14, 2);
  if (hour === undefined || minute === undefined || text[13] !== ":" || hour > 23 || minute > 59) return undefined;
  let at = 16;
  let second = 0;
  let millisecond = 0;
  if (text[at] === ":") {
    const read = digits(text, at + 1, 2);
    if (read === undefined || read > 59) return undefined;
    second = read;
    at += 3;
    if (text[at] === ".") {
      const fraction = fractionDigits(text, at + 1);
      if (fraction === undefined) return undefined;
      millisecond = fraction.millisecond;
      at += 1 + fraction.length;
    }
  }
  const offset = offsetAt(text, at);
  return offset === undefined ? undefined : { year, month, day, hour, minute, second, millisecond, offset };
}

function offsetAt(text: string, at: number): Offset | undefined {
  if (at === text.length) return noOffset;
  if (text[at] === "Z" && at + 1 === text.length) return { kind: "minutes", east: 0 };
  const sign = text[at] === "+" ? 1 : text[at] === "-" ? -1 : 0;
  const hours = digits(text, at + 1, 2);
  const minutes = digits(text, at + 4, 2);
  if (sign === 0 || hours === undefined || minutes === undefined || text[at + 3] !== ":" || at + 6 !== text.length) return undefined;
  if (hours > 14 || minutes > 59) return undefined;
  return { kind: "minutes", east: sign * (hours * 60 + minutes) };
}

function fractionDigits(text: string, at: number): { readonly millisecond: number; readonly length: number } | undefined {
  let length = 0;
  while (length < 7 && isDigit(text.charCodeAt(at + length))) length++;
  if (length === 0) return undefined;
  const milliseconds = text.slice(at, at + Math.min(length, 3)).padEnd(3, "0");
  return { millisecond: Number(milliseconds), length };
}

function digits(text: string, at: number, count: number): number | undefined {
  if (at + count > text.length) return undefined;
  let read = 0;
  for (let position = at; position < at + count; position++) {
    const code = text.charCodeAt(position);
    if (!isDigit(code)) return undefined;
    read = read * 10 + (code - 48);
  }
  return read;
}

function isDigit(code: number): boolean {
  return code >= 48 && code <= 57;
}

function isCalendarDay(year: number, month: number, day: number): boolean {
  if (year < 1 || month < 1 || month > 12 || day < 1) return false;
  const probe = new Date(0);
  probe.setUTCFullYear(year, month - 1, day);
  return probe.getUTCFullYear() === year && probe.getUTCMonth() === month - 1 && probe.getUTCDate() === day;
}
