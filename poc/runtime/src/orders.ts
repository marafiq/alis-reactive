import { Stop, value, type Read } from "./read";

export type Ordering = "numbers" | "ordinal" | "booleans";

const heldAs: Readonly<Record<Ordering, string>> = { numbers: "number", ordinal: "string", booleans: "boolean" };

export function sortOrder(left: Read, right: Read, ordering: Ordering): Read {
  if (left.kind === "nothing" || right.kind === "nothing") throw new Stop("A sort key read nothing.");
  if (left.value === null || right.value === null) return value(nullsFirst(left.value, right.value));
  if (typeof left.value !== heldAs[ordering] || typeof right.value !== heldAs[ordering]) throw new Stop("A sort key holds a value its order does not compare.");
  return value(sign(left.value as number | string, right.value as number | string));
}

function nullsFirst(left: unknown, right: unknown): number {
  if (left === right) return 0;
  return left === null ? -1 : 1;
}

function sign(left: number | string, right: number | string): number {
  if (left < right) return -1;
  return left > right ? 1 : 0;
}
