import { describe, expect, it } from "vitest";
import { resolveGather } from "../execution/gather";
import type { RequestPayloadAssignment, RequestPayloadTarget, PathSegment, Plan, RequestInput, Shape, StructuredPath, ValueProducer } from "../types";

const dateShape: Shape = { kind: "date" };
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

function target(name: string): RequestPayloadTarget {
  return { name, path: structuredPath(name) };
}

function structuredPath(name: string): StructuredPath {
  const [first, ...rest] = name.split(".").map(pathSegment);
  if (first === undefined) throw new Error(`Expected path for ${name}`);

  return [first, ...rest];
}

function pathSegment(part: string): PathSegment {
  return { kind: "property", name: part };
}

function gatherInput(
  fields: RequestPayloadAssignment[],
  transport: Extract<RequestInput, { kind: "gather" }>["transport"] = "json",
): RequestInput {
  return {
    kind: "gather",
    fields,
    transport,
    selection: { kind: "explicit" },
  };
}

describe("resolveGather", () => {
  it("formats request fields through their own shapes", () => {
    const input = gatherInput([
      {
        target: target("scheduledFor"),
        source: literal(isoDate, dateShape),
      },
    ]);

    expect(resolveGather(input, "POST", emptyPlan, {}).body).toEqual({
      scheduledFor: isoDate,
    });
  });

  it("rejects object values sent through scalar query-string slots", () => {
    const input = gatherInput([
      {
        target: target("metadata"),
        source: literalValue({ acuity: "high" }, objectShape),
      },
    ]);

    expect(() => resolveGather(input, "GET", emptyPlan, {}))
      .toThrow('gather value "metadata" cannot be serialized as a scalar');
  });

  it("rejects object values sent through scalar form-data slots", () => {
    const input = gatherInput([
      {
        target: target("metadata"),
        source: literalValue({ acuity: "high" }, objectShape),
      },
    ], "form-data");

    expect(() => resolveGather(input, "POST", emptyPlan, {}))
      .toThrow('gather value "metadata" cannot be serialized as a scalar');
  });
});
