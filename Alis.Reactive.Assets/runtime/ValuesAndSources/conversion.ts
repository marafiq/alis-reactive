import { assertNever, type Culture, type DateMeaning, type FieldConversion, type FieldNothing, type NumberOf, type NumberOverflow, type NumberShape, type Shape, type StringShape, type TimeZone } from "../Schema/index";
import { nothing, value, type Read } from "./read";

interface WallTime {
  readonly year: number;
  readonly month: number;
  readonly day: number;
  readonly hour: number;
  readonly minute: number;
  readonly second: number;
  readonly millisecond: number;
}

type Offset = { readonly kind: "none" } | { readonly kind: "east"; readonly minutes: number };

interface DateText {
  readonly wall: WallTime;
  readonly offset: Offset;
}

interface NumberStyle {
  readonly decimal: string;
  readonly group: string;
  readonly decimalPoint: boolean;
  readonly thousands: boolean;
  readonly exponent: boolean;
  readonly whole: boolean;
}

const invariantFloat: NumberStyle = { decimal: ".", group: ",", decimalPoint: true, thousands: false, exponent: true, whole: false };

const earliestInstant = -62_135_596_800_000;

const latestInstant = 253_402_300_799_999;

const latestOffsetMinutes = 14 * 60;

const dayMilliseconds = 86_400_000;

export function converted(read: Read, shape: Shape, culture: Culture, timeZone: TimeZone): Read {
  if (read.kind === "nothing") return nothing;
  const held = read.value;
  switch (shape.kind) {
    case "string":
      return asText(held, shape, culture, timeZone);
    case "number":
      return asNumber(held, shape, culture);
    case "boolean":
      return asBoolean(held);
    case "date":
      return asDate(held);
    case "instant":
      return asInstant(held, shape.from, timeZone);
    case "file":
      return held instanceof File ? read : nothing;
    case "any":
      return read;
    case "array":
      return isList(held) ? asList(held, (item) => converted(item, shape.item, culture, timeZone)) : nothing;
    case "nullable":
      if (held === null) return read;
      return isBlank(held) ? nothing : converted(read, shape.inner, culture, timeZone);
    case "object":
      return isRecord(held) ? asRecord(held, Object.entries(shape.fields), (field, fieldShape) => converted(field, fieldShape, culture, timeZone)) : nothing;
    default:
      return assertNever(shape);
  }
}

function asText(held: unknown, shape: StringShape, culture: Culture, timeZone: TimeZone): Read {
  const text = textOf(held, shape, culture, timeZone);
  return text === undefined ? nothing : value(text);
}

function textOf(held: unknown, shape: StringShape, culture: Culture, timeZone: TimeZone): string | undefined {
  if (typeof held === "string") return held;
  if (typeof held === "number") return Number.isFinite(held) ? numberText(held, shape.negativeZero, culture) : undefined;
  if (typeof held === "boolean") return held ? "true" : "false";
  if (isList(held)) return listText(held, shape, culture, timeZone);
  if (!(held instanceof Date) || !isMoment(held)) return undefined;
  switch (shape.date) {
    case "none":
      return undefined;
    case "date":
      return shownWall(localWall(held), false);
    case "instant":
      return shownInstant(held, timeZone);
    default:
      return assertNever(shape.date);
  }
}

function listText(items: readonly unknown[], shape: StringShape, culture: Culture, timeZone: TimeZone): string {
  const texts: string[] = [];
  for (const item of items) {
    const text = textOf(item, shape, culture, timeZone);
    texts.push(text ?? "");
  }
  return texts.join(",");
}

function numberText(held: number, negativeZero: string, culture: Culture): string {
  if (Object.is(held, -0)) return negativeZero;
  const written = held.toExponential();
  const at = written.indexOf("e");
  const exponent = Number(written.slice(at + 1));
  if (exponent < -4 || exponent >= 17) {
    const sign = exponent < 0 ? "-" : "+";
    return separated(written.slice(0, at), culture.decimal) + "E" + sign + String(Math.abs(exponent)).padStart(2, "0");
  }
  return separated(String(held), culture.decimal);
}

