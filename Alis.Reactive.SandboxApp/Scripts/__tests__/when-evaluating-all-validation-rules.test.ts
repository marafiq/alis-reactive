// BDD tests for all 18 validation rule types with all applicable coercion types.
// Tests user behaviors: "when nurse enters X, error Y shows."
// Pure rule-engine tests — no DOM, no orchestrator.

import { describe, it, expect } from "vitest";
import { ruleFails, type PeerReader } from "../validation/rule-engine";
import type { ValidationRule } from "../types";

// ── Helpers ──────────────────────────────────────────────

function rule(overrides: Partial<ValidationRule> & { rule: ValidationRule["rule"] }): ValidationRule {
  return { message: "error", ...overrides };
}

const noPeers: PeerReader = { readPeer: () => undefined };

function withPeer(fieldName: string, value: unknown): PeerReader {
  return { readPeer: (name) => name === fieldName ? value : undefined };
}

// ── required ─────────────────────────────────────────────

describe("When resident name is left blank", () => {
  it("shows required error for empty string", () => {
    expect(ruleFails(rule({ rule: "required" }), "", noPeers)).toBe(true);
  });
  it("shows required error for null", () => {
    expect(ruleFails(rule({ rule: "required" }), null, noPeers)).toBe(true);
  });
  it("shows required error for undefined", () => {
    expect(ruleFails(rule({ rule: "required" }), undefined, noPeers)).toBe(true);
  });
  it("shows required error for false (unchecked checkbox)", () => {
    expect(ruleFails(rule({ rule: "required" }), false, noPeers)).toBe(true);
  });
  it("passes when name is filled", () => {
    expect(ruleFails(rule({ rule: "required" }), "John", noPeers)).toBe(false);
  });
});

// ── empty ────────────────────────────────────────────────

describe("When salary must be empty (not employed)", () => {
  it("passes when value is empty string", () => {
    expect(ruleFails(rule({ rule: "empty" }), "", noPeers)).toBe(false);
  });
  it("passes when value is null", () => {
    expect(ruleFails(rule({ rule: "empty" }), null, noPeers)).toBe(false);
  });
  it("passes when value is false", () => {
    expect(ruleFails(rule({ rule: "empty" }), false, noPeers)).toBe(false);
  });
  it("fails when value is present", () => {
    expect(ruleFails(rule({ rule: "empty" }), "50000", noPeers)).toBe(true);
  });
});

// ── minLength / maxLength ────────────────────────────────

describe("When resident name is too short", () => {
  it("fails when name has fewer characters than minimum", () => {
    expect(ruleFails(rule({ rule: "minLength", constraint: 3 }), "Jo", noPeers)).toBe(true);
  });
  it("passes when name meets minimum", () => {
    expect(ruleFails(rule({ rule: "minLength", constraint: 3 }), "Joe", noPeers)).toBe(false);
  });
  it("skips empty (required handles that)", () => {
    expect(ruleFails(rule({ rule: "minLength", constraint: 3 }), "", noPeers)).toBe(false);
  });
});

describe("When notes exceed maximum length", () => {
  it("fails when too long", () => {
    expect(ruleFails(rule({ rule: "maxLength", constraint: 5 }), "toolong", noPeers)).toBe(true);
  });
  it("passes at exactly max", () => {
    expect(ruleFails(rule({ rule: "maxLength", constraint: 5 }), "exact", noPeers)).toBe(false);
  });
  it("skips empty", () => {
    expect(ruleFails(rule({ rule: "maxLength", constraint: 5 }), "", noPeers)).toBe(false);
  });
});

// ── email ────────────────────────────────────────────────

describe("When nurse enters invalid email", () => {
  it("fails for missing @", () => {
    expect(ruleFails(rule({ rule: "email" }), "notanemail", noPeers)).toBe(true);
  });
  it("passes for valid email", () => {
    expect(ruleFails(rule({ rule: "email" }), "nurse@facility.com", noPeers)).toBe(false);
  });
  it("skips empty", () => {
    expect(ruleFails(rule({ rule: "email" }), "", noPeers)).toBe(false);
  });
});

// ── regex ────────────────────────────────────────────────

