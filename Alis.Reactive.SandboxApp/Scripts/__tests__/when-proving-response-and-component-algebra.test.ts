import { afterEach, describe, expect, it, vi } from "vitest";
import { boot } from "../lifecycle/boot";
import { TestWidget } from "../components/lab/test-widget";

function flushMicrotasks() {
  return new Promise(resolve => setTimeout(resolve, 0));
}

function mountWidget(id: string): TestWidget {
  const el = document.createElement("div");
  el.id = id;
  const widget = new TestWidget(el);
  (el as any).ej2_instances = [widget];
  document.body.appendChild(el);
  return widget;
}

function mockJsonFetch(body: unknown, status = 200) {
  globalThis.fetch = vi.fn(async () =>
    new Response(JSON.stringify(body), {
      status,
      headers: { "Content-Type": "application/json" },
    })
  ) as typeof fetch;
}

describe("when proving response and component algebra", () => {
  afterEach(() => {
    document.body.innerHTML = "";
    vi.restoreAllMocks();
  });

  it("walks nested json response fields inside onSuccess", async () => {
    document.body.innerHTML = `
      <div id="resident-name"></div>
      <div id="resident-city"></div>
      <div id="second-tag"></div>
    `;

    mockJsonFetch({
      resident: {
        name: "Amina",
        address: { city: "Albany" },
      },
      tags: ["draft", "approved"],
    });

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "http",
          request: {
            verb: "GET",
            url: "/api/resident",
            onSuccess: [{
              commands: [
                {
                  kind: "mutate-element",
                  target: "resident-name",
                  mutation: {
                    kind: "set-prop",
                    prop: "textContent",
                    value: { kind: "source", source: { kind: "event", path: "responseBody.resident.name" } },
                  },
                },
                {
                  kind: "mutate-element",
                  target: "resident-city",
                  mutation: {
                    kind: "set-prop",
                    prop: "textContent",
                    value: { kind: "source", source: { kind: "event", path: "responseBody.resident.address.city" } },
                  },
                },
                {
                  kind: "mutate-element",
                  target: "second-tag",
                  mutation: {
                    kind: "set-prop",
                    prop: "textContent",
                    value: { kind: "source", source: { kind: "event", path: "responseBody.tags.1" } },
                  },
                },
              ],
            }],
          },
        },
      }],
    });

    await flushMicrotasks();

    expect(document.getElementById("resident-name")!.textContent).toBe("Amina");
    expect(document.getElementById("resident-city")!.textContent).toBe("Albany");
    expect(document.getElementById("second-tag")!.textContent).toBe("approved");
  });

  it("walks array item object paths inside onSuccess", async () => {
    document.body.innerHTML = `
      <div id="first-city"></div>
      <div id="second-code"></div>
    `;

    mockJsonFetch({
      residents: [
        { address: { city: "Albany" }, meta: { code: "ALB" } },
        { address: { city: "Boston" }, meta: { code: "BOS" } },
      ],
    });

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "http",
          request: {
            verb: "GET",
            url: "/api/residents",
            onSuccess: [{
              commands: [
                {
                  kind: "mutate-element",
                  target: "first-city",
                  mutation: {
                    kind: "set-prop",
                    prop: "textContent",
                    value: {
                      kind: "source",
                      source: { kind: "event", path: "responseBody.residents.0.address.city" },
                    },
                  },
                },
                {
                  kind: "mutate-element",
                  target: "second-code",
                  mutation: {
                    kind: "set-prop",
                    prop: "textContent",
                    value: {
                      kind: "source",
                      source: { kind: "event", path: "responseBody.residents.1.meta.code" },
                    },
                  },
                },
              ],
            }],
          },
        },
      }],
    });

    await flushMicrotasks();

    expect(document.getElementById("first-city")!.textContent).toBe("Albany");
    expect(document.getElementById("second-code")!.textContent).toBe("BOS");
  });

  it("supports source-vs-source conditions inside onSuccess using responseBody and a component value", async () => {
    const expected = document.createElement("input");
    expected.id = "expected-status";
    expected.value = "approved";
    document.body.appendChild(expected);

    const result = document.createElement("div");
    result.id = "status-result";
    document.body.appendChild(result);

    mockJsonFetch({ status: "approved" });

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "http",
          request: {
            verb: "GET",
            url: "/api/status",
            onSuccess: [{
              reaction: {
                kind: "conditional",
                branches: [
                  {
                    guard: {
                      kind: "value",
                      source: { kind: "event", path: "responseBody.status" },
                      coerceAs: "string",
                      op: "eq",
                      rightSource: { kind: "component", componentId: "expected-status", vendor: "native", readExpr: "value" },
                    },
                    reaction: {
                      kind: "sequential",
                      commands: [{
                        kind: "mutate-element",
                        target: "status-result",
                        mutation: {
                          kind: "set-prop",
                          prop: "textContent",
                          value: { kind: "literal", value: "matched" },
                        },
                      }],
                    },
                  },
                  {
                    guard: null,
                    reaction: {
                      kind: "sequential",
                      commands: [{
                        kind: "mutate-element",
                        target: "status-result",
                        mutation: {
                          kind: "set-prop",
                          prop: "textContent",
                          value: { kind: "literal", value: "not-matched" },
                        },
                      }],
                    },
                  },
                ],
              },
            }],
          },
        },
      }],
    });

    await flushMicrotasks();

    expect(result.textContent).toBe("matched");
  });

  it("sets a property on a native component root from a literal", () => {
    const input = document.createElement("input");
    input.id = "native-target";
    document.body.appendChild(input);

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "sequential",
          commands: [{
            kind: "mutate-element",
            target: "native-target",
            mutation: {
              kind: "set-prop",
              prop: "value",
              value: { kind: "literal", value: "from-literal" },
            },
          }],
        },
      }],
    });

    expect(input.value).toBe("from-literal");
  });

  it("sets a property on a fusion component root from a literal", () => {
    const widget = mountWidget("fusion-target");

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "sequential",
          commands: [{
            kind: "mutate-element",
            target: "fusion-target",
            vendor: "fusion",
            mutation: {
              kind: "set-prop",
              prop: "value",
              value: { kind: "literal", value: "from-literal" },
            },
          }],
        },
      }],
    });

    expect(widget.value).toBe("from-literal");
  });

  it("reads a property from a component root and consumes it elsewhere", () => {
    const input = document.createElement("input");
    input.id = "source-input";
    input.value = "from-component-root";
    document.body.appendChild(input);

    const echo = document.createElement("div");
    echo.id = "component-echo";
    document.body.appendChild(echo);

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "sequential",
          commands: [{
            kind: "mutate-element",
            target: "component-echo",
            mutation: {
              kind: "set-prop",
              prop: "textContent",
              value: {
                kind: "source",
                source: { kind: "component", componentId: "source-input", vendor: "native", readExpr: "value" },
              },
            },
          }],
        },
      }],
    });

    expect(echo.textContent).toBe("from-component-root");
  });

  it("walks array item object paths from a component root", () => {
    const widget = mountWidget("resident-grid");
    widget.setItems([
      { meta: { name: "Amina", code: "ALB" } },
      { meta: { name: "Basil", code: "BOS" } },
    ]);

    const echo = document.createElement("div");
    echo.id = "resident-grid-echo";
    document.body.appendChild(echo);

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "sequential",
          commands: [{
            kind: "mutate-element",
            target: "resident-grid-echo",
            mutation: {
              kind: "set-prop",
              prop: "textContent",
              value: {
                kind: "source",
                source: {
                  kind: "component",
                  componentId: "resident-grid",
                  vendor: "fusion",
                  readExpr: "items.1.meta.name",
                },
              },
            },
          }],
        },
      }],
    });

    expect(echo.textContent).toBe("Basil");
  });

  it("calls a method with args on a native component root", () => {
    const panel = document.createElement("div");
    panel.id = "panel";
    document.body.appendChild(panel);

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "sequential",
          commands: [{
            kind: "mutate-element",
            target: "panel",
            mutation: {
              kind: "call",
              method: "setAttribute",
              args: [
                { kind: "literal", value: "data-status" },
                { kind: "literal", value: "active" },
              ],
            },
          }],
        },
      }],
    });

    expect(panel.getAttribute("data-status")).toBe("active");
  });

  it("calls a method with args on a fusion component root", () => {
    const widget = mountWidget("items-widget");

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "sequential",
          commands: [{
            kind: "mutate-element",
            target: "items-widget",
            vendor: "fusion",
            mutation: {
              kind: "call",
              method: "setItems",
              args: [{ kind: "literal", value: ["x", "y", "z"] }],
            },
          }],
        },
      }],
    });

    expect(widget.items).toEqual(["x", "y", "z"]);
  });

  it("calls a method without args on a fusion component root", () => {
    const widget = mountWidget("focus-widget");

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "sequential",
          commands: [{
            kind: "mutate-element",
            target: "focus-widget",
            vendor: "fusion",
            mutation: {
              kind: "call",
              method: "focus",
            },
          }],
        },
      }],
    });

    expect(widget.focused).toBe(true);
  });
});
