import { afterEach, describe, expect, it, vi } from "vitest";
import { boot, loadPartialSlot, resetBootStateForTests, unloadPartialSlot } from "../lifecycle/boot";
import type { ComponentObject, BrowserObjectContract, PlanDocument, ReactionGraph, Shape } from "../types/index";

const signalR = vi.hoisted(() => {
  class FakeHubConnection {
    readonly state = "Connected";
    stopCalls = 0;
    stopPromise: Promise<void> = Promise.resolve();
    private readonly handlers = new Map<string, Set<(...args: unknown[]) => void>>();
    private onClose: ((error?: Error) => void) | undefined;

    start(): Promise<void> {
      return Promise.resolve();
    }

    stop(): Promise<void> {
      this.stopCalls += 1;
      this.stopPromise = Promise.resolve().then(() => {
        this.onClose?.();
      });
      return this.stopPromise;
    }

    on(method: string, handler: (...args: unknown[]) => void): void {
      const handlers = this.handlers.get(method) ?? new Set<(...args: unknown[]) => void>();
      handlers.add(handler);
      this.handlers.set(method, handlers);
    }

    off(method: string, handler: (...args: unknown[]) => void): void {
      this.handlers.get(method)?.delete(handler);
    }

    onclose(handler: (error?: Error) => void): void {
      this.onClose = handler;
    }

    onreconnecting(): void {}

    onreconnected(): void {}

    emit(method: string, payload: Record<string, unknown>): void {
      for (const handler of this.handlers.get(method) ?? []) {
        handler(payload);
      }
    }
  }

  const connections: FakeHubConnection[] = [];

  class FakeHubConnectionBuilder {
    withUrl(): FakeHubConnectionBuilder {
      return this;
    }

    withAutomaticReconnect(): FakeHubConnectionBuilder {
      return this;
    }

    configureLogging(): FakeHubConnectionBuilder {
      return this;
    }

    build(): FakeHubConnection {
      const connection = new FakeHubConnection();
      connections.push(connection);
      return connection;
    }
  }

  return {
    connections,
    FakeHubConnectionBuilder,
  };
});

vi.mock("@microsoft/signalr", () => ({
  HubConnectionBuilder: signalR.FakeHubConnectionBuilder,
  HubConnectionState: {
    Disconnected: "Disconnected",
    Connecting: "Connecting",
    Connected: "Connected",
    Disconnecting: "Disconnecting",
    Reconnecting: "Reconnecting",
  },
  LogLevel: {
    Information: 2,
    Warning: 3,
  },
}));

const stringShape: Shape = { kind: "string" };

afterEach(() => {
  resetBootStateForTests();
  document.body.innerHTML = "";
  FakeEventSource.reset();
  signalR.connections.length = 0;
  vi.restoreAllMocks();
  vi.unstubAllGlobals();
});

function nativeTextType(): BrowserObjectContract {
  return {
    properties: {
      textContent: {
        path: [{ kind: "property", name: "textContent" }],
        shape: stringShape,
        access: "readwrite",
      },
    },
    methods: {},
    events: {},
  };
}

function displayComponent(id: string): ComponentObject {
  return {
    id,
    vendor: "native",
    type: "native.text",
    role: { kind: "object-target" },
    binding: { kind: "none" },
    container: { kind: "none" },
  };
}

function rootPlan(planId: string): PlanDocument {
  return {
    version: 3,
    planId,
    scope: { kind: "root" },
    types: { "native.text": nativeTextType() },
    components: {
      status: displayComponent("status"),
      secondary: displayComponent("secondary"),
    },
    behaviors: [],
  };
}

function partialPlan(planId: string, behaviors: PlanDocument["behaviors"]): PlanDocument {
  return {
    version: 3,
    planId,
    scope: { kind: "partial" },
    types: {},
    components: {},
    behaviors,
  };
}

function setText(component: string, value: string): ReactionGraph {
  return {
    kind: "set",
    on: { kind: "component", component },
    property: "textContent",
    value: { kind: "literal", value, shape: stringShape },
  };
}

