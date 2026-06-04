import { describe, expect, it } from "vitest";
import { evaluateValue } from "../../values/evaluate";
import { registerPlugin } from "../../plugins/catalog";
import { RuntimeResolutionError } from "../../browser-objects/runtime-plan";
import type { ComponentObject, BrowserObjectContract, PlanDocument, Shape, ValueExpression } from "../../types/index";

const stringShape: Shape = { kind: "string" };
const numberShape: Shape = { kind: "number" };
const objectShape: Shape = { kind: "object", fields: {}, additional: true };
const residentObjectShape: Shape = {
  kind: "object",
  additional: false,
  fields: {
    age: numberShape,
    address: {
      kind: "object",
      additional: false,
      fields: {
        zipCode: numberShape,
      },
    },
  },
};
const stringArrayShape: Shape = { kind: "array", item: stringShape };
const numberArrayShape: Shape = { kind: "array", item: numberShape };
const rawShape: Shape = { kind: "raw" };

function valueEvaluationPlan(entries: {
  readonly types?: Record<string, BrowserObjectContract>;
  readonly components?: Record<string, ComponentObject>;
} = {}): PlanDocument {
  return {
    version: 3,
    planId: "Runtime.ValueEvaluation",
    scope: { kind: "root" },
    types: entries.types ?? {},
    components: entries.components ?? {},
    behaviors: [],
  };
}

function literal(value: string, shape: Shape = stringShape): ValueExpression {
  return { kind: "literal", value, shape };
}

