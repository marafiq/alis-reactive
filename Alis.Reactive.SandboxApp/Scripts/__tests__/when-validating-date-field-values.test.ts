import { describe, it, expect } from "vitest";
import { ruleFails, type PeerReader } from "../validation/rule-engine";

const noPeers: PeerReader = { readPeer: () => undefined };

describe("rule-engine: Date values use ISO representation", () => {
  it("minLength checks ISO string length, not locale length", () => {
    const rule = { rule: "minLength" as const, message: "too short", constraint: 30 };
    // ISO "2024-03-15T00:00:00.000Z" = 24 chars < 30 → should FAIL
    expect(ruleFails(rule, new Date("2024-03-15T00:00:00Z"), noPeers)).toBe(true);
  });

  it("equalTo matches when both peer Date objects represent the same moment", () => {
    const rule = { rule: "equalTo" as const, message: "mismatch", field: "other" };
    const peers: PeerReader = { readPeer: () => new Date("2024-03-15T00:00:00Z") };
    expect(ruleFails(rule, new Date("2024-03-15T00:00:00Z"), peers)).toBe(false);
  });

  it("notEqual detects match between Date object and ISO constraint", () => {
    const rule = { rule: "notEqual" as const, message: "must differ",
      constraint: "2024-03-15T00:00:00.000Z" };
    expect(ruleFails(rule, new Date("2024-03-15T00:00:00Z"), noPeers)).toBe(true);
  });
});
