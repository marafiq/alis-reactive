import { describe, it, expect, beforeEach, vi } from "vitest";
import { JSDOM } from "jsdom";
import type { RequestDescriptor } from "../types";

let execRequest: typeof import("../http/http").execRequest;

function mockFetch(status: number, body: unknown) {
  return vi.fn().mockResolvedValue({
    ok: status >= 200 && status < 300,
    status,
    headers: { get: () => "application/json" },
    json: () => Promise.resolve(body),
    text: () => Promise.resolve(JSON.stringify(body)),
  });
}

beforeEach(async () => {
  const dom = new JSDOM(`<!DOCTYPE html><html><body>
    <div id="result">empty</div>
    <div id="error">none</div>
    <div id="spinner" hidden></div>
    <div id="residents">empty</div>
    <div id="facilities">empty</div>
  </body></html>`);

  (globalThis as any).document = dom.window.document;
  (globalThis as any).CustomEvent = dom.window.CustomEvent;
  (globalThis as any).FormData = dom.window.FormData;

  const mod = await import("../http/http");
  execRequest = mod.execRequest;
});

describe("http request execution", () => {
  it("server success triggers success handler", async () => {
    (globalThis as any).fetch = mockFetch(200, { message: "ok" });

    const req: RequestDescriptor = {
      verb: "GET",
      url: "/api/test",
      onSuccess: [{
        commands: [{ kind: "mutate-element", target: "result", mutation: { kind: "set-prop", prop: "textContent" }, value: "loaded" }],
      }],
    };

    await execRequest(req);

    expect(document.getElementById("result")!.textContent).toBe("loaded");
    expect(fetch).toHaveBeenCalledWith("/api/test", expect.objectContaining({ method: "GET" }));
  });

  it("server error triggers error handler", async () => {
    (globalThis as any).fetch = mockFetch(400, { errors: ["bad"] });

    const req: RequestDescriptor = {
      verb: "POST",
      url: "/api/save",
      onError: [{
        statusCode: 400,
        commands: [{ kind: "mutate-element", target: "error", mutation: { kind: "set-prop", prop: "textContent" }, value: "bad request" }],
      }],
    };

    await execRequest(req);

    expect(document.getElementById("error")!.textContent).toBe("bad request");
  });

  it("loading indicator shows during request and hides after", async () => {
    let resolveReq: (v: unknown) => void;
    (globalThis as any).fetch = vi.fn().mockReturnValue(
      new Promise((resolve) => { resolveReq = resolve; })
    );

    const req: RequestDescriptor = {
      verb: "GET",
      url: "/api/test",
      whileLoading: [
        { kind: "mutate-element", target: "spinner", mutation: { kind: "call", method: "removeAttribute", args: [{ kind: "literal", value: "hidden" }] } },
      ],
      onSuccess: [{
        commands: [{ kind: "mutate-element", target: "spinner", mutation: { kind: "call", method: "setAttribute", args: [{ kind: "literal", value: "hidden" }, { kind: "literal", value: "" }] } }],
      }],
    };

    const promise = execRequest(req);

    // While loading, spinner should be visible
    expect(document.getElementById("spinner")!.hasAttribute("hidden")).toBe(false);

    // Resolve the fetch
    resolveReq!({
      ok: true,
      status: 200,
      headers: { get: () => "application/json" },
      json: () => Promise.resolve({}),
      text: () => Promise.resolve("{}"),
    });

    await promise;

    // After response, success handler hides it
    expect(document.getElementById("spinner")!.hasAttribute("hidden")).toBe(true);
  });

  it("chained request fires after first succeeds", async () => {
    const fetchMock = vi.fn()
      .mockResolvedValueOnce({
        ok: true, status: 200,
        headers: { get: () => "application/json" },
        json: () => Promise.resolve({ data: "residents" }),
        text: () => Promise.resolve('{"data":"residents"}'),
      })
      .mockResolvedValueOnce({
        ok: true, status: 200,
        headers: { get: () => "application/json" },
        json: () => Promise.resolve({ data: "facilities" }),
        text: () => Promise.resolve('{"data":"facilities"}'),
      });
    (globalThis as any).fetch = fetchMock;

    const req: RequestDescriptor = {
      verb: "GET",
      url: "/api/residents",
      onSuccess: [{
        commands: [{ kind: "mutate-element", target: "residents", mutation: { kind: "set-prop", prop: "textContent" }, value: "loaded" }],
      }],
      chained: {
        verb: "GET",
        url: "/api/facilities",
        onSuccess: [{
          commands: [{ kind: "mutate-element", target: "facilities", mutation: { kind: "set-prop", prop: "textContent" }, value: "loaded" }],
        }],
      },
    };

    await execRequest(req);

    expect(document.getElementById("residents")!.textContent).toBe("loaded");
    expect(document.getElementById("facilities")!.textContent).toBe("loaded");
    expect(fetchMock).toHaveBeenCalledTimes(2);
    expect(fetchMock).toHaveBeenNthCalledWith(1, "/api/residents", expect.anything());
    expect(fetchMock).toHaveBeenNthCalledWith(2, "/api/facilities", expect.anything());
  });

  it("gather items are sent as POST body", async () => {
    (globalThis as any).fetch = mockFetch(200, {});

    const req: RequestDescriptor = {
      verb: "POST",
      url: "/api/save",
      gather: [
        { kind: "static", param: "name", value: "test" },
      ],
      onSuccess: [{
        commands: [{ kind: "mutate-element", target: "result", mutation: { kind: "set-prop", prop: "textContent" }, value: "ok" }],
      }],
    };

    await execRequest(req);

    expect(fetch).toHaveBeenCalledWith("/api/save", expect.objectContaining({
      method: "POST",
      body: JSON.stringify({ name: "test" }),
    }));
  });

  it("error handler without matching status code falls through to catch-all", async () => {
    (globalThis as any).fetch = mockFetch(500, { error: "internal" });

    const req: RequestDescriptor = {
      verb: "GET",
      url: "/api/test",
      onError: [
        { statusCode: 400, commands: [{ kind: "mutate-element", target: "error", mutation: { kind: "set-prop", prop: "textContent" }, value: "400" }] },
        { commands: [{ kind: "mutate-element", target: "error", mutation: { kind: "set-prop", prop: "textContent" }, value: "catch-all" }] },
      ],
    };

    await execRequest(req);

    expect(document.getElementById("error")!.textContent).toBe("catch-all");
  });
});
