// conditions/compare-engine.ts — pure SYNC condition evaluation (compare/all/any/not).
//
// Leaf module: it receives the value-evaluator by dependency injection (evalValue) and
// imports NOTHING from values/evaluate. This breaks the would-be cycle
// evaluate -> conditions -> evaluate (design §14.1): values/evaluate imports this leaf and
// passes its own evaluator to evaluate per-element array-op predicates, while
// conditions.ts delegates its sync subset here, passing evaluateValue. There is still one
// value resolver — the injected evalValue is always evaluateValue.

import type {
  CompareCondition,
  CollectionItemCompareCondition,
  EqualityCompareOp,
  EqualityCompareCondition,
  MembershipCompareCondition,
  MembershipCompareOp,
  OrderedCompareOp,
  OrderedCompareCondition,
  PlanDocument,
  RangeCompareCondition,
  RegexCompareCondition,
  Shape,
  TextLengthCompareCondition,
  TextCompareOp,
  TextCompareCondition,
  UnaryCompareOp,
  ValueExpression,
  ValidationCondition,
  ExecContext,
} from "../types/index";
import { scope } from "../diagnostics/trace";
import { assertNever } from "../shared/assert-never";
import { applyShape, toString } from "../shared/shape-convert";
import { ExecutionContext } from "../browser-objects/execution-context";
import { RuntimeShape } from "../browser-objects/runtime-shape";

const log = scope("conditions");

/** Value resolver injected by the caller - always values/evaluate's evaluateValue. */
export type ValueEvaluator = (expression: ValueExpression, plan: PlanDocument, ctx?: ExecContext) => unknown;

type TextOperand =
  | { readonly kind: "text"; readonly value: string }
  | { readonly kind: "missing" };

interface ComparisonLeft {
  readonly raw: unknown;
  readonly shaped: unknown;
}

type OrderedConditionValue =
  | { readonly kind: "number"; readonly value: number }
  | { readonly kind: "text"; readonly value: string }
  | { readonly kind: "boolean"; readonly value: boolean }
  | { readonly kind: "missing" };

const missingText: TextOperand = { kind: "missing" };
const missingOrderedConditionValue: OrderedConditionValue = { kind: "missing" };
const noRightOperandTrace = { kind: "none" } as const;

/** Evaluate the sync condition subset (compare/all/any/not). Confirm is not part of this subset. */
export function evaluateSyncCondition(
  condition: ValidationCondition,
  plan: PlanDocument,
  context: ExecutionContext,
  evalValue: ValueEvaluator,
): boolean {
  switch (condition.kind) {
    case "compare":
      return evaluateCompare(condition, plan, context, evalValue);
    case "all":
      return condition.terms.every(term => evaluateSyncCondition(term, plan, context, evalValue));
    case "any":
      return condition.terms.some(term => evaluateSyncCondition(term, plan, context, evalValue));
    case "not":
      return !evaluateSyncCondition(condition.term, plan, context, evalValue);
    default:
      return assertNever(condition, "condition kind");
  }
}

// -- Compare evaluation (always synchronous; shared by the async lane in conditions.ts) --

export function evaluateCompare(
  condition: CompareCondition,
  plan: PlanDocument,
  context: ExecutionContext,
  evalValue: ValueEvaluator,
): boolean {
  const left = resolveComparisonLeft(condition, plan, context, evalValue);

  switch (condition.op) {
    case "is-null":
    case "not-null":
    case "is-empty":
    case "not-empty":
    case "truthy":
    case "falsy":
      traceCompare(condition, left, noRightOperandTrace);
      return unaryMatches(condition.op, left);

    case "eq":
    case "neq": {
      const right = resolveRightValue(condition, plan, context, condition.shape, evalValue);
      traceCompare(condition, left, right);
      return equalityMatches(condition.op, left, right);
    }

    case "gt":
    case "gte":
    case "lt":
    case "lte": {
      const right = resolveRightValue(condition, plan, context, condition.shape, evalValue);
      traceCompare(condition, left, right);
      return orderedComparisonMatches(condition.op, left.shaped, right);
    }

    case "in":
    case "not-in": {
      const right = resolveMembershipItems(condition, plan, context, evalValue);
      traceCompare(condition, left, right);
      return membershipMatches(condition.op, left, right);
    }

    case "between": {
      const [lower, upper] = resolveRangeBounds(condition, plan, context, evalValue);
      traceCompare(condition, left, [lower, upper]);
      return inclusiveRangeContains(lower, upper, left.shaped);
    }

    case "array-contains": {
      const right = resolveRightValue(condition, plan, context, condition.itemShape, evalValue);
      const items = shapeCollectionItems(left.shaped, condition.itemShape);
      traceCompare(condition, left, right);
      return items !== undefined && items.includes(right);
    }

    case "contains":
      return evaluateTextComparison(condition.op, left, textOperand(condition), condition);

    case "starts-with":
      return evaluateTextComparison(condition.op, left, textOperand(condition), condition);

    case "ends-with":
      return evaluateTextComparison(condition.op, left, textOperand(condition), condition);

    case "matches":
      return evaluateRegexComparison(left, textOperand(condition), condition);

    case "min-length":
      return evaluateMinimumLengthComparison(left, minimumTextLength(condition), condition);

    default:
      return assertNever(condition, "compare condition");
  }
}

