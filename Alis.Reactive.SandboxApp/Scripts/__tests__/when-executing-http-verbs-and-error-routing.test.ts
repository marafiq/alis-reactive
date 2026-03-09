import { describe, it, expect, beforeEach, vi } from "vitest";
import { JSDOM } from "jsdom";
import type { RequestDescriptor, ExecContext, Reaction, ConditionalReaction } from "../types";

let execRequest: typeof import("../http").execRequest;
let executeReaction: typeof import("../execute").executeReaction;
let executeReactionAsync: typeof import("../execute").executeReactionAsync;

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
    <form id="testForm">
      <input name="FirstName" value="John" />
      <input name="LastName" value="Doe" />
      <input name="Email" value="john@example.com" />
    </form>
  </body></html>`);

  (globalThis as any).document = dom.window.document;
  (globalThis as any).CustomEvent = dom.window.CustomEvent;
  (globalThis as any).Function = dom.window.Function;
  (globalThis as any).FormData = dom.window.FormData;
  (globalThis as any).window = dom.window;

  const httpMod = await import("../http");
  execRequest = httpMod.execRequest;

  const execMod = await import("../execute");
  executeReaction = execMod.executeReaction;
  executeReactionAsync = execMod.executeReactionAsync;
});

describe("PUT verb", () => {
  it("sends PUT method in fetch request", async () => {
    (globalThis as any).fetch = mockFetch(200, { updated: true });

    const req: RequestDescriptor = {
      verb: "PUT",
      url: "/api/update",
      gather: [
        { kind: "static", param: "name", value: "Updated Name" },
        { kind: "static", param: "facilityId", value: "1" },
      ],
      onSuccess: [{
        commands: [{ kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "updated" }],
      }],
    };

    await execRequest(req);

    expect(fetch).toHaveBeenCalledWith("/api/update", expect.objectContaining({
      method: "PUT",
      body: JSON.stringify({ name: "Updated Name", facilityId: "1" }),
    }));
    expect(document.getElementById("result")!.textContent).toBe("updated");
  });
});

describe("DELETE verb", () => {
  it("sends DELETE method in fetch request", async () => {
    (globalThis as any).fetch = mockFetch(200, { deleted: true });

    const req: RequestDescriptor = {
      verb: "DELETE",
      url: "/api/delete/42",
      onSuccess: [{
        commands: [{ kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "deleted" }],
      }],
    };

    await execRequest(req);

    expect(fetch).toHaveBeenCalledWith("/api/delete/42", expect.objectContaining({
      method: "DELETE",
    }));
    expect(document.getElementById("result")!.textContent).toBe("deleted");
  });

  it("DELETE does not send body when no gather items", async () => {
    (globalThis as any).fetch = mockFetch(200, {});

    const req: RequestDescriptor = {
      verb: "DELETE",
      url: "/api/delete/1",
      onSuccess: [{ commands: [] }],
    };

    await execRequest(req);

    const callInit = (fetch as any).mock.calls[0][1];
    expect(callInit.body).toBeUndefined();
  });
});

describe("multi-status error routing", () => {
  it("routes 422 to matching handler, not 400 or catch-all", async () => {
    (globalThis as any).fetch = mockFetch(422, { errors: { Name: ["required"] } });

    const req: RequestDescriptor = {
      verb: "POST",
      url: "/api/validate",
      onError: [
        { statusCode: 400, commands: [{ kind: "mutate-element", target: "error", jsEmit: "el.textContent = val", value: "400 error" }] },
        { statusCode: 422, commands: [{ kind: "mutate-element", target: "error", jsEmit: "el.textContent = val", value: "422 validation" }] },
        { statusCode: 500, commands: [{ kind: "mutate-element", target: "error", jsEmit: "el.textContent = val", value: "500 server" }] },
        { commands: [{ kind: "mutate-element", target: "error", jsEmit: "el.textContent = val", value: "catch-all" }] },
      ],
    };

    await execRequest(req);

    expect(document.getElementById("error")!.textContent).toBe("422 validation");
  });

  it("routes 500 to matching handler", async () => {
    (globalThis as any).fetch = mockFetch(500, { error: "internal" });

    const req: RequestDescriptor = {
      verb: "POST",
      url: "/api/validate",
      onError: [
        { statusCode: 422, commands: [{ kind: "mutate-element", target: "error", jsEmit: "el.textContent = val", value: "422" }] },
        { statusCode: 500, commands: [{ kind: "mutate-element", target: "error", jsEmit: "el.textContent = val", value: "500 server" }] },
      ],
    };

    await execRequest(req);

    expect(document.getElementById("error")!.textContent).toBe("500 server");
  });

  it("falls to catch-all when no status matches", async () => {
    (globalThis as any).fetch = mockFetch(503, {});

    const req: RequestDescriptor = {
      verb: "GET",
      url: "/api/test",
      onError: [
        { statusCode: 422, commands: [{ kind: "mutate-element", target: "error", jsEmit: "el.textContent = val", value: "422" }] },
        { statusCode: 500, commands: [{ kind: "mutate-element", target: "error", jsEmit: "el.textContent = val", value: "500" }] },
        { commands: [{ kind: "mutate-element", target: "error", jsEmit: "el.textContent = val", value: "unknown error" }] },
      ],
    };

    await execRequest(req);

    expect(document.getElementById("error")!.textContent).toBe("unknown error");
  });
});

describe("GET with gather items as URL params", () => {
  it("appends static gather items as query params for GET", async () => {
    (globalThis as any).fetch = mockFetch(200, [{ name: "John Doe" }]);

    const req: RequestDescriptor = {
      verb: "GET",
      url: "/api/search",
      gather: [
        { kind: "static", param: "q", value: "John" },
      ],
      onSuccess: [{
        commands: [{ kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "found" }],
      }],
    };

    await execRequest(req);

    expect(fetch).toHaveBeenCalledWith("/api/search?q=John", expect.objectContaining({ method: "GET" }));
    expect(document.getElementById("result")!.textContent).toBe("found");
  });

  it("does not send body for GET requests even with gather items", async () => {
    (globalThis as any).fetch = mockFetch(200, {});

    const req: RequestDescriptor = {
      verb: "GET",
      url: "/api/data",
      gather: [
        { kind: "static", param: "page", value: "1" },
      ],
      onSuccess: [{ commands: [] }],
    };

    await execRequest(req);

    const callInit = (fetch as any).mock.calls[0][1];
    expect(callInit.body).toBeUndefined();
    expect(callInit.headers).toBeUndefined();
  });
});

describe("IncludeAll gather from form", () => {
  it("gathers all form fields and sends as POST body", async () => {
    (globalThis as any).fetch = mockFetch(200, { count: 3 });

    const req: RequestDescriptor = {
      verb: "POST",
      url: "/api/formdata",
      gather: [
        { kind: "all", formId: "testForm" },
      ],
      onSuccess: [{
        commands: [{ kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "submitted" }],
      }],
    };

    await execRequest(req);

    expect(fetch).toHaveBeenCalledWith("/api/formdata", expect.objectContaining({
      method: "POST",
      body: JSON.stringify({ FirstName: "John", LastName: "Doe", Email: "john@example.com" }),
    }));
    expect(document.getElementById("result")!.textContent).toBe("submitted");
  });
});

describe("confirm guard triggers async HTTP reaction", () => {
  it("executes HTTP reaction in branch body when confirm resolves true", async () => {
    (globalThis as any).fetch = mockFetch(200, { deleted: true });

    // Mock window.alis.confirm to resolve true
    (globalThis as any).window.alis = {
      confirm: vi.fn().mockResolvedValue(true),
    };

    const reaction: ConditionalReaction = {
      kind: "conditional",
      branches: [
        {
          guard: { kind: "confirm", message: "Are you sure?" },
          reaction: {
            kind: "http",
            request: {
              verb: "DELETE",
              url: "/api/delete/42",
              onSuccess: [{
                commands: [{ kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "deleted" }],
              }],
            },
          },
        },
      ],
    };

    // executeReaction detects confirm → switches to async path
    executeReaction(reaction);

    // Wait for async operations to complete
    await new Promise(r => setTimeout(r, 50));

    expect((globalThis as any).window.alis.confirm).toHaveBeenCalledWith("Are you sure?");
    expect(fetch).toHaveBeenCalledWith("/api/delete/42", expect.objectContaining({ method: "DELETE" }));
    expect(document.getElementById("result")!.textContent).toBe("deleted");
  });

  it("does not execute HTTP reaction when confirm resolves false", async () => {
    (globalThis as any).fetch = mockFetch(200, {});

    // Mock window.alis.confirm to resolve false
    (globalThis as any).window.alis = {
      confirm: vi.fn().mockResolvedValue(false),
    };

    const reaction: ConditionalReaction = {
      kind: "conditional",
      branches: [
        {
          guard: { kind: "confirm", message: "Are you sure?" },
          reaction: {
            kind: "http",
            request: {
              verb: "DELETE",
              url: "/api/delete/42",
              onSuccess: [{
                commands: [{ kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "deleted" }],
              }],
            },
          },
        },
      ],
    };

    executeReaction(reaction);

    await new Promise(r => setTimeout(r, 50));

    expect((globalThis as any).window.alis.confirm).toHaveBeenCalledWith("Are you sure?");
    expect(fetch).not.toHaveBeenCalled();
    expect(document.getElementById("result")!.textContent).toBe("empty");
  });
});
