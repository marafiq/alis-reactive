import { describe, it, expect, afterEach } from "vitest";
import { boot } from "../lifecycle/boot";
import type { Command } from "../types";

function es(path: string) {
  return { kind: "event" as const, path };
}

function setText(target: string, value: string): Command {
  return { kind: "mutate-element", target, mutation: { kind: "set-prop", prop: "textContent" }, value };
}

function show(target: string): Command {
  return { kind: "mutate-element", target, mutation: { kind: "call", method: "removeAttribute", args: [{ kind: "literal", value: "hidden" }] } };
}

function hide(target: string): Command {
  return { kind: "mutate-element", target, mutation: { kind: "call", method: "setAttribute", args: [{ kind: "literal", value: "hidden" }, { kind: "literal", value: "" }] } };
}

function dispatch(event: string, payload?: Record<string, unknown>): Command {
  return { kind: "dispatch", event, payload };
}

describe("when executing commands with conditions", () => {
  afterEach(() => { document.body.innerHTML = ""; });

  function setupDom(...ids: string[]) {
    for (const id of ids) {
      const el = document.createElement("div");
      el.id = id;
      el.textContent = "";
      document.body.appendChild(el);
    }
  }

  // ── Single command + condition (no HTTP) ──

  it("single command before condition — both execute", () => {
    setupDom("status", "express");

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "sequential",
          commands: [dispatch("order-submitted", { total: 150 })],
        },
      }, {
        trigger: { kind: "custom-event", event: "order-submitted" },
        reaction: {
          kind: "conditional",
          commands: [setText("status", "Received")],
          branches: [{
            guard: { kind: "value", source: es("evt.total"), coerceAs: "number", op: "gte", operand: 100 },
            reaction: { kind: "sequential", commands: [show("express")] },
          }],
        },
      }],
    });

    expect(document.getElementById("status")!.textContent).toBe("Received");
    expect(document.getElementById("express")!.hasAttribute("hidden")).toBe(false);
  });

  // ── Multiple commands + condition ──

  it("multiple commands before condition — all execute then branch evaluates", () => {
    setupDom("confirmation", "spinner", "tier");

    // Set initial hidden state
    document.getElementById("confirmation")!.setAttribute("hidden", "");

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "sequential",
          commands: [dispatch("order-submitted", { total: 750 })],
        },
      }, {
        trigger: { kind: "custom-event", event: "order-submitted" },
        reaction: {
          kind: "conditional",
          commands: [
            show("confirmation"),
            setText("spinner", "Done"),
          ],
          branches: [
            {
              guard: { kind: "value", source: es("evt.total"), coerceAs: "number", op: "gte", operand: 1000 },
              reaction: { kind: "sequential", commands: [setText("tier", "Gold")] },
            },
            {
              guard: { kind: "value", source: es("evt.total"), coerceAs: "number", op: "gte", operand: 500 },
              reaction: { kind: "sequential", commands: [setText("tier", "Silver")] },
            },
            {
              guard: null,
              reaction: { kind: "sequential", commands: [setText("tier", "Bronze")] },
            },
          ],
        },
      }],
    });

    // All pre-commands ran
    expect(document.getElementById("confirmation")!.hasAttribute("hidden")).toBe(false);
    expect(document.getElementById("spinner")!.textContent).toBe("Done");
    // Second branch matched (750 >= 500)
    expect(document.getElementById("tier")!.textContent).toBe("Silver");
  });

  // ── Commands execute even when no branch matches ──

  it("commands execute even when no branch matches", () => {
    setupDom("status", "tier");

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "sequential",
          commands: [dispatch("check", { total: 10 })],
        },
      }, {
        trigger: { kind: "custom-event", event: "check" },
        reaction: {
          kind: "conditional",
          commands: [setText("status", "Checked")],
          branches: [{
            guard: { kind: "value", source: es("evt.total"), coerceAs: "number", op: "gte", operand: 1000 },
            reaction: { kind: "sequential", commands: [setText("tier", "Gold")] },
          }],
        },
      }],
    });

    // Command ran
    expect(document.getElementById("status")!.textContent).toBe("Checked");
    // No branch matched — tier stays empty
    expect(document.getElementById("tier")!.textContent).toBe("");
  });

  // ── Multiple actions inside Then branch ──

  it("multiple actions inside then branch", () => {
    setupDom("loading", "admin-panel", "delete-btn");

    document.getElementById("admin-panel")!.setAttribute("hidden", "");
    document.getElementById("delete-btn")!.setAttribute("hidden", "");

    let adminLoaded = false;
    document.addEventListener("admin-loaded", () => { adminLoaded = true; });

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "sequential",
          commands: [dispatch("role-check", { role: "admin" })],
        },
      }, {
        trigger: { kind: "custom-event", event: "role-check" },
        reaction: {
          kind: "conditional",
          commands: [setText("loading", "Done")],
          branches: [{
            guard: { kind: "value", source: es("evt.role"), coerceAs: "string", op: "eq", operand: "admin" },
            reaction: {
              kind: "sequential",
              commands: [
                show("admin-panel"),
                show("delete-btn"),
                dispatch("admin-loaded"),
              ],
            },
          }, {
            guard: null,
            reaction: {
              kind: "sequential",
              commands: [
                hide("admin-panel"),
                hide("delete-btn"),
              ],
            },
          }],
        },
      }],
    });

    expect(document.getElementById("loading")!.textContent).toBe("Done");
    expect(document.getElementById("admin-panel")!.hasAttribute("hidden")).toBe(false);
    expect(document.getElementById("delete-btn")!.hasAttribute("hidden")).toBe(false);
    expect(adminLoaded).toBe(true);
  });

  // ── Condition-only (no pre-commands) still works ──

  it("condition without pre-commands works as before", () => {
    setupDom("badge");
    document.getElementById("badge")!.setAttribute("hidden", "");

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "sequential",
          commands: [dispatch("status-check", { status: "active" })],
        },
      }, {
        trigger: { kind: "custom-event", event: "status-check" },
        reaction: {
          kind: "conditional",
          branches: [{
            guard: { kind: "value", source: es("evt.status"), coerceAs: "string", op: "eq", operand: "active" },
            reaction: { kind: "sequential", commands: [show("badge")] },
          }, {
            guard: null,
            reaction: { kind: "sequential", commands: [hide("badge")] },
          }],
        },
      }],
    });

    expect(document.getElementById("badge")!.hasAttribute("hidden")).toBe(false);
  });

  // ── Dispatch in pre-commands chains into custom-event listener ──

  it("dispatch in pre-commands chains into custom-event listener", () => {
    setupDom("result");

    // Listener for the dispatched event
    let received = false;
    document.addEventListener("audit-log", () => { received = true; });

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "sequential",
          commands: [dispatch("order-submitted", { total: 50 })],
        },
      }, {
        trigger: { kind: "custom-event", event: "order-submitted" },
        reaction: {
          kind: "conditional",
          commands: [dispatch("audit-log")],
          branches: [{
            guard: { kind: "value", source: es("evt.total"), coerceAs: "number", op: "gte", operand: 100 },
            reaction: { kind: "sequential", commands: [setText("result", "Express")] },
          }, {
            guard: null,
            reaction: { kind: "sequential", commands: [setText("result", "Standard")] },
          }],
        },
      }],
    });

    expect(received).toBe(true);
    expect(document.getElementById("result")!.textContent).toBe("Standard");
  });

  // ── Else branch with multiple actions ──

  it("else branch with multiple actions executes all", () => {
    setupDom("panel", "notice", "status");

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "sequential",
          commands: [dispatch("role-check", { role: "viewer" })],
        },
      }, {
        trigger: { kind: "custom-event", event: "role-check" },
        reaction: {
          kind: "conditional",
          commands: [setText("status", "Loaded")],
          branches: [{
            guard: { kind: "value", source: es("evt.role"), coerceAs: "string", op: "eq", operand: "admin" },
            reaction: { kind: "sequential", commands: [setText("panel", "Admin")] },
          }, {
            guard: null,
            reaction: {
              kind: "sequential",
              commands: [
                setText("panel", "Read-only"),
                setText("notice", "Contact admin for access"),
              ],
            },
          }],
        },
      }],
    });

    expect(document.getElementById("status")!.textContent).toBe("Loaded");
    expect(document.getElementById("panel")!.textContent).toBe("Read-only");
    expect(document.getElementById("notice")!.textContent).toBe("Contact admin for access");
  });
});
