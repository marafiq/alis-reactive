import { assertNever, type Operator } from "../Schema/index";
import { nothing, Stop, value, type Read } from "../ValuesAndSources/index";

export type KeyOperator = Extract<Operator, "dateKey" | "wallKey" | "instantKey" | "dayKey" | "timeKey" | "spanKey">;

type Keyed = { readonly kind: "key"; readonly text: string } | { readonly kind: "noValue" } | { readonly kind: "outside" };

interface Day {
  readonly year: number;
  readonly month: number;
  readonly day: number;
}

interface Time {
  readonly hour: number;
  readonly minute: number;
  readonly second: number;
  readonly fraction: string;
}

const noValue: Keyed = { kind: "noValue" };

const outside: Keyed = { kind: "outside" };

const ticksPerSecond = 10_000_000n;

const ticksOffset = 2n ** 63n;

const latestTicks = 2n ** 63n - 1n;

const latestOffsetMinutes = 14 * 60;

export function keyOf(key: KeyOperator, held: Read, start: (() => Read) | undefined): Read {
  if (held.kind === "nothing") return nothing;
  if (held.value === null) return value(null);
  const keyed = keyedAs(key, held.value);
  switch (keyed.kind) {
    case "key":
      return value(keyed.text);
    case "noValue":
      return start === undefined ? nothing : keyOf(key, start(), undefined);
    case "outside":
      if (start !== undefined) return keyOf(key, start(), undefined);
      throw new Stop("The value " + described(held.value) + " is outside the forms the " + typeOf(key) + " is compared in.");
    default:
      return assertNever(keyed);
  }
}

function keyedAs(key: KeyOperator, held: unknown): Keyed {
  switch (key) {
    case "dateKey":
      return held instanceof Date ? localKey(held) : typeof held === "string" ? dateTextKey(held, false) : outside;
    case "wallKey":
      return typeof held === "string" ? dateTextKey(held, true) : outside;
    case "instantKey":
      return held instanceof Date ? utcKey(held) : typeof held === "string" ? instantTextKey(held) : outside;
    case "dayKey":
      return typeof held === "string" ? dayTextKey(held) : outside;
    case "timeKey":
      return typeof held === "string" ? timeTextKey(held) : outside;
    case "spanKey":
      return typeof held === "string" ? spanTextKey(held) : outside;
    default:
      return assertNever(key);
  }
}

function typeOf(key: KeyOperator): string {
  switch (key) {
    case "dateKey":
    case "wallKey":
      return "DateTime";
    case "instantKey":
      return "DateTimeOffset";
    case "dayKey":
      return "DateOnly";
    case "timeKey":
      return "TimeOnly";
    case "spanKey":
      return "TimeSpan";
    default:
      return assertNever(key);
  }
}

function described(held: unknown): string {
  if (held instanceof Date) return Number.isNaN(held.getTime()) ? "a Date of no time" : "the Date " + held.toISOString();
  if (typeof held === "string") return JSON.stringify(held);
  if (typeof held === "number" || typeof held === "boolean") return String(held);
  return typeof held === "object" ? "an object" : "a " + typeof held;
}

function localKey(held: Date): Keyed {
  if (Number.isNaN(held.getTime())) return outside;
  const day = { year: held.getFullYear(), month: held.getMonth() + 1, day: held.getDate() };
  const time = { hour: held.getHours(), minute: held.getMinutes(), second: held.getSeconds(), fraction: milliseconds(held.getMilliseconds()) };
  return day.year >= 1 && day.year <= 9999 ? { kind: "key", text: dayText(day) + "T" + timeText(time) } : outside;
}

function utcKey(held: Date): Keyed {
  if (Number.isNaN(held.getTime())) return outside;
  const day = { year: held.getUTCFullYear(), month: held.getUTCMonth() + 1, day: held.getUTCDate() };
  const time = { hour: held.getUTCHours(), minute: held.getUTCMinutes(), second: held.getUTCSeconds(), fraction: milliseconds(held.getUTCMilliseconds()) };
  return day.year >= 1 && day.year <= 9999 ? { kind: "key", text: dayText(day) + "T" + timeText(time) } : outside;
}

