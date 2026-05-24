import { describe, expect, it } from "vitest";
import { resolveGather } from "../execution/gather";
import type { ObjectProducer, Plan, RequestInput, Shape, ValueProducer } from "../types";

const dateShape: Shape = { kind: "date" };
const noneShape: Shape = { kind: "none" };
const objectShape: Shape = { kind: "object", fields: {}, additional: true };
const isoDate = "2026-01-02T03:04:05.000Z";

const emptyPlan: Plan = {
  version: 3,
  planId: "Runtime.Gather",
  scope: { kind: "root" },
  types: {},
  components: {},
  behaviors: [],
};

function literal(value: string, shape: Shape): ValueProducer {
  return { kind: "literal", value, shape };
}

function literalValue(value: unknown, shape: Shape): ValueProducer {
  return { kind: "literal", value, shape };
}

function objectValue(fields: Record<string, ValueProducer>): ObjectProducer {
  return { kind: "object", fields, shape: noneShape };
}

describe("resolveGather", () => {
  it("formats declared static fields through their own shapes", () => {
    const input: RequestInput = {
      kind: "gather",
      components: [],
      transport: "json",
      statics: {
        kind: "value",
        value: objectValue({
          scheduledFor: literal(isoDate, dateShape),
        }),
      },
      selection: { kind: "explicit" },
    };

    expect(resolveGather(input, "POST", emptyPlan, {}).body).toEqual({
      scheduledFor: isoDate,
    });
  });

  it("formats declared value-body fields through their own shapes", () => {
    const input: RequestInput = {
      kind: "value",
      transport: "json",
      value: objectValue({
        scheduledFor: literal(isoDate, dateShape),
      }),
    };

    expect(resolveGather(input, "POST", emptyPlan, {}).body).toEqual({
      scheduledFor: isoDate,
    });
  });

  it("rejects leaf fields that conflict with already assigned nested fields", () => {
    const input: RequestInput = {
      kind: "value",
      transport: "json",
      value: objectValue({
        "address.city": literal("Seattle", noneShape),
        address: literal("flat-address", noneShape),
      }),
    };

    expect(() => resolveGather(input, "POST", emptyPlan, {}))
      .toThrow('gather key "address" conflicts at "address"');
  });

  it("rejects nested fields that conflict with already assigned leaf fields", () => {
    const input: RequestInput = {
      kind: "value",
      transport: "json",
      value: objectValue({
        address: literal("flat-address", noneShape),
        "address.city": literal("Seattle", noneShape),
      }),
    };

    expect(() => resolveGather(input, "POST", emptyPlan, {}))
      .toThrow('gather key "address.city" conflicts at "address"');
  });

  it("rejects object values sent through scalar query-string slots", () => {
    const input: RequestInput = {
      kind: "value",
      transport: "json",
      value: objectValue({
        metadata: literalValue({ acuity: "high" }, objectShape),
      }),
    };

    expect(() => resolveGather(input, "GET", emptyPlan, {}))
      .toThrow('gather value "metadata" cannot be serialized as a scalar');
  });

  it("rejects object values sent through scalar form-data slots", () => {
    const input: RequestInput = {
      kind: "value",
      transport: "form-data",
      value: objectValue({
        metadata: literalValue({ acuity: "high" }, objectShape),
      }),
    };

    expect(() => resolveGather(input, "POST", emptyPlan, {}))
      .toThrow('gather value "metadata" cannot be serialized as a scalar');
  });
});
