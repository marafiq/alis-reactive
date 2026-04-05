// Rule Engine — Pure validation rule evaluation
// No DOM, no vendor, no side effects. Takes a value + ValidationRule → pass/fail.
// Uses Shape for ALL type-aware comparisons via convertByShape from shape-convert.

import { convertByShape, toString } from "../core/shape-convert";
import type { ValidationRule, Shape } from "../types";

export interface PeerReader {
  readPeer(componentKey: string): unknown;
}

function compareValues(a: unknown, b: unknown, shape: Shape): number {
  const ra = convertByShape(a, shape);
  const rb = convertByShape(b, shape);
  if (!ra.ok || !rb.ok) return NaN;
  const ca = ra.value as number;
  const cb = rb.value as number;
  if (Number.isNaN(ca) || Number.isNaN(cb)) return NaN;
  return ca - cb;
}

function resolveTarget(rule: ValidationRule, peerReader: PeerReader): unknown {
  if (rule.otherComponent) {
    return peerReader.readPeer(rule.otherComponent) ?? undefined;
  }
  if (rule.constraint?.kind === "literal") return rule.constraint.value;
  return undefined;
}

function failsComparisonRule(
  rule: ValidationRule, value: unknown, empty: boolean, peerReader: PeerReader,
): boolean {
  const target = resolveTarget(rule, peerReader);
  if (target === undefined) return true;
  if (!rule.shape) return true; // comparison requires shape
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
  if (!rule.constraint || rule.constraint.kind !== "literal") return true;
  const arr = rule.constraint.value;
  if (!Array.isArray(arr) || arr.length < 2) return true;
  if (empty) return false;
  if (!rule.shape) return true;
  const [lo, hi] = arr;
  const cmpLo = compareValues(value, lo, rule.shape);
  const cmpHi = compareValues(value, hi, rule.shape);
  if (Number.isNaN(cmpLo) || Number.isNaN(cmpHi)) return true;
  if (rule.name === "range") return cmpLo < 0 || cmpHi > 0;
  return cmpLo <= 0 || cmpHi >= 0;
}

function failsEqualityRule(
  rule: ValidationRule, value: unknown, empty: boolean, peerReader: PeerReader,
): boolean {
  switch (rule.name) {
    case "equalTo": {
      if (empty) return false;
      const target = resolveTarget(rule, peerReader);
      if (target === undefined) return true;
      if (rule.shape) return compareValues(value, target, rule.shape) !== 0;
      const sv = toString(value); const tv = toString(target);
      return (sv.ok ? sv.value : "") !== (tv.ok ? tv.value : "");
    }
    case "notEqual": {
      const constraint = rule.constraint?.kind === "literal" ? rule.constraint.value : undefined;
      const sv = toString(value); const tv = toString(constraint);
      return !empty && (sv.ok ? sv.value : "") === (tv.ok ? tv.value : "");
    }
    case "notEqualTo": {
      const target = resolveTarget(rule, peerReader);
      if (target === undefined) return true;
      if (rule.shape) return !empty && compareValues(value, target, rule.shape) === 0;
      const sv = toString(value); const tv = toString(target);
      return !empty && (sv.ok ? sv.value : "") === (tv.ok ? tv.value : "");
    }
    default: return true;
  }
}

export function ruleFails(
  rule: ValidationRule,
  value: unknown,
  peerReader: PeerReader,
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
      return failsComparisonRule(rule, value, empty, peerReader);
    case "range": case "exclusiveRange":
      return failsRangeRule(rule, value, empty);
    case "equalTo": case "notEqual": case "notEqualTo":
      return failsEqualityRule(rule, value, empty, peerReader);
    case "atLeastOne":  return Array.isArray(value) ? value.length === 0 : empty;
    default:            return true;
  }
}

function getConstraintValue(rule: ValidationRule): unknown {
  if (!rule.constraint) return undefined;
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
