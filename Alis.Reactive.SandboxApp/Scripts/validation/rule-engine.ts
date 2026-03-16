// Rule Engine — Pure validation rule evaluation
//
// No DOM, no vendor, no side effects. Takes a value and rule → pass/fail.
// Used by validation.ts orchestrator. Testable without jsdom.

import type { ValidationRule, ValidationField } from "../types";

/** Represents the result of reading a peer field for equalTo comparison. */
export interface PeerReader {
  readPeer(fieldName: string): string | undefined;
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
    case "minLength":
      return !empty && str.length < Number(rule.constraint);
    case "maxLength":
      return !empty && str.length > Number(rule.constraint);
    case "email":
      return !empty && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(str);
    case "regex": {
      try { return !empty && !new RegExp(String(rule.constraint)).test(str); }
      catch { return false; }
    }
    case "url":
      return !empty && !/^https?:\/\/.+/.test(str);
    case "min":
      return !empty && Number(str) < Number(rule.constraint);
    case "max":
      return !empty && Number(str) > Number(rule.constraint);
    case "range": {
      const [lo, hi] = rule.constraint as [number, number];
      const n = Number(str);
      return !empty && (n < lo || n > hi);
    }
    case "equalTo": {
      const peerVal = peerReader.readPeer(String(rule.constraint));
      if (peerVal === undefined) return false;
      return String(value ?? "") !== peerVal;
    }
    case "atLeastOne":
      return Array.isArray(value) ? value.length === 0 : empty;
    default:
      return false;
  }
}
