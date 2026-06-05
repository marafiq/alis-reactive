import { afterEach, describe, expect, it } from "vitest";
import { resolveRequestInput } from "../../../execution/requests/gather";
import type {
  JsonValue,
  BrowserObjectContract,
  ComponentObject,
  PathSegment,
  PlanDocument,
  RequestInput,
  RequestInputAssignment,
  RequestPayloadTarget,
  Shape,
  StructuredPath,
  ValueExpression,
} from "../../../types/index";

const stringShape: Shape = { kind: "string" };
const dateShape: Shape = { kind: "date" };
const rawShape: Shape = { kind: "raw" };
const objectShape: Shape = { kind: "object", fields: {}, additional: true };
const isoDate = "2026-01-02T03:04:05.000Z";

const emptyPlan: PlanDocument = {
  version: 3,
  planId: "Runtime.Gather",
  scope: { kind: "root" },
  types: {},
  components: {},
  behaviors: [],
};

function literal(value: JsonValue, shape: Shape): ValueExpression {
  return { kind: "literal", value, shape };
}

function readEventPayload(path: string, shape: Shape): ValueExpression {
  return {
    kind: "read",
    from: { kind: "payload", scope: "event", type: { kind: "untyped" } },
    member: path,
    path: structuredPath(path),
    shape,
    access: { kind: "property" },
  };
}

function readUrlParameter(name: string, shape: Shape): ValueExpression {
  return {
    kind: "read",
    from: { kind: "url" },
    member: name,
    path: [],
    shape,
    access: { kind: "property" },
  };
}

function runtimeLiteral(value: unknown, shape: Shape): ValueExpression {
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

function assignment(name: string, source: ValueExpression): RequestInputAssignment {
  return { target: target(name), source };
}

function header(name: string, source: ValueExpression): RequestInputAssignment {
  return { target: { kind: "header", name }, source };
}

function routeParam(name: string, source: ValueExpression): RequestInputAssignment {
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
    registeredInputs: { kind: "explicit" },
  };
}

function arrayShape(item: Shape): Shape {
  return { kind: "array", item };
}

function resolveBody(input: RequestInput, method: "GET" | "POST" = "POST"): Record<string, unknown> | FormData {
  return resolveRequestInput(input, method, emptyPlan, {}).body;
}

afterEach(() => {
  history.replaceState({}, "", "/");
});

