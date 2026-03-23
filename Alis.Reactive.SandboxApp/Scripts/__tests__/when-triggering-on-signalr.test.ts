/**
 * SignalR trigger tests — mocks @microsoft/signalr HubConnection.
 * Mock exception justified: HubConnection requires a real server endpoint.
 *
 * Uses vi.resetModules() + dynamic import because the vitest setup file
 * transitively loads @microsoft/signalr via boot.ts → trigger.ts → signalr.ts.
 * Without reset, the real module is cached before vi.mock can intercept it.
 */
import { describe, it, expect, vi, beforeEach } from "vitest";
import type { Reaction } from "../types";

// Track registered handlers per method name
const handlers = new Map<string, (...args: unknown[]) => void>();
let startCalled = false;

// Must declare vi.mock BEFORE any imports of the target module.
// vi.mock is hoisted by vitest.
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
        start() { startCalled = true; return Promise.resolve(); },
        stop() { return Promise.resolve(); },
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

vi.mock("../execution/execute", () => ({
  executeReaction: vi.fn(() => Promise.resolve()),
}));

const seq = (event: string): Reaction => ({
  kind: "sequential",
  commands: [{ kind: "dispatch", event }],
});

let wireSignalR: typeof import("../execution/signalr").wireSignalR;
let executeReaction: typeof import("../execution/execute").executeReaction;

beforeEach(async () => {
  handlers.clear();
  startCalled = false;
  vi.clearAllMocks();

  // Reset module cache so vi.mock takes effect (setup file pre-loads the real module)
  vi.resetModules();
  const signalrMod = await import("../execution/signalr");
  const execMod = await import("../execution/execute");
  wireSignalR = signalrMod.wireSignalR;
  executeReaction = execMod.executeReaction;
});

describe("when triggering on signalr", () => {
  it("registers a handler for the specified method name", () => {
    wireSignalR(
      { kind: "signalr", hubUrl: "/hubs/a", methodName: "ReceiveUpdate" },
      seq("out")
    );
    expect(handlers.has("ReceiveUpdate")).toBe(true);
  });

  it("passes deserialized payload directly to executeReaction as evt", () => {
    const reaction = seq("out");
    wireSignalR(
      { kind: "signalr", hubUrl: "/hubs/b", methodName: "Receive" },
      reaction
    );

    const handler = handlers.get("Receive")!;
    handler({ count: 5, message: "test" });

    expect(executeReaction).toHaveBeenCalledWith(
      reaction,
      expect.objectContaining({ evt: { count: 5, message: "test" } })
    );
  });

  it("throws on non-object payload — does not invent arg0/arg1 keys", () => {
    wireSignalR(
      { kind: "signalr", hubUrl: "/hubs/c", methodName: "Receive" },
      seq("out")
    );
    const handler = handlers.get("Receive")!;
    expect(() => handler("raw string")).toThrow("expected single object argument");
  });

  it("throws on null payload", () => {
    wireSignalR(
      { kind: "signalr", hubUrl: "/hubs/d", methodName: "Receive" },
      seq("out")
    );
    const handler = handlers.get("Receive")!;
    expect(() => handler(null)).toThrow("expected single object argument");
  });

  it("throws on multiple arguments", () => {
    wireSignalR(
      { kind: "signalr", hubUrl: "/hubs/e", methodName: "Receive" },
      seq("out")
    );
    const handler = handlers.get("Receive")!;
    expect(() => handler("a", "b")).toThrow("got 2 args");
  });

  it("starts the connection", () => {
    wireSignalR(
      { kind: "signalr", hubUrl: "/hubs/f", methodName: "Msg" },
      seq("out")
    );
    expect(startCalled).toBe(true);
  });
});
