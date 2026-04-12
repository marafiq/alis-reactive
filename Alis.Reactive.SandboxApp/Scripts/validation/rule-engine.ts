// Rule Engine — Pure validation rule evaluation
// No DOM, no vendor, no side effects. Takes a value + ValidationRule → pass/fail.
// Uses Shape for ALL type-aware comparisons via convertByShape from shape-convert.
// otherValue (for peer comparisons) is pre-resolved by the orchestrator.

import { convertByShape, applyShape, toString } from "../core/shape-convert";
import type { ValidationRule, Shape } from "../types";

function compareValues(a: unknown, b: unknown, shape: Shape): number {
  const ra = convertByShape(a, shape);
  const rb = convertByShape(b, shape);
  if (!ra.ok || !rb.ok) return NaN;
  const ca = ra.value as number;
  const cb = rb.value as number;
  if (Number.isNaN(ca) || Number.isNaN(cb)) return NaN;
  return ca - cb;
}

function resolveTarget(rule: ValidationRule, otherValue?: unknown): unknown {
  if (otherValue !== undefined) return otherValue;
  if (rule.constraint.kind === "literal") return rule.constraint.value;
  return undefined;
}

function failsComparisonRule(
  rule: ValidationRule, value: unknown, empty: boolean, otherValue?: unknown,
): boolean {
  const target = resolveTarget(rule, otherValue);
  if (target === undefined) return true;
  if (rule.shape.kind === "none") return true;
  const cmp = compareValues(value, target, rule.shape);
  switch (rule.name) {
    case "min": return !empty && (Number.isNaN(cmp) || cmp < 0);
    case "max": return !empty && (Number.isNaN(cmp) || cmp > 0);
    case "gt":  return empty || Number.isNaN(cmp) || cmp <= 0;
    case "lt":  return !empty && (Number.isNaN(cmp) || cmp >= 0);
    default:    return true;
  }
}

function failsRangeRule(rule: ValidationRule, value: unknown, empty: boolean): boolean {
  if (rule.constraint.kind !== "literal") return true;
  const arr = rule.constraint.value;
  if (!Array.isArray(arr) || arr.length < 2) return true;
  if (empty) return false;
  if (rule.shape.kind === "none") return true;
  const [lo, hi] = arr;
  const cmpLo = compareValues(value, lo, rule.shape);
  const cmpHi = compareValues(value, hi, rule.shape);
  if (Number.isNaN(cmpLo) || Number.isNaN(cmpHi)) return true;
  if (rule.name === "range") return cmpLo < 0 || cmpHi > 0;
  return cmpLo <= 0 || cmpHi >= 0;
}

/** Shape-aware equality — applies shape to both sides and uses strict equality. */
function shapeEqual(a: unknown, b: unknown, shape: Shape): boolean {
  if (shape.kind !== "none") {
    const ca = applyShape(a, shape);
    const cb = applyShape(b, shape);
    return ca === cb;
  }
  const sa = toString(a); const sb = toString(b);
  return (sa.ok ? sa.value : "") === (sb.ok ? sb.value : "");
}

function failsEqualityRule(
  rule: ValidationRule, value: unknown, empty: boolean, otherValue?: unknown,
): boolean {
  switch (rule.name) {
    case "equalTo": {
      if (empty) return false;
      const target = resolveTarget(rule, otherValue);
      if (target === undefined) return true;
      return !shapeEqual(value, target, rule.shape);
    }
    case "notEqual": {
      const constraint = rule.constraint.kind === "literal" ? rule.constraint.value : undefined;
      return !empty && shapeEqual(value, constraint, rule.shape);
    }
    case "notEqualTo": {
      const target = resolveTarget(rule, otherValue);
      if (target === undefined) return true;
      return !empty && shapeEqual(value, target, rule.shape);
    }
    default: return true;
  }
}

export function ruleFails(
  rule: ValidationRule,
  value: unknown,
  otherValue?: unknown,
): boolean {
  const strResult = toString(value);
  const str = strResult.ok ? strResult.value : "";
  const empty = value == null || str === "" || value === false
    || !strResult.ok
    || (Array.isArray(value) && value.length === 0);

  switch (rule.name) {
    case "required":    return empty;
    case "empty":       return !empty;
    case "minLength":   return !empty && str.length < Number(getConstraintValue(rule));
    case "maxLength":   return !empty && str.length > Number(getConstraintValue(rule));
    case "email":       return !empty && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(str);
    case "regex": {
      const constraint = String(getConstraintValue(rule) ?? "");
      try { return !empty && !new RegExp(constraint).test(str); }
      catch { return true; }
    }
    case "url":         return !empty && !/^https?:\/\/.+/.test(str);
    case "creditCard":  return !empty && !luhn(str.replace(/\D/g, ""));
    case "min": case "max": case "gt": case "lt":
      return failsComparisonRule(rule, value, empty, otherValue);
    case "range": case "exclusiveRange":
      return failsRangeRule(rule, value, empty);
    case "equalTo": case "notEqual": case "notEqualTo":
      return failsEqualityRule(rule, value, empty, otherValue);
    case "atLeastOne":  return Array.isArray(value) ? value.length === 0 : empty;
    default:            return true;
  }
}

function getConstraintValue(rule: ValidationRule): unknown {
  if (rule.constraint.kind === "literal") return rule.constraint.value;
  return undefined;
}

function luhn(digits: string): boolean {
  if (digits.length < 13) return false;
  let sum = 0;
  let alt = false;
  for (let i = digits.length - 1; i >= 0; i--) {
    let n = parseInt(digits[i], 10);
    if (alt) { n *= 2; if (n > 9) n -= 9; }
    sum += n;
    alt = !alt;
  }
  return sum % 10 === 0;
}