describe("When phone format is invalid", () => {
  it("fails when pattern does not match", () => {
    expect(ruleFails(rule({ rule: "regex", constraint: "^\\d{3}-\\d{3}-\\d{4}$" }), "123", noPeers)).toBe(true);
  });
  it("passes when pattern matches", () => {
    expect(ruleFails(rule({ rule: "regex", constraint: "^\\d{3}-\\d{3}-\\d{4}$" }), "555-123-4567", noPeers)).toBe(false);
  });
  it("skips empty", () => {
    expect(ruleFails(rule({ rule: "regex", constraint: "^\\d+$" }), "", noPeers)).toBe(false);
  });
  it("fails closed on broken regex", () => {
    expect(ruleFails(rule({ rule: "regex", constraint: "[" }), "test", noPeers)).toBe(true);
  });
});

// ── url ──────────────────────────────────────────────────

describe("When website URL is invalid", () => {
  it("fails for non-http URL", () => {
    expect(ruleFails(rule({ rule: "url" }), "not-a-url", noPeers)).toBe(true);
  });
  it("passes for valid http URL", () => {
    expect(ruleFails(rule({ rule: "url" }), "https://facility.com", noPeers)).toBe(false);
  });
  it("skips empty", () => {
    expect(ruleFails(rule({ rule: "url" }), "", noPeers)).toBe(false);
  });
});

// ── creditCard ───────────────────────────────────────────

describe("When payment card number is invalid", () => {
  it("fails for invalid Luhn", () => {
    expect(ruleFails(rule({ rule: "creditCard" }), "1234567890123", noPeers)).toBe(true);
  });
  it("passes for valid Visa number", () => {
    // 4111111111111111 is a standard Visa test number
    expect(ruleFails(rule({ rule: "creditCard" }), "4111111111111111", noPeers)).toBe(false);
  });
  it("passes with spaces/dashes (stripped)", () => {
    expect(ruleFails(rule({ rule: "creditCard" }), "4111-1111-1111-1111", noPeers)).toBe(false);
  });
  it("fails for too-short number", () => {
    expect(ruleFails(rule({ rule: "creditCard" }), "411111", noPeers)).toBe(true);
  });
  it("skips empty", () => {
    expect(ruleFails(rule({ rule: "creditCard" }), "", noPeers)).toBe(false);
  });
});

// ── min / max with coerceAs: "number" ────────────────────

describe("When resident age is below minimum", () => {
  it("fails when age < min (number coercion)", () => {
    expect(ruleFails(rule({ rule: "min", constraint: 18, coerceAs: "number" }), "17", noPeers)).toBe(true);
  });
  it("passes when age == min", () => {
    expect(ruleFails(rule({ rule: "min", constraint: 18, coerceAs: "number" }), "18", noPeers)).toBe(false);
  });
  it("passes when age > min", () => {
    expect(ruleFails(rule({ rule: "min", constraint: 18, coerceAs: "number" }), "65", noPeers)).toBe(false);
  });
  it("skips empty", () => {
    expect(ruleFails(rule({ rule: "min", constraint: 18, coerceAs: "number" }), "", noPeers)).toBe(false);
  });
});

describe("When resident age exceeds maximum", () => {
  it("fails when age > max", () => {
    expect(ruleFails(rule({ rule: "max", constraint: 120, coerceAs: "number" }), "121", noPeers)).toBe(true);
  });
  it("passes when age == max", () => {
    expect(ruleFails(rule({ rule: "max", constraint: 120, coerceAs: "number" }), "120", noPeers)).toBe(false);
  });
  it("skips empty", () => {
    expect(ruleFails(rule({ rule: "max", constraint: 120, coerceAs: "number" }), "", noPeers)).toBe(false);
  });
});

// ── gt / lt with coerceAs: "number" ──────────────────────

describe("When monthly rate must be greater than zero", () => {
  it("fails when rate is zero (gt implies required)", () => {
    expect(ruleFails(rule({ rule: "gt", constraint: 0, coerceAs: "number" }), "0", noPeers)).toBe(true);
  });
  it("fails when empty (gt implies required)", () => {
    expect(ruleFails(rule({ rule: "gt", constraint: 0, coerceAs: "number" }), "", noPeers)).toBe(true);
  });
  it("passes when rate is positive", () => {
    expect(ruleFails(rule({ rule: "gt", constraint: 0, coerceAs: "number" }), "500", noPeers)).toBe(false);
  });
});

describe("When deposit must be less than total", () => {
  it("fails when deposit >= limit", () => {
    expect(ruleFails(rule({ rule: "lt", constraint: 1000000, coerceAs: "number" }), "1000000", noPeers)).toBe(true);
  });
  it("passes when deposit < limit", () => {
    expect(ruleFails(rule({ rule: "lt", constraint: 1000000, coerceAs: "number" }), "999999", noPeers)).toBe(false);
  });
  it("skips empty", () => {
    expect(ruleFails(rule({ rule: "lt", constraint: 1000000, coerceAs: "number" }), "", noPeers)).toBe(false);
  });
});

