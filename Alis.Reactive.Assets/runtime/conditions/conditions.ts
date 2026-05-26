// conditions.ts — V3 Condition evaluation.
// Uses SHARED resolver for value resolution.
// Condition is a discriminated union: compare, all, any, not, confirm.
// ValidationCondition is the sync subset: compare, all, any, not.

import type {
  Condition,
  CompareCondition,
  EqualityCompareOp,
  MembershipCompareCondition,
  MembershipCompareOp,
  OrderedCompareOp,
  Plan,
  RangeCompareCondition,
  Shape,
  TextCompareOp,
  UnaryCompareOp,
  ValidationCondition,
  ValueProducer,
} from "../types";
import type { ExecContext } from "../types";
import { scope } from "../core/trace";
import { assertNever } from "../core/assert-never";
import { applyShape, toString } from "../core/shape-convert";
import { evaluateValue } from "../core/evaluate";
import { ExecutionContext } from "../domain/execution-context";
import { RuntimeShape } from "../domain/runtime-shape";

const log = scope("conditions");

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

interface AlisBrowserApi {
  readonly alis?: {
    readonly confirm?: (message: string) => Promise<boolean> | boolean;
  };
}

const missingText: TextOperand = { kind: "missing" };
const missingOrderedConditionValue: OrderedConditionValue = { kind: "missing" };
const noRightOperandTrace = { kind: "none" } as const;

/** Sync condition evaluation for validation conditions. */
export function evaluateCondition(condition: ValidationCondition, plan: Plan, ctx?: ExecContext): boolean {
  return evaluateConditionSync(condition, plan, ExecutionContext.from(ctx));
}

/** Async condition evaluation — required when conditions contain ConfirmCondition. */
export async function evaluateConditionAsync(condition: Condition, plan: Plan, ctx?: ExecContext): Promise<boolean> {
  return evaluateConditionAsyncCore(condition, plan, ExecutionContext.from(ctx));
}

/** Current-lane condition evaluation. Crosses to async only when a reached term requires it. */
export function evaluateConditionInCurrentLane(
  condition: Condition,
  plan: Plan,
  ctx?: ExecContext,
): boolean | Promise<boolean> {
  return evaluateConditionInLane(condition, plan, ExecutionContext.from(ctx));
}

function evaluateConditionSync(
  condition: ValidationCondition,
  plan: Plan,
  context: ExecutionContext,
): boolean {
  switch (condition.kind) {
    case "compare":
      return evaluateCompare(condition, plan, context);
    case "all":
      return condition.terms.every(term => evaluateConditionSync(term, plan, context));
    case "any":
      return condition.terms.some(term => evaluateConditionSync(term, plan, context));
    case "not":
      return !evaluateConditionSync(condition.term, plan, context);
    default:
      return assertNever(condition, "condition kind");
  }
}

async function evaluateConditionAsyncCore(
  condition: Condition,
  plan: Plan,
  context: ExecutionContext,
): Promise<boolean> {
  switch (condition.kind) {
    case "compare":
      return evaluateCompare(condition, plan, context);
    case "all":
      for (const term of condition.terms) {
        if (!await evaluateConditionAsyncCore(term, plan, context)) return false;
      }
      return true;
    case "any":
      for (const term of condition.terms) {
        if (await evaluateConditionAsyncCore(term, plan, context)) return true;
      }
      return false;
    case "not":
      return !(await evaluateConditionAsyncCore(condition.term, plan, context));
    case "confirm":
      return evaluateConfirmCondition(condition.message);
    default:
      return assertNever(condition, "condition kind");
  }
}

function evaluateConditionInLane(
  condition: Condition,
  plan: Plan,
  context: ExecutionContext,
): boolean | Promise<boolean> {
  switch (condition.kind) {
    case "compare":
      return evaluateCompare(condition, plan, context);
    case "all":
      return evaluateAllInLane(condition.terms, plan, context, 0);
    case "any":
      return evaluateAnyInLane(condition.terms, plan, context, 0);
    case "not":
      return negateConditionInLane(condition.term, plan, context);
    case "confirm":
      return evaluateConfirmCondition(condition.message);
    default:
      return assertNever(condition, "condition kind");
  }
}

function evaluateAllInLane(
  terms: readonly Condition[],
  plan: Plan,
  context: ExecutionContext,
  startIndex: number,
): boolean | Promise<boolean> {
  for (let index = startIndex; index < terms.length; index++) {
    const termMatches = evaluateConditionInLane(terms[index]!, plan, context);
    if (termMatches instanceof Promise) {
      return termMatches.then(matches =>
        matches ? evaluateAllInLane(terms, plan, context, index + 1) : false);
    }

    if (!termMatches) return false;
  }

  return true;
}

