import { describe, it, expect, beforeEach } from "vitest";
import { JSDOM } from "jsdom";
import type { Plan } from "../types";

// Boot must be imported AFTER setting up the DOM
let boot: (plan: Plan) => void;

beforeEach(async () => {
  const dom = new JSDOM(`<!DOCTYPE html><html><body>
    <p id="status" class="text-muted">waiting</p>
    <div id="panel">initial</div>
    <div id="loader" hidden>loading...</div>
    <div id="content">visible</div>
  </body></html>`);

  (globalThis as any).document = dom.window.document;
  (globalThis as any).CustomEvent = dom.window.CustomEvent;
  (globalThis as any).Function = dom.window.Function;

  // Re-import to get fresh module with new document
  const mod = await import("../boot");
  boot = mod.boot;
});

describe("mutate-element command", () => {
  it("adds a class to an element", () => {
    boot({
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "sequential",
          commands: [{ kind: "mutate-element", target: "status", jsEmit: "el.classList.add(val)", value: "active" }],
        },
      }],
    });

    expect(document.getElementById("status")!.classList.contains("active")).toBe(true);
  });

  it("removes a class from an element", () => {
    boot({
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "sequential",
          commands: [{ kind: "mutate-element", target: "status", jsEmit: "el.classList.remove(val)", value: "text-muted" }],
        },
      }],
    });

    expect(document.getElementById("status")!.classList.contains("text-muted")).toBe(false);
  });

  it("sets text content", () => {
    boot({
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "sequential",
          commands: [{ kind: "mutate-element", target: "panel", jsEmit: "el.textContent = val", value: "done" }],
        },
      }],
    });

    expect(document.getElementById("panel")!.textContent).toBe("done");
  });

  it("sets inner HTML", () => {
    boot({
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "sequential",
          commands: [{ kind: "mutate-element", target: "panel", jsEmit: "el.innerHTML = val", value: "<strong>ok</strong>" }],
        },
      }],
    });

    expect(document.getElementById("panel")!.innerHTML).toBe("<strong>ok</strong>");
  });

  it("shows a hidden element", () => {
    boot({
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "sequential",
          commands: [{ kind: "mutate-element", target: "loader", jsEmit: "el.removeAttribute('hidden')" }],
        },
      }],
    });

    expect(document.getElementById("loader")!.hasAttribute("hidden")).toBe(false);
  });

  it("hides a visible element", () => {
    boot({
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "sequential",
          commands: [{ kind: "mutate-element", target: "content", jsEmit: "el.setAttribute('hidden','')" }],
        },
      }],
    });

    expect(document.getElementById("content")!.hasAttribute("hidden")).toBe(true);
  });

  it("toggles a class", () => {
    boot({
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "sequential",
          commands: [{ kind: "mutate-element", target: "status", jsEmit: "el.classList.toggle(val)", value: "text-muted" }],
        },
      }],
    });

    // text-muted was present, toggle removes it
    expect(document.getElementById("status")!.classList.contains("text-muted")).toBe(false);
  });

  it("chains mutations with dispatches", () => {
    const events: string[] = [];
    document.addEventListener("step-done", () => events.push("step-done"));

    boot({
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "sequential",
          commands: [
            { kind: "mutate-element", target: "status", jsEmit: "el.classList.add(val)", value: "complete" },
            { kind: "dispatch", event: "step-done" },
            { kind: "mutate-element", target: "panel", jsEmit: "el.textContent = val", value: "next" },
          ],
        },
      }],
    });

    expect(document.getElementById("status")!.classList.contains("complete")).toBe(true);
    expect(events).toEqual(["step-done"]);
    expect(document.getElementById("panel")!.textContent).toBe("next");
  });
});
