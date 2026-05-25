import { assertNever } from "../core/assert-never";
import { toString } from "../core/shape-convert";
import type { ConvertResult } from "../core/shape-convert";
import { RuntimeShape } from "../domain/runtime-shape";
import type {
  Shape,
  ValueProducer,
  ValidationRuleOperand as PlanValidationRuleOperand,
} from "../types";

export type ResolvedPeerValue =
  | { readonly kind: "present"; readonly value: unknown }
  | { readonly kind: "absent" };

export class ValidationSubject {
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

export abstract class ValidationScalarTarget {
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

export class ValidationLengthConstraint {
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

export class ShapedComparison {
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

export abstract class ValidationRangeTarget {
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