function milliseconds(count: number): string {
  return String(count).padStart(3, "0").padEnd(7, "0");
}

function dateTextKey(text: string, offsetDropped: boolean): Keyed {
  const day = dayAt(text);
  if (day.kind === "outside") return outside;
  if (text.length === 10) return offsetDropped ? outside : day.kind === "read" ? { kind: "key", text: dayText(day.day) + "T00:00:00.0000000" } : noValue;
  if (text[10] !== "T") return outside;
  const time = timeAt(text, 11);
  if (time.kind === "outside") return outside;
  const offset = offsetAt(text.slice(time.end));
  const offsetFits = offset.kind === "none" || (offsetDropped && offset.kind !== "outside");
  if (!offsetFits) return outside;
  if (day.kind === "noValue" || time.kind === "noValue" || offset.kind === "noValue") return noValue;
  return { kind: "key", text: dayText(day.day) + "T" + timeText(time.time) };
}

function instantTextKey(text: string): Keyed {
  const day = dayAt(text);
  if (day.kind === "outside" || text.length === 10 || text[10] !== "T") return outside;
  const time = timeAt(text, 11);
  if (time.kind === "outside") return outside;
  const offset = offsetAt(text.slice(time.end));
  if (offset.kind === "outside" || offset.kind === "none") return outside;
  if (day.kind === "noValue" || time.kind === "noValue" || offset.kind === "noValue") return noValue;
  const moment = new Date(0);
  moment.setUTCFullYear(day.day.year, day.day.month - 1, day.day.day);
  moment.setUTCHours(time.time.hour, time.time.minute - offset.minutes, 0, 0);
  const utc = { year: moment.getUTCFullYear(), month: moment.getUTCMonth() + 1, day: moment.getUTCDate() };
  if (utc.year < 1 || utc.year > 9999) return noValue;
  return { kind: "key", text: dayText(utc) + "T" + timeText({ ...time.time, hour: moment.getUTCHours(), minute: moment.getUTCMinutes() }) };
}

function dayTextKey(text: string): Keyed {
  const day = dayAt(text);
  if (day.kind !== "read") return day.kind === "noValue" && text.length === 10 ? noValue : outside;
  return text.length === 10 ? { kind: "key", text: dayText(day.day) } : outside;
}

function timeTextKey(text: string): Keyed {
  const time = timeAt(text, 0);
  if (time.kind === "outside" || time.end !== text.length) return outside;
  return time.kind === "noValue" ? noValue : { kind: "key", text: timeText(time.time) };
}

function spanTextKey(text: string): Keyed {
  const negative = text.startsWith("-");
  const body = negative ? text.slice(1) : text;
  const dot = body.indexOf(".");
  const colon = body.indexOf(":");
  const hasDays = dot >= 0 && dot < colon;
  const days = hasDays ? body.slice(0, dot) : "0";
  const clock = hasDays ? body.slice(dot + 1) : body;
  if (!isDigits(days)) return outside;
  const short = !negative && !hasDays && clock.length === 5;
  const time = short ? shortSpanAt(clock) : timeAt(clock, 0);
  if (time.kind === "outside" || time.end !== clock.length) return outside;
  if (!short && clock.length < 8) return outside;
  if (time.kind === "noValue") return time.hourPastTheDay ? outside : noValue;
  const ticks =
    BigInt(days) * 86_400n * ticksPerSecond +
    BigInt(time.time.hour * 3600 + time.time.minute * 60 + time.time.second) * ticksPerSecond +
    BigInt(time.time.fraction);
  const signed = negative ? -ticks : ticks;
  if (signed > latestTicks || signed < -ticksOffset) return noValue;
  return { kind: "key", text: (signed + ticksOffset).toString().padStart(20, "0") };
}

type DayRead = { readonly kind: "read"; readonly day: Day } | { readonly kind: "noValue" } | { readonly kind: "outside" };

