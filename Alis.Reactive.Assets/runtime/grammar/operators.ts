import { asNumber, asText, isMissing } from "./shape";
import { toJavaScriptString } from "../shared/javascript-string";

export function equal(left: unknown, right: unknown): boolean {
  if (isMissing(left) || isMissing(right)) return isMissing(left) && isMissing(right);
  if (left instanceof Date && right instanceof Date) return left.getTime() === right.getTime();
  return left === right;
}

type Ordered =
  | { readonly kind: "number"; readonly value: number }
  | { readonly kind: "text"; readonly value: string }
  | { readonly kind: "boolean"; readonly value: boolean }
  | { readonly kind: "none" };

function ordered(value: unknown): Ordered {
  if (typeof value === "number" && Number.isFinite(value)) return { kind: "number", value };
  if (value instanceof Date && Number.isFinite(value.getTime())) return { kind: "number", value: value.getTime() };
  if (typeof value === "string") return { kind: "text", value };
  if (typeof value === "boolean") return { kind: "boolean", value };
  return { kind: "none" };
}

export function compare(left: unknown, right: unknown): number | undefined {
  const a = ordered(left);
  const b = ordered(right);
  if (a.kind === "none" || a.kind !== b.kind) return undefined;
  if (a.kind === "number" && b.kind === "number") return Math.sign(a.value - b.value);
  if (a.kind === "text" && b.kind === "text") return a.value === b.value ? 0 : a.value > b.value ? 1 : -1;
  if (a.kind === "boolean" && b.kind === "boolean") return Number(a.value) - Number(b.value);
  return undefined;
}

export function lessThan(left: unknown, right: unknown): boolean {
  const sign = compare(left, right);
  return sign !== undefined && sign < 0;
}

export function lessThanOrEqual(left: unknown, right: unknown): boolean {
  const sign = compare(left, right);
  return sign !== undefined && sign <= 0;
}

export function greaterThan(left: unknown, right: unknown): boolean {
  const sign = compare(left, right);
  return sign !== undefined && sign > 0;
}

export function greaterThanOrEqual(left: unknown, right: unknown): boolean {
  const sign = compare(left, right);
  return sign !== undefined && sign >= 0;
}

export function arithmetic(op: "add" | "sub" | "mul" | "mod", left: unknown, right: unknown): unknown {
  const a = asNumber(left);
  const b = asNumber(right);
  if (a === undefined || b === undefined) return null;
  switch (op) {
    case "add": return a + b;
    case "sub": return a - b;
    case "mul": return a * b;
    case "mod": return a % b;
  }
}

export function negate(value: unknown): unknown {
  const number = asNumber(value);
  return number === undefined ? null : -number;
}

export function truthy(value: unknown): boolean {
  return !!value;
}

export function empty(value: unknown): boolean {
  return value === "" || isMissing(value) || (Array.isArray(value) && value.length === 0);
}

export function matches(value: unknown, pattern: unknown, flags: unknown): boolean {
  if (isMissing(value)) return false;
  const text = asText(value);
  if (text === undefined) return false;
  return new RegExp(pattern as string, flags as string).test(text);
}

export function toArray(value: unknown): unknown[] {
  if (Array.isArray(value)) return value;
  if (isMissing(value)) return [];
  if (typeof value === "string" || typeof value === "number") return [value];
  const isIterable = typeof value === "object" && Symbol.iterator in (value as object);
  if (isIterable) return Array.from(value as Iterable<unknown>);
  throw new Error(`[alis] an array method needs an array, got ${describe(value)}`);
}

export function merge(records: readonly unknown[]): Record<string, unknown> {
  return Object.assign({}, ...(records as object[]));
}

export function finite(value: unknown): number {
  const number = typeof value === "number" ? value : Number(value);
  return Number.isFinite(number) ? number : 0;
}

export function order(left: unknown, right: unknown): number {
  const a = sortKey(left);
  const b = sortKey(right);
  if (typeof a === "number" && typeof b === "number") {
    const aIsFinite = Number.isFinite(a);
    const bIsFinite = Number.isFinite(b);
    if (aIsFinite && bIsFinite) return a - b;
    if (aIsFinite !== bIsFinite) return aIsFinite ? -1 : 1;
    return 0;
  }
  const aText = toJavaScriptString(a);
  const bText = toJavaScriptString(b);
  return aText === bText ? 0 : aText > bText ? 1 : -1;
}

function sortKey(value: unknown): unknown {
  return value instanceof Date ? value.getTime() : value;
}

function describe(value: unknown): string {
  if (typeof value === "object") return (value as object).constructor?.name ?? "object";
  return typeof value;
}