function resolveComparisonLeft(
  condition: CompareCondition,
  plan: PlanDocument,
  context: ExecutionContext,
  evalValue: ValueEvaluator,
): ComparisonLeft {
  const raw = evalValue(condition.left, plan, context.raw);
  const shaped = applyShape(raw, condition.shape);
  return { raw, shaped };
}

type ScalarRightCondition =
  | EqualityCompareCondition
  | OrderedCompareCondition
  | CollectionItemCompareCondition;

type TextRightCondition =
  | TextCompareCondition
  | RegexCompareCondition;

function traceCompare(condition: CompareCondition, left: ComparisonLeft, right: unknown): void {
  log.trace("compare", { op: condition.op, left: left.shaped, right });
}

function resolveRightValue(
  condition: ScalarRightCondition,
  plan: PlanDocument,
  context: ExecutionContext,
  shape: Shape,
  evalValue: ValueEvaluator,
): unknown {
  const raw = evalValue(condition.right.value, plan, context.raw);
  return applyShape(raw, shape);
}

function resolveMembershipItems(
  condition: MembershipCompareCondition,
  plan: PlanDocument,
  context: ExecutionContext,
  evalValue: ValueEvaluator,
): unknown[] {
  return condition.right.value.items.map(item =>
    resolveShapedComparisonItem(item, plan, context, condition.shape, evalValue));
}

function resolveRangeBounds(
  condition: RangeCompareCondition,
  plan: PlanDocument,
  context: ExecutionContext,
  evalValue: ValueEvaluator,
): [unknown, unknown] {
  const [lower, upper] = condition.right.value.items;
  return [
    resolveShapedComparisonItem(lower, plan, context, condition.shape, evalValue),
    resolveShapedComparisonItem(upper, plan, context, condition.shape, evalValue),
  ];
}

function resolveShapedComparisonItem(
  producer: ValueExpression,
  plan: PlanDocument,
  context: ExecutionContext,
  shape: Shape,
  evalValue: ValueEvaluator,
): unknown {
  return applyShape(evalValue(producer, plan, context.raw), shape);
}

function textOperand(condition: TextRightCondition): string {
  return condition.right.value.value;
}

function minimumTextLength(condition: TextLengthCompareCondition): number {
  return condition.right.value.value;
}

function unaryMatches(op: UnaryCompareOp, left: ComparisonLeft): boolean {
  switch (op) {
    case "is-null":
      return isMissingValue(left.raw);
    case "not-null":
      return !isMissingValue(left.raw);
    case "is-empty":
      return isEmpty(left.raw);
    case "not-empty":
      return !isEmpty(left.raw);
    case "truthy":
      return !!left.shaped;
    case "falsy":
      return !left.shaped;
    default:
      return assertNever(op, "unary comparison operator");
  }
}

function equalityMatches(op: EqualityCompareOp, left: ComparisonLeft, right: unknown): boolean {
  const valuesAreEqual = left.shaped === right;
  if (op === "eq") return valuesAreEqual;

  return !valuesAreEqual;
}

function membershipMatches(op: MembershipCompareOp, left: ComparisonLeft, values: readonly unknown[]): boolean {
  const collectionContainsLeft = values.includes(left.shaped);
  if (op === "in") return collectionContainsLeft;

  return !collectionContainsLeft;
}

