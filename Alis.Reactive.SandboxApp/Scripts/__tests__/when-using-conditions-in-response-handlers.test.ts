import { describe, it, expect, afterEach } from "vitest";
import { boot } from "../boot";
import type { Plan, Command, Guard, Reaction, StatusHandler } from "../types";

function es(path: string) {
  return { kind: "event" as const, path };
}

function setText(target: string, value: string): Command {
  return { kind: "mutate-element", target, prop: "textContent", value };
}

function show(target: string): Command {
  return { kind: "mutate-element", target, method: "removeAttribute", args: ["hidden"], value: "" };
}

function hide(target: string): Command {
  return { kind: "mutate-element", target, method: "setAttribute", args: ["hidden", ""], value: "" };
}

function dispatch(event: string, payload?: Record<string, unknown>): Command {
  return { kind: "dispatch", event, payload };
}

function valGuard(path: string, coerceAs: "string" | "number" | "boolean", op: string, operand?: unknown): Guard {
  return { kind: "value", source: es(path), coerceAs, op: op as any, operand };
}

describe("when using conditions in response handlers", () => {
  afterEach(() => { document.body.innerHTML = ""; });

  function setupDom(...ids: string[]) {
    for (const id of ids) {
      const el = document.createElement("div");
      el.id = id;
      el.textContent = "";
      document.body.appendChild(el);
    }
  }

  function mockFetch(body: unknown = { ok: true }, status = 200) {
    globalThis.fetch = (async () =>
      new Response(JSON.stringify(body), {
        status,
        headers: { "Content-Type": "application/json" },
      })
    ) as any;
  }

  // ── Conditions in OnSuccess ──

  it("if-else inside OnSuccess executes the matching branch", async () => {
    setupDom("status", "admin-notice");
    document.getElementById("admin-notice")!.setAttribute("hidden", "");
    mockFetch();

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "sequential",
          commands: [dispatch("save", { role: "admin" })],
        },
      }, {
        trigger: { kind: "custom-event", event: "save" },
        reaction: {
          kind: "http",
          request: {
            verb: "POST",
            url: "/api/save",
            onSuccess: [{
              reaction: {
                kind: "conditional",
                commands: [setText("status", "Saved")],
                branches: [
                  {
                    guard: valGuard("evt.role", "string", "eq", "admin"),
                    reaction: { kind: "sequential", commands: [show("admin-notice")] },
                  },
                  {
                    guard: null,
                    reaction: { kind: "sequential", commands: [hide("admin-notice")] },
                  },
                ],
              },
            }],
          },
        },
      }],
    });

    await new Promise(r => setTimeout(r, 50));

    expect(document.getElementById("status")!.textContent).toBe("Saved");
    expect(document.getElementById("admin-notice")!.hasAttribute("hidden")).toBe(false);
  });

  it("else branch executes inside OnSuccess when condition is false", async () => {
    setupDom("status", "admin-notice");
    mockFetch();

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "sequential",
          commands: [dispatch("save", { role: "viewer" })],
        },
      }, {
        trigger: { kind: "custom-event", event: "save" },
        reaction: {
          kind: "http",
          request: {
            verb: "POST",
            url: "/api/save",
            onSuccess: [{
              reaction: {
                kind: "conditional",
                commands: [setText("status", "Saved")],
                branches: [
                  {
                    guard: valGuard("evt.role", "string", "eq", "admin"),
                    reaction: { kind: "sequential", commands: [setText("admin-notice", "Admin")] },
                  },
                  {
                    guard: null,
                    reaction: { kind: "sequential", commands: [setText("admin-notice", "User")] },
                  },
                ],
              },
            }],
          },
        },
      }],
    });

    await new Promise(r => setTimeout(r, 50));

    expect(document.getElementById("status")!.textContent).toBe("Saved");
    expect(document.getElementById("admin-notice")!.textContent).toBe("User");
  });

  // ── if/elseif/else in OnSuccess ──

  it("if-elseif-else inside OnSuccess picks the right branch", async () => {
    setupDom("status", "tier");
    mockFetch();

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "sequential",
          commands: [dispatch("process", { total: 750 })],
        },
      }, {
        trigger: { kind: "custom-event", event: "process" },
        reaction: {
          kind: "http",
          request: {
            verb: "POST",
            url: "/api/process",
            onSuccess: [{
              reaction: {
                kind: "conditional",
                commands: [setText("status", "Done")],
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
          },
        },
      }],
    });

    await new Promise(r => setTimeout(r, 50));

    expect(document.getElementById("status")!.textContent).toBe("Done");
    expect(document.getElementById("tier")!.textContent).toBe("Silver");
  });

  // ── Conditions in OnError ──

  it("conditions inside OnError execute correctly", async () => {
    setupDom("error");
    globalThis.fetch = (async () =>
      new Response(JSON.stringify({ message: "fail" }), {
        status: 500,
        headers: { "Content-Type": "application/json" },
      })
    ) as any;

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "sequential",
          commands: [dispatch("save", { role: "admin" })],
        },
      }, {
        trigger: { kind: "custom-event", event: "save" },
        reaction: {
          kind: "http",
          request: {
            verb: "POST",
            url: "/api/save",
            onError: [{
              statusCode: 500,
              reaction: {
                kind: "conditional",
                branches: [
                  {
                    guard: valGuard("evt.role", "string", "eq", "admin"),
                    reaction: { kind: "sequential", commands: [setText("error", "Admin: check server logs")] },
                  },
                  {
                    guard: null,
                    reaction: { kind: "sequential", commands: [setText("error", "Please try again")] },
                  },
                ],
              },
            }],
          },
        },
      }],
    });

    await new Promise(r => setTimeout(r, 50));

    expect(document.getElementById("error")!.textContent).toBe("Admin: check server logs");
  });

  // ── HTTP inside OnSuccess branch ──

  it("http reaction inside OnSuccess branch executes correctly", async () => {
    setupDom("result");
    let fetchCount = 0;
    globalThis.fetch = (async (url: string) => {
      fetchCount++;
      return new Response(JSON.stringify({ ok: true }), {
        status: 200,
        headers: { "Content-Type": "application/json" },
      });
    }) as any;

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "sequential",
          commands: [dispatch("check", { isActive: true })],
        },
      }, {
        trigger: { kind: "custom-event", event: "check" },
        reaction: {
          kind: "http",
          request: {
            verb: "POST",
            url: "/api/check",
            onSuccess: [{
              reaction: {
                kind: "conditional",
                branches: [
                  {
                    guard: valGuard("evt.isActive", "boolean", "truthy"),
                    reaction: {
                      kind: "http",
                      request: {
                        verb: "POST",
                        url: "/api/activate",
                        onSuccess: [{ commands: [setText("result", "Activated")] }],
                      },
                    },
                  },
                  {
                    guard: null,
                    reaction: { kind: "sequential", commands: [setText("result", "Inactive")] },
                  },
                ],
              },
            }],
          },
        },
      }],
    });

    await new Promise(r => setTimeout(r, 100));

    expect(document.getElementById("result")!.textContent).toBe("Activated");
    expect(fetchCount).toBe(2); // /api/check + /api/activate
  });

  // ── Compound AND in OnSuccess ──

  it("compound AND inside OnSuccess evaluates correctly", async () => {
    setupDom("admin-panel");
    document.getElementById("admin-panel")!.setAttribute("hidden", "");
    mockFetch();

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "sequential",
          commands: [dispatch("auth", { isActive: true, isAdmin: true })],
        },
      }, {
        trigger: { kind: "custom-event", event: "auth" },
        reaction: {
          kind: "http",
          request: {
            verb: "POST",
            url: "/api/auth",
            onSuccess: [{
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
          },
        },
      }],
    });

    await new Promise(r => setTimeout(r, 50));

    expect(document.getElementById("admin-panel")!.hasAttribute("hidden")).toBe(false);
  });

  // ── Backward compatibility: plain commands in OnSuccess still work ──

  it("plain commands in OnSuccess still work (backward compatible)", async () => {
    setupDom("status");
    mockFetch();

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
            onSuccess: [{
              commands: [setText("status", "Ready")],
            }],
          },
        },
      }],
    });

    await new Promise(r => setTimeout(r, 50));

    expect(document.getElementById("status")!.textContent).toBe("Ready");
  });
});
