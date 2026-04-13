import { describe, expect, it } from "vitest";
import {
  formatTraceparent,
  generateSpanId,
  generateTraceId,
  isValidLevel,
  parseTraceparent,
  promoteTracingConfig,
  resolveInitialTracingConfig,
  resolveLevel,
  type IncrementalTracingState,
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

describe("resolveInitialTracingConfig — multi-plan config resolution (Codex round 3 finding 2)", () => {
  // root.ts discovers every [data-reactive-plan] element on the page and
  // feeds the union of (dataset.trace, parsed plan) pairs into this helper.
  // Rule: most-verbose level across all plans (dataset-over-plan precedence
  // preserved per-plan), first traceparent in document order wins.

  it("returns off/undefined when no plans are discovered", () => {
    const result = resolveInitialTracingConfig([], []);
    expect(result.level).toBe("off");
    expect(result.traceparent).toBeUndefined();
  });

  it("resolves level and traceparent from a single plan", () => {
    const result = resolveInitialTracingConfig(
      [{ dataset: { trace: "debug" } }],
      [{ traceparent: "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01" }],
    );
    expect(result.level).toBe("debug");
    expect(result.traceparent).toBe("00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01");
    expect(result.invalidTraceparents).toEqual([]);
  });

  it("preserves dataset-over-plan precedence per plan", () => {
    // plan.traceLevel = trace, dataset = error → dataset wins for that plan.
    const result = resolveInitialTracingConfig(
      [{ dataset: { trace: "error" } }],
      [{ traceLevel: "trace" }],
    );
    expect(result.level).toBe("error");
  });

  it("takes the MOST VERBOSE level across multiple plans", () => {
    // plans[0] says info, plans[1] says trace — trace wins globally
    // because the whole runtime uses a singleton level and any plan
    // asking for tracing must get it.
    const result = resolveInitialTracingConfig(
      [
        { dataset: { trace: "info" } },
        { dataset: { trace: "trace" } },
      ],
      [{}, {}],
    );
    expect(result.level).toBe("trace");
  });

  it("reverse order of verbosity also yields the most-verbose level", () => {
    const result = resolveInitialTracingConfig(
      [
        { dataset: { trace: "trace" } },
        { dataset: { trace: "info" } },
      ],
      [{}, {}],
    );
    expect(result.level).toBe("trace");
  });

  it("mixes dataset + plan.traceLevel across multiple plans", () => {
    // plans[0] has plan.traceLevel = warn. plans[1] has dataset = debug.
    // Per-plan resolved levels: warn and debug. Most verbose: debug.
    const result = resolveInitialTracingConfig(
      [{ dataset: {} }, { dataset: { trace: "debug" } }],
      [{ traceLevel: "warn" }, {}],
    );
    expect(result.level).toBe("debug");
  });

  it("invalid dataset.trace values fall through to plan.traceLevel", () => {
    const result = resolveInitialTracingConfig(
      [{ dataset: { trace: "bogus" } }],
      [{ traceLevel: "info" }],
    );
    expect(result.level).toBe("info");
  });

  it("takes the FIRST VALID traceparent in document order", () => {
    const second = "00-aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa-bbbbbbbbbbbbbbbb-01";
    const third = "00-cccccccccccccccccccccccccccccccc-dddddddddddddddd-01";
    const result = resolveInitialTracingConfig(
      [{ dataset: {} }, { dataset: {} }, { dataset: {} }],
      [
        {}, // no traceparent
        { traceparent: second },
        { traceparent: third },
      ],
    );
    expect(result.traceparent).toBe(second);
    expect(result.invalidTraceparents).toEqual([]);
  });

  it("skips a malformed leading traceparent and keeps scanning for a valid one (Codex round 4 finding 3)", () => {
    // Regression: previously the first non-empty traceparent was taken
    // without validation, so a malformed leading plan would suppress
    // correlation for every later plan. The walk now validates with
    // parseTraceparent and continues past rejections.
    const valid = "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01";
    const result = resolveInitialTracingConfig(
      [{ dataset: {} }, { dataset: {} }, { dataset: {} }],
      [
        { traceparent: "not-a-traceparent" }, // malformed
        { traceparent: "" }, // empty (ignored silently)
        { traceparent: valid }, // first valid — wins
      ],
    );
    expect(result.traceparent).toBe(valid);
    expect(result.invalidTraceparents).toEqual([
      { index: 0, value: "not-a-traceparent" },
    ]);
  });

  it("reports every rejected candidate, even ones between valid and no-op plans", () => {
    const valid = "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01";
    const result = resolveInitialTracingConfig(
      [{ dataset: {} }, { dataset: {} }, { dataset: {} }],
      [
        { traceparent: "garbage" },
        { traceparent: "00-00000000000000000000000000000000-00f067aa0ba902b7-01" }, // reserved all-zero
        { traceparent: valid },
      ],
    );
    expect(result.traceparent).toBe(valid);
    expect(result.invalidTraceparents).toHaveLength(2);
    expect(result.invalidTraceparents[0]).toEqual({ index: 0, value: "garbage" });
    expect(result.invalidTraceparents[1].index).toBe(1);
  });

  it("rejects all-zero span-id traceparents (W3C reserved)", () => {
    const result = resolveInitialTracingConfig(
      [{ dataset: {} }],
      [
        {
          traceparent: "00-4bf92f3577b34da6a3ce929d0e0e4736-0000000000000000-01",
        },
      ],
    );
    expect(result.traceparent).toBeUndefined();
    expect(result.invalidTraceparents).toHaveLength(1);
  });

  it("returns undefined traceparent when no plan carries one", () => {
    const result = resolveInitialTracingConfig(
      [{ dataset: { trace: "trace" } }, { dataset: { trace: "error" } }],
      [{}, {}],
    );
    expect(result.traceparent).toBeUndefined();
  });

  it("handles pre-parse call where plans array is empty-object placeholders", () => {
    // root.ts calls this once BEFORE parsing plans, passing dataset info
    // from plan elements and an empty-object array for plans. The helper
    // must not crash on that shape.
    const result = resolveInitialTracingConfig(
      [{ dataset: { trace: "trace" } }, { dataset: { trace: "info" } }],
      [{}, {}],
    );
    expect(result.level).toBe("trace");
    expect(result.traceparent).toBeUndefined();
  });

  it("ignores plan elements beyond the plans array length and vice versa", () => {
    // Defensive: mismatched-length inputs should not panic.
    const result1 = resolveInitialTracingConfig(
      [{ dataset: { trace: "trace" } }],
      [{}, {}, {}],
    );
    expect(result1.level).toBe("trace");

    const result2 = resolveInitialTracingConfig(
      [{ dataset: { trace: "info" } }, { dataset: { trace: "trace" } }],
      [{}],
    );
    // Only plans[0] contributes because plans array stops at length 1.
    expect(result2.level).toBe("info");
  });
});

describe("promoteTracingConfig — incremental fold for root.ts parse loop (Codex round 5 finding 2)", () => {
  // root.ts folds each successfully parsed plan's tracing config into an
  // accumulator BEFORE parsing the next plan element, so a `plan.parse.fail`
  // event on a malformed later element emits at the verbosity the user
  // asked for via `plan.traceLevel` on any earlier successful plan.
  // promoteTracingConfig is the pure fold step.

  const start: IncrementalTracingState = { level: "off", traceparent: undefined };

  it("starts at off and raises to the plan's resolved level", () => {
    const result = promoteTracingConfig(
      start,
      { dataset: {} },
      { traceLevel: "warn" },
      0,
    );
    expect(result.state.level).toBe("warn");
    expect(result.state.traceparent).toBeUndefined();
    expect(result.rejectedTraceparent).toBeUndefined();
  });

  it("is upward-only on level — a quieter plan does NOT lower an already-raised state", () => {
    // First plan asks for trace.
    const after1 = promoteTracingConfig(
      start,
      { dataset: {} },
      { traceLevel: "trace" },
      0,
    );
    expect(after1.state.level).toBe("trace");

    // Second plan only asks for warn.
    const after2 = promoteTracingConfig(
      after1.state,
      { dataset: {} },
      { traceLevel: "warn" },
      1,
    );
    expect(after2.state.level).toBe("trace");
  });

  it("honors dataset-over-plan precedence per plan", () => {
    // plan.traceLevel = trace, dataset = error → dataset wins → level = error.
    const result = promoteTracingConfig(
      start,
      { dataset: { trace: "error" } },
      { traceLevel: "trace" },
      0,
    );
    expect(result.state.level).toBe("error");
  });

  it("first valid traceparent wins and is sticky", () => {
    const first = "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01";
    const second = "00-aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa-bbbbbbbbbbbbbbbb-01";

    const after1 = promoteTracingConfig(
      start,
      { dataset: {} },
      { traceparent: first },
      0,
    );
    expect(after1.state.traceparent).toBe(first);
    expect(after1.rejectedTraceparent).toBeUndefined();

    const after2 = promoteTracingConfig(
      after1.state,
      { dataset: {} },
      { traceparent: second },
      1,
    );
    // Still the first — sticky.
    expect(after2.state.traceparent).toBe(first);
    expect(after2.rejectedTraceparent).toBeUndefined();
  });

  it("reports malformed traceparent and keeps scanning", () => {
    const after1 = promoteTracingConfig(
      start,
      { dataset: {} },
      { traceparent: "garbage" },
      0,
    );
    expect(after1.state.traceparent).toBeUndefined();
    expect(after1.rejectedTraceparent).toEqual({ index: 0, value: "garbage" });

    const valid = "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01";
    const after2 = promoteTracingConfig(
      after1.state,
      { dataset: {} },
      { traceparent: valid },
      1,
    );
    expect(after2.state.traceparent).toBe(valid);
    expect(after2.rejectedTraceparent).toBeUndefined();
  });

  it("pure function — does not mutate the input state", () => {
    const input: IncrementalTracingState = { level: "info", traceparent: undefined };
    const snapshotLevel = input.level;
    const snapshotTp = input.traceparent;

    promoteTracingConfig(
      input,
      { dataset: { trace: "trace" } },
      { traceparent: "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01" },
      0,
    );

    expect(input.level).toBe(snapshotLevel);
    expect(input.traceparent).toBe(snapshotTp);
  });

  it("real-world boot sequence: early plan raises level, later malformed traceparent is rejected, parse.fail semantics visible", () => {
    // Simulate the scenario Codex flagged: plans[0] carries traceLevel=error,
    // plans[1] carries a malformed traceparent, plans[2] succeeds with
    // a valid traceparent. After folding all three, the accumulated
    // state should be: level=error, traceparent=plans[2].traceparent,
    // one rejected traceparent reported at index 1.
    const valid = "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01";

    const after0 = promoteTracingConfig(
      start,
      { dataset: {} },
      { traceLevel: "error" },
      0,
    );
    expect(after0.state.level).toBe("error");

    const after1 = promoteTracingConfig(
      after0.state,
      { dataset: {} },
      { traceparent: "not-a-traceparent" },
      1,
    );
    expect(after1.state.level).toBe("error"); // level preserved
    expect(after1.state.traceparent).toBeUndefined();
    expect(after1.rejectedTraceparent).toEqual({ index: 1, value: "not-a-traceparent" });

    const after2 = promoteTracingConfig(
      after1.state,
      { dataset: {} },
      { traceparent: valid },
      2,
    );
    expect(after2.state.level).toBe("error");
    expect(after2.state.traceparent).toBe(valid);
    expect(after2.rejectedTraceparent).toBeUndefined();
  });
});
