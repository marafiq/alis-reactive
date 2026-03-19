import { describe, it, expect, beforeEach, vi } from "vitest";
import { JSDOM } from "jsdom";
import type { Plan, RequestDescriptor } from "../types";

let boot: typeof import("../lifecycle/boot").boot;
let execRequest: typeof import("../http/http").execRequest;

function mockFetch(status: number, body: unknown, contentType = "application/json") {
  return vi.fn().mockResolvedValue({
    ok: status >= 200 && status < 300,
    status,
    headers: { get: (h: string) => h.toLowerCase() === "content-type" ? contentType : null },
    json: () => Promise.resolve(body),
    text: () => Promise.resolve(typeof body === "string" ? body : JSON.stringify(body)),
  });
}

function setupDom(...ids: string[]) {
  for (const id of ids) {
    const el = document.createElement("div");
    el.id = id;
    el.textContent = "—";
    document.body.appendChild(el);
  }
}

beforeEach(async () => {
  const dom = new JSDOM(`<!DOCTYPE html><html><body></body></html>`);
  (globalThis as any).document = dom.window.document;
  (globalThis as any).CustomEvent = dom.window.CustomEvent;
  (globalThis as any).Event = dom.window.Event;
  (globalThis as any).FormData = dom.window.FormData;

  const bootMod = await import("../lifecycle/boot");
  boot = bootMod.boot;
  const httpMod = await import("../http/http");
  execRequest = httpMod.execRequest;
});

// ═══════════════════════════════════════════════════════════════
// HTTP verbs — every verb sends the right method
// ═══════════════════════════════════════════════════════════════

describe("HTTP verbs", () => {
  it("GET sends method GET with no body", async () => {
    setupDom("r");
    (globalThis as any).fetch = mockFetch(200, {});
    await execRequest({ verb: "GET", url: "/api/test", onSuccess: [{ commands: [{ kind: "mutate-element", target: "r", mutation: { kind: "set-prop", prop: "textContent" }, value: "ok" }] }] });
    expect(fetch).toHaveBeenCalledWith("/api/test", expect.objectContaining({ method: "GET" }));
    expect(document.getElementById("r")!.textContent).toBe("ok");
  });

  it("POST sends method POST with JSON body", async () => {
    setupDom("r");
    (globalThis as any).fetch = mockFetch(200, {});
    await execRequest({ verb: "POST", url: "/api/save", gather: [{ kind: "static", param: "x", value: 1 }], onSuccess: [{ commands: [{ kind: "mutate-element", target: "r", mutation: { kind: "set-prop", prop: "textContent" }, value: "saved" }] }] });
    expect(fetch).toHaveBeenCalledWith("/api/save", expect.objectContaining({ method: "POST", body: '{"x":1}' }));
  });

  it("PUT sends method PUT", async () => {
    setupDom("r");
    (globalThis as any).fetch = mockFetch(200, {});
    await execRequest({ verb: "PUT", url: "/api/update", gather: [{ kind: "static", param: "name", value: "new" }], onSuccess: [{ commands: [{ kind: "mutate-element", target: "r", mutation: { kind: "set-prop", prop: "textContent" }, value: "updated" }] }] });
    expect(fetch).toHaveBeenCalledWith("/api/update", expect.objectContaining({ method: "PUT" }));
  });

  it("DELETE sends method DELETE", async () => {
    setupDom("r");
    (globalThis as any).fetch = mockFetch(200, {});
    await execRequest({ verb: "DELETE", url: "/api/remove/42", onSuccess: [{ commands: [{ kind: "mutate-element", target: "r", mutation: { kind: "set-prop", prop: "textContent" }, value: "deleted" }] }] });
    expect(fetch).toHaveBeenCalledWith("/api/remove/42", expect.objectContaining({ method: "DELETE" }));
  });
});

// ═══════════════════════════════════════════════════════════════
// Response routing — success, error by code, catch-all
// ═══════════════════════════════════════════════════════════════

