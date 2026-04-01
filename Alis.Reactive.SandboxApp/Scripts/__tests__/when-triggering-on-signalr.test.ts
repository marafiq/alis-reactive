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
const handlers = new Map<string, Array<(...args: unknown[]) => void>>();
let startCalled = false;
const connections: Array<{ stop: ReturnType<typeof vi.fn>; state: string }> = [];

function handlerCount(method: string): number {
  return (handlers.get(method) ?? []).length;
}

function emit(method: string, ...args: unknown[]) {
  for (const handler of handlers.get(method) ?? []) {
    handler(...args);
  }
}

// Must declare vi.mock BEFORE any imports of the target module.
// vi.mock is hoisted by vitest.
vi.mock("@microsoft/signalr", () => ({
  HubConnectionBuilder: class {
    withUrl() { return this; }
    withAutomaticReconnect() { return this; }
    configureLogging() { return this; }
    build() {
      const connection = {
        on(method: string, handler: (...args: unknown[]) => void) {
          if (!handlers.has(method)) handlers.set(method, []);
          handlers.get(method)!.push(handler);
        },
        off(method: string, handler: (...args: unknown[]) => void) {
          const existing = handlers.get(method);
          if (!existing) return;
          handlers.set(method, existing.filter(current => current !== handler));
        },
        start() {
          startCalled = true;
          connection.state = "Connected";
          return Promise.resolve();
        },
        stop: vi.fn(() => {
          connection.state = "Disconnected";
          return Promise.resolve();
        }),
        onreconnecting() {},
        onreconnected() {},
        onclose() {},
        state: "Disconnected",
      };
      connections.push(connection);
      return connection;
    }
  },
  HubConnectionState: { Connected: "Connected", Disconnected: "Disconnected" },
  LogLevel: { Warning: 4, Information: 2 },
}));

vi.mock("../execution/execute", () => {
  const executeReaction = vi.fn(() => Promise.resolve());
  return {
    executeReaction,
    executeReactionSequence: vi.fn((reactions: Reaction[], ctx: unknown) =>
      Promise.all(reactions.map(reaction => executeReaction(reaction, ctx)))),
  };
});

const seq = (event: string): Reaction => ({
  kind: "sequential",
  commands: [{ kind: "dispatch", event }],
});

let wireSignalR: typeof import("../execution/signalr").wireSignalR;
let executeReaction: typeof import("../execution/execute").executeReaction;

beforeEach(async () => {
  handlers.clear();
  connections.length = 0;
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

    emit("Receive", { count: 5, message: "test" });

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
    expect(() => emit("Receive", "raw string")).toThrow("expected single object argument");
  });

  it("throws on null payload", () => {
    wireSignalR(
      { kind: "signalr", hubUrl: "/hubs/d", methodName: "Receive" },
      seq("out")
    );
    expect(() => emit("Receive", null)).toThrow("expected single object argument");
  });

  it("throws on multiple arguments", () => {
    wireSignalR(
      { kind: "signalr", hubUrl: "/hubs/e", methodName: "Receive" },
      seq("out")
    );
    expect(() => emit("Receive", "a", "b")).toThrow("got 2 args");
  });

  it("starts the connection", () => {
    wireSignalR(
      { kind: "signalr", hubUrl: "/hubs/f", methodName: "Msg" },
      seq("out")
    );
    expect(startCalled).toBe(true);
  });

  it("aborting one shared subscriber does not tear down the surviving subscriber", () => {
    const first = new AbortController();
    const second = new AbortController();
    const reactionA = seq("a");
    const reactionB = seq("b");

    wireSignalR(
      { kind: "signalr", hubUrl: "/hubs/shared", methodName: "Receive" },
      reactionA,
      undefined,
      first.signal
    );
    wireSignalR(
      { kind: "signalr", hubUrl: "/hubs/shared", methodName: "Receive" },
      reactionB,
      undefined,
      second.signal
    );

    expect(connections).toHaveLength(1);
    const shared = connections[0];

    first.abort();
    expect(shared.stop).not.toHaveBeenCalled();
    expect(handlerCount("Receive")).toBe(1);

    emit("Receive", { ok: true });

    expect(executeReaction).toHaveBeenCalledTimes(1);
    expect(executeReaction).toHaveBeenCalledWith(
      reactionB,
      expect.objectContaining({ evt: { ok: true } })
    );

    second.abort();
    expect(shared.stop).toHaveBeenCalledTimes(1);
    expect(handlerCount("Receive")).toBe(0);

    wireSignalR(
      { kind: "signalr", hubUrl: "/hubs/shared", methodName: "Receive" },
      seq("fresh")
    );
    expect(connections).toHaveLength(2);
  });
});