// ── range with coerceAs: "number" ────────────────────────

describe("When age must be between 0 and 120", () => {
  it("fails when below range", () => {
    expect(ruleFails(rule({ rule: "range", constraint: [0, 120], coerceAs: "number" }), "-1", noPeers)).toBe(true);
  });
  it("fails when above range", () => {
    expect(ruleFails(rule({ rule: "range", constraint: [0, 120], coerceAs: "number" }), "121", noPeers)).toBe(true);
  });
  it("passes at boundaries (inclusive)", () => {
    expect(ruleFails(rule({ rule: "range", constraint: [0, 120], coerceAs: "number" }), "0", noPeers)).toBe(false);
    expect(ruleFails(rule({ rule: "range", constraint: [0, 120], coerceAs: "number" }), "120", noPeers)).toBe(false);
  });
  it("passes within range", () => {
    expect(ruleFails(rule({ rule: "range", constraint: [0, 120], coerceAs: "number" }), "65", noPeers)).toBe(false);
  });
  it("skips empty", () => {
    expect(ruleFails(rule({ rule: "range", constraint: [0, 120], coerceAs: "number" }), "", noPeers)).toBe(false);
  });
});

// ── exclusiveRange with coerceAs: "number" ───────────────

describe("When score must be exclusively between 0 and 100", () => {
  it("fails at boundary (exclusive)", () => {
    expect(ruleFails(rule({ rule: "exclusiveRange", constraint: [0, 100], coerceAs: "number" }), "0", noPeers)).toBe(true);
    expect(ruleFails(rule({ rule: "exclusiveRange", constraint: [0, 100], coerceAs: "number" }), "100", noPeers)).toBe(true);
  });
  it("passes within range", () => {
    expect(ruleFails(rule({ rule: "exclusiveRange", constraint: [0, 100], coerceAs: "number" }), "50", noPeers)).toBe(false);
  });
  it("skips empty", () => {
    expect(ruleFails(rule({ rule: "exclusiveRange", constraint: [0, 100], coerceAs: "number" }), "", noPeers)).toBe(false);
  });
});

// ── min / max with coerceAs: "date" ──────────────────────

describe("When admission date must be on or after 2020-01-01", () => {
  it("fails when date is before minimum", () => {
    expect(ruleFails(
      rule({ rule: "min", constraint: "2020-01-01", coerceAs: "date" }),
      "2019-12-31", noPeers
    )).toBe(true);
  });
  it("passes when date equals minimum", () => {
    expect(ruleFails(
      rule({ rule: "min", constraint: "2020-01-01", coerceAs: "date" }),
      "2020-01-01", noPeers
    )).toBe(false);
  });
  it("passes when date is after minimum", () => {
    expect(ruleFails(
      rule({ rule: "min", constraint: "2020-01-01", coerceAs: "date" }),
      "2025-06-15", noPeers
    )).toBe(false);
  });
  it("skips empty", () => {
    expect(ruleFails(
      rule({ rule: "min", constraint: "2020-01-01", coerceAs: "date" }),
      "", noPeers
    )).toBe(false);
  });
});

describe("When admission date must be on or before 2030-12-31", () => {
  it("fails when date is after maximum", () => {
    expect(ruleFails(
      rule({ rule: "max", constraint: "2030-12-31", coerceAs: "date" }),
      "2031-01-01", noPeers
    )).toBe(true);
  });
  it("passes when date equals maximum", () => {
    expect(ruleFails(
      rule({ rule: "max", constraint: "2030-12-31", coerceAs: "date" }),
      "2030-12-31", noPeers
    )).toBe(false);
  });
});

// ── gt / lt with coerceAs: "date" ────────────────────────

