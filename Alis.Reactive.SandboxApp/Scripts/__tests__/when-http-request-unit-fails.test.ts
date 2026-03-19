import { describe, it, expect, beforeEach, vi } from "vitest";
import { JSDOM } from "jsdom";
import type { RequestDescriptor } from "../types";

/**
 * BDD: When an HTTP request unit fails before fetch
 *
 * Sequence diagram (from design spec):
 *
 *   execRequest(req, ctx)
 *   try {
 *     whileLoading (sync)           ← can throw
 *     gather → ResolvedFetch        ← can throw
 *     fetch(resolved)               ← can throw (network)
 *     await routeHandlers(...)
 *   } catch (err) {
 *     status = err instanceof TypeError ? 0 : -1
 *     await routeHandlers(req.onError, status, ctx)
 *     return  // no chained
 *   }
 *   if (req.chained) await execRequest(req.chained, ctx)
 *
 * One request = one unit = one error boundary.
 * Any throw in the unit routes to the developer's onError handlers.
 */

let execRequest: typeof import("../execution/http").execRequest;

function mockFetch(status: number, body: unknown) {
  return vi.fn().mockResolvedValue({
    ok: status >= 200 && status < 300,
    status,
    headers: { get: () => "application/json" },
    json: () => Promise.resolve(body),
    text: () => Promise.resolve(JSON.stringify(body)),
  });
}

function setText(target: string, value: string) {
  return {
    kind: "mutate-element" as const,
    target,
    mutation: { kind: "set-prop" as const, prop: "textContent" },
    value,
  };
}

function show(target: string) {
  return {
    kind: "mutate-element" as const,
    target,
    mutation: { kind: "call" as const, method: "removeAttribute", args: [{ kind: "literal" as const, value: "hidden" }] },
  };
}

function hide(target: string) {
  return {
    kind: "mutate-element" as const,
    target,
    mutation: { kind: "call" as const, method: "setAttribute", args: [{ kind: "literal" as const, value: "hidden" }, { kind: "literal" as const, value: "" }] },
  };
}

beforeEach(async () => {
  const dom = new JSDOM(`<!DOCTYPE html><html><body>
    <div id="error">none</div>
    <div id="spinner" hidden></div>
    <div id="status">idle</div>
    <div id="result">empty</div>
  </body></html>`);

  (globalThis as any).document = dom.window.document;
  (globalThis as any).CustomEvent = dom.window.CustomEvent;
  (globalThis as any).FormData = dom.window.FormData;

  const mod = await import("../execution/http");
  execRequest = mod.execRequest;
});

// ═══════════════════════════════════════════════════════════════
// Gather failure — IncludeAll with no components
// ═══════════════════════════════════════════════════════════════

describe("when gather throws in an http request", () => {

  it("routes the error to onError with status -1", async () => {
    (globalThis as any).fetch = vi.fn();

    const req: RequestDescriptor = {
      verb: "POST",
      url: "/api/save",
      gather: [{ kind: "all" }], // IncludeAll with no components → throw
      onError: [{
        statusCode: -1,
        commands: [setText("error", "gather failed")],
      }],
    };

    await execRequest(req);

    expect(document.getElementById("error")!.textContent).toBe("gather failed");
  });

  it("does not call fetch", async () => {
    (globalThis as any).fetch = vi.fn();

    const req: RequestDescriptor = {
      verb: "POST",
      url: "/api/save",
      gather: [{ kind: "all" }],
      onError: [{ commands: [setText("error", "failed")] }],
    };

    await execRequest(req);

    expect(fetch).not.toHaveBeenCalled();
  });

  it("does not fire chained request", async () => {
    (globalThis as any).fetch = vi.fn();

    const req: RequestDescriptor = {
      verb: "POST",
      url: "/api/save",
      gather: [{ kind: "all" }],
      onError: [{ commands: [setText("error", "failed")] }],
      chained: {
        verb: "GET",
        url: "/api/refresh",
        onSuccess: [{ commands: [setText("result", "refreshed")] }],
      },
    };

    await execRequest(req);

    expect(document.getElementById("result")!.textContent).toBe("empty");
    expect(fetch).not.toHaveBeenCalled();
  });

  it("developer can revert whileLoading UI in onError handler", async () => {
    (globalThis as any).fetch = vi.fn();

    // Scenario: nurse clicks "Save Resident" →
    // whileLoading shows spinner → gather throws (component not registered) →
    // onError handler hides spinner and shows error message
    const req: RequestDescriptor = {
      verb: "POST",
      url: "/api/residents",
      whileLoading: [show("spinner")],
      gather: [{ kind: "all" }], // throws — no components
      onError: [{
        statusCode: -1,
        commands: [
          hide("spinner"),
          setText("error", "Form is not properly configured"),
        ],
      }],
    };

    // Before: spinner is hidden
    expect(document.getElementById("spinner")!.hasAttribute("hidden")).toBe(true);

    await execRequest(req);

    // whileLoading showed it, onError(-1) hid it
    expect(document.getElementById("spinner")!.hasAttribute("hidden")).toBe(true);
    expect(document.getElementById("error")!.textContent).toBe("Form is not properly configured");
    expect(fetch).not.toHaveBeenCalled();
  });

  it("catch-all onError fires when no -1 handler registered", async () => {
    (globalThis as any).fetch = vi.fn();

    const req: RequestDescriptor = {
      verb: "POST",
      url: "/api/save",
      gather: [{ kind: "all" }],
      onError: [{
        // no statusCode — catch-all
        commands: [setText("error", "something went wrong")],
      }],
    };

    await execRequest(req);

    expect(document.getElementById("error")!.textContent).toBe("something went wrong");
  });
});

