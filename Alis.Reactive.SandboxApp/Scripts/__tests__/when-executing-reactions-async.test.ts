import { describe, it, expect, beforeEach, vi } from "vitest";
import { boot, resetBootStateForTests } from "../lifecycle/boot";
import type { Reaction, Command } from "../types";

/**
 * BDD: When reactions execute through the unified async path
 *
 * Design invariant: executeReaction is async, returning Promise<void>.
 * Single code path for ALL reaction kinds:
 *   - sequential: sync DOM mutations, returns resolved promise (no await hit)
 *   - conditional: evaluateGuardAsync for all guards, await branch reaction
 *   - http: await executeHttpReaction
 *   - parallel-http: await executeParallelHttpReaction
 *
 * The ONLY void boundary is trigger.ts — .catch() there, nowhere else.
 */

function setText(target: string, value: string): Command {
  return { kind: "mutate-element", target, mutation: { kind: "set-prop", prop: "textContent" }, value };
}

function dispatch(event: string, payload?: Record<string, unknown>): Command {
  return { kind: "dispatch", event, payload };
}

function mockResponse(status: number, body: unknown) {
  return {
    ok: status >= 200 && status < 300,
    status,
    headers: { get: (h: string) => h.toLowerCase() === "content-type" ? "application/json" : null },
    json: () => Promise.resolve(body),
    text: () => Promise.resolve(JSON.stringify(body)),
  };
}

beforeEach(() => {
  document.body.innerHTML = "";
  resetBootStateForTests();
  vi.restoreAllMocks();
});

function setupDom(...ids: string[]) {
  for (const id of ids) {
    const el = document.createElement("div");
    el.id = id;
    el.textContent = "";
    document.body.appendChild(el);
  }
}

// ═══════════════════════════════════════════════════════════════
// Sequential reaction — sync mutations, no await needed
// ═══════════════════════════════════════════════════════════════

describe("when a sequential reaction executes", () => {

  it("all DOM mutations complete synchronously within event callback", () => {
    setupDom("a", "b");

    boot({
      planId: "test", components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "sequential",
          commands: [setText("a", "first"), setText("b", "second")],
        },
      }],
    });

    // Sync — no await needed, mutations happened in boot
    expect(document.getElementById("a")!.textContent).toBe("first");
    expect(document.getElementById("b")!.textContent).toBe("second");
  });
});

// ═══════════════════════════════════════════════════════════════
// Conditional reaction — guard evaluation + branch selection
// ═══════════════════════════════════════════════════════════════

describe("when a conditional reaction evaluates sync guards", () => {

  it("takes the matching branch and executes its reaction", async () => {
    setupDom("result");

    boot({
      planId: "test", components: {},
      entries: [{
        trigger: { kind: "custom-event", event: "check-status" },
        reaction: {
          kind: "conditional",
          branches: [
            {
              guard: {
                kind: "value",
                source: { kind: "event", path: "evt.level" },
                coerceAs: "string",
                op: "eq",
                operand: "critical",
              },
              reaction: { kind: "sequential", commands: [setText("result", "CRITICAL")] },
            },
            {
              guard: null, // else branch
              reaction: { kind: "sequential", commands: [setText("result", "normal")] },
            },
          ],
        },
      }],
    });

    document.dispatchEvent(new CustomEvent("check-status", { detail: { level: "critical" } }));
    await new Promise(r => setTimeout(r, 10));

    expect(document.getElementById("result")!.textContent).toBe("CRITICAL");
  });

  it("falls to else branch when no guard matches", async () => {
    setupDom("result");

    boot({
      planId: "test", components: {},
      entries: [{
        trigger: { kind: "custom-event", event: "check-status" },
        reaction: {
          kind: "conditional",
          branches: [
            {
              guard: {
                kind: "value",
                source: { kind: "event", path: "evt.level" },
                coerceAs: "string",
                op: "eq",
                operand: "critical",
              },
              reaction: { kind: "sequential", commands: [setText("result", "CRITICAL")] },
            },
            {
              guard: null,
              reaction: { kind: "sequential", commands: [setText("result", "normal")] },
            },
          ],
        },
      }],
    });

    document.dispatchEvent(new CustomEvent("check-status", { detail: { level: "info" } }));
    await new Promise(r => setTimeout(r, 10));

    expect(document.getElementById("result")!.textContent).toBe("normal");
  });
});

// ═══════════════════════════════════════════════════════════════
// Conditional → HTTP — guard gates an HTTP reaction
// ═══════════════════════════════════════════════════════════════

