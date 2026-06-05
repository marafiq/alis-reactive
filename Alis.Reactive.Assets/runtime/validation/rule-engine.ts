// Pure rule evaluation; orchestrator resolves peers and owns DOM/vendor side effects.

import { assertNever } from "../shared/assert-never";
import type {
  LengthValidationRule,
  LiteralEqualityValidationRule,
  NoOperandValidationRule,
  OrderedComparisonValidationRule,
  PeerEqualityValidationRule,
  PeerOrderedComparisonValidationRule,
  RangeValidationRule,
  RegexValidationRule,
  Shape,
  ValidationRule,
  ValidationRuleName,
} from "../types/index";
import {
  ValidationLengthConstraint,
  ValidationRangeTarget,
  ValidationScalarTarget,
  ValidationSubject,
} from "./rule-operands";

type NonPeerValidationRule =
  | NoOperandValidationRule
  | LengthValidationRule
  | RegexValidationRule
  | RangeValidationRule
  | OrderedComparisonValidationRule
  | LiteralEqualityValidationRule;
type PeerTargetValidationRule =
  | PeerEqualityValidationRule
  | PeerOrderedComparisonValidationRule;
type EqualityRuleEvaluation =
  | { readonly rule: LiteralEqualityValidationRule; readonly value: unknown }
  | { readonly rule: PeerEqualityValidationRule; readonly value: unknown; readonly peerValue: unknown };
type OrderedRuleEvaluation =
  | { readonly rule: OrderedComparisonValidationRule; readonly value: unknown }
  | { readonly rule: PeerOrderedComparisonValidationRule; readonly value: unknown; readonly peerValue: unknown };

export type RuleEvaluation =
  | { readonly rule: NonPeerValidationRule; readonly value: unknown }
  | { readonly rule: PeerTargetValidationRule; readonly value: unknown; readonly peerValue: unknown };

export function ruleFails(evaluation: RuleEvaluation): boolean {
  const subject = ValidationSubject.from(evaluation.value);
  if ("peerValue" in evaluation) return peerTargetRuleFails(evaluation, subject);

  return fieldRuleFails(evaluation, subject);
}

function fieldRuleFails(
  evaluation: { readonly rule: NonPeerValidationRule; readonly value: unknown },
  subject: ValidationSubject,
): boolean {
  switch (evaluation.rule.name) {
    case "required": return requiredFails(subject);
    case "empty": return emptyFails(subject);
    case "minLength":
    case "maxLength":
      return lengthRuleFails(evaluation.rule.name, subject, evaluation.rule);
    case "email": return emailFails(subject);
    case "regex": return regexFails(subject, evaluation.rule);
    case "url": return urlFails(subject);
    case "creditCard": return creditCardFails(subject);
    case "min":
    case "max":
    case "gt":
    case "lt":
      return orderedComparisonFails({ rule: evaluation.rule, value: evaluation.value }, subject);
    case "range":
    case "exclusiveRange":
      return rangeFails(evaluation.rule.name, subject, evaluation.rule);
    case "equalTo":
    case "notEqual":
      return equalityFails({ rule: evaluation.rule, value: evaluation.value }, subject);
    case "atLeastOne": return atLeastOneFails(subject);
    default: return assertNever(evaluation.rule, "validation rule");
  }
}

function peerTargetRuleFails(
  evaluation: { readonly rule: PeerTargetValidationRule; readonly value: unknown; readonly peerValue: unknown },
  subject: ValidationSubject,
): boolean {
  switch (evaluation.rule.name) {
    case "min":
    case "max":
    case "gt":
    case "lt":
      return orderedComparisonFails({
        rule: evaluation.rule,
        value: evaluation.value,
        peerValue: evaluation.peerValue,
      }, subject);
    case "equalTo":
    case "notEqualTo":
      return equalityFails({
        rule: evaluation.rule,
        value: evaluation.value,
        peerValue: evaluation.peerValue,
      }, subject);
    default: return assertNever(evaluation.rule, "peer validation rule");
  }
}

type LengthRuleName = Extract<ValidationRuleName, "minLength" | "maxLength">;
type RangeRuleName = Extract<ValidationRuleName, "range" | "exclusiveRange">;
function constraint(rule: OrderedComparisonValidationRule | RegexValidationRule): ValidationScalarTarget {
  return ValidationScalarTarget.fromLiteral(rule.execution.value);
}

function comparisonShape(rule: ValidationRule): Shape {
  return rule.execution.comparisonShape;
}

function lengthConstraint(rule: LengthValidationRule): ValidationLengthConstraint {
  return ValidationLengthConstraint.fromLiteral(rule.execution.value);
}

function rangeTarget(rule: RangeValidationRule): ValidationRangeTarget {
  return ValidationRangeTarget.fromLiteral(rule.execution.value);
}

function requiredFails(subject: ValidationSubject): boolean {
  return subject.isEmpty;
}

function emptyFails(subject: ValidationSubject): boolean {
  return !subject.isEmpty;
}

function lengthRuleFails(
  ruleName: LengthRuleName,
  subject: ValidationSubject,
  rule: LengthValidationRule,
): boolean {
  if (subject.isEmpty) return false;

  const actualLength = subject.length;
  const expectedLength = lengthConstraint(rule);

  if (ruleName === "minLength") return expectedLength.isGreaterThan(actualLength);
  return expectedLength.isLessThan(actualLength);
}

