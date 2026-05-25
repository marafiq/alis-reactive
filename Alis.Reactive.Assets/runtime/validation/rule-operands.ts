import { assertNever } from "../core/assert-never";
import { toString } from "../core/shape-convert";
import type { ConvertResult } from "../core/shape-convert";
import { RuntimeShape } from "../domain/runtime-shape";
import type {
  Shape,
  LiteralValidationConstraintOperand,
  NumericValidationConstraintOperand,
  RangeValidationConstraintOperand,
  ValidationConstraintOperand,
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
    constraintOperand: ValidationConstraintOperand,
  ): ValidationScalarTarget {
    if (peerValue.kind === "present") return ValidationScalarTarget.available(peerValue.value);
    return ValidationScalarTarget.fromConstraintOperand(constraintOperand);
  }

  static fromConstraintOperand(operand: ValidationConstraintOperand): ValidationScalarTarget {
    switch (operand.kind) {
      case "none": return ValidationScalarTarget.missing();
      case "value": return ValidationScalarTarget.fromLiteral(operand);
      default: return assertNever(operand, "validation rule operand");
    }
  }

  static available(value: unknown): ValidationScalarTarget {
    return new AvailableValidationScalarTarget(value);
  }

  private static fromLiteral(operand: LiteralValidationConstraintOperand): ValidationScalarTarget {
    return ValidationScalarTarget.available(operand.value.value);
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

  static fromOperand(operand: NumericValidationConstraintOperand): ValidationLengthConstraint {
    return new ValidationLengthConstraint(operand.value.value);
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

export class ValidationRangeTarget {
  private constructor(
    readonly lowerBound: ValidationScalarTarget,
    readonly upperBound: ValidationScalarTarget,
  ) {}

  static fromOperand(operand: RangeValidationConstraintOperand): ValidationRangeTarget {
    const [lower, upper] = operand.value.value;

    return new ValidationRangeTarget(
      ValidationScalarTarget.available(lower),
      ValidationScalarTarget.available(upper),
    );
  }
}