describe("When discharge date must be after admission date (cross-property)", () => {
  it("fails when discharge equals admission", () => {
    expect(ruleFails(
      rule({ rule: "gt", field: "AdmissionDate", coerceAs: "date" }),
      "2025-03-15",
      withPeer("AdmissionDate", "2025-03-15")
    )).toBe(true);
  });
  it("fails when discharge is before admission", () => {
    expect(ruleFails(
      rule({ rule: "gt", field: "AdmissionDate", coerceAs: "date" }),
      "2025-03-14",
      withPeer("AdmissionDate", "2025-03-15")
    )).toBe(true);
  });
  it("passes when discharge is after admission", () => {
    expect(ruleFails(
      rule({ rule: "gt", field: "AdmissionDate", coerceAs: "date" }),
      "2025-03-16",
      withPeer("AdmissionDate", "2025-03-15")
    )).toBe(false);
  });
  it("fails when empty (gt implies required)", () => {
    expect(ruleFails(
      rule({ rule: "gt", field: "AdmissionDate", coerceAs: "date" }),
      "",
      withPeer("AdmissionDate", "2025-03-15")
    )).toBe(true);
  });
  it("fails closed when peer is unresolvable", () => {
    expect(ruleFails(
      rule({ rule: "gt", field: "AdmissionDate", coerceAs: "date" }),
      "2025-03-16", noPeers
    )).toBe(true);
  });
});

describe("When follow-up date must be before discharge date (cross-property)", () => {
  it("fails when follow-up equals discharge", () => {
    expect(ruleFails(
      rule({ rule: "lt", field: "DischargeDate", coerceAs: "date" }),
      "2025-04-01",
      withPeer("DischargeDate", "2025-04-01")
    )).toBe(true);
  });
  it("passes when follow-up is before discharge", () => {
    expect(ruleFails(
      rule({ rule: "lt", field: "DischargeDate", coerceAs: "date" }),
      "2025-03-31",
      withPeer("DischargeDate", "2025-04-01")
    )).toBe(false);
  });
});

// ── Cross-property min / max with date ───────────────────

describe("When end date must be on or after start date (cross-property)", () => {
  it("fails when end date is before start date", () => {
    expect(ruleFails(
      rule({ rule: "min", field: "StartDate", coerceAs: "date" }),
      "2025-01-01",
      withPeer("StartDate", "2025-06-01")
    )).toBe(true);
  });
  it("passes when end date equals start date", () => {
    expect(ruleFails(
      rule({ rule: "min", field: "StartDate", coerceAs: "date" }),
      "2025-06-01",
      withPeer("StartDate", "2025-06-01")
    )).toBe(false);
  });
  it("passes when end date is after start date", () => {
    expect(ruleFails(
      rule({ rule: "min", field: "StartDate", coerceAs: "date" }),
      "2025-07-01",
      withPeer("StartDate", "2025-06-01")
    )).toBe(false);
  });
});

describe("When start date must be on or before end date (cross-property)", () => {
  it("fails when start date is after end date", () => {
    expect(ruleFails(
      rule({ rule: "max", field: "EndDate", coerceAs: "date" }),
      "2025-07-01",
      withPeer("EndDate", "2025-06-01")
    )).toBe(true);
  });
  it("passes when start date equals end date", () => {
    expect(ruleFails(
      rule({ rule: "max", field: "EndDate", coerceAs: "date" }),
      "2025-06-01",
      withPeer("EndDate", "2025-06-01")
    )).toBe(false);
  });
});

// ── range with coerceAs: "date" ──────────────────────────

describe("When appointment must be within a date range", () => {
  it("fails when before range start", () => {
    expect(ruleFails(
      rule({ rule: "range", constraint: ["2025-01-01", "2025-12-31"], coerceAs: "date" }),
      "2024-12-31", noPeers
    )).toBe(true);
  });
  it("fails when after range end", () => {
    expect(ruleFails(
      rule({ rule: "range", constraint: ["2025-01-01", "2025-12-31"], coerceAs: "date" }),
      "2026-01-01", noPeers
    )).toBe(true);
  });
  it("passes at boundaries (inclusive)", () => {
    expect(ruleFails(
      rule({ rule: "range", constraint: ["2025-01-01", "2025-12-31"], coerceAs: "date" }),
      "2025-01-01", noPeers
    )).toBe(false);
    expect(ruleFails(
      rule({ rule: "range", constraint: ["2025-01-01", "2025-12-31"], coerceAs: "date" }),
      "2025-12-31", noPeers
    )).toBe(false);
  });
  it("passes within range", () => {
    expect(ruleFails(
      rule({ rule: "range", constraint: ["2025-01-01", "2025-12-31"], coerceAs: "date" }),
      "2025-06-15", noPeers
    )).toBe(false);
  });
});

// ── exclusiveRange with coerceAs: "date" ─────────────────

