// conditions.ts — V3 Condition evaluation.
// Uses SHARED resolver for value resolution.
// Condition is a discriminated union: compare, all, any, not, confirm.

import type { Condition, CompareCondition, CompareOp, Plan, Shape, ValidationCondition } from "../types";
import type { ExecContext } from "../types";
import { scope } from "../core/trace";
import { assertNever } from "../core/assert-never";
import { applyShape, toString } from "../core/shape-convert";
import { evaluateValue } from "../core/evaluate";
import { ExecutionContext } from "../domain/execution-context";
import { RuntimeShape } from "../domain/runtime-shape";

const log = scope("conditions");

type RuntimeEvaluableCondition = Condition | ValidationCondition;

type TextOperand =
  | { readonly kind: "text"; readonly value: string }
  | { readonly kind: "missing" };

type UnaryCompareOp = Extract<CompareOp, "is-null" | "not-null" | "is-empty" | "not-empty" | "truthy" | "falsy">;
type EqualityCompareOp = Extract<CompareOp, "eq" | "neq">;
type OrderedCompareOp = Extract<CompareOp, "gt" | "gte" | "lt" | "lte">;
type MembershipCompareOp = Extract<CompareOp, "in" | "not-in">;
type TextCompareOp = Extract<CompareOp, "contains" | "starts-with" | "ends-with">;

interface AlisBrowserApi {
  readonly alis?: {
    readonly confirm?: (message: string) => Promise<boolean> | boolean;
  };
}

const missingText: TextOperand = { kind: "missing" };
const textOperandShape: Shape = { kind: "string" };

/** Sync condition evaluation. Confirm conditions return false in sync context. */
export function evaluateCondition(condition: RuntimeEvaluableCondition, plan: Plan, ctx?: ExecContext): boolean {
  return RuntimeCondition.from(condition, plan, ExecutionContext.from(ctx)).evaluateSync();
}

/** Async condition evaluation — required when conditions contain ConfirmCondition. */
export async function evaluateConditionAsync(condition: Condition, plan: Plan, ctx?: ExecContext): Promise<boolean> {
  return RuntimeCondition.from(condition, plan, ExecutionContext.from(ctx)).evaluateAsync();
}

/** Current-lane condition evaluation. Crosses to async only when a reached term requires it. */
export function evaluateConditionInCurrentLane(
  condition: Condition,
  plan: Plan,
  ctx?: ExecContext,
): boolean | Promise<boolean> {
  return RuntimeCondition.from(condition, plan, ExecutionContext.from(ctx)).evaluateInCurrentLane();
}

abstract class RuntimeCondition {
  static from(condition: RuntimeEvaluableCondition, plan: Plan, context: ExecutionContext): RuntimeCondition {
    switch (condition.kind) {
      case "compare":
        return new CompareRuntimeCondition(condition, plan, context);
      case "all":
        return new AllRuntimeCondition(condition.terms, plan, context);
      case "any":
        return new AnyRuntimeCondition(condition.terms, plan, context);
      case "not":
        return new NotRuntimeCondition(condition.term, plan, context);
      case "confirm":
        return new ConfirmRuntimeCondition(condition.message);
      default:
        return assertNever(condition, "condition kind");
    }
  }

  abstract evaluateSync(): boolean;

  abstract evaluateAsync(): Promise<boolean>;

  abstract evaluateInCurrentLane(): boolean | Promise<boolean>;
}

class CompareRuntimeCondition extends RuntimeCondition {
  constructor(
    private readonly condition: CompareCondition,
    private readonly plan: Plan,
    private readonly context: ExecutionContext,
  ) {
    super();
  }

  evaluateSync(): boolean {
    return evaluateCompare(this.condition, this.plan, this.context);
  }

  async evaluateAsync(): Promise<boolean> {
    return this.evaluateSync();
  }

  evaluateInCurrentLane(): boolean {
    return this.evaluateSync();
  }
}

abstract class CompositeRuntimeCondition extends RuntimeCondition {
  constructor(
    kind: "all" | "any",
    protected readonly terms: readonly RuntimeEvaluableCondition[],
    protected readonly plan: Plan,
    protected readonly context: ExecutionContext,
  ) {
    super();
    if (terms.length === 0) {
      throw new Error(`[alis] ${kind} condition requires at least one term`);
    }
  }

