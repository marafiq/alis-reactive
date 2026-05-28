import { describe, expect, it, vi } from "vitest";
import { registerComponentRuntime, type ComponentRuntimeDriver } from "../domain/component-runtime";
import { executeReaction } from "../execution/execute";
import type {
  ComponentObject, ConditionGraph, ExecContext, BrowserObjectContract, JsonValue, MemberAccess, PlanDocument, ReactionGraph, RequestPlan, Shape, ValueExpression,
} from "../types";

const stringShape: Shape = { kind: "string" };
const noneShape: Shape = { kind: "none" };
const booleanShape: Shape = { kind: "boolean" };
const rawShape: Shape = { kind: "raw" };
const customWidgetVendor = "acme-widget";

interface CustomWidgetRoot {
  value: string;
  setValue(next: string): void;
}

interface CustomWidgetHost extends HTMLElement {
  reactiveWidget?: CustomWidgetRoot;
}

type BrowserWindowWithConfirm = typeof window & {
  alis?: { confirm?: (message: string) => boolean | Promise<boolean> };
};

const customWidgetRuntime: ComponentRuntimeDriver = {
  resolveRoot: element => {
    const root = (element as CustomWidgetHost).reactiveWidget;
    if (root !== undefined) return root;

    throw new Error(`[test] custom widget root missing for ${element.id}`);
  },
  wireEvent: () => undefined,
};

registerComponentRuntime(customWidgetVendor, customWidgetRuntime);

function literal(value: string): ValueExpression {
  return { kind: "literal", value, shape: stringShape };
}

function conditionResult(value: boolean): ConditionGraph {
  return {
    kind: "compare",
    left: shapedLiteral(value, booleanShape),
    op: "eq",
    right: { kind: "value", value: shapedLiteral(true, booleanShape) },
    shape: booleanShape,
    itemShape: noneShape,
  };
}

function shapedLiteral(value: JsonValue, shape: Shape): ValueExpression {
  return { kind: "literal", value, shape };
}

function setResidentName(value: string): ReactionGraph {
  return {
    kind: "set",
    on: { kind: "component", component: "resident-name" },
    property: "value",
    value: literal(value),
  };
}

function markLocal(value: string): ReactionGraph {
  return {
    kind: "call",
    on: { kind: "payload", scope: "local", type: { kind: "untyped" } },
    method: "mark",
    args: [literal(value)],
  };
}

function requestWithSuccess(reaction: ReactionGraph): ReactionGraph {
  const request: RequestPlan = {
    method: "GET",
    url: "/residents/42",
    validation: { kind: "none" },
    input: { kind: "none" },
    whileLoading: [],
    success: [{ match: { kind: "any" }, reaction }],
    error: [],
    finally: [],
    chain: { kind: "terminal" },
  };

  return { kind: "request", request };
}

function responseJson(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { "Content-Type": "application/json" },
  });
}

function arrayLiteral(value: JsonValue[], item: Shape): ValueExpression {
  return {
    kind: "literal",
    value,
    shape: { kind: "array", item },
  };
}

function textBoxPlan(): PlanDocument {
  return textBoxPlanWithValueAccess("readwrite");
}

function textBoxPlanWithValueAccess(access: MemberAccess): PlanDocument {
  const type: BrowserObjectContract = {
    properties: {
      value: {
        path: [{ kind: "property", name: "value" }],
        shape: stringShape,
        access,
      },
    },
    methods: {
      setValue: {
        path: [{ kind: "property", name: "setValue" }],
        arguments: { kind: "exact", shapes: [stringShape] },
        returns: noneShape,
      },
    },
    events: {},
  };
  const component: ComponentObject = {
    id: "resident-name",
    vendor: "native",
    type: "native.textbox",
    role: { kind: "object-target" },
    binding: { kind: "none" },
    container: { kind: "none" },
  };

  return {
    version: 3,
    planId: "Runtime.ReactionExecution",
    scope: { kind: "root" },
    types: { "native.textbox": type },
    components: { "resident-name": component },
    behaviors: [],
  };
}