function separated(digits: string, decimal: string): string {
  return digits.split(".").join(decimal);
}

function asNumber(held: unknown, shape: NumberShape, culture: Culture): Read {
  if (typeof held === "number") return Number.isFinite(held) ? value(held) : nothing;
  if (typeof held === "boolean") return value(held ? 1 : 0);
  if (held instanceof Date) return isMoment(held) ? value(held.getTime()) : nothing;
  if (typeof held !== "string" || isBlank(held)) return nothing;
  return numberOf(held, styleOf(shape, culture), shape.overflow);
}

function styleOf(shape: NumberShape, culture: Culture): NumberStyle {
  switch (shape.text) {
    case "view":
      return viewStyle(shape.of, culture);
    case "invariant":
      return invariantFloat;
    default:
      return assertNever(shape.text);
  }
}

function viewStyle(of: NumberOf, culture: Culture): NumberStyle {
  switch (of) {
    case "integer":
      return { decimal: culture.decimal, group: culture.group, decimalPoint: false, thousands: culture.integer.group, exponent: culture.integer.exponent, whole: true };
    case "fraction":
      return { decimal: culture.decimal, group: culture.group, decimalPoint: true, thousands: culture.fraction.group, exponent: culture.fraction.exponent, whole: false };
    default:
      return assertNever(of);
  }
}

function numberOf(text: string, style: NumberStyle, overflow: NumberOverflow): Read {
  let at = pastNumberWhite(text, 0);
  let written = "";
  if (text.startsWith("+", at)) at += 1;
  else if (text.startsWith("-", at)) {
    written = "-";
    at += 1;
  }
  let digits = false;
  let decimal = false;
  while (at < text.length) {
    if (isDigit(text.charCodeAt(at))) {
      written += text.charAt(at);
      digits = true;
      at += 1;
    } else if (style.decimalPoint && !decimal && startsWithSeparator(text, at, style.decimal)) {
      written += ".";
      decimal = true;
      at += style.decimal.length;
    } else if (style.thousands && digits && !decimal && startsWithSeparator(text, at, style.group)) {
      at += style.group.length;
    } else break;
  }
  if (!digits) return nothing;
  if (style.exponent && (text.startsWith("e", at) || text.startsWith("E", at))) {
    const power = exponentAt(text, at + 1);
    if (power !== undefined) {
      written += "e" + power.written;
      at = power.end;
    }
  }
  if (pastNumberWhite(text, at) !== text.length) return nothing;
  const read = Number(written);
  if (!Number.isFinite(read)) {
    switch (overflow) {
      case "nothing":
        return nothing;
      case "infinity":
        return value(read);
      default:
        return assertNever(overflow);
    }
  }
  if (!style.whole) return value(read);
  return Number.isInteger(read) ? value(read + 0) : nothing;
}

function exponentAt(text: string, from: number): { readonly written: string; readonly end: number } | undefined {
  let at = from;
  let written = "";
  if (text.startsWith("+", at)) at += 1;
  else if (text.startsWith("-", at)) {
    written = "-";
    at += 1;
  }
  const start = at;
  while (at < text.length && isDigit(text.charCodeAt(at))) at += 1;
  return at === start ? undefined : { written: written + text.slice(start, at), end: at };
}

function startsWithSeparator(text: string, at: number, separator: string): boolean {
  return separator.length > 0 && text.startsWith(separator, at);
}

function pastNumberWhite(text: string, from: number): number {
  let at = from;
  while (at < text.length && isNumberWhite(text.charCodeAt(at))) at += 1;
  return at;
}

function isNumberWhite(code: number): boolean {
  return (code >= 0x09 && code <= 0x0d) || code === 0x20;
}

function isDigit(code: number): boolean {
  return code >= 0x30 && code <= 0x39;
}

function asBoolean(held: unknown): Read {
  if (typeof held === "boolean") return value(held);
  if (typeof held !== "string") return nothing;
  const word = trimmed(held).toUpperCase();
  if (word === "TRUE") return value(true);
  if (word === "FALSE") return value(false);
  return nothing;
}

