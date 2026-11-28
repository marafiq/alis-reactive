import { described, nothing, Stop, value, type Read } from "../ValuesAndSources/index";
import { decimalSum } from "./decimals";

const greatestInt = 2_147_483_647;

const leastInt = -2_147_483_648;

export function summedInts(list: Read): Read {
  if (list.kind === "nothing") return nothing;
  let total = 0;
  for (const term of list.value as readonly unknown[]) {
    if (typeof term !== "number" || !Number.isInteger(term)) throw new Stop("The term " + described(term) + " is no int, which an int sum adds.");
    total += term;
    if (total > greatestInt || total < leastInt) throw new Stop("An int sum passes the range of int at the term " + String(term) + ", as .NET's checked sum refuses it.");
  }
  return value(total);
}

export function summedDoubles(list: Read): Read {
  if (list.kind === "nothing") return nothing;
  let total = 0;
  for (const term of list.value as readonly unknown[]) {
    if (typeof term !== "number") throw new Stop("The term " + described(term) + " is no number, which a double sum adds.");
    total += term;
  }
  return value(total);
}

export function summedDecimals(list: Read): Read {
  if (list.kind === "nothing") return nothing;
  return value(decimalSum(list.value as readonly unknown[]));
}
