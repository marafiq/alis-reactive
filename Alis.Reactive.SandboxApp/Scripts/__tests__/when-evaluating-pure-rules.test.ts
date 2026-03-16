import { describe, it, expect } from "vitest";
import { ruleFails, type PeerReader } from "../validation/rule-engine";
import { evalCondition, type ConditionReader } from "../validation/condition";

// ── Pure rule-engine tests (no DOM, no vendor) ──────────

const noPeers: PeerReader = { readPeer: () => undefined };

describe("rule-engine: required", () => {
  const rule = { rule: "required" as const, message: "required" };

  it("fails for empty string", () => expect(ruleFails(rule, "", noPeers)).toBe(true));
  it("fails for null", () => expect(ruleFails(rule, null, noPeers)).toBe(true));
  it("fails for undefined", () => expect(ruleFails(rule, undefined, noPeers)).toBe(true));
  it("fails for false", () => expect(ruleFails(rule, false, noPeers)).toBe(true));
  it("passes for non-empty string", () => expect(ruleFails(rule, "abc", noPeers)).toBe(false));
  it("passes for true", () => expect(ruleFails(rule, true, noPeers)).toBe(false));
  it("passes for zero (number)", () => expect(ruleFails(rule, 0, noPeers)).toBe(false));
});

describe("rule-engine: minLength", () => {
  const rule = { rule: "minLength" as const, message: "too short", constraint: 3 };

  it("fails below threshold", () => expect(ruleFails(rule, "ab", noPeers)).toBe(true));
  it("passes at threshold", () => expect(ruleFails(rule, "abc", noPeers)).toBe(false));
  it("passes above threshold", () => expect(ruleFails(rule, "abcd", noPeers)).toBe(false));
  it("skips empty (not required)", () => expect(ruleFails(rule, "", noPeers)).toBe(false));
});

describe("rule-engine: maxLength", () => {
  const rule = { rule: "maxLength" as const, message: "too long", constraint: 5 };

  it("fails above threshold", () => expect(ruleFails(rule, "abcdef", noPeers)).toBe(true));
  it("passes at threshold", () => expect(ruleFails(rule, "abcde", noPeers)).toBe(false));
  it("passes below threshold", () => expect(ruleFails(rule, "abc", noPeers)).toBe(false));
  it("skips empty", () => expect(ruleFails(rule, "", noPeers)).toBe(false));
});

describe("rule-engine: email", () => {
  const rule = { rule: "email" as const, message: "bad email" };

  it("fails for no @", () => expect(ruleFails(rule, "noatsign", noPeers)).toBe(true));
  it("fails for missing domain", () => expect(ruleFails(rule, "user@", noPeers)).toBe(true));
  it("passes for valid email", () => expect(ruleFails(rule, "user@example.com", noPeers)).toBe(false));
  it("skips empty", () => expect(ruleFails(rule, "", noPeers)).toBe(false));
});

describe("rule-engine: regex", () => {
  const rule = { rule: "regex" as const, message: "bad format", constraint: "^\\d{5}$" };

  it("fails for non-matching", () => expect(ruleFails(rule, "abc", noPeers)).toBe(true));
  it("passes for matching", () => expect(ruleFails(rule, "12345", noPeers)).toBe(false));
  it("skips empty", () => expect(ruleFails(rule, "", noPeers)).toBe(false));
  it("fails closed on invalid regex (blocks, does not pass)", () => {
    const bad = { rule: "regex" as const, message: "bad", constraint: "[invalid" };
    expect(ruleFails(bad, "test", noPeers)).toBe(true);
  });
});

describe("rule-engine: url", () => {
  const rule = { rule: "url" as const, message: "bad url" };

  it("fails for non-url", () => expect(ruleFails(rule, "not-a-url", noPeers)).toBe(true));
  it("passes for https", () => expect(ruleFails(rule, "https://example.com", noPeers)).toBe(false));
  it("passes for http", () => expect(ruleFails(rule, "http://example.com", noPeers)).toBe(false));
  it("skips empty", () => expect(ruleFails(rule, "", noPeers)).toBe(false));
});

describe("rule-engine: min", () => {
  const rule = { rule: "min" as const, message: "too low", constraint: 100 };

  it("fails below", () => expect(ruleFails(rule, "50", noPeers)).toBe(true));
  it("passes at boundary", () => expect(ruleFails(rule, "100", noPeers)).toBe(false));
  it("passes above", () => expect(ruleFails(rule, "200", noPeers)).toBe(false));
});

describe("rule-engine: max", () => {
  const rule = { rule: "max" as const, message: "too high", constraint: 500 };

  it("fails above", () => expect(ruleFails(rule, "600", noPeers)).toBe(true));
  it("passes at boundary", () => expect(ruleFails(rule, "500", noPeers)).toBe(false));
  it("passes below", () => expect(ruleFails(rule, "200", noPeers)).toBe(false));
});

describe("rule-engine: range", () => {
  const rule = { rule: "range" as const, message: "out of range", constraint: [0, 120] as [number, number] };

  it("fails below", () => expect(ruleFails(rule, "-1", noPeers)).toBe(true));
  it("fails above", () => expect(ruleFails(rule, "121", noPeers)).toBe(true));
  it("passes at lower bound", () => expect(ruleFails(rule, "0", noPeers)).toBe(false));
  it("passes at upper bound", () => expect(ruleFails(rule, "120", noPeers)).toBe(false));
  it("passes within", () => expect(ruleFails(rule, "50", noPeers)).toBe(false));
});

