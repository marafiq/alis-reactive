import { afterEach, describe, expect, it } from "vitest";
import { resolveGather } from "../execution/gather";
import type {
  JsonValue,
  PathSegment,
  Plan,
  RequestInput,
  RequestInputAssignment,
  RequestPayloadTarget,
  Shape,
  StructuredPath,
  ValueProducer,
} from "../types";

const stringShape: Shape = { kind: "string" };
const dateShape: Shape = { kind: "date" };
const rawShape: Shape = { kind: "raw" };
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

function literal(value: JsonValue, shape: Shape): ValueProducer {
  return { kind: "literal", value, shape };
}

function readEventPayload(path: string, shape: Shape): ValueProducer {
  return {
    kind: "read",
    from: { kind: "payload", scope: "event", type: { kind: "untyped" } },
    member: path,
    path: structuredPath(path),
    shape,
    access: { kind: "property" },
  };
}

function readUrlParameter(name: string, shape: Shape): ValueProducer {
  return {
    kind: "read",
    from: { kind: "url" },
    member: name,
    path: [],
    shape,
    access: { kind: "property" },
  };
}

function browserValue(value: unknown, shape: Shape): ValueProducer {
  return { kind: "literal", value, shape };
}

function target(name: string): RequestPayloadTarget {
  return { kind: "payload", name, path: structuredPath(name) };
}

function structuredPath(name: string): StructuredPath {
  const [first, ...rest] = name.split(".").map(pathSegment);
  if (first === undefined) throw new Error(`Expected path for ${name}`);

  return [first, ...rest];
}

function pathSegment(part: string): PathSegment {
  return { kind: "property", name: part };
}

function assignment(name: string, source: ValueProducer): RequestInputAssignment {
  return { target: target(name), source };
}

function header(name: string, source: ValueProducer): RequestInputAssignment {
  return { target: { kind: "header", name }, source };
}

function routeParam(name: string, source: ValueProducer): RequestInputAssignment {
  return { target: { kind: "route-param", name }, source };
}

function gatherInput(
  assignments: RequestInputAssignment[],
  bodyFormat: Extract<RequestInput, { kind: "gather" }>["bodyFormat"] = "json",
): RequestInput {
  return {
    kind: "gather",
    assignments,
    bodyFormat,
    sourceSelection: { kind: "explicit" },
  };
}

function arrayShape(item: Shape): Shape {
  return { kind: "array", item };
}

function resolveBody(input: RequestInput, method: "GET" | "POST" = "POST"): Record<string, unknown> | FormData {
  return resolveGather(input, method, emptyPlan, {}).body;
}

afterEach(() => {
  history.replaceState({}, "", "/");
});