describe("response routing", () => {
  it("200 routes to success handler", async () => {
    setupDom("r");
    (globalThis as any).fetch = mockFetch(200, { msg: "ok" });
    await execRequest({ verb: "GET", url: "/api/test", onSuccess: [{ commands: [{ kind: "mutate-element", target: "r", mutation: { kind: "set-prop", prop: "textContent" }, value: "success" }] }] });
    expect(document.getElementById("r")!.textContent).toBe("success");
  });

  it("400 routes to matching error handler", async () => {
    setupDom("r");
    (globalThis as any).fetch = mockFetch(400, {});
    await execRequest({ verb: "POST", url: "/api/save", onError: [{ statusCode: 400, commands: [{ kind: "mutate-element", target: "r", mutation: { kind: "set-prop", prop: "textContent" }, value: "bad request" }] }] });
    expect(document.getElementById("r")!.textContent).toBe("bad request");
  });

  it("422 routes to matching error handler — not 400", async () => {
    setupDom("r400", "r422");
    (globalThis as any).fetch = mockFetch(422, {});
    await execRequest({
      verb: "POST", url: "/api/save",
      onError: [
        { statusCode: 400, commands: [{ kind: "mutate-element", target: "r400", mutation: { kind: "set-prop", prop: "textContent" }, value: "400" }] },
        { statusCode: 422, commands: [{ kind: "mutate-element", target: "r422", mutation: { kind: "set-prop", prop: "textContent" }, value: "422" }] },
      ],
    });
    expect(document.getElementById("r400")!.textContent).toBe("—");
    expect(document.getElementById("r422")!.textContent).toBe("422");
  });

  it("500 falls to catch-all when no specific handler", async () => {
    setupDom("r");
    (globalThis as any).fetch = mockFetch(500, {});
    await execRequest({
      verb: "GET", url: "/api/fail",
      onError: [
        { statusCode: 400, commands: [{ kind: "mutate-element", target: "r", mutation: { kind: "set-prop", prop: "textContent" }, value: "400" }] },
        { commands: [{ kind: "mutate-element", target: "r", mutation: { kind: "set-prop", prop: "textContent" }, value: "catch-all" }] },
      ],
    });
    expect(document.getElementById("r")!.textContent).toBe("catch-all");
  });
});

// ═══════════════════════════════════════════════════════════════
// Typed response — responseBody path resolution
// ═══════════════════════════════════════════════════════════════

describe("typed response body", () => {
  // KNOWN GAP: execRequest() direct call doesn't set responseBody in exec context
  // This works via boot() → executeHttpReaction() which sets ctx.responseBody
  // TODO: test via boot() integration instead of direct execRequest()
  it("success handler sets static values from response", async () => {
    setupDom("rname", "rcount");
    (globalThis as any).fetch = mockFetch(200, { name: "Margaret", count: 3 });
    await execRequest({
      verb: "GET", url: "/api/data",
      onSuccess: [{
        commands: [
          { kind: "mutate-element", target: "rname", mutation: { kind: "set-prop", prop: "textContent" }, value: "loaded" },
          { kind: "mutate-element", target: "rcount", mutation: { kind: "set-prop", prop: "textContent" }, value: "3" },
        ],
      }],
    });
    expect(document.getElementById("rname")!.textContent).toBe("loaded");
    expect(document.getElementById("rcount")!.textContent).toBe("3");
  });

  it("response body source resolution via execRequest", async () => {
    setupDom("rname");
    (globalThis as any).fetch = mockFetch(200, { name: "Margaret" });
    await execRequest({
      verb: "GET", url: "/api/data",
      onSuccess: [{
        commands: [
          { kind: "mutate-element", target: "rname", mutation: { kind: "set-prop", prop: "textContent" }, source: { kind: "event", path: "responseBody.name" } },
        ],
      }],
    });
    // responseBody resolution works — execRequest sets ctx.responseBody from parsed JSON
    expect(document.getElementById("rname")!.textContent).toBe("Margaret");
  });

  it("nested response fields resolve correctly", async () => {
    setupDom("city");
    (globalThis as any).fetch = mockFetch(200, { address: { city: "Portland" } });
    await execRequest({
      verb: "GET", url: "/api/resident",
      onSuccess: [{
        commands: [
          { kind: "mutate-element", target: "city", mutation: { kind: "set-prop", prop: "textContent" }, source: { kind: "event", path: "responseBody.address.city" } },
        ],
      }],
    });
    expect(document.getElementById("city")!.textContent).toBe("Portland");
  });
});

// ═══════════════════════════════════════════════════════════════
// WhileLoading — commands execute during request, response after
// ═══════════════════════════════════════════════════════════════

