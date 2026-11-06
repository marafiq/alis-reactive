import type { Expression, OperatorExpression } from "./plan";
import { assertNever, nothing, value, type Read } from "./read";
import { lowerInvariant, nullOrEmpty, tested, trimmed, upperInvariant } from "./text";

type Evaluate = (argument: Expression) => Read;

type Ordered = number | string;

export function operate(expression: OperatorExpression, evaluate: Evaluate): Read {
  const args = expression.args;
  switch (expression.op) {
    case "eq":
      return compared(args, evaluate, (left, right) => left === right);
    case "neq":
      return compared(args, evaluate, (left, right) => left !== right);
    case "lt":
      return ordered(args, evaluate, (left, right) => left < right);
    case "lte":
      return ordered(args, evaluate, (left, right) => left <= right);
    case "gt":
      return ordered(args, evaluate, (left, right) => left > right);
    case "gte":
      return ordered(args, evaluate, (left, right) => left >= right);
    case "not":
      return negated(evaluate(first(args)));
    case "and":
      return both(args, evaluate);
    case "or":
      return either(args, evaluate);
    case "trim":
      return trimmed(evaluate(first(args)), "both");
    case "trimStart":
      return trimmed(evaluate(first(args)), "start");
    case "trimEnd":
      return trimmed(evaluate(first(args)), "end");
    case "upperInvariant":
      return upperInvariant(evaluate(first(args)));
    case "lowerInvariant":
      return lowerInvariant(evaluate(first(args)));
    case "contains":
      return tested(evaluate(first(args)), evaluate(second(args)), (held, sought) => held.includes(sought));
    case "startsWith":
      return tested(evaluate(first(args)), evaluate(second(args)), (held, sought) => held.startsWith(sought));
    case "endsWith":
      return tested(evaluate(first(args)), evaluate(second(args)), (held, sought) => held.endsWith(sought));
    case "nullOrEmpty":
      return nullOrEmpty(evaluate(first(args)));
    default:
      return assertNever(expression.op);
  }
}

function compared(args: readonly Expression[], evaluate: Evaluate, test: (left: unknown, right: unknown) => boolean): Read {
  const left = evaluate(first(args));
  const right = evaluate(second(args));
  if (left.kind === "nothing" || right.kind === "nothing") return nothing;
  return value(test(left.value, right.value));
}

function ordered(args: readonly Expression[], evaluate: Evaluate, test: (left: Ordered, right: Ordered) => boolean): Read {
  const left = evaluate(first(args));
  const right = evaluate(second(args));
  if (left.kind === "nothing" || right.kind === "nothing") return nothing;
  if (left.value === null || right.value === null) return value(false);
  return value(test(left.value as Ordered, right.value as Ordered));
}

function negated(term: Read): Read {
  return term.kind === "nothing" ? nothing : value(!term.value);
}

function both(args: readonly Expression[], evaluate: Evaluate): Read {
  const left = evaluate(first(args));
  if (isFalse(left)) return value(false);
  const right = evaluate(second(args));
  if (isFalse(right)) return value(false);
  return left.kind === "nothing" || right.kind === "nothing" ? nothing : value(true);
}

function either(args: readonly Expression[], evaluate: Evaluate): Read {
  const left = evaluate(first(args));
  if (isTrue(left)) return value(true);
  const right = evaluate(second(args));
  if (isTrue(right)) return value(true);
  return left.kind === "nothing" || right.kind === "nothing" ? nothing : value(false);
}

function isTrue(term: Read): boolean {
  return term.kind === "value" && Boolean(term.value);
}

function isFalse(term: Read): boolean {
  return term.kind === "value" && !term.value;
}

function first(args: readonly Expression[]): Expression {
  const argument = args[0];
  if (argument === undefined) throw new Error("The plan writes an operator with no operand.");
  return argument;
}

function second(args: readonly Expression[]): Expression {
  const argument = args[1];
  if (argument === undefined) throw new Error("The plan writes an operator with one operand where it compares two.");
  return argument;
}
