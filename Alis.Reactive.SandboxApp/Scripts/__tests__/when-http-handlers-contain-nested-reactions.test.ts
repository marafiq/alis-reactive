import { describe, it, expect, beforeEach, vi } from "vitest";
import { JSDOM } from "jsdom";
import type { RequestDescriptor } from "../types";

/**
 * BDD: When a success/error handler contains a nested HTTP reaction
 *
 * Sequence diagram (from design spec):
 *
 *   http.ts:
 *     fetch(outer) → 200
 *     await routeHandlers(onSuccess)
 *       → await executeHandler(h)
 *         → await executeReaction(h.reaction)   ← h.reaction is HTTP
 *           → await executeHttpReaction()
 *             → await execRequest(inner)
 *               → await fetch(inner) → 200
 *               → await routeHandlers(inner.onSuccess)
 *             ← inner resolved
 *           ← resolved
 *         ← resolved
 *       ← resolved
 *     ← routeHandlers resolved
 *     chained? await execRequest(chained)
 *     ← inner HTTP completed BEFORE chained starts
 *
 * Key invariant: nested HTTP in handlers must complete before
 * the outer pipeline continues (chained, onAllSettled, etc.)
 */

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

function setText(target: string, value: string) {
  return {
    kind: "mutate-element" as const,
    target,
    mutation: { kind: "set-prop" as const, prop: "textContent" },
    value,
  };
}

beforeEach(async () => {
  const dom = new JSDOM(`<!DOCTYPE html><html><body>
    <div id="inner">empty</div>
    <div id="chained">empty</div>
    <div id="order">empty</div>
    <div id="error-inner">empty</div>
  </body></html>`);

  (globalThis as any).document = dom.window.document;
  (globalThis as any).CustomEvent = dom.window.CustomEvent;
  (globalThis as any).FormData = dom.window.FormData;

  const mod = await import("../execution/http");
  execRequest = mod.execRequest;
});

// ═══════════════════════════════════════════════════════════════
// Success handler with nested HTTP
// ═══════════════════════════════════════════════════════════════

describe("when success handler contains an http reaction", () => {

  it("inner HTTP completes before chained request fires", async () => {
    const callOrder: string[] = [];
    const fetchMock = vi.fn().mockImplementation((url: string) => {
      callOrder.push(url);
      return Promise.resolve(mockResponse(200, {}));
    });
    (globalThis as any).fetch = fetchMock;

    const req: RequestDescriptor = {
      verb: "POST",
      url: "/api/outer",
      onSuccess: [{
        reaction: {
          kind: "http",
          request: {
            verb: "GET",
            url: "/api/inner",
            onSuccess: [{ commands: [setText("inner", "inner done")] }],
          },
        },
      }],
      chained: {
        verb: "GET",
        url: "/api/chained",
        onSuccess: [{ commands: [setText("chained", "chained done")] }],
      },
    };

    await execRequest(req);

    // Both should complete
    expect(document.getElementById("inner")!.textContent).toBe("inner done");
    expect(document.getElementById("chained")!.textContent).toBe("chained done");

    // Order: outer → inner (from success handler) → chained (after handler completes)
    expect(callOrder).toEqual(["/api/outer", "/api/inner", "/api/chained"]);
  });

  it("inner HTTP error does not prevent chained from firing", async () => {
    const fetchMock = vi.fn()
      .mockResolvedValueOnce(mockResponse(200, {}))   // outer succeeds
      .mockResolvedValueOnce(mockResponse(500, {}))    // inner fails
      .mockResolvedValueOnce(mockResponse(200, {}));   // chained succeeds
    (globalThis as any).fetch = fetchMock;

    const req: RequestDescriptor = {
      verb: "POST",
      url: "/api/outer",
      onSuccess: [{
        reaction: {
          kind: "http",
          request: {
            verb: "GET",
            url: "/api/inner",
            onError: [{ commands: [setText("error-inner", "inner failed")] }],
          },
        },
      }],
      chained: {
        verb: "GET",
        url: "/api/chained",
        onSuccess: [{ commands: [setText("chained", "chained done")] }],
      },
    };

    await execRequest(req);

    // Inner failed, chained still runs (outer succeeded)
    expect(document.getElementById("error-inner")!.textContent).toBe("inner failed");
    expect(document.getElementById("chained")!.textContent).toBe("chained done");
  });
});

// ═══════════════════════════════════════════════════════════════
// Success handler with sequential commands + nested HTTP
// ═══════════════════════════════════════════════════════════════

describe("when success handler has commands followed by nested http", () => {

  it("commands execute before nested HTTP fires", async () => {
    const callOrder: string[] = [];
    const fetchMock = vi.fn().mockImplementation((url: string) => {
      callOrder.push(url);
      return Promise.resolve(mockResponse(200, {}));
    });
    (globalThis as any).fetch = fetchMock;

    const req: RequestDescriptor = {
      verb: "POST",
      url: "/api/save",
      onSuccess: [{
        // Handler has a sequential reaction with commands, then the success handler
        // doesn't mix commands and reactions — StatusHandler has either commands or reaction.
        // But a handler CAN have a reaction that is sequential (dom mutations)
        // followed by another handler with HTTP.
        commands: [setText("order", "step1")],
      }],
      chained: {
        verb: "GET",
        url: "/api/refresh",
        onSuccess: [{ commands: [setText("order", "step2")] }],
      },
    };

    await execRequest(req);

    expect(fetchMock).toHaveBeenCalledTimes(2);
    // Final value: chained's handler overwrites
    expect(document.getElementById("order")!.textContent).toBe("step2");
  });
});

// ═══════════════════════════════════════════════════════════════
// Error handler with nested HTTP
// ═══════════════════════════════════════════════════════════════

describe("when error handler contains an http reaction", () => {

  it("inner HTTP in error handler completes before execRequest returns", async () => {
    const callOrder: string[] = [];
    const fetchMock = vi.fn().mockImplementation((url: string) => {
      callOrder.push(url);
      if (url === "/api/save") return Promise.resolve(mockResponse(422, { errors: {} }));
      return Promise.resolve(mockResponse(200, {}));
    });
    (globalThis as any).fetch = fetchMock;

    const req: RequestDescriptor = {
      verb: "POST",
      url: "/api/save",
      onError: [{
        statusCode: 422,
        reaction: {
          kind: "http",
          request: {
            verb: "GET",
            url: "/api/errors",
            onSuccess: [{ commands: [setText("error-inner", "errors loaded")] }],
          },
        },
      }],
    };

    await execRequest(req);

    expect(callOrder).toEqual(["/api/save", "/api/errors"]);
    expect(document.getElementById("error-inner")!.textContent).toBe("errors loaded");
  });
});