// ═══════════════════════════════════════════════════════════════
// WhileLoading failure — target element missing
// ═══════════════════════════════════════════════════════════════

describe("when whileLoading throws in an http request", () => {

  it("routes the error to onError with status -1", async () => {
    (globalThis as any).fetch = vi.fn();

    const req: RequestDescriptor = {
      verb: "GET",
      url: "/api/data",
      whileLoading: [
        // Target "nonexistent" does not exist → mutateElement throws
        setText("nonexistent", "loading..."),
      ],
      onError: [{
        statusCode: -1,
        commands: [setText("error", "loading UI failed")],
      }],
    };

    await execRequest(req);

    expect(document.getElementById("error")!.textContent).toBe("loading UI failed");
    expect(fetch).not.toHaveBeenCalled();
  });

  it("does not fire chained request", async () => {
    (globalThis as any).fetch = vi.fn();

    const req: RequestDescriptor = {
      verb: "GET",
      url: "/api/data",
      whileLoading: [setText("nonexistent", "loading...")],
      onError: [{ commands: [setText("error", "failed")] }],
      chained: {
        verb: "GET",
        url: "/api/more",
        onSuccess: [{ commands: [setText("result", "loaded")] }],
      },
    };

    await execRequest(req);

    expect(document.getElementById("result")!.textContent).toBe("empty");
    expect(fetch).not.toHaveBeenCalled();
  });
});

// ═══════════════════════════════════════════════════════════════
// Network error — fetch throws TypeError
// ═══════════════════════════════════════════════════════════════

describe("when fetch throws a network error", () => {

  it("routes to onError with status 0", async () => {
    (globalThis as any).fetch = vi.fn().mockRejectedValue(new TypeError("Failed to fetch"));

    const req: RequestDescriptor = {
      verb: "GET",
      url: "/api/data",
      onError: [{
        statusCode: 0,
        commands: [setText("error", "no internet")],
      }],
    };

    await execRequest(req);

    expect(document.getElementById("error")!.textContent).toBe("no internet");
  });

  it("does not fire chained request on network error", async () => {
    (globalThis as any).fetch = vi.fn().mockRejectedValue(new TypeError("Failed to fetch"));

    const req: RequestDescriptor = {
      verb: "POST",
      url: "/api/save",
      onError: [{ commands: [setText("error", "offline")] }],
      chained: {
        verb: "GET",
        url: "/api/refresh",
        onSuccess: [{ commands: [setText("result", "refreshed")] }],
      },
    };

    await execRequest(req);

    expect(document.getElementById("error")!.textContent).toBe("offline");
    expect(document.getElementById("result")!.textContent).toBe("empty");
  });

  it("whileLoading UI remains — developer reverts in onError", async () => {
    (globalThis as any).fetch = vi.fn().mockRejectedValue(new TypeError("Failed to fetch"));

    const req: RequestDescriptor = {
      verb: "POST",
      url: "/api/save",
      whileLoading: [show("spinner")],
      onError: [{
        commands: [
          hide("spinner"),
          setText("error", "Network unavailable"),
        ],
      }],
    };

    await execRequest(req);

    expect(document.getElementById("spinner")!.hasAttribute("hidden")).toBe(true);
    expect(document.getElementById("error")!.textContent).toBe("Network unavailable");
  });
});

// ═══════════════════════════════════════════════════════════════
// Error status convention: -1 (client), 0 (network), 4xx/5xx (server)
// ═══════════════════════════════════════════════════════════════

describe("error status routing", () => {

  it("distinguishes client error (-1) from network error (0) from server error (4xx)", async () => {
    // This test verifies the status convention:
    //   -1 = client error (gather/whileLoading throw)
    //    0 = network error (fetch TypeError)
    //  400 = server error (HTTP response)

    // Test client error (-1)
    (globalThis as any).fetch = vi.fn();
    const clientReq: RequestDescriptor = {
      verb: "POST",
      url: "/api/save",
      gather: [{ kind: "all" }], // throws
      onError: [
        { statusCode: -1, commands: [setText("error", "client")] },
        { statusCode: 0, commands: [setText("error", "network")] },
        { statusCode: 400, commands: [setText("error", "server")] },
      ],
    };

    await execRequest(clientReq);
    expect(document.getElementById("error")!.textContent).toBe("client");
  });

  it("routes network error to status 0 handler — not -1", async () => {
    (globalThis as any).fetch = vi.fn().mockRejectedValue(new TypeError("net"));

    const networkReq: RequestDescriptor = {
      verb: "GET",
      url: "/api/test",
      onError: [
        { statusCode: -1, commands: [setText("error", "client")] },
        { statusCode: 0, commands: [setText("error", "network")] },
        { statusCode: 400, commands: [setText("error", "server")] },
      ],
    };

    await execRequest(networkReq);
    expect(document.getElementById("error")!.textContent).toBe("network");
  });

  it("routes server error to matching status — not -1 or 0", async () => {
    (globalThis as any).fetch = mockFetch(400, { errors: {} });

    const serverReq: RequestDescriptor = {
      verb: "POST",
      url: "/api/save",
      onError: [
        { statusCode: -1, commands: [setText("error", "client")] },
        { statusCode: 0, commands: [setText("error", "network")] },
        { statusCode: 400, commands: [setText("error", "server")] },
      ],
    };

    await execRequest(serverReq);
    expect(document.getElementById("error")!.textContent).toBe("server");
  });
});