describe("when a conditional reaction gates an http request", () => {

  it("HTTP fires when guard passes", async () => {
    setupDom("result");
    (globalThis as any).fetch = vi.fn().mockResolvedValue(mockResponse(200, {}));

    boot({
      planId: "test", components: {},
      entries: [{
        trigger: { kind: "custom-event", event: "maybe-save" },
        reaction: {
          kind: "conditional",
          branches: [
            {
              guard: {
                kind: "value",
                source: { kind: "event", path: "evt.confirmed" },
                coerceAs: "boolean",
                op: "truthy",
              },
              reaction: {
                kind: "http",
                request: {
                  verb: "POST",
                  url: "/api/save",
                  onSuccess: [{ commands: [setText("result", "saved")] }],
                },
              },
            },
          ],
        },
      }],
    });

    document.dispatchEvent(new CustomEvent("maybe-save", { detail: { confirmed: true } }));
    await new Promise(r => setTimeout(r, 50));

    expect(document.getElementById("result")!.textContent).toBe("saved");
    expect(fetch).toHaveBeenCalledWith("/api/save", expect.anything());
  });

  it("HTTP does NOT fire when guard fails", async () => {
    setupDom("result");
    (globalThis as any).fetch = vi.fn();

    boot({
      planId: "test", components: {},
      entries: [{
        trigger: { kind: "custom-event", event: "maybe-save" },
        reaction: {
          kind: "conditional",
          branches: [
            {
              guard: {
                kind: "value",
                source: { kind: "event", path: "evt.confirmed" },
                coerceAs: "boolean",
                op: "truthy",
              },
              reaction: {
                kind: "http",
                request: {
                  verb: "POST",
                  url: "/api/save",
                  onSuccess: [{ commands: [setText("result", "saved")] }],
                },
              },
            },
          ],
        },
      }],
    });

    document.dispatchEvent(new CustomEvent("maybe-save", { detail: { confirmed: false } }));
    await new Promise(r => setTimeout(r, 50));

    expect(document.getElementById("result")!.textContent).toBe("");
    expect(fetch).not.toHaveBeenCalled();
  });
});

// ═══════════════════════════════════════════════════════════════
// ConfirmGuard → HTTP — async dialog before HTTP
// ═══════════════════════════════════════════════════════════════

describe("when a confirm guard gates an http reaction", () => {

  it("HTTP fires after user confirms", async () => {
    setupDom("result");
    (globalThis as any).fetch = vi.fn().mockResolvedValue(mockResponse(200, {}));
    (globalThis as any).window = globalThis.window ?? {};
    (globalThis as any).window.alis = {
      confirm: vi.fn().mockResolvedValue(true),
    };

    boot({
      planId: "test", components: {},
      entries: [{
        trigger: { kind: "custom-event", event: "delete" },
        reaction: {
          kind: "conditional",
          branches: [{
            guard: { kind: "confirm", message: "Delete this resident?" },
            reaction: {
              kind: "http",
              request: {
                verb: "DELETE",
                url: "/api/residents/42",
                onSuccess: [{ commands: [setText("result", "deleted")] }],
              },
            },
          }],
        },
      }],
    });

    document.dispatchEvent(new CustomEvent("delete"));
    await new Promise(r => setTimeout(r, 50));

    expect((globalThis as any).window.alis.confirm).toHaveBeenCalledWith("Delete this resident?");
    expect(fetch).toHaveBeenCalledWith("/api/residents/42", expect.objectContaining({ method: "DELETE" }));
    expect(document.getElementById("result")!.textContent).toBe("deleted");
  });

  it("HTTP does NOT fire after user cancels", async () => {
    setupDom("result");
    (globalThis as any).fetch = vi.fn();
    (globalThis as any).window = globalThis.window ?? {};
    (globalThis as any).window.alis = {
      confirm: vi.fn().mockResolvedValue(false),
    };

    boot({
      planId: "test", components: {},
      entries: [{
        trigger: { kind: "custom-event", event: "delete" },
        reaction: {
          kind: "conditional",
          branches: [{
            guard: { kind: "confirm", message: "Delete this resident?" },
            reaction: {
              kind: "http",
              request: {
                verb: "DELETE",
                url: "/api/residents/42",
                onSuccess: [{ commands: [setText("result", "deleted")] }],
              },
            },
          }],
        },
      }],
    });

    document.dispatchEvent(new CustomEvent("delete"));
    await new Promise(r => setTimeout(r, 50));

    expect((globalThis as any).window.alis.confirm).toHaveBeenCalledWith("Delete this resident?");
    expect(fetch).not.toHaveBeenCalled();
    expect(document.getElementById("result")!.textContent).toBe("");
  });
});

// ═══════════════════════════════════════════════════════════════
// Event chain — dispatch in reaction triggers another listener
// ═══════════════════════════════════════════════════════════════

describe("when a reaction dispatches an event that another listener handles", () => {

  it("dispatch is synchronous — listener fires inline", async () => {
    setupDom("step1", "step2");

    boot({
      planId: "test", components: {},
      entries: [
        // Entry 1: on dom-ready → set step1 + dispatch "phase2"
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [
              setText("step1", "done"),
              dispatch("phase2"),
            ],
          },
        },
        // Entry 2: on "phase2" → set step2
        {
          trigger: { kind: "custom-event", event: "phase2" },
          reaction: {
            kind: "sequential",
            commands: [setText("step2", "done")],
          },
        },
      ],
    });

    // Two-phase boot: custom-event listener wired first, then dom-ready fires.
    // dispatch("phase2") fires synchronously within dom-ready reaction.
    expect(document.getElementById("step1")!.textContent).toBe("done");
    expect(document.getElementById("step2")!.textContent).toBe("done");
  });
});
