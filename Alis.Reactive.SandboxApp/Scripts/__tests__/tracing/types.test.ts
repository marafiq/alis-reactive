import { describe, it, expect } from "vitest";
import { LEVELS, SEVERITY } from "../../tracing/types";

describe("LEVELS", () => {
  it("off is lowest (0)", () => {
    expect(LEVELS.off).toBe(0);
  });

  it("levels increase from error to trace", () => {
    expect(LEVELS.error).toBeLessThan(LEVELS.warn);
    expect(LEVELS.warn).toBeLessThan(LEVELS.info);
    expect(LEVELS.info).toBeLessThan(LEVELS.debug);
    expect(LEVELS.debug).toBeLessThan(LEVELS.trace);
  });

  it("has exactly 6 levels", () => {
    expect(Object.keys(LEVELS)).toHaveLength(6);
  });
});

describe("SEVERITY", () => {
  it("maps to OTel severity numbers", () => {
    expect(SEVERITY.error).toBe(17);
    expect(SEVERITY.warn).toBe(13);
    expect(SEVERITY.info).toBe(9);
    expect(SEVERITY.debug).toBe(5);
    expect(SEVERITY.trace).toBe(1);
  });

  it("does not include off", () => {
    expect("off" in SEVERITY).toBe(false);
  });
});