function isBlank(held: unknown): held is string {
  return typeof held === "string" && trimmed(held).length === 0;
}

function trimmed(text: string): string {
  let start = 0;
  let end = text.length;
  while (start < end && isDotNetWhiteSpace(text.charCodeAt(start))) start += 1;
  while (end > start && isDotNetWhiteSpace(text.charCodeAt(end - 1))) end -= 1;
  return text.slice(start, end);
}

function isDotNetWhiteSpace(code: number): boolean {
  return (
    (code >= 0x09 && code <= 0x0d) ||
    code === 0x20 ||
    code === 0x85 ||
    code === 0xa0 ||
    code === 0x1680 ||
    (code >= 0x2000 && code <= 0x200a) ||
    code === 0x2028 ||
    code === 0x2029 ||
    code === 0x202f ||
    code === 0x205f ||
    code === 0x3000
  );
}

function asDate(held: unknown): Read {
  if (held instanceof Date) return isMoment(held) ? value(held) : nothing;
  const text = typeof held === "string" ? dateText(held) : undefined;
  return text === undefined ? nothing : value(localDate(text.wall));
}

function asInstant(held: unknown, from: DateMeaning, timeZone: TimeZone): Read {
  if (held instanceof Date) return isMoment(held) ? instantOfDate(held, from, timeZone) : nothing;
  const text = typeof held === "string" ? dateText(held) : undefined;
  if (text === undefined) return nothing;
  const moment = momentOf(text, timeZone);
  if (moment < earliestInstant) return nothing;
  if (moment > latestInstant) return nothing;
  return value(new Date(moment));
}

function instantOfDate(held: Date, from: DateMeaning, timeZone: TimeZone): Read {
  switch (from) {
    case "none":
      return nothing;
    case "instant":
      return value(held);
    case "date": {
      const moment = zonedMoment(localWall(held), timeZone);
      return moment < earliestInstant || moment > latestInstant ? nothing : value(new Date(moment));
    }
    default:
      return assertNever(from);
  }
}

function localWall(held: Date): WallTime {
  return {
    year: held.getFullYear(),
    month: held.getMonth() + 1,
    day: held.getDate(),
    hour: held.getHours(),
    minute: held.getMinutes(),
    second: held.getSeconds(),
    millisecond: held.getMilliseconds(),
  };
}

function shownWall(wall: WallTime, withTime: boolean): string {
  const day = String(wall.year).padStart(4, "0") + "-" + twoDigits(wall.month) + "-" + twoDigits(wall.day);
  if (!withTime && wall.hour === 0 && wall.minute === 0 && wall.second === 0 && wall.millisecond === 0) return day;
  const time = "T" + twoDigits(wall.hour) + ":" + twoDigits(wall.minute) + ":" + twoDigits(wall.second);
  const fraction = wall.millisecond === 0 ? "" : "." + String(wall.millisecond).padStart(3, "0");
  return day + time + fraction;
}

function shownInstant(held: Date, timeZone: TimeZone): string {
  const offset = shownOffset(held, timeZone);
  const wall = new Date(held.getTime() + offset);
  return shownWall({
    year: wall.getUTCFullYear(),
    month: wall.getUTCMonth() + 1,
    day: wall.getUTCDate(),
    hour: wall.getUTCHours(),
    minute: wall.getUTCMinutes(),
    second: wall.getUTCSeconds(),
    millisecond: wall.getUTCMilliseconds(),
  }, true) + offsetText(offset);
}

function shownOffset(held: Date, timeZone: TimeZone): number {
  switch (timeZone.kind) {
    case "clock":
      return -held.getTimezoneOffset() * 60_000;
    case "zone":
      return offsetIn(timeZone.id, held.getTime());
    default:
      return assertNever(timeZone);
  }
}

function offsetText(offset: number): string {
  if (offset === 0) return "Z";
  const minutes = Math.abs(offset) / 60_000;
  return (offset < 0 ? "-" : "+") + twoDigits(Math.floor(minutes / 60)) + ":" + twoDigits(minutes % 60);
}

