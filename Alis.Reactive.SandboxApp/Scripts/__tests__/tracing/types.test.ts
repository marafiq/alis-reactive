import { describe, expect, it } from "vitest";
import { LEVELS, SEVERITY, type Level } from "../../tracing/types";

describe("LEVELS", () => {
  it("orders levels from off (0) to trace (5)", () => {
    expect(LEVELS.off).toBe(0);
    expect(LEVELS.error).toBe(1);
    expect(LEVELS.warn).toBe(2);
    expect(LEVELS.info).toBe(3);
    expect(LEVELS.debug).toBe(4);
    expect(LEVELS.trace).toBe(5);
  });

  it("is strictly monotonic", () => {
    const order: Level[] = ["off", "error", "warn", "info", "debug", "trace"];
    for (let i = 1; i < order.length; i++) {
      expect(LEVELS[order[i]]).toBeGreaterThan(LEVELS[order[i - 1]]);
    }
  });
});

describe("SEVERITY", () => {
  it("uses OpenTelemetry severity numbers for emittable levels", () => {
    expect(SEVERITY.error).toBe(17);
    expect(SEVERITY.warn).toBe(13);
    expect(SEVERITY.info).toBe(9);
    expect(SEVERITY.debug).toBe(5);
    expect(SEVERITY.trace).toBe(1);
  });

  it("has no entry for off", () => {
    // Compile-time: SEVERITY is Record<Exclude<Level, "off">, number>.
    // Runtime: off is not a key.
    expect(Object.prototype.hasOwnProperty.call(SEVERITY, "off")).toBe(false);
  });
});
