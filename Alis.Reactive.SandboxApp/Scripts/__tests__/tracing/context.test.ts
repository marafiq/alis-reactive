import { describe, it, expect } from "vitest";
import { parseTraceparent, formatTraceparent, isValidLevel, resolveLevel } from "../../tracing/context";

describe("parseTraceparent", () => {
  it("parses valid traceparent", () => {
    const result = parseTraceparent("00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01");
    expect(result).toEqual({
      traceId: "4bf92f3577b34da6a3ce929d0e0e4736",
      spanId: "00f067aa0ba902b7",
      flags: "01",
    });
  });

  it("preserves flags as raw hex", () => {
    const result = parseTraceparent("00-aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa-bbbbbbbbbbbbbbbb-ff");
    expect(result?.flags).toBe("ff");
  });

  it("returns undefined for wrong version", () => {
    expect(parseTraceparent("01-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01")).toBeUndefined();
  });

  it("returns undefined for wrong trace-id length", () => {
    expect(parseTraceparent("00-4bf92f-00f067aa0ba902b7-01")).toBeUndefined();
  });

  it("returns undefined for wrong span-id length", () => {
    expect(parseTraceparent("00-4bf92f3577b34da6a3ce929d0e0e4736-00f0-01")).toBeUndefined();
  });

  it("returns undefined for wrong flags length", () => {
    expect(parseTraceparent("00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-1")).toBeUndefined();
  });

  it("returns undefined for empty string", () => {
    expect(parseTraceparent("")).toBeUndefined();
  });
});

describe("formatTraceparent", () => {
  it("formats with default flags", () => {
    expect(formatTraceparent("aaa", "bbb")).toBe("00-aaa-bbb-01");
  });

  it("formats with custom flags", () => {
    expect(formatTraceparent("aaa", "bbb", "ff")).toBe("00-aaa-bbb-ff");
  });
});

describe("isValidLevel", () => {
  it("accepts valid levels", () => {
    for (const level of ["off", "error", "warn", "info", "debug", "trace"]) {
      expect(isValidLevel(level)).toBe(true);
    }
  });

  it("rejects invalid levels", () => {
    expect(isValidLevel("verbose")).toBe(false);
    expect(isValidLevel("")).toBe(false);
    expect(isValidLevel("DEBUG")).toBe(false);
  });
});

describe("resolveLevel", () => {
  it("returns plan level when provided", () => {
    expect(resolveLevel("debug", "error")).toBe("debug");
  });

  it("falls back to element data-trace when plan level is undefined", () => {
    expect(resolveLevel(undefined, "warn")).toBe("warn");
  });

  it("defaults to off when no sources provided", () => {
    expect(resolveLevel(undefined, undefined)).toBe("off");
  });

  it("skips invalid plan level and uses element level", () => {
    expect(resolveLevel("INVALID", "info")).toBe("info");
  });
});
