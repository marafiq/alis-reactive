import { describe, it, expect, beforeEach, vi } from "vitest";
import { JSDOM } from "jsdom";


let boot: typeof import("../lifecycle/boot").boot;
let execRequest: typeof import("../execution/http").execRequest;

function mockResponse(status: number, body: unknown) {
  return {
    ok: status >= 200 && status < 300,
    status,
    headers: { get: (h: string) => h.toLowerCase() === "content-type" ? "application/json" : null },
    json: () => Promise.resolve(body),
    text: () => Promise.resolve(JSON.stringify(body)),
  };
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
  const httpMod = await import("../execution/http");
  execRequest = httpMod.execRequest;
});

describe("chained HTTP requests", () => {

  it("chained request fires after first succeeds", async () => {
    setupDom("first", "second");
    const fetchMock = vi.fn()
      .mockResolvedValueOnce(mockResponse(200, { id: 1 }))
      .mockResolvedValueOnce(mockResponse(200, { items: 3 }));
    (globalThis as any).fetch = fetchMock;

    await execRequest({
      verb: "POST", url: "/api/save",
      onSuccess: [{ commands: [{ kind: "mutate-element", target: "first", mutation: { kind: "set-prop", prop: "textContent" }, value: "saved" }] }],
      chained: {
        verb: "GET", url: "/api/list",
        onSuccess: [{ commands: [{ kind: "mutate-element", target: "second", mutation: { kind: "set-prop", prop: "textContent" }, value: "refreshed" }] }],
      },
    });

    expect(document.getElementById("first")!.textContent).toBe("saved");
    expect(document.getElementById("second")!.textContent).toBe("refreshed");
    expect(fetchMock).toHaveBeenCalledTimes(2);
  });

  it("chained request does NOT fire when first fails", async () => {
    setupDom("first", "second");
    (globalThis as any).fetch = vi.fn().mockResolvedValue(mockResponse(500, {}));

    await execRequest({
      verb: "POST", url: "/api/save",
      onError: [{ commands: [{ kind: "mutate-element", target: "first", mutation: { kind: "set-prop", prop: "textContent" }, value: "failed" }] }],
      chained: {
        verb: "GET", url: "/api/list",
        onSuccess: [{ commands: [{ kind: "mutate-element", target: "second", mutation: { kind: "set-prop", prop: "textContent" }, value: "should not appear" }] }],
      },
    });

    expect(document.getElementById("first")!.textContent).toBe("failed");
    expect(document.getElementById("second")!.textContent).toBe("—");
    expect(fetch).toHaveBeenCalledTimes(1);
  });

  it("chained request has its own gather", async () => {
    setupDom("r");
    const fetchMock = vi.fn()
      .mockResolvedValueOnce(mockResponse(200, {}))
      .mockResolvedValueOnce(mockResponse(200, {}));
    (globalThis as any).fetch = fetchMock;

    await execRequest({
      verb: "POST", url: "/api/save",
      gather: [{ kind: "static", param: "name", value: "Margaret" }],
      onSuccess: [{ commands: [{ kind: "mutate-element", target: "r", mutation: { kind: "set-prop", prop: "textContent" }, value: "saved" }] }],
      chained: {
        verb: "GET", url: "/api/search",
        gather: [{ kind: "static", param: "q", value: "Thompson" }],
        onSuccess: [{ commands: [{ kind: "mutate-element", target: "r", mutation: { kind: "set-prop", prop: "textContent" }, value: "found" }] }],
      },
    });

    expect(fetchMock).toHaveBeenNthCalledWith(1, "/api/save", expect.objectContaining({ body: '{"name":"Margaret"}' }));
    expect(fetchMock).toHaveBeenNthCalledWith(2, "/api/search?q=Thompson", expect.anything());
    expect(document.getElementById("r")!.textContent).toBe("found");
  });

  it("chained request reads typed response body", async () => {
    setupDom("name", "count");
    const fetchMock = vi.fn()
      .mockResolvedValueOnce(mockResponse(200, { ok: true }))
      .mockResolvedValueOnce(mockResponse(200, { name: "Margaret", count: 5 }));
    (globalThis as any).fetch = fetchMock;

    await execRequest({
      verb: "POST", url: "/api/save",
      onSuccess: [{ commands: [{ kind: "mutate-element", target: "name", mutation: { kind: "set-prop", prop: "textContent" }, value: "saving..." }] }],
      chained: {
        verb: "GET", url: "/api/detail",
        onSuccess: [{ commands: [
          { kind: "mutate-element", target: "name", mutation: { kind: "set-prop", prop: "textContent" }, source: { kind: "event", path: "responseBody.name" } },
          { kind: "mutate-element", target: "count", mutation: { kind: "set-prop", prop: "textContent" }, source: { kind: "event", path: "responseBody.count" } },
        ]}],
      },
    });

    expect(document.getElementById("name")!.textContent).toBe("Margaret");
    expect(document.getElementById("count")!.textContent).toBe("5");
  });

  it("chained request error routes to its own error handler", async () => {
    setupDom("first", "second");
    const fetchMock = vi.fn()
      .mockResolvedValueOnce(mockResponse(200, {}))
      .mockResolvedValueOnce(mockResponse(404, {}));
    (globalThis as any).fetch = fetchMock;

    await execRequest({
      verb: "POST", url: "/api/save",
      onSuccess: [{ commands: [{ kind: "mutate-element", target: "first", mutation: { kind: "set-prop", prop: "textContent" }, value: "saved" }] }],
      chained: {
        verb: "GET", url: "/api/missing",
        onError: [{ statusCode: 404, commands: [{ kind: "mutate-element", target: "second", mutation: { kind: "set-prop", prop: "textContent" }, value: "not found" }] }],
      },
    });

    expect(document.getElementById("first")!.textContent).toBe("saved");
    expect(document.getElementById("second")!.textContent).toBe("not found");
  });

  it("double chain: A → B → C all succeed in sequence", async () => {
    setupDom("a", "b", "c");
    const fetchMock = vi.fn()
      .mockResolvedValueOnce(mockResponse(200, {}))
      .mockResolvedValueOnce(mockResponse(200, {}))
      .mockResolvedValueOnce(mockResponse(200, {}));
    (globalThis as any).fetch = fetchMock;

    await execRequest({
      verb: "POST", url: "/api/step1",
      onSuccess: [{ commands: [{ kind: "mutate-element", target: "a", mutation: { kind: "set-prop", prop: "textContent" }, value: "1" }] }],
      chained: {
        verb: "POST", url: "/api/step2",
        onSuccess: [{ commands: [{ kind: "mutate-element", target: "b", mutation: { kind: "set-prop", prop: "textContent" }, value: "2" }] }],
        chained: {
          verb: "POST", url: "/api/step3",
          onSuccess: [{ commands: [{ kind: "mutate-element", target: "c", mutation: { kind: "set-prop", prop: "textContent" }, value: "3" }] }],
        },
      },
    });

    expect(document.getElementById("a")!.textContent).toBe("1");
    expect(document.getElementById("b")!.textContent).toBe("2");
    expect(document.getElementById("c")!.textContent).toBe("3");
    expect(fetchMock).toHaveBeenCalledTimes(3);
  });

  it("double chain stops at second failure — third never fires", async () => {
    setupDom("a", "b", "c");
    const fetchMock = vi.fn()
      .mockResolvedValueOnce(mockResponse(200, {}))
      .mockResolvedValueOnce(mockResponse(500, {}));
    (globalThis as any).fetch = fetchMock;

    await execRequest({
      verb: "POST", url: "/api/step1",
      onSuccess: [{ commands: [{ kind: "mutate-element", target: "a", mutation: { kind: "set-prop", prop: "textContent" }, value: "1" }] }],
      chained: {
        verb: "POST", url: "/api/step2",
        onError: [{ commands: [{ kind: "mutate-element", target: "b", mutation: { kind: "set-prop", prop: "textContent" }, value: "failed at 2" }] }],
        chained: {
          verb: "POST", url: "/api/step3",
          onSuccess: [{ commands: [{ kind: "mutate-element", target: "c", mutation: { kind: "set-prop", prop: "textContent" }, value: "3" }] }],
        },
      },
    });

    expect(document.getElementById("a")!.textContent).toBe("1");
    expect(document.getElementById("b")!.textContent).toBe("failed at 2");
    expect(document.getElementById("c")!.textContent).toBe("—");
    expect(fetchMock).toHaveBeenCalledTimes(2);
  });

  it("chained via boot: dom-ready → POST → chained GET → DOM update", async () => {
    setupDom("status", "data");
    const fetchMock = vi.fn()
      .mockResolvedValueOnce(mockResponse(200, {}))
      .mockResolvedValueOnce(mockResponse(200, { result: "loaded" }));
    (globalThis as any).fetch = fetchMock;

    boot({
      planId: "test", components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "http",
          request: {
            verb: "POST", url: "/api/init",
            onSuccess: [{ commands: [{ kind: "mutate-element", target: "status", mutation: { kind: "set-prop", prop: "textContent" }, value: "initialized" }] }],
            chained: {
              verb: "GET", url: "/api/data",
              onSuccess: [{ commands: [
                { kind: "mutate-element", target: "data", mutation: { kind: "set-prop", prop: "textContent" }, source: { kind: "event", path: "responseBody.result" } },
              ]}],
            },
          },
        },
      }],
    });

    await new Promise(r => setTimeout(r, 50));
    expect(document.getElementById("status")!.textContent).toBe("initialized");
    expect(document.getElementById("data")!.textContent).toBe("loaded");
  });
});

