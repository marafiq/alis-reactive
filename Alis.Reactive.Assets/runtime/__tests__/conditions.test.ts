import { describe, expect, it } from "vitest";
import { evaluateCondition } from "../conditions/conditions";
import type { CompareCondition, Plan, Shape, ValueProducer } from "../types";

const stringShape: Shape = { kind: "string" };
const numberShape: Shape = { kind: "number" };
const booleanShape: Shape = { kind: "boolean" };
const dateShape: Shape = { kind: "date" };
const rawShape: Shape = { kind: "raw" };
const stringArrayShape: Shape = { kind: "array", item: stringShape };
const numberArrayShape: Shape = { kind: "array", item: numberShape };

function plan(): Plan {
  return {
    version: 3,
    planId: "Condition.Runtime",
    scope: { kind: "root" },
    types: {},
    components: {},
    behaviors: [],
  };
}

function literal(value: unknown, shape: Shape): ValueProducer {
  return { kind: "literal", value, shape };
}

describe("condition runtime", () => {
  it("orders numbers through a comparable condition value", () => {
    const condition: CompareCondition = {
      kind: "compare",
      left: literal("72", numberShape),
      op: "gt",
      right: { kind: "value", value: literal(60, numberShape) },
      shape: numberShape,
      itemShape: { kind: "none" },
    };

    expect(evaluateCondition(condition, plan())).toBe(true);
  });

  it("orders dates through the declared date shape", () => {
    const condition: CompareCondition = {
      kind: "compare",
      left: literal("2026-07-15", dateShape),
      op: "gt",
      right: { kind: "value", value: literal("2026-06-01", dateShape) },
      shape: dateShape,
      itemShape: { kind: "none" },
    };

    expect(evaluateCondition(condition, plan())).toBe(true);
  });

  it("compares date-shaped equality with date literal text", () => {
    const condition: CompareCondition = {
      kind: "compare",
      left: literal("2026-07-01", dateShape),
      op: "eq",
      right: { kind: "value", value: literal("2026-07-01", dateShape) },
      shape: dateShape,
      itemShape: { kind: "none" },
    };

    expect(evaluateCondition(condition, plan())).toBe(true);
  });

  it("rejects invalid date ordering instead of treating NaN as behavior", () => {
    const condition: CompareCondition = {
      kind: "compare",
      left: literal("not-a-date", dateShape),
      op: "gt",
      right: { kind: "value", value: literal("2026-06-01", dateShape) },
      shape: dateShape,
      itemShape: { kind: "none" },
    };

    expect(evaluateCondition(condition, plan())).toBe(false);
  });

  it("rejects invalid numeric ordering instead of treating malformed text as zero", () => {
    const condition: CompareCondition = {
      kind: "compare",
      left: literal("not-a-number", numberShape),
      op: "gte",
      right: { kind: "value", value: literal(0, numberShape) },
      shape: numberShape,
      itemShape: { kind: "none" },
    };

    expect(evaluateCondition(condition, plan())).toBe(false);
  });

  it("preserves ordered string comparisons already expressible by the DSL", () => {
    const condition: CompareCondition = {
      kind: "compare",
      left: literal("beta", stringShape),
      op: "gt",
      right: { kind: "value", value: literal("alpha", stringShape) },
      shape: stringShape,
      itemShape: { kind: "none" },
    };

    expect(evaluateCondition(condition, plan())).toBe(true);
  });

  it("preserves ordered boolean comparisons already expressible by the DSL", () => {
    const condition: CompareCondition = {
      kind: "compare",
      left: literal(true, booleanShape),
      op: "gt",
      right: { kind: "value", value: literal(false, booleanShape) },
      shape: booleanShape,
      itemShape: { kind: "none" },
    };

    expect(evaluateCondition(condition, plan())).toBe(true);
  });

  it("rejects mixed ordered comparison domains", () => {
    const condition: CompareCondition = {
      kind: "compare",
      left: literal("72", rawShape),
      op: "gt",
      right: { kind: "value", value: literal(60, rawShape) },
      shape: rawShape,
      itemShape: { kind: "none" },
    };

    expect(evaluateCondition(condition, plan())).toBe(false);
  });

  it("uses the declared item shape for array-contains right operand", () => {
    const condition: CompareCondition = {
      kind: "compare",
      left: literal(["routine", "urgent"], stringArrayShape),
      op: "array-contains",
      right: { kind: "value", value: literal("urgent", stringShape) },
      shape: stringArrayShape,
      itemShape: stringShape,
    };

    expect(evaluateCondition(condition, plan())).toBe(true);
  });

  it("evaluates between through an explicit numeric range", () => {
    const condition: CompareCondition = {
      kind: "compare",
      left: literal("72", numberShape),
      op: "between",
      right: { kind: "value", value: literal([60, 90], numberArrayShape) },
      shape: numberShape,
      itemShape: numberShape,
    };

    expect(evaluateCondition(condition, plan())).toBe(true);
  });

  it("rejects malformed between range descriptors instead of inferring missing bounds", () => {
    const condition: CompareCondition = {
      kind: "compare",
      left: literal(72, numberShape),
      op: "between",
      right: { kind: "value", value: literal([60], numberArrayShape) },
      shape: numberShape,
      itemShape: numberShape,
    };

    expect(() => evaluateCondition(condition, plan()))
      .toThrow("[alis] between comparison range must contain exactly two bounds, got 1");
  });

  it("evaluates date ranges with the same ordered condition value", () => {
    const condition: CompareCondition = {
      kind: "compare",
      left: literal("2026-07-15", dateShape),
      op: "between",
      right: {
        kind: "value",
        value: literal(["2026-06-01", "2026-08-01"], { kind: "array", item: dateShape }),
      },
      shape: dateShape,
      itemShape: dateShape,
    };

    expect(evaluateCondition(condition, plan())).toBe(true);
  });

  it("rejects malformed min-length constraints", () => {
    const condition: CompareCondition = {
      kind: "compare",
      left: literal("clinical note", stringShape),
      op: "min-length",
      right: { kind: "value", value: literal("not-a-length", stringShape) },
      shape: stringShape,
      itemShape: { kind: "none" },
    };

    expect(evaluateCondition(condition, plan())).toBe(false);
  });
});
