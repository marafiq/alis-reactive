// BDD: gather serializer must produce correct wire form per field's shape.
// The plan carries per-field Shape on each ValueProducer — the runtime must
// honor it across transports (JSON, FormData, query). This is the HTTP boundary
// that consumes shape-applied values and emits strings/structures appropriate
// to the destination.

import { describe, it, expect, beforeAll } from "vitest";
import { resolveGather } from "../execution/gather";
import { evaluateValue } from "../core/evaluate";
import { setValueEvaluator } from "../conditions/conditions";
import type { Plan, GatherInput, ValueProducer, Shape } from "../types";

beforeAll(() => setValueEvaluator(evaluateValue));

function emptyPlan(): Plan {
  return {
    version: 3,
    planId: "test",
    types: {},
    objects: {},
    contracts: {},
    bindings: {},
    workflows: [],
  } as unknown as Plan;
}

const S = {
  string: { kind: "string" } as Shape,
  number: { kind: "number" } as Shape,
  boolean: { kind: "boolean" } as Shape,
  date: { kind: "date" } as Shape,
  nullableDate: { kind: "nullable", inner: { kind: "date" } } as Shape,
  nullableString: { kind: "nullable", inner: { kind: "string" } } as Shape,
  nullableNumber: { kind: "nullable", inner: { kind: "number" } } as Shape,
  arrayOfString: { kind: "array", item: { kind: "string" } } as Shape,
  arrayOfDate: { kind: "array", item: { kind: "date" } } as Shape,
};

const lit = (value: unknown, shape: Shape): ValueProducer =>
  ({ kind: "literal", value, shape }) as ValueProducer;

function jsonGather(fields: Array<[string, ValueProducer]>): GatherInput {
  return {
    kind: "gather",
    components: fields.map(([key, value]) => ({ key, value })),
    transport: "json",
    statics: { kind: "none" } as unknown as ValueProducer,
  } as unknown as GatherInput;
}

function formDataGather(fields: Array<[string, ValueProducer]>): GatherInput {
  return {
    kind: "gather",
    components: fields.map(([key, value]) => ({ key, value })),
    transport: "form-data",
    statics: { kind: "none" } as unknown as ValueProducer,
  } as unknown as GatherInput;
}

// ── JSON transport ──

describe("JSON transport — each field serialized per its shape", () => {
  const plan = emptyPlan();

  it("serializes a string field as a raw string", () => {
    const input = jsonGather([["name", lit("Dr. Smith", S.string)]]);
    const r = resolveGather(input, "POST", plan);
    expect((r.body as Record<string, unknown>).name).toBe("Dr. Smith");
  });

  it("serializes a number field as a number", () => {
    const input = jsonGather([["age", lit(42, S.number)]]);
    const r = resolveGather(input, "POST", plan);
    expect((r.body as Record<string, unknown>).age).toBe(42);
  });

  it("serializes a boolean field as a boolean", () => {
    const input = jsonGather([["isActive", lit(true, S.boolean)]]);
    const r = resolveGather(input, "POST", plan);
    expect((r.body as Record<string, unknown>).isActive).toBe(true);
  });

  it("serializes a Date field as an ISO string", () => {
    const input = jsonGather([["admissionDate", lit("2025-06-15", S.date)]]);
    const r = resolveGather(input, "POST", plan);
    const body = r.body as Record<string, unknown>;
    expect(typeof body.admissionDate).toBe("string");
    expect(body.admissionDate).toMatch(/^2025-06-15T/);
  });

  it("serializes a Nullable(Date) field with null as null", () => {
    const input = jsonGather([["dischargeDate", lit(null, S.nullableDate)]]);
    const r = resolveGather(input, "POST", plan);
    expect((r.body as Record<string, unknown>).dischargeDate).toBeNull();
  });

  it("serializes a Nullable(Date) field with a value as an ISO string", () => {
    const input = jsonGather([["dischargeDate", lit("2025-12-31", S.nullableDate)]]);
    const r = resolveGather(input, "POST", plan);
    const body = r.body as Record<string, unknown>;
    expect(body.dischargeDate).toMatch(/^2025-12-31T/);
  });

  it("serializes a Nullable(Number) field with null as null", () => {
    const input = jsonGather([["amount", lit(null, S.nullableNumber)]]);
    const r = resolveGather(input, "POST", plan);
    expect((r.body as Record<string, unknown>).amount).toBeNull();
  });

  it("serializes an Array(String) field as a JSON array of strings", () => {
    const input = jsonGather([["tags", lit(["a", "b", "c"], S.arrayOfString)]]);
    const r = resolveGather(input, "POST", plan);
    expect((r.body as Record<string, unknown>).tags).toEqual(["a", "b", "c"]);
  });

  it("serializes an Array(Date) field as an array of ISO strings", () => {
    const input = jsonGather([["dates", lit(["2025-01-01", "2025-12-31"], S.arrayOfDate)]]);
    const r = resolveGather(input, "POST", plan);
    const dates = (r.body as Record<string, unknown>).dates as string[];
    expect(dates).toHaveLength(2);
    expect(dates[0]).toMatch(/^2025-01-01T/);
    expect(dates[1]).toMatch(/^2025-12-31T/);
  });

  it("serializes multiple typed fields in one request body", () => {
    const input = jsonGather([
      ["name", lit("Dr. Smith", S.string)],
      ["age", lit(42, S.number)],
      ["admissionDate", lit("2025-06-15", S.date)],
      ["dischargeDate", lit(null, S.nullableDate)],
      ["tags", lit(["urgent", "new"], S.arrayOfString)],
    ]);
    const r = resolveGather(input, "POST", plan);
    const body = r.body as Record<string, unknown>;
    expect(body.name).toBe("Dr. Smith");
    expect(body.age).toBe(42);
    expect(typeof body.admissionDate).toBe("string");
    expect(body.dischargeDate).toBeNull();
    expect(body.tags).toEqual(["urgent", "new"]);
  });
});

