import { beforeEach, describe, expect, it, vi } from "vitest";
import { wireServerPush } from "../execution/server-push";
import { createPlan, flushAsync, htmlBlockContract } from "./support/v2-fixtures";

class MockEventSource {
  static readonly CLOSED = 2;
  static instances: MockEventSource[] = [];

  readonly url: string;
  readyState = 1;
  onopen: (() => void) | null = null;
  onerror: (() => void) | null = null;
  private readonly listeners = new Map<string, Array<(event: { data: string }) => void>>();

  constructor(url: string) {
    this.url = url;
    MockEventSource.instances.push(this);
  }

  addEventListener(type: string, handler: (event: { data: string }) => void): void {
    const existing = this.listeners.get(type) ?? [];
    existing.push(handler);
    this.listeners.set(type, existing);
  }

  close = vi.fn();

  emit(type: string, data: unknown): void {
    const payload = typeof data === "string" ? data : JSON.stringify(data);
    for (const handler of this.listeners.get(type) ?? []) {
      handler({ data: payload });
    }
  }

  static reset(): void {
    MockEventSource.instances = [];
  }
}

describe("when triggering v2 live channels", () => {
  beforeEach(() => {
    document.body.innerHTML = '<div id="status"></div>';
    MockEventSource.reset();
    (globalThis as { EventSource?: unknown }).EventSource = MockEventSource as unknown as typeof EventSource;
  });

  it("routes server-push payloads straight into action execution", async () => {
    const plan = createPlan({
      contracts: {
        "html.block": htmlBlockContract,
      },
      objects: {
        status: { contract: "html.block", elementId: "status" },
      },
    });

    const controller = new AbortController();
    wireServerPush(
      { kind: "server-push", url: "/api/stream", eventType: "resident-updated" },
      {
        kind: "set",
        target: { object: "status", member: "text" },
        value: { kind: "context", scope: "event", path: [{ prop: "message" }] },
      },
      { plan },
      controller.signal
    );

    MockEventSource.instances[0].emit("resident-updated", { message: "live" });
    await flushAsync();

    expect(document.getElementById("status")?.textContent).toBe("live");

    controller.abort();
    expect(MockEventSource.instances[0].close).toHaveBeenCalledOnce();
  });
});