  protected runtimeTerms(): RuntimeCondition[] {
    return this.terms.map(term => RuntimeCondition.from(term, this.plan, this.context));
  }

  protected runtimeTerm(term: RuntimeEvaluableCondition): RuntimeCondition {
    return RuntimeCondition.from(term, this.plan, this.context);
  }
}

class AllRuntimeCondition extends CompositeRuntimeCondition {
  constructor(terms: readonly RuntimeEvaluableCondition[], plan: Plan, context: ExecutionContext) {
    super("all", terms, plan, context);
  }

  evaluateSync(): boolean {
    return this.runtimeTerms().every(term => term.evaluateSync());
  }

  async evaluateAsync(): Promise<boolean> {
    for (const term of this.runtimeTerms()) {
      if (!await term.evaluateAsync()) return false;
    }

    return true;
  }

  evaluateInCurrentLane(): boolean | Promise<boolean> {
    return this.evaluateFrom(0);
  }

  private evaluateFrom(startIndex: number): boolean | Promise<boolean> {
    for (let index = startIndex; index < this.terms.length; index++) {
      const term = this.terms[index]!;
      const termMatches = this.runtimeTerm(term).evaluateInCurrentLane();
      if (termMatches instanceof Promise) {
        return this.evaluateAfterAsyncTerm(termMatches, index + 1);
      }

      if (!termMatches) return false;
    }

    return true;
  }

  private async evaluateAfterAsyncTerm(termMatches: Promise<boolean>, nextIndex: number): Promise<boolean> {
    if (!await termMatches) return false;

    return await this.evaluateFrom(nextIndex);
  }
}

class AnyRuntimeCondition extends CompositeRuntimeCondition {
  constructor(terms: readonly RuntimeEvaluableCondition[], plan: Plan, context: ExecutionContext) {
    super("any", terms, plan, context);
  }

  evaluateSync(): boolean {
    return this.runtimeTerms().some(term => term.evaluateSync());
  }

  async evaluateAsync(): Promise<boolean> {
    for (const term of this.runtimeTerms()) {
      if (await term.evaluateAsync()) return true;
    }

    return false;
  }

  evaluateInCurrentLane(): boolean | Promise<boolean> {
    return this.evaluateFrom(0);
  }

  private evaluateFrom(startIndex: number): boolean | Promise<boolean> {
    for (let index = startIndex; index < this.terms.length; index++) {
      const term = this.terms[index]!;
      const termMatches = this.runtimeTerm(term).evaluateInCurrentLane();
      if (termMatches instanceof Promise) {
        return this.evaluateAfterAsyncTerm(termMatches, index + 1);
      }

      if (termMatches) return true;
    }

    return false;
  }

  private async evaluateAfterAsyncTerm(termMatches: Promise<boolean>, nextIndex: number): Promise<boolean> {
    if (await termMatches) return true;

    return await this.evaluateFrom(nextIndex);
  }
}

class NotRuntimeCondition extends RuntimeCondition {
  constructor(
    private readonly term: RuntimeEvaluableCondition,
    private readonly plan: Plan,
    private readonly context: ExecutionContext,
  ) {
    super();
  }

  evaluateSync(): boolean {
    return !RuntimeCondition.from(this.term, this.plan, this.context).evaluateSync();
  }

  async evaluateAsync(): Promise<boolean> {
    return !(await RuntimeCondition.from(this.term, this.plan, this.context).evaluateAsync());
  }

  evaluateInCurrentLane(): boolean | Promise<boolean> {
    const termMatches = RuntimeCondition.from(this.term, this.plan, this.context).evaluateInCurrentLane();
    if (termMatches instanceof Promise) return this.negateAsync(termMatches);

    return !termMatches;
  }

  private async negateAsync(termMatches: Promise<boolean>): Promise<boolean> {
    return !(await termMatches);
  }
}

class ConfirmRuntimeCondition extends RuntimeCondition {
  constructor(private readonly message: string) {
    super();
  }

  evaluateSync(): boolean {
    log.warn("confirm.sync-denied");
    return false;
  }