describe("whileLoading", () => {
  it("loading commands fire immediately, success handler fires after", async () => {
    setupDom("spinner", "status");
    let resolveFetch!: (v: unknown) => void;
    (globalThis as any).fetch = vi.fn().mockReturnValue(new Promise(r => { resolveFetch = r; }));

    const promise = execRequest({
      verb: "GET", url: "/api/slow",
      whileLoading: [{ kind: "mutate-element", target: "spinner", mutation: { kind: "set-prop", prop: "textContent" }, value: "loading" }],
      onSuccess: [{ commands: [{ kind: "mutate-element", target: "status", mutation: { kind: "set-prop", prop: "textContent" }, value: "done" }] }],
    });

    // While loading
    expect(document.getElementById("spinner")!.textContent).toBe("loading");
    expect(document.getElementById("status")!.textContent).toBe("—");

    resolveFetch({ ok: true, status: 200, headers: { get: () => "application/json" }, json: () => Promise.resolve({}), text: () => Promise.resolve("{}") });
    await promise;

    expect(document.getElementById("status")!.textContent).toBe("done");
  });
});

// ═══════════════════════════════════════════════════════════════
// FormData — contentType form-data uses FormData
// ═══════════════════════════════════════════════════════════════

describe("formData content type", () => {
  it("form-data sends FormData instead of JSON", async () => {
    setupDom("r");
    (globalThis as any).fetch = mockFetch(200, {});
    await execRequest({
      verb: "POST", url: "/api/upload",
      contentType: "form-data",
      gather: [{ kind: "static", param: "name", value: "test" }],
      onSuccess: [{ commands: [{ kind: "mutate-element", target: "r", mutation: { kind: "set-prop", prop: "textContent" }, value: "uploaded" }] }],
    });
    const call = (fetch as any).mock.calls[0];
    expect(call[1].body).toBeInstanceOf(FormData);
  });
});

// ═══════════════════════════════════════════════════════════════
// Chained requests — second fires after first succeeds
// ═══════════════════════════════════════════════════════════════

describe("chained requests", () => {
  it("chained request fires only after first succeeds", async () => {
    setupDom("first", "second");
    const fetchMock = vi.fn()
      .mockResolvedValueOnce({ ok: true, status: 200, headers: { get: () => "application/json" }, json: () => Promise.resolve({}), text: () => Promise.resolve("{}") })
      .mockResolvedValueOnce({ ok: true, status: 200, headers: { get: () => "application/json" }, json: () => Promise.resolve({}), text: () => Promise.resolve("{}") });
    (globalThis as any).fetch = fetchMock;

    await execRequest({
      verb: "POST", url: "/api/save",
      onSuccess: [{ commands: [{ kind: "mutate-element", target: "first", mutation: { kind: "set-prop", prop: "textContent" }, value: "saved" }] }],
      chained: {
        verb: "GET", url: "/api/refresh",
        onSuccess: [{ commands: [{ kind: "mutate-element", target: "second", mutation: { kind: "set-prop", prop: "textContent" }, value: "refreshed" }] }],
      },
    });

    expect(document.getElementById("first")!.textContent).toBe("saved");
    expect(document.getElementById("second")!.textContent).toBe("refreshed");
    expect(fetchMock).toHaveBeenCalledTimes(2);
  });

  it("chained request does NOT fire when first fails", async () => {
    setupDom("first", "second");
    (globalThis as any).fetch = mockFetch(500, {});

    await execRequest({
      verb: "POST", url: "/api/save",
      onError: [{ commands: [{ kind: "mutate-element", target: "first", mutation: { kind: "set-prop", prop: "textContent" }, value: "failed" }] }],
      chained: {
        verb: "GET", url: "/api/refresh",
        onSuccess: [{ commands: [{ kind: "mutate-element", target: "second", mutation: { kind: "set-prop", prop: "textContent" }, value: "refreshed" }] }],
      },
    });

    expect(document.getElementById("first")!.textContent).toBe("failed");
    expect(document.getElementById("second")!.textContent).toBe("—");
    expect(fetch).toHaveBeenCalledTimes(1);
  });
});

// ═══════════════════════════════════════════════════════════════
// Gather with GET — params in URL
// ═══════════════════════════════════════════════════════════════

describe("gather in GET requests", () => {
  it("static gather items become URL params", async () => {
    setupDom("r");
    (globalThis as any).fetch = mockFetch(200, {});
    await execRequest({
      verb: "GET", url: "/api/search",
      gather: [{ kind: "static", param: "q", value: "hello" }, { kind: "static", param: "page", value: 1 }],
      onSuccess: [{ commands: [{ kind: "mutate-element", target: "r", mutation: { kind: "set-prop", prop: "textContent" }, value: "found" }] }],
    });
    expect(fetch).toHaveBeenCalledWith("/api/search?q=hello&page=1", expect.anything());
  });
});