// ── GET / query transport ──

describe("GET request — fields serialized as URL-encoded params", () => {
  const plan = emptyPlan();

  it("serializes a string field as a URL param", () => {
    const input = jsonGather([["name", lit("Dr. Smith", S.string)]]);
    const r = resolveGather(input, "GET", plan);
    expect(r.urlParams).toContain("name=Dr.%20Smith");
  });

  it("serializes a Date field as an ISO string in the URL", () => {
    const input = jsonGather([["when", lit("2025-06-15", S.date)]]);
    const r = resolveGather(input, "GET", plan);
    expect(r.urlParams.some(p => p.startsWith("when=2025-06-15"))).toBe(true);
  });

  it("serializes a Nullable(Date) null as an empty value (via wire-format → null → formatted)", () => {
    const input = jsonGather([["when", lit(null, S.nullableDate)]]);
    const r = resolveGather(input, "GET", plan);
    // null serialization: wire is null → serializeValue warns and returns empty string
    expect(r.urlParams.some(p => p.startsWith("when="))).toBe(true);
  });

  it("serializes an array field as repeated params — one entry per element", () => {
    const input = jsonGather([["tag", lit(["a", "b", "c"], S.arrayOfString)]]);
    const r = resolveGather(input, "GET", plan);
    expect(r.urlParams.filter(p => p.startsWith("tag=")).length).toBe(3);
  });
});

// ── FormData transport ──

describe("FormData transport — fields appended as string entries", () => {
  const plan = emptyPlan();

  it("serializes a string field as a FormData entry", () => {
    const input = formDataGather([["name", lit("Dr. Smith", S.string)]]);
    const r = resolveGather(input, "POST", plan);
    expect(r.body).toBeInstanceOf(FormData);
    expect((r.body as FormData).get("name")).toBe("Dr. Smith");
  });

  it("serializes a Date field as an ISO string FormData entry", () => {
    const input = formDataGather([["admissionDate", lit("2025-06-15", S.date)]]);
    const r = resolveGather(input, "POST", plan);
    const val = (r.body as FormData).get("admissionDate");
    expect(typeof val).toBe("string");
    expect(val as string).toMatch(/^2025-06-15T/);
  });

  it("serializes a number field as its string form", () => {
    const input = formDataGather([["age", lit(42, S.number)]]);
    const r = resolveGather(input, "POST", plan);
    expect((r.body as FormData).get("age")).toBe("42");
  });

  it("serializes an Array(String) field as multiple entries for the same key", () => {
    const input = formDataGather([["tag", lit(["a", "b"], S.arrayOfString)]]);
    const r = resolveGather(input, "POST", plan);
    expect((r.body as FormData).getAll("tag")).toEqual(["a", "b"]);
  });
});