function emailFails(subject: ValidationSubject): boolean {
  const valueWasProvided = !subject.isEmpty;
  const valueLooksLikeEmail = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(subject.text);
  return valueWasProvided && !valueLooksLikeEmail;
}

function regexFails(subject: ValidationSubject, rule: RegexValidationRule): boolean {
  const pattern = constraint(rule).textOrEmpty();
  try {
    return !subject.isEmpty && !new RegExp(pattern).test(subject.text);
  } catch {
    return true;
  }
}

function urlFails(subject: ValidationSubject): boolean {
  const valueWasProvided = !subject.isEmpty;
  const valueLooksLikeUrl = /^https?:\/\/.+/.test(subject.text);
  return valueWasProvided && !valueLooksLikeUrl;
}

function creditCardFails(subject: ValidationSubject): boolean {
  const valueWasProvided = !subject.isEmpty;
  const valuePassesLuhn = luhn(subject.text.replace(/\D/g, ""));
  return valueWasProvided && !valuePassesLuhn;
}

function orderedComparisonFails(
  evaluation: OrderedRuleEvaluation,
  subject: ValidationSubject,
): boolean {
  const comparisonTarget = "peerValue" in evaluation
    ? ValidationScalarTarget.available(evaluation.peerValue)
    : constraint(evaluation.rule);
  const comparison = subject.compareTo(comparisonTarget, comparisonShape(evaluation.rule));
  const valueIsEmpty = subject.isEmpty;

  switch (evaluation.rule.name) {
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
    default: return assertNever(evaluation.rule, "ordered validation comparison");
  }
}

function rangeFails(ruleName: RangeRuleName, subject: ValidationSubject, rule: RangeValidationRule): boolean {
  const range = rangeTarget(rule);
  if (subject.isEmpty) return false;

  const lowerComparison = subject.compareTo(range.lowerBound, comparisonShape(rule));
  const upperComparison = subject.compareTo(range.upperBound, comparisonShape(rule));
  const rangeCannotBeCompared = lowerComparison.cannotCompare || upperComparison.cannotCompare;
  if (rangeCannotBeCompared) return true;

  if (ruleName === "range") {
    const valueFallsOutsideInclusiveRange =
      lowerComparison.lessThanTarget || upperComparison.greaterThanTarget;
    return valueFallsOutsideInclusiveRange;
  }

  const valueFallsOutsideExclusiveRange =
    lowerComparison.lessThanTarget || lowerComparison.equalToTarget ||
    upperComparison.greaterThanTarget || upperComparison.equalToTarget;
  return valueFallsOutsideExclusiveRange;
}

function equalityFails(evaluation: EqualityRuleEvaluation, subject: ValidationSubject): boolean {
  if ("peerValue" in evaluation) {
    const peerEqualityTarget = ValidationScalarTarget.available(evaluation.peerValue);
    if (evaluation.rule.name === "notEqualTo") {
      return notEqualToFails(subject, peerEqualityTarget, comparisonShape(evaluation.rule));
    }

    return equalToFails(subject, peerEqualityTarget, comparisonShape(evaluation.rule));
  }

  const literalEqualityTarget = ValidationScalarTarget.fromLiteral(evaluation.rule.execution.value);
  if (evaluation.rule.name === "notEqual") {
    return notEqualFails(subject, literalEqualityTarget, comparisonShape(evaluation.rule));
  }

  return equalToFails(subject, literalEqualityTarget, comparisonShape(evaluation.rule));
}

function equalToFails(
  subject: ValidationSubject,
  target: ValidationScalarTarget,
  shape: Shape,
): boolean {
  if (subject.isEmpty) return false;

  return !subject.equalsTarget(target, shape);
}

function notEqualFails(
  subject: ValidationSubject,
  target: ValidationScalarTarget,
  shape: Shape,
): boolean {
  const valueWasProvided = !subject.isEmpty;
  const valueEqualsForbiddenTarget = subject.equalsTarget(target, shape);
  return valueWasProvided && valueEqualsForbiddenTarget;
}

function notEqualToFails(
  subject: ValidationSubject,
  target: ValidationScalarTarget,
  shape: Shape,
): boolean {
  const valueWasProvided = !subject.isEmpty;
  const valueEqualsPeerTarget = subject.equalsTarget(target, shape);
  return valueWasProvided && valueEqualsPeerTarget;
}

function atLeastOneFails(subject: ValidationSubject): boolean {
  return subject.failsAtLeastOne();
}

function luhn(digits: string): boolean {
  if (digits.length < 13) return false;
  let sum = 0;
  let doubleNextDigit = false;
  for (let digitIndex = digits.length - 1; digitIndex >= 0; digitIndex--) {
    const digit = digits[digitIndex];
    if (digit === undefined) return false;
    let digitValue = parseInt(digit, 10);
    if (doubleNextDigit) {
      digitValue *= 2;
      if (digitValue > 9) digitValue -= 9;
    }
    sum += digitValue;
    doubleNextDigit = !doubleNextDigit;
  }
  return sum % 10 === 0;
}
