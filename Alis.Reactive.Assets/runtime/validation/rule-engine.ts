// Rule Engine - pure validation rule evaluation.
// No DOM, no vendor, no side effects. The orchestrator resolves peers first.

import { assertNever } from "../core/assert-never";
import { toString } from "../core/shape-convert";
import type { ConvertResult } from "../core/shape-convert";
import type { ValidationRule, ValidationRuleName, Shape, ValueProducer, ValidationRuleOperand as PlanValidationRuleOperand } from "../types";
import { RuntimeShape } from "../domain/runtime-shape";

export type ResolvedPeerValue =
  | { readonly kind: "present"; readonly value: unknown }
  | { readonly kind: "absent" };

export interface RuleEvaluation {
  readonly rule: ValidationRule;
  readonly value: unknown;
  readonly peerValue: ResolvedPeerValue;
}

const missingPeerValue: ResolvedPeerValue = { kind: "absent" };

export function peerValue(value: unknown): ResolvedPeerValue {
  return { kind: "present", value };
}

export function noPeerValue(): ResolvedPeerValue {
  return missingPeerValue;
}

export function ruleFails(evaluation: RuleEvaluation): boolean {
  const ruleEvaluation = ValidationRuleEvaluation.from(evaluation);
  return ValidationRuleOperations.from(ruleEvaluation.rule.name).fails(ruleEvaluation);
}

class ValidationRuleEvaluation {
  private constructor(
    readonly rule: ValidationRule,
    readonly subject: ValidationSubject,
    private readonly peerValue: ResolvedPeerValue,
  ) {}

  static from(evaluation: RuleEvaluation): ValidationRuleEvaluation {
    return new ValidationRuleEvaluation(
      evaluation.rule,
      ValidationSubject.from(evaluation.value),
      evaluation.peerValue,
    );
  }

  comparisonTarget(): ValidationScalarTarget {
    return ValidationScalarTarget.fromResolvedPeerOrConstraint(
      this.peerValue,
      this.rule.execution.constraint,
    );
  }

  constraint(): ValidationScalarTarget {
    return ValidationScalarTarget.fromConstraintOperand(this.rule.execution.constraint);
  }

  get comparisonShape(): Shape {
    return this.rule.execution.comparisonShape;
  }

  lengthConstraint(): ValidationLengthConstraint {
    return ValidationLengthConstraint.fromOperand(this.rule.execution.constraint);
  }

  rangeTarget(): ValidationRangeTarget {
    return ValidationRangeTarget.fromOperand(this.rule.execution.constraint);
  }
}

class ValidationSubject {
  private constructor(
    readonly raw: unknown,
    private readonly textConversion: ConvertResult<string>,
  ) {}

  static from(raw: unknown): ValidationSubject {
    return new ValidationSubject(raw, toString(raw));
  }

  get text(): string {
    if (this.textConversion.ok) return this.textConversion.value;
    return "";
  }

  get isEmpty(): boolean {
    const valueIsMissing = MissingValidationValue.matches(this.raw);
    const valueIsFalse = this.raw === false;
    const valueIsEmptyString = this.text === "";
    const valueIsEmptyArray = EmptyValidationCollection.matches(this.raw);
    const valueCannotBeConvertedToText = !this.textConversion.ok;
    return valueIsMissing || valueIsFalse || valueIsEmptyString || valueCannotBeConvertedToText || valueIsEmptyArray;
  }

  get length(): number {
    return this.text.length;
  }

  failsAtLeastOne(): boolean {
    if (Array.isArray(this.raw)) return this.raw.length === 0;
    return this.isEmpty;
  }

  compareTo(target: ValidationScalarTarget, shape: Shape): ShapedComparison {
    return target.compareWithSubject(this.raw, shape);
  }

  equalsTarget(target: ValidationScalarTarget, shape: Shape): boolean {
    return target.equalsSubject(this.raw, shape);
  }
}

abstract class ValidationScalarTarget {
  static fromResolvedPeerOrConstraint(
    peerValue: ResolvedPeerValue,
    constraintOperand: PlanValidationRuleOperand,
  ): ValidationScalarTarget {
    if (peerValue.kind === "present") return ValidationScalarTarget.available(peerValue.value);
    return ValidationScalarTarget.fromConstraintOperand(constraintOperand);
  }

