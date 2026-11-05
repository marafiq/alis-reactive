// A date leaves the browser as the clock the user picked with that clock's offset
// (2026-11-04T09:00:00.000-08:00, or Z at offset 0): the instant is unchanged, and a
// server that binds DateTimeOffset reads both the picked clock and the instant.

import { toString, type ShapeConversionResult } from "../../shared/shape-convert";

export function requestWireText(value: unknown): ShapeConversionResult<string> {
  if (isValidDate(value)) return { ok: true, value: dateWithOffset(value) };

  return toString(value);
}

// JSON.stringify hands a replacer the result of Date.prototype.toJSON, so the date
// itself is read from the holder. Dates nested in any value (an array of Schedule
// events) take the same form; the values being sent are not changed.
export function requestJsonBody(body: Record<string, unknown>): string {
  return JSON.stringify(body, function dateOnTheWire(this: unknown, key: string, value: unknown): unknown {
    const held = (this as Record<string, unknown>)[key];
    if (isValidDate(held)) return dateWithOffset(held);

    return value;
  });
}

function isValidDate(value: unknown): value is Date {
  return value instanceof Date && !Number.isNaN(value.getTime());
}

function dateWithOffset(date: Date): string {
  const day = `${yearText(date.getFullYear())}-${twoDigits(date.getMonth() + 1)}-${twoDigits(date.getDate())}`;
  const clock = `${twoDigits(date.getHours())}:${twoDigits(date.getMinutes())}:${twoDigits(date.getSeconds())}`;
  const milliseconds = `${date.getMilliseconds()}`.padStart(3, "0");
  return `${day}T${clock}.${milliseconds}${offsetText(date)}`;
}

function offsetText(date: Date): string {
  const minutesEastOfUtc = -Math.round(date.getTimezoneOffset());
  if (minutesEastOfUtc === 0) return "Z";

  const sign = minutesEastOfUtc > 0 ? "+" : "-";
  const magnitude = Math.abs(minutesEastOfUtc);
  return `${sign}${twoDigits(Math.floor(magnitude / 60))}:${twoDigits(magnitude % 60)}`;
}

function yearText(year: number): string {
  const fitsFourDigits = year >= 0 && year <= 9999;
  if (fitsFourDigits) return `${year}`.padStart(4, "0");

  return `${year < 0 ? "-" : "+"}${`${Math.abs(year)}`.padStart(6, "0")}`;
}

function twoDigits(value: number): string {
  return `${value}`.padStart(2, "0");
}
