import { assertNever, notBuilt, type Operator } from "../Schema/index";
import { nothing, Stop, value, type Read } from "../ValuesAndSources/index";
import { arrayOf, comparedBooleans, comparedCulture, comparedNumbers, comparedOrdinal, lower, nonNull, nullableValue, summedDecimals, summedDoubles, summedInts, trimmed, upper } from "../ArrayLambdas/index";
import { decimalsEqual, precisionScaled } from "./decimals";
import { inFlags } from "./flags";
import { keyOf, partOf } from "./keys";
import { empty, included } from "./lists";
import { holdsLuhnSum, isEmailAddress, isWebAddress, matched, nullOrEmpty, textChecked, textTested } from "./texts";

export type Term = () => Read;

export function operated(op: Operator, terms: readonly Term[], collation: string): Read {
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
    case "finite":
    case "merge":
      throw notBuilt("the operator '" + op + "'");
    case "nullableValue":
      return nullableValue(termOf(op, terms, 0)());
    case "nonNull":
      return nonNull(termOf(op, terms, 0)());
    case "toArray":
      return arrayOf(termOf(op, terms, 0)());
    case "compareNumbers":
      return comparedNumbers(termOf(op, terms, 0)(), termOf(op, terms, 1)());
    case "compareOrdinal":
      return comparedOrdinal(termOf(op, terms, 0)(), termOf(op, terms, 1)());
    case "compareBooleans":
      return comparedBooleans(termOf(op, terms, 0)(), termOf(op, terms, 1)());
    case "compareCulture":
      return comparedCulture(termOf(op, terms, 0)(), termOf(op, terms, 1)(), collation);
    case "trim":
      return trimmed(termOf(op, terms, 0)(), "both");
    case "trimStart":
      return trimmed(termOf(op, terms, 0)(), "start");
    case "trimEnd":
      return trimmed(termOf(op, terms, 0)(), "end");
    case "upperInvariant":
      return upper(termOf(op, terms, 0)(), "invariant");
    case "lowerInvariant":
      return lower(termOf(op, terms, 0)(), "invariant");
    case "upper":
      return upper(termOf(op, terms, 0)(), "linguistic");
    case "lower":
      return lower(termOf(op, terms, 0)(), "linguistic");
    case "upperTurkic":
      return upper(termOf(op, terms, 0)(), "turkic");
    case "lowerTurkic":
      return lower(termOf(op, terms, 0)(), "turkic");
    case "sumInt":
      return summedInts(termOf(op, terms, 0)());
    case "sumDouble":
      return summedDoubles(termOf(op, terms, 0)());
    case "sumDecimal":
      return summedDecimals(termOf(op, terms, 0)());
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
    case "year":
    case "month":
    case "day":
    case "hour":
    case "minute":
    case "second":
    case "dayOfWeek":
      return partOf(op, termOf(op, terms, 0)());
    case "emailAddress":
      return textChecked(termOf(op, terms, 0)(), isEmailAddress);
    case "creditCard":
      return textChecked(termOf(op, terms, 0)(), holdsLuhnSum);
    case "url":
      return textChecked(termOf(op, terms, 0)(), isWebAddress);
    case "precisionScale":
      return precisionScaled(termOf(op, terms, 0)(), termOf(op, terms, 1)(), termOf(op, terms, 2)());
    case "inFlags":
      return inFlags(termOf(op, terms, 0)(), termOf(op, terms, 1)());
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