describe("realtime trigger lifecycle", () => {
  it("removes server-push handlers owned by an unloaded partial slot", () => {
    vi.stubGlobal("EventSource", FakeEventSource);
    document.body.innerHTML = `<span id="status"></span><span id="secondary"></span>`;
    const planId = "Resident.ServerPushLifecycle";

    boot(rootPlan(planId));
    loadPartialSlot("first-slot", [
      partialPlan(planId, [
        {
          startsWhen: {
            kind: "server-push",
            url: "/events",
            eventFilter: {
              kind: "named",
              event: "residentUpdated",
              payloadType: { kind: "untyped" },
            },
          },
          reaction: setText("status", "first"),
        },
      ]),
    ]);
    loadPartialSlot("second-slot", [
      partialPlan(planId, [
        {
          startsWhen: {
            kind: "server-push",
            url: "/events",
            eventFilter: {
              kind: "named",
              event: "residentUpdated",
              payloadType: { kind: "untyped" },
            },
          },
          reaction: setText("secondary", "second"),
        },
      ]),
    ]);

    const source = FakeEventSource.single("/events");
    source.emit("residentUpdated", {});
    expect(document.getElementById("status")?.textContent).toBe("first");
    expect(document.getElementById("secondary")?.textContent).toBe("second");

    document.getElementById("status")!.textContent = "";
    document.getElementById("secondary")!.textContent = "";
    unloadPartialSlot("first-slot");

    source.emit("residentUpdated", {});

    expect(document.getElementById("status")?.textContent).toBe("");
    expect(document.getElementById("secondary")?.textContent).toBe("second");
    expect(source.closed).toBe(false);

    unloadPartialSlot("second-slot");

    expect(source.closed).toBe(true);
  });

  it("removes SignalR handlers owned by an unloaded partial slot", async () => {
    document.body.innerHTML = `<span id="status"></span><span id="secondary"></span>`;
    const planId = "Resident.SignalRLifecycle";

    boot(rootPlan(planId));
    loadPartialSlot("first-slot", [
      partialPlan(planId, [
        {
          startsWhen: {
            kind: "signalr",
            hubUrl: "/residentHub",
            method: "ResidentUpdated",
            payloadType: { kind: "untyped" },
          },
          reaction: setText("status", "first"),
        },
      ]),
    ]);
    loadPartialSlot("second-slot", [
      partialPlan(planId, [
        {
          startsWhen: {
            kind: "signalr",
            hubUrl: "/residentHub",
            method: "ResidentUpdated",
            payloadType: { kind: "untyped" },
          },
          reaction: setText("secondary", "second"),
        },
      ]),
    ]);

    const connection = signalR.connections[0];
    expect(connection).toBeDefined();
    connection.emit("ResidentUpdated", {});
    expect(document.getElementById("status")?.textContent).toBe("first");
    expect(document.getElementById("secondary")?.textContent).toBe("second");

    document.getElementById("status")!.textContent = "";
    document.getElementById("secondary")!.textContent = "";
    unloadPartialSlot("first-slot");

    connection.emit("ResidentUpdated", {});

    expect(document.getElementById("status")?.textContent).toBe("");
    expect(document.getElementById("secondary")?.textContent).toBe("second");
    expect(connection.stopCalls).toBe(0);

    unloadPartialSlot("second-slot");

    expect(connection.stopCalls).toBe(1);
    await expect(connection.stopPromise).resolves.toBeUndefined();
  });
});

class FakeEventSource extends EventTarget {
  static readonly CLOSED = 2;
  private static readonly created = new Map<string, FakeEventSource[]>();

  readonly url: string;
  readyState = 1;
  closed = false;
  onopen: (() => void) | null = null;
  onerror: (() => void) | null = null;

  constructor(url: string) {
    super();
    this.url = url;
    const sources = FakeEventSource.created.get(url) ?? [];
    sources.push(this);
    FakeEventSource.created.set(url, sources);
  }

  close(): void {
    this.closed = true;
    this.readyState = FakeEventSource.CLOSED;
  }

  emit(eventName: string, payload: unknown): void {
    this.dispatchEvent(new MessageEvent(eventName, {
      data: JSON.stringify(payload),
    }));
  }

  static single(url: string): FakeEventSource {
    const sources = FakeEventSource.created.get(url) ?? [];
    expect(sources).toHaveLength(1);
    return sources[0];
  }

  static reset(): void {
    FakeEventSource.created.clear();
  }
}