  async evaluateAsync(): Promise<boolean> {
    const confirmFn = (window as AlisBrowserApi).alis?.confirm;
    if (!confirmFn) {
      log.error("confirm.dialog-missing");
      throw new Error("[alis] confirm condition requires @Html.FusionConfirmDialog() in layout");
    }

    const accepted = await confirmFn(this.message);
    log.debug("confirm.result", { accepted, message: this.message });
    return accepted;
  }

  evaluateInCurrentLane(): Promise<boolean> {
    return this.evaluateAsync();
  }
}

// -- Compare evaluation --

function evaluateCompare(cond: CompareCondition, plan: Plan, context: ExecutionContext): boolean {
  const left = ComparisonLeft.resolve(cond, plan, context);
  const operation = ComparisonOperation.from(cond);
  const right = ComparisonRight.resolve(cond, plan, context, operation);
  log.trace("compare", { op: cond.op, left: left.shaped, right: right.traceValue });
  return operation.evaluate(left, right);
}

class ComparisonLeft {
  private constructor(
    readonly raw: unknown,
    readonly shaped: unknown,
  ) {}

  static resolve(condition: CompareCondition, plan: Plan, context: ExecutionContext): ComparisonLeft {
    const raw = evaluateValue(condition.left, plan, context.raw);
    const shaped = applyShape(raw, condition.shape);
    return new ComparisonLeft(raw, shaped);
  }
}

class ComparisonRight {
  private constructor(private readonly state: ComparisonRightState) {}

  static resolve(
    condition: CompareCondition,
    plan: Plan,
    context: ExecutionContext,
    operation: ComparisonOperation,
  ): ComparisonRight {
    if (condition.right.kind === "none") return ComparisonRight.absent();

    const raw = evaluateValue(condition.right.value, plan, context.raw);
    const rightOperandIsCollection = Array.isArray(raw) && operation.shapesRightCollection;
    if (rightOperandIsCollection) {
      return ComparisonRight.present(raw.map(item => applyShape(item, condition.shape)));
    }

    return ComparisonRight.present(applyShape(raw, operation.rightOperandShape(condition)));
  }

  static absent(): ComparisonRight {
    return new ComparisonRight(absentComparisonRight);
  }

  static present(value: unknown): ComparisonRight {
    return new ComparisonRight({ kind: "present", value });
  }

  get traceValue(): unknown {
    if (this.state.kind === "present") return this.state.value;
    return absentComparisonRightTrace;
  }

  requireValue(operator: string): unknown {
    if (this.state.kind === "present") return this.state.value;

    throw new Error(`[alis] comparison operator "${operator}" requires a right operand`);
  }

  collection(operator: string): ComparisonCollection {
    return ComparisonCollection.from(this.requireValue(operator));
  }
}

type ComparisonRightState =
  | { readonly kind: "present"; readonly value: unknown }
  | { readonly kind: "absent" };

const absentComparisonRight: ComparisonRightState = { kind: "absent" };
const absentComparisonRightTrace = { kind: "absent" } as const;

abstract class ComparisonOperation {
  readonly shapesRightCollection: boolean = false;

  static from(condition: CompareCondition): ComparisonOperation {
    switch (condition.op) {
      case "is-null":
      case "not-null":
      case "is-empty":
      case "not-empty":
      case "truthy":
      case "falsy":
        return new UnaryComparisonOperation(condition.op);

      case "eq":
      case "neq":
        return new EqualityComparisonOperation(condition.op);

      case "gt":
      case "gte":
      case "lt":
      case "lte":
        return new OrderedComparisonOperation(condition.op);

      case "in":
      case "not-in":
        return new MembershipComparisonOperation(condition.op);

      case "between":
        return new BetweenComparisonOperation();

      case "array-contains":
        return new ArrayContainsComparisonOperation(condition.itemShape);

      case "contains":
      case "starts-with":
      case "ends-with":
        return new TextComparisonOperation(condition.op);

      case "matches":
        return RegexComparisonOperation.instance;

      case "min-length":
        return MinLengthComparisonOperation.instance;

      default:
        return assertNever(condition.op, "condition operator");
    }
  }

  rightOperandShape(condition: CompareCondition): Shape {
    return condition.shape;
  }

  abstract evaluate(left: ComparisonLeft, right: ComparisonRight): boolean;
}

class UnaryComparisonOperation extends ComparisonOperation {
  constructor(private readonly op: UnaryCompareOp) {
    super();
  }

