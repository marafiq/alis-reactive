// Rule Engine — Pure validation rule evaluation
//
// No DOM, no vendor, no side effects. Takes a value and rule → pass/fail.
// Used by validation.ts orchestrator. Testable without jsdom.

import { coerce } from "../core/coerce";
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
  const ca = coerce(a, coerceAs) as number;
  const cb = coerce(b, coerceAs) as number;
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

export function ruleFails(
  rule: ValidationRule,
  value: unknown,
  peerReader: PeerReader
): boolean {
  const str = value == null ? "" : String(value);
  const empty = value == null || str === "" || value === false;

  switch (rule.rule) {
    case "required":
      return empty;
    case "empty":
      return !empty;
    case "minLength":
      return !empty && str.length < Number(rule.constraint);
    case "maxLength":
      return !empty && str.length > Number(rule.constraint);
    case "email":
      return !empty && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(str);
    case "regex": {
      try { return !empty && !new RegExp(String(rule.constraint)).test(str); }
      catch { return true; } // Broken regex → fail-closed (block, don't pass)
    }
    case "url":
      return !empty && !/^https?:\/\/.+/.test(str);
    case "creditCard":
      return !empty && !luhn(str.replace(/\D/g, ""));

    case "min": {
      const target = resolveTarget(rule, peerReader);
      if (target === undefined) return true;
      return !empty && compareValues(value, target, rule.coerceAs) < 0;
    }
    case "max": {
      const target = resolveTarget(rule, peerReader);
      if (target === undefined) return true;
      return !empty && compareValues(value, target, rule.coerceAs) > 0;
    }
    case "gt": {
      const target = resolveTarget(rule, peerReader);
      if (target === undefined) return true;
      return empty || compareValues(value, target, rule.coerceAs) <= 0;
    }
    case "lt": {
      const target = resolveTarget(rule, peerReader);
      if (target === undefined) return true;
      return !empty && compareValues(value, target, rule.coerceAs) >= 0;
    }

    case "range": {
      const [lo, hi] = rule.constraint as [unknown, unknown];
      if (empty) return false;
      return compareValues(value, lo, rule.coerceAs) < 0
          || compareValues(value, hi, rule.coerceAs) > 0;
    }
    case "exclusiveRange": {
      const [lo, hi] = rule.constraint as [unknown, unknown];
      if (empty) return false;
      return compareValues(value, lo, rule.coerceAs) <= 0
          || compareValues(value, hi, rule.coerceAs) >= 0;
    }

    case "equalTo": {
      if (empty) return false;
      const target = resolveTarget(rule, peerReader);
      if (target === undefined) return true;
      if (rule.coerceAs) {
        return compareValues(value, target, rule.coerceAs) !== 0;
      }
      return String(value ?? "") !== String(target ?? "");
    }
    case "notEqual":
      return !empty && String(value) === String(rule.constraint);
    case "notEqualTo": {
      const target = resolveTarget(rule, peerReader);
      if (target === undefined) return true;
      if (rule.coerceAs) {
        return !empty && compareValues(value, target, rule.coerceAs) === 0;
      }
      return !empty && String(value ?? "") === String(target ?? "");
    }

    case "atLeastOne":
      return Array.isArray(value) ? value.length === 0 : empty;

    default:
      return true; // Unknown rule type → fail-closed (block, don't pass)
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
