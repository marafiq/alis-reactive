import { described, nothing, Stop, value, type Read } from "../ValuesAndSources/index";

export function arrayOf(iterable: Read): Read {
  if (iterable.kind === "nothing" || iterable.value === null) return nothing;
  const held = iterable.value;
  if (!isIterable(held)) throw new Stop("The value " + described(held) + " yields no items: it has no iterator.");
  return value(Array.from(held));
}

function isIterable(held: unknown): held is Iterable<unknown> {
  return typeof Reflect.get(Object(held) as object, Symbol.iterator) === "function";
}