  evaluate(left: ComparisonLeft, _right: ComparisonRight): boolean {
    switch (this.op) {
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
        assertNever(this.op, "unary comparison operator");
    }
  }
}

class EqualityComparisonOperation extends ComparisonOperation {
  constructor(private readonly op: EqualityCompareOp) {
    super();
  }

  evaluate(left: ComparisonLeft, right: ComparisonRight): boolean {
    const rightValue = right.requireValue(this.op);
    const valuesAreEqual = left.shaped === rightValue;
    if (this.op === "eq") return valuesAreEqual;

    return !valuesAreEqual;
  }
}

class OrderedComparisonOperation extends ComparisonOperation {
  constructor(private readonly op: OrderedCompareOp) {
    super();
  }

  evaluate(left: ComparisonLeft, right: ComparisonRight): boolean {
    return ConditionOrdering
      .between(left.shaped, right.requireValue(this.op))
      .matches(this.op);
  }
}

class MembershipComparisonOperation extends ComparisonOperation {
  override readonly shapesRightCollection = true;

  constructor(private readonly op: MembershipCompareOp) {
    super();
  }

  evaluate(left: ComparisonLeft, right: ComparisonRight): boolean {
    const collection = right.collection(this.op);
    if (!collection.isAvailable) return this.op === "not-in";

    const collectionContainsLeft = collection.includes(left.shaped);
    if (this.op === "in") return collectionContainsLeft;
    return !collectionContainsLeft;
  }
}

class BetweenComparisonOperation extends ComparisonOperation {
  override readonly shapesRightCollection = true;

  evaluate(left: ComparisonLeft, right: ComparisonRight): boolean {
    const range = ComparisonRange.from(right.requireValue("between"));
    return range.contains(left.shaped);
  }
}

abstract class ComparisonRange {
  static from(value: unknown): ComparisonRange {
    const descriptor = ComparisonRangeDescriptor.from(value);

    const lowerComparable = OrderedConditionValue.from(descriptor.lower);
    const upperComparable = OrderedConditionValue.from(descriptor.upper);
    const rangeHasComparableBounds = lowerComparable.tryCompareTo(upperComparable) !== undefined;
    if (!rangeHasComparableBounds) return MissingComparisonRange.instance;

    return new OrderedComparisonRange(lowerComparable, upperComparable);
  }

  abstract contains(value: unknown): boolean;
}

class ComparisonRangeDescriptor {
  private constructor(
    readonly lower: unknown,
    readonly upper: unknown,
  ) {}

  static from(value: unknown): ComparisonRangeDescriptor {
    if (!Array.isArray(value)) {
      throw new Error("[alis] between comparison range must be an array with exactly two bounds");
    }

    const rangeDeclaresExactlyTwoBounds = value.length === 2;
    if (!rangeDeclaresExactlyTwoBounds) {
      throw new Error(`[alis] between comparison range must contain exactly two bounds, got ${value.length}`);
    }

    return new ComparisonRangeDescriptor(value[0], value[1]);
  }
}

class OrderedComparisonRange extends ComparisonRange {
  constructor(
    private readonly lowerBound: OrderedConditionValue,
    private readonly upperBound: OrderedConditionValue,
  ) {
    super();
  }

  contains(value: unknown): boolean {
    const subject = OrderedConditionValue.from(value);
    const lowerComparison = subject.tryCompareTo(this.lowerBound);
    const upperComparison = subject.tryCompareTo(this.upperBound);
    const subjectHasRangeShape = lowerComparison !== undefined && upperComparison !== undefined;
    if (!subjectHasRangeShape) return false;

    return lowerComparison >= 0 && upperComparison <= 0;
  }
}

class MissingComparisonRange extends ComparisonRange {
  static readonly instance = new MissingComparisonRange();

  contains(_value: unknown): boolean {
    return false;
  }
}

class ArrayContainsComparisonOperation extends ComparisonOperation {
  constructor(private readonly itemShape: Shape) {
    super();
  }

  override rightOperandShape(): Shape {
    return this.itemShape;
  }

  evaluate(left: ComparisonLeft, right: ComparisonRight): boolean {
    const items = this.shapeItems(left.shaped);
    return Array.isArray(items) && items.includes(right.requireValue("array-contains"));
  }

