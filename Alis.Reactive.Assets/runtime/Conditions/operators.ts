import { assertNever, notBuilt, type Operator } from "../Schema/index";
import { nothing, Stop, value, type Read } from "../ValuesAndSources/index";
import { decimalsEqual } from "./decimals";
import { keyOf } from "./keys";
import { empty, included } from "./lists";
import { matched, nullOrEmpty, textTested } from "./texts";

export type Term = () => Read;

export function operated(op: Operator, terms: readonly Term[]): Read {
  switch (op) {
    case "eq":
      return compared(op, terms, (left, right) => left === right);
    case "neq":
      return compared(op, terms, (left, right) => left !== right);
    case "lt":
      return compared(op, terms, (left, right) => ordered(left, right, (order) => order < 0));
    case "lte":
      return compared(op, terms, (left, right) => ordered(left, right, (order) => order <= 0));
    case "gt":
      return compared(op, terms, (left, right) => ordered(left, right, (order) => order > 0));
    case "gte":
      return compared(op, terms, (left, right) => ordered(left, right, (order) => order >= 0));
    case "not":
      return not(termOf(op, terms, 0)());
    case "and":
      return and(termOf(op, terms, 0), termOf(op, terms, 1));
    case "or":
      return or(termOf(op, terms, 0), termOf(op, terms, 1));
    case "coalesce":
      return coalesced(termOf(op, terms, 0), termOf(op, terms, 1));
    case "cond":
      return chosen(termOf(op, terms, 0), termOf(op, terms, 1), termOf(op, terms, 2));
    case "add":
    case "sub":
    case "mul":
    case "mod":
    case "neg":
    case "nullableValue":
    case "nonNull":
    case "toArray":
    case "finite":
    case "compareNumbers":
    case "compareOrdinal":
    case "compareBooleans":
    case "compareCulture":
    case "merge":
    case "trim":
    case "trimStart":
    case "trimEnd":
    case "upperInvariant":
    case "lowerInvariant":
    case "upper":
    case "lower":
    case "upperTurkic":
    case "lowerTurkic":
    case "sumInt":
    case "sumDouble":
    case "sumDecimal":
      throw notBuilt("the operator '" + op + "'");
    case "eqDecimal":
      return decimalsEqual(termOf(op, terms, 0)(), termOf(op, terms, 1)());
    case "contains":
      return textTested(termOf(op, terms, 0)(), termOf(op, terms, 1)(), (held, sought) => held.includes(sought));
    case "startsWith":
      return textTested(termOf(op, terms, 0)(), termOf(op, terms, 1)(), (held, sought) => held.startsWith(sought));
    case "endsWith":
      return textTested(termOf(op, terms, 0)(), termOf(op, terms, 1)(), (held, sought) => held.endsWith(sought));
    case "nullOrEmpty":
      return nullOrEmpty(termOf(op, terms, 0)());
    case "matches":
      return matched(termOf(op, terms, 0)(), termOf(op, terms, 1)());
    case "empty":
      return empty(termOf(op, terms, 0)());
    case "includes":
      return included(termOf(op, terms, 0)(), termOf(op, terms, 1)());
    case "dateKey":
    case "wallKey":
    case "instantKey":
    case "dayKey":
    case "timeKey":
    case "spanKey":
      return keyOf(op, termOf(op, terms, 0)(), terms[1]);
    default:
      return assertNever(op);
  }
}

function termOf(op: Operator, terms: readonly Term[], position: number): Term {
  const term = terms[position];
  if (term === undefined) throw new Error("The plan's operator '" + op + "' has no argument at " + String(position) + ".");
  return term;
}

function compared(op: Operator, terms: readonly Term[], holds: (left: unknown, right: unknown) => boolean): Read {
  const left = termOf(op, terms, 0)();
  const right = termOf(op, terms, 1)();
  if (left.kind === "nothing" || right.kind === "nothing") return nothing;
  return value(holds(left.value, right.value));
}

function ordered(left: unknown, right: unknown, holds: (order: number) => boolean): boolean {
  if (left === null || right === null) return false;
  if (typeof left === "number" && typeof right === "number") return holds(left < right ? -1 : left > right ? 1 : 0);
  if (typeof left === "string" && typeof right === "string") return holds(left < right ? -1 : left > right ? 1 : 0);
  throw new Stop("The values " + JSON.stringify(left) + " and " + JSON.stringify(right) + " have no order between them.");
}

function not(term: Read): Read {
  if (term.kind === "nothing" || typeof term.value !== "boolean") return nothing;
  return value(!term.value);
}

function and(left: Term, right: Term): Read {
  const first = left();
  if (isFalse(first)) return value(false);
  const second = right();
  if (isFalse(second)) return value(false);
  return isTrue(first) && isTrue(second) ? value(true) : nothing;
}

function or(left: Term, right: Term): Read {
  const first = left();
  if (isTrue(first)) return value(true);
  const second = right();
  if (isTrue(second)) return value(true);
  return isFalse(first) && isFalse(second) ? value(false) : nothing;
}

function coalesced(first: Term, second: Term): Read {
  const read = first();
  return read.kind === "nothing" || read.value === null ? second() : read;
}

function chosen(condition: Term, then: Term, otherwise: Term): Read {
  return isTrue(condition()) ? then() : otherwise();
}

function isTrue(read: Read): boolean {
  return read.kind === "value" && read.value === true;
}

function isFalse(read: Read): boolean {
  return read.kind === "value" && read.value === false;
}
