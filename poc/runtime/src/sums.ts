import { nothing, Stop, value, type Read } from "./read";

const intMax = 2147483647;
const intMin = -2147483648;
const decimalMax = 79228162514264337593543950335n;

const decimalText = /^-?\d+(\.\d+)?$/;

interface Decimal {
  readonly digits: bigint;
  readonly scale: number;
}

export function intSum(list: Read): Read {
  const terms = termsOf(list);
  if (terms === undefined) return nothing;
  let total = 0;
  for (const term of terms) {
    total += term;
    if (total > intMax || total < intMin) throw new Stop("An int sum passed the range of int.");
  }
  return value(total);
}

export function doubleSum(list: Read): Read {
  const terms = termsOf(list);
  if (terms === undefined) return nothing;
  let total = 0;
  for (const term of terms) total += term;
  return value(total);
}

export function decimalSum(list: Read): Read {
  if (list.kind === "nothing") return nothing;
  const exact = (list.value as readonly unknown[]).map(decimalTerm);
  const scale = exact.reduce((largest, term) => Math.max(largest, term.scale), 0);
  let total = 0n;
  for (const term of exact) total += term.digits * 10n ** BigInt(scale - term.scale);
  const magnitude = total < 0n ? -total : total;
  if (magnitude > decimalMax * 10n ** BigInt(scale)) throw new Stop("A decimal sum passed the range of decimal.");
  return value(Number(placed(total, scale)));
}

function termsOf(list: Read): readonly number[] | undefined {
  if (list.kind === "nothing") return undefined;
  const terms = list.value as readonly unknown[];
  for (const term of terms) {
    if (typeof term !== "number") throw new Stop("A sum's term is not a number.");
  }
  return terms as readonly number[];
}

function decimalTerm(term: unknown): Decimal {
  if (typeof term === "number") return decimalOf(String(term));
  if (typeof term === "string" && decimalText.test(term)) return decimalOf(term);
  throw new Stop("A sum's term is not a number.");
}

export function invariantText(held: number): string {
  if (!Number.isFinite(held)) return String(held);
  const exact = decimalOf(String(held));
  return placed(exact.digits, exact.scale);
}

function decimalOf(text: string): Decimal {
  const [mantissa = "", exponent = "0"] = text.split("e");
  const negative = mantissa.startsWith("-");
  const [whole = "", fraction = ""] = (negative ? mantissa.slice(1) : mantissa).split(".");
  const scale = fraction.length - Number(exponent);
  const digits = BigInt(whole + fraction) * 10n ** BigInt(Math.max(0, -scale));
  return { digits: negative ? -digits : digits, scale: Math.max(0, scale) };
}

function placed(total: bigint, scale: number): string {
  const negative = total < 0n;
  const digits = (negative ? -total : total).toString().padStart(scale + 1, "0");
  const whole = digits.slice(0, digits.length - scale);
  const fraction = digits.slice(digits.length - scale);
  return (negative ? "-" : "") + whole + (scale > 0 ? "." + fraction : "");
}