describe("resolveRequestInput", () => {
  it("returns an empty request body for requests without gathered input", () => {
    expect(resolveRequestInput({ kind: "none" }, "POST", emptyPlan, {})).toEqual({
      urlParams: [],
      routeParams: {},
      headers: {},
      body: {},
    });
  });

  it("emits JSON bodies from explicit body field assignments", () => {
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

  it("reads event payload values through the shared value expression path", () => {
    const input = gatherInput([
      assignment("resident.name", readEventPayload("detail.name", stringShape)),
    ]);

    expect(resolveRequestInput(input, "POST", emptyPlan, {
      event: { detail: { name: "Ada" } },
    }).body).toEqual({
      resident: { name: "Ada" },
    });
  });

  it("reads URL query values through the shared value expression path", () => {
    history.replaceState({}, "", "/residents?filter=fall%20risk");
    const input = gatherInput([
      assignment("filter", readUrlParameter("filter", stringShape)),
    ]);

    expect(resolveBody(input)).toEqual({
      filter: "fall risk",
    });
  });

  it("reads component method return values through the shared value expression path", () => {
    document.body.innerHTML = `<div id="shift-schedule"></div>`;
    const schedule = document.getElementById("shift-schedule") as HTMLElement & {
      getEvents(): unknown[];
    };
    schedule.getEvents = () => [{ id: 1, subject: "Rounds" }];
    const scheduleContract: BrowserObjectContract = {
      properties: {},
      methods: {
        getEvents: {
          path: [{ kind: "property", name: "getEvents" }],
          arguments: { kind: "exact", shapes: [] },
          returns: arrayShape(objectShape),
        },
      },
      events: {},
    };
    const scheduleComponent: ComponentObject = {
      id: "shift-schedule",
      vendor: "native",
      type: "fusion.schedule",
      role: { kind: "object-target" },
      binding: { kind: "none" },
      container: { kind: "none" },
    };
    const input = gatherInput([
      assignment("events", {
        kind: "read",
        from: { kind: "component", component: "shift-schedule" },
        member: "getEvents",
        path: [],
        shape: arrayShape(objectShape),
        access: { kind: "method", args: [] },
      }),
    ]);
    const plan: PlanDocument = {
      ...emptyPlan,
      types: { "fusion.schedule": scheduleContract },
      components: { "shift-schedule": scheduleComponent },
    };

    expect(resolveRequestInput(input, "POST", plan, {}).body).toEqual({
      events: [{ id: 1, subject: "Rounds" }],
    });
  });

  it("emits GET input as query parameters and keeps the request body empty", () => {
    const input = gatherInput([
      assignment("search", literal("Ada Lovelace", stringShape)),
      assignment("tags", literal(["fall risk", "new"], arrayShape(stringShape))),
    ]);

    expect(resolveRequestInput(input, "GET", emptyPlan, {})).toEqual({
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

  it("resolves headers and route params through the same request input path", () => {
    const input = gatherInput([
      routeParam("residentId", literal("42", stringShape)),
      header("X-Tenant", literal("memory-care", stringShape)),
      assignment("name", literal("Ada", stringShape)),
    ]);

    expect(resolveRequestInput(input, "POST", emptyPlan, {})).toEqual({
      urlParams: [],
      routeParams: { residentId: "42" },
      headers: { "X-Tenant": "memory-care" },
      body: { name: "Ada" },
    });
  });

  it("resolves route params from typed URL value expressions", () => {
    history.replaceState({}, "", "/residents?residentId=42");
    const input = gatherInput([
      routeParam("residentId", readUrlParameter("residentId", { kind: "number" })),
    ]);

    expect(resolveRequestInput(input, "GET", emptyPlan, {})).toEqual({
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

    expect(resolveRequestInput(input, "POST", emptyPlan, {})).toEqual({
      urlParams: [],
      routeParams: { residentId: "42" },
      headers: { "X-Tenant": "memory-care" },
      body: {},
    });
  });

  it("writes form-data from scalar, array, and File values", () => {
    const file = new File(["hello"], "summary.txt", { type: "text/plain" });
    const input = gatherInput([
      assignment("resident", literal("Ada", stringShape)),
      assignment("tags", literal(["fall-risk", "new"], arrayShape(stringShape))),
      assignment("documents", runtimeLiteral([file], arrayShape(rawShape))),
    ], "form-data");

    const body = resolveBody(input) as FormData;

    expect(body.getAll("resident")).toEqual(["Ada"]);
    expect(body.getAll("tags")).toEqual(["fall-risk", "new"]);
    expect(body.getAll("documents")).toEqual([file]);
  });

  it("uses rawFile wrappers as File values for form-data", () => {
    const file = new File(["hello"], "summary.txt", { type: "text/plain" });
    const input = gatherInput([
      assignment("documents", runtimeLiteral([{ rawFile: file }], arrayShape(rawShape))),
    ], "form-data");

    const body = resolveBody(input) as FormData;

    expect(body.getAll("documents")).toEqual([file]);
  });

  it("requires form-data body format when File values are gathered into the request body", () => {
    const file = new File(["hello"], "summary.txt", { type: "text/plain" });
    const input = gatherInput([
      assignment("documents", runtimeLiteral([file], arrayShape(rawShape))),
    ]);

    expect(() => resolveBody(input)).toThrow("[alis] File objects require form-data body format");
  });

  it("does not send File values through query parameters", () => {
    const file = new File(["hello"], "summary.txt", { type: "text/plain" });
    const input = gatherInput([
      assignment("documents", runtimeLiteral([file], arrayShape(rawShape))),
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
