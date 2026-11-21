import { described, Stop, value, type Read } from "../ValuesAndSources/index";

const collators = new Map<string, Intl.Collator>();

export function comparedNumbers(left: Read, right: Read): Read {
  return compared(left, right, "number", (first, second) => numberOrder(first as number, second as number));
}

export function comparedOrdinal(left: Read, right: Read): Read {
  return compared(left, right, "string", (first, second) => sign(first as string, second as string));
}

export function comparedBooleans(left: Read, right: Read): Read {
  return compared(left, right, "boolean", (first, second) => sign(Number(first), Number(second)));
}

export function comparedCulture(left: Read, right: Read, collation: string): Read {
  return compared(left, right, "string", (first, second) => Math.sign(collatorOf(collation).compare(first as string, second as string)));
}

function compared(left: Read, right: Read, held: "number" | "string" | "boolean", order: (first: unknown, second: unknown) => number): Read {
  if (left.kind === "nothing" || right.kind === "nothing") throw new Stop("A sort key reads nothing.");
  if (left.value === null || right.value === null) return value(nullsFirst(left.value, right.value));
  if (typeof left.value !== held) throw unordered(left.value, held);
  if (typeof right.value !== held) throw unordered(right.value, held);
  return value(order(left.value, right.value));
}

function numberOrder(first: number, second: number): number {
  if (Number.isNaN(first)) return Number.isNaN(second) ? 0 : -1;
  if (Number.isNaN(second)) return 1;
  return sign(first, second);
}

function sign<T extends number | string>(first: T, second: T): number {
  if (first < second) return -1;
  return first > second ? 1 : 0;
}

function nullsFirst(left: unknown, right: unknown): number {
  if (left === right) return 0;
  return left === null ? -1 : 1;
}

function unordered(held: unknown, ordered: string): Stop {
  return new Stop("The sort key " + described(held) + " is no " + ordered + ", which its order compares.");
}

function collatorOf(collation: string): Intl.Collator {
  const known = collators.get(collation);
  if (known !== undefined) return known;
  if (Intl.Collator.supportedLocalesOf([collation]).length === 0) throw new Stop("The browser has no collation for the culture '" + collation + "'.");
  const collator = new Intl.Collator(collation);
  collators.set(collation, collator);
  return collator;
}