  static fromConstraintOperand(operand: PlanValidationRuleOperand): ValidationScalarTarget {
    switch (operand.kind) {
      case "none": return ValidationScalarTarget.missing();
      case "value": return ValidationScalarTarget.fromConstraintProducer(operand.value);
      default: return assertNever(operand, "validation rule operand");
    }
  }

  static available(value: unknown): ValidationScalarTarget {
    return new AvailableValidationScalarTarget(value);
  }

  private static fromConstraintProducer(producer: ValueProducer): ValidationScalarTarget {
    if (producer.kind === "literal") return ValidationScalarTarget.available(producer.value);

    throw new Error(`[alis] validation constraint operand must be a literal, got "${producer.kind}"`);
  }

  static missing(): ValidationScalarTarget {
    return MissingValidationScalarTarget.instance;
  }

  abstract get isAvailable(): boolean;

  abstract textOrEmpty(): string;

  abstract compareWithSubject(subject: unknown, shape: Shape): ShapedComparison;

  abstract equalsSubject(subject: unknown, shape: Shape): boolean;
}

class AvailableValidationScalarTarget extends ValidationScalarTarget {
  constructor(private readonly value: unknown) {
    super();
  }

  get isAvailable(): boolean {
    return true;
  }

  textOrEmpty(): string {
    const converted = toString(this.value);
    if (converted.ok) return converted.value;

    return "";
  }

  compareWithSubject(subject: unknown, shape: Shape): ShapedComparison {
    return ShapedComparison.between(subject, this.value, shape);
  }

  equalsSubject(subject: unknown, shape: Shape): boolean {
    return ShapeAwareEquality.matches(subject, this.value, shape);
  }
}

class MissingValidationScalarTarget extends ValidationScalarTarget {
  static readonly instance = new MissingValidationScalarTarget();

  get isAvailable(): boolean {
    return false;
  }

  textOrEmpty(): string {
    return "";
  }

  compareWithSubject(): ShapedComparison {
    return ShapedComparison.missing();
  }

  equalsSubject(): boolean {
    return false;
  }
}

class ValidationLengthConstraint {
  private constructor(private readonly expectedLength: number) {}

  static fromOperand(operand: PlanValidationRuleOperand): ValidationLengthConstraint {
    switch (operand.kind) {
      case "none":
        throw new Error("[alis] validation length constraint is missing");
      case "value":
        return ValidationLengthConstraint.fromProducer(operand.value);
      default:
        return assertNever(operand, "validation length operand");
    }
  }

  private static fromProducer(producer: ValueProducer): ValidationLengthConstraint {
    if (producer.kind !== "literal") {
      throw new Error(`[alis] validation constraint operand must be a literal, got "${producer.kind}"`);
    }

    return ValidationLengthConstraint.fromValue(producer.value);
  }

  private static fromValue(value: unknown): ValidationLengthConstraint {
    const expectedLength = Number(value);
    if (Number.isFinite(expectedLength)) return new ValidationLengthConstraint(expectedLength);

    throw new Error(`[alis] validation length constraint must be a finite number, got "${String(value)}"`);
  }

  isGreaterThan(actualLength: number): boolean {
    return this.expectedLength > actualLength;
  }

  isLessThan(actualLength: number): boolean {
    return this.expectedLength < actualLength;
  }
}

class ShapedComparison {
  private constructor(private readonly difference: number) {}

  static missing(): ShapedComparison {
    return new ShapedComparison(NaN);
  }

  static between(left: unknown, right: unknown, shape: Shape): ShapedComparison {
    const comparisonShape = RuntimeShape.from(shape);
    if (!comparisonShape.isDeclared) return ShapedComparison.missing();

    const leftValue = ComparableValidationValue.from(left, comparisonShape);
    const rightValue = ComparableValidationValue.from(right, comparisonShape);
    const operandsAreComparable = leftValue !== undefined && rightValue !== undefined;
    if (!operandsAreComparable) return ShapedComparison.missing();

    return new ShapedComparison(leftValue.differenceFrom(rightValue));
  }

  get cannotCompare(): boolean {
    return Number.isNaN(this.difference);
  }

  get lessThanTarget(): boolean {
    return this.difference < 0;
  }