function shapeCollectionItems(value: unknown, itemShape: Shape): unknown[] | undefined {
  if (!Array.isArray(value)) return undefined;

  return RuntimeShape.from(itemShape).applyEach(value);
}

function evaluateTextComparison(
  op: TextCompareOp,
  left: ComparisonLeft,
  right: string,
  condition: CompareCondition,
): boolean {
  traceCompare(condition, left, right);

  switch (op) {
    case "contains":
      return textOp(left.shaped, right, (source, operand) => source.includes(operand));
    case "starts-with":
      return textOp(left.shaped, right, (source, operand) => source.startsWith(operand));
    case "ends-with":
      return textOp(left.shaped, right, (source, operand) => source.endsWith(operand));
    default:
      return assertNever(op, "text comparison operator");
  }
}

function evaluateRegexComparison(left: ComparisonLeft, pattern: string, condition: CompareCondition): boolean {
  traceCompare(condition, left, pattern);

  const leftText = asText(left.shaped);
  if (leftText.kind === "missing") return false;

  return new RegExp(pattern).test(leftText.value);
}

function evaluateMinimumLengthComparison(left: ComparisonLeft, minimumLength: number, condition: CompareCondition): boolean {
  traceCompare(condition, left, minimumLength);

  const leftText = asText(left.shaped);
  return leftText.kind === "text" && leftText.value.length >= minimumLength;
}

function orderedComparisonMatches(op: OrderedCompareOp, left: unknown, right: unknown): boolean {
  const comparison = compareOrderedValues(left, right);
  if (comparison === undefined) return false;

  switch (op) {
    case "gt":
      return comparison > 0;
    case "gte":
      return comparison >= 0;
    case "lt":
      return comparison < 0;
    case "lte":
      return comparison <= 0;
    default:
      return assertNever(op, "ordered comparison operator");
  }
}

function inclusiveRangeContains(lower: unknown, upper: unknown, value: unknown): boolean {
  const boundsCanBeOrdered = compareOrderedValues(lower, upper) !== undefined;
  if (!boundsCanBeOrdered) return false;

  const lowerComparison = compareOrderedValues(value, lower);
  const upperComparison = compareOrderedValues(value, upper);
  if (lowerComparison === undefined || upperComparison === undefined) return false;

  return lowerComparison >= 0 && upperComparison <= 0;
}

function compareOrderedValues(left: unknown, right: unknown): number | undefined {
  const leftValue = toOrderedConditionValue(left);
  const rightValue = toOrderedConditionValue(right);

  switch (leftValue.kind) {
    case "number":
      return rightValue.kind === "number" ? leftValue.value - rightValue.value : undefined;
    case "text":
      if (rightValue.kind !== "text") return undefined;
      if (leftValue.value === rightValue.value) return 0;
      return leftValue.value > rightValue.value ? 1 : -1;
    case "boolean":
      return rightValue.kind === "boolean"
        ? booleanRank(leftValue.value) - booleanRank(rightValue.value)
        : undefined;
    case "missing":
      return undefined;
    default:
      return assertNever(leftValue, "ordered condition value");
  }
}

function toOrderedConditionValue(value: unknown): OrderedConditionValue {
  if (typeof value === "number" && Number.isFinite(value)) return { kind: "number", value };
  if (typeof value === "string") return { kind: "text", value };
  if (typeof value === "boolean") return { kind: "boolean", value };

  return missingOrderedConditionValue;
}

function booleanRank(value: boolean): number {
  return value ? 1 : 0;
}

function textOp(left: unknown, right: string, predicate: (source: string, operand: string) => boolean): boolean {
  const leftText = asText(left);
  return leftText.kind === "text" && predicate(leftText.value, right);
}

function isEmpty(value: unknown): boolean {
  const valueIsEmptyText = value === "";
  const valueIsMissing = isMissingValue(value);
  const valueIsEmptyCollection = Array.isArray(value) && value.length === 0;
  return valueIsEmptyText || valueIsMissing || valueIsEmptyCollection;
}

function asText(value: unknown): TextOperand {
  if (isMissingValue(value)) return missingText;
  const result = toString(value);
  if (result.ok) return { kind: "text", value: result.value };
  return missingText;
}

function isMissingValue(value: unknown): boolean {
  return value === null || value === undefined;
}
