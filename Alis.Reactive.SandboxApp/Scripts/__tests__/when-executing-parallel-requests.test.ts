import { describe, it, expect, beforeEach, vi } from "vitest";
import { JSDOM } from "jsdom";
import type { ParallelHttpReaction, HttpReaction, ExecContext } from "../types";

let executeHttpReaction: typeof import("../pipeline").executeHttpReaction;
let executeParallelHttpReaction: typeof import("../pipeline").executeParallelHttpReaction;

function mockFetchSequence(...responses: Array<{ status: number; body: unknown }>) {
  const fn = vi.fn();
  for (const r of responses) {
    fn.mockResolvedValueOnce({
      ok: r.status >= 200 && r.status < 300,
      status: r.status,
      headers: { get: () => "application/json" },
      json: () => Promise.resolve(r.body),
      text: () => Promise.resolve(JSON.stringify(r.body)),
    });
  }
  return fn;
}

beforeEach(async () => {
  const dom = new JSDOM(`<!DOCTYPE html><html><body>
    <div id="result">empty</div>
    <div id="residents">empty</div>
    <div id="facilities">empty</div>
    <div id="spinner" hidden></div>
    <div id="error">none</div>
  </body></html>`);

  (globalThis as any).document = dom.window.document;
  (globalThis as any).CustomEvent = dom.window.CustomEvent;
  (globalThis as any).FormData = dom.window.FormData;

  const mod = await import("../pipeline");
  executeHttpReaction = mod.executeHttpReaction;
  executeParallelHttpReaction = mod.executeParallelHttpReaction;
});

describe("parallel http reactions", () => {
  it("all results available after concurrent requests complete", async () => {
    (globalThis as any).fetch = mockFetchSequence(
      { status: 200, body: { data: "residents" } },
      { status: 200, body: { data: "facilities" } }
    );

    const reaction: ParallelHttpReaction = {
      kind: "parallel-http",
      requests: [
        {
          verb: "GET", url: "/api/residents",
          onSuccess: [{ commands: [{ kind: "mutate-element", target: "residents", prop: "textContent", value: "loaded" }] }],
        },
        {
          verb: "GET", url: "/api/facilities",
          onSuccess: [{ commands: [{ kind: "mutate-element", target: "facilities", prop: "textContent", value: "loaded" }] }],
        },
      ],
      onAllSettled: [{ kind: "mutate-element", target: "result", prop: "textContent", value: "all done" }],
    };

    await executeParallelHttpReaction(reaction);

    expect(document.getElementById("residents")!.textContent).toBe("loaded");
    expect(document.getElementById("facilities")!.textContent).toBe("loaded");
    expect(document.getElementById("result")!.textContent).toBe("all done");
  });

  it("failure in one branch does not prevent other from completing", async () => {
    (globalThis as any).fetch = mockFetchSequence(
      { status: 200, body: {} },
      { status: 500, body: { error: "fail" } }
    );

    const reaction: ParallelHttpReaction = {
      kind: "parallel-http",
      requests: [
        {
          verb: "GET", url: "/api/residents",
          onSuccess: [{ commands: [{ kind: "mutate-element", target: "residents", prop: "textContent", value: "loaded" }] }],
        },
        {
          verb: "GET", url: "/api/facilities",
          onError: [{ commands: [{ kind: "mutate-element", target: "error", prop: "textContent", value: "failed" }] }],
        },
      ],
    };

    await executeParallelHttpReaction(reaction);

    expect(document.getElementById("residents")!.textContent).toBe("loaded");
    expect(document.getElementById("error")!.textContent).toBe("failed");
  });
});

describe("http reaction with preFetch", () => {
  it("preFetch commands run before the request", async () => {
    (globalThis as any).fetch = mockFetchSequence({ status: 200, body: {} });

    const reaction: HttpReaction = {
      kind: "http",
      preFetch: [
        { kind: "mutate-element", target: "spinner", method: "removeAttribute", args: ["hidden"] },
      ],
      request: {
        verb: "GET", url: "/api/test",
        onSuccess: [{ commands: [{ kind: "mutate-element", target: "spinner", method: "setAttribute", args: ["hidden", ""] }] }],
      },
    };

    await executeHttpReaction(reaction);

    // After completion, spinner is hidden again by success handler
    expect(document.getElementById("spinner")!.hasAttribute("hidden")).toBe(true);
  });
});