  get greaterThanTarget(): boolean {
    return this.difference > 0;
  }

  get equalToTarget(): boolean {
    return this.difference === 0;
  }
}

class ComparableValidationValue {
  private constructor(private readonly value: number) {}

  static from(raw: unknown, shape: RuntimeShape): ComparableValidationValue | undefined {
    const converted = shape.convert(raw);
    if (!converted.ok) return undefined;
    if (typeof converted.value !== "number") return undefined;
    if (!Number.isFinite(converted.value)) return undefined;

    return new ComparableValidationValue(converted.value);
  }

  differenceFrom(target: ComparableValidationValue): number {
    return this.value - target.value;
  }
}

class ShapeAwareEquality {
  static matches(left: unknown, right: unknown, shape: Shape): boolean {
    const comparisonShape = RuntimeShape.from(shape);
    if (comparisonShape.isDeclared) {
      return comparisonShape.apply(left) === comparisonShape.apply(right);
    }

    const normalizedLeft = ShapeAwareEquality.textOrEmpty(left);
    const normalizedRight = ShapeAwareEquality.textOrEmpty(right);
    return normalizedLeft === normalizedRight;
  }

  private static textOrEmpty(value: unknown): string {
    const text = toString(value);
    if (text.ok) return text.value;
    return "";
  }
}

class MissingValidationValue {
  static matches(value: unknown): boolean {
    const valueIsNull = value === null;
    const valueIsUndefined = value === undefined;
    return valueIsNull || valueIsUndefined;
  }
}

class EmptyValidationCollection {
  static matches(value: unknown): boolean {
    const valueIsCollection = Array.isArray(value);
    if (!valueIsCollection) return false;

    return value.length === 0;
  }
}

interface RuleOperation {
  fails(evaluation: ValidationRuleEvaluation): boolean;
}

type LengthRuleName = Extract<ValidationRuleName, "minLength" | "maxLength">;
type OrderedComparisonRuleName = Extract<ValidationRuleName, "min" | "max" | "gt" | "lt">;
type RangeRuleName = Extract<ValidationRuleName, "range" | "exclusiveRange">;
type EqualityRuleName = Extract<ValidationRuleName, "equalTo" | "notEqual" | "notEqualTo">;

class ValidationRuleOperations {
  static from(ruleName: ValidationRuleName): RuleOperation {
    switch (ruleName) {
      case "required": return new RequiredRuleOperation();
      case "empty": return new EmptyRuleOperation();
      case "minLength":
      case "maxLength":
        return new LengthRuleOperation(ruleName);
      case "email": return new EmailRuleOperation();
      case "regex": return new RegexRuleOperation();
      case "url": return new UrlRuleOperation();
      case "creditCard": return new CreditCardRuleOperation();
      case "min":
      case "max":
      case "gt":
      case "lt":
        return new OrderedComparisonRuleOperation(ruleName);
      case "range":
      case "exclusiveRange":
        return new RangeRuleOperation(ruleName);
      case "equalTo":
      case "notEqual":
      case "notEqualTo":
        return new EqualityRuleOperation(ruleName);
      case "atLeastOne": return new AtLeastOneRuleOperation();
      default: return assertNever(ruleName, "validation rule");
    }
  }
}

class RequiredRuleOperation implements RuleOperation {
  fails(evaluation: ValidationRuleEvaluation): boolean {
    return evaluation.subject.isEmpty;
  }
}

class EmptyRuleOperation implements RuleOperation {
  fails(evaluation: ValidationRuleEvaluation): boolean {
    return !evaluation.subject.isEmpty;
  }
}

class LengthRuleOperation implements RuleOperation {
  constructor(private readonly ruleName: LengthRuleName) {}

  fails(evaluation: ValidationRuleEvaluation): boolean {
    if (evaluation.subject.isEmpty) return false;

    const actualLength = evaluation.subject.length;
    const expectedLength = evaluation.lengthConstraint();

    if (this.ruleName === "minLength") return expectedLength.isGreaterThan(actualLength);
    return expectedLength.isLessThan(actualLength);
  }
}

class EmailRuleOperation implements RuleOperation {
  fails(evaluation: ValidationRuleEvaluation): boolean {
    const valueWasProvided = !evaluation.subject.isEmpty;
    const valueLooksLikeEmail = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(evaluation.subject.text);
    return valueWasProvided && !valueLooksLikeEmail;
  }
}

