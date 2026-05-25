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
  const ruleEvaluation = prepareValidationRuleEvaluation(evaluation);

  switch (ruleEvaluation.rule.name) {
    case "required": return requiredFails(ruleEvaluation);
    case "empty": return emptyFails(ruleEvaluation);
    case "minLength":
    case "maxLength":
      return lengthRuleFails(ruleEvaluation.rule.name, ruleEvaluation);
    case "email": return emailFails(ruleEvaluation);
    case "regex": return regexFails(ruleEvaluation);
    case "url": return urlFails(ruleEvaluation);
    case "creditCard": return creditCardFails(ruleEvaluation);
    case "min":
    case "max":
    case "gt":
    case "lt":
      return orderedComparisonFails(ruleEvaluation.rule.name, ruleEvaluation);
    case "range":
    case "exclusiveRange":
      return rangeFails(ruleEvaluation.rule.name, ruleEvaluation);
    case "equalTo":
    case "notEqual":
    case "notEqualTo":
      return equalityFails(ruleEvaluation.rule.name, ruleEvaluation);
    case "atLeastOne": return atLeastOneFails(ruleEvaluation);
    default: return assertNever(ruleEvaluation.rule.name, "validation rule");
  }
}

interface ValidationRuleEvaluation {
  readonly rule: ValidationRule;
  readonly subject: ValidationSubject;
  readonly peerValue: ResolvedPeerValue;
}

function prepareValidationRuleEvaluation(evaluation: RuleEvaluation): ValidationRuleEvaluation {
  return {
    rule: evaluation.rule,
    subject: ValidationSubject.from(evaluation.value),
    peerValue: evaluation.peerValue,
  };
}

type LengthRuleName = Extract<ValidationRuleName, "minLength" | "maxLength">;
type OrderedComparisonRuleName = Extract<ValidationRuleName, "min" | "max" | "gt" | "lt">;
type RangeRuleName = Extract<ValidationRuleName, "range" | "exclusiveRange">;
type EqualityRuleName = Extract<ValidationRuleName, "equalTo" | "notEqual" | "notEqualTo">;

function comparisonTarget(evaluation: ValidationRuleEvaluation): ValidationScalarTarget {
  return ValidationScalarTarget.fromResolvedPeerOrConstraint(
    evaluation.peerValue,
    evaluation.rule.execution.constraint,
  );
}

function constraint(evaluation: ValidationRuleEvaluation): ValidationScalarTarget {
  return ValidationScalarTarget.fromConstraintOperand(evaluation.rule.execution.constraint);
}

function comparisonShape(evaluation: ValidationRuleEvaluation): Shape {
  return evaluation.rule.execution.comparisonShape;
}

function lengthConstraint(evaluation: ValidationRuleEvaluation): ValidationLengthConstraint {
  return ValidationLengthConstraint.fromOperand(evaluation.rule.execution.constraint);
}

function rangeTarget(evaluation: ValidationRuleEvaluation): ValidationRangeTarget {
  return ValidationRangeTarget.fromOperand(evaluation.rule.execution.constraint);
}

function requiredFails(evaluation: ValidationRuleEvaluation): boolean {
  return evaluation.subject.isEmpty;
}

function emptyFails(evaluation: ValidationRuleEvaluation): boolean {
  return !evaluation.subject.isEmpty;
}

function lengthRuleFails(ruleName: LengthRuleName, evaluation: ValidationRuleEvaluation): boolean {
  if (evaluation.subject.isEmpty) return false;

  const actualLength = evaluation.subject.length;
  const expectedLength = lengthConstraint(evaluation);

  if (ruleName === "minLength") return expectedLength.isGreaterThan(actualLength);
  return expectedLength.isLessThan(actualLength);
}

function emailFails(evaluation: ValidationRuleEvaluation): boolean {
  const valueWasProvided = !evaluation.subject.isEmpty;
  const valueLooksLikeEmail = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(evaluation.subject.text);
  return valueWasProvided && !valueLooksLikeEmail;
}

function regexFails(evaluation: ValidationRuleEvaluation): boolean {
  const pattern = constraint(evaluation).textOrEmpty();
  try {
    return !evaluation.subject.isEmpty && !new RegExp(pattern).test(evaluation.subject.text);
  } catch {
    return true;
  }
}

function urlFails(evaluation: ValidationRuleEvaluation): boolean {
  const valueWasProvided = !evaluation.subject.isEmpty;
  const valueLooksLikeUrl = /^https?:\/\/.+/.test(evaluation.subject.text);
  return valueWasProvided && !valueLooksLikeUrl;
}

function creditCardFails(evaluation: ValidationRuleEvaluation): boolean {
  const valueWasProvided = !evaluation.subject.isEmpty;
  const valuePassesLuhn = luhn(evaluation.subject.text.replace(/\D/g, ""));
  return valueWasProvided && !valuePassesLuhn;
}

function orderedComparisonFails(
  ruleName: OrderedComparisonRuleName,
  evaluation: ValidationRuleEvaluation,
): boolean {
  const target = comparisonTarget(evaluation);
  const comparison = evaluation.subject.compareTo(target, comparisonShape(evaluation));
  const valueIsEmpty = evaluation.subject.isEmpty;

  switch (ruleName) {
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
    default: return assertNever(ruleName, "ordered validation comparison");
  }
}

function rangeFails(ruleName: RangeRuleName, evaluation: ValidationRuleEvaluation): boolean {
  const range = rangeTarget(evaluation);
  if (!range.isAvailable) return true;
  if (evaluation.subject.isEmpty) return false;

  const lowerComparison = evaluation.subject.compareTo(range.lowerBound, comparisonShape(evaluation));
  const upperComparison = evaluation.subject.compareTo(range.upperBound, comparisonShape(evaluation));
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

function equalityFails(ruleName: EqualityRuleName, evaluation: ValidationRuleEvaluation): boolean {
  switch (ruleName) {
    case "equalTo": return equalToFails(evaluation);
    case "notEqual": return notEqualFails(evaluation);
    case "notEqualTo": return notEqualToFails(evaluation);
    default: return assertNever(ruleName, "equality validation comparison");
  }
}

function equalToFails(evaluation: ValidationRuleEvaluation): boolean {
  if (evaluation.subject.isEmpty) return false;

  const target = comparisonTarget(evaluation);
  if (!target.isAvailable) return true;
  return !evaluation.subject.equalsTarget(target, comparisonShape(evaluation));
}

function notEqualFails(evaluation: ValidationRuleEvaluation): boolean {
  const target = constraint(evaluation);
  if (!target.isAvailable) return false;
  const valueWasProvided = !evaluation.subject.isEmpty;
  const valueEqualsForbiddenTarget = evaluation.subject.equalsTarget(target, comparisonShape(evaluation));
  return valueWasProvided && valueEqualsForbiddenTarget;
}

function notEqualToFails(evaluation: ValidationRuleEvaluation): boolean {
  const target = comparisonTarget(evaluation);
  if (!target.isAvailable) return true;
  const valueWasProvided = !evaluation.subject.isEmpty;
  const valueEqualsPeerTarget = evaluation.subject.equalsTarget(target, comparisonShape(evaluation));
  return valueWasProvided && valueEqualsPeerTarget;
}

function atLeastOneFails(evaluation: ValidationRuleEvaluation): boolean {
  return evaluation.subject.failsAtLeastOne();
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