describe("resolveGather", () => {
  it("returns an empty payload for requests without gathered input", () => {
    expect(resolveGather({ kind: "none" }, "POST", emptyPlan, {})).toEqual({
      urlParams: [],
      routeParams: {},
      headers: {},
      body: {},
    });
  });

  it("emits JSON bodies from explicit payload assignments", () => {
    const input = gatherInput([
      assignment("resident.scheduledFor", literal(isoDate, dateShape)),
      assignment("resident.metadata", literal({ acuity: "high" }, objectShape)),
      assignment("tags", literal(["fall-risk", "new"], arrayShape(stringShape))),
      assignment("nickname", literal("", stringShape)),
    ]);

    expect(resolveBody(input)).toEqual({
      resident: {
        scheduledFor: isoDate,
        metadata: { acuity: "high" },
      },
      tags: ["fall-risk", "new"],
      nickname: null,
    });
  });

  it("reads event payload values through the shared value producer path", () => {
    const input = gatherInput([
      assignment("resident.name", readEventPayload("detail.name", stringShape)),
    ]);

    expect(resolveGather(input, "POST", emptyPlan, {
      event: { detail: { name: "Ada" } },
    }).body).toEqual({
      resident: { name: "Ada" },
    });
  });

  it("reads URL query values through the shared value producer path", () => {
    history.replaceState({}, "", "/residents?filter=fall%20risk");
    const input = gatherInput([
      assignment("filter", readUrlParameter("filter", stringShape)),
    ]);

    expect(resolveBody(input)).toEqual({
      filter: "fall risk",
    });
  });

  it("emits GET input as query parameters and keeps the request body empty", () => {
    const input = gatherInput([
      assignment("search", literal("Ada Lovelace", stringShape)),
      assignment("tags", literal(["fall risk", "new"], arrayShape(stringShape))),
    ]);

    expect(resolveGather(input, "GET", emptyPlan, {})).toEqual({
      urlParams: [
        "search=Ada%20Lovelace",
        "tags=fall%20risk",
        "tags=new",
      ],
      routeParams: {},
      headers: {},
      body: {},
    });
  });

  it("resolves headers and route params through the same request input projection", () => {
    const input = gatherInput([
      routeParam("residentId", literal("42", stringShape)),
      header("X-Tenant", literal("memory-care", stringShape)),
      assignment("name", literal("Ada", stringShape)),
    ]);

    expect(resolveGather(input, "POST", emptyPlan, {})).toEqual({
      urlParams: [],
      routeParams: { residentId: "42" },
      headers: { "X-Tenant": "memory-care" },
      body: { name: "Ada" },
    });
  });

  it("resolves route params from typed URL value producers", () => {
    history.replaceState({}, "", "/residents?residentId=42");
    const input = gatherInput([
      routeParam("residentId", readUrlParameter("residentId", { kind: "number" })),
    ]);

    expect(resolveGather(input, "GET", emptyPlan, {})).toEqual({
      urlParams: [],
      routeParams: { residentId: "42" },
      headers: {},
      body: {},
    });
  });

  it("uses the last authored assignment when a scalar target is repeated", () => {
    const input = gatherInput([
      routeParam("residentId", literal("41", stringShape)),
      routeParam("residentId", literal("42", stringShape)),
      header("X-Tenant", literal("skilled", stringShape)),
      header("X-Tenant", literal("memory-care", stringShape)),
    ]);

    expect(resolveGather(input, "POST", emptyPlan, {})).toEqual({
      urlParams: [],
      routeParams: { residentId: "42" },
      headers: { "X-Tenant": "memory-care" },
      body: {},
    });
  });

  it("writes form-data from scalar, array, and browser file values", () => {
    const file = new File(["hello"], "summary.txt", { type: "text/plain" });
    const input = gatherInput([
      assignment("resident", literal("Ada", stringShape)),
      assignment("tags", literal(["fall-risk", "new"], arrayShape(stringShape))),
      assignment("documents", browserValue([file], arrayShape(rawShape))),
    ], "form-data");

    const body = resolveBody(input) as FormData;

    expect(body.getAll("resident")).toEqual(["Ada"]);
    expect(body.getAll("tags")).toEqual(["fall-risk", "new"]);
    expect(body.getAll("documents")).toEqual([file]);
  });

  it("uses rawFile wrappers as browser file values for form-data", () => {
    const file = new File(["hello"], "summary.txt", { type: "text/plain" });
    const input = gatherInput([
      assignment("documents", browserValue([{ rawFile: file }], arrayShape(rawShape))),
    ], "form-data");

    const body = resolveBody(input) as FormData;

    expect(body.getAll("documents")).toEqual([file]);
  });

  it("requires form-data body format when browser files are gathered into the request body", () => {
    const file = new File(["hello"], "summary.txt", { type: "text/plain" });
    const input = gatherInput([
      assignment("documents", browserValue([file], arrayShape(rawShape))),
    ]);

    expect(() => resolveBody(input)).toThrow("[alis] File objects require form-data body format");
  });

  it("does not send browser files through query parameters", () => {
    const file = new File(["hello"], "summary.txt", { type: "text/plain" });
    const input = gatherInput([
      assignment("documents", browserValue([file], arrayShape(rawShape))),
    ]);

    expect(() => resolveBody(input, "GET")).toThrow("[alis] File objects cannot be sent via GET");
  });

  it("cannot serialize object values through scalar query-string slots", () => {
    const input = gatherInput([
      assignment("metadata", literal({ acuity: "high" }, objectShape)),
    ]);

    expect(() => resolveBody(input, "GET"))
      .toThrow('gather value "metadata" cannot be serialized as a scalar');
  });

  it("cannot serialize object values through scalar form-data slots", () => {
    const input = gatherInput([
      assignment("metadata", literal({ acuity: "high" }, objectShape)),
    ], "form-data");

    expect(() => resolveBody(input))
      .toThrow('gather value "metadata" cannot be serialized as a scalar');
  });
});