function customWidgetPlan(): PlanDocument {
  const type: BrowserObjectContract = {
    properties: {
      value: {
        path: [{ kind: "property", name: "value" }],
        shape: stringShape,
        access: "readwrite",
      },
    },
    methods: {
      setValue: {
        path: [{ kind: "property", name: "setValue" }],
        arguments: { kind: "exact", shapes: [stringShape] },
        returns: noneShape,
      },
    },
    events: {},
  };

  return {
    version: 3,
    planId: "Runtime.CustomVendorExecution",
    scope: { kind: "root" },
    types: { "acme.widget": type },
    components: {
      "resident-widget": {
        id: "resident-widget",
        vendor: customWidgetVendor,
        type: "acme.widget",
        role: { kind: "object-target" },
        binding: { kind: "none" },
        container: { kind: "none" },
      },
    },
    behaviors: [],
  };
}

describe("executeReaction member targets", () => {
  it("sets a component property through the declared JS object contract", () => {
    document.body.innerHTML = `<input id="resident-name" value="Ada" />`;
    const reaction: ReactionGraph = {
      kind: "set",
      on: { kind: "component", component: "resident-name" },
      property: "value",
      value: literal("Grace"),
    };

    executeReaction(reaction, textBoxPlan());

    const element = document.getElementById("resident-name") as HTMLInputElement;
    expect(element.value).toBe("Grace");
  });

  it("uses registered vendor runtimes without changing reaction execution", () => {
    document.body.innerHTML = `<div id="resident-widget"></div>`;
    const host = document.getElementById("resident-widget") as CustomWidgetHost;
    host.reactiveWidget = {
      value: "Ada",
      setValue(next: string): void {
        this.value = next;
      },
    };
    const plan = customWidgetPlan();

    executeReaction({
      kind: "set",
      on: { kind: "component", component: "resident-widget" },
      property: "value",
      value: literal("Grace"),
    }, plan);

    executeReaction({
      kind: "call",
      on: { kind: "component", component: "resident-widget" },
      method: "setValue",
      args: [literal("Katherine")],
    }, plan);

    expect(host.reactiveWidget.value).toBe("Katherine");
  });

  it("fails with component context when a vendor runtime is not registered", () => {
    document.body.innerHTML = `<div id="resident-widget"></div>`;
    const plan = customWidgetPlan();
    plan.components["resident-widget"].vendor = "missing-widget-vendor";

    expect(() => executeReaction({
      kind: "set",
      on: { kind: "component", component: "resident-widget" },
      property: "value",
      value: literal("Grace"),
    }, plan)).toThrow('component runtime not registered for component "resident-widget"');
  });

  it("prepares property writes through the declared target shape", () => {
    document.body.innerHTML = `<input id="active-status" type="checkbox" />`;
    const type: BrowserObjectContract = {
      properties: {
        checked: {
          path: [{ kind: "property", name: "checked" }],
          shape: booleanShape,
          access: "readwrite",
        },
      },
      methods: {},
      events: {},
    };
    const plan: PlanDocument = {
      version: 3,
      planId: "Runtime.PropertyWriteShape",
      scope: { kind: "root" },
      types: { "native.checkbox": type },
      components: {
        "active-status": {
          id: "active-status",
          vendor: "native",
          type: "native.checkbox",
          role: { kind: "object-target" },
          binding: { kind: "none" },
          container: { kind: "none" },
        },
      },
      behaviors: [],
    };

    executeReaction({
      kind: "set",
      on: { kind: "component", component: "active-status" },
      property: "checked",
      value: shapedLiteral("true", rawShape),
    }, plan);

    const element = document.getElementById("active-status") as HTMLInputElement;
    expect(element.checked).toBe(true);
  });

  it("writes array values to text properties using browser string semantics", () => {
    document.body.innerHTML = `<span id="allergy-echo"></span>`;
    const type: BrowserObjectContract = {
      properties: {
        text: {
          path: [{ kind: "property", name: "textContent" }],
          shape: stringShape,
          access: "write",
        },
      },
      methods: {},
      events: {},
    };
    const plan: PlanDocument = {
      version: 3,
      planId: "Runtime.TextPropertyShape",
      scope: { kind: "root" },
      types: { "native.element.allergy-echo": type },
      components: {
        "allergy-echo": {
          id: "allergy-echo",
          vendor: "native",
          type: "native.element.allergy-echo",
          role: { kind: "object-target" },
          binding: { kind: "none" },
          container: { kind: "none" },
        },
      },
      behaviors: [],
    };

    executeReaction({
      kind: "set",
      on: { kind: "component", component: "allergy-echo" },
      property: "text",
      value: arrayLiteral(["Peanuts", "Shellfish", "Dairy"], stringShape),
    }, plan);

    expect(document.getElementById("allergy-echo")?.textContent).toBe("Peanuts,Shellfish,Dairy");
  });

  it("calls a component method with evaluated arguments", () => {
    document.body.innerHTML = `<input id="resident-name" value="Ada" />`;
    const element = document.getElementById("resident-name") as HTMLInputElement & {
      setValue(next: string): void;
    };
    element.setValue = function setValue(next: string): void {
      this.value = next;
    };
    const reaction: ReactionGraph = {
      kind: "call",
      on: { kind: "component", component: "resident-name" },
      method: "setValue",
      args: [literal("Katherine")],
    };

    executeReaction(reaction, textBoxPlan());

    expect(element.value).toBe("Katherine");
  });

  it("sets and calls payload members inside the active execution context", () => {
    const local = {
      status: "pending",
      setStatus(next: string): void {
        this.status = next;
      },
    };
    const context: ExecContext = { local };
    const source = { kind: "payload", scope: "local", type: { kind: "untyped" } } as const;
    const setReaction: ReactionGraph = {
      kind: "set",
      on: source,
      property: "status",
      value: literal("ready"),
    };
    const callReaction: ReactionGraph = {
      kind: "call",
      on: source,
      method: "setStatus",
      args: [literal("done")],
    };

    executeReaction(setReaction, textBoxPlan(), context);
    expect(local.status).toBe("ready");

    executeReaction(callReaction, textBoxPlan(), context);
    expect(local.status).toBe("done");
  });

  it("keeps branch execution synchronous when an earlier guard matches before a confirm guard", () => {
    document.body.innerHTML = `<input id="resident-name" value="Ada" />`;
    const browserWindow = window as BrowserWindowWithConfirm;
    const confirm = vi.fn(() => true);
    browserWindow.alis = { confirm };

    try {
      const reaction: ReactionGraph = {
        kind: "branch",
        cases: [
          {
            guard: { kind: "when", condition: conditionResult(true) },
            reaction: setResidentName("matched"),
          },
          {
            guard: { kind: "when", condition: { kind: "confirm", message: "Continue?" } },
            reaction: setResidentName("confirmed"),
          },
        ],
      };

      const completion = executeReaction(reaction, textBoxPlan());

      expect(completion).toBeUndefined();
      expect(confirm).not.toHaveBeenCalled();
      expect((document.getElementById("resident-name") as HTMLInputElement).value).toBe("matched");
    } finally {
      delete browserWindow.alis;
    }
  });

  it("keeps any guards synchronous when an earlier term decides the condition before confirm", () => {
    document.body.innerHTML = `<input id="resident-name" value="Ada" />`;
    const browserWindow = window as BrowserWindowWithConfirm;
    const confirm = vi.fn(() => true);
    browserWindow.alis = { confirm };

    try {
      const reaction: ReactionGraph = {
        kind: "branch",
        cases: [{
          guard: {
            kind: "when",
            condition: {
              kind: "any",
              terms: [
                conditionResult(true),
                { kind: "confirm", message: "Continue?" },
              ],
            },
          },
          reaction: setResidentName("matched"),
        }],
      };

      const completion = executeReaction(reaction, textBoxPlan());

      expect(completion).toBeUndefined();
      expect(confirm).not.toHaveBeenCalled();
      expect((document.getElementById("resident-name") as HTMLInputElement).value).toBe("matched");
    } finally {
      delete browserWindow.alis;
    }
  });

  it("keeps all guards synchronous when an earlier term rejects the condition before confirm", () => {
    document.body.innerHTML = `<input id="resident-name" value="Ada" />`;
    const browserWindow = window as BrowserWindowWithConfirm;
    const confirm = vi.fn(() => true);
    browserWindow.alis = { confirm };

    try {
      const reaction: ReactionGraph = {
        kind: "branch",
        cases: [
          {
            guard: {
              kind: "when",
              condition: {
                kind: "all",
                terms: [
                  conditionResult(false),
                  { kind: "confirm", message: "Continue?" },
                ],
              },
            },
            reaction: setResidentName("matched"),
          },
          {
            guard: { kind: "default" },
            reaction: setResidentName("default"),
          },
        ],
      };

      const completion = executeReaction(reaction, textBoxPlan());

      expect(completion).toBeUndefined();
      expect(confirm).not.toHaveBeenCalled();
      expect((document.getElementById("resident-name") as HTMLInputElement).value).toBe("default");
    } finally {
      delete browserWindow.alis;
    }
  });

  it("crosses the async boundary only when a reached guard term requires confirm", async () => {
    document.body.innerHTML = `<input id="resident-name" value="Ada" />`;
    const browserWindow = window as BrowserWindowWithConfirm;
    const confirm = vi.fn(async () => true);
    browserWindow.alis = { confirm };

    try {
      const reaction: ReactionGraph = {
        kind: "branch",
        cases: [{
          guard: {
            kind: "when",
            condition: {
              kind: "all",
              terms: [
                conditionResult(true),
                { kind: "confirm", message: "Continue?" },
              ],
            },
          },
          reaction: setResidentName("confirmed"),
        }],
      };

      const completion = executeReaction(reaction, textBoxPlan());

      expect(completion).toBeInstanceOf(Promise);
      await completion;
      expect(confirm).toHaveBeenCalledOnce();
      expect((document.getElementById("resident-name") as HTMLInputElement).value).toBe("confirmed");
    } finally {
      delete browserWindow.alis;
    }
  });

  it("keeps sequence steps synchronous until an authored request is reached", async () => {
    let releaseResponse!: () => void;
    const response = new Promise<Response>(resolve => {
      releaseResponse = () => resolve(responseJson({ saved: true }));
    });
    const fetchMock = vi.fn(() => response);
    vi.stubGlobal("fetch", fetchMock);
    const marks: string[] = [];
    const context: ExecContext = {
      local: {
        mark(value: string): void {
          marks.push(value);
        },
      },
    };
    const reaction: ReactionGraph = {
      kind: "sequence",
      steps: [
        markLocal("start"),
        {
          kind: "branch",
          cases: [
            {
              guard: { kind: "when", condition: conditionResult(true) },
              reaction: markLocal("branch"),
            },
            {
              guard: { kind: "default" },
              reaction: markLocal("default"),
            },
          ],
        },
        requestWithSuccess(markLocal("success")),
        markLocal("after"),
      ],
    };

    try {
      const completion = executeReaction(reaction, textBoxPlan(), context);

      expect(completion).toBeInstanceOf(Promise);
      expect(marks).toEqual(["start", "branch"]);

      await Promise.resolve();
      expect(fetchMock).toHaveBeenCalledOnce();
      expect(marks).toEqual(["start", "branch"]);

      releaseResponse();
      await completion;

      expect(marks).toEqual(["start", "branch", "success", "after"]);
    } finally {
      vi.unstubAllGlobals();
    }
  });

  it("dispatches an empty detail for explicit no-payload events", () => {
    let detail: unknown;
    document.addEventListener("resident:saved", event => {
      detail = (event as CustomEvent).detail;
    }, { once: true });

    executeReaction({
      kind: "dispatch",
      event: "resident:saved",
      payload: { kind: "none" },
    }, textBoxPlan());

    expect(detail).toEqual({});
  });

  it("dispatches evaluated value payloads", () => {
    let detail: unknown;
    document.addEventListener("resident:selected", event => {
      detail = (event as CustomEvent).detail;
    }, { once: true });

    executeReaction({
      kind: "dispatch",
      event: "resident:selected",
      payload: {
        kind: "value",
        data: literal("Ada"),
        payloadType: { kind: "untyped" },
      },
    }, textBoxPlan());

    expect(detail).toBe("Ada");
  });
});