function evaluateAnyInLane(
  terms: readonly Condition[],
  plan: Plan,
  context: ExecutionContext,
  startIndex: number,
): boolean | Promise<boolean> {
  for (let index = startIndex; index < terms.length; index++) {
    const termMatches = evaluateConditionInLane(terms[index]!, plan, context);
    if (termMatches instanceof Promise) {
      return termMatches.then(matches =>
        matches ? true : evaluateAnyInLane(terms, plan, context, index + 1));
    }

    if (termMatches) return true;
  }

  return false;
}

function negateConditionInLane(
  condition: Condition,
  plan: Plan,
  context: ExecutionContext,
): boolean | Promise<boolean> {
  const termMatches = evaluateConditionInLane(condition, plan, context);
  if (termMatches instanceof Promise) return termMatches.then(matches => !matches);

  return !termMatches;
}

async function evaluateConfirmCondition(message: string): Promise<boolean> {
  const confirmFn = (window as AlisBrowserApi).alis?.confirm;
  if (!confirmFn) {
    log.error("confirm.dialog-missing");
    throw new Error("[alis] confirm condition requires @Html.FusionConfirmDialog() in layout");
  }

  const accepted = await confirmFn(message);
  log.debug("confirm.result", { accepted, message });
  return accepted;
}

// -- Compare evaluation --

function evaluateCompare(condition: CompareCondition, plan: Plan, context: ExecutionContext): boolean {
  const left = resolveComparisonLeft(condition, plan, context);

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
      const right = resolveRightValue(condition, plan, context, condition.shape);
      traceCompare(condition, left, right);
      return equalityMatches(condition.op, left, right);
    }

    case "gt":
    case "gte":
    case "lt":
    case "lte": {
      const right = resolveRightValue(condition, plan, context, condition.shape);
      traceCompare(condition, left, right);
      return orderedComparisonMatches(condition.op, left.shaped, right);
    }

    case "in":
    case "not-in": {
      const right = resolveRightArray(condition, plan, context);
      traceCompare(condition, left, right);
      return membershipMatches(condition.op, left, right);
    }

    case "between": {
      const [lower, upper] = resolveRightArray(condition, plan, context);
      traceCompare(condition, left, [lower, upper]);
      return inclusiveRangeContains(lower, upper, left.shaped);
    }

    case "array-contains": {
      const right = resolveRightValue(condition, plan, context, condition.itemShape);
      const items = shapeCollectionItems(left.shaped, condition.itemShape);
      traceCompare(condition, left, right);
      return items !== undefined && items.includes(right);
    }

    case "contains":
      return evaluateTextComparison(condition.op, left, condition.right.value.value, condition);

    case "starts-with":
      return evaluateTextComparison(condition.op, left, condition.right.value.value, condition);

    case "ends-with":
      return evaluateTextComparison(condition.op, left, condition.right.value.value, condition);

    case "matches":
      return evaluateRegexComparison(left, condition.right.value.value, condition);

    case "min-length":
      return evaluateMinimumLengthComparison(left, condition.right.value.value, condition);

    default:
      return assertNever(condition, "compare condition");
  }
}

function resolveComparisonLeft(condition: CompareCondition, plan: Plan, context: ExecutionContext): ComparisonLeft {
  const raw = evaluateValue(condition.left, plan, context.raw);
  const shaped = applyShape(raw, condition.shape);
  return { raw, shaped };
}

type RightValuedCondition = Extract<CompareCondition, { readonly right: { readonly kind: "value" } }>;

function traceCompare(condition: CompareCondition, left: ComparisonLeft, right: unknown): void {
  log.trace("compare", { op: condition.op, left: left.shaped, right });
}

function resolveRightValue(
  condition: RightValuedCondition,
  plan: Plan,
  context: ExecutionContext,
  shape: Shape,
): unknown {
  const raw = evaluateValue(condition.right.value as ValueProducer, plan, context.raw);
  return applyShape(raw, shape);
}

function resolveRightArray(
  condition: MembershipCompareCondition | RangeCompareCondition,
  plan: Plan,
  context: ExecutionContext,
): unknown[] {
  const raw = evaluateValue(condition.right.value, plan, context.raw) as unknown[];
  return raw.map(item => applyShape(item, condition.shape));
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

  try {
    return new RegExp(pattern).test(leftText.value);
  } catch {
    log.warn("regex.invalid", { operand: pattern });
    return false;
  }
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
      assertNever(op, "ordered comparison operator");
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
