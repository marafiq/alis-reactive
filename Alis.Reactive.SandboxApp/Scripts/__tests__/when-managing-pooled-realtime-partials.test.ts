import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import type { Plan, Entry, ComponentEntry } from "../types";

class MockEventSource {
  static CLOSED = 2;
  static CONNECTING = 0;
  static OPEN = 1;
  static instances: MockEventSource[] = [];

  readonly url: string;
  readonly close = vi.fn();
  readyState = MockEventSource.OPEN;
  onopen: (() => void) | null = null;
  onerror: (() => void) | null = null;
  private readonly listeners = new Map<string, Array<(e: MessageEvent) => void>>();

  constructor(url: string) {
    this.url = url;
    MockEventSource.instances.push(this);
  }

  addEventListener(type: string, handler: (e: MessageEvent) => void) {
    if (!this.listeners.has(type)) this.listeners.set(type, []);
    this.listeners.get(type)!.push(handler);
  }

  removeEventListener(type: string, handler: (e: MessageEvent) => void) {
    const handlers = this.listeners.get(type);
    if (!handlers) return;
    this.listeners.set(type, handlers.filter(current => current !== handler));
  }

  emit(type: string, data: unknown) {
    const json = typeof data === "string" ? data : JSON.stringify(data);
    for (const handler of this.listeners.get(type) ?? []) {
      handler({ data: json } as MessageEvent);
    }
  }

  handlerCount(type: string) {
    return (this.listeners.get(type) ?? []).length;
  }

  static reset() {
    MockEventSource.instances = [];
  }
}

(globalThis as any).EventSource = MockEventSource;

const methodHandlers = new Map<string, Array<(...args: unknown[]) => void>>();
const signalrConnections: Array<{ stop: ReturnType<typeof vi.fn>; state: string }> = [];

