// Condition — Pure validation condition evaluation
//
// No DOM, no vendor, no side effects. Takes operator + values → true/false/null.
// null means "cannot evaluate" (source not available).

import type { ValidationCondition } from "../types";

/** Reads a condition source field's string value. Returns undefined if unavailable. */
export interface ConditionReader {
  readConditionSource(fieldName: string): string | undefined;
}

/**
 * Evaluates a validation condition.
 * Returns true if the condition is met (rule should apply),
 * false if the condition is not met (rule should be skipped),
 * null if the source cannot be read (caller decides behavior).
 */
export function evalCondition(
  cond: ValidationCondition,
  reader: ConditionReader
): boolean | null {
  const raw = reader.readConditionSource(cond.field);
  if (raw === undefined) return null;

  const str = raw;
  const empty = str === "";

  switch (cond.op) {
    case "truthy": return !empty;
    case "falsy": return empty;
    case "eq": return empty ? false : str === String(cond.value ?? "");
    case "neq": return empty ? false : str !== String(cond.value ?? "");
    default: return true;
  }
}
