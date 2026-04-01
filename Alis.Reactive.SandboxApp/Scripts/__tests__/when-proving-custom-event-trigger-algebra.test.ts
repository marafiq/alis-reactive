import { afterEach, describe, expect, it } from "vitest";
import { boot } from "../lifecycle/boot";

describe("when proving custom-event trigger algebra", () => {
  function flushMicrotasks() {
    return new Promise(resolve => setTimeout(resolve, 0));
  }

  afterEach(() => {
    document.body.innerHTML = "";
  });

  it("sets a property on the trigger payload from a literal", () => {
    const detail: Record<string, unknown> = {};

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "custom-event", event: "trigger-literal" },
        reaction: {
          kind: "sequential",
          commands: [{
            kind: "mutate-event",
            mutation: {
              kind: "set-prop",
              prop: "status",
              value: { kind: "literal", value: "ok" },
            },
          }],
        },
      }],
    });

    document.dispatchEvent(new CustomEvent("trigger-literal", { detail }));
    expect(detail.status).toBe("ok");
  });

  it("sets a property on the trigger payload from a component value", () => {
    const input = document.createElement("input");
    input.id = "src-input";
    input.value = "from-component";
    document.body.appendChild(input);

    const detail: Record<string, unknown> = {};

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "custom-event", event: "trigger-component-source" },
        reaction: {
          kind: "sequential",
          commands: [{
            kind: "mutate-event",
            mutation: {
              kind: "set-prop",
              prop: "copied",
              value: {
                kind: "source",
                source: { kind: "component", componentId: "src-input", vendor: "native", readExpr: "value" },
              },
            },
          }],
        },
      }],
    });

    document.dispatchEvent(new CustomEvent("trigger-component-source", { detail }));
    expect(detail.copied).toBe("from-component");
  });

  it("reads a property from the trigger payload and consumes it elsewhere", () => {
    const echo = document.createElement("div");
    echo.id = "echo";
    document.body.appendChild(echo);

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "custom-event", event: "trigger-read" },
        reaction: {
          kind: "sequential",
          commands: [{
            kind: "mutate-element",
            target: "echo",
            mutation: {
              kind: "set-prop",
              prop: "textContent",
              value: {
                kind: "source",
                source: { kind: "event", path: "evt.payload.name" },
              },
            },
          }],
        },
      }],
    });

    document.dispatchEvent(new CustomEvent("trigger-read", {
      detail: { payload: { name: "resident-a" } },
    }));

    expect(echo.textContent).toBe("resident-a");
  });

  it("walks array item object paths from the trigger payload", () => {
    const echo = document.createElement("div");
    echo.id = "array-echo";
    document.body.appendChild(echo);

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "custom-event", event: "trigger-array-read" },
        reaction: {
          kind: "sequential",
          commands: [{
            kind: "mutate-element",
            target: "array-echo",
            mutation: {
              kind: "set-prop",
              prop: "textContent",
              value: {
                kind: "source",
                source: { kind: "event", path: "evt.residents.1.meta.name" },
              },
            },
          }],
        },
      }],
    });

    document.dispatchEvent(new CustomEvent("trigger-array-read", {
      detail: {
        residents: [
          { meta: { name: "Amina" } },
          { meta: { name: "Basil" } },
        ],
      },
    }));

    expect(echo.textContent).toBe("Basil");
  });

  it("calls a method on the trigger payload with args", () => {
    const calls: unknown[][] = [];
    const detail = {
      record(...args: unknown[]) {
        calls.push(args);
      },
    };

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "custom-event", event: "trigger-call-with-args" },
        reaction: {
          kind: "sequential",
          commands: [{
            kind: "mutate-event",
            mutation: {
              kind: "call",
              method: "record",
              args: [
                { kind: "source", source: { kind: "event", path: "evt.meta.label" } },
                { kind: "source", source: { kind: "event", path: "evt.meta.count" }, coerce: "number" },
              ],
            },
          }],
        },
      }],
    });

    document.dispatchEvent(new CustomEvent("trigger-call-with-args", {
      detail: Object.assign(detail, { meta: { label: "active", count: "2" } }),
    }));

    expect(calls).toEqual([["active", 2]]);
  });

  it("calls a method on the trigger payload with no args", () => {
    const detail = {
      touched: false,
      touch() {
        this.touched = true;
      },
    };

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "custom-event", event: "trigger-call-no-args" },
        reaction: {
          kind: "sequential",
          commands: [{
            kind: "mutate-event",
            mutation: {
              kind: "call",
              method: "touch",
            },
          }],
        },
      }],
    });

    document.dispatchEvent(new CustomEvent("trigger-call-no-args", { detail }));
    expect(detail.touched).toBe(true);
  });

  it("supports source-vs-source conditions on a custom-event trigger", async () => {
    const input = document.createElement("input");
    input.id = "approval-input";
    input.value = "approved";
    document.body.appendChild(input);

    const echo = document.createElement("div");
    echo.id = "approval-echo";
    document.body.appendChild(echo);

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "custom-event", event: "approval-check" },
        reaction: {
          kind: "conditional",
          branches: [
            {
              guard: {
                kind: "value",
                source: { kind: "event", path: "evt.status" },
                coerceAs: "string",
                op: "eq",
                rightSource: { kind: "component", componentId: "approval-input", vendor: "native", readExpr: "value" },
              },
              reaction: {
                kind: "sequential",
                commands: [{
                  kind: "mutate-element",
                  target: "approval-echo",
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
                  target: "approval-echo",
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
    });

    document.dispatchEvent(new CustomEvent("approval-check", { detail: { status: "approved" } }));
    await flushMicrotasks();
    expect(echo.textContent).toBe("matched");
  });
});