describe("parallel HTTP requests", () => {

  it("two parallel GETs both fire and both success handlers run", async () => {
    setupDom("r1", "r2");
    const fetchMock = vi.fn()
      .mockResolvedValueOnce(mockResponse(200, { a: "residents" }))
      .mockResolvedValueOnce(mockResponse(200, { b: "facilities" }));
    (globalThis as any).fetch = fetchMock;

    boot({
      planId: "test", components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "parallel-http",
          requests: [
            { verb: "GET", url: "/api/residents", onSuccess: [{ commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "residents loaded" }] }] },
            { verb: "GET", url: "/api/facilities", onSuccess: [{ commands: [{ kind: "mutate-element", target: "r2", mutation: { kind: "set-prop", prop: "textContent" }, value: "facilities loaded" }] }] },
          ],
        },
      }],
    });

    await new Promise(r => setTimeout(r, 50));
    expect(document.getElementById("r1")!.textContent).toBe("residents loaded");
    expect(document.getElementById("r2")!.textContent).toBe("facilities loaded");
    expect(fetchMock).toHaveBeenCalledTimes(2);
  });

  it("parallel: one succeeds one fails — both handlers fire independently", async () => {
    setupDom("r1", "r2");
    const fetchMock = vi.fn()
      .mockResolvedValueOnce(mockResponse(200, {}))
      .mockResolvedValueOnce(mockResponse(500, {}));
    (globalThis as any).fetch = fetchMock;

    boot({
      planId: "test", components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "parallel-http",
          requests: [
            { verb: "GET", url: "/api/ok", onSuccess: [{ commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "ok" }] }] },
            { verb: "GET", url: "/api/fail", onError: [{ commands: [{ kind: "mutate-element", target: "r2", mutation: { kind: "set-prop", prop: "textContent" }, value: "error" }] }] },
          ],
        },
      }],
    });

    await new Promise(r => setTimeout(r, 50));
    expect(document.getElementById("r1")!.textContent).toBe("ok");
    expect(document.getElementById("r2")!.textContent).toBe("error");
  });

  it("parallel with onAllSettled fires after all complete", async () => {
    setupDom("r1", "r2", "done");
    const fetchMock = vi.fn()
      .mockResolvedValueOnce(mockResponse(200, {}))
      .mockResolvedValueOnce(mockResponse(200, {}));
    (globalThis as any).fetch = fetchMock;

    boot({
      planId: "test", components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "parallel-http",
          requests: [
            { verb: "GET", url: "/api/a", onSuccess: [{ commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "a done" }] }] },
            { verb: "GET", url: "/api/b", onSuccess: [{ commands: [{ kind: "mutate-element", target: "r2", mutation: { kind: "set-prop", prop: "textContent" }, value: "b done" }] }] },
          ],
          onAllSettled: [{ kind: "mutate-element", target: "done", mutation: { kind: "set-prop", prop: "textContent" }, value: "all settled" }],
        },
      }],
    });

    await new Promise(r => setTimeout(r, 50));
    expect(document.getElementById("r1")!.textContent).toBe("a done");
    expect(document.getElementById("r2")!.textContent).toBe("b done");
    expect(document.getElementById("done")!.textContent).toBe("all settled");
  });

  it("parallel with preFetch commands execute before requests fire", async () => {
    setupDom("spinner", "r1", "r2");
    document.getElementById("spinner")!.setAttribute("hidden", "");
    const fetchMock = vi.fn()
      .mockResolvedValueOnce(mockResponse(200, {}))
      .mockResolvedValueOnce(mockResponse(200, {}));
    (globalThis as any).fetch = fetchMock;

    boot({
      planId: "test", components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "parallel-http",
          preFetch: [
            { kind: "mutate-element", target: "spinner", mutation: { kind: "call", method: "removeAttribute", args: [{ kind: "literal", value: "hidden" }] } },
          ],
          requests: [
            { verb: "GET", url: "/api/a", onSuccess: [{ commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "a" }] }] },
            { verb: "GET", url: "/api/b", onSuccess: [{ commands: [{ kind: "mutate-element", target: "r2", mutation: { kind: "set-prop", prop: "textContent" }, value: "b" }] }] },
          ],
          onAllSettled: [
            { kind: "mutate-element", target: "spinner", mutation: { kind: "call", method: "setAttribute", args: [{ kind: "literal", value: "hidden" }, { kind: "literal", value: "" }] } },
          ],
        },
      }],
    });

    await new Promise(r => setTimeout(r, 50));
    expect(document.getElementById("r1")!.textContent).toBe("a");
    expect(document.getElementById("r2")!.textContent).toBe("b");
    expect(document.getElementById("spinner")!.hasAttribute("hidden")).toBe(true);
  });

  it("parallel reads typed response bodies from each request", async () => {
    setupDom("name1", "name2");
    const fetchMock = vi.fn()
      .mockResolvedValueOnce(mockResponse(200, { name: "Margaret" }))
      .mockResolvedValueOnce(mockResponse(200, { name: "Eleanor" }));
    (globalThis as any).fetch = fetchMock;

    boot({
      planId: "test", components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "parallel-http",
          requests: [
            { verb: "GET", url: "/api/resident/1", onSuccess: [{ commands: [
              { kind: "mutate-element", target: "name1", mutation: { kind: "set-prop", prop: "textContent" }, source: { kind: "event", path: "responseBody.name" } },
            ]}] },
            { verb: "GET", url: "/api/resident/2", onSuccess: [{ commands: [
              { kind: "mutate-element", target: "name2", mutation: { kind: "set-prop", prop: "textContent" }, source: { kind: "event", path: "responseBody.name" } },
            ]}] },
          ],
        },
      }],
    });

    await new Promise(r => setTimeout(r, 50));
    expect(document.getElementById("name1")!.textContent).toBe("Margaret");
    expect(document.getElementById("name2")!.textContent).toBe("Eleanor");
  });

  it("three parallel requests all fire concurrently", async () => {
    setupDom("a", "b", "c");
    const fetchMock = vi.fn()
      .mockResolvedValueOnce(mockResponse(200, {}))
      .mockResolvedValueOnce(mockResponse(200, {}))
      .mockResolvedValueOnce(mockResponse(200, {}));
    (globalThis as any).fetch = fetchMock;

    boot({
      planId: "test", components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "parallel-http",
          requests: [
            { verb: "GET", url: "/api/a", onSuccess: [{ commands: [{ kind: "mutate-element", target: "a", mutation: { kind: "set-prop", prop: "textContent" }, value: "a" }] }] },
            { verb: "GET", url: "/api/b", onSuccess: [{ commands: [{ kind: "mutate-element", target: "b", mutation: { kind: "set-prop", prop: "textContent" }, value: "b" }] }] },
            { verb: "GET", url: "/api/c", onSuccess: [{ commands: [{ kind: "mutate-element", target: "c", mutation: { kind: "set-prop", prop: "textContent" }, value: "c" }] }] },
          ],
        },
      }],
    });

    await new Promise(r => setTimeout(r, 50));
    expect(document.getElementById("a")!.textContent).toBe("a");
    expect(document.getElementById("b")!.textContent).toBe("b");
    expect(document.getElementById("c")!.textContent).toBe("c");
    expect(fetchMock).toHaveBeenCalledTimes(3);
  });
});
