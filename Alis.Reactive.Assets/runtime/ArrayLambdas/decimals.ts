import { described, Stop } from "../ValuesAndSources/index";

export interface Decimal {
  readonly digits: bigint;
  readonly scale: number;
}

const greatestDigits = 79_228_162_514_264_337_593_543_950_335n;

const greatestScale = 28;

export function decimalOf(held: unknown): Decimal {
  const text = typeof held === "number" && Number.isFinite(held) ? plainText(held) : typeof held === "string" ? held : undefined;
  const read = text === undefined ? undefined : decimalText(text);
  if (read === undefined) throw new Stop("The value " + described(held) + " is no decimal the framework reads.");
  return read;
}

function plainText(held: number): string {
  const shortest = String(held);
  const exponentAt = shortest.indexOf("e");
  if (exponentAt < 0) return shortest;
  const negative = shortest.startsWith("-");
  const mantissa = shortest.slice(negative ? 1 : 0, exponentAt);
  const exponent = Number(shortest.slice(exponentAt + 1));
  const point = mantissa.indexOf(".");
  const digits = mantissa.replace(".", "");
  const pointAt = (point < 0 ? mantissa.length : point) + exponent;
  const plain =
    pointAt <= 0 ? "0." + "0".repeat(-pointAt) + digits : pointAt >= digits.length ? digits + "0".repeat(pointAt - digits.length) : digits.slice(0, pointAt) + "." + digits.slice(pointAt);
  return (negative ? "-" : "") + plain;
}

function decimalText(text: string): Decimal | undefined {
  const negative = text.startsWith("-");
  const body = negative ? text.slice(1) : text;
  const point = body.indexOf(".");
  const whole = point < 0 ? body : body.slice(0, point);
  const fraction = point < 0 ? "" : body.slice(point + 1);
  const shaped = isDigits(whole) && (point < 0 || (fraction.length >= 1 && isDigits(fraction)));
  if (!shaped || fraction.length > greatestScale) return undefined;
  const digits = BigInt(whole + fraction);
  if (digits > greatestDigits) return undefined;
  return { digits: negative ? -digits : digits, scale: fraction.length };
}

function isDigits(text: string): boolean {
  if (text.length === 0) return false;
  for (let at = 0; at < text.length; at += 1) {
    const code = text.charCodeAt(at);
    if (code < 0x30 || code > 0x39) return false;
  }
  return true;
}

export function decimalSum(terms: readonly unknown[]): number {
  const exact = terms.map(decimalOf);
  const scale = exact.reduce((largest, term) => Math.max(largest, term.scale), 0);
  const total = exact.reduce((sum, term) => sum + term.digits * 10n ** BigInt(scale - term.scale), 0n);
  const magnitude = total < 0n ? -total : total;
  if (magnitude > greatestDigits * 10n ** BigInt(scale)) throw new Stop("A decimal sum passes decimal.MaxValue.");
  return Number(placed(total, scale));
}

export function decimalsCompared(left: unknown, right: unknown): boolean {
  const first = decimalOf(left);
  const second = decimalOf(right);
  const scale = Math.max(first.scale, second.scale);
  return first.digits * 10n ** BigInt(scale - first.scale) === second.digits * 10n ** BigInt(scale - second.scale);
}

function placed(total: bigint, scale: number): string {
  const negative = total < 0n;
  const digits = (negative ? -total : total).toString().padStart(scale + 1, "0");
  const whole = digits.slice(0, digits.length - scale);
  const fraction = digits.slice(digits.length - scale);
  return (negative ? "-" : "") + whole + (scale > 0 ? "." + fraction : "");
}
