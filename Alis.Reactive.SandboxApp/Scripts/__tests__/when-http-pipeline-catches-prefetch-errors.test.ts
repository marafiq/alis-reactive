import { describe, it, expect, beforeEach, vi } from "vitest";
import { JSDOM } from "jsdom";
import type { HttpReaction, ParallelHttpReaction } from "../types";

/**
 * BDD: When pre-fetch commands or validation throw at the pipeline level
 *
 * Sequence diagram (from design spec):
 *
 *   pipeline.ts (executeHttpReaction):
 *   try {
 *     preFetch: executeCommands()     ← can throw
 *     validation: passesValidation()  ← can throw
 *   } catch (err) {
 *     log.error("pre-request error")
 *     await routeHandlers(req.onError, -1, ctx)
 *     return
 *   }
 *   await execRequest(req, ctx)       ← has its own boundary
 *
 *   pipeline.ts (executeParallelHttpReaction):
 *   try {
 *     preFetch: executeCommands()     ← shared across all requests
 *   } catch (err) {
 *     log.error("pre-request error")
 *     return                          ← can't route to individual onError
 *   }
 */

let executeHttpReaction: typeof import("../execution/pipeline").executeHttpReaction;
let executeParallelHttpReaction: typeof import("../execution/pipeline").executeParallelHttpReaction;

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

beforeEach(async () => {
  const dom = new JSDOM(`<!DOCTYPE html><html><body>
    <div id="error">none</div>
    <div id="status">idle</div>
    <div id="result">empty</div>
    <div id="r1">empty</div>
    <div id="r2">empty</div>
  </body></html>`);

  (globalThis as any).document = dom.window.document;
  (globalThis as any).CustomEvent = dom.window.CustomEvent;
  (globalThis as any).FormData = dom.window.FormData;

  const mod = await import("../execution/pipeline");
  executeHttpReaction = mod.executeHttpReaction;
  executeParallelHttpReaction = mod.executeParallelHttpReaction;
});

// ═══════════════════════════════════════════════════════════════
// Single HTTP — preFetch error routes to request's onError
// ═══════════════════════════════════════════════════════════════

describe("when preFetch throws in a single http reaction", () => {

  it("routes to the request's onError with status -1", async () => {
    (globalThis as any).fetch = vi.fn();

    const reaction: HttpReaction = {
      kind: "http",
      preFetch: [
        // Target "nonexistent" does not exist → mutateElement throws
        setText("nonexistent", "loading..."),
      ],
      request: {
        verb: "POST",
        url: "/api/save",
        onSuccess: [{ commands: [setText("result", "saved")] }],
        onError: [{
          statusCode: -1,
          commands: [setText("error", "pre-fetch failed")],
        }],
      },
    };

    await executeHttpReaction(reaction);

    expect(document.getElementById("error")!.textContent).toBe("pre-fetch failed");
    expect(document.getElementById("result")!.textContent).toBe("empty");
    expect(fetch).not.toHaveBeenCalled();
  });

  it("catch-all onError fires when no -1 handler registered", async () => {
    (globalThis as any).fetch = vi.fn();

    const reaction: HttpReaction = {
      kind: "http",
      preFetch: [setText("nonexistent", "loading...")],
      request: {
        verb: "POST",
        url: "/api/save",
        onError: [{
          // catch-all — no statusCode
          commands: [setText("error", "something went wrong")],
        }],
      },
    };

    await executeHttpReaction(reaction);

    expect(document.getElementById("error")!.textContent).toBe("something went wrong");
  });
});

// ═══════════════════════════════════════════════════════════════
// Parallel HTTP — shared preFetch error
// ═══════════════════════════════════════════════════════════════

describe("when preFetch throws in a parallel http reaction", () => {

  it("does not fire any requests", async () => {
    (globalThis as any).fetch = vi.fn();

    const reaction: ParallelHttpReaction = {
      kind: "parallel-http",
      preFetch: [setText("nonexistent", "loading...")],
      requests: [
        { verb: "GET", url: "/api/a", onSuccess: [{ commands: [setText("r1", "a")] }] },
        { verb: "GET", url: "/api/b", onSuccess: [{ commands: [setText("r2", "b")] }] },
      ],
    };

    await executeParallelHttpReaction(reaction);

    expect(fetch).not.toHaveBeenCalled();
    expect(document.getElementById("r1")!.textContent).toBe("empty");
    expect(document.getElementById("r2")!.textContent).toBe("empty");
  });
});

// ═══════════════════════════════════════════════════════════════
// Parallel HTTP — individual request gather failure
// ═══════════════════════════════════════════════════════════════

describe("when one parallel request's gather throws", () => {

  it("other requests complete — failed one routes to its own onError", async () => {
    (globalThis as any).fetch = mockFetch(200, { ok: true });

    const reaction: ParallelHttpReaction = {
      kind: "parallel-http",
      requests: [
        {
          verb: "GET",
          url: "/api/residents",
          onSuccess: [{ commands: [setText("r1", "loaded")] }],
        },
        {
          verb: "POST",
          url: "/api/broken",
          gather: [{ kind: "all" }], // throws — no components
          onError: [{
            statusCode: -1,
            commands: [setText("r2", "gather failed")],
          }],
        },
      ],
    };

    await executeParallelHttpReaction(reaction);

    expect(document.getElementById("r1")!.textContent).toBe("loaded");
    expect(document.getElementById("r2")!.textContent).toBe("gather failed");
  });

  it("onAllSettled still fires after all requests settle", async () => {
    (globalThis as any).fetch = mockFetch(200, {});

    const reaction: ParallelHttpReaction = {
      kind: "parallel-http",
      requests: [
        {
          verb: "GET",
          url: "/api/ok",
          onSuccess: [{ commands: [setText("r1", "ok")] }],
        },
        {
          verb: "POST",
          url: "/api/broken",
          gather: [{ kind: "all" }],
          onError: [{ commands: [setText("r2", "failed")] }],
        },
      ],
      onAllSettled: [setText("status", "done")],
    };

    await executeParallelHttpReaction(reaction);

    expect(document.getElementById("r1")!.textContent).toBe("ok");
    expect(document.getElementById("r2")!.textContent).toBe("failed");
    expect(document.getElementById("status")!.textContent).toBe("done");
  });
});