vi.mock("@microsoft/signalr", () => ({
  HubConnectionBuilder: class {
    withUrl() { return this; }
    withAutomaticReconnect() { return this; }
    configureLogging() { return this; }
    build() {
      const connection = {
        on(method: string, handler: (...args: unknown[]) => void) {
          if (!methodHandlers.has(method)) methodHandlers.set(method, []);
          methodHandlers.get(method)!.push(handler);
        },
        off(method: string, handler: (...args: unknown[]) => void) {
          const existing = methodHandlers.get(method);
          if (!existing) return;
          methodHandlers.set(method, existing.filter(current => current !== handler));
        },
        start() {
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
      signalrConnections.push(connection);
      return connection;
    }
  },
  HubConnectionState: { Connected: "Connected", Disconnected: "Disconnected" },
  LogLevel: { Warning: 4, Information: 2 },
}));

let boot: typeof import("../lifecycle/boot").boot;
let mergePlan: typeof import("../lifecycle/boot").mergePlan;
let resetBootStateForTests: typeof import("../lifecycle/boot").resetBootStateForTests;

function rootPlan(overrides?: Partial<Plan>): Plan {
  return {
    planId: "Test.Model",
    components: {},
    entries: [],
    ...overrides,
  };
}

function comp(id: string): ComponentEntry {
  return { id, vendor: "native", readExpr: "value", componentType: "textbox", coerceAs: "string" };
}

function partialPlan(sourceId: string, entries: Entry[]): Plan {
  return {
    planId: "Test.Model",
    sourceId,
    components: {},
    entries,
  };
}

function sseEntry(url: string, event: string): Entry {
  return {
    trigger: { kind: "server-push", url },
    reaction: {
      kind: "sequential",
      commands: [{ kind: "dispatch", event }],
    },
  };
}

function signalrEntry(hubUrl: string, methodName: string, event: string): Entry {
  return {
    trigger: { kind: "signalr", hubUrl, methodName },
    reaction: {
      kind: "sequential",
      commands: [{ kind: "dispatch", event }],
    },
  };
}

function sseGatherEntry(url: string): Entry {
  return {
    trigger: { kind: "server-push", url },
    reaction: {
      kind: "http",
      request: {
        verb: "POST",
        url: "/api/sse-save",
        gather: [{ kind: "all" }],
      },
    },
  };
}

function signalrGatherEntry(hubUrl: string, methodName: string): Entry {
  return {
    trigger: { kind: "signalr", hubUrl, methodName },
    reaction: {
      kind: "http",
      request: {
        verb: "POST",
        url: "/api/signalr-save",
        gather: [{ kind: "all" }],
      },
    },
  };
}

function emitSignalR(method: string, payload: unknown) {
  for (const handler of methodHandlers.get(method) ?? []) {
    handler(payload);
  }
}

function flushAsync() {
  return new Promise(resolve => setTimeout(resolve, 0));
}

beforeEach(async () => {
  vi.restoreAllMocks();
  MockEventSource.reset();
  methodHandlers.clear();
  signalrConnections.length = 0;
  document.body.innerHTML = "";
  vi.resetModules();

  const bootModule = await import("../lifecycle/boot");
  boot = bootModule.boot;
  mergePlan = bootModule.mergePlan;
  resetBootStateForTests = bootModule.resetBootStateForTests;
});

afterEach(() => {
  resetBootStateForTests();
  MockEventSource.reset();
  methodHandlers.clear();
  signalrConnections.length = 0;
  document.body.innerHTML = "";
  vi.restoreAllMocks();
});

describe("when managing pooled realtime partials", () => {
  it("removing one SSE partial leaves the sibling partial active on the shared source", async () => {
    let aCount = 0;
    let bCount = 0;
    document.addEventListener("a-fired", () => { aCount++; });
    document.addEventListener("b-fired", () => { bCount++; });

    boot(rootPlan());
    mergePlan(partialPlan("slot-a", [sseEntry("/api/live", "a-fired")]));
    mergePlan(partialPlan("slot-b", [sseEntry("/api/live", "b-fired")]));

    expect(MockEventSource.instances).toHaveLength(1);
    const shared = MockEventSource.instances[0];

    shared.emit("message", { ok: true });
    await flushAsync();

    expect(aCount).toBe(1);
    expect(bCount).toBe(1);

    mergePlan(partialPlan("slot-a", []));
    expect(shared.close).not.toHaveBeenCalled();
    expect(shared.handlerCount("message")).toBe(1);

    shared.emit("message", { ok: true });
    await flushAsync();

    expect(aCount).toBe(1);
    expect(bCount).toBe(2);

    mergePlan(partialPlan("slot-b", []));
    expect(shared.close).toHaveBeenCalledTimes(1);
  });

  it("removing one SignalR partial leaves the sibling partial active on the shared hub", async () => {
    let aCount = 0;
    let bCount = 0;
    document.addEventListener("a-fired", () => { aCount++; });
    document.addEventListener("b-fired", () => { bCount++; });

    boot(rootPlan());
    mergePlan(partialPlan("slot-a", [signalrEntry("/hubs/live", "ReceiveResidents", "a-fired")]));
    mergePlan(partialPlan("slot-b", [signalrEntry("/hubs/live", "ReceiveResidents", "b-fired")]));

    expect(signalrConnections).toHaveLength(1);
    const shared = signalrConnections[0];

    emitSignalR("ReceiveResidents", { ok: true });
    await flushAsync();

    expect(aCount).toBe(1);
    expect(bCount).toBe(1);

    mergePlan(partialPlan("slot-a", []));
    expect(shared.stop).not.toHaveBeenCalled();

    emitSignalR("ReceiveResidents", { ok: true });
    await flushAsync();

    expect(aCount).toBe(1);
    expect(bCount).toBe(2);

    mergePlan(partialPlan("slot-b", []));
    expect(shared.stop).toHaveBeenCalledTimes(1);
  });

  it("a surviving SSE subscription reads the rebuilt component registry after a partial removal", async () => {
    document.body.innerHTML = '<input id="name-a" value="Ada" /><input id="name-b" value="Bea" />';
    const fetchSpy = vi.fn(async () =>
      new Response(JSON.stringify({ ok: true }), {
        status: 200,
        headers: { "Content-Type": "application/json" },
      })
    );
    globalThis.fetch = fetchSpy as typeof fetch;

    boot(rootPlan());
    mergePlan({
      ...partialPlan("slot-a", [sseGatherEntry("/api/live")]),
      components: { Name: comp("name-a") },
    });
    mergePlan({
      ...partialPlan("slot-b", []),
      components: { Name: comp("name-b") },
    });

    const shared = MockEventSource.instances[0];
    shared.emit("message", { ok: true });
    await flushAsync();
    await flushAsync();

    expect(JSON.parse(String(fetchSpy.mock.calls.at(-1)?.[1]?.body ?? "{}"))).toEqual({ Name: "Bea" });

    mergePlan(partialPlan("slot-b", []));

    shared.emit("message", { ok: true });
    await flushAsync();
    await flushAsync();

    expect(JSON.parse(String(fetchSpy.mock.calls.at(-1)?.[1]?.body ?? "{}"))).toEqual({ Name: "Ada" });
  });

  it("a surviving SignalR subscription reads the rebuilt component registry after a partial removal", async () => {
    document.body.innerHTML = '<input id="name-a" value="Ada" /><input id="name-b" value="Bea" />';
    const fetchSpy = vi.fn(async () =>
      new Response(JSON.stringify({ ok: true }), {
        status: 200,
        headers: { "Content-Type": "application/json" },
      })
    );
    globalThis.fetch = fetchSpy as typeof fetch;

    boot(rootPlan());
    mergePlan({
      ...partialPlan("slot-a", [signalrGatherEntry("/hubs/live", "ReceiveResidents")]),
      components: { Name: comp("name-a") },
    });
    mergePlan({
      ...partialPlan("slot-b", []),
      components: { Name: comp("name-b") },
    });

    emitSignalR("ReceiveResidents", { ok: true });
    await flushAsync();
    await flushAsync();

    expect(JSON.parse(String(fetchSpy.mock.calls.at(-1)?.[1]?.body ?? "{}"))).toEqual({ Name: "Bea" });

    mergePlan(partialPlan("slot-b", []));

    emitSignalR("ReceiveResidents", { ok: true });
    await flushAsync();
    await flushAsync();

    expect(JSON.parse(String(fetchSpy.mock.calls.at(-1)?.[1]?.body ?? "{}"))).toEqual({ Name: "Ada" });
  });
});
