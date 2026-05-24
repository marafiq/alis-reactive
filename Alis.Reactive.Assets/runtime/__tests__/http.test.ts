import { afterEach, describe, expect, it, vi } from "vitest";
import { executeRequest, routeHandlers } from "../execution/http";
import type {
  Component,
  JsType,
  PayloadScope,
  Plan,
  Reaction,
  Request,
  Shape,
  ValueProducer,
} from "../types";

const stringShape: Shape = { kind: "string" };
const objectShape: Shape = { kind: "object", fields: {}, additional: true };

afterEach(() => {
  vi.unstubAllGlobals();
  document.body.innerHTML = "";
});

function literal(value: string): ValueProducer {
  return { kind: "literal", value, shape: stringShape };
}

function objectProducer(fields: Record<string, ValueProducer>): ValueProducer {
  return { kind: "object", fields, shape: objectShape };
}

function payloadRead(scope: PayloadScope, member: string): ValueProducer {
  return {
    kind: "read",
    from: { kind: "payload", scope, type: { kind: "untyped" } },
    member,
    path: [{ kind: "property", name: member }],
    shape: stringShape,
    access: { kind: "property" },
  };
}

function setText(component: string, value: ValueProducer): Reaction {
  return {
    kind: "set",
    on: { kind: "component", component },
    property: "textContent",
    value,
  };
}

function request(overrides: Partial<Request>): Request {
  return {
    method: "POST",
    url: "/residents",
    headers: {},
    routeParams: {},
    validation: { kind: "none" },
    input: { kind: "none" },
    before: [],
    success: [],
    error: [],
    complete: [],
    chain: { kind: "terminal" },
    ...overrides,
  };
}

function nativeTextPlan(componentKeys: string[]): Plan {
  const textType: JsType = {
    properties: {
      textContent: {
        path: [{ kind: "property", name: "textContent" }],
        shape: stringShape,
        access: "readwrite",
      },
    },
    methods: {},
    events: {},
  };
  const components = Object.fromEntries(
    componentKeys.map(key => [key, nativeComponent(key)])
  );

  return {
    version: 3,
    planId: "Runtime.HttpExecution",
    scope: { kind: "root" },
    types: { "native.text": textType },
    components,
    behaviors: [],
  };
}

function nativeComponent(id: string): Component {
  return {
    id,
    vendor: "native",
    type: "native.text",
    contribution: { kind: "object-target" },
    binding: { kind: "none" },
    container: { kind: "none" },
  };
}

function responseJson(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { "Content-Type": "application/json" },
  });
}

function mockFetch(responses: Response[]) {
  const fetchMock = vi.fn(async () => {
    const response = responses.shift();
    if (response === undefined) return new Response("", { status: 204 });

    return response;
  });

  vi.stubGlobal("fetch", fetchMock);
  return fetchMock;
}