class RegexRuleOperation implements RuleOperation {
  fails(evaluation: ValidationRuleEvaluation): boolean {
    const constraint = this.pattern(evaluation);
    try {
      return !evaluation.subject.isEmpty && !new RegExp(constraint).test(evaluation.subject.text);
    } catch {
      return true;
    }
  }

  private pattern(evaluation: ValidationRuleEvaluation): string {
    return evaluation.constraint().textOrEmpty();
  }
}

class UrlRuleOperation implements RuleOperation {
  fails(evaluation: ValidationRuleEvaluation): boolean {
    const valueWasProvided = !evaluation.subject.isEmpty;
    const valueLooksLikeUrl = /^https?:\/\/.+/.test(evaluation.subject.text);
    return valueWasProvided && !valueLooksLikeUrl;
  }
}

class CreditCardRuleOperation implements RuleOperation {
  fails(evaluation: ValidationRuleEvaluation): boolean {
    const valueWasProvided = !evaluation.subject.isEmpty;
    const valuePassesLuhn = luhn(evaluation.subject.text.replace(/\D/g, ""));
    return valueWasProvided && !valuePassesLuhn;
  }
}

class OrderedComparisonRuleOperation implements RuleOperation {
  constructor(private readonly ruleName: OrderedComparisonRuleName) {}

  fails(evaluation: ValidationRuleEvaluation): boolean {
    const target = evaluation.comparisonTarget();
    const comparison = evaluation.subject.compareTo(target, evaluation.comparisonShape);
    const valueIsEmpty = evaluation.subject.isEmpty;

    switch (this.ruleName) {
      case "min": {
        const valueIsBelowMinimum = comparison.cannotCompare || comparison.lessThanTarget;
        return !valueIsEmpty && valueIsBelowMinimum;
      }
      case "max": {
        const valueIsAboveMaximum = comparison.cannotCompare || comparison.greaterThanTarget;
        return !valueIsEmpty && valueIsAboveMaximum;
      }
      case "gt": {
        const valueIsNotGreaterThanTarget =
          comparison.cannotCompare || comparison.lessThanTarget || comparison.equalToTarget;
        return valueIsEmpty || valueIsNotGreaterThanTarget;
      }
      case "lt": {
        const valueIsNotLessThanTarget =
          comparison.cannotCompare || comparison.greaterThanTarget || comparison.equalToTarget;
        return !valueIsEmpty && valueIsNotLessThanTarget;
      }
      default: return assertNever(this.ruleName, "ordered validation comparison");
    }
  }
}

class RangeRuleOperation implements RuleOperation {
  constructor(private readonly ruleName: RangeRuleName) {}

  fails(evaluation: ValidationRuleEvaluation): boolean {
    const range = evaluation.rangeTarget();
    if (!range.isAvailable) return true;
    if (evaluation.subject.isEmpty) return false;

    const lowerComparison = evaluation.subject.compareTo(range.lowerBound, evaluation.comparisonShape);
    const upperComparison = evaluation.subject.compareTo(range.upperBound, evaluation.comparisonShape);
    const rangeCannotBeCompared = lowerComparison.cannotCompare || upperComparison.cannotCompare;
    if (rangeCannotBeCompared) return true;

    if (this.ruleName === "range") {
      const valueFallsOutsideInclusiveRange =
        lowerComparison.lessThanTarget || upperComparison.greaterThanTarget;
      return valueFallsOutsideInclusiveRange;
    }

    const valueFallsOutsideExclusiveRange =
      lowerComparison.lessThanTarget || lowerComparison.equalToTarget ||
      upperComparison.greaterThanTarget || upperComparison.equalToTarget;
    return valueFallsOutsideExclusiveRange;
  }
}

abstract class ValidationRangeTarget {
  static fromOperand(operand: PlanValidationRuleOperand): ValidationRangeTarget {
    switch (operand.kind) {
      case "none":
        return MissingValidationRangeTarget.instance;
      case "value":
        return ValidationRangeTarget.fromProducer(operand.value);
      default:
        return assertNever(operand, "validation range operand");
    }
  }

