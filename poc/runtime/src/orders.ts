import { Stop, value, type Read } from "./read";

type Key = number | string;

export type Ordering = "numbers" | "ordinal" | "booleans";

const heldAs: Readonly<Record<Ordering, string>> = { numbers: "number", ordinal: "string", booleans: "boolean" };

const collators = new Map<string, Intl.Collator>();

export function sortOrder(left: Read, right: Read, ordering: Ordering): Read {
  return ordered(left, right, heldAs[ordering], sign);
}

export function cultureOrder(left: Read, right: Read, collation: string): Read {
  return ordered(left, right, "string", (first, second) => Math.sign(collatorOf(collation).compare(first as string, second as string)));
}

function ordered(left: Read, right: Read, held: string, compare: (left: Key, right: Key) => number): Read {
  if (left.kind === "nothing" || right.kind === "nothing") throw new Stop("A sort key read nothing.");
  if (left.value === null || right.value === null) return value(nullsFirst(left.value, right.value));
  if (typeof left.value !== held || typeof right.value !== held) throw new Stop("A sort key holds a value its order does not compare.");
  return value(compare(left.value as Key, right.value as Key));
}

function collatorOf(collation: string): Intl.Collator {
  const known = collators.get(collation);
  if (known !== undefined) return known;
  const browserHasTheCollation = Intl.Collator.supportedLocalesOf([collation]).length > 0;
  if (!browserHasTheCollation) throw new Stop("The browser has no collation for the culture '" + collation + "'.");
  const collator = new Intl.Collator(collation);
  collators.set(collation, collator);
  return collator;
}

function nullsFirst(left: unknown, right: unknown): number {
  if (left === right) return 0;
  return left === null ? -1 : 1;
}

function sign(left: Key, right: Key): number {
  if (left < right) return -1;
  return left > right ? 1 : 0;
}
