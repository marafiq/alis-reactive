import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import type { Plan } from "../types";
import { TestWidget } from "../components/lab/test-widget";

const methodHandlers = new Map<string, Array<(...args: unknown[]) => void>>();

vi.mock("@microsoft/signalr", () => ({
  HubConnectionBuilder: class {
    withUrl() { return this; }
    withAutomaticReconnect() { return this; }
    configureLogging() { return this; }
    build() {
      return {
        on(method: string, handler: (...args: unknown[]) => void) {
          if (!methodHandlers.has(method)) methodHandlers.set(method, []);
          methodHandlers.get(method)!.push(handler);
        },
        off(method: string, handler: (...args: unknown[]) => void) {
          const existing = methodHandlers.get(method);
          if (!existing) return;
          methodHandlers.set(method, existing.filter(current => current !== handler));
        },
        start() { return Promise.resolve(); },
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

function flushAsync() {
  return new Promise(resolve => setTimeout(resolve, 0));
}

function mountWidget(id: string): TestWidget {
  const el = document.getElementById(id)!;
  const widget = new TestWidget(el);
  (el as any).ej2_instances = [widget];
  return widget;
}

function emitSignalR(method: string, payload: unknown) {
  for (const handler of methodHandlers.get(method) ?? []) {
    handler(payload);
  }
}

let boot: typeof import("../lifecycle/boot").boot;
let resetBootStateForTests: typeof import("../lifecycle/boot").resetBootStateForTests;
let widget: TestWidget;

beforeEach(async () => {
  vi.restoreAllMocks();
  methodHandlers.clear();
  vi.resetModules();

  document.body.innerHTML = `
    <input id="selected-name" />
    <input id="expected-status" value="ok" />
    <div id="signalr-phase">—</div>
    <div id="signalr-order">—</div>
    <div id="response-label">—</div>
    <div id="signalr-widget"></div>
  `;
  widget = mountWidget("signalr-widget");

  const bootModule = await import("../lifecycle/boot");
  boot = bootModule.boot;
  resetBootStateForTests = bootModule.resetBootStateForTests;
});

afterEach(() => {
  resetBootStateForTests();
  document.body.innerHTML = "";
  methodHandlers.clear();
  vi.restoreAllMocks();
});

describe("when proving signalr pipeline semantics", () => {
  it("preserves condition -> request -> condition order and supports non-input component participation", async () => {
    const requestBodies: unknown[] = [];

    globalThis.fetch = vi.fn(async (_url, init) => {
      requestBodies.push(JSON.parse(String(init?.body ?? "{}")));
      return new Response(
        JSON.stringify({
          status: "ok",
          result: {
            items: [{ name: "Loaded Bea" }],
          },
        }),
        {
          status: 200,
          headers: { "Content-Type": "application/json" },
        }
      );
    }) as typeof fetch;

    const plan: Plan = {
      planId: "Test.Model",
      components: {},
      entries: [
        {
          trigger: { kind: "signalr", hubUrl: "/hubs/live", methodName: "ReceiveResidents" },
          reaction: {
            kind: "conditional",
            branches: [
              {
                guard: {
                  kind: "value",
                  source: { kind: "event", path: "evt.items.1.enabled" },
                  coerceAs: "boolean",
                  op: "truthy",
                },
                reaction: {
                  kind: "sequential",
                  commands: [
                    {
                      kind: "mutate-element",
                      target: "selected-name",
                      mutation: {
                        kind: "set-prop",
                        prop: "value",
                        value: {
                          kind: "source",
                          source: { kind: "event", path: "evt.items.1.profile.name" },
                        },
                      },
                    },
                    {
                      kind: "mutate-element",
                      target: "signalr-widget",
                      vendor: "fusion",
                      mutation: {
                        kind: "call",
                        method: "setItems",
                        args: [
                          {
                            kind: "source",
                            source: { kind: "event", path: "evt.items" },
                          },
                        ],
                      },
                    },
                  ],
                },
              },
              {
                guard: null,
                reaction: {
                  kind: "sequential",
                  commands: [
                    {
                      kind: "mutate-element",
                      target: "selected-name",
                      mutation: {
                        kind: "set-prop",
                        prop: "value",
                        value: { kind: "literal", value: "disabled" },
                      },
                    },
                  ],
                },
              },
            ],
          },
        },
        {
          trigger: { kind: "signalr", hubUrl: "/hubs/live", methodName: "ReceiveResidents" },
          reaction: {
            kind: "http",
            preFetch: [
              {
                kind: "mutate-element",
                target: "signalr-phase",
                mutation: {
                  kind: "set-prop",
                  prop: "textContent",
                  value: { kind: "literal", value: "request-started" },
                },
              },
            ],
            request: {
              verb: "POST",
              url: "/api/live/residents",
              gather: [
                {
                  kind: "component",
                  componentId: "selected-name",
                  vendor: "native",
                  name: "selectedName",
                  readExpr: "value",
                },
                {
                  kind: "component",
                  componentId: "signalr-widget",
                  vendor: "fusion",
                  name: "secondItemName",
                  readExpr: "items.1.profile.name",
                },
                {
                  kind: "event",
                  param: "mode",
                  path: "evt.mode",
                },
              ],
              onSuccess: [
                {
                  reaction: {
                    kind: "conditional",
                    branches: [
                      {
                        guard: {
                          kind: "value",
                          source: { kind: "event", path: "responseBody.status" },
                          coerceAs: "string",
                          op: "eq",
                          rightSource: {
                            kind: "component",
                            componentId: "expected-status",
                            vendor: "native",
                            readExpr: "value",
                          },
                        },
                        reaction: {
                          kind: "sequential",
                          commands: [
                            {
                              kind: "mutate-element",
                              target: "response-label",
                              mutation: {
                                kind: "set-prop",
                                prop: "textContent",
                                value: {
                                  kind: "source",
                                  source: { kind: "event", path: "responseBody.result.items.0.name" },
                                },
                              },
                            },
                            {
                              kind: "mutate-element",
                              target: "signalr-widget",
                              vendor: "fusion",
                              mutation: {
                                kind: "call",
                                method: "setProperty",
                                args: [
                                  { kind: "literal", value: "serverStatus" },
                                  {
                                    kind: "source",
                                    source: { kind: "event", path: "responseBody.status" },
                                  },
                                ],
                              },
                            },
                          ],
                        },
                      },
                      {
                        guard: null,
                        reaction: {
                          kind: "sequential",
                          commands: [
                            {
                              kind: "mutate-element",
                              target: "response-label",
                              mutation: {
                                kind: "set-prop",
                                prop: "textContent",
                                value: { kind: "literal", value: "unexpected" },
                              },
                            },
                          ],
                        },
                      },
                    ],
                  },
                },
              ],
            },
          },
        },
        {
          trigger: { kind: "signalr", hubUrl: "/hubs/live", methodName: "ReceiveResidents" },
          reaction: {
            kind: "conditional",
            branches: [
              {
                guard: {
                  kind: "all",
                  guards: [
                    {
                      kind: "value",
                      source: {
                        kind: "component",
                        componentId: "selected-name",
                        vendor: "native",
                        readExpr: "value",
                      },
                      coerceAs: "string",
                      op: "eq",
                      operand: "Bea",
                    },
                    {
                      kind: "value",
                      source: {
                        kind: "component",
                        componentId: "signalr-widget",
                        vendor: "fusion",
                        readExpr: "items.1.profile.name",
                      },
                      coerceAs: "string",
                      op: "eq",
                      operand: "Bea",
                    },
                    {
                      kind: "value",
                      source: {
                        kind: "component",
                        componentId: "signalr-phase",
                        vendor: "native",
                        readExpr: "textContent",
                      },
                      coerceAs: "string",
                      op: "eq",
                      operand: "request-started",
                    },
                  ],
                },
                reaction: {
                  kind: "sequential",
                  commands: [
                    {
                      kind: "mutate-element",
                      target: "signalr-order",
                      mutation: {
                        kind: "set-prop",
                        prop: "textContent",
                        value: { kind: "literal", value: "ordered" },
                      },
                    },
                  ],
                },
              },
              {
                guard: null,
                reaction: {
                  kind: "sequential",
                  commands: [
                    {
                      kind: "mutate-element",
                      target: "signalr-order",
                      mutation: {
                        kind: "set-prop",
                        prop: "textContent",
                        value: { kind: "literal", value: "out-of-order" },
                      },
                    },
                  ],
                },
              },
            ],
          },
        },
      ],
    };

    boot(plan);

    expect(methodHandlers.get("ReceiveResidents")).toHaveLength(1);

    emitSignalR("ReceiveResidents", {
      mode: "live",
      items: [
        { enabled: false, profile: { name: "Ava" } },
        { enabled: true, profile: { name: "Bea" } },
      ],
    });

    await flushAsync();
    await flushAsync();

    expect(requestBodies).toEqual([
      { selectedName: "Bea", secondItemName: "Bea", mode: "live" },
    ]);
    expect((document.getElementById("selected-name") as HTMLInputElement).value).toBe("Bea");
    expect(widget.items[1]).toEqual({ enabled: true, profile: { name: "Bea" } });
    expect(document.getElementById("signalr-phase")!.textContent).toBe("request-started");
    expect(document.getElementById("signalr-order")!.textContent).toBe("ordered");
    expect(document.getElementById("response-label")!.textContent).toBe("Loaded Bea");
    expect(widget.getProperty("serverStatus")).toBe("ok");
  });
});
