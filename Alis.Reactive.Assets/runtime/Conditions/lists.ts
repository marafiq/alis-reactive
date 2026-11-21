import { described, nothing, Stop, value, type Read } from "../ValuesAndSources/index";

export function empty(list: Read): Read {
  if (list.kind === "nothing" || list.value === null) return value(true);
  if (!Array.isArray(list.value)) throw new Stop("The value " + described(list.value) + " is tested as a list, and it is no list.");
  return value(list.value.length === 0);
}

export function included(list: Read, item: Read): Read {
  if (item.kind === "nothing") return nothing;
  if (list.kind === "nothing" || !Array.isArray(list.value)) throw new Error("The plan's includes carries no list.");
  return value(list.value.includes(item.value));
}
