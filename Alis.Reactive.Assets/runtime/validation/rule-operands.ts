import { toString } from "../core/shape-convert";
import type { ConvertResult } from "../core/shape-convert";
import { RuntimeShape } from "../domain/runtime-shape";
import type {
  Shape,
  LiteralProducer,
  NumericLiteralProducer,
  RangeLiteralProducer,
} from "../types";

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
    const valueIsMissing = isMissingValidationValue(this.raw);
    const valueIsFalse = this.raw === false;
    const valueIsEmptyString = this.text === "";
    const valueIsEmptyArray = isEmptyValidationCollection(this.raw);
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

export class ValidationScalarTarget {
  private constructor(private readonly value: unknown) {}

  static fromLiteral(literal: LiteralProducer): ValidationScalarTarget {
    return ValidationScalarTarget.available(literal.value);
  }

  static available(value: unknown): ValidationScalarTarget {
    return new ValidationScalarTarget(value);
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
    return valuesMatchShape(subject, this.value, shape);
  }
}

export class ValidationLengthConstraint {
  private constructor(private readonly expectedLength: number) {}

  static fromLiteral(literal: NumericLiteralProducer): ValidationLengthConstraint {
    return new ValidationLengthConstraint(literal.value);
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

    const leftValue = comparableValidationNumber(left, comparisonShape);
    const rightValue = comparableValidationNumber(right, comparisonShape);
    const operandsAreComparable = leftValue !== undefined && rightValue !== undefined;
    if (!operandsAreComparable) return ShapedComparison.missing();

    return new ShapedComparison(leftValue - rightValue);
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

function comparableValidationNumber(raw: unknown, shape: RuntimeShape): number | undefined {
  const converted = shape.convert(raw);
  if (!converted.ok) return undefined;
  if (typeof converted.value !== "number") return undefined;
  if (!Number.isFinite(converted.value)) return undefined;

  return converted.value;
}

function valuesMatchShape(left: unknown, right: unknown, shape: Shape): boolean {
  const comparisonShape = RuntimeShape.from(shape);
  if (comparisonShape.isDeclared) {
    return comparisonShape.apply(left) === comparisonShape.apply(right);
  }

  const normalizedLeft = textOrEmpty(left);
  const normalizedRight = textOrEmpty(right);
  return normalizedLeft === normalizedRight;
}

function textOrEmpty(value: unknown): string {
  const text = toString(value);
  if (text.ok) return text.value;
  return "";
}

function isMissingValidationValue(value: unknown): boolean {
  const valueIsNull = value === null;
  const valueIsUndefined = value === undefined;
  return valueIsNull || valueIsUndefined;
}

function isEmptyValidationCollection(value: unknown): boolean {
  const valueIsCollection = Array.isArray(value);
  if (!valueIsCollection) return false;

  return value.length === 0;
}

export class ValidationRangeTarget {
  private constructor(
    readonly lowerBound: ValidationScalarTarget,
    readonly upperBound: ValidationScalarTarget,
  ) {}

  static fromLiteral(literal: RangeLiteralProducer): ValidationRangeTarget {
    const [lower, upper] = literal.value;

    return new ValidationRangeTarget(
      ValidationScalarTarget.available(lower),
      ValidationScalarTarget.available(upper),
    );
  }
}