describe("evaluateValue", () => {
  it("evaluates object and array producers through one evaluation context", () => {
    const producer: ValueExpression = {
      kind: "object",
      shape: objectShape,
      fields: {
        age: literal("42", numberShape),
        tags: {
          kind: "array",
          shape: stringArrayShape,
          items: [literal("alpha"), literal("beta")],
        },
      },
    };

    expect(evaluateValue(producer, valueEvaluationPlan())).toEqual({
      age: 42,
      tags: ["alpha", "beta"],
    });
  });

  it("prepares composite value expressions through their declared output shape", () => {
    const producer: ValueExpression = {
      kind: "array",
      shape: numberArrayShape,
      items: [
        literal("41", rawShape),
        literal("42", rawShape),
      ],
    };

    expect(evaluateValue(producer, valueEvaluationPlan())).toEqual([41, 42]);
  });

  it("applies declared field shapes to object value expressions", () => {
    const producer: ValueExpression = {
      kind: "object",
      shape: residentObjectShape,
      fields: {
        age: literal("42", rawShape),
        address: {
          kind: "literal",
          value: {
            zipCode: "90210",
            ignored: "not declared",
          },
          shape: rawShape,
        },
        ignored: literal("not declared", rawShape),
      },
    };

    expect(evaluateValue(producer, valueEvaluationPlan())).toEqual({
      age: 42,
      address: {
        zipCode: 90210,
      },
    });
  });

  it("reads payload values by structured path and applies the requested shape", () => {
    const producer: ValueExpression = {
      kind: "read",
      from: { kind: "payload", scope: "event", type: { kind: "untyped" } },
      member: "ignored-when-path-is-structured",
      path: [
        { kind: "property", name: "address" },
        { kind: "property", name: "zipCode" },
      ],
      shape: numberShape,
      access: { kind: "property" },
    };

    const value = evaluateValue(producer, valueEvaluationPlan(), {
      event: { address: { zipCode: "90210" } },
    });

    expect(value).toBe(90210);
  });

  it("allows absent payload paths to evaluate as missing values", () => {
    const producer: ValueExpression = {
      kind: "read",
      from: { kind: "payload", scope: "event", type: { kind: "untyped" } },
      member: "address.zipCode",
      path: [
        { kind: "property", name: "address" },
        { kind: "property", name: "zipCode" },
      ],
      shape: numberShape,
      access: { kind: "property" },
    };

    expect(evaluateValue(producer, valueEvaluationPlan(), { event: {} })).toBeUndefined();
  });

  it("does not coerce malformed numeric text to zero", () => {
    const producer = literal("not-a-number", numberShape);

    expect(evaluateValue(producer, valueEvaluationPlan())).toBe("not-a-number");
  });

  it("does not coerce malformed date text to NaN", () => {
    const producer = literal("not-a-date", { kind: "date" });

    expect(evaluateValue(producer, valueEvaluationPlan())).toBe("not-a-date");
  });

  it("does not normalize impossible date-only text", () => {
    const producer = literal("2026-99-99", { kind: "date" });

    expect(evaluateValue(producer, valueEvaluationPlan())).toBe("2026-99-99");
  });

  it("reads the whole payload through the explicit responseBody member", () => {
    const producer: ValueExpression = {
      kind: "read",
      from: { kind: "payload", scope: "success", type: { kind: "untyped" } },
      member: "responseBody",
      path: [],
      shape: objectShape,
      access: { kind: "property" },
    };
    const payload = { data: { name: "Ada" } };

    expect(evaluateValue(producer, valueEvaluationPlan(), { response: payload })).toBe(payload);
  });

  it("reads component properties and calls component methods from the declared JS object contract", () => {
    document.body.innerHTML = `<input id="resident-name" value="Ada" />`;
    const element = document.getElementById("resident-name") as HTMLInputElement & {
      appendSuffix(suffix: string): string;
    };
    element.appendSuffix = function appendSuffix(suffix: string): string {
      return this.value + suffix;
    };

    const objectContract: BrowserObjectContract = {
      properties: {
        value: {
          path: [{ kind: "property", name: "value" }],
          shape: stringShape,
          access: "read",
        },
      },
      methods: {
        appendSuffix: {
          path: [{ kind: "property", name: "appendSuffix" }],
          arguments: { kind: "exact", shapes: [stringShape] },
          returns: stringShape,
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
    const runtimePlan = valueEvaluationPlan({
      types: { "native.textbox": objectContract },
      components: { "resident-name": component },
    });

    const readValue: ValueExpression = {
      kind: "read",
      from: { kind: "component", component: "resident-name" },
      member: "value",
      path: [{ kind: "property", name: "value" }],
      shape: stringShape,
      access: { kind: "property" },
    };
    const callMethod: ValueExpression = {
      kind: "read",
      from: { kind: "component", component: "resident-name" },
      member: "appendSuffix",
      path: [{ kind: "property", name: "appendSuffix" }],
      shape: stringShape,
      access: {
        kind: "method",
        args: [literal(" Lovelace")],
      },
    };

    expect(evaluateValue(readValue, runtimePlan)).toBe("Ada");
    expect(evaluateValue(callMethod, runtimePlan)).toBe("Ada Lovelace");
  });

  it("rejects declared runtime object property paths that are not present on the JS object", () => {
    document.body.innerHTML = `<div id="resident-name"></div>`;
    const objectContract: BrowserObjectContract = {
      properties: {
        value: {
          path: [{ kind: "property", name: "value" }],
          shape: stringShape,
          access: "read",
        },
      },
      methods: {},
      events: {},
    };
    const producer: ValueExpression = {
      kind: "read",
      from: { kind: "component", component: "resident-name" },
      member: "value",
      path: [{ kind: "property", name: "value" }],
      shape: stringShape,
      access: { kind: "property" },
    };

    expect(() => evaluateValue(producer, valueEvaluationPlan({
      types: { "native.textbox": objectContract },
      components: {
        "resident-name": {
          id: "resident-name",
          vendor: "native",
          type: "native.textbox",
          role: { kind: "object-target" },
          binding: { kind: "none" },
          container: { kind: "none" },
        },
      },
    }))).toThrow('runtime path member "value" is missing on component "resident-name".value');
  });

  it("throws typed runtime resolution errors for missing component elements", () => {
    const objectContract: BrowserObjectContract = {
      properties: {
        value: {
          path: [{ kind: "property", name: "value" }],
          shape: stringShape,
          access: "read",
        },
      },
      methods: {},
      events: {},
    };
    const producer: ValueExpression = {
      kind: "read",
      from: { kind: "component", component: "resident-name" },
      member: "value",
      path: [{ kind: "property", name: "value" }],
      shape: stringShape,
      access: { kind: "property" },
    };

    document.body.innerHTML = "";

    try {
      evaluateValue(producer, valueEvaluationPlan({
        types: { "native.textbox": objectContract },
        components: {
          "resident-name": {
            id: "resident-name",
            vendor: "native",
            type: "native.textbox",
            role: { kind: "object-target" },
            binding: { kind: "none" },
            container: { kind: "none" },
          },
        },
      }));
      throw new Error("expected missing component element to fail");
    } catch (error) {
      expect(error).toBeInstanceOf(RuntimeResolutionError);
      if (error instanceof RuntimeResolutionError) {
        expect(error.target).toEqual({ kind: "element", id: "resident-name" });
      }
    }
  });

  it("calls root function plugins through the declared $call contract", () => {
    const pluginName = "slugifyRootRuntime";
    registerPlugin(pluginName, (value: string): string =>
      value.toLowerCase().replace(/\s+/g, "-"));

    const pluginType: BrowserObjectContract = {
      properties: {},
      methods: {
        $call: {
          path: [],
          arguments: { kind: "exact", shapes: [stringShape] },
          returns: stringShape,
        },
      },
      events: {},
    };
    const producer: ValueExpression = {
      kind: "read",
      from: { kind: "plugin", name: pluginName, type: "plugin." + pluginName },
      member: "$call",
      path: [],
      shape: stringShape,
      access: {
        kind: "method",
        args: [literal("John Doe")],
      },
    };

    expect(evaluateValue(producer, valueEvaluationPlan({
      types: { ["plugin." + pluginName]: pluginType },
    }))).toBe("john-doe");
  });

  it("reads plugin object properties through the declared property contract", () => {
    const pluginName = "authRuntimeProperty";
    registerPlugin(pluginName, { token: "abc-123" });

    const pluginType: BrowserObjectContract = {
      properties: {
        token: {
          path: [{ kind: "property", name: "token" }],
          shape: stringShape,
          access: "read",
        },
      },
      methods: {},
      events: {},
    };
    const producer: ValueExpression = {
      kind: "read",
      from: { kind: "plugin", name: pluginName, type: "plugin." + pluginName },
      member: "token",
      path: [],
      shape: stringShape,
      access: { kind: "property" },
    };

    expect(evaluateValue(producer, valueEvaluationPlan({
      types: { ["plugin." + pluginName]: pluginType },
    }))).toBe("abc-123");
  });

  it("prepares exact runtime method arguments through their declared shapes", () => {
    document.body.innerHTML = `<button id="save"></button>`;
    const element = document.getElementById("save") as HTMLButtonElement & {
      describeAge(age: number): string;
    };
    element.describeAge = function describeAge(age: number): string {
      return `${typeof age}:${age}`;
    };

    const objectContract: BrowserObjectContract = {
      properties: {},
      methods: {
        describeAge: {
          path: [{ kind: "property", name: "describeAge" }],
          arguments: { kind: "exact", shapes: [numberShape] },
          returns: stringShape,
        },
      },
      events: {},
    };

    const producer: ValueExpression = {
      kind: "read",
      from: { kind: "component", component: "save" },
      member: "describeAge",
      path: [{ kind: "property", name: "describeAge" }],
      shape: stringShape,
      access: {
        kind: "method",
        args: [literal("42", rawShape)],
      },
    };

    expect(evaluateValue(producer, valueEvaluationPlan({
      types: { "native.button": objectContract },
      components: {
        save: {
          id: "save",
          vendor: "native",
          type: "native.button",
          role: { kind: "object-target" },
          binding: { kind: "none" },
          container: { kind: "none" },
        },
      },
    }))).toBe("number:42");
  });

});
