/**
 * SSE trigger tests — mocks EventSource (not available in jsdom).
 * Mock exception justified: EventSource is a browser platform API that jsdom
 * does not implement. The mock is minimal — just enough to verify handler wiring.
 */
import { describe, it, expect, vi, beforeEach } from "vitest";

// Mock executeReaction to capture calls without side effects
vi.mock("../execution/execute", () => ({
  executeReaction: vi.fn(() => Promise.resolve()),
}));

// Minimal EventSource mock
class MockEventSource {
  static CLOSED = 2;
  static CONNECTING = 0;
  static OPEN = 1;
  readonly CLOSED = 2;
  readonly CONNECTING = 0;
  readonly OPEN = 1;

  url: string;
  readyState = MockEventSource.OPEN;
  onopen: (() => void) | null = null;
  onerror: (() => void) | null = null;
  private listeners = new Map<string, Array<(e: any) => void>>();

  constructor(url: string) {
    this.url = url;
    MockEventSource.instances.push(this);
  }

  addEventListener(type: string, handler: (e: any) => void) {
    if (!this.listeners.has(type)) this.listeners.set(type, []);
    this.listeners.get(type)!.push(handler);
  }

  close = vi.fn();

  emit(type: string, data: unknown) {
    const json = typeof data === "string" ? data : JSON.stringify(data);
    for (const h of this.listeners.get(type) ?? []) h({ data: json } as MessageEvent);
  }

  handlerCount(type: string): number {
    return (this.listeners.get(type) ?? []).length;
  }

  static instances: MockEventSource[] = [];
  static reset() { MockEventSource.instances = []; }
}

(globalThis as any).EventSource = MockEventSource;

import type { ServerPushTrigger, Reaction } from "../types";

const seq = (event: string): Reaction => ({
  kind: "sequential",
  commands: [{ kind: "dispatch", event }],
});

/** Reaction with a mutate-element command — provides a target for retry indicator anchoring. */
const mutating = (target: string): Reaction => ({
  kind: "sequential",
  commands: [{ kind: "mutate-element", target, mutation: { kind: "set-prop", prop: "textContent" } }],
});

let wireServerPush: typeof import("../execution/server-push").wireServerPush;
let executeReaction: typeof import("../execution/execute").executeReaction;

beforeEach(async () => {
  vi.clearAllMocks();
  MockEventSource.reset();
  // Reset module cache so vi.mock(executeReaction) takes effect
  vi.resetModules();
  const sseModule = await import("../execution/server-push");
  const execModule = await import("../execution/execute");
  wireServerPush = sseModule.wireServerPush;
  executeReaction = execModule.executeReaction;
});

describe("when triggering on server push (SSE)", () => {
  it("creates an EventSource for the trigger URL", () => {
    wireServerPush({ kind: "server-push", url: "/api/test-1" }, seq("out"));

    expect(MockEventSource.instances).toHaveLength(1);
    expect(MockEventSource.instances[0].url).toBe("/api/test-1");
  });

  it("listens on 'message' when no eventType is specified", () => {
    wireServerPush({ kind: "server-push", url: "/api/test-2" }, seq("out"));

    const es = MockEventSource.instances[0];
    expect(es.handlerCount("message")).toBe(1);
  });

  it("listens on named event type when specified", () => {
    wireServerPush(
      { kind: "server-push", url: "/api/test-3", eventType: "notification" },
      seq("out")
    );

    const es = MockEventSource.instances[0];
    expect(es.handlerCount("notification")).toBe(1);
    expect(es.handlerCount("message")).toBe(0);
  });

  it("parses JSON data and passes to executeReaction as evt", () => {
    const reaction = seq("out");
    wireServerPush({ kind: "server-push", url: "/api/test-4" }, reaction);

    const es = MockEventSource.instances[0];
    es.emit("message", { count: 3, message: "hello" });

    expect(executeReaction).toHaveBeenCalledWith(
      reaction,
      expect.objectContaining({ evt: { count: 3, message: "hello" } })
    );
  });

  it("throws on non-JSON SSE data", () => {
    wireServerPush({ kind: "server-push", url: "/api/test-5" }, seq("out"));

    const es = MockEventSource.instances[0];
    expect(() => es.emit("message", "not json {{{")).toThrow(SyntaxError);
  });

  it("multiple triggers on same URL share one EventSource", () => {
    wireServerPush({ kind: "server-push", url: "/api/shared" }, seq("a"));
    wireServerPush(
      { kind: "server-push", url: "/api/shared", eventType: "alert" },
      seq("b")
    );

    // Only one EventSource created (pool deduplicates by URL)
    expect(MockEventSource.instances).toHaveLength(1);
    const es = MockEventSource.instances[0];
    // Both handlers registered on the same EventSource
    expect(es.handlerCount("message")).toBe(1);
    expect(es.handlerCount("alert")).toBe(1);
  });

  it("multiple catch-all triggers on same URL both fire", () => {
    wireServerPush({ kind: "server-push", url: "/api/multi" }, seq("a"));
    wireServerPush({ kind: "server-push", url: "/api/multi" }, seq("b"));

    const es = MockEventSource.instances[0];
    // addEventListener("message") used — both handlers coexist
    expect(es.handlerCount("message")).toBe(2);
  });

  it("closes EventSource on abort signal", () => {
    const controller = new AbortController();
    wireServerPush(
      { kind: "server-push", url: "/api/test-abort" },
      seq("out"),
      undefined,
      controller.signal
    );

    const es = MockEventSource.instances[0];
    controller.abort();
    expect(es.close).toHaveBeenCalled();
  });
});

describe("when SSE connection closes permanently", () => {
  it("shows retry indicator on the mutation target's parent", () => {
    document.body.innerHTML = `<div id="parent"><span id="status">ok</span></div>`;
    wireServerPush(
      { kind: "server-push", url: "/api/retry-test" },
      mutating("status")
    );

    const es = MockEventSource.instances[0];
    // Simulate permanent close
    (es as any).readyState = MockEventSource.CLOSED;
    es.onerror!();

    const indicator = document.querySelector("[data-alis-retry]");
    expect(indicator).toBeTruthy();
    expect(indicator?.getAttribute("data-alis-retry")).toBe("/api/retry-test");
  });

  it("re-creates EventSource and re-wires handlers on retry click", () => {
    document.body.innerHTML = `<div><span id="target">text</span></div>`;
    wireServerPush(
      { kind: "server-push", url: "/api/retry-rewire" },
      mutating("target")
    );

    expect(MockEventSource.instances).toHaveLength(1);
    const es = MockEventSource.instances[0];

    // Simulate permanent close
    (es as any).readyState = MockEventSource.CLOSED;
    es.onerror!();

    // Click retry
    const btn = document.querySelector("[data-alis-retry]") as HTMLElement;
    btn.click();

    // A new EventSource should have been created for the same URL
    expect(MockEventSource.instances).toHaveLength(2);
    expect(MockEventSource.instances[1].url).toBe("/api/retry-rewire");

    // Retry indicator should be removed
    expect(document.querySelector("[data-alis-retry]")).toBeNull();
  });

  it("does not show retry indicator when there are no mutation targets", () => {
    wireServerPush(
      { kind: "server-push", url: "/api/no-target" },
      seq("out")
    );

    const es = MockEventSource.instances[0];
    (es as any).readyState = MockEventSource.CLOSED;
    es.onerror!();

    expect(document.querySelector("[data-alis-retry]")).toBeNull();
  });
});