// ═══════════════════════════════════════════════════════════════
// Into — HTML content injection
// ═══════════════════════════════════════════════════════════════

describe("Into command", () => {
  it("injects HTML response body into target element", async () => {
    setupDom("container");
    (globalThis as any).fetch = vi.fn().mockResolvedValue({
      ok: true, status: 200,
      headers: { get: (h: string) => h.toLowerCase() === "content-type" ? "text/html" : null },
      text: () => Promise.resolve("<p>Partial content</p>"),
      json: () => Promise.reject("not json"),
    });

    await execRequest({
      verb: "GET", url: "/api/partial",
      onSuccess: [{ commands: [{ kind: "into", target: "container" }] }],
    });

    expect(document.getElementById("container")!.innerHTML).toBe("<p>Partial content</p>");
  });
});

// ═══════════════════════════════════════════════════════════════
// Full integration — boot → trigger → HTTP → response → DOM
// ═══════════════════════════════════════════════════════════════

describe("full boot-to-http integration", () => {
  it("dom-ready triggers HTTP GET, success updates DOM", async () => {
    setupDom("name");
    (globalThis as any).fetch = mockFetch(200, { name: "Margaret" });

    boot({
      planId: "test", components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "http",
          request: {
            verb: "GET", url: "/api/resident",
            onSuccess: [{ commands: [
              { kind: "mutate-element", target: "name", mutation: { kind: "set-prop", prop: "textContent" }, source: { kind: "event", path: "responseBody.name" } },
            ]}],
          },
        },
      }],
    });

    await new Promise(r => setTimeout(r, 50));
    expect(document.getElementById("name")!.textContent).toBe("Margaret");
  });

  it("custom-event triggers HTTP POST with gather, success shows result", async () => {
    setupDom("result");
    const input = document.createElement("input");
    input.id = "NameField";
    input.value = "John";
    document.body.appendChild(input);

    (globalThis as any).fetch = mockFetch(200, { saved: true });

    boot({
      planId: "test",
      components: { "Name": { id: "NameField", vendor: "native", readExpr: "value" } },
      entries: [{
        trigger: { kind: "custom-event", event: "do-save" },
        reaction: {
          kind: "http",
          request: {
            verb: "POST", url: "/api/save",
            gather: [{ kind: "component", componentId: "NameField", vendor: "native", name: "Name", readExpr: "value" }],
            onSuccess: [{ commands: [{ kind: "mutate-element", target: "result", mutation: { kind: "set-prop", prop: "textContent" }, value: "saved" }] }],
          },
        },
      }],
    });

    document.dispatchEvent(new CustomEvent("do-save"));
    await new Promise(r => setTimeout(r, 50));

    expect(document.getElementById("result")!.textContent).toBe("saved");
    expect(fetch).toHaveBeenCalledWith("/api/save", expect.objectContaining({
      method: "POST",
      body: JSON.stringify({ Name: "John" }),
    }));
  });

  it("preFetch commands execute before HTTP request fires", async () => {
    setupDom("spinner", "data");
    document.getElementById("spinner")!.setAttribute("hidden", "");

    (globalThis as any).fetch = mockFetch(200, { items: 5 });

    boot({
      planId: "test", components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "http",
          preFetch: [
            { kind: "mutate-element", target: "spinner", mutation: { kind: "call", method: "removeAttribute", args: [{ kind: "literal", value: "hidden" }] } },
          ],
          request: {
            verb: "GET", url: "/api/data",
            onSuccess: [{ commands: [
              { kind: "mutate-element", target: "spinner", mutation: { kind: "call", method: "setAttribute", args: [{ kind: "literal", value: "hidden" }, { kind: "literal", value: "" }] } },
              { kind: "mutate-element", target: "data", mutation: { kind: "set-prop", prop: "textContent" }, value: "loaded" },
            ]}],
          },
        },
      }],
    });

    await new Promise(r => setTimeout(r, 50));
    expect(document.getElementById("spinner")!.hasAttribute("hidden")).toBe(true);
    expect(document.getElementById("data")!.textContent).toBe("loaded");
  });
});
