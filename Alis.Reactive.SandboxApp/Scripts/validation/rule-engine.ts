// Rule Engine — Pure validation rule evaluation
//
// No DOM, no vendor, no side effects. Takes a value and rule → pass/fail.
// Used by validation.ts orchestrator. Testable without jsdom.

import { coerce, toString } from "../core/coerce";
import type { CoercionType } from "../core/coerce";
import type { ValidationRule } from "../types";

/** Reads a peer field's raw value for cross-property comparisons. */
export interface PeerReader {
  readPeer(fieldName: string): unknown;
}

function compareValues(a: unknown, b: unknown, coerceAs: CoercionType | undefined): number {
  if (!coerceAs) {
    throw new Error(
      "[alis:validation] compareValues called without coerceAs. " +
      "Comparison rules (min, max, gt, lt, range, exclusiveRange) require explicit coerceAs in the plan. " +
      "The adapter must set coerceAs from the property type."
    );
  }
  const ra = coerce(a, coerceAs);
  const rb = coerce(b, coerceAs);
  if (!ra.ok || !rb.ok) return NaN; // coerce failed → comparison undefined
  const ca = ra.value as number;
  const cb = rb.value as number;
  if (Number.isNaN(ca) || Number.isNaN(cb)) return NaN;
  return ca - cb;
}

function resolveTarget(rule: ValidationRule, peerReader: PeerReader): unknown {
  if (rule.field) {
    const peer = peerReader.readPeer(rule.field);
    if (peer == null) return undefined;
    return peer;
  }
  return rule.constraint;
}

// --- Extracted evaluators (pure functions, no DOM, no side effects) ---

function failsComparisonRule(
  rule: ValidationRule, value: unknown, empty: boolean, peerReader: PeerReader
): boolean {
  const target = resolveTarget(rule, peerReader);
  if (target === undefined) return true;
  switch (rule.rule) {
    case "min": return !empty && compareValues(value, target, rule.coerceAs) < 0;
    case "max": return !empty && compareValues(value, target, rule.coerceAs) > 0;
    case "gt":  return empty || compareValues(value, target, rule.coerceAs) <= 0;
    case "lt":  return !empty && compareValues(value, target, rule.coerceAs) >= 0;
    default:    return true;
  }
}

function failsRangeRule(
  rule: ValidationRule, value: unknown, empty: boolean
): boolean {
  const [lo, hi] = rule.constraint as [unknown, unknown];
  if (empty) return false;
  if (rule.rule === "range") {
    return compareValues(value, lo, rule.coerceAs) < 0
        || compareValues(value, hi, rule.coerceAs) > 0;
  }
  // exclusiveRange
  return compareValues(value, lo, rule.coerceAs) <= 0
      || compareValues(value, hi, rule.coerceAs) >= 0;
}

function failsEqualityRule(
  rule: ValidationRule, value: unknown, empty: boolean, peerReader: PeerReader
): boolean {
  switch (rule.rule) {
    case "equalTo": {
      if (empty) return false;
      const target = resolveTarget(rule, peerReader);
      if (target === undefined) return true;
      if (rule.coerceAs) return compareValues(value, target, rule.coerceAs) !== 0;
      const sv = toString(value); const tv = toString(target);
      return (sv.ok ? sv.value : "") !== (tv.ok ? tv.value : "");
    }
    case "notEqual": {
      const sv = toString(value); const tv = toString(rule.constraint);
      return !empty && (sv.ok ? sv.value : "") === (tv.ok ? tv.value : "");
    }
    case "notEqualTo": {
      const target = resolveTarget(rule, peerReader);
      if (target === undefined) return true;
      if (rule.coerceAs) return !empty && compareValues(value, target, rule.coerceAs) === 0;
      const sv = toString(value); const tv = toString(target);
      return !empty && (sv.ok ? sv.value : "") === (tv.ok ? tv.value : "");
    }
    default: return true;
  }
}

// --- Main dispatcher ---

export function ruleFails(
  rule: ValidationRule,
  value: unknown,
  peerReader: PeerReader
): boolean {
  const strResult = toString(value);
  const str = strResult.ok ? strResult.value : "";
  const empty = value == null || str === "" || value === false
    || (Array.isArray(value) && value.length === 0);

  switch (rule.rule) {
    case "required":    return empty;
    case "empty":       return !empty;
    case "minLength":   return !empty && str.length < Number(rule.constraint);
    case "maxLength":   return !empty && str.length > Number(rule.constraint);
    case "email":       return !empty && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(str);
    case "regex": {
      const constraintResult = toString(rule.constraint);
      const constraint = constraintResult.ok ? constraintResult.value : "";
      try { return !empty && !new RegExp(constraint).test(str); }
      catch { return true; }
    }
    case "url":         return !empty && !/^https?:\/\/.+/.test(str);
    case "creditCard":  return !empty && !luhn(str.replace(/\D/g, ""));

    case "min": case "max": case "gt": case "lt":
      return failsComparisonRule(rule, value, empty, peerReader);

    case "range": case "exclusiveRange":
      return failsRangeRule(rule, value, empty);

    case "equalTo": case "notEqual": case "notEqualTo":
      return failsEqualityRule(rule, value, empty, peerReader);

    case "atLeastOne":  return Array.isArray(value) ? value.length === 0 : empty;
    default:            return true;
  }
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
