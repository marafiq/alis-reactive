import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import type { Plan } from "../types";
import { TestWidget } from "../components/lab/test-widget";

function flushAsync() {
  return new Promise(resolve => setTimeout(resolve, 0));
}

function mountWidget(id: string): TestWidget {
  const el = document.getElementById(id)!;
  const widget = new TestWidget(el);
  (el as any).ej2_instances = [widget];
  return widget;
}

class MockEventSource {
  static CLOSED = 2;
  static CONNECTING = 0;
  static OPEN = 1;
  static instances: MockEventSource[] = [];

  readonly CLOSED = 2;
  readonly CONNECTING = 0;
  readonly OPEN = 1;

  readonly url: string;
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

  close() {}

  emit(type: string, data: unknown) {
    const json = typeof data === "string" ? data : JSON.stringify(data);
    for (const handler of this.listeners.get(type) ?? []) {
      handler({ data: json } as MessageEvent);
    }
  }

  handlerCount(type: string): number {
    return (this.listeners.get(type) ?? []).length;
  }

  static reset() {
    MockEventSource.instances = [];
  }
}

(globalThis as any).EventSource = MockEventSource;

let boot: typeof import("../lifecycle/boot").boot;
let resetBootStateForTests: typeof import("../lifecycle/boot").resetBootStateForTests;
let widget: TestWidget;

beforeEach(async () => {
  vi.restoreAllMocks();
  MockEventSource.reset();
  document.body.innerHTML = `
    <input id="resident-name" />
    <input id="expected-status" value="loaded" />
    <div id="request-phase">—</div>
    <div id="post-request-order">—</div>
    <div id="response-title">—</div>
    <div id="result-widget"></div>
  `;
  widget = mountWidget("result-widget");

  const bootModule = await import("../lifecycle/boot");
  boot = bootModule.boot;
  resetBootStateForTests = bootModule.resetBootStateForTests;
});

afterEach(() => {
  resetBootStateForTests();
  document.body.innerHTML = "";
  MockEventSource.reset();
  vi.restoreAllMocks();
});

describe("when proving server-push pipeline semantics", () => {
  it("preserves condition -> request -> condition order and reuses shared component/response semantics", async () => {
    const requestBodies: unknown[] = [];

    globalThis.fetch = vi.fn(async (_url, init) => {
      requestBodies.push(JSON.parse(String(init?.body ?? "{}")));
      return new Response(
        JSON.stringify({
          status: "loaded",
          meta: { summary: { title: "Loaded Resident" } },
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
          trigger: { kind: "server-push", url: "/api/stream", eventType: "resident-updated" },
          reaction: {
            kind: "conditional",
            branches: [
              {
                guard: {
                  kind: "value",
                  source: { kind: "event", path: "evt.person.active" },
                  coerceAs: "boolean",
                  op: "truthy",
                },
                reaction: {
                  kind: "sequential",
                  commands: [
                    {
                      kind: "mutate-element",
                      target: "resident-name",
                      mutation: {
                        kind: "set-prop",
                        prop: "value",
                        value: {
                          kind: "source",
                          source: { kind: "event", path: "evt.person.profile.name" },
                        },
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
                      target: "resident-name",
                      mutation: {
                        kind: "set-prop",
                        prop: "value",
                        value: { kind: "literal", value: "inactive" },
                      },
                    },
                  ],
                },
              },
            ],
          },
        },
        {
          trigger: { kind: "server-push", url: "/api/stream", eventType: "resident-updated" },
          reaction: {
            kind: "http",
            preFetch: [
              {
                kind: "mutate-element",
                target: "request-phase",
                mutation: {
                  kind: "set-prop",
                  prop: "textContent",
                  value: { kind: "literal", value: "request-started" },
                },
              },
            ],
            request: {
              verb: "POST",
              url: "/api/residents/save",
              gather: [
                {
                  kind: "component",
                  componentId: "resident-name",
                  vendor: "native",
                  name: "residentName",
                  readExpr: "value",
                },
                {
                  kind: "event",
                  param: "city",
                  path: "evt.person.address.city",
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
                              target: "response-title",
                              mutation: {
                                kind: "set-prop",
                                prop: "textContent",
                                value: {
                                  kind: "source",
                                  source: { kind: "event", path: "responseBody.meta.summary.title" },
                                },
                              },
                            },
                            {
                              kind: "mutate-element",
                              target: "result-widget",
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
                              target: "response-title",
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
          trigger: { kind: "server-push", url: "/api/stream", eventType: "resident-updated" },
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
                        componentId: "resident-name",
                        vendor: "native",
                        readExpr: "value",
                      },
                      coerceAs: "string",
                      op: "eq",
                      operand: "Amina",
                    },
                    {
                      kind: "value",
                      source: {
                        kind: "component",
                        componentId: "request-phase",
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
                      target: "post-request-order",
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
                      target: "post-request-order",
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

    expect(MockEventSource.instances).toHaveLength(1);
    expect(MockEventSource.instances[0].handlerCount("resident-updated")).toBe(1);

    MockEventSource.instances[0].emit("resident-updated", {
      person: {
        active: true,
        profile: { name: "Amina" },
        address: { city: "Albany" },
      },
    });

    await flushAsync();
    await flushAsync();

    expect(requestBodies).toEqual([{ residentName: "Amina", city: "Albany" }]);
    expect((document.getElementById("resident-name") as HTMLInputElement).value).toBe("Amina");
    expect(document.getElementById("request-phase")!.textContent).toBe("request-started");
    expect(document.getElementById("post-request-order")!.textContent).toBe("ordered");
    expect(document.getElementById("response-title")!.textContent).toBe("Loaded Resident");
    expect(widget.getProperty("serverStatus")).toBe("loaded");
  });
});