function twoDigits(value: number): string {
  return String(value).padStart(2, "0");
}

function momentOf(text: DateText, timeZone: TimeZone): number {
  switch (text.offset.kind) {
    case "east":
      return utcOf(text.wall) - text.offset.minutes * 60_000;
    case "none":
      return zonedMoment(text.wall, timeZone);
    default:
      return assertNever(text.offset);
  }
}

function zonedMoment(wall: WallTime, timeZone: TimeZone): number {
  switch (timeZone.kind) {
    case "clock":
      return localDate(wall).getTime();
    case "zone": {
      const asIfUtc = utcOf(wall);
      return asIfUtc - wallOffset(timeZone.id, asIfUtc);
    }
    default:
      return assertNever(timeZone);
  }
}

function wallOffset(zone: string, asIfUtc: number): number {
  const before = offsetIn(zone, asIfUtc - dayMilliseconds);
  const after = offsetIn(zone, asIfUtc + dayMilliseconds);
  if (before === after) return before;
  const beforeHolds = offsetIn(zone, asIfUtc - before) === before;
  const afterHolds = offsetIn(zone, asIfUtc - after) === after;
  if (beforeHolds !== afterHolds) return beforeHolds ? before : after;
  return Math.min(before, after);
}

function offsetIn(zone: string, moment: number): number {
  const parts = new Intl.DateTimeFormat("en-US", {
    timeZone: zone,
    hourCycle: "h23",
    era: "short",
    year: "numeric",
    month: "numeric",
    day: "numeric",
    hour: "numeric",
    minute: "numeric",
    second: "numeric",
  }).formatToParts(moment);
  const year = Number(partOf(parts, "year"));
  const wall: WallTime = {
    year: partOf(parts, "era") === "BC" ? 1 - year : year,
    month: Number(partOf(parts, "month")),
    day: Number(partOf(parts, "day")),
    hour: Number(partOf(parts, "hour")),
    minute: Number(partOf(parts, "minute")),
    second: Number(partOf(parts, "second")),
    millisecond: 0,
  };
  const millisecond = ((moment % 1000) + 1000) % 1000;
  return utcOf(wall) - (moment - millisecond);
}

function partOf(parts: readonly Intl.DateTimeFormatPart[], type: Intl.DateTimeFormatPartTypes): string {
  const part = parts.find((each) => each.type === type);
  if (part === undefined) throw new Error("The browser's Intl wrote no " + type + " of a moment in a time zone.");
  return part.value;
}

function dateText(text: string): DateText | undefined {
  const year = digitsAt(text, 0, 4);
  const month = digitsAt(text, 5, 2);
  const day = digitsAt(text, 8, 2);
  if (year === undefined || month === undefined || day === undefined || text[4] !== "-" || text[7] !== "-") return undefined;
  if (!isCalendarDay(year, month, day)) return undefined;
  if (text.length === 10) return { wall: { year, month, day, hour: 0, minute: 0, second: 0, millisecond: 0 }, offset: { kind: "none" } };
  const hour = digitsAt(text, 11, 2);
  const minute = digitsAt(text, 14, 2);
  if (text[10] !== "T" || hour === undefined || minute === undefined || text[13] !== ":" || hour > 23 || minute > 59) return undefined;
  let at = 16;
  let second = 0;
  let millisecond = 0;
  if (text[at] === ":") {
    const seconds = digitsAt(text, at + 1, 2);
    if (seconds === undefined || seconds > 59) return undefined;
    second = seconds;
    at += 3;
    if (text[at] === ".") {
      const places = fractionPlaces(text, at + 1);
      if (places === 0) return undefined;
      millisecond = Number(text.slice(at + 1, at + 1 + Math.min(places, 3)).padEnd(3, "0"));
      at += 1 + places;
    }
  }
  const offset = offsetAt(text, at);
  return offset === undefined ? undefined : { wall: { year, month, day, hour, minute, second, millisecond }, offset };
}

