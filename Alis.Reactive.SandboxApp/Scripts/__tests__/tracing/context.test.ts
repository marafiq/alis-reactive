import { describe, expect, it } from "vitest";
import {
  formatTraceparent,
  generateSpanId,
  generateTraceId,
  isValidLevel,
  parseTraceparent,
  resolveLevel,
} from "../../tracing/context";

describe("parseTraceparent", () => {
  it("parses a valid W3C traceparent", () => {
    const parsed = parseTraceparent(
      "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01",
    );
    expect(parsed).toEqual({
      version: "00",
      traceId: "4bf92f3577b34da6a3ce929d0e0e4736",
      parentId: "00f067aa0ba902b7",
      flags: "01",
    });
  });

  it("preserves the flags byte exactly", () => {
    const parsed = parseTraceparent(
      "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-00",
    );
    expect(parsed?.flags).toBe("00");
  });

  it("returns undefined for undefined input", () => {
    expect(parseTraceparent(undefined)).toBeUndefined();
  });

  it("returns undefined for empty string", () => {
    expect(parseTraceparent("")).toBeUndefined();
  });

  it("returns undefined for malformed input", () => {
    expect(parseTraceparent("not-a-traceparent")).toBeUndefined();
  });

  it("rejects uppercase hex (W3C requires lowercase)", () => {
    expect(
      parseTraceparent("00-4BF92F3577B34DA6A3CE929D0E0E4736-00f067aa0ba902b7-01"),
    ).toBeUndefined();
  });

  it("rejects non-00 version bytes", () => {
    expect(
      parseTraceparent("01-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01"),
    ).toBeUndefined();
  });

  it("rejects the reserved all-zero trace-id", () => {
    expect(
      parseTraceparent("00-00000000000000000000000000000000-00f067aa0ba902b7-01"),
    ).toBeUndefined();
  });

  it("rejects the reserved all-zero span-id", () => {
    expect(
      parseTraceparent("00-4bf92f3577b34da6a3ce929d0e0e4736-0000000000000000-01"),
    ).toBeUndefined();
  });

  it("rejects short trace-id", () => {
    expect(
      parseTraceparent("00-4bf92f3577b34da6a3ce929d0e0e473-00f067aa0ba902b7-01"),
    ).toBeUndefined();
  });
});

describe("formatTraceparent", () => {
  it("produces a valid W3C header", () => {
    const header = formatTraceparent(
      "4bf92f3577b34da6a3ce929d0e0e4736",
      "00f067aa0ba902b7",
    );
    expect(header).toBe(
      "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01",
    );
  });

  it("defaults flags to sampled (01)", () => {
    const header = formatTraceparent(
      "4bf92f3577b34da6a3ce929d0e0e4736",
      "00f067aa0ba902b7",
    );
    expect(header.endsWith("-01")).toBe(true);
  });

  it("preserves explicit flags", () => {
    const header = formatTraceparent(
      "4bf92f3577b34da6a3ce929d0e0e4736",
      "00f067aa0ba902b7",
      "00",
    );
    expect(header.endsWith("-00")).toBe(true);
  });

  it("round-trips through parseTraceparent", () => {
    const header = formatTraceparent(
      "4bf92f3577b34da6a3ce929d0e0e4736",
      "00f067aa0ba902b7",
      "01",
    );
    const parsed = parseTraceparent(header);
    expect(parsed).toEqual({
      version: "00",
      traceId: "4bf92f3577b34da6a3ce929d0e0e4736",
      parentId: "00f067aa0ba902b7",
      flags: "01",
    });
  });
});

describe("isValidLevel", () => {
  it("recognizes all six levels", () => {
    expect(isValidLevel("off")).toBe(true);
    expect(isValidLevel("error")).toBe(true);
    expect(isValidLevel("warn")).toBe(true);
    expect(isValidLevel("info")).toBe(true);
    expect(isValidLevel("debug")).toBe(true);
    expect(isValidLevel("trace")).toBe(true);
  });

  it("rejects undefined", () => {
    expect(isValidLevel(undefined)).toBe(false);
  });

  it("rejects unknown levels", () => {
    expect(isValidLevel("fatal")).toBe(false);
    expect(isValidLevel("INFO")).toBe(false);
    expect(isValidLevel("")).toBe(false);
  });
});

describe("resolveLevel", () => {
  it("prefers dataset over plan", () => {
    expect(resolveLevel("info", "debug")).toBe("debug");
  });

  it("falls back to plan when dataset is missing", () => {
    expect(resolveLevel("info", undefined)).toBe("info");
  });

  it("falls back to default when both are missing", () => {
    expect(resolveLevel(undefined, undefined)).toBe("off");
  });

  it("accepts a custom fallback", () => {
    expect(resolveLevel(undefined, undefined, "warn")).toBe("warn");
  });

  it("ignores invalid values and falls through", () => {
    expect(resolveLevel("bogus", "also-bogus", "info")).toBe("info");
  });

  it("skips invalid dataset but accepts valid plan", () => {
    expect(resolveLevel("info", "bogus")).toBe("info");
  });
});

describe("generateTraceId", () => {
  it("returns 32 lowercase hex digits", () => {
    const id = generateTraceId();
    expect(id).toMatch(/^[0-9a-f]{32}$/);
  });

  it("generates unique IDs across successive calls", () => {
    const ids = new Set<string>();
    for (let i = 0; i < 100; i++) {
      ids.add(generateTraceId());
    }
    expect(ids.size).toBe(100);
  });

  it("never generates the reserved all-zero trace-id", () => {
    for (let i = 0; i < 100; i++) {
      expect(generateTraceId()).not.toBe("00000000000000000000000000000000");
    }
  });
});

describe("generateSpanId", () => {
  it("returns 16 lowercase hex digits", () => {
    const id = generateSpanId();
    expect(id).toMatch(/^[0-9a-f]{16}$/);
  });

  it("generates unique IDs across successive calls", () => {
    const ids = new Set<string>();
    for (let i = 0; i < 100; i++) {
      ids.add(generateSpanId());
    }
    expect(ids.size).toBe(100);
  });
});
