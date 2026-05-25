// Rule Engine - pure validation rule evaluation.
// No DOM, no vendor, no side effects. The orchestrator resolves peers first.

import { assertNever } from "../core/assert-never";
import type { ValidationRule, ValidationRuleName, Shape } from "../types";
import {
  ValidationLengthConstraint,
  ValidationRangeTarget,
  ValidationScalarTarget,
  ValidationSubject,
} from "./rule-operands";
import type { ResolvedPeerValue } from "./rule-operands";

export type { ResolvedPeerValue } from "./rule-operands";

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