function offsetAt(text: string, at: number): Offset | undefined {
  if (at === text.length) return { kind: "none" };
  if (text[at] === "Z" && at + 1 === text.length) return { kind: "east", minutes: 0 };
  const sign = text[at] === "+" ? 1 : text[at] === "-" ? -1 : 0;
  const hours = digitsAt(text, at + 1, 2);
  const minutes = digitsAt(text, at + 4, 2);
  if (sign === 0 || hours === undefined || minutes === undefined || text[at + 3] !== ":" || at + 6 !== text.length) return undefined;
  const east = hours * 60 + minutes;
  if (minutes > 59 || east > latestOffsetMinutes) return undefined;
  return { kind: "east", minutes: sign * east };
}

function fractionPlaces(text: string, from: number): number {
  let places = 0;
  while (places < 7 && isDigit(text.charCodeAt(from + places))) places += 1;
  return places;
}

function digitsAt(text: string, at: number, count: number): number | undefined {
  if (at + count > text.length) return undefined;
  let read = 0;
  for (let position = at; position < at + count; position += 1) {
    const code = text.charCodeAt(position);
    if (!isDigit(code)) return undefined;
    read = read * 10 + (code - 0x30);
  }
  return read;
}

function isCalendarDay(year: number, month: number, day: number): boolean {
  return year >= 1 && month >= 1 && month <= 12 && day >= 1 && day <= daysIn(year, month);
}

function daysIn(year: number, month: number): number {
  if (month === 2) return isLeap(year) ? 29 : 28;
  return month === 4 || month === 6 || month === 9 || month === 11 ? 30 : 31;
}

function isLeap(year: number): boolean {
  return year % 4 === 0 && (year % 100 !== 0 || year % 400 === 0);
}

function localDate(wall: WallTime): Date {
  const local = new Date(2000, 0, 1, wall.hour, wall.minute, wall.second, wall.millisecond);
  local.setFullYear(wall.year, wall.month - 1, wall.day);
  return local;
}

function utcOf(wall: WallTime): number {
  const moment = new Date(0);
  moment.setUTCFullYear(wall.year, wall.month - 1, wall.day);
  return moment.setUTCHours(wall.hour, wall.minute, wall.second, wall.millisecond);
}

function isMoment(held: Date): boolean {
  return !Number.isNaN(held.getTime());
}

function asList(items: readonly unknown[], item: (read: Read) => Read): Read {
  const reads = Array.from(items, (each) => item(value(each)));
  if (reads.every((read, at) => keeps(items[at], read))) return value(items);
  return value(reads.map((read) => (read.kind === "value" ? read.value : undefined)));
}

function asRecord(held: object, fields: readonly (readonly [string, FieldConversion])[], field: (read: Read, shape: Shape) => Read): Read {
  const changed: [string, Read, FieldNothing][] = [];
  for (const [name, conversion] of fields) {
    const member: unknown = Reflect.get(held, name);
    const read = field(value(member), conversion.shape);
    if (!keeps(member, read)) changed.push([name, read, conversion.nothing]);
  }
  if (changed.length === 0) return value(held);
  if (!isPlainObject(held)) return nothing;
  const copy: Record<string, unknown> = { ...held };
  for (const [name, read, absent] of changed) {
    if (read.kind === "value") copy[name] = read.value;
    else {
      switch (absent) {
        case "omit":
          Reflect.deleteProperty(copy, name);
          break;
        case "null":
          copy[name] = null;
          break;
        default:
          return assertNever(absent);
      }
    }
  }
  return value(copy);
}

function keeps(held: unknown, read: Read): boolean {
  return read.kind === "nothing" ? held === undefined : Object.is(read.value, held);
}

function isList(held: unknown): held is readonly unknown[] {
  return Array.isArray(held);
}

function isRecord(held: unknown): held is object {
  return typeof held === "object" && held !== null && !Array.isArray(held);
}

function isPlainObject(held: object): boolean {
  const prototype: unknown = Object.getPrototypeOf(held);
  return prototype === Object.prototype || prototype === null;
}
