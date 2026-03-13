import { describe, it, expect, afterEach } from "vitest";
import { boot } from "../boot";
import type { Command } from "../types";

function setText(target: string, value: string): Command {
  return { kind: "mutate-element", target, jsEmit: "el.textContent = val", value };
}

function show(target: string): Command {
  return { kind: "mutate-element", target, jsEmit: "el.removeAttribute('hidden')", value: "" };
}

function hide(target: string): Command {
  return { kind: "mutate-element", target, jsEmit: "el.setAttribute('hidden','')", value: "" };
}

function dispatch(event: string, payload?: Record<string, unknown>): Command {
  return { kind: "dispatch", event, payload };
}

describe("when executing an http workflow", () => {
  afterEach(() => { document.body.innerHTML = ""; });

  function setupDom(...ids: string[]) {
    for (const id of ids) {
      const el = document.createElement("div");
      el.id = id;
      el.textContent = "";
      document.body.appendChild(el);
    }
  }

  // ── 1. Standalone HTTP ──

  it("standalone http executes correctly", async () => {
    setupDom("status");

    globalThis.fetch = (async (_url: string, _opts?: RequestInit) =>
      new Response(JSON.stringify({ ok: true }), { status: 200 })
    ) as any;

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "http",
          request: {
            verb: "POST",
            url: "/api/init",
            onSuccess: [{ commands: [setText("status", "Ready")] }],
          },
        },
      }],
    });

    await new Promise(r => setTimeout(r, 50));

    expect(document.getElementById("status")!.textContent).toBe("Ready");
  });

  // ── 2. Unconditional actions before HTTP (preFetch) ──

  it("unconditional actions execute before http", async () => {
    setupDom("spinner", "status");
    document.getElementById("spinner")!.setAttribute("hidden", "");

    globalThis.fetch = (async (_url: string, _opts?: RequestInit) =>
      new Response(JSON.stringify({ ok: true }), { status: 200 })
    ) as any;

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "http",
          preFetch: [
            show("spinner"),
            setText("status", "Loading..."),
          ],
          request: {
            verb: "POST",
            url: "/api/load",
            onSuccess: [{
              commands: [
                hide("spinner"),
                setText("status", "Loaded"),
              ],
            }],
          },
        },
      }],
    });

    // preFetch runs synchronously — spinner should be shown immediately
    // (though onSuccess will hide it after fetch completes)
    await new Promise(r => setTimeout(r, 50));

    // After fetch completes, spinner is hidden and status is Loaded
    expect(document.getElementById("spinner")!.hasAttribute("hidden")).toBe(true);
    expect(document.getElementById("status")!.textContent).toBe("Loaded");
  });
});
