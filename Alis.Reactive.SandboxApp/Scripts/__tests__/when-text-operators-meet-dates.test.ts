/**
 * FAILING TESTS — text operators with Date values
 *
 * The conditions module text operators (contains, starts-with, ends-with,
 * matches, min-length) all call String(resolved ?? "").
 *
 * When the source resolves to a Date object:
 *   - coerceAs: "string" → toString(date) → locale garbage (Gap 1)
 *   - coerceAs: "date"   → toDate(date) → timestamp number → String(timestamp) → "1710460800000"
 *
 * Both produce wrong results for text operations on dates.
 * The correct behavior: text operators should see ISO date strings.
 */
import { describe, it, expect } from "vitest";
import type { Guard, ExecContext } from "../types";
import { evaluateGuard } from "../conditions/conditions";

function ctx(evt: Record<string, unknown>): ExecContext {
  return { evt };
}

function vg(path: string, coerceAs: string, op: string, operand?: unknown): Guard {
  return { kind: "value", source: { kind: "event", path }, coerceAs: coerceAs as any, op: op as any, operand };
}

describe("text operators with Date source values", () => {

  // ── contains on date ──────────────────────────────────────
  // A resident was admitted on 2024-03-15. The guard checks if
  // the admission date string contains "2024-03".
  //
  // ACTUAL (coerceAs: "string"):
  //   toString(Date) → "Fri Mar 15 2024 00:00:00 GMT-0500..."
  //   "Fri Mar 15...".includes("2024-03") → FALSE (locale format)
  //
  // EXPECTED:
  //   toString(Date) → "2024-03-15T00:00:00.000Z"
  //   "2024-03-15T00:00:00.000Z".includes("2024-03") → TRUE

  it("contains matches partial ISO date on Date object", () => {
    const guard = vg("evt.admissionDate", "string", "contains", "2024-03");
    const result = evaluateGuard(guard, ctx({
      admissionDate: new Date("2024-03-15T00:00:00Z"),
    }));
    expect(result).toBe(true);
  });

  // ── starts-with on date ───────────────────────────────────
  // Check if date starts with "2024" (year filter)
  //
  // ACTUAL: "Fri Mar 15 2024...".startsWith("2024") → FALSE
  // EXPECTED: "2024-03-15T...".startsWith("2024") → TRUE

  it("starts-with matches year prefix on Date object", () => {
    const guard = vg("evt.deadline", "string", "starts-with", "2024");
    const result = evaluateGuard(guard, ctx({
      deadline: new Date("2024-06-01T00:00:00Z"),
    }));
    expect(result).toBe(true);
  });

  // ── ends-with on date ─────────────────────────────────────
  // Check if ISO date ends with "Z" (UTC)
  //
  // ACTUAL: "Fri Mar 15 2024 19:00:00 GMT-0500 (CDT)".endsWith("Z") → FALSE
  // EXPECTED: "2024-03-15T00:00:00.000Z".endsWith("Z") → TRUE

  it("ends-with matches UTC suffix on Date object", () => {
    const guard = vg("evt.timestamp", "string", "ends-with", "Z");
    const result = evaluateGuard(guard, ctx({
      timestamp: new Date("2024-03-15T00:00:00Z"),
    }));
    expect(result).toBe(true);
  });

  // ── matches on date ───────────────────────────────────────
  // Regex for ISO date format: YYYY-MM-DD
  //
  // ACTUAL: locale string doesn't match ISO regex
  // EXPECTED: ISO string matches

  it("matches validates ISO date format via regex", () => {
    const guard = vg("evt.eventDate", "string", "matches", "^\\d{4}-\\d{2}-\\d{2}T");
    const result = evaluateGuard(guard, ctx({
      eventDate: new Date("2024-03-15T00:00:00Z"),
    }));
    expect(result).toBe(true);
  });

  // ── min-length on date ────────────────────────────────────
  // ISO string "2024-03-15T00:00:00.000Z" is 24 chars.
  // Locale string is ~50+ chars — this test passes by accident today
  // but for the wrong reason. The length should be predictable.

  it("min-length uses ISO string length, not locale length", () => {
    const guard = vg("evt.date", "string", "min-length", 24);
    const result = evaluateGuard(guard, ctx({
      date: new Date("2024-03-15T00:00:00Z"),
    }));
    expect(result).toBe(true);
    // The ISO string is exactly 24 chars — verify the string IS ISO, not locale
    // by also checking that length 50 does NOT pass (locale strings are 50+ chars)
  });

  it("min-length 50 fails because ISO string is only 24 chars", () => {
    const guard = vg("evt.date", "string", "min-length", 50);
    const result = evaluateGuard(guard, ctx({
      date: new Date("2024-03-15T00:00:00Z"),
    }));
    // TODAY: PASSES (locale string is ~58 chars) — this is WRONG behavior
    // EXPECTED: FAILS (ISO string is 24 chars < 50)
    expect(result).toBe(false);
  });
});