  private shapeItems(value: unknown): unknown {
    return ComparisonCollection
      .from(value)
      .shapedOrOriginal(value, this.itemShape);
  }
}

abstract class ComparisonCollection {
  static from(value: unknown): ComparisonCollection {
    const valueIsCollection = Array.isArray(value);
    if (!valueIsCollection) return MissingComparisonCollection.instance;

    return new AvailableComparisonCollection(value);
  }

  static isEmpty(value: unknown): boolean {
    return ComparisonCollection.from(value).isEmpty;
  }

  abstract get isAvailable(): boolean;
  abstract get isEmpty(): boolean;
  abstract includes(value: unknown): boolean;
  abstract shapedOrOriginal(original: unknown, itemShape: Shape): unknown;
}

class AvailableComparisonCollection extends ComparisonCollection {
  constructor(private readonly items: unknown[]) {
    super();
  }

  get isAvailable(): boolean {
    return true;
  }

  get isEmpty(): boolean {
    return this.items.length === 0;
  }

  includes(value: unknown): boolean {
    return this.items.includes(value);
  }

  shapedOrOriginal(_original: unknown, itemShape: Shape): unknown[] {
    return RuntimeShape.from(itemShape).applyEach(this.items);
  }
}

class MissingComparisonCollection extends ComparisonCollection {
  static readonly instance = new MissingComparisonCollection();

  get isAvailable(): boolean {
    return false;
  }

  get isEmpty(): boolean {
    return false;
  }

  includes(_value: unknown): boolean {
    return false;
  }

  shapedOrOriginal(original: unknown, _itemShape: Shape): unknown {
    return original;
  }
}

class TextComparisonOperation extends ComparisonOperation {
  constructor(private readonly op: TextCompareOp) {
    super();
  }

  override rightOperandShape(): Shape {
    return textOperandShape;
  }

  evaluate(left: ComparisonLeft, right: ComparisonRight): boolean {
    const rightValue = right.requireValue(this.op);
    switch (this.op) {
      case "contains":
        return textOp(left.shaped, rightValue, (source, operand) => source.includes(operand));
      case "starts-with":
        return textOp(left.shaped, rightValue, (source, operand) => source.startsWith(operand));
      case "ends-with":
        return textOp(left.shaped, rightValue, (source, operand) => source.endsWith(operand));
      default:
        assertNever(this.op, "text comparison operator");
    }
  }
}

class RegexComparisonOperation extends ComparisonOperation {
  static readonly instance = new RegexComparisonOperation();

  private constructor() {
    super();
  }

  override rightOperandShape(): Shape {
    return textOperandShape;
  }

  evaluate(left: ComparisonLeft, right: ComparisonRight): boolean {
    const rightValue = right.requireValue("matches");
    const leftText = asText(left.shaped);
    const rightText = asText(rightValue);
    const operandsAreText = leftText.kind === "text" && rightText.kind === "text";
    if (!operandsAreText) return false;

    try {
      return new RegExp(rightText.value).test(leftText.value);
    } catch {
      log.warn("regex.invalid", { operand: rightValue });
      return false;
    }
  }
}

class MinLengthComparisonOperation extends ComparisonOperation {
  static readonly instance = new MinLengthComparisonOperation();

  private constructor() {
    super();
  }

  evaluate(left: ComparisonLeft, right: ComparisonRight): boolean {
    const rightValue = right.requireValue("min-length");
    const leftText = asText(left.shaped);
    if (leftText.kind === "missing") return false;

    return MinimumTextLength.from(rightValue).accepts(leftText.value);
  }
}

class ConditionOrdering {
  private constructor(private readonly comparison: number | undefined) {}

  static between(left: unknown, right: unknown): ConditionOrdering {
    const leftValue = OrderedConditionValue.from(left);
    const rightValue = OrderedConditionValue.from(right);
    const comparison = leftValue.tryCompareTo(rightValue);
    if (comparison === undefined) return ConditionOrdering.unavailable;

    return new ConditionOrdering(comparison);
  }

  matches(op: OrderedCompareOp): boolean {
    if (this.comparison === undefined) return false;

    switch (op) {
      case "gt":
        return this.comparison > 0;
      case "gte":
        return this.comparison >= 0;
      case "lt":
        return this.comparison < 0;
      case "lte":
        return this.comparison <= 0;
      default:
        assertNever(op, "ordered comparison operator");
    }
  }