  private static fromProducer(producer: ValueProducer): ValidationRangeTarget {
    if (producer.kind !== "literal") {
      throw new Error(`[alis] validation range constraint operand must be a literal, got "${producer.kind}"`);
    }

    const bounds = ValidationRangeDescriptor.from(producer.value);

    return new AvailableValidationRangeTarget(
      ValidationScalarTarget.available(bounds.lower),
      ValidationScalarTarget.available(bounds.upper),
    );
  }

  abstract get isAvailable(): boolean;
  abstract get lowerBound(): ValidationScalarTarget;
  abstract get upperBound(): ValidationScalarTarget;
}

class AvailableValidationRangeTarget extends ValidationRangeTarget {
  constructor(
    readonly lowerBound: ValidationScalarTarget,
    readonly upperBound: ValidationScalarTarget,
  ) {
    super();
  }

  get isAvailable(): boolean {
    return true;
  }
}

class MissingValidationRangeTarget extends ValidationRangeTarget {
  static readonly instance = new MissingValidationRangeTarget();

  get isAvailable(): boolean {
    return false;
  }

  get lowerBound(): ValidationScalarTarget {
    return ValidationScalarTarget.missing();
  }

  get upperBound(): ValidationScalarTarget {
    return ValidationScalarTarget.missing();
  }
}

class ValidationRangeDescriptor {
  private constructor(
    readonly lower: unknown,
    readonly upper: unknown,
  ) {}

  static from(value: unknown): ValidationRangeDescriptor {
    const targetIsCollection = Array.isArray(value);
    if (!targetIsCollection) {
      throw new Error("[alis] validation range descriptor must be an array with exactly two bounds");
    }

    const rangeDeclaresExactlyTwoBounds = value.length === 2;
    if (!rangeDeclaresExactlyTwoBounds) {
      throw new Error(`[alis] validation range descriptor must contain exactly two bounds, got ${value.length}`);
    }

    return new ValidationRangeDescriptor(value[0], value[1]);
  }
}

class EqualityRuleOperation implements RuleOperation {
  constructor(private readonly ruleName: EqualityRuleName) {}

  fails(evaluation: ValidationRuleEvaluation): boolean {
    switch (this.ruleName) {
      case "equalTo": return this.failsEqualTo(evaluation);
      case "notEqual": return this.failsNotEqual(evaluation);
      case "notEqualTo": return this.failsNotEqualTo(evaluation);
      default: return assertNever(this.ruleName, "equality validation comparison");
    }
  }

  private failsEqualTo(evaluation: ValidationRuleEvaluation): boolean {
    if (evaluation.subject.isEmpty) return false;

    const target = evaluation.comparisonTarget();
    if (!target.isAvailable) return true;
    return !evaluation.subject.equalsTarget(target, evaluation.comparisonShape);
  }

  private failsNotEqual(evaluation: ValidationRuleEvaluation): boolean {
    const target = evaluation.constraint();
    if (!target.isAvailable) return false;
    const valueWasProvided = !evaluation.subject.isEmpty;
    const valueEqualsForbiddenTarget = evaluation.subject.equalsTarget(target, evaluation.comparisonShape);
    return valueWasProvided && valueEqualsForbiddenTarget;
  }

  private failsNotEqualTo(evaluation: ValidationRuleEvaluation): boolean {
    const target = evaluation.comparisonTarget();
    if (!target.isAvailable) return true;
    const valueWasProvided = !evaluation.subject.isEmpty;
    const valueEqualsPeerTarget = evaluation.subject.equalsTarget(target, evaluation.comparisonShape);
    return valueWasProvided && valueEqualsPeerTarget;
  }
}

class AtLeastOneRuleOperation implements RuleOperation {
  fails(evaluation: ValidationRuleEvaluation): boolean {
    return evaluation.subject.failsAtLeastOne();
  }
}

function luhn(digits: string): boolean {
  if (digits.length < 13) return false;
  let sum = 0;
  let alt = false;
  for (let i = digits.length - 1; i >= 0; i--) {
    const digit = digits[i];
    if (digit === undefined) return false;
    let n = parseInt(digit, 10);
    if (alt) {
      n *= 2;
      if (n > 9) n -= 9;
    }
    sum += n;
    alt = !alt;
  }
  return sum % 10 === 0;
}