describe("rule-engine: equalTo", () => {
  it("fails when values differ", () => {
    const rule = { rule: "equalTo" as const, message: "must match", constraint: "Password" };
    const reader: PeerReader = { readPeer: () => "secret" };
    expect(ruleFails(rule, "different", reader)).toBe(true);
  });

  it("passes when values match", () => {
    const rule = { rule: "equalTo" as const, message: "must match", constraint: "Password" };
    const reader: PeerReader = { readPeer: () => "secret" };
    expect(ruleFails(rule, "secret", reader)).toBe(false);
  });

  it("fails closed when peer is unavailable (blocks, does not pass)", () => {
    const rule = { rule: "equalTo" as const, message: "must match", constraint: "Missing" };
    expect(ruleFails(rule, "anything", noPeers)).toBe(true);
  });
});

describe("rule-engine: atLeastOne", () => {
  const rule = { rule: "atLeastOne" as const, message: "need one" };

  it("fails for empty array", () => expect(ruleFails(rule, [], noPeers)).toBe(true));
  it("passes for non-empty array", () => expect(ruleFails(rule, ["a"], noPeers)).toBe(false));
  it("fails for empty string", () => expect(ruleFails(rule, "", noPeers)).toBe(true));
  it("passes for non-empty string", () => expect(ruleFails(rule, "tag1", noPeers)).toBe(false));
});

// ── Pure condition tests (no DOM, no vendor) ────────────

function readerWith(values: Record<string, string>): ConditionReader {
  return { readConditionSource: (name) => values[name] };
}

const noSource: ConditionReader = { readConditionSource: () => undefined };

describe("condition: truthy", () => {
  it("true for non-empty string", () => expect(evalCondition({ field: "f", op: "truthy" }, readerWith({ f: "yes" }))).toBe(true));
  it("false for empty string", () => expect(evalCondition({ field: "f", op: "truthy" }, readerWith({ f: "" }))).toBe(false));
  it("false for 'false'", () => expect(evalCondition({ field: "f", op: "truthy" }, readerWith({ f: "false" }))).toBe(false));
  it("null when source unavailable", () => expect(evalCondition({ field: "f", op: "truthy" }, noSource)).toBeNull());
});

describe("condition: falsy", () => {
  it("true for empty string", () => expect(evalCondition({ field: "f", op: "falsy" }, readerWith({ f: "" }))).toBe(true));
  it("true for 'false'", () => expect(evalCondition({ field: "f", op: "falsy" }, readerWith({ f: "false" }))).toBe(true));
  it("false for non-empty", () => expect(evalCondition({ field: "f", op: "falsy" }, readerWith({ f: "yes" }))).toBe(false));
  it("null when source unavailable", () => expect(evalCondition({ field: "f", op: "falsy" }, noSource)).toBeNull());
});

describe("condition: eq", () => {
  it("true when values match", () => expect(evalCondition({ field: "f", op: "eq", value: "Memory Care" }, readerWith({ f: "Memory Care" }))).toBe(true));
  it("false when values differ", () => expect(evalCondition({ field: "f", op: "eq", value: "Memory Care" }, readerWith({ f: "Assisted" }))).toBe(false));
  it("false when source is empty (no intent expressed)", () => expect(evalCondition({ field: "f", op: "eq", value: "Memory Care" }, readerWith({ f: "" }))).toBe(false));
  it("null when source unavailable", () => expect(evalCondition({ field: "f", op: "eq", value: "x" }, noSource)).toBeNull());
});

describe("condition: neq", () => {
  it("true when values differ", () => expect(evalCondition({ field: "f", op: "neq", value: "Independent" }, readerWith({ f: "Assisted" }))).toBe(true));
  it("false when values match", () => expect(evalCondition({ field: "f", op: "neq", value: "Independent" }, readerWith({ f: "Independent" }))).toBe(false));
  it("false when source is empty (no intent expressed)", () => expect(evalCondition({ field: "f", op: "neq", value: "Independent" }, readerWith({ f: "" }))).toBe(false));
  it("null when source unavailable", () => expect(evalCondition({ field: "f", op: "neq", value: "x" }, noSource)).toBeNull());
});

describe("condition: empty source handling", () => {
  it("truthy treats empty as falsy", () => expect(evalCondition({ field: "f", op: "truthy" }, readerWith({ f: "" }))).toBe(false));
  it("falsy treats empty as true", () => expect(evalCondition({ field: "f", op: "falsy" }, readerWith({ f: "" }))).toBe(true));
  it("eq with empty source returns false (not yet determined)", () => expect(evalCondition({ field: "f", op: "eq", value: "x" }, readerWith({ f: "" }))).toBe(false));
  it("neq with empty source returns false (not yet determined)", () => expect(evalCondition({ field: "f", op: "neq", value: "x" }, readerWith({ f: "" }))).toBe(false));
  it("eq with non-empty source compares normally", () => expect(evalCondition({ field: "f", op: "eq", value: 42 }, readerWith({ f: "42" }))).toBe(true));
  it("neq with non-empty source compares normally", () => expect(evalCondition({ field: "f", op: "neq", value: 42 }, readerWith({ f: "42" }))).toBe(false));
});