  private static readonly unavailable = new ConditionOrdering(undefined);
}

type OrderedConditionDomain = "number" | "text" | "boolean" | "missing";

abstract class OrderedConditionValue {
  static from(value: unknown): OrderedConditionValue {
    if (typeof value === "number") return NumberOrderedConditionValue.from(value);
    if (typeof value === "string") return new TextOrderedConditionValue(value);
    if (typeof value === "boolean") return new BooleanOrderedConditionValue(value);

    return MissingOrderedConditionValue.instance;
  }

  abstract get domain(): OrderedConditionDomain;
  abstract tryCompareTo(other: OrderedConditionValue): number | undefined;
}

class NumberOrderedConditionValue extends OrderedConditionValue {
  private constructor(private readonly value: number) {
    super();
  }

  static from(value: number): OrderedConditionValue {
    if (!Number.isFinite(value)) return MissingOrderedConditionValue.instance;

    return new NumberOrderedConditionValue(value);
  }

  get domain(): OrderedConditionDomain {
    return "number";
  }

  tryCompareTo(other: OrderedConditionValue): number | undefined {
    if (!(other instanceof NumberOrderedConditionValue)) return undefined;

    return this.value - other.value;
  }
}

class TextOrderedConditionValue extends OrderedConditionValue {
  constructor(private readonly value: string) {
    super();
  }

  get domain(): OrderedConditionDomain {
    return "text";
  }

  tryCompareTo(other: OrderedConditionValue): number | undefined {
    if (!(other instanceof TextOrderedConditionValue)) return undefined;
    if (this.value === other.value) return 0;

    return this.value > other.value ? 1 : -1;
  }
}

class BooleanOrderedConditionValue extends OrderedConditionValue {
  constructor(private readonly value: boolean) {
    super();
  }

  get domain(): OrderedConditionDomain {
    return "boolean";
  }

  tryCompareTo(other: OrderedConditionValue): number | undefined {
    if (!(other instanceof BooleanOrderedConditionValue)) return undefined;

    return BooleanOrderedConditionValue.toRank(this.value) - BooleanOrderedConditionValue.toRank(other.value);
  }

  private static toRank(value: boolean): number {
    return value ? 1 : 0;
  }
}

class MissingOrderedConditionValue extends OrderedConditionValue {
  static readonly instance = new MissingOrderedConditionValue();

  get domain(): OrderedConditionDomain {
    return "missing";
  }

  tryCompareTo(_other: OrderedConditionValue): number | undefined {
    return undefined;
  }
}

abstract class MinimumTextLength {
  static from(value: unknown): MinimumTextLength {
    const minimum = MinimumTextLength.toFiniteNumber(value);
    if (minimum === undefined) return MissingMinimumTextLength.instance;

    return new PresentMinimumTextLength(minimum);
  }

  private static toFiniteNumber(value: unknown): number | undefined {
    const parsed = typeof value === "number" || typeof value === "string"
      ? Number(value)
      : NaN;
    const valueIsUsableLength = Number.isFinite(parsed) && parsed >= 0;
    if (!valueIsUsableLength) return undefined;

    return parsed;
  }

  abstract accepts(text: string): boolean;
}

class PresentMinimumTextLength extends MinimumTextLength {
  constructor(private readonly value: number) {
    super();
  }

  accepts(text: string): boolean {
    return text.length >= this.value;
  }
}

class MissingMinimumTextLength extends MinimumTextLength {
  static readonly instance = new MissingMinimumTextLength();

  accepts(_text: string): boolean {
    return false;
  }
}

function textOp(left: unknown, right: unknown, predicate: (source: string, operand: string) => boolean): boolean {
  const leftText = asText(left);
  const rightText = asText(right);
  const operandsAreText = leftText.kind === "text" && rightText.kind === "text";
  return operandsAreText && predicate(leftText.value, rightText.value);
}

function isEmpty(value: unknown): boolean {
  const valueIsEmptyText = value === "";
  const valueIsMissing = isMissingValue(value);
  const valueIsEmptyCollection = ComparisonCollection.isEmpty(value);
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
