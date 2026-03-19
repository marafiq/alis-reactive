import { describe, it, expect, afterEach } from "vitest";
import { boot } from "../lifecycle/boot";
import type { Command, Guard } from "../types";

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

function valGuard(path: string, coerceAs: "string" | "number" | "boolean", op: string, operand?: unknown): Guard {
  return { kind: "value", source: es(path), coerceAs, op: op as any, operand };
}

describe("when executing a conditional workflow", () => {
  afterEach(() => { document.body.innerHTML = ""; });

  function setupDom(...ids: string[]) {
    for (const id of ids) {
      const el = document.createElement("div");
      el.id = id;
      el.textContent = "";
      document.body.appendChild(el);
    }
  }

  // ── 1. If / Else ──

  it("if-else branches execute correctly", async () => {
    setupDom("panel");
    document.getElementById("panel")!.setAttribute("hidden", "");

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: { kind: "sequential", commands: [dispatch("role-check", { role: "admin" })] },
      }, {
        trigger: { kind: "custom-event", event: "role-check" },
        reaction: {
          kind: "conditional",
          branches: [
            {
              guard: valGuard("evt.role", "string", "eq", "admin"),
              reaction: { kind: "sequential", commands: [show("panel")] },
            },
            {
              guard: null,
              reaction: { kind: "sequential", commands: [hide("panel")] },
            },
          ],
        },
      }],
    });

    await new Promise(r => setTimeout(r, 0));
    expect(document.getElementById("panel")!.hasAttribute("hidden")).toBe(false);
  });

  // ── 2. If / ElseIf / Else ──

  it("if-elseif-else branches execute correctly", async () => {
    setupDom("tier");

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: { kind: "sequential", commands: [dispatch("tier-check", { total: 750 })] },
      }, {
        trigger: { kind: "custom-event", event: "tier-check" },
        reaction: {
          kind: "conditional",
          branches: [
            {
              guard: valGuard("evt.total", "number", "gte", 1000),
              reaction: { kind: "sequential", commands: [setText("tier", "Gold")] },
            },
            {
              guard: valGuard("evt.total", "number", "gte", 500),
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

    await new Promise(r => setTimeout(r, 0));
    expect(document.getElementById("tier")!.textContent).toBe("Silver");
  });

  // ── 3. Multiple actions inside Then ──

  it("multiple actions execute inside then", async () => {
    setupDom("admin-panel", "delete-btn");
    document.getElementById("admin-panel")!.setAttribute("hidden", "");
    document.getElementById("delete-btn")!.setAttribute("hidden", "");

    let adminLoaded = false;
    document.addEventListener("admin-loaded", () => { adminLoaded = true; });

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: { kind: "sequential", commands: [dispatch("role-check", { role: "admin" })] },
      }, {
        trigger: { kind: "custom-event", event: "role-check" },
        reaction: {
          kind: "conditional",
          branches: [
            {
              guard: valGuard("evt.role", "string", "eq", "admin"),
              reaction: {
                kind: "sequential",
                commands: [show("admin-panel"), show("delete-btn"), dispatch("admin-loaded")],
              },
            },
            {
              guard: null,
              reaction: { kind: "sequential", commands: [hide("admin-panel")] },
            },
          ],
        },
      }],
    });

    await new Promise(r => setTimeout(r, 0));
    expect(document.getElementById("admin-panel")!.hasAttribute("hidden")).toBe(false);
    expect(document.getElementById("delete-btn")!.hasAttribute("hidden")).toBe(false);
    expect(adminLoaded).toBe(true);
  });

  // ── 4. Multiple actions inside Else ──

  it("multiple actions execute inside else", async () => {
    setupDom("panel", "notice");

    let accessDenied = false;
    document.addEventListener("access-denied", () => { accessDenied = true; });

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: { kind: "sequential", commands: [dispatch("role-check", { role: "viewer" })] },
      }, {
        trigger: { kind: "custom-event", event: "role-check" },
        reaction: {
          kind: "conditional",
          branches: [
            {
              guard: valGuard("evt.role", "string", "eq", "admin"),
              reaction: { kind: "sequential", commands: [show("panel")] },
            },
            {
              guard: null,
              reaction: {
                kind: "sequential",
                commands: [
                  hide("panel"),
                  setText("notice", "Contact admin for access"),
                  dispatch("access-denied"),
                ],
              },
            },
          ],
        },
      }],
    });

    await new Promise(r => setTimeout(r, 0));
    expect(document.getElementById("notice")!.textContent).toBe("Contact admin for access");
    expect(accessDenied).toBe(true);
  });

  // ── 5. Compound conditions (AND) ──

  it("compound conditions execute correctly", async () => {
    setupDom("admin-panel");
    document.getElementById("admin-panel")!.setAttribute("hidden", "");

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "sequential",
          commands: [dispatch("compound-check", { isActive: true, isAdmin: true })],
        },
      }, {
        trigger: { kind: "custom-event", event: "compound-check" },
        reaction: {
          kind: "conditional",
          branches: [
            {
              guard: {
                kind: "all",
                guards: [
                  valGuard("evt.isActive", "boolean", "truthy"),
                  valGuard("evt.isAdmin", "boolean", "truthy"),
                ],
              },
              reaction: { kind: "sequential", commands: [show("admin-panel")] },
            },
            {
              guard: null,
              reaction: { kind: "sequential", commands: [hide("admin-panel")] },
            },
          ],
        },
      }],
    });

    await new Promise(r => setTimeout(r, 0));
    expect(document.getElementById("admin-panel")!.hasAttribute("hidden")).toBe(false);
  });

  it("compound AND fails when one condition is false", () => {
    setupDom("admin-panel");
    document.getElementById("admin-panel")!.setAttribute("hidden", "");

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "sequential",
          commands: [dispatch("compound-check", { isActive: true, isAdmin: false })],
        },
      }, {
        trigger: { kind: "custom-event", event: "compound-check" },
        reaction: {
          kind: "conditional",
          branches: [
            {
              guard: {
                kind: "all",
                guards: [
                  valGuard("evt.isActive", "boolean", "truthy"),
                  valGuard("evt.isAdmin", "boolean", "truthy"),
                ],
              },
              reaction: { kind: "sequential", commands: [show("admin-panel")] },
            },
            {
              guard: null,
              reaction: { kind: "sequential", commands: [hide("admin-panel")] },
            },
          ],
        },
      }],
    });

    // AND fails — else branch runs, panel stays hidden
    expect(document.getElementById("admin-panel")!.hasAttribute("hidden")).toBe(true);
  });

  // ── 6. Unconditional actions alongside a condition block ──

  it("unconditional actions execute alongside a condition block", async () => {
    setupDom("status", "admin-panel");
    document.getElementById("admin-panel")!.setAttribute("hidden", "");

    let workflowStarted = false;
    document.addEventListener("workflow-started", () => { workflowStarted = true; });

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: { kind: "sequential", commands: [dispatch("workflow", { role: "admin" })] },
      }, {
        trigger: { kind: "custom-event", event: "workflow" },
        reaction: {
          kind: "conditional",
          commands: [
            setText("status", "Processing..."),
            dispatch("workflow-started"),
          ],
          branches: [
            {
              guard: valGuard("evt.role", "string", "eq", "admin"),
              reaction: { kind: "sequential", commands: [show("admin-panel")] },
            },
            {
              guard: null,
              reaction: { kind: "sequential", commands: [hide("admin-panel")] },
            },
          ],
        },
      }],
    });

    // Pre-commands (unconditional) run sync — assert immediately
    expect(document.getElementById("status")!.textContent).toBe("Processing...");
    expect(workflowStarted).toBe(true);
    // Branch is async — flush microtasks
    await new Promise(r => setTimeout(r, 0));
    expect(document.getElementById("admin-panel")!.hasAttribute("hidden")).toBe(false);
  });

  // ── 7. Then branch with HTTP workflow ──

  it("a then branch can execute an http workflow", async () => {
    setupDom("result");

    const _mockFetch = globalThis.fetch = Object.assign(
      async (_url: string, _opts?: RequestInit) =>
        new Response(JSON.stringify({ ok: true }), { status: 200 }),
      { __isMock: true }
    ) as any;

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: { kind: "sequential", commands: [dispatch("save-check", { role: "admin" })] },
      }, {
        trigger: { kind: "custom-event", event: "save-check" },
        reaction: {
          kind: "conditional",
          branches: [
            {
              guard: valGuard("evt.role", "string", "eq", "admin"),
              reaction: {
                kind: "http",
                request: {
                  verb: "POST",
                  url: "/api/admin/save",
                  onSuccess: [{ commands: [setText("result", "Saved")] }],
                },
              },
            },
            {
              guard: null,
              reaction: { kind: "sequential", commands: [setText("result", "Unauthorized")] },
            },
          ],
        },
      }],
    });

    // Wait for async fetch to complete
    await new Promise(r => setTimeout(r, 50));

    expect(document.getElementById("result")!.textContent).toBe("Saved");
  });

  // ── 8. Else branch with HTTP workflow ──

  it("an else branch can execute an http workflow", async () => {
    setupDom("result");

    globalThis.fetch = (async (_url: string, _opts?: RequestInit) =>
      new Response(JSON.stringify({ ok: true }), { status: 200 })
    ) as any;

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: { kind: "sequential", commands: [dispatch("save-check", { role: "user" })] },
      }, {
        trigger: { kind: "custom-event", event: "save-check" },
        reaction: {
          kind: "conditional",
          branches: [
            {
              guard: valGuard("evt.role", "string", "eq", "admin"),
              reaction: {
                kind: "http",
                request: {
                  verb: "POST",
                  url: "/api/admin/save",
                  onSuccess: [{ commands: [setText("result", "Admin Saved")] }],
                },
              },
            },
            {
              guard: null,
              reaction: {
                kind: "http",
                request: {
                  verb: "POST",
                  url: "/api/user/save",
                  onSuccess: [{ commands: [setText("result", "User Saved")] }],
                },
              },
            },
          ],
        },
      }],
    });

    await new Promise(r => setTimeout(r, 50));

    expect(document.getElementById("result")!.textContent).toBe("User Saved");
  });

  // ── 9. Branch chooses HTTP versus plain UI actions ──

  it("a branch can choose http versus plain ui actions", async () => {
    setupDom("result", "error", "retry-btn");
    document.getElementById("retry-btn")!.setAttribute("hidden", "");

    globalThis.fetch = (async (_url: string, _opts?: RequestInit) =>
      new Response(JSON.stringify({ ok: true }), { status: 200 })
    ) as any;

    // Active = true → HTTP branch
    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: { kind: "sequential", commands: [dispatch("action-check", { isActive: true })] },
      }, {
        trigger: { kind: "custom-event", event: "action-check" },
        reaction: {
          kind: "conditional",
          branches: [
            {
              guard: valGuard("evt.isActive", "boolean", "truthy"),
              reaction: {
                kind: "http",
                request: {
                  verb: "POST",
                  url: "/api/process",
                  onSuccess: [{ commands: [setText("result", "Processed")] }],
                },
              },
            },
            {
              guard: null,
              reaction: {
                kind: "sequential",
                commands: [
                  setText("error", "Account inactive"),
                  show("retry-btn"),
                ],
              },
            },
          ],
        },
      }],
    });

    await new Promise(r => setTimeout(r, 50));

    expect(document.getElementById("result")!.textContent).toBe("Processed");
    // retry-btn stays hidden since Then branch ran
    expect(document.getElementById("retry-btn")!.hasAttribute("hidden")).toBe(true);
  });

  // ── 10. Confirm branches (sync test — mock window.confirm) ──

  it("confirm branches execute multiple actions correctly", async () => {
    setupDom("item", "status");

    // Mock window.alis.confirm (used by async guard evaluation)
    (window as any).alis = { confirm: (_msg: string) => Promise.resolve(true) };

    let itemDeleted = false;
    document.addEventListener("item-deleted", () => { itemDeleted = true; });

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: { kind: "sequential", commands: [dispatch("delete-check", {})] },
      }, {
        trigger: { kind: "custom-event", event: "delete-check" },
        reaction: {
          kind: "conditional",
          branches: [
            {
              guard: { kind: "confirm", message: "Are you sure you want to delete?" },
              reaction: {
                kind: "sequential",
                commands: [
                  hide("item"),
                  setText("status", "Deleted"),
                  dispatch("item-deleted"),
                ],
              },
            },
            {
              guard: null,
              reaction: { kind: "sequential", commands: [setText("status", "Cancelled")] },
            },
          ],
        },
      }],
    });

    // Confirm is async — wait for microtasks
    await new Promise(r => setTimeout(r, 50));

    expect(document.getElementById("item")!.hasAttribute("hidden")).toBe(true);
    expect(document.getElementById("status")!.textContent).toBe("Deleted");
    expect(itemDeleted).toBe(true);

    delete (window as any).alis;
  });

  it("confirm cancelled executes else branch", async () => {
    setupDom("item", "status");

    (window as any).alis = { confirm: (_msg: string) => Promise.resolve(false) };

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: { kind: "sequential", commands: [dispatch("delete-check", {})] },
      }, {
        trigger: { kind: "custom-event", event: "delete-check" },
        reaction: {
          kind: "conditional",
          branches: [
            {
              guard: { kind: "confirm", message: "Are you sure?" },
              reaction: { kind: "sequential", commands: [hide("item"), setText("status", "Deleted")] },
            },
            {
              guard: null,
              reaction: { kind: "sequential", commands: [setText("status", "Cancelled")] },
            },
          ],
        },
      }],
    });

    await new Promise(r => setTimeout(r, 50));

    expect(document.getElementById("item")!.hasAttribute("hidden")).toBe(false);
    expect(document.getElementById("status")!.textContent).toBe("Cancelled");

    delete (window as any).alis;
  });
});