describe("executeRequest HTTP lifecycle", () => {
  it("prepares request intent, exposes response scope, and completes with the request snapshot", async () => {
    document.body.innerHTML = `
      <span id="before"></span>
      <span id="success"></span>
      <span id="complete"></span>
    `;
    const fetchMock = mockFetch([responseJson({ message: "Saved" })]);
    const saveResident = request({
      url: "/residents/{residentId}",
      routeParams: { residentId: literal("42") },
      headers: { "X-Unit": literal("memory-care") },
      input: {
        kind: "value",
        value: objectProducer({ name: literal("Ada") }),
        transport: "json",
      },
      before: [setText("before", literal("Started"))],
      success: [
        { match: { kind: "any" }, reaction: setText("success", payloadRead("success", "message")) },
      ],
      complete: [setText("complete", payloadRead("request", "name"))],
    });

    await executeRequest(saveResident, nativeTextPlan(["before", "success", "complete"]));

    expect(document.getElementById("before")?.textContent).toBe("Started");
    expect(document.getElementById("success")?.textContent).toBe("Saved");
    expect(document.getElementById("complete")?.textContent).toBe("Ada");
    expect(fetchMock).toHaveBeenCalledTimes(1);

    const [url, init] = fetchMock.mock.calls[0] as [string, RequestInit];
    expect(url).toBe("/residents/42");
    expect(init.method).toBe("POST");
    expect(init.headers).toMatchObject({
      "Content-Type": "application/json",
      "X-Unit": "memory-care",
    });
    expect(init.body).toBe(JSON.stringify({ name: "Ada" }));
  });

  it("runs a follow-up request only after the first request succeeds", async () => {
    document.body.innerHTML = `
      <span id="resident"></span>
      <span id="facility"></span>
    `;
    const fetchMock = mockFetch([
      responseJson({ name: "John Doe" }),
      responseJson({ name: "West Wing" }),
    ]);
    const loadFacility = request({
      method: "GET",
      url: "/facilities/{facilityId}",
      routeParams: { facilityId: literal("west wing") },
      headers: { "X-Hop": literal("second") },
      success: [
        { match: { kind: "any" }, reaction: setText("facility", payloadRead("success", "name")) },
      ],
    });
    const loadResident = request({
      method: "GET",
      url: "/residents/{residentId}",
      routeParams: { residentId: literal("42") },
      headers: { "X-Hop": literal("first") },
      success: [
        { match: { kind: "any" }, reaction: setText("resident", payloadRead("success", "name")) },
      ],
      chain: { kind: "follow-up", next: loadFacility },
    });

    await executeRequest(loadResident, nativeTextPlan(["resident", "facility"]));

    expect(document.getElementById("resident")?.textContent).toBe("John Doe");
    expect(document.getElementById("facility")?.textContent).toBe("West Wing");
    expect(fetchMock).toHaveBeenCalledTimes(2);

    const [firstUrl, firstInit] = fetchMock.mock.calls[0] as [string, RequestInit];
    const [secondUrl, secondInit] = fetchMock.mock.calls[1] as [string, RequestInit];
    expect(firstUrl).toBe("/residents/42");
    expect(firstInit.headers).toMatchObject({ "X-Hop": "first" });
    expect(secondUrl).toBe("/facilities/west%20wing");
    expect(secondInit.headers).toMatchObject({ "X-Hop": "second" });
  });

  it("routes error responses, still completes, and does not run the follow-up request", async () => {
    document.body.innerHTML = `
      <span id="error"></span>
      <span id="complete"></span>
      <span id="follow-up"></span>
    `;
    const fetchMock = mockFetch([responseJson({ errorSummary: "Validation failed" }, 422)]);
    const followUp = request({
      method: "GET",
      url: "/should-not-run",
      success: [
        { match: { kind: "any" }, reaction: setText("follow-up", literal("ran")) },
      ],
    });
    const failingSave = request({
      input: {
        kind: "value",
        value: objectProducer({ name: literal("Ada") }),
        transport: "json",
      },
      error: [
        { match: { kind: "status", status: 422 }, reaction: setText("error", payloadRead("error", "errorSummary")) },
      ],
      complete: [setText("complete", payloadRead("request", "name"))],
      chain: { kind: "follow-up", next: followUp },
    });

    await executeRequest(failingSave, nativeTextPlan(["error", "complete", "follow-up"]));

    expect(document.getElementById("error")?.textContent).toBe("Validation failed");
    expect(document.getElementById("complete")?.textContent).toBe("Ada");
    expect(document.getElementById("follow-up")?.textContent).toBe("");
    expect(fetchMock).toHaveBeenCalledTimes(1);
  });

  it("treats response content type matching as case-insensitive", async () => {
    document.body.innerHTML = `<span id="success"></span>`;
    const fetchMock = mockFetch([
      new Response(JSON.stringify({ message: "Saved" }), {
        status: 200,
        headers: { "Content-Type": "Application/JSON; charset=utf-8" },
      }),
    ]);
    const saveResident = request({
      success: [
        { match: { kind: "any" }, reaction: setText("success", payloadRead("success", "message")) },
      ],
    });

    await executeRequest(saveResident, nativeTextPlan(["success"]));

    expect(document.getElementById("success")?.textContent).toBe("Saved");
    expect(fetchMock).toHaveBeenCalledTimes(1);
  });

  it("parses structured json response content types", async () => {
    document.body.innerHTML = `<span id="error"></span>`;
    const fetchMock = mockFetch([
      new Response(JSON.stringify({ title: "Validation failed" }), {
        status: 422,
        headers: { "Content-Type": "application/problem+json; charset=utf-8" },
      }),
    ]);
    const saveResident = request({
      error: [
        { match: { kind: "any" }, reaction: setText("error", payloadRead("error", "title")) },
      ],
    });

    await executeRequest(saveResident, nativeTextPlan(["error"]));

    expect(document.getElementById("error")?.textContent).toBe("Validation failed");
    expect(fetchMock).toHaveBeenCalledTimes(1);
  });

  it("routes an empty json response body as a successful response with no payload", async () => {
    document.body.innerHTML = `<span id="success"></span>`;
    const fetchMock = mockFetch([
      new Response(null, {
        status: 204,
        headers: { "Content-Type": "application/json" },
      }),
    ]);
    const saveResident = request({
      success: [
        { match: { kind: "any" }, reaction: setText("success", literal("no content")) },
      ],
      error: [
        { match: { kind: "any" }, reaction: setText("success", literal("wrong outcome")) },
      ],
    });

    await executeRequest(saveResident, nativeTextPlan(["success"]));

    expect(document.getElementById("success")?.textContent).toBe("no content");
    expect(fetchMock).toHaveBeenCalledTimes(1);
  });

  it("preserves the request snapshot when the browser fails before a response is available", async () => {
    document.body.innerHTML = `
      <span id="error"></span>
      <span id="complete"></span>
    `;
    const fetchMock = vi.fn(async () => {
      throw new TypeError("Failed to fetch");
    });
    vi.stubGlobal("fetch", fetchMock);
    const failingSave = request({
      input: {
        kind: "value",
        value: objectProducer({ name: literal("Ada") }),
        transport: "json",
      },
      error: [
        { match: { kind: "any" }, reaction: setText("error", payloadRead("request", "name")) },
      ],
      complete: [setText("complete", payloadRead("request", "name"))],
    });

    await executeRequest(failingSave, nativeTextPlan(["error", "complete"]));

    expect(document.getElementById("error")?.textContent).toBe("Ada");
    expect(document.getElementById("complete")?.textContent).toBe("Ada");
    expect(fetchMock).toHaveBeenCalledTimes(1);
  });

  it("routes unresolved route template placeholders before fetch is attempted", async () => {
    document.body.innerHTML = `<span id="error"></span>`;
    const fetchMock = mockFetch([responseJson({ message: "Saved" })]);
    const invalidRequest = request({
      method: "GET",
      url: "/residents/{residentId}",
      error: [
        { match: { kind: "any" }, reaction: setText("error", literal("route missing")) },
      ],
    });

    await executeRequest(invalidRequest, nativeTextPlan(["error"]));

    expect(document.getElementById("error")?.textContent).toBe("route missing");
    expect(fetchMock).not.toHaveBeenCalled();
  });

  it("routes object-valued headers as request preparation failures before fetch", async () => {
    document.body.innerHTML = `<span id="error"></span>`;
    const fetchMock = mockFetch([responseJson({ message: "Saved" })]);
    const invalidRequest = request({
      headers: {
        "X-Metadata": objectProducer({ acuity: literal("high") }),
      },
      error: [
        { match: { kind: "any" }, reaction: setText("error", literal("header failed")) },
      ],
    });

    await executeRequest(invalidRequest, nativeTextPlan(["error"]));

    expect(document.getElementById("error")?.textContent).toBe("header failed");
    expect(fetchMock).not.toHaveBeenCalled();
  });

  it("does not translate before-reaction failures into request outcomes", async () => {
    document.body.innerHTML = `
      <span id="error"></span>
      <span id="complete"></span>
    `;
    const fetchMock = mockFetch([responseJson({ message: "Saved" })]);
    const blockedSave = request({
      before: [setText("missing-before-target", literal("Started"))],
      error: [
        { match: { kind: "any" }, reaction: setText("error", literal("wrong outcome")) },
      ],
      complete: [setText("complete", literal("done"))],
    });

    await expect(executeRequest(blockedSave, nativeTextPlan(["error", "complete"])))
      .rejects.toThrow("missing-before-target");

    expect(document.getElementById("error")?.textContent).toBe("");
    expect(document.getElementById("complete")?.textContent).toBe("");
    expect(fetchMock).not.toHaveBeenCalled();
  });

  it("runs completion after a response outcome even when the selected response reaction fails", async () => {
    document.body.innerHTML = `
      <span id="error"></span>
      <span id="complete"></span>
    `;
    const fetchMock = mockFetch([responseJson({ message: "Saved" })]);
    const saveResident = request({
      success: [
        { match: { kind: "any" }, reaction: setText("missing-success-target", payloadRead("success", "message")) },
      ],
      error: [
        { match: { kind: "any" }, reaction: setText("error", literal("wrong outcome")) },
      ],
      complete: [setText("complete", literal("done"))],
    });

    await expect(executeRequest(saveResident, nativeTextPlan(["error", "complete"])))
      .rejects.toThrow("missing-success-target");

    expect(document.getElementById("error")?.textContent).toBe("");
    expect(document.getElementById("complete")?.textContent).toBe("done");
    expect(fetchMock).toHaveBeenCalledTimes(1);
  });

  it("rejects exact response handlers outside the HTTP status range", async () => {
    document.body.innerHTML = `<span id="error"></span>`;

    await expect(
      routeHandlers(
        [
          { match: { kind: "status", status: 0 }, reaction: setText("error", literal("network")) },
        ],
        400,
        nativeTextPlan(["error"]),
      ),
    ).rejects.toThrow("100 to 599");
  });
});