describe("When visit must be exclusively within a date range", () => {
  it("fails at boundaries (exclusive)", () => {
    expect(ruleFails(
      rule({ rule: "exclusiveRange", constraint: ["2025-01-01", "2025-12-31"], coerceAs: "date" }),
      "2025-01-01", noPeers
    )).toBe(true);
    expect(ruleFails(
      rule({ rule: "exclusiveRange", constraint: ["2025-01-01", "2025-12-31"], coerceAs: "date" }),
      "2025-12-31", noPeers
    )).toBe(true);
  });
  it("passes within range", () => {
    expect(ruleFails(
      rule({ rule: "exclusiveRange", constraint: ["2025-01-01", "2025-12-31"], coerceAs: "date" }),
      "2025-06-15", noPeers
    )).toBe(false);
  });
});

// ── min / max with Date objects (Syncfusion components) ──

describe("When Syncfusion DatePicker returns Date objects", () => {
  it("min: fails when Date object is before minimum ISO string", () => {
    expect(ruleFails(
      rule({ rule: "min", constraint: "2020-01-01", coerceAs: "date" }),
      new Date(2019, 11, 31), noPeers  // Dec 31, 2019
    )).toBe(true);
  });
  it("min: passes when Date object is after minimum ISO string", () => {
    expect(ruleFails(
      rule({ rule: "min", constraint: "2020-01-01", coerceAs: "date" }),
      new Date(2020, 0, 2), noPeers  // Jan 2, 2020
    )).toBe(false);
  });
  it("gt cross-property: compares Date object vs Date object from peer", () => {
    expect(ruleFails(
      rule({ rule: "gt", field: "AdmissionDate", coerceAs: "date" }),
      new Date(2025, 2, 16), // Mar 16, 2025
      withPeer("AdmissionDate", new Date(2025, 2, 15)) // Mar 15, 2025
    )).toBe(false);
  });
});

// ── equalTo (cross-property via field) ───────────────────

describe("When confirm email must match email", () => {
  it("fails when values differ", () => {
    expect(ruleFails(
      rule({ rule: "equalTo", field: "Email" }),
      "different@test.com",
      withPeer("Email", "john@test.com")
    )).toBe(true);
  });
  it("passes when values match", () => {
    expect(ruleFails(
      rule({ rule: "equalTo", field: "Email" }),
      "john@test.com",
      withPeer("Email", "john@test.com")
    )).toBe(false);
  });
  it("skips empty", () => {
    expect(ruleFails(
      rule({ rule: "equalTo", field: "Email" }),
      "",
      withPeer("Email", "john@test.com")
    )).toBe(false);
  });
  it("fails closed when peer unresolvable", () => {
    expect(ruleFails(
      rule({ rule: "equalTo", field: "Email" }),
      "test@test.com", noPeers
    )).toBe(true);
  });
});

// ── equalTo (fixed value via constraint) ─────────────────

describe("When status must equal a specific value", () => {
  it("fails when value does not match constraint", () => {
    expect(ruleFails(
      rule({ rule: "equalTo", constraint: "active" }),
      "inactive", noPeers
    )).toBe(true);
  });
  it("passes when value matches constraint", () => {
    expect(ruleFails(
      rule({ rule: "equalTo", constraint: "active" }),
      "active", noPeers
    )).toBe(false);
  });
});

// ── equalTo with coerceAs: "number" ──────────────────────

describe("When numeric values must be equal", () => {
  it("compares numerically, not as strings", () => {
    expect(ruleFails(
      rule({ rule: "equalTo", constraint: 100, coerceAs: "number" }),
      "100", noPeers
    )).toBe(false);
  });
});

// ── notEqual (fixed value) ───────────────────────────────

describe("When status must not equal 'deleted'", () => {
  it("fails when value equals forbidden value", () => {
    expect(ruleFails(
      rule({ rule: "notEqual", constraint: "deleted" }),
      "deleted", noPeers
    )).toBe(true);
  });
  it("passes when value differs", () => {
    expect(ruleFails(
      rule({ rule: "notEqual", constraint: "deleted" }),
      "active", noPeers
    )).toBe(false);
  });
  it("skips empty", () => {
    expect(ruleFails(
      rule({ rule: "notEqual", constraint: "deleted" }),
      "", noPeers
    )).toBe(false);
  });
});

// ── notEqualTo (cross-property via field) ────────────────

