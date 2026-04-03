import { beforeEach, describe, expect, it, vi } from "vitest";
import type { PlanAction } from "../types";
import { createPlan, flushAsync, htmlBlockContract } from "./support/v2-fixtures";

const handlers = new Map<string, (...args: unknown[]) => void>();
let startCalled = false;

vi.mock("@microsoft/signalr", () => ({
  HubConnectionBuilder: class {
    withUrl() { return this; }
    withAutomaticReconnect() { return this; }
    configureLogging() { return this; }
    build() {
      return {
        on(method: string, handler: (...args: unknown[]) => void) {
          handlers.set(method, handler);
        },
        start() {
          startCalled = true;
          return Promise.resolve();
        },
        stop() {
          return Promise.resolve();
        },
        onreconnecting() {},
        onreconnected() {},
        onclose() {},
        state: "Disconnected",
      };
    }
  },
  HubConnectionState: { Disconnected: "Disconnected" },
  LogLevel: { Warning: 4, Information: 2 },
}));

describe("when triggering v2 signalr subscriptions", () => {
  let wireSignalR: typeof import("../execution/signalr").wireSignalR;

  beforeEach(async () => {
    document.body.innerHTML = '<div id="status"></div>';
    handlers.clear();
    startCalled = false;
    vi.resetModules();
    wireSignalR = (await import("../execution/signalr")).wireSignalR;
  });

  it("starts the connection and executes the action with the hub payload", async () => {
    const plan = createPlan({
      contracts: {
        "html.block": htmlBlockContract,
      },
      objects: {
        status: { contract: "html.block", elementId: "status" },
      },
    });

    const action: PlanAction = {
      kind: "set",
      target: { object: "status", member: "text" },
      value: { kind: "context", scope: "event", path: [{ prop: "message" }] },
    };

    wireSignalR(
      { kind: "signalr", hubUrl: "/hubs/residents", method: "ReceiveUpdate" },
      action,
      { plan }
    );

    handlers.get("ReceiveUpdate")?.({ message: "hub-live" });
    await flushAsync();

    expect(startCalled).toBe(true);
    expect(document.getElementById("status")?.textContent).toBe("hub-live");
  });

  it("fails fast on non-object signalr payloads", () => {
    const plan = createPlan({
      contracts: {
        "html.block": htmlBlockContract,
      },
      objects: {
        status: { contract: "html.block", elementId: "status" },
      },
    });

    wireSignalR(
      { kind: "signalr", hubUrl: "/hubs/residents", method: "ReceiveUpdate" },
      {
        kind: "set",
        target: { object: "status", member: "text" },
        value: { kind: "literal", value: "ignored" },
      },
      { plan }
    );

    expect(() => handlers.get("ReceiveUpdate")?.("raw")).toThrow("expected single object argument");
  });
});