type TimeRead =
  | { readonly kind: "read"; readonly time: Time; readonly end: number }
  | { readonly kind: "noValue"; readonly end: number; readonly hourPastTheDay: boolean }
  | { readonly kind: "outside" };

type OffsetRead = { readonly kind: "none" } | { readonly kind: "read"; readonly minutes: number } | { readonly kind: "noValue" } | { readonly kind: "outside" };

function dayAt(text: string): DayRead {
  const year = digitsAt(text, 0, 4);
  const month = digitsAt(text, 5, 2);
  const day = digitsAt(text, 8, 2);
  if (year === undefined || month === undefined || day === undefined || text[4] !== "-" || text[7] !== "-") return { kind: "outside" };
  return isCalendarDay(year, month, day) ? { kind: "read", day: { year, month, day } } : { kind: "noValue" };
}

function timeAt(text: string, at: number): TimeRead {
  const hour = digitsAt(text, at, 2);
  const minute = digitsAt(text, at + 3, 2);
  if (hour === undefined || minute === undefined || text[at + 2] !== ":") return { kind: "outside" };
  let end = at + 5;
  let second = 0;
  let fraction = "0000000";
  if (text[end] === ":") {
    const seconds = digitsAt(text, end + 1, 2);
    if (seconds === undefined) return { kind: "outside" };
    second = seconds;
    end += 3;
    if (text[end] === ".") {
      const places = fractionPlaces(text, end + 1);
      if (places === 0) return { kind: "outside" };
      fraction = text.slice(end + 1, end + 1 + places).padEnd(7, "0");
      end += 1 + places;
    }
  }
  if (hour > 23 || minute > 59 || second > 59) return { kind: "noValue", end, hourPastTheDay: hour > 23 };
  return { kind: "read", time: { hour, minute, second, fraction }, end };
}

function shortSpanAt(text: string): TimeRead {
  const hour = digitsAt(text, 0, 2);
  const minute = digitsAt(text, 3, 2);
  if (hour === undefined || minute === undefined || text[2] !== ":") return { kind: "outside" };
  if (hour > 23 || minute > 59) return { kind: "noValue", end: 5, hourPastTheDay: hour > 23 };
  return { kind: "read", time: { hour, minute, second: 0, fraction: "0000000" }, end: 5 };
}

function offsetAt(text: string): OffsetRead {
  if (text.length === 0) return { kind: "none" };
  if (text === "Z") return { kind: "read", minutes: 0 };
  const sign = text.startsWith("+") ? 1 : text.startsWith("-") ? -1 : 0;
  const hours = digitsAt(text, 1, 2);
  const minutes = digitsAt(text, 4, 2);
  if (sign === 0 || hours === undefined || minutes === undefined || text[3] !== ":" || text.length !== 6) return { kind: "outside" };
  const east = hours * 60 + minutes;
  if (minutes > 59 || east > latestOffsetMinutes) return { kind: "noValue" };
  return { kind: "read", minutes: sign * east };
}

function dayText(day: Day): string {
  return String(day.year).padStart(4, "0") + "-" + two(day.month) + "-" + two(day.day);
}

function timeText(time: Time): string {
  return two(time.hour) + ":" + two(time.minute) + ":" + two(time.second) + "." + time.fraction;
}

function two(count: number): string {
  return String(count).padStart(2, "0");
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

function isDigits(text: string): boolean {
  return text.length > 0 && digitsAt(text, 0, text.length) !== undefined;
}

function isDigit(code: number): boolean {
  return code >= 0x30 && code <= 0x39;
}

function isCalendarDay(year: number, month: number, day: number): boolean {
  return year >= 1 && month >= 1 && month <= 12 && day >= 1 && day <= daysIn(year, month);
}

function daysIn(year: number, month: number): number {
  if (month === 2) return year % 4 === 0 && (year % 100 !== 0 || year % 400 === 0) ? 29 : 28;
  return month === 4 || month === 6 || month === 9 || month === 11 ? 30 : 31;
}