describe("When alternate email must not equal primary email", () => {
  it("fails when values are the same", () => {
    expect(ruleFails(
      rule({ rule: "notEqualTo", field: "Email" }),
      "same@test.com",
      withPeer("Email", "same@test.com")
    )).toBe(true);
  });
  it("passes when values differ", () => {
    expect(ruleFails(
      rule({ rule: "notEqualTo", field: "Email" }),
      "alt@test.com",
      withPeer("Email", "main@test.com")
    )).toBe(false);
  });
  it("skips empty", () => {
    expect(ruleFails(
      rule({ rule: "notEqualTo", field: "Email" }),
      "",
      withPeer("Email", "main@test.com")
    )).toBe(false);
  });
  it("fails closed when peer unresolvable", () => {
    expect(ruleFails(
      rule({ rule: "notEqualTo", field: "Email" }),
      "test@test.com", noPeers
    )).toBe(true);
  });
});

// ── notEqualTo with coerceAs: "number" ───────────────────

describe("When numeric values must not be equal (cross-property)", () => {
  it("fails when numerically equal", () => {
    expect(ruleFails(
      rule({ rule: "notEqualTo", field: "OtherAmount", coerceAs: "number" }),
      "100",
      withPeer("OtherAmount", "100")
    )).toBe(true);
  });
  it("passes when numerically different", () => {
    expect(ruleFails(
      rule({ rule: "notEqualTo", field: "OtherAmount", coerceAs: "number" }),
      "200",
      withPeer("OtherAmount", "100")
    )).toBe(false);
  });
});

// ── atLeastOne ───────────────────────────────────────────

describe("When checklist must have at least one selection", () => {
  it("fails for empty array", () => {
    expect(ruleFails(rule({ rule: "atLeastOne" }), [], noPeers)).toBe(true);
  });
  it("passes for non-empty array", () => {
    expect(ruleFails(rule({ rule: "atLeastOne" }), ["option1"], noPeers)).toBe(false);
  });
  it("fails for null", () => {
    expect(ruleFails(rule({ rule: "atLeastOne" }), null, noPeers)).toBe(true);
  });
  it("fails for empty string", () => {
    expect(ruleFails(rule({ rule: "atLeastOne" }), "", noPeers)).toBe(true);
  });
});

// ── fail-fast: comparison without coerceAs throws ────────

describe("When comparison rule is missing coerceAs", () => {
  it("min throws without coerceAs (no silent fallback)", () => {
    expect(() => ruleFails(rule({ rule: "min", constraint: 18 }), "17", noPeers))
      .toThrow("coerceAs");
  });
  it("gt throws without coerceAs", () => {
    expect(() => ruleFails(rule({ rule: "gt", constraint: 0 }), "1", noPeers))
      .toThrow("coerceAs");
  });
  it("range throws without coerceAs", () => {
    expect(() => ruleFails(rule({ rule: "range", constraint: [0, 120] }), "50", noPeers))
      .toThrow("coerceAs");
  });
});

// ── NaN handling (invalid values with coercion) ──────────

describe("When invalid value is coerced for date comparison", () => {
  it("min with invalid date string coerces to NaN — fails closed", () => {
    // "not-a-date" is not empty, toDate returns NaN → NaN fail-closed → fails
    expect(ruleFails(
      rule({ rule: "min", constraint: "2020-01-01", coerceAs: "date" }),
      "not-a-date", noPeers
    )).toBe(true);
  });
  it("gt with invalid date string fails closed (NaN)", () => {
    // "not-a-date" is not empty, compareValues returns NaN → fail closed
    expect(ruleFails(
      rule({ rule: "gt", constraint: "2020-01-01", coerceAs: "date" }),
      "not-a-date", noPeers
    )).toBe(true);
  });
});

// ── empty rule with non-string types ─────────────────────

describe("When empty rule checks non-string values", () => {
  it("zero is not empty (non-null, non-false, coerces to '0')", () => {
    expect(ruleFails(rule({ rule: "empty" }), 0, noPeers)).toBe(true);
  });
  it("positive number is not empty", () => {
    expect(ruleFails(rule({ rule: "empty" }), 42, noPeers)).toBe(true);
  });
  it("non-empty array is not empty", () => {
    expect(ruleFails(rule({ rule: "empty" }), [1, 2], noPeers)).toBe(true);
  });
});

// ── unknown rule type ────────────────────────────────────

describe("When an unknown rule type is encountered", () => {
  it("fails closed (blocks, does not silently pass)", () => {
    expect(ruleFails(rule({ rule: "unknownRule" as any }), "value", noPeers)).toBe(true);
  });
});
